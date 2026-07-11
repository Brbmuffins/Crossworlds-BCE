using UnityEngine;

public class CastAnimator : MonoBehaviour
{
    // In your AnimatorController create 3 triggers: CastDamage, CastHeal, CastSupport
    // Each trigger transitions from Any State into its own animation state.

    const float CancelBlendTime = 0.08f;

    private Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    public void PlayCast(AbilityCategory category)
    {
        EnsureAnimator();
        if (anim == null) return;

        switch (category)
        {
            case AbilityCategory.Heal:    anim.SetTrigger("CastHeal");    break;
            case AbilityCategory.Support: anim.SetTrigger("CastSupport"); break;
            default:                      anim.SetTrigger("CastDamage");  break;
        }
    }

    public void CancelCast(bool preferMovementState)
    {
        EnsureAnimator();
        if (anim == null) return;

        ResetTriggerIfPresent("CastDamage");
        ResetTriggerIfPresent("CastHeal");
        ResetTriggerIfPresent("CastSupport");
        ResetTriggerIfPresent("CastTwoHanded");
        ResetTriggerIfPresent("Cast");

        string targetState = preferMovementState
            ? FirstAvailableState("Run", "Sprint", "Running", "Rifle Run")
            : FirstAvailableState("IdleCombat", "Idle", "Idle Stand", "Rifle Idle");

        if (!string.IsNullOrEmpty(targetState))
            anim.CrossFadeInFixedTime(targetState, CancelBlendTime, 0, 0f);
    }

    void EnsureAnimator()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    void ResetTriggerIfPresent(string triggerName)
    {
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
