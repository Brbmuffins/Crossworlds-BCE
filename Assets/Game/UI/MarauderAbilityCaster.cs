/// <summary>
/// Marauder-specific ability caster.
///
/// The Marauder owns its serialized spellbook independently so its abilities
/// can be changed without Arcanist spellbook normalization affecting it.
/// </summary>
public class MarauderAbilityCaster : AbilityCaster
{
    protected override bool ShouldBackfillMissingSpellbookEntries => false;

    protected override void ConfigureInitialSpellbook()
    {
        // Keep the spellbook authored on the Marauder prefab.
    }
}
