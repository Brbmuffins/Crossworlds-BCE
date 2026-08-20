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

-- Repair invalid enum-zero rows to supported temporary values first, otherwise
-- strict MariaDB can reject the enum alteration as truncated data.
UPDATE items SET item_type = 'weapon' WHERE id IN ('bow_oak', 'staff_oak');
UPDATE items SET item_type = 'trinket' WHERE id IN ('augment_copper', 'augment_iron');
UPDATE items SET item_type = 'material' WHERE id IN (
  'flask_hp_minor', 'flask_mp_minor', 'flask_hp_major', 'flask_void_resist',
  'kit_iron_warden', 'flask_speed', 'flask_damage'
);

-- The live schema predates consumables. Extend it without renaming or removing
-- any existing values used by the legacy gear contract.
ALTER TABLE items MODIFY COLUMN item_type ENUM(
  'weapon', 'armor_head', 'armor_chest', 'armor_legs', 'armor_feet',
  'armor_hands', 'offhand', 'ring', 'trinket', 'material', 'consumable'
) NOT NULL;

-- Repair rows created while their intended enum values did not exist.
UPDATE items SET item_type = 'weapon' WHERE id IN ('bow_oak', 'staff_oak');
UPDATE items SET item_type = 'trinket' WHERE id IN ('augment_copper', 'augment_iron');
UPDATE items SET item_type = 'consumable' WHERE id IN (
  'flask_hp_minor', 'flask_mp_minor', 'flask_hp_major', 'flask_void_resist',
  'kit_iron_warden', 'flask_speed', 'flask_damage'
);

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
   'common', 'weapon',
   '{"agi":8}',
   45),

  ('staff_oak',
   'Oak Staff',
   'common', 'weapon',
   '{"int":8}',
   45),

  ('augment_copper',
   'Copper Augment',
   'common', 'trinket',
   '{"stat_bonus_pct":10}',
   30),

  ('augment_iron',
   'Iron Augment',
   'uncommon', 'trinket',
   '{"stat_bonus_pct":18}',
   75);

-- ── 6–7. Recipes ─────────────────────────────────────────────────────────────
-- Recipe IDs and profession IDs are explicit stable strings because the live
-- schema uses VARCHAR keys. Never use LAST_INSERT_ID() for this table.

INSERT INTO recipes
  (id, name, profession_id, skill_level_required, result_item_id, result_quantity, recipe_type, craft_time_seconds)
VALUES
  ('smelt_copper_ingot', 'Smelt Copper Ingot', 'mining', 1, 'ingot_copper', 1, 'smelt', 2.0),
  ('smelt_iron_ingot', 'Smelt Iron Ingot', 'mining', 5, 'ingot_iron', 1, 'smelt', 2.0),
  ('smelt_gold_ingot', 'Smelt Gold Ingot', 'mining', 15, 'ingot_gold', 1, 'smelt', 2.0),
  ('mill_oak_plank', 'Mill Oak Plank', 'woodcutting', 1, 'plank_oak', 1, 'smelt', 2.0),
  ('prepare_river_fillet', 'Prepare River Fillet', 'fishing', 1, 'fish_fillet', 1, 'smelt', 2.0),
  ('craft_minor_healing_potion', 'Minor Healing Potion', 'fishing', 1, 'flask_hp_minor', 1, 'craft', 3.0),
  ('craft_swiftness_flask', 'Swiftness Flask', 'fishing', 3, 'flask_speed', 1, 'craft', 3.0),
  ('craft_void_resist_flask', 'Void Resist Flask', 'fishing', 5, 'flask_void_resist', 1, 'craft', 4.0),
  ('craft_major_healing_flask', 'Major Healing Flask', 'fishing', 8, 'flask_hp_major', 1, 'craft', 4.0),
  ('craft_copper_helm', 'Copper Helm', 'mining', 3, 'helm_copper', 1, 'craft', 5.0),
  ('craft_copper_augment', 'Copper Augment', 'mining', 5, 'augment_copper', 1, 'craft', 3.0),
  ('craft_oak_shortbow', 'Oak Shortbow', 'woodcutting', 3, 'bow_oak', 1, 'craft', 5.0),
  ('craft_oak_staff', 'Oak Staff', 'woodcutting', 3, 'staff_oak', 1, 'craft', 5.0),
  ('craft_iron_warden_kit', 'Iron Warden Blast Kit', 'mining', 12, 'kit_iron_warden', 1, 'craft', 6.0),
  ('craft_iron_helm', 'Iron Helm', 'mining', 8, 'helm_iron', 1, 'craft', 5.0),
  ('craft_iron_augment', 'Iron Augment', 'mining', 12, 'augment_iron', 1, 'craft', 3.0),
  ('craft_iron_chestplate', 'Iron Chestplate', 'mining', 10, 'chest_iron', 1, 'craft', 8.0),
  ('craft_forge_tempered_flask', 'Forge-Tempered Flask', 'mining', 10, 'flask_damage', 1, 'craft', 5.0),
  ('craft_gold_tempered_helm', 'Gold-Tempered Helm', 'mining', 18, 'helm_gold', 1, 'craft', 10.0)
ON DUPLICATE KEY UPDATE
  name = VALUES(name), profession_id = VALUES(profession_id),
  skill_level_required = VALUES(skill_level_required), result_item_id = VALUES(result_item_id),
  result_quantity = VALUES(result_quantity), recipe_type = VALUES(recipe_type),
  craft_time_seconds = VALUES(craft_time_seconds);

INSERT INTO recipe_ingredients (recipe_id, item_id, quantity) VALUES
  ('smelt_copper_ingot', 'ore_copper', 3),
  ('smelt_iron_ingot', 'ore_iron', 3),
  ('smelt_gold_ingot', 'ore_gold', 3),
  ('mill_oak_plank', 'log_oak', 3),
  ('prepare_river_fillet', 'fish_river', 2),
  ('craft_minor_healing_potion', 'fish_fillet', 2),
  ('craft_swiftness_flask', 'fish_fillet', 2),
  ('craft_swiftness_flask', 'ingot_copper', 1),
  ('craft_void_resist_flask', 'fish_fillet', 3),
  ('craft_void_resist_flask', 'ingot_copper', 2),
  ('craft_major_healing_flask', 'fish_fillet', 4),
  ('craft_major_healing_flask', 'ingot_iron', 1),
  ('craft_copper_helm', 'ingot_copper', 2),
  ('craft_copper_augment', 'ingot_copper', 1),
  ('craft_oak_shortbow', 'plank_oak', 3),
  ('craft_oak_staff', 'plank_oak', 3),
  ('craft_iron_warden_kit', 'ingot_iron', 3),
  ('craft_iron_warden_kit', 'plank_oak', 2),
  ('craft_iron_helm', 'ingot_iron', 2),
  ('craft_iron_augment', 'ingot_iron', 1),
  ('craft_iron_chestplate', 'ingot_iron', 3),
  ('craft_iron_chestplate', 'ingot_copper', 1),
  ('craft_forge_tempered_flask', 'ingot_iron', 2),
  ('craft_forge_tempered_flask', 'fish_fillet', 3),
  ('craft_gold_tempered_helm', 'ingot_gold', 2),
  ('craft_gold_tempered_helm', 'ingot_iron', 1)
ON DUPLICATE KEY UPDATE quantity = VALUES(quantity);

-- Convert any rows written by the retired numeric profession contract. A
-- temporary table avoids ambiguous target/source references in MariaDB.
DROP TEMPORARY TABLE IF EXISTS profession_id_migration;
CREATE TEMPORARY TABLE profession_id_migration (
  character_id BIGINT UNSIGNED NOT NULL,
  profession_id VARCHAR(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  skill_level INT NOT NULL,
  skill_xp INT NOT NULL
);
INSERT INTO profession_id_migration (character_id, profession_id, skill_level, skill_xp)
SELECT character_id,
       CASE profession_id
         WHEN '0' THEN 'woodcutting'
         WHEN '1' THEN 'fishing'
         WHEN '2' THEN 'mining'
       END AS profession_id,
       skill_level,
       skill_xp
FROM professions
WHERE profession_id IN ('0', '1', '2');

UPDATE professions AS existing
JOIN profession_id_migration AS migrated
  ON migrated.character_id = existing.character_id
 AND migrated.profession_id = existing.profession_id
SET existing.skill_level = GREATEST(existing.skill_level, migrated.skill_level),
    existing.skill_xp = GREATEST(existing.skill_xp, migrated.skill_xp);

DELETE FROM professions WHERE profession_id IN ('0', '1', '2');

INSERT IGNORE INTO professions (character_id, profession_id, skill_level, skill_xp)
SELECT character_id, profession_id, skill_level, skill_xp
FROM profession_id_migration;

DROP TEMPORARY TABLE profession_id_migration;

-- Remove the malformed row created by the previous numeric/LAST_INSERT_ID patch.
DELETE FROM recipe_ingredients WHERE recipe_id = '';
DELETE FROM recipes WHERE id = '';

-- ── Done ────────────────────────────────────────────────────────────────────
SELECT 'Professions patch complete.' AS status;
SELECT COUNT(*) AS total_items    FROM items;
SELECT COUNT(*) AS total_recipes  FROM recipes;
SELECT COUNT(*) AS total_ingredients FROM recipe_ingredients;
