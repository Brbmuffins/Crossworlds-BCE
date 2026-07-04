# AGENT_TASKS.md — Self-Contained Task Prompts

Each task below is a complete prompt for a fresh agent session (any model) with **zero
prior conversation**. Copy one task block in, let it run, review the diff. Strategy and
sizing live in `ROADMAP.md`; this file is the execution format. Tasks are ordered — do
not start one whose *Depends* is unmet. Update the checkbox + ROADMAP status when done.

**Rules that apply to every task** (repeated so tasks survive copy-paste):
- Repo: `D:\Crossworlds` (Unity 6000.4.10f1, Mirror/KCP). Read root `CLAUDE.md` first.
- Minimum change. No refactors beyond the task. If scope balloons, stop and report.
- Client-only code guards: `#if UNITY_EDITOR || !UNITY_SERVER` (editor scripts compile
  with UNITY_SERVER when the build target is Dedicated Server — plain `!UNITY_SERVER`
  breaks them).
- **Compile gate:** after C# changes run
  `powershell -ExecutionPolicy Bypass -File tools\build-server.ps1`
  (or, if LFS pointers block it, invoke Unity batchmode directly with
  `-executeMethod BuildScript.BuildDedicatedServer` and read `build\server-build.log`
  for `error CS`). Do not report done with compile errors.
- Commit in topical slices, message footer:
  `Co-Authored-By: <your model name> <noreply@anthropic.com>`
- You cannot push or `git lfs pull` (no GitHub auth in CLI) — never try; note it for
  the owner instead.

---

## ✅ T1 — Central server config (ROADMAP 2.1)

**Context:** The VPS IP `15.204.243.36` is hardcoded in ~8 scripts and serialized in
LoginScene. A server move requires many edits. Create one source of truth.

**Files:** new `Assets/Game/Systems/ServerConfig.cs`; edit
`Assets/Game/Networking/RodNetworkManager.cs`, `Assets/Game/Networking/RodNetworkAuthenticator.cs`,
`Assets/Game/UI/LoginManager.cs`, `Assets/Game/UI/CharacterSelectUI.cs`,
`Assets/Game/UI/CharacterSelectManager.cs`, `Assets/Game/Editor/RodProjectSettings.cs`,
`Assets/Game/Editor/RodEditorSetup.cs`. Find any others with
`grep -rn "15.204.243.36" Assets --include=*.cs`.

**Steps:** Static class `ServerConfig` with `public const string ServerIP = "15.204.243.36";`,
`public static string AuthBaseUrl => $"http://{ServerIP}:3000";` plus an optional
`PlayerPrefs("serverIP")` override getter. Replace hardcoded literals; keep existing
public Inspector fields but default them from ServerConfig in `Awake()`/field
initializers so serialized scene values still win if deliberately set.

**Accept:** grep shows the IP only in ServerConfig.cs (scene YAML serialized copies are
fine — note them for an editor cleanup); compile gate passes.
**Depends:** none. **Editor step for owner:** none.

## ✅ T2 — Scene-name constants (ROADMAP 2.2)

**Context:** Scene names/paths are string literals (`"Arena_Copper"`, full paths in
`RodNetworkManager.Awake`). A scene rename breaks portals silently.

**Files:** new `Assets/Game/Systems/SceneNames.cs`; edit
`Assets/Game/Networking/RodNetworkManager.cs`, `Assets/Game/Networking/PortalTransition.cs`,
`Assets/Game/Scene/HubReturnTrigger.cs`, `Assets/Game/Scene/ArenaPortalTrigger.cs`,
`Assets/Game/Editor/BuildScript.cs` (SCENES array).

**Steps:** Static class with `public const string Login/CharacterSelect/Hub/ArenaCopper`
(names) and `public const string LoginPath/HubPath/...` (full `Assets/...unity` paths).
Replace literals. Do NOT rename any scene.

**Accept:** no scene-name string literals left in the edited files; compile gate passes.
**Depends:** none.

## ✅ T3 — Arena scene generation, script side (ROADMAP 1.1)

**Context:** No arena scene exists; `PortalTransition.arenaSceneName` defaults to
`"Arena_Copper"`. An editor script `Assets/Game/Editor/ArenaSceneBuilder.cs` arrived in
the last merge — it may already build an arena.

**Steps:** Read `ArenaSceneBuilder.cs`. Ensure it: creates/saves
`Assets/Game/Scenes/Arena_Copper.unity`; adds ground with a baked-NavMesh-compatible
setup, 4+ spawn points, a `WaveSpawner` wired to `ArenaPortalTrigger` (calls
`StartWaves()` + `ArenaSessionController` if present), and a `HubReturnTrigger`; adds
the scene to `EditorBuildSettings.scenes` and to `BuildScript.SCENES`. Extend it
minimally if pieces are missing. Do not try to run it — it needs the editor.

**Accept:** compile gate passes; report the exact menu item the owner must click, in
order, including NavMesh bake (`Window ▸ AI ▸ Navigation ▸ Bake`) and a play-mode
checklist (portal → arena → wave 1 spawns → return to hub).
**Depends:** T2 recommended first (scene constants). **Editor step for owner:** run the
builder menu item, bake NavMesh, verify in play mode.

## ✅ T4 — API contract smoke tests (ROADMAP 2.3)

**Context:** All client JSON parsing is `JsonUtility` on ad-hoc classes; float-format
and NaN bugs have burned time. Catch server/response drift before players do.

**Files:** new `Assets/Game/Editor/ApiContractTests.cs` (editor-only, `MenuItem`).

**Steps:** Menu command `BCE/Diagnostics/API Contract Check` that synchronously (or via
`EditorCoroutine`-free polling of `UnityWebRequest`) hits `GET /api/health`,
`GET /api/items`, `GET /api/enemies` on `ServerConfig.AuthBaseUrl` (T1) and asserts:
HTTP 200, `success == true` where applicable, and at least one parsed row with
non-default fields. Log PASS/FAIL per endpoint to the console.

**Accept:** compile gate passes; owner runs the menu item against the live VPS and all
report PASS. **Depends:** T1.

## ☐ T5 — Progress-save sanity caps (ROADMAP 4.2 — VPS session, not this repo)

**Context:** `POST /api/character/save-progress` trusts client values (level/xp/gold).
Server-side validation only; no schema change. Work happens over SSH on the VPS in
`/opt/crossworlds-auth/server.js` — read `_CONTEXT/CLAUDE.md` conventions first
(transactions, parameterized SQL, `{success,error}` shape, log prefixes).

**Steps:** In the save-progress handler, load the current row; reject with 400 +
player-readable error when: level increases by >2 per call, xp jumps beyond
level-curve max for the new level, or gold delta exceeds a sane cap (e.g. 10,000)
without matching `gold_transactions` entries. Log rejects with `[PROGRESS]`.
Smoke test with curl (normal save passes, absurd values 400), restart service,
check journalctl.

**Accept:** curl evidence of both cases; normal Unity play unaffected.
**Depends:** SSH access (owner must be present or provide it).

## ☐ T6 — Owner/editor checklist (no agent — human steps)

1. GitHub Desktop: **pull then push** `main` (local is 23+ ahead), which also fixes LFS.
2. Interactive shell: `git lfs pull` → then rerun `tools\build-server.ps1` → real
   deploy artifact → `scp` + `sudo bash deploy-server.sh` (see CLAUDE.md Build & deploy).
3. Unity editor: assign `networkedPrefabs` on the NetworkManager in Hub
   (Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem, boss prefab).
4. Rotate the leaked MySQL/dashboard passwords on the VPS (ROADMAP Q7).
5. Answer the ⚠ DECISION questions at the bottom of ROADMAP.md (Brandalf, mastery
   endpoints, server-side player HP, dead scene/script deletions).

---

**Blocked / decision-gated (do not start):** arena content beyond Arena_Copper,
Brandalf sixth-hero wiring, server-side player HP (4.1), arena session tokens (4.3),
anything marked ⚠ in ROADMAP.md.
