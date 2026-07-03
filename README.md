# Crossworlds BCE

> **Co-op action RPG · 5 classes · 32 abilities · Smite-style combat · Multiplayer hub + arenas**

![Crossworlds BCE](Docs/logo.png)

**[🌐 playcrossworlds.com](https://playcrossworlds.com/) · [▶ Play in Browser](https://playcrossworlds.com/play/) · [⚔ Combat Reference](https://playcrossworlds.com/combat/) · [GM Dashboard](http://playcrossworlds.com:4000)**

---

![Multiplayer in the Hub](Docs/multiplayer-chat-working.png)

---

Crossworlds BCE is a server-authoritative co-op action RPG built on Unity 6 and Mirror networking. Players log in, choose from five hero classes, meet in a shared hub world, and enter combat arenas through portals. Every ability is a skill shot, AoE, or telegraphed cone — no auto-attack tab targeting. Dodge rolls, traveling projectiles, and enemy telegraph indicators make every fight readable and punishing to play recklessly.

---

## Contents

- [Classes](#classes)
- [Spellbook — All 32 Abilities](#spellbook--all-32-abilities)
- [Combat System](#combat-system)
- [Features Status](#features-status)
- [Developer Reference](#developer-reference)
  - [VPS Operations](#vps-operations)
  - [Database Schema](#database-schema)
  - [API Reference](#api-reference)
  - [Networking](#networking)
  - [Build & Deploy](#build--deploy)
  - [Editor Steps](#editor-steps)
  - [Changelog](#changelog)
  - [Open TODOs](#open-todos)
  - [Project Structure](#project-structure)

---

## Classes

Five hero classes, each with a distinct role and playstyle. Class indices are fixed — never renumber.

| # | Class | Hero | Role | Identity |
|---|-------|------|------|----------|
| 0 | **Warden** | | Battlemage / Utility | Deploys construct turrets, buffs allies, traps and controls space |
| 1 | **Ironclad** | | Tank / Disruptor | Absorbs damage, counters attacks, pulls enemies into the team |
| 2 | **Shadowblade** | **Bo-gar** | Assassin / Disruptor | Stealth burst, debuff stacking, silence zones, harvests stacks for big AoE |
| 3 | **Cleric** | **Brandolf** | Support / Healer | Heals, shields, revives, soul bonds, and in extremis rewinds time |
| 4 | **Arcanist** | | Mage / Control | Phase-shifts across terrain, chain lightning, singularity pulls, event horizons |

### Warden
*"Hold the line. The constructs will do the rest."*

The Warden is a battlefield engineer who wins through positioning and attrition. They deploy runic sentinel turrets, lay snare traps, redirect their constructs mid-fight, and pulse cooldowns for the whole team with Battle Hymn. Their ultimate, Conjurer's Surge, triggers every active construct simultaneously for burst rounds.

**Passive:** Runic Mastery — each construct the Warden controls increases ability damage by a flat bonus.  
**Playstyle:** Plant turrets before the pull, snare the lead enemy, redirect fire to priority targets, hymn when the team is burning CDs.  
**Synergies:** Ironclad pulls enemies into Warden's snare fields; Cleric keeps the turrets' owner alive long enough to matter.

### Ironclad
*"Let them hit me. I'm building a debt they can't pay."*

The Ironclad defines the frontline. Counter Blow turns incoming damage into a cone burst; Gravity Slam pulls a mob into a single point for the team to cleanse; Stalwart Stance becomes a damage sponge while tripling Threat generation. Iron Rampart raises an impenetrable stone wall that stops all projectiles for 10 seconds.

**Passive:** Iron Will — each hit taken in Stalwart Stance generates a Fortify stack that reduces the cooldown of Shieldwall Charge.  
**Playstyle:** Initiate with Shieldwall Charge, Gravity Slam to bunch, hold Counter Blow stance while the team burns, Iron Rampart to split ranged encounters.  
**Synergies:** Shadowblade silences enemies in Gravity Slam's kill zone; Arcanist's chain lightning bounces between the bunched mob; Warden's sentinel fires into a clump.

### Shadowblade — Bo-gar

<img src="Inspiration ART/Hero Bo-Gar.png" alt="Bo-gar" width="180"/>

*"They can't hit what they can't see. Or silence."*

The Shadowblade is a precision assassin and soft CC specialist. Shadow Veil into a stealth-boosted Mind Spike is the signature opener. Silence Ward drops a fog field that stops all enemy abilities. Dark Harvest consumes all active debuff stacks on nearby enemies for massive AoE — ideally after Ironclad has applied Threat stacks and Cleric has applied Cursed.

**Passive:** Phantom Discipline — landing an attack from stealth resets Shadow Veil's cooldown (once per veil).  
**Playstyle:** Stealth → Mind Spike burst, plant Silence Ward on the caster mob, rotate AoE into Binding Wave to root, harvest stacks for the finisher.  
**Synergies:** Ironclad applies Threat stacks (debuffs), Silence Ward stacks Cursed; Shadowblade Dark Harvest converts all of it to damage.

### Cleric — Brandolf

<img src="Inspiration ART/Hero Brandolf.png" alt="Brandolf" width="180"/>

*"The fight ends when I say it ends."*

The Cleric is the team's life insurance. Spirit Wisps drift and seek allies. Sacred Aegis grows stronger as the target takes hits. Soul Bond reroutes incoming damage onto the Cleric themselves as a sacrifice. Divine Spark revives a downed teammate — or detonates holy energy on undead. Temporal Grace is the most powerful ability in the game: full-team time rewind.

**Passive:** Grace Under Fire — when the Cleric's HP drops below 30%, all active heal abilities tick at double rate for 8 seconds.  
**Playstyle:** Stay mobile, maintain Spirit Wisps rotation, Soul Bond the squishiest carry, save Divine Spark for revives, never waste Temporal Grace.  
**Synergies:** Every class benefits from Temporal Grace. Sacred Aegis pairs with Ironclad when he's in Stalwart Stance absorbing the most hits.

### Arcanist
*"Distance is an illusion. So is the concept of 'safe.'"*

The Arcanist controls space. Arcane Step is a true blink — it bypasses terrain, colliders, enemy hitboxes. Void Maw opens a singularity that drags enemies in before detonating. Forked Lightning chains between four targets. Collapsing Void is the team's hardest-hitting ability: an event horizon that pulls for 3 seconds then collapses for 60 AoE with the Weakened debuff applied.

**Passive:** Phase Resonance — each blink (Arcane Step) within 4 seconds of the last one reduces Void Maw's cooldown by 2 seconds (stacks up to 3 times).  
**Playstyle:** Open with Arcane Step to flank, drop Void Maw to pull the pack, chain lightning the clump, blink out when the mob turns, Collapsing Void when the full team is in position.  
**Synergies:** Void Maw pull combines with Ironclad's Gravity Slam for near-instant bunching; Warden's turrets fire into the singularity zone automatically.

---

## Spellbook — All 32 Abilities

### Shared Pool (indices 0–7 — available to all classes)

| # | Ability | Type | Shape | Damage | CD | Description |
|---|---------|------|-------|--------|----|-------------|
| 0 | **Runic Sentinel** | Support | Deploy | — | 6s | Deploys a stationary runic turret that fires void bolts at nearby enemies until destroyed. |
| 1 | **Void Bolt** | Damage | Skill Shot | 15–45 | 3s | Fires a skill-shot bolt of void energy. Charge up to triple damage — you must aim and dodge to use it well. |
| 2 | **Mending Circle** | Heal | AoE | — | 5s | Inscribes a glowing rune circle on the ground that heals all allies standing inside it. |
| 3 | **Storm Lash** | Damage | Line | 15–50 | 4s | Unleashes a rushing wall of storm energy in a line, damaging all enemies it passes through. |
| 4 | **Ember Surge** | Damage | AoE | 20–45 | 4s | Detonates a burst of fire at the target point, scorching all enemies caught in the blast. |
| 5 | **Mind Spike** | Damage | AoE | 35 | 5s | Sends a focused psychic spike to the target point, dealing heavy single-target damage. |
| 6 | **Binding Wave** | Damage | AoE | 15 | 6s | Releases a wide void pulse that damages and Binds all enemies in range, rooting them in place. |
| 7 | **Arcane Ward** | Support | Self | 50 absorb | 8s | Instantly wraps you in an arcane barrier absorbing up to 50 damage. Expires after 5 seconds. |

### Warden (indices 8–12)

| # | Ability | Type | Shape | Damage | CD | Description |
|---|---------|------|-------|--------|----|-------------|
| 8 | **Runic Snare** | Damage | Deploy | 40 | 5s | Places an armed rune trap at the target point. Detonates in a burst when an enemy walks over it. |
| 9 | **Battle Hymn** | Support | AoE | — | 12s | Channels a rallying war hymn that reduces ability cooldowns for all nearby allies. |
| 10 | **Spirit Redirect** | Support | Target | — | 8s | Commands your active Runic Sentinel to abandon its post and focus fire on your target. |
| 11 | **Mend** | Heal | Target | — | 6s | Channels restorative energy into a single ally, healing wounds and purging all active debuffs. |
| 12 | **Conjurer's Surge** | Support | Self | — | 45s | Surges all your active deployed constructs simultaneously, triggering them at full power at once. |

### Ironclad (indices 13–18)

| # | Ability | Type | Shape | Damage | CD | Description |
|---|---------|------|-------|--------|----|-------------|
| 13 | **Counter Blow** | Support/Damage | Cone | up to 60 | 10s | Enters an absorption stance for 3 seconds. Releasing unleashes all absorbed damage as a cone burst. |
| 14 | **Gravity Slam** | Support | AoE | — | 7s | Slams the ground with gravitational force, pulling all nearby enemies into the impact point. |
| 15 | **Shieldwall Charge** | Damage | Line | 25 | 6s | Charges forward, slamming through enemies for 25 damage and generating Threat stacks on each hit. |
| 16 | **Stalwart Stance** | Support | Self | — | 14s | Plants your feet: 40% damage reduction and tripled Threat generation for 6 seconds. Cannot move. |
| 17 | **Rune Chain** | Support | Target | — | 9s | Etches a runic leash onto one enemy for 5 seconds, absorbing 15% of attacks they land on allies. |
| 18 | **Iron Rampart** | Support | Deploy | — | 50s | Raises a massive stone rune wall in front of you that blocks all projectiles for 10 seconds. |

### Arcanist (indices 19–22)

| # | Ability | Type | Shape | Damage | CD | Description |
|---|---------|------|-------|--------|----|-------------|
| 19 | **Arcane Step** | Support | Blink | — | 4s | Phase-shifts your body to the targeted location, bypassing terrain and enemy colliders. |
| 20 | **Void Maw** | Damage | AoE | 20 | 9s | Opens a singularity that pulls all enemies inward for 3 seconds, then detonates in a burst of void energy. |
| 21 | **Forked Lightning** | Damage | AoE | 30 chain | 7s | Unleashes chain lightning that arcs between up to 4 enemies (30 / 25 / 20 / 15 damage per jump). |
| 22 | **Collapsing Void** | Damage | AoE | 60 | 50s | Summons a massive event horizon. Pulls for 3 seconds, then collapses for 60 AoE and applies Weakened. |

### Cleric (indices 23–28)

| # | Ability | Type | Shape | Damage | CD | Description |
|---|---------|------|-------|--------|----|-------------|
| 23 | **Soul Bond** | Support | Target | — | 9s | Bonds with a nearby ally for 5 seconds, rerouting all incoming damage dealt to them onto you instead. |
| 24 | **Spirit Wisps** | Heal | Deploy | — | 7s | Releases drifting wisps that seek out nearby allies to heal them and chip enemies they pass through. |
| 25 | **Divine Spark** | Heal/Damage | Target | 60 (undead) | 14s | Revives a downed ally at 30% HP — or, if cast on undead enemies, detonates for 60 holy damage. |
| 26 | **Sacred Aegis** | Support | Target | — | 10s | Places a living shield on an ally that grows stronger (up to 80 absorb) as they take hits over 8 seconds. |
| 27 | **Dispel** | Support | Target | — | 7s | Instantly purges every active debuff from a target ally, no matter how many are stacked. |
| 28 | **Temporal Grace** | Heal | Team | — | 60s | Rewinds the entire team 5 seconds, restoring their HP, positions, and clearing all debuffs gained since then. |

### Shadowblade (indices 29–31)

| # | Ability | Type | Shape | Damage | CD | Description |
|---|---------|------|-------|--------|----|-------------|
| 29 | **Shadow Veil** | Support | Self | — | 10s | Vanishes into full invisibility for 4 seconds. Breaking stealth immediately with Mind Spike deals +50% bonus damage. |
| 30 | **Silence Ward** | Support | Deploy | — | 12s | Plants a cursed fog field that silences all enemy abilities and applies Cursed (DoT) while they remain inside. |
| 31 | **Dark Harvest** | Damage | AoE | 20/stack | 40s | Consumes all active debuff stacks on nearby enemies, dealing 20 damage per stack consumed. |

---

## Combat System

### Smite-Style Directional Combat

Every offensive action requires aim. There is no auto-attack or tab-targeting.

| Mechanic | Detail |
|----------|--------|
| **Skill Shots** | Void Bolt fires a traveling `PlayerProjectile` — you must lead the target. Charge for up to 3× damage. |
| **AoE Placement** | Circle and Rectangle indicators show the exact area before you release. |
| **Cone Abilities** | Directional burst (Counter Blow, Shieldwall Charge) — face the target cluster. |
| **Charge Mechanic** | Hold LMB to hold an indicator. Damage and size scale with hold time up to `maxChargeTime`. Release to cast. |
| **Dodge Roll** | Left Alt or V — 2 charges, 0.35s roll, full invulnerability during the animation. |
| **Enemy Telegraphs** | 0.45s before any enemy attack lands, a red cylinder indicator appears on the ground. Read it, dodge it. |

### Server Authority

All game-state mutations run server-only (`[Server]` attribute). Clients receive visual feedback via `[ClientRpc]`. Client-only code (VFX, HUD, tooltips) is behind `#if !UNITY_SERVER`. Players are never trusted — damage is calculated server-side, kill rewards issued atomically.

---

### State Layer — DoTs, HoTs & Transitions

> **Design status:** full design is authored in `CrossWorlds/_context/COMBAT_PROPOSAL.md`. Mechanics below are the confirmed design — implementation follows the proposal's code notes. Nothing new in code yet.

The base combat is burst-oriented (build resource → detonate → reset). The state layer adds a **persistent consequence plane** so players also track what states are active on enemies and allies simultaneously. Same 32 ability slots. No new buttons. All states are visible on `StatusEffectHUD`.

#### DoT States (applied to enemies)

| State | Damage | Duration | Stacks | Notes |
|-------|--------|----------|--------|-------|
| **Void Rot** | 4/s | 6s | up to 3× | Each stack also raises void ability damage received by 2% (3 stacks = +6%). |
| **Burning** | 6/s | 5s | 1× | Refreshes on reapply. If target is also Weakened → **Combustion**: +50% tick damage, +2s duration. |
| **Void Leak** | 3/s | 8s | 1× | On death, spreads 1 Void Rot stack to enemies within 3u. Contagion mechanic. |
| **Hemorrhage** | 5/s | 3s | 1× | Applied by Dark Mark. Does not spread; stacks alongside Cursed. Single-target DoT setup tool. |
| **Cursed** | 8/s | 4s | stackable | Shadowblade's primary stack currency. Dark Harvest now also consumes Void Rot stacks. |

#### HoT States (applied to allies)

| State | Healing | Duration | Source | Notes |
|-------|---------|----------|--------|-------|
| **Renewal** | +10 HP/s | 5s | Mending Circle on-exit | Applied when an ally *leaves* the zone, not on entry. The afterglow follows them. |
| **Triage** | +6 HP/s | 4s | Spirit Wisps on-land | Replaces the Wisps' instant heal. Same total healing over 4s. Cleric's Triage Loop procs on each tick. |
| **Sacred Regeneration** | +8–15 HP/s | 2–6s | Sacred Aegis on break | Duration scales with absorbed damage before break: min 2s (20 absorb), max 6s (80 absorb). |

#### Transition States

Emergent states produced when two specific conditions are simultaneously true on the same target. No new cast required — they occur as a consequence of correct two-player sequencing.

| Condition A | Condition B | Transition | Effect |
|-------------|-------------|------------|--------|
| Burning (active) | Slow (active) | **Scorched** | Slow upgrades to a 1.5s root. Cleared by Mend or Dispel. |
| Void Rot ×3 | Binding Wave hit | **Void Collapse** | 15 instant damage + radiates 1 Void Rot stack to all enemies within 3u. |
| Cursed ×4+ | Dark Harvest | **Withered** | After detonation: −40% move speed for 4s. (3 stacks = max damage; 4+ stacks = Withered CC — Shadowblade's decision.) |
| Weakened + Burning | — | **Combustion** | Burning already gets +50% tick and +2s from Weakened presence. Named for visual clarity. |
| Renewal + Sacred Aegis (same ally) | — | **Sanctified** | Incoming damage drains the HoT first before the shield absorbs. Extends effective shield duration. Cleric-exclusive. |
| Void Leak + Silence Ward (enemy inside zone) | — | **Void Silence** | Cursed tick damage inside the Ward is doubled for Void Leak enemies. Zone becomes high-damage if pre-seeded. |
| Shieldwall Charge (passing Void Rot enemy) | — | **Contamination Pass** | Ironclad spreads 1 Void Rot stack to every enemy passed through during the charge. AoE DoT seeding as a side effect. |

---

### Per-Class State Mechanics

#### Warden — Construct Tactician
- **Runic Snare detonation** → enemy gains **Structural Weakness** (6s): +15% damage taken from constructs. Snare becomes a mark, not just a trap.
- **Overengineered at max stacks** → active Sentinels emit a void pulse every 4s applying 1 Void Rot stack (3u radius). Constructs become passive DoT applicators.
- **Sentinel amplification** → Sentinel shots deal bonus void damage equal to (Void Rot stacks on target × 3). At 3 stacks, +9 void per hit — highest sustained single-target DPS window in the game when combined with Arcanist pre-stacking and Conjurer's Surge.
- **Battle Hymn interaction** → allies who cast during the 8s CDR window apply **Attuned** (1s); on expiry, each proc reduces the Warden's next construct CD by 1s. Cross-class CDR feedback loop.
- **Mend** can break the Scorched transition (Burning+Slow root) — distinct cleanse flavor from Cleric's Dispel.

#### Ironclad — Iron Vanguard
- **Counter Blow max-absorption release** → 2s of **Iron Aura**: damage dealt to nearby allies (6u) is partially redirected to Ironclad (15% per hit). A second passive Soul Bond window without Cleric involvement.
- **Stalwart Stance** → also applies **Resolve**: next CC received is reduced by 50% duration. Encourages standing ground during Stance.
- **Rune Chain Tension** → leashed enemy accumulates 1 Tension stack per 1.5s while leashed. On leash expiry or break: AoE Stagger on the target and nearby (2u). Free CC from a setup ability.
- **Iron Rampart Exposed** → enemies that impact the wall (blocked projectiles or enemies driven into it) gain **Exposed** (5s): +15% physical damage taken. Drive enemies into the wall via Gravity Slam for the team's follow-up.
- **Contamination Pass** → Shieldwall Charge through a Void Rot enemy spreads 1 stack to the entire charge path. Accidental AoE DoT seeding.

#### Arcanist — Void Mage
- **Void Bolt hit** → always applies 1 Void Rot stack (charged or not; charge gives burst damage, not extra stacks). 3 Void Bolt hits = fully stacked target ready for Void Collapse, Sentinel amplification, or Dark Harvest consumption.
- **Ember Surge split** → uncharged applies Burning (DoT setup); charged = existing burst damage. The key decision: charge for damage, or tap for Combustion setup.
- **Arcane Step echo** → the existing 1s-delay detonation also applies Void Leak to all enemies in the radius. Void Leak seeded passively on every blink through a group.
- **Forked Lightning DoT propagation** → if the first chain target has Void Rot ×3, the chain spreads 1 Void Rot stack to each subsequent target (up to 4 enemies). One cast seeds an entire cluster.
- **Phase Charge exclusive proc** → if the 6th cast (Phase Charge empowered hit) applies Burning, Combustion triggers immediately without Weakened being present. One-time proc that rewards saving Phase Charge for Ember Surge.
- **Arcane Ward on break** → releases a void pulse applying 1 Void Rot stack to enemies within 4u. Self-defense becomes stack application.

#### Cleric — Soul Warden
- **Mending Circle Renewal** → allies who *leave* the zone carry Renewal (+10 HP/s, 5s). Zone's reach now extends beyond its radius.
- **Spirit Wisps become Triage** → Wisps apply Triage (+6 HP/s, 4s) on landing instead of instant heal. Same total healing over duration. Cleric tracks active Triage timers and decides when to re-cast.
- **Sacred Aegis → Sacred Regeneration on break** → conversion on full absorb. Min 2s at 20 absorb; max 6s at 80 absorb. Rewards proactive shielding before damage arrives.
- **Sanctified** → Renewal + Sacred Aegis both active on same ally = incoming damage drains the HoT first before shield absorbs. Purely emergent.
- **Triage Loop passive refinement** → 5% instant + 3% as a 4s personal Triage HoT on the Cleric herself. Refreshes with every heal applied. Always self-sustaining during active healing.
- **Dispel change** → targets the *most dangerous* single debuff (highest damage/duration), not all debuffs. Mend (surgical, removes 1) vs Dispel (targeted nuclear against transition states like Scorched or Void Silence).

#### Shadowblade — Void Infiltrator
- **Dark Mark gains Hemorrhage** → Dark Mark now applies Hemorrhage (+5 dmg/s, 3s) alongside its existing Weakened + Cursed. Single-target DoT setup tool on priority enemies.
- **Shadow Veil exit** → exit already applies Cursed via Corruption; now also applies 1 Void Rot stack to all nearby enemies. Dual-DoT burst on stealth exit.
- **Bounty System kill radiation** → on kill, Corruption passive gives −2s CDR (unchanged) + now also applies 1 Cursed stack to the nearest 2 enemies (within 8u). Every kill seeds the next target in wave content.
- **Withered at 4+ stacks** → Dark Harvest at 4+ Cursed stacks = Withered (−40% move speed, 4s). 3 stacks = max damage; 4+ stacks = Withered CC. Against bosses at phase transitions, Withered is almost always worth more.
- **Shattered on Weakened Harvest** → if the target is Weakened when Dark Harvest fires, survivors gain **Shattered** (3s): next attack from any player deals +25% damage. Team-wide finish window.

---

### Cross-Class Synergies (State Layer)

| Combo | Mechanic |
|-------|----------|
| **Arcanist → Shadowblade** | Void Bolt builds Void Rot. Dark Harvest now consumes Void Rot stacks. 3 pre-built Void Rot stacks = 3 free Harvest stacks. |
| **Ironclad → Arcanist** | Rune Chain holds a target 5s — clean window for 3 Void Bolts → Void Rot ×3 → Binding Wave → Void Collapse (15 AoE + contamination). |
| **Warden + Arcanist** | Arcanist pre-builds Void Rot ×3 on a target; Warden Spirit Redirects all Sentinels to that target (+9 void per hit). Conjurer's Surge during this window = highest single-target DPS in the game. |
| **Cleric + Any** | Mending Circle Renewal changes arena movement — the team routes *through* the zone edge to pick up Renewal, not stand in the center. |
| **Arcanist + Shadowblade** | Arcane Step echo applies Void Leak to a group; Shadowblade drops Silence Ward on the same group → **Void Silence**: doubled Cursed ticks inside the ward. Highest sustained AoE damage window. |
| **Ironclad + Warden** | Iron Rampart blocks a corridor; Gravity Slam pulls enemies into the wall → Exposed (+15% physical taken); Warden's AoE lands on a clump at full density. |
| **Shadowblade + Ironclad** | Kill radiation (Bounty System) applies 1 free Cursed stack to the Rune Chain target. Reduces Shadowblade cast count needed to reach Dark Harvest threshold. |
| **Cleric + Ironclad** | Sacred Aegis on Ironclad while he Counter Blow absorbs; Aegis breaks → Sacred Regeneration activates; Counter Blow releases simultaneously. If Renewal also active → **Sanctified**: layered recovery post-burst. |
| **⚠ Negative** | Mending Circle must NOT cleanse enemy debuffs — a poorly placed zone could erase Shadowblade's Corruption/Void Rot stacks. Design rule: Mending Circle cleanse is ally-only. |

---

### Boss Mechanic Hooks (Design — Phase 3)

Each boss reads at least one active state from the party and reacts to it.

#### Null Architect
- **Phase 2 — Tether Web:** leashed players gain 1 Void Rot stack every 2s while tethered. At Void Rot ×3 while leashed: tether briefly stuns (1s). Mend or Dispel clears Void Rot from the leashed player — Cleric has a clear urgent job.
- **Final Surge — Resonance Penalty:** for every active DoT on the Null Architect when Final Surge begins, it gains +5% attack speed (max +15% with 3 DoTs active). Forces a pre-Surge DoT cleanse window where the team *stops* applying damage. Real tension.

#### Iron Warden (Phase 3)
- **Magnetized Pulse:** each active turret arm emits a pulse every 12s applying Magnetized (8 dmg/s, 3s) to all players. Not cleansed by Dispel. Cleared by standing in Mending Circle for 1.5s. Cleric's zone placement becomes infrastructure.
- **Arm Respawn Burning Immunity:** respawned arms are immune to Burning for 6s. Void Rot pre-stacks before destruction carry over. Spirit Redirect during the immunity window is the intended counter.
- **Core Threat Targeting:** Core targets the player with the lowest Threat stacks. Ironclad in Stalwart Stance locks Core onto himself; while targeted, Counter Blow absorb applies a team-wide 15% redirect.

#### Void Herald (Phase 3)
- **Void Mark Escalation:** stealth phase applies Void Mark to all players (3 Cursed stacks, boss-applied, not Harvest-consumable). +1 stack per 2s. Silence Ward placed inside reveals the Herald for 2s. Dispel clears 3 stacks from one ally per cast.
- **Dark Harvest Mirror:** at 8+ Void Mark stacks when stealth ends → 160 instant damage (20 × 8). Temporal Grace is the primary counter — but rolling back also un-Dispels any cleanse casts made in the last 5s.
- **Void Leak Trap:** if the Herald has Void Leak active when stealth ends, the reveal detonation spreads 1 Cursed stack to nearby players. Arcanist must track their own Arcane Step echo to not seed Void Leak on the Herald during setup.

---

## Features Status

| Feature | Status |
|---------|--------|
| Login / register / JWT auth | ✅ Live |
| Character select (5 classes) | ✅ Live |
| Shared hub world (multiplayer) | ✅ Live |
| Chat system | ✅ Live |
| Ability bar (1–4 hotkeys) | ✅ Live |
| AoE / Cone / Line indicators | ✅ Live |
| Skill shot (Void Bolt) | ✅ Implemented |
| Spellbook panel (Tab) | ✅ Live |
| Ability tooltip on hover | ✅ Live |
| Dodge roll (Alt/V, 2 charges) | ✅ Live |
| Enemy telegraph indicators | ✅ Implemented |
| Enemy AI (NavMesh, aggro) | 🔶 Editor steps pending |
| Arena portal transition | 🔶 In progress |
| Drop table / loot | ✅ API live · 🔶 Unity client pending |
| Inventory bag UI | 🔶 Pending |
| Progression (XP / levels) | ✅ API live · 🔶 Unity pending |
| Crafting | ✅ API live · 🔶 Unity pending |
| GM dashboard | ✅ Live |
| Uptime monitoring (Kuma) | 🔶 Needs web UI config |
| HTTPS / SSL | 🔶 Pending |
| CI/CD pipeline | 🔶 Secrets not configured |

---

---

# Developer Reference

---

## VPS Operations

### Server Info

| Item | Value |
|------|-------|
| Host | `playcrossworlds.com` / `15.204.243.36` |
| SSH | `ssh ubuntu@playcrossworlds.com` |
| Game binary | `/game/Builds/CrossworldsBCE.x86_64` |
| Game data dir | `/game/Builds/CrossworldsBCE_Data/` |
| Auth server | `/opt/crossworlds-auth/server.js` (legacy path: `/opt/rod-auth/`) |
| Dashboard | `/opt/crossworlds-dashboard/server.js` |
| Game log | `/var/log/crossworlds.log` |
| Web root | `/var/www/crossworlds/` |
| Client download | `/var/www/crossworlds/downloads/CrossworldsBCE.zip` |

### Services

| Service | Port | What |
|---------|------|------|
| `crossworlds` | 7777/UDP | Unity game server (Mirror/KCP) |
| `crossworlds-auth` | 3000/TCP | Node.js auth + character + game API |
| `crossworlds-dashboard` | 4000/TCP | GM/admin web dashboard + Socket.io |
| `rod-realtime` | 5000/TCP (local) | Realtime relay — `/opt/rod-realtime/server.js` (purpose TBD) |
| nginx | 80/443 | Public download page, SSL via Certbot |
| Uptime Kuma | 3001 | Monitoring (web UI needs config) |

### Essential Commands

```bash
# Status
sudo systemctl status crossworlds crossworlds-auth crossworlds-dashboard

# Restart
sudo systemctl restart crossworlds-auth
sudo systemctl restart crossworlds-dashboard
sudo systemctl restart crossworlds

# Logs
sudo journalctl -u crossworlds-auth -n 50 --no-pager
sudo journalctl -u crossworlds -n 50 --no-pager
tail -f /var/log/crossworlds.log

# Ports
ss -ulnp | grep 7777    # game UDP
ss -tlnp                # all TCP

# Binary sanity check
ls -la /game/Builds/CrossworldsBCE.x86_64

# Health check
curl http://localhost:3000/api/health

# Database
mysql -u crossworlds -p crossworlds   # password in /opt/crossworlds-auth/.env → DB_PASS
```

### Deploy New Build

```bash
# Local (PowerShell)
powershell -ExecutionPolicy Bypass -File tools\build-server.ps1
scp build\crossworlds-server.tar.gz tools\deploy-server.sh ubuntu@playcrossworlds.com:~

# On VPS
sudo bash deploy-server.sh             # auto-backup, restart, verify, auto-rollback on failure
sudo bash deploy-server.sh --rollback  # manual rollback
```

**Binary name is critical.** The systemd service `ExecStart` must match exactly:

```bash
cat /etc/systemd/system/crossworlds.service
# If you ever rename the binary:
systemctl daemon-reload && systemctl restart crossworlds
```

### Dashboard URLs

| URL | Access |
|-----|--------|
| `https://playcrossworlds.com/` | Public download page |
| `http://playcrossworlds.com:4000` | Manager dashboard (HTTP Basic Auth) |
| `http://playcrossworlds.com:4000/gm-dashboard?token=<TOKEN>` | GM dashboard (token in VPS .env) |

GM Dashboard: server status, spawn events, last 50 log lines (color-coded), restart button, log download.

### Known Pitfalls

| Symptom | Cause | Fix |
|---------|-------|-----|
| Server starts then crashes | `UnityPlayer.so` version mismatch | Upload matching `UnityPlayer.so` from same build session |
| `Could not spawn` in game | Old binary after prefab rebuild | Upload fresh server build |
| Server not listening on 7777 | Wrong binary name in systemd, or crash | Check service file; check game log |
| Players can't see each other | Client/server have different prefab assetIds | Rebuild BOTH client and server after any prefab change |
| Auth server returns 500 | DB connection issue or bad .env | `journalctl -u crossworlds-auth`; verify MySQL is running |
| `crossworlds.service` restart loop | systemd burst limit | Add `StartLimitIntervalSec=0` to [Service] block |

### Credentials

**Never commit.** All secrets live on the VPS only.

| Secret | Location |
|--------|----------|
| MySQL password | `DB_PASS` in `/opt/crossworlds-auth/.env` |
| JWT secret | `JWT_SECRET` in `/opt/crossworlds-auth/.env` |
| Admin API token | `ADMIN_TOKEN` in `/opt/crossworlds-auth/.env` and `/opt/crossworlds-dashboard/.env` |
| Dashboard HTTP Basic Auth | dashboard `.env` / nginx config |

> **Q7 outstanding:** Credentials leaked into git history once. Rotate on VPS if not already done.

---

## Database Schema

MySQL 8, database `crossworlds`.

### Old Gear System — DO NOT TOUCH

Unity calls these on every player spawn. Never rename, remove, or alter.

| Table | Purpose |
|-------|---------|
| `item_template` | Static item definitions (old system) |
| `item_instance` | Per-player item instances (old system) |
| `character_gear` | Equipped gear per character (old system) |
| `loot_tables` | Extended: added `source_name VARCHAR(64)`, `new_item_id VARCHAR(64)`; old int columns preserved |

Protected endpoints (read only, never modify): `GET /character`, `POST /character`, `PATCH /character/position`, `POST /character/gear/equip`, `GET /items`

### New System

```sql
-- Core character progression
characters (
  id, account_id, name, class_id, level, experience, gold,
  stat_str, stat_agi, stat_int, stat_vit,
  pos_x, pos_y, pos_z, orientation
  -- note: column is "experience" not "xp" — server.js and Unity must use "experience"
)

-- Item catalog
items (id VARCHAR(64), name, rarity, item_type, stat_bonus JSON, sell_value)

-- Player inventory
inventory (character_id, slot_index, item_id, quantity, equipped)

-- Crafting
professions   (character_id, profession_id, skill_level, skill_xp)
recipes       (id, profession_id, skill_level_required, result_item_id)
recipe_ingredients (recipe_id, item_id, quantity)

-- Enemy templates
enemy_templates (
  id VARCHAR(64), display_name, max_hp,
  damage_min, damage_max, move_speed, aggro_range,
  xp_reward, gold_reward_min, gold_reward_max,
  loot_source_id   -- links to loot_tables.source_name
)

-- Loot (DB-backed, seeded: goblin / troll / skeleton / mimic)
loot_tables (extended):
  source_name VARCHAR(64)   -- e.g. "goblin", "mimic"
  new_item_id VARCHAR(64)   -- links to items.id
```

### Phase 2 Stubs (schema only, no endpoints yet)

```sql
gold_transactions, marketplace_listings, guilds, guild_members
```

### MySQL 8 Migration Pattern

```sql
-- ADD COLUMN IF NOT EXISTS workaround (MySQL 8 doesn't support it directly)
SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.COLUMNS
   WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='tbl' AND COLUMN_NAME='col') = 0,
  'ALTER TABLE tbl ADD COLUMN col INT NOT NULL DEFAULT 0',
  'SELECT 1'
);
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
```

---

## API Reference

All new endpoints return `{ success: true, data: {...} }` or `{ success: false, error: "player-readable string" }`.

### Auth (old system — do not modify)

```
POST /login
POST /register
GET  /character                      → character + gear (Unity spawn)
POST /character                      → create character
PATCH /character/position
POST /character/gear/equip
GET  /items                          → item_template rows
```

### New Endpoints

```
GET  /api/health

# Inventory
GET  /api/inventory/:characterId
POST /api/inventory/save             { characterId, slots:[{slot_index, item_id, quantity, equipped}] }
POST /api/inventory/equip            { characterId, slot_index, equipped: 0|1 }
GET  /api/items                      all items table rows (no auth, for Unity bag UI)

# Progression
POST /api/character/save-progress    { characterId, level, experience, gold, stat_str, stat_agi, stat_int, stat_vit }
GET  /api/character/stats/:characterId  requireJWT → { base, bonus, total, level, gold }

# Crafting
GET  /api/professions/:characterId
GET  /api/recipes?profession=mining
POST /api/craft                      { characterId, recipeId }

# Enemies
GET  /api/enemies                    all enemy_templates (no auth, for Unity arena load)
GET  /api/enemies/:id                single enemy

# Combat
POST /api/combat/hit                 requireJWT { characterId, enemyTemplateId, damageDealt }
POST /api/combat/kill                requireJWT { characterId, enemyTemplateId }
                                       → { xpGained, goldGained, itemDropped }

# Loot
POST /api/loot/roll                  requireJWT { characterId, enemyType }   (old in-memory)
POST /api/loot/drop                  requireJWT { characterId, sourceId }    (DB-backed)

# Economy
POST /api/gold/adjust                requireJWT { characterId, amount (signed int) }

# Server
GET  /api/maintenance/status         { maintenance: bool }
GET  /api/broadcast/pending          returns + marks delivered

# Admin (header: x-admin-token)
GET  /api/admin/stats
GET  /api/admin/accounts
POST /api/admin/accounts/create      { username, password }
PATCH /api/admin/accounts/:id/ban    { banned: bool }
DELETE /api/admin/accounts/:id       cascades chars / inventory / gear
GET  /api/admin/characters
PATCH /api/admin/characters/:id      whitelist: level, experience, gold, stat_*, pos_*
POST /api/admin/characters/:id/give-item
POST /api/admin/characters/:id/give-gold
POST /api/admin/broadcast            { message }
GET  /api/admin/logs                 ?lines=&prefix=
POST /api/admin/maintenance/toggle
```

### Combat Anti-Exploit

Two in-memory Maps in `server.js`:

**Hit gate (`recentHits`)**
- Key: `` `${charId}:${enemyTemplateId}` ``
- `POST /api/combat/hit` writes `Date.now()` to the key
- `POST /api/combat/kill` requires key age < 30 000 ms → else HTTP 400
- Entry consumed on kill; next kill of same type needs a fresh hit
- Pruned every 60 s

**Kill rate limiter (`lastKillTime`)**
- Key: `charId`
- Minimum 2 000 ms between any two kills → else HTTP 429
- Per-character, all enemy types

**Kill flow (execution order):**
1. JWT + character ownership
2. Enemy template lookup (404 if unknown)
3. Rate limiter check (429)
4. Hit gate check (400)
5. Delete hit entry, record kill timestamp
6. Transaction: award XP + gold + `rollDbLoot()` + INSERT `gold_transactions`
7. Return `{ xpGained, goldGained, itemDropped }`

**Caveat:** Maps are in-process — cleared on `crossworlds-auth` restart. Acceptable for alpha.

### Server Coding Rules

```js
// Auth pattern — on every player-data endpoint
app.post('/api/endpoint', requireJWT, async (req, res) => {
  const char = await ownedCharacter(req, res, req.body.characterId);
  if (!char) return;  // 403 already sent
});

// Transaction pattern — on every multi-table write
const conn = await pool.getConnection();
try {
  await conn.beginTransaction();
  // writes
  await conn.commit();
} catch (e) {
  await conn.rollback(); throw e;
} finally { conn.release(); }
```

Log prefixes: `[LOGIN]` `[LOGOUT]` `[CRAFT]` `[LOOT]` `[PROGRESS]` `[CHAT]` `[GM]` `[COMBAT]` `[TRADE]`

SQL: parameterized queries only. No string interpolation. Ever.

---

## Networking

**Stack:** Unity 6 (6000.4.10f1) + Mirror + KCP transport, UDP 7777.

**Ports — frozen:**

| Port | Service | Rule |
|------|---------|------|
| 3000 | Auth server | Never proxy or change |
| 4000 | Dashboard | Never proxy or change |
| 7777/UDP | Game server | Hardcoded in Unity — never change |
| 80/443 | Nginx | SSL live via Certbot |
| 3001 | Uptime Kuma | Do not touch |

**Scene order:** LoginScene (0) → CharacterSelect (1) → Hub (2) → Arena (in progress)

**Mirror discipline:**
- `[Server]` on every game-state mutation
- `[ClientRpc]` for visual-only effects (telegraphs, hit VFX)
- Client-only code behind `#if !UNITY_SERVER`
- Client-side singletons (`CombatSessionTracker`, `InventoryManager`) notified from `OnStartClient` — NOT from server-side spawn paths
- `#if UNITY_EDITOR` guards are safe; Editor scripts never compile with `UNITY_SERVER`

**Class indices (canonical — never renumber):**

```
0 = Warden       (legacy docs: Engineer)
1 = Ironclad     (legacy docs: Guardian)
2 = Shadowblade
3 = Cleric
4 = Arcanist     (legacy docs: Wraith / Medic — ignore old names)
```

---

## Build & Deploy

Unity version: **6000.4.10f1** (from `ProjectSettings/ProjectVersion.txt` — older docs saying `6000.0.77f1` are stale).

### Server Build (Linux x86_64 headless)

```powershell
# 1. Pull LFS assets (requires GitHub Desktop — CLI agent has no LFS auth)
git lfs pull

# 2. Build
powershell -ExecutionPolicy Bypass -File tools\build-server.ps1
# Refuses if LFS pointer files remain; output: build\crossworlds-server.tar.gz

# 3. Upload + deploy
scp build\crossworlds-server.tar.gz tools\deploy-server.sh ubuntu@playcrossworlds.com:~
ssh ubuntu@playcrossworlds.com "sudo bash deploy-server.sh"
```

### Client Build (Windows)

1. Unity → File → Build Settings → Windows x86_64
2. Zip output to `/var/www/crossworlds/downloads/CrossworldsBCE.zip`
3. Upload via FileZilla: Host `playcrossworlds.com`, Port 22, Protocol SFTP

---

## Editor Steps

These require Unity to be open. Implement script side from CLI, then complete in the editor.

### PlayerProjectile Prefab (Skill Shot — Void Bolt)
1. Create empty GameObject → add `NetworkIdentity`, `PlayerProjectile`, `SphereCollider` (Is Trigger ✓, Radius 0.25)
2. Save as `Assets/Game/Prefabs/PlayerProjectile.prefab`
3. On each hero prefab's `AbilityCaster`, assign this prefab to **Player Projectile Prefab**
4. Add to `RodNetworkManager` → **Spawnable Prefabs** list

### Enemy Prefabs
- Run `BCE/Setup/4a` through `4d` to generate Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldBoss prefabs
- Set `enemyTemplateId` on each prefab variant to match `enemy_templates.id` rows in the DB

### NavMesh
Bake NavMesh in Arena_Copper (Window → AI → Navigation → Bake).

### Enemy Animators
Set the Avatar field on each enemy's Animator component to the matching humanoid Avatar asset.

---

## Changelog

### 2026-07-03 — Smite-style combat + spellbook
- **`PlayerProjectile.cs`** — traveling skill-shot projectile (NetworkBehaviour), hits Enemy tag, server-spawned, self-destructs at max range or on hit
- **`AbilityShape.SkillShot`** — new enum value; Void Bolt converted (range 14, speed 20, 15–45 charge damage, aim indicator as thin beam)
- **`AbilityDef`** — added `projectileSpeed` and `description` fields; all 32 abilities have descriptions
- **Enemy telegraphs** — `AttackSequence()` coroutine in `EnemyController`; `RpcShowTelegraph()` shows red cylinder 0.45s before attack
- **Spellbook panel (Tab)** — `AbilityBar.cs` builds 130×96 cards; icon, type badge, damage range, CD; click to select, 1–4 to equip
- **Ability tooltips** — `AbilityTooltipUI.cs` self-bootstrapping singleton (Canvas sortOrder 201); name/stats/description on hover
- **Dodge roll** — confirmed already live in `PlayerMovement.cs` (Left Alt/V, 2 charges, 0.35s invulnerability)
- **Fix** — `AbilityBar.cs` EventTrigger calls to `AbilityTooltipUI` wrapped in `#if !UNITY_SERVER` (resolved CS0103 on server build)

### Prior
- 5-class system with passives and class ability pools (`ClassAbilityPool` ScriptableObject)
- Mirror/KCP dedicated server build pipeline (`tools/build-server.ps1`, `tools/deploy-server.sh`)
- Node/Express auth API with all Phase 1 endpoints live on VPS
- Drop table, inventory, crafting, enemy templates — all API-complete
- GM dashboard with Socket.io live log, maintenance toggle, broadcast queue
- 20 dead scripts removed; web submodule cleanup

---

## Open TODOs

| Priority | Task |
|----------|------|
| 🔴 | Rotate credentials leaked into git history (ROADMAP Q7) |
| 🔴 | `GmConsole.cs` — add `#if !UNITY_SERVER` guard (spamming errors on server build) |
| 🔴 | **CLASS_NAMES** — live server still has `['Engineer','Guardian',...]`; live characters use those names. Changing to `['Warden','Ironclad',...]` needs coordinated Unity deploy + `UPDATE characters SET class_name=...` migration. See `_CONTEXT/VPS_SERVER.md` for migration plan. |
| 🟡 | Document `rod-realtime` (port 5000 local) — found on VPS during 2026-07-03 audit; purpose unknown |
| 🟡 | Create `PlayerProjectile` prefab in editor; assign to hero prefabs; register in NetworkManager |
| 🟡 | Bake NavMesh in Arena_Copper |
| 🟡 | Set `enemyTemplateId` on enemy prefab variants (match `enemy_templates.id` in DB) |
| 🟡 | Inventory bag UI (4×6 grid, tooltip, equip) → `/api/inventory/*` |
| 🟡 | Progression HUD (XP bar, level-up panel) → `/api/character/save-progress` |
| 🟡 | Portal transition (Hub → Arena scene loading) |
| 🟢 | Configure Uptime Kuma web UI at `http://15.204.243.36:3001` |
| 🟢 | HTTPS / Cloudflare SSL (all traffic plain HTTP; JWT in transit unencrypted) |
| 🟢 | Configure CI/CD secrets (`.github/workflows/build-and-deploy.yml` exists, needs secrets) |
| 🟢 | Domain name (currently IP-only on public page) |

---

## Project Structure

```
Assets/Game/
  Abilities/Scripts/     deployables + ability behaviours (mines, walls, zones)
  Characters/Scripts/    class passives, NPC controller, ability pools
  Combat/Scripts/        EnemyController, WaveSpawner, Health, WorldBossController,
                         StatusEffectManager, CombatSessionTracker, DropTable,
                         WorldItem, PlayerProjectile
  Networking/            RodNetworkManager, RodNetworkAuthenticator, PlayerIdentity,
                         PortalTransition, RodChatManager, ForgeNPC
  Systems/               client REST singletons (InventoryManager, ItemCatalog, HeroMastery)
  UI/                    HUDs, panels, GmConsole, LoginManager, PlayerProgressManager,
                         AbilityCaster, AbilityBar, AbilityTooltipUI
  Editor/                BCE menu builders (scenes/prefabs are reproducible from these)
  Scenes/                LoginScene(0), CharacterSelect(1), Hub(2)
  Prefabs/               5 hero prefabs + Enemy_Grunt / Enemy_Ranged / Enemy_Elite
  Heroes/Brandalf/       6th-hero model — DECISION PENDING (skin vs class), don't wire

CrossWorlds/             legacy staging tree + design docs (read-only reference)
_CONTEXT/                server/API docs (CLAUDE.md, VPS_SERVER.md)
Docs/                    logo.png, screenshots, ClassSpec.html, SpellBook.html
tools/                   build-server.ps1, deploy-server.sh
web/                     Three.js browser client submodule (separate project)
```

---

*Unity 6 (6000.4.10f1) · Mirror/KCP · Node.js/Express · MySQL 8 · VPS: playcrossworlds.com*
