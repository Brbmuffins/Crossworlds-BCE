# Crossworlds BCE Combat Strategy Guide

> Full-field player guide for the July 2026 Unity combat build.
> Built from the live combat scripts, the current class pools, and the recent Brbmuffins combat commits around swarm behavior, wisps, zone configs, heavy attacks, cast stability, Cleric VFX, and variant-zone abilities.

## Table of Contents

- [1. The Big Picture](#1-the-big-picture)
- [2. Combat Rules That Matter](#2-combat-rules-that-matter)
- [3. Reading Enemies](#3-reading-enemies)
- [4. What You Get From Combat](#4-what-you-get-from-combat)
- [5. Maxing A Class](#5-maxing-a-class)
- [6. Hero Chapters](#6-hero-chapters)
- [7. Party Comps](#7-party-comps)
- [8. Boss And Late-Wave Survival](#8-boss-and-late-wave-survival)
- [9. Quick Reference Tables](#9-quick-reference-tables)

---

## 1. The Big Picture

Crossworlds combat is not a tab-target MMO system. It is a lane, cone, circle, and timing game. You win by placing effects where enemies are going, not where they were, and by making the enemy pack behave the way your team wants.

The current combat identity is:

- Aim-based abilities with visible ground indicators.
- Server-authoritative health, damage, shields, redirects, and enemy AI.
- Cast commitment: click to lock the target, then the spell fires after cast time unless you move too far.
- Wave combat that escalates by enemy count, enemy stat multipliers, elites, and wisp swarms.
- Gear and mastery drive combat power. Character level exists, but current combat power is gear plus mastery, not level scaling.
- Status effects are the combo language: Slow, Stagger, Silence, Cursed, Weakened, and Bound.

The July 2026 combat direction from recent commits is important: fights are meant to feel more aggressive and readable. Enemies no longer politely line up. They spread around the player, rush faster into attack positions, and later waves add floating wisps that flank and pulse damage. The game is moving toward swarm pressure plus big telegraphed danger.

### Current Class Names

Some scripts still use older internal names. Player-facing names are:

| Class Index | Current Name | Legacy/Internal Flavor |
|---|---|---|
| 0 | Warden | Engineer / construct commander |
| 1 | Ironclad | Guardian / threat anchor |
| 2 | Shadowblade | Wraith / assassin |
| 3 | Cleric | Medic / field healer |
| 4 | Arcanist | Phaser / void mage |

---

## 2. Combat Rules That Matter

### Rule 1: Power Comes From Gear And Mastery

The `CharacterStats` pipeline says this plainly: combat power is gear plus mastery. The account/character level track can award XP and gold UI progress, but it should not be treated as your damage scaler.

Your actual combat channels are:

| Power Channel | What It Does |
|---|---|
| Max Health | More HP before downed state |
| Damage | Multiplies outgoing ability damage |
| Damage Reduction | Reduces incoming damage, capped for gear |
| Move Speed | Helps kiting and telegraph dodging |
| Cooldown Reduction | Lowers ability cooldowns, capped at 60% total |
| Heal Power | Multiplies healing dealt |

### Rule 2: Damage Is Layered, Not Flat

When you take damage, the game checks several layers:

1. Invulnerability, such as dodge or scripted immunity, can ignore the hit.
2. Weakened makes incoming damage 25% higher.
3. Ability damage reduction applies through named sources, such as Threat Protocol.
4. Gear damage reduction applies separately.
5. Redirects can send part of the damage to another character.
6. Absorption can catch damage before HP, such as Ironclad's Counter Blow behavior.
7. Shields are consumed before health.
8. HP drops, events fire, and downed/death handling begins.

That means a good defensive play is not just "have more health." A great defensive play stacks the right layer at the right time: dodge first, shield before impact, redirect only when the receiver is prepared, and use damage reduction during incoming burst.

### Rule 3: Cast Commitment Is A Skill Check

Abilities can have cast time. You aim, click to commit, and then the cast completes if you do not move too far. Recent fixes raised the movement tolerance to handle dedicated-server corrections, so spells should no longer cancel just because Mirror nudged your position.

Player advice:

- Aim, commit, then hold your ground for the cast window.
- If you need to move, cancel before commitment or accept losing the cast.
- Use instant or low-cast-time tools when enemies are already in melee.
- Use longer commitment spells after Ironclad, Warden, or Arcanist has controlled the pack.

### Rule 4: Variant Zones Are The New Advanced Layer

Several newer class defaults use "sweet spot" variant zones. A single cone or beam can have different effects depending on where the target lands inside the indicator.

The current default variant-style abilities are:

| Class | Ability | Near Zone | Mid Zone | Far Zone |
|---|---|---|---|---|
| Cleric | Healing Cone | Burst healing | Healing over time | Shielding |
| Cleric | Mending Beam | Burst healing | Healing over time | Shielding |
| Arcanist | Conflagration Cone | Burst fire damage | Cursed/DoT pressure | Slow/Weakened setup |
| Arcanist | Ember Beam | Burst fire damage | Cursed/DoT pressure | Slow/Weakened setup |

Default selection is cursor distance, but the options menu can switch to scroll-wheel zone selection. Cursor distance is faster. Scroll-wheel mode is more deliberate.

### Rule 5: The Team Is A Combo Machine

The best damage is set up. The strongest teams do this:

- Ironclad pulls or holds enemies.
- Arcanist groups and Weakens them.
- Shadowblade stacks debuffs and detonates them.
- Warden creates the kill zone before enemies arrive.
- Cleric keeps the team alive through the burst window.

If everyone fires their biggest button alone, the party gets five small fireworks. If everyone fires into the same controlled pack, the screen disappears.

---

## 3. Reading Enemies

### Basic Enemy Behavior

Enemies run a server-side loop:

1. Idle until a player enters aggro radius or damages them.
2. Chase the target.
3. Attack when in range.
4. Leash back if dragged too far from spawn.
5. Die, roll loot, report kill progress, and despawn or respawn depending on configuration.

Recent swarm behavior makes them more dangerous:

- Enemies are quicker to begin attacking.
- Body collision is relaxed so packs do not jam themselves into a slow line.
- They spread around a player using stable chase slots instead of stacking in one spot.
- Their animator timing is desynced so mobs feel less robotic.

Player meaning: do not expect a neat front line. You will get surrounded if you stand still.

### Melee Grunts

Grunts want to close and swing. Their role is to occupy space and punish greedy casts.

Counterplay:

- Kite in arcs, not straight backward.
- Use circles and cones where the pack is pathing.
- Drop snares or pulls at the spot they must cross.
- Let Ironclad take the first wave of contact.

### Ranged Enemies

Ranged enemies try to hold preferred distance and fire projectiles when possible.

Counterplay:

- Break line and force them to reposition.
- Use Iron Rampart to block projectile lanes.
- Use Arcane Step or Shadow Veil to reach them.
- Pull them into the melee pack with Gravity Slam or Void Maw.

### Elites

Elites are the priority target. They hit harder, appear on configured wave intervals, and should be treated as the fight's mini-boss.

Counterplay:

- Call focus target.
- Stack Weakened, Cursed, and damage cooldowns on the elite.
- Do not waste Dark Harvest before stacks exist.
- Save shields and revives for elite pressure.

### Wisps

Wisps are the floating swarm tier added in recent commits. They do not need NavMesh. They scan for players, float toward personal chase slots, deal contact damage, and pulse AoE damage on a cooldown.

Default behavior:

- Wisp swarm can appear every even wave.
- Default swarm count is 3.
- Contact damage is light but annoying.
- Pulse damage punishes clustered players.

Counterplay:

- Spread just enough that one pulse does not hit everyone.
- Kill wisps before they sit on the healer.
- Use cones and beams because they float into predictable lanes around the target.
- Cleric should pre-shield the chased player, not chase the wisp.

### Enemy Heavy Attacks

EnemyHeavyAttack is the big late-wave pressure system. It adds random special abilities on cooldown while an enemy is in chase or attack state. Each has a telegraph wind-up before damage lands.

| Heavy Attack | What It Does | How To Beat It |
|---|---|---|
| Ground Slam | Self-centered AoE, high damage, Stagger | Roll out or through; do not stack on tank |
| Void Burst | AoE at target, damage, Slow, Cursed DoT | Move immediately after telegraph |
| Chain Lightning | Jumps between players, applies Weakened | Spread before it fires |
| Ground Spikes | Cone toward target, damage and Slow | Sidestep, do not backpedal |
| Hex Blast | Heavy single-target hit, Weakened and Cursed | Shield, cleanse, or line defensive cooldown |

If a pack has both wisps and heavy attackers, kill priority is usually: elite heavy caster, wisp on healer, ranged enemy, melee trash.

---

## 4. What You Get From Combat

Combat rewards have several layers.

### Gold And Item Drops

Enemies can use a `DropTable`. The table always rolls gold, then may roll items based on weighted entries. World drops appear as floating pickups. Gold pickups go straight to progress. Item pickups pass to inventory saving.

Baseline design rates:

| Enemy | Gold | Item Pattern |
|---|---|---|
| Grunt | 1-5 | Mostly nothing, common copper shards, rare copper bar |
| Ranged | 1-3 | Mostly nothing, copper shards |
| Elite | 10-25 | Better material odds, chance for gear |

Exact tables depend on prefab and zone setup.

### Character XP And Gold

The client posts kill progress through `/api/combat/kill` using the player's JWT and enemy template ID. `PlayerProgressManager` also tracks level, XP, gold, and saves to the auth API.

Important design note: this is a progress track, not the current source of combat power. Treat character level as account progression until the design intentionally changes it.

### Mastery XP

WaveSpawner awards mastery XP to the hero you are currently playing:

| Event | Mastery XP |
|---|---|
| Wave clear | `40 + wave * 15` |
| Arena clear | 200 bonus |

Examples:

| Wave | XP |
|---|---|
| 1 | 55 |
| 5 | 115 |
| 10 | 190 |

In a 10-wave clear, wave-clear mastery alone totals 1,225 XP, plus 200 completion XP, for 1,425 mastery XP before any future kill, dungeon, quest, or boss sources are counted.

### Mastery Bonuses

Current code applies mastery bonuses through `HeroMasteryManager` into `CharacterStats`. The bonuses are aggregated from all five heroes, so maxing multiple heroes can help your whole account's combat profile.

Current code bonuses:

| Hero | Level 6 Bonus | Level 10 Bonus |
|---|---|---|
| Warden | +8% damage | +5% cooldown reduction |
| Ironclad | +10% max HP | +8% damage |
| Shadowblade | +8% damage | +8% cooldown reduction |
| Cleric | +15% healing | +10% max HP |
| Arcanist | +8% cooldown reduction | +10% damage |

This is different from some older design notes that described class-specific passive upgrades. Those are good future flavor, but the current manager applies broad stat channels.

### Gear And Attunements

Gear contributes through stat modifiers:

- Damage
- Max Health
- Damage Reduction
- Move Speed
- Cooldown Reduction
- Heal Power

For maxing a combat role, gear should support the job you actually perform:

| Role | Best Stats |
|---|---|
| Warden | Damage, cooldown reduction, some max HP |
| Ironclad | Max HP, damage reduction, cooldown reduction |
| Shadowblade | Damage, cooldown reduction, move speed |
| Cleric | Heal power, cooldown reduction, max HP |
| Arcanist | Damage, cooldown reduction, move speed |

---

## 5. Maxing A Class

### The Mastery Route

If your only goal is to max one hero, play that hero in repeatable wave content and clear full arenas. Mastery XP is awarded on wave clear and completion, so surviving to the end matters more than farming early trash forever.

Basic route:

1. Pick your main.
2. Build a survivable four-slot loadout.
3. Run arenas to full clear, not just early waves.
4. Upgrade gear around your class role.
5. Learn enemy heavy telegraphs.
6. Push higher zones only when your clear speed stays stable.
7. Once your main reaches level 6, consider cross-mastery bonuses.

### The Cross-Mastery Route

Because mastery bonuses aggregate, the strongest account path is not necessarily "one class to 10 first." The power-gamer route is:

1. Get your main to level 6 for its first meaningful bonus.
2. Get Warden to level 6 for +8% damage.
3. Get Shadowblade to level 6 for another +8% damage if you play damage classes.
4. Get Arcanist to level 6 for +8% cooldown reduction.
5. Get Cleric to level 6 if you heal or use self-sustain.
6. Get Ironclad to level 6 if survival is blocking clears.
7. Push your main to 10.

Recommended priority by main:

| Main | First Off-Class Masteries |
|---|---|
| Warden | Arcanist 6, Shadowblade 6, Ironclad 6 |
| Ironclad | Cleric 6, Arcanist 6, Warden 6 |
| Shadowblade | Warden 6, Arcanist 6, Ironclad 6 |
| Cleric | Arcanist 6, Ironclad 6, Warden 6 |
| Arcanist | Warden 6, Shadowblade 6, Ironclad 6 |

### XP Reality Check

The current code uses `200 * level^1.4` as the XP-to-next calculation, while older design docs list larger cumulative targets up to 25,000 XP. Treat the code as the current build behavior and the older table as a long-term design target.

### How To Farm Efficiently

Best habits:

- Full clear arenas for the 200 completion bonus.
- Do not over-push a zone where wipes waste time.
- Use team compositions that clear waves quickly.
- Build cooldown reduction early; more casts means faster wave clears and better survival.
- Kill wisps fast because they waste healer attention.
- Save ultimates for elite waves, wisp waves, or boss-style moments.

Bad habits:

- Resetting after easy early waves.
- Chasing scattered enemies instead of grouping them.
- Blowing long cooldowns into one low-health enemy.
- Playing pure damage with no shield, heal, or mobility.
- Ignoring heavy telegraphs because "the healer has it."

---

## 6. Hero Chapters

## Warden - Construct Tactician

Warden is a setup class. You are strongest before the fight starts. If you are reacting after enemies arrive, you are already late.

### Default Loadout

| Slot | Ability | Purpose |
|---|---|---|
| 1 | Runic Sentinel | Sustained deployable pressure |
| 2 | Runic Snare | Trap burst and kill-zone setup |
| 3 | Battle Hymn | Cooldown acceleration |
| 4 | Mend | Emergency heal and cleanse |

### Passive: Overengineered

Every 4 seconds, each active deployable within 12 units gains an output stack. Each stack improves deployable damage/healing by 8%, up to 5 stacks. Stacks stay on the deployable until it is destroyed.

What that means:

- Your deployables are investments.
- Place them where they survive.
- Fight near them.
- Do not abandon your kill zone unless mechanics force it.

### How To Play

Open with Sentinel before combat. Place Snare where enemies must walk. Use Battle Hymn when the team is actively spending cooldowns, not while everyone is repositioning. Use Mend to save a key player or clear a dangerous debuff.

### Warden Rotation

1. Plant Runic Sentinel.
2. Place Runic Snare at the choke or expected enemy path.
3. Let enemies enter the zone.
4. Use Battle Hymn as your team starts burning.
5. Refresh Sentinel/Snare positioning as the fight moves.
6. Use Mend reactively.

### Best Stats

Damage and cooldown reduction first. Max HP second. Warden wants to cast often and keep deployables active.

### Advanced Tips

- Do not place Sentinel in front of the tank. Put it behind or beside the line.
- Snare is strongest at spawn lanes, portals, and boss feet.
- Battle Hymn gets better with coordinated teams.
- Warden plus Ironclad is a classic trap-and-hold pair.

### Common Mistakes

- Fighting away from your constructs.
- Using Battle Hymn before anyone has committed cooldowns.
- Replacing useful deployables too quickly.
- Building pure damage and dying before stacks matter.

---

## Ironclad - Iron Vanguard

Ironclad is the anchor. You do not just survive damage; you convert enemy attention into team safety and counter-pressure.

### Default Loadout

| Slot | Ability | Purpose |
|---|---|---|
| 1 | Arcane Ward | Personal shield |
| 2 | Shieldwall Charge | Engage and threat stacks |
| 3 | Stalwart Stance | Damage reduction and threat generation |
| 4 | Iron Rampart | Projectile wall and lane control |

### Passive: Threat Protocol

Taking damage builds stacks. At 5 stacks, nearby enemies redirect aggro to you and you gain 20% damage reduction for 6 seconds. Death clears stacks.

What that means:

- Taking controlled hits is good.
- Taking uncontrolled hits without healer support is not.
- Your job is to make enemies hit the one target built to survive it.

### How To Play

Start fights by entering first, but not blindly. Charge through packs, then plant Stalwart Stance when you know enemies will commit. Use Arcane Ward before large telegraphed damage. Save Iron Rampart for ranged waves, boss projectiles, or saving the backline.

### Ironclad Rotation

1. Arcane Ward before first impact if needed.
2. Shieldwall Charge through the pack.
3. Hold position with Stalwart Stance.
4. Let Threat Protocol trigger.
5. Use Iron Rampart when projectiles or ranged pressure appear.
6. Rotate back to Charge as cooldowns return.

### Best Stats

Max HP, damage reduction, cooldown reduction. Damage is useful, but only after you can survive the job.

### Advanced Tips

- Do not kite enemies out of Warden traps or Arcanist zones.
- Stand slightly ahead of allies, not on top of them.
- Rampart is not just defense. It can split the map and force ranged enemies to move.
- If the enemy has Chain Lightning, spreading is still required. A tank cannot absorb bad formation.

### Common Mistakes

- Charging out of healer range.
- Using Stalwart Stance while a telegraph requires movement.
- Saving Rampart until after projectiles have already landed.
- Building only HP and having no cooldown uptime.

---

## Shadowblade - Void Infiltrator

Shadowblade is a burst-and-reset class. The whole kit is about creating a debuff bank and cashing it out with Dark Harvest.

### Default Loadout

| Slot | Ability | Purpose |
|---|---|---|
| 1 | Fan of Blades | Close cone damage |
| 2 | Dark Mark | Debuff setup |
| 3 | Dark Harvest | Debuff detonation |
| 4 | Shadow Veil | Stealth and safety |

### Passive: Bounty System

On kill, Shadowblade reduces all ability cooldowns. Normal kills reduce by 2 seconds. Elite kills reduce by 5 seconds.

What that means:

- You snowball through waves.
- Last-hitting matters when the hook sees you as the killing source.
- Elite kills are enormous for tempo.

### How To Play

Open from safety, mark targets, stack debuffs, then detonate. Dark Harvest is your signature button, but it is worthless if the enemy has no debuffs. Shadow Veil is both engage and escape.

### Shadowblade Rotation

1. Shadow Veil to position or avoid opening pressure.
2. Dark Mark the priority target or packed group.
3. Fan of Blades through multiple enemies.
4. Wait for team debuffs: Weakened, Cursed, Slow, Silence, Stagger.
5. Dark Harvest when stacks are high.
6. Use kill CDR to keep the chain going.

### Best Stats

Damage, cooldown reduction, move speed. Shadowblade wants short windows and fast resets.

### Advanced Tips

- Dark Harvest consumes debuffs. Coordinate with Arcanist and Cleric before detonating.
- Weakened is both a damage amplifier and a Harvest stack.
- You are best into controlled packs, not scattered enemies.
- Veil can save you from wisp pressure if you are trapped.

### Common Mistakes

- Harvesting one stack.
- Fighting face-to-face like Ironclad.
- Chasing kills out of party range.
- Forgetting that Bounty needs kills to snowball.

---

## Cleric - Soul Warden

Cleric is not a passive health bar babysitter. Cleric decides whether the team gets to make mistakes and keep playing.

### Default Loadout

| Slot | Ability | Purpose |
|---|---|---|
| 1 | Healing Cone | Variant burst/HoT/shield support |
| 2 | Mending Beam | Variant line healing |
| 3 | Sacred Aegis | Shield target before damage |
| 4 | Temporal Grace | Team rewind ultimate |

### Passive: Triage Loop

Healing allies returns 8% of the healed amount to the Cleric. It rewards constant ally healing and helps Cleric survive splash and redirected damage.

### How To Play

Cleric plays ahead of damage. Use Sacred Aegis before incoming hits. Aim Healing Cone or Mending Beam so the correct ally lands in the correct sweet spot. Save Temporal Grace for the start of a wipe, not after everyone is already safe or dead.

### Cleric Rotation

1. Track the tank and the most fragile damage dealer.
2. Pre-shield the player about to take damage.
3. Use Healing Cone for close burst or far shield based on positioning.
4. Use Mending Beam through multiple allies when they line up.
5. Save Temporal Grace for disaster.

### Best Stats

Heal power, cooldown reduction, max HP. Damage is optional. Your output is measured in prevented wipes.

### Advanced Tips

- If using cursor-distance variant mode, learn the exact ranges for burst, HoT, and shield.
- A far-zone shield can be better than a near-zone heal before impact.
- Temporal Grace rewinds position and debuffs too. It is a mechanic reset, not just a heal.
- Cleric and Ironclad together let the team survive heavy wave spikes.

### Common Mistakes

- Healing after damage instead of shielding before it.
- Holding Temporal Grace until the recovery window is gone.
- Standing in the same place as the whole team during wisp pulses.
- Ignoring your own position because Triage is healing you.

---

## Arcanist - Void Mage

Arcanist is the zone controller and pack destroyer. The class gets better when enemies are grouped and worse when the fight becomes scattered.

### Default Loadout

| Slot | Ability | Purpose |
|---|---|---|
| 1 | Conflagration Cone | Variant burst/DoT/control |
| 2 | Ember Beam | Variant line burst/DoT/control |
| 3 | Void Maw | Pull and pulse damage |
| 4 | Collapsing Void | Ultimate pull, burst, Weakened |

### Passive: Phase Charge

Every ability cast builds toward a charge. At 6 charges, the next offensive ability deals +40% damage and resets the meter.

What that means:

- Count your casts.
- Do not spend the charged hit on a low-value spell.
- Try to make Collapsing Void or a large packed hit consume the bonus.

### How To Play

Use Void Maw to gather enemies, then hit the clump with beams, cones, and Collapsing Void. Use variant zones deliberately: close for burst, mid for DoT, far for Slow/Weakened setup.

### Arcanist Rotation

1. Build Phase Charge with normal casts.
2. Void Maw the pack.
3. Use Conflagration Cone or Ember Beam through the clump.
4. Watch for charge readiness.
5. Spend charged damage on Collapsing Void or a high-value packed hit.
6. Reposition before enemies collapse onto you.

### Best Stats

Damage, cooldown reduction, move speed. Arcanist wants frequent casts and clean spacing.

### Advanced Tips

- Far-zone control can be better than near-zone burst if Shadowblade is preparing Harvest.
- Void Maw plus Collapsing Void is the core pack deletion combo.
- Weakened raises everyone else's damage, not just yours.
- Do not break formation just to chase a low-health enemy.

### Common Mistakes

- Spending Phase Charge on a small hit.
- Standing still after a pull.
- Using Collapsing Void before enemies are grouped.
- Ignoring wisps because you are tunnel-visioning the big pack.

---

## 7. Party Comps

### The Classic Clear Team

| Slot | Class | Job |
|---|---|---|
| 1 | Ironclad | Holds aggro and controls projectile lanes |
| 2 | Cleric | Keeps tank/carry alive and rewinds disasters |
| 3 | Arcanist | Groups and Weakens packs |
| 4 | Shadowblade | Detonates debuffs |
| 5 | Warden | Builds kill zones and cooldown windows |

Plan:

1. Warden sets Sentinel and Snare.
2. Ironclad starts the pull.
3. Arcanist gathers with Void Maw.
4. Cleric shields the player taking focus.
5. Shadowblade waits for stacks.
6. Arcanist/Shadowblade spend ultimates together.

### Duo: Ironclad + Cleric

This is the safest progression duo. Ironclad creates predictable damage intake, Cleric answers it.

Weakness: clear speed.

Fix: Ironclad slots more damage or Cleric adds Smite/Holy Bolt if available.

### Duo: Arcanist + Shadowblade

This is the kill-speed duo. Arcanist groups and Weakens, Shadowblade detonates.

Weakness: fragile if enemies survive the opener.

Fix: one player must bring a shield or control tool.

### Duo: Warden + Ironclad

This is the lane-control duo. Warden builds the zone, Ironclad keeps enemies inside it.

Weakness: mobile ranged pressure.

Fix: Rampart and ranged focus calls.

### Trio: Warden + Arcanist + Shadowblade

This is the speed-farm trio.

Plan:

1. Warden places traps.
2. Arcanist pulls into traps.
3. Shadowblade detonates debuff stacks.

Weakness: no dedicated healer. Bring Arcane Ward and play cleaner.

---

## 8. Boss And Late-Wave Survival

### Late-Wave Rules

Late waves are not just "more enemies." Dynamic difficulty can increase enemy health and damage per wave and per additional player. Concurrency caps prevent infinite flood, but spawns are paced until the full wave has entered.

That means:

- A wave can feel long even when the screen is not full.
- Cooldown management matters more later.
- Wasted ultimates are punished harder.
- Sustain and debuff cleanse gain value.

### Boss Rules

World boss systems exist and use health phase events, status effects, reflect, tethers, shards, and drops. The design direction is group boss events with participation tracking and weekly-style drops, though full server reward integration may vary by current deployment.

Player rules:

- Never stack unless the mechanic rewards stacking.
- Save big cooldowns for phase transitions.
- Cleanse Weakened and Cursed before a known burst.
- If a boss summons objects or shards, kill the mechanic unless the raid lead calls burn.
- Cleric's Temporal Grace is a phase-recovery button.

### Survival Checklist

Before starting serious combat, ask:

- Does someone have a shield?
- Does someone have a cleanse or heal?
- Can we group enemies?
- Can we stop ranged/projectile pressure?
- Who calls Dark Harvest?
- Who saves ultimate for elite/wisp waves?

---

## 9. Quick Reference Tables

### Default Class Loadouts

| Class | Slot 1 | Slot 2 | Slot 3 | Slot 4 |
|---|---|---|---|---|
| Warden | Runic Sentinel | Runic Snare | Battle Hymn | Mend |
| Ironclad | Arcane Ward | Shieldwall Charge | Stalwart Stance | Iron Rampart |
| Shadowblade | Fan of Blades | Dark Mark | Dark Harvest | Shadow Veil |
| Cleric | Healing Cone | Mending Beam | Sacred Aegis | Temporal Grace |
| Arcanist | Conflagration Cone | Ember Beam | Void Maw | Collapsing Void |

### Status Effects

| Effect | Meaning | Best Use |
|---|---|---|
| Slow | Movement reduction | Keep packs in zones |
| Stagger | Brief interrupt | Stop attacks and buy cast time |
| Silenced | Prevents abilities | Shut down heavy/caster windows |
| Cursed | Damage over time | Shadowblade Harvest fuel |
| Weakened | +25% incoming damage | Team burst window |
| Bound | Movement leash/root | Hold priority targets |

### Mastery Power Targets

| Goal | Why |
|---|---|
| Main to 6 | First meaningful mastery breakpoint |
| Warden 6 | +8% damage |
| Shadowblade 6 | +8% damage |
| Arcanist 6 | +8% cooldown reduction |
| Cleric 6 | +15% healing |
| Ironclad 6 | +10% max HP |
| Main to 10 | Identity capstone and second bonus |

### The Big Combos

| Combo | Result |
|---|---|
| Void Maw + Collapsing Void | Pull, pulse, burst, Weakened |
| Silence Ward + Dark Mark + Dark Harvest | Debuff stack detonation |
| Shieldwall Charge + Warden Snare | Pack forced through traps |
| Stalwart Stance + Cleric Shield | Controlled tank spike |
| Battle Hymn + any burn phase | Faster cooldown cycle |
| Temporal Grace after failed mechanic | Team reset |

---

## Final Advice

Play the fight like a map, not a damage meter. Put enemies where your team wants them. Put allies where your heals and shields want them. Learn the heavy telegraphs. Full clear arenas. Max mastery intelligently. And when the screen gets ugly, do not press every button at once: call the pack, stack the debuffs, then cash out.

