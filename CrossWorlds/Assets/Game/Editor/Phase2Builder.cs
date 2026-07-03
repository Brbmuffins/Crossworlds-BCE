// COPY TO: Assets/Game/Editor/Phase2Builder.cs
#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Phase2Builder — BCE menu tools for setting up Phase 2 systems in the editor.
///
/// Menu: BCE/Setup/
///   7 ► Verify Phase 2 API Endpoints
///   8 ► Tag PlayerNameplate for Guild Tag
///   9 ► List Phase 2 Scripts Status
/// </summary>
public static class Phase2Builder
{
    const string BASE_URL = "http://15.204.243.36:3000";

    // ─────────────────────────────────────────────────────────────────────────
    // Menu item 7 — API health check for all Phase 2 endpoints
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("BCE/Setup/7 ▶ Verify Phase 2 API Endpoints")]
    static void VerifyPhase2Api()
    {
        // Run the check as an editor coroutine via EditorApplication.update
        var checker = new ApiChecker();
        checker.Start();
    }

    class ApiChecker
    {
        static readonly (string method, string path, string body)[] Endpoints =
        {
            ("GET",  "/api/health",                               null),
            ("GET",  "/api/quests/available/1",                   null),
            ("GET",  "/api/quests/active/1",                      null),
            ("GET",  "/api/quests/completed/1",                   null),
            ("GET",  "/api/guilds/my/1",                          null),
            ("GET",  "/api/guilds/search?name=test",              null),
            ("GET",  "/api/talents/1",                            null),
            ("POST", "/api/talents/invest",  "{\"characterId\":1,\"talentId\":1}"),
        };

        int _index = 0;
        UnityWebRequest _req;
        System.Collections.Generic.List<string> _results = new();

        public void Start()
        {
            Debug.Log($"[Phase2Builder] Checking {Endpoints.Length} Phase 2 endpoints against {BASE_URL}...");
            EditorApplication.update += Tick;
        }

        void Tick()
        {
            if (_req == null)
            {
                if (_index >= Endpoints.Length)
                {
                    // Done
                    EditorApplication.update -= Tick;
                    string report = string.Join("\n", _results);
                    Debug.Log($"[Phase2Builder] Phase 2 API Report:\n{report}");
                    EditorUtility.DisplayDialog("Phase 2 API Check", report, "OK");
                    return;
                }

                var (method, path, body) = Endpoints[_index];
                string url = BASE_URL + path;

                if (method == "GET")
                {
                    _req = UnityWebRequest.Get(url);
                }
                else
                {
                    _req = new UnityWebRequest(url, "POST");
                    if (body != null)
                    {
                        _req.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
                        _req.downloadHandler = new DownloadHandlerBuffer();
                        _req.SetRequestHeader("Content-Type", "application/json");
                    }
                }

                // No real JWT — we just want to see if endpoint exists (200 or 401, not 404)
                _req.SendWebRequest();
            }

            if (!_req.isDone) return;

            var (m, p, _) = Endpoints[_index];
            int code = (int)_req.responseCode;
            string status;

            if (_req.result == UnityWebRequest.Result.ConnectionError)
                status = "❌ UNREACHABLE";
            else if (code == 404)
                status = "❌ NOT FOUND (404)";
            else if (code == 401 || code == 200)
                status = $"✓  {code}";
            else
                status = $"?  {code}";

            _results.Add($"{m,-4} {p,-45} → {status}");
            _req.Dispose();
            _req = null;
            _index++;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Menu item 8 — Ensure selected PlayerNameplate has a guild tag label slot
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("BCE/Setup/8 ▶ Add Guild Tag to Selected PlayerNameplate")]
    static void AddGuildTagToNameplate()
    {
        var sel = Selection.activeGameObject;
        if (sel == null)
        {
            EditorUtility.DisplayDialog("No selection", "Select a PlayerNameplate prefab or GameObject.", "OK");
            return;
        }

        // Check if GuildTagLabel already exists
        var existing = sel.transform.Find("GuildTagLabel");
        if (existing != null)
        {
            Debug.Log("[Phase2Builder] GuildTagLabel already exists on this nameplate.");
            return;
        }

        var go = new GameObject("GuildTagLabel");
        go.transform.SetParent(sel.transform, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, 22f); // above the name
        rect.sizeDelta = new Vector2(80f, 14f);

        var txt = go.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = "[GUILD]";
        txt.fontSize = 8f;
        txt.color = new Color(0.9f, 0.8f, 0.2f);
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.fontStyle = TMPro.FontStyles.Bold;

        Undo.RegisterCreatedObjectUndo(go, "Add GuildTagLabel");
        EditorUtility.SetDirty(sel);
        Debug.Log($"[Phase2Builder] Added GuildTagLabel to {sel.name}. Wire it up in PlayerNameplate.cs with a [GuildTag] field.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Menu item 9 — List all Phase 2 scripts and whether they're in Assets/
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("BCE/Setup/9 ▶ Phase 2 Script Status")]
    static void ListPhase2ScriptStatus()
    {
        var scripts = new (string name, string expectedPath)[]
        {
            ("TalentModifierApplier", "Assets/Game/Characters/Scripts/TalentModifierApplier.cs"),
            ("TalentTreeUI",          "Assets/Game/UI/TalentTreeUI.cs"),
            ("QuestTracker",          "Assets/Game/UI/QuestTracker.cs"),
            ("QuestLogUI",            "Assets/Game/UI/QuestLogUI.cs"),
            ("GuildPanelUI",          "Assets/Game/UI/GuildPanelUI.cs"),
            ("Phase2Builder",         "Assets/Game/Editor/Phase2Builder.cs"),
            ("WorldBossController",   "Assets/Game/Combat/WorldBossController.cs"),
            ("WorldBossHealthBar",    "Assets/Game/UI/WorldBossHealthBar.cs"),
            ("WorldBossBuilder",      "Assets/Game/Editor/WorldBossBuilder.cs"),
        };

        var lines = new System.Collections.Generic.List<string>();
        lines.Add("Phase 2 Script Status\n");

        foreach (var (name, path) in scripts)
        {
            bool exists = System.IO.File.Exists(System.IO.Path.Combine(
                Application.dataPath.Replace("Assets", ""), path));
            string status = exists ? "✓  FOUND" : "✗  MISSING — copy from CrossWorlds/_scripts/";
            lines.Add($"{status}\n  {path}");
        }

        string report = string.Join("\n", lines);
        Debug.Log($"[Phase2Builder]\n{report}");
        EditorUtility.DisplayDialog("Phase 2 Script Status", report, "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Menu item: Open staging folder in Explorer
    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("BCE/Setup/Open _scripts Staging Folder")]
    static void OpenStagingFolder()
    {
        // The _scripts folder sits next to Assets/
        string path = Application.dataPath.Replace("Assets", "") + "../CrossWorlds/_scripts/";
        path = System.IO.Path.GetFullPath(path);
        if (System.IO.Directory.Exists(path))
            EditorUtility.RevealInFinder(path);
        else
            EditorUtility.DisplayDialog("Not Found",
                $"Staging folder not found at:\n{path}\n\nExpected next to your Unity project.", "OK");
    }
}
#endif
