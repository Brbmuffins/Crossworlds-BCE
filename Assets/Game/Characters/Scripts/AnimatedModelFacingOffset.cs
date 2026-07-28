using UnityEngine;

/// <summary>
/// Applies a visual-only yaw correction after Animator evaluation.
/// The gameplay root keeps Unity's +Z forward for navigation, combat, and networking.
/// </summary>
[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public sealed class AnimatedModelFacingOffset : MonoBehaviour
{
    [SerializeField] Transform animatedRoot;
    [SerializeField] string animatedRootName = "Armature";
    [SerializeField] float yawOffset = 90f;

    Quaternion _offset;
    bool _offsetApplied;

    void Awake()
    {
        _offset = Quaternion.Euler(0f, yawOffset, 0f);
        ResolveAnimatedRoot();
    }

    void Update()
    {
        RemoveOffset();
    }

    void LateUpdate()
    {
        if (animatedRoot == null)
            ResolveAnimatedRoot();
        if (animatedRoot == null)
            return;

        animatedRoot.localRotation = _offset * animatedRoot.localRotation;
        _offsetApplied = true;
    }

    void OnDisable()
    {
        RemoveOffset();
    }

    void ResolveAnimatedRoot()
    {
        var animator = GetComponentInChildren<Animator>(true);
        if (animator == null)
            return;

        foreach (var child in animator.GetComponentsInChildren<Transform>(true))
        {
            if (child == animator.transform || child.name != animatedRootName)
                continue;

            animatedRoot = child;
            return;
        }
    }

    void RemoveOffset()
    {
        if (!_offsetApplied || animatedRoot == null)
            return;

        animatedRoot.localRotation = Quaternion.Inverse(_offset) * animatedRoot.localRotation;
        _offsetApplied = false;
    }
}
