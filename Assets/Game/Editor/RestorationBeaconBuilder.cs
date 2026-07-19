#if UNITY_EDITOR
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/4r — Restoration Beacon Prefab Builder
///
/// Creates Assets/Game/Networking/RestorationBeacon.prefab with:
///   • NetworkIdentity (so Mirror spawns it on all clients)
///   • RestorationBeacon component (heals + lifetime logic)
///   • Earth staff FBX as a visual child (planted-in-ground pose)
///   • CapsuleCollider for physical presence
///   • idleVFX  ← Healing circle.prefab (persistent ground circle)
///   • pulseVFX ← Healing buff.prefab   (per-pulse burst every 3 s)
///
/// Then registers the prefab in:
///   • RodNetworkManager.worldPrefabs  (so clients learn about it)
///   • AbilityCaster.beaconPrefab on Cleric.prefab (so the spell casts it)
///
/// Finally force-reimports the prefab so Mirror writes a correct non-zero _assetId.
///
/// Run: BCE → Setup → 4r ▶ Restoration Beacon Builder
/// Required after: open Unity editor, run BCE/Setup/4v to confirm VFX wiring.
/// </summary>
public static class RestorationBeaconBuilder
{
    const string BeaconPrefabPath  = "Assets/Game/Networking/RestorationBeacon.prefab";
    const string ClericPrefabPath  = "Assets/Game/Game_Prefabs/Cleric.prefab";
    const string NetworkManagerPath = "Assets/Game/Networking/NetworkManager.prefab";

    // Visual — Earth staff (best thematic fit for a healing beacon planted in ground)
    const string EarthStaffPath =
        "Assets/brbMuff Folder/brbmuffins Lab/brbmuffins Staff Set/Models/Earth staff.fbx";

    // VFX
    const string MagicPackPfx = "Assets/brbMuff Folder/brbmuffins Studio/brbmuffins Magic Pack/Prefabs";
    static string HealingCircle => $"{MagicPackPfx}/Magic circles/Healing circle.prefab";
    static string HealingBuff   => "Assets/Game/Resources/FX/dark magic/Healing buff.prefab";

    [MenuItem("BCE/Setup/4r ▶ Restoration Beacon Builder", priority = 43)]
    static void Build()
    {
        var report = new StringBuilder();
        report.AppendLine("── Restoration Beacon Builder ────────────────────");

        // ── 1. Create or update the prefab ────────────────────────────────────
        var beaconPrefab = BuildPrefab(report);
        if (beaconPrefab == null)
        {
            EditorUtility.DisplayDialog("Restoration Beacon Builder", report.ToString(), "OK");
            return;
        }

        // ── 2. Register in RodNetworkManager.worldPrefabs ─────────────────────
        RegisterInNetworkManager(beaconPrefab, report);

        // ── 3. Assign to AbilityCaster.beaconPrefab on Cleric.prefab ──────────
        AssignToCleric(beaconPrefab, report);

        // ── 4. Also add to ClassPrefabFixer ExtraPrefabs is done by running 4z ─
        //       (the fixer already reimports; we force-reimport now for immediacy)
        AssetDatabase.ImportAsset(BeaconPrefabPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Read back assetId
        var reimported = AssetDatabase.LoadAssetAtPath<GameObject>(BeaconPrefabPath);
        var ni = reimported?.GetComponent<Mirror.NetworkIdentity>();
        uint aid = ni != null ? ni.assetId : 0;
        report.AppendLine($"  assetId after reimport: {aid}  {(aid != 0 ? "✅" : "❌ still 0 — run 4z")}");

        report.AppendLine();
        report.AppendLine("EDITOR STEPS REMAINING:");
        report.AppendLine("  1. In Hierarchy: assign VFX visually if desired (or run 4v)");
        report.AppendLine("  2. Add RestorationBeacon.prefab to NetworkManager spawnPrefabs list");
        report.AppendLine("     (RodNetworkManager.worldPrefabs inspector array — done above if prefab found)");
        report.AppendLine("  3. Ctrl+S, rebuild + redeploy server");

        EditorUtility.DisplayDialog("Restoration Beacon Builder", report.ToString(), "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 1. Build / update prefab asset
    // ─────────────────────────────────────────────────────────────────────────

    static GameObject BuildPrefab(StringBuilder r)
    {
        // Check if prefab already exists — update rather than recreate
        bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(BeaconPrefabPath) != null;

        // Create root
        var root = new GameObject("RestorationBeacon");

        // NetworkIdentity — required for Mirror to spawn it on all clients
        root.AddComponent<Mirror.NetworkIdentity>();

        // RestorationBeacon component — healing logic
        var beacon = root.AddComponent<RestorationBeacon>();
        beacon.healPerPulse  = 12f;
        beacon.pulseInterval = 3f;
        beacon.radius        = 8f;
        beacon.lifetime      = 30f;

        // CapsuleCollider — gives physical presence so player can stand near it
        var col = root.AddComponent<CapsuleCollider>();
        col.radius = 0.25f;
        col.height = 1.8f;
        col.isTrigger = false;

        // ── VFX pre-wire ──────────────────────────────────────────────────────
        var idleAsset  = AssetDatabase.LoadAssetAtPath<GameObject>(HealingCircle);
        var pulseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(HealingBuff);

        if (idleAsset  != null) { beacon.idleVFX  = idleAsset;  r.AppendLine($"  → idleVFX  = {idleAsset.name}"); }
        else r.AppendLine($"  ⚠ idleVFX not found: {HealingCircle}");

        if (pulseAsset != null) { beacon.pulseVFX = pulseAsset; r.AppendLine($"  → pulseVFX = {pulseAsset.name}"); }
        else r.AppendLine($"  ⚠ pulseVFX not found: {HealingBuff}");

        // ── Visual child: Earth staff ─────────────────────────────────────────
        AddStaffVisual(root, r);

        // ── Save as prefab ────────────────────────────────────────────────────
        string verb = exists ? "Updated" : "Created";
        var prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, BeaconPrefabPath);
        Object.DestroyImmediate(root);

        if (prefabAsset == null)
        {
            r.AppendLine($"  ✗ Failed to save prefab at {BeaconPrefabPath}");
            return null;
        }

        r.AppendLine($"  ✓ {verb} prefab: {BeaconPrefabPath}");
        return prefabAsset;
    }

    static void AddStaffVisual(GameObject root, StringBuilder r)
    {
        var staffMesh = AssetDatabase.LoadAssetAtPath<GameObject>(EarthStaffPath);
        if (staffMesh == null)
        {
            r.AppendLine($"  ⚠ Earth staff not found: {EarthStaffPath}");
            // Fallback: simple cylinder as placeholder
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = "StaffVisual_placeholder";
            cyl.transform.SetParent(root.transform);
            cyl.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            cyl.transform.localScale    = new Vector3(0.08f, 0.9f, 0.08f);
            Object.DestroyImmediate(cyl.GetComponent<CapsuleCollider>());
            r.AppendLine("  → Using cylinder placeholder for staff visual");
            return;
        }

        var staff = (GameObject)PrefabUtility.InstantiatePrefab(staffMesh);
        if (staff == null) staff = Object.Instantiate(staffMesh);

        staff.name = "StaffVisual";
        staff.transform.SetParent(root.transform);

        // Planted-in-ground pose: stand upright, butt of staff at ground level
        staff.transform.localPosition = new Vector3(0f, 0f, 0f);
        staff.transform.localRotation = Quaternion.identity;
        staff.transform.localScale    = new Vector3(1f, 1f, 1f);

        r.AppendLine($"  ✓ Staff visual: {staffMesh.name}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. Register in RodNetworkManager.worldPrefabs
    // ─────────────────────────────────────────────────────────────────────────

    static void RegisterInNetworkManager(GameObject beaconPrefab, StringBuilder r)
    {
        // Look for NetworkManager prefab first, then any instance in the project
        string[] guids = AssetDatabase.FindAssets("t:Prefab RodNetworkManager");
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets("t:Prefab NetworkManager");

        string nmPath = null;
        foreach (var g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go == null) continue;
            if (go.GetComponent<RodNetworkManager>() != null) { nmPath = p; break; }
        }

        if (nmPath == null)
        {
            r.AppendLine("  ⚠ RodNetworkManager prefab not found in project");
            r.AppendLine("    → Add RestorationBeacon.prefab to worldPrefabs manually in Inspector");
            return;
        }

        var nmGO = AssetDatabase.LoadAssetAtPath<GameObject>(nmPath);
        var nm   = nmGO.GetComponent<RodNetworkManager>();

        if (nm.worldPrefabs == null)
            nm.worldPrefabs = new GameObject[0];

        bool alreadyPresent = System.Array.Exists(nm.worldPrefabs, p => p == beaconPrefab);
        if (!alreadyPresent)
        {
            var list = nm.worldPrefabs.ToList();
            list.Add(beaconPrefab);
            nm.worldPrefabs = list.ToArray();
            EditorUtility.SetDirty(nmGO);
            r.AppendLine($"  ✓ Added to RodNetworkManager.worldPrefabs ({nmPath})");
        }
        else
        {
            r.AppendLine("  ✓ Already in RodNetworkManager.worldPrefabs — skipped");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. Assign beaconPrefab on Cleric.prefab's AbilityCaster
    // ─────────────────────────────────────────────────────────────────────────

    static void AssignToCleric(GameObject beaconPrefab, StringBuilder r)
    {
        var cleric = AssetDatabase.LoadAssetAtPath<GameObject>(ClericPrefabPath);
        if (cleric == null)
        {
            r.AppendLine($"  ⚠ Cleric.prefab not found: {ClericPrefabPath}");
            return;
        }

        var caster = cleric.GetComponent<AbilityCaster>();
        if (caster == null)
        {
            r.AppendLine("  ⚠ Cleric.prefab has no AbilityCaster component");
            return;
        }

        caster.beaconPrefab = beaconPrefab;
        EditorUtility.SetDirty(cleric);
        r.AppendLine("  ✓ AbilityCaster.beaconPrefab → RestorationBeacon.prefab");
    }

    [MenuItem("BCE/Setup/4r ▶ Restoration Beacon Builder", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
