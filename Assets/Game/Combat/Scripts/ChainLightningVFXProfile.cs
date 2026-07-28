using UnityEngine;

/// <summary>
/// Reusable presentation owned by the Chain Lightning spell rather than by a
/// particular enemy prefab. The Arcane profile is loaded by spell identity.
/// </summary>
[CreateAssetMenu(fileName = "ChainLightning_Arcane", menuName = "BCE/Enemy Abilities/Chain Lightning VFX")]
public sealed class ChainLightningVFXProfile : ScriptableObject
{
    const string ArcaneResourcePath = "EnemyAbilities/ChainLightning_Arcane";

    [Header("Resource Effects")]
    public string impactVFXPath = "FX/dark magic/Lightning attack";

    [Header("Arc")]
    public Color arcColor = new Color(0.35f, 0.8f, 1f, 1f);
    [Min(0.01f)] public float startWidth = 0.09f;
    [Min(0.005f)] public float endWidth = 0.025f;
    [Range(2, 16)] public int segments = 8;
    [Min(0f)] public float jitter = 0.18f;
    [Min(0.05f)] public float lifetime = 0.22f;
    [Min(0f)] public float casterHeightFallback = 1.35f;
    [Min(0f)] public float castOriginForwardOffset = 0.35f;
    [Min(0f)] public float impactScale = 0.65f;

    static ChainLightningVFXProfile _arcane;
    static Material _arcMaterial;

    public static ChainLightningVFXProfile LoadArcane()
    {
        if (_arcane == null)
            _arcane = Resources.Load<ChainLightningVFXProfile>(ArcaneResourcePath);
        return _arcane;
    }

    public void Present(GameObject caster, Vector3[] hitPositions)
    {
        Present(caster, hitPositions, -1);
    }

    public void Present(GameObject caster, Vector3[] hitPositions, int attackVariant)
    {
        if (caster == null || hitPositions == null || hitPositions.Length == 0) return;

        Vector3 from = ResolveCastOrigin(caster, attackVariant);
        for (int i = 0; i < hitPositions.Length; i++)
        {
            Vector3 to = hitPositions[i];
            CreateArc(from, to);
            SpawnImpact(to);
            from = to;
        }
    }

    Vector3 ResolveCastOrigin(GameObject caster, int attackVariant)
    {
        EnemyController enemy = caster.GetComponent<EnemyController>();
        if (enemy != null)
            return attackVariant >= 0
                ? enemy.ResolveAttackVfxOrigin(attackVariant)
                : enemy.ResolveCurrentAttackVfxOrigin();

        Animator animator = caster.GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman)
        {
            Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (hand != null)
                return hand.position + caster.transform.forward * castOriginForwardOffset;

            Transform chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (chest != null)
                return chest.position + caster.transform.forward * castOriginForwardOffset;
        }
        return caster.transform.position
            + Vector3.up * casterHeightFallback
            + caster.transform.forward * castOriginForwardOffset;
    }

    void CreateArc(Vector3 from, Vector3 to)
    {
        GameObject arc = new GameObject("ChainLightning_Arcane_Arc");
        LineRenderer line = arc.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = Mathf.Max(2, segments);
        line.startWidth = startWidth;
        line.endWidth = endWidth;
        line.startColor = arcColor;
        line.endColor = new Color(arcColor.r, arcColor.g, arcColor.b, 0.15f);
        line.numCornerVertices = 2;
        line.numCapVertices = 2;
        line.material = GetArcMaterial();

        Vector3 direction = to - from;
        for (int i = 0; i < line.positionCount; i++)
        {
            float t = i / (float)(line.positionCount - 1);
            Vector3 point = Vector3.Lerp(from, to, t);
            if (i > 0 && i < line.positionCount - 1)
            {
                float envelope = Mathf.Sin(t * Mathf.PI);
                point += Random.insideUnitSphere * (jitter * envelope);
            }
            line.SetPosition(i, point);
        }

        Destroy(arc, lifetime);
    }

    void SpawnImpact(Vector3 position)
    {
        if (string.IsNullOrWhiteSpace(impactVFXPath)) return;
        GameObject prefab = Resources.Load<GameObject>(impactVFXPath);
        if (prefab == null) return;

        GameObject impact = Instantiate(prefab, position, Quaternion.identity);
        impact.transform.localScale *= impactScale;
        Destroy(impact, Mathf.Max(2f, lifetime));
    }

    static Material GetArcMaterial()
    {
        if (_arcMaterial != null) return _arcMaterial;
        Shader shader = Shader.Find("Sprites/Default");
        _arcMaterial = new Material(shader) { name = "ChainLightning_Arcane_Runtime" };
        return _arcMaterial;
    }
}
