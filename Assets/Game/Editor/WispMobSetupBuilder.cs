#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/4w — Wisp Mob Combat Setup
///
/// Patches Assets/Game/Game_Prefabs/Muffin Junk/Wisp_Mob.prefab with the
/// components needed for it to be a combat mob in the WaveSpawner:
///
///   • Health              (maxHealth=20, isPlayer=false)
///   • WispCombat          (floating aggro + contact/pulse damage, server-auth)
///   • NetworkIdentity     (Mirror spawning on all clients)
///   • NetworkTransformReliable (syncs server-driven float position to clients)
///
/// Existing WispMob, SphereCollider(trigger), MeshRenderer, TrailRenderer,
/// and GlowLight child are preserved.
///
/// Also registers the wisp prefab in:
///   • RodNetworkManager.worldPrefabs (client spawn registry)
///   • WaveSpawner.wispPrefab in ZoneSceneSetupBuilder defaults
///
/// After running:
///   1. Run BCE/Setup/4z to write correct _assetId
///   2. Add WaveSpawner.wispPrefab = Wisp_Mob in every zone scene Inspector
///   3. Rebuild + redeploy server
/// </summary>
public static class WispMobSetupBuilder
{
    const string WispPrefabPath = "Assets/Game/Game_Prefabs/Muffin Junk/Wisp_Mob.prefab";

    [MenuItem("BCE/Setup/4w ▶ Wisp Mob Combat Setup", priority = 44)]
    static void Run()
    {
        var report = new StringBuilder();
        report.AppendLine("── Wisp Mob Combat Setup ─────────────────────────");

        var wisp = AssetDatabase.LoadAssetAtPath<GameObject>(WispPrefabPath);
        if (wisp == null)
        {
            report.AppendLine($"  ✗ Prefab not found: {WispPrefabPath}");
            EditorUtility.DisplayDialog("Wisp Mob Combat Setup", report.ToString(), "OK");
            return;
        }

        bool dirty = false;

        // ── 1. Tag ────────────────────────────────────────────────────────────────
        if (wisp.tag != "Enemy")
        {
            wisp.tag = "Enemy";
            dirty = true;
            report.AppendLine("  ✓ tag → Enemy");
        }
        else report.AppendLine("  ✓ tag: Enemy (already set)");

        // ── 2. Health ─────────────────────────────────────────────────────────────
        var health = wisp.GetComponent<Health>();
        if (health == null)
        {
            health = wisp.AddComponent<Health>();
            dirty  = true;
            report.AppendLine("  ✓ Health added");
        }
        health.maxHealth     = 20f;
        health.currentHealth = 20f;
        health.isPlayer      = false;
        dirty = true;
        report.AppendLine("  ✓ Health: maxHealth=20, isPlayer=false");

        // ── 3. WispCombat ─────────────────────────────────────────────────────────
        var combat = wisp.GetComponent<WispCombat>();
        if (combat == null)
        {
            combat = wisp.AddComponent<WispCombat>();
            dirty  = true;
            report.AppendLine("  ✓ WispCombat added");
        }
        else report.AppendLine("  ✓ WispCombat: already present");

        // ── 4. NetworkIdentity ────────────────────────────────────────────────────
        var netId = wisp.GetComponent<Mirror.NetworkIdentity>();
        if (netId == null)
        {
            netId  = wisp.AddComponent<Mirror.NetworkIdentity>();
            dirty  = true;
            report.AppendLine("  ✓ NetworkIdentity added");
        }
        else report.AppendLine("  ✓ NetworkIdentity: already present");

        // ── 5. NetworkTransformReliable ───────────────────────────────────────────
        var netTransform = wisp.GetComponent<Mirror.NetworkTransformReliable>();
        if (netTransform == null)
        {
            // Try the simpler NetworkTransform base if Reliable isn't available
            netTransform = wisp.AddComponent<Mirror.NetworkTransformReliable>();
            dirty = true;
            report.AppendLine("  ✓ NetworkTransformReliable added (syncs float pos to clients)");
        }
        else report.AppendLine("  ✓ NetworkTransformReliable: already present");

        // ── 6. Save + force-reimport for _assetId ─────────────────────────────────
        if (dirty) EditorUtility.SetDirty(wisp);
        AssetDatabase.SaveAssets();

        AssetDatabase.ImportAsset(WispPrefabPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        var reimported = AssetDatabase.LoadAssetAtPath<GameObject>(WispPrefabPath);
        var ni2  = reimported?.GetComponent<Mirror.NetworkIdentity>();
        uint aid = ni2 != null ? ni2.assetId : 0;
        report.AppendLine($"  assetId after reimport: {aid}  {(aid != 0 ? "✅" : "❌ run BCE/Setup/4z")}");

        // ── 7. Register in RodNetworkManager.worldPrefabs ─────────────────────────
        RegisterInNetworkManager(reimported, report);

        report.AppendLine();
        report.AppendLine("REQUIRED STEPS:");
        report.AppendLine("  1. If assetId=0 above → run BCE/Setup/4z");
        report.AppendLine("  2. Assign Wisp_Mob to WaveSpawner.wispPrefab in each zone scene");
        report.AppendLine("  3. Rebuild + redeploy server");

        EditorUtility.DisplayDialog("Wisp Mob Combat Setup", report.ToString(), "OK");
    }

    static void RegisterInNetworkManager(GameObject wispPrefab, StringBuilder r)
    {
        if (wispPrefab == null) return;

        string[] guids = AssetDatabase.FindAssets("t:Prefab RodNetworkManager");
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets("t:Prefab NetworkManager");

        string nmPath = null;
        foreach (var g in guids)
        {
            string p  = AssetDatabase.GUIDToAssetPath(g);
            var go    = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go?.GetComponent<RodNetworkManager>() != null) { nmPath = p; break; }
        }

        if (nmPath == null)
        {
            r.AppendLine("  ⚠ RodNetworkManager prefab not found — add Wisp_Mob to worldPrefabs manually");
            return;
        }

        var nmGO = AssetDatabase.LoadAssetAtPath<GameObject>(nmPath);
        var nm   = nmGO.GetComponent<RodNetworkManager>();

        nm.worldPrefabs = nm.worldPrefabs ?? new GameObject[0];
        bool alreadyIn  = System.Array.Exists(nm.worldPrefabs, p => p == wispPrefab);
        if (!alreadyIn)
        {
            var list = new System.Collections.Generic.List<GameObject>(nm.worldPrefabs)
                { wispPrefab };
            nm.worldPrefabs = list.ToArray();
            EditorUtility.SetDirty(nmGO);
            r.AppendLine($"  ✓ Added to RodNetworkManager.worldPrefabs");
        }
        else r.AppendLine("  ✓ Already in RodNetworkManager.worldPrefabs");
    }

    [MenuItem("BCE/Setup/4w ▶ Wisp Mob Combat Setup", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
