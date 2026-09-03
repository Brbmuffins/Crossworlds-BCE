using UnityEngine;

public enum GatheringNodeCategory
{
    Mining,
    Fishing,
    Woodcutting,
    Herbalism,
    Salvaging,
    Other
}

public enum GatheringNodeFallbackShape
{
    Rock,
    Tree,
    FishingSpot
}

/// <summary>
/// Reusable authoring data for a gathering node. Node Forge copies the runtime
/// values to AfkGatheringStation while retaining this asset link for later updates.
/// </summary>
[CreateAssetMenu(fileName = "GatheringNode", menuName = "BCE/Gathering Node Definition")]
public sealed class GatheringNodeDefinition : ScriptableObject
{
    [Header("Identity")]
    public string nodeId = "new_node";
    public string displayName = "New Gathering Node";
    public GatheringNodeCategory category = GatheringNodeCategory.Mining;

    [Header("Profession")]
    [Tooltip("0 = Woodcutting, 1 = Fishing, 2 = Mining. Other systems may add additional IDs.")]
    public int professionId = 2;
    [Min(1)] public int minimumLevel = 1;

    [Header("Yield")]
    [Tooltip("Optional weighted reward table. When assigned, the dedicated server rolls the reward.")]
    public GatheringLootTable lootTable;
    [Tooltip("Preferred item reference. When assigned, its stable database ID is used.")]
    public LootItemDefinition yieldItem;
    [Tooltip("Database item ID. Use this for existing database-only materials.")]
    public string itemId = "ore_copper";
    [Min(1)] public int itemQuantity = 1;
    [Min(0.1f)] public float secondsPerYield = 5f;
    [Min(0)] public int experiencePerYield = 10;
    [Min(1)] public int bonusYieldLevel = 10;

    [Header("Depletion & Respawn")]
    [Min(1)] public int minimumAwardsPerSpawn = 1;
    [Min(1)] public int maximumAwardsPerSpawn = 5;
    [Min(1f)] public float respawnSeconds = 900f;

    [Header("Interaction")]
    [Min(0.5f)] public float interactionRange = 3f;
    [Min(0.5f)] public float cancelRadius = 4f;
    [Min(0f)] public float promptHeight = 2.4f;
    [Tooltip("Optional prompt verb such as harvesting or salvaging. Leave empty for profession defaults.")]
    public string interactionVerb = "";
    public string gatheringAnimationBool = "";

    [Header("Visual")]
    [Tooltip("Prefab instantiated as a child of the functional node root.")]
    public GameObject visualPrefab;
    public Vector3 visualLocalPosition = Vector3.zero;
    public Vector3 visualLocalEulerAngles = Vector3.zero;
    public Vector3 visualLocalScale = Vector3.one;
    public GameObject yieldVFXPrefab;

    [Header("Interaction Collider")]
    public Vector3 colliderCenter = new(0f, 0.6f, 0f);
    public Vector3 colliderSize = new(1.5f, 1.5f, 1.5f);

    [Header("Placeholder Visual")]
    [Tooltip("Used only until a visual prefab is assigned.")]
    public GatheringNodeFallbackShape fallbackShape = GatheringNodeFallbackShape.Rock;
    public Color fallbackColor = new(0.45f, 0.45f, 0.45f, 1f);

    public string ResolvedItemId =>
        yieldItem != null && !string.IsNullOrWhiteSpace(yieldItem.itemId)
            ? yieldItem.itemId.Trim()
            : itemId?.Trim() ?? "";

#if UNITY_EDITOR || !UNITY_SERVER
    public void ApplyTo(AfkGatheringStation station)
    {
        if (station == null) return;
        station.stationName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
        station.professionId = professionId;
        station.minLevelRequired = Mathf.Max(1, minimumLevel);
        station.itemId = ResolvedItemId;
        station.lootTable = lootTable;
        station.itemQuantity = Mathf.Max(1, itemQuantity);
        station.tickInterval = Mathf.Max(0.1f, secondsPerYield);
        station.xpPerTick = Mathf.Max(0, experiencePerYield);
        station.bonusYieldLevel = Mathf.Max(1, bonusYieldLevel);
        station.minimumAwardsPerSpawn = Mathf.Max(1, minimumAwardsPerSpawn);
        station.maximumAwardsPerSpawn = Mathf.Max(station.minimumAwardsPerSpawn, maximumAwardsPerSpawn);
        station.respawnSeconds = Mathf.Max(1f, respawnSeconds);
        station.interactRange = Mathf.Max(0.5f, interactionRange);
        station.cancelRadius = Mathf.Max(station.interactRange, cancelRadius);
        station.promptHeight = Mathf.Max(0f, promptHeight);
        station.interactionVerb = interactionVerb ?? "";
        station.tickVFXPrefab = yieldVFXPrefab;
        station.gatheringAnimBool = gatheringAnimationBool ?? "";
    }
#endif

    public void ApplyTo(GatheringNodeNetworkState state)
    {
        if (state == null) return;
        state.minimumAwardsPerSpawn = Mathf.Max(1, minimumAwardsPerSpawn);
        state.maximumAwardsPerSpawn = Mathf.Max(state.minimumAwardsPerSpawn, maximumAwardsPerSpawn);
        state.respawnSeconds = Mathf.Max(1f, respawnSeconds);
        state.interactionRange = Mathf.Max(0.5f, interactionRange);
        state.minimumSecondsBetweenAwards = Mathf.Max(0.1f, secondsPerYield - 0.25f);
        state.lootTable = lootTable;
        state.experiencePerAward = Mathf.Max(0, experiencePerYield);
        state.bonusYieldLevel = Mathf.Max(1, bonusYieldLevel);
        state.professionId = professionId;
    }

    void OnValidate()
    {
        nodeId = nodeId?.Trim().ToLowerInvariant() ?? "";
        if (yieldItem != null && !string.IsNullOrWhiteSpace(yieldItem.itemId))
            itemId = yieldItem.itemId.Trim();
        minimumLevel = Mathf.Max(1, minimumLevel);
        itemQuantity = Mathf.Max(1, itemQuantity);
        secondsPerYield = Mathf.Max(0.1f, secondsPerYield);
        experiencePerYield = Mathf.Max(0, experiencePerYield);
        bonusYieldLevel = Mathf.Max(1, bonusYieldLevel);
        minimumAwardsPerSpawn = Mathf.Max(1, minimumAwardsPerSpawn);
        maximumAwardsPerSpawn = Mathf.Max(minimumAwardsPerSpawn, maximumAwardsPerSpawn);
        respawnSeconds = Mathf.Max(1f, respawnSeconds);
        interactionRange = Mathf.Max(0.5f, interactionRange);
        cancelRadius = Mathf.Max(interactionRange, cancelRadius);
        promptHeight = Mathf.Max(0f, promptHeight);
        colliderSize.x = Mathf.Max(0.1f, colliderSize.x);
        colliderSize.y = Mathf.Max(0.1f, colliderSize.y);
        colliderSize.z = Mathf.Max(0.1f, colliderSize.z);
        if (visualLocalScale == Vector3.zero) visualLocalScale = Vector3.one;
    }
}
