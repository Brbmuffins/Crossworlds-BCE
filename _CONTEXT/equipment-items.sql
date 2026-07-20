-- ============================================================================
-- Equipment system — VPS DB migration (run on rod_online, MySQL 8)
-- ============================================================================
-- Touches ONLY the new `items` + `inventory` tables. Does NOT modify the sacred
-- old gear tables (item_template, item_instance, character_gear).
--
-- Apply on the VPS:
--   ssh ubuntu@playcrossworlds.com
--   mysql -u rodgame -p"$(grep DB_PASS /opt/rod-auth/.env | cut -d= -f2)" rod_online < equipment-items.sql
--   -- verify:  SELECT id,item_type FROM items WHERE id LIKE '%_base' OR id LIKE '%_sneaker' ORDER BY id;
--
-- Contents: 2 sneaker upgrade items + a base loadout for every class
--           (5 classes × 6 slots = 30 base items). item ids match
--           ItemData.serverItemId in Unity (EquipmentSetupBuilder).
-- ----------------------------------------------------------------------------

-- 1) Extend item_type so the 6 equip slots are distinguishable.
--    (equipped-slot identity is derived from item_type: one equipped item per type.)
ALTER TABLE items
  MODIFY item_type ENUM(
    'weapon','armor_head','armor_chest','armor_legs',
    'armor_feet','armor_hands','offhand',
    'ring','trinket','material'
  ) NOT NULL;

-- 2) Seed items. stat_bonus JSON mirrors ItemData.baseModifiers.
--    Keys: damage_pct, dr_pct, move_pct, cdr_pct, heal_pct, max_health.
INSERT INTO items (id, name, rarity, item_type, stat_bonus, icon_id, sell_value, crafted) VALUES
  -- ── Sneaker upgrade set (has world models) ────────────────────────────────
  ('sword_sneaker',        'Sneaker Blade',       'uncommon', 'weapon',      '{"damage_pct":0.08}',              'sword_sneaker',        25, 0),
  ('shield_sneaker',       'Sneaker Bulwark',     'uncommon', 'offhand',     '{"dr_pct":0.06,"max_health":20}',  'shield_sneaker',       25, 0),

  -- ── Warden base loadout ───────────────────────────────────────────────────
  ('warden_head_base',     'Warden Helm',         'common',   'armor_head',  '{"max_health":10}',                'warden_head_base',      5, 0),
  ('warden_chest_base',    'Warden Chestguard',   'common',   'armor_chest', '{"max_health":20}',                'warden_chest_base',     8, 0),
  ('warden_feet_base',     'Warden Boots',        'common',   'armor_feet',  '{"move_pct":0.04}',                'warden_feet_base',      5, 0),
  ('warden_hands_base',    'Warden Gloves',       'common',   'armor_hands', '{"cdr_pct":0.03}',                 'warden_hands_base',     5, 0),
  ('warden_mainhand_base', 'Warden Carbine',      'common',   'weapon',      '{"damage_pct":0.05}',              'warden_mainhand_base',  8, 0),
  ('warden_offhand_base',  'Warden Deflector',    'common',   'offhand',     '{"dr_pct":0.03}',                  'warden_offhand_base',   6, 0),

  -- ── Ironclad base loadout ─────────────────────────────────────────────────
  ('ironclad_head_base',     'Ironclad Helm',        'common', 'armor_head',  '{"max_health":10}',               'ironclad_head_base',      5, 0),
  ('ironclad_chest_base',    'Ironclad Chestguard',  'common', 'armor_chest', '{"max_health":20}',               'ironclad_chest_base',     8, 0),
  ('ironclad_feet_base',     'Ironclad Boots',       'common', 'armor_feet',  '{"move_pct":0.04}',               'ironclad_feet_base',      5, 0),
  ('ironclad_hands_base',    'Ironclad Gloves',      'common', 'armor_hands', '{"cdr_pct":0.03}',                'ironclad_hands_base',     5, 0),
  ('ironclad_mainhand_base', 'Ironclad Blade',       'common', 'weapon',      '{"damage_pct":0.05}',             'ironclad_mainhand_base',  8, 0),
  ('ironclad_offhand_base',  'Ironclad Bulwark',     'common', 'offhand',     '{"dr_pct":0.05,"max_health":15}', 'ironclad_offhand_base',   6, 0),

  -- ── Shadowblade base loadout ──────────────────────────────────────────────
  ('shadowblade_head_base',     'Shadowblade Helm',        'common', 'armor_head',  '{"max_health":10}',        'shadowblade_head_base',      5, 0),
  ('shadowblade_chest_base',    'Shadowblade Chestguard',  'common', 'armor_chest', '{"max_health":20}',        'shadowblade_chest_base',     8, 0),
  ('shadowblade_feet_base',     'Shadowblade Boots',       'common', 'armor_feet',  '{"move_pct":0.04}',        'shadowblade_feet_base',      5, 0),
  ('shadowblade_hands_base',    'Shadowblade Gloves',      'common', 'armor_hands', '{"cdr_pct":0.03}',         'shadowblade_hands_base',     5, 0),
  ('shadowblade_mainhand_base', 'Shadowblade Dagger',      'common', 'weapon',      '{"damage_pct":0.05}',      'shadowblade_mainhand_base',  8, 0),
  ('shadowblade_offhand_base',  'Shadowblade Parrying Dagger', 'common', 'offhand', '{"damage_pct":0.03}',      'shadowblade_offhand_base',   6, 0),

  -- ── Cleric base loadout ───────────────────────────────────────────────────
  ('cleric_head_base',     'Cleric Helm',         'common', 'armor_head',  '{"max_health":10}',                'cleric_head_base',      5, 0),
  ('cleric_chest_base',    'Cleric Chestguard',   'common', 'armor_chest', '{"max_health":20}',                'cleric_chest_base',     8, 0),
  ('cleric_feet_base',     'Cleric Boots',        'common', 'armor_feet',  '{"move_pct":0.04}',                'cleric_feet_base',      5, 0),
  ('cleric_hands_base',    'Cleric Gloves',       'common', 'armor_hands', '{"cdr_pct":0.03}',                 'cleric_hands_base',     5, 0),
  ('cleric_mainhand_base', 'Cleric Mace',         'common', 'weapon',      '{"heal_pct":0.05}',                'cleric_mainhand_base',  8, 0),
  ('cleric_offhand_base',  'Cleric Tome',         'common', 'offhand',     '{"heal_pct":0.04}',                'cleric_offhand_base',   6, 0),

  -- ── Arcanist base loadout ─────────────────────────────────────────────────
  ('arcanist_head_base',     'Arcanist Helm',        'common', 'armor_head',  '{"max_health":10}',             'arcanist_head_base',      5, 0),
  ('arcanist_chest_base',    'Arcanist Chestguard',  'common', 'armor_chest', '{"max_health":20}',             'arcanist_chest_base',     8, 0),
  ('arcanist_feet_base',     'Arcanist Boots',       'common', 'armor_feet',  '{"move_pct":0.04}',             'arcanist_feet_base',      5, 0),
  ('arcanist_hands_base',    'Arcanist Gloves',      'common', 'armor_hands', '{"cdr_pct":0.03}',              'arcanist_hands_base',     5, 0),
  ('arcanist_mainhand_base', 'Arcanist Staff',       'common', 'weapon',      '{"damage_pct":0.05}',           'arcanist_mainhand_base',  8, 0),
  ('arcanist_offhand_base',  'Arcanist Focus',       'common', 'offhand',     '{"cdr_pct":0.04}',              'arcanist_offhand_base',   6, 0)
ON DUPLICATE KEY UPDATE
  name = VALUES(name), rarity = VALUES(rarity), item_type = VALUES(item_type),
  stat_bonus = VALUES(stat_bonus), icon_id = VALUES(icon_id), sell_value = VALUES(sell_value);

-- ============================================================================
-- 3) RECOMMENDED server.js change (apply in /opt/rod-auth/server.js, then
--    `sudo systemctl restart crossworlds-auth`). Optional but prevents two items
--    of the same slot both being flagged equipped=1 (double stat application).
--
--    In POST /api/inventory/equip, when equipping (equipped=1), first clear the
--    equipped flag on any OTHER inventory row whose item shares the same item_type:
--
--      if (equipped === 1) {
--        await conn.query(
--          `UPDATE inventory inv
--             JOIN items i ON i.id = inv.item_id
--             SET inv.equipped = 0
--           WHERE inv.character_id = ? AND i.item_type = ? AND inv.slot_index <> ?`,
--          [characterId, itemType, slot_index]
--        );
--      }
--
--    (Wrap the two writes in a transaction — see conventions.md.)
-- ============================================================================
