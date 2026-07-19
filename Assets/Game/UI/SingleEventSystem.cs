using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(EventSystem))]
public class SingleEventSystem : MonoBehaviour
{
    void Awake()
    {
        EventSystem myEs = GetComponent<EventSystem>();
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
        foreach (var es in eventSystems)
        {
            if (es != null && es != myEs && es.gameObject.activeInHierarchy && es.enabled)
            {
                // If there is already another active and enabled EventSystem in the scene,
                // destroy this duplicate EventSystem GameObject immediately.
                Destroy(gameObject);
                return;
            }
        }
    }
}