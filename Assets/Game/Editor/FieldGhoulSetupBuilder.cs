#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// FieldGhoulSetupBuilder — BCE/Hub World/Place Field Ghoul NPCs
///
/// One click does everything:
///   1. Loads Enemy_Grunt.prefab as the base body
///   2. Strips EnemyController (combat brain), keeps Health / NavMeshAgent / NetworkIdentity
///   3. Adds FieldGhoulNPC (wander loop) + NetworkTransform (position sync)
///   4. Tints the capsule a sickly green-grey so players recognise it as passive
///   5. Saves FieldGhoul_NPC.prefab to Assets/Game/Prefabs/
///   6. Places 6 instances across the Hub at sensible starting positions
///   7. Registers the prefab in RodNetworkManager.worldPrefabs
///   8. Marks the scene dirty — just Ctrl+S and bake NavMesh
///
/// Re-running clears any existing ghoul instances and re-places from scratch.
/// Enemy_Grunt.prefab must exist first (BCE/Setup/4a).
/// </summary>
public static class FieldGhoulSetupBuilder
{
    const string PrefabDir = "Assets/Game/Prefabs";
    const string GruntPath = "Assets/Game/Prefabs/Enemy_Grunt.prefab";
    const string GhoulPath = "Assets/Game/Prefabs/FieldGhoul_NPC.prefab";
    const string MatPath   = "Assets/Game/Prefabs/FieldGhoul_Material.mat";

    // (name, world position, wander radius)
    // Positions are starting guesses — move them in the scene after running.
    static readonly (string name, Vector3 pos, float radius)[] Placements =
    {
        ("FieldGhoul_Gundab_A",   new Vector3(  8f, 0.05f,  55f), 10f),
        ("FieldGhoul_Gundab_B",   new Vector3(-12f, 0.05f,  60f),  8f),
        ("FieldGhoul_Gundab_C",   new Vector3( 18f, 0.05f,  50f), 12f),
        ("FieldGhoul_Grove_A",    new Vector3( 55f, 0.05f,  14f),  8f),
        ("FieldGhoul_AshPlains_A",new Vector3(-54f, 0.05f,   8f), 10f),
        ("FieldGhoul_HubApproach",new Vector3(  6f, 0.05f,  22f),  7f),
    };

    // ── Menu entry ────────────────────────────────────────────────────────────

    [MenuItem("BCE/Hub World/Place Field Ghoul NPCs")]
    public static void PlaceGhouls()
    {
        // ── 0. Guard ──────────────────────────────────────────────────────────
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.StartsWith("Hub"))
        {
            bool cont = EditorUtility.DisplayDialog("Wrong Scene",
                $"Active scene is '{scene.name}', not Hub.\n\nContinue anyway?",
                "Yes", "Cancel");
            if (!cont) return;
        }

        // ── 1. Build (or refresh) the FieldGhoul_NPC prefab ──────────────────
        var ghoulPrefab = BuildGhoulPrefab();
        if (ghoulPrefab == null)
        {
            EditorUtility.DisplayDialog("Missing Source",
                "Could not create FieldGhoul_NPC.prefab.\n\n" +
                "Run BCE/Setup/4a first to create Enemy_Grunt.prefab.", "OK");
            return;
        }

        // ── 2. Clear existing ghoul instances in scene ────────────────────────
        // Removes the whole GhoulNPCs root if it exists, plus any stray instances.
        var oldRoot = GameObject.Find("FieldGhoulNPCs");
        if (oldRoot != null) Object.DestroyImmediate(oldRoot);

        foreach (var stray in Object.FindObjectsByType<FieldGhoulNPC>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            Object.DestroyImmediate(stray.gameObject);

        // ── 3. Place instances ────────────────────────────────────────────────
        var ghoulRoot = new GameObject("FieldGhoulNPCs");
        int placed    = 0;

        foreach (var (name, pos, radius) in Placements)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(ghoulPrefab);
            instance.name = name;
            instance.transform.SetParent(ghoulRoot.transform, worldPositionStays: true);
            instance.transform.position = pos;
            instance.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            var npc = instance.GetComponent<FieldGhoulNPC>();
            if (npc != null)
            {
                npc.wanderRadius = radius;
                EditorUtility.SetDirty(instance);
            }

            placed++;
        }

        // ── 4. Register in RodNetworkManager.worldPrefabs ────────────────────
        RegisterWithNetworkManager(ghoulPrefab);

        // ── 5. Finish ─────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = ghoulRoot;

        EditorUtility.DisplayDialog("Field Ghouls Placed ✓",
            $"Placed {placed} Field Ghoul NPCs in scene '{scene.name}'.\n\n" +
            $"Prefab:  {GhoulPath}\n\n" +
            "NEXT STEPS:\n" +
            "1. Select 'FieldGhoulNPCs' in the Hierarchy\n" +
            "   → drag children to better positions on your terrain\n" +
            "2. Bake NavMesh: Window → AI → Navigation → Bake\n" +
            "   (ghouls won't move until NavMesh is baked)\n" +
            "3. Ctrl+S",
            "Done!");
    }

    [MenuItem("BCE/Hub World/Place Field Ghoul NPCs", true)]
    static bool Validate() => !Application.isPlaying;

    // ── Prefab factory ────────────────────────────────────────────────────────

    static GameObject BuildGhoulPrefab()
    {
        var gruntPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GruntPath);
        if (gruntPrefab == null)
        {
            Debug.LogWarning($"[GhoulNPC] Enemy_Grunt.prefab not found at {GruntPath}");
            return null;
        }

        // If the prefab already exists, ask whether to overwrite.
        var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GhoulPath);
        if (existingPrefab != null)
        {
            bool overwrite = EditorUtility.DisplayDialog("Prefab Exists",
                "FieldGhoul_NPC.prefab already exists.\nOverwrite it?",
                "Overwrite", "Keep Existing");
            if (!overwrite) return existingPrefab;
        }

        // ── Instantiate source, unpack prefab link, modify ───────────────────
        var tempGO = (GameObject)PrefabUtility.InstantiatePrefab(gruntPrefab);
        PrefabUtility.UnpackPrefabInstance(
            tempGO, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        tempGO.name = "FieldGhoul_NPC";

        // Remove combat brain; keep Health, NavMeshAgent, NetworkIdentity, Collider
        var ec = tempGO.GetComponent<EnemyController>();
        if (ec != null) Object.DestroyImmediate(ec);

        // Add wander brain
        if (tempGO.GetComponent<FieldGhoulNPC>() == null)
            tempGO.AddComponent<FieldGhoulNPC>();

        // Add position sync so clients see the NPC walking around
        if (tempGO.GetComponent<Mirror.NetworkTransform>() == null)
            tempGO.AddComponent<Mirror.NetworkTransform>();

        // ── Tune stats ────────────────────────────────────────────────────────
        var health = tempGO.GetComponent<Health>();
        if (health != null)
        {
            health.maxHealth     = 200f;
            health.currentHealth = 200f;
        }

        var agent = tempGO.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed           = 2f;
            agent.stoppingDistance = 0.5f;
            agent.angularSpeed    = 120f;
        }

        // ── Distinctive passive tint ──────────────────────────────────────────
        ApplyGhoulTint(tempGO);

        // ── Save prefab asset ─────────────────────────────────────────────────
        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempGO, GhoulPath);
        Object.DestroyImmediate(tempGO);
        AssetDatabase.Refresh();

        Debug.Log($"[GhoulNPC] Saved FieldGhoul_NPC.prefab → {GhoulPath}");
        return savedPrefab;
    }

    static void ApplyGhoulTint(GameObject go)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null || rend.sharedMaterial == null) return;

        // Reuse saved material asset if it exists (avoids duplicate assets on re-run)
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        if (mat == null)
        {
            mat = new Material(rend.sharedMaterial);
            AssetDatabase.CreateAsset(mat, MatPath);
        }

        // Sickly grey-green: clearly "not an enemy" at a glance
        mat.color = new Color(0.50f, 0.62f, 0.44f, 1f);
        EditorUtility.SetDirty(mat);

        rend.sharedMaterial = mat;
    }

    // ── NetworkManager wiring ─────────────────────────────────────────────────

    static void RegisterWithNetworkManager(GameObject ghoulPrefab)
    {
        var nm = Object.FindAnyObjectByType<RodNetworkManager>();
        if (nm == null)
        {
            Debug.LogWarning("[GhoulNPC] RodNetworkManager not found — add FieldGhoul_NPC to worldPrefabs manually.");
            return;
        }

        var so   = new SerializedObject(nm);
        var prop = so.FindProperty("worldPrefabs");
        if (prop == null)
        {
            Debug.LogWarning("[GhoulNPC] worldPrefabs property not found on RodNetworkManager.");
            return;
        }

        // Check if already registered
        for (int i = 0; i < prop.arraySize; i++)
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == ghoulPrefab)
                return;   // already in list

        prop.arraySize++;
        prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = ghoulPrefab;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(nm);
        Debug.Log("[GhoulNPC] FieldGhoul_NPC registered in RodNetworkManager.worldPrefabs.");
    }
}
#endif
