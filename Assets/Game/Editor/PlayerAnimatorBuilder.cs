#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// PlayerAnimatorBuilder — BCE editor menu to build the player AnimatorController.
///
/// BCE toolbar → Setup → 5a ▶ Create Player AnimController
///
/// Output: Assets/Game/Animations/PlayerAnimController.controller
///
/// Clip priority: brbmuffins Sword Art → Blink (fallback).
/// After running:
///   1. Assign the generated controller to the Animator on each class prefab.
///   2. Set Animator → Avatar to a Humanoid avatar matching the character mesh.
///   3. Nest the character mesh as a child of each class prefab.
///
/// Animator parameters produced:
///   float   Speed          0=idle  1=run  1.5=sprint (set by PlayerAnimator)
///   bool    IsInCombat     switches Idle ↔ IdleCombat (set by PlayerAnimator)
///   bool    isBackwards    (set by PlayerMovement)
///   bool    isSprinting    (set by PlayerMovement — controls Sprint blend)
///   trigger GetHit         (set by PlayerAnimator via Health.onDamageTaken)
///   trigger Death          (set by PlayerAnimator via Health.onDeath)
///   trigger Attack         (generic attack — damage abilities + direct Attack calls)
///   trigger CastDamage     (set by CastAnimator → damage abilities → MeleeAttack_OneHanded)
///   trigger CastHeal       (set by CastAnimator → heal abilities → SpellCast)
///   trigger CastSupport    (set by CastAnimator → support abilities → Buff)
///   trigger CastTwoHanded  (heavy attacks for Ironclad class → MeleeAttack_TwoHanded)
///   trigger Block          (blocking / shield ability → BlockingLoop)
///   trigger dodge          (set by PlayerMovement → RollForward)
/// </summary>
public static class PlayerAnimatorBuilder
{
    const string OutDir   = "Assets/Game/Animations";
    const string CtrlPath = "Assets/Game/Animations/PlayerAnimController.controller";

    // brbmuffins Sword Art — primary source
    const string Brb   = "Assets/brbmuffins Swords/brbmuffins Sword Art/Animations/Animations_Starter_Pack";
    // Blink — fallback
    const string Blink = "Assets/Blink/Art/Animations";

    [MenuItem("BCE/Setup/5a ▶ Create Player AnimController")]
    public static void CreatePlayerAnimController()
    {
        EnsureDir(OutDir);

        // ── Load clips: brbmuffins first, Blink as fallback ───────────────────────
        AnimationClip idle         = Clip("Movement/Idle.fbx");
        AnimationClip idleCombat   = Clip("Combat/IdleCombat.fbx");
        AnimationClip runFwd       = Clip("Movement/RunForward.fbx");
        AnimationClip runBack      = Clip("Movement/RunBackward.fbx");
        AnimationClip sprint       = Clip("Movement/Sprint.fbx");
        AnimationClip melee1H      = Clip("Combat/MeleeAttack_OneHanded.fbx",
                                          "Combat/MeeleeAttack_OneHanded.fbx");   // Blink typo fallback
        AnimationClip melee2H      = Clip("Combat/MeleeAttack_TwoHanded.fbx",
                                          "Combat/MeeleeAttack_TwoHanded.fbx");
        AnimationClip spellCast    = Clip("Combat/SpellCast.fbx");
        AnimationClip castingLoop  = Clip("Combat/CastingLoop.fbx");
        AnimationClip buff         = Clip("Combat/Buff.fbx");
        AnimationClip blocking     = Clip("Combat/BlockingLoop.fbx");
        AnimationClip getHit       = Clip("Combat/GetHit.fbx");
        AnimationClip death        = Clip("Combat/Death.fbx");
        AnimationClip rollFwd      = Clip("Movement/RollForward.fbx");

        if (idle == null)
        {
            Debug.LogError(
                "[BCE] No Idle.fbx found in brbmuffins or Blink packs.\n" +
                "Expected: Assets/brbmuffins Swords/brbmuffins Sword Art/Animations/…/Movement/Idle.fbx\n" +
                "Or:       Assets/Blink/Art/Animations/Movement/Idle.fbx");
            return;
        }

        // Fallbacks for optional clips
        if (melee2H     == null) melee2H    = melee1H;
        if (melee1H     == null) melee1H    = spellCast;
        if (buff        == null) buff       = spellCast ?? castingLoop;
        if (blocking    == null) blocking   = idleCombat ?? idle;
        if (castingLoop == null) castingLoop = spellCast;

        // ── Create or overwrite controller ────────────────────────────────────────
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(CtrlPath);

        // ── Parameters ────────────────────────────────────────────────────────────
        ctrl.AddParameter("Speed",         AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsInCombat",    AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("isBackwards",   AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("isSprinting",   AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("GetHit",        AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Death",         AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Attack",        AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("CastDamage",    AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("CastHeal",      AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("CastSupport",   AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("dodge",         AnimatorControllerParameterType.Trigger);

        ctrl.AddParameter("CastTwoHanded", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Block",         AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;

        // ── States ────────────────────────────────────────────────────────────────
        //  Layout: left (rest) → centre (movement) → right (combat)
        var stIdle      = S(sm, "Idle",          idle,        new Vector3(-300,   0));
        var stIdleCbt   = S(sm, "IdleCombat",    idleCombat,  new Vector3(-300,  90));
        var stRun       = S(sm, "Run",           runFwd,      new Vector3(   0,   0));
        var stRunBack   = S(sm, "RunBackward",   runBack,     new Vector3(   0,  90));
        var stSprint    = S(sm, "Sprint",        sprint,      new Vector3(   0, 180));
        var stMelee1H   = S(sm, "CastDamage",    melee1H,     new Vector3( 300,   0));
        var stMelee2H   = S(sm, "CastTwoHanded", melee2H,     new Vector3( 300, -90));
        var stCastHeal  = S(sm, "CastHeal",      spellCast,   new Vector3( 300,  90));
        var stCastSup   = S(sm, "CastSupport",   buff,        new Vector3( 300, 180));
        var stBlock     = S(sm, "Block",         blocking,    new Vector3( 300, 270));
        var stGetHit    = S(sm, "GetHit",        getHit,      new Vector3( 600,   0));
        var stDeath     = S(sm, "Death",         death,       new Vector3( 600,  90));
        var stDodge     = S(sm, "Dodge",         rollFwd,     new Vector3(   0, 270));

        sm.defaultState = stIdle;

        // ── Movement transitions ──────────────────────────────────────────────────
        BoolT(stIdle,    stIdleCbt,  "IsInCombat", true,  0.15f);
        BoolT(stIdleCbt, stIdle,     "IsInCombat", false, 0.20f);

        FloatT(stIdle,    stRun, "Speed", 0.10f, isLess: false, dur: 0.12f);
        FloatT(stIdleCbt, stRun, "Speed", 0.10f, isLess: false, dur: 0.12f);
        FloatT(stRun,     stIdleCbt, "Speed", 0.05f, isLess: true, dur: 0.20f);

        BoolT(stRun,     stRunBack, "isBackwards", true,  0.10f);
        BoolT(stRunBack, stRun,     "isBackwards", false, 0.10f);

        BoolT(stRun,    stSprint, "isSprinting", true,  0.12f);
        BoolT(stSprint, stRun,    "isSprinting", false, 0.12f);
        FloatT(stSprint, stIdleCbt, "Speed", 0.05f, isLess: true, dur: 0.20f);

        // ── Combat triggers from Any State ────────────────────────────────────────
        AnyT(sm, stMelee1H,  "CastDamage",    0.05f);
        AnyT(sm, stMelee1H,  "Attack",        0.05f);
        AnyT(sm, stMelee2H,  "CastTwoHanded", 0.05f);
        AnyT(sm, stCastHeal, "CastHeal",      0.05f);
        AnyT(sm, stCastSup,  "CastSupport",   0.05f);
        AnyT(sm, stBlock,    "Block",         0.05f);
        AnyT(sm, stGetHit,   "GetHit",        0.05f);
        AnyT(sm, stDeath,    "Death",         0.05f);
        AnyT(sm, stDodge,    "dodge",         0.05f);

        // ── Return to locomotion after combat anims ───────────────────────────────
        ExitT(stMelee1H,  stIdleCbt, exitTime: 0.85f, dur: 0.15f);
        ExitT(stMelee2H,  stIdleCbt, exitTime: 0.85f, dur: 0.15f);
        ExitT(stCastHeal, stIdleCbt, exitTime: 0.85f, dur: 0.15f);
        ExitT(stCastSup,  stIdleCbt, exitTime: 0.85f, dur: 0.15f);
        ExitT(stBlock,    stIdleCbt, exitTime: 0.90f, dur: 0.10f);
        ExitT(stGetHit,   stIdleCbt, exitTime: 0.70f, dur: 0.15f);
        ExitT(stDodge,    stRun,     exitTime: 0.90f, dur: 0.10f);
        // Death: no return — stays dead until respawn

        // ── Finalise ──────────────────────────────────────────────────────────────
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string source = LoadClip($"{Brb}/Movement/Idle.fbx") != null ? "brbmuffins" : "Blink";
        Debug.Log(
            $"[BCE] PlayerAnimController created ({source} clips) → {CtrlPath}\n" +
            "NEXT:\n" +
            "1. Assign controller to Animator on each class prefab.\n" +
            "2. Set Animator → Avatar to a Humanoid avatar matching your mesh.\n" +
            "3. Nest character mesh as child of each class prefab.\n" +
            "4. Add PlayerAnimator component to each class prefab root.");
    }

    // ── State helper ─────────────────────────────────────────────────────────────

    static AnimatorState S(AnimatorStateMachine sm, string name, AnimationClip clip, Vector3 pos)
    {
        var s = sm.AddState(name, pos);
        s.motion             = clip;
        s.writeDefaultValues = true;
        return s;
    }

    // ── Transition helpers ────────────────────────────────────────────────────────

    static void BoolT(AnimatorState from, AnimatorState to,
        string param, bool value, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = dur;
        t.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
    }

    static void FloatT(AnimatorState from, AnimatorState to,
        string param, float threshold, bool isLess, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.duration    = dur;
        t.AddCondition(isLess ? AnimatorConditionMode.Less : AnimatorConditionMode.Greater,
            threshold, param);
    }

    static void AnyT(AnimatorStateMachine sm, AnimatorState to,
        string triggerParam, float dur)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime        = false;
        t.duration           = dur;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, triggerParam);
    }

    static void ExitT(AnimatorState from, AnimatorState to, float exitTime, float dur)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true;
        t.exitTime    = exitTime;
        t.duration    = dur;
    }

    // ── Clip loader — brbmuffins first, Blink fallback ───────────────────────────

    /// <summary>Try brbmuffins path, then Blink path (same relative sub-path).</summary>
    static AnimationClip Clip(string relativePath, string blinkOverridePath = null)
    {
        string brb  = $"{Brb}/{relativePath}";
        string blink = $"{Blink}/{(blinkOverridePath ?? relativePath)}";
        return LoadClip(brb) ?? LoadClip(blink);
    }

    static AnimationClip LoadClip(string fbxPath)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        if (all == null || all.Length == 0) return null;
        return all
            .OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
    }

    // ── Directory helper ──────────────────────────────────────────────────────────

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
