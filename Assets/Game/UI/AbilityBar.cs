using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

// Ability bar — bottom HUD strip (4 equipped slots) + Tab spellbook overlay (all 8 spells).
//
// Inspector setup:
//   1. Assign caster.
//   2. Assign ability1–4 (the icon Images in your bottom bar).
//   3. Assign cooldown1–4 (fill-type Images overlaying each icon).
//   4. Assign spellbookPanel — a UI Panel that will auto-populate with spell cards.
//   5. (Optional) assign keyLabels1–4 (TextMeshProUGUI showing "1"–"4" on slots).
//
// Spellbook cards are created at runtime inside spellbookPanel.
// Tab opens/closes the spellbook. Click a card to select it, then press 1–4 to equip.

public class AbilityBar : MonoBehaviour
{
    public AbilityCaster caster;

    [Header("Bottom bar — 4 equipped slots")]
    public Image ability1;
    public Image ability2;
    public Image ability3;
    public Image ability4;

    public Image cooldown1;
    public Image cooldown2;
    public Image cooldown3;
    public Image cooldown4;

    public TextMeshProUGUI keyLabel1;
    public TextMeshProUGUI keyLabel2;
    public TextMeshProUGUI keyLabel3;
    public TextMeshProUGUI keyLabel4;

    [Header("Spellbook panel — assign a UI Panel; cards are created at runtime")]
    public GameObject spellbookPanel;

    // Runtime state
    private Image[] icons;
    private Image[] cooldownOverlays;
    private Color[] baseColors;
    private int selectedSlot = 0;

    private int pendingSpellbookIndex = -1;    // which spellbook spell the player clicked
    private Image[] spellbookCards;            // generated card backgrounds
    private TextMeshProUGUI[] spellbookLabels; // generated name labels
    private bool spellbookOpen = false;

    static readonly Color ColorSelected    = new Color(1f, 0.85f, 0.2f);
    static readonly Color ColorPendingCard = new Color(0.3f, 0.8f, 1f);
    static readonly Color ColorEquipped    = new Color(0.5f, 1f, 0.5f);
    static readonly Color ColorNormal      = new Color(0.15f, 0.15f, 0.2f, 0.85f);
    static readonly Color ColorDamage      = new Color(1f, 0.35f, 0.2f);
    static readonly Color ColorHeal        = new Color(0.2f, 0.9f, 0.4f);
    static readonly Color ColorSupport     = new Color(0.3f, 0.6f, 1f);

    void Start()
    {
        icons          = new Image[] { ability1, ability2, ability3, ability4 };
        cooldownOverlays = new Image[] { cooldown1, cooldown2, cooldown3, cooldown4 };
        baseColors     = new Color[4];

        RefreshBottomBar();

        if (spellbookPanel != null)
        {
            BuildSpellbookCards();
            spellbookPanel.SetActive(false);
        }

        HighlightSlot(0);
    }

    void Update()
    {
        HandleSlotInput();
        HandleSpellbookToggle();
        HandlePendingEquip();
        RefreshCooldowns();
        RefreshHeldTint();
    }

    // ── Bottom bar ──────────────────────────────────────────────────────────

    void RefreshBottomBar()
    {
        for (int i = 0; i < 4; i++)
        {
            if (icons[i] == null) continue;

            AbilityDef ab = (caster != null && i < caster.abilities.Length) ? caster.abilities[i] : null;

            if (ab != null)
            {
                icons[i].sprite = ab.icon;
                baseColors[i] = ab.icon != null ? Color.white : CategoryColor(ab.category);
            }
            else
            {
                icons[i].sprite = null;
                baseColors[i] = new Color(0.3f, 0.3f, 0.35f);
            }

            icons[i].color = baseColors[i];

            if (cooldownOverlays[i] != null)
                cooldownOverlays[i].fillAmount = 0f;
        }

        TextMeshProUGUI[] labels = { keyLabel1, keyLabel2, keyLabel3, keyLabel4 };
        string[] keys = { "1", "2", "3", "4" };
        for (int i = 0; i < 4; i++)
            if (labels[i] != null) labels[i].text = keys[i];
    }

    void HandleSlotInput()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);
    }

    void SelectSlot(int slot)
    {
        if (spellbookOpen && pendingSpellbookIndex >= 0)
        {
            // Equip the pending spell into this slot
            caster.EquipSpell(pendingSpellbookIndex, slot);
            pendingSpellbookIndex = -1;
            RefreshBottomBar();
            RefreshSpellbookCards();
            HighlightSlot(slot);
            return;
        }

        selectedSlot = slot;
        HighlightSlot(slot);
    }

    void HighlightSlot(int slot)
    {
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] == null) continue;
            icons[i].transform.localScale = (i == slot) ? Vector3.one * 1.15f : Vector3.one;
        }
    }

    void RefreshCooldowns()
    {
        if (caster == null) return;
        for (int i = 0; i < 4; i++)
        {
            if (cooldownOverlays[i] != null)
                cooldownOverlays[i].fillAmount = caster.GetCooldownFraction(i);
        }
    }

    void RefreshHeldTint()
    {
        if (caster == null) return;
        for (int i = 0; i < 4; i++)
        {
            if (icons[i] == null) continue;
            icons[i].color = caster.HeldAbilityIndex == i ? Color.yellow : baseColors[i];
        }
    }

    // ── Spellbook panel ─────────────────────────────────────────────────────

    void HandleSpellbookToggle()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            spellbookOpen = !spellbookOpen;
            if (spellbookPanel != null)
                spellbookPanel.SetActive(spellbookOpen);

            if (!spellbookOpen)
                pendingSpellbookIndex = -1;

            // Unlock/relock cursor
            Cursor.lockState = spellbookOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible   = spellbookOpen;
        }
    }

    void HandlePendingEquip()
    {
        if (!spellbookOpen || pendingSpellbookIndex < 0) return;

        // ESC or RMB cancels pending selection
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            pendingSpellbookIndex = -1;
            RefreshSpellbookCards();
        }
    }

    void BuildSpellbookCards()
    {
        if (caster == null || spellbookPanel == null) return;

        int count = caster.spellbook.Length;
        spellbookCards  = new Image[count];
        spellbookLabels = new TextMeshProUGUI[count];

        // 4-column grid, taller cards to fit stat line
        int   cols   = 4;
        float cardW  = 130f;
        float cardH  = 96f;
        float gapX   = 10f;
        float gapY   = 10f;
        float startX = -(cols * (cardW + gapX) - gapX) / 2f + cardW / 2f;
        float startY = 60f;

        for (int i = 0; i < count; i++)
        {
            int capturedIndex = i;
            AbilityDef ab = caster.spellbook[i];

            // ── Card root ────────────────────────────────────────────────
            var cardGO = new GameObject("SpellCard_" + i, typeof(RectTransform), typeof(Image), typeof(Button));
            cardGO.transform.SetParent(spellbookPanel.transform, false);

            var rt = cardGO.GetComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(cardW, cardH);
            rt.anchoredPosition = new Vector2(startX + (i % cols) * (cardW + gapX),
                                              startY - (i / cols) * (cardH + gapY));

            Image bg = cardGO.GetComponent<Image>();
            bg.color = ColorNormal;
            spellbookCards[i] = bg;

            // ── Category color strip (left edge) ─────────────────────────
            var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(cardGO.transform, false);
            var srt = strip.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(0, 1);
            srt.sizeDelta = new Vector2(5f, 0); srt.anchoredPosition = Vector2.zero;
            strip.GetComponent<Image>().color = CategoryColor(ab.category);

            // ── Icon (top-left square) ────────────────────────────────────
            if (ab.icon != null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(cardGO.transform, false);
                var irt = iconGO.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.06f, 0.60f);
                irt.anchorMax = new Vector2(0.30f, 0.98f);
                irt.offsetMin = irt.offsetMax = Vector2.zero;
                iconGO.GetComponent<Image>().sprite = ab.icon;
            }

            // ── Ability name ──────────────────────────────────────────────
            float nameLeft = ab.icon != null ? 0.33f : 0.08f;
            var nameLbl = MakeLabel("Name", cardGO.transform,
                new Vector2(nameLeft, 0.60f), new Vector2(1f, 1f), 10.5f, FontStyles.Bold, Color.white);
            nameLbl.text      = ab.abilityName;
            spellbookLabels[i] = nameLbl;

            // ── Type badge (shape label) ──────────────────────────────────
            var typeLbl = MakeLabel("Type", cardGO.transform,
                new Vector2(0.08f, 0.36f), new Vector2(1f, 0.60f), 8.5f, FontStyles.Normal,
                CategoryColor(ab.category));
            typeLbl.text = ShapeLabel(ab.shape) + " · " + ab.category;

            // ── Stats line (damage + CD) ──────────────────────────────────
            string dmg = ab.damage > 0f
                ? (ab.maxChargeDamage > ab.damage
                    ? $"{ab.damage:0}–{ab.maxChargeDamage:0} dmg  "
                    : $"{ab.damage:0} dmg  ")
                : "";
            string cd = $"{ab.cooldown:0}s CD";
            var statsLbl = MakeLabel("Stats", cardGO.transform,
                new Vector2(0.08f, 0.08f), new Vector2(1f, 0.36f), 8f, FontStyles.Normal,
                new Color(0.75f, 0.75f, 0.80f));
            statsLbl.text = dmg + cd;

            // ── Click to select for equip ─────────────────────────────────
            var btn = cardGO.GetComponent<Button>();
            btn.onClick.AddListener(() => OnSpellCardClicked(capturedIndex));

            // ── Hover → tooltip ───────────────────────────────────────────
            var trigger = cardGO.AddComponent<EventTrigger>();
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(data =>
            {
                var pos = ((PointerEventData)data).position;
                AbilityTooltipUI.Instance?.Show(caster.spellbook[capturedIndex], pos);
            });
            trigger.triggers.Add(enterEntry);

            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => AbilityTooltipUI.Instance?.Hide());
            trigger.triggers.Add(exitEntry);
        }
    }

    static string ShapeLabel(AbilityShape shape)
    {
        switch (shape)
        {
            case AbilityShape.SkillShot:  return "Skill Shot";
            case AbilityShape.Cone:       return "Cone";
            case AbilityShape.Rectangle:  return "Line";
            default:                      return "AoE";
        }
    }

    static TextMeshProUGUI MakeLabel(string name, Transform parent,
        Vector2 anchMin, Vector2 anchMax, float size, FontStyles style, Color col)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchMin; rt.anchorMax = anchMax;
        rt.offsetMin = new Vector2(4f, 0f); rt.offsetMax = new Vector2(-4f, 0f);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.fontSize  = size;
        t.fontStyle = style;
        t.color     = col;
        t.alignment = TextAlignmentOptions.TopLeft;
        t.enableWordWrapping = false;
        return t;
    }

    void OnSpellCardClicked(int spellbookIndex)
    {
        pendingSpellbookIndex = spellbookIndex;
        RefreshSpellbookCards();
    }

    void RefreshSpellbookCards()
    {
        if (spellbookCards == null || caster == null) return;

        for (int i = 0; i < spellbookCards.Length; i++)
        {
            if (spellbookCards[i] == null) continue;

            if (i == pendingSpellbookIndex)
            {
                spellbookCards[i].color = ColorPendingCard;
            }
            else if (caster.IsEquipped(i, out int slot))
            {
                spellbookCards[i].color = ColorEquipped;
                if (spellbookLabels[i] != null)
                    spellbookLabels[i].text = caster.spellbook[i].abilityName + " [" + (slot + 1) + "]";
            }
            else
            {
                spellbookCards[i].color = ColorNormal;
                if (spellbookLabels[i] != null)
                    spellbookLabels[i].text = caster.spellbook[i].abilityName;
            }
        }
    }

    Color CategoryColor(AbilityCategory cat)
    {
        switch (cat)
        {
            case AbilityCategory.Heal:    return ColorHeal;
            case AbilityCategory.Support: return ColorSupport;
            default:                      return ColorDamage;
        }
    }
}
