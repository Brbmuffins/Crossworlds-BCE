using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ItemCatalogManager — Singleton. Loads GET /api/items on startup.
/// Provides Dictionary lookups for name, rarity, stat_bonus, sell_value.
///
/// Self-bootstrapping — no Inspector setup required.
/// Subscribe to OnCatalogLoaded to know when data is available.
///
/// API: GET /api/items  (no auth — public endpoint)
/// </summary>
public class ItemCatalogManager : MonoBehaviour
{
    public static ItemCatalogManager Instance { get; private set; }
    public static event System.Action OnCatalogLoaded;

    [System.Serializable]
    public class ItemData
    {
        public string id;
        public string name;
        public string rarity;      // common / uncommon / rare / epic
        public string item_type;   // weapon / armor_head / armor_chest / armor_legs / ring / trinket / material
        public string stat_bonus;  // raw JSON string
        public string icon_id;
        public int    sell_value;
    }

    private readonly Dictionary<string, ItemData> _catalog = new Dictionary<string, ItemData>();
    public bool IsLoaded { get; private set; }

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[ItemCatalogManager]");
        DontDestroyOnLoad(go);
        go.AddComponent<ItemCatalogManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadCatalog());
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ─── Public API ───────────────────────────────────────────────────────────
    public ItemData Get(string itemId)
    {
        _catalog.TryGetValue(itemId ?? "", out var d);
        return d;
    }

    public string GetDisplayName(string itemId) => Get(itemId)?.name ?? itemId;
    public string GetRarity(string itemId)       => Get(itemId)?.rarity ?? "common";
    public int    GetSellValue(string itemId)    => Get(itemId)?.sell_value ?? 0;
    public void   Reload()                       => StartCoroutine(LoadCatalog());

    // ─── Load ─────────────────────────────────────────────────────────────────
    IEnumerator LoadCatalog()
    {
        string ip  = PlayerPrefs.GetString("serverIP", "15.204.243.36");
        string url = $"http://{ip}:3000/api/items";

        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[CATALOG] Failed to load item catalog: {req.error}");
            yield break;
        }

        var wrapper = JsonUtility.FromJson<CatalogResponse>(req.downloadHandler.text);
        if (wrapper == null || !wrapper.success)
        {
            Debug.LogError($"[CATALOG] Server error: {wrapper?.error}");
            yield break;
        }

        _catalog.Clear();
        if (wrapper.data != null)
            foreach (var item in wrapper.data)
                if (!string.IsNullOrEmpty(item.id))
                    _catalog[item.id] = item;

        IsLoaded = true;
        Debug.Log($"[CATALOG] Loaded {_catalog.Count} items from server");
        OnCatalogLoaded?.Invoke();
    }

    [System.Serializable]
    class CatalogResponse { public bool success; public List<ItemData> data; public string error; }
}
