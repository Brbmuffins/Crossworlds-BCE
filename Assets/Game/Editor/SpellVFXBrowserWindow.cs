#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;

namespace Crossworlds.EditorTools
{
    /// <summary>
    /// Fast, searchable live preview for the exact prefabs shipped with Spells Pack 2.
    /// Keeps the asset name and path visible so a chosen effect can be reused in Spell Forge.
    /// </summary>
    public sealed class SpellVFXBrowserWindow : EditorWindow
    {
        const string PrefabRoot = "Assets/Game/FX/Spells Pack 2/Prefabs";
        const float ListWidth = 310f;

        public static GameObject SpellForgeSelection { get; private set; }
        public static event Action SpellForgeSelectionChanged;

        sealed class Entry
        {
            public GameObject prefab;
            public string path;
            public string category;
            public string searchText;
        }

        readonly List<Entry> entries = new();
        readonly List<Entry> filteredEntries = new();
        readonly List<string> categories = new();

        PreviewRenderUtility preview;
        GameObject previewInstance;
        GameObject previewSource;
        GameObject ground;
        ParticleSystem[] particleSystems = Array.Empty<ParticleSystem>();
        VisualEffect[] visualEffects = Array.Empty<VisualEffect>();
        Animator[] animators = Array.Empty<Animator>();
        PlayableDirector[] directors = Array.Empty<PlayableDirector>();

        Vector2 listScroll;
        string search = "";
        int categoryIndex;
        int selectedIndex = -1;
        bool playing = true;
        bool autoReplay = true;
        float replaySeconds = 5f;
        float playbackSpeed = 1f;
        float previewTime;
        double lastUpdate;
        float cameraYaw = 25f;
        float cameraPitch = 18f;
        float cameraZoom = 1f;
        bool orbiting;
        Bounds effectBounds = new(Vector3.up, Vector3.one * 2f);

        [MenuItem("BCE/Spell Forge/VFX Browser", priority = 39)]
        public static void Open()
        {
            var window = GetWindow<SpellVFXBrowserWindow>(
                false, "Spell VFX Browser", true);
            window.minSize = new Vector2(820f, 540f);
            window.Show();
            window.Focus();
        }

        void OnEnable()
        {
            RefreshAssets();
            EditorApplication.update += Tick;
        }

        void OnDisable()
        {
            EditorApplication.update -= Tick;
            CleanupPreview();
        }

        void OnGUI()
        {
            DrawToolbar();

            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    $"No prefabs were found below:\n{PrefabRoot}",
                    MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawAssetList();
                DrawPreviewPanel();
            }
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Spells Pack 2", GUILayout.Width(92f));

                string nextSearch = GUILayout.TextField(
                    search, GUI.skin.FindStyle("ToolbarSearchTextField"),
                    GUILayout.MinWidth(150f));
                if (nextSearch != search)
                {
                    search = nextSearch;
                    RebuildFilter();
                }

                int nextCategory = EditorGUILayout.Popup(
                    categoryIndex, categories.ToArray(),
                    EditorStyles.toolbarPopup, GUILayout.Width(175f));
                if (nextCategory != categoryIndex)
                {
                    categoryIndex = nextCategory;
                    RebuildFilter();
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label(
                    $"{filteredEntries.Count} / {entries.Count} prefabs",
                    EditorStyles.miniLabel);
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton,
                    GUILayout.Width(60f)))
                    RefreshAssets();
            }
        }

        void DrawAssetList()
        {
            using (new EditorGUILayout.VerticalScope(
                GUILayout.Width(ListWidth), GUILayout.ExpandHeight(true)))
            {
                EditorGUILayout.LabelField(
                    "Exact prefab assets", EditorStyles.boldLabel);
                listScroll = EditorGUILayout.BeginScrollView(listScroll);

                for (int i = 0; i < filteredEntries.Count; i++)
                {
                    Entry entry = filteredEntries[i];
                    bool selected = i == selectedIndex;
                    GUIStyle row = selected
                        ? new GUIStyle(EditorStyles.helpBox)
                        {
                            normal = { background = Texture2D.grayTexture }
                        }
                        : EditorStyles.helpBox;

                    Rect rect = EditorGUILayout.BeginVertical(row);
                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                        SelectEntry(i);

                    EditorGUILayout.LabelField(
                        entry.prefab.name,
                        selected ? EditorStyles.boldLabel : EditorStyles.label);
                    EditorGUILayout.LabelField(
                        entry.category, EditorStyles.miniLabel);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        void DrawPreviewPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                Entry selected = SelectedEntry();
                if (selected == null)
                {
                    EditorGUILayout.HelpBox(
                        "Choose a prefab to preview it.",
                        MessageType.Info);
                    return;
                }

                EditorGUILayout.LabelField(
                    selected.prefab.name, LargeTitleStyle());
                EditorGUILayout.LabelField(
                    selected.category, EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    "Current Spell Forge VFX selection",
                    EditorStyles.centeredGreyMiniLabel);

                Rect previewRect = GUILayoutUtility.GetRect(
                    200f, Mathf.Max(260f, position.height - 245f),
                    GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(previewRect, new Color(0.055f, 0.065f, 0.075f, 1f));
                HandleCameraInput(previewRect);
                DrawPreview(previewRect);

                DrawPlaybackControls();
                DrawAssetControls(selected);

                EditorGUILayout.LabelField("Asset path", EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(
                    selected.path, EditorStyles.textField,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        void DrawPlaybackControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Replay", GUILayout.Width(70f)))
                    RestartPreview();

                if (GUILayout.Button(playing ? "Pause" : "Play",
                    GUILayout.Width(70f)))
                {
                    playing = !playing;
                    SetVisualEffectPause(!playing);
                    lastUpdate = EditorApplication.timeSinceStartup;
                }

                if (GUILayout.Button("Frame", GUILayout.Width(60f)))
                    FrameEffect();

                GUILayout.Label("Speed", GUILayout.Width(38f));
                float nextSpeed = GUILayout.HorizontalSlider(
                    playbackSpeed, 0.1f, 2f, GUILayout.MinWidth(90f));
                if (!Mathf.Approximately(nextSpeed, playbackSpeed))
                {
                    playbackSpeed = nextSpeed;
                    foreach (VisualEffect effect in visualEffects)
                        if (effect != null) effect.playRate = playbackSpeed;
                }
                GUILayout.Label($"{playbackSpeed:0.00}x", GUILayout.Width(42f));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                autoReplay = EditorGUILayout.ToggleLeft(
                    "Auto replay", autoReplay, GUILayout.Width(88f));
                using (new EditorGUI.DisabledScope(!autoReplay))
                {
                    GUILayout.Label("after", GUILayout.Width(30f));
                    replaySeconds = EditorGUILayout.Slider(
                        replaySeconds, 1f, 15f);
                    GUILayout.Label("seconds", GUILayout.Width(52f));
                }
            }
        }

        void DrawAssetControls(Entry entry)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy Prefab Name"))
                {
                    EditorGUIUtility.systemCopyBuffer = entry.prefab.name;
                    ShowNotification(new GUIContent(
                        $"Copied: {entry.prefab.name}"));
                }

                if (GUILayout.Button("Copy Asset Path"))
                {
                    EditorGUIUtility.systemCopyBuffer = entry.path;
                    ShowNotification(new GUIContent("Asset path copied"));
                }

                if (GUILayout.Button("Show In Project"))
                {
                    Selection.activeObject = entry.prefab;
                    EditorGUIUtility.PingObject(entry.prefab);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Prefab"))
                    AssetDatabase.OpenAsset(entry.prefab);

                if (GUILayout.Button("Place In Current Scene"))
                    PlaceInCurrentScene(entry.prefab);
            }
        }

        void Tick()
        {
            if (previewInstance == null) return;

            double now = EditorApplication.timeSinceStartup;
            if (lastUpdate <= 0d) lastUpdate = now;
            float delta = Mathf.Min((float)(now - lastUpdate), 0.05f);
            lastUpdate = now;

            if (playing)
            {
                AdvancePreview(delta * playbackSpeed);
                previewTime += delta * playbackSpeed;
                if (autoReplay && previewTime >= replaySeconds)
                    RestartPreview();
            }

            Repaint();
        }

        void AdvancePreview(float delta)
        {
            foreach (ParticleSystem particles in particleSystems)
            {
                if (particles == null) continue;
                particles.Simulate(delta, false, false, false);
            }

            foreach (Animator animator in animators)
            {
                if (animator == null || !animator.enabled) continue;
                animator.speed = 1f;
                animator.Update(delta);
            }

            foreach (PlayableDirector director in directors)
            {
                if (director == null || director.playableAsset == null) continue;
                double duration = director.playableAsset.duration;
                director.time = duration > 0d
                    ? (director.time + delta) % duration
                    : director.time + delta;
                director.Evaluate();
            }

            foreach (VisualEffect effect in visualEffects)
            {
                if (effect == null) continue;
                effect.playRate = playbackSpeed;
                effect.AdvanceOneFrame();
            }
        }

        void DrawPreview(Rect rect)
        {
            if (preview == null || previewInstance == null) return;

            Vector3 center = effectBounds.center;
            float radius = Mathf.Clamp(effectBounds.extents.magnitude, 0.75f, 25f);
            Quaternion orbit = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            preview.camera.transform.position =
                center + orbit * new Vector3(0f, radius * 0.18f,
                    radius * 2.7f * cameraZoom);
            preview.camera.transform.LookAt(center);
            preview.camera.aspect =
                Mathf.Max(0.1f, rect.width / Mathf.Max(1f, rect.height));
            preview.camera.nearClipPlane = Mathf.Max(0.01f, radius * 0.01f);
            preview.camera.farClipPlane = Mathf.Max(50f, radius * 10f);

            if (ground != null)
            {
                ground.transform.position =
                    new Vector3(center.x, effectBounds.min.y - 0.02f, center.z);
                ground.transform.localScale =
                    Vector3.one * Mathf.Clamp(radius * 0.45f, 0.5f, 12f);
            }

            preview.BeginPreview(rect, GUIStyle.none);
            preview.Render(true);
            Texture result = preview.EndPreview();
            if (result != null)
                GUI.DrawTexture(rect, result, ScaleMode.StretchToFill, false);

            GUI.Label(
                new Rect(rect.x + 8f, rect.y + 7f, rect.width - 16f, 20f),
                $"LIVE PREVIEW  •  {previewTime:0.0}s",
                EditorStyles.whiteMiniLabel);
        }

        void HandleCameraInput(Rect rect)
        {
            Event current = Event.current;
            if (!rect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseUp) orbiting = false;
                return;
            }

            if (current.type == EventType.MouseDown &&
                (current.button == 0 || current.button == 1))
            {
                orbiting = true;
                current.Use();
            }
            else if (current.type == EventType.MouseDrag && orbiting)
            {
                cameraYaw += current.delta.x * 0.55f;
                cameraPitch = Mathf.Clamp(
                    cameraPitch - current.delta.y * 0.4f, -75f, 75f);
                current.Use();
            }
            else if (current.type == EventType.MouseUp)
            {
                orbiting = false;
                current.Use();
            }
            else if (current.type == EventType.ScrollWheel)
            {
                cameraZoom = Mathf.Clamp(
                    cameraZoom + current.delta.y * 0.05f, 0.35f, 3f);
                current.Use();
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Orbit);
        }

        void RefreshAssets()
        {
            string selectedPath = SelectedEntry()?.path;
            entries.Clear();
            categories.Clear();
            categories.Add("All categories");

            foreach (string guid in AssetDatabase.FindAssets(
                "t:Prefab", new[] { PrefabRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                string relative = path.StartsWith(PrefabRoot + "/", StringComparison.Ordinal)
                    ? path.Substring(PrefabRoot.Length + 1)
                    : path;
                string category = Path.GetDirectoryName(relative)
                    ?.Replace('\\', '/') ?? "Uncategorized";

                entries.Add(new Entry
                {
                    prefab = prefab,
                    path = path,
                    category = category,
                    searchText = (prefab.name + " " + category + " " + path)
                        .ToLowerInvariant()
                });

                if (!categories.Contains(category))
                    categories.Add(category);
            }

            entries.Sort((left, right) =>
                string.Compare(left.path, right.path,
                    StringComparison.OrdinalIgnoreCase));
            categories.Sort(1, categories.Count - 1,
                StringComparer.OrdinalIgnoreCase);
            categoryIndex = Mathf.Clamp(
                categoryIndex, 0, Mathf.Max(0, categories.Count - 1));
            RebuildFilter(selectedPath);
        }

        void RebuildFilter(string preferredPath = null)
        {
            string category = categoryIndex > 0 &&
                              categoryIndex < categories.Count
                ? categories[categoryIndex]
                : null;
            string[] terms = search
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(term => term.ToLowerInvariant())
                .ToArray();

            filteredEntries.Clear();
            foreach (Entry entry in entries)
            {
                if (category != null && entry.category != category)
                    continue;
                if (terms.Any(term => !entry.searchText.Contains(term)))
                    continue;
                filteredEntries.Add(entry);
            }

            int nextIndex = !string.IsNullOrEmpty(preferredPath)
                ? filteredEntries.FindIndex(entry => entry.path == preferredPath)
                : -1;
            if (nextIndex < 0 && filteredEntries.Count > 0)
                nextIndex = 0;

            selectedIndex = nextIndex;
            if (selectedIndex >= 0)
            {
                SetSpellForgeSelection(filteredEntries[selectedIndex].prefab);
                EnsurePreview(filteredEntries[selectedIndex].prefab);
            }
            else
            {
                SetSpellForgeSelection(null);
                CleanupPreview();
            }
            Repaint();
        }

        void SelectEntry(int index)
        {
            if (index < 0 || index >= filteredEntries.Count) return;
            selectedIndex = index;
            SetSpellForgeSelection(filteredEntries[index].prefab);
            EnsurePreview(filteredEntries[index].prefab);
        }

        static void SetSpellForgeSelection(GameObject prefab)
        {
            if (SpellForgeSelection == prefab) return;
            SpellForgeSelection = prefab;
            SpellForgeSelectionChanged?.Invoke();
        }

        Entry SelectedEntry()
        {
            return selectedIndex >= 0 && selectedIndex < filteredEntries.Count
                ? filteredEntries[selectedIndex]
                : null;
        }

        void EnsurePreview(GameObject source)
        {
            if (source == null) return;
            if (preview != null && previewInstance != null &&
                previewSource == source)
                return;

            CleanupPreview();
            preview = new PreviewRenderUtility();
            preview.camera.fieldOfView = 32f;
            preview.camera.clearFlags = CameraClearFlags.Color;
            preview.camera.backgroundColor =
                new Color(0.055f, 0.065f, 0.075f, 1f);
            preview.lights[0].intensity = 1.15f;
            preview.lights[0].transform.rotation =
                Quaternion.Euler(35f, 35f, 0f);
            preview.lights[1].intensity = 0.8f;

            previewInstance = Instantiate(source);
            previewInstance.name = source.name + "_SpellVFXPreview";
            previewInstance.hideFlags = HideFlags.HideAndDontSave;
            previewInstance.SetActive(true);
            preview.AddSingleGO(previewInstance);

            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "SpellVFXPreview_Ground";
            ground.hideFlags = HideFlags.HideAndDontSave;
            Collider collider = ground.GetComponent<Collider>();
            if (collider != null) DestroyImmediate(collider);
            Renderer renderer = ground.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                            Shader.Find("Unlit/Color") ??
                            Shader.Find("Standard");
            if (renderer != null && shader != null)
            {
                var material = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    color = new Color(0.11f, 0.13f, 0.15f, 1f)
                };
                renderer.sharedMaterial = material;
            }
            preview.AddSingleGO(ground);

            particleSystems =
                previewInstance.GetComponentsInChildren<ParticleSystem>(true);
            visualEffects =
                previewInstance.GetComponentsInChildren<VisualEffect>(true);
            animators = previewInstance.GetComponentsInChildren<Animator>(true);
            directors =
                previewInstance.GetComponentsInChildren<PlayableDirector>(true);
            previewSource = source;
            RestartPreview();
            FrameEffect();
        }

        void RestartPreview()
        {
            if (previewInstance == null) return;
            previewTime = 0f;
            lastUpdate = EditorApplication.timeSinceStartup;

            foreach (PlayableDirector director in directors)
            {
                if (director == null) continue;
                director.time = 0d;
                director.Evaluate();
            }

            foreach (Animator animator in animators)
            {
                if (animator == null) continue;
                animator.Rebind();
                animator.Update(0f);
            }

            foreach (ParticleSystem particles in particleSystems)
            {
                if (particles == null) continue;
                particles.Stop(false,
                    ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.Clear(false);
                particles.Simulate(0f, false, true, false);
                particles.Play(false);
            }

            foreach (VisualEffect effect in visualEffects)
            {
                if (effect == null) continue;
                effect.playRate = playbackSpeed;
                effect.Reinit();
                effect.pause = !playing;
                if (playing) effect.Play();
            }

            AdvancePreview(0.02f);
            effectBounds = CalculateEffectBounds();
            Repaint();
        }

        void FrameEffect()
        {
            effectBounds = CalculateEffectBounds();
            cameraYaw = 25f;
            cameraPitch = 18f;
            cameraZoom = 1f;
            Repaint();
        }

        Bounds CalculateEffectBounds()
        {
            if (previewInstance == null)
                return new Bounds(Vector3.up, Vector3.one * 2f);

            Renderer[] renderers =
                previewInstance.GetComponentsInChildren<Renderer>(true);
            bool found = false;
            Bounds bounds = new(Vector3.up, Vector3.one * 2f);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled) continue;
                Bounds next = renderer.bounds;
                if (next.size.sqrMagnitude < 0.0001f) continue;
                if (!found)
                {
                    bounds = next;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(next);
                }
            }

            if (!found)
                bounds = new Bounds(previewInstance.transform.position + Vector3.up,
                    Vector3.one * 2f);

            Vector3 extents = bounds.extents;
            extents.x = Mathf.Clamp(extents.x, 0.5f, 25f);
            extents.y = Mathf.Clamp(extents.y, 0.5f, 25f);
            extents.z = Mathf.Clamp(extents.z, 0.5f, 25f);
            bounds.extents = extents;
            return bounds;
        }

        void SetVisualEffectPause(bool paused)
        {
            foreach (VisualEffect effect in visualEffects)
                if (effect != null) effect.pause = paused;
        }

        void CleanupPreview()
        {
            if (preview != null)
                preview.Cleanup();
            preview = null;
            previewInstance = null;
            previewSource = null;
            ground = null;
            particleSystems = Array.Empty<ParticleSystem>();
            visualEffects = Array.Empty<VisualEffect>();
            animators = Array.Empty<Animator>();
            directors = Array.Empty<PlayableDirector>();
            previewTime = 0f;
            lastUpdate = 0d;
        }

        static void PlaceInCurrentScene(GameObject prefab)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog(
                    "Spell VFX Browser",
                    "Open a scene before placing this prefab.", "OK");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, scene)
                as GameObject;
            if (instance == null) return;

            SceneView view = SceneView.lastActiveSceneView;
            instance.transform.position =
                view != null ? view.pivot : Vector3.zero;
            Undo.RegisterCreatedObjectUndo(
                instance, "Place Spell VFX Prefab");
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
        }

        static GUIStyle LargeTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                fixedHeight = 23f
            };
        }
    }
}
#endif
