#if !UNITY_SERVER
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AbilityHUD — self-bootstrapping bottom-right ability icon strip.
/// Shows 4 equipped ability slots (keys 1–4) + 1 Tab/spellbook button.
/// Reads from the local player's AbilityCaster each frame.
///
/// No inspector setup required — auto-finds local PlayerIdentity → AbilityCaster.
/// Complements AbilityBar (which needs manual Image references; this builds its own).
///
/// Layout: [1][2][3][4] [T]  — bottom-right HUD, 64px slots, 4px gap.
/// </summary>
public class AbilityHUD : MonoBehaviour
{
    // ── Layout constants ──────────────────────────────────────────────────────
    const int   SLOTS      = 4;
    const float SLOT_SIZE  = 64f;
    const float GAP        = 4f;
    const float TAB_SIZE   = 44f;  // slightly smaller Tab button
    const float MARGIN_R   = 16f;
    const float MARGIN_B   = 16f;

    // ── Colours ───────────────────────────────────────────────────────────────
    static readonly Color ColDamage   = new Color(1.00f, 0.35f, 0.20f, 0.90f);
    static readonly Color ColHeal     = new Color(0.20f, 0.90f, 0.40f, 0.90f);
    static readonly Color ColSupport  = new Color(0.30f, 0.60f, 1.00f, 0.90f);
    static readonly Color ColBg       = new Color(0.08f, 0.08f, 0.10f, 0.85f);
    static readonly Color ColHeld     = new Color(1.00f, 0.90f, 0.15f, 1.00f);
    static readonly Color ColCD       = new Color(0.00f, 0.00f, 0.00f, 0.68f);
    static readonly Color ColEmpty    = new Color(0.20f, 0.20f, 0.22f, 0.80f);
    static readonly Color ColTabNorm  = new Color(0.18f, 0.18f, 0.22f, 0.88f);
    static readonly Color ColTabOpen  = new Color(0.30f, 0.60f, 1.00f, 0.90f);

    // ── UI references ─────────────────────────────────────────────────────────
    struct Slot
    {
        public Image     bg;
        public Image     icon;
        public Image     cdFill;
        public Text      keyLabel;
        public Text      cdLabel;    // "2.1s" remaining text
    }

    Slot[]      _slots   = new Slot[SLOTS];
    Image       _tabBg;
    Text        _tabLabel;

    // ── Runtime ───────────────────────────────────────────────────────────────
    AbilityCaster _caster;
    bool          _tabOpen; // mirrors AbilityBar spellbook state (we just display, don't own it)

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("AbilityHUD", typeof(RectTransform));
        DontDestroyOnLoad(go);
        go.AddComponent<AbilityHUD>();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        BuildUI();
        SetSlotsVisible(false);
    }

    IEnumerator Start()
    {
        // Poll until local PlayerIdentity has an AbilityCaster
        while (true)
        {
            _caster = FindLocalCaster();
            if (_caster != null)
            {
                SetSlotsVisible(true);
                RefreshSlotDefs();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if (_caster == null)
        {
            _caster = FindLocalCaster();
            if (_caster == null) return;
            SetSlotsVisible(true);
            RefreshSlotDefs();
        }

        RefreshCooldowns();
        RefreshHeldHighlight();
        RefreshTabState();
    }

    // ── UI Construction ───────────────────────────────────────────────────────
    void BuildUI()
    {
        // Root canvas
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        // Container — bottom-right anchor
        var containerGO = new GameObject("SlotContainer", typeof(RectTransform));
        containerGO.transform.SetParent(transform, false);
        var containerRT = containerGO.GetComponent<RectTransform>();
        containerRT.anchorMin = Vector2.right;
        containerRT.anchorMax = Vector2.right;
        containerRT.pivot     = Vector2.right; // right-bottom anchor

        // Total width: 4 slots + gap + 1 tab
        float totalW = SLOTS * SLOT_SIZE + (SLOTS - 1) * GAP + GAP * 2f + TAB_SIZE;
        containerRT.sizeDelta        = new Vector2(totalW, SLOT_SIZE);
        containerRT.anchoredPosition = new Vector2(-MARGIN_R, MARGIN_B + SLOT_SIZE / 2f);

        // Build 4 ability slots
        for (int i = 0; i < SLOTS; i++)
        {
            float xPos = i * (SLOT_SIZE + GAP);
            _slots[i] = BuildSlot(containerGO.transform, xPos, i + 1);
        }

        // Tab button (spellbook)
        float tabX = SLOTS * (SLOT_SIZE + GAP) + GAP;
        (_tabBg, _tabLabel) = BuildTabButton(containerGO.transform, tabX);
    }

    Slot BuildSlot(Transform parent, float xPos, int keyNum)
    {
        var s = new Slot();

        // Background
        var bgGO = new GameObject($"Slot{keyNum}", typeof(RectTransform));
        bgGO.transform.SetParent(parent, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.sizeDelta        = new Vector2(SLOT_SIZE, SLOT_SIZE);
        bgRT.anchorMin        = new Vector2(0f, 0.5f);
        bgRT.anchorMax        = new Vector2(0f, 0.5f);
        bgRT.pivot            = new Vector2(0f, 0.5f);
        bgRT.anchoredPosition = new Vector2(xPos, 0f);
        s.bg = bgGO.AddComponent<Image>();
        s.bg.color = ColBg;

        // Category icon (filled with category colour if no sprite)
        var iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(bgGO.transform, false);
        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.1f, 0.1f);
        iconRT.anchorMax = new Vector2(0.9f, 0.9f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;
        s.icon = iconGO.AddComponent<Image>();

        // Cooldown dark overlay (radial fill)
        var cdGO = new GameObject("CDFill", typeof(RectTransform));
        cdGO.transform.SetParent(bgGO.transform, false);
        var cdRT = cdGO.GetComponent<RectTransform>();
        cdRT.anchorMin = Vector2.zero;
        cdRT.anchorMax = Vector2.one;
        cdRT.offsetMin = Vector2.zero;
        cdRT.offsetMax = Vector2.zero;
        s.cdFill = cdGO.AddComponent<Image>();
        s.cdFill.color     = ColCD;
        s.cdFill.type      = Image.Type.Filled;
        s.cdFill.fillMethod = Image.FillMethod.Radial360;
        s.cdFill.fillClockwise = false;
        s.cdFill.fillAmount    = 0f;

        // Key label (bottom-left corner)
        var keyGO = new GameObject("KeyLabel", typeof(RectTransform));
        keyGO.transform.SetParent(bgGO.transform, false);
        var keyRT = keyGO.GetComponent<RectTransform>();
        keyRT.anchorMin        = Vector2.zero;
        keyRT.anchorMax        = new Vector2(0.5f, 0.35f);
        keyRT.offsetMin        = new Vector2(3f, 2f);
        keyRT.offsetMax        = Vector2.zero;
        s.keyLabel = keyGO.AddComponent<Text>();
        s.keyLabel.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        s.keyLabel.fontSize  = 11;
        s.keyLabel.text      = keyNum.ToString();
        s.keyLabel.color     = new Color(1f, 1f, 1f, 0.6f);
        s.keyLabel.alignment = TextAnchor.LowerLeft;

        // CD remaining text (centre, only when on cooldown)
        var cdTextGO = new GameObject("CDLabel", typeof(RectTransform));
        cdTextGO.transform.SetParent(bgGO.transform, false);
        var cdTextRT = cdTextGO.GetComponent<RectTransform>();
        cdTextRT.anchorMin = new Vector2(0.1f, 0.2f);
        cdTextRT.anchorMax = new Vector2(0.9f, 0.8f);
        cdTextRT.offsetMin = Vector2.zero;
        cdTextRT.offsetMax = Vector2.zero;
        s.cdLabel = cdTextGO.AddComponent<Text>();
        s.cdLabel.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        s.cdLabel.fontSize  = 18;
        s.cdLabel.fontStyle = FontStyle.Bold;
        s.cdLabel.alignment = TextAnchor.MiddleCenter;
        s.cdLabel.color     = Color.white;
        s.cdLabel.gameObject.SetActive(false);

        return s;
    }

    (Image bg, Text label) BuildTabButton(Transform parent, float xPos)
    {
        var bgGO = new GameObject("TabBtn", typeof(RectTransform));
        bgGO.transform.SetParent(parent, false);
        var rt = bgGO.GetComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(TAB_SIZE, TAB_SIZE);
        rt.anchorMin        = new Vector2(0f, 0.5f);
        rt.anchorMax        = new Vector2(0f, 0.5f);
        rt.pivot            = new Vector2(0f, 0.5f);
        rt.anchoredPosition = new Vector2(xPos, 0f);
        var bg = bgGO.AddComponent<Image>();
        bg.color = ColTabNorm;

        var labelGO = new GameObject("TabLabel", typeof(RectTransform));
        labelGO.transform.SetParent(bgGO.transform, false);
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;
        var label = labelGO.AddComponent<Text>();
        label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize  = 13;
        label.fontStyle = FontStyle.Bold;
        label.text      = "TAB";
        label.alignment = TextAnchor.MiddleCenter;
        label.color     = new Color(1f, 1f, 1f, 0.7f);

        return (bg, label);
    }

    // ── Per-frame refresh ─────────────────────────────────────────────────────
    void RefreshSlotDefs()
    {
        if (_caster == null) return;
        for (int i = 0; i < SLOTS; i++)
        {
            var ab = (i < _caster.abilities.Length) ? _caster.abilities[i] : null;
            if (ab == null)
            {
                _slots[i].bg.color   = ColEmpty;
                _slots[i].icon.color = new Color(0, 0, 0, 0);
                _slots[i].icon.sprite = null;
            }
            else
            {
                _slots[i].icon.sprite = ab.icon;
                _slots[i].icon.color  = ab.icon != null ? Color.white : CategoryColour(ab.category);
                _slots[i].bg.color    = ColBg;
            }
        }
    }

    void RefreshCooldowns()
    {
        for (int i = 0; i < SLOTS; i++)
        {
            float frac = _caster.GetCooldownFraction(i);
            _slots[i].cdFill.fillAmount = frac;

            bool onCD = frac > 0.01f;
            _slots[i].cdLabel.gameObject.SetActive(onCD);
            if (onCD)
            {
                var ab = (i < _caster.abilities.Length) ? _caster.abilities[i] : null;
                float remaining = ab != null ? frac * ab.cooldown : 0f;
                _slots[i].cdLabel.text = remaining > 1f
                    ? Mathf.CeilToInt(remaining).ToString()
                    : remaining.ToString("F1");
            }
        }
    }

    void RefreshHeldHighlight()
    {
        int held = _caster.HeldAbilityIndex;
        for (int i = 0; i < SLOTS; i++)
        {
            _slots[i].bg.color = i == held ? ColHeld : ColBg;
        }
    }

    void RefreshTabState()
    {
        // Check if AbilityBar spellbook is open by looking for an active spellbookPanel
        // We infer from cursor state: if cursor is unlocked, spellbook is probably open
        bool open = !UnityEngine.InputSystem.Mouse.current.rightButton.isPressed &&
                     UnityEngine.Cursor.lockState == CursorLockMode.None &&
                     _caster != null;
        _tabBg.color = open ? ColTabOpen : ColTabNorm;
    }

    void SetSlotsVisible(bool visible)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(visible);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static AbilityCaster FindLocalCaster()
    {
        foreach (var id in FindObjectsByType<Mirror.NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!id.isLocalPlayer) continue;
            var caster = id.GetComponent<AbilityCaster>();
            if (caster != null) return caster;
        }
        return null;
    }

    static Color CategoryColour(AbilityCategory cat) => cat switch
    {
        AbilityCategory.Heal    => ColHeal,
        AbilityCategory.Support => ColSupport,
        _                       => ColDamage,
    };
}
#endif
