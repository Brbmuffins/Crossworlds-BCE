using System;
using System.Collections;
using System.Collections.Generic;
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
//  Inspector:  drop 6 CharacterData assets into Characters[0-5].
//              Everything else (camera, RT, lights) auto-builds at runtime.
//              Optionally assign a previewCamera/RT/spawnPoint to override.
// ═══════════════════════════════════════════════════════════════════════════

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Roster")]
    public CharacterData[] characters;

    [Header("Server")]
    public string serverAddress = "15.204.243.36";

    [Header("Scene Character Stage")]
    [Tooltip("Use the scene camera and place selected characters directly in the scene.")]
    public bool          useSceneStage = true;
    [Tooltip("Optional scene model used as the exact preview pose. If empty, an object named Arcanist is used.")]
    public GameObject    sceneModelReference;

    [Header("Legacy Render Texture Preview")]
    public Camera        previewCamera;
    public RenderTexture previewRenderTexture;
    public Transform     previewSpawnPoint;
    public float         rotationSpeed = 22f;
    [Tooltip("Scene-stage height relative to Arcanist, in class-index order.")]
    public float[]       sceneClassHeightMultipliers = { 1.22f, 1.12f, 0.96f, 1.02f, 1.00f, 1.18f };

    // ── Runtime refs ─────────────────────────────────────────────────────────
    int           _sel;
    GameObject    _previewInstance;
    GameObject    _previewRoot;
    RenderTexture _rt;
    bool          _usingSceneStage;
    Bounds        _sceneReferenceBounds;
    bool          _hasSceneReferenceBounds;

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
    TextMeshProUGUI[] _availabilityBadges;

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
        CharacterClassAvailability.AvailabilityChanged += RefreshAvailability;
        EnsureSingleEventSystem();
        BuildPreview();
        BuildUI();
        if (characters != null && characters.Length > 0)
            ShowClass(0);
    }

    // Two enabled EventSystems (e.g. a DontDestroyOnLoad one carried in from a gameplay
    // scene plus this scene's own) make uGUI silently stop processing clicks — you reach
    // character select after logout but nothing is clickable. Rebuild a single clean
    // EventSystem synchronously so the Input System's UI actions stay enabled.
    void EnsureSingleEventSystem() => SingleEventSystem.ForceSingle();

    void Update()
    {
        // Scene-stage characters hold their authored facing. The legacy isolated
        // preview keeps its turntable behavior.
        if (_previewRoot != null && !_usingSceneStage)
            _previewRoot.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.leftArrowKey.wasPressedThisFrame)  ShowClass((_sel - 1 + characters.Length) % characters.Length);
        if (kb.rightArrowKey.wasPressedThisFrame) ShowClass((_sel + 1) % characters.Length);
    }

    void OnDestroy()
    {
        CharacterClassAvailability.AvailabilityChanged -= RefreshAvailability;
        if (_rt != null) { _rt.Release(); Destroy(_rt); }
    }

    // ── 3D Preview setup ──────────────────────────────────────────────────────

    void BuildPreview()
    {
        if (useSceneStage)
        {
            if (sceneModelReference == null)
                sceneModelReference = FindSceneModelReference("Arcanist");

            Transform stagePose = previewSpawnPoint != null
                ? previewSpawnPoint
                : sceneModelReference != null ? sceneModelReference.transform : null;

            if (stagePose != null)
            {
                _usingSceneStage = true;
                _hasSceneReferenceBounds = sceneModelReference != null
                    && TryGetBodyBounds(sceneModelReference, out _sceneReferenceBounds);
                var root = new GameObject("CharacterPreview_StageRoot");
                root.transform.SetPositionAndRotation(stagePose.position, stagePose.rotation);
                root.transform.localScale = Vector3.one;
                _previewRoot = root;
                previewSpawnPoint = root.transform;

                if (sceneModelReference != null)
                    sceneModelReference.SetActive(false);
                return;
            }

            Debug.LogWarning("[CharSel] No scene preview pose found. Falling back to the render-texture preview.");
        }

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
        var platMat = plat.GetComponent<Renderer>().material;
        var urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader != null) platMat.shader = urpShader;
        platMat.color = new Color(0.06f, 0.06f, 0.12f);
        SetLayer(plat, PREV_LAY);

        // Ring
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "_PrevRing";
        Destroy(ring.GetComponent<Collider>());
        ring.transform.SetPositionAndRotation(new Vector3(0f, -0.07f, 0f), Quaternion.identity);
        ring.transform.localScale = new Vector3(2.0f, 0.02f, 2.0f);
        var ringMat = ring.GetComponent<Renderer>().material;
        if (urpShader != null) ringMat.shader = urpShader;
        ringMat.color = new Color(0.08f, 0.08f, 0.16f);
        SetLayer(ring, PREV_LAY);

        // Spawn point + rotation root
        var spawnGO = new GameObject("_PrevSpawn");
        spawnGO.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        previewSpawnPoint = spawnGO.transform;

        _previewRoot = new GameObject("_PrevRoot");
        _previewRoot.transform.position = Vector3.zero;
    }

    static GameObject FindSceneModelReference(string objectName)
    {
        foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            // Unity appends " (1)", " (2)", etc. when a scene prefab is
            // deleted and re-added while another object used the base name.
            bool nameMatches = candidate.name == objectName
                || candidate.name.StartsWith(objectName + " (");
            if (candidate.scene.IsValid() && nameMatches)
                return candidate;
        }
        return null;
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
        if (_bgPanel && !_usingSceneStage)
            _bgPanel.color = Color.Lerp(BgDeep, d.classColorDark, 0.20f);

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
        RefreshAvailability();

        // 3D preview
        if (_previewInstance != null) Destroy(_previewInstance);
        GameObject prefab = d.previewPrefab != null ? d.previewPrefab : d.prefab;
        Transform spawnParent = _previewRoot != null ? _previewRoot.transform : previewSpawnPoint;
        if (prefab != null && spawnParent != null)
        {
            // Stage the clone beneath an inactive object so gameplay Awake methods
            // (notably Mirror.NetworkAnimator) cannot run before preview cleanup.
            var staging = new GameObject("PreviewStaging");
            staging.SetActive(false);
            staging.transform.SetParent(spawnParent, false);

            _previewInstance = Instantiate(prefab, staging.transform);

            // NetworkAnimator initializes from Awake even when it is disabled, so
            // remove it from this visual-only clone before activation. Keep the
            // remaining gameplay component graph intact because several scripts
            // use RequireComponent dependencies (for example CharacterStats
            // depends on Health); disabling them is sufficient for the preview.
            foreach (var networkAnimator in
                     _previewInstance.GetComponentsInChildren<Mirror.NetworkAnimator>(true))
            {
                if (networkAnimator) DestroyImmediate(networkAnimator);
            }

            foreach (var mb in _previewInstance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb) mb.enabled = false;
            }

            _previewInstance.transform.SetParent(spawnParent, false);
            Destroy(staging);
            _previewInstance.transform.localPosition = Vector3.zero;
            _previewInstance.transform.localRotation = Quaternion.identity;
            _previewInstance.transform.localScale    = Vector3.one;

            // Animator is not a MonoBehaviour, so it remains active on the visual
            // clone. Evaluate its default state before measuring bounds; otherwise
            // new humanoid rigs are fitted in bind pose and move after the first
            // animated frame, which makes them appear buried in the stage.
            foreach (var animator in _previewInstance.GetComponentsInChildren<Animator>(true))
            {
                animator.applyRootMotion = false;
                animator.Rebind();
                animator.Update(0f);
            }

            if (!_usingSceneStage)
            {
                SetLayer(_previewInstance, PREV_LAY);
                FitPreview(_previewInstance);   // bounds-based auto-scale + ground on platform
            }
            else
            {
                FitScenePreview(_previewInstance);
                StartCoroutine(RefitScenePreviewAfterAnimation(_previewInstance));
            }

            // Neutralize preview physics. Animator/renderers remain active so the
            // selected character can hold its authored idle pose.
            foreach (var rb in _previewInstance.GetComponentsInChildren<Rigidbody>(true))
                rb.isKinematic = true;
            foreach (var col in _previewInstance.GetComponentsInChildren<Collider>(true))
                col.enabled = false;
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

    // Match the selected prefab to the authored scene model instead of inheriting
    // its root scale. Different class prefabs have different native mesh sizes.
    void FitScenePreview(GameObject go)
    {
        if (!_hasSceneReferenceBounds || !TryGetBodyBounds(go, out Bounds bounds))
            return;
        if (bounds.size.y < 0.0001f)
            return;

        float classHeight = 1f;
        if (sceneClassHeightMultipliers != null && _sel < sceneClassHeightMultipliers.Length)
            classHeight = Mathf.Max(0.1f, sceneClassHeightMultipliers[_sel]);

        float scale = (_sceneReferenceBounds.size.y * classHeight) / bounds.size.y;
        go.transform.localScale *= scale;

        if (!TryGetBodyBounds(go, out bounds))
            return;

        Vector3 offset = new Vector3(
            _sceneReferenceBounds.center.x - bounds.center.x,
            _sceneReferenceBounds.min.y - bounds.min.y,
            _sceneReferenceBounds.center.z - bounds.center.z);
        go.transform.position += offset;
    }

    IEnumerator RefitScenePreviewAfterAnimation(GameObject instance)
    {
        // Let the Animator advance out of bind pose, then fit the actual rendered
        // pose. Guard against a rapid class switch destroying this instance.
        yield return null;
        if (instance != null && instance == _previewInstance)
            FitScenePreview(instance);
    }

    static bool TryGetBodyBounds(GameObject go, out Bounds bounds)
    {
        // Weapons and quivers can extend below the feet. Prefer the tallest
        // skinned character mesh so those accessories do not drive grounding.
        var skinned = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        Renderer body = null;
        float tallest = 0f;
        foreach (var renderer in skinned)
        {
            if (renderer.bounds.size.y > tallest)
            {
                body = renderer;
                tallest = renderer.bounds.size.y;
            }
        }

        if (body == null)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer.bounds.size.y > tallest)
                {
                    body = renderer;
                    tallest = renderer.bounds.size.y;
                }
            }
        }

        if (body == null)
        {
            bounds = default;
            return false;
        }

        bounds = body.bounds;
        return true;
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
        _bgPanel = MkImg(root, "BG", _usingSceneStage ? Clear : BgDeep);
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
        _availabilityBadges = new TextMeshProUGUI[characters.Length];

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

            var badge = MkTMP(rt, "Availability", "IN DEVELOPMENT", 8f, FontStyles.Bold);
            badge.color = new Color(0.82f, 0.64f, 0.34f, 1f);
            badge.alignment = TextAlignmentOptions.TopLeft;
            RectSet(badge.rectTransform, 14f, 55f, LEFT_W - 20f, 14f, top: true);
            _availabilityBadges[i] = badge;

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
        var panel = MkImg(root, "Center", _usingSceneStage ? Clear : BgDeep);
        var rt    = panel.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(LEFT_W, 0f);
        rt.offsetMax = new Vector2(-RIGHT_W, 0f);

        if (!_usingSceneStage)
        {
            // RenderTexture display (legacy fallback only).
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
        }

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
        if (!CharacterClassAvailability.IsPlayable(idx))
        {
            RefreshAvailability();
            Debug.LogWarning("[CharSel] Deploy blocked: selected class is still in development.");
            return;
        }
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
        string serverIP = PlayerPrefs.GetString("game_server_ip", serverAddress).Trim();
        string url      = $"{ServerConfig.AuthBaseUrl}/character";  // environment-aware (dev → :3010)
        string json     = $"{{\"class_index\":{classIndex}}}";

        using var req = new UnityWebRequest();
        req.url = url;
        req.method = "POST";
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + jwt);
        req.timeout = 8;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[CharSel] POST /character failed: {req.error}");
            ResetDeployButton("CONNECT FAILED — RETRY");
            yield break;
        }

        // Verify the server actually wrote the class we selected.
        // If not, the DB value will override at spawn — warn loudly so it shows in logs.
        try
        {
            var charData = JsonUtility.FromJson<CharacterApiResponse>(req.downloadHandler.text);
            if (charData == null || charData.id <= 0 || charData.class_index != classIndex ||
                string.IsNullOrEmpty(charData.token))
            {
                Debug.LogError($"[CharSel] Server did not return a character-bound token for class {classIndex}. " +
                               "Deployment stopped so progression cannot be applied to the wrong character.");
                ResetDeployButton("CLASS UPDATE FAILED — RETRY");
                yield break;
            }
            PlayerPrefs.SetString("jwt_token", charData.token);
            PlayerPrefs.Save();
            AuthManager.Token = charData.token;
            AuthManager.CharacterId = charData.id;
        }
        catch { /* non-fatal: response shape may differ */ }

        // If a previous session is still live, stop it before starting a new one.
        // Mirror silently ignores StartClient() when already active, causing a permanent "CONNECTING…" lock.
        if (NetworkClient.active)
        {
            Debug.Log("[CharSel] NetworkClient still active — stopping before reconnect.");
            NetworkManager.singleton.StopClient();
            yield return new WaitForSeconds(0.3f);
        }

        NetworkManager.singleton.networkAddress = serverIP;

        // Environment-aware port: prod → 7777, dev → 7778. The dev game service is a
        // separate systemd unit on the same box, launched with -port 7778 -authurl :3010.
        if (NetworkManager.singleton.transport is PortTransport pt)
            pt.Port = ServerConfig.GamePort;

        Debug.Log($"[CharSel] Class {classIndex} confirmed. Connecting to {serverIP}:{ServerConfig.GamePort} " +
                  $"({ServerConfig.Environment})...");
        NetworkManager.singleton.StartClient();

        // Timeout guard: re-enable the button if the scene never changes.
        StartCoroutine(ConnectionTimeout(15f));
    }

    IEnumerator ConnectionTimeout(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_deployBtn != null && !_deployBtn.interactable)
        {
            NetworkManager.singleton?.StopClient();
            ResetDeployButton("CONNECTION TIMED OUT — RETRY");
            Debug.LogWarning("[CharSel] Connection timed out — button re-enabled.");
        }
    }

    void ResetDeployButton(string label)
    {
        if (_deployBtn)   _deployBtn.interactable = CharacterClassAvailability.IsPlayable(_sel);
        if (_deployLabel) _deployLabel.text = CharacterClassAvailability.IsPlayable(_sel)
            ? label
            : "IN DEVELOPMENT";
    }

    public void RefreshAvailability()
    {
        bool selectedPlayable = CharacterClassAvailability.IsPlayable(_sel);
        if (_deployBtn) _deployBtn.interactable = selectedPlayable;
        if (_deployLabel) _deployLabel.text = selectedPlayable ? "DEPLOY" : "IN DEVELOPMENT";

        if (_availabilityBadges == null) return;
        for (int i = 0; i < _availabilityBadges.Length; i++)
        {
            if (_availabilityBadges[i] == null) continue;
            bool normallyLocked = i == 1 || i == 2;
            _availabilityBadges[i].gameObject.SetActive(normallyLocked);
            _availabilityBadges[i].text = CharacterClassAvailability.IsPlayable(i)
                ? "GM TEST BUILD"
                : "IN DEVELOPMENT";
            _availabilityBadges[i].color = CharacterClassAvailability.IsPlayable(i)
                ? new Color(0.45f, 0.82f, 0.65f, 1f)
                : new Color(0.82f, 0.64f, 0.34f, 1f);
        }
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

    [System.Serializable]
    class CharacterApiResponse { public int id; public int class_index; public string token; }
}

/// <summary>
/// Central policy for classes that are visible for preview but not ready to deploy.
/// The testing override intentionally lasts only for the current app session.
/// </summary>
public static class CharacterClassAvailability
{
    static readonly HashSet<int> UnplayableClassIndices = new HashSet<int> { 1, 2 };

    static readonly HashSet<string> GmUsers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "DevPlayer",
        "brbmuffins",
        "ForYurHealth",
        "YaDingusMD",
        "SleepyBoySteve",
    };

    public static event Action AvailabilityChanged;

    public static bool TestingOverrideEnabled { get; private set; }

    public static bool IsPlayable(int classIndex)
    {
        return TestingOverrideEnabled || !UnplayableClassIndices.Contains(classIndex);
    }

    public static bool IsCurrentUserGm()
    {
        return GmUsers.Contains(PlayerPrefs.GetString("username", ""));
    }

    public static bool TryEnableTestingOverride()
    {
        if (!IsCurrentUserGm()) return false;
        if (TestingOverrideEnabled) return true;

        TestingOverrideEnabled = true;
        AvailabilityChanged?.Invoke();
        return true;
    }
}
