#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// HubWorldBuilder — BCE/Hub World/Make Hub a Combat World
///
/// Run with Hub.unity open. Adds combat infrastructure directly to the Hub
/// so it functions as a persistent open world — no separate Arena scene needed:
///
///   • WaveSpawner — auto-wired to Enemy_Grunt / Ranged / Elite prefabs
///   • 8 enemy spawn points around the perimeter (radius 30)
///   • NavMeshSurface on the ground (bake afterward)
///   • CombatAudio object (assign Retro Sci-Fi clips in Inspector)
///   • CombatZoneTrigger — a central disc that starts waves when a player enters
///   • ArenaSessionController — tracks session XP / kills
///
/// After running:
///   1. Window → AI → Navigation → Bake NavMesh
///   2. On CombatAudio: assign clips from Assets/Retro Sci-Fi Pack/Uncompressed/
///   3. On WaveSpawner: confirm enemy prefabs are set (auto-assigned)
///   4. NetworkManager.spawnPrefabs: Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem
///   5. Ctrl+S
/// </summary>
public static class HubWorldBuilder
{
    const string PrefabDir = "Assets/Game/Prefabs";

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

        // ── 1. NavMeshSurface on the ground plane ─────────────────────────────
        var groundGO = GameObject.Find("Ground");
        if (groundGO == null)
        {
            // Try any Plane tagged Ground
            foreach (var obj in Object.FindObjectsByType<GameObject>(
                FindObjectsInactive.Exclude))
            {
                if (obj.CompareTag("Ground")) { groundGO = obj; break; }
            }
        }

        if (groundGO != null)
        {
            var surfaceType = System.Type.GetType(
                "Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (surfaceType != null)
            {
                if (groundGO.GetComponent(surfaceType) == null)
                {
                    groundGO.AddComponent(surfaceType);
                    Debug.Log("[HubWorld] NavMeshSurface added to Ground. Bake it: Window → AI → Navigation → Bake.");
                }
            }
            else
            {
                Debug.LogWarning("[HubWorld] NavMeshSurface not found — install AI Navigation package, then add manually.");
            }
        }
        else
        {
            Debug.LogWarning("[HubWorld] No 'Ground' GameObject found — add NavMeshSurface manually.");
        }

        // ── 2. Enemy spawn points (8, perimeter ring radius 30) ──────────────
        var spawnRoot = new GameObject("CombatSpawnPoints");
        var spawnPts  = new List<Transform>();
        for (int i = 0; i < 8; i++)
        {
            float angle = (360f / 8f * i) * Mathf.Deg2Rad;
            var sp = new GameObject($"EnemySpawn_{i}");
            sp.transform.SetParent(spawnRoot.transform, false);
            sp.transform.localPosition = new Vector3(
                Mathf.Sin(angle) * 30f, 0.1f, Mathf.Cos(angle) * 30f);
            spawnPts.Add(sp.transform);

            // Small red visual marker (editor only — no renderer in builds matters little)
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "SpawnMarker";
            marker.transform.SetParent(sp.transform, false);
            marker.transform.localScale = Vector3.one * 0.5f;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.15f, 0.1f);
            marker.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(marker.GetComponent<SphereCollider>());
        }

        // ── 3. WaveSpawner ────────────────────────────────────────────────────
        var wsGO = new GameObject("WaveSpawner");
        wsGO.AddComponent<Mirror.NetworkIdentity>();
        var spawner = wsGO.AddComponent<WaveSpawner>();

        spawner.spawnPoints         = spawnPts;
        spawner.baseEnemiesPerWave  = 4;
        spawner.enemiesAddedPerWave = 2;
        spawner.timeBetweenWaves    = 10f;
        spawner.eliteEveryNWaves    = 3;
        spawner.introDelay          = 5f;
        spawner.maxWaves            = 0;       // 0 = infinite in the open world

        // Auto-assign enemy prefabs
        var gruntPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Grunt.prefab");
        var rangedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Ranged.prefab");
        var elitePrefab  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Elite.prefab");

        if (gruntPrefab  != null) spawner.enemyPrefabs.Add(gruntPrefab);
        if (rangedPrefab != null) spawner.enemyPrefabs.Add(rangedPrefab);
        if (elitePrefab  != null) spawner.elitePrefab = elitePrefab;

        if (gruntPrefab == null)
            Debug.LogWarning("[HubWorld] Enemy prefabs not found — run BCE/Setup/4a–4c first.");

        // ── 4. Combat Zone trigger (central disc, starts waves on player enter) ─
        var zoneGO = new GameObject("CombatZone_Trigger");
        zoneGO.transform.position = new Vector3(0f, 0.05f, 0f);

        var zoneTrigger = zoneGO.AddComponent<CapsuleCollider>();
        zoneTrigger.isTrigger = true;
        zoneTrigger.height    = 1f;
        zoneTrigger.radius    = 20f;   // 40-unit diameter combat zone
        zoneTrigger.center    = Vector3.zero;

        zoneGO.AddComponent<CombatZoneStarter>();

        // Visual: flat transparent disc
        var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "ZoneDisc";
        disc.transform.SetParent(zoneGO.transform, false);
        disc.transform.localScale = new Vector3(40f, 0.01f, 40f);
        Object.DestroyImmediate(disc.GetComponent<CapsuleCollider>());
        var discMat = new Material(Shader.Find("Sprites/Default"));
        discMat.color = new Color(0.8f, 0.1f, 0.1f, 0.12f);
        disc.GetComponent<Renderer>().sharedMaterial = discMat;

        // ── 5. CombatAudio (wrapped in #if !UNITY_SERVER — use reflection) ──────
        var caType = System.Type.GetType("CombatAudio");
        if (caType != null && Object.FindAnyObjectByType(caType) == null)
        {
            var audioGO = new GameObject("CombatAudio");
            audioGO.AddComponent(caType);
            Debug.Log("[HubWorld] CombatAudio added — assign Retro Sci-Fi clips in Inspector.");
        }
        else if (caType == null)
        {
            Debug.LogWarning("[HubWorld] CombatAudio type not found (may be excluded by UNITY_SERVER define). Add manually after switching build target to Standalone.");
        }

        // ── 6. ArenaSessionController ─────────────────────────────────────────
        if (Object.FindAnyObjectByType<ArenaSessionController>() == null)
        {
            var sessionGO = new GameObject("ArenaSessionController");
            sessionGO.AddComponent<Mirror.NetworkIdentity>();
            sessionGO.AddComponent<ArenaSessionController>();
        }

        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log(
            "[BCE] ✅ Hub upgraded to Combat World.\n" +
            "NEXT:\n" +
            "1. Window → AI → Navigation → Bake NavMesh\n" +
            "2. CombatAudio Inspector → assign clips from Assets/Retro Sci-Fi Pack/\n" +
            "3. NetworkManager.spawnPrefabs: add Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem\n" +
            "4. Ctrl+S");

        Selection.activeGameObject = wsGO;

        EditorUtility.DisplayDialog("✅ Hub → Combat World",
            "Combat infrastructure added to Hub:\n\n" +
            "  • WaveSpawner (infinite waves, auto-assigned prefabs)\n" +
            "  • 8 enemy spawn points (perimeter, radius 30)\n" +
            "  • CombatZone trigger disc (radius 20 — enter to start)\n" +
            "  • CombatAudio object\n" +
            "  • ArenaSessionController\n" +
            "  • NavMeshSurface on Ground\n\n" +
            "NEXT: Bake NavMesh → assign CombatAudio clips → Ctrl+S",
            "Let's go!");
    }
}
#endif
