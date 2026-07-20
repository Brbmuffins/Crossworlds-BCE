# CC Axis + Enchant Layer — VPS / Server Handoff

**Date:** 2026-07-20
**Author:** Combat systems pass (Unity client side done; this is the server/DB remainder)
**Design basis:** `Docs/combat/gear_balance_enchants_2026-07-19.md` (§4.3, §5, §6) and `COMBAT_PROFILE.md`
**Applies to:** the auth API on the VPS (`/opt/crossworlds-auth/server.js`), its MySQL DB, and the dashboard. SSH + service commands per `_CONTEXT/VPS_SERVER.md`.

> **Why this doc exists:** the CLI agent works in the Unity client repo only. Per project rules, server/DB work happens on the VPS, and the gear tables are sacred. The Unity-side of the CC axis is already implemented (see §0). Everything below is the server/DB half that must be done on the VPS.

> **Session scope — this is the COMPLETE server-side task list.** The 2026-07-19/20 combat pass also landed dynamic wave scaling, the CombatBalanceConfig pipeline caps (DR/CDR/total-DR clamp, overheal→shield), and the full Stun lockout — **all of that is client/Unity-side and needs ZERO server work.** The only server/DB work from the entire pass is what's in this doc: two stat keys, one `items` column, an enchant seed pool, and rarity tables.

---

## 0. What is ALREADY done (Unity client — no server action needed)

These shipped in the client and are waiting for data:

| Area | What landed | File |
|---|---|---|
| **New status** | `Stun` — full action lockout, with diminishing returns + immunity | `StatusEffect.cs`, `StatusEffectManager.cs` (`ApplyStun`, `IsStunned`) |
| **New stat channels** | `Tenacity` (cap 0.60) and `ControlPower` (cap 0.50) on `CharacterStats` | `StatModifier.cs` (`StatType.Tenacity`, `StatType.ControlPower`), `CharacterStats.cs` |
| **CC duration formula** | `appliedDuration = base × (1 − targetTenacity) × (1 + casterControlPower)`, applied server-side to Stun/Slow/Silenced/Bound | `StatusEffectManager.ScaleControlDuration` + `CombatBalanceConfig.CCDuration` |
| **Stun lockout (done)** | Stunned players are frozen (movement + dodge + ability cast/interrupt) via a server→client `SyncVar` on `Health` (`_stunned`); enemies gate server-side | `Health.cs` (`IsStunned`/`SetStunned`), `PlayerMovement.cs`, `AbilityCaster.cs`, `EnemyController`/`EnemyAI`/`FieldGhoulNPC` |
| **All tunables** | Stun DR/immunity + Tenacity/Control caps are serialized fields | `CombatBalanceConfig` (create the `Resources/CombatBalanceConfig` asset in Unity) |

**Consequence for the server:** `Stun` is fully functional end-to-end — anything that calls `StatusEffectManager.ApplyStun` (an enchant like `stagger_to_stun`, or a future stunning enemy) works immediately. The client can already *consume* `Tenacity`/`ControlPower` as gear stats. It just needs the DB to supply the stat values and the enchant socket data.

---

## ⚠ Blocking open question — verify BEFORE seeding gear stats

**How does a gear item's DB `stat_bonus` reach `CharacterStats` on the client?**

In the current client, **gear** stats come from Unity `ItemData.baseModifiers` ScriptableObjects (authored in-editor, matched to the DB by `ItemData.serverItemId` → `items.id`). The `stat_bonus` JSON key vocabulary (`damage_pct`, `dr_pct`, `cdr_pct`, `heal_pct`, `move_pct`, `max_health`) is only parsed at runtime for **consumables** (`ConsumableEffect.Effects`, a hand-maintained dict). No runtime JSON→StatModifier parser was found for gear.

Server-side (per `_CONTEXT/CLAUDE.md`): the `items` table already has a `stat_bonus JSON` column, and `GET /api/items` (no auth) returns every row for the Unity bag UI. So the DB is the authoring home — but confirm whether Unity actually reads `stat_bonus` from that payload for **gear**, or only mirrors it into hand-authored `ItemData` SOs. If the former, find and extend the parser; if the latter, the DB values are reference-only until the SOs carry the matching `StatType.Tenacity`/`ControlPower` modifiers (the enum already supports them).

So one of these is true — confirm which on the server before adding keys:

1. **SO-authored gear (likely):** DB `stat_bonus` is reference/authoring data; a human (or a generator) mirrors it into `ItemData` SOs in Unity. → Adding `tenacity_pct`/`control_pct` to the DB is only useful once the SOs (or a DB→SO generator) carry the matching `StatType.Tenacity`/`ControlPower` modifiers. **The client enum already supports them.**
2. **Server returns stats at equip:** if `/character/gear/equip` (or an `/api/*` equivalent) returns computed stat_bonus the client applies, then there must be a client parser to extend — locate it and add the two keys there too.

**Do not seed rare gear until this path is confirmed**, or the numbers won't move the character.

---

## 1. New `stat_bonus` keys (design §4.3)

Extend the seed vocabulary with two additive, per-slot keys:

```
"tenacity_pct"  → StatType.Tenacity      (client cap 0.60, enforced in CharacterStats.Recalculate)
"control_pct"   → StatType.ControlPower   (client cap 0.50, enforced in CharacterStats.Recalculate)
```

- Additive across slots; the **caps live in Unity** (`CombatBalanceConfig.tenacityCap` / `controlPowerCap`), not the DB — do not also clamp server-side or you'll double-cap.
- Keep them inside the 2.0–2.5× power band (see §3). Tenacity competes with DR/MaxHealth for defensive slots; ControlPower competes with Damage for offensive slots.

---

## 2. Enchant / attunement layer (design §5)

**Rule: base stats scale power; enchants change how you play.** Do not make enchants "+more stats."

### 2.1 Schema — one additive, nullable column

Target the **`items`** table (the new-system table; `ItemData.serverItemId` → `items.id`). It is NOT one of the sacred legacy tables (`item_template`, `item_instance`, `character_gear`). Use the project's idempotent MySQL-8 pattern (`_CONTEXT/CLAUDE.md` → "MySQL 8 migrations"):

```sql
-- shape of the column: [{ "id": "far_zone_hot" }, ...]  -- just socketed attunement ids
SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.COLUMNS
   WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='items' AND COLUMN_NAME='modifiers') = 0,
  'ALTER TABLE items ADD COLUMN modifiers JSON NULL',
  'SELECT 1'
);
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
-- verify:
SHOW CREATE TABLE items;
```

- Nullable, additive, backward-compatible — existing rows read as no enchant. No data migration.
- **Surface it in `GET /api/items`** (the no-auth endpoint that feeds the Unity bag UI, per `_CONTEXT/CLAUDE.md`) right alongside `stat_bonus`, so the client can read socketed ids. Same for any equip/inventory payload the client applies hooks from. Keep the `{success, data}` envelope on any new/changed `/api/*` response.
- One socket per piece for Phase 1 (`CombatBalanceConfig.maxAttunementSlotsPerItem = 1`); the JSON array already supports multi-slot later. **No random affix rolls yet** — a fixed, hand-authored pool.
- Parameterized queries only; wrap any multi-table seed in a transaction; restart + verify per `_CONTEXT/CLAUDE.md` ("After any change").

### 2.2 Starter pool (~12, hand-authored)

Each id below is stored in `items.modifiers`; its *magnitudes* live on a Unity `AttunementDef` ScriptableObject (one asset per id — that's a Unity task, not DB). The DB only stores the socketed id.

| id | Effect | Client hook |
|---|---|---|
| `far_zone_hot` | Far-zone shields also grant 8% of absorb as a 3s HoT | variant resolve + `onHealApplied` |
| `close_zone_curse` | Close-zone releases apply 1 Cursed stack | variant resolve → `StatusEffectManager` |
| `charge_slow` | Full-charge casts apply Slow (1.5s) | chargeable release |
| `killstreak_free_cast` | First cast after a kill costs no cooldown | `onKilledBy` (self) |
| `overheal_shield_plus` | +15% to the overheal→shield spill rate | heal path (`Health.Heal`, already spills) |
| `weakened_hunter` | +15% damage to Weakened targets | `TakeDamage` amp layer |
| `threat_siphon` | Each Threat stack also grants +2% ControlPower (Ironclad) | Threat Protocol |
| `wisp_split` | Spirit Wisps have a 20% chance to spawn a second orb | Cleric wisp behaviour |
| `phase_overflow` | Phase Charge triggers at 5 casts instead of 6 (Arcanist) | Phase Charge meter |
| `stagger_to_stun` | Your Stagger has a 25% chance to become a 1.5s Stun | Stagger apply → `StatusEffectManager.ApplyStun` |
| `bulwark_reflect` | While shielded, reflect 10% of absorbed damage | shield layer |
| `tenacity_on_low` | +25% Tenacity while below 35% HP | `onHealthChanged` |

> Note `stagger_to_stun` and `charge_slow` now have real client targets: `ApplyStun(...)` and the Slow path both exist and both respect the Tenacity/ControlPower formula.

---

## 3. Rarity roll tables (design §6, §2.3)

Derive every roll range from the **2.0–2.5× base→BiS power band**, not per-item invention. Target ceilings (post cap-fixes, already wired client-side):

| Metric | Base | BiS target | Mult |
|---|---|---|---|
| Effective HP (non-tank) | 100 | ~230 | 2.3× |
| Single-target DPS | 1.0× | ~2.2× | 2.2× |
| Heal throughput | 1.0× | ~2.2× | 2.2× |
| Cast frequency (CDR) | 1.0× | 1.67× (gear cap 0.40) | 1.67× |
| Tank EHP (with cds) | — | ≤6.7× (total-DR clamp 0.85) | ceiling |

If any rarity table pushes a channel past its column, the range is too fat — trim it. The DR/CDR/clamp caps are enforced in Unity (`CombatBalanceConfig`), so the DB just needs to stay within band.

---

## 4. Ground rules (from CLAUDE.md — do not violate)

- **Sacred, never modify:** `/character`, `/character/gear/equip`, `item_template`, `character_gear`. Unity calls them on every spawn. The `modifiers` column goes on the newer `items` table (confirm §0/§2.1).
- **New `/api/*` responses** use the `{success, data}` / `{success, error}` envelope; error strings are shown to players verbatim.
- **Back up before altering:** the deploy path auto-backs-up; for raw SQL take a manual dump first. Everything here is additive/nullable and reversible (`ALTER TABLE items DROP COLUMN modifiers;`).
- **Never commit credentials** — `.env` on the VPS only.
- Ports frozen: 3000 auth, 4000 dashboard, 7777/UDP game, 3001 Kuma.

---

## 5. Suggested order on the VPS

1. **Resolve §0's open question** (SO-authored vs server-returned gear stats). Everything else depends on it.
2. Add `tenacity_pct` / `control_pct` to the stat_bonus seed vocabulary (and to any server stat parser, if one exists).
3. `ALTER TABLE items ADD COLUMN modifiers JSON NULL;` and surface it in the equip/inventory payload.
4. Seed the 12-enchant pool ids; author the matching `AttunementDef` SOs in Unity (separate client task).
5. Derive rarity roll tables from the §3 band; seed rare gear.
6. Dashboard: expose `modifiers` for manual per-item socket editing (per design §8.3, live enchant editing is a dashboard feature, not Unity).

---

## 6. Verification

- After the `ALTER`, confirm existing characters still equip old gear (nullable column = no behavior change).
- Seed one test item with `tenacity_pct: 0.2`; in a test fight, a Slow/Stun on that character should be ~20% shorter (the client formula reads `CharacterStats.Tenacity`). If duration doesn't change, §0's stat-application path is the culprit — the DB value isn't reaching `CharacterStats`.
- Seed one `stagger_to_stun` enchant; confirm Stun applies AND respects diminishing returns (3 stuns in 8s → 4s immunity) — all driven by the `CombatBalanceConfig` asset.
- Rollback: `ALTER TABLE items DROP COLUMN modifiers;` + revert the seed rows.
