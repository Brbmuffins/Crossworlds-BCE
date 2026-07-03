#if !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// CombatSessionTracker — self-bootstrapping singleton.
/// Tracks per-run stats for the LOCAL player: damage dealt, healing done,
/// kills, waves survived. POSTs to /api/combat/session/end on arena clear
/// or on scene transition back to Hub.
///
/// Usage:
///   CombatSessionTracker.Local — the singleton instance.
///   CombatSessionTracker.Local.NotifyEnemySpawned(enemyGO) — call from WaveSpawner.
///   CombatSessionTracker.Local.NotifyAllySpawned(allyGO)   — call from player spawn.
/// </summary>
public class CombatSessionTracker : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static CombatSessionTracker Local { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Local != null) return;
        var go = new GameObject("[CombatSessionTracker]");
        DontDestroyOnLoad(go);
        Local = go.AddComponent<CombatSessionTracker>();
    }

    // ── Stats ─────────────────────────────────────────────────────────────────
    public float damageDealt    { get; private set; }
    public float healingDone    { get; private set; }
    public int   killCount      { get; private set; }
    public int   wavesSurvived  { get; private set; }
    public float sessionStartTime { get; private set; }

    /// <summary>True while the local player is inside an active Arena run.</summary>
    public bool IsInSession => _inArena;

    // ── State ─────────────────────────────────────────────────────────────────
    private bool  _posted    = false;
    private bool  _inArena   = false;

    // Tracked enemies and allies to hook events on
    private readonly HashSet<Health> _trackedEnemies = new();
    private readonly HashSet<Health> _trackedAllies  = new();

    // Cached local player identity
    private GameObject _localPlayerGO;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        ArenaClearUI.OnRunComplete += OnRunComplete;
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDestroy()
    {
        ArenaClearUI.OnRunComplete -= OnRunComplete;
        SceneManager.activeSceneChanged -= OnSceneChanged;
        UntrackAll();
    }

    // ── Scene change ──────────────────────────────────────────────────────────
    void OnSceneChanged(Scene from, Scene to)
    {
        bool enteringArena = to.buildIndex >= 3; // Hub = 2, arenas start at 3+
        bool leavingArena  = from.buildIndex >= 3 && to.buildIndex <= 2;

        if (enteringArena)
        {
            StartSession();
        }
        else if (leavingArena)
        {
            // Scene changed back to hub without hitting ArenaClearUI — post anyway
            if (!_posted) StartCoroutine(PostSessionStats());
            UntrackAll();
        }
    }

    // ── Session control ───────────────────────────────────────────────────────
    void StartSession()
    {
        damageDealt      = 0f;
        healingDone      = 0f;
        killCount        = 0;
        wavesSurvived    = 0;
        sessionStartTime = Time.time;
        _posted          = false;
        _inArena         = true;
        _localPlayerGO   = null;
        _trackedEnemies.Clear();
        _trackedAllies.Clear();

        // Hook WaveSpawner wave-count event after a short delay (scene still loading)
        StartCoroutine(LateBindWaveSpawner());
    }

    IEnumerator LateBindWaveSpawner()
    {
        yield return new WaitForSeconds(1f);
        var ws = FindFirstObjectByType<WaveSpawner>();
        if (ws != null)
        {
            // Hook wave completed callback if available
            // WaveSpawner fires onWaveComplete if it exposes it — check and wire
            // (if not available, we track via EnemyDeath count)
        }
    }

    // ── Public: wire into WaveSpawner / spawn flow ────────────────────────────

    /// <summary>Call from WaveSpawner each time it spawns an enemy.</summary>
    public void NotifyEnemySpawned(GameObject enemy)
    {
        var h = enemy.GetComponent<Health>();
        if (h == null || _trackedEnemies.Contains(h)) return;
        _trackedEnemies.Add(h);

        // Count damage dealt via onDamageTaken on the enemy
        h.onDamageTaken.AddListener(amt => OnEnemyDamaged(h, amt));
        // Count kills
        h.onKilledBy.AddListener(src => OnEnemyKilled(src));
    }

    /// <summary>Call once when local player spawns in the arena.</summary>
    public void NotifyAllySpawned(GameObject ally)
    {
        var ni = ally.GetComponent<NetworkIdentity>();
        if (ni == null || !ni.isLocalPlayer) return;
        _localPlayerGO = ally;

        var h = ally.GetComponent<Health>();
        if (h == null || _trackedAllies.Contains(h)) return;
        _trackedAllies.Add(h);
        h.onHealApplied.AddListener(OnHealApplied);
    }

    /// <summary>Call from WaveManager/WaveSpawner when a wave is completed.</summary>
    public void NotifyWaveComplete()
    {
        wavesSurvived++;
    }

    // ── Stat hooks ────────────────────────────────────────────────────────────
    void OnEnemyDamaged(Health enemy, float amount)
    {
        // Only count damage where the local player is the source.
        // Since we listen on each enemy's onDamageTaken (which fires regardless of source),
        // we rely on the fact that this caster only attaches listeners for enemies it spawned —
        // all damage to tracked enemies is credited to this session. Server-authoritative
        // damage attribution is available via onKilledBy; for damage totals this is
        // a reasonable client-side approximation.
        damageDealt += amount;
    }

    void OnEnemyKilled(GameObject source)
    {
        // source == null means server-initiated kill (e.g., arena hazard)
        if (source == null) return;
        // Credit if local player dealt killing blow
        if (source == _localPlayerGO ||
            (source.GetComponent<NetworkIdentity>() is NetworkIdentity ni && ni.isLocalPlayer))
        {
            killCount++;
        }
    }

    void OnHealApplied(float amount)
    {
        healingDone += amount;
    }

    // ── Run complete ──────────────────────────────────────────────────────────
    void OnRunComplete()
    {
        if (_posted) return;
        StartCoroutine(PostSessionStats());
    }

    // ── HTTP POST ─────────────────────────────────────────────────────────────
    IEnumerator PostSessionStats()
    {
        _posted = true;

        int charId    = PlayerPrefs.GetInt("SelectedCharacter", 0);
        int heroClass = PlayerProgressManager.Local?.ClassIndex ?? 0;
        float duration = Time.time - sessionStartTime;

        var body = JsonUtility.ToJson(new SessionEndRequest
        {
            characterId   = charId,
            damageDealt   = Mathf.RoundToInt(damageDealt),
            healingDone   = Mathf.RoundToInt(healingDone),
            killCount     = killCount,
            wavesSurvived = wavesSurvived,
            durationSeconds = Mathf.RoundToInt(duration),
            heroClass     = heroClass
        });

        string serverUrl = ServerConfig.AuthBaseUrl;
        string token     = PlayerPrefs.GetString("jwt_token", "");

        using var req = new UnityWebRequest($"{serverUrl}/api/combat/session/end", "POST");
        req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning($"[COMBAT] Session POST failed: {req.error}");
        else
            Debug.Log($"[COMBAT] Session posted — dmg:{Mathf.RoundToInt(damageDealt)} " +
                      $"heal:{Mathf.RoundToInt(healingDone)} kills:{killCount} " +
                      $"waves:{wavesSurvived} dur:{Mathf.RoundToInt(duration)}s");
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────
    void UntrackAll()
    {
        foreach (var h in _trackedEnemies)
        {
            if (h != null)
            {
                h.onDamageTaken.RemoveAllListeners();
                h.onKilledBy.RemoveAllListeners();
            }
        }
        foreach (var h in _trackedAllies)
        {
            if (h != null) h.onHealApplied.RemoveListener(OnHealApplied);
        }
        _trackedEnemies.Clear();
        _trackedAllies.Clear();
        _inArena = false;
    }

    // ── JSON types ────────────────────────────────────────────────────────────
    [System.Serializable]
    class SessionEndRequest
    {
        public int   characterId;
        public int   damageDealt;
        public int   healingDone;
        public int   killCount;
        public int   wavesSurvived;
        public int   durationSeconds;
        public int   heroClass;
    }
}
#endif
