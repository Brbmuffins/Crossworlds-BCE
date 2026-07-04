using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZoneTitleUI : MonoBehaviour
{
    public static ZoneTitleUI Instance { get; private set; }

    [Header("Timing")]
    [SerializeField] float fadeInTime = 0.35f;
    [SerializeField] float holdTime = 2.1f;
    [SerializeField] float fadeOutTime = 0.75f;

    CanvasGroup _group;
    TextMeshProUGUI _title;
    TextMeshProUGUI _subtitle;
    Coroutine _showRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Application.isBatchMode || Instance != null) return;

        var go = new GameObject("Zone Title UI");
        DontDestroyOnLoad(go);
        go.AddComponent<ZoneTitleUI>();
    }

    public void Show(string zoneName, string subtitle = "")
    {
        if (string.IsNullOrWhiteSpace(zoneName)) return;

        EnsureBuilt();

        _title.text = zoneName.Trim();
        _subtitle.text = string.IsNullOrWhiteSpace(subtitle) ? "" : subtitle.Trim();
        _subtitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(_subtitle.text));

        if (_showRoutine != null) StopCoroutine(_showRoutine);
        _showRoutine = StartCoroutine(ShowRoutine());
    }

    public static ZoneTitleUI GetOrCreate()
    {
        if (Instance != null) return Instance;
        if (Application.isBatchMode) return null;

        var go = new GameObject("Zone Title UI");
        DontDestroyOnLoad(go);
        return go.AddComponent<ZoneTitleUI>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureBuilt();
    }

    void EnsureBuilt()
    {
        if (_group != null) return;

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2500;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        _group = gameObject.AddComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.interactable = false;
        _group.blocksRaycasts = false;

        var root = new GameObject("ZoneTitleRoot", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, 185f);
        rootRect.sizeDelta = new Vector2(920f, 170f);

        _title = MakeText("ZoneName", rootRect, 54f, FontStyles.SmallCaps | FontStyles.Bold);
        _title.rectTransform.anchorMin = new Vector2(0f, 0.45f);
        _title.rectTransform.anchorMax = new Vector2(1f, 1f);
        _title.rectTransform.offsetMin = Vector2.zero;
        _title.rectTransform.offsetMax = Vector2.zero;
        _title.color = new Color(1f, 0.86f, 0.45f, 1f);

        _subtitle = MakeText("ZoneSubtitle", rootRect, 24f, FontStyles.SmallCaps);
        _subtitle.rectTransform.anchorMin = new Vector2(0f, 0f);
        _subtitle.rectTransform.anchorMax = new Vector2(1f, 0.42f);
        _subtitle.rectTransform.offsetMin = Vector2.zero;
        _subtitle.rectTransform.offsetMax = Vector2.zero;
        _subtitle.color = new Color(0.84f, 0.91f, 1f, 0.95f);
    }

    IEnumerator ShowRoutine()
    {
        yield return FadeTo(1f, fadeInTime);
        yield return new WaitForSeconds(holdTime);
        yield return FadeTo(0f, fadeOutTime);
        _showRoutine = null;
    }

    IEnumerator FadeTo(float target, float duration)
    {
        float start = _group.alpha;
        if (duration <= 0f)
        {
            _group.alpha = target;
            yield break;
        }

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            _group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        _group.alpha = target;
    }

    static TextMeshProUGUI MakeText(string name, Transform parent, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(Shadow));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = size;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(14f, size * 0.45f);
        text.fontSizeMax = size;
        text.fontStyle = style;
        text.raycastTarget = false;
        text.characterSpacing = 0f;

        var shadow = go.GetComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }
}
