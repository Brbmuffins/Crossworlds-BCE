using UnityEngine;
using UnityEditor;

/// <summary>
/// BCE/Setup/CharacterSelect ▶ Build Class Data
///
/// Creates or replaces the five CharacterData ScriptableObjects used by
/// CharacterSelectUI. Run once after first import; re-run to refresh stats/lore.
///
/// Sprites are loaded from:
///   Assets/Game/UI/CharacterSelect/Portraits/class-{name}.png
///   Assets/Game/UI/CharacterSelect/AbilityIcons/{ability}.png
///
/// After running, drag the 5 assets into CharacterSelectUI.characters
/// in the CharacterSelect scene Inspector (index order 0–4 matches classPrefabs).
/// </summary>
public static class CharacterSelectDataBuilder
{
    const string OutDir     = "Assets/Game/Data/CharacterSelect";
    const string PortraitDir = "Assets/Game/UI/CharacterSelect/Portraits";
    const string IconDir     = "Assets/Game/UI/CharacterSelect/AbilityIcons";

    [MenuItem("BCE/Setup/CharacterSelect - Fix Sprite Import Settings")]
    public static void FixImportSettings()
    {
        // Set all PNGs in Portraits/ and AbilityIcons/ to Sprite (UI) texture type.
        // Run this ONCE after first import, before running Build Class Data.
        int count = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PortraitDir, IconDir }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType      = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                count++;
            }
        }
        Debug.Log($"[BCE] Fixed import settings on {count} textures. Now run Build Class Data.");
    }

    [MenuItem("BCE/Setup/CharacterSelect - Build Class Data")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(OutDir))
        {
            AssetDatabase.CreateFolder("Assets/Game/Data", "CharacterSelect");
        }

        CreateMarauder();
        CreateIronclad();
        CreateShadowblade();
        CreateCleric();
        CreateArcanist();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[BCE] 5 CharacterData assets created in " + OutDir +
                  "\nNEXT: drag them into CharacterSelectUI.characters[0-4] in the Inspector.");
    }

    // ── 0 — Marauder ──────────────────────────────────────────────────────────

    static void CreateMarauder()
    {
        var d = Make("Marauder");
        d.className       = "Marauder";
        d.roleTagline     = "Control DPS  ·  Turret Tactician  ·  Area Denial";
        d.loreDescription =
            "A master of runic constructs, the Marauder bends the battlefield through automated sentinels and snare fields. " +
            "Where others fight with muscle and steel, the Marauder fights with precision and architecture — turning every engagement into a controlled kill zone.";
        d.classColor     = new Color(0.88f, 0.58f, 0.18f);
        d.classColorDark = new Color(0.30f, 0.18f, 0.05f);
        d.portrait       = LoadPortrait("marauder");

        d.traits = new TraitPill[]
        {
            new TraitPill { label = "Turret Tactician" },
            new TraitPill { label = "Rune Forger"      },
            new TraitPill { label = "Area Denial"      },
            new TraitPill { label = "Field Commander"  },
        };

        d.stats = new ClassStat[]
        {
            new ClassStat { label = "Damage",        value = 3 },
            new ClassStat { label = "Control",        value = 5 },
            new ClassStat { label = "Mobility",       value = 2 },
            new ClassStat { label = "Survivability",  value = 3 },
            new ClassStat { label = "Utility",        value = 4 },
        };

        d.coreAbilities = new AbilityPreview[]
        {
            new AbilityPreview
            {
                icon        = LoadIcon("runic-sentinel"),
                abilityName = "Runic Sentinel",
                description = "Deploy an automated turret that fires at nearby enemies until destroyed.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("runic-snare"),
                abilityName = "Runic Snare",
                description = "Lay a rune trap that roots the first enemy to cross it.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("rune-chain"),
                abilityName = "Rune Chain",
                description = "Link two runes — enemies passing the beam take damage and are slowed.",
            },
        };

        d.deployableName        = "Runic Sentinel";
        d.deployableDescription = "Places an automated turret. Marauder's signature — persists between waves and scales with tech upgrades.";
        d.deployableIcon        = LoadIcon("runic-sentinel");

        Save(d, "Marauder");
    }

    // ── 1 — Ironclad ─────────────────────────────────────────────────────────

    static void CreateIronclad()
    {
        var d = Make("Templar");
        d.className       = "Templar";
        d.roleTagline     = "Tank  ·  Vanguard  ·  Frontline Fortress";
        d.loreDescription =
            "An indestructible wall of armored might, the Templar absorbs punishment so their allies don't have to. " +
            "Masters of aggro and mitigation, they hold the line against overwhelming odds — then punish overcommitment with a devastating counter-charge.";
        d.classColor     = new Color(0.42f, 0.68f, 0.88f);
        d.classColorDark = new Color(0.08f, 0.18f, 0.30f);
        d.portrait       = LoadPortrait("ironclad");

        d.traits = new TraitPill[]
        {
            new TraitPill { label = "Immovable"    },
            new TraitPill { label = "Fortress"     },
            new TraitPill { label = "Battle Crier" },
            new TraitPill { label = "Counter Punch"},
        };

        d.stats = new ClassStat[]
        {
            new ClassStat { label = "Damage",        value = 2 },
            new ClassStat { label = "Control",        value = 3 },
            new ClassStat { label = "Mobility",       value = 2 },
            new ClassStat { label = "Survivability",  value = 5 },
            new ClassStat { label = "Utility",        value = 3 },
        };

        d.coreAbilities = new AbilityPreview[]
        {
            new AbilityPreview
            {
                icon        = LoadIcon("shieldwall-charge"),
                abilityName = "Shieldwall Charge",
                description = "Burst forward, knocking back the first enemy hit and briefly stunning them.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("stalwart-stance"),
                abilityName = "Stalwart Stance",
                description = "Enter a defensive stance — take dramatically reduced damage but cannot move.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("iron-rampart"),
                abilityName = "Iron Rampart",
                description = "Deploy a directional barrier that blocks projectiles and enemy movement.",
            },
        };

        d.deployableName        = "Iron Rampart";
        d.deployableDescription = "Deploys a destructible barrier the entire team can hide behind. Absorbs a fixed amount of damage before shattering.";
        d.deployableIcon        = LoadIcon("iron-rampart");

        Save(d, "Templar");
    }

    // ── 2 — Shadowblade ──────────────────────────────────────────────────────

    static void CreateShadowblade()
    {
        var d = Make("Night Hunter");
        d.className       = "Night Hunter";
        d.roleTagline     = "Burst DPS  ·  Assassin  ·  Flanker";
        d.loreDescription =
            "A phantom of the battlefield, the Night Hunter strikes from darkness and vanishes before retaliation. " +
            "High risk, devastating reward — masters of void-stepping and positional play who reward spatial awareness with lethal efficiency.";
        d.classColor     = new Color(0.72f, 0.22f, 0.90f);
        d.classColorDark = new Color(0.18f, 0.05f, 0.28f);
        d.portrait       = LoadPortrait("shadowblade");

        d.traits = new TraitPill[]
        {
            new TraitPill { label = "Assassin"    },
            new TraitPill { label = "Void Walker" },
            new TraitPill { label = "Elusive"     },
            new TraitPill { label = "Flank King"  },
        };

        d.stats = new ClassStat[]
        {
            new ClassStat { label = "Damage",        value = 5 },
            new ClassStat { label = "Control",        value = 2 },
            new ClassStat { label = "Mobility",       value = 5 },
            new ClassStat { label = "Survivability",  value = 2 },
            new ClassStat { label = "Utility",        value = 2 },
        };

        d.coreAbilities = new AbilityPreview[]
        {
            new AbilityPreview
            {
                icon        = LoadIcon("shadow-veil"),
                abilityName = "Shadow Veil",
                description = "Vanish from sight. Next attack deals bonus damage and restores a dodge charge.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("dark-harvest"),
                abilityName = "Dark Harvest",
                description = "Drain life from a target — deal damage and restore health proportional to the hit.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("arcane-step"),
                abilityName = "Arcane Step",
                description = "Instantly blink to a target location, ignoring terrain.",
            },
        };

        d.deployableName        = "Shadow Veil";
        d.deployableDescription = "Projects a veil over a wide area, briefly cloaking all allies inside from enemy detection.";
        d.deployableIcon        = LoadIcon("shadow-veil");

        Save(d, "Night Hunter");
    }

    // ── 3 — Cleric ───────────────────────────────────────────────────────────

    static void CreateCleric()
    {
        var d = Make("Cleric");
        d.className       = "Cleric";
        d.roleTagline     = "Healer  ·  Support  ·  Divine Sustain";
        d.loreDescription =
            "A divine conduit who mends wounds and fortifies the party's resolve. " +
            "The Cleric keeps the team alive through relentless triage and sacred aegis — and when cornered, " +
            "their spiritual redirection turns enemy aggression into healing for the whole party.";
        d.classColor     = new Color(0.95f, 0.85f, 0.25f);
        d.classColorDark = new Color(0.28f, 0.22f, 0.05f);
        d.portrait       = LoadPortrait("cleric");

        d.traits = new TraitPill[]
        {
            new TraitPill { label = "Life Binder"   },
            new TraitPill { label = "Sanctifier"    },
            new TraitPill { label = "Spirit Guide"  },
            new TraitPill { label = "Chain Healer"  },
        };

        d.stats = new ClassStat[]
        {
            new ClassStat { label = "Damage",        value = 2 },
            new ClassStat { label = "Control",        value = 2 },
            new ClassStat { label = "Mobility",       value = 3 },
            new ClassStat { label = "Survivability",  value = 4 },
            new ClassStat { label = "Utility",        value = 5 },
        };

        d.coreAbilities = new AbilityPreview[]
        {
            new AbilityPreview
            {
                icon        = LoadIcon("mending-circle"),
                abilityName = "Mending Circle",
                description = "Place a healing zone — all allies inside recover health over time.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("mend"),
                abilityName = "Mend",
                description = "Instantly restore a moderate amount of health to a nearby ally.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("soul-bond"),
                abilityName = "Soul Bond",
                description = "Link to an ally — share a portion of incoming damage and outgoing heals.",
            },
        };

        d.deployableName        = "Mending Circle";
        d.deployableDescription = "Drops a persistent healing zone. All allies in range recover health each second for its duration.";
        d.deployableIcon        = LoadIcon("mending-circle");

        Save(d, "Cleric");
    }

    // ── 4 — Arcanist ─────────────────────────────────────────────────────────

    static void CreateArcanist()
    {
        var d = Make("Arcanist");
        d.className       = "Arcanist";
        d.roleTagline     = "Burst DPS  ·  Void Mage  ·  Singularity";
        d.loreDescription =
            "A wielder of unstable arcane forces who tears space itself with gravitational magic. " +
            "The Arcanist deals the highest single-target burst in the roster — at the cost of paper-thin defences " +
            "and cooldowns that demand precise timing. Every spell reshapes the battlefield.";
        d.classColor     = new Color(0.38f, 0.22f, 0.98f);
        d.classColorDark = new Color(0.10f, 0.05f, 0.28f);
        d.portrait       = LoadPortrait("arcanist");
        d.prefab         = LoadModel("Assets/Game/Prefabs/Arcanist.prefab");
        d.previewPrefab  = d.prefab;

        d.traits = new TraitPill[]
        {
            new TraitPill { label = "Void Bender"  },
            new TraitPill { label = "Arcane Surge" },
            new TraitPill { label = "Singularity"  },
            new TraitPill { label = "Glass Cannon" },
        };

        d.stats = new ClassStat[]
        {
            new ClassStat { label = "Damage",        value = 5 },
            new ClassStat { label = "Control",        value = 4 },
            new ClassStat { label = "Mobility",       value = 3 },
            new ClassStat { label = "Survivability",  value = 1 },
            new ClassStat { label = "Utility",        value = 3 },
        };

        d.coreAbilities = new AbilityPreview[]
        {
            new AbilityPreview
            {
                icon        = LoadIcon("void-bolt"),
                abilityName = "Void Bolt",
                description = "Fire a concentrated void projectile that pierces one enemy for heavy damage.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("collapsing-void"),
                abilityName = "Collapsing Void",
                description = "Summon a singularity that pulls nearby enemies together before detonating.",
            },
            new AbilityPreview
            {
                icon        = LoadIcon("forked-lightning"),
                abilityName = "Forked Lightning",
                description = "Unleash arcs of arcane lightning that chain between up to 4 targets.",
            },
        };

        d.deployableName        = "Conjurer's Surge";
        d.deployableDescription = "Summons an arcane familiar that auto-fires void bolts at enemies for a limited duration.";
        d.deployableIcon        = LoadIcon("conjurers-surge");

        Save(d, "Arcanist");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static CharacterData Make(string name)
    {
        var d = ScriptableObject.CreateInstance<CharacterData>();
        d.name = name;
        return d;
    }

    static void Save(CharacterData d, string name)
    {
        string path = $"{OutDir}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(d, existing);
            EditorUtility.SetDirty(existing);
        }
        else
        {
            AssetDatabase.CreateAsset(d, path);
        }
        Debug.Log($"[BCE] Saved {path}");
    }

    static Sprite LoadPortrait(string className)
    {
        string path = $"{PortraitDir}/class-{className}.png";
        var tex = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (tex == null)
            Debug.LogWarning($"[BCE] Portrait not found: {path}");
        return tex;
    }

    static Sprite LoadIcon(string iconName)
    {
        string path = $"{IconDir}/{iconName}.png";
        var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (spr == null)
            Debug.LogWarning($"[BCE] Icon not found: {path}");
        return spr;
    }

    static GameObject LoadModel(string path)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (model == null)
            Debug.LogWarning($"[BCE] Model not found: {path}");
        return model;
    }
}
