#if !UNITY_SERVER
using System.Collections;
using UnityEngine;

/// <summary>
/// FloatingDamageText — spawns world-space damage numbers that float up and fade.
///
/// Usage (call from ClientRpc or client-side damage callbacks):
///   FloatingDamageText.Spawn(worldPosition, amount, type);
///
/// Types: Normal, Critical, Heal, HealCrit, Shield, TriageReturn, Miss
/// Pool of 32 text objects; reused automatically via generation tokens.
/// When a slot is recycled mid-animation the old coroutine self-terminates.
/// </summary>
public class FloatingDamageText : MonoBehaviour
{
    // ── Public types ──────────────────────────────────────────────────────────
    public enum DamageType { Normal, Critical, Heal, Miss, HealCrit, Shield, TriageReturn }

    // ── Singleton / pool ──────────────────────────────────────────────────────
    static FloatingDamageText _instance;
    const int PoolSize = 32;
    GameObject[] _pool;
    TextMesh[]   _meshes;
    int[]        _gen;       // generation token per slot — incremented on recycle
    int          _poolHead;

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;
        var go = new GameObject("[FloatingDamageText]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<FloatingDamageText>();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        _pool     = new GameObject[PoolSize];
        _meshes   = new TextMesh[PoolSize];
        _gen      = new int[PoolSize];
        _poolHead = 0;

        for (int i = 0; i < PoolSize; i++)
        {
            var obj = new GameObject($"DmgText_{i}");
            obj.transform.SetParent(transform, false);
            obj.SetActive(false);
            DontDestroyOnLoad(obj);

            var tm = obj.AddComponent<TextMesh>();
            tm.characterSize = 0.12f;
            tm.fontSize      = 48;
            tm.fontStyle     = FontStyle.Bold;
            tm.anchor        = TextAnchor.MiddleCenter;
            tm.alignment     = TextAlignment.Center;

            // Use RodBillboard if available, else built-in fallback
            var bbType = System.Type.GetType("RodBillboard");
            if (bbType != null) obj.AddComponent(bbType);
            else obj.AddComponent<DmgTextBillboard>();

            _pool[i]   = obj;
            _meshes[i] = tm;
        }
    }

    // ── Static API ────────────────────────────────────────────────────────────
    public static void Spawn(Vector3 worldPos, float amount, DamageType type = DamageType.Normal,
                             string textOverride = null)
    {
        if (_instance == null) return;
        _instance.SpawnInternal(worldPos, amount, type, textOverride);
    }

    public static void Spawn(Vector3 worldPos, int amount, DamageType type = DamageType.Normal,
                             string textOverride = null)
        => Spawn(worldPos, (float)amount, type, textOverride);

    // ── Internal spawn ────────────────────────────────────────────────────────
    void SpawnInternal(Vector3 worldPos, float amount, DamageType type, string textOverride)
    {
        int idx = _poolHead;
        _poolHead = (_poolHead + 1) % PoolSize;

        _gen[idx]++;
        int myGen = _gen[idx];

        var obj  = _pool[idx];
        var mesh = _meshes[idx];
        obj.SetActive(false);

        switch (type)
        {
            case DamageType.Critical:
                mesh.text          = textOverride ?? $"{Mathf.RoundToInt(amount)}!";
                mesh.color         = new Color(1.00f, 0.35f, 0.05f);
                mesh.characterSize = 0.18f;
                break;
            case DamageType.Heal:
                mesh.text          = textOverride ?? $"+{Mathf.RoundToInt(amount)}";
                mesh.color         = new Color(0.20f, 0.90f, 0.30f);
                mesh.characterSize = 0.12f;
                break;
            case DamageType.HealCrit:
                mesh.text          = textOverride ?? $"+{Mathf.RoundToInt(amount)}!";
                mesh.color         = new Color(0.40f, 1.00f, 0.40f);
                mesh.characterSize = 0.16f;
                break;
            case DamageType.Shield:
                mesh.text          = textOverride ?? $"[{Mathf.RoundToInt(amount)}]";
                mesh.color         = new Color(0.40f, 0.70f, 1.00f);
                mesh.characterSize = 0.11f;
                break;
            case DamageType.TriageReturn:
                mesh.text          = textOverride ?? $"+{Mathf.RoundToInt(amount)}";
                mesh.color         = new Color(1.00f, 0.80f, 0.20f);
                mesh.characterSize = 0.09f;
                break;
            case DamageType.Miss:
                mesh.text          = textOverride ?? "MISS";
                mesh.color         = new Color(0.75f, 0.75f, 0.75f);
                mesh.characterSize = 0.10f;
                break;
            default:
                mesh.text          = textOverride ?? Mathf.RoundToInt(amount).ToString();
                mesh.color         = new Color(1.00f, 0.92f, 0.80f);
                mesh.characterSize = 0.12f;
                break;
        }

        obj.transform.position = worldPos + new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range( 0.8f, 1.4f),
            Random.Range(-0.2f, 0.2f));

        float startScale = type switch
        {
            DamageType.HealCrit     => 1.4f,
            DamageType.Heal         => 1.1f,
            DamageType.TriageReturn => 0.75f,
            _                       => 1.0f
        };
        obj.transform.localScale = Vector3.one * startScale;
        obj.SetActive(true);

        bool isHealType = type == DamageType.Heal || type == DamageType.HealCrit;
        bool isCrit     = type == DamageType.Critical || type == DamageType.HealCrit;
        float riseOverride = isHealType ? 2.0f : (type == DamageType.TriageReturn ? 1.0f : 0f);

        StartCoroutine(Animate(obj, mesh, idx, myGen, isCrit, riseOverride));
    }

    IEnumerator Animate(GameObject obj, TextMesh mesh, int idx, int gen,
                        bool isCrit, float riseOverride = 0f)
    {
        float lifetime  = isCrit ? 1.4f : 1.0f;
        float riseSpeed = riseOverride > 0f ? riseOverride : (isCrit ? 2.5f : 1.6f);
        Vector3 startPos  = obj.transform.position;
        Color   startCol  = mesh.color;
        Vector3 baseScale = obj.transform.localScale;

        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            // Stale slot — another spawn recycled this index
            if (_gen[idx] != gen) yield break;

            float t = elapsed / lifetime;

            // Rise
            obj.transform.position = startPos + Vector3.up * (riseSpeed * elapsed);

            // Crit punch: scale up fast then settle
            if (isCrit)
            {
                float punchT = Mathf.Clamp01(t / 0.12f);
                float settle = Mathf.Clamp01((t - 0.12f) / 0.18f);
                float s = punchT < 1f
                    ? Mathf.Lerp(1f, 1.5f, punchT)
                    : Mathf.Lerp(1.5f, 1f, settle);
                obj.transform.localScale = baseScale * s;
            }

            // Fade out last 35% of lifetime
            float fadeStart = 0.65f;
            float alpha = t < fadeStart ? 1f
                        : Mathf.Lerp(1f, 0f, (t - fadeStart) / (1f - fadeStart));
            mesh.color = new Color(startCol.r, startCol.g, startCol.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.SetActive(false);
    }
}

/// <summary>
/// DmgTextBillboard — Lightweight camera-facing component for floating damage text.
/// Used automatically by FloatingDamageText when RodBillboard isn't in the project.
/// Runs client-side only (FloatingDamageText is already inside #if !UNITY_SERVER).
/// </summary>
public class DmgTextBillboard : MonoBehaviour
{
    Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    void LateUpdate()
    {
        if (_cam == null) { _cam = Camera.main; return; }
        transform.forward = _cam.transform.forward;
    }
}
#endif
