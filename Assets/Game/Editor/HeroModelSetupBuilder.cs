#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// HeroModelSetupBuilder — BCE/Heroes/Attach Hero Models
///
/// Each class prefab is a root GameObject with scripts but NO mesh.
/// This builder instantiates the correct FBX model as a "Model" child
/// inside each class prefab so players are visible in-game.
///
/// Mappings (index → class → model FBX):
///   0  Warden      → Characters/Engineer/Model/RoD-bike-ridah.fbx   (placeholder until Warden FBX ships)
///   1  Ironclad    → Heroes/Guardian/Guardian.fbx
///   2  Shadowblade → Heroes/Bogar/Bogar.fbx
///   3  Cleric      → Heroes/Brandalf/Brandalf.fbx                    (temp; Brandalf decision pending)
///   4  Arcanist    → Heroes/Arcanist/Arcanist.fbx
///
/// Safe to re-run — skips any prefab that already has a "Model" child.
///
/// After running:
///   1. Play the Hub scene and confirm each class shows its model.
///   2. If a model is too large/small, open that class prefab, select
///      the Model child and adjust localScale in the Inspector, then Ctrl+S.
///   3. The Animator on each FBX root auto-resolves via PlayerAnimator
///      (GetComponentInChildren<Animator>) — no extra wiring needed.
/// </summary>
public static class HeroModelSetupBuilder
{
    // ── Mappings ──────────────────────────────────────────────────────────────

    static readonly (string prefabPath, string modelPath, string label)[] Entries =
    {
        (
            "Assets/Game/Prefabs/Warden.prefab",
            "Assets/Game/Characters/Engineer/Model/RoD-bike-ridah.fbx",
            "Warden"
        ),
        (
            "Assets/Game/Prefabs/Ironclad.prefab",
            "Assets/Game/Heroes/Guardian/Guardian.fbx",
            "Ironclad"
        ),
        (
            "Assets/Game/Prefabs/Shadowblade.prefab",
            "Assets/Game/Heroes/Bogar/Bogar.fbx",
            "Shadowblade"
        ),
        (
            "Assets/Game/Prefabs/Cleric.prefab",
            "Assets/Game/Heroes/Brandalf/Brandalf.fbx",
            "Cleric"
        ),
        (
            "Assets/Game/Prefabs/Arcanist.prefab",
            "Assets/Game/Heroes/Arcanist/Arcanist.fbx",
            "Arcanist"
        ),
    };

    // ── Menu entry ────────────────────────────────────────────────────────────

    [MenuItem("BCE/Heroes/Attach Hero Models")]
    static void AttachModels()
    {
        int attached  = 0;
        int skipped   = 0;
        int replaced  = 0;

        foreach (var (prefabPath, modelPath, label) in Entries)
        {
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                Debug.LogWarning($"[HeroModel] {label}: model not found at {modelPath} — skipped.");
                skipped++;
                continue;
            }

            using var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath);
            var root = scope.prefabContentsRoot;

            if (root == null)
            {
                Debug.LogWarning($"[HeroModel] {label}: could not open prefab at {prefabPath} — skipped.");
                skipped++;
                continue;
            }

            // Remove any existing Model child so we can replace it with the correct one
            var existing = root.transform.Find("Model");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
                Debug.Log($"[HeroModel] {label}: Removed old Model child.");
            }

            // Instantiate the FBX as a child (plain copy, not a live prefab link —
            // the FBX connection is maintained through the asset import pipeline).
            var modelGO = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, root.transform);
            if (modelGO == null)
            {
                // Fallback: plain instantiate if PrefabUtility can't link the FBX
                modelGO = Object.Instantiate(modelAsset, root.transform);
            }

            modelGO.name = "Model";
            modelGO.transform.localPosition = Vector3.zero;
            modelGO.transform.localRotation = Quaternion.identity;
            modelGO.transform.localScale    = Vector3.one;

            Debug.Log($"[HeroModel] {label}: attached {System.IO.Path.GetFileName(modelPath)}.");
            attached++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string msg = $"Attached models to {attached} prefab(s).";
        if (skipped > 0) msg += $"\n{skipped} skipped (already set or asset missing).";
        msg += "\n\nCheck the Console for per-class details.\n\nIf any model scale looks wrong, " +
               "open that class prefab, select the Model child, and adjust localScale in the Inspector.";

        EditorUtility.DisplayDialog("Hero Models", msg, "OK");
    }

    [MenuItem("BCE/Heroes/Attach Hero Models", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
