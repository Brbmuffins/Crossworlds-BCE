using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// NPCInteractionManager — Singleton. Manages all hub NPC interactions.
/// Tracks which NPC the local player is near, shows/hides the E-key prompt,
/// and routes E-key presses to the correct NPC.
///
/// Copy to: Assets/Game/Systems/NPCInteractionManager.cs
/// Self-bootstrapping — no scene setup or Inspector wiring required.
/// Auto-disables in Arena scene.
///
/// NPCs register themselves via NPCInteractionManager.Register(this)
/// and unregister on destroy via NPCInteractionManager.Unregister(this).
/// </summary>
public class NPCInteractionManager : MonoBehaviour
{
    public static NPCInteractionManager Instance { get; private set; }

    private const string ARENA_SCENE = "Arena";
    private const float  PROMPT_CHECK_INTERVAL = 0.15f;   // seconds between proximity checks

    // The interface every interactable NPC must implement
    public interface IHubNPC
    {
        Transform transform { get; }
        float InteractRadius { get; }
        void Interact();
    }

    private readonly System.Collections.Generic.List<IHubNPC> _npcs
        = new System.Collections.Generic.List<IHubNPC>();

    private IHubNPC  _nearestNPC;
    private bool     _promptVisible;

#if !UNITY_SERVER
    private Canvas     _promptCanvas;
    private TMPro.TextMeshProUGUI _promptLabel;
#endif

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[NPCInteractionManager]");
        DontDestroyOnLoad(go);
        go.AddComponent<NPCInteractionManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
#if !UNITY_SERVER
        BuildPromptUI();
#endif
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(ProximityLoop());
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ─── NPC Registration ─────────────────────────────────────────────────────
    public static void Register(IHubNPC npc)
    {
        if (Instance == null) return;
        if (!Instance._npcs.Contains(npc))
            Instance._npcs.Add(npc);
    }

    public static void Unregister(IHubNPC npc)
    {
        if (Instance == null) return;
        Instance._npcs.Remove(npc);
        if (Instance._nearestNPC == npc)
        {
            Instance._nearestNPC = null;
            Instance.HidePrompt();
        }
    }

    // ─── Scene Change ─────────────────────────────────────────────────────────
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool inArena = scene.name == ARENA_SCENE;
#if !UNITY_SERVER
        if (_promptCanvas != null)
            _promptCanvas.gameObject.SetActive(!inArena);
#endif
        if (inArena) HidePrompt();
    }

    // ─── Proximity Loop ───────────────────────────────────────────────────────
    IEnumerator ProximityLoop()
    {
        var wait = new WaitForSeconds(PROMPT_CHECK_INTERVAL);
        while (true)
        {
            yield return wait;

            // Disable in Arena
            if (SceneManager.GetActiveScene().name == ARENA_SCENE)
            {
                HidePrompt();
                continue;
            }

#if !UNITY_SERVER
            var localPlayer = Mirror.NetworkClient.localPlayer;
            if (localPlayer == null) { HidePrompt(); continue; }

            Transform playerT = localPlayer.transform;
            IHubNPC nearest  = null;
            float   nearDist  = float.MaxValue;

            foreach (var npc in _npcs)
            {
                if (npc == null || npc.transform == null) continue;
                float d = Vector3.Distance(playerT.position, npc.transform.position);
                if (d <= npc.InteractRadius && d < nearDist)
                {
                    nearDist = d;
                    nearest  = npc;
                }
            }

            if (nearest != _nearestNPC)
            {
                _nearestNPC = nearest;
                if (nearest != null) ShowPrompt("[E] Interact");
                else                 HidePrompt();
            }
#endif
        }
    }

    // ─── E Key Handling ───────────────────────────────────────────────────────
#if !UNITY_SERVER
    void Update()
    {
        if (SceneManager.GetActiveScene().name == ARENA_SCENE) return;
        if (_nearestNPC == null) return;
        if (UnityEngine.InputSystem.Keyboard.current?.eKey.wasPressedThisFrame == true)
            _nearestNPC.Interact();
    }
#endif

    // ─── Prompt UI ────────────────────────────────────────────────────────────
#if !UNITY_SERVER
    void BuildPromptUI()
    {
        var canvasGo = new GameObject("NPCPromptCanvas");
        canvasGo.transform.SetParent(transform);
        _promptCanvas = canvasGo.AddComponent<Canvas>();
        _promptCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _promptCanvas.sortingOrder = 10;
        canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode
            = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var labelGo = new GameObject("PromptLabel");
        labelGo.transform.SetParent(canvasGo.transform, false);
        _promptLabel = labelGo.AddComponent<TMPro.TextMeshProUGUI>();
        _promptLabel.fontSize  = 18f;
        _promptLabel.color     = Color.white;
        _promptLabel.alignment = TMPro.TextAlignmentOptions.Center;

        var rt = labelGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.3f);
        rt.anchorMax = new Vector2(0.5f, 0.3f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300f, 40f);
        rt.anchoredPosition = Vector2.zero;

        canvasGo.SetActive(false);
    }

    void ShowPrompt(string text)
    {
        if (_promptCanvas == null) return;
        _promptLabel.text = text;
        _promptCanvas.gameObject.SetActive(true);
        _promptVisible = true;
    }

    void HidePrompt()
    {
        if (_promptCanvas == null) return;
        _promptCanvas.gameObject.SetActive(false);
        _promptVisible = false;
    }
#else
    void ShowPrompt(string text) { }
    void HidePrompt() { }
#endif
}
