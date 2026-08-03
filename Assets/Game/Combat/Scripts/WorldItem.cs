using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// WorldItem — Dropped item in the world. Floats, rotates, glows by rarity.
///
/// Pickup flow:
///   Player enters trigger → CmdPickup (server) → RpcOnPickedUp (local client only)
///   → InventoryManager.OnItemPickedUp → POST /api/inventory/save
///
/// itemId "gold:N" awards gold instead of an inventory item.
/// Glow light auto-created in Start() — no Inspector assignment needed.
/// </summary>
public class WorldItem : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnItemIdChanged))]
    public string itemId = "";

    [SyncVar]
    public int quantity = 1;

    [Header("Float / Spin")]
    public float floatSpeed     = 1.3f;
    public float floatAmplitude = 0.18f;
    public float rotateSpeed    = 55f;

    // Glow light — auto-created in Start(), no Inspector assignment required.
    private Light _glowLight;

    private Vector3 _origin;
    private bool    _pickedUp = false;
    private bool    _pickupRequested = false;

    static readonly Color ColorCommon   = new Color(0.75f, 0.75f, 0.75f);
    static readonly Color ColorUncommon = new Color(0.2f,  0.9f,  0.2f);
    static readonly Color ColorRare     = new Color(0.2f,  0.5f,  1f);
    static readonly Color ColorEpic     = new Color(0.7f,  0.1f,  1f);
    static readonly Color ColorGold     = new Color(1f,    0.8f,  0.1f);

    void Start()
    {
        _origin = transform.position;

        // Auto-create glow light if not already present in hierarchy
        _glowLight = GetComponentInChildren<Light>(includeInactive: false);
        if (_glowLight == null)
        {
            var lg = new GameObject("GlowLight");
            lg.transform.SetParent(transform, false);
            lg.transform.localPosition = Vector3.zero;
            _glowLight           = lg.AddComponent<Light>();
            _glowLight.type      = LightType.Point;
            _glowLight.range     = 1.8f;
            _glowLight.intensity = 0.8f;
            _glowLight.shadows   = LightShadows.None;
        }

        // Auto-add trigger collider for pickup if missing
        if (GetComponent<Collider>() == null)
        {
            var sc      = gameObject.AddComponent<SphereCollider>();
            sc.radius    = 0.6f;
            sc.isTrigger = true;
        }

        ApplyRarityGlow(itemId);
    }

#if UNITY_EDITOR || !UNITY_SERVER
    void Update()
    {
        float y = _origin.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
#endif

    void OnTriggerEnter(Collider other)
    {
        if (_pickedUp || _pickupRequested || !other.CompareTag("Player")) return;
        var netId = other.GetComponent<NetworkIdentity>();
        if (netId == null || !netId.isLocalPlayer) return;

        _pickupRequested = true;
        CmdPickup();
    }

    [Command(requiresAuthority = false)]
    void CmdPickup(NetworkConnectionToClient sender = null)
    {
        if (_pickedUp || sender == null || sender.identity == null)
            return;

        NetworkIdentity picker = sender.identity;
        if (picker.connectionToClient != sender)
            return;

        // This command intentionally does not require authority because the
        // pickup belongs to the world. Validate proximity on the server so a
        // client cannot collect arbitrary loot elsewhere in the zone.
        const float maximumPickupDistance = 3f;
        Vector3 offset = picker.transform.position - transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude > maximumPickupDistance * maximumPickupDistance)
        {
            Debug.LogWarning(
                $"[LOOT] Rejected distant pickup from netId={picker.netId} " +
                $"for '{itemId}' ({offset.magnitude:F2}m away).",
                this);
            return;
        }

        _pickedUp = true;
        if (!itemId.StartsWith("gold:"))
            QuestLocalRuntime.ServerReport(sender,
                QuestObjectiveType.CollectItem, itemId, Mathf.Max(1, quantity));

        TargetOnPickedUp(sender, itemId, quantity);
        StartCoroutine(DestroyAfterRewardDispatch());
    }

    [TargetRpc]
    void TargetOnPickedUp(
        NetworkConnectionToClient target,
        string pickedItemId,
        int qty)
    {

        // Gold pickup — award directly to progress, don't add to inventory
        if (pickedItemId.StartsWith("gold:"))
        {
            if (int.TryParse(pickedItemId.Substring(5), out int goldAmt))
            {
                var progress = PlayerProgressManager.Local;
                if (progress == null)
                {
                    Debug.LogError(
                        $"[LOOT] PlayerProgressManager unavailable — " +
                        $"could not award {goldAmt} gold.");
                    return;
                }

                progress.AwardGold(goldAmt);
                Debug.Log($"[LOOT] Picked up {goldAmt} gold");
            }
            return;
        }

#if UNITY_EDITOR || !UNITY_SERVER
        var inv = InventoryManager.Instance;
        if (inv != null)
            inv.OnItemPickedUp(pickedItemId, qty);
        else
            Debug.LogWarning($"[LOOT] InventoryManager not found — {pickedItemId} x{qty} lost on client");
#endif
    }

    [Server]
    IEnumerator DestroyAfterRewardDispatch()
    {
        // Keep this identity alive for one transport frame so the targeted
        // reward is dispatched before the shared pickup's destroy message.
        yield return null;
        if (gameObject != null)
            NetworkServer.Destroy(gameObject);
    }

    void OnItemIdChanged(string _, string newVal) => ApplyRarityGlow(newVal);

    void ApplyRarityGlow(string id)
    {
        if (_glowLight == null) return;
        _glowLight.color = GetRarityColor(id);
    }

    public static Color GetRarityColor(string id)
    {
        if (string.IsNullOrEmpty(id))                    return ColorCommon;
        if (id.StartsWith("gold:"))                      return ColorGold;
        if (id.Contains("epic"))                         return ColorEpic;
        if (id.Contains("iron") || id.Contains("rare"))  return ColorRare;
        if (id.Contains("bar")  || id.Contains("uncommon")) return ColorUncommon;
        return ColorCommon;
    }
}
