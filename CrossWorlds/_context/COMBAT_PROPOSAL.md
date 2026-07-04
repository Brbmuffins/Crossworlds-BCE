# Crossworlds BCE — Combat Design Proposal v2

> ⚠️ **STATUS: PENDING IDEA — NOT IMPLEMENTED**
> Nothing in this document exists in code yet. This is a design proposal
> for discussion and playtesting. Do not merge any of this into live systems
> until the mechanic has been validated in a playtest session.
> Source of truth for current combat: `COMBAT.md` and `COMBAT_ATLAS.md`

Last updated: 2026-06-29  
Author: CrossWorlds / Claude

---

## Design Intent

The current combat system is burst-oriented. Every class plays a rotation: build resource, detonate, reset. That works, but it means players are only ever asking "what cooldowns are ready?" This proposal adds a **persistent state layer** — DoTs, HoTs, and state transitions — so players also ask "what states are active?" on enemies and allies simultaneously.

The goal is not complexity for its own sake. It's the feeling of playing an instrument with more strings. The extra strings are always there, always visible, and using them well is what separates a competent player from a good one.

**Rules this proposal follows:**
- Same 32 ability slots. No new abilities.
- No ability cooldowns or base damages change.
- All new mechanics are consequences of existing ability hits, not new buttons.
- Every new state is visible on StatusEffectHUD. No hidden tracking.
- No single class can trigger a state transition alone (except one Shadowblade-specific case by design).

---

## New State Types

### DoT States (applied to enemies)

| State | Damage | Duration | Stacks | Notes |
|---|---|---|---|---|
| **Void Rot** | 4 dmg/s | 6s | up to 3× | Each stack also increases void ability damage received by 2%. 3 stacks = +6% void damage taken. |
| **Burning** | 6 dmg/s | 5s | 1× | Refreshes on reapply, does not stack. If target is also Weakened: +50% tick damage and +2s duration (see Combustion transition). |
| **Void Leak** | 3 dmg/s | 8s | 1× | On death with Void Leak active, spreads 1 Void Rot stack to enemies within 3u. Contagion mechanic. |
| **Hemorrhage** | 5 dmg/s | 3s | 1× | Applied only by Dark Mark. Does not stack with other DoTs but stacks alongside Cursed. Single-target DoT setup tool. |
| **Cursed** | 8 dmg/s | 4s | stackable | Unchanged. Still Shadowblade's primary stack currency. Dark Harvest now also consumes Void Rot stacks if present. |

### HoT States (applied to allies)

| State | Healing | Duration | Source | Notes |
|---|---|---|---|---|
| **Renewal** | +10 HP/s | 5s | Mending Circle on-exit | Applied when an ally leaves the zone, not on entry. The zone's afterglow follows them. |
| **Triage** | +6 HP/s | 4s | Spirit Wisps on-land | Replaces the Wisps' instant heal. Same total healing over 4s. Cleric's Triage Loop passive procs on each tick. |
| **Sacred Regeneration** | +8–15 HP/s | 2–6s | Sacred Aegis on break | Duration scales with how much the Aegis absorbed before breaking. Min 2s at 20 absorb, max 6s at 80 absorb. |

### Transition States

Transitions are emergent states produced when two specific conditions are true on the same target simultaneously. They cannot be deliberately aimed at — they require two ability interactions landing in the right order.

| Condition A | Condition B | Transition | Effect |
|---|---|---|---|
| Burning (active) | Slow (active) | **Scorched** | Slow upgrades to a 1.5s root. Removed by Mend or Dispel. |
| Void Rot ×3 (active) | Binding Wave hit | **Void Collapse** | 15 instant damage + radiates 1 Void Rot stack to all enemies within 3u. |
| Cursed ×4+ (active) | Dark Harvest | **Withered** | After detonation: move speed −40% for 4s. Shadowblade decision: 3 stacks = max damage, 4+ stacks = Withered CC. |
| Weakened + Burning | — | **Combustion** | Burning already gets +50% tick and +2s from being alongside Weakened. Treat as a named state for visual clarity. |
| Renewal + Sacred Aegis (both active, same ally) | — | **Sanctified** | Incoming damage drains the HoT first before the shield absorbs. Effectively extends shield duration. Cleric-exclusive. |
| Void Leak (on enemy) + Silence Ward zone (enemy inside) | — | **Void Silence** | Cursed tick damage inside Silence Ward is doubled for Void Leak enemies. Zone becomes high-damage if pre-seeded. |
| Shieldwall Charge (passing through Void Rot enemy) | — | **Contamination Pass** | Ironclad spreads 1 Void Rot stack to every enemy passed through during the charge. AoE DoT seeding by accident. |

---

## Per-Class Mechanical Changes

### Warden — Construct Tactician

**Unchanged:** All abilities, cooldowns, Overengineered stacks, construct limits, deployable system.

**New mechanic — Runic Snare:**
When a Runic Snare detonates, the enemy gains **Structural Weakness** (6s): damage taken from constructs +15%. This creates a clear setup loop — place Snare on a priority target, trigger it, then use Spirit Redirect to focus all Sentinels on that target for the Structural Weakness window. Snare becomes a mark, not just a damage trap.

**New mechanic — Overengineered at max stacks:**
At 5 Overengineered stacks, active Sentinels also emit a void pulse every 4s applying 1 Void Rot stack to nearby enemies (3u radius). Low volume, but consistent. Warden's constructs become passive DoT applicators when the Warden has sustained presence. The pulse does not reset the Overengineered timer.

**New mechanic — Sentinel amplification:**
Runic Sentinel shots deal bonus void damage equal to (Void Rot stacks on target × 3). At 3 stacks, each Sentinel hit deals +9 void damage. During Conjurer's Surge overload with Spirit Redirect focused on a 3-stack target, this is the game's highest sustained single-target DPS window — but requires an Arcanist to pre-build the Void Rot stacks.

**New mechanic — Battle Hymn interaction:**
During the 8s CDR window from Battle Hymn, allies who cast abilities apply **Attuned** to themselves (1s debuff that's actually a CDR proc trigger). On expiry, Attuned reduces the Warden's next construct ability cooldown by 1s per proc consumed. Passive cross-class CDR return — the more the team casts during Hymn, the faster Warden's next Sentinel comes back.

**Mend (ability 11):** Warden's Mend already clears 1 debuff. Specifically it can now break the Scorched transition (the Burning+Slow root) — treated as a structural interference, different flavor from Cleric's Dispel. This gives the Warden a niche cleanse role in mixed groups.

---

### Ironclad — Iron Vanguard

**Unchanged:** All abilities, cooldowns, Threat Protocol passive, tank-and-aggro loop.

**New mechanic — Counter Blow Iron Aura:**
When Counter Blow releases at maximum absorption (3+ hits absorbed), the burst is followed by 2s of **Iron Aura**: during Iron Aura, damage dealt to nearby allies (6u) is partially redirected to Ironclad (15% of each hit). This is a second, shorter, passive Soul Bond window. The Ironclad who absorbs and releases correctly also becomes a brief team umbrella without Cleric involvement.

**New mechanic — Stalwart Stance Resolve:**
Stalwart Stance now also applies **Resolve** to Ironclad (active for duration of Stance): next CC received is reduced by 50% duration. This is not a HoT but follows the same "visible timer" pattern — Ironclad tracks whether Resolve is up before choosing to hold position. Encourages actually standing still during Stance rather than retreating.

**New mechanic — Rune Chain Tension:**
While an enemy is leashed by Rune Chain, they accumulate Tension passively (1 stack per 1.5s). When the leash expires or is broken, Tension releases: AoE Stagger on the leashed enemy and all within 2u. This is free crowd control from an ability that's primarily used for setup — leash the target, let the tension build, the snap does work automatically.

**New mechanic — Iron Rampart Exposed:**
Enemies that impact Iron Rampart (blocked projectiles, enemies that run into the wall face) gain **Exposed** for 5s: incoming physical damage +15%. The wall now has an offensive function — drive melee enemies into it via Gravity Slam, and they get Exposed for the team's follow-up.

**Contamination Pass (from Transitions table):** Shieldwall Charge passing through a Void Rot-stacked enemy spreads 1 stack to all enemies in the charge path. The Ironclad becomes an accidental DoT spreader in dense packs. Pairs with Arcanist pre-stacking a single target.

---

### Arcanist — Void Mage

**Unchanged:** All abilities, cooldowns, Phase Charge passive, blink mechanic.

**New mechanic — Void Bolt applies Void Rot:**
Every Void Bolt hit (charged or uncharged) applies 1 Void Rot stack to the target. Charged Void Bolt does not apply additional stacks — it still does more burst damage. This gives Void Bolt a consistent secondary purpose: stack builder. The Arcanist who fires 3 Void Bolts has a fully-stacked target ready for transitions, Sentinel amplification, or Dark Harvest consumption.

**New mechanic — Ember Surge split behavior:**
Uncharged Ember Surge now applies Burning instead of its burst damage (burst still fires at full charge). This is the key Arcanist decision: charge for damage, or tap for DoT. Combustion (Weakened + Burning) is only accessible via uncharged — so the Arcanist who wants the sustained DoT window deliberately sacrifices the burst shot. High-skill optimization.

**New mechanic — Arcane Step Void Leak:**
The existing echo that detonates 1s after blink (already exists, deals 20 dmg) now also applies Void Leak to all enemies in the explosion radius. No damage change. The echo already fires — the Void Leak is a side effect. Arcanist blinking through or near groups seeds Void Leak passively.

**New mechanic — Forked Lightning DoT propagation:**
If the first chain target has Void Rot × 3, the lightning chain spreads 1 Void Rot stack to each subsequent chain target (up to 4 enemies). The chain becomes a DoT propagator in already-primed groups. Requires setup (Void Bolt first, then Forked Lightning) but the payoff is 4 enemies all carrying Void Rot from one cast.

**New mechanic — Phase Charge DoT timing:**
If the 6th cast (the Phase Charge empowered hit) applies Burning, Combustion triggers immediately without requiring Weakened to already be present — this is a one-time Phase Charge exclusive proc. Rewards Arcanist for intentionally saving their Phase Charge cast for Ember Surge (uncharged) rather than defaulting to Collapsing Void.

**Arcane Ward void burst:** When Arcane Ward's 50 absorb is fully consumed and breaks, it releases a void pulse applying 1 Void Rot stack to enemies within 4u. Self-defense becomes stack application.

---

### Cleric — Soul Warden

**Unchanged:** All abilities, cooldowns, Soul Bond redirect, Temporal Grace rollback.

**New mechanic — Mending Circle Renewal:**
Allies who leave a Mending Circle zone carry **Renewal** (10 HP/s, 5s). This is applied on-exit. Allies standing inside still receive the existing zone pulses. The change means the Cleric's zone has reach beyond its radius — players who pass through take something with them.

**New mechanic — Spirit Wisps become Triage:**
Wisps now apply **Triage** (6 HP/s, 4s) on landing instead of an instant heal. Total healing is equivalent. The Cleric now sees active Triage durations on allies and decides when to re-cast versus trusting the existing tick. Triage Loop passive procs on each Triage tick (not just direct heals), giving the Cleric a sustained self-recovery stream proportional to how many allies have active Triage.

**New mechanic — Sacred Aegis breaks into Sacred Regeneration:**
When Sacred Aegis is fully consumed by damage, it converts to **Sacred Regeneration** (8–15 HP/s, 2–6s). Duration scales with total absorb before break: 20 absorb = 2s, 80 absorb = 6s. This rewards proactive shielding — the Cleric who casts Aegis before damage arrives and lets it charge gets a 6s HoT. The Cleric who panic-casts it into a hit gets 2s.

**Sanctified transition:** Renewal + Sacred Aegis active simultaneously on the same ally = Sanctified. Incoming damage drains the HoT first before the shield absorbs. The Cleric's two sustained tools stack into a multiplied effective buffer. Purely emergent — no new cast required.

**Triage Loop passive refinement:** Currently returns 8% of each heal instantly. Proposed split: 5% instant + 3% as a 4s personal Triage HoT on the Cleric. The Cleric carries their own Triage HoT that refreshes with every heal applied. Always self-sustaining as long as allies need healing.

**Dispel change:** Dispel now targets the most dangerous single debuff on the ally (worst by damage/duration), not all debuffs simultaneously. The existing Mend already removes 1 debuff — Dispel becomes the targeted nuclear option against high-stack or transition states (Scorched, Void Silence, etc.). This creates a real choice between Mend (surgical) and Dispel (worst-of-stack).

---

### Shadowblade — Void Infiltrator

**Unchanged:** All abilities, cooldowns, Corruption passive stack-per-hit, Bounty System CDR on kill.

**New mechanic — Dark Mark gains Hemorrhage:**
Dark Mark (the E ability from WraithAbilities) now also applies **Hemorrhage** (5 dmg/s, 3s) in addition to its existing Weakened + Cursed. Hemorrhage is a single-target exclusive DoT — it doesn't spread, doesn't stack with the AoE Corruption, but it does stack alongside Cursed ticks. Dark Mark becomes a meaningful single-target DoT setup tool on priority enemies, not just a Weakened applicator.

**New mechanic — Shadow Veil exit Void Rot:**
The existing Shadow Veil exit already applies Cursed to all nearby enemies via Corruption. Now it also applies 1 Void Rot stack to all nearby enemies. The stealth exit becomes a dual-DoT burst — Cursed stacks for Harvest setup, Void Rot for void ability amplification and potential Sentinel bonuses.

**New mechanic — Bounty System kill radiation:**
On kill, Corruption passive already gives −2s CDR. Now it also applies 1 Cursed stack to the nearest 2 enemies (within 8u). Kill radiation. This keeps the Shadowblade generating stacks in wave-clear content, not just single-target encounters. In arena waves, every kill seeds the next target.

**New mechanic — Withered at 4+ stacks (from Transitions table):**
Dark Harvest at 4+ Cursed stacks produces Withered (−40% move speed, 4s) on the target after detonation. The Shadowblade now has a real decision at each Harvest: fire at 3 stacks for maximum damage (60 from Harvest + Void Rot consumed) or hold to 4 stacks for Withered CC at a slight burst reduction. Against bosses at phase transitions, Withered is almost always worth more.

**New mechanic — Shattered on Weakened Harvest:**
If the target is Weakened when Dark Harvest fires, survivors (non-lethal Harvest) gain **Shattered** (3s): next attack from any player deals +25% damage to this target. The whole team benefits from a brief finish window. Encourages the Arcanist or Cleric to confirm the kill or maximize the final shot.

---

## Updated Cross-Class Synergies

These are new synergies that emerge from the state layer. Existing synergies from COMBAT_ATLAS.md remain unchanged and are not repeated here.

### Arcanist → Shadowblade (Stack Feeding)
Void Bolt builds Void Rot. Dark Harvest consumes Void Rot (now reads all debuff stacks, not just Cursed). At 3 Void Rot stacks on a target, an Arcanist has effectively given the Shadowblade 3 extra Dark Harvest stacks without the Shadowblade hitting the target once. In a Warden/Arcanist/Shadowblade trio, the stack economy on a leashed priority target is enormous.

### Ironclad → Arcanist (Setup for Transitions)
Rune Chain holds a target stationary for 5s. This is the easiest window to build Void Rot × 3 via three consecutive Void Bolts. Once at 3 stacks, the Ironclad's Binding Wave into the leashed target triggers Void Collapse: 15 instant AoE damage + Void Rot contamination to the cluster. The Ironclad setups a chain reaction the Arcanist detonates.

### Warden + Arcanist (Amplified Constructs)
Arcanist builds Void Rot × 3. Warden uses Spirit Redirect to focus all Sentinels on that target. Each Sentinel hit deals +9 void damage. During Conjurer's Surge overload: rapid-fire hits each doing +9 bonus damage on a focused target. This is the highest single-target DPS window in the game and requires two specific classes coordinating intentionally.

### Cleric + Any Class (Mending Circle Placement)
Renewal following Mending Circle means players actively want to pass through the zone on the way to their next position, not just stand in it. The Cleric's zone becomes a waypoint players route through. This emergently changes arena movement patterns — the team clusters around the zone's edge, not the center, to capture Renewal on the move.

### Arcanist + Shadowblade (Void Silence Zone)
Arcane Step echo applies Void Leak to a group. Shadowblade drops Silence Ward on the same group. Inside the Ward, Void Leak enemies receive doubled Cursed tick damage. Most sustained AoE damage window in the game when properly layered. Requires prior communication about positioning.

### Ironclad + Warden (Gravity Slam into Wall)
Iron Rampart blocks a corridor. Gravity Slam pulls enemies toward the Ironclad while the wall blocks their escape backward. Enemies in the pull are Exposed by hitting the wall. Warden's Runic Sentinels firing on Exposed enemies get no direct bonus (Exposed is physical damage — construct shots may not be physical, design decision needed here) but the cluster density amplifies any AoE ability landing on them.

### Shadowblade + Ironclad (Kill Radiation → Stance)
Shadowblade kills an enemy (Bounty System radiates Cursed to 2 nearby). One of those nearby enemies is the Ironclad's Rune Chain target. It now has a free Cursed stack the Shadowblade didn't need to generate. In sustained combat, this reduces how many Shadowblade casts are needed to reach Dark Harvest threshold on chained targets.

### Cleric + Ironclad (Sacred Shield into Iron Aura)
Cleric casts Sacred Aegis on Ironclad. Ironclad absorbs damage into Counter Blow while the Aegis charges. When Aegis breaks (Sacred Regeneration activates), Counter Blow simultaneously releases. The Ironclad has a burst hit + the Cleric's HoT ticking simultaneously — and if Sacred Regeneration + Renewal are both up on the Ironclad: Sanctified. Double-layered recovery post-burst.

### Negative Synergy — Cleric Mending Circle vs Shadowblade Stacks
Mending Circle zone may need to respect the Debuff/Buff distinction: it should not cleanse Cursed from enemies that walk through it. If the zone cleanses all status effects (including enemy debuffs), a poorly placed Mending Circle erases Shadowblade's Corruption stacks. **Design decision before implementation:** confirm Mending Circle cleanse applies to allies only, or introduce a separate "Cleanse" and "Dispel" distinction in the code.

---

## Boss Mechanic Hooks

Each boss should read at least one active state from the party and react to it. Full boss designs in PHASE3_DESIGN.md — these are additive hooks only.

### Null Architect (existing)

**Phase 2 — Tether Web:**
- Leashed players gain 1 Void Rot stack every 2s while tethered
- At Void Rot × 3 while leashed: tether briefly stuns player (1s) instead of just holding
- Mend or Dispel clears Void Rot from the leashed player — Cleric has an urgent, clear job
- This is the main design intent: the tether is now actively dangerous, not just inconvenient

**Final Surge — Resonance Penalty:**
- For every active DoT on the Null Architect when Final Surge begins, it gains +5% attack speed
- Maximum: 3 DoTs (Burning, Cursed, Void Rot) = +15% attack speed entering the hardest phase
- Players must choose: let DoTs fall off 10s before the phase transition, or push burst through and accept the speed penalty
- Forces a pre-Surge DoT cleanse window where the team stops applying damage. Real tension.

### Iron Warden (Phase 3 design)

**Magnetized Pulse:**
- Each active turret arm emits a pulse every 12s applying Magnetized (8 dmg/s, 3s) to all players
- Magnetized is not cleansed by Dispel — it's a boss mechanic type
- Cleared by standing inside Mending Circle for 1.5s (the zone grounds the charge)
- Cleric's zone placement becomes a safety infrastructure decision, not just a heal choice

**Arm Respawn Burning Immunity:**
- Respawned arms are immune to Burning for 6s after respawn
- Void Rot stacks from before destruction carry over on the arm — pre-stack before destroying
- Warden + Spirit Redirect during the Burning immunity window (Sentinels still fire normally) is the intended counter

**Core Threat Targeting:**
- Core targets player with lowest Threat stacks
- Ironclad in Stalwart Stance generates 3× Threat: Core locks onto them immediately
- While Core is targeting Ironclad: Counter Blow absorb applies as a team-wide partial redirect (15%)
- Rewards Ironclad for being the active target, not just the passive tank

### Void Herald (Phase 3 design)

**Void Mark Escalation:**
- Stealth phase applies Void Mark to all players: 3 Cursed stacks (boss-applied, not consumable by Dark Harvest)
- Every 2s in stealth: +1 Void Mark stack per player
- Silence Ward placed inside the stealth zone reveals the Herald for 2s
- Cleric's Dispel clears 3 Void Mark stacks from one ally — with 3+ players, Cleric can cover ~2 targets per stealth phase if they react immediately

**Dark Harvest Mirror:**
- When stealth ends: any player at 8+ Void Mark stacks takes instant 160 damage (20 × 8)
- Temporal Grace is the primary counter — roll back 5s before the final escalation tick
- Note: Temporal Grace also rolls back Cleric's Dispel casts made in the last 5s. If Cleric Dispelled twice, the rollback un-Dispels. Use Grace early or not at all.

**Void Leak Trap:**
- If the Herald has Void Leak active on it when stealth ends, the reveal detonation spreads 1 Cursed stack to nearby players from the Leak spread mechanic
- Arcanist must NOT apply Void Leak (via Arcane Step echo) to the Herald during stealth setup
- The Arcanist tracking their own DoT application is a skill requirement, not a mechanic the game enforces

---

## Implementation Notes (when this goes to code)

> Not for current sprint. Notes for whoever implements this.

**StatusEffect.cs:**
- Add new `StatusEffectType` enum values: `VoidRot`, `Burning`, `VoidLeak`, `Hemorrhage`, `Renewal`, `Triage`, `SacredRegeneration`, `Scorched`, `Withered`, `Combustion`, `Sanctified`, `VoidSilence`, `Magnetized`, `VoidMark`, `StructuralWeakness`, `Resolve`, `IronAura`
- Void Rot needs a stack count (max 3) with per-stack void damage amplification stored on the target's `CharacterStats`
- Transition states need a TransitionChecker that runs on StatusEffectManager.Update() — checks for combination conditions and fires the transition

**StatusEffectManager.cs:**
- Add `ConsumeByType(StatusEffectType)` for Dispel targeting the worst single debuff
- Expand `ConsumeDebuffStacks()` to count Void Rot stacks alongside Cursed for Dark Harvest
- Add `GetHighestDangerDebuff()` helper for new Dispel behavior
- Split `RemoveAll()` into `RemoveDebuffs()` and `RemoveBuffs()` — existing Mend/Dispel calls need updating

**AbilityCaster.cs:**
- Void Bolt: add `ApplyStatus(VoidRot, 1)` on hit
- Ember Surge: check charge level — uncharged = apply Burning, charged = existing burst
- Arcane Step echo (ArcaneStepBehaviour): add `ApplyStatus(VoidLeak, 1)` on echo detonation
- Forked Lightning: check first target for VoidRot×3, propagate 1 stack to chain targets if true
- Dark Mark (WraithAbilities): add Hemorrhage application
- Dark Harvest: read `ConsumeDebuffStacks()` expanded to include VoidRot
- Mending Circle: on-exit event apply Renewal to departing ally (requires zone tracking trigger)
- Spirit Wisps: landing apply Triage instead of instant heal
- Sacred Aegis: on-break event, calculate absorbed amount → apply SacredRegeneration

**New Passive — Ironclad Counter Blow:**
Iron Aura proc on max-absorption release. Needs a new `IronAuraHandler` component similar to `SoulBondTether`.

**Sentinel damage amplification:**
`RunicSentinel.cs` fire method — before applying damage, query target's VoidRot stack count → add `stacks × 3` bonus void damage.

**Void Collapse transition:**
TransitionChecker: if target has VoidRot×3 AND receives Binding Wave hit → fire VoidCollapse (15 dmg, spread 1 VoidRot to adjacent).

**Boss states:**
`WorldBossController.cs` Final Surge: query how many DoT types are active on boss → multiply attack speed by (1 + 0.05 × dotsActive).
