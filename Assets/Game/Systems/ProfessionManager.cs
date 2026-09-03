#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ProfessionManager — client-side singleton.
/// Loads profession records on login, caches them locally, and exposes
/// level-check + XP-award APIs to AfkGatheringStation and other systems.
///
/// Game-facing profession indices:
///   0 = Woodcutting   1 = Fishing   2 = Mining
/// Wire/database IDs are the stable strings: woodcutting, fishing, mining.
///
/// XP formula (matches ProfessionsPanel.ts):
///   xpToNextLevel = skill_level × 50
///
/// Server endpoint used:
///   GET  /api/professions/:characterId  — load
///   POST /api/professions/award-xp      — award XP + receive level-up signal
/// </summary>
public class ProfessionManager : MonoBehaviour
{
    public static ProfessionManager Local { get; private set; }

    public static readonly string[] ProfessionNames = { "Woodcutting", "Fishing", "Mining" };
    public static readonly string[] ProfessionWireIds = { "woodcutting", "fishing", "mining" };

    public static string ToWireId(int professionId) =>
        professionId >= 0 && professionId < ProfessionWireIds.Length
            ? ProfessionWireIds[professionId]
            : string.Empty;

    public static bool TryFromWireId(string wireId, out int professionId)
    {
        for (int i = 0; i < ProfessionWireIds.Length; i++)
        {
            if (string.Equals(ProfessionWireIds[i], wireId, System.StringComparison.OrdinalIgnoreCase))
            {
                professionId = i;
                return true;
            }
        }

        professionId = -1;
        return false;
    }

    // ── Events ────────────────────────────────────────────────────────────────────
    /// Fired when a profession levels up. Args: professionId, newLevel.
    public event System.Action<int, int> onLevelUp;

    // ── Local cache ───────────────────────────────────────────────────────────────
    [System.Serializable]
    public class ProfessionRecord
    {
        public int profession_id;
        public int skill_level;
        public int skill_xp;
    }

    [System.Serializable]
    class ProfessionWireRecord
    {
        public string profession_id;
        public int skill_level;
        public int skill_xp;
    }

    private readonly Dictionary<int, ProfessionRecord> _professions = new();
    private bool _loaded = false;
    public  bool IsLoaded => _loaded;

    // ─────────────────────────────────────────────────────────────────────────────

    // Self-bootstrap so Local is never null, matching InventoryManager /
    // ItemCatalogManager. Without this, nothing instantiated ProfessionManager
    // and every ProfessionManager.Local?.… call silently no-opped.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Local != null) return;
        var go = new GameObject("[ProfessionManager]");
        DontDestroyOnLoad(go);
        go.AddComponent<ProfessionManager>();
    }

    void Awake()
    {
        if (Local != null && Local != this) { Destroy(gameObject); return; }
        Local = this;
        DontDestroyOnLoad(gameObject);
    }

    // Attempts an initial load; no-ops until AuthManager.CharacterId is set, at
    // which point PlayerIdentity.OnStartLocalPlayer re-triggers Load().
    void Start() => Load();

    void OnDestroy()
    {
        if (Local == this) Local = null;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────────────────────

    /// Returns current skill level for a profession (0 = not started, 1+ = active).
    public int GetLevel(int professionId)
    {
        return _professions.TryGetValue(professionId, out var rec) ? rec.skill_level : 1;
    }

    /// Returns current XP within the current level, and the threshold for the next.
    public (int current, int needed) GetXpProgress(int professionId)
    {
        if (!_professions.TryGetValue(professionId, out var rec)) return (0, 50);
        int needed = Mathf.Max(1, rec.skill_level) * 50;
        return (rec.skill_xp, needed);
    }

    /// Loads profession data from the server. Called on hub entry / login.
    public void Load()
    {
        if (AuthManager.CharacterId <= 0) return;
        StartCoroutine(FetchProfessions());
    }

    /// Posts XP to the server and updates the local cache.
    /// Returns true if a level-up occurred.
    public void AwardXp(int professionId, int xpAmount)
    {
        StartCoroutine(PostAwardXp(professionId, xpAmount));
    }

    /// <summary>Updates the client cache from a trusted dedicated-server transaction.</summary>
    public void ApplyAuthoritativeState(int professionId, int skillLevel, int skillXp, bool leveledUp)
    {
        int oldLevel = GetLevel(professionId);
        if (!_professions.TryGetValue(professionId, out ProfessionRecord record))
        {
            record = new ProfessionRecord { profession_id = professionId };
            _professions[professionId] = record;
        }
        record.skill_level = Mathf.Max(1, skillLevel);
        record.skill_xp = Mathf.Max(0, skillXp);
        if (leveledUp && record.skill_level > oldLevel)
        {
            onLevelUp?.Invoke(professionId, record.skill_level);
            string professionName = professionId < ProfessionNames.Length
                ? ProfessionNames[professionId] : $"Profession {professionId}";
            RodChatManager.Instance?.AddSystemMessage(
                $"{professionName} reached level {record.skill_level}!");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Network
    // ─────────────────────────────────────────────────────────────────────────────

    IEnumerator FetchProfessions()
    {
        int    charId = AuthManager.CharacterId;
        string jwt    = AuthManager.Token;

        using var req = UnityWebRequest.Get($"{ServerConfig.AuthBaseUrl}/api/professions/{charId}");
        req.SetRequestHeader("Authorization", $"Bearer {jwt}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[PROF] Failed to load professions: {req.error}");
            yield break;
        }

        var wrapper = JsonUtility.FromJson<ProfessionListWrapper>(req.downloadHandler.text);
        _professions.Clear();

        if (wrapper?.data != null)
        {
            foreach (var p in wrapper.data)
            {
                if (!TryFromWireId(p.profession_id, out int professionId))
                {
                    Debug.LogWarning($"[PROF] Ignoring unknown profession id '{p.profession_id}'.");
                    continue;
                }

                _professions[professionId] = new ProfessionRecord
                {
                    profession_id = professionId,
                    skill_level = p.skill_level,
                    skill_xp = p.skill_xp
                };
            }
        }

        // Seed missing professions at level 1, 0 xp so UI always has entries
        for (int i = 0; i < ProfessionNames.Length; i++)
        {
            if (!_professions.ContainsKey(i))
                _professions[i] = new ProfessionRecord { profession_id = i, skill_level = 1, skill_xp = 0 };
        }

        _loaded = true;
        Debug.Log($"[PROF] Loaded {_professions.Count} professions for char {charId}.");
    }

    IEnumerator PostAwardXp(int professionId, int xpAmount)
    {
        int    charId = AuthManager.CharacterId;
        string jwt    = AuthManager.Token;

        if (charId <= 0 || string.IsNullOrEmpty(jwt)) yield break;

        string wireId = ToWireId(professionId);
        if (string.IsNullOrEmpty(wireId))
        {
            Debug.LogWarning($"[PROF] Refusing to award XP for unknown profession index {professionId}.");
            yield break;
        }

        string json = $"{{\"characterId\":{charId},\"professionId\":\"{wireId}\",\"xpAmount\":{xpAmount}}}";

        using var req = new UnityWebRequest($"{ServerConfig.AuthBaseUrl}/api/professions/award-xp", "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {jwt}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[PROF] award-xp failed: {req.error}");
            // Apply locally as fallback so the session feels responsive
            ApplyXpLocally(professionId, xpAmount);
            yield break;
        }

        var resp = JsonUtility.FromJson<AwardXpResponse>(req.downloadHandler.text);
        if (resp?.success == true && resp.data != null)
        {
            int oldLevel = GetLevel(professionId);

            if (_professions.TryGetValue(professionId, out var rec))
            {
                rec.skill_level = resp.data.skill_level;
                rec.skill_xp    = resp.data.skill_xp;
            }
            else
            {
                _professions[professionId] = new ProfessionRecord
                {
                    profession_id = professionId,
                    skill_level   = resp.data.skill_level,
                    skill_xp      = resp.data.skill_xp
                };
            }

            if (resp.data.leveled_up && resp.data.skill_level > oldLevel)
            {
                onLevelUp?.Invoke(professionId, resp.data.skill_level);
                string name = professionId < ProfessionNames.Length ? ProfessionNames[professionId] : $"Profession {professionId}";
                RodChatManager.Instance?.AddSystemMessage(
                    $"{name} reached level {resp.data.skill_level}!");
                Debug.Log($"[PROF] Level up! {name} → {resp.data.skill_level}");
            }
        }
    }

    void ApplyXpLocally(int professionId, int xpAmount)
    {
        if (!_professions.TryGetValue(professionId, out var rec)) return;
        rec.skill_xp += xpAmount;
        int needed = rec.skill_level * 50;
        while (rec.skill_xp >= needed)
        {
            rec.skill_xp  -= needed;
            rec.skill_level++;
            needed          = rec.skill_level * 50;
            onLevelUp?.Invoke(professionId, rec.skill_level);
        }
    }

    // ── JSON wrappers ─────────────────────────────────────────────────────────────

    [System.Serializable] class ProfessionListWrapper { public bool success; public List<ProfessionWireRecord> data; }
    [System.Serializable] class AwardXpData { public int skill_level; public int skill_xp; public bool leveled_up; }
    [System.Serializable] class AwardXpResponse { public bool success; public AwardXpData data; }
}
#endif
