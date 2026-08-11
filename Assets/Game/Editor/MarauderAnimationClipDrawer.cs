#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MarauderAnimationClipAttribute))]
public sealed class MarauderAnimationClipDrawer : PropertyDrawer
{
    sealed class ClipCache
    {
        public string[] folders;
        public AnimationClip[] clips;
        public GUIContent[] options;
    }

    static readonly Dictionary<string, string[]> ClassAnimationFolders =
        new(System.StringComparer.OrdinalIgnoreCase)
        {
            {
                "Arcanist",
                new[]
                {
                    "Assets/Game/3D Models/Heroes/Dravos/" +
                    "fantasy_goblin_3d_model"
                }
            },
            {
                "Marauder",
                new[]
                {
                    "Assets/Game/3D Models/Heroes/Marauder/Animation"
                }
            },
            {
                "Ironclad",
                new[]
                {
                    "Assets/Game/3D Models/Heroes/Guardian"
                }
            },
            {
                "Shadowblade",
                new[]
                {
                    "Assets/Game/3D Models/Heroes/Bogar"
                }
            },
            {
                "Cleric",
                new[]
                {
                    "Assets/Game/3D Models/Heroes/Brandalf/" +
                    "Combat Animations"
                }
            },
            {
                "Necromancer",
                new[]
                {
                    "Assets/Game/3D Models/Heroes/Necromancer"
                }
            }
        };

    static readonly Dictionary<string, ClipCache> CachedByClass =
        new(System.StringComparer.OrdinalIgnoreCase);

    static MarauderAnimationClipDrawer()
    {
        EditorApplication.projectChanged += ClearCache;
    }

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        string className = ResolveClassName(property);
        ClipCache cache = GetCache(className);

        AnimationClip current = property.objectReferenceValue as AnimationClip;
        AnimationClip[] clips = cache.clips;
        GUIContent[] options = cache.options;

        int selectedIndex = current == null
            ? 0
            : System.Array.IndexOf(clips, current) + 1;

        // Preserve an existing cross-class or external selection rather than
        // silently clearing serialized spell data.
        if (current != null && selectedIndex == 0)
        {
            clips = cache.clips.Concat(new[] { current }).ToArray();
            options = cache.options.Concat(new[]
            {
                new GUIContent($"External / {current.name}")
            }).ToArray();
            selectedIndex = clips.Length;
        }

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();
        int nextIndex = EditorGUI.Popup(
            position,
            new GUIContent(
                string.IsNullOrEmpty(className)
                    ? "Cast Animation"
                    : $"{className} Animation",
                BuildTooltip(className, cache.folders)),
            selectedIndex,
            options);

        if (EditorGUI.EndChangeCheck())
            property.objectReferenceValue =
                nextIndex <= 0 ? null : clips[nextIndex - 1];

        EditorGUI.EndProperty();
    }

    static string ResolveClassName(SerializedProperty property)
    {
        Object target = property?.serializedObject?.targetObject;
        if (target == null) return "";

        string typeName = target.GetType().Name;
        const string suffix = "AbilityCaster";
        return typeName.EndsWith(
            suffix,
            System.StringComparison.OrdinalIgnoreCase)
            ? typeName.Substring(0, typeName.Length - suffix.Length)
            : "";
    }

    static ClipCache GetCache(string className)
    {
        string cacheKey = string.IsNullOrEmpty(className)
            ? "AbilityCaster"
            : className;
        if (CachedByClass.TryGetValue(cacheKey, out ClipCache cache))
            return cache;

        string[] configuredFolders =
            ClassAnimationFolders.TryGetValue(
                className ?? "", out string[] folders)
                ? folders
                : new[]
                {
                    $"Assets/Game/3D Models/Heroes/{className}"
                };
        string[] validFolders = configuredFolders
            .Where(AssetDatabase.IsValidFolder)
            .ToArray();

        var clips = new List<AnimationClip>();
        foreach (string folder in validFolders)
        {
            foreach (string guid in AssetDatabase.FindAssets(
                         "t:AnimationClip",
                         new[] { folder }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                clips.AddRange(
                    AssetDatabase.LoadAllAssetsAtPath(assetPath)
                        .OfType<AnimationClip>()
                        .Where(clip =>
                            clip != null &&
                            !clip.name.StartsWith("__preview__")));
            }
        }

        AnimationClip[] cachedClips = clips
            .Distinct()
            .OrderBy(clip => AssetDatabase.GetAssetPath(clip))
            .ThenBy(clip => clip.name)
            .ToArray();

        GUIContent[] cachedOptions =
            new GUIContent[cachedClips.Length + 1];
        cachedOptions[0] = new GUIContent("Category Default");

        for (int i = 0; i < cachedClips.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(cachedClips[i]);
            string filename = Path.GetFileNameWithoutExtension(path);
            string prefix =
                $"animation_{className?.ToLowerInvariant()}_";
            if (!string.IsNullOrEmpty(className) &&
                filename.StartsWith(
                    prefix,
                    System.StringComparison.OrdinalIgnoreCase))
                filename = filename.Substring(prefix.Length);

            cachedOptions[i + 1] =
                new GUIContent(ObjectNames.NicifyVariableName(filename));
        }

        cache = new ClipCache
        {
            folders = validFolders,
            clips = cachedClips,
            options = cachedOptions
        };
        CachedByClass[cacheKey] = cache;
        return cache;
    }

    static string BuildTooltip(string className, string[] folders)
    {
        string owner = string.IsNullOrEmpty(className)
            ? "the selected class"
            : className;
        if (folders == null || folders.Length == 0)
            return $"No animation folder is configured for {owner}. " +
                   "Category Default remains available.";

        return $"Animation clips available to {owner}, loaded from: " +
               string.Join(", ", folders);
    }

    static void ClearCache()
    {
        CachedByClass.Clear();
    }
}
#endif
