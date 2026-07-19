#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mirror;

/// <summary>
/// BCE/Setup/10 — Gathering Zone Builder
///
/// Creates Assets/Game/Scenes/Gathering Zone.unity and populates it with real
/// art assets from the project:
///
///   • Ground plane (tagged Ground)
///   • Directional light + ambient FireFlies + DustMotes
///   • 4 NetworkStartPositions near the entrance portal
///   • 3 AfkGatheringStations wired with real models + tick VFX:
///       - Timber Stand   (S_Tree_E prefab,  WoodImpacts  VFX)
///       - Crystal Spring (S_Cattail_A/B,    BigSplash    VFX)  ← Tripo model pending
///       - Ore Deposit    (Copper.fbx,        StoneImpacts VFX)
///   • ForgeNPC crafting bench (cube placeholder + TinyFlames + Steam)  ← Tripo pending
///   • HubReturnTrigger portal (circular portal model + NetworkIdentity)
///   • Decorative vegetation scattered around the zone
///
/// Also patches HUB scene WaypointMapTrigger serialized data.
///
/// After running:
///   1. Window → AI → Navigation → Bake
///   2. Ctrl+S (Mirror sceneId bake)
///   3. File → Build Settings → Add Open Scenes
///   4. Rebuild + redeploy server
/// </summary>
public static class GatheringZoneBuilder
{
    // ── Scene ────────────────────────────────────────────────────────────────────
    const string ScenePath = SceneNames.GatheringZonePath;
    const string HubPath   = SceneNames.HubPath;

    // ── Visual models ────────────────────────────────────────────────────────────
    const string VegRoot      = "Assets/brbMuff Folder/Vegetation_Stylized_Pack_ByLuxArtStudios/Prefabs";
    const string TreeA        = VegRoot + "/Trees/S_Tree_A.prefab";
    const string TreeE        = VegRoot + "/Trees/S_Tree_E.prefab";
    const string TreeG        = VegRoot + "/Trees/S_Tree_G.prefab";
    const string BushB        = VegRoot + "/Bushes/S_Bush_B.prefab";
    const string BushD        = VegRoot + "/Bushes/S_Bush_D.prefab";
    const string CattailA     = VegRoot + "/Bushes/S_Cattail_A.prefab";
    const string CattailB     = VegRoot + "/Bushes/S_Cattail_B.prefab";
    const string FlowersA     = VegRoot + "/Bushes/S_Flowers_A.prefab";
    const string CopperFbx    = "Assets/brbMuff Folder/Metal Ore/Models/Copper.fbx";

    // Tripo-generated models
    const string FishingSpotGlb  = "Assets/Game/3D Models/Gathering Zone/model_fishing_spot.glb";
    const string ForgeGlb        = "Assets/Game/3D Models/Gathering Zone/model_forge_workbench.glb";

    // Portal model (existing in project)
    const string PortalFbx    = "Assets/Game/3D Models/Portals/circular+portal+3d+model.fbm/circular+portal+3d+model.fbx";

    // ── VFX prefabs ──────────────────────────────────────────────────────────────
    const string FxRoot      = "Assets/Game/FX/Particle Pack/EffectExamples";
    const string VfxWood     = FxRoot + "/Weapon Effects/Prefabs/WoodImpacts.prefab";
    const string VfxStone    = FxRoot + "/Weapon Effects/Prefabs/StoneImpacts.prefab";
    const string VfxSplash   = FxRoot + "/Water Effects/Prefabs/BigSplash.prefab";
    const string VfxFireFly  = FxRoot + "/Misc Effects/Prefabs/FireFlies.prefab";
    const string VfxDust     = FxRoot + "/Misc Effects/Prefabs/DustMotesEffect.prefab";
    const string VfxFlames   = FxRoot + "/Fire & Explosion Effects/Prefabs/TinyFlames.prefab";
    const string VfxSteam    = FxRoot + "/Smoke & Steam Effects/Prefabs/Steam.prefab";

    // ─────────────────────────────────────────────────────────────────────────────

    [MenuItem("BCE/Setup/10 ▶ Gathering Zone — Build Scene", priority = 52)]
    static void Run()
    {
        var report = new StringBuilder();
        report.AppendLine("── Gathering Zone Builder ─────────────────────────────────");

        // ── 1. Open or create scene ──────────────────────────────────────────────
        bool existed = System.IO.File.Exists(
            System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), ScenePath));

        var scene = existed
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("Gathering Zone Builder",
                $"Failed to open/create scene at {ScenePath}.", "OK");
            return;
        }
        report.AppendLine(existed ? "  ✓ Reopened existing scene" : "  ✓ Created new scene");

        Vector3 center   = Vector3.zero;
        Vector3 entrance = center + new Vector3(0f, 0.1f, -30f);

        // ── 2. Directional light ─────────────────────────────────────────────────
        if (GameObject.FindFirstObjectByType<Light>() == null)
        {
            var lg    = new GameObject("Directional Light");
            var light = lg.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.intensity = 1.15f;
            light.color     = new Color(1f, 0.97f, 0.86f);
            lg.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            report.AppendLine("  ✓ Directional Light");
        }

        // ── 3. Ground plane ──────────────────────────────────────────────────────
        if (GameObject.Find("Ground") == null)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Plane);
            g.name = "Ground";
            g.tag  = "Ground";
            g.transform.position   = center;
            g.transform.localScale = new Vector3(8f, 1f, 8f);
            var rend = g.GetComponent<Renderer>();
            if (rend != null)
                rend.sharedMaterial = new Material(Shader.Find("Standard"))
                    { color = new Color(0.33f, 0.52f, 0.26f) };
            report.AppendLine("  ✓ Ground plane (80m × 80m)");
        }

        // ── 4. Ambient VFX ───────────────────────────────────────────────────────
        PlaceAmbientVFX(center, report);

        // ── 5. Player spawn points ───────────────────────────────────────────────
        var spawnParent = GetOrCreate("PlayerSpawnPoints");
        if (spawnParent.transform.childCount == 0)
        {
            Vector3[] spawns = {
                entrance + new Vector3( 2f, 0f,  1f),
                entrance + new Vector3(-2f, 0f,  1f),
                entrance + new Vector3( 2f, 0f, -1f),
                entrance + new Vector3(-2f, 0f, -1f),
            };
            foreach (var pos in spawns)
            {
                var sp = new GameObject("NetworkStartPosition");
                sp.transform.SetParent(spawnParent.transform);
                sp.transform.position = pos;
                sp.AddComponent<NetworkStartPosition>();
            }
            report.AppendLine("  ✓ 4 NetworkStartPositions");
        }

        // ── 6. Hub return portal ─────────────────────────────────────────────────
        PlacePortal(entrance, report);

        // ── 7. AFK Gathering Stations ────────────────────────────────────────────
        PlaceGatheringStations(center, report);

        // ── 8. Forge NPC ─────────────────────────────────────────────────────────
        PlaceForge(center, report);

        // ── 9. Decorative vegetation ─────────────────────────────────────────────
        PlaceDecorativeVeg(center, report);

        // ── 10. Save ─────────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
        report.AppendLine(saved ? $"  ✓ Saved → {ScenePath}" : $"  ✗ Save FAILED");

        // ── 11. Patch HUB scene waypoint map ────────────────────────────────────
        PatchHubWaypointMap(report);

        report.AppendLine();
        report.AppendLine("REQUIRED EDITOR STEPS:");
        report.AppendLine("  1. Window → AI → Navigation → Bake");
        report.AppendLine("  2. Ctrl+S  (Mirror sceneId bake)");
        report.AppendLine("  3. File → Build Settings → Add Open Scenes");
        report.AppendLine("  4. Rebuild + redeploy server");
        report.AppendLine();
        report.AppendLine("Tripo models: fishing_spot + forge_workbench in");
        report.AppendLine("  Assets/Game/3D Models/Gathering Zone/");
        report.AppendLine("  If Unity hasn't imported them yet, re-run this builder after");
        report.AppendLine("  Unity refreshes the asset database (Ctrl+R).");

        EditorUtility.DisplayDialog("Gathering Zone Builder", report.ToString(), "Done");
    }

    // ── Portal (circular portal model + NetworkIdentity + HubReturnTrigger) ───────

    static void PlacePortal(Vector3 entrance, StringBuilder r)
    {
        if (GameObject.Find("HubReturnPortal") != null) return;

        var portalGO = new GameObject("HubReturnPortal");
        portalGO.transform.position = entrance + new Vector3(0f, 0f, -3f);

        portalGO.AddComponent<NetworkIdentity>();
        var trigger                      = portalGO.AddComponent<HubReturnTrigger>();
        trigger.saveProgressBeforeReturn = true;
        trigger.runArenaCleanup          = false;
        trigger.promptText               = "Return to Hub";

        // Try real portal model; fallback to sphere
        var portalModel = AssetDatabase.LoadAssetAtPath<GameObject>(PortalFbx);
        if (portalModel != null)
        {
            var vis = (GameObject)PrefabUtility.InstantiatePrefab(portalModel, portalGO.transform);
            vis.name = "PortalMesh";
            vis.transform.localPosition = new Vector3(0f, 0f, 0f);
            vis.transform.localScale    = new Vector3(1.5f, 1.5f, 1.5f);
            r.AppendLine("  ✓ HubReturnPortal — circular portal model");
        }
        else
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "PortalMarker";
            sphere.transform.SetParent(portalGO.transform);
            sphere.transform.localPosition = new Vector3(0f, 1f, 0f);
            sphere.transform.localScale    = new Vector3(0.8f, 0.8f, 0.8f);
            r.AppendLine("  ⚠ HubReturnPortal — portal model not found, using sphere placeholder");
        }
    }

    // ── AFK Gathering Stations ────────────────────────────────────────────────────

    static void PlaceGatheringStations(Vector3 center, StringBuilder r)
    {
        // VFX prefab refs
        var vfxWood  = AssetDatabase.LoadAssetAtPath<GameObject>(VfxWood);
        var vfxStone = AssetDatabase.LoadAssetAtPath<GameObject>(VfxStone);
        var vfxSplash = AssetDatabase.LoadAssetAtPath<GameObject>(VfxSplash);

        // Model refs
        var treeE   = AssetDatabase.LoadAssetAtPath<GameObject>(TreeE);
        var copperFbx = AssetDatabase.LoadAssetAtPath<GameObject>(CopperFbx);
        var cattailA  = AssetDatabase.LoadAssetAtPath<GameObject>(CattailA);
        var cattailB  = AssetDatabase.LoadAssetAtPath<GameObject>(CattailB);

        // Station layout: triangle around center
        var defs = new[]
        {
            new StationDef
            {
                name     = "Timber Stand",
                profId   = 0,
                itemId   = "material_wood",
                xp       = 12,
                tick     = 6f,
                pos      = center + new Vector3(-15f, 0f, 10f),
                visualPrefab   = treeE,
                visualScale    = new Vector3(1.2f, 1.2f, 1.2f),
                visualOffset   = new Vector3(0f, 0f, 0f),
                tickVFXPrefab  = vfxWood,
                animBool       = "isChopping",
            },
            new StationDef
            {
                name     = "Crystal Spring",
                profId   = 1,
                itemId   = "fish_raw",
                xp       = 10,
                tick     = 7f,
                pos      = center + new Vector3(15f, 0f, 10f),
                visualPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>(FishingSpotGlb) ?? cattailA,
                visualScale    = new Vector3(1.5f, 1.5f, 1.5f),
                visualOffset   = Vector3.zero,
                tickVFXPrefab  = vfxSplash,
                animBool       = "isFishing",
            },
            new StationDef
            {
                name     = "Ore Deposit",
                profId   = 2,
                itemId   = "ore_copper",
                xp       = 15,
                tick     = 8f,
                pos      = center + new Vector3(0f, 0f, 25f),
                visualPrefab   = copperFbx,
                visualScale    = new Vector3(2f, 2f, 2f),
                visualOffset   = new Vector3(0f, 0f, 0f),
                tickVFXPrefab  = vfxStone,
                animBool       = "isMining",
            },
        };

        int placed = 0;
        foreach (var def in defs)
        {
            if (GameObject.Find(def.name) != null) continue;

            var go = new GameObject(def.name);
            go.transform.position = def.pos;

            var s               = go.AddComponent<AfkGatheringStation>();
            s.stationName       = def.name;
            s.professionId      = def.profId;
            s.itemId            = def.itemId;
            s.itemQuantity      = 1;
            s.tickInterval      = def.tick;
            s.xpPerTick         = def.xp;
            s.bonusYieldLevel   = 10;
            s.interactRange     = 3.5f;
            s.cancelRadius      = 5f;
            s.minLevelRequired  = 1;
            s.tickVFXPrefab     = def.tickVFXPrefab;
            s.gatheringAnimBool = def.animBool;

            // Trigger collider for range detection
            var col = go.AddComponent<SphereCollider>();
            col.radius    = s.interactRange;
            col.isTrigger = true;

            // Visual model
            if (def.name == "Crystal Spring")
            {
                // Cattail cluster — place A + B side by side
                AddVisualChild(go, cattailA, new Vector3(-0.5f, 0f, 0f), Vector3.one);
                AddVisualChild(go, cattailB, new Vector3( 0.5f, 0f, 0.3f), Vector3.one);
                // Second pair further back for density
                if (cattailA != null) AddVisualChild(go, cattailA, new Vector3( 0.3f, 0f, -0.4f), Vector3.one);
            }
            else
            {
                AddVisualChild(go, def.visualPrefab, def.visualOffset, def.visualScale);
            }

            placed++;
        }

        r.AppendLine($"  ✓ {placed} gathering station(s) placed");
        if (vfxWood  == null) r.AppendLine($"    ⚠ WoodImpacts VFX not found: {VfxWood}");
        if (vfxStone == null) r.AppendLine($"    ⚠ StoneImpacts VFX not found: {VfxStone}");
        if (vfxSplash == null) r.AppendLine($"    ⚠ BigSplash VFX not found: {VfxSplash}");
    }

    // ── Forge NPC ─────────────────────────────────────────────────────────────────

    static void PlaceForge(Vector3 center, StringBuilder r)
    {
        if (GameObject.Find("ForgeStation") != null) return;

        var go = new GameObject("ForgeStation");
        go.transform.position = center + new Vector3(0f, 0f, 5f);

        var forge           = go.AddComponent<ForgeNPC>();
        forge.npcName       = "Crafting Bench";
        forge.professionId  = 0;
        forge.interactRange = 4f;

        // Tripo-generated forge model; cube fallback if GLB not yet imported
        var forgeModel = AssetDatabase.LoadAssetAtPath<GameObject>(ForgeGlb);
        if (forgeModel != null)
        {
            var vis = (GameObject)PrefabUtility.InstantiatePrefab(forgeModel, go.transform);
            vis.name = "ForgeMesh";
            vis.transform.localPosition = new Vector3(0f, 0f, 0f);
            vis.transform.localScale    = Vector3.one * 1.2f;
        }
        else
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "ForgeMesh";
            block.transform.SetParent(go.transform);
            block.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            block.transform.localScale    = new Vector3(1.4f, 0.9f, 0.9f);
            var rend2 = block.GetComponent<Renderer>();
            if (rend2 != null)
                rend2.sharedMaterial = new Material(Shader.Find("Standard"))
                    { color = new Color(0.22f, 0.22f, 0.22f) };
        }

        // Fire effect
        var flames = AssetDatabase.LoadAssetAtPath<GameObject>(VfxFlames);
        if (flames != null)
        {
            var f = (GameObject)PrefabUtility.InstantiatePrefab(flames, go.transform);
            f.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            f.transform.localScale    = Vector3.one * 0.6f;
        }

        // Steam effect
        var steam = AssetDatabase.LoadAssetAtPath<GameObject>(VfxSteam);
        if (steam != null)
        {
            var s = (GameObject)PrefabUtility.InstantiatePrefab(steam, go.transform);
            s.transform.localPosition = new Vector3(0.3f, 1.4f, 0f);
            s.transform.localScale    = Vector3.one * 0.5f;
        }

        r.AppendLine("  ✓ ForgeNPC + TinyFlames + Steam (placeholder cube — swap for Tripo model)");
    }

    // ── Ambient VFX ───────────────────────────────────────────────────────────────

    static void PlaceAmbientVFX(Vector3 center, StringBuilder r)
    {
        if (GameObject.Find("AmbientVFX") != null) return;

        var root = new GameObject("AmbientVFX");
        root.transform.position = center;

        var fireflies = AssetDatabase.LoadAssetAtPath<GameObject>(VfxFireFly);
        if (fireflies != null)
        {
            var ff = (GameObject)PrefabUtility.InstantiatePrefab(fireflies, root.transform);
            ff.transform.localPosition = new Vector3(0f, 1f, 0f);
            ff.transform.localScale    = Vector3.one * 3f;
        }

        var dust = AssetDatabase.LoadAssetAtPath<GameObject>(VfxDust);
        if (dust != null)
        {
            var d = (GameObject)PrefabUtility.InstantiatePrefab(dust, root.transform);
            d.transform.localPosition = new Vector3(5f, 0.5f, 8f);
            d.transform.localScale    = Vector3.one * 2f;
        }

        r.AppendLine(
            $"  ✓ Ambient VFX — FireFlies={fireflies != null}, DustMotes={dust != null}");
    }

    // ── Decorative vegetation ─────────────────────────────────────────────────────

    static void PlaceDecorativeVeg(Vector3 center, StringBuilder r)
    {
        if (GameObject.Find("DecorativeVegetation") != null) return;

        var root = new GameObject("DecorativeVegetation");
        root.transform.position = center;

        // Paths + offsets for scattered decoration
        var vegDefs = new[]
        {
            (TreeA,   new Vector3(-25f, 0f, -20f), 1f),
            (TreeA,   new Vector3( 25f, 0f, -18f), 0.9f),
            (TreeG,   new Vector3(-22f, 0f,  18f), 1.1f),
            (TreeG,   new Vector3( 22f, 0f,  22f), 1f),
            (BushB,   new Vector3(-10f, 0f, -15f), 1f),
            (BushD,   new Vector3( 12f, 0f, -12f), 1f),
            (FlowersA,new Vector3( -5f, 0f,   2f), 1f),
            (FlowersA,new Vector3(  8f, 0f,   0f), 1f),
        };

        int placed = 0;
        foreach (var (path, pos, scale) in vegDefs)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            instance.transform.position    = center + pos;
            instance.transform.localScale  = Vector3.one * scale;
            instance.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
            placed++;
        }

        r.AppendLine($"  ✓ {placed} decorative vegetation objects");
    }

    // ── HUB waypoint map patch ────────────────────────────────────────────────────

    static void PatchHubWaypointMap(StringBuilder r)
    {
        if (!System.IO.File.Exists(
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), HubPath)))
        {
            r.AppendLine("  ⚠ HUB scene not found — patch waypoint map manually");
            return;
        }

        var hubScene = EditorSceneManager.OpenScene(HubPath, OpenSceneMode.Additive);

        WaypointMapTrigger map = null;
        foreach (var go in hubScene.GetRootGameObjects())
        {
            map = go.GetComponentInChildren<WaypointMapTrigger>(true);
            if (map != null) break;
        }

        if (map == null)
        {
            r.AppendLine("  ⚠ WaypointMapTrigger not found in HUB");
            EditorSceneManager.CloseScene(hubScene, true);
            return;
        }

        bool patched = false;
        for (int i = 0; i < map.nodes.Length; i++)
        {
            if (map.nodes[i].id != "gathering") continue;
            if (map.nodes[i].sceneName != SceneNames.GatheringZone || !map.nodes[i].unlocked)
            {
                map.nodes[i].sceneName   = SceneNames.GatheringZone;
                map.nodes[i].unlocked    = true;
                map.nodes[i].subtitle    = "harvest · craft · relax";
                map.nodes[i].description = "A peaceful zone for AFK woodcutting, fishing, and mining. A forge is available for crafting.";
                EditorUtility.SetDirty(map);
                patched = true;
            }
            break;
        }

        if (patched)
        {
            EditorSceneManager.MarkSceneDirty(hubScene);
            EditorSceneManager.SaveScene(hubScene);
            r.AppendLine("  ✓ HUB WaypointMapTrigger gathering node patched + saved");
        }
        else
        {
            r.AppendLine("  ✓ HUB WaypointMapTrigger gathering node already correct");
        }

        EditorSceneManager.CloseScene(hubScene, false);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────────

    static void AddVisualChild(GameObject parent, GameObject prefab,
                               Vector3 localPos, Vector3 localScale)
    {
        if (prefab == null) return;
        var vis = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
        vis.transform.localPosition = localPos;
        vis.transform.localScale    = localScale;
    }

    static GameObject GetOrCreate(string name)
    {
        var found = GameObject.Find(name);
        return found != null ? found : new GameObject(name);
    }

    struct StationDef
    {
        public string     name;
        public int        profId;
        public string     itemId;
        public int        xp;
        public float      tick;
        public Vector3    pos;
        public GameObject visualPrefab;
        public Vector3    visualScale;
        public Vector3    visualOffset;
        public GameObject tickVFXPrefab;
        public string     animBool;
    }

    [MenuItem("BCE/Setup/10 ▶ Gathering Zone — Build Scene", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
