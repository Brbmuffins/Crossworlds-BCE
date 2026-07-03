#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary>
/// ResourceNode — interactive mining/gathering node. Client-side: F-key within range.
///
/// Upgraded from legacy Inventory.AddItem() system:
/// - Finds local player via Mirror NetworkIdentity.isLocalPlayer
/// - Awards item via POST /api/inventory/save to server API
/// - Awards profession XP via PlayerProgressManager.Local
/// - Refreshes InventoryBagUI.Instance if open
///
/// Depletes after hitsToDeplete harvests, respawns after respawnTime seconds.
/// Works in both Hub (static scene object) and Arena (server-side).
/// </summary>
public class ResourceNode : MonoBehaviour
{
    [Header("Yield")]
    [Tooltip("Item ID in the new item table (e.g. 'ore_copper'). Must match items table in rod_online.")]
    public string yieldItemId = "ore_copper";
    public int yieldQuantity = 1;

    [Header("Interaction")]
    public int   hitsToDeplete = 3;
    public float respawnTime   = 60f;
    public float interactRange = 3f;

    [Header("XP Reward")]
    public int professionId = 2;       // 2 = Mining
    public int professionXpPerHit = 10;

    // ── Runtime ───────────────────────────────────────────────────────────────
    int        _hitsRemaining;
    Collider   _col;
    Renderer[] _renderers;
    bool       _depleted;

    // Player tracking (throttled to avoid per-frame Find)
    Transform  _localPlayer;
    float      _scanTimer;

    // ── Prompt ────────────────────────────────────────────────────────────────
    GameObject _promptGO;
    TextMesh   _promptMesh;
    bool       _promptVisible;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _hitsRemaining = hitsToDeplete;
        _col           = GetComponent<Collider>();
        _renderers     = GetComponentsInChildren<Renderer>();
        BuildPrompt();
    }

    void Start() { _promptGO.SetActive(false); }

    void Update()
    {
        if (_depleted) return;

        // Throttled player search
        _scanTimer -= Time.deltaTime;
        if (_localPlayer == null && _scanTimer <= 0f)
        {
            _scanTimer   = 0.5f;
            _localPlayer = FindLocalPlayer();
        }
        if (_localPlayer == null) return;

        float dist    = Vector3.Distance(transform.position, _localPlayer.position);
        bool  inRange = dist <= interactRange;

        // Show/hide prompt
        if (inRange != _promptVisible)
        {
            _promptVisible = inRange;
            _promptGO.SetActive(inRange && !_depleted);
        }

        // Billboard
        if (_promptVisible)
        {
            var cam = Camera.main;
            if (cam != null)
                _promptGO.transform.rotation = Quaternion.LookRotation(
                    _promptGO.transform.position - cam.transform.position,
                    cam.transform.up);
        }

        // F-key harvest
        if (inRange && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            Harvest();
        }
    }

    // ── Harvest ───────────────────────────────────────────────────────────────

    void Harvest()
    {
        if (_depleted) return;

        // Award item via API
        StartCoroutine(PostInventoryItem());

        // Award profession XP
        PlayerProgressManager.Local?.AwardXp(professionXpPerHit);

        // Refresh inventory UI
        InventoryBagUI.Refresh();

        Debug.Log($"[LOOT] ResourceNode '{yieldItemId}' harvested ×{yieldQuantity}");

        _hitsRemaining--;
        if (_hitsRemaining <= 0)
        {
            StartCoroutine(Deplete());
        }
    }

    // ── API call: POST /api/inventory/save ────────────────────────────────────

    IEnumerator PostInventoryItem()
    {
        string characterId = PlayerPrefs.GetString("SelectedCharacter", "");
        string jwt         = PlayerPrefs.GetString("jwt_token", "");
        string serverIp    = ServerConfig.ServerIP;

        if (string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(jwt))
        {
            Debug.LogWarning("[ResourceNode] Cannot save item — no character or JWT in PlayerPrefs");
            yield break;
        }

        string url  = $"http://{serverIp}:3000/api/inventory/add-item";
        string json = $"{{\"characterId\":\"{characterId}\",\"itemId\":\"{yieldItemId}\",\"quantity\":{yieldQuantity}}}";

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {jwt}");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[ResourceNode] Inventory save failed: {req.error}");
        }
        else
        {
            Debug.Log($"[ResourceNode] {yieldItemId} ×{yieldQuantity} saved to inventory.");
            InventoryBagUI.Refresh();
        }
    }

    // ── Deplete / Respawn ─────────────────────────────────────────────────────

    IEnumerator Deplete()
    {
        _depleted = true;
        _promptGO.SetActive(false);
        SetVisible(false);

        yield return new WaitForSeconds(respawnTime);

        _hitsRemaining = hitsToDeplete;
        _depleted = false;
        SetVisible(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void SetVisible(bool visible)
    {
        if (_col) _col.enabled = visible;
        foreach (var r in _renderers)
            if (r) r.enabled = visible;
    }

    void BuildPrompt()
    {
        _promptGO = new GameObject("ResourcePrompt");
        _promptGO.transform.SetParent(transform, false);
        _promptGO.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        _promptGO.transform.localScale    = Vector3.one * 0.018f;

        _promptMesh = _promptGO.AddComponent<TextMesh>();
        _promptMesh.text          = $"[F]  Mine";
        _promptMesh.characterSize = 0.5f;
        _promptMesh.fontSize      = 60;
        _promptMesh.fontStyle     = FontStyle.Bold;
        _promptMesh.anchor        = TextAnchor.MiddleCenter;
        _promptMesh.alignment     = TextAlignment.Center;
        _promptMesh.color         = new Color(0.70f, 0.95f, 0.50f);
    }

    static Transform FindLocalPlayer()
    {
        foreach (var id in FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (id.isLocalPlayer) return id.transform;
        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 1f, 0.2f, 0.3f);
        Gizmos.DrawSphere(transform.position, interactRange);
    }
}
#endif
