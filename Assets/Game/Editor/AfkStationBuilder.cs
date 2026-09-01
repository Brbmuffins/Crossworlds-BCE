// AfkGatheringStation and ProfessionManager are #if UNITY_EDITOR || !UNITY_SERVER — guard this
// editor script the same way so it compiles cleanly under the Dedicated Server target.
#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEditor;

/// <summary>
/// BCE/Setup/8 — Create AFK Gathering Stations
///
/// Drops three pre-configured AFK gathering stations into the active scene:
///   a) Copper Vein     (Mining, level 1, 5s tick)
///   b) Oak Tree        (Woodcutting, level 1, 6s tick)
///   c) Fishing Spot    (Fishing, level 1, 8s tick)
///
/// Each station is a mesh + AfkGatheringStation component.
/// Position them in the Hub scene away from the Forge NPC.
///
/// Runtime requirements (no compile errors before these exist):
///   - ProfessionManager singleton must be in Hub scene
///   - RodChatManager.Instance must exist
///   - AuthManager.CharacterId / AuthManager.Token must be set by login flow
/// </summary>
public static class AfkStationBuilder
{
    [MenuItem("BCE/Setup/8a ▶ Place Copper Vein (Mining)")]
    static void PlaceCopperVein() => PlaceStation(new StationConfig
    {
        stationName    = "Copper Vein",
        professionId   = 2,
        itemId         = "ore_copper",
        xpPerTick      = 10,
        tickInterval   = 5f,
        minLevel       = 1,
        bonusLevel     = 10,
        meshColor      = new Color(0.72f, 0.45f, 0.20f),   // copper brown
        promptColor    = new Color(0.70f, 0.95f, 0.50f),
    });

    [MenuItem("BCE/Setup/8b ▶ Place Iron Vein (Mining Lv 5)")]
    static void PlaceIronVein() => PlaceStation(new StationConfig
    {
        stationName    = "Iron Vein",
        professionId   = 2,
        itemId         = "ore_iron",
        xpPerTick      = 20,
        tickInterval   = 6f,
        minLevel       = 5,
        bonusLevel     = 15,
        meshColor      = new Color(0.4f, 0.4f, 0.5f),      // grey iron
        promptColor    = new Color(0.9f, 0.8f, 0.3f),
    });

    [MenuItem("BCE/Setup/8c ▶ Place Gold Vein (Mining Lv 15)")]
    static void PlaceGoldVein() => PlaceStation(new StationConfig
    {
        stationName    = "Gold Vein",
        professionId   = 2,
        itemId         = "ore_gold",
        xpPerTick      = 40,
        tickInterval   = 8f,
        minLevel       = 15,
        bonusLevel     = 25,
        meshColor      = new Color(1f, 0.82f, 0.18f),      // gold
        promptColor    = new Color(0.9f, 0.8f, 0.3f),
    });

    [MenuItem("BCE/Setup/8d ▶ Place Oak Tree (Woodcutting)")]
    static void PlaceOakTree() => PlaceStation(new StationConfig
    {
        stationName    = "Oak Tree",
        professionId   = 0,
        itemId         = "log_oak",
        xpPerTick      = 10,
        tickInterval   = 6f,
        minLevel       = 1,
        bonusLevel     = 10,
        meshColor      = new Color(0.33f, 0.20f, 0.08f),   // bark brown
        promptColor    = new Color(0.70f, 0.95f, 0.50f),
        meshShape      = PrimitiveType.Cylinder,
        meshScale      = new Vector3(0.6f, 3.5f, 0.6f),
    });

    [MenuItem("BCE/Setup/8e ▶ Place Fishing Spot")]
    static void PlaceFishingSpot() => PlaceStation(new StationConfig
    {
        stationName    = "Fishing Spot",
        professionId   = 1,
        itemId         = "fish_river",
        xpPerTick      = 8,
        tickInterval   = 8f,
        minLevel       = 1,
        bonusLevel     = 10,
        meshColor      = new Color(0.15f, 0.45f, 0.70f),   // water blue
        promptColor    = new Color(0.70f, 0.95f, 0.50f),
        meshShape      = PrimitiveType.Quad,
        meshScale      = new Vector3(3f, 3f, 1f),
    });

    [MenuItem("BCE/Setup/8f ▶ Place ProfessionManager in Scene")]
    static void PlaceProfessionManager()
    {
        var existing = Object.FindAnyObjectByType<ProfessionManager>();
        if (existing != null)
        {
            Debug.Log("[BCE] ProfessionManager already in scene.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // No manual placement needed: ProfessionManager self-bootstraps at runtime
        // (RuntimeInitializeOnLoadMethod) and Load() is auto-triggered from
        // PlayerIdentity.OnStartLocalPlayer once auth is ready. Creating one here
        // would just be destroyed by the singleton guard. This menu item is a no-op.
        Debug.Log("[BCE] ProfessionManager self-bootstraps at runtime — no scene object required. Nothing to place.");
    }

    // ─────────────────────────────────────────────────────────────────────────────

    struct StationConfig
    {
        public string        stationName;
        public int           professionId;
        public string        itemId;
        public int           xpPerTick;
        public float         tickInterval;
        public int           minLevel;
        public int           bonusLevel;
        public Color         meshColor;
        public Color         promptColor;
        public PrimitiveType meshShape;
        public Vector3       meshScale;
    }

    static void PlaceStation(StationConfig cfg)
    {
        // Default mesh shape if not specified
        if (cfg.meshShape == default) cfg.meshShape = PrimitiveType.Cube;
        if (cfg.meshScale == default) cfg.meshScale  = new Vector3(1.2f, 1.2f, 1.2f);

        var go = GameObject.CreatePrimitive(cfg.meshShape);
        go.name = cfg.stationName;

        // Position in front of camera
        var cam   = SceneView.lastActiveSceneView?.camera;
        var pos   = cam != null
            ? cam.transform.position + cam.transform.forward * 5f
            : Vector3.zero;
        pos.y     = 0f;
        go.transform.position   = pos;
        go.transform.localScale = cfg.meshScale;

        // Material
        var mat   = new Material(Shader.Find("Standard"));
        mat.color = cfg.meshColor;
        go.GetComponent<Renderer>().sharedMaterial = mat;

        // AfkGatheringStation component
        var station              = go.AddComponent<AfkGatheringStation>();
        station.stationName      = cfg.stationName;
        station.professionId     = cfg.professionId;
        station.itemId           = cfg.itemId;
        station.xpPerTick        = cfg.xpPerTick;
        station.tickInterval     = cfg.tickInterval;
        station.minLevelRequired = cfg.minLevel;
        station.bonusYieldLevel  = cfg.bonusLevel;
        station.itemQuantity     = 1;
        station.interactRange    = 3f;
        station.cancelRadius     = 4f;

        go.AddComponent<Mirror.NetworkIdentity>();
        var networkState = go.AddComponent<GatheringNodeNetworkState>();
        networkState.persistentNodeId = GUID.Generate().ToString();
        networkState.minimumAwardsPerSpawn = 1;
        networkState.maximumAwardsPerSpawn = 5;
        networkState.respawnSeconds = 900f;
        networkState.interactionRange = station.interactRange;
        networkState.minimumSecondsBetweenAwards = Mathf.Max(0.1f, station.tickInterval - 0.25f);

        // Atmospheric point light (weak)
        var lightGO         = new GameObject("Light");
        lightGO.transform.SetParent(go.transform, false);
        lightGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        var light           = lightGO.AddComponent<Light>();
        light.type          = LightType.Point;
        light.color         = cfg.meshColor;
        light.intensity     = 0.6f;
        light.range         = 4f;

        Undo.RegisterCreatedObjectUndo(go, $"Create {cfg.stationName}");
        Selection.activeGameObject = go;

        Debug.Log($"[BCE] {cfg.stationName} placed. " +
                  $"Profession {cfg.professionId}, level {cfg.minLevel}+, " +
                  $"item '{cfg.itemId}', {cfg.tickInterval}s tick.");
    }
}
#endif // UNITY_EDITOR || !UNITY_SERVER
