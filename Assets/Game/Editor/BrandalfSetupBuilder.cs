#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// BrandalfSetupBuilder — BCE/Heroes/Setup Brandalf
///
/// One-click setup to make Brandalf your playable Cleric in Hub:
///
///   1. Builds BrandalfAnimController.controller using Brandalf's own animation
///      clips, mapped to the project's standard parameter names so PlayerAnimator,
///      CastAnimator, and PlayerMovement drive him out of the box.
///
///   2. Patches the Cleric prefab:
///        • Adds Brandalf.fbx as a child named "Model" (replaces any existing Model child)
///        • Assigns BrandalfAnimController to the Animator
///        • Sets Avatar from Brandalf.fbx (if a Humanoid avatar is embedded)
///        • applyRootMotion = false (physics-driven)
///
/// Brandalf's animation set:
///   Brandalf.fbx (idle)  Running.fbx  Run Backward.fbx
///   Standing Jump.fbx    Running Jump.fbx  Jump Backwards.fbx  Death.fbx
///   Combat Animations/AOE.fbx
///   Combat Animations/Big Single Target.fbx
///   Combat Animations/BigGroundCast.fbx
///   Combat Animations/Standing 1H Magic Attack 02.fbx
///
/// Missing clips fall back to brbmuffins, then Blink.
///
/// Standard parameters produced (compatible with PlayerAnimator + CastAnimator):
///   float   Speed         0=idle  1=run  (PlayerAnimator sets this)
///   bool    IsInCombat    idle ↔ idle-combat pose
///   bool    isBackwards   (PlayerMovement)
///   bool    isSprinting   (PlayerMovement)
///   trigger Jump          (PlayerMovement)
///   trigger dodge         (PlayerMovement)
///   trigger GetHit        (PlayerAnimator via Health.onDamageTaken)
///   trigger Death         (PlayerAnimator via Health.onDeath)
///   trigger Attack        (generic attack)
///   trigger CastDamage    → Big Single Target
///   trigger CastHeal      → AOE (closest to a heal AoE)
///   trigger CastSupport   → BigGroundCast
///   trigger CastTwoHanded → AOE (heavy variant)
/// </summary>
public static class BrandalfSetupBuilder
{
    const string HeroDir    = "Assets/Game/3D Models/Heroes/Brandalf";
    const string OutDir     = "Assets/Game/Animations";
    const string CtrlPath   = "Assets/Game/Animations/BrandalfAnimController.controller";
    const string PrefabPath = "Assets/Game/Game_Prefabs/Cleric.prefab";

    // Fallback packs for missing clips
    const string Brb   = "Assets/brbmuffins Swords/brbmuffins Sword Art/Animations/Animations_Starter_Pack";
    const string Blink = "Assets/Blink/Art/Animations";

    [MenuItem("BCE/Heroes/Setup Brandalf")]
    public static void SetupBrandalf()
    {
        EnsureDir(OutDir);

        // ── 1. Build the animator controller ─────────────────────────────────────
        var ctrl = BuildController();
        if (ctrl == null) return;

        // ── 2. Patch Arcanist prefab ──────────────────────────────────────────────
        PatchClericPrefab(ctrl);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("✅ Brandalf Ready",
            "Brandalf wired into Cleric prefab.\n\n" +
            "Controller: BrandalfAnimController.controller\n" +
            "Mesh: Brandalf.fbx (child of Cleric root)\n\n" +
            "NEXT:\n" +
            "1. Run BCE/Hub World/Wire Combat Assets (Make Playable) if not done.\n" +
            "2. Press Play — select Cleric at character select.\n" +
            "3. Walk into the red zone to start fighting!",
            "Let's go!");
    }

    // ── Build controller ──────────────────────────────────────────────────────────

    static AnimatorController BuildController()
    {
        // ── Brandalf clips ────────────────────────────────────────────────────────
        AnimationClip idle       = LoadClip($"{HeroDir}/Brandalf.fbx");           // T-pose / idle embedded
        AnimationClip runFwd     = LoadClip($"{HeroDir}/Running.fbx");
        AnimationClip runBack    = LoadClip($"{HeroDir}/Run Backward.fbx");
        AnimationClip standJump  = LoadClip($"{HeroDir}/Standing Jump.fbx");
        AnimationClip runJump    = LoadClip($"{HeroDir}/Running Jump.fbx");
        AnimationClip jumpBack   = LoadClip($"{HeroDir}/Jump Backwards.fbx");
        AnimationClip death      = LoadClip($"{HeroDir}/Death.fbx");

        // Combat animations
        AnimationClip bigSingle  = LoadClip($"{HeroDir}/Combat Animations/Big Single Target.fbx");
        AnimationClip aoe        = LoadClip($"{HeroDir}/Combat Animations/AOE.fbx");
        AnimationClip groundCast = LoadClip($"{HeroDir}/Combat Animations/BigGroundCast.fbx");
        AnimationClip magic1H    = LoadClip($"{HeroDir}/Combat Animations/Standing 1H Magic Attack 02.fbx");

        if (runFwd == null)
        {
            Debug.LogError("[BCE] Brandalf/Running.fbx not found — check Assets/Game/Heroes/Brandalf/");
            return null;
        }

        // ── Fallbacks for missing clips ────────────────────────────────────────────
        if (idle == null)
            idle = FallbackClip("Movement/Idle.fbx");

        // Idle-combat pose: use magic1H as a looping ready stance, or fall back
        AnimationClip idleCombat = FallbackClip("Combat/IdleCombat.fbx") ?? idle;

        // GetHit — Brandalf has no GetHit animation; use brbmuffins
        AnimationClip getHit = FallbackClip("Combat/GetHit.fbx");

        // Assign combat clips, cross-mapping to our trigger slots:
        //   CastDamage  → Big Single Target (aimed spell)
        //   Attack      → Standing 1H Magic Attack 02 (quick cast)
        //   CastHeal    → AOE (closest we have to a heal visual)
        //   CastSupport → BigGroundCast (ground AoE / buff zone)
        //   CastTwoHanded → AOE (heavy variant)
        AnimationClip castDamage  = bigSingle  ?? aoe ?? magic1H;
        AnimationClip castHeal    = aoe        ?? groundCast ?? magic1H;
        AnimationClip castSupport = groundCast ?? aoe ?? magic1H;
        AnimationClip castHeavy   = aoe        ?? groundCast ?? magic1H;
        AnimationClip attackQuick = magic1H    ?? bigSingle;

        // ── Create controller ──────────────────────────────────────────────────────
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(CtrlPath);

        // ── Parameters (our standard set) ─────────────────────────────────────────
        ctrl.AddParameter("Speed",         AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsInCombat",    AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("isBackwards",   AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("isSprinting",   AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("isGrounded",    AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Jump",          AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("dodge",         AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("GetHit",        AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Death",         AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("IsDead",        AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Attack",        AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("CastDamage",    AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("CastHeal",      AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("CastSupport",   AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("CastTwoHanded", AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;

        // ── States ─────────────────────────────────────────────────────────────────
        var stIdle      = S(sm, "Idle",          idle,        new Vector3(-300,   0));
        var stIdleCbt   = S(sm, "IdleCombat",    idleCombat,  new Vector3(-300,  90));
        var stRun       = S(sm, "Run",           runFwd,      new Vector3(   0,   0));
        var stRunBack   = S(sm, "RunBackward",   runBack,     new Vector3(   0,  90));
        var stJump      = S(sm, "Jump",          standJump,   new Vector3(   0, 180));
        var stRunJump   = S(sm, "RunningJump",   runJump,     new Vector3( 200, 180));
        var stJumpBack  = S(sm, "JumpBackward",  jumpBack,    new Vector3( 400, 180));
        var stCastDmg   = S(sm, "CastDamage",   castDamage,  new Vector3( 300,   0));
        var stCastHeal  = S(sm, "CastHeal",     castHeal,    new Vector3( 300,  90));
        var stCastSup   = S(sm, "CastSupport",  castSupport, new Vector3( 300, 180));
        var stAttack    = S(sm, "Attack",        attackQuick, new Vector3( 300, -90));
        var stCastHeavy = S(sm, "CastTwoHanded",castHeavy,   new Vector3( 300, 270));
        var stGetHit    = getHit != null ? S(sm, "GetHit", getHit, new Vector3( 600,   0)) : null;
        var stDeath     = S(sm, "Death",         death,       new Vector3( 600,  90));

        sm.defaultState = stIdle;

        // ── Locomotion ─────────────────────────────────────────────────────────────
        // Idle ↔ IdleCombat
        BoolT(stIdle,    stIdleCbt,  "IsInCombat", true,  0.15f);
        BoolT(stIdleCbt, stIdle,     "IsInCombat", false, 0.20f);

        // Idle/IdleCombat → Run
        FloatT(stIdle,    stRun, "Speed", 0.10f, isLess: false, dur: 0.12f);
        FloatT(stIdleCbt, stRun, "Speed", 0.10f, isLess: false, dur: 0.12f);

        // Run → Idle
        FloatBoolT(stRun, stIdle,    "Speed", 0.05f, true, "IsInCombat", false, 0.20f);
        FloatBoolT(stRun, stIdleCbt, "Speed", 0.05f, true, "IsInCombat", true,  0.20f);

        // Run ↔ RunBackward
        BoolT(stRun,     stRunBack, "isBackwards", true,  0.10f);
        BoolT(stRunBack, stRun,     "isBackwards", false, 0.10f);

        // Jumps from Any State (Speed < 0.1 = standing, Speed > 0.1 = running)
        JumpT(sm, stJump,     maxSpeed: 0.10f, backwards: false, standing: true);
        JumpT(sm, stRunJump,  maxSpeed: 0.10f, backwards: false, standing: false);
        JumpT(sm, stJumpBack, maxSpeed: 0.10f, backwards: true,  standing: false);

        // Jumps return to locomotion on exit
        ExitT(stJump,     stIdle,    0.85f, 0.15f);
        ExitT(stRunJump,  stRun,     0.85f, 0.15f);
        ExitT(stJumpBack, stRunBack, 0.85f, 0.15f);

        // ── Combat — Any State triggers ────────────────────────────────────────────
        AnyT(sm, stCastDmg,   "CastDamage",    0.05f);
        AnyT(sm, stAttack,    "Attack",         0.05f);
        AnyT(sm, stCastHeal,  "CastHeal",       0.05f);
        AnyT(sm, stCastSup,   "CastSupport",    0.05f);
        AnyT(sm, stCastHeavy, "CastTwoHanded",  0.05f);
        if (stGetHit != null) AnyT(sm, stGetHit, "GetHit", 0.05f);
        AnyT(sm, stDeath,     "Death",           0.05f);

        // ── Return after combat ────────────────────────────────────────────────────
        ExitT(stCastDmg,   stIdleCbt, 0.85f, 0.15f);
        ExitT(stAttack,    stIdleCbt, 0.85f, 0.15f);
        ExitT(stCastHeal,  stIdleCbt, 0.85f, 0.15f);
        ExitT(stCastSup,   stIdleCbt, 0.85f, 0.15f);
        ExitT(stCastHeavy, stIdleCbt, 0.85f, 0.15f);
        if (stGetHit != null) ExitT(stGetHit, stIdleCbt, 0.70f, 0.15f);
        // Hold the death pose until PlayerAnimator reports that the player revived.
        BoolT(stDeath, stIdle, "IsDead", false, 0.15f);

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();

        Debug.Log("[BCE] BrandalfAnimController created → " + CtrlPath);
        return ctrl;
    }

    // ── Patch Arcanist prefab ─────────────────────────────────────────────────────

    static void PatchClericPrefab(AnimatorController ctrl)
    {
        if (!System.IO.File.Exists(PrefabPath))
        {
            Debug.LogWarning("[BCE] Cleric.prefab not found — run BCE/Setup/4 first, then re-run.");
            return;
        }

        var brandalfFbx = AssetDatabase.LoadAssetAtPath<GameObject>($"{HeroDir}/Brandalf.fbx");
        if (brandalfFbx == null)
        {
            Debug.LogWarning("[BCE] Brandalf.fbx not found at " + HeroDir);
            return;
        }

        // Extract humanoid avatar from the FBX
        Avatar brandalfAvatar = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath($"{HeroDir}/Brandalf.fbx"))
        {
            if (obj is Avatar av) { brandalfAvatar = av; break; }
        }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(PrefabPath))
        {
            var root = scope.prefabContentsRoot;
            if (root.GetComponent<PlayerFallReset>() == null)
                root.AddComponent<PlayerFallReset>();
            if (root.GetComponent<PlayerAnimator>() == null)
                root.AddComponent<PlayerAnimator>();

            // ── Remove any existing Model child ───────────────────────────────────
            var existing = root.transform.Find("Model");
            GameObject modelGO;
            if (existing != null)
            {
                // Preserve Cleric-specific scale, placement, and prefab overrides.
                modelGO = existing.gameObject;
            }
            else
            {
                modelGO = (GameObject)PrefabUtility.InstantiatePrefab(brandalfFbx, root.transform);
                modelGO.name = "Model";
                modelGO.transform.localPosition = Vector3.zero;
                modelGO.transform.localRotation = Quaternion.identity;
                modelGO.transform.localScale = Vector3.one;
            }

            // ── Wire the Animator ─────────────────────────────────────────────────
            // Animator may be on the model root or embedded in the FBX hierarchy
            var anim = modelGO.GetComponent<Animator>();
            if (anim == null) anim = modelGO.GetComponentInChildren<Animator>(true);
            if (anim == null) anim = modelGO.AddComponent<Animator>();

            anim.runtimeAnimatorController = ctrl;
            anim.applyRootMotion           = false;
            if (brandalfAvatar != null) anim.avatar = brandalfAvatar;

            // ── Disable any placeholder mesh renderer on root ─────────────────────
            var rootRenderer = root.GetComponent<SkinnedMeshRenderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;
            var rootMeshRenderer = root.GetComponent<MeshRenderer>();
            if (rootMeshRenderer != null) rootMeshRenderer.enabled = false;

            Debug.Log("[BCE] Cleric prefab patched → BrandalfAnimController assigned.");
        }

        AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceUpdate);
    }

    // ── Clip loaders ─────────────────────────────────────────────────────────────

    static AnimationClip LoadClip(string fbxPath)
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        if (all == null || all.Length == 0) return null;
        return all.OfType<AnimationClip>()
                  .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
    }

    static AnimationClip FallbackClip(string relativePath)
    {
        return LoadClip($"{Brb}/{relativePath}") ?? LoadClip($"{Blink}/{relativePath}");
    }

    // ── Transition helpers ────────────────────────────────────────────────────────

    static AnimatorState S(AnimatorStateMachine sm, string name, AnimationClip clip, Vector3 pos)
    {
        var s = sm.AddState(name, pos);
        s.motion             = clip;
        s.writeDefaultValues = true;
        return s;
    }

    static void BoolT(AnimatorState from, AnimatorState to, string param, bool value, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = dur;
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
    }

    static void FloatT(AnimatorState from, AnimatorState to, string param, float threshold, bool isLess, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = dur;
        t.AddCondition(isLess ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater, threshold, param);
    }

    static void FloatBoolT(AnimatorState from, AnimatorState to, string floatParam,
        float threshold, bool isLess, string boolParam, bool boolValue, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration = dur;
        t.AddCondition(isLess ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater,
            threshold, floatParam);
        t.AddCondition(boolValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
            0, boolParam);
    }

    static void JumpT(AnimatorStateMachine sm, AnimatorState to, float maxSpeed,
        bool backwards, bool standing)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime = false;
        t.duration = 0.05f;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, "Jump");
        t.AddCondition(standing ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater,
            maxSpeed, "Speed");
        t.AddCondition(backwards ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
            0, "isBackwards");
    }

    static void AnyT(AnimatorStateMachine sm, AnimatorState to, string trigger, float dur,
        string condition2Param = null, AnimatorConditionMode condition2Mode = AnimatorConditionMode.If)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime         = false;
        t.duration            = dur;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        if (condition2Param != null)
            t.AddCondition(condition2Mode, 0, condition2Param);
    }

    static void ExitT(AnimatorState from, AnimatorState to, float exitTime, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true;
        t.exitTime    = exitTime;
        t.duration    = dur;
    }

    static void EnsureDir(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}
#endif
