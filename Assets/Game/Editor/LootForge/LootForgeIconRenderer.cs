#if UNITY_EDITOR
using System;
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
            try
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source, previewScene);
                if (instance == null) instance = UnityEngine.Object.Instantiate(source);
                SceneManager.MoveGameObjectToScene(instance, previewScene);
                instance.hideFlags = HideFlags.HideAndDontSave;

                Bounds bounds = CalculateBounds(instance);
                if (bounds.size.sqrMagnitude < 0.0001f)
                {
                    error = "The selected visual has no enabled Renderer and cannot be photographed.";
                    return null;
                }

                instance.transform.position -= bounds.center;
                instance.transform.rotation = Quaternion.Euler(12f, -28f, -8f);
                bounds = CalculateBounds(instance);

                var cameraObject = new GameObject("LootIconCamera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, previewScene);
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camera.orthographic = true;
                camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x) * 1.18f;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 100f;
                camera.transform.position = new Vector3(0f, 0f, -Mathf.Max(4f, bounds.size.magnitude * 2f));
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
                if (!renderer.enabled) continue;
                if (!found) { bounds = renderer.bounds; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

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
