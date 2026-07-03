using System.Collections;
using UnityEngine;
using Mirror;

/// <summary>
/// HeroCosmeticApplier — Per-player component. Reads mastery level from
/// HeroMasteryManager and tints the player's renderers to reflect mastery tier.
///
/// Copy to: Assets/Game/Characters/Scripts/HeroCosmeticApplier.cs
/// Attach to the player prefab root. No Inspector wiring required.
///
/// Tiers: 0-2=default grey | 3-4=bronze | 5-6=silver | 7-9=gold | 10=diamond
/// </summary>
#if !UNITY_SERVER
public class HeroCosmeticApplier : NetworkBehaviour
{
    // Tier colors (base color tint applied via MaterialPropertyBlock)
    static readonly Color TierDefault = new Color(0.80f, 0.80f, 0.80f);
    static readonly Color TierBronze  = new Color(0.80f, 0.50f, 0.20f);
    static readonly Color TierSilver  = new Color(0.85f, 0.85f, 0.95f);
    static readonly Color TierGold    = new Color(1.00f, 0.85f, 0.20f);
    static readonly Color TierDiamond = new Color(0.50f, 0.90f, 1.00f);

    private Renderer[] _renderers;
    private int _appliedLevel = -1;
    private int _heroClass    = -1;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        _renderers = GetComponentsInChildren<Renderer>();
        HeroMasteryManager.OnMasteryLevelUp += OnMasteryLevelUp;
        StartCoroutine(InitWhenReady());
    }

    void OnDestroy()
    {
        HeroMasteryManager.OnMasteryLevelUp -= OnMasteryLevelUp;
    }

    IEnumerator InitWhenReady()
    {
        // Wait for both PlayerIdentity and HeroMasteryManager to be ready
        var pid = GetComponent<PlayerIdentity>();
        while (pid == null) { yield return null; pid = GetComponent<PlayerIdentity>(); }

        while (HeroMasteryManager.Instance == null || !HeroMasteryManager.Instance.IsLoaded)
            yield return new WaitForSeconds(0.5f);

        _heroClass = pid.classIndex;
        ApplyTier(HeroMasteryManager.Instance.GetLevel(_heroClass));
    }

    void OnMasteryLevelUp(int heroClass, int newLevel)
    {
        if (!isLocalPlayer || heroClass != _heroClass) return;
        ApplyTier(newLevel);
    }

    void ApplyTier(int masteryLevel)
    {
        if (masteryLevel == _appliedLevel) return;
        _appliedLevel = masteryLevel;

        Color tint = GetTierColor(masteryLevel);
        var block  = new MaterialPropertyBlock();

        foreach (var rend in _renderers)
        {
            if (rend == null) continue;
            rend.GetPropertyBlock(block);
            block.SetColor("_BaseColor", tint);
            rend.SetPropertyBlock(block);
        }

        Debug.Log($"[COSMETIC] {name}: mastery lv{masteryLevel} → {GetTierName(masteryLevel)} tint");
    }

    static Color GetTierColor(int lv)
    {
        if (lv >= 10) return TierDiamond;
        if (lv >= 7)  return TierGold;
        if (lv >= 5)  return TierSilver;
        if (lv >= 3)  return TierBronze;
        return TierDefault;
    }

    static string GetTierName(int lv)
    {
        if (lv >= 10) return "Diamond";
        if (lv >= 7)  return "Gold";
        if (lv >= 5)  return "Silver";
        if (lv >= 3)  return "Bronze";
        return "Default";
    }
}
#endif
