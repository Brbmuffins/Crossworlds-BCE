#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Crossworlds.EditorTools.EnemyForge
{
    internal static class EnemyForgeAnimationLibrary
    {
        const string ModelRoot = "Assets/Game/3D Models";

        public static string FindSuggestedFolder(GameObject source)
        {
            if (source == null) return string.Empty;
            string sourcePath = AssetDatabase.GetAssetPath(source).Replace('\\', '/');
            string sourceDirectory = Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(sourceDirectory)) return string.Empty;
            string sourceName = Path.GetFileNameWithoutExtension(sourcePath);
            var candidates = new List<string>();

            // Walk from the selected prefab toward the shared 3D Models root.
            // The nearest ancestor with an Animation child is the prefab's model family.
            string ancestor = sourceDirectory;
            while (!string.IsNullOrEmpty(ancestor) &&
                   ancestor.StartsWith(ModelRoot, StringComparison.OrdinalIgnoreCase))
            {
                string animation = ancestor + "/Animation";
                if (AssetDatabase.IsValidFolder(animation) && LoadClips(animation).Count > 0)
                    return animation;
                if (ancestor.Equals(ModelRoot, StringComparison.OrdinalIgnoreCase)) break;
                ancestor = Path.GetDirectoryName(ancestor)?.Replace('\\', '/');
            }

            // Preferred project layout:
            //   .../model_aw_instigator/Prefab/model_aw_instigator_3D_rigged.prefab
            //   .../model_aw_instigator_Animation/
            string modelDirectory = Path.GetFileName(sourceDirectory).Equals("Prefab", StringComparison.OrdinalIgnoreCase)
                ? Path.GetDirectoryName(sourceDirectory)?.Replace('\\', '/')
                : sourceDirectory;
            if (!string.IsNullOrEmpty(modelDirectory))
                candidates.Add(modelDirectory + "/Animation");

            string normalizedSourceName = NormalizeModelName(sourceName);
            candidates.Add(sourceDirectory + "/Animation");
            candidates.Add(sourceDirectory + "/" + sourceName + "/Animation");

            foreach (string candidate in candidates)
                if (AssetDatabase.IsValidFolder(candidate) && LoadClips(candidate).Count > 0) return candidate;

            var matchingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { normalizedSourceName };
            ancestor = sourceDirectory;
            while (!string.IsNullOrEmpty(ancestor) && ancestor.StartsWith(ModelRoot, StringComparison.OrdinalIgnoreCase))
            {
                string leafName = Path.GetFileName(ancestor);
                if (!leafName.Equals("Prefab", StringComparison.OrdinalIgnoreCase) &&
                    !leafName.Equals("Animation", StringComparison.OrdinalIgnoreCase))
                    matchingNames.Add(NormalizeModelName(leafName));
                if (ancestor.Equals(ModelRoot, StringComparison.OrdinalIgnoreCase)) break;
                ancestor = Path.GetDirectoryName(ancestor)?.Replace('\\', '/');
            }

            string[] folders = AssetDatabase.FindAssets("t:Folder", new[] { ModelRoot });
            foreach (string guid in folders)
            {
                string folder = AssetDatabase.GUIDToAssetPath(guid);
                string leaf = Path.GetFileName(folder);
                if (!leaf.Equals("Animation", StringComparison.OrdinalIgnoreCase) || LoadClips(folder).Count == 0) continue;
                string parentName = NormalizeModelName(Path.GetFileName(Path.GetDirectoryName(folder)));
                if (matchingNames.Contains(parentName)) return folder;
            }
            return string.Empty;
        }

        static string NormalizeModelName(string value)
        {
            string normalized = value;
            if (normalized.StartsWith("prefab_", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(7);
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s*\(\d+\)(\s+\d+)?$", string.Empty);
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"_3d_rigged.*$", string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"_enemy$", string.Empty,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return normalized.Trim();
        }

        public static List<AnimationClip> LoadClips(string folder)
        {
            var clips = new List<AnimationClip>();
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder)) return clips;
            foreach (string guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { folder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())
                    if (!clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase) && !clips.Contains(clip)) clips.Add(clip);
            }
            clips.Sort((a, b) => string.Compare(GetDisplayName(a), GetDisplayName(b), StringComparison.OrdinalIgnoreCase));
            return clips;
        }

        public static string GetDisplayName(AnimationClip clip)
        {
            if (clip == null) return "None";
            string path = AssetDatabase.GetAssetPath(clip);
            string fileName = string.IsNullOrEmpty(path) ? clip.name : Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(clip.name) || clip.name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                return fileName;
            return fileName + " — " + clip.name;
        }

        public static bool IsFolderForSource(GameObject source, string folder)
        {
            if (source == null || string.IsNullOrEmpty(folder)) return false;
            string expected = FindSuggestedFolder(source);
            if (!string.IsNullOrEmpty(expected))
                return expected.Equals(folder.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public static string BrowseForFolder()
        {
            string absolute = EditorUtility.OpenFolderPanel("Select Animation Folder", Application.dataPath, string.Empty);
            string path = ToAssetPath(absolute);
            if (string.IsNullOrEmpty(path)) return string.Empty;
            if (!path.StartsWith(ModelRoot + "/", StringComparison.OrdinalIgnoreCase) ||
                !Path.GetFileName(path).Equals("Animation", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Invalid Animation Folder",
                    "Choose a folder named Animation beneath Assets/Game/3D Models.", "OK");
                return string.Empty;
            }
            return path;
        }

        public static AnimationClip BrowseForClip()
        {
            string absolute = EditorUtility.OpenFilePanel(
                "Select FBX Animation File", Application.dataPath, "fbx");
            string path = ToAssetPath(absolute);
            if (string.IsNullOrEmpty(path)) return null;
            if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Invalid Animation File",
                    "Choose an imported .fbx file inside this project's Assets folder.", "OK");
                return null;
            }

            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                .FirstOrDefault(candidate =>
                    !candidate.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase));
            if (clip == null)
                EditorUtility.DisplayDialog("No Animation Clip Found",
                    $"'{Path.GetFileName(path)}' does not contain an imported AnimationClip. " +
                    "Check the model's Animation import settings.", "OK");
            return clip;
        }

        static string ToAssetPath(string absolute)
        {
            if (string.IsNullOrEmpty(absolute)) return string.Empty;
            absolute = absolute.Replace('\\', '/');
            string data = Application.dataPath.Replace('\\', '/');
            if (!absolute.StartsWith(data, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Outside Project", "Select a folder or file inside this project's Assets folder.", "OK");
                return string.Empty;
            }
            return "Assets" + absolute.Substring(data.Length);
        }
    }
}
#endif
