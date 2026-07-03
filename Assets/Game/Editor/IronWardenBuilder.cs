// IronWardenController is standard runtime code (no #if guard needed on the controller),
// but this editor builder references SiegeTurretBehaviour which lives in the same
// Combat/Scripts folder. Guard the builder under UNITY_EDITOR only.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

/// <summary>
/// BCE/Setup/9 — Scaffold The Iron Warden boss encounter.
///
/// Menu items:
///   9a  Place Iron Warden root GameObject + IronWardenController component
///   9b  Place Warden Arena trigger (BoxCollider, activates boss on player enter)
///   9c  Place two SiegeTurret placeholder objects (Phase 2)
///   9d  Place Devastation warning circle prefab (if WarningCircle prefab assigned)
///
/// After running these in the editor, assign the warningCirclePrefab field on the
/// IronWardenController component and bake NavMesh for the arena scene.
/// </summary>
public static class IronWardenBuilder
{
    const string MenuRoot = "BCE/Setup/9 — Iron Warden/";

    [MenuItem(MenuRoot + "9a ▶ Place Iron Warden Boss")]
    static void PlaceIronWarden()
    {
        var existing = Object.FindAnyObjectByType<IronWardenController>();
        if (existing != null)
        {
            Debug.Log("[BCE] IronWardenController already in scene.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Root object
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "IronWarden";
        go.transform.localScale = new Vector3(3f, 4f, 3f);
        go.transform.position   = Vector3.zero;

        // Orange forge-fire material
        var mat   = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.25f, 0.25f, 0.30f);
        go.GetComponent<Renderer>().sharedMaterial = mat;

        // Point light — forge glow
        var lightGO = new GameObject("ForgeGlow");
        lightGO.transform.SetParent(go.transform, false);
        lightGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        var light   = lightGO.AddComponent<Light>();
        light.type  = LightType.Point;
        light.color = new Color(1f, 0.45f, 0.1f);
        light.intensity = 3f;
        light.range     = 12f;

        // Controller
        var ctrl = go.AddComponent<IronWardenController>();
        // Mirror NetworkIdentity required for SyncVar/ClientRpc
        go.AddComponent<Mirror.NetworkIdentity>();

        // NavMeshAgent for movement in Phase 3
        var agent = go.AddComponent<NavMeshAgent>();
        agent.speed        = 3.5f;
        agent.angularSpeed = 200f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 4f;

        Undo.RegisterCreatedObjectUndo(go, "Create Iron Warden");
        Selection.activeGameObject = go;

        Debug.Log("[BCE] IronWardenController placed. " +
                  "Assign warningCirclePrefab in Inspector, then bake NavMesh.");
    }

    [MenuItem(MenuRoot + "9b ▶ Place Warden Arena Trigger")]
    static void PlaceArenaTrigger()
    {
        var go = new GameObject("WardenArenaTrigger");
        go.transform.position = new Vector3(0f, 1f, -15f);

        var col           = go.AddComponent<BoxCollider>();
        col.isTrigger     = true;
        col.size          = new Vector3(6f, 3f, 2f);

        var trigger = go.AddComponent<BossArenaTrigger>();
        // BossArenaTrigger.cs: OnTriggerEnter → finds IronWardenController → calls Activate()

        Undo.RegisterCreatedObjectUndo(go, "Create WardenArenaTrigger");
        Selection.activeGameObject = go;
        Debug.Log("[BCE] Arena trigger placed at z=-15. Position in front of the boss door.");
    }

    [MenuItem(MenuRoot + "9c ▶ Place Siege Turret Placeholders (×2)")]
    static void PlaceSiegeTurrets()
    {
        PlaceTurret("SiegeTurret_Left",  new Vector3(-8f, 0f, 0f));
        PlaceTurret("SiegeTurret_Right", new Vector3( 8f, 0f, 0f));
        Debug.Log("[BCE] Siege turret placeholders placed. Add SiegeTurretBehaviour and assign warden reference.");
    }

    static void PlaceTurret(string name, Vector3 pos)
    {
        var go      = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name     = name;
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(1.5f, 2f, 1.5f);

        var mat   = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.3f, 0.3f, 0.4f);
        go.GetComponent<Renderer>().sharedMaterial = mat;

        var light2         = new GameObject("TurretGlow");
        light2.transform.SetParent(go.transform, false);
        light2.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        var l              = light2.AddComponent<Light>();
        l.type             = LightType.Point;
        l.color            = new Color(1f, 0.45f, 0.1f);
        l.intensity        = 1.5f;
        l.range            = 6f;

        go.AddComponent<Mirror.NetworkIdentity>();

        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
    }
}
#endif
