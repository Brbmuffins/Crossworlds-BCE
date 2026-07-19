#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BCE/Setup/4v — Healing VFX Builder
///
/// Auto-assigns the best healing/restoration visual effects (discovered via
/// full project audit) to every healing ability slot in the game:
///
///   ClericHealVFX (on Cleric.prefab):
///     healBurstPrefab  ← Holy hit.prefab        (divine impact burst)
///     healAuraPrefab   ← Healing.prefab          (character aura wrap)
///     healCrossPrefab  ← FX_PinkCross_Up.prefab  (classic +HP rising cross)
///
///   RestorationBeacon.prefab:
///     idleVFX   ← Healing circle.prefab    (persistent ground circle)
///     pulseVFX  ← Healing buff.prefab      (per-pulse burst every 3s)
///
///   AbilityCaster spellbook on Cleric.prefab (by abilityName):
///     Mending Circle   castVFX ← Healing circle.prefab   hitVFX ← Green hit.prefab
///     Mend             castVFX ← Healing.prefab (aura)   hitVFX ← Holy hit.prefab
///     Spirit Wisps     castVFX ← Healing circle.prefab   hitVFX ← Sparks green.prefab
///     Divine Spark     castVFX ← FX_LightPillar.prefab   hitVFX ← Holy hit.prefab
///     Temporal Grace   castVFX ← Healing circle.prefab   hitVFX ← Teleport.prefab
///
/// Run: BCE → Setup → 4v ▶ Healing VFX Builder
/// Then: Ctrl+S, rebuild + redeploy.
/// </summary>
public static class HealingVFXBuilder
{
    // ── Asset paths ───────────────────────────────────────────────────────────────

    // brbmuffins Magic Pack (the premium sock-blowers)
    const string MagicPackPfx = "Assets/brbMuff Folder/brbmuffins Studio/brbmuffins Magic Pack/Prefabs";

    static string HolyHit         => $"{MagicPackPfx}/Hits and explosions/Holy hit.prefab";
    static string GreenHit        => $"{MagicPackPfx}/Hits and explosions/Green hit.prefab";
    static string HealingAura     => $"{MagicPackPfx}/Character auras/Healing.prefab";
    static string LoveAura        => $"{MagicPackPfx}/Character auras/Love aura.prefab";
    static string HealingCircle   => $"{MagicPackPfx}/Magic circles/Healing circle.prefab";
    static string MagicCircle     => $"{MagicPackPfx}/Magic circles/Magic circle.prefab";
    static string SparksGreen     => $"{MagicPackPfx}/Sparks/Sparks green.prefab";
    static string SparksExplodeGreen => $"{MagicPackPfx}/Sparks/Sparks explode green.prefab";
    static string Teleport        => $"{MagicPackPfx}/Environment/Teleport.prefab";
    static string CrystalGreen    => $"{MagicPackPfx}/Environment/Crystal effect green.prefab";
    static string PortalGreen     => $"{MagicPackPfx}/Portals/Portal green.prefab";
    static string AoEGreen        => $"{MagicPackPfx}/AoE effects/AoE slash green.prefab";

    // brbmuffins Free VFX
    const string FreeVFXPfx = "Assets/brbMuff Folder/brbmuffins VFX/brbmuffins Free VFX/Prefab";

    static string LightPillar     => $"{FreeVFXPfx}/FX_LightPillar.prefab";
    static string PinkCrossUp     => $"{FreeVFXPfx}/FX_PinkCross_Up.prefab";
    static string GreenLightShrink=> $"{FreeVFXPfx}/FX_Greenlight_shrink.prefab";
    static string GreenHitFree    => $"{FreeVFXPfx}/FX_Green_Hit.prefab";

    // brbmuffins Trails
    const string TrailsPfx = "Assets/brbMuff Folder/brbmuffins Trails/brbmuffins Trails VFX/VFX/Particles";

    static string TrailNature     => $"{TrailsPfx}/VFX_Trail_Nature.prefab";
    static string TrailCosmos     => $"{TrailsPfx}/VFX_Trail_Cosmos.prefab";

    // Dark magic pack (used by RestorationBeacon per existing comments)
    static string HealingBuff     => "Assets/Game/FX/dark magic/Healing buff.prefab";
    static string LeavesShield    => "Assets/Game/FX/dark magic/Leaves shield.prefab";

    // Unity Particle Pack
    static string EllenRespawn    =>
        "Assets/Game/FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/EllenRespawn.prefab";
    static string Respawn         =>
        "Assets/Game/FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn.prefab";
    static string FireFlies       =>
        "Assets/Game/FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies.prefab";

    // Game prefab paths
    const string ClericPrefabPath      = "Assets/Game/Game_Prefabs/Cleric.prefab";
    const string BeaconPrefabPath      = "Assets/Game/Networking/RestorationBeacon.prefab";

    // ── Menu item ─────────────────────────────────────────────────────────────────

    [MenuItem("BCE/Setup/4v ▶ Healing VFX Builder", priority = 45)]
    static void Build()
    {
        var report = new StringBuilder();
        report.AppendLine("── Healing VFX Builder ──────────────────────────");

        // ── 1. Cleric.prefab — ClericHealVFX ─────────────────────────────────
        WireClericHealVFX(report);

        // ── 2. RestorationBeacon.prefab ───────────────────────────────────────
        WireRestorationBeacon(report);

        // ── 3. AbilityCaster spellbook on Cleric.prefab ───────────────────────
        WireClericSpellbook(report);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        report.AppendLine();
        report.AppendLine("Done. Ctrl+S to save scenes, then rebuild + redeploy.");
        EditorUtility.DisplayDialog("Healing VFX Builder", report.ToString(), "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 1. ClericHealVFX
    // ─────────────────────────────────────────────────────────────────────────────

    static void WireClericHealVFX(StringBuilder r)
    {
        var cleric = AssetDatabase.LoadAssetAtPath<GameObject>(ClericPrefabPath);
        if (cleric == null) { r.AppendLine($"  ✗ Cleric.prefab not found: {ClericPrefabPath}"); return; }

        var vfx = cleric.GetComponent<ClericHealVFX>();
        if (vfx == null) { r.AppendLine("  ✗ Cleric.prefab has no ClericHealVFX component — add it first."); return; }

        // Layer 1: Holy hit burst (most important — the satisfying impact)
        Set(ref vfx.healBurstPrefab, HolyHit, "ClericHealVFX.healBurstPrefab", r);

        // Layer 2: Healing aura wrap (the soft glow that hugs the Cleric)
        Set(ref vfx.healAuraPrefab, HealingAura, "ClericHealVFX.healAuraPrefab", r);

        // Layer 3: Rising cross (the classic +HP visual — pink/white rising)
        Set(ref vfx.healCrossPrefab, PinkCrossUp, "ClericHealVFX.healCrossPrefab", r);

        EditorUtility.SetDirty(cleric);
        r.AppendLine("  ✓ ClericHealVFX wired (3 layers)");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 2. RestorationBeacon.prefab
    // ─────────────────────────────────────────────────────────────────────────────

    static void WireRestorationBeacon(StringBuilder r)
    {
        var beacon = AssetDatabase.LoadAssetAtPath<GameObject>(BeaconPrefabPath);
        if (beacon == null)
        {
            r.AppendLine($"  ⚠ RestorationBeacon.prefab not found at {BeaconPrefabPath} — skipping.");
            r.AppendLine("    (It's a deployable prefab. Create it then re-run.)");
            return;
        }

        var rb = beacon.GetComponent<RestorationBeacon>();
        if (rb == null) { r.AppendLine("  ✗ RestorationBeacon.prefab has no RestorationBeacon component."); return; }

        // Persistent ground circle — the "beacon is here" visual
        Set(ref rb.idleVFX, HealingCircle, "RestorationBeacon.idleVFX", r);

        // Per-pulse burst — fire every pulseInterval seconds on heal
        Set(ref rb.pulseVFX, HealingBuff, "RestorationBeacon.pulseVFX", r);

        EditorUtility.SetDirty(beacon);
        r.AppendLine("  ✓ RestorationBeacon wired");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 3. AbilityCaster spellbook — Cleric healing spells
    // ─────────────────────────────────────────────────────────────────────────────

    static void WireClericSpellbook(StringBuilder r)
    {
        var cleric = AssetDatabase.LoadAssetAtPath<GameObject>(ClericPrefabPath);
        if (cleric == null) return;

        var caster = cleric.GetComponent<AbilityCaster>();
        if (caster == null) { r.AppendLine("  ✗ Cleric.prefab has no AbilityCaster."); return; }

        if (caster.spellbook == null || caster.spellbook.Length == 0)
        {
            r.AppendLine("  ⚠ AbilityCaster.spellbook is empty — populate spells then re-run.");
            return;
        }

        int wired = 0;
        foreach (var spell in caster.spellbook)
        {
            if (spell == null) continue;
            if (!WireSpell(spell, r)) continue;
            wired++;
        }

        if (wired > 0)
        {
            EditorUtility.SetDirty(cleric);
            r.AppendLine($"  ✓ Wired VFX on {wired} healing spell(s)");
        }
        else
        {
            r.AppendLine("  ⚠ No healing spells matched by name — check abilityName values in Inspector");
        }
    }

    /// <summary>
    /// Returns true if the spell was matched and modified.
    /// Matches on abilityName substring (case-insensitive) for robustness.
    /// </summary>
    static bool WireSpell(AbilityDef spell, StringBuilder r)
    {
        string n = spell.abilityName?.ToLower() ?? "";

        // Mending Circle — AoE ground-targeted heal
        // Cast: healing circle expands on the ground
        // Hit:  green sparkle burst on each healed ally
        if (n.Contains("mending") || n.Contains("mending circle"))
        {
            SetGO(ref spell.castVFX, HealingCircle, $"  {spell.abilityName}.castVFX", r);
            SetGO(ref spell.hitVFX,  GreenHit,      $"  {spell.abilityName}.hitVFX",  r);
            return true;
        }

        // Mend — single-target heal + debuff cleanse
        // Cast: healing aura wraps the caster for cast feedback
        // Hit:  holy divine impact on the target
        if (n == "mend" || n.Contains("field repair") || n.Contains("mend"))
        {
            SetGO(ref spell.castVFX, HealingAura, $"  {spell.abilityName}.castVFX", r);
            SetGO(ref spell.hitVFX,  HolyHit,     $"  {spell.abilityName}.hitVFX",  r);
            return true;
        }

        // Spirit Wisps — floating healing orbs cloud (naniteSwarm)
        // Cast: healing circle on ground where wisps emerge from
        // Hit:  green sparks as each wisp lands on an ally
        if (n.Contains("spirit") || n.Contains("wisp"))
        {
            SetGO(ref spell.castVFX, HealingCircle, $"  {spell.abilityName}.castVFX", r);
            SetGO(ref spell.hitVFX,  SparksGreen,   $"  {spell.abilityName}.hitVFX",  r);
            return true;
        }

        // Divine Spark — revive (30% HP) or holy nuke on undead
        // Cast: light pillar descends from above (the most dramatic cast in the game)
        // Hit:  holy hit burst at impact
        if (n.Contains("divine") || n.Contains("spark"))
        {
            SetGO(ref spell.castVFX, LightPillar, $"  {spell.abilityName}.castVFX", r);
            SetGO(ref spell.hitVFX,  HolyHit,     $"  {spell.abilityName}.hitVFX",  r);
            return true;
        }

        // Temporal Grace — rewind entire team (most epic ability)
        // Cast: portal green + teleport shimmer on all team members
        // Hit:  teleport burst as team "snaps back" to previous state
        if (n.Contains("temporal") || n.Contains("grace") || n.Contains("rewind"))
        {
            SetGO(ref spell.castVFX, PortalGreen, $"  {spell.abilityName}.castVFX", r);
            SetGO(ref spell.hitVFX,  Teleport,    $"  {spell.abilityName}.hitVFX",  r);
            return true;
        }

        // Restoration Beacon — the deployable
        // Cast: crystal green burst where beacon lands
        // (deployablePrefab is wired separately via RestorationBeacon.prefab)
        if (n.Contains("restoration") || n.Contains("beacon"))
        {
            SetGO(ref spell.castVFX, CrystalGreen, $"  {spell.abilityName}.castVFX", r);
            return true;
        }

        // Arcane Ward / Sacred Aegis — shield spells
        // Cast: leaves shield wraps target (nature-health flavored)
        // Hit:  green light shrink pulse as shield activates
        if (n.Contains("ward") || n.Contains("aegis") || n.Contains("shield"))
        {
            SetGO(ref spell.castVFX, LeavesShield,     $"  {spell.abilityName}.castVFX", r);
            SetGO(ref spell.hitVFX,  GreenLightShrink, $"  {spell.abilityName}.hitVFX",  r);
            return true;
        }

        // Triage Loop / Nanite Triage — HoT (heal over time)
        if (n.Contains("triage") || n.Contains("nanite") || n.Contains("regen"))
        {
            SetGO(ref spell.castVFX, HealingAura,   $"  {spell.abilityName}.castVFX", r);
            SetGO(ref spell.hitVFX,  SparksGreen,   $"  {spell.abilityName}.hitVFX",  r);
            return true;
        }

        // Fallback: any remaining heal-category spell
        if (spell.category == AbilityCategory.Heal)
        {
            SetGO(ref spell.castVFX, HealingCircle, $"  {spell.abilityName}.castVFX", r);
            SetGO(ref spell.hitVFX,  GreenHitFree,  $"  {spell.abilityName}.hitVFX",  r);
            return true;
        }

        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    static void Set(ref GameObject field, string assetPath, string label, StringBuilder r)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null) { r.AppendLine($"  ⚠ Not found: {assetPath}"); return; }
        field = asset;
        r.AppendLine($"  → {label} = {asset.name}");
    }

    static void SetGO(ref GameObject field, string assetPath, string label, StringBuilder r)
    {
        Set(ref field, assetPath, label, r);
    }

    [MenuItem("BCE/Setup/4v ▶ Healing VFX Builder", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
