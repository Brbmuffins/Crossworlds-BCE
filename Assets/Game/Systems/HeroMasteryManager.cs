#if UNITY_EDITOR || !UNITY_SERVER
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// HeroMasteryManager — self-bootstrapping singleton.
/// Fetches all 5 hero mastery rows, tracks XP/level locally,
/// applies L6 and L10 passive bonuses to CharacterStats, and
/// posts XP awards to /api/mastery/award-xp.
///
/// L6 / L10 passive bonuses per hero:
///   0 Warden:     L6 = +8% dmg,      L10 = +5% CDR
///   1 Ironclad:   L6 = +10% max HP,  L10 = +8% dmg
///   2 Shadowblade: L6 = +8% dmg,     L10 = +8% CDR
///   3 Cleric:      L6 = +15% heal,   L10 = +10% max HP
///   4 Arcanist:    L6 = +8% CDR,     L10 = +10% dmg
///
/// Server endpoints (not yet live — stub data used until they exist):
///   GET  /api/mastery/:characterId
///   POST /api/mastery/award-xp  { characterId, heroId, amount }
/// </summary>
public class HeroMasteryManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static HeroMasteryManager Local { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Local != null) return;
        var go = new GameObject("[HeroMasteryManager]");
        DontDestroyOnLoad(go);
        Local = go.AddComponent<HeroMasteryManager>();
    }

    // ── Data ──────────────────────────────────────────────────────────────────
    [Serializable]
    public class MasteryEntry
    {
        public int heroId;
        public int level;
        public int xp;
        public int xpToNext;

        public float XpFraction => xpToNext > 0 ? Mathf.Clamp01((float)xp / xpToNext) : 1f;
    }

    /// <summary>Mastery for each of the 5 heroes. Index == heroId.</summary>
    public MasteryEntry[] Masteries { get; private set; } = DefaultMasteries();

    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>Fires whenever any mastery level or XP changes.</summary>
    public event Action OnMasteryChanged;

    /// <summary>Fires when a hero reaches a new level. (heroId, newLevel)</summary>
    public event Action<int, int> OnHeroLevelUp;

    // ── Private state ─────────────────────────────────────────────────────────
    int    _characterId   = -1;
    string _jwt           = "";
    bool   _loaded        = false;


    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    IEnumerator WaitForLocalPlayer()
    {
        float waited = 0f;
        while (waited < 15f)
        {
            // Accept auth from CharacterSelectManager pre-population (before spawn)
            // or from PlayerIdentity SyncVar (after spawn) — whichever arrives first.
            int  charId = AuthManager.CharacterId > 0 ? AuthManager.CharacterId : 0;
            string jwt  = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : "";

            if (charId <= 0)
            {
                // Fallback: poll PlayerIdentity
                var id = FindLocalIdentity();
                if (id != null && id.characterId > 0)
                {
                    charId = id.characterId;
                    jwt    = !string.IsNullOrEmpty(jwt) ? jwt : PlayerPrefs.GetString("jwt_token", "");
                }
            }

            if (charId > 0 && !string.IsNullOrEmpty(jwt))
            {
                _characterId = charId;
                _jwt         = jwt;
                StartCoroutine(FetchMastery());
                yield break;
            }

            waited += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
        // No server character found — apply stub defaults so HUD still shows
        ApplyBonuses();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Award mastery XP to a hero. Updates local cache immediately and posts
    /// to the server. Call from WaveSpawner / EnemyController.
    /// </summary>
    public void AwardXp(int heroId, int amount)
    {
        if (heroId < 0 || heroId >= Masteries.Length) return;

        var entry = Masteries[heroId];
        int prevLevel = entry.level;
        entry.xp += amount;

        // Level up locally
        while (entry.xp >= entry.xpToNext && entry.xpToNext > 0)
        {
            entry.xp     -= entry.xpToNext;
            entry.level++;
            entry.xpToNext = XpForLevel(entry.level + 1);
            Debug.Log($"[MASTERY] Hero {heroId} reached Lv{entry.level}");
            OnHeroLevelUp?.Invoke(heroId, entry.level);
        }

        OnMasteryChanged?.Invoke();

        if (entry.level != prevLevel)
            ApplyBonuses(); // new level may unlock a bonus

        if (_characterId > 0)
            StartCoroutine(PostAwardXp(heroId, amount));
    }

    /// <summary>Force a refresh from the server.</summary>
    public void Refresh() => StartCoroutine(FetchMastery());

    // ── Fetch ─────────────────────────────────────────────────────────────────
    IEnumerator FetchMastery()
    {
        string url = $"{ServerConfig.AuthBaseUrl}/api/mastery/{_characterId}";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {_jwt}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[MASTERY] Fetch failed ({req.error}) — using defaults");
            ApplyBonuses();
            yield break;
        }

        try
        {
            var root = JsonUtility.FromJson<MasteryFetchResponse>(req.downloadHandler.text);
            if (!root.success || root.data == null || root.data.Length == 0)
            {
                Debug.LogWarning("[MASTERY] Empty mastery response — using defaults");
                ApplyBonuses();
                yield break;
            }

            // Merge server data into our 5-entry array
            foreach (var row in root.data)
            {
                if (row.heroId >= 0 && row.heroId < Masteries.Length)
                {
                    var e = Masteries[row.heroId];
                    e.heroId   = row.heroId;
                    e.level    = row.level;
                    e.xp       = row.xp;
                    e.xpToNext = row.xpToNext > 0 ? row.xpToNext : XpForLevel(row.level + 1);
                }
            }

            _loaded = true;
            ApplyBonuses();
            OnMasteryChanged?.Invoke();
            Debug.Log($"[MASTERY] Loaded — hero levels: " +
                      $"{Masteries[0].level}/{Masteries[1].level}/{Masteries[2].level}/" +
                      $"{Masteries[3].level}/{Masteries[4].level}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MASTERY] Parse error: {e.Message} — using defaults");
            ApplyBonuses();
        }
    }

    // ── Post XP award ─────────────────────────────────────────────────────────
    IEnumerator PostAwardXp(int heroId, int amount)
    {
        string url = $"{ServerConfig.AuthBaseUrl}/api/mastery/award-xp";
        string body = JsonUtility.ToJson(new AwardXpRequest
        {
            characterId = _characterId,
            heroId      = heroId,
            amount      = amount
        });

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {_jwt}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"[MASTERY] XP award POST failed: {req.error}");
        else
            Debug.Log($"[MASTERY] Posted +{amount}xp to hero {heroId}");
    }

    // ── Bonus calculation & application ──────────────────────────────────────

    /// <summary>
    /// Compute aggregate mastery passive bonuses from all 5 heroes' current
    /// levels, then push them to the local player's CharacterStats.
    /// Call whenever mastery levels change.
    /// </summary>
    void ApplyBonuses()
    {
        float dmg   = 0f;
        float heal  = 0f;
        float cdr   = 0f;
        float maxHp = 0f;

        for (int i = 0; i < Masteries.Length; i++)
        {
            int lv = Masteries[i].level;
            GetBonuses(i, lv, out float d6, out float h6, out float c6, out float hp6,
                               out float d10, out float h10, out float c10, out float hp10);

            if (lv >= 6)  { dmg += d6;  heal += h6;  cdr += c6;  maxHp += hp6;  }
            if (lv >= 10) { dmg += d10; heal += h10; cdr += c10; maxHp += hp10; }
        }

        // Find the local player's CharacterStats component and push bonuses.
        // If the player hasn't spawned yet, mark pending and retry once they do.
        var localStats = FindLocalCharacterStats();
        if (localStats != null)
        {
            localStats.SetMasteryBonuses(dmg, heal, cdr, maxHp);
        }
        else
        {
            StartCoroutine(RetryApplyBonuses());
        }
    }

    IEnumerator RetryApplyBonuses()
    {
        float waited = 0f;
        while (waited < 30f)
        {
            yield return new WaitForSeconds(0.5f);
            waited += 0.5f;
            var localStats = FindLocalCharacterStats();
            if (localStats != null)
            {
                ApplyBonuses();
                yield break;
            }
        }
        Debug.LogWarning("[MASTERY] Timed out waiting for CharacterStats — mastery bonuses not applied.");
    }

    // ── Per-hero bonus definitions ────────────────────────────────────────────
    // All values are percentages (0.08 = 8%). maxHp values are fractions of BaseMaxHealth.
    //
    //   Hero 0 Warden:      L6 = +8% dmg,     L10 = +5% CDR
    //   Hero 1 Ironclad:    L6 = +10% maxHp,  L10 = +8% dmg
    //   Hero 2 Shadowblade: L6 = +8% dmg,     L10 = +8% CDR
    //   Hero 3 Cleric:      L6 = +15% heal,   L10 = +10% maxHp
    //   Hero 4 Arcanist:    L6 = +8% CDR,     L10 = +10% dmg
    static void GetBonuses(int heroId, int level,
        out float d6,  out float h6,  out float c6,  out float hp6,
        out float d10, out float h10, out float c10, out float hp10)
    {
        d6 = h6 = c6 = hp6 = d10 = h10 = c10 = hp10 = 0f;
        switch (heroId)
        {
            case 0: // Warden
                d6 = 0.08f; c10 = 0.05f;
                break;
            case 1: // Ironclad
                hp6 = 0.10f; d10 = 0.08f;
                break;
            case 2: // Shadowblade
                d6 = 0.08f; c10 = 0.08f;
                break;
            case 3: // Cleric
                h6 = 0.15f; hp10 = 0.10f;
                break;
            case 4: // Arcanist
                c6 = 0.08f; d10 = 0.10f;
                break;
        }
    }

    // ── XP curve ─────────────────────────────────────────────────────────────
    /// <summary>Mastery XP required to reach a given level. Separate from character XP curve.</summary>
    public static int XpForLevel(int level) => Mathf.RoundToInt(200f * Mathf.Pow(level, 1.4f));

    // ── Helpers ───────────────────────────────────────────────────────────────
    static PlayerIdentity FindLocalIdentity()
    {
        foreach (var id in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (id.isLocalPlayer) return id;
        return null;
    }

    static CharacterStats FindLocalCharacterStats()
    {
        var id = FindLocalIdentity();
        return id != null ? id.GetComponent<CharacterStats>() : null;
    }

    static MasteryEntry[] DefaultMasteries()
    {
        var arr = new MasteryEntry[5];
        for (int i = 0; i < 5; i++)
            arr[i] = new MasteryEntry { heroId = i, level = 1, xp = 0, xpToNext = XpForLevel(2) };
        return arr;
    }

    // ── JSON shapes ───────────────────────────────────────────────────────────
    [Serializable]
    class MasteryFetchResponse
    {
        public bool          success;
        public MasteryEntry[] data;
    }

    [Serializable]
    class AwardXpRequest
    {
        public int characterId;
        public int heroId;
        public int amount;
    }
}
#endif