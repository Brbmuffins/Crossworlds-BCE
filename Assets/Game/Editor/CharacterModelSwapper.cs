using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/CharacterSelect - Swap Class Prefab Models
///
/// Replaces the placeholder Engineer "Model" child inside each class prefab with
/// that class's own model, so the in-game spawn matches the character-select preview.
///
/// For each class it:
///   1. Forces the model FBX to Humanoid + auto-creates an avatar.
///   2. Opens the class prefab, removes the old "Model" child, instantiates the new
///      model as "Model", resets its transform.
///   3. Auto-scales the model to ~1.8u tall (Tripo raws aren't normalized) and grounds it.
///   4. Wires the Model's Animator: controller, avatar = the new model's avatar,
///      root motion off.
///
/// Warden is intentionally skipped — its model already IS the Engineer model.
/// PlayerMovement finds the Animator via GetComponentInChildren at runtime, so no
/// serialized references need fixing.
/// </summary>
public static class CharacterModelSwapper
{
    const float TargetHeight = 1.8f;   // matches the CapsuleCollider height

    const string DefaultControllerPath =
        "Assets/Game/Characters/Engineer/Animations/AnimationController.controller";

    struct Entry
    {
        public string className;
        public string prefabPath;
        public string modelPath;
        public string controllerPath;
    }

    static readonly Entry[] Map =
    {
        new Entry { className = "Ironclad",    prefabPath = "Assets/Game/Game_Prefabs/Ironclad.prefab",    modelPath = "Assets/Game/3D Models/Heroes/Guardian/Guardian.fbx" },
        new Entry { className = "Shadowblade", prefabPath = "Assets/Game/Game_Prefabs/Shadowblade.prefab", modelPath = "Assets/Game/3D Models/Heroes/Bogar/Bogar.fbx" },
        new Entry { className = "Cleric",      prefabPath = "Assets/Game/Game_Prefabs/Cleric.prefab",      modelPath = "Assets/Game/3D Models/Heroes/Brandalf/Brandalf.fbx" },
        new Entry
        {
            className = "Arcanist",
            prefabPath = "Assets/Game/Game_Prefabs/Arcanist.prefab",
            modelPath = "Assets/Game/3D Models/Heroes/Dravos/fantasy_goblin_3d_model/Idle.fbx",
            controllerPath = "Assets/Game/3D Models/Heroes/Dravos/fantasy_goblin_3d_model/Dravos.controller"
        },
    };

    [MenuItem("BCE/Setup/CharacterSelect - Swap Class Prefab Models")]
    public static void Swap()
    {
        int done = 0;
        var report = new List<string>();

        foreach (var e in Map)
        {
            string controllerPath = string.IsNullOrEmpty(e.controllerPath) ? DefaultControllerPath : e.controllerPath;
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.LogError($"[ModelSwap] AnimationController not found at {controllerPath}. Skipping {e.className}.");
                continue;
            }

            // 1 — Ensure the model is Humanoid with an avatar.
            var avatar = EnsureHumanoid(e.modelPath);
            if (avatar == null)
                report.Add($"⚠ {e.className}: no valid Humanoid avatar — fix the Rig tab manually, then re-run.");

            // 2 — Load the prefab contents for editing.
            var root = PrefabUtility.LoadPrefabContents(e.prefabPath);
            if (root == null)
            {
                Debug.LogError($"[ModelSwap] Could not load prefab {e.prefabPath}");
                continue;
            }

            // Remove any existing "Model" child(ren).
            var existing = new List<Transform>();
            foreach (Transform c in root.transform)
                if (c.name == "Model") existing.Add(c);
            foreach (var t in existing) Object.DestroyImmediate(t.gameObject);

            // 3 — Instantiate the new model as "Model".
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(e.modelPath);
            if (modelAsset == null)
            {
                Debug.LogError($"[ModelSwap] Model FBX not found at {e.modelPath}");
                PrefabUtility.UnloadPrefabContents(root);
                continue;
            }

            var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, root.scene);
            model.transform.SetParent(root.transform, false);
            model.name = "Model";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale    = Vector3.one;

            // 4 — Auto-scale to target height and ground on y=0.
            FitToHeight(model, TargetHeight);

            // 5 — Wire the Animator.
            var anim = model.GetComponent<Animator>();
            if (anim == null) anim = model.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            if (avatar != null) anim.avatar = avatar;
            anim.applyRootMotion = false;
            anim.cullingMode     = AnimatorCullingMode.CullUpdateTransforms;

            // Save.
            PrefabUtility.SaveAsPrefabAsset(root, e.prefabPath);
            PrefabUtility.UnloadPrefabContents(root);

            done++;
            report.Add($"✔ {e.className} ← {System.IO.Path.GetFileName(e.modelPath)}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ModelSwap] Swapped {done}/{Map.Length} class prefabs.\n" +
                  string.Join("\n", report) +
                  "\nWarden left as-is (already uses the Engineer model).");
    }

    // Force a model to Humanoid + create-from-this-model avatar. Returns the avatar,
    // or null if the rig failed to produce a valid one.
    static Avatar EnsureHumanoid(string fbxPath)
    {
        var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError($"[ModelSwap] No ModelImporter at {fbxPath}");
            return null;
        }

        bool changed = false;
        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            changed = true;
        }
        if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            changed = true;
        }
        if (changed)
        {
            importer.SaveAndReimport();
        }

        // Find the generated avatar sub-asset. Return null if it isn't a valid
        // human rig so the caller can warn (Humanoid clips won't retarget onto it).
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
        {
            if (obj is Avatar av && av.isValid && av.isHuman)
                return av;
        }
        return null;
    }

    // Scale a model so its combined renderer bounds are TargetHeight tall, then
    // centre X/Z and drop its feet to y = 0.
    static void FitToHeight(GameObject go, float targetH)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        if (b.size.y < 0.0001f) return;

        float scale = targetH / b.size.y;
        go.transform.localScale = go.transform.localScale * scale;

        // Re-measure after scaling.
        b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        go.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);
    }
}
