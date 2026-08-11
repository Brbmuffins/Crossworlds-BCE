#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Crossworlds.EditorTools.LootForge
{
    /// <summary>Previews and authors the same local equipment position used at runtime.</summary>
    internal sealed class LootForgeEquipmentPreviewWindow : EditorWindow
    {
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
        GameObject characterPrefab;
        GameObject itemPrefab;
        int classIndex;
        Vector3 localPosition;
        float cameraYaw;
        float cameraPitch = 6f;
        float cameraZoom = 1f;
        bool orbiting;

        public static void Open(LootItemDefinition activeDefinition)
        {
            var window = GetWindow<LootForgeEquipmentPreviewWindow>(
                false, "Loot Equipment Preview", true);
            window.definition = activeDefinition;
            window.localPosition = activeDefinition != null
                ? activeDefinition.EffectiveEquippedLocalPosition : Vector3.zero;
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
                localPosition = definition != null
                    ? definition.EffectiveEquippedLocalPosition : Vector3.zero;
                RebuildPreview();
            }

            int nextClass = EditorGUILayout.Popup("Preview Class", classIndex, ClassNames);
            if (nextClass != classIndex)
            {
                classIndex = nextClass;
                LoadDefaultCharacter();
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

            EnsurePreview(nextItem);
            Rect previewRect = GUILayoutUtility.GetRect(
                100f, Mathf.Max(300f, position.height - 220f), GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, new Color(0.08f, 0.08f, 0.08f, 1f));
            HandleCamera(previewRect);
            DrawPreview(previewRect);

            EditorGUILayout.LabelField(
                "Drag the preview to rotate; use the mouse wheel to zoom.", EditorStyles.miniLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Front")) SetView(0f);
                if (GUILayout.Button("Left")) SetView(90f);
                if (GUILayout.Button("Back")) SetView(180f);
                if (GUILayout.Button("Right")) SetView(270f);
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Equipped Item Position", EditorStyles.boldLabel);
            DrawAxisSlider("X", ref localPosition.x);
            DrawAxisSlider("Y", ref localPosition.y);
            DrawAxisSlider("Z", ref localPosition.z);
            localPosition = EditorGUILayout.Vector3Field("Exact X / Y / Z", localPosition);
            ApplyPositionToPreview();

            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.45f);
            if (GUILayout.Button("Update and Save Position", GUILayout.Height(34f))) SavePosition();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.HelpBox(SaveDestinationText(), MessageType.None);
        }

        void DrawAxisSlider(string label, ref float value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label, GUILayout.Width(28f));
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
            preview.AddSingleGO(characterInstance);

            Transform anchor = ResolveAnchor(characterInstance.transform, definition);
            itemInstance = Instantiate(nextItem, anchor, false);
            itemInstance.name = nextItem.name + "_LootForgePreview";
            DisableBehaviours(itemInstance);
            itemInstance.transform.localRotation =
                Quaternion.Euler(definition.EffectiveEquippedLocalEulerAngles);
            Vector3 scale = definition.EffectiveEquippedLocalScale;
            itemInstance.transform.localScale = scale.sqrMagnitude > 0.0001f ? scale : Vector3.one;
            itemPrefab = nextItem;
            ApplyPositionToPreview();
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

        void HandleCamera(Rect rect)
        {
            Event current = Event.current;
            if (!rect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseUp) orbiting = false;
                return;
            }
            if (current.type == EventType.MouseDown && (current.button == 0 || current.button == 1))
            {
                orbiting = true;
                current.Use();
            }
            else if (current.type == EventType.MouseDrag && orbiting)
            {
                cameraYaw += current.delta.x * 0.55f;
                cameraPitch = Mathf.Clamp(cameraPitch - current.delta.y * 0.4f, -70f, 70f);
                current.Use();
            }
            else if (current.type == EventType.MouseUp)
            {
                orbiting = false;
                current.Use();
            }
            else if (current.type == EventType.ScrollWheel)
            {
                cameraZoom = Mathf.Clamp(cameraZoom + current.delta.y * 0.05f, 0.55f, 2.2f);
                current.Use();
            }
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Orbit);
        }

        void SetView(float yaw)
        {
            cameraYaw = yaw;
            cameraPitch = 6f;
            Repaint();
        }

        void ApplyPositionToPreview()
        {
            if (itemInstance != null) itemInstance.transform.localPosition = localPosition;
        }

        void SavePosition()
        {
            if (definition.attachmentProfile != null && !definition.overrideAttachmentProfile)
            {
                Undo.RecordObject(definition.attachmentProfile, "Update Loot Equipment Position");
                definition.attachmentProfile.localPosition = localPosition;
                EditorUtility.SetDirty(definition.attachmentProfile);
            }
            else
            {
                Undo.RecordObject(definition, "Update Loot Equipment Position");
                definition.equippedLocalPosition = localPosition;
                EditorUtility.SetDirty(definition);
            }
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent("Equipment position saved"));
        }

        string SaveDestinationText()
        {
            if (definition == null) return "";
            return definition.attachmentProfile != null && !definition.overrideAttachmentProfile
                ? $"Updates shared profile: {definition.attachmentProfile.name}"
                : "Updates this loot item's equipped position.";
        }

        void LoadDefaultCharacter()
        {
            classIndex = Mathf.Clamp(classIndex, 0, DefaultPrefabPaths.Length - 1);
            characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabPaths[classIndex]);
        }

        GameObject ResolveItemPrefab() => definition == null ? null :
            (definition.equippedVisualPrefab != null
                ? definition.equippedVisualPrefab : definition.worldVisualPrefab);

        static Transform ResolveAnchor(Transform root, LootItemDefinition item)
        {
            string exactName = item.EffectiveAttachmentBoneName;
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

        static Bounds CalculateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = new Bounds(root.transform.position, Vector3.one);
            bool found = false;
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.enabled) continue;
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
            itemPrefab = null;
        }
    }
}
#endif
