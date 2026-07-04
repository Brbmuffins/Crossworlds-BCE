using UnityEditor;
using UnityEngine;

public static class MapZoneBuilder
{
    [MenuItem("Rate of Decay/World/Create Zone Trigger")]
    public static void CreateZoneTrigger()
    {
        var go = new GameObject("ZoneTrigger_New Zone");
        Undo.RegisterCreatedObjectUndo(go, "Create Zone Trigger");

        go.transform.position = GetSceneViewPosition();

        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(24f, 8f, 24f);
        box.center = new Vector3(0f, 4f, 0f);

        var zone = go.AddComponent<MapZoneTrigger>();
        zone.zoneName = "New Zone";
        zone.subtitle = "Discovered";

        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
    }

    static Vector3 GetSceneViewPosition()
    {
        var view = SceneView.lastActiveSceneView;
        if (view == null) return Vector3.zero;

        var ray = new Ray(view.camera.transform.position, view.camera.transform.forward);
        if (Physics.Raycast(ray, out var hit, 5000f)) return hit.point;

        return view.pivot;
    }
}
