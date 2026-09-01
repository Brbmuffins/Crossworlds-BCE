#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mirror;

namespace Crossworlds.EditorTools.NodeForge
{
    public sealed class NodeForgeWindow : EditorWindow
    {
        const string DefinitionFolder = "Assets/Game/Resources/NodeForge/Definitions";

        readonly List<GatheringNodeDefinition> definitions = new();
        GatheringNodeDefinition definition;
        string search = "";
        int categoryFilter;
        Vector2 definitionScroll;
        Vector2 editorScroll;
        bool clickPlacement;
        bool snapToGround = true;
        float groundOffset;

        [MenuItem("BCE/Node Forge", priority = 38)]
        static void Open() => GetWindow<NodeForgeWindow>("Node Forge");

        [InitializeOnLoadMethod]
        static void QueueOpenSceneUpgrade()
        {
            EditorApplication.delayCall += () => UpgradeOpenSceneNodes();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += () => UpgradeOpenSceneNodes();
        }

        [OnOpenAsset]
        static bool OpenDefinition(EntityId entityId, int line)
        {
            GatheringNodeDefinition opened = AssetDatabase.LoadAssetAtPath<GatheringNodeDefinition>(
                AssetDatabase.GetAssetPath(entityId));
            if (opened == null) return false;
            NodeForgeWindow window = GetWindow<NodeForgeWindow>("Node Forge");
            window.definition = opened;
            window.RefreshDefinitions();
            window.Show();
            window.Focus();
            return true;
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
            RefreshDefinitions();
            if (!EditorApplication.isPlayingOrWillChangePlaymode) UpgradeOpenSceneNodes();
        }

        void OnDisable() => SceneView.duringSceneGui -= DuringSceneGUI;

        void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Node Forge", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Create reusable gathering definitions, associate visual prefabs, and place functional nodes. " +
                "Placed nodes retain their definition link so they can be refreshed later.", MessageType.Info);

            DrawDefinitionBrowser();
            if (definition == null)
            {
                EditorGUILayout.HelpBox("Select a definition above or create a new one.", MessageType.None);
                return;
            }

            editorScroll = EditorGUILayout.BeginScrollView(editorScroll);
            DrawDefinitionEditor();
            DrawValidation();
            DrawPlacementTools();
            EditorGUILayout.EndScrollView();
        }

        void DrawDefinitionBrowser()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                search = EditorGUILayout.TextField(new GUIContent("Search"), search);
                string[] filters = { "All", "Mining", "Fishing", "Woodcutting", "Herbalism", "Salvaging", "Other" };
                categoryFilter = EditorGUILayout.Popup(categoryFilter, filters, GUILayout.Width(115));
                if (GUILayout.Button("Refresh", GUILayout.Width(65))) RefreshDefinitions();
                if (GUILayout.Button("New", GUILayout.Width(55))) CreateDefinition();
            }

            definitionScroll = EditorGUILayout.BeginScrollView(definitionScroll, GUILayout.Height(130));
            IEnumerable<GatheringNodeDefinition> visible = definitions.Where(MatchesFilter);
            foreach (GatheringNodeDefinition candidate in visible)
            {
                bool selected = candidate == definition;
                Color old = GUI.backgroundColor;
                if (selected) GUI.backgroundColor = new Color(0.45f, 0.75f, 1f);
                string item = candidate.ResolvedItemId;
                string label = $"{candidate.displayName}   [{candidate.category}]   → {item}";
                if (GUILayout.Button(label, EditorStyles.miniButton))
                {
                    definition = candidate;
                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = old;
            }
            EditorGUILayout.EndScrollView();

            GatheringNodeDefinition selectedDefinition =
                (GatheringNodeDefinition)EditorGUILayout.ObjectField(
                    "Open Definition", definition, typeof(GatheringNodeDefinition), false);
            if (selectedDefinition != definition)
            {
                definition = selectedDefinition;
                if (definition != null && !definitions.Contains(definition)) definitions.Add(definition);
            }
        }

        void DrawDefinitionEditor()
        {
            SerializedObject serialized = new(definition);
            serialized.Update();

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Definition", EditorStyles.boldLabel);
            Draw(serialized, "nodeId", "Node ID");
            Draw(serialized, "displayName", "Display Name");
            Draw(serialized, "category", "Category");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Profession & Yield", EditorStyles.boldLabel);
            Draw(serialized, "professionId", "Profession ID");
            Draw(serialized, "minimumLevel", "Minimum Level");
            Draw(serialized, "yieldItem", "Loot Forge Item");
            Draw(serialized, "itemId", "Database Item ID");
            Draw(serialized, "itemQuantity", "Quantity per Yield");
            Draw(serialized, "secondsPerYield", "Seconds per Yield");
            Draw(serialized, "experiencePerYield", "XP per Yield");
            Draw(serialized, "bonusYieldLevel", "Bonus Yield Level");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Depletion & Respawn", EditorStyles.boldLabel);
            Draw(serialized, "minimumAwardsPerSpawn", "Minimum Awards");
            Draw(serialized, "maximumAwardsPerSpawn", "Maximum Awards");
            Draw(serialized, "respawnSeconds", "Respawn Seconds");
            EditorGUILayout.HelpBox("Recommended live value: 900 seconds (15 minutes).", MessageType.None);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Interaction", EditorStyles.boldLabel);
            Draw(serialized, "interactionRange", "Interaction Range");
            Draw(serialized, "cancelRadius", "Cancel Radius");
            Draw(serialized, "promptHeight", "Prompt Height");
            Draw(serialized, "interactionVerb", "Interaction Verb");
            Draw(serialized, "gatheringAnimationBool", "Animation Bool");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
            Draw(serialized, "visualPrefab", "Node Visual Prefab");
            DrawVisualPreview(serialized.FindProperty("visualPrefab").objectReferenceValue as GameObject);
            Draw(serialized, "visualLocalPosition", "Visual Position");
            Draw(serialized, "visualLocalEulerAngles", "Visual Rotation");
            Draw(serialized, "visualLocalScale", "Visual Scale");
            Draw(serialized, "yieldVFXPrefab", "Yield VFX Prefab");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Collider & Placeholder", EditorStyles.boldLabel);
            Draw(serialized, "colliderCenter", "Collider Center");
            Draw(serialized, "colliderSize", "Collider Size");
            Draw(serialized, "fallbackShape", "Placeholder Shape");
            Draw(serialized, "fallbackColor", "Placeholder Color");

            if (serialized.ApplyModifiedProperties())
                EditorUtility.SetDirty(definition);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.5f);
                if (GUILayout.Button("Save Definition", GUILayout.Height(30))) SaveDefinition();
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("Ping Asset", GUILayout.Height(30)))
                    EditorGUIUtility.PingObject(definition);
                GUI.backgroundColor = new Color(0.9f, 0.4f, 0.35f);
                if (GUILayout.Button("Delete...", GUILayout.Height(30), GUILayout.Width(75)))
                    DeleteDefinition();
                GUI.backgroundColor = Color.white;
            }
        }

        void DrawValidation()
        {
            List<string> blockers = Validate(definition, true);
            List<string> warnings = Validate(definition, false);
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            if (blockers.Count == 0 && warnings.Count == 0)
                EditorGUILayout.HelpBox("Definition is ready to place and test.", MessageType.Info);
            foreach (string issue in blockers) EditorGUILayout.HelpBox(issue, MessageType.Error);
            foreach (string issue in warnings) EditorGUILayout.HelpBox(issue, MessageType.Warning);
        }

        void DrawPlacementTools()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Scene Placement", EditorStyles.boldLabel);
            snapToGround = EditorGUILayout.Toggle("Snap Down to Ground", snapToGround);
            groundOffset = EditorGUILayout.FloatField("Surface Offset", groundOffset);

            bool blocked = Validate(definition, true).Count > 0;
            using (new EditorGUI.DisabledScope(blocked))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Place at Scene View Target", GUILayout.Height(30)))
                        PlaceAt(SceneView.lastActiveSceneView != null
                            ? SceneView.lastActiveSceneView.pivot : Vector3.zero);
                    if (GUILayout.Button("Place in Front of View", GUILayout.Height(30)))
                        PlaceInFrontOfView();
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(Selection.activeTransform == null))
                        if (GUILayout.Button("Place at Selected Position", GUILayout.Height(30)))
                            PlaceAt(Selection.activeTransform.position);

                    GUI.backgroundColor = clickPlacement ? new Color(1f, 0.72f, 0.2f) : Color.white;
                    if (GUILayout.Button(clickPlacement ? "Cancel Scene Placement" : "Click Location in Scene",
                                         GUILayout.Height(30)))
                    {
                        clickPlacement = !clickPlacement;
                        if (clickPlacement) SceneView.lastActiveSceneView?.Focus();
                    }
                    GUI.backgroundColor = Color.white;
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Placed Node Maintenance", EditorStyles.boldLabel);
            GatheringNodeInstance selectedInstance = SelectedInstance();
            using (new EditorGUI.DisabledScope(selectedInstance == null))
            {
                if (GUILayout.Button("Update Selected Node From Open Definition", GUILayout.Height(28)))
                    UpdateInstance(selectedInstance, definition, true);
                if (GUILayout.Button("Frame Selected Node for Testing", GUILayout.Height(28)))
                {
                    Selection.activeGameObject = selectedInstance.gameObject;
                    SceneView.lastActiveSceneView?.FrameSelected();
                }
            }
            if (GUILayout.Button("Update Every Placed Instance of This Definition", GUILayout.Height(28)))
                UpdateAllInstances();
            if (GUILayout.Button("Upgrade Every Gathering Node in Open Scenes", GUILayout.Height(28)))
            {
                int upgraded = UpgradeOpenSceneNodes();
                EditorUtility.DisplayDialog("Node Forge", $"Upgraded {upgraded} gathering node(s).", "OK");
            }

            int instanceCount = Object.FindObjectsByType<GatheringNodeInstance>(
                FindObjectsInactive.Include)
                .Count(instance => instance.definition == definition);
            EditorGUILayout.HelpBox(
                $"Placed in open scenes: {instanceCount}. In Play Mode, approach the node and press F. " +
                "A logged-in character is required to test inventory and profession XP persistence.",
                MessageType.None);
        }

        void DuringSceneGUI(SceneView sceneView)
        {
            if (!clickPlacement || definition == null) return;
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(12, 12, 310, 46), EditorStyles.helpBox);
            GUILayout.Label("Node Forge: click a surface to place. Esc cancels.");
            GUILayout.EndArea();
            Handles.EndGUI();

            Event current = Event.current;
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                clickPlacement = false;
                current.Use();
                Repaint();
                return;
            }
            if (current.type != EventType.MouseDown || current.button != 0 || current.alt) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            Vector3 point;
            if (Physics.Raycast(ray, out RaycastHit hit, 10000f)) point = hit.point;
            else
            {
                Plane plane = new(Vector3.up, Vector3.zero);
                point = plane.Raycast(ray, out float distance) ? ray.GetPoint(distance) : sceneView.pivot;
            }
            PlaceAt(point, false);
            clickPlacement = false;
            current.Use();
            Repaint();
        }

        void PlaceInFrontOfView()
        {
            Camera camera = SceneView.lastActiveSceneView?.camera;
            PlaceAt(camera != null ? camera.transform.position + camera.transform.forward * 5f : Vector3.zero);
        }

        void PlaceAt(Vector3 requestedPosition, bool performGroundSnap = true)
        {
            Vector3 position = requestedPosition;
            if (snapToGround && performGroundSnap)
            {
                Vector3 rayStart = requestedPosition + Vector3.up * 1000f;
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 2000f))
                    position = hit.point;
            }
            position += Vector3.up * groundOffset;

            GameObject root = new($"Node_{SafeName(definition.nodeId)}");
            Undo.RegisterCreatedObjectUndo(root, $"Place {definition.displayName}");
            root.transform.position = position;
            Undo.AddComponent<NetworkIdentity>(root);
            GatheringNodeNetworkState networkState = Undo.AddComponent<GatheringNodeNetworkState>(root);
            networkState.persistentNodeId = GUID.Generate().ToString();
            definition.ApplyTo(networkState);
            GatheringNodeInstance instance = Undo.AddComponent<GatheringNodeInstance>(root);
            instance.definition = definition;
            Undo.AddComponent<AfkGatheringStation>(root);
            Undo.AddComponent<BoxCollider>(root);
            instance.ApplyDefinition();
            RebuildVisual(instance);

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(root.scene);
            SceneView.RepaintAll();
        }

        void UpdateInstance(GatheringNodeInstance instance,
                            GatheringNodeDefinition source, bool rebuildVisual)
        {
            if (instance == null || source == null) return;
            Undo.RecordObject(instance, "Update Gathering Node");
            instance.definition = source;
            GatheringNodeNetworkState networkState = EnsureNetworkState(instance.gameObject);
            source.ApplyTo(networkState);
            AfkGatheringStation station = instance.GetComponent<AfkGatheringStation>();
            if (station != null) Undo.RecordObject(station, "Update Gathering Node");
            BoxCollider collider = instance.GetComponent<BoxCollider>();
            if (collider != null) Undo.RecordObject(collider, "Update Gathering Node");
            instance.ApplyDefinition();
            if (rebuildVisual) RebuildVisual(instance);
            EditorUtility.SetDirty(instance);
            EditorSceneManager.MarkSceneDirty(instance.gameObject.scene);
        }

        void RebuildVisual(GatheringNodeInstance instance)
        {
            if (instance.visualRoot != null)
                Undo.DestroyObjectImmediate(instance.visualRoot);
            else
            {
                Transform existing = instance.transform.Find("Visual");
                if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);
            }

            GameObject visual;
            if (instance.definition.visualPrefab != null)
            {
                visual = PrefabUtility.InstantiatePrefab(
                    instance.definition.visualPrefab, instance.gameObject.scene) as GameObject;
                if (visual == null) return;
                Undo.RegisterCreatedObjectUndo(visual, "Create Node Visual");
            }
            else
            {
                PrimitiveType shape = instance.definition.fallbackShape switch
                {
                    GatheringNodeFallbackShape.Tree => PrimitiveType.Cylinder,
                    GatheringNodeFallbackShape.FishingSpot => PrimitiveType.Quad,
                    _ => PrimitiveType.Cube
                };
                visual = GameObject.CreatePrimitive(shape);
                Undo.RegisterCreatedObjectUndo(visual, "Create Placeholder Node Visual");
                Collider childCollider = visual.GetComponent<Collider>();
                if (childCollider != null) Undo.DestroyObjectImmediate(childCollider);
                Renderer renderer = visual.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit") ??
                                    Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
                    if (shader != null)
                    {
                        Material material = new(shader);
                        material.name = $"{instance.definition.displayName} Placeholder";
                        material.color = instance.definition.fallbackColor;
                        renderer.sharedMaterial = material;
                    }
                }
            }

            visual.name = "Visual";
            visual.transform.SetParent(instance.transform, false);
            visual.transform.localPosition = instance.definition.visualLocalPosition;
            visual.transform.localEulerAngles = instance.definition.visualLocalEulerAngles;
            visual.transform.localScale = instance.definition.visualLocalScale;
            instance.visualRoot = visual;
            EditorUtility.SetDirty(instance);
        }

        void UpdateAllInstances()
        {
            int updated = 0;
            foreach (GatheringNodeInstance instance in Object.FindObjectsByType<GatheringNodeInstance>(
                         FindObjectsInactive.Include))
            {
                if (instance.definition != definition) continue;
                UpdateInstance(instance, definition, true);
                updated++;
            }
            EditorUtility.DisplayDialog("Node Forge", $"Updated {updated} placed node(s).", "OK");
        }

        void RefreshDefinitions()
        {
            definitions.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:GatheringNodeDefinition"))
            {
                GatheringNodeDefinition found = AssetDatabase.LoadAssetAtPath<GatheringNodeDefinition>(
                    AssetDatabase.GUIDToAssetPath(guid));
                if (found != null) definitions.Add(found);
            }
            definitions.Sort((a, b) => string.Compare(a.displayName, b.displayName,
                System.StringComparison.OrdinalIgnoreCase));
            if (definition == null && definitions.Count > 0) definition = definitions[0];
            Repaint();
        }

        bool MatchesFilter(GatheringNodeDefinition candidate)
        {
            if (categoryFilter > 0 && (int)candidate.category != categoryFilter - 1) return false;
            if (string.IsNullOrWhiteSpace(search)) return true;
            string needle = search.Trim();
            return candidate.displayName.Contains(needle, System.StringComparison.OrdinalIgnoreCase) ||
                   candidate.nodeId.Contains(needle, System.StringComparison.OrdinalIgnoreCase) ||
                   candidate.ResolvedItemId.Contains(needle, System.StringComparison.OrdinalIgnoreCase);
        }

        void CreateDefinition()
        {
            EnsureFolder(DefinitionFolder);
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Gathering Node Definition", "new_gathering_node", "asset",
                "Choose where to save the reusable definition.", DefinitionFolder);
            if (string.IsNullOrEmpty(path)) return;
            GatheringNodeDefinition created = CreateInstance<GatheringNodeDefinition>();
            created.nodeId = SafeName(System.IO.Path.GetFileNameWithoutExtension(path)).ToLowerInvariant();
            created.displayName = ObjectNames.NicifyVariableName(created.nodeId);
            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.SaveAssets();
            definition = created;
            RefreshDefinitions();
            Selection.activeObject = created;
        }

        void SaveDefinition()
        {
            List<string> blockers = Validate(definition, true);
            if (blockers.Count > 0)
            {
                EditorUtility.DisplayDialog("Node Forge", string.Join("\n• ", blockers), "OK");
                return;
            }
            definition.nodeId = definition.nodeId.Trim().ToLowerInvariant();
            definition.displayName = definition.displayName.Trim();
            if (definition.yieldItem != null) definition.itemId = definition.yieldItem.itemId;
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            RefreshDefinitions();
        }

        void DeleteDefinition()
        {
            string path = AssetDatabase.GetAssetPath(definition);
            if (string.IsNullOrEmpty(path)) return;
            int useCount = Object.FindObjectsByType<GatheringNodeInstance>(
                FindObjectsInactive.Include)
                .Count(instance => instance.definition == definition);
            if (!EditorUtility.DisplayDialog("Delete Node Definition",
                    $"Delete '{definition.displayName}'?\n\n{useCount} placed node(s) in open scenes reference it.",
                    "Delete", "Cancel")) return;
            AssetDatabase.DeleteAsset(path);
            definition = null;
            RefreshDefinitions();
        }

        static List<string> Validate(GatheringNodeDefinition value, bool blockers)
        {
            List<string> issues = new();
            if (value == null) return issues;
            if (blockers)
            {
                if (!Regex.IsMatch(value.nodeId ?? "", "^[a-z0-9_-]{1,64}$"))
                    issues.Add("Node ID must contain 1–64 lowercase letters, numbers, underscores, or hyphens.");
                if (string.IsNullOrWhiteSpace(value.displayName))
                    issues.Add("Display Name is required.");
                if (string.IsNullOrWhiteSpace(value.ResolvedItemId))
                    issues.Add("Assign a Loot Forge Item or enter an existing Database Item ID.");
                if (value.professionId < 0 || value.professionId > 2)
                    issues.Add("Profession ID must currently be 0 (Woodcutting), 1 (Fishing), or 2 (Mining).");
                if (value.cancelRadius < value.interactionRange)
                    issues.Add("Cancel Radius must be at least as large as Interaction Range.");
                if (value.minimumAwardsPerSpawn < 1 ||
                    value.maximumAwardsPerSpawn < value.minimumAwardsPerSpawn)
                    issues.Add("Award range must be at least 1 and Maximum Awards cannot be below Minimum Awards.");
                if (value.respawnSeconds < 1f) issues.Add("Respawn Seconds must be at least 1.");
            }
            else
            {
                if (value.visualPrefab == null)
                    issues.Add("No Node Visual Prefab is assigned; placements will use the configured placeholder.");
                if (value.yieldItem == null)
                    issues.Add("No Loot Forge Item is assigned. Confirm that the Database Item ID already exists.");
            }
            return issues;
        }

        static GatheringNodeInstance SelectedInstance()
        {
            return Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInParent<GatheringNodeInstance>() : null;
        }

        static GatheringNodeNetworkState EnsureNetworkState(GameObject root)
        {
            if (root.GetComponent<NetworkIdentity>() == null)
                Undo.AddComponent<NetworkIdentity>(root);
            GatheringNodeNetworkState state = root.GetComponent<GatheringNodeNetworkState>();
            if (state == null) state = Undo.AddComponent<GatheringNodeNetworkState>(root);
            if (string.IsNullOrWhiteSpace(state.persistentNodeId))
                state.persistentNodeId = GUID.Generate().ToString();
            return state;
        }

        static int UpgradeOpenSceneNodes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return 0;
            int upgraded = 0;
            foreach (AfkGatheringStation station in Object.FindObjectsByType<AfkGatheringStation>(
                         FindObjectsInactive.Include))
            {
                if (station == null || EditorUtility.IsPersistent(station) ||
                    !station.gameObject.scene.IsValid()) continue;

                bool neededIdentity = station.GetComponent<NetworkIdentity>() == null;
                bool neededState = station.GetComponent<GatheringNodeNetworkState>() == null;
                GatheringNodeNetworkState state = EnsureNetworkState(station.gameObject);
                GatheringNodeInstance instance = station.GetComponent<GatheringNodeInstance>();
                if (instance?.definition != null) instance.definition.ApplyTo(state);
                else
                {
                    state.minimumAwardsPerSpawn = Mathf.Max(1, station.minimumAwardsPerSpawn);
                    state.maximumAwardsPerSpawn = Mathf.Max(
                        state.minimumAwardsPerSpawn, station.maximumAwardsPerSpawn);
                    state.respawnSeconds = Mathf.Max(1f, station.respawnSeconds);
                    state.interactionRange = Mathf.Max(0.5f, station.interactRange);
                    state.minimumSecondsBetweenAwards = Mathf.Max(0.1f, station.tickInterval - 0.25f);
                }
                EditorUtility.SetDirty(state);
                if (neededIdentity || neededState)
                {
                    EditorSceneManager.MarkSceneDirty(station.gameObject.scene);
                    upgraded++;
                }
            }
            return upgraded;
        }

        /// <summary>
        /// One-time batch migration entry point used before release builds. It
        /// serializes the shared Mirror depletion state onto every legacy
        /// gathering node so client and server receive identical scene IDs.
        /// </summary>
        public static void UpgradeAllProjectScenesForBuild()
        {
            int upgraded = 0;
            foreach (string sceneGuid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Game/Scenes" }))
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                UnityEngine.SceneManagement.Scene scene =
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int sceneUpgrades = UpgradeOpenSceneNodes();
                if (sceneUpgrades <= 0) continue;
                EditorSceneManager.SaveScene(scene);
                upgraded += sceneUpgrades;
                Debug.Log($"[Node Forge] Upgraded and saved {sceneUpgrades} gathering node(s) in {scenePath}.");
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Node Forge] Scene migration complete: {upgraded} gathering node(s) upgraded.");
        }

        static void Draw(SerializedObject serialized, string property, string label)
        {
            SerializedProperty found = serialized.FindProperty(property);
            if (found != null) EditorGUILayout.PropertyField(found, new GUIContent(label), true);
        }

        static void DrawVisualPreview(GameObject prefab)
        {
            if (prefab == null) return;
            Texture2D preview = AssetPreview.GetAssetPreview(prefab) ?? AssetPreview.GetMiniThumbnail(prefab);
            if (preview == null) return;
            Rect rect = GUILayoutUtility.GetRect(96f, 96f, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit, true);
        }

        static string SafeName(string value)
        {
            string cleaned = Regex.Replace(value ?? "node", "[^a-zA-Z0-9_-]+", "_").Trim('_');
            return string.IsNullOrWhiteSpace(cleaned) ? "node" : cleaned;
        }

        static void EnsureFolder(string folder)
        {
            string current = "Assets";
            foreach (string part in folder.Substring("Assets/".Length).Split('/'))
            {
                string next = $"{current}/{part}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part);
                current = next;
            }
        }
    }
}
#endif
