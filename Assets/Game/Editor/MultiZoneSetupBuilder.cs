using System.Collections.Generic;
using System.IO;
using Mirror;                       // InterestManagementBase — the 6.6 conflict check
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// ═══════════════════════════════════════════════════════════════════════════
//  MultiZoneSetupBuilder — BCE ▶ Setup ▶ 6z
//
//  One-time editor setup for the multi-zone world (ROADMAP 6.3):
//    1. Creates Assets/Game/Scenes/_Container.unity — an empty scene that holds
//       nothing. It is what RodNetworkManager.onlineScene points at; every zone
//       (Hub included) loads additively on top of it at runtime.
//    2. Registers _Container and every zone in SceneNames.Zones into Build
//       Settings. LoadSceneAsync fails silently for scenes that are not there,
//       which shows up as "could not acquire zone" at runtime.
//    3. Adds the ZoneManager component to the RodNetworkManager GameObject.
//
//  Safe to re-run: it creates only what is missing.
// ═══════════════════════════════════════════════════════════════════════════

public static class MultiZoneSetupBuilder
{
    const string ContainerPath = SceneNames.ContainerPath;

    [MenuItem("BCE/Setup/6z - Multi-Zone (Container Scene + ZoneManager)")]
    public static void Run()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[BCE 6z] Cancelled — unsaved scenes were not saved.");
            return;
        }

        var report = new List<string>();

        CreateContainerSceneIfMissing(report);
        RegisterScenesInBuildSettings(report);
        AddZoneManagerToNetworkManager(report);
        MarkChatManagerGlobal(report);

        string summary = string.Join("\n", report);
        Debug.Log("[BCE 6z] Multi-zone setup:\n" + summary);
        EditorUtility.DisplayDialog("BCE ▶ 6z Multi-Zone Setup", summary, "OK");
    }

    // ── 1. Container scene ────────────────────────────────────────────────────

    static void CreateContainerSceneIfMissing(List<string> report)
    {
        if (File.Exists(ContainerPath))
        {
            report.Add($"✓ Container scene already exists: {ContainerPath}");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(ContainerPath));

        // Empty, not DefaultGameObjects: the container must hold nothing. A stray
        // camera or light here would render on top of every zone.
        Scene container = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        bool saved = EditorSceneManager.SaveScene(container, ContainerPath);

        report.Add(saved
            ? $"✓ Created empty container scene: {ContainerPath}"
            : $"✗ FAILED to save container scene to {ContainerPath}");

        AssetDatabase.Refresh();
    }

    // ── 2. Build Settings ─────────────────────────────────────────────────────

    static void RegisterScenesInBuildSettings(List<string> report)
    {
        var wanted = new List<string> { ContainerPath, SceneNames.LoginPath, SceneNames.CharacterSelectPath };

        foreach (string zone in SceneNames.Zones)
        {
            string path = PathForZone(zone);
            if (path != null) wanted.Add(path);
        }

        List<EditorBuildSettingsScene> current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var present = new HashSet<string>();
        foreach (var s in current) present.Add(s.path);

        int added = 0;
        foreach (string path in wanted)
        {
            if (present.Contains(path)) continue;

            if (!File.Exists(path))
            {
                report.Add($"⚠ Scene file missing, not added to Build Settings: {path}");
                continue;
            }

            current.Add(new EditorBuildSettingsScene(path, true));
            present.Add(path);
            added++;
        }

        if (added > 0)
        {
            EditorBuildSettings.scenes = current.ToArray();
            report.Add($"✓ Added {added} scene(s) to Build Settings.");
        }
        else
        {
            report.Add("✓ Build Settings already contained every zone.");
        }
    }

    static string PathForZone(string zoneName)
    {
        switch (zoneName)
        {
            case SceneNames.Hub:             return SceneNames.HubPath;
            case SceneNames.Darkwood:        return SceneNames.DarkwoodPath;
            case SceneNames.ToujamBasin:     return SceneNames.ToujamBasinPath;
            case SceneNames.AshenWastelands: return SceneNames.AshenWastelandsPath;
            case SceneNames.GMIsland:        return SceneNames.GMIslandPath;
            case SceneNames.VoidDungeon:     return SceneNames.VoidDungeonPath;
            case SceneNames.GatheringZone:   return SceneNames.GatheringZonePath;
            case SceneNames.ArenaCopper:     return SceneNames.ArenaCopperPath;
            default:                         return null;
        }
    }

    // ── 3. ZoneManager component ──────────────────────────────────────────────

    static void AddZoneManagerToNetworkManager(List<string> report)
    {
        if (!File.Exists(SceneNames.LoginPath))
        {
            report.Add($"✗ {SceneNames.LoginPath} not found — cannot add ZoneManager.");
            return;
        }

        Scene login = EditorSceneManager.OpenScene(SceneNames.LoginPath, OpenSceneMode.Single);

        RodNetworkManager nm = Object.FindFirstObjectByType<RodNetworkManager>();
        if (nm == null)
        {
            report.Add("✗ No RodNetworkManager in LoginScene — run BCE ▶ Setup ▶ 4 first.");
            return;
        }

        bool changed = false;

        if (nm.GetComponent<ZoneManager>() == null)
        {
            Undo.AddComponent<ZoneManager>(nm.gameObject);
            report.Add($"✓ Added ZoneManager to '{nm.gameObject.name}'.");
            changed = true;
        }
        else
        {
            report.Add("✓ ZoneManager already on the RodNetworkManager GameObject.");
        }

        // ROADMAP 6.6 — interest management. Only ONE may exist per NetworkManager, so
        // strip any stock Mirror one first; ours subclasses SceneDistance and adds the
        // world-global exemption that keeps chat alive.
        foreach (InterestManagementBase existing in nm.GetComponents<InterestManagementBase>())
        {
            if (existing is CrossworldsInterestManagement) continue;

            report.Add($"⚠ Removed conflicting {existing.GetType().Name} — only one is allowed.");
            Undo.DestroyObjectImmediate(existing);
            changed = true;
        }

        if (nm.GetComponent<CrossworldsInterestManagement>() == null)
        {
            Undo.AddComponent<CrossworldsInterestManagement>(nm.gameObject);
            report.Add("✓ Added CrossworldsInterestManagement (scene + distance scoping).");
            changed = true;
        }
        else
        {
            report.Add("✓ CrossworldsInterestManagement already present.");
        }

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(login);
            EditorSceneManager.SaveScene(login);
            report.Add("✓ Saved LoginScene.");
        }
    }

    // ── 4. ChatManager must survive interest management ───────────────────────

    static void MarkChatManagerGlobal(List<string> report)
    {
        RodNetworkManager nm = Object.FindFirstObjectByType<RodNetworkManager>();
        GameObject prefab = nm != null ? nm.chatManagerPrefab : null;

        if (prefab == null)
        {
            report.Add("⚠ chatManagerPrefab not assigned — cannot mark it world-global. " +
                       "Run BCE ▶ Setup ▶ 4p, then re-run 6z, or CHAT WILL BE SILENT once " +
                       "interest management is active.");
            return;
        }

        if (prefab.GetComponent<GlobalNetworkObject>() != null)
        {
            report.Add("✓ ChatManager prefab already marked world-global.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(prefab);
        GameObject root = PrefabUtility.LoadPrefabContents(path);

        var marker = root.AddComponent<GlobalNetworkObject>();
        marker.rationale = "Chat is global across all zones (ROADMAP open question 9). " +
                           "Lives in the DontDestroyOnLoad scene, which matches no player's " +
                           "scene, so without this it would have zero observers.";

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);

        report.Add($"✓ Marked ChatManager prefab world-global: {path}");
    }
}
