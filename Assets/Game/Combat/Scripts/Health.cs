using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Mirror;

// Extended Health — replaces the original.
// Backward compatible: TakeDamage / Heal / ApplyShield / currentHealth / maxHealth
// all work identically. New features are additive.
public class Health : NetworkBehaviour
{
    [Header("Settings")]
    [SyncVar(hook = nameof(OnMaxHealthSynced))]
    public float maxHealth = 100f;
    [SyncVar(hook = nameof(OnCurrentHealthSynced))]
    public float currentHealth;
    public bool isPlayer  = false;  // players go downed instead of dying outright
    public bool isRobotic = false;  // Defibrillator deals 60 burst dmg to robotics

    [Header("Combat Feedback")]
    [SerializeField] bool showEnemyHealthBar = true;
    [SerializeField] bool showFloatingNumbers = true;
    [SerializeField, Tooltip("Extra world-space height above the detected enemy head for the floating health bar. Lower values pull it closer.")]
    float enemyHealthBarHeightOffset = 0.25f;
    [SerializeField, Tooltip("Optional fixed local Y height for the enemy health bar. Use -1 to auto-place from render/collider bounds.")]
    float enemyHealthBarFixedHeight = -1f;

    [Header("Enemy Hover Info")]
    [SerializeField, Tooltip("Show a top-screen tooltip and hover highlight when the cursor is over this enemy.")]
    bool showEnemyHoverInfo = true;
    [SerializeField, Tooltip("Optional name shown in the enemy hover tooltip. Leave blank to use the object name.")]
    string enemyDisplayName = "";
    [SerializeField, Min(0), Tooltip("Optional enemy level shown in the hover tooltip. Use 0 to hide the level for now.")]
    int enemyLevel = 0;

    [Header("Player Respawn")]
    [SerializeField, Tooltip("Automatically revive player characters after they stay downed for the respawn delay.")]
    bool autoRespawnPlayers = true;
    [SerializeField, Min(0f), Tooltip("Seconds a player remains downed before automatic respawn. Set to 0 for immediate respawn.")]
    float playerRespawnDelay = 10f;
    [SerializeField, Range(0.01f, 1f), Tooltip("Health percent restored by the automatic respawn.")]
    float playerRespawnHealthPercent = 1f;
    [SerializeField, Tooltip("Move the player back to a Network Start Position before restoring health. If disabled, the player revives where they fell.")]
    bool respawnAtStartPosition = false;
    [SerializeField, Min(0f), Tooltip("Seconds of server-side invulnerability after automatic respawn.")]
    float respawnInvulnerabilitySeconds = 2f;

    [Header("Simple Enemy Death / Respawn")]
    [SerializeField, Tooltip("Fallback lifecycle for enemies without EnemyController/major boss scripts. Keeps corpse briefly, despawns it, then optionally respawns it.")]
    bool manageSimpleEnemyDeathLifecycle = true;
    [SerializeField, Min(0f), Tooltip("Seconds the dead model remains visible before despawning.")]
    float simpleEnemyDeadModelSeconds = 3f;
    [SerializeField, Tooltip("If enabled, this simple enemy reappears after the respawn delay. Wave systems disable this on their spawned enemies.")]
    bool simpleEnemyRespawns = true;
    [SerializeField, Min(0f), Tooltip("Seconds after despawn before this simple enemy reappears.")]
    float simpleEnemyRespawnDelay = 30f;
    [SerializeField, Tooltip("If respawn is off, destroy the dead enemy after the dead model timer.")]
    bool destroySimpleEnemyIfRespawnDisabled = true;

    // ── Events ────────────────────────────────────────────────────
    public UnityEvent<float, float> onHealthChanged;    // (current, max)
    public UnityEvent               onDeath;
    public UnityEvent<float>        onDamageTaken;      // raw final damage (after shield, after redirect)
    public UnityEvent<GameObject>   onDamagedBy = new UnityEvent<GameObject>(); // source that caused the damage, when known
    public UnityEvent<float>        onHealApplied;      // heal amount (for Triage Loop)
    public UnityEvent<bool>         onDownedChanged;    // true = just went down, false = revived
    public UnityEvent<GameObject>   onKilledBy;         // who dealt the killing blow (for BountySystem)

    // ── Shield ────────────────────────────────────────────────────
    private float _shieldRemaining = 0f;
    public  bool  HasShield      => _shieldRemaining > 0f;
    public  float ShieldRemaining => _shieldRemaining;

    // ── Down State (players only) ──────────────────────────────────
    [SyncVar(hook = nameof(OnDownedSynced))]
    private bool _isDowned = false;
    public  bool IsDowned  => _isDowned;
    public  bool IsAlive   => !_isDowned && currentHealth > 0f;

    // ── Damage Redirect (Transfer Protocol) ───────────────────────
    // When set, a fraction of incoming damage is sent to redirectTarget instead.
    private Health _redirectTarget      = null;
    private float  _redirectFraction    = 0f;
    private float  _redirectExpiry      = 0f;
    private bool   _isRedirecting       = false; // re-entrancy guard for mutual redirects

    // ── Damage Absorption (Kinetic Reversal) ──────────────────────
    // When active, absorbed damage accumulates instead of hitting HP.
    private bool  _absorbing        = false;
    private float _absorptionExpiry = 0f;
    private float _absorbedAmount   = 0f;
    public  float AbsorbedAmount    => _absorbedAmount;

    // ── DR modifier (source-keyed) ────────────────────────────────
    // Siege Mode, Threat Protocol, resist flasks, and boss immunity each own a
    // named key so overlapping reductions coexist instead of clobbering the
    // single scalar they used to share.
    private readonly Dictionary<string, float> _drSources = new Dictionary<string, float>();
    // Combined multiplicatively so stacked sources are independent and never
    // exceed 100%:  eff = 1 - Π(1 - fᵢ).
    public float DamageReductionBonus
    {
        get
        {
            float remaining = 1f;
            foreach (var f in _drSources.Values) remaining *= (1f - Mathf.Clamp01(f));
            return Mathf.Clamp01(1f - remaining);
        }
    }

    // ── Gear / Attunement channels (driven by CharacterStats) ─────
    private float _baseMaxHealth       = 0f;   // captured at Awake, before gear
    private float _gearMaxHealthBonus  = 0f;
    private float _gearDamageReduction = 0f;   // stacks with ability DR
    public  float BaseMaxHealth        => _baseMaxHealth;

    public float Fraction => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public float EnemyHealthBarHeightOffset => enemyHealthBarHeightOffset;
    public float EnemyHealthBarFixedHeight => enemyHealthBarFixedHeight;
    public bool ShowEnemyHoverInfo => showEnemyHoverInfo;
    public int EnemyLevel => enemyLevel;
    public bool HasEnemyLevel => enemyLevel > 0;
    public string EnemyHoverDisplayName => GetEnemyHoverDisplayName();
    public bool ShouldShowEnemyHoverInfo() => showEnemyHoverInfo && !isPlayer && IsEnemyLikeForHud();

    // ── StatusEffect integration ───────────────────────────────────
    private StatusEffectManager _statusEffects;
    private Coroutine _playerRespawnRoutine;
    private Coroutine _respawnInvulnerabilityRoutine;
    private Coroutine _simpleEnemyDeathLifecycleRoutine;
    private bool _hasServerSpawnPoint;
    private Vector3 _serverSpawnPosition;
    private Quaternion _serverSpawnRotation;

    void Awake()
    {
        _baseMaxHealth = maxHealth;
        if (!NetworkClient.active && !NetworkServer.active)
        {
            currentHealth = maxHealth;
            _serverSpawnPosition = transform.position;
            _serverSpawnRotation = transform.rotation;
            _hasServerSpawnPoint = true;
        }
        _statusEffects = GetComponent<StatusEffectManager>();
#if UNITY_EDITOR || !UNITY_SERVER
        TryAttachEnemyHealthBar();
#endif
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _serverSpawnPosition = transform.position;
        _serverSpawnRotation = transform.rotation;
        _hasServerSpawnPoint = true;

        if (currentHealth <= 0f)
            currentHealth = maxHealth;
    }

    void OnDisable()
    {
        StopPlayerRespawnRoutine();
        StopRespawnInvulnerabilityRoutine();
        StopSimpleEnemyDeathLifecycleRoutine();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
#if UNITY_EDITOR || !UNITY_SERVER
        TryAttachEnemyHealthBar();
#endif
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        onDownedChanged?.Invoke(_isDowned);
    }

    void Update()
    {
        if (NetworkClient.active && !NetworkServer.active)
            return;

        // Clear expired redirect
        if (_redirectTarget != null && Time.time >= _redirectExpiry)
            ClearRedirect();

        // Clear expired absorption
        if (_absorbing && Time.time >= _absorptionExpiry)
            _absorbing = false;
    }

    // ── Public API ────────────────────────────────────────────────

    public void ApplyShield(float amount)
    {
        if (!CanMutateCombatState()) return;
        _shieldRemaining = Mathf.Max(_shieldRemaining, amount);
    }

    // ── Invulnerability (Dodge Roll i-frames) ────────────────────
    // Set to true by PlayerMovement during a dodge. While true, all damage is ignored.
    [HideInInspector] public bool isInvulnerable = false;

    public void TakeDamage(float amount, GameObject source = null)
    {
        if (!CanMutateCombatState()) return;
        if (_isDowned) return;
        if (currentHealth <= 0f) return;
        if (isInvulnerable) return;   // dodge roll i-frames

        // Weakened: +25% incoming (Collapsing Void)
        if (_statusEffects != null && _statusEffects.IsWeakened)
            amount *= 1.25f;

        // Damage reduction (source-keyed: Siege Mode, Threat Protocol, resist flasks, immunity)
        amount *= (1f - DamageReductionBonus);

        // Gear damage reduction (attunement system) — stacks multiplicatively
        amount *= (1f - _gearDamageReduction);

        // Damage redirect (Transfer Protocol) — redirect sends a portion to the medic.
        // _isRedirecting prevents an infinite loop when two Healths redirect to each
        // other (A→B while B→A): the re-entrant call skips its own redirect branch.
        if (_redirectTarget != null && _redirectTarget.IsAlive
            && Time.time < _redirectExpiry && !_isRedirecting)
        {
            float redirected = amount * _redirectFraction;
            amount -= redirected;
            _isRedirecting = true;
            _redirectTarget.TakeDamage(redirected); // no source override for the redirect
            _isRedirecting = false;
        }

        // Absorption (Kinetic Reversal) — intercept damage before shield/HP
        if (_absorbing && Time.time < _absorptionExpiry)
        {
            _absorbedAmount += amount;
            onDamageTaken?.Invoke(amount);
            onDamagedBy?.Invoke(source);
            return; // absorbed — no HP lost
        }

        // Shield
        if (_shieldRemaining > 0f)
        {
            float absorbed = Mathf.Min(_shieldRemaining, amount);
            _shieldRemaining -= absorbed;
            amount -= absorbed;
        }

        if (amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        onDamageTaken?.Invoke(amount);
        onDamagedBy?.Invoke(source);
        ShowDamageFeedback(amount);

        if (currentHealth <= 0f)
            HandleDeath(source);
    }

    public void Heal(float amount)
    {
        if (!CanMutateCombatState()) return;
        if (_isDowned || currentHealth <= 0f) return;
        float actual = Mathf.Min(amount, maxHealth - currentHealth);
        if (actual <= 0f) return;
        currentHealth += actual;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        onHealApplied?.Invoke(actual);
        ShowHealFeedback(actual);
    }

    // Defibrillator: revive a downed player at hpPercent (e.g. 0.3 = 30%)
    public void Revive(float hpPercent)
    {
        if (!CanMutateCombatState()) return;
        if (!_isDowned) return;
        StopPlayerRespawnRoutine();
        _isDowned     = false;
        currentHealth = maxHealth * Mathf.Clamp01(hpPercent);
        onDownedChanged?.Invoke(false);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ── Transfer Protocol ──────────────────────────────────────────
    // Redirects `fraction` (0–1) of incoming damage to `target` for `duration` seconds.
    public void SetDamageRedirect(Health target, float fraction, float duration)
    {
        if (!CanMutateCombatState()) return;
        if (target == this) return;   // never redirect to self (would loop)
        _redirectTarget   = target;
        _redirectFraction = fraction;
        _redirectExpiry   = Time.time + duration;
    }

    public void ClearRedirect()
    {
        if (!CanMutateCombatState()) return;
        _redirectTarget   = null;
        _redirectFraction = 0f;
    }

    // ── Kinetic Reversal ──────────────────────────────────────────
    public void BeginAbsorption(float duration)
    {
        if (!CanMutateCombatState()) return;
        _absorbing        = true;
        _absorbedAmount   = 0f;
        _absorptionExpiry = Time.time + duration;
    }

    public void EndAbsorption()
    {
        if (!CanMutateCombatState()) return;
        _absorbing = false;
    }

    // ── DR sources ─────────────────────────────────────────────────
    // Each caller owns a named key (e.g. "siege_mode", "threat_protocol",
    // "resist_void", "warden_immunity"). Setting the same key again replaces
    // that source's value; clearing removes only that source.
    public void SetDamageReduction(string source, float fraction)
    {
        if (!CanMutateCombatState()) return;
        _drSources[source] = Mathf.Clamp01(fraction);
    }

    public void ClearDamageReduction(string source)
    {
        if (!CanMutateCombatState()) return;
        _drSources.Remove(source);
    }

    // ── Gear / Attunement channels (called by CharacterStats) ─────
    // Adjusts max HP by the gear bonus, preserving current HP (and granting
    // the added HP on equip; clamping on unequip).
    public void SetGearMaxHealthBonus(float bonus)
    {
        if (!CanMutateCombatState()) return;
        if (_baseMaxHealth <= 0f) _baseMaxHealth = maxHealth; // safety if called pre-Awake
        float delta = bonus - _gearMaxHealthBonus;
        _gearMaxHealthBonus = bonus;
        maxHealth = _baseMaxHealth + _gearMaxHealthBonus;
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Max(0f, delta), 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetGearDamageReduction(float fraction)
    {
        if (!CanMutateCombatState()) return;
        _gearDamageReduction = Mathf.Clamp01(fraction);
    }

    // ── Adaptive Shield ───────────────────────────────────────────
    // Called by AdaptiveShieldHandler each time the target takes a hit.
    public void GrowShield(float amount)
    {
        if (!CanMutateCombatState()) return;
        _shieldRemaining = Mathf.Min(_shieldRemaining + amount, 80f);
    }

    public void SetSimpleEnemyRespawnEnabled(bool enabled)
    {
        simpleEnemyRespawns = enabled;
    }

    // ── Private ───────────────────────────────────────────────────
    private bool CanMutateCombatState()
    {
        if (!NetworkClient.active && !NetworkServer.active) return true;
        if (NetworkServer.active) return true;

        Debug.LogWarning($"[COMBAT] Ignored client-side Health mutation on {name}. Combat state must change on the server.");
        return false;
    }

    void OnCurrentHealthSynced(float oldValue, float newValue)
    {
        onHealthChanged?.Invoke(newValue, maxHealth);
    }

    void OnMaxHealthSynced(float oldValue, float newValue)
    {
        onHealthChanged?.Invoke(currentHealth, newValue);
    }

    void OnDownedSynced(bool oldValue, bool newValue)
    {
        onDownedChanged?.Invoke(newValue);
    }

    void StartSimpleEnemyDeathLifecycle()
    {
        if (!ShouldUseSimpleEnemyDeathLifecycle()) return;

        StopSimpleEnemyDeathLifecycleRoutine();
        SetSimpleEnemyCombatEnabled(false);
        _simpleEnemyDeathLifecycleRoutine = StartCoroutine(SimpleEnemyDeathLifecycleRoutine());
    }

    void StopSimpleEnemyDeathLifecycleRoutine()
    {
        if (_simpleEnemyDeathLifecycleRoutine == null) return;

        StopCoroutine(_simpleEnemyDeathLifecycleRoutine);
        _simpleEnemyDeathLifecycleRoutine = null;
    }

    bool ShouldUseSimpleEnemyDeathLifecycle()
    {
        if (!manageSimpleEnemyDeathLifecycle || isPlayer) return false;
        if (NetworkClient.active && !NetworkServer.active) return false;
        if (!IsEnemyLike()) return false;

        return GetComponent<EnemyController>() == null
            && GetComponent<WorldBossController>() == null
            && GetComponent<IronWardenController>() == null
            && GetComponent<SiegeTurretBehaviour>() == null;
    }

    bool IsEnemyLike()
    {
        return IsEnemyLikeForHud();
    }

    bool IsEnemyLikeForHud()
    {
        return CompareTag("Enemy")
            || GetComponent<EnemyAI>() != null
            || GetComponent<FieldGhoulNPC>() != null
            || GetComponent<EnemyController>() != null
            || GetComponent<IronWardenController>() != null
            || GetComponent<WorldBossController>() != null
            || GetComponent<SiegeTurretBehaviour>() != null
            || GetComponent<WispMob>() != null;
    }

    IEnumerator SimpleEnemyDeathLifecycleRoutine()
    {
        float corpseSeconds = Mathf.Max(0f, simpleEnemyDeadModelSeconds);
        if (corpseSeconds > 0f)
            yield return new WaitForSeconds(corpseSeconds);

        SetSimpleEnemyVisible(false);
        if (CanSendClientRpc())
            RpcSetSimpleEnemyVisible(false);

        if (!simpleEnemyRespawns)
        {
            _simpleEnemyDeathLifecycleRoutine = null;
            if (destroySimpleEnemyIfRespawnDisabled)
                DestroySimpleEnemyObject();
            yield break;
        }

        float respawnSeconds = Mathf.Max(0f, simpleEnemyRespawnDelay);
        if (respawnSeconds > 0f)
            yield return new WaitForSeconds(respawnSeconds);

        _simpleEnemyDeathLifecycleRoutine = null;
        RespawnSimpleEnemy();
    }

    void DestroySimpleEnemyObject()
    {
        if (NetworkServer.active && netIdentity != null && netIdentity.netId != 0)
            NetworkServer.Destroy(gameObject);
        else
            Destroy(gameObject);
    }

    void RespawnSimpleEnemy()
    {
        GetSimpleEnemyRespawnTransform(out Vector3 position, out Quaternion rotation);

        ApplySimpleEnemyRespawn(position, rotation);
        if (CanSendClientRpc())
            RpcRespawnSimpleEnemy(position, rotation);
    }

    void GetSimpleEnemyRespawnTransform(out Vector3 position, out Quaternion rotation)
    {
        FieldGhoulNPC fieldMob = GetComponent<FieldGhoulNPC>();
        if (fieldMob != null && fieldMob.HasLeashReturnPoint)
        {
            position = fieldMob.LeashReturnPosition;
            rotation = fieldMob.LeashReturnRotation;
            return;
        }

        EnemyAI enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            position = enemyAI.HomePosition;
            rotation = enemyAI.HomeRotation;
            return;
        }

        position = _hasServerSpawnPoint ? _serverSpawnPosition : transform.position;
        rotation = _hasServerSpawnPoint ? _serverSpawnRotation : transform.rotation;
    }

    bool CanSendClientRpc()
    {
        return NetworkServer.active
            && netIdentity != null
            && netIdentity.netId != 0;
    }

    [ClientRpc]
    void RpcSetSimpleEnemyVisible(bool visible)
    {
        SetSimpleEnemyVisible(visible);
    }

    [ClientRpc]
    void RpcRespawnSimpleEnemy(Vector3 position, Quaternion rotation)
    {
        ApplySimpleEnemyRespawn(position, rotation);
    }

    void ApplySimpleEnemyRespawn(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            rb.rotation = rotation;
        }

        _statusEffects?.RemoveAll();

        if (!NetworkClient.active || NetworkServer.active)
            currentHealth = maxHealth;

        SetSimpleEnemyVisible(true);
        SetSimpleEnemyCombatEnabled(true);
        ResetSimpleEnemyAnimators();
        BroadcastMessage("OnEnemyRespawned", SendMessageOptions.DontRequireReceiver);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        Physics.SyncTransforms();
    }

    void SetSimpleEnemyVisible(bool visible)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            renderer.enabled = visible;
    }

    void SetSimpleEnemyCombatEnabled(bool enabled)
    {
        foreach (var collider in GetComponentsInChildren<Collider>(true))
            collider.enabled = enabled;

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (!enabled)
            {
                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                }
                agent.enabled = false;
            }
            else
            {
                agent.enabled = true;
                if (agent.isOnNavMesh)
                {
                    agent.Warp(transform.position);
                    agent.isStopped = false;
                }
            }
        }

        var enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
            enemyAI.enabled = enabled;
    }

    void ResetSimpleEnemyAnimators()
    {
        foreach (var animator in GetComponentsInChildren<Animator>(true))
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    void StartPlayerRespawnRoutine()
    {
        if (!autoRespawnPlayers || !isPlayer) return;

        StopPlayerRespawnRoutine();
        _playerRespawnRoutine = StartCoroutine(PlayerRespawnRoutine());
    }

    void StopPlayerRespawnRoutine()
    {
        if (_playerRespawnRoutine == null) return;

        StopCoroutine(_playerRespawnRoutine);
        _playerRespawnRoutine = null;
    }

    IEnumerator PlayerRespawnRoutine()
    {
        float delay = Mathf.Max(0f, playerRespawnDelay);
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        _playerRespawnRoutine = null;
        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        if (!isPlayer || !_isDowned) return;

        if (respawnAtStartPosition)
            MoveToRespawnPoint();

        Revive(playerRespawnHealthPercent);
        StartRespawnInvulnerability();
    }

    void MoveToRespawnPoint()
    {
        Vector3 position = _hasServerSpawnPoint ? _serverSpawnPosition : transform.position;
        Quaternion rotation = _hasServerSpawnPoint ? _serverSpawnRotation : transform.rotation;

        Transform startPosition = NetworkManager.singleton != null
            ? NetworkManager.singleton.GetStartPosition()
            : null;

        if (startPosition != null)
        {
            position = startPosition.position;
            rotation = startPosition.rotation;
        }

        ApplyRespawnTransform(position, rotation);

        var networkTransform = GetComponent<NetworkTransformBase>();
        if (NetworkServer.active && networkTransform != null)
            networkTransform.ServerTeleport(position, rotation);
        else if (NetworkServer.active)
            RpcApplyRespawnTransform(position, rotation);
    }

    [ClientRpc]
    void RpcApplyRespawnTransform(Vector3 position, Quaternion rotation)
    {
        ApplyRespawnTransform(position, rotation);
    }

    void ApplyRespawnTransform(Vector3 position, Quaternion rotation)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            rb.rotation = rotation;
        }

        transform.SetPositionAndRotation(position, rotation);
        Physics.SyncTransforms();
    }

    void StartRespawnInvulnerability()
    {
        StopRespawnInvulnerabilityRoutine();
        isInvulnerable = false;

        float duration = Mathf.Max(0f, respawnInvulnerabilitySeconds);
        if (duration <= 0f) return;

        isInvulnerable = true;
        _respawnInvulnerabilityRoutine = StartCoroutine(RespawnInvulnerabilityRoutine(duration));
    }

    void StopRespawnInvulnerabilityRoutine()
    {
        if (_respawnInvulnerabilityRoutine != null)
        {
            StopCoroutine(_respawnInvulnerabilityRoutine);
            _respawnInvulnerabilityRoutine = null;
        }

        isInvulnerable = false;
    }

    IEnumerator RespawnInvulnerabilityRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
        _respawnInvulnerabilityRoutine = null;
    }

    void ShowDamageFeedback(float amount)
    {
        if (!showFloatingNumbers || amount <= 0f) return;

        if (CanRpcCombatFeedback())
            RpcShowDamageNumber(amount, transform.position);
        else if (!NetworkClient.active || NetworkServer.active)
            SpawnDamageNumber(amount, transform.position);
    }

    void ShowHealFeedback(float amount)
    {
        if (!showFloatingNumbers || amount <= 0f) return;

        if (CanRpcCombatFeedback())
            RpcShowHealNumber(amount, transform.position);
        else if (!NetworkClient.active || NetworkServer.active)
            SpawnHealNumber(amount, transform.position);
    }

    bool CanRpcCombatFeedback()
    {
        return NetworkServer.active
            && netIdentity != null
            && netIdentity.netId != 0;
    }

    [ClientRpc]
    void RpcShowDamageNumber(float amount, Vector3 worldPosition)
    {
        SpawnDamageNumber(amount, worldPosition);
    }

    [ClientRpc]
    void RpcShowHealNumber(float amount, Vector3 worldPosition)
    {
        SpawnHealNumber(amount, worldPosition);
    }

    static void SpawnDamageNumber(float amount, Vector3 worldPosition)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        FloatingDamageText.Spawn(worldPosition, amount, FloatingDamageText.DamageType.Normal);
#endif
    }

    static void SpawnHealNumber(float amount, Vector3 worldPosition)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        FloatingDamageText.Spawn(worldPosition, amount, FloatingDamageText.DamageType.Heal);
#endif
    }

    string GetEnemyHoverDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(enemyDisplayName))
            return enemyDisplayName.Trim();

        return FormatObjectNameForDisplay(gameObject.name);
    }

    static string FormatObjectNameForDisplay(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
            return "Enemy";

        string cleaned = rawName
            .Replace("(Clone)", "")
            .Replace("_", " ")
            .Replace("-", " ")
            .Trim();

        if (cleaned.Length == 0)
            return "Enemy";

        var spaced = new StringBuilder(cleaned.Length + 8);
        for (int i = 0; i < cleaned.Length; i++)
        {
            char c = cleaned[i];
            if (i > 0
                && char.IsUpper(c)
                && !char.IsWhiteSpace(cleaned[i - 1])
                && (char.IsLower(cleaned[i - 1])
                    || (i + 1 < cleaned.Length && char.IsLower(cleaned[i + 1]))))
            {
                spaced.Append(' ');
            }

            spaced.Append(c);
        }

        bool upperNext = true;
        for (int i = 0; i < spaced.Length; i++)
        {
            char c = spaced[i];
            if (char.IsWhiteSpace(c))
            {
                upperNext = true;
                continue;
            }

            if (upperNext)
            {
                spaced[i] = char.ToUpperInvariant(c);
                upperNext = false;
            }
        }

        return spaced.ToString();
    }

#if UNITY_EDITOR || !UNITY_SERVER
    void TryAttachEnemyHealthBar()
    {
        if (!showEnemyHealthBar || isPlayer) return;
        if (!ShouldShowEnemyHealthBar()) return;
        if (GetComponent<EnemyHealthBar>() != null) return;

        gameObject.AddComponent<EnemyHealthBar>();
    }

    bool ShouldShowEnemyHealthBar()
    {
        return IsEnemyLikeForHud();
    }
#endif

    private void HandleDeath(GameObject source)
    {
        if (isPlayer)
        {
            _isDowned = true;
            onDownedChanged?.Invoke(true);
            StartPlayerRespawnRoutine();
            // Do NOT invoke onDeath for players — they are downed, not dead.
        }
        else
        {
            onDeath?.Invoke();
            onKilledBy?.Invoke(source);
            StartSimpleEnemyDeathLifecycle();
        }
    }
}
