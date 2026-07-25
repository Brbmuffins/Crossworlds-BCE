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
///   0  Marauder    → 3D Models/Heroes/Marauder
///   1  Ironclad    → Heroes/Guardian/Guardian.fbx
///   2  Shadowblade → Heroes/Bogar/Bogar.fbx
///   3  Cleric      → Heroes/Brandalf/Brandalf.fbx                    (temp; Brandalf decision pending)
///   4  Arcanist    → Heroes/Dravos/fantasy_goblin_3d_model/Idle.fbx
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
    // (prefabPath, modelPath, controllerPath, label)
    // controllerPath = null → leave the model's existing Animator controller as-is

    static readonly (string prefabPath, string modelPath, string controllerPath, string label)[] Entries =
    {
        (
            "Assets/Game/Game_Prefabs/Marauder.prefab",
            "Assets/Game/3D Models/Heroes/Marauder/tripo_convert_6af764f8-667b-4b86-846d-7567dc4750ac.fbx",
            null,
            "Marauder"
        ),
        (
            "Assets/Game/Game_Prefabs/Ironclad.prefab",
            "Assets/Game/Heroes/Guardian/Guardian.fbx",
            null,
            "Ironclad"
        ),
        (
            "Assets/Game/Game_Prefabs/Shadowblade.prefab",
            "Assets/Game/Heroes/Bogar/Bogar.fbx",
            null,
            "Shadowblade"
        ),
        (
            "Assets/Game/Game_Prefabs/Cleric.prefab",
            "Assets/Game/Heroes/Brandalf/Brandalf.fbx",
            "Assets/Game/Heroes/Brandalf/Brandalf.controller",
            "Cleric"
        ),
        (
            "Assets/Game/Game_Prefabs/Arcanist.prefab",
            "Assets/Game/Heroes/Dravos/fantasy_goblin_3d_model/Idle.fbx",
            "Assets/Game/Heroes/Dravos/fantasy_goblin_3d_model/Dravos.controller",
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

        foreach (var (prefabPath, modelPath, controllerPath, label) in Entries)
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

            // Remove any existing Model child so we can replace it cleanly
            var existing = root.transform.Find("Model");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
                replaced++;
                Debug.Log($"[HeroModel] {label}: removed old Model child.");
            }

            // Instantiate FBX as a child — asset pipeline maintains the import link
            var modelGO = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, root.transform);
            if (modelGO == null)
                modelGO = Object.Instantiate(modelAsset, root.transform);

            modelGO.name = "Model";
            modelGO.transform.localPosition = Vector3.zero;
            modelGO.transform.localRotation = Quaternion.identity;
            modelGO.transform.localScale    = Vector3.one;

            // Assign AnimatorController if specified
            if (controllerPath != null)
            {
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller != null)
                {
                    var anim = modelGO.GetComponentInChildren<Animator>();
                    if (anim != null)
                    {
                        anim.runtimeAnimatorController = controller;
                        Debug.Log($"[HeroModel] {label}: assigned {System.IO.Path.GetFileName(controllerPath)} to Animator.");
                    }
                    else
                    {
                        Debug.LogWarning($"[HeroModel] {label}: no Animator found on model — controller not assigned.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[HeroModel] {label}: controller not found at {controllerPath}.");
                }
            }

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
