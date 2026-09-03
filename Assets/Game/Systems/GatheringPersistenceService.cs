using System;
using System.Collections;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Trusted dedicated-server bridge for atomic gathering rewards.</summary>
public static class GatheringPersistenceService
{
    static GatheringPersistenceHost _host;
    static string Token => Environment.GetEnvironmentVariable("CROSSWORLDS_GAME_SERVICE_TOKEN") ?? "";
    static string BaseUrl => (Environment.GetEnvironmentVariable("CROSSWORLDS_AUTH_URL") ??
                             "http://127.0.0.1:3000").TrimEnd('/');

    static GatheringPersistenceHost Host
    {
        get
        {
            if (_host != null) return _host;
            var go = new GameObject("[GatheringPersistenceService]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            return _host = go.AddComponent<GatheringPersistenceHost>();
        }
    }

    [Server]
    public static void Award(NetworkConnectionToClient sender, string nodeId,
        LootItemDefinition item, int baseQuantity, int professionId, int xp,
        int bonusYieldLevel, Action<NetworkConnectionToClient, GatheringAwardResponse> callback)
    {
        int characterId = sender?.identity?.GetComponent<PlayerIdentity>()?.characterId ?? -1;
        if (!NetworkServer.active || characterId < 1 || item == null ||
            item.databaseItemType != LootDatabaseItemType.Material ||
            string.IsNullOrWhiteSpace(item.itemId) || string.IsNullOrWhiteSpace(Token))
        {
            callback?.Invoke(sender, GatheringAwardResponse.Failed("Gathering service is unavailable."));
            return;
        }

        var payload = new GatheringAwardRequest
        {
            characterId = characterId,
            nodeId = nodeId ?? "",
            itemId = item.itemId.Trim(),
            baseQuantity = Mathf.Clamp(baseQuantity, 1, 99),
            professionId = ProfessionWireId(professionId),
            xpAmount = Mathf.Clamp(xp, 0, 500),
            bonusYieldLevel = Mathf.Max(1, bonusYieldLevel)
        };
        Host.StartCoroutine(Post(sender, JsonUtility.ToJson(payload), callback));
    }

    static string ProfessionWireId(int professionId) => professionId switch
    {
        0 => "woodcutting",
        1 => "fishing",
        _ => "mining"
    };

    static IEnumerator Post(NetworkConnectionToClient sender, string json,
        Action<NetworkConnectionToClient, GatheringAwardResponse> callback)
    {
        using var request = new UnityWebRequest(BaseUrl + "/api/game/gathering/award", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Game-Server-Token", Token);
        request.timeout = 10;
        yield return request.SendWebRequest();

        GatheringAwardResponse response = null;
        try { response = JsonUtility.FromJson<GatheringAwardResponse>(request.downloadHandler?.text ?? ""); }
        catch (Exception exception) { Debug.LogWarning($"[GATHER DB] Invalid response: {exception.Message}"); }
        if (request.result != UnityWebRequest.Result.Success || response?.success != true)
        {
            string error = response?.error;
            if (string.IsNullOrWhiteSpace(error)) error = request.error ?? "Gathering reward failed.";
            Debug.LogWarning($"[GATHER DB] Award failed: {error}");
            response = GatheringAwardResponse.Failed(error);
        }
        callback?.Invoke(sender, response);
    }

    [Serializable] sealed class GatheringAwardRequest
    {
        public int characterId;
        public string nodeId;
        public string itemId;
        public int baseQuantity;
        public string professionId;
        public int xpAmount;
        public int bonusYieldLevel;
    }
}

[Serializable]
public sealed class GatheringAwardResponse
{
    public bool success;
    public string error;
    public GatheringAwardData data;
    public static GatheringAwardResponse Failed(string message) =>
        new() { success = false, error = message ?? "Gathering reward failed." };
}

[Serializable]
public sealed class GatheringAwardData
{
    public string item_id;
    public string item_name;
    public int quantity;
    public int stored;
    public int rejected;
    public bool bonus_yield;
    public int skill_level;
    public int skill_xp;
    public bool leveled_up;
}

public sealed class GatheringPersistenceHost : MonoBehaviour { }
