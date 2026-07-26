using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

[Serializable]
public sealed class LocalQuestProgress
{
    public string questId;
    public LocalQuestStatus status;
    public List<int> objectiveProgress = new();
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
            Definitions[definition.questId] = definition;
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
        return index >= 0 && index < state.objectiveProgress.Count
            ? state.objectiveProgress[index] : 0;
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
        if (!string.IsNullOrWhiteSpace(itemId) && itemQuantity > 0)
            InventoryManager.Instance?.OnItemPickedUp(itemId, itemQuantity);
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
    }

    [Server]
    public static void ServerComplete(NetworkConnectionToClient sender, string questId)
    {
        if (!TryGetDefinition(questId, out QuestDefinition definition) ||
            !QuestGiver.ServerPlayerIsNearQuest(sender, definition)) return;
        Dictionary<string, LocalQuestProgress> states = GetServerStates(sender);
        if (!states.TryGetValue(questId, out var state) ||
            state.status != LocalQuestStatus.ReadyToTurnIn) return;
        state.status = LocalQuestStatus.Completed;
        SendState(sender);
        RodChatManager.Instance?.ServerGrantQuestReward(sender, definition.goldReward,
            definition.experienceReward, definition.itemRewardId, definition.itemRewardQuantity);
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
                if (objective.type != type ||
                    !string.Equals(objective.targetId, targetId, StringComparison.OrdinalIgnoreCase) ||
                    (definition.objectivesMustBeCompletedInOrder && !PriorComplete(definition, state, i)))
                    continue;
                int required = Mathf.Max(1, objective.requiredAmount);
                int next = Mathf.Clamp(state.objectiveProgress[i] + amount, 0, required);
                changed |= next != state.objectiveProgress[i];
                state.objectiveProgress[i] = next;
            }
            changed |= UpdateReady(definition, state);
        }
        if (changed) SendState(sender);
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
        var state = new LocalQuestProgress { questId = definition.questId, status = status };
        for (int i = 0; i < definition.objectives.Count; i++) state.objectiveProgress.Add(0);
        return state;
    }

    static bool PriorComplete(QuestDefinition definition, LocalQuestProgress state, int index)
    {
        for (int i = 0; i < index; i++)
            if (state.objectiveProgress[i] < Mathf.Max(1, definition.objectives[i].requiredAmount))
                return false;
        return true;
    }

    static bool UpdateReady(QuestDefinition definition, LocalQuestProgress state)
    {
        if (state.status != LocalQuestStatus.Active) return false;
        for (int i = 0; i < definition.objectives.Count; i++)
            if (state.objectiveProgress[i] < Mathf.Max(1, definition.objectives[i].requiredAmount))
                return false;
        state.status = LocalQuestStatus.ReadyToTurnIn;
        return true;
    }

    [Server]
    static void SendState(NetworkConnectionToClient sender)
    {
        var envelope = new QuestStateEnvelope();
        envelope.quests.AddRange(GetServerStates(sender).Values);
        RodChatManager.Instance?.ServerSendQuestState(sender, JsonUtility.ToJson(envelope));
    }
}
