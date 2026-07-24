using UnityEngine;

// Stat channels that gear and attunements can modify.
// DESIGN PILLAR: no leveling — every point of character power comes from
// equipped gear and the attunements socketed into it.
public enum StatType
{
    MaxHealth = 0,          // bonus hit points
    Damage = 1,             // outgoing damage      (Percent: 0.10 = +10%)
    DamageReduction = 2,    // incoming damage cut   (Percent: 0.10 = -10% taken)
    MoveSpeed = 3,          // movement speed        (Percent: 0.10 = +10%)
    CooldownReduction = 4,  // ability cooldowns     (Percent: 0.10 = -10% cd)
    HealPower = 5,          // healing dealt         (Percent: 0.10 = +10%)
    CriticalStrikeChance = 6, // crit chance         (Percent: 0.10 = +10%)
    MaxMana = 7,            // bonus mana
    Hp5 = 8,                // health per 5 seconds
    Mp5 = 9                 // mana per 5 seconds
}

public enum ModifierKind
{
    Flat,       // added directly — use for MaxHealth (e.g. +25 HP)
    Percent     // fraction — use for the rest (e.g. 0.10 = +10%)
}

// One stat change. Gear carries an array of these; so does each attunement.
[System.Serializable]
public struct StatModifier
{
    public StatType     stat;
    public ModifierKind kind;
    public float        value;

    public StatModifier(StatType stat, ModifierKind kind, float value)
    {
        this.stat  = stat;
        this.kind  = kind;
        this.value = value;
    }

    // Tooltip label, e.g. "+10% Damage" or "+25 Max Health".
    public string Describe()
    {
        string sign = value >= 0f ? "+" : "";
        string amount = kind == ModifierKind.Percent
            ? sign + Mathf.RoundToInt(value * 100f) + "%"
            : sign + Mathf.RoundToInt(value);
        return amount + " " + Prettify(stat);
    }

    static string Prettify(StatType s)
    {
        switch (s)
        {
            case StatType.MaxHealth:         return "Max Health";
            case StatType.MaxMana:           return "Max Mana";
            case StatType.Damage:            return "Damage";
            case StatType.DamageReduction:   return "Damage Reduction";
            case StatType.MoveSpeed:         return "Move Speed";
            case StatType.CooldownReduction: return "Cooldown Reduction";
            case StatType.HealPower:         return "Heal Power";
            case StatType.CriticalStrikeChance: return "Critical Strike Chance";
            case StatType.Hp5:               return "HP5";
            case StatType.Mp5:               return "MP5";
            default:                         return s.ToString();
        }
    }
}
