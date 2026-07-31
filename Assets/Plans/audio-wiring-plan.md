# Project Overview
- **Game Title**: Crossworlds
- **High-Level Concept**: A cooperative multiplayer action RPG where players select character classes (Arcanist, Cleric, Ironclad, Marauder, Shadowblade) to battle waves of enemies and epic bosses in atmospheric, instanced arenas.
- **Players**: Single player, online co-op multiplayer (powered by Mirror)
- **Inspiration / Reference Games**: World of Warcraft, Diablo, Torchlight
- **Tone / Art Direction**: Stylized dark fantasy, contrasting thick volumetric fogs and atmospheric lighting with colorful, high-impact spell effects.
- **Target Platform**: PC (StandaloneWindows64)
- **Screen Orientation / Resolution**: Landscape 1920x1080
- **Render Pipeline**: Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
Players spawn into character hubs, equip customized loadouts, select a combat class, and enter arena instances (e.g., Arena_Copper, VoidDungeon) to defeat waves of enemies, collect gold and item loot drops, gain experience, and eventually defeat high-threat world bosses.

## Controls and Input Methods
- **Locomotion**: WASD or Point-and-Click movement via Unity's Input System.
- **Combat**: Action-bar abilities triggered by keys (1-4) or mouse clicks, using real-time spatial casting templates (circles, cones, rectangles) with cursor-based aiming.

# UI
- **HUD Action Bar**: Displays abilities, cooldowns, and active player resources (mana/health).
- **Damage Texts**: Floating floating-combat-text popups tracking hits, heals, and damage values.
- **Combat Sound Feedback**: Spatialized positional SFX triggered by cast executions and target impacts.

# Key Asset & Context
- **Class Prefabs**:
  - `Assets/Game/Game_Prefabs/Arcanist.prefab`
  - `Assets/Game/Game_Prefabs/Cleric.prefab`
  - `Assets/Game/Game_Prefabs/Ironclad.prefab`
  - `Assets/Game/Game_Prefabs/Marauder.prefab`
  - `Assets/Game/Game_Prefabs/Shadowblade.prefab`
- **Enemy Prefabs**:
  - `Assets/Game/3D Models/Enemies/Ogres/O'gar Brute/Prefab_OgreBrute.prefab` (and secondary enemy templates)
- **Audio Assets**:
  - Spell FX Pack: `Assets/Magic Spell Sound Effects Pack Vol 1/` (containing Frost, Fire, Lightning, Dark, Nature, Arcane, etc.)
  - Orc Voice Pack: `Assets/Orc/` (containing `Growl_Hard` and `Vocalfry` variants for Aggro, Attack, GetHit, and Death events)

# Implementation Steps

## Step 1: Discover and Map Audio Clip Assets
- **Description**: Scan the audio pack folder structure programmatically to build a dictionary of clips matching spell categories and Orc voice configurations.
- **Assigned role**: explorer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 2: Write automatic Audio Matcher Script
- **Description**: Create an editor-only utility script `Assets/Game/Editor/AudioWiringUtility.cs` which:
  1. Opens the character class prefabs (`Arcanist`, `Cleric`, `Ironclad`, `Marauder`, `Shadowblade`) and scans their `AbilityCaster.spellbook` array.
  2. Uses keyword matching to assign `castSFX` and `hitSFX` to each `AbilityDef` based on its name and properties:
     - **Frost/Ice**: `Assets/Magic Spell Sound Effects Pack Vol 1/Frost/` (e.g., "Frost Magic Spell 2-1")
     - **Fire/Ember/Burn**: `Assets/Magic Spell Sound Effects Pack Vol 1/Fire/` (e.g., "Fire Magic Spell 5-1")
     - **Lightning/Storm/Shock**: `Assets/Magic Spell Sound Effects Pack Vol 1/Lightning/` (e.g., "Lightning Magic Spell 1")
     - **Dark/Void/Shadow**: `Assets/Magic Spell Sound Effects Pack Vol 1/Dark/` (e.g., "Dark Magic Spell 2")
     - **Nature/Thorn/Vine**: `Assets/Magic Spell Sound Effects Pack Vol 1/Nature/` (e.g., "Nature Magic Spell 1")
     - **Heal/Mend**: `Assets/Magic Spell Sound Effects Pack Vol 1/General Spells and Effects/` (e.g., "Heal Swell" or positive magic swell)
     - **Arcane/Other**: `Assets/Magic Spell Sound Effects Pack Vol 1/Arcane/`
  3. Locates enemy prefabs (especially Ogre/Orc/Cyclops models) and:
     - Adds the `EnemySfxProfile` component if it is missing.
     - Automatically maps the Orc voice clips using a blend of "Growl Hard" (highly intense for Attack, Aggro, Death) and "Vocalfry" (raspy, atmospheric for GetHit, breathing, or auxiliary attacks) to satisfy the "little of both" requirement:
       - `aggro`: `Orc_Growl1_Growl_Hard.wav`
       - `attack1` (heavy): `Orc_Attack1_Growl_Hard.wav`
       - `attack2` (fry blend): `Orc_Attack2_Vocalfry.wav`
       - `attack3`: `Orc_Attack3_Growl_Hard.wav`
       - `attackImpact`: `Orc_Grunt1_Growl_Hard.wav`
       - `getHit`: `Orc_Hurt1_Vocalfry.wav` or `Orc_Hurt2_Vocalfry.wav`
       - `death`: `Orc_Death1_Growl_Hard.wav` or `Orc_Death2_Growl_Hard.wav`
  4. Saves changes to the Prefabs and pushes them into the scene setups.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## Step 3: Execute Utility to Wire Assets
- **Description**: Trigger the custom editor tool `AudioWiringUtility` to apply all assignments in the Unity Editor database.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## Step 4: Validate Prefab Serialized Changes
- **Description**: Programmatically query and inspect a random subset of spellbooks and enemy profiles to ensure no `null` properties remain on active spells and combat units.
- **Assigned role**: explorer
- **Dependencies**: Step 3
- **Parallelizable**: Yes

# Verification & Testing
1. **Spell Casting Test**: Open the hub scene and cast various abilities as Arcanist or Cleric. Ensure that different elements (Ice Vortex vs Ember Surge vs Void Bolt) emit their distinct element-mapped `castSFX` and trigger `hitSFX` when targets are impacted.
2. **Combat Audio Test**: Spawn an Ogre Brute or Orc Grunt. Approach to trigger aggro (verify Growl Hard SFX), strike them to verify raspy Vocalfry `getHit` SFX, and kill them to verify intense death growls.
3. **Console Log Validation**: Verify there are no missing audio clip warnings or NullReferenceExceptions thrown by the `AbilityCaster` or `EnemyController` runtime loops.
