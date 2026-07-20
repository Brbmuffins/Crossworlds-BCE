# Crossworlds BCE — Combat Profile (Current, July 2026)

> **What this is:** A single-source description of every layer in the live combat system — damage resolution, status effects, class kits, passives, gear/mastery bonuses, and the new variant-zone layering added today. Use this as the authoritative v3 reference replacing the v2 proposal.

---

## 1. Damage Resolution Pipeline

Every `TakeDamage(amount, source)` call passes through these layers in order — each reduces or redirects the raw number before any HP is touched.

```
Raw damage (from ability, projectile, DoT tick, etc.)
  │
  ├─ [1] Invulnerability gate — dodge i-frames → discard entirely
  │
  ├─ [2] Weakened amplifier — if target has Weakened status → ×1.25
  │
  ├─ [3] Ability DR — named-key dictionary, multiplicative
  │       Sources: "siege_mode", "threat_protocol", "resist_<type>"
  │       eff = 1 − Π(1 − fᵢ)  so stacked sources never silently cancel
  │
  ├─ [4] Gear DR — CharacterStats.DamageReduction (0–0.8), multiplicative
  │       Applied after ability DR, independent key
  │
  ├─ [5] Damage Redirect (Transfer Protocol / Soul Bond)
  │       fraction of post-DR amount → redirected to Cleric's Health
  │       re-entrancy guard prevents mutual redirect loops
  │
  ├─ [6] Kinetic Reversal absorption window
  │       if Ironclad absorption is active → accumulate, skip HP entirely
  │       accumulated amount returned as CounterBlow burst (up to 60 dmg)
  │
  ├─ [7] Shield buffer — depleted first before HP
  │       GrowShield (Adaptive / Sacred Aegis) can grow it on hit up to 80
  │
  └─ [8] HP deduction → onHealthChanged / onDamageTaken / onDamagedBy events
```

**Player death** → downed state (not destroy); auto-revive timer (10 s default); respawn invulnerability (2 s). **Enemy death** → `onDeath` + `onKilledBy` events → DropTable rolls → loot spawned.

---

## 2. Status Effects

All effects live in `StatusEffectManager` alongside `Health`. Refresh-or-add semantics: same type already present takes the longer duration and stronger magnitude.

| Type | Mechanic | Notes |
|---|---|---|
| `Slow` | Reduces NavMesh/movement speed by `value` fraction | Max-of-all-stacks wins |
| `Stagger` | Interrupts enemy heavy attack windup | `IsStaggered` polled by `EnemyHeavyAttack` |
| `Silenced` | Prevents ability use | `IsSilenced` polled by `AbilityCaster` |
| `Cursed` | Damage over time (`value` DPS, ticks every 0.5 s) | Stacks sum before single tick — prevents Threat Protocol spam |
| `Weakened` | +25% incoming damage | Applied by Collapsing Void; amplifier sits in `TakeDamage` |
| `Bound` | Root/leash (positional control) | Excluded from Dark Harvest detonation intentionally |

**Debuff detonation** (`IsDebuff` = Slow, Stagger, Silenced, Cursed, Weakened — not Bound):
- `CountDebuffStacks()` — read by Shadowblade before Dark Harvest
- `ConsumeDebuffStacks()` — called on Dark Harvest, removes all, returns count → ×20 dmg per stack

**Purge Protocol** — `RemoveAll()` clears everything instantly. Called by: Dispel (Cleric), respawn, Snapshot rollback.

---

## 3. Gear & Mastery Stat Channels

`CharacterStats` aggregates every `StatModifier` from `Equipment` and mastery overlays, then pushes the results into `Health` and makes them available to `AbilityCaster` / movement code. No leveling — all power comes from gear.

| Channel | Cap | Consumer |
|---|---|---|
| `MaxHealthBonus` (flat HP from gear + mastery%) | — | `Health.SetGearMaxHealthBonus` |
| `DamageMultiplier` (1 + pct gear + mastery + flask) | — | `AbilityCaster` before dealing damage |
| `DamageReduction` (pct, additive gear sources) | 0.8 | `Health.SetGearDamageReduction` (layer 4 above) |
| `MoveSpeedMultiplier` | — | `PlayerMovement` |
| `CooldownReduction` (pct gear + mastery) | 0.6 | `AbilityCaster` (scales all timers) |
| `HealMultiplier` | — | All heal paths in `AbilityCaster` |
| `TemporaryCDR` (Overdrive ability) | ±0.6 | Additive on top of gear CDR |
| `TemporaryDmgPct` (consumable flasks) | −1 → +2 | Folded into DamageMultiplier on `Recalculate()` |

---

## 4. Class Kits & Passives

### 4.1 Warden (index 0) — "Construct Commander"
**Passive: Overengineered** — every 4 s, each active deployable within 12 u gains +1 output stack (max 5, +8% dmg/heal per stack). Stacks persist on the deployable; cleared on destruction.

**Kit:**
- Runic Snare [8] — proximity burst rune trap (also Shadowblade)
- Battle Hymn [9] — team CDR aura, instant self-cast
- Spirit Redirect [10] — redirect active Runic Sentinel onto focus target
- Mend [11] — single-target direct heal + debuff cleanse
- Conjurer's Surge [12] *(Ultimate, 45 s)* — all constructs activate at full power simultaneously
- Thorn Volley [44] — cone, variant system (single thorn / volley / briar storm)
- Earth Surge [45] — circle, variant (tremor / quake / fissure)
- Vine Grasp [46] — circle, variant (root / stranglehold / forest prison)

---

### 4.2 Ironclad (index 1) — "Threat Anchor"
**Passive: Threat Protocol** — each hit taken generates 1 Threat stack. At 5 stacks: forces aggro onto self within 12 u radius AND grants 20% DR for 6 s (stored as "threat_protocol" in the named-key DR system). Death clears stacks. External `AddStacks(n)` allows abilities (Shieldwall Charge, Stalwart Stance) to accelerate buildup.

**Kit:**
- Counter Blow [13] — 3 s absorption window (Kinetic Reversal); release as 60-dmg cone
- Gravity Slam [14] — pull all enemies in radius to anchor point
- Shieldwall Charge [15] — rect charge, 25 dmg, stagger + 3 Threat stacks
- Stalwart Stance [16] — stationary: 40% DR + 3× Threat generation for 6 s
- Rune Chain [17] — leash 1 enemy 8 u for 5 s; absorb 15% of their attacks on allies
- Iron Rampart [18] *(Ultimate, 50 s)* — 8 u-wide stone wall, blocks projectiles 10 s
- Hammer Strike [47] — rect, chargeable, variant (quick blow / heavy slam / seismic slam)
- War Cry [48] — circle aura, variant (shout heal / rally shield / primal roar)
- Juggernaut Rush [49] — rect charge, chargeable, variant (dash / bull rush / wrecking ball)

---

### 4.3 Shadowblade (index 2) — "Debuff Detonator"
**Passive: Bounty System** — killing any enemy CDR's all abilities by 2 s; elite kill = 5 s. Hooks into `Health.onKilledBy` on every enemy at scene start (and on new wave spawns via `HookEnemy`).

**Kit:**
- Runic Snare [8] — proximity burst rune trap (shared with Warden)
- Shadow Veil [29] — 4 s full invisibility; breaking with Mind Spike = +50% dmg
- Silence Ward [30] — 5 s AoE enemy silence
- Dark Harvest [31] *(Ultimate, 40 s)* — consume all detonatable debuffs in range: 20 dmg/stack
- Dark Mark [32] — cursed burst, sets up Dark Harvest combo
- Fan of Blades [33] — cone, chargeable (12 / 30 dmg)
- Blade Flurry [50] — cone, chargeable, variant (slash / flurry ×3 / maelstrom ×5)
- Poison Cloud [51] — circle, variant (mist / miasma / death fog)
- Death Strike [52] — circle, variant (stab / deep cut / assassination)

---

### 4.4 Cleric (index 3) — "Field Medic"
**Passive: Triage Loop** — each point of healing dealt to an ally returns 8% back to the Cleric's own HP. Hooks into `Health.onHealApplied` on all tagged ally objects; re-hook via `HookAllAllies()` when players join.

**Kit:**
- Soul Bond [23] — tether ally: their damage reroutes to Cleric for 5 s (Transfer Protocol)
- Spirit Wisps [24] — mobile healing orbs, drift toward ally, chip enemies en-route
- Divine Spark [25] — revive downed ally at 30% HP OR 60 burst dmg to undead enemies
- Sacred Aegis [26] — growing shield (20 → 80 absorb as target takes hits over 8 s)
- Dispel [27] — instant cleanse all debuffs from target ally
- Temporal Grace [28] *(Ultimate, 60 s)* — rewind entire team 5 s: HP, position, debuffs via SnapshotSystem
- Holy Bolt [53] — cone heal, variant (flash / radiance / divine ray)
- Divine Shield [54] — circle shield aura, variant (shelter / bastion / cathedral)
- Smite [55] — cone holy dmg, chargeable, variant (strike / judgement / wrath)
- Healing Cone [35] — cone, **variant sweet-spot** (HPS burst close / HoT mid / Shield far)
- Mending Beam [36] — rect beam, **variant sweet-spot** (same layers as cone)

---

### 4.5 Arcanist (index 4) — "Zone Controller"
**Passive: Phase Charge** — every ability cast (except Phase Shift) charges a meter. At 6 charges the next offensive ability deals +40% damage and resets the meter. `ConsumeBonusIfCharged()` called by `AbilityCaster` before applying damage.

**Kit:**
- Arcane Step [19] — blink 10 u; lands 4 pulse-damage bursts at destination (10 dmg each)
- Void Maw [20] — 8 u pull zone; 4 × 20 dmg pulses over 4 s
- Forked Lightning [21] — chain lightning, 4 targets (30/25/20/15)
- Collapsing Void [22] *(Ultimate, 50 s)* — 12 u pull, 60 AoE + applies Weakened window for Dark Harvest combos
- Ether Lance [34] — rect piercing beam, chargeable (15 / 40 dmg)
- Fireball [41] — cone, chargeable, variant (quick shot / triple burst / inferno nova)
- Chain Lightning [42] — circle, chargeable, variant (arc / chain / thunderstorm)
- Frost Nova [43] — circle control, variant (chill / freeze / blizzard)
- Conflagration Cone [37] — cone, **variant sweet-spot** (burst close / burn DoT mid / Slow+Weakened far)
- Ember Beam [38] — rect, **variant sweet-spot** (same layers as cone)
- Ice Spikes [39] — cone, ground-eruption, Slow
- Meteor Shower [40] — large circle AoE, delayed bombardment, 30 s cooldown

---

## 5. Variant Zone Layering (NEW — Today)

The biggest addition to the ability system. Any `AbilityDef` can now carry an `AbilityVariant[]` array. Each variant is a **named zone** with its own heal/HoT/shield/damage/status payload, indicator tint, and optional VFX overrides.

**Zone selection:** two modes, switchable via `PlayerPrefs("VariantScrollMode")`:
- **Cursor distance** (default) — aim fraction maps linearly to zone index; near zones activate close-in, far zones at max range.
- **Scroll wheel** — player manually steps through zones while the indicator is held.

**Resolution:** on cast commit, `_activeVariantIndex` is snapshotted. `ResolveVariantCast()` reads `ability.variants[clampedIdx]` and processes that zone's payload independently of the base `AbilityDef` damage field. This means a single ability definition expresses a full spectrum of outcomes — the same key press can deliver burst heal, a HoT, or a shield depending purely on positioning.

**Backfill:** `BackfillVariantDefaults()` runs in `Awake` to patch null fields inside each variant slot (inheriting targetTag, VFX, etc. from the parent `AbilityDef`). `BackfillVariantVFX()` separately ensures variant `hitVFX` entries are populated for all registered prefabs.

**Cross-class use cases in the spellbook today:**
- Healing Cone [35] / Mending Beam [36] — Cleric: Close = instant burst heal, Mid = HoT ticks, Far = shield
- Conflagration Cone [37] / Ember Beam [38] — Arcanist: Close = burst fire dmg, Mid = Cursed DoT, Far = Slow + Weakened
- New class-expansion spells (Fireball, Chain Lightning, Frost Nova, Thorn Volley, Earth Surge, Vine Grasp, Hammer Strike, War Cry, Juggernaut Rush, Blade Flurry, Poison Cloud, Death Strike, Holy Bolt, Divine Shield, Smite) — all carry variant stubs for 3 tiers of outcome

---

## 6. Ability Mechanics Glossary

| Mechanic | How it works |
|---|---|
| **Chargeable** | Hold to charge; `maxChargeTime` scales damage from `damage` → `maxChargeDamage`, indicator grows to `maxChargeSizeMultiplier` |
| **Chain Lightning** | Hits `chainTargets` enemies; each jump loses `chainDamageFalloff` dmg |
| **Pull / Singularity** | `pullRadius` → `SingularityBehaviour`; enemies pulled over `pullDuration` then pulse dmg fires |
| **Pulse Damage** | `pulseCount` bursts at `pulseInterval`; radius is `pulseRadius` (default = half indicator); `pulseDamage` per hit |
| **Deployable** | `deployablePrefab` spawned by `DeployableManager`; stacked by `PassiveOverengineered`; registered per owner |
| **Cast Time** | `castTime` seconds after aim-commit before spell fires; movement beyond 0.5 m cancels without starting cooldown |
| **Phase Shift** (Arcanist) | Only ability that does NOT charge Phase Charge meter |

---

## 7. System Interactions & Cross-Class Combos

| Combo | Classes | Mechanic |
|---|---|---|
| Dark Mark → Silence Ward → Dark Harvest | Shadowblade | Stack Cursed + Silenced on a group, then detonate for 40+ dmg/enemy |
| Collapsing Void → Dark Harvest | Arcanist + Shadowblade | Void applies Weakened; Harvest detonates Weakened stacks too |
| Soul Bond → tank absorbs for ally | Cleric + Ironclad | Transfer Protocol: Cleric takes a fraction of the Ironclad's redirected damage; Triage Loop partially heals Cleric back |
| Stalwart Stance + Threat Protocol | Ironclad | 3× Threat stack rate during stance means single-stance activation almost guaranteed in one fight |
| Phase Charge → Collapsing Void | Arcanist | 6-cast buildup → +40% on the 60-dmg ultimate = 84 base before DR/gear |
| Bounty System → Conjurer's Surge | Shadowblade + Warden (co-op) | Kill CDR resets chains for Shadowblade while Warden surges all constructs simultaneously |

---

## 8. What's Live vs. Stubbed

| Layer | Status |
|---|---|
| `Health` pipeline (all 8 layers above) | ✅ Implemented |
| `StatusEffectManager` (all 6 types) | ✅ Implemented |
| `CharacterStats` gear + mastery channels | ✅ Implemented |
| All 5 class passives | ✅ Implemented |
| Base spellbook (indices 0–31) | ✅ Defined in AbilityCaster |
| Variant zone layering (indices 35–55) | ✅ Defined, resolution logic present |
| Variant `AbilityVariant[]` payloads populated | 🔶 Stubs exist; actual heal/dmg/status values need Inspector assignment |
| `PhaseRelayDeployable`, `ShadowRelayDeployable`, `RestorationBeacon`, `BastionNode` | ✅ Scripts exist, prefabs need Inspector wiring |
| DeployableManager stack system | ✅ Implemented |
| SnapshotSystem (Temporal Grace backing) | ✅ Implemented (10 × 0.5 s ring buffer) |
| Temporal Grace actually calling Rollback | 🔶 Needs AbilityCaster hook |
| Arena scene for combat to run in | ❌ Scene missing — no end-to-end loop yet |
