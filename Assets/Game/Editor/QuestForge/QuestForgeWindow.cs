#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class QuestForgeWindow : EditorWindow
{
    const string DefaultFolder = "Assets/Game/Quests/Quest Forge";
    QuestDefinition _quest;
    string _questName = "";
    GameObject _questGiverSource;
    bool _placingQuestGiver;
    Vector2 _scroll;

    [MenuItem("BCE/Quest Forge", priority = 36)]
    static void Open() => GetWindow<QuestForgeWindow>("Quest Forge");

    [OnOpenAsset]
    static bool OpenQuestAsset(int instanceId, int line)
    {
        string assetPath = AssetDatabase.GetAssetPath(instanceId);
        QuestDefinition quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(assetPath);
        if (quest == null) return false;
        QuestForgeWindow window = GetWindow<QuestForgeWindow>("Quest Forge");
        window.LoadQuest(quest);
        window.Show();
        window.Focus();
        return true;
    }

    void OnEnable() => SceneView.duringSceneGui += DuringSceneGUI;
    void OnDisable() => SceneView.duringSceneGui -= DuringSceneGUI;

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "ISOLATED DEVELOPMENT TOOL\nAuthored quests use Mirror server authority and per-player synchronization. " +
            "The Quest Forge itself remains local and unpushed. Database persistence is not enabled yet.",
            MessageType.Info);

        DrawQuestDropArea();

        EditorGUILayout.LabelField("Quest Name", EditorStyles.boldLabel);
        _questName = EditorGUILayout.TextField("Name", _questName);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Quest Giver", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Choose a prefab asset or an object already placed in the scene. " +
            "You can also click Pick Location and then click the ground in the Scene view.",
            MessageType.None);
        _questGiverSource = (GameObject)EditorGUILayout.ObjectField(
            "Giver Prefab / Object", _questGiverSource, typeof(GameObject), true);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Use Selected Object"))
                _questGiverSource = Selection.activeGameObject;
            GUI.backgroundColor = _placingQuestGiver ? new Color(1f, 0.75f, 0.2f) : Color.white;
            if (GUILayout.Button(_placingQuestGiver ? "Cancel Location Pick" : "Pick Location in Scene"))
            {
                _placingQuestGiver = !_placingQuestGiver;
                if (_placingQuestGiver) SceneView.lastActiveSceneView?.Focus();
            }
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Quest Definition", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            QuestDefinition selectedQuest = (QuestDefinition)EditorGUILayout.ObjectField(
                "Definition Asset", _quest, typeof(QuestDefinition), false);
            if (selectedQuest != _quest)
                LoadQuest(selectedQuest);
            if (GUILayout.Button("Create New", GUILayout.Width(100))) CreateQuest();
        }
        if (_quest == null)
        {
            EditorGUILayout.HelpBox("Create or select a Quest Definition.", MessageType.Info);
            DrawSaveDeployButtons();
            return;
        }

        var serialized = new SerializedObject(_quest);
        serialized.Update();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (string property in new[] { "questId", "title", "description",
                     "objectivesMustBeCompletedInOrder", "offerText", "activeText",
                     "completionText", "turnInPrefab" })
            DrawProperty(serialized, property);
        EditorGUILayout.LabelField("Quest Events", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "For a simple discovery quest, select the model in the Scene and use the button below. " +
            "Quest Forge will create the objective, matching ID, and a separate child trigger.",
            MessageType.None);
        if (GUILayout.Button("Use Selected Model as Find Objective", GUILayout.Height(28)))
        {
            serialized.ApplyModifiedProperties();
            AssociateSelectedFindObject();
            serialized.Update();
        }
        DrawProperty(serialized, "objectives", true);
        EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);
        foreach (string property in new[] { "goldReward", "experienceReward",
                     "itemRewardId", "itemRewardQuantity" })
            DrawProperty(serialized, property);
        EditorGUILayout.EndScrollView();
        serialized.ApplyModifiedProperties();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Selected = Object Target")) ConfigureObject();
            if (GUILayout.Button("Selected = Quest Area")) ConfigureArea();
            if (GUILayout.Button("Selected = Enemy Target")) ConfigureEnemy();
        }
        if (GUILayout.Button("Validate Quest")) ValidateQuest();
        EditorGUILayout.HelpBox(
            "Quest state is held authoritatively by the Mirror server for this session. " +
            "Restart the server to clear development progress.",
            MessageType.Info);

        DrawSaveDeployButtons();
    }

    void DrawSaveDeployButtons()
    {
        EditorGUILayout.Space(10);
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_questName)))
            if (GUILayout.Button("Save Quest", GUILayout.Height(32)))
                SaveQuest();

        using (new EditorGUI.DisabledScope(
                   string.IsNullOrWhiteSpace(_questName) || _questGiverSource == null))
        {
            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.45f);
            if (GUILayout.Button("Deploy Quest", GUILayout.Height(38)))
                DeployQuest();
            GUI.backgroundColor = Color.white;
        }
    }

    void DrawQuestDropArea()
    {
        Rect dropArea = GUILayoutUtility.GetRect(0f, 54f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, _quest == null
            ? "Drag a saved Quest asset here to open it"
            : $"Open Quest: {_quest.title}\nDrop another Quest asset to switch",
            EditorStyles.helpBox);

        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition) ||
            (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform))
            return;

        QuestDefinition droppedQuest = null;
        foreach (Object draggedObject in DragAndDrop.objectReferences)
        {
            droppedQuest = draggedObject as QuestDefinition;
            if (droppedQuest != null) break;
        }

        DragAndDrop.visualMode = droppedQuest != null
            ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
        if (evt.type == EventType.DragPerform && droppedQuest != null)
        {
            DragAndDrop.AcceptDrag();
            LoadQuest(droppedQuest);
            GUI.FocusControl(null);
        }
        evt.Use();
    }

    void LoadQuest(QuestDefinition quest)
    {
        _quest = quest;
        if (_quest != null)
        {
            _questName = _quest.title;
            _questGiverSource = _quest.questGiverPrefab;
        }
        else
        {
            _questName = "";
            _questGiverSource = null;
        }
        Repaint();
    }

    void DuringSceneGUI(SceneView sceneView)
    {
        if (!_placingQuestGiver) return;
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(12, 12, 310, 52), EditorStyles.helpBox);
        GUILayout.Label("Click the ground to create the quest start.\nEsc cancels.");
        GUILayout.EndArea();
        Handles.EndGUI();

        Event evt = Event.current;
        if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape)
        {
            _placingQuestGiver = false;
            evt.Use();
            Repaint();
            return;
        }
        if (evt.type != EventType.MouseDown || evt.button != 0 || evt.alt) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
        Vector3 position;
        if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
            position = hit.point;
        else
        {
            var ground = new Plane(Vector3.up, Vector3.zero);
            if (!ground.Raycast(ray, out float distance)) return;
            position = ray.GetPoint(distance);
        }

        var giverObject = new GameObject("Quest Giver Start");
        Undo.RegisterCreatedObjectUndo(giverObject, "Create Quest Giver Start");
        giverObject.transform.position = position;
        _questGiverSource = giverObject;
        Selection.activeGameObject = giverObject;
        _placingQuestGiver = false;
        evt.Use();
        Repaint();
    }

    static void DrawProperty(SerializedObject serialized, string name, bool children = false)
    {
        SerializedProperty property = serialized.FindProperty(name);
        if (property != null) EditorGUILayout.PropertyField(property, children);
    }

    void CreateQuest()
    {
        _quest = CreateInstance<QuestDefinition>();
        _questName = "New Quest";
        _quest.title = _questName;
        _quest.questId = Slug(_questName);
    }

    bool SaveQuest()
    {
        string cleanName = SanitizeFileName(_questName);
        if (string.IsNullOrWhiteSpace(cleanName))
        {
            EditorUtility.DisplayDialog("Quest Forge", "Enter a valid quest name.", "OK");
            return false;
        }

        EnsureFolder(DefaultFolder);
        if (_quest == null) _quest = CreateInstance<QuestDefinition>();
        _questName = cleanName;
        _quest.title = cleanName;
        _quest.questId = Slug(cleanName);

        string desiredPath = $"{DefaultFolder}/{cleanName}.asset";
        string currentPath = AssetDatabase.GetAssetPath(_quest);
        QuestDefinition collision = AssetDatabase.LoadAssetAtPath<QuestDefinition>(desiredPath);
        if (collision != null && collision != _quest)
        {
            EditorUtility.DisplayDialog("Quest Forge",
                $"A quest named '{cleanName}' already exists. Select it or choose another name.", "OK");
            return false;
        }

        if (string.IsNullOrEmpty(currentPath))
            AssetDatabase.CreateAsset(_quest, desiredPath);
        else if (!string.Equals(currentPath, desiredPath, System.StringComparison.OrdinalIgnoreCase))
        {
            string error = AssetDatabase.MoveAsset(currentPath, desiredPath);
            if (!string.IsNullOrEmpty(error))
            {
                EditorUtility.DisplayDialog("Quest Forge", error, "OK");
                return false;
            }
        }

        EditorUtility.SetDirty(_quest);
        AssetDatabase.SaveAssets();
        Selection.activeObject = _quest;
        Debug.Log($"[Quest Forge] Saved '{cleanName}' to {desiredPath}.");
        return true;
    }

    void DeployQuest()
    {
        if (!SaveQuest()) return;

        if (EditorUtility.IsPersistent(_questGiverSource))
        {
            if (!AttachQuestToPrefab(_questGiverSource))
            {
                EditorUtility.DisplayDialog("Quest Forge",
                    "The selected project asset must be a prefab.", "OK");
                return;
            }
            Undo.RecordObject(_quest, "Assign Quest Giver Prefab");
            _quest.questGiverPrefab = _questGiverSource;
            EditorUtility.SetDirty(_quest);
        }
        else
        {
            QuestGiver giver = _questGiverSource.GetComponent<QuestGiver>() ??
                               Undo.AddComponent<QuestGiver>(_questGiverSource);
            Undo.RecordObject(giver, "Designate Quest Start");
            if (!giver.quests.Contains(_quest)) giver.quests.Add(_quest);
            EnsureVisibleQuestMarker(giver, true);
            EditorUtility.SetDirty(giver);
            if (_questGiverSource.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(_questGiverSource.scene);
        }

        if (_quest.turnInPrefab != null && _quest.turnInPrefab != _quest.questGiverPrefab &&
            !AttachQuestToPrefab(_quest.turnInPrefab))
            EditorUtility.DisplayDialog("Quest Forge",
                "The quest giver was configured, but the Turn In reference is not a prefab asset.", "OK");

        RepairDiscoveryObjectiveAssociations();
        AssetDatabase.SaveAssets();
        Selection.activeObject = _questGiverSource;
        Debug.Log($"[Quest Forge] Deployed '{_quest.title}' to '{_questGiverSource.name}'.");
    }

    void RepairDiscoveryObjectiveAssociations()
    {
        foreach (QuestObjectiveDefinition objective in _quest.objectives)
        {
            if (objective.type != QuestObjectiveType.EnterArea ||
                string.IsNullOrWhiteSpace(objective.targetId)) continue;

            string areaName = $"QuestArea_{objective.targetId}";
            foreach (Transform candidate in
                     Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (candidate.name != areaName || candidate.parent == null) continue;

                BoxCollider trigger = candidate.GetComponent<BoxCollider>() ??
                                      Undo.AddComponent<BoxCollider>(candidate.gameObject);
                Undo.RecordObject(trigger, "Repair Quest Discovery Trigger");
                trigger.isTrigger = true;
                FitDiscoveryTrigger(candidate.parent.gameObject, trigger);

                QuestAreaTarget area = candidate.GetComponent<QuestAreaTarget>() ??
                                       Undo.AddComponent<QuestAreaTarget>(candidate.gameObject);
                Undo.RecordObject(area, "Repair Quest Discovery Association");
                area.areaId = objective.targetId;
                EnsureObjectiveMarker(candidate.parent.gameObject, _quest, objective.targetId);
                EditorUtility.SetDirty(candidate.gameObject);
                EditorSceneManager.MarkSceneDirty(candidate.gameObject.scene);
            }
        }
    }

    bool AttachQuestToPrefab(GameObject prefab)
    {
        string path = AssetDatabase.GetAssetPath(prefab);
        if (prefab == null || string.IsNullOrEmpty(path) ||
            PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            return false;

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            QuestGiver giver = root.GetComponent<QuestGiver>() ?? root.AddComponent<QuestGiver>();
            if (!giver.quests.Contains(_quest)) giver.quests.Add(_quest);
            EnsureVisibleQuestMarker(giver, false);
            EditorUtility.SetDirty(giver);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        return true;
    }

    static GameObject SelectedOrWarn()
    {
        if (Selection.activeGameObject != null) return Selection.activeGameObject;
        EditorUtility.DisplayDialog("Quest Forge", "Select a scene object or prefab instance first.", "OK");
        return null;
    }

    void ConfigureObject()
    {
        GameObject selected = SelectedOrWarn(); if (selected == null) return;
        QuestInteractableTarget target = selected.GetComponent<QuestInteractableTarget>() ??
                                         Undo.AddComponent<QuestInteractableTarget>(selected);
        target.targetId = Slug(selected.name);
        EditorUtility.SetDirty(target);
    }

    void ConfigureArea()
    {
        GameObject selected = SelectedOrWarn(); if (selected == null) return;
        Collider collider = selected.GetComponent<Collider>();
        if (collider == null) collider = Undo.AddComponent<BoxCollider>(selected);
        Undo.RecordObject(collider, "Configure Quest Area");
        collider.isTrigger = true;
        QuestAreaTarget target = selected.GetComponent<QuestAreaTarget>() ??
                                 Undo.AddComponent<QuestAreaTarget>(selected);
        target.areaId = Slug(selected.name);
        EditorUtility.SetDirty(selected);
    }

    void ConfigureEnemy()
    {
        GameObject selected = SelectedOrWarn(); if (selected == null) return;
        QuestEnemyTarget target = selected.GetComponent<QuestEnemyTarget>() ??
                                  Undo.AddComponent<QuestEnemyTarget>(selected);
        EnemyController enemy = selected.GetComponent<EnemyController>();
        target.enemyTemplateId = enemy != null ? enemy.enemyTemplateId : Slug(selected.name);
        EditorUtility.SetDirty(target);
    }

    void AssociateSelectedFindObject()
    {
        if (_quest == null)
        {
            EditorUtility.DisplayDialog("Quest Forge",
                "Create or select a quest definition first.", "OK");
            return;
        }

        GameObject selected = Selection.activeGameObject;
        if (selected == null || EditorUtility.IsPersistent(selected))
        {
            EditorUtility.DisplayDialog("Quest Forge",
                "Select the gravestone or other model instance in the Scene hierarchy first.", "OK");
            return;
        }

        string baseId = Slug(selected.name);
        string targetId = baseId.EndsWith("_discovery")
            ? baseId
            : $"{baseId}_discovery";

        Undo.RecordObject(_quest, "Associate Find Quest Object");
        QuestObjectiveDefinition objective = null;
        foreach (QuestObjectiveDefinition candidate in _quest.objectives)
        {
            if (candidate.type == QuestObjectiveType.EnterArea &&
                string.Equals(candidate.targetId, targetId,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                objective = candidate;
                break;
            }
        }
        if (objective == null)
        {
            objective = new QuestObjectiveDefinition { type = QuestObjectiveType.EnterArea };
            _quest.objectives.Add(objective);
        }

        objective.targetId = targetId;
        objective.description = $"Find {NicifyName(selected.name)}";
        objective.requiredAmount = 1;
        GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(selected);
        if (prefabSource != null) objective.targetPrefab = prefabSource;
        EditorUtility.SetDirty(_quest);

        Transform triggerTransform = selected.transform.Find($"QuestArea_{targetId}");
        GameObject triggerObject;
        if (triggerTransform != null)
            triggerObject = triggerTransform.gameObject;
        else
        {
            triggerObject = new GameObject($"QuestArea_{targetId}");
            Undo.RegisterCreatedObjectUndo(triggerObject, "Create Quest Discovery Area");
            triggerObject.transform.SetParent(selected.transform, false);
        }

        BoxCollider trigger = triggerObject.GetComponent<BoxCollider>() ??
                              Undo.AddComponent<BoxCollider>(triggerObject);
        Undo.RecordObject(trigger, "Configure Quest Discovery Area");
        trigger.isTrigger = true;
        FitDiscoveryTrigger(selected, trigger);

        QuestAreaTarget area = triggerObject.GetComponent<QuestAreaTarget>() ??
                               Undo.AddComponent<QuestAreaTarget>(triggerObject);
        Undo.RecordObject(area, "Associate Quest Discovery Area");
        area.areaId = targetId;
        EnsureObjectiveMarker(selected, _quest, targetId);
        EditorUtility.SetDirty(triggerObject);
        EditorSceneManager.MarkSceneDirty(selected.scene);
        Selection.activeGameObject = triggerObject;

        Debug.Log($"[Quest Forge] Associated '{selected.name}' with find objective '{targetId}'.");
    }

    static void FitDiscoveryTrigger(GameObject model, BoxCollider trigger)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            trigger.transform.localPosition = Vector3.up;
            trigger.size = new Vector3(3f, 2f, 3f);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        trigger.transform.position = bounds.center;
        trigger.transform.rotation = model.transform.rotation;

        Vector3 scale = trigger.transform.lossyScale;
        Vector3 localSize = new Vector3(
            bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(scale.x)),
            bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(scale.y)),
            bounds.size.z / Mathf.Max(0.001f, Mathf.Abs(scale.z)));
        trigger.center = Vector3.zero;
        trigger.size = new Vector3(
            Mathf.Max(3f, localSize.x + 2f),
            Mathf.Max(2f, localSize.y + 1f),
            Mathf.Max(3f, localSize.z + 2f));
    }

    static void EnsureObjectiveMarker(
        GameObject questObject, QuestDefinition quest, string targetId)
    {
        QuestObjectiveMarker association = questObject.GetComponent<QuestObjectiveMarker>() ??
                                           Undo.AddComponent<QuestObjectiveMarker>(questObject);
        Undo.RecordObject(association, "Associate Quest Objective Marker");
        association.quest = quest;
        association.targetId = targetId;

        Transform markerTransform = null;
        foreach (Transform child in questObject.GetComponentsInChildren<Transform>(true))
            if (child.name == "QuestObjectiveMarker") { markerTransform = child; break; }

        GameObject markerObject;
        if (markerTransform != null)
            markerObject = markerTransform.gameObject;
        else
        {
            markerObject = new GameObject("QuestObjectiveMarker");
            Undo.RegisterCreatedObjectUndo(markerObject, "Create Quest Objective Marker");
            markerObject.transform.SetParent(questObject.transform, false);
        }

        Bounds bounds = new Bounds(questObject.transform.position, Vector3.one);
        Renderer[] renderers = questObject.GetComponentsInChildren<Renderer>();
        bool foundRenderer = false;
        foreach (Renderer renderer in renderers)
        {
            if (renderer.transform == markerObject.transform ||
                renderer.transform.IsChildOf(markerObject.transform)) continue;
            if (!foundRenderer) { bounds = renderer.bounds; foundRenderer = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        markerObject.transform.position =
            foundRenderer
                ? new Vector3(bounds.center.x, bounds.max.y + 0.75f, bounds.center.z)
                : questObject.transform.position + Vector3.up * 2.75f;

        TMPro.TextMeshPro marker = markerObject.GetComponent<TMPro.TextMeshPro>() ??
                                   markerObject.AddComponent<TMPro.TextMeshPro>();
        marker.text = "!";
        marker.fontSize = 5f;
        marker.alignment = TMPro.TextAlignmentOptions.Center;
        marker.color = new Color(1f, 0.82f, 0.08f);
        marker.rectTransform.sizeDelta = new Vector2(1.5f, 1.5f);
        if (markerObject.GetComponent<QuestMarkerBillboard>() == null)
            markerObject.AddComponent<QuestMarkerBillboard>();
        markerObject.SetActive(true);
        EditorUtility.SetDirty(association);
        EditorUtility.SetDirty(markerObject);
    }

    static string NicifyName(string value)
    {
        string clean = value.Replace("(Clone)", "").Trim();
        return ObjectNames.NicifyVariableName(clean.Replace("_", " "));
    }

    void ValidateQuest()
    {
        int errors = 0;
        if (string.IsNullOrWhiteSpace(_quest.questId)) errors++;
        if (string.IsNullOrWhiteSpace(_quest.title)) errors++;
        if (_questGiverSource == null) errors++;
        if (_quest.objectives.Count == 0) errors++;
        foreach (QuestObjectiveDefinition objective in _quest.objectives)
            if (string.IsNullOrWhiteSpace(objective.targetId)) errors++;
        EditorUtility.DisplayDialog("Quest Forge Validation",
            errors == 0 ? "Quest passed Mirror compatibility validation." :
                $"Quest has {errors} blocking issue(s).", "OK");
    }

    static string Slug(string value) => value.Trim().ToLowerInvariant().Replace(" ", "_");

    static string SanitizeFileName(string value)
    {
        string result = value?.Trim() ?? "";
        foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
            result = result.Replace(invalid.ToString(), "");
        return result;
    }

    static void EnsureVisibleQuestMarker(QuestGiver giver, bool recordUndo)
    {
        Transform existing = null;
        foreach (Transform child in giver.GetComponentsInChildren<Transform>(true))
            if (child.name == "QuestMarker") { existing = child; break; }
        GameObject markerObject;
        if (existing != null)
            markerObject = existing.gameObject;
        else
        {
            markerObject = new GameObject("QuestMarker");
            if (recordUndo)
                Undo.RegisterCreatedObjectUndo(markerObject, "Create Quest Marker");
            markerObject.transform.SetParent(
                giver.markerAnchor != null ? giver.markerAnchor : giver.transform, false);
        }

        if (giver.markerAnchor != null)
            markerObject.transform.localPosition = Vector3.zero;
        else
        {
            Renderer[] renderers = giver.GetComponentsInChildren<Renderer>();
            bool foundRenderer = false;
            Bounds bounds = default;
            foreach (Renderer renderer in renderers)
            {
                if (renderer.transform == markerObject.transform ||
                    renderer.transform.IsChildOf(markerObject.transform)) continue;
                if (!foundRenderer) { bounds = renderer.bounds; foundRenderer = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            if (foundRenderer)
                markerObject.transform.position = new Vector3(
                    bounds.center.x, bounds.max.y + 0.75f, bounds.center.z);
            else markerObject.transform.position =
                giver.transform.position + Vector3.up * 2.75f;
        }
        TMPro.TextMeshPro marker = markerObject.GetComponent<TMPro.TextMeshPro>() ??
                                   markerObject.AddComponent<TMPro.TextMeshPro>();
        marker.text = "?";
        marker.fontSize = 5f;
        marker.alignment = TMPro.TextAlignmentOptions.Center;
        marker.color = new Color(1f, 0.82f, 0.08f);
        marker.rectTransform.sizeDelta = new Vector2(1.5f, 1.5f);
        if (markerObject.GetComponent<QuestMarkerBillboard>() == null)
            markerObject.AddComponent<QuestMarkerBillboard>();
        markerObject.SetActive(true);
        EditorUtility.SetDirty(markerObject);
    }

    static void EnsureFolder(string path)
    {
        string current = "Assets";
        foreach (string part in path.Substring(7).Split('/'))
        {
            string next = $"{current}/{part}";
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part);
            current = next;
        }
    }
}
#endif
