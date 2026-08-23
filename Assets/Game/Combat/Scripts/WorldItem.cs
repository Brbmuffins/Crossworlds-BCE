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

    [SyncVar(hook = nameof(OnLootRarityChanged))]
    public ItemRarity lootRarity = ItemRarity.Common;

    [Header("Float / Spin")]
    public float floatSpeed     = 1.3f;
    public float floatAmplitude = 0.18f;
    public float rotateSpeed    = 55f;

    // Glow light — auto-created in Start(), no Inspector assignment required.
    private Light _glowLight;
    private GameObject _lootBeamInstance;
    private GameObject _lootVisualInstance;

    private Vector3 _origin;
    private bool    _pickedUp = false;
    private bool    _pickupRequested = false;
    private int     _pendingPickerConnectionId = -1;

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
        {
            if (sender != null) TargetResetPickup(sender);
            return;
        }

        NetworkIdentity picker = sender.identity;
        if (picker.connectionToClient != sender)
        {
            TargetResetPickup(sender);
            return;
        }

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
            TargetResetPickup(sender);
            return;
        }

        _pickedUp = true;
        _pendingPickerConnectionId = sender.connectionId;
        TargetOnPickedUp(sender, itemId, quantity);
        if (itemId.StartsWith("gold:"))
            StartCoroutine(DestroyAfterRewardDispatch());
        else
            StartCoroutine(ResetTimedOutPickup(sender.connectionId));
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
            inv.PersistWorldPickup(pickedItemId, qty,
                stored => CmdCompleteItemPickup(stored));
        else
        {
            Debug.LogWarning($"[LOOT] InventoryManager not found — {pickedItemId} x{qty} lost on client");
            CmdCompleteItemPickup(0);
        }
#endif
    }

    [Command(requiresAuthority = false)]
    void CmdCompleteItemPickup(int stored, NetworkConnectionToClient sender = null)
    {
        if (!_pickedUp || sender == null ||
            sender.connectionId != _pendingPickerConnectionId)
            return;

        int persisted = Mathf.Clamp(stored, 0, Mathf.Max(1, quantity));
        if (persisted > 0)
        {
            QuestLocalRuntime.ServerReport(sender,
                QuestObjectiveType.CollectItem, itemId, persisted);
            quantity -= persisted;
        }

        if (quantity <= 0)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }

        _pickedUp = false;
        _pendingPickerConnectionId = -1;
        TargetResetPickup(sender);
    }

    [TargetRpc]
    void TargetResetPickup(NetworkConnectionToClient target)
    {
        _pickupRequested = false;
    }

    [Server]
    IEnumerator ResetTimedOutPickup(int connectionId)
    {
        yield return new WaitForSeconds(12f);
        if (!_pickedUp || _pendingPickerConnectionId != connectionId) yield break;
        _pickedUp = false;
        _pendingPickerConnectionId = -1;
        if (NetworkServer.connections.TryGetValue(
                connectionId, out NetworkConnectionToClient connection))
            TargetResetPickup(connection);
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

    /// <summary>
    /// Attaches one local visual beam to this synchronized pickup. The beam is
    /// parented to the WorldItem so it follows the floating loot and is removed
    /// automatically when the pickup's network identity is destroyed.
    /// </summary>
    public void AttachLootBeam(GameObject beamPrefab)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (beamPrefab == null || _lootBeamInstance != null)
            return;

        _lootBeamInstance = Instantiate(
            beamPrefab,
            transform.position,
            Quaternion.identity,
            transform);
        _lootBeamInstance.name = $"{beamPrefab.name} (Loot Beam)";
        _lootBeamInstance.transform.localPosition = Vector3.zero;
        foreach (Collider beamCollider in
                 _lootBeamInstance.GetComponentsInChildren<Collider>(true))
            beamCollider.enabled = false;
        ApplyLootBeamColor(GetRarityColor(itemId, lootRarity));
#endif
    }

    /// <summary>
    /// Replaces the pickup wrapper's default Visual child with the model
    /// authored on the matching DropEntry. The model stays local visual data;
    /// the WorldItem root remains the single networked pickup identity.
    /// </summary>
    public void AttachLootVisual(GameObject visualPrefab)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (visualPrefab == null || _lootVisualInstance != null)
            return;

        Transform defaultVisual = transform.Find("Visual");
        if (defaultVisual != null)
            defaultVisual.gameObject.SetActive(false);

        _lootVisualInstance = Instantiate(
            visualPrefab,
            transform.position,
            transform.rotation,
            transform);
        _lootVisualInstance.name = $"{visualPrefab.name} (Loot Visual)";
        _lootVisualInstance.transform.localPosition = Vector3.zero;
        _lootVisualInstance.transform.localRotation = Quaternion.identity;

        foreach (Collider visualCollider in
                 _lootVisualInstance.GetComponentsInChildren<Collider>(true))
            visualCollider.enabled = false;
#endif
    }

    public void AttachLootPresentation(
        GameObject visualPrefab,
        GameObject beamPrefab)
    {
        AttachLootVisual(visualPrefab);
        AttachLootBeam(beamPrefab);
    }

    void ApplyRarityGlow(string id)
    {
        Color rarityColor = GetRarityColor(id, lootRarity);
        if (_glowLight != null)
            _glowLight.color = rarityColor;
        ApplyLootBeamColor(rarityColor);
    }

    void OnLootRarityChanged(ItemRarity _, ItemRarity newRarity)
    {
        ApplyRarityGlow(itemId);
    }

    void ApplyLootBeamColor(Color color)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (_lootBeamInstance == null)
            return;

        foreach (ParticleSystem particles in
                 _lootBeamInstance.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = particles.main;
            main.startColor = TintParticleColor(main.startColor, color);
        }

        foreach (Light beamLight in
                 _lootBeamInstance.GetComponentsInChildren<Light>(true))
            beamLight.color = color;

        foreach (TrailRenderer trail in
                 _lootBeamInstance.GetComponentsInChildren<TrailRenderer>(true))
            trail.colorGradient = TintGradient(trail.colorGradient, color);

        foreach (LineRenderer line in
                 _lootBeamInstance.GetComponentsInChildren<LineRenderer>(true))
            line.colorGradient = TintGradient(line.colorGradient, color);
#endif
    }

#if UNITY_EDITOR || !UNITY_SERVER
    static ParticleSystem.MinMaxGradient TintParticleColor(
        ParticleSystem.MinMaxGradient source,
        Color tint)
    {
        switch (source.mode)
        {
            case ParticleSystemGradientMode.TwoColors:
                return new ParticleSystem.MinMaxGradient(
                    TintColor(source.colorMin, tint),
                    TintColor(source.colorMax, tint));
            case ParticleSystemGradientMode.Gradient:
                return new ParticleSystem.MinMaxGradient(
                    TintGradient(source.gradient, tint));
            case ParticleSystemGradientMode.TwoGradients:
                return new ParticleSystem.MinMaxGradient(
                    TintGradient(source.gradientMin, tint),
                    TintGradient(source.gradientMax, tint));
            case ParticleSystemGradientMode.RandomColor:
                return new ParticleSystem.MinMaxGradient(
                    TintGradient(source.gradient, tint));
            default:
                return new ParticleSystem.MinMaxGradient(
                    TintColor(source.color, tint));
        }
    }

    static Gradient TintGradient(Gradient source, Color tint)
    {
        if (source == null)
            return new Gradient();

        GradientColorKey[] colorKeys = source.colorKeys;
        for (int i = 0; i < colorKeys.Length; i++)
            colorKeys[i].color = TintColor(colorKeys[i].color, tint);

        var result = new Gradient();
        result.mode = source.mode;
        result.SetKeys(colorKeys, source.alphaKeys);
        return result;
    }

    static Color TintColor(Color source, Color tint)
    {
        float brightness = Mathf.Max(source.r, Mathf.Max(source.g, source.b));
        return new Color(
            tint.r * brightness,
            tint.g * brightness,
            tint.b * brightness,
            source.a * tint.a);
    }
#endif

    public static Color GetRarityColor(string id) =>
        GetRarityColor(id, ItemRarity.Common);

    public static Color GetRarityColor(string id, ItemRarity rarity)
    {
        if (string.IsNullOrEmpty(id))                    return ItemRarityUtility.Color(ItemRarity.Common);
        if (id.StartsWith("gold:"))                      return ColorGold;
        if (rarity != ItemRarity.Common) return ItemRarityUtility.Color(rarity);

        // Preserve the existing naming-convention fallback for older tables
        // that have not authored the new rarity field yet.
        return ItemRarityUtility.Color(ItemRarityUtility.InferLegacyItemId(id));
    }
}
