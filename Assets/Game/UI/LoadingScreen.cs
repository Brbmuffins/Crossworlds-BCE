#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Mirror;

/// <summary>
/// Full-screen loading overlay that hides the scene-load gap.
/// Bootstraps itself — no scene object needed.
///
/// Usage:
///   LoadingScreen.Show("Hub");          // before SceneManager.LoadScene
///   LoadingScreen.Hide();               // explicit hide (optional — auto-hides too)
/// </summary>
public sealed class LoadingScreen : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    static LoadingScreen _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[LoadingScreen]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<LoadingScreen>();
        _instance.Build();
        _instance.SetVisible(false);
    }

    // ── Config ────────────────────────────────────────────────────────────────
    const float FadeOutDuration   = 0.4f;
    const float PhysicsSettleTime = 0.6f; // seconds after sceneLoaded before fade starts
    const float EnvironmentSettleTime = 2.5f; // additive skybox/volume activation

    // ── State ─────────────────────────────────────────────────────────────────
    Canvas          _canvas;
    CanvasGroup     _group;
    TextMeshProUGUI _sceneLabel;
    TextMeshProUGUI _dotLabel;
    RawImage        _backgroundImage;
    AspectRatioFitter _backgroundFitter;
    bool            _visible;
    Coroutine       _autoHideCo;
    Coroutine       _dotCo;
    string          _pendingLabel;

    static readonly Dictionary<string, string> BackgroundsByDestination =
        new Dictionary<string, string>
        {
            { "hub", "hub" },
            { "darkwood", "darkwood" },
            { "toujambasin", "toujam-basin" },
            { "boneyard", "boneyard" },
            { "ashenwasteland", "ashen-wastelands" },
            { "ashenwastelands", "ashen-wastelands" },
            { "gatheringzone", "gathering-zone" },
            { "arenacopper", "arena-copper" },
            { "copperarena", "arena-copper" },
            { "pvpzone", "pvp-zone" },
            { "gmisland", "gm-island" },
            { "voiddungeon", "void-dungeon" },
        };

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Show the loading screen. Call immediately before LoadScene.</summary>
    public static void Show(string label = "")
    {
        if (_instance == null) Bootstrap();
        _instance.DoShow(label);
    }

    /// <summary>Hide immediately (skips auto-hide timer). Usually not needed.</summary>
    public static void Hide()
    {
        if (_instance == null) return;
        _instance.DoHide();
    }

    /// <summary>
    /// Restarts the settle countdown when the destination environment is actually
    /// ready. Also handles host travel to an already server-cached zone, where no
    /// sceneLoaded callback is raised and the old auto-hide path would wait forever.
    /// </summary>
    public static void NotifyEnvironmentReady()
    {
        if (_instance == null || !_instance._visible) return;
        _instance.RestartAutoHide();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_instance == this) _instance = null;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    void DoShow(string label)
    {
        bool wasVisible = _visible;
        if (_autoHideCo != null) { StopCoroutine(_autoHideCo); _autoHideCo = null; }

        // Scene-context windows must not survive a zone change merely because their
        // bootstrap object is persistent. Closing them here covers portals, map
        // travel, GM travel, and any future caller that uses the loading overlay.
        QuestLocalDialogue.BeginZoneTravel();
        NPCInteractionManager.Instance?.BeginZoneTravel();
        HangmanDialogueUI.Instance?.Hide();
        WaypointMapUI.Hide();

        // Keep the source-zone music audible beneath the newly visible overlay,
        // then fade it to silence before the server begins loading the destination.
        if (!wasVisible)
            MusicController.Instance?.Stop(MusicController.TravelFadeOutSeconds);

        _pendingLabel = label;
        ApplyBackground(label);
        if (_sceneLabel != null)
            _sceneLabel.text = string.IsNullOrEmpty(label) ? "Loading..." : label;

        SetVisible(true);
        _group.alpha = 1f;

        if (_dotCo != null) StopCoroutine(_dotCo);
        _dotCo = StartCoroutine(AnimateDots());
    }

    void DoHide()
    {
        if (_autoHideCo != null) { StopCoroutine(_autoHideCo); _autoHideCo = null; }
        if (_dotCo != null)      { StopCoroutine(_dotCo);      _dotCo = null; }
        StartCoroutine(FadeOut());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-hide after physics has had time to bake colliders and settle rigidbodies
        if (!_visible) return;
        RestartAutoHide();
    }

    void RestartAutoHide()
    {
        if (_autoHideCo != null) StopCoroutine(_autoHideCo);
        _autoHideCo = StartCoroutine(AutoHideAfterSettle());
    }

    IEnumerator AutoHideAfterSettle()
    {
        // Freeze the local player's rigidbody so they can't fall through unloaded geometry
        Rigidbody playerRb = null;
        float waitStart = Time.unscaledTime;

        // Wait up to 2s for the local player to spawn
        while (Time.unscaledTime - waitStart < 2f)
        {
            playerRb = FindLocalPlayerRigidbody();
            if (playerRb != null) break;
            yield return null;
        }

        if (playerRb != null)
            playerRb.isKinematic = true;

        // Wait for physics/terrain to settle
        yield return new WaitForSeconds(PhysicsSettleTime);

        // Additively-loaded scenes share global RenderSettings. Unity can finish
        // skybox, ambient probe and URP volume activation a few frames after the
        // sceneLoaded callback. Keep that bright-to-dark setup hidden rather than
        // exposing the destination before its intended presentation is stable.
        yield return new WaitForSecondsRealtime(EnvironmentSettleTime);

        // Prepare first-use terrain/material and configured spell rendering while
        // this client is still fully covered by the loading overlay.
        yield return ZoneVisualPreloader.PrewarmLoadedZone();
        PlayerIdentity.Local?.ReportZoneVisualsReady();

        // Re-enable physics
        if (playerRb != null)
            playerRb.isKinematic = false;

        DoHide();
    }

    IEnumerator FadeOut()
    {
        if (_dotCo != null) { StopCoroutine(_dotCo); _dotCo = null; }

        float t = 0f;
        while (t < FadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(1f, 0f, t / FadeOutDuration);
            yield return null;
        }

        SetVisible(false);
    }

    IEnumerator AnimateDots()
    {
        string[] frames = { ".", "..", "..." };
        int i = 0;
        while (true)
        {
            if (_dotLabel != null) _dotLabel.text = frames[i % frames.Length];
            i++;
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }

    void SetVisible(bool on)
    {
        _visible = on;
        if (_canvas != null) _canvas.gameObject.SetActive(on);
        if (_group  != null) _group.alpha = on ? 1f : 0f;
    }

    static Rigidbody FindLocalPlayerRigidbody()
    {
        foreach (var id in FindObjectsByType<NetworkIdentity>(
                     FindObjectsInactive.Exclude))
        {
            if (id.isLocalPlayer)
                return id.GetComponent<Rigidbody>();
        }
        return null;
    }

    void ApplyBackground(string destinationLabel)
    {
        if (_backgroundImage == null)
            return;

        Texture2D texture = null;
        string key = NormalizeDestination(destinationLabel);
        if (BackgroundsByDestination.TryGetValue(key, out string resourceName))
            texture = Resources.Load<Texture2D>($"LoadingScreens/{resourceName}");

        _backgroundImage.texture = texture;
        _backgroundImage.color = texture != null
            ? Color.white
            : new Color(0.05f, 0.05f, 0.08f, 1f);

        if (_backgroundFitter != null)
            _backgroundFitter.aspectRatio = texture != null && texture.height > 0
                ? (float)texture.width / texture.height
                : 16f / 9f;
    }

    static string NormalizeDestination(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return string.Empty;

        var key = new StringBuilder(label.Length);
        foreach (char character in label)
            if (char.IsLetterOrDigit(character))
                key.Append(char.ToLowerInvariant(character));

        return key.ToString();
    }

    // ── Build UI ──────────────────────────────────────────────────────────────

    void Build()
    {
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9999;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        _group = gameObject.AddComponent<CanvasGroup>();
        _group.interactable   = false;
        _group.blocksRaycasts = true;

        // Destination artwork fills the screen while preserving its cinematic aspect.
        var bg = new GameObject("BG", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
        bg.transform.SetParent(transform, false);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = bgRt.anchorMax = new Vector2(0.5f, 0.5f);
        bgRt.anchoredPosition = Vector2.zero;
        _backgroundImage = bg.GetComponent<RawImage>();
        _backgroundImage.color = new Color(0.05f, 0.05f, 0.08f, 1f);
        _backgroundFitter = bg.GetComponent<AspectRatioFitter>();
        _backgroundFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        _backgroundFitter.aspectRatio = 16f / 9f;

        // A restrained veil keeps the destination label readable over bright scenes.
        var veil = new GameObject("ArtworkVeil", typeof(RectTransform), typeof(Image));
        veil.transform.SetParent(transform, false);
        var veilRt = veil.GetComponent<RectTransform>();
        veilRt.anchorMin = Vector2.zero;
        veilRt.anchorMax = Vector2.one;
        veilRt.offsetMin = veilRt.offsetMax = Vector2.zero;
        veil.GetComponent<Image>().color = new Color(0.015f, 0.01f, 0.025f, 0.26f);

        // Scene / destination label
        var labelGO = new GameObject("SceneLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(transform, false);
        var labelRt = labelGO.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0.2f, 0.45f);
        labelRt.anchorMax = new Vector2(0.8f, 0.55f);
        labelRt.offsetMin = labelRt.offsetMax = Vector2.zero;
        _sceneLabel = labelGO.GetComponent<TextMeshProUGUI>();
        _sceneLabel.text      = "Loading...";
        _sceneLabel.alignment = TextAlignmentOptions.Center;
        _sceneLabel.fontSize  = 36f;
        _sceneLabel.color     = new Color(0.9f, 0.85f, 0.7f, 1f);
        _sceneLabel.fontStyle = FontStyles.Bold;

        // Animated dots
        var dotGO = new GameObject("Dots", typeof(RectTransform), typeof(TextMeshProUGUI));
        dotGO.transform.SetParent(transform, false);
        var dotRt = dotGO.GetComponent<RectTransform>();
        dotRt.anchorMin = new Vector2(0.2f, 0.38f);
        dotRt.anchorMax = new Vector2(0.8f, 0.45f);
        dotRt.offsetMin = dotRt.offsetMax = Vector2.zero;
        _dotLabel = dotGO.GetComponent<TextMeshProUGUI>();
        _dotLabel.text      = ".";
        _dotLabel.alignment = TextAlignmentOptions.Center;
        _dotLabel.fontSize  = 24f;
        _dotLabel.color     = new Color(0.6f, 0.6f, 0.8f, 1f);
    }
}
#endif
