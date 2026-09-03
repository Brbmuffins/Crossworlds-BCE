using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Server-authoritative shared depletion state for one gathering node. The node
/// remains spawned as a network scene object while its visual and colliders are
/// hidden, then becomes available again after the configured timer.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
[DisallowMultipleComponent]
public sealed class GatheringNodeNetworkState : NetworkBehaviour
{
    [Header("Stable Identity")]
    [Tooltip("Unique per placed node. Node Forge assigns this automatically.")]
    public string persistentNodeId;

    [Header("Depletion")]
    [Min(1)] public int minimumAwardsPerSpawn = 1;
    [Min(1)] public int maximumAwardsPerSpawn = 5;
    [Min(1f)] public float respawnSeconds = 900f;

    [Header("Server Validation")]
    [Min(0.5f)] public float interactionRange = 3f;
    [Min(0.1f)] public float minimumSecondsBetweenAwards = 4.75f;

    [Header("Authoritative Reward")]
    public GatheringLootTable lootTable;
    [Min(0)] public int experiencePerAward = 10;
    [Min(1)] public int bonusYieldLevel = 10;
    [Tooltip("Profession index. Mining is 2.")]
    public int professionId = 2;

    [SyncVar(hook = nameof(OnRemainingAwardsChanged))]
    int remainingAwards;
    [SyncVar(hook = nameof(OnDepletedChanged))]
    bool depleted;
    [SyncVar]
    double respawnAtNetworkTime;

    public int RemainingAwards => remainingAwards;
    public bool IsDepleted => depleted;
    public double RespawnAtNetworkTime => respawnAtNetworkTime;

    public event Action<bool, bool, int, string, int, int, int, bool, string> AwardRequestCompleted;

    readonly Dictionary<int, double> nextAwardByConnection = new();
    readonly HashSet<int> pendingAwardConnections = new();
    Renderer[] cachedRenderers = Array.Empty<Renderer>();
    Collider[] cachedColliders = Array.Empty<Collider>();
    bool[] rendererDefaults = Array.Empty<bool>();
    bool[] colliderDefaults = Array.Empty<bool>();
    bool presentationCached;

    struct PersistedState
    {
        public int remaining;
        public bool depleted;
        public double respawnAt;
    }

    // Retains timers while additive zones unload and reload during one server run.
    static readonly Dictionary<string, PersistedState> ServerStates = new();

    public override void OnStartServer()
    {
        base.OnStartServer();
        string key = StateKey();
        if (ServerStates.TryGetValue(key, out PersistedState saved))
        {
            remainingAwards = saved.remaining;
            depleted = saved.depleted;
            respawnAtNetworkTime = saved.respawnAt;
            if (depleted && NetworkTime.time >= respawnAtNetworkTime)
                ResetNode("respawn-after-zone-reload");
            else
                Debug.Log($"[GATHER NODE] RESTORE {NodeLogLabel()} remaining={remainingAwards} " +
                          $"depleted={depleted} respawnIn={Math.Max(0d, respawnAtNetworkTime - NetworkTime.time):0.0}s");
        }
        else ResetNode("spawn");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        CachePresentation();
        ApplyPresentation(depleted);
    }

    [ServerCallback]
    void Update()
    {
        if (depleted && NetworkTime.time >= respawnAtNetworkTime)
            ResetNode("respawn");
    }

    public void RequestAward()
    {
        if (!isClient)
        {
            AwardRequestCompleted?.Invoke(false, depleted, remainingAwards, "", 0, 0, 0, false,
                "Gathering requires a server connection.");
            return;
        }
        CmdRequestAward();
    }

    public void ApplyOfflinePresentation(bool hide) => ApplyPresentation(hide);

    [Command(requiresAuthority = false)]
    void CmdRequestAward(NetworkConnectionToClient sender = null)
    {
        if (sender?.identity == null)
            return;

        if (depleted && NetworkTime.time >= respawnAtNetworkTime)
            ResetNode("respawn-on-request");

        float allowedRange = Mathf.Max(0.5f, interactionRange) + 1f;
        if ((sender.identity.transform.position - transform.position).sqrMagnitude >
            allowedRange * allowedRange)
        {
            TargetAwardResult(sender, false, depleted, remainingAwards);
            return;
        }

        int connectionId = sender.connectionId;
        if (pendingAwardConnections.Contains(connectionId)) return;
        if (nextAwardByConnection.TryGetValue(connectionId, out double nextAllowed) &&
            NetworkTime.time < nextAllowed)
        {
            TargetAwardResult(sender, false, depleted, remainingAwards);
            return;
        }

        if (depleted || remainingAwards <= 0)
        {
            if (!depleted) DepleteNode();
            TargetAwardResult(sender, false, true, 0);
            return;
        }
        if (remainingAwards - pendingAwardConnections.Count <= 0)
        {
            TargetAwardResult(sender, false, false, remainingAwards, "", 0, 0, 0, false,
                "Another miner is finishing this node's final reward.");
            return;
        }

        if (lootTable == null || !lootTable.TryRoll(out LootItemDefinition item, out int quantity))
        {
            TargetAwardResult(sender, false, depleted, remainingAwards, "", 0, 0, 0, false,
                "This node has no valid reward table.");
            return;
        }

        pendingAwardConnections.Add(connectionId);
        GatheringPersistenceService.Award(sender, persistentNodeId, item, quantity,
            professionId, experiencePerAward, bonusYieldLevel, CompleteAward);
    }

    [Server]
    void CompleteAward(NetworkConnectionToClient sender, GatheringAwardResponse response)
    {
        if (sender == null) return;
        int connectionId = sender.connectionId;
        pendingAwardConnections.Remove(connectionId);
        if (response?.success != true || response.data == null || response.data.stored < 1)
        {
            TargetAwardResult(sender, false, depleted, remainingAwards, "", 0, 0, 0, false,
                response?.error ?? "Your inventory is full.");
            return;
        }

        nextAwardByConnection[connectionId] =
            NetworkTime.time + Mathf.Max(0.1f, minimumSecondsBetweenAwards);
        int awardsBefore = remainingAwards;
        remainingAwards--;
        bool exhausted = remainingAwards <= 0;
        Debug.Log($"[GATHER NODE] AWARD {NodeLogLabel()} connection={connectionId} " +
                  $"player={sender.identity.name} before={awardsBefore} remaining={remainingAwards} " +
                  $"depleted={exhausted}");
        if (exhausted) DepleteNode();
        else SaveServerState();

        GatheringAwardData data = response.data;
        TargetAwardResult(sender, true, exhausted, remainingAwards, data.item_id,
            data.stored, data.skill_level, data.skill_xp, data.leveled_up,
            data.bonus_yield ? "Bonus yield!" : "");
    }

    [TargetRpc]
    void TargetAwardResult(NetworkConnection target, bool granted, bool nowDepleted, int awardsLeft,
        string itemId = "", int quantity = 0, int skillLevel = 0, int skillXp = 0,
        bool leveledUp = false, string message = "")
    {
        AwardRequestCompleted?.Invoke(granted, nowDepleted, awardsLeft, itemId, quantity,
            skillLevel, skillXp, leveledUp, message);
    }

    [Server]
    void ResetNode(string reason)
    {
        int minimum = Mathf.Max(1, minimumAwardsPerSpawn);
        int maximum = Mathf.Max(minimum, maximumAwardsPerSpawn);
        remainingAwards = UnityEngine.Random.Range(minimum, maximum + 1);
        depleted = false;
        respawnAtNetworkTime = 0d;
        nextAwardByConnection.Clear();
        pendingAwardConnections.Clear();
        SaveServerState();
        Debug.Log($"[GATHER NODE] READY {NodeLogLabel()} reason={reason} " +
                  $"awards={remainingAwards} range={minimum}-{maximum} " +
                  $"respawnSeconds={Mathf.Max(1f, respawnSeconds):0.#}");
    }

    [Server]
    void DepleteNode()
    {
        remainingAwards = 0;
        depleted = true;
        respawnAtNetworkTime = NetworkTime.time + Mathf.Max(1f, respawnSeconds);
        SaveServerState();
        Debug.Log($"[GATHER NODE] DEPLETED {NodeLogLabel()} remaining=0 " +
                  $"respawnSeconds={Mathf.Max(1f, respawnSeconds):0.#} " +
                  $"respawnAt={respawnAtNetworkTime:0.000}");
    }

    [Server]
    void SaveServerState()
    {
        ServerStates[StateKey()] = new PersistedState
        {
            remaining = remainingAwards,
            depleted = depleted,
            respawnAt = respawnAtNetworkTime
        };
    }

    string StateKey()
    {
        string id = string.IsNullOrWhiteSpace(persistentNodeId)
            ? $"{transform.position.x:0.###}_{transform.position.y:0.###}_{transform.position.z:0.###}"
            : persistentNodeId.Trim();
        return $"{gameObject.scene.path}|{id}";
    }

    string NodeLogLabel()
    {
        string id = string.IsNullOrWhiteSpace(persistentNodeId)
            ? "position-fallback"
            : persistentNodeId.Trim();
        return $"scene={gameObject.scene.name} node={name} id={id}";
    }

    void OnRemainingAwardsChanged(int oldValue, int newValue)
    {
#if UNITY_EDITOR || !UNITY_SERVER
        GetComponent<AfkGatheringStation>()?.SetRemainingAwards(newValue);
#endif
    }

    void OnDepletedChanged(bool oldValue, bool newValue) => ApplyPresentation(newValue);

    void CachePresentation()
    {
        if (presentationCached) return;
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider>(true);
        rendererDefaults = new bool[cachedRenderers.Length];
        colliderDefaults = new bool[cachedColliders.Length];
        for (int i = 0; i < cachedRenderers.Length; i++)
            rendererDefaults[i] = cachedRenderers[i] != null && cachedRenderers[i].enabled;
        for (int i = 0; i < cachedColliders.Length; i++)
            colliderDefaults[i] = cachedColliders[i] != null && cachedColliders[i].enabled;
        presentationCached = true;
    }

    void ApplyPresentation(bool hide)
    {
        CachePresentation();
        for (int i = 0; i < cachedRenderers.Length; i++)
            if (cachedRenderers[i] != null)
                cachedRenderers[i].enabled = !hide && rendererDefaults[i];
        for (int i = 0; i < cachedColliders.Length; i++)
            if (cachedColliders[i] != null)
                cachedColliders[i].enabled = !hide && colliderDefaults[i];
#if UNITY_EDITOR || !UNITY_SERVER
        AfkGatheringStation station = GetComponent<AfkGatheringStation>();
        if (station != null)
        {
            station.SetRemainingAwards(remainingAwards);
            station.SetNodeDepleted(hide);
        }
#endif
    }

    void OnValidate()
    {
        minimumAwardsPerSpawn = Mathf.Max(1, minimumAwardsPerSpawn);
        maximumAwardsPerSpawn = Mathf.Max(minimumAwardsPerSpawn, maximumAwardsPerSpawn);
        respawnSeconds = Mathf.Max(1f, respawnSeconds);
        interactionRange = Mathf.Max(0.5f, interactionRange);
        minimumSecondsBetweenAwards = Mathf.Max(0.1f, minimumSecondsBetweenAwards);
        experiencePerAward = Mathf.Max(0, experiencePerAward);
        bonusYieldLevel = Mathf.Max(1, bonusYieldLevel);
    }
}
