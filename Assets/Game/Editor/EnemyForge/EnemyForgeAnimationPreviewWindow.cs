#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Crossworlds.EditorTools.EnemyForge
{
    internal sealed class EnemyForgeAnimationPreviewWindow : EditorWindow
    {
        struct PreviewState
        {
            public string name;
            public AnimationClip clip;
            public float speed;
            public float impact;
            public bool hasImpact;
            public int attackIndex;
            public Vector3 vfxOffset;
        }

        EnemyForgeDefinition definition;
        PreviewRenderUtility preview;
        GameObject instance;
        GameObject source;
        GameObject vfxMarker;
        GameObject groundReference;
        GameObject elementalHandEffect;
        GameObject elementalSpellEffect;
        GameObject elementalHitEffect;
        GameObject elementalHandSource;
        GameObject elementalSpellSource;
        GameObject elementalHitSource;
        Vector3 elementalHandBaseScale;
        Vector3 elementalSpellBaseScale;
        Vector3 elementalHitBaseScale;
        ElementalLightningVFXProfile elementalProfile;
        int stateIndex;
        bool playing = true;
        float playTime;
        float previousNormalized;
        double lastUpdate;
        double flashUntil;
        Vector2 controlsScroll;
        float cameraYaw;
        float cameraPitch = 6f;
        float cameraZoom = 1f;
        bool orbiting;

        public static void Open(EnemyForgeDefinition activeDefinition)
        {
            var window = GetWindow<EnemyForgeAnimationPreviewWindow>(
                false, "Enemy Animation Preview", true);
            window.definition = activeDefinition;
            window.minSize = new Vector2(440f, 480f);
            window.Show();
            window.Focus();
            window.Repaint();
        }

        void OnDisable() => Cleanup();
        void Update() => Repaint();

        void OnGUI()
        {
            definition = (EnemyForgeDefinition)EditorGUILayout.ObjectField(
                "Enemy Definition", definition, typeof(EnemyForgeDefinition), false);
            if (definition == null || definition.source == null)
            {
                EditorGUILayout.HelpBox(
                    "Open Animation State Mapping in Enemy Forge or select a definition.",
                    MessageType.Info);
                Cleanup();
                return;
            }

            List<PreviewState> states = BuildStates(definition);
            if (states.Count == 0)
            {
                EditorGUILayout.HelpBox("Assign an animation in Enemy Forge to preview it.",
                    MessageType.Info);
                Cleanup();
                return;
            }

            stateIndex = Mathf.Clamp(stateIndex, 0, states.Count - 1);
            EnsurePreview(definition.source, definition.elementalLightningVfxProfile);
            PreviewState state = states[stateIndex];
            Rect previewRect = GUILayoutUtility.GetRect(
                100f, Mathf.Max(260f, position.height - 170f), GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, new Color(0.1f, 0.1f, 0.1f, 1f));
            HandlePreviewCameraInput(previewRect);
            DrawPreview(previewRect, state);

            controlsScroll = EditorGUILayout.BeginScrollView(controlsScroll);
            DrawViewControls();
            DrawUpdateConfigurationButton();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(playing ? "Pause" : "Play", GUILayout.Width(62f)))
                {
                    playing = !playing;
                    lastUpdate = EditorApplication.timeSinceStartup;
                }
                int next = EditorGUILayout.Popup(
                    stateIndex, states.ConvertAll(item => item.name).ToArray());
                if (next != stateIndex)
                {
                    stateIndex = next;
                    playTime = 0f;
                    previousNormalized = 0f;
                    lastUpdate = EditorApplication.timeSinceStartup;
                }
            }

            state = states[stateIndex];
            float speed = Mathf.Max(0.25f, state.speed);
            float duration = Mathf.Max(0.01f, state.clip.length / speed);
            float normalized = Mathf.Clamp01(playTime / duration);
            DrawTimeline(state, normalized, duration);
            EditorGUILayout.LabelField(
                $"Animation Speed: {speed:0.00}x    Played Duration: {duration:0.00}s",
                EditorStyles.miniLabel);
            if (state.attackIndex >= 0)
                DrawVfxPositionControls(state.attackIndex);
            EditorGUILayout.EndScrollView();
        }

        void DrawPreview(Rect rect, PreviewState state)
        {
            if (preview == null || instance == null) return;
            float speed = Mathf.Max(0.25f, state.speed);
            float duration = Mathf.Max(0.01f, state.clip.length / speed);
            double now = EditorApplication.timeSinceStartup;
            if (lastUpdate <= 0d) lastUpdate = now;
            if (playing && Event.current.type == EventType.Repaint)
            {
                playTime += (float)(now - lastUpdate);
                if (playTime >= duration)
                {
                    playTime %= duration;
                    previousNormalized = 0f;
                }
            }
            lastUpdate = now;

            float normalized = Mathf.Clamp01(playTime / duration);
            if (state.hasImpact && previousNormalized < state.impact &&
                normalized >= state.impact)
                flashUntil = now + 0.16d;
            previousNormalized = normalized;
            state.clip.SampleAnimation(instance,
                Mathf.Clamp(playTime * speed, 0f, state.clip.length));

            Bounds bounds = BoundsFor(instance);
            UpdateVfxMarker(state, bounds, normalized);
            UpdateElementalLightningEffects(state, bounds, duration);
            Vector3 center = bounds.center;
            float radius = Mathf.Max(bounds.extents.magnitude, 0.5f);
            Quaternion orbit = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            preview.camera.transform.position =
                center + orbit * new Vector3(0f, radius * 0.15f,
                    radius * 2.6f * cameraZoom);
            preview.camera.transform.LookAt(center);
            preview.camera.aspect = Mathf.Max(0.1f, rect.width / Mathf.Max(1f, rect.height));
            preview.camera.nearClipPlane = Mathf.Max(0.01f, radius * 0.01f);
            preview.camera.farClipPlane = radius * 8f;
            preview.BeginPreview(rect, GUIStyle.none);
            preview.Render(true);
            GUI.DrawTexture(rect, preview.EndPreview(), ScaleMode.StretchToFill, false);
            if (state.attackIndex >= 0)
                DrawVfxStartOverlay(rect);

            if (now < flashUntil)
            {
                EditorGUI.DrawRect(rect, new Color(1f, 0.68f, 0.05f, 0.18f));
                GUI.Label(new Rect(rect.x, rect.y + 8f, rect.width, 24f),
                    "HIT / VFX", HitStyle());
            }
        }

        void HandlePreviewCameraInput(Rect rect)
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
                cameraPitch = Mathf.Clamp(cameraPitch - current.delta.y * 0.4f, -70f, 70f);
                current.Use();
                Repaint();
            }
            else if (current.type == EventType.MouseUp)
            {
                orbiting = false;
                current.Use();
            }
            else if (current.type == EventType.ScrollWheel)
            {
                cameraZoom = Mathf.Clamp(cameraZoom + current.delta.y * 0.05f, 0.55f, 2.2f);
                current.Use();
                Repaint();
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Orbit);
        }

        void DrawViewControls()
        {
            EditorGUILayout.LabelField(
                "Preview View — drag the model to orbit; mouse wheel zooms",
                EditorStyles.miniLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Front")) SetView(0f);
                if (GUILayout.Button("Left")) SetView(90f);
                if (GUILayout.Button("Back")) SetView(180f);
                if (GUILayout.Button("Right")) SetView(270f);
            }
        }

        void SetView(float yaw)
        {
            cameraYaw = yaw;
            cameraPitch = 6f;
            Repaint();
        }

        void DrawUpdateConfigurationButton()
        {
            using (new EditorGUI.DisabledScope(definition == null))
            {
                if (!GUILayout.Button("Update Configuration", GUILayout.Height(26f))) return;
                EditorUtility.SetDirty(definition);
                AssetDatabase.SaveAssets();
                ShowNotification(new GUIContent("Enemy Forge configuration updated"));
            }
            EditorGUILayout.Space(3f);
        }

        void DrawTimeline(PreviewState state, float normalized, float duration)
        {
            GUILayout.Space(14f);
            Rect timeline = GUILayoutUtility.GetRect(0f, 24f, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(timeline, normalized,
                $"{state.name}  {playTime:0.00}s / {duration:0.00}s");
            if (!state.hasImpact) return;
            float x = timeline.x + timeline.width * Mathf.Clamp01(state.impact);
            EditorGUI.DrawRect(new Rect(x - 1f, timeline.y, 2f, timeline.height),
                new Color(1f, 0.67f, 0.05f, 1f));
            GUI.Label(new Rect(
                    Mathf.Clamp(x - 28f, timeline.x, timeline.xMax - 56f),
                    timeline.y - 17f, 56f, 18f),
                "Hit/VFX", EditorStyles.centeredGreyMiniLabel);
        }

        void EnsurePreview(GameObject nextSource, ElementalLightningVFXProfile nextProfile)
        {
            GameObject nextHand = nextProfile != null ? nextProfile.handEffect : null;
            GameObject nextSpell = nextProfile != null ? nextProfile.spellEffect : null;
            GameObject nextHit = nextProfile != null ? nextProfile.hitEffect : null;
            if (preview != null && instance != null && source == nextSource &&
                elementalProfile == nextProfile &&
                elementalHandSource == nextHand &&
                elementalSpellSource == nextSpell &&
                elementalHitSource == nextHit)
                return;
            Cleanup();
            preview = new PreviewRenderUtility();
            preview.camera.fieldOfView = 30f;
            preview.camera.clearFlags = CameraClearFlags.Color;
            preview.camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            preview.lights[0].intensity = 1.2f;
            preview.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
            preview.lights[1].intensity = 1.2f;
            instance = Instantiate(nextSource);
            instance.name = nextSource.name + "_EnemyForgeAnimationPreview";
            foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour != null) behaviour.enabled = false;
            preview.AddSingleGO(instance);
            vfxMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vfxMarker.name = "EnemyForge_VFX_Origin";
            Collider markerCollider = vfxMarker.GetComponent<Collider>();
            if (markerCollider != null) DestroyImmediate(markerCollider);
            Renderer markerRenderer = vfxMarker.GetComponent<Renderer>();
            Shader markerShader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (markerRenderer != null && markerShader != null)
            {
                var markerMaterial = new Material(markerShader);
                markerMaterial.color = new Color(1f, 0.62f, 0.05f, 1f);
                markerRenderer.sharedMaterial = markerMaterial;
            }
            preview.AddSingleGO(vfxMarker);
            groundReference = GameObject.CreatePrimitive(PrimitiveType.Cube);
            groundReference.name = "EnemyForge_GroundReference";
            Collider groundCollider = groundReference.GetComponent<Collider>();
            if (groundCollider != null) DestroyImmediate(groundCollider);
            Renderer groundRenderer = groundReference.GetComponent<Renderer>();
            Shader groundShader = Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (groundRenderer != null && groundShader != null)
            {
                var groundMaterial = new Material(groundShader);
                groundMaterial.color = new Color(0.18f, 0.2f, 0.17f, 1f);
                groundRenderer.sharedMaterial = groundMaterial;
            }
            preview.AddSingleGO(groundReference);
            elementalProfile = nextProfile;
            elementalHandSource = nextHand;
            elementalSpellSource = nextSpell;
            elementalHitSource = nextHit;
            elementalHandEffect = CreatePreviewEffect(nextHand, out elementalHandBaseScale);
            elementalSpellEffect = CreatePreviewEffect(nextSpell, out elementalSpellBaseScale);
            elementalHitEffect = CreatePreviewEffect(nextHit, out elementalHitBaseScale);
            source = nextSource;
            playTime = 0f;
            previousNormalized = 0f;
            lastUpdate = EditorApplication.timeSinceStartup;
        }

        void Cleanup()
        {
            if (preview != null) preview.Cleanup();
            preview = null;
            instance = null;
            vfxMarker = null;
            groundReference = null;
            elementalHandEffect = null;
            elementalSpellEffect = null;
            elementalHitEffect = null;
            elementalHandSource = null;
            elementalSpellSource = null;
            elementalHitSource = null;
            elementalProfile = null;
            source = null;
            lastUpdate = 0d;
        }

        GameObject CreatePreviewEffect(GameObject prefab, out Vector3 baseScale)
        {
            baseScale = Vector3.one;
            if (prefab == null) return null;
            GameObject effect = Instantiate(prefab);
            effect.name = prefab.name + "_EnemyForgePreview";
            baseScale = effect.transform.localScale;
            foreach (MonoBehaviour behaviour in effect.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour != null) behaviour.enabled = false;
            preview.AddSingleGO(effect);
            effect.SetActive(false);
            return effect;
        }

        static List<PreviewState> BuildStates(EnemyForgeDefinition d)
        {
            var states = new List<PreviewState>();
            Add(states, "Idle", d.idleAnimation, d.idleAnimationSpeed);
            Add(states, "Chase", d.chaseAnimation, d.chaseAnimationSpeed);
            Add(states, "Attack 1", d.attackAnimation, d.attackAnimationSpeed,
                d.attackImpactPoint, 0, d.attackVfxOffset);
            Add(states, "Attack 2", d.attackAnimation2, d.attackAnimationSpeed2,
                d.attackImpactPoint2, 1, d.attackVfxOffset2);
            Add(states, "Attack 3", d.attackAnimation3, d.attackAnimationSpeed3,
                d.attackImpactPoint3, 2, d.attackVfxOffset3);
            Add(states, "Attack 4", d.attackAnimation4, d.attackAnimationSpeed4,
                d.attackImpactPoint4, 3, d.attackVfxOffset4);
            Add(states, "Get Hit", d.getHitAnimation, d.getHitAnimationSpeed);
            Add(states, "Death", d.deathAnimation, d.deathAnimationSpeed);
            return states;
        }

        static void Add(List<PreviewState> states, string name, AnimationClip clip,
            float speed, float impact = -1f, int attackIndex = -1,
            Vector3 vfxOffset = default)
        {
            if (clip == null) return;
            states.Add(new PreviewState
            {
                name = name,
                clip = clip,
                speed = speed,
                impact = Mathf.Clamp01(impact),
                hasImpact = impact >= 0f,
                attackIndex = attackIndex,
                vfxOffset = vfxOffset
            });
        }

        void DrawVfxPositionControls(int attackIndex)
        {
            Vector3 current = GetVfxOffset(attackIndex);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "VFX Origin (Right Hand / Casting Position)", EditorStyles.boldLabel);
            Vector3 next = EditorGUILayout.Vector3Field("Position Offset", current);
            EditorGUI.indentLevel++;
            next.x = EditorGUILayout.Slider(
                new GUIContent("X Offset", "Left / right from the casting hand."), next.x, -3f, 3f);
            next.y = EditorGUILayout.Slider(
                new GUIContent("Y Offset", "Lower / raise from the casting hand."), next.y, -3f, 3f);
            next.z = EditorGUILayout.Slider(
                new GUIContent("Z Offset", "Back / forward from the casting hand."), next.z, -3f, 3f);
            EditorGUI.indentLevel--;
            if (next == current) return;
            Undo.RecordObject(definition, "Adjust Attack VFX Origin");
            SetVfxOffset(attackIndex, next);
            EditorUtility.SetDirty(definition);
        }

        void UpdateVfxMarker(PreviewState state, Bounds bounds, float normalized)
        {
            if (vfxMarker == null) return;
            Renderer markerRenderer = vfxMarker.GetComponent<Renderer>();
            bool visible = state.attackIndex >= 0;
            if (markerRenderer != null) markerRenderer.enabled = visible;
            if (!visible) return;

            Animator animator = instance.GetComponentInChildren<Animator>();
            vfxMarker.transform.position = EnemyController.ResolveAttackVfxOrigin(
                instance.transform, animator,
                instance.GetComponentsInChildren<Renderer>(true), state.vfxOffset);
            float pulse = state.hasImpact &&
                Mathf.Abs(normalized - state.impact) < 0.035f ? 1.65f : 1f;
            float size = Mathf.Max(0.06f, bounds.extents.magnitude * 0.045f) * pulse;
            vfxMarker.transform.localScale = Vector3.one * size;
            UpdateGroundReference(bounds);
        }

        void UpdateGroundReference(Bounds bounds)
        {
            if (groundReference == null) return;
            float width = Mathf.Max(2f, bounds.size.x * 1.8f);
            float depth = Mathf.Max(2f, bounds.size.z * 1.8f);
            groundReference.transform.position =
                new Vector3(bounds.center.x, bounds.min.y - 0.025f, bounds.center.z);
            groundReference.transform.localScale = new Vector3(width, 0.05f, depth);
        }

        void UpdateElementalLightningEffects(
            PreviewState state, Bounds bounds, float duration)
        {
            bool elementalSelected = definition != null &&
                (definition.castAttack == EnemyForgeCastAttack.ElementalLightning ||
                 definition.openingCast == EnemyForgeCastAttack.ElementalLightning);
            bool validAttack = elementalSelected && elementalProfile != null &&
                state.attackIndex >= 0;
            if (!validAttack)
            {
                SetEffectVisible(elementalHandEffect, false, 0f);
                SetEffectVisible(elementalSpellEffect, false, 0f);
                SetEffectVisible(elementalHitEffect, false, 0f);
                return;
            }

            float impactTime = state.hasImpact
                ? Mathf.Clamp01(state.impact) * duration
                : duration;
            float handEnd = elementalProfile.handLifetime > 0f
                ? elementalProfile.handLifetime
                : impactTime;
            float hitElapsed = playTime - impactTime;
            float spellDuration = elementalProfile.spellLifetime > 0f
                ? elementalProfile.spellLifetime
                : 0.75f;

            Animator currentAnimator = instance.GetComponentInChildren<Animator>();
            Vector3 castOrigin = EnemyController.ResolveAttackVfxOrigin(
                instance.transform, currentAnimator,
                instance.GetComponentsInChildren<Renderer>(true), state.vfxOffset);
            Vector3 targetGround = bounds.center +
                instance.transform.forward * Mathf.Max(2f, bounds.extents.z * 3f);
            targetGround.y = bounds.min.y;

            if (elementalHandEffect != null)
            {
                elementalHandEffect.transform.position = castOrigin;
                elementalHandEffect.transform.rotation = instance.transform.rotation;
                elementalHandEffect.transform.localScale = ApplyThickness(
                    elementalHandBaseScale, elementalProfile.handScale,
                    elementalProfile.handThickness);
            }
            if (elementalSpellEffect != null)
            {
                elementalSpellEffect.transform.position =
                    targetGround;
                elementalSpellEffect.transform.rotation = Quaternion.identity;
                elementalSpellEffect.transform.localScale = ApplyThickness(
                    elementalSpellBaseScale, elementalProfile.spellScale,
                    elementalProfile.spellThickness);
            }
            if (elementalHitEffect != null)
            {
                elementalHitEffect.transform.position =
                    targetGround;
                elementalHitEffect.transform.rotation = Quaternion.identity;
                elementalHitEffect.transform.localScale = ApplyThickness(
                    elementalHitBaseScale, elementalProfile.hitScale,
                    elementalProfile.hitThickness);
            }

            SetEffectVisible(elementalHandEffect,
                playTime <= handEnd, playTime);
            SetEffectVisible(elementalSpellEffect,
                hitElapsed >= 0f && hitElapsed <= spellDuration,
                Mathf.Max(0f, hitElapsed));
            SetEffectVisible(elementalHitEffect,
                hitElapsed >= 0f && hitElapsed <= elementalProfile.hitLifetime,
                Mathf.Max(0f, hitElapsed));
        }

        static void SetEffectVisible(GameObject effect, bool visible, float simulationTime)
        {
            if (effect == null) return;
            if (effect.activeSelf != visible) effect.SetActive(visible);
            if (!visible) return;
            foreach (ParticleSystem particles in effect.GetComponentsInChildren<ParticleSystem>(true))
                particles.Simulate(Mathf.Max(0f, simulationTime), true, true, true);
        }

        static Vector3 ApplyThickness(Vector3 baseScale, float scale, float thickness)
        {
            Vector3 sized = baseScale * Mathf.Max(0.01f, scale);
            float width = Mathf.Max(0.01f, thickness);
            sized.x *= width;
            sized.z *= width;
            return sized;
        }

        void DrawVfxStartOverlay(Rect previewRect)
        {
            if (vfxMarker == null || preview == null) return;
            Vector3 viewport = preview.camera.WorldToViewportPoint(vfxMarker.transform.position);
            if (viewport.z <= 0f) return;

            float x = Mathf.Clamp(
                previewRect.x + viewport.x * previewRect.width,
                previewRect.x + 12f, previewRect.xMax - 92f);
            float y = Mathf.Clamp(
                previewRect.y + (1f - viewport.y) * previewRect.height,
                previewRect.y + 12f, previewRect.yMax - 12f);
            EditorGUI.DrawRect(new Rect(x - 10f, y - 10f, 20f, 20f),
                new Color(0.18f, 0.035f, 0.005f, 1f));
            EditorGUI.DrawRect(new Rect(x - 7f, y - 7f, 14f, 14f),
                new Color(1f, 0.35f, 0.025f, 1f));
            EditorGUI.DrawRect(new Rect(x - 3f, y - 3f, 6f, 6f),
                new Color(1f, 0.95f, 0.35f, 1f));

            var labelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            labelStyle.normal.textColor = new Color(1f, 0.78f, 0.15f);
            GUI.Label(new Rect(x + 11f, y - 9f, 82f, 18f), "VFX START", labelStyle);
            GUI.Label(new Rect(previewRect.x + 8f, previewRect.yMax - 24f,
                    previewRect.width - 16f, 18f),
                "Orange target = deployed VFX origin", labelStyle);
        }

        Vector3 GetVfxOffset(int index)
        {
            return index switch
            {
                1 => definition.attackVfxOffset2,
                2 => definition.attackVfxOffset3,
                3 => definition.attackVfxOffset4,
                _ => definition.attackVfxOffset
            };
        }

        void SetVfxOffset(int index, Vector3 value)
        {
            switch (index)
            {
                case 1: definition.attackVfxOffset2 = value; break;
                case 2: definition.attackVfxOffset3 = value; break;
                case 3: definition.attackVfxOffset4 = value; break;
                default: definition.attackVfxOffset = value; break;
            }
        }

        static Bounds BoundsFor(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(Vector3.up, Vector3.one * 2f);
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        static GUIStyle HitStyle()
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = new Color(1f, 0.85f, 0.2f);
            return style;
        }
    }
}
#endif
