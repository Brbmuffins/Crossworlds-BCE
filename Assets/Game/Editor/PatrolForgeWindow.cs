#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public sealed class PatrolForgeWindow : EditorWindow
{
    readonly List<GameObject> _agents = new();
    EnemyPatrolRoute _route;
    string _routeName = "Patrol Route";
    bool _placingWaypoints;
    bool _staggerStartingWaypoints = true;
    Vector2 _scroll;

    [MenuItem("BCE/Patrol Forge", priority = 37)]
    static void Open() => GetWindow<PatrolForgeWindow>("Patrol Forge");

    void OnEnable() => SceneView.duringSceneGui += DuringSceneGUI;

    void OnDisable() => SceneView.duringSceneGui -= DuringSceneGUI;

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Create one route, add multiple scene enemies or models, then click points in the Scene view. " +
            "Patrol runs on the Mirror server and pauses for combat and leash return.",
            MessageType.Info);

        EditorGUILayout.LabelField("Patrol Route", EditorStyles.boldLabel);
        _routeName = EditorGUILayout.TextField("Route Name", _routeName);
        EnemyPatrolRoute nextRoute = (EnemyPatrolRoute)EditorGUILayout.ObjectField(
            "Existing Route", _route, typeof(EnemyPatrolRoute), true);
        if (nextRoute != _route)
        {
            _route = nextRoute;
            if (_route != null)
            {
                _routeName = _route.name;
                LoadRegisteredPatrolObjects();
            }
            SceneView.RepaintAll();
        }

        if (_route == null)
        {
            if (GUILayout.Button("Create Route in Current Scene", GUILayout.Height(30)))
                CreateRoute();
        }
        else
        {
            Undo.RecordObject(_route, "Edit Patrol Route");
            _route.mode = (EnemyPatrolMode)EditorGUILayout.EnumPopup("Route Behavior", _route.mode);
            _route.waypointWaitSeconds = EditorGUILayout.FloatField(
                "Wait at Each Point", _route.waypointWaitSeconds);
            _route.arrivalDistance = EditorGUILayout.FloatField(
                "Arrival Distance", _route.arrivalDistance);
            EditorUtility.SetDirty(_route);

            GUI.backgroundColor = _placingWaypoints
                ? new Color(1f, 0.72f, 0.2f) : new Color(0.55f, 0.85f, 1f);
            if (GUILayout.Button(
                    _placingWaypoints ? "Finish Placing Waypoints" : "Click Waypoints in Scene",
                    GUILayout.Height(32)))
            {
                if (!_placingWaypoints)
                    AddSelectedObjects();
                _placingWaypoints = !_placingWaypoints;
                if (_placingWaypoints) SceneView.lastActiveSceneView?.Focus();
            }
            GUI.backgroundColor = Color.white;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Remove Last Waypoint") && _route.waypoints.Count > 0)
                    RemoveLastWaypoint();
                if (GUILayout.Button("Select Route"))
                    Selection.activeGameObject = _route.gameObject;
            }
            GUI.backgroundColor = new Color(1f, 0.48f, 0.42f);
            if (GUILayout.Button("Delete Route", GUILayout.Height(26)))
                DeleteRoute();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.LabelField($"Waypoints: {_route.Count}");
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Patrolling Models / Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select one or more placed objects in the Hierarchy, then add them here. " +
            "EnemyController objects keep their normal aggro, combat, leash, and respawn behavior.",
            MessageType.None);
        if (GUILayout.Button("Add Selected Scene Objects", GUILayout.Height(28)))
            AddSelectedObjects();

        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(80f));
        for (int i = _agents.Count - 1; i >= 0; i--)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _agents[i] = (GameObject)EditorGUILayout.ObjectField(
                    _agents[i], typeof(GameObject), true);
                if (GUILayout.Button("Remove", GUILayout.Width(65)))
                    _agents.RemoveAt(i);
            }
        }
        EditorGUILayout.EndScrollView();

        bool groupFormation = _route != null && _route.groupFormationPatrol;
        using (new EditorGUI.DisabledScope(_route == null))
        {
            bool nextGroupFormation = EditorGUILayout.Toggle(
                new GUIContent("Group Formation Patrol",
                    "Keeps assigned models in formation continuously. Members adjust speed while moving instead of waiting at waypoints."),
                groupFormation);
            if (_route != null && nextGroupFormation != groupFormation)
            {
                Undo.RecordObject(_route, "Change Group Formation Patrol");
                _route.groupFormationPatrol = nextGroupFormation;
                EditorUtility.SetDirty(_route);
                groupFormation = nextGroupFormation;
            }
        }
        using (new EditorGUI.DisabledScope(groupFormation))
        {
            _staggerStartingWaypoints = EditorGUILayout.Toggle(
                new GUIContent("Stagger Starting Points",
                    "Distributes models along different waypoints. Disabled while formation is preserved."),
                _staggerStartingWaypoints);
        }

        using (new EditorGUI.DisabledScope(_route == null || _route.Count == 0 || _agents.Count == 0))
        {
            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.45f);
            if (GUILayout.Button("Apply Route to All Listed Objects", GUILayout.Height(36)))
                ApplyRoute();
            GUI.backgroundColor = Color.white;
        }
    }

    void CreateRoute()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        string cleanName = string.IsNullOrWhiteSpace(_routeName)
            ? "Patrol Route" : _routeName.Trim();
        var go = new GameObject(cleanName);
        Undo.RegisterCreatedObjectUndo(go, "Create Patrol Route");
        _route = Undo.AddComponent<EnemyPatrolRoute>(go);
        foreach (GameObject selected in selectedObjects)
        {
            if (selected == null || EditorUtility.IsPersistent(selected)) continue;
            EnemyController enemy = selected.GetComponentInParent<EnemyController>();
            GameObject target = enemy != null ? enemy.gameObject : selected;
            AddPatrolTarget(target);
        }
        Selection.activeGameObject = go;
        EditorSceneManager.MarkSceneDirty(go.scene);
        _placingWaypoints = true;
        SceneView.lastActiveSceneView?.Focus();
    }

    void AddSelectedObjects()
    {
        foreach (GameObject selected in Selection.gameObjects)
        {
            if (selected == null || EditorUtility.IsPersistent(selected)) continue;
            EnemyController enemy = selected.GetComponentInParent<EnemyController>();
            GameObject target = enemy != null ? enemy.gameObject : selected;
            AddPatrolTarget(target);
        }
    }

    void LoadRegisteredPatrolObjects()
    {
        _agents.Clear();
        if (_route == null) return;
        foreach (GameObject target in _route.patrolObjects)
            AddPatrolTarget(target);
    }

    void AddPatrolTarget(GameObject target)
    {
        if (target == null || EditorUtility.IsPersistent(target)) return;
        if (_route != null &&
            (target == _route.gameObject ||
             target.transform.IsChildOf(_route.transform) ||
             target.scene != _route.gameObject.scene))
            return;
        if (target.GetComponent<EnemyPatrolRoute>() != null) return;
        if (!_agents.Contains(target)) _agents.Add(target);
    }

    void ApplyRoute()
    {
        int configured = 0;
        Vector3 groupCenter = Vector3.zero;
        int validCount = 0;
        foreach (GameObject candidate in _agents)
        {
            if (!IsValidPatrolTarget(candidate)) continue;
            groupCenter += candidate.transform.position;
            validCount++;
        }
        if (validCount > 0) groupCenter /= validCount;
        int formationStart = FindNextWaypoint(groupCenter);
        Vector3 formationForward = _route.GetWaypointForward(formationStart);
        Vector3 formationRight = Vector3.Cross(Vector3.up, formationForward).normalized;

        for (int i = 0; i < _agents.Count; i++)
        {
            GameObject target = _agents[i];
            if (!IsValidPatrolTarget(target)) continue;

            NavMeshAgent navAgent = target.GetComponent<NavMeshAgent>() ??
                                    Undo.AddComponent<NavMeshAgent>(target);
            EnemyPatrolAgent patrol = target.GetComponent<EnemyPatrolAgent>() ??
                                      Undo.AddComponent<EnemyPatrolAgent>(target);
            Undo.RecordObject(patrol, "Assign Patrol Route");
            patrol.route = _route;
            patrol.preserveFormation = _route.groupFormationPatrol;
            if (_route.groupFormationPatrol)
            {
                Vector3 spacing = target.transform.position - groupCenter;
                patrol.formationOffset = new Vector2(
                    Vector3.Dot(spacing, formationRight),
                    Vector3.Dot(spacing, formationForward));
                patrol.startingWaypoint = formationStart;
            }
            else
            {
                patrol.formationOffset = Vector2.zero;
                patrol.startingWaypoint = _staggerStartingWaypoints
                    ? FindNextWaypoint(target.transform.position)
                    : 0;
            }
            EditorUtility.SetDirty(patrol);
            EditorUtility.SetDirty(navAgent);

            Undo.RecordObject(_route, "Register Patrol Object");
            if (!_route.patrolObjects.Contains(target))
                _route.patrolObjects.Add(target);
            EditorUtility.SetDirty(_route);

            EnemyController enemy = target.GetComponent<EnemyController>();
            if (enemy != null)
            {
                Undo.RecordObject(enemy, "Disable Random Roaming for Patrol");
                enemy.enableRoaming = false;
                EditorUtility.SetDirty(enemy);
            }
            EditorSceneManager.MarkSceneDirty(target.scene);
            configured++;
        }
        EditorUtility.DisplayDialog("Patrol Forge",
            $"Applied '{_route.name}' to {configured} scene object(s).", "OK");
    }

    bool IsValidPatrolTarget(GameObject target)
    {
        return target != null &&
               !EditorUtility.IsPersistent(target) &&
               target != _route.gameObject &&
               !target.transform.IsChildOf(_route.transform) &&
               target.scene == _route.gameObject.scene &&
               target.GetComponent<EnemyPatrolRoute>() == null;
    }

    int FindNextWaypoint(Vector3 position)
    {
        if (_route == null || _route.Count <= 1) return 0;

        int nearest = 0;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < _route.Count; i++)
        {
            Transform waypoint = _route.waypoints[i];
            if (waypoint == null) continue;
            float distance = (waypoint.position - position).sqrMagnitude;
            if (distance >= nearestDistance) continue;
            nearestDistance = distance;
            nearest = i;
        }

        return _route.mode == EnemyPatrolMode.OneWay
            ? Mathf.Min(nearest + 1, _route.Count - 1)
            : (nearest + 1) % _route.Count;
    }

    void DuringSceneGUI(SceneView sceneView)
    {
        DrawRouteHandles();
        if (!_placingWaypoints || _route == null) return;

        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(12, 12, 350, 58), EditorStyles.helpBox);
        GUILayout.Label("Click terrain or walkable ground to add a waypoint.\nEsc or right-click finishes.");
        GUILayout.EndArea();
        Handles.EndGUI();

        Event evt = Event.current;
        if ((evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape) ||
            (evt.type == EventType.MouseDown && evt.button == 1))
        {
            _placingWaypoints = false;
            evt.Use();
            Repaint();
            return;
        }
        if (evt.type != EventType.MouseDown || evt.button != 0 || evt.alt) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 10000f, Physics.AllLayers,
                QueryTriggerInteraction.Ignore))
            return;

        Vector3 position = hit.point;
        if (NavMesh.SamplePosition(position, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
            position = navHit.position;
        AddWaypoint(position);
        evt.Use();
    }

    void AddWaypoint(Vector3 position)
    {
        var waypoint = new GameObject($"Waypoint_{_route.Count + 1:00}");
        Undo.RegisterCreatedObjectUndo(waypoint, "Add Patrol Waypoint");
        waypoint.transform.SetParent(_route.transform, true);
        waypoint.transform.position = position;
        Undo.RecordObject(_route, "Add Patrol Waypoint");
        _route.waypoints.Add(waypoint.transform);
        EditorUtility.SetDirty(_route);
        EditorSceneManager.MarkSceneDirty(_route.gameObject.scene);
        SceneView.RepaintAll();
    }

    void RemoveLastWaypoint()
    {
        int index = _route.waypoints.Count - 1;
        Transform waypoint = _route.waypoints[index];
        Undo.RecordObject(_route, "Remove Patrol Waypoint");
        _route.waypoints.RemoveAt(index);
        if (waypoint != null) Undo.DestroyObjectImmediate(waypoint.gameObject);
        EditorUtility.SetDirty(_route);
        EditorSceneManager.MarkSceneDirty(_route.gameObject.scene);
    }

    void DeleteRoute()
    {
        if (_route == null) return;

        string routeName = _route.name;
        if (!EditorUtility.DisplayDialog(
                "Delete Patrol Route",
                $"Delete '{routeName}', all of its waypoints, and patrol components assigned to it?\n\n" +
                "Enemy combat settings and roaming settings will not be changed. This can be undone.",
                "Delete Route",
                "Cancel"))
            return;

        EnemyPatrolRoute routeToDelete = _route;
        EnemyPatrolAgent[] patrolAgents =
            Resources.FindObjectsOfTypeAll<EnemyPatrolAgent>();
        foreach (EnemyPatrolAgent patrol in patrolAgents)
        {
            if (patrol == null || patrol.route != routeToDelete ||
                EditorUtility.IsPersistent(patrol))
                continue;

            if (!_agents.Contains(patrol.gameObject))
                _agents.Add(patrol.gameObject);
            Undo.DestroyObjectImmediate(patrol);
        }

        GameObject routeObject = routeToDelete.gameObject;
        _route = null;
        _placingWaypoints = false;
        Undo.DestroyObjectImmediate(routeObject);
        EditorSceneManager.MarkAllScenesDirty();
        SceneView.RepaintAll();
        Repaint();
    }

    void DrawRouteHandles()
    {
        if (_route == null || _route.waypoints == null) return;
        Handles.color = new Color(0.1f, 0.9f, 1f, 0.95f);
        for (int i = 0; i < _route.waypoints.Count; i++)
        {
            Transform waypoint = _route.waypoints[i];
            if (waypoint == null) continue;

            float size = HandleUtility.GetHandleSize(waypoint.position) * 0.12f;
            Handles.SphereHandleCap(0, waypoint.position, Quaternion.identity, size, EventType.Repaint);
            Handles.Label(waypoint.position + Vector3.up * size, $"{i + 1}");

            if (i > 0 && _route.waypoints[i - 1] != null)
                Handles.DrawAAPolyLine(4f, _route.waypoints[i - 1].position, waypoint.position);
            if (_route.mode == EnemyPatrolMode.Loop && i == _route.waypoints.Count - 1 &&
                _route.waypoints.Count > 1 && _route.waypoints[0] != null)
                Handles.DrawAAPolyLine(4f, waypoint.position, _route.waypoints[0].position);

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.PositionHandle(waypoint.position, waypoint.rotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(waypoint, "Move Patrol Waypoint");
                waypoint.position = moved;
                EditorSceneManager.MarkSceneDirty(waypoint.gameObject.scene);
            }
        }
    }
}
#endif
