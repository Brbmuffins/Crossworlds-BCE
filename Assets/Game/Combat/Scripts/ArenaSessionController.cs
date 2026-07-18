using Mirror;
using UnityEngine;

/// <summary>
/// ArenaSessionController — tracks XP and kills for the current combat session.
/// Attach to a NetworkIdentity GameObject in the Hub/Arena scene.
/// WaveSpawner calls BeginSession() and EndSession() automatically.
/// </summary>
public class ArenaSessionController : NetworkBehaviour
{
    public static ArenaSessionController Instance { get; private set; }

    [SyncVar] public int  totalKills   = 0;
    [SyncVar] public int  totalXP      = 0;
    [SyncVar] public int  currentWave  = 0;
    [SyncVar] public bool sessionActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    [Server]
    public void BeginSession()
    {
        totalKills    = 0;
        totalXP       = 0;
        currentWave   = 0;
        sessionActive = true;
        Debug.Log("[ArenaSession] Session started.");
    }

    [Server]
    public void EndSession()
    {
        sessionActive = false;
        Debug.Log($"[ArenaSession] Session ended. Kills: {totalKills}  XP: {totalXP}  Waves: {currentWave}");
    }

    [Server]
    public void RegisterKill(int xpReward)
    {
        totalKills++;
        totalXP += xpReward;
    }

    [Server]
    public void AdvanceWave(int wave)
    {
        currentWave = wave;
    }
}
