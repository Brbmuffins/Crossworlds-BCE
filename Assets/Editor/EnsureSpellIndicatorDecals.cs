#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
static class EnsureSpellIndicatorDecals
{
    static readonly string[] RendererAssetPaths =
    {
        "Assets/Settings/PC_Renderer.asset",
        "Assets/Settings/Mobile_Renderer.asset"
    };

    static EnsureSpellIndicatorDecals()
    {
        EditorApplication.delayCall += EnsureDecalRendererFeatures;
    }

    static void EnsureDecalRendererFeatures()
    {
        foreach (string path in RendererAssetPaths)
            EnsureDecalRendererFeature(path);
    }

    static void EnsureDecalRendererFeature(string rendererAssetPath)
    {
        ScriptableRendererData rendererData =
            AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(rendererAssetPath);
        if (rendererData == null)
            return;

        var serializedRenderer = new SerializedObject(rendererData);
        SerializedProperty features = serializedRenderer.FindProperty("m_RendererFeatures");
        SerializedProperty featureMap = serializedRenderer.FindProperty("m_RendererFeatureMap");
        if (features == null || featureMap == null)
            return;

        for (int i = 0; i < features.arraySize; i++)
        {
            if (features.GetArrayElementAtIndex(i).objectReferenceValue is DecalRendererFeature)
                return;
        }

        DecalRendererFeature decalFeature = ScriptableObject.CreateInstance<DecalRendererFeature>();
        decalFeature.name = "Decal";
        AssetDatabase.AddObjectToAsset(decalFeature, rendererData);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(decalFeature, out string _, out long localId);

        features.arraySize++;
        features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = decalFeature;

        featureMap.arraySize++;
        featureMap.GetArrayElementAtIndex(featureMap.arraySize - 1).longValue = localId;

        serializedRenderer.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(decalFeature);
        EditorUtility.SetDirty(rendererData);
        AssetDatabase.SaveAssetIfDirty(rendererData);
        AssetDatabase.ImportAsset(rendererAssetPath);
    }
}
#endif
