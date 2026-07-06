# Crossworlds BCE — Combat Design Audit & Recommendations
**Date:** 2026-07-06  
**Scope:** Wave tuning, TTK/TTD curves, enemy composition, class counterplay gaps  
**Based on:** Live code audit of `EnemyController.cs`, `WaveSpawner.cs`, `Health.cs`, `StatusEffectManager.cs`  
**Status:** Proposal — implement after Phase 1 arena is playable

---

## 1. Code → Numbers: What's Actually Shipped

### Player HP
```
maxHealth = 100f (Health.cs default, isPlayer = true)
```
No gear bonus applied at spawn in current code — base 100 HP until CharacterStats wires in.

### Enemy damage (EnemyController.cs Inspector defaults)
```
damage         = 12f
attackInterval = 1.5f   (BehaviorLoop tick 0.2s, attackTimer decremented per tick)
Effective DPS  = 12 / 1.5 = 8 DPS per enemy
```

### Wave enemy counts (WaveSpawner.cs)
```
count = baseEnemiesPerWave(4) + (wave - 1) * enemiesAddedPerWave(2)
```

| Wave | Trash count | Elite? | Total |
|------|-------------|--------|-------|
| 1    | 4           | No     | 4     |
| 2    | 6           | No     | 6     |
| 3    | 8           | Yes    | 9     |
| 5    | 12          | No     | 12    |
| 6    | 14          | Yes    | 15    |
| 10   | 22          | Yes    | 23    |

**Critical finding: enemy damage does NOT scale per wave.** A wave-10 grunt hits exactly as hard as a wave-1 grunt. Only the _number_ of enemies grows. This produces a chaos curve (screen flooded with equally-weak enemies) rather than a tension curve (enemies become genuinely threatening).

---

## 2. TTK / TTD Analysis

### TTD floor (time for a player to die with no inputs)
```
1 enemy focusing:   100 / 8  = 12.5s   ✅ fine
2 enemies focusing: 100 / 16 =  6.25s  ✅ fine
4 enemies focusing: 100 / 32 =  3.1s   ⚠️ borderline (target floor is 3s)
6 enemies focusing: 100 / 48 =  2.1s   ❌ below floor — one-shot feel
```

With no aggro cap in the current code, all enemies in range will target the same player (typically whoever they aggroed first). On wave 5+, a grouped spawn can send 4–6 grunts at one player simultaneously — pushing TTD below 3s before the player can react. This violates the readability lens: players die faster than they can process what hit them.

### TTK (time a player needs to kill a Grunt)
No player-side DPS numbers exist in the current scripts — abilities are stubs. Ballpark from playtesting placeholder:
- Assume ~20 DPS single-target for a DPS class at wave 1 gear
- Grunt with 60–80 HP: TTK ~3–4s ✅ appropriate for trash

**Recommended Grunt HP by wave tier:**

| Wave tier | Grunt HP | Elite HP | Rationale |
|-----------|----------|----------|-----------|
| Waves 1–3  | 60       | 300      | Trash dies fast, elite ~15s focused |
| Waves 4–6  | 80       | 450      | Slight durability bump |
| Waves 7–10 | 110      | 650      | Sustained threat; requires class cooperation |

---

## 3. Critical Fix: Wave Scaling

### Problem
Flat enemy stats across all 10 waves make waves 7–10 feel identical to waves 3–4, just noisier. Players never develop new answers — they just execute the same rotation faster.

### Fix: Add per-wave stat multipliers to WaveSpawner

Two new Inspector fields:
```csharp
[Header("Wave Scaling")]
[Tooltip("HP multiplier added per wave. 0.08 = +8% per wave.")]
public float hpScalePerWave     = 0.08f;
[Tooltip("Damage multiplier added per wave. 0.05 = +5% per wave.")]
public float damageScalePerWave = 0.05f;
```

Apply in `SpawnEnemy()`:
```csharp
void SpawnEnemy(GameObject prefab)
{
    // ... existing spawn code ...
    var ec = enemy.GetComponent<EnemyController>();
    if (ec != null)
    {
        float scale = currentWave - 1;
        ec.damage     *= 1f + damageScalePerWave * scale;
        var h = enemy.GetComponent<Health>();
        if (h != null) h.maxHealth *= 1f + hpScalePerWave * scale;
    }
    // ... rest of existing code ...
}
```

**Resulting TTD at wave 10 vs single enemy:**
```
damage = 12 * (1 + 0.05*9) = 12 * 1.45 = 17.4
TTD    = 100 / (17.4/1.5)  = 100 / 11.6 = 8.6s  ✅
```
Still comfortable for a skilled player. With 4 attacking: 2.15s — dangerous, which is appropriate for wave 10.

### Fix: Aggro cap to enforce TTD floor

Add to EnemyController Inspector:
```csharp
[Header("Aggro")]
[Tooltip("Max enemies that can simultaneously target one player. 0 = unlimited.")]
public static int globalAggroCapPerPlayer = 3;
```

In `TickIdle()`, before setting target: count how many EnemyControllers already target that Transform. If ≥ cap, pick next nearest live player instead. This keeps any single player from taking >3×8=24 DPS simultaneously (~4.2s TTD minimum), and forces spatial awareness rather than kiting one corner.

---

## 4. Wave Composition Shifts

Currently: hardcoded 67% grunt / 33% ranged every wave. No composition change across 10 waves. Tension flatlines.

**Recommended composition by wave:**

| Wave | Grunt% | Ranged% | Notes |
|------|--------|---------|-------|
| 1–2  | 100%   | 0%      | Teach melee aggro, no projectile threat |
| 3–4  | 70%    | 30%     | Introduce ranged — one per squad |
| 5–6  | 60%    | 40%     | Ranged pressure forces positioning |
| 7–8  | 50%    | 50%     | Full mix; elites every 3rd wave |
| 9–10 | 40%    | 60%     | Ranged-heavy — Arcanist/Engineer shine |

Implement via a `List<WaveComposition>` ScriptableObject rather than a hardcoded ratio. Each entry has `waveMin`, `waveMax`, `gruntFraction`. `PickEnemyPrefab()` looks up the current wave's entry.

---

## 5. Class Counterplay Gaps

### What currently exists
All 3 enemy types (Grunt, Ranged, Elite) have identical AI patterns — just different stats. No enemy creates a unique threat that a specific class answers. The Guardian's Taunt, the Cleric's crowd control, and the Engineer's turrets have no enemy that particularly rewards or punishes them.

### Recommended enemy design additions (Phase 2 scope)

**Shielder Grunt** — carries a forward shield, takes 80% reduced damage from the front.  
- Universal answer: flank or knock back  
- Class answer: Shadowblade backstab ignores shield; Guardian Ground Slam hits from below  
- Decay hook: shield durability degrades from Void Rot ticks

**Leaper** — charges straight at lowest-HP player, ignores pathing.  
- Universal answer: dodge/i-frame window  
- Class answer: Engineer Grapple Hook can intercept the charge path; Cleric Divine Rush can rescue the target  
- Decay hook: Leap deals bonus damage proportional to gear decay on the target (encourages repair)

**Void Emitter** — stationary, pulses Weakened every 4s in 6u radius, melee range only.  
- Universal answer: destroy it fast (low HP)  
- Class answer: Arcanist Overcharge burst kills in one rotation; Guardian Taunt keeps grunts from bodyblocking it  
- Decay hook: each pulse also applies 1 Void Rot stack, pre-seeding the debuff combo from COMBAT_PROPOSAL.md

### Spawn cost table (for budget-based wave system)

| Enemy         | Spawn cost | Notes |
|---------------|-----------|-------|
| Grunt         | 1         | baseline |
| Ranged        | 2         | projectile threat multiplier |
| Shielder      | 2         | positional puzzle |
| Leaper        | 3         | burst threat |
| Void Emitter  | 3         | area denial |
| Elite         | 8         | one per wave cap |
| World Boss    | 30        | not in arena wave budget |

**Wave budget = 6 + (wave - 1) × 3** (wave 1 = 6 pts, wave 10 = 33 pts).  
Composition is randomized from a weighted pool for that wave's budget. This replaces the flat count formula and enables emergent wave variety without scripted wave sequences.

---

## 6. Mastery XP Curve Check

Current formula: `xp = 40 + wave * 15` (WaveSpawner.cs:220)

| Wave | XP   | Cumulative (10 waves) |
|------|------|-----------------------|
| 1    | 55   | 55    |
| 5    | 115  | 575   |
| 10   | 190  | 1,250 + 200 arena bonus = 1,450 |

Without knowing the mastery XP thresholds for milestones, these numbers can't be validated. **Action item:** confirm milestone thresholds in `HeroMasteryManager` or DB, then back-calculate how many full 10-wave clears reach each milestone. Target: milestone 1 reachable in ~3 arena clears, milestone 4 requiring 15–20 clears.

---

## 7. Implementation Priority

| Priority | Change | Files | Risk |
|----------|--------|-------|------|
| **P0** | Per-wave HP/damage scaling | `WaveSpawner.cs` | Low — additive multiplier |
| **P0** | Aggro cap per player | `EnemyController.cs` | Low — new field + idle check |
| **P1** | Wave composition ScriptableObject | `WaveSpawner.cs` + new SO | Medium |
| **P2** | Shielder / Leaper / Void Emitter enemy types | New prefabs + EnemyController variants | Medium |
| **P3** | Budget-based wave system | Replaces count formula | High — redesign |

P0 changes can be committed immediately (two Inspector fields + ~10 lines of math). P1+ are Phase 2 scope.

---

## 8. What to Watch in First Playtest

- **Wave 3 elite arrival:** does the team scramble or ignore it? If ignored, Elite HP is too low or the +damage elite bonus isn't tuned.  
- **Wave 6 ranged-heavy:** do players spread to avoid simultaneous projectile hits, or do they stand still? If they stand still and don't die, ranged damage is too low.  
- **Cleric solo viability:** can a solo Cleric survive wave 4 without teammates? If yes, healing is overcentralizing. If no (dies wave 2), Cleric passive sustain needs a floor.  
- **Guardian aggro check:** does Taunt actually pull enemies off teammates, or do enemies re-target immediately? Bug risk — Guard's aggro override needs to beat the existing `_target` assignment in TickChase.
