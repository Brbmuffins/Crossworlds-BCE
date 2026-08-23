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
        public Outline rarityOutline;
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
        if (count > 0)
        {
            Color neon = NeonRarity(rarity);
            slot.background.color = Color.Lerp(FilledSlot, neon, 0.16f);
            if (slot.rarityOutline != null)
            {
                neon.a = 0.96f;
                slot.rarityOutline.effectColor = neon;
                slot.rarityOutline.effectDistance = new Vector2(2f, -2f);
            }
        }
        else
        {
            slot.background.color = EmptySlot;
            if (slot.rarityOutline != null) slot.rarityOutline.effectColor = Color.clear;
        }
        slot.quantity.text = count > 1 ? count.ToString() : "";
        slot.equippedMarker.gameObject.SetActive(equipped);
    }

    static Color NeonRarity(Color rarity)
    {
        float max = Mathf.Max(rarity.r, Mathf.Max(rarity.g, rarity.b));
        float min = Mathf.Min(rarity.r, Mathf.Min(rarity.g, rarity.b));
        if (max - min < 0.18f) return new Color(1f, 1f, 1f, 1f);
        if (rarity.g > rarity.r && rarity.g > rarity.b) return new Color(0.2f, 1f, 0.3f, 1f);
        if (rarity.r > 0.9f && rarity.g < 0.2f && rarity.b < 0.3f) return new Color(1f, 0.05f, 0.18f, 1f);
        if (rarity.r > 0.9f && rarity.g > 0.2f && rarity.g < 0.75f && rarity.b < 0.3f) return new Color(1f, 0.48f, 0.04f, 1f);
        if (rarity.r > 0.45f && rarity.b > 0.65f) return new Color(0.82f, 0.16f, 1f, 1f);
        return new Color(0.12f, 0.55f, 1f, 1f);
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
