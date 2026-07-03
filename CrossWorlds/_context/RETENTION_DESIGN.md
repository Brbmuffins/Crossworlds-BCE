# Crossworlds BCE — Retention & Daily Systems Design

Status: Design — Phase 2 post-playtest implementation  
Last updated: 2026-06-28

---

## Design Goal

The question this document answers: *why does a player open the game on day 7, day 30, and day 90?*

Phase 1 gives players a reason to play the first session. Phase 2 gives them a reason to come back for a week. This document designs the systems that make coming back feel rewarding on any given day — without requiring other players to be online.

Core principle: **every login should offer at least one thing to do and one thing to look forward to.**

---

## Daily Systems

### Daily Login Reward

Simple streak system. Login once per calendar day → claim reward. No complex chain — the reward resets to day 1 if you miss a day, but the cap is day 7 (so a week-long player gets the same rewards as a month-long player, just repeatedly).

**Weekly cycle:**

| Day | Reward |
|---|---|
| 1 | 50 gold |
| 2 | 2× `material_copper_shard` |
| 3 | 100 gold |
| 4 | Random uncommon item (from loot pool) |
| 5 | 150 gold + 500 XP |
| 6 | 1× recipe unlock scroll (single-use, unlocks one random recipe) |
| 7 | 300 gold + 1× rare item + 7-day cosmetic title |

**Schema:**
```sql
daily_logins — character_id FK PK, last_claim_date DATE, current_streak INT DEFAULT 0,
               total_logins INT DEFAULT 0
```

**Endpoint:** `POST /api/daily/claim` — JWT, {characterId}  
Checks `last_claim_date` vs today (server time). Awards reward, increments streak. Returns reward description for Unity popup.

**Unity:** `DailyRewardUI.cs` — auto-opens on Hub load if unclaimed. Shows animated reward card (class color flash, item name, gold amount). Can be dismissed after claim.

---

### Daily Quests

3 quests generated fresh each real day per character. Mix of kill, gather, and social quests. Designed to take 15–30 minutes total.

**Quest pool examples:**

*Kill quests:*
- "Clear a wave in the Copper Arena without dying" — bonus 200 XP
- "Kill 15 Grunt enemies" — 100 gold
- "Land the killing blow on an Elite enemy" — uncommon item drop

*Gather quests:*
- "Mine 5 Copper Ore" — 150 XP + 75 gold
- "Craft 1 item at the Forge" — 100 XP + recipe scroll
- "Collect 3 WorldItems in a single arena run" — 200 gold

*Social quests:*
- "Complete an arena run with at least 2 other players" — 300 XP
- "Revive a downed ally" — 150 XP (Cleric magnet)
- "Deal damage to a World Boss" — 500 XP (pulls players into boss events)

**Schema:**
```sql
daily_quest_pool   — id VARCHAR(64) PK, type ENUM(kill,gather,social,craft),
                     description VARCHAR(255), target_type VARCHAR(64), target_count INT,
                     xp_reward INT, gold_reward INT, item_reward_id VARCHAR(64) NULL

character_daily_quests — character_id FK, quest_id FK, date DATE, progress INT DEFAULT 0,
                         completed TINYINT DEFAULT 0, reward_claimed TINYINT DEFAULT 0,
                         PRIMARY KEY(character_id, quest_id, date)
```

**Endpoint:** `GET /api/daily/quests/:characterId` — auto-generates 3 quests for today if none exist (seeded by `character_id + date` for determinism — same player always gets same daily).

**Unity:** `DailyQuestUI.cs` — tab in QuestLogUI. Shows today's 3 quests, progress bars, claim reward button on completion.

---

### Profession Daily Bonus

Each profession (Mining, Smithing) has a "bonus window" — 15 real minutes each day during which XP from that profession is doubled. Window time rotates daily (different each day to reward different playstyles/timezones).

No schema needed — calculated server-side from `DATE + profession_id` hash.

**Endpoint:** `GET /api/professions/bonus-window` — returns `{ profession_id, window_start, window_end, active: bool }` for current time.

---

## Weekly Systems

### Weekly Challenge

One challenge active per week, shared by all players. Progress is cumulative across the server — everyone contributes.

**Example weekly challenges:**
- "Server total: kill 5,000 Grunt enemies" → at completion, all online players get 500 gold
- "Server total: craft 200 items" → reward: all players unlock a temporary recipe for the week
- "Server total: complete 50 dungeon runs" → bonus loot in dungeons for the rest of the week

**Schema:**
```sql
weekly_challenges  — id VARCHAR(64) PK, week_start DATE, description VARCHAR(255),
                     type VARCHAR(64), target_count INT, current_count INT DEFAULT 0,
                     completed TINYINT DEFAULT 0, reward_type VARCHAR(64), reward_value INT

weekly_contributors — challenge_id FK, character_id FK, contribution INT DEFAULT 0,
                      PRIMARY KEY(challenge_id, character_id)
```

**Endpoint:** `GET /api/challenges/weekly` — current week's challenge + server progress.  
Unity: `WeeklyHUD.cs` — small widget in Hub showing challenge name + progress bar, updates every 30s.

---

### Weekly Milestone

Personal weekly milestone. Resets Monday. Threshold system — clear higher milestones for better rewards.

**Milestone tiers:**
- Tier 1 (easy): Complete 3 daily quests this week → 200 gold
- Tier 2 (medium): Complete 1 arena run with full party → uncommon gear
- Tier 3 (hard): Reach wave 5 in an arena → rare material + 500 XP
- Tier 4 (prestige): Kill a World Boss this week → title + rare item

**Schema:** Extend `character_quests` with `weekly_reset TINYINT DEFAULT 0` flag, or dedicated `weekly_milestones` table.

---

## Season System

Seasons run 8 weeks. Each season has a theme and a unique reward track.

**Season 1: "The Copper Age"** (launch season)

Earn Season Points from: arena kills (1pt), crafting (2pt), dungeon completions (10pt), world boss (25pt), PvP wins (5pt).

**Season track (cumulative):**

| Points | Reward |
|---|---|
| 50 | Cosmetic nameplate color: copper |
| 150 | Title: "Copper Runner" |
| 300 | 500 gold |
| 500 | Exclusive item: `ring_copper_season` (better stats than craftable) |
| 750 | 1,000 gold + rare crafting material |
| 1,000 | Title: "Iron Pioneer" + unique class-colored player trail VFX |
| 1,500 | Prestige badge on character select screen |

At season end, rewards are locked in. Season 2 starts fresh with a new theme.

**Schema:**
```sql
seasons              — id INT PK, name VARCHAR(64), theme VARCHAR(64),
                       start_date DATE, end_date DATE, active TINYINT DEFAULT 0

character_season_xp  — character_id FK, season_id FK, points INT DEFAULT 0,
                       last_reward_tier INT DEFAULT 0,
                       PRIMARY KEY(character_id, season_id)
```

---

## Player Progression Loop (day-by-day)

```
Day 1:  First session. Login reward day 1. Tutorial quest. First arena run.
Day 2:  Login reward day 2. Daily quests (kill 15 grunts, mine ore). Craft first item.
Day 3:  Login reward day 3. Talent tree opens. Invest first points. Notice build feel different.
Day 4:  Login reward day 4. Daily quest sends into dungeon. First dungeon completion.
Day 5:  Login reward day 5. World boss spawns. Join the community fight.
Day 6:  Login reward day 6 (recipe scroll). Unlock new recipe. Craft something better.
Day 7:  Login reward day 7 (rare item). Weekly milestone complete. Feel the week of progress.
Week 2: Guild invite from a player met in Day 5 boss. New social layer activates.
Week 4: Season halfway. Check leaderboard. PvP queue to prove the build.
Week 8: Season ends. Prestige badge earned. Season 2 starts with new theme and energy.
```

---

## Anti-Burnout Rules

These are hard rules to prevent players from feeling obligated or exhausted:

1. **Daily quests never require other players.** Social quests are bonus, never mandatory.
2. **Missing a daily is low-cost.** Streak resets to 1, not 0 reward — day 1 gives 50 gold, always.
3. **Season points have no weekly cap.** A player who logs in once a week can still hit tier 3.
4. **No timers visible in-game beyond the bonus window.** No countdown UI pressure.
5. **Daily login claim is persistent for 24h** — claim at midnight or at noon, same reward.
6. **No FOMO on cosmetics.** Season cosmetics are titles and colors, not stat items.

---

## What This Is NOT

- Not a gacha system — no random premium pulls
- Not an energy system — no "you ran out of plays"
- Not a pay-to-win track — all rewards are earnable in normal play
- Not an always-online requirement — solo players can earn everything except "complete with party" quests
