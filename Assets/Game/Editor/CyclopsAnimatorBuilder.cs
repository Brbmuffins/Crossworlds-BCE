#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class CyclopsAnimatorBuilder
{
    const string Root = "Assets/Game/NPC/Cyclops";
    const string ControllerPath = Root + "/Cyclops.controller";
    const string PrefabDir = "Assets/Game/Prefabs";
    const string PrefabPath = PrefabDir + "/Cyclops_Mob.prefab";

    const string IdlePath = Root + "/Idle.fbx";
    const string RunPath = Root + "/Run.fbx";
    const string AttackPath = Root + "/Attack.fbx";

    [MenuItem("Rate of Decay/Enemies/Cyclops/Create Animator Controller")]
    public static void CreateAnimatorController()
    {
        ConfigureModelImport(IdlePath, "Idle", loop: true);
        ConfigureModelImport(RunPath, "Run", loop: true);
        ConfigureModelImport(AttackPath, "Attack", loop: false);

        AnimationClip idle = LoadClip(IdlePath);
        AnimationClip run = LoadClip(RunPath);
        AnimationClip attack = LoadClip(AttackPath);

        if (idle == null || run == null || attack == null)
        {
            Debug.LogError("[CyclopsAnimatorBuilder] Missing one or more Cyclops animation clips.");
            return;
        }

        if (File.Exists(ControllerPath))
            AssetDatabase.DeleteAsset(ControllerPath);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = AddState(stateMachine, "Idle", idle, new Vector3(240f, 120f, 0f));
        AnimatorState runState = AddState(stateMachine, "Run", run, new Vector3(520f, 120f, 0f));
        AnimatorState attackState = AddState(stateMachine, "Attack", attack, new Vector3(520f, 280f, 0f));
        AnimatorState deathState = AddState(stateMachine, "Death", idle, new Vector3(760f, 280f, 0f));

        stateMachine.defaultState = idleState;

        AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.12f;
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.12f;
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        AddAnyStateTrigger(stateMachine, attackState, "Attack", 0.05f, finalState: false);
        AddReturnTransition(attackState, idleState, 0.08f);

        AddAnyStateTrigger(stateMachine, deathState, "Die", 0.05f, finalState: true);
        AnimatorStateTransition deathBool = stateMachine.AddAnyStateTransition(deathState);
        deathBool.hasExitTime = false;
        deathBool.duration = 0.05f;
        deathBool.canTransitionToSelf = false;
        deathBool.AddCondition(AnimatorConditionMode.If, 0f, "IsDead");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CyclopsAnimatorBuilder] Created {ControllerPath}");
    }

    [MenuItem("Rate of Decay/Enemies/Cyclops/Create Or Update Enemy Prefab")]
    public static void CreateOrUpdateEnemyPrefab()
    {
        CreateAnimatorController();
        EnsureDir(PrefabDir);

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(IdlePath);

        if (controller == null || modelPrefab == null)
        {
            Debug.LogError("[CyclopsAnimatorBuilder] Could not load Cyclops controller or Idle model.");
            return;
        }

        GameObject root = new GameObject("Cyclops_Mob");
        root.tag = "Enemy";

        CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
        collider.radius = 0.55f;
        collider.height = 2.6f;
        collider.center = new Vector3(0f, 1.3f, 0f);

        NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
        agent.speed = 3.2f;
        agent.angularSpeed = 480f;
        agent.acceleration = 16f;
        agent.stoppingDistance = 1.7f;
        agent.radius = 0.55f;
        agent.height = 2.6f;

        Health health = root.AddComponent<Health>();
        health.maxHealth = 180f;

        FieldGhoulNPC roaming = root.AddComponent<FieldGhoulNPC>();
        roaming.wanderRadius = 8f;
        roaming.minWaitTime = 2f;
        roaming.maxWaitTime = 5f;

        EnemyController enemy = root.AddComponent<EnemyController>();
        enemy.aggroRadius = 10f;
        enemy.leashRadius = 24f;
        enemy.attackRange = 2.1f;
        enemy.attackInterval = 1.8f;
        enemy.damage = 22f;
        enemy.enemyTemplateId = "cyclops_basic";

        root.AddComponent<NetworkIdentity>();
        root.AddComponent<NetworkTransformUnreliable>();

        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
        if (model == null)
            model = Object.Instantiate(modelPrefab);

        model.name = "Cyclops_Model";
        model.transform.SetParent(root.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        Animator animator = model.GetComponentInChildren<Animator>();
        if (animator == null)
            animator = model.AddComponent<Animator>();

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        if (animator.GetComponent<CyclopsAnimationDriver>() == null)
            animator.gameObject.AddComponent<CyclopsAnimationDriver>();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out bool saved);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (saved)
            Debug.Log($"[CyclopsAnimatorBuilder] Saved {PrefabPath}");
        else
            Debug.LogError("[CyclopsAnimatorBuilder] Failed to save Cyclops_Mob.prefab.");
    }

    [MenuItem("Rate of Decay/Enemies/Cyclops/Assign Controller To Selection")]
    public static void AssignControllerToSelection()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            CreateAnimatorController();
            controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        }

        if (controller == null)
        {
            Debug.LogError("[CyclopsAnimatorBuilder] Could not create or load Cyclops.controller.");
            return;
        }

        int assigned = 0;
        foreach (GameObject selected in Selection.gameObjects)
        {
            GameObject cyclopsRoot = GetCyclopsRoot(selected);
            SetupNpcComponents(cyclopsRoot);

            Animator animator = cyclopsRoot.GetComponentInChildren<Animator>();
            if (animator == null)
                animator = cyclopsRoot.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            GameObject driverTarget = animator.gameObject;
            if (driverTarget.GetComponent<CyclopsAnimationDriver>() == null)
                driverTarget.AddComponent<CyclopsAnimationDriver>();

            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(driverTarget);
            EditorUtility.SetDirty(cyclopsRoot);
            assigned++;
        }

        Debug.Log($"[CyclopsAnimatorBuilder] Assigned Cyclops controller to {assigned} selected object(s).");
    }

    [MenuItem("Rate of Decay/Enemies/Cyclops/Add NPC Components To Selection")]
    public static void AddNpcComponentsToSelection()
    {
        int updated = 0;
        foreach (GameObject selected in Selection.gameObjects)
        {
            GameObject cyclopsRoot = GetCyclopsRoot(selected);
            SetupNpcComponents(cyclopsRoot);
            EditorUtility.SetDirty(cyclopsRoot);
            updated++;
        }

        if (updated > 0)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[CyclopsAnimatorBuilder] Added Cyclops NPC components to {updated} selected object(s).");
    }

    [MenuItem("Rate of Decay/Enemies/Cyclops/Assign Controller To All Cyclops In Open Scene")]
    public static void AssignControllerToAllCyclopsInOpenScene()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            CreateAnimatorController();
            controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        }

        if (controller == null)
        {
            Debug.LogError("[CyclopsAnimatorBuilder] Could not create or load Cyclops.controller.");
            return;
        }

        int assigned = 0;
        HashSet<GameObject> visited = new HashSet<GameObject>();
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!LooksLikeCyclops(go))
                continue;

            GameObject cyclopsRoot = GetCyclopsRoot(go);
            if (!visited.Add(cyclopsRoot))
                continue;

            SetupNpcComponents(cyclopsRoot);

            Animator animator = cyclopsRoot.GetComponentInChildren<Animator>();
            if (animator == null)
                animator = cyclopsRoot.AddComponent<Animator>();

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            GameObject driverTarget = animator.gameObject;
            if (driverTarget.GetComponent<CyclopsAnimationDriver>() == null)
                driverTarget.AddComponent<CyclopsAnimationDriver>();

            EditorUtility.SetDirty(animator);
            EditorUtility.SetDirty(driverTarget);
            EditorUtility.SetDirty(cyclopsRoot);
            assigned++;
        }

        if (assigned > 0)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[CyclopsAnimatorBuilder] Assigned Cyclops controller to {assigned} Cyclops object(s) in the open scene.");
    }

    [MenuItem("Rate of Decay/Enemies/Cyclops/Add NPC Components To All Cyclops In Open Scene")]
    public static void AddNpcComponentsToAllCyclopsInOpenScene()
    {
        int updated = 0;
        HashSet<GameObject> visited = new HashSet<GameObject>();
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!LooksLikeCyclops(go))
                continue;

            GameObject cyclopsRoot = GetCyclopsRoot(go);
            if (!visited.Add(cyclopsRoot))
                continue;

            SetupNpcComponents(cyclopsRoot);
            EditorUtility.SetDirty(cyclopsRoot);
            updated++;
        }

        if (updated > 0)
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[CyclopsAnimatorBuilder] Added Cyclops NPC components to {updated} Cyclops object(s) in the open scene.");
    }

    static AnimatorState AddState(AnimatorStateMachine stateMachine, string name, AnimationClip clip, Vector3 position)
    {
        AnimatorState state = stateMachine.AddState(name, position);
        state.motion = clip;
        state.writeDefaultValues = true;
        return state;
    }

    static void AddAnyStateTrigger(AnimatorStateMachine stateMachine, AnimatorState target, string trigger, float duration, bool finalState)
    {
        AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(target);
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

    static bool LooksLikeCyclops(GameObject go)
    {
        if (go.name.IndexOf("cyclops", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        if (go.name.IndexOf("ogre", System.StringComparison.OrdinalIgnoreCase) >= 0)
            return true;

        Transform current = go.transform.parent;
        while (current != null)
        {
            if (current.name.IndexOf("cyclops", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            current = current.parent;
        }

        return false;
    }

    static GameObject GetCyclopsRoot(GameObject go)
    {
        Transform current = go.transform;
        Transform best = current;

        while (current != null)
        {
            if (HasCyclopsName(current.gameObject))
                best = current;

            current = current.parent;
        }

        return best.gameObject;
    }

    static bool HasCyclopsName(GameObject go)
    {
        return go.name.IndexOf("cyclops", System.StringComparison.OrdinalIgnoreCase) >= 0
            || go.name.IndexOf("ogre", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static void SetupNpcComponents(GameObject go)
    {
        CapsuleCollider collider = go.GetComponent<CapsuleCollider>();
        if (collider == null)
            collider = Undo.AddComponent<CapsuleCollider>(go);

        collider.radius = 0.55f;
        collider.height = 2.6f;
        collider.center = new Vector3(0f, 1.3f, 0f);

        NavMeshAgent agent = go.GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = Undo.AddComponent<NavMeshAgent>(go);

        agent.speed = 3.2f;
        agent.angularSpeed = 480f;
        agent.acceleration = 16f;
        agent.stoppingDistance = 1.7f;
        agent.radius = 0.55f;
        agent.height = 2.6f;

        Health health = go.GetComponent<Health>();
        if (health == null)
            health = Undo.AddComponent<Health>(go);

        health.maxHealth = 180f;

        FieldGhoulNPC roaming = go.GetComponent<FieldGhoulNPC>();
        if (roaming == null)
            roaming = Undo.AddComponent<FieldGhoulNPC>(go);

        roaming.wanderRadius = 8f;
        roaming.minWaitTime = 2f;
        roaming.maxWaitTime = 5f;
    }

    static AnimationClip LoadClip(string path)
    {
        return AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => !clip.name.StartsWith("__preview__", System.StringComparison.Ordinal))
            ?? AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
    }

    static void ConfigureModelImport(string path, string clipName, bool loop)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
            return;

        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.clipAnimations;

        foreach (ModelImporterClipAnimation clip in clips)
        {
            clip.name = clipName;
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
