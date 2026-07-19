#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/4g — Ice Guardian Turret Prefab
///
/// Builds Assets/Game/Networking/IceGuardianTurret.prefab — a fully networked
/// deployable turret for the Warden's "Runic Sentinel" ability.
///
/// Components added to the prefab root:
///   • Rigidbody           (kinematic, no gravity — required by GuardianFollower)
///   • CapsuleCollider     (so IgnoreOwnerCollisions can Physics.IgnoreCollision it)
///   • NetworkIdentity     (Mirror spawn — makes it visible on all clients)
///   • NetworkTransformReliable (syncs position as guardian floats with player)
///   • Health              (maxHealth=80, isPlayer=false — so it can be killed)
///   • TurretController    (range=10, fireRate=1.8/s, damage=12, tag="Enemy")
///   • GuardianFollower    (follows owner's left-rear slot, teleports if too far)
///
/// Visual children:
///   • FrostGuardian.fbx mesh (Tripo-generated guardian model)
///   • IceGuardian.prefab VFX (ice particle aura — plays on awake)
///
/// After running:
///   1. BCE → Setup → 4z  (write _assetId)
///   2. BCE → Heroes → Assign Ability VFX  (points Runic Sentinel turretPrefab here)
///   3. Rebuild + redeploy server
/// </summary>
public static class IceGuardianTurretBuilder
{
    const string OutPath      = "Assets/Game/Networking/IceGuardianTurret.prefab";
    const string FrostFbx     = "Assets/Game/3D Models/Guardians/Frost Guardian/" +
                                 "tripo_convert_95c0180f-284f-4922-9040-945ada9c46ce.fbx";
    const string IceVFXPath   = "Assets/Game/Game_Prefabs/IceGuardian.prefab";

    [MenuItem("BCE/Setup/4g ▶ Ice Guardian Turret Prefab", priority = 44)]
    static void Run()
    {
        var report = new StringBuilder();
        report.AppendLine("── Ice Guardian Turret Builder ──────────────────────");

        // ── 1. Build prefab contents in scene ────────────────────────────────────
        var root = new GameObject("IceGuardianTurret");

        // Rigidbody first — GuardianFollower's MovePosition + IgnoreCollision needs it
        var rb            = root.AddComponent<Rigidbody>();
        rb.isKinematic    = true;
        rb.useGravity     = false;
        report.AppendLine("  ✓ Rigidbody (kinematic, no gravity)");

        // CapsuleCollider — sized to the guardian
        var col       = root.AddComponent<CapsuleCollider>();
        col.height    = 1.8f;
        col.radius    = 0.35f;
        col.center    = new Vector3(0f, 0.9f, 0f);
        col.isTrigger = false;
        report.AppendLine("  ✓ CapsuleCollider");

        // NetworkIdentity — required for Mirror to spawn on clients
        var netId = root.AddComponent<Mirror.NetworkIdentity>();
        report.AppendLine("  ✓ NetworkIdentity");

        // NetworkTransformReliable — syncs floating position to clients
        root.AddComponent<Mirror.NetworkTransformReliable>();
        report.AppendLine("  ✓ NetworkTransformReliable");

        // Health — so the guardian can be damaged / killed
        var health           = root.AddComponent<Health>();
        health.maxHealth     = 80f;
        health.currentHealth = 80f;
        health.isPlayer      = false;
        report.AppendLine("  ✓ Health (maxHealth=80)");

        // TurretController — targeting + firing logic
        var turret             = root.AddComponent<TurretController>();
        turret.range           = 10f;
        turret.fireRate        = 1.8f;
        turret.damage          = 12f;
        turret.targetTag       = "Enemy";
        turret.retargetInterval = 0.2f;
        turret.recoilDistance  = 0f;        // floating guardian — no barrel recoil
        report.AppendLine("  ✓ TurretController (range=10, rate=1.8/s, dmg=12)");

        // GuardianFollower — hovers at owner's left-rear slot
        var gf                      = root.AddComponent<GuardianFollower>();
        gf.followOwner              = true;
        gf.followOffset             = new Vector3(-1.6f, 0.8f, -1.4f);  // left-rear, slightly above ground
        gf.offsetUsesTargetRotation = true;
        gf.keepRigidbodyKinematic   = true;
        gf.disableGravity           = true;
        gf.ignoreOwnerCollisions    = true;
        gf.moveSpeed                = 5f;
        gf.catchUpDistance          = 6f;
        gf.catchUpSpeed             = 10f;
        gf.teleportDistance         = 24f;
        gf.snapToGround             = false;    // floating guardian — don't ground-snap
        gf.rotateTowardMovement     = false;    // TurretController handles rotation
        report.AppendLine("  ✓ GuardianFollower (left-rear float slot, teleport@24m)");

        // ── 2. FrostGuardian mesh child ───────────────────────────────────────────
        var frostFbx = AssetDatabase.LoadAssetAtPath<GameObject>(FrostFbx);
        if (frostFbx != null)
        {
            var mesh = (GameObject)PrefabUtility.InstantiatePrefab(frostFbx, root.transform);
            mesh.name = "GuardianMesh";
            mesh.transform.localPosition = Vector3.zero;
            mesh.transform.localScale    = Vector3.one * 0.6f;   // scale to match player
            mesh.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            report.AppendLine("  ✓ FrostGuardian mesh child");
        }
        else
        {
            // Fallback: blue sphere placeholder
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "GuardianMeshPlaceholder";
            sphere.transform.SetParent(root.transform);
            sphere.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            sphere.transform.localScale    = new Vector3(0.6f, 0.8f, 0.6f);
            var rend = sphere.GetComponent<Renderer>();
            if (rend != null)
                rend.sharedMaterial = new Material(Shader.Find("Standard"))
                    { color = new Color(0.4f, 0.7f, 1f) };
            report.AppendLine($"  ⚠ FrostGuardian mesh not found at:\n    {FrostFbx}\n    Using sphere placeholder.");
        }

        // ── 3. IceGuardian VFX child (ice aura particle — plays on awake) ────────
        var iceFX = AssetDatabase.LoadAssetAtPath<GameObject>(IceVFXPath);
        if (iceFX != null)
        {
            var vfx = (GameObject)PrefabUtility.InstantiatePrefab(iceFX, root.transform);
            vfx.name = "IceAura";
            vfx.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            vfx.transform.localScale    = Vector3.one * 0.4f;
            report.AppendLine("  ✓ IceGuardian VFX aura child");
        }
        else
        {
            report.AppendLine($"  ⚠ IceGuardian VFX not found at: {IceVFXPath}");
        }

        // ── 4. Save as prefab ─────────────────────────────────────────────────────
        bool replaced = System.IO.File.Exists(
            System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), OutPath));

        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, OutPath, out bool success);
        Object.DestroyImmediate(root);

        if (!success || savedPrefab == null)
        {
            report.AppendLine($"  ✗ SaveAsPrefabAsset FAILED → {OutPath}");
            EditorUtility.DisplayDialog("Ice Guardian Turret Builder", report.ToString(), "OK");
            return;
        }
        report.AppendLine(replaced
            ? $"  ✓ Prefab updated → {OutPath}"
            : $"  ✓ Prefab created → {OutPath}");

        // ── 5. Force-reimport for _assetId ────────────────────────────────────────
        AssetDatabase.ImportAsset(OutPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var reimported = AssetDatabase.LoadAssetAtPath<GameObject>(OutPath);
        var netId2     = reimported?.GetComponent<Mirror.NetworkIdentity>();
        uint assetId   = netId2 != null ? netId2.assetId : 0;
        report.AppendLine($"  assetId after reimport: {assetId}  {(assetId != 0 ? "✅" : "❌ run BCE/Setup/4z")}");

        // ── 6. Register in RodNetworkManager.worldPrefabs ─────────────────────────
        RegisterInNetworkManager(reimported, report);

        report.AppendLine();
        report.AppendLine("NEXT STEPS:");
        report.AppendLine("  1. If assetId=0 → run BCE/Setup/4z");
        report.AppendLine("  2. BCE → Heroes → Assign Ability VFX");
        report.AppendLine("     (routes Runic Sentinel's turretPrefab here)");
        report.AppendLine("  3. Rebuild + redeploy server");

        EditorUtility.DisplayDialog("Ice Guardian Turret Builder", report.ToString(), "Done");
    }

    static void RegisterInNetworkManager(GameObject prefab, StringBuilder r)
    {
        if (prefab == null) return;

        string[] guids = AssetDatabase.FindAssets("t:Prefab RodNetworkManager");
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets("t:Prefab NetworkManager");

        string nmPath = null;
        foreach (var g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            var go   = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go?.GetComponent<RodNetworkManager>() != null) { nmPath = p; break; }
        }

        if (nmPath == null)
        {
            r.AppendLine("  ⚠ RodNetworkManager prefab not found — add IceGuardianTurret to worldPrefabs manually");
            return;
        }

        var nmGO = AssetDatabase.LoadAssetAtPath<GameObject>(nmPath);
        var nm   = nmGO.GetComponent<RodNetworkManager>();

        nm.worldPrefabs = nm.worldPrefabs ?? new GameObject[0];
        bool already    = System.Array.Exists(nm.worldPrefabs, p => p == prefab);
        if (!already)
        {
            var list        = new System.Collections.Generic.List<GameObject>(nm.worldPrefabs) { prefab };
            nm.worldPrefabs = list.ToArray();
            EditorUtility.SetDirty(nmGO);
            AssetDatabase.SaveAssets();
            r.AppendLine("  ✓ Registered in RodNetworkManager.worldPrefabs");
        }
        else
        {
            r.AppendLine("  ✓ Already in RodNetworkManager.worldPrefabs");
        }
    }

    [MenuItem("BCE/Setup/4g ▶ Ice Guardian Turret Prefab", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
