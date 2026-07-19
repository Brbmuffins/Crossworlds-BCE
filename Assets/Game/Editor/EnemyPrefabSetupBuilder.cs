#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/4h — Enemy Prefab Setup
///
/// One-click patch for the three core enemy prefabs:
///   1. Adds EnemyHeavyAttack if missing (gives all enemies the hard-hit ability system)
///   2. Sets sensible enemyTemplateId per tier if the field is still the default
///   3. Force-reimports each prefab so NetworkIdentity.OnValidate writes a correct
///      non-zero _assetId (same bug as the class prefabs — SaveAsPrefabAsset never
///      triggers OnValidate, so the value stays 0 in built clients)
///
/// Run once, commit the changed prefab files, rebuild + redeploy.
/// </summary>
public static class EnemyPrefabSetupBuilder
{
    const string PrefabDir = "Assets/Game/Game_Prefabs/Muffin Junk";

    struct EnemyEntry
    {
        public string fileName;
        public string templateId;
        public float  heavyMinCooldown;
        public float  heavyMaxCooldown;
        public float  heavyDamageMult;

        // Which heavy attack types are allowed (null = all five)
        public EnemyHeavyAttack.HeavyAbilityType[] allowedTypes;
    }

    static readonly EnemyEntry[] Enemies = {
        new EnemyEntry {
            fileName          = "Enemy_Grunt",
            templateId        = "grunt_basic",
            heavyMinCooldown  = 12f,
            heavyMaxCooldown  = 20f,
            heavyDamageMult   = 2.0f,
            allowedTypes      = new[] {
                EnemyHeavyAttack.HeavyAbilityType.GroundSlam,
                EnemyHeavyAttack.HeavyAbilityType.GroundSpikes,
            },
        },
        new EnemyEntry {
            fileName          = "Enemy_Ranged",
            templateId        = "ranged_basic",
            heavyMinCooldown  = 10f,
            heavyMaxCooldown  = 16f,
            heavyDamageMult   = 2.2f,
            allowedTypes      = new[] {
                EnemyHeavyAttack.HeavyAbilityType.VoidBurst,
                EnemyHeavyAttack.HeavyAbilityType.ChainLightning,
            },
        },
        new EnemyEntry {
            fileName          = "Enemy_Elite",
            templateId        = "elite_basic",
            heavyMinCooldown  = 8f,
            heavyMaxCooldown  = 14f,
            heavyDamageMult   = 3.0f,
            allowedTypes      = null,   // elites can roll all 5
        },
    };

    [MenuItem("BCE/Setup/4h ▶ Enemy Prefab Setup (HeavyAttack + assetId)", priority = 41)]
    static void Run()
    {
        var report = new StringBuilder();
        int patched = 0;
        int missing = 0;

        foreach (var entry in Enemies)
        {
            string path   = $"{PrefabDir}/{entry.fileName}.prefab";
            var    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                report.AppendLine($"  ✗ Not found: {path}");
                missing++;
                continue;
            }

            bool dirty = false;

            // ── 1. EnemyHeavyAttack ───────────────────────────────────────────
            var ha = prefab.GetComponent<EnemyHeavyAttack>();
            if (ha == null)
            {
                ha    = prefab.AddComponent<EnemyHeavyAttack>();
                dirty = true;
            }

            // Always apply the configured values
            ha.minCooldown    = entry.heavyMinCooldown;
            ha.maxCooldown    = entry.heavyMaxCooldown;
            ha.damageMultiplier = entry.heavyDamageMult;
            ha.availableTypes = entry.allowedTypes ?? new EnemyHeavyAttack.HeavyAbilityType[0];
            dirty = true;

            // ── 2. enemyTemplateId ────────────────────────────────────────────
            var ec = prefab.GetComponent<EnemyController>();
            if (ec != null && (string.IsNullOrEmpty(ec.enemyTemplateId) ||
                               ec.enemyTemplateId == "grunt_basic" && entry.fileName != "Enemy_Grunt"))
            {
                ec.enemyTemplateId = entry.templateId;
                dirty = true;
            }

            if (dirty) EditorUtility.SetDirty(prefab);

            // ── 3. Force-reimport to fix _assetId: 0 ─────────────────────────
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // Read back assetId for report
            var ni    = prefab.GetComponent<Mirror.NetworkIdentity>();
            uint asId = ni != null ? ni.assetId : 0;
            string ok = asId != 0 ? "✅" : "❌ still 0";
            report.AppendLine($"  {entry.fileName}  assetId={asId} {ok}  heavy={ha.minCooldown}-{ha.maxCooldown}s");
            patched++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = patched > 0
            ? $"Patched {patched} enemy prefab(s).\n\n{report}\nCommit changed prefabs + redeploy."
            : $"No prefabs found in {PrefabDir}.";
        if (missing > 0) summary += $"\n\n{missing} prefab(s) missing — check path.";

        EditorUtility.DisplayDialog("Enemy Prefab Setup", summary, "OK");
    }

    [MenuItem("BCE/Setup/4h ▶ Enemy Prefab Setup (HeavyAttack + assetId)", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
