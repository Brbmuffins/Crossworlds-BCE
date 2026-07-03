using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

/// <summary>
/// CraftingUI — F key toggle (or call Open() from forge NPC trigger).
/// Fetches GET /api/recipes?profession=mining
/// Renders recipe list with ingredient requirements.
/// Click craft → POST /api/craft → shows result.
///
/// Self-bootstrapping. No scene object required.
/// Switch profession tab by calling SetProfession(id).
/// </summary>
public class CraftingUI : MonoBehaviour
{
    // ── Bootstrap ─────────────────────────────────────────────────────────────
    public static CraftingUI Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[CraftingUI]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<CraftingUI>();
    }

    // ── State ─────────────────────────────────────────────────────────────────
    int    _currentProfession = 1; // 1 = mining
    bool   _open;
    bool   _loading;
    List<RecipeData> _recipes = new List<RecipeData>();

    // ── UI refs ───────────────────────────────────────────────────────────────
    Canvas          _canvas;
    GameObject      _panel;
    Transform       _recipeListRoot;
    TextMeshProUGUI _statusText;
    TextMeshProUGUI _resultText;
    GameObject      _selectedHighlight;
    int             _selectedRecipeId = -1;
    Button          _craftBtn;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake() { BuildUI(); _panel.SetActive(false); }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        if (kb.fKey.wasPressedThisFrame && !AnyInputFocused())
            Toggle();
    }

    // ── Public ────────────────────────────────────────────────────────────────
    public void Open(int professionId = 1)
    {
        _currentProfession = professionId;
        _open = true;
        _panel.SetActive(true);
        StartCoroutine(FetchRecipes());
    }

    public void SetProfession(int id) { _currentProfession = id; StartCoroutine(FetchRecipes()); }

    // ── Toggle ────────────────────────────────────────────────────────────────
    void Toggle()
    {
        _open = !_open;
        _panel.SetActive(_open);
        if (_open) StartCoroutine(FetchRecipes());
    }

    // ── API: Fetch recipes ────────────────────────────────────────────────────
    IEnumerator FetchRecipes()
    {
        if (_loading) yield break;
        _loading = true;
        SetStatus("Loading recipes...");
        ClearRecipeList();

        string ip    = PlayerPrefs.GetString("serverIP", "localhost");
        string token = PlayerPrefs.GetString("jwt_token", "");
        string url   = $"http://{ip}:3000/api/recipes?profession={_currentProfession}";

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        _loading = false;

        if (req.result != UnityWebRequest.Result.Success)
        {
            SetStatus($"Error: {req.error}");
            yield break;
        }

        try
        {
            var resp = JsonUtility.FromJson<RecipeListResponse>(req.downloadHandler.text);
            if (resp.success)
            {
                _recipes = new List<RecipeData>(resp.data ?? new RecipeData[0]);
                BuildRecipeList();
                SetStatus(_recipes.Count == 0 ? "No recipes available." : "");
            }
            else SetStatus($"Server: {resp.error}");
        }
        catch (Exception e) { SetStatus($"Parse error: {e.Message}"); }
    }

    // ── API: Craft ────────────────────────────────────────────────────────────
    IEnumerator PostCraft(int recipeId)
    {
        string charId = GetCharacterId();
        if (string.IsNullOrEmpty(charId)) { SetResult("No character loaded."); yield break; }

        string ip    = PlayerPrefs.GetString("serverIP", "localhost");
        string token = PlayerPrefs.GetString("jwt_token", "");
        string url   = $"http://{ip}:3000/api/craft";
        string body  = $"{{\"characterId\":{charId},\"recipeId\":{recipeId}}}";

        _craftBtn.interactable = false;
        SetResult("Crafting...");

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        _craftBtn.interactable = true;

        if (req.result != UnityWebRequest.Result.Success)
        {
            SetResult($"Failed: {req.error}");
            yield break;
        }

        try
        {
            var resp = JsonUtility.FromJson<CraftResponse>(req.downloadHandler.text);
            if (resp.success)
            {
                SetResult($"✓ Crafted {resp.data?.result_item_id ?? "item"}!");
                // Refresh inventory so the new item shows up
                InventoryBagUI.Refresh();
            }
            else SetResult($"✗ {resp.error}");
        }
        catch (Exception e) { SetResult($"Parse error: {e.Message}"); }
    }

    // ── Render recipe list ────────────────────────────────────────────────────
    void BuildRecipeList()
    {
        ClearRecipeList();
        _selectedRecipeId = -1;

        foreach (var recipe in _recipes)
        {
            var row = BuildRecipeRow(recipe);
            row.transform.SetParent(_recipeListRoot, false);
        }

        _craftBtn.interactable = false;
    }

    GameObject BuildRecipeRow(RecipeData recipe)
    {
        var row = new GameObject($"Recipe_{recipe.id}", typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 60f);

        var img = row.GetComponent<Image>();
        img.color = new Color(0.10f, 0.07f, 0.20f, 1f);

        var btn = row.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(0.18f, 0.13f, 0.36f);
        colors.pressedColor     = new Color(0.3f, 0.2f, 0.6f);
        btn.colors = colors;
        int capturedId = recipe.id;
        btn.onClick.AddListener(() => SelectRecipe(capturedId, img));

        // Result item name
        var nameGO = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGO.transform.SetParent(row.transform, false);
        var nameT = nameGO.GetComponent<TextMeshProUGUI>();
        nameT.text      = FormatItemName(recipe.result_item_id);
        nameT.fontSize  = 11f;
        nameT.fontStyle = FontStyles.Bold;
        nameT.color     = Color.white;
        nameT.alignment = TextAlignmentOptions.TopLeft;
        var nRt = nameGO.GetComponent<RectTransform>();
        nRt.anchorMin = new Vector2(0f, 0.5f); nRt.anchorMax = Vector2.one;
        nRt.offsetMin = new Vector2(8f, 0f); nRt.offsetMax = new Vector2(-8f, -4f);

        // Ingredients
        var ingGO = new GameObject("Ingredients", typeof(RectTransform), typeof(TextMeshProUGUI));
        ingGO.transform.SetParent(row.transform, false);
        var ingT = ingGO.GetComponent<TextMeshProUGUI>();
        ingT.text      = BuildIngredientString(recipe.ingredients);
        ingT.fontSize  = 9f;
        ingT.color     = new Color(0.6f, 0.6f, 0.8f);
        ingT.alignment = TextAlignmentOptions.TopLeft;
        var iRt = ingGO.GetComponent<RectTransform>();
        iRt.anchorMin = Vector2.zero; iRt.anchorMax = new Vector2(1f, 0.5f);
        iRt.offsetMin = new Vector2(8f, 4f); iRt.offsetMax = new Vector2(-8f, 0f);

        // Skill level req
        if (recipe.skill_level_required > 0)
        {
            var reqGO = new GameObject("SkillReq", typeof(RectTransform), typeof(TextMeshProUGUI));
            reqGO.transform.SetParent(row.transform, false);
            var reqT = reqGO.GetComponent<TextMeshProUGUI>();
            reqT.text      = $"Skill {recipe.skill_level_required}";
            reqT.fontSize  = 9f;
            reqT.color     = new Color(0.9f, 0.6f, 0.2f);
            reqT.alignment = TextAlignmentOptions.Right;
            var rRt = reqGO.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.5f, 0.5f); rRt.anchorMax = Vector2.one;
            rRt.offsetMin = new Vector2(0f, 0f); rRt.offsetMax = new Vector2(-8f, -4f);
        }

        return row;
    }

    void SelectRecipe(int recipeId, Image rowImage)
    {
        _selectedRecipeId = recipeId;
        _craftBtn.interactable = true;
        SetResult("");
        // Could highlight selected row — simple approach: store and recolour
    }

    void ClearRecipeList()
    {
        foreach (Transform child in _recipeListRoot)
            Destroy(child.gameObject);
    }

    // ── Status / result text helpers ──────────────────────────────────────────
    void SetStatus(string msg) { if (_statusText) _statusText.text = msg; }
    void SetResult(string msg) { if (_resultText) _resultText.text = msg; }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static string GetCharacterId()
    {
        foreach (var id in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (id.isLocalPlayer) return id.characterId.ToString();
        return null;
    }

    static string FormatItemName(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return "Unknown";
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(itemId.Replace("_", " "));
    }

    static string BuildIngredientString(IngredientData[] ingredients)
    {
        if (ingredients == null || ingredients.Length == 0) return "No ingredients";
        var parts = new List<string>();
        foreach (var ing in ingredients)
            parts.Add($"{FormatItemName(ing.item_id)} ×{ing.quantity}");
        return string.Join("  |  ", parts);
    }

    static bool AnyInputFocused()
    {
        foreach (var f in FindObjectsByType<TMP_InputField>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (f.isFocused) return true;
        return false;
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void BuildUI()
    {
        var cgo = new GameObject("CraftingCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvas = cgo.GetComponent<Canvas>();
        _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 115;
        var scaler = cgo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        var root = _canvas.GetComponent<RectTransform>();

        // Panel — centre screen
        _panel = new GameObject("CraftPanel", typeof(RectTransform), typeof(Image));
        _panel.transform.SetParent(root, false);
        _panel.GetComponent<Image>().color = new Color(0.04f, 0.03f, 0.10f, 0.97f);
        var pRt = _panel.GetComponent<RectTransform>();
        pRt.anchorMin = pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot     = new Vector2(0.5f, 0.5f);
        pRt.sizeDelta = new Vector2(420f, 500f);

        // Title bar
        var titleBar = Child("TitleBar", pRt, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -40f), Vector2.zero);
        titleBar.AddComponent<Image>().color = new Color(0.08f, 0.05f, 0.20f, 1f);
        var tT = TextChild("Title", titleBar.GetComponent<RectTransform>(),
            new Vector2(8f, 0f), new Vector2(-8f, 0f));
        tT.text      = "CRAFTING  <size=8><color=#475569>F to close</color></size>";
        tT.fontSize  = 12f;
        tT.fontStyle = FontStyles.Bold;
        tT.color     = new Color(0.7f, 0.6f, 1f);
        tT.alignment = TextAlignmentOptions.Left;

        // Status text
        _statusText = TextChild("Status",
            Child("StatusArea", pRt, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -58f), new Vector2(0f, -40f)).GetComponent<RectTransform>(),
            new Vector2(8f, 0f), new Vector2(-8f, 0f));
        _statusText.fontSize  = 10f;
        _statusText.color     = new Color(0.5f, 0.5f, 0.7f);
        _statusText.alignment = TextAlignmentOptions.Left;

        // Scroll area for recipe list
        var scrollGO = new GameObject("RecipeScroll",
            typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGO.transform.SetParent(pRt, false);
        scrollGO.GetComponent<Image>().color = Color.clear;
        var sRt = scrollGO.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0f, 0f); sRt.anchorMax = new Vector2(1f, 1f);
        sRt.offsetMin = new Vector2(8f, 80f); sRt.offsetMax = new Vector2(-8f, -58f);

        var scroll = scrollGO.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.scrollSensitivity = 20f;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(sRt, false);
        viewport.GetComponent<Image>().color = Color.clear;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        var vpRt = viewport.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = vpRt.offsetMax = Vector2.zero;
        scroll.viewport = vpRt;

        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(vpRt, false);
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f); contentRt.anchorMax = Vector2.one;
        contentRt.pivot     = new Vector2(0.5f, 1f);
        contentRt.offsetMin = contentRt.offsetMax = Vector2.zero;
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing          = 4f;
        vlg.padding          = new RectOffset(0, 0, 4, 4);
        vlg.childControlWidth  = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRt;
        _recipeListRoot = content.transform;

        // Bottom bar — craft button + result
        var bottomBar = Child("BottomBar", pRt, Vector2.zero, new Vector2(1f, 0f),
            new Vector2(8f, 8f), new Vector2(-8f, 72f));

        _resultText = TextChild("Result", bottomBar.GetComponent<RectTransform>(),
            new Vector2(0f, 26f), new Vector2(0f, 0f));
        _resultText.fontSize  = 11f;
        _resultText.color     = new Color(0.4f, 1f, 0.5f);
        _resultText.alignment = TextAlignmentOptions.Center;
        var rrRt = _resultText.GetComponent<RectTransform>();
        rrRt.anchorMin = new Vector2(0f, 1f); rrRt.anchorMax = Vector2.one;
        rrRt.offsetMin = new Vector2(0f, -24f); rrRt.offsetMax = Vector2.zero;

        // Craft button
        var btnGO = new GameObject("CraftBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(bottomBar.transform, false);
        var btnRt = btnGO.GetComponent<RectTransform>();
        btnRt.anchorMin = Vector2.zero; btnRt.anchorMax = new Vector2(1f, 0f);
        btnRt.offsetMin = Vector2.zero; btnRt.offsetMax = new Vector2(0f, 26f);
        btnGO.GetComponent<Image>().color = new Color(0.3f, 0.18f, 0.72f);
        _craftBtn = btnGO.GetComponent<Button>();
        _craftBtn.interactable = false;
        _craftBtn.onClick.AddListener(() =>
        {
            if (_selectedRecipeId >= 0) StartCoroutine(PostCraft(_selectedRecipeId));
        });
        var btnT = TextChild("BtnLabel", btnRt, Vector2.zero, Vector2.zero);
        btnT.text      = "CRAFT";
        btnT.fontSize  = 12f;
        btnT.fontStyle = FontStyles.Bold;
        btnT.color     = Color.white;
        btnT.alignment = TextAlignmentOptions.Center;
        var btRt = btnT.GetComponent<RectTransform>();
        btRt.anchorMin = Vector2.zero; btRt.anchorMax = Vector2.one;
        btRt.offsetMin = btRt.offsetMax = Vector2.zero;
    }

    static GameObject Child(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        return go;
    }

    static TextMeshProUGUI TextChild(string name, RectTransform parent,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = offsetMin;   rt.offsetMax = offsetMax;
        return go.GetComponent<TextMeshProUGUI>();
    }

    // ── JSON shapes ───────────────────────────────────────────────────────────
    [Serializable] class RecipeListResponse
    {
        public bool         success;
        public string       error;
        public RecipeData[] data;
    }

    [Serializable] class RecipeData
    {
        public int             id;
        public int             profession_id;
        public int             skill_level_required;
        public string          result_item_id;
        public IngredientData[] ingredients;
    }

    [Serializable] class IngredientData
    {
        public string item_id;
        public int    quantity;
    }

    [Serializable] class CraftResponse
    {
        public bool       success;
        public string     error;
        public CraftResult data;
    }

    [Serializable] class CraftResult
    {
        public string result_item_id;
    }
}
