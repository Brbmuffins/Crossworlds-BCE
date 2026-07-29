#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
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
        const string ClassPortraitFolder =
            "Assets/Game/Art/Class Portraits";
        const double AutoSaveDelay = 0.75d;
        const float ClassPreviewSize = 120f;
        const float VFXPreviewSize = 68f;
        const float AbilityIconSize = 30f;

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

        static readonly string[] SpellTabs =
        {
            "Logistics",
            "Animation",
            "VFX & Preview"
        };

        readonly HashSet<string> expandedAbilities = new();
        readonly Dictionary<string, int> abilityTabs = new();

        GameObject prefabAsset;
        Texture2D classPortrait;
        AbilityCaster abilityCaster;
        SerializedObject serializedCaster;
        SerializedProperty spellbook;
        Vector2 scroll;
        string search = "";
        int classIndex;
        bool pendingSave;
        double lastChangeTime;
        string lastSaveMessage = "";
        string activePreviewAbility;
        SpellVFXPreviewPanel spellPreview;

        [MenuItem("BCE/Spell Forge/Spellbook", priority = 38)]
        public static void Open()
        {
            var window = GetWindow<ClassAbilityCasterEditorWindow>(
                false, "Spellbook", true);
            window.minSize = new Vector2(700f, 600f);
            window.Show();
            window.Focus();
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Spellbook");
            minSize = new Vector2(700f, 600f);
            spellPreview ??= new SpellVFXPreviewPanel();
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
            spellPreview?.Dispose();
            spellPreview = null;
            Undo.undoRedoPerformed -= OnUndoRedo;
            SpellVFXBrowserWindow.SpellForgeSelectionChanged -= Repaint;
        }

        void Update()
        {
            RepaintWhilePreviewLoads(classPortrait);
            RepaintWhilePreviewLoads(
                SpellVFXBrowserWindow.SpellForgeSelection);
            if (spellPreview != null && spellPreview.Tick())
                Repaint();

            if (!pendingSave) return;
            if (EditorApplication.timeSinceStartup - lastChangeTime <
                AutoSaveDelay)
                return;
            if (GUIUtility.hotControl != 0) return;
            SaveCurrent();
        }

        void OnGUI()
        {
            DrawBrandBanner();
            EditorGUILayout.Space(4f);
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
                using (new EditorGUILayout.HorizontalScope(
                    GUILayout.Height(ClassPreviewSize)))
                {
                    DrawAssetPreview(
                        classPortrait, ClassPreviewSize,
                        "No Class Portrait");

                    EditorGUILayout.Space(8f);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(
                            "ACTIVE CLASS", EyebrowStyle());

                        int nextClass = EditorGUILayout.Popup(
                            classIndex, ClassNames,
                            GUILayout.Height(23f));
                        if (nextClass != classIndex)
                        {
                            SaveCurrent();
                            classIndex = nextClass;
                            EditorPrefs.SetInt(
                                ClassPreference, classIndex);
                            LoadClass();
                        }

                        string casterName = abilityCaster != null
                            ? abilityCaster.GetType().Name
                            : "Missing AbilityCaster";
                        EditorGUILayout.LabelField(
                            casterName, SectionTitleStyle());

                        EditorGUILayout.SelectableLabel(
                            PrefabPaths[classIndex],
                            EditorStyles.miniLabel,
                            GUILayout.Height(18f));

                        EditorGUILayout.Space(5f);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            using (new EditorGUI.DisabledScope(
                                prefabAsset == null))
                            {
                                if (GUILayout.Button("Show Prefab"))
                                {
                                    Selection.activeObject = prefabAsset;
                                    EditorGUIUtility.PingObject(prefabAsset);
                                }

                                if (GUILayout.Button("Open Prefab"))
                                    AssetDatabase.OpenAsset(prefabAsset);
                            }

                            if (GUILayout.Button("VFX Browser"))
                                SpellVFXBrowserWindow.Open();

                            using (new EditorGUI.DisabledScope(!pendingSave))
                            {
                                if (GUILayout.Button(
                                    pendingSave
                                        ? "Save Changes"
                                        : "Saved",
                                    GUILayout.Width(96f)))
                                    SaveCurrent();
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(lastSaveMessage))
                {
                    EditorGUILayout.Space(2f);
                    EditorGUILayout.LabelField(
                        lastSaveMessage, EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        void DrawVFXSelection()
        {
            GameObject selectedVFX =
                SpellVFXBrowserWindow.SpellForgeSelection;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        "SELECTED VFX", EyebrowStyle());
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(
                        "Browse Effects", EditorStyles.miniButton,
                        GUILayout.Width(96f)))
                        SpellVFXBrowserWindow.Open();
                }

                if (selectedVFX == null)
                {
                    EditorGUILayout.HelpBox(
                        "Choose an effect in the VFX Browser, then assign it " +
                        "to a spell as its cast, hit, or deployable prefab.",
                        MessageType.Info);
                    return;
                }

                string path = AssetDatabase.GetAssetPath(selectedVFX);
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawAssetPreview(
                        selectedVFX, VFXPreviewSize, "VFX");

                    EditorGUILayout.Space(7f);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(
                            selectedVFX.name, SectionTitleStyle());
                        EditorGUILayout.SelectableLabel(
                            path, EditorStyles.miniLabel,
                            GUILayout.Height(18f));

                        GUILayout.FlexibleSpace();
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.ObjectField(
                                selectedVFX, typeof(GameObject), false);

                            if (GUILayout.Button(
                                "Copy Name", GUILayout.Width(82f)))
                            {
                                EditorGUIUtility.systemCopyBuffer =
                                    selectedVFX.name;
                                ShowNotification(new GUIContent(
                                    $"Copied: {selectedVFX.name}"));
                            }

                            if (GUILayout.Button(
                                "Show", GUILayout.Width(54f)))
                            {
                                Selection.activeObject = selectedVFX;
                                EditorGUIUtility.PingObject(selectedVFX);
                            }
                        }
                    }
                }
            }
        }

        void DrawSpellbookToolbar()
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"{ClassNames[classIndex]} Spellbook",
                    SectionTitleStyle());
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(
                    $"{spellbook.arraySize} " +
                    (spellbook.arraySize == 1 ? "spell" : "spells"),
                    EditorStyles.miniLabel,
                    GUILayout.Width(58f));
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Search", GUILayout.Width(43f));
                string nextSearch = GUILayout.TextField(
                    search, GUI.skin.FindStyle("ToolbarSearchTextField"));
                if (nextSearch != search)
                {
                    search = nextSearch;
                    Repaint();
                }

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
                    Sprite abilityIcon = ability
                        .FindPropertyRelative("icon")
                        ?.objectReferenceValue as Sprite;
                    Rect foldoutRect = GUILayoutUtility.GetRect(
                        14f, AbilityIconSize,
                        GUILayout.Width(14f),
                        GUILayout.Height(AbilityIconSize));
                    nextExpanded = EditorGUI.Foldout(
                        foldoutRect,
                        nextExpanded,
                        GUIContent.none,
                        false,
                        EditorStyles.foldout);

                    RepaintWhilePreviewLoads(abilityIcon);
                    if (DrawAbilityIcon(abilityIcon))
                        nextExpanded = !nextExpanded;

                    var abilityLabel = new GUIContent(
                        $"{index + 1}. {abilityName}",
                        abilityIcon != null
                            ? $"Icon: {abilityIcon.name}"
                            : "No spell icon assigned");
                    if (GUILayout.Button(
                        abilityLabel,
                        AbilityNameStyle(),
                        GUILayout.Height(AbilityIconSize)))
                        nextExpanded = !nextExpanded;

                    if (nextExpanded != expanded)
                    {
                        if (nextExpanded) expandedAbilities.Add(key);
                        else
                        {
                            expandedAbilities.Remove(key);
                            StopPreviewIfActive(key);
                        }
                    }

                    GUILayout.FlexibleSpace();
                    DrawAbilitySummary(ability);
                }

                DrawVFXSummary(ability);
                if (!nextExpanded) return;

                EditorGUILayout.Space(3f);
                DrawAbilityTabs(ability, key);
                EditorGUILayout.Space(5f);

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

        void DrawAbilityTabs(
            SerializedProperty ability, string key)
        {
            int selectedTab = abilityTabs.TryGetValue(key, out int storedTab)
                ? Mathf.Clamp(storedTab, 0, SpellTabs.Length - 1)
                : 0;
            int nextTab = GUILayout.Toolbar(
                selectedTab, SpellTabs, GUILayout.Height(25f));
            if (nextTab != selectedTab)
            {
                abilityTabs[key] = nextTab;
                if (nextTab == 2)
                    activePreviewAbility = key;
                else
                    StopPreviewIfActive(key);
                GUI.FocusControl(null);
            }

            EditorGUILayout.Space(4f);
            switch (nextTab)
            {
                case 1:
                    DrawAnimationTab(ability);
                    break;
                case 2:
                    DrawVFXTab(ability, key);
                    break;
                default:
                    DrawLogisticsTab(ability);
                    break;
            }
        }

        static void DrawLogisticsTab(SerializedProperty ability)
        {
            DrawFieldGroup(
                "IDENTITY & TARGETING", ability,
                "abilityName",
                "variantOnly",
                "icon",
                "category",
                "shape",
                "range",
                "coneAngle",
                "rectWidth",
                "indicatorSize",
                "targetTag");

            DrawFieldGroup(
                "TIMING & COST", ability,
                "cooldown",
                "manaCost");

            DrawFieldGroup(
                "DAMAGE & CHARGE", ability,
                "damage",
                "chargeable",
                "maxChargeTime",
                "maxChargeDamage",
                "maxChargeSizeMultiplier");

            DrawFieldGroup(
                "HEALING & DEFENSE", ability,
                "shieldAbsorb",
                "shieldDuration",
                "healAmount",
                "hotTickAmount",
                "hotTicks",
                "hotInterval");

            DrawFieldGroup(
                "STATUS & DURATION", ability,
                "statusEffect",
                "statusDuration",
                "statusValue",
                "activeDuration");

            DrawFieldGroup(
                "ADVANCED EFFECTS", ability,
                "chainTargets",
                "chainDamageFalloff",
                "pullRadius",
                "pullDuration",
                "usePulseDamage",
                "pulseCount",
                "pulseInterval",
                "pulseRadius",
                "pulseDamage",
                "variants");
        }

        static void DrawAnimationTab(SerializedProperty ability)
        {
            DrawFieldGroup(
                "CAST", ability,
                "castTime",
                "marauderCastAnimation");

            DrawFieldGroup(
                "CASTER MOVEMENT", ability,
                "moveCasterToTarget",
                "instantMovement",
                "movementTiming",
                "moveToSpeed",
                "fixedMovementDuration",
                "movementArcHeight",
                "resolveEffectsOnLanding",
                "animationLandingPoint");
        }

        void DrawVFXTab(
            SerializedProperty ability, string key)
        {
            DrawVFXSelection();
            EditorGUILayout.Space(4f);
            DrawBrowserAssignmentButtons(ability);

            DrawFieldGroup(
                "ASSIGNED PREFABS", ability,
                "castVFX",
                "hitVFX",
                "spawnTurret",
                "turretPrefab",
                "deployablePrefab");

            DrawFieldGroup(
                "VISUAL TUNING", ability,
                "variantIndicatorTint",
                "chargedTint",
                "fireVisual",
                "pulseVFXLifetime");

            DrawSpellPreview(ability, key);
        }

        void DrawSpellPreview(
            SerializedProperty ability, string key)
        {
            EditorGUILayout.LabelField(
                "CHARACTER SPELL PREVIEW", EyebrowStyle());
            using (new EditorGUILayout.VerticalScope(
                EditorStyles.helpBox))
            {
                if (prefabAsset == null)
                {
                    StopPreviewIfActive(key);
                    EditorGUILayout.HelpBox(
                        "The selected class prefab is unavailable.",
                        MessageType.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(activePreviewAbility))
                    activePreviewAbility = key;

                if (activePreviewAbility != key)
                {
                    EditorGUILayout.HelpBox(
                        "Another expanded spell owns the live preview.",
                        MessageType.None);
                    if (GUILayout.Button("Preview This Spell"))
                        activePreviewAbility = key;
                    return;
                }

                spellPreview ??= new SpellVFXPreviewPanel();
                AnimationClip castAnimation = ability
                    .FindPropertyRelative("marauderCastAnimation")
                    ?.objectReferenceValue as AnimationClip;
                AbilityCategory category = (AbilityCategory)(
                    ability.FindPropertyRelative("category")
                        ?.intValue ?? 0);
                float castTime = ability
                    .FindPropertyRelative("castTime")
                    ?.floatValue ?? 0f;
                float range = ability
                    .FindPropertyRelative("range")
                    ?.floatValue ?? 0f;
                GameObject castVFX = ability
                    .FindPropertyRelative("castVFX")
                    ?.objectReferenceValue as GameObject;
                GameObject hitVFX = ability
                    .FindPropertyRelative("hitVFX")
                    ?.objectReferenceValue as GameObject;
                GameObject deployable = ability
                    .FindPropertyRelative("deployablePrefab")
                    ?.objectReferenceValue as GameObject;

                spellPreview.EnsureSpell(
                    prefabAsset,
                    castAnimation,
                    category,
                    castTime,
                    range,
                    castVFX,
                    hitVFX,
                    deployable);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        spellPreview.AnimationLabel,
                        SectionTitleStyle());
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(
                        $"Resolve: {castTime:0.##}s",
                        EditorStyles.miniLabel,
                        GUILayout.Width(92f));
                }

                Rect previewRect = GUILayoutUtility.GetRect(
                    280f, 280f, GUILayout.ExpandWidth(true));
                spellPreview.Draw(previewRect);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(
                        "Replay", GUILayout.Width(72f)))
                        spellPreview.Replay();

                    if (GUILayout.Button(
                        spellPreview.IsPlaying ? "Pause" : "Play",
                        GUILayout.Width(72f)))
                        spellPreview.TogglePlayback();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(
                        "Show Class Prefab", GUILayout.Width(116f)))
                    {
                        Selection.activeObject = prefabAsset;
                        EditorGUIUtility.PingObject(prefabAsset);
                    }
                }
            }
        }

        static void DrawFieldGroup(
            string title,
            SerializedProperty ability,
            params string[] propertyNames)
        {
            EditorGUILayout.LabelField(title, EyebrowStyle());
            using (new EditorGUILayout.VerticalScope(
                EditorStyles.helpBox))
            {
                foreach (string propertyName in propertyNames)
                {
                    SerializedProperty property =
                        ability.FindPropertyRelative(propertyName);
                    if (property != null)
                        EditorGUILayout.PropertyField(property, true);
                }
            }
        }

        void StopPreviewIfActive(string key)
        {
            if (activePreviewAbility != key) return;
            activePreviewAbility = null;
            spellPreview?.Clear();
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
            spellPreview?.Clear();
            activePreviewAbility = null;
            abilityTabs.Clear();
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
            spellPreview?.Clear();
            activePreviewAbility = null;
            abilityTabs.Clear();
            serializedCaster.Update();
            spellbook.DeleteArrayElementAtIndex(index);
            serializedCaster.ApplyModifiedProperties();
            expandedAbilities.Clear();
            MarkChanged();
        }

        void LoadClass()
        {
            spellPreview?.Clear();
            activePreviewAbility = null;
            prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                PrefabPaths[classIndex]);
            classPortrait = LoadClassPortrait(ClassNames[classIndex]);
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

        static Texture2D LoadClassPortrait(string className)
        {
            string directPath =
                $"{ClassPortraitFolder}/{className}.png";
            Texture2D portrait =
                AssetDatabase.LoadAssetAtPath<Texture2D>(directPath);
            if (portrait != null) return portrait;

            string[] portraitGuids = AssetDatabase.FindAssets(
                $"{className} t:Texture2D",
                new[] { ClassPortraitFolder });
            foreach (string guid in portraitGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.Equals(
                    Path.GetFileNameWithoutExtension(path),
                    className,
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                portrait =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (portrait != null) return portrait;
            }

            return null;
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

        void DrawBrandBanner()
        {
            Rect banner = EditorGUILayout.GetControlRect(
                false, 60f, GUILayout.ExpandWidth(true));
            Color background = EditorGUIUtility.isProSkin
                ? new Color(0.11f, 0.08f, 0.16f)
                : new Color(0.31f, 0.22f, 0.42f);
            Color accent = EditorGUIUtility.isProSkin
                ? new Color(0.62f, 0.36f, 0.88f)
                : new Color(0.48f, 0.24f, 0.68f);

            EditorGUI.DrawRect(banner, background);
            EditorGUI.DrawRect(
                new Rect(banner.x, banner.y, 5f, banner.height), accent);

            GUI.Label(
                new Rect(
                    banner.x + 16f, banner.y + 7f,
                    banner.width - 32f, 16f),
                "BCE  /  SPELL FORGE", BannerEyebrowStyle());
            GUI.Label(
                new Rect(
                    banner.x + 15f, banner.y + 22f,
                    banner.width - 30f, 30f),
                "Spellbook", BannerTitleStyle());

            if (spellbook != null)
            {
                GUI.Label(
                    new Rect(
                        banner.xMax - 180f, banner.y + 21f,
                        164f, 24f),
                    $"{ClassNames[classIndex]}  •  " +
                    $"{spellbook.arraySize} spells",
                    BannerStatusStyle());
            }
        }

        static void DrawAssetPreview(
            UnityEngine.Object asset, float size, string emptyLabel)
        {
            Rect previewRect = GUILayoutUtility.GetRect(
                size, size,
                GUILayout.Width(size), GUILayout.Height(size));
            EditorGUI.DrawRect(
                previewRect,
                EditorGUIUtility.isProSkin
                    ? new Color(0.09f, 0.09f, 0.11f)
                    : new Color(0.74f, 0.74f, 0.76f));
            GUI.Box(previewRect, GUIContent.none, EditorStyles.helpBox);

            Rect contentRect = new Rect(
                previewRect.x + 4f, previewRect.y + 4f,
                previewRect.width - 8f, previewRect.height - 8f);
            Texture preview = GetAssetPreview(asset);

            if (preview != null)
            {
                GUI.DrawTexture(
                    contentRect, preview,
                    ScaleMode.ScaleToFit, true);
                return;
            }

            GUI.Label(
                contentRect,
                asset != null ? "Generating Preview…" : emptyLabel,
                CenteredMiniLabelStyle());
        }

        static bool DrawAbilityIcon(Sprite icon)
        {
            Rect iconRect = GUILayoutUtility.GetRect(
                AbilityIconSize, AbilityIconSize,
                GUILayout.Width(AbilityIconSize),
                GUILayout.Height(AbilityIconSize));
            EditorGUI.DrawRect(
                iconRect,
                EditorGUIUtility.isProSkin
                    ? new Color(0.08f, 0.08f, 0.10f)
                    : new Color(0.70f, 0.70f, 0.72f));

            bool clicked = GUI.Button(
                iconRect,
                new GUIContent(
                    "",
                    icon != null
                        ? $"Icon: {icon.name}"
                        : "No spell icon assigned"),
                GUIStyle.none);

            Texture preview = GetAssetPreview(icon);
            Rect contentRect = new Rect(
                iconRect.x + 2f, iconRect.y + 2f,
                iconRect.width - 4f, iconRect.height - 4f);
            if (preview != null)
                GUI.DrawTexture(
                    contentRect, preview,
                    ScaleMode.ScaleToFit, true);
            else
                GUI.Label(
                    contentRect, "—",
                    CenteredMiniLabelStyle());

            return clicked;
        }

        static Texture GetAssetPreview(UnityEngine.Object asset)
        {
            if (asset == null) return null;
            if (asset is Texture texture) return texture;
            return AssetPreview.GetAssetPreview(asset) ??
                   AssetPreview.GetMiniThumbnail(asset);
        }

        void RepaintWhilePreviewLoads(UnityEngine.Object asset)
        {
            if (asset != null &&
                AssetPreview.IsLoadingAssetPreview(asset.GetEntityId()))
                Repaint();
        }

        static GUIStyle BannerTitleStyle()
        {
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22
            };
            style.normal.textColor = Color.white;
            return style;
        }

        static GUIStyle BannerEyebrowStyle()
        {
            var style = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 9
            };
            style.normal.textColor =
                new Color(0.78f, 0.63f, 0.96f);
            return style;
        }

        static GUIStyle BannerStatusStyle()
        {
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
            style.normal.textColor =
                new Color(0.84f, 0.82f, 0.88f);
            return style;
        }

        static GUIStyle EyebrowStyle()
        {
            var style = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 9
            };
            style.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.70f, 0.52f, 0.92f)
                : new Color(0.38f, 0.16f, 0.58f);
            return style;
        }

        static GUIStyle SectionTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13
            };
        }

        static GUIStyle AbilityNameStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };
        }

        static GUIStyle CenteredMiniLabelStyle()
        {
            return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
#endif
