using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// PlayerHUD — fully self-building, self-bootstrapping HUD.
///
/// Builds at runtime:
///   • HP bar (bottom-left) with shield glow + damage flash
///   • 4-slot ability bar (bottom-centre) with icon, keybind, cooldown sweep
///   • Active-slot gold ring indicator
///   • Shrine-driven spell loadout — grid of all class abilities with icons
///   • Floating damage/heal numbers at world position
///
/// Zero Inspector wiring required. Add this script to any persistent
/// GameObject and it works. Self-bootstrapping via RuntimeInitialize.
///
/// Binds to the local Mirror player on spawn. Gracefully degrades if
/// Health or AbilityCaster aren't present yet.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    public static PlayerHUD Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        // Compiles on every build target (no #if UNITY_SERVER), but never runs on a
        // truly headless server — a real dedicated server has no graphics device.
        // This lets the HUD appear in the editor whether the active build target is
        // Client or Dedicated Server.
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            return;
        if (Instance != null) return;
        var go = new GameObject("[PlayerHUD]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<PlayerHUD>();
    }

    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color BgDark      = new Color(0.06f, 0.06f, 0.10f, 0.92f);
    static readonly Color BgMid       = new Color(0.10f, 0.10f, 0.16f, 0.88f);
    static readonly Color SlotNormal  = new Color(0.12f, 0.12f, 0.18f, 0.90f);
    static readonly Color SlotActive  = new Color(0.95f, 0.80f, 0.15f, 1.00f);
    static readonly Color CooldownCol = new Color(0.00f, 0.00f, 0.00f, 0.72f);
    static readonly Color HpFull      = new Color(0.20f, 0.80f, 0.35f, 1.00f);
    static readonly Color HpLow       = new Color(0.85f, 0.18f, 0.12f, 1.00f);
    static readonly Color HealthWellFull = new Color(0.90f, 0.02f, 0.01f, 0.96f);
    static readonly Color HealthWellLow  = new Color(0.55f, 0.00f, 0.00f, 0.96f);
    static readonly Color HealthWellBg   = new Color(0.24f, 0.24f, 0.24f, 0.88f);
    static readonly Color ManaWellFull   = new Color(0.02f, 0.20f, 0.76f, 0.96f);
    static readonly Color ManaWellBg     = new Color(0.20f, 0.22f, 0.28f, 0.88f);
    static readonly Color ShieldCol   = new Color(0.35f, 0.70f, 1.00f, 0.80f);
    static readonly Color DmgColor    = new Color(1.00f, 0.32f, 0.15f, 1.00f);
    static readonly Color HealColor   = new Color(0.22f, 0.90f, 0.45f, 1.00f);
    static readonly Color TextPrimary = new Color(0.95f, 0.93f, 0.88f, 1.00f);
    static readonly Color TextDim     = new Color(0.55f, 0.53f, 0.50f, 1.00f);
    static readonly Color Transparent = new Color(0, 0, 0, 0);
    static readonly Color IconReady   = new Color(1.20f, 1.20f, 1.20f, 1.00f);
    static readonly Color IconCooldown = new Color(0.36f, 0.36f, 0.40f, 1.00f);

    // ── UI state ──────────────────────────────────────────────────────────────
    Canvas      _canvas;
    Canvas      _spellbookCanvas;
    static Sprite _healthWellSprite;
    static Sprite _slotRingSprite;

    // HP bar
    Image       _hpFill;
    Image       _hpBg;
    Image       _shieldFill;
    Image       _actionHealthFill;
    Image       _actionManaFill;
    TextMeshProUGUI _hpLabel;
    float       _displayedHp   = 1f;
    float       _hpFlashTimer  = 0f;

    // Ability bar
    const int Slots = 4;
    const string ActionBarFrameResource = "UI/PlayerHUD";
    // The source artwork has transparent export padding around its 1431x382
    // visible frame. These dimensions preserve the previous on-screen scale,
    // while the negative offset places the visible frame back at screen bottom.
    const float ActionBarFrameWidth = 965f;
    const float ActionBarFrameHeight = 644f;
    const float ActionBarBottomOffset = -219f;
    const float ActionBarContentYOffset = 219f;
    const float ActionBarSlotSize = 104f;
    const float ActionBarSlotCenterY = 110f + ActionBarContentYOffset;
    const float ActionBarSlotStartX = -175f;
    const float ActionBarSlotStepX = 114.5f;
    const float ActionBarNameY = 48f + ActionBarContentYOffset;
    const float ActionBarSlotContentInset = 0f;
    const float ActionBarHealthWellCenterY = 121f + ActionBarContentYOffset;
    const float ActionBarHealthWellCenterX = -340f;
    const float ActionBarHealthWellSize = 164f;
    const float ActionBarManaWellCenterX = 340f;
    const float ActionBarManaWellCenterY = 121f + ActionBarContentYOffset;
    const float ActionBarManaWellSize = 164f;
    Image[]             _slotBg       = new Image[Slots];
    Image[]             _slotIcon     = new Image[Slots];
    Image[]             _slotCooldown = new Image[Slots];
    Image[]             _slotRing     = new Image[Slots];
    TextMeshProUGUI[]   _slotKey      = new TextMeshProUGUI[Slots];
    TextMeshProUGUI[]   _slotName     = new TextMeshProUGUI[Slots];
    TextMeshProUGUI[]   _slotCdText   = new TextMeshProUGUI[Slots];   // live cooldown countdown
    int                 _activeSlot         = 0;
    bool                _wasAimingLastFrame = false;

    // Ability tooltip
    const float AbilityTooltipWidth = 370f;
    const float AbilityTooltipHeight = 176f;
    RectTransform       _abilityTooltipRoot;
    TextMeshProUGUI     _abilityTooltipName;
    TextMeshProUGUI     _abilityTooltipStats;
    TextMeshProUGUI     _abilityTooltipDescription;
    int                 _abilityTooltipSlot = -1;

    // Cast bar
    GameObject          _castBarRoot;
    Image               _castBarFill;
    RectTransform       _castBarFillRect;
    TextMeshProUGUI     _castBarName;
    TextMeshProUGUI     _castBarTime;

    // Spellbook
    GameObject          _spellbookPanel;
    bool                _spellbookOpen = false;
    SpellLoadoutShrine  _activeLoadoutShrine;
    Transform           _spellbookGridParent;
    ScrollRect          _spellScroll;
    TextMeshProUGUI     _spellbookInstructions;
    readonly Image[]    _loadoutSlotBg = new Image[Slots];
    readonly Image[]    _loadoutSlotIcon = new Image[Slots];
    readonly TextMeshProUGUI[] _loadoutSlotName =
        new TextMeshProUGUI[Slots];
    readonly Dictionary<int, Image> _spellCardBackgrounds = new();
    int                 _pendingSpellIdx = -1;
    int                 _pendingLoadoutSlot = -1;

    public static bool IsSpellLoadoutOpen =>
        Instance != null && Instance._spellbookOpen;

    // Floating numbers
    Canvas      _floatCanvas;

    // ── Player refs ───────────────────────────────────────────────────────────
    Health          _health;
    CharacterStats  _stats;
    AbilityCaster   _caster;
    float           _scanTimer;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        { enabled = false; return; }
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildAllUI();
    }

    void Update()
    {
        ScanForPlayer();

        // The HUD only belongs in gameplay scenes. Hide it on the menus (Login /
        // CharacterSelect) so it neither renders nor eats UI raycasts there — but
        // show it in gameplay scenes even before a player has fully bound, so it
        // never silently disappears. Ticks below already null-check _health/_caster.
        string scene = SceneManager.GetActiveScene().name;
        bool menuScene = scene == "Login" || scene == "LoginScene" || scene == "CharacterSelect";
        bool show = !menuScene;
        // Use SetActive (not canvas.enabled) so the GraphicRaycaster deregisters and
        // cannot intercept clicks on menu-scene canvases with a higher sortingOrder.
        if (_canvas      != null && _canvas.gameObject.activeSelf      != show) _canvas.gameObject.SetActive(show);
        if (_floatCanvas != null && _floatCanvas.gameObject.activeSelf != show) _floatCanvas.gameObject.SetActive(show);
        if (!show)
        {
            HideAbilityTooltip();
            CloseSpellLoadout();
            return;
        }

        TickHpBar();
        TickManaWell();
        TickAbilityBar();
        TickCastBar();
        TickAbilityTooltip();
        TickSpellbook();
    }

    // ── Player scan ───────────────────────────────────────────────────────────

    void ScanForPlayer()
    {
        if (_health) return;  // Unity null check catches destroyed objects
        _scanTimer -= Time.deltaTime;
        if (_scanTimer > 0f) return;
        _scanTimer = 0.5f;

        bool networkActive = NetworkClient.active || NetworkServer.active;

        foreach (var ni in FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude))
        {
            bool isLocal = networkActive ? ni.isLocalPlayer : (ni.CompareTag("Player") || ni.GetComponent<PlayerMovement>() != null);
            if (!isLocal) continue;
            BindPlayer(ni.gameObject);
            break;
        }
    }

    void BindPlayer(GameObject player)
    {
        // Unsubscribe from old player if we're rebinding (e.g. after respawn)
        if (_health) _health.onHealthChanged.RemoveListener(OnHealthChanged);
        if (_stats) _stats.onManaChanged.RemoveListener(OnManaChanged);
        if (_caster) _caster.SpellEquipResult -= OnSpellEquipResult;

        _health = player.GetComponent<Health>();
        _stats = player.GetComponent<CharacterStats>();
        _caster = player.GetComponent<AbilityCaster>();
        if (_caster) _caster.SpellEquipResult += OnSpellEquipResult;

        if (_health)
        {
            _health.onHealthChanged.AddListener(OnHealthChanged);
            _displayedHp = _health.Fraction;
            UpdateHpLabel(_health.currentHealth, _health.maxHealth);
        }

        if (_stats)
        {
            _stats.onManaChanged.AddListener(OnManaChanged);
            OnManaChanged(_stats.CurrentMana, _stats.MaxMana);
        }
        else
        {
            SetManaWellFraction(1f);
        }

        RebuildAbilitySlots();
        RebuildSpellbook();
    }

    void OnDestroy()
    {
        if (_health) _health.onHealthChanged.RemoveListener(OnHealthChanged);
        if (_stats) _stats.onManaChanged.RemoveListener(OnManaChanged);
        if (_caster) _caster.SpellEquipResult -= OnSpellEquipResult;
    }

    void OnHealthChanged(float current, float max)
    {
        if (max <= 0f) return;
        float newFrac = current / max;
        if (newFrac < _displayedHp) _hpFlashTimer = 0.25f; // damage flash
        _displayedHp = newFrac;
        UpdateHpLabel(current, max);
    }

    void OnManaChanged(float current, float max)
    {
        SetManaWellFraction(max > 0f ? current / max : 0f);
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    void BuildAllUI()
    {
        _canvas = MakeCanvas(100);
        BuildHpBar();
        BuildAbilityBar();
        BuildCastBar();
        BuildAbilityTooltip();

        _floatCanvas  = MakeCanvas(110);
        _spellbookCanvas = MakeCanvas(120);
        _spellbookPanel  = BuildSpellbookPanel();
        _spellbookCanvas.gameObject.SetActive(false);
    }

    Canvas MakeCanvas(int order)
    {
        var go = new GameObject("HUDCanvas_" + order); go.transform.SetParent(transform, false);
        var c  = go.AddComponent<Canvas>();
        c.renderMode  = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = order;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution  = new Vector2(1920, 1080);
        cs.matchWidthOrHeight   = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return c;
    }

    // ── HP bar ────────────────────────────────────────────────────────────────

    void BuildHpBar()
    {
        var root = Rt(_canvas.transform, "HpRoot");
        root.anchorMin = new Vector2(0.01f, 0.93f);
        root.anchorMax = new Vector2(0.10f, 0.97f);
        root.offsetMin = root.offsetMax = Vector2.zero;

        // Panel
        var panel = Img(root, "HpPanel", BgDark);
        Stretch(panel.rectTransform);

        // HP BG (dark red)
        _hpBg = Img(root, "HpBg", new Color(0.22f, 0.05f, 0.05f, 1f));
        _hpBg.rectTransform.anchorMin = new Vector2(0.03f, 0.15f);
        _hpBg.rectTransform.anchorMax = new Vector2(0.97f, 0.85f);
        _hpBg.rectTransform.offsetMin = _hpBg.rectTransform.offsetMax = Vector2.zero;

        // HP fill
        _hpFill = Img(root, "HpFill", HpFull);
        _hpFill.type       = Image.Type.Filled;
        _hpFill.fillMethod = Image.FillMethod.Horizontal;
        _hpFill.fillAmount = 1f;
        _hpFill.rectTransform.anchorMin = new Vector2(0.03f, 0.15f);
        _hpFill.rectTransform.anchorMax = new Vector2(0.97f, 0.85f);
        _hpFill.rectTransform.offsetMin = _hpFill.rectTransform.offsetMax = Vector2.zero;

        // Shield fill (on top)
        _shieldFill = Img(root, "ShieldFill", ShieldCol);
        _shieldFill.type       = Image.Type.Filled;
        _shieldFill.fillMethod = Image.FillMethod.Horizontal;
        _shieldFill.fillAmount = 0f;
        _shieldFill.rectTransform.anchorMin = new Vector2(0.03f, 0.68f);
        _shieldFill.rectTransform.anchorMax = new Vector2(0.97f, 0.85f);
        _shieldFill.rectTransform.offsetMin = _shieldFill.rectTransform.offsetMax = Vector2.zero;

        // Label
        _hpLabel = Lbl(root, "HpLabel", "HP", 11f);
        _hpLabel.rectTransform.anchorMin = new Vector2(0.03f, 0f);
        _hpLabel.rectTransform.anchorMax = new Vector2(0.97f, 0.18f);
        _hpLabel.rectTransform.offsetMin = _hpLabel.rectTransform.offsetMax = Vector2.zero;
        _hpLabel.alignment = TextAlignmentOptions.Center;
        _hpLabel.color     = TextDim;
    }

    void TickHpBar()
    {
        float target = (_health != null) ? _health.Fraction : 1f;
        _displayedHp = Mathf.MoveTowards(_displayedHp, target, Time.deltaTime * 1.5f);
        _hpFill.fillAmount = _displayedHp;
        _hpFill.color = Color.Lerp(HpLow, HpFull, _displayedHp);
        if (_actionHealthFill != null)
        {
            _actionHealthFill.fillAmount = _displayedHp;
            _actionHealthFill.color = Color.Lerp(HealthWellLow, HealthWellFull, _displayedHp);
        }

        if (_hpFlashTimer > 0f)
        {
            _hpFlashTimer -= Time.deltaTime;
            float t = _hpFlashTimer / 0.25f;
            _hpFill.color = Color.Lerp(_hpFill.color, Color.white, t * 0.6f);
            if (_actionHealthFill != null)
                _actionHealthFill.color = Color.Lerp(_actionHealthFill.color, Color.white, t * 0.35f);
        }

        // Shield
        if (_health != null && _health.HasShield)
        {
            float shieldFrac = Mathf.Clamp01(_health.ShieldRemaining / _health.maxHealth);
            _shieldFill.fillAmount = shieldFrac;
            _shieldFill.color = new Color(ShieldCol.r, ShieldCol.g, ShieldCol.b,
                0.5f + 0.5f * Mathf.PingPong(Time.time * 2f, 1f));
        }
        else
        {
            _shieldFill.fillAmount = 0f;
        }
    }

    void TickManaWell()
    {
        float target = _stats != null ? _stats.ManaFraction : 1f;
        SetManaWellFraction(target);
    }

    void SetManaWellFraction(float fraction)
    {
        if (_actionManaFill == null)
            return;

        _actionManaFill.fillAmount = Mathf.Clamp01(fraction);
        _actionManaFill.color = ManaWellFull;
    }

    void UpdateHpLabel(float current, float max)
    {
        if (_hpLabel != null)
            _hpLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    // ── Ability bar ───────────────────────────────────────────────────────────

    void BuildAbilityBar()
    {
        var root = Rt(_canvas.transform, "AbilityBar");
        root.anchorMin        = new Vector2(0.5f, 0f);
        root.anchorMax        = new Vector2(0.5f, 0f);
        root.pivot            = new Vector2(0.5f, 0f);
        root.anchoredPosition = new Vector2(0f, ActionBarBottomOffset);
        root.sizeDelta        = new Vector2(ActionBarFrameWidth, ActionBarFrameHeight);

        BuildHealthWell(root);
        BuildManaWell(root);

        // Build the ability contents before the artwork so the HUD's decorative
        // borders mask their oversized edges and frame each slot cleanly.
        for (int i = 0; i < Slots; i++)
        {
            float x = ActionBarSlotStartX + i * ActionBarSlotStepX;
            BuildSlot(root, i, x, ActionBarSlotCenterY, ActionBarSlotSize);
        }

        var frame = Img(root, "ActionBarFrame", Color.white);
        frame.sprite = Resources.Load<Sprite>(ActionBarFrameResource);
        frame.preserveAspect = true;
        frame.raycastTarget = false;
        if (frame.sprite == null)
            frame.color = BgDark;
        Stretch(frame.rectTransform);

        // Selection rings must remain above the decorative frame, while the
        // ability icon, backdrop, and cooldown stay masked beneath it.
        for (int i = 0; i < Slots; i++)
        {
            float x = ActionBarSlotStartX + i * ActionBarSlotStepX;
            var ringRt = _slotRing[i].rectTransform;
            ringRt.SetParent(root, false);
            ringRt.anchorMin = new Vector2(0.5f, 0f);
            ringRt.anchorMax = new Vector2(0.5f, 0f);
            ringRt.pivot = new Vector2(0.5f, 0.5f);
            ringRt.anchoredPosition = new Vector2(x, ActionBarSlotCenterY);
            ringRt.sizeDelta = new Vector2(ActionBarSlotSize + 8f, ActionBarSlotSize + 8f);
        }
    }

    void BuildHealthWell(RectTransform parent)
    {
        var wellRt = Rt(parent, "HealthWell");
        wellRt.anchorMin        = new Vector2(0.5f, 0f);
        wellRt.anchorMax        = new Vector2(0.5f, 0f);
        wellRt.pivot            = new Vector2(0.5f, 0.5f);
        wellRt.anchoredPosition = new Vector2(ActionBarHealthWellCenterX, ActionBarHealthWellCenterY);
        wellRt.sizeDelta        = new Vector2(ActionBarHealthWellSize, ActionBarHealthWellSize);

        var bg = Img(wellRt, "Bg", HealthWellBg);
        bg.sprite = HealthWellSprite();
        bg.raycastTarget = false;
        Stretch(bg.rectTransform);

        _actionHealthFill = Img(wellRt, "Fill", HealthWellFull);
        _actionHealthFill.sprite = HealthWellSprite();
        _actionHealthFill.type = Image.Type.Filled;
        _actionHealthFill.fillMethod = Image.FillMethod.Vertical;
        _actionHealthFill.fillOrigin = (int)Image.OriginVertical.Bottom;
        _actionHealthFill.fillAmount = 1f;
        _actionHealthFill.raycastTarget = false;
        Stretch(_actionHealthFill.rectTransform);

    }

    void BuildManaWell(RectTransform parent)
    {
        var wellRt = Rt(parent, "ManaWell");
        wellRt.anchorMin        = new Vector2(0.5f, 0f);
        wellRt.anchorMax        = new Vector2(0.5f, 0f);
        wellRt.pivot            = new Vector2(0.5f, 0.5f);
        wellRt.anchoredPosition = new Vector2(ActionBarManaWellCenterX, ActionBarManaWellCenterY);
        wellRt.sizeDelta        = new Vector2(ActionBarManaWellSize, ActionBarManaWellSize);

        var bg = Img(wellRt, "Bg", ManaWellBg);
        bg.sprite = HealthWellSprite();
        bg.raycastTarget = false;
        Stretch(bg.rectTransform);

        _actionManaFill = Img(wellRt, "Fill", ManaWellFull);
        _actionManaFill.sprite = HealthWellSprite();
        _actionManaFill.type = Image.Type.Filled;
        _actionManaFill.fillMethod = Image.FillMethod.Vertical;
        _actionManaFill.fillOrigin = (int)Image.OriginVertical.Bottom;
        _actionManaFill.fillAmount = 1f;
        _actionManaFill.raycastTarget = false;
        Stretch(_actionManaFill.rectTransform);

    }

    void BuildSlot(RectTransform parent, int i, float x, float y, float size)
    {
        // Container — anchored to bar bottom so slots sit inside the panel
        var slotRt = Rt(parent, "Slot_" + i);
        slotRt.anchorMin        = new Vector2(0.5f, 0f);
        slotRt.anchorMax        = new Vector2(0.5f, 0f);
        slotRt.pivot            = new Vector2(0.5f, 0.5f);
        slotRt.anchoredPosition = new Vector2(x, y);
        slotRt.sizeDelta        = new Vector2(size, size);

        var contentRt = Rt(slotRt, "Content");
        Stretch(contentRt);
        contentRt.offsetMin = new Vector2(ActionBarSlotContentInset, ActionBarSlotContentInset);
        contentRt.offsetMax = new Vector2(-ActionBarSlotContentInset, -ActionBarSlotContentInset);

        // Background
        _slotBg[i] = Img(contentRt, "Bg", SlotNormal);
        Stretch(_slotBg[i].rectTransform);

        // Icon
        _slotIcon[i] = Img(contentRt, "Icon", Transparent);
        _slotIcon[i].preserveAspect = true;
        _slotIcon[i].rectTransform.anchorMin = new Vector2(0.02f, 0.02f);
        _slotIcon[i].rectTransform.anchorMax = new Vector2(0.98f, 0.98f);
        _slotIcon[i].rectTransform.offsetMin = _slotIcon[i].rectTransform.offsetMax = Vector2.zero;

        // Cooldown overlay (filled from top)
        _slotCooldown[i] = Img(contentRt, "Cooldown", Transparent);
        _slotCooldown[i].type       = Image.Type.Filled;
        _slotCooldown[i].fillMethod = Image.FillMethod.Vertical;
        _slotCooldown[i].fillOrigin = (int)Image.OriginVertical.Top;
        _slotCooldown[i].fillAmount = 0f;
        Stretch(_slotCooldown[i].rectTransform);

        // Cooldown countdown number (centred over the icon, hidden when ready)
        _slotCdText[i] = Lbl(contentRt, "CdText", "", 20f);
        _slotCdText[i].fontStyle = FontStyles.Bold;
        _slotCdText[i].color     = Color.white;
        _slotCdText[i].alignment = TextAlignmentOptions.Center;
        Stretch(_slotCdText[i].rectTransform);

        // Active ring (border glow only; transparent center)
        _slotRing[i] = Img(slotRt, "Ring", Transparent);
        _slotRing[i].sprite = SlotRingSprite();
        _slotRing[i].type = Image.Type.Sliced;
        _slotRing[i].raycastTarget = false;
        _slotRing[i].rectTransform.anchorMin = new Vector2(-0.06f, -0.06f);
        _slotRing[i].rectTransform.anchorMax = new Vector2(1.06f, 1.06f);
        _slotRing[i].rectTransform.offsetMin = _slotRing[i].rectTransform.offsetMax = Vector2.zero;

        // Key label (number)
        _slotKey[i] = Lbl(contentRt, "Key", (i + 1).ToString(), 12f);
        _slotKey[i].fontStyle = FontStyles.Bold;
        _slotKey[i].color     = new Color(1f, 1f, 1f, 0.55f);
        _slotKey[i].rectTransform.anchorMin = new Vector2(0.02f, 0.72f);
        _slotKey[i].rectTransform.anchorMax = new Vector2(0.40f, 0.98f);
        _slotKey[i].rectTransform.offsetMin = _slotKey[i].rectTransform.offsetMax = Vector2.zero;
        _slotKey[i].alignment = TextAlignmentOptions.TopLeft;

        // Ability name (bottom strip — anchored to bar bottom, below the slot)
        _slotName[i] = Lbl(parent, "Name_" + i, "", 9f);
        _slotName[i].color     = TextDim;
        _slotName[i].alignment = TextAlignmentOptions.Center;
        var nameRt = _slotName[i].rectTransform;
        nameRt.anchorMin        = new Vector2(0.5f, 0f);
        nameRt.anchorMax        = new Vector2(0.5f, 0f);
        nameRt.pivot            = new Vector2(0.5f, 0f);
        nameRt.anchoredPosition = new Vector2(x, ActionBarNameY);
        nameRt.sizeDelta        = new Vector2(size, 16f);

        int capturedSlot = i;
        var pointerEvents = slotRt.gameObject.AddComponent<EventTrigger>();
        pointerEvents.triggers = new List<EventTrigger.Entry>();
        AddPointerEvent(
            pointerEvents,
            EventTriggerType.PointerEnter,
            data => ShowAbilityTooltip(
                capturedSlot,
                ((PointerEventData)data).position));
        AddPointerEvent(
            pointerEvents,
            EventTriggerType.PointerExit,
            _ => HideAbilityTooltip());
        AddPointerEvent(
            pointerEvents,
            EventTriggerType.PointerClick,
            data =>
            {
                var pointer = data as PointerEventData;
                if (pointer == null ||
                    pointer.button != PointerEventData.InputButton.Left)
                    return;

                _caster?.TryActivateSlot(capturedSlot);
            });
    }

    void RebuildAbilitySlots()
    {
        for (int i = 0; i < Slots; i++)
        {
            AbilityDef ab = (_caster != null && _caster.abilities != null && i < _caster.abilities.Length)
                ? _caster.abilities[i] : null;

            if (ab != null)
            {
                _slotIcon[i].sprite = ab.icon;
                _slotIcon[i].color  = ab.icon != null ? IconReady
                    : CategoryTint(ab.category);
                if (_slotName[i] != null) _slotName[i].text = ab.abilityName;
            }
            else
            {
                _slotIcon[i].sprite = null;
                _slotIcon[i].color  = new Color(0.25f, 0.25f, 0.30f, 0.6f);
                if (_slotName[i] != null) _slotName[i].text = "—";
            }
        }
        SetActiveSlot(_activeSlot);
    }

    void TickAbilityBar()
    {
        // Mirror AbilityCaster's held slot so the ring tracks the ability currently being aimed.
        if (_caster != null) _activeSlot = _caster.HeldAbilityIndex;

        // While aiming, show the active variant name below the held slot label
        bool isAiming = _caster != null && _caster.HeldAbilityIndex >= 0;
        if (isAiming && _caster.abilities != null)
        {
            int held = _caster.HeldAbilityIndex;
            AbilityDef heldAbility = (held < _caster.abilities.Length) ? _caster.abilities[held] : null;
            if (heldAbility != null && heldAbility.variants != null && heldAbility.variants.Length > 0)
            {
                int vi = _caster.ActiveVariantIndex;
                vi = Mathf.Clamp(vi, 0, heldAbility.variants.Length - 1);
                string vName = _caster.GetVariantDisplayName(heldAbility, vi);
                if (_slotName[held] != null && _slotName[held].text != vName)
                    _slotName[held].text = vName;

                if (_slotIcon[held] != null)
                {
                    _slotIcon[held].color = _caster.GetVariantTint(heldAbility, vi);
                }
            }
        }
        else if (!isAiming && _caster != null && _caster.abilities != null && _wasAimingLastFrame)
        {
            // Restore ability names once when aim ends
            for (int s = 0; s < Slots; s++)
            {
                AbilityDef ab = (s < _caster.abilities.Length) ? _caster.abilities[s] : null;
                if (_slotName[s] != null)
                    _slotName[s].text = ab != null ? ab.abilityName : "—";

                if (_slotIcon[s] != null)
                {
                    _slotIcon[s].color = (ab != null && ab.icon != null) ? IconReady
                        : (ab != null ? CategoryTint(ab.category) : new Color(0.25f, 0.25f, 0.30f, 0.6f));
                }
            }
        }
        _wasAimingLastFrame = isAiming;

        // Cooldown fills + ring pulse
        for (int i = 0; i < Slots; i++)
        {
            float cd = (_caster != null) ? _caster.GetCooldownFraction(i) : 0f;
            _slotCooldown[i].fillAmount = cd;
            _slotCooldown[i].color = cd > 0.001f
                ? new Color(CooldownCol.r, CooldownCol.g, CooldownCol.b, Mathf.Lerp(0.35f, CooldownCol.a, cd))
                : Transparent;

            AbilityDef ab = (_caster != null && _caster.abilities != null && i < _caster.abilities.Length)
                ? _caster.abilities[i] : null;
            bool showingVariantTint = isAiming && i == _caster.HeldAbilityIndex
                && ab != null && ab.variants != null && ab.variants.Length > 0;
            if (_slotIcon[i] != null && !showingVariantTint)
                _slotIcon[i].color = SlotIconColor(ab, cd);

            // Live countdown number over the icon while on cooldown
            if (_slotCdText[i] != null)
            {
                float rem = (_caster != null) ? _caster.GetCooldownRemaining(i) : 0f;
                _slotCdText[i].text = rem > 0.05f
                    ? (rem >= 1f ? Mathf.Ceil(rem).ToString("0") : rem.ToString("0.0"))
                    : "";
            }

            // Active slot: gold ring with pulse
            if (i == _activeSlot)
            {
                float pulse = 0.7f + 0.3f * Mathf.Sin(Time.time * 4f);
                _slotRing[i].color = new Color(SlotActive.r, SlotActive.g, SlotActive.b, pulse);
                _slotBg[i].color   = new Color(0.20f, 0.18f, 0.08f, 0.95f);
                _slotKey[i].color  = SlotActive;
            }
            else
            {
                _slotRing[i].color = Transparent;
                _slotBg[i].color   = SlotNormal;
                _slotKey[i].color  = new Color(1f, 1f, 1f, 0.45f);
            }

            // Scale bounce when slot selected
            float targetScale = (i == _activeSlot) ? 1.08f : 1.00f;
            _slotBg[i].transform.localScale = Vector3.Lerp(
                _slotBg[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * 12f);
        }
    }

    void SetActiveSlot(int slot)
    {
        _activeSlot = slot;
    }

    void BuildAbilityTooltip()
    {
        _abilityTooltipRoot = Rt(
            _canvas.transform,
            "AbilityTooltip");
        _abilityTooltipRoot.anchorMin = Vector2.zero;
        _abilityTooltipRoot.anchorMax = Vector2.zero;
        _abilityTooltipRoot.pivot = new Vector2(0.5f, 0.5f);
        _abilityTooltipRoot.sizeDelta =
            new Vector2(AbilityTooltipWidth, AbilityTooltipHeight);

        var shadow = Img(
            _abilityTooltipRoot,
            "Shadow",
            new Color(0f, 0f, 0f, 0.72f));
        Stretch(shadow.rectTransform);
        shadow.rectTransform.offsetMin = new Vector2(5f, -5f);
        shadow.rectTransform.offsetMax = new Vector2(5f, -5f);
        shadow.raycastTarget = false;

        var border = Img(
            _abilityTooltipRoot,
            "Border",
            new Color(0.66f, 0.50f, 0.12f, 0.98f));
        Stretch(border.rectTransform);
        border.raycastTarget = false;

        var panel = Img(
            _abilityTooltipRoot,
            "Panel",
            new Color(0.035f, 0.035f, 0.065f, 0.985f));
        Stretch(panel.rectTransform);
        panel.rectTransform.offsetMin = new Vector2(2f, 2f);
        panel.rectTransform.offsetMax = new Vector2(-2f, -2f);
        panel.raycastTarget = false;

        var accent = Img(
            _abilityTooltipRoot,
            "Accent",
            SlotActive);
        accent.rectTransform.anchorMin = new Vector2(0f, 0f);
        accent.rectTransform.anchorMax = new Vector2(0f, 1f);
        accent.rectTransform.pivot = new Vector2(0f, 0.5f);
        accent.rectTransform.sizeDelta = new Vector2(5f, 0f);
        accent.rectTransform.anchoredPosition = Vector2.zero;
        accent.raycastTarget = false;

        _abilityTooltipName = Lbl(
            _abilityTooltipRoot,
            "Name",
            "",
            18f);
        _abilityTooltipName.fontStyle = FontStyles.Bold;
        _abilityTooltipName.color = TextPrimary;
        _abilityTooltipName.enableAutoSizing = true;
        _abilityTooltipName.fontSizeMin = 13f;
        _abilityTooltipName.fontSizeMax = 18f;
        _abilityTooltipName.alignment =
            TextAlignmentOptions.MidlineLeft;
        _abilityTooltipName.rectTransform.anchorMin =
            new Vector2(0.055f, 0.72f);
        _abilityTooltipName.rectTransform.anchorMax =
            new Vector2(0.95f, 0.94f);
        _abilityTooltipName.rectTransform.offsetMin =
            _abilityTooltipName.rectTransform.offsetMax =
                Vector2.zero;
        _abilityTooltipName.raycastTarget = false;

        _abilityTooltipStats = Lbl(
            _abilityTooltipRoot,
            "Stats",
            "",
            11.5f);
        _abilityTooltipStats.fontStyle = FontStyles.Bold;
        _abilityTooltipStats.color =
            new Color(0.95f, 0.78f, 0.24f, 1f);
        _abilityTooltipStats.alignment =
            TextAlignmentOptions.MidlineLeft;
        _abilityTooltipStats.rectTransform.anchorMin =
            new Vector2(0.055f, 0.56f);
        _abilityTooltipStats.rectTransform.anchorMax =
            new Vector2(0.95f, 0.72f);
        _abilityTooltipStats.rectTransform.offsetMin =
            _abilityTooltipStats.rectTransform.offsetMax =
                Vector2.zero;
        _abilityTooltipStats.raycastTarget = false;

        _abilityTooltipDescription = Lbl(
            _abilityTooltipRoot,
            "Description",
            "",
            12f);
        _abilityTooltipDescription.color =
            new Color(0.84f, 0.83f, 0.87f, 1f);
        _abilityTooltipDescription.enableAutoSizing = true;
        _abilityTooltipDescription.fontSizeMin = 9f;
        _abilityTooltipDescription.fontSizeMax = 12f;
        _abilityTooltipDescription.textWrappingMode =
            TextWrappingModes.Normal;
        _abilityTooltipDescription.alignment =
            TextAlignmentOptions.TopLeft;
        _abilityTooltipDescription.rectTransform.anchorMin =
            new Vector2(0.055f, 0.09f);
        _abilityTooltipDescription.rectTransform.anchorMax =
            new Vector2(0.95f, 0.55f);
        _abilityTooltipDescription.rectTransform.offsetMin =
            _abilityTooltipDescription.rectTransform.offsetMax =
                Vector2.zero;
        _abilityTooltipDescription.raycastTarget = false;

        _abilityTooltipRoot.gameObject.SetActive(false);
    }

    static void AddPointerEvent(
        EventTrigger trigger,
        EventTriggerType type,
        UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry
        {
            eventID = type
        };
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    void ShowAbilityTooltip(int slot, Vector2 pointerPosition)
    {
        if (!TryGetSlottedAbility(slot, out _))
        {
            HideAbilityTooltip();
            return;
        }

        _abilityTooltipSlot = slot;
        RefreshAbilityTooltip(pointerPosition);
        _abilityTooltipRoot.gameObject.SetActive(true);
        _abilityTooltipRoot.SetAsLastSibling();
    }

    void HideAbilityTooltip()
    {
        _abilityTooltipSlot = -1;
        if (_abilityTooltipRoot != null)
            _abilityTooltipRoot.gameObject.SetActive(false);
    }

    void TickAbilityTooltip()
    {
        if (_abilityTooltipSlot < 0 ||
            _abilityTooltipRoot == null ||
            !_abilityTooltipRoot.gameObject.activeSelf)
            return;

        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null)
        {
            HideAbilityTooltip();
            return;
        }

        RefreshAbilityTooltip(mouse.position.ReadValue());
    }

    void RefreshAbilityTooltip(Vector2 pointerPosition)
    {
        if (!TryGetSlottedAbility(
                _abilityTooltipSlot,
                out AbilityDef ability))
        {
            HideAbilityTooltip();
            return;
        }

        float remaining =
            _caster.GetCooldownRemaining(_abilityTooltipSlot);
        float cooldown = _caster.CooldownFor(ability);
        float mana = _caster.ManaCostFor(ability, 0);

        _abilityTooltipName.text =
            string.IsNullOrWhiteSpace(ability.abilityName)
                ? "UNNAMED SPELL"
                : ability.abilityName.ToUpperInvariant();
        _abilityTooltipStats.text =
            remaining > 0.05f
                ? $"{(ability.instantCast ? "INSTANT CAST     " : "")}COOLDOWN  {remaining:0.#}s / {cooldown:0.#}s     MANA  {mana:0.#}"
                : $"{(ability.instantCast ? "INSTANT CAST     " : "")}COOLDOWN  {cooldown:0.#}s     MANA  {mana:0.#}";
        _abilityTooltipDescription.text =
            string.IsNullOrWhiteSpace(ability.description)
                ? "No description authored yet."
                : ability.description.Trim();

        PositionAbilityTooltip(pointerPosition);
    }

    bool TryGetSlottedAbility(int slot, out AbilityDef ability)
    {
        ability = null;
        if (_caster == null ||
            _caster.abilities == null ||
            slot < 0 ||
            slot >= _caster.abilities.Length)
            return false;

        ability = _caster.abilities[slot];
        return ability != null;
    }

    void PositionAbilityTooltip(Vector2 pointerPosition)
    {
        float scale = _canvas != null
            ? Mathf.Max(0.01f, _canvas.scaleFactor)
            : 1f;
        float halfWidth = AbilityTooltipWidth * scale * 0.5f;
        float halfHeight = AbilityTooltipHeight * scale * 0.5f;
        float gap = 18f * scale;

        float x = Mathf.Clamp(
            pointerPosition.x,
            halfWidth + 8f,
            Screen.width - halfWidth - 8f);
        float y = pointerPosition.y + halfHeight + gap;
        if (y + halfHeight > Screen.height - 8f)
            y = pointerPosition.y - halfHeight - gap;
        y = Mathf.Clamp(
            y,
            halfHeight + 8f,
            Screen.height - halfHeight - 8f);

        _abilityTooltipRoot.position =
            new Vector3(x, y, 0f);
    }

    // ── Spellbook ─────────────────────────────────────────────────────────────

    void BuildCastBar()
    {
        var root = Rt(_canvas.transform, "CastBar");
        root.anchorMin        = new Vector2(0.5f, 0f);
        root.anchorMax        = new Vector2(0.5f, 0f);
        root.pivot            = new Vector2(0.5f, 0f);
        root.anchoredPosition = new Vector2(0f, 164f);
        root.sizeDelta        = new Vector2(380f, 42f);
        _castBarRoot = root.gameObject;

        var bg = Img(root, "Bg", new Color(0.04f, 0.04f, 0.07f, 0.92f));
        Stretch(bg.rectTransform);

        var track = Img(root, "Track", new Color(0.11f, 0.11f, 0.15f, 0.96f));
        track.rectTransform.anchorMin = new Vector2(0.035f, 0.18f);
        track.rectTransform.anchorMax = new Vector2(0.965f, 0.70f);
        track.rectTransform.offsetMin = track.rectTransform.offsetMax = Vector2.zero;

        _castBarFill = Img(track.rectTransform, "Fill", SlotActive);
        _castBarFill.type = Image.Type.Simple;
        _castBarFillRect = _castBarFill.rectTransform;
        _castBarFillRect.anchorMin = Vector2.zero;
        _castBarFillRect.anchorMax = new Vector2(0f, 1f);
        _castBarFillRect.offsetMin = _castBarFillRect.offsetMax = Vector2.zero;

        _castBarName = Lbl(root, "Name", "", 11f);
        _castBarName.fontStyle = FontStyles.Bold;
        _castBarName.color     = TextPrimary;
        _castBarName.enableAutoSizing = true;
        _castBarName.fontSizeMin = 8f;
        _castBarName.fontSizeMax = 11f;
        _castBarName.rectTransform.anchorMin = new Vector2(0.04f, 0.70f);
        _castBarName.rectTransform.anchorMax = new Vector2(0.74f, 0.99f);
        _castBarName.rectTransform.offsetMin = _castBarName.rectTransform.offsetMax = Vector2.zero;
        _castBarName.alignment = TextAlignmentOptions.MidlineLeft;

        _castBarTime = Lbl(root, "Time", "", 11f);
        _castBarTime.fontStyle = FontStyles.Bold;
        _castBarTime.color     = TextDim;
        _castBarTime.rectTransform.anchorMin = new Vector2(0.74f, 0.70f);
        _castBarTime.rectTransform.anchorMax = new Vector2(0.96f, 0.99f);
        _castBarTime.rectTransform.offsetMin = _castBarTime.rectTransform.offsetMax = Vector2.zero;
        _castBarTime.alignment = TextAlignmentOptions.MidlineRight;

        _castBarRoot.SetActive(false);
    }

    void TickCastBar()
    {
        bool casting = _caster != null && _caster.IsCommittedCasting;
        if (_castBarRoot != null && _castBarRoot.activeSelf != casting)
            _castBarRoot.SetActive(casting);
        if (!casting)
        {
            SetCastBarProgress(0f);
            return;
        }

        float progress = _caster.CommittedCastProgress;
        float remaining = _caster.CommittedCastRemaining;
        Color tint = CategoryTint(_caster.CommittedCastCategory);

        SetCastBarProgress(progress);
        _castBarFill.color = Color.Lerp(tint, Color.white, 0.16f);
        _castBarName.text = _caster.CommittedCastDisplayName.ToUpperInvariant();
        _castBarTime.text = $"{remaining:0.0}s";
    }

    void SetCastBarProgress(float progress)
    {
        if (_castBarFillRect == null) return;
        _castBarFillRect.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);
        _castBarFillRect.offsetMin = _castBarFillRect.offsetMax = Vector2.zero;
    }

    GameObject BuildSpellbookPanel()
    {
        var root = Rt(_spellbookCanvas.transform, "Spellbook");
        root.anchorMin = new Vector2(0.10f, 0.08f);
        root.anchorMax = new Vector2(0.90f, 0.92f);
        root.offsetMin = root.offsetMax = Vector2.zero;

        // Semi-transparent backdrop
        var backdrop = Img(root, "Backdrop", new Color(0.04f, 0.04f, 0.08f, 0.95f));
        Stretch(backdrop.rectTransform);

        // Header
        var header = Lbl(root, "Header", "SPELL LOADOUT", 22f);
        header.fontStyle = FontStyles.Bold;
        header.color     = new Color(0.95f, 0.80f, 0.15f, 1f);
        header.rectTransform.anchorMin = new Vector2(0f, 0.91f);
        header.rectTransform.anchorMax = new Vector2(1f, 1.00f);
        header.rectTransform.offsetMin = header.rectTransform.offsetMax = Vector2.zero;
        header.alignment = TextAlignmentOptions.Center;

        var close = Img(root, "Close", new Color(0.18f, 0.12f, 0.13f, 0.95f));
        close.rectTransform.anchorMin = new Vector2(0.955f, 0.935f);
        close.rectTransform.anchorMax = new Vector2(0.985f, 0.985f);
        close.rectTransform.offsetMin = close.rectTransform.offsetMax = Vector2.zero;
        var closeButton = close.gameObject.AddComponent<Button>();
        closeButton.onClick.AddListener(CloseSpellLoadout);
        var closeLabel = Lbl(close.rectTransform, "Label", "×", 20f);
        closeLabel.alignment = TextAlignmentOptions.Center;
        Stretch(closeLabel.rectTransform);

        // Sub-header
        _spellbookInstructions = Lbl(
            root,
            "Sub",
            "Choose a spell, then choose one of your four equipped slots",
            11f);
        _spellbookInstructions.color = TextDim;
        _spellbookInstructions.rectTransform.anchorMin =
            new Vector2(0f, 0.865f);
        _spellbookInstructions.rectTransform.anchorMax =
            new Vector2(1f, 0.92f);
        _spellbookInstructions.rectTransform.offsetMin =
            _spellbookInstructions.rectTransform.offsetMax = Vector2.zero;
        _spellbookInstructions.alignment =
            TextAlignmentOptions.Center;

        BuildLoadoutSlots(root);

        // ── Scrollable card grid ──────────────────────────────────────────────
        // The class pool can be up to 32 cards; a fixed grid overflowed the panel
        // and spilled onto the action bar. Wrap it in a ScrollRect so it always
        // stays inside the panel body and scrolls instead.
        var scrollGO = new GameObject("SpellScroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGO.transform.SetParent(root, false);
        var scrollRt = scrollGO.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.02f, 0.04f);
        scrollRt.anchorMax = new Vector2(0.98f, 0.70f);
        scrollRt.offsetMin = scrollRt.offsetMax = Vector2.zero;

        // Viewport clips overflow (needs a graphic for the mask to work).
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        viewport.transform.SetParent(scrollGO.transform, false);
        var vpRt = viewport.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = vpRt.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.004f);

        // Content grows downward; ContentSizeFitter drives its height from the grid.
        var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot     = new Vector2(0.5f, 1f);
        contentRt.offsetMin = contentRt.offsetMax = Vector2.zero;

        var glg = content.GetComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(215f, 185f);
        glg.spacing         = new Vector2(12f, 12f);
        glg.padding         = new RectOffset(6, 6, 6, 6);
        glg.startCorner     = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis       = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment  = TextAnchor.UpperCenter;
        // Fixed column count pairs reliably with ContentSizeFitter for row height;
        // 6 columns fit the panel width and keep cards readable.
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 6;

        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _spellScroll = scrollGO.GetComponent<ScrollRect>();
        _spellScroll.viewport      = vpRt;
        _spellScroll.content       = contentRt;
        _spellScroll.horizontal    = false;
        _spellScroll.vertical      = true;
        _spellScroll.movementType  = ScrollRect.MovementType.Clamped;
        _spellScroll.scrollSensitivity = 28f;

        // Cards populated later in RebuildSpellbook()
        root.gameObject.name = "SpellbookRoot";
        _spellbookGridParent = content.transform;
        return root.gameObject;
    }

    void BuildLoadoutSlots(RectTransform parent)
    {
        var slotsRoot = Rt(parent, "EquippedSlots");
        slotsRoot.anchorMin = new Vector2(0.16f, 0.72f);
        slotsRoot.anchorMax = new Vector2(0.84f, 0.855f);
        slotsRoot.offsetMin = slotsRoot.offsetMax = Vector2.zero;

        for (int slot = 0; slot < Slots; slot++)
        {
            int capturedSlot = slot;
            var slotRoot = Rt(slotsRoot, $"Equipped_{slot + 1}");
            float minX = slot / (float)Slots;
            float maxX = (slot + 1) / (float)Slots;
            slotRoot.anchorMin = new Vector2(minX, 0f);
            slotRoot.anchorMax = new Vector2(maxX, 1f);
            slotRoot.offsetMin = new Vector2(5f, 3f);
            slotRoot.offsetMax = new Vector2(-5f, -3f);

            _loadoutSlotBg[slot] =
                Img(slotRoot, "Background", SlotNormal);
            Stretch(_loadoutSlotBg[slot].rectTransform);
            var button =
                _loadoutSlotBg[slot].gameObject.AddComponent<Button>();
            button.onClick.AddListener(
                () => OnLoadoutSlotClick(capturedSlot));

            _loadoutSlotIcon[slot] =
                Img(slotRoot, "Icon", Transparent);
            _loadoutSlotIcon[slot].preserveAspect = true;
            _loadoutSlotIcon[slot].raycastTarget = false;
            _loadoutSlotIcon[slot].rectTransform.anchorMin =
                new Vector2(0.03f, 0.12f);
            _loadoutSlotIcon[slot].rectTransform.anchorMax =
                new Vector2(0.34f, 0.88f);
            _loadoutSlotIcon[slot].rectTransform.offsetMin =
                _loadoutSlotIcon[slot].rectTransform.offsetMax =
                    Vector2.zero;

            var key = Lbl(
                slotRoot,
                "SlotNumber",
                $"SLOT {slot + 1}",
                10f);
            key.fontStyle = FontStyles.Bold;
            key.color = SlotActive;
            key.raycastTarget = false;
            key.rectTransform.anchorMin = new Vector2(0.37f, 0.54f);
            key.rectTransform.anchorMax = new Vector2(0.96f, 0.91f);
            key.rectTransform.offsetMin =
                key.rectTransform.offsetMax = Vector2.zero;
            key.alignment = TextAlignmentOptions.BottomLeft;

            _loadoutSlotName[slot] =
                Lbl(slotRoot, "SpellName", "EMPTY", 11f);
            _loadoutSlotName[slot].fontStyle = FontStyles.Bold;
            _loadoutSlotName[slot].enableAutoSizing = true;
            _loadoutSlotName[slot].fontSizeMin = 7f;
            _loadoutSlotName[slot].fontSizeMax = 11f;
            _loadoutSlotName[slot].raycastTarget = false;
            _loadoutSlotName[slot].rectTransform.anchorMin =
                new Vector2(0.37f, 0.10f);
            _loadoutSlotName[slot].rectTransform.anchorMax =
                new Vector2(0.96f, 0.56f);
            _loadoutSlotName[slot].rectTransform.offsetMin =
                _loadoutSlotName[slot].rectTransform.offsetMax =
                    Vector2.zero;
            _loadoutSlotName[slot].alignment =
                TextAlignmentOptions.TopLeft;
        }
    }

    void RebuildSpellbook()
    {
        if (_spellbookGridParent == null) return;

        // Clear old cards
        _spellCardBackgrounds.Clear();
        for (int i = _spellbookGridParent.childCount - 1; i >= 0; i--)
            Destroy(_spellbookGridParent.GetChild(i).gameObject);

        RefreshLoadoutSlots();

        bool usingSpellbook = _caster != null && _caster.spellbook != null && _caster.spellbook.Length > 0;
        AbilityDef[] pool = usingSpellbook
            ? _caster.spellbook
            : (_caster != null && _caster.abilities != null ? _caster.abilities : null);

        if (pool == null) return;

        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] == null) continue;
            if (usingSpellbook && pool[i].variantOnly)
                continue;
            if (usingSpellbook && _caster.classPool != null && !_caster.IsAllowedByClass(i))
                continue;
            int idx = i;
            BuildSpellCard(_spellbookGridParent, pool[i], idx);
        }
    }

    void BuildSpellCard(Transform parent, AbilityDef ab, int idx)
    {
        bool hasVariants = HasVariants(ab);

        var card = new GameObject("Card_" + idx); card.transform.SetParent(parent, false);
        var cardRt = card.AddComponent<RectTransform>();

        var bg = card.AddComponent<Image>();
        _spellCardBackgrounds[idx] = bg;

        var btn = card.AddComponent<Button>();
        btn.onClick.AddListener(() => OnSpellCardClick(idx));

        // Left color strip by category
        var strip = Img(cardRt, "Strip", CategoryTint(ab.category));
        strip.rectTransform.anchorMin = Vector2.zero;
        strip.rectTransform.anchorMax = new Vector2(0f, 1f);
        strip.rectTransform.sizeDelta = new Vector2(5f, 0f);
        strip.rectTransform.anchoredPosition = Vector2.zero;

        // Icon
        var iconImg = Img(cardRt, "Icon", ab.icon != null ? Color.white : CategoryTint(ab.category));
        iconImg.sprite = ab.icon;
        iconImg.preserveAspect = true;
        iconImg.rectTransform.anchorMin = new Vector2(0.05f, 0.40f);
        iconImg.rectTransform.anchorMax = new Vector2(0.32f, 0.95f);
        iconImg.rectTransform.offsetMin = iconImg.rectTransform.offsetMax = Vector2.zero;

        // Name - auto-sizes down from 13pt if text is long
        var name = Lbl(cardRt, "Name", ab.abilityName.ToUpper(), 13f);
        name.fontStyle          = FontStyles.Bold;
        name.color              = Color.white;
        name.enableAutoSizing   = true;
        name.fontSizeMin        = 9f;
        name.fontSizeMax        = 13f;
        name.textWrappingMode   = TextWrappingModes.Normal;
        name.rectTransform.anchorMin = new Vector2(0.35f, 0.58f);
        name.rectTransform.anchorMax = new Vector2(0.97f, 0.94f);
        name.rectTransform.offsetMin = name.rectTransform.offsetMax = Vector2.zero;
        name.alignment = TextAlignmentOptions.BottomLeft;

        // Delivery + category line (how the ability is thrown)
        string delivery = ab.shape == AbilityShape.Cone      ? "CONE"
                        : ab.shape == AbilityShape.Rectangle ? "LINE"
                        : ab.spawnTurret                     ? "DEPLOY"
                        : ab.range <= 0f                     ? "SELF"
                        :                                      "AoE";
        string variantBadge = hasVariants ? $"  |  {CountValidVariants(ab)} ZONES" : "";
        string instantCastBadge = ab.instantCast ? "  |  INSTANT CAST" : "";
        var cat = Lbl(cardRt, "Cat", $"{delivery}  |  {ab.category.ToString().ToUpper()}{variantBadge}{instantCastBadge}", 10f);
        cat.color     = CategoryTint(ab.category);
        cat.fontStyle = FontStyles.Bold;
        cat.rectTransform.anchorMin = new Vector2(0.35f, 0.44f);
        cat.rectTransform.anchorMax = new Vector2(0.97f, 0.60f);
        cat.rectTransform.offsetMin = cat.rectTransform.offsetMax = Vector2.zero;
        cat.alignment = TextAlignmentOptions.TopLeft;

        // Readable stat lines (one fact per line, colour-coded)
        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrWhiteSpace(ab.description))
        {
            sb.Append(
                $"<color=#d8d3e3><i>{ab.description.Trim()}</i></color>\n");
        }
        if (hasVariants)
        {
            AppendVariantStats(sb, ab, _caster);
        }
        else
        {
            if (ab.manaCost > 0f)
                sb.Append($"<color=#4aa3ff>Mana</color> {ab.manaCost:0}\n");
            if (ab.chargeable && ab.maxChargeDamage > ab.damage)
                sb.Append($"<color=#ff6b4a>Damage</color> {ab.damage:0}-{ab.maxChargeDamage:0}  <i><color=#94a3b8>hold to charge</color></i>\n");
            else if (ab.damage > 0f)
                sb.Append($"<color=#ff6b4a>Damage</color> {ab.damage:0}\n");
            if (ab.secondaryDamage > 0f)
            {
                sb.Append(
                    $"<color=#ff936b>Secondary Damage</color> {ab.secondaryDamage:0}");
                if (ab.secondaryDamageDelay > 0f)
                    sb.Append($" | {ab.secondaryDamageDelay:0.##}s delay");
                sb.Append("\n");
            }
            if (ab.healAmount > 0f)
                sb.Append($"<color=#39e67a>Heal</color> +{ab.healAmount:0}\n");
            if (ab.shieldAbsorb > 0f)
                sb.Append($"<color=#5aa0ff>Shield</color> {ab.shieldAbsorb:0}" + (ab.shieldDuration > 0f ? $" | {ab.shieldDuration:0.#}s\n" : "\n"));
        }
        sb.Append(ab.range > 0f
            ? $"<color=#94a3b8>Range</color> {ab.range:0.#}m"
            : "<color=#94a3b8>Self-cast</color>");

        var desc = Lbl(cardRt, "Desc", sb.ToString(), hasVariants ? 9.4f : 10.5f);
        desc.color             = new Color(0.88f, 0.88f, 0.92f, 1f);
        desc.richText          = true;
        desc.enableAutoSizing  = true;
        desc.fontSizeMin       = 7.5f;
        desc.fontSizeMax       = hasVariants ? 9.4f : 10.5f;
        desc.textWrappingMode  = TextWrappingModes.Normal;
        desc.lineSpacing       = hasVariants ? 2f : 6f;
        desc.rectTransform.anchorMin = new Vector2(0.06f, 0.05f);
        desc.rectTransform.anchorMax = new Vector2(0.97f, 0.42f);
        desc.rectTransform.offsetMin = desc.rectTransform.offsetMax = Vector2.zero;
        desc.alignment = TextAlignmentOptions.TopLeft;

        // Cooldown pill - top-right, unmistakable
        var cdPill = Img(cardRt, "CdPill", ab.cooldown > 0f
            ? new Color(0.14f, 0.11f, 0.03f, 0.95f)
            : new Color(0.05f, 0.14f, 0.07f, 0.95f));
        cdPill.rectTransform.anchorMin = new Vector2(0.62f, 0.82f);
        cdPill.rectTransform.anchorMax = new Vector2(0.97f, 0.99f);
        cdPill.rectTransform.offsetMin = cdPill.rectTransform.offsetMax = Vector2.zero;

        var cdLabel = Lbl(cdPill.rectTransform, "CD",
            ab.cooldown > 0f ? $"CD {ab.cooldown:0.#}s" : "INSTANT", 10f);
        cdLabel.color     = ab.cooldown > 0f ? new Color(1f, 0.82f, 0.25f) : new Color(0.5f, 0.9f, 0.6f);
        cdLabel.fontStyle = FontStyles.Bold;
        Stretch(cdLabel.rectTransform);
        cdLabel.alignment = TextAlignmentOptions.Center;

        if (_caster != null &&
            _caster.IsEquipped(idx, out int equippedSlot))
        {
            var equipped = Lbl(
                cardRt,
                "Equipped",
                $"SLOT {equippedSlot + 1}",
                9f);
            equipped.color = new Color(0.50f, 1.00f, 0.65f, 1f);
            equipped.fontStyle = FontStyles.Bold;
            equipped.rectTransform.anchorMin =
                new Vector2(0.68f, 0.02f);
            equipped.rectTransform.anchorMax =
                new Vector2(0.97f, 0.16f);
            equipped.rectTransform.offsetMin =
                equipped.rectTransform.offsetMax = Vector2.zero;
            equipped.alignment = TextAlignmentOptions.BottomRight;
        }

        RefreshSpellbookSelectionVisuals();
    }

    static bool HasVariants(AbilityDef ab)
    {
        return ab != null && ab.variants != null && ab.variants.Length > 0;
    }

    static int CountValidVariants(AbilityDef ab)
    {
        if (!HasVariants(ab)) return 0;

        int count = 0;
        for (int i = 0; i < ab.variants.Length; i++)
            if (ab.variants[i] != null) count++;

        return Mathf.Max(count, ab.variants.Length);
    }

    static string BuildVariantNameList(AbilityDef ab, AbilityCaster caster = null)
    {
        if (!HasVariants(ab)) return "";

        var names = new System.Text.StringBuilder();
        int count = 0;
        for (int i = 0; i < ab.variants.Length; i++)
        {
            AbilityVariant variant = ab.variants[i];
            if (variant == null) continue;

            if (count > 0) names.Append(" / ");
            string variantName = caster != null ? caster.GetVariantDisplayName(ab, i) : variant.spellbookAbilityName;
            names.Append(string.IsNullOrEmpty(variantName) ? $"Zone {i + 1}" : variantName);
            count++;
        }

        string text = names.ToString();
        return text.Length > 48 ? text.Substring(0, 45) + "..." : text;
    }

    static void AppendVariantStats(System.Text.StringBuilder sb, AbilityDef ab, AbilityCaster caster = null)
    {
        sb.Append($"<color=#f6c453>Variants</color> {BuildVariantNameList(ab, caster)}\n");

        bool hasDamage = false;
        bool hasSecondaryDamage = false;
        bool hasHeal = false;
        bool hasHot = false;
        bool hasShield = false;
        bool hasStatus = false;
        bool hasMana = false;
        float minDamage = float.MaxValue;
        float maxDamage = 0f;
        float minSecondaryDamage = float.MaxValue;
        float maxSecondaryDamage = 0f;
        float minMana = float.MaxValue;
        float maxMana = 0f;
        float maxHeal = 0f;
        float maxHot = 0f;
        float maxShield = 0f;

        for (int i = 0; i < ab.variants.Length; i++)
        {
            AbilityVariant variant = ab.variants[i];
            if (variant == null) continue;
            AbilityDef payload = caster != null ? caster.GetVariantPayload(ab, i) : null;
            if (payload == null) continue;

            float damage = payload.damage;
            float secondaryDamage = payload.secondaryDamage;
            float heal = payload.healAmount;
            float hotTickAmount = payload.hotTickAmount;
            int hotTicks = payload.hotTicks;
            float shield = payload.shieldAbsorb;
            float statusDuration = payload.statusDuration;
            float mana = Mathf.Max(0f, payload.manaCost);

            minMana = Mathf.Min(minMana, mana);
            maxMana = Mathf.Max(maxMana, mana);
            if (mana > 0f)
                hasMana = true;

            if (damage > 0f)
            {
                hasDamage = true;
                minDamage = Mathf.Min(minDamage, damage);
                maxDamage = Mathf.Max(maxDamage, damage);
            }

            if (secondaryDamage > 0f)
            {
                hasSecondaryDamage = true;
                minSecondaryDamage = Mathf.Min(
                    minSecondaryDamage,
                    secondaryDamage);
                maxSecondaryDamage = Mathf.Max(
                    maxSecondaryDamage,
                    secondaryDamage);
            }

            if (heal > 0f)
            {
                hasHeal = true;
                maxHeal = Mathf.Max(maxHeal, heal);
            }

            if (hotTickAmount > 0f && hotTicks > 0)
            {
                hasHot = true;
                maxHot = Mathf.Max(maxHot, hotTickAmount * hotTicks);
            }

            if (shield > 0f)
            {
                hasShield = true;
                maxShield = Mathf.Max(maxShield, shield);
            }

            if (statusDuration > 0f)
                hasStatus = true;
        }

        bool wrote = false;
        if (hasMana)
        {
            sb.Append($"<color=#4aa3ff>Mana</color> {FormatRange(minMana, maxMana)}");
            wrote = true;
        }
        if (hasDamage)
        {
            if (wrote) sb.Append("  ");
            sb.Append($"<color=#ff6b4a>Damage</color> {FormatRange(minDamage, maxDamage)}");
            wrote = true;
        }
        if (hasSecondaryDamage)
        {
            if (wrote) sb.Append("  ");
            sb.Append(
                $"<color=#ff936b>Secondary</color> " +
                FormatRange(minSecondaryDamage, maxSecondaryDamage));
            wrote = true;
        }
        if (hasHeal)
        {
            if (wrote) sb.Append("  ");
            sb.Append($"<color=#39e67a>Heal</color> +{maxHeal:0}");
            wrote = true;
        }
        if (hasHot)
        {
            if (wrote) sb.Append("  ");
            sb.Append($"<color=#39e67a>HoT</color> +{maxHot:0}");
            wrote = true;
        }
        if (hasShield)
        {
            if (wrote) sb.Append("  ");
            sb.Append($"<color=#5aa0ff>Shield</color> {maxShield:0}");
            wrote = true;
        }
        if (hasStatus)
        {
            if (wrote) sb.Append("  ");
            sb.Append("<color=#f6c453>Status</color>");
            wrote = true;
        }
        if (wrote)
            sb.Append("\n");
    }

    static string FormatRange(float min, float max)
    {
        if (Mathf.Abs(max - min) < 0.05f)
            return max.ToString("0");

        return $"{min:0}-{max:0}";
    }

    void OnSpellCardClick(int idx)
    {
        if (_pendingLoadoutSlot >= 0)
        {
            EquipSpellIntoSlot(idx, _pendingLoadoutSlot);
            return;
        }

        _pendingSpellIdx = idx;
        RefreshSpellbookSelectionVisuals();
        RefreshLoadoutSlots();
        SetSpellbookInstructions(
            "Choose an equipped slot, or press 1–4");
    }

    void OnLoadoutSlotClick(int slot)
    {
        if (_pendingSpellIdx >= 0)
        {
            EquipSpellIntoSlot(_pendingSpellIdx, slot);
            return;
        }

        _pendingLoadoutSlot =
            _pendingLoadoutSlot == slot ? -1 : slot;
        RefreshLoadoutSlots();
        RefreshSpellbookSelectionVisuals();
        SetSpellbookInstructions(
            _pendingLoadoutSlot >= 0
                ? $"Choose a spell for Slot {_pendingLoadoutSlot + 1}"
                : "Choose a spell, then choose one of your four equipped slots");
    }

    void EquipSpellIntoSlot(int spellbookIndex, int slot)
    {
        if (_caster == null) return;

        if (!_caster.EquipSpell(spellbookIndex, slot))
        {
            SetSpellbookInstructions(
                "Unable to update loadout. Move closer to the Spell Shrine and try again.");
            return;
        }

        _pendingSpellIdx = -1;
        _pendingLoadoutSlot = -1;
        SetSpellbookInstructions("Updating loadout…");
    }

    void OnSpellEquipResult(bool accepted, string message)
    {
        if (accepted)
        {
            RebuildAbilitySlots();
            RebuildSpellbook();
            SetSpellbookInstructions(
                "Loadout updated — choose another spell or press Escape to finish");
            return;
        }

        SetSpellbookInstructions(
            string.IsNullOrWhiteSpace(message)
                ? "The server rejected that loadout change."
                : message);
    }

    void RefreshLoadoutSlots()
    {
        for (int slot = 0; slot < Slots; slot++)
        {
            AbilityDef ability =
                _caster != null &&
                _caster.abilities != null &&
                slot < _caster.abilities.Length
                    ? _caster.abilities[slot]
                    : null;

            if (_loadoutSlotIcon[slot] != null)
            {
                _loadoutSlotIcon[slot].sprite =
                    ability?.icon;
                _loadoutSlotIcon[slot].color =
                    ability == null
                        ? new Color(0.25f, 0.25f, 0.30f, 0.6f)
                        : ability.icon != null
                            ? Color.white
                            : CategoryTint(ability.category);
            }

            if (_loadoutSlotName[slot] != null)
                _loadoutSlotName[slot].text =
                    ability?.abilityName?.ToUpperInvariant() ??
                    "EMPTY";

            if (_loadoutSlotBg[slot] != null)
            {
                bool targetSlot =
                    _pendingLoadoutSlot == slot ||
                    (_pendingSpellIdx >= 0 &&
                     _caster != null &&
                     _caster.IsEquipped(
                         _pendingSpellIdx,
                         out int equippedSlot) &&
                     equippedSlot == slot);
                _loadoutSlotBg[slot].color = targetSlot
                    ? new Color(0.32f, 0.26f, 0.07f, 0.98f)
                    : SlotNormal;
            }
        }
    }

    void RefreshSpellbookSelectionVisuals()
    {
        foreach (KeyValuePair<int, Image> entry in
                 _spellCardBackgrounds)
        {
            if (entry.Value == null) continue;

            bool selected = entry.Key == _pendingSpellIdx;
            bool equipped =
                _caster != null &&
                _caster.IsEquipped(entry.Key, out _);
            entry.Value.color = selected
                ? new Color(0.33f, 0.26f, 0.06f, 0.98f)
                : equipped
                    ? new Color(0.08f, 0.22f, 0.15f, 0.95f)
                    : BgMid;
        }
    }

    void SetSpellbookInstructions(string message)
    {
        if (_spellbookInstructions != null)
            _spellbookInstructions.text = message;
    }

    public void OpenSpellLoadout(SpellLoadoutShrine shrine)
    {
        if (_caster == null || _spellbookCanvas == null)
        {
            Debug.LogWarning(
                "[SpellLoadout] Cannot open before the local player " +
                "and AbilityCaster are available.");
            return;
        }

        _activeLoadoutShrine = shrine;
        _spellbookOpen = true;
        _pendingSpellIdx = -1;
        _pendingLoadoutSlot = -1;
        RebuildSpellbook();
        SetSpellbookInstructions(
            "Choose a spell, then choose one of your four equipped slots");
        _spellbookCanvas.gameObject.SetActive(true);
        if (_spellScroll != null)
            _spellScroll.verticalNormalizedPosition = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseSpellLoadout()
    {
        _spellbookOpen = false;
        _activeLoadoutShrine = null;
        _pendingSpellIdx = -1;
        _pendingLoadoutSlot = -1;
        if (_spellbookCanvas != null)
            _spellbookCanvas.gameObject.SetActive(false);
    }

    public void CloseSpellLoadout(SpellLoadoutShrine shrine)
    {
        if (_activeLoadoutShrine != shrine) return;
        CloseSpellLoadout();
    }

    public bool IsEditingAt(SpellLoadoutShrine shrine)
    {
        return _spellbookOpen &&
               _activeLoadoutShrine == shrine;
    }

    void TickSpellbook()
    {
        if (!_spellbookOpen ||
            UnityEngine.InputSystem.Keyboard.current == null)
            return;

        var kb = UnityEngine.InputSystem.Keyboard.current;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            CloseSpellLoadout();
            return;
        }

        // Equip pending spell into selected slot
        if (_pendingSpellIdx >= 0)
        {
            int equip = -1;
            if (kb.digit1Key.wasPressedThisFrame) equip = 0;
            if (kb.digit2Key.wasPressedThisFrame) equip = 1;
            if (kb.digit3Key.wasPressedThisFrame) equip = 2;
            if (kb.digit4Key.wasPressedThisFrame) equip = 3;

            if (equip >= 0)
                EquipSpellIntoSlot(_pendingSpellIdx, equip);
        }
    }

    // ── Floating damage numbers ───────────────────────────────────────────────

    public void ShowFloatNumber(Vector3 worldPos, float amount, bool isHeal = false)
    {
        StartCoroutine(FloatRoutine(worldPos, amount, isHeal));
    }

    IEnumerator FloatRoutine(Vector3 worldPos, float amount, bool isHeal)
    {
        var go  = new GameObject("FloatNum", typeof(RectTransform));
        go.transform.SetParent(_floatCanvas.transform, false);
        var rt  = go.GetComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();

        tmp.text      = isHeal ? $"+{Mathf.RoundToInt(amount)}" : $"-{Mathf.RoundToInt(amount)}";
        tmp.fontSize  = 28f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = isHeal ? HealColor : DmgColor;
        tmp.alignment = TextAlignmentOptions.Center;
        rt.sizeDelta  = new Vector2(130f, 48f);

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(2f, -2f);
        shadow.useGraphicAlpha = true;

        float elapsed = 0f;
        float dur     = 1.2f;
        Vector3 startScreen = Camera.main != null
            ? Camera.main.WorldToScreenPoint(worldPos)
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);

        // Slight random horizontal spread
        float spreadX = Random.Range(-30f, 30f);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / dur;
            float y  = Mathf.Lerp(0f, 80f, Mathf.SmoothStep(0f, 1f, t));
            float a  = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _floatCanvas.GetComponent<RectTransform>(),
                new Vector2(startScreen.x + spreadX, startScreen.y + y),
                null, out Vector2 localPt);

            rt.anchoredPosition = localPt;
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, a);
            yield return null;
        }

        Destroy(go);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    Color CategoryTint(AbilityCategory cat) => cat switch
    {
        AbilityCategory.Heal    => HealColor,
        AbilityCategory.Support => new Color(0.30f, 0.60f, 1.00f),
        _                       => DmgColor,
    };

    Color SlotIconColor(AbilityDef ab, float cooldownFraction)
    {
        if (ab == null)
            return new Color(0.25f, 0.25f, 0.30f, 0.6f);

        Color ready = ab.icon != null ? IconReady : CategoryTint(ab.category);
        if (cooldownFraction <= 0.001f)
            return ready;

        float dim = Mathf.Clamp01(0.35f + cooldownFraction * 0.65f);
        return Color.Lerp(ready, IconCooldown, dim);
    }

    RectTransform Rt(Transform parent, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    Image Img(RectTransform parent, string name, Color color)
    {
        var go  = new GameObject(name); go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    TextMeshProUGUI Lbl(RectTransform parent, string name, string text, float size)
    {
        var go  = new GameObject(name); go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = size;
        tmp.color    = TextPrimary;
        return tmp;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Sprite HealthWellSprite()
    {
        if (_healthWellSprite != null)
            return _healthWellSprite;

        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "HUD_HealthWellCircle",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = (size - 2) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius + 0.5f - distance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(false, true);
        _healthWellSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        _healthWellSprite.name = "HUD_HealthWellCircle";
        _healthWellSprite.hideFlags = HideFlags.HideAndDontSave;
        return _healthWellSprite;
    }

    static Sprite SlotRingSprite()
    {
        if (_slotRingSprite != null)
            return _slotRingSprite;

        const int size = 32;
        const int thickness = 3;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "HUD_SlotRing",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool border = x < thickness || x >= size - thickness
                    || y < thickness || y >= size - thickness;
                pixels[y * size + x] = border ? Color.white : Transparent;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(false, true);

        _slotRingSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size,
            0,
            SpriteMeshType.FullRect,
            new Vector4(thickness, thickness, thickness, thickness));
        _slotRingSprite.name = "HUD_SlotRing";
        _slotRingSprite.hideFlags = HideFlags.HideAndDontSave;
        return _slotRingSprite;
    }
}
