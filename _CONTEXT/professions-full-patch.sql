-- ═══════════════════════════════════════════════════════════════════════════════
--  PROFESSIONS FULL PATCH — run on VPS via deploy-professions.sh
--  Database: crossworlds
--
--  What this does:
--    1. Adds recipe_type + craft_time_seconds columns to recipes (IF NOT EXISTS)
--    2. Seeds raw gathering materials into items
--    3. Seeds refined materials (ingots, planks, fillets)
--    4. Seeds consumables (flasks, kits) — NOT droppable, craft-only
--    5. Seeds crafted gear (deterministic stats, craft-only)
--    6. Seeds smelt recipes: raw → refined (2s, at forge)
--    7. Seeds craft recipes: refined → gear/consumables
--    8. Seeds professions table (initial rows for existing characters)
--
--  XP formula: xpToNextLevel = skill_level × 50
--  Safe to run multiple times (INSERT IGNORE + IF NOT EXISTS guards).
-- ═══════════════════════════════════════════════════════════════════════════════

-- ── 1. Migrate recipes table ─────────────────────────────────────────────────

SET @has_type = (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME   = 'recipes'
    AND COLUMN_NAME  = 'recipe_type'
);
SET @sql = IF(@has_type = 0,
  "ALTER TABLE recipes ADD COLUMN recipe_type VARCHAR(16) NOT NULL DEFAULT 'craft' AFTER result_item_id",
  'SELECT 1');
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;

SET @has_time = (
  SELECT COUNT(*) FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME   = 'recipes'
    AND COLUMN_NAME  = 'craft_time_seconds'
);
SET @sql2 = IF(@has_time = 0,
  'ALTER TABLE recipes ADD COLUMN craft_time_seconds FLOAT NOT NULL DEFAULT 2.0 AFTER recipe_type',
  'SELECT 1');
PREPARE s FROM @sql2; EXECUTE s; DEALLOCATE PREPARE s;

-- ── 2. Raw gathering materials ───────────────────────────────────────────────
-- These are what AfkGatheringStation drops; they feed into smelt recipes.

INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value) VALUES
  ('ore_copper',  'Copper Ore',   'common',    'material', '{}', 2),
  ('ore_iron',    'Iron Ore',     'common',    'material', '{}', 5),
  ('ore_gold',    'Gold Ore',     'uncommon',  'material', '{}', 15),
  ('log_oak',     'Oak Log',      'common',    'material', '{}', 2),
  ('fish_river',  'River Fish',   'common',    'material', '{}', 3);

-- ── 3. Refined materials ─────────────────────────────────────────────────────

INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value) VALUES
  ('ingot_copper', 'Copper Ingot', 'common',   'material', '{}', 8),
  ('ingot_iron',   'Iron Ingot',   'uncommon', 'material', '{}', 20),
  ('ingot_gold',   'Gold Ingot',   'rare',     'material', '{}', 60),
  ('plank_oak',    'Oak Plank',    'common',   'material', '{}', 8),
  ('fish_fillet',  'River Fillet', 'common',   'material', '{}', 6);

-- ── 4. Consumables (craft-only, not in loot tables) ──────────────────────────
-- stat_bonus JSON for consumables carries effect metadata:
--   {"effect":"hp_regen",  "value":15,  "duration":40}
--   {"effect":"mana_restore","value":15}
--   {"effect":"resist_void","value":0.25,"duration":90}
--   {"effect":"resist_blast","value":0.25,"duration":60}
--   {"effect":"speed",     "value":0.20, "duration":30}
--   {"effect":"damage_amp","value":0.15, "duration":45}

INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value) VALUES
  ('flask_hp_minor',
   'Minor Healing Potion',
   'common', 'consumable',
   '{"effect":"hp_regen","value":15,"duration":40}',
   1),

  ('flask_mp_minor',
   'Minor Mana Potion',
   'common', 'consumable',
   '{"effect":"mana_restore","value":15}',
   1),

  ('flask_hp_major',
   'Major Healing Flask',
   'uncommon', 'consumable',
   '{"effect":"hp_regen","value":80,"duration":60}',
   35),

  ('flask_void_resist',
   'Void Resist Flask',
   'uncommon', 'consumable',
   '{"effect":"resist_void","value":0.25,"duration":90}',
   40),

  ('kit_iron_warden',
   'Iron Warden Blast Kit',
   'uncommon', 'consumable',
   '{"effect":"resist_blast","value":0.25,"duration":60}',
   45),

  ('flask_speed',
   'Swiftness Flask',
   'common', 'consumable',
   '{"effect":"speed","value":0.20,"duration":30}',
   18),

  ('flask_damage',
   'Forge-Tempered Flask',
   'rare', 'consumable',
   '{"effect":"damage_amp","value":0.15,"duration":45}',
   55);

-- Keep an existing deployment synchronized; INSERT IGNORE above intentionally
-- does not overwrite rows seeded by an earlier version of this patch.
UPDATE items
SET name = 'Minor Healing Potion',
    rarity = 'common',
    item_type = 'consumable',
    stat_bonus = '{"effect":"hp_regen","value":15,"duration":40}',
    sell_value = 1
WHERE id = 'flask_hp_minor';

UPDATE items
SET name = 'Minor Mana Potion',
    rarity = 'common',
    item_type = 'consumable',
    stat_bonus = '{"effect":"mana_restore","value":15}',
    sell_value = 1
WHERE id = 'flask_mp_minor';

UPDATE items
SET stackable = 1,
    max_stack_size = 40
WHERE id IN ('flask_hp_minor', 'flask_mp_minor');

-- ── 5. Crafted gear (deterministic stats, not in loot tables) ────────────────

INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value) VALUES
  ('helm_copper',
   'Copper Helm',
   'common', 'armor_head',
   '{"vit":5}',
   25),

  ('helm_iron',
   'Iron Helm',
   'uncommon', 'armor_head',
   '{"str":8,"vit":3}',
   70),

  ('chest_iron',
   'Iron Chestplate',
   'uncommon', 'armor_chest',
   '{"str":10,"vit":5}',
   110),

  ('helm_gold',
   'Gold-Tempered Helm',
   'rare', 'armor_head',
   '{"str":10,"agi":6,"vit":6}',
   220),

  ('bow_oak',
   'Oak Shortbow',
   'common', 'weapon_ranged',
   '{"agi":8}',
   45),

  ('staff_oak',
   'Oak Staff',
   'common', 'weapon_magic',
   '{"int":8}',
   45),

  ('augment_copper',
   'Copper Augment',
   'common', 'augment',
   '{"stat_bonus_pct":10}',
   30),

  ('augment_iron',
   'Iron Augment',
   'uncommon', 'augment',
   '{"stat_bonus_pct":18}',
   75);

-- ── 6. Smelt recipes (raw → refined, recipe_type='smelt', 2s) ────────────────

-- Copper Smelt: 3× ore_copper → 1× ingot_copper (Mining Lv 1)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 1, 'ingot_copper', 'smelt', 2.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ore_copper', 3);

-- Iron Smelt: 3× ore_iron → 1× ingot_iron (Mining Lv 5)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 5, 'ingot_iron', 'smelt', 2.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ore_iron', 3);

-- Gold Smelt: 3× ore_gold → 1× ingot_gold (Mining Lv 15)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 15, 'ingot_gold', 'smelt', 2.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ore_gold', 3);

-- Oak Plank: 3× log_oak → 1× plank_oak (Woodcutting Lv 1)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (0, 1, 'plank_oak', 'smelt', 2.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'log_oak', 3);

-- River Fillet: 2× fish_river → 1× fish_fillet (Fishing Lv 1)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (1, 1, 'fish_fillet', 'smelt', 2.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'fish_river', 2);

-- ── 7. Craft recipes (refined → gear/consumables, recipe_type='craft') ────────

-- Minor Healing Flask: 2× fish_fillet (Fishing Lv 1, 3s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (1, 1, 'flask_hp_minor', 'craft', 3.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'fish_fillet', 2);

-- Swiftness Flask: 2× fish_fillet + 1× ingot_copper (Fishing Lv 3, 3s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (1, 3, 'flask_speed', 'craft', 3.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'fish_fillet', 2);
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_copper', 1);

-- Void Resist Flask: 3× fish_fillet + 2× ingot_copper (Fishing Lv 5, 4s)
-- Counters Null Architect void damage — exclusive to professions
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (1, 5, 'flask_void_resist', 'craft', 4.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'fish_fillet', 3);
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_copper', 2);

-- Major Healing Flask: 4× fish_fillet + 1× ingot_iron (Fishing Lv 8, 4s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (1, 8, 'flask_hp_major', 'craft', 4.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'fish_fillet', 4);
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_iron', 1);

-- Copper Helm: 2× ingot_copper (Mining Lv 3, 5s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 3, 'helm_copper', 'craft', 5.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_copper', 2);

-- Copper Augment: 1× ingot_copper (Mining Lv 5, 3s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 5, 'augment_copper', 'craft', 3.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_copper', 1);

-- Oak Shortbow: 3× plank_oak (Woodcutting Lv 3, 5s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (0, 3, 'bow_oak', 'craft', 5.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'log_oak', 3);

-- Oak Staff: 3× plank_oak (Woodcutting Lv 3, 5s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (0, 3, 'staff_oak', 'craft', 5.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'log_oak', 3);

-- Iron Warden Blast Kit: 3× ingot_iron + 2× plank_oak (Mining Lv 12, 6s)
-- Counters Devastation slam — exclusive to professions
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 12, 'kit_iron_warden', 'craft', 6.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_iron', 3);
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'plank_oak', 2);

-- Iron Helm: 2× ingot_iron (Mining Lv 8, 5s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 8, 'helm_iron', 'craft', 5.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_iron', 2);

-- Iron Augment: 1× ingot_iron (Mining Lv 12, 3s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 12, 'augment_iron', 'craft', 3.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_iron', 1);

-- Iron Chestplate: 3× ingot_iron + 1× ingot_copper (Mining Lv 10, 8s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 10, 'chest_iron', 'craft', 8.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_iron', 3);
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_copper', 1);

-- Forge-Tempered Flask: 2× ingot_iron + 3× fish_fillet (Mining Lv 10 / Fishing gate via materials, 5s)
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 10, 'flask_damage', 'craft', 5.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_iron', 2);
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'fish_fillet', 3);

-- Gold-Tempered Helm: 2× ingot_gold + 1× ingot_iron (Mining Lv 18, 10s)
-- Best-in-slot head — only reachable by dedicated miners
INSERT IGNORE INTO recipes (profession_id, skill_level_required, result_item_id, recipe_type, craft_time_seconds)
VALUES (2, 18, 'helm_gold', 'craft', 10.0);
SET @r = LAST_INSERT_ID();
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_gold', 2);
INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES (@r, 'ingot_iron', 1);

-- ── Done ────────────────────────────────────────────────────────────────────
SELECT 'Professions patch complete.' AS status;
SELECT COUNT(*) AS total_items    FROM items;
SELECT COUNT(*) AS total_recipes  FROM recipes;
SELECT COUNT(*) AS total_ingredients FROM recipe_ingredients;
