#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using UnityEngine;

/// <summary>
/// HitstopManager — client-side time-freeze on melee connect.
///
/// Uses unscaled time internally so the freeze cannot interfere with
/// server tick cadence (which runs at Time.time, not Time.unscaledTime).
/// Never stacks: a second call during an active freeze is ignored unless
/// it's heavier than the current one (in which case it extends).
///
/// Usage:
///   HitstopManager.Freeze(HitstopManager.Weight.Light);   // 30ms
///   HitstopManager.Freeze(HitstopManager.Weight.Heavy);   // 80ms
///   HitstopManager.Freeze(0.05f);                         // 50ms custom
///
/// All durations tunable in the FeelConfig ScriptableObject if present.
/// </summary>
public class HitstopManager : MonoBehaviour
{
    public enum Weight { Light = 0, Medium = 1, Heavy = 2, KillBlow = 3 }

    // ── Singleton ─────────────────────────────────────────────────────────────
    static HitstopManager _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[HitstopManager]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<HitstopManager>();
    }

    // ── Default durations (seconds) ───────────────────────────────────────────
    static readonly float[] _defaults = { 0.030f, 0.050f, 0.080f, 0.110f };

    bool  _freezing;
    float _freezeEnd;   // unscaled time when freeze expires

    // ── Static API ────────────────────────────────────────────────────────────

    /// <summary>Freeze using a preset weight.</summary>
    public static void Freeze(Weight w)
    {
        float dur = _defaults[(int)w];
        // Allow FeelConfig override when present
        if (FeelConfig.Instance != null)
            dur = FeelConfig.Instance.HitstopDuration((FeelConfig.HitstopWeight)(int)w);
        FreezeFor(dur);
    }

    /// <summary>Freeze for an exact duration in seconds.</summary>
    public static void FreezeFor(float seconds)
    {
        if (_instance == null) return;
        _instance.DoFreeze(seconds);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    void DoFreeze(float duration)
    {
        float end = Time.unscaledTime + duration;
        if (_freezing && end <= _freezeEnd) return;   // already freezing longer
        _freezeEnd = end;
        if (!_freezing) StartCoroutine(FreezeRoutine());
    }

    IEnumerator FreezeRoutine()
    {
        _freezing = true;
        Time.timeScale = 0f;

        while (Time.unscaledTime < _freezeEnd)
            yield return null;   // WaitForSecondsRealtime allocates — poll instead

        Time.timeScale = 1f;
        _freezing = false;
    }
}
#endif // UNITY_EDITOR || !UNITY_SERVER
