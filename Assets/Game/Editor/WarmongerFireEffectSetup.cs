#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

/// <summary>Applies the authored Warmonger FireSocket and installs its looping fire effect.</summary>
internal static class WarmongerFireEffectSetup
{
    const string WeaponPrefabPath =
        "Assets/Game/3D Models/Weapons/Axes/main_hand/warmonger/prefab_warmonger.prefab";
    const string FlamePrefabPath =
        "Assets/Game/FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/MediumFlames.prefab";
    const string EffectName = "Warmonger_Fire";

    [InitializeOnLoadMethod]
    static void ScheduleSetup() => EditorApplication.delayCall += Apply;

    [MenuItem("BCE/Setup/Apply Warmonger Fire Effect")]
    public static void Apply()
    {
        EditorApplication.delayCall -= Apply;
        GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WeaponPrefabPath);
        GameObject flamePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FlamePrefabPath);
        if (weaponPrefab == null || flamePrefab == null) return;

        ApplySceneSocketOverride();

        GameObject root = PrefabUtility.LoadPrefabContents(WeaponPrefabPath);
        try
        {
            Transform socket = FindChild(root.transform, "FireSocket");
            if (socket == null)
            {
                // Unity can serialize an added child into a scene without exposing
                // it as an applicable override after a script reload. Recover the
                // exact authored scene transform directly into the weapon prefab.
                var socketObject = new GameObject("FireSocket");
                socket = socketObject.transform;
                socket.SetParent(root.transform, false);
                socket.localPosition = new Vector3(-0.036f, 0.615f, 0.028f);
                socket.localRotation = Quaternion.identity;
                socket.localScale = Vector3.one;
            }

            Transform existing = socket.Find(EffectName);
            if (existing == null)
            {
                var effect = (GameObject)PrefabUtility.InstantiatePrefab(flamePrefab, socket);
                effect.name = EffectName;
                existing = effect.transform;
            }

            existing.localPosition = Vector3.zero;
            existing.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            existing.localScale = Vector3.one * 0.12f;

            foreach (ParticleSystem particles in existing.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particles.main;
                main.loop = true;
                main.prewarm = true;
                main.playOnAwake = true;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }

            Transform lightTransform = existing.Find("FireLight");
            Light fireLight;
            if (lightTransform == null)
            {
                var lightObject = new GameObject("FireLight");
                lightTransform = lightObject.transform;
                lightTransform.SetParent(existing, false);
                fireLight = lightObject.AddComponent<Light>();
            }
            else
            {
                fireLight = lightTransform.GetComponent<Light>() ??
                            lightTransform.gameObject.AddComponent<Light>();
            }

            lightTransform.localPosition = new Vector3(0f, 0f, 0.35f);
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.32f, 0.06f);
            fireLight.intensity = 1.15f;
            fireLight.range = 2.25f;
            fireLight.shadows = LightShadows.None;

            PrefabUtility.SaveAsPrefabAsset(root, WeaponPrefabPath);
            RemoveDuplicateSceneSocketOverrides();
            RegenerateInventoryIcon();
            Debug.Log("[WARMONGER] Applied looping flame and fire light to FireSocket.", weaponPrefab);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void ApplySceneSocketOverride()
    {
        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform == null || transform.name != "FireSocket" ||
                !transform.gameObject.scene.IsValid() ||
                !PrefabUtility.IsAddedGameObjectOverride(transform.gameObject))
                continue;

            string sourcePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                transform.gameObject);
            if (!string.Equals(sourcePath, WeaponPrefabPath, StringComparison.OrdinalIgnoreCase))
                continue;

            PrefabUtility.ApplyAddedGameObject(
                transform.gameObject, WeaponPrefabPath, InteractionMode.AutomatedAction);
            return;
        }
    }

    static void RemoveDuplicateSceneSocketOverrides()
    {
        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform == null || transform.name != "FireSocket" ||
                !transform.gameObject.scene.IsValid() ||
                !PrefabUtility.IsAddedGameObjectOverride(transform.gameObject))
                continue;

            string sourcePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                transform.gameObject);
            if (!string.Equals(sourcePath, WeaponPrefabPath, StringComparison.OrdinalIgnoreCase))
                continue;

            PrefabUtility.RevertAddedGameObject(
                transform.gameObject, InteractionMode.AutomatedAction);
        }
    }

    static Transform FindChild(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == childName) return child;
        return null;
    }

    static void RegenerateInventoryIcon()
    {
        GameObject weapon = AssetDatabase.LoadAssetAtPath<GameObject>(WeaponPrefabPath);
        if (weapon == null) return;

        var temporaryDefinition = ScriptableObject.CreateInstance<LootItemDefinition>();
        try
        {
            temporaryDefinition.itemId = "warmonger";
            temporaryDefinition.displayName = "Warmonger";
            temporaryDefinition.equippedVisualPrefab = weapon;
            // Warmonger's imported mesh is one-sided. This is its known-visible
            // face; use extra framing instead of rotating toward the culled side.
            temporaryDefinition.inventoryIconEulerAngles = new Vector3(12f, -28f, -8f);
            temporaryDefinition.inventoryIconZoom = 1.8f;
            Crossworlds.EditorTools.LootForge.LootForgeIconRenderer.Render(
                temporaryDefinition, out string error);
            if (!string.IsNullOrWhiteSpace(error))
                Debug.LogWarning("[WARMONGER] Inventory icon regeneration failed: " + error);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(temporaryDefinition);
        }
    }
}
#endif
