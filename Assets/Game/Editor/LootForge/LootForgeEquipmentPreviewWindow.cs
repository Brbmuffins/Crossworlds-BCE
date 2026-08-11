#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Crossworlds.EditorTools.LootForge
{
    /// <summary>Previews and authors the same local equipment position used at runtime.</summary>
    internal sealed class LootForgeEquipmentPreviewWindow : EditorWindow
    {
        enum TransformToolMode { Move, Rotate }

        static readonly string[] ClassNames =
            { "Marauder", "Templar", "Night Hunter", "Cleric", "Arcanist" };
        static readonly string[] DefaultPrefabPaths =
        {
            "Assets/Game/Game_Prefabs/Marauder.prefab",
            "Assets/Game/Game_Prefabs/Ironclad.prefab",
            "Assets/Game/Game_Prefabs/Shadowblade.prefab",
            "Assets/Game/Game_Prefabs/Cleric.prefab",
            "Assets/Game/Game_Prefabs/Arcanist.prefab"
        };

        LootItemDefinition definition;
        PreviewRenderUtility preview;
        GameObject characterInstance;
        GameObject itemInstance;
        Transform attachmentAnchor;
        GameObject characterPrefab;
        GameObject itemPrefab;
        int classIndex;
        Vector3 localPosition;
        Vector3 localEulerAngles;
        TransformToolMode transformTool = TransformToolMode.Move;
        float cameraYaw;
        float cameraPitch = 6f;
        float cameraZoom = 1f;
        bool orbiting;
        bool transforming;
        PivotRotation handleOrientation = PivotRotation.Local;

        public static void Open(LootItemDefinition activeDefinition)
        {
            var window = GetWindow<LootForgeEquipmentPreviewWindow>(
                false, "Loot Equipment Preview", true);
            window.definition = activeDefinition;
            window.LoadClassTransform();
            window.LoadDefaultCharacter();
            window.minSize = new Vector2(480f, 540f);
            window.Show();
            window.Focus();
        }

        void OnDisable() => Cleanup();
        void Update() => Repaint();

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            definition = (LootItemDefinition)EditorGUILayout.ObjectField(
                "Loot Definition", definition, typeof(LootItemDefinition), false);
            if (EditorGUI.EndChangeCheck())
            {
                LoadClassTransform();
                RebuildPreview();
            }

            int nextClass = EditorGUILayout.Popup("Preview Class", classIndex, ClassNames);
            if (nextClass != classIndex)
            {
                classIndex = nextClass;
                LoadDefaultCharacter();
                LoadClassTransform();
                RebuildPreview();
            }
            EditorGUI.BeginChangeCheck();
            characterPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Character Prefab", characterPrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck()) RebuildPreview();

            GameObject nextItem = ResolveItemPrefab();
            if (definition == null || nextItem == null || characterPrefab == null)
            {
                EditorGUILayout.HelpBox(
                    "Select an equipment definition with a visual prefab and a character prefab.",
                    MessageType.Info);
                Cleanup();
                return;
            }

            EditorGUILayout.LabelField("Equipment Transform Tool", EditorStyles.boldLabel);
            transformTool = (TransformToolMode)GUILayout.Toolbar(
                (int)transformTool, new[] { "Move (W)", "Rotate (E)" },
                GUILayout.Height(26f));
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Handle Orientation", GUILayout.Width(116f));
                handleOrientation = (PivotRotation)GUILayout.Toolbar(
                    (int)handleOrientation, new[] { "Global", "Local" });
            }
            HandleToolShortcuts();
            EditorGUILayout.LabelField(
                transformTool == TransformToolMode.Move
                    ? "Drag a colored axis or plane handle to position the item."
                    : "Drag a colored rotation ring to orient the item.",
                EditorStyles.miniLabel);
            DrawSaveTransformButton();

            EnsurePreview(nextItem);
            Rect previewRect = GUILayoutUtility.GetRect(
                100f, Mathf.Max(260f, position.height - 285f), GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, new Color(0.08f, 0.08f, 0.08f, 1f));
            DrawPreview(previewRect);
            DrawTransformHandle(previewRect);
            HandleCamera(previewRect);
            HandleDirectTransformDrag(previewRect);

            EditorGUILayout.LabelField(
                "Right-drag or Alt+left-drag to orbit; mouse wheel zooms. " +
                "Left-drag the gizmo or empty viewport to edit the item.",
                EditorStyles.miniLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Front")) SetView(0f);
                if (GUILayout.Button("Left")) SetView(90f);
                if (GUILayout.Button("Back")) SetView(180f);
                if (GUILayout.Button("Right")) SetView(270f);
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Equipped Item Position", EditorStyles.miniBoldLabel);
            DrawAxisSlider("X", ref localPosition.x);
            DrawAxisSlider("Y", ref localPosition.y);
            DrawAxisSlider("Z", ref localPosition.z);
            localPosition = EditorGUILayout.Vector3Field("Exact X / Y / Z", localPosition);
            localEulerAngles = EditorGUILayout.Vector3Field(
                "Rotation X / Y / Z", localEulerAngles);
            ApplyTransformToPreview();
        }

        void DrawSaveTransformButton()
        {
            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.45f);
            if (GUILayout.Button("Update and Save Transform", GUILayout.Height(34f)))
                SaveTransform();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.HelpBox(SaveDestinationText(), MessageType.None);
        }

        void DrawAxisSlider(string label, ref float value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(28f));
                value = GUILayout.HorizontalSlider(value, -2f, 2f);
                value = EditorGUILayout.FloatField(value, GUILayout.Width(70f));
            }
        }

        void EnsurePreview(GameObject nextItem)
        {
            if (preview != null && characterInstance != null && itemInstance != null &&
                itemPrefab == nextItem) return;
            RebuildPreview();
        }

        void RebuildPreview()
        {
            Cleanup();
            GameObject nextItem = ResolveItemPrefab();
            if (definition == null || characterPrefab == null || nextItem == null) return;

            preview = new PreviewRenderUtility();
            preview.camera.fieldOfView = 30f;
            preview.camera.clearFlags = CameraClearFlags.Color;
            preview.camera.backgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f);
            preview.lights[0].intensity = 1.25f;
            preview.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
            preview.lights[1].intensity = 1f;

            characterInstance = Instantiate(characterPrefab);
            characterInstance.name = characterPrefab.name + "_LootForgePreview";
            DisableBehaviours(characterInstance);
            FreezeCharacterPose(characterInstance);
            characterInstance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            preview.AddSingleGO(characterInstance);

            attachmentAnchor = ResolveAnchor(characterInstance.transform, definition, classIndex);
            // Keep the equipment independent while authoring, like a separate
            // GameObject in a normal scene. Its world pose is converted to the
            // attachment anchor's local pose for runtime when values are saved.
            itemInstance = Instantiate(nextItem);
            itemInstance.name = nextItem.name + "_LootForgePreview";
            DisableBehaviours(itemInstance);
            Vector3 scale = definition.EffectiveEquippedLocalScaleForClass(classIndex);
            Vector3 authoredScale = scale.sqrMagnitude > 0.0001f ? scale : Vector3.one;
            itemInstance.transform.localScale = Vector3.Scale(
                attachmentAnchor.lossyScale, authoredScale);
            preview.AddSingleGO(itemInstance);
            itemPrefab = nextItem;
            ApplyTransformToPreview();
        }

        void DrawPreview(Rect rect)
        {
            if (preview == null || characterInstance == null) return;
            Bounds bounds = CalculateBounds(characterInstance);
            Vector3 center = bounds.center;
            float radius = Mathf.Max(bounds.extents.magnitude, 0.5f);
            Quaternion orbit = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
            preview.camera.transform.position =
                center + orbit * new Vector3(0f, radius * 0.12f, radius * 2.7f * cameraZoom);
            preview.camera.transform.LookAt(center);
            preview.camera.aspect = Mathf.Max(0.1f, rect.width / Mathf.Max(1f, rect.height));
            preview.camera.nearClipPlane = Mathf.Max(0.01f, radius * 0.01f);
            preview.camera.farClipPlane = radius * 8f;
            preview.BeginPreview(rect, GUIStyle.none);
            preview.Render(true);
            GUI.DrawTexture(rect, preview.EndPreview(), ScaleMode.StretchToFill, false);
        }

        void DrawTransformHandle(Rect rect)
        {
            if (preview == null || itemInstance == null || Event.current.type == EventType.ScrollWheel)
                return;

            Handles.SetCamera(rect, preview.camera);
            EditorGUI.BeginChangeCheck();
            if (transformTool == TransformToolMode.Move)
            {
                Quaternion handleRotation = handleOrientation == PivotRotation.Local
                    ? itemInstance.transform.rotation
                    : Quaternion.identity;
                Vector3 worldPosition = Handles.PositionHandle(
                    itemInstance.transform.position, handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Move Loot Preview Item");
                    localPosition = attachmentAnchor != null
                        ? attachmentAnchor.InverseTransformPoint(worldPosition) : worldPosition;
                    ApplyTransformToPreview();
                    Repaint();
                }
            }
            else
            {
                Quaternion worldRotation = Handles.RotationHandle(
                    itemInstance.transform.rotation, itemInstance.transform.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(this, "Rotate Loot Preview Item");
                    Quaternion parentRotation = attachmentAnchor != null
                        ? attachmentAnchor.rotation : Quaternion.identity;
                    localEulerAngles = (Quaternion.Inverse(parentRotation) * worldRotation).eulerAngles;
                    ApplyTransformToPreview();
                    Repaint();
                }
            }
        }

        void HandleCamera(Rect rect)
        {
            Event current = Event.current;
            int controlId = GUIUtility.GetControlID(
                "LootForgeCameraOrbit".GetHashCode(), FocusType.Passive, rect);
            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != controlId) return;
            if (!rect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseUp && orbiting)
                {
                    orbiting = false;
                    if (GUIUtility.hotControl == controlId) GUIUtility.hotControl = 0;
                }
                return;
            }
            bool orbitButton = current.button == 1 || current.button == 2 ||
                               (current.button == 0 && current.alt);
            if (current.type == EventType.MouseDown && orbitButton)
            {
                orbiting = true;
                GUIUtility.hotControl = controlId;
                current.Use();
            }
            else if (current.type == EventType.MouseDrag && orbiting &&
                     GUIUtility.hotControl == controlId)
            {
                cameraYaw += current.delta.x * 0.55f;
                cameraPitch = Mathf.Clamp(cameraPitch - current.delta.y * 0.4f, -70f, 70f);
                current.Use();
                Repaint();
            }
            else if (current.type == EventType.MouseUp && orbiting)
            {
                orbiting = false;
                GUIUtility.hotControl = 0;
                current.Use();
            }
            else if (current.type == EventType.ScrollWheel)
            {
                cameraZoom = Mathf.Clamp(cameraZoom + current.delta.y * 0.05f, 0.55f, 2.2f);
                current.Use();
            }
        }

        void HandleDirectTransformDrag(Rect rect)
        {
            Event current = Event.current;
            int controlId = GUIUtility.GetControlID(
                "LootForgeTransformDrag".GetHashCode(), FocusType.Passive, rect);
            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != controlId) return;
            if (current.type == EventType.MouseDown && current.button == 0 && !current.alt &&
                rect.Contains(current.mousePosition))
            {
                transforming = true;
                GUIUtility.hotControl = controlId;
                current.Use();
            }
            else if (current.type == EventType.MouseDrag && transforming &&
                     GUIUtility.hotControl == controlId)
            {
                if (transformTool == TransformToolMode.Move)
                    ApplyMouseMove(current.delta, current.shift);
                else
                    ApplyMouseRotation(current.delta, current.shift);
                ApplyTransformToPreview();
                current.Use();
                Repaint();
            }
            else if (current.type == EventType.MouseUp && transforming)
            {
                transforming = false;
                GUIUtility.hotControl = 0;
                current.Use();
                Repaint();
            }
        }

        void ApplyMouseMove(Vector2 delta, bool depthMode)
        {
            if (preview == null || itemInstance == null) return;
            Bounds bounds = CalculateBounds(characterInstance);
            float sensitivity = Mathf.Max(0.0005f,
                bounds.extents.magnitude * cameraZoom * 0.0025f);
            Vector3 worldDelta = depthMode
                ? preview.camera.transform.right * (delta.x * sensitivity) +
                  preview.camera.transform.forward * (-delta.y * sensitivity)
                : preview.camera.transform.right * (delta.x * sensitivity) +
                  preview.camera.transform.up * (-delta.y * sensitivity);
            localPosition += attachmentAnchor != null
                ? attachmentAnchor.InverseTransformVector(worldDelta) : worldDelta;
        }

        void ApplyMouseRotation(Vector2 delta, bool rollMode)
        {
            const float degreesPerPixel = 0.55f;
            if (rollMode)
                localEulerAngles.z -= delta.x * degreesPerPixel;
            else
            {
                localEulerAngles.y -= delta.x * degreesPerPixel;
                localEulerAngles.x += delta.y * degreesPerPixel;
            }
            localEulerAngles.x = Mathf.Repeat(localEulerAngles.x, 360f);
            localEulerAngles.y = Mathf.Repeat(localEulerAngles.y, 360f);
            localEulerAngles.z = Mathf.Repeat(localEulerAngles.z, 360f);
        }

        void HandleToolShortcuts()
        {
            Event current = Event.current;
            if (current.type != EventType.KeyDown) return;
            if (current.keyCode == KeyCode.W)
            {
                transformTool = TransformToolMode.Move;
                current.Use();
                Repaint();
            }
            else if (current.keyCode == KeyCode.E)
            {
                transformTool = TransformToolMode.Rotate;
                current.Use();
                Repaint();
            }
        }

        void SetView(float yaw)
        {
            cameraYaw = yaw;
            cameraPitch = 6f;
            Repaint();
        }

        void ApplyTransformToPreview()
        {
            if (itemInstance == null) return;
            if (attachmentAnchor != null)
            {
                itemInstance.transform.SetPositionAndRotation(
                    attachmentAnchor.TransformPoint(localPosition),
                    attachmentAnchor.rotation * Quaternion.Euler(localEulerAngles));
            }
            else
            {
                itemInstance.transform.SetPositionAndRotation(
                    localPosition, Quaternion.Euler(localEulerAngles));
            }
        }

        void SaveTransform()
        {
            UnityEngine.Object saveTarget = definition.attachmentProfile != null &&
                                            !definition.overrideAttachmentProfile
                ? definition.attachmentProfile : definition;
            Undo.RecordObject(saveTarget, "Update Class Equipment Transform");
            EquipmentAttachmentClassOverride classTransform =
                definition.GetOrCreateClassOverride(classIndex);
            classTransform.localPosition = localPosition;
            classTransform.localEulerAngles = localEulerAngles;
            EditorUtility.SetDirty(saveTarget);
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent("Equipment transform saved"));
        }

        string SaveDestinationText()
        {
            if (definition == null) return "";
            return definition.attachmentProfile != null && !definition.overrideAttachmentProfile
                ? $"Saves {ClassNames[classIndex]} only in shared profile: {definition.attachmentProfile.name}"
                : $"Saves a {ClassNames[classIndex]} override on this loot item.";
        }

        void LoadClassTransform()
        {
            localPosition = definition != null
                ? definition.EffectiveEquippedLocalPositionForClass(classIndex) : Vector3.zero;
            localEulerAngles = definition != null
                ? definition.EffectiveEquippedLocalEulerAnglesForClass(classIndex) : Vector3.zero;
        }

        void LoadDefaultCharacter()
        {
            classIndex = Mathf.Clamp(classIndex, 0, DefaultPrefabPaths.Length - 1);
            characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabPaths[classIndex]);
        }

        GameObject ResolveItemPrefab() => definition == null ? null :
            (definition.equippedVisualPrefab != null
                ? definition.equippedVisualPrefab : definition.worldVisualPrefab);

        static Transform ResolveAnchor(Transform root, LootItemDefinition item, int previewClassIndex)
        {
            string exactName = item.EffectiveAttachmentBoneNameForClass(previewClassIndex);
            if (!string.IsNullOrWhiteSpace(exactName))
            {
                Transform exact = FindTransform(root, exactName);
                if (exact != null) return exact;
            }
            string[] aliases = item.equipmentSlot switch
            {
                LootEquipmentSlot.MainHand => new[] { "RightHand", "Hand_R", "mixamorig:RightHand", "Bip001 R Hand", "r_hand" },
                LootEquipmentSlot.OffHand => new[] { "LeftHand", "Hand_L", "mixamorig:LeftHand", "Bip001 L Hand", "l_hand" },
                LootEquipmentSlot.Head => new[] { "Head", "mixamorig:Head", "Bip001 Head", "head" },
                LootEquipmentSlot.Chest => new[] { "Chest", "UpperChest", "Spine2", "mixamorig:Spine2" },
                LootEquipmentSlot.Hands => new[] { "RightHand", "Hand_R", "mixamorig:RightHand" },
                LootEquipmentSlot.Legs => new[] { "Hips", "Pelvis", "mixamorig:Hips" },
                LootEquipmentSlot.Feet => new[] { "RightFoot", "Foot_R", "mixamorig:RightFoot" },
                _ => Array.Empty<string>()
            };
            foreach (string alias in aliases)
            {
                Transform found = FindTransform(root, alias);
                if (found != null) return found;
            }
            return root;
        }

        static Transform FindTransform(Transform root, string name)
        {
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
                if (string.Equals(candidate.name, name, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            return null;
        }

        static void DisableBehaviours(GameObject root)
        {
            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour != null) behaviour.enabled = false;
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
        }

        static void FreezeCharacterPose(GameObject root)
        {
            foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
            {
                animator.applyRootMotion = false;
                if (animator.runtimeAnimatorController != null)
                {
                    animator.Rebind();
                    animator.Update(0f);
                }
                animator.speed = 0f;
                animator.enabled = false;
            }
            foreach (Animation animation in root.GetComponentsInChildren<Animation>(true))
            {
                animation.Stop();
                animation.enabled = false;
            }
        }

        Bounds CalculateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = new Bounds(root.transform.position, Vector3.one);
            bool found = false;
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.enabled) continue;
                // The equipped visual is parented beneath the character's hand.
                // Excluding it keeps camera framing locked to the player while
                // the item is moved far away or rotated during authoring.
                if (itemInstance != null &&
                    (renderer.transform == itemInstance.transform ||
                     renderer.transform.IsChildOf(itemInstance.transform)))
                    continue;
                if (!found) { bounds = renderer.bounds; found = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        void Cleanup()
        {
            if (preview != null) preview.Cleanup();
            preview = null;
            characterInstance = null;
            itemInstance = null;
            attachmentAnchor = null;
            itemPrefab = null;
        }
    }
}
#endif
