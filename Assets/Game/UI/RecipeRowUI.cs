#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Runtime presentation and interaction for one server-provided Forge recipe.</summary>
public sealed class RecipeRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Labels")]
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI ingredientsLabel;
    public TextMeshProUGUI timeLabel;
    public Button craftButton;
    public Image rarityBadge;
    public Image icon;

    string _itemId;

    static readonly Color ColourLocked = new(0.4f, 0.4f, 0.4f, 0.5f);

    public void Populate(ForgeCraftingPanel.RecipeData recipe, System.Action<ForgeCraftingPanel.RecipeData> onCraft)
    {
        nameLabel.text = recipe.result_name;
        levelLabel.text = $"Lv {recipe.skill_level_required}";
        timeLabel.text = $"{recipe.craft_time_seconds:0.#}s";

        _itemId = recipe.result_item_id;
        var definition = LootItemCatalog.Find(_itemId);
        if (icon != null)
        {
            icon.sprite = definition != null ? definition.inventoryIcon : null;
            icon.preserveAspect = true;
            icon.color = icon.sprite != null ? Color.white : Color.clear;
        }

        bool hasIngredients = true;
        var ingredientParts = new List<string>();
        if (recipe.ingredients != null)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                int have = InventoryManager.Instance?.GetItemCount(ingredient.item_id) ?? 0;
                string colour = have >= ingredient.quantity ? "white" : "red";
                hasIngredients &= have >= ingredient.quantity;
                ingredientParts.Add($"<color={colour}>{ingredient.quantity}× {ingredient.name}</color>");
            }
        }
        ingredientsLabel.text = string.Join("  ", ingredientParts);

        if (rarityBadge != null)
        {
            ItemRarityUtility.TryParse(recipe.result_rarity, out ItemRarity rarity);
            rarityBadge.color = ItemRarityUtility.Color(rarity);
        }

        var group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        craftButton.onClick.RemoveAllListeners();

        if (recipe.unlocked && hasIngredients)
        {
            craftButton.interactable = true;
            craftButton.onClick.AddListener(() => onCraft(recipe));
            group.alpha = 1f;
        }
        else
        {
            craftButton.interactable = false;
            group.alpha = 0.45f;
            nameLabel.color = ColourLocked;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(_itemId)) ItemTooltipUI.Instance?.Show(_itemId, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData) => ItemTooltipUI.Instance?.Hide();
}
#endif
