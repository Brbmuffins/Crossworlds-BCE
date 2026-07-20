# Crossworlds BCE — Aiming Cone & Combat Feel Suggestions
**Date:** 2026-07-19
**Author:** Combat design pass (tester lens)
**Scope:** The Healing Cone mechanic → generalized aim/AoE grammar across Healer/DPS/Tank; fluidity, readability, "everyone alive together" goal
**Status:** Proposal — numbers are starting points, tune in playtest

---

## The core insight

The Healing Cone (aim up = Bubble/shield, middle = HoT, bottom = HPS/instant) is a **one-input, aim-to-choose-intensity, hold-and-release** verb. That's a genuinely fresh feel. The mistake would be shipping it as a Cleric-only quirk. If the same *grip* — hold to charge, aim to pick the flavor, release to commit — is the spine of DPS and Tank too, the whole party reads the same, teaches the same, and the arena becomes one legible live space instead of three unrelated control schemes.

Everything below serves the stated combat goal: **people walk around, interact with each other and the world, and see everything resolve live, together, smoothly.**

---

## 1. Make the cone a shared verb across all three roles

Keep the exact interaction (hold → aim within a cone → release) and reskin the three zones per role. Same muscle memory, three fantasies.

| Role | Near zone (bottom) | Mid zone | Far zone (top) |
|---|---|---|---|
| **Healer (Cleric)** | HPS — instant burst | HoT — heal over time | Bubble — shield |
| **DPS (Arcanist/Shadowblade)** | Point-blank nuke (high dmg, small radius) | Cone spray (medium dmg, medium arc) | Long-range lance (low dmg, pierce/range) |
| **Tank (Ironclad/Guardian)** | Slam pull (drag enemies in) | Taunt wave (aggro + slow) | Shieldwall throw (ranged block / knockback line) |

Why this matters for fluidity: a player who mains DPS and later tries Cleric already knows the grip. Co-op reads become "he's charging near-zone, that's a point-blank, back off" — the aim direction *telegraphs intent to teammates*, not just to the game. That is the interaction-between-players goal, for free.

**Tunable:** one `AimConeAbility` ScriptableObject with three `ZoneDefinition` entries (angle range, payload ability id, radius, charge-scaling curve). Each role gets its own asset. No new input code per class.

---

## 2. Aiming feel — the part that decides whether this is smooth or fiddly

The infographic shows discrete zones. Discrete-only aiming feels notchy. Blend it:

- **Continuous charge, snapped commit.** While held, the cone brightens the zone the cursor is over and shows a soft gradient at the boundaries so the player *sees* the zone flip before they release. Snap the payload to the zone at release, but preview continuously.
- **Charge = power, aim = type.** Hold time scales magnitude (bigger shield / bigger nuke) up to a cap ~0.8s; aim picks the type. This gives one input two expressive axes without a second button. Cap it so holding is never strictly better than tapping — tap = fast weak, hold = slow strong. Suggested: 0.15s min cast, 0.8s full charge, magnitude 1.0×→1.8×.
- **Release deadzone / cancel.** Aiming straight back at yourself (or pressing the same key) cancels with no cooldown burn — panic-safe. Nothing kills flow like a mis-aimed heal eating a 6s cooldown.
- **Controller + mouse parity.** Right-stick tilt = cone aim. Since the zones are radial, both input types map cleanly. Design for both now so you're not retrofitting.
- **Server authority note:** the *release event* (aim vector + charge time) is the command. Client previews the cone locally for zero-latency feel; server validates vector/charge and applies the payload. Never let the client report the heal amount — it reports aim+charge, server computes. (Matches CLAUDE.md Mirror discipline.)

---

## 3. AoE effects — legibility first, spectacle second

The current combat audit flags the real problem: waves are a *chaos curve* (lots of equally-weak enemies) not a *tension curve*. AoE feel has to fight that noise, not add to it. Rules:

- **Ground decals before the pop.** Every AoE — heal zone, enemy telegraph, tank slam — paints a persistent ground decal during the wind-up so players read space, not just flashes. Heals = green ring, enemy = red/orange ring, tank = blue ring. Consistent color language across the whole game (heal green / threat red-orange / control blue). This is the single highest-leverage readability change.
- **Overlap = visible synergy.** When two friendly AoEs overlap (HoT zone + tank taunt zone), brighten the intersection. The proposal doc's Sanctified/transition ideas become *visible on the floor* — players see the safe pocket and stand in it. That's your "interact with each other" pillar rendered in light.
- **Additive glow, not opaque fills.** Big VFX flood a co-op screen fast. Use additive/bloom edges and keep zone interiors semi-transparent so you can always see allies and enemies *through* the effect. If the effect hides the fight, it failed.
- **Impact scales with charge.** A full-charge release gets a bigger burst + brief hitstop (~40–60ms on the caster only, never on other clients — see polish note). A tapped one is a small pop. The player *feels* the choice they made.

---

## 4. Keep the "everyone live together, smoothly" goal intact

This is a networked co-op arena. The feel goal lives or dies on netcode discipline:

- **Local-predict the cone, server-confirm the payload.** Cone visuals and charge UI are client-only (`#if UNITY_EDITOR || !UNITY_SERVER`, per CLAUDE.md — not `!UNITY_SERVER` alone). The payload spawn is `[Server]`. Players get instant feel; state stays authoritative.
- **Broadcast intent early.** Send a lightweight "player X started charging near-zone at vector V" so *other clients* can render the cone on the caster before release. That's what makes teammates able to react to each other — the whole social-combat point. One small ClientRpc on charge-start, throttled.
- **Cull VFX by relevance, not distance alone.** With 4 players + 22 enemies (wave 10 per the audit), unbounded particle systems will spike GC and framerate. Pool everything; cap concurrent AoE decals; LOD the particle counts. Loop in the perf-profiler skill before adding any system that scales with enemy count.
- **Nameplate/heal numbers pooled.** The healing doc already specs `FloatingDamageText` reuse — extend that pool for AoE tick numbers too rather than spawning per-tick. Per-tick instantiation on a HoT zone over 4 allies is a classic hitch source.

---

## 5. Big-VFX shopping list (free / license-friendly assets)

Prioritized for a Unity 6 URP project. Verify each license at download time — a couple are CC-BY (need attribution), a couple are fully free.

- **Unity VFX Graph + built-in URP samples** — free, first-party. Best path for the cone glow, additive AoE rings, and GPU-particle bursts that scale to many enemies without CPU cost. Start here.
- **Cartoon FX Remaster Free (Jean Moreno / JMO)** — Asset Store, free. Huge library of stylized bursts, heals, shields, impacts. Fast to wire, reads well at a distance — ideal for the Bubble/HoT/HPS pops and hit sparks.
- **Unity Particle Pack** — Asset Store, free, first-party. Fire, magic, explosions; good base to recolor into your green-heal / red-threat / blue-control palette.
- **Kenney.nl** — CC0 (no attribution needed). Particle textures, UI icons, and SFX packs. Perfect for the cone-zone icons (Bubble/HoT/HPS glyphs) and placeholder impact sounds.
- **Freesound.org** — CC0/CC-BY per clip. Source the healing sound-design intents from HEALING_DESIGN.md (warm chime, crystalline shimmer, reverse-time whoosh). Filter to CC0 to avoid attribution bookkeeping.
- **Sonniss GDC Game Audio Bundle** — free yearly, royalty-free. Large high-quality SFX library for impacts, whooshes, UI.
- **Mixamo** — free (Adobe account). If any telegraph/cast animations need distinct silhouettes (readability lens), grab cast/slam/block anims here.
- **Gabriel Aguiar Pro (YouTube) free shader/VFX tutorials** — not an asset but the fastest way to build a bespoke shield-bubble and additive cone shader in URP that matches your palette exactly.

Recommendation: build the **cone + AoE rings in VFX Graph** (scales, GPU-cheap, matches the live-together goal), use **Cartoon FX Remaster** for the moment-to-moment pops, and **Kenney/Freesound (CC0)** for icons and audio to avoid license friction.

---

## 6. Combat-structure suggestions (fresh-vibe retention)

- **Reward aim mastery, don't require it.** Center-zone should be forgiving; the *edges* (max range lance, point-blank nuke) are where skill pays off. New players tap the middle and survive; skilled players ride the boundaries.
- **Cross-role cone combos.** Tank's near-zone pull + DPS point-blank on the same cluster = a designed "stack and detonate" moment both players can see coming via each other's cones. Formalize 2–3 of these (they echo the proposal's synergy transitions) so pugs discover them naturally.
- **One threat that *forces* the far zone.** Give at least one enemy a mechanic that punishes hugging (e.g., a delayed nova) so the ranged cone zone has a reason to exist. Otherwise everyone defaults to near-zone and the aiming axis goes unused.
- **Tie a cone tier to decay/entropy hook.** Per the design pillars, wire at least one zone to durability — e.g., full-charge releases cost a sliver of gear durability, making the big pop a real decision. Keeps the mechanic inside Corrosion's identity instead of generic.
- **Fix the chaos curve first (from the 07-06 audit).** None of this feels good if wave 10 is just 22 identical weak grunts. Land the per-wave HP/damage scaling + aggro cap before polishing VFX, or the polish decorates a flat fight.

---

## Implementation order (lowest risk → highest payoff)

1. `AimConeAbility` + `ZoneDefinition` ScriptableObjects (data first, no magic numbers).
2. Client-side cone preview (continuous highlight, boundary gradient, cancel deadzone) — feel proof before any payload work.
3. Ground-decal color language (green/red-orange/blue) shared across heals, telegraphs, tank zones — biggest readability win.
4. Server payload application per zone (`[Server]`, validated aim+charge).
5. Charge-start ClientRpc so teammates see each other charging.
6. VFX pass (VFX Graph rings + Cartoon FX pops), pooled, perf-profiled.
7. Cross-role combo tuning + one anti-hug enemy + decay hook.

Run the netcode-reviewer skill on steps 4–5 and perf-profiler on step 6 before merge.
