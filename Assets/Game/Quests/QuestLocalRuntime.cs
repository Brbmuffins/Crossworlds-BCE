using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[Serializable]
public sealed class LocalQuestProgress
{
    public string questId;
    public int definitionVersion = 1;
    public LocalQuestStatus status;
    public List<string> objectiveIds = new();
    public List<int> objectiveProgress = new();
    public bool rewardClaimed;
}

public enum LocalQuestStatus { Available, Active, ReadyToTurnIn, Completed }

[Serializable]
sealed class QuestStateEnvelope
{
    public List<LocalQuestProgress> quests = new();
}

/// <summary>
/// Mirror-compatible quest state. The server owns progress; this client object
/// is only an owner-specific synchronized view used by markers and UI.
/// Persistence is intentionally omitted while Quest Forge remains in testing.
/// </summary>
public sealed class QuestLocalRuntime : MonoBehaviour
{
    public static QuestLocalRuntime Instance { get; private set; }
    public static event Action StateChanged;

    static readonly Dictionary<int, Dictionary<string, LocalQuestProgress>> ServerStates = new();
    static readonly Dictionary<string, QuestDefinition> Definitions =
        new(StringComparer.OrdinalIgnoreCase);

    readonly Dictionary<string, LocalQuestProgress> _clientStates =
        new(StringComparer.OrdinalIgnoreCase);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        if (Instance != null) return;
        var go = new GameObject("[QuestMirrorClient]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<QuestLocalRuntime>();
    }

    public void Register(QuestDefinition definition) => RegisterDefinition(definition);

    public static void RegisterDefinition(QuestDefinition definition)
    {
        if (definition != null && !string.IsNullOrWhiteSpace(definition.questId))
        {
            Definitions[definition.questId] = definition;
            QuestPersistenceService.SyncDefinition(definition);
        }
    }

    public LocalQuestStatus GetStatus(QuestDefinition definition)
    {
        if (definition == null) return LocalQuestStatus.Completed;
        RegisterDefinition(definition);
        return _clientStates.TryGetValue(definition.questId, out var state)
            ? state.status : LocalQuestStatus.Available;
    }

    public int GetProgress(QuestDefinition definition, int index)
    {
        if (definition == null || !_clientStates.TryGetValue(definition.questId, out var state))
            return 0;
        if (index < 0 || index >= definition.objectives.Count) return 0;
        string objectiveId = definition.objectives[index]?.objectiveId;
        int persistedIndex = !string.IsNullOrWhiteSpace(objectiveId)
            ? state.objectiveIds.FindIndex(x => string.Equals(x, objectiveId, StringComparison.OrdinalIgnoreCase))
            : index;
        return persistedIndex >= 0 && persistedIndex < state.objectiveProgress.Count
            ? state.objectiveProgress[persistedIndex] : 0;
    }

    public void Accept(QuestDefinition definition)
    {
        RegisterDefinition(definition);
        RodChatManager.Instance?.RequestQuestAccept(definition != null ? definition.questId : "");
    }

    public void Complete(QuestDefinition definition)
    {
        RegisterDefinition(definition);
        RodChatManager.Instance?.RequestQuestComplete(definition != null ? definition.questId : "");
    }

    public static void RequestInteraction(string targetId) =>
        RodChatManager.Instance?.RequestQuestInteraction(targetId);

    public static void ClientApplyState(string json)
    {
        if (Instance == null) Bootstrap();
        if (Instance == null) return;
        Instance._clientStates.Clear();
        QuestStateEnvelope envelope = JsonUtility.FromJson<QuestStateEnvelope>(json);
        if (envelope?.quests != null)
            foreach (LocalQuestProgress state in envelope.quests)
                if (state != null && !string.IsNullOrWhiteSpace(state.questId))
                    Instance._clientStates[state.questId] = state;
        StateChanged?.Invoke();
    }

    public static void ClientGrantReward(int gold, int xp, string itemId, int itemQuantity)
    {
        if (gold > 0) PlayerProgressManager.Local?.AwardGold(gold);
        if (xp > 0) PlayerProgressManager.Local?.AwardXp(xp);
#if UNITY_EDITOR || !UNITY_SERVER
        if (!string.IsNullOrWhiteSpace(itemId) && itemQuantity > 0)
            InventoryManager.Instance?.OnItemPickedUp(itemId, itemQuantity);
#endif
    }

    [Server]
    public static void ServerAccept(NetworkConnectionToClient sender, string questId)
    {
        if (!TryGetDefinition(questId, out QuestDefinition definition) ||
            !QuestGiver.ServerPlayerIsNearQuest(sender, definition)) return;
        Dictionary<string, LocalQuestProgress> states = GetServerStates(sender);
        if (states.ContainsKey(questId)) return;
        states[questId] = CreateProgress(definition, LocalQuestStatus.Active);
        UpdateReady(definition, states[questId]);
        SendState(sender);
        QuestPersistenceService.SaveState(sender, definition, states[questId]);
    }

    [Server]
    public static void ServerComplete(NetworkConnectionToClient sender, string questId)
    {
        if (!TryGetDefinition(questId, out QuestDefinition definition) ||
            !QuestGiver.ServerPlayerIsNearQuest(sender, definition)) return;
        Dictionary<string, LocalQuestProgress> states = GetServerStates(sender);
        if (!states.TryGetValue(questId, out var state) ||
            state.status != LocalQuestStatus.ReadyToTurnIn) return;
        if (!QuestPersistenceService.IsEnabled)
        {
            state.status = LocalQuestStatus.Completed;
            SendState(sender);
            RodChatManager.Instance?.ServerGrantQuestReward(sender, definition.goldReward,
                definition.experienceReward, definition.itemRewardId, definition.itemRewardQuantity);
            return;
        }
        QuestPersistenceService.ClaimReward(sender, definition, () =>
        {
            state.status = LocalQuestStatus.Completed;
            state.rewardClaimed = true;
            SendState(sender);
            RodChatManager.Instance?.ServerRefreshQuestRewards(sender);
        });
    }

    [Server]
    public static void ServerRequestInteraction(NetworkConnectionToClient sender, string targetId)
    {
        if (sender?.identity == null || string.IsNullOrWhiteSpace(targetId)) return;
        bool nearby = false;
        foreach (QuestInteractableTarget target in
                 UnityEngine.Object.FindObjectsByType<QuestInteractableTarget>(FindObjectsInactive.Exclude))
        {
            if (string.Equals(target.targetId, targetId, StringComparison.OrdinalIgnoreCase) &&
                target.gameObject.scene == sender.identity.gameObject.scene &&
                Vector3.Distance(target.transform.position, sender.identity.transform.position) <=
                target.interactionRadius + 1.5f)
            {
                nearby = true;
                break;
            }
        }
        if (nearby) ServerReport(sender, QuestObjectiveType.InteractWithObject, targetId, 1);
    }

    [Server]
    public static void ServerReport(
        NetworkConnectionToClient sender, QuestObjectiveType type, string targetId, int amount)
    {
        if (sender == null || string.IsNullOrWhiteSpace(targetId) || amount <= 0) return;
        string reportedTargetId = type == QuestObjectiveType.KillEnemy
            ? QuestTargetId.NormalizeEnemy(targetId) : targetId;
        Dictionary<string, LocalQuestProgress> states = GetServerStates(sender);
        bool changed = false;
        foreach (var pair in states)
        {
            if (pair.Value.status != LocalQuestStatus.Active ||
                !TryGetDefinition(pair.Key, out QuestDefinition definition)) continue;
            LocalQuestProgress state = pair.Value;
            for (int i = 0; i < definition.objectives.Count; i++)
            {
                QuestObjectiveDefinition objective = definition.objectives[i];
                int stateIndex = StateIndex(state, objective, i);
                if (stateIndex < 0) continue;
                string objectiveTargetId = type == QuestObjectiveType.KillEnemy
                    ? QuestTargetId.NormalizeEnemy(objective.targetId) : objective.targetId;
                if (objective.type != type ||
                    !string.Equals(objectiveTargetId, reportedTargetId,
                        StringComparison.OrdinalIgnoreCase) ||
                    (definition.objectivesMustBeCompletedInOrder && !PriorComplete(definition, state, i)))
                    continue;
                int required = Mathf.Max(1, objective.requiredAmount);
                int next = Mathf.Clamp(state.objectiveProgress[stateIndex] + amount, 0, required);
                changed |= next != state.objectiveProgress[stateIndex];
                state.objectiveProgress[stateIndex] = next;
            }
            changed |= UpdateReady(definition, state);
        }
        if (changed)
        {
            SendState(sender);
            foreach (var pair in states)
                if (TryGetDefinition(pair.Key, out QuestDefinition definition))
                    QuestPersistenceService.SaveState(sender, definition, pair.Value);
        }
    }

    [Server]
    public static void ServerClear(NetworkConnectionToClient sender)
    {
        if (sender == null) return;
        ServerStates.Remove(sender.connectionId);
        SendState(sender);
    }

    [Server]
    public static void ServerForget(NetworkConnectionToClient sender)
    {
        if (sender != null) ServerStates.Remove(sender.connectionId);
    }

    [Server]
    public static void ServerLoad(NetworkConnectionToClient sender) =>
        QuestPersistenceService.LoadState(sender, ApplyLoadedServerState);

    [Server]
    static void ApplyLoadedServerState(NetworkConnectionToClient sender, List<LocalQuestProgress> loaded)
    {
        if (sender == null) return;
        var states = GetServerStates(sender);
        states.Clear();
        if (loaded != null)
            foreach (LocalQuestProgress state in loaded)
                if (state != null && !string.IsNullOrWhiteSpace(state.questId))
                    states[state.questId] = state;
        SendState(sender);
    }

    static bool TryGetDefinition(string id, out QuestDefinition definition) =>
        Definitions.TryGetValue(id ?? "", out definition);

    static Dictionary<string, LocalQuestProgress> GetServerStates(NetworkConnectionToClient sender)
    {
        if (!ServerStates.TryGetValue(sender.connectionId, out var states))
        {
            states = new Dictionary<string, LocalQuestProgress>(StringComparer.OrdinalIgnoreCase);
            ServerStates[sender.connectionId] = states;
        }
        return states;
    }

    static LocalQuestProgress CreateProgress(QuestDefinition definition, LocalQuestStatus status)
    {
        var state = new LocalQuestProgress
        {
            questId = definition.questId,
            definitionVersion = Mathf.Max(1, definition.definitionVersion),
            status = status
        };
        for (int i = 0; i < definition.objectives.Count; i++)
        {
            state.objectiveIds.Add(definition.objectives[i]?.objectiveId ?? $"legacy_{i}");
            state.objectiveProgress.Add(0);
        }
        return state;
    }

    static bool PriorComplete(QuestDefinition definition, LocalQuestProgress state, int index)
    {
        for (int i = 0; i < index; i++)
        {
            int stateIndex = StateIndex(state, definition.objectives[i], i);
            if (stateIndex < 0 ||
                state.objectiveProgress[stateIndex] < Mathf.Max(1, definition.objectives[i].requiredAmount))
                return false;
        }
        return true;
    }

    static bool UpdateReady(QuestDefinition definition, LocalQuestProgress state)
    {
        if (state.status != LocalQuestStatus.Active) return false;
        for (int i = 0; i < definition.objectives.Count; i++)
        {
            int stateIndex = StateIndex(state, definition.objectives[i], i);
            if (stateIndex < 0 ||
                state.objectiveProgress[stateIndex] < Mathf.Max(1, definition.objectives[i].requiredAmount))
                return false;
        }
        state.status = LocalQuestStatus.ReadyToTurnIn;
        return true;
    }

    static int StateIndex(LocalQuestProgress state, QuestObjectiveDefinition objective, int legacyIndex)
    {
        if (state == null) return -1;
        if (!string.IsNullOrWhiteSpace(objective?.objectiveId))
        {
            int found = state.objectiveIds.FindIndex(x =>
                string.Equals(x, objective.objectiveId, StringComparison.OrdinalIgnoreCase));
            if (found >= 0 && found < state.objectiveProgress.Count) return found;
        }
        return legacyIndex >= 0 && legacyIndex < state.objectiveProgress.Count ? legacyIndex : -1;
    }

    [Server]
    static void SendState(NetworkConnectionToClient sender)
    {
        var envelope = new QuestStateEnvelope();
        envelope.quests.AddRange(GetServerStates(sender).Values);
        RodChatManager.Instance?.ServerSendQuestState(sender, JsonUtility.ToJson(envelope));
    }
}
