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
    const string PrefabDir = "Assets/Game/Game_Prefabs";

    [MenuItem("BCE/Combat/Create Class Ability Pools")]
    public static void CreateAllPools()
    {
        // Ensure output directory exists
        if (!AssetDatabase.IsValidFolder(OutputDir))
        {
            AssetDatabase.CreateFolder("Assets/Game/Data", "ClassPools");
            AssetDatabase.Refresh();
        }

        CreatePool("Warden",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,8,9,10,11,12,44,45,46 },
            defaultEquipped:  new[] { 0,8,9,11 });          // Sentinel, Snare, Hymn, Mend

        CreatePool("Ironclad",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,13,14,15,16,17,18,47,48,49 },
            defaultEquipped:  new[] { 7,15,16,18 });         // Shield, Charge, Stance, Rampart

        CreatePool("Arcanist",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,19,20,21,22,34,37,38,39,40,41,42,43 },
            defaultEquipped:  new[] { 37,38,20,22 });         // Conflagration Cone, Ember Beam, Void Maw, Collapsing Void

        CreatePool("Cleric",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,23,24,25,26,27,28,35,36,53,54,55 },
            defaultEquipped:  new[] { 35,36,26,28 });        // Healing Cone, Mending Beam, Sacred Aegis, Temporal Grace

        CreatePool("Shadowblade",
            availableIndices: new[] { 0,1,2,3,4,5,6,7,29,30,31,32,33,50,51,52 },
            defaultEquipped:  new[] { 33,32,31,29 });        // Fan of Blades, Dark Mark, Dark Harvest, Shadow Veil

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
