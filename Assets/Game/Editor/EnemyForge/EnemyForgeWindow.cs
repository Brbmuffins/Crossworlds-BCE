#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Crossworlds.EditorTools.EnemyForge
{
    internal sealed class EnemyForgeWindow : EditorWindow
    {
        EnemyForgeDefinition definition;
        Vector2 scroll;
        List<EnemyForgeIssue> issues = new List<EnemyForgeIssue>();
        bool acknowledgeCombatSourceChanges;
        bool showCombatSourceChanges;
        GameObject lastAnimationSource;
        GameObject lastBuiltPrefab;
        bool showSource = true;
        bool showIdentity;
        bool showVitals;
        bool showMovement;
        bool showBasicAttack;
        bool showHeavyAttack;
        bool showRewards;
        bool showLifecycle;
        bool showAnimations;
        bool showSfx;
        PreviewRenderUtility animationPreview;
        GameObject animationPreviewInstance;
        GameObject animationPreviewSource;
        bool animationPreviewPlaying = true;
        int animationPreviewState;
        float animationPreviewTime;
        double animationPreviewLastUpdate;
        int validatedConfigurationHash;
        GameObject validatedDeploymentTarget;
        bool validationPassed;
        string definitionBaselineJson;

        [MenuItem("BCE/Enemy Forge", priority = 35)]
        static void Open() => GetWindow<EnemyForgeWindow>("Enemy Forge");

        void OnDisable() => CleanupAnimationPreview();

        void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Enemy Forge", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select or create a local authoring definition. Building creates a new prefab and never modifies the source model.", MessageType.Info);

            var sourceState = EnemyForgeCombatSourceMonitor.Check();
            bool sourceLocked = DrawCombatSourceGuard(sourceState);

            using (new EditorGUI.DisabledScope(sourceLocked))
            {

                using (new EditorGUILayout.HorizontalScope())
                {
                    var next = (EnemyForgeDefinition)EditorGUILayout.ObjectField("Definition", definition, typeof(EnemyForgeDefinition), false);
                    if (next != definition) SetDefinition(next);
                    if (GUILayout.Button("New", GUILayout.Width(60))) CreateDefinition();
                }

                scroll = EditorGUILayout.BeginScrollView(scroll);
                showSource = DrawFoldout("Prefab Source", showSource);
                if (showSource) DrawSourceDropZone();
                if (definition != null)
                {
                    EditorGUILayout.Space(6);
                    EditorGUILayout.LabelField("Network Deployment", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("All Enemy Forge prefabs are server-authoritative Mirror prefabs and are registered for runtime spawning.", MessageType.Info);
                    DrawDeploymentTools();
                    SyncAnimationFolderToSource();
                    DrawConfigurationSections();

                    bool nextShowAnimations = DrawFoldout("Animation State Mapping", showAnimations);
                    if (nextShowAnimations && !showAnimations)
                        EnemyForgeAnimationPreviewWindow.Open(definition);
                    showAnimations = nextShowAnimations;
                    if (showAnimations) DrawAnimationMapping();
                }

                EditorGUILayout.EndScrollView();

                if (definition != null)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField("Validation & Build", EditorStyles.boldLabel);
                    DrawBuildTools();
                }
            }
        }

        void DrawConfigurationSections()
        {
            var serialized = new SerializedObject(definition);
            serialized.Update();
            DrawPropertySection(serialized, "Enemy Identity & Output", ref showIdentity,
                "archetype", "rootTag", "templateId", "outputFolder");
            DrawPropertySection(serialized, "Vitals", ref showVitals, "maxHealth", "robotic");
            DrawPropertySection(serialized, "Movement & Perception", ref showMovement,
                "moveSpeed", "acceleration", "angularSpeed", "agentRadius", "agentHeight",
                "agentBaseOffset", "stoppingDistance", "aggroRadius", "leashRadius",
                "enableRoaming", "roamingRadius", "roamingMinWait", "roamingMaxWait");
            DrawPropertySection(serialized, "Basic Attack", ref showBasicAttack,
                "aggroWhenDamaged", "attackRange", "attackInterval", "damage", "combatTurnSpeed",
                "projectilePrefab", "preferredRange", "tooCloseDistance");
            DrawCastAttackSection(serialized);
            DrawPropertySection(serialized, "Rewards", ref showRewards, "dropTable", "worldItemPrefab");
            DrawPropertySection(serialized, "Lifecycle", ref showLifecycle,
                "deadModelVisibleSeconds", "respawnAfterDeath", "respawnDelay", "corpseGroundOffset");
            DrawPropertySection(serialized, "SFX", ref showSfx,
                "aggroSfx", "attack1Sfx", "attack2Sfx", "attack3Sfx", "attack4Sfx",
                "attackImpactSfx", "getHitSfx", "deathSfx", "sfxVolume",
                "sfxPitchVariation", "sfxMinDistance", "sfxMaxDistance");
            serialized.ApplyModifiedProperties();
        }

        static void DrawPropertySection(SerializedObject serialized, string title, ref bool expanded, params string[] properties)
        {
            expanded = DrawFoldout(title, expanded);
            if (!expanded) return;
            EditorGUI.indentLevel++;
            foreach (string propertyName in properties)
            {
                var property = serialized.FindProperty(propertyName);
                if (property == null) continue;

                if (propertyName == "corpseGroundOffset")
                    DrawCorpseGroundOffset(property);
                else
                    EditorGUILayout.PropertyField(property, true);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(3);
        }

        void DrawCastAttackSection(SerializedObject serialized)
        {
            bool ranged = definition.archetype == EnemyForgeArchetype.Ranged;
            showHeavyAttack = DrawFoldout(ranged ? "Ranged Attack" : "Heavy Attack", showHeavyAttack);
            if (!showHeavyAttack) return;

            EditorGUI.indentLevel++;
            DrawLabeledProperty(serialized, "addHeavyAttack",
                ranged ? "Enable Ranged Attack" : "Enable Heavy Attack");
            DrawLabeledProperty(serialized, "castAttack",
                ranged ? "Ranged Attack Selection" : "Heavy Attack Selection");
            bool usesElementalLightning =
                definition.castAttack == EnemyForgeCastAttack.ElementalLightning ||
                definition.openingCast == EnemyForgeCastAttack.ElementalLightning;
            if (usesElementalLightning)
            {
                SerializedProperty profileProperty =
                    serialized.FindProperty("elementalLightningVfxProfile");
                if (profileProperty != null && profileProperty.objectReferenceValue == null)
                {
                    profileProperty.objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<ElementalLightningVFXProfile>(
                            "Assets/Game/Resources/EnemyAbilities/ElementalLightning.asset");
                }
                DrawLabeledProperty(serialized, "elementalLightningVfxProfile",
                    "Elemental Lightning VFX");
                DrawElementalLightningVfx(
                    profileProperty?.objectReferenceValue as ElementalLightningVFXProfile);
            }
            DrawLabeledProperty(serialized, "heavyMinCooldown", "Minimum Cooldown");
            DrawLabeledProperty(serialized, "heavyMaxCooldown", "Maximum Cooldown");
            DrawLabeledProperty(serialized, "heavyDamageMultiplier", "Damage Multiplier");
            if (ranged)
            {
                DrawLabeledProperty(serialized, "rangedCastDistance", "Cast Distance to Target");
                DrawLabeledProperty(serialized, "castImmediatelyOnAggro", "Cast Immediately on Aggro");
                DrawLabeledProperty(serialized, "openingCast", "Opening Cast");
                DrawLabeledProperty(serialized, "openingCastDelay", "Opening Cast Delay");
                DrawLabeledProperty(serialized, "openingCastOncePerAggro", "Opening Cast Once Per Aggro");
                DrawLabeledProperty(serialized, "openingCastRequiresLineOfSight", "Require Line of Sight");
                DrawLabeledProperty(serialized, "cancelOpeningCastIfTargetInvalid", "Cancel If Target Dies / Leaves Leash");
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(3);
        }

        static void DrawElementalLightningVfx(ElementalLightningVFXProfile profile)
        {
            if (profile == null)
            {
                EditorGUILayout.HelpBox(
                    "Forge once to create the reusable Elemental Lightning profile.",
                    MessageType.Info);
                return;
            }

            var effect = new SerializedObject(profile);
            effect.Update();
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Elemental Lightning Effects", EditorStyles.boldLabel);
            DrawLabeledProperty(effect, "handEffect", "Hand Effect");
            DrawFloatSlider(effect, "handScale", "Hand Scale", 0.05f, 3f);
            DrawFloatSlider(effect, "handThickness", "Hand Thickness", 0.1f, 5f);
            DrawFloatSlider(effect, "handLifetime", "Hand Lifetime (0 = Impact)", 0f, 8f);
            EditorGUILayout.Space(2f);
            DrawLabeledProperty(effect, "spellEffect", "Spell Effect");
            DrawFloatSlider(effect, "spellScale", "Spell Scale", 0.05f, 3f);
            DrawFloatSlider(effect, "spellThickness", "Bolt Thickness", 0.1f, 5f);
            DrawFloatSlider(effect, "spellLifetime", "Bolt Lifetime (0 = 0.75s)", 0f, 8f);
            EditorGUILayout.Space(2f);
            DrawLabeledProperty(effect, "hitEffect", "Hit Effect");
            DrawFloatSlider(effect, "hitScale", "Hit Scale", 0.05f, 3f);
            DrawFloatSlider(effect, "hitThickness", "Hit Thickness", 0.1f, 5f);
            DrawFloatSlider(effect, "hitLifetime", "Hit Lifetime", 0.05f, 8f);
            if (effect.ApplyModifiedProperties())
                EditorUtility.SetDirty(profile);
        }

        static void DrawFloatSlider(SerializedObject serialized, string propertyName,
            string label, float minimum, float maximum)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null)
                property.floatValue = EditorGUILayout.Slider(
                    label, property.floatValue, minimum, maximum);
        }

        static void DrawLabeledProperty(SerializedObject serialized, string propertyName, string label)
        {
            var property = serialized.FindProperty(propertyName);
            if (property != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }

        static void DrawCorpseGroundOffset(SerializedProperty property)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(
                    property.displayName,
                    "Negative values lower the corpse. Positive values raise it."));
                GUILayout.Label("Lower", EditorStyles.miniLabel, GUILayout.Width(36));
                property.floatValue = GUILayout.HorizontalSlider(property.floatValue, -0.5f, 0.5f);
                GUILayout.Label("Raise", EditorStyles.miniLabel, GUILayout.Width(34));
                property.floatValue = Mathf.Clamp(
                    EditorGUILayout.FloatField(property.floatValue, GUILayout.Width(48)),
                    -0.5f,
                    0.5f);
            }
        }

        static bool DrawFoldout(string title, bool expanded)
        {
            EditorGUILayout.Space(4);
            return EditorGUILayout.Foldout(expanded, title, true, EditorStyles.foldoutHeader);
        }

        void DrawBuildTools()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Validate")) RunValidation();
                if (GUILayout.Button("Restore Defaults"))
                {
                    if (EditorUtility.DisplayDialog("Restore Archetype Defaults?",
                        $"Replace the imported or customized health, movement, attack, and heavy-attack values with the {definition.archetype} defaults?\n\nThe source prefab will not be modified.",
                        "Restore Defaults", "Cancel"))
                    {
                        Undo.RecordObject(definition, "Restore enemy archetype defaults");
                        definition.ApplyArchetypeDefaults();
                        EditorUtility.SetDirty(definition);
                    }
                }
            }
            DrawIssues();
            bool validationIsCurrent = IsValidationCurrentAndClean();
            if (!validationIsCurrent)
                EditorGUILayout.HelpBox(validationPassed
                    ? "Configuration changed after validation. Validate again before saving or updating deployed prefabs."
                    : "A clean validation is required before saving or updating deployed prefabs.", MessageType.Warning);
            using (new EditorGUI.DisabledScope(!validationIsCurrent))
            {
                if (GUILayout.Button("Save As New", GUILayout.Height(34)))
                {
                    issues = EnemyForgeValidator.ValidateDefinition(definition);
                    if (!HasErrors())
                    {
                        var prefab = EnemyForgeBuilder.Build(definition);
                        if (prefab != null)
                        {
                            lastBuiltPrefab = prefab;
                            EnemyForgeDeployment.RegisterForNetworkSpawning(prefab);
                            var prefabIssues = EnemyForgeValidator.ValidatePrefab(prefab);
                            issues.AddRange(prefabIssues);
                            if (prefabIssues.Exists(x => x.severity == EnemyForgeSeverity.Error))
                                validationPassed = false;
                            else
                                CaptureDefinitionBaseline();
                        }
                    }
                }
            }
            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(lastBuiltPrefab == null || !validationIsCurrent))
                {
                    if (GUILayout.Button("Deploy", GUILayout.Width(220), GUILayout.Height(28)))
                    {
                        if (EditorUtility.DisplayDialog("Deploy Enemy Changes?",
                            $"WARNING: This will update the current forged prefab '{lastBuiltPrefab.name}' in place, then deploy the same combat and animation changes to all matching associated instances in loaded scenes.\n\nThe original source model will not be modified. Scene-instance changes support Undo.\n\nContinue?",
                            "OK", "Cancel"))
                        {
                            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                            int updated = EnemyForgeBuilder.Deploy(lastBuiltPrefab, definition);
                            if (updated >= 0)
                                EnemyForgeDeployment.RegisterForNetworkSpawning(lastBuiltPrefab);
                            if (updated >= 0) CaptureDefinitionBaseline();
                            EditorUtility.DisplayDialog("Deployment Result",
                                updated < 0 ? "Deployment failed. Review the Unity Console for details." :
                                updated > 0 ? $"Updated the current prefab and deployed changes to {updated} matching scene instance(s)." :
                                "Updated the current prefab. No matching deployed scene instances were found.", "OK");
                        }
                    }
                }
                GUILayout.FlexibleSpace();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(definitionBaselineJson) && !EnemyForgeBuilder.HasDeployBackup()))
                {
                    if (GUILayout.Button("Revert Changes", GUILayout.Width(220), GUILayout.Height(28)))
                        ShowRevertChoices();
                }
                GUILayout.FlexibleSpace();
            }
        }

        void ShowRevertChoices()
        {
            int choice = EditorUtility.DisplayDialogComplex("Revert Enemy Forge Changes",
                "Choose what to restore:\n\n" +
                "Revert Unsaved Settings restores the Forge fields to their last selected/saved baseline.\n\n" +
                "Roll Back Last Deploy restores the prefab, animation override, and loaded scene files captured immediately before the last Deploy. Changes made to those files after that Deploy will be lost.",
                "Revert Unsaved Settings", "Cancel", "Roll Back Last Deploy");
            if (choice == 0)
            {
                if (string.IsNullOrEmpty(definitionBaselineJson))
                {
                    EditorUtility.DisplayDialog("Revert Settings", "No settings baseline is available.", "OK");
                    return;
                }
                Undo.RecordObject(definition, "Revert Enemy Forge settings");
                EditorJsonUtility.FromJsonOverwrite(definitionBaselineJson, definition);
                EditorUtility.SetDirty(definition);
                validationPassed = false;
                validatedConfigurationHash = 0;
                CleanupAnimationPreview();
                issues = EnemyForgeValidator.ValidateDefinition(definition);
                EditorUtility.DisplayDialog("Settings Reverted", "Unsaved Enemy Forge settings were restored. Validate again before saving or deploying.", "OK");
            }
            else if (choice == 2)
            {
                if (!EnemyForgeBuilder.HasDeployBackup())
                {
                    EditorUtility.DisplayDialog("Rollback Unavailable", "No local deployment backup is available.", "OK");
                    return;
                }
                if (!EditorUtility.DisplayDialog("Confirm Last Deploy Rollback",
                    "This will replace the current prefab, animation override, and backed-up scene files with their pre-deploy versions. Any later changes in those files will be lost. Continue?",
                    "Roll Back", "Cancel")) return;
                string result = EnemyForgeBuilder.RollBackLastDeploy();
                validationPassed = false;
                validatedConfigurationHash = 0;
                EditorUtility.DisplayDialog("Rollback Result", result, "OK");
            }
        }

        void CaptureDefinitionBaseline()
        {
            definitionBaselineJson = definition != null ? EditorJsonUtility.ToJson(definition) : null;
        }

        void RunValidation()
        {
            issues = EnemyForgeValidator.ValidateDefinition(definition);
            if (lastBuiltPrefab != null)
                issues.AddRange(EnemyForgeValidator.ValidatePrefab(lastBuiltPrefab));
            validationPassed = !HasErrors();
            validatedConfigurationHash = validationPassed ? GetConfigurationHash() : 0;
            validatedDeploymentTarget = validationPassed ? lastBuiltPrefab : null;
        }

        bool IsValidationCurrentAndClean()
        {
            return validationPassed && validatedConfigurationHash != 0 &&
                   validatedConfigurationHash == GetConfigurationHash() &&
                   validatedDeploymentTarget == lastBuiltPrefab;
        }

        int GetConfigurationHash()
        {
            if (definition == null) return 0;
            unchecked
            {
                string json = EditorJsonUtility.ToJson(definition);
                int hash = 17;
                for (int i = 0; i < json.Length; i++) hash = hash * 31 + json[i];
                return hash == 0 ? 1 : hash;
            }
        }

        void DrawDeploymentTools()
        {
            var nextTarget = (GameObject)EditorGUILayout.ObjectField("Deployment Target", lastBuiltPrefab, typeof(GameObject), false);
            if (nextTarget != lastBuiltPrefab)
            {
                lastBuiltPrefab = nextTarget;
                validationPassed = false;
                validatedConfigurationHash = 0;
                validatedDeploymentTarget = null;
            }
            if (lastBuiltPrefab == null)
            {
                EditorGUILayout.LabelField("Build an enemy, or select an existing forged prefab to enable testing and deployment actions.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

            bool registered = EnemyForgeDeployment.IsNetworkRegistered(lastBuiltPrefab);
            EditorGUILayout.HelpBox(registered
                ? "Network status: Registered for Mirror runtime spawning. Applying again will safely repair or update its network components."
                : "Network registration is required. Save As New or Deploy will configure Mirror synchronization and register this prefab with RodNetworkManager.",
                registered ? MessageType.Info : MessageType.Warning);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Validate Readiness"))
                    issues = EnemyForgeDeployment.ValidateReadiness(lastBuiltPrefab);
                if (GUILayout.Button("Place in Current Scene"))
                    EnemyForgeDeployment.PlaceInCurrentScene(lastBuiltPrefab);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Replace Selected Scene Object"))
                    EnemyForgeDeployment.ReplaceSelected(lastBuiltPrefab);
                if (GUILayout.Button(registered ? "Repair Network Deployment" : "Apply Network Deployment"))
                    EnemyForgeDeployment.RegisterForNetworkSpawning(lastBuiltPrefab);
            }
            EditorGUILayout.HelpBox("After deployment, spawn the prefab from the server with NetworkServer.Spawn, or save a placed instance as a Mirror scene object.", MessageType.None);
        }

        bool DrawCombatSourceGuard(EnemyForgeSourceState state)
        {
            if (!state.requiresAcknowledgement)
            {
                acknowledgeCombatSourceChanges = false;
                return false;
            }

            EditorGUILayout.HelpBox(state.summary, MessageType.Warning);
            int safeCount = state.safeChanges != null ? state.safeChanges.Length : 0;
            int warningCount = state.blockingChanges != null ? state.blockingChanges.Length : 0;
            showCombatSourceChanges = EditorGUILayout.Foldout(
                showCombatSourceChanges,
                $"Review combat changes ({safeCount} safe, {warningCount} requiring confirmation)",
                true);
            if (showCombatSourceChanges)
            {
                if (state.safeChanges != null)
                    foreach (string change in state.safeChanges)
                        EditorGUILayout.HelpBox("Safe: " + change, MessageType.Info);
                if (state.blockingChanges != null)
                    foreach (string change in state.blockingChanges)
                        EditorGUILayout.HelpBox("Review: " + change, MessageType.Error);
            }

            using (new EditorGUI.DisabledScope(!state.CanAcknowledge))
            {
                bool next = EditorGUILayout.ToggleLeft(
                    "I acknowledge the audited combat changes and accept the updated contract",
                    acknowledgeCombatSourceChanges);
                if (next && !acknowledgeCombatSourceChanges)
                {
                    if (!state.AuditPassed && !EditorUtility.DisplayDialog(
                            "Accept Updated Combat Contract?",
                            "Serialized combat fields were removed, renamed, or changed. " +
                            "Accept only after the Enemy Forge adapter has been updated for these changes. " +
                            "This records the current combat contract as the new local baseline.",
                            "Accept and Continue",
                            "Cancel"))
                    {
                        acknowledgeCombatSourceChanges = false;
                        return true;
                    }

                    EnemyForgeCombatSourceMonitor.Acknowledge(state.fingerprint);
                    acknowledgeCombatSourceChanges = false;
                    EnemyForgeSourceState verifiedState = EnemyForgeCombatSourceMonitor.Check();
                    if (verifiedState.requiresAcknowledgement)
                    {
                        EditorUtility.DisplayDialog(
                            "Acknowledgement Not Saved",
                            "Enemy Forge could not save the updated combat contract. The authoring controls remain locked.",
                            "OK");
                        return true;
                    }

                    issues = definition != null
                        ? EnemyForgeValidator.ValidateDefinition(definition)
                        : new List<EnemyForgeIssue>();
                    Repaint();
                    return false;
                }
                acknowledgeCombatSourceChanges = next;
            }
            if (!state.CanAcknowledge)
                EditorGUILayout.LabelField(
                    "Restore the missing required combat source before acknowledgement can be accepted.",
                    EditorStyles.wordWrappedLabel);
            return true;
        }

        void CreateDefinition()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Enemy Forge Definition", "EnemyDefinition", "asset", "Choose where to store the local authoring definition.", "Assets/Game/Data/EnemyForge");
            if (string.IsNullOrEmpty(path)) return;
            var asset = CreateInstance<EnemyForgeDefinition>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            SetDefinition(asset);
        }

        void DrawSourceDropZone()
        {
            EditorGUILayout.Space(5);
            var rect = GUILayoutUtility.GetRect(0f, 64f, GUILayout.ExpandWidth(true));
            var previous = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.65f, 0.82f, 1f);
            GUI.Box(rect, definition != null && definition.source != null
                ? $"Source: {definition.source.name}\nDrop another prefab or model to replace it"
                : "Drag a prefab or imported model here", EditorStyles.helpBox);
            GUI.backgroundColor = previous;

            if (definition != null && definition.source != null)
                DrawSourcePreview(definition.source);

            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition) ||
                (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)) return;

            var candidate = FirstValidDraggedSource();
            DragAndDrop.visualMode = candidate != null ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            if (evt.type == EventType.DragPerform && candidate != null)
            {
                DragAndDrop.AcceptDrag();
                if (definition == null) SetDefinition(CreateInstance<EnemyForgeDefinition>());
                Undo.RecordObject(definition, "Assign Enemy Forge source");
                definition.source = candidate;
                if (IsEnemyForgePrefab(candidate)) lastBuiltPrefab = candidate;
                string sourcePath = AssetDatabase.GetAssetPath(candidate).Replace('\\', '/');
                string sourceFolder = System.IO.Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
                if (!string.IsNullOrEmpty(sourceFolder)) definition.outputFolder = sourceFolder;
                SyncAnimationFolderToSource(true);
                if (string.IsNullOrWhiteSpace(definition.templateId) || definition.templateId == "enemy_basic")
                    definition.templateId = MakeTemplateId(candidate.name);
                string importSummary = definition.ImportFromSource(candidate);
                if (IsEnemyForgePrefab(candidate)) definition.agentBaseOffset = 0f;
                EditorUtility.SetDirty(definition);
                issues = EnemyForgeValidator.ValidateDefinition(definition);
                issues.Insert(0, new EnemyForgeIssue(EnemyForgeSeverity.Info, importSummary));
                CaptureDefinitionBaseline();
            }
            evt.Use();
        }

        void DrawSourcePreview(GameObject source)
        {
            const float previewHeight = 180f;
            Rect previewRect = GUILayoutUtility.GetRect(0f, previewHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, new Color(0.12f, 0.12f, 0.12f, 1f));

            Texture2D preview = AssetPreview.GetAssetPreview(source);
            bool isLoading = AssetPreview.IsLoadingAssetPreview(source.GetEntityId());
            if (preview == null && !isLoading)
                preview = AssetPreview.GetMiniThumbnail(source);

            if (preview != null)
            {
                Rect imageRect = FitTexture(previewRect, preview.width, preview.height);
                GUI.DrawTexture(imageRect, preview, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GUI.Label(previewRect, isLoading ? "Generating prefab preview…" : "Preview unavailable", EditorStyles.centeredGreyMiniLabel);
            }

            if (isLoading) Repaint();
        }

        List<KeyValuePair<string, AnimationClip>> GetMappedAnimationStates()
        {
            var states = new List<KeyValuePair<string, AnimationClip>>();
            if (definition == null) return states;
            AddPreviewState(states, "Idle", definition.idleAnimation);
            AddPreviewState(states, "Chase", definition.chaseAnimation);
            AddPreviewState(states, "Attack 1", definition.attackAnimation);
            AddPreviewState(states, "Attack 2", definition.attackAnimation2);
            AddPreviewState(states, "Attack 3", definition.attackAnimation3);
            AddPreviewState(states, "Attack 4", definition.attackAnimation4);
            AddPreviewState(states, "Get Hit", definition.getHitAnimation);
            AddPreviewState(states, "Death", definition.deathAnimation);
            return states;
        }

        static void AddPreviewState(List<KeyValuePair<string, AnimationClip>> states, string name, AnimationClip clip)
        {
            if (clip != null) states.Add(new KeyValuePair<string, AnimationClip>(name, clip));
        }

        bool DrawAnimatedSourcePreview(GameObject source, Rect rect, List<KeyValuePair<string, AnimationClip>> states)
        {
            EnsureAnimationPreview(source);
            if (animationPreview == null || animationPreviewInstance == null) return false;
            animationPreviewState = Mathf.Clamp(animationPreviewState, 0, states.Count - 1);
            AnimationClip clip = states[animationPreviewState].Value;
            float duration = Mathf.Max(clip.length, 0.01f);

            double now = EditorApplication.timeSinceStartup;
            if (animationPreviewLastUpdate <= 0d) animationPreviewLastUpdate = now;
            if (animationPreviewPlaying && Event.current.type == EventType.Repaint)
            {
                animationPreviewTime += (float)(now - animationPreviewLastUpdate);
                if (animationPreviewTime >= duration)
                {
                    animationPreviewTime = 0f;
                    animationPreviewState = (animationPreviewState + 1) % states.Count;
                    clip = states[animationPreviewState].Value;
                }
            }
            animationPreviewLastUpdate = now;
            clip.SampleAnimation(animationPreviewInstance, Mathf.Clamp(animationPreviewTime, 0f, Mathf.Max(clip.length, 0.01f)));

            Bounds bounds = CalculatePreviewBounds(animationPreviewInstance);
            Vector3 center = bounds.center;
            float radius = Mathf.Max(bounds.extents.magnitude, 0.5f);
            // Unity characters normally face +Z. Place the preview camera on
            // that side looking back at the model so the front faces the viewer.
            animationPreview.camera.transform.position = center + new Vector3(0f, radius * 0.15f, radius * 2.6f);
            animationPreview.camera.transform.LookAt(center);
            animationPreview.camera.nearClipPlane = Mathf.Max(0.01f, radius * 0.01f);
            animationPreview.camera.farClipPlane = radius * 8f;

            animationPreview.BeginPreview(rect, GUIStyle.none);
            animationPreview.Render(true);
            Texture result = animationPreview.EndPreview();
            GUI.DrawTexture(rect, result, ScaleMode.StretchToFill, false);
            if (animationPreviewPlaying) Repaint();
            return true;
        }

        void DrawAnimationPreviewControls(List<KeyValuePair<string, AnimationClip>> states)
        {
            animationPreviewState = Mathf.Clamp(animationPreviewState, 0, states.Count - 1);
            AnimationClip clip = states[animationPreviewState].Value;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(animationPreviewPlaying ? "Pause" : "Play", GUILayout.Width(58)))
                {
                    animationPreviewPlaying = !animationPreviewPlaying;
                    animationPreviewLastUpdate = EditorApplication.timeSinceStartup;
                }
                int next = EditorGUILayout.Popup(animationPreviewState, states.Select(x => x.Key).ToArray());
                if (next != animationPreviewState)
                {
                    animationPreviewState = next;
                    animationPreviewTime = 0f;
                }
            }
            float normalized = clip.length > 0f ? Mathf.Clamp01(animationPreviewTime / clip.length) : 0f;
            Rect progress = GUILayoutUtility.GetRect(0f, 18f, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(progress, normalized, $"{states[animationPreviewState].Key}  {animationPreviewTime:0.00}s / {clip.length:0.00}s");
        }

        void EnsureAnimationPreview(GameObject source)
        {
            if (animationPreview != null && animationPreviewInstance != null && animationPreviewSource == source) return;
            CleanupAnimationPreview();
            animationPreview = new PreviewRenderUtility();
            animationPreview.camera.fieldOfView = 30f;
            animationPreview.camera.clearFlags = CameraClearFlags.Color;
            animationPreview.camera.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);
            animationPreview.lights[0].intensity = 1.2f;
            animationPreview.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
            animationPreview.lights[1].intensity = 1.2f;
            animationPreviewInstance = Instantiate(source);
            animationPreviewInstance.name = source.name + "_EnemyForgePreview";
            foreach (var behaviour in animationPreviewInstance.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour != null) behaviour.enabled = false;
            animationPreview.AddSingleGO(animationPreviewInstance);
            animationPreviewSource = source;
            animationPreviewTime = 0f;
            animationPreviewLastUpdate = EditorApplication.timeSinceStartup;
        }

        void CleanupAnimationPreview()
        {
            if (animationPreview != null) animationPreview.Cleanup();
            animationPreview = null;
            animationPreviewInstance = null;
            animationPreviewSource = null;
            animationPreviewLastUpdate = 0d;
        }

        static Bounds CalculatePreviewBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(Vector3.up, Vector3.one * 2f);
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        static Rect FitTexture(Rect area, float width, float height)
        {
            if (width <= 0f || height <= 0f) return area;
            float scale = Mathf.Min(area.width / width, area.height / height);
            float fittedWidth = width * scale;
            float fittedHeight = height * scale;
            return new Rect(
                area.x + (area.width - fittedWidth) * 0.5f,
                area.y + (area.height - fittedHeight) * 0.5f,
                fittedWidth,
                fittedHeight);
        }

        void SyncAnimationFolderToSource(bool force = false)
        {
            if (definition == null) return;
            if (!force && definition.source == lastAnimationSource) return;

            Undo.RecordObject(definition, "Change Enemy Forge model source");
            lastAnimationSource = definition.source;
            definition.animationFolder = null;
            definition.idleAnimation = null;
            definition.chaseAnimation = null;
            definition.attackAnimation = null;
            definition.attackAnimation2 = null;
            definition.attackAnimation3 = null;
            definition.attackAnimation4 = null;
            definition.attackImpactPoint2 = 0.45f;
            definition.attackImpactPoint3 = 0.45f;
            definition.attackImpactPoint4 = 0.45f;
            definition.getHitAnimation = null;
            definition.deathAnimation = null;

            string folder = EnemyForgeAnimationLibrary.FindSuggestedFolder(definition.source);
            if (!string.IsNullOrEmpty(folder))
                definition.animationFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
            PopulateAnimationMappingsFromSourceOverride();
            EditorUtility.SetDirty(definition);
            OpenAnimationPreviewOnNextEditorUpdate(definition);
        }

        void PopulateAnimationMappingsFromSourceOverride()
        {
            if (definition.source == null) return;
            var animator = definition.source.GetComponentInChildren<Animator>(true);
            var overrideController = animator != null
                ? animator.runtimeAnimatorController as AnimatorOverrideController
                : null;
            if (overrideController == null) return;

            var mappings = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(mappings);
            foreach (var mapping in mappings)
            {
                if (mapping.Key == null) continue;
                switch (mapping.Key.name)
                {
                    case "EnemyForge_Idle":
                        definition.idleAnimation = mapping.Value;
                        break;
                    case "EnemyForge_Chase":
                        definition.chaseAnimation = mapping.Value;
                        break;
                    case "EnemyForge_Attack":
                        definition.attackAnimation = mapping.Value;
                        break;
                    case "EnemyForge_Attack2":
                        definition.attackAnimation2 = mapping.Value;
                        break;
                    case "EnemyForge_Attack3":
                        definition.attackAnimation3 = mapping.Value;
                        break;
                    case "EnemyForge_Attack4":
                        definition.attackAnimation4 = mapping.Value;
                        break;
                    case "EnemyForge_GetHit":
                        definition.getHitAnimation = mapping.Value;
                        break;
                    case "EnemyForge_Death":
                        definition.deathAnimation = mapping.Value;
                        break;
                }
            }

            var controller = definition.source.GetComponent<EnemyController>();
            var delays = controller != null ? controller.attackAnimationImpactDelays : null;
            if (delays == null) return;
            if (definition.attackAnimation2 != null && definition.attackAnimation2.length > 0.01f &&
                delays.Length > 1)
                definition.attackImpactPoint2 = Mathf.Clamp01(delays[1] / definition.attackAnimation2.length);
            if (definition.attackAnimation3 != null && definition.attackAnimation3.length > 0.01f &&
                delays.Length > 2)
                definition.attackImpactPoint3 = Mathf.Clamp01(delays[2] / definition.attackAnimation3.length);
            if (definition.attackAnimation4 != null && definition.attackAnimation4.length > 0.01f &&
                delays.Length > 3)
                definition.attackImpactPoint4 = Mathf.Clamp01(delays[3] / definition.attackAnimation4.length);
        }

        void DrawAnimationMapping()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Open Animation Preview", GUILayout.Width(168)))
                    EnemyForgeAnimationPreviewWindow.Open(definition);
            }
            bool supportsOverrides = definition.animationDriverMode != EnemyForgeAnimationDriverMode.ExistingModelDriver;
            var nextMode = (EnemyForgeAnimationDriverMode)EditorGUILayout.EnumPopup("Animation Driver Mode", definition.animationDriverMode);
            if (nextMode != definition.animationDriverMode)
            {
                Undo.RecordObject(definition, "Change animation driver mode");
                definition.animationDriverMode = nextMode;
                EditorUtility.SetDirty(definition);
            }
            using (new EditorGUI.DisabledScope(!supportsOverrides))
            {
            bool generate = EditorGUILayout.Toggle("Generate Animator Override", definition.generateAnimatorController);
            if (generate != definition.generateAnimatorController)
            {
                Undo.RecordObject(definition, "Toggle Animator generation");
                definition.generateAnimatorController = generate;
                EditorUtility.SetDirty(definition);
            }
            }
            if (!supportsOverrides)
                EditorGUILayout.HelpBox("Existing Model Driver mode preserves the source Animator Controller. Clip mappings below are saved but are not assigned during build.", MessageType.Info);
            string folderPath = definition.animationFolder != null
                ? AssetDatabase.GetAssetPath(definition.animationFolder)
                : string.Empty;
            var clips = EnemyForgeAnimationLibrary.LoadClips(folderPath);

            DrawAnimationChoice("Idle", clips, ref definition.idleAnimation);
            DrawAnimationSpeedSlider("Animation Speed", definition.idleAnimation, ref definition.idleAnimationSpeed);
            DrawAnimationChoice("Chase / Movement", clips, ref definition.chaseAnimation);
            DrawAnimationSpeedSlider("Animation Speed", definition.chaseAnimation, ref definition.chaseAnimationSpeed);
            DrawAnimationChoice("Combat Attack 1", clips, ref definition.attackAnimation);
            DrawAnimationSpeedSlider("Animation Speed", definition.attackAnimation, ref definition.attackAnimationSpeed);
            DrawAttackTimingSlider("Attack 1 Hit / VFX Timing", definition.attackAnimation,
                definition.attackAnimationSpeed, ref definition.attackImpactPoint);
            DrawAnimationChoice("Combat Attack 2", clips, ref definition.attackAnimation2);
            DrawAnimationSpeedSlider("Animation Speed", definition.attackAnimation2, ref definition.attackAnimationSpeed2);
            DrawAttackTimingSlider("Attack 2 Hit / VFX Timing", definition.attackAnimation2,
                definition.attackAnimationSpeed2, ref definition.attackImpactPoint2);
            DrawAnimationChoice("Combat Attack 3", clips, ref definition.attackAnimation3);
            DrawAnimationSpeedSlider("Animation Speed", definition.attackAnimation3, ref definition.attackAnimationSpeed3);
            DrawAttackTimingSlider("Attack 3 Hit / VFX Timing", definition.attackAnimation3,
                definition.attackAnimationSpeed3, ref definition.attackImpactPoint3);
            DrawAnimationChoice("Combat Attack 4", clips, ref definition.attackAnimation4);
            DrawAnimationSpeedSlider("Animation Speed", definition.attackAnimation4, ref definition.attackAnimationSpeed4);
            DrawAttackTimingSlider("Attack 4 Hit / VFX Timing", definition.attackAnimation4,
                definition.attackAnimationSpeed4, ref definition.attackImpactPoint4);
            DrawAnimationChoice("Get Hit", clips, ref definition.getHitAnimation);
            DrawAnimationSpeedSlider("Animation Speed", definition.getHitAnimation, ref definition.getHitAnimationSpeed);
            DrawAnimationChoice("Death", clips, ref definition.deathAnimation);
            DrawAnimationSpeedSlider("Animation Speed", definition.deathAnimation, ref definition.deathAnimationSpeed);
        }

        void DrawAnimationChoice(string label, List<AnimationClip> clips, ref AnimationClip selected)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var available = new List<AnimationClip>(clips);
                if (selected != null && !available.Contains(selected)) available.Add(selected);
                var options = new List<string> { "None" };
                options.AddRange(available.ConvertAll(EnemyForgeAnimationLibrary.GetDisplayName));
                int current = selected == null ? 0 : available.IndexOf(selected) + 1;
                if (current < 0) current = 0;
                int next = EditorGUILayout.Popup(label, current, options.ToArray());
                if (next != current)
                {
                    Undo.RecordObject(definition, "Assign " + label + " animation");
                    selected = next == 0 ? null : available[next - 1];
                    EditorUtility.SetDirty(definition);
                    if (selected != null)
                        OpenAnimationPreviewOnNextEditorUpdate(definition);
                }
                if (GUILayout.Button("Find File", GUILayout.Width(75)))
                {
                    var clip = EnemyForgeAnimationLibrary.BrowseForClip();
                    if (clip != null)
                    {
                        Undo.RecordObject(definition, "Assign " + label + " animation");
                        selected = clip;
                        EditorUtility.SetDirty(definition);
                        OpenAnimationPreviewOnNextEditorUpdate(definition);
                        Repaint();
                    }
                }
            }
        }

        static void OpenAnimationPreviewOnNextEditorUpdate(EnemyForgeDefinition target)
        {
            if (target == null || !HasAssignedAnimation(target)) return;
            EditorApplication.delayCall += () =>
            {
                if (target != null)
                    EnemyForgeAnimationPreviewWindow.Open(target);
            };
        }

        static bool HasAssignedAnimation(EnemyForgeDefinition target)
        {
            return target != null &&
                (target.idleAnimation != null ||
                 target.chaseAnimation != null ||
                 target.attackAnimation != null ||
                 target.attackAnimation2 != null ||
                 target.attackAnimation3 != null ||
                 target.attackAnimation4 != null ||
                 target.getHitAnimation != null ||
                 target.deathAnimation != null);
        }

        void DrawAttackTimingSlider(string label, AnimationClip clip, float playbackSpeed,
            ref float normalizedPoint)
        {
            using (new EditorGUI.DisabledScope(clip == null))
            {
                EditorGUI.indentLevel++;
                float next = EditorGUILayout.Slider(
                    new GUIContent(label,
                        "Normalized position inside this animation. Move right to place the hit/VFX closer to the end."),
                    normalizedPoint, 0f, 1f);
                if (!Mathf.Approximately(next, normalizedPoint))
                {
                    Undo.RecordObject(definition, "Adjust " + label);
                    normalizedPoint = next;
                    EditorUtility.SetDirty(definition);
                }

                if (clip != null)
                {
                    float playedLength = clip.length / Mathf.Max(0.25f, playbackSpeed);
                    float impactSeconds = playedLength * normalizedPoint;
                    float secondsBeforeEnd = Mathf.Max(0f, playedLength - impactSeconds);
                    EditorGUILayout.LabelField(
                        $"{impactSeconds:0.00}s from start  •  {secondsBeforeEnd:0.00}s before animation end",
                        EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
        }

        void DrawAnimationSpeedSlider(string label, AnimationClip clip, ref float speed)
        {
            using (new EditorGUI.DisabledScope(clip == null))
            {
                EditorGUI.indentLevel++;
                float next = EditorGUILayout.Slider(
                    new GUIContent(label, "Playback multiplier. 1.0 is the animation's imported speed."),
                    speed, 0.25f, 3f);
                if (!Mathf.Approximately(next, speed))
                {
                    Undo.RecordObject(definition, "Adjust " + label);
                    speed = next;
                    EditorUtility.SetDirty(definition);
                }
                EditorGUI.indentLevel--;
            }
        }

        static GameObject FirstValidDraggedSource()
        {
            foreach (var dragged in DragAndDrop.objectReferences)
            {
                var go = dragged as GameObject;
                if (go != null && AssetDatabase.Contains(go)) return go;
            }
            return null;
        }

        static string MakeTemplateId(string value)
        {
            var chars = value.ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!char.IsLetterOrDigit(chars[i])) chars[i] = '_';
            return new string(chars).Trim('_') + "_basic";
        }

        void SetDefinition(EnemyForgeDefinition value)
        {
            definition = value;
            lastAnimationSource = definition != null ? definition.source : null;
            if (definition != null && IsEnemyForgePrefab(definition.source))
                lastBuiltPrefab = definition.source;
            issues.Clear();
            validationPassed = false;
            validatedConfigurationHash = 0;
            validatedDeploymentTarget = null;
            CaptureDefinitionBaseline();
            OpenAnimationPreviewOnNextEditorUpdate(definition);
        }

        static bool IsEnemyForgePrefab(GameObject candidate)
        {
            if (candidate == null || !PrefabUtility.IsPartOfPrefabAsset(candidate)) return false;
            var animator = candidate.GetComponentInChildren<Animator>(true);
            var overrideController = animator != null
                ? animator.runtimeAnimatorController as AnimatorOverrideController
                : null;
            if (overrideController == null || overrideController.runtimeAnimatorController == null) return false;
            return AssetDatabase.GetAssetPath(overrideController.runtimeAnimatorController)
                .Equals("Assets/Game/Data/EnemyForge/EnemyForge_Base.controller", System.StringComparison.OrdinalIgnoreCase);
        }

        void DrawIssues()
        {
            foreach (var issue in issues)
            {
                var type = issue.severity == EnemyForgeSeverity.Error ? MessageType.Error :
                    issue.severity == EnemyForgeSeverity.Warning ? MessageType.Warning : MessageType.Info;
                EditorGUILayout.HelpBox(issue.message, type);
            }
        }

        bool HasErrors()
        {
            foreach (var issue in issues) if (issue.severity == EnemyForgeSeverity.Error) return true;
            return false;
        }
    }
}
#endif
