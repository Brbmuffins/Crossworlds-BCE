using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Arcanist-specific caster that uses the shared AbilityCaster combat behavior,
/// but keeps the visible spellbook limited to the four Arcanist core slots.
/// Spellbook-backed variant payloads are kept as hidden variantOnly entries.
/// </summary>
public class ArcanistAbilityCaster : AbilityCaster
{
    static readonly string[] CoreAbilityNames =
    {
        "Ice Spikes",
        "Ember Beam",
        "Ice Guardian",
        "Meteor Shower",
    };

    protected override bool ShouldBackfillMissingSpellbookEntries => false;

    protected override void ConfigureInitialSpellbook()
    {
        EnsureArcanistSpellbook();
        EnsureArcanistEquippedIndices();
    }

    void EnsureArcanistSpellbook()
    {
        if (!HasArcanistCoreSpellbook(spellbook))
        {
            spellbook = CreateDefaultArcanistSpellbook();
            return;
        }

        NormalizeCoreVariantReferencesByName();

        List<AbilityDef> compact = null;
        int count = spellbook.Length;
        for (int i = 4; i < count; i++)
        {
            AbilityDef entry = spellbook[i];
            if (entry == null)
            {
                if (compact == null)
                    compact = NewCoreList();
                continue;
            }

            if (entry.variantOnly || IsReferencedByCoreVariant(entry, i))
            {
                if (!entry.variantOnly)
                    entry.variantOnly = true;

                if (compact != null)
                    compact.Add(entry);
                continue;
            }

            if (compact == null)
                compact = CopyEntriesBefore(i);
        }

        if (compact != null)
            spellbook = compact.ToArray();
    }

    void EnsureArcanistEquippedIndices()
    {
        if (equippedIndices != null
            && equippedIndices.Length == 4
            && equippedIndices[0] == 0
            && equippedIndices[1] == 1
            && equippedIndices[2] == 2
            && equippedIndices[3] == 3)
            return;

        equippedIndices = new[] { 0, 1, 2, 3 };
    }

    void NormalizeCoreVariantReferencesByName()
    {
        for (int i = 0; i < 4; i++)
        {
            AbilityDef owner = spellbook[i];
            if (owner?.variants == null) continue;

            foreach (AbilityVariant variant in owner.variants)
            {
                if (variant == null
                    || !variant.useSpellbookAbilityIndex
                    || variant.spellbookAbilityIndex < 0
                    || variant.spellbookAbilityIndex >= spellbook.Length)
                    continue;

                AbilityDef payload = spellbook[variant.spellbookAbilityIndex];
                if (payload == null || ReferenceEquals(payload, owner) || string.IsNullOrEmpty(payload.abilityName))
                    continue;

                variant.spellbookAbilityName = payload.abilityName;
                variant.useSpellbookAbilityIndex = false;
                variant.spellbookAbilityIndex = -1;
            }
        }
    }

    bool IsReferencedByCoreVariant(AbilityDef candidate, int spellbookIndex)
    {
        if (candidate == null) return false;

        for (int i = 0; i < 4; i++)
        {
            AbilityDef owner = spellbook[i];
            if (owner?.variants == null) continue;

            foreach (AbilityVariant variant in owner.variants)
            {
                if (variant == null) continue;

                if (!string.IsNullOrEmpty(variant.spellbookAbilityName)
                    && string.Equals(variant.spellbookAbilityName, candidate.abilityName, System.StringComparison.OrdinalIgnoreCase))
                    return true;

                if (variant.useSpellbookAbilityIndex && variant.spellbookAbilityIndex == spellbookIndex)
                    return true;
            }
        }

        return false;
    }

    static bool HasArcanistCoreSpellbook(AbilityDef[] source)
    {
        if (source == null || source.Length < 4) return false;

        for (int i = 0; i < CoreAbilityNames.Length; i++)
        {
            if (!string.Equals(source[i]?.abilityName, CoreAbilityNames[i], System.StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    List<AbilityDef> NewCoreList()
    {
        var compact = new List<AbilityDef>(spellbook.Length);
        for (int i = 0; i < 4; i++)
            compact.Add(spellbook[i]);
        return compact;
    }

    List<AbilityDef> CopyEntriesBefore(int stopIndex)
    {
        var compact = new List<AbilityDef>(spellbook.Length);
        for (int i = 0; i < stopIndex; i++)
            compact.Add(spellbook[i]);
        return compact;
    }

    static AbilityDef[] CreateDefaultArcanistSpellbook()
    {
        return new[]
        {
            new AbilityDef
            {
                abilityName = "Ice Spikes",
                shape = AbilityShape.Cone,
                category = AbilityCategory.Damage,
                range = 10f,
                coneAngle = 60f,
                rectWidth = 2f,
                indicatorSize = 2f,
                cooldown = 0f,
                castTime = 0.6f,
                chargeable = true,
                maxChargeTime = 1.5f,
                damage = 20f,
                maxChargeDamage = 50f,
                maxChargeSizeMultiplier = 1.8f,
                targetTag = "Enemy",
                variants = new AbilityVariant[0],
            },
            new AbilityDef
            {
                abilityName = "Ember Beam",
                shape = AbilityShape.Rectangle,
                category = AbilityCategory.Damage,
                range = 12f,
                coneAngle = 60f,
                rectWidth = 2f,
                indicatorSize = 1.5f,
                cooldown = 6f,
                castTime = 0.6f,
                damage = 25f,
                targetTag = "Enemy",
                variants = new AbilityVariant[0],
            },
            new AbilityDef
            {
                abilityName = "Ice Guardian",
                shape = AbilityShape.Circle,
                category = AbilityCategory.Support,
                range = 8f,
                indicatorSize = 8f,
                spawnTurret = true,
                cooldown = 30f,
                castTime = 0.6f,
                damage = 3f,
                targetTag = "Enemy",
                variants = new AbilityVariant[0],
            },
            new AbilityDef
            {
                abilityName = "Meteor Shower",
                shape = AbilityShape.Circle,
                category = AbilityCategory.Damage,
                range = 14f,
                coneAngle = 60f,
                rectWidth = 1.5f,
                indicatorSize = 12f,
                cooldown = 0f,
                castTime = 0.6f,
                damage = 7f,
                targetTag = "Enemy",
                usePulseDamage = true,
                pulseCount = 10,
                pulseInterval = 0.3f,
                pulseDamage = 7f,
                pulseVFXLifetime = 1f,
                variants = new AbilityVariant[0],
            },
        };
    }
}
