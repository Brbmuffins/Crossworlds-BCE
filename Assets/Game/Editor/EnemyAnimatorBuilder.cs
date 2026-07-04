#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// EnemyAnimatorBuilder — BCE/Setup/4d ▶ Create Enemy AnimController
///
/// Builds Assets/Game/Animations/EnemyAnimController.controller using
/// brbmuffins Sword Art animation clips (Blink fallback).
///
/// State machine mirrors EnemyController's EnemyState enum:
///   Idle → Walk/Chase → Attack → GetHit → Dead
///
/// Parameters (driven by EnemyController.OnStateChanged + RpcMeleeSwing):
///   float   Speed     0=idle, 1=walking/chasing
///   trigger Attack    melee swing (RpcMeleeSwing)
///   trigger GetHit    (optional — triggered by TakeDamage RPC if added)
///   trigger Death     death (RpcPlayDeathEffect / OnStateChanged Dead)
///
/// After running:
///   1. Assign EnemyAnimController to the Animator on Enemy_Grunt/Ranged/Elite prefabs.
///   2. Set Animator → Avatar to a matching Humanoid avatar.
///   3. Nest a character mesh as child of each enemy prefab.
///
/// EnemyController already calls SetFloat("Speed"), SetTrigger("Attack"),
/// SetTrigger("Death") via OnStateChanged and the Rpc stubs — no code changes needed.
/// </summary>
public static class EnemyAnimatorBuilder
{
    const string OutDir   = "Assets/Game/Animations";
    const string CtrlPath = "Assets/Game/Animations/EnemyAnimController.controller";

    // Tripo-generated clips (character-pipeline output) — highest priority
    const string TripoGrunt = "Assets/Game/Characters/Enemies/Grunt";
    // brbmuffins Sword Art — secondary
    const string Brb   = "Assets/brbmuffins Swords/brbmuffins Sword Art/Animations/Animations_Starter_Pack";
    // Blink — last fallback
    const string Blink = "Assets/Blink/Art/Animations";

    [MenuItem("BCE/Setup/4d ▶ Create Enemy AnimController", priority = 4)]
    public static void CreateEnemyAnimController()
    {
        EnsureDir(OutDir);

        // ── Load clips: Tripo first, brbmuffins second, Blink fallback ───────────
        AnimationClip idle     = TripoClip("idle")
                              ?? Clip("Movement/Idle.fbx");
        AnimationClip walk     = TripoClip("walk")
                              ?? Clip("Movement/RunForward.fbx");
        AnimationClip attack1H = TripoClip("slash")
                              ?? Clip("Combat/MeleeAttack_OneHanded.fbx",
                                      "Combat/MeeleeAttack_OneHanded.fbx");
        AnimationClip attack2H = attack1H;    // same clip — controller just aliases it
        AnimationClip getHit   = TripoClip("hurt")
                              ?? Clip("Combat/GetHit.fbx");
        AnimationClip stunned  = getHit
                              ?? Clip("Combat/StunnedLoop.fbx");
        AnimationClip death    = TripoClip("fall")
                              ?? Clip("Combat/Death.fbx");

        if (idle == null)
        {
            Debug.LogError(
                "[BCE] No Idle clip found for enemy controller.\n" +
                "Run BCE/Setup/4d after Tripo pipeline completes, or import:\n" +
                "  " + Brb + "/Movement/Idle.fbx\n" +
                "  " + Blink + "/Movement/Idle.fbx");
            return;
        }

        // Fallbacks
        if (getHit == null) getHit  = idle;
        if (stunned == null) stunned = idle;
        if (death  == null) death   = getHit;

        // ── Create controller ─────────────────────────────────────────────────────
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(CtrlPath);

        // ── Parameters ────────────────────────────────────────────────────────────
        ctrl.AddParameter("Speed",  AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("GetHit", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Death",  AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;

        // ── States ────────────────────────────────────────────────────────────────
        var stIdle    = S(sm, "Idle",     idle,     new Vector3(-200,   0));
        var stWalk    = S(sm, "Chase",    walk,     new Vector3(   0,   0));
        var stAtk1H   = S(sm, "Attack",   attack1H, new Vector3( 200,   0));
        var stAtk2H   = S(sm, "Attack2H", attack2H, new Vector3( 200,  90));
        var stGetHit  = S(sm, "GetHit",   getHit,   new Vector3( 400,   0));
        var stDeath   = S(sm, "Dead",     death,    new Vector3( 400,  90));

        sm.defaultState = stIdle;

        // ── Locomotion ────────────────────────────────────────────────────────────
        // Idle → Chase (Speed > 0.1)
        FloatT(stIdle, stWalk, "Speed", 0.10f, isLess: false, dur: 0.15f);
        // Chase → Idle (Speed < 0.05)
        FloatT(stWalk, stIdle, "Speed", 0.05f, isLess: true,  dur: 0.20f);

        // ── Combat from Any State ─────────────────────────────────────────────────
        AnyT(sm, stAtk1H,  "Attack", 0.05f);
        AnyT(sm, stGetHit, "GetHit", 0.05f);
        AnyT(sm, stDeath,  "Death",  0.05f);

        // ── Return after attack / getHit ──────────────────────────────────────────
        ExitT(stAtk1H,  stIdle,   exitTime: 0.90f, dur: 0.10f);
        ExitT(stAtk2H,  stIdle,   exitTime: 0.90f, dur: 0.10f);
        ExitT(stGetHit, stIdle,   exitTime: 0.80f, dur: 0.15f);
        // Dead: no return (EnemyController destroys the object after deathDelay)

        // ── Finalise ──────────────────────────────────────────────────────────────
        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        bool hasTripo = TripoClip("idle") != null;
        string source = hasTripo ? "Tripo AI"
                      : (LoadClip($"{Brb}/Movement/Idle.fbx") != null ? "brbmuffins" : "Blink");
        Debug.Log(
            $"[BCE] EnemyAnimController created ({source} clips) → {CtrlPath}\n" +
            "NEXT:\n" +
            "1. Re-run BCE/Setup/4a–4c to attach Tripo meshes to enemy prefabs.\n" +
            "2. Set Animator → Avatar to a Humanoid avatar on each enemy prefab.\n" +
            "3. EnemyController already drives Speed/Attack/Death — no code changes needed.");

        EditorUtility.DisplayDialog("✅ Enemy AnimController Ready",
            $"EnemyAnimController.controller built ({source} clips).\n\n" +
            "States: Idle → Chase → Attack → GetHit → Dead\n\n" +
            (hasTripo
                ? "Using Tripo AI generated animations ✅\nRe-run 4a/4b/4c to attach meshes."
                : "Tripo clips not found — using " + source + " fallback.\nRun after Tripo pipeline completes for real animations."),
            "Done!");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Looks for the first AnimationClip in the Tripo Grunt output subdirectory for
    /// the given animation name (e.g. "walk", "slash"). Falls back through Ranged/Elite
    /// in case Grunt clips haven't landed yet.
    /// </summary>
    static AnimationClip TripoClip(string animName)
    {
        string[] enemyDirs = { TripoGrunt,
            "Assets/Game/Characters/Enemies/Ranged",
            "Assets/Game/Characters/Enemies/Elite" };

        foreach (string baseDir in enemyDirs)
        {
            string subDir = $"{baseDir}/{animName}";
            if (!AssetDatabase.IsValidFolder(subDir)) continue;

            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { subDir });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = LoadClip(path);
                if (clip != null) return clip;
            }
        }
        return null;
    }

    static AnimationClip Clip(string relativePath, string blinkOverridePath = null)
    {
        string brb   = $"{Brb}/{relativePath}";
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

    static AnimatorState S(AnimatorStateMachine sm, string name, AnimationClip clip, Vector3 pos)
    {
        var s = sm.AddState(name, pos);
        s.motion             = clip;
        s.writeDefaultValues = true;
        return s;
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

    static void AnyT(AnimatorStateMachine sm, AnimatorState to, string trigger, float dur)
    {
        var t = sm.AddAnyStateTransition(to);
        t.hasExitTime         = false;
        t.duration            = dur;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
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
