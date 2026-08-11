#if UNITY_EDITOR
using System;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

namespace Crossworlds.EditorTools.EnemyForge
{
    internal static class EnemyForgeBuilder
    {
        const string BackupFolder = "Library/EnemyForgeBackups/LastDeploy";
        const string BackupManifestPath = BackupFolder + "/manifest.json";
        const string FallbackProjectilePath =
            "Assets/Game/Prefabs/EnemyForge/EnemyForge_RangedTestProjectile.prefab";
        const string ElementalLightningProfilePath =
            "Assets/Game/Resources/EnemyAbilities/ElementalLightning.asset";
        const string LightningAuraPath =
            "Assets/Game/FX/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Character auras/Lightning aura.prefab";
        const string LightningProjectorPath =
            "Assets/Game/FX/dark magic/SubEffects/LightningStrikeProjector.prefab";
        const string ElectroHitPath =
            "Assets/Game/FX/brbmuffins Studio/brbmuffins Magic Pack/Prefabs/Hits and explosions/Electro hit.prefab";

        [InitializeOnLoadMethod]
        static void ScheduleGeneratedAnimationRepair()
        {
            EditorApplication.delayCall += NormalizeExistingGeneratedClips;
        }

        [Serializable]
        sealed class DeployBackupManifest
        {
            public string prefabPath;
            public string prefabBackup;
            public string overridePath;
            public string overrideBackup;
            public List<SceneBackup> scenes = new List<SceneBackup>();
        }

        [Serializable]
        sealed class SceneBackup
        {
            public string assetPath;
            public string backupPath;
        }

        public static GameObject Build(EnemyForgeDefinition d)
        {
            string folder = ResolveOutputFolder(d);
            EnsureFolder(folder);
            var sourcePath = AssetDatabase.GetAssetPath(d.source);
            GameObject root;
            if (!string.IsNullOrEmpty(sourcePath) && PrefabUtility.IsPartOfPrefabAsset(d.source))
                root = (GameObject)PrefabUtility.InstantiatePrefab(d.source);
            else
                root = UnityEngine.Object.Instantiate(d.source);

            root.name = BuildForgedPrefabName(d.source.name, d.IsNpc);
            Undo.RegisterCreatedObjectUndo(root, "Build enemy prefab");

            // Prefab assets should have a neutral root transform. Imported/source prefabs
            // sometimes contain a scene-authoring position, which made newly forged enemies
            // appear far away when dragged into a scene.
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;

            Configure(root, d, false);
            ConfigureDriverMode(root, d);

            string targetPath = $"{folder}/{root.name}.prefab";
            string path = ChooseSavePath(targetPath);
            if (string.IsNullOrEmpty(path))
            {
                UnityEngine.Object.DestroyImmediate(root);
                return null;
            }

            if (d.generateAnimatorController && d.animationDriverMode != EnemyForgeAnimationDriverMode.ExistingModelDriver)
                GenerateAnimatorOverride(root, d, path);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            return prefab;
        }

        public static GameObject UpdateBuiltPrefabConfiguration(
            EnemyForgeDefinition d)
        {
            if (d == null || d.source == null) return null;

            string folder = ResolveOutputFolder(d);
            string path =
                $"{folder}/{BuildForgedPrefabName(d.source.name, d.IsNpc)}.prefab";
            GameObject prefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return null;

            try
            {
                using (var scope =
                       new PrefabUtility.EditPrefabContentsScope(path))
                {
                    GameObject root = scope.prefabContentsRoot;
                    root.transform.position = Vector3.zero;
                    root.transform.rotation = Quaternion.identity;
                    Configure(root, d, false);
                    ConfigureDriverMode(root, d);
                    if (d.generateAnimatorController &&
                        d.animationDriverMode !=
                        EnemyForgeAnimationDriverMode.ExistingModelDriver)
                        GenerateAnimatorOverride(root, d, path);
                }

                AssetDatabase.ImportAsset(
                    path, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        static void Configure(GameObject root, EnemyForgeDefinition d, bool recordUndo)
        {
            if (recordUndo) Undo.RegisterFullObjectHierarchyUndo(root, "Update enemy instance");
            TrySetRootTag(root, d.IsNpc ? EnemyForgeRootTag.NPC : d.rootTag);
            GetOrAdd<NetworkIdentity>(root);
            if (!d.IsNpc)
            {
                var health = GetOrAdd<Health>(root);
                health.maxHealth = d.maxHealth;
                health.isPlayer = false;
                health.isRobotic = d.robotic;
                health.ConfigureEnemyHoverIdentity(d.enemyDisplayName, d.enemyLevel);
            }

            var agent = GetOrAdd<NavMeshAgent>(root);
            agent.speed = d.moveSpeed;
            agent.acceleration = d.acceleration;
            agent.angularSpeed = d.angularSpeed;
            agent.radius = d.agentRadius;
            agent.height = d.agentHeight;
            agent.baseOffset = d.agentBaseOffset;
            agent.stoppingDistance = d.stoppingDistance;

            EnsureCollider(root, d);

            if (d.IsNpc)
            {
                ConfigureNonCombatNpc(root, d, recordUndo);
                EditorUtility.SetDirty(root);
                return;
            }

            var controller = GetOrAdd<EnemyController>(root);
            var sfx = GetOrAdd<EnemySfxProfile>(root);
            sfx.aggro = d.aggroSfx;
            sfx.attack1 = d.attack1Sfx;
            sfx.attack2 = d.attack2Sfx;
            sfx.attack3 = d.attack3Sfx;
            sfx.attack4 = d.attack4Sfx;
            sfx.attackImpact = d.attackImpactSfx;
            sfx.getHit = d.getHitSfx;
            sfx.death = d.deathSfx;
            sfx.volume = d.sfxVolume;
            sfx.pitchVariation = d.sfxPitchVariation;
            sfx.minDistance = d.sfxMinDistance;
            sfx.maxDistance = Mathf.Max(d.sfxMinDistance + 0.1f, d.sfxMaxDistance);
            controller.allowOfflineSimulation = false;
            controller.enemyForgeRuntimeProfileVersion = EnemyController.EnemyForgeRuntimeProfileVersion;
            controller.enemyTemplateId = d.templateId.Trim();
            controller.rewardCategory = d.rewardCategory;
            controller.aggroRadius = d.aggroRadius;
            controller.leashRadius = d.leashRadius;
            controller.enableRoaming = d.enableRoaming;
            controller.roamingRadius = d.roamingRadius;
            controller.roamingMinWait = d.roamingMinWait;
            controller.roamingMaxWait = d.roamingMaxWait;
            controller.aggroWhenDamaged = d.aggroWhenDamaged;
            // A ranged enemy navigates toward preferredRange. Its attack envelope
            // must reach that ring or it will arrive, stop, and remain in Chase
            // without ever firing.
            float rangedEngagementRange = d.preferredRange + Mathf.Max(0.5f, d.agentRadius);
            controller.attackRange = d.IsRanged
                ? Mathf.Max(d.attackRange, rangedEngagementRange)
                : d.attackRange;
            var attackClips = GetAttackClips(d);
            var attackSpeeds = GetAttackSpeeds(d);
            float longestAttackClip = LongestClipLength(attackClips, attackSpeeds);
            bool hasAlternateAttacks = d.attackAnimation2 != null ||
                d.attackAnimation3 != null || d.attackAnimation4 != null;
            // Ranged casts and randomized attack sets must complete their longest
            // selected clip before another attack can retrigger the Animator.
            controller.attackInterval = (d.IsRanged || hasAlternateAttacks) && longestAttackClip > 0f
                ? Mathf.Max(d.attackInterval, longestAttackClip)
                : d.attackInterval;
            controller.damage = d.damage;
            controller.combatTurnSpeed = d.combatTurnSpeed;
            float maximumImpactDelay = Mathf.Max(0f, controller.attackInterval - 0.05f);
            controller.attackAnimationVariantMask = BuildAttackVariantMask(attackClips);
            controller.attackAnimationImpactDelays = BuildAttackImpactDelays(
                attackClips, GetAttackImpactPoints(d), attackSpeeds, maximumImpactDelay);
            controller.attackImpactDelay = controller.attackAnimationImpactDelays[0];
            controller.idleAnimationSpeed = d.idleAnimationSpeed;
            controller.chaseAnimationSpeed = d.chaseAnimationSpeed;
            controller.attackAnimationSpeeds = attackSpeeds;
            controller.getHitAnimationSpeed = d.getHitAnimationSpeed;
            controller.deathAnimationSpeed = d.deathAnimationSpeed;
            controller.attackVfxOffsets = new[]
            {
                d.attackVfxOffset,
                d.attackVfxOffset2,
                d.attackVfxOffset3,
                d.attackVfxOffset4
            };
            controller.isRanged = d.IsRanged;
            controller.projectilePrefab = d.IsRanged
                ? (d.projectilePrefab != null ? d.projectilePrefab : EnsureFallbackProjectilePrefab())
                : d.projectilePrefab;
            controller.preferredRange = d.preferredRange;
            controller.tooCloseDistance = d.tooCloseDistance;
            controller.dropTable = d.dropTable;
            controller.worldItemPrefab =
                EnemyForgeLootPrefabUtility.ResolveNetworkReadyPickup(
                    d.worldItemPrefab);
            controller.lootBeamPrefab = d.lootBeamPrefab;
            if (controller.worldItemPrefab != d.worldItemPrefab)
            {
                d.worldItemPrefab = controller.worldItemPrefab;
                EditorUtility.SetDirty(d);
            }
            controller.deadModelVisibleSeconds = d.deadModelVisibleSeconds;
            controller.respawnAfterDeath = d.respawnAfterDeath;
            controller.respawnDelay = d.respawnDelay;
            // Standard Enemy Forge safety profile: death clips must never lift the
            // corpse away from the authored ground contact point.
            controller.keepCorpseGrounded = true;
            controller.corpseGroundingRenderer = FindPrimaryBodyRenderer(root);
            controller.corpseGroundOffset = d.corpseGroundOffset;

            if (d.addHeavyAttack)
            {
                var heavy = GetOrAdd<EnemyHeavyAttack>(root);
                heavy.minCooldown = d.heavyMinCooldown;
                heavy.maxCooldown = d.heavyMaxCooldown;
                heavy.damageMultiplier = d.heavyDamageMultiplier;
                heavy.castDistanceToTarget = d.IsRanged && d.castImmediatelyOnAggro
                    ? Mathf.Max(d.rangedCastDistance, d.aggroRadius)
                    : d.rangedCastDistance;
                heavy.castImmediatelyOnAggro = d.IsRanged && d.castImmediatelyOnAggro;
                heavy.openingCastRandom = d.openingCast == EnemyForgeCastAttack.RandomAll;
                heavy.openingCastType = heavy.openingCastRandom
                    ? EnemyHeavyAttack.HeavyAbilityType.HexBlast
                    : (EnemyHeavyAttack.HeavyAbilityType)((int)d.openingCast - 1);
                heavy.openingCastDelay = d.openingCastDelay;
                heavy.openingCastOncePerAggro = d.openingCastOncePerAggro;
                heavy.openingCastRequiresLineOfSight = d.openingCastRequiresLineOfSight;
                heavy.cancelOpeningCastIfTargetInvalid = d.cancelOpeningCastIfTargetInvalid;
                heavy.availableTypes = d.castAttack == EnemyForgeCastAttack.RandomAll
                    ? System.Array.Empty<EnemyHeavyAttack.HeavyAbilityType>()
                    : new[]
                    {
                        (EnemyHeavyAttack.HeavyAbilityType)((int)d.castAttack - 1)
                    };
                heavy.elementalLightningVfxProfile =
                    d.elementalLightningVfxProfile != null
                        ? d.elementalLightningVfxProfile
                        : EnsureElementalLightningProfile();
            }
            else
            {
                var heavy = root.GetComponent<EnemyHeavyAttack>();
                if (heavy != null)
                {
                    if (recordUndo)
                        Undo.DestroyObjectImmediate(heavy);
                    else
                        UnityEngine.Object.DestroyImmediate(heavy, true);
                }
            }
            EditorUtility.SetDirty(root);
        }

        static void ConfigureNonCombatNpc(GameObject root, EnemyForgeDefinition d,
            bool recordUndo)
        {
            RemoveComponent<EnemyController>(root, recordUndo);
            RemoveComponent<EnemyHeavyAttack>(root, recordUndo);
            RemoveComponent<EnemySfxProfile>(root, recordUndo);
            RemoveComponent<EnemyWanderAI>(root, recordUndo);
            RemoveComponent<EnemyAI>(root, recordUndo);
            RemoveComponent<NpcController>(root, recordUndo);
            RemoveComponent<Health>(root, recordUndo);

            var npc = GetOrAdd<ForgedNpcController>(root);
            npc.npcDisplayName = string.IsNullOrWhiteSpace(d.enemyDisplayName)
                ? root.name.Replace("forged_", string.Empty).Replace("_NPC", string.Empty)
                : d.enemyDisplayName.Trim();
            npc.enableRoaming = d.enableRoaming;
            npc.roamingRadius = d.roamingRadius;
            npc.roamingMinWait = d.roamingMinWait;
            npc.roamingMaxWait = Mathf.Max(d.roamingMinWait, d.roamingMaxWait);

            var transformSync = root.GetComponent<NetworkTransformBase>();
            if (transformSync == null)
                transformSync = Undo.AddComponent<NetworkTransformUnreliable>(root);
            transformSync.target = root.transform;
            transformSync.syncDirection = SyncDirection.ServerToClient;
            transformSync.syncPosition = true;
            transformSync.syncRotation = true;
            transformSync.syncScale = false;

            Animator animator = root.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                var animatorSync = GetOrAdd<NetworkAnimator>(root);
                animatorSync.animator = animator;
                animatorSync.clientAuthority = false;
                EditorUtility.SetDirty(animatorSync);
            }

            EditorUtility.SetDirty(npc);
            EditorUtility.SetDirty(transformSync);
        }

        static void RemoveComponent<T>(GameObject root, bool recordUndo)
            where T : Component
        {
            T component = root.GetComponent<T>();
            if (component == null) return;
            if (recordUndo) Undo.DestroyObjectImmediate(component);
            else UnityEngine.Object.DestroyImmediate(component, true);
        }

        static ElementalLightningVFXProfile EnsureElementalLightningProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<ElementalLightningVFXProfile>(
                ElementalLightningProfilePath);
            if (profile == null)
            {
                EnsureFolder("Assets/Game/Resources/EnemyAbilities");
                profile = ScriptableObject.CreateInstance<ElementalLightningVFXProfile>();
                AssetDatabase.CreateAsset(profile, ElementalLightningProfilePath);
            }

            profile.handEffect = AssetDatabase.LoadAssetAtPath<GameObject>(LightningAuraPath);
            profile.spellEffect = AssetDatabase.LoadAssetAtPath<GameObject>(LightningProjectorPath);
            profile.hitEffect = AssetDatabase.LoadAssetAtPath<GameObject>(ElectroHitPath);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static GameObject EnsureFallbackProjectilePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(FallbackProjectilePath);
            if (existing != null)
                return existing;

            EnsureFolder(Path.GetDirectoryName(FallbackProjectilePath)?.Replace('\\', '/'));

            var root = new GameObject("EnemyForge_RangedTestProjectile");
            try
            {
                var identity = root.AddComponent<NetworkIdentity>();
                var transformSync = root.AddComponent<NetworkTransformUnreliable>();
                transformSync.target = root.transform;
                transformSync.syncDirection = SyncDirection.ServerToClient;
                transformSync.syncPosition = true;
                transformSync.syncRotation = true;
                transformSync.syncScale = false;

                var hitbox = root.AddComponent<SphereCollider>();
                hitbox.radius = 0.22f;
                hitbox.isTrigger = true;

                var body = root.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                var projectile = root.AddComponent<EnemyProjectile>();
                projectile.speed = 12f;
                projectile.lifetime = 4f;

                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Temporary Projectile Visual";
                visual.transform.SetParent(root.transform, false);
                visual.transform.localScale = Vector3.one * 0.35f;
                var visualCollider = visual.GetComponent<Collider>();
                if (visualCollider != null)
                    UnityEngine.Object.DestroyImmediate(visualCollider);

                EditorUtility.SetDirty(identity);
                EditorUtility.SetDirty(transformSync);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, FallbackProjectilePath);
                AssetDatabase.ImportAsset(FallbackProjectilePath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static SkinnedMeshRenderer FindPrimaryBodyRenderer(GameObject root)
        {
            SkinnedMeshRenderer best = null;
            float largestVolume = -1f;
            foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer == null) continue;
                string objectName = renderer.gameObject.name.ToLowerInvariant();
                if (objectName.Contains("weapon") || objectName.Contains("mace") ||
                    objectName.Contains("sword") || objectName.Contains("shield") ||
                    objectName.Contains("vfx") || objectName.Contains("particle"))
                    continue;
                Vector3 size = renderer.bounds.size;
                float volume = size.x * size.y * size.z;
                if (volume <= largestVolume) continue;
                largestVolume = volume;
                best = renderer;
            }
            return best;
        }

        static string ResolveOutputFolder(EnemyForgeDefinition d)
        {
            string sourcePath = AssetDatabase.GetAssetPath(d.source).Replace('\\', '/');
            string sourceFolder = System.IO.Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
            return !string.IsNullOrEmpty(sourceFolder) && sourceFolder.StartsWith("Assets")
                ? sourceFolder
                : d.outputFolder;
        }

        static string ChooseSavePath(string targetPath)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) == null) return targetPath;
            int choice = EditorUtility.DisplayDialogComplex(
                "Enemy Prefab Already Exists",
                $"{targetPath} already exists.\n\nReplace it, or save this enemy as a new prefab?",
                "Replace", "Cancel", "Save As New");
            if (choice == 0) return targetPath;
            if (choice == 2) return AssetDatabase.GenerateUniqueAssetPath(targetPath);
            return null;
        }

        public static int Deploy(GameObject savedPrefab, EnemyForgeDefinition d)
        {
            if (savedPrefab == null || d == null) return -1;
            string path = AssetDatabase.GetAssetPath(savedPrefab);
            if (string.IsNullOrEmpty(path) || !PrefabUtility.IsPartOfPrefabAsset(savedPrefab)) return -1;

            try
            {
                path = EnsureForgedAssetNames(path, d.IsNpc);
                CreateDeployBackup(path);
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    GameObject root = scope.prefabContentsRoot;
                    root.name = Path.GetFileNameWithoutExtension(path);
                    root.transform.position = Vector3.zero;
                    root.transform.rotation = Quaternion.identity;
                    Configure(root, d, false);
                    ConfigureDriverMode(root, d);
                    if (d.generateAnimatorController && d.animationDriverMode != EnemyForgeAnimationDriverMode.ExistingModelDriver)
                        GenerateAnimatorOverride(root, d, path);
                }
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
                var updatedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return UpdateDeployedInstances(updatedPrefab, d);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return -1;
            }
        }

        static string EnsureForgedAssetNames(string prefabPath, bool isNpc)
        {
            string folder = Path.GetDirectoryName(prefabPath).Replace('\\', '/');
            string oldName = Path.GetFileNameWithoutExtension(prefabPath);
            if (oldName.StartsWith("forged_", StringComparison.OrdinalIgnoreCase))
                return prefabPath;

            string newName = BuildForgedPrefabName(oldName, isNpc);
            string newPrefabPath = $"{folder}/{newName}.prefab";
            if (AssetDatabase.LoadMainAssetAtPath(newPrefabPath) != null)
                throw new InvalidOperationException(
                    $"Cannot rename '{oldName}' to '{newName}' because that prefab already exists.");

            var moves = new List<KeyValuePair<string, string>>();
            string oldOverride = $"{folder}/{oldName}_Animations.overrideController";
            string newOverride = $"{folder}/{newName}_Animations.overrideController";
            if (AssetDatabase.LoadMainAssetAtPath(oldOverride) != null)
                moves.Add(new KeyValuePair<string, string>(oldOverride, newOverride));

            string clipFolder = folder + "/EnemyForgeClips";
            if (AssetDatabase.IsValidFolder(clipFolder))
            {
                foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { clipFolder }))
                {
                    string oldClipPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!Path.GetFileNameWithoutExtension(oldClipPath)
                        .StartsWith(oldName + "_", StringComparison.OrdinalIgnoreCase))
                        continue;
                    string suffix = Path.GetFileName(oldClipPath).Substring(oldName.Length);
                    moves.Add(new KeyValuePair<string, string>(
                        oldClipPath, $"{clipFolder}/{newName}{suffix}"));
                }
            }

            moves.Add(new KeyValuePair<string, string>(prefabPath, newPrefabPath));
            foreach (var move in moves)
            {
                if (AssetDatabase.LoadMainAssetAtPath(move.Value) != null)
                    throw new InvalidOperationException(
                        $"Cannot migrate Enemy Forge asset naming because '{move.Value}' already exists.");
            }

            foreach (var move in moves)
            {
                string error = AssetDatabase.MoveAsset(move.Key, move.Value);
                if (!string.IsNullOrEmpty(error))
                    throw new InvalidOperationException(
                        $"Enemy Forge could not rename '{move.Key}' to '{move.Value}': {error}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[Enemy Forge] Renamed deployed prefab '{oldName}' to '{newName}' and migrated its generated animation assets.");
            return newPrefabPath;
        }

        public static bool HasDeployBackup()
        {
            return File.Exists(ToAbsolutePath(BackupManifestPath));
        }

        public static string RollBackLastDeploy()
        {
            string manifestFile = ToAbsolutePath(BackupManifestPath);
            if (!File.Exists(manifestFile)) return "No local deployment backup is available.";
            try
            {
                var manifest = JsonUtility.FromJson<DeployBackupManifest>(File.ReadAllText(manifestFile));
                if (manifest == null || string.IsNullOrEmpty(manifest.prefabPath))
                    return "The local deployment backup is invalid.";

                var loadedScenePaths = new HashSet<string>();
                string activeScenePath = SceneManager.GetActiveScene().path;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.isLoaded && !string.IsNullOrEmpty(scene.path)) loadedScenePaths.Add(scene.path);
                }

                RestoreBackupFile(manifest.prefabBackup, manifest.prefabPath);
                if (!string.IsNullOrEmpty(manifest.overridePath) && !string.IsNullOrEmpty(manifest.overrideBackup))
                    RestoreBackupFile(manifest.overrideBackup, manifest.overridePath);
                foreach (var sceneBackup in manifest.scenes)
                    RestoreBackupFile(sceneBackup.backupPath, sceneBackup.assetPath);

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset(manifest.prefabPath, ImportAssetOptions.ForceUpdate);

                var scenesToReload = manifest.scenes.FindAll(x => loadedScenePaths.Contains(x.assetPath));
                if (scenesToReload.Count > 0)
                {
                    int primaryIndex = scenesToReload.FindIndex(x => x.assetPath == activeScenePath);
                    if (primaryIndex < 0) primaryIndex = 0;
                    Scene primary = EditorSceneManager.OpenScene(scenesToReload[primaryIndex].assetPath, OpenSceneMode.Single);
                    SceneManager.SetActiveScene(primary);
                    for (int i = 0; i < scenesToReload.Count; i++)
                    {
                        if (i == primaryIndex) continue;
                        EditorSceneManager.OpenScene(scenesToReload[i].assetPath, OpenSceneMode.Additive);
                    }
                }
                return $"Restored the prefab and {manifest.scenes.Count} scene backup(s) from the last Deploy.";
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return "Rollback failed. Review the Unity Console for details.";
            }
        }

        static void CreateDeployBackup(string prefabPath)
        {
            string backupDirectory = ToAbsolutePath(BackupFolder);
            if (Directory.Exists(backupDirectory)) Directory.Delete(backupDirectory, true);
            Directory.CreateDirectory(backupDirectory);

            var manifest = new DeployBackupManifest
            {
                prefabPath = prefabPath,
                prefabBackup = BackupFolder + "/prefab.backup"
            };
            CopyToBackup(prefabPath, manifest.prefabBackup);

            string folder = Path.GetDirectoryName(prefabPath).Replace('\\', '/');
            string overridePath = $"{folder}/{Path.GetFileNameWithoutExtension(prefabPath)}_Animations.overrideController";
            if (File.Exists(ToAbsolutePath(overridePath)))
            {
                manifest.overridePath = overridePath;
                manifest.overrideBackup = BackupFolder + "/animations.backup";
                CopyToBackup(overridePath, manifest.overrideBackup);
            }

            int sceneIndex = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || string.IsNullOrEmpty(scene.path) || !File.Exists(ToAbsolutePath(scene.path))) continue;
                var entry = new SceneBackup
                {
                    assetPath = scene.path,
                    backupPath = $"{BackupFolder}/scene_{sceneIndex++}.backup"
                };
                CopyToBackup(entry.assetPath, entry.backupPath);
                manifest.scenes.Add(entry);
            }
            File.WriteAllText(ToAbsolutePath(BackupManifestPath), JsonUtility.ToJson(manifest, true));
        }

        static void CopyToBackup(string assetPath, string backupPath)
        {
            File.Copy(ToAbsolutePath(assetPath), ToAbsolutePath(backupPath), true);
        }

        static void RestoreBackupFile(string backupPath, string assetPath)
        {
            if (!File.Exists(ToAbsolutePath(backupPath))) throw new FileNotFoundException("Backup file is missing.", backupPath);
            File.Copy(ToAbsolutePath(backupPath), ToAbsolutePath(assetPath), true);
        }

        static string ToAbsolutePath(string projectRelativePath)
        {
            return Path.GetFullPath(projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        static int UpdateDeployedInstances(GameObject savedPrefab, EnemyForgeDefinition d)
        {
            if (savedPrefab == null || d == null) return 0;
            string expectedName = savedPrefab.name;
            int updated = 0;
            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate == null || EditorUtility.IsPersistent(candidate) || !candidate.scene.IsValid()) continue;
                if (candidate.transform.parent != null) continue;
                string cleanName = candidate.name.EndsWith("(Clone)", StringComparison.Ordinal)
                    ? candidate.name.Substring(0, candidate.name.Length - 7).TrimEnd()
                    : candidate.name;
                var source = PrefabUtility.GetCorrespondingObjectFromSource(candidate);
                bool sameSourceName = source != null && source.name == expectedName;
                if (cleanName != expectedName && !sameSourceName) continue;
                candidate.name = expectedName;
                Configure(candidate, d, true);
                ConfigureDriverMode(candidate, d);
                var savedAnimator = savedPrefab.GetComponentInChildren<Animator>(true);
                var candidateAnimator = candidate.GetComponentInChildren<Animator>(true);
                if (savedAnimator != null && candidateAnimator != null)
                    candidateAnimator.runtimeAnimatorController = savedAnimator.runtimeAnimatorController;
                EditorSceneManager.MarkSceneDirty(candidate.scene);
                updated++;
            }
            if (updated > 0)
                Debug.Log($"[Enemy Forge] Deployed changes to {updated} matching world instance(s) named {expectedName}.");
            return updated;
        }

        static readonly string[] LegacyAiTypeNames = { "EnemyWanderAI", "EnemyAI" };

        static void ConfigureDriverMode(GameObject root, EnemyForgeDefinition d)
        {
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null || component is EnemyController) continue;
                string typeName = component.GetType().Name;
                if (Array.IndexOf(LegacyAiTypeNames, typeName) >= 0)
                {
                    component.enabled = false;
                    EditorUtility.SetDirty(component);
                    continue;
                }
                if (EnemyForgeValidator.IsAnimationDriver(typeName))
                {
                    bool keep = d.animationDriverMode != EnemyForgeAnimationDriverMode.EnemyForgeStandard;
                    component.enabled = keep;
                    EditorUtility.SetDirty(component);
                }
            }
        }

        static AnimationClip[] GetAttackClips(EnemyForgeDefinition d) =>
            new[] { d.attackAnimation, d.attackAnimation2, d.attackAnimation3, d.attackAnimation4 };

        static float[] GetAttackImpactPoints(EnemyForgeDefinition d) =>
            new[]
            {
                d.attackImpactPoint,
                d.attackImpactPoint2,
                d.attackImpactPoint3,
                d.attackImpactPoint4
            };

        static float[] GetAttackSpeeds(EnemyForgeDefinition d) =>
            new[]
            {
                d.attackAnimationSpeed,
                d.attackAnimationSpeed2,
                d.attackAnimationSpeed3,
                d.attackAnimationSpeed4
            };

        static float LongestClipLength(AnimationClip[] clips, float[] speeds)
        {
            float longest = 0f;
            for (int i = 0; i < clips.Length; i++)
                if (clips[i] != null)
                    longest = Mathf.Max(longest,
                        clips[i].length / Mathf.Max(0.25f, speeds[i]));
            return longest;
        }

        static int BuildAttackVariantMask(AnimationClip[] clips)
        {
            int mask = 1;
            for (int i = 1; i < clips.Length; i++)
                if (clips[i] != null)
                    mask |= 1 << i;
            return mask;
        }

        static float[] BuildAttackImpactDelays(AnimationClip[] clips, float[] normalizedImpacts,
            float[] speeds, float maximumDelay)
        {
            var delays = new float[4];
            float fallback = clips[0] != null
                ? Mathf.Clamp(clips[0].length * normalizedImpacts[0] /
                    Mathf.Max(0.25f, speeds[0]), 0f, maximumDelay)
                : Mathf.Min(0.35f, maximumDelay);
            for (int i = 0; i < delays.Length; i++)
                delays[i] = clips[i] != null
                    ? Mathf.Clamp(clips[i].length * normalizedImpacts[i] /
                        Mathf.Max(0.25f, speeds[i]), 0f, maximumDelay)
                    : fallback;
            return delays;
        }

        static void GenerateAnimatorOverride(GameObject root, EnemyForgeDefinition d, string prefabPath)
        {
            if (d.idleAnimation == null)
            {
                Debug.LogWarning("[Enemy Forge] Animator Override was not generated because no Idle clip is assigned.", root);
                return;
            }

            string folder = System.IO.Path.GetDirectoryName(prefabPath).Replace('\\', '/');
            var baseController = EnsureSharedBaseController();
            if (baseController == null) return;
            string overridePath = $"{folder}/{System.IO.Path.GetFileNameWithoutExtension(prefabPath)}_Animations.overrideController";
            var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
            if (overrideController == null)
            {
                overrideController = new AnimatorOverrideController(baseController);
                AssetDatabase.CreateAsset(overrideController, overridePath);
            }
            else
            {
                overrideController.runtimeAnimatorController = baseController;
            }

            const string baseClipFolder = "Assets/Game/Data/EnemyForge/BaseClips";
            string generatedClipFolder = folder + "/EnemyForgeClips";
            string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            AnimationClip idle = EnsurePrefabClip(d.idleAnimation, generatedClipFolder, prefabName, "Idle", true);
            AnimationClip chaseSource = d.chaseAnimation != null ? d.chaseAnimation : d.idleAnimation;
            AnimationClip chase = EnsurePrefabClip(chaseSource, generatedClipFolder, prefabName, "Chase", true);
            AnimationClip attack = EnsurePrefabClip(d.attackAnimation, generatedClipFolder, prefabName, "Attack", false);
            AnimationClip attack2 = EnsurePrefabClip(d.attackAnimation2, generatedClipFolder, prefabName, "Attack2", false);
            AnimationClip attack3 = EnsurePrefabClip(d.attackAnimation3, generatedClipFolder, prefabName, "Attack3", false);
            AnimationClip attack4 = EnsurePrefabClip(d.attackAnimation4, generatedClipFolder, prefabName, "Attack4", false);
            AnimationClip getHit = EnsurePrefabClip(d.getHitAnimation, generatedClipFolder, prefabName, "GetHit", false);
            AnimationClip death = EnsurePrefabClip(d.deathAnimation, generatedClipFolder, prefabName, "Death", false);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                OverridePair(baseClipFolder + "/EnemyForge_Idle.anim", idle),
                OverridePair(baseClipFolder + "/EnemyForge_Chase.anim", chase),
                OverridePair(baseClipFolder + "/EnemyForge_Attack.anim", attack),
                OverridePair(baseClipFolder + "/EnemyForge_Attack2.anim", attack2),
                OverridePair(baseClipFolder + "/EnemyForge_Attack3.anim", attack3),
                OverridePair(baseClipFolder + "/EnemyForge_Attack4.anim", attack4),
                OverridePair(baseClipFolder + "/EnemyForge_GetHit.anim", getHit),
                OverridePair(baseClipFolder + "/EnemyForge_Death.anim", death)
            };
            overrides.RemoveAll(pair => pair.Key == null);
            overrideController.ApplyOverrides(overrides);
            EditorUtility.SetDirty(overrideController);
            AssetDatabase.SaveAssetIfDirty(overrideController);
            var animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null) animator = Undo.AddComponent<Animator>(root);
            animator.runtimeAnimatorController = overrideController;
            animator.applyRootMotion = false;
            EditorUtility.SetDirty(animator);
        }

        static AnimationClip EnsurePrefabClip(AnimationClip source, string folder, string prefabName,
            string stateName, bool loop)
        {
            if (source == null) return null;
            EnsureFolder(folder);
            string path = $"{folder}/{SanitizeName(prefabName)}_{stateName}.anim";
            var local = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            string sourcePath = AssetDatabase.GetAssetPath(source);
            if (local == null)
            {
                local = UnityEngine.Object.Instantiate(source);
                local.name = $"{SanitizeName(prefabName)}_{stateName}";
                AssetDatabase.CreateAsset(local, path);
            }
            else if (!sourcePath.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.CopySerialized(source, local);
                local.name = $"{SanitizeName(prefabName)}_{stateName}";
            }

            var settings = AnimationUtility.GetAnimationClipSettings(local);
            settings.loopTime = loop;
            settings.loopBlend = loop;
            if (IsAttackState(stateName))
            {
                // Enemy prefabs remain NavMesh/server driven, so root motion is
                // disabled on their Animator. Preserve vertical hip movement,
                // but leave horizontal travel and global orientation extracted.
                // Baking horizontal travel while the controller turns toward a
                // moving target makes the rendered model orbit its prefab root.
                // Leave global orientation extracted so EnemyController's
                // per-frame target facing can steer long/combo attacks.
                settings.loopBlendOrientation = false;
                settings.loopBlendPositionY = true;
                settings.loopBlendPositionXZ = false;
            }
            if (stateName.Equals("Death", StringComparison.OrdinalIgnoreCase))
            {
                // Normalize the prefab-local death clip against its feet. Many
                // source FBXs preserve an elevated authored hip/root height,
                // making the death pose visibly begin above the ground.
                settings.keepOriginalPositionY = false;
                settings.heightFromFeet = true;
                settings.level = 0f;
            }
            AnimationUtility.SetAnimationClipSettings(local, settings);
            if (stateName.Equals("Chase", StringComparison.OrdinalIgnoreCase))
                NormalizeLocomotionRootTranslation(local);
            EditorUtility.SetDirty(local);
            AssetDatabase.SaveAssetIfDirty(local);
            return local;
        }

        static bool IsAttackState(string stateName)
        {
            return !string.IsNullOrEmpty(stateName) &&
                stateName.StartsWith("Attack", StringComparison.OrdinalIgnoreCase);
        }

        static void NormalizeExistingGeneratedClips()
        {
            bool changed = false;
            foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/Game" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                if (!path.Contains("/EnemyForgeClips/"))
                    continue;

                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) continue;

                if (path.EndsWith("_Chase.anim", StringComparison.OrdinalIgnoreCase) &&
                    NormalizeLocomotionRootTranslation(clip))
                    changed = true;

                string clipName = Path.GetFileNameWithoutExtension(path);
                int attackMarker = clipName.LastIndexOf("_Attack", StringComparison.OrdinalIgnoreCase);
                if (attackMarker >= 0 && BakeAttackRootIntoPose(clip))
                    changed = true;
            }

            if (changed)
                AssetDatabase.SaveAssets();
        }

        static bool BakeAttackRootIntoPose(AnimationClip clip)
        {
            if (clip == null) return false;

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (!settings.loopBlendOrientation &&
                settings.loopBlendPositionY &&
                !settings.loopBlendPositionXZ)
                return false;

            settings.loopBlendOrientation = false;
            settings.loopBlendPositionY = true;
            settings.loopBlendPositionXZ = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return true;
        }

        static bool NormalizeLocomotionRootTranslation(AnimationClip clip)
        {
            if (clip == null) return false;

            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            string rootPath = null;
            int rootDepth = int.MaxValue;
            foreach (EditorCurveBinding binding in bindings)
            {
                if (!binding.propertyName.StartsWith("m_LocalPosition.", StringComparison.Ordinal))
                    continue;

                string leafName = string.IsNullOrEmpty(binding.path)
                    ? string.Empty
                    : binding.path.Substring(binding.path.LastIndexOf('/') + 1);
                if (leafName.IndexOf("hips", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    rootPath = binding.path;
                    break;
                }

                int depth = string.IsNullOrEmpty(binding.path)
                    ? 0
                    : binding.path.Split('/').Length;
                if (depth >= rootDepth) continue;
                rootDepth = depth;
                rootPath = binding.path;
            }

            if (rootPath == null) return false;

            bool changed = false;
            foreach (EditorCurveBinding binding in bindings)
            {
                if (binding.path != rootPath ||
                    !binding.propertyName.StartsWith("m_LocalPosition.", StringComparison.Ordinal))
                    continue;

                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null || curve.length == 0) continue;
                Keyframe[] keys = curve.keys;
                float fixedValue = keys[0].value;
                bool curveChanged = false;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (Mathf.Abs(keys[i].value - fixedValue) > 0.0001f ||
                        Mathf.Abs(keys[i].inTangent) > 0.0001f ||
                        Mathf.Abs(keys[i].outTangent) > 0.0001f)
                        curveChanged = true;
                    keys[i].value = fixedValue;
                    keys[i].inTangent = 0f;
                    keys[i].outTangent = 0f;
                }

                if (!curveChanged) continue;
                curve.keys = keys;
                AnimationUtility.SetEditorCurve(clip, binding, curve);
                changed = true;
            }

            if (changed)
                EditorUtility.SetDirty(clip);
            return changed;
        }

        static KeyValuePair<AnimationClip, AnimationClip> OverridePair(string placeholderPath, AnimationClip replacement) =>
            new KeyValuePair<AnimationClip, AnimationClip>(
                AssetDatabase.LoadAssetAtPath<AnimationClip>(placeholderPath), replacement);

        static AnimatorController EnsureSharedBaseController()
        {
            const string dataFolder = "Assets/Game/Data/EnemyForge";
            const string clipFolder = dataFolder + "/BaseClips";
            const string controllerPath = dataFolder + "/EnemyForge_Base.controller";
            EnsureFolder(clipFolder);

            var idleClip = EnsurePlaceholderClip(clipFolder + "/EnemyForge_Idle.anim", "EnemyForge_Idle");
            var chaseClip = EnsurePlaceholderClip(clipFolder + "/EnemyForge_Chase.anim", "EnemyForge_Chase");
            var attackClip = EnsurePlaceholderClip(clipFolder + "/EnemyForge_Attack.anim", "EnemyForge_Attack");
            var attackClip2 = EnsurePlaceholderClip(clipFolder + "/EnemyForge_Attack2.anim", "EnemyForge_Attack2");
            var attackClip3 = EnsurePlaceholderClip(clipFolder + "/EnemyForge_Attack3.anim", "EnemyForge_Attack3");
            var attackClip4 = EnsurePlaceholderClip(clipFolder + "/EnemyForge_Attack4.anim", "EnemyForge_Attack4");
            var hitClip = EnsurePlaceholderClip(clipFolder + "/EnemyForge_GetHit.anim", "EnemyForge_GetHit");
            var deathClip = EnsurePlaceholderClip(clipFolder + "/EnemyForge_Death.anim", "EnemyForge_Death");

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.parameters = new[]
            {
                new AnimatorControllerParameter { name = "Speed", type = AnimatorControllerParameterType.Float },
                new AnimatorControllerParameter { name = "Attack", type = AnimatorControllerParameterType.Trigger },
                new AnimatorControllerParameter { name = "AttackVariant", type = AnimatorControllerParameterType.Int },
                new AnimatorControllerParameter { name = "GetHit", type = AnimatorControllerParameterType.Trigger },
                new AnimatorControllerParameter { name = "Death", type = AnimatorControllerParameterType.Trigger },
                FloatParameter("IdleSpeed"),
                FloatParameter("ChaseSpeed"),
                FloatParameter("Attack1Speed"),
                FloatParameter("Attack2Speed"),
                FloatParameter("Attack3Speed"),
                FloatParameter("Attack4Speed"),
                FloatParameter("GetHitSpeed"),
                FloatParameter("DeathSpeed")
            };
            var machine = controller.layers[0].stateMachine;
            foreach (var transition in machine.anyStateTransitions) machine.RemoveAnyStateTransition(transition);
            foreach (var child in machine.states) machine.RemoveState(child.state);
            var idle = AddAnimationState(machine, "Idle", idleClip, new Vector3(100f, 80f));
            var chase = AddAnimationState(machine, "Chase", chaseClip, new Vector3(350f, 80f));
            var attack = AddAnimationState(machine, "Attack", attackClip, new Vector3(350f, 220f));
            var attack2 = AddAnimationState(machine, "Attack 2", attackClip2, new Vector3(350f, 340f));
            var attack3 = AddAnimationState(machine, "Attack 3", attackClip3, new Vector3(520f, 340f));
            var attack4 = AddAnimationState(machine, "Attack 4", attackClip4, new Vector3(690f, 340f));
            var hit = AddAnimationState(machine, "GetHit", hitClip, new Vector3(600f, 80f));
            var death = AddAnimationState(machine, "Dead", deathClip, new Vector3(600f, 220f));
            SetSpeedParameter(idle, "IdleSpeed");
            SetSpeedParameter(chase, "ChaseSpeed");
            SetSpeedParameter(attack, "Attack1Speed");
            SetSpeedParameter(attack2, "Attack2Speed");
            SetSpeedParameter(attack3, "Attack3Speed");
            SetSpeedParameter(attack4, "Attack4Speed");
            SetSpeedParameter(hit, "GetHitSpeed");
            SetSpeedParameter(death, "DeathSpeed");
            machine.defaultState = idle;
            AddFloatTransition(idle, chase, "Speed", AnimatorConditionMode.Greater, 0.1f, 0.15f);
            AddFloatTransition(chase, idle, "Speed", AnimatorConditionMode.Less, 0.05f, 0.05f);
            AddAnyTrigger(machine, death, "Death", 0.08f);
            AddAnyTrigger(machine, hit, "GetHit", 0.08f);
            AddAnyAttackTrigger(machine, attack, 0, 0.08f);
            AddAnyAttackTrigger(machine, attack2, 1, 0.08f);
            AddAnyAttackTrigger(machine, attack3, 2, 0.08f);
            AddAnyAttackTrigger(machine, attack4, 3, 0.08f);
            AddExitLocomotionTransitions(attack, idle, chase, 0.9f, 0.12f);
            AddExitLocomotionTransitions(attack2, idle, chase, 0.9f, 0.12f);
            AddExitLocomotionTransitions(attack3, idle, chase, 0.9f, 0.12f);
            AddExitLocomotionTransitions(attack4, idle, chase, 0.9f, 0.12f);
            AddExitLocomotionTransitions(hit, idle, chase, 0.85f, 0.12f);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        static AnimationClip EnsurePlaceholderClip(string path, string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null) return clip;
            clip = new AnimationClip { name = name };
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        static AnimatorState AddAnimationState(AnimatorStateMachine machine, string name, AnimationClip clip, Vector3 position)
        {
            var state = machine.AddState(name, position);
            state.motion = clip;
            state.writeDefaultValues = false;
            return state;
        }

        static AnimatorControllerParameter FloatParameter(string name) =>
            new AnimatorControllerParameter
            {
                name = name,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1f
            };

        static void SetSpeedParameter(AnimatorState state, string parameter)
        {
            state.speedParameterActive = true;
            state.speedParameter = parameter;
        }

        static void AddFloatTransition(AnimatorState from, AnimatorState to, string parameter,
            AnimatorConditionMode mode, float threshold, float duration)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.hasFixedDuration = false;
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.AddCondition(mode, threshold, parameter);
        }

        static void AddAnyTrigger(AnimatorStateMachine machine, AnimatorState state, string parameter, float duration)
        {
            var transition = machine.AddAnyStateTransition(state);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.hasFixedDuration = false;
            transition.canTransitionToSelf = false;
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
        }

        static void AddAnyAttackTrigger(AnimatorStateMachine machine, AnimatorState state,
            int variant, float duration)
        {
            var transition = machine.AddAnyStateTransition(state);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.hasFixedDuration = false;
            transition.canTransitionToSelf = false;
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            transition.AddCondition(AnimatorConditionMode.Equals, variant, "AttackVariant");
        }

        static void AddExitLocomotionTransitions(AnimatorState from, AnimatorState idle, AnimatorState chase,
            float exitTime, float duration)
        {
            AddExitLocomotionTransition(from, idle, AnimatorConditionMode.Less, 0.1f, exitTime, duration);
            AddExitLocomotionTransition(from, chase, AnimatorConditionMode.Greater, 0.099f, exitTime, duration);
        }

        static void AddExitLocomotionTransition(AnimatorState from, AnimatorState to,
            AnimatorConditionMode mode, float threshold, float exitTime, float duration)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
            transition.hasFixedDuration = false;
            transition.interruptionSource = TransitionInterruptionSource.None;
            transition.AddCondition(mode, threshold, "Speed");
        }

        static T GetOrAdd<T>(GameObject root) where T : Component
        {
            var value = root.GetComponent<T>();
            return value != null ? value : Undo.AddComponent<T>(root);
        }

        static void EnsureCollider(GameObject root, EnemyForgeDefinition d)
        {
            // Combat code targets the Enemy/Player-tagged Health root. A collider
            // somewhere under an imported model is not sufficient: it may be an
            // untagged weapon/accessory collider and direct attacks will miss Health.
            foreach (var existing in root.GetComponents<Collider>())
            {
                if (existing == null || existing.isTrigger) continue;
                existing.enabled = true;
                EditorUtility.SetDirty(existing);
                return;
            }

            var collider = Undo.AddComponent<CapsuleCollider>(root);
            collider.radius = d.agentRadius;
            collider.height = Mathf.Max(d.agentHeight, d.agentRadius * 2f);
            collider.center = Vector3.up * collider.height * 0.5f;
            collider.isTrigger = false;
            collider.enabled = true;
        }

        static void TrySetRootTag(GameObject root, EnemyForgeRootTag selectedTag)
        {
            string tagName = selectedTag switch
            {
                EnemyForgeRootTag.Player => "Player",
                EnemyForgeRootTag.NPC => "NPC",
                _ => "Enemy"
            };
            try { root.tag = tagName; }
            catch (UnityException)
            {
                Debug.LogWarning($"[Enemy Forge] The {tagName} tag is not defined; prefab remains {root.tag}.", root);
            }
        }

        static string SanitizeName(string value)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value;
        }

        static string BuildForgedPrefabName(string sourceName, bool isNpc)
        {
            string name = SanitizeName(sourceName ?? string.Empty).Trim();
            if (name.EndsWith("_Enemy", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - "_Enemy".Length);
            if (name.EndsWith("_NPC", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - "_NPC".Length);

            if (name.StartsWith("forged_", StringComparison.OrdinalIgnoreCase))
                name = name.Substring("forged_".Length);
            else
            {
                int prefixEnd = name.IndexOf('_');
                if (prefixEnd >= 0 && prefixEnd < name.Length - 1)
                    name = name.Substring(prefixEnd + 1);
            }

            if (string.IsNullOrWhiteSpace(name))
                name = isNpc ? "npc" : "enemy";
            return "forged_" + name + (isNpc ? "_NPC" : "_Enemy");
        }

        static void EnsureFolder(string path)
        {
            var parts = path.Replace('\\', '/').TrimEnd('/').Split('/');
            if (parts.Length == 0 || parts[0] != "Assets") throw new ArgumentException("Output must be inside Assets/.");
            var current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
