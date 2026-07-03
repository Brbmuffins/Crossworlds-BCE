#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Mirror;

/// <summary>
/// ClassPoolBuilder — BCE/Setup menu for creating all 5 class player prefabs.
///
/// Menu: BCE/Setup/
///   4  ► Create Class Prefabs (5 Classes)   — creates new prefabs (skips existing)
///   4b ► Fix Existing Prefabs               — patches existing prefabs to use
///                                             CharacterController + PlayerController
///
/// Creates: Warden, Ironclad, Shadowblade, Cleric, Arcanist
///   - NetworkIdentity + NetworkTransformReliable
///   - PlayerIdentity (SyncVars: playerName, classIndex, characterId)
///   - AbilityCaster (class abilities wired by classIndex)
///   - Health (100 HP)
///   - CharacterController (replaces old kinematic Rigidbody — fixes floor fall-through)
///   - PlayerController (movement, gravity, Animator drive)
///   - Registered in NetworkManager.spawnPrefabs
/// </summary>
public class ClassPoolBuilder : Editor
{
    // ─── Class Definitions ────────────────────────────────────────────────────
    static readonly (int index, string name, Color color)[] CLASSES =
    {
        (0, "Warden",      new Color(0.29f, 0.56f, 0.89f)),
        (1, "Ironclad",    new Color(0.94f, 0.63f, 0.13f)),
        (2, "Shadowblade", new Color(0.61f, 0.35f, 0.71f)),
        (3, "Cleric",      new Color(0.18f, 0.80f, 0.44f)),
        (4, "Arcanist",    new Color(0.91f, 0.30f, 0.24f)),
    };

    const string PREFAB_DIR = "Assets/Game/Prefabs";

    // ─── Menu: Create ─────────────────────────────────────────────────────────
    [MenuItem("BCE/Setup/4 ▶ Create Class Prefabs (5 Classes)")]
    static void CreateClassPrefabs()
    {
        System.IO.Directory.CreateDirectory(
            Application.dataPath + "/../" + PREFAB_DIR.Substring("Assets/".Length));
        AssetDatabase.Refresh();

        var nm = FindFirstObjectByType<NetworkManager>();

        int created = 0;
        foreach (var (idx, className, col) in CLASSES)
        {
            string path = $"{PREFAB_DIR}/{className}.prefab";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log($"[ClassPool] {className}.prefab already exists — run 4b to patch");
                continue;
            }

            var go     = BuildClassObject(idx, className, col);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            if (nm != null && prefab != null && !nm.spawnPrefabs.Contains(prefab))
                nm.spawnPrefabs.Add(prefab);

            Debug.Log($"[ClassPool] Created {path}");
            created++;
        }

        if (nm != null)
            EditorUtility.SetDirty(nm);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Class Prefabs",
            $"{created} prefab(s) created.\n\n" +
            "Existing prefabs were skipped — run BCE/Setup/4b to patch them with " +
            "CharacterController + PlayerController.\n\n" +
            "Next: BCE/Setup/4a–4d for enemy + WorldItem prefabs.",
            "OK");
    }

    // ─── Menu: Fix Existing Prefabs ───────────────────────────────────────────
    /// <summary>
    /// Patches any class prefabs that still have kinematic Rigidbody physics.
    /// Replaces with CharacterController + PlayerController.
    /// Safe to run multiple times — skips components already present.
    /// </summary>
    [MenuItem("BCE/Setup/4 ▶ Fix Player Prefabs (CharacterController + PlayerController)")]
    static void FixExistingPrefabs()
    {
        int patched = 0;
        int skipped = 0;

        foreach (var (_, className, _) in CLASSES)
        {
            string path   = $"{PREFAB_DIR}/{className}.prefab";
            var    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.Log($"[ClassPool] {className}.prefab not found — skipping (run menu 4 first)");
                skipped++;
                continue;
            }

            // Open prefab for editing
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            var    contents   = PrefabUtility.LoadPrefabContents(prefabPath);

            bool changed = false;

            // ── Remove kinematic Rigidbody if present ─────────────────────────
            var rb = contents.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Object.DestroyImmediate(rb);
                Debug.Log($"[ClassPool] {className}: removed Rigidbody");
                changed = true;
            }

            // ── Remove standalone CapsuleCollider (CC has its own) ────────────
            // Keep it if AbilityCaster/Health depends on it — but CC replaces it for physics
            var oldCol = contents.GetComponent<CapsuleCollider>();
            if (oldCol != null)
            {
                Object.DestroyImmediate(oldCol);
                Debug.Log($"[ClassPool] {className}: removed CapsuleCollider (CC handles this)");
                changed = true;
            }

            // ── Add CharacterController if missing ────────────────────────────
            if (contents.GetComponent<CharacterController>() == null)
            {
                var cc    = contents.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.4f;
                cc.center = new Vector3(0f, 1f, 0f);
                Debug.Log($"[ClassPool] {className}: added CharacterController");
                changed = true;
            }

            // ── Add PlayerController if missing ───────────────────────────────
            var pcType = System.Type.GetType("PlayerController");
            if (pcType != null && contents.GetComponent(pcType) == null)
            {
                contents.AddComponent(pcType);
                Debug.Log($"[ClassPool] {className}: added PlayerController");
                changed = true;
            }
            else if (pcType == null)
            {
                Debug.LogWarning($"[ClassPool] PlayerController type not found — " +
                                 "ensure PlayerController.cs is in the project and compiled");
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                patched++;
            }
            else
            {
                skipped++;
                Debug.Log($"[ClassPool] {className}: already up to date");
            }

            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Fix Prefabs",
            $"Patched: {patched}  |  Already up to date: {skipped}\n\n" +
            "Each prefab now uses CharacterController for movement and gravity.\n" +
            "Add an Animator to any prefab to enable walk animations (Speed param).",
            "OK");
    }

    // ─── Prefab Construction ──────────────────────────────────────────────────
    static GameObject BuildClassObject(int classIndex, string className, Color col)
    {
        var go = new GameObject(className);

        // ── Visuals (capsule placeholder) ────────────────────────────────────
        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.SetParent(go.transform);
        capsule.transform.localPosition = Vector3.zero;
        capsule.name = "Model";
        Object.DestroyImmediate(capsule.GetComponent<CapsuleCollider>());

        var mr = capsule.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = col;
            mr.sharedMaterial = mat;
        }

        // ── Physics — CharacterController (not Rigidbody) ────────────────────
        // CharacterController handles gravity integration and slope/step physics.
        // PlayerController applies gravity manually each frame via CC.Move().
        var cc    = go.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.4f;
        cc.center = new Vector3(0f, 1f, 0f);

        go.tag = "Player";

        // ── Mirror ───────────────────────────────────────────────────────────
        go.AddComponent<NetworkIdentity>();

        var nt = go.AddComponent<NetworkTransformReliable>();
        nt.syncPosition = true;
        nt.syncRotation = true;
        nt.syncScale    = false;

        // ── Game components ───────────────────────────────────────────────────
        var health = go.AddComponent<Health>();
        health.maxHp = 100f;

        var piType = System.Type.GetType("PlayerIdentity");
        if (piType != null)
            go.AddComponent(piType);

        var acType = System.Type.GetType("AbilityCaster");
        if (acType != null)
            go.AddComponent(acType);

        go.AddComponent<HeroCosmeticApplier>();

        // ── Movement ─────────────────────────────────────────────────────────
        var pcType = System.Type.GetType("PlayerController");
        if (pcType != null)
            go.AddComponent(pcType);

        return go;
    }

    // ─── Validation ───────────────────────────────────────────────────────────
    [MenuItem("BCE/Setup/4 ▶ Create Class Prefabs (5 Classes)", true)]
    static bool ValidateCreate() => !Application.isPlaying;

    [MenuItem("BCE/Setup/4 ▶ Fix Player Prefabs (CharacterController + PlayerController)", true)]
    static bool ValidateFix() => !Application.isPlaying;
}
#endif
