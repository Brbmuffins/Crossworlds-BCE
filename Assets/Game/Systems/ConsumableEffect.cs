#if !UNITY_SERVER
using System.Collections;
using UnityEngine;

/// <summary>
/// Applies timed stat effects from crafted consumable items.
///
/// Effect types (match stat_bonus JSON "effect" field from items table):
///   hp_regen      — restores {value} HP over {duration} seconds (ticks every 2s)
///   resist_void   — reduces Null Architect void damage by {value}% for {duration}s
///   resist_blast  — reduces Iron Warden blast damage by {value}% for {duration}s
///   speed         — adds {value}% move speed bonus for {duration}s
///   damage_amp    — adds {value}% outgoing damage bonus for {duration}s
///
/// Usage:
///   ConsumableEffect.Apply("flask_hp_minor", localPlayerGO);
///   // Reads effect metadata from InventoryManager's item catalog.
///   // Only one effect per type can be active; reuse refreshes duration.
/// </summary>
public static class ConsumableEffect
{
    public static void Apply(string itemId, GameObject target)
    {
        var catalog = ItemCatalog.Instance;
        if (catalog == null) return;

        var item = catalog.Get(itemId);
        if (item == null || item.statBonus == null) return;

        if (!item.statBonus.TryGetValue("effect",   out var effectRaw)) return;
        if (!item.statBonus.TryGetValue("value",    out var valueRaw))  return;
        if (!item.statBonus.TryGetValue("duration", out var durRaw))    return;

        string effectType = effectRaw.ToString();
        float  value      = float.Parse(valueRaw.ToString());
        float  duration   = float.Parse(durRaw.ToString());

        var runner = target.GetComponent<ConsumableEffectRunner>()
                  ?? target.AddComponent<ConsumableEffectRunner>();

        runner.Apply(effectType, value, duration, itemId);
    }
}

/// <summary>
/// MonoBehaviour that runs active consumable coroutines on the player GameObject.
/// One instance per player — manages all simultaneous effects.
/// </summary>
public class ConsumableEffectRunner : MonoBehaviour
{
    // Active effect coroutines keyed by effect type so same type refreshes rather than stacks
    System.Collections.Generic.Dictionary<string, Coroutine> _active =
        new System.Collections.Generic.Dictionary<string, Coroutine>();

    public void Apply(string effectType, float value, float duration, string itemId)
    {
        if (_active.TryGetValue(effectType, out var existing) && existing != null)
            StopCoroutine(existing);

        RodChatManager.Instance?.SystemMessage($"[BUFF] {effectType.Replace('_', ' ')} active for {duration}s");

        _active[effectType] = effectType switch
        {
            "hp_regen"      => StartCoroutine(HpRegenEffect((int)value, duration)),
            "resist_void"   => StartCoroutine(ResistEffect("void",  value, duration)),
            "resist_blast"  => StartCoroutine(ResistEffect("blast", value, duration)),
            "speed"         => StartCoroutine(SpeedEffect(value, duration)),
            "damage_amp"    => StartCoroutine(DamageAmpEffect(value, duration)),
            _               => null
        };
    }

    // ── HP Regen ─────────────────────────────────────────────────────────────
    IEnumerator HpRegenEffect(int totalHeal, float duration)
    {
        var hp   = GetComponent<HealthComponent>();
        int ticks = Mathf.Max(1, Mathf.RoundToInt(duration / 2f));
        int perTick = Mathf.Max(1, totalHeal / ticks);

        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(2f);
            hp?.Heal(perTick);
        }
        _active.Remove("hp_regen");
    }

    // ── Resist (void / blast) ─────────────────────────────────────────────────
    // Registers with StatusEffectManager so boss damage calculations can read it
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
