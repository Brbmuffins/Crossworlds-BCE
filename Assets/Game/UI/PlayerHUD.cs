using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
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
///   • Spellbook overlay (Tab) — grid of all class abilities with icons
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
    static readonly Color ShieldCol   = new Color(0.35f, 0.70f, 1.00f, 0.80f);
    static readonly Color DmgColor    = new Color(1.00f, 0.32f, 0.15f, 1.00f);
    static readonly Color HealColor   = new Color(0.22f, 0.90f, 0.45f, 1.00f);
    static readonly Color TextPrimary = new Color(0.95f, 0.93f, 0.88f, 1.00f);
    static readonly Color TextDim     = new Color(0.55f, 0.53f, 0.50f, 1.00f);
    static readonly Color Transparent = new Color(0, 0, 0, 0);

    // ── UI state ──────────────────────────────────────────────────────────────
    Canvas      _canvas;
    Canvas      _spellbookCanvas;

    // HP bar
    Image       _hpFill;
    Image       _hpBg;
    Image       _shieldFill;
    TextMeshProUGUI _hpLabel;
    float       _displayedHp   = 1f;
    float       _hpFlashTimer  = 0f;

    // Ability bar
    const int Slots = 4;
    Image[]             _slotBg       = new Image[Slots];
    Image[]             _slotIcon     = new Image[Slots];
    Image[]             _slotCooldown = new Image[Slots];
    Image[]             _slotRing     = new Image[Slots];
    TextMeshProUGUI[]   _slotKey      = new TextMeshProUGUI[Slots];
    TextMeshProUGUI[]   _slotName     = new TextMeshProUGUI[Slots];
    int                 _activeSlot   = 0;

    // Spellbook
    GameObject          _spellbookPanel;
    bool                _spellbookOpen = false;

    // Floating numbers
    Canvas      _floatCanvas;

    // ── Player refs ───────────────────────────────────────────────────────────
    Health          _health;
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
            if (_spellbookOpen && _spellbookCanvas != null)
            {
                _spellbookOpen = false;
                _spellbookCanvas.gameObject.SetActive(false);
            }
            return;
        }

        TickHpBar();
        TickAbilityBar();
        TickSpellbook();
    }

    // ── Player scan ───────────────────────────────────────────────────────────

    void ScanForPlayer()
    {
        if (_health) return;  // Unity null check catches destroyed objects
        _scanTimer -= Time.deltaTime;
        if (_scanTimer > 0f) return;
        _scanTimer = 0.5f;

        foreach (var ni in FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!ni.isLocalPlayer) continue;
            BindPlayer(ni.gameObject);
            break;
        }
    }

    void BindPlayer(GameObject player)
    {
        // Unsubscribe from old player if we're rebinding (e.g. after respawn)
        if (_health) _health.onHealthChanged.RemoveListener(OnHealthChanged);

        _health = player.GetComponent<Health>();
        _caster = player.GetComponent<AbilityCaster>();

        if (_health)
        {
            _health.onHealthChanged.AddListener(OnHealthChanged);
            _displayedHp = _health.Fraction;
        }

        RebuildAbilitySlots();
        RebuildSpellbook();
    }

    void OnHealthChanged(float current, float max)
    {
        if (max <= 0f) return;
        float newFrac = current / max;
        if (newFrac < _displayedHp) _hpFlashTimer = 0.25f; // damage flash
        _displayedHp = newFrac;
        UpdateHpLabel(current, max);
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    void BuildAllUI()
    {
        _canvas = MakeCanvas(100);
        BuildHpBar();
        BuildAbilityBar();

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
        root.anchorMin = new Vector2(0.01f, 0.88f);
        root.anchorMax = new Vector2(0.13f, 0.96f);
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

        if (_hpFlashTimer > 0f)
        {
            _hpFlashTimer -= Time.deltaTime;
            float t = _hpFlashTimer / 0.25f;
            _hpFill.color = Color.Lerp(_hpFill.color, Color.white, t * 0.6f);
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

    void UpdateHpLabel(float current, float max)
    {
        if (_hpLabel != null)
            _hpLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    // ── Ability bar ───────────────────────────────────────────────────────────

    void BuildAbilityBar()
    {
        float slotSize  = 72f;
        float gap       = 8f;
        float barWidth  = Slots * slotSize + (Slots - 1) * gap + 24f;
        float barHeight = slotSize + 32f;

        var root = Rt(_canvas.transform, "AbilityBar");
        root.anchorMin        = new Vector2(0.5f, 0f);
        root.anchorMax        = new Vector2(0.5f, 0f);
        root.pivot            = new Vector2(0.5f, 0f);
        root.anchoredPosition = new Vector2(0f, 50f);
        root.sizeDelta        = new Vector2(barWidth, barHeight);

        // Panel background
        var bg = Img(root, "BarBg", BgDark);
        Stretch(bg.rectTransform);

        // Build 4 slots
        float startX = -(Slots * slotSize + (Slots - 1) * gap) / 2f + slotSize / 2f;

        for (int i = 0; i < Slots; i++)
        {
            float x = startX + i * (slotSize + gap);
            BuildSlot(root, i, x, slotSize);
        }
    }

    void BuildSlot(RectTransform parent, int i, float x, float size)
    {
        // Container — anchored to bar bottom so slots sit inside the panel
        var slotRt = Rt(parent, "Slot_" + i);
        slotRt.anchorMin        = new Vector2(0.5f, 0f);
        slotRt.anchorMax        = new Vector2(0.5f, 0f);
        slotRt.pivot            = new Vector2(0.5f, 0f);
        slotRt.anchoredPosition = new Vector2(x, 20f);
        slotRt.sizeDelta        = new Vector2(size, size);

        // Background
        _slotBg[i] = Img(slotRt, "Bg", SlotNormal);
        Stretch(_slotBg[i].rectTransform);

        // Icon
        _slotIcon[i] = Img(slotRt, "Icon", Transparent);
        _slotIcon[i].preserveAspect = true;
        _slotIcon[i].rectTransform.anchorMin = new Vector2(0.08f, 0.08f);
        _slotIcon[i].rectTransform.anchorMax = new Vector2(0.92f, 0.92f);
        _slotIcon[i].rectTransform.offsetMin = _slotIcon[i].rectTransform.offsetMax = Vector2.zero;

        // Cooldown overlay (filled from top)
        _slotCooldown[i] = Img(slotRt, "Cooldown", CooldownCol);
        _slotCooldown[i].type       = Image.Type.Filled;
        _slotCooldown[i].fillMethod = Image.FillMethod.Vertical;
        _slotCooldown[i].fillOrigin = (int)Image.OriginVertical.Top;
        _slotCooldown[i].fillAmount = 0f;
        Stretch(_slotCooldown[i].rectTransform);

        // Active ring (border glow)
        _slotRing[i] = Img(slotRt, "Ring", Transparent);
        _slotRing[i].rectTransform.anchorMin = new Vector2(-0.06f, -0.06f);
        _slotRing[i].rectTransform.anchorMax = new Vector2(1.06f, 1.06f);
        _slotRing[i].rectTransform.offsetMin = _slotRing[i].rectTransform.offsetMax = Vector2.zero;

        // Key label (number)
        _slotKey[i] = Lbl(slotRt, "Key", (i + 1).ToString(), 12f);
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
        nameRt.anchoredPosition = new Vector2(x, 2f);
        nameRt.sizeDelta        = new Vector2(size, 16f);
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
                _slotIcon[i].color  = ab.icon != null ? Color.white
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

        // Cooldown fills + ring pulse
        for (int i = 0; i < Slots; i++)
        {
            float cd = (_caster != null) ? _caster.GetCooldownFraction(i) : 0f;
            _slotCooldown[i].fillAmount = cd;

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

    // ── Spellbook ─────────────────────────────────────────────────────────────

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
        var header = Lbl(root, "Header", "SPELLBOOK", 22f);
        header.fontStyle = FontStyles.Bold;
        header.color     = new Color(0.95f, 0.80f, 0.15f, 1f);
        header.rectTransform.anchorMin = new Vector2(0f, 0.88f);
        header.rectTransform.anchorMax = new Vector2(1f, 1.00f);
        header.rectTransform.offsetMin = header.rectTransform.offsetMax = Vector2.zero;
        header.alignment = TextAlignmentOptions.Center;

        // Sub-header
        var sub = Lbl(root, "Sub", "Click a spell · Press 1-4 to equip · Tab to close", 11f);
        sub.color     = TextDim;
        sub.rectTransform.anchorMin = new Vector2(0f, 0.82f);
        sub.rectTransform.anchorMax = new Vector2(1f, 0.89f);
        sub.rectTransform.offsetMin = sub.rectTransform.offsetMax = Vector2.zero;
        sub.alignment = TextAlignmentOptions.Center;

        // Card grid container
        var grid = new GameObject("Grid"); grid.transform.SetParent(root, false);
        var gridRt = grid.AddComponent<RectTransform>();
        gridRt.anchorMin = new Vector2(0.02f, 0.04f);
        gridRt.anchorMax = new Vector2(0.98f, 0.82f);
        gridRt.offsetMin = gridRt.offsetMax = Vector2.zero;

        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(190f, 130f);
        glg.spacing         = new Vector2(10f, 10f);
        glg.startCorner     = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis       = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment  = TextAnchor.UpperCenter;
        glg.constraint      = GridLayoutGroup.Constraint.Flexible;

        // Cards populated later in RebuildSpellbook()
        root.gameObject.name = "SpellbookRoot";
        _spellbookGridParent = grid.transform;
        return root.gameObject;
    }

    Transform _spellbookGridParent;

    void RebuildSpellbook()
    {
        if (_spellbookGridParent == null) return;

        // Clear old cards
        for (int i = _spellbookGridParent.childCount - 1; i >= 0; i--)
            Destroy(_spellbookGridParent.GetChild(i).gameObject);

        AbilityDef[] pool = (_caster != null && _caster.spellbook != null && _caster.spellbook.Length > 0)
            ? _caster.spellbook
            : (_caster != null && _caster.abilities != null ? _caster.abilities : null);

        if (pool == null) return;

        for (int i = 0; i < pool.Length; i++)
        {
            int idx = i;
            BuildSpellCard(_spellbookGridParent, pool[i], idx);
        }
    }

    void BuildSpellCard(Transform parent, AbilityDef ab, int idx)
    {
        var card = new GameObject("Card_" + idx); card.transform.SetParent(parent, false);
        var cardRt = card.AddComponent<RectTransform>();

        var bg = card.AddComponent<Image>();
        bg.color = BgMid;

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

        // Name — auto-sizes down from 13pt if text is long
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

        // Category tag
        var cat = Lbl(cardRt, "Cat", ab.category.ToString().ToUpper(), 10f);
        cat.color     = CategoryTint(ab.category);
        cat.rectTransform.anchorMin = new Vector2(0.35f, 0.40f);
        cat.rectTransform.anchorMax = new Vector2(0.97f, 0.60f);
        cat.rectTransform.offsetMin = cat.rectTransform.offsetMax = Vector2.zero;
        cat.alignment = TextAlignmentOptions.TopLeft;

        // Stats row
        string descText = $"{ab.range:0.#}m range";
        if (ab.damage > 0f)       descText += $"  •  {ab.damage:0} dmg";
        if (ab.healAmount > 0f)   descText += $"  •  +{ab.healAmount:0} hp";
        if (ab.shieldAbsorb > 0f) descText += $"  •  {ab.shieldAbsorb:0} shield";
        var desc = Lbl(cardRt, "Desc", descText, 10f);
        desc.color             = TextDim;
        desc.textWrappingMode  = TextWrappingModes.Normal;
        desc.rectTransform.anchorMin = new Vector2(0.04f, 0.04f);
        desc.rectTransform.anchorMax = new Vector2(0.97f, 0.40f);
        desc.rectTransform.offsetMin = desc.rectTransform.offsetMax = Vector2.zero;
        desc.alignment = TextAlignmentOptions.TopLeft;

        // Cooldown badge — top-right corner
        var cdLabel = Lbl(cardRt, "CD", $"{ab.cooldown:0.#}s", 10f);
        cdLabel.color     = new Color(0.9f, 0.7f, 0.2f, 0.9f);
        cdLabel.fontStyle = FontStyles.Bold;
        cdLabel.rectTransform.anchorMin = new Vector2(0.65f, 0.86f);
        cdLabel.rectTransform.anchorMax = new Vector2(0.97f, 1.00f);
        cdLabel.rectTransform.offsetMin = cdLabel.rectTransform.offsetMax = Vector2.zero;
        cdLabel.alignment = TextAlignmentOptions.TopRight;
    }

    int _pendingSpellIdx = -1;

    void OnSpellCardClick(int idx)
    {
        _pendingSpellIdx = idx;
        // Highlight pending — player presses 1-4 to equip in TickSpellbook
    }

    void TickSpellbook()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null) return;
        var kb = UnityEngine.InputSystem.Keyboard.current;

        if (kb.tabKey.wasPressedThisFrame)
        {
            _spellbookOpen = !_spellbookOpen;
            _spellbookCanvas.gameObject.SetActive(_spellbookOpen);
            if (!_spellbookOpen) _pendingSpellIdx = -1;
            Cursor.lockState = _spellbookOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible   = _spellbookOpen;
        }

        // Equip pending spell into selected slot
        if (_spellbookOpen && _pendingSpellIdx >= 0)
        {
            int equip = -1;
            if (kb.digit1Key.wasPressedThisFrame) equip = 0;
            if (kb.digit2Key.wasPressedThisFrame) equip = 1;
            if (kb.digit3Key.wasPressedThisFrame) equip = 2;
            if (kb.digit4Key.wasPressedThisFrame) equip = 3;

            if (equip >= 0 && _caster != null)
            {
                _caster.EquipSpell(_pendingSpellIdx, equip);
                _pendingSpellIdx = -1;
                RebuildAbilitySlots();
            }
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
        tmp.fontSize  = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = isHeal ? HealColor : DmgColor;
        tmp.alignment = TextAlignmentOptions.Center;
        rt.sizeDelta  = new Vector2(100f, 40f);

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
}
