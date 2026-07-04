# Crossworlds BCE — Hero Mastery System

Inspired by: Smite's per-god mastery track  
Status: Phase 2 build target  
Last updated: 2026-06-28

---

## Concept

Account level (shared across all heroes) tells you how far you are in the game. Hero Mastery tells you how far you are with a *specific* hero. A player who exclusively mains Cleric should feel their Cleric getting stronger, more expressive, and more recognized — independent of how many heroes they've tried.

This is the answer to: *"I don't care about quests — I just want to play my hero."*

---

## Mastery Track

Each hero has 10 mastery levels. Mastery XP is earned only when playing that hero. The track is per-character-per-hero (one character can master multiple heroes, but mastery is earned separately for each).

### XP Table

| Mastery Level | XP Required (cumulative) |
|---|---|
| 1 → 2 | 500 |
| 2 → 3 | 1,200 |
| 3 → 4 | 2,200 |
| 4 → 5 | 3,800 |
| 5 → 6 | 6,000 |
| 6 → 7 | 9,000 |
| 7 → 8 | 13,000 |
| 8 → 9 | 18,000 |
| 9 → 10 | 25,000 |

Level 10 is a meaningful achievement — roughly 30–50 hours of dedicated play on that hero.

### Mastery XP Sources

| Action | Mastery XP | Notes |
|---|---|---|
| Arena wave cleared (per wave) | 10 | Playing any arena while on this hero |
| Elite enemy kill | 25 | |
| World boss participation | 150 | Any damage dealt counts |
| World boss kill | 300 | |
| Dungeon room cleared | 40 | |
| Dungeon completed | 200 | |
| PvP win | 75 | |
| Craft an item | 15 | Profession actions count |
| Daily quest completed | 50 | While playing this hero |
| Revive an ally (Cleric only) | 30 | Class-specific bonus |
| Kill streak ×5 without dying | 40 | Any hero |

Mastery XP is NOT earned from: death, spectating, being AFK, or actions taken by other players.

---

## Mastery Rewards by Level

### All Heroes

| Level | Reward |
|---|---|
| 2 | **Mastery Border I** — thin colored ring on nameplate (hero color) |
| 3 | **Ability VFX Tint** — all cast indicators shift to a richer, saturated version of class color |
| 4 | **Title: "[Hero] Apprentice"** — shown under name in nameplate |
| 5 | **Mastery Border II** — double ring, glowing, shown in Character Select |
| 6 | **Minor Passive Bonus** — class-specific (see below) |
| 7 | **Ability Trail** — movement leaves a faint hero-colored particle trail (0.3s lifetime, 6 particles) |
| 8 | **Title: "[Hero] Veteran"** |
| 9 | **Mastery Border III** — animated rotating ring, visible to all players in Hub |
| 10 | **Title: "Master [Hero]"** + unique aura VFX (persistent ambient glow) + permanent minor bonus II |

### Class-Specific Bonuses at Mastery 6

These are minor — feel, not power. They reward dedication without breaking balance.

| Hero | Mastery 6 Bonus |
|---|---|
| Warden | Runic Sentinel auto-targets sooner (aggro radius +1u) |
| Ironclad | Threat Protocol activates at 4 stacks instead of 5 |
| Arcanist | Phase Charge fires at 5 casts instead of 6 |
| Cleric | Triage Loop returns 12% instead of 8% |
| Shadowblade | Bounty System CDR: +0.5s on normal kill, +1s on elite |

### Class-Specific Bonuses at Mastery 10

| Hero | Mastery 10 Bonus |
|---|---|
| Warden | Deployable limit: 4 instead of 3 |
| Ironclad | Threat Protocol grants 25% DR instead of 20% |
| Arcanist | Phase Charge damage bonus: +50% instead of +40% |
| Cleric | Triage Loop returns 18% instead of 8% — significant, feels amazing to main |
| Shadowblade | Corruption DPS: 10/s instead of 8/s |

---

## Schema

```sql
hero_mastery (
  character_id    INT NOT NULL,
  hero_class      INT NOT NULL,       -- 0=Warden 1=Ironclad 2=Arcanist 3=Cleric 4=Shadowblade
  mastery_xp      INT NOT NULL DEFAULT 0,
  mastery_level   INT NOT NULL DEFAULT 1,
  last_updated    TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (character_id, hero_class),
  FOREIGN KEY (character_id) REFERENCES characters(id)
)
```

XP thresholds are hardcoded server-side (no DB table needed — the array is static).

---

## Server Endpoints

```
GET  /api/mastery/:characterId
     Returns: { heroClass, masteryXp, masteryLevel, xpToNext, bonusesUnlocked[] }
     for all 5 heroes (always returns all 5, unplayed heroes = level 1, 0 xp)

POST /api/mastery/award
     JWT required. Body: { characterId, heroClass, xpAmount, source }
     Awards XP, checks for level-up, returns { newXp, newLevel, leveledUp, rewardsUnlocked[] }
     Log prefix: [MASTERY]

GET  /api/mastery/leaderboard?heroClass=3
     Public. Top 20 players by mastery level + XP for a given hero class.
     Used by combat atlas web page + in-game leaderboard.
```

`POST /api/mastery/award` is called by the game server (Unity `[Command]`) — not directly by the client. The server awards XP based on server-verified events (wave cleared, enemy killed, etc.) so it can't be spoofed.

---

## Unity Integration

### `HeroMasteryManager.cs`
Path: `Assets/Game/Characters/Scripts/HeroMasteryManager.cs`

- Self-bootstrapping singleton, `DontDestroyOnLoad`
- On scene load: `GET /api/mastery/:characterId`, caches all 5 hero mastery states
- `AwardMasteryXp(int xpAmount, string source)` — calls `POST /api/mastery/award` for current hero
- On level-up response: fires `OnMasteryLevelUp(int newLevel)` UnityEvent
- Applies Mastery 6/10 bonuses to `CharacterStats`, `PassiveTriageLoop`, etc. on login
- Static accessor: `HeroMasteryManager.Local`

### `HeroMasteryHUD.cs`
Path: `Assets/Game/UI/HeroMasteryHUD.cs`

- Self-bootstrapping, `DontDestroyOnLoad`
- Persistent slim XP bar at bottom of screen, ABOVE the main XP bar (different color — class color, not gold)
- Shows: hero icon placeholder (colored circle) + "Mastery X" + current XP / XP to next
- On XP award: bar fills with smooth lerp, then flashes class color
- On level-up: full-screen class-colored flash (smaller than account LevelUp), "MASTERY LEVEL X" text punch
- H key: opens `HeroMasteryUI` full panel

### `HeroMasteryUI.cs`
Path: `Assets/Game/UI/HeroMasteryUI.cs`

- Full panel, H key toggle
- Shows all 5 hero cards: hero name, current mastery level (1–10), XP progress bar, unlocked rewards listed
- Active hero card highlighted with class color border + glow
- Each reward row: locked (grey) or unlocked (class color) with reward description
- Leaderboard tab: calls `GET /api/mastery/leaderboard?heroClass=X`, shows top 20

### `PlayerNameplate.cs` extension
- Read mastery level from `HeroMasteryManager.Local` cache
- Show mastery border ring around nameplate based on level:
  - Level 2–4: thin static ring
  - Level 5–8: double ring
  - Level 9–10: animated rotating ring (rotates at 30°/s)

---

## Integration with Other Systems

**On arena wave clear:** `WaveSpawner.cs` fires `HeroMasteryManager.Local?.AwardMasteryXp(10 * waveNumber, "arena_wave")`

**On enemy kill:** `EnemyController.cs` death sequence checks `isElite` → award 10 or 25 mastery XP to killing player via `[ClientRpc]`

**On craft:** `CraftingUI.cs` POST /api/craft success → `HeroMasteryManager.Local?.AwardMasteryXp(15, "craft")`

**On Cleric revive:** `AbilityCaster.CastDefibrillator()` — after successful revive → `AwardMasteryXp(30, "revive")`

**Daily quest complete:** `QuestTracker.cs` → `AwardMasteryXp(50, "daily_quest")`

---

## Visual Identity Per Hero

At Mastery 3+, ability cast indicators shift from neutral grey/white to the hero's saturated class color:

| Hero | Mastery indicator color |
|---|---|
| Warden | Bright electric blue `#5ba8ff` |
| Ironclad | Burnished gold `#ffb830` |
| Arcanist | Void crimson `#ff3c3c` |
| Cleric | Radiant emerald `#30ff7a` |
| Shadowblade | Deep violet `#c060ff` |

This is cosmetic only — it requires changing the `GetCategoryColor()` return value in `AbilityCaster` once mastery level is read. If mastery < 3, use default category colors.

---

## Notes on Quests vs Mastery

Quests push players toward specific actions ("kill 15 grunts"). Mastery rewards them for just *playing their hero well*. The two systems complement each other:

- Daily quests give direction on any given session
- Mastery gives the sense of deepening over weeks and months
- Players who ignore quests still progress via mastery from normal play
- Players who love optimization will chase both simultaneously

The Cleric in particular benefits — every heal, every revive, every Triage proc is quietly building toward Mastery 10, which gives a meaningful and thematic reward (Triage Loop 18% is genuinely satisfying for a healer main).
