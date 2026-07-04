#if !UNITY_SERVER
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// HeroCosmeticApplier — self-bootstrapping singleton.
///
/// Fetches the local player's saved cosmetic choice (palette + trail),
/// then applies it using MaterialPropertyBlock (no new material instances).
///
/// 10 colour palettes:
///   0 Default    — class-default tones (no override)
///   1 Crimson    — deep red + orange emissive
///   2 Arctic     — ice blue + white emissive
///   3 Shadow     — near-black + violet emissive
///   4 Golden     — amber + warm gold emissive
///   5 Neon Green — cyberpunk lime + bright green emissive
///   6 Sakura     — rose pink + peach emissive
///   7 Void       — deep indigo + purple emissive
///   8 Copper     — bronze + warm orange emissive
///   9 Glacial    — pale blue + stark white emissive
///
/// Trail IDs:
///   0 None  — no trail
///   1 Class — thin trail coloured to current class
///   2 Gold  — wide gold ribbon
///   3 Ghost — transparent white wisp
///
/// Server endpoint (stubbed until live):
///   GET /api/cosmetics/:characterId
///   Response: { success, data: { paletteId, trailId } }
///
/// NOTE: cosmetics are currently LOCAL-PLAYER ONLY.
/// To sync to remote players, add SyncVar paletteId/trailId to PlayerIdentity
/// and drive ApplyPalette / SetTrail from OnStartClient.
/// </summary>
public class HeroCosmeticApplier : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static HeroCosmeticApplier Local { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Local != null) return;
        var go = new GameObject("[HeroCosmeticApplier]");
        DontDestroyOnLoad(go);
        Local = go.AddComponent<HeroCosmeticApplier>();
    }

    // ── Palette definitions ────────────────────────────────────────────────────
    public struct Palette
    {
        public string Name;
        public Color  Primary;    // applied to _BaseColor / _Color
        public Color  Emission;   // applied to _EmissionColor
    }

    static readonly Palette[] Palettes =
    {
        // 0 Default — no override; let existing materials show through
        new Palette { Name = "Default",    Primary = Color.white,                         Emission = Color.black },
        // 1 Crimson
        new Palette { Name = "Crimson",    Primary = new Color(0.75f, 0.08f, 0.08f),      Emission = new Color(0.60f, 0.08f, 0.00f) },
        // 2 Arctic
        new Palette { Name = "Arctic",     Primary = new Color(0.55f, 0.80f, 1.00f),      Emission = new Color(0.20f, 0.55f, 0.90f) },
        // 3 Shadow
        new Palette { Name = "Shadow",     Primary = new Color(0.08f, 0.06f, 0.12f),      Emission = new Color(0.30f, 0.00f, 0.55f) },
        // 4 Golden
        new Palette { Name = "Golden",     Primary = new Color(0.95f, 0.72f, 0.10f),      Emission = new Color(0.75f, 0.48f, 0.00f) },
        // 5 Neon Green
        new Palette { Name = "Neon Green", Primary = new Color(0.05f, 0.65f, 0.10f),      Emission = new Color(0.00f, 0.90f, 0.15f) },
        // 6 Sakura
        new Palette { Name = "Sakura",     Primary = new Color(0.95f, 0.55f, 0.65f),      Emission = new Color(0.80f, 0.30f, 0.45f) },
        // 7 Void
        new Palette { Name = "Void",       Primary = new Color(0.10f, 0.04f, 0.25f),      Emission = new Color(0.35f, 0.10f, 0.80f) },
        // 8 Copper
        new Palette { Name = "Copper",     Primary = new Color(0.72f, 0.42f, 0.18f),      Emission = new Color(0.55f, 0.28f, 0.05f) },
        // 9 Glacial
        new Palette { Name = "Glacial",    Primary = new Color(0.85f, 0.95f, 1.00f),      Emission = new Color(0.70f, 0.88f, 1.00f) },
    };

    // ── Trail configs ─────────────────────────────────────────────────────────
    struct TrailConfig
    {
        public float Time;        // TrailRenderer.time
        public float StartWidth;
        public float EndWidth;
        public Color StartColor;
        public Color EndColor;
    }

    // Trail 0 = no trail (nil); configs for 1-3
    static TrailConfig TrailClass(int classIdx)
    {
        Color c = ClassColor(classIdx);
        return new TrailConfig
        {
            Time = 0.35f, StartWidth = 0.08f, EndWidth = 0f,
            StartColor = new Color(c.r, c.g, c.b, 0.7f),
            EndColor   = new Color(c.r, c.g, c.b, 0f),
        };
    }

    static readonly TrailConfig TrailGold = new TrailConfig
    {
        Time = 0.5f, StartWidth = 0.14f, EndWidth = 0.01f,
        StartColor = new Color(1.00f, 0.82f, 0.10f, 0.85f),
        EndColor   = new Color(0.95f, 0.65f, 0.00f, 0f),
    };

    static readonly TrailConfig TrailGhost = new TrailConfig
    {
        Time = 0.5f, StartWidth = 0.06f, EndWidth = 0f,
        StartColor = new Color(1f, 1f, 1f, 0.50f),
        EndColor   = new Color(1f, 1f, 1f, 0f),
    };

    // ── State ─────────────────────────────────────────────────────────────────
    int _appliedPalette = -1;
    int _appliedTrail   = -1;

    // Player body root and trail attachment
    GameObject      _playerRoot;
    TrailRenderer   _trail;

    // Cached MaterialPropertyBlock (re-used to avoid allocations)
    MaterialPropertyBlock _mpb = new MaterialPropertyBlock();

    // Shader property IDs (cached at first use)
    static int _propBaseColor  = -1;
    static int _propColor      = -1;
    static int _propEmission   = -1;

    // API
    int    _characterId = -1;
    string _jwt         = "";

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        _propBaseColor = Shader.PropertyToID("_BaseColor");
        _propColor     = Shader.PropertyToID("_Color");
        _propEmission  = Shader.PropertyToID("_EmissionColor");
    }

    void Start()
    {
        StartCoroutine(WaitForLocalPlayer());
    }

    IEnumerator WaitForLocalPlayer()
    {
        float waited = 0f;
        while (waited < 15f)
        {
            var id = FindLocalIdentity();
            if (id != null && id.characterId > 0)
            {
                _characterId = AuthManager.CharacterId > 0 ? AuthManager.CharacterId : id.characterId;
                _jwt         = !string.IsNullOrEmpty(AuthManager.Token) ? AuthManager.Token : PlayerPrefs.GetString("jwt_token", "");
                _playerRoot  = id.gameObject;
                StartCoroutine(FetchCosmetics());
                yield break;
            }
            waited += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
        // No player found — apply palette 0 (default, no change)
        ApplyPalette(0);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Apply a colour palette to the local player.</summary>
    public void ApplyPalette(int paletteId)
    {
        if (paletteId < 0 || paletteId >= Palettes.Length) paletteId = 0;
        if (paletteId == _appliedPalette) return;
        _appliedPalette = paletteId;

        if (_playerRoot == null) { _playerRoot = FindLocalPlayer(); }
        if (_playerRoot == null) return;

        if (paletteId == 0)
        {
            // Default — clear any property block overrides
            ClearPaletteOverride(_playerRoot);
            return;
        }

        var pal = Palettes[paletteId];
        ApplyPropertyBlock(_playerRoot, pal.Primary, pal.Emission);
        Debug.Log($"[COSMETIC] Applied palette {paletteId} ({pal.Name})");
    }

    /// <summary>Set the local player's trail effect.</summary>
    public void SetTrail(int trailId)
    {
        if (trailId == _appliedTrail) return;
        _appliedTrail = trailId;

        if (_playerRoot == null) { _playerRoot = FindLocalPlayer(); }
        if (_playerRoot == null) return;

        // Remove existing trail
        if (_trail != null)
        {
            Destroy(_trail);
            _trail = null;
        }

        if (trailId == 0) return; // No trail

        // Attach trail to the player's feet
        Transform feet = _playerRoot.transform.Find("trail_attach") ?? _playerRoot.transform;
        var trailGO = new GameObject("CosmeticTrail");
        trailGO.transform.SetParent(feet, false);
        trailGO.transform.localPosition = Vector3.zero;

        _trail = trailGO.AddComponent<TrailRenderer>();

        TrailConfig cfg = trailId switch
        {
            1 => TrailClass(PlayerProgressManager.Local?.ClassIndex ?? 0),
            2 => TrailGold,
            3 => TrailGhost,
            _ => TrailClass(0),
        };

        _trail.time        = cfg.Time;
        _trail.startWidth  = cfg.StartWidth;
        _trail.endWidth    = cfg.EndWidth;
        _trail.minVertexDistance = 0.05f;
        _trail.autodestruct      = false;

        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(cfg.StartColor, 0f),
                new GradientColorKey(cfg.EndColor,   1f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(cfg.StartColor.a, 0f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        _trail.colorGradient = grad;

        // Use additive transparent material
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetFloat("_Mode", 1f);    // Transparent (URP: already transparent)
        _trail.material = mat;

        Debug.Log($"[COSMETIC] Set trail {trailId}");
    }

    // ── Fetch from server ─────────────────────────────────────────────────────
    IEnumerator FetchCosmetics()
    {
        string url = $"{ServerConfig.AuthBaseUrl}/api/cosmetics/{_characterId}";
        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", $"Bearer {_jwt}");
        req.timeout = 8;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[COSMETIC] Fetch failed ({req.error}) — using defaults");
            ApplyPalette(0);
            yield break;
        }

        try
        {
            var r = JsonUtility.FromJson<CosmeticResponse>(req.downloadHandler.text);
            if (r.success && r.data != null)
            {
                ApplyPalette(r.data.paletteId);
                SetTrail(r.data.trailId);
            }
            else
            {
                ApplyPalette(0);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[COSMETIC] Parse error: {e.Message}");
            ApplyPalette(0);
        }
    }

    // ── MaterialPropertyBlock helpers ─────────────────────────────────────────
    void ApplyPropertyBlock(GameObject root, Color primary, Color emission)
    {
        foreach (var rend in root.GetComponentsInChildren<Renderer>(includeInactive: false))
        {
            // Skip trail renderers (managed separately)
            if (rend is TrailRenderer) continue;
            // Skip particle renderers (don't tint particles with character palette)
            if (rend is ParticleSystemRenderer) continue;

            rend.GetPropertyBlock(_mpb);
            _mpb.SetColor(_propBaseColor, primary);
            _mpb.SetColor(_propColor,     primary);
            _mpb.SetColor(_propEmission,  emission);
            rend.SetPropertyBlock(_mpb);
        }
    }

    void ClearPaletteOverride(GameObject root)
    {
        var empty = new MaterialPropertyBlock();
        foreach (var rend in root.GetComponentsInChildren<Renderer>(includeInactive: false))
        {
            if (rend is TrailRenderer || rend is ParticleSystemRenderer) continue;
            rend.SetPropertyBlock(empty);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static PlayerIdentity FindLocalIdentity()
    {
        foreach (var id in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (id.isLocalPlayer) return id;
        return null;
    }

    static GameObject FindLocalPlayer()
    {
        var id = FindLocalIdentity();
        return id != null ? id.gameObject : null;
    }

    static Color ClassColor(int idx) => idx switch
    {
        0 => new Color(0.40f, 0.80f, 0.40f),
        1 => new Color(0.60f, 0.60f, 0.75f),
        2 => new Color(0.60f, 0.10f, 0.80f),
        3 => new Color(0.95f, 0.80f, 0.20f),
        4 => new Color(0.30f, 0.55f, 1.00f),
        _ => Color.white,
    };

    // ── JSON shapes ───────────────────────────────────────────────────────────
    [Serializable]
    class CosmeticResponse
    {
        public bool       success;
        public CosmeticData data;
    }

    [Serializable]
    class CosmeticData
    {
        public int paletteId;
        public int trailId;
    }
}
#endif
