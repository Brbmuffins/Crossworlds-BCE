#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Renders a visual-only copy of the local player's current model into Character UI.</summary>
public sealed class CharacterModelPreview
{
    const int PreviewLayer = 31;
    static readonly Vector3 StageOrigin = new Vector3(0f, -1000f, 0f);

    readonly RawImage _target;
    RenderTexture _texture;
    GameObject _stage;
    GameObject _model;
    Camera _camera;
    GameObject _sourcePlayer;

    public CharacterModelPreview(RawImage target)
    {
        _target = target;
        CreateStage();
    }

    public void Refresh(GameObject sourcePlayer, bool rebuild = false)
    {
        if (_target != null)
        {
            _target.texture = _texture;
            _target.color = Color.white;
            _target.enabled = true;
        }
        if (sourcePlayer == null) return;
        if (!rebuild && sourcePlayer == _sourcePlayer)
        {
            if (_camera != null) _camera.Render();
            return;
        }
        _sourcePlayer = sourcePlayer;
        if (_model != null) Object.Destroy(_model);

        Animator sourceAnimator = sourcePlayer.GetComponentInChildren<Animator>(true);
        Transform sourceRoot = sourcePlayer.transform;
        var map = new Dictionary<Transform, Transform>();
        _model = CloneHierarchy(sourceRoot, _stage.transform, map).gameObject;
        _model.name = sourcePlayer.name + "_CharacterPreview";
        // Keep the copied rig close to its preview camera. Large gameplay-world offsets
        // cause precision loss in skinned meshes and can produce an apparently empty RT.
        _model.transform.localPosition = Vector3.zero;
        _model.transform.localRotation = Quaternion.identity;

        int rendererCount = CopyRenderers(sourceRoot, map);
        if (rendererCount == 0)
        {
            Debug.LogWarning($"[CHARACTER PREVIEW] No mesh renderers found beneath '{sourcePlayer.name}'.");
            return;
        }
        if (sourceAnimator != null && map.TryGetValue(sourceAnimator.transform, out Transform animatorRoot))
        {
            var animator = animatorRoot.gameObject.AddComponent<Animator>();
            animator.avatar = sourceAnimator.avatar;
            animator.runtimeAnimatorController = sourceAnimator.runtimeAnimatorController;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.speed = 1f;
        }

        SetLayerRecursive(_model.transform, PreviewLayer);
        FrameModel();
        _camera.Render();
        Debug.Log($"[CHARACTER PREVIEW] Rendering '{sourcePlayer.name}' with {rendererCount} mesh renderer(s), " +
                  $"bounds {_model.GetComponentInChildren<Renderer>().bounds.size}.");
    }

    public void Dispose()
    {
        if (_texture != null) { _texture.Release(); Object.Destroy(_texture); }
        if (_stage != null) Object.Destroy(_stage);
    }

    public void RenderFrame()
    {
        if (_camera != null && _model != null) _camera.Render();
    }

    void CreateStage()
    {
        _texture = new RenderTexture(512, 640, 24, RenderTextureFormat.ARGB32)
        {
            name = "CharacterWindowPreview",
            antiAliasing = 2,
            useMipMap = false
        };
        _texture.Create();
        if (_target != null) _target.texture = _texture;

        _stage = new GameObject("[CharacterPreviewStage]");
        _stage.transform.position = StageOrigin;
        Object.DontDestroyOnLoad(_stage);

        var cameraObject = new GameObject("Camera");
        cameraObject.transform.SetParent(_stage.transform, false);
        _camera = cameraObject.AddComponent<Camera>();
        _camera.enabled = false;
        _camera.cullingMask = 1 << PreviewLayer;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0.045f, 0.03f, 0.055f, 1f);
        _camera.targetTexture = _texture;
        _camera.orthographic = true;
        _camera.orthographicSize = 1.2f;
        _camera.nearClipPlane = 0.05f;
        _camera.farClipPlane = 100f;

        AddLight("Key", new Vector3(35f, -35f, 0f), 1.35f, new Color(1f, .83f, .55f));
        AddLight("Fill", new Vector3(20f, 145f, 0f), .8f, new Color(.48f, .36f, .7f));
    }

    void AddLight(string name, Vector3 rotation, float intensity, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_stage.transform, false);
        go.transform.localEulerAngles = rotation;
        var light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = intensity;
        light.color = color;
        light.cullingMask = 1 << PreviewLayer;
    }

    static Transform CloneHierarchy(Transform source, Transform parent, Dictionary<Transform, Transform> map)
    {
        var clone = new GameObject(source.name).transform;
        clone.SetParent(parent, false);
        clone.localPosition = source.localPosition;
        clone.localRotation = source.localRotation;
        clone.localScale = source.localScale;
        map[source] = clone;
        for (int i = 0; i < source.childCount; i++) CloneHierarchy(source.GetChild(i), clone, map);
        return clone;
    }

    static int CopyRenderers(Transform sourceRoot, Dictionary<Transform, Transform> map)
    {
        int copied = 0;
        foreach (var source in sourceRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (!map.TryGetValue(source.transform, out Transform target)) continue;
            var renderer = target.gameObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = source.sharedMesh;
            renderer.sharedMaterials = source.sharedMaterials;
            renderer.enabled = true;
            renderer.forceRenderingOff = false;
            renderer.localBounds = source.localBounds;
            renderer.quality = source.quality;
            renderer.updateWhenOffscreen = true;
            if (source.rootBone != null && map.TryGetValue(source.rootBone, out Transform rootBone)) renderer.rootBone = rootBone;
            var bones = new Transform[source.bones.Length];
            for (int i = 0; i < bones.Length; i++)
                if (source.bones[i] != null && map.TryGetValue(source.bones[i], out Transform bone)) bones[i] = bone;
            renderer.bones = bones;
            copied++;
        }

        foreach (var source in sourceRoot.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (!map.TryGetValue(source.transform, out Transform target)) continue;
            MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
            if (sourceFilter == null) continue;
            target.gameObject.AddComponent<MeshFilter>().sharedMesh = sourceFilter.sharedMesh;
            var renderer = target.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = source.sharedMaterials;
            renderer.enabled = true;
            renderer.forceRenderingOff = false;
            copied++;
        }
        return copied;
    }

    void FrameModel()
    {
        Renderer[] renderers = _model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

        Vector3 localCenter = bounds.center - StageOrigin;
        _model.transform.localPosition -= new Vector3(localCenter.x, bounds.min.y - StageOrigin.y, localCenter.z);
        _model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        renderers = _model.GetComponentsInChildren<Renderer>(true);
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        Vector3 center = bounds.center - StageOrigin;
        float height = Mathf.Max(bounds.size.y, .5f);
        float depth = Mathf.Max(bounds.size.z, .5f);
        float distance = depth * 2f + 3f;
        _camera.orthographicSize = height * .56f;
        _camera.transform.localPosition = center + new Vector3(0f, height * .02f, -distance);
        _camera.nearClipPlane = .01f;
        _camera.farClipPlane = distance * 2f + depth;
        _camera.transform.LookAt(StageOrigin + center + new Vector3(0f, height * .02f, 0f));
    }

    static void SetLayerRecursive(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++) SetLayerRecursive(root.GetChild(i), layer);
    }
}
#endif
