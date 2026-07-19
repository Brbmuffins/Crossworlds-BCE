#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mirror;

/// <summary>
/// BCE/Setup/10 — Gathering Zone Builder
///
/// Creates Assets/Game/Scenes/Gathering Zone.unity and populates it with:
///
///   • Ground plane (tagged Ground, ColliderLayer Default)
///   • Directional light
///   • 4 NetworkStartPositions near the entrance portal
///   • 3 AfkGatheringStations (Timber Stand / Crystal Spring / Ore Deposit)
///   • 1 ForgeNPC crafting bench (interactive crafting, not AFK)
///   • 1 HubReturnTrigger portal back to Hub (NetworkIdentity required)
///
/// After running:
///   1. Window → AI → Navigation → Bake (NavMesh for NPC/player movement)
///   2. Ctrl+S (Mirror bakes sceneIds on save)
///   3. File → Build Settings → Add Open Scenes (add "Gathering Zone")
///   4. Also patches the HUB scene WaypointMapTrigger so the gathering node
///      shows the correct sceneName and unlocked=true at runtime.
///   5. Rebuild + redeploy server
///
/// Also patches HUB scene WaypointMapTrigger serialized data (additive open/save).
/// </summary>
public static class GatheringZoneBuilder
{
    const string ScenePath  = SceneNames.GatheringZonePath;
    const string SceneName  = SceneNames.GatheringZone;
    const string HubPath    = SceneNames.HubPath;

    [MenuItem("BCE/Setup/10 ▶ Gathering Zone — Build Scene", priority = 52)]
    static void Run()
    {
        var report = new StringBuilder();
        report.AppendLine("── Gathering Zone Builder ────────────────────────────────");

        // ── 1. Create or reopen the scene ────────────────────────────────────────
        bool existed = System.IO.File.Exists(
            System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), ScenePath));

        var scene = existed
            ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        if (!scene.IsValid())
        {
            EditorUtility.DisplayDialog("Gathering Zone Builder",
                $"Failed to open/create scene at {ScenePath}.", "OK");
            return;
        }

        report.AppendLine(existed ? "  ✓ Reopened existing scene" : "  ✓ Created new scene");

        Vector3 center = Vector3.zero;

        // ── 2. Directional light ─────────────────────────────────────────────────
        if (GameObject.FindFirstObjectByType<Light>() == null)
        {
            var lightGO = new GameObject("Directional Light");
            var light   = lightGO.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.intensity = 1.2f;
            light.color     = new Color(1f, 0.96f, 0.84f);
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            report.AppendLine("  ✓ Directional Light");
        }

        // ── 3. Ground plane ──────────────────────────────────────────────────────
        if (GameObject.Find("Ground") == null)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.tag  = "Ground";
            ground.transform.position   = center;
            ground.transform.localScale = new Vector3(8f, 1f, 8f);   // 80m × 80m
            // Material tint — gentle forest green
            var rend = ground.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sharedMaterial = new Material(Shader.Find("Standard"))
                {
                    color = new Color(0.35f, 0.55f, 0.28f)
                };
            }
            report.AppendLine("  ✓ Ground plane (80m × 80m, tagged Ground)");
        }

        // ── 4. Player spawn points ───────────────────────────────────────────────
        var spawnParent = GetOrCreate("PlayerSpawnPoints", null);
        Vector3 entrance = center + new Vector3(0f, 0.1f, -30f);
        if (spawnParent.transform.childCount == 0)
        {
            Vector3[] spawns = {
                entrance + new Vector3( 2f, 0f,  1f),
                entrance + new Vector3(-2f, 0f,  1f),
                entrance + new Vector3( 2f, 0f, -1f),
                entrance + new Vector3(-2f, 0f, -1f),
            };
            foreach (var pos in spawns)
            {
                var sp = new GameObject("NetworkStartPosition");
                sp.transform.SetParent(spawnParent.transform);
                sp.transform.position = pos;
                sp.AddComponent<NetworkStartPosition>();
            }
            report.AppendLine("  ✓ 4 NetworkStartPositions near entrance");
        }

        // ── 5. Hub return portal ─────────────────────────────────────────────────
        if (GameObject.Find("HubReturnPortal") == null)
        {
            var portalGO = new GameObject("HubReturnPortal");
            portalGO.transform.position = entrance + new Vector3(0f, 0f, -3f);

            portalGO.AddComponent<NetworkIdentity>();

            var trigger  = portalGO.AddComponent<HubReturnTrigger>();
            trigger.saveProgressBeforeReturn = true;
            trigger.runArenaCleanup          = false;
            trigger.promptText               = "Return to Hub";

            // Visual marker — small cyan sphere
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "PortalMarker";
            marker.transform.SetParent(portalGO.transform);
            marker.transform.localPosition = new Vector3(0f, 1f, 0f);
            marker.transform.localScale    = new Vector3(0.8f, 0.8f, 0.8f);
            var mr = marker.GetComponent<Renderer>();
            if (mr != null)
            {
                mr.sharedMaterial = new Material(Shader.Find("Standard"));
                mr.sharedMaterial.color = new Color(0.3f, 0.8f, 1f);
                mr.sharedMaterial.SetFloat("_Metallic", 0.8f);
                mr.sharedMaterial.SetFloat("_Glossiness", 0.9f);
            }

            report.AppendLine("  ✓ HubReturnTrigger portal at entrance");
        }

        // ── 6. AFK Gathering Stations ────────────────────────────────────────────
        // Arranged in a triangle around the center
        var stationDefs = new[]
        {
            new { name = "Timber Stand",    profId = 0, itemId = "material_wood",   xp = 12, tick = 6f,  pos = center + new Vector3(-15f, 0f,  10f) },
            new { name = "Crystal Spring",  profId = 1, itemId = "fish_raw",         xp = 10, tick = 7f,  pos = center + new Vector3( 15f, 0f,  10f) },
            new { name = "Ore Deposit",     profId = 2, itemId = "ore_copper",       xp = 15, tick = 8f,  pos = center + new Vector3(  0f, 0f,  25f) },
        };

        int stationsAdded = 0;
        foreach (var def in stationDefs)
        {
            if (GameObject.Find(def.name) != null) continue;

            var go = new GameObject(def.name);
            go.transform.position = def.pos;

            var station               = go.AddComponent<AfkGatheringStation>();
            station.stationName       = def.name;
            station.professionId      = def.profId;
            station.itemId            = def.itemId;
            station.itemQuantity      = 1;
            station.tickInterval      = def.tick;
            station.xpPerTick         = def.xp;
            station.bonusYieldLevel   = 10;
            station.interactRange     = 3.5f;
            station.cancelRadius      = 5f;
            station.minLevelRequired  = 1;

            // Visual marker — colour-coded sphere
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "StationMarker";
            marker.transform.SetParent(go.transform);
            marker.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            marker.transform.localScale    = new Vector3(1.2f, 1.2f, 1.2f);
            var rend = marker.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = def.profId == 0 ? new Color(0.35f, 0.72f, 0.18f)   // wood — green
                        : def.profId == 1 ? new Color(0.18f, 0.55f, 0.85f)   // fish — blue
                                          : new Color(0.65f, 0.45f, 0.22f);  // ore  — brown
                rend.sharedMaterial       = new Material(Shader.Find("Standard")) { color = c };
            }

            // Trigger collider for interaction range detection
            var col = go.AddComponent<SphereCollider>();
            col.radius    = station.interactRange;
            col.isTrigger = true;

            stationsAdded++;
        }
        report.AppendLine($"  ✓ {stationsAdded} AfkGatheringStation(s) placed");

        // ── 7. Forge NPC (crafting bench) ────────────────────────────────────────
        if (GameObject.Find("ForgeStation") == null)
        {
            var forgeGO = new GameObject("ForgeStation");
            forgeGO.transform.position = center + new Vector3(0f, 0f, 5f);

            var forge            = forgeGO.AddComponent<ForgeNPC>();
            forge.npcName        = "Crafting Bench";
            forge.professionId   = 0;   // default — panel shows all recipes
            forge.interactRange  = 4f;

            // Visual — anvil-ish cube
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "ForgeBlock";
            block.transform.SetParent(forgeGO.transform);
            block.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            block.transform.localScale    = new Vector3(1.2f, 0.8f, 0.8f);
            var rend = block.GetComponent<Renderer>();
            if (rend != null)
                rend.sharedMaterial = new Material(Shader.Find("Standard"))
                {
                    color = new Color(0.25f, 0.25f, 0.25f)
                };

            report.AppendLine("  ✓ ForgeNPC (Crafting Bench) at center");
        }

        // ── 8. Save scene ────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
        report.AppendLine(saved ? $"  ✓ Saved → {ScenePath}" : $"  ✗ Save FAILED → {ScenePath}");

        // ── 9. Patch HUB scene WaypointMapTrigger ───────────────────────────────
        PatchHubWaypointMap(report);

        // ── 10. Report ───────────────────────────────────────────────────────────
        report.AppendLine();
        report.AppendLine("REQUIRED EDITOR STEPS:");
        report.AppendLine("  1. Window → AI → Navigation → Bake (give players + NPCs walkable floor)");
        report.AppendLine("  2. Ctrl+S (Mirror sceneId bake)");
        report.AppendLine("  3. File → Build Settings → Add Open Scenes");
        report.AppendLine("     • \"Gathering Zone\" must appear in the list");
        report.AppendLine("  4. Rebuild + redeploy server so it knows the scene");
        report.AppendLine();
        report.AppendLine("OPTIONAL: swap the coloured sphere markers for real");
        report.AppendLine("          art assets in the Inspector once they're available.");

        EditorUtility.DisplayDialog("Gathering Zone Builder", report.ToString(), "Done");
    }

    // ── Patch HUB WaypointMapTrigger in its scene ─────────────────────────────────

    static void PatchHubWaypointMap(StringBuilder r)
    {
        if (!System.IO.File.Exists(
                System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), HubPath)))
        {
            r.AppendLine("  ⚠ HUB scene not found — open it manually and set gathering node sceneName/unlocked");
            return;
        }

        // Open HUB additively so we don't abandon the gathering zone
        var hubScene = EditorSceneManager.OpenScene(HubPath, OpenSceneMode.Additive);

        WaypointMapTrigger map = null;
        foreach (var go in hubScene.GetRootGameObjects())
        {
            map = go.GetComponentInChildren<WaypointMapTrigger>(true);
            if (map != null) break;
        }

        if (map == null)
        {
            r.AppendLine("  ⚠ WaypointMapTrigger not found in HUB — patch manually");
            EditorSceneManager.CloseScene(hubScene, true);
            return;
        }

        bool patched = false;
        for (int i = 0; i < map.nodes.Length; i++)
        {
            if (map.nodes[i].id != "gathering") continue;

            if (map.nodes[i].sceneName != SceneNames.GatheringZone ||
                !map.nodes[i].unlocked)
            {
                map.nodes[i].sceneName   = SceneNames.GatheringZone;
                map.nodes[i].unlocked    = true;
                map.nodes[i].subtitle    = "harvest · craft · relax";
                map.nodes[i].description = "A peaceful zone for AFK woodcutting, fishing, and mining. A forge is available for crafting.";
                EditorUtility.SetDirty(map);
                patched = true;
            }
            break;
        }

        if (patched)
        {
            EditorSceneManager.MarkSceneDirty(hubScene);
            EditorSceneManager.SaveScene(hubScene);
            r.AppendLine("  ✓ HUB WaypointMapTrigger gathering node patched + saved");
        }
        else
        {
            r.AppendLine("  ✓ HUB WaypointMapTrigger gathering node already correct");
        }

        EditorSceneManager.CloseScene(hubScene, false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    static GameObject GetOrCreate(string name, Transform parent)
    {
        var found = GameObject.Find(name);
        if (found != null) return found;

        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent);
        return go;
    }

    [MenuItem("BCE/Setup/10 ▶ Gathering Zone — Build Scene", true)]
    static bool Validate() => !Application.isPlaying;
}
#endif
