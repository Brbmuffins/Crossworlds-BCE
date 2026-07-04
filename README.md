# Crossworlds BCE

> **Co-op action RPG · 5 classes · 32 abilities · Smite-style combat · Multiplayer hub + arenas**

![Crossworlds BCE](Docs/logo.png)

**[🌐 playcrossworlds.com](https://playcrossworlds.com/) · [▶ Play in Browser](https://playcrossworlds.com/play/) · [⚔ Combat Reference](https://playcrossworlds.com/combat/) · [GM Dashboard](http://playcrossworlds.com:4000)**

---

![Multiplayer in the Hub](Docs/multiplayer-chat-working.png)

---

> *The worlds tore open, and where their edges meet the Void pours through — and it does not come mindlessly. It builds. It sieges. In a drowned cathedral-city at the seam of realities, a handful of heroes hold the last ground: an engineer of runic war-machines, an unbreakable knight, a shadow-walking monk, a muffin-headed healer who bends time itself, and a mage who treats distance as a suggestion. Step through the portal. Hold the line.*

Crossworlds BCE is a server-authoritative co-op action RPG built on Unity 6 and Mirror networking. Players log in, choose from five hero classes, meet in a shared hub world, and enter combat arenas through portals. Every ability is a skill shot, AoE, or telegraphed cone — no auto-attack tab targeting. Dodge rolls, traveling projectiles, and enemy telegraph indicators make every fight readable and punishing to play recklessly.

---

## Contents

- [How to Play](#how-to-play)
- [Classes](#classes)
- [Spellbook — All 32 Abilities](#spellbook--all-32-abilities)
- [Combat System](#combat-system)
- [AFK Professions](#afk-professions)
- [Encounters & NPCs](#encounters--npcs)
  - [NPCs — Hub World](#npcs--hub-world)
  - [Enemy Roster](#enemy-roster)
  - [Boss Encounter — The Null Architect](#boss-encounter--the-null-architect)
  - [Boss Encounter — The Iron Warden](#boss-encounter--the-iron-warden)
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

## How to Play

Crossworlds is a party-based action RPG. You and your allies share a hub world, then portal into arenas to fight escalating enemy waves and bosses for loot, mastery, and crafting materials. Combat is **aim-based** — there is no auto-attack and no tab-targeting; every ability is a skill shot, placed AoE, or telegraphed cone.

### Core Loop

1. **Log in** and choose one of five classes at Character Select.
2. **Spawn into the Hub** — a shared, persistent social space. Chat, visit the Forge, and work gathering nodes (mine, fish, chop).
3. **Build your kit** — open the Spellbook (**Tab**) and slot up to four abilities into **1–4**.
4. **Enter a portal** — talk to **The Hangman** to launch an arena run.
5. **Survive the arena** — clear escalating waves, read enemy telegraphs, dodge, and burst down the boss.
6. **Collect loot** — gear, gold, and materials drop and persist to your character automatically.
7. **Return to the Hub** — craft upgrades at the Forge, level professions, re-slot abilities.
8. **Repeat** with stronger gear, higher hero mastery, and a sharper rotation.

### Controls

| Input | Action |
|-------|--------|
| **W A S D** | Move |
| **Left Shift** (while moving) | Sprint |
| **Space** | Jump |
| **Left Alt** or **V** | Dodge roll — 2 charges, 0.35 s of full invulnerability, 5 s recharge each |
| **1 – 4** | Select an equipped ability slot |
| **Left Mouse** | Aim and cast the selected ability. *Hold* to charge — damage and area scale up to 3× |
| **Right Mouse** / **Esc** | Cancel the held or aimed ability |
| **Tab** | Open the Spellbook and equip abilities into slots 1–4 |
| **F** | Gather from / interact with a resource node |
| **E** | Interact with an NPC (Forge Master, The Hangman) |
| **Enter** | Open chat |

### Progression — three parallel tracks

| Track | Earned by | Payoff |
|-------|-----------|--------|
| **Hero Mastery** | Clearing waves and bosses (XP scales with wave number) | Per-hero passive bonuses — damage, healing, cooldown reduction, max HP — applied through `CharacterStats` |
| **Professions** | Gathering + crafting: Woodcutting, Fishing, Mining | Unlocks higher-tier smelt and craft recipes — see [AFK Professions](#afk-professions) |
| **Gear** | Loot drops + Forge crafting | Stat bonuses and consumables; some flasks (e.g. boss resist kits) are **craft-exclusive** and never drop |

Power comes primarily from **gear, hero mastery, and player skill** — reading telegraphs, landing skill shots, and sequencing abilities across the party.

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

<img src="Docs/icons/classes/class-warden.png" width="100%"/>

<p align="center"><img src="Docs/models/class-warden/class-warden.webp" width="200"/><br/><sub><b>Warden — 3D model</b> · <a href="Docs/models/class-warden/class-warden.glb">⬇ GLB</a></sub></p>

*"Hold the line. The constructs will do the rest."*

The Warden is a battlefield engineer who wins through positioning and attrition. They deploy runic sentinel turrets, lay snare traps, redirect their constructs mid-fight, and pulse cooldowns for the whole team with Battle Hymn. Their ultimate, Conjurer's Surge, triggers every active construct simultaneously for burst rounds.

**Passive:** Runic Mastery — each construct the Warden controls increases ability damage by a flat bonus.  
**Playstyle:** Plant turrets before the pull, snare the lead enemy, redirect fire to priority targets, hymn when the team is burning CDs.  
**Synergies:** Ironclad pulls enemies into Warden's snare fields; Cleric keeps the turrets' owner alive long enough to matter.

### Ironclad

<img src="Docs/icons/classes/class-ironclad.png" width="100%"/>

<p align="center"><img src="Docs/models/class-ironclad/class-ironclad.webp" width="200"/><br/><sub><b>Ironclad — 3D model</b> · <a href="Docs/models/class-ironclad/class-ironclad.glb">⬇ GLB</a></sub></p>

*"Let them hit me. I'm building a debt they can't pay."*

The Ironclad defines the frontline. Counter Blow turns incoming damage into a cone burst; Gravity Slam pulls a mob into a single point for the team to cleanse; Stalwart Stance becomes a damage sponge while tripling Threat generation. Iron Rampart raises an impenetrable stone wall that stops all projectiles for 10 seconds.

**Passive:** Iron Will — each hit taken in Stalwart Stance generates a Fortify stack that reduces the cooldown of Shieldwall Charge.  
**Playstyle:** Initiate with Shieldwall Charge, Gravity Slam to bunch, hold Counter Blow stance while the team burns, Iron Rampart to split ranged encounters.  
**Synergies:** Shadowblade silences enemies in Gravity Slam's kill zone; Arcanist's chain lightning bounces between the bunched mob; Warden's sentinel fires into a clump.

### Shadowblade — Bo-gar

<img src="Inspiration ART/Hero Bo-Gar.png" alt="Bo-gar" width="180"/>

<img src="Docs/icons/classes/class-shadowblade.png" width="100%"/>

<p align="center"><img src="Docs/models/class-shadowblade/class-shadowblade.webp" width="200"/><br/><sub><b>Bo-gar — 3D model</b> · <a href="Docs/models/class-shadowblade/class-shadowblade.glb">⬇ GLB</a></sub></p>

*"They can't hit what they can't see. Or silence."*

The Shadowblade is a precision assassin and soft CC specialist. Shadow Veil into a stealth-boosted Mind Spike is the signature opener. Silence Ward drops a fog field that stops all enemy abilities. Dark Harvest consumes all active debuff stacks on nearby enemies for massive AoE — ideally after Ironclad has applied Threat stacks and Cleric has applied Cursed.

**Passive:** Phantom Discipline — landing an attack from stealth resets Shadow Veil's cooldown (once per veil).  
**Playstyle:** Stealth → Mind Spike burst, plant Silence Ward on the caster mob, rotate AoE into Binding Wave to root, harvest stacks for the finisher.  
**Synergies:** Ironclad applies Threat stacks (debuffs), Silence Ward stacks Cursed; Shadowblade Dark Harvest converts all of it to damage.

### Cleric — Brandolf

<img src="Inspiration ART/Hero Brandolf.png" alt="Brandolf" width="180"/>

<img src="Docs/icons/classes/class-cleric.png" width="100%"/>

<p align="center"><img src="Docs/models/class-cleric/class-cleric.webp" width="200"/><br/><sub><b>Brandolf — 3D model</b> · <a href="Docs/models/class-cleric/class-cleric.glb">⬇ GLB</a></sub></p>

*"The fight ends when I say it ends."*

The Cleric is the team's life insurance. Spirit Wisps drift and seek allies. Sacred Aegis grows stronger as the target takes hits. Soul Bond reroutes incoming damage onto the Cleric themselves as a sacrifice. Divine Spark revives a downed teammate — or detonates holy energy on undead. Temporal Grace is the most powerful ability in the game: full-team time rewind.

**Passive:** Grace Under Fire — when the Cleric's HP drops below 30%, all active heal abilities tick at double rate for 8 seconds.  
**Playstyle:** Stay mobile, maintain Spirit Wisps rotation, Soul Bond the squishiest carry, save Divine Spark for revives, never waste Temporal Grace.  
**Synergies:** Every class benefits from Temporal Grace. Sacred Aegis pairs with Ironclad when he's in Stalwart Stance absorbing the most hits.

### Arcanist

<img src="Docs/icons/classes/class-arcanist.png" width="100%"/>

<p align="center"><img src="Docs/models/class-arcanist/class-arcanist.webp" width="200"/><br/><sub><b>Arcanist — 3D model</b> · <a href="Docs/models/class-arcanist/class-arcanist.glb">⬇ GLB</a></sub></p>

*"Distance is an illusion. So is the concept of 'safe.'"*

The Arcanist controls space. Arcane Step is a true blink — it bypasses terrain, colliders, enemy hitboxes. Void Maw opens a singularity that drags enemies in before detonating. Forked Lightning chains between four targets. Collapsing Void is the team's hardest-hitting ability: an event horizon that pulls for 3 seconds then collapses for 60 AoE with the Weakened debuff applied.

**Passive:** Phase Resonance — each blink (Arcane Step) within 4 seconds of the last one reduces Void Maw's cooldown by 2 seconds (stacks up to 3 times).  
**Playstyle:** Open with Arcane Step to flank, drop Void Maw to pull the pack, chain lightning the clump, blink out when the mob turns, Collapsing Void when the full team is in position.  
**Synergies:** Void Maw pull combines with Ironclad's Gravity Slam for near-instant bunching; Warden's turrets fire into the singularity zone automatically.

---

## Spellbook — All 32 Abilities

Every class draws from the **shared pool (0–7)** plus its own kit. Equip any four abilities into slots **1–4** via the Spellbook (**Tab**). Indices are fixed and mirrored server-side — never renumber.

**Shape** describes how an ability is delivered:

| Shape | Meaning |
|-------|---------|
| **Skill Shot** | Travels in a straight line — must be aimed and led onto moving targets |
| **Line** | Sweeps everything along a straight path in front of you |
| **Cone** | Bursts in a wedge in your facing direction |
| **AoE** | Placed circle at a target point on the ground |
| **Deploy** | Leaves a persistent object — turret, trap, wall, ward, or wisps |
| **Blink** | Instantly relocates you, ignoring terrain and colliders |
| **Self / Target / Team** | Affects you, a single ally/enemy, or the whole party |

Damage values are single-hit unless noted; ranges like `15–45` are uncharged → fully charged.

### Shared Pool (indices 0–7 — available to all classes)

| | # | Ability | Type | Shape | Damage | CD | Description |
|---|---|---------|------|-------|--------|----|-------------|
| <img src="Docs/icons/runic-sentinel.png" width="120"/> | 0 | **Runic Sentinel** | Support | Deploy | — | 6s | Deploys a stationary runic turret that fires void bolts at nearby enemies until destroyed. |
| <img src="Docs/icons/void-bolt.png" width="120"/> | 1 | **Void Bolt** | Damage | Skill Shot | 15–45 | 3s | Fires a skill-shot bolt of void energy. Charge up to triple damage — you must aim and dodge to use it well. |
| <img src="Docs/icons/mending-circle.png" width="120"/> | 2 | **Mending Circle** | Heal | AoE | — | 5s | Inscribes a glowing rune circle on the ground that heals all allies standing inside it. |
| <img src="Docs/icons/storm-lash.png" width="120"/> | 3 | **Storm Lash** | Damage | Line | 15–50 | 4s | Unleashes a rushing wall of storm energy in a line, damaging all enemies it passes through. |
| <img src="Docs/icons/ember-surge.png" width="120"/> | 4 | **Ember Surge** | Damage | AoE | 20–45 | 4s | Detonates a burst of fire at the target point, scorching all enemies caught in the blast. |
| <img src="Docs/icons/mind-spike.png" width="120"/> | 5 | **Mind Spike** | Damage | AoE | 35 | 5s | Sends a focused psychic spike to the target point, dealing heavy single-target damage. |
| <img src="Docs/icons/binding-wave.png" width="120"/> | 6 | **Binding Wave** | Damage | AoE | 15 | 6s | Releases a wide void pulse that damages and Binds all enemies in range, rooting them in place. |
| <img src="Docs/icons/arcane-ward.png" width="120"/> | 7 | **Arcane Ward** | Support | Self | 50 absorb | 8s | Instantly wraps you in an arcane barrier absorbing up to 50 damage. Expires after 5 seconds. |

### Warden (indices 8–12)

| | # | Ability | Type | Shape | Damage | CD | Description |
|---|---|---------|------|-------|--------|----|-------------|
| <img src="Docs/icons/runic-snare.png" width="120"/> | 8 | **Runic Snare** | Damage | Deploy | 40 | 5s | Places an armed rune trap at the target point. Detonates in a burst when an enemy walks over it. |
| <img src="Docs/icons/battle-hymn.png" width="120"/> | 9 | **Battle Hymn** | Support | AoE | — | 12s | Channels a rallying war hymn that reduces ability cooldowns for all nearby allies. |
| <img src="Docs/icons/spirit-redirect.png" width="120"/> | 10 | **Spirit Redirect** | Support | Target | — | 8s | Commands your active Runic Sentinel to abandon its post and focus fire on your target. |
| <img src="Docs/icons/mend.png" width="120"/> | 11 | **Mend** | Heal | Target | — | 6s | Channels restorative energy into a single ally, healing wounds and purging all active debuffs. |
| <img src="Docs/icons/conjurers-surge.png" width="120"/> | 12 | **Conjurer's Surge** | Support | Self | — | 45s | Surges all your active deployed constructs simultaneously, triggering them at full power at once. |

### Ironclad (indices 13–18)

| | # | Ability | Type | Shape | Damage | CD | Description |
|---|---|---------|------|-------|--------|----|-------------|
| <img src="Docs/icons/counter-blow.png" width="120"/> | 13 | **Counter Blow** | Support/Damage | Cone | up to 60 | 10s | Enters an absorption stance for 3 seconds. Releasing unleashes all absorbed damage as a cone burst. |
| <img src="Docs/icons/gravity-slam.png" width="120"/> | 14 | **Gravity Slam** | Support | AoE | — | 7s | Slams the ground with gravitational force, pulling all nearby enemies into the impact point. |
| <img src="Docs/icons/shieldwall-charge.png" width="120"/> | 15 | **Shieldwall Charge** | Damage | Line | 25 | 6s | Charges forward, slamming through enemies for 25 damage and generating Threat stacks on each hit. |
| <img src="Docs/icons/stalwart-stance.png" width="120"/> | 16 | **Stalwart Stance** | Support | Self | — | 14s | Plants your feet: 40% damage reduction and tripled Threat generation for 6 seconds. Cannot move. |
| <img src="Docs/icons/rune-chain.png" width="120"/> | 17 | **Rune Chain** | Support | Target | — | 9s | Etches a runic leash onto one enemy for 5 seconds, absorbing 15% of attacks they land on allies. |
| <img src="Docs/icons/iron-rampart.png" width="120"/> | 18 | **Iron Rampart** | Support | Deploy | — | 50s | Raises a massive stone rune wall in front of you that blocks all projectiles for 10 seconds. |

### Arcanist (indices 19–22)

| | # | Ability | Type | Shape | Damage | CD | Description |
|---|---|---------|------|-------|--------|----|-------------|
| <img src="Docs/icons/arcane-step.png" width="120"/> | 19 | **Arcane Step** | Support | Blink | — | 4s | Phase-shifts your body to the targeted location, bypassing terrain and enemy colliders. |
| <img src="Docs/icons/void-maw.png" width="120"/> | 20 | **Void Maw** | Damage | AoE | 20 | 9s | Opens a singularity that pulls all enemies inward for 3 seconds, then detonates in a burst of void energy. |
| <img src="Docs/icons/forked-lightning.png" width="120"/> | 21 | **Forked Lightning** | Damage | AoE | 30 chain | 7s | Unleashes chain lightning that arcs between up to 4 enemies (30 / 25 / 20 / 15 damage per jump). |
| <img src="Docs/icons/collapsing-void.png" width="120"/> | 22 | **Collapsing Void** | Damage | AoE | 60 | 50s | Summons a massive event horizon. Pulls for 3 seconds, then collapses for 60 AoE and applies Weakened. |

### Cleric — Brandolf (indices 23–28)

| | # | Ability | Type | Shape | Damage | CD | Description |
|---|---|---------|------|-------|--------|----|-------------|
| <img src="Docs/icons/soul-bond.png" width="120"/> | 23 | **Soul Bond** | Support | Target | — | 9s | Bonds with a nearby ally for 5 seconds, rerouting all incoming damage dealt to them onto you instead. |
| <img src="Docs/icons/spirit-wisps.png" width="120"/> | 24 | **Spirit Wisps** | Heal | Deploy | — | 7s | Releases drifting wisps that seek out nearby allies to heal them and chip enemies they pass through. |
| <img src="Docs/icons/divine-spark.png" width="120"/> | 25 | **Divine Spark** | Heal/Damage | Target | 60 (undead) | 14s | Revives a downed ally at 30% HP — or, if cast on undead enemies, detonates for 60 holy damage. |
| <img src="Docs/icons/sacred-aegis.png" width="120"/> | 26 | **Sacred Aegis** | Support | Target | — | 10s | Places a living shield on an ally that grows stronger (up to 80 absorb) as they take hits over 8 seconds. |
| <img src="Docs/icons/dispel.png" width="120"/> | 27 | **Dispel** | Support | Target | — | 7s | Instantly purges every active debuff from a target ally, no matter how many are stacked. |
| <img src="Docs/icons/temporal-grace.png" width="120"/> | 28 | **Temporal Grace** | Heal | Team | — | 60s | Rewinds the entire team 5 seconds, restoring their HP, positions, and clearing all debuffs gained since then. |

### Shadowblade — Bo-gar (indices 29–31)

| | # | Ability | Type | Shape | Damage | CD | Description |
|---|---|---------|------|-------|--------|----|-------------|
| <img src="Docs/icons/shadow-veil.png" width="120"/> | 29 | **Shadow Veil** | Support | Self | — | 10s | Vanishes into full invisibility for 4 seconds. Breaking stealth immediately with Mind Spike deals +50% bonus damage. |
| <img src="Docs/icons/silence-ward.png" width="120"/> | 30 | **Silence Ward** | Support | Deploy | — | 12s | Plants a cursed fog field that silences all enemy abilities and applies Cursed (DoT) while they remain inside. |
| <img src="Docs/icons/dark-harvest.png" width="120"/> | 31 | **Dark Harvest** | Damage | AoE | 20/stack | 40s | Consumes all active debuff stacks on nearby enemies, dealing 20 damage per stack consumed. |

---

## Combat System

<img src="Docs/icons/combat/combat-directional.png" width="100%"/>

> *Every shot is aimed. A charged Void Bolt streaks into a pack while the AoE indicator burns on the floor — no auto-attack, no tab-target, just placement and timing.*

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

<img src="Docs/icons/combat/combat-telegraph-dodge.png" width="100%"/>

> *The red circle is the telegraph; the blue shimmer is dodge-roll i-frames. Read the tell, roll through it — the slam lands on empty ground.*

### Server Authority

All game-state mutations run server-only (`[Server]` attribute). Clients receive visual feedback via `[ClientRpc]`. Client-only code (VFX, HUD, tooltips) is behind `#if !UNITY_SERVER`. Players are never trusted — damage is calculated server-side, kill rewards issued atomically.

---

### State Layer — DoTs, HoTs & Transitions

> **Design status:** full design is authored in `CrossWorlds/_context/COMBAT_PROPOSAL.md`. Mechanics below are the confirmed design — implementation follows the proposal's code notes. Nothing new in code yet.

The base combat is burst-oriented (build resource → detonate → reset). The state layer adds a **persistent consequence plane** so players also track what states are active on enemies and allies simultaneously. Same 32 ability slots. No new buttons. All states are visible on `StatusEffectHUD`.

<img src="Docs/icons/combat/combat-state-layer.png" width="100%"/>

> *Two planes at once: enemies rotting under green Void Rot and orange Burning DoTs on the left, allies bathed in a violet Mending Circle HoT on the right. You manage both at the same time.*

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

<img src="Docs/icons/combat/party-synergy.png" width="100%"/>

> *Alone, each hero is strong. Together, the seams between their kits become weapons: the Arcanist seeds decay the Shadowblade harvests, the Ironclad bunches the pack the Warden's turrets shred, and the Cleric makes every reckless play survivable. The best plays live in these overlaps.*

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

## AFK Professions

<img src="Docs/icons/characters/forge-crafting.png" width="100%"/>

> *Between runs, the real work: ore into ingots, ingots into gear, the anvil ringing under the rose window. Some of the strongest tools in the game are never dropped — only forged.*

Press **F** once at a gathering station, then go make a coffee. The game rewards you while you're idle.

Every `tickInterval` seconds the station awards one item directly to your inventory (via `/api/inventory/add-item`) and posts profession XP to the server (via `POST /api/professions/award-xp`). Moving more than 4 units away, pressing **F** again, or pressing **Escape** cancels the session. Level gates prevent high-tier nodes until you've earned them.

---

### Professions

<table>
<tr>
<td align="center"><img src="Docs/icons/professions/mining.png" width="80"/><br/><b>Mining</b><br/>ID 2</td>
<td align="center"><img src="Docs/icons/professions/woodcutting.png" width="80"/><br/><b>Woodcutting</b><br/>ID 0</td>
<td align="center"><img src="Docs/icons/professions/fishing.png" width="80"/><br/><b>Fishing</b><br/>ID 1</td>
</tr>
</table>

XP formula (matches web panel): **xpToNextLevel = currentLevel × 50**
(Level 1→2: 50 xp · Level 5→6: 250 xp · Level 10→11: 500 xp)

---

### Resource Tiers

<table>
<tr>
  <th>Profession</th>
  <th>Tier</th>
  <th>Station</th>
  <th>Item</th>
  <th>Level Req</th>
  <th>Tick</th>
  <th>XP/tick</th>
  <th>Bonus ×2 at</th>
</tr>
<tr>
  <td rowspan="3"><b>Mining</b></td>
  <td>1</td>
  <td>Copper Vein</td>
  <td><img src="Docs/icons/professions/ore-copper.png" width="28"/> ore_copper</td>
  <td>1</td>
  <td>5s</td>
  <td>10</td>
  <td>Lv 10</td>
</tr>
<tr>
  <td>2</td>
  <td>Iron Vein</td>
  <td><img src="Docs/icons/professions/ore-iron.png" width="28"/> ore_iron</td>
  <td>5</td>
  <td>6s</td>
  <td>20</td>
  <td>Lv 15</td>
</tr>
<tr>
  <td>3</td>
  <td>Gold Vein</td>
  <td><img src="Docs/icons/professions/ore-gold.png" width="28"/> ore_gold</td>
  <td>15</td>
  <td>8s</td>
  <td>40</td>
  <td>Lv 25</td>
</tr>
<tr>
  <td rowspan="1"><b>Woodcutting</b></td>
  <td>1</td>
  <td>Oak Tree</td>
  <td><img src="Docs/icons/professions/log-oak.png" width="28"/> log_oak</td>
  <td>1</td>
  <td>6s</td>
  <td>10</td>
  <td>Lv 10</td>
</tr>
<tr>
  <td rowspan="1"><b>Fishing</b></td>
  <td>1</td>
  <td>Fishing Spot</td>
  <td><img src="Docs/icons/professions/fish-river.png" width="28"/> fish_river</td>
  <td>1</td>
  <td>8s</td>
  <td>8</td>
  <td>Lv 10</td>
</tr>
</table>

---

### Gathering HUD

While AFK gathering a minimal HUD appears at the bottom of the screen:

```
⛏  Copper Vein                          [STOP]
    ore_copper          Next: 3.2s   ████████░░
```

- Progress bar fills toward next item reward
- Flashes yellow on yield (`+1× ore_copper`)
- Flashes and shows `🎉 Mining level 5!` on level-up
- **STOP** button or **F / Escape** cancels the session

---

### AFK Loop — How It Works

```
Player presses F at station
        │
        ▼
Level check (ProfessionManager.GetLevel)
        │
    Level OK?──No──► "Requires Mining level X" in chat
        │
       Yes
        │
        ▼
Gather coroutine starts (every tickInterval seconds):
  1. POST /api/inventory/add-item     → item saved to DB
  2. POST /api/professions/award-xp   → XP saved, level-up checked
  3. GatheringHUD.Pulse(qty)          → bar flashes, counter updates
  4. 20% chance bonus ×2 at high level
        │
        ▼
Cancel triggers:
  • Player drifts > 4u from start position
  • F key or Escape
  • STOP button in HUD
```

---

### Backend — `POST /api/professions/award-xp`

New VPS endpoint. Code in [`_CONTEXT/professions-award-xp-patch.js`](_CONTEXT/professions-award-xp-patch.js) — insert near `GET /api/professions/:characterId` in `server.js`.

```
POST /api/professions/award-xp   (requireJWT)
Body: { characterId, professionId, xpAmount }
Returns: { success, data: { skill_level, skill_xp, leveled_up, profession_id } }
```

- Upserts the `professions` row (safe for first-time use)
- XP is capped 1–500 per call (anti-exploit)
- Level-up loop handles multi-level gains at low XP totals
- Level-up logged server-side: `[PROF] char 1 — Mining leveled up to 5`

**Apply to VPS:**
```bash
# On VPS: insert the patch block into server.js, then restart
sudo nano /opt/crossworlds-auth/server.js   # paste near GET /api/professions route
sudo systemctl restart crossworlds-auth
curl -s http://localhost:3000/api/professions/1   # verify endpoint responds
```

---

### Implementation Files

| File | Purpose |
|------|---------|
| [`Assets/Game/Systems/AfkGatheringStation.cs`](Assets/Game/Systems/AfkGatheringStation.cs) | AFK loop — F to start, auto-award per tick, drift cancel |
| [`Assets/Game/Systems/ProfessionManager.cs`](Assets/Game/Systems/ProfessionManager.cs) | Client singleton — loads/caches profession levels, posts XP, fires level-up event |
| [`Assets/Game/UI/GatheringHUD.cs`](Assets/Game/UI/GatheringHUD.cs) | Screen-space HUD — progress bar, pulse on yield, level-up flash |
| [`Assets/Game/Editor/AfkStationBuilder.cs`](Assets/Game/Editor/AfkStationBuilder.cs) | BCE menu: 8a–8f drop stations into Hub scene |
| [`_CONTEXT/professions-award-xp-patch.js`](_CONTEXT/professions-award-xp-patch.js) | VPS patch — `POST /api/professions/award-xp` |
| `Assets/Game/Combat/Scripts/ResourceNode.cs` | Legacy manual F-key harvest (still valid for one-shot depleting nodes) |

**Editor steps to wire it up:**
1. Open Unity → Hub scene
2. `BCE/Setup/8f` — adds `ProfessionManager` to scene
3. `BCE/Setup/8a` through `8e` — drops each station mesh into the scene; position as desired
4. Call `ProfessionManager.Local.Load()` from your hub `OnStartClient` (or `LoginManager` post-login callback)
5. Apply VPS patch → `sudo systemctl restart crossworlds-auth`

---

## Encounters & NPCs

<img src="Docs/icons/characters/arena-entry.png" width="100%"/>

> *The portal spits you onto cracked rune-stone, and the fog is already moving. No countdown, no safe first step — the arena starts the instant your boots land.*

### NPCs — Hub World

<img src="Docs/icons/characters/hub-sanctuary.png" width="100%"/>

> *Between runs, the arena falls silent and the city breathes again. Forge-fire and lantern-light push back the fog; adventurers trade rumours over the anvil while the portal turns, patient, at the far wall.*

Two persistent NPCs live in the Hub world and drive the crafting and trading loop.

| NPC | Script | Role |
|-----|--------|------|
| **Forge Master** | `ForgeNPC.cs` | Crafting station — material → gear recipes, interacts via `INPCInteractable` |
| **The Hangman** | `HangmanNPC.cs` | Travelling merchant — rare item trades, triggers dialogue UI before arena entry |

<table>
<tr>
<td align="center"><img src="Docs/icons/characters/npc-forge.png" width="200"/><br/><b>Forge Master</b></td>
<td align="center"><img src="Docs/icons/characters/npc-hangman.png" width="200"/><br/><b>The Hangman</b></td>
</tr>
</table>

**NPC architecture:**  
- All hub NPCs implement `INPCInteractable` — proximity-trigger interaction, handled by `NPCInteractionManager`
- `NpcController.cs` drives idle breathing + slow patrol within `patrolRadius`
- Animated via `Animator` Speed parameter (0 = idle, 1 = walk)
- No Health/Mirror dependency — hub NPCs are purely cosmetic/interactive

---

### Enemy Roster

<img src="Docs/icons/characters/wave-assault.png" width="100%"/>

> *The Void does not tire, and it never comes alone. Grunts crash in first, archers crackle behind them, and somewhere in the fog a Knight is already moving. Watch the red circles — everything that kills you announces itself first.*

Three enemy variants spawn in arenas. All are server-authoritative via `EnemyController.cs`.

<table>
<tr>
<td align="center">
  <img src="Docs/models/enemy-grunt/enemy-grunt.webp" width="200"/><br/>
  <sub><i>3D model render</i></sub><br/>
  <img src="Docs/icons/characters/enemy-grunt.png" width="200"/><br/>
  <b>Void Grunt</b><br/>
  <a href="Docs/models/enemy-grunt/enemy-grunt.glb">⬇ GLB</a>
</td>
<td align="center">
  <img src="Docs/models/enemy-ranged/enemy-ranged.webp" width="200"/><br/>
  <sub><i>3D model render</i></sub><br/>
  <img src="Docs/icons/characters/enemy-ranged.png" width="200"/><br/>
  <b>Void Archer</b><br/>
  <a href="Docs/models/enemy-ranged/enemy-ranged.glb">⬇ GLB</a>
</td>
<td align="center">
  <img src="Docs/models/enemy-elite/enemy-elite.webp" width="200"/><br/>
  <sub><i>3D model render</i></sub><br/>
  <img src="Docs/icons/characters/enemy-elite.png" width="200"/><br/>
  <b>Void Knight (Elite)</b><br/>
  <a href="Docs/models/enemy-elite/enemy-elite.glb">⬇ GLB</a>
</td>
</tr>
</table>

| Type | Prefab | Role | Aggro | Attack | Damage | XP |
|------|--------|------|-------|--------|--------|----|
| **Void Grunt** | `Enemy_Grunt` | Melee frontline | 8u | 1.5s | 12 | 20 |
| **Void Archer** | `Enemy_Ranged` | Ranged kite | 8u | 2s | 10 | 20 |
| **Void Knight** | `Enemy_Elite` | Elite tank | 10u | 2s | 20 | 50 |

**Enemy AI state machine** (`EnemyController.cs`):

```
Idle → (player enters aggroRadius) → Chase → (in attackRange) → Attack
              ↑ leashRadius exceeded ←──────────────────────────────────┘
```

- **0.45s attack telegraph** — red cylinder AoE preview before damage lands (dodge it)
- Ranged enemies backpedal if the player closes inside `tooCloseDistance = 3u`
- Respects all status effects: Stagger (interrupt), Bound (immobile), Silenced (can't attack), Slow (speed multiplied)
- Death: VFX burst → loot rolls → `POST /api/combat/kill` for server-authoritative XP → `NetworkServer.Destroy`

**Wave composition** (`WaveSpawner.cs`):
- 67% Grunts / 33% Archers per wave
- +2 enemies per wave (Wave 1 = 4, Wave 5 = 12, Wave 10 = 22)
- Elite spawns every 3rd wave; wave waits for all enemies dead before advancing
- Max 10 waves → arena clear bonus (+200 mastery XP)

---

### Fields of Gundab — Field Goul

<sub>New enemy zone — 2026-07-04 content update.</sub>

The **Fields of Gundab** are the first themed enemy zone beyond the void arenas, with a fully-animated signature foe: the **Field Goul**, a lurching brute.

| Enemy | Model | Animations | Driver |
|-------|-------|------------|--------|
| **Field Goul** | `Assets/Game/Enemies/Fields of Gundab/Field Goul/` (Tripo model + basecolor) | Idle · Run · Punch · Scream · Death | `FieldGoulAnimationDriver.cs` |

- **`FieldGoulAnimationDriver`** drives the Animator from movement + combat state: a `Speed` float (blends Idle↔Run from actual velocity), `Attack` / `Scream` / `Die` triggers, and an `IsDead` bool. It reads the shared `Health` (`onDeath` → death animation, `IsAlive` gate) and disables `EnemyAI` on death, so the Goul plugs into the same server-authoritative combat as the void roster.
- **Animator controller** (`Field Goul/Controller/Field_Goul.controller`) is generated by the **`FieldGoulAnimatorBuilder`** editor script (BCE menu) — clip import flags, states, and parameters are reproducible from code.
- **Environment:** a wooden-fence set dresses the fields (`Assets/Game/3d Assets/Fences/`), reproducible via `Tools/generate_wooden_fence_fbx.py`.

> **Editor step:** run `FieldGoulAnimatorBuilder`, place the Field Goul prefab, and add it to a wave/spawn set + bake NavMesh. Model, animations, and driver are in the repo; prefab wiring is a Unity step.

---

### Cursed Chest Encounter

<img src="Docs/icons/characters/wave-chest.png" width="300" align="right"/>

The **Wave Chest** (`WaveChest.cs`) is a special encounter placed in arena rooms. Hold-to-open (2s) → spawns a custom enemy roster → loot on completion.

- Scales enemy count to nearby player count
- Prep window lets players position Warden turrets before first wave
- Drops configured loot on all-clear

---

### Boss Encounter — The Null Architect

<img src="Docs/icons/characters/boss-arena-scene.png" width="100%"/>

<table>
<tr>
<td align="center" width="50%">
  <img src="Docs/models/null-architect/null-architect.webp" width="100%"/><br/>
  <sub><i>3D model — Tripo PBR render</i></sub><br/>
  <a href="Docs/models/null-architect/null-architect.glb">⬇ Download GLB</a>
</td>
<td align="center" width="50%">
  <img src="Docs/icons/characters/null-architect-phase1.png" width="100%"/><br/>
  <img src="Docs/icons/characters/null-architect-phase2.png" width="100%"/><br/>
  <img src="Docs/icons/characters/null-architect-phase3.png" width="100%"/><br/>
  <sub><i>Phase 1 · Phase 2 · Phase 3</i></sub>
</td>
</tr>
</table>

> *"The Null Architect did not build the void — it was built by it."*

The Null Architect is Crossworlds BCE's world boss. It is a fully scripted 3-phase encounter implemented in `WorldBossController.cs`. The fight is server-authoritative: all ability timing, phase transitions, and damage are computed on the dedicated server and broadcast to clients via Mirror RPCs.

**Entry:** A `BossTrigger` child collider (radius 15u) starts the fight the moment the first player steps inside. Once started, the encounter cannot be reset until the boss dies or the server restarts.

---

#### Phase 1 — The Mirror (100% → 60% HP)

<img src="Docs/icons/characters/null-architect-phase1.png" width="260" align="right"/>

The Null Architect fights at full strength. Every 18 seconds it enters a **Reflect window**.

| Timing | Event |
|--------|-------|
| T−3s | Chat warning: *"REFLECT in 3s — stop attacking!"*; telegraph VFX pulses |
| T=0 | `isReflecting = true` — **stop all DPS** |
| T+4s | Reflect window closes — resume attack |

**Mechanic:** Any damage dealt to the boss during the 4-second window is reflected as an AoE pulse hitting all players within 10u for **75% of that damage**. A single Collapsing Void (60 dmg) during reflect hits every teammate for 45 damage.

**Counter:** Watch chat + VFX. Cease fire on the warning. Repositioning and dodge-rolling are safe.

**Phase transition at 60% HP:**
- 4-second immunity window (`isImmune = true`)
- Chat: *"PHASE SHIFT — Null Architect fragments into shards!"*
- All Phase 1 coroutines stopped cleanly

---

#### Phase 2 — Shard Fracture (60% → 30% HP)

<img src="Docs/icons/characters/null-architect-phase2.png" width="260" align="right"/>

Three **Null Shards** spawn at 6u spacing from the boss. Each has 400 HP and must be destroyed **simultaneously** — surviving shards cross-heal for 50% of damage dealt to their sibling.

| Shard Count | Cross-heal per hit |
|-------------|-------------------|
| 3 alive | Others heal 50% of damage received |
| 2 alive | Survivor heals 50% of damage to dead sibling |
| 1 alive | Safe to kill freely |

Every 25 seconds the boss casts **Tether Web**:
- Randomly pairs all players
- Each pair must stay within **6u** of each other for 6 seconds
- Snapping the tether deals **40 damage to both** tethered players

**Strategy:** Coordinate DPS on shards (burst simultaneously), stack with your tether partner, then swap focus to the boss.

**Phase transition at 30% HP:**
- 4-second immunity window
- Chat: *"CRITICAL — Null Architect destabilises! All damage amplified!"*
- Boss gains `Weakened` status effect → **+25% incoming damage** for the rest of the fight

---

#### Phase 3 — Null Collapse (30% → 0% HP)

<img src="Docs/icons/characters/null-architect-phase3.png" width="260" align="right"/>

No more shards. The boss is weakened (+25% dmg taken) but dangerous — Void Drain punishes spread.

Every 12 seconds: **Void Drain**

| Zone | Effect |
|------|--------|
| Within 5u of boss | Safe |
| Beyond 5u | 8 damage/second for 4 seconds |

Players must stack on the boss during Void Drain, then spread for Tether safety between casts. Void Drain and Tether Web can overlap — coordinate both.

**At 10% HP: Final Surge** (one-time)

- Chat: *"⚠⚠ FINAL SURGE — Null Architect ENRAGED! Burn it down NOW!"*
- Boss movement speed ×3, attack speed ×3
- Lasts 15 seconds — if the boss survives the surge it returns to normal
- Burn everything; cooldowns should be saved for this window

---

#### Boss Loot Table

| Item | Drop Type | Chance |
|------|-----------|--------|
| `sword_iron` | Guaranteed | 100% |
| `plate_iron` | Guaranteed | 100% |
| `ring_copper` | Rare | 35% |
| `material_copper_bar` | Rare | 35% |

#### Boss HP Bar (Client UI — `WorldBossHealthBar.cs`)

- Phase markers drawn at 60% and 30% HP thresholds
- Bar color changes by phase: **cyan** (P1) → **orange** (P2) → **red** (P3) → **white** (immune/transition)
- Driven by `SyncVar` hook `OnPhaseSync` → `WorldBossHealthBar.OnPhaseChanged(phase)`

---

#### Boss Quick Reference Card

```
The Null Architect — 2000 HP · 3 Phases · Server-Authoritative

Phase 1 (100–60%)   Reflect Pulse every 18s — stop DPS during 4s window
                    Transition: 4s immune → 3 Null Shards spawn

Phase 2 (60–30%)    3 Shards (400HP each) — burst simultaneously or cross-heal
                    Tether Web every 25s — stay within 6u of your pair
                    Transition: 4s immune → Boss gains Weakened (+25% dmg taken)

Phase 3 (30–0%)     Void Drain every 12s — stack within 5u or 8dps for 4s
                    Final Surge at 10% — speed×3, atk×3 for 15s (burn window)

Loot:               sword_iron + plate_iron (guaranteed)
                    ring_copper, material_copper_bar (35% each)
```

---

#### Implementation Files

| File | Purpose |
|------|---------|
| [`Assets/Game/Combat/Scripts/WorldBossController.cs`](Assets/Game/Combat/Scripts/WorldBossController.cs) | All phase logic, RPCs, loot |
| [`Assets/Game/Combat/Scripts/WaveSpawner.cs`](Assets/Game/Combat/Scripts/WaveSpawner.cs) | Arena wave manager |
| [`Assets/Game/Combat/Scripts/WaveManager.cs`](Assets/Game/Combat/Scripts/WaveManager.cs) | Flexible wave rosters (boss, elite, mob tiers) |
| [`Assets/Game/Combat/Scripts/EnemyController.cs`](Assets/Game/Combat/Scripts/EnemyController.cs) | Enemy AI state machine |
| [`Assets/Game/Combat/Scripts/StatusEffectManager.cs`](Assets/Game/Combat/Scripts/StatusEffectManager.cs) | Buff/debuff system |
| [`Assets/Game/Combat/Scripts/ArenaSessionController.cs`](Assets/Game/Combat/Scripts/ArenaSessionController.cs) | Session XP/kill tracking |
| [`Assets/Game/Combat/Scripts/WaveChest.cs`](Assets/Game/Combat/Scripts/WaveChest.cs) | Cursed chest encounter |
| [`Assets/Game/UI/WorldBossHealthBar.cs`](Assets/Game/UI/WorldBossHealthBar.cs) | Client phase-aware HP bar |
| [`Assets/Game/Editor/WorldBossBuilder.cs`](Assets/Game/Editor/WorldBossBuilder.cs) | One-click boss prefab builder |
| [`Assets/Game/Editor/ArenaSceneBuilder.cs`](Assets/Game/Editor/ArenaSceneBuilder.cs) | One-click arena scene builder |

---

### Boss Encounter — The Iron Warden

<img src="Docs/icons/characters/iron-warden-arena.png" width="100%"/>

<table>
<tr>
<td width="340" valign="top">
<img src="Docs/models/iron-warden/iron-warden.webp" width="320"/>
<br/><sub><a href="Docs/models/iron-warden/iron-warden.glb">⬇ Download 3D model (.glb)</a></sub>
</td>
<td valign="top">

**The Iron Warden** is a siege-engine colossus — a fortress golem built to outlast anything thrown at it. Where the Null Architect punishes aggression with phase reactions, the Iron Warden punishes passivity: it layers mechanical systems that force the party to make active decisions under pressure, or get ground to dust.

The encounter is implemented in `IronWardenController.cs` following the same server-authoritative pattern as the Null Architect. Phase transitions, ability timing, turret spawns, and Magnet Pulls are all computed on the dedicated server. Clients receive `[ClientRpc]` announcements and VFX triggers; no game-state decisions run client-side.

**Entry:** A `BossArenaTrigger` collider (placed via **BCE/Setup/9b**) fires `IronWardenController.Activate()` the moment any player enters. The encounter cannot be reset once started.

</td>
</tr>
</table>

> *"The Iron Warden was forged to hold. It does not attack you. It simply makes sure you can never leave."*

---

#### Phase 1 — Siege Protocol (100–60% HP)

<img src="Docs/icons/characters/iron-warden-phase1.png" width="260" align="right"/>

The Warden opens in full fortress mode. Its rotating Barrier Wall forces positional discipline while Mortar Strikes punish clustering.

| Timing | Event |
|--------|-------|
| Every 12 s | **Barrier Wall** — a ring of iron shields rotates around the boss. Players in the wrong arc receive 10 dmg/s push-back and reduced damage output. Players on the correct arc deal full damage. |
| Every 20 s | **Mortar Strike** — five red AoE circles appear in sequence across the arena, each detonating 1 second after the previous (30 dmg per blast). Spread out. |

The Barrier Wall arc indicator is a client-side VFX (`RpcSpawnBarrierWall`). Pay attention to it — damage output is halved for anyone on the wrong side.

**Phase transition at 60% HP:**
- 3-second stagger animation
- Chat: *"[BOSS] Shield Matrix online — destroy BOTH turrets simultaneously!"*
- All Phase 1 coroutines stopped cleanly

---

#### Phase 2 — Shield Matrix (60–25% HP)

<img src="Docs/icons/characters/iron-warden-phase2.png" width="260" align="right"/>

The Warden opens its chest cavity and deploys two **Siege Turrets** at opposite flanks. Until both are destroyed at the same time, the boss is completely immune to damage.

| Timing | Event |
|--------|-------|
| Immediate | Two **Siege Turrets** (300 HP each) spawn at ±8u on the X axis. Fire orange bolts at nearby players (10 dmg, every 3 s). |
| If only one turret dies | The surviving turret **auto-repairs** to full HP after 10 seconds. Both must die within seconds of each other. |
| Every 20 s | **Magnet Pull** — all players are yanked toward the boss centre, then a 60-dmg AoE stomp drops in a 8u radius. |

The immunity mechanic demands coordination: split the party, burst both turrets simultaneously, then immediately pivot to the boss before the next Magnet Pull. `SiegeTurretBehaviour.cs` handles turret combat and calls `warden.OnTurretDestroyed()` — the boss tracks count and restores immunity if only one fell.

**Phase transition at 25% HP:**
- Chat: *"[BOSS] The Iron Warden's core is exposed — RAMPAGE!"*
- Boss NavMeshAgent speed multiplied by 1.5×

---

#### Phase 3 — Rampage (25–0% HP)

<img src="Docs/icons/characters/iron-warden-phase3.png" width="260" align="right"/>

Armor cracked open to reveal the forge-fire core. The Warden stops playing defence and tries to physically destroy the party.

| Timing | Event |
|--------|-------|
| Every 4 s | **Ground Slam** — 35 dmg in 6u radius. 0.45s telegraph cylinder before impact. |
| At ≤15% HP | **Lockdown** — all players rooted for 5 seconds while the Warden charges a Devastation Slam (60 dmg, 10u radius). |

The Lockdown fires only once. It is telegraphed with a 1.5-second warning circle. Healers need to pre-stack healing before the root drops — you cannot move during the charge. After Devastation fires, the Warden is at its most vulnerable: this is the burn window.

> **Lockdown sequence:** `LockdownSequence()` applies `StatusEffectManager.ApplyRoot()` to all `PlayerIdentity` objects via server-side `FindObjectsByType`, waits 5 seconds, shows a 1.5s `RpcShowWarningCircle`, then calls `DealAoeDamage()` — all server-side, no client prediction.

---

#### Class Synergies (Iron Warden)

Each class has a specific role this fight pressures:

| Class | Role |
|-------|------|
| **Warden** | Runic Snare + Iron Rampart to hold turret aggro while the ranged party bursts |
| **Ironclad** | Shieldwall Charge through Barrier Wall arc — can maintain damage in wrong-arc zones briefly |
| **Cleric** | Mending Circle pre-staged for Magnet Pull landing zone; Dispel on Magnetized status |
| **Arcanist** | Void Maw + Forked Lightning for simultaneous turret burst — the key to breaking Shield Matrix |
| **Shadowblade** | Shadow Veil to dodge Mortar Strike; silence turrets during repair window |

---

#### Boss Quick Reference Card

```
The Iron Warden — 3000 HP · 3 Phases · Server-Authoritative

Phase 1 (100–60%)  Barrier Wall every 12s — position on correct arc for full damage
                   Mortar Strike every 20s — 5 staggered blasts, spread out
                   Transition: turret deploy announcement

Phase 2 (60–25%)  IMMUNE until both Siege Turrets (300 HP each) die simultaneously
                   One turret alone → surviving turret repairs after 10s
                   Magnet Pull every 20s → yank to centre + 60 AoE stomp
                   Transition: NavMesh speed ×1.5

Phase 3 (25–0%)   Ground Slam every 4s — 35 dmg, 6u radius, 0.45s telegraph
                   At 15% HP: Lockdown (root 5s) → Devastation (60 AoE, one-shot)

Kill:             ArenaSessionController.OnBossKilled() → chest + XP
```

#### Implementation Files

| File | Purpose |
|------|---------|
| [`Assets/Game/Combat/Scripts/IronWardenController.cs`](Assets/Game/Combat/Scripts/IronWardenController.cs) | Boss controller — phases, abilities, SyncVar |
| [`Assets/Game/Combat/Scripts/SiegeTurretBehaviour.cs`](Assets/Game/Combat/Scripts/SiegeTurretBehaviour.cs) | Phase 2 turret — fire loop, death, warden callback |
| [`Assets/Game/Editor/IronWardenBuilder.cs`](Assets/Game/Editor/IronWardenBuilder.cs) | One-click boss + turret + trigger builder |

> **Editor steps:** Run **BCE/Setup/9a** to place the boss, **9b** for the arena trigger, **9c** for turret placeholders. Assign `warningCirclePrefab` in the `IronWardenController` Inspector field. Bake NavMesh for the arena scene before playtesting.

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

### Network-Controlled Objects & Making Mobs

Crossworlds is **server-authoritative**. Anything that moves, deals damage, or has
health — players, enemies, projectiles, deployables — is a *networked object*: it
carries a `NetworkIdentity`, is **created only on the server**, and Mirror mirrors it
down to every client. Clients never instantiate these directly.

**How a mob actually gets into the world:**

```
Server: Instantiate(prefab) → NetworkServer.Spawn(enemy)
                                      │
                        Mirror replicates to every client
                                      │
Client: looks up the prefab in RodNetworkManager ▸ Registered Spawnable Prefabs,
        instantiates its own copy, and keeps it in sync
```

`WaveSpawner.SpawnEnemy()` ([`WaveSpawner.cs:139`](Assets/Game/Combat/Scripts/WaveSpawner.cs))
is the reference path — it runs under `[Server]`, `Instantiate`s the prefab, then calls
`NetworkServer.Spawn`. It never places enemies in the scene by hand.

**Checklist — adding a new mob:**

1. Build the prefab with a **`NetworkIdentity`** + `Health` + `EnemyController` (or an
   AI/animation driver). Model, colliders, and Animator go on/under it as normal.
2. **Register it:** add the prefab to `RodNetworkManager` ▸ **Registered Spawnable
   Prefabs**. If it isn't in this list, the server spawns it but **clients can't
   instantiate it** — it's invisible to everyone but the host.
3. **Spawn it from server code** — add it to a `WaveSpawner.enemyPrefabs` /
   `WaveManager` roster, or `NetworkServer.Spawn` it from your own `[Server]` method.
   Never drag it into the scene to make it appear.
4. Bake NavMesh if it navigates, and set the Animator Avatar (see [Editor Steps](#editor-steps)).

### ⚠ "My model disappears the moment I hit Play"

This is the #1 confusion when adding or previewing character/enemy models. **Cause:**
the model has a `NetworkIdentity` and was **dragged into the scene**. Mirror treats any
scene object with a `NetworkIdentity` as an unspawned network object and **disables it on
Play** until the server explicitly spawns it — so it vanishes.

**How to avoid it:**

- **Previewing a model?** Drag it into the scene to look at it, but **delete it before
  pressing Play**, or work on it in **Prefab Mode** (double-click the prefab) instead of
  the scene. Prefab Mode never triggers the disappear.
- **Making a decorative prop** (statue, tree, fence, non-interactive NPC)? **Do not add a
  `NetworkIdentity`.** Plain scene objects with no `NetworkIdentity` stay put on Play and
  render for everyone — this is correct for anything purely cosmetic.
- **Making an actual mob/enemy?** Keep the `NetworkIdentity`, but **don't place it in the
  scene** — register it and let a spawner create it at runtime (checklist above). Seeing
  it "disappear" in the scene is Mirror working as intended; it will appear in-game when
  spawned.
- **It spawns but only the host sees it?** The prefab isn't in **Registered Spawnable
  Prefabs** on `RodNetworkManager`. Add it there.

> Rule of thumb: **cosmetic = no NetworkIdentity, lives in the scene.
> Gameplay = has NetworkIdentity, is spawned by the server, never hand-placed.**

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

## Design Pillars

- **Harder = more reward** — wave difficulty multiplier feeds the loot score; higher cycles drop rarer gear.
- **Shared common goal** — one enemy pool, one boss HP bar; every player's damage contributes.
- **Multiple paths** — Ironclad tanks, Warden builds, Cleric sustains, Shadowblade pressures, Arcanist controls — all valid, all needed.
- **Zone in and battle** — no lobby meta, no mandatory prep; pick a class, enter the arena, fight.
- **Community crafting** — the hub is the shared social + crafting space; forge upgrades from gathered materials.
- **State layer** — combat rewards tracking DoTs/HoTs/transition states across the party, not just raw DPS.

---

## VFX & Assets

- brbmuffins Technologies particle pack (ElectricalSparks, EnergyExplosion, SmallExplosion, FireFlies, HeatDistortion).
- brbmuffins Dark Arts fantasy pack (Magic circle, Death magic circle, Lightning strike, Mana wall, Ground spikes, Fireball).
- `RodBillboard` — zone/label text always faces the camera; `EnemyDeathVFX` — death particles via `Health.onDeath`; `LoginScreenVFX` — login-screen ambience.
- 3D: enemy, boss, and all five class models are Tripo-generated GLBs (`Docs/models/`); Field Goul + wooden-fence set added in the Gundab update.

---

## GM Console

In-game admin console — gated by the `GM_USERS` allowlist in `GmConsole.cs`; ↑/↓ for command history:

| Command | Effect |
|---|---|
| `speed <n>` | Multiply move + sprint speed by n |
| `fly` | Toggle fly mode (gravity off; Space/Ctrl for vertical) |
| `god` | Toggle `Health.isInvulnerable` |
| `heal` / `kill` | Full-heal self / kill all `"Enemy"`-tagged objects |
| `spawn [n]` / `wave [n]` | Spawn n enemies / start WaveSpawner or jump to wave n |
| `tp <x> <y> <z>` / `goto <name>` | Teleport to coords / to another player |
| `pos` / `players` | Print position / list connected players + class + position |
| `noclip` | Toggle player colliders off |
| `clear` / `help` | Clear log / list commands |

---

## Changelog

### 2026-07-04 — Fields of Gundab + code-review fixes + branch reconciliation

**New content (Fields of Gundab):**
- **Field Goul** enemy — Tripo model + Idle/Run/Punch/Scream/Death animations, `FieldGoulAnimationDriver.cs` (Speed/Attack/Scream/Die/IsDead, integrates with `Health`/`EnemyAI`), and `FieldGoulAnimatorBuilder.cs` (editor-generated animator controller).
- Wooden-fence environment set (`3d Assets/Fences/`) + `Tools/generate_wooden_fence_fbx.py`.
- New gameplay scripts: `PlayerMountBike`, `RevealAura`, `Blinking` (Engineer), `PassiveOverengineered`, `PassiveTriageLoop`, `HealthBarUI`, `EquipmentUI`, `InventoryUI`, `ItemPickup`, `CharacterSelectUI`, `EnemyUi`.

**Code-review fixes (see `Docs/reviews/code-review-2026-07-04.md`):**
- Collapsed the duplicate crafting UI → **`ForgeCraftingPanel`** is canonical (Smelt/Craft tabs, `/api/professions/recipes`); legacy `CraftingUI` removed; `ForgeNPC` repointed.
- **Inventory slot cap unified to 24** across `InventoryManager`, bag UI, and server (was 32 client / 24 server → items could land in invisible slots).
- **`Health` damage reduction is now source-keyed** — `SetDamageReduction(source, f)` combined multiplicatively; Threat Protocol, Siege Mode, resist flasks, and Iron Warden immunity no longer clobber one another.
- **Consumables are usable** — bag-UI click on a flask calls `ConsumableEffect.Apply`, decrements, and persists (added `IsConsumable`). *(Health-mutating effects apply server/host-side; a `[Command]` bridge is the follow-up.)*
- `ProfessionManager` now self-bootstraps + auto-loads; `AfkStationBuilder` no longer creates a redundant instance. Progression-canon note added (character level is a non-power track).

**Branch reconciliation:** merged last night's `origin/main` history-preserving (no force-push) — kept our newer code on conflicts, removed 7 relocated duplicate scripts, restored 443 of our files the merge would have deleted, and kept our redacted `_CONTEXT/CLAUDE.md`.

### 2026-06-28 — Stability & networking bug-fix pass
Localized, behavior-preserving audit across combat, networking, abilities, status:
- `AbilityCaster.cs` — `isLocalPlayer` guard so remote clones don't process local input.
- `RodPositionSaver.cs` — floats serialize with `InvariantCulture` (locale-safe position saves).
- `RodNetworkManager.cs` — `OnCreatePlayer` rejects duplicate `CreatePlayerMessage` (no orphaned player objects).
- `WaveManager.cs` — `JumpToWave` despawns current enemies instead of orphaning them.
- `EnemyAI.cs` — drops dead/downed targets instead of attacking corpses.
- `StatusEffectManager.cs` — re-applying an effect refreshes magnitude/source.
- `Health.cs` — self + re-entrancy guard on Transfer Protocol redirect (no infinite loop).
- `RodChatManager.cs` — server logs `[CHAT] <user>: <msg>`.

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
  Abilities/Scripts/       deployables + ability behaviours (mines, walls, zones)
  Characters/Scripts/      class passives (PassiveThreatProtocol, PassivePhaseCharge,
                           PassiveOverengineered, PassiveTriageLoop), NPC controller, ability pools
  Characters/Engineer/     PlayerMovement + kit scripts (Blinking, PlayerMountBike, RevealAura)
  Characters/Bob/Scripts/  EnemyUi (nameplate + health-bar toggle)
  Combat/Scripts/          EnemyController, EnemyProjectile, WaveSpawner, Health, HealthBarUI,
                           StatusEffectManager, CombatSessionTracker, DropTable, WorldItem,
                           PlayerProjectile, WorldBossController, IronWardenController,
                           SiegeTurretBehaviour, BossArenaTrigger, WraithAbilities
  Enemies/Fields of Gundab/  Field Goul — model + Idle/Run/Punch/Scream/Death anims,
                           Field_Goul.controller, FieldGoulAnimationDriver
  Items/Scripts/           CharacterStats, Equipment, EquipmentUI, InventoryUI
  Objects/Scripts/         ItemPickup
  Networking/              RodNetworkManager, RodNetworkAuthenticator, PlayerIdentity,
                           PortalTransition, RodChatManager, ForgeNPC
  Systems/                 client REST singletons (InventoryManager, ItemCatalog, HeroMastery,
                           ProfessionManager, ConsumableEffect, AfkGatheringStation)
  UI/                      HUDs + panels: GmConsole, LoginManager, PlayerProgressManager,
                           AbilityCaster, AbilityBar, AbilityTooltipUI, InventoryBagUI,
                           ForgeCraftingPanel, GatheringHUD, CharacterSelectUI, WorldBossHealthBar
  Editor/                  BCE menu builders (scenes/prefabs reproducible from these):
                           HubSceneBuilder, EnemyBuilder, AfkStationBuilder, IronWardenBuilder,
                           FieldGoulAnimatorBuilder
  3d Assets/Fences/        wooden-fence set (Gundab dressing; see Tools/generate_wooden_fence_fbx.py)
  Scenes/                  LoginScene(0), CharacterSelect(1), Hub(2)
  Prefabs/                 5 hero prefabs + Enemy_Grunt / Enemy_Ranged / Enemy_Elite
  Heroes/Brandalf/         6th-hero model — DECISION PENDING (skin vs class), don't wire

CrossWorlds/               legacy staging tree + design docs (read-only reference)
_CONTEXT/                  server/API docs (CLAUDE.md, VPS_SERVER.md), VPS patch files
Docs/                      logo.png, screenshots; icons/ (ability + class + combat art),
                           models/ (enemy, boss & class GLB + renders), reviews/ (code reviews)
Tools/                     generate_wooden_fence_fbx.py (asset generator)
tools/                     build-server.ps1, deploy-server.sh, deploy-inventory-patch.sh,
                           deploy-craft-fix.sh
web/                       Three.js browser client submodule (separate project)
```

---

*Unity 6 (6000.4.10f1) · Mirror/KCP · Node.js/Express · MySQL 8 · VPS: playcrossworlds.com*
