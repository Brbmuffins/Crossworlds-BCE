using UnityEngine;

/// <summary>
/// Marauder-specific ability caster.
///
/// The Marauder owns its serialized spellbook independently so its abilities
/// can be changed without Arcanist spellbook normalization affecting it.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class MarauderAbilityCaster : AbilityCaster
{
    protected override bool ShouldBackfillMissingSpellbookEntries => false;

    protected override void Awake()
    {
        // Imported-model prefab overrides can lose direct component references
        // when Unity rebuilds the FBX-backed prefab. Repair Mirror's required
        // Animator reference before NetworkAnimator runs at the default order.
        Mirror.NetworkAnimator networkAnimator =
            GetComponent<Mirror.NetworkAnimator>();
        if (networkAnimator != null && networkAnimator.animator == null)
        {
            networkAnimator.animator =
                GetComponent<Animator>() ??
                GetComponentInChildren<Animator>(true);
        }

        base.Awake();
    }

    protected override void ConfigureInitialSpellbook()
    {
        // Keep the spellbook authored on the Marauder prefab.
    }
}
