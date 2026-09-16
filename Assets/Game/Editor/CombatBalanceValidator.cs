using System;
using UnityEditor;
using UnityEngine;

public static class CombatBalanceValidator
{
    static readonly string[] ClassPrefabs =
    {
        "Marauder", "Ironclad", "Shadowblade", "Cleric", "Arcanist", "Necromancer"
    };

    [MenuItem("BCE/Validate/Combat Balance")]
    public static void Validate()
    {
        CombatBalance.ReloadForEditorValidation();
        int errors = 0;
        int checkedRules = 0;
        if (CombatBalance.Version <= 0)
        {
            Debug.LogError("[COMBAT BALANCE] Missing or invalid versioned balance definition.");
            errors++;
        }

        for (int classIndex = 0; classIndex < ClassPrefabs.Length; classIndex++)
        {
            string path = $"Assets/Game/Game_Prefabs/{ClassPrefabs[classIndex]}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            AbilityCaster caster = prefab != null ? prefab.GetComponentInChildren<AbilityCaster>(true) : null;
            foreach (CombatBalance.AbilityRule rule in CombatBalance.GetRulesForClass(classIndex))
            {
                checkedRules++;
                if (!CombatBalance.TryGetStat(rule, out CombatScalingStat stat))
                {
                    Debug.LogError($"[COMBAT BALANCE] Invalid stat for {rule.abilityName}.");
                    errors++;
                    continue;
                }
                int baseline = CharacterStats.GetScalingBaseline(stat);
                if (!Mathf.Approximately(CombatBalance.CalculateStatBonus(stat, baseline, rule.damagePerPoint), 0f) ||
                    !Mathf.Approximately(CombatBalance.CalculateStatBonus(stat, baseline + 10, rule.damagePerPoint),
                        10f * rule.damagePerPoint))
                {
                    Debug.LogError($"[COMBAT BALANCE] Baseline formula failed for {rule.abilityName}.");
                    errors++;
                }
                if (caster == null || caster.spellbook == null)
                {
                    Debug.LogError($"[COMBAT BALANCE] Missing AbilityCaster spellbook in {path}.");
                    errors++;
                    break;
                }
                bool found = false;
                foreach (AbilityDef ability in caster.spellbook)
                {
                    if (ability == null || !string.Equals(ability.abilityName,
                            rule.abilityName, StringComparison.OrdinalIgnoreCase)) continue;
                    found = ability.category == AbilityCategory.Damage &&
                        (ability.damage > 0f || ability.pulseDamage > 0f || ability.secondaryDamage > 0f);
                    break;
                }
                if (found) continue;
                Debug.LogError($"[COMBAT BALANCE] {path}: {rule.abilityName} is missing or has no damage payload.");
                errors++;
            }
        }

        if (checkedRules == 0)
        {
            Debug.LogError("[COMBAT BALANCE] No ability scaling rules were loaded.");
            errors++;
        }
        if (errors == 0)
            Debug.Log($"[COMBAT BALANCE] PASS: version {CombatBalance.Version}, {checkedRules} ability rule(s).");
        else
            Debug.LogError($"[COMBAT BALANCE] FAIL: {errors} error(s), {checkedRules} rule(s) checked.");
    }
}
