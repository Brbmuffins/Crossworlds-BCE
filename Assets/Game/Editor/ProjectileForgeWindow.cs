#if UNITY_EDITOR
using System;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crossworlds.EditorTools
{
    /// <summary>Wraps a visual-only VFX prefab in a reusable Mirror player projectile.</summary>
    public sealed class ProjectileForgeWindow : EditorWindow
    {
        const string OutputFolder = "Assets/Game/Prefabs/Player/Projectiles";
        const string LoginScenePath = "Assets/Game/Scenes/LoginScene.unity";

        GameObject travellingVFX;
        GameObject impactVFX;
        string projectileName = "New Projectile";
        float speed = 18f;
        float lifetime = 3f;
        float maxRange = 12f;
        float colliderRadius = 0.3f;
        Action<GameObject> createdCallback;

        [MenuItem("BCE/Spell Forge/Projectile Builder", priority = 40)]
        public static void Open()
        {
            OpenWithVisual(SpellVFXBrowserWindow.SpellForgeSelection);
        }

        public static void OpenWithVisual(GameObject visual)
        {
            OpenWithVisual(visual, null);
        }

        public static void OpenWithVisual(
            GameObject visual,
            Action<GameObject> onCreated)
        {
            var window = GetWindow<ProjectileForgeWindow>(true, "Projectile Builder");
            window.minSize = new Vector2(410f, 360f);
            window.createdCallback = onCreated;
            if (visual != null)
            {
                window.travellingVFX = visual;
                window.projectileName = visual.name + " Projectile";
            }
            window.Show();
            window.Focus();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("PROJECTILE PREFAB BUILDER", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Choose a travelling VFX prefab. This tool wraps it in the networking, movement, collision, and damage components required by Spell Forge.",
                MessageType.Info);

            projectileName = EditorGUILayout.TextField("Projectile Name", projectileName);
            travellingVFX = (GameObject)EditorGUILayout.ObjectField(
                "Travelling VFX", travellingVFX, typeof(GameObject), false);
            impactVFX = (GameObject)EditorGUILayout.ObjectField(
                "Impact VFX (Optional)", impactVFX, typeof(GameObject), false);

            EditorGUILayout.Space(5f);
            speed = EditorGUILayout.FloatField("Default Speed", speed);
            lifetime = EditorGUILayout.FloatField("Lifetime", lifetime);
            maxRange = EditorGUILayout.FloatField("Maximum Range", maxRange);
            colliderRadius = EditorGUILayout.FloatField("Collider Radius", colliderRadius);

            EditorGUILayout.Space(8f);
            using (new EditorGUI.DisabledScope(
                travellingVFX == null || string.IsNullOrWhiteSpace(projectileName)))
            {
                if (GUILayout.Button("Create Networked Projectile Prefab", GUILayout.Height(34f)))
                    CreateProjectile();
            }

            EditorGUILayout.HelpBox(
                createdCallback != null
                    ? "The finished projectile will be assigned to the current spell automatically."
                    : "After creation, assign the selected prefab to Projectile VFX Prefab in the spell's VFX tab.",
                MessageType.None);
        }

        void CreateProjectile()
        {
            EnsureFolder(OutputFolder);
            string safeName = SanitizeName(projectileName);
            string path = AssetDatabase.GenerateUniqueAssetPath(
                $"{OutputFolder}/{safeName}.prefab");

            var root = new GameObject(safeName);
            GameObject prefab = null;
            try
            {
                root.AddComponent<NetworkIdentity>();
                var sync = root.AddComponent<NetworkTransformUnreliable>();
                sync.target = root.transform;
                sync.syncDirection = SyncDirection.ServerToClient;
                sync.syncPosition = true;
                sync.syncRotation = true;
                sync.syncScale = false;

                var collider = root.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.radius = Mathf.Max(0.05f, colliderRadius);

                var body = root.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                var projectile = root.AddComponent<PlayerProjectile>();
                projectile.speed = Mathf.Max(0.1f, speed);
                projectile.lifetime = Mathf.Max(0.1f, lifetime);
                projectile.maxRange = Mathf.Max(0.1f, maxRange);
                projectile.impactVFX = impactVFX;

                var visual = (GameObject)PrefabUtility.InstantiatePrefab(
                    travellingVFX, root.transform);
                visual.name = "Travelling VFX";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;

                prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                DestroyImmediate(root);
            }

            // The temporary root must be destroyed before LoginScene is saved;
            // otherwise Unity serializes that construction object into the scene.
            RegisterWithNetworkManager(prefab);
            AssetDatabase.SaveAssets();
            SpellVFXBrowserWindow.SetExternalSpellForgeSelection(prefab);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            createdCallback?.Invoke(prefab);
            createdCallback = null;
            Debug.Log($"[Projectile Forge] Created and registered {path}", prefab);
            ShowNotification(new GUIContent("Projectile prefab created"));
        }

        static void RegisterWithNetworkManager(GameObject prefab)
        {
            Scene scene = SceneManager.GetSceneByPath(LoginScenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere)
                scene = EditorSceneManager.OpenScene(LoginScenePath, OpenSceneMode.Additive);

            try
            {
                RodNetworkManager manager = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<RodNetworkManager>(true))
                    .FirstOrDefault();
                if (manager == null)
                    throw new InvalidOperationException("LoginScene has no RodNetworkManager.");

                if (!manager.spawnPrefabs.Contains(prefab))
                    manager.spawnPrefabs.Add(prefab);

                GameObject[] worldPrefabs = manager.worldPrefabs ?? Array.Empty<GameObject>();
                if (!worldPrefabs.Contains(prefab))
                {
                    Array.Resize(ref worldPrefabs, worldPrefabs.Length + 1);
                    worldPrefabs[worldPrefabs.Length - 1] = prefab;
                    manager.worldPrefabs = worldPrefabs;
                }

                EditorUtility.SetDirty(manager);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        static string SanitizeName(string value)
        {
            char[] invalid = System.IO.Path.GetInvalidFileNameChars();
            string cleaned = new string(value
                .Where(c => !invalid.Contains(c))
                .ToArray())
                .Trim()
                .Replace(' ', '_');
            return string.IsNullOrEmpty(cleaned) ? "Player_Projectile" : cleaned;
        }

        static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
