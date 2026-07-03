using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  EnemyDeathVFX
//  Placed on enemy GameObjects by RodCombatWorldBuilder.
//  Stores the AssetDatabase path of a death VFX prefab; loads and spawns it
//  when Health fires onDeath.
//
//  Uses Resources.Load as a runtime fallback when AssetDatabase is unavailable
//  (i.e. in a build). For builds, copy the VFX prefabs into a Resources/ folder.
// ─────────────────────────────────────────────────────────────────────────────

[DisallowMultipleComponent]
public class EnemyDeathVFX : MonoBehaviour
{
    // Set by RodCombatWorldBuilder at edit time (full asset path).
    [HideInInspector] public string vfxPath;

    // Optional direct reference for builds (drag in Inspector).
    public GameObject vfxPrefabOverride;

    void Start()
    {
        var health = GetComponent<Health>();
        if (health != null)
            health.onDeath.AddListener(OnDied);
    }

    void OnDied()
    {
        SpawnFX();
        Destroy(gameObject, 0.1f);
    }

    void SpawnFX()
    {
        GameObject prefab = vfxPrefabOverride;

#if UNITY_EDITOR
        if (prefab == null && !string.IsNullOrEmpty(vfxPath))
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(vfxPath);
#endif

        if (prefab != null)
        {
            var fx = Instantiate(prefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(fx, 3f);
        }
        else
        {
            // Procedural fallback — burst particles when no VFX prefab is assigned.
            // Looks fine in-engine for grey-box testing; replace with real prefab for ship.
            SpawnProceduralBurst();
        }
    }

    void SpawnProceduralBurst()
    {
        var go = new GameObject("DeathBurst_Procedural");
        go.transform.position = transform.position + Vector3.up * 0.6f;
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.duration        = 0.3f;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(2.5f, 7f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.12f, 0.45f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(0.90f, 0.25f, 0.10f, 1f),  // crimson
            new Color(0.25f, 0.05f, 0.05f, 0.8f) // dark red
        );
        main.gravityModifier = 0.4f;
        main.maxParticles    = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 40) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.3f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.y = new ParticleSystem.MinMaxCurve(0f, 2.5f); // upward drift

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.black, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var psr = go.GetComponent<ParticleSystemRenderer>();
        var shader = Shader.Find("Sprites/Default");
        if (shader != null) psr.material = new Material(shader);

        ps.Play();
        Destroy(go, 2.5f);
    }
}
