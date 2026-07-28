#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Imports the Brontosaurus as a Generic rig, loops its embedded walk take,
/// and creates a ready-to-place animated prefab.
/// </summary>
public static class BrontosaurusSetupBuilder
{
    const string ModelPath =
        "Assets/Game/3D Models/Toujam/Brontasaurus/brontowalknew.fbx";
    const string ControllerPath =
        "Assets/Game/3D Models/Toujam/Brontasaurus/Brontosaurus.controller";
    const string PrefabDirectory = "Assets/Game/Prefabs/Environment";
    const string PrefabPath = PrefabDirectory + "/Brontosaurus.prefab";

    [InitializeOnLoadMethod]
    static void BuildOnceAfterImport()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath) == null)
            return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab != null &&
            prefab.GetComponent<BrontosaurusAnimationDriver>() != null &&
            AssetDatabase.GetDependencies(PrefabPath, false).Contains(ModelPath))
            return;

        EditorApplication.delayCall += Build;
    }

    [MenuItem("BCE/Setup/Build Animated Brontosaurus")]
    public static void Build()
    {
        ConfigureModelImporter();

        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        var walk = AssetDatabase.LoadAllAssetsAtPath(ModelPath)
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => !clip.name.StartsWith("__preview__"));

        if (model == null || walk == null)
        {
            Debug.LogError("[BCE] Brontosaurus model or embedded animation was not found.");
            return;
        }

        var controller = BuildController(walk);
        EnsureFolder("Assets/Game/Prefabs", "Environment");

        var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (instance == null)
        {
            Debug.LogError("[BCE] Could not instantiate the Brontosaurus model.");
            return;
        }

        instance.name = "Brontosaurus";
        var animator = instance.GetComponent<Animator>();
        if (animator == null)
            animator = instance.AddComponent<Animator>();

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        if (instance.GetComponent<BrontosaurusAnimationDriver>() == null)
            instance.AddComponent<BrontosaurusAnimationDriver>();

        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        Object.DestroyImmediate(instance);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
        Debug.Log($"[BCE] Animated Brontosaurus prefab created at {PrefabPath}");
    }

    static void ConfigureModelImporter()
    {
        var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
        if (importer == null)
            return;

        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true;

        var clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.defaultClipAnimations;

        foreach (var clip in clips)
        {
            clip.loopTime = true;
            clip.loopPose = true;
            clip.keepOriginalPositionY = true;
            clip.keepOriginalPositionXZ = true;
            clip.keepOriginalOrientation = true;
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    static AnimatorController BuildController(AnimationClip walk)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(ControllerPath);

        var controller =
            AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        var stateMachine = controller.layers[0].stateMachine;
        var state = stateMachine.AddState("Walk");
        state.motion = walk;
        state.speed = 1f;
        stateMachine.defaultState = state;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
