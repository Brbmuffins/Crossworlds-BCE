#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns fixed-size screen UI combat numbers from world-space anchors.
/// Callers still pass a world position; this class projects it onto the UI canvas.
/// </summary>
public class FloatingDamageText : MonoBehaviour
{
    public enum DamageType { Normal, PlayerDamage, Critical, Heal, Miss, HealCrit, Shield, TriageReturn }

    static FloatingDamageText _instance;

    const int PoolSize = 32;
    const float TextSizeMultiplier = 1.71f;
    const float ShadowAlpha = 0.68f;
    const float DefaultLifetime = 1.0f;
    const float CritLifetime = 1.35f;
    const float DefaultScreenRise = 88f;
    const float HealScreenRise = 104f;
    const float CritScreenRise = 116f;
    const float TriageScreenRise = 54f;
    const float CritImpactDuration = 0.34f;
    const float CritJoltDistance = 24f;
    const float CritJoltHeight = 10f;
    const float CritTiltDegrees = 8f;

    GameObject[] _pool;
    RectTransform[] _rects;
    TextMeshProUGUI[] _labels;
    Shadow[] _shadows;
    int[] _gen;
    int _poolHead;

    RectTransform _canvasRect;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null) return;

        var go = new GameObject("[FloatingDamageText]");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<FloatingDamageText>();
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        CreateCanvas();
        CreatePool();
    }

    public static void Spawn(Vector3 worldPos, float amount, DamageType type = DamageType.Normal,
                             string textOverride = null)
    {
        if (_instance == null) return;
        _instance.SpawnInternal(worldPos, amount, type, textOverride, false);
    }

    public static void SpawnAnchored(Vector3 worldPos, float amount, DamageType type = DamageType.Normal,
                                     string textOverride = null)
    {
        if (_instance == null) return;
        _instance.SpawnInternal(worldPos, amount, type, textOverride, true);
    }

    public static void Spawn(Vector3 worldPos, int amount, DamageType type = DamageType.Normal,
                             string textOverride = null)
        => Spawn(worldPos, (float)amount, type, textOverride);

    public static void SpawnAnchored(Vector3 worldPos, int amount, DamageType type = DamageType.Normal,
                                     string textOverride = null)
        => SpawnAnchored(worldPos, (float)amount, type, textOverride);

    void CreateCanvas()
    {
        var canvasObj = new GameObject("FloatingDamageCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObj.transform.SetParent(transform, false);

        _canvasRect = canvasObj.GetComponent<RectTransform>();
        _canvasRect.anchorMin = Vector2.zero;
        _canvasRect.anchorMax = Vector2.one;
        _canvasRect.offsetMin = Vector2.zero;
        _canvasRect.offsetMax = Vector2.zero;

        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    void CreatePool()
    {
        _pool = new GameObject[PoolSize];
        _rects = new RectTransform[PoolSize];
        _labels = new TextMeshProUGUI[PoolSize];
        _shadows = new Shadow[PoolSize];
        _gen = new int[PoolSize];
        _poolHead = 0;

        for (int i = 0; i < PoolSize; i++)
        {
            var obj = new GameObject($"DmgText_{i}", typeof(RectTransform));
            obj.transform.SetParent(_canvasRect, false);
            obj.SetActive(false);

            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(180f, 58f);

            var label = obj.AddComponent<TextMeshProUGUI>();
            ConfigureLabel(label);

            var shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, ShadowAlpha);
            shadow.effectDistance = new Vector2(2.5f, -2.5f);
            shadow.useGraphicAlpha = true;

            _pool[i] = obj;
            _rects[i] = rect;
            _labels[i] = label;
            _shadows[i] = shadow;
        }
    }

    void SpawnInternal(Vector3 worldPos, float amount, DamageType type, string textOverride = null, bool anchored = false)
    {
        int idx = _poolHead;
        _poolHead = (_poolHead + 1) % PoolSize;

        _gen[idx]++;
        int myGen = _gen[idx];

        var obj = _pool[idx];
        var rect = _rects[idx];
        var label = _labels[idx];
        var shadow = _shadows[idx];

        obj.SetActive(false);

        ConfigureForType(rect, label, shadow, amount, type, textOverride);

        Vector3 worldAnchor = anchored
            ? worldPos
            : worldPos + Vector3.up * Random.Range(0.8f, 1.4f);

        Vector2 screenScatter = new Vector2(
            Random.Range(-30f, 30f),
            anchored ? Random.Range(-8f, 8f) : Random.Range(-12f, 12f));

        if (!TrySetScreenPosition(rect, worldAnchor, screenScatter))
            return;

        rect.localScale = Vector3.one * StartingScale(type);
        rect.localRotation = Quaternion.identity;
        obj.SetActive(true);

        float critJoltDirection = Random.value < 0.5f ? -1f : 1f;
        StartCoroutine(Animate(
            obj,
            rect,
            label,
            idx,
            myGen,
            worldAnchor,
            screenScatter,
            type,
            critJoltDirection));
    }

    IEnumerator Animate(GameObject obj, RectTransform rect, TextMeshProUGUI label, int idx, int gen,
                        Vector3 worldAnchor, Vector2 screenScatter, DamageType type,
                        float critJoltDirection)
    {
        float lifetime = type == DamageType.Critical || type == DamageType.HealCrit
            ? CritLifetime
            : DefaultLifetime;
        float rise = RiseDistance(type);
        float startScale = StartingScale(type);
        Color startColor = label.color;

        float elapsed = 0f;
        while (elapsed < lifetime)
        {
            if (_gen[idx] != gen)
            {
                obj.SetActive(false);
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);
            float easedRise = Mathf.SmoothStep(0f, rise, t);
            Vector2 screenOffset = screenScatter + Vector2.up * easedRise;

            bool isDamageCrit = type == DamageType.Critical;
            if (isDamageCrit && elapsed < CritImpactDuration)
            {
                float impactT = Mathf.Clamp01(
                    elapsed / CritImpactDuration);
                float envelope = (1f - impactT) * (1f - impactT);
                float wave = Mathf.Sin(impactT * Mathf.PI * 5f) * envelope;
                screenOffset += new Vector2(
                    critJoltDirection * wave * CritJoltDistance,
                    Mathf.Abs(wave) * CritJoltHeight);
                rect.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    critJoltDirection * wave * CritTiltDegrees);
            }
            else
                rect.localRotation = Quaternion.identity;

            if (!TrySetScreenPosition(rect, worldAnchor, screenOffset))
            {
                obj.SetActive(false);
                yield break;
            }

            float scale = startScale;
            if (type == DamageType.Critical || type == DamageType.HealCrit)
                scale *= CritScaleMultiplier(elapsed);
            rect.localScale = new Vector3(scale, scale, 1f);

            if (t > 0.6f)
            {
                float alpha = 1f - (t - 0.6f) / 0.4f;
                Color c = startColor;
                c.a = alpha;
                label.color = c;
            }

            yield return null;
        }

        obj.SetActive(false);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    static float CritScaleMultiplier(float elapsed)
    {
        // Preserve the existing 1.28 peak size, but reach it much faster and
        // rebound below/above resting scale before settling for a harder impact.
        if (elapsed < 0.065f)
            return Mathf.Lerp(1f, 1.28f, EaseOutCubic(elapsed / 0.065f));
        if (elapsed < 0.14f)
            return Mathf.Lerp(1.28f, 0.94f, Mathf.SmoothStep(0f, 1f, (elapsed - 0.065f) / 0.075f));
        if (elapsed < 0.23f)
            return Mathf.Lerp(0.94f, 1.04f, Mathf.SmoothStep(0f, 1f, (elapsed - 0.14f) / 0.09f));
        if (elapsed < CritImpactDuration)
            return Mathf.Lerp(1.04f, 1f, Mathf.SmoothStep(0f, 1f, (elapsed - 0.23f) / 0.11f));

        return 1f;
    }

    static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float inverse = 1f - t;
        return 1f - inverse * inverse * inverse;
    }

    bool TrySetScreenPosition(RectTransform rect, Vector3 worldAnchor, Vector2 screenOffset)
    {
        Vector2 screenPoint;
        var cam = Camera.main;

        if (cam != null)
        {
            Vector3 projected = cam.WorldToScreenPoint(worldAnchor);
            if (projected.z <= 0f)
                return false;

            screenPoint = new Vector2(projected.x + screenOffset.x, projected.y + screenOffset.y);
        }
        else
        {
            screenPoint = new Vector2(
                Screen.width * 0.5f + screenOffset.x,
                Screen.height * 0.5f + screenOffset.y);
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPoint, null, out Vector2 localPoint))
            return false;

        rect.anchoredPosition = localPoint;
        return true;
    }

    static void ConfigureLabel(TextMeshProUGUI label)
    {
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.fontStyle = FontStyles.Bold;
        label.raycastTarget = false;
    }

    static void ConfigureForType(RectTransform rect, TextMeshProUGUI label, Shadow shadow,
                                 float amount, DamageType type, string textOverride)
    {
        label.color = Color.white;
        label.text = textOverride ?? Mathf.RoundToInt(amount).ToString();
        label.fontSize = ScaledFont(30f);
        rect.sizeDelta = new Vector2(180f, 58f);

        switch (type)
        {
            case DamageType.Critical:
                label.text = textOverride ?? $"{Mathf.RoundToInt(amount)}!";
                label.color = new Color(1.00f, 0.35f, 0.05f);
                label.fontSize = ScaledFont(40f);
                rect.sizeDelta = new Vector2(220f, 72f);
                break;
            case DamageType.PlayerDamage:
                label.text = textOverride ?? $"-{Mathf.RoundToInt(amount)}";
                label.color = new Color(1.00f, 0.16f, 0.10f);
                label.fontSize = ScaledFont(32f);
                rect.sizeDelta = new Vector2(190f, 62f);
                break;
            case DamageType.Heal:
                label.text = textOverride ?? $"+{Mathf.RoundToInt(amount)}";
                label.color = new Color(0.20f, 0.90f, 0.30f);
                label.fontSize = ScaledFont(31f);
                break;
            case DamageType.HealCrit:
                label.text = textOverride ?? $"+{Mathf.RoundToInt(amount)}!";
                label.color = new Color(0.40f, 1.00f, 0.40f);
                label.fontSize = ScaledFont(37f);
                rect.sizeDelta = new Vector2(210f, 68f);
                break;
            case DamageType.Shield:
                label.text = textOverride ?? $"[{Mathf.RoundToInt(amount)}]";
                label.color = new Color(0.40f, 0.70f, 1.00f);
                label.fontSize = ScaledFont(29f);
                break;
            case DamageType.TriageReturn:
                label.text = textOverride ?? $"+{Mathf.RoundToInt(amount)}";
                label.color = new Color(1.00f, 0.80f, 0.20f);
                label.fontSize = ScaledFont(23f);
                rect.sizeDelta = new Vector2(150f, 48f);
                break;
            case DamageType.Miss:
                label.text = textOverride ?? "MISS";
                label.color = new Color(0.75f, 0.75f, 0.75f);
                label.fontSize = ScaledFont(28f);
                rect.sizeDelta = new Vector2(170f, 54f);
                break;
            default:
                label.text = textOverride ?? Mathf.RoundToInt(amount).ToString();
                label.color = new Color(1.00f, 0.92f, 0.80f);
                label.fontSize = ScaledFont(30f);
                break;
        }

        shadow.effectColor = new Color(0f, 0f, 0f, ShadowAlpha);
    }

    static float ScaledFont(float size) => size * TextSizeMultiplier;

    static float StartingScale(DamageType type) => type switch
    {
        DamageType.Critical     => 1.08f,
        DamageType.HealCrit     => 1.08f,
        DamageType.TriageReturn => 0.9f,
        _                       => 1.0f
    };

    static float RiseDistance(DamageType type) => type switch
    {
        DamageType.Critical     => CritScreenRise,
        DamageType.HealCrit     => CritScreenRise,
        DamageType.Heal         => HealScreenRise,
        DamageType.TriageReturn => TriageScreenRise,
        _                       => DefaultScreenRise
    };
}
#endif
