using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ═══════════════════════════════════════════════════════════════════════════
//  EscMenu
//  Press Escape → pause overlay with Resume / Logout / Quit.
//  Attach to a persistent GameObject (e.g. the NetworkManager object).
//  Safe on dedicated server — bails out early if headless.
// ═══════════════════════════════════════════════════════════════════════════

public class EscMenu : MonoBehaviour
{
    // ── Auto-bootstrap ────────────────────────────────────────────────────
    // Creates itself once on game start — no manual prefab wiring needed.
    static EscMenu _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return; // headless server — skip
        if (_instance != null) return;

        var go = new GameObject("EscMenu");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<EscMenu>();
    }


    // ── UI ────────────────────────────────────────────────────────────────
    Canvas      _canvas;
    GameObject  _panel;
    GameObject  _mainView;
    GameObject  _optionsView;
    Slider      _musicSlider;
    Slider      _sfxSlider;
    Slider      _zoomSlider;
    Slider      _interfaceScaleSlider;
    TextMeshProUGUI _musicValueLabel;
    TextMeshProUGUI _sfxValueLabel;
    TextMeshProUGUI _zoomValueLabel;
    TextMeshProUGUI _interfaceScaleValueLabel;
    TextMeshProUGUI _variantModeValueLabel;
    bool        _open;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        // Never run on headless dedicated server
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            enabled = false;
            return;
        }
        BuildUI();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (!kb.escapeKey.wasPressedThisFrame) return;

        // Free camera owns Escape so it can restore the hidden UI and return to
        // the player without briefly opening the pause menu underneath it.
        if (CameraFollow.IsAnyFreeCameraActive)
            return;

        // If chat is open, let RodChatManager handle Escape (closes chat).
        // Don't open the ESC menu at the same time.
        if (RodChatManager.Instance != null && RodChatManager.Instance.IsOpen)
            return;

        // If a chat / input field is focused via EventSystem, first ESC defocuses it.
        var es  = UnityEngine.EventSystems.EventSystem.current;
        var sel = es?.currentSelectedGameObject;
        if (sel != null && sel.GetComponent<TMP_InputField>() != null)
        {
            es.SetSelectedGameObject(null);
            return;
        }

        if (_open && _optionsView != null && _optionsView.activeSelf)
        {
            ShowMainMenu();
            return;
        }

        SetOpen(!_open);
    }

    // ── Actions ───────────────────────────────────────────────────────────

    void Resume()   => SetOpen(false);

    void ShowOptions()
    {
        RefreshMusicSlider();
        RefreshSfxSlider();
        RefreshZoomSlider();
        RefreshInterfaceScaleSlider();
        RefreshVariantModeLabel();
        _mainView.SetActive(false);
        _optionsView.SetActive(true);
    }

    void ShowMainMenu()
    {
        _optionsView.SetActive(false);
        _mainView.SetActive(true);
    }

    void ChangeCharacter()
    {
        // LoginScene briefly rebuilds the persistent NetworkManager before its
        // one-shot redirect to CharacterSelect. Keep the loading overlay above both
        // scene loads so the intermediate login UI is never presented to the player.
#if !UNITY_SERVER
        LoadingScreen.Show("Character Selection");
#endif
        SetOpen(false);

        // Re-enable cursor before the scene transition so character select isn't mouse-locked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Return to CharacterSelect WITHOUT clearing the session — the player keeps their
        // JWT and picks a different class without re-entering credentials. RodNetworkManager
        // redirects offlineScene for this one disconnect; do NOT also LoadScene here.
        if (NetworkManager.singleton is RodNetworkManager rod)
        {
            rod.ReturnToCharacterSelect();
        }
        else
        {
            // Preserve the same one-shot redirect when a scene has a base Mirror
            // NetworkManager instead of RodNetworkManager. LoginScene rebuilds the
            // correct manager, then LoginManager forwards to CharacterSelect.
            Debug.LogWarning("[EscMenu] NetworkManager is not RodNetworkManager — using generic disconnect fallback.");
            RodNetworkManager.PendingChangeCharacter = true;

            if (NetworkServer.active && NetworkClient.isConnected)
                NetworkManager.singleton.StopHost();
            else if (NetworkClient.isConnected || NetworkClient.active)
                NetworkManager.singleton.StopClient();
            else if (NetworkServer.active)
                NetworkManager.singleton.StopServer();
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.Login);
        }
    }

    void Logout()
    {
        // Logout means leave the active character, not exit the account or game.
        // Keep the authenticated session and return to class/character selection.
        ChangeCharacter();
    }

    void Quit()
    {
        SetOpen(false);
#if !UNITY_SERVER
        LoadingScreen.Show("Saving Character");
#endif

        if (NetworkManager.singleton is RodNetworkManager rod)
            rod.QuitAfterSavingPosition();
        else
        {
            // A non-game scene has no active character position to preserve.
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void SetOpen(bool open)
    {
        // Unity can report carried-over Input System actions as enabled even after
        // they have stopped delivering pointer events. Rebind them whenever this
        // explicit modal is opened so its controls always receive clicks.
        if (open)
            SingleEventSystem.ForceRebuild();

        _open = open;
        _panel.SetActive(open);

        // Enable/disable canvas raycasting with the panel
        var cg = _canvas.GetComponent<CanvasGroup>();
        if (cg != null) { cg.blocksRaycasts = open; cg.interactable = open; }

        if (open)
        {
            ShowMainMenu();
            RefreshMusicSlider();
            RefreshSfxSlider();
            RefreshZoomSlider();
            RefreshInterfaceScaleSlider();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    // ── Procedural UI ─────────────────────────────────────────────────────

    void BuildUI()
    {
        // Canvas
        var cgo = new GameObject("EscMenuCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        DontDestroyOnLoad(cgo);

        _canvas = cgo.GetComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        // Above every ordinary persistent HUD/console canvas, but below the loading
        // screen (9999). This prevents invisible higher-order UI from taking clicks.
        _canvas.sortingOrder = 3000;

        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        var root = _canvas.GetComponent<RectTransform>();

        // _panel is the full-screen container — hiding it removes EVERYTHING including
        // the overlay, so nothing blocks clicks when the menu is closed.
        _panel = MakeRect("EscPanel", root, Vector2.zero, Vector2.one);

        // Dark screen overlay (child of _panel, so hidden with it)
        var overlay = MakeRect("Overlay", _panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        Img(overlay, new Color(0f, 0f, 0f, 0.6f));

        // Centred card (also child of _panel)
        var card = MakeRect("Card", _panel.GetComponent<RectTransform>(),
            new Vector2(0.34f, 0.16f), new Vector2(0.66f, 0.84f));
        Img(card, new Color(0.04f, 0.03f, 0.12f, 0.97f));
        var cardRt = card.GetComponent<RectTransform>();

        // Title
        var title = MakeTmp("Title", cardRt,
            new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.95f));
        title.text      = "MENU";
        title.fontSize  = 22f;
        title.color     = new Color(0.5f, 0.8f, 1f);
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;

        // Divider
        var div = MakeRect("Divider", cardRt,
            new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.77f));
        Img(div, new Color(0.2f, 0.2f, 0.35f, 1f));

        _mainView = MakeRect("MainView", cardRt, Vector2.zero, Vector2.one);
        var mainRt = _mainView.GetComponent<RectTransform>();

        _optionsView = MakeRect("OptionsView", cardRt, Vector2.zero, Vector2.one);
        var optionsRt = _optionsView.GetComponent<RectTransform>();

        // Buttons — five stacked entries, evenly spaced within the card.
        MakeButton("Resume",   mainRt, new Vector2(0.1f, 0.60f), new Vector2(0.9f, 0.71f),
            new Color(0.08f, 0.5f, 0.18f), Resume);

        MakeButton("Change Character", mainRt, new Vector2(0.1f, 0.47f), new Vector2(0.9f, 0.58f),
            new Color(0.20f, 0.15f, 0.45f), ChangeCharacter);

        MakeButton("Options", mainRt, new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.45f),
            new Color(0.08f, 0.25f, 0.5f), ShowOptions);

        MakeButton("Logout",   mainRt, new Vector2(0.1f, 0.21f), new Vector2(0.9f, 0.32f),
            new Color(0.45f, 0.35f, 0.05f), Logout);

        MakeButton("Quit Game", mainRt, new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.19f),
            new Color(0.5f, 0.05f, 0.05f), Quit);

        BuildOptionsView(optionsRt);
        _optionsView.SetActive(false);
        _panel.SetActive(false);

        // Belt-and-suspenders: also disable raycasts on the canvas itself when hidden
        var cg = cgo.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable   = false;
    }

    void BuildOptionsView(RectTransform parent)
    {
        var header = MakeTmp("OptionsHeader", parent,
            new Vector2(0.1f, 0.84f), new Vector2(0.9f, 0.91f));
        header.text      = "OPTIONS";
        header.fontSize  = 16f;
        header.color     = new Color(0.5f, 0.8f, 1f);
        header.fontStyle = FontStyles.Bold;
        header.alignment = TextAlignmentOptions.Center;

        var musicLabel = MakeTmp("MusicVolumeLabel", parent,
            new Vector2(0.1f, 0.72f), new Vector2(0.58f, 0.77f));
        musicLabel.text      = "MUSIC";
        musicLabel.fontSize  = 13f;
        musicLabel.color     = Color.white;
        musicLabel.fontStyle = FontStyles.Bold;
        musicLabel.alignment = TextAlignmentOptions.MidlineLeft;

        _musicValueLabel = MakeTmp("MusicVolumeValue", parent,
            new Vector2(0.6f, 0.72f), new Vector2(0.9f, 0.77f));
        _musicValueLabel.fontSize  = 13f;
        _musicValueLabel.color     = new Color(0.75f, 0.9f, 1f);
        _musicValueLabel.fontStyle = FontStyles.Bold;
        _musicValueLabel.alignment = TextAlignmentOptions.MidlineRight;

        _musicSlider = MakeSlider("MusicVolumeSlider", parent,
            new Vector2(0.1f, 0.67f), new Vector2(0.9f, 0.71f));
        _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        var sfxLabel = MakeTmp("SfxVolumeLabel", parent,
            new Vector2(0.1f, 0.57f), new Vector2(0.58f, 0.62f));
        sfxLabel.text      = "SFX";
        sfxLabel.fontSize  = 13f;
        sfxLabel.color     = Color.white;
        sfxLabel.fontStyle = FontStyles.Bold;
        sfxLabel.alignment = TextAlignmentOptions.MidlineLeft;

        _sfxValueLabel = MakeTmp("SfxVolumeValue", parent,
            new Vector2(0.6f, 0.57f), new Vector2(0.9f, 0.62f));
        _sfxValueLabel.fontSize  = 13f;
        _sfxValueLabel.color     = new Color(0.75f, 0.9f, 1f);
        _sfxValueLabel.fontStyle = FontStyles.Bold;
        _sfxValueLabel.alignment = TextAlignmentOptions.MidlineRight;

        _sfxSlider = MakeSlider("SfxVolumeSlider", parent,
            new Vector2(0.1f, 0.52f), new Vector2(0.9f, 0.56f));
        _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

        var zoomLabel = MakeTmp("CameraZoomLabel", parent,
            new Vector2(0.1f, 0.42f), new Vector2(0.58f, 0.47f));
        zoomLabel.text      = "CAMERA ZOOM";
        zoomLabel.fontSize  = 13f;
        zoomLabel.color     = Color.white;
        zoomLabel.fontStyle = FontStyles.Bold;
        zoomLabel.alignment = TextAlignmentOptions.MidlineLeft;

        _zoomValueLabel = MakeTmp("CameraZoomValue", parent,
            new Vector2(0.6f, 0.42f), new Vector2(0.9f, 0.47f));
        _zoomValueLabel.fontSize  = 13f;
        _zoomValueLabel.color     = new Color(0.75f, 0.9f, 1f);
        _zoomValueLabel.fontStyle = FontStyles.Bold;
        _zoomValueLabel.alignment = TextAlignmentOptions.MidlineRight;

        _zoomSlider = MakeSlider("CameraZoomSlider", parent,
            new Vector2(0.1f, 0.37f), new Vector2(0.9f, 0.41f));
        _zoomSlider.onValueChanged.AddListener(OnCameraZoomChanged);

        var interfaceScaleLabel = MakeTmp("InterfaceScaleLabel", parent,
            new Vector2(0.1f, 0.27f), new Vector2(0.58f, 0.32f));
        interfaceScaleLabel.text      = "INTERFACE SCALE";
        interfaceScaleLabel.fontSize  = 13f;
        interfaceScaleLabel.color     = Color.white;
        interfaceScaleLabel.fontStyle = FontStyles.Bold;
        interfaceScaleLabel.alignment = TextAlignmentOptions.MidlineLeft;

        _interfaceScaleValueLabel = MakeTmp("InterfaceScaleValue", parent,
            new Vector2(0.6f, 0.27f), new Vector2(0.9f, 0.32f));
        _interfaceScaleValueLabel.fontSize  = 13f;
        _interfaceScaleValueLabel.color     = new Color(0.75f, 0.9f, 1f);
        _interfaceScaleValueLabel.fontStyle = FontStyles.Bold;
        _interfaceScaleValueLabel.alignment = TextAlignmentOptions.MidlineRight;

        _interfaceScaleSlider = MakeSlider("InterfaceScaleSlider", parent,
            new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.26f));
        _interfaceScaleSlider.onValueChanged.AddListener(OnInterfaceScaleChanged);

        // Variant zone selector mode
        var variantLabel = MakeTmp("VariantModeLabel", parent,
            new Vector2(0.1f, 0.13f), new Vector2(0.58f, 0.18f));
        variantLabel.text      = "ABILITY ZONE";
        variantLabel.fontSize  = 13f;
        variantLabel.color     = Color.white;
        variantLabel.fontStyle = FontStyles.Bold;
        variantLabel.alignment = TextAlignmentOptions.MidlineLeft;

        // Clickable value — toggles between MOUSE and SCROLL WHEEL
        var variantValueGo = MakeRect("VariantModeValueBtn", parent,
            new Vector2(0.58f, 0.13f), new Vector2(0.9f, 0.18f));
        _variantModeValueLabel = variantValueGo.AddComponent<TextMeshProUGUI>();
        _variantModeValueLabel.fontSize  = 13f;
        _variantModeValueLabel.color     = new Color(0.75f, 0.9f, 1f);
        _variantModeValueLabel.fontStyle = FontStyles.Bold;
        _variantModeValueLabel.alignment = TextAlignmentOptions.MidlineRight;
        var variantBtn = variantValueGo.AddComponent<Button>();
        variantBtn.onClick.AddListener(ToggleVariantMode);

        MakeButton("Back", parent, new Vector2(0.1f, 0.03f), new Vector2(0.9f, 0.10f),
            new Color(0.16f, 0.16f, 0.26f), ShowMainMenu);

        RefreshMusicSlider();
        RefreshSfxSlider();
        RefreshZoomSlider();
        RefreshInterfaceScaleSlider();
        RefreshVariantModeLabel();
    }

    void RefreshMusicSlider()
    {
        if (_musicSlider == null) return;

        float volume = MusicController.Instance != null
            ? MusicController.Instance.Volume
            : PlayerPrefs.GetFloat(MusicController.VolumePrefKey, 0.6f);

        _musicSlider.SetValueWithoutNotify(Mathf.Clamp01(volume));
        UpdateMusicValueLabel(volume);
    }

    void OnMusicVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);

        if (MusicController.Instance != null)
        {
            MusicController.Instance.SetMuted(value <= 0.001f);
            MusicController.Instance.SetVolume(value);
            MusicController.Instance.SavePreferences();
        }
        else
        {
            PlayerPrefs.SetFloat(MusicController.VolumePrefKey, value);
            PlayerPrefs.SetInt(MusicController.MutedPrefKey, value <= 0.001f ? 1 : 0);
            PlayerPrefs.Save();
        }

        UpdateMusicValueLabel(value);
    }

    void UpdateMusicValueLabel(float value)
    {
        if (_musicValueLabel != null)
            _musicValueLabel.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
    }

    void RefreshSfxSlider()
    {
        if (_sfxSlider == null) return;
        float volume = SfxVolumeSettings.Volume;
        _sfxSlider.SetValueWithoutNotify(volume);
        UpdateSfxValueLabel(volume);
    }

    void OnSfxVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        SfxVolumeSettings.SetVolume(value);
        UpdateSfxValueLabel(value);
    }

    void UpdateSfxValueLabel(float value)
    {
        if (_sfxValueLabel != null)
            _sfxValueLabel.text = $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
    }

    void RefreshZoomSlider()
    {
        if (_zoomSlider == null) return;

        CameraFollow follow = FindCameraFollow();
        float min = follow != null ? follow.minDistance : 1.5f;
        float max = follow != null ? follow.maxDistance : 20f;
        float distance = follow != null
            ? follow.distance
            : PlayerPrefs.GetFloat(CameraFollow.ZoomDistancePrefKey, CameraFollow.DefaultZoomDistance);

        distance = Mathf.Clamp(distance, min, max);
        _zoomSlider.SetValueWithoutNotify(Mathf.InverseLerp(min, max, distance));
        UpdateZoomValueLabel(distance);
    }

    void OnCameraZoomChanged(float value)
    {
        CameraFollow follow = FindCameraFollow();
        float distance;

        if (follow != null)
        {
            follow.SetZoomNormalized(value);
            distance = follow.distance;
        }
        else
        {
            distance = Mathf.Lerp(1.5f, 20f, Mathf.Clamp01(value));
            PlayerPrefs.SetFloat(CameraFollow.ZoomDistancePrefKey, distance);
            PlayerPrefs.Save();
        }

        UpdateZoomValueLabel(distance);
    }

    void UpdateZoomValueLabel(float distance)
    {
        if (_zoomValueLabel != null)
            _zoomValueLabel.text = $"{Mathf.Max(0f, distance):0.0}m";
    }

    static CameraFollow FindCameraFollow()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            CameraFollow follow = cam.GetComponent<CameraFollow>();
            if (follow != null)
                return follow;
        }

        return FindAnyObjectByType<CameraFollow>();
    }

    void RefreshInterfaceScaleSlider()
    {
        if (_interfaceScaleSlider == null) return;
        float scale = InterfaceScaleSettings.Scale;
        _interfaceScaleSlider.SetValueWithoutNotify(Mathf.InverseLerp(
            InterfaceScaleSettings.Minimum, InterfaceScaleSettings.Maximum, scale));
        UpdateInterfaceScaleValueLabel(scale);
    }

    void OnInterfaceScaleChanged(float normalized)
    {
        float scale = Mathf.Lerp(InterfaceScaleSettings.Minimum,
            InterfaceScaleSettings.Maximum, Mathf.Clamp01(normalized));
        scale = Mathf.Round(scale * 20f) / 20f;
        InterfaceScaleSettings.SetScale(scale);
        UpdateInterfaceScaleValueLabel(scale);
    }

    void UpdateInterfaceScaleValueLabel(float scale)
    {
        if (_interfaceScaleValueLabel != null)
            _interfaceScaleValueLabel.text = $"{Mathf.RoundToInt(scale * 100f)}%";
    }

    void ToggleVariantMode()
    {
        bool current = PlayerPrefs.GetInt("VariantScrollMode", 0) == 1;
        bool next = !current;
        PlayerPrefs.SetInt("VariantScrollMode", next ? 1 : 0);
        PlayerPrefs.Save();
        RefreshVariantModeLabel();

        // Apply immediately to the local player's AbilityCaster if in-game
        var caster = FindAnyObjectByType<AbilityCaster>();
        if (caster != null)
            caster.useScrollWheelVariants = next;
    }

    void RefreshVariantModeLabel()
    {
        if (_variantModeValueLabel == null) return;
        bool scroll = PlayerPrefs.GetInt("VariantScrollMode", 0) == 1;
        _variantModeValueLabel.text = scroll ? "SCROLL WHEEL" : "MOUSE DISTANCE";
    }

    // ── UI helpers ────────────────────────────────────────────────────────

    static GameObject MakeRect(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
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
        return go.AddComponent<TextMeshProUGUI>();
    }

    void MakeButton(string label, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Color bgColor, UnityEngine.Events.UnityAction onClick)
    {
        var go = MakeRect(label + "Btn", parent, anchorMin, anchorMax);
        Img(go, bgColor);

        var txt = MakeTmp(label + "Lbl", go.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one);
        txt.text      = label.ToUpper();
        txt.fontSize  = 14f;
        txt.color     = Color.white;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = bgColor;
        colors.highlightedColor = bgColor * 1.35f;
        colors.pressedColor     = bgColor * 0.7f;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);
    }

    Slider MakeSlider(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = MakeRect(name, parent, anchorMin, anchorMax);
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.02f, 0.07f, 1f);

        var fillArea = MakeRect("FillArea", go.GetComponent<RectTransform>(),
            new Vector2(0.02f, 0.25f), new Vector2(0.98f, 0.75f));
        var fill = MakeRect("Fill", fillArea.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one);
        Img(fill, new Color(0.2f, 0.65f, 1f, 1f));

        var handleArea = MakeRect("HandleArea", go.GetComponent<RectTransform>(),
            new Vector2(0.02f, 0f), new Vector2(0.98f, 1f));
        var handle = MakeRect("Handle", handleArea.GetComponent<RectTransform>(),
            new Vector2(0f, 0.05f), new Vector2(0.06f, 0.95f));
        Img(handle, new Color(0.9f, 0.95f, 1f, 1f));

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.transition = Selectable.Transition.ColorTint;
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }
}
