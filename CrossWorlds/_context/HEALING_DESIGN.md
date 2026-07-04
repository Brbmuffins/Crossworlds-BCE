# Crossworlds BCE — Healing Feel Design

Status: Phase 1 polish + Phase 2 expansion  
Last updated: 2026-06-28

---

## The Problem

Healing in BCE currently has no feel. Functionally it works — `Health.Heal()` increments HP — but the player casting the heal gets nothing back. No number, no sound, no visual confirmation. The healed ally doesn't feel saved. The Cleric has no feedback that their kit is working.

Good healing feel has three layers: **confirmation** (I cast it), **impact** (it landed), and **payoff** (they survived because of me). BCE has none of these right now.

---

## Design Principles

**1. Healing numbers should feel earned, not clinical.**  
A crit heal should pop. A chain of Spirit Wisps landing should feel like a cascade. Numbers should be green, legible, and slightly larger than damage numbers to visually compete for attention.

**2. The healed player should know who saved them.**  
Soul Bond tether, Mend flash — the target should see something happen on their screen too, not just the Cleric.

**3. Cleric survivability is tied to Triage Loop.**  
The 8% self-heal is invisible right now. Make it visible — a small golden pulse on the Cleric's HP bar every time Triage fires. This is the Cleric's reward for healing well.

**4. Urgency has a visual language.**  
Low HP allies should pulse red to Cleric. High-priority heal targets need to be obvious at a glance.

---

## Per-Ability Feel Improvements

### Mend (index 11) — Single target direct heal
**Current:** Health.Heal(40) called silently. No VFX, no number.

**Target feel:**
- Green floating number (+40) on the healed target, same pool as `FloatingDamageText` but green, slightly larger font
- 0.3s green flash on the healed target's nameplate border
- Cleric: brief golden ring particle at cast point (radius 0.5, 8 particles, 0.5s lifetime)
- If Mend also cleanses a debuff: purple "CLEANSED" text appears above the +40 number

**Unity script:** Extend `CastFieldRepair()` in `AbilityCaster.cs`. After `h.Heal(healAmt)`, call `FloatingDamageText.Spawn(pos, amount, DamageType.Heal)` — add `Heal` to the enum and handle green color.

---

### Spirit Wisps (index 24) — Mobile healing orbs
**Current:** `NaniteSwarmBehaviour` spawns orbs but they have no trail and are nearly invisible.

**Target feel:**
- Each wisp: soft green point light (range 2, intensity 1.5), trailing particle system (6 particles, 0.3s lifetime, green-white)
- When a wisp reaches the target: small burst (8 particles, radial), green +heal number
- If multiple wisps land in sequence: each successive number is slightly larger (cascade feel, max 1.5× size)
- Cleric sees a soft green halo on themselves each time a wisp heals

---

### Soul Bond (index 23) — Damage redirect tether
**Current:** Damage redirects silently. No visual connection between Cleric and ally.

**Target feel:**
- Visible tether line: `SoulBondTether.cs` — `LineRenderer` between Cleric and bonded ally, gold/amber color, 0.05 width, slow pulse animation (alpha 0.4 → 0.8, 1s cycle)
- When damage redirects: tether flashes white briefly (0.1s), "BOND ABSORBED" text appears near midpoint of tether
- Bonded ally sees a small gold shield icon above their health bar while bond is active
- On bond expiry: tether dissolves with 0.3s fade + small particle scatter

**Unity script:** `SoulBondTether.cs` — MonoBehaviour, created at cast time by `CastTransferProtocol()`. Takes two transforms (Cleric + target). Destroys itself when `TransferProtocolHandler` reports inactive.

---

### Sacred Aegis (index 26) — Growing shield
**Current:** Shield absorbs hits and grows, but no UI shows the current value.

**Target feel:**
- `ShieldValueHUD.cs` — world-space bar above shielded ally (distinct from health bar, blue-white color, below the green nameplate)
- Bar fills as shield grows. Shows current absorb value as text: "Shield: 45"
- On each hit absorbed: bar briefly flashes white, shield value ticks up with a bounce
- When shield breaks: bar shatters (particle burst, blue-white), "AEGIS BROKEN" text
- Cleric: small confirmation flash on their screen edge when the shield absorbs a hit (they're in range — they need to know it's working)

---

### Divine Spark (index 25) — Revive
**Current:** `Health.Revive(0.30f)` called. No drama.

**Target feel:**
- Revived player: full-screen white flash (0.15s) fading to transparent, then "REVIVED" text in gold
- AoE visual: pillar of white/gold light at target position, 1.5s duration (simple cylinder mesh with emissive material + point light)
- Cleric: "REVIVED" text appears in the Cleric's screen in the same gold color — confirmation that the priority cast worked
- Sound design intent: sharp rising chime (not a heal sound — resurrection is a distinct event)

---

### Dispel (index 27) — Full debuff cleanse
**Current:** `StatusEffectManager.RemoveAll()` called silently.

**Target feel:**
- Each debuff removed: brief colored particle for that debuff type (Cursed = purple smoke puff, Weakened = orange burst, Slow = blue ripple)
- Text: "CLEANSED ×N" where N = number of effects removed
- If Dispel removes Dark Harvest stacks from Silence Ward (Shadowblade debuffs): golden "PURGED" text — this is a big deal

---

### Temporal Grace (index 28) — Full team rewind
**Current:** `SnapshotSystem.Rollback(5f)` called. Zero visual.

**Target feel:**
- This is the most powerful ability in the game. It needs to feel cosmic.
- Cast: brief screen freeze (1 frame), then all players snap-dissolve and reappear at old positions
- During rewind: reverse time VFX — all particles on screen play backward for 0.3s, blue-white chromatic aberration edge
- After rewind: all affected players get a 1s invulnerability gold outline
- HUD text: "TIME REWOUND" in large, fading gold text at top-center
- Sound design intent: deep reverb whoosh + time-stretch audio effect

---

## Triage Loop Feedback

The Cleric's passive (8% of each ally heal returns to Cleric HP) is invisible. Make it visible:

- `TriageLoopFeedback.cs` — MonoBehaviour on Cleric, attached by `PassiveTriageLoop`
- Hooks into `Health.onHealApplied` on self
- Each Triage return: small golden "+N" text above Cleric's own HP bar (smaller than normal heal numbers, distinctly golden not green)
- Over time: Cleric who is actively healing sees a steady stream of small golden numbers ticking back — the visual reward for doing their job

---

## Universal Healing Numbers

Extend `FloatingDamageText.cs` with heal support:

```csharp
public enum DamageType { Normal, Critical, Heal, HealCrit, Shield, TriageReturn }

// In Spawn():
case DamageType.Heal:       color = new Color(0.2f, 0.9f, 0.3f); scale = 1.1f; break;
case DamageType.HealCrit:   color = new Color(0.4f, 1.0f, 0.4f); scale = 1.4f; break;
case DamageType.Shield:     color = new Color(0.4f, 0.7f, 1.0f); scale = 1.0f; break;
case DamageType.TriageReturn: color = new Color(1.0f, 0.8f, 0.2f); scale = 0.8f; break;
```

Heals float upward and slightly toward the caster (visual homing effect — feels like returning energy).

---

## Ally Health Urgency System

The Cleric needs to identify low-HP allies at a glance in a fight.

`ClericRadarUI.cs` — MonoBehaviour, active only when `PlayerProgressManager.Local.ClassIndex == 3` (Cleric):
- Scans all ally `Health` components every 0.5s
- Any ally below 40% HP: their nameplate outline pulses red (sine wave alpha)
- Any ally below 20% HP: red outline + nameplate scales up 1.2× + soft ping sound
- The lowest-HP ally always shown in a small portrait in the top-left corner of the Cleric's screen

This is the "healer radar" — Cleric always knows who needs saving without breaking camera rotation.

---

## Healing Sound Design Intent (for future audio pass)

These are design notes for when SFX are added. Each heal ability should have a distinct audio identity:

| Ability | Sound Character |
|---|---|
| Mend | Warm chime, single note, resonant tail |
| Spirit Wisps | Soft sustained tone that builds as wisps land, cascade of light bells |
| Soul Bond | Low harmonic drone when bond active, sharp click on absorb, dissolve on expire |
| Sacred Aegis | Crystalline shimmer on apply, glass crack on each hit absorbed, shattering sound on break |
| Divine Spark | Rising chime cue → silence → soft exhale (resurrection breath) |
| Dispel | Wind sweep, cleansing whoosh, each removed effect has a small pop |
| Temporal Grace | Deep reverb swell, reverse audio blur, clock-tick stutter, release |
| Triage Loop passive | Almost silent — barely audible warm tick, like a heartbeat pulse |

---

## Scripts to Write (implementation order)

1. **Extend `FloatingDamageText.cs`** — add `DamageType.Heal`, `HealCrit`, `Shield`, `TriageReturn`
2. **`SoulBondTether.cs`** — LineRenderer visual between Cleric and bonded ally
3. **`ShieldValueHUD.cs`** — world-space shield bar above shielded ally
4. **`ClericRadarUI.cs`** — low-HP ally detection + nameplate pulse, Cleric-only
5. **`TriageLoopFeedback.cs`** — golden tick numbers on Triage passive returns
6. **Extend `AbilityCaster.cs`** — add `FloatingDamageText.Spawn()` calls in Mend, Divine Spark, Dispel
7. **Extend `NaniteSwarmBehaviour.cs`** — add trail particles + landing burst to Spirit Wisps
8. **Extend `SnapshotSystem.Rollback()`** — add screen flash + "TIME REWOUND" UI call
