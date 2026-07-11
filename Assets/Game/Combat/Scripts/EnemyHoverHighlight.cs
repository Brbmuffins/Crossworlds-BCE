#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Temporary enemy hover treatment. Uses property blocks and a ring so prefab
/// materials are restored exactly when hover ends.
/// </summary>
[DisallowMultipleComponent]
public class EnemyHoverHighlight : MonoBehaviour
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    static Material _ringMaterial;

    [SerializeField] Color tintColor = new Color(0.50f, 0.95f, 1f, 1f);
    [SerializeField, Range(0f, 1f)] float tintStrength = 0.30f;
    [SerializeField] Color glowColor = new Color(0.18f, 0.80f, 1f, 1f);
    [SerializeField, Min(0f)] float glowStrength = 1.35f;
    [SerializeField] Color ringColor = new Color(0.34f, 0.95f, 1f, 0.92f);
    [SerializeField, Min(12)] int ringSegments = 64;
    [SerializeField, Min(0.01f)] float ringWidth = 0.035f;
    [SerializeField, Min(0f)] float ringHeightOffset = 0.035f;
    [SerializeField, Min(0.1f)] float ringRadiusScale = 1.13f;

    readonly List<Renderer> _renderers = new List<Renderer>();
    RendererState[] _originalStates;
    MaterialPropertyBlock _scratchBlock;
    LineRenderer _ring;
    bool _highlighted;

    struct RendererState
    {
        public Renderer renderer;
        public MaterialPropertyBlock block;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_highlighted == highlighted)
            return;

        _highlighted = highlighted;
        if (_highlighted)
            EnableHighlight();
        else
            DisableHighlight();
    }

    void LateUpdate()
    {
        if (_highlighted)
            UpdateRing();
    }

    void OnDisable()
    {
        SetHighlighted(false);
    }

    void OnDestroy()
    {
        SetHighlighted(false);
    }

    void EnableHighlight()
    {
        CacheRenderers();
        CaptureOriginalBlocks();
        ApplyTint();
        EnsureRing();
        UpdateRing();
        if (_ring != null)
            _ring.enabled = true;
    }

    void DisableHighlight()
    {
        RestoreOriginalBlocks();
        if (_ring != null)
            _ring.enabled = false;
    }

    void CacheRenderers()
    {
        _renderers.Clear();
        var all = GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in all)
        {
            if (renderer == null || renderer is LineRenderer) continue;
            if (!renderer.enabled) continue;
            _renderers.Add(renderer);
        }
    }

    void CaptureOriginalBlocks()
    {
        _originalStates = new RendererState[_renderers.Count];
        for (int i = 0; i < _renderers.Count; i++)
        {
            var block = new MaterialPropertyBlock();
            _renderers[i].GetPropertyBlock(block);
            _originalStates[i] = new RendererState
            {
                renderer = _renderers[i],
                block = block
            };
        }
    }

    void ApplyTint()
    {
        if (_scratchBlock == null)
            _scratchBlock = new MaterialPropertyBlock();

        foreach (var renderer in _renderers)
        {
            if (renderer == null || renderer.sharedMaterial == null) continue;

            Material mat = renderer.sharedMaterial;
            Color baseColor = GetBaseColor(mat);
            Color hoverColor = Color.Lerp(baseColor, tintColor, tintStrength);
            hoverColor.a = baseColor.a;

            _scratchBlock.Clear();
            renderer.GetPropertyBlock(_scratchBlock);

            bool changed = false;
            if (mat.HasProperty(BaseColorId))
            {
                _scratchBlock.SetColor(BaseColorId, hoverColor);
                changed = true;
            }

            if (mat.HasProperty(ColorId))
            {
                _scratchBlock.SetColor(ColorId, hoverColor);
                changed = true;
            }

            if (mat.HasProperty(EmissionColorId))
            {
                Color emission = glowColor * glowStrength;
                emission.a = glowColor.a;
                _scratchBlock.SetColor(EmissionColorId, emission);
                changed = true;
            }

            if (changed)
                renderer.SetPropertyBlock(_scratchBlock);
        }
    }

    void RestoreOriginalBlocks()
    {
        if (_originalStates == null) return;

        foreach (var state in _originalStates)
        {
            if (state.renderer != null)
                state.renderer.SetPropertyBlock(state.block);
        }

        _originalStates = null;
    }

    static Color GetBaseColor(Material mat)
    {
        if (mat.HasProperty(BaseColorId))
            return mat.GetColor(BaseColorId);

        if (mat.HasProperty(ColorId))
            return mat.GetColor(ColorId);

        return Color.white;
    }

    void EnsureRing()
    {
        if (_ring != null)
            return;

        var go = new GameObject("EnemyHoverRing");
        go.transform.SetParent(transform, false);
        go.layer = gameObject.layer;

        _ring = go.AddComponent<LineRenderer>();
        _ring.useWorldSpace = true;
        _ring.loop = true;
        _ring.positionCount = Mathf.Max(12, ringSegments);
        _ring.widthMultiplier = ringWidth;
        _ring.numCornerVertices = 2;
        _ring.numCapVertices = 2;
        _ring.textureMode = LineTextureMode.Stretch;
        _ring.shadowCastingMode = ShadowCastingMode.Off;
        _ring.receiveShadows = false;
        _ring.startColor = ringColor;
        _ring.endColor = ringColor;

        Material mat = RingMaterial;
        if (mat != null)
            _ring.sharedMaterial = mat;
    }

    void UpdateRing()
    {
        if (_ring == null) return;

        if (!TryGetHoverBounds(out Bounds bounds))
        {
            _ring.enabled = false;
            return;
        }

        int segments = Mathf.Max(12, ringSegments);
        if (_ring.positionCount != segments)
            _ring.positionCount = segments;

        float radius = Mathf.Max(0.35f, Mathf.Max(bounds.extents.x, bounds.extents.z) * ringRadiusScale);
        float y = bounds.min.y + ringHeightOffset;
        Vector3 center = bounds.center;

        for (int i = 0; i < segments; i++)
        {
            float angle = (Mathf.PI * 2f * i) / segments;
            var point = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                y,
                center.z + Mathf.Sin(angle) * radius);
            _ring.SetPosition(i, point);
        }

        _ring.startColor = ringColor;
        _ring.endColor = ringColor;
        _ring.widthMultiplier = ringWidth;
        _ring.enabled = true;
    }

    bool TryGetHoverBounds(out Bounds bounds)
    {
        bool hasBounds = false;
        bounds = new Bounds(transform.position, Vector3.one);

        foreach (var renderer in _renderers)
        {
            if (renderer == null || !renderer.enabled) continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (hasBounds)
            return true;

        var colliders = GetComponentsInChildren<Collider>(true);
        foreach (var collider in colliders)
        {
            if (collider == null || !collider.enabled) continue;

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds;
    }

    static Material RingMaterial
    {
        get
        {
            if (_ringMaterial != null)
                return _ringMaterial;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");

            if (shader == null)
                return null;

            _ringMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            return _ringMaterial;
        }
    }
}
#endif
