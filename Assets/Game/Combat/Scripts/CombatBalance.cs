using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum CombatScalingStat { Strength, Agility, Intelligence, Vitality }

/// <summary>
/// Versioned ability coefficients. Damage is resolved by the Unity game server;
/// clients load the bundled copy only to explain the server's current version.
/// </summary>
public static class CombatBalance
{
    const string ResourcePath = "Combat/ability-scaling-v1";
    const string ServerOverrideEnvironmentVariable = "CROSSWORLDS_COMBAT_BALANCE_PATH";

    [Serializable]
    sealed class BalanceFile
    {
        public int version;
        public AbilityRule[] abilities;
    }

    [Serializable]
    public sealed class AbilityRule
    {
        public int classIndex;
        public string abilityName;
        public string stat;
        public float damagePerPoint;
    }

    static BalanceFile _loaded;
    static readonly Dictionary<string, AbilityRule> Rules = new(StringComparer.OrdinalIgnoreCase);

#if UNITY_EDITOR
    public static void ReloadForEditorValidation()
    {
        Rules.Clear();
        _loaded = null;
    }
#endif

    public static int Version
    {
        get { EnsureLoaded(); return _loaded.version; }
    }

    public static bool TryGetRule(int classIndex, string abilityName, out AbilityRule rule)
    {
        EnsureLoaded();
        return Rules.TryGetValue(Key(classIndex, abilityName), out rule);
    }

    public static IEnumerable<AbilityRule> GetRulesForClass(int classIndex)
    {
        EnsureLoaded();
        foreach (AbilityRule rule in Rules.Values)
            if (rule.classIndex == classIndex) yield return rule;
    }

    public static bool TryGetStat(AbilityRule rule, out CombatScalingStat stat) =>
        Enum.TryParse(rule?.stat, true, out stat) && Enum.IsDefined(typeof(CombatScalingStat), stat);

    public static float CalculateStatBonus(CombatScalingStat stat, int statValue, float damagePerPoint) =>
        Mathf.Max(0, statValue - (stat == CombatScalingStat.Vitality ? 10 : 5))
        * Mathf.Max(0f, damagePerPoint);

    static string Key(int classIndex, string abilityName) =>
        $"{classIndex}:{abilityName?.Trim()}";

    static void EnsureLoaded()
    {
        if (_loaded != null) return;

        TextAsset bundled = Resources.Load<TextAsset>(ResourcePath);
        string json = bundled != null ? bundled.text : null;
#if UNITY_SERVER && !UNITY_EDITOR
        string serverPath = Environment.GetEnvironmentVariable(ServerOverrideEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(serverPath))
        {
            try { json = File.ReadAllText(serverPath); }
            catch (Exception error)
            {
                Debug.LogError($"[COMBAT BALANCE] Cannot read server override: {error.Message}");
                json = null;
            }
        }
#endif

        BalanceFile parsed = null;
        if (!string.IsNullOrWhiteSpace(json))
        {
            try { parsed = JsonUtility.FromJson<BalanceFile>(json); }
            catch (Exception error) { Debug.LogError($"[COMBAT BALANCE] Invalid JSON: {error.Message}"); }
        }

        _loaded = parsed != null && parsed.version > 0 && parsed.abilities != null
            ? parsed : new BalanceFile { version = 0, abilities = Array.Empty<AbilityRule>() };
        if (_loaded.version == 0)
            Debug.LogError("[COMBAT BALANCE] No valid ability-scaling config; ability-specific bonuses are disabled.");

        foreach (AbilityRule rule in _loaded.abilities)
        {
            if (rule == null || rule.classIndex < 0 || rule.classIndex > 5 ||
                string.IsNullOrWhiteSpace(rule.abilityName) || rule.damagePerPoint < 0f ||
                float.IsNaN(rule.damagePerPoint) || float.IsInfinity(rule.damagePerPoint) ||
                !TryGetStat(rule, out _))
            {
                Debug.LogError("[COMBAT BALANCE] Ignoring an invalid ability rule.");
                continue;
            }
            if (!Rules.TryAdd(Key(rule.classIndex, rule.abilityName), rule))
                Debug.LogError($"[COMBAT BALANCE] Duplicate rule for class {rule.classIndex}, {rule.abilityName}.");
        }
    }
}
