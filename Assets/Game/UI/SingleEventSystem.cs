using UnityEngine;
using UnityEngine.EventSystems;

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
            if (es.GetInstanceID() < survivor.GetInstanceID())
                survivor = es;

        foreach (var es in all)
            if (es != survivor)
                Destroy(es.gameObject);
    }
}