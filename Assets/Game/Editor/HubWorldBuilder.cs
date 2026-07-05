#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// HubWorldBuilder — BCE/Hub World/Make Hub a Combat World
///
/// Run with Hub.unity open. Adds combat infrastructure directly to the Hub
/// so it functions as a persistent open world — no separate Arena scene needed.
///
/// What gets added:
///   • WaveSpawner (infinite, auto-wired to Enemy_Grunt / Ranged / Elite)
///   • 16 enemy spawn points spread across the world (4 rings at different radii)
///   • Multiple named CombatZone triggers matching the Hub's geography
///   • CombatAudio object
///   • ArenaSessionController
///   • NavMeshSurface on the ground plane
///   • Auto-assigns worldPrefabs on RodNetworkManager (no more manual step)
///
/// After running:
///   1. Drag spawn points in the scene to match Hangdanger's zone geometry
///   2. Window → AI → Navigation → Bake NavMesh
///   3. CombatAudio Inspector → assign clips from Assets/Retro Sci-Fi Pack/
///   4. Ctrl+S
/// </summary>
public static class HubWorldBuilder
{
    const string PrefabDir = "Assets/Game/Prefabs";

    // Named combat zones: (name, world position, trigger radius)
    // These are starting positions — move them in the scene after running to
    // match Hangdanger's actual zone layout.
    static readonly (string name, Vector3 pos, float radius)[] CombatZones =
    {
        ("CombatZone_FieldOfGundab",      new Vector3(  0f, 0.05f,  60f), 35f),
        ("CombatZone_CorruptedGrove",     new Vector3( 60f, 0.05f,   0f), 25f),
        ("CombatZone_AshPlains",          new Vector3(-60f, 0.05f,   0f), 25f),
        ("CombatZone_BossArena",          new Vector3(  0f, 0.05f,-60f), 30f),
    };

    [MenuItem("BCE/Hub World/Make Hub a Combat World")]
    public static void UpgradeHub()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.StartsWith("Hub"))
        {
            bool go = EditorUtility.DisplayDialog(
                "Hub World Upgrade",
                $"Active scene is '{scene.name}', not Hub.\n\nContinue anyway?",
                "Yes", "Cancel");
            if (!go) return;
        }

        // ── Guard: avoid duplicates ───────────────────────────────────────────
        if (Object.FindAnyObjectByType<WaveSpawner>() != null)
        {
            EditorUtility.DisplayDialog("Already upgraded",
                "A WaveSpawner already exists in this scene.\nRemove it first if you want to re-run.",
                "OK");
            return;
        }

        // ── Load prefabs ──────────────────────────────────────────────────────
        var gruntPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Grunt.prefab");
        var rangedPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Ranged.prefab");
        var elitePrefab   = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Elite.prefab");
        var worldItemPref = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/WorldItem.prefab");
        var wispPrefab    = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Wisp_Mob.prefab");
        var wraithPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Wraith.prefab");

        if (gruntPrefab == null)
            Debug.LogWarning("[HubWorld] Enemy_Grunt.prefab not found — run BCE/Setup/4a–4c first.");

        // ── 1. NavMeshSurface on the ground plane ─────────────────────────────
        var groundGO = GameObject.Find("Ground");
        if (groundGO == null)
            foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
                if (obj.CompareTag("Ground")) { groundGO = obj; break; }

        if (groundGO != null)
        {
            var surfaceType = System.Type.GetType(
                "Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (surfaceType != null && groundGO.GetComponent(surfaceType) == null)
            {
                groundGO.AddComponent(surfaceType);
                Debug.Log("[HubWorld] NavMeshSurface added to Ground. Bake: Window → AI → Navigation → Bake.");
            }
            else if (surfaceType == null)
                Debug.LogWarning("[HubWorld] NavMeshSurface not found — install AI Navigation package.");
        }
        else
            Debug.LogWarning("[HubWorld] No 'Ground' GameObject found — add NavMeshSurface manually.");

        // ── 2. Spawn points — 4 rings matching the zone layout ───────────────
        var spawnRoot = new GameObject("CombatSpawnPoints");
        var spawnPts  = new List<Transform>();

        // 4 points per named zone, placed around each zone's position
        foreach (var zone in CombatZones)
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = (360f / 4f * i) * Mathf.Deg2Rad;
                float r     = zone.radius * 0.7f; // inside the zone boundary
                var sp = new GameObject($"EnemySpawn_{zone.name}_{i}");
                sp.transform.SetParent(spawnRoot.transform, false);
                sp.transform.position = zone.pos + new Vector3(
                    Mathf.Sin(angle) * r, 0.1f, Mathf.Cos(angle) * r);
                spawnPts.Add(sp.transform);

                // Red sphere visual marker
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "SpawnMarker";
                marker.transform.SetParent(sp.transform, false);
                marker.transform.localScale = Vector3.one * 0.5f;
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (mat.shader.name == "Hidden/InternalErrorShader")
                    mat = new Material(Shader.Find("Standard")); // URP fallback
                mat.color = new Color(1f, 0.15f, 0.1f);
                marker.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(marker.GetComponent<SphereCollider>());
            }
        }

        // ── 3. WaveSpawner ────────────────────────────────────────────────────
        var wsGO = new GameObject("WaveSpawner");
        wsGO.AddComponent<Mirror.NetworkIdentity>();
        var spawner = wsGO.AddComponent<WaveSpawner>();

        spawner.spawnPoints         = spawnPts;
        spawner.baseEnemiesPerWave  = 5;
        spawner.enemiesAddedPerWave = 2;
        spawner.timeBetweenWaves    = 12f;
        spawner.eliteEveryNWaves    = 3;
        spawner.introDelay          = 3f;
        spawner.maxWaves            = 0;   // 0 = infinite open-world waves

        if (gruntPrefab  != null) spawner.enemyPrefabs.Add(gruntPrefab);
        if (rangedPrefab != null) spawner.enemyPrefabs.Add(rangedPrefab);
        if (elitePrefab  != null) spawner.elitePrefab = elitePrefab;

        // ── 4. Named CombatZone triggers ──────────────────────────────────────
        // Each zone has its own CombatZoneStarter — first player to enter any
        // zone starts the global WaveSpawner.
        var zonesRoot = new GameObject("CombatZones");
        foreach (var zone in CombatZones)
        {
            var zGO = new GameObject(zone.name);
            zGO.transform.SetParent(zonesRoot.transform, false);
            zGO.transform.position = zone.pos;

            var col = zGO.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.height    = 1f;
            col.radius    = zone.radius;
            col.center    = Vector3.zero;
            zGO.AddComponent<CombatZoneStarter>();

            // Transparent disc so you can see zone boundaries in the scene
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "ZoneDisc";
            disc.transform.SetParent(zGO.transform, false);
            disc.transform.localScale = new Vector3(zone.radius * 2f, 0.01f, zone.radius * 2f);
            Object.DestroyImmediate(disc.GetComponent<CapsuleCollider>());
            var discMat = new Material(Shader.Find("Sprites/Default"));
            discMat.color = new Color(0.8f, 0.1f, 0.1f, 0.10f);
            disc.GetComponent<Renderer>().sharedMaterial = discMat;
        }

        // ── 5. CombatAudio ────────────────────────────────────────────────────
        var caType = System.Type.GetType("CombatAudio");
        if (caType != null && Object.FindAnyObjectByType(caType) == null)
        {
            var audioGO = new GameObject("CombatAudio");
            audioGO.AddComponent(caType);
            Debug.Log("[HubWorld] CombatAudio added — assign clips in Inspector.");
        }

        // ── 6. ArenaSessionController ─────────────────────────────────────────
        if (Object.FindAnyObjectByType<ArenaSessionController>() == null)
        {
            var sessionGO = new GameObject("ArenaSessionController");
            sessionGO.AddComponent<Mirror.NetworkIdentity>();
            sessionGO.AddComponent<ArenaSessionController>();
        }

        // ── 7. Auto-assign worldPrefabs on RodNetworkManager ─────────────────
        var nm = Object.FindAnyObjectByType<RodNetworkManager>();
        if (nm != null)
        {
            var worldPrefabList = new List<GameObject>();
            if (gruntPrefab   != null) worldPrefabList.Add(gruntPrefab);
            if (rangedPrefab  != null) worldPrefabList.Add(rangedPrefab);
            if (elitePrefab   != null) worldPrefabList.Add(elitePrefab);
            if (worldItemPref != null) worldPrefabList.Add(worldItemPref);
            if (wispPrefab    != null) worldPrefabList.Add(wispPrefab);
            if (wraithPrefab  != null) worldPrefabList.Add(wraithPrefab);

            var so   = new SerializedObject(nm);
            var prop = so.FindProperty("worldPrefabs");
            if (prop != null)
            {
                prop.arraySize = worldPrefabList.Count;
                for (int i = 0; i < worldPrefabList.Count; i++)
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = worldPrefabList[i];
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(nm);
                Debug.Log($"[HubWorld] worldPrefabs assigned on RodNetworkManager ({worldPrefabList.Count} prefabs).");
            }
        }
        else
        {
            Debug.LogWarning("[HubWorld] RodNetworkManager not found in scene — assign worldPrefabs manually.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = wsGO;

        EditorUtility.DisplayDialog("✅ Hub → Combat World",
            "Combat infrastructure added:\n\n" +
            "  • WaveSpawner (infinite waves)\n" +
            "  • 16 spawn points across 4 zones\n" +
            "  • 4 named CombatZone triggers\n" +
            "  • ArenaSessionController\n" +
            "  • CombatAudio\n" +
            "  • NavMeshSurface on Ground\n" +
            "  • worldPrefabs wired on NetworkManager\n\n" +
            "NEXT:\n" +
            "1. Drag zone discs to match Hangdanger's actual zones\n" +
            "2. Bake NavMesh: Window → AI → Navigation → Bake\n" +
            "3. CombatAudio → assign clips\n" +
            "4. Ctrl+S",
            "Let's go!");
    }
}
#endif
