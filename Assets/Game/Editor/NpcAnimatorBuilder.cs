#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// NpcAnimatorBuilder — BCE/Setup/5b ▶ Create NPC AnimController
///
/// Builds Assets/Game/Animations/NpcAnimController.controller for ambient
/// hub NPCs (ForgeNPC, HangmanNPC, vendor NPCs) driven by NpcController.
///
/// State machine:
///   Idle ←→ Walk  (driven by float "Speed": 0=idle, >0.1=walk)
///
/// Clip priority: brbmuffins Sword Art → Blink → built-in fallback.
/// After running:
///   1. Assign NpcAnimController to the Animator on each NPC prefab/object.
///   2. Set Animator → Avatar to a Humanoid avatar matching the NPC mesh.
///   3. NpcController already drives Speed — no code changes needed.
/// </summary>
public static class NpcAnimatorBuilder
{
    const string OutDir   = "Assets/Game/Animations";
    const string CtrlPath = "Assets/Game/Animations/NpcAnimController.controller";

    const string Brb   = "Assets/brbmuffins Swords/brbmuffins Sword Art/Animations/Animations_Starter_Pack";
    const string Blink = "Assets/Blink/Art/Animations";

    [MenuItem("BCE/Setup/5b ▶ Create NPC AnimController", priority = 5)]
    public static void CreateNpcAnimController()
    {
        EnsureDir(OutDir);

        AnimationClip idle = Clip("Movement/Idle.fbx");
        AnimationClip walk = Clip("Movement/Walk.fbx") ?? Clip("Movement/RunForward.fbx");

        if (idle == null)
        {
            Debug.LogError(
                "[BCE] No Idle.fbx found for NPC controller.\n" +
                "Check: " + Brb + "/Movement/Idle.fbx\n" +
                "Or:    " + Blink + "/Movement/Idle.fbx");
            return;
        }

        if (walk == null) walk = idle;

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(CtrlPath);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);

        var sm = ctrl.layers[0].stateMachine;

        var stIdle = S(sm, "Idle", idle, new Vector3(-150, 0));
        var stWalk = S(sm, "Walk", walk, new Vector3( 150, 0));
        sm.defaultState = stIdle;

        FloatT(stIdle, stWalk, "Speed", 0.10f, isLess: false, dur: 0.20f);
        FloatT(stWalk, stIdle, "Speed", 0.05f, isLess: true,  dur: 0.25f);

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string source = LoadClip($"{Brb}/Movement/Idle.fbx") != null ? "brbmuffins" : "Blink";
        Debug.Log(
            $"[BCE] NpcAnimController created ({source} clips) → {CtrlPath}\n" +
            "NEXT:\n" +
            "1. Assign NpcAnimController to Animator on each NPC in Hub.unity.\n" +
            "2. Set Animator → Avatar to a Humanoid avatar matching the NPC mesh.\n" +
            "3. NpcController.Update() drives Speed automatically — no code changes.");

        EditorUtility.DisplayDialog("✅ NPC AnimController Ready",
            $"NpcAnimController.controller built ({source} clips).\n\n" +
            "States: Idle ↔ Walk (driven by NpcController.Speed)\n\n" +
            "Assign to ForgeNPC, HangmanNPC, and any ambient hub NPCs.",
            "Done!");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static AnimationClip Clip(string relative, string blinkOverride = null)
    {
        string brb   = $"{Brb}/{relative}";
        string blink = $"{Blink}/{(blinkOverride ?? relative)}";
        return LoadClip(brb) ?? LoadClip(blink);
    }

    static AnimationClip LoadClip(string fbxPath)
    {
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        if (all == null || all.Length == 0) return null;
        return all.OfType<AnimationClip>()
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
