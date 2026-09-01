#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;

/// <summary>Links a placed gathering node back to its reusable Node Forge definition.</summary>
[DisallowMultipleComponent]
public sealed class GatheringNodeInstance : MonoBehaviour
{
    public GatheringNodeDefinition definition;
    [HideInInspector] public GameObject visualRoot;

    [ContextMenu("Apply Node Definition")]
    public void ApplyDefinition()
    {
        if (definition == null) return;
        AfkGatheringStation station = GetComponent<AfkGatheringStation>();
        if (station == null) station = gameObject.AddComponent<AfkGatheringStation>();
        definition.ApplyTo(station);

        GatheringNodeNetworkState networkState = GetComponent<GatheringNodeNetworkState>();
        if (networkState != null) definition.ApplyTo(networkState);

        BoxCollider interactionCollider = GetComponent<BoxCollider>();
        if (interactionCollider == null) interactionCollider = gameObject.AddComponent<BoxCollider>();
        interactionCollider.isTrigger = false;
        interactionCollider.center = definition.colliderCenter;
        interactionCollider.size = definition.colliderSize;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (definition == null || UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
        AfkGatheringStation station = GetComponent<AfkGatheringStation>();
        if (station != null) definition.ApplyTo(station);
        BoxCollider interactionCollider = GetComponent<BoxCollider>();
        if (interactionCollider != null)
        {
            interactionCollider.center = definition.colliderCenter;
            interactionCollider.size = definition.colliderSize;
        }
    }
#endif
}
#endif
