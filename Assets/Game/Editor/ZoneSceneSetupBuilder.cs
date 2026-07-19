#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Mirror;

/// <summary>
/// BCE/Setup/8 — Zone Combat Setup
///
/// Injects the full combat backend into an existing zone scene so enemies
/// can path + wave-spawn. The scene already has terrain/geometry; this adds:
///
///   • NavMeshSurface on the first terrain or large mesh it finds
///     (you still have to open Window → AI → Navigation and click Bake)
///   • WaveSpawner with 4 cardinal spawn points wired in
///   • CombatZoneStarter trigger (player entering starts waves)
///   • 4 NetworkStartPosition objects for player spawn
///   • EnemyHeavyAttack on all EnemyController objects in the scene
///
/// Run on each zone scene with it open (additive or single):
///   BCE → Setup → 8 ▶ Zone Combat Setup (active scene)
///
/// After running:
///   1. Window → AI → Navigation → Bake
///   2. Ctrl+S (scene save so sceneIds are baked)
///   3. Add scene to Build Settings (index after CharacterSelect)
///   4. Rebuild + redeploy server
///
/// Repeat per zone: VoidDungeon, Toujam Basin, Ashen Wastelands, GM Island
/// </summary>
public static class ZoneSceneSetupBuilder
{
    const string PrefabDir      = "Assets/Game/Prefabs";
    const string GruntPrefab    = "Enemy_Grunt";
    const string RangedPrefab   = "Enemy_Ranged";
    const string ElitePrefab    = "Enemy_Elite";
    const string WispPrefabPath = "Assets/Game/Game_Prefabs/Muffin Junk/Wisp_Mob.prefab";

    [MenuItem("BCE/Setup/8 ▶ Zone Combat Setup (active scene)", priority = 48)]
    static void SetupZoneScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.isLoaded || string.IsNullOrEmpty(scene.name))
        {
            EditorUtility.DisplayDialog("Zone Combat Setup",
                "No active scene loaded. Open the scene you want to set up first.", "OK");
            return;
        }

        var report = new StringBuilder();
        report.AppendLine($"Zone: {scene.name}");
        report.AppendLine("─────────────────────────────");

        // Determine scene center from existing terrain or geometry
        Vector3 center = FindSceneCenter();

        // ── 1. NavMesh Surface ────────────────────────────────────────────────────
        int navAdded = EnsureNavMeshSurfaces(report);

        // ── 2. Combat Root parent ─────────────────────────────────────────────────
        var existing = GameObject.Find("CombatSystem");
        if (existing != null)
        {
            bool replace = EditorUtility.DisplayDialog("Zone Combat Setup",
                $"Scene '{scene.name}' already has a CombatSystem object.\nReplace it?",
                "Replace", "Keep & Update");
            if (replace)
            {
                Object.DestroyImmediate(existing);
                report.AppendLine("  ✗ Removed old CombatSystem.");
            }
        }

        var root = new GameObject("CombatSystem");

        // ── 3. Spawn points ───────────────────────────────────────────────────────
        var spawnParent = new GameObject("SpawnPoints");
        spawnParent.transform.SetParent(root.transform);

        float spread = 12f;
        Vector3[] cardinals = {
            center + new Vector3(0f,  0.2f,  spread),
            center + new Vector3(0f,  0.2f, -spread),
            center + new Vector3( spread, 0.2f, 0f),
            center + new Vector3(-spread, 0.2f, 0f),
        };

        var spawnTransforms = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            var sp = new GameObject($"SpawnPoint_{i + 1}");
            sp.transform.SetParent(spawnParent.transform);
            sp.transform.position = cardinals[i];
            spawnTransforms[i] = sp.transform;
        }
        report.AppendLine($"  ✓ {cardinals.Length} spawn points at r={spread}m around {center}");

        // ── 4. WaveSpawner ────────────────────────────────────────────────────────
        var wsGO = new GameObject("WaveSpawner");
        wsGO.transform.SetParent(root.transform);
        wsGO.transform.position = center;

        wsGO.AddComponent<NetworkIdentity>();
        var ws = wsGO.AddComponent<WaveSpawner>();

        // Wire spawn points
        ws.spawnPoints.AddRange(spawnTransforms);

        // Wire prefabs from asset database (optional — user can assign in Inspector)
        var grunt  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{GruntPrefab}.prefab");
        var ranged = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{RangedPrefab}.prefab");
        var elite  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/{ElitePrefab}.prefab");

        if (grunt  != null) ws.enemyPrefabs.Add(grunt);
        if (ranged != null) ws.enemyPrefabs.Add(ranged);
        if (elite  != null) ws.elitePrefab = elite;

        var wisp = AssetDatabase.LoadAssetAtPath<GameObject>(WispPrefabPath);
        if (wisp  != null) { ws.wispPrefab = wisp; ws.wispEveryNWaves = 2; ws.wispCountPerSwarm = 3; }

        int wired = (grunt != null ? 1 : 0) + (ranged != null ? 1 : 0) + (elite != null ? 1 : 0);
        report.AppendLine(wired > 0
            ? $"  ✓ WaveSpawner wired: {wired}/3 ground enemies + wisp={wisp != null}"
            : $"  ⚠ WaveSpawner: no enemy prefabs found in {PrefabDir} — assign manually");

        // Wave tuning — escalates per zone difficulty (tweak in Inspector)
        ws.baseEnemiesPerWave  = 4;
        ws.enemiesAddedPerWave = 2;
        ws.eliteEveryNWaves    = 3;
        ws.maxWaves            = 0;  // infinite — zone is persistent combat area
        ws.timeBetweenWaves    = 10f;
        ws.introDelay          = 5f;

        // ── 5. Combat Zone Trigger ────────────────────────────────────────────────
        var triggerGO = new GameObject("CombatZoneTrigger");
        triggerGO.transform.SetParent(root.transform);
        triggerGO.transform.position = center;
        triggerGO.tag = "Untagged";

        var sphere = triggerGO.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius    = spread * 1.8f;  // wider than spawn radius

        var starter = triggerGO.AddComponent<CombatZoneStarter>();
        starter.autoStart = true;
        report.AppendLine($"  ✓ CombatZoneTrigger (r={sphere.radius:F0}m)");

        // ── 6. Network start positions (player spawn) ─────────────────────────────
        var nspParent = new GameObject("PlayerSpawnPoints");
        nspParent.transform.SetParent(root.transform);

        Vector3[] playerSpawns = {
            center + new Vector3( 2f, 0.2f,  2f),
            center + new Vector3(-2f, 0.2f,  2f),
            center + new Vector3( 2f, 0.2f, -2f),
            center + new Vector3(-2f, 0.2f, -2f),
        };

        foreach (var ps in playerSpawns)
        {
            var nsp = new GameObject("NetworkStartPosition");
            nsp.transform.SetParent(nspParent.transform);
            nsp.transform.position = ps;
            nsp.AddComponent<NetworkStartPosition>();
        }
        report.AppendLine($"  ✓ {playerSpawns.Length} NetworkStartPositions");

        // ── 7. Add EnemyHeavyAttack to any existing EnemyController in scene ────
        int heavyAdded = PatchExistingEnemies(report);

        // ── 8. NavMesh reminder ───────────────────────────────────────────────────
        report.AppendLine();
        report.AppendLine("REQUIRED EDITOR STEPS:");
        report.AppendLine("  1. Window → AI → Navigation → Agents tab → confirm settings");
        report.AppendLine("  2. Bake tab → click Bake (bakes NavMesh for this scene)");
        report.AppendLine("  3. Ctrl+S (Mirror bakes sceneIds on save)");
        report.AppendLine($"  4. Add '{scene.name}' to Build Settings if not present");
        report.AppendLine("  5. Rebuild + redeploy server");
        if (wired < 3)
            report.AppendLine("  6. Assign enemy prefabs to WaveSpawner in Inspector");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Zone Combat Setup — " + scene.name,
            report.ToString(), "Done");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    static Vector3 FindSceneCenter()
    {
        // Try to find a Terrain first
        var terrain = Object.FindAnyObjectByType<Terrain>();
        if (terrain != null)
        {
            var td = terrain.terrainData;
            return terrain.transform.position + new Vector3(
                td.size.x * 0.5f, 0f, td.size.z * 0.5f);
        }

        // Fall back to the bounding center of all renderers
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return new Vector3(b.center.x, 0f, b.center.z);
        }

        return Vector3.zero;
    }

    static int EnsureNavMeshSurfaces(StringBuilder report)
    {
        int added = 0;

        // Add NavMeshSurface to every Terrain that doesn't already have one
        foreach (var terrain in Object.FindObjectsByType<Terrain>(FindObjectsInactive.Exclude))
        {
            if (terrain.GetComponent<NavMeshSurface>() != null) continue;
            var surf = terrain.gameObject.AddComponent<NavMeshSurface>();
            surf.collectObjects = CollectObjects.All;
            surf.useGeometry    = NavMeshCollectGeometry.PhysicsColliders;
            EditorUtility.SetDirty(terrain.gameObject);
            added++;
            report.AppendLine($"  ✓ NavMeshSurface added to Terrain '{terrain.name}'");
        }

        // No terrain — target the largest ground/floor mesh in the scene
        if (added == 0)
        {
            float best = 0f;
            MeshFilter bigMesh = null;

            foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude))
            {
                if (mf.sharedMesh == null) continue;
                string n = mf.gameObject.name.ToLower();
                bool isGround = mf.gameObject.CompareTag("Ground")
                             || n.Contains("ground") || n.Contains("terrain")
                             || n.Contains("floor")  || n.Contains("plane");
                if (!isGround) continue;
                float sz = mf.sharedMesh.bounds.extents.sqrMagnitude;
                if (sz > best) { best = sz; bigMesh = mf; }
            }

            if (bigMesh != null && bigMesh.GetComponent<NavMeshSurface>() == null)
            {
                var surf = bigMesh.gameObject.AddComponent<NavMeshSurface>();
                surf.collectObjects = CollectObjects.All;
                surf.useGeometry    = NavMeshCollectGeometry.PhysicsColliders;
                EditorUtility.SetDirty(bigMesh.gameObject);
                added++;
                report.AppendLine($"  ✓ NavMeshSurface added to '{bigMesh.name}'");
            }
            else if (bigMesh == null)
            {
                report.AppendLine(
                    "  ⚠ No Terrain or Ground mesh found. Tag your ground object as 'Ground'" +
                    " then re-run, or add NavMeshSurface manually.");
            }
        }

        return added;
    }

    static int PatchExistingEnemies(StringBuilder report)
    {
        var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Exclude);
        int added = 0;

        foreach (var ec in enemies)
        {
            if (ec.GetComponent<EnemyHeavyAttack>() != null) continue;
            ec.gameObject.AddComponent<EnemyHeavyAttack>();
            EditorUtility.SetDirty(ec.gameObject);
            added++;
        }

        if (added > 0)
            report.AppendLine($"  ✓ EnemyHeavyAttack added to {added} existing enemy (ies)");

        return added;
    }

    [MenuItem("BCE/Setup/8 ▶ Zone Combat Setup (active scene)", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
