using UnityEngine;

/// <summary>
/// Arcanist-specific caster that uses the shared AbilityCaster combat behavior,
/// while preserving the original four Arcanist core slots.
/// Additional visible spells and hidden variant payloads are authored in the
/// Spellbook and must survive editor validation.
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
        // Seed a brand-new caster, but never replace an authored spellbook during
        // OnValidate. Unity can expose a partially-applied serialized array while
        // an individual field is being edited; treating that transient state as a
        // missing core spellbook previously erased every custom spell and icon.
        if (spellbook == null || spellbook.Length == 0)
        {
            // OnValidate can run while Unity is rebuilding a serialized array.
            // Leave editor-time initialization alone; Awake will still seed a
            // genuinely empty runtime caster.
            if (!Application.isPlaying)
                return;

            spellbook = CreateDefaultArcanistSpellbook();
            return;
        }

        if (HasArcanistCoreSpellbook(spellbook))
            NormalizeCoreVariantReferencesByName();
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
