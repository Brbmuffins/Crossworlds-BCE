#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class FieldGoulAnimatorBuilder
{
    const string Root = "Assets/Game/Enemies/Fields of Gundab/Field Goul";
    const string AnimationDir = Root + "/Animations";
    const string ControllerDir = Root + "/Controller";
    const string ControllerPath = ControllerDir + "/Field_Goul.controller";

    [MenuItem("Rate of Decay/Enemies/Field Goul/Create Animator Controller")]
    public static void CreateController()
    {
        EnsureDir(ControllerDir);

        ConfigureClipImport(AnimationDir + "/Idle.fbx", true);
        ConfigureClipImport(AnimationDir + "/Run.fbx", true);
        ConfigureClipImport(AnimationDir + "/Punch.fbx", false);
        ConfigureClipImport(AnimationDir + "/Scream.fbx", false);
        ConfigureClipImport(AnimationDir + "/Death.fbx", false);

        AssetDatabase.ImportAsset(AnimationDir + "/Idle.fbx");
        AssetDatabase.ImportAsset(AnimationDir + "/Run.fbx");
        AssetDatabase.ImportAsset(AnimationDir + "/Punch.fbx");
        AssetDatabase.ImportAsset(AnimationDir + "/Scream.fbx");
        AssetDatabase.ImportAsset(AnimationDir + "/Death.fbx");

        AnimationClip idle = LoadClip(AnimationDir + "/Idle.fbx");
        AnimationClip run = LoadClip(AnimationDir + "/Run.fbx");
        AnimationClip punch = LoadClip(AnimationDir + "/Punch.fbx");
        AnimationClip scream = LoadClip(AnimationDir + "/Scream.fbx");
        AnimationClip death = LoadClip(AnimationDir + "/Death.fbx");

        if (idle == null || run == null || punch == null || scream == null || death == null)
        {
            Debug.LogError("[FieldGoulAnimatorBuilder] Missing one or more Field Goul animation clips.");
            return;
        }

        if (File.Exists(ControllerPath))
            AssetDatabase.DeleteAsset(ControllerPath);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Scream", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimatorState idleState = sm.AddState("Idle", new Vector3(240f, 120f, 0f));
        AnimatorState runState = sm.AddState("Run", new Vector3(520f, 120f, 0f));
        AnimatorState punchState = sm.AddState("Punch", new Vector3(520f, 280f, 0f));
        AnimatorState screamState = sm.AddState("Scream", new Vector3(240f, 280f, 0f));
        AnimatorState deathState = sm.AddState("Death", new Vector3(760f, 280f, 0f));

        idleState.motion = idle;
        runState.motion = run;
        punchState.motion = punch;
        screamState.motion = scream;
        deathState.motion = death;
        sm.defaultState = idleState;

        AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.12f;
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.12f;
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        AddAnyStateTrigger(sm, punchState, "Attack", 0.06f, false);
        AddReturnTransition(punchState, idleState, 0.08f);

        AddAnyStateTrigger(sm, screamState, "Scream", 0.08f, false);
        AddReturnTransition(screamState, idleState, 0.1f);

        AddAnyStateTrigger(sm, deathState, "Die", 0.05f, true);
        AnimatorStateTransition deathBool = sm.AddAnyStateTransition(deathState);
        deathBool.hasExitTime = false;
        deathBool.duration = 0.05f;
        deathBool.canTransitionToSelf = false;
        deathBool.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FieldGoulAnimatorBuilder] Created {ControllerPath}");
    }

    [MenuItem("Rate of Decay/Enemies/Field Goul/Assign Controller To Selection")]
    public static void AssignControllerToSelection()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            CreateController();
            controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        }

        if (controller == null)
        {
            Debug.LogError("[FieldGoulAnimatorBuilder] Could not create or load Field_Goul.controller.");
            return;
        }

        int assigned = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            Animator animator = go.GetComponentInChildren<Animator>();
            if (animator == null)
                animator = go.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            GameObject driverTarget = animator.gameObject;
            if (driverTarget.GetComponent<FieldGoulAnimationDriver>() == null)
                driverTarget.AddComponent<FieldGoulAnimationDriver>();

            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(driverTarget);
            assigned++;
        }

        Debug.Log($"[FieldGoulAnimatorBuilder] Assigned Field Goul controller to {assigned} selected object(s).");
    }

    static void AddAnyStateTrigger(AnimatorStateMachine sm, AnimatorState target, string trigger, float duration, bool finalState)
    {
        AnimatorStateTransition transition = sm.AddAnyStateTransition(target);
        transition.hasExitTime = false;
        transition.duration = duration;
        transition.canTransitionToSelf = false;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);

        if (!finalState)
            transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");
    }

    static void AddReturnTransition(AnimatorState from, AnimatorState to, float duration)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 0.9f;
        transition.duration = duration;
        transition.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDead");
    }

    static AnimationClip LoadClip(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
                return clip;
        }

        return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    static void ConfigureClipImport(string path, bool loop)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
            return;

        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.clipAnimations;

        foreach (ModelImporterClipAnimation clip in clips)
        {
            clip.loopTime = loop;
            clip.loopPose = loop;
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    static void EnsureDir(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folder = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent))
            EnsureDir(parent);

        AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, folder);
    }
}
#endif
