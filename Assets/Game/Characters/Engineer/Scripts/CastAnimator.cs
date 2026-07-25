using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class CastAnimator : MonoBehaviour
{
    // In your AnimatorController create 3 triggers: CastDamage, CastHeal, CastSupport
    // Each trigger transitions from Any State into its own animation state.

    const float CancelBlendTime = 0.08f;

    private Animator anim;
    private string activeCastTrigger;
    private PlayableGraph activeClipGraph;
    private Coroutine activeClipRoutine;
    private int activeClipPlayId;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    public void PlayCast(
        AbilityCategory category,
        AnimationClip animationClip = null,
        float playbackSpeed = 1f)
    {
        EnsureAnimator();
        if (anim == null) return;

        if (animationClip != null)
        {
            PlayAnimationClip(
                animationClip,
                playbackSpeed);
            return;
        }

        StopActiveClipPlayback();

        string triggerName = ResolveCategoryTrigger(category);
        if (!HasTrigger(triggerName))
        {
            activeCastTrigger = null;
            return;
        }

        ResetTriggerIfPresent(activeCastTrigger);
        anim.SetTrigger(triggerName);
        activeCastTrigger = triggerName;
    }

    public void CancelCast(bool preferMovementState)
    {
        EnsureAnimator();
        if (anim == null) return;

        bool hadActiveCast =
            !string.IsNullOrEmpty(activeCastTrigger) ||
            activeClipGraph.IsValid();

        StopActiveClipPlayback();
        ResetTriggerIfPresent(activeCastTrigger);
        ResetTriggerIfPresent("CastDamage");
        ResetTriggerIfPresent("CastHeal");
        ResetTriggerIfPresent("CastSupport");
        ResetTriggerIfPresent("CastTwoHanded");
        ResetTriggerIfPresent("Cast");
        activeCastTrigger = null;

        if (!hadActiveCast)
            return;

        string targetState = preferMovementState
            ? FirstAvailableState("Run", "Sprint", "Running", "Rifle Run")
            : FirstAvailableState("IdleCombat", "Idle", "Idle Stand", "Rifle Idle");

        if (!string.IsNullOrEmpty(targetState))
            anim.CrossFadeInFixedTime(targetState, CancelBlendTime, 0, 0f);
    }

    void OnDisable()
    {
        StopActiveClipPlayback();
    }

    void OnDestroy()
    {
        StopActiveClipPlayback();
    }

    void PlayAnimationClip(
        AnimationClip clip,
        float playbackSpeed)
    {
        StopActiveClipPlayback();
        ResetTriggerIfPresent(activeCastTrigger);
        activeCastTrigger = null;

        activeClipGraph = PlayableGraph.Create($"{name} Ability Cast");
        activeClipGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        AnimationClipPlayable clipPlayable =
            AnimationClipPlayable.Create(activeClipGraph, clip);
        float safePlaybackSpeed =
            Mathf.Max(0.01f, playbackSpeed);
        clipPlayable.SetSpeed(safePlaybackSpeed);
        clipPlayable.SetApplyFootIK(true);
        clipPlayable.SetApplyPlayableIK(true);

        AnimationPlayableOutput output =
            AnimationPlayableOutput.Create(activeClipGraph, "Ability Cast", anim);
        output.SetSourcePlayable(clipPlayable);

        activeClipGraph.Play();

        int playId = ++activeClipPlayId;
        activeClipRoutine = StartCoroutine(
            FinishClipPlaybackAfter(
                Mathf.Max(
                    0.01f,
                    clip.length / safePlaybackSpeed),
                playId));
    }

    IEnumerator FinishClipPlaybackAfter(float duration, int playId)
    {
        yield return new WaitForSeconds(duration);

        if (playId != activeClipPlayId)
            yield break;

        activeClipRoutine = null;
        StopActiveClipPlayback();
    }

    void StopActiveClipPlayback()
    {
        activeClipPlayId++;

        if (activeClipRoutine != null)
        {
            StopCoroutine(activeClipRoutine);
            activeClipRoutine = null;
        }

        if (activeClipGraph.IsValid())
            activeClipGraph.Destroy();
    }

    void EnsureAnimator()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    static string ResolveCategoryTrigger(AbilityCategory category)
    {
        switch (category)
        {
            case AbilityCategory.Heal:    return "CastHeal";
            case AbilityCategory.Support: return "CastSupport";
            default:                      return "CastDamage";
        }
    }

    bool HasTrigger(string triggerName)
    {
        if (string.IsNullOrEmpty(triggerName) ||
            anim == null ||
            anim.runtimeAnimatorController == null)
            return false;

        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger &&
                parameter.name == triggerName)
                return true;
        }

        return false;
    }

    void ResetTriggerIfPresent(string triggerName)
    {
        if (string.IsNullOrEmpty(triggerName))
            return;

        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger &&
                parameter.name == triggerName)
            {
                anim.ResetTrigger(triggerName);
                return;
            }
        }
    }

    string FirstAvailableState(params string[] stateNames)
    {
        foreach (string stateName in stateNames)
        {
            string fullPath = "Base Layer." + stateName;
            if (anim.HasState(0, Animator.StringToHash(fullPath)))
                return fullPath;

            if (anim.HasState(0, Animator.StringToHash(stateName)))
                return stateName;
        }

        return null;
    }
}
