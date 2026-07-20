using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// PortalTransition — attach to each portal in Hub scene.
/// Each player enters independently; only the triggering client loads the scene.
/// ServerChangeScene is intentionally NOT used here — that would move all players.
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
    }

    [Command(requiresAuthority = false)]
    void CmdRequestEnter(NetworkConnectionToClient sender = null)
    {
        if (sender == null) return;

        int connId = sender.connectionId;
        if (_entered.Contains(connId)) return;
        _entered.Add(connId);

        TargetBeginTransition(sender, portalDisplayName, arenaSceneName, transitionDelay);
    }

    /// <summary>Tells only the triggering client to load the scene.</summary>
    [TargetRpc]
    void TargetBeginTransition(NetworkConnectionToClient target, string displayName, string sceneName, float delay)
    {
        if (_enteringLocally) return;
        _enteringLocally = true;

        RodChatManager.Instance?.AddSystemMessage($"Entering {displayName}...");
        if (_promptObj != null) _promptObj.SetActive(false);

        StartCoroutine(LocalLoad(sceneName, delay));
    }

    IEnumerator LocalLoad(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
#if UNITY_EDITOR || !UNITY_SERVER
        LoadingScreen.Show(sceneName);
#endif
        SceneManager.LoadScene(sceneName);
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

        float dist = Vector3.Distance(transform.position, _localPlayer.position);
        bool inRange = dist <= promptRadius;

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
        foreach (var id in FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (id.isLocalPlayer) { _localPlayer = id.transform; return; }
        }
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
