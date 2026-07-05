using System.Collections;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using TMPro;

// ═══════════════════════════════════════════════════════════════════════════
//  CHARACTER SELECT UI  —  3-panel, self-building, self-bootstrapping.
//
//  Layout:  [Class List 220px] | [3D Preview - centre] | [Details 420px]
//
//  Inspector:  drop 5 CharacterData assets into Characters[0-4].
//              Everything else (camera, RT, lights) auto-builds at runtime.
//              Optionally assign a previewCamera/RT/spawnPoint to override.
// ═══════════════════════════════════════════════════════════════════════════

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Roster")]
    public CharacterData[] characters;

    [Header("Server")]
    public string serverAddress = "15.204.243.36";

    [Header("3D Preview  (leave null — auto-built at runtime)")]
    public Camera        previewCamera;
    public RenderTexture previewRenderTexture;
    public Transform     previewSpawnPoint;
    public float         rotationSpeed = 22f;

    // ── Runtime refs ─────────────────────────────────────────────────────────
    int           _sel;
    GameObject    _previewInstance;
    GameObject    _previewRoot;
    RenderTexture _rt;

    // UI handles set during BuildUI
    Image             _bgPanel;
    RawImage          _previewDisplay;
    Image             _portraitOverlay;
    TextMeshProUGUI   _className;
    TextMeshProUGUI   _roleTag;
    TextMeshProUGUI   _lore;
    Transform         _traitRow;
    Transform         _statCol;
    Transform         _abilRow;
    Image             _depPanel;
    TextMeshProUGUI   _depName;
    TextMeshProUGUI   _depDesc;
    Image             _depIcon;
    Button[]          _classBtns;
    Image[]           _classBtnBg;
    Button            _deployBtn;
    TextMeshProUGUI   _deployLabel;

    // ── Palette ───────────────────────────────────────────────────────────────
    static readonly Color BgDeep     = new Color(0.04f, 0.03f, 0.08f, 1.00f);
    static readonly Color PanelDark  = new Color(0.06f, 0.05f, 0.10f, 0.97f);
    static readonly Color PanelMid   = new Color(0.08f, 0.07f, 0.13f, 0.95f);
    static readonly Color TextPrim   = new Color(0.95f, 0.93f, 0.88f, 1.00f);
    static readonly Color TextDim    = new Color(0.55f, 0.53f, 0.50f, 1.00f);
    static readonly Color Clear      = new Color(0, 0, 0, 0);

    const float LEFT_W   = 220f;
    const float RIGHT_W  = 430f;
    const int   PREV_LAY = 31;      // "CharacterPreview" layer

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        EnsureSingleEventSystem();
        BuildPreview();
        BuildUI();
        if (characters != null && characters.Length > 0)
            ShowClass(0);
    }

    // Two enabled EventSystems (e.g. a DontDestroyOnLoad one carried in from another
    // scene plus this scene's own) make uGUI silently stop processing clicks. Keep one,
    // disable the rest, and guarantee it drives the new Input System.
    void EnsureSingleEventSystem()
    {
        var systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        EventSystem keep = systems.Length > 0 ? systems[0] : null;

        if (keep == null)
        {
            var go = new GameObject("EventSystem");
            keep = go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }
        else
        {
            for (int i = 1; i < systems.Length; i++)
            {
                Debug.LogWarning($"[CharSel] Disabling duplicate EventSystem '{systems[i].name}' — it was blocking clicks.");
                systems[i].gameObject.SetActive(false);
            }
            if (keep.GetComponent<InputSystemUIInputModule>() == null)
            {
                foreach (var m in keep.GetComponents<BaseInputModule>()) m.enabled = false;
                keep.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }
    }

    void Update()
    {
        if (_previewRoot != null)
            _previewRoot.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.leftArrowKey.wasPressedThisFrame)  ShowClass((_sel - 1 + characters.Length) % characters.Length);
        if (kb.rightArrowKey.wasPressedThisFrame) ShowClass((_sel + 1) % characters.Length);
    }

    void OnDestroy()
    {
        if (_rt != null) { _rt.Release(); Destroy(_rt); }
    }

    // ── 3D Preview setup ──────────────────────────────────────────────────────

    void BuildPreview()
    {
        if (previewCamera != null) return; // manually wired — skip

        _rt = new RenderTexture(512, 820, 24) { antiAliasing = 4 };
        previewRenderTexture = _rt;

        // Exclude preview layer from main camera
        if (Camera.main != null) Camera.main.cullingMask &= ~(1 << PREV_LAY);

        // Preview camera
        var camGO = new GameObject("_PrevCam");
        previewCamera = camGO.AddComponent<Camera>();
        previewCamera.cullingMask     = 1 << PREV_LAY;
        previewCamera.clearFlags      = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0.04f, 0.03f, 0.08f, 1f);
        previewCamera.targetTexture   = _rt;
        previewCamera.fieldOfView     = 40f;
        previewCamera.nearClipPlane   = 0.1f;
        previewCamera.farClipPlane    = 25f;
        camGO.transform.SetPositionAndRotation(
            new Vector3(0f, 1.45f, -3.1f), Quaternion.Euler(6f, 0f, 0f));

        // Key light
        SpawnPreviewLight("_KeyLight", new Color(0.95f, 0.95f, 1f), 1.8f,
            new Vector3(2f, 5f, -3f), Quaternion.Euler(45f, -30f, 0f));

        // Fill light
        SpawnPreviewLight("_FillLight", new Color(0.4f, 0.5f, 0.85f), 0.5f,
            new Vector3(-3f, 2f, 2f), Quaternion.Euler(20f, 150f, 0f));

        // Rim light (back)
        SpawnPreviewLight("_RimLight", new Color(0.6f, 0.4f, 1f), 0.35f,
            new Vector3(0f, 3f, 4f), Quaternion.Euler(30f, 180f, 0f));

        // Platform
        var plat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plat.name = "_PrevPlat";
        Destroy(plat.GetComponent<Collider>());
        plat.transform.SetPositionAndRotation(new Vector3(0f, -0.06f, 0f), Quaternion.identity);
        plat.transform.localScale = new Vector3(1.7f, 0.04f, 1.7f);
        plat.GetComponent<Renderer>().material.color = new Color(0.06f, 0.06f, 0.12f);
        SetLayer(plat, PREV_LAY);

        // Ring
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "_PrevRing";
        Destroy(ring.GetComponent<Collider>());
        ring.transform.SetPositionAndRotation(new Vector3(0f, -0.07f, 0f), Quaternion.identity);
        ring.transform.localScale = new Vector3(2.0f, 0.02f, 2.0f);
        ring.GetComponent<Renderer>().material.color = new Color(0.08f, 0.08f, 0.16f);
        SetLayer(ring, PREV_LAY);

        // Spawn point + rotation root
        var spawnGO = new GameObject("_PrevSpawn");
        spawnGO.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        previewSpawnPoint = spawnGO.transform;

        _previewRoot = new GameObject("_PrevRoot");
        _previewRoot.transform.position = Vector3.zero;
    }

    void SpawnPreviewLight(string n, Color col, float intensity, Vector3 pos, Quaternion rot)
    {
        var go = new GameObject(n); SetLayer(go, PREV_LAY);
        var l = go.AddComponent<Light>();
        l.type = LightType.Directional; l.color = col; l.intensity = intensity;
        go.transform.SetPositionAndRotation(pos, rot);
    }

    // ── Show class ────────────────────────────────────────────────────────────

    public void ShowClass(int idx)
    {
        if (characters == null || characters.Length == 0) return;
        _sel = Mathf.Clamp(idx, 0, characters.Length - 1);
        CharacterData d = characters[_sel];

        // Background tint
        if (_bgPanel) _bgPanel.color = Color.Lerp(BgDeep, d.classColorDark, 0.20f);

        // Header
        _className.text  = d.className.ToUpper();
        _className.color = d.classColor;
        _roleTag.text    = d.roleTagline;
        _lore.text       = d.loreDescription;

        // Trait pills
        ClearChildren(_traitRow);
        foreach (var t in d.traits ?? new TraitPill[0])
            BuildTraitPill(_traitRow, t, d.classColor);

        // Stat bars
        ClearChildren(_statCol);
        foreach (var s in d.stats ?? new ClassStat[0])
            BuildStatBar(_statCol, s, d.classColor);

        // Ability cards
        ClearChildren(_abilRow);
        foreach (var a in d.coreAbilities ?? new AbilityPreview[0])
            BuildAbilityCard(_abilRow, a, d.classColor);

        // Deployable
        _depPanel.color  = new Color(d.classColor.r, d.classColor.g, d.classColor.b, 0.12f);
        _depName.text    = d.deployableName?.ToUpper() ?? "";
        _depName.color   = d.classColor;
        _depDesc.text    = d.deployableDescription ?? "";
        _depIcon.sprite  = d.deployableIcon;
        _depIcon.color   = d.deployableIcon != null ? Color.white : Clear;

        // Class buttons highlight
        UpdateClassButtons(d);

        // 3D preview
        if (_previewInstance != null) Destroy(_previewInstance);
        GameObject prefab = d.previewPrefab != null ? d.previewPrefab : d.prefab;
        if (prefab != null && previewSpawnPoint != null)
        {
            _previewInstance = Instantiate(prefab,
                _previewRoot != null ? _previewRoot.transform : previewSpawnPoint);
            _previewInstance.transform.localPosition = Vector3.zero;
            _previewInstance.transform.localRotation = Quaternion.identity;
            _previewInstance.transform.localScale    = Vector3.one;

            // Neutralize gameplay: kill physics, disable all scripts (controllers,
            // Mirror NetworkBehaviours, etc.). Animator/renderers are Behaviours, not
            // MonoBehaviours, so the idle pose + mesh survive.
            foreach (var rb in _previewInstance.GetComponentsInChildren<Rigidbody>(true))
                rb.isKinematic = true;
            foreach (var col in _previewInstance.GetComponentsInChildren<Collider>(true))
                col.enabled = false;
            foreach (var mb in _previewInstance.GetComponentsInChildren<MonoBehaviour>(true))
                if (mb) mb.enabled = false;

            SetLayer(_previewInstance, PREV_LAY);
            FitPreview(_previewInstance);   // bounds-based auto-scale + ground on platform
            if (_portraitOverlay) _portraitOverlay.color = Clear;
        }
        else
        {
            // Fallback: show 2D portrait
            if (_portraitOverlay)
            {
                _portraitOverlay.sprite = d.portrait;
                _portraitOverlay.color  = d.portrait != null ? Color.white : Clear;
            }
        }

        // Tint platform to class colour
        var plat = GameObject.Find("_PrevPlat");
        if (plat != null)
            plat.GetComponent<Renderer>().material.color =
                Color.Lerp(new Color(0.05f, 0.05f, 0.10f), d.classColor, 0.28f);
    }

    // Auto-scale any model to a standard height and ground it on the platform,
    // regardless of the FBX's native import scale (raw Tripo models aren't normalized).
    void FitPreview(GameObject go)
    {
        const float TARGET_H = 2.1f;

        // Measure with root un-rotated so world bounds map cleanly to local offsets.
        if (_previewRoot != null) _previewRoot.transform.rotation = Quaternion.identity;
        go.transform.localScale    = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        if (b.size.y < 0.0001f) return;

        float scale = TARGET_H / b.size.y;
        go.transform.localScale = Vector3.one * scale;

        // Re-measure after scaling, then centre X/Z and drop feet to y=0.
        b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        go.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);
    }

    void UpdateClassButtons(CharacterData d)
    {
        if (_classBtns == null) return;
        for (int i = 0; i < _classBtns.Length; i++)
        {
            bool sel = i == _sel;
            _classBtnBg[i].color = sel
                ? new Color(d.classColor.r, d.classColor.g, d.classColor.b, 0.18f)
                : new Color(1f, 1f, 1f, 0.03f);
            var lbl = _classBtns[i].GetComponentInChildren<TextMeshProUGUI>();
            if (lbl) lbl.color = sel ? d.classColor : TextDim;
        }
    }

    // ── UI construction ───────────────────────────────────────────────────────

    void BuildUI()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) { Debug.LogError("[CharSel] Must be child of a Canvas."); return; }
        // Sit above the self-bootstrapping gameplay HUDs (90–201) that also wake up in
        // this scene, so none of them intercept the class-select clicks.
        canvas.overrideSorting = true;
        canvas.sortingOrder    = 500;
        var root = canvas.GetComponent<RectTransform>();

        // Full-screen background
        _bgPanel = MkImg(root, "BG", BgDeep);
        Stretch(_bgPanel.rectTransform);

        BuildClassList(root);
        BuildCenter(root);
        BuildDetails(root);
    }

    // ── Left: class list ──────────────────────────────────────────────────────

    void BuildClassList(RectTransform root)
    {
        // Left strip — fixed width, full height
        var panel = MkImg(root, "ClassList", PanelDark);
        panel.rectTransform.anchorMin = Vector2.zero;
        panel.rectTransform.anchorMax = new Vector2(0f, 1f);
        panel.rectTransform.offsetMin = Vector2.zero;
        panel.rectTransform.offsetMax = new Vector2(LEFT_W, 0f);

        // "SELECT CLASS" header
        var hdr = MkTMP(panel.rectTransform, "Hdr", "SELECT CLASS", 9f, FontStyles.Bold);
        hdr.color     = TextDim;
        hdr.alignment = TextAlignmentOptions.Center;
        RectSet(hdr.rectTransform, 0f, 0f, LEFT_W, 36f, top: true);

        if (characters == null) return;

        _classBtns  = new Button[characters.Length];
        _classBtnBg = new Image[characters.Length];

        float BTN_H = 78f;
        float yOff  = 44f;

        for (int i = 0; i < characters.Length; i++)
        {
            int ci = i;
            var d  = characters[i];

            var btnGO = new GameObject("Btn_" + d.className, typeof(RectTransform));
            btnGO.transform.SetParent(panel.rectTransform, false);
            _classBtnBg[i] = btnGO.AddComponent<Image>();
            _classBtnBg[i].color = new Color(1f, 1f, 1f, 0.03f);
            _classBtns[i]  = btnGO.AddComponent<Button>();
            var rt = btnGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.sizeDelta        = new Vector2(-10f, BTN_H);
            rt.anchoredPosition = new Vector2(5f, -(yOff + BTN_H * 0.5f));
            yOff += BTN_H + 3f;

            // Colour stripe
            var stripe = MkImg(rt, "Stripe", d.classColor);
            stripe.rectTransform.anchorMin = Vector2.zero;
            stripe.rectTransform.anchorMax = new Vector2(0f, 1f);
            stripe.rectTransform.offsetMin = Vector2.zero;
            stripe.rectTransform.offsetMax = new Vector2(3f, 0f);

            // Class name
            var lbl = MkTMP(rt, "Name", d.className.ToUpper(), 13f, FontStyles.Bold);
            lbl.color     = TextDim;
            lbl.alignment = TextAlignmentOptions.TopLeft;
            RectSet(lbl.rectTransform, 14f, 10f, LEFT_W - 20f, 22f, top: true);

            // Role (first segment only)
            string roleShort = d.roleTagline.Contains("·")
                ? d.roleTagline.Split('·')[0].Trim()
                : d.roleTagline;
            var role = MkTMP(rt, "Role", roleShort, 9f, FontStyles.Normal);
            role.color     = new Color(0.48f, 0.47f, 0.45f, 1f);
            role.alignment = TextAlignmentOptions.TopLeft;
            RectSet(role.rectTransform, 14f, 35f, LEFT_W - 20f, 16f, top: true);

            _classBtns[i].onClick.AddListener(() => ShowClass(ci));
        }

        // Footer version tag
        var ver = MkTMP(panel.rectTransform, "Ver", "v0.1  ALPHA", 8f, FontStyles.Normal);
        ver.color     = new Color(0.28f, 0.27f, 0.25f, 1f);
        ver.alignment = TextAlignmentOptions.Center;
        RectSet(ver.rectTransform, 0f, 0f, LEFT_W, 24f, top: false);
    }

    // ── Centre: 3D preview ────────────────────────────────────────────────────

    void BuildCenter(RectTransform root)
    {
        var panel = MkImg(root, "Center", BgDeep);
        var rt    = panel.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(LEFT_W, 0f);
        rt.offsetMax = new Vector2(-RIGHT_W, 0f);

        // RenderTexture display
        var rawGO = new GameObject("Preview3D", typeof(RectTransform));
        rawGO.transform.SetParent(rt, false);
        _previewDisplay = rawGO.AddComponent<RawImage>();
        _previewDisplay.texture = previewRenderTexture;
        _previewDisplay.color   = Color.white;
        var rawRt = rawGO.GetComponent<RectTransform>();
        rawRt.anchorMin = Vector2.zero; rawRt.anchorMax = Vector2.one;
        rawRt.offsetMin = rawRt.offsetMax = Vector2.zero;
        var arf = rawGO.AddComponent<AspectRatioFitter>();
        arf.aspectMode  = AspectRatioFitter.AspectMode.FitInParent;
        arf.aspectRatio = 512f / 820f;

        // Portrait fallback (Image — shown when no 3D prefab assigned)
        var portGO = new GameObject("Portrait", typeof(RectTransform));
        portGO.transform.SetParent(rt, false);
        _portraitOverlay = portGO.AddComponent<Image>();
        _portraitOverlay.preserveAspect = true;
        _portraitOverlay.color = Clear;
        var portRt = portGO.GetComponent<RectTransform>();
        portRt.anchorMin = new Vector2(0.08f, 0.06f);
        portRt.anchorMax = new Vector2(0.92f, 0.97f);
        portRt.offsetMin = portRt.offsetMax = Vector2.zero;

        // Subtle bottom gradient / vignette bar
        var grad = MkImg(rt, "BottomFade", new Color(0f, 0f, 0f, 0.55f));
        grad.rectTransform.anchorMin = Vector2.zero;
        grad.rectTransform.anchorMax = new Vector2(1f, 0f);
        grad.rectTransform.sizeDelta = new Vector2(0f, 120f);
        grad.rectTransform.anchoredPosition = Vector2.zero;
    }

    // ── Right: class details ──────────────────────────────────────────────────

    void BuildDetails(RectTransform root)
    {
        var panel = MkImg(root, "Details", PanelMid);
        var rt    = panel.rectTransform;
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(-RIGHT_W, 0f);
        rt.offsetMax = Vector2.zero;
        var p = rt;

        float y = 20f;
        float W = RIGHT_W;
        float pad = 22f;
        float inner = W - pad * 2f;

        // ── Class name ──
        _className = MkTMP(p, "ClassName", "", 30f, FontStyles.Bold);
        _className.alignment = TextAlignmentOptions.TopLeft;
        RectSet(_className.rectTransform, pad, y, inner, 42f, top: true); y += 44f;

        // ── Role tagline ──
        _roleTag = MkTMP(p, "Role", "", 11f, FontStyles.Normal);
        _roleTag.color     = TextDim;
        _roleTag.alignment = TextAlignmentOptions.TopLeft;
        RectSet(_roleTag.rectTransform, pad, y, inner, 18f, top: true); y += 24f;

        HRule(p, y, W); y += 14f;

        // ── Lore ──
        _lore = MkTMP(p, "Lore", "", 11f, FontStyles.Normal);
        _lore.color            = TextDim;
        _lore.alignment        = TextAlignmentOptions.TopLeft;
        _lore.textWrappingMode = TextWrappingModes.Normal;
        RectSet(_lore.rectTransform, pad, y, inner, 84f, top: true); y += 90f;

        // ── Trait pills ──
        var traitGO = new GameObject("TraitRow", typeof(RectTransform));
        traitGO.transform.SetParent(p, false);
        var trailRt = traitGO.GetComponent<RectTransform>();
        _traitRow = trailRt;
        RectSet(trailRt, pad, y, inner, 28f, top: true);
        var thl = traitGO.AddComponent<HorizontalLayoutGroup>();
        thl.spacing = 5f; thl.childAlignment = TextAnchor.MiddleLeft;
        thl.childForceExpandWidth = false; thl.childForceExpandHeight = true;
        y += 36f;

        HRule(p, y, W); y += 12f;

        // ── Stat bars ──
        var statGO = new GameObject("Stats", typeof(RectTransform));
        statGO.transform.SetParent(p, false);
        var statRt = statGO.GetComponent<RectTransform>();
        _statCol = statRt;
        RectSet(statRt, pad, y, inner * 0.72f, 110f, top: true);
        var svl = statGO.AddComponent<VerticalLayoutGroup>();
        svl.spacing = 5f; svl.childAlignment = TextAnchor.UpperLeft;
        svl.childForceExpandWidth = true; svl.childForceExpandHeight = false;
        y += 118f;

        HRule(p, y, W); y += 10f;

        // ── Deployable panel ──
        var depGO = new GameObject("Deployable", typeof(RectTransform));
        depGO.transform.SetParent(p, false);
        _depPanel = depGO.AddComponent<Image>();
        _depPanel.color = new Color(0f, 0f, 0f, 0f); // prevent default white flash
        var depRt = depGO.GetComponent<RectTransform>();
        RectSet(depRt, pad, y, inner, 64f, top: true);
        y += 72f;

        // Icon
        var diGO = new GameObject("DepIcon", typeof(RectTransform));
        diGO.transform.SetParent(depRt, false);
        _depIcon = diGO.AddComponent<Image>();
        _depIcon.preserveAspect = true;
        var diRt = diGO.GetComponent<RectTransform>();
        diRt.anchorMin = Vector2.zero; diRt.anchorMax = new Vector2(0f, 1f);
        diRt.offsetMin = new Vector2(6f, 5f); diRt.offsetMax = new Vector2(58f, -5f);

        _depName = MkTMP(depRt, "DepName", "", 12f, FontStyles.Bold);
        _depName.alignment = TextAlignmentOptions.BottomLeft;
        var dnRt = _depName.rectTransform;
        dnRt.anchorMin = new Vector2(0f, 0.5f); dnRt.anchorMax = Vector2.one;
        dnRt.offsetMin = new Vector2(64f, 0f); dnRt.offsetMax = new Vector2(-8f, -4f);

        _depDesc = MkTMP(depRt, "DepDesc", "", 10f, FontStyles.Normal);
        _depDesc.color            = TextDim;
        _depDesc.alignment        = TextAlignmentOptions.TopLeft;
        _depDesc.textWrappingMode = TextWrappingModes.Normal;
        var ddRt = _depDesc.rectTransform;
        ddRt.anchorMin = new Vector2(0f, 0f); ddRt.anchorMax = new Vector2(1f, 0.5f);
        ddRt.offsetMin = new Vector2(64f, 4f); ddRt.offsetMax = new Vector2(-8f, 0f);

        HRule(p, y, W); y += 10f;

        // ── Ability cards  (fixed 3-column grid, no stretching) ──
        float cardW = (inner - 16f) / 3f;
        float cardH = 160f;

        var abilGO = new GameObject("Abilities", typeof(RectTransform));
        abilGO.transform.SetParent(p, false);
        var abilRt = abilGO.GetComponent<RectTransform>();
        _abilRow = abilRt;
        RectSet(abilRt, pad, y, inner, cardH, top: true);
        var ahl = abilGO.AddComponent<HorizontalLayoutGroup>();
        ahl.spacing = 8f;
        ahl.childAlignment         = TextAnchor.UpperLeft;
        ahl.childControlWidth      = true;
        ahl.childControlHeight     = true;
        ahl.childForceExpandWidth  = true;
        ahl.childForceExpandHeight = true;
        ahl.padding                = new RectOffset(0, 0, 0, 0);
        y += cardH + 10f;

        // ── DEPLOY button ──
        float btnH = 48f;
        var btnGO = new GameObject("DeployBtn", typeof(RectTransform));
        btnGO.transform.SetParent(p, false);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.12f, 0.55f, 0.30f, 1f);
        var btnRt = btnGO.GetComponent<RectTransform>();
        RectSet(btnRt, pad, y, inner, btnH, top: true);
        var btn = btnGO.AddComponent<Button>();
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor      = new Color(0.12f, 0.50f, 0.28f);
        cb.highlightedColor = new Color(0.18f, 0.72f, 0.40f);
        cb.pressedColor     = Color.white;
        btn.colors          = cb;
        btn.targetGraphic   = btnImg;
        btn.onClick.AddListener(Play);
        _deployBtn = btn;

        var lbl = MkTMP(btnRt, "Lbl", "DEPLOY", 17f, FontStyles.Bold);
        lbl.alignment = TextAlignmentOptions.Center;
        Stretch(lbl.rectTransform);
        _deployLabel = lbl;
    }

    // ── Card / pill / bar builders ────────────────────────────────────────────

    void BuildTraitPill(Transform parent, TraitPill trait, Color accent)
    {
        var pillGO = new GameObject("Pill", typeof(RectTransform));
        pillGO.transform.SetParent(parent, false);
        var bg = pillGO.AddComponent<Image>();
        bg.color = new Color(accent.r, accent.g, accent.b, 0.16f);
        var le = pillGO.AddComponent<LayoutElement>();
        le.preferredHeight = 26f; le.minWidth = 68f;
        var hg = pillGO.AddComponent<HorizontalLayoutGroup>();
        hg.padding = new RectOffset(8, 8, 3, 3); hg.spacing = 5f;
        hg.childAlignment = TextAnchor.MiddleLeft;
        hg.childControlWidth = true; hg.childControlHeight = true;
        hg.childForceExpandWidth = false; hg.childForceExpandHeight = true;

        var lbl = MkTMP(pillGO.GetComponent<RectTransform>(), "L", trait.label.ToUpper(), 9f, FontStyles.Bold);
        lbl.color     = accent;
        lbl.alignment = TextAlignmentOptions.MidlineLeft;
        var lle = lbl.gameObject.AddComponent<LayoutElement>();
        lle.flexibleWidth = 1f;
    }

    void BuildStatBar(Transform parent, ClassStat stat, Color accent)
    {
        var rowGO = new GameObject("Stat_" + stat.label, typeof(RectTransform));
        rowGO.transform.SetParent(parent, false);
        var le = rowGO.AddComponent<LayoutElement>(); le.preferredHeight = 18f;
        var hg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hg.spacing = 6f; hg.childAlignment = TextAnchor.MiddleLeft;
        hg.childControlWidth = true; hg.childControlHeight = true;
        hg.childForceExpandWidth = false; hg.childForceExpandHeight = true;

        var lbl = MkTMP(rowGO.GetComponent<RectTransform>(), "L", stat.label.ToUpper(), 9f, FontStyles.Normal);
        lbl.color     = TextDim;
        lbl.alignment = TextAlignmentOptions.MidlineLeft;
        lbl.gameObject.AddComponent<LayoutElement>().preferredWidth = 88f;

        for (int i = 0; i < 5; i++)
        {
            var pip = new GameObject("P" + i, typeof(RectTransform));
            pip.transform.SetParent(rowGO.transform, false);
            pip.AddComponent<Image>().color = i < stat.value
                ? new Color(accent.r, accent.g, accent.b, 0.88f)
                : new Color(0.22f, 0.22f, 0.27f, 0.70f);
            var ple = pip.AddComponent<LayoutElement>();
            ple.preferredWidth = 16f; ple.preferredHeight = 8f;
        }
    }

    void BuildAbilityCard(Transform parent, AbilityPreview ability, Color accent)
    {
        var cardGO = new GameObject("Card_" + ability.abilityName, typeof(RectTransform));
        cardGO.transform.SetParent(parent, false);
        cardGO.AddComponent<Image>().color = new Color(accent.r, accent.g, accent.b, 0.16f);
        var vg = cardGO.AddComponent<VerticalLayoutGroup>();
        vg.padding = new RectOffset(6, 6, 8, 6);
        vg.spacing = 4f;
        vg.childAlignment         = TextAnchor.UpperCenter;
        vg.childControlWidth      = true;
        vg.childControlHeight     = true;
        vg.childForceExpandWidth  = true;
        vg.childForceExpandHeight = false;

        // Accent top bar
        var bar = new GameObject("Bar", typeof(RectTransform));
        bar.transform.SetParent(cardGO.transform, false);
        bar.AddComponent<Image>().color = new Color(accent.r, accent.g, accent.b, 0.55f);
        var barLE = bar.AddComponent<LayoutElement>();
        barLE.preferredHeight = 2f; barLE.flexibleWidth = 1f;

        // Icon — fixed square, never stretches
        var iconGO = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(cardGO.transform, false);
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite          = ability.icon;
        iconImg.preserveAspect  = true;
        iconImg.color           = ability.icon != null ? Color.white
            : new Color(accent.r, accent.g, accent.b, 0.35f);
        var ile = iconGO.AddComponent<LayoutElement>();
        ile.preferredWidth  = 52f;
        ile.preferredHeight = 52f;
        ile.flexibleWidth   = 0f;

        // Name
        var nameGO = MkTMP(cardGO.GetComponent<RectTransform>(), "Name",
            ability.abilityName.ToUpper(), 9f, FontStyles.Bold);
        nameGO.color     = accent;
        nameGO.alignment = TextAlignmentOptions.Center;
        nameGO.gameObject.AddComponent<LayoutElement>().preferredHeight = 14f;

        // Description
        var descGO = MkTMP(cardGO.GetComponent<RectTransform>(), "Desc",
            ability.description, 8f, FontStyles.Normal);
        descGO.color            = TextDim;
        descGO.alignment        = TextAlignmentOptions.Top;
        descGO.textWrappingMode = TextWrappingModes.Normal;
        descGO.gameObject.AddComponent<LayoutElement>().preferredHeight = 52f;
    }

    // ── Play / navigation ─────────────────────────────────────────────────────

    public void Play()
    {
        int idx = Mathf.Clamp(_sel, 0, (characters?.Length ?? 1) - 1);
        PlayerPrefs.SetInt("SelectedCharacter", idx);
        PlayerPrefs.Save();

        if (NetworkManager.singleton == null)
        {
            Debug.LogError("[CharSel] No NetworkManager — load LoginScene first.");
            if (_deployLabel) _deployLabel.text = "NO NETWORK MANAGER";
            return;
        }

        // Dev mode (login used the dev shortcut): host locally, skip the HTTP call.
        bool dev = PlayerPrefs.GetString("jwt_token", "") == "dev";
        if (dev)
        {
            if (_deployLabel) _deployLabel.text = "STARTING HOST...";
            NetworkManager.singleton.networkAddress = "localhost";
            NetworkManager.singleton.StartHost();
            return;
        }

        // Production: confirm the class on the DB, then connect to the game server.
        if (_deployBtn)   _deployBtn.interactable = false;
        if (_deployLabel) _deployLabel.text = "CONNECTING...";
        StartCoroutine(PostCharacterThenConnect(idx));
    }

    IEnumerator PostCharacterThenConnect(int classIndex)
    {
        string jwt      = PlayerPrefs.GetString("jwt_token", "");
        string serverIP = PlayerPrefs.GetString("game_server_ip", serverAddress);
        string url      = $"http://{serverIP}:3000/character";
        string json     = $"{{\"class_index\":{classIndex}}}";

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + jwt);
        req.timeout = 8;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[CharSel] POST /character failed: {req.error}");
            if (_deployBtn)   _deployBtn.interactable = true;
            if (_deployLabel) _deployLabel.text = "CONNECT FAILED — RETRY";
            yield break;
        }

        NetworkManager.singleton.networkAddress = serverIP;
        Debug.Log($"[CharSel] Class {classIndex} confirmed. Connecting to {serverIP}...");
        NetworkManager.singleton.StartClient();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    Image MkImg(RectTransform parent, string name, Color col)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>(); img.color = col;
        return img;
    }

    TextMeshProUGUI MkTMP(RectTransform parent, string name, string text, float size, FontStyles style)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style; tmp.color = TextPrim;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return tmp;
    }

    // Top-left anchored rect (x, topY from top edge, width, height)
    void RectSet(RectTransform rt, float x, float topY, float w, float h, bool top)
    {
        if (top)
        {
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
            rt.sizeDelta        = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(x + w * 0.5f, -(topY + h * 0.5f));
        }
        else // bottom
        {
            rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(1f, 0f);
            rt.sizeDelta        = new Vector2(0f, h);
            rt.anchoredPosition = new Vector2(0f, topY + h * 0.5f);
        }
    }

    void HRule(RectTransform parent, float topY, float panelW)
    {
        var go = new GameObject("Rule"); go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = new Color(0.22f, 0.21f, 0.30f, 0.65f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
        rt.sizeDelta        = new Vector2(panelW - 20f, 1f);
        rt.anchoredPosition = new Vector2(panelW * 0.5f, -topY);
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    void ClearChildren(Transform t)
    {
        if (!t) return;
        for (int i = t.childCount - 1; i >= 0; i--)
            Destroy(t.GetChild(i).gameObject);
    }

    void SetLayer(GameObject go, int layer)
    {
        if (layer < 0) return;
        go.layer = layer;
        foreach (Transform c in go.transform) SetLayer(c.gameObject, layer);
    }
}
