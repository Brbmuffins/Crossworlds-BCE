using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum AbilityShape { Circle, Cone, Rectangle }
public enum AbilityCategory { Damage, Heal, Support }

// One zone selector for a variant-spell. Cursor distance picks the active referenced spell.
[System.Serializable]
public class AbilityVariant
{
    [HideInInspector]
    public string variantName = "Variant";

    [Header("Spellbook Reference")]
    [Tooltip("When true and no abilityName is set, this variant resolves its payload from spellbookAbilityIndex.")]
    public bool useSpellbookAbilityIndex = false;

    [Tooltip("Optional index of another entry in this AbilityCaster spellbook. Use -1 when using abilityName or leaving the zone unassigned.")]
    public int spellbookAbilityIndex = -1;

    [Tooltip("Optional abilityName of another entry in this AbilityCaster spellbook. When set, this zone resolves that spell's AbilityDef payload.")]
    public string spellbookAbilityName;

    [HideInInspector]
    public Color indicatorTint = new Color(0.2f, 1f, 0.3f, 0.7f);
    [HideInInspector]
    public float healAmount;
    [HideInInspector]
    public float hotTickAmount;
    [HideInInspector]
    public int   hotTicks;
    [HideInInspector]
    [Min(0.1f)] public float hotInterval = 1f;
    [HideInInspector]
    public float shieldAbsorb;
    [HideInInspector]
    public float shieldDuration = 5f;
    [HideInInspector]
    public float damage;
    [HideInInspector]
    public StatusEffectType statusEffect;
    [HideInInspector]
    public float            statusDuration;
    [HideInInspector]
    public float            statusValue;
    [HideInInspector]
    public string targetTag;  // "Player", "Enemy", or "" to use ability's default
    [HideInInspector]
    public GameObject castVFX;
    [HideInInspector]
    public GameObject hitVFX;
}

[System.Serializable]
public class AbilityDef
{
    public string abilityName = "Ability";

    [Header("Spellbook Visibility")]
    [Tooltip("When true, this spell can be referenced by another spell's variants but is hidden from the spellbook UI and cannot be equipped directly.")]
    public bool variantOnly = false;

    [Header("Variant Visuals")]
    [Tooltip("Tint used when this spell is selected as another spell's active variant zone.")]
    public Color variantIndicatorTint = new Color(0.2f, 1f, 0.3f, 0.7f);

    public AbilityShape shape = AbilityShape.Circle;
    public AbilityCategory category = AbilityCategory.Damage;
    public float range = 4f;
    public float coneAngle = 60f;
    public float rectWidth = 1.5f;
    public float indicatorSize = 1.5f;
    public bool spawnTurret = false;
    public GameObject turretPrefab;
    public float cooldown = 3f;
    public Sprite icon;

    [Header("Spell Timing")]
    [Tooltip("Seconds after committing the aim before this spell fires. Moving during this window cancels the cast without starting cooldown.")]
    [Min(0f)] public float castTime = 0.6f;

    [Header("Charge")]
    public bool chargeable = false;
    public float maxChargeTime = 1.5f;
    public float damage = 10f;
    public float maxChargeDamage = 10f;
    public float maxChargeSizeMultiplier = 1.8f;
    public string targetTag = "Enemy";

    public Color chargedTint = new Color(0f, 0f, 0f, 0f);
    public bool fireVisual = false;

    [Header("VFX Prefabs")]
    public GameObject castVFX;
    public GameObject hitVFX;

    [Header("Shield")]
    public float shieldAbsorb   = 0f;
    public float shieldDuration = 0f;

    [Header("Heal")]
    public float healAmount = 0f;          // Field Repair single-target heal

    [Header("Heal over Time")]
    public float hotTickAmount = 0f;
    public int   hotTicks = 0;
    [Min(0.1f)] public float hotInterval = 1f;

    [Header("Status on hit")]
    public StatusEffectType statusEffect;
    public float            statusDuration;
    public float            statusValue;

    [Header("Timed Effects")]
    public float activeDuration = 0f;      // Phase Cloak, Siege Mode, Iron Tether, Transfer Protocol

    [Header("Chain Lightning")]
    public int   chainTargets       = 0;   // Arc Lance: 4
    public float chainDamageFalloff = 5f;  // damage lost per jump

    [Header("Pull / Zone")]
    public float pullRadius   = 0f;        // Magnetize, Singularity, Event Horizon
    public float pullDuration = 0f;        // Singularity pull phase

    [Header("Pulse Damage")]
    [UnityEngine.Serialization.FormerlySerializedAs("overridePulseSettings")]
    [Tooltip("Turn on to add pulse damage to this spell, or to customize built-in pulses for spells like Void Maw.")]
    public bool usePulseDamage = false;
    [Tooltip("How many damage pulses happen after the spell lands. Set to 0 to disable custom pulses.")]
    [Min(0)] public int pulseCount = 0;
    [Tooltip("Seconds between each pulse. Leave at 0 to use the default 1 second.")]
    [Min(0f)] public float pulseInterval = 0f;
    [Tooltip("Damage radius for each pulse. Leave at 0 to use half of the spell indicator size.")]
    [Min(0f)] public float pulseRadius = 0f;
    [Tooltip("Damage dealt by each pulse before character damage bonuses. Leave at 0 to use this spell's Damage value.")]
    [Min(0f)] public float pulseDamage = 0f;
    [Tooltip("How long each pulse hit VFX stays visible. Leave at 0 to use the default.")]
    [Min(0f)] public float pulseVFXLifetime = 0f;

    [Header("Deployable Scene Prefab")]
    // The runtime object spawned in the world by this ability (mine, wall, zone, etc.)
    public GameObject deployablePrefab;

    [Header("Charge Variants (cursor distance selects referenced spellbook zone)")]
    [Tooltip("2-4 zones ordered near-to-far. Each zone must reference another spellbook entry for its behavior.")]
    public AbilityVariant[] variants;
}

public class AbilityCaster : NetworkBehaviour
{
    const string DefaultDecalShaderName = "Shader Graphs/Decal";
    const float RectFillAlpha = 0.14f;
    const float RectFillChargedAlpha = 0.24f;
    const float RectDecalAlpha = 0.35f;
    const string DefaultDecalMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Decal.mat";
    const int ArcaneStepPulseCount = 4;
    const float ArcaneStepPulseInterval = 1f;
    const int VoidMawPulseCount = 4;
    const float VoidMawPulseInterval = 1f;
    const float DefaultVoidMawPulseVFXLifetime = 1f;
    const int ConeArcSegments = 36;
    const int ConeRadialSegments = 12;
    const int RectCornerCount = 4;
    static readonly Vector3 GroundDecalPivot = new Vector3(0f, 0f, 0.5f);
    static bool s_warnedInvalidRectDecal;
    static bool s_loggedRectIndicatorPath;
    static readonly string[] SharedClassAbilityNames =
    {
        "Runic Sentinel",
        "Void Bolt",
        "Mending Circle",
        "Storm Lash",
        "Ember Surge",
        "Mind Spike",
        "Binding Wave",
        "Arcane Ward",
    };
    static readonly string[] WardenAbilityNames =
    {
        "Runic Snare",
        "Battle Hymn",
        "Spirit Redirect",
        "Mend",
        "Conjurer's Surge",
        "Thorn Volley",
        "Earth Surge",
        "Vine Grasp",
    };
    static readonly string[] IroncladAbilityNames =
    {
        "Counter Blow",
        "Gravity Slam",
        "Shieldwall Charge",
        "Stalwart Stance",
        "Rune Chain",
        "Iron Rampart",
        "Hammer Strike",
        "War Cry",
        "Juggernaut Rush",
    };
    static readonly string[] ArcanistAbilityNames =
    {
        "Arcane Step",
        "Void Maw",
        "Forked Lightning",
        "Collapsing Void",
        "Ether Lance",
        "Conflagration Cone",
        "Ember Beam",
        "Ice Spikes",
        "Ice Guardian",
        "Meteor Shower",
        "Fireball",
        "Chain Lightning",
        "Frost Nova",
    };
    static readonly string[] ClericAbilityNames =
    {
        "Soul Bond",
        "Spirit Wisps",
        "Divine Spark",
        "Sacred Aegis",
        "Dispel",
        "Temporal Grace",
        "Healing Cone",
        "Mending Beam",
        "Holy Bolt",
        "Divine Shield",
        "Smite",
    };
    static readonly string[] ShadowbladeAbilityNames =
    {
        "Shadow Veil",
        "Silence Ward",
        "Dark Harvest",
        "Dark Mark",
        "Fan of Blades",
        "Blade Flurry",
        "Poison Cloud",
        "Death Strike",
    };
    static readonly string[] WardenDefaultAbilityNames = { "Runic Sentinel", "Runic Snare", "Battle Hymn", "Mend" };
    static readonly string[] IroncladDefaultAbilityNames = { "Arcane Ward", "Shieldwall Charge", "Stalwart Stance", "Iron Rampart" };
    static readonly string[] ClericDefaultAbilityNames = { "Healing Cone", "Mending Beam", "Sacred Aegis", "Temporal Grace" };
    static readonly string[] ShadowbladeDefaultAbilityNames = { "Fan of Blades", "Dark Mark", "Dark Harvest", "Shadow Veil" };

    public Camera cam;
    public CastAnimator castAnimator;

    [Header("Cast Time")]
    [Tooltip("Horizontal movement beyond this distance (metres) interrupts a committed cast. Set high enough to tolerate Mirror position corrections on a dedicated server (~0.5 m recommended).")]
    [SerializeField, Min(0f)] float castMoveInterruptDistance = 0.5f;
    [Tooltip("Briefly freezes player movement when an aim is committed so leftover physics drift cannot move the locked spell target.")]
    [SerializeField, Min(0f)] float commitMovementLockDuration = 0.08f;

    [Header("Class")]
    [Tooltip("Assign the chosen class's ClassAbilityPool asset before play starts.")]
    public ClassAbilityPool classPool;

    [Header("Ability Handlers — assign if your class uses these abilities")]
    public KineticReversalHandler kineticReversalHandler;
    public SiegeModeHandler       siegeModeHandler;
    public DashHandler            dashHandler;
    public StealthHandler         stealthHandler;
    public TransferProtocolHandler transferProtocolHandler;
    public IronTetherHandler      ironTetherHandler;

    [Header("Deployable Prefabs — assign the matching runtime prefabs")]
    [Tooltip("ShockMineBehaviour prefab (Runic Snare) — rune burst trap")]
    public GameObject shockMinePrefab;
    [Tooltip("NaniteSwarmBehaviour prefab (Spirit Wisps) — healing orb cloud")]
    public GameObject naniteSwarmPrefab;
    [Tooltip("SingularityBehaviour prefab (Void Maw ability)")]
    public GameObject singularityPrefab;
    [Tooltip("SingularityBehaviour prefab with applyExposed=true (Collapsing Void)")]
    public GameObject eventHorizonPrefab;
    [Tooltip("LastBastionWall prefab (Iron Rampart) — stone rune wall")]
    public GameObject lastBastionPrefab;
    [Tooltip("NullFieldZone prefab (Silence Ward) — curse fog")]
    public GameObject nullFieldPrefab;

    [Header("Cleric VFX")]
    [Tooltip("ClericHealVFX component on the Cleric prefab — triggers particle burst on heal casts")]
    public ClericHealVFX healVFX;

    /// <summary>Raised on the local client whenever a heal-category ability fires.</summary>
    public event System.Action OnHealCast;

    [Header("Class Deployables")]
    [Tooltip("RestorationBeacon (Cleric) or BastionNode (Ironclad)")]
    public GameObject beaconPrefab;
    [Tooltip("PhaseRelayDeployable (Arcanist)")]
    public GameObject phaseRelayPrefab;
    [Tooltip("ShadowRelayDeployable (Shadowblade)")]
    public GameObject shadowRelayPrefab;

    [Header("Mouse Aim")]
    public float minimumAimDistance = 1f;

    [Header("Variant Selection")]
    [Tooltip("When ON: scroll wheel steps through zone variants while aiming. When OFF: cursor distance from caster picks the zone.")]
    public bool useScrollWheelVariants = false;

    [Header("Ground Projection")]
    [SerializeField] float indicatorGroundOffset = 0.02f;
    [SerializeField] float indicatorRaycastHeight = 12f;
    [SerializeField] float indicatorRaycastDistance = 40f;
    [SerializeField] float indicatorDecalProjectionDepth = 8f;
    [SerializeField] Material indicatorDecalMaterial;

    [Header("Indicator Textures")]
    [Tooltip("Damage circle indicator — try MagicCircle10 / MagicCircle17 / Circle17")]
    public Texture2D indicatorTextureDamage;
    [Tooltip("Heal circle indicator — try SnowCircle / MagicCircle14")]
    public Texture2D indicatorTextureHeal;
    [Tooltip("Support circle indicator — try Circle42 / MagicCircle13")]
    public Texture2D indicatorTextureSupport;

    [Header("Spellbook — all available spells")]
    public AbilityDef[] spellbook = new AbilityDef[]
    {
        // ── SHARED / CROSS-CLASS (indices 0–7) ─────────────────────────────────────────
        new AbilityDef { abilityName = "Runic Sentinel",   shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 10f, indicatorSize = 1.5f, spawnTurret = true, cooldown = 6f },
        new AbilityDef { abilityName = "Void Bolt",        shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 8f, coneAngle = 60f, cooldown = 3f, chargeable = true, maxChargeTime = 1.5f, damage = 10f, maxChargeDamage = 30f, maxChargeSizeMultiplier = 1.6f, targetTag = "Enemy", chargedTint = new Color(0.4f, 0.1f, 0.8f, 0.9f) },
        new AbilityDef { abilityName = "Mending Circle",   shape = AbilityShape.Circle,    category = AbilityCategory.Heal,    range = 6f, indicatorSize = 3f, cooldown = 5f },
        new AbilityDef { abilityName = "Storm Lash",       shape = AbilityShape.Rectangle, category = AbilityCategory.Damage,  range = 10f, rectWidth = 1.5f, cooldown = 4f, chargeable = true, maxChargeTime = 1.5f, damage = 15f, maxChargeDamage = 50f, maxChargeSizeMultiplier = 1.8f, targetTag = "Enemy" },
        new AbilityDef { abilityName = "Ember Surge",      shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 12f, indicatorSize = 2f, cooldown = 4f, chargeable = true, maxChargeTime = 1.5f, damage = 20f, maxChargeDamage = 45f, maxChargeSizeMultiplier = 2f, targetTag = "Enemy", chargedTint = new Color(1f, 0.4f, 0.05f, 0.9f) },
        new AbilityDef { abilityName = "Mind Spike",       shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 10f, indicatorSize = 2.5f, cooldown = 5f, damage = 35f, targetTag = "Enemy" },
        new AbilityDef { abilityName = "Binding Wave",     shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 5f, indicatorSize = 5f, cooldown = 6f, damage = 15f, targetTag = "Enemy" },
        new AbilityDef { abilityName = "Arcane Ward",      shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f, indicatorSize = 1f, cooldown = 8f, shieldAbsorb = 50f, shieldDuration = 5f },

        // ── WARDEN (indices 8–12) ───────────────────────────────────────────────────────
        // [8]  Runic Snare — proximity burst rune trap; Warden and Shadowblade
        new AbilityDef { abilityName = "Runic Snare",      shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 8f, indicatorSize = 1f, cooldown = 5f, damage = 40f, targetTag = "Enemy" },
        // [9]  Battle Hymn — team CDR aura; instant self-cast
        new AbilityDef { abilityName = "Battle Hymn",      shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f, indicatorSize = 6f, cooldown = 12f },
        // [10] Spirit Redirect — redirect active Runic Sentinel onto focus target
        new AbilityDef { abilityName = "Spirit Redirect",  shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 12f, indicatorSize = 1f, cooldown = 8f },
        // [11] Mend — single-target direct heal + debuff cleanse
        new AbilityDef { abilityName = "Mend",             shape = AbilityShape.Circle,    category = AbilityCategory.Heal,    range = 6f, indicatorSize = 1f, cooldown = 6f },
        // [12] Conjurer's Surge (Warden Ultimate) — all constructs activate at full power simultaneously
        new AbilityDef { abilityName = "Conjurer's Surge", shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f, indicatorSize = 1f, cooldown = 45f },

        // ── IRONCLAD (indices 13–18) ───────────────────────────────────────────────────
        // [13] Counter Blow — absorb damage for 3s, release as cone burst up to 60 dmg
        new AbilityDef { abilityName = "Counter Blow",     shape = AbilityShape.Cone,      category = AbilityCategory.Support, range = 8f, coneAngle = 70f, cooldown = 10f, damage = 60f, targetTag = "Enemy" },
        // [14] Gravity Slam — pull all enemies in radius to anchor point, no damage
        new AbilityDef { abilityName = "Gravity Slam",     shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 10f, indicatorSize = 4f, cooldown = 7f },
        // [15] Shieldwall Charge — charge forward 6 units, 25 dmg, stagger + 3 Threat stacks
        new AbilityDef { abilityName = "Shieldwall Charge",shape = AbilityShape.Rectangle, category = AbilityCategory.Damage,  range = 6f, rectWidth = 2f, cooldown = 6f, damage = 25f, targetTag = "Enemy" },
        // [16] Stalwart Stance — stationary stance: 40% DR + 3x Threat generation for 6s
        new AbilityDef { abilityName = "Stalwart Stance",  shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f, indicatorSize = 1f, cooldown = 14f },
        // [17] Rune Chain — leash one enemy within 8 units for 5s; absorb 15% of their attacks on allies
        new AbilityDef { abilityName = "Rune Chain",       shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 8f, indicatorSize = 1f, cooldown = 9f },
        // [18] Iron Rampart (Ironclad Ultimate) — full-width stone rune wall, blocks projectiles 10s
        new AbilityDef { abilityName = "Iron Rampart",     shape = AbilityShape.Rectangle, category = AbilityCategory.Support, range = 8f, rectWidth = 8f, cooldown = 50f },

        // ── ARCANIST (indices 19–22) ───────────────────────────────────────────────────
        // [19] Arcane Step — teleport up to 10 units in aimed direction
        new AbilityDef { abilityName = "Arcane Step",      shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 10f, indicatorSize = 3.5f, cooldown = 4f, castTime = 0.25f, damage = 10f, targetTag = "Enemy", pulseCount = ArcaneStepPulseCount, pulseInterval = ArcaneStepPulseInterval, pulseDamage = 10f },
        // [20] Void Maw — pull enemies to center and pulse damage 4 times
        new AbilityDef { abilityName = "Void Maw",         shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 10f, indicatorSize = 8f, cooldown = 9f, damage = 20f, targetTag = "Enemy", pulseCount = VoidMawPulseCount, pulseInterval = VoidMawPulseInterval, pulseDamage = 20f, pulseVFXLifetime = DefaultVoidMawPulseVFXLifetime },
        // [21] Forked Lightning — chain lightning, jumps up to 4 enemies (30/25/20/15 dmg)
        new AbilityDef { abilityName = "Forked Lightning",  shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 10f, indicatorSize = 1.5f, cooldown = 7f, damage = 30f, targetTag = "Enemy" },
        // [22] Collapsing Void (Arcanist Ultimate) — 12-unit pull, 3s collapse, 60 AoE + Weakened window
        new AbilityDef { abilityName = "Collapsing Void",  shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 14f, indicatorSize = 12f, cooldown = 50f, damage = 60f, targetTag = "Enemy" },

        // ── CLERIC (indices 23–28) ─────────────────────────────────────────────────────
        // [23] Soul Bond — tether ally: their incoming damage reroutes to you for 5s
        new AbilityDef { abilityName = "Soul Bond",        shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 8f, indicatorSize = 1f, cooldown = 9f },
        // [24] Spirit Wisps — mobile healing orbs, drift toward ally, chip enemies they pass through
        new AbilityDef { abilityName = "Spirit Wisps",     shape = AbilityShape.Circle,    category = AbilityCategory.Heal,    range = 10f, indicatorSize = 2f, cooldown = 7f },
        // [25] Divine Spark — revive downed ally at 30% HP OR 60 burst dmg to undead enemies
        new AbilityDef { abilityName = "Divine Spark",     shape = AbilityShape.Circle,    category = AbilityCategory.Heal,    range = 6f, indicatorSize = 1.5f, cooldown = 14f, damage = 60f, targetTag = "Enemy" },
        // [26] Sacred Aegis — shield on ally that scales 20→80 absorb as they take hits over 8s
        new AbilityDef { abilityName = "Sacred Aegis",     shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 8f, indicatorSize = 1f, cooldown = 10f, shieldAbsorb = 20f, shieldDuration = 8f },
        // [27] Dispel — instant cleanse all debuffs from target ally
        new AbilityDef { abilityName = "Dispel",           shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 8f, indicatorSize = 1f, cooldown = 7f },
        // [28] Temporal Grace (Cleric Ultimate) — rewind entire team 5 seconds: HP, position, debuffs
        new AbilityDef { abilityName = "Temporal Grace",   shape = AbilityShape.Circle,    category = AbilityCategory.Heal,    range = 0f, indicatorSize = 1f, cooldown = 60f },

        // ── SHADOWBLADE (indices 29–31) ────────────────────────────────────────────────
        // [29] Shadow Veil — full invisibility for 4s; breaking with Mind Spike = +50% damage
        new AbilityDef { abilityName = "Shadow Veil",      shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f, indicatorSize = 1f, cooldown = 10f },
        // [30] Silence Ward — silence all enemy abilities in radius for 5s
        new AbilityDef { abilityName = "Silence Ward",     shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 10f, indicatorSize = 5f, cooldown = 12f },
        // [31] Dark Harvest (Shadowblade Ultimate) — consume all active debuffs on enemies in range: 20 dmg per stack
        new AbilityDef { abilityName = "Dark Harvest",     shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 8f, indicatorSize = 8f, cooldown = 40f, damage = 20f, targetTag = "Enemy" },
        // [32] Dark Mark (Shadowblade) — a cursed burst at the target point; sets up Dark Harvest
        new AbilityDef { abilityName = "Dark Mark",        shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 9f, indicatorSize = 1.5f, cooldown = 6f, damage = 20f, targetTag = "Enemy" },
        // [33] Fan of Blades (Shadowblade) — close cone of shadow blades; hold to charge
        new AbilityDef { abilityName = "Fan of Blades",    shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 6f, coneAngle = 70f, cooldown = 5f, chargeable = true, maxChargeTime = 1.2f, damage = 12f, maxChargeDamage = 30f, maxChargeSizeMultiplier = 1.4f, targetTag = "Enemy", chargedTint = new Color(0.4f, 0.1f, 0.8f, 0.9f) },
        // [34] Ether Lance (Arcanist) — piercing line of void energy; hold to charge
        new AbilityDef { abilityName = "Ether Lance",      shape = AbilityShape.Rectangle, category = AbilityCategory.Damage,  range = 12f, rectWidth = 1.2f, cooldown = 5f, chargeable = true, maxChargeTime = 1.5f, damage = 15f, maxChargeDamage = 40f, maxChargeSizeMultiplier = 1.6f, targetTag = "Enemy" },
        // [35] Healing Cone (Cleric) — healing cone with multi-layer sweet spots: HPS / Burst (close), HoT (mid), Shield (far)
        new AbilityDef { abilityName = "Healing Cone",     shape = AbilityShape.Cone,      category = AbilityCategory.Heal,    range = 10f, coneAngle = 60f, cooldown = 5f, targetTag = "Player", healAmount = 25f, shieldAbsorb = 30f },
        // [36] Mending Beam (Cleric) — healing beam with multi-layer sweet spots
        new AbilityDef { abilityName = "Mending Beam",     shape = AbilityShape.Rectangle, category = AbilityCategory.Heal,    range = 12f, rectWidth = 2.0f, cooldown = 6f, targetTag = "Player", healAmount = 25f, shieldAbsorb = 30f },
        // [37] Conflagration Cone (Arcanist) — fire cone with multi-layer sweet spots: Burst (close), Burn DoT (mid), Slow/Weakened (far)
        new AbilityDef { abilityName = "Conflagration Cone", shape = AbilityShape.Cone,     category = AbilityCategory.Damage,  range = 10f, coneAngle = 60f, cooldown = 5f, targetTag = "Enemy", damage = 20f },
        // [38] Ember Beam (Arcanist) — fire beam with multi-layer sweet spots
        new AbilityDef { abilityName = "Ember Beam",       shape = AbilityShape.Rectangle, category = AbilityCategory.Damage,  range = 12f, rectWidth = 2.0f, cooldown = 6f, targetTag = "Enemy", damage = 25f },

        // [39] Ice Spikes (Arcanist) — Hangdanger: ground-eruption cone, slow + burst; changed rect→cone
        new AbilityDef { abilityName = "Ice Spikes",       shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 10f, coneAngle = 60f, cooldown = 5f, targetTag = "Enemy", damage = 20f,
            castVFX = null, hitVFX = null },  // assign Ice freeze skill VFX via SpellIconAssigner / Inspector

        // [40] Meteor Shower (Arcanist) — Hangdanger: delayed AoE bombardment circle; large radius
        new AbilityDef { abilityName = "Meteor Shower",    shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 14f, indicatorSize = 8f, cooldown = 30f, targetTag = "Enemy", damage = 30f },

        // ── NEW ARCANIST ──────────────────────────────────────────────────────────────
        // [41] Fireball — cone burst: V0 quick shot, V1 triple burst, V2 inferno nova
        new AbilityDef { abilityName = "Fireball",          shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 10f, coneAngle = 55f,  cooldown = 4f,  targetTag = "Enemy", damage = 25f, chargeable = true, maxChargeTime = 1.2f, maxChargeDamage = 45f, maxChargeSizeMultiplier = 1.7f, chargedTint = new Color(1f,0.35f,0f,0.9f) },
        // [42] Chain Lightning — circle burst: V0 arc, V1 chain, V2 thunderstorm
        new AbilityDef { abilityName = "Chain Lightning",   shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 10f, indicatorSize = 3f, cooldown = 5f, targetTag = "Enemy", damage = 30f, chargeable = true, maxChargeTime = 1.5f, maxChargeDamage = 55f, maxChargeSizeMultiplier = 2.0f, chargedTint = new Color(0.3f,0.8f,1f,0.9f) },
        // [43] Frost Nova — circle AoE control: V0 chill, V1 freeze, V2 blizzard
        new AbilityDef { abilityName = "Frost Nova",        shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 8f,  indicatorSize = 4f, cooldown = 6f, targetTag = "Enemy", damage = 15f },

        // ── NEW WARDEN ────────────────────────────────────────────────────────────────
        // [44] Thorn Volley — cone: V0 single thorn, V1 3-shot volley, V2 briar storm
        new AbilityDef { abilityName = "Thorn Volley",      shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 9f,  coneAngle = 50f,  cooldown = 4f,  targetTag = "Enemy", damage = 20f, chargeable = true, maxChargeTime = 1.0f, maxChargeDamage = 36f, maxChargeSizeMultiplier = 1.5f, chargedTint = new Color(0.1f,0.7f,0.1f,0.9f) },
        // [45] Earth Surge — circle: V0 tremor, V1 quake, V2 fissure
        new AbilityDef { abilityName = "Earth Surge",       shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 8f,  indicatorSize = 3f, cooldown = 5f, targetTag = "Enemy", damage = 20f },
        // [46] Vine Grasp — circle: V0 root, V1 stranglehold, V2 forest prison
        new AbilityDef { abilityName = "Vine Grasp",        shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 9f,  indicatorSize = 2f, cooldown = 7f, targetTag = "Enemy", damage = 10f },

        // ── NEW IRONCLAD ─────────────────────────────────────────────────────────────
        // [47] Hammer Strike — rect: V0 quick blow, V1 heavy slam, V2 seismic slam
        new AbilityDef { abilityName = "Hammer Strike",     shape = AbilityShape.Rectangle, category = AbilityCategory.Damage,  range = 5f,  rectWidth = 2.5f, cooldown = 4f,  targetTag = "Enemy", damage = 30f, chargeable = true, maxChargeTime = 1.5f, maxChargeDamage = 70f, maxChargeSizeMultiplier = 1.8f, chargedTint = new Color(1f,0.5f,0.0f,0.9f) },
        // [48] War Cry — circle aura: V0 shout heal, V1 rally shield, V2 primal roar
        new AbilityDef { abilityName = "War Cry",           shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f,  indicatorSize = 6f, cooldown = 10f, targetTag = "Player", healAmount = 30f },
        // [49] Juggernaut Rush — rect charge: V0 dash, V1 bull rush, V2 wrecking ball
        new AbilityDef { abilityName = "Juggernaut Rush",   shape = AbilityShape.Rectangle, category = AbilityCategory.Damage,  range = 7f,  rectWidth = 2f,   cooldown = 6f,  targetTag = "Enemy", damage = 25f, chargeable = true, maxChargeTime = 1.2f, maxChargeDamage = 60f, maxChargeSizeMultiplier = 2.0f, chargedTint = new Color(1f,0.6f,0.1f,0.9f) },

        // ── NEW SHADOWBLADE ───────────────────────────────────────────────────────────
        // [50] Blade Flurry — cone: V0 slash, V1 flurry x3, V2 maelstrom x5
        new AbilityDef { abilityName = "Blade Flurry",      shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 6f,  coneAngle = 65f,  cooldown = 4f,  targetTag = "Enemy", damage = 25f, chargeable = true, maxChargeTime = 1.0f, maxChargeDamage = 50f, maxChargeSizeMultiplier = 1.6f, chargedTint = new Color(0.5f,0f,0.8f,0.9f) },
        // [51] Poison Cloud — circle: V0 mist, V1 miasma, V2 death fog
        new AbilityDef { abilityName = "Poison Cloud",      shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 8f,  indicatorSize = 3f, cooldown = 6f, targetTag = "Enemy", damage = 10f },
        // [52] Death Strike — circle: V0 stab, V1 deep cut, V2 assassination
        new AbilityDef { abilityName = "Death Strike",      shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 7f,  indicatorSize = 1.2f, cooldown = 7f, targetTag = "Enemy", damage = 40f },

        // ── NEW CLERIC ────────────────────────────────────────────────────────────────
        // [53] Holy Bolt — cone heal: V0 flash, V1 radiance, V2 divine ray
        new AbilityDef { abilityName = "Holy Bolt",         shape = AbilityShape.Cone,      category = AbilityCategory.Heal,    range = 10f, coneAngle = 55f,  cooldown = 4f,  targetTag = "Player", healAmount = 30f },
        // [54] Divine Shield — circle: V0 shelter, V1 bastion, V2 cathedral
        new AbilityDef { abilityName = "Divine Shield",     shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f,  indicatorSize = 5f, cooldown = 12f, targetTag = "Player", shieldAbsorb = 40f, shieldDuration = 5f },
        // [55] Smite — cone holy damage: V0 strike, V1 judgement, V2 wrath
        new AbilityDef { abilityName = "Smite",             shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 9f,  coneAngle = 60f,  cooldown = 5f,  targetTag = "Enemy", damage = 35f, chargeable = true, maxChargeTime = 1.2f, maxChargeDamage = 75f, maxChargeSizeMultiplier = 1.8f, chargedTint = new Color(1f,0.95f,0.3f,0.9f) },
    };

    [Header("Equipped slots (indices into spellbook)")]
    public int[] equippedIndices = new int[] { 0, 1, 2, 3 };

    // The 4 active ability slots — derived from spellbook via equippedIndices at runtime.
    // Shown read-only in Inspector for debugging; do not edit here, edit spellbook above.
    [SerializeField, HideInInspector] private AbilityDef[] _equippedAbilities = new AbilityDef[4];
    public AbilityDef[] abilities => _equippedAbilities;

    private int heldAbilityIndex = -1;
    private GameObject activeIndicator;
    private GameObject _rangeRingGO;
    private float aimTimer = 0f;
    private Coroutine committedCastRoutine;
    private int committedCastSlot = -1;
    private GameObject committedCastIndicator;
    private AbilityDef committedCastAbility;
    private int committedCastVariantIndex = -1;
    private float committedCastDuration;
    private float committedCastElapsed;

    // Read by CameraFollow to suspend orbit while an indicator is active
    public static bool    IsAimingLocally { get; private set; }
    // Read by PlayerMovement to face the cursor during aim (Smite-style)
    public static Vector3 AimDirection    { get; private set; }
    private float[] cooldownTimers = new float[4];

    private GameObject activeShieldVFX;
    private float shieldVFXTimer = 0f;

    // Variant zone selection — updated every frame while aiming, snapshotted at commit.
    private int   _activeVariantIndex = 0;
    private float _currentAimFraction = 0f;

    // ── Cached component refs ──────────────────────────────────────
    private ClassPassive         _passive;
    private PassivePhaseCharge   _phaseCharge;
    private PassiveBountySystem  _bounty;
    private Health               _health;
    private CharacterStats       _characterStats;  // gear/attunement bonuses

    public int HeldAbilityIndex    => heldAbilityIndex;
    public int ActiveVariantIndex  => _activeVariantIndex;
    public bool IsCommittedCasting => committedCastRoutine != null && committedCastAbility != null && committedCastDuration > 0f;
    public string CommittedCastName => committedCastAbility != null ? committedCastAbility.abilityName : "";
    public int CommittedCastVariantIndex => IsCommittedCasting ? committedCastVariantIndex : -1;
    public string CommittedCastVariantName
    {
        get
        {
            return IsCommittedCasting
                ? GetVariantDisplayName(committedCastAbility, committedCastVariantIndex)
                : "";
        }
    }
    public string CommittedCastDisplayName
    {
        get
        {
            string abilityName = CommittedCastName;
            string variantName = CommittedCastVariantName;
            return string.IsNullOrEmpty(variantName) ? abilityName : $"{abilityName} - {variantName}";
        }
    }
    public AbilityCategory CommittedCastCategory
    {
        get
        {
            if (committedCastAbility == null) return AbilityCategory.Damage;
            AbilityDef payload = GetVariantPayload(committedCastAbility, committedCastVariantIndex);
            return payload != null ? payload.category : committedCastAbility.category;
        }
    }
    public float CommittedCastProgress => committedCastDuration > 0f ? Mathf.Clamp01(committedCastElapsed / committedCastDuration) : 0f;
    public float CommittedCastRemaining => Mathf.Max(0f, committedCastDuration - committedCastElapsed);

    public AbilityDef GetVariantPayload(AbilityDef ability, int variantIndex)
    {
        AbilityVariant variant = GetVariant(ability, variantIndex);
        return ResolveVariantSpellbookAbility(ability, variant);
    }

    public string GetVariantDisplayName(AbilityDef ability, int variantIndex)
    {
        AbilityVariant variant = GetVariant(ability, variantIndex);
        if (variant == null) return "";

        AbilityDef payload = ResolveVariantSpellbookAbility(ability, variant);
        if (payload != null && !string.IsNullOrEmpty(payload.abilityName)) return payload.abilityName;
        if (!string.IsNullOrEmpty(variant.spellbookAbilityName)) return variant.spellbookAbilityName;
        if (variant.useSpellbookAbilityIndex && variant.spellbookAbilityIndex >= 0) return $"Spellbook #{variant.spellbookAbilityIndex}";
        return "Missing Variant";
    }

    public Color GetVariantTint(AbilityDef ability, int variantIndex)
    {
        AbilityDef payload = GetVariantPayload(ability, variantIndex);
        if (payload != null) return payload.variantIndicatorTint;

        return new Color(1f, 0.1f, 0.3f, 0.75f);
    }

    protected virtual bool ShouldBackfillMissingSpellbookEntries => true;

    protected virtual void ConfigureInitialSpellbook()
    {
    }

    AbilityVariant GetVariant(AbilityDef ability, int variantIndex)
    {
        if (ability?.variants == null || ability.variants.Length == 0) return null;
        int idx = Mathf.Clamp(variantIndex, 0, ability.variants.Length - 1);
        return ability.variants[idx];
    }

    AbilityDef ResolveVariantSpellbookAbility(AbilityDef owner, AbilityVariant variant)
    {
        if (variant == null || !HasVariantSpellbookReference(variant)) return null;

        AbilityDef payload = null;
        string abilityName = string.IsNullOrEmpty(variant.spellbookAbilityName) ? "" : variant.spellbookAbilityName.Trim();
        if (!string.IsNullOrEmpty(abilityName))
            payload = FindSpellbookAbilityByName(abilityName);

        if (payload == null
            && variant.useSpellbookAbilityIndex
            && variant.spellbookAbilityIndex >= 0
            && spellbook != null
            && variant.spellbookAbilityIndex < spellbook.Length)
            payload = spellbook[variant.spellbookAbilityIndex];

        if (payload == null || ReferenceEquals(payload, owner)) return null;
        return payload;
    }

    static bool HasVariantSpellbookReference(AbilityVariant variant)
    {
        return variant != null
            && ((variant.useSpellbookAbilityIndex && variant.spellbookAbilityIndex >= 0)
                || (!string.IsNullOrEmpty(variant.spellbookAbilityName)
                    && !string.IsNullOrEmpty(variant.spellbookAbilityName.Trim())));
    }

    protected virtual void Awake()
    {
        // Remote-player gating is handled by ShouldProcessLocalInput() in Update.
        // isLocalPlayer is NOT set in Awake (Mirror sets it after instantiation),
        // so any enabled-check here would wrongly disable the local player too.

        ConfigureInitialSpellbook();

        if (ShouldBackfillMissingSpellbookEntries)
            BackfillMissingSpellbookEntries();

        // Seed by ability name for known classes so old prefab spellbook order cannot
        // make class-pool indices point at the wrong spell.
        ApplyDefaultLoadoutFromClassPool();

        useScrollWheelVariants = PlayerPrefs.GetInt("VariantScrollMode", 0) == 1;

        BackfillVariantDefaults();
        MigrateInlineVariantsToSpellbookReferences();
        BackfillVariantVFX();
        SyncEquippedFromSpellbook();
        GenerateProceduralIcons();

        _passive        = GetComponent<ClassPassive>();
        _phaseCharge    = GetComponent<PassivePhaseCharge>();
        _bounty         = GetComponent<PassiveBountySystem>();
        _health         = GetComponent<Health>();
        _characterStats = GetComponent<CharacterStats>();
        if (castAnimator == null)
            castAnimator = GetComponent<CastAnimator>() ?? GetComponentInChildren<CastAnimator>(true);

        // Register this player with SnapshotSystem
        SnapshotSystem.Instance?.Track(gameObject);
    }

    void Start()
    {
        if (!ShouldProcessLocalInput()) return;

        // Cursor is always free — CameraFollow locks it only while right-mouse orbit is
        // active.  AbilityCaster never locks; it only ensures cursor is visible for aim.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Solo / editor play (no Mirror session): wire camera now.
        // In networked play, OnStartLocalPlayer handles this instead.
        if (!NetworkClient.active && !NetworkServer.active)
        {
            Camera sceneCam = Camera.main;
            if (sceneCam != null) WireCamera(sceneCam);
            else StartCoroutine(AcquireCameraRetry());
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        ConfigureInitialSpellbook();

        if (ShouldBackfillMissingSpellbookEntries)
            BackfillMissingSpellbookEntries();

        BackfillSpellBehaviorDefaults();
        BackfillVariantDefaults();
        MigrateInlineVariantsToSpellbookReferences();
        BackfillVariantVFX();
    }

    void BackfillSpellBehaviorDefaults()
    {
        if (spellbook == null) return;

        foreach (AbilityDef ability in spellbook)
        {
            if (ability == null || ability.usePulseDamage)
                continue;

            switch (ability.abilityName)
            {
                case "Arcane Step":
                    BackfillPulseDefaults(
                        ability,
                        ArcaneStepPulseCount,
                        ArcaneStepPulseInterval,
                        ability.damage > 0f ? ability.damage : 10f,
                        0f);
                    break;

                case "Void Maw":
                    BackfillPulseDefaults(
                        ability,
                        VoidMawPulseCount,
                        VoidMawPulseInterval,
                        ability.damage > 0f ? ability.damage : 20f,
                        DefaultVoidMawPulseVFXLifetime);
                    break;
            }
        }
    }

    static void BackfillPulseDefaults(AbilityDef ability, int count, float interval, float damage, float vfxLifetime)
    {
        if (ability.pulseCount <= 0) ability.pulseCount = count;
        if (ability.pulseInterval <= 0f) ability.pulseInterval = interval;
        if (ability.pulseDamage <= 0f) ability.pulseDamage = damage;
        if (ability.pulseVFXLifetime <= 0f) ability.pulseVFXLifetime = vfxLifetime;
    }
#endif

    // Mirror fires this exactly once per local player object, after isLocalPlayer is
    // confirmed — the safe place for any local-player-only setup.
    // Also handles the DontDestroyOnLoad carry-over case: if the player persists across
    // scenes, sceneLoaded re-runs AcquireCamera to grab the new scene's camera.
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        AcquireCamera();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (this == null)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            return;
        }

        // Scene is fully loaded by the time this callback fires.
        Camera sceneCam = Camera.main;
        if (sceneCam != null)
            WireCamera(sceneCam);
        else
            StartCoroutine(AcquireCameraRetry());
    }

    void AcquireCamera()
    {
        Camera sceneCam = Camera.main;
        if (sceneCam == null)
        {
            // Camera may not be loaded yet (race during scene transition).
            // CameraFollow's self-heal coroutine will pick up the player anyway,
            // but start a retry so PlayerMovement.cam is also wired correctly.
            StartCoroutine(AcquireCameraRetry());
            return;
        }

        WireCamera(sceneCam);
    }

    System.Collections.IEnumerator AcquireCameraRetry()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new UnityEngine.WaitForSeconds(0.2f);
            Camera sceneCam = Camera.main;
            if (sceneCam != null) { WireCamera(sceneCam); yield break; }
        }
        Debug.LogWarning("[AbilityCaster] AcquireCamera: Camera.main still null after retries.");
    }

    void WireCamera(Camera sceneCam)
    {
        // Wire PlayerMovement.cam so WASD movement is camera-relative.
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.cam = sceneCam;

        // Wire CameraFollow from here (OnStartLocalPlayer) as the authoritative path.
        // PlayerMovement.Start() also does this as a fast path, but Mirror's
        // OnStartLocalPlayer fires after the prefab is fully spawned and isLocalPlayer
        // is confirmed — safer than relying on Start() timing alone.
        var follow = sceneCam.GetComponent<CameraFollow>()
                  ?? FindAnyObjectByType<CameraFollow>()
                  ?? sceneCam.gameObject.AddComponent<CameraFollow>();
        follow.target = transform;
    }

    public void SyncEquippedFromSpellbook()
    {
        _equippedAbilities = new AbilityDef[4];
        for (int i = 0; i < 4; i++)
        {
            int idx = (i < equippedIndices.Length) ? equippedIndices[i] : -1;
            _equippedAbilities[i] = (idx >= 0 && idx < spellbook.Length && spellbook[idx] != null && !spellbook[idx].variantOnly)
                ? spellbook[idx]
                : null;
        }
    }

    public void EquipSpell(int spellbookIndex, int slot)
    {
        if (slot < 0 || slot >= 4) return;
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        if (!IsAllowedByClass(spellbookIndex)) return;

        if (ShouldRouteCastToServer())
            CmdEquipSpell(spellbookIndex, slot);

        if (committedCastSlot == slot)
            CancelCommittedCast();

        if (heldAbilityIndex == slot)
            CancelAim();

        equippedIndices[slot] = spellbookIndex;
        _equippedAbilities[slot] = spellbook[spellbookIndex];
        cooldownTimers[slot] = 0f;
    }

    [Command]
    void CmdEquipSpell(int spellbookIndex, int slot)
    {
        if (slot < 0 || slot >= 4) return;
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        if (!IsAllowedByClass(spellbookIndex)) return;

        equippedIndices[slot] = spellbookIndex;
        _equippedAbilities[slot] = spellbook[spellbookIndex];
        cooldownTimers[slot] = 0f;
    }

    // Returns true if this spellbook index is permitted for the current class.
    // Always returns true when no classPool is assigned (editor / testing).
    public bool IsAllowedByClass(int spellbookIndex)
    {
        if (IsVariantOnlySpellbookIndex(spellbookIndex)) return false;
        if (classPool == null) return true;

        if (TryGetClassAbilityNames(classPool.className, out string[] classAbilityNames))
        {
            string abilityName = GetSpellbookAbilityName(spellbookIndex);
            if (string.IsNullOrEmpty(abilityName)) return false;
            if (ContainsAbilityName(SharedClassAbilityNames, abilityName)
                || ContainsAbilityName(classAbilityNames, abilityName))
                return true;

            // Known spells are resolved by name to protect stale prefab order. Brand-new
            // custom names can still be enabled through the class pool's index list.
            return !IsKnownAbilityName(abilityName) && PoolContainsIndex(classPool, spellbookIndex);
        }

        return PoolContainsIndex(classPool, spellbookIndex);
    }

    // Apply a class pool and reset to its default loadout.
    public void ApplyClass(ClassAbilityPool pool)
    {
        classPool = pool;
        if (pool == null) return;

        ConfigureInitialSpellbook();

        if (ShouldBackfillMissingSpellbookEntries)
            BackfillMissingSpellbookEntries();

        ApplyDefaultLoadoutFromClassPool();

        SyncEquippedFromSpellbook();
    }

    void ApplyDefaultLoadoutFromClassPool()
    {
        if (classPool == null || equippedIndices == null) return;

        if (TryGetDefaultAbilityNames(classPool.className, out string[] defaultAbilityNames))
        {
            for (int i = 0; i < equippedIndices.Length && i < 4; i++)
            {
                int idx = (i < defaultAbilityNames.Length) ? FindDefaultAbilityIndex(defaultAbilityNames[i]) : -1;
                equippedIndices[i] = idx >= 0 ? idx : GetPoolDefaultIndex(classPool, i);
            }
            return;
        }

        for (int i = 0; i < equippedIndices.Length && i < 4; i++)
            equippedIndices[i] = GetPoolDefaultIndex(classPool, i);
    }

    int FindDefaultAbilityIndex(string abilityName)
    {
        int idx = FindSpellbookIndexByName(abilityName);
        if (idx >= 0) return idx;

        return -1;
    }

    static int GetPoolDefaultIndex(ClassAbilityPool pool, int slot)
    {
        if (pool?.defaultEquipped == null) return -1;
        return slot < pool.defaultEquipped.Length ? pool.defaultEquipped[slot] : -1;
    }

    string GetSpellbookAbilityName(int spellbookIndex)
    {
        if (spellbook == null || spellbookIndex < 0 || spellbookIndex >= spellbook.Length)
            return "";
        return spellbook[spellbookIndex]?.abilityName ?? "";
    }

    bool IsVariantOnlySpellbookIndex(int spellbookIndex)
    {
        return spellbook != null
            && spellbookIndex >= 0
            && spellbookIndex < spellbook.Length
            && spellbook[spellbookIndex] != null
            && spellbook[spellbookIndex].variantOnly;
    }

    int FindSpellbookIndexByName(string abilityName)
    {
        if (spellbook == null || string.IsNullOrEmpty(abilityName)) return -1;
        string searchName = abilityName.Trim();
        for (int i = 0; i < spellbook.Length; i++)
        {
            if (string.Equals(spellbook[i]?.abilityName, searchName, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    AbilityDef FindSpellbookAbilityByName(string abilityName)
    {
        int idx = FindSpellbookIndexByName(abilityName);
        return idx >= 0 ? spellbook[idx] : null;
    }

    static bool TryGetClassAbilityNames(string className, out string[] abilityNames)
    {
        switch (className)
        {
            case "Warden":
                abilityNames = WardenAbilityNames;
                return true;
            case "Ironclad":
                abilityNames = IroncladAbilityNames;
                return true;
            case "Arcanist":
                abilityNames = ArcanistAbilityNames;
                return true;
            case "Cleric":
                abilityNames = ClericAbilityNames;
                return true;
            case "Shadowblade":
                abilityNames = ShadowbladeAbilityNames;
                return true;
            default:
                abilityNames = null;
                return false;
        }
    }

    static bool TryGetDefaultAbilityNames(string className, out string[] abilityNames)
    {
        switch (className)
        {
            case "Warden":
                abilityNames = WardenDefaultAbilityNames;
                return true;
            case "Ironclad":
                abilityNames = IroncladDefaultAbilityNames;
                return true;
            case "Cleric":
                abilityNames = ClericDefaultAbilityNames;
                return true;
            case "Shadowblade":
                abilityNames = ShadowbladeDefaultAbilityNames;
                return true;
            default:
                abilityNames = null;
                return false;
        }
    }

    static bool ContainsAbilityName(string[] abilityNames, string abilityName)
    {
        if (abilityNames == null || string.IsNullOrEmpty(abilityName)) return false;
        for (int i = 0; i < abilityNames.Length; i++)
        {
            if (string.Equals(abilityNames[i], abilityName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    static bool IsKnownAbilityName(string abilityName)
    {
        return ContainsAbilityName(SharedClassAbilityNames, abilityName)
            || ContainsAbilityName(WardenAbilityNames, abilityName)
            || ContainsAbilityName(IroncladAbilityNames, abilityName)
            || ContainsAbilityName(ArcanistAbilityNames, abilityName)
            || ContainsAbilityName(ClericAbilityNames, abilityName)
            || ContainsAbilityName(ShadowbladeAbilityNames, abilityName);
    }

    static bool PoolContainsIndex(ClassAbilityPool pool, int spellbookIndex)
    {
        if (pool?.availableIndices == null) return false;
        foreach (int idx in pool.availableIndices)
            if (idx == spellbookIndex) return true;
        return false;
    }

    public bool IsEquipped(int spellbookIndex, out int slot)
    {
        for (int i = 0; i < equippedIndices.Length; i++)
        {
            if (equippedIndices[i] == spellbookIndex)
            {
                slot = i;
                return true;
            }
        }
        slot = -1;
        return false;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0f)
                cooldownTimers[i] -= Time.deltaTime;
        }

        if (activeShieldVFX != null)
        {
            shieldVFXTimer -= Time.deltaTime;
            if (shieldVFXTimer <= 0f)
            {
                Destroy(activeShieldVFX);
                activeShieldVFX = null;
            }
        }

        if (!ShouldProcessLocalInput())
            return;

        if (IsDowned())
        {
            if (heldAbilityIndex != -1)
                CancelAim();

            if (committedCastRoutine != null)
                CancelCommittedCast();

            return;
        }

        // Smite-style: update AimDirection every frame so the character always faces
        // the cursor regardless of whether an ability indicator is active.
        RefreshAimDirection();

        if (committedCastRoutine != null)
            return;

        for (int i = 0; i < 4; i++)
        {
            if (abilities[i] == null) continue;

            KeyControl key = GetDigitKey(i);
            if (key == null) continue;

            if (key.wasPressedThisFrame && cooldownTimers[i] <= 0f)
            {
                // Self-cast shields skip aiming but still respect cast time.
                if (abilities[i].shieldAbsorb > 0f && abilities[i].range <= 0f)
                {
                    if (heldAbilityIndex != -1) CancelAim();
                    BeginCommittedCast(i, abilities[i], null, 0f, 0);
                }
                else if (heldAbilityIndex == i)
                {
                    CancelAim();
                }
                else
                {
                    if (heldAbilityIndex != -1)
                        CancelAim();

                    heldAbilityIndex = i;
                    aimTimer = 0f;
                    _activeVariantIndex = 0;
                    _currentAimFraction = 0f;
                    activeIndicator = CreateIndicator(abilities[i]);
                    IsAimingLocally = true;

                    // Force cursor free in case camera orbit had it locked
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible   = true;
                }
            }
        }

        if (heldAbilityIndex != -1)
        {
            aimTimer += Time.deltaTime;

            if (activeIndicator != null)
                UpdateIndicatorTransform(abilities[heldAbilityIndex], activeIndicator, aimTimer);

            if (Keyboard.current.escapeKey.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelAim();
            }
            else if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Snapshot the variant index at commit time so it survives the cast-time window.
                BeginCommittedCast(heldAbilityIndex, abilities[heldAbilityIndex], activeIndicator, aimTimer, _activeVariantIndex);

                IsAimingLocally = false;
                heldAbilityIndex = -1;
                activeIndicator = null;
                _activeVariantIndex = 0;
                _currentAimFraction = 0f;
                DestroyRangeRing();
                // Cursor stays free — CameraFollow owns lock state when not aiming
            }
        }
    }

    void CancelAim()
    {
        IsAimingLocally = false;
        if (activeIndicator != null) Destroy(activeIndicator);
        activeIndicator = null;
        DestroyRangeRing();
        heldAbilityIndex = -1;
        _activeVariantIndex = 0;
        _currentAimFraction = 0f;
        // Cursor stays free — CameraFollow resumes ownership
    }

    void BeginCommittedCast(int slot, AbilityDef ability, GameObject indicator, float aimTime, int variantIndex = 0)
    {
        if (ability == null)
        {
            if (indicator != null) Destroy(indicator);
            return;
        }

        if (committedCastRoutine != null)
            CancelCommittedCast();

        RequestCommitMovementLock();
        SnapshotCommittedIndicator(indicator);

        PlayCommittedCastAnimation(ability);
        BroadcastCommittedCastAnimation(ability);

        float castTime = CastTimeFor(ability);
        Debug.Log($"[CastTime] {ability.abilityName} committed with castTime={castTime:0.###}s.", this);
        if (castTime <= 0f)
        {
            if (FinalizeCast(ability, indicator, aimTime, variantIndex))
                StartCooldown(slot, ability);
            else if (indicator != null)
                Destroy(indicator);
            return;
        }

        committedCastSlot = slot;
        committedCastIndicator = indicator;
        committedCastAbility = ability;
        committedCastVariantIndex = NormalizeVariantIndex(ability, variantIndex);
        committedCastDuration = castTime;
        committedCastElapsed = 0f;
        committedCastRoutine = StartCoroutine(CommittedCastRoutine(slot, ability, indicator, aimTime, castTime, variantIndex));
    }

    int NormalizeVariantIndex(AbilityDef ability, int variantIndex)
    {
        if (ability?.variants == null || ability.variants.Length == 0)
            return -1;

        return Mathf.Clamp(variantIndex, 0, ability.variants.Length - 1);
    }

    void RequestCommitMovementLock()
    {
        if (commitMovementLockDuration <= 0f)
            return;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.RequestMovementLock(commitMovementLockDuration);
    }

    void SnapshotCommittedIndicator(GameObject indicator)
    {
        if (indicator == null)
            return;

        indicator.transform.SetParent(null, true);
    }

    System.Collections.IEnumerator CommittedCastRoutine(int slot, AbilityDef ability, GameObject indicator, float aimTime, float castTime, int variantIndex = 0)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < castTime)
        {
            if (WasCommittedCastInterrupted(startPosition))
            {
                Debug.Log($"Cast interrupted: {ability.abilityName}");
                bool preferMovementState = HasMovementInput();
                CancelCommittedCastAnimation(ability, preferMovementState);
                BroadcastCommittedCastAnimationCancelled(ability, preferMovementState);
                if (indicator != null) Destroy(indicator);
                ClearCommittedCast();
                yield break;
            }

            elapsed = Mathf.Min(castTime, elapsed + Time.deltaTime);
            committedCastElapsed = elapsed;
            yield return null;
        }

        committedCastElapsed = castTime;
        bool castStarted = FinalizeCast(ability, indicator, aimTime, variantIndex);
        Debug.Log($"[CastTime] {ability.abilityName} resolved after {castTime:0.###}s.", this);
        if (castStarted)
            StartCooldown(slot, ability);
        else if (indicator != null)
            Destroy(indicator);

        ClearCommittedCast();
    }

    bool WasCommittedCastInterrupted(Vector3 startPosition)
    {
        if (IsDowned())
            return true;

        // On a dedicated server Mirror can nudge the client's position by a few cm
        // (lag-compensation correction). Using key.isPressed caused false interrupts
        // whenever WASD was held at click time. Rely on actual position displacement
        // only — the threshold (Inspector) should be large enough to tolerate jitter.
        Vector3 delta = transform.position - startPosition;
        delta.y = 0f;
        return delta.sqrMagnitude > castMoveInterruptDistance * castMoveInterruptDistance;
    }

    bool IsDowned()
    {
        return _health != null && _health.IsDowned;
    }

    bool HasMovementInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return false;

        return keyboard.wKey.isPressed
            || keyboard.aKey.isPressed
            || keyboard.sKey.isPressed
            || keyboard.dKey.isPressed
            || keyboard.upArrowKey.isPressed
            || keyboard.downArrowKey.isPressed
            || keyboard.leftArrowKey.isPressed
            || keyboard.rightArrowKey.isPressed
            || keyboard.spaceKey.wasPressedThisFrame
            || keyboard.leftAltKey.wasPressedThisFrame
            || keyboard.vKey.wasPressedThisFrame;
    }

    void CancelCommittedCast()
    {
        if (committedCastSlot >= 0 && committedCastSlot < abilities.Length)
        {
            AbilityDef ability = abilities[committedCastSlot];
            CancelCommittedCastAnimation(ability, false);
            BroadcastCommittedCastAnimationCancelled(ability, false);
        }

        if (committedCastRoutine != null)
            StopCoroutine(committedCastRoutine);

        if (committedCastIndicator != null)
            Destroy(committedCastIndicator);

        ClearCommittedCast();
    }

    void ClearCommittedCast()
    {
        committedCastRoutine = null;
        committedCastSlot = -1;
        committedCastIndicator = null;
        committedCastAbility = null;
        committedCastVariantIndex = -1;
        committedCastDuration = 0f;
        committedCastElapsed = 0f;
    }

    void StartCooldown(int slot, AbilityDef ability)
    {
        if (slot < 0 || slot >= cooldownTimers.Length || ability == null) return;
        cooldownTimers[slot] = CooldownFor(ability);
    }

    void PlayCommittedCastAnimation(AbilityDef ability)
    {
        if (ability == null) return;
        castAnimator?.PlayCast(ability.category);
    }

    void BroadcastCommittedCastAnimation(AbilityDef ability)
    {
        int spellbookIndex = FindSpellbookIndex(ability);
        if (spellbookIndex < 0) return;

        if (ShouldRouteCastToServer())
            CmdCommittedCastAnimationStarted(spellbookIndex);
        else if (NetworkServer.active)
            RpcCommittedCastAnimationStarted(spellbookIndex);
    }

    [Command]
    void CmdCommittedCastAnimationStarted(int spellbookIndex)
    {
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        int equippedSlot = FindEquippedSlotForSpellbookIndex(spellbookIndex);
        if (equippedSlot < 0) return;
        if (cooldownTimers[equippedSlot] > 0f) return;

        RpcCommittedCastAnimationStarted(spellbookIndex);
    }

    [ClientRpc]
    void RpcCommittedCastAnimationStarted(int spellbookIndex)
    {
        if (isLocalPlayer) return;
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        PlayCommittedCastAnimation(spellbook[spellbookIndex]);
    }

    void CancelCommittedCastAnimation(AbilityDef ability, bool preferMovementState)
    {
        if (ability == null) return;
        castAnimator?.CancelCast(preferMovementState);
    }

    void BroadcastCommittedCastAnimationCancelled(AbilityDef ability, bool preferMovementState)
    {
        int spellbookIndex = FindSpellbookIndex(ability);
        if (spellbookIndex < 0) return;

        if (ShouldRouteCastToServer())
            CmdCommittedCastAnimationCancelled(spellbookIndex, preferMovementState);
        else if (NetworkServer.active)
            RpcCommittedCastAnimationCancelled(spellbookIndex, preferMovementState);
    }

    [Command]
    void CmdCommittedCastAnimationCancelled(int spellbookIndex, bool preferMovementState)
    {
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        int equippedSlot = FindEquippedSlotForSpellbookIndex(spellbookIndex);
        if (equippedSlot < 0) return;

        RpcCommittedCastAnimationCancelled(spellbookIndex, preferMovementState);
    }

    [ClientRpc]
    void RpcCommittedCastAnimationCancelled(int spellbookIndex, bool preferMovementState)
    {
        if (isLocalPlayer) return;
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        CancelCommittedCastAnimation(spellbook[spellbookIndex], preferMovementState);
    }

    public float GetCooldownFraction(int slot)
    {
        if (slot < 0 || slot >= cooldownTimers.Length) return 0f;
        if (abilities[slot] == null || abilities[slot].cooldown <= 0f) return 0f;

        return Mathf.Clamp01(cooldownTimers[slot] / abilities[slot].cooldown);
    }

    // Seconds of cooldown left on a slot (0 when ready). Used by the HUD countdown.
    public float GetCooldownRemaining(int slot)
    {
        if (slot < 0 || slot >= cooldownTimers.Length) return 0f;
        return Mathf.Max(0f, cooldownTimers[slot]);
    }

    KeyControl GetDigitKey(int index)
    {
        switch (index)
        {
            case 0: return Keyboard.current.digit1Key;
            case 1: return Keyboard.current.digit2Key;
            case 2: return Keyboard.current.digit3Key;
            case 3: return Keyboard.current.digit4Key;
            default: return null;
        }
    }

    bool ShouldProcessLocalInput()
    {
        if (!NetworkClient.active && !NetworkServer.active) return true;
        return isLocalPlayer;
    }

    bool ShouldRouteCastToServer()
    {
        return NetworkClient.active && !NetworkServer.active && isLocalPlayer;
    }

    Vector3 GetCameraAimPoint()
    {
        // Mouse cursor world position — cast a ray from cursor, not screen centre
        Vector2 mp  = Mouse.current.position.ReadValue();
        Ray     ray = cam.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));

        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreIndicatorHit(hit))
                continue;
            return hit.point;
        }

        Plane groundPlane = new Plane(Vector3.up, transform.position);
        if (groundPlane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return transform.position + transform.forward * minimumAimDistance;
    }

    // Called every frame to keep AimDirection current so PlayerMovement can always
    // rotate the character toward the cursor (Smite-style), not just during aim mode.
    void RefreshAimDirection()
    {
        Vector3 tp = GetCameraAimPoint();
        Vector3 to = tp - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 0.001f)
            AimDirection = to.normalized;
    }

    void GetAimData(AbilityDef ability, out Vector3 aimDir, out float aimDistance)
    {
        Vector3 targetPoint = GetCameraAimPoint();
        Vector3 toTarget = targetPoint - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f)
        {
            aimDir = transform.forward;
            aimDir.y = 0f;
            aimDir.Normalize();
            aimDistance = minimumAimDistance;
            AimDirection = aimDir;
            return;
        }

        aimDistance = Mathf.Clamp(toTarget.magnitude, minimumAimDistance, ability.range);
        aimDir = toTarget.normalized;
        AimDirection = aimDir;  // PlayerMovement reads this to face the cursor
    }

    GameObject CreateIndicator(AbilityDef ability)
    {
        Color c = GetCategoryColor(ability.category);
        GameObject indicator = new GameObject("AimIndicator");
        // Parent to player so the indicator moves with them even between Update frames
        indicator.transform.SetParent(transform);

        if (ability.shape == AbilityShape.Cone)
        {
            indicator.AddComponent<ConeAimData>();
            BuildConeOutline(indicator, c);
            BuildProjectedConeFill(indicator, c);

            // Zone arc dividers for variant spells.
            if (ability.variants != null && ability.variants.Length > 1)
            {
                for (int z = 1; z < ability.variants.Length; z++)
                {
                    var arcGO = new GameObject($"ZoneArc_{z}");
                    arcGO.transform.SetParent(indicator.transform, false);
                    var arcLR = arcGO.AddComponent<LineRenderer>();
                    arcLR.useWorldSpace = true;
                    arcLR.loop = false;
                    arcLR.startWidth = arcLR.endWidth = 0.06f;
                    arcLR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    arcLR.receiveShadows = false;
                    arcLR.material = new Material(Shader.Find("Sprites/Default"));
                    arcLR.startColor = arcLR.endColor = new Color(0.5f, 1f, 0.5f, 0.45f);
                    arcLR.positionCount = 0;
                }
            }
        }
        else if (ability.shape == AbilityShape.Circle)
        {
            var disk = CreateCircleIndicator();
            disk.transform.SetParent(indicator.transform, false);
            disk.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // face up
            disk.transform.localScale    = new Vector3(ability.indicatorSize,
                                                       ability.indicatorSize, 1f);

            var rend = disk.GetComponent<Renderer>();
            var mat  = CreateTransparentIndicatorMaterial();
            mat.mainTexture = ProceduralRingTexture;
            SetMaterialTexture(mat, ProceduralRingTexture);
            mat.color       = c;
            rend.material   = mat;

            BuildCircleOutline(indicator, c);
        }
        else // Rectangle
        {
            // Directional shapes read better as an outline than a stretched circle texture
            indicator.AddComponent<RectangleAimData>();
            BuildOutlineLR(indicator, ability, c);
            if (!BuildProjectedRectDecal(indicator, c))
                BuildProjectedRectFill(indicator, c);

            // Zone crossbar dividers for variant rect spells.
            if (ability.variants != null && ability.variants.Length > 1)
            {
                for (int z = 1; z < ability.variants.Length; z++)
                {
                    var barGO = new GameObject($"RectZone_{z}");
                    barGO.transform.SetParent(indicator.transform, false);
                    var barLR = barGO.AddComponent<LineRenderer>();
                    barLR.useWorldSpace = true;
                    barLR.loop = false;
                    barLR.startWidth = barLR.endWidth = 0.06f;
                    barLR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    barLR.receiveShadows = false;
                    barLR.material = new Material(Shader.Find("Sprites/Default"));
                    barLR.startColor = barLR.endColor = new Color(1f, 0.7f, 0.2f, 0.45f);
                    barLR.positionCount = 2;
                }
            }
        }

        // Range ring: only for Circle/Rectangle — cone length already shows the range
        if (ability.range > 0f && ability.shape != AbilityShape.Cone)
            _rangeRingGO = CreateRangeRing(ability.range, c);

        return indicator;
    }

    void BuildOutlineLR(GameObject go, AbilityDef ability, Color color)
    {
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace   = true;
        lr.loop            = true;
        lr.startWidth      = lr.endWidth = 0.08f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows  = false;
        lr.material        = new Material(Shader.Find("Sprites/Default"));
        lr.startColor      = lr.endColor = color;
        lr.positionCount   = 0;   // filled in UpdateIndicatorTransform every frame
    }

    GameObject CreateCircleIndicator()
    {
        var go = new GameObject("CircleIndicator");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        const int radialSegments = 8;
        const int angularSegments = 96;
        Vector3[] vertices = new Vector3[1 + radialSegments * angularSegments];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[angularSegments * 3 + (radialSegments - 1) * angularSegments * 6];

        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f);

        int vertex = 1;
        for (int r = 1; r <= radialSegments; r++)
        {
            float radius = r / (float)radialSegments * 0.5f;
            for (int i = 0; i < angularSegments; i++)
            {
                float angle = i / (float)angularSegments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                vertices[vertex] = new Vector3(x, y, 0f);
                uv[vertex] = new Vector2(x + 0.5f, y + 0.5f);
                vertex++;
            }
        }

        int tri = 0;
        for (int i = 0; i < angularSegments; i++)
        {
            triangles[tri++] = 0;
            triangles[tri++] = 1 + i;
            triangles[tri++] = 1 + ((i + 1) % angularSegments);
        }

        for (int r = 1; r < radialSegments; r++)
        {
            int innerStart = 1 + (r - 1) * angularSegments;
            int outerStart = 1 + r * angularSegments;
            for (int i = 0; i < angularSegments; i++)
            {
                int next = (i + 1) % angularSegments;
                int innerA = innerStart + i;
                int innerB = innerStart + next;
                int outerA = outerStart + i;
                int outerB = outerStart + next;

                triangles[tri++] = innerA;
                triangles[tri++] = outerA;
                triangles[tri++] = innerB;
                triangles[tri++] = innerB;
                triangles[tri++] = outerA;
                triangles[tri++] = outerB;
            }
        }

        Mesh mesh = new Mesh { name = "CircleIndicatorDisk" };
        mesh.MarkDynamic();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;

        return go;
    }

    void BuildCircleOutline(GameObject indicator, Color color)
    {
        var outline = new GameObject("CircleIndicatorOutline");
        outline.transform.SetParent(indicator.transform, false);

        LineRenderer lr = outline.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = lr.endWidth = 0.12f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = BrightOutlineColor(color);
        lr.positionCount = 0;
    }

    void BuildConeOutline(GameObject indicator, Color color)
    {
        LineRenderer lr = indicator.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = lr.endWidth = 0.10f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = BrightOutlineColor(color);
        lr.positionCount = 0;
    }

    GameObject CreateRangeRing(float range, Color c)
    {
        var go = new GameObject("RangeRing");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace   = false;     // local-space ring: move GO, don't rebuild points
        lr.loop            = true;
        lr.startWidth      = lr.endWidth = 0.04f;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows  = false;
        lr.material        = new Material(Shader.Find("Sprites/Default"));
        lr.startColor      = lr.endColor = new Color(c.r, c.g, c.b, 0.30f);

        const int segs = 64;
        lr.positionCount = segs;
        for (int i = 0; i < segs; i++)
        {
            float a = i / (float)segs * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * range, 0f, Mathf.Sin(a) * range));
        }
        // Anchor to player — lr uses local space so ring auto-follows without per-frame update
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.up * 0.05f;
        return go;
    }

    void DestroyRangeRing()
    {
        if (_rangeRingGO != null) { Destroy(_rangeRingGO); _rangeRingGO = null; }
    }

    Color GetCategoryColor(AbilityCategory category)
    {
        switch (category)
        {
            case AbilityCategory.Heal:    return new Color(0.2f, 1f, 0.3f, 0.55f);
            case AbilityCategory.Support: return new Color(0.2f, 0.6f, 1f, 0.55f);
            default:                      return new Color(1f, 0.4f, 0.05f, 0.55f);
        }
    }

    // Returns the assigned Inspector texture for a category, or a procedural ring
    // if none is assigned.  Procedural ring is cached after first build.
    Texture2D GetIndicatorTexture(AbilityCategory category)
    {
        switch (category)
        {
            case AbilityCategory.Heal:    return indicatorTextureHeal    ?? ProceduralRingTexture;
            case AbilityCategory.Support: return indicatorTextureSupport ?? ProceduralRingTexture;
            default:                      return indicatorTextureDamage  ?? ProceduralRingTexture;
        }
    }

    static Texture2D s_proceduralRing;
    static Texture2D ProceduralRingTexture
    {
        get
        {
            if (s_proceduralRing != null) return s_proceduralRing;

            const int size = 128;
            s_proceduralRing = new Texture2D(size, size, TextureFormat.RGBA32, false);
            s_proceduralRing.wrapMode = TextureWrapMode.Clamp;
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                // Normalised distance from centre: 0 = centre, 1 = edge
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f),
                                           new Vector2(half, half)) / half;

                // Bright outer ring band
                float ring  = Mathf.SmoothStep(0.60f, 0.74f, d)
                            * (1f - Mathf.SmoothStep(0.87f, 1.00f, d));
                // Very faint inner fill so the centre area is visible but not blinding
                float inner = (1f - Mathf.SmoothStep(0f, 0.60f, d)) * 0.12f;

                s_proceduralRing.SetPixel(x, y,
                    new Color(1f, 1f, 1f, Mathf.Clamp01(ring + inner)));
            }
            s_proceduralRing.Apply();
            return s_proceduralRing;
        }
    }

    float GetChargeFraction(AbilityDef ability, float timer)
    {
        if (!ability.chargeable || ability.maxChargeTime <= 0f) return 0f;
        float t = timer % ability.maxChargeTime;
        return t / ability.maxChargeTime;
    }

    void UpdateIndicatorTransform(AbilityDef ability, GameObject indicator, float aimTime)
    {
        GetAimData(ability, out Vector3 aimDir, out float aimDistance);
        float chargeFraction = GetChargeFraction(ability, aimTime);

        var lr = indicator.GetComponent<LineRenderer>();

        if (ability.shape == AbilityShape.Circle)
        {
            float sizeMul = Mathf.Lerp(1f, ability.maxChargeSizeMultiplier, chargeFraction);
            Vector3 centre = ProjectToGround(transform.position + aimDir * aimDistance);

            indicator.transform.position = centre;
            indicator.transform.rotation = Quaternion.identity;

            // Scale the Quad child for charge growth
            if (indicator.transform.childCount > 0)
            {
                float size = ability.indicatorSize * sizeMul;
                indicator.transform.GetChild(0).localScale = new Vector3(size, size, 1f);
                UpdateCircleOutline(indicator, centre, size * 0.5f);
            }

            UpdateProjectedCircleFill(indicator);
        }
        else if (ability.shape == AbilityShape.Rectangle)
        {
            bool hasVariants = ability.variants != null && ability.variants.Length > 0;

            float widthMul = Mathf.Lerp(1f, ability.maxChargeSizeMultiplier, chargeFraction);
            float hw = ability.rectWidth * widthMul / 2f;
            // Variant rects always show at full range; cursor distance picks zone.
            float rectLength = hasVariants ? ability.range : aimDistance;
            Vector3 mid = ProjectToGround(transform.position + aimDir * (rectLength / 2f), out Vector3 groundNormal);
            Vector3 groundForward = Vector3.ProjectOnPlane(aimDir, groundNormal);
            if (groundForward.sqrMagnitude < 0.0001f)
                groundForward = aimDir;
            groundForward.Normalize();

            indicator.transform.position   = mid;
            indicator.transform.rotation   = Quaternion.LookRotation(groundForward, groundNormal);
            // Keep localScale for VFX, server proxies, and the damage fallback path.
            indicator.transform.localScale = new Vector3(ability.rectWidth * widthMul, 1f, rectLength);

            RectangleAimData rectData = UpdateRectangleAimData(indicator, mid, groundForward, groundNormal, hw, rectLength / 2f);
            if (lr != null) SetRectPoints(lr, rectData);
            UpdateProjectedRectDecal(indicator, rectData);
            UpdateProjectedRectFill(indicator, rectData);

            if (hasVariants)
            {
                if (useScrollWheelVariants)
                {
                    float scroll = Mouse.current?.scroll.y.ReadValue() ?? 0f;
                    if (scroll > 0f) _activeVariantIndex = Mathf.Min(_activeVariantIndex + 1, ability.variants.Length - 1);
                    else if (scroll < 0f) _activeVariantIndex = Mathf.Max(_activeVariantIndex - 1, 0);
                    _currentAimFraction = ability.variants.Length > 1
                        ? (float)_activeVariantIndex / (ability.variants.Length - 1)
                        : 0f;
                }
                else
                {
                    _currentAimFraction = ability.range > 0f
                        ? Mathf.Clamp01(aimDistance / ability.range)
                        : 0f;
                    _activeVariantIndex = Mathf.Clamp(
                        Mathf.FloorToInt(_currentAimFraction * ability.variants.Length),
                        0, ability.variants.Length - 1);
                }

                UpdateRectZoneMarkers(indicator, ability, rectData);
                Color vc = GetVariantTint(ability, _activeVariantIndex);
                if (lr != null) lr.startColor = lr.endColor = vc;
                SetProjectedRectColor(indicator, vc);
            }
        }
        else if (ability.shape == AbilityShape.Cone)
        {
            bool hasVariants = ability.variants != null && ability.variants.Length > 0;

            float chargeMul   = Mathf.Lerp(1f, ability.maxChargeSizeMultiplier, chargeFraction);
            // Variant spells: always show at full range; cursor distance only picks zone.
            float distanceMul = hasVariants ? 1f : (ability.range > 0f ? aimDistance / ability.range : 1f);
            float visualRange = ability.range * distanceMul * chargeMul;
            // Pull origin 0.5 units behind the player so the character body sits
            // inside the fan rather than at the very tip.
            Vector3 coneOrigin = ProjectToGround(transform.position - aimDir * 0.5f, out Vector3 groundNormal);
            Vector3 groundForward = Vector3.ProjectOnPlane(aimDir, groundNormal);
            if (groundForward.sqrMagnitude < 0.0001f)
                groundForward = aimDir;
            groundForward.Normalize();

            indicator.transform.position   = coneOrigin;
            indicator.transform.rotation   = Quaternion.LookRotation(aimDir, Vector3.up);
            indicator.transform.localScale = Vector3.one * distanceMul * chargeMul;

            ConeAimData coneData = UpdateConeAimData(indicator, coneOrigin, groundForward, groundNormal, visualRange, ability.coneAngle * 0.5f);
            if (lr != null) SetConeOutlinePoints(lr, coneData);
            UpdateProjectedConeFill(indicator, coneData);

            if (hasVariants)
            {
                if (useScrollWheelVariants)
                {
                    float scroll = Mouse.current?.scroll.y.ReadValue() ?? 0f;
                    if (scroll > 0f) _activeVariantIndex = Mathf.Min(_activeVariantIndex + 1, ability.variants.Length - 1);
                    else if (scroll < 0f) _activeVariantIndex = Mathf.Max(_activeVariantIndex - 1, 0);
                    _currentAimFraction = ability.variants.Length > 1
                        ? (float)_activeVariantIndex / (ability.variants.Length - 1)
                        : 0f;
                }
                else
                {
                    _currentAimFraction = ability.range > 0f
                        ? Mathf.Clamp01(aimDistance / ability.range)
                        : 0f;
                    _activeVariantIndex = Mathf.Clamp(
                        Mathf.FloorToInt(_currentAimFraction * ability.variants.Length),
                        0, ability.variants.Length - 1);
                }

                UpdateConeZoneArcs(indicator, ability, coneData);
                SetConeIndicatorColor(indicator, GetVariantTint(ability, _activeVariantIndex));
            }
        }

        // Charge tint — apply to LR (circle/rect) or renderer (cone)
        if (ability.chargeable)
        {
            Color baseColor = GetCategoryColor(ability.category);
            Color c = ability.chargedTint.a > 0f
                ? Color.Lerp(baseColor, ability.chargedTint, chargeFraction)
                : new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(baseColor.a, 0.95f, chargeFraction));

            if (ability.shape == AbilityShape.Rectangle && lr != null)
            {
                lr.startColor = lr.endColor = c;
                SetProjectedRectColor(indicator, c);
            }
            else if (ability.shape == AbilityShape.Circle)
            {
                SetCircleIndicatorColor(indicator, c);
            }
            else if (ability.shape == AbilityShape.Cone)
            {
                SetConeIndicatorColor(indicator, c);
            }
            else
            {
                var rend = indicator.GetComponentInChildren<Renderer>();
                if (rend != null) rend.material.color = c;
            }
        }
    }

    // ── LineRenderer helpers ─────────────────────────────────────────────────

    static void SetRingPoints(LineRenderer lr, Vector3 centre, float radius, int segs)
    {
        if (lr.positionCount != segs) lr.positionCount = segs;
        for (int i = 0; i < segs; i++)
        {
            float a = i / (float)segs * Mathf.PI * 2f;
            lr.SetPosition(i, centre + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius));
        }
    }

    void UpdateCircleOutline(GameObject indicator, Vector3 centre, float radius)
    {
        LineRenderer lr = GetCircleOutline(indicator);
        if (lr == null)
            return;

        const int segs = 128;
        if (lr.positionCount != segs) lr.positionCount = segs;
        for (int i = 0; i < segs; i++)
        {
            float a = i / (float)segs * Mathf.PI * 2f;
            Vector3 flatPoint = centre + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            lr.SetPosition(i, ProjectToGround(flatPoint));
        }
    }

    void SetCircleIndicatorColor(GameObject indicator, Color color)
    {
        MeshRenderer[] renderers = indicator.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer != null && renderer.gameObject.name == "CircleIndicator" && renderer.material != null)
                renderer.material.color = color;
        }

        LineRenderer outline = GetCircleOutline(indicator);
        if (outline != null)
            outline.startColor = outline.endColor = BrightOutlineColor(color);
    }

    LineRenderer GetCircleOutline(GameObject indicator)
    {
        LineRenderer[] renderers = indicator.GetComponentsInChildren<LineRenderer>();
        foreach (LineRenderer renderer in renderers)
        {
            if (renderer != null && renderer.gameObject.name == "CircleIndicatorOutline")
                return renderer;
        }

        return null;
    }

    static Color BrightOutlineColor(Color color)
    {
        return new Color(
            Mathf.Clamp01(color.r * 1.5f + 0.2f),
            Mathf.Clamp01(color.g * 1.5f + 0.2f),
            Mathf.Clamp01(color.b * 1.5f + 0.2f),
            Mathf.Clamp01(Mathf.Max(color.a, 0.95f)));
    }

    ConeAimData UpdateConeAimData(GameObject indicator, Vector3 origin, Vector3 fwd, Vector3 up, float range, float halfAngle)
    {
        ConeAimData data = indicator.GetComponent<ConeAimData>();
        if (data == null)
            data = indicator.AddComponent<ConeAimData>();

        data.EnsurePoints(ConeArcSegments);

        Vector3 visualNormal = up.sqrMagnitude > 0.0001f ? up.normalized : Vector3.up;
        if (Vector3.Dot(visualNormal, Vector3.up) < 0f)
            visualNormal = -visualNormal;

        Vector3 visualForward = Vector3.ProjectOnPlane(fwd, visualNormal);
        if (visualForward.sqrMagnitude < 0.0001f)
            visualForward = new Vector3(fwd.x, 0f, fwd.z);
        if (visualForward.sqrMagnitude < 0.0001f)
            visualForward = transform.forward;
        visualForward.Normalize();

        data.valid = true;
        data.origin = origin;
        data.visualForward = visualForward;
        data.visualNormal = visualNormal;
        data.visualRange = Mathf.Max(0.05f, range);
        data.halfAngle = Mathf.Max(0f, halfAngle);

        data.outlinePoints[0] = origin;
        for (int i = 0; i <= ConeArcSegments; i++)
        {
            float t = i / (float)ConeArcSegments;
            float angle = Mathf.Lerp(-data.halfAngle, data.halfAngle, t);
            Vector3 dir = Quaternion.AngleAxis(angle, data.visualNormal) * data.visualForward;
            data.outlinePoints[i + 1] = ProjectToGround(origin + dir * data.visualRange);
        }

        return data;
    }

    void SetConeOutlinePoints(LineRenderer lr, ConeAimData data)
    {
        if (lr == null || data == null || !data.valid)
            return;

        data.EnsurePoints(ConeArcSegments);
        if (lr.positionCount != data.outlinePoints.Length)
        {
            lr.positionCount = data.outlinePoints.Length;
            lr.loop = true;
        }

        for (int i = 0; i < data.outlinePoints.Length; i++)
            lr.SetPosition(i, data.outlinePoints[i]);
    }

    void SetConeIndicatorColor(GameObject indicator, Color color)
    {
        MeshRenderer renderer = GetProjectedConeFillRenderer(indicator);
        if (renderer != null && renderer.material != null)
        {
            float alpha = Mathf.Lerp(RectFillAlpha, RectFillChargedAlpha, Mathf.InverseLerp(0.55f, 0.95f, color.a));
            renderer.material.color = new Color(color.r, color.g, color.b, alpha);
        }

        LineRenderer outline = indicator.GetComponent<LineRenderer>();
        if (outline != null)
            outline.startColor = outline.endColor = BrightOutlineColor(color);
    }

    // The orange outline corners are the source of truth for rectangle fill and hits.
    RectangleAimData UpdateRectangleAimData(GameObject indicator, Vector3 centre, Vector3 fwd, Vector3 up, float hw, float hl)
    {
        RectangleAimData data = indicator.GetComponent<RectangleAimData>();
        if (data == null)
            data = indicator.AddComponent<RectangleAimData>();

        data.EnsureCorners();

        Vector3 right = Vector3.Cross(up, fwd);
        if (right.sqrMagnitude < 0.0001f)
            right = Vector3.Cross(Vector3.up, fwd);
        if (right.sqrMagnitude < 0.0001f)
            right = transform.right;
        right.Normalize();

        data.corners[0] = ProjectToGround(centre - right * hw - fwd * hl);
        data.corners[1] = ProjectToGround(centre + right * hw - fwd * hl);
        data.corners[2] = ProjectToGround(centre + right * hw + fwd * hl);
        data.corners[3] = ProjectToGround(centre - right * hw + fwd * hl);

        Vector3 nearCenter = (data.corners[0] + data.corners[1]) * 0.5f;
        Vector3 farCenter = (data.corners[3] + data.corners[2]) * 0.5f;
        Vector3 leftCenter = (data.corners[0] + data.corners[3]) * 0.5f;
        Vector3 rightCenter = (data.corners[1] + data.corners[2]) * 0.5f;

        Vector3 visualForward = farCenter - nearCenter;
        Vector3 visualRight = rightCenter - leftCenter;
        float visualLength = visualForward.magnitude;
        float visualWidth = visualRight.magnitude;

        if (visualForward.sqrMagnitude < 0.0001f)
        {
            visualForward = fwd;
            visualLength = hl * 2f;
        }
        if (visualRight.sqrMagnitude < 0.0001f)
        {
            visualRight = right;
            visualWidth = hw * 2f;
        }

        visualForward.Normalize();
        visualRight = Vector3.ProjectOnPlane(visualRight, visualForward);
        if (visualRight.sqrMagnitude < 0.0001f)
            visualRight = Vector3.Cross(Vector3.up, visualForward);
        if (visualRight.sqrMagnitude < 0.0001f)
            visualRight = right;
        visualRight.Normalize();

        Vector3 visualNormal = Vector3.Cross(visualForward, visualRight);
        if (visualNormal.sqrMagnitude < 0.0001f)
            visualNormal = up.sqrMagnitude > 0.0001f ? up : Vector3.up;
        visualNormal.Normalize();
        if (Vector3.Dot(visualNormal, Vector3.up) < 0f)
            visualNormal = -visualNormal;

        Vector3 damageForward = new Vector3(visualForward.x, 0f, visualForward.z);
        if (damageForward.sqrMagnitude < 0.0001f)
            damageForward = new Vector3(fwd.x, 0f, fwd.z);
        if (damageForward.sqrMagnitude < 0.0001f)
            damageForward = transform.forward;
        damageForward.Normalize();

        Vector3 flatForwardSpan = new Vector3(farCenter.x - nearCenter.x, 0f, farCenter.z - nearCenter.z);
        Vector3 flatRightSpan = new Vector3(rightCenter.x - leftCenter.x, 0f, rightCenter.z - leftCenter.z);
        float damageLength = flatForwardSpan.sqrMagnitude > 0.0001f ? flatForwardSpan.magnitude : hl * 2f;
        float damageWidth = flatRightSpan.sqrMagnitude > 0.0001f ? flatRightSpan.magnitude : hw * 2f;

        data.valid = true;
        data.visualCenter = (data.corners[0] + data.corners[1] + data.corners[2] + data.corners[3]) * 0.25f;
        data.visualRight = visualRight;
        data.visualForward = visualForward;
        data.visualNormal = visualNormal;
        data.visualWidth = Mathf.Max(0.05f, visualWidth);
        data.visualLength = Mathf.Max(0.05f, visualLength);
        data.damageCenter = data.visualCenter;
        data.damageRotation = Quaternion.LookRotation(damageForward, Vector3.up);
        data.damageHalfExtents = new Vector3(Mathf.Max(0.05f, damageWidth) * 0.5f, 1f, Mathf.Max(0.05f, damageLength) * 0.5f);

        return data;
    }

    void SetRectPoints(LineRenderer lr, RectangleAimData data)
    {
        if (lr == null || data == null || !data.valid)
            return;

        data.EnsureCorners();
        if (lr.positionCount != RectCornerCount) { lr.positionCount = RectCornerCount; lr.loop = true; }
        for (int i = 0; i < RectCornerCount; i++)
            lr.SetPosition(i, data.corners[i]);
    }

    void BuildProjectedRectFill(GameObject indicator, Color color)
    {
        var fill = new GameObject("ProjectedRectFill");
        fill.transform.SetParent(indicator.transform, false);

        if (!s_loggedRectIndicatorPath)
        {
            s_loggedRectIndicatorPath = true;
            Debug.Log("[SpellIndicator] Rectangle indicator using mesh fallback fill.", this);
        }

        var mf = fill.AddComponent<MeshFilter>();
        var mr = fill.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        var mat = CreateTransparentIndicatorMaterial();
        mat.color = new Color(color.r, color.g, color.b, RectFillAlpha);
        mat.mainTexture = RectDecalTexture;
        mat.renderQueue = 3000;
        mr.material = mat;

        const int widthSegments = 12;
        const int lengthSegments = 48;
        Vector3[] vertices = new Vector3[(widthSegments + 1) * (lengthSegments + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[widthSegments * lengthSegments * 6];

        int v = 0;
        for (int z = 0; z <= lengthSegments; z++)
        {
            float nz = z / (float)lengthSegments;
            for (int x = 0; x <= widthSegments; x++)
            {
                float nx = x / (float)widthSegments;
                vertices[v] = new Vector3(nx - 0.5f, 0f, nz - 0.5f);
                uv[v] = new Vector2(nx, nz);
                v++;
            }
        }

        int t = 0;
        for (int z = 0; z < lengthSegments; z++)
        for (int x = 0; x < widthSegments; x++)
        {
            int i = z * (widthSegments + 1) + x;
            triangles[t++] = i;
            triangles[t++] = i + widthSegments + 1;
            triangles[t++] = i + 1;
            triangles[t++] = i + 1;
            triangles[t++] = i + widthSegments + 1;
            triangles[t++] = i + widthSegments + 2;
        }

        Mesh mesh = new Mesh { name = "ProjectedRectIndicator" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;
    }

    void BuildProjectedConeFill(GameObject indicator, Color color)
    {
        var fill = new GameObject("ProjectedConeFill");
        fill.transform.SetParent(indicator.transform, false);

        var mf = fill.AddComponent<MeshFilter>();
        var mr = fill.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        var mat = CreateTransparentIndicatorMaterial();
        mat.color = new Color(color.r, color.g, color.b, RectFillAlpha);
        mat.renderQueue = 3000;
        mr.material = mat;

        Vector3[] vertices = new Vector3[(ConeRadialSegments + 1) * (ConeArcSegments + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[ConeRadialSegments * ConeArcSegments * 6];

        int v = 0;
        for (int r = 0; r <= ConeRadialSegments; r++)
        {
            float radial = r / (float)ConeRadialSegments;
            for (int a = 0; a <= ConeArcSegments; a++)
            {
                float arc = a / (float)ConeArcSegments;
                vertices[v] = Vector3.zero;
                uv[v] = new Vector2(radial, arc);
                v++;
            }
        }

        int t = 0;
        int stride = ConeArcSegments + 1;
        for (int r = 0; r < ConeRadialSegments; r++)
        for (int a = 0; a < ConeArcSegments; a++)
        {
            int i = r * stride + a;
            triangles[t++] = i;
            triangles[t++] = i + stride;
            triangles[t++] = i + 1;
            triangles[t++] = i + 1;
            triangles[t++] = i + stride;
            triangles[t++] = i + stride + 1;
        }

        Mesh mesh = new Mesh { name = "ProjectedConeIndicator" };
        mesh.MarkDynamic();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;
    }

    bool BuildProjectedRectDecal(GameObject indicator, Color color)
    {
        Material mat = CreateRectDecalMaterial(color);
        if (mat == null)
        {
            WarnInvalidRectDecal(null);
            return false;
        }

        var decalGO = new GameObject("ProjectedRectDecal");
        decalGO.transform.SetParent(indicator.transform, false);

        DecalProjector projector = decalGO.AddComponent<DecalProjector>();
        projector.material = mat;
        projector.drawDistance = indicatorRaycastDistance;
        projector.fadeFactor = 1f;
        projector.fadeScale = 1f;
        projector.startAngleFade = 180f;
        projector.endAngleFade = 180f;
        projector.scaleMode = DecalScaleMode.ScaleInvariant;
        projector.pivot = GroundDecalPivot;
        projector.size = new Vector3(1f, 1f, indicatorDecalProjectionDepth);

        bool valid = projector.IsValid();
        if (!valid)
        {
            WarnInvalidRectDecal(mat);
            Destroy(decalGO);
        }
        else if (!s_loggedRectIndicatorPath)
        {
            s_loggedRectIndicatorPath = true;
            string shaderName = mat != null && mat.shader != null ? mat.shader.name : "none";
            Debug.Log($"[SpellIndicator] Rectangle indicator using URP DecalProjector. Shader: {shaderName}", this);
        }

        return valid;
    }

    Material CreateRectDecalMaterial(Color color)
    {
        Material mat = indicatorDecalMaterial != null
            ? new Material(indicatorDecalMaterial)
            : null;

        if (mat == null)
        {
#if UNITY_EDITOR
            Material packageDecal = AssetDatabase.LoadAssetAtPath<Material>(DefaultDecalMaterialPath);
            if (packageDecal != null)
                mat = new Material(packageDecal);
#endif
        }

        if (mat == null)
        {
            UniversalRenderPipelineAsset pipelineAsset =
                UniversalRenderPipeline.asset
                ?? (UnityEngine.QualitySettings.renderPipeline as UniversalRenderPipelineAsset)
                ?? (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset);

            Material defaultDecal = pipelineAsset != null ? pipelineAsset.decalMaterial : null;
            if (defaultDecal != null)
                mat = new Material(defaultDecal);
        }

        if (mat == null)
        {
            Shader shader = Shader.Find(DefaultDecalShaderName);
            if (shader == null)
                return null;

            mat = new Material(shader);
        }

        Color decalColor = new Color(color.r, color.g, color.b, RectDecalAlpha);
        SetMaterialColor(mat, decalColor);

        Texture2D texture = RectDecalTexture;
        if (mat.HasProperty("Base_Map")) mat.SetTexture("Base_Map", texture);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);

        return mat;
    }

    void WarnInvalidRectDecal(Material mat)
    {
        if (s_warnedInvalidRectDecal)
            return;

        s_warnedInvalidRectDecal = true;
        string shaderName = mat != null && mat.shader != null ? mat.shader.name : "none";
        Debug.LogWarning(
            $"Rectangle spell decal material is not a valid URP DecalProjector material. Shader: {shaderName}. " +
            "Using mesh fallback; assign a URP Decal material to indicatorDecalMaterial if this warning appears.",
            this);
    }

    Material CreateTransparentIndicatorMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        Material mat = shader != null
            ? new Material(shader)
            : new Material(Shader.Find("Sprites/Default"));

        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
        if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
        if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        return mat;
    }

    static Texture2D s_rectDecalTexture;
    static Texture2D RectDecalTexture
    {
        get
        {
            if (s_rectDecalTexture != null)
                return s_rectDecalTexture;

            const int size = 32;
            s_rectDecalTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            s_rectDecalTexture.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float edge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                float edgeFade = Mathf.SmoothStep(0f, 1f, edge / 5f);
                float lengthNoise = Mathf.PerlinNoise(x * 0.18f, y * 0.06f);
                float brush = Mathf.Lerp(0.55f, 1f, lengthNoise);
                float alpha = edgeFade * brush;
                s_rectDecalTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            s_rectDecalTexture.Apply();
            return s_rectDecalTexture;
        }
    }

    void UpdateProjectedRectDecal(GameObject indicator, RectangleAimData data)
    {
        DecalProjector projector = indicator.GetComponentInChildren<DecalProjector>();
        if (projector == null || data == null || !data.valid)
            return;

        Transform decalTransform = projector.transform;
        decalTransform.position = data.visualCenter;
        decalTransform.rotation = Quaternion.LookRotation(-data.visualNormal, data.visualForward);
        projector.pivot = GroundDecalPivot;
        projector.size = new Vector3(data.visualWidth, data.visualLength, indicatorDecalProjectionDepth);
    }

    void SetProjectedRectColor(GameObject indicator, Color color)
    {
        DecalProjector projector = indicator.GetComponentInChildren<DecalProjector>();
        if (projector != null && projector.material != null)
            SetMaterialColor(projector.material, color);

        MeshRenderer renderer = indicator.GetComponentInChildren<MeshRenderer>();
        if (renderer != null && renderer.gameObject.name == "ProjectedRectFill" && renderer.material != null)
        {
            float alpha = Mathf.Lerp(RectFillAlpha, RectFillChargedAlpha, Mathf.InverseLerp(0.55f, 0.95f, color.a));
            renderer.material.color = new Color(color.r, color.g, color.b, alpha);
        }
    }

    static void SetMaterialColor(Material mat, Color color)
    {
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
    }

    static void SetMaterialTexture(Material mat, Texture texture)
    {
        if (mat.HasProperty("Base_Map")) mat.SetTexture("Base_Map", texture);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
    }

    void UpdateProjectedCircleFill(GameObject indicator)
    {
        MeshFilter mf = null;
        MeshFilter[] filters = indicator.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter filter in filters)
        {
            if (filter != null && filter.gameObject.name == "CircleIndicator")
            {
                mf = filter;
                break;
            }
        }

        if (mf == null || mf.sharedMesh == null) return;

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        if (uv == null || uv.Length != vertices.Length) return;

        Transform fillTransform = mf.transform;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 flatLocal = new Vector3(uv[i].x - 0.5f, uv[i].y - 0.5f, 0f);
            Vector3 world = fillTransform.TransformPoint(flatLocal);
            vertices[i] = fillTransform.InverseTransformPoint(ProjectToGround(world));
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    void UpdateProjectedRectFill(GameObject indicator, RectangleAimData data)
    {
        MeshFilter mf = null;
        MeshFilter[] filters = indicator.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter filter in filters)
        {
            if (filter != null && filter.gameObject.name == "ProjectedRectFill")
            {
                mf = filter;
                break;
            }
        }

        if (mf == null || mf.sharedMesh == null || data == null || !data.valid) return;

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        if (uv == null || uv.Length != vertices.Length) return;

        for (int i = 0; i < vertices.Length; i++)
        {
            float localX = (uv[i].x - 0.5f) * data.visualWidth;
            float localZ = (uv[i].y - 0.5f) * data.visualLength;
            Vector3 world = data.visualCenter + data.visualRight * localX + data.visualForward * localZ;
            vertices[i] = mf.transform.InverseTransformPoint(ProjectToGround(world));
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    void UpdateProjectedConeFill(GameObject indicator, ConeAimData data)
    {
        MeshFilter mf = GetProjectedConeFillFilter(indicator);
        if (mf == null || mf.sharedMesh == null || data == null || !data.valid) return;

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        if (uv == null || uv.Length != vertices.Length) return;

        for (int i = 0; i < vertices.Length; i++)
        {
            float radial = uv[i].x;
            float angle = Mathf.Lerp(-data.halfAngle, data.halfAngle, uv[i].y);
            Vector3 dir = Quaternion.AngleAxis(angle, data.visualNormal) * data.visualForward;
            Vector3 world = data.origin + dir * (radial * data.visualRange);
            vertices[i] = mf.transform.InverseTransformPoint(ProjectToGround(world));
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    MeshFilter GetProjectedConeFillFilter(GameObject indicator)
    {
        MeshFilter[] filters = indicator.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter filter in filters)
            if (filter != null && filter.gameObject.name == "ProjectedConeFill")
                return filter;

        return null;
    }

    MeshRenderer GetProjectedConeFillRenderer(GameObject indicator)
    {
        MeshRenderer[] renderers = indicator.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
            if (renderer != null && renderer.gameObject.name == "ProjectedConeFill")
                return renderer;

        return null;
    }

    Vector3 ProjectToGround(Vector3 point)
    {
        return ProjectToGround(point, out _);
    }

    Vector3 ProjectToGround(Vector3 point, out Vector3 normal)
    {
        if (TryProjectToTerrain(point, out Vector3 terrainPoint, out normal))
            return terrainPoint;

        Vector3 origin = point + Vector3.up * indicatorRaycastHeight;
        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            indicatorRaycastDistance,
            ~0,
            QueryTriggerInteraction.Ignore);

        normal = Vector3.up;
        if (hits.Length == 0)
            return point + Vector3.up * indicatorGroundOffset;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreIndicatorHit(hit)) continue;

            normal = hit.normal;
            return hit.point + hit.normal * indicatorGroundOffset;
        }

        return point + Vector3.up * indicatorGroundOffset;
    }

    bool ShouldIgnoreIndicatorHit(RaycastHit hit)
    {
        Collider hitCollider = hit.collider;
        if (hitCollider == null)
            return true;

        Transform hitTransform = hitCollider.transform;
        if (hitTransform == transform || hitTransform.IsChildOf(transform))
            return true;

        if (hitCollider.GetComponentInParent<Health>() != null)
            return true;

        return HasIndicatorIgnoredTag(hitTransform);
    }

    static bool HasIndicatorIgnoredTag(Transform hitTransform)
    {
        while (hitTransform != null)
        {
            if (hitTransform.CompareTag("Player") || hitTransform.CompareTag("Enemy"))
                return true;
            hitTransform = hitTransform.parent;
        }

        return false;
    }

    bool TryProjectToTerrain(Vector3 point, out Vector3 projected, out Vector3 normal)
    {
        projected = point + Vector3.up * indicatorGroundOffset;
        normal = Vector3.up;

        Terrain[] terrains = Terrain.activeTerrains;
        if (terrains == null || terrains.Length == 0)
            return false;

        foreach (Terrain terrain in terrains)
        {
            if (terrain == null || terrain.terrainData == null) continue;

            Vector3 pos = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            float localX = point.x - pos.x;
            float localZ = point.z - pos.z;

            if (localX < 0f || localZ < 0f || localX > size.x || localZ > size.z)
                continue;

            float nx = Mathf.Clamp01(localX / size.x);
            float nz = Mathf.Clamp01(localZ / size.z);
            float y = pos.y + terrain.SampleHeight(point);

            normal = terrain.terrainData.GetInterpolatedNormal(nx, nz).normalized;
            projected = new Vector3(point.x, y, point.z) + normal * indicatorGroundOffset;
            return true;
        }

        return false;
    }

    void SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        SpawnVFX(prefab, position, rotation, 4f);
    }

    void SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float lifetime)
    {
        if (prefab == null) return;
        GameObject fx = Instantiate(prefab, position, GetVFXSpawnRotation(prefab, rotation));
        Destroy(fx, Mathf.Max(0.05f, lifetime));
    }

    static Quaternion GetVFXSpawnRotation(GameObject prefab, Quaternion requestedRotation)
    {
        if (prefab != null && prefab.name == "Ice freeze skill")
            return requestedRotation * prefab.transform.rotation;

        return requestedRotation;
    }

#if UNITY_EDITOR || !UNITY_SERVER
    System.Collections.IEnumerator TravelVFX(GameObject prefab, Vector3 from, Vector3 to,
                                             Quaternion rotation, float duration)
    {
        if (prefab == null) yield break;
        GameObject fx = Instantiate(prefab, from, GetVFXSpawnRotation(prefab, rotation));
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (fx == null) yield break;
            fx.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
        if (fx != null) Destroy(fx, 3f);
    }
#endif

    // ── Networked hit VFX ─────────────────────────────────────────────────────
    // Damage resolves server-side, so a plain SpawnVFX at an impact point would only
    // appear on the (headless) server. EmitHitVFX broadcasts the impact to every
    // client via ClientRpc instead, mirroring RpcCastConfirmed. Offline/solo falls
    // back to a direct local spawn.
    void EmitHitVFX(GameObject hitVFXPrefab, Vector3 position, float lifetime = 4f)
    {
        if (hitVFXPrefab == null) return;

        if (NetworkServer.active)
        {
            ResolveHitVfxIndices(hitVFXPrefab, out int spellbookIndex, out int variantIndex);
            if (spellbookIndex >= 0)
                RpcPlayHitVFX(spellbookIndex, position, lifetime, variantIndex);
        }
        else if (!NetworkClient.active)
        {
            // Offline / solo editor play — no network, spawn locally.
            SpawnVFX(hitVFXPrefab, position, Quaternion.identity, lifetime);
        }
    }

    // Resolve a hitVFX prefab to a spellbook index so it can be sent over the RPC.
    // Any index whose hitVFX matches works — the client re-looks-up the same prefab.
    void ResolveHitVfxIndices(GameObject hitVFXPrefab, out int spellbookIndex, out int variantIndex)
    {
        spellbookIndex = -1;
        variantIndex = -1;
        if (spellbook == null) return;

        for (int i = 0; i < spellbook.Length; i++)
        {
            if (spellbook[i] == null) continue;
            if (spellbook[i].hitVFX == hitVFXPrefab)
            {
                spellbookIndex = i;
                return;
            }
        }
    }

    [ClientRpc]
    void RpcPlayHitVFX(int spellbookIndex, Vector3 position, float lifetime, int variantIndex)
    {
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        AbilityDef ability = spellbook[spellbookIndex];
        if (ability == null) return;
#if UNITY_EDITOR || !UNITY_SERVER
        GameObject hitVfxPrefab = ability.hitVFX;
        SpawnVFX(hitVfxPrefab, position, Quaternion.identity, lifetime);
#endif
    }

    // Called by BountySystem passive when a kill is registered.
    public void ReduceAllCooldowns(float seconds)
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
            cooldownTimers[i] = Mathf.Max(0f, cooldownTimers[i] - seconds);
    }

    // Cooldown after gear/attunement Cooldown Reduction is applied.
    float CooldownFor(AbilityDef ability)
    {
        float cd = ability.cooldown;
        if (_characterStats != null)
            cd *= (1f - _characterStats.CooldownReduction);
        return cd;
    }

    float CastTimeFor(AbilityDef ability)
    {
        return ability != null ? Mathf.Max(0f, ability.castTime) : 0f;
    }

    bool FinalizeCast(AbilityDef ability, GameObject indicator, float aimTime, int variantIndex = 0)
    {
        if (ability == null) return false;

        if (ShouldRouteCastToServer())
        {
            int spellbookIndex = FindSpellbookIndex(ability);
            if (spellbookIndex < 0)
            {
                Debug.LogWarning($"[COMBAT] Could not route unknown ability '{ability?.abilityName}' to server.");
                return false;
            }

            Vector3    castPosition = indicator != null ? indicator.transform.position : transform.position;
            Quaternion castRotation = indicator != null ? indicator.transform.rotation : transform.rotation;
            Vector3    castScale    = indicator != null ? indicator.transform.localScale : Vector3.one;

            CmdFinalizeCast(spellbookIndex, castPosition, castRotation, castScale, aimTime, variantIndex);
            PlayLocalCastVFX(ability, castPosition, castRotation, variantIndex);

            if (indicator != null)
                Destroy(indicator);
            return true;
        }

        Debug.Log("Cast ability: " + ability.abilityName);

        AbilityDef passiveAbility = GetVariantPayload(ability, variantIndex) ?? ability;

        // Notify passive (Phase Charge meter, etc.)
        _passive?.OnAbilityCast(passiveAbility);

        // Phase Charge: scale next damage ability
        float damageMultiplier = _phaseCharge != null
            ? _phaseCharge.ConsumeBonusIfCharged(passiveAbility)
            : 1f;

        // Gear + attunement damage bonus (CharacterStats) — applies to every
        // shape and every dispatched ability since they all read this value.
        if (_characterStats != null)
            damageMultiplier *= _characterStats.DamageMultiplier;

        ResolveCastEffects(ability, indicator, aimTime, damageMultiplier, transform.position, variantIndex);
        if (indicator != null)
            Destroy(indicator);
        return true;
    }

    void ResolveCastEffects(AbilityDef ability, GameObject indicator, float aimTime, float damageMultiplier, Vector3 castOrigin, int variantIndex = 0)
    {
        if (ability == null) return;

#if UNITY_EDITOR || !UNITY_SERVER
        if (ability.category == AbilityCategory.Heal && (ability.variants == null || ability.variants.Length == 0))
            OnHealCast?.Invoke();
#endif

        // Variant spells must resolve one of their zones to a spellbook payload.
        if (ability.variants != null && ability.variants.Length > 0)
        {
            int clampedIdx = Mathf.Clamp(variantIndex, 0, ability.variants.Length - 1);
            AbilityVariant variant = ability.variants[clampedIdx];
            AbilityDef referencedAbility = ResolveVariantSpellbookAbility(ability, variant);
            if (referencedAbility == null)
            {
                Debug.LogWarning($"[COMBAT] Variant {clampedIdx} on '{ability.abilityName}' has no spellbook payload reference.");
                return;
            }

            ResolveReferencedVariantCast(ability, referencedAbility, indicator, aimTime, damageMultiplier, castOrigin);
            return;
        }
        else if (ability.abilityName == "Healing Cone" || ability.abilityName == "Mending Beam" ||
            ability.abilityName == "Conflagration Cone" || ability.abilityName == "Ember Beam")
        {
            ApplySweetSpotEffects(ability, indicator, damageMultiplier, castOrigin);
        }

        bool isVariantSpell = ability.variants != null && ability.variants.Length > 0;

        if (!isVariantSpell && ability.shape == AbilityShape.Rectangle && ability.damage > 0f && indicator != null)
        {
            float chargeFraction = GetChargeFraction(ability, aimTime);
            float damage = Mathf.Lerp(ability.damage, ability.maxChargeDamage, chargeFraction) * damageMultiplier;
            ApplyRectangleDamage(ability, indicator, damage);
        }

        if (!isVariantSpell && ability.shape == AbilityShape.Cone && ability.damage > 0f && indicator != null)
        {
            float chargeFraction = GetChargeFraction(ability, aimTime);
            float damage = Mathf.Lerp(ability.damage, ability.maxChargeDamage, chargeFraction) * damageMultiplier;
            float coneRange = ability.range * indicator.transform.localScale.x;
            ApplyConeDamage(ability, indicator, damage, coneRange, castOrigin);

#if UNITY_EDITOR || !UNITY_SERVER
            if (ability.fireVisual)
                SpawnFireBurst(castOrigin + indicator.transform.forward * coneRange + Vector3.up * 0.5f, indicator.transform.rotation, coneRange, ability.coneAngle);
#endif
        }

        if (ability.shape == AbilityShape.Circle && ability.damage > 0f && !IsArcaneStep(ability) && !IsVoidMaw(ability))
        {
            ApplyCircleDamage(ability, indicator, damageMultiplier);
        }

        // Variant spells handle shields via their BUBBLE zone; skip the self-shield path.
        if (ability.shieldAbsorb > 0f && (ability.variants == null || ability.variants.Length == 0))
            CastMagicShield(ability);

        if (ability.spawnTurret && indicator != null)
            SpawnTurret(ability, indicator.transform.position);

        // ── Route to ability-specific behaviours ──────────────────
        Vector3    castPoint   = indicator != null ? indicator.transform.position : transform.position;
        Quaternion castVfxRot  = indicator != null ? indicator.transform.rotation  : transform.rotation;

        // Cone: effect spawns at the far (wide) end, not at the player's feet.
        if (ability.shape == AbilityShape.Cone && indicator != null)
        {
            float coneRange = ability.range * indicator.transform.localScale.x;
            castPoint = castOrigin + indicator.transform.forward * coneRange;
        }

#if UNITY_EDITOR || !UNITY_SERVER
        GameObject castVfxPrefab = ability.castVFX;

        if (castVfxPrefab != null)
        {
            if (ability.shape == AbilityShape.Rectangle && indicator != null)
                StartCoroutine(TravelVFX(castVfxPrefab,
                    castOrigin + Vector3.up * 1.2f,
                    castPoint + Vector3.up * 0.5f,
                    castVfxRot, 0.3f));
            else
                SpawnVFX(castVfxPrefab, castPoint + Vector3.up * 0.8f, castVfxRot);
        }
#endif
        DispatchAbility(ability, castPoint, damageMultiplier);
        StartPulseDamageIfNeeded(ability, castPoint, damageMultiplier);
    }

    [Command]
    void CmdFinalizeCast(int spellbookIndex, Vector3 castPosition, Quaternion castRotation, Vector3 castScale, float aimTime, int variantIndex)
    {
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;

        int equippedSlot = FindEquippedSlotForSpellbookIndex(spellbookIndex);
        if (equippedSlot < 0)
        {
            Debug.LogWarning($"[COMBAT] Rejected unequipped ability index {spellbookIndex} from {name}.");
            return;
        }

        AbilityDef ability = spellbook[spellbookIndex];
        if (ability == null) return;
        if (cooldownTimers[equippedSlot] > 0f) return;

        GameObject serverIndicator = CreateServerCastProxy(ability, castPosition, castRotation, castScale);
        if (!FinalizeCast(ability, serverIndicator, aimTime, variantIndex))
        {
            if (serverIndicator != null) Destroy(serverIndicator);
            return;
        }

        cooldownTimers[equippedSlot] = CooldownFor(ability);

        RpcCastConfirmed(spellbookIndex, castPosition, castRotation, variantIndex);
    }

    [ClientRpc]
    void RpcCastConfirmed(int spellbookIndex, Vector3 position, Quaternion rotation, int variantIndex)
    {
        if (isLocalPlayer) return;
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        PlayLocalCastVFX(spellbook[spellbookIndex], position, rotation, variantIndex);
    }

    GameObject CreateServerCastProxy(AbilityDef ability, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (ability == null || ability.range <= 0f) return null;

        GameObject proxy = new GameObject($"ServerCast_{ability.abilityName}");
        proxy.transform.position = position;
        proxy.transform.rotation = rotation;
        proxy.transform.localScale = scale;

        if (ability.shape == AbilityShape.Rectangle)
        {
            proxy.AddComponent<RectangleAimData>();
            UpdateRectangleAimData(
                proxy,
                position,
                rotation * Vector3.forward,
                rotation * Vector3.up,
                Mathf.Abs(scale.x) * 0.5f,
                Mathf.Abs(scale.z) * 0.5f);
        }

        return proxy;
    }

    void PlayLocalCastVFX(AbilityDef ability, Vector3 position, Quaternion rotation, int variantIndex = 0)
    {
        if (ability == null) return;
        SpawnLocalCastVFX(ability, position, rotation, transform.position, variantIndex);
    }

    void SpawnLocalCastVFX(AbilityDef ability, Vector3 position, Quaternion rotation, Vector3 castOrigin, int variantIndex = 0)
    {
        if (ability == null) return;

        AbilityDef displayAbility = GetVariantPayload(ability, variantIndex) ?? ability;

#if UNITY_EDITOR || !UNITY_SERVER
        if (displayAbility.category == AbilityCategory.Heal) OnHealCast?.Invoke();
#endif

        GameObject castVfxPrefab = displayAbility.castVFX != null ? displayAbility.castVFX : ability.castVFX;

        if (castVfxPrefab == null) return;

#if UNITY_EDITOR || !UNITY_SERVER
        if (displayAbility.shape == AbilityShape.Rectangle)
            StartCoroutine(TravelVFX(castVfxPrefab,
                castOrigin + Vector3.up * 1.2f,
                position + Vector3.up * 0.5f,
                rotation, 0.3f));
        else
#endif
            SpawnVFX(castVfxPrefab, position + Vector3.up, rotation);
    }

    int FindSpellbookIndex(AbilityDef ability)
    {
        if (ability == null || spellbook == null) return -1;
        for (int i = 0; i < spellbook.Length; i++)
            if (ReferenceEquals(spellbook[i], ability)) return i;
        return -1;
    }

    int FindEquippedSlotForSpellbookIndex(int spellbookIndex)
    {
        if (equippedIndices == null) return -1;
        for (int i = 0; i < equippedIndices.Length && i < cooldownTimers.Length; i++)
            if (equippedIndices[i] == spellbookIndex) return i;
        return -1;
    }

    // ── Ability dispatch ─────────────────────────────────────────
    static bool IsArcaneStep(AbilityDef ability)
    {
        return ability != null && ability.abilityName == "Arcane Step";
    }

    static bool IsVoidMaw(AbilityDef ability)
    {
        return ability != null && ability.abilityName == "Void Maw";
    }

    void DispatchAbility(AbilityDef ability, Vector3 castPoint, float dmgMult)
    {
        switch (ability.abilityName)
        {
            // ─ Warden ────────────────────────────────────────────
            case "Runic Snare":
                SpawnDeployableAt(shockMinePrefab ?? ability.deployablePrefab, castPoint,
                    go => { var m = go.GetComponent<ShockMineBehaviour>(); if (m) m.owner = gameObject; });
                break;

            case "Battle Hymn":
                CastOverdrive(ability);
                break;

            case "Spirit Redirect":
                CastDroneCommand(castPoint);
                break;

            case "Mend":
                CastFieldRepair(ability, castPoint);
                break;

            case "Conjurer's Surge":
                CastSystemOverload();
                break;

            // ─ Ironclad ──────────────────────────────────────────
            case "Counter Blow":
                kineticReversalHandler?.Activate();
                break;

            case "Gravity Slam":
                CastMagnetize(ability, castPoint);
                break;

            case "Shieldwall Charge":
                dashHandler?.BreachSlam(GetComponent<PassiveThreatProtocol>());
                break;

            case "Stalwart Stance":
                siegeModeHandler?.Activate();
                break;

            case "Rune Chain":
                CastIronTether(castPoint);
                break;

            case "Iron Rampart":
                SpawnDeployableAt(lastBastionPrefab ?? ability.deployablePrefab, castPoint, null, transform.rotation);
                break;

            // ─ Arcanist ──────────────────────────────────────────
            case "Arcane Step":
                dashHandler?.PhaseShift(castPoint);
                break;

            case "Void Maw":
                CastSingularity(ability, castPoint, false, dmgMult);
                break;

            case "Forked Lightning":
                CastArcLance(ability, castPoint, dmgMult);
                break;

            case "Collapsing Void":
                CastSingularity(ability, castPoint, true, dmgMult);
                break;

            // ─ Cleric ────────────────────────────────────────────
            case "Soul Bond":
                CastTransferProtocol(castPoint);
                break;

            case "Spirit Wisps":
                CastNaniteSwarm(ability, castPoint);
                break;

            case "Divine Spark":
                CastDefibrillator(ability, castPoint, dmgMult);
                break;

            case "Sacred Aegis":
                CastAdaptiveShield(castPoint);
                break;

            case "Dispel":
                CastPurgeProtocol(castPoint);
                break;

            case "Temporal Grace":
                SnapshotSystem.Instance?.Rollback(5f);
                break;

            case "Restoration Beacon":
                SpawnDeployableAt(beaconPrefab ?? ability.deployablePrefab, castPoint, go =>
                {
                    var rb = go.GetComponent<RestorationBeacon>();
                    if (rb != null) { rb.ownerID = gameObject.GetInstanceID(); rb.owner = gameObject; }
                });
                break;

            // ─ Shadowblade ───────────────────────────────────────
            case "Shadow Veil":
            {
                float dur = ability.activeDuration > 0f ? ability.activeDuration : 4f;
                stealthHandler?.BeginCloak(dur);
                break;
            }

            case "Silence Ward":
                SpawnDeployableAt(nullFieldPrefab ?? ability.deployablePrefab, castPoint, null);
                break;

            case "Dark Harvest":
                CastCollapse(ability, castPoint, dmgMult);
                break;
        }
    }

    // ── New ability methods ──────────────────────────────────────

    void StartPulseDamageIfNeeded(AbilityDef ability, Vector3 centre, float damageMultiplier)
    {
        if (!ShouldRunPulseDamage(ability))
            return;

        StartCoroutine(PulseDamage(ability, centre, damageMultiplier));
    }

    static bool ShouldRunPulseDamage(AbilityDef ability)
    {
        if (ability == null)
            return false;

        if (IsArcaneStep(ability) || IsVoidMaw(ability))
            return true;

        return ability.usePulseDamage && ability.pulseCount > 0;
    }

    System.Collections.IEnumerator PulseDamage(AbilityDef ability, Vector3 centre, float damageMultiplier)
    {
        int pulseCount = GetPulseCount(ability, GetDefaultPulseCount(ability));
        if (pulseCount <= 0)
            yield break;

        float radius = GetPulseRadius(ability, GetDefaultPulseRadius(ability));
        float damage = GetPulseDamage(ability, GetDefaultPulseDamage(ability), damageMultiplier);
        float interval = GetPulseInterval(ability, GetDefaultPulseInterval(ability));
        float vfxLifetime = GetPulseVFXLifetime(ability, GetDefaultPulseVFXLifetime(ability));

        for (int pulse = 0; pulse < pulseCount; pulse++)
        {
            ApplyPulseDamage(centre, radius, damage, ability.targetTag, ability.hitVFX, vfxLifetime);
            if (pulse < pulseCount - 1)
                yield return new WaitForSeconds(interval);
        }
    }

    static int GetDefaultPulseCount(AbilityDef ability)
    {
        if (IsArcaneStep(ability)) return ArcaneStepPulseCount;
        if (IsVoidMaw(ability)) return VoidMawPulseCount;
        return 0;
    }

    static float GetDefaultPulseInterval(AbilityDef ability)
    {
        if (IsArcaneStep(ability)) return ArcaneStepPulseInterval;
        if (IsVoidMaw(ability)) return VoidMawPulseInterval;
        return 1f;
    }

    static float GetDefaultPulseRadius(AbilityDef ability)
    {
        if (ability == null) return 1.5f;
        if (IsArcaneStep(ability)) return ability.indicatorSize > 0f ? ability.indicatorSize * 0.5f : 1.75f;
        if (IsVoidMaw(ability)) return ability.indicatorSize > 0f ? ability.indicatorSize * 0.5f : 4f;
        return ability.indicatorSize > 0f ? ability.indicatorSize * 0.5f : 1.5f;
    }

    static float GetDefaultPulseDamage(AbilityDef ability)
    {
        if (ability == null) return 0f;
        if (IsArcaneStep(ability)) return ability.damage > 0f ? ability.damage : 10f;
        if (IsVoidMaw(ability)) return ability.damage > 0f ? ability.damage : 20f;
        return ability.damage > 0f ? ability.damage : 0f;
    }

    static float GetDefaultPulseVFXLifetime(AbilityDef ability)
    {
        if (IsVoidMaw(ability)) return DefaultVoidMawPulseVFXLifetime;
        return 4f;
    }

    static int GetPulseCount(AbilityDef ability, int fallback)
    {
        if (ability != null && ability.usePulseDamage)
            return Mathf.Max(0, ability.pulseCount);

        return fallback;
    }

    static float GetPulseInterval(AbilityDef ability, float fallback)
    {
        if (ability != null && ability.usePulseDamage && ability.pulseInterval > 0f)
            return ability.pulseInterval;

        return fallback;
    }

    static float GetPulseRadius(AbilityDef ability, float fallback)
    {
        if (ability != null && ability.usePulseDamage && ability.pulseRadius > 0f)
            return ability.pulseRadius;

        return fallback;
    }

    static float GetPulseDamage(AbilityDef ability, float fallback, float damageMultiplier)
    {
        float damage = fallback;
        if (ability != null && ability.usePulseDamage && ability.pulseDamage > 0f)
            damage = ability.pulseDamage;

        return damage * damageMultiplier;
    }

    static float GetPulseVFXLifetime(AbilityDef ability, float fallback)
    {
        if (ability != null && ability.usePulseDamage && ability.pulseVFXLifetime > 0f)
            return ability.pulseVFXLifetime;

        return fallback;
    }

    void ApplyPulseDamage(Vector3 centre, float radius, float damage, string targetTag, GameObject hitVFX, float hitVFXLifetime = 4f)
    {
        Collider[] hits = Physics.OverlapSphere(centre, radius);
        var damaged = new System.Collections.Generic.HashSet<Health>();

        foreach (Collider hit in hits)
        {
            Health health = hit.GetComponentInParent<Health>();
            if (health == null || damaged.Contains(health))
                continue;

            if (!HitMatchesTargetTag(hit, health, targetTag))
                continue;

            damaged.Add(health);
            health.TakeDamage(damage, gameObject);
            EmitHitVFX(hitVFX, health.transform.position + Vector3.up * 0.5f, hitVFXLifetime);
        }
    }

    static bool HitMatchesTargetTag(Collider hit, Health health, string targetTag)
    {
        if (string.IsNullOrEmpty(targetTag))
            return true;

        if (hit.CompareTag(targetTag) || health.CompareTag(targetTag))
            return true;

        Transform root = health.transform.root;
        return root != null && root.CompareTag(targetTag);
    }

    static bool TryGetMatchingHealth(Collider hit, string targetTag, out Health health)
    {
        health = hit != null ? hit.GetComponentInParent<Health>() : null;
        return health != null && HitMatchesTargetTag(hit, health, targetTag);
    }

    static bool AddMatchingHit(Collider hit, string targetTag, System.Collections.Generic.List<Collider> hits, System.Collections.Generic.HashSet<Health> matched)
    {
        if (!TryGetMatchingHealth(hit, targetTag, out Health health))
            return false;

        if (matched != null && !matched.Add(health))
            return false;

        hits?.Add(hit);
        return true;
    }

    void CastOverdrive(AbilityDef ability)
    {
        float duration  = ability.activeDuration > 0f ? ability.activeDuration : 8f;
        float auraRange = ability.indicatorSize  > 0f ? ability.indicatorSize  : 12f;

        // Apply a temporary +30% CDR buff to all allies in range (including self).
        // CDR is tracked in CharacterStats — AddTemporaryCDR clamps total CDR to 0.6 (60% max).
        Collider[] hits = Physics.OverlapSphere(transform.position, auraRange);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;

            CharacterStats cs = col.GetComponent<CharacterStats>();
            if (cs != null)
                StartCoroutine(OverdriveCDRBuff(cs, duration));

            // Buff VFX on each ally
            if (ability.castVFX != null)
            {
                GameObject fx = Instantiate(ability.castVFX,
                    col.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                Destroy(fx, duration + 0.5f);
            }
        }
    }

    private System.Collections.IEnumerator OverdriveCDRBuff(CharacterStats cs, float duration)
    {
        const float bonusCDR = 0.30f;   // +30% cooldown reduction for the duration
        cs.AddTemporaryCDR(bonusCDR);
        yield return new UnityEngine.WaitForSeconds(duration);
        cs.AddTemporaryCDR(-bonusCDR);  // remove the buff when expired
    }

    void CastDroneCommand(Vector3 castPoint)
    {
        // Find the nearest enemy to the cast point and redirect all active turrets to it.
        Collider[] hits = Physics.OverlapSphere(castPoint, 2f);
        Transform focusTarget = null;
        float best = Mathf.Infinity;
        foreach (var col in hits)
        {
            if (!col.CompareTag("Enemy")) continue;
            float d = Vector3.Distance(castPoint, col.transform.position);
            if (d < best) { best = d; focusTarget = col.transform; }
        }

        if (focusTarget == null) return;

        // Find this player's deployed turrets and set their focus target.
        if (DeployableManager.Instance != null)
        {
            foreach (var dep in DeployableManager.Instance.GetAll(gameObject.GetInstanceID()))
            {
                if (dep == null) continue;
                var tc = dep.GetComponent<TurretController>();
                if (tc != null) tc.SetFocusTarget(focusTarget, 6f);
            }
        }
    }

    void CastFieldRepair(AbilityDef ability, Vector3 castPoint)
    {
        float healAmt = ability.healAmount > 0f ? ability.healAmount : 40f;
        // Find nearest ally at cast point
        Collider[] hits = Physics.OverlapSphere(castPoint, 1.5f);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            Health h = col.GetComponent<Health>();
            if (h == null || h == _health) continue;
            h.Heal(healAmt);
            col.GetComponent<StatusEffectManager>()?.RemoveAll();   // clears 1 debuff
            EmitHitVFX(ability.hitVFX, col.transform.position + Vector3.up);
            break;
        }
    }

    void CastSystemOverload()
    {
        if (DeployableManager.Instance == null) return;
        DeployableManager.Instance.SystemOverload(gameObject.GetInstanceID(), 8f);

        // Force all turrets to rapid-fire mode for 8 seconds
        foreach (var dep in DeployableManager.Instance.GetAll(gameObject.GetInstanceID()))
        {
            if (dep == null) continue;
            var tc = dep.GetComponent<TurretController>();
            if (tc != null) tc.SetOverloadMode(8f);
        }
    }

    void CastMagnetize(AbilityDef ability, Vector3 castPoint)
    {
        float radius   = ability.pullRadius > 0f ? ability.pullRadius : 4f;
        float duration = ability.pullDuration > 0f ? ability.pullDuration : 2f;

        Collider[] hits = Physics.OverlapSphere(castPoint, radius);
        foreach (var col in hits)
        {
            if (!col.CompareTag(ability.targetTag)) continue;
            StartCoroutine(PullToPoint(col, castPoint, duration));
        }

        if (ability.castVFX != null)
            SpawnVFX(ability.castVFX, castPoint, Quaternion.identity);
    }

    System.Collections.IEnumerator PullToPoint(Collider col, Vector3 center, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && col != null)
        {
            elapsed += Time.fixedDeltaTime;
            Rigidbody rb = col.GetComponent<Rigidbody>();
            Vector3 dir = (center - col.transform.position).normalized;
            if (rb != null) rb.AddForce(dir * 14f, ForceMode.Acceleration);
            else            col.transform.position += dir * 5f * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    void CastIronTether(Vector3 castPoint)
    {
        if (ironTetherHandler == null) return;
        // Find nearest enemy near the cast point
        Collider[] hits = Physics.OverlapSphere(castPoint, 2f);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Enemy")) continue;
            ironTetherHandler.Activate(col.gameObject);
            return;
        }
    }

    void CastSingularity(AbilityDef ability, Vector3 castPoint, bool isEventHorizon, float dmgMult)
    {
        GameObject prefab = isEventHorizon
            ? (eventHorizonPrefab ?? ability.deployablePrefab)
            : (singularityPrefab  ?? ability.deployablePrefab);

        if (prefab == null) return;

        SpawnDeployableAt(prefab, castPoint, go =>
        {
            var s = go.GetComponent<SingularityBehaviour>();
            if (s == null) return;
            s.burstDamage      = isEventHorizon ? s.burstDamage * dmgMult : 0f;
            s.applyExposed     = isEventHorizon;
            s.owner            = gameObject;
            // Check for Phase Relay bonus
            float bonus = PhaseRelayDeployable.GetBonusNearPoint(castPoint, gameObject.GetInstanceID());
            s.pullDurationBonus = bonus;
        });
    }

    void CastArcLance(AbilityDef ability, Vector3 startPoint, float dmgMult)
    {
        int   maxChain   = ability.chainTargets > 0 ? ability.chainTargets : 4;
        float dmg        = ability.damage * dmgMult;
        float falloff    = ability.chainDamageFalloff;
        float jumpRadius = 6f;
        string tag       = ability.targetTag;

        Transform last = null;
        Collider   nearest = FindNearestInRadius(startPoint, jumpRadius, tag, null);

        for (int i = 0; i < maxChain && nearest != null; i++)
        {
            Health h = nearest.GetComponentInParent<Health>();
            h?.TakeDamage(Mathf.Max(1f, dmg), gameObject);

            Vector3 nearestPos = h != null ? h.transform.position : nearest.transform.position;
            EmitHitVFX(ability.hitVFX, nearestPos + Vector3.up * 0.5f);

            // Draw lightning between jumps (quick LineRenderer)
            Vector3 from = last != null ? last.position + Vector3.up * 0.8f
                                        : startPoint    + Vector3.up * 0.8f;
            DrawLightningLine(from, nearestPos + Vector3.up * 0.8f, 0.15f);

            last  = h != null ? h.transform : nearest.transform;
            dmg   = Mathf.Max(1f, dmg - falloff);
            nearest = FindNearestInRadius(last.position, jumpRadius, tag, last);
        }
    }

    Collider FindNearestInRadius(Vector3 center, float radius, string tag, Transform exclude)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        float best = Mathf.Infinity;
        Collider found = null;
        foreach (var col in hits)
        {
            if (!TryGetMatchingHealth(col, tag, out Health health)) continue;
            if (exclude != null && health.transform == exclude) continue;
            float d = Vector3.Distance(center, health.transform.position);
            if (d < best) { best = d; found = col; }
        }
        return found;
    }

    void DrawLightningLine(Vector3 from, Vector3 to, float duration)
    {
        GameObject go   = new GameObject("ArcLance");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 8;
        lr.startWidth    = 0.05f;
        lr.endWidth      = 0.01f;
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor    = new Color(0.8f, 0.4f, 1f, 0.9f);
        lr.endColor      = new Color(0.5f, 0.2f, 1f, 0.3f);

        for (int i = 0; i < 8; i++)
        {
            float t   = i / 7f;
            Vector3 p = Vector3.Lerp(from, to, t);
            if (i > 0 && i < 7)
            {
                Vector3 perp = Vector3.Cross((to - from).normalized, Vector3.up);
                p += perp * (Random.Range(-0.3f, 0.3f));
                p += Vector3.up * Random.Range(-0.15f, 0.15f);
            }
            lr.SetPosition(i, p);
        }

        Destroy(go, duration);
    }

    void CastTransferProtocol(Vector3 castPoint)
    {
        if (transferProtocolHandler == null) return;
        Collider[] hits = Physics.OverlapSphere(castPoint, 1.5f);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            if (col.gameObject == gameObject) continue;
            transferProtocolHandler.Activate(col.gameObject);
            return;
        }
    }

    void CastNaniteSwarm(AbilityDef ability, Vector3 castPoint)
    {
        GameObject prefab = naniteSwarmPrefab ?? ability.deployablePrefab;
        if (prefab == null) return;

        // Find nearest ally to target
        Collider[] hits = Physics.OverlapSphere(castPoint, 3f);
        Health targetH = null; Transform targetT = null;
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            targetH = col.GetComponent<Health>();
            targetT = col.transform;
            break;
        }
        if (targetH == null) { targetH = _health; targetT = transform; }

        SpawnDeployableAt(prefab, transform.position + Vector3.up, go =>
        {
            var s = go.GetComponent<NaniteSwarmBehaviour>();
            if (s == null) return;
            s.targetHealth = targetH;
            s.target       = targetT;
            s.owner        = gameObject;
            if (ability.healAmount > 0f) s.healAmount = ability.healAmount;
        });
    }

    void CastDefibrillator(AbilityDef ability, Vector3 castPoint, float dmgMult)
    {
        // Priority 1: revive a downed ally nearby
        Collider[] allies = Physics.OverlapSphere(castPoint, 2f);
        foreach (var col in allies)
        {
            if (!col.CompareTag("Player")) continue;
            Health h = col.GetComponent<Health>();
            if (h != null && h.IsDowned)
            {
                h.Revive(0.30f);
                EmitHitVFX(ability.hitVFX, col.transform.position + Vector3.up);
                return;
            }
        }

        // Priority 2: deal burst damage to robotic enemies in range
        Collider[] enemies = Physics.OverlapSphere(castPoint, 2f);
        foreach (var col in enemies)
        {
            if (!TryGetMatchingHealth(col, "Enemy", out Health h)) continue;
            if (h == null || !h.isRobotic) continue;
            float dmg = (ability.damage > 0f ? ability.damage : 60f) * dmgMult;
            h.TakeDamage(dmg, gameObject);
            EmitHitVFX(ability.hitVFX, h.transform.position + Vector3.up);
        }
    }

    void CastAdaptiveShield(Vector3 castPoint)
    {
        // Apply a 20-absorb shield to nearest ally that grows as they take hits.
        Collider[] hits = Physics.OverlapSphere(castPoint, 2f);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            Health h = col.GetComponent<Health>();
            if (h == null) continue;
            h.ApplyShield(20f);
            // Subscribe to grow shield on each hit for 8s
            StartCoroutine(AdaptiveShieldRoutine(h, 8f));
            return;
        }
    }

    System.Collections.IEnumerator AdaptiveShieldRoutine(Health target, float duration)
    {
        float expiry = Time.time + duration;
        void OnHit(float _) { target.GrowShield(10f); }
        target.onDamageTaken.AddListener(OnHit);
        while (Time.time < expiry) yield return null;
        target.onDamageTaken.RemoveListener(OnHit);
    }

    void CastPurgeProtocol(Vector3 castPoint)
    {
        Collider[] hits = Physics.OverlapSphere(castPoint, 1.5f);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            col.GetComponent<StatusEffectManager>()?.RemoveAll();
            return;
        }
    }

    void CastCollapse(AbilityDef ability, Vector3 castPoint, float dmgMult)
    {
        float baseDmg = ability.damage > 0f ? ability.damage : 20f;
        float radius  = ability.indicatorSize > 0f ? ability.indicatorSize / 2f : 4f;

        Collider[] hits = Physics.OverlapSphere(castPoint, radius);
        var damaged = new System.Collections.Generic.HashSet<Health>();
        foreach (var col in hits)
        {
            if (!TryGetMatchingHealth(col, ability.targetTag, out Health health) || !damaged.Add(health)) continue;
            var sem = col.GetComponent<StatusEffectManager>() ?? health.GetComponent<StatusEffectManager>();
            if (sem == null) continue;
            int stacks = sem.ConsumeDebuffStacks();
            if (stacks > 0)
            {
                float dmg = baseDmg * stacks * dmgMult;
                health.TakeDamage(dmg, gameObject);
                EmitHitVFX(ability.hitVFX, health.transform.position + Vector3.up * 0.5f);
            }
        }
    }

    // Generic helper: instantiate a deployable prefab, run optional init, register it.
    // Runs server-side in networked play (called from CmdFinalizeCast → ResolveCastEffects),
    // so NetworkServer.Spawn replicates the deployable — and its Start()-spawned ambient
    // VFX — to every client. init runs before Spawn so owner/target fields are set first.
    void SpawnDeployableAt(GameObject prefab, Vector3 pos, System.Action<GameObject> init,
                            Quaternion? rot = null)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, pos, rot ?? Quaternion.identity);
        init?.Invoke(go);

        // Replicate to clients when authoritative. Guard on NetworkIdentity so this
        // degrades to a server-local object (old behaviour) if the prefab hasn't been
        // given a NetworkIdentity yet — run BCE ▶ Setup ▶ 4d to add them.
        if (NetworkServer.active && go.GetComponent<NetworkIdentity>() != null)
            NetworkServer.Spawn(go);

        DeployableManager.Instance?.Register(go, gameObject.GetInstanceID(),
            classPool != null ? GetClassDeployableLimit() : 1);
    }

    int GetClassDeployableLimit()
    {
        if (classPool == null) return 1;
        return classPool.className == "Warden" ? 3 : 1;
    }

    void ApplyCircleDamage(AbilityDef ability, GameObject indicator, float damageMultiplier = 1f)
    {
        Vector3 center = indicator != null ? indicator.transform.position : transform.position;
        Collider[] hits = Physics.OverlapSphere(center, ability.indicatorSize / 2f);
        var damaged = new System.Collections.Generic.HashSet<Health>();

        foreach (Collider hit in hits)
        {
            if (!TryGetMatchingHealth(hit, ability.targetTag, out Health health) || !damaged.Add(health))
                continue;

            health.TakeDamage(ability.damage * damageMultiplier, gameObject);
            EmitHitVFX(ability.hitVFX, health.transform.position + Vector3.up * 0.5f);
        }
    }

    void ApplyRectangleDamage(AbilityDef ability, GameObject indicator, float damage)
    {
        RectangleAimData rectData = indicator.GetComponent<RectangleAimData>();
        Vector3 center;
        Vector3 halfExtents;
        Quaternion rotation;

        if (rectData != null && rectData.valid)
        {
            center = rectData.damageCenter;
            halfExtents = rectData.damageHalfExtents;
            rotation = rectData.damageRotation;
        }
        else
        {
            float rectangleLength = indicator.transform.localScale.z;
            center = indicator.transform.position;
            halfExtents = new Vector3(
                indicator.transform.localScale.x / 2f,
                1f,
                rectangleLength / 2f
            );
            rotation = indicator.transform.rotation;
        }

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            rotation
        );

        var damaged = new System.Collections.Generic.HashSet<Health>();
        foreach (Collider hit in hits)
        {
            if (!TryGetMatchingHealth(hit, ability.targetTag, out Health health) || !damaged.Add(health))
                continue;

            health.TakeDamage(damage, gameObject);
            EmitHitVFX(ability.hitVFX, health.transform.position + Vector3.up * 0.5f);
        }
    }

    void ApplyConeDamage(AbilityDef ability, GameObject indicator, float damage, float coneRange, Vector3 castOrigin)
    {
        Collider[] hits = Physics.OverlapSphere(castOrigin, coneRange);
        var damaged = new System.Collections.Generic.HashSet<Health>();

        foreach (Collider hit in hits)
        {
            if (!TryGetMatchingHealth(hit, ability.targetTag, out Health health) || damaged.Contains(health))
                continue;

            Vector3 toHit = health.transform.position - castOrigin;
            toHit.y = 0;

            if (toHit.sqrMagnitude < 0.0001f) continue;

            float angle = Vector3.Angle(indicator.transform.forward, toHit);
            if (angle > ability.coneAngle / 2f) continue;

            damaged.Add(health);
            health.TakeDamage(damage, gameObject);
            EmitHitVFX(ability.hitVFX, health.transform.position + Vector3.up * 0.5f);
        }
    }

    void ApplySweetSpotEffects(AbilityDef ability, GameObject indicator, float damageMultiplier, Vector3 castOrigin)
    {
        if (indicator == null) return;

        System.Collections.Generic.List<Collider> hitColliders = new System.Collections.Generic.List<Collider>();
        var matched = new System.Collections.Generic.HashSet<Health>();
        float maxRange = ability.range;

        if (ability.shape == AbilityShape.Cone)
        {
            maxRange = ability.range * indicator.transform.localScale.x;
            Collider[] sphereHits = Physics.OverlapSphere(castOrigin, maxRange);
            foreach (var hit in sphereHits)
            {
                if (!TryGetMatchingHealth(hit, ability.targetTag, out Health health) || matched.Contains(health))
                    continue;

                Vector3 toHit = health.transform.position - castOrigin;
                toHit.y = 0;
                if (toHit.sqrMagnitude < 0.0001f) continue;

                float angle = Vector3.Angle(indicator.transform.forward, toHit);
                if (angle > ability.coneAngle / 2f) continue;

                matched.Add(health);
                hitColliders.Add(hit);
            }
        }
        else if (ability.shape == AbilityShape.Rectangle)
        {
            RectangleAimData rectData = indicator.GetComponent<RectangleAimData>();
            Vector3 center;
            Vector3 halfExtents;
            Quaternion rotation;

            if (rectData != null && rectData.valid)
            {
                center = rectData.damageCenter;
                halfExtents = rectData.damageHalfExtents;
                rotation = rectData.damageRotation;
                maxRange = halfExtents.z * 2f;
            }
            else
            {
                float rectangleLength = indicator.transform.localScale.z;
                center = indicator.transform.position;
                halfExtents = new Vector3(
                    indicator.transform.localScale.x / 2f,
                    1f,
                    rectangleLength / 2f
                );
                rotation = indicator.transform.rotation;
                maxRange = rectangleLength;
            }

            Collider[] boxHits = Physics.OverlapBox(center, halfExtents, rotation);
            foreach (var hit in boxHits)
            {
                AddMatchingHit(hit, ability.targetTag, hitColliders, matched);
            }
        }

        foreach (Collider hit in hitColliders)
        {
            Health targetHealth = hit.GetComponentInParent<Health>();
            if (targetHealth == null) continue;

            Vector3 hitPos = targetHealth.transform.position + Vector3.up * 0.5f;
            Vector3 floatingTextPos = targetHealth.transform.position + Vector3.up * 1.5f;
            Vector3 toTarget = targetHealth.transform.position - castOrigin;
            toTarget.y = 0;

            float distance = 0f;
            if (ability.shape == AbilityShape.Cone)
            {
                distance = toTarget.magnitude;
            }
            else if (ability.shape == AbilityShape.Rectangle)
            {
                distance = Vector3.Dot(toTarget, indicator.transform.forward);
            }

            float fraction = Mathf.Clamp01(distance / maxRange);

            if (ability.category == AbilityCategory.Heal)
            {
                if (fraction <= 0.33f)
                {
                    // Zone 1: HPS / Instant Burst
                    float healVal = (ability.healAmount > 0f ? ability.healAmount : 25f) * 1.5f;
                    targetHealth.Heal(healVal);
                    EmitHitVFX(ability.hitVFX, hitPos);
                    FloatingDamageText.Spawn(floatingTextPos, healVal, FloatingDamageText.DamageType.HealCrit);
                }
                else if (fraction <= 0.66f)
                {
                    // Zone 2: HoT / Healing over Time
                    float instantHeal = (ability.healAmount > 0f ? ability.healAmount : 25f) * 0.5f;
                    targetHealth.Heal(instantHeal);

                    float tickAmount = (ability.healAmount > 0f ? ability.healAmount : 25f) * 0.25f;
                    StartCoroutine(ApplyHealOverTime(targetHealth, tickAmount, 5, 1f));

                    EmitHitVFX(ability.hitVFX, hitPos);
                    FloatingDamageText.Spawn(floatingTextPos, instantHeal, FloatingDamageText.DamageType.Heal);
                }
                else
                {
                    // Zone 3: Bubble / Shield
                    float shieldAmount = ability.shieldAbsorb > 0f ? ability.shieldAbsorb : 30f;
                    targetHealth.ApplyShield(shieldAmount);

                    EmitHitVFX(ability.hitVFX, hitPos);
                    FloatingDamageText.Spawn(floatingTextPos, shieldAmount, FloatingDamageText.DamageType.Shield);
                }
            }
            else
            {
                if (fraction <= 0.33f)
                {
                    // Zone 1: Burst Damage + Stagger
                    float dmgVal = (ability.damage > 0f ? ability.damage : 20f) * 1.6f * damageMultiplier;
                    targetHealth.TakeDamage(dmgVal, gameObject);

                    StatusEffectManager sem = hit.GetComponent<StatusEffectManager>() ?? targetHealth.GetComponent<StatusEffectManager>();
                    if (sem != null)
                    {
                        sem.AddEffect(new StatusEffect(StatusEffectType.Stagger, 0.8f, 0f, gameObject));
                    }

                    EmitHitVFX(ability.hitVFX, hitPos);
                }
                else if (fraction <= 0.66f)
                {
                    // Zone 2: Burn DoT (Cursed effect)
                    float instantDmg = (ability.damage > 0f ? ability.damage : 20f) * 0.6f * damageMultiplier;
                    targetHealth.TakeDamage(instantDmg, gameObject);

                    StatusEffectManager sem = hit.GetComponent<StatusEffectManager>() ?? targetHealth.GetComponent<StatusEffectManager>();
                    if (sem != null)
                    {
                        float dps = (ability.damage > 0f ? ability.damage : 20f) * 0.3f;
                        sem.AddEffect(new StatusEffect(StatusEffectType.Cursed, 6f, dps, gameObject));
                    }

                    EmitHitVFX(ability.hitVFX, hitPos);
                }
                else
                {
                    // Zone 3: Slow & Weakened
                    float dmgVal = (ability.damage > 0f ? ability.damage : 20f) * 0.8f * damageMultiplier;
                    targetHealth.TakeDamage(dmgVal, gameObject);

                    StatusEffectManager sem = hit.GetComponent<StatusEffectManager>() ?? targetHealth.GetComponent<StatusEffectManager>();
                    if (sem != null)
                    {
                        sem.AddEffect(new StatusEffect(StatusEffectType.Slow, 4f, 0.4f, gameObject));
                        sem.AddEffect(new StatusEffect(StatusEffectType.Weakened, 4f, 0.25f, gameObject));
                    }

                    EmitHitVFX(ability.hitVFX, hitPos);
                }
            }
        }
    }

    System.Collections.IEnumerator ApplyHealOverTime(Health target, float tickAmount, int ticks, float interval)
    {
        for (int i = 0; i < ticks; i++)
        {
            yield return new UnityEngine.WaitForSeconds(interval);
            if (target != null && target.IsAlive)
            {
                target.Heal(tickAmount);
            }
        }
    }

    void CastMagicShield(AbilityDef ability)
    {
        Health health = GetComponent<Health>();
        if (health != null)
            health.ApplyShield(ability.shieldAbsorb);

        // Remove any existing shield VFX before spawning new one
        if (activeShieldVFX != null)
            Destroy(activeShieldVFX);

        if (ability.castVFX != null)
        {
            activeShieldVFX = Instantiate(ability.castVFX, transform.position, Quaternion.identity, transform);
            activeShieldVFX.transform.localPosition = Vector3.zero;
            shieldVFXTimer = ability.shieldDuration > 0f ? ability.shieldDuration : 5f;
        }
    }

    void SpawnTurret(AbilityDef ability, Vector3 position)
    {
        if (ability.turretPrefab != null)
        {
            GameObject turret = Instantiate(ability.turretPrefab, position, Quaternion.identity);
            turret.name = "Turret";

            ConfigureSpawnedTurret(turret);
        }
        else
        {
            GameObject turret = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            turret.name = "Turret (Placeholder)";
            turret.transform.position = position;
            turret.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            ConfigureSpawnedTurret(turret);
        }
    }

    void ConfigureSpawnedTurret(GameObject turret)
    {
        if (turret == null)
            return;

        TurretController turretController = turret.GetComponent<TurretController>();
        if (turretController == null)
            turretController = turret.AddComponent<TurretController>();

        turretController.owner = gameObject;

        GuardianFollower guardian = turret.GetComponent<GuardianFollower>();
        if (guardian != null)
            guardian.BindToOwner(transform);

        // Replicate to clients so other players see the sentinel/guardian. Guard on
        // NetworkIdentity: non-networked prefabs still work in solo/editor play.
        if (NetworkServer.active && turret.GetComponent<NetworkIdentity>() != null)
            NetworkServer.Spawn(turret);

        DeployableManager.Instance?.Register(
            turret,
            gameObject.GetInstanceID(),
            classPool != null ? GetClassDeployableLimit() : 1);
    }

#if UNITY_EDITOR || !UNITY_SERVER
    void SpawnFireBurst(Vector3 position, Quaternion rotation, float coneRange, float coneAngle)
    {
        GameObject go = new GameObject("FireBurst");
        go.transform.position = position;
        go.transform.rotation = rotation;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        // A freshly added ParticleSystem is already playing; main.duration can only be
        // set while stopped, so clear it before configuring.
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.4f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(coneRange * 0.6f, coneRange * 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 1f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.55f, 0.05f),
            new Color(1f, 0.9f, 0.3f)
        );
        main.maxParticles = 60;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 40) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = Mathf.Clamp(coneAngle / 2f, 1f, 89f);
        shape.radius = 0.15f;

        ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Sprites/Default");
        if (particleShader != null)
            psr.material = new Material(particleShader);

        ps.Play();
        Destroy(go, main.duration + main.startLifetime.constantMax + 0.5f);
    }
#endif // UNITY_EDITOR || !UNITY_SERVER

    // ── Charge-variant system ────────────────────────────────────────────────

    // ── Procedural icon generation ────────────────────────────────────────────────────
    // Creates 64x64 Sprite for any ability whose .icon is still null.
    // Shape symbol + category background color — good enough for play-testing.
    static readonly Dictionary<string, Sprite> _iconCache = new();

    void GenerateProceduralIcons()
    {
        if (spellbook == null) return;
        foreach (var ab in spellbook)
        {
            if (ab == null || ab.icon != null) continue;
            if (_iconCache.TryGetValue(ab.abilityName, out var cached))
            { ab.icon = cached; continue; }
            ab.icon = _iconCache[ab.abilityName] = MakeIconSprite(ab);
        }
    }

    static Sprite MakeIconSprite(AbilityDef ab)
    {
        const int S = 64;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);

        // Category background
        Color bg = ab.category == AbilityCategory.Heal    ? new Color(0.10f, 0.44f, 0.22f) :
                   ab.category == AbilityCategory.Support  ? new Color(0.18f, 0.28f, 0.52f) :
                                                             new Color(0.50f, 0.12f, 0.08f);
        Color bgDark = bg * 0.6f; bgDark.a = 1f;

        for (int y = 0; y < S; y++)
        for (int x = 0; x < S; x++)
        {
            // Subtle radial gradient
            float dx = (x - S * 0.5f) / (S * 0.5f);
            float dy = (y - S * 0.5f) / (S * 0.5f);
            float r  = Mathf.Clamp01(dx * dx + dy * dy);
            tex.SetPixel(x, y, Color.Lerp(bg, bgDark, r * 0.6f));
        }

        // Shape symbol in bright overlay
        Color fg = Color.white; fg.a = 0.82f;
        int cx = S / 2, cy = S / 2;
        switch (ab.shape)
        {
            case AbilityShape.Cone:
                // Upward triangle
                for (int y2 = 6; y2 < S - 6; y2++)
                {
                    float t  = (float)(y2 - 6) / (S - 12);
                    int   hw = Mathf.RoundToInt(t * 20);
                    for (int x2 = cx - hw; x2 <= cx + hw; x2++)
                        if (x2 >= 0 && x2 < S) tex.SetPixel(x2, y2, fg);
                }
                break;
            case AbilityShape.Rectangle:
                // Filled rect with a thin inner frame
                for (int y2 = 14; y2 < S - 14; y2++)
                for (int x2 = 10; x2 < S - 10; x2++)
                {
                    bool border2 = y2 < 17 || y2 > S - 17 || x2 < 13 || x2 > S - 13;
                    Color c = border2 ? fg : fg * 0.55f; c.a = fg.a;
                    tex.SetPixel(x2, y2, c);
                }
                break;
            default: // Circle
                int rad = 22;
                for (int y2 = 0; y2 < S; y2++)
                for (int x2 = 0; x2 < S; x2++)
                {
                    int dx2 = x2 - cx, dy2 = y2 - cy;
                    int dist2 = dx2 * dx2 + dy2 * dy2;
                    if (dist2 <= rad * rad)
                    {
                        bool rim = dist2 >= (rad - 3) * (rad - 3);
                        Color c = rim ? fg : fg * 0.5f; c.a = fg.a;
                        tex.SetPixel(x2, y2, c);
                    }
                }
                break;
        }

        // Border frame
        Color border = new Color(0.04f, 0.04f, 0.04f, 1f);
        for (int i = 0; i < S; i++)
        {
            tex.SetPixel(i, 0, border); tex.SetPixel(i, 1, border);
            tex.SetPixel(i, S - 1, border); tex.SetPixel(i, S - 2, border);
            tex.SetPixel(0, i, border); tex.SetPixel(1, i, border);
            tex.SetPixel(S - 1, i, border); tex.SetPixel(S - 2, i, border);
        }

        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);
    }

    // Expands a stale serialized spellbook that was saved before entries 35-38 existed.
    // Unity serialized data overrides C# field initializers, so new spells appended to
    // the inline array never appear in old prefab saves. This runs before SyncEquipped
    // so class-pool defaultEquipped indices always resolve.
    public void BackfillMissingSpellbookEntries()
    {
        if (spellbook == null) return;

        // 56 entries (0-55): 41 original + 15 new class abilities
        const int RequiredLength = 56;
        if (spellbook.Length >= RequiredLength) return;

        AbilityDef[] expanded = new AbilityDef[RequiredLength];
        System.Array.Copy(spellbook, expanded, spellbook.Length);

        // Fill any missing entries. Only touches null slots so serialized data wins.
        var missing = new (int idx, AbilityDef def)[]
        {
            (35, new AbilityDef { abilityName = "Healing Cone",       shape = AbilityShape.Cone,      category = AbilityCategory.Heal,   range = 10f, coneAngle = 60f,   cooldown = 5f,  targetTag = "Player", healAmount = 25f, shieldAbsorb = 30f }),
            (36, new AbilityDef { abilityName = "Mending Beam",       shape = AbilityShape.Rectangle, category = AbilityCategory.Heal,   range = 12f, rectWidth = 2.0f,  cooldown = 6f,  targetTag = "Player", healAmount = 25f, shieldAbsorb = 30f }),
            (37, new AbilityDef { abilityName = "Conflagration Cone", shape = AbilityShape.Cone,      category = AbilityCategory.Damage, range = 10f, coneAngle = 60f,   cooldown = 5f,  targetTag = "Enemy",  damage = 20f }),
            (38, new AbilityDef { abilityName = "Ember Beam",         shape = AbilityShape.Rectangle, category = AbilityCategory.Damage, range = 12f, rectWidth = 2.0f,  cooldown = 6f,  targetTag = "Enemy",  damage = 25f }),
            (39, new AbilityDef { abilityName = "Ice Spikes",         shape = AbilityShape.Cone,      category = AbilityCategory.Damage, range = 10f, coneAngle = 60f,   cooldown = 5f,  targetTag = "Enemy",  damage = 20f }),
            (40, new AbilityDef { abilityName = "Meteor Shower",      shape = AbilityShape.Circle,    category = AbilityCategory.Damage, range = 14f, indicatorSize = 8f, cooldown = 30f, targetTag = "Enemy",  damage = 30f }),
            // New Arcanist
            (41, new AbilityDef { abilityName = "Fireball",         shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 10f, coneAngle = 55f,   cooldown = 4f,  targetTag = "Enemy", damage = 25f }),
            (42, new AbilityDef { abilityName = "Chain Lightning",   shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 10f, indicatorSize = 3f, cooldown = 5f, targetTag = "Enemy", damage = 30f }),
            (43, new AbilityDef { abilityName = "Frost Nova",        shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 8f,  indicatorSize = 4f, cooldown = 6f, targetTag = "Enemy", damage = 15f }),
            // New Warden
            (44, new AbilityDef { abilityName = "Thorn Volley",      shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 9f,  coneAngle = 50f,   cooldown = 4f,  targetTag = "Enemy", damage = 20f }),
            (45, new AbilityDef { abilityName = "Earth Surge",       shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 8f,  indicatorSize = 3f, cooldown = 5f, targetTag = "Enemy", damage = 20f }),
            (46, new AbilityDef { abilityName = "Vine Grasp",        shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 9f,  indicatorSize = 2f, cooldown = 7f, targetTag = "Enemy", damage = 10f }),
            // New Ironclad
            (47, new AbilityDef { abilityName = "Hammer Strike",     shape = AbilityShape.Rectangle, category = AbilityCategory.Damage,  range = 5f,  rectWidth = 2.5f,  cooldown = 4f,  targetTag = "Enemy", damage = 30f }),
            (48, new AbilityDef { abilityName = "War Cry",           shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f,  indicatorSize = 6f, cooldown = 10f, targetTag = "Player", healAmount = 30f }),
            (49, new AbilityDef { abilityName = "Juggernaut Rush",   shape = AbilityShape.Rectangle, category = AbilityCategory.Damage,  range = 7f,  rectWidth = 2f,    cooldown = 6f,  targetTag = "Enemy", damage = 25f }),
            // New Shadowblade
            (50, new AbilityDef { abilityName = "Blade Flurry",      shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 6f,  coneAngle = 65f,   cooldown = 4f,  targetTag = "Enemy", damage = 25f }),
            (51, new AbilityDef { abilityName = "Poison Cloud",      shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 8f,  indicatorSize = 3f, cooldown = 6f, targetTag = "Enemy", damage = 10f }),
            (52, new AbilityDef { abilityName = "Death Strike",      shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 7f,  indicatorSize = 1.2f,cooldown = 7f, targetTag = "Enemy", damage = 40f }),
            // New Cleric
            (53, new AbilityDef { abilityName = "Holy Bolt",         shape = AbilityShape.Cone,      category = AbilityCategory.Heal,    range = 10f, coneAngle = 55f,   cooldown = 4f,  targetTag = "Player", healAmount = 30f }),
            (54, new AbilityDef { abilityName = "Divine Shield",     shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 0f,  indicatorSize = 5f, cooldown = 12f, targetTag = "Player", shieldAbsorb = 40f, shieldDuration = 5f }),
            (55, new AbilityDef { abilityName = "Smite",             shape = AbilityShape.Cone,      category = AbilityCategory.Damage,  range = 9f,  coneAngle = 60f,   cooldown = 5f,  targetTag = "Enemy", damage = 35f }),
        };

        foreach (var (idx, def) in missing)
            if (expanded[idx] == null) expanded[idx] = def;

        spellbook = expanded;
    }

    // Inject variant data at runtime for spells that have been converted.
    // The serialized prefab data may predate the variants field; this ensures
    // they're always present regardless of what Unity serialized.
    // Patches null castVFX / hitVFX on any already-existing variant without touching stats.
    // Runs after BackfillVariantDefaults so pre-serialized variants missing VFX get filled in.
    void BackfillVariantVFX()
    {
        if (spellbook == null) return;
        foreach (var ab in spellbook)
        {
            if (ab?.variants == null || ab.variants.Length == 0) continue;
            switch (ab.abilityName)
            {
                // ── SHARED ───────────────────────────────────────────────────────────────
                case "Runic Sentinel":
                    FillVFX(ab,0,"FX/dark magic/Effects normal/Magic circle",     "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts");
                    FillVFX(ab,1,"FX/dark magic/Effects normal/Magic circle",     "FX/dark magic/Effects normal/Snow circle");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Healing buff");
                    break;
                case "Void Bolt":
                    FillVFX(ab,0,"FX/dark magic/Dard magic shoot",                "FX/dark magic/Effects normal/Death magic circle");
                    FillVFX(ab,1,"FX/dark magic/Dard magic shoot",                "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks");
                    FillVFX(ab,2,"FX/dark magic/Dard magic shoot",                "FX/dark magic/Effects normal/Magic circle");
                    break;
                case "Mending Circle":
                    FillVFX(ab,0,"FX/dark magic/Effects normal/Magic circle",     "FX/Spells/Human_Spell_Heal");
                    FillVFX(ab,1,"FX/dark magic/Healing buff",                    "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/Spells/Human_SpellAura_Heal");
                    break;
                case "Storm Lash":
                    FillVFX(ab,0,"FX/Spells/Human_SpellAura_Lightning",           "FX/Spells/Spell_LightningStrike");
                    FillVFX(ab,1,"FX/Spells/Human_SpellAura_Lightning",           "FX/dark magic/Effects with projectors/Lightning strike skill");
                    FillVFX(ab,2,"FX/Spells/Human_SpellAura_Lightning",           "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks");
                    break;
                case "Ember Surge":
                    FillVFX(ab,0,"FX/Spells/Spell_FireballLarge",                 "FX/dark magic/Fireball");
                    FillVFX(ab,1,"FX/Spells/Human_SpellAura_Fire",                "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/WildFire");
                    FillVFX(ab,2,"FX/Spells/Human_SpellAura_Fire",                "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/SmallExplosion");
                    break;
                case "Mind Spike":
                    FillVFX(ab,0,"FX/dark magic/Plazma sphere",                   "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion");
                    FillVFX(ab,1,"FX/dark magic/Glowing orbs",                    "FX/Particle Pack/EffectExamples/Legacy Particles/Prefabs/PlasmaExplosionEffect");
                    FillVFX(ab,2,"FX/dark magic/Glowing orbs",                    "FX/dark magic/Effects normal/Magic circle");
                    break;
                case "Binding Wave":
                    FillVFX(ab,0,"FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter", "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts");
                    FillVFX(ab,1,"FX/dark magic/Ground spikes",                   "FX/dark magic/Ground spikes");
                    FillVFX(ab,2,"FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/GroundFog","FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/SmokeEffect");
                    break;
                case "Arcane Ward":
                    FillVFX(ab,0,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Shield buff");
                    FillVFX(ab,1,"FX/dark magic/Magic buff",                      "FX/Spells/Human_SpellAura_Heal");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/Spells/Human_Spell_Shield");
                    break;

                // ── WARDEN ───────────────────────────────────────────────────────────────
                case "Runic Snare":
                    FillVFX(ab,0,"FX/dark magic/Leaves shield",                   "FX/dark magic/Ground spikes");
                    FillVFX(ab,1,"FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas","FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas");
                    FillVFX(ab,2,"FX/dark magic/Leaves shield",                   "FX/dark magic/Effects normal/Snow circle");
                    break;
                case "Battle Hymn":
                    FillVFX(ab,0,"FX/Spells/Human_Spell_Heal",                    "FX/Spells/Human_SpellAura_Heal");
                    FillVFX(ab,1,"FX/dark magic/Magic buff",                      "FX/Spells/Spell_LightningStrike");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Healing buff");
                    break;
                case "Spirit Redirect":
                    FillVFX(ab,0,"FX/dark magic/Leaves shield",                   "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts");
                    FillVFX(ab,1,"FX/dark magic/Effects normal/Lightning strike skill","FX/Spells/Spell_LightningStrike");
                    FillVFX(ab,2,"FX/dark magic/Healing buff",                    "FX/Spells/Human_SpellAura_Heal");
                    break;
                case "Mend":
                    FillVFX(ab,0,"FX/Spells/Human_Spell_Heal",                    "FX/Spells/Human_SpellAura_Heal");
                    FillVFX(ab,1,"FX/Spells/Human_SpellAura_Heal",                "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Healing buff");
                    break;
                case "Conjurer's Surge":
                    FillVFX(ab,0,"FX/dark magic/Effects normal/Magic circle",     "FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter");
                    FillVFX(ab,1,"FX/dark magic/Magic arrow",                     "FX/dark magic/Effects normal/Snow circle");
                    FillVFX(ab,2,"FX/dark magic/Plazma sphere",                   "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion");
                    break;

                // ── IRONCLAD ─────────────────────────────────────────────────────────────
                case "Counter Blow":
                    FillVFX(ab,0,"FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter","FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts");
                    FillVFX(ab,1,"FX/Spells/Human_Spell_Shockwave_Ground",        "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shockwave_Explosion",     "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts");
                    break;
                case "Gravity Slam":
                    FillVFX(ab,0,"FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter","FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts");
                    FillVFX(ab,1,"FX/Spells/Human_Spell_Shockwave_Explosion",     "FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter");
                    FillVFX(ab,2,"FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/DustStorm","FX/dark magic/Effects normal/Snow circle");
                    break;
                case "Shieldwall Charge":
                    FillVFX(ab,0,"FX/Spells/Human_Spell_Shockwave_Ground",        "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts");
                    FillVFX(ab,1,"FX/Spells/Human_Spell_Shockwave_Ground",        "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shockwave_Explosion",     "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect");
                    break;
                case "Stalwart Stance":
                    FillVFX(ab,0,"FX/Spells/Human_Spell_Shield",                  "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts");
                    FillVFX(ab,1,"FX/dark magic/Shield buff",                     "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect");
                    FillVFX(ab,2,"FX/dark magic/Magic buff",                      "FX/Spells/Human_Spell_Shield");
                    break;
                case "Rune Chain":
                    FillVFX(ab,0,"FX/dark magic/Ground spikes",                   "FX/dark magic/Ground spikes");
                    FillVFX(ab,1,"FX/dark magic/Effects normal/Mana wall",        "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts");
                    FillVFX(ab,2,"FX/dark magic/Healing buff",                    "FX/Spells/Human_SpellAura_Heal");
                    break;
                case "Iron Rampart":
                    FillVFX(ab,0,"FX/dark magic/Effects normal/Mana wall",        "FX/Spells/Human_Spell_Shield");
                    FillVFX(ab,1,"FX/dark magic/Effects with projectors/Mana wall","FX/dark magic/Ground spikes");
                    FillVFX(ab,2,"FX/dark magic/Effects normal/Mana wall",        "FX/dark magic/Shield buff");
                    break;

                // ── ARCANIST ─────────────────────────────────────────────────────────────
                case "Arcane Step":
                    FillVFX(ab,0,"FX/dark magic/Magic arrow",                     "FX/Spells/Ice Rend");
                    FillVFX(ab,1,"FX/dark magic/Plazma sphere",                   "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks");
                    FillVFX(ab,2,"FX/Spells/Ice Rend",                            "FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/GroundFog");
                    break;
                case "Void Maw":
                    FillVFX(ab,0,"FX/dark magic/Plazma sphere",                   "FX/dark magic/Effects with projectors/Death magic circle");
                    FillVFX(ab,1,"FX/dark magic/Effects normal/Magic circle",     "FX/dark magic/Glowing orbs");
                    FillVFX(ab,2,"FX/dark magic/Effects normal/Magic circle",     "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion");
                    break;
                case "Forked Lightning":
                    FillVFX(ab,0,"FX/Spells/Human_SpellAura_Lightning",           "FX/Spells/Spell_LightningStrike");
                    FillVFX(ab,1,"FX/Particle Pack/EffectExamples/Legacy Particles/Prefabs/LightnigStormCloud","FX/dark magic/Effects with projectors/Lightning strike skill");
                    FillVFX(ab,2,"FX/Spells/Human_SpellAura_Lightning",           "FX/Particle Pack/EffectExamples/Legacy Particles/Prefabs/ElectricalSparksEffect");
                    break;
                case "Collapsing Void":
                    FillVFX(ab,0,"FX/dark magic/Effects normal/Magic circle",     "FX/dark magic/Effects with projectors/Death magic circle");
                    FillVFX(ab,1,"FX/dark magic/Plazma sphere",                   "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion");
                    FillVFX(ab,2,"FX/dark magic/Effects with projectors/Death magic circle","FX/dark magic/Effects with projectors/Death magic circle");
                    break;
                case "Ether Lance":
                    FillVFX(ab,0,"FX/dark magic/Dard magic shoot",                "FX/dark magic/Effects normal/Death magic circle");
                    FillVFX(ab,1,"FX/dark magic/Magic arrow",                     "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks");
                    FillVFX(ab,2,"FX/dark magic/Plazma sphere",                   "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion");
                    break;

                // ── CLERIC ───────────────────────────────────────────────────────────────
                case "Soul Bond":
                    FillVFX(ab,0,"FX/dark magic/Healing buff",                    "FX/Spells/Human_SpellAura_Heal");
                    FillVFX(ab,1,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Shield buff");
                    FillVFX(ab,2,"FX/dark magic/Healing buff",                    "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies");
                    break;
                case "Spirit Wisps":
                    FillVFX(ab,0,"FX/dark magic/Glowing orbs",                    "FX/dark magic/Glowing orbs");
                    FillVFX(ab,1,"FX/dark magic/Effects normal/Magic circle",     "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Healing buff");
                    break;
                case "Divine Spark":
                    FillVFX(ab,0,"FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn","FX/Spells/Human_Spell_Heal");
                    FillVFX(ab,1,"FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn","FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/LargeFlames");
                    FillVFX(ab,2,"FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion","FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/BigExplosion");
                    break;
                case "Sacred Aegis":
                    FillVFX(ab,0,"FX/dark magic/Leaves shield",                   "FX/dark magic/Shield buff");
                    FillVFX(ab,1,"FX/Spells/Human_Spell_Shield",                  "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn");
                    FillVFX(ab,2,"FX/dark magic/Magic buff",                      "FX/Spells/Human_Spell_Shield");
                    break;
                case "Dispel":
                    FillVFX(ab,0,"FX/Spells/Human_Spell_Heal",                    "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn");
                    FillVFX(ab,1,"FX/dark magic/Healing buff",                    "FX/Spells/Human_SpellAura_Heal");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/Spells/Human_SpellAura_Heal");
                    break;
                case "Temporal Grace":
                    FillVFX(ab,0,"FX/dark magic/Magic buff",                      "FX/Spells/Human_SpellAura_Heal");
                    FillVFX(ab,1,"FX/dark magic/Glowing orbs",                    "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Magic buff");
                    break;

                // ── SHADOWBLADE ──────────────────────────────────────────────────────────
                case "Shadow Veil":
                    FillVFX(ab,0,"FX/dark magic/Feathers buff",                   "FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/SmokeEffect");
                    FillVFX(ab,1,"FX/dark magic/Feathers buff",                   "FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/GroundFog");
                    FillVFX(ab,2,"FX/dark magic/Feathers buff",                   "FX/dark magic/Effects normal/Death magic circle");
                    break;
                case "Silence Ward":
                    FillVFX(ab,0,"FX/dark magic/Effects normal/Magic circle",     "FX/dark magic/Ground spikes");
                    FillVFX(ab,1,"FX/dark magic/Plazma sphere",                   "FX/dark magic/Effects normal/Snow circle");
                    FillVFX(ab,2,"FX/dark magic/Effects with projectors/Death magic circle","FX/dark magic/Effects normal/Death magic circle");
                    break;
                case "Dark Harvest":
                    FillVFX(ab,0,"FX/dark magic/Effects normal/Death magic circle","FX/dark magic/Effects normal/Magic circle");
                    FillVFX(ab,1,"FX/dark magic/Effects with projectors/Death magic circle","FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/LargeFlames");
                    FillVFX(ab,2,"FX/dark magic/Effects normal/Death magic circle","FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect");
                    break;
                case "Dark Mark":
                    FillVFX(ab,0,"FX/dark magic/Dard magic shoot",                "FX/dark magic/Effects normal/Magic circle");
                    FillVFX(ab,1,"FX/dark magic/Magic arrow",                     "FX/dark magic/Effects normal/Death magic circle");
                    FillVFX(ab,2,"FX/dark magic/Plazma sphere",                   "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts");
                    break;
                case "Fan of Blades":
                    FillVFX(ab,0,"FX/dark magic/Dard magic shoot",                "FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts");
                    FillVFX(ab,1,"FX/dark magic/Magic arrow",                     "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect");
                    FillVFX(ab,2,"FX/dark magic/Feathers buff",                   "FX/dark magic/Effects normal/Magic circle");
                    break;

                // ── CLERIC CONE / BEAM ────────────────────────────────────────────────────
                case "Healing Cone":
                    FillVFX(ab,0,"FX/Spells/Human_SpellAura_Heal",               "FX/Spells/Human_Spell_Heal");
                    FillVFX(ab,1,"FX/dark magic/Healing buff",                    "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Shield buff");
                    break;
                case "Mending Beam":
                    FillVFX(ab,0,"FX/Spells/Human_SpellAura_Heal",               "FX/Spells/Human_Spell_Heal");
                    FillVFX(ab,1,"FX/dark magic/Healing buff",                    "FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies");
                    FillVFX(ab,2,"FX/Spells/Human_Spell_Shield",                  "FX/dark magic/Shield buff");
                    break;

                // ── ARCANIST CONE / BEAM ─────────────────────────────────────────────────
                case "Conflagration Cone":
                    FillVFX(ab,0,"FX/Spells/Spell_FireballLarge",                 "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FireBall");
                    FillVFX(ab,1,"FX/Spells/Human_SpellAura_Fire",                "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/WildFire");
                    FillVFX(ab,2,"FX/Spells/Human_SpellAura_Fire",                "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/MediumFlames");
                    break;
                case "Ember Beam":
                    FillVFX(ab,0,"FX/Spells/Spell_FireballLarge",                 "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FireBall");
                    FillVFX(ab,1,"FX/Spells/Human_SpellAura_Fire",                "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FlameThrower");
                    FillVFX(ab,2,"FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/LargeFlames","FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/MediumFlames");
                    break;

                // ── HANGDANGER'S ARCANIST ─────────────────────────────────────────────────
                case "Ice Spikes":
                    FillVFX(ab,0,"FX/dark magic/Effects normal/Ice freeze skill", "FX/dark magic/Effects normal/Ice freeze skill");
                    FillVFX(ab,1,"FX/Spells/Human_SpellAura_Ice",                 "FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/Ice Lance");
                    FillVFX(ab,2,"FX/dark magic/Effects with projectors/Ice freeze skill","FX/Spells/Ice Rend Large");
                    break;
                case "Meteor Shower":
                    FillVFX(ab,0,"FX/dark magic/Effects with projectors/Meteor rain",    "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/SmallExplosion");
                    FillVFX(ab,1,"FX/dark magic/Effects normal/Meteor rain",             "FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/MediumFlames");
                    FillVFX(ab,2,"FX/dark magic/Effects with projectors/Meteor rain Large","FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/BigExplosion");
                    break;
            }
        }
    }

    void FillVFX(AbilityDef ab, int vi, string castPath, string hitPath)
    {
        if (vi >= ab.variants.Length || ab.variants[vi] == null) return;
        var v = ab.variants[vi];

        AbilityDef payload = ResolveVariantSpellbookAbility(ab, v);
        if (payload != null)
        {
            if (payload.castVFX == null) payload.castVFX = Resources.Load<GameObject>(castPath);
            if (payload.hitVFX  == null) payload.hitVFX  = Resources.Load<GameObject>(hitPath);
        }
    }

    void MigrateInlineVariantsToSpellbookReferences()
    {
        if (spellbook == null) return;

        for (int i = 0; i < spellbook.Length; i++)
        {
            AbilityDef owner = spellbook[i];
            if (owner == null || owner.variantOnly || owner.variants == null || owner.variants.Length == 0)
                continue;

            for (int v = 0; v < owner.variants.Length; v++)
            {
                AbilityVariant variant = owner.variants[v];
                if (variant == null) continue;

                bool hasReference = HasVariantSpellbookReference(variant);
                bool hasLegacyPayload = HasLegacyInlineVariantPayload(variant);
                bool hasLegacyName = HasLegacyVariantName(variant);
                if (hasReference && !hasLegacyPayload && !hasLegacyName)
                {
                    AbilityDef referencedPayload = ResolveVariantSpellbookAbility(owner, variant);
                    if (referencedPayload != null && !ReferenceEquals(referencedPayload, owner))
                        referencedPayload.variantOnly = true;
                    continue;
                }

                AbilityDef payload = hasReference
                    ? ResolveVariantSpellbookAbility(owner, variant)
                    : null;

                if (payload == null)
                {
                    string payloadName = BuildVariantPayloadName(owner, variant, v);
                    payload = FindOrCreateVariantPayloadAbility(payloadName);
                }

                if (payload == null || ReferenceEquals(payload, owner))
                    continue;

                bool canCopyLegacyData = !hasReference || payload.variantOnly;
                if ((hasLegacyPayload || hasLegacyName) && canCopyLegacyData)
                    CopyInlineVariantToPayload(owner, variant, payload);

                payload.variantOnly = true;
                variant.spellbookAbilityName = payload.abilityName;
                variant.useSpellbookAbilityIndex = false;
                variant.spellbookAbilityIndex = -1;
                ClearInlineVariantPayload(variant);
            }
        }
    }

    AbilityDef FindOrCreateVariantPayloadAbility(string payloadName)
    {
        if (string.IsNullOrEmpty(payloadName)) return null;

        AbilityDef existing = FindSpellbookAbilityByName(payloadName);
        if (existing != null) return existing;

        var payload = new AbilityDef
        {
            abilityName = payloadName,
            variantOnly = true,
            damage = 0f,
            maxChargeDamage = 0f,
            cooldown = 0f
        };

        int oldLength = spellbook != null ? spellbook.Length : 0;
        AbilityDef[] expanded = new AbilityDef[oldLength + 1];
        if (oldLength > 0) System.Array.Copy(spellbook, expanded, oldLength);
        expanded[oldLength] = payload;
        spellbook = expanded;
        return payload;
    }

    string BuildVariantPayloadName(AbilityDef owner, AbilityVariant variant, int variantIndex)
    {
        string ownerName = !string.IsNullOrEmpty(owner?.abilityName) ? owner.abilityName.Trim() : "Ability";
        string variantName = HasLegacyVariantName(variant) ? variant.variantName.Trim() : $"Variant {variantIndex + 1}";
        string baseName = string.Equals(ownerName, variantName, System.StringComparison.OrdinalIgnoreCase)
            ? $"{ownerName} Variant {variantIndex + 1}"
            : $"{ownerName} {variantName}";

        string candidate = baseName;
        int suffix = 2;
        while (FindSpellbookAbilityByName(candidate) != null)
        {
            candidate = $"{baseName} {suffix}";
            suffix++;
        }
        return candidate;
    }

    static bool HasLegacyVariantName(AbilityVariant variant)
    {
        return variant != null
            && !string.IsNullOrEmpty(variant.variantName)
            && !string.Equals(variant.variantName, "Variant", System.StringComparison.OrdinalIgnoreCase);
    }

    static bool HasLegacyInlineVariantPayload(AbilityVariant variant)
    {
        if (variant == null) return false;
        return variant.damage > 0f
            || variant.healAmount > 0f
            || (variant.hotTicks > 0 && variant.hotTickAmount > 0f)
            || variant.shieldAbsorb > 0f
            || variant.statusDuration > 0f
            || !string.IsNullOrEmpty(variant.targetTag)
            || variant.castVFX != null
            || variant.hitVFX != null
            || (variant.indicatorTint.a > 0f && !IsDefaultLegacyVariantTint(variant.indicatorTint));
    }

    static bool IsDefaultLegacyVariantTint(Color tint)
    {
        return Mathf.Approximately(tint.r, 0.2f)
            && Mathf.Approximately(tint.g, 1f)
            && Mathf.Approximately(tint.b, 0.3f)
            && Mathf.Approximately(tint.a, 0.7f);
    }

    void CopyInlineVariantToPayload(AbilityDef owner, AbilityVariant variant, AbilityDef payload)
    {
        if (owner == null || variant == null || payload == null) return;

        payload.variantOnly = true;
        payload.shape = owner.shape;
        payload.range = owner.range;
        payload.coneAngle = owner.coneAngle;
        payload.rectWidth = owner.rectWidth;
        payload.indicatorSize = owner.indicatorSize;
        payload.cooldown = owner.cooldown;
        payload.castTime = owner.castTime;
        payload.icon = owner.icon;
        payload.category = InferVariantPayloadCategory(owner, variant);
        payload.targetTag = !string.IsNullOrEmpty(variant.targetTag) ? variant.targetTag : owner.targetTag;

        payload.damage = variant.damage;
        payload.maxChargeDamage = variant.damage;
        payload.chargeable = false;
        payload.healAmount = variant.healAmount;
        payload.hotTickAmount = variant.hotTickAmount;
        payload.hotTicks = variant.hotTicks;
        payload.hotInterval = variant.hotInterval;
        payload.shieldAbsorb = variant.shieldAbsorb;
        payload.shieldDuration = variant.shieldDuration;
        payload.statusEffect = variant.statusEffect;
        payload.statusDuration = variant.statusDuration;
        payload.statusValue = variant.statusValue;

        payload.castVFX = variant.castVFX;
        payload.hitVFX = variant.hitVFX;
        payload.variantIndicatorTint = variant.indicatorTint.a > 0f
            ? variant.indicatorTint
            : owner.variantIndicatorTint;
    }

    static AbilityCategory InferVariantPayloadCategory(AbilityDef owner, AbilityVariant variant)
    {
        if (variant.healAmount > 0f || (variant.hotTicks > 0 && variant.hotTickAmount > 0f))
            return AbilityCategory.Heal;
        if (variant.shieldAbsorb > 0f && variant.damage <= 0f)
            return AbilityCategory.Support;
        return owner != null ? owner.category : AbilityCategory.Damage;
    }

    static void ClearInlineVariantPayload(AbilityVariant variant)
    {
        if (variant == null) return;

        variant.variantName = "";
        variant.indicatorTint = Color.clear;
        variant.healAmount = 0f;
        variant.hotTickAmount = 0f;
        variant.hotTicks = 0;
        variant.hotInterval = 1f;
        variant.shieldAbsorb = 0f;
        variant.shieldDuration = 0f;
        variant.damage = 0f;
        variant.statusEffect = default(StatusEffectType);
        variant.statusDuration = 0f;
        variant.statusValue = 0f;
        variant.targetTag = "";
        variant.castVFX = null;
        variant.hitVFX = null;
    }

    void BackfillVariantDefaults()
    {
        if (spellbook == null) return;
        foreach (AbilityDef ability in spellbook)
        {
            if (ability == null) continue;
            // A serialized variants list, even empty, is designer-authored. Do not
            // recreate default variants after the Inspector minus button removes them.
            if (ability.variants != null && !HasBrokenMigratedVariantPayloads(ability))
                continue;

            switch (ability.abilityName)
            {
                // ── SHARED ────────────────────────────────────────────────────────────
                case "Runic Sentinel":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Sentinel",  indicatorTint=new Color(0.2f,0.8f,0.4f,0.75f), targetTag="Enemy",  damage=15f, statusEffect=StatusEffectType.Stagger, statusDuration=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Overwatch", indicatorTint=new Color(0.1f,0.6f,0.3f,0.75f), targetTag="Enemy",  damage=10f, statusEffect=StatusEffectType.Slow,    statusDuration=3f, statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Snow circle") },
                        new AbilityVariant { variantName="Bastion",   indicatorTint=new Color(0.2f,1f,0.6f,0.75f),   targetTag="Player", shieldAbsorb=20f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff") },
                    };
                    break;

                case "Void Bolt":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Shatter", indicatorTint=new Color(0.6f,0.1f,1f,0.75f),   targetTag="Enemy", damage=28f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.7f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle") },
                        new AbilityVariant { variantName="Drain",   indicatorTint=new Color(0.4f,0.1f,0.8f,0.75f), targetTag="Enemy", damage=16f, statusEffect=StatusEffectType.Slow,     statusDuration=3f,  statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks") },
                        new AbilityVariant { variantName="Wither",  indicatorTint=new Color(0.2f,0.0f,0.6f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Weakened, statusDuration=4f,  statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle") },
                    };
                    break;

                case "Mending Circle":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Surge",     indicatorTint=new Color(0.1f,1f,0.2f,0.75f), targetTag="Player", healAmount=45f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal") },
                        new AbilityVariant { variantName="Rain",      indicatorTint=new Color(0.4f,1f,0.1f,0.75f), targetTag="Player", hotTickAmount=12f, hotTicks=4, hotInterval=1.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies") },
                        new AbilityVariant { variantName="Sanctuary", indicatorTint=new Color(0.1f,0.7f,1f,0.75f), targetTag="Player", shieldAbsorb=35f, shieldDuration=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                    };
                    break;

                case "Storm Lash":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Strike", indicatorTint=new Color(1f,1f,0.2f,0.75f),   targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger, statusDuration=0.7f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Lightning"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Lightning strike skill") },
                        new AbilityVariant { variantName="Shock",  indicatorTint=new Color(0.6f,1f,0.2f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Cursed,  statusDuration=4f,  statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Lightning"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Lightning strike skill") },
                        new AbilityVariant { variantName="Zap",    indicatorTint=new Color(0.3f,0.8f,1f,0.75f), targetTag="Enemy", damage=18f, statusEffect=StatusEffectType.Slow,    statusDuration=4f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Lightning"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks") },
                    };
                    break;

                case "Ember Surge":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Flare",   indicatorTint=new Color(1f,0.4f,0.0f,0.75f),  targetTag="Enemy", damage=28f, statusEffect=StatusEffectType.Stagger, statusDuration=0.6f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Fireball") },
                        new AbilityVariant { variantName="Inferno", indicatorTint=new Color(1f,0.6f,0.0f,0.75f),  targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Cursed,  statusDuration=5f,  statusValue=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FlameStream") },
                        new AbilityVariant { variantName="Singe",   indicatorTint=new Color(1f,0.8f,0.15f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Slow,    statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/SmallExplosion") },
                    };
                    break;

                case "Mind Spike":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Pierce",   indicatorTint=new Color(0.8f,0.3f,1f,0.75f),  targetTag="Enemy", damage=45f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Glowing orbs"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion") },
                        new AbilityVariant { variantName="Echo",     indicatorTint=new Color(0.6f,0.2f,0.9f,0.75f), targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Glowing orbs"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Fracture", indicatorTint=new Color(0.4f,0.1f,0.7f,0.75f), targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Weakened, statusDuration=4f,  statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Glowing orbs"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle") },
                    };
                    break;

                case "Binding Wave":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Slam",     indicatorTint=new Color(0.7f,0.7f,0.2f,0.75f), targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Stagger, statusDuration=1f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Chain",    indicatorTint=new Color(0.5f,0.5f,0.1f,0.75f), targetTag="Enemy", damage=12f, statusEffect=StatusEffectType.Cursed,  statusDuration=4f, statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes") },
                        new AbilityVariant { variantName="Suppress", indicatorTint=new Color(0.3f,0.3f,0.0f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Slow,    statusDuration=5f, statusValue=0.4f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/GroundFog"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/SmokeEffect") },
                    };
                    break;

                case "Arcane Ward":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Barrier",  indicatorTint=new Color(0.5f,0.7f,1f,0.75f),  targetTag="Player", shieldAbsorb=30f, shieldDuration=3f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="Ward",     indicatorTint=new Color(0.3f,0.5f,1f,0.75f),  targetTag="Player", shieldAbsorb=50f, shieldDuration=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="Fortress", indicatorTint=new Color(0.1f,0.3f,0.9f,0.75f), targetTag="Player", shieldAbsorb=80f, shieldDuration=8f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield") },
                    };
                    break;

                // ── WARDEN ────────────────────────────────────────────────────────────
                case "Runic Snare":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Trap",          indicatorTint=new Color(0.2f,0.8f,0.3f,0.75f), targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Stagger, statusDuration=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes") },
                        new AbilityVariant { variantName="Venom Snare",   indicatorTint=new Color(0.1f,0.6f,0.2f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Cursed,  statusDuration=6f, statusValue=5f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas") },
                        new AbilityVariant { variantName="Cripple Snare", indicatorTint=new Color(0.0f,0.4f,0.1f,0.75f), targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Slow,    statusDuration=5f, statusValue=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Snow circle") },
                    };
                    break;

                case "Battle Hymn":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Hymn",    indicatorTint=new Color(0.3f,1f,0.4f,0.75f), targetTag="Player", healAmount=25f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="War Song", indicatorTint=new Color(0.5f,0.9f,0.1f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Weakened, statusDuration=3f, statusValue=0.2f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Lightning strike skill") },
                        new AbilityVariant { variantName="Rally",   indicatorTint=new Color(0.1f,1f,0.3f,0.75f), targetTag="Player", healAmount=35f, shieldAbsorb=20f, shieldDuration=3f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                    };
                    break;

                case "Spirit Redirect":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Redirect",   indicatorTint=new Color(0.2f,0.9f,0.4f,0.75f), targetTag="Enemy",  damage=20f, statusEffect=StatusEffectType.Stagger, statusDuration=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Overcharge", indicatorTint=new Color(0.1f,0.7f,0.3f,0.75f), targetTag="Enemy",  damage=30f, statusEffect=StatusEffectType.Weakened, statusDuration=2f, statusValue=0.2f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Lightning strike skill") },
                        new AbilityVariant { variantName="Siphon",     indicatorTint=new Color(0.2f,1f,0.5f,0.75f),   targetTag="Player", healAmount=20f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                    };
                    break;

                case "Mend":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Mend",    indicatorTint=new Color(0.1f,1f,0.2f,0.75f),  targetTag="Player", healAmount=50f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="Mend+HoT",indicatorTint=new Color(0.4f,1f,0.1f,0.75f),  targetTag="Player", healAmount=25f, hotTickAmount=8f, hotTicks=3, hotInterval=1f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies") },
                        new AbilityVariant { variantName="Fortify", indicatorTint=new Color(0.1f,0.8f,1f,0.75f),  targetTag="Player", healAmount=20f, shieldAbsorb=30f, shieldDuration=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff") },
                    };
                    break;

                case "Conjurer's Surge":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Surge",    indicatorTint=new Color(0.2f,1f,0.4f,0.75f),  targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger, statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter") },
                        new AbilityVariant { variantName="Cascade",  indicatorTint=new Color(0.1f,0.8f,0.3f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Slow,    statusDuration=3f,  statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Snow circle") },
                        new AbilityVariant { variantName="Overload", indicatorTint=new Color(0.0f,0.6f,0.2f,0.75f), targetTag="Enemy", damage=50f, statusEffect=StatusEffectType.Weakened, statusDuration=4f,  statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion") },
                    };
                    break;

                // ── IRONCLAD ──────────────────────────────────────────────────────────
                case "Counter Blow":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Counter",  indicatorTint=new Color(1f,0.6f,0.1f,0.75f),  targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Stagger, statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts") },
                        new AbilityVariant { variantName="Riposte",  indicatorTint=new Color(1f,0.45f,0.0f,0.75f), targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Slow,    statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect") },
                        new AbilityVariant { variantName="Punish",   indicatorTint=new Color(0.9f,0.3f,0.0f,0.75f), targetTag="Enemy", damage=60f, statusEffect=StatusEffectType.Weakened, statusDuration=3f,  statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                    };
                    break;

                case "Gravity Slam":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Pull Slam", indicatorTint=new Color(1f,0.55f,0.0f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Stagger, statusDuration=1f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Crush",     indicatorTint=new Color(1f,0.4f,0.0f,0.75f),  targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Weakened, statusDuration=3f, statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter") },
                        new AbilityVariant { variantName="Vortex",    indicatorTint=new Color(0.9f,0.25f,0.0f,0.75f),targetTag="Enemy", damage=10f, statusEffect=StatusEffectType.Slow,    statusDuration=5f, statusValue=0.4f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/GroundFog"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Snow circle") },
                    };
                    break;

                case "Shieldwall Charge":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Charge",        indicatorTint=new Color(1f,0.6f,0.1f,0.75f),  targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Stagger, statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Ground"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Cleave",        indicatorTint=new Color(1f,0.45f,0.0f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Slow,    statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Ground"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts") },
                        new AbilityVariant { variantName="Battering Ram", indicatorTint=new Color(0.9f,0.3f,0.0f,0.75f), targetTag="Enemy", damage=35f, statusEffect=StatusEffectType.Weakened, statusDuration=2f, statusValue=0.2f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Ground"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect") },
                    };
                    break;

                case "Stalwart Stance":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Defensive",    indicatorTint=new Color(1f,0.65f,0.15f,0.75f), targetTag="Player", shieldAbsorb=40f, shieldDuration=6f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts") },
                        new AbilityVariant { variantName="Counter Stance",indicatorTint=new Color(1f,0.5f,0.05f,0.75f), targetTag="Player", healAmount=20f, shieldAbsorb=25f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect") },
                        new AbilityVariant { variantName="Iron Will",    indicatorTint=new Color(0.9f,0.35f,0.0f,0.75f), targetTag="Player", shieldAbsorb=70f, shieldDuration=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield") },
                    };
                    break;

                case "Rune Chain":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Chain",      indicatorTint=new Color(1f,0.6f,0.1f,0.75f),  targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Slow, statusDuration=5f, statusValue=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes") },
                        new AbilityVariant { variantName="Heavy Chain", indicatorTint=new Color(1f,0.45f,0.0f,0.75f), targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Slow, statusDuration=4f, statusValue=0.4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Soul Chain", indicatorTint=new Color(0.9f,0.3f,0.0f,0.75f), targetTag="Player", healAmount=20f, shieldAbsorb=15f, shieldDuration=3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                    };
                    break;

                case "Iron Rampart":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Fortress",  indicatorTint=new Color(1f,0.6f,0.1f,0.75f),  targetTag="Player", shieldAbsorb=20f, shieldDuration=6f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Mana wall"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield") },
                        new AbilityVariant { variantName="Thorns",    indicatorTint=new Color(1f,0.4f,0.0f,0.75f),  targetTag="Enemy",  damage=25f, statusEffect=StatusEffectType.Stagger, statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Mana wall"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes") },
                        new AbilityVariant { variantName="Bulwark",   indicatorTint=new Color(0.9f,0.25f,0.0f,0.75f), targetTag="Player", shieldAbsorb=35f, shieldDuration=8f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Mana wall"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield") },
                    };
                    break;

                // ── ARCANIST ──────────────────────────────────────────────────────────
                case "Arcane Step":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Swift Step",   indicatorTint=new Color(0.5f,0.2f,1f,0.75f),  targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Stagger, statusDuration=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Ice Rend"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Ice Rend") },
                        new AbilityVariant { variantName="Phase Strike", indicatorTint=new Color(0.35f,0.1f,0.9f,0.75f), targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Stagger, statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Ice Rend"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks") },
                        new AbilityVariant { variantName="Void Walk",    indicatorTint=new Color(0.2f,0.0f,0.75f,0.75f), targetTag="Enemy", damage=10f, statusEffect=StatusEffectType.Slow,    statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Ice Rend"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/GroundFog") },
                    };
                    break;

                case "Void Maw":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Crush",    indicatorTint=new Color(0.5f,0.1f,0.9f,0.75f),  targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Stagger, statusDuration=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle") },
                        new AbilityVariant { variantName="Drain",    indicatorTint=new Color(0.35f,0.0f,0.75f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Cursed,  statusDuration=5f, statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Glowing orbs") },
                        new AbilityVariant { variantName="Collapse", indicatorTint=new Color(0.2f,0.0f,0.6f,0.75f),  targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Weakened, statusDuration=4f, statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion") },
                    };
                    break;

                case "Forked Lightning":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Static",   indicatorTint=new Color(0.9f,0.9f,0.3f,0.75f), targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger, statusDuration=0.7f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Lightning"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Lightning strike skill") },
                        new AbilityVariant { variantName="Storm",    indicatorTint=new Color(0.6f,0.8f,0.2f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Cursed,  statusDuration=4f,  statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Legacy Particles/Prefabs/LightnigStormCloud"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Lightning strike skill") },
                        new AbilityVariant { variantName="Overload", indicatorTint=new Color(0.4f,0.6f,1f,0.75f),   targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Weakened, statusDuration=3f,  statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Lightning"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Legacy Particles/Prefabs/ElectricalSparksEffect") },
                    };
                    break;

                case "Collapsing Void":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Rift",         indicatorTint=new Color(0.5f,0.1f,0.9f,0.75f),  targetTag="Enemy", damage=60f, statusEffect=StatusEffectType.Stagger, statusDuration=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle") },
                        new AbilityVariant { variantName="Singularity",  indicatorTint=new Color(0.35f,0.0f,0.75f,0.75f), targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Cursed,  statusDuration=8f,  statusValue=5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion") },
                        new AbilityVariant { variantName="Annihilation", indicatorTint=new Color(0.2f,0.0f,0.6f,0.75f),  targetTag="Enemy", damage=80f, statusEffect=StatusEffectType.Weakened, statusDuration=5f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle") },
                    };
                    break;

                case "Ether Lance":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Pierce",    indicatorTint=new Color(0.5f,0.2f,1f,0.75f),   targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger, statusDuration=0.6f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle") },
                        new AbilityVariant { variantName="Drain",     indicatorTint=new Color(0.35f,0.1f,0.85f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Cursed,  statusDuration=5f, statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks") },
                        new AbilityVariant { variantName="Void Surge",indicatorTint=new Color(0.2f,0.0f,0.7f,0.75f), targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Weakened, statusDuration=3f, statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion") },
                    };
                    break;

                // ── CLERIC ────────────────────────────────────────────────────────────
                case "Soul Bond":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Bond",           indicatorTint=new Color(1f,0.95f,0.3f,0.75f), targetTag="Player", healAmount=25f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="Shielding Bond", indicatorTint=new Color(0.9f,0.85f,0.2f,0.75f), targetTag="Player", shieldAbsorb=30f, shieldDuration=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="Sustain Bond",   indicatorTint=new Color(0.8f,0.75f,0.1f,0.75f), targetTag="Player", hotTickAmount=8f, hotTicks=4, hotInterval=1.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies") },
                    };
                    break;

                case "Spirit Wisps":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Wisp Swarm",    indicatorTint=new Color(1f,0.95f,0.3f,0.75f),  targetTag="Player", healAmount=40f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Glowing orbs") },
                        new AbilityVariant { variantName="Mending Wisps", indicatorTint=new Color(0.9f,1f,0.3f,0.75f),   targetTag="Player", hotTickAmount=10f, hotTicks=4, hotInterval=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies") },
                        new AbilityVariant { variantName="Guard Wisps",   indicatorTint=new Color(0.3f,0.9f,1f,0.75f),   targetTag="Player", healAmount=15f, shieldAbsorb=25f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff") },
                    };
                    break;

                case "Divine Spark":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Smite",     indicatorTint=new Color(1f,0.95f,0.3f,0.75f),  targetTag="Enemy", damage=60f, statusEffect=StatusEffectType.Stagger,  statusDuration=1f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal") },
                        new AbilityVariant { variantName="Sacred Flame",indicatorTint=new Color(1f,0.7f,0.1f,0.75f), targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Cursed,   statusDuration=5f,  statusValue=5f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FlameStream") },
                        new AbilityVariant { variantName="Exorcism",  indicatorTint=new Color(0.9f,0.9f,0.5f,0.75f), targetTag="Enemy", damage=80f, statusEffect=StatusEffectType.Weakened, statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion") },
                    };
                    break;

                case "Sacred Aegis":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Aegis",           indicatorTint=new Color(1f,0.95f,0.3f,0.75f),  targetTag="Player", shieldAbsorb=30f, shieldDuration=8f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn") },
                        new AbilityVariant { variantName="Reinforced Aegis",indicatorTint=new Color(0.9f,0.85f,0.2f,0.75f), targetTag="Player", shieldAbsorb=50f, shieldDuration=6f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn") },
                        new AbilityVariant { variantName="Unyielding Aegis",indicatorTint=new Color(0.8f,0.75f,0.1f,0.75f), targetTag="Player", shieldAbsorb=80f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield") },
                    };
                    break;

                case "Dispel":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Dispel",   indicatorTint=new Color(1f,0.95f,0.3f,0.75f),  targetTag="Player", healAmount=20f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn") },
                        new AbilityVariant { variantName="Purify",   indicatorTint=new Color(0.9f,1f,0.3f,0.75f),   targetTag="Player", healAmount=35f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn") },
                        new AbilityVariant { variantName="Sanctify", indicatorTint=new Color(0.3f,0.9f,1f,0.75f),   targetTag="Player", healAmount=20f, shieldAbsorb=20f, shieldDuration=3f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                    };
                    break;

                case "Temporal Grace":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Grace",       indicatorTint=new Color(1f,0.95f,0.3f,0.75f),  targetTag="Player", healAmount=60f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Magic buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="Time Spiral", indicatorTint=new Color(0.9f,0.85f,0.2f,0.75f), targetTag="Player", healAmount=40f, hotTickAmount=8f, hotTicks=5, hotInterval=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Magic buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies") },
                        new AbilityVariant { variantName="Chronoshift", indicatorTint=new Color(0.7f,0.7f,1f,0.75f),   targetTag="Player", healAmount=80f, shieldAbsorb=40f, shieldDuration=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Magic buff") },
                    };
                    break;

                // ── SHADOWBLADE ───────────────────────────────────────────────────────
                case "Shadow Veil":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Fade",       indicatorTint=new Color(0.5f,0.0f,0.7f,0.75f), targetTag="Player", shieldAbsorb=20f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Feathers buff"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Feathers buff") },
                        new AbilityVariant { variantName="Phantom",    indicatorTint=new Color(0.4f,0.0f,0.6f,0.75f), targetTag="Player", shieldAbsorb=35f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Feathers buff"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Feathers buff") },
                        new AbilityVariant { variantName="Ghost Walk", indicatorTint=new Color(0.3f,0.0f,0.5f,0.75f), targetTag="Player", shieldAbsorb=50f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Feathers buff"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Feathers buff") },
                    };
                    break;

                case "Silence Ward":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Hush",    indicatorTint=new Color(0.6f,0.0f,0.8f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Slow,    statusDuration=5f, statusValue=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes") },
                        new AbilityVariant { variantName="Nullify", indicatorTint=new Color(0.5f,0.0f,0.7f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Cursed,  statusDuration=4f, statusValue=3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Snow circle") },
                        new AbilityVariant { variantName="Void Silence",indicatorTint=new Color(0.4f,0.0f,0.6f,0.75f),targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Weakened, statusDuration=4f, statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle") },
                    };
                    break;

                case "Dark Harvest":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Harvest",      indicatorTint=new Color(0.6f,0.0f,0.8f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Weakened, statusDuration=4f, statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle") },
                        new AbilityVariant { variantName="Blood Harvest",indicatorTint=new Color(0.7f,0.0f,0.5f,0.75f), targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Cursed,   statusDuration=5f, statusValue=5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FlameStream") },
                        new AbilityVariant { variantName="Soul Harvest", indicatorTint=new Color(0.5f,0.0f,0.4f,0.75f), targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Stagger,  statusDuration=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect") },
                    };
                    break;

                case "Dark Mark":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Mark",  indicatorTint=new Color(0.6f,0.0f,0.8f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Cursed,   statusDuration=4f, statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle") },
                        new AbilityVariant { variantName="Scar",  indicatorTint=new Color(0.5f,0.0f,0.6f,0.75f), targetTag="Enemy", damage=28f, statusEffect=StatusEffectType.Weakened, statusDuration=3f, statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle") },
                        new AbilityVariant { variantName="Brand", indicatorTint=new Color(0.4f,0.0f,0.5f,0.75f), targetTag="Enemy", damage=35f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.6f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                    };
                    break;

                case "Fan of Blades":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Slash",  indicatorTint=new Color(0.7f,0.0f,1f,0.75f),   targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.6f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts") },
                        new AbilityVariant { variantName="Bleed",  indicatorTint=new Color(0.5f,0.0f,0.8f,0.75f), targetTag="Enemy", damage=10f, statusEffect=StatusEffectType.Cursed,   statusDuration=5f,  statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect") },
                        new AbilityVariant { variantName="Expose", indicatorTint=new Color(0.3f,0.0f,0.6f,0.75f), targetTag="Enemy", damage=14f, statusEffect=StatusEffectType.Weakened, statusDuration=4f,  statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle") },
                    };
                    break;

                // ── CLERIC CONE / BEAM ─────────────────────────────────────────────────
                case "Healing Cone":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="HPS",    indicatorTint=new Color(0.1f,1f,0.2f,0.75f), targetTag="Player", healAmount=45f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal") },
                        new AbilityVariant { variantName="HOT",    indicatorTint=new Color(0.5f,1f,0.1f,0.75f), targetTag="Player", hotTickAmount=8f, hotTicks=5, hotInterval=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies") },
                        new AbilityVariant { variantName="BUBBLE", indicatorTint=new Color(0.1f,0.7f,1f,0.75f), targetTag="Player", shieldAbsorb=40f, shieldDuration=6f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff") },
                    };
                    break;

                case "Mending Beam":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="HPS",    indicatorTint=new Color(0.1f,1f,0.2f,0.75f), targetTag="Player", healAmount=45f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal") },
                        new AbilityVariant { variantName="HOT",    indicatorTint=new Color(0.5f,1f,0.1f,0.75f), targetTag="Player", hotTickAmount=8f, hotTicks=5, hotInterval=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies") },
                        new AbilityVariant { variantName="BUBBLE", indicatorTint=new Color(0.1f,0.7f,1f,0.75f), targetTag="Player", shieldAbsorb=40f, shieldDuration=6f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff") },
                    };
                    break;

                // ── ARCANIST CONE / BEAM ───────────────────────────────────────────────
                case "Conflagration Cone":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Burst",  indicatorTint=new Color(1f,0.15f,0.0f,0.75f), targetTag="Enemy", damage=32f, statusEffect=StatusEffectType.Stagger, statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FireBall") },
                        new AbilityVariant { variantName="Burn",   indicatorTint=new Color(1f,0.45f,0.0f,0.75f), targetTag="Enemy", damage=12f, statusEffect=StatusEffectType.Cursed,  statusDuration=6f,  statusValue=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FlameStream") },
                        new AbilityVariant { variantName="Scorch", indicatorTint=new Color(1f,0.75f,0.0f,0.75f), targetTag="Enemy", damage=16f, statusEffect=StatusEffectType.Slow,    statusDuration=4f,  statusValue=0.4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/MediumFlames") },
                    };
                    break;

                case "Ember Beam":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Burst",  indicatorTint=new Color(1f,0.15f,0.0f,0.75f), targetTag="Enemy", damage=32f, statusEffect=StatusEffectType.Stagger, statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FireBall") },
                        new AbilityVariant { variantName="Burn",   indicatorTint=new Color(1f,0.45f,0.0f,0.75f), targetTag="Enemy", damage=12f, statusEffect=StatusEffectType.Cursed,  statusDuration=6f,  statusValue=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FlameStream") },
                        new AbilityVariant { variantName="Scorch", indicatorTint=new Color(1f,0.75f,0.0f,0.75f), targetTag="Enemy", damage=16f, statusEffect=StatusEffectType.Slow,    statusDuration=4f,  statusValue=0.4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Fire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/MediumFlames") },
                    };
                    break;

                // ── HANGDANGER'S ARCANIST SPELLS ─────────────────────────────────────
                case "Ice Spikes":
                    // Hangdanger: cone eruption changed from rect→cone; VFX = "Ice freeze skill"
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Frost Spikes",  indicatorTint=new Color(0.4f,0.85f,1f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Slow,    statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Ice freeze skill"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Ice freeze skill") },
                        new AbilityVariant { variantName="Ice Lance",     indicatorTint=new Color(0.2f,0.7f,1f,0.75f),  targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Ice freeze skill"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Ice Rend") },
                        new AbilityVariant { variantName="Glacial Prison",indicatorTint=new Color(0.1f,0.5f,0.9f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Bound,    statusDuration=2.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Ice freeze skill"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects with projectors/Ice freeze skill") },
                    };
                    break;

                case "Meteor Shower":
                    // Hangdanger: delayed AoE bombardment; VFX = Arcanist_MeteorIpact prefab
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Shower",     indicatorTint=new Color(1f,0.5f,0.1f,0.75f),  targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.6f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects with projectors/Meteor rain"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/SmallExplosion") },
                        new AbilityVariant { variantName="Barrage",    indicatorTint=new Color(1f,0.35f,0.0f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Cursed,   statusDuration=4f,  statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects with projectors/Meteor rain"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/MediumFlames") },
                        new AbilityVariant { variantName="Extinction", indicatorTint=new Color(0.9f,0.2f,0.0f,0.75f), targetTag="Enemy", damage=50f, statusEffect=StatusEffectType.Weakened, statusDuration=4f,  statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects with projectors/Meteor rain"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/BigExplosion") },
                    };
                    break;

                // ── NEW ARCANIST ──────────────────────────────────────────────────────
                case "Fireball":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Quick Shot",   indicatorTint=new Color(1f,0.5f,0.1f,0.75f),  targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Stagger, statusDuration=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Spell_Fireball"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/SmallExplosion") },
                        new AbilityVariant { variantName="Triple Burst", indicatorTint=new Color(1f,0.35f,0.0f,0.75f), targetTag="Enemy", damage=18f, statusEffect=StatusEffectType.Cursed,  statusDuration=4f, statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Spell_FireballLarge"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/FireBall") },
                        new AbilityVariant { variantName="Inferno Nova", indicatorTint=new Color(0.9f,0.15f,0.0f,0.75f), targetTag="Enemy", damage=45f, statusEffect=StatusEffectType.Weakened, statusDuration=3f, statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/WildFire"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/BigExplosion") },
                    };
                    break;

                case "Chain Lightning":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Arc",         indicatorTint=new Color(0.9f,0.9f,0.3f,0.75f), targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.7f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Lightning"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Spell_LightningStrike") },
                        new AbilityVariant { variantName="Chain",       indicatorTint=new Color(0.5f,0.8f,0.2f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Cursed,   statusDuration=4f,  statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Legacy Particles/Prefabs/LightnigStormCloud"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects with projectors/Lightning strike skill") },
                        new AbilityVariant { variantName="Thunderstorm",indicatorTint=new Color(0.3f,0.6f,1f,0.75f),   targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Weakened, statusDuration=3f,  statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Spell_LightningStrike"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Legacy Particles/Prefabs/ElectricalSparksEffect") },
                    };
                    break;

                case "Frost Nova":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Chill",       indicatorTint=new Color(0.5f,0.85f,1f,0.75f),  targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Slow,  statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Snow circle"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Snow circle") },
                        new AbilityVariant { variantName="Freeze",      indicatorTint=new Color(0.3f,0.7f,1f,0.75f),   targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Bound, statusDuration=2.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Ice freeze skill"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/Ice Lance") },
                        new AbilityVariant { variantName="Blizzard",    indicatorTint=new Color(0.1f,0.5f,0.9f,0.75f), targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Slow,  statusDuration=5f,  statusValue=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects with projectors/Snow circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Ice Rend Large") },
                    };
                    break;

                // ── NEW WARDEN ────────────────────────────────────────────────────────
                case "Thorn Volley":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Spine",       indicatorTint=new Color(0.2f,0.8f,0.2f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Stagger, statusDuration=0.5f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Volley",      indicatorTint=new Color(0.1f,0.6f,0.1f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Slow,    statusDuration=3f,  statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Goop Effects/Prefabs/GoopSpray") },
                        new AbilityVariant { variantName="Briar Storm", indicatorTint=new Color(0.0f,0.4f,0.0f,0.75f), targetTag="Enemy", damage=12f, statusEffect=StatusEffectType.Cursed,  statusDuration=5f,  statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Goop Effects/Prefabs/GoopSprayEffect") },
                    };
                    break;

                case "Earth Surge":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Tremor",      indicatorTint=new Color(0.6f,0.4f,0.1f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Stagger, statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Quake",       indicatorTint=new Color(0.5f,0.3f,0.0f,0.75f), targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Slow,    statusDuration=4f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/DustStorm") },
                        new AbilityVariant { variantName="Fissure",     indicatorTint=new Color(0.4f,0.2f,0.0f,0.75f), targetTag="Enemy", damage=45f, statusEffect=StatusEffectType.Weakened, statusDuration=4f, statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter") },
                    };
                    break;

                case "Vine Grasp":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Root",         indicatorTint=new Color(0.2f,0.7f,0.1f,0.75f), targetTag="Enemy", damage=10f, statusEffect=StatusEffectType.Bound,   statusDuration=2f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes") },
                        new AbilityVariant { variantName="Stranglehold", indicatorTint=new Color(0.1f,0.5f,0.1f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Bound,   statusDuration=3f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Goop Effects/Prefabs/GoopStreamEffect") },
                        new AbilityVariant { variantName="Forest Prison",indicatorTint=new Color(0.0f,0.35f,0.0f,0.75f), targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Bound,  statusDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Leaves shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Goop Effects/Prefabs/GoopSpray") },
                    };
                    break;

                // ── NEW IRONCLAD ──────────────────────────────────────────────────────
                case "Hammer Strike":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Quick Blow",   indicatorTint=new Color(1f,0.6f,0.1f,0.75f),  targetTag="Enemy", damage=30f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.7f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts") },
                        new AbilityVariant { variantName="Heavy Slam",   indicatorTint=new Color(1f,0.45f,0.0f,0.75f), targetTag="Enemy", damage=50f, statusEffect=StatusEffectType.Weakened, statusDuration=3f,  statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Ground"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Seismic Slam", indicatorTint=new Color(0.9f,0.25f,0.0f,0.75f), targetTag="Enemy", damage=70f, statusEffect=StatusEffectType.Slow,   statusDuration=4f,  statusValue=0.4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Explosion"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Magic Effects/Prefabs/EarthShatter") },
                    };
                    break;

                case "War Cry":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Battle Shout", indicatorTint=new Color(1f,0.65f,0.15f,0.75f), targetTag="Player", healAmount=30f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Magic buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal") },
                        new AbilityVariant { variantName="Rally Cry",    indicatorTint=new Color(1f,0.5f,0.05f,0.75f), targetTag="Player", healAmount=15f, shieldAbsorb=25f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Shield buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="Primal Roar",  indicatorTint=new Color(0.9f,0.35f,0.0f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Stagger, statusDuration=1f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Explosion"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts") },
                    };
                    break;

                case "Juggernaut Rush":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Charge",       indicatorTint=new Color(1f,0.6f,0.1f,0.75f),   targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Ground"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts") },
                        new AbilityVariant { variantName="Bull Rush",     indicatorTint=new Color(1f,0.45f,0.0f,0.75f),  targetTag="Enemy", damage=35f, statusEffect=StatusEffectType.Slow,     statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Explosion"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/StoneImpacts") },
                        new AbilityVariant { variantName="Wrecking Ball", indicatorTint=new Color(0.9f,0.25f,0.0f,0.75f), targetTag="Enemy", damage=60f, statusEffect=StatusEffectType.Weakened, statusDuration=4f, statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/BigExplosion"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shockwave_Explosion") },
                    };
                    break;

                // ── NEW SHADOWBLADE ───────────────────────────────────────────────────
                case "Blade Flurry":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Slash",        indicatorTint=new Color(0.7f,0.0f,1f,0.75f),   targetTag="Enemy", damage=25f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.6f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts") },
                        new AbilityVariant { variantName="Flurry",       indicatorTint=new Color(0.5f,0.0f,0.8f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Cursed,   statusDuration=4f,  statusValue=4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Magic arrow"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/SparksEffect") },
                        new AbilityVariant { variantName="Maelstrom",    indicatorTint=new Color(0.3f,0.0f,0.6f,0.75f), targetTag="Enemy", damage=10f, statusEffect=StatusEffectType.Weakened, statusDuration=4f,  statusValue=0.25f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Feathers buff"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Ground spikes") },
                    };
                    break;

                case "Poison Cloud":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Mist",         indicatorTint=new Color(0.4f,0.7f,0.1f,0.75f), targetTag="Enemy", damage=10f, statusEffect=StatusEffectType.Cursed, statusDuration=4f, statusValue=3f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas") },
                        new AbilityVariant { variantName="Miasma",       indicatorTint=new Color(0.2f,0.5f,0.0f,0.75f), targetTag="Enemy", damage=15f, statusEffect=StatusEffectType.Cursed, statusDuration=6f, statusValue=5f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/GroundFog"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas") },
                        new AbilityVariant { variantName="Death Fog",    indicatorTint=new Color(0.1f,0.3f,0.0f,0.75f), targetTag="Enemy", damage=20f, statusEffect=StatusEffectType.Slow,   statusDuration=4f, statusValue=0.4f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Smoke & Steam Effects/Prefabs/PoisonGas") },
                    };
                    break;

                case "Death Strike":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Stab",          indicatorTint=new Color(0.6f,0.0f,0.8f,0.75f), targetTag="Enemy", damage=40f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Dard magic shoot"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects normal/Death magic circle") },
                        new AbilityVariant { variantName="Deep Cut",      indicatorTint=new Color(0.4f,0.0f,0.6f,0.75f), targetTag="Enemy", damage=35f, statusEffect=StatusEffectType.Weakened, statusDuration=4f, statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Magic arrow"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects with projectors/Death magic circle") },
                        new AbilityVariant { variantName="Assassination", indicatorTint=new Color(0.2f,0.0f,0.4f,0.75f), targetTag="Enemy", damage=75f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Plazma sphere"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Effects with projectors/Death magic circle") },
                    };
                    break;

                // ── NEW CLERIC ────────────────────────────────────────────────────────
                case "Holy Bolt":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Flash",         indicatorTint=new Color(1f,0.95f,0.4f,0.75f), targetTag="Player", healAmount=30f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal") },
                        new AbilityVariant { variantName="Radiance",      indicatorTint=new Color(1f,0.85f,0.2f,0.75f), targetTag="Player", healAmount=45f, hotTickAmount=5f, hotTicks=3, hotInterval=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Healing buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/FireFlies") },
                        new AbilityVariant { variantName="Divine Ray",    indicatorTint=new Color(0.9f,0.75f,0.1f,0.75f), targetTag="Player", healAmount=70f, shieldAbsorb=20f, shieldDuration=4f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                    };
                    break;

                case "Divine Shield":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Shelter",       indicatorTint=new Color(1f,0.95f,0.4f,0.75f), targetTag="Player", shieldAbsorb=40f, shieldDuration=5f,
                            castVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield"),
                            hitVFX = Resources.Load<GameObject>("FX/dark magic/Shield buff") },
                        new AbilityVariant { variantName="Bastion",       indicatorTint=new Color(0.9f,0.85f,0.2f,0.75f), targetTag="Player", shieldAbsorb=70f, shieldDuration=6f, healAmount=20f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Shield buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_SpellAura_Heal") },
                        new AbilityVariant { variantName="Cathedral",     indicatorTint=new Color(0.8f,0.7f,0.1f,0.75f), targetTag="Player", shieldAbsorb=120f, shieldDuration=8f, hotTickAmount=10f, hotTicks=5, hotInterval=1f,
                            castVFX = Resources.Load<GameObject>("FX/dark magic/Magic buff"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Shield") },
                    };
                    break;

                case "Smite":
                    ability.variants = new AbilityVariant[]
                    {
                        new AbilityVariant { variantName="Strike",        indicatorTint=new Color(1f,0.95f,0.4f,0.75f),  targetTag="Enemy", damage=35f, statusEffect=StatusEffectType.Stagger,  statusDuration=0.8f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn"),
                            hitVFX = Resources.Load<GameObject>("FX/Spells/Human_Spell_Heal") },
                        new AbilityVariant { variantName="Judgement",     indicatorTint=new Color(1f,0.7f,0.1f,0.75f),   targetTag="Enemy", damage=50f, statusEffect=StatusEffectType.Slow,     statusDuration=3f,  statusValue=0.35f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Misc Effects/Prefabs/Respawn"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/LargeFlames") },
                        new AbilityVariant { variantName="Wrath",         indicatorTint=new Color(0.9f,0.5f,0.0f,0.75f), targetTag="Enemy", damage=75f, statusEffect=StatusEffectType.Weakened, statusDuration=4f,  statusValue=0.3f,
                            castVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/EnergyExplosion"),
                            hitVFX = Resources.Load<GameObject>("FX/Particle Pack/EffectExamples/Fire & Explosion Effects/Prefabs/BigExplosion") },
                    };
                    break;
            }
        }
    }

    // Draws zone-divider arcs and animates the active zone for cone variant spells.
    void UpdateConeZoneArcs(GameObject indicator, AbilityDef ability, ConeAimData coneData)
    {
        if (ability.variants == null || ability.variants.Length < 2 || !coneData.valid) return;

        const int arcSegs = 22;
        float pulse  = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);    // 0→1 at 6 Hz
        float pulseW = 0.08f + 0.08f * pulse;                        // width breathes 0.08→0.16

        Color activeVariantTint = GetVariantTint(ability, _activeVariantIndex);

        for (int z = 1; z < ability.variants.Length; z++)
        {
            Transform arcT = indicator.transform.Find($"ZoneArc_{z}");
            if (arcT == null) continue;
            LineRenderer arcLR = arcT.GetComponent<LineRenderer>();
            if (arcLR == null) continue;

            float t        = (float)z / ability.variants.Length;
            float arcRange = coneData.visualRange * t;

            arcLR.positionCount = arcSegs + 1;
            for (int i = 0; i <= arcSegs; i++)
            {
                float frac  = i / (float)arcSegs;
                float angle = Mathf.Lerp(-coneData.halfAngle, coneData.halfAngle, frac);
                Vector3 dir = Quaternion.AngleAxis(angle, coneData.visualNormal) * coneData.visualForward;
                arcLR.SetPosition(i, ProjectToGround(coneData.origin + dir * arcRange));
            }

            bool nearActive = (z == _activeVariantIndex || z == _activeVariantIndex + 1);
            if (nearActive)
            {
                arcLR.startWidth = arcLR.endWidth = pulseW;
                float a = 0.65f + 0.35f * pulse;
                arcLR.startColor = arcLR.endColor = new Color(
                    activeVariantTint.r * 0.8f + 0.2f,
                    activeVariantTint.g * 0.8f + 0.2f,
                    activeVariantTint.b * 0.8f + 0.2f, a);
            }
            else
            {
                arcLR.startWidth = arcLR.endWidth = 0.04f;
                arcLR.startColor = arcLR.endColor = new Color(0.4f, 0.4f, 0.4f, 0.25f);
            }
        }

        // Zone cursor: a bright diamond that sits on the cone center-axis at the midpoint
        // of the active zone and pulses, making it obvious which zone is selected.
        UpdateZoneCursor(indicator, ability, coneData, pulse, activeVariantTint);

        // Zone name floats above the cursor
        UpdateZoneLabel(indicator, ability, coneData, activeVariantTint);
    }

    // Draws crossbar zone-divider lines and animates the active band for rect variant spells.
    void UpdateRectZoneMarkers(GameObject indicator, AbilityDef ability, RectangleAimData rectData)
    {
        if (ability.variants == null || ability.variants.Length < 2 || !rectData.valid) return;

        float pulse  = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);
        float pulseW = 0.08f + 0.08f * pulse;

        Color activeVariantTint = GetVariantTint(ability, _activeVariantIndex);

        for (int z = 1; z < ability.variants.Length; z++)
        {
            Transform barT = indicator.transform.Find($"RectZone_{z}");
            if (barT == null) continue;
            LineRenderer barLR = barT.GetComponent<LineRenderer>();
            if (barLR == null) continue;

            float t = (float)z / ability.variants.Length;
            Vector3 left  = Vector3.Lerp(rectData.corners[0], rectData.corners[3], t);
            Vector3 right = Vector3.Lerp(rectData.corners[1], rectData.corners[2], t);
            barLR.SetPosition(0, left);
            barLR.SetPosition(1, right);

            bool nearActive = (z == _activeVariantIndex || z == _activeVariantIndex + 1);
            if (nearActive)
            {
                barLR.startWidth = barLR.endWidth = pulseW;
                float a = 0.65f + 0.35f * pulse;
                barLR.startColor = barLR.endColor = new Color(
                    activeVariantTint.r * 0.8f + 0.2f,
                    activeVariantTint.g * 0.8f + 0.2f,
                    activeVariantTint.b * 0.8f + 0.2f, a);
            }
            else
            {
                barLR.startWidth = barLR.endWidth = 0.04f;
                barLR.startColor = barLR.endColor = new Color(0.4f, 0.4f, 0.4f, 0.25f);
            }
        }

        // Rect zone label
        UpdateRectZoneLabel(indicator, ability, rectData, activeVariantTint);
    }

    // Small bright dot on the cone centre-axis at the midpoint of the active zone.
    void UpdateZoneCursor(GameObject indicator, AbilityDef ability, ConeAimData coneData, float pulse, Color tint)
    {
        const string CursorName = "ZoneCursor";
        Transform cursorT = indicator.transform.Find(CursorName);
        if (cursorT == null)
        {
            var go = new GameObject(CursorName);
            go.transform.SetParent(indicator.transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = false;
            lr.positionCount = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            cursorT = go.transform;
        }

        int n = ability.variants.Length;
        float tMin = (float)_activeVariantIndex / n;
        float tMax = (float)(_activeVariantIndex + 1) / n;
        float tMid = (tMin + tMax) * 0.5f;

        // Draw a short perpendicular tick on the centerline at the zone midpoint
        float midRange  = coneData.visualRange * tMid;
        Vector3 midBase = ProjectToGround(coneData.origin + coneData.visualForward * midRange);
        Vector3 right   = Vector3.Cross(coneData.visualForward, Vector3.up).normalized;
        float tickHalf  = ability.range * 0.07f;

        var cursorLR = cursorT.GetComponent<LineRenderer>();
        if (cursorLR != null)
        {
            cursorLR.positionCount = 2;
            cursorLR.SetPosition(0, midBase - right * tickHalf);
            cursorLR.SetPosition(1, midBase + right * tickHalf);
            float w = 0.10f + 0.06f * pulse;
            cursorLR.startWidth = cursorLR.endWidth = w;
            float a = 0.7f + 0.3f * pulse;
            cursorLR.startColor = cursorLR.endColor = new Color(tint.r, tint.g, tint.b, a);
        }
    }

    // World-space variant name tag floating above the active zone midpoint.
    void UpdateZoneLabel(GameObject indicator, AbilityDef ability, ConeAimData coneData, Color tint)
    {
        const string LabelName = "ZoneLabel";
        Transform lblT = indicator.transform.Find(LabelName);
        if (lblT == null)
        {
            var go = new GameObject(LabelName);
            go.transform.SetParent(indicator.transform, false);
            var tmp = go.AddComponent<TMPro.TextMeshPro>();
            tmp.fontSize     = 0.38f;
            tmp.fontStyle    = TMPro.FontStyles.Bold;
            tmp.alignment    = TMPro.TextAlignmentOptions.Center;
            tmp.color        = Color.white;
            tmp.sortingOrder = 10;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 1f);
            lblT = go.transform;
        }

        int n = ability.variants.Length;
        float tMid = ((float)_activeVariantIndex + 0.5f) / n;
        Vector3 pos = ProjectToGround(coneData.origin + coneData.visualForward * (coneData.visualRange * tMid));
        pos.y += 0.06f;

        lblT.position = pos;
        // Lie flat on ground; text rows run along the indicator's forward direction.
        lblT.rotation = Quaternion.LookRotation(Vector3.down, coneData.visualForward);

        var tmp2 = lblT.GetComponent<TMPro.TextMeshPro>();
        if (tmp2 != null)
        {
            tmp2.text  = GetVariantDisplayName(ability, _activeVariantIndex).ToUpper();
            tmp2.color = new Color(tint.r * 0.7f + 0.3f, tint.g * 0.7f + 0.3f, tint.b * 0.7f + 0.3f, 0.95f);
        }
    }

    // Same label for rectangle shapes.
    void UpdateRectZoneLabel(GameObject indicator, AbilityDef ability, RectangleAimData rectData, Color tint)
    {
        const string LabelName = "ZoneLabel";
        Transform lblT = indicator.transform.Find(LabelName);
        if (lblT == null)
        {
            var go = new GameObject(LabelName);
            go.transform.SetParent(indicator.transform, false);
            var tmp = go.AddComponent<TMPro.TextMeshPro>();
            tmp.fontSize     = 0.38f;
            tmp.fontStyle    = TMPro.FontStyles.Bold;
            tmp.alignment    = TMPro.TextAlignmentOptions.Center;
            tmp.color        = Color.white;
            tmp.sortingOrder = 10;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 1f);
            lblT = go.transform;
        }

        int n = ability.variants.Length;
        float tMid  = ((float)_activeVariantIndex + 0.5f) / n;
        Vector3 midLeft  = Vector3.Lerp(rectData.corners[0], rectData.corners[3], tMid);
        Vector3 midRight = Vector3.Lerp(rectData.corners[1], rectData.corners[2], tMid);
        Vector3 pos = (midLeft + midRight) * 0.5f;
        pos.y += 0.06f;

        lblT.position = pos;
        lblT.rotation = Quaternion.LookRotation(Vector3.down, rectData.visualForward);

        var tmp2 = lblT.GetComponent<TMPro.TextMeshPro>();
        if (tmp2 != null)
        {
            tmp2.text  = GetVariantDisplayName(ability, _activeVariantIndex).ToUpper();
            tmp2.color = new Color(tint.r * 0.7f + 0.3f, tint.g * 0.7f + 0.3f, tint.b * 0.7f + 0.3f, 0.95f);
        }
    }

    // Resolves a variant by using another spellbook entry as its payload.
    // Runs server-side in networked play (called via CmdFinalizeCast -> FinalizeCast).
    void ResolveReferencedVariantCast(
        AbilityDef selectorAbility,
        AbilityDef effectAbility,
        GameObject indicator,
        float aimTime,
        float damageMultiplier,
        Vector3 castOrigin)
    {
        if (selectorAbility == null || effectAbility == null) return;

#if UNITY_EDITOR || !UNITY_SERVER
        if (effectAbility.category == AbilityCategory.Heal) OnHealCast?.Invoke();
#endif

        GameObject hitVfxPrefab = effectAbility.hitVFX != null ? effectAbility.hitVFX : selectorAbility.hitVFX;

        if (indicator != null && HasDirectPayload(effectAbility))
        {
            var hits = new System.Collections.Generic.List<Collider>();
            CollectHitsForAbilityShape(effectAbility, indicator, castOrigin, effectAbility.targetTag, hits);
            foreach (Collider hit in hits)
                ApplyAbilityDefPayloadToHit(effectAbility, hit, hitVfxPrefab, damageMultiplier, aimTime);
        }

        Vector3 castPoint = GetCastPointForAbility(effectAbility, indicator, castOrigin);
        Quaternion castRotation = indicator != null ? indicator.transform.rotation : transform.rotation;
        GameObject castVfxPrefab = effectAbility.castVFX != null ? effectAbility.castVFX : selectorAbility.castVFX;
        SpawnCastVFXForAbility(effectAbility, castVfxPrefab, castPoint, castRotation, castOrigin);

        if (effectAbility.spawnTurret && indicator != null)
            SpawnTurret(effectAbility, indicator.transform.position);

        DispatchAbility(effectAbility, castPoint, damageMultiplier);
        StartPulseDamageIfNeeded(effectAbility, castPoint, damageMultiplier);
    }

    bool HasBrokenMigratedVariantPayloads(AbilityDef ability)
    {
        if (ability?.variants == null || ability.variants.Length <= 1) return false;

        for (int i = 0; i < ability.variants.Length; i++)
        {
            AbilityVariant variant = ability.variants[i];
            if (variant == null) continue;
            if (!HasVariantSpellbookReference(variant)) return false;

            AbilityDef payload = ResolveVariantSpellbookAbility(ability, variant);
            if (payload != null && payload.variantOnly && !HasMeaningfulPayload(payload))
                return true;
        }

        return false;
    }

    static bool HasMeaningfulPayload(AbilityDef ability)
    {
        if (ability == null) return false;
        return HasDirectPayload(ability)
            || ability.spawnTurret
            || ability.deployablePrefab != null
            || ability.usePulseDamage
            || ability.chainTargets > 0
            || ability.pullRadius > 0f
            || ability.pullDuration > 0f
            || ability.activeDuration > 0f;
    }

    static bool HasDirectPayload(AbilityDef ability)
    {
        if (ability == null) return false;
        return ability.damage > 0f
            || ability.healAmount > 0f
            || (ability.hotTicks > 0 && ability.hotTickAmount > 0f)
            || ability.shieldAbsorb > 0f
            || ability.statusDuration > 0f;
    }

    void CollectHitsForAbilityShape(AbilityDef shapeAbility, GameObject indicator, Vector3 castOrigin, string targetTag, System.Collections.Generic.List<Collider> hits)
    {
        if (shapeAbility == null || indicator == null || hits == null) return;

        var matched = new System.Collections.Generic.HashSet<Health>();
        if (shapeAbility.shape == AbilityShape.Cone)
        {
            float maxRange = shapeAbility.range * indicator.transform.localScale.x;
            foreach (Collider c in Physics.OverlapSphere(castOrigin, maxRange))
            {
                if (!TryGetMatchingHealth(c, targetTag, out Health health) || matched.Contains(health))
                    continue;

                Vector3 toHit = health.transform.position - castOrigin;
                toHit.y = 0f;
                if (toHit.sqrMagnitude > 0.0001f &&
                    Vector3.Angle(indicator.transform.forward, toHit) > shapeAbility.coneAngle * 0.5f)
                    continue;

                matched.Add(health);
                hits.Add(c);
            }
        }
        else if (shapeAbility.shape == AbilityShape.Rectangle)
        {
            RectangleAimData rectData = indicator.GetComponent<RectangleAimData>();
            if (rectData != null && rectData.valid)
            {
                foreach (Collider c in Physics.OverlapBox(rectData.damageCenter, rectData.damageHalfExtents, rectData.damageRotation))
                    AddMatchingHit(c, targetTag, hits, matched);
            }
            else
            {
                float rectangleLength = Mathf.Abs(indicator.transform.localScale.z);
                Vector3 halfExtents = new Vector3(
                    Mathf.Abs(indicator.transform.localScale.x) * 0.5f,
                    1f,
                    rectangleLength * 0.5f);

                foreach (Collider c in Physics.OverlapBox(indicator.transform.position, halfExtents, indicator.transform.rotation))
                    AddMatchingHit(c, targetTag, hits, matched);
            }
        }
        else
        {
            float radius = Mathf.Max(0f, shapeAbility.indicatorSize * 0.5f);
            foreach (Collider c in Physics.OverlapSphere(indicator.transform.position, radius))
                AddMatchingHit(c, targetTag, hits, matched);
        }
    }

    void ApplyAbilityDefPayloadToHit(AbilityDef ability, Collider hit, GameObject hitVfxPrefab, float damageMultiplier, float aimTime)
    {
        if (ability == null || hit == null) return;

        Health health = hit.GetComponent<Health>() ?? hit.GetComponentInParent<Health>();
        if (health == null) return;

        Vector3 hitPos = health.transform.position + Vector3.up * 0.5f;
        bool playedHitVfx = false;

        if (ability.healAmount > 0f)
        {
            health.Heal(ability.healAmount);
            FloatingDamageText.Spawn(hitPos + Vector3.up, ability.healAmount, FloatingDamageText.DamageType.Heal);
            playedHitVfx = true;
        }

        if (ability.hotTicks > 0 && ability.hotTickAmount > 0f)
        {
            StartCoroutine(ApplyHealOverTime(health, ability.hotTickAmount, ability.hotTicks, ability.hotInterval));
            float totalHot = ability.hotTickAmount * ability.hotTicks;
            FloatingDamageText.Spawn(hitPos + Vector3.up * 1.3f, totalHot, FloatingDamageText.DamageType.Heal);
            playedHitVfx = true;
        }

        if (ability.shieldAbsorb > 0f)
        {
            health.ApplyShield(ability.shieldAbsorb);
            FloatingDamageText.Spawn(hitPos + Vector3.up * 1.3f, ability.shieldAbsorb, FloatingDamageText.DamageType.Shield);
            playedHitVfx = true;
        }

        if (ability.damage > 0f)
        {
            float chargeFraction = GetChargeFraction(ability, aimTime);
            float damage = ability.chargeable
                ? Mathf.Lerp(ability.damage, ability.maxChargeDamage, chargeFraction)
                : ability.damage;
            health.TakeDamage(damage * damageMultiplier, gameObject);
            playedHitVfx = true;
        }

        if (ability.statusDuration > 0f)
        {
            StatusEffectManager sem = hit.GetComponent<StatusEffectManager>() ?? health.GetComponent<StatusEffectManager>();
            sem?.AddEffect(new StatusEffect(ability.statusEffect, ability.statusDuration, ability.statusValue, gameObject));
            playedHitVfx = true;
        }

        if (playedHitVfx)
            EmitHitVFX(hitVfxPrefab, hitPos);
    }

    Vector3 GetCastPointForAbility(AbilityDef ability, GameObject indicator, Vector3 castOrigin)
    {
        Vector3 castPoint = indicator != null ? indicator.transform.position : transform.position;
        if (ability != null && ability.shape == AbilityShape.Cone && indicator != null)
        {
            float coneRange = ability.range * indicator.transform.localScale.x;
            castPoint = castOrigin + indicator.transform.forward * coneRange;
        }
        return castPoint;
    }

    void SpawnCastVFXForAbility(AbilityDef ability, GameObject castVfxPrefab, Vector3 castPoint, Quaternion castRotation, Vector3 castOrigin)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (castVfxPrefab == null) return;

        if (ability != null && ability.shape == AbilityShape.Rectangle)
            StartCoroutine(TravelVFX(castVfxPrefab,
                castOrigin + Vector3.up * 1.2f,
                castPoint + Vector3.up * 0.5f,
                castRotation, 0.3f));
        else
            SpawnVFX(castVfxPrefab, castPoint + Vector3.up * 0.8f, castRotation);
#endif
    }

}

internal class ConeAimData : MonoBehaviour
{
    public bool valid;
    public Vector3 origin;
    public Vector3 visualForward;
    public Vector3 visualNormal;
    public float visualRange;
    public float halfAngle;
    public Vector3[] outlinePoints;

    public void EnsurePoints(int arcSegments)
    {
        int required = Mathf.Max(1, arcSegments) + 2;
        if (outlinePoints == null || outlinePoints.Length != required)
            outlinePoints = new Vector3[required];
    }
}

internal class RectangleAimData : MonoBehaviour
{
    const int CornerCount = 4;

    public bool valid;
    public Vector3[] corners = new Vector3[CornerCount];
    public Vector3 visualCenter;
    public Vector3 visualRight;
    public Vector3 visualForward;
    public Vector3 visualNormal;
    public float visualWidth;
    public float visualLength;
    public Vector3 damageCenter;
    public Quaternion damageRotation;
    public Vector3 damageHalfExtents;

    public void EnsureCorners()
    {
        if (corners == null || corners.Length != CornerCount)
            corners = new Vector3[CornerCount];
    }
}
