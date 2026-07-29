using UnityEngine;

[CreateAssetMenu(
    fileName = "ElementalLightning",
    menuName = "BCE/Enemy Abilities/Elemental Lightning VFX")]
public sealed class ElementalLightningVFXProfile : ScriptableObject
{
    [Header("Composite Effects")]
    public GameObject handEffect;
    public GameObject spellEffect;
    public GameObject hitEffect;

    [Header("Presentation")]
    [Min(0.01f)] public float handScale = 0.45f;
    [Min(0.01f)] public float handThickness = 1f;
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

        SpawnTimed(handEffect, origin, caster.transform.rotation,
            handScale, handThickness,
            handLifetime > 0f ? handLifetime : defaultLifetime,
            caster.transform, false);
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

    static void SpawnTimed(GameObject prefab, Vector3 position, Quaternion rotation,
        float scale, float thickness, float lifetime, Transform parent, bool singleShot)
    {
        if (prefab == null) return;
        GameObject instance = Instantiate(prefab, position, rotation);
        if (parent != null) instance.transform.SetParent(parent, true);
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
        Destroy(instance, Mathf.Max(0.05f, lifetime));
    }
}
