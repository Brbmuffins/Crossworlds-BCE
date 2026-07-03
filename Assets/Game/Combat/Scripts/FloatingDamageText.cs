using System.Collections;
using UnityEngine;

/// <summary>
/// FloatingDamageText — spawns world-space damage numbers that float up and fade.
///
/// Usage (call from ClientRpc or client-side damage callbacks):
///   FloatingDamageText.Spawn(worldPosition, amount, type);
///
/// Types: Normal, Critical, Heal, Miss
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

    GameObject[]    _pool;
    TextMesh[]      _meshes;
    int[]           _gen;       // generation token per slot — incremented on recycle
    int             _poolHead;

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

            // RodBillboard faces camera if available; else inline fallback
            var bbType = System.Type.GetType("RodBillboard");
            if (bbType != null) obj.AddComponent(bbType);
            else obj.AddComponent<DmgTextBillboard>();

            _pool[i]   = obj;
            _meshes[i] = tm;
        }
    }

    // ── Static API ────────────────────────────────────────────────────────────

    /// <summary>Spawn a floating damage number at a world position.</summary>
    public static void Spawn(Vector3 worldPos, float amount, DamageType type = DamageType.Normal,
                             string textOverride = null)
    {
        if (_instance == null) return;
        _instance.SpawnInternal(worldPos, amount, type, textOverride);
    }

    /// <summary>Integer overload.</summary>
    public static void Spawn(Vector3 worldPos, int amount, DamageType type = DamageType.Normal,
                             string textOverride = null)
        => Spawn(worldPos, (float)amount, type, textOverride);

    // ── Internal spawn ────────────────────────────────────────────────────────
    void SpawnInternal(Vector3 worldPos, float amount, DamageType type, string textOverride = null)
    {
        int idx = _poolHead;
        _poolHead = (_poolHead + 1) % PoolSize;

        // Invalidate any running animation on this slot
        _gen[idx]++;
        int myGen = _gen[idx];

        var obj  = _pool[idx];
        var mesh = _meshes[idx];
        obj.SetActive(false);

        // Configure text content & colour
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

        // Position with slight random scatter
        obj.transform.position = worldPos + new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range( 0.8f, 1.4f),
            Random.Range(-0.2f, 0.2f));

        // Type-specific scale
        float startScale = type switch
        {
            DamageType.HealCrit     => 1.4f,
            DamageType.Heal         => 1.1f,
            DamageType.TriageReturn => 0.75f,
            _                       => 1.0f
        };
        obj.transform.localScale = Vector3.one * startScale;
        obj.SetActive(true);

        // Heal types drift upward faster; TriageReturn is subtle
        bool isCrit       = type == DamageType.Critical || type == DamageType.HealCrit;
        bool isHealType   = type == DamageType.Heal || type == DamageType.HealCrit;
        float riseOverride = isHealType ? 2.0f : (type == DamageType.TriageReturn ? 1.0f : 0f);

        StartCoroutine(Animate(obj, mesh, idx, myGen, isCrit, riseOverride));
    }

    IEnumerator Animate(GameObject obj, TextMesh mesh, int idx, int gen, bool isCrit,
                        float riseOverride = 0f)
    {
        float lifetime  = isCrit ? 1.4f : 1.0f;
        float riseSpeed = riseOverride > 0f ? riseOverride : (isCrit ? 2.5f : 1.6f);
        Vector3 startPos = obj.transform.position;
        Color   startCol = mesh.color;

        // Crit: quick punch-in scale
        if (isCrit)
        {
            float pe = 0f, punchDur = 0.15f;
            while (pe < punchDur)
            {
                if (_gen[idx] != gen) { obj.SetActive(false); yield break; }
                pe += Time.deltaTime;
                float s = Mathf.LerpUnclamped(1f, 1.4f, Mathf.Sin(pe / punchDur * Mathf.PI));
                obj.transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            obj.transform.localScale = Vector3.one;
        }

        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            // Generation check — slot was recycled, bail out
            if (_gen[idx] != gen) { obj.SetActive(false); yield break; }

            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            obj.transform.position = startPos + Vector3.up * (riseSpeed * elapsed);

            // Fade out in last 40%
            if (t > 0.6f)
            {
                float alpha = 1f - (t - 0.6f) / 0.4f;
                Color c = startCol; c.a = alpha; mesh.color = c;
            }

            yield return null;
        }

        obj.SetActive(false);
        obj.transform.localScale = Vector3.one;
    }
}

// ── Inline billboard fallback — faces camera each LateUpdate ─────────────────
// Only instantiated when RodBillboard is not available in the project.
internal class DmgTextBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position,
            cam.transform.up);
    }
}
