using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum AbilityShape { Circle, Cone, Rectangle }
public enum AbilityCategory { Damage, Heal, Support }

[System.Serializable]
public class AbilityDef
{
    public string abilityName = "Ability";
    public AbilityShape shape = AbilityShape.Circle;
    public AbilityCategory category = AbilityCategory.Damage;
    public float range = 4f;
    public float coneAngle = 60f;
    public float rectWidth = 1.5f;
    public float indicatorSize = 1.5f;
    public bool spawnTurret = false;
    public GameObject turretPrefab;
    public ItemData turretItem;
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

    [Header("Timed Effects")]
    public float activeDuration = 0f;      // Phase Cloak, Siege Mode, Iron Tether, Transfer Protocol

    [Header("Chain Lightning")]
    public int   chainTargets       = 0;   // Arc Lance: 4
    public float chainDamageFalloff = 5f;  // damage lost per jump

    [Header("Pull / Zone")]
    public float pullRadius   = 0f;        // Magnetize, Singularity, Event Horizon
    public float pullDuration = 0f;        // Singularity pull phase

    [Header("Deployable Scene Prefab")]
    // The runtime object spawned in the world by this ability (mine, wall, zone, etc.)
    public GameObject deployablePrefab;
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
    static readonly Vector3 GroundDecalPivot = new Vector3(0f, 0f, 0.5f);
    static bool s_warnedInvalidRectDecal;
    static bool s_loggedRectIndicatorPath;

    public Camera cam;
    public Inventory inventory;
    public CastAnimator castAnimator;

    [Header("Cast Time")]
    [Tooltip("Horizontal movement beyond this distance interrupts a committed cast. Movement input interrupts immediately.")]
    [SerializeField, Min(0f)] float castMoveInterruptDistance = 0.035f;

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
        new AbilityDef { abilityName = "Arcane Step",      shape = AbilityShape.Circle,    category = AbilityCategory.Support, range = 10f, indicatorSize = 3.5f, cooldown = 4f, castTime = 0.25f, damage = 10f, targetTag = "Enemy" },
        // [20] Void Maw — pull enemies to center for 3s then 20 AoE burst
        new AbilityDef { abilityName = "Void Maw",         shape = AbilityShape.Circle,    category = AbilityCategory.Damage,  range = 10f, indicatorSize = 8f, cooldown = 9f, damage = 20f, targetTag = "Enemy" },
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
    private float committedCastDuration;
    private float committedCastElapsed;

    // Read by CameraFollow to suspend orbit while an indicator is active
    public static bool    IsAimingLocally { get; private set; }
    // Read by PlayerMovement to face the cursor during aim (Smite-style)
    public static Vector3 AimDirection    { get; private set; }
    private float[] cooldownTimers = new float[4];

    private GameObject activeShieldVFX;
    private float shieldVFXTimer = 0f;

    // ── Cached component refs ──────────────────────────────────────
    private ClassPassive         _passive;
    private PassivePhaseCharge   _phaseCharge;
    private PassiveBountySystem  _bounty;
    private Health               _health;
    private CharacterStats       _characterStats;  // gear/attunement bonuses

    public int HeldAbilityIndex => heldAbilityIndex;
    public bool IsCommittedCasting => committedCastRoutine != null && committedCastAbility != null && committedCastDuration > 0f;
    public string CommittedCastName => committedCastAbility != null ? committedCastAbility.abilityName : "";
    public AbilityCategory CommittedCastCategory => committedCastAbility != null ? committedCastAbility.category : AbilityCategory.Damage;
    public float CommittedCastProgress => committedCastDuration > 0f ? Mathf.Clamp01(committedCastElapsed / committedCastDuration) : 0f;
    public float CommittedCastRemaining => Mathf.Max(0f, committedCastDuration - committedCastElapsed);

    void Awake()
    {
        // Remote-player gating is handled by ShouldProcessLocalInput() in Update.
        // isLocalPlayer is NOT set in Awake (Mirror sets it after instantiation),
        // so any enabled-check here would wrongly disable the local player too.

        // Seed the equipped loadout from the class pool so the action bar matches the
        // selected class. Without this, equippedIndices stays at the hardcoded shared
        // pool {0,1,2,3} for every class (bar wouldn't match Character Select).
        if (classPool != null && classPool.defaultEquipped != null)
            for (int i = 0; i < equippedIndices.Length && i < 4; i++)
                equippedIndices[i] = (i < classPool.defaultEquipped.Length)
                    ? classPool.defaultEquipped[i] : -1;

        SyncEquippedFromSpellbook();

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

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
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
                  ?? FindFirstObjectByType<CameraFollow>()
                  ?? sceneCam.gameObject.AddComponent<CameraFollow>();
        follow.target = transform;
    }

    public void SyncEquippedFromSpellbook()
    {
        _equippedAbilities = new AbilityDef[4];
        for (int i = 0; i < 4; i++)
        {
            int idx = (i < equippedIndices.Length) ? equippedIndices[i] : -1;
            _equippedAbilities[i] = (idx >= 0 && idx < spellbook.Length) ? spellbook[idx] : null;
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
        if (classPool == null) return true;
        foreach (int idx in classPool.availableIndices)
            if (idx == spellbookIndex) return true;
        return false;
    }

    // Apply a class pool and reset to its default loadout.
    public void ApplyClass(ClassAbilityPool pool)
    {
        classPool = pool;
        if (pool == null) return;

        for (int i = 0; i < 4; i++)
            equippedIndices[i] = (i < pool.defaultEquipped.Length) ? pool.defaultEquipped[i] : -1;

        SyncEquippedFromSpellbook();
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

            bool hasTurretAvailable =
                !abilities[i].spawnTurret ||
                abilities[i].turretItem == null ||
                inventory == null ||
                inventory.HasItem(abilities[i].turretItem);

            if (key.wasPressedThisFrame && cooldownTimers[i] <= 0f && hasTurretAvailable)
            {
                // Self-cast shields skip aiming but still respect cast time.
                if (abilities[i].shieldAbsorb > 0f && abilities[i].range <= 0f)
                {
                    if (heldAbilityIndex != -1) CancelAim();
                    BeginCommittedCast(i, abilities[i], null, 0f);
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
                BeginCommittedCast(heldAbilityIndex, abilities[heldAbilityIndex], activeIndicator, aimTimer);

                IsAimingLocally = false;
                heldAbilityIndex = -1;
                activeIndicator = null;
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
        // Cursor stays free — CameraFollow resumes ownership
    }

    void BeginCommittedCast(int slot, AbilityDef ability, GameObject indicator, float aimTime)
    {
        if (ability == null)
        {
            if (indicator != null) Destroy(indicator);
            return;
        }

        if (committedCastRoutine != null)
            CancelCommittedCast();

        PlayCommittedCastAnimation(ability);
        BroadcastCommittedCastAnimation(ability);

        float castTime = CastTimeFor(ability);
        Debug.Log($"[CastTime] {ability.abilityName} committed with castTime={castTime:0.###}s.", this);
        if (castTime <= 0f)
        {
            if (FinalizeCast(ability, indicator, aimTime))
                StartCooldown(slot, ability);
            else if (indicator != null)
                Destroy(indicator);
            return;
        }

        committedCastSlot = slot;
        committedCastIndicator = indicator;
        committedCastAbility = ability;
        committedCastDuration = castTime;
        committedCastElapsed = 0f;
        committedCastRoutine = StartCoroutine(CommittedCastRoutine(slot, ability, indicator, aimTime, castTime));
    }

    System.Collections.IEnumerator CommittedCastRoutine(int slot, AbilityDef ability, GameObject indicator, float aimTime, float castTime)
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
        bool castStarted = FinalizeCast(ability, indicator, aimTime);
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

        if (HasMovementInput())
            return true;

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
            var fan  = CreateConeIndicator(ability.range, ability.coneAngle);
            fan.transform.SetParent(indicator.transform, false);
            var rend = fan.GetComponent<Renderer>();
            var mat  = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(c.r, c.g, c.b, 0.30f);
            rend.material = mat;
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
            BuildOutlineLR(indicator, ability, c);
            if (!BuildProjectedRectDecal(indicator, c))
                BuildProjectedRectFill(indicator, c);
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
            float widthMul = Mathf.Lerp(1f, ability.maxChargeSizeMultiplier, chargeFraction);
            float hw = ability.rectWidth * widthMul / 2f;
            Vector3 mid = ProjectToGround(transform.position + aimDir * (aimDistance / 2f), out Vector3 groundNormal);
            Vector3 groundForward = Vector3.ProjectOnPlane(aimDir, groundNormal);
            if (groundForward.sqrMagnitude < 0.0001f)
                groundForward = aimDir;
            groundForward.Normalize();

            indicator.transform.position   = mid;
            indicator.transform.rotation   = Quaternion.LookRotation(groundForward, groundNormal);
            // Keep localScale so ApplyRectangleDamage still works
            indicator.transform.localScale = new Vector3(ability.rectWidth * widthMul, 1f, aimDistance);

            if (lr != null) SetRectPoints(lr, mid, groundForward, groundNormal, hw, aimDistance / 2f);
            UpdateProjectedRectDecal(indicator, mid, groundForward, groundNormal, ability.rectWidth * widthMul, aimDistance);
            UpdateProjectedRectFill(indicator);
        }
        else if (ability.shape == AbilityShape.Cone)
        {
            float chargeMul   = Mathf.Lerp(1f, ability.maxChargeSizeMultiplier, chargeFraction);
            float distanceMul = aimDistance / ability.range;
            // Pull origin 0.5 units behind the player so the character body sits
            // inside the fan rather than at the very tip.
            indicator.transform.position   = ProjectToGround(transform.position - aimDir * 0.5f);
            indicator.transform.rotation   = Quaternion.LookRotation(aimDir);
            indicator.transform.localScale = Vector3.one * distanceMul * chargeMul;
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

    void SetRectPoints(LineRenderer lr, Vector3 centre, Vector3 fwd, Vector3 up, float hw, float hl)
    {
        if (lr.positionCount != 4) { lr.positionCount = 4; lr.loop = true; }
        Vector3 right = Vector3.Cross(up, fwd).normalized;
        lr.SetPosition(0, ProjectToGround(centre - right * hw - fwd * hl));
        lr.SetPosition(1, ProjectToGround(centre + right * hw - fwd * hl));
        lr.SetPosition(2, ProjectToGround(centre + right * hw + fwd * hl));
        lr.SetPosition(3, ProjectToGround(centre - right * hw + fwd * hl));
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

    void UpdateProjectedRectDecal(GameObject indicator, Vector3 centre, Vector3 forward, Vector3 normal, float width, float length)
    {
        DecalProjector projector = indicator.GetComponentInChildren<DecalProjector>();
        if (projector == null)
            return;

        Transform decalTransform = projector.transform;
        decalTransform.position = centre;
        decalTransform.rotation = Quaternion.LookRotation(-normal, forward);
        projector.pivot = GroundDecalPivot;
        projector.size = new Vector3(width, length, indicatorDecalProjectionDepth);
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

    void UpdateProjectedRectFill(GameObject indicator)
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

        if (mf == null || mf.sharedMesh == null) return;

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uv = mesh.uv;
        if (uv == null || uv.Length != vertices.Length) return;

        Transform indicatorTransform = indicator.transform;
        Vector3 centre = indicatorTransform.position;
        Vector3 right = indicatorTransform.right;
        Vector3 forward = indicatorTransform.forward;
        Vector3 scale = indicatorTransform.localScale;
        Vector3 fillOffset = mf.transform.localPosition;

        for (int i = 0; i < vertices.Length; i++)
        {
            float localX = (uv[i].x - 0.5f) * scale.x + fillOffset.x;
            float localZ = (uv[i].y - 0.5f) * scale.z + fillOffset.z;
            Vector3 world = centre + right * localX + forward * localZ;
            vertices[i] = mf.transform.InverseTransformPoint(ProjectToGround(world));
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
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
        if (prefab == null) return;
        GameObject fx = Instantiate(prefab, position, rotation);
        Destroy(fx, 4f);
    }

#if UNITY_EDITOR || !UNITY_SERVER
    System.Collections.IEnumerator TravelVFX(GameObject prefab, Vector3 from, Vector3 to,
                                             Quaternion rotation, float duration)
    {
        if (prefab == null) yield break;
        GameObject fx = Instantiate(prefab, from, rotation);
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

    bool FinalizeCast(AbilityDef ability, GameObject indicator, float aimTime)
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

            CmdFinalizeCast(spellbookIndex, castPosition, castRotation, castScale, aimTime);
            PlayLocalCastVFX(ability, castPosition, castRotation);

            if (indicator != null)
                Destroy(indicator);
            return true;
        }

        Debug.Log("Cast ability: " + ability.abilityName);

        // Notify passive (Phase Charge meter, etc.)
        _passive?.OnAbilityCast(ability);

        // Phase Charge: scale next damage ability
        float damageMultiplier = _phaseCharge != null
            ? _phaseCharge.ConsumeBonusIfCharged(ability)
            : 1f;

        // Gear + attunement damage bonus (CharacterStats) — applies to every
        // shape and every dispatched ability since they all read this value.
        if (_characterStats != null)
            damageMultiplier *= _characterStats.DamageMultiplier;

        ResolveCastEffects(ability, indicator, aimTime, damageMultiplier, transform.position);
        if (indicator != null)
            Destroy(indicator);
        return true;
    }

    void ResolveCastEffects(AbilityDef ability, GameObject indicator, float aimTime, float damageMultiplier, Vector3 castOrigin)
    {
        if (ability == null) return;

#if UNITY_EDITOR || !UNITY_SERVER
        if (ability.category == AbilityCategory.Heal) OnHealCast?.Invoke();
#endif

        if (ability.shape == AbilityShape.Rectangle && ability.damage > 0f && indicator != null)
        {
            float chargeFraction = GetChargeFraction(ability, aimTime);
            float damage = Mathf.Lerp(ability.damage, ability.maxChargeDamage, chargeFraction) * damageMultiplier;
            ApplyRectangleDamage(ability, indicator, damage);
        }

        if (ability.shape == AbilityShape.Cone && ability.damage > 0f && indicator != null)
        {
            float chargeFraction = GetChargeFraction(ability, aimTime);
            float damage = Mathf.Lerp(ability.damage, ability.maxChargeDamage, chargeFraction) * damageMultiplier;
            float coneRange = ability.range * indicator.transform.localScale.x;
            ApplyConeDamage(ability, indicator, damage, coneRange, castOrigin);

            if (ability.fireVisual)
                SpawnFireBurst(castOrigin + indicator.transform.forward * coneRange + Vector3.up * 0.5f, indicator.transform.rotation, coneRange, ability.coneAngle);
        }

        if (ability.shape == AbilityShape.Circle && ability.damage > 0f && !IsArcaneStep(ability))
        {
            ApplyCircleDamage(ability, indicator, damageMultiplier);
        }

        if (ability.shieldAbsorb > 0f)
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
        if (ability.castVFX != null)
        {
            if (ability.shape == AbilityShape.Rectangle && indicator != null)
                StartCoroutine(TravelVFX(ability.castVFX,
                    castOrigin + Vector3.up * 1.2f,
                    castPoint + Vector3.up * 0.5f,
                    castVfxRot, 0.3f));
            else
                SpawnVFX(ability.castVFX, castPoint + Vector3.up * 0.8f, castVfxRot);
        }
#endif
        DispatchAbility(ability, castPoint, damageMultiplier);
    }

    [Command]
    void CmdFinalizeCast(int spellbookIndex, Vector3 castPosition, Quaternion castRotation, Vector3 castScale, float aimTime)
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
        if (!FinalizeCast(ability, serverIndicator, aimTime))
        {
            if (serverIndicator != null) Destroy(serverIndicator);
            return;
        }

        cooldownTimers[equippedSlot] = CooldownFor(ability);

        RpcCastConfirmed(spellbookIndex, castPosition, castRotation);
    }

    [ClientRpc]
    void RpcCastConfirmed(int spellbookIndex, Vector3 position, Quaternion rotation)
    {
        if (isLocalPlayer) return;
        if (spellbookIndex < 0 || spellbookIndex >= spellbook.Length) return;
        PlayLocalCastVFX(spellbook[spellbookIndex], position, rotation);
    }

    GameObject CreateServerCastProxy(AbilityDef ability, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (ability == null || ability.range <= 0f) return null;

        GameObject proxy = new GameObject($"ServerCast_{ability.abilityName}");
        proxy.transform.position = position;
        proxy.transform.rotation = rotation;
        proxy.transform.localScale = scale;
        return proxy;
    }

    void PlayLocalCastVFX(AbilityDef ability, Vector3 position, Quaternion rotation)
    {
        if (ability == null) return;
        SpawnLocalCastVFX(ability, position, rotation, transform.position);
    }

    void SpawnLocalCastVFX(AbilityDef ability, Vector3 position, Quaternion rotation, Vector3 castOrigin)
    {
        if (ability == null) return;

#if UNITY_EDITOR || !UNITY_SERVER
        if (ability.category == AbilityCategory.Heal) OnHealCast?.Invoke();
#endif

        if (ability.castVFX == null) return;

#if UNITY_EDITOR || !UNITY_SERVER
        if (ability.shape == AbilityShape.Rectangle)
            StartCoroutine(TravelVFX(ability.castVFX,
                castOrigin + Vector3.up * 1.2f,
                position + Vector3.up * 0.5f,
                rotation, 0.3f));
        else
#endif
            SpawnVFX(ability.castVFX, position + Vector3.up, rotation);
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
                StartCoroutine(ArcaneStepPulseDamage(ability, castPoint, dmgMult));
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

    System.Collections.IEnumerator ArcaneStepPulseDamage(AbilityDef ability, Vector3 centre, float damageMultiplier)
    {
        float radius = ability.indicatorSize > 0f ? ability.indicatorSize * 0.5f : 1.75f;
        float damage = (ability.damage > 0f ? ability.damage : 10f) * damageMultiplier;

        for (int pulse = 0; pulse < ArcaneStepPulseCount; pulse++)
        {
            ApplyPulseDamage(centre, radius, damage, ability.targetTag, ability.hitVFX);
            yield return new WaitForSeconds(ArcaneStepPulseInterval);
        }
    }

    void ApplyPulseDamage(Vector3 centre, float radius, float damage, string targetTag, GameObject hitVFX)
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
            SpawnVFX(hitVFX, health.transform.position + Vector3.up * 0.5f, Quaternion.identity);
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
            if (ability.hitVFX != null)
                SpawnVFX(ability.hitVFX, col.transform.position + Vector3.up, Quaternion.identity);
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
            s.burstDamage     *= dmgMult;
            s.pulseDamage      = isEventHorizon ? 0f : s.pulseDamage * dmgMult;
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
            Health h = nearest.GetComponent<Health>();
            h?.TakeDamage(Mathf.Max(1f, dmg), gameObject);

            if (ability.hitVFX != null)
                SpawnVFX(ability.hitVFX, nearest.transform.position + Vector3.up * 0.5f, Quaternion.identity);

            // Draw lightning between jumps (quick LineRenderer)
            Vector3 from = last != null ? last.position + Vector3.up * 0.8f
                                        : startPoint    + Vector3.up * 0.8f;
            DrawLightningLine(from, nearest.transform.position + Vector3.up * 0.8f, 0.15f);

            last  = nearest.transform;
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
            if (!col.CompareTag(tag)) continue;
            if (exclude != null && col.transform == exclude) continue;
            float d = Vector3.Distance(center, col.transform.position);
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
                if (ability.hitVFX != null)
                    SpawnVFX(ability.hitVFX, col.transform.position + Vector3.up, Quaternion.identity);
                return;
            }
        }

        // Priority 2: deal burst damage to robotic enemies in range
        Collider[] enemies = Physics.OverlapSphere(castPoint, 2f);
        foreach (var col in enemies)
        {
            if (!col.CompareTag("Enemy")) continue;
            Health h = col.GetComponent<Health>();
            if (h == null || !h.isRobotic) continue;
            float dmg = (ability.damage > 0f ? ability.damage : 60f) * dmgMult;
            h.TakeDamage(dmg, gameObject);
            if (ability.hitVFX != null)
                SpawnVFX(ability.hitVFX, col.transform.position + Vector3.up, Quaternion.identity);
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
        foreach (var col in hits)
        {
            if (!col.CompareTag(ability.targetTag)) continue;
            var sem = col.GetComponent<StatusEffectManager>();
            if (sem == null) continue;
            int stacks = sem.ConsumeDebuffStacks();
            if (stacks > 0)
            {
                float dmg = baseDmg * stacks * dmgMult;
                col.GetComponent<Health>()?.TakeDamage(dmg, gameObject);
                if (ability.hitVFX != null)
                    SpawnVFX(ability.hitVFX, col.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
    }

    // Generic helper: instantiate a deployable prefab, run optional init, register it.
    void SpawnDeployableAt(GameObject prefab, Vector3 pos, System.Action<GameObject> init,
                            Quaternion? rot = null)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, pos, rot ?? Quaternion.identity);
        init?.Invoke(go);
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

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(ability.targetTag)) continue;

            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(ability.damage * damageMultiplier);
                SpawnVFX(ability.hitVFX, hit.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
    }

    void ApplyRectangleDamage(AbilityDef ability, GameObject indicator, float damage)
    {
        float rectangleLength = indicator.transform.localScale.z;
        Vector3 halfExtents = new Vector3(
            indicator.transform.localScale.x / 2f,
            1f,
            rectangleLength / 2f
        );

        Collider[] hits = Physics.OverlapBox(
            indicator.transform.position,
            halfExtents,
            indicator.transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(ability.targetTag)) continue;

            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                SpawnVFX(ability.hitVFX, hit.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
    }

    void ApplyConeDamage(AbilityDef ability, GameObject indicator, float damage, float coneRange, Vector3 castOrigin)
    {
        Collider[] hits = Physics.OverlapSphere(castOrigin, coneRange);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(ability.targetTag)) continue;

            Vector3 toHit = hit.transform.position - castOrigin;
            toHit.y = 0;

            if (toHit.sqrMagnitude < 0.0001f) continue;

            float angle = Vector3.Angle(indicator.transform.forward, toHit);
            if (angle > ability.coneAngle / 2f) continue;

            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                SpawnVFX(ability.hitVFX, hit.transform.position + Vector3.up * 0.5f, Quaternion.identity);
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

            if (turret.GetComponent<TurretController>() == null)
                turret.AddComponent<TurretController>();

            if (ability.turretItem != null && inventory != null)
            {
                inventory.RemoveItem(ability.turretItem);

                TurretPickup pickup = turret.GetComponent<TurretPickup>();
                if (pickup != null)
                {
                    pickup.item = ability.turretItem;
                    pickup.inventory = inventory;
                }
            }
        }
        else
        {
            GameObject turret = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            turret.name = "Turret (Placeholder)";
            turret.transform.position = position;
            turret.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            turret.AddComponent<TurretController>();
        }
    }

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

    GameObject CreateConeIndicator(float range, float angle)
    {
        GameObject go = new GameObject("ConeIndicator");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        int segments = 20;
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        float startAngle = -angle / 2f;
        float step = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float a = (startAngle + step * i) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * range;
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        return go;
    }
}
