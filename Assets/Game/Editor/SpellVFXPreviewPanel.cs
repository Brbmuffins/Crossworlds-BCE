#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.VFX;

namespace Crossworlds.EditorTools
{
    /// <summary>
    /// Stages a class prefab, its cast animation, and the assigned spell
    /// prefabs together inside the Spellbook window.
    /// </summary>
    internal sealed class SpellVFXPreviewPanel : IDisposable
    {
        const float HitDelay = 0.35f;

        PreviewRenderUtility preview;
        GameObject characterInstance;
        GameObject castingInstance;
        GameObject castInstance;
        GameObject hitInstance;
        GameObject deployableInstance;
        GameObject ground;
        GameObject targetMarker;
        Material groundMaterial;
        Material markerMaterial;
        Animator characterAnimator;
        PlayableGraph animationGraph;

        GameObject characterSource;
        GameObject castingSource;
        GameObject castSource;
        GameObject hitSource;
        GameObject deployableSource;
        AnimationClip castAnimation;
        AbilityCategory category;
        float castTime;
        float range;

        bool playing = true;
        bool castTriggered;
        bool hitTriggered;
        bool deployableTriggered;
        float previewTime;
        float sequenceDuration = 4f;
        float nextBoundsUpdate;
        double lastUpdate;
        Bounds stageBounds =
            new Bounds(Vector3.up, Vector3.one * 2f);

        public bool IsPlaying => playing;

        public string AnimationLabel =>
            castAnimation != null
                ? castAnimation.name
                : $"{category} Cast Animation";

        public void EnsureSpell(
            GameObject nextCharacter,
            AnimationClip nextAnimation,
            AbilityCategory nextCategory,
            float nextCastTime,
            float nextRange,
            GameObject nextCastingVFX,
            GameObject nextCastVFX,
            GameObject nextHitVFX,
            GameObject nextDeployable)
        {
            if (Matches(
                nextCharacter,
                nextAnimation,
                nextCategory,
                nextCastTime,
                nextRange,
                nextCastingVFX,
                nextCastVFX,
                nextHitVFX,
                nextDeployable))
                return;

            characterSource = nextCharacter;
            castAnimation = nextAnimation;
            category = nextCategory;
            castTime = Mathf.Max(0f, nextCastTime);
            range = Mathf.Max(0f, nextRange);
            castingSource = nextCastingVFX;
            castSource = nextCastVFX;
            hitSource = nextHitVFX;
            deployableSource = nextDeployable;
            BuildStage();
        }

        public bool Tick()
        {
            if (preview == null ||
                characterInstance == null ||
                !playing)
                return false;

            double now = EditorApplication.timeSinceStartup;
            if (lastUpdate <= 0d) lastUpdate = now;
            float delta = Mathf.Min(
                (float)(now - lastUpdate), 0.05f);
            lastUpdate = now;

            AdvanceCharacter(delta);
            previewTime += delta;
            ActivateDueEffects();
            AdvanceEffect(castingInstance, delta);
            AdvanceEffect(castInstance, delta);
            AdvanceEffect(hitInstance, delta);
            AdvanceEffect(deployableInstance, delta);

            if (previewTime >= nextBoundsUpdate)
            {
                stageBounds = CalculateStageBounds();
                nextBoundsUpdate = previewTime + 0.25f;
            }

            if (previewTime >= sequenceDuration)
                Replay();

            return true;
        }

        public void Draw(Rect rect)
        {
            EditorGUI.DrawRect(
                rect, new Color(0.045f, 0.04f, 0.06f, 1f));
            if (preview == null || characterInstance == null)
                return;

            Vector3 center = stageBounds.center;
            float radius = Mathf.Clamp(
                stageBounds.extents.magnitude, 1.25f, 25f);
            Quaternion orbit =
                Quaternion.Euler(14f, 24f, 0f);
            preview.camera.transform.position =
                center + orbit * new Vector3(
                    0f, radius * 0.18f, radius * 2.6f);
            preview.camera.transform.LookAt(
                center + Vector3.up * radius * 0.04f);
            preview.camera.aspect = Mathf.Max(
                0.1f, rect.width / Mathf.Max(1f, rect.height));
            preview.camera.nearClipPlane =
                Mathf.Max(0.01f, radius * 0.01f);
            preview.camera.farClipPlane =
                Mathf.Max(50f, radius * 10f);

            if (ground != null)
            {
                ground.transform.localScale =
                    Vector3.one * Mathf.Clamp(
                        radius * 0.4f, 0.65f, 8f);
            }

            preview.BeginPreview(rect, GUIStyle.none);
            preview.Render(true);
            Texture result = preview.EndPreview();
            if (result != null)
            {
                GUI.DrawTexture(
                    rect, result,
                    ScaleMode.StretchToFill, false);
            }

            GUI.Label(
                new Rect(
                    rect.x + 8f, rect.y + 7f,
                    rect.width - 16f, 20f),
                $"CHARACTER SPELL PREVIEW  •  " +
                $"{PreviewPhase()}  •  {previewTime:0.0}s",
                EditorStyles.whiteMiniLabel);
        }

        public void Replay()
        {
            if (characterInstance == null) return;

            previewTime = 0f;
            nextBoundsUpdate = 0f;
            lastUpdate = EditorApplication.timeSinceStartup;
            castTriggered = false;
            hitTriggered = false;
            deployableTriggered = false;
            DeactivateEffect(castingInstance);
            DeactivateEffect(castInstance);
            DeactivateEffect(hitInstance);
            DeactivateEffect(deployableInstance);
            StartCharacterAnimation();
            ActivateEffect(castingInstance);
            ActivateDueEffects();
            if (!playing)
            {
                SetEffectPause(castingInstance, true);
                SetEffectPause(castInstance, true);
                SetEffectPause(hitInstance, true);
                SetEffectPause(deployableInstance, true);
            }
            stageBounds = CalculateStageBounds();
        }

        public void TogglePlayback()
        {
            playing = !playing;
            SetEffectPause(castingInstance, !playing);
            SetEffectPause(castInstance, !playing);
            SetEffectPause(hitInstance, !playing);
            SetEffectPause(deployableInstance, !playing);
            lastUpdate = EditorApplication.timeSinceStartup;
        }

        public void Clear()
        {
            if (animationGraph.IsValid())
                animationGraph.Destroy();

            DestroyMaterial(ref groundMaterial);
            DestroyMaterial(ref markerMaterial);

            if (preview != null)
                preview.Cleanup();
            preview = null;
            characterInstance = null;
            castingInstance = null;
            castInstance = null;
            hitInstance = null;
            deployableInstance = null;
            ground = null;
            targetMarker = null;
            characterAnimator = null;
            characterSource = null;
            castingSource = null;
            castSource = null;
            hitSource = null;
            deployableSource = null;
            castAnimation = null;
            previewTime = 0f;
            lastUpdate = 0d;
            stageBounds =
                new Bounds(Vector3.up, Vector3.one * 2f);
        }

        public void Dispose()
        {
            Clear();
        }

        bool Matches(
            GameObject nextCharacter,
            AnimationClip nextAnimation,
            AbilityCategory nextCategory,
            float nextCastTime,
            float nextRange,
            GameObject nextCastingVFX,
            GameObject nextCastVFX,
            GameObject nextHitVFX,
            GameObject nextDeployable)
        {
            return preview != null &&
                   characterSource == nextCharacter &&
                   castAnimation == nextAnimation &&
                   category == nextCategory &&
                   Mathf.Approximately(
                       castTime, Mathf.Max(0f, nextCastTime)) &&
                   Mathf.Approximately(
                       range, Mathf.Max(0f, nextRange)) &&
                   castingSource == nextCastingVFX &&
                   castSource == nextCastVFX &&
                   hitSource == nextHitVFX &&
                   deployableSource == nextDeployable;
        }

        void BuildStage()
        {
            GameObject nextCharacter = characterSource;
            AnimationClip nextAnimation = castAnimation;
            AbilityCategory nextCategory = category;
            float nextCastTime = castTime;
            float nextRange = range;
            GameObject nextCasting = castingSource;
            GameObject nextCast = castSource;
            GameObject nextHit = hitSource;
            GameObject nextDeployable = deployableSource;

            Clear();

            characterSource = nextCharacter;
            castAnimation = nextAnimation;
            category = nextCategory;
            castTime = nextCastTime;
            range = nextRange;
            castingSource = nextCasting;
            castSource = nextCast;
            hitSource = nextHit;
            deployableSource = nextDeployable;

            if (characterSource == null) return;

            preview = new PreviewRenderUtility();
            preview.camera.fieldOfView = 34f;
            preview.camera.clearFlags = CameraClearFlags.Color;
            preview.camera.backgroundColor =
                new Color(0.045f, 0.04f, 0.06f, 1f);
            preview.lights[0].intensity = 1.25f;
            preview.lights[0].transform.rotation =
                Quaternion.Euler(35f, 35f, 0f);
            preview.lights[1].intensity = 0.9f;

            characterInstance = CreateStageObject(
                characterSource,
                characterSource.name + "_SpellbookCharacter",
                Vector3.zero,
                true);
            characterAnimator = characterInstance
                .GetComponentInChildren<Animator>(true);
            if (characterAnimator != null)
            {
                characterAnimator.enabled = true;
                characterAnimator.applyRootMotion = false;
                characterAnimator.cullingMode =
                    AnimatorCullingMode.AlwaysAnimate;
            }

            Vector3 targetPosition =
                Vector3.forward * PreviewDistance(range);
            castingInstance = CreateStageObject(
                castingSource,
                "Spellbook_CastingVFX",
                Vector3.zero,
                false);
            if (castingInstance != null)
                castingInstance.transform.SetParent(
                    characterInstance.transform, true);
            castInstance = CreateStageObject(
                castSource,
                "Spellbook_CastVFX",
                targetPosition + Vector3.up * 0.8f,
                false);
            hitInstance = CreateStageObject(
                hitSource,
                "Spellbook_HitVFX",
                targetPosition + Vector3.up * 0.5f,
                false);
            deployableInstance = CreateStageObject(
                deployableSource,
                "Spellbook_Deployable",
                targetPosition,
                false);

            CreateGround();
            CreateTargetMarker(targetPosition);
            sequenceDuration = Mathf.Max(
                4f,
                castTime + 2.5f,
                castAnimation != null
                    ? castAnimation.length + 0.5f
                    : 0f);
            Replay();
        }

        GameObject CreateStageObject(
            GameObject source,
            string instanceName,
            Vector3 position,
            bool startActive)
        {
            if (source == null) return null;

            GameObject instance =
                UnityEngine.Object.Instantiate(source);
            instance.name = instanceName;
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.identity;

            foreach (MonoBehaviour behaviour in
                instance.GetComponentsInChildren<
                    MonoBehaviour>(true))
            {
                if (behaviour != null)
                    behaviour.enabled = false;
            }

            foreach (Camera camera in
                instance.GetComponentsInChildren<Camera>(true))
            {
                if (camera != null)
                    camera.enabled = false;
            }

            foreach (AudioSource audioSource in
                instance.GetComponentsInChildren<
                    AudioSource>(true))
            {
                if (audioSource != null)
                    audioSource.enabled = false;
            }

            instance.SetActive(true);
            preview.AddSingleGO(instance);
            instance.SetActive(startActive);
            return instance;
        }

        void CreateGround()
        {
            ground =
                GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "SpellbookPreview_Ground";
            ground.hideFlags = HideFlags.HideAndDontSave;
            ground.transform.position = Vector3.zero;
            RemoveCollider(ground);

            Renderer renderer = ground.GetComponent<Renderer>();
            Shader shader = PreviewShader();
            if (renderer != null && shader != null)
            {
                groundMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    color = new Color(0.10f, 0.09f, 0.12f, 1f)
                };
                renderer.sharedMaterial = groundMaterial;
            }

            preview.AddSingleGO(ground);
        }

        void CreateTargetMarker(Vector3 position)
        {
            targetMarker =
                GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            targetMarker.name = "SpellbookPreview_Target";
            targetMarker.hideFlags = HideFlags.HideAndDontSave;
            targetMarker.transform.position =
                position + Vector3.up * 0.015f;
            targetMarker.transform.localScale =
                new Vector3(0.65f, 0.015f, 0.65f);
            RemoveCollider(targetMarker);

            Renderer renderer =
                targetMarker.GetComponent<Renderer>();
            Shader shader = PreviewShader();
            if (renderer != null && shader != null)
            {
                markerMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    color = new Color(0.35f, 0.12f, 0.52f, 1f)
                };
                renderer.sharedMaterial = markerMaterial;
            }

            preview.AddSingleGO(targetMarker);
        }

        void StartCharacterAnimation()
        {
            if (animationGraph.IsValid())
                animationGraph.Destroy();
            if (characterAnimator == null) return;

            characterAnimator.Rebind();
            characterAnimator.Update(0f);

            if (castAnimation != null)
            {
                animationGraph = PlayableGraph.Create(
                    "Spellbook Character Cast Preview");
                animationGraph.SetTimeUpdateMode(
                    DirectorUpdateMode.Manual);
                AnimationClipPlayable clipPlayable =
                    AnimationClipPlayable.Create(
                        animationGraph, castAnimation);
                clipPlayable.SetApplyFootIK(true);
                clipPlayable.SetApplyPlayableIK(true);
                AnimationPlayableOutput output =
                    AnimationPlayableOutput.Create(
                        animationGraph,
                        "Spellbook Cast",
                        characterAnimator);
                output.SetSourcePlayable(clipPlayable);
                animationGraph.Play();
                animationGraph.Evaluate(0f);
                return;
            }

            string trigger = category switch
            {
                AbilityCategory.Heal => "CastHeal",
                AbilityCategory.Support => "CastSupport",
                _ => "CastDamage"
            };
            foreach (AnimatorControllerParameter parameter in
                characterAnimator.parameters)
            {
                if (parameter.type !=
                        AnimatorControllerParameterType.Trigger ||
                    parameter.name != trigger)
                    continue;

                characterAnimator.SetTrigger(trigger);
                characterAnimator.Update(0f);
                break;
            }
        }

        void AdvanceCharacter(float delta)
        {
            if (animationGraph.IsValid())
                animationGraph.Evaluate(delta);
            else if (characterAnimator != null &&
                     characterAnimator.enabled)
                characterAnimator.Update(delta);
        }

        void ActivateDueEffects()
        {
            if (castingInstance != null &&
                previewTime >= castTime &&
                castingInstance.activeSelf)
            {
                DeactivateEffect(castingInstance);
            }

            if (!castTriggered && previewTime >= castTime)
            {
                castTriggered = true;
                ActivateEffect(castInstance);
            }

            if (!deployableTriggered && previewTime >= castTime)
            {
                deployableTriggered = true;
                ActivateEffect(deployableInstance);
            }

            if (!hitTriggered &&
                previewTime >= castTime + HitDelay)
            {
                hitTriggered = true;
                ActivateEffect(hitInstance);
            }
        }

        static void ActivateEffect(GameObject root)
        {
            if (root == null) return;
            root.SetActive(true);

            foreach (PlayableDirector director in
                root.GetComponentsInChildren<
                    PlayableDirector>(true))
            {
                if (director == null) continue;
                director.time = 0d;
                director.Evaluate();
            }

            foreach (Animator animator in
                root.GetComponentsInChildren<Animator>(true))
            {
                if (animator == null) continue;
                animator.enabled = true;
                animator.Rebind();
                animator.Update(0f);
            }

            foreach (ParticleSystem particles in
                root.GetComponentsInChildren<
                    ParticleSystem>(true))
            {
                if (particles == null) continue;
                particles.Stop(
                    false,
                    ParticleSystemStopBehavior
                        .StopEmittingAndClear);
                particles.Clear(false);
                particles.Simulate(
                    0f, false, true, false);
                particles.Play(false);
            }

            foreach (VisualEffect effect in
                root.GetComponentsInChildren<
                    VisualEffect>(true))
            {
                if (effect == null) continue;
                effect.Reinit();
                effect.pause = false;
                effect.Play();
            }
        }

        static void DeactivateEffect(GameObject root)
        {
            if (root == null) return;

            foreach (ParticleSystem particles in
                root.GetComponentsInChildren<
                    ParticleSystem>(true))
            {
                if (particles == null) continue;
                particles.Stop(
                    false,
                    ParticleSystemStopBehavior
                        .StopEmittingAndClear);
                particles.Clear(false);
            }

            foreach (VisualEffect effect in
                root.GetComponentsInChildren<
                    VisualEffect>(true))
            {
                if (effect != null)
                    effect.pause = true;
            }

            root.SetActive(false);
        }

        static void AdvanceEffect(
            GameObject root, float delta)
        {
            if (root == null || !root.activeInHierarchy)
                return;

            foreach (ParticleSystem particles in
                root.GetComponentsInChildren<
                    ParticleSystem>(true))
            {
                if (particles != null)
                    particles.Simulate(
                        delta, false, false, false);
            }

            foreach (Animator animator in
                root.GetComponentsInChildren<Animator>(true))
            {
                if (animator != null && animator.enabled)
                    animator.Update(delta);
            }

            foreach (PlayableDirector director in
                root.GetComponentsInChildren<
                    PlayableDirector>(true))
            {
                if (director == null ||
                    director.playableAsset == null)
                    continue;
                double duration =
                    director.playableAsset.duration;
                director.time = duration > 0d
                    ? (director.time + delta) % duration
                    : director.time + delta;
                director.Evaluate();
            }

            foreach (VisualEffect effect in
                root.GetComponentsInChildren<
                    VisualEffect>(true))
            {
                if (effect != null)
                    effect.AdvanceOneFrame();
            }
        }

        static void SetEffectPause(
            GameObject root, bool paused)
        {
            if (root == null) return;
            foreach (VisualEffect effect in
                root.GetComponentsInChildren<
                    VisualEffect>(true))
            {
                if (effect != null)
                    effect.pause = paused;
            }
        }

        Bounds CalculateStageBounds()
        {
            bool found = false;
            Bounds bounds =
                new Bounds(Vector3.up, Vector3.one * 2f);
            EncapsulateRenderers(
                characterInstance, ref bounds, ref found);
            EncapsulateRenderers(
                targetMarker, ref bounds, ref found);
            EncapsulateRenderers(
                castInstance, ref bounds, ref found);
            EncapsulateRenderers(
                hitInstance, ref bounds, ref found);
            EncapsulateRenderers(
                deployableInstance, ref bounds, ref found);

            Vector3 extents = bounds.extents;
            extents.x = Mathf.Clamp(extents.x, 0.75f, 25f);
            extents.y = Mathf.Clamp(extents.y, 0.75f, 25f);
            extents.z = Mathf.Clamp(extents.z, 0.75f, 25f);
            bounds.extents = extents;
            return bounds;
        }

        static void EncapsulateRenderers(
            GameObject root,
            ref Bounds bounds,
            ref bool found)
        {
            if (root == null || !root.activeInHierarchy)
                return;

            foreach (Renderer renderer in
                root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null ||
                    !renderer.enabled ||
                    !renderer.gameObject.activeInHierarchy)
                    continue;
                Bounds next = renderer.bounds;
                if (next.size.sqrMagnitude < 0.0001f)
                    continue;

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
        }

        string PreviewPhase()
        {
            if (previewTime < castTime)
                return "CASTING";
            if (previewTime < castTime + HitDelay)
                return "RESOLVING";
            return "IMPACT";
        }

        static float PreviewDistance(float spellRange)
        {
            if (spellRange <= 0.1f) return 0.35f;
            return Mathf.Clamp(
                spellRange * 0.35f, 1.5f, 4f);
        }

        static Shader PreviewShader()
        {
            return
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Standard");
        }

        static void RemoveCollider(GameObject target)
        {
            Collider collider = target.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }

        static void DestroyMaterial(ref Material material)
        {
            if (material != null)
                UnityEngine.Object.DestroyImmediate(material);
            material = null;
        }
    }
}
#endif
