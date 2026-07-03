// Assets/Game/Scene/LoginBackgroundFX.cs
// Self-bootstrapping arena dungeon gate. Zero scene setup.
#if !UNITY_SERVER
using UnityEngine;

/// <summary>
/// LoginBackgroundFX — 3D dungeon gate atmosphere for the login scene.
///
/// Creates in world space:
///   - Stone ground plane (dark, fog-covered)
///   - Two torch pillars with flickering orange point lights
///   - Monster silhouettes lurking in the background
///   - Ember particle streams rising from each torch
///   - Ambient mote particles drifting across the arena floor
///   - Ground fog (Unity scene fog)
///
/// LoginUI.ClassColor drives the torch/ambient tint in real time.
/// </summary>
public class LoginBackgroundFX : MonoBehaviour
{
    // LoginUI pushes the current class color here every frame
    public static Color ClassColor = new Color(0.5f, 0.4f, 0.8f);

    // Torch light refs for flicker
    Light _torchL;
    Light _torchR;
    float _flickerT;

    // Camera pulse (very subtle)
    Camera _cam;
    float  _camT;

    // ── Bootstrap ─────────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0) return;
        var old = GameObject.Find("[LoginBG]");
        if (old != null) Destroy(old);
        new GameObject("[LoginBG]").AddComponent<LoginBackgroundFX>();
    }

    // ── Init ──────────────────────────────────────────────────────────────────
    void Start()
    {
        SetupCamera();
        SetupFog();
        BuildGround();
        BuildArenaWalls();
        BuildTorches();
        BuildSilhouettes();
        BuildEmbers();
        BuildMotes();
        BuildGateFrame();
    }

    // ── Update — flicker + ambient pulse ──────────────────────────────────────
    void Update()
    {
        _flickerT += Time.deltaTime;

        // Torch flicker — layered sine waves so it feels organic
        float flicker = 1f
            + 0.18f * Mathf.Sin(_flickerT * 7.3f)
            + 0.09f * Mathf.Sin(_flickerT * 13.7f)
            + 0.05f * Mathf.Sin(_flickerT * 31.1f);

        if (_torchL != null) _torchL.intensity = 1.4f * flicker;
        if (_torchR != null) _torchR.intensity = 1.4f * flicker * (1f + 0.06f * Mathf.Sin(_flickerT * 5.2f));

        // Subtle ambient tint shift with class color
        RenderSettings.ambientLight = Color.Lerp(
            new Color(0.04f, 0.03f, 0.06f),
            new Color(ClassColor.r * 0.08f, ClassColor.g * 0.08f, ClassColor.b * 0.09f),
            0.6f);

        // Very subtle camera bob (arena breathing)
        _camT += Time.deltaTime * 0.4f;
        if (_cam != null)
        {
            float bob = Mathf.Sin(_camT) * 0.015f;
            _cam.transform.localPosition = new Vector3(0f, 1.8f + bob, -3f);
        }
    }

    // ── Camera ────────────────────────────────────────────────────────────────
    void SetupCamera()
    {
        _cam = Camera.main;
        if (_cam == null) return;
        _cam.clearFlags      = CameraClearFlags.SolidColor;
        _cam.backgroundColor = new Color(0.018f, 0.014f, 0.025f, 1f); // deep void purple-black
        _cam.fieldOfView     = 55f;
        _cam.transform.position = new Vector3(0f, 1.8f, -3f);
        _cam.transform.rotation = Quaternion.Euler(8f, 0f, 0f);    // slight downward look
    }

    // ── Fog ───────────────────────────────────────────────────────────────────
    void SetupFog()
    {
        RenderSettings.fog           = true;
        RenderSettings.fogMode       = FogMode.Exponential;
        RenderSettings.fogDensity    = 0.045f;
        RenderSettings.fogColor      = new Color(0.025f, 0.02f, 0.04f, 1f);
        RenderSettings.ambientMode   = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight  = new Color(0.04f, 0.03f, 0.06f);
    }

    // ── Ground plane ──────────────────────────────────────────────────────────
    void BuildGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(transform);
        ground.transform.localPosition = Vector3.zero;
        ground.transform.localScale    = new Vector3(4f, 1f, 4f); // 40x40 units
        Destroy(ground.GetComponent<Collider>());

        var mat   = new Material(Shader.Find("Universal Render Pipeline/Lit")
                               ?? Shader.Find("Standard"));
        mat.color = new Color(0.06f, 0.055f, 0.07f, 1f); // very dark stone
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.05f);
        if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic",   0f);
        ground.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // ── Back walls (arena gate) ───────────────────────────────────────────────
    void BuildArenaWalls()
    {
        // Back wall
        MakeWall("WallBack", new Vector3(0f, 5f, 18f), new Vector3(40f, 10f, 0.5f));
        // Left wall (receding)
        MakeWall("WallLeft",  new Vector3(-12f, 5f, 10f), new Vector3(0.5f, 10f, 16f));
        // Right wall (receding)
        MakeWall("WallRight", new Vector3( 12f, 5f, 10f), new Vector3(0.5f, 10f, 16f));
    }

    void MakeWall(string name, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform);
        go.transform.localPosition = pos;
        go.transform.localScale    = scale;
        Destroy(go.GetComponent<Collider>());
        var mat   = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = new Color(0.055f, 0.048f, 0.065f, 1f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // ── Gate frame (stone arch) ───────────────────────────────────────────────
    void BuildGateFrame()
    {
        // Left pillar
        MakePillar("GatePillarL", new Vector3(-2.8f, 3.5f, 8f));
        // Right pillar
        MakePillar("GatePillarR", new Vector3( 2.8f, 3.5f, 8f));
        // Lintel (top bar)
        var lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lintel.name = "GateLintel";
        lintel.transform.SetParent(transform);
        lintel.transform.localPosition = new Vector3(0f, 7.5f, 8f);
        lintel.transform.localScale    = new Vector3(6.5f, 0.8f, 0.7f);
        Destroy(lintel.GetComponent<Collider>());
        lintel.GetComponent<MeshRenderer>().sharedMaterial = DarkStoneMat();

        // Gate glow (emanates from inside the arch — the dungeon beyond)
        var glowGO = new GameObject("GateGlow");
        glowGO.transform.SetParent(transform);
        glowGO.transform.localPosition = new Vector3(0f, 3.5f, 9f);
        var glow = glowGO.AddComponent<Light>();
        glow.type      = LightType.Point;
        glow.color     = new Color(0.4f, 0.2f, 0.7f, 1f);  // purple void glow
        glow.intensity = 0.9f;
        glow.range     = 6f;
        glow.shadows   = LightShadows.None;
    }

    void MakePillar(string name, Vector3 pos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform);
        go.transform.localPosition = pos;
        go.transform.localScale    = new Vector3(0.9f, 7f, 0.7f);
        Destroy(go.GetComponent<Collider>());
        go.GetComponent<MeshRenderer>().sharedMaterial = DarkStoneMat();
    }

    Material DarkStoneMat()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = new Color(0.07f, 0.06f, 0.08f, 1f);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0f);
        return mat;
    }

    // ── Torch pillars ─────────────────────────────────────────────────────────
    void BuildTorches()
    {
        _torchL = BuildTorch("TorchL", new Vector3(-5.5f, 0f, 5f));
        _torchR = BuildTorch("TorchR", new Vector3( 5.5f, 0f, 5f));
    }

    Light BuildTorch(string name, Vector3 basePos)
    {
        var root = new GameObject(name);
        root.transform.SetParent(transform);
        root.transform.localPosition = basePos;

        // Pillar body
        var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.transform.SetParent(root.transform);
        pillar.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        pillar.transform.localScale    = new Vector3(0.18f, 1.5f, 0.18f);
        Destroy(pillar.GetComponent<Collider>());
        var pm = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        pm.color = new Color(0.25f, 0.18f, 0.1f, 1f); // dark wood
        pillar.GetComponent<MeshRenderer>().sharedMaterial = pm;

        // Torch cup
        var cup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cup.transform.SetParent(root.transform);
        cup.transform.localPosition = new Vector3(0f, 3.1f, 0f);
        cup.transform.localScale    = new Vector3(0.28f, 0.22f, 0.28f);
        Destroy(cup.GetComponent<Collider>());
        cup.GetComponent<MeshRenderer>().sharedMaterial = pm;

        // Flame light
        var lightGO = new GameObject("Light");
        lightGO.transform.SetParent(root.transform);
        lightGO.transform.localPosition = new Vector3(0f, 3.4f, 0f);
        var light = lightGO.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = new Color(1f, 0.55f, 0.1f);  // warm orange
        light.intensity = 1.4f;
        light.range     = 9f;
        light.shadows   = LightShadows.None;

        return light;
    }

    // ── Monster silhouettes ───────────────────────────────────────────────────
    // Dark lurking shapes in the background — players know something's in there
    void BuildSilhouettes()
    {
        // Silhouette data: (position, scale) — lurking at varying distances
        var shapes = new (Vector3 pos, Vector3 scale)[]
        {
            // Far background, standing
            (new Vector3(-7f,  1f,  16f), new Vector3(0.7f, 2.2f, 0.4f)),
            (new Vector3( 8f,  1f,  14f), new Vector3(0.7f, 2.0f, 0.4f)),
            (new Vector3( 0f,  1f,  19f), new Vector3(1.1f, 2.8f, 0.5f)), // bigger/closer to gate

            // Mid-distance, hunched
            (new Vector3(-10f, 0.6f, 11f), new Vector3(0.9f, 1.5f, 0.5f)),
            (new Vector3( 9f,  0.6f, 12f), new Vector3(0.8f, 1.6f, 0.5f)),

            // Crawling shape on the ground, near gate
            (new Vector3(-1.5f, 0.4f, 10f), new Vector3(1.2f, 0.6f, 0.6f)),

            // Something large, further right
            (new Vector3(11f, 2f, 17f), new Vector3(1.5f, 3.5f, 0.6f)),
        };

        var silMat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                                ?? Shader.Find("Standard"));
        silMat.color = new Color(0.025f, 0.02f, 0.03f, 1f); // near-black

        foreach (var (pos, scale) in shapes)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Silhouette";
            go.transform.SetParent(transform);
            go.transform.localPosition = pos;
            go.transform.localScale    = scale;
            Destroy(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().sharedMaterial = silMat;
        }
    }

    // ── Ember particles (torch streams) ───────────────────────────────────────
    void BuildEmbers()
    {
        SpawnEmberStream("EmbersL", new Vector3(-5.5f, 3.4f, 5f));
        SpawnEmberStream("EmbersR", new Vector3( 5.5f, 3.4f, 5f));
        // Gate embers — purple/void colored
        SpawnGateEmbers("GateEmbers", new Vector3(0f, 1f, 8.5f));
    }

    void SpawnEmberStream(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.localPosition = pos;

        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop          = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0.5f, 1.8f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);
        main.startColor    = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.65f, 0.1f, 0.9f),
            new Color(1f, 0.35f, 0.05f, 0.5f));
        main.maxParticles  = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.06f;

        var em = ps.emission;
        em.rateOverTime = 18f;

        var sh = ps.shape;
        sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 0.12f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
        vel.y = new ParticleSystem.MinMaxCurve(0.8f, 2.0f);

        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[]{ new GradientColorKey(Color.white,0f), new GradientColorKey(Color.white,1f) },
            new[]{ new GradientAlphaKey(0f,0f), new GradientAlphaKey(0.9f,0.1f),
                   new GradientAlphaKey(0.6f,0.7f), new GradientAlphaKey(0f,1f) });
        fade.color = g;

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.material   = AdditiveMat(new Color(1f,0.6f,0.15f));
    }

    void SpawnGateEmbers(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.localPosition = pos;

        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop          = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startColor    = new ParticleSystem.MinMaxGradient(
            new Color(0.5f, 0.2f, 0.9f, 0.8f),
            new Color(0.3f, 0.1f, 0.6f, 0.4f));
        main.maxParticles  = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.03f;

        var em = ps.emission; em.rateOverTime = 10f;
        var sh = ps.shape;
        sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale   = new Vector3(4f, 0.1f, 0.1f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        vel.y = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);

        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[]{ new GradientColorKey(Color.white,0f), new GradientColorKey(Color.white,1f) },
            new[]{ new GradientAlphaKey(0f,0f), new GradientAlphaKey(0.8f,0.15f),
                   new GradientAlphaKey(0.4f,0.8f), new GradientAlphaKey(0f,1f) });
        fade.color = g;

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.material   = AdditiveMat(new Color(0.5f,0.2f,1f));
    }

    // ── Ambient motes (drifting across the arena floor) ───────────────────────
    void BuildMotes()
    {
        var go = new GameObject("Motes");
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(0f, 0.3f, 6f);

        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop          = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize     = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor    = new ParticleSystem.MinMaxGradient(
            new Color(0.6f, 0.4f, 1.0f, 0.5f),
            new Color(0.3f, 0.7f, 0.9f, 0.3f));
        main.maxParticles  = 150;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission; em.rateOverTime = 12f;
        var sh = ps.shape;
        sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale   = new Vector3(20f, 0.1f, 12f);

        var noise = ps.noise;
        noise.enabled    = true;
        noise.strength   = 0.3f;
        noise.frequency  = 0.2f;
        noise.scrollSpeed = 0.05f;

        var fade = ps.colorOverLifetime;
        fade.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[]{ new GradientColorKey(Color.white,0f), new GradientColorKey(Color.white,1f) },
            new[]{ new GradientAlphaKey(0f,0f), new GradientAlphaKey(0.5f,0.2f),
                   new GradientAlphaKey(0.3f,0.85f), new GradientAlphaKey(0f,1f) });
        fade.color = g;

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.material   = AdditiveMat(new Color(0.5f, 0.35f, 1f));
    }

    // ── Material helper ───────────────────────────────────────────────────────
    static Material AdditiveMat(Color tint)
    {
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                  ?? Shader.Find("Particles/Standard Unlit")
                  ?? Shader.Find("Legacy Shaders/Particles/Additive");
        if (shader == null) return new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
        var mat = new Material(shader);
        mat.color = tint;
        if (mat.HasProperty("_Surface")) { mat.SetFloat("_Surface",1f); mat.SetFloat("_Blend",0f); }
        return mat;
    }
}
#endif
