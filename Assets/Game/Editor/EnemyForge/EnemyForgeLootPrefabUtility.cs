#if UNITY_EDITOR
using System;
using Mirror;
using UnityEditor;
using UnityEngine;

namespace Crossworlds.EditorTools.EnemyForge
{
    internal static class EnemyForgeLootPrefabUtility
    {
        const string GeneratedFolder =
            "Assets/Game/Resources/EnemyForge/Loot";
        const string ChaosWeaverPath =
            "Assets/Game/3D Models/Ashen Wasteland/aw_enemies/model_aw_chaos_weaver/Prefab/forged_aw_chaos_weaver_Enemy.prefab";
        const string OrnateBagPath =
            "Assets/Game/3D Models/Loot/loot_bag/prebab_loot_bag_ornate.prefab";

        [InitializeOnLoadMethod]
        static void ScheduleChaosWeaverRepair()
        {
            EditorApplication.delayCall += RepairChaosWeaverLootAssignment;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += RepairChaosWeaverLootAssignment;
        }

        public static GameObject ResolveNetworkReadyPickup(GameObject selected)
        {
            if (selected == null)
                return null;

            if (selected.GetComponent<WorldItem>() != null &&
                selected.GetComponent<NetworkIdentity>() != null)
                return selected;

            string sourcePath = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(sourcePath) ||
                !PrefabUtility.IsPartOfPrefabAsset(selected))
            {
                Debug.LogError(
                    $"[Enemy Forge] Loot visual '{selected.name}' must be a prefab asset. " +
                    "Drag the prefab from the Project window, not a scene instance.",
                    selected);
                return null;
            }

            EnsureFolder(GeneratedFolder);
            string sourceGuid = AssetDatabase.AssetPathToGUID(sourcePath);
            string safeName = SanitizeName(selected.name);
            string suffix = sourceGuid.Length >= 8
                ? sourceGuid.Substring(0, 8)
                : sourceGuid;
            string generatedPath =
                $"{GeneratedFolder}/WorldItem_{safeName}_{suffix}.prefab";

            GameObject existing =
                AssetDatabase.LoadAssetAtPath<GameObject>(generatedPath);
            if (existing != null &&
                existing.GetComponent<WorldItem>() != null &&
                existing.GetComponent<NetworkIdentity>() != null)
                return existing;

            var root = new GameObject($"WorldItem_{safeName}");
            try
            {
                root.AddComponent<NetworkIdentity>();
                root.AddComponent<WorldItem>();

                GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(selected);
                visual.name = "Visual";
                visual.transform.SetParent(root.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;

                // Pickup visuals must never become physical navigation blockers.
                foreach (Collider visualCollider in
                         visual.GetComponentsInChildren<Collider>(true))
                    visualCollider.enabled = false;

                var trigger = root.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                ConfigureTriggerFromVisual(root.transform, visual, trigger);

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
                    root, generatedPath);
                AssetDatabase.ImportAsset(
                    generatedPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();
                Debug.Log(
                    $"[Enemy Forge] Created network-ready loot pickup '{generatedPath}' " +
                    $"from visual '{sourcePath}'.",
                    prefab);
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        static void ConfigureTriggerFromVisual(
            Transform root, GameObject visual, SphereCollider trigger)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                trigger.center = Vector3.up * 0.35f;
                trigger.radius = 0.8f;
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            trigger.center = root.InverseTransformPoint(bounds.center);
            trigger.radius = Mathf.Clamp(
                Mathf.Max(bounds.extents.x, bounds.extents.z) + 0.35f,
                0.65f,
                2.5f);
        }

        static void RepairChaosWeaverLootAssignment()
        {
            GameObject chaosWeaver =
                AssetDatabase.LoadAssetAtPath<GameObject>(ChaosWeaverPath);
            GameObject ornateBag =
                AssetDatabase.LoadAssetAtPath<GameObject>(OrnateBagPath);
            if (chaosWeaver == null || ornateBag == null)
                return;

            EnemyController controller =
                chaosWeaver.GetComponent<EnemyController>();
            if (controller == null || controller.dropTable == null)
                return;

            GameObject pickup = ResolveNetworkReadyPickup(ornateBag);
            if (pickup == null || controller.worldItemPrefab == pickup)
                return;

            using (var scope =
                   new PrefabUtility.EditPrefabContentsScope(ChaosWeaverPath))
            {
                EnemyController editable =
                    scope.prefabContentsRoot.GetComponent<EnemyController>();
                if (editable != null)
                    editable.worldItemPrefab = pickup;
            }

            AssetDatabase.ImportAsset(
                ChaosWeaverPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            Debug.Log(
                "[Enemy Forge] Repaired the Ashen Chaos Weaver loot pickup " +
                $"assignment with '{pickup.name}'.",
                pickup);
        }

        static string SanitizeName(string value)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                value = value.Replace(c, '_');
            return value.Trim();
        }

        static void EnsureFolder(string path)
        {
            string[] parts = path.Replace('\\', '/').TrimEnd('/').Split('/');
            string current = "Assets";
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
