-- Crossworlds BCE: additional mineable ores, ingots, and smelting recipes.
-- Idempotent: safe to re-run after a server restore or database rebuild.

START TRANSACTION;

INSERT INTO items
  (id, name, rarity, item_type, equipment_slot, two_handed, stat_bonus,
   icon_id, sell_value, crafted, stackable, max_stack_size)
VALUES
  ('ore_bronze',    'Bronze Ore',    'common',    'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '38f4f9a6aa0b4e9cb1ab6b63a7ea2592', 4,   0, 1, 99),
  ('ingot_bronze',  'Bronze Ingot',  'uncommon',  'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '3573dee8ba3c46099a7ac505193a8d58', 16,  1, 1, 99),
  ('ore_tin',       'Tin Ore',       'common',    'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '5e35c6feb1034a3eb3cd105c9c56bf1e', 3,   0, 1, 99),
  ('ingot_tin',     'Tin Ingot',     'common',    'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '1014f32b8baa4bf2a8652e91be9c9317', 12,  1, 1, 99),
  ('ore_steel',     'Steel Ore',     'uncommon',  'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '1ab2b6b108744459aa8263ddf76cd1c2', 18,  0, 1, 99),
  ('ingot_steel',   'Steel Ingot',   'rare',      'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '7fdaf2e6b86a4717b81cd963a0a65ba2', 72,  1, 1, 99),
  ('ore_silver',    'Silver Ore',    'uncommon',  'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '9e2c00dbebe6490899d0c481c1f0bb94', 12,  0, 1, 99),
  ('ingot_silver',  'Silver Ingot',  'rare',      'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), 'c0f858ffb70e42e597c2c205945f3e90', 48,  1, 1, 99),
  ('ore_hell',      'Hell Ore',      'epic',      'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '3ea93378e5f6407baaaad627672c5ad1', 50,  0, 1, 99),
  ('ingot_hell',    'Hell Ingot',    'legendary', 'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), 'd9ca89468d874972a45f349707d16040', 200, 1, 1, 99),
  ('ore_moon',      'Moon Ore',      'legendary', 'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), '10db5f5169d246219619a6e1128503df', 75,  0, 1, 99),
  ('ingot_moon',    'Moon Ingot',    'relic',     'material', NULL, 0, JSON_OBJECT('stat_agi',0,'stat_int',0,'stat_str',0,'stat_vit',0), 'f5fa71a941ae41279dedd5aa2f468b37', 300, 1, 1, 99)
ON DUPLICATE KEY UPDATE
  name = VALUES(name),
  rarity = VALUES(rarity),
  item_type = VALUES(item_type),
  equipment_slot = VALUES(equipment_slot),
  two_handed = VALUES(two_handed),
  stat_bonus = VALUES(stat_bonus),
  icon_id = VALUES(icon_id),
  sell_value = VALUES(sell_value),
  crafted = VALUES(crafted),
  stackable = VALUES(stackable),
  max_stack_size = VALUES(max_stack_size);

INSERT INTO recipes
  (id, name, profession_id, skill_level_required, result_item_id,
   recipe_type, craft_time_seconds, result_quantity)
VALUES
  ('smelt_tin_ingot',    'Smelt Tin Ingot',    'mining', 3,  'ingot_tin',    'smelt', 2, 1),
  ('smelt_bronze_ingot', 'Smelt Bronze Ingot', 'mining', 8,  'ingot_bronze', 'smelt', 2, 1),
  ('smelt_silver_ingot', 'Smelt Silver Ingot', 'mining', 12, 'ingot_silver', 'smelt', 2, 1),
  ('smelt_steel_ingot',  'Smelt Steel Ingot',  'mining', 20, 'ingot_steel',  'smelt', 2, 1),
  ('smelt_hell_ingot',   'Smelt Hell Ingot',   'mining', 35, 'ingot_hell',   'smelt', 2, 1),
  ('smelt_moon_ingot',   'Smelt Moon Ingot',   'mining', 50, 'ingot_moon',   'smelt', 2, 1)
ON DUPLICATE KEY UPDATE
  name = VALUES(name),
  profession_id = VALUES(profession_id),
  skill_level_required = VALUES(skill_level_required),
  result_item_id = VALUES(result_item_id),
  recipe_type = VALUES(recipe_type),
  craft_time_seconds = VALUES(craft_time_seconds),
  result_quantity = VALUES(result_quantity);

INSERT INTO recipe_ingredients (recipe_id, item_id, quantity)
VALUES
  ('smelt_tin_ingot',    'ore_tin',    3),
  ('smelt_bronze_ingot', 'ore_bronze', 3),
  ('smelt_silver_ingot', 'ore_silver', 3),
  ('smelt_steel_ingot',  'ore_steel',  3),
  ('smelt_hell_ingot',   'ore_hell',   3),
  ('smelt_moon_ingot',   'ore_moon',   3)
ON DUPLICATE KEY UPDATE quantity = VALUES(quantity);

COMMIT;
