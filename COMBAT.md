# Rate of Decay ONLINE — Combat Bible
*Last updated: June 2026*

---

## Combat Philosophy

The feel we're going for: **active, readable, positional**. No tab targeting. No standing still casting. Every button press should feel like a decision that matters on the battlefield. GW2 proved that a small ability bar forces better game design — each slot earns its place.

**Core pillars:**
- **Dodge is a resource**, not a reaction button. Two charges means you plan, not panic.
- **Combos reward knowledge**, not reflexes. A Wraith who knows when to detonate beats button-mashers.
- **Positioning matters every second**. Singularity pulls, Null Field zones, and Iron Tether leashes only work if someone built for them.
- **Every class can survive**. No hard trinity — roles exist but no one is helpless without a healer.

---

## Dodge Roll — The Foundation of Combat Feel

The dodge roll is **the most important single feature** for making combat feel GW2-like. Everything else is secondary.

### Spec
- **Input**: Space (default jump), or dedicated Left Alt bind
- **Behavior**: Burst of speed in your current movement direction (or backward if no input)
- **I-frames**: 0.35s of `isInvulnerable = true` on Health — no damage taken
- **Charges**: 2 maximum
- **Recharge**: One charge every 5s (both charges recharge independently)
- **Stamina bar**: Show as two small pips below the health bar — one pip = one charge
- **Animation**: Hook into `Animator.SetTrigger("dodge")` — use the roll animation from Kevin Iglesias/Human Animations pack

### How it's implemented
See `PlayerMovement.cs` — the dodge system has been added directly. The `Health.cs` script's `isInvulnerable` field is checked at the top of `TakeDamage()`.

### Why it feels good
- Dodge is never "free" — spending both charges to survive one combo means you're vulnerable for 10s
- It rewards movement — a stationary player runs out of dodges fast
- I-frames make every successful dodge feel impactful, not just cosmetic

---

## The Four Classes

### ENGINEER — The Architect
*Builds the battlefield before the fight starts. Strongest when the team positions around their structures.*

**Identity**: Deployables, area denial, turret warfare
**Passive**: Overengineered — deployables gain bonus duration/effect when planted in overlapping zones

**4-Slot Standard Loadout:**
| Slot | Ability | Cooldown | What it does |
|------|---------|----------|--------------|
| 1 | Sentinel Drop | 6s | Deploy auto-turret — sustained DPS anchor |
| 2 | Shock Mine | 5s | Proximity explosive — 40 burst, place at chokepoints |
| 3 | Arc Cannon | 4s | Chargeable rectangle — 15 to 50 damage, good for lanes |
| 4 | Dark Blast | 3s | Fast cone filler — 10 to 30 damage, lowest cooldown |
| U | System Overload / Overdrive | 12-45s | — see ultimate section |

**Combo: The Killzone**
1. Drop Sentinel at a corner facing into a room
2. Plant 2 Shock Mines at approach paths
3. Use Arc Cannon (hold for full charge) on anything that makes it past the mines
4. Use Dark Blast for mobile cleanup

**How it flows**: The Engineer is proactive. If you're casting your abilities reactively, you're playing them wrong. Set up before enemies arrive. The turret + mines do most of the work; your direct casts finish stragglers.

**What makes it satisfying**: Watching 3 enemies walk into your kill zone simultaneously. The turret starts firing, a mine detonates, you drop an Arc Cannon on the survivor. Zero chaos, pure execution.

---

### GUARDIAN — The Anvil
*Takes hits for the team, punishes enemies who focus them, controls single targets.*

**Identity**: Crowd control, damage absorption, threat management
**Passives**: Threat Protocol (stacks on hit → DR bonus), Triage Loop (low HP triggers emergency heal)

**4-Slot Standard Loadout:**
| Slot | Ability | Cooldown | What it does |
|------|---------|----------|--------------|
| 1 | Breach Slam | 6s | Dash forward, stagger all enemies hit (0.8s) |
| 2 | Iron Tether | 9s | Lock one enemy in place for 5s, leash them to you |
| 3 | Kinetic Reversal | 10s | Absorb incoming damage for 3s, release as a 70° cone |
| 4 | Siege Mode | 14s | Root yourself, 40% DR, triple Threat stacks for 6s |
| U | Last Bastion | 50s | Deploy a hardlight wall blocking all projectiles for 10s |

**Combo: The Iron Vice**
1. **Breach Slam** into a group → enemies are staggered, you're in melee
2. **Kinetic Reversal** immediately → you absorb the counter-attack while they can't act
3. **Iron Tether** the highest-priority target → leashed, can't retreat
4. Use the Kinetic Reversal burst (auto-releases after 3s) → cone damage hits all nearby enemies
5. **Siege Mode** if overwhelmed → you become a wall, generating Threat stacks to pull aggro

**Why the combo works**: Breach Slam staggers for 0.8s — that's exactly enough time to activate Kinetic Reversal before they can hit back. You absorb their entire counterattack burst, then detonate it back in their faces. Iron Tether ensures your primary target can't escape while this plays out.

**Design note**: Iron Tether's documented 15% damage redirect to the leashed enemy's attacks on allies is not yet implemented — when it is, the combo becomes even more powerful since all damage the tethered enemy deals flows into your Kinetic Reversal pool.

---

### WRAITH — The Knife
*Surgical. Goes in, disrupts, gets out. Wins by knowing exactly when to detonate.*

**Identity**: Stealth, debuff loading, single-target assassination, zone suppression
**Passive**: Bounty System — bonus credit on kills from stealth

**4-Slot Standard Loadout:**
| Slot | Ability | Cooldown | What it does |
|------|---------|----------|--------------|
| 1 | Phase Cloak | 10s | Enter stealth. Next ability from stealth: +50% damage |
| 2 | Neural Spike | 5s | AoE interrupt + 35 damage (triggers backstab bonus from cloak) |
| 3 | Null Field | 12s | Zone: suppresses enemy attacks + 4 DPS decay for 5s |
| 4 | Shadow Relay | deployable | Extends next cloak by 3s, reduces cloak CD on kill |
| U | Collapse | 40s | Detonate all debuffs on all enemies in range — 20 damage per stack |

**Combo: The Setup**
1. **Phase Cloak** → approach safely
2. **Neural Spike** from stealth → 52.5 damage (backstab ×1.5) + interrupt
3. **Null Field** → suppresses target, applies DOT and Suppress debuffs (2 stacks loaded)
4. Wait 2s for DOT to tick, then **Collapse** → 40 burst damage (2 stacks × 20) from the debuffs Null Field applied
5. Total sustained burst: ~130 damage without gear, clean sequence

**Advanced: Shadow Relay positioning**
Drop Shadow Relay *before* you cloak, in the position you plan to fight from. When you cloak and walk within 6 units of it, your cloak lasts 7 seconds instead of 4 — enough to set up Null Field before Neural Spike. The Shadow Relay also gives -4s cloak CD per kill, meaning active Wraiths can cycle back into stealth faster than cooldowns suggest.

**What makes it satisfying**: The Collapse detonation. Watching a single button press delete a debuffed enemy is viscerally gratifying — if you loaded 4 stacks, that's 80+ damage from one cast.

---

### MEDIC — The Lifeline
*Reactive, high-stakes. Not a passive heal-bot. At their best in dire situations.*

**Identity**: Heal deployment, damage redirects, cleansing, emergency revival
**Passive**: Triage Loop (also applies to team — low-HP allies get heal ticks)

**4-Slot Standard Loadout:**
| Slot | Ability | Cooldown | What it does |
|------|---------|----------|--------------|
| 1 | Restoration Beacon | 6s | Deploy: heals 12 HP every 3s to allies in 8u for 30s |
| 2 | Nanite Swarm | 7s | Mobile heal orb flies to target ally, heals 30 on arrival |
| 3 | Transfer Protocol | 9s | Redirect 100% of ally's incoming damage to yourself for 5s |
| 4 | Adaptive Shield | 10s | Apply 20 absorb shield; grows if ally takes damage |
| U | System Rollback | 60s | Rollback entire team 5 seconds in time (position + HP) |

**Combo: The Tank-Medic Loop**
1. Place **Restoration Beacon** at the Guardian's position
2. Cast **Adaptive Shield** on the Guardian — they're absorbing hits, shield grows with damage
3. When Guardian dips below 30%, hit **Nanite Swarm** → quick burst heal while you stay at range
4. If Guardian is about to be bursted: **Transfer Protocol** → eat their damage yourself for 5s while the Beacon heals you back
5. If everything goes wrong: **System Rollback** → everyone teleports back 5s. Guardian is at full health. Enemies are back in their starting positions.

**Advanced with Guardian**:
Transfer Protocol + Guardian's Kinetic Reversal is a cross-class combo. Guardian activates Kinetic Reversal first. Medic then activates Transfer Protocol targeting the Guardian. For the next 3 seconds, the Guardian absorbs all incoming damage AND the Medic is absorbing the Guardian's damage — meaning the Kinetic Reversal pool fills from two sources. Release hits harder.

**What makes it satisfying**: System Rollback is a "we don't lose" button. It doesn't feel overpowered because it has a 60s cooldown, but when it lands it creates an extraordinary moment. The entire team snaps backward in time, enemies are confused, and the fight resets. Nothing in WoW or GW2 does exactly this.

---

## Cross-Class Combos

These work best in a coordinated 4-player group:

### "The Anvil + The Knife" (Guardian + Wraith)
- Wraith drops **Null Field** (suppress + DOT) on an enemy pack
- Guardian **Breach Slams** in, **Iron Tethers** the priority target
- Tethered target can't escape. They're suppressed. DOT is ticking.
- Wraith **Collapses** — every enemy in the null field detonates simultaneously
- Guardian **Kinetic Reversal** eats the remaining counterattack

### "The Architect's Trap" (Engineer + Wraith)
- Engineer places **Sentinel Drop** and **Shock Mines** at a funnel
- Wraith drives enemies into the funnel using **Null Field** as a herding tool (enemies retreat from it)
- Enemies hit the mines, turret fires, Engineer finishes with **Arc Cannon**
- Wraith picks off survivors from **Phase Cloak** for Bounty System stacks

### "The Eternal Wall" (Guardian + Medic)
- Guardian uses **Last Bastion** wall — blocks all projectiles for 10s
- Medic stands behind the wall using **Restoration Beacon** and **Adaptive Shield**
- Guardian steps out, takes a hit to build Kinetic Reversal, steps back behind wall
- This cycle makes a small-group duo nearly unkillable in ranged encounters

### "The Clock" (Medic + Anyone)
- Any teammate is about to die
- Medic **System Rollback** → everyone goes back 5 seconds
- The "about to die" scenario is undone — enemies are further away, HP is restored
- The combo requires the Medic to save the rollback — it's the highest-stakes resource management in the game

---

## Ability Combos Quick Reference

| Trigger Ability | Combo Ability | Effect |
|----------------|---------------|--------|
| Breach Slam (stagger) | → Kinetic Reversal | Absorb counterattack during 0.8s stagger window |
| Iron Tether (leash) | → Kinetic Reversal | Absorb tethered enemy's attacks into the pool |
| Null Field (suppress + DOT) | → Collapse | Detonate for 40+ burst (2 stacks minimum) |
| Phase Cloak (stealth) | → Neural Spike | +50% backstab damage (52.5 vs 35) |
| Shadow Relay (nearby) | → Phase Cloak | +3s cloak duration (7s total) |
| Singularity (pull) | → Arc Lance | Chain hits all bunched enemies (max jumps) |
| Event Horizon (Exposed) | → Arc Lance | All jumps deal +25% on Exposed targets |
| Phase Relay (nearby) | → Singularity | +2s pull phase (5s total pull) |
| Adaptive Shield (on ally) | → Transfer Protocol | Absorb ally's damage; shield grows from redirected hits |
| Restoration Beacon (placed) | → Transfer Protocol | Beacon heals Medic while eating ally's damage |
| Kinetic Reversal (Guardian) | → Transfer Protocol (Medic) | Both players absorb simultaneously, KR pool fills faster |

---

## VFX Assignments — Existing Assets

All VFX uses assets you already have. No new purchases needed.

### brbmuffins Dark Arts / Fantasy Pack

| Effect | Prefab | Used By |
|--------|--------|---------|
| Shield absorb (blue tint) | `Prefabs/Shield buff.prefab` | Kinetic Reversal activation, Adaptive Shield, Bastion Node |
| Healing pulse | `Prefabs/Healing buff.prefab` | Restoration Beacon pulse, Nanite Swarm arrival |
| Support buff (purple tint) | `Prefabs/Magic buff.prefab` | Phase Relay idle, Overdrive aura |
| Ground ring | `Prefabs/Effects normal/Magic circle.prefab` | Restoration Beacon base, Singularity cast |
| Danger zone (red tint) | `Prefabs/Effects normal/Death magic circle.prefab` | Singularity ambient, Event Horizon |
| Projectile wall | `Prefabs/Effects normal/Mana wall.prefab` | Last Bastion wall (tint blue for hardlight) |
| Orbs orbit | `Prefabs/Glowing orbs.prefab` | Nanite Swarm in flight, Transfer Protocol tether orb |
| Dome | `Prefabs/Leaves shield.prefab` | Iron Tether anchor (tint blue/grey) |
| Ball impact | `Prefabs/Plazma sphere.prefab` | Phase Shift arrival |

### brbmuffins Technologies / Particle Pack

| Effect | Prefab | Used By |
|--------|--------|---------|
| Ground crack | `Fire & Explosion/Prefabs/EarthShatter.prefab` | Breach Slam impact |
| Electric sparks | `Misc Effects/Prefabs/ElectricalSparks.prefab` | Shock Mine idle |
| Small explosion | `Fire & Explosion/Prefabs/SmallExplosion.prefab` | Shock Mine detonation |
| Dissolve out | `Misc Effects/Prefabs/DissolveSolidHorizontal.prefab` | Phase Shift depart, Phase Cloak enter |
| Ground fog (purple tint) | `Smoke & Steam/Prefabs/GroundFog.prefab` | Null Field zone |
| Dust motes (dark) | `Misc Effects/Prefabs/DustMotesEffect.prefab` | Shadow Relay idle |
| Steam vents | `Smoke & Steam/Prefabs/Steam.prefab` | Siege Mode anchor VFX |
| Energy burst | `Fire & Explosion/Prefabs/EnergyExplosion.prefab` | Kinetic Reversal release, Overdrive |
| Plasma explosion | `Legacy Particles/PlasmaExplosionEffect.prefab` | Singularity/Event Horizon burst |
| Heat shimmer | `Misc Effects/HeatDistortion.prefab` | Singularity ambient (layered with magic circle) |

### Dodge Roll VFX (from existing packs)
- **Enter dodge**: `DissolveSolidHorizontal.prefab` scaled down to player feet
- **Exit dodge**: `SparksEffect.prefab` brief spark at landing point
- Both are instantiated and immediately auto-destroyed (2s lifetime)

---

## Bugs Fixed in This Update

The following issues were found during code review and have been corrected:

### 1. LineRenderer Invisible in URP
**Files**: `IronTetherHandler.cs`, `TransferProtocolHandler.cs`
**Problem**: Both used `Shader.Find("Sprites/Default")` for the chain/tether line material. This shader is not supported in URP and renders as invisible (or a pink error material).
**Fix**: Changed to `Shader.Find("Universal Render Pipeline/Particles/Unlit")` with `_BaseColor` property set instead of `_Color`.

### 2. Overdrive Was a Complete Stub
**File**: `AbilityCaster.cs`
**Problem**: `CastOverdrive()` only played particle effects. The actual buff (team CDR) had a TODO comment.
**Fix**: Overdrive now applies a 10s cooldown reduction aura to all teammates in 12u range using the existing `CharacterStats` system. 

### 3. Stealth Immediately Broken by EnemyAI
**File**: `EnemyAI.cs`
**Problem**: When `StealthHandler` cleared `aggroTarget` to null, `EnemyAI.Update()` called `FindNearestPlayer()` on the very next frame, re-acquiring the cloaked Wraith (still tagged "Player", still nearest).
**Fix**: Added a `_suppressedUntil` timestamp. When `SetAggroTarget(null)` is called externally (stealth), the AI waits 6 seconds before searching for a new target. Visual result: enemies lose the Wraith and stand confused for the duration of the cloak.

### 4. Dodge Roll Added
**File**: `PlayerMovement.cs`
**Details**: See Dodge Roll section above. Two charges, 5s recharge per charge, 0.35s i-frames, directional burst.

---

## Numbers Reference — All Ability Values

### Engineer
| Ability | CD | Damage | Range | Notes |
|---------|-----|--------|-------|-------|
| Sentinel Drop | 6s | Turret auto | — | Max 3 deployables |
| Dark Blast | 3s | 10–30 | Cone | Hold to charge |
| Repair Wave | 5s | Heal 25 | 8u AoE | |
| Arc Cannon | 4s | 15–50 | Rectangle | Hold to charge |
| Thermal Charge | 4s | 20–45 | 5u Circle | Hold to charge |
| Shock Mine | 5s | 40 burst | Place | 2.5u blast radius |
| Overdrive | 12s | — | 12u AoE | Team CDR aura (now implemented) |
| Field Repair | 6s | Heal 40 | — | + debuff cleanse |
| Drone Command | 8s | — | 2u | Redirect turrets |
| System Overload | 45s | — | — | Engineer ultimate |

### Guardian
| Ability | CD | Damage | Range | Notes |
|---------|-----|--------|-------|-------|
| Kinetic Reversal | 10s | 20–60 | 8u cone | Absorb 3s → release |
| Magnetize | 7s | — | Pull | CC |
| Breach Slam | 6s | 25 | 6u dash | + Stagger 0.8s |
| Siege Mode | 14s | — | Self | 40% DR, root, ×3 Threat |
| Iron Tether | 9s | — | 8u | Leash 5s |
| Last Bastion | 50s | — | Wall | Blocks projectiles 10s |
| Deflect Protocol | 8s | — | Self | Shield 50 absorb |

### Wraith
| Ability | CD | Damage | Range | Notes |
|---------|-----|--------|-------|-------|
| Neural Spike | 5s | 35 (52.5 from cloak) | 5u circle | Interrupt |
| Stasis Wave | 6s | 15 | 5u circle | |
| Phase Cloak | 10s | — | Self | 4s stealth (+3s near Shadow Relay) |
| Null Field | 12s | 4/s DOT | 5u zone | + Suppress |
| Shadow Relay | deployable | — | — | Extends cloak, CDR on kill |
| Collapse | 40s | 20×stacks | 4u radius | Wraith ultimate |

### Phaser  
| Ability | CD | Damage | Range | Notes |
|---------|-----|--------|-------|-------|
| Phase Shift | 4s | — | 10u | Teleport |
| Singularity | 9s | 20 burst | 8u pull | 3s pull (+2s near Phase Relay) |
| Arc Lance | 7s | 30→25→20→15 | Chain | 4 jumps, 5 falloff |
| Phase Relay | deployable | — | 10u influence | Extends Singularity |
| Event Horizon | 50s | 60 | 8u | + Exposed 8s |

### Medic
| Ability | CD | Damage | Range | Notes |
|---------|-----|--------|-------|-------|
| Transfer Protocol | 9s | — | 12u | Redirect 100% incoming to Medic 5s |
| Nanite Swarm | 7s | Heal 30 | targeted | + 5 chip to enemies en route |
| Defibrillator | 14s | 60 (robotic) | 2u | Or revive downed ally at 50% HP |
| Adaptive Shield | 10s | — | Ally | 20 absorb, grows with damage taken |
| Purge Protocol | 7s | — | Ally | Cleanse all debuffs |
| Restoration Beacon | 6s | Heal 12/3s | 8u zone | 30s lifetime |
| System Rollback | 60s | — | Team | Rollback all positions + HP 5s |

---

## Combat Flow Timing — What Good Play Looks Like

A well-played 3-minute dungeon fight looks like:

**0:00** — Engineer drops Sentinel, plants mines at the entrance  
**0:03** — Wraith phases in, drops Null Field on the elite, Shadow Relay in the backline  
**0:06** — Guardian Breach Slams the staggered group, Kinetic Reversal  
**0:08** — Medic drops Restoration Beacon under the Guardian  
**0:10** — Elite is suppressed + DOT, Guardian absorbs its flailing attacks  
**0:12** — Kinetic Reversal bursts (60 damage cone), Wraith Collapses (40 burst)  
**0:14** — Elite dies. Adds hit the mines.  
**0:20** — Guardian Iron Tethers the last surviving add. Cleanup.  

That's a 20-second encounter with 4 coordinated players. Every ability was used, every cooldown mattered, nobody stood still. That's the feel.

---

## Outstanding Work (Not Yet Done)

| Item | Priority | Notes |
|------|----------|-------|
| Iron Tether 15% damage redirect | High | Documented in spellbook, not implemented in IronTetherHandler |
| Wall climbing | Medium | Needs Kinematic Character Controller or custom implementation |
| Phase Cloak duration in spellbook | Low | Currently hardcoded fallback 4s in AbilityCaster; add `activeDuration = 4f` to spellbook entry |
| Kinetic Reversal "Lightning cone" VFX | Low | Third VFX slot was designed but no public field exists in the handler |
| Siege Mode speed restoration edge case | Low | Should save baseMoveSpeed not currentSpeed to avoid slow-stacking bug |
| EnemyAI elite behavior | Low | `isElite` field is declared but never branched on |
| Defibrillator priority edge case | Low | Can't damage robots while a downed ally is nearby — may want to add a modifier-key override |
