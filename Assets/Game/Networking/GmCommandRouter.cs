using System;
using System.Collections.Generic;
using System.Globalization;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GmCommandRouter
{
    static readonly HashSet<string> TemporaryGmUsers = new(StringComparer.OrdinalIgnoreCase)
    {
        "DevPlayer",
        "brbmuffins",
        "ForYurHealth",
        "YaDingusMD",
        "SleepyBoySteve",
    };

    static readonly Dictionary<string, ArriveDestination> ArriveDestinations =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["hub"] = new ArriveDestination(SceneNames.Hub, HubReturnSpawnPoint.DefaultSpawnId, "HUB"),
            ["darkwood"] = new ArriveDestination(SceneNames.Darkwood, "DarkwoodSpawnLocation", "Darkwood"),
            ["dw"] = new ArriveDestination(SceneNames.Darkwood, "DarkwoodSpawnLocation", "Darkwood"),
            ["ashen"] = new ArriveDestination(SceneNames.AshenWastelands, "AshenWastelandsSpawnPoint", "Ashen Wastelands"),
            ["ashenwastelands"] = new ArriveDestination(SceneNames.AshenWastelands, "AshenWastelandsSpawnPoint", "Ashen Wastelands"),
        };

    public static bool TryHandle(string rawMessage, NetworkConnectionToClient sender, RodChatManager chat)
    {
        if (string.IsNullOrWhiteSpace(rawMessage) || !rawMessage.StartsWith("/", StringComparison.Ordinal))
            return false;

        string[] parts = rawMessage.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return true;

        string command = parts[0].TrimStart('/').ToLowerInvariant();
        switch (command)
        {
            case "gm":
                HandleGmToggle(parts, sender, chat);
                return true;
            case "gmhelp":
                SendHelp(sender, chat);
                return true;
            case "arrive":
                HandleArrive(parts, sender, chat);
                return true;
            case "fly":
                HandleFly(parts, sender, chat);
                return true;
            case "speed":
                HandleSpeed(parts, sender, chat);
                return true;
            default:
                chat.SendGmFeedback(sender, $"Unknown GM command '/{command}'. Try /gmhelp.");
                return true;
        }
    }

    static void HandleGmToggle(string[] parts, NetworkConnectionToClient sender, RodChatManager chat)
    {
        if (!TryGetAuth(sender, chat, out RodPlayerAuth auth))
            return;

        if (!IsGmAllowed(auth))
        {
            chat.SendGmFeedback(sender, "GM access denied.");
            Debug.LogWarning($"[GM] Denied /gm request from '{auth.username}'.");
            return;
        }

        if (parts.Length < 2)
        {
            chat.SendGmFeedback(sender, auth.gmActive ? "GM mode is ON." : "GM mode is OFF. Use /gm on.");
            return;
        }

        string mode = parts[1].ToLowerInvariant();
        if (mode == "on" || mode == "1" || mode == "true")
        {
            auth.gmActive = true;
            chat.SendGmFeedback(sender, "GM mode ON.");
            Debug.Log($"[GM] {auth.username} enabled GM mode.");
            return;
        }

        if (mode == "off" || mode == "0" || mode == "false")
        {
            auth.gmActive = false;
            auth.gmFlyEnabled = false;
            auth.gmSpeedMultiplier = 1f;
            chat.TargetSetGmFly(sender, false);
            chat.TargetSetGmSpeed(sender, 1f);
            chat.SendGmFeedback(sender, "GM mode OFF. Fly and speed reset.");
            Debug.Log($"[GM] {auth.username} disabled GM mode.");
            return;
        }

        chat.SendGmFeedback(sender, "Usage: /gm on or /gm off");
    }

    static void HandleArrive(string[] parts, NetworkConnectionToClient sender, RodChatManager chat)
    {
        if (!RequireActiveGm(sender, chat, "arrive", out RodPlayerAuth auth))
            return;

        if (parts.Length < 2)
        {
            chat.SendGmFeedback(sender, "Usage: /arrive hub | darkwood | ashen");
            return;
        }

        string key = NormalizeKey(parts[1]);
        if (!ArriveDestinations.TryGetValue(key, out ArriveDestination destination))
        {
            chat.SendGmFeedback(sender, "Unknown arrival. Use: hub, darkwood, ashen.");
            return;
        }

        if (!CanLoadScene(destination.SceneName))
        {
            chat.SendGmFeedback(sender, $"{destination.DisplayName} is not in Build Settings.");
            return;
        }

        if (SceneMatchesCurrent(destination.SceneName))
        {
            if (TryPlaceSenderAtCurrentSceneSpawn(sender, destination, chat))
                Debug.Log($"[GM] {auth.username} arrived at {destination.DisplayName} in current scene.");
            return;
        }

        if (NetworkManager.singleton == null)
        {
            chat.SendGmFeedback(sender, "No NetworkManager is available for scene travel.");
            return;
        }

        HubReturnArrival.Request(destination.SceneName, destination.SpawnId, true);
        chat.SendGmFeedback(sender, $"Arriving at {destination.DisplayName}...");
        Debug.Log($"[GM] {auth.username} requested arrive: {destination.SceneName}/{destination.SpawnId}");
        NetworkManager.singleton.ServerChangeScene(destination.SceneName);
    }

    static void HandleFly(string[] parts, NetworkConnectionToClient sender, RodChatManager chat)
    {
        if (!RequireActiveGm(sender, chat, "fly", out RodPlayerAuth auth))
            return;

        bool enabled = !auth.gmFlyEnabled;
        if (parts.Length >= 2 && TryParseToggle(parts[1], out bool explicitValue))
            enabled = explicitValue;

        auth.gmFlyEnabled = enabled;
        chat.TargetSetGmFly(sender, enabled);
        chat.SendGmFeedback(sender, enabled ? "Fly ON. Space rises, Ctrl descends." : "Fly OFF.");
        Debug.Log($"[GM] {auth.username} set fly={enabled}.");
    }

    static void HandleSpeed(string[] parts, NetworkConnectionToClient sender, RodChatManager chat)
    {
        if (!RequireActiveGm(sender, chat, "speed", out RodPlayerAuth auth))
            return;

        if (parts.Length < 2 ||
            !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float multiplier))
        {
            chat.SendGmFeedback(sender, "Usage: /speed 1 | /speed 2.5");
            return;
        }

        multiplier = Mathf.Clamp(multiplier, 0.25f, 8f);
        auth.gmSpeedMultiplier = multiplier;
        chat.TargetSetGmSpeed(sender, multiplier);
        chat.SendGmFeedback(sender, $"Speed set to x{multiplier:0.##}.");
        Debug.Log($"[GM] {auth.username} set speed x{multiplier:0.##}.");
    }

    static bool TryPlaceSenderAtCurrentSceneSpawn(
        NetworkConnectionToClient sender,
        ArriveDestination destination,
        RodChatManager chat)
    {
        if (sender?.identity == null)
        {
            chat.SendGmFeedback(sender, "No player object found to move.");
            return false;
        }

        Transform spawnPoint = HubReturnSpawnPoint.Find(destination.SpawnId);
        if (spawnPoint == null)
        {
            chat.SendGmFeedback(sender, $"Arrival point '{destination.SpawnId}' was not found here.");
            return false;
        }

        GameObject player = sender.identity.gameObject;
        Vector3 position = spawnPoint.position;
        Quaternion rotation = spawnPoint.rotation;

        HubReturnArrival.PlacePlayer(player, spawnPoint, Vector3.zero, true);

        var networkTransform = player.GetComponent<NetworkTransformBase>();
        if (networkTransform != null)
            networkTransform.ServerTeleport(position, rotation);

        chat.SendGmFeedback(sender, $"Arrived at {destination.DisplayName}.");
        return true;
    }

    static void SendHelp(NetworkConnectionToClient sender, RodChatManager chat)
    {
        chat.SendGmFeedback(sender,
            "GM commands: /gm on, /gm off, /arrive hub|darkwood|ashen, /fly [on|off], /speed <multiplier>.");
    }

    static bool RequireActiveGm(
        NetworkConnectionToClient sender,
        RodChatManager chat,
        string permission,
        out RodPlayerAuth auth)
    {
        auth = null;
        if (!TryGetAuth(sender, chat, out auth))
            return false;

        if (!IsGmAllowed(auth))
        {
            chat.SendGmFeedback(sender, "GM access denied.");
            Debug.LogWarning($"[GM] Denied /{permission} from '{auth.username}'.");
            return false;
        }

        if (!auth.gmActive)
        {
            chat.SendGmFeedback(sender, "GM mode is OFF. Use /gm on first.");
            return false;
        }

        return true;
    }

    static bool TryGetAuth(NetworkConnectionToClient sender, RodChatManager chat, out RodPlayerAuth auth)
    {
        auth = sender?.authenticationData as RodPlayerAuth;
        if (auth != null)
            return true;

        chat.SendGmFeedback(sender, "No authenticated player found for GM command.");
        return false;
    }

    static bool IsGmAllowed(RodPlayerAuth auth)
    {
        if (auth == null)
            return false;

        return auth.gmAllowed ||
               auth.gmLevel > 0 ||
               string.Equals(auth.jwt, "dev", StringComparison.OrdinalIgnoreCase) ||
               TemporaryGmUsers.Contains(auth.username ?? "");
    }

    static bool TryParseToggle(string value, out bool enabled)
    {
        switch (value.ToLowerInvariant())
        {
            case "on":
            case "1":
            case "true":
            case "yes":
                enabled = true;
                return true;
            case "off":
            case "0":
            case "false":
            case "no":
                enabled = false;
                return true;
            default:
                enabled = false;
                return false;
        }
    }

    static string NormalizeKey(string value)
    {
        return value.Replace(" ", "").Replace("-", "").Replace("_", "").ToLowerInvariant();
    }

    static bool CanLoadScene(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName) &&
               (Application.CanStreamedLevelBeLoaded(sceneName) || SceneMatchesCurrent(sceneName));
    }

    static bool SceneMatchesCurrent(string sceneName)
    {
        return string.Equals(SceneManager.GetActiveScene().name, sceneName, StringComparison.OrdinalIgnoreCase);
    }

    readonly struct ArriveDestination
    {
        public readonly string SceneName;
        public readonly string SpawnId;
        public readonly string DisplayName;

        public ArriveDestination(string sceneName, string spawnId, string displayName)
        {
            SceneName = sceneName;
            SpawnId = spawnId;
            DisplayName = displayName;
        }
    }
}
