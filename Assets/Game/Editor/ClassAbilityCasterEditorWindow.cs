#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Crossworlds.EditorTools
{
    /// <summary>
    /// Central Spell Forge editor for the AbilityCaster attached to each class prefab.
    /// </summary>
    public sealed class ClassAbilityCasterEditorWindow : EditorWindow
    {
        const string ClassPreference = "BCE.SpellForge.SelectedClass";
        const double AutoSaveDelay = 0.75d;

        static readonly string[] ClassNames =
        {
            "Arcanist",
            "Marauder",
            "Ironclad",
            "Shadowblade",
            "Cleric"
        };

        static readonly string[] PrefabPaths =
        {
            "Assets/Game/Game_Prefabs/Arcanist.prefab",
            "Assets/Game/Game_Prefabs/Marauder.prefab",
            "Assets/Game/Game_Prefabs/Ironclad.prefab",
            "Assets/Game/Game_Prefabs/Shadowblade.prefab",
            "Assets/Game/Game_Prefabs/Cleric.prefab"
        };

        readonly HashSet<string> expandedAbilities = new();

        GameObject prefabAsset;
        AbilityCaster abilityCaster;
        SerializedObject serializedCaster;
        SerializedProperty spellbook;
        Vector2 scroll;
        string search = "";
        int classIndex;
        bool pendingSave;
        double lastChangeTime;
        string lastSaveMessage = "";

        [MenuItem("BCE/Spell Forge/Spellbook", priority = 38)]
        public static void Open()
        {
            var window = GetWindow<ClassAbilityCasterEditorWindow>(
                false, "Class Ability Casters", true);
            window.minSize = new Vector2(650f, 560f);
            window.Show();
            window.Focus();
        }

        void OnEnable()
        {
            classIndex = Mathf.Clamp(
                EditorPrefs.GetInt(ClassPreference, 0),
                0, ClassNames.Length - 1);
            LoadClass();
            Undo.undoRedoPerformed += OnUndoRedo;
            SpellVFXBrowserWindow.SpellForgeSelectionChanged += Repaint;
        }

        void OnDisable()
        {
            SaveCurrent();
            Undo.undoRedoPerformed -= OnUndoRedo;
            SpellVFXBrowserWindow.SpellForgeSelectionChanged -= Repaint;
        }

        void Update()
        {
            if (!pendingSave) return;
            if (EditorApplication.timeSinceStartup - lastChangeTime <
                AutoSaveDelay)
                return;
            if (GUIUtility.hotControl != 0) return;
            SaveCurrent();
        }

        void OnGUI()
        {
            DrawHeader();

            if (prefabAsset == null || abilityCaster == null ||
                serializedCaster == null || spellbook == null)
            {
                EditorGUILayout.HelpBox(
                    $"No AbilityCaster was found on:\n{PrefabPaths[classIndex]}",
                    MessageType.Error);
                if (GUILayout.Button("Reload Class Prefab"))
                    LoadClass();
                return;
            }

            serializedCaster.UpdateIfRequiredOrScript();
            DrawVFXSelection();
            DrawSpellbookToolbar();

            EditorGUI.BeginChangeCheck();
            DrawAbilities();
            if (EditorGUI.EndChangeCheck())
            {
                serializedCaster.ApplyModifiedProperties();
                MarkChanged();
            }
        }

        void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    "BCE Spell Forge", TitleStyle());

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        "Class", GUILayout.Width(38f));
                    int nextClass = EditorGUILayout.Popup(
                        classIndex, ClassNames);
                    if (nextClass != classIndex)
                    {
                        SaveCurrent();
                        classIndex = nextClass;
                        EditorPrefs.SetInt(ClassPreference, classIndex);
                        LoadClass();
                    }

                    if (GUILayout.Button("Open VFX Browser",
                        GUILayout.Width(126f)))
                        SpellVFXBrowserWindow.Open();
                }

                if (prefabAsset == null) return;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        $"{(abilityCaster != null ? abilityCaster.GetType().Name : "Missing AbilityCaster")}  •  " +
                        $"{PrefabPaths[classIndex]}",
                        EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Show Prefab", GUILayout.Width(90f)))
                    {
                        Selection.activeObject = prefabAsset;
                        EditorGUIUtility.PingObject(prefabAsset);
                    }

                    if (GUILayout.Button("Open Prefab", GUILayout.Width(88f)))
                        AssetDatabase.OpenAsset(prefabAsset);

                    using (new EditorGUI.DisabledScope(!pendingSave))
                    {
                        if (GUILayout.Button(
                            pendingSave ? "Save Changes" : "Saved",
                            GUILayout.Width(96f)))
                            SaveCurrent();
                    }
                }

                if (!string.IsNullOrEmpty(lastSaveMessage))
                    EditorGUILayout.LabelField(
                        lastSaveMessage, EditorStyles.centeredGreyMiniLabel);
            }
        }

        void DrawVFXSelection()
        {
            GameObject selectedVFX =
                SpellVFXBrowserWindow.SpellForgeSelection;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    "VFX Browser Selection", EditorStyles.boldLabel);

                if (selectedVFX == null)
                {
                    EditorGUILayout.HelpBox(
                        "Select an effect in BCE → Spell Forge → VFX Browser. " +
                        "It will appear here for one-click assignment.",
                        MessageType.Info);
                    return;
                }

                string path = AssetDatabase.GetAssetPath(selectedVFX);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField(
                        selectedVFX, typeof(GameObject), false);
                    if (GUILayout.Button("Copy Name", GUILayout.Width(82f)))
                    {
                        EditorGUIUtility.systemCopyBuffer = selectedVFX.name;
                        ShowNotification(new GUIContent(
                            $"Copied: {selectedVFX.name}"));
                    }
                    if (GUILayout.Button("Show", GUILayout.Width(54f)))
                    {
                        Selection.activeObject = selectedVFX;
                        EditorGUIUtility.PingObject(selectedVFX);
                    }
                }
                EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
            }
        }

        void DrawSpellbookToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(
                    $"{ClassNames[classIndex]} Abilities",
                    GUILayout.Width(130f));
                string nextSearch = GUILayout.TextField(
                    search, GUI.skin.FindStyle("ToolbarSearchTextField"));
                if (nextSearch != search)
                {
                    search = nextSearch;
                    Repaint();
                }
                GUILayout.Label(
                    $"{spellbook.arraySize} entries",
                    EditorStyles.miniLabel, GUILayout.Width(68f));

                if (GUILayout.Button("Collapse All",
                    EditorStyles.toolbarButton, GUILayout.Width(78f)))
                {
                    expandedAbilities.Clear();
                    GUI.FocusControl(null);
                }
            }
        }

        void DrawAbilities()
        {
            string searchTerm = search.Trim();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            int visibleCount = 0;
            for (int index = 0; index < spellbook.arraySize; index++)
            {
                SerializedProperty ability =
                    spellbook.GetArrayElementAtIndex(index);
                SerializedProperty nameProperty =
                    ability.FindPropertyRelative("abilityName");
                string abilityName =
                    nameProperty?.stringValue ?? $"Ability {index + 1}";

                if (!string.IsNullOrEmpty(searchTerm) &&
                    abilityName.IndexOf(
                        searchTerm, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                visibleCount++;
                DrawAbility(index, ability, abilityName);
                EditorGUILayout.Space(3f);
            }

            if (visibleCount == 0)
                EditorGUILayout.HelpBox(
                    $"No abilities match “{searchTerm}”.",
                    MessageType.Info);

            EditorGUILayout.Space(5f);
            if (GUILayout.Button("+ Add Ability", GUILayout.Height(25f)))
                AddAbility();

            EditorGUILayout.EndScrollView();
        }

        void DrawAbility(
            int index, SerializedProperty ability, string abilityName)
        {
            string key = ClassNames[classIndex] + ":" + index;
            bool expanded = expandedAbilities.Contains(key);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                bool nextExpanded = expanded;
                using (new EditorGUILayout.HorizontalScope())
                {
                    nextExpanded = EditorGUILayout.Foldout(
                        expanded,
                        $"{index + 1}. {abilityName}",
                        true, EditorStyles.foldoutHeader);
                    if (nextExpanded != expanded)
                    {
                        if (nextExpanded) expandedAbilities.Add(key);
                        else expandedAbilities.Remove(key);
                    }

                    GUILayout.FlexibleSpace();
                    DrawAbilitySummary(ability);
                }

                DrawVFXSummary(ability);
                if (!nextExpanded) return;

                EditorGUILayout.Space(3f);
                DrawBrowserAssignmentButtons(ability);
                EditorGUILayout.Space(3f);
                DrawPropertyChildren(ability);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(
                        "Duplicate Ability", GUILayout.Width(116f)))
                    {
                        DuplicateAbility(index);
                        GUIUtility.ExitGUI();
                    }
                    if (GUILayout.Button(
                        "Remove Ability", GUILayout.Width(104f)))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Remove Ability",
                            $"Remove “{abilityName}” from " +
                            $"{ClassNames[classIndex]}?",
                            "Remove", "Cancel"))
                        {
                            RemoveAbility(index);
                            GUIUtility.ExitGUI();
                        }
                    }
                }
            }
        }

        static void DrawAbilitySummary(SerializedProperty ability)
        {
            SerializedProperty category =
                ability.FindPropertyRelative("category");
            SerializedProperty cooldown =
                ability.FindPropertyRelative("cooldown");
            SerializedProperty mana =
                ability.FindPropertyRelative("manaCost");

            string categoryName = category != null
                ? category.enumDisplayNames[
                    Mathf.Clamp(category.enumValueIndex, 0,
                        category.enumDisplayNames.Length - 1)]
                : "Ability";
            float cooldownValue = cooldown?.floatValue ?? 0f;
            float manaValue = mana?.floatValue ?? 0f;

            GUILayout.Label(
                $"{categoryName}  •  {cooldownValue:0.#}s  •  " +
                $"{manaValue:0.#} MP",
                EditorStyles.miniLabel);
        }

        static void DrawVFXSummary(SerializedProperty ability)
        {
            GameObject cast = ability.FindPropertyRelative("castVFX")
                ?.objectReferenceValue as GameObject;
            GameObject hit = ability.FindPropertyRelative("hitVFX")
                ?.objectReferenceValue as GameObject;
            GameObject deploy = ability
                .FindPropertyRelative("deployablePrefab")
                ?.objectReferenceValue as GameObject;

            string summary =
                $"Cast: {(cast != null ? cast.name : "None")}   " +
                $"Hit: {(hit != null ? hit.name : "None")}   " +
                $"Deploy: {(deploy != null ? deploy.name : "None")}";
            EditorGUILayout.LabelField(summary, EditorStyles.miniLabel);
        }

        void DrawBrowserAssignmentButtons(SerializedProperty ability)
        {
            GameObject selectedVFX =
                SpellVFXBrowserWindow.SpellForgeSelection;
            using (new EditorGUI.DisabledScope(selectedVFX == null))
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(
                    selectedVFX != null
                        ? $"Assign {selectedVFX.name}:"
                        : "Select a prefab in VFX Browser:",
                    EditorStyles.miniLabel, GUILayout.MinWidth(165f));

                if (GUILayout.Button("→ Cast VFX"))
                    AssignVFX(ability, "castVFX", selectedVFX);
                if (GUILayout.Button("→ Hit VFX"))
                    AssignVFX(ability, "hitVFX", selectedVFX);
                if (GUILayout.Button("→ Deployable"))
                    AssignVFX(ability, "deployablePrefab", selectedVFX);
            }
        }

        static void DrawPropertyChildren(SerializedProperty parent)
        {
            SerializedProperty child = parent.Copy();
            SerializedProperty end = child.GetEndProperty();
            bool enterChildren = true;

            while (child.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                EditorGUILayout.PropertyField(child, true);
            }
        }

        void AssignVFX(
            SerializedProperty ability, string fieldName, GameObject prefab)
        {
            if (prefab == null) return;
            SerializedProperty field =
                ability.FindPropertyRelative(fieldName);
            if (field == null) return;

            field.objectReferenceValue = prefab;
            serializedCaster.ApplyModifiedProperties();
            MarkChanged();
        }

        void AddAbility()
        {
            serializedCaster.ApplyModifiedProperties();
            Undo.RecordObject(abilityCaster, "Add Spell Forge ability");

            AbilityDef[] current =
                abilityCaster.spellbook ?? Array.Empty<AbilityDef>();
            var next = new AbilityDef[current.Length + 1];
            Array.Copy(current, next, current.Length);
            next[current.Length] = new AbilityDef
            {
                abilityName = $"New {ClassNames[classIndex]} Ability"
            };
            abilityCaster.spellbook = next;
            serializedCaster.Update();
            spellbook = serializedCaster.FindProperty("spellbook");

            int index = next.Length - 1;
            expandedAbilities.Add(ClassNames[classIndex] + ":" + index);
            MarkChanged();
        }

        void DuplicateAbility(int index)
        {
            serializedCaster.ApplyModifiedProperties();
            serializedCaster.Update();
            spellbook.InsertArrayElementAtIndex(index);
            SerializedProperty duplicate =
                spellbook.GetArrayElementAtIndex(index);
            SerializedProperty name =
                duplicate.FindPropertyRelative("abilityName");
            if (name != null)
                name.stringValue += " Copy";
            serializedCaster.ApplyModifiedProperties();
            expandedAbilities.Add(
                ClassNames[classIndex] + ":" + index);
            MarkChanged();
        }

        void RemoveAbility(int index)
        {
            serializedCaster.Update();
            spellbook.DeleteArrayElementAtIndex(index);
            serializedCaster.ApplyModifiedProperties();
            expandedAbilities.Clear();
            MarkChanged();
        }

        void LoadClass()
        {
            prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                PrefabPaths[classIndex]);
            abilityCaster = prefabAsset != null
                ? prefabAsset.GetComponentInChildren<AbilityCaster>(true)
                : null;
            serializedCaster = abilityCaster != null
                ? new SerializedObject(abilityCaster)
                : null;
            spellbook = serializedCaster?.FindProperty("spellbook");
            pendingSave = false;
            lastSaveMessage = "";
            scroll = Vector2.zero;
            Repaint();
        }

        void MarkChanged()
        {
            if (abilityCaster == null) return;
            EditorUtility.SetDirty(abilityCaster);
            EditorUtility.SetDirty(prefabAsset);
            pendingSave = true;
            lastChangeTime = EditorApplication.timeSinceStartup;
            lastSaveMessage = "Unsaved changes";
            Repaint();
        }

        void SaveCurrent()
        {
            if (!pendingSave || prefabAsset == null) return;
            if (serializedCaster != null)
                serializedCaster.ApplyModifiedProperties();

            PrefabUtility.SavePrefabAsset(
                prefabAsset, out bool success);
            if (success)
            {
                pendingSave = false;
                lastSaveMessage =
                    $"{ClassNames[classIndex]} saved";
            }
            else
            {
                lastSaveMessage = "Save failed — check the Console";
            }
            Repaint();
        }

        void OnUndoRedo()
        {
            serializedCaster?.Update();
            Repaint();
        }

        static GUIStyle TitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 17,
                fixedHeight = 25f
            };
        }
    }
}
#endif
