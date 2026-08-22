// ═══════════════════════════════════════════════════════════════════════════
//  HubSceneBuilder — BCE/Build Hub Scene  (Editor-only, never ships in build)
//
//  Wipes decoration/environment from the scene, then rebuilds with only:
//    • Directional light
//    • Gray ground plane (100×100, tagged "Ground")
//    • 8 NetworkStartPosition spawn points
//    • RodChatManager scene object (NetworkIdentity — required for chat)
//
//  Objects with NetworkIdentity or NetworkManager are always preserved.
//  Run: Menu → BCE → Build Hub Scene (Hub.unity must be open), then Ctrl+S.
// ═══════════════════════════════════════════════════════════════════════════

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HubSceneBuilder
{
    const string ForgePrefabPath = "Assets/Game/3D Models/HUB ASSETS/Forge/prefab_forge.prefab";
    [MenuItem("BCE/Build Hub Scene")]
    static void Build()
    {
        // ── Destroy decoration/environment — preserve networking objects ──
        // Keep anything with a NetworkIdentity (RodChatManager, etc.),
        // NetworkManager, or NetworkAuthenticator — deleting those breaks Mirror.
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude))
        {
            if (go.transform.parent != null) continue; // root objects only
            if (go.GetComponent<Mirror.NetworkIdentity>()      != null) continue;
            if (go.GetComponent<Mirror.NetworkManager>()       != null) continue;
            if (go.GetComponent<Mirror.NetworkAuthenticator>() != null) continue;
            Object.DestroyImmediate(go);
        }

        // ── Ensure RodChatManager scene object exists ─────────────────────
        // Chat requires a scene NetworkBehaviour with a NetworkIdentity.
        // If the previous step preserved it, this is a no-op.
        if (Object.FindAnyObjectByType<RodChatManager>() == null)
        {
            var chatGO = new GameObject("RodChatManager");
            chatGO.AddComponent<Mirror.NetworkIdentity>();
            chatGO.AddComponent<RodChatManager>();
            Debug.Log("[HubSceneBuilder] Added RodChatManager to scene.");
        }

        // ── Main Camera ───────────────────────────────────────────────────
        // Must be tagged MainCamera so Camera.main resolves.
        // PlayerMovement.Start() finds it and adds CameraFollow automatically.
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetPositionAndRotation(
            new Vector3(0f, 5f, -8f),
            Quaternion.Euler(20f, 0f, 0f));
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();

        // ── Directional light ─────────────────────────────────────────────
        var sunGO = new GameObject("Sun");
        sunGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var sun = sunGO.AddComponent<Light>();
        sun.type      = LightType.Directional;
        sun.intensity = 1f;
        sun.shadows   = LightShadows.None;

        // ── Gray ground plane ─────────────────────────────────────────────
        // Plane primitive is 10×10 units natively; scale ×10 = 100×100.
        // Replace auto-MeshCollider with BoxCollider — more reliable edge behaviour.
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.tag  = "Ground";
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        Object.DestroyImmediate(ground.GetComponent<MeshCollider>());
        var bc    = ground.AddComponent<BoxCollider>();
        bc.center = Vector3.zero;
        bc.size   = new Vector3(1f, 0.02f, 1f);

        // ── 8 NetworkStartPosition spawn points ───────────────────────────
        for (int i = 0; i < 8; i++)
        {
            float a  = (360f / 8f * i) * Mathf.Deg2Rad;
            var   sp = new GameObject($"SpawnPoint_{i}");
            sp.transform.position = new Vector3(Mathf.Sin(a) * 4f, 0.1f, Mathf.Cos(a) * 4f);
            sp.AddComponent<Mirror.NetworkStartPosition>();
        }

        // ── Portals (3, at 120° intervals, radius 21) ─────────────────────
        // Uses PortalTransition (NetworkBehaviour, ServerChangeScene) rather than
        // HubPortal (client-side SceneManager.LoadScene) — correct for multiplayer.
        var portalDefs = new (string label, string scene, Color color)[]
        {
            ("Copper Arena",  SceneNames.ArenaCopper,  new Color(0.2f, 0.6f, 1.0f)), // blue
            ("Iron Arena",    "Arena_Iron",    new Color(0.3f, 0.9f, 0.3f)), // green
            ("Dark Forge",    "",              new Color(1.0f, 0.8f, 0.1f)), // yellow — coming soon
        };

        for (int i = 0; i < portalDefs.Length; i++)
        {
            var (label, scene, col) = portalDefs[i];
            float angle = (120f * i - 90f) * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Cos(angle) * 21f, 0f, Mathf.Sin(angle) * 21f);

            // Visual — two stacked cylinders for an arch look
            var portalGO = new GameObject($"Portal_{label.Replace(" ", "")}");
            portalGO.transform.position = pos;
            portalGO.transform.LookAt(Vector3.zero);

            var arch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arch.name = "Arch";
            arch.transform.SetParent(portalGO.transform, false);
            arch.transform.localPosition = new Vector3(0f, 2f, 0f);
            arch.transform.localScale    = new Vector3(0.25f, 2f, 0.25f);
            Object.DestroyImmediate(arch.GetComponent<CapsuleCollider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = col;
            arch.GetComponent<Renderer>().sharedMaterial = mat;

            // Trigger collider for proximity check
            var triggerGO = new GameObject("Trigger");
            triggerGO.transform.SetParent(portalGO.transform, false);
            triggerGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var sc = triggerGO.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 3f;

            // Point light
            var lightGO = new GameObject("PortalLight");
            lightGO.transform.SetParent(portalGO.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 2f, 0f);
            var pl = lightGO.AddComponent<Light>();
            pl.type      = LightType.Point;
            pl.color     = col;
            pl.intensity = 3f;
            pl.range     = 10f;

            // NetworkIdentity required for PortalTransition (NetworkBehaviour)
            portalGO.AddComponent<Mirror.NetworkIdentity>();

            // PortalTransition — server-authoritative scene load
            var pt = portalGO.AddComponent<PortalTransition>();
            pt.arenaSceneName   = scene;
            pt.portalDisplayName = label;

            Debug.Log($"[HubSceneBuilder] Added portal: {label} → {(string.IsNullOrEmpty(scene) ? "Coming Soon" : scene)}");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[HubSceneBuilder] ✓ Hub ready with 3 portals. Ctrl+S to save.\n" +
                  "NEXT: Run BCE/Hub Setup/Add Forge and Mining NPCs, then add portals to NetworkManager scene list.");
    }

    // ─────────────────────────────────────────────────────────────────────────────
    //  Step 8: Add Forge + Crafting Trainer + Mining Stations
    //  Run AFTER BCE/Build Hub Scene. Adds:
    //    • prefab_forge (ForgeNPC, opens ForgeCraftingPanel)
    //    • 1 Crafting Trainer NPC reserved for skill/recipe learning
    //    • 3 Copper Ore gathering stations (server-backed inventory add)
    //  Mining stations are scattered at radius 14 from center.
    // ─────────────────────────────────────────────────────────────────────────────
    [MenuItem("BCE/Hub Setup/8 - Add Forge, Trainer and Mining")]
    static void AddForgeAndMining()
    {
        // The physical forge owns crafting. Refresh the prefab source, then use
        // its placed instance (or instantiate its authored pose when missing).
        ForgeModelMaterialFix.Apply();
        var forgePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ForgePrefabPath);
        var forgeObject = GameObject.Find("prefab_forge");
        if (forgeObject == null && forgePrefab != null)
            forgeObject = PrefabUtility.InstantiatePrefab(forgePrefab) as GameObject;

        if (forgeObject == null)
        {
            Debug.LogError("[HubSceneBuilder] prefab_forge was not found or could not be instantiated.");
            return;
        }

        forgeObject.name = "prefab_forge";
        var forgeInteraction = forgeObject.GetComponent<ForgeNPC>() ?? forgeObject.AddComponent<ForgeNPC>();
        forgeInteraction.professionId = 2;
        forgeInteraction.npcName = "Craft";
        forgeInteraction.interactRange = 3.5f;
        forgeInteraction.promptHeight = 3f;

        // The former Forge Master NPC is reserved for learning the skill and
        // recipes. No trainer API is invented here; this only separates roles.
        var trainer = GameObject.Find("forged_male_vendor_rigged_NPC (1)")
                      ?? GameObject.Find("Forge Master")
                      ?? GameObject.Find("Crafting Trainer");
        if (trainer != null)
        {
            var oldForgeInteraction = trainer.GetComponent<ForgeNPC>();
            if (oldForgeInteraction != null) Object.DestroyImmediate(oldForgeInteraction);
            trainer.name = "Crafting Trainer";
            var npcController = trainer.GetComponent<ForgedNpcController>();
            if (npcController != null) npcController.npcDisplayName = "Crafting Trainer";
        }
        else Debug.LogWarning("[HubSceneBuilder] Male Crafting Trainer NPC was not found.");

        Debug.Log("[HubSceneBuilder] ✓ Forge owns crafting; male NPC reserved as Crafting Trainer.");

        // ── Mining Stations (3 × Copper Ore) ───────────────────────────────────
        var minePositions = new Vector3[]
        {
            new Vector3( 14f, 0f,  6f),
            new Vector3( 18f, 0f, -2f),
            new Vector3( 10f, 0f, 10f),
        };

        for (int i = 0; i < minePositions.Length; i++)
        {
            if (GameObject.Find($"OreNode_Copper_{i}") != null) continue;
            var mineGO = new GameObject($"OreNode_Copper_{i}");
            mineGO.transform.position = minePositions[i];

            // Visual: flattened cube with a bluish-grey rock tint
            var oreMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            oreMesh.name = "OreMesh";
            oreMesh.transform.SetParent(mineGO.transform, false);
            oreMesh.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            oreMesh.transform.localScale    = new Vector3(0.9f, 0.7f, 0.9f);
            oreMesh.transform.Rotate(0f, Random.Range(0f, 360f), 0f);
            var oreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            oreMat.color = new Color(0.35f, 0.55f, 0.65f); // copper-tinted stone
            oreMesh.GetComponent<Renderer>().sharedMaterial = oreMat;

            // Collider on parent for proximity/selection.
            var bc = mineGO.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 0.4f, 0f);
            bc.size   = new Vector3(0.9f, 0.7f, 0.9f);

            // Server-backed gathering station.
#if UNITY_EDITOR || !UNITY_SERVER
            var station = mineGO.AddComponent<AfkGatheringStation>();
            station.stationName      = "Copper Vein";
            station.professionId     = 2;
            station.minLevelRequired = 1;
            station.itemId           = "ore_copper";
            station.itemQuantity     = 1;
            station.tickInterval     = 5f;
            station.xpPerTick        = 10;
            station.interactRange    = 3f;
            station.cancelRadius     = 4f;

            mineGO.AddComponent<Mirror.NetworkIdentity>();
            var networkState = mineGO.AddComponent<GatheringNodeNetworkState>();
            networkState.persistentNodeId = GUID.Generate().ToString();
            networkState.minimumAwardsPerSpawn = 1;
            networkState.maximumAwardsPerSpawn = 5;
            networkState.respawnSeconds = 900f;
            networkState.interactionRange = station.interactRange;
            networkState.minimumSecondsBetweenAwards = Mathf.Max(0.1f, station.tickInterval - 0.25f);
#endif
        }

        Debug.Log("[HubSceneBuilder] ✓ Added 3 Copper Ore gathering stations. Press Ctrl+S to save scene.");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    //  Step 9: Place HangmanNPC (arena entrance challenge NPC)
    //    • Capsule body near the Copper Arena portal
    //    • SphereCollider trigger auto-created by HangmanNPC.Awake
    //    • NetworkIdentity (required — HangmanNPC is a NetworkBehaviour)
    [MenuItem("BCE/Hub Setup/9 - Place HangmanNPC (Arena Entrance)")]
    static void PlaceHangmanNPC()
    {
        // Position in front of the Copper Arena portal (portal is near 0,0,20 by convention)
        var go = new GameObject("HangmanNPC");
        go.transform.position = new Vector3(0f, 0f, 14f);

        // Capsule visual
        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Body";
        capsule.transform.SetParent(go.transform, false);
        capsule.transform.localPosition = new Vector3(0f, 1f, 0f);
        Object.DestroyImmediate(capsule.GetComponent<CapsuleCollider>());
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.12f, 0.08f, 0.22f);
        capsule.GetComponent<Renderer>().sharedMaterial = mat;

        // Eerie glow light
        var lightGO = new GameObject("HangmanGlow");
        lightGO.transform.SetParent(go.transform, false);
        lightGO.transform.localPosition = new Vector3(0f, 2f, 0f);
        var l = lightGO.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(0.6f, 0.1f, 0.9f); l.intensity = 1.8f; l.range = 6f;

        // NetworkIdentity (required for NetworkBehaviour + Commands)
        go.AddComponent<Mirror.NetworkIdentity>();

        // HangmanNPC script (arenaSceneName defaults to SceneNames.ArenaCopper)
        go.AddComponent<HangmanNPC>();

        // Mark dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        UnityEditor.Selection.activeGameObject = go;

        Debug.Log("[HubSceneBuilder] ✓ HangmanNPC placed at (0, 0, 14).\n" +
                  "HangmanDialogueUI is self-bootstrapping — no scene object needed.\n" +
                  "NEXT: Ctrl+S to save Hub.unity");
    }
}
#endif
