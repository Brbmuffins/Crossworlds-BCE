using UnityEngine;

/// <summary>
/// Ensures the Brontosaurus' embedded Generic-rig walk state starts and keeps
/// evaluating when the creature is enabled or returned from animator culling.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class BrontosaurusAnimationDriver : MonoBehaviour
{
    static readonly int WalkState = Animator.StringToHash("Walk");

    Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    void OnEnable()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[BCE] Brontosaurus has no Animator Controller.", this);
            return;
        }

        _animator.enabled = true;
        _animator.Rebind();
        _animator.Update(0f);
        _animator.Play(WalkState, 0, 0f);
    }
}
