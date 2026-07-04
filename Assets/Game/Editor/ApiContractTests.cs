#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

// ═══════════════════════════════════════════════════════════════════════════
//  ApiContractTests — BCE/Diagnostics/API Contract Check
//
//  Hits the live VPS endpoints from the editor and reports PASS/FAIL per
//  endpoint to the Console. Requires an active internet connection and the
//  VPS to be running. No JWT needed — only public/game-status endpoints.
//
//  Run: BCE → Diagnostics → API Contract Check
// ═══════════════════════════════════════════════════════════════════════════
public static class ApiContractTests
{
    [MenuItem("BCE/Diagnostics/API Contract Check")]
    public static void RunAll()
    {
        Debug.Log($"[ApiContract] Checking {ServerConfig.AuthBaseUrl} ...");

        var results = new List<(string label, bool pass, string detail)>();

        results.Add(CheckHealth());
        results.Add(CheckItems());
        results.Add(CheckEnemies());

        var sb = new StringBuilder();
        sb.AppendLine("[ApiContract] ── Results ───────────────────────────────");
        int passed = 0;
        foreach (var (label, pass, detail) in results)
        {
            string marker = pass ? "✅ PASS" : "❌ FAIL";
            sb.AppendLine($"  {marker}  {label}  {detail}");
            if (pass) passed++;
        }
        sb.AppendLine($"[ApiContract] {passed}/{results.Count} endpoints passed.");

        if (passed == results.Count)
            Debug.Log(sb.ToString());
        else
            Debug.LogError(sb.ToString());
    }

    // ── Endpoint checks ───────────────────────────────────────────────────────

    static (string, bool, string) CheckHealth()
    {
        const string label = "GET /api/health";
        try
        {
            using var req = UnityWebRequest.Get($"{ServerConfig.AuthBaseUrl}/api/health");
            req.timeout = 8;
            var op = req.SendWebRequest();
            while (!op.isDone) { } // spin — editor context, acceptable

            if (req.result != UnityWebRequest.Result.Success)
                return (label, false, $"HTTP error: {req.error}");

            int code = (int)req.responseCode;
            if (code != 200)
                return (label, false, $"Expected 200 got {code}");

            var body = req.downloadHandler.text;
            bool hasSuccess = body.Contains("\"success\"") || body.Contains("\"status\"");
            if (!hasSuccess)
                return (label, false, $"No success/status key in response: {Truncate(body)}");

            return (label, true, $"({code})");
        }
        catch (Exception e)
        {
            return (label, false, $"Exception: {e.Message}");
        }
    }

    static (string, bool, string) CheckItems()
    {
        const string label = "GET /api/items";
        try
        {
            using var req = UnityWebRequest.Get($"{ServerConfig.AuthBaseUrl}/api/items");
            req.timeout = 8;
            var op = req.SendWebRequest();
            while (!op.isDone) { }

            if (req.result != UnityWebRequest.Result.Success)
                return (label, false, $"HTTP error: {req.error}");

            int code = (int)req.responseCode;
            if (code != 200)
                return (label, false, $"Expected 200 got {code}");

            var body = req.downloadHandler.text;
            // Expect a non-empty JSON array or object with items
            if (body.Length < 5 || (body.TrimStart()[0] != '[' && !body.Contains("\"items\"")))
                return (label, false, $"Unexpected response shape: {Truncate(body)}");

            return (label, true, $"({code}, {body.Length} bytes)");
        }
        catch (Exception e)
        {
            return (label, false, $"Exception: {e.Message}");
        }
    }

    static (string, bool, string) CheckEnemies()
    {
        const string label = "GET /api/enemies";
        try
        {
            using var req = UnityWebRequest.Get($"{ServerConfig.AuthBaseUrl}/api/enemies");
            req.timeout = 8;
            var op = req.SendWebRequest();
            while (!op.isDone) { }

            if (req.result != UnityWebRequest.Result.Success)
                return (label, false, $"HTTP error: {req.error}");

            int code = (int)req.responseCode;
            if (code != 200)
                return (label, false, $"Expected 200 got {code}");

            var body = req.downloadHandler.text;
            if (body.Length < 5 || (body.TrimStart()[0] != '[' && !body.Contains("\"enemies\"")))
                return (label, false, $"Unexpected response shape: {Truncate(body)}");

            return (label, true, $"({code}, {body.Length} bytes)");
        }
        catch (Exception e)
        {
            return (label, false, $"Exception: {e.Message}");
        }
    }

    static string Truncate(string s, int max = 120) =>
        s.Length <= max ? s : s.Substring(0, max) + "…";
}
#endif
