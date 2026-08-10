using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// ═══════════════════════════════════════════════════════════════════════════
//  RodChatManager
//  Attach to a GameObject with NetworkIdentity in the Hub scene.
//  The Hub scene builder (RoD/Setup/5) does this automatically.
//
//  Flow:
//    Client presses Enter or T → input opens
//    Client types + presses Enter → [Command] CmdSendChat(msg)
//    Server pulls username from conn.authenticationData (anti-spoof)
//    Server → [ClientRpc] RpcReceiveChat(username, msg, unixTimestamp)
//    All clients render the message
//
//  Keys:
//    Enter / T     — open / focus chat input
//    Enter         — send message
//    Escape        — close without sending
//
//  Chat fades out FADE_AFTER seconds after the last message when not active.
//  It reappears instantly on any new message or when the player opens it.
//
//  Inspector:
//    No public fields required — fully procedural UI.
// ═══════════════════════════════════════════════════════════════════════════

public class RodChatManager : NetworkBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────
    public static RodChatManager Instance { get; private set; }

    /// <summary>True while the chat input field is open. Used by CameraFollow to suppress camera orbit.</summary>
    public bool IsOpen => _open;
    public bool IsGmModeActive => _gmModeActive;

    // ── Tunables ──────────────────────────────────────────────────────────
    const int   MAX_MESSAGES = 60;
    const int   MAX_MSG_LEN  = 200;
    const float FADE_DELAY   = 8f;   // seconds of inactivity before fade starts
    const float FADE_TIME    = 2f;   // seconds to fully fade

    // ── UI ────────────────────────────────────────────────────────────────
    Canvas         _canvas;
    CanvasGroup    _cg;
    GameObject     _panel;
    TMP_Text       _log;
    GameObject     _inputArea;
    TMP_InputField _input;

    bool   _open;
    float  _lastMsgTime = -999f;
    string _typedText   = "";

    bool _gmFlyActive;
    bool _gmFreeCameraActive;
    bool _gmModeActive;
    bool _gmMapTravelPending;
    float _gmSpeedMultiplier = 1f;
    float _gmFreeCameraSpeed = RodPlayerAuth.DefaultFreeCameraSpeed;
    CameraFollow _gmFreeCamera;
    GameObject _gmLocalPlayer;
    PlayerMovement _gmMovement;
    Rigidbody _gmRigidbody;
    float _gmBaseMoveSpeed;
    float _gmBaseSprintSpeed;

    readonly List<string> _history = new();

    public void RequestOnlineRoster()
    {
        // NetworkClient.active (not isClient): during client shutdown the identity is
        // still isClient==true while the connection has already gone inactive, so a
        // roster refresh fired from OnStopClient teardown would hit SendCommandInternal's
        // "without an active client" guard and log an error. Skip the Cmd once inactive.
        if (NetworkClient.active && isClient)
            CmdRequestOnlineRoster();
    }

    [Command(requiresAuthority = false)]
    void CmdRequestOnlineRoster(NetworkConnectionToClient sender = null)
    {
        if (sender == null) return;

        var names = new List<string>();
        var classes = new List<int>();
        var scenes = new List<string>();
        int localIndex = -1;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn == null || !conn.isAuthenticated) continue;

            var identity = conn.identity != null
                ? conn.identity.GetComponent<PlayerIdentity>()
                : null;
            var auth = conn.authenticationData as RodPlayerAuth;

            string playerName = identity != null && !string.IsNullOrWhiteSpace(identity.playerName)
                ? identity.playerName
                : auth != null && !string.IsNullOrWhiteSpace(auth.username)
                    ? auth.username
                    : "Connecting...";
            int classIndex = identity != null
                ? identity.classIndex
                : auth != null ? auth.classIndex : 0;
            string sceneName = conn.identity != null && conn.identity.gameObject.scene.IsValid()
                ? conn.identity.gameObject.scene.name
                : SceneNames.NormalizeZone(auth != null ? auth.zone : null);

            if (conn == sender) localIndex = names.Count;
            names.Add(playerName);
            classes.Add(classIndex);
            scenes.Add(sceneName);
        }

        TargetReceiveOnlineRoster(
            sender, names.ToArray(), classes.ToArray(), scenes.ToArray(), localIndex);
    }

    [TargetRpc]
    void TargetReceiveOnlineRoster(
        NetworkConnection target,
        string[] names,
        int[] classes,
        string[] scenes,
        int localIndex)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        PlayerListUI.ReceiveOnlineRoster(names, classes, scenes, localIndex);
#endif
    }

    // Quest Forge transport. Quest rules remain isolated in QuestLocalRuntime;
    // this global Mirror object only carries owner requests and responses.
    public void RequestQuestAccept(string questId)
    {
        if (isClient && !string.IsNullOrWhiteSpace(questId)) CmdQuestAccept(questId);
    }

    public void RequestQuestComplete(string questId)
    {
        if (isClient && !string.IsNullOrWhiteSpace(questId)) CmdQuestComplete(questId);
    }

    public void RequestQuestInteraction(string targetId)
    {
        if (isClient && !string.IsNullOrWhiteSpace(targetId)) CmdQuestInteraction(targetId);
    }

    [Command(requiresAuthority = false)]
    void CmdQuestAccept(string questId, NetworkConnectionToClient sender = null) =>
        QuestLocalRuntime.ServerAccept(sender, questId);

    [Command(requiresAuthority = false)]
    void CmdQuestComplete(string questId, NetworkConnectionToClient sender = null) =>
        QuestLocalRuntime.ServerComplete(sender, questId);

    [Command(requiresAuthority = false)]
    void CmdQuestInteraction(string targetId, NetworkConnectionToClient sender = null) =>
        QuestLocalRuntime.ServerRequestInteraction(sender, targetId);

    [Server]
    public void ServerSendQuestState(NetworkConnectionToClient target, string json)
    {
        if (target != null) TargetReceiveQuestState(target, json);
    }

    [Server]
    public void ServerGrantQuestReward(
        NetworkConnectionToClient target, int gold, int xp, string itemId, int itemQuantity)
    {
        if (target != null) TargetGrantQuestReward(target, gold, xp, itemId ?? "", itemQuantity);
    }

    [Server]
    public void ServerRefreshQuestRewards(NetworkConnectionToClient target)
    {
        if (target != null) TargetRefreshQuestRewards(target);
    }

    [TargetRpc]
    void TargetReceiveQuestState(NetworkConnection target, string json)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        QuestLocalRuntime.ClientApplyState(json);
#endif
    }

    [TargetRpc]
    void TargetGrantQuestReward(
        NetworkConnection target, int gold, int xp, string itemId, int itemQuantity)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        QuestLocalRuntime.ClientGrantReward(gold, xp, itemId, itemQuantity);
#endif
    }

    [TargetRpc]
    void TargetRefreshQuestRewards(NetworkConnection target)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        PlayerProgressManager.Local?.Refresh();
        InventoryBagUI.Refresh();
#endif
    }

    // ── Name colours — deterministic from username hash ───────────────────
    static readonly string[] NAME_COLORS =
        { "#f472b6", "#a78bfa", "#34d399", "#60a5fa", "#fbbf24", "#fb923c" };

    // ── Lifecycle ─────────────────────────────────────────────────────────

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (Instance != null && Instance != this) return;
        Instance = this;

        EnsureEventSystem();
        BuildUI();
        AddSystemMessage("Connected to Hub.");
        CmdRequestGmState();

        // Keep chat visible across all ServerChangeScene transitions.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (Instance == null) Instance = this;
    }

    public override void OnStopClient()
    {
        if (_gmFreeCamera != null)
            _gmFreeCamera.SetFreeCameraEnabled(false);
        base.OnStopClient();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                       UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Re-wire EventSystem in case the new scene destroyed the one we made.
        EnsureEventSystem();
        ResetGmPlayerCache();
        _gmMapTravelPending = false;
        if (_gmFlyActive || !Mathf.Approximately(_gmSpeedMultiplier, 1f))
            ApplyGmMovementState();
        if (_gmFreeCameraActive)
            ApplyGmFreeCameraState();
        AddSystemMessage($"Entered {scene.name}.");
    }

    /// <summary>
    /// Hub scene wipes all non-Network objects (including the EventSystem from LoginScene).
    /// Without an EventSystem, UI button clicks never fire and IsPointerOverGameObject() is
    /// always false — causing every left-click to lock the cursor and re-center it.
    /// </summary>
    static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var go = new GameObject("EventSystem",
            typeof(EventSystem),
            typeof(InputSystemUIInputModule),
            typeof(SingleEventSystem));

        // Don't destroy across scenes — Mirror may load scenes mid-session
        DontDestroyOnLoad(go);
    }

    void OnDestroy()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
        if (Instance == this) Instance = null;
        if (_canvas != null) Destroy(_canvas.gameObject);
    }

    void Update()
    {
        if (_canvas == null) return;

        // ── Input handling ─────────────────────────────────────────────
        var kb = Keyboard.current;
        if (kb == null) return;

#if UNITY_EDITOR || !UNITY_SERVER
        if (!_open &&
            _gmModeActive &&
            kb.mKey.wasPressedThisFrame &&
            !AnyOtherInputFocused())
        {
            ToggleGmMap();
        }
#endif

        if (!_open)
        {
            bool openKey = kb.enterKey.wasPressedThisFrame
                        || kb.numpadEnterKey.wasPressedThisFrame
                        || kb.tKey.wasPressedThisFrame;

            if (openKey && !AnyOtherInputFocused())
                OpenInput();
        }
        else
        {
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                CloseInput(send: true);
            else if (kb.escapeKey.wasPressedThisFrame)
                CloseInput(send: false);
            else if (kb.backspaceKey.wasPressedThisFrame && _typedText.Length > 0)
            {
                _typedText = _typedText[..^1];
                _input.text = _typedText;
            }
        }

        // ── Fade ───────────────────────────────────────────────────────
        if (!_open)
        {
            float elapsed = Time.unscaledTime - _lastMsgTime;
            float t = Mathf.InverseLerp(FADE_DELAY, FADE_DELAY + FADE_TIME, elapsed);
            _cg.alpha = Mathf.Lerp(1f, 0f, t);

            // Keep blocksRaycasts ON even when faded so clicking the chat area doesn't
            // count as "clicked on world" and trigger cursor lock → re-centering.
            // interactable=false prevents actual interaction with the invisible elements.
            _cg.blocksRaycasts = true;
            _cg.interactable   = false;
        }
        else
        {
            _cg.alpha          = 1f;
            _cg.blocksRaycasts = true;
            _cg.interactable   = true;
        }

        UpdateGmMovement();
        UpdateGmFreeCamera();
    }

    // ── Server-side command ───────────────────────────────────────────────

    [Command(requiresAuthority = false)]
    void CmdSendChat(string message, NetworkConnectionToClient sender = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        message = message.Trim();
        if (message.Length > MAX_MSG_LEN)
            message = message[..MAX_MSG_LEN];

        if (GmCommandRouter.TryHandle(message, sender, this))
            return;

        // Pull username from server-authoritative auth data — cannot be spoofed
        string username = "Unknown";
        if (sender?.authenticationData is RodPlayerAuth auth)
            username = auth.username;

        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Debug.Log($"[CHAT] {username}: {message}");
        RpcReceiveChat(username, message, ts);
    }

    [Command(requiresAuthority = false)]
    void CmdRequestGmState(NetworkConnectionToClient sender = null)
    {
        if (sender == null)
            return;

        RodPlayerAuth auth = sender.authenticationData as RodPlayerAuth;
        TargetSetGmMode(sender, GmCommandRouter.IsActiveGm(auth));
    }

    [Command(requiresAuthority = false)]
    void CmdRequestGmMapTravel(
        string sceneName,
        string arrivalSpawnId,
        NetworkConnectionToClient sender = null)
    {
        if (sender == null)
            return;

        RodPlayerAuth auth = sender.authenticationData as RodPlayerAuth;
        if (!GmCommandRouter.IsActiveGm(auth))
        {
            TargetRejectGmMapTravel(sender, "GM mode is OFF. Use /gm on first.");
            return;
        }

        sceneName = sceneName?.Trim();
        arrivalSpawnId = arrivalSpawnId?.Trim();
        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            TargetRejectGmMapTravel(sender, $"{sceneName ?? "That destination"} is not in Build Settings.");
            return;
        }

        if (ZoneManager.Instance == null)
        {
            TargetRejectGmMapTravel(sender, "Zone system is not running. Try again shortly.");
            return;
        }

        SendGmFeedback(sender, $"GM traveling to {sceneName}...");
        Debug.Log($"[GM] {auth.username} requested map travel: {sceneName}/{arrivalSpawnId}");
        ZoneManager.Instance.MovePlayerToZone(sender, sceneName, arrivalSpawnId);
    }

    [TargetRpc]
    public void TargetBeginZoneTravel(
        NetworkConnectionToClient target, string destinationLabel)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        ZoneCameraDirector.BeginTravel(destinationLabel);
        LoadingScreen.Show(destinationLabel);
#endif
    }

    [TargetRpc]
    public void TargetCompleteZoneTravel(
        NetworkConnectionToClient target, string destinationScene)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        // This completion signal also covers same-zone travel and server-cached
        // additive zones, neither of which is guaranteed to raise sceneLoaded.
        ZoneCameraDirector.RefreshNow(destinationScene);
        WaypointMapTrigger.NotifyTravelComplete();
        NPCInteractionManager.Instance?.CompleteZoneTravel();
        QuestLocalDialogue.CompleteZoneTravel();
        LoadingScreen.NotifyEnvironmentReady();
#endif
        _gmMapTravelPending = false;
    }

    // ── Client-side receive ───────────────────────────────────────────────

    [ClientRpc]
    void RpcReceiveChat(string username, string message, long unixTs)
    {
        var time     = DateTimeOffset.FromUnixTimeSeconds(unixTs).ToLocalTime();
        string tStr  = time.ToString("HH:mm");
        string nCol  = NameColor(username);

        string line =
            $"<color=#64748b>[{tStr}]</color> " +
            $"<color={nCol}><b>{Esc(username)}</b></color>" +
            $"<color=#94a3b8>: </color>" +
            $"<color=#e2e8f0>{Esc(message)}</color>";

        PushLine(line);
    }

    // ── Public helpers ────────────────────────────────────────────────────

    /// <summary>Show a local-only system message (join, leave, etc.).</summary>
    public void AddSystemMessage(string msg) =>
        PushLine($"<color=#60a5fa>» </color><color=#94a3b8>{Esc(msg)}</color>");

    // ── Private ───────────────────────────────────────────────────────────

    [Server]
    public void SendGmFeedback(NetworkConnectionToClient target, string message)
    {
        if (target == null)
        {
            Debug.Log($"[GM] {message}");
            return;
        }

        TargetReceiveGmMessage(target, message);
    }

    [TargetRpc]
    public void TargetReceiveGmMessage(NetworkConnectionToClient target, string message)
    {
        PushLine($"<color=#fbbf24>GM</color><color=#94a3b8>: </color><color=#e2e8f0>{Esc(message)}</color>");
    }

    [TargetRpc]
    public void TargetSetGmMode(NetworkConnectionToClient target, bool enabled)
    {
        _gmModeActive = enabled;
        if (enabled)
            return;

        _gmFreeCameraActive = false;
        ApplyGmFreeCameraState();
        _gmMapTravelPending = false;
#if UNITY_EDITOR || !UNITY_SERVER
        WaypointMapUI.Hide();
#endif
    }

    [TargetRpc]
    public void TargetSetGmFly(NetworkConnectionToClient target, bool enabled)
    {
        _gmFlyActive = enabled;
        ApplyGmMovementState();
    }

    [TargetRpc]
    public void TargetSetGmSpeed(NetworkConnectionToClient target, float multiplier)
    {
        _gmSpeedMultiplier = Mathf.Clamp(multiplier, 0.25f, 8f);
        ApplyGmMovementState();
    }

    [TargetRpc]
    public void TargetSetGmFreeCamera(
        NetworkConnectionToClient target,
        bool enabled,
        float moveSpeed)
    {
        _gmFreeCameraActive = enabled;
        _gmFreeCameraSpeed = Mathf.Clamp(
            moveSpeed,
            RodPlayerAuth.MinFreeCameraSpeed,
            RodPlayerAuth.MaxFreeCameraSpeed);
        ApplyGmFreeCameraState();
    }

    [TargetRpc]
    void TargetRejectGmMapTravel(NetworkConnectionToClient target, string message)
    {
        _gmMapTravelPending = false;
        AddSystemMessage(message);
#if UNITY_EDITOR || !UNITY_SERVER
        WaypointMapUI.SetStatus(message);
        NPCInteractionManager.Instance?.CompleteZoneTravel();
        QuestLocalDialogue.CompleteZoneTravel();
        LoadingScreen.Hide();
#endif
    }

#if UNITY_EDITOR || !UNITY_SERVER
    void ToggleGmMap()
    {
        if (WaypointMapUI.IsVisible)
        {
            WaypointMapUI.Hide();
            return;
        }

        _gmMapTravelPending = false;
        WaypointMapTrigger.ShowForGm(HandleGmMapNodeSelected);
    }

    void HandleGmMapNodeSelected(WaypointMapNode node)
    {
        if (_gmMapTravelPending || node == null)
            return;

        if (!node.CanTravel)
        {
            WaypointMapUI.SetStatus($"{node.displayName} is not available yet.");
            return;
        }

        _gmMapTravelPending = true;
        WaypointMapUI.SetStatus($"GM traveling to {node.displayName}...");
        LoadingScreen.Show(node.displayName);
        CmdRequestGmMapTravel(node.sceneName, node.arrivalSpawnId);
        WaypointMapUI.Hide();
    }
#endif

    void OpenInput()
    {
        _open      = true;
        _typedText = "";
        _inputArea.SetActive(true);
        _panel.SetActive(true);
        _cg.alpha          = 1f;
        _cg.interactable   = true;
        _cg.blocksRaycasts = true;
        _input.text = "";

        // Capture text via Input System directly — bypasses TMP's EventSystem dependency
        // so typing works regardless of which Input Module the EventSystem uses.
        Keyboard.current.onTextInput -= OnTextInput; // guard against double-subscribe
        Keyboard.current.onTextInput += OnTextInput;
    }

    void OnTextInput(char c)
    {
        if (!_open) return;
        if (c < 32 || c == 127) return;              // skip control characters
        if (_typedText.Length >= MAX_MSG_LEN) return;
        _typedText  += c;
        _input.text  = _typedText;
    }

    void CloseInput(bool send)
    {
        Keyboard.current.onTextInput -= OnTextInput;

        if (send)
        {
            string txt = _typedText.Trim();
            if (!string.IsNullOrEmpty(txt))
                CmdSendChat(txt);
        }
        _typedText = "";
        _open      = false;
        _inputArea.SetActive(false);
        _lastMsgTime = Time.unscaledTime;
    }

    void PushLine(string formatted)
    {
        _history.Add(formatted);
        if (_history.Count > MAX_MESSAGES) _history.RemoveAt(0);

        if (_log != null)
        {
            _log.text = string.Join("\n", _history);
            _lastMsgTime = Time.unscaledTime;

            if (!_panel.activeSelf) _panel.SetActive(true);
        }
    }

    bool AnyOtherInputFocused()
    {
        foreach (var f in FindObjectsByType<TMP_InputField>())
            if (f != _input && f.isFocused) return true;
        return false;
    }

    // Prevent TMP rich-text injection from user input.
    // Replace angle brackets so tags like <color=...> can't be smuggled in.
    static string Esc(string s) =>
        s.Replace("<", "[").Replace(">", "]");

    static string NameColor(string name)
    {
        int h = 0;
        foreach (char c in name) h = h * 31 + c;
        return NAME_COLORS[Math.Abs(h) % NAME_COLORS.Length];
    }

    // ── Procedural UI ─────────────────────────────────────────────────────
    // Layout (fraction of 1920×1080):
    //   Panel    — bottom left, rows 4%→30% of screen height, cols 1%→35% of screen width (aligned with abilities)
    //   Log      — fills panel body
    //   InputBg  — bottom 13% of panel, hidden when not typing
    // ─────────────────────────────────────────────────────────────────────

    void ResetGmPlayerCache()
    {
        _gmLocalPlayer = null;
        _gmMovement = null;
        _gmRigidbody = null;
        _gmBaseMoveSpeed = 0f;
        _gmBaseSprintSpeed = 0f;
    }

    void UpdateGmFreeCamera()
    {
        if (!_gmFreeCameraActive)
            return;

        if (_gmFreeCamera == null || !_gmFreeCamera.isActiveAndEnabled ||
            !_gmFreeCamera.IsFreeCameraActive)
            ApplyGmFreeCameraState();
    }

    void ApplyGmFreeCameraState()
    {
        if (!_gmFreeCameraActive && _gmFreeCamera != null &&
            _gmFreeCamera.IsFreeCameraActive)
            _gmFreeCamera.SetFreeCameraEnabled(false);

        CameraFollow follow = FindActiveCameraFollow();
        if (follow == null)
            return;

        if (_gmFreeCamera != null && _gmFreeCamera != follow &&
            _gmFreeCamera.IsFreeCameraActive)
            _gmFreeCamera.SetFreeCameraEnabled(false);

        _gmFreeCamera = follow;
        _gmFreeCamera.SetFreeCameraSpeed(_gmFreeCameraSpeed);
        _gmFreeCamera.SetFreeCameraEnabled(_gmFreeCameraActive);
    }

    static CameraFollow FindActiveCameraFollow()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null &&
            mainCamera.TryGetComponent(out CameraFollow mainFollow))
            return mainFollow;

        foreach (CameraFollow follow in FindObjectsByType<CameraFollow>(
                     FindObjectsInactive.Exclude))
        {
            if (follow != null && follow.isActiveAndEnabled)
                return follow;
        }

        return null;
    }

    void UpdateGmMovement()
    {
        if (!_gmFlyActive && Mathf.Approximately(_gmSpeedMultiplier, 1f))
            return;

        if (!TryCacheGmLocalPlayer())
            return;

        if (!Mathf.Approximately(_gmSpeedMultiplier, 1f))
            ApplyGmSpeed();

        // TargetSetGmFly can arrive before the local player finishes spawning.
        // Apply the state as soon as the player controller becomes available.
        if (_gmFlyActive && _gmMovement != null && !_gmMovement.GmFlightEnabled)
            _gmMovement.SetGmFlightEnabled(true);
    }

    void ApplyGmMovementState()
    {
        if (!TryCacheGmLocalPlayer())
            return;

        ApplyGmSpeed();

        if (_gmMovement != null)
        {
            _gmMovement.SetGmFlightEnabled(_gmFlyActive);
            return;
        }

        if (_gmRigidbody == null)
            return;

        _gmRigidbody.useGravity = !_gmFlyActive;
        _gmRigidbody.linearVelocity = Vector3.zero;
    }

    void ApplyGmSpeed()
    {
        if (_gmMovement == null)
            return;

        if (_gmBaseMoveSpeed <= 0f)
            _gmBaseMoveSpeed = _gmMovement.moveSpeed;
        if (_gmBaseSprintSpeed <= 0f)
            _gmBaseSprintSpeed = _gmMovement.sprintSpeed;

        _gmMovement.moveSpeed = _gmBaseMoveSpeed * _gmSpeedMultiplier;
        _gmMovement.sprintSpeed = _gmBaseSprintSpeed * _gmSpeedMultiplier;
    }

    bool TryCacheGmLocalPlayer()
    {
        if (_gmLocalPlayer != null)
            return true;

        var identities = FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude);
        foreach (var identity in identities)
        {
            if (identity != null && identity.isLocalPlayer)
                return CacheGmPlayer(identity.gameObject);
        }

        if (!NetworkClient.active && !NetworkServer.active)
        {
            GameObject taggedPlayer = GameObject.FindWithTag("Player");
            if (taggedPlayer != null)
                return CacheGmPlayer(taggedPlayer);
        }

        return false;
    }

    bool CacheGmPlayer(GameObject player)
    {
        if (player == null)
            return false;

        _gmLocalPlayer = player;
        _gmMovement = player.GetComponent<PlayerMovement>();
        _gmRigidbody = player.GetComponent<Rigidbody>();

        if (_gmMovement != null)
        {
            _gmBaseMoveSpeed = _gmMovement.moveSpeed;
            _gmBaseSprintSpeed = _gmMovement.sprintSpeed;
        }

        return true;
    }

    // Resolve a usable TMP font once. Relying on the implicit default font asset can
    // yield invisible text (visible box, no glyphs) when TMP_Settings.defaultFontAsset
    // isn't wired — assign this explicitly to every text object we create.
    static TMP_FontAsset s_font;
    static TMP_FontAsset ChatFont
    {
        get
        {
            if (s_font != null) return s_font;
            s_font = TMP_Settings.defaultFontAsset;
            if (s_font == null)
                s_font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (s_font == null)
            {
                // Last resort: adopt the font from any TMP text already alive in the scene.
                var any = FindAnyObjectByType<TMP_Text>();
                if (any != null) s_font = any.font;
            }
            return s_font;
        }
    }

    void BuildUI()
    {
        // ── Canvas ────────────────────────────────────────────────────────
        var cgo = new GameObject("ChatCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        // Mark persistent so the overlay survives ServerChangeScene (the ChatManager
        // GO already has DontDestroyOnLoad, but the canvas is a separate object).
        DontDestroyOnLoad(cgo);
        _canvas = cgo.GetComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        _cg = cgo.GetComponent<CanvasGroup>();
        _cg.alpha          = 0f;
        _cg.blocksRaycasts = false;   // invisible → don't block mouse input
        _cg.interactable   = false;

        var root = _canvas.GetComponent<RectTransform>();

        // ── Panel ─────────────────────────────────────────────────────────
        _panel = MakeRect("ChatPanel", root,
            new Vector2(0.01f, 0.04f), new Vector2(0.35f, 0.30f));
        Img(_panel, new Color(0.02f, 0.02f, 0.06f, 0.88f));

        // ── Header ────────────────────────────────────────────────────────
        var header = MakeRect("Header", _panel.GetComponent<RectTransform>(),
            new Vector2(0f, 0.93f), new Vector2(1f, 1f));
        Img(header, new Color(0.05f, 0.03f, 0.14f, 1f));
        var titleLbl = MakeTmp("Title", header.GetComponent<RectTransform>(),
            new Vector2(0.01f, 0f), new Vector2(1f, 1f));
        titleLbl.text      = "CHAT  <size=8><color=#475569>Enter/T to type · Esc to close</color></size>";
        titleLbl.fontSize  = 10f;
        titleLbl.color     = new Color(0.5f, 0.8f, 1f);
        titleLbl.fontStyle = FontStyles.Bold;
        titleLbl.alignment = TextAlignmentOptions.MidlineLeft;

        // ── Log text (plain fixed rect — no ScrollRect, no Mask/Viewport) ───
        var logGO = MakeRect("LogText", _panel.GetComponent<RectTransform>(),
            new Vector2(0f, 0.13f), new Vector2(1f, 0.93f),
            new Vector2(6f, 4f), new Vector2(-6f, -4f));
        _log = logGO.AddComponent<TextMeshProUGUI>();
        if (ChatFont != null) _log.font = ChatFont;
        _log.fontSize         = 11f;
        _log.color            = Color.white;
        _log.alignment        = TextAlignmentOptions.TopLeft;
        _log.textWrappingMode = TextWrappingModes.Normal;
        _log.richText         = true;
        _log.overflowMode     = TextOverflowModes.Overflow;

        // ── Input area ────────────────────────────────────────────────────
        _inputArea = MakeRect("InputArea", _panel.GetComponent<RectTransform>(),
            new Vector2(0f, 0f), new Vector2(1f, 0.13f));
        Img(_inputArea, new Color(0.04f, 0.03f, 0.12f, 1f));

        var promptLbl = MakeTmp("Prompt", _inputArea.GetComponent<RectTransform>(),
            new Vector2(0.01f, 0f), new Vector2(0.06f, 1f));
        promptLbl.text      = ">";
        promptLbl.fontSize  = 13f;
        promptLbl.color     = new Color(0.5f, 0.8f, 1f);
        promptLbl.alignment = TextAlignmentOptions.MidlineLeft;

        var inputGO = MakeRect("InputField", _inputArea.GetComponent<RectTransform>(),
            new Vector2(0.06f, 0f), new Vector2(1f, 1f));
        Img(inputGO, Color.clear);

        var phGO = MakeRect("Placeholder", inputGO.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one);
        var ph = phGO.AddComponent<TextMeshProUGUI>();
        if (ChatFont != null) ph.font = ChatFont;
        ph.text      = "Say something...";
        ph.fontSize  = 13f;
        ph.color     = new Color(0.3f, 0.27f, 0.4f);
        ph.fontStyle = FontStyles.Italic;

        var txtGO = MakeRect("Text", inputGO.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one);
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        if (ChatFont != null) txt.font = ChatFont;
        txt.fontSize = 13f;
        txt.color    = Color.white;

        _input = inputGO.AddComponent<TMP_InputField>();
        _input.textComponent  = txt;
        _input.placeholder    = ph;
        _input.textViewport   = inputGO.GetComponent<RectTransform>();
        _input.caretColor     = new Color(0.5f, 0.8f, 1f);
        _input.characterLimit = MAX_MSG_LEN;
        // Enter is handled in Update() — onSubmit is unreliable with new Input System.

        _inputArea.SetActive(false);
        _panel.SetActive(false);
    }

    // ── UI helpers ────────────────────────────────────────────────────────

    static GameObject MakeRect(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin = default, Vector2 offsetMax = default)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return go;
    }

    static void Img(GameObject go, Color col)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = col;
    }

    static TextMeshProUGUI MakeTmp(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = MakeRect(name, parent, anchorMin, anchorMax);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (ChatFont != null) t.font = ChatFont;
        return t;
    }
}
