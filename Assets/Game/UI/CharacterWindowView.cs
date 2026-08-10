#if UNITY_EDITOR || !UNITY_SERVER
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CharacterWindowView : MonoBehaviour
{
    [Serializable] public sealed class EquipmentSlotView
    {
        public CharacterEquipmentSlot slot;
        public Button button;
        public Image background;
        public Image icon;
        public TextMeshProUGUI label;
        public TextMeshProUGUI quantity;
        public GameObject disabledOverlay;
    }

    public Button closeButton;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerLevel;
    public TextMeshProUGUI className;
    public TextMeshProUGUI strValue;
    public TextMeshProUGUI agiValue;
    public TextMeshProUGUI intValue;
    public TextMeshProUGUI vitValue;
    public RawImage characterPreview;
    public TextMeshProUGUI[] combatValues;
    public EquipmentSlotView[] equipmentSlots;

    public void Initialize(Action close, Action<CharacterEquipmentSlot> click,
        Action<CharacterEquipmentSlot, PointerEventData> enter, Action exit)
    {
        closeButton.onClick.AddListener(() => close());
        foreach (var equipment in equipmentSlots)
        {
            CharacterEquipmentSlot captured = equipment.slot;
            equipment.button.onClick.AddListener(() => click(captured));
            var trigger = equipment.button.gameObject.GetComponent<EventTrigger>() ?? equipment.button.gameObject.AddComponent<EventTrigger>();
            var onEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            onEnter.callback.AddListener(data => enter(captured, (PointerEventData)data));
            var onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            onExit.callback.AddListener(_ => exit());
            trigger.triggers.Add(onEnter);
            trigger.triggers.Add(onExit);
        }
    }

    public void SetEquipment(CharacterEquipmentSlot slot, Sprite icon, int quantity, Color rarity, bool disabled)
    {
        var view = Array.Find(equipmentSlots, candidate => candidate.slot == slot);
        if (view == null) return;
        view.icon.sprite = icon;
        view.icon.preserveAspect = true;
        view.icon.color = icon != null ? Color.white : Color.clear;
        view.quantity.text = quantity > 1 ? quantity.ToString() : "";
        view.background.color = icon != null ? new Color32(30, 22, 35, 245) : new Color32(17, 14, 18, 235);
        var outline = view.background.GetComponent<Outline>();
        if (outline != null) outline.effectColor = icon != null ? rarity : new Color32(83, 65, 39, 180);
        view.button.interactable = !disabled && icon != null;
        if (view.disabledOverlay != null) view.disabledOverlay.SetActive(disabled);
    }
}
#endif
