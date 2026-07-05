#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// ClassPoolBuilder — BCE/Combat/Create Class Ability Pools
/// Creates 5 ClassAbilityPool ScriptableObject assets in Assets/Game/Data/ClassPools/
/// and assigns them to each class prefab's AbilityCaster.classPool field.
/// Run once per project; safe to re-run (AssetDatabase.CreateAsset overwrites).
/// </summary>
public static class ClassPoolBuilder
{
    const string OutputDir = "Assets/Game/Data/ClassPools";
    const string PrefabDir = "Assets/Game/Prefabs";

    [MenuItem("BCE/Combat/Create Class Ability Pools")]
    static void CreateAllPools()
    {
        // Ensure output directory exists
        if (!AssetDatabase.IsValidFolder(OutputDir))
        {
            AssetDatabase.CreateFolder("Assets/Game/Data", "ClassPools");
            AssetDatabase.Refresh();
        }

        CreatePool("Warden",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,8,9,10,11,12 },
            defaultEquipped:  new[] { 0,8,9,11 });          // Sentinel, Snare, Hymn, Mend

        CreatePool("Ironclad",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,13,14,15,16,17,18 },
            defaultEquipped:  new[] { 7,15,16,18 });         // Shield, Charge, Stance, Rampart

        CreatePool("Arcanist",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,19,20,21,22,34 },  // +34 Ether Lance
            defaultEquipped:  new[] { 19,20,21,22 });        // Step, Maw, Lightning, Void

        CreatePool("Cleric",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,23,24,25,26,27,28 },
            defaultEquipped:  new[] { 24,23,26,28 });        // Wisps, Bond, Aegis, Grace

        CreatePool("Shadowblade",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,29,30,31,32,33 },  // +32 Dark Mark, +33 Fan of Blades
            defaultEquipped:  new[] { 1,30,31,29 });         // VoidBolt, Silence, Harvest, Veil

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Wire to prefabs
        AssignToPrefabs();

        Debug.Log("[ClassPoolBuilder] ✓ All 5 ClassAbilityPool assets created in " + OutputDir);
    }

    static void CreatePool(string className, int[] availableIndices, int[] defaultEquipped)
    {
        string path = $"{OutputDir}/{className}_Pool.asset";

        // Load existing so we don't break any prefab references that are already set
        var existing = AssetDatabase.LoadAssetAtPath<ClassAbilityPool>(path);
        ClassAbilityPool pool;
        if (existing != null)
        {
            pool = existing;
        }
        else
        {
            pool = ScriptableObject.CreateInstance<ClassAbilityPool>();
            AssetDatabase.CreateAsset(pool, path);
        }

        pool.className        = className;
        pool.availableIndices = availableIndices;
        pool.defaultEquipped  = defaultEquipped;
        EditorUtility.SetDirty(pool);

        Debug.Log($"[ClassPoolBuilder] {className}_Pool.asset → {path}");
    }

    static void AssignToPrefabs()
    {
        string[] classNames = { "Warden", "Ironclad", "Arcanist", "Cleric", "Shadowblade" };

        foreach (string cls in classNames)
        {
            string poolPath   = $"{OutputDir}/{cls}_Pool.asset";
            string prefabPath = $"{PrefabDir}/{cls}.prefab";

            var pool = AssetDatabase.LoadAssetAtPath<ClassAbilityPool>(poolPath);
            if (pool == null) { Debug.LogWarning($"[ClassPoolBuilder] Pool not found: {poolPath}"); continue; }

            var prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabObj == null)
            {
                Debug.LogWarning($"[ClassPoolBuilder] Prefab not found: {prefabPath} — assign {cls}_Pool.asset manually.");
                continue;
            }

            // Edit the prefab asset in place
            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                var caster = scope.prefabContentsRoot.GetComponent<AbilityCaster>();
                if (caster == null)
                    caster = scope.prefabContentsRoot.GetComponentInChildren<AbilityCaster>();

                if (caster == null)
                {
                    Debug.LogWarning($"[ClassPoolBuilder] No AbilityCaster found on {prefabPath} — assign manually.");
                    continue;
                }

                caster.classPool = pool;
                Debug.Log($"[ClassPoolBuilder] Assigned {cls}_Pool → {prefabPath}:AbilityCaster");
            }
        }
    }
}
#endif
