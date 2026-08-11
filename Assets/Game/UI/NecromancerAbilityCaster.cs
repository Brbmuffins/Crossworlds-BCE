/// <summary>
/// Necromancer-specific caster. Its spellbook is authored independently in
/// Spell Forge and must never be expanded with another class's legacy entries.
/// </summary>
public class NecromancerAbilityCaster : AbilityCaster
{
    protected override bool ShouldBackfillMissingSpellbookEntries => false;

    protected override void ConfigureInitialSpellbook()
    {
        // Keep the spellbook authored on the Necromancer prefab.
    }
}
