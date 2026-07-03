using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// PlayerProgressManager — fetches and caches the local player's level/xp/gold/stats.
/// Other UI scripts (XpBar, CharacterSheetUI) read from PlayerProgressManager.Local.
///
/// Self-bootstrapping. Fetches on first local player found, then on scene load.
///
/// Save path: call SaveProgress() any time (level-up, scene exit, periodic).
/// API: GET /character (reuses existing endpoint — returns level, xp, gold, stats)
///      POST /api/character/save-progress
/// </summary>
public class PlayerProgressManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static PlayerProgressManager Local { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Local != null) return;
        var go = new GameObject("[PlayerProgressManager]");
        DontDestroyOnLoad(go);
        Local = go.AddComponent<PlayerProgressManager>();
    }

    // ── Data (read by UI) ─────────────────────────────────────────────────────
    public int   Level   { get; private set; } = 1;
    public int   Xp      { get; private set; } = 0;
    public int   XpToNext{ get; private set; } = 100;
    public int   Gold    { get; private set; } = 0;
    public int   StatStr { get; private set; } = 5;
    public int   StatAgi { get; private set; } = 5;
    public int   StatInt { get; private set; } = 5;
    public int   StatVit { get; private set; } = 5;

    public float XpFraction => XpToNext > 0 ? Mathf.Clamp01((float)Xp / XpToNext) : 1f;

    /// <summary>Local player's class index (0–4). Reads from PlayerIdentity.classIndex.</summary>
    public int ClassIndex
    {
        get
        {
            var id = FindLocalIdentity();
            return id != null ? id.classIndex : 0;
        }
    }

    // ── Events ────────────────────────────────────────────────────────────────
    public event Action OnDataRefreshed;   // any stat changed
    public event Action<int> OnLevelUp;    // fired with new level

    // ── Internal ──────────────────────────────────────────────────────────────
    int    _characterId = -1;
    string _jwt         = "";
    string _serverIP    = "localhost";
    bool   _fetching    = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()  { StartCoroutine(WaitForLocalPlayer()); }
    void OnEnable()  { UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    IEnumerator WaitForLocalPlayer()
    {
        // Wait up to 10s for local PlayerIdentity to appear
        float t = 0f;
        while (t < 10f)
        {
            var id = FindLocalIdentity();
            if (id != null && id.characterId > 0)
            {
                _characterId = id.characterId;
                _jwt         = PlayerPrefs.GetString("jwt_token", "");
                _serverIP    = PlayerPrefs.GetString("serverIP", "localhost");
                StartCoroutine(FetchProgress());
                yield break;
            }
            t += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Award XP locally and save. Call from EnemyController death loot.</summary>
    public void AwardXp(int amount)
    {
        int prevLevel = Level;
        Xp += amount;
        while (Xp >= XpToNext)
        {
            Xp -= XpToNext;
            Level++;
            XpToNext = XpForLevel(Level + 1);
            OnLevelUp?.Invoke(Level);
            Debug.Log($"[PROGRESS] Level up → {Level}");
        }
        OnDataRefreshed?.Invoke();
        if (Level != prevLevel) StartCoroutine(PostProgress());
    }

    /// <summary>Award gold locally and save.</summary>
    public void AwardGold(int amount)
    {
        Gold += amount;
        OnDataRefreshed?.Invoke();
        StartCoroutine(PostProgress());
    }

    /// <summary>Force a save to the server (call on scene exit).</summary>
    public void SaveProgress() => StartCoroutine(PostProgress());

    /// <summary>Force a refresh from server.</summary>
    public void Refresh() => StartCoroutine(FetchProgress());

    // ── Fetch from server ─────────────────────────────────────────────────────
    IEnumerator FetchProgress()
    {
        if (_fetching || _characterId < 0) yield break;
        _fetching = true;

        string url = $"http://{_serverIP}:3000/character";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {_jwt}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        _fetching = false;

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[PROGRESS] Fetch failed: {req.error}");
            yield break;
        }

        try
        {
            var r = JsonUtility.FromJson<CharacterFetchResponse>(req.downloadHandler.text);
            int prevLevel = Level;
            Level    = Mathf.Max(1, r.level);
            Xp       = r.xp;
            XpToNext = XpForLevel(Level + 1);
            Gold     = r.gold;
            StatStr  = r.stat_str;
            StatAgi  = r.stat_agi;
            StatInt  = r.stat_int;
            StatVit  = r.stat_vit;
            OnDataRefreshed?.Invoke();
            if (Level > prevLevel) OnLevelUp?.Invoke(Level);
            Debug.Log($"[PROGRESS] Loaded Lv{Level} {Xp}/{XpToNext}xp {Gold}g");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PROGRESS] Parse error: {e.Message}");
        }
    }

    // ── Save to server ────────────────────────────────────────────────────────
    IEnumerator PostProgress()
    {
        if (_characterId < 0) yield break;

        string url  = $"http://{_serverIP}:3000/api/character/save-progress";
        string body = JsonUtility.ToJson(new SaveProgressRequest
        {
            characterId = _characterId,
            level       = Level,
            xp          = Xp,
            gold        = Gold,
            stat_str    = StatStr,
            stat_agi    = StatAgi,
            stat_int    = StatInt,
            stat_vit    = StatVit,
        });

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {_jwt}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            Debug.Log($"[PROGRESS] Saved Lv{Level} {Xp}xp {Gold}g");
        else
            Debug.LogWarning($"[PROGRESS] Save failed: {req.error}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static PlayerIdentity FindLocalIdentity()
    {
        foreach (var id in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (id.isLocalPlayer) return id;
        return null;
    }

    /// <summary>XP required to reach the next level. Simple curve: 100 × level^1.5</summary>
    public static int XpForLevel(int level) => Mathf.RoundToInt(100f * Mathf.Pow(level, 1.5f));

    // ── JSON shapes ───────────────────────────────────────────────────────────
    [Serializable] class CharacterFetchResponse
    {
        public int level, xp, gold, stat_str, stat_agi, stat_int, stat_vit;
    }

    [Serializable] class SaveProgressRequest
    {
        public int characterId, level, xp, gold, stat_str, stat_agi, stat_int, stat_vit;
    }
}
