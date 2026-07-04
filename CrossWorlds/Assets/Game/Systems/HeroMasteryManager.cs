using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

/// <summary>
/// HeroMasteryManager — Singleton. Loads per-hero mastery data and awards mastery XP.
///
/// Self-bootstrapping — no Inspector setup required.
/// Call LoadMastery(charId) after login. Call AwardMasteryXp(heroClass, amount) on kill.
/// Subscribe to OnMasteryLevelUp for cosmetic feedback.
///
/// API:
///   GET  /api/mastery/:characterId   — {success, data:[{hero_class, mastery_level, mastery_xp}]}
///   POST /api/mastery/award          — {characterId, heroClass, xpAmount}
///
/// Hero classes: 0=Warden, 1=Ironclad, 2=Shadowblade, 3=Cleric, 4=Arcanist
/// </summary>
public class HeroMasteryManager : MonoBehaviour
{
    public static HeroMasteryManager Instance { get; private set; }

    /// Fired when a hero's mastery level increases: (heroClassIndex, newLevel)
    public static event System.Action<int, int> OnMasteryLevelUp;

    [System.Serializable]
    public class MasteryRow
    {
        public int hero_class;
        public int mastery_level;
        public int mastery_xp;
    }

    private readonly Dictionary<int, MasteryRow> _mastery = new Dictionary<int, MasteryRow>();
    public bool IsLoaded { get; private set; }

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[HeroMasteryManager]");
        DontDestroyOnLoad(go);
        go.AddComponent<HeroMasteryManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // ─── Public API ───────────────────────────────────────────────────────────
    public MasteryRow GetMastery(int heroClass) =>
        _mastery.TryGetValue(heroClass, out var d) ? d : null;

    public int GetLevel(int heroClass) => GetMastery(heroClass)?.mastery_level ?? 0;

    /// <summary>Award mastery XP for the given hero class. Call on each enemy kill.</summary>
    public void AwardMasteryXp(int heroClassIndex, int xpAmount)
    {
        int charId = PlayerPrefs.GetInt("CharacterId", 0);
        if (charId <= 0) { Debug.LogWarning("[MASTERY] AwardMasteryXp: no CharacterId in PlayerPrefs"); return; }
        StartCoroutine(PostAward(charId, heroClassIndex, xpAmount));
    }

    public IEnumerator LoadMastery(int characterId)
    {
        string ip    = PlayerPrefs.GetString("serverIP", "15.204.243.36");
        string token = PlayerPrefs.GetString("jwt_token", "");
        string url   = $"http://{ip}:3000/api/mastery/{characterId}";

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[MASTERY] Load failed: {req.error}");
            yield break;
        }

        var response = JsonUtility.FromJson<MasteryListResponse>(req.downloadHandler.text);
        if (response == null || !response.success)
        {
            Debug.LogError($"[MASTERY] Server error: {response?.error}");
            yield break;
        }

        _mastery.Clear();
        if (response.data != null)
            foreach (var m in response.data)
                _mastery[m.hero_class] = m;

        IsLoaded = true;
        Debug.Log($"[MASTERY] Loaded mastery for {_mastery.Count} heroes (char#{characterId})");
    }

    // ─── Post Award ───────────────────────────────────────────────────────────
    IEnumerator PostAward(int charId, int heroClass, int xpAmount)
    {
        string ip    = PlayerPrefs.GetString("serverIP", "15.204.243.36");
        string token = PlayerPrefs.GetString("jwt_token", "");

        string json = $"{{"characterId":{charId},"heroClass":{heroClass},"xpAmount":{xpAmount}}}";
        using var req = new UnityWebRequest($"http://{ip}:3000/api/mastery/award", "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[MASTERY] Award POST failed: {req.error}");
            yield break;
        }

        var response = JsonUtility.FromJson<MasteryAwardResponse>(req.downloadHandler.text);
        if (response?.success != true || response.data == null) yield break;

        // Update local cache and fire level-up event if applicable
        if (_mastery.TryGetValue(heroClass, out var local))
        {
            int oldLevel = local.mastery_level;
            local.mastery_xp    = response.data.mastery_xp;
            local.mastery_level = response.data.mastery_level;
            if (response.data.mastery_level > oldLevel)
            {
                Debug.Log($"[MASTERY] Hero {heroClass} reached mastery level {response.data.mastery_level}!");
                OnMasteryLevelUp?.Invoke(heroClass, response.data.mastery_level);
            }
        }
        else
        {
            // First time seeing this hero class
            _mastery[heroClass] = response.data;
        }
    }

    // ─── JSON shapes ──────────────────────────────────────────────────────────
    [System.Serializable] class MasteryListResponse  { public bool success; public List<MasteryRow> data; public string error; }
    [System.Serializable] class MasteryAwardResponse { public bool success; public MasteryRow data; public string error; }
}
