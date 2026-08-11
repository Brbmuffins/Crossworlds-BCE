#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crossworlds.EditorTools.LootForge
{
    /// <summary>Renders the authored loot model into a transparent 256x256 inventory sprite.</summary>
    static class LootForgeIconRenderer
    {
        const string IconFolder = "Assets/Game/UI/Resources/Inventory/Generated";
        const int IconSize = 256;

        public static Sprite Render(LootItemDefinition definition, out string error)
        {
            error = null;
            if (definition == null)
            {
                error = "Create or select a Loot Definition first.";
                return null;
            }

            GameObject source = definition.equippedVisualPrefab != null
                ? definition.equippedVisualPrefab
                : definition.worldVisualPrefab;
            if (source == null)
            {
                error = "Assign an Equipped Visual Prefab or World Visual Prefab before generating an icon.";
                return null;
            }

            EnsureFolder(IconFolder);
            string id = string.IsNullOrWhiteSpace(definition.itemId)
                ? source.name.ToLowerInvariant().Replace(' ', '_')
                : definition.itemId.Trim().ToLowerInvariant();
            foreach (char invalid in Path.GetInvalidFileNameChars()) id = id.Replace(invalid, '_');
            string assetPath = $"{IconFolder}/{id}.png";
            string absolutePath = Path.GetFullPath(assetPath);

            Scene previewScene = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null;
            Texture2D output = null;
            var fallbackMaterials = new List<Material>();
            var fallbackMeshes = new List<Mesh>();
            try
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source, previewScene);
                if (instance == null) instance = UnityEngine.Object.Instantiate(source);
                SceneManager.MoveGameObjectToScene(instance, previewScene);
                instance.hideFlags = HideFlags.HideAndDontSave;
                foreach (Transform child in instance.GetComponentsInChildren<Transform>(true))
                    child.gameObject.layer = 0;
                foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                {
                    if (!IsPhotographedRenderer(renderer)) continue;
                    renderer.forceRenderingOff = false;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.renderingLayerMask = uint.MaxValue;
                }

                Bounds bounds = CalculateBounds(instance);
                if (bounds.size.sqrMagnitude < 0.0001f)
                {
                    error = "The selected visual has no enabled Renderer and cannot be photographed.";
                    return null;
                }

                // Rotate first, then center. Centering before rotation moves an
                // off-pivot item away from the camera when the root is rotated.
                instance.transform.rotation = Quaternion.Euler(
                    definition.inventoryIconEulerAngles);
                bounds = CalculateBounds(instance);
                instance.transform.position -= bounds.center;
                bounds = CalculateBounds(instance);

                var cameraObject = new GameObject("LootIconCamera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, previewScene);
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = ~0;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camera.orthographic = true;
                float zoom = Mathf.Clamp(definition.inventoryIconZoom, 0.5f, 3f);
                camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x) *
                                          1.18f * zoom;
                camera.nearClipPlane = 0.01f;
                float cameraDistance = Mathf.Max(4f, bounds.size.magnitude * 2f);
                camera.farClipPlane = cameraDistance + bounds.extents.magnitude * 2f + 10f;
                camera.transform.position = new Vector3(0f, 0f, -cameraDistance);
                camera.transform.LookAt(Vector3.zero);

                AddLight(previewScene, "Key", new Vector3(-2f, 3f, -4f), 1.25f);
                AddLight(previewScene, "Fill", new Vector3(3f, 1f, -2f), 0.65f);

                target = new RenderTexture(IconSize, IconSize, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = 8,
                    hideFlags = HideFlags.HideAndDontSave
                };
                target.Create();
                camera.targetTexture = target;
                camera.Render();

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                output = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false, false);
                output.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
                output.Apply(false, false);
                RenderTexture.active = previous;
                RemoveNativePreviewBackground(output);

                int visiblePixelCount = 0;
                foreach (Color32 pixel in output.GetPixels32())
                    if (pixel.a > 8 && (pixel.r + pixel.g + pixel.b) > 12)
                        visiblePixelCount++;
                if (visiblePixelCount < 64)
                {
                    // Some imported FBX materials rely on scene-only shader state
                    // and render transparent in an isolated URP preview. Retry the
                    // mesh with an opaque, double-sided unlit copy of its texture.
                    Shader fallbackShader = Shader.Find("Universal Render Pipeline/Unlit") ??
                                            Shader.Find("Unlit/Texture");
                    if (fallbackShader != null)
                    {
                        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                        {
                            if (!IsPhotographedRenderer(renderer)) continue;
                            Mesh mesh = null;
                            if (renderer is MeshRenderer)
                                mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
                            else if (renderer is SkinnedMeshRenderer skinned)
                            {
                                mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
                                skinned.BakeMesh(mesh);
                                fallbackMeshes.Add(mesh);
                            }
                            if (mesh == null) continue;

                            Material[] sourceMaterials = renderer.sharedMaterials;
                            if (sourceMaterials == null || sourceMaterials.Length == 0)
                                sourceMaterials = new Material[1];
                            var replacements = new Material[sourceMaterials.Length];
                            for (int i = 0; i < replacements.Length; i++)
                            {
                                Material sourceMaterial = sourceMaterials[i];
                                var replacement = new Material(fallbackShader)
                                {
                                    hideFlags = HideFlags.HideAndDontSave
                                };
                                Texture texture = sourceMaterial != null
                                    ? sourceMaterial.GetTexture("_BaseMap") ?? sourceMaterial.mainTexture
                                    : null;
                                if (texture != null)
                                {
                                    if (replacement.HasProperty("_BaseMap"))
                                        replacement.SetTexture("_BaseMap", texture);
                                    replacement.mainTexture = texture;
                                }
                                if (replacement.HasProperty("_BaseColor"))
                                    replacement.SetColor("_BaseColor", Color.white);
                                if (replacement.HasProperty("_Cull"))
                                    replacement.SetFloat("_Cull", 0f);
                                replacements[i] = replacement;
                                fallbackMaterials.Add(replacement);
                            }

                            var proxy = new GameObject(
                                renderer.name + "_LootIconProxy",
                                typeof(MeshFilter), typeof(MeshRenderer));
                            proxy.hideFlags = HideFlags.HideAndDontSave;
                            proxy.layer = 0;
                            SceneManager.MoveGameObjectToScene(proxy, previewScene);
                            proxy.transform.SetPositionAndRotation(
                                renderer.transform.position, renderer.transform.rotation);
                            proxy.transform.localScale = renderer.transform.lossyScale;
                            proxy.GetComponent<MeshFilter>().sharedMesh = mesh;
                            proxy.GetComponent<MeshRenderer>().sharedMaterials = replacements;
                            renderer.enabled = false;
                        }

                        camera.Render();
                        RenderTexture.active = target;
                        output.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
                        output.Apply(false, false);
                        RenderTexture.active = previous;
                        RemoveNativePreviewBackground(output);
                        visiblePixelCount = RestoreMissingAlpha(output);
                    }

                    if (visiblePixelCount < 64)
                    {
                        // Manual Camera.Render can fail for some URP/importer
                        // combinations even though Unity's own asset preview can
                        // display the prefab. Use that editor-native renderer as
                        // the final fallback.
                        UnityEditor.Editor previewEditor = null;
                        Texture2D staticPreview = null;
                        try
                        {
                            previewEditor = UnityEditor.Editor.CreateEditor(source);
                            staticPreview = previewEditor?.RenderStaticPreview(
                                AssetDatabase.GetAssetPath(source), null, IconSize, IconSize);
                            if (staticPreview == null)
                                staticPreview = AssetPreview.GetAssetPreview(source) ??
                                                AssetPreview.GetMiniThumbnail(source);
                            if (staticPreview != null)
                            {
                                Graphics.Blit(staticPreview, target);
                                RenderTexture.active = target;
                                output.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
                                output.Apply(false, false);
                                RenderTexture.active = previous;
                                RemoveNativePreviewBackground(output);
                                visiblePixelCount = RestoreMissingAlpha(output);
                            }
                        }
                        finally
                        {
                            if (previewEditor != null)
                                UnityEngine.Object.DestroyImmediate(previewEditor);
                            if (staticPreview != null &&
                                !AssetDatabase.Contains(staticPreview))
                                UnityEngine.Object.DestroyImmediate(staticPreview);
                        }
                    }

                    if (visiblePixelCount < 64)
                    {
                        Debug.LogError(
                            $"[LOOT FORGE ICON] Empty render for '{source.name}'. " +
                            $"Bounds center={bounds.center} size={bounds.size}; " +
                            $"camera={camera.transform.position} ortho={camera.orthographicSize} " +
                            $"near={camera.nearClipPlane} far={camera.farClipPlane}; " +
                            $"meshRenderers={CountPhotographedRenderers(instance)}.", source);
                        error = "The selected visual contains geometry but neither its material nor the opaque fallback produced a visible image. The existing PNG was preserved.";
                        return null;
                    }
                }
                File.WriteAllBytes(absolutePath, output.EncodeToPNG());
            }
            catch (Exception exception)
            {
                error = $"Icon generation failed: {exception.Message}";
                return null;
            }
            finally
            {
                if (output != null) UnityEngine.Object.DestroyImmediate(output);
                foreach (Material material in fallbackMaterials)
                    if (material != null) UnityEngine.Object.DestroyImmediate(material);
                foreach (Mesh mesh in fallbackMeshes)
                    if (mesh != null) UnityEngine.Object.DestroyImmediate(mesh);
                if (target != null)
                {
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(target);
                }
                if (previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.sRGBTexture = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = IconSize;
                importer.spritePixelsPerUnit = 100f;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        static Bounds CalculateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool found = false;
            foreach (Renderer renderer in renderers)
            {
                // Persistent weapon VFX must not influence the photographed mesh
                // framing. Their dynamic bounds can be much larger than the item.
                if (!IsPhotographedRenderer(renderer)) continue;
                if (!found) { bounds = renderer.bounds; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        static bool IsPhotographedRenderer(Renderer renderer) =>
            renderer != null && renderer.enabled &&
            renderer is not ParticleSystemRenderer &&
            renderer is not TrailRenderer && renderer is not LineRenderer;

        static int CountPhotographedRenderers(GameObject root)
        {
            int count = 0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                if (IsPhotographedRenderer(renderer)) count++;
            return count;
        }

        static int RestoreMissingAlpha(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            int alphaPixels = 0;
            int rgbPixels = 0;
            foreach (Color32 pixel in pixels)
            {
                if (pixel.a > 8) alphaPixels++;
                if ((pixel.r + pixel.g + pixel.b) > 12) rgbPixels++;
            }
            if (alphaPixels >= 64 || rgbPixels < 64) return alphaPixels;

            // URP can preserve mesh RGB while returning zero alpha from a manual
            // Camera.Render into a transparent RenderTexture. Reconstruct alpha
            // only for colored pixels; the black clear background remains clear.
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                if ((pixel.r + pixel.g + pixel.b) <= 12) continue;
                pixel.a = (byte)Mathf.Clamp(
                    Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b)) * 2, 32, 255);
                pixels[i] = pixel;
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return rgbPixels;
        }

        static void RemoveNativePreviewBackground(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            Color32[] pixels = texture.GetPixels32();
            Color32 a = pixels[0];
            Color32 b = pixels[width - 1];
            Color32 c = pixels[(height - 1) * width];
            Color32 d = pixels[pixels.Length - 1];
            var background = new Color32(
                (byte)((a.r + b.r + c.r + d.r) / 4),
                (byte)((a.g + b.g + c.g + d.g) / 4),
                (byte)((a.b + b.b + c.b + d.b) / 4), 255);

            const float hardDistance = 4f;
            const float edgeDistance = 58f;
            float maximumDistanceSquared = edgeDistance * edgeDistance;
            var connected = new bool[pixels.Length];
            var queue = new Queue<int>();

            void AddIfBackground(int index)
            {
                if (connected[index] || ColorDistanceSquared(pixels[index], background) > maximumDistanceSquared)
                    return;
                connected[index] = true;
                queue.Enqueue(index);
            }

            for (int x = 0; x < width; x++)
            {
                AddIfBackground(x);
                AddIfBackground((height - 1) * width + x);
            }
            for (int y = 0; y < height; y++)
            {
                AddIfBackground(y * width);
                AddIfBackground(y * width + width - 1);
            }

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int x = index % width;
                int y = index / width;
                if (x > 0) AddIfBackground(index - 1);
                if (x + 1 < width) AddIfBackground(index + 1);
                if (y > 0) AddIfBackground(index - width);
                if (y + 1 < height) AddIfBackground(index + width);
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                if (!connected[i]) continue;
                Color32 pixel = pixels[i];
                float distance = Mathf.Sqrt(ColorDistanceSquared(pixel, background));
                float alpha = Mathf.Clamp01((distance - hardDistance) /
                                            (edgeDistance - hardDistance));
                if (alpha <= 0.001f)
                {
                    pixel.a = 0;
                }
                else
                {
                    // Undo the preview background contribution from blended edge
                    // pixels to avoid a gray fringe in the inventory slot.
                    pixel.r = Unblend(pixel.r, background.r, alpha);
                    pixel.g = Unblend(pixel.g, background.g, alpha);
                    pixel.b = Unblend(pixel.b, background.b, alpha);
                    pixel.a = (byte)Mathf.RoundToInt(alpha * 255f);
                }
                pixels[i] = pixel;
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
        }

        static float ColorDistanceSquared(Color32 value, Color32 background)
        {
            float r = value.r - background.r;
            float g = value.g - background.g;
            float b = value.b - background.b;
            return r * r + g * g + b * b;
        }

        static byte Unblend(byte value, byte background, float alpha) =>
            (byte)Mathf.Clamp(Mathf.RoundToInt(
                (value - background * (1f - alpha)) / Mathf.Max(0.001f, alpha)), 0, 255);

        static void AddLight(Scene scene, string name, Vector3 position, float intensity)
        {
            var lightObject = new GameObject(name, typeof(Light));
            SceneManager.MoveGameObjectToScene(lightObject, scene);
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = intensity;
            light.color = new Color(1f, 0.93f, 0.82f);
            lightObject.transform.position = position;
            lightObject.transform.LookAt(Vector3.zero);
        }

        static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
