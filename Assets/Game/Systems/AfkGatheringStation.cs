#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

/// <summary>
/// AfkGatheringStation — Press F once to start, then go AFK.
///
/// Drift beyond cancelRadius, press F/Escape, or click STOP to cancel.
/// Items are added via POST /api/inventory/add-item (server validates JWT + ownership).
/// XP is awarded via ProfessionManager.AwardXp → POST /api/professions/award-xp.
///
/// Profession IDs:  0 = Woodcutting  |  1 = Fishing  |  2 = Mining
/// </summary>
public class AfkGatheringStation : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────────

    [Header("Identity")]
    public string stationName = "Copper Vein";

    [Header("Profession")]
    [Tooltip("0 = Woodcutting  1 = Fishing  2 = Mining")]
    public int professionId      = 2;
    public int minLevelRequired  = 1;

    [Header("Yield — per tick")]
    public string itemId       = "ore_copper";
    public int    itemQuantity = 1;
    public float  tickInterval = 5f;

    [Header("XP")]
    public int xpPerTick = 10;

    [Header("Bonus Yield")]
    [Tooltip("At this level there is a 20% chance to award double items per tick.")]
    public int bonusYieldLevel = 10;

    [Header("Depletion & Respawn")]
    [Min(1)] public int minimumAwardsPerSpawn = 1;
    [Min(1)] public int maximumAwardsPerSpawn = 5;
    [Min(1f)] public float respawnSeconds = 900f;

    [Header("Interaction")]
    public float interactRange = 3f;
    [Tooltip("How far the player must drift before gathering auto-cancels.")]
    public float cancelRadius  = 4f;
    [Tooltip("Local height of the floating interaction prompt above the node root.")]
    public float promptHeight = 2.4f;
    [Tooltip("Optional prompt verb such as harvesting or salvaging. Leave empty for profession defaults.")]
    public string interactionVerb = "";

    [Header("VFX")]
    [Tooltip("Particle prefab spawned at the station on each yield tick.")]
    public GameObject tickVFXPrefab;

    [Header("Animation")]
    [Tooltip("Bool parameter name on the local player's Animator to set while gathering. Leave empty to skip.")]
    public string gatheringAnimBool = "";

    // ── Runtime ───────────────────────────────────────────────────────────────────

    bool       _gathering;
    Coroutine  _loop;
    Transform  _localPlayer;
    Vector3    _gatherOrigin;
    float      _scanTimer;
    float      _tickProgress;

    GameObject _promptGO;
    TextMesh   _promptMesh;
    TextMesh   _promptShadowMesh;
    bool       _promptVisible;
    bool       _promptShowsInRange;
    int        _remainingAwards;
    bool       _waitingForAward;
    bool       _awardResolved;
    bool       _awardGranted;
    bool       _awardDepleted;
    double     _localRespawnAt = -1d;

    GatheringNodeNetworkState _networkState;
    Renderer[] _localNodeRenderers;
    Collider[] _localNodeColliders;
    bool[] _localRendererDefaults;
    bool[] _localColliderDefaults;

    readonly HashSet<GatheringNodeHoverTarget> _hoverTargets = new();

    GatheringHUD _hud;

    // ─────────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        _remainingAwards = Random.Range(
            Mathf.Max(1, minimumAwardsPerSpawn),
            Mathf.Max(Mathf.Max(1, minimumAwardsPerSpawn), maximumAwardsPerSpawn) + 1);
        _networkState = GetComponent<GatheringNodeNetworkState>();
        if (_networkState != null)
            _networkState.AwardRequestCompleted += OnAwardRequestCompleted;
        BuildPrompt();
        InstallHoverTargets();
        CacheLocalPresentation();
    }
    void Start()  => _promptGO.SetActive(false);

    void OnDestroy()
    {
        if (_networkState != null)
            _networkState.AwardRequestCompleted -= OnAwardRequestCompleted;
    }

    void Update()
    {
        if (!Mirror.NetworkClient.active && _localRespawnAt > 0d &&
            Time.realtimeSinceStartupAsDouble >= _localRespawnAt)
        {
            int respawnAwards = Random.Range(
                Mathf.Max(1, minimumAwardsPerSpawn),
                Mathf.Max(Mathf.Max(1, minimumAwardsPerSpawn), maximumAwardsPerSpawn) + 1);
            _localRespawnAt = -1d;
            if (_networkState != null) _networkState.ApplyOfflinePresentation(false);
            else ApplyLocalPresentation(false);
            _remainingAwards = respawnAwards;
        }

        // Throttled local-player search
        _scanTimer -= Time.deltaTime;
        if (_scanTimer <= 0f)
        {
            _scanTimer   = 0.5f;
            _localPlayer = FindLocalPlayer();
        }

        // Null-check every frame — player object can be destroyed on disconnect
        if (_localPlayer == null || !_localPlayer.gameObject.activeInHierarchy)
        {
            // Player vanished mid-gather (disconnect/scene change) — stop cleanly
            if (_gathering) StopGathering();
            if (_promptVisible)
            {
                _promptVisible = false;
                _promptGO.SetActive(false);
            }
            return;
        }

        float dist    = Vector3.Distance(transform.position, _localPlayer.position);
        bool  inRange = dist <= interactRange;

        // ── Gathering active ───────────────────────────────────────────────────
        if (_gathering)
        {
            _tickProgress += Time.deltaTime / tickInterval;
            if (_hud != null)
                _hud.SetProgress(_tickProgress, tickInterval - _tickProgress * tickInterval);

            float drift = Vector3.Distance(_localPlayer.position, _gatherOrigin);
            if (drift > cancelRadius)
            {
                StopGathering("You moved away and stopped gathering.");
                return;
            }

            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    StopGathering("You stopped gathering.");
                else if (Keyboard.current.fKey.wasPressedThisFrame)
                    StopGathering("You stopped gathering.");
            }
            return;
        }

        // ── Prompt visibility ──────────────────────────────────────────────────
        // Proximity alone never displays a label. It appears only while the
        // pointer is over this node or one of its visual-child colliders.
        bool showPrompt = _hoverTargets.Count > 0;
        if (showPrompt != _promptVisible)
        {
            _promptVisible = showPrompt;
            _promptGO.SetActive(showPrompt);
        }

        if (_promptVisible)
        {
            if (_promptShowsInRange != inRange)
                UpdatePromptText(inRange);
            var cam = Camera.main;
            if (cam != null)
                _promptGO.transform.rotation = Quaternion.LookRotation(
                    _promptGO.transform.position - cam.transform.position, cam.transform.up);
        }

        if (inRange && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            TryStartGathering();
    }

    // ── Gathering session ─────────────────────────────────────────────────────────

    void TryStartGathering()
    {
        var pm = ProfessionManager.Local;

        // Wait for profession data to load before allowing level-gated stations
        if (pm != null && !pm.IsLoaded && minLevelRequired > 1)
        {
            RodChatManager.Instance?.AddSystemMessage("Loading profession data — try again in a moment.");
            return;
        }

        int level = pm != null ? pm.GetLevel(professionId) : 1;
        if (level < minLevelRequired)
        {
            string name = professionId < ProfessionManager.ProfessionNames.Length
                ? ProfessionManager.ProfessionNames[professionId]
                : $"Profession {professionId}";
            RodChatManager.Instance?.AddSystemMessage(
                $"Requires {name} level {minLevelRequired}. You are level {level}.");
            return;
        }

        _gathering    = true;
        _gatherOrigin = _localPlayer.position;
        _tickProgress = 0f;
        _promptVisible = false;
        _promptGO.SetActive(false);
        SetGatheringAnim(true);

        string verb = GetGatherVerb();
        RodChatManager.Instance?.AddSystemMessage($"You begin {verb} {stationName}...");

        // Pass onStop callback so STOP button cancels cleanly through StopGathering
        _hud = GatheringHUD.Show(stationName, itemId, tickInterval, professionId,
                                 () => StopGathering("You stopped gathering."));

        if (pm != null) pm.onLevelUp += OnLevelUp;

        _loop = StartCoroutine(GatherLoop());
    }

    IEnumerator GatherLoop()
    {
        while (_gathering)
        {
            yield return new WaitForSeconds(tickInterval);
            if (!_gathering) yield break;

            _tickProgress = 0f;

            if (_networkState != null && Mirror.NetworkClient.active)
            {
                _waitingForAward = true;
                _awardResolved = false;
                _awardGranted = false;
                _awardDepleted = false;
                _networkState.RequestAward();
                while (_gathering && !_awardResolved) yield return null;
                _waitingForAward = false;
                if (!_gathering) yield break;
                if (!_awardGranted)
                {
                    if (_awardDepleted)
                        StopGathering($"{stationName} is depleted. It will respawn in 15 minutes.");
                    continue;
                }
            }
            else
            {
                // Offline editor fallback. Networked games always use the shared
                // server-authoritative counter above.
                if (_remainingAwards <= 0)
                {
                    StopGathering($"{stationName} is depleted.");
                    yield break;
                }
                _remainingAwards--;
                _awardDepleted = _remainingAwards <= 0;
                if (_awardDepleted)
                {
                    _localRespawnAt = Time.realtimeSinceStartupAsDouble + Mathf.Max(1f, respawnSeconds);
                    if (_networkState != null) _networkState.ApplyOfflinePresentation(true);
                    else ApplyLocalPresentation(true);
                }
            }

            int qty   = itemQuantity;
            int level = ProfessionManager.Local?.GetLevel(professionId) ?? 1;
            if (level >= bonusYieldLevel && Random.value < 0.20f)
            {
                qty *= 2;
                RodChatManager.Instance?.AddSystemMessage($"Bonus yield! x{qty} {itemId}");
            }

            StartCoroutine(PostItem(qty));
            ProfessionManager.Local?.AwardXp(professionId, xpPerTick);
            SpawnTickVFX();

            if (_hud != null) _hud.Pulse(qty);

            Debug.Log($"[GATHER] {stationName}: +{qty}x {itemId}, +{xpPerTick} xp");

            if (_awardDepleted)
            {
                StopGathering($"{stationName} is depleted. It will respawn in 15 minutes.");
                yield break;
            }
        }
    }

    void StopGathering(string message = null)
    {
        if (!_gathering) return;
        _gathering = false;
        SetGatheringAnim(false);

        if (_loop != null) { StopCoroutine(_loop); _loop = null; }

        var pm = ProfessionManager.Local;
        if (pm != null) pm.onLevelUp -= OnLevelUp;

        // Only call Hide if the HUD object still exists (STOP button may have
        // already destroyed it via the onStop callback path)
        if (_hud != null)
        {
            _hud.Hide();
            _hud = null;
        }

        _tickProgress = 0f;

        if (!string.IsNullOrEmpty(message))
            RodChatManager.Instance?.AddSystemMessage(message);

        if (_localPlayer != null)
        {
            float d = Vector3.Distance(transform.position, _localPlayer.position);
            _promptVisible = _hoverTargets.Count > 0;
            UpdatePromptText(d <= interactRange);
            _promptGO.SetActive(_promptVisible);
        }
    }

    void OnLevelUp(int profId, int newLevel)
    {
        if (profId != professionId || _hud == null) return;
        _hud.FlashLevelUp(newLevel);
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
            Debug.LogWarning($"[GATHER] Inventory save failed ({req.responseCode}): {req.error}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    void SpawnTickVFX()
    {
        if (tickVFXPrefab == null) return;
        var vfx = Instantiate(tickVFXPrefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);
        Destroy(vfx, 3f);
    }

    void SetGatheringAnim(bool on)
    {
        if (string.IsNullOrEmpty(gatheringAnimBool) || _localPlayer == null) return;
        var anim = _localPlayer.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetBool(gatheringAnimBool, on);
    }

    string GetGatherVerb()
    {
        if (!string.IsNullOrWhiteSpace(interactionVerb)) return interactionVerb.Trim();
        return professionId switch
        {
            0 => "chopping",
            1 => "fishing at",
            _ => "mining"
        };
    }

    void BuildPrompt()
    {
        _promptGO = new GameObject("GatherPrompt");
        _promptGO.transform.SetParent(transform, false);
        _promptGO.transform.localPosition = new Vector3(0f, promptHeight, 0f);
        _promptGO.transform.localScale    = Vector3.one * 0.032f;

        _promptMesh = _promptGO.AddComponent<TextMesh>();
        _promptMesh.characterSize = 0.5f;
        _promptMesh.fontSize      = 64;
        _promptMesh.fontStyle     = FontStyle.Bold;
        _promptMesh.anchor        = TextAnchor.MiddleCenter;
        _promptMesh.alignment     = TextAlignment.Center;
        _promptMesh.color         = minLevelRequired <= 1
            ? new Color(0.70f, 0.95f, 0.50f)
            : new Color(0.90f, 0.80f, 0.30f);

        var shadowGO = new GameObject("TextShadow");
        shadowGO.transform.SetParent(_promptGO.transform, false);
        shadowGO.transform.localPosition = new Vector3(0.035f, -0.035f, 0.01f);
        _promptShadowMesh = shadowGO.AddComponent<TextMesh>();
        _promptShadowMesh.characterSize = _promptMesh.characterSize;
        _promptShadowMesh.fontSize = _promptMesh.fontSize;
        _promptShadowMesh.fontStyle = FontStyle.Bold;
        _promptShadowMesh.anchor = TextAnchor.MiddleCenter;
        _promptShadowMesh.alignment = TextAlignment.Center;
        _promptShadowMesh.color = new Color(0f, 0f, 0f, 0.9f);

        ConfigurePromptRenderer(_promptMesh, 2);
        ConfigurePromptRenderer(_promptShadowMesh, 1);
        UpdatePromptText(false);
    }

    void UpdatePromptText(bool inRange)
    {
        _promptShowsInRange = inRange;
        string text;
        if (inRange)
        {
            string verb = GetGatherVerb();
            text = $"[F]  {char.ToUpper(verb[0])}{verb.Substring(1)} — {stationName}\n" +
                   $"Level {minLevelRequired}+ required\n" +
                   $"{Mathf.Max(0, _remainingAwards)} award(s) remaining";
        }
        else text = $"{stationName}\nMove closer";

        _promptMesh.text = text;
        if (_promptShadowMesh != null) _promptShadowMesh.text = text;
    }

    static void ConfigurePromptRenderer(TextMesh mesh, int sortingOrder)
    {
        if (mesh == null) return;
        var renderer = mesh.GetComponent<MeshRenderer>();
        if (renderer == null) return;
        renderer.sortingOrder = sortingOrder;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    void InstallHoverTargets()
    {
        foreach (Collider nodeCollider in GetComponentsInChildren<Collider>(true))
        {
            GatheringNodeHoverTarget target =
                nodeCollider.GetComponent<GatheringNodeHoverTarget>() ??
                nodeCollider.gameObject.AddComponent<GatheringNodeHoverTarget>();
            target.owner = this;
        }
    }

    internal void SetHovered(GatheringNodeHoverTarget target, bool hovered)
    {
        if (target == null) return;
        if (hovered) _hoverTargets.Add(target);
        else _hoverTargets.Remove(target);
    }

    internal void SetRemainingAwards(int value)
    {
        _remainingAwards = Mathf.Max(0, value);
        if (_promptVisible && _localPlayer != null)
            UpdatePromptText(Vector3.Distance(transform.position, _localPlayer.position) <= interactRange);
    }

    internal void SetNodeDepleted(bool depleted)
    {
        if (!depleted) return;
        _hoverTargets.Clear();
        _promptVisible = false;
        if (_promptGO != null) _promptGO.SetActive(false);
        if (_gathering && !_waitingForAward)
            StopGathering($"{stationName} is depleted. It will respawn in 15 minutes.");
    }

    void OnAwardRequestCompleted(bool granted, bool depleted, int remaining)
    {
        _awardGranted = granted;
        _awardDepleted = depleted;
        _remainingAwards = Mathf.Max(0, remaining);
        _awardResolved = true;
    }

    void CacheLocalPresentation()
    {
        _localNodeRenderers = GetComponentsInChildren<Renderer>(true);
        _localNodeColliders = GetComponentsInChildren<Collider>(true);
        _localRendererDefaults = new bool[_localNodeRenderers.Length];
        _localColliderDefaults = new bool[_localNodeColliders.Length];
        for (int i = 0; i < _localNodeRenderers.Length; i++)
            _localRendererDefaults[i] = _localNodeRenderers[i] != null && _localNodeRenderers[i].enabled;
        for (int i = 0; i < _localNodeColliders.Length; i++)
            _localColliderDefaults[i] = _localNodeColliders[i] != null && _localNodeColliders[i].enabled;
    }

    void ApplyLocalPresentation(bool hide)
    {
        for (int i = 0; i < _localNodeRenderers.Length; i++)
            if (_localNodeRenderers[i] != null)
                _localNodeRenderers[i].enabled = !hide && _localRendererDefaults[i];
        for (int i = 0; i < _localNodeColliders.Length; i++)
            if (_localNodeColliders[i] != null)
                _localNodeColliders[i].enabled = !hide && _localColliderDefaults[i];
        SetNodeDepleted(hide);
    }

    static Transform FindLocalPlayer()
    {
        foreach (var id in FindObjectsByType<Mirror.NetworkIdentity>(FindObjectsInactive.Exclude))
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

/// <summary>Forwards Unity mouse-hover events from node child colliders.</summary>
public sealed class GatheringNodeHoverTarget : MonoBehaviour
{
    [HideInInspector] public AfkGatheringStation owner;

    void OnMouseEnter() => owner?.SetHovered(this, true);
    void OnMouseOver() => owner?.SetHovered(this, true);
    void OnMouseExit() => owner?.SetHovered(this, false);
    void OnDisable() => owner?.SetHovered(this, false);
}
#endif
