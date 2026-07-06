#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

/// <summary>
/// NullArchitectRoomBuilder — BCE/Setup/10a
///
/// Deterministically builds the Null Architect void-cathedral boss arena into
/// the VoidDungeon scene.  Run this BEFORE BCE/Setup/6 (which creates the boss
/// GameObject — save it as a prefab, then assign it to NullArchitectArenaStarter).
///
/// What this builder does (all cosmetic unless noted):
///   1. Render settings — thick violet exponential fog, near-zero ambient, URP Volume stub
///   2. Lighting — faint void directional, 8 god-ray spots, floor rune glows, boss halo
///   3. Floor — 80×80 rune-stone plane with NavMeshSurface + emissive seam strips
///   4. Cathedral bones — 8 broken pillars (r = 38 u), 4 partial arch ribs, scattered debris
///   5. Void seam tears — 6 cracked-wall emissive strips + particle emitters
///   6. VFX — drifting particulate dome, rune-floor glow rings
///   7. Reused brbmuffins light-pillar prefabs at 4 god-ray positions
///   8. Boss spawn marker + 3 shard anchor reference empties (cosmetic)
///   9. Player spawn points (NetworkStartPosition, 4 × r = 8 u)
///  10. Networked scene objects: NullArchitectArenaStarter, ArenaSessionController, RodChatManager
///  11. Invisible boundary walls, return portal stub, camera
///  12. Save VoidDungeon scene + register in Build Settings
///
/// DOES NOT place the boss in the scene.  The boss must be a prefab in
/// NetworkManager.spawnPrefabs — server-spawned via NullArchitectArenaStarter.
///
/// Art direction: void-purple death fog, cathedral-city drowned at a seam of
/// realities, near-zero ambient, god-ray pale void-light through fractured bones,
/// rune-stone floor bleeding purple glow, drifting particulate, void seams tearing.
/// Phase colour arc: Phase 1 cyan-violet → Phase 2 orange-into-purple → Phase 3 deep red.
/// </summary>
public static class NullArchitectRoomBuilder
{
    // ── Paths ────────────────────────────────────────────────────────────────────
    const string ScenePath  = "Assets/Game/Scenes/VoidDungeon.unity";
    const string PrefabDir  = "Assets/Game/Prefabs";

    // Reused brbmuffins VFX prefab paths — these must exist in project
    const string PathLightPillar    = "Assets/brbmuffins VFX/brbmuffins Free VFX/Prefab/FX_LightPillar.prefab";
    const string PathTrailVoid      = "Assets/brbmuffins Trails/brbmuffins Trails VFX/VFX/Particles/VFX_Trail_Void.prefab";
    const string PathTrailDark      = "Assets/brbmuffins Trails/brbmuffins Trails VFX/VFX/Particles/VFX_Trail_Dark.prefab";
    const string PathDeathCircle    = "Assets/brbmuffins Dark Arts/brbmuffins Fantasy Pack/Prefabs/Effects normal/Death magic circle.prefab";
    const string PathMagicCircle    = "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Magic circles/Magic circle.prefab";
    const string PathCrystalBlue    = "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Environment/Crystal effect blue.prefab";
    const string PathSmokeExplosion = "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/AoE effects/Smoke AOE explosion.prefab";
    const string PathRedExplosion   = "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/AoE effects/Red energy explosion.prefab";
    const string PathPurpleHit      = "Assets/brbmuffins VFX/brbmuffins Free VFX/Prefab/FX_Purple_Hit_02.prefab";
    const string PathSlashPurple    = "Assets/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Slash effects/Charge slash purple.prefab";
    const string PathGlowOrbs       = "Assets/brbmuffins Dark Arts/brbmuffins Fantasy Pack/Prefabs/Glowing orbs.prefab";

    // ── Colours (Void art direction) ─────────────────────────────────────────────
    static readonly Color ColFog           = new Color(0.13f, 0.03f, 0.20f, 1f);
    static readonly Color ColAmbient       = new Color(0.018f, 0.008f, 0.032f, 1f);
    static readonly Color ColSun           = new Color(0.18f, 0.08f, 0.30f, 1f);
    static readonly Color ColGodRay        = new Color(0.72f, 0.68f, 1.00f, 1f);   // pale void-light
    static readonly Color ColBossHalo      = new Color(0.22f, 0.04f, 0.42f, 1f);
    static readonly Color ColRuneGlow      = new Color(0.45f, 0.08f, 0.90f, 1f);
    static readonly Color ColFloor         = new Color(0.10f, 0.07f, 0.16f, 1f);
    static readonly Color ColPillar        = new Color(0.14f, 0.10f, 0.20f, 1f);
    static readonly Color ColSeamEmissive  = new Color(0.60f, 0.10f, 1.00f, 1f);   // bright purple crack
    static readonly Color ColVoidParticle  = new Color(0.55f, 0.10f, 1.00f, 1f);
    static readonly Color ColDeathFog      = new Color(0.08f, 0.02f, 0.14f, 1f);   // debris/corner fog

    // ── Menu entry ───────────────────────────────────────────────────────────────

    [MenuItem("BCE/Setup/10a ▶ Build Null Architect Room (VoidDungeon)")]
    public static void BuildNullArchitectRoom()
    {
        // Guard: don't accidentally nuke the Hub
        var active = EditorSceneManager.GetActiveScene();
        if (active.name == "Hub")
        {
            bool ok = EditorUtility.DisplayDialog("Build Null Architect Room",
                "Active scene is the Hub.\n" +
                "This will open/overwrite VoidDungeon.unity — continue?", "Yes", "Cancel");
            if (!ok) return;
        }

        // Open VoidDungeon.  If unsaved work exists, offer to save.
        if (!string.IsNullOrEmpty(active.path) && active.path != ScenePath)
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorSceneManager.SetActiveScene(scene);

        // ── 1. Render settings ───────────────────────────────────────────────────
        SetupRenderSettings();

        // ── 2. Lighting ──────────────────────────────────────────────────────────
        var lightRoot = GetOrCreate("_Lighting");
        BuildDirectionalLight(lightRoot);
        BuildGodRayLights(lightRoot);
        BuildFloorRuneLights(lightRoot);
        BuildBossHaloLight(lightRoot);

        // ── 3. Floor ─────────────────────────────────────────────────────────────
        var floorGO = BuildFloor();

        // ── 4. Cathedral bones ────────────────────────────────────────────────────
        var geomRoot = GetOrCreate("_CathedralGeometry");
        BuildPillars(geomRoot);
        BuildArchRibs(geomRoot);
        BuildFloorDebris(geomRoot);

        // ── 5. Void seam tears ────────────────────────────────────────────────────
        var seamRoot = GetOrCreate("_VoidSeams");
        BuildVoidSeams(seamRoot);

        // ── 6. VFX (cosmetic, NO NetworkIdentity) ─────────────────────────────────
        var vfxRoot = GetOrCreate("_VFX_Cosmetic");
        BuildParticulateDome(vfxRoot);
        BuildRuneFloorEmitters(vfxRoot);
        PlaceReusedVFXPrefabs(vfxRoot);
        PlaceVoidSeamParticles(vfxRoot);

        // ── 7. Invisible boundary walls ───────────────────────────────────────────
        BuildBoundaryWalls();

        // ── 8. Boss spawn marker + shard anchors (cosmetic) ──────────────────────
        BuildBossMarkers();

        // ── 9. Player spawn points ────────────────────────────────────────────────
        BuildPlayerSpawns();

        // ── 10. Networked scene objects ───────────────────────────────────────────
        BuildNetworkedSceneObjects();

        // ── 11. Return portal stub ────────────────────────────────────────────────
        BuildReturnPortal();

        // ── 12. Camera ────────────────────────────────────────────────────────────
        BuildCamera();

        // ── Save + register ────────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, ScenePath);

        var buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!buildScenes.Exists(s => s.path == ScenePath))
        {
            buildScenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log("[BCE/10a] VoidDungeon added to Build Settings.");
        }

        Debug.Log(
            "[BCE/10a] Null Architect room built.\n\n" +
            "REQUIRED NEXT STEPS:\n" +
            "1. Window → AI → Navigation → Bake NavMesh on ArenaFloor\n" +
            "2. BCE/Setup/6 to create the boss GO; save it as a prefab:\n" +
            "   Assets/Game/Prefabs/NullArchitect_Boss.prefab\n" +
            "3. RodNetworkManager → Registered Spawnable Prefabs: add NullArchitect_Boss, NullShard, WorldItem\n" +
            "4. NullArchitectArenaStarter Inspector → Boss Prefab: assign NullArchitect_Boss.prefab\n" +
            "5. WorldBossController Inspector → assign VFX fields (see SCENE_SETUP.md)\n" +
            "6. Ctrl+S\n");
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Render Settings
    // ════════════════════════════════════════════════════════════════════════════

    static void SetupRenderSettings()
    {
        // Void: near-zero ambient, oppressive thick purple fog
        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = ColAmbient;

        RenderSettings.fog          = true;
        RenderSettings.fogMode      = FogMode.ExponentialSquared;
        RenderSettings.fogColor     = ColFog;
        RenderSettings.fogDensity   = 0.055f;   // thick but not instant black-out

        RenderSettings.skybox            = null;   // no skybox — pure black void
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        RenderSettings.reflectionIntensity   = 0.05f;

        // URP Global Volume — volumetric fog + post overrides
        // Placed as a scene object so the user can tune it in the Inspector
        if (Object.FindAnyObjectByType<UnityEngine.Rendering.Volume>() == null)
        {
            var volGO  = new GameObject("GlobalVolume_VoidCathedral");
            var vol    = volGO.AddComponent<UnityEngine.Rendering.Volume>();
            vol.isGlobal  = true;
            vol.priority  = 10;
            // Profile must be created & assigned manually (cannot create VolumeProfile
            // with specific overrides without knowing the exact URP version fields).
            // See SCENE_SETUP.md → Inspector assignments → step V1.
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Lighting
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildDirectionalLight(GameObject root)
    {
        var go = new GameObject("VoidMoon_Directional");
        go.transform.SetParent(root.transform, false);
        go.transform.rotation = Quaternion.Euler(28f, -55f, 0f);
        var l = go.AddComponent<Light>();
        l.type      = LightType.Directional;
        l.color     = ColSun;
        l.intensity = 0.28f;                  // barely there — room reads from point lights
        l.shadows   = LightShadows.Soft;
        l.shadowStrength = 0.9f;
    }

    static void BuildGodRayLights(GameObject root)
    {
        // 8 spots from above, simulating shafts through broken cathedral roof
        // Placed at alternating angles around the circle
        float[] angles  = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        float[] radii   = { 22f, 30f, 18f, 28f, 24f, 26f, 20f, 32f };
        float[] heights = { 22f, 18f, 24f, 20f, 22f, 18f, 26f, 21f };

        for (int i = 0; i < angles.Length; i++)
        {
            float rad = angles[i] * Mathf.Deg2Rad;
            var pos   = new Vector3(Mathf.Sin(rad) * radii[i], heights[i], Mathf.Cos(rad) * radii[i]);

            var go = new GameObject($"GodRay_{i:00}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(88f, angles[i], 0f); // nearly straight down

            var l = go.AddComponent<Light>();
            l.type      = LightType.Spot;
            l.color     = ColGodRay;
            l.intensity = 2.8f + (i % 3) * 0.4f;
            l.range     = heights[i] + 6f;
            l.spotAngle = 7f + (i % 4) * 1.5f;   // narrow — god-ray feel
            l.shadows   = LightShadows.None;       // performance
        }
    }

    static void BuildFloorRuneLights(GameObject root)
    {
        // 4 low point lights at cardinal 20u — rune-stone floor glow
        var cardinals = new Vector3[]
        {
            new Vector3( 20f, 0.15f,   0f),
            new Vector3(-20f, 0.15f,   0f),
            new Vector3(  0f, 0.15f,  20f),
            new Vector3(  0f, 0.15f, -20f),
        };
        foreach (var pos in cardinals)
        {
            var go = new GameObject("RuneGlow_Floor");
            go.transform.SetParent(root.transform, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type      = LightType.Point;
            l.color     = ColRuneGlow;
            l.intensity = 1.8f;
            l.range     = 14f;
            l.shadows   = LightShadows.None;
        }

        // 4 more at diagonal 30u for wider coverage
        var diags = new Vector3[]
        {
            new Vector3( 22f, 0.1f,  22f),
            new Vector3(-22f, 0.1f,  22f),
            new Vector3( 22f, 0.1f, -22f),
            new Vector3(-22f, 0.1f, -22f),
        };
        foreach (var pos in diags)
        {
            var go = new GameObject("RuneGlow_Diag");
            go.transform.SetParent(root.transform, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type      = LightType.Point;
            l.color     = new Color(0.35f, 0.05f, 0.70f, 1f);
            l.intensity = 1.2f;
            l.range     = 10f;
            l.shadows   = LightShadows.None;
        }
    }

    static void BuildBossHaloLight(GameObject root)
    {
        // Enormous point light above boss origin — the void presence
        var go = new GameObject("BossHalo_Point");
        go.transform.SetParent(root.transform, false);
        go.transform.position = new Vector3(0f, 9f, 0f);
        var l = go.AddComponent<Light>();
        l.type      = LightType.Point;
        l.color     = ColBossHalo;
        l.intensity = 7.5f;
        l.range     = 38f;
        l.shadows   = LightShadows.Soft;
        l.shadowStrength = 0.7f;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Floor
    // ════════════════════════════════════════════════════════════════════════════

    static GameObject BuildFloor()
    {
        // Core floor plane — 80×80 (plane default = 10×10, scale ×8)
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ArenaFloor";
        floor.tag  = "Ground";
        floor.transform.position   = Vector3.zero;
        floor.transform.localScale = new Vector3(8f, 1f, 8f);

        var mat  = BuildMaterial("M_RuneStoneFloor", ColFloor, emissiveColor: ColRuneGlow * 0.12f);
        floor.GetComponent<Renderer>().sharedMaterial = mat;

        // Replace MeshCollider with BoxCollider (NavMesh-friendly)
        Object.DestroyImmediate(floor.GetComponent<MeshCollider>());
        var bc   = floor.AddComponent<BoxCollider>();
        bc.size  = new Vector3(1f, 0.04f, 1f);

        // NavMeshSurface — bake after running builder
        var surfaceType = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
        if (surfaceType != null)
            floor.AddComponent(surfaceType);
        else
            Debug.LogWarning("[BCE/10a] NavMeshSurface not found — add manually to ArenaFloor.");

        // Emissive rune seam strips (cross pattern — the rune bleeds through the floor)
        var runeRoot = new GameObject("RuneSeams");
        runeRoot.transform.SetParent(floor.transform, false);
        BuildRuneSeam(runeRoot, new Vector3(0f, 0.01f, 0f),  new Vector3(60f, 0.04f, 0.5f));   // E-W main
        BuildRuneSeam(runeRoot, new Vector3(0f, 0.01f, 0f),  new Vector3(0.5f, 0.04f, 60f));   // N-S main
        BuildRuneSeam(runeRoot, new Vector3(0f, 0.01f, 0f),  new Vector3(35f, 0.04f, 0.3f), 45f);   // diagonal
        BuildRuneSeam(runeRoot, new Vector3(0f, 0.01f, 0f),  new Vector3(35f, 0.04f, 0.3f), -45f);  // diagonal

        // 8-pointed rune ring at boss radius (10u)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Sin(angle) * 10f, 0.01f, Mathf.Cos(angle) * 10f);
            BuildRuneSeam(runeRoot, pos, new Vector3(3f, 0.04f, 0.25f), i * 45f);
        }

        return floor;
    }

    static void BuildRuneSeam(GameObject parent, Vector3 localPos, Vector3 localScale, float rotY = 0f)
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "RuneSeam";
        strip.transform.SetParent(parent.transform, false);
        strip.transform.localPosition = localPos;
        strip.transform.localScale    = localScale;
        strip.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
        Object.DestroyImmediate(strip.GetComponent<BoxCollider>());
        var mat = BuildMaterial("M_RuneSeamEmissive", new Color(0.02f, 0.01f, 0.05f),
                                emissiveColor: ColSeamEmissive * 2.5f);
        strip.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Cathedral Bones
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildPillars(GameObject root)
    {
        // 8 broken cathedral pillars at radius 38u — alternating heights for drama
        float[] heights = { 14f, 10f, 18f, 8f, 16f, 12f, 15f, 9f };
        float   radius  = 38f;

        for (int i = 0; i < 8; i++)
        {
            float angle  = (i / 8f) * 360f * Mathf.Deg2Rad;
            var   center = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
            float h      = heights[i];

            var pillarRoot = new GameObject($"Pillar_{i:00}");
            pillarRoot.transform.SetParent(root.transform, false);
            pillarRoot.transform.position = center;

            // Main shaft
            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shaft.name = "Shaft";
            shaft.transform.SetParent(pillarRoot.transform, false);
            shaft.transform.localPosition = new Vector3(0f, h * 0.5f, 0f);
            shaft.transform.localScale    = new Vector3(2.8f, h, 2.8f);
            shaft.transform.localRotation = Quaternion.Euler(0f, i * 22.5f, Random.Range(-1.5f, 1.5f));
            Object.DestroyImmediate(shaft.GetComponent<BoxCollider>());
            shaft.AddComponent<BoxCollider>(); // collision for player pathing
            shaft.GetComponent<Renderer>().sharedMaterial = BuildMaterial("M_Pillar", ColPillar);

            // Broken cap (angled cube, crumbled top)
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = "Cap";
            cap.transform.SetParent(pillarRoot.transform, false);
            cap.transform.localPosition = new Vector3(
                Random.Range(-0.6f, 0.6f), h + 0.3f, Random.Range(-0.4f, 0.4f));
            cap.transform.localScale    = new Vector3(3.2f, 0.9f, 3.2f);
            cap.transform.localRotation = Quaternion.Euler(
                Random.Range(-15f, 15f), Random.Range(0f, 45f), Random.Range(-15f, 15f));
            Object.DestroyImmediate(cap.GetComponent<BoxCollider>());
            cap.GetComponent<Renderer>().sharedMaterial = BuildMaterial("M_Pillar", ColPillar);

            // Void seam crack down one face of pillar (emissive strip)
            var crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crack.name = "VoidCrack";
            crack.transform.SetParent(pillarRoot.transform, false);
            crack.transform.localPosition = new Vector3(1.41f, h * 0.4f, 0f); // front face
            crack.transform.localScale    = new Vector3(0.08f, h * 0.7f, 0.3f);
            Object.DestroyImmediate(crack.GetComponent<BoxCollider>());
            crack.GetComponent<Renderer>().sharedMaterial = BuildMaterial("M_PillarCrack",
                new Color(0.01f, 0.0f, 0.03f), emissiveColor: ColSeamEmissive * 1.8f);
        }
    }

    static void BuildArchRibs(GameObject root)
    {
        // 4 partial arch ribs connecting pillar tops — broken Gothic cathedral feel
        // Each rib is a pair of angled beams meeting at a peak
        var ribPairs = new (Vector3 a, Vector3 b)[]
        {
            (new Vector3(-38f, 14f, 0f),  new Vector3( 38f, 14f, 0f)),   // E-W
            (new Vector3(0f, 18f,  38f),  new Vector3(0f, 18f, -38f)),   // N-S
            (new Vector3(-27f, 10f, -27f), new Vector3(27f, 10f, 27f)),   // diagonal A
            (new Vector3(-27f, 12f, 27f),  new Vector3(27f, 12f, -27f)), // diagonal B
        };

        int idx = 0;
        foreach (var (a, b) in ribPairs)
        {
            var mid    = (a + b) * 0.5f + Vector3.up * 5f;  // arch peak above midpoint
            BuildArchSegment(root, a, mid, $"Arch{idx}_Left");
            BuildArchSegment(root, b, mid, $"Arch{idx}_Right");
            idx++;
        }
    }

    static void BuildArchSegment(GameObject root, Vector3 from, Vector3 to, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(root.transform, false);

        var diff   = to - from;
        go.transform.position   = from + diff * 0.5f;
        go.transform.up         = diff.normalized;
        go.transform.localScale = new Vector3(1.4f, diff.magnitude, 1.4f);
        Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        go.GetComponent<Renderer>().sharedMaterial = BuildMaterial("M_Arch", ColPillar);
    }

    static void BuildFloorDebris(GameObject root)
    {
        // 10 fallen stone chunks scattered around the room perimeter
        var debrisPositions = new Vector3[]
        {
            new Vector3( 28f, 0f,  18f), new Vector3(-22f, 0f,  30f),
            new Vector3( 35f, 0f, -10f), new Vector3(-30f, 0f, -25f),
            new Vector3( 12f, 0f,  38f), new Vector3(-15f, 0f, -36f),
            new Vector3( 38f, 0f,  28f), new Vector3(-38f, 0f,  15f),
            new Vector3( 20f, 0f, -35f), new Vector3(-10f, 0f,  28f),
        };

        for (int i = 0; i < debrisPositions.Length; i++)
        {
            var chunk = new GameObject($"Debris_{i:00}");
            chunk.transform.SetParent(root.transform, false);
            chunk.transform.position = debrisPositions[i];

            int pieceCount = Random.Range(2, 5);
            for (int p = 0; p < pieceCount; p++)
            {
                var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.name = "Chunk";
                piece.transform.SetParent(chunk.transform, false);
                float s   = Random.Range(0.5f, 2.2f);
                piece.transform.localPosition = new Vector3(
                    Random.Range(-1.5f, 1.5f), s * 0.3f, Random.Range(-1.5f, 1.5f));
                piece.transform.localScale    = new Vector3(s, s * Random.Range(0.4f, 1.0f), s * 0.8f);
                piece.transform.localRotation = Random.rotation;
                Object.DestroyImmediate(piece.GetComponent<BoxCollider>());
                piece.GetComponent<Renderer>().sharedMaterial =
                    BuildMaterial("M_Debris", ColPillar * 0.85f);
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Void Seam Tears
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildVoidSeams(GameObject root)
    {
        // 6 seam tears placed at the perimeter where walls meet void
        var seamPositions = new (Vector3 pos, float rotY, float height)[]
        {
            (new Vector3( 38f, 6f,  10f), 90f, 8f),
            (new Vector3(-38f, 8f, -12f), -90f, 10f),
            (new Vector3(  8f, 5f,  38f), 0f, 7f),
            (new Vector3(-10f, 9f, -38f), 180f, 12f),
            (new Vector3( 38f, 4f, -22f), 90f, 6f),
            (new Vector3(-28f, 7f,  38f), -90f, 9f),
        };

        int idx = 0;
        foreach (var (pos, rotY, height) in seamPositions)
        {
            var seamGO = new GameObject($"VoidSeam_{idx:00}");
            seamGO.transform.SetParent(root.transform, false);
            seamGO.transform.position = pos;
            seamGO.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Main glowing crack strip
            var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = "SeamStrip";
            strip.transform.SetParent(seamGO.transform, false);
            strip.transform.localPosition = Vector3.zero;
            strip.transform.localScale    = new Vector3(0.15f, height, 0.5f);
            Object.DestroyImmediate(strip.GetComponent<BoxCollider>());
            strip.GetComponent<Renderer>().sharedMaterial = BuildMaterial(
                $"M_VoidSeam_{idx}", new Color(0.0f, 0.0f, 0.02f),
                emissiveColor: ColSeamEmissive * 3.5f);

            // Flanking hair-cracks (2 thin strips either side)
            for (int side = -1; side <= 1; side += 2)
            {
                var hair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hair.name = "HairCrack";
                hair.transform.SetParent(seamGO.transform, false);
                hair.transform.localPosition = new Vector3(side * 0.4f, Random.Range(-1f, 1f), 0f);
                hair.transform.localScale    = new Vector3(0.06f, height * Random.Range(0.4f, 0.8f), 0.15f);
                hair.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-8f, 8f));
                Object.DestroyImmediate(hair.GetComponent<BoxCollider>());
                hair.GetComponent<Renderer>().sharedMaterial = BuildMaterial(
                    $"M_HairCrack_{idx}_{side}", new Color(0f, 0f, 0.01f),
                    emissiveColor: ColSeamEmissive * 1.5f);
            }

            idx++;
        }

        // 3 ceiling void rifts (particles pour downward through broken roof)
        var ceilingRifts = new Vector3[]
        {
            new Vector3( 15f, 20f,  8f),
            new Vector3(-12f, 18f, -10f),
            new Vector3(  2f, 22f,  16f),
        };

        int r = 0;
        foreach (var pos in ceilingRifts)
        {
            var rift = new GameObject($"CeilingRift_{r:00}");
            rift.transform.SetParent(root.transform, false);
            rift.transform.position = pos;

            // Emissive disc
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "RiftDisc";
            disc.transform.SetParent(rift.transform, false);
            disc.transform.localPosition = Vector3.zero;
            disc.transform.localScale    = new Vector3(3f, 0.05f, 3f);
            Object.DestroyImmediate(disc.GetComponent<CapsuleCollider>());
            disc.GetComponent<Renderer>().sharedMaterial = BuildMaterial(
                $"M_CeilingRift_{r}", new Color(0f, 0f, 0.02f),
                emissiveColor: ColSeamEmissive * 4f);

            r++;
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // VFX — Cosmetic Only (NO NetworkIdentity)
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildParticulateDome(GameObject root)
    {
        // Large drifting void particulate — fills the entire room
        var go = new GameObject("VoidParticulateDome");
        go.transform.SetParent(root.transform, false);
        go.transform.position = new Vector3(0f, 8f, 0f);

        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop             = true;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(6f, 12f);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
        main.startSize        = new ParticleSystem.MinMaxCurve(0.03f, 0.12f);
        main.startColor       = new ParticleSystem.MinMaxGradient(
            ColVoidParticle * 0.3f, new Color(0.8f, 0.5f, 1f, 0.5f));
        main.maxParticles     = 2000;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.gravityModifier  = new ParticleSystem.MinMaxCurve(-0.004f, 0.008f); // slight drift

        var emission = ps.emission;
        emission.rateOverTime = 80f;

        var shape = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.Box;
        shape.scale        = new Vector3(80f, 16f, 80f);

        // Renderer — additive blend for soft glow
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode         = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge       = 10f;
    }

    static void BuildRuneFloorEmitters(GameObject root)
    {
        // 4 emitters at cardinal 12u — floor seam glow particles rising from runes
        var positions = new Vector3[]
        {
            new Vector3( 12f, 0.05f,   0f),
            new Vector3(-12f, 0.05f,   0f),
            new Vector3(  0f, 0.05f,  12f),
            new Vector3(  0f, 0.05f, -12f),
        };

        int i = 0;
        foreach (var pos in positions)
        {
            var go = new GameObject($"RuneGlowEmitter_{i:00}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = pos;

            var ps   = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop           = true;
            main.startLifetime  = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            main.startSpeed     = new ParticleSystem.MinMaxCurve(0.1f, 0.6f);
            main.startSize      = new ParticleSystem.MinMaxCurve(0.04f, 0.18f);
            main.startColor     = new ParticleSystem.MinMaxGradient(
                new Color(0.6f, 0.1f, 1f, 0.8f), new Color(0.3f, 0.05f, 0.7f, 0.4f));
            main.maxParticles   = 200;
            main.gravityModifier = new ParticleSystem.MinMaxCurve(-0.12f, -0.04f); // rise

            var emission = ps.emission;
            emission.rateOverTime = 20f;

            var shape = ps.shape;
            shape.enabled    = true;
            shape.shapeType  = ParticleSystemShapeType.Circle;
            shape.radius     = 2.5f;

            i++;
        }
    }

    static void PlaceVoidSeamParticles(GameObject root)
    {
        // Particle emitters at each void seam — sparking purple energy pours from the tears
        var seamPositions = new Vector3[]
        {
            new Vector3( 37.5f, 6f,  10f),
            new Vector3(-37.5f, 8f, -12f),
            new Vector3(  8f,   5f,  37.5f),
            new Vector3(-10f,   9f, -37.5f),
            new Vector3( 37.5f, 4f, -22f),
            new Vector3(-28f,   7f,  37.5f),
        };

        int i = 0;
        foreach (var pos in seamPositions)
        {
            var go = new GameObject($"SeamParticles_{i:00}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = pos;

            var ps   = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop          = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            main.startSpeed    = new ParticleSystem.MinMaxCurve(0.3f, 1.5f);
            main.startSize     = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(0.9f, 0.5f, 1f, 1f), ColSeamEmissive);
            main.maxParticles  = 150;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var shape = ps.shape;
            shape.enabled   = true;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale     = new Vector3(0.2f, 4f, 0.1f);

            i++;
        }

        // Ceiling rift downpour particles
        var riftPositions = new Vector3[]
        {
            new Vector3( 15f, 20f,  8f),
            new Vector3(-12f, 18f, -10f),
            new Vector3(  2f, 22f,  16f),
        };

        int r = 0;
        foreach (var pos in riftPositions)
        {
            var go = new GameObject($"RiftDownpour_{r:00}");
            go.transform.SetParent(root.transform, false);
            go.transform.position = pos;

            var ps   = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop          = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSpeed    = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize     = new ParticleSystem.MinMaxCurve(0.03f, 0.15f);
            main.startColor    = new ParticleSystem.MinMaxGradient(
                new Color(0.7f, 0.4f, 1f, 0.9f), new Color(0.4f, 0.1f, 0.8f, 0.5f));
            main.maxParticles  = 300;
            main.gravityModifier = new ParticleSystem.MinMaxCurve(0.4f, 0.8f); // fall down

            var emission = ps.emission;
            emission.rateOverTime = 50f;

            var shape = ps.shape;
            shape.enabled   = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius    = 1.2f;

            r++;
        }
    }

    static void PlaceReusedVFXPrefabs(GameObject root)
    {
        // Light pillar prefab — 4 positions for god-ray reinforcement
        var pillarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathLightPillar);
        if (pillarPrefab != null)
        {
            var pillarPositions = new Vector3[]
            {
                new Vector3( 22f, 0f,  8f),
                new Vector3(-18f, 0f, -14f),
                new Vector3(  6f, 0f,  24f),
                new Vector3(-24f, 0f,  18f),
            };
            int i = 0;
            foreach (var pos in pillarPositions)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pillarPrefab);
                inst.name = $"LightPillar_{i:00}";
                inst.transform.SetParent(root.transform, false);
                inst.transform.position = pos;
                inst.transform.localScale = new Vector3(0.6f, 1.4f, 0.6f);
                // No NetworkIdentity — cosmetic VFX
                i++;
            }
        }
        else
        {
            Debug.LogWarning("[BCE/10a] FX_LightPillar not found at expected path. God-ray pillars skipped.");
        }

        // Glowing orbs prefab — ambient void energy drifting near ceiling rifts
        var orbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathGlowOrbs);
        if (orbPrefab != null)
        {
            var orbPositions = new Vector3[]
            {
                new Vector3( 10f, 12f,  5f),
                new Vector3(-8f,  10f, -6f),
            };
            int i = 0;
            foreach (var pos in orbPositions)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(orbPrefab);
                inst.name = $"GlowOrb_{i:00}";
                inst.transform.SetParent(root.transform, false);
                inst.transform.position = pos;
                i++;
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Boundary Walls
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildBoundaryWalls()
    {
        float half = 40f;
        var walls  = new (Vector3 pos, Vector3 size)[]
        {
            (new Vector3( half, 5f, 0f),   new Vector3(1f, 10f, 82f)),
            (new Vector3(-half, 5f, 0f),   new Vector3(1f, 10f, 82f)),
            (new Vector3(0f, 5f,  half),   new Vector3(82f, 10f, 1f)),
            (new Vector3(0f, 5f, -half),   new Vector3(82f, 10f, 1f)),
        };
        foreach (var (pos, size) in walls)
        {
            var w = new GameObject("BoundaryWall");
            w.transform.position = pos;
            var bc = w.AddComponent<BoxCollider>();
            bc.size = size;
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Boss Markers (cosmetic)
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildBossMarkers()
    {
        // Boss spawn origin — server will spawn prefab here
        var spawnMarker = new GameObject("MARKER_BossSpawnOrigin");
        spawnMarker.transform.position = new Vector3(0f, 0f, 0f);
        // Not registered as NetworkIdentity — purely a visual reference in editor

        // 3 shard anchor positions — matches WorldBossController.SpawnShards offsets:
        // Vector3.forward, Vector3.left, Vector3.right * shardSpreadRadius (6u)
        var shardOffsets = new (string name, Vector3 offset)[]
        {
            ("MARKER_ShardAnchor_0_Forward", Vector3.forward * 6f),
            ("MARKER_ShardAnchor_1_Left",    Vector3.left    * 6f),
            ("MARKER_ShardAnchor_2_Right",   Vector3.right   * 6f),
        };
        foreach (var (name, offset) in shardOffsets)
        {
            var anchor = new GameObject(name);
            anchor.transform.position = offset;
        }

        // Boss trigger zone visual (cosmetic sphere — no collider, no NetworkIdentity)
        // Actual BossTrigger lives as child of boss prefab
        var triggerViz = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        triggerViz.name = "MARKER_BossTrigger15u_VISUAL_ONLY";
        triggerViz.transform.position   = Vector3.zero;
        triggerViz.transform.localScale = Vector3.one * 30f; // diameter = 30u = radius 15u
        Object.DestroyImmediate(triggerViz.GetComponent<SphereCollider>());
        var vizMat = BuildMaterial("M_TriggerVisualization",
            new Color(0.5f, 0.1f, 1f, 0.05f));
        var mr = triggerViz.GetComponent<MeshRenderer>();
        mr.sharedMaterial = vizMat;
        // Turn off shadows — pure editor guide
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows    = false;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Player Spawns
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildPlayerSpawns()
    {
        for (int i = 0; i < 4; i++)
        {
            float angle = (90f * i) * Mathf.Deg2Rad;
            var   sp    = new GameObject($"PlayerSpawn_{i}");
            sp.transform.position = new Vector3(Mathf.Sin(angle) * 8f, 0.1f, Mathf.Cos(angle) * 8f);
            sp.AddComponent<Mirror.NetworkStartPosition>();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Networked Scene Objects
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildNetworkedSceneObjects()
    {
        // NullArchitectArenaStarter — server-spawns boss on scene load
        if (Object.FindAnyObjectByType<NullArchitectArenaStarter>() == null)
        {
            var go = new GameObject("NullArchitectArenaStarter");
            go.AddComponent<Mirror.NetworkIdentity>();
            go.AddComponent<NullArchitectArenaStarter>();
            // Inspector: assign Boss Prefab after saving boss from BCE/Setup/6
        }

        // ArenaSessionController — tracks session XP + boss kill reward
        if (Object.FindAnyObjectByType<ArenaSessionController>() == null)
        {
            var go = new GameObject("ArenaSessionController");
            go.AddComponent<Mirror.NetworkIdentity>();
            go.AddComponent<ArenaSessionController>();
        }

        // RodChatManager — boss announcements via RpcAnnounce go through this
        if (Object.FindAnyObjectByType<RodChatManager>() == null)
        {
            var go = new GameObject("RodChatManager");
            go.AddComponent<Mirror.NetworkIdentity>();
            go.AddComponent<RodChatManager>();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Return Portal
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildReturnPortal()
    {
        var portal = new GameObject("ReturnPortal_Hub");
        portal.transform.position = new Vector3(0f, 0f, -38f);
        portal.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // Arch visual
        var arch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arch.name = "Arch";
        arch.transform.SetParent(portal.transform, false);
        arch.transform.localPosition = new Vector3(0f, 2f, 0f);
        arch.transform.localScale    = new Vector3(0.2f, 2f, 0.2f);
        Object.DestroyImmediate(arch.GetComponent<CapsuleCollider>());
        arch.GetComponent<Renderer>().sharedMaterial =
            BuildMaterial("M_PortalArch", new Color(0.3f, 0.1f, 0.5f),
                emissiveColor: new Color(0.3f, 0.1f, 0.6f) * 2f);

        // Light
        var lightGO = new GameObject("PortalLight");
        lightGO.transform.SetParent(portal.transform, false);
        lightGO.transform.localPosition = new Vector3(0f, 2f, 0f);
        var l = lightGO.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(0.4f, 0.2f, 0.9f);
        l.intensity = 4f; l.range = 12f; l.shadows = LightShadows.None;

        // Trigger
        var trigger = new GameObject("Trigger");
        trigger.transform.SetParent(portal.transform, false);
        trigger.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        var sc = trigger.AddComponent<SphereCollider>();
        sc.isTrigger = true; sc.radius = 3f;

        portal.AddComponent<Mirror.NetworkIdentity>();
        var hrt = portal.AddComponent<HubReturnTrigger>();
        hrt.hubSceneName = SceneNames.Hub;
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Camera
    // ════════════════════════════════════════════════════════════════════════════

    static void BuildCamera()
    {
        if (Object.FindAnyObjectByType<Camera>() != null) return;
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetPositionAndRotation(new Vector3(0f, 8f, -12f), Quaternion.Euler(28f, 0f, 0f));
        var cam = camGO.AddComponent<Camera>();
        cam.backgroundColor = ColDeathFog; // seen only if skybox off
        camGO.AddComponent<AudioListener>();
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Helpers
    // ════════════════════════════════════════════════════════════════════════════

    static Material BuildMaterial(string name, Color albedo, Color emissiveColor = default)
    {
        var mat  = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = name;
        mat.color = albedo;
        if (emissiveColor != default)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissiveColor);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        return mat;
    }

    static GameObject GetOrCreate(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null) return existing;
        return new GameObject(name);
    }
}
#endif
