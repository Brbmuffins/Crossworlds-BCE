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
    static readonly string[] SCENES =
    {
        "Assets/Game/Scenes/LoginScene.unity",
        "Assets/Game/Scenes/CharacterSelect.unity",
        "Assets/Game/Scenes/Hub.unity",
    };

    // ── Dedicated Server (Linux) ─────────────────────────────────────────

    [MenuItem("RoD/Build/Dedicated Server (Linux)")]
    public static void BuildDedicatedServer()
    {
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes              = SCENES,
            locationPathName    = "build/DedicatedServer/RateOfDecayOnline.x86_64",
            target              = BuildTarget.StandaloneLinux64,
            subtarget           = (int)StandaloneBuildSubtarget.Server,
            options             = BuildOptions.None,
        });

        bool ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log(ok
            ? $"[RoD] ✅ Server build OK ({report.summary.totalSize / 1_048_576} MB)"
            : $"[RoD] ❌ Server build FAILED — {report.summary.totalErrors} error(s)");

        if (Application.isBatchMode)
            EditorApplication.Exit(ok ? 0 : 1);
    }

    // ── Windows Client ───────────────────────────────────────────────────

    [MenuItem("RoD/Build/Windows Client")]
    public static void BuildWindowsClient()
    {
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes              = SCENES,
            locationPathName    = "build/WindowsClient/RateOfDecayOnline.exe",
            target              = BuildTarget.StandaloneWindows64,
            options             = BuildOptions.None,
        });

        bool ok = report.summary.result == BuildResult.Succeeded;
        Debug.Log(ok
            ? $"[RoD] ✅ Client build OK ({report.summary.totalSize / 1_048_576} MB)"
            : $"[RoD] ❌ Client build FAILED — {report.summary.totalErrors} error(s)");

        if (Application.isBatchMode)
            EditorApplication.Exit(ok ? 0 : 1);
    }
}
