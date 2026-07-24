#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Crossworlds.EditorTools.EnemyForge
{
    [InitializeOnLoad]
    internal static class EnemyForgeDeployment
    {
        const string LoginScenePath = "Assets/Game/Scenes/LoginScene.unity";

        static EnemyForgeDeployment()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            bool hasNetworkEnemy = false;
            bool hasNetworkManager = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (!hasNetworkEnemy && root.GetComponentInChildren<EnemyController>(true) != null &&
                        root.GetComponentInChildren<NetworkIdentity>(true) != null)
                        hasNetworkEnemy = true;
                    if (!hasNetworkManager && root.GetComponentInChildren<RodNetworkManager>(true) != null)
                        hasNetworkManager = true;
                }
            }
            if (hasNetworkEnemy && !hasNetworkManager)
            {
                EditorUtility.DisplayDialog("Network Manager Required",
                    "This scene contains server-authoritative enemies but no RodNetworkManager. Start through LoginScene and enter this scene through the normal network flow. Also save the scene after placing new enemies.",
                    "OK");
            }
        }

        public static List<EnemyForgeIssue> ValidateReadiness(GameObject prefab)
        {
            var result = new List<EnemyForgeIssue>();
            if (prefab == null)
            {
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Build or select a forged enemy prefab first."));
                return result;
            }
            if (!PrefabUtility.IsPartOfPrefabAsset(prefab))
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "The deployment target must be a saved prefab asset."));
            if (prefab.GetComponent<NetworkIdentity>() == null)
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "NetworkIdentity is missing."));
            if (prefab.GetComponent<EnemyController>() == null)
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "EnemyController is missing."));
            if (prefab.GetComponent<Health>() == null)
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Health is missing."));
            if (prefab.GetComponent<NavMeshAgent>() == null)
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "NavMeshAgent is missing."));

            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path))
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "The prefab is not stored in the Asset database."));
            if (prefab.transform.position != Vector3.zero || prefab.transform.rotation != Quaternion.identity)
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "The prefab root is not at zero position/rotation. Rebuild it before placement."));

            if (!result.Any(x => x.severity == EnemyForgeSeverity.Error))
                result.Add(new EnemyForgeIssue(EnemyForgeSeverity.Info, "Prefab is ready for scene placement and network registration."));
            return result;
        }

        public static GameObject PlaceInCurrentScene(GameObject prefab)
        {
            if (!CanDeploy(prefab)) return null;
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("No Active Scene", "Open the scene where the enemy should be tested first.", "OK");
                return null;
            }

            Vector3 requested = Selection.activeTransform != null && Selection.activeTransform.gameObject.scene == scene
                ? Selection.activeTransform.position
                : SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
            Vector3 placement = requested;
            if (NavMesh.SamplePosition(requested, out NavMeshHit hit, 25f, NavMesh.AllAreas))
                placement = hit.position;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Place forged enemy");
            instance.transform.SetPositionAndRotation(placement, Quaternion.identity);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = instance;
            SceneView.lastActiveSceneView?.FrameSelected();

            if (!NavMesh.SamplePosition(placement, out _, 1f, NavMesh.AllAreas))
                Debug.LogWarning("[Enemy Forge] Enemy was placed, but no baked NavMesh was found nearby. AI movement will not work until the NavMesh is baked.", instance);
            EditorUtility.DisplayDialog("Network Enemy Placed",
                "The network-ready enemy was placed and the scene is marked as changed. Save the scene, then test through LoginScene using the normal server flow.", "OK");
            return instance;
        }

        public static GameObject ReplaceSelected(GameObject prefab)
        {
            if (!CanDeploy(prefab)) return null;
            var old = Selection.activeGameObject;
            if (old == null || EditorUtility.IsPersistent(old) || !old.scene.IsValid())
            {
                EditorUtility.DisplayDialog("Select a Scene Object", "Select the scene object that should be replaced.", "OK");
                return null;
            }
            if (!EditorUtility.DisplayDialog("Replace Selected Object?",
                $"Replace '{old.name}' with '{prefab.name}' while preserving its parent and transform?\n\nThis action supports Undo.",
                "Replace", "Cancel")) return null;

            Transform parent = old.transform.parent;
            int sibling = old.transform.GetSiblingIndex();
            Vector3 position = old.transform.position;
            Quaternion rotation = old.transform.rotation;
            Vector3 scale = old.transform.localScale;
            Scene scene = old.scene;
            var replacement = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            Undo.RegisterCreatedObjectUndo(replacement, "Replace with forged enemy");
            replacement.transform.SetParent(parent, true);
            replacement.transform.SetPositionAndRotation(position, rotation);
            replacement.transform.localScale = scale;
            replacement.transform.SetSiblingIndex(sibling);
            Undo.DestroyObjectImmediate(old);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = replacement;
            return replacement;
        }

        public static bool RegisterForNetworkSpawning(GameObject prefab)
        {
            if (!CanDeploy(prefab)) return false;

            if (!EnsureNetworkComponents(prefab)) return false;

            Scene login = SceneManager.GetSceneByPath(LoginScenePath);
            bool openedHere = !login.IsValid() || !login.isLoaded;
            if (openedHere)
                login = EditorSceneManager.OpenScene(LoginScenePath, OpenSceneMode.Additive);
            if (!login.IsValid() || !login.isLoaded)
            {
                EditorUtility.DisplayDialog("Registration Failed", "LoginScene could not be opened.", "OK");
                return false;
            }

            var manager = login.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<RodNetworkManager>(true))
                .FirstOrDefault();
            if (manager == null)
            {
                if (openedHere) EditorSceneManager.CloseScene(login, true);
                EditorUtility.DisplayDialog("Registration Failed", "RodNetworkManager was not found in LoginScene.", "OK");
                return false;
            }

            Undo.RecordObject(manager, "Register forged enemy for network spawning");
            var world = new List<GameObject>(manager.worldPrefabs ?? Array.Empty<GameObject>());
            bool changed = false;
            var dependencies = new List<GameObject> { prefab };
            var enemyController = prefab.GetComponent<EnemyController>();
            if (enemyController != null && enemyController.projectilePrefab != null)
                dependencies.Add(enemyController.projectilePrefab);

            foreach (GameObject dependency in dependencies)
            {
                if (dependency == null) continue;
                if (!world.Contains(dependency)) { world.Add(dependency); changed = true; }
                if (!manager.spawnPrefabs.Contains(dependency)) { manager.spawnPrefabs.Add(dependency); changed = true; }
            }
            manager.worldPrefabs = world.ToArray();
            EditorUtility.SetDirty(manager);
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(login);
                EditorSceneManager.SaveScene(login);
                AssetDatabase.SaveAssets();
            }
            if (openedHere) EditorSceneManager.CloseScene(login, true);
            EditorUtility.DisplayDialog("Network Registration", changed
                ? "The enemy and its projectile dependencies are now registered for Mirror runtime spawning."
                : "The enemy and its projectile dependencies were already registered; no changes were needed.", "OK");
            return true;
        }

        static bool EnsureNetworkComponents(GameObject prefab)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) return false;
            try
            {
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    GameObject root = scope.prefabContentsRoot;
                    var identity = root.GetComponent<NetworkIdentity>();
                    if (identity == null) identity = root.AddComponent<NetworkIdentity>();

                    var transformSync = root.GetComponent<NetworkTransformBase>();
                    if (transformSync == null) transformSync = root.AddComponent<NetworkTransformUnreliable>();
                    if (transformSync.target == null) transformSync.target = root.transform;
                    transformSync.syncDirection = SyncDirection.ServerToClient;
                    transformSync.syncPosition = true;
                    transformSync.syncRotation = true;
                    transformSync.syncScale = false;

                    var animator = root.GetComponentInChildren<Animator>(true);
                    if (animator != null)
                    {
                        var animatorSync = root.GetComponent<NetworkAnimator>();
                        if (animatorSync == null) animatorSync = root.AddComponent<NetworkAnimator>();
                        animatorSync.animator = animator;
                        animatorSync.clientAuthority = false;
                        EditorUtility.SetDirty(animatorSync);
                    }
                    else
                    {
                        Debug.LogWarning("[Enemy Forge] No Animator was found, so NetworkAnimator was not added. Transform and combat state will still network correctly.", prefab);
                    }

                    EditorUtility.SetDirty(identity);
                    EditorUtility.SetDirty(transformSync);
                }
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Network Deployment Failed", "The prefab could not be updated:\n" + ex.Message, "OK");
                return false;
            }
        }

        public static bool IsNetworkRegistered(GameObject prefab)
        {
            if (prefab == null) return false;
            string guid = AssetDatabase.AssetPathToGUID(LoginScenePath);
            if (string.IsNullOrEmpty(guid)) return false;
            string prefabGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
            if (string.IsNullOrEmpty(prefabGuid)) return false;
            string absolute = System.IO.Path.GetFullPath(LoginScenePath);
            return System.IO.File.Exists(absolute) && System.IO.File.ReadAllText(absolute).Contains("guid: " + prefabGuid);
        }

        static bool CanDeploy(GameObject prefab)
        {
            var issues = ValidateReadiness(prefab);
            if (issues.Any(x => x.severity == EnemyForgeSeverity.Error))
            {
                var error = issues.First(x => x.severity == EnemyForgeSeverity.Error);
                EditorUtility.DisplayDialog("Enemy Not Ready", error.message, "OK");
                return false;
            }
            return true;
        }
    }
}
#endif
