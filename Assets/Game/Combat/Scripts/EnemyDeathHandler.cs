using UnityEngine;
using Mirror;

/// <summary>
/// EnemyDeathHandler — lightweight death handler for scene-placed enemies
/// that do NOT use EnemyController (which handles its own death internally).
///
/// Auto-wires to Health.onDeath via OnStartServer.
/// On death (server only):
///   1. Rolls DropTable → spawns WorldItem(s) via NetworkServer.Spawn
///   2. RpcAwardProgress → each client calls PlayerProgressManager + HeroMasteryManager
///
/// If EnemyController is present on the same GO, this component self-disables
/// in Awake to avoid double-awarding XP and double-spawning loot.
///
/// Inspector:
///   xpReward       — XP given to every player (default 25)
///   dropTable      — ScriptableObject asset; leave null for no item drops
///   worldItemPrefab— prefab with WorldItem component; required for loot to appear
/// </summary>
public class EnemyDeathHandler : NetworkBehaviour
{
    [Header("Rewards")]
    [Tooltip("XP awarded to every player in the arena on death.")]
    public int xpReward = 25;

    [Header("Drops")]
    [Tooltip("ScriptableObject DropTable asset. Assign in Inspector on each enemy prefab.")]
    public DropTable dropTable;
    [Tooltip("WorldItem prefab spawned for each rolled drop. Must have a WorldItem component.")]
    public GameObject worldItemPrefab;

    // ── Auto-found refs ───────────────────────────────────────────────────────
    Health _health;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        // Arena-only: disable in Hub and any non-Arena scene (defensive guard)
        if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Arena"))
        {
            enabled = false;
            return;
        }

        // Yield to EnemyController if present — it handles death fully
        if (GetComponent<EnemyController>() != null)
        {
            enabled = false;
            return;
        }

        _health = GetComponent<Health>();
        if (_health == null)
            Debug.LogWarning($"[DEATH] {name}: EnemyDeathHandler requires a Health component.");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (_health != null)
            _health.onDeath.AddListener(OnDeathServer);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (_health != null)
            _health.onDeath.RemoveListener(OnDeathServer);
    }

#if !UNITY_SERVER
    public override void OnStartClient()
    {
        base.OnStartClient();
        // Notify CombatSessionTracker so it can hook damage/kill events
        CombatSessionTracker.Local?.NotifyEnemySpawned(gameObject);
    }
#endif

    // ── Server: death ─────────────────────────────────────────────────────────
    [Server]
    void OnDeathServer()
    {
        if (dropTable != null)
        {
            var (items, gold) = dropTable.RollDrops();
            foreach (var (itemId, qty) in items)
                SpawnWorldItem(itemId, qty);
            if (gold > 0)
                SpawnWorldItem($"gold:{gold}", 1);
        }

        if (xpReward > 0)
            RpcAwardProgress(xpReward);

        Debug.Log($"[DEATH] {name} killed — {xpReward}xp broadcast");
    }

    [Server]
    void SpawnWorldItem(string itemId, int qty)
    {
        if (worldItemPrefab == null)
        {
            Debug.LogWarning($"[DEATH] {name}: worldItemPrefab not assigned — loot lost ({itemId})");
            return;
        }

        Vector3 scatter = Random.insideUnitSphere * 1.2f;
        scatter.y = 0.5f;
        var wi   = Instantiate(worldItemPrefab, transform.position + scatter, Quaternion.identity);
        var comp = wi.GetComponent<WorldItem>();
        if (comp != null) { comp.itemId = itemId; comp.quantity = qty; }
        NetworkServer.Spawn(wi);
        Debug.Log($"[LOOT] {name} dropped {qty}x {itemId}");
    }

    // ── ClientRpc ─────────────────────────────────────────────────────────────
    [ClientRpc]
    void RpcAwardProgress(int xp)
    {
#if !UNITY_SERVER
        PlayerProgressManager.Local?.AwardXp(xp);
        int heroId = PlayerProgressManager.Local?.ClassIndex ?? 0;
        HeroMasteryManager.Local?.AwardXp(heroId, Mathf.RoundToInt(xp * 0.5f));
#endif
    }
}
