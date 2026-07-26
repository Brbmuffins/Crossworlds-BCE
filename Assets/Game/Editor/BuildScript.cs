using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  BuildScript
//  Called by GitHub Actions via -executeMethod, and available as menu items.
//
//  CI usage:
//    unity-builder → buildMethod: BuildScript.BuildDedicatedServer
//    unity-builder → buildMethod: BuildScript.BuildWindowsClient
// ═══════════════════════════════════════════════════════════════════════════

public static class BuildScript
{
    // Pull the enabled Scenes-In-Build list (the Linux Server profile uses the global
    // list, OverrideGlobalSceneList=0) rather than a hardcoded array — so scenes added
    // later (_Container onlineScene, extra zones) never silently drop out of the build.
    // Scene 0 stays LoginScene as long as it's first in Build Settings.
    static string[] SCENES =>
        EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

    // ── Dedicated Server (Linux) ─────────────────────────────────────────

    [MenuItem("BCE/Build/Dedicated Server (Linux)")]
    public static void BuildDedicatedServer()
    {
        bool prevEnable = Unity.Multiplayer.Editor.EditorMultiplayerRolesManager.EnableMultiplayerRoles;
        BuildReport report;
        try
        {
            Unity.Multiplayer.Editor.EditorMultiplayerRolesManager.EnableMultiplayerRoles = false;
            report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes              = SCENES,
                // Must match the live systemd unit's ExecStart binary name exactly:
                // /game/<runid>/CrossWords.x86_64 (and CrossWords_Data). Do NOT rename
                // downstream — the pack/deploy steps and the VPS unit all expect this.
                locationPathName    = "build/DedicatedServer/CrossWords.x86_64",
                target              = BuildTarget.StandaloneLinux64,
                subtarget           = (int)StandaloneBuildSubtarget.Server,
                options             = BuildOptions.None,
            });
        }
        finally
        {
            Unity.Multiplayer.Editor.EditorMultiplayerRolesManager.EnableMultiplayerRoles = prevEnable;
        }

        bool ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log(ok
            ? $"[BCE] ✅ Server build OK ({report.summary.totalSize / 1_048_576} MB)"
            : $"[BCE] ❌ Server build FAILED — {report.summary.totalErrors} error(s)");

        if (Application.isBatchMode)
            EditorApplication.Exit(ok ? 0 : 1);
    }

    // ── Windows Client ───────────────────────────────────────────────────

    [MenuItem("BCE/Build/Windows Client")]
    public static void BuildWindowsClient()
    {
        bool prevEnable = Unity.Multiplayer.Editor.EditorMultiplayerRolesManager.EnableMultiplayerRoles;
        BuildReport report;
        try
        {
            Unity.Multiplayer.Editor.EditorMultiplayerRolesManager.EnableMultiplayerRoles = false;
            report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes              = SCENES,
                locationPathName    = "build/WindowsClient/Crossworlds.exe",
                target              = BuildTarget.StandaloneWindows64,
                subtarget           = (int)StandaloneBuildSubtarget.Player,
                options             = BuildOptions.None,
            });
        }
        finally
        {
            Unity.Multiplayer.Editor.EditorMultiplayerRolesManager.EnableMultiplayerRoles = prevEnable;
        }

        bool ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log(ok
            ? $"[BCE] ✅ Client build OK ({report.summary.totalSize / 1_048_576} MB)"
            : $"[BCE] ❌ Client build FAILED — {report.summary.totalErrors} error(s)");

        if (Application.isBatchMode)
            EditorApplication.Exit(ok ? 0 : 1);
    }
}
