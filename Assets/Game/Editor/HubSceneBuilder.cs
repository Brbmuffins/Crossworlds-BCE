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
    //  Step 8: Add Forge NPC + Mining Nodes
    //  Run AFTER BCE/Build Hub Scene. Adds:
    //    • 1 Forge Master NPC (ForgeNPC, opens smithing CraftingUI)
    //    • 3 Copper Ore nodes (ResourceNode, yieldItemId=ore_copper, respawn=60s)
    //  Mining nodes are scattered at radius 14 from center.
    // ─────────────────────────────────────────────────────────────────────────────
    [MenuItem("BCE/Hub Setup/8 - Add Forge and Mining NPCs")]
    static void AddForgeAndMining()
    {
        // ── Forge NPC ─────────────────────────────────────────────────────────
        // Placed behind the spawn ring at z=-8, facing players coming in.
        var forgeParent = new GameObject("ForgeNPC_Master");
        forgeParent.transform.position = new Vector3(-12f, 0f, -4f);
        forgeParent.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        // Visual: simple capsule + orange point light to make it stand out
        var npcBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npcBody.name = "Body";
        npcBody.transform.SetParent(forgeParent.transform, false);
        npcBody.transform.localPosition = new Vector3(0f, 1f, 0f);
        npcBody.transform.localScale    = new Vector3(0.7f, 0.85f, 0.7f);
        Object.DestroyImmediate(npcBody.GetComponent<CapsuleCollider>());
        var npcMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        npcMat.color = new Color(0.55f, 0.35f, 0.15f);
        npcBody.GetComponent<Renderer>().sharedMaterial = npcMat;

        var forgeLightGO = new GameObject("ForgeLight");
        forgeLightGO.transform.SetParent(forgeParent.transform, false);
        forgeLightGO.transform.localPosition = new Vector3(0f, 2.5f, 0f);
        var forgeLight = forgeLightGO.AddComponent<Light>();
        forgeLight.type      = LightType.Point;
        forgeLight.color     = new Color(1f, 0.55f, 0.1f);
        forgeLight.intensity = 2.5f;
        forgeLight.range     = 8f;

        // Proximity collider for visual radius
        var triggerCol = forgeParent.AddComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius    = 4f;

        // ForgeNPC script
        var fnpc = forgeParent.AddComponent<ForgeNPC>();
        fnpc.professionId  = 1;
        fnpc.npcName       = "Forge Master";
        fnpc.interactRange = 3.5f;

        Debug.Log("[HubSceneBuilder] ✓ Added Forge Master NPC at (-12, 0, -4).");

        // ── Mining Nodes (3 × Copper Ore) ──────────────────────────────────────
        var minePositions = new Vector3[]
        {
            new Vector3( 14f, 0f,  6f),
            new Vector3( 18f, 0f, -2f),
            new Vector3( 10f, 0f, 10f),
        };

        for (int i = 0; i < minePositions.Length; i++)
        {
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

            // Collider on parent (ResourceNode looks for GetComponent<Collider>())
            var bc = mineGO.AddComponent<BoxCollider>();
            bc.center = new Vector3(0f, 0.4f, 0f);
            bc.size   = new Vector3(0.9f, 0.7f, 0.9f);

            // ResourceNode script
            var rn = mineGO.AddComponent<ResourceNode>();
            rn.yieldItemId       = "ore_copper";
            rn.yieldQuantity     = 1;
            rn.hitsToDeplete     = 3;
            rn.respawnTime       = 60f;
            rn.interactRange     = 3f;
            rn.professionId      = 2;
            rn.professionXpPerHit = 15;
        }

        Debug.Log("[HubSceneBuilder] ✓ Added 3 Copper Ore nodes. Press Ctrl+S to save scene.");
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
                  "NEXT:\n" +
                  "1. Add HangmanNPC to NetworkManager.spawnPrefabs (or register by code in RodNetworkManager)\n" +
                  "2. Place HangmanDialogueUI prefab in scene (it is NOT self-bootstrapping)\n" +
                  "3. Ctrl+S");
    }
}
#endif
