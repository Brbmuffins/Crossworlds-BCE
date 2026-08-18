using System;
using System.Collections;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

public static class VendorPersistenceService
{
    static VendorPersistenceHost _host;
    static string Token => Environment.GetEnvironmentVariable("CROSSWORLDS_GAME_SERVICE_TOKEN") ?? "";
    static string BaseUrl => (Environment.GetEnvironmentVariable("CROSSWORLDS_AUTH_URL") ?? "http://127.0.0.1:3000").TrimEnd('/');
    static VendorPersistenceHost Host
    {
        get
        {
            if (_host != null) return _host;
            var go = new GameObject("[VendorPersistenceService]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            return _host = go.AddComponent<VendorPersistenceHost>();
        }
    }

    public static void SyncProfile(VendorProfile profile)
    {
        if (!NetworkServer.active || profile == null || string.IsNullOrWhiteSpace(Token)) return;
        Host.StartCoroutine(SyncProfileAfterItems(profile));
    }

    static IEnumerator SyncProfileAfterItems(VendorProfile profile)
    {
        // Loot Forge definitions synchronize at server startup too. Give those item
        // rows time to exist before inserting foreign-keyed vendor stock, then retry
        // transient failures rather than leaving the vendor unusable for this boot.
        yield return new WaitForSeconds(2f);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            bool succeeded = false;
            yield return Post("/api/game/vendors/definitions", ProfileJson(profile),
                (ok, _) => succeeded = ok);
            if (succeeded) yield break;
            yield return new WaitForSeconds(3f);
        }
    }

    public static void Buy(NetworkConnectionToClient sender, VendorProfile profile, string itemId, int quantity,
        Action<NetworkConnectionToClient, bool, string> callback) =>
        Transact(sender, profile, "/api/game/vendors/buy",
            $"{{\"characterId\":{CharacterId(sender)},\"vendorId\":\"{Escape(profile.vendorId)}\",\"itemId\":\"{Escape(itemId)}\",\"quantity\":{quantity}}}", callback);

    public static void Sell(NetworkConnectionToClient sender, VendorProfile profile, int slotIndex, int quantity,
        Action<NetworkConnectionToClient, bool, string> callback) =>
        Transact(sender, profile, "/api/game/vendors/sell",
            $"{{\"characterId\":{CharacterId(sender)},\"vendorId\":\"{Escape(profile.vendorId)}\",\"slotIndex\":{slotIndex},\"quantity\":{quantity}}}", callback);

    static void Transact(NetworkConnectionToClient sender, VendorProfile profile, string path, string json,
        Action<NetworkConnectionToClient, bool, string> callback)
    {
        if (!NetworkServer.active || sender == null || profile == null || string.IsNullOrWhiteSpace(Token))
        {
            callback?.Invoke(sender, false, "Vendor service is unavailable.");
            return;
        }
        Host.StartCoroutine(Post(path, json, (ok, response) => callback?.Invoke(sender, ok, Message(response, ok))));
    }

    static IEnumerator Post(string path, string json, Action<bool, string> callback)
    {
        using var request = new UnityWebRequest(BaseUrl + path, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-Game-Server-Token", Token);
        request.timeout = 10;
        yield return request.SendWebRequest();
        bool ok = request.result == UnityWebRequest.Result.Success;
        if (!ok) Debug.LogWarning($"[VENDOR DB] {path} failed: {request.error} {request.downloadHandler.text}");
        callback?.Invoke(ok, request.downloadHandler?.text);
    }

    static string ProfileJson(VendorProfile profile)
    {
        var dto = new ProfileDto
        {
            vendorId = profile.vendorId?.Trim(), displayName = profile.displayName?.Trim(),
            buysItems = profile.buysItems, stock = Array.ConvertAll(profile.stock ?? Array.Empty<VendorStockEntry>(), e =>
                new StockDto { itemId = e?.item?.itemId, buyPrice = Mathf.Max(1, e?.buyPrice ?? 1) })
        };
        return JsonUtility.ToJson(dto);
    }

    static int CharacterId(NetworkConnectionToClient sender) =>
        sender?.identity != null ? sender.identity.GetComponent<PlayerIdentity>()?.characterId ?? -1 : -1;
    static string Escape(string value) => (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
    static string Message(string json, bool ok)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            try { var response = JsonUtility.FromJson<ResponseDto>(json); if (!string.IsNullOrWhiteSpace(response?.message)) return response.message; if (!string.IsNullOrWhiteSpace(response?.error)) return response.error; }
            catch { }
        }
        return ok ? "Transaction completed." : "Transaction failed.";
    }

    [Serializable] sealed class ProfileDto { public string vendorId, displayName; public bool buysItems; public StockDto[] stock; }
    [Serializable] sealed class StockDto { public string itemId; public int buyPrice; }
    [Serializable] sealed class ResponseDto { public string message, error; }
}

public sealed class VendorPersistenceHost : MonoBehaviour { }
