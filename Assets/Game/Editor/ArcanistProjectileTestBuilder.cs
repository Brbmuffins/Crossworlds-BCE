#if UNITY_EDITOR
using System;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Builds and wires the reusable Arcanist Fireball projectile test.</summary>
public static class ArcanistProjectileTestBuilder
{
    const string ProjectilePath = "Assets/Game/Prefabs/Player/Player_FireballProjectile.prefab";
    const string ArcanistPath = "Assets/Game/Game_Prefabs/Arcanist.prefab";
    const string LoginScenePath = "Assets/Game/Scenes/LoginScene.unity";
    const string FireballVfxPath = "Assets/Game/FX/dark magic/Fireball.prefab";

    [MenuItem("BCE/Heroes/Build Arcanist Projectile Test", priority = 45)]
    public static void Build()
    {
        EnsureFolder("Assets/Game/Prefabs/Player");
        GameObject projectile = BuildProjectilePrefab();
        int spellIndex = WireArcanist(projectile);
        RegisterNetworkPrefab(projectile);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[BCE] Arcanist Fireball projectile ready at spellbook index {spellIndex}, equipped in slot 1.");
    }

    static GameObject BuildProjectilePrefab()
    {
        var root = new GameObject("Player_FireballProjectile");
        try
        {
            root.AddComponent<NetworkIdentity>();
            var sync = root.AddComponent<NetworkTransformUnreliable>();
            sync.target = root.transform;
            sync.syncDirection = SyncDirection.ServerToClient;
            sync.syncPosition = true;
            sync.syncRotation = true;
            sync.syncScale = false;

            var hitbox = root.AddComponent<SphereCollider>();
            hitbox.radius = 0.3f;
            hitbox.isTrigger = true;

            var body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            var projectile = root.AddComponent<PlayerProjectile>();
            projectile.speed = 18f;
            projectile.lifetime = 3f;
            projectile.maxRange = 12f;

            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FireballVfxPath);
            if (visualPrefab != null)
            {
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab, root.transform);
                visual.name = "Fireball Visual";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
            }
            else
            {
                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Fallback Fireball Visual";
                visual.transform.SetParent(root.transform, false);
                visual.transform.localScale = Vector3.one * 0.45f;
                UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            }

            return PrefabUtility.SaveAsPrefabAsset(root, ProjectilePath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    static int WireArcanist(GameObject projectilePrefab)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(ArcanistPath);
        try
        {
            var caster = root.GetComponentInChildren<ArcanistAbilityCaster>(true);
            if (caster == null)
                throw new InvalidOperationException("Arcanist prefab has no ArcanistAbilityCaster.");

            caster.spellbook ??= Array.Empty<AbilityDef>();
            int index = Array.FindIndex(caster.spellbook,
                ability => ability != null && ability.abilityName == "Fireball");
            if (index < 0)
            {
                index = caster.spellbook.Length;
                Array.Resize(ref caster.spellbook, index + 1);
                caster.spellbook[index] = new AbilityDef();
            }

            AbilityDef fireball = caster.spellbook[index];
            fireball.abilityName = "Fireball";
            fireball.description = "Launch a fireball that damages the first enemy it hits.";
            fireball.shape = AbilityShape.Cone;
            fireball.category = AbilityCategory.Damage;
            fireball.range = 12f;
            fireball.coneAngle = 8f;
            fireball.useFixedRange = true;
            fireball.cooldown = 2f;
            fireball.castTime = 0.25f;
            fireball.damage = 25f;
            fireball.maxChargeDamage = 25f;
            fireball.chargeable = false;
            fireball.targetTag = "Enemy";
            fireball.launchProjectile = true;
            fireball.projectilePrefab = projectilePrefab;
            fireball.projectileSpeed = 18f;
            fireball.combustionPoints = 10;
            fireball.spendCombustion = false;
            fireball.variants = Array.Empty<AbilityVariant>();

            if (caster.equippedIndices == null || caster.equippedIndices.Length != 4)
                caster.equippedIndices = new[] { index, 1, 2, 3 };
            else
                caster.equippedIndices[0] = index;

            if (caster.classPool != null)
            {
                caster.classPool.availableIndices ??= Array.Empty<int>();
                if (!caster.classPool.availableIndices.Contains(index))
                {
                    int oldLength = caster.classPool.availableIndices.Length;
                    Array.Resize(ref caster.classPool.availableIndices, oldLength + 1);
                    caster.classPool.availableIndices[oldLength] = index;
                }
                if (caster.classPool.defaultEquipped == null || caster.classPool.defaultEquipped.Length != 4)
                    caster.classPool.defaultEquipped = new[] { index, 1, 2, 3 };
                else
                    caster.classPool.defaultEquipped[0] = index;
                EditorUtility.SetDirty(caster.classPool);
            }

            EditorUtility.SetDirty(caster);
            PrefabUtility.SaveAsPrefabAsset(root, ArcanistPath);
            return index;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void RegisterNetworkPrefab(GameObject projectilePrefab)
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

            if (!manager.spawnPrefabs.Contains(projectilePrefab))
                manager.spawnPrefabs.Add(projectilePrefab);
            GameObject[] worldPrefabs = manager.worldPrefabs ?? Array.Empty<GameObject>();
            if (!worldPrefabs.Contains(projectilePrefab))
            {
                Array.Resize(ref worldPrefabs, worldPrefabs.Length + 1);
                worldPrefabs[worldPrefabs.Length - 1] = projectilePrefab;
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
#endif
