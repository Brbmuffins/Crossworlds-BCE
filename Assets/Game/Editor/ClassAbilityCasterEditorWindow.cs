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
        const string AbilityFilterPreference =
            "BCE.SpellForge.AbilityFilter";
        const string ClassPortraitFolder =
            "Assets/Game/Art/Class Portraits";
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

        enum MovementType
        {
            None,
            Dash,
            Leap,
            Teleport
        }

        enum AbilityListFilter
        {
            Core,
            Variants,
            All
        }

        readonly HashSet<string> expandedAbilities = new();
        readonly Dictionary<string, int> abilityTabs = new();
        readonly List<int> visibleAbilityIndices = new();

        GameObject prefabAsset;
        Texture2D classPortrait;
        AbilityCaster abilityCaster;
        SerializedObject serializedCaster;
        SerializedProperty spellbook;
        Vector2 scroll;
        string search = "";
        string appliedSearch = "";
        int classIndex;
        AbilityListFilter abilityListFilter = AbilityListFilter.Core;
        int coreAbilityCount;
        int variantAbilityCount;
        int visibleCoreCount;
        int visibleVariantCount;
        bool pendingSave;
        string lastSaveMessage = "";
        string activePreviewAbility;
        string activeIconPropertyPath;
        int activeIconPickerControlId;
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
            abilityListFilter = (AbilityListFilter)Mathf.Clamp(
                EditorPrefs.GetInt(
                    AbilityFilterPreference,
                    (int)AbilityListFilter.Core),
                0, (int)AbilityListFilter.All);
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
            if (EditorGUIUtility.editingTextField) return;

            RepaintWhilePreviewLoads(classPortrait);
            RepaintWhilePreviewLoads(
                SpellVFXBrowserWindow.SpellForgeSelection);
            if (spellPreview != null && spellPreview.Tick())
                Repaint();
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

            if (!pendingSave &&
                !EditorGUIUtility.editingTextField)
                serializedCaster.UpdateIfRequiredOrScript();
            HandleIconPickerEvent();
            DrawSpellbookToolbar();

            bool guiChanged = false;
            using (var changeCheck =
                new EditorGUI.ChangeCheckScope())
            {
                DrawAbilities();
                guiChanged = changeCheck.changed;
            }

            bool propertiesApplied =
                serializedCaster.ApplyModifiedProperties();
            if (guiChanged || propertiesApplied)
                MarkChanged();
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
                            GUIUtility.ExitGUI();
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
                    $"{coreAbilityCount} core  |  " +
                    $"{variantAbilityCount} variants",
                    EditorStyles.miniLabel,
                    GUILayout.Width(132f));
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

                GUILayout.Space(8f);
                GUILayout.Label("View", GUILayout.Width(29f));
                string[] filterLabels =
                {
                    $"Core ({coreAbilityCount})",
                    $"Variants ({variantAbilityCount})",
                    "All"
                };
                AbilityListFilter nextFilter =
                    (AbilityListFilter)GUILayout.Toolbar(
                        (int)abilityListFilter,
                        filterLabels,
                        EditorStyles.toolbarButton,
                        GUILayout.Width(244f));
                if (nextFilter != abilityListFilter)
                {
                    abilityListFilter = nextFilter;
                    EditorPrefs.SetInt(
                        AbilityFilterPreference,
                        (int)abilityListFilter);
                    GUI.FocusControl(null);
                    Repaint();
                    GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("Collapse All",
                    EditorStyles.toolbarButton, GUILayout.Width(78f)))
                {
                    expandedAbilities.Clear();
                    GUI.FocusControl(null);
                    GUIUtility.ExitGUI();
                }
            }
        }

        void DrawAbilities()
        {
            using (var scrollView =
                new EditorGUILayout.ScrollViewScope(scroll))
            {
                scroll = scrollView.scrollPosition;

                if (Event.current.type == EventType.Layout)
                    RebuildVisibleAbilityIndices();

                bool? drawingVariants = null;
                foreach (int index in visibleAbilityIndices)
                {
                    if (index < 0 || index >= spellbook.arraySize)
                        continue;
                    SerializedProperty ability =
                        spellbook.GetArrayElementAtIndex(index);
                    bool isVariant = IsVariantAbility(ability);
                    if (!drawingVariants.HasValue ||
                        drawingVariants.Value != isVariant)
                    {
                        drawingVariants = isVariant;
                        DrawAbilityGroupHeader(
                            isVariant ? "VARIANT ABILITIES" :
                                "CORE ABILITIES",
                            isVariant ? visibleVariantCount :
                                visibleCoreCount);
                    }

                    SerializedProperty nameProperty =
                        ability.FindPropertyRelative("abilityName");
                    string abilityName =
                        nameProperty?.stringValue ??
                        $"Ability {index + 1}";

                    DrawAbility(index, ability, abilityName);
                    EditorGUILayout.Space(3f);
                }

                if (visibleAbilityIndices.Count == 0)
                    EditorGUILayout.HelpBox(
                        BuildEmptyListMessage(),
                        MessageType.Info);

                EditorGUILayout.Space(5f);
                bool addAsVariant =
                    abilityListFilter == AbilityListFilter.Variants;
                string addLabel = addAsVariant
                    ? "+ Add Variant Ability"
                    : "+ Add Core Ability";
                if (GUILayout.Button(
                    addLabel, GUILayout.Height(25f)))
                {
                    AddAbility(addAsVariant);
                    GUIUtility.ExitGUI();
                }
            }
        }

        void RebuildVisibleAbilityIndices()
        {
            appliedSearch = search.Trim();
            visibleAbilityIndices.Clear();
            coreAbilityCount = 0;
            variantAbilityCount = 0;
            visibleCoreCount = 0;
            visibleVariantCount = 0;

            for (int index = 0; index < spellbook.arraySize; index++)
            {
                SerializedProperty ability =
                    spellbook.GetArrayElementAtIndex(index);
                if (IsVariantAbility(ability))
                    variantAbilityCount++;
                else
                    coreAbilityCount++;
            }

            if (abilityListFilter != AbilityListFilter.Variants)
                AppendVisibleAbilities(variantOnly: false);
            if (abilityListFilter != AbilityListFilter.Core)
                AppendVisibleAbilities(variantOnly: true);
        }

        void AppendVisibleAbilities(bool variantOnly)
        {
            for (int index = 0; index < spellbook.arraySize; index++)
            {
                SerializedProperty ability =
                    spellbook.GetArrayElementAtIndex(index);
                if (IsVariantAbility(ability) != variantOnly)
                    continue;

                string abilityName = ability
                    .FindPropertyRelative("abilityName")
                    ?.stringValue ?? $"Ability {index + 1}";
                if (!string.IsNullOrEmpty(appliedSearch) &&
                    abilityName.IndexOf(
                        appliedSearch,
                        StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                visibleAbilityIndices.Add(index);
                if (variantOnly)
                    visibleVariantCount++;
                else
                    visibleCoreCount++;
            }
        }

        static bool IsVariantAbility(SerializedProperty ability)
        {
            return ability?.FindPropertyRelative("variantOnly")
                ?.boolValue == true;
        }

        string BuildEmptyListMessage()
        {
            string viewName = abilityListFilter switch
            {
                AbilityListFilter.Variants => "variant abilities",
                AbilityListFilter.All => "abilities",
                _ => "core abilities"
            };

            return string.IsNullOrEmpty(appliedSearch)
                ? $"No {viewName} are currently available."
                : $"No {viewName} match \"{appliedSearch}\".";
        }

        static void DrawAbilityGroupHeader(string title, int count)
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(title, EyebrowStyle());
                GUILayout.FlexibleSpace();
                GUILayout.Label(
                    $"{count} shown",
                    EditorStyles.miniLabel);
            }
            EditorGUILayout.Space(1f);
        }

        void DrawAbility(
            int index, SerializedProperty ability, string abilityName)
        {
            string key = ClassNames[classIndex] + ":" + index;
            bool expanded = expandedAbilities.Contains(key);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                bool nextExpanded = expanded;
                Sprite abilityIcon = ability
                    .FindPropertyRelative("icon")
                    ?.objectReferenceValue as Sprite;
                RepaintWhilePreviewLoads(abilityIcon);

                bool toggleRequested = false;
                using (new EditorGUILayout.HorizontalScope(
                    GUILayout.Height(AbilityIconSize)))
                {
                    Rect foldoutRect =
                        GUILayoutUtility.GetRect(
                            GUIContent.none,
                            GUIStyle.none,
                            GUILayout.Width(16f),
                            GUILayout.Height(AbilityIconSize));
                    toggleRequested = EditorGUI.DropdownButton(
                        foldoutRect,
                        new GUIContent(expanded ? "▼" : "▶"),
                        FocusType.Passive,
                        FoldoutGlyphStyle());

                    Rect iconRect =
                        GUILayoutUtility.GetRect(
                            GUIContent.none,
                            GUIStyle.none,
                            GUILayout.Width(AbilityIconSize),
                            GUILayout.Height(AbilityIconSize));
                    DrawAbilityIcon(iconRect, abilityIcon);

                    GUILayout.Label(
                        new GUIContent(
                            $"{index + 1}. {abilityName}",
                            abilityIcon != null
                                ? $"Icon: {abilityIcon.name}"
                                : "No spell icon assigned"),
                        AbilityNameStyle(),
                        GUILayout.Height(AbilityIconSize),
                        GUILayout.MinWidth(80f));
                    if (IsVariantAbility(ability))
                    {
                        GUILayout.Label(
                            new GUIContent(
                                "VARIANT",
                                "Referenced by a core ability and hidden " +
                                "from the player loadout."),
                            VariantBadgeStyle(),
                            GUILayout.Width(58f),
                            GUILayout.Height(18f));
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(
                        AbilitySummaryText(ability),
                        AbilitySummaryStyle(),
                        GUILayout.Width(210f),
                        GUILayout.Height(AbilityIconSize));
                }

                if (toggleRequested)
                {
                    nextExpanded = !expanded;
                    if (nextExpanded)
                        expandedAbilities.Add(key);
                    else
                    {
                        expandedAbilities.Remove(key);
                        StopPreviewIfActive(key);
                    }
                    Repaint();
                    GUIUtility.ExitGUI();
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
                GUIUtility.ExitGUI();
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

        void DrawLogisticsTab(SerializedProperty ability)
        {
            DrawFieldGroup(
                "IDENTITY & TARGETING", ability,
                "abilityName",
                "description",
                "variantOnly",
                "icon",
                "category",
                "shape",
                "range",
                "coneAngle",
                "useFixedConeRange",
                "rectWidth",
                "indicatorSize",
                "targetTag");

            DrawMovementLogistics(ability);

            DrawFieldGroup(
                "TIMING & COST", ability,
                "cooldown",
                "manaCost");

            DrawFieldGroup(
                "DAMAGE & CHARGE", ability,
                "damage",
                "damageDelay",
                "chargeable",
                "maxChargeTime",
                "maxChargeDamage",
                "maxChargeSizeMultiplier");

            DrawCrowdControlGroup(ability);

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

            SerializedProperty crowdControl =
                ability.FindPropertyRelative("crowdControlType");
            bool usesSpellbookPull = crowdControl != null &&
                crowdControl.enumValueIndex ==
                (int)AbilityCrowdControlType.Pull;

            if (usesSpellbookPull)
            {
                DrawFieldGroup(
                    "ADVANCED EFFECTS", ability,
                    "chainTargets",
                    "chainDamageFalloff",
                    "usePulseDamage",
                    "pulseCount",
                    "pulseInterval",
                    "pulseRadius",
                    "pulseDamage",
                    "variants");
            }
            else
            {
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
        }

        void DrawCrowdControlGroup(
            SerializedProperty ability)
        {
            SerializedProperty controlType =
                ability.FindPropertyRelative("crowdControlType");

            EditorGUILayout.LabelField(
                "CROWD CONTROL", EyebrowStyle());
            using (new EditorGUILayout.VerticalScope(
                EditorStyles.helpBox))
            {
                if (controlType == null)
                    return;

                EditorGUILayout.PropertyField(controlType);
                if (controlType.enumValueIndex !=
                    (int)AbilityCrowdControlType.Pull)
                    return;

                EditorGUILayout.PropertyField(
                    ability.FindPropertyRelative(
                        "pullDestination"));
                EditorGUILayout.PropertyField(
                    ability.FindPropertyRelative(
                        "pullDuration"));
                EditorGUILayout.PropertyField(
                    ability.FindPropertyRelative(
                        "pullSpeed"));
                EditorGUILayout.PropertyField(
                    ability.FindPropertyRelative(
                        "pullStopDistance"));
            }
        }

        void DrawAnimationTab(SerializedProperty ability)
        {
            DrawFieldGroup(
                "CAST", ability,
                "castTime",
                "marauderCastAnimation");
        }

        void DrawMovementLogistics(SerializedProperty ability)
        {
            SerializedProperty movesCaster =
                ability.FindPropertyRelative("moveCasterToTarget");
            SerializedProperty instantMovement =
                ability.FindPropertyRelative("instantMovement");
            SerializedProperty movementTiming =
                ability.FindPropertyRelative("movementTiming");
            SerializedProperty moveToSpeed =
                ability.FindPropertyRelative("moveToSpeed");
            SerializedProperty fixedMovementDuration =
                ability.FindPropertyRelative("fixedMovementDuration");
            SerializedProperty movementArcHeight =
                ability.FindPropertyRelative("movementArcHeight");
            SerializedProperty resolveEffectsOnLanding =
                ability.FindPropertyRelative("resolveEffectsOnLanding");
            SerializedProperty animationLandingPoint =
                ability.FindPropertyRelative("animationLandingPoint");

            using (new EditorGUILayout.VerticalScope(
                EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    "MOVEMENT", EyebrowStyle());

                MovementType movementType = GetMovementType(
                    movesCaster,
                    instantMovement,
                    movementArcHeight);
                MovementType nextMovementType =
                    (MovementType)EditorGUILayout.EnumPopup(
                        new GUIContent(
                            "Movement Type",
                            "How casting this spell moves its caster."),
                        movementType);
                if (nextMovementType != movementType)
                {
                    SetMovementType(
                        nextMovementType,
                        movesCaster,
                        instantMovement,
                        movementArcHeight);
                    movementType = nextMovementType;
                }

                if (movementType == MovementType.None)
                    return;

                if (movementType != MovementType.Teleport)
                {
                    EditorGUILayout.PropertyField(movementTiming);
                    if (movementTiming.enumValueIndex ==
                        (int)AbilityMovementTiming.FixedDuration)
                    {
                        EditorGUILayout.PropertyField(
                            fixedMovementDuration);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(moveToSpeed);
                    }

                    if (movementType == MovementType.Leap)
                        EditorGUILayout.PropertyField(
                            movementArcHeight);

                    EditorGUILayout.PropertyField(
                        resolveEffectsOnLanding);
                    EditorGUILayout.PropertyField(
                        animationLandingPoint);
                }
            }
        }

        static MovementType GetMovementType(
            SerializedProperty movesCaster,
            SerializedProperty instantMovement,
            SerializedProperty movementArcHeight)
        {
            if (movesCaster == null || !movesCaster.boolValue)
                return MovementType.None;
            if (instantMovement?.boolValue == true)
                return MovementType.Teleport;
            return (movementArcHeight?.floatValue ?? 0f) > 0.01f
                ? MovementType.Leap
                : MovementType.Dash;
        }

        static void SetMovementType(
            MovementType movementType,
            SerializedProperty movesCaster,
            SerializedProperty instantMovement,
            SerializedProperty movementArcHeight)
        {
            if (movesCaster == null ||
                instantMovement == null ||
                movementArcHeight == null)
                return;

            movesCaster.boolValue =
                movementType != MovementType.None;
            instantMovement.boolValue =
                movementType == MovementType.Teleport;

            if (movementType == MovementType.Dash)
                movementArcHeight.floatValue = 0f;
            else if (movementType == MovementType.Leap &&
                movementArcHeight.floatValue <= 0.01f)
                movementArcHeight.floatValue = 3f;
        }

        void DrawVFXTab(
            SerializedProperty ability, string key)
        {
            DrawVFXSelection();
            EditorGUILayout.Space(4f);
            DrawBrowserAssignmentButtons(ability);

            DrawFieldGroup(
                "ASSIGNED PREFABS", ability,
                "castingVFX",
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
                "castVFXAtCaster",
                "hitVFXFollowsTarget",
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
                GameObject castingVFX = ability
                    .FindPropertyRelative("castingVFX")
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
                    castingVFX,
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

        void DrawFieldGroup(
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
                    if (property == null) continue;

                    if (property.propertyType ==
                        SerializedPropertyType.String &&
                        propertyName == "description")
                    {
                        DrawDescriptionProperty(property);
                    }
                    else if (property.propertyType ==
                        SerializedPropertyType.String)
                    {
                        DrawStringProperty(property);
                    }
                    else if (propertyName == "icon" &&
                        property.propertyType ==
                        SerializedPropertyType.ObjectReference)
                    {
                        DrawIconProperty(property);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(
                            property, true);
                    }
                }
            }
        }

        void DrawIconProperty(SerializedProperty property)
        {
            Rect rowRect = EditorGUILayout.GetControlRect();
            Rect fieldRect = EditorGUI.PrefixLabel(
                rowRect,
                new GUIContent(
                    property.displayName,
                    property.tooltip));
            Rect pickerRect = new Rect(
                fieldRect.xMax - 19f,
                fieldRect.y,
                19f,
                fieldRect.height);
            Rect valueRect = new Rect(
                fieldRect.x,
                fieldRect.y,
                fieldRect.width - pickerRect.width,
                fieldRect.height);

            Sprite current =
                property.objectReferenceValue as Sprite;
            GUI.Label(
                valueRect,
                current != null
                    ? current.name
                    : "None (Sprite)",
                EditorStyles.objectField);

            int controlId = GUIUtility.GetControlID(
                property.propertyPath.GetHashCode(),
                FocusType.Passive,
                pickerRect);
            if (GUI.Button(
                pickerRect,
                EditorGUIUtility.IconContent("d_pick"),
                EditorStyles.miniButton))
            {
                activeIconPropertyPath = property.propertyPath;
                activeIconPickerControlId = controlId;
                EditorGUIUtility.ShowObjectPicker<Sprite>(
                    current,
                    false,
                    "t:Sprite",
                    controlId);
            }
        }

        void HandleIconPickerEvent()
        {
            Event currentEvent = Event.current;
            if (currentEvent == null ||
                currentEvent.type != EventType.ExecuteCommand ||
                string.IsNullOrEmpty(activeIconPropertyPath))
                return;

            bool updated =
                currentEvent.commandName == "ObjectSelectorUpdated";
            bool closed =
                currentEvent.commandName == "ObjectSelectorClosed";
            if (!updated && !closed) return;

            int pickerControlId =
                EditorGUIUtility.GetObjectPickerControlID();
            if (pickerControlId != activeIconPickerControlId)
                return;

            SerializedProperty iconProperty =
                serializedCaster.FindProperty(
                    activeIconPropertyPath);
            if (iconProperty != null)
            {
                UnityEngine.Object picked =
                    EditorGUIUtility.GetObjectPickerObject();
                if (iconProperty.objectReferenceValue != picked)
                {
                    iconProperty.objectReferenceValue = picked;
                    serializedCaster.ApplyModifiedProperties();
                    MarkChanged();
                }
            }

            if (closed)
                activeIconPropertyPath = null;

            Repaint();
        }

        static void DrawStringProperty(
            SerializedProperty property)
        {
            string controlName =
                "BCE.Spellbook." + property.propertyPath;
            GUI.SetNextControlName(controlName);
            string nextValue = EditorGUILayout.TextField(
                new GUIContent(
                    property.displayName,
                    property.tooltip),
                property.stringValue);
            if (nextValue != property.stringValue)
                property.stringValue = nextValue;

            if (GUI.GetNameOfFocusedControl() == controlName)
                EditorGUIUtility.editingTextField = true;
        }

        static void DrawDescriptionProperty(
            SerializedProperty property)
        {
            string controlName =
                "BCE.Spellbook." + property.propertyPath;
            EditorGUILayout.LabelField(
                new GUIContent(
                    property.displayName,
                    property.tooltip));
            GUI.SetNextControlName(controlName);
            string nextValue = EditorGUILayout.TextArea(
                property.stringValue,
                GUILayout.MinHeight(54f));
            if (nextValue != property.stringValue)
                property.stringValue = nextValue;

            if (GUI.GetNameOfFocusedControl() == controlName)
                EditorGUIUtility.editingTextField = true;
        }

        void StopPreviewIfActive(string key)
        {
            if (activePreviewAbility != key) return;
            activePreviewAbility = null;
            spellPreview?.Clear();
        }

        static string AbilitySummaryText(
            SerializedProperty ability)
        {
            SerializedProperty category =
                ability.FindPropertyRelative("category");
            SerializedProperty cooldown =
                ability.FindPropertyRelative("cooldown");
            SerializedProperty mana =
                ability.FindPropertyRelative("manaCost");
            SerializedProperty movesCaster =
                ability.FindPropertyRelative("moveCasterToTarget");
            SerializedProperty instantMovement =
                ability.FindPropertyRelative("instantMovement");
            SerializedProperty movementArcHeight =
                ability.FindPropertyRelative("movementArcHeight");

            string categoryName = category != null
                ? category.enumDisplayNames[
                    Mathf.Clamp(category.enumValueIndex, 0,
                        category.enumDisplayNames.Length - 1)]
                : "Ability";
            float cooldownValue = cooldown?.floatValue ?? 0f;
            float manaValue = mana?.floatValue ?? 0f;
            string movementLabel = "";
            if (movesCaster?.boolValue == true)
            {
                movementLabel = instantMovement?.boolValue == true
                    ? "Teleport"
                    : (movementArcHeight?.floatValue ?? 0f) > 0.01f
                        ? "Leap"
                        : "Dash";
            }

            return
                $"{categoryName}" +
                (string.IsNullOrEmpty(movementLabel)
                    ? ""
                    : $"  •  {movementLabel}") +
                $"  •  {cooldownValue:0.#}s  •  " +
                $"{manaValue:0.#} MP";
        }

        static void DrawVFXSummary(SerializedProperty ability)
        {
            GameObject cast = ability.FindPropertyRelative("castVFX")
                ?.objectReferenceValue as GameObject;
            GameObject casting = ability.FindPropertyRelative("castingVFX")
                ?.objectReferenceValue as GameObject;
            GameObject hit = ability.FindPropertyRelative("hitVFX")
                ?.objectReferenceValue as GameObject;
            GameObject deploy = ability
                .FindPropertyRelative("deployablePrefab")
                ?.objectReferenceValue as GameObject;

            string summary =
                $"Casting: {(casting != null ? casting.name : "None")}   " +
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

                if (GUILayout.Button("→ Casting"))
                    AssignVFX(ability, "castingVFX", selectedVFX);
                if (GUILayout.Button("→ Cast"))
                    AssignVFX(ability, "castVFX", selectedVFX);
                if (GUILayout.Button("→ Hit"))
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

        void AddAbility(bool variantOnly)
        {
            serializedCaster.ApplyModifiedProperties();
            Undo.RecordObject(abilityCaster, "Add Spell Forge ability");

            AbilityDef[] current =
                abilityCaster.spellbook ?? Array.Empty<AbilityDef>();
            var next = new AbilityDef[current.Length + 1];
            Array.Copy(current, next, current.Length);
            next[current.Length] = new AbilityDef
            {
                abilityName = variantOnly
                    ? $"New {ClassNames[classIndex]} Variant"
                    : $"New {ClassNames[classIndex]} Ability",
                variantOnly = variantOnly
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
            if (spellbook != null)
                RebuildVisibleAbilityIndices();
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
            pendingSave = true;
            lastSaveMessage = "Unsaved changes";
        }

        void SaveCurrent()
        {
            if (!pendingSave || prefabAsset == null) return;
            if (serializedCaster != null)
                serializedCaster.ApplyModifiedProperties();

            EditorUtility.SetDirty(abilityCaster);
            EditorUtility.SetDirty(prefabAsset);
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

        static void DrawAbilityIcon(
            Rect iconRect, Sprite icon)
        {
            EditorGUI.DrawRect(
                iconRect,
                EditorGUIUtility.isProSkin
                    ? new Color(0.08f, 0.08f, 0.10f)
                    : new Color(0.70f, 0.70f, 0.72f));

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

        static GUIStyle AbilitySummaryStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
        }

        static GUIStyle VariantBadgeStyle()
        {
            var style = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9
            };
            style.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.82f, 0.66f, 1f)
                : new Color(0.42f, 0.18f, 0.62f);
            return style;
        }

        static GUIStyle FoldoutGlyphStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10
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
