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
            ["boneyard"] = new ArriveDestination(SceneNames.Boneyard, "BoneYardPlayerSpawnPoint", "Bone Yard"),
            ["bone"] = new ArriveDestination(SceneNames.Boneyard, "BoneYardPlayerSpawnPoint", "Bone Yard"),
            ["pvp"] = new ArriveDestination(SceneNames.PvpZone, HubReturnSpawnPoint.DefaultSpawnId, "PVP Zone"),
            ["pvpzone"] = new ArriveDestination(SceneNames.PvpZone, HubReturnSpawnPoint.DefaultSpawnId, "PVP Zone"),
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
            case "freecam":
                HandleFreeCamera(parts, sender, chat);
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
            chat.TargetSetGmMode(sender, false);
            chat.SendGmFeedback(sender, "GM access denied.");
            Debug.LogWarning($"[GM] Denied /gm request from '{auth.username}'.");
            return;
        }

        if (parts.Length < 2)
        {
            chat.TargetSetGmMode(sender, auth.gmActive);
            chat.SendGmFeedback(sender, auth.gmActive ? "GM mode is ON." : "GM mode is OFF. Use /gm on.");
            return;
        }

        string mode = parts[1].ToLowerInvariant();
        if (mode == "on" || mode == "1" || mode == "true")
        {
            auth.gmActive = true;
            chat.TargetSetGmMode(sender, true);
            chat.SendGmFeedback(sender, "GM mode ON. Press M anywhere to open GM travel.");
            Debug.Log($"[GM] {auth.username} enabled GM mode.");
            return;
        }

        if (mode == "off" || mode == "0" || mode == "false")
        {
            auth.gmActive = false;
            auth.gmFlyEnabled = false;
            auth.gmFreeCameraEnabled = false;
            auth.gmSpeedMultiplier = 1f;
            chat.TargetSetGmMode(sender, false);
            chat.TargetSetGmFly(sender, false);
            chat.TargetSetGmSpeed(sender, 1f);
            chat.TargetSetGmFreeCamera(
                sender, false, auth.gmFreeCameraSpeed);
            chat.SendGmFeedback(sender, "GM mode OFF. Fly, free camera, and speed reset.");
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
            chat.SendGmFeedback(sender, "Usage: /arrive hub | darkwood | ashen | boneyard | pvp");
            return;
        }

        string key = NormalizeKey(parts[1]);
        if (!ArriveDestinations.TryGetValue(key, out ArriveDestination destination))
        {
            chat.SendGmFeedback(sender, "Unknown arrival. Use: hub, darkwood, ashen, boneyard, pvp.");
            return;
        }

        if (!CanLoadScene(destination.SceneName))
        {
            chat.SendGmFeedback(sender, $"{destination.DisplayName} is not in Build Settings.");
            return;
        }

        if (ZoneManager.Instance == null)
        {
            chat.SendGmFeedback(sender, "Zone system is not running — cannot travel.");
            return;
        }

        // ROADMAP 6.4: moves only this GM. ServerChangeScene used to drag every
        // connected player along. Same-zone /arrive works too — ZoneManager
        // repositions to the spawn point rather than reloading the scene.
        chat.SendGmFeedback(sender, $"Arriving at {destination.DisplayName}...");
        Debug.Log($"[GM] {auth.username} requested arrive: {destination.SceneName}/{destination.SpawnId}");
        ZoneManager.Instance.MovePlayerToZone(sender, destination.SceneName, destination.SpawnId);
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
        chat.SendGmFeedback(sender, enabled
            ? "Fly ON. WASD moves, Space rises, Ctrl/C descends, Shift boosts. Release the controls to stop."
            : "Fly OFF.");
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

    static void HandleFreeCamera(
        string[] parts,
        NetworkConnectionToClient sender,
        RodChatManager chat)
    {
        if (!RequireActiveGm(sender, chat, "freecam", out RodPlayerAuth auth))
            return;

        if (parts.Length >= 2 &&
            string.Equals(parts[1], "speed", StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length < 3 || !TryParseFreeCameraSpeed(parts[2], out float speed))
            {
                chat.SendGmFeedback(sender,
                    $"Usage: /freecam speed <{RodPlayerAuth.MinFreeCameraSpeed:0.##}-{RodPlayerAuth.MaxFreeCameraSpeed:0}>.");
                return;
            }

            auth.gmFreeCameraSpeed = speed;
            chat.TargetSetGmFreeCamera(
                sender, auth.gmFreeCameraEnabled, speed);
            chat.SendGmFeedback(sender, $"Free camera speed set to {speed:0.##}.");
            Debug.Log($"[GM] {auth.username} set free camera speed={speed:0.##}.");
            return;
        }

        bool enabled = !auth.gmFreeCameraEnabled;
        if (parts.Length >= 2)
        {
            if (!TryParseToggle(parts[1], out enabled))
            {
                chat.SendGmFeedback(sender,
                    "Usage: /freecam [on|off] [speed] or /freecam speed <value>.");
                return;
            }

            if (parts.Length >= 3)
            {
                if (!TryParseFreeCameraSpeed(parts[2], out float speed))
                {
                    chat.SendGmFeedback(sender,
                        $"Free camera speed must be {RodPlayerAuth.MinFreeCameraSpeed:0.##}-{RodPlayerAuth.MaxFreeCameraSpeed:0}.");
                    return;
                }

                auth.gmFreeCameraSpeed = speed;
            }
        }

        auth.gmFreeCameraEnabled = enabled;
        chat.TargetSetGmFreeCamera(
            sender, enabled, auth.gmFreeCameraSpeed);
        chat.SendGmFeedback(sender, enabled
            ? $"Free camera ON at speed {auth.gmFreeCameraSpeed:0.##}. UI hidden. WASD moves, Space/E rises, Ctrl/Q/C descends, RMB looks, Shift boosts, Alt slows, Escape exits."
            : "Free camera OFF. Camera returned to your character.");
        Debug.Log($"[GM] {auth.username} set free camera={enabled} speed={auth.gmFreeCameraSpeed:0.##}.");
    }

    static void SendHelp(NetworkConnectionToClient sender, RodChatManager chat)
    {
        chat.SendGmFeedback(sender,
            "GM commands: /gm on|off, /arrive hub|darkwood|ashen|boneyard|pvp, /fly [on|off], /speed <multiplier>, /freecam [on|off] [speed], /freecam speed <value>.");
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

    public static bool IsActiveGm(RodPlayerAuth auth)
    {
        return auth != null && auth.gmActive && IsGmAllowed(auth);
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

    static bool TryParseFreeCameraSpeed(string value, out float speed)
    {
        if (!float.TryParse(
                value, NumberStyles.Float, CultureInfo.InvariantCulture, out speed))
            return false;

        return !float.IsNaN(speed) && !float.IsInfinity(speed) &&
               speed >= RodPlayerAuth.MinFreeCameraSpeed &&
               speed <= RodPlayerAuth.MaxFreeCameraSpeed;
    }

    static string NormalizeKey(string value)
    {
        return value.Replace(" ", "").Replace("-", "").Replace("_", "").ToLowerInvariant();
    }

    static bool CanLoadScene(string sceneName)
    {
        // The old "…or it's the active scene" clause is gone with ROADMAP 6.4: the
        // active scene is now the empty container, never a zone, so it could only
        // ever have returned false. Build Settings membership is the real check.
        return !string.IsNullOrWhiteSpace(sceneName) &&
               Application.CanStreamedLevelBeLoaded(sceneName);
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
