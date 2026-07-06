-- ============================================================
-- Crossworlds BCE — Arena Content Seed
-- Generated: 2026-07-06 by content-generator skill
-- Apply on VPS: mysql -u crossworlds -p crossworlds < seed_arena_content_2026-07-06.sql
--
-- Adds:
--   8 enemy_templates (grunt×2, ranged×2, elite×2, void_emitter, shielder)
--   Loot source entries in loot_tables for each new enemy
--   8 new items (4 materials, 4 consumables)
--
-- Safe to re-run: all INSERTs use INSERT IGNORE to skip existing IDs.
-- ============================================================

-- ── New Items ────────────────────────────────────────────────────────────────
-- ID convention: type_adjective_noun (matches existing: sword_copper, plate_copper)
-- stat_bonus JSON matches CharacterStats keys: str, agi, int, vit, max_hp, dmg, armor

INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value) VALUES

-- Materials (stackable, used for crafting / future upgrade system)
('material_void_shard',
 'Void Shard',
 'uncommon',
 'material',
 '{}',
 8),

('material_iron_fragment',
 'Iron Fragment',
 'common',
 'material',
 '{}',
 4),

('material_phase_crystal',
 'Phase Crystal',
 'rare',
 'material',
 '{}',
 25),

('material_soul_ember',
 'Soul Ember',
 'uncommon',
 'material',
 '{}',
 12),

-- Consumables (one-use, stackable)
-- heal_amount is in the Unity ItemData SO; sell_value here, effect driven by ConsumableEffect.cs
('consumable_health_potion_small',
 'Minor Healing Draught',
 'common',
 'consumable',
 '{"heal_amount": 30}',
 6),

('consumable_void_ward_scroll',
 'Void Ward Scroll',
 'uncommon',
 'consumable',
 '{"resist_void": 0.20, "duration": 8}',
 18),

('consumable_iron_will_tonic',
 'Iron Will Tonic',
 'uncommon',
 'consumable',
 '{"armor": 5, "duration": 12}',
 15),

('consumable_phase_salve',
 'Phase Salve',
 'rare',
 'consumable',
 '{"speed": 0.25, "duration": 6}',
 30);


-- ── Enemy Templates ──────────────────────────────────────────────────────────
-- Schema: id, display_name, max_hp, damage_min, damage_max,
--         move_speed, aggro_range, xp_reward, gold_reward_min, gold_reward_max,
--         loot_source_id
--
-- Tier 1 (waves 1–4): baseline stats, low XP
-- Tier 2 (waves 5–10): scaled up ~40% HP, ~25% damage, higher XP
-- Calibrated against COMBAT_DESIGN_2026-07-06.md TTK/TTD tables:
--   Player HP: 100 (base), DPS class ~20 DPS at tier 1 gear
--   TTK trash tier 1: 60HP / 20DPS = 3s ✓
--   TTD vs single grunt: 60HP / (12/1.5 DPS) = 7.5s ✓ (safe floor)

INSERT IGNORE INTO enemy_templates
  (id, display_name, max_hp, damage_min, damage_max,
   move_speed, aggro_range, xp_reward,
   gold_reward_min, gold_reward_max, loot_source_id)
VALUES

-- ── Grunt (melee pressure, wave filler) ──────────────────────────────────────
('grunt_basic',
 'Void Grunt',
 60, 10, 14,
 3.5, 8.0, 10,
 1, 5, 'grunt_basic'),

('grunt_veteran',
 'Veteran Void Grunt',
 100, 14, 18,
 4.0, 9.0, 18,
 2, 8, 'grunt_veteran'),

-- ── Ranged (positional threat, keep-away pattern) ────────────────────────────
-- Lower HP than grunt (paper; punished by reaching melee range)
-- Higher aggro radius so it notices players first and opens with shots
('ranged_basic',
 'Void Rifler',
 45, 8, 12,
 3.0, 10.0, 12,
 1, 4, 'ranged_basic'),

('ranged_veteran',
 'Veteran Void Rifler',
 75, 12, 16,
 3.5, 11.0, 22,
 2, 7, 'ranged_veteran'),

-- ── Elite (wave anchor — one per wave-3 cycle, boss-lite) ────────────────────
-- TTK elite_basic at tier 1 gear: 300HP / 20DPS = 15s → matches COMBAT_DESIGN target
-- Elite has highest gold/XP; gives teams a priority target to focus
('elite_basic',
 'Void Warlord',
 300, 18, 28,
 3.0, 12.0, 50,
 10, 25, 'elite_basic'),

('elite_veteran',
 'Corrupted Void Warlord',
 550, 25, 40,
 3.5, 14.0, 90,
 20, 50, 'elite_veteran'),

-- ── Void Emitter (NEW — stationary area denial, DoT seeder) ─────────────────
-- No move_speed (stationary); pulses Weakened in 6u radius every 4s.
-- Low HP (glass cannon) — teams must prioritize destroying it.
-- Decay hook: each pulse seeds Void Rot stack (pairs with COMBAT_PROPOSAL transitions).
-- Class answer: Arcanist Overcharge burst kills in one rotation;
--               Guardian Taunt keeps grunts from blocking sight.
('void_emitter_basic',
 'Void Emitter',
 40, 5, 8,
 0.0, 0.0, 15,
 3, 8, 'void_emitter_basic'),

-- ── Shielder (NEW — positional puzzle, front-shield) ────────────────────────
-- Carries a front shield: 80% damage reduction from front.
-- Slower than grunt (shield weight). Must be flanked or knocked back.
-- Class answer: Shadowblade backstab bypasses shield;
--               Guardian Ground Slam hits from below (AoE).
-- Decay hook: shield durability decays under Void Rot ticks.
('shielder_basic',
 'Void Shieldbearer',
 80, 12, 16,
 2.5, 8.0, 14,
 2, 6, 'shielder_basic');


-- ── Loot Tables ──────────────────────────────────────────────────────────────
-- Schema: source_name, new_item_id, drop_chance (0–1 float), min_qty, max_qty
-- source_name links to enemy_templates.loot_source_id
-- Multiple rows per source_name = multiple possible drops (rolled independently or as pool)
-- nothingWeight equivalent: low-weight rows + high nothing implied by drop_chance

INSERT IGNORE INTO loot_tables (source_name, new_item_id, drop_chance, min_qty, max_qty) VALUES

-- Grunt Basic drops
('grunt_basic', 'material_copper_shard',  0.30, 1, 2),
('grunt_basic', 'material_iron_fragment', 0.08, 1, 1),
('grunt_basic', 'consumable_health_potion_small', 0.05, 1, 1),

-- Grunt Veteran drops (more materials, slight upgrade mat chance)
('grunt_veteran', 'material_copper_shard',   0.25, 1, 2),
('grunt_veteran', 'material_iron_fragment',  0.15, 1, 2),
('grunt_veteran', 'material_soul_ember',     0.06, 1, 1),

-- Ranged Basic drops (copper_shard only — paper enemy, minimal drops)
('ranged_basic', 'material_copper_shard',  0.25, 1, 2),

-- Ranged Veteran drops
('ranged_veteran', 'material_copper_shard',   0.20, 1, 2),
('ranged_veteran', 'material_iron_fragment',  0.10, 1, 1),
('ranged_veteran', 'material_void_shard',     0.05, 1, 1),

-- Elite Basic drops (COMBAT.md baseline: 20% nothing, 40% copper_bar, 30% copper_shard, 10% gear)
('elite_basic', 'material_copper_bar',      0.40, 1, 2),
('elite_basic', 'material_copper_shard',    0.30, 2, 4),
('elite_basic', 'material_phase_crystal',   0.10, 1, 1),
('elite_basic', 'consumable_void_ward_scroll', 0.08, 1, 1),

-- Elite Veteran drops (higher tier materials, rare gear chance)
('elite_veteran', 'material_copper_bar',       0.35, 2, 3),
('elite_veteran', 'material_phase_crystal',    0.20, 1, 2),
('elite_veteran', 'material_void_shard',       0.15, 1, 2),
('elite_veteran', 'material_soul_ember',       0.10, 1, 2),
('elite_veteran', 'consumable_iron_will_tonic',0.08, 1, 1),
('elite_veteran', 'consumable_phase_salve',    0.05, 1, 1),

-- Void Emitter drops (priority kill → reward with void materials)
('void_emitter_basic', 'material_void_shard',       0.60, 1, 2),
('void_emitter_basic', 'material_soul_ember',        0.20, 1, 1),
('void_emitter_basic', 'consumable_void_ward_scroll',0.10, 1, 1),

-- Shielder drops (iron-themed)
('shielder_basic', 'material_iron_fragment', 0.40, 1, 2),
('shielder_basic', 'material_copper_bar',    0.15, 1, 1),
('shielder_basic', 'consumable_iron_will_tonic', 0.08, 1, 1);


-- ── Verification query (run after to confirm) ────────────────────────────────
-- SELECT id, display_name, max_hp, xp_reward FROM enemy_templates WHERE id LIKE 'grunt%' OR id LIKE 'elite%' OR id LIKE 'ranged%' OR id LIKE 'void_%' OR id LIKE 'shielder%';
-- SELECT source_name, new_item_id, drop_chance FROM loot_tables WHERE source_name IN ('grunt_basic','grunt_veteran','ranged_basic','ranged_veteran','elite_basic','elite_veteran','void_emitter_basic','shielder_basic');
-- SELECT id, name, rarity FROM items WHERE id LIKE 'material_void%' OR id LIKE 'material_iron%' OR id LIKE 'material_phase%' OR id LIKE 'material_soul%' OR id LIKE 'consumable_health%' OR id LIKE 'consumable_void%' OR id LIKE 'consumable_iron%' OR id LIKE 'consumable_phase_salve%';
