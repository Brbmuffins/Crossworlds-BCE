#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ArenaSceneBuilder — BCE/Setup/7
/// Creates a complete arena scene ready for combat:
///   - Ground plane (100×100) with NavMeshSurface
///   - Boundary walls (invisible, stop players escaping)
///   - 4 cardinal enemy spawn points
///   - 4 player spawn points (NetworkStartPosition)
///   - WaveSpawner wired with enemy prefabs from Assets/Game/Prefabs/
///   - Return portal (back to Hub)
///   - Directional light + ambient purple fog
///   - RodChatManager scene object
///
/// Run BCE → Setup → 7 ▶ Create Arena Scene
/// Then: Window → AI → Navigation → Bake NavMesh, Ctrl+S
/// Add "Arena_Copper" to Build Settings (index 3+)
/// </summary>
public static class ArenaSceneBuilder
{
    const string PrefabDir = "Assets/Game/Prefabs";

    [MenuItem("BCE/Setup/7 ▶ Create Arena Scene (Arena_Copper)")]
    public static void CreateArenaScene()
    {
        // Warn if Hub is open — we don't want to accidentally wipe it
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name == "Hub" || activeScene.name == "Hub.unity")
        {
            bool ok = EditorUtility.DisplayDialog("Create Arena Scene",
                "This will create a NEW scene called Arena_Copper.\n" +
                "Your Hub scene will remain open.\n\nContinue?", "Yes", "Cancel");
            if (!ok) return;
        }

        // Unity refuses NewSceneMode.Additive when the active scene is untitled/unsaved.
        // Prompt the user to save or discard it first.
        if (string.IsNullOrEmpty(activeScene.path))
        {
            bool saved = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (!saved)
            {
                // User chose "Don't Save" — discard the untitled scene by opening a temp blank one
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        // Create and set new scene
        var arenaScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SetActiveScene(arenaScene);

        // ── Lighting ──────────────────────────────────────────────────────────
        var sunGO = new GameObject("Sun");
        sunGO.transform.rotation = Quaternion.Euler(40f, -60f, 0f);
        var sun = sunGO.AddComponent<Light>();
        sun.type      = LightType.Directional;
        sun.color     = new Color(0.7f, 0.5f, 0.9f); // slightly purple battle light
        sun.intensity = 1.1f;
        sun.shadows   = LightShadows.Soft;

        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.04f, 0.12f);
        RenderSettings.fog          = true;
        RenderSettings.fogColor     = new Color(0.05f, 0.02f, 0.1f);
        RenderSettings.fogMode      = FogMode.Linear;
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance   = 80f;

        // ── Ground ────────────────────────────────────────────────────────────
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "ArenaGround";
        ground.tag  = "Ground";
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        groundMat.color = new Color(0.12f, 0.08f, 0.18f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;
        Object.DestroyImmediate(ground.GetComponent<MeshCollider>());
        var gbc = ground.AddComponent<BoxCollider>();
        gbc.size = new Vector3(1f, 0.02f, 1f);

        // NavMeshSurface — bake NavMesh after running this script
        if (ground.GetComponent("NavMeshSurface") == null)
        {
            var surfaceType = System.Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (surfaceType != null)
                ground.AddComponent(surfaceType);
            else
                Debug.LogWarning("[Arena] NavMeshSurface type not found — ensure AI Navigation package is installed. Add manually.");
        }

        // ── Invisible boundary walls (keep players + enemies inside) ─────────
        float half = 50f;
        var walls = new (Vector3 pos, Vector3 size)[]
        {
            (new Vector3( half, 3f, 0f),   new Vector3(1f, 6f, 102f)),
            (new Vector3(-half, 3f, 0f),   new Vector3(1f, 6f, 102f)),
            (new Vector3(0f,  3f,  half),  new Vector3(102f, 6f, 1f)),
            (new Vector3(0f,  3f, -half),  new Vector3(102f, 6f, 1f)),
        };
        foreach (var (pos, size) in walls)
        {
            var wall = new GameObject("BoundaryWall");
            wall.transform.position = pos;
            var bc = wall.AddComponent<BoxCollider>();
            bc.size = size;
            // No renderer — invisible wall
        }

        // ── Atmospheric pillars / rubble decoration ───────────────────────────
        PlaceDecorations();

        // ── Player spawn points (NetworkStartPosition) ────────────────────────
        for (int i = 0; i < 4; i++)
        {
            float a  = (90f * i) * Mathf.Deg2Rad;
            var   sp = new GameObject($"PlayerSpawn_{i}");
            sp.transform.position = new Vector3(Mathf.Sin(a) * 5f, 0.1f, Mathf.Cos(a) * 5f);
            sp.AddComponent<Mirror.NetworkStartPosition>();
        }

        // ── Enemy spawn points (used by WaveSpawner) ──────────────────────────
        var spawnRoot = new GameObject("EnemySpawnPoints");
        var spawnPts  = new List<Transform>();
        var cardinals = new Vector3[]
        {
            new Vector3(  0f, 0f,  20f),
            new Vector3(  0f, 0f, -20f),
            new Vector3( 20f, 0f,   0f),
            new Vector3(-20f, 0f,   0f),
        };
        foreach (var offset in cardinals)
        {
            var sp = new GameObject($"EnemySpawn_{offset.x}_{offset.z}");
            sp.transform.SetParent(spawnRoot.transform, false);
            sp.transform.localPosition = offset;
            spawnPts.Add(sp.transform);

            // Visual marker (editor only gizmo-sphere via empty GO name convention)
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "SpawnMarker";
            marker.transform.SetParent(sp.transform, false);
            marker.transform.localScale = Vector3.one * 0.4f;
            marker.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                { color = new Color(1f, 0.2f, 0.2f) };
            Object.DestroyImmediate(marker.GetComponent<SphereCollider>());
        }

        // ── ArenaSessionController ────────────────────────────────────────────
        var sessionGO = new GameObject("ArenaSessionController");
        sessionGO.AddComponent<Mirror.NetworkIdentity>();
        sessionGO.AddComponent<ArenaSessionController>();

        // ── WaveSpawner ────────────────────────────────────────────────────────
        var wsGO = new GameObject("WaveSpawner");
        wsGO.AddComponent<Mirror.NetworkIdentity>();
        var spawner = wsGO.AddComponent<WaveSpawner>();
        spawner.spawnPoints            = spawnPts;
        spawner.baseEnemiesPerWave     = 4;
        spawner.enemiesAddedPerWave    = 2;
        spawner.timeBetweenWaves       = 8f;
        spawner.eliteEveryNWaves       = 3;
        spawner.introDelay             = 4f;

        // Auto-assign enemy prefabs if they exist
        var gruntPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Grunt.prefab");
        var rangedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Ranged.prefab");
        var elitePrefab  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Elite.prefab");

        if (gruntPrefab  != null) spawner.enemyPrefabs.Add(gruntPrefab);
        if (rangedPrefab != null) spawner.enemyPrefabs.Add(rangedPrefab);
        if (elitePrefab  != null) spawner.elitePrefab = elitePrefab;

        if (gruntPrefab == null || rangedPrefab == null)
            Debug.LogWarning("[Arena] Enemy prefabs not found — run BCE/Setup/4a–4c first, then re-run this.");

        // ArenaAutoStarter: calls WaveSpawner.StartWaves() server-side on scene load.
        // This is the bridge between scene activation and wave begin.
        var starterGO = new GameObject("ArenaAutoStarter");
        starterGO.AddComponent<Mirror.NetworkIdentity>();
        starterGO.AddComponent<ArenaAutoStarter>();

        // ── Return portal (back to Hub) ────────────────────────────────────────
        var returnPortal = new GameObject("ReturnPortal_Hub");
        returnPortal.transform.position = new Vector3(0f, 0f, -30f);
        returnPortal.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        var returnArch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        returnArch.name = "Arch";
        returnArch.transform.SetParent(returnPortal.transform, false);
        returnArch.transform.localPosition = new Vector3(0f, 2f, 0f);
        returnArch.transform.localScale    = new Vector3(0.25f, 2f, 0.25f);
        Object.DestroyImmediate(returnArch.GetComponent<CapsuleCollider>());
        returnArch.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            { color = new Color(0.2f, 0.8f, 0.4f) };

        var returnTrigger = new GameObject("Trigger");
        returnTrigger.transform.SetParent(returnPortal.transform, false);
        returnTrigger.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        var rtc = returnTrigger.AddComponent<SphereCollider>();
        rtc.isTrigger = true; rtc.radius = 3f;

        var returnLight = new GameObject("ReturnLight");
        returnLight.transform.SetParent(returnPortal.transform, false);
        returnLight.transform.localPosition = new Vector3(0f, 2f, 0f);
        var rl = returnLight.AddComponent<Light>();
        rl.type = LightType.Point; rl.color = new Color(0.2f, 0.9f, 0.4f); rl.intensity = 3f; rl.range = 10f;

        returnPortal.AddComponent<Mirror.NetworkIdentity>();
        var hrt = returnPortal.AddComponent<HubReturnTrigger>();
        hrt.hubSceneName = SceneNames.Hub;

        // ── RodChatManager ────────────────────────────────────────────────────
        if (Object.FindAnyObjectByType<RodChatManager>() == null)
        {
            var chatGO = new GameObject("RodChatManager");
            chatGO.AddComponent<Mirror.NetworkIdentity>();
            chatGO.AddComponent<RodChatManager>();
        }

        // ── Camera ────────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetPositionAndRotation(new Vector3(0f, 6f, -10f), Quaternion.Euler(25f, 0f, 0f));
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();

        // ── Save ──────────────────────────────────────────────────────────────
        string savePath = SceneNames.ArenaCopperPath;
        EditorSceneManager.SaveScene(arenaScene, savePath);

        // ── Register in Build Settings ────────────────────────────────────────
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool alreadyIn = scenes.Exists(s => s.path == savePath);
        if (!alreadyIn)
        {
            scenes.Add(new EditorBuildSettingsScene(savePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[BCE] Arena_Copper added to Build Settings.");
        }

        Debug.Log("[BCE] Arena_Copper scene created at " + savePath + "\n" +
                  "NEXT:\n" +
                  "1. Window → AI → Navigation → Bake NavMesh on ArenaGround\n" +
                  "2. NetworkManager (LoginScene) → spawnPrefabs: add Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem\n" +
                  "3. Ctrl+S to save scene\n" +
                  "4. Play-mode checklist:\n" +
                  "   a. Hub portal → arena loads\n" +
                  "   b. Wave 1 enemies spawn after introDelay (4 s)\n" +
                  "   c. Kill all enemies → wave 2 begins\n" +
                  "   d. Return portal → player back in Hub");

        Selection.activeGameObject = wsGO;
    }

    static void PlaceDecorations()
    {
        // 4 corner rubble piles from primitives (replace with real assets later)
        var corners = new Vector3[]
        {
            new Vector3( 35f, 0f,  35f), new Vector3(-35f, 0f,  35f),
            new Vector3( 35f, 0f, -35f), new Vector3(-35f, 0f, -35f),
        };

        foreach (var c in corners)
        {
            var pile = new GameObject("RubblePile");
            pile.transform.position = c;

            for (int i = 0; i < 4; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = "Rock";
                rock.transform.SetParent(pile.transform, false);
                rock.transform.localPosition = new Vector3(
                    Random.Range(-1.5f, 1.5f), Random.Range(0f, 0.8f), Random.Range(-1.5f, 1.5f));
                rock.transform.localRotation = Random.rotation;
                float s = Random.Range(0.4f, 1.2f);
                rock.transform.localScale = new Vector3(s, s * 0.6f, s);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.15f, 0.25f);
                rock.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(rock.GetComponent<BoxCollider>());
            }
        }

        // 4 torches at mid-radius
        var torchPositions = new Vector3[]
        {
            new Vector3( 15f, 0f, 0f), new Vector3(-15f, 0f,  0f),
            new Vector3(  0f, 0f, 15f), new Vector3(  0f, 0f, -15f),
        };

        foreach (var tp in torchPositions)
        {
            var torch = new GameObject("Torch");
            torch.transform.position = tp;

            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Pole";
            pole.transform.SetParent(torch.transform, false);
            pole.transform.localPosition = new Vector3(0f, 1f, 0f);
            pole.transform.localScale    = new Vector3(0.08f, 1f, 0.08f);
            var poleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            poleMat.color = new Color(0.2f, 0.12f, 0.05f);
            pole.GetComponent<Renderer>().sharedMaterial = poleMat;
            Object.DestroyImmediate(pole.GetComponent<CapsuleCollider>());

            var flame = new GameObject("FlameLight");
            flame.transform.SetParent(torch.transform, false);
            flame.transform.localPosition = new Vector3(0f, 2.1f, 0f);
            var fl = flame.AddComponent<Light>();
            fl.type      = LightType.Point;
            fl.color     = new Color(1f, 0.5f, 0.1f);
            fl.intensity = 2f;
            fl.range     = 8f;
        }
    }
}
#endif
