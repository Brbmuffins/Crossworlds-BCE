#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Forge NPC crafting panel — two tabs: Smelt and Craft.
///
/// Smelt tab: converts raw gathering materials → refined materials (2s per action).
///   Recipes sourced from GET /api/professions/recipes/:characterId (recipe_type = "smelt")
///
/// Craft tab: converts refined materials → consumables and gear.
///   Recipes sourced from same endpoint (recipe_type = "craft")
///
/// Locked recipes shown greyed out with level requirement. Available recipes
/// show ingredient counts in red if inventory is short.
///
/// Wire in Inspector:
///   smeltTab / craftTab     — the Tab button GameObjects
///   smeltContent / craftContent — ScrollRect content transforms
///   recipeRowPrefab         — prefab with RecipeRowUI component
///   progressBar             — Slider shown during craft_time_seconds animation
///   progressLabel           — TMP showing "Smelting Copper Ingot…"
///   closeButton             — closes the panel
/// </summary>
public class ForgeCraftingPanel : MonoBehaviour
{
    [Header("Tabs")]
    public Button smeltTabButton;
    public Button craftTabButton;
    public GameObject smeltContent;
    public GameObject craftContent;

    [Header("Recipe List")]
    public Transform smeltListParent;
    public Transform craftListParent;
    public GameObject recipeRowPrefab;

    [Header("Progress")]
    public GameObject progressOverlay;
    public Slider     progressBar;
    public TextMeshProUGUI progressLabel;

    [Header("Navigation")]
    public Button closeButton;
    public TextMeshProUGUI statusLabel;

    public static ForgeCraftingPanel Instance { get; private set; }

    bool _smeltTabActive = true;
    bool _crafting       = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var prefab = Resources.Load<GameObject>("Forge/ForgeWindow");
        if (prefab == null)
        {
            Debug.LogError("[FORGE] Resources/Forge/ForgeWindow.prefab is missing. Run BCE/Setup/Rebuild Forge Crafting UI.");
            return;
        }

        var window = Instantiate(prefab);
        DontDestroyOnLoad(window);
        window.SetActive(false);
    }

    void Awake()
    {
        Instance = this;
        // Null-guarded so a partially-wired panel still registers Instance rather
        // than throwing in Awake (its Inspector fields are an editor step).
        if (smeltTabButton  != null) smeltTabButton.onClick.AddListener(() => ShowTab(true));
        if (craftTabButton  != null) craftTabButton.onClick.AddListener(() => ShowTab(false));
        if (closeButton     != null) closeButton.onClick.AddListener(Close);
        if (progressOverlay != null) progressOverlay.SetActive(false);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    void Update()
    {
        if (!_crafting && Keyboard.current?.escapeKey.wasPressedThisFrame == true && !IsInputFocused())
            Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        GetComponentInChildren<ForgeWindowDragHandle>(true)?.ApplySavedPosition();
        ShowTab(true);
        SetStatus("Loading recipes…");
        StartCoroutine(LoadRecipes());
    }

    public void Close()
    {
        if (_crafting) return; // don't close mid-craft
        gameObject.SetActive(false);
    }

    void ShowTab(bool smelt)
    {
        _smeltTabActive = smelt;
        if (smeltContent != null) smeltContent.SetActive(smelt);
        if (craftContent != null) craftContent.SetActive(!smelt);
    }

    // ── Load recipes from server ──────────────────────────────────────────────

    IEnumerator LoadRecipes()
    {
        int    charId = AuthManager.CharacterId;
        string token  = AuthManager.Token;
        if (charId <= 0 || string.IsNullOrEmpty(token))
        {
            SetStatus("Select a character before using the Forge.");
            yield break;
        }
        string url    = $"{ServerConfig.AuthBaseUrl}/api/professions/recipes/{charId}";

        using var req = UnityEngine.Networking.UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[FORGE] Failed to load recipes: {req.error}");
            SetStatus(ReadError(req.downloadHandler.text, "Unable to load recipes."));
            yield break;
        }

        var json = JsonUtility.FromJson<RecipesResponse>(req.downloadHandler.text);
        if (json?.success != true || json.data == null)
        {
            SetStatus("Unable to load recipes.");
            yield break;
        }

        if (smeltListParent == null || craftListParent == null || recipeRowPrefab == null)
        {
            Debug.LogError("[FORGE] ForgeWindow prefab references are incomplete. Rebuild it from BCE/Setup/Rebuild Forge Crafting UI.");
            SetStatus("Forge interface setup is incomplete.");
            yield break;
        }

        // Clear old rows
        foreach (Transform t in smeltListParent) Destroy(t.gameObject);
        foreach (Transform t in craftListParent) Destroy(t.gameObject);

        PopulateList(json.data.smelt, smeltListParent);
        PopulateList(json.data.craft, craftListParent);
        SetStatus($"{json.data.smelt?.Length ?? 0} smelting and {json.data.craft?.Length ?? 0} crafting recipes available.");
    }

    void PopulateList(RecipeData[] recipes, Transform parent)
    {
        if (recipes == null) return;
        foreach (var recipe in recipes)
        {
            var row = Instantiate(recipeRowPrefab, parent).GetComponent<RecipeRowUI>();
            if (row != null) row.Populate(recipe, OnCraftClicked);
        }
    }

    // ── Craft ─────────────────────────────────────────────────────────────────

    void OnCraftClicked(RecipeData recipe)
    {
        if (_crafting) return;
        StartCoroutine(CraftRoutine(recipe));
    }

    IEnumerator CraftRoutine(RecipeData recipe)
    {
        _crafting = true;
        SetStatus("");
        progressOverlay.SetActive(true);
        progressLabel.text = $"{(recipe.recipe_type == "smelt" ? "Smelting" : "Crafting")} {recipe.result_name}…";
        progressBar.value  = 0f;

        float elapsed  = 0f;
        float duration = Mathf.Max(0.1f, recipe.craft_time_seconds);

        while (elapsed < duration)
        {
            elapsed           += Time.deltaTime;
            progressBar.value  = elapsed / duration;
            yield return null;
        }

        // POST /api/craft
        int    charId = AuthManager.CharacterId;
        string token  = AuthManager.Token;
        string body   = JsonUtility.ToJson(new CraftRequest { characterId = charId, recipeId = recipe.recipe_id });

        using var req = new UnityEngine.Networking.UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/craft", "POST");
        req.uploadHandler   = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        progressOverlay.SetActive(false);
        _crafting = false;

        if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            var resp = JsonUtility.FromJson<CraftResponse>(req.downloadHandler.text);
            if (resp?.success == true && resp.data != null)
            {
                RodChatManager.Instance?.AddSystemMessage(
                    $"[FORGE] Crafted {resp.data.result_name}!" +
                    (resp.data.leveled_up ? $" {GetProfessionName(recipe.profession_id)} leveled up to {resp.data.skill_level}!" : ""));
                var inv = InventoryManager.Instance;
                if (inv != null) yield return inv.LoadInventory();
                InventoryBagUI.Refresh();
                SetStatus($"Crafted {resp.data.result_name}.");
                StartCoroutine(LoadRecipes()); // refresh ingredient counts
            }
            else
            {
                // API convention: error strings are player-readable, show verbatim
                RodChatManager.Instance?.AddSystemMessage(
                    $"[FORGE] {(string.IsNullOrEmpty(resp?.error) ? "Craft failed" : resp.error)}");
                SetStatus(string.IsNullOrEmpty(resp?.error) ? "Craft failed." : resp.error);
            }
        }
        else
        {
            string error = ReadError(req.downloadHandler.text, "Server error — try again.");
            RodChatManager.Instance?.AddSystemMessage($"[FORGE] {error}");
            SetStatus(error);
        }
    }

    void SetStatus(string value)
    {
        if (statusLabel != null) statusLabel.text = value ?? "";
    }

    static string ReadError(string json, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            var response = JsonUtility.FromJson<ErrorResponse>(json);
            if (!string.IsNullOrWhiteSpace(response?.error)) return response.error;
        }
        return fallback;
    }

    static bool IsInputFocused()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        return selected != null && selected.GetComponent<TMP_InputField>() != null;
    }

    static string GetProfessionName(string id) =>
        ProfessionManager.TryFromWireId(id, out int professionId)
            ? ProfessionManager.ProfessionNames[professionId]
            : "Profession";

    // ── JSON shapes ───────────────────────────────────────────────────────────

    [System.Serializable] class RecipesResponse
    {
        public bool        success;
        public RecipesData data;
    }
    [System.Serializable] class RecipesData
    {
        public RecipeData[] smelt;
        public RecipeData[] craft;
    }
    [System.Serializable] public class RecipeData
    {
        public string recipe_id;
        public string profession_id;
        public int    skill_level_required;
        public string result_item_id;
        public string result_name;
        public string result_rarity;
        public string recipe_type;
        public float  craft_time_seconds;
        public bool   unlocked;
        public IngredientData[] ingredients;
    }
    [System.Serializable] public class IngredientData
    {
        public string item_id;
        public string name;
        public int    quantity;
    }
    [System.Serializable] class CraftRequest
    {
        public int characterId;
        public string recipeId;
    }
    [System.Serializable] class CraftResponse
    {
        public bool        success;
        public CraftResult data;
        public string      error;
    }
    [System.Serializable] class ErrorResponse { public string error; }
    [System.Serializable] class CraftResult
    {
        public string result_name;
        public bool   leveled_up;
        public int    skill_level;
        public int    xp_gained;
    }
}

#endif
