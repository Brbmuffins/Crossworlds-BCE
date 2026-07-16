#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// WorldBossBuilder — drops the Null Architect (WorldBossController) into the active scene.
///   BCE/Setup/6  ▶ Create World Boss (Null Architect)   — needs Darkwood (or an arena) open
///   BCE/Setup/6b ▶ Register Boss Spawnables             — opens LoginScene, registers prefabs
///
/// Split into two menu items on purpose: 6b has to open LoginScene to reach
/// RodNetworkManager, which would discard an unsaved boss created by 6. Run 6, save the
/// scene, then run 6b (or let BCE/Setup/GO sequence it).
///
/// What changed from the original builder (all of it was blocking online play):
///   • BossTrigger moved OUT to Assets/Game/Combat/Scripts/BossTrigger.cs. It was declared
///     inside this Editor-folder file behind `#if UNITY_EDITOR`, so it compiled into the
///     Editor assembly and was stripped from every build — online, the fight could never
///     start and the trigger was a missing script.
///   • Boss now gets NetworkTransformUnreliable + NetworkAnimator (server authority). With
///     a NetworkIdentity alone it spawned for clients and then stood frozen — the same bug
///     NetworkSyncFixer (4n) exists to fix on the hero/enemy prefabs.
///   • Uses the real ashen-wasteland model instead of a grey capsule (falls back to the
///     capsule if the art is missing).
///   • Assigns worldItemPrefab — it was never set, so the boss dropped nothing on death.
///   • 6b registers NullShard + WorldItem in RodNetworkManager. Both are NetworkServer.Spawn'd
///     at runtime, so without registration clients cannot instantiate them.
///
/// The boss is a SCENE object, not a spawnable prefab: Mirror bakes its sceneId when you
/// save the scene and spawns it server-side via NetworkServer.SpawnObjects(). That is why
/// saving the scene after running 6 is mandatory, and why the boss itself is not registered
/// in spawnPrefabs (only the things it spawns at runtime are).
/// </summary>
public static class WorldBossBuilder
{
    const string BossModelPath =
        "Assets/Game/3D Models/Bosses/model_ashen_wastland_boss/Model/model_boss_ashen_wasteland_3D.fbx";
    const string BossControllerPath =
        "Assets/Game/3D Models/Bosses/model_ashen_wastland_boss/Controller/boss_ashen_wasteland_controller.controller";
    const string ShardPrefabPath = "Assets/Game/Prefabs/NullShard.prefab";
    const string WorldItemPath   = "Assets/Game/Game_Prefabs/Muffin Junk/WorldItem.prefab";
    const string LoginScenePath  = "Assets/Game/Scenes/LoginScene.unity";

    // ─────────────────────────────────────────────────────────────────────────────
    // 6 — create the boss in the active scene
    // ─────────────────────────────────────────────────────────────────────────────

    [MenuItem("BCE/Setup/6 ▶ Create World Boss (Null Architect)", priority = 20)]
    public static void CreateWorldBoss()
    {
        var scene = EditorSceneManager.GetActiveScene();

        // FindAny, not FindFirst: we only care whether one exists, and FindFirst is
        // deprecated for relying on instance-ID ordering.
        if (UnityEngine.Object.FindAnyObjectByType<WorldBossController>() != null &&
            !EditorUtility.DisplayDialog("Boss Already Present",
                $"'{scene.name}' already contains a WorldBossController.\n\nAdd another?",
                "Add Another", "Cancel"))
            return;

        var boss = new GameObject("NullArchitect_Boss");
        boss.tag = "Enemy";
        boss.transform.position = new Vector3(0f, 1.5f, 0f);

        bool usedRealModel = AttachVisual(boss, out Animator animator);

        // Hit volume — abilities find targets with OverlapSphere / SphereCastAll.
        var capsule = boss.AddComponent<CapsuleCollider>();
        capsule.center = new Vector3(0f, 1.5f, 0f);
        capsule.radius = 1.2f;
        capsule.height = 3f;

        var agent = boss.AddComponent<NavMeshAgent>();
        agent.speed            = 4f;
        agent.angularSpeed     = 180f;
        agent.stoppingDistance = 2f;
        agent.radius           = 1.2f;
        agent.height           = 3f;

        // Health before the controller: WorldBossController [RequireComponent]s Health +
        // StatusEffectManager, and we want our maxHealth to win over a default-constructed one.
        var health = boss.AddComponent<Health>();
        health.maxHealth = 2000f;

        var ctrl = boss.AddComponent<WorldBossController>();
        ConfigureFight(ctrl);
        ctrl.nullShardPrefab  = BuildShardPrefab();
        ctrl.worldItemPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(WorldItemPath);

        // Void Drain VFX placeholder — client-cosmetic, deliberately no NetworkIdentity.
        var drainVFX = new GameObject("VoidDrainVFX");
        drainVFX.transform.SetParent(boss.transform, false);
        drainVFX.SetActive(false);
        ctrl.voidDrainVFX = drainVFX;

        // Networking — identity + movement/anim replication (server authority: the server
        // runs the NavMesh AI and every [Server] ability coroutine).
        boss.AddComponent<NetworkIdentity>();
        AddServerAuthTransform(boss);
        AddServerAuthAnimator(boss, animator);

        // Proximity trigger — first player inside starts the fight.
        var triggerObj = new GameObject("BossTrigger");
        triggerObj.transform.SetParent(boss.transform, false);
        var triggerCol = triggerObj.AddComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius    = 15f;
        triggerObj.AddComponent<BossTrigger>();

        var lightObj = new GameObject("BossLight");
        lightObj.transform.SetParent(boss.transform, false);
        lightObj.transform.localPosition = new Vector3(0f, 4f, 0f);
        var l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.4f, 0.1f, 1f);
        l.intensity = 3f;
        l.range = 20f;

        Selection.activeGameObject = boss;
        EditorUtility.SetDirty(boss);
        EditorSceneManager.MarkSceneDirty(scene);

        EditorUtility.DisplayDialog("Null Architect Created ✓",
            $"Placed in '{scene.name}' at {boss.transform.position}.\n\n" +
            $"  • Visual: {(usedRealModel ? "ashen-wasteland model" : "CAPSULE fallback — model not found")}\n" +
            $"  • Health: 2000\n" +
            $"  • NullShard prefab: {(ctrl.nullShardPrefab != null ? "built ✓" : "FAILED ✗")}\n" +
            $"  • Drops (worldItemPrefab): {(ctrl.worldItemPrefab != null ? "wired ✓" : "NOT FOUND ✗")}\n" +
            "  • NetworkIdentity + NetworkTransform + NetworkAnimator ✓\n\n" +
            "NEXT — REQUIRED, IN ORDER:\n" +
            "1. Move the boss where you want it (it spawns at world origin).\n" +
            "2. Bake the NavMesh if you moved it far (Window → AI → Navigation).\n" +
            "3. SAVE THE SCENE (Ctrl+S) — Mirror bakes the sceneId on save; without it\n" +
            "   the boss will not spawn for clients.\n" +
            "4. Run BCE/Setup/6b to register NullShard + WorldItem for spawning.\n" +
            "5. Commit + push — CI builds and deploys the server automatically.",
            "Done");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 6b — register the prefabs the boss spawns at runtime
    // ─────────────────────────────────────────────────────────────────────────────

    [MenuItem("BCE/Setup/6b ▶ Register Boss Spawnables (NullShard + WorldItem)", priority = 21)]
    public static void RegisterBossSpawnables()
    {
        var report = new List<string>();
        bool ok = RegisterSpawnables(report);

        EditorUtility.DisplayDialog(ok ? "Boss Spawnables Registered ✓" : "Registration Failed ✗",
            string.Join("\n", report) +
            (ok ? "\n\nSaved LoginScene. Rebuild/redeploy (or just push — CI does it)."
                : "\n\nAdd them to RodNetworkManager.worldPrefabs by hand."),
            "OK");
    }

    /// <summary>Opens LoginScene and adds the runtime-spawned boss prefabs to RodNetworkManager.</summary>
    internal static bool RegisterSpawnables(List<string> report)
    {
        if (!File.Exists(LoginScenePath)) { report.Add("  ✗ LoginScene not found"); return false; }

        var toRegister = new[] { ShardPrefabPath, WorldItemPath }
            .Select(p => (path: p, go: AssetDatabase.LoadAssetAtPath<GameObject>(p)))
            .ToArray();

        foreach (var (path, go) in toRegister)
            if (go == null) report.Add($"  ✗ missing: {Path.GetFileName(path)} (run BCE/Setup/6 first?)");

        var prefabs = toRegister.Where(t => t.go != null).Select(t => t.go).ToList();
        if (prefabs.Count == 0) return false;

        var scene = EditorSceneManager.OpenScene(LoginScenePath, OpenSceneMode.Single);
        var nm = scene.GetRootGameObjects()
            .Select(r => r.GetComponent<RodNetworkManager>())
            .FirstOrDefault(x => x != null);
        if (nm == null) { report.Add("  ✗ RodNetworkManager not in LoginScene"); return false; }

        var world = new List<GameObject>(nm.worldPrefabs ?? Array.Empty<GameObject>());
        foreach (var p in prefabs)
        {
            // A prefab must carry a NetworkIdentity or Mirror cannot spawn it at all.
            if (p.GetComponent<NetworkIdentity>() == null)
            {
                report.Add($"  ✗ {p.name} has no NetworkIdentity — skipped");
                continue;
            }

            bool isNew = !world.Contains(p);
            if (isNew) world.Add(p);
            if (!nm.spawnPrefabs.Contains(p)) nm.spawnPrefabs.Add(p);
            report.Add($"  {(isNew ? "+" : "=")} {p.name}{(isNew ? " registered" : " already registered")}");
        }

        nm.worldPrefabs = world.ToArray();
        EditorUtility.SetDirty(nm);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        report.Add($"  → worldPrefabs total: {world.Count}");
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    static void ConfigureFight(WorldBossController ctrl)
    {
        ctrl.phase2Threshold            = 0.60f;
        ctrl.phase3Threshold            = 0.30f;
        ctrl.finalSurgeThreshold        = 0.10f;
        ctrl.reflectPulseInterval       = 18f;
        ctrl.reflectTelegraphDuration   = 3f;
        ctrl.reflectWindowDuration      = 4f;
        ctrl.shardSpreadRadius          = 6f;
        ctrl.tetherWebInterval          = 25f;
        ctrl.tetherWebDuration          = 6f;
        ctrl.tetherWebLeashDistance     = 6f;
        ctrl.tetherWebSnapDamage        = 40f;
        ctrl.voidDrainInterval          = 12f;
        ctrl.voidDrainRadius            = 5f;
        ctrl.voidDrainTickDamage        = 8f;
        ctrl.voidDrainDuration          = 4f;
        ctrl.finalSurgeSpeedMultiplier  = 3f;
        ctrl.finalSurgeAttackMultiplier = 3f;
        ctrl.finalSurgeDuration         = 15f;
        ctrl.immunityWindowDuration     = 4f;
        ctrl.guaranteedDropItemIds      = new List<string> { "sword_iron", "plate_iron" };
        ctrl.rareDropItemIds            = new List<string> { "ring_copper", "material_copper_bar" };
        ctrl.rareDropChance             = 0.35f;
    }

    /// <summary>Parents the boss art under the root. Returns false if it fell back to a capsule.</summary>
    static bool AttachVisual(GameObject boss, out Animator animator)
    {
        animator = null;
        var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BossModelPath);

        if (modelAsset == null)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Visual_CapsuleFallback";
            capsule.transform.SetParent(boss.transform, false);
            capsule.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            capsule.transform.localScale    = new Vector3(2.5f, 1.5f, 2.5f);
            UnityEngine.Object.DestroyImmediate(capsule.GetComponent<Collider>()); // root owns the hit volume
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.05f, 0f, 0.15f) };
            capsule.GetComponent<Renderer>().sharedMaterial = mat;
            return false;
        }

        var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        model.name = "Visual";
        model.transform.SetParent(boss.transform, false);

        animator = model.GetComponentInChildren<Animator>(true);
        if (animator == null) animator = model.AddComponent<Animator>();

        var animCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(BossControllerPath);
        if (animCtrl != null) animator.runtimeAnimatorController = animCtrl;

        // Root motion fights NavMeshAgent for control of the transform — the agent drives.
        animator.applyRootMotion = false;
        return true;
    }

    static void AddServerAuthTransform(GameObject boss)
    {
        var nt = boss.AddComponent<NetworkTransformUnreliable>();
        if (nt.target == null) nt.target = boss.transform;
        nt.syncDirection = SyncDirection.ServerToClient;
        nt.syncPosition  = true;
        nt.syncRotation  = true;
        nt.syncScale     = false;
    }

    static void AddServerAuthAnimator(GameObject boss, Animator animator)
    {
        var na = boss.AddComponent<NetworkAnimator>();
        na.animator = animator != null ? animator : boss.GetComponentInChildren<Animator>(true);
        na.clientAuthority = false;
    }

    static GameObject BuildShardPrefab()
    {
        var shard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shard.name = "NullShard";
        shard.tag  = "Enemy";
        shard.transform.localScale = Vector3.one * 1.2f;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.3f, 0f, 0.8f) };
        shard.GetComponent<Renderer>().sharedMaterial = mat;

        shard.AddComponent<NetworkIdentity>();

        var shardHealth = shard.AddComponent<Health>();
        shardHealth.maxHealth = 400f;

        var shardLight = new GameObject("ShardLight");
        shardLight.transform.SetParent(shard.transform, false);
        var l = shardLight.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.5f, 0.2f, 1f);
        l.intensity = 2f;
        l.range = 8f;

        if (!AssetDatabase.IsValidFolder("Assets/Game/Prefabs"))
            AssetDatabase.CreateFolder("Assets/Game", "Prefabs");

        var prefab = PrefabUtility.SaveAsPrefabAsset(shard, ShardPrefabPath, out bool ok);
        UnityEngine.Object.DestroyImmediate(shard);

        if (!ok) Debug.LogWarning($"[BCE] Could not save NullShard to {ShardPrefabPath}");
        return ok ? prefab : null;
    }
}
#endif
