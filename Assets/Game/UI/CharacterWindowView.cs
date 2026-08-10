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

    public void Initialize(Action close, Action<CharacterEquipmentSlot> rightClick,
        Action<CharacterEquipmentSlot, PointerEventData> enter, Action exit,
        Action<CharacterEquipmentSlot, PointerEventData> beginDrag,
        Action<CharacterEquipmentSlot, PointerEventData> drag,
        Action<CharacterEquipmentSlot, PointerEventData> endDrag)
    {
        closeButton.onClick.AddListener(() => close());
        foreach (var equipment in equipmentSlots)
        {
            CharacterEquipmentSlot captured = equipment.slot;
            var trigger = equipment.button.gameObject.GetComponent<EventTrigger>() ?? equipment.button.gameObject.AddComponent<EventTrigger>();
            var onClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            onClick.callback.AddListener(data =>
            {
                var pointer = (PointerEventData)data;
                if (pointer.button == PointerEventData.InputButton.Right) rightClick(captured);
            });
            var onEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            onEnter.callback.AddListener(data => enter(captured, (PointerEventData)data));
            var onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            onExit.callback.AddListener(_ => exit());
            var onBeginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            onBeginDrag.callback.AddListener(data => beginDrag(captured, (PointerEventData)data));
            var onDrag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            onDrag.callback.AddListener(data => drag(captured, (PointerEventData)data));
            var onEndDrag = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            onEndDrag.callback.AddListener(data => endDrag(captured, (PointerEventData)data));
            trigger.triggers.Add(onClick);
            trigger.triggers.Add(onEnter);
            trigger.triggers.Add(onExit);
            trigger.triggers.Add(onBeginDrag);
            trigger.triggers.Add(onDrag);
            trigger.triggers.Add(onEndDrag);
        }
    }

    public bool TryGetSlotAt(Vector2 screenPosition, Camera eventCamera, out CharacterEquipmentSlot slot)
    {
        foreach (var equipment in equipmentSlots)
        {
            if (equipment.disabledOverlay != null && equipment.disabledOverlay.activeSelf) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    equipment.button.transform as RectTransform, screenPosition, eventCamera))
            {
                slot = equipment.slot;
                return true;
            }
        }
        slot = default;
        return false;
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
