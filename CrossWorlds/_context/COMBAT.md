# COMBAT.md — Classes, Abilities, Combat Design

> Load this file when: designing or debugging class abilities, combat feel,
> enemy behavior, damage systems, hit registration, or adding a new class.

**Last verified:** 2026-06-28

---

## The 5 Classes

| class_index | Name | Role | Playstyle |
|---|---|---|---|
| 0 | Engineer | Utility / builder | Turrets, gadgets, area control |
| 1 | Guardian | Tank / frontline | High armor, taunts, sustain |
| 2 | Shadowblade | Burst / rogue | Stealth, backstab, high single-target |
| 3 | Cleric | Healer / support | Heals, buffs, crowd control |
| 4 | Arcanist | Ranged magic DPS | Spells, AoE, elemental damage |

**One character per account. Class chosen at creation, never changes.**

---

## Stat System

Each class has a primary stat that scales their damage:

| Class | Primary Stat | Secondary |
|---|---|---|
| Engineer | INT | AGI |
| Guardian | VIT | STR |
| Shadowblade | AGI | STR |
| Cleric | INT | VIT |
| Arcanist | INT | — |

Stats in DB on `characters` table: `stat_str`, `stat_agi`, `stat_int`, `stat_vit`

Base stats at creation: STR 5, AGI 5, INT 5, VIT 10
Stats scale per level — class-specific bonuses applied on level up.

Equipment `stat_bonus` JSON (from `items` table) is additive on top of base stats.

---

## Seeded Gear by Class

| Class | Starting weapon option |
|---|---|
| Engineer / Guardian | `sword_copper` (STR) |
| Shadowblade | `dagger_shadow` (AGI + STR) |
| Cleric | `tome_cleric` (INT + VIT) |
| Arcanist | `staff_apprentice` (INT) |

Universal armor: `plate_copper` (VIT), `ring_copper` (STR + VIT)

---

## Ability Design Principles

1. **Every class has a primary attack** — fast, low cooldown, bread-and-butter
2. **Every class has a heavy attack** — slower, higher damage or effect
3. **Every class has a mobility tool** — dash, blink, shield bash, or repositioning ability
4. **One defining class mechanic** — the thing that makes the class feel different

### Guardian
- Primary: Shield Bash — melee hit, short stagger
- Heavy: Ground Slam — AoE knockback
- Mobility: Charge — rush forward, pin target
- Class mechanic: Taunt — forces nearby enemies to attack Guardian

### Shadowblade
- Primary: Quick Strike — fast double-hit
- Heavy: Backstab — massive damage from behind
- Mobility: Shadow Step — short-range blink
- Class mechanic: Stealth — brief invisibility, next attack is empowered

### Cleric
- Primary: Holy Strike — melee/ranged hybrid
- Heavy: Smite — burst holy damage
- Mobility: Divine Rush — forward dash + small heal
- Class mechanic: Mend — channeled heal on friendly target

### Arcanist
- Primary: Arcane Bolt — ranged projectile
- Heavy: Arcane Burst — AoE explosion at cursor
- Mobility: Blink — short teleport
- Class mechanic: Overcharge — next spell does double damage (short window)

### Engineer
- Primary: Wrench Strike — melee hit
- Heavy: Grenade — thrown AoE
- Mobility: Grapple Hook — pull to location
- Class mechanic: Deploy Turret — places auto-attacking turret (limited duration)

---

## Enemy Design (Week 4 target)

### Enemy types needed
| Type | Behavior | Drop |
|---|---|---|
| Grunt (melee) | Patrol → aggro → chase → attack → death | copper_shard, small gold |
| Ranged | Patrol → aggro → keep distance → projectile → death | copper_shard |
| Elite | Higher HP, harder hits, aggro radius 2× | copper_bar, gear |

### Enemy behavior loop
```
IDLE → (player within aggro radius) → CHASE
CHASE → (within attack range) → ATTACK
ATTACK → (target dead or out of range) → IDLE or CHASE
DEATH → play death anim → roll drop table → despawn after 3s
```

### NavMesh requirements
- Bake NavMesh in arena scene before enemy prefabs will pathfind
- Enemy prefab needs: `NavMeshAgent`, `EnemyController`, `EnemyHealth`, `DropTable`
- Set `NavMeshAgent.stoppingDistance` = attack range (e.g. 1.5 for melee, 5 for ranged)

---

## Drop Table Design

`DropTable` ScriptableObject on each enemy prefab:

```csharp
[System.Serializable]
public class DropEntry {
    public string itemId;      // matches items.id in DB (e.g. "material_copper_shard")
    public float weight;       // relative probability
    public int minQty;
    public int maxQty;
}

public class DropTable : ScriptableObject {
    public int minGold;
    public int maxGold;
    public List<DropEntry> drops;
    public float nothingWeight;  // chance of no item drop
}
```

**Grunt baseline drop table:**
- 60% nothing
- 30% copper_shard (1–2)
- 10% copper_bar (1)
- Gold: 1–5

**Elite drop table:**
- 20% nothing
- 40% copper_bar (1–2)
- 30% copper_shard (2–4)
- 10% random gear (sword_copper / plate_copper / ring_copper)
- Gold: 10–25

---

## Combat TODOs (Week 4 Unity)

- [ ] Enemy prefabs: grunt + ranged (NavMesh, aggro, attack hitbox, death anim)
- [ ] Wave spawner in arena (N enemies, escalating per wave)
- [ ] `DropTable` ScriptableObject + WorldItem prefab (float, rotate, rarity glow)
- [ ] Pickup sphere → POST /api/inventory/save
- [ ] Multiplayer hit confirmation (server-authoritative damage)
- [ ] Damage number display (Week 7 polish but can stub now)

## Combat TODOs (Week 7 Polish)

- [ ] Floating damage numbers (white = normal, yellow = crit, green = heal, red = taken)
- [ ] Hit VFX per damage type (spark, blood poof, magic burst)
- [ ] Class-specific ability VFX (fireball, slash trail, heal beam)
- [ ] Health bars on enemies
- [ ] Death animations
- [ ] Ability icon hotbar
