using UnityEngine;

[CreateAssetMenu(
    fileName = "ElementalLightning",
    menuName = "BCE/Enemy Abilities/Elemental Lightning VFX")]
public sealed class ElementalLightningVFXProfile : ScriptableObject
{
    [Header("Composite Effects")]
    [Tooltip("Effect spawned at the cast origin for the full wind-up.")]
    public GameObject castEffect;
    public GameObject handEffect;
    public GameObject spellEffect;
    public GameObject hitEffect;

    [Header("Presentation")]
    [Min(0.01f)] public float castScale = 1f;
    [Min(0.01f)] public float castThickness = 1f;
    [Tooltip("Zero follows the configured attack impact timing.")]
    [Min(0f)] public float castLifetime;
    [Min(0.01f)] public float handScale = 0.45f;
    [Min(0.01f)] public float handThickness = 1f;
    [Tooltip("Offsets hand-effect appearance from spell delivery without changing playback speed. Zero appears at delivery; negative values appear later and positive values appear earlier.")]
    [Range(-3f, 3f)] public float handAppearanceTiming;
    [Tooltip("Zero follows the configured attack impact timing.")]
    [Min(0f)] public float handLifetime;
    [Min(0.01f)] public float spellScale = 0.75f;
    [Min(0.01f)] public float spellThickness = 1f;
    [Tooltip("Zero follows the configured attack impact timing.")]
    [Min(0f)] public float spellLifetime;
    [Min(0.01f)] public float hitScale = 0.7f;
    [Min(0.01f)] public float hitThickness = 1f;
    [Min(0.05f)] public float hitLifetime = 1.5f;

    public void PresentCast(GameObject caster, Vector3 targetPosition,
        int attackVariant, float castDuration)
    {
        if (caster == null) return;
        float defaultLifetime = Mathf.Max(0.1f, castDuration + 0.15f);
        EnemyController enemy = caster.GetComponent<EnemyController>();
        Vector3 origin = enemy != null
            ? enemy.ResolveAttackVfxOrigin(attackVariant)
            : caster.transform.position + caster.transform.up * 1.2f;

        SpawnTimed(castEffect, origin, caster.transform.rotation,
            castScale, castThickness,
            castLifetime > 0f ? castLifetime : defaultLifetime,
            null, false);
        float handDelayFromAttackStart =
            Mathf.Max(0f, castDuration - handAppearanceTiming);
        SpawnTimed(handEffect, origin, caster.transform.rotation,
            handScale, handThickness,
            handLifetime > 0f ? handLifetime : defaultLifetime,
            caster.transform, false, -handDelayFromAttackStart, attackVariant);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            $"[ElementalHandTiming] PresentCast prefab=" +
            $"{(handEffect != null ? handEffect.name : "None")} " +
            $"setting={handAppearanceTiming:0.###} variant={attackVariant} " +
            $"castImpact={castDuration:0.###} " +
            $"appearanceAt={handDelayFromAttackStart:0.###} " +
            $"realtime={Time.realtimeSinceStartup:0.###}",
            caster);
#endif
    }

    public void PresentHit(Vector3 targetPosition)
    {
        SpawnTimed(spellEffect, targetPosition, Quaternion.identity,
            spellScale, spellThickness,
            spellLifetime > 0f ? spellLifetime : 0.75f,
            null, true);
        SpawnTimed(hitEffect, targetPosition, Quaternion.identity,
            hitScale, hitThickness, hitLifetime, null, true);
    }

    internal static void SpawnTimed(GameObject prefab, Vector3 position, Quaternion rotation,
        float scale, float thickness, float lifetime, Transform parent, bool singleShot,
        float appearanceTiming = 0f, int attackVariant = -1)
    {
        if (prefab == null) return;
        GameObject instance = Instantiate(prefab, position, rotation);
        if (parent != null) instance.transform.SetParent(parent, true);
        if (parent != null && attackVariant >= 0)
        {
            var follower = instance.AddComponent<AttackVfxOriginFollower>();
            follower.Initialize(parent.gameObject, attackVariant);
        }
        Vector3 sized = instance.transform.localScale * Mathf.Max(0.01f, scale);
        float width = Mathf.Max(0.01f, thickness);
        sized.x *= width;
        sized.z *= width;
        instance.transform.localScale = sized;
        if (singleShot)
        {
            foreach (ParticleSystem particles in
                instance.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particles.main;
                main.loop = false;
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.Play(true);
            }
        }
        ParticleSystem[] particleSystems =
            instance.GetComponentsInChildren<ParticleSystem>(true);
        if (appearanceTiming < 0f)
        {
            // Delay the whole prefab rather than only its ParticleSystems. Some
            // spell prefabs also contain Visual Effect Graphs or renderers that
            // otherwise become visible immediately and get ahead of the preview.
            Animator animator = parent != null
                ? parent.GetComponentInChildren<Animator>()
                : null;
            DelayedEffectActivator.Schedule(
                instance, -appearanceTiming, lifetime, animator, attackVariant);
        }
        else if (appearanceTiming > 0f)
        {
            // Match the preview's advanced timeline without changing simulation speed.
            foreach (ParticleSystem particles in particleSystems)
            {
                particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.Simulate(appearanceTiming, false, true, true);
                particles.Play(false);
            }
        }
        if (appearanceTiming >= 0f)
            Destroy(instance, Mathf.Max(0.05f, lifetime));
    }
}

sealed class AttackVfxOriginFollower : MonoBehaviour
{
    GameObject caster;
    Animator casterAnimator;
    Renderer[] casterRenderers;
    Vector3 offset;

    public void Initialize(GameObject caster, int attackVariant)
    {
        this.caster = caster;
        if (caster != null)
        {
            casterAnimator = caster.GetComponentInChildren<Animator>();
            EnemyController enemy = caster.GetComponent<EnemyController>();
            if (enemy != null && enemy.attackVfxOffsets != null &&
                attackVariant >= 0 &&
                attackVariant < enemy.attackVfxOffsets.Length)
                offset = enemy.attackVfxOffsets[attackVariant];

            Renderer[] allRenderers =
                caster.GetComponentsInChildren<Renderer>(true);
            var modelRenderers = new System.Collections.Generic.List<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                if (renderer != null &&
                    !renderer.transform.IsChildOf(transform))
                    modelRenderers.Add(renderer);
            }
            casterRenderers = modelRenderers.ToArray();
        }
        UpdateOrigin();
    }

    void LateUpdate() => UpdateOrigin();

    void UpdateOrigin()
    {
        if (caster == null) return;
        transform.position = EnemyController.ResolveAttackVfxOrigin(
            caster.transform, casterAnimator, casterRenderers, offset);
        transform.rotation = caster.transform.rotation;
    }
}

sealed class DelayedEffectActivator : MonoBehaviour
{
    GameObject target;
    Animator animator;
    float remaining;
    float effectLifetime;
    int expectedAttackStateHash;
    int initialStateHash;
    float initialNormalizedTime;
    bool waitingForAnimation;
    bool attackCycleConfirmed;
    float activationAnimationSeconds;

    public static void Schedule(
        GameObject target, float delay, float lifetime,
        Animator animator, int attackVariant)
    {
        if (target == null || delay <= 0f) return;

        target.SetActive(false);
        var scheduler = new GameObject($"{target.name} Appearance Delay");
        var activator = scheduler.AddComponent<DelayedEffectActivator>();
        activator.target = target;
        activator.animator = animator;
        activator.remaining = delay;
        activator.effectLifetime = Mathf.Max(0.05f, lifetime);
        activator.waitingForAnimation =
            animator != null && animator.isActiveAndEnabled &&
            attackVariant >= 0;
        if (activator.waitingForAnimation)
        {
            string attackStateName = attackVariant == 0
                ? "Attack"
                : $"Attack {attackVariant + 1}";
            activator.expectedAttackStateHash =
                Animator.StringToHash(attackStateName);
            AnimatorStateInfo initial =
                animator.GetCurrentAnimatorStateInfo(0);
            activator.initialStateHash = initial.shortNameHash;
            activator.initialNormalizedTime = initial.normalizedTime;
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            $"[ElementalHandTiming] Scheduled prefab={target.name} " +
            $"delay={delay:0.###} expectedState={activator.expectedAttackStateHash} " +
            $"initialState={activator.initialStateHash} " +
            $"initialNormalized={activator.initialNormalizedTime:0.###} " +
            $"realtime={Time.realtimeSinceStartup:0.###}",
            animator);
#endif
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (waitingForAnimation)
        {
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
            bool beganInExpectedState =
                initialStateHash == expectedAttackStateHash;
            bool restartedExpectedState =
                current.shortNameHash == expectedAttackStateHash &&
                (!beganInExpectedState ||
                 current.normalizedTime + 0.01f < initialNormalizedTime);
            AnimatorStateInfo attackTimeline = current;
            bool hasAttackTimeline = false;
            attackCycleConfirmed |= restartedExpectedState;

            if (animator.IsInTransition(0))
            {
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(0);
                if (next.shortNameHash == expectedAttackStateHash)
                {
                    attackTimeline = next;
                    hasAttackTimeline = true;
                    attackCycleConfirmed = true;
                }
            }

            if (!hasAttackTimeline &&
                current.shortNameHash == expectedAttackStateHash &&
                attackCycleConfirmed)
            {
                attackTimeline = current;
                hasAttackTimeline = true;
            }

            if (!hasAttackTimeline) return;

            // Enemy Forge displays played animation seconds. Drive activation
            // from that same position in the live Animator instead of a second
            // wall-clock timer that can drift ahead of the visible animation.
            float effectiveSpeed = Mathf.Max(
                0.0001f,
                Mathf.Abs(attackTimeline.speed *
                          attackTimeline.speedMultiplier));
            float animationSeconds =
                Mathf.Max(0f, attackTimeline.normalizedTime) *
                Mathf.Max(0.0001f, attackTimeline.length) /
                effectiveSpeed;
            if (animationSeconds + 0.001f < remaining) return;

            activationAnimationSeconds = animationSeconds;
            waitingForAnimation = false;
            remaining = 0f;
        }

        remaining -= Time.deltaTime;
        if (remaining > 0f) return;

        target.SetActive(true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log(
            $"[ElementalHandTiming] Activated prefab={target.name} " +
            $"animationSeconds={activationAnimationSeconds:0.###} " +
            $"realtime={Time.realtimeSinceStartup:0.###}",
            animator);
#endif
        Destroy(target, effectLifetime);
        Destroy(gameObject);
    }
}
