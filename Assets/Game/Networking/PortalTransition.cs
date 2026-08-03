using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// PortalTransition — attach to each portal in Hub scene.
/// Each player enters independently.
///
/// ROADMAP 6.4: this used to fake per-player travel with a client-local
/// SceneManager.LoadScene that never told the server — so the client stood in the
/// arena while the server still had their identity in Hub, observing Hub objects.
/// Travel now goes through ZoneManager, which moves the player server-side and
/// sends that one client an additive SceneMessage.
/// </summary>
public class PortalTransition : NetworkBehaviour
{
    [Header("Portal Config")]
    [Tooltip("Scene name exactly as it appears in Build Settings")]
    public string arenaSceneName = SceneNames.ArenaCopper;
    [Tooltip("Shown in the prompt and chat announcement")]
    public string portalDisplayName = "Copper Arena";
    [Tooltip("Seconds before scene loads after trigger — lets the prompt show")]
    public float transitionDelay = 1.5f;

    [Header("Prompt UI")]
    [Tooltip("Distance at which the E prompt appears (client-side)")]
    public float promptRadius = 4f;

    // Per-connection set so each player can enter independently
    private readonly HashSet<int> _entered = new HashSet<int>();

    private Transform        _localPlayer;
    private GameObject       _promptObj;
    private TextMeshProUGUI  _promptText;
    private bool             _promptVisible;
    private bool             _enteringLocally;   // client-side: prevents double-trigger

    // ── Server ────────────────────────────────────────────────────────────────

    [Server]
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var identity = other.GetComponentInParent<NetworkIdentity>();
        if (identity == null) return;

        var conn = identity.connectionToClient;
        if (conn == null) return;

        int connId = conn.connectionId;
        if (_entered.Contains(connId)) return;
        _entered.Add(connId);

        TargetBeginTransition(conn, portalDisplayName, arenaSceneName, transitionDelay);
        StartCoroutine(BeginTransition(conn, arenaSceneName, transitionDelay));
    }

    [Command(requiresAuthority = false)]
    void CmdRequestEnter(NetworkConnectionToClient sender = null)
    {
        if (sender == null) return;

        int connId = sender.connectionId;
        if (_entered.Contains(connId)) return;
        _entered.Add(connId);

        TargetBeginTransition(sender, portalDisplayName, arenaSceneName, transitionDelay);
        StartCoroutine(BeginTransition(sender, arenaSceneName, transitionDelay));
    }

    /// <summary>
    /// Server-side: waits out the transition delay so the client can show its
    /// prompt, then hands the player to ZoneManager.
    /// </summary>
    [Server]
    IEnumerator BeginTransition(NetworkConnectionToClient conn, string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);

        // They may have disconnected during the delay.
        if (conn == null || !NetworkServer.connections.ContainsKey(conn.connectionId))
        {
            _entered.Remove(conn?.connectionId ?? -1);
            yield break;
        }

        if (ZoneManager.Instance == null)
        {
            Debug.LogError("[Portal] ZoneManager missing — cannot enter. Run BCE ▶ Setup ▶ 6z.");
            _entered.Remove(conn.connectionId);
            yield break;
        }

        ZoneManager.Instance.MovePlayerToZone(conn, sceneName);

        // Release the re-entry guard. Before 6.4 travel never actually worked, so a
        // permanent entry meant nothing; now a player who comes back to Hub must be
        // able to use the same portal again.
        _entered.Remove(conn.connectionId);
    }

    /// <summary>Client-side cosmetics only — the actual move is server-authoritative.</summary>
    [TargetRpc]
    void TargetBeginTransition(NetworkConnectionToClient target, string displayName, string sceneName, float delay)
    {
        if (_enteringLocally) return;
        _enteringLocally = true;

        RodChatManager.Instance?.AddSystemMessage($"Entering {displayName}...");
        if (_promptObj != null) _promptObj.SetActive(false);

#if UNITY_EDITOR || !UNITY_SERVER
        LoadingScreen.Show(sceneName);
#endif
        StartCoroutine(ClearLocalEntryGuard(delay + 2f));
    }

    /// <summary>
    /// Re-arms the local prompt after travel so returning to Hub and taking the same
    /// portal again works. The client has no completion signal from the additive load,
    /// so this is time-based on purpose.
    /// </summary>
    IEnumerator ClearLocalEntryGuard(float delay)
    {
        yield return new WaitForSeconds(delay);
        _enteringLocally = false;
    }

    // ── Client ────────────────────────────────────────────────────────────────

    public override void OnStartClient()
    {
        base.OnStartClient();
        BuildPromptUI();
    }

    void Update()
    {
        if (isServer) return;
        if (_enteringLocally) return;
        if (_localPlayer == null) FindLocalPlayer();
        if (_localPlayer == null) return;

        bool inRange = (transform.position - _localPlayer.position).sqrMagnitude <= promptRadius * promptRadius;

        if (inRange != _promptVisible)
        {
            _promptVisible = inRange;
            if (_promptObj != null) _promptObj.SetActive(inRange);
        }

        if (inRange && UnityEngine.InputSystem.Keyboard.current?.eKey.wasPressedThisFrame == true)
            CmdRequestEnter();
    }

    void FindLocalPlayer()
    {
        if (PlayerIdentity.Local != null)
            _localPlayer = PlayerIdentity.Local.transform;
    }

    // ── Prompt UI (world-space billboard) ────────────────────────────────────

    void BuildPromptUI()
    {
        _promptObj = new GameObject("PortalPrompt");
        _promptObj.transform.SetParent(transform, false);
        _promptObj.transform.localPosition = new Vector3(0f, 3.5f, 0f);
        _promptObj.AddComponent<RodBillboard>();

        var canvas = _promptObj.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;
        var rt = _promptObj.GetComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(220f, 50f);
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
