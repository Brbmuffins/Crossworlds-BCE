using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// ArenaSessionController — Zero-config arena session lifecycle manager.
///
/// Self-bootstraps at startup. Listens for scene changes and automatically:
///   Hub/any → Arena : CombatSessionTracker.Local.BeginSession()
///   Arena   → any   : CombatSessionTracker.Local.EndSession()  (posts stats)
///
/// No manual wire-up required. Just ensure CombatSessionTracker is also
/// bootstrapped (it is, via [RuntimeInitializeOnLoadMethod]).
///
/// Copy to: Assets/Game/Combat/Scripts/ArenaSessionController.cs
/// </summary>
public class ArenaSessionController : MonoBehaviour
{
    private const string ARENA_SCENE = "Arena";

    // ─── Bootstrap ────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        var go = new GameObject("[ArenaSessionController]");
        go.AddComponent<ArenaSessionController>();
        DontDestroyOnLoad(go);
    }

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    void OnEnable()
    {
        SceneManager.sceneLoaded   += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded   -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // ─── Scene Events ─────────────────────────────────────────────────────────
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != ARENA_SCENE) return;

        // Only the local client begins a session
        var tracker = CombatSessionTracker.Local;
        if (tracker == null)
        {
            Debug.LogWarning("[SESSION] ArenaSessionController: CombatSessionTracker.Local not ready on Arena load");
            return;
        }

        tracker.BeginSession();
        Debug.Log("[SESSION] Arena loaded — combat session started");
    }

    void OnSceneUnloaded(Scene scene)
    {
        if (scene.name != ARENA_SCENE) return;

        var tracker = CombatSessionTracker.Local;
        if (tracker == null) return;

        if (tracker.IsInSession)
        {
            tracker.EndSession();
            Debug.Log("[SESSION] Arena unloaded — combat session ended, stats posted");
        }
    }
}
