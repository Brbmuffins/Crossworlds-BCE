// Assets/Game/Scene/LoginUI.cs
// Self-bootstrapping. No scene setup needed.
#if !UNITY_SERVER
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// LoginUI — Crossworlds BCE login screen.
///
/// PvE dungeon grinder. Your squad is already inside.
/// The class colors pulse. The icon floats. The dungeon is live.
///
/// Icon: tries Resources/crossworlds_icon first, falls back to
///       downloading from playcrossworlds.com/icon.png at runtime.
///
/// Flow: POST /login → JWT → PlayerPrefs → SceneManager.LoadScene(1)
/// </summary>
public class LoginSceneUI : MonoBehaviour
{
    // ── Class data ────────────────────────────────────────────────────────────
    static readonly Color[] ClassColors =
    {
        new Color(0.29f, 0.56f, 0.89f), // Warden      — blue
        new Color(0.94f, 0.63f, 0.13f), // Ironclad    — gold
        new Color(0.61f, 0.35f, 0.71f), // Shadowblade — purple
        new Color(0.18f, 0.80f, 0.44f), // Cleric      — green
        new Color(0.91f, 0.30f, 0.24f), // Arcanist    — red
    };

    static readonly string[] ClassLines =
    {
        "WARDEN  ·  Front line. Eats the hits so the team doesn't.",
        "IRONCLAD  ·  Bruiser. Still standing when everyone else is down.",
        "SHADOWBLADE  ·  In, kill, out. No second chances for the monster.",
        "CLERIC  ·  Keeps the squad breathing. MVPs don't top damage charts.",
        "ARCANIST  ·  Nukes the room. The dungeon doesn't survive Act 3.",
    };

    const string AUTH_URL  = "http://15.204.243.36:3000/login";
    const string ICON_URL  = "https://playcrossworlds.com/icon.png";
    const float  CYCLE_SEC = 16f;

    // ── UI refs ───────────────────────────────────────────────────────────────
    TMP_InputField _username;
    TMP_InputField _password;
    TMP_Text       _statusText;
    TMP_Text       _classTicker;
    Image          _bgTint;
    Image          _accentLine;
    Image          _btnBg;
    Image[]        _underlines = new Image[2];

    // Icon + glow refs
    RawImage _iconImg;
    Image    _innerGlow;
    Image    _outerGlow;
    float    _iconT;

    int   _classIdx = 0;
    float _cycleT   = 0f;
    bool  _busy     = false;

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    // Disabled: existing LoginScene already has a login UI wired in the scene.
    // LoginBackgroundFX.cs handles the dungeon atmosphere behind it.
    // Re-enable this if you want LoginUI to fully replace the scene UI.
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0) return;
        var old = GameObject.Find("[LoginUI]");
        if (old != null) Destroy(old);
        new GameObject("[LoginUI]").AddComponent<LoginUI>();
    }

    // ── Init ──────────────────────────────────────────────────────────────────
    void Start()
    {
        Build();
        StartCoroutine(EnterKeyWatcher());
        StartCoroutine(LoadIcon());
    }

    // ── Update — color cycle + icon animation ─────────────────────────────────
    void Update()
    {
        // Class color cycle
        _cycleT += Time.deltaTime / CYCLE_SEC;
        if (_cycleT >= 1f) { _cycleT -= 1f; _classIdx = (_classIdx + 1) % ClassColors.Length; }

        int   next = (_classIdx + 1) % ClassColors.Length;
        Color c    = Color.Lerp(ClassColors[_classIdx], ClassColors[next], _cycleT);
        PushColor(c);

        // Ticker crossfade
        if (_classTicker != null)
        {
            float alpha = Mathf.Abs((_cycleT - 0.5f) * 2f);
            var   tc    = _classTicker.color;
            _classTicker.color = new Color(tc.r, tc.g, tc.b, Mathf.Lerp(0f, 0.75f, alpha));
            _classTicker.text  = ClassLines[_cycleT > 0.5f ? next : _classIdx];
        }

        // Icon float + glow pulse
        _iconT += Time.deltaTime;
        float bob   = Mathf.Sin(_iconT * 0.8f) * 4f;            // gentle 4px bob
        float pulse = 0.55f + 0.25f * Mathf.Sin(_iconT * 1.3f); // inner glow breathes
        float outer = 0.18f + 0.10f * Mathf.Sin(_iconT * 0.7f); // outer glow slower

        if (_iconImg != null)
        {
            var rt = _iconImg.rectTransform;
            rt.anchoredPosition = new Vector2(0f, bob);
        }

        if (_innerGlow != null)
            _innerGlow.color = new Color(c.r, c.g, c.b, pulse);
        if (_outerGlow != null)
            _outerGlow.color = new Color(c.r * 0.7f, c.g * 0.7f, c.b * 0.7f, outer);

        // Broadcast color to 3D background
        LoginBackgroundFX.ClassColor = c;
    }

    void PushColor(Color c)
    {
        if (_bgTint     != null) _bgTint.color     = new Color(c.r*0.04f, c.g*0.04f, c.b*0.05f, 1f);
        if (_accentLine != null) _accentLine.color  = new Color(c.r, c.g, c.b, 0.75f);
        if (_btnBg      != null) _btnBg.color       = new Color(c.r*0.55f, c.g*0.55f, c.b*0.6f, 0.92f);

        for (int i = 0; i < 2; i++)
        {
            if (_underlines[i] == null) continue;
            bool focused = (i == 0 && _username != null && _username.isFocused)
                        || (i == 1 && _password != null && _password.isFocused);
            _underlines[i].color = focused
                ? new Color(c.r, c.g, c.b, 1f)
                : new Color(0.35f, 0.35f, 0.38f, 0.7f);
        }
    }

    // ── Icon load — local first, web fallback ─────────────────────────────────
    IEnumerator LoadIcon()
    {
        // Try Resources folder (place icon.png there as crossworlds_icon.png)
        var localTex = Resources.Load<Texture2D>("crossworlds_icon");
        if (localTex != null)
        {
            ApplyIconTexture(localTex);
            yield break;
        }

        // Web fallback
        var req = UnityWebRequestTexture.GetTexture(ICON_URL);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            ApplyIconTexture(DownloadHandlerTexture.GetContent(req));
        // If both fail, the glow still shows — mystical void orb
    }

    void ApplyIconTexture(Texture2D tex)
    {
        if (_iconImg == null) return;
        _iconImg.texture = tex;
        _iconImg.color   = Color.white;
    }

    // ── Login flow ────────────────────────────────────────────────────────────
    void TryLogin()
    {
        if (_busy) return;
        string user = _username?.text.Trim() ?? "";
        string pass = _password?.text ?? "";
        if (user.Length == 0 || pass.Length == 0) { Status("Name and password required.", true); return; }
        StartCoroutine(DoLogin(user, pass));
    }

    IEnumerator DoLogin(string user, string pass)
    {
        _busy = true;
        Status("Dropping in...", false);

        string body = $"{{\"username\":\"{EscJ(user)}\",\"password\":\"{EscJ(pass)}\"}}";
        var req     = new UnityWebRequest(AUTH_URL, "POST");
        req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Status("Can't reach the server. Is it up?", true);
            _busy = false;
            yield break;
        }

        string resp = req.downloadHandler.text;
        if (resp.Contains("\"token\""))
        {
            int q1 = resp.IndexOf('"', resp.IndexOf("\"token\"") + 8);
            int q2 = resp.IndexOf('"', q1 + 1);
            string jwt = resp.Substring(q1 + 1, q2 - q1 - 1);
            PlayerPrefs.SetString("jwt_token", jwt);
            PlayerPrefs.SetString("username",  user);
            PlayerPrefs.Save();
            Status("Entering the dungeon...", false);
            yield return new WaitForSeconds(0.7f);
            SceneManager.LoadScene(1);
        }
        else
        {
            bool bad = resp.ToLower().Contains("invalid") || resp.ToLower().Contains("wrong");
            Status(bad ? "Wrong credentials. Check with the team." : "Login failed — server returned an error.", true);
            _busy = false;
        }
    }

    void Status(string msg, bool error)
    {
        if (_statusText == null) return;
        _statusText.text  = msg;
        _statusText.color = error ? new Color(0.95f,0.35f,0.25f,1f) : new Color(0.65f,0.65f,0.65f,0.85f);
    }

    static string EscJ(string s) => s.Replace("\\","\\\\").Replace("\"","\\\"");

    IEnumerator EnterKeyWatcher()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                TryLogin();
            yield return null;
        }
    }

    // ── Build UI ──────────────────────────────────────────────────────────────
    void Build()
    {
        var cGO            = new GameObject("LoginCanvas");
        cGO.transform.SetParent(transform);
        var canvas         = cGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Full-screen dark background (shifts tint with class color)
        _bgTint = R(cGO,"BgTint",0,0,1,1).Img(new Color(0.02f,0.02f,0.04f,1f));

        // Vignette
        R(cGO,"VigTop",    0f,0.84f,1f,1f  ).Img(new Color(0,0,0,0.7f));
        R(cGO,"VigBottom", 0f,0f,   1f,0.1f).Img(new Color(0,0,0,0.7f));
        R(cGO,"VigLeft",   0f,0f,0.07f,1f  ).Img(new Color(0,0,0,0.45f));
        R(cGO,"VigRight",  0.93f,0f,1f,1f  ).Img(new Color(0,0,0,0.45f));

        // ── Icon — outer glow ring ────────────────────────────────────────────
        // Outer soft glow (largest, dimmest)
        var outerGlowGO = R(cGO,"IconGlowOuter", 0.41f,0.795f, 0.59f,0.92f);
        _outerGlow = outerGlowGO.Img(new Color(0.3f,0.3f,0.5f,0.15f));

        // Inner glow (smaller, brighter, breathes)
        var innerGlowGO = R(cGO,"IconGlowInner", 0.43f,0.810f, 0.57f,0.910f);
        _innerGlow = innerGlowGO.Img(new Color(0.3f,0.4f,0.8f,0.45f));

        // The icon itself
        var iconGO = R(cGO,"Icon", 0.44f,0.818f, 0.56f,0.905f);
        _iconImg = iconGO.GO.AddComponent<RawImage>();
        _iconImg.color = new Color(1f,1f,1f,0f); // hidden until texture loads

        // ── Title ─────────────────────────────────────────────────────────────
        // Class ticker (very top)
        _classTicker = R(cGO,"Ticker",0.1f,0.925f,0.9f,0.955f)
            .Txt(ClassLines[0], 11, TextAlignmentOptions.Center,
                 new Color(0.6f,0.6f,0.6f,0f), 2f);

        // "CROSSWORLDS"
        R(cGO,"Title",0.05f,0.74f,0.95f,0.82f)
            .Txt("CROSSWORLDS", 84, TextAlignmentOptions.Center, Color.white, 14f, FontStyles.Bold);

        // "BATTLE CLASH EDITION"
        R(cGO,"Edition",0.2f,0.705f,0.8f,0.742f)
            .Txt("BATTLE CLASH EDITION", 13, TextAlignmentOptions.Center,
                 new Color(0.55f,0.55f,0.6f,0.9f), 7f);

        // Tagline
        R(cGO,"Tag",0.2f,0.668f,0.8f,0.705f)
            .Txt("The dungeon is live.", 15, TextAlignmentOptions.Center,
                 new Color(0.6f,0.6f,0.6f,0.45f), 0f, FontStyles.Italic);

        // Accent line
        _accentLine = R(cGO,"Line",0.35f,0.658f,0.65f,0.663f)
            .Img(new Color(0.5f,0.5f,0.9f,0.7f));

        // ── Login panel ───────────────────────────────────────────────────────
        var panel = R(cGO,"Panel", 0.32f,0.245f, 0.68f,0.650f);
        panel.Img(new Color(0.055f,0.055f,0.08f,0.92f));
        Border(panel.GO);

        _username      = Field(panel.GO,"WARRIOR NAME", false,  0.75f, 0.93f);
        _underlines[0] = Under(panel.GO,"WARRIOR NAME");

        _password      = Field(panel.GO,"PASSWORD",     true,   0.40f, 0.58f);
        _underlines[1] = Under(panel.GO,"PASSWORD");

        // Drop In button
        var btn = R(panel.GO,"Btn", 0.06f,0.07f, 0.94f,0.28f);
        _btnBg  = btn.Img(new Color(0.3f,0.3f,0.5f,0.9f));
        var b   = btn.GO.AddComponent<Button>();
        b.targetGraphic = _btnBg;
        var cb  = ColorBlock.defaultColorBlock;
        cb.normalColor = Color.white; cb.highlightedColor = new Color(1.15f,1.15f,1.15f,1f);
        cb.pressedColor = new Color(0.75f,0.75f,0.75f,1f); b.colors = cb;
        b.onClick.AddListener(TryLogin);
        R(btn.GO,"L",0,0,1,1).Txt("DROP IN", 14, TextAlignmentOptions.Center,
                                  Color.white, 8f, FontStyles.Bold);

        // Status
        _statusText = R(cGO,"Status",0.2f,0.185f,0.8f,0.24f)
            .Txt("", 12, TextAlignmentOptions.Center, new Color(0.6f,0.6f,0.6f,0.7f));

        // Footer
        R(cGO,"Footer",0.05f,0.02f,0.95f,0.065f)
            .Txt("● SERVER ONLINE  ·  PLAYCROSSWORLDS.COM  ·  INVITE ONLY  ·  PVE ALPHA",
                 10, TextAlignmentOptions.Center, new Color(0.3f,0.65f,0.3f,0.55f), 2f);
    }

    // ── Input field builder ───────────────────────────────────────────────────
    TMP_InputField Field(GameObject parent, string label, bool isPass,
        float yMin, float yMax)
    {
        R(parent, label+"_Lbl", 0.06f, yMax+0.01f, 0.94f, yMax+0.1f)
            .Txt(label, 9, TextAlignmentOptions.Left,
                 new Color(0.45f,0.45f,0.5f,1f), 3f);

        var ctr  = R(parent, label+"_Ctr", 0.06f, yMin, 0.94f, yMax);
        var area = R(ctr.GO,"TA",0f,0.2f,1f,1f);
        area.GO.AddComponent<RectMask2D>();

        var ph = R(area.GO,"PH",0,0,1,1)
            .Txt(isPass ? "••••••••" : "enter name", 15, TextAlignmentOptions.Left,
                 new Color(0.3f,0.3f,0.33f,1f), 0f, FontStyles.Italic);
        var tx = R(area.GO,"TX",0,0,1,1)
            .Txt("", 15, TextAlignmentOptions.Left, Color.white);

        // Underline bar
        R(ctr.GO, label+"_Line", 0f,0f, 1f,0.1f)
            .Img(new Color(0.35f,0.35f,0.38f,0.7f));

        var f = ctr.GO.AddComponent<TMP_InputField>();
        f.textViewport  = area.RT;
        f.textComponent = (TMP_Text)tx;
        f.placeholder   = (TMP_Text)ph;
        f.caretColor    = Color.white;
        f.caretWidth    = 2;
        f.selectionColor = new Color(0.4f,0.4f,0.8f,0.35f);
        if (isPass) { f.contentType = TMP_InputField.ContentType.Password; f.asteriskChar = '●'; }
        return f;
    }

    Image Under(GameObject panel, string label)
    {
        var t = panel.transform.Find(label + "_Ctr/" + label + "_Line");
        return t != null ? t.GetComponent<Image>() : null;
    }

    void Border(GameObject p)
    {
        var c = new Color(0.22f,0.22f,0.3f,0.5f);
        R(p,"BL",0f,0f,0.003f,1f).Img(c);   R(p,"BR",0.997f,0f,1f,1f).Img(c);
        R(p,"BT",0f,0.996f,1f,1f).Img(c);   R(p,"BB",0f,0f,1f,0.004f).Img(c);
    }

    // ── Fluent builder ────────────────────────────────────────────────────────
    N R(GameObject p, string name, float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(name);
        go.transform.SetParent(p.transform, false);
        var rt       = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(x0,y0);
        rt.anchorMax = new Vector2(x1,y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return new N(go,rt);
    }

    struct N
    {
        public GameObject    GO;
        public RectTransform RT;
        public N(GameObject g, RectTransform r) { GO=g; RT=r; }

        public Image Img(Color c)
        { var i=GO.AddComponent<Image>(); i.color=c; return i; }

        public TMP_Text Txt(string t, float sz, TextAlignmentOptions align, Color col,
            float spacing=0f, FontStyles style=FontStyles.Normal)
        {
            var tmp = GO.AddComponent<TextMeshProUGUI>();
            tmp.text=t; tmp.fontSize=sz; tmp.alignment=align;
            tmp.color=col; tmp.characterSpacing=spacing; tmp.fontStyle=style;
            tmp.enableWordWrapping=false;
            return tmp;
        }
    }
}
#endif
