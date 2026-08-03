using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

// Ensures only one EventSystem is alive at any time.
// Keeps the instance with the lowest GetInstanceID() (the oldest / DontDestroyOnLoad one)
// so the result is deterministic regardless of scene load order or Awake timing.
[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(EventSystem))]
public class SingleEventSystem : MonoBehaviour
{
    void Awake()
    {
        var all = FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
        if (all.Length <= 1) return;

        // Find the survivor: lowest instance ID wins (created earliest).
        EventSystem survivor = all[0];
        foreach (var es in all)
            if (es.GetEntityId().CompareTo(survivor.GetEntityId()) < 0)
                survivor = es;

        foreach (var es in all)
            if (es != survivor)
                Destroy(es.gameObject);
    }

    /// <summary>
    /// Force exactly one freshly-built EventSystem, healing dead UI input after a
    /// return from gameplay.
    ///
    /// The bug this fixes: gameplay UI (chat, waypoint map, hub-return) each create a
    /// DontDestroyOnLoad EventSystem that carries into LoginScene/CharacterSelect on
    /// logout. With the Input System, an InputSystemUIInputModule owns the shared UI
    /// input actions; destroying its EventSystem disables those actions. A DEFERRED
    /// Destroy runs its OnDisable at end-of-frame — AFTER any survivor is set up — so
    /// the survivor is left with disabled actions and silently stops processing clicks
    /// (you reach the menu but nothing is clickable, and you can't log back in).
    ///
    /// So: destroy every EventSystem with DestroyImmediate (OnDisable runs NOW, releasing
    /// the actions), then create one fresh whose module binds and re-enables them last.
    /// </summary>
    public static void ForceSingle()
    {
        var all = FindObjectsByType<EventSystem>(FindObjectsInactive.Include);

        // First launch: a single EventSystem authored in the current scene is healthy —
        // leave its (possibly custom-configured) module untouched. Only rebuild when a
        // carried-over DontDestroyOnLoad EventSystem and/or a duplicate is present.
        if (all.Length == 1 && all[0].gameObject.scene.name != "DontDestroyOnLoad")
            return;

        Rebuild(all);
    }

    /// <summary>
    /// Verifies that the active EventSystem still has a live Input System module
    /// and enabled pointer/click actions. Scene transitions can leave the objects
    /// present while their shared input actions have already been disabled.
    /// </summary>
    public static void EnsureHealthy()
    {
        EventSystem current = EventSystem.current;
        InputSystemUIInputModule module = current != null
            ? current.GetComponent<InputSystemUIInputModule>()
            : null;

        bool pointerEnabled = module?.point?.action != null && module.point.action.enabled;
        bool clickEnabled = module?.leftClick?.action != null && module.leftClick.action.enabled;
        bool healthy = current != null && current.isActiveAndEnabled &&
                       module != null && module.isActiveAndEnabled &&
                       pointerEnabled && clickEnabled;

        if (healthy)
            return;

        Rebuild(FindObjectsByType<EventSystem>(FindObjectsInactive.Include));
    }

    /// <summary>
    /// Rebinds UI input even when Unity reports the existing action references as
    /// enabled. This is used for an explicitly opened modal after scene travel,
    /// where stale shared actions can look healthy but no longer deliver events.
    /// </summary>
    public static void ForceRebuild()
    {
        Rebuild(FindObjectsByType<EventSystem>(FindObjectsInactive.Include));
    }

    static void Rebuild(EventSystem[] all)
    {

        foreach (var es in all)
            DestroyImmediate(es.gameObject);

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        var module = go.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();
        DontDestroyOnLoad(go);
    }
}
