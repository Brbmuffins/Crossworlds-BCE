# Crossworlds BCE — Combat Atlas
## Source of Truth for the Interactive Ability Web

Generated from: `AbilityCaster.cs`, `WraithAbilities.cs`, `StatusEffect.cs`, `Passive*.cs`  
Last updated: 2026-06-28

---

## Heroes at a Glance

| Index | Hero | Color | Role | Passive |
|---|---|---|---|---|
| 0 | **Warden** | `#4a90e2` | Construct Tactician | Overengineered — deployables near you gain +8% output per stack (every 4s, max 5) |
| 1 | **Ironclad** | `#f0a020` | Iron Vanguard | Threat Protocol — every 5 hits taken: redirect nearby enemy aggro + 20% DR for 6s |
| 2 | **Arcanist** | `#e74c3c` | Void Mage | Phase Charge — every 6 casts: next offensive ability deals +40% damage |
| 3 | **Cleric** | `#2ecc71` | Soul Warden | Triage Loop — 8% of each ally heal you apply returns to your own HP |
| 4 | **Shadowblade** | `#9b59b6` | Void Infiltrator | Bounty System + Corruption — kill = −2s CDR (−5s on elite); every ability hit adds a Cursed stack (8 dmg/s, 4s) |

---

## Status Effects

These are the only status effect types in the engine. Synergies depend on applying and consuming these correctly.

| Effect | What it does | Primary source |
|---|---|---|
| **Slow** | Reduces move speed by `value` fraction | Void Bolt, Nature's Grasp |
| **Stagger** | Brief interrupt — skips next attack tick | Shieldwall Charge |
| **Silenced** | Cannot use abilities | Silence Ward zone |
| **Cursed** | DoT — deals `value` damage per second | Shadowblade Corruption passive, Silence Ward |
| **Weakened** | Incoming damage +25% | Collapsing Void (Event Horizon mode) |
| **Bound** | Cannot move beyond max range | Rune Chain leash |

`Dark Harvest` consumes ALL active debuff stacks on targets in range: 20 damage per stack.

---

## Shared Abilities — All Heroes (indices 0–7)

Every hero can equip any of these 4 slots.

| # | Name | Shape | Category | Range | CD | Key Stat | What it does |
|---|---|---|---|---|---|---|---|
| 0 | **Runic Sentinel** | Circle | Support | 10 | 6s | Deploys turret | Place an auto-attacking construct. Warden can have 3 active; others 1. Overengineered passive stacks on this. |
| 1 | **Void Bolt** | Cone | Damage | 8 | 3s | 10–30 dmg (chargeable) | Chargeable cone burst. Applies Cursed + Slow (Shadowblade). Charge tint turns purple at max. |
| 2 | **Mending Circle** | Circle | Heal | 6 | 5s | AoE heal | Drop a healing zone (radius 3). Triage Loop passive triggers on Cleric for each hit. |
| 3 | **Storm Lash** | Rectangle | Damage | 10 | 4s | 15–50 dmg (chargeable) | Chargeable lightning sweep. Chains through grouped enemies. |
| 4 | **Ember Surge** | Circle | Damage | 12 | 4s | 20–45 dmg (chargeable) | Chargeable AoE fireball. Charge tint turns orange. Triggers Phase Charge meter. |
| 5 | **Mind Spike** | Circle | Damage | 10 | 5s | 35 dmg | Precision psychic shot. Key synergy target — deals bonus vs debuffed enemies. |
| 6 | **Binding Wave** | Circle | Damage | 5 | 6s | 15 dmg | Short-range AoE burst. Applies Stagger. Useful to interrupt cast windows. |
| 7 | **Arcane Ward** | Circle | Support | 0 | 8s | 50 absorb / 5s | Instant self-cast shield. Blocks next 50 damage. Cannot be aimed — fires immediately. |

---

## Warden (indices 8–12)

**Playstyle:** Field commander. Place constructs, redirect them, amplify their output, then surge them all at once. The more constructs active, the stronger they become via Overengineered stacks.

**Class limit:** Max 3 active deployables at once.

| # | Name | Shape | Category | Range | CD | Key Stat | What it does |
|---|---|---|---|---|---|---|---|
| 8 | **Runic Snare** | Circle | Damage | 8 | 5s | 40 dmg | Place a proximity burst rune trap. Enemy walks over it → detonation. Owner context preserved for Overengineered stacking. |
| 9 | **Battle Hymn** | Circle | Support | 0 (self) | 12s | 30% CDR aura / 8s | Instant self-cast. All allies within 12u gain 30% cooldown reduction for 8s. CDR is clamped to 60% max. |
| 10 | **Spirit Redirect** | Circle | Support | 12 | 8s | Turret focus | Target an enemy. All of your active Runic Sentinels immediately focus that target for 6s. |
| 11 | **Mend** | Circle | Heal | 6 | 6s | 40 heal | Single-target direct heal on nearest ally at cast point. Also clears 1 debuff (RemoveAll). |
| 12 | **Conjurer's Surge** *(Ultimate)* | Circle | Support | 0 | 45s | 8s overload | All active constructs enter overload mode: rapid-fire rate for 8s. Stackable with Overengineered. |

---

## Ironclad (indices 13–18)

**Playstyle:** The anvil. Absorb punishment, convert it into damage, force enemies to attack you, and wall off areas. Threat Protocol fires automatically from taking hits — the more aggression the Ironclad draws, the more DR they accumulate.

| # | Name | Shape | Category | Range | CD | Key Stat | What it does |
|---|---|---|---|---|---|---|---|
| 13 | **Counter Blow** | Cone | Support | 8 | 10s | Up to 60 dmg | Activate: absorb incoming hits for 3s, then release as a 70° cone burst. Damage scales with hits absorbed. |
| 14 | **Gravity Slam** | Circle | Support | 10 | 7s | Pull radius 4 | Pull all enemies in a 4u radius toward the cast point over 2s. No damage — pure setup. Combo with teammate AoE. |
| 15 | **Shieldwall Charge** | Rectangle | Damage | 6 | 6s | 25 dmg | Charge forward 6u through enemies. Deals 25 dmg + Stagger. Adds 3 Threat Protocol stacks directly. |
| 16 | **Stalwart Stance** | Circle | Support | 0 | 14s | 40% DR / 6s | Stationary stance: 40% damage reduction and 3× Threat stack generation for 6s. Cannot move while active. |
| 17 | **Rune Chain** | Circle | Support | 8 | 9s | Leash 5s | Leash a single enemy within 8u. They cannot move beyond leash range for 5s. Absorbs 15% of leashed enemy's attacks on allies. |
| 18 | **Iron Rampart** *(Ultimate)* | Rectangle | Support | 8 | 50s | 10s wall | Deploy a full-width stone rune wall. Blocks all projectiles. 8u wide, 10s duration. Splits the arena. |

---

## Arcanist (indices 19–22)

**Playstyle:** Void convergence. Blink into position, collapse space, detonate clusters. Phase Charge rewards rapid casting — every 6 casts, the next offensive hit hits 40% harder. Manage your blink cooldown to stay inside the kill zone without dying.

| # | Name | Shape | Category | Range | CD | Key Stat | What it does |
|---|---|---|---|---|---|---|---|
| 19 | **Arcane Step** | Circle | Support | 10 | 4s | Teleport | Instantly blink to cursor position. Leaves a void echo that detonates for 20 dmg 1s later. |
| 20 | **Void Maw** | Circle | Damage | 10 | 9s | 20 dmg + pull | Open a void singularity (8u radius). Pulls all enemies toward center for 3s, then 20 AoE burst. |
| 21 | **Forked Lightning** | Circle | Damage | 10 | 7s | 30/25/20/15 chain | Chain lightning. Hits up to 4 enemies in sequence (6u jump radius). Damage: 30 → 25 → 20 → 15. |
| 22 | **Collapsing Void** *(Ultimate)* | Circle | Damage | 14 | 50s | 60 dmg + Weakened | 12u pull zone, 3s collapse, 60 AoE burst. Applies Weakened to all hit enemies (+25% damage taken). Uses separate `eventHorizonPrefab` (applyExposed=true). |

---

## Cleric (indices 23–28)

**Playstyle:** Soul anchor. Keep the team alive through burst and sustained damage by bonding yourself to allies, healing them proactively, and rewinding disasters with Temporal Grace. Triage Loop means every heal you land also heals you — sustained presence without babysitting your own HP.

| # | Name | Shape | Category | Range | CD | Key Stat | What it does |
|---|---|---|---|---|---|---|---|
| 23 | **Soul Bond** | Circle | Support | 8 | 9s | Damage redirect / 5s | Tether yourself to an ally (TransferProtocolHandler). All damage they take reroutes to you for 5s. Syncs with Arcane Ward to absorb the redirected damage. |
| 24 | **Spirit Wisps** | Circle | Heal | 10 | 7s | Mobile heal orbs | Spawn drifting healing orbs (NaniteSwarmBehaviour) that home toward the nearest ally. Chip damage to enemies they pass through. |
| 25 | **Divine Spark** | Circle | Heal | 6 | 14s | Revive 30% HP or 60 dmg | Priority 1: revive a downed ally in 2u at 30% HP. Priority 2: deal 60 dmg to robotic enemies. |
| 26 | **Sacred Aegis** | Circle | Support | 8 | 10s | 20→80 absorb / 8s | Apply a scaling shield on an ally. Starts at 20 absorb, grows by +10 per hit taken, over 8s. |
| 27 | **Dispel** | Circle | Support | 8 | 7s | Full cleanse | Instantly remove ALL active debuffs (StatusEffectManager.RemoveAll) from target ally. |
| 28 | **Temporal Grace** *(Ultimate)* | Circle | Heal | 0 | 60s | Full team rewind 5s | SnapshotSystem.Rollback(5f). Reverts ALL players to their state 5 seconds ago: HP, position, active debuffs. No animation. Instant cast. |

---

## Shadowblade (indices 29–31)

**Playstyle:** Curse engine. Every ability hit applies a Corruption stack (Cursed DoT, 8 dmg/s, 4s). Build stacks via Void Bolt, Silence Ward, Dark Mark. Then Dark Harvest detonates all stacks simultaneously for massive burst. Shadow Veil makes you invulnerable for 2s and curses everything nearby on exit.

**Passive — Corruption:** Every ability hit applies a Cursed stack (8 dmg/s, 4s). Stacks are independent timers. Dark Harvest consumes all stacks.

**Additional ability — Dark Mark [E]** (from WraithAbilities.cs, not in AbilityCaster spellbook):  
Single-target mark. Applies Weakened (+25% dmg taken) and a Cursed stack. No aim required — nearest enemy.

| # | Name | Shape | Category | Range | CD | Key Stat | What it does |
|---|---|---|---|---|---|---|---|
| 29 | **Shadow Veil** *(Ultimate)* | Circle | Support | 0 | 10s | 2s invulnerable / stealth | Enter full stealth + invulnerability for 2s (StealthHandler.BeginCloak). On exit, Corruption passive applies Cursed to ALL enemies in range. First attack from stealth deals the Corruption proc instantly. |
| 30 | **Silence Ward** | Circle | Support | 10 | 12s | Silence zone / 5s | Deploy a null-field zone (5u radius). Enemies inside: Silenced (cannot cast) + Cursed DoT per tick. Zone lasts 5s. Core stack-builder. |
| 31 | **Dark Harvest** | Circle | Damage | 8 | 40s | 20 dmg per debuff stack | Detonate ALL active debuff stacks on every enemy in 8u radius. 20 damage per stack. Consumes stacks. A target with 6 stacks takes 120 damage instantly. Core combo finisher. |

---

## Core Synergies (Code-Derived)

These are interactions that exist in the actual C# logic, not design intentions.

| Synergy | Abilities | Mechanic |
|---|---|---|
| **Curse → Harvest** | Any curse source → Dark Harvest (31) | Dark Harvest reads `StatusEffectManager.ConsumeDebuffStacks()` — 20 dmg per stack. More stacks = more burst. |
| **Void Collapse** | Void Maw (20) → Collapsing Void (22) | Void Maw gathers enemies to center. Collapsing Void detonates the cluster for 60 + Weakened. Both use SingularityBehaviour. |
| **Phase Amplify** | Any 6 casts → Phase Charge passive → offensive | Arcanist's 6th cast after a dry period gets +40% damage. Time Collapsing Void as the 6th cast. |
| **Bond Absorb** | Soul Bond (23) + Arcane Ward (7) | Soul Bond redirects ally damage to Cleric. Arcane Ward (50 absorb) blocks that redirected damage. Cleric becomes a 0-damage sponge. |
| **Rune Lock** | Rune Chain (17) → any AoE | Rune Chain leashes an enemy in place for 5s. Team can pile AoE damage on a target that literally cannot move. |
| **Charge Repel** | Shieldwall Charge (15) → Gravity Slam (14) | Charge knocks enemies back. Gravity Slam immediately pulls them back into a tight cluster. Back-to-back = forced group. |
| **Stance Absorb** | Stalwart Stance (16) → Counter Blow (13) | Stalwart Stance generates 3× Threat stacks per hit. Counter Blow then releases absorbed damage as burst. Stack farming into burst release. |
| **Hymn Overload** | Battle Hymn (9) + Conjurer's Surge (12) | Battle Hymn gives 30% CDR to all. Conjurer's Surge sends constructs into rapid-fire overload for 8s. Together: maximized construct output window. |
| **Redirect + Redirect** | Soul Bond (23) + Counter Blow (13) | Ironclad absorbs ally damage via Counter Blow. Cleric bonds ally so *their* damage redirects to Cleric. Both effects simultaneously. |
| **Rewind Safety** | Temporal Grace (28) + any wipe scenario | Temporal Grace rolls back HP, position, and debuffs 5 seconds. Cast it the moment a wipe starts, not after. Requires SnapshotSystem tracking all players. |
| **Ward Silence Harvest** | Silence Ward (30) → Dark Mark → Dark Harvest (31) | Silence Ward applies Silenced + Cursed stacks. Dark Mark adds Weakened + Cursed. Dark Harvest detonates everything. 3-step team kill. |
| **Construct Focus** | Spirit Redirect (10) + Runic Sentinel (0) | Spirit Redirect instantly pivots all active Warden turrets to focus one target for 6s. Combined with Overengineered stacks, burst is significant. |
| **Phase Blink Cluster** | Arcane Step (19) + Forked Lightning (21) | Blink into melee range of clustered enemies. Forked Lightning chains through maximum targets at close range. |
| **Triage Mend** | Mend (11) + Triage Loop passive | Cleric's Triage Loop passive only triggers on heals Cleric applies. Mend heals ally → 8% of 40 = 3.2 HP back to Cleric each cast. Low but free. |
| **Dispel Timing** | Dispel (27) + high-debuff scenarios | Most effective against Shadowblade DoT stacks before Dark Harvest fires. Also clears Weakened from Collapsing Void. |
| **Wisp Bond** | Spirit Wisps (24) + Soul Bond (23) | Wisps drift toward bonded ally. Heals land on them → Cleric takes the redirected damage → Triage Loop feeds Cleric HP back. |

---

## Mechanics Reference

### Cast Flow
1. Press 1/2/3/4 → ability enters aim mode (indicator appears at cursor)
2. Left-click → `FinalizeCast` → `DispatchAbility` → effect applied
3. Right-click / Escape → cancel aim with no CD
4. Instant-cast abilities (range=0, shieldAbsorb>0) fire immediately on keypress — no aim phase

### Chargeable Abilities
Void Bolt, Storm Lash, Ember Surge support holding for max charge:
- `maxChargeTime` = 1.5s
- Damage lerps from `damage` → `maxChargeDamage`
- Indicator size scales by `maxChargeSizeMultiplier`
- Charged tint applied if `chargedTint.a > 0`

### Deployables
Managed by `DeployableManager`. Each deployable is registered with an owner ID and a class limit.
- Warden: limit 3
- All others: limit 1
- Exceeding limit destroys the oldest deployable of that type
- `PhaseRelayDeployable.GetBonusNearPoint()` can extend Void Maw/Collapsing Void pull duration if a Phase Relay is nearby (Arcanist class deployable)

### Ability Shapes
- **Circle**: `Physics.OverlapSphere(center, indicatorSize/2)`
- **Rectangle**: `Physics.OverlapBox(center, halfExtents, rotation)`
- **Cone**: `Physics.OverlapSphere` + angle check `Vector3.Angle < coneAngle/2`

### Damage Scaling
Total damage = `ability.damage × PhaseChargeMult × CharacterStats.DamageMultiplier`

### Cooldown Reduction
- Base cooldown from `ability.cooldown`
- CDR from `CharacterStats.CooldownReduction` (gear/attunement)
- Clamped: `CooldownFor(ability) = ability.cooldown × (1 - CDR)`
- Battle Hymn adds temporary CDR via `AddTemporaryCDR()` clamped to 60% total

---

## VPS Build Brief — Interactive Combat Web

**What to build:** A single HTML page at `/var/www/crossworlds/combat/index.html`

**Design reference:** Blizzard WoW Dragonflight talent web — clean, dark, no cartoon icons

**Visual rules:**
- Background: deep void, near-black (`#06080f` → `#0c0e1a` radial gradient)
- Nodes: clean circles or hexagons, colored by hero — no emoji, no icons
- Each node: hero color border, subtle fill, hero class tag, ability name in a clean sans-serif
- Connections: thin glowing lines between synergistic abilities, colored by the synergy's dominant class
- Font: Inter, Barlow, or system-ui — never decorative
- No gradients on text, no drop shadows on text, no glow on text
- The page feels like a diagram, not a game menu

**Node layout:**
- 5 hero clusters arranged in a pentagon
- Shared abilities (0-7) in the center overlapping all clusters
- Within each cluster: abilities arranged close together so connection lines are short
- Ultimates (12, 18, 22, 28, 29) at the outer edge of each cluster

**Interactions:**
- Hover node: dim everything else to 20%, highlight this node and its connections
- Click node: side panel with full ability description and real stats from this file
- Click a synergy partner name in the side panel: jump to that node
- Class filter buttons (top): show/hide clusters — shared always visible
- Synergy lines pulse on hover to show direction of dependency

**Data source:** Use EXACTLY the ability names, stats, and synergy descriptions from this file — no invented abilities.

**File:** Self-contained single HTML file. Load D3.js from CDN for force graph layout if needed.

**Deploy:**
```bash
mkdir -p /var/www/crossworlds/combat
# write index.html
chown -R www-data:www-data /var/www/crossworlds/combat/
chmod -R 755 /var/www/crossworlds/combat/
nginx -t && systemctl reload nginx
curl -I https://playcrossworlds.com/combat/
```
