#if UNITY_EDITOR || !UNITY_SERVER
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum InventoryFilter { All, Gear, Materials }

/// <summary>Serialized, prefab-owned inventory presentation. Contains no persistence logic.</summary>
public sealed class InventoryBagView : MonoBehaviour
{
    [Serializable] public sealed class Slot
    {
        public Button button;
        public Image background;
        public Image icon;
        public Image equippedMarker;
        public TextMeshProUGUI quantity;
        [NonSerialized] public EventTrigger trigger;
    }

    public Button closeButton;
    public Button allTab;
    public Button gearTab;
    public Button materialsTab;
    public Image allTabImage;
    public Image gearTabImage;
    public Image materialsTabImage;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI goldText;
    public Slot[] slots;

    static readonly Color ActiveTab = new Color32(92, 65, 25, 255);
    static readonly Color IdleTab = new Color32(232, 222, 195, 235);
    static readonly Color EmptySlot = new Color32(25, 38, 35, 185);
    static readonly Color FilledSlot = new Color32(40, 55, 48, 225);

    public void Initialize(Action close, Action<InventoryFilter> filter, Action<int> click,
        Action<int, PointerEventData> enter, Action exit,
        Action<int, PointerEventData> beginDrag,
        Action<int, PointerEventData> drag,
        Action<int, PointerEventData> endDrag)
    {
        closeButton.onClick.AddListener(() => close());
        allTab.onClick.AddListener(() => filter(InventoryFilter.All));
        gearTab.onClick.AddListener(() => filter(InventoryFilter.Gear));
        materialsTab.onClick.AddListener(() => filter(InventoryFilter.Materials));
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;
            slots[i].button.onClick.AddListener(() => click(index));
            var trigger = slots[i].button.gameObject.GetComponent<EventTrigger>() ?? slots[i].button.gameObject.AddComponent<EventTrigger>();
            slots[i].trigger = trigger;
            var onEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            onEnter.callback.AddListener(data => enter(index, (PointerEventData)data));
            var onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            onExit.callback.AddListener(_ => exit());
            var onBeginDrag = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
            onBeginDrag.callback.AddListener(data => beginDrag(index, (PointerEventData)data));
            var onDrag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
            onDrag.callback.AddListener(data => drag(index, (PointerEventData)data));
            var onEndDrag = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
            onEndDrag.callback.AddListener(data => endDrag(index, (PointerEventData)data));
            trigger.triggers.Add(onEnter);
            trigger.triggers.Add(onExit);
            trigger.triggers.Add(onBeginDrag);
            trigger.triggers.Add(onDrag);
            trigger.triggers.Add(onEndDrag);
        }
        SetActiveFilter(InventoryFilter.All);
    }

    public void SetSlot(int index, Sprite sprite, int count, bool equipped, Color rarity)
    {
        if (index < 0 || index >= slots.Length) return;
        var slot = slots[index];
        slot.icon.sprite = sprite;
        slot.icon.preserveAspect = true;
        slot.icon.color = sprite != null ? Color.white : (count > 0 ? rarity : Color.clear);
        slot.background.color = count > 0 ? FilledSlot : EmptySlot;
        slot.quantity.text = count > 1 ? count.ToString() : "";
        slot.equippedMarker.gameObject.SetActive(equipped);
    }

    public void SetActiveFilter(InventoryFilter filter)
    {
        allTabImage.color = filter == InventoryFilter.All ? ActiveTab : IdleTab;
        gearTabImage.color = filter == InventoryFilter.Gear ? ActiveTab : IdleTab;
        materialsTabImage.color = filter == InventoryFilter.Materials ? ActiveTab : IdleTab;
    }

    public void SetStatus(string value) => statusText.text = value ?? "";
    public void SetGold(int value) => goldText.text = value.ToString("N0");
}
#endif
