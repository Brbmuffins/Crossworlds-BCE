using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Dedicated-server adapter for persistent Quest Forge state.</summary>
public static class QuestPersistenceService
{
    static QuestPersistenceHost _host;
    static readonly HashSet<string> SyncedDefinitions = new(StringComparer.OrdinalIgnoreCase);

    static string ServiceToken => Environment.GetEnvironmentVariable("CROSSWORLDS_GAME_SERVICE_TOKEN") ?? "";

    static string AuthBaseUrl
    {
        get
        {
            string configured = Environment.GetEnvironmentVariable("CROSSWORLDS_AUTH_URL");
            if (!string.IsNullOrWhiteSpace(configured)) return configured.TrimEnd('/');
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++)
                if (string.Equals(args[i], "-authurl", StringComparison.OrdinalIgnoreCase))
                    return args[i + 1].TrimEnd('/');
            return "http://127.0.0.1:3000";
        }
    }

    static QuestPersistenceHost Host
    {
        get
        {
            if (_host != null) return _host;
            var go = new GameObject("[QuestPersistenceService]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _host = go.AddComponent<QuestPersistenceHost>();
            return _host;
        }
    }

    public static bool IsEnabled => NetworkServer.active && !string.IsNullOrWhiteSpace(ServiceToken);

    public static void SyncDefinition(QuestDefinition definition)
    {
        if (!IsEnabled || definition == null || string.IsNullOrWhiteSpace(definition.questId)) return;
        string key = $"{definition.questId}:{Mathf.Max(1, definition.definitionVersion)}";
        if (!SyncedDefinitions.Add(key)) return;
        Host.StartCoroutine(PostJson("/api/game/quests/definitions", BuildDefinition(definition), null));
    }

    [Server]
    public static void LoadState(NetworkConnectionToClient sender,
        Action<NetworkConnectionToClient, List<LocalQuestProgress>> callback)
    {
        int characterId = CharacterId(sender);
        if (!IsEnabled || characterId < 1) { callback?.Invoke(sender, new List<LocalQuestProgress>()); return; }
        Host.StartCoroutine(GetState(sender, characterId, callback));
    }

    [Server]
    public static void SaveState(NetworkConnectionToClient sender, QuestDefinition definition,
        LocalQuestProgress state)
    {
        int characterId = CharacterId(sender);
        if (!IsEnabled || characterId < 1 || definition == null || state == null) return;
        Host.StartCoroutine(SyncThenSend(definition, $"/api/game/quests/state/{characterId}",
            "PUT", BuildState(state), null));
    }

    [Server]
    public static void ClaimReward(NetworkConnectionToClient sender, QuestDefinition definition, Action onSuccess)
    {
        int characterId = CharacterId(sender);
        if (!IsEnabled || characterId < 1 || definition == null)
        {
            Debug.LogError("[QUEST DB] Reward claim refused because persistence is not configured.");
            return;
        }
        Host.StartCoroutine(SyncThenSend(definition,
            $"/api/game/quests/claim/{characterId}/{UnityWebRequest.EscapeURL(definition.questId)}",
            "POST", "{}", onSuccess));
    }

    static int CharacterId(NetworkConnectionToClient sender) =>
        sender?.identity != null ? sender.identity.GetComponent<PlayerIdentity>()?.characterId ?? -1 : -1;

    static IEnumerator GetState(NetworkConnectionToClient sender, int characterId,
        Action<NetworkConnectionToClient, List<LocalQuestProgress>> callback)
    {
        using var request = UnityWebRequest.Get($"{AuthBaseUrl}/api/game/quests/state/{characterId}");
        Configure(request);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[QUEST DB] Load failed for character {characterId}: {request.error}");
            callback?.Invoke(sender, new List<LocalQuestProgress>());
            yield break;
        }
        QuestStateResponse response = JsonUtility.FromJson<QuestStateResponse>(request.downloadHandler.text);
        var result = new List<LocalQuestProgress>();
        if (response?.data?.quests != null)
            foreach (QuestStateDto quest in response.data.quests) result.Add(quest.ToRuntime());
        callback?.Invoke(sender, result);
    }

    static IEnumerator PostJson(string path, string json, Action onSuccess) =>
        SendJson(path, "POST", json, onSuccess);
    static IEnumerator PutJson(string path, string json, Action onSuccess) =>
        SendJson(path, "PUT", json, onSuccess);

    static IEnumerator SendJson(string path, string method, string json, Action onSuccess)
    {
        using var request = new UnityWebRequest(AuthBaseUrl + path, method);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        Configure(request);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success) onSuccess?.Invoke();
        else Debug.LogWarning($"[QUEST DB] {method} {path} failed: {request.error} {request.downloadHandler.text}");
    }

    static IEnumerator SyncThenSend(QuestDefinition definition, string path, string method,
        string json, Action onSuccess)
    {
        bool synced = false;
        yield return SendJson("/api/game/quests/definitions", "POST", BuildDefinition(definition),
            () => synced = true);
        if (!synced) yield break;
        yield return SendJson(path, method, json, onSuccess);
    }

    static void Configure(UnityWebRequest request)
    {
        request.SetRequestHeader("X-Game-Server-Token", ServiceToken);
        request.timeout = 10;
    }

    static string BuildDefinition(QuestDefinition q)
    {
        bool hasItemReward = q.itemRewardQuantity > 0 && !string.IsNullOrWhiteSpace(q.itemRewardId);
        var dto = new QuestDefinitionDto
        {
            questId = q.questId, definitionVersion = Mathf.Max(1, q.definitionVersion), title = q.title,
            description = q.description, minimumLevel = Mathf.Max(1, q.minimumLevel), offerText = q.offerText,
            activeText = q.activeText, completionText = q.completionText,
            objectivesInOrder = q.objectivesMustBeCompletedInOrder, goldReward = q.goldReward,
            experienceReward = q.experienceReward,
            itemRewardId = hasItemReward ? q.itemRewardId.Trim() : null,
            itemRewardQuantity = hasItemReward ? q.itemRewardQuantity : 0
        };
        for (int i = 0; i < q.objectives.Count; i++)
        {
            QuestObjectiveDefinition o = q.objectives[i];
            if (o == null) continue;
            dto.objectives.Add(new QuestObjectiveDto { objectiveId = o.objectiveId, type = TypeName(o.type),
                targetId = o.targetId, description = o.description, requiredAmount = Mathf.Max(1, o.requiredAmount) });
        }
        return JsonUtility.ToJson(dto);
    }

    static string BuildState(LocalQuestProgress state)
    {
        var dto = new QuestStateSaveDto { questId = state.questId, status = StatusName(state.status) };
        for (int i = 0; i < state.objectiveIds.Count; i++)
            dto.objectives.Add(new QuestObjectiveStateDto { objectiveId = state.objectiveIds[i],
                currentAmount = i < state.objectiveProgress.Count ? state.objectiveProgress[i] : 0 });
        return JsonUtility.ToJson(dto);
    }

    static string TypeName(QuestObjectiveType type) => type switch
    {
        QuestObjectiveType.KillEnemy => "kill_enemy",
        QuestObjectiveType.CollectItem => "collect_item",
        QuestObjectiveType.InteractWithObject => "interact_object",
        _ => "enter_area"
    };
    static string StatusName(LocalQuestStatus status) => status switch
    {
        LocalQuestStatus.ReadyToTurnIn => "ready_to_turn_in",
        LocalQuestStatus.Completed => "complete",
        _ => "active"
    };

    [Serializable] sealed class QuestDefinitionDto
    {
        public string questId, title, description, offerText, activeText, completionText, itemRewardId;
        public int definitionVersion, minimumLevel, goldReward, experienceReward, itemRewardQuantity;
        public bool objectivesInOrder;
        public List<QuestObjectiveDto> objectives = new();
    }
    [Serializable] sealed class QuestObjectiveDto
    { public string objectiveId, type, targetId, description; public int requiredAmount; }
    [Serializable] sealed class QuestStateSaveDto
    { public string questId, status; public List<QuestObjectiveStateDto> objectives = new(); }
    [Serializable] sealed class QuestObjectiveStateDto
    { public string objectiveId; public int currentAmount; public bool completed; }
    [Serializable] sealed class QuestStateResponse { public bool success; public QuestStateEnvelopeDto data; }
    [Serializable] sealed class QuestStateEnvelopeDto { public List<QuestStateDto> quests = new(); }
    [Serializable] sealed class QuestStateDto
    {
        public string questId, status; public int definitionVersion; public bool rewardClaimed;
        public List<QuestObjectiveStateDto> objectives = new();
        public LocalQuestProgress ToRuntime()
        {
            var state = new LocalQuestProgress { questId = questId, definitionVersion = definitionVersion,
                status = status == "complete" ? LocalQuestStatus.Completed :
                    status == "ready_to_turn_in" ? LocalQuestStatus.ReadyToTurnIn : LocalQuestStatus.Active,
                rewardClaimed = rewardClaimed };
            foreach (QuestObjectiveStateDto objective in objectives)
            { state.objectiveIds.Add(objective.objectiveId); state.objectiveProgress.Add(objective.currentAmount); }
            return state;
        }
    }
}

public sealed class QuestPersistenceHost : MonoBehaviour { }
