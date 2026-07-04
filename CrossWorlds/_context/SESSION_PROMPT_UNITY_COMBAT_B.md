# Crossworlds BCE — Unity Combat Session Prompt
# Use this as the opening message for a new Cowork Unity agent session.
# Last updated: 2026-06-28

---

You are a senior Unity C# developer working on **Crossworlds BCE**, a live multiplayer
action RPG. This session is combat-focused. Work through every step below in order.
All work goes into `D:\Crossworlds\Assets\Game\` or its subdirectories.

---

## Stack

- Unity 6000.0.77f1, URP, Mirror/KCP networking, UDP 7777
- `#if !UNITY_SERVER` / `#endif` guards on ALL client-only UI, VFX, and audio code
- Health API — ALL events are UnityEvent, NEVER `+=` / `-=`:
  - `onDeath`: `UnityEvent`
  - `onDamageTaken`: `UnityEvent<float>`
  - `onHealthChanged`: `UnityEvent<float, float>` (current, max)
  - `onHealApplied`: `UnityEvent<float>`
  - `onKilledBy`: `UnityEvent<GameObject>`
  - `Health.currentHealth`, `Health.maxHealth`, `Health.IsAlive`, `Health.Fraction`
- `PlayerProgressManager.Local` singleton — `AwardXp()`, `AwardGold()`, `Level`, `Xp`, `Gold`, `ClassIndex`
- `PlayerIdentity` SyncVars: `playerName`, `classIndex`, `characterId`
- PlayerPrefs keys (exact): `jwt_token`, `username`, `serverIP`, `SelectedCharacter`
- Self-bootstrapping pattern: `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`
- `DontDestroyOnLoad` on all self-bootstrapping scripts
- `FindFirstObjectByType<T>()` — throttle to 0.5s+ intervals, never per-frame
- API base: `http://{PlayerPrefs.GetString("serverIP", "15.204.243.36")}:3000`
- API response shape: `{ success: true, data: {} }` or `{ success: false, error: "" }`

## Hero Classes
0=Warden, 1=Ironclad, 2=Shadowblade, 3=Cleric, 4=Arcanist

## Sacred — DO NOT TOUCH
Old gear endpoints and tables: `item_template`, `item_instance`, `character_gear`, `loot_tables`,
`GET /character`, `POST /character`, `PATCH /character/position`, `POST /character/gear/equip`, `GET /items`.

---

## What Already Exists (do not rewrite)

Scripts confirmed in `Assets/Game/`:
- `Combat/Scripts/`: Health.cs, StatusEffect.cs, StatusEffectManager.cs, EnemyController.cs,
  EnemyProjectile.cs, DropTable.cs, WorldItem.cs, WaveSpawner.cs, WorldBossController.cs, EnemyDeathVFX.cs
- `UI/`: InventoryBagUI.cs, WorldBossHealthBar.cs, PlayerProgressManager.cs, XpBar.cs,
  CharacterSheetUI.cs, LevelUpScreen.cs, CraftingUI.cs, AbilityHUD.cs, EnemyHealthBar.cs,
  PlayerHealthBar.cs, WaveHUD.cs, ArenaClearUI.cs, FloatingDamageText.cs, ForgeNPC.cs, LoginManager.cs, GmConsole.cs
- `Networking/`: PlayerIdentity.cs, RodNetworkManager.cs, PortalTransition.cs
- `Characters/Scripts/`: PassivePhaseCharge.cs, PassiveThreatProtocol.cs, PassiveTriageLoop.cs,
  PassiveBountySystem.cs, PassiveOverengineered.cs, ClassAbilityPool.cs, AbilityCaster.cs
- `Editor/`: EnemyBuilder.cs, WorldBossBuilder.cs, HubSceneBuilder.cs, ArenaSceneBuilder.cs

Scripts written this sprint (locate in session outputs or `_scripts/`, copy if not in Assets):
- SoulBondTether.cs, ShieldValueHUD.cs, ClericRadarUI.cs, StatusEffectHUD.cs,
  CombatSessionTracker.cs, ClassPoolBuilder.cs
- FloatingDamageText.cs extended with DamageType.Heal/HealCrit/Shield/TriageReturn

---

## FloatingDamageText — Heal Extension (confirm this exists before Step 1)

`FloatingDamageText.cs` must have this enum and Spawn() cases. If missing, add them:

```csharp
public enum DamageType { Normal, Critical, Heal, HealCrit, Shield, TriageReturn }

// In Spawn() switch:
case DamageType.Heal:         color = new Color(0.2f, 0.9f, 0.3f); scale = 1.1f; break;
case DamageType.HealCrit:     color = new Color(0.4f, 1.0f, 0.4f); scale = 1.4f; break;
case DamageType.Shield:       color = new Color(0.4f, 0.7f, 1.0f); scale = 1.0f; break;
case DamageType.TriageReturn: color = new Color(1.0f, 0.8f, 0.2f); scale = 0.75f; break;
```

---

## Step 1 — Wire CombatSessionTracker into WaveSpawner and RodNetworkManager

Read `WaveSpawner.cs` first. In the method that instantiates each enemy (server-side spawn):
```csharp
CombatSessionTracker.Local?.NotifyEnemySpawned(go);
```

Read `RodNetworkManager.cs`. In `OnServerAddPlayer` or wherever the player GameObject is created:
```csharp
CombatSessionTracker.Local?.NotifyAllySpawned(conn.identity.gameObject);
```

Both calls are null-safe (`?.`). Compile-check after.

---

## Step 2 — Wire Healing Numbers into AbilityCaster.cs

Read AbilityCaster.cs first. Find these three methods and add FloatingDamageText calls:

**`CastFieldRepair()` (Mend — index 11):** After `h.Heal(healAmt)`:
```csharp
#if !UNITY_SERVER
FloatingDamageText.Spawn(target.transform.position + Vector3.up * 2f, healAmt, DamageType.Heal);
#endif
```

**`CastDefibrillator()` (Divine Spark — index 25):** After successful revive call:
```csharp
#if !UNITY_SERVER
FloatingDamageText.Spawn(target.transform.position + Vector3.up * 2f, 999f, DamageType.HealCrit);
HeroMasteryManager.Local?.AwardMasteryXp(30, "revive");
#endif
```

**`CastMassDispel()` or equivalent (Dispel — index 27):** After RemoveAll():
```csharp
#if !UNITY_SERVER
FloatingDamageText.Spawn(target.transform.position + Vector3.up * 2f, removedCount, DamageType.Heal);
#endif
```

**`AdaptiveShieldRoutine()` or `CastTransferProtocol()` (Sacred Aegis / Soul Bond):** After shield apply:
```csharp
#if !UNITY_SERVER
FloatingDamageText.Spawn(target.transform.position + Vector3.up * 2f, shieldAmount, DamageType.Shield);
#endif
```

Use the actual method names you find in the file. Add `#if !UNITY_SERVER` guards on all of these.

---

## Step 3 — HeroMasteryManager.cs

Path: `Assets/Game/Characters/Scripts/HeroMasteryManager.cs`

Self-bootstrapping singleton, `DontDestroyOnLoad`. Does NOT run on server (`#if !UNITY_SERVER`).

```
XP thresholds (cumulative): [0, 500, 1200, 2200, 3800, 6000, 9000, 13000, 18000, 25000]
Level 1 = 0 xp, Level 10 = 25000 xp
```

**On start:**
- `GET /api/mastery/{characterId}` using jwt_token
- Cache response: `Dictionary<int, MasteryData>` for all 5 heroes (heroClass → data)
- If character has no mastery rows yet, API auto-creates them (server handles this)
- Apply Mastery 6 bonus for current hero's class if masteryLevel >= 6
- Apply Mastery 10 bonus if masteryLevel >= 10

**`AwardMasteryXp(int xp, string source)`**
- POST `/api/mastery/award` body: `{ characterId, heroClass: PlayerProgressManager.Local.ClassIndex, xpAmount: xp, source }`
- On response: update cache, fire `OnMasteryXpAwarded(int newXp, int newLevel, bool leveledUp)`
- If `leveledUp`: fire `OnMasteryLevelUp(int newLevel)`, apply new bonus

**Mastery 6 bonuses** (patch the relevant passive on the local player only):
```
ClassIndex 0 (Warden):      PassiveOverengineered — aggro radius +1 (set field directly)
ClassIndex 1 (Ironclad):    PassiveThreatProtocol — stackThreshold = 4 instead of 5
ClassIndex 2 (Shadowblade): PassiveBountySystem — cdrNormal += 0.5f, cdrElite += 1f
ClassIndex 3 (Cleric):      PassiveTriageLoop — healReturnPercent = 0.12f
ClassIndex 4 (Arcanist):    PassivePhaseCharge — castThreshold = 5 instead of 6
```

**Mastery 10 bonuses** (additive on top of 6):
```
ClassIndex 0: PassiveOverengineered — deployableLimit = 4
ClassIndex 1: PassiveThreatProtocol — damageReduction = 0.25f
ClassIndex 2: PassiveBountySystem — corruption DPS: set corruptionDps = 10f on WraithAbilities
ClassIndex 3: PassiveTriageLoop — healReturnPercent = 0.18f
ClassIndex 4: PassivePhaseCharge — damageBonus = 0.50f (from 0.40f)
```

Read the actual passive scripts to confirm field names before writing. Static accessor: `HeroMasteryManager.Local`.

---

## Step 4 — HeroMasteryHUD.cs

Path: `Assets/Game/UI/HeroMasteryHUD.cs`

Self-bootstrapping, `DontDestroyOnLoad`, client-only (`#if !UNITY_SERVER`).

- Slim XP bar at bottom of screen, ABOVE the main XP bar. Class color per hero:
  - 0=Warden `#5ba8ff`, 1=Ironclad `#ffb830`, 2=Shadowblade `#c060ff`, 3=Cleric `#30ff7a`, 4=Arcanist `#ff3c3c`
- Label: "Mastery {level}" + XP fraction
- Subscribe to `HeroMasteryManager.Local.OnMasteryXpAwarded`: smooth lerp fill
- Subscribe to `HeroMasteryManager.Local.OnMasteryLevelUp`: class-colored screen flash + "MASTERY LEVEL {N}" punch text (smaller than LevelUpScreen, 1.5s duration)
- H key: toggle `HeroMasteryUI` panel (find or instantiate)
- Build UI entirely in code (no prefab dependency)

---

## Step 5 — HeroMasteryUI.cs

Path: `Assets/Game/UI/HeroMasteryUI.cs`

Full panel, H key toggle, client-only.

- Shows 5 hero cards in a row
- Each card: hero name, mastery level (1–10), XP progress bar, list of unlocked reward strings
- Active hero (matches `PlayerProgressManager.Local.ClassIndex`) gets class-color border + glow
- Reward strings per level: Level 2 "Mastery Border I", Level 3 "VFX Tint", Level 4 "Apprentice Title",
  Level 5 "Mastery Border II", Level 6 "Passive Bonus", Level 7 "Movement Trail", Level 8 "Veteran Title",
  Level 9 "Rotating Ring", Level 10 "Master Title + Aura"
- Locked rewards: grey text. Unlocked: class color text.
- Refresh data from `HeroMasteryManager.Local` cache on open
- Build UI entirely in code

---

## Step 6 — HeroCosmeticApplier.cs

Path: `Assets/Game/Characters/Scripts/HeroCosmeticApplier.cs`

MonoBehaviour, goes on each player prefab (all 5 hero prefabs). Client-only.

Uses `MaterialPropertyBlock` — NO new Material allocations. Never call `renderer.material`.

**Color palettes** (primary, secondary):
```
storm_warden:     #3a4a5e, #5ba8ff   — storm grey + lightning blue
verdant_warden:   #1a3a1a, #c8860a   — forest green + amber
obsidian_guard:   #1a1a1a, #ff6820   — matte black + molten orange
gilded_vanguard:  #c8a830, #f0f0f0   — polished gold + white
crimson_void:     #8b0000, #1a1a2e   — deep red + void black
pale_arcanist:    #e8e8f0, #9060c0   — ice white + pale violet
shadow_cleric:    #2a1040, #c8a030   — deep violet + gold
dawn_cleric:      #f5f0e8, #ff8c42   — warm ivory + sunrise orange
crimson_blade:    #8b0000, #1a1a1a   — blood red + black
phantom:          #4a4a4a, #d0d0d8   — near-invisible grey + shimmer
```

**`ApplySkin(string palette)`**
- Finds all `SkinnedMeshRenderer` and `MeshRenderer` on this GameObject and children
- Gets colors from palette dict
- Sets via `MaterialPropertyBlock`: `_BaseColor` (primary), `_EmissionColor` (secondary × 0.3f)
- Stores current palette in a field so it survives scene reload

**`ClearSkin()`** — resets block to default (no override)

**On Start:** check local character's inventory for equipped cosmetic item with `"cosmetic":"skin"` matching this hero. If found, call `ApplySkin()` with the palette. Only runs on local player (`PlayerIdentity.isLocalPlayer`).

**Trail cosmetics:** `ApplyTrail(string trailType)` — enable the appropriate `TrailRenderer` child component (name: "Trail_Silver", "Trail_Ember", "Trail_Void"). Each trail has color set via the renderer's `colorGradient`.

Trail colors:
- trail_silver: `#c0c0c0` → transparent
- trail_ember: `#ff6820` → `#c84010` → transparent
- trail_void: `#6020c0` → `#301060` → transparent

---

## Step 7 — WaveSpawner Integration: Mastery XP on wave clear

Read `WaveSpawner.cs`. Find the method called when a wave is completed (all enemies dead).
Add server-side award via `[ClientRpc]` to the player who got the kill, OR award to all players
with a broadcast RPC. Pattern:

```csharp
// After wave complete, RPC to all players:
[ClientRpc]
void RpcAwardWaveMasteryXp(int waveNumber)
{
#if !UNITY_SERVER
    HeroMasteryManager.Local?.AwardMasteryXp(10 * waveNumber, "arena_wave");
#endif
}
```

Call it: `RpcAwardWaveMasteryXp(currentWave);` at end of wave-complete sequence.

---

## Step 8 — ClassPoolBuilder.cs (editor tool)

Path: `Assets/Game/Editor/ClassPoolBuilder.cs`

BCE menu item: `BCE/Combat/Create Class Ability Pools`

Creates a `ClassAbilityPool` ScriptableObject asset for each of the 5 heroes in `Assets/Game/Data/`.

Read `ClassAbilityPool.cs` first to confirm field names (`availableIndices`, `defaultEquipped`).

Default ability loadouts:
```
Warden    (0): availableIndices=[0,1,2,3,8,9,10,11,12], defaultEquipped=[0,8,9,11]
Ironclad  (1): availableIndices=[0,1,2,3,13,14,15,16,17,18], defaultEquipped=[7,15,16,18]
Arcanist  (4): availableIndices=[0,1,2,3,4,5,6,7,19,20,21,22], defaultEquipped=[19,20,21,22]
Cleric    (3): availableIndices=[0,1,2,3,23,24,25,26,27,28], defaultEquipped=[24,23,26,28]
Shadowblade(2):availableIndices=[0,1,2,3,29,30,31], defaultEquipped=[1,30,31,29]
```

Save each asset as `Assets/Game/Data/Pool_Warden.asset` etc. Use `AssetDatabase.CreateAsset()`.

---

## Process Rules

1. Read every file you're about to edit before touching it
2. `#if !UNITY_SERVER` guards on ALL UI, HUD, VFX, and API-call code
3. `FindFirstObjectByType<T>()` only in Start/Awake or throttled coroutines — never Update
4. All Health API hooks via `.AddListener()` / `.RemoveListener()` — NEVER `+=` / `-=`
5. After writing each script, check for compile errors before moving to the next step
6. Write scripts directly to `Assets/Game/` subdirectories (not `_scripts/`)
7. At the end: confirm all 8 steps complete and list any wire-up or Inspector steps needed

---

## On Completion

Report:
- Which scripts were written/modified and their full paths
- Any Inspector steps needed (e.g., add component to prefab, assign ScriptableObject)
- Any remaining wire-up that needs a future session
