using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// PortalTransition — attach to each portal in Hub scene.
/// Server: first player to enter triggers scene load for all clients.
/// Client: shows "Press E to enter" prompt when in range.
///
/// Setup (BCE/Build Hub Scene auto-adds portals — add this component + set arenaSceneName):
///   1. Attach to portal GameObject (needs a trigger Collider)
///   2. Set arenaSceneName in inspector (e.g. "Arena_Copper")
///   3. The portal must have a trigger SphereCollider on it
///
/// The arena scene must be added to Build Settings.
/// </summary>
public class PortalTransition : NetworkBehaviour
{
    [Header("Portal Config")]
    [Tooltip("Scene name exactly as it appears in Build Settings")]
    public string arenaSceneName = "Arena_Copper";
    [Tooltip("Shown in the prompt and chat announcement")]
    public string portalDisplayName = "Copper Arena";
    [Tooltip("Seconds before scene loads after trigger — lets the prompt show")]
    public float transitionDelay = 1.5f;

    [Header("Prompt UI")]
    [Tooltip("Distance at which the E prompt appears (client-side)")]
    public float promptRadius = 4f;

    private bool          _triggered;
    private Transform     _localPlayer;
    private GameObject    _promptObj;
    private TextMeshProUGUI _promptText;
    private bool          _promptVisible;

    // ── Server ────────────────────────────────────────────────────────────────

    [Server]
    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        RpcAnnounce($"A portal to {portalDisplayName} has opened! Warping all players...");
        Invoke(nameof(LoadArena), transitionDelay);
    }

    [Server]
    void LoadArena()
    {
        Debug.Log($"[PORTAL] Loading arena scene: {arenaSceneName}");
        NetworkManager.singleton.ServerChangeScene(arenaSceneName);
    }

    // ── Client ────────────────────────────────────────────────────────────────

    public override void OnStartClient()
    {
        base.OnStartClient();
        BuildPromptUI();
    }

    void Update()
    {
        if (isServer) return; // server doesn't need the prompt
        if (_localPlayer == null) FindLocalPlayer();
        if (_localPlayer == null) return;

        float dist = Vector3.Distance(transform.position, _localPlayer.position);
        bool inRange = dist <= promptRadius;

        if (inRange != _promptVisible)
        {
            _promptVisible = inRange;
            if (_promptObj != null) _promptObj.SetActive(inRange);
        }

        // E to enter — sends a Cmd so the server handles the transition
        if (inRange && UnityEngine.InputSystem.Keyboard.current?.eKey.wasPressedThisFrame == true)
            CmdRequestEnter();
    }

    [Command(requiresAuthority = false)]
    void CmdRequestEnter()
    {
        if (_triggered) return;
        _triggered = true;
        RpcAnnounce($"A portal to {portalDisplayName} has opened! Warping all players...");
        Invoke(nameof(LoadArena), transitionDelay);
    }

    [ClientRpc]
    void RpcAnnounce(string message)
    {
        Debug.Log($"[PORTAL] {message}");
        RodChatManager.Instance?.AddSystemMessage(message);
    }

    void FindLocalPlayer()
    {
        foreach (var id in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (id.isLocalPlayer) { _localPlayer = id.transform; return; }
        }
    }

    // ── Prompt UI (world-space billboard) ─────────────────────────────────────
    void BuildPromptUI()
    {
        _promptObj = new GameObject("PortalPrompt");
        _promptObj.transform.SetParent(transform, false);
        _promptObj.transform.localPosition = new Vector3(0f, 3.5f, 0f);
        _promptObj.AddComponent<RodBillboard>(); // always faces camera

        var canvas = _promptObj.AddComponent<Canvas>();
        canvas.renderMode    = RenderMode.WorldSpace;
        canvas.sortingOrder  = 10;
        var rt = _promptObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220f, 50f);
        rt.localScale = Vector3.one * 0.012f;

        var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(_promptObj.transform, false);
        bg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(_promptObj.transform, false);
        _promptText = textGO.GetComponent<TextMeshProUGUI>();
        _promptText.text      = $"<color=#fbbf24>[E]</color> Enter {portalDisplayName}";
        _promptText.fontSize  = 14f;
        _promptText.alignment = TextAlignmentOptions.Center;
        _promptText.color     = Color.white;
        var tRt = textGO.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
        tRt.offsetMin = tRt.offsetMax = Vector2.zero;

        _promptObj.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, promptRadius);
    }
}
