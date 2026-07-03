#if !UNITY_SERVER
using System.Collections;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Applies timed stat effects from crafted consumable items.
///
/// Effect types (match stat_bonus JSON "effect" field from items table):
///   hp_regen      — restores {value} HP over {duration} seconds (ticks every 2 s)
///   resist_void   — reduces void/boss damage by {value}% for {duration} s
///   resist_blast  — reduces blast/boss damage by {value}% for {duration} s
///   speed         — adds {value}% move speed bonus for {duration} s
///   damage_amp    — adds {value}% outgoing damage bonus for {duration} s
///
/// Usage:
///   ConsumableEffect.Apply("flask_hp_minor", localPlayerGO);
///
/// One effect per type active at a time; reusing the same type refreshes duration.
/// </summary>
public static class ConsumableEffect
{
    public static void Apply(string itemId, GameObject target)
    {
        var catalog = ItemCatalog.Instance;
        if (catalog == null) return;

        var item = catalog.Get(itemId);
        if (item?.statBonus == null) return;

        if (!item.statBonus.TryGetValue("effect",   out var effectRaw)) return;
        if (!item.statBonus.TryGetValue("value",    out var valueRaw))  return;
        if (!item.statBonus.TryGetValue("duration", out var durRaw))    return;

        if (!float.TryParse(valueRaw.ToString(),    NumberStyles.Float, CultureInfo.InvariantCulture, out float value))    return;
        if (!float.TryParse(durRaw.ToString(),      NumberStyles.Float, CultureInfo.InvariantCulture, out float duration)) return;

        var runner = target.GetComponent<ConsumableEffectRunner>()
                  ?? target.AddComponent<ConsumableEffectRunner>();

        runner.Apply(effectRaw.ToString(), value, duration);
    }
}

/// <summary>
/// MonoBehaviour that runs active consumable coroutines on the player GameObject.
/// One instance per player, manages all simultaneous effects.
/// </summary>
public class ConsumableEffectRunner : MonoBehaviour
{
    readonly System.Collections.Generic.Dictionary<string, Coroutine> _active =
        new System.Collections.Generic.Dictionary<string, Coroutine>();

    public void Apply(string effectType, float value, float duration)
    {
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
        var hp     = GetComponent<Health>();
        int ticks  = Mathf.Max(1, Mathf.RoundToInt(duration / 2f));
        int perTick = Mathf.Max(1, totalHeal / ticks);

        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(2f);
            hp?.Heal(perTick);
        }
        _active.Remove("hp_regen");
    }

    // ── Resist — uses Health.SetDamageReduction ───────────────────────────────
    IEnumerator ResistEffect(string damageType, float resistPct, float duration)
    {
        var sem = GetComponent<StatusEffectManager>();
        sem?.AddResist(damageType, resistPct);
        yield return new WaitForSeconds(duration);
        sem?.RemoveResist(damageType, resistPct);
        _active.Remove($"resist_{damageType}");
    }

    // ── Speed ──────────────────────────────────────────────────────────────────
    IEnumerator SpeedEffect(float bonus, float duration)
    {
        var cc = GetComponent<PlayerController>();
        cc?.AddSpeedBonus(bonus);
        yield return new WaitForSeconds(duration);
        cc?.RemoveSpeedBonus(bonus);
        _active.Remove("speed");
    }

    // ── Damage Amp ────────────────────────────────────────────────────────────
    IEnumerator DamageAmpEffect(float bonus, float duration)
    {
        var combat = GetComponent<PlayerCombat>();
        combat?.AddDamageMultiplier(bonus);
        yield return new WaitForSeconds(duration);
        combat?.RemoveDamageMultiplier(bonus);
        _active.Remove("damage_amp");
    }
}
#endif
