#if UNITY_EDITOR || !UNITY_SERVER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies timed stat effects from crafted consumable items.
///
/// Effect definitions live in the static table below and MUST match the
/// stat_bonus JSON seeded by _CONTEXT/professions-full-patch.sql — the server
/// validates crafting/ownership, the client only plays out the buff.
///
/// Effect types:
///   hp_regen      — restores {value} HP over {duration} seconds (ticks every 2 s)
///   resist_void   — reduces incoming damage by {value} fraction for {duration} s
///   resist_blast  — reduces incoming damage by {value} fraction for {duration} s
///   speed         — adds {value} fraction move speed for {duration} s
///   damage_amp    — adds {value} fraction outgoing damage for {duration} s
///
/// Usage:
///   ConsumableEffect.Apply("flask_hp_minor", localPlayerGO);
///
/// One effect per type active at a time; reusing the same type refreshes duration.
///
/// NOTE: hp_regen and resist mutate Health, which only accepts server-side
/// changes — they take effect in host mode / on the server player object.
/// Pure-client authoritative buffs need a Command bridge (ROADMAP item).
/// </summary>
public static class ConsumableEffect
{
    class EffectDef
    {
        public string type; public float value; public float duration;
        public EffectDef(string t, float v, float d) { type = t; value = v; duration = d; }
    }

    // Keep in sync with the consumable seeds in professions-full-patch.sql
    static readonly Dictionary<string, EffectDef> Effects = new Dictionary<string, EffectDef>
    {
        { "flask_hp_minor",    new EffectDef("hp_regen",     15f,   40f) },
        { "flask_mp_minor",    new EffectDef("mana_restore", 15f,    0f) },
        { "flask_hp_major",    new EffectDef("hp_regen",     80f,   60f) },
        { "flask_void_resist", new EffectDef("resist_void",  0.25f, 90f) },
        { "kit_iron_warden",   new EffectDef("resist_blast", 0.25f, 60f) },
        { "flask_speed",       new EffectDef("speed",        0.20f, 30f) },
        { "flask_damage",      new EffectDef("damage_amp",   0.15f, 45f) },
    };

    /// True if this item id is a usable consumable (drives "use" vs "equip" in the bag UI).
    public static bool IsConsumable(string itemId) =>
        !string.IsNullOrEmpty(itemId) && Effects.ContainsKey(itemId);

    /// <summary>Player-facing effect text used by inventory and vendor tooltips.</summary>
    public static bool TryGetTooltip(string itemId, out string tooltip)
    {
        tooltip = null;
        if (string.IsNullOrEmpty(itemId) || !Effects.TryGetValue(itemId, out var def))
            return false;

        tooltip = def.type switch
        {
            "hp_regen" => $"Restores {def.value:0} HP over {def.duration:0} seconds.",
            "mana_restore" => $"Restores {def.value:0} MP immediately.",
            "resist_void" => $"Reduces Void damage by {def.value:P0} for {def.duration:0} seconds.",
            "resist_blast" => $"Reduces Blast damage by {def.value:P0} for {def.duration:0} seconds.",
            "speed" => $"Increases movement speed by {def.value:P0} for {def.duration:0} seconds.",
            "damage_amp" => $"Increases damage by {def.value:P0} for {def.duration:0} seconds.",
            _ => null
        };
        return !string.IsNullOrWhiteSpace(tooltip);
    }

    /// Returns true if the item is a known consumable and its effect was started.
    public static bool Apply(string itemId, GameObject target)
    {
        if (target == null || !Effects.TryGetValue(itemId, out var def)) return false;

        var runner = target.GetComponent<ConsumableEffectRunner>()
                  ?? target.AddComponent<ConsumableEffectRunner>();

        runner.Apply(def.type, def.value, def.duration);
        return true;
    }
}

/// <summary>
/// MonoBehaviour that runs active consumable coroutines on the player GameObject.
/// One instance per player, manages all simultaneous effects.
/// </summary>
public class ConsumableEffectRunner : MonoBehaviour
{
    readonly Dictionary<string, Coroutine> _active = new Dictionary<string, Coroutine>();

    public void Apply(string effectType, float value, float duration)
    {
        if (effectType == "mana_restore")
        {
            GetComponent<CharacterStats>()?.RestoreMana(value);
            RodChatManager.Instance?.AddSystemMessage($"[CONSUMABLE] Restored {value:0} MP");
            return;
        }

        if (_active.TryGetValue(effectType, out var existing) && existing != null)
            StopCoroutine(existing);

        string label = System.Globalization.CultureInfo.InvariantCulture
            .TextInfo.ToTitleCase(effectType.Replace('_', ' '));
        RodChatManager.Instance?.AddSystemMessage($"[BUFF] {label} active for {duration:F0}s");

        _active[effectType] = effectType switch
        {
            "hp_regen"     => StartCoroutine(HpRegenEffect((int)value, duration)),
            "resist_void"  => StartCoroutine(ResistEffect("void",  value, duration)),
            "resist_blast" => StartCoroutine(ResistEffect("blast", value, duration)),
            "speed"        => StartCoroutine(SpeedEffect(value, duration)),
            "damage_amp"   => StartCoroutine(DamageAmpEffect(value, duration)),
            _              => null
        };
    }

    // ── HP Regen ─────────────────────────────────────────────────────────────
    IEnumerator HpRegenEffect(int totalHeal, float duration)
    {
        var hp      = GetComponent<Health>();
        int ticks = Mathf.Max(1, Mathf.RoundToInt(duration / 2f));
        float perTick = totalHeal / (float)ticks;

        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(2f);
            hp?.Heal(perTick);
        }
        _active.Remove("hp_regen");
    }

    // ── Resist — uses Health.SetDamageReduction via StatusEffectManager ───────
    IEnumerator ResistEffect(string damageType, float resistFraction, float duration)
    {
        var sem = GetComponent<StatusEffectManager>();
        sem?.AddResist(damageType, resistFraction);
        yield return new WaitForSeconds(duration);
        sem?.RemoveResist(damageType, resistFraction);
        _active.Remove($"resist_{damageType}");
    }

    // ── Speed — scales PlayerMovement.moveSpeed (client-authoritative) ────────
    IEnumerator SpeedEffect(float bonus, float duration)
    {
        var pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.moveSpeed *= (1f + bonus);
            pm.sprintSpeed *= (1f + bonus);
        }
        yield return new WaitForSeconds(duration);
        if (pm != null)
        {
            pm.moveSpeed /= (1f + bonus);
            pm.sprintSpeed /= (1f + bonus);
        }
        _active.Remove("speed");
    }

    // ── Damage Amp — temporary channel on CharacterStats ─────────────────────
    IEnumerator DamageAmpEffect(float bonus, float duration)
    {
        var stats = GetComponent<CharacterStats>();
        stats?.AddTemporaryDamagePct(bonus);
        yield return new WaitForSeconds(duration);
        stats?.AddTemporaryDamagePct(-bonus);
        _active.Remove("damage_amp");
    }
}
#endif
