#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Crossworlds.EditorTools.LootForge
{
    public sealed class LootForgeWindow : EditorWindow
    {
        const string ItemFolder = "Assets/Game/Resources/LootForge/Items";
        LootItemDefinition definition;
        DropTable dropTable;
        float dropWeight = 1f;
        int minimumQuantity = 1;
        int maximumQuantity = 1;
        Vector2 scroll;

        [MenuItem("BCE/Loot Forge", priority = 36)]
        static void Open() => GetWindow<LootForgeWindow>("Loot Forge");

        [OnOpenAsset]
        static bool OpenLootDefinition(EntityId entityId, int line)
        {
            LootItemDefinition selected = AssetDatabase.LoadAssetAtPath<LootItemDefinition>(
                AssetDatabase.GetAssetPath(entityId));
            if (selected == null) return false;
            LootForgeWindow window = GetWindow<LootForgeWindow>("Loot Forge");
            window.definition = selected;
            window.Show();
            window.Focus();
            return true;
        }

        void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Loot Forge", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Create one reusable loot definition for the database record, inventory icon, rarity, and world visual. " +
                "Enemy Forge continues to assign the Drop Table, Loot Visual/Pickup Prefab, and Loot Beam to enemies.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                definition = (LootItemDefinition)EditorGUILayout.ObjectField(
                    "Loot Definition", definition, typeof(LootItemDefinition), false);
                if (GUILayout.Button("New", GUILayout.Width(70)))
                {
                    definition = CreateInstance<LootItemDefinition>();
                    definition.databaseItemType = LootDatabaseItemType.Unspecified;
                    definition.equipmentSlot = LootEquipmentSlot.None;
                }
            }

            if (definition == null)
            {
                EditorGUILayout.HelpBox("Create or select a Loot Item Definition.", MessageType.None);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            var serialized = new SerializedObject(definition);
            serialized.Update();
            Draw(serialized, "itemId", "Database Item ID");
            Draw(serialized, "displayName", "Display Name");
            SerializedProperty itemType = serialized.FindProperty("databaseItemType");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(itemType, new GUIContent("Database Item Type"));
            if (EditorGUI.EndChangeCheck())
            {
                LootDatabaseItemType selected =
                    (LootDatabaseItemType)itemType.enumValueIndex;
                ApplyTypeDefaults(serialized, selected);
            }
            Draw(serialized, "rarity", "Rarity");
            Draw(serialized, "sellValue", "Sell Value");
            Draw(serialized, "crafted", "Crafted Item");
            Draw(serialized, "stackable", "Stackable");
            if (serialized.FindProperty("stackable").boolValue)
                Draw(serialized, "maxStackSize", "Maximum Stack Size");
            Draw(serialized, "inventoryIcon", "Inventory Icon");
            Draw(serialized, "inventoryIconEulerAngles", "Icon Preview Rotation");
            Draw(serialized, "inventoryIconZoom", "Icon Preview Zoom");
            Draw(serialized, "worldVisualPrefab", "World Visual Prefab");
            if (IsEquipment((LootDatabaseItemType)itemType.enumValueIndex))
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Equipment", EditorStyles.boldLabel);
                Draw(serialized, "equipmentSlot", "Equipment Slot");
                Draw(serialized, "equippedVisualPrefab", "Equipped Visual Prefab");
                if ((LootEquipmentSlot)serialized.FindProperty("equipmentSlot").enumValueIndex ==
                    LootEquipmentSlot.MainHand)
                    Draw(serialized, "twoHanded", "Two Handed Weapon");
                Draw(serialized, "attachmentProfile", "Attachment Profile");
                if (GUILayout.Button("Apply Recommended Attachment Profile"))
                {
                    serialized.ApplyModifiedProperties();
                    EnsureRecommendedAttachmentProfile(true);
                    serialized.Update();
                }
                using (new EditorGUI.DisabledScope(
                           definition.equippedVisualPrefab == null && definition.worldVisualPrefab == null))
                {
                    if (GUILayout.Button("Open Equipment Position Preview", GUILayout.Height(28)))
                    {
                        serialized.ApplyModifiedProperties();
                        LootForgeEquipmentPreviewWindow.Open(definition);
                        serialized.Update();
                    }
                }
                if (GUILayout.Button("Create Shared Profile From Current Transform"))
                {
                    serialized.ApplyModifiedProperties();
                    CreateAttachmentProfile();
                    serialized.Update();
                }
                if (serialized.FindProperty("attachmentProfile").objectReferenceValue != null)
                    Draw(serialized, "overrideAttachmentProfile", "Override Profile Transform");
                if (serialized.FindProperty("attachmentProfile").objectReferenceValue == null ||
                    serialized.FindProperty("overrideAttachmentProfile").boolValue)
                {
                    Draw(serialized, "attachmentBoneName", "Attachment Bone Override");
                    Draw(serialized, "equippedLocalPosition", "Equipped Position");
                    Draw(serialized, "equippedLocalEulerAngles", "Equipped Rotation");
                    Draw(serialized, "equippedLocalScale", "Equipped Scale");
                }
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Stat Bonuses", EditorStyles.miniBoldLabel);
                Draw(serialized, "bonusStrength", "Strength");
                Draw(serialized, "bonusAgility", "Agility");
                Draw(serialized, "bonusIntelligence", "Intelligence");
                Draw(serialized, "bonusVitality", "Vitality");
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Tooltip Spell Bonuses", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(
                    serialized.FindProperty("spellBonusDescriptions"),
                    new GUIContent("Spell Bonus Lines"), true);
            }
            EditorGUILayout.HelpBox(
                "World Visual Prefab is optional. Leave it empty for inventory-only items such as tickets, " +
                "wood, crafting materials, or consumables. The enemy's assigned network-ready pickup prefab " +
                "will be used as the world representation.",
                MessageType.None);
            serialized.ApplyModifiedProperties();
            NormalizeVisualPrefabReferences();
            EnsureRecommendedAttachmentProfile(false);

            using (new EditorGUI.DisabledScope(
                       definition.worldVisualPrefab == null && definition.equippedVisualPrefab == null))
            {
                if (GUILayout.Button(definition.inventoryIcon == null
                        ? "Generate Missing Inventory PNG"
                        : "Regenerate Inventory PNG", GUILayout.Height(28)))
                    GenerateInventoryIcon(true);
            }

            if (definition.inventoryIcon != null)
            {
                Texture2D preview = AssetPreview.GetAssetPreview(definition.inventoryIcon) ??
                                    AssetPreview.GetMiniThumbnail(definition.inventoryIcon);
                if (preview != null)
                    GUILayout.Label(preview, GUILayout.Width(96), GUILayout.Height(96));
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Drop Table Assignment", EditorStyles.boldLabel);
            dropTable = (DropTable)EditorGUILayout.ObjectField(
                "Existing Drop Table", dropTable, typeof(DropTable), false);
            dropWeight = Mathf.Max(0f, EditorGUILayout.FloatField("Drop Weight", dropWeight));
            minimumQuantity = Mathf.Max(1, EditorGUILayout.IntField("Minimum Quantity", minimumQuantity));
            maximumQuantity = Mathf.Max(minimumQuantity,
                EditorGUILayout.IntField("Maximum Quantity", maximumQuantity));
            EditorGUILayout.HelpBox(
                "This adds or updates one entry. It does not change the table's Nothing Weight, gold, or other loot entries.",
                MessageType.None);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Validate Loot", GUILayout.Height(28))) ShowValidation();
            if (GUILayout.Button("Save Loot Definition", GUILayout.Height(32))) SaveDefinition(true);

            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.45f);
            using (new EditorGUI.DisabledScope(dropTable == null))
                if (GUILayout.Button("Save and Assign to Drop Table", GUILayout.Height(38)))
                    SaveAndAssign();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.HelpBox(
                "Database synchronization is queued automatically. The live items table is updated when a server build " +
                "containing this definition is deployed and starts.", MessageType.Info);

            if (definition.databaseItemType != LootDatabaseItemType.Unspecified)
                EditorGUILayout.HelpBox(BuildDeploymentSummary(), MessageType.None);
        }

        static void Draw(SerializedObject serialized, string property, string label)
        {
            SerializedProperty value = serialized.FindProperty(property);
            if (value != null) EditorGUILayout.PropertyField(value, new GUIContent(label), true);
        }

        void ShowValidation()
        {
            List<string> issues = ValidateDefinition(dropTableRequired: false);
            EditorUtility.DisplayDialog("Loot Forge Validation",
                issues.Count == 0 ? "Loot passed validation." : string.Join("\n• ", issues), "OK");
        }

        bool SaveDefinition(bool showConfirmation)
        {
            if (definition.inventoryIcon == null && !GenerateInventoryIcon(false)) return false;
            List<string> issues = ValidateDefinition(dropTableRequired: false);
            if (issues.Count > 0)
            {
                EditorUtility.DisplayDialog("Loot Forge", "Correct these issues:\n• " +
                    string.Join("\n• ", issues), "OK");
                return false;
            }

            EnsureFolder(ItemFolder);
            definition.itemId = definition.itemId.Trim().ToLowerInvariant();
            definition.displayName = definition.displayName.Trim();
            string iconPath = AssetDatabase.GetAssetPath(definition.inventoryIcon);
            definition.iconId = string.IsNullOrEmpty(iconPath)
                ? null : AssetDatabase.AssetPathToGUID(iconPath);
            string desiredPath = $"{ItemFolder}/{SafeFileName(definition.itemId)}.asset";
            string currentPath = AssetDatabase.GetAssetPath(definition);
            LootItemDefinition collision =
                AssetDatabase.LoadAssetAtPath<LootItemDefinition>(desiredPath);
            if (collision != null && collision != definition)
            {
                EditorUtility.DisplayDialog("Loot Forge",
                    $"Another loot definition already owns '{definition.itemId}'.", "OK");
                return false;
            }
            if (string.IsNullOrEmpty(currentPath)) AssetDatabase.CreateAsset(definition, desiredPath);
            else if (!string.Equals(currentPath, desiredPath,
                         System.StringComparison.OrdinalIgnoreCase))
            {
                string error = AssetDatabase.MoveAsset(currentPath, desiredPath);
                if (!string.IsNullOrEmpty(error))
                {
                    EditorUtility.DisplayDialog("Loot Forge", error, "OK");
                    return false;
                }
            }
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            Selection.activeObject = definition;
            if (showConfirmation)
                EditorUtility.DisplayDialog("Loot Forge",
                    $"Saved '{definition.displayName}'. Database sync is queued for the next server deployment.", "OK");
            return true;
        }

        void SaveAndAssign()
        {
            if (definition.inventoryIcon == null && !GenerateInventoryIcon(false)) return;
            List<string> issues = ValidateDefinition(dropTableRequired: true);
            if (issues.Count > 0)
            {
                EditorUtility.DisplayDialog("Loot Forge", "Correct these issues:\n• " +
                    string.Join("\n• ", issues), "OK");
                return;
            }
            if (!SaveDefinition(false)) return;

            Undo.RecordObject(dropTable, "Assign Loot Forge Item");
            DropEntry entry = null;
            foreach (DropEntry candidate in dropTable.drops)
                if (candidate != null &&
                    (candidate.itemDefinition == definition || string.Equals(
                        candidate.ResolvedItemId, definition.itemId,
                        System.StringComparison.OrdinalIgnoreCase)))
                { entry = candidate; break; }
            if (entry == null)
            {
                entry = new DropEntry();
                dropTable.drops.Add(entry);
            }
            entry.itemDefinition = definition;
            entry.itemId = "";
            entry.weight = dropWeight;
            entry.minQty = minimumQuantity;
            entry.maxQty = maximumQuantity;
            EditorUtility.SetDirty(dropTable);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Loot Forge",
                $"'{definition.displayName}' is saved and assigned to '{dropTable.name}'.\n\n" +
                "Use Enemy Forge to assign that Drop Table, a pickup/loot visual, and a Loot Beam to an enemy.", "OK");
        }

        List<string> ValidateDefinition(bool dropTableRequired)
        {
            NormalizeVisualPrefabReferences();
            EnsureRecommendedAttachmentProfile(false);
            var issues = new List<string>();
            string id = definition.itemId?.Trim() ?? "";
            if (!System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-z0-9_-]{1,64}$"))
                issues.Add("Database Item ID must use 1–64 lowercase letters, numbers, underscores, or hyphens.");
            if (string.IsNullOrWhiteSpace(definition.displayName) || definition.displayName.Trim().Length > 128)
                issues.Add("Display Name is required and must be 128 characters or fewer.");
            if (definition.databaseItemType == LootDatabaseItemType.Unspecified)
                issues.Add("Database Item Type must be explicitly selected; new items cannot silently default to Material.");
            bool equipment = IsEquipment(definition.databaseItemType);
            if (equipment && definition.equipmentSlot == LootEquipmentSlot.None)
                issues.Add("Equipment Slot is required for every equipment item.");
            if (!equipment && definition.equipmentSlot != LootEquipmentSlot.None)
                issues.Add("Materials cannot occupy a paper-doll equipment slot.");
            if (equipment && definition.stackable)
                issues.Add("Equipment cannot be stackable.");
            if (equipment && definition.maxStackSize != 1)
                issues.Add("Equipment Maximum Stack Size must be 1.");
            if (equipment && definition.equippedVisualPrefab == null &&
                definition.worldVisualPrefab == null)
                issues.Add("Equipment requires an Equipped Visual Prefab or reusable World Visual Prefab.");
            if (equipment && definition.equipmentSlot != LootEquipmentSlot.Ring &&
                definition.equipmentSlot != LootEquipmentSlot.Trinket &&
                definition.attachmentProfile == null)
                issues.Add("Equipment requires an attachment profile. Use Apply Recommended Attachment Profile.");
            if (equipment && !SlotMatchesType(definition.databaseItemType, definition.equipmentSlot))
                issues.Add($"{definition.databaseItemType} cannot use the {definition.equipmentSlot} equipment slot.");
            if (definition.inventoryIcon == null)
                issues.Add("Inventory Icon is required.");
            if (definition.worldVisualPrefab != null &&
                !PrefabUtility.IsPartOfPrefabAsset(definition.worldVisualPrefab))
                issues.Add("When assigned, World Visual Prefab must be a prefab asset from the Project window.");
            if (definition.equippedVisualPrefab != null &&
                !PrefabUtility.IsPartOfPrefabAsset(definition.equippedVisualPrefab))
                issues.Add("When assigned, Equipped Visual Prefab must be a prefab asset from the Project window.");
            if (definition.sellValue < 0) issues.Add("Sell Value cannot be negative.");
            if (definition.stackable && definition.maxStackSize < 1)
                issues.Add("Maximum Stack Size must be at least 1.");
            if (dropTableRequired && dropTable == null) issues.Add("Select an existing Drop Table.");

            foreach (string guid in AssetDatabase.FindAssets("t:LootItemDefinition"))
            {
                LootItemDefinition other = AssetDatabase.LoadAssetAtPath<LootItemDefinition>(
                    AssetDatabase.GUIDToAssetPath(guid));
                if (other != null && other != definition && string.Equals(
                        other.itemId?.Trim(), id, System.StringComparison.OrdinalIgnoreCase))
                { issues.Add($"Item ID '{id}' is already used by '{other.name}'."); break; }
            }
            return issues;
        }

        void NormalizeVisualPrefabReferences()
        {
            if (definition == null) return;
            GameObject world = ResolvePrefabAsset(definition.worldVisualPrefab);
            GameObject equipped = ResolvePrefabAsset(definition.equippedVisualPrefab);
            if (world == definition.worldVisualPrefab &&
                equipped == definition.equippedVisualPrefab)
                return;

            Undo.RecordObject(definition, "Use loot prefab assets");
            definition.worldVisualPrefab = world;
            definition.equippedVisualPrefab = equipped;
            EditorUtility.SetDirty(definition);
        }

        static GameObject ResolvePrefabAsset(GameObject value)
        {
            if (value == null || PrefabUtility.IsPartOfPrefabAsset(value)) return value;
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(value);
            return string.IsNullOrWhiteSpace(path)
                ? value
                : AssetDatabase.LoadAssetAtPath<GameObject>(path) ?? value;
        }

        void EnsureRecommendedAttachmentProfile(bool force)
        {
            if (definition == null || !IsEquipment(definition.databaseItemType)) return;
            if (!force && definition.attachmentProfile != null &&
                !LootForgeAttachmentDefaults.IsDefault(definition.attachmentProfile))
                return;

            EquipmentAttachmentProfile recommended =
                LootForgeAttachmentDefaults.GetOrCreate(
                    definition.equipmentSlot, definition.twoHanded);
            if (recommended == definition.attachmentProfile) return;
            Undo.RecordObject(definition, "Assign recommended attachment profile");
            definition.attachmentProfile = recommended;
            definition.overrideAttachmentProfile = false;
            EditorUtility.SetDirty(definition);
        }

        bool GenerateInventoryIcon(bool showConfirmation)
        {
            Sprite icon = LootForgeIconRenderer.Render(definition, out string error);
            if (icon == null)
            {
                EditorUtility.DisplayDialog("Loot Forge Inventory Icon",
                    string.IsNullOrEmpty(error) ? "Inventory icon generation failed." : error, "OK");
                return false;
            }
            Undo.RecordObject(definition, "Generate Loot Inventory Icon");
            definition.inventoryIcon = icon;
            definition.iconId = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(icon));
            EditorUtility.SetDirty(definition);
            if (showConfirmation)
                EditorUtility.DisplayDialog("Loot Forge Inventory Icon",
                    $"Created transparent 256x256 PNG:\n{AssetDatabase.GetAssetPath(icon)}", "OK");
            return true;
        }

        void CreateAttachmentProfile()
        {
            const string folder = "Assets/Game/Resources/LootForge/Attachment Profiles";
            EnsureFolder(folder);
            string baseName = string.IsNullOrWhiteSpace(definition.displayName)
                ? definition.itemId : definition.displayName;
            string path = AssetDatabase.GenerateUniqueAssetPath(
                $"{folder}/{SafeFileName(baseName)} Attachment.asset");
            var profile = CreateInstance<EquipmentAttachmentProfile>();
            profile.profileName = baseName;
            profile.attachmentBoneName = definition.attachmentBoneName;
            profile.localPosition = definition.equippedLocalPosition;
            profile.localEulerAngles = definition.equippedLocalEulerAngles;
            profile.localScale = definition.equippedLocalScale.sqrMagnitude > 0.0001f
                ? definition.equippedLocalScale : Vector3.one;
            profile.twoHanded = definition.twoHanded;
            AssetDatabase.CreateAsset(profile, path);
            Undo.RecordObject(definition, "Assign Equipment Attachment Profile");
            definition.attachmentProfile = profile;
            definition.overrideAttachmentProfile = false;
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            Selection.activeObject = profile;
        }

        static void ApplyTypeDefaults(SerializedObject serialized, LootDatabaseItemType type)
        {
            bool equipment = IsEquipment(type);
            serialized.FindProperty("stackable").boolValue = !equipment;
            serialized.FindProperty("maxStackSize").intValue = equipment ? 1 : 99;
            SerializedProperty slot = serialized.FindProperty("equipmentSlot");
            slot.enumValueIndex = (int)DefaultSlot(type);
        }

        static bool IsEquipment(LootDatabaseItemType type) =>
            type != LootDatabaseItemType.Material &&
            type != LootDatabaseItemType.Unspecified &&
            type != LootDatabaseItemType.Consumable;

        static LootEquipmentSlot DefaultSlot(LootDatabaseItemType type) => type switch
        {
            LootDatabaseItemType.Weapon => LootEquipmentSlot.MainHand,
            LootDatabaseItemType.ArmorHead => LootEquipmentSlot.Head,
            LootDatabaseItemType.ArmorChest => LootEquipmentSlot.Chest,
            LootDatabaseItemType.ArmorLegs => LootEquipmentSlot.Legs,
            LootDatabaseItemType.ArmorFeet => LootEquipmentSlot.Feet,
            LootDatabaseItemType.ArmorHands => LootEquipmentSlot.Hands,
            LootDatabaseItemType.Offhand => LootEquipmentSlot.OffHand,
            LootDatabaseItemType.Ring => LootEquipmentSlot.Ring,
            LootDatabaseItemType.Trinket => LootEquipmentSlot.Trinket,
            _ => LootEquipmentSlot.None
        };

        static bool SlotMatchesType(LootDatabaseItemType type, LootEquipmentSlot slot)
        {
            // Weapon is the database category for wielded weapon items. It may be
            // authored for either hand; the dedicated Offhand category remains
            // available for shields, focuses, tomes, and similar offhand gear.
            if (type == LootDatabaseItemType.Weapon)
                return slot == LootEquipmentSlot.MainHand ||
                       slot == LootEquipmentSlot.OffHand;
            return DefaultSlot(type) == slot;
        }

        string BuildDeploymentSummary()
        {
            string category = definition.databaseItemType switch
            {
                LootDatabaseItemType.Material => "Materials",
                LootDatabaseItemType.Consumable => "Consumables",
                _ => "Gear"
            };
            string slot = definition.equipmentSlot == LootEquipmentSlot.None
                ? "not equipped" : definition.equipmentSlot.ToString();
            return $"Deployment preview: Inventory category {category}; slot {slot}; " +
                   $"stack {(definition.stackable ? definition.maxStackSize : 1)}; " +
                   $"database sync on next server start.";
        }

        static string SafeFileName(string value)
        {
            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');
            return value;
        }

        static void EnsureFolder(string path)
        {
            string[] parts = path.Replace('\\', '/').Split('/');
            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
