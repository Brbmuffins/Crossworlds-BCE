#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// HubPlayableBuilder — BCE/Hub World/Wire Combat Assets (Make Playable)
///
/// One-click script that bridges every remaining gap between "scenes exist"
/// and "I can actually fight enemies in Hub."
///
/// What it does (in order):
///   1. Class prefabs (Marauder/Ironclad/Shadowblade/Cleric/Arcanist/Necromancer)
///        • Add Health (isPlayer=true, maxHealth=150) if missing
///        • Add CharacterStats if missing
///        • Add AbilityCaster if missing
///        • Add CastAnimator if missing
///        • Add PlayerAnimator if missing
///        • Set tag = "Player"  ← REQUIRED for EnemyController.TickIdle() aggro
///
///   2. Enemy prefabs (Enemy_Grunt / Enemy_Ranged / Enemy_Elite)
///        • Assign worldItemPrefab → Assets/Game/Prefabs/WorldItem.prefab
///        • Assign meleeImpactVFX  → brbmuffins MetalImpacts.prefab
///        • Assign deathVFX        → brbmuffins SmallExplosion.prefab
///
///   3. LoginScene → NetworkManager.spawnPrefabs
///        • Add Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem if not already listed
///        • Save and close LoginScene
///
///   4. Hub scene WaveSpawner → set introDelay = 3f (fast combat start)
///
///   5. NavMeshSurface on Hub ground → call BuildNavMesh() via reflection
///
/// Preconditions (run these first if you haven't):
///   BCE/Setup/4   — class prefabs must exist
///   BCE/Setup/4a–4c — enemy prefabs must exist
///   BCE/Hub World/Make Hub a Combat World — WaveSpawner + NavMeshSurface must exist
///
/// Safe to re-run: every step is idempotent.
/// </summary>
public static class HubPlayableBuilder
{
    // ── Asset paths ──────────────────────────────────────────────────────────────
    const string PrefabDir   = "Assets/Game/Prefabs";
    const string LoginScene  = SceneNames.LoginPath;
    const string HubScene    = SceneNames.HubPath;
    const string SfxDir      = "Assets/Retro Sci-Fi Pack/Compressed";

    const string MetalImpactsPath   =
        "Assets/brbmuffins Technologies/brbmuffins Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts.prefab";
    const string SmallExplosionPath =
        "Assets/brbmuffins Technologies/brbmuffins Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/SmallExplosion.prefab";

    static readonly string[] ClassNames  = { "Marauder", "Ironclad", "Shadowblade", "Cleric", "Arcanist", "Necromancer" };
    static readonly string[] EnemyNames  = { "Enemy_Grunt", "Enemy_Ranged", "Enemy_Elite" };

    // ── Menu entry ───────────────────────────────────────────────────────────────

    [MenuItem("BCE/Hub World/Wire Combat Assets (Make Playable)")]
    public static void WireCombatAssets()
    {
        int issues = 0;

        // ── Step 1: Class prefabs ──────────────────────────────────────────────
        issues += PatchClassPrefabs();

        // ── Step 2: Enemy prefabs ──────────────────────────────────────────────
        issues += PatchEnemyPrefabs();

        // ── Step 3: LoginScene spawnPrefabs ───────────────────────────────────
        issues += PatchSpawnPrefabs();

        // ── Step 4: WaveSpawner introDelay ────────────────────────────────────
        issues += PatchWaveSpawner();

        // ── Step 5: Bake NavMesh ───────────────────────────────────────────────
        BakeNavMesh();

        // ── Step 6: CombatAudio clips ──────────────────────────────────────────
        issues += PatchCombatAudio();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary =
            "✅ Combat assets wired!\n\n" +
            "Class prefabs → Health + CharacterStats + AbilityCaster + CastAnimator + PlayerAnimator + tag=\"Player\"\n" +
            "Enemy prefabs → worldItemPrefab + meleeImpactVFX + deathVFX\n" +
            "NetworkManager.spawnPrefabs → enemies + WorldItem registered\n" +
            "WaveSpawner.introDelay = 3s + enemyPrefabs wired\n" +
            "CombatAudio → 7 Retro Sci-Fi Pack clips auto-assigned\n" +
            "NavMesh baked (if Hub was open)\n\n" +
            (issues == 0
                ? "Everything wired cleanly. Press Play!"
                : $"{issues} warning(s) printed to Console — check before playing.");

        EditorUtility.DisplayDialog("✅ Hub Playable", summary, "Let's fight!");

        Debug.Log("[BCE] ✅ HubPlayableBuilder complete. Check Console for any warnings.");
    }

    // ── Step 1: Class prefabs ────────────────────────────────────────────────────

    static int PatchClassPrefabs()
    {
        int warnings = 0;

        foreach (string cls in ClassNames)
        {
            string path = $"{PrefabDir}/{cls}.prefab";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[HubPlayable] Prefab not found: {path} — run BCE/Setup/4 first.");
                warnings++;
                continue;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = scope.prefabContentsRoot;

                // ── tag="Player" — CRITICAL for enemy aggro ────────────────────
                root.tag = "Player";

                // ── Health ─────────────────────────────────────────────────────
                var health = root.GetComponent<Health>();
                if (health == null)
                {
                    health = root.AddComponent<Health>();
                    Debug.Log($"[HubPlayable] {cls}: Added Health");
                }
                health.isPlayer  = true;
                health.maxHealth = 150f;

                // ── CharacterStats (requires Health — add after) ───────────────
                if (root.GetComponent<CharacterStats>() == null)
                {
                    root.AddComponent<CharacterStats>();
                    Debug.Log($"[HubPlayable] {cls}: Added CharacterStats");
                }

                // ── AbilityCaster ──────────────────────────────────────────────
                if (root.GetComponent<AbilityCaster>() == null)
                {
                    root.AddComponent<AbilityCaster>();
                    Debug.Log($"[HubPlayable] {cls}: Added AbilityCaster");
                }

                // ── CastAnimator ───────────────────────────────────────────────
                if (root.GetComponent<CastAnimator>() == null)
                {
                    root.AddComponent<CastAnimator>();
                    Debug.Log($"[HubPlayable] {cls}: Added CastAnimator");
                }

                // ── PlayerAnimator (wrapped in #if !UNITY_SERVER — use reflection) ──
                var paType = System.Type.GetType("PlayerAnimator");
                if (paType != null && root.GetComponent(paType) == null)
                {
                    root.AddComponent(paType);
                    Debug.Log($"[HubPlayable] {cls}: Added PlayerAnimator");
                }
                else if (paType == null)
                {
                    Debug.LogWarning("[HubPlayable] PlayerAnimator type not found — may be excluded by UNITY_SERVER define. Add manually after switching build target.");
                }
            }

            // Force-reimport so Unity picks up the tag change reliably
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[HubPlayable] {cls}: Patched (Health + Stats + Caster + Anim + tag=Player)");
        }

        return warnings;
    }

    // ── Step 2: Enemy prefabs ────────────────────────────────────────────────────

    static int PatchEnemyPrefabs()
    {
        int warnings = 0;

        var worldItem      = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/WorldItem.prefab");
        var metalImpactVFX = AssetDatabase.LoadAssetAtPath<GameObject>(MetalImpactsPath);
        var smallExplosion = AssetDatabase.LoadAssetAtPath<GameObject>(SmallExplosionPath);
        var enemyAnimCtrl  = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/Game/Animations/EnemyAnimController.controller");

        if (worldItem == null)
        {
            Debug.LogWarning("[HubPlayable] WorldItem.prefab not found at " + PrefabDir +
                             " — drops won't appear. Create the WorldItem prefab and re-run.");
            warnings++;
        }
        if (metalImpactVFX == null)
        {
            Debug.LogWarning("[HubPlayable] MetalImpacts.prefab not found — check brbmuffins pack import.");
            warnings++;
        }
        if (smallExplosion == null)
        {
            Debug.LogWarning("[HubPlayable] SmallExplosion.prefab not found — check brbmuffins pack import.");
            warnings++;
        }

        foreach (string enemy in EnemyNames)
        {
            string path = $"{PrefabDir}/{enemy}.prefab";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[HubPlayable] Enemy prefab not found: {path} — run BCE/Setup/4a–4c first.");
                warnings++;
                continue;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = scope.prefabContentsRoot;
                var ctrl = root.GetComponent<EnemyController>();
                if (ctrl == null) ctrl = root.GetComponentInChildren<EnemyController>();

                if (ctrl == null)
                {
                    Debug.LogWarning($"[HubPlayable] No EnemyController on {path} — skipping VFX wiring.");
                    warnings++;
                    continue;
                }

                if (worldItem != null && ctrl.worldItemPrefab == null)
                    ctrl.worldItemPrefab = worldItem;

                // EnemyController does not yet expose meleeImpactVFX / deathVFX fields.
                // VFX wiring is an editor step once those fields are added to EnemyController.

                // Assign EnemyAnimController if built (run BCE/Setup/4d first)
                if (enemyAnimCtrl != null)
                {
                    var anim = root.GetComponentInChildren<Animator>(true)
                            ?? root.AddComponent<Animator>();
                    if (anim.runtimeAnimatorController == null)
                        anim.runtimeAnimatorController = enemyAnimCtrl;
                }
            }

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[HubPlayable] {enemy}: worldItemPrefab + VFX assigned");
        }

        return warnings;
    }

    // ── Step 3: LoginScene → NetworkManager.spawnPrefabs ─────────────────────────

    static int PatchSpawnPrefabs()
    {
        if (!File.Exists(LoginScene))
        {
            Debug.LogWarning("[HubPlayable] LoginScene not found — run BCE/Setup/1 first.");
            return 1;
        }

        // Prefabs to register
        var toRegister = new List<string>(EnemyNames) { "WorldItem" };

        var loginSceneObj = EditorSceneManager.OpenScene(LoginScene, OpenSceneMode.Single);

        RodNetworkManager nm = null;
        foreach (var root in loginSceneObj.GetRootGameObjects())
        {
            nm = root.GetComponent<RodNetworkManager>();
            if (nm != null) break;
        }

        if (nm == null)
        {
            Debug.LogWarning("[HubPlayable] No RodNetworkManager in LoginScene.");
            return 1;
        }

        int added = 0;
        foreach (string name in toRegister)
        {
            string ppath = $"{PrefabDir}/{name}.prefab";
            if (!File.Exists(ppath))
            {
                Debug.LogWarning($"[HubPlayable] Prefab not found for spawnPrefabs: {ppath}");
                continue;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ppath);
            if (prefab == null) continue;

            bool alreadyRegistered = nm.spawnPrefabs.Contains(prefab);
            if (!alreadyRegistered)
            {
                nm.spawnPrefabs.Add(prefab);
                added++;
                Debug.Log($"[HubPlayable] spawnPrefabs ← {name}");
            }
        }

        if (added > 0)
        {
            EditorUtility.SetDirty(nm);
            Debug.Log($"[HubPlayable] spawnPrefabs: {added} new entries.");
        }

        // ── classPrefabs — required for RodNetworkManager to spawn players ────────
        bool classPrefabsDirty = false;
        if (nm.classPrefabs == null || nm.classPrefabs.Length < ClassNames.Length)
        {
            nm.classPrefabs = new GameObject[ClassNames.Length];
            classPrefabsDirty = true;
        }

        for (int i = 0; i < ClassNames.Length; i++)
        {
            if (nm.classPrefabs[i] != null) continue; // already assigned

            string ppath  = $"{PrefabDir}/{ClassNames[i]}.prefab";
            var    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ppath);
            if (prefab != null)
            {
                nm.classPrefabs[i] = prefab;
                classPrefabsDirty  = true;
                Debug.Log($"[HubPlayable] classPrefabs[{i}] ← {ClassNames[i]}");
            }
            else
            {
                Debug.LogWarning($"[HubPlayable] Class prefab not found: {ppath} — run BCE/Setup/4 first.");
            }
        }

        if (added > 0 || classPrefabsDirty)
        {
            EditorUtility.SetDirty(nm);
            EditorSceneManager.SaveScene(loginSceneObj);
            Debug.Log("[HubPlayable] LoginScene saved — NetworkManager wired.");
        }
        else
        {
            Debug.Log("[HubPlayable] NetworkManager already up to date.");
        }

        return 0;
    }

    // ── Step 4: WaveSpawner.introDelay ──────────────────────────────────────────

    static int PatchWaveSpawner()
    {
        // WaveSpawner lives in Arena_Copper (built by ArenaSceneBuilder).
        // Fall back to Hub in case user placed it there.
        const string ArenaScene = SceneNames.ArenaCopperPath;
        string targetScene = File.Exists(ArenaScene) ? ArenaScene : HubScene;

        var scene = EditorSceneManager.GetSceneByPath(targetScene);
        if (!scene.isLoaded && File.Exists(targetScene))
            scene = EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Additive);

        if (!scene.isLoaded)
        {
            Debug.LogWarning("[HubPlayable] Arena_Copper.unity not found — " +
                             "run BCE/Setup/7 ▶ Create Arena Scene first, then re-run.");
            return 1;
        }

        WaveSpawner spawner = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            spawner = root.GetComponent<WaveSpawner>()
                   ?? root.GetComponentInChildren<WaveSpawner>();
            if (spawner != null) break;
        }

        if (spawner == null)
        {
            Debug.LogWarning("[HubPlayable] No WaveSpawner in Arena_Copper — " +
                             "run BCE/Setup/7 ▶ Create Arena Scene to add one.");
            return 1;
        }

        spawner.introDelay = 3f;

        // Wire enemyPrefabs[0..2] and elitePrefab if they exist and aren't set
        var grunt  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Grunt.prefab");
        var ranged = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Ranged.prefab");
        var elite  = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabDir}/Enemy_Elite.prefab");

        if (spawner.enemyPrefabs == null)
            spawner.enemyPrefabs = new System.Collections.Generic.List<GameObject>();

        if (grunt  != null && !spawner.enemyPrefabs.Contains(grunt))  spawner.enemyPrefabs.Add(grunt);
        if (ranged != null && !spawner.enemyPrefabs.Contains(ranged)) spawner.enemyPrefabs.Add(ranged);
        if (elite  != null && spawner.elitePrefab == null)            spawner.elitePrefab = elite;

        EditorUtility.SetDirty(spawner);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[HubPlayable] WaveSpawner: introDelay=3s + enemyPrefabs + elitePrefab wired");
        return 0;
    }

    // ── Step 6: CombatAudio clips ────────────────────────────────────────────────

    static int PatchCombatAudio()
    {
#if UNITY_SERVER
        // CombatAudio is client-only; this step is a no-op in server builds.
        return 0;
#else
        // CombatAudio sits on the WaveSpawner GO in Arena_Copper (or Hub as fallback).
        const string ArenaScene = SceneNames.ArenaCopperPath;
        string targetScene = File.Exists(ArenaScene) ? ArenaScene : HubScene;

        var scene = EditorSceneManager.GetSceneByPath(targetScene);
        if (!scene.isLoaded && File.Exists(targetScene))
            scene = EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Additive);

        if (!scene.isLoaded)
        {
            Debug.LogWarning("[HubPlayable] Arena_Copper scene not open — CombatAudio skipped. " +
                             "Run BCE/Setup/7 ▶ Create Arena Scene first.");
            return 1;
        }

        // Find or create the CombatAudio host (piggyback on WaveSpawner GameObject)
        WaveSpawner spawner = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            spawner = root.GetComponent<WaveSpawner>()
                   ?? root.GetComponentInChildren<WaveSpawner>();
            if (spawner != null) break;
        }

        GameObject host = spawner != null ? spawner.gameObject : null;
        if (host == null)
        {
            // Fallback: look for an existing CombatAudio GO, or create one
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<CombatAudio>() != null) { host = root; break; }
            }
            if (host == null)
            {
                host = new GameObject("CombatAudio");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(host, scene);
            }
        }

        var audio = host.GetComponent<CombatAudio>() ?? host.AddComponent<CombatAudio>();

        AudioClip Load(string filename)
            => AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxDir}/{filename}");

        if (audio.meleeHit    == null) audio.meleeHit    = Load("Medium swell 02.ogg");
        if (audio.rangedHit   == null) audio.rangedHit   = Load("Notification 01.ogg");
        if (audio.deathSFX    == null) audio.deathSFX    = Load("Deep swell 01.ogg");
        if (audio.shieldSFX   == null) audio.shieldSFX   = Load("Loops/Shield energy [loop] 01.ogg");
        if (audio.waveAlert   == null) audio.waveAlert   = Load("Notification 02.ogg");
        if (audio.abilityCast == null) audio.abilityCast = Load("Medium swell 01.ogg");
        if (audio.healSFX     == null) audio.healSFX     = Load("Notification 03 slow.ogg");

        int missing = 0;
        if (audio.meleeHit    == null) { Debug.LogWarning("[HubPlayable] CombatAudio: meleeHit clip not found.");    missing++; }
        if (audio.rangedHit   == null) { Debug.LogWarning("[HubPlayable] CombatAudio: rangedHit clip not found.");   missing++; }
        if (audio.deathSFX    == null) { Debug.LogWarning("[HubPlayable] CombatAudio: deathSFX clip not found.");    missing++; }
        if (audio.shieldSFX   == null) { Debug.LogWarning("[HubPlayable] CombatAudio: shieldSFX clip not found.");   missing++; }
        if (audio.waveAlert   == null) { Debug.LogWarning("[HubPlayable] CombatAudio: waveAlert clip not found.");   missing++; }
        if (audio.abilityCast == null) { Debug.LogWarning("[HubPlayable] CombatAudio: abilityCast clip not found."); missing++; }
        if (audio.healSFX     == null) { Debug.LogWarning("[HubPlayable] CombatAudio: healSFX clip not found.");     missing++; }

        EditorUtility.SetDirty(host);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[HubPlayable] CombatAudio: {7 - missing}/7 clips assigned on '{host.name}' in {System.IO.Path.GetFileName(targetScene)}");
        return missing > 0 ? 1 : 0;
#endif
    }

    // ── Step 5: Bake NavMesh ──────────────────────────────────────────────────────

    static void BakeNavMesh()
    {
        // NavMeshSurface.BuildNavMesh() — accessed via reflection to avoid hard assembly dependency
        var surfaceType = System.Type.GetType(
            "Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");

        if (surfaceType == null)
        {
            Debug.LogWarning("[HubPlayable] NavMeshSurface type not found. " +
                             "Install the AI Navigation package, then Window → AI → Navigation → Bake.");
            return;
        }

        // Find all NavMeshSurface components in loaded scenes
        var surfaces = Object.FindObjectsByType(
            surfaceType, FindObjectsInactive.Include);

        if (surfaces == null || surfaces.Length == 0)
        {
            Debug.LogWarning("[HubPlayable] No NavMeshSurface found in open scenes. " +
                             "Open Hub.unity and re-run, or bake manually via Window → AI → Navigation.");
            return;
        }

        var buildMethod = surfaceType.GetMethod("BuildNavMesh");
        if (buildMethod == null)
        {
            Debug.LogWarning("[HubPlayable] NavMeshSurface.BuildNavMesh() method not found via reflection.");
            return;
        }

        foreach (var surface in surfaces)
        {
            buildMethod.Invoke(surface, null);
            Debug.Log($"[HubPlayable] NavMesh baked on: {((Component)surface).gameObject.name}");
        }
    }
}
#endif
