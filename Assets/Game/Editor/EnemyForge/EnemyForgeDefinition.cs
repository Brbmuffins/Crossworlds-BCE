#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace Crossworlds.EditorTools.EnemyForge
{
    internal enum EnemyForgeArchetype { Melee, Ranged, Elite }
    internal enum EnemyForgeRootTag { Enemy, Player }
    internal enum EnemyForgeCastAttack
    {
        RandomAll,
        GroundSlam,
        VoidBurst,
        ChainLightning,
        GroundSpikes,
        HexBlast,
        ElementalLightning
    }
    internal enum EnemyForgeAnimationDriverMode
    {
        EnemyForgeStandard,
        ExistingModelDriver,
        HybridOverride
    }

    internal sealed class EnemyForgeDefinition : ScriptableObject
    {
        public const int CurrentVersion = 1;

        [HideInInspector] public int version = CurrentVersion;
        public GameObject source;
        public EnemyForgeArchetype archetype = EnemyForgeArchetype.Melee;
        [Tooltip("Tag assigned to the root of the saved prefab. Enemy is recommended for hostile mobs.")]
        public EnemyForgeRootTag rootTag = EnemyForgeRootTag.Enemy;
        public string templateId = "enemy_basic";
        public string outputFolder = "Assets/Game/Prefabs/EnemyForge";
        [Tooltip("Apply the generated combat settings to matching prefab instances in currently loaded scenes.")]
        public bool updateMatchingWorldInstances;

        [Header("Animation Mapping")]
        [Tooltip("Standard is recommended. It disables competing writers only on the generated prefab and assigns a prefab-specific override controller.")]
        public EnemyForgeAnimationDriverMode animationDriverMode = EnemyForgeAnimationDriverMode.EnemyForgeStandard;
        [HideInInspector]
        public DefaultAsset animationFolder;
        [HideInInspector]
        public AnimationClip idleAnimation;
        [HideInInspector]
        public AnimationClip chaseAnimation;
        [HideInInspector]
        public AnimationClip attackAnimation;
        [HideInInspector]
        public AnimationClip attackAnimation2;
        [HideInInspector]
        public AnimationClip attackAnimation3;
        [HideInInspector]
        public AnimationClip attackAnimation4;
        [HideInInspector]
        public AnimationClip getHitAnimation;
        [HideInInspector]
        public AnimationClip deathAnimation;
        [HideInInspector, Range(0.25f, 3f)] public float idleAnimationSpeed = 1f;
        [HideInInspector, Range(0.25f, 3f)] public float chaseAnimationSpeed = 1f;
        [HideInInspector, Range(0.25f, 3f)] public float attackAnimationSpeed = 1f;
        [HideInInspector, Range(0.25f, 3f)] public float attackAnimationSpeed2 = 1f;
        [HideInInspector, Range(0.25f, 3f)] public float attackAnimationSpeed3 = 1f;
        [HideInInspector, Range(0.25f, 3f)] public float attackAnimationSpeed4 = 1f;
        [HideInInspector, Range(0.25f, 3f)] public float getHitAnimationSpeed = 1f;
        [HideInInspector, Range(0.25f, 3f)] public float deathAnimationSpeed = 1f;
        [HideInInspector]
        public bool generateAnimatorController = true;

        [Header("SFX")]
        public AudioClip aggroSfx;
        public AudioClip attack1Sfx;
        public AudioClip attack2Sfx;
        public AudioClip attack3Sfx;
        public AudioClip attack4Sfx;
        public AudioClip attackImpactSfx;
        public AudioClip getHitSfx;
        public AudioClip deathSfx;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 0.35f)] public float sfxPitchVariation = 0.05f;
        [Min(0f)] public float sfxMinDistance = 2f;
        [Min(0.1f)] public float sfxMaxDistance = 25f;

        [Header("Vitals")]
        [Tooltip("Name shown in the enemy hover display. Leave blank to format the prefab or scene object name.")]
        public string enemyDisplayName = "";
        [Min(0), Tooltip("Level shown in the enemy hover display. Use 0 to hide the level.")]
        public int enemyLevel = 0;
        [Min(1f)] public float maxHealth = 60f;
        public bool robotic;

        [Header("Movement and perception")]
        [Min(0f)] public float moveSpeed = 4.5f;
        [Min(0f)] public float acceleration = 8f;
        [Min(0f)] public float angularSpeed = 250f;
        [Min(0.05f)] public float agentRadius = 0.4f;
        [Min(0.1f)] public float agentHeight = 2f;
        public float agentBaseOffset = 0f;
        [Tooltip("Minimum horizontal combat spacing from the player. Increase this for wider enemies to prevent overlap or stacking.")]
        [Min(0f)] public float stoppingDistance = 1.2f;
        [Min(0f)] public float aggroRadius = 8f;
        [Min(0f)] public float leashRadius = 20f;
        public bool enableRoaming = true;
        [Min(0f)] public float roamingRadius = 8f;
        [Min(0f)] public float roamingMinWait = 2f;
        [Min(0f)] public float roamingMaxWait = 5f;
        public bool aggroWhenDamaged = true;

        [Header("Basic attack")]
        [Min(0.05f)] public float attackRange = 1.5f;
        [Min(0.05f)] public float attackInterval = 1.5f;
        [Min(0f)] public float damage = 12f;
        [Min(0f)] public float combatTurnSpeed = 1080f;
        [Range(0f, 1f)] public float attackImpactPoint = 0.45f;
        [HideInInspector, Range(0f, 1f)] public float attackImpactPoint2 = 0.45f;
        [HideInInspector, Range(0f, 1f)] public float attackImpactPoint3 = 0.45f;
        [HideInInspector, Range(0f, 1f)] public float attackImpactPoint4 = 0.45f;
        [HideInInspector] public Vector3 attackVfxOffset = Vector3.zero;
        [HideInInspector] public Vector3 attackVfxOffset2 = Vector3.zero;
        [HideInInspector] public Vector3 attackVfxOffset3 = Vector3.zero;
        [HideInInspector] public Vector3 attackVfxOffset4 = Vector3.zero;
        public GameObject projectilePrefab;
        [Min(0f)] public float preferredRange = 5f;
        [Min(0f)] public float tooCloseDistance = 3f;

        [Header("Cast Attack")]
        [InspectorName("Enable Cast Attack")]
        public bool addHeavyAttack = true;
        [InspectorName("Cast Attack Spell")]
        [Tooltip("Choose one spell for this enemy, or Random All to rotate among every available enemy spell.")]
        public EnemyForgeCastAttack castAttack = EnemyForgeCastAttack.RandomAll;
        public ElementalLightningVFXProfile elementalLightningVfxProfile;
        [Min(0f)] public float heavyMinCooldown = 12f;
        [Min(0f)] public float heavyMaxCooldown = 20f;
        [Min(0f)] public float heavyDamageMultiplier = 2f;
        [Tooltip("Maximum target distance at which a Ranged enemy may begin its selected cast attack.")]
        [Min(0.1f)] public float rangedCastDistance = 15f;
        [Tooltip("Begin the configured opening cast as soon as a Ranged enemy acquires a target.")]
        public bool castImmediatelyOnAggro = true;
        [Tooltip("Spell used when combat begins, or Random All to select from the configured spell pool.")]
        public EnemyForgeCastAttack openingCast = EnemyForgeCastAttack.RandomAll;
        [Tooltip("Additional delay before the opening cast animation begins.")]
        [Min(0f)] public float openingCastDelay = 0f;
        [Tooltip("Prevents repeated damage or target notifications from restarting the opening cast during the same combat engagement.")]
        public bool openingCastOncePerAggro = true;
        [Tooltip("Requires an unobstructed path to the target before beginning the opening cast.")]
        public bool openingCastRequiresLineOfSight = true;
        [Tooltip("Cancels the opening cast if its target dies or leaves the enemy's leash before impact.")]
        public bool cancelOpeningCastIfTargetInvalid = true;

        [Header("Rewards")]
        public DropTable dropTable;
        public GameObject worldItemPrefab;

        [Header("Lifecycle")]
        [Min(0f)] public float deadModelVisibleSeconds = 3f;
        public bool respawnAfterDeath;
        [Min(0f)] public float respawnDelay = 30f;
        [HideInInspector] public bool keepCorpseGrounded = true;
        [Range(-0.5f, 0.5f)] public float corpseGroundOffset = -0.05f;

        public bool IsRanged => archetype == EnemyForgeArchetype.Ranged;

        public void ApplyArchetypeDefaults()
        {
            agentBaseOffset = 0f;
            switch (archetype)
            {
                case EnemyForgeArchetype.Ranged:
                    maxHealth = 40f; moveSpeed = 3.5f; stoppingDistance = 4f;
                    aggroRadius = 15f; leashRadius = 30f;
                    attackRange = 15f; attackInterval = 2f; damage = 10f;
                    preferredRange = 10f; tooCloseDistance = 4f;
                    rangedCastDistance = 15f;
                    castImmediatelyOnAggro = true;
                    openingCast = EnemyForgeCastAttack.RandomAll;
                    openingCastDelay = 0f;
                    openingCastOncePerAggro = true;
                    openingCastRequiresLineOfSight = true;
                    cancelOpeningCastIfTargetInvalid = true;
                    heavyMinCooldown = 0f; heavyMaxCooldown = 0f; heavyDamageMultiplier = 2.2f;
                    break;
                case EnemyForgeArchetype.Elite:
                    maxHealth = 300f; moveSpeed = 3.8f; stoppingDistance = 1.8f;
                    attackRange = 2f; attackInterval = 2f; damage = 28f;
                    heavyMinCooldown = 8f; heavyMaxCooldown = 14f; heavyDamageMultiplier = 3f;
                    break;
                default:
                    maxHealth = 60f; moveSpeed = 4.5f; stoppingDistance = 1.2f;
                    attackRange = 1.5f; attackInterval = 1.5f; damage = 12f;
                    heavyMinCooldown = 12f; heavyMaxCooldown = 20f; heavyDamageMultiplier = 2f;
                    break;
            }
        }

        public string ImportFromSource(GameObject selected)
        {
            if (selected == null) return "No source was selected.";
            rootTag = selected.CompareTag("Player") ? EnemyForgeRootTag.Player : EnemyForgeRootTag.Enemy;
            var health = selected.GetComponent<Health>();
            var agent = selected.GetComponent<NavMeshAgent>();
            var controller = selected.GetComponent<EnemyController>();
            var heavy = selected.GetComponent<EnemyHeavyAttack>();
            var sfx = selected.GetComponent<EnemySfxProfile>();

            if (controller != null && controller.isRanged)
                archetype = EnemyForgeArchetype.Ranged;
            else if ((health != null && health.maxHealth >= 200f) ||
                     (controller != null && controller.damage >= 25f))
                archetype = EnemyForgeArchetype.Elite;
            else
                archetype = EnemyForgeArchetype.Melee;

            // Establish complete, safe values first, then preserve every value the
            // selected combat-ready prefab already defines.
            ApplyArchetypeDefaults();
            int importedComponents = 0;
            if (health != null)
            {
                maxHealth = health.maxHealth;
                robotic = health.isRobotic;
                enemyDisplayName = health.ConfiguredEnemyDisplayName;
                enemyLevel = health.ConfiguredEnemyLevel;
                importedComponents++;
            }
            if (agent != null)
            {
                moveSpeed = agent.speed;
                acceleration = agent.acceleration;
                angularSpeed = agent.angularSpeed;
                agentRadius = agent.radius;
                agentHeight = agent.height;
                agentBaseOffset = agent.baseOffset;
                stoppingDistance = agent.stoppingDistance;
                importedComponents++;
            }
            if (controller != null)
            {
                if (!string.IsNullOrWhiteSpace(controller.enemyTemplateId)) templateId = controller.enemyTemplateId;
                aggroRadius = controller.aggroRadius;
                leashRadius = controller.leashRadius;
                enableRoaming = controller.enableRoaming;
                roamingRadius = controller.roamingRadius;
                roamingMinWait = controller.roamingMinWait;
                roamingMaxWait = controller.roamingMaxWait;
                aggroWhenDamaged = controller.aggroWhenDamaged;
                attackRange = controller.attackRange;
                attackInterval = controller.attackInterval;
                damage = controller.damage;
                combatTurnSpeed = controller.combatTurnSpeed;
                idleAnimationSpeed = controller.idleAnimationSpeed;
                chaseAnimationSpeed = controller.chaseAnimationSpeed;
                attackAnimationSpeed = controller.attackAnimationSpeeds != null && controller.attackAnimationSpeeds.Length > 0 ? controller.attackAnimationSpeeds[0] : 1f;
                attackAnimationSpeed2 = controller.attackAnimationSpeeds != null && controller.attackAnimationSpeeds.Length > 1 ? controller.attackAnimationSpeeds[1] : 1f;
                attackAnimationSpeed3 = controller.attackAnimationSpeeds != null && controller.attackAnimationSpeeds.Length > 2 ? controller.attackAnimationSpeeds[2] : 1f;
                attackAnimationSpeed4 = controller.attackAnimationSpeeds != null && controller.attackAnimationSpeeds.Length > 3 ? controller.attackAnimationSpeeds[3] : 1f;
                getHitAnimationSpeed = controller.getHitAnimationSpeed;
                deathAnimationSpeed = controller.deathAnimationSpeed;
                attackVfxOffset = controller.attackVfxOffsets != null && controller.attackVfxOffsets.Length > 0 ? controller.attackVfxOffsets[0] : Vector3.zero;
                attackVfxOffset2 = controller.attackVfxOffsets != null && controller.attackVfxOffsets.Length > 1 ? controller.attackVfxOffsets[1] : Vector3.zero;
                attackVfxOffset3 = controller.attackVfxOffsets != null && controller.attackVfxOffsets.Length > 2 ? controller.attackVfxOffsets[2] : Vector3.zero;
                attackVfxOffset4 = controller.attackVfxOffsets != null && controller.attackVfxOffsets.Length > 3 ? controller.attackVfxOffsets[3] : Vector3.zero;
                if (attackAnimation != null && attackAnimation.length > 0.01f)
                    attackImpactPoint = Mathf.Clamp01(
                        controller.attackImpactDelay * Mathf.Max(0.25f, attackAnimationSpeed) /
                        attackAnimation.length);
                if (controller.attackAnimationImpactDelays != null)
                {
                    if (attackAnimation2 != null && attackAnimation2.length > 0.01f &&
                        controller.attackAnimationImpactDelays.Length > 1)
                        attackImpactPoint2 = Mathf.Clamp01(
                            controller.attackAnimationImpactDelays[1] * Mathf.Max(0.25f, attackAnimationSpeed2) /
                            attackAnimation2.length);
                    if (attackAnimation3 != null && attackAnimation3.length > 0.01f &&
                        controller.attackAnimationImpactDelays.Length > 2)
                        attackImpactPoint3 = Mathf.Clamp01(
                            controller.attackAnimationImpactDelays[2] * Mathf.Max(0.25f, attackAnimationSpeed3) /
                            attackAnimation3.length);
                    if (attackAnimation4 != null && attackAnimation4.length > 0.01f &&
                        controller.attackAnimationImpactDelays.Length > 3)
                        attackImpactPoint4 = Mathf.Clamp01(
                            controller.attackAnimationImpactDelays[3] * Mathf.Max(0.25f, attackAnimationSpeed4) /
                            attackAnimation4.length);
                }
                projectilePrefab = controller.projectilePrefab;
                preferredRange = controller.preferredRange;
                tooCloseDistance = controller.tooCloseDistance;
                dropTable = controller.dropTable;
                worldItemPrefab = controller.worldItemPrefab;
                deadModelVisibleSeconds = controller.deadModelVisibleSeconds;
                respawnAfterDeath = controller.respawnAfterDeath;
                respawnDelay = controller.respawnDelay;
                keepCorpseGrounded = controller.keepCorpseGrounded;
                corpseGroundOffset = controller.corpseGroundOffset;
                importedComponents++;
            }
            addHeavyAttack = heavy != null;
            if (heavy != null)
            {
                heavyMinCooldown = heavy.minCooldown;
                heavyMaxCooldown = heavy.maxCooldown;
                heavyDamageMultiplier = heavy.damageMultiplier;
                rangedCastDistance = heavy.castDistanceToTarget;
                castImmediatelyOnAggro = heavy.castImmediatelyOnAggro;
                openingCast = heavy.openingCastRandom
                    ? EnemyForgeCastAttack.RandomAll
                    : (EnemyForgeCastAttack)((int)heavy.openingCastType + 1);
                openingCastDelay = heavy.openingCastDelay;
                openingCastOncePerAggro = heavy.openingCastOncePerAggro;
                openingCastRequiresLineOfSight = heavy.openingCastRequiresLineOfSight;
                cancelOpeningCastIfTargetInvalid = heavy.cancelOpeningCastIfTargetInvalid;
                elementalLightningVfxProfile = heavy.elementalLightningVfxProfile;
                castAttack = heavy.availableTypes != null && heavy.availableTypes.Length > 0
                    ? (EnemyForgeCastAttack)((int)heavy.availableTypes[0] + 1)
                    : EnemyForgeCastAttack.RandomAll;
                importedComponents++;
            }
            if (sfx != null)
            {
                aggroSfx = sfx.aggro;
                attack1Sfx = sfx.attack1;
                attack2Sfx = sfx.attack2;
                attack3Sfx = sfx.attack3;
                attack4Sfx = sfx.attack4;
                attackImpactSfx = sfx.attackImpact;
                getHitSfx = sfx.getHit;
                deathSfx = sfx.death;
                sfxVolume = sfx.volume;
                sfxPitchVariation = sfx.pitchVariation;
                sfxMinDistance = sfx.minDistance;
                sfxMaxDistance = sfx.maxDistance;
                importedComponents++;
            }

            return importedComponents > 0
                ? $"Imported existing settings from {importedComponents} combat component(s); missing values use {archetype} defaults."
                : $"No existing combat components were found; applied {archetype} defaults.";
        }
    }
}
#endif
