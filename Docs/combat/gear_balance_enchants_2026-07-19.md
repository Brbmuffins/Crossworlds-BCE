# Crossworlds BCE — Gear, Balance & Enchant Design
**Date:** 2026-07-19
**Author:** Combat design pass
**Scope:** How equipment impacts the current (v3) combat profile — power-band targets, DR/CDR cap fixes, a CC/tenacity axis, and a variant-anchored enchant layer
**Basis:** `COMBAT_PROFILE.md` (v3, July 2026) — actual `CharacterStats` channels, `StatusEffectManager` types, and variant-zone system as shipped
**Status:** Proposal — numbers are starting points; validate in playtest once the arena scene exists

---

## 0. The governing fact: no leveling

Per the combat profile, **all power comes from gear** — there is no XP curve. This has one dominant consequence: **gear balance IS combat balance.** There is no level system absorbing power creep or smoothing mixed-gear co-op. Every decision below flows from that.

Two guardrails fall out of it:

1. **Pick a target power band and reverse-engineer rarity from it.** A fully-geared character should land at roughly **2.0–2.5× the effective throughput** of a base-geared one — not 5×. Wider than that and a geared player trivializes waves a fresh teammate can't survive, breaking co-op. Narrower and drops stop feeling like power. Every rarity roll table is derived from this ceiling, not invented per-item.
2. **The stat caps are the real difficulty ceiling.** With no levels, `DamageReduction` (cap 0.8) and `CooldownReduction` (cap 0.6) are the hardest levers in the game. They need to be set deliberately (Section 2).

---

## 1. How gear maps onto the combat verbs

The variant-zone system gives every ability three dials: **magnitude** (charge), **type** (aim zone), **frequency** (cooldown). The existing `CharacterStats` channels drive them directly — no new plumbing needed to start:

| `CharacterStats` channel | Cap | Cone lever it drives | Build fantasy |
|---|---|---|---|
| `DamageMultiplier` | — | payload magnitude (offense zones) | glass cannon / bruiser |
| `HealMultiplier` | — | payload magnitude (heal zones) | medic / off-healer |
| `CooldownReduction` | 0.6 | cast frequency | "always casting" uptime build |
| `DamageReduction` | 0.8 | effective HP (survival) | frontline anchor |
| `MaxHealthBonus` | — | effective HP (survival) | frontline anchor |
| `MoveSpeedMultiplier` | — | reposition between casts | kiter / dodger |
| `TemporaryCDR` / `TemporaryDmgPct` | ±0.6 / −1→+2 | burst-window spikes | consumable-driven burst |

The design win already present: **role is a gear question, not a class question.** Every class has both heal and damage variants (Warden Mend, Ironclad War Cry shout-heal, Cleric Smite), and Heal/Damage are separate channels. A Cleric who stacks `DamageMultiplier` + damage variants is a valid off-healer; one who stacks `HealMultiplier` is a pure medic. Gear expresses the build; variants execute it. **Do not add a class-locked role system — the coexistence engine is already here.**

---

## 2. Cap & stacking fixes (do this before seeding rare gear)

### 2.1 The DR stacking hole

Gear DR (pipeline layer 4) and ability DR (layer 3) are **separate multiplicative layers**, so they compound *past* the 0.8 gear cap. Worked example — a gear-capped Ironclad in Stalwart Stance:

```
Ability DR  = Stalwart 0.40 combined with Threat Protocol 0.20
            = 1 − (1−0.40)(1−0.20) = 1 − 0.48 = 0.52   → ×0.48 multiplier
Gear DR     = 0.80 (capped)                              → ×0.20 multiplier
Incoming    = raw × 0.48 × 0.20 = raw × 0.096
            → ~90.4% total DR, ~10.4× effective HP
```

Add the 80-absorb shield, flat `MaxHealthBonus`, Soul Bond redirect, and Kinetic Reversal (eats hits entirely), and a stance tank is functionally unkillable for the 6s window. That may be an intended cooldown payoff — but with gear pushing the floor it should be a *decision*, not an emergent surprise.

**Fix (pick one):**
- **A — Total-DR clamp (preferred, minimal change):** after both layers resolve in `TakeDamage`, clamp effective DR to **0.85** (`incoming = max(raw × 0.15, computed)`). One line in the pipeline, preserves both systems, guarantees a damage floor. 0.85 = ~6.7× EHP ceiling, still a huge tank payoff without immunity.
- **B — Shared additive pool:** fold ability DR named-keys and gear DR into one additive pool capped at 0.85 before the multiply. Bigger refactor; violates minimum-change. Prefer A.

### 2.2 Reconsider the CDR cap

0.6 CDR = abilities fire **2.5× as often**. Because there's no leveling, gear alone can reach this, and it swings both DPS and healing throughput more than any damage stat. Recommend **lowering the gear-reachable CDR cap to 0.40** (2.5× → 1.67× frequency) and reserving the remaining headroom to 0.60 for the temporary Overdrive window (`TemporaryCDR`). This keeps "always casting" as an *earned burst state*, not a passive baseline, and stops CDR from being the one stat every build stacks first.

### 2.3 Target-number sanity table (base vs BiS, post-fix)

| Metric | Base gear | Fully geared (target) | Multiplier |
|---|---|---|---|
| Effective HP (non-tank) | 100 | ~230 | 2.3× |
| Single-target DPS | 1.0× | ~2.2× | 2.2× |
| Heal throughput | 1.0× | ~2.2× | 2.2× |
| Cast frequency (CDR) | 1.0× | 1.67× (cap 0.40) | 1.67× |
| Tank EHP (with cds) | — | ≤6.7× (clamp 0.85) | ceiling |

If any rarity table pushes a channel past its column here, the roll range is too fat — trim it.

---

## 3. Coexistence guarantees (so healing isn't a trap)

Two mechanics keep `HealMultiplier` a real build target rather than dead weight:

1. **Overheal spills into shield.** Healing beyond a target's max HP converts a fraction (start **25%**, capped at the existing 80 shield ceiling) into Sacred-Aegis-style absorb. You already have the Far-zone shield to spill into. Now stacking `HealMultiplier` is never wasted on a topped-off ally — throughput always converts to value.
2. **Sustained incoming pressure must exist.** Per the 07-06 audit, waves are currently a chaos curve (many equally-weak enemies) not a tension curve, and enemy damage doesn't scale per wave. **Land per-wave damage/HP scaling + the aggro cap first.** If nothing sustains damage on the party, every `HealMultiplier` point is dead and every build defaults to damage. Healing balance is downstream of the wave-scaling fix — sequence it accordingly.

---

## 4. New axis: Crowd Control + gear

The status system has Slow, Stagger, Silenced, Cursed, Weakened, Bound — but **no player-facing Stun and no CC-resistance stat.** That's a clean opening for a third build axis beyond offense/defense.

### 4.1 New status type

- **`Stun`** — full action lockout (movement + abilities). Short by default (**1.5s**). **Hard diminishing returns:** each Stun on the same target within 8s applies at 50% of the previous duration; after 3, target gains 4s Stun immunity. This is a fairness floor — without it, co-op degenerates into stunlock death. Resolves in `StatusEffectManager` like any other type.

### 4.2 Two new `CharacterStats` channels

| Channel | Cap | Effect | Competes with |
|---|---|---|---|
| `Tenacity` | 0.60 | reduces duration of CC *applied to you* | DR / MaxHealth for defensive slots |
| `ControlPower` | 0.50 | extends duration of CC *you apply* | DamageMultiplier for offensive slots |

**Duration formula (server-side, authoritative):**
```
appliedDuration = baseDuration × (1 − target.Tenacity) × (1 + caster.ControlPower)
```
Applies to Stun, Slow, Silenced, Bound (not to DoTs like Cursed, and not to Weakened's amplifier). A glass-cannon who skipped Tenacity gets chain-CC'd and dies — the correct consequence of a build choice. A `ControlPower` build (Frost Nova freeze, Vine Grasp prison, a Stun variant) becomes a real controller identity that is neither top-DPS nor top-heal.

### 4.3 New `stat_bonus` JSON keys

Extend the existing seed pattern (`damage_pct`, `dr_pct`, `move_pct`, `cdr_pct`, `heal_pct`, `max_health`) with:
```
"tenacity_pct"     → CharacterStats.Tenacity
"control_pct"      → CharacterStats.ControlPower
```
Additive across slots, clamped at the caps above in `Recalculate()`.

---

## 5. Enchant / modifier layer

Keep it two-layered and resist the urge to make enchants "+more stats."

- **Base stats (`stat_bonus`, rarity-scaled)** = the *throughput* dial. Higher rarity rolls bigger channel values within the Section 2.3 band. This is the drop-chase.
- **Enchants (new `modifiers JSON` column on `items`)** = *behavior* changes anchored to the variant system. Not raw numbers — new interactions.

**Rule: stats scale power, enchants change how you play.**

### 5.1 Schema (minimal change)

Add one nullable column to `items`:
```sql
ALTER TABLE items ADD COLUMN modifiers JSON NULL;
-- shape: [{ "id": "far_zone_hot" }, ...]   (just the socketed attunement ids)
```
Read at equip alongside `stat_bonus`; apply as passive hooks into events the pipeline already fires (`onHealApplied`, `onKilledBy`, `onDamagedBy`, the named-key DR dict, variant resolution). One enchant slot per piece to start; the column already supports an array for a Phase-2 multi-slot expansion. **No random affix rolls yet** — a fixed, hand-authored pool.

### 5.2 Starter enchant pool (~12, hand-authored)

Terminology note: the existing `CharacterStats` header already calls these **attunements** ("the attunements socketed into it"). Using that name for consistency.

| id | Effect | Hooks into |
|---|---|---|
| `far_zone_hot` | Far-zone shields also grant 8% of absorb as a 3s HoT | variant resolve + `onHealApplied` |
| `close_zone_curse` | Close-zone releases apply 1 Cursed stack | variant resolve → `StatusEffectManager` |
| `charge_slow` | Full-charge casts apply Slow (1.5s) | chargeable release |
| `killstreak_free_cast` | First cast after a kill costs no cooldown | `onKilledBy` (self) |
| `overheal_shield_plus` | +15% to the overheal→shield spill rate | heal path |
| `weakened_hunter` | +15% damage to Weakened targets (stacks with the ×1.25 amp) | `TakeDamage` amp layer |
| `threat_siphon` | Each Threat stack also grants +2% ControlPower (Ironclad) | Threat Protocol |
| `wisp_split` | Spirit Wisps have a 20% chance to spawn a second orb | Cleric wisp behaviour |
| `phase_overflow` | Phase Charge triggers at 5 casts instead of 6 (Arcanist) | Phase Charge meter |
| `stagger_to_stun` | Your Stagger has a 25% chance to become a 1.5s Stun | Stagger apply |
| `bulwark_reflect` | While shielded, reflect 10% of absorbed damage | shield layer |
| `tenacity_on_low` | +25% Tenacity while below 35% HP | `onHealthChanged` |

Each attunement's magnitudes (the 8%, the 1.5s, the 25% chance) are **not** hardcoded — they live on an `AttunementDef` ScriptableObject, one asset per attunement, so every one is Inspector-editable (Section 8).

---

## 6. Implementation order (lowest risk → highest payoff)

1. **Cap fixes** (Section 2) — total-DR clamp at 0.85, lower gear CDR cap to 0.40. One-to-two line changes in `Health.TakeDamage` / `CharacterStats.Recalculate`. Highest leverage, smallest diff.
2. **Overheal→shield spill** (Section 3.1) — makes `HealMultiplier` a real target.
3. **Wave scaling + aggro cap** (07-06 audit) — the prerequisite that gives healing a job. Sequence before any rare-gear seeding.
4. **CC axis** (Section 4) — `Stun` status + `Tenacity`/`ControlPower` channels + two new `stat_bonus` keys.
5. **Enchant layer** (Section 5) — `modifiers JSON` column, equip-time application, starter pool of 12.
6. **Rarity roll tables** — derived from the Section 2.3 power band, not invented per item.

Run **netcode-reviewer** on steps 1 and 4 (status/CC and damage-pipeline changes are server-authoritative), **content-generator** on steps 5–6 (schema-conforming enchant + rarity data), and **playtest-analyst** to design the telemetry that verifies the 2.0–2.5× power band holds once the arena scene is live.

---

## 7. Open decisions for the owner

- **Power-band width:** confirm the 2.0–2.5× base→BiS target, or set your own — everything else derives from it.
- **DR fix A vs B:** total-DR clamp (minimal change) vs shared additive pool (cleaner, bigger refactor). Recommend A.
- **Stance-tank immunity:** is the ~10× EHP window intended as a cooldown payoff, or should the clamp reel it in? Design call, not a bug.

---

## 8. HARD REQUIREMENT — everything above is Unity-editable, zero recompiles

**Rule: no balance number lives in a system script.** Every tunable is a serialized field on a ScriptableObject asset, edited in the Inspector, live-tweakable in Play mode. This follows the project's existing `FeelConfig` / `ZoneConfig` pattern and the "no magic numbers" pillar.

### 8.1 Two config assets carry the whole balance surface

**`CombatBalanceConfig`** (created — `Assets/Game/Combat/Scripts/CombatBalanceConfig.cs`) — Resources-loaded singleton, server-safe, same shape as `FeelConfig`. Holds every scalar from this doc:

| Doc section | Field on `CombatBalanceConfig` |
|---|---|
| Gear DR cap (2.1) | `gearDrCap` (0.80) |
| Total-DR clamp (2.1) | `totalDrClamp` (0.85) — via `ClampTotalDR()` |
| Gear CDR cap (2.2) | `gearCdrCap` (0.40) |
| Overdrive CDR cap (2.2) | `overdriveCdrCap` (0.60) |
| Overheal→shield (3.1) | `overhealToShieldRate` (0.25), `overhealShieldCap` (80) |
| Stun DR (4.1) | `stunBaseDuration`, `stunRepeatFalloff`, `stunFalloffWindow`, `stunImmunityThreshold`, `stunImmunityDuration` |
| Tenacity / Control caps (4.2) | `tenacityCap` (0.60), `controlPowerCap` (0.50) |
| CC duration formula (4.2) | `CCDuration(base, tenacity, control)` static helper |
| Wave scaling (6.3) | `hpScalePerWave`, `damageScalePerWave`, `globalAggroCapPerPlayer` |
| Power band (2.3) | `targetPowerBandMin/Max` (reference for tooling) |
| Attunement sockets (5) | `maxAttunementSlotsPerItem` |

**`AttunementDef`** (to build — `Assets/Game/Items/Scripts/AttunementDef.cs`) — one ScriptableObject asset per enchant in Section 5.2, so each attunement's *magnitudes* are Inspector-editable, not hardcoded. Proposed fields: `id`, `displayName`, `description`, `effectType` (enum), `magnitude`, `chance`, `duration`, `radius`, `targetVariantZone` (which zone tier it empowers). The runtime hook layer reads these; designers add/retune attunements without touching code.

### 8.2 Wiring step — replace the hardcoded clamps (editor compile needed)

`CharacterStats.cs` currently hardcodes the caps:
```
line 55:  Mathf.Clamp(CooldownReduction + _temporaryCDR, 0f, 0.6f)
line 119: DamageReduction   = Mathf.Clamp(pctDR,  0f, 0.8f)
line 121: CooldownReduction = Mathf.Clamp(pctCdr + _masteryCdrPct, 0f, 0.6f)
```
Replace the literals with `CombatBalanceConfig.Instance` reads (with the current literal as the fallback default when no asset is loaded, exactly like FeelConfig's systems do). Same treatment for the DR literal in `Health.TakeDamage` (add the `ClampTotalDR()` call at the end of the DR math) and the wave literals in `WaveSpawner`.

### 8.3 What is NOT a Unity asset (and why that's correct)

- **Per-item gear stat values** (`stat_bonus` JSON) and **which attunement is socketed on which item** (`modifiers` JSON) live in **MySQL**, editable via the dashboard / SQL — because they're per-drop content the live server owns, not global tuning. Unity holds the *rules and magnitudes*; the DB holds *which item rolled what*. This split is deliberate: designers tune combat feel in Unity; loot content is authored/seeded server-side.
- If per-item live editing without SQL is wanted later, that's a dashboard feature, not a Unity one.

### 8.4 Editor steps for the owner (CLI can't do these)

1. Open the project once so Unity compiles `CombatBalanceConfig.cs` (confirm no errors — this batch is review-only, not build-verified).
2. `Assets → Create → Crossworlds → CombatBalanceConfig`. Put the asset in any `Resources/` folder, named **`CombatBalanceConfig`** (the singleton loads it by that name).
3. Values ship pre-filled with the doc's recommended numbers — tweak freely in the Inspector, including during Play mode.
4. After the 8.2 wiring change lands, delete-and-reimport is not needed; just confirm the systems read the asset (temporarily change `totalDrClamp` and watch damage-through change in a test fight).
