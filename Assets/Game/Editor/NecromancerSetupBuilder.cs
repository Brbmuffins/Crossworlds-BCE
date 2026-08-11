#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Mirror;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the complete local Necromancer class at canonical class index 5.
/// Existing class indices 0-4 are never changed.
/// </summary>
public static class NecromancerSetupBuilder
{
    const int ClassIndex = 5;
    const string AutoBuildSessionKey = "BCE.Necromancer.AutoBuildAttempted";
    const string HeroDir = "Assets/Game/3D Models/Heroes/Necromancer";
    const string SourcePrefabPath = "Assets/Game/Game_Prefabs/Cleric.prefab";
    const string PrefabPath = "Assets/Game/Game_Prefabs/Necromancer.prefab";
    const string ControllerPath = HeroDir + "/Necromancer.controller";
    const string MaterialPath = HeroDir + "/Necromancer.mat";
    const string PoolPath = "Assets/Game/Data/ClassPools/Necromancer_Pool.asset";
    const string CharacterDataPath = "Assets/Game/Data/CharacterSelect/Necromancer.asset";
    const string PortraitPath = "Assets/Game/Art/Class Portraits/Necromancer.png";

    static readonly string IdlePath = HeroDir + "/Zombie Idle.fbx";
    static readonly string CombatIdlePath = HeroDir + "/Zombie Idle (1).fbx";
    static readonly string RunPath = HeroDir + "/Zombie Running.fbx";
    static readonly string JumpPath = HeroDir + "/Jump.fbx";
    static readonly string AttackPath = HeroDir + "/Standing 1H Magic Attack 01.fbx";
    static readonly string CastPath = HeroDir + "/Standing 1H Magic Attack 03.fbx";
    static readonly string CastAltPath = HeroDir + "/Standing 1H Magic Attack 03 (1).fbx";
    static readonly string AreaCastPath = HeroDir + "/Standing 2H Magic Area Attack 01.fbx";
    static readonly string HeavyCastPath = HeroDir + "/Standing 2H Magic Attack 05.fbx";
    static readonly string DeathPath = HeroDir + "/Death From Right.fbx";

    [InitializeOnLoadMethod]
    static void QueueFirstBuild()
    {
        if (Application.isBatchMode ||
            AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null ||
            SessionState.GetBool(AutoBuildSessionKey, false))
            return;

        SessionState.SetBool(AutoBuildSessionKey, true);
        EditorApplication.delayCall += RunFirstBuildWhenReady;
    }

    static void RunFirstBuildWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += RunFirstBuildWhenReady;
            return;
        }

        // Never interrupt Play Mode. Queue the build for the moment the editor
        // returns to Edit Mode so a first import cannot silently skip setup.
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            return;
        }

        Build();
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
            return;

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.delayCall += RunFirstBuildWhenReady;
    }

    [MenuItem("BCE/Heroes/Build Necromancer Class", priority = 24)]
    public static void Build()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Necromancer Setup",
                "Exit Play Mode before building the Necromancer class.", "OK");
            return;
        }

        if (!ValidateInputs(out string error))
        {
            EditorUtility.DisplayDialog("Necromancer Setup", error, "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string returnScene = SceneManager.GetActiveScene().path;

        try
        {
            ConfigurePortrait();
            ConfigureModelImporters();

            AnimatorController controller = BuildAnimatorController();
            ClassAbilityPool pool = BuildClassPool();
            GameObject prefab = BuildPrefab(controller, pool);
            CharacterData data = BuildCharacterData(prefab);

            WireCharacterSelect(data);
            WireNetworkManager(prefab);

            AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Necromancer Ready",
                "Necromancer was built at class index 5.\n\n" +
                "Created and wired:\n" +
                "- Generic rig + animator controller\n" +
                "- Network player prefab\n" +
                "- Spell Forge starter spellbook\n" +
                "- Character-select card and preview\n" +
                "- LoginScene class/spawn registration\n\n" +
                "Open BCE > Spell Forge > Spellbook to replace the starter abilities.",
                "Done");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Necromancer Setup Failed",
                ex.Message + "\n\nSee the Console for the full stack trace.", "OK");
        }
        finally
        {
            if (!string.IsNullOrEmpty(returnScene) && File.Exists(returnScene) &&
                !string.Equals(SceneManager.GetActiveScene().path, returnScene,
                    StringComparison.OrdinalIgnoreCase))
            {
                EditorSceneManager.OpenScene(returnScene, OpenSceneMode.Single);
            }
        }
    }

    static bool ValidateInputs(out string error)
    {
        string[] required =
        {
            SourcePrefabPath, IdlePath, CombatIdlePath, RunPath, JumpPath,
            AttackPath, CastPath, CastAltPath, AreaCastPath, HeavyCastPath,
            DeathPath, MaterialPath, PortraitPath,
            SceneNames.CharacterSelectPath, SceneNames.LoginPath,
        };

        string missing = required.FirstOrDefault(path =>
            AssetDatabase.LoadMainAssetAtPath(path) == null && !File.Exists(path));
        if (missing != null)
        {
            error = "Required asset is missing:\n" + missing;
            return false;
        }

        error = null;
        return true;
    }

    static void ConfigurePortrait()
    {
        var importer = AssetImporter.GetAtPath(PortraitPath) as TextureImporter;
        if (importer == null) return;

        bool dirty = importer.textureType != TextureImporterType.Sprite ||
                     importer.spriteImportMode != SpriteImportMode.Single ||
                     importer.mipmapEnabled;
        if (!dirty) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    static void ConfigureModelImporters()
    {
        ConfigureModel(IdlePath, true);
        ConfigureModel(CombatIdlePath, true);
        ConfigureModel(RunPath, true);
        ConfigureModel(JumpPath, false);
        ConfigureModel(AttackPath, false);
        ConfigureModel(CastPath, false);
        ConfigureModel(CastAltPath, false);
        ConfigureModel(AreaCastPath, false);
        ConfigureModel(HeavyCastPath, false);
        ConfigureModel(DeathPath, false);
    }

    static void ConfigureModel(string path, bool loop)
    {
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) return;

        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.defaultClipAnimations;

        foreach (ModelImporterClipAnimation clip in clips)
        {
            clip.loopTime = loop;
            clip.loopPose = loop;
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    static AnimatorController BuildAnimatorController()
    {
        AnimatorController existing =
            AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (existing != null)
            return existing;

        AnimationClip idle = LoadClip(IdlePath);
        AnimationClip combatIdle = LoadClip(CombatIdlePath) ?? idle;
        AnimationClip run = LoadClip(RunPath);
        AnimationClip jump = LoadClip(JumpPath);
        AnimationClip attack = LoadClip(AttackPath);
        AnimationClip cast = LoadClip(CastPath) ?? attack;
        AnimationClip castAlt = LoadClip(CastAltPath) ?? cast;
        AnimationClip areaCast = LoadClip(AreaCastPath) ?? castAlt;
        AnimationClip heavyCast = LoadClip(HeavyCastPath) ?? areaCast;
        AnimationClip death = LoadClip(DeathPath);

        if (idle == null || run == null || death == null)
            throw new InvalidOperationException(
                "Necromancer requires valid Idle, Running, and Death animation clips.");

        AnimatorController ctrl =
            AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        AddParameter(ctrl, "Speed", AnimatorControllerParameterType.Float);
        AddParameter(ctrl, "IsInCombat", AnimatorControllerParameterType.Bool);
        AddParameter(ctrl, "isMoving", AnimatorControllerParameterType.Bool);
        AddParameter(ctrl, "isBackwards", AnimatorControllerParameterType.Bool);
        AddParameter(ctrl, "isSprinting", AnimatorControllerParameterType.Bool);
        AddParameter(ctrl, "isGrounded", AnimatorControllerParameterType.Bool);
        AddParameter(ctrl, "inWater", AnimatorControllerParameterType.Bool);
        AddParameter(ctrl, "IsDead", AnimatorControllerParameterType.Bool);
        AddParameter(ctrl, "Jump", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "dodge", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "GetHit", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "Death", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "Attack", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "CastDamage", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "CastHeal", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "CastSupport", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "CastTwoHanded", AnimatorControllerParameterType.Trigger);
        AddParameter(ctrl, "Block", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = ctrl.layers[0].stateMachine;
        AnimatorState stIdle = State(sm, "Idle", idle, new Vector3(-300, 0));
        AnimatorState stCombatIdle = State(sm, "IdleCombat", combatIdle, new Vector3(-300, 90));
        AnimatorState stRun = State(sm, "Run", run, new Vector3(0, 0));
        AnimatorState stJump = State(sm, "Jump", jump ?? run, new Vector3(0, 120));
        AnimatorState stAttack = State(sm, "Attack", attack ?? cast, new Vector3(300, -90));
        AnimatorState stCastDamage = State(sm, "CastDamage", cast, new Vector3(300, 0));
        AnimatorState stCastHeal = State(sm, "CastHeal", castAlt, new Vector3(300, 90));
        AnimatorState stCastSupport = State(sm, "CastSupport", areaCast, new Vector3(300, 180));
        AnimatorState stCastHeavy = State(sm, "CastTwoHanded", heavyCast, new Vector3(300, 270));
        AnimatorState stDeath = State(sm, "Death", death, new Vector3(600, 90));
        sm.defaultState = stIdle;

        BoolTransition(stIdle, stCombatIdle, "IsInCombat", true, 0.15f);
        BoolTransition(stCombatIdle, stIdle, "IsInCombat", false, 0.15f);
        FloatTransition(stIdle, stRun, "Speed", 0.08f, AnimatorConditionMode.Greater, 0.12f);
        FloatTransition(stCombatIdle, stRun, "Speed", 0.08f, AnimatorConditionMode.Greater, 0.12f);
        FloatTransition(stRun, stIdle, "Speed", 0.05f, AnimatorConditionMode.Less, 0.18f);

        AnyTrigger(sm, stJump, "Jump");
        AnyTrigger(sm, stAttack, "Attack");
        AnyTrigger(sm, stCastDamage, "CastDamage");
        AnyTrigger(sm, stCastHeal, "CastHeal");
        AnyTrigger(sm, stCastSupport, "CastSupport");
        AnyTrigger(sm, stCastHeavy, "CastTwoHanded");
        AnyTrigger(sm, stDeath, "Death");

        ExitTransition(stJump, stIdle, 0.85f);
        ExitTransition(stAttack, stCombatIdle, 0.85f);
        ExitTransition(stCastDamage, stCombatIdle, 0.85f);
        ExitTransition(stCastHeal, stCombatIdle, 0.85f);
        ExitTransition(stCastSupport, stCombatIdle, 0.85f);
        ExitTransition(stCastHeavy, stCombatIdle, 0.85f);
        BoolTransition(stDeath, stIdle, "IsDead", false, 0.15f);

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        return ctrl;
    }

    static ClassAbilityPool BuildClassPool()
    {
        EnsureFolder("Assets/Game/Data/ClassPools");
        ClassAbilityPool pool = AssetDatabase.LoadAssetAtPath<ClassAbilityPool>(PoolPath);
        if (pool == null)
        {
            pool = ScriptableObject.CreateInstance<ClassAbilityPool>();
            AssetDatabase.CreateAsset(pool, PoolPath);
        }

        pool.className = "Necromancer";
        pool.availableIndices = new[] { 0, 1, 2, 3 };
        pool.defaultEquipped = new[] { 0, 1, 2, 3 };
        EditorUtility.SetDirty(pool);
        return pool;
    }

    static GameObject BuildPrefab(AnimatorController controller, ClassAbilityPool pool)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existing != null)
            return RelinkExistingPrefab(controller, pool);

        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(IdlePath);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (source == null || modelAsset == null)
            throw new InvalidOperationException("Could not load the source prefab or Necromancer model.");

        GameObject root = UnityEngine.Object.Instantiate(source);
        try
        {
            root.name = "Necromancer";
            if (PrefabUtility.IsPartOfPrefabInstance(root))
                PrefabUtility.UnpackPrefabInstance(root,
                    PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            root.tag = "Player";
            PlayerIdentity identity = root.GetComponent<PlayerIdentity>() ?? root.AddComponent<PlayerIdentity>();
            identity.classIndex = ClassIndex;

            RemoveComponent<ClericAnimationDriver>(root);
            RemoveComponent<ClericHealVFX>(root);

            AbilityCaster oldCaster = root.GetComponent<AbilityCaster>();
            string casterJson = oldCaster != null ? EditorJsonUtility.ToJson(oldCaster) : null;
            if (oldCaster != null)
                UnityEngine.Object.DestroyImmediate(oldCaster);

            NecromancerAbilityCaster caster = root.AddComponent<NecromancerAbilityCaster>();
            if (!string.IsNullOrEmpty(casterJson))
                EditorJsonUtility.FromJsonOverwrite(casterJson, caster);

            caster.classPool = pool;
            caster.castAnimator = root.GetComponent<CastAnimator>() ?? root.AddComponent<CastAnimator>();
            caster.spellbook = CreateStarterSpellbook();
            caster.equippedIndices = new[] { 0, 1, 2, 3 };
            caster.kineticReversalHandler = null;
            caster.siegeModeHandler = null;
            caster.dashHandler = null;
            caster.stealthHandler = null;
            caster.transferProtocolHandler = null;
            caster.ironTetherHandler = null;
            caster.healVFX = null;
            caster.beaconPrefab = null;
            caster.phaseRelayPrefab = null;
            caster.shadowRelayPrefab = null;
            caster.shockMinePrefab = null;
            caster.naniteSwarmPrefab = null;
            caster.singularityPrefab = null;
            caster.eventHorizonPrefab = null;
            caster.lastBastionPrefab = null;
            caster.nullFieldPrefab = null;

            Transform oldModel = root.transform.Find("Model");
            if (oldModel != null)
                UnityEngine.Object.DestroyImmediate(oldModel.gameObject);

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, root.transform);
            if (model == null)
                model = UnityEngine.Object.Instantiate(modelAsset, root.transform);
            model.name = "Model";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            FitToHeight(model, 1.8f);

            if (material != null)
            {
                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                        materials[i] = material;
                    renderer.sharedMaterials = materials;
                }
            }

            Animator animator = model.GetComponentInChildren<Animator>(true) ?? model.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.avatar = LoadAvatar(IdlePath);
            animator.applyRootMotion = false;

            if (root.GetComponent<PlayerAnimator>() == null)
                root.AddComponent<PlayerAnimator>();

            NetworkAnimator networkAnimator = root.GetComponent<NetworkAnimator>() ?? root.AddComponent<NetworkAnimator>();
            networkAnimator.animator = animator;
            networkAnimator.clientAuthority = true;

            Health health = root.GetComponent<Health>() ?? root.AddComponent<Health>();
            health.isPlayer = true;

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }

        AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
    }

    static GameObject RelinkExistingPrefab(
        AnimatorController controller, ClassAbilityPool pool)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            root.name = "Necromancer";
            root.tag = "Player";

            PlayerIdentity identity = root.GetComponent<PlayerIdentity>() ??
                                      root.AddComponent<PlayerIdentity>();
            identity.classIndex = ClassIndex;

            AbilityCaster currentCaster =
                root.GetComponentInChildren<AbilityCaster>(true);
            NecromancerAbilityCaster caster =
                currentCaster as NecromancerAbilityCaster;
            if (caster == null)
            {
                string casterJson = currentCaster != null
                    ? EditorJsonUtility.ToJson(currentCaster)
                    : null;
                if (currentCaster != null)
                    UnityEngine.Object.DestroyImmediate(currentCaster);

                caster = root.AddComponent<NecromancerAbilityCaster>();
                if (!string.IsNullOrEmpty(casterJson))
                    EditorJsonUtility.FromJsonOverwrite(casterJson, caster);
            }

            caster.classPool = pool;
            caster.castAnimator = root.GetComponent<CastAnimator>() ??
                                  root.AddComponent<CastAnimator>();
            if (caster.spellbook == null || caster.spellbook.Length == 0)
                caster.spellbook = CreateStarterSpellbook();
            if (caster.equippedIndices == null || caster.equippedIndices.Length == 0)
                caster.equippedIndices = new[] { 0, 1, 2, 3 };

            Animator animator = root.GetComponentInChildren<Animator>(true);
            if (animator == null)
                throw new InvalidOperationException(
                    "The replacement Necromancer prefab has no Animator in its model hierarchy.");
            animator.runtimeAnimatorController = controller;
            animator.avatar = LoadAvatar(IdlePath);
            animator.applyRootMotion = false;

            if (root.GetComponent<PlayerAnimator>() == null)
                root.AddComponent<PlayerAnimator>();

            NetworkAnimator networkAnimator =
                root.GetComponent<NetworkAnimator>() ?? root.AddComponent<NetworkAnimator>();
            networkAnimator.animator = animator;
            networkAnimator.clientAuthority = true;

            Health health = root.GetComponent<Health>() ?? root.AddComponent<Health>();
            health.isPlayer = true;

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
    }

    static AbilityDef[] CreateStarterSpellbook()
    {
        AnimationClip quick = LoadClip(AttackPath);
        AnimationClip cast = LoadClip(CastPath);
        AnimationClip area = LoadClip(AreaCastPath);
        AnimationClip ward = LoadClip(HeavyCastPath);

        return new[]
        {
            new AbilityDef
            {
                abilityName = "Necrotic Bolt",
                description = "Starter targeted death-magic strike.",
                shape = AbilityShape.Circle,
                category = AbilityCategory.Damage,
                range = 12f,
                indicatorSize = 1.5f,
                cooldown = 1.5f,
                castTime = 0.45f,
                damage = 20f,
                targetTag = "Enemy",
                icon = LoadIcon("void-bolt"),
                marauderCastAnimation = quick,
                variants = Array.Empty<AbilityVariant>(),
            },
            new AbilityDef
            {
                abilityName = "Bone Spear",
                description = "Starter piercing line attack.",
                shape = AbilityShape.Rectangle,
                category = AbilityCategory.Damage,
                range = 12f,
                rectWidth = 1.6f,
                cooldown = 5f,
                castTime = 0.65f,
                damage = 32f,
                targetTag = "Enemy",
                icon = LoadIcon("dark-harvest"),
                marauderCastAnimation = cast,
                variants = Array.Empty<AbilityVariant>(),
            },
            new AbilityDef
            {
                abilityName = "Grave Bloom",
                description = "Starter area-damage spell.",
                shape = AbilityShape.Circle,
                category = AbilityCategory.Damage,
                range = 10f,
                indicatorSize = 5f,
                cooldown = 8f,
                castTime = 0.8f,
                damage = 30f,
                targetTag = "Enemy",
                icon = LoadIcon("collapsing-void"),
                marauderCastAnimation = area,
                variants = Array.Empty<AbilityVariant>(),
            },
            new AbilityDef
            {
                abilityName = "Soul Ward",
                description = "Starter allied shielding circle.",
                shape = AbilityShape.Circle,
                category = AbilityCategory.Support,
                range = 8f,
                indicatorSize = 5f,
                cooldown = 10f,
                castTime = 0.7f,
                shieldAbsorb = 40f,
                shieldDuration = 6f,
                targetTag = "Player",
                icon = LoadIcon("arcane-ward"),
                marauderCastAnimation = ward,
                variants = Array.Empty<AbilityVariant>(),
            },
        };
    }

    static CharacterData BuildCharacterData(GameObject prefab)
    {
        EnsureFolder("Assets/Game/Data/CharacterSelect");
        CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(CharacterDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<CharacterData>();
            AssetDatabase.CreateAsset(data, CharacterDataPath);
        }

        data.name = "Necromancer";
        data.className = "Necromancer";
        data.roleTagline = "Summoner - Attrition - Battlefield Control";
        data.loreDescription =
            "A grave-weaver who turns the boundary between life and death into a weapon. " +
            "The Necromancer controls space with curses, bone magic, and persistent soul wards.";
        data.classColor = new Color(0.45f, 0.75f, 0.20f);
        data.classColorDark = new Color(0.06f, 0.12f, 0.08f);
        data.portrait = AssetDatabase.LoadAssetAtPath<Sprite>(PortraitPath);
        data.prefab = prefab;
        data.previewPrefab = prefab;
        data.traits = new[]
        {
            new TraitPill { label = "Grave Weaver" },
            new TraitPill { label = "Soul Magic" },
            new TraitPill { label = "Area Control" },
            new TraitPill { label = "Attrition" },
        };
        data.stats = new[]
        {
            new ClassStat { label = "Damage", value = 4 },
            new ClassStat { label = "Control", value = 5 },
            new ClassStat { label = "Mobility", value = 2 },
            new ClassStat { label = "Survivability", value = 3 },
            new ClassStat { label = "Utility", value = 4 },
        };
        data.coreAbilities = new[]
        {
            new AbilityPreview { abilityName = "Necrotic Bolt", description = "Focused death magic at a selected enemy area." },
            new AbilityPreview { abilityName = "Bone Spear", description = "A piercing line of bone through enemies." },
            new AbilityPreview { abilityName = "Grave Bloom", description = "Wide corrupted-ground detonation." },
        };
        data.deployableName = "Soul Ward";
        data.deployableDescription = "A protective circle that shields allies inside it.";
        data.deployableIcon = null;

        EditorUtility.SetDirty(data);
        return data;
    }

    static void WireCharacterSelect(CharacterData data)
    {
        Scene scene = EditorSceneManager.OpenScene(SceneNames.CharacterSelectPath, OpenSceneMode.Single);
        CharacterSelectUI ui = UnityEngine.Object.FindFirstObjectByType<CharacterSelectUI>();
        if (ui == null)
            throw new InvalidOperationException("CharacterSelectUI was not found in CharacterSelect.unity.");

        CharacterData[] roster = new CharacterData[ClassIndex + 1];
        if (ui.characters != null)
            Array.Copy(ui.characters, roster, Math.Min(ui.characters.Length, roster.Length));
        roster[ClassIndex] = data;
        ui.characters = roster;

        float[] heights = new float[ClassIndex + 1];
        for (int i = 0; i < heights.Length; i++) heights[i] = 1f;
        if (ui.sceneClassHeightMultipliers != null)
            Array.Copy(ui.sceneClassHeightMultipliers, heights,
                Math.Min(ui.sceneClassHeightMultipliers.Length, heights.Length));
        heights[ClassIndex] = 1f;
        ui.sceneClassHeightMultipliers = heights;

        EditorUtility.SetDirty(ui);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void WireNetworkManager(GameObject prefab)
    {
        Scene scene = EditorSceneManager.OpenScene(SceneNames.LoginPath, OpenSceneMode.Single);
        RodNetworkManager manager = UnityEngine.Object.FindFirstObjectByType<RodNetworkManager>();
        if (manager == null)
            throw new InvalidOperationException("RodNetworkManager was not found in LoginScene.unity.");

        GameObject[] classes = new GameObject[ClassIndex + 1];
        if (manager.classPrefabs != null)
            Array.Copy(manager.classPrefabs, classes,
                Math.Min(manager.classPrefabs.Length, classes.Length));
        classes[ClassIndex] = prefab;
        manager.classPrefabs = classes;

        if (!manager.spawnPrefabs.Contains(prefab))
            manager.spawnPrefabs.Add(prefab);

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static AnimationClip LoadClip(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => !clip.name.StartsWith("__preview__"));

    static Avatar LoadAvatar(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Avatar>()
            .FirstOrDefault(avatar => avatar.isValid);

    static Sprite LoadIcon(string fileName) =>
        AssetDatabase.LoadAssetAtPath<Sprite>(
            $"Assets/Game/UI/CharacterSelect/AbilityIcons/{fileName}.png");

    static void FitToHeight(GameObject model, float targetHeight)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y < 0.0001f)
            return;

        float scale = targetHeight / bounds.size.y;
        model.transform.localScale *= scale;

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        model.transform.localPosition +=
            new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
    }

    static AnimatorState State(AnimatorStateMachine sm, string name,
        AnimationClip clip, Vector3 position)
    {
        AnimatorState state = sm.AddState(name, position);
        state.motion = clip;
        state.writeDefaultValues = true;
        return state;
    }

    static void AddParameter(AnimatorController ctrl, string name,
        AnimatorControllerParameterType type)
    {
        if (ctrl.parameters.Any(parameter => parameter.name == name)) return;
        ctrl.AddParameter(name, type);
    }

    static void AnyTrigger(AnimatorStateMachine sm, AnimatorState state, string trigger)
    {
        AnimatorStateTransition transition = sm.AddAnyStateTransition(state);
        transition.hasExitTime = false;
        transition.duration = 0.05f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(AnimatorConditionMode.If, 0f, trigger);
    }

    static void BoolTransition(AnimatorState from, AnimatorState to,
        string parameter, bool value, float duration)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = duration;
        transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
            0f, parameter);
    }

    static void FloatTransition(AnimatorState from, AnimatorState to,
        string parameter, float threshold, AnimatorConditionMode mode, float duration)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = duration;
        transition.AddCondition(mode, threshold, parameter);
    }

    static void ExitTransition(AnimatorState from, AnimatorState to, float exitTime)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = exitTime;
        transition.duration = 0.12f;
    }

    static void RemoveComponent<T>(GameObject root) where T : Component
    {
        T component = root.GetComponent<T>();
        if (component != null)
            UnityEngine.Object.DestroyImmediate(component);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
