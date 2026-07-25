#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MarauderAnimationClipAttribute))]
public sealed class MarauderAnimationClipDrawer : PropertyDrawer
{
    const string AnimationFolder =
        "Assets/Game/3D Models/Heroes/Marauder/Animation";

    static AnimationClip[] cachedClips;
    static GUIContent[] cachedOptions;

    static MarauderAnimationClipDrawer()
    {
        EditorApplication.projectChanged += ClearCache;
    }

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        EnsureCache();

        AnimationClip current = property.objectReferenceValue as AnimationClip;
        AnimationClip[] clips = cachedClips;
        GUIContent[] options = cachedOptions;

        int selectedIndex = current == null
            ? 0
            : System.Array.IndexOf(clips, current) + 1;

        // Preserve a clip that was assigned before this drawer existed, even if
        // it does not live in the Marauder folder.
        if (current != null && selectedIndex == 0)
        {
            clips = cachedClips.Concat(new[] { current }).ToArray();
            options = cachedOptions.Concat(new[]
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
                "Marauder Animation",
                "Animation clips found in the Marauder/Animation folder."),
            selectedIndex,
            options);

        if (EditorGUI.EndChangeCheck())
            property.objectReferenceValue =
                nextIndex <= 0 ? null : clips[nextIndex - 1];

        EditorGUI.EndProperty();
    }

    static void EnsureCache()
    {
        if (cachedClips != null && cachedOptions != null)
            return;

        var clips = new List<AnimationClip>();
        foreach (string guid in AssetDatabase.FindAssets(
                     "t:AnimationClip",
                     new[] { AnimationFolder }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            clips.AddRange(
                AssetDatabase.LoadAllAssetsAtPath(assetPath)
                    .OfType<AnimationClip>()
                    .Where(clip =>
                        clip != null &&
                        !clip.name.StartsWith("__preview__")));
        }

        cachedClips = clips
            .Distinct()
            .OrderBy(clip => AssetDatabase.GetAssetPath(clip))
            .ThenBy(clip => clip.name)
            .ToArray();

        cachedOptions = new GUIContent[cachedClips.Length + 1];
        cachedOptions[0] = new GUIContent("Category Default");

        for (int i = 0; i < cachedClips.Length; i++)
        {
            string path = AssetDatabase.GetAssetPath(cachedClips[i]);
            string filename = Path.GetFileNameWithoutExtension(path);
            const string prefix = "animation_marauder_";
            if (filename.StartsWith(prefix))
                filename = filename.Substring(prefix.Length);

            cachedOptions[i + 1] =
                new GUIContent(ObjectNames.NicifyVariableName(filename));
        }
    }

    static void ClearCache()
    {
        cachedClips = null;
        cachedOptions = null;
    }
}
#endif
