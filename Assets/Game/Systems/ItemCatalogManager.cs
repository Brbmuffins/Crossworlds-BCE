#if !UNITY_SERVER
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ItemCatalogManager — self-bootstrapping singleton.
/// Fetches GET /items (old item_template endpoint) on startup, caches for UI lookup.
/// Also exposes rarity helpers for new-system item_ids (parsed by naming convention).
///
/// Usage:
///   ItemCatalogManager.Instance.GetTemplate("sword_copper") → ItemTemplate
///   ItemCatalogManager.GetRarityColor("sword_iron") → Color
/// </summary>
public class ItemCatalogManager : MonoBehaviour
{
    public static ItemCatalogManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[ItemCatalogManager]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<ItemCatalogManager>();
    }

    // ── Data ──────────────────────────────────────────────────────────────────
    [Serializable]
    public class ItemTemplate
    {
        public int    id;
        public string name;
        public string item_type;
        public int    stat_str;
        public int    stat_agi;
        public int    stat_int;
        public int    stat_vit;
        public int    sell_value;
    }

    readonly Dictionary<int,    ItemTemplate> _byId   = new();
    readonly Dictionary<string, ItemTemplate> _byName = new();

    public bool IsLoaded { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => StartCoroutine(FetchCatalog());

    // ── Public API ────────────────────────────────────────────────────────────
    public ItemTemplate GetTemplate(int id)     => _byId.TryGetValue(id, out var t)    ? t : null;
    public ItemTemplate GetTemplate(string name)=> _byName.TryGetValue(name, out var t) ? t : null;

    /// <summary>
    /// Returns a display-friendly name for a new-system item_id string.
    /// Falls back to formatting the id itself if not in catalog.
    /// </summary>
    public static string GetDisplayName(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return "Unknown";
        if (Instance != null)
        {
            var t = Instance.GetTemplate(itemId);
            if (t != null) return t.name;
        }
        // Format snake_case → Title Case
        var parts = itemId.Split('_');
        for (int i = 0; i < parts.Length; i++)
            if (parts[i].Length > 0)
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
        return string.Join(" ", parts);
    }

    /// <summary>Rarity colour for new-system item_id strings (parsed by convention).</summary>
    public static Color GetRarityColor(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return Color.gray;
        if (itemId.Contains("epic"))      return new Color(0.7f, 0.1f, 1f);
        if (itemId.Contains("rare") || itemId.Contains("iron")) return new Color(0.2f, 0.5f, 1f);
        if (itemId.Contains("uncommon") || itemId.Contains("bar")) return new Color(0.2f, 0.9f, 0.2f);
        return Color.gray; // common
    }

    // ── Fetch ─────────────────────────────────────────────────────────────────
    IEnumerator FetchCatalog()
    {
        string ip  = PlayerPrefs.GetString("serverIP", "localhost");
        string url = $"http://{ip}:3000/items";

        using var req = UnityWebRequest.Get(url);
        req.timeout = 10;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[CATALOG] Fetch failed ({req.error}) — operating without catalog");
            yield break;
        }

        try
        {
            var resp = JsonUtility.FromJson<CatalogResponse>(req.downloadHandler.text);
            if (resp?.items != null)
            {
                foreach (var item in resp.items)
                {
                    if (item == null) continue;
                    _byId[item.id]     = item;
                    _byName[item.name] = item;
                }
                IsLoaded = true;
                Debug.Log($"[CATALOG] Loaded {_byId.Count} item templates");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[CATALOG] Parse error: {e.Message}");
        }
    }

    // ── JSON ──────────────────────────────────────────────────────────────────
    [Serializable] class CatalogResponse { public ItemTemplate[] items; }
}
#endif
