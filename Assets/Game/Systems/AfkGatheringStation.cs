#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

/// <summary>
/// AfkGatheringStation — Press F once, then go AFK.
///
/// The player presses F to start gathering. Every <tickInterval> seconds
/// the station awards one item (via /api/inventory/add-item) and some
/// profession XP (via ProfessionManager.AwardXp). Moving away, pressing F
/// again, or pressing Escape cancels the session.
///
/// Profession IDs:  0 = Woodcutting  |  1 = Fishing  |  2 = Mining
///
/// Level gates:
///   minLevelRequired — station refuses to start if player is below this level.
///   bonusYieldLevel  — at this level the station has a 20% chance to double yield.
///
/// Place this on any mesh in the Hub scene. No network component required —
/// resource gathering is purely client-authoritative for alpha; the server
/// validates via JWT + character ownership on every inventory/XP write.
/// </summary>
public class AfkGatheringStation : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────────

    [Header("Identity")]
    [Tooltip("Display name shown in the HUD while gathering.")]
    public string stationName = "Copper Vein";

    [Header("Profession")]
    [Tooltip("0 = Woodcutting  1 = Fishing  2 = Mining")]
    public int professionId = 2;
    public int minLevelRequired = 1;

    [Header("Yield — per tick")]
    public string itemId = "ore_copper";
    public int    itemQuantity = 1;
    public float  tickInterval = 5f;     // seconds between each reward

    [Header("XP")]
    public int xpPerTick = 10;

    [Header("Bonus Yield")]
    [Tooltip("At this level there is a 20 % chance to award double items per tick.")]
    public int bonusYieldLevel = 10;

    [Header("Interaction")]
    public float interactRange = 3f;
    [Tooltip("How far the player must drift before gathering auto-cancels.")]
    public float cancelRadius = 4f;

    // ── Runtime ───────────────────────────────────────────────────────────────────

    bool       _gathering;
    Coroutine  _loop;
    Transform  _localPlayer;
    Vector3    _gatherOrigin;     // player position when gathering started
    float      _scanTimer;
    float      _tickProgress;    // 0–1 within the current tick

    GameObject _promptGO;
    TextMesh   _promptMesh;
    bool       _promptVisible;

    // HUD (instance owned by this station while gathering)
    GatheringHUD _hud;

    // ─────────────────────────────────────────────────────────────────────────────

    void Awake() => BuildPrompt();
    void Start()  => _promptGO.SetActive(false);

    void Update()
    {
        // Throttled player search
        _scanTimer -= Time.deltaTime;
        if (_localPlayer == null && _scanTimer <= 0f)
        {
            _scanTimer   = 0.5f;
            _localPlayer = FindLocalPlayer();
        }
        if (_localPlayer == null) return;

        float dist   = Vector3.Distance(transform.position, _localPlayer.position);
        bool  inRange = dist <= interactRange;

        // Drift cancel while gathering
        if (_gathering)
        {
            _tickProgress += Time.deltaTime / tickInterval;
            _hud?.SetProgress(_tickProgress, tickInterval - (_tickProgress * tickInterval));

            float drift = Vector3.Distance(_localPlayer.position, _gatherOrigin);
            if (drift > cancelRadius)
                StopGathering("You moved away and stopped gathering.");

            // Escape or F cancels
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    StopGathering("You stopped gathering.");
                if (Keyboard.current.fKey.wasPressedThisFrame)
                    StopGathering("You stopped gathering.");
            }
            return;
        }

        // Prompt visibility
        if (inRange != _promptVisible)
        {
            _promptVisible = inRange;
            _promptGO.SetActive(inRange);
        }

        // Billboard prompt
        if (_promptVisible)
        {
            var cam = Camera.main;
            if (cam != null)
                _promptGO.transform.rotation = Quaternion.LookRotation(
                    _promptGO.transform.position - cam.transform.position, cam.transform.up);
        }

        // Start gathering
        if (inRange && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            TryStartGathering();
    }

    // ── Gathering session ─────────────────────────────────────────────────────────

    void TryStartGathering()
    {
        // Level gate
        int level = ProfessionManager.Local != null
            ? ProfessionManager.Local.GetLevel(professionId)
            : 1;

        if (level < minLevelRequired)
        {
            string profName = professionId < ProfessionManager.ProfessionNames.Length
                ? ProfessionManager.ProfessionNames[professionId]
                : $"Profession {professionId}";
            RodChatManager.Instance?.AddSystemMessage(
                $"Requires {profName} level {minLevelRequired}. You are level {level}.");
            return;
        }

        _gathering    = true;
        _gatherOrigin = _localPlayer.position;
        _tickProgress = 0f;
        _promptGO.SetActive(false);

        // Announce
        string verb = GetGatherVerb();
        RodChatManager.Instance?.AddSystemMessage($"You begin {verb} {stationName}...");

        // HUD
        _hud = GatheringHUD.Show(stationName, itemId, tickInterval);

        // Subscribe to level-up so HUD can flash
        if (ProfessionManager.Local != null)
            ProfessionManager.Local.onLevelUp += OnLevelUp;

        _loop = StartCoroutine(GatherLoop());
    }

    IEnumerator GatherLoop()
    {
        while (_gathering)
        {
            yield return new WaitForSeconds(tickInterval);
            if (!_gathering) yield break;

            _tickProgress = 0f;

            // Bonus yield
            int qty = itemQuantity;
            int level = ProfessionManager.Local?.GetLevel(professionId) ?? 1;
            if (level >= bonusYieldLevel && Random.value < 0.20f)
            {
                qty *= 2;
                RodChatManager.Instance?.AddSystemMessage($"⭐ Bonus yield! ×{qty} {itemId}");
            }

            // Award item
            StartCoroutine(PostItem(qty));

            // Award profession XP
            ProfessionManager.Local?.AwardXp(professionId, xpPerTick);

            // HUD pulse
            _hud?.Pulse(qty);

            Debug.Log($"[GATHER] {stationName}: +{qty}× {itemId}, +{xpPerTick} xp");
        }
    }

    void StopGathering(string message = null)
    {
        if (!_gathering) return;
        _gathering = false;

        if (_loop != null) { StopCoroutine(_loop); _loop = null; }

        if (ProfessionManager.Local != null)
            ProfessionManager.Local.onLevelUp -= OnLevelUp;

        _hud?.Hide();
        _hud = null;
        _tickProgress = 0f;

        if (!string.IsNullOrEmpty(message))
            RodChatManager.Instance?.AddSystemMessage(message);

        // Restore prompt if still in range
        if (_localPlayer != null)
        {
            float d = Vector3.Distance(transform.position, _localPlayer.position);
            _promptVisible = d <= interactRange;
            _promptGO.SetActive(_promptVisible);
        }
    }

    void OnLevelUp(int profId, int newLevel)
    {
        if (profId != professionId) return;
        _hud?.FlashLevelUp(newLevel);
    }

    // ── Item API ──────────────────────────────────────────────────────────────────

    IEnumerator PostItem(int qty)
    {
        int    charId = AuthManager.CharacterId;
        string jwt    = AuthManager.Token;
        if (charId <= 0 || string.IsNullOrEmpty(jwt)) yield break;

        string json = $"{{\"characterId\":{charId},\"itemId\":\"{itemId}\",\"quantity\":{qty}}}";

        using var req = new UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/inventory/add-item", "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {jwt}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            InventoryBagUI.Refresh();
        else
            Debug.LogWarning($"[GATHER] Inventory save failed: {req.error}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    string GetGatherVerb() => professionId switch
    {
        0 => "chopping",
        1 => "fishing at",
        _ => "mining"
    };

    void BuildPrompt()
    {
        string verb = GetGatherVerb();

        _promptGO = new GameObject("GatherPrompt");
        _promptGO.transform.SetParent(transform, false);
        _promptGO.transform.localPosition = new Vector3(0f, 2.4f, 0f);
        _promptGO.transform.localScale    = Vector3.one * 0.018f;

        _promptMesh = _promptGO.AddComponent<TextMesh>();
        _promptMesh.text          = $"[F]  {char.ToUpper(verb[0])}{verb.Substring(1)} — {stationName}\nLevel {minLevelRequired}+ required";
        _promptMesh.characterSize = 0.5f;
        _promptMesh.fontSize      = 54;
        _promptMesh.fontStyle     = FontStyle.Bold;
        _promptMesh.anchor        = TextAnchor.MiddleCenter;
        _promptMesh.alignment     = TextAlignment.Center;
        _promptMesh.color         = minLevelRequired <= 1
            ? new Color(0.70f, 0.95f, 0.50f)   // green — available
            : new Color(0.90f, 0.80f, 0.30f);   // gold — gated
    }

    static Transform FindLocalPlayer()
    {
        foreach (var id in FindObjectsByType<Mirror.NetworkIdentity>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (id.isLocalPlayer) return id.transform;
        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 1f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, cancelRadius);
    }
}
#endif
