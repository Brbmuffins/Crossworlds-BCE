// ═══════════════════════════════════════════════════════════════════════════════
//  SERVER.JS PATCH — Professions endpoints
//  Applied by deploy-professions.sh
//
//  INSERT BOTH BLOCKS near the existing GET /api/professions/:characterId route.
//
//  Block 1: POST /api/professions/award-xp   (new)
//  Block 2: GET  /api/professions/recipes/:characterId  (new — filters by skill)
//  Block 3: POST /api/craft patch — extend existing handler with smelt support
//           (the /api/craft endpoint already exists; this adds recipe_type awareness
//            and craft_time_seconds to its response so the Unity panel can show timers)
//
//  After applying:
//    sudo systemctl restart crossworlds-auth
//    sudo journalctl -u crossworlds-auth -n 10 --no-pager
// ═══════════════════════════════════════════════════════════════════════════════

// ── Block 1: POST /api/professions/award-xp ─────────────────────────────────

const PROFESSION_NAMES = ['Woodcutting', 'Fishing', 'Mining'];

function xpToNextLevel(currentLevel) {
  return Math.max(1, currentLevel) * 50;
}

app.post('/api/professions/award-xp', requireJWT, async (req, res) => {
  const { characterId, professionId, xpAmount } = req.body;

  if (!characterId || professionId === undefined || !xpAmount)
    return res.json({ success: false, error: 'Missing characterId, professionId, or xpAmount' });

  if (typeof professionId !== 'number' || professionId < 0 || professionId > 10)
    return res.json({ success: false, error: 'Invalid professionId' });

  const xp = Math.min(Math.max(parseInt(xpAmount, 10), 1), 500);

  const [chars] = await db.promise().query(
    'SELECT id FROM characters WHERE id = ? AND account_id = ?',
    [characterId, req.user.accountId]
  );
  if (!chars.length)
    return res.json({ success: false, error: 'Character not found or not yours' });

  // Upsert — create row if first time this profession is used
  await db.promise().query(
    `INSERT INTO professions (character_id, profession_id, skill_level, skill_xp)
     VALUES (?, ?, 1, 0)
     ON DUPLICATE KEY UPDATE skill_xp = skill_xp`,
    [characterId, professionId]
  );

  const [[prof]] = await db.promise().query(
    'SELECT skill_level, skill_xp FROM professions WHERE character_id = ? AND profession_id = ?',
    [characterId, professionId]
  );

  let { skill_level, skill_xp } = prof;
  skill_xp += xp;

  let leveled_up = false;
  while (skill_xp >= xpToNextLevel(skill_level)) {
    skill_xp  -= xpToNextLevel(skill_level);
    skill_level++;
    leveled_up = true;
  }

  await db.promise().query(
    `UPDATE professions SET skill_level = ?, skill_xp = ?
     WHERE character_id = ? AND profession_id = ?`,
    [skill_level, skill_xp, characterId, professionId]
  );

  const profName = PROFESSION_NAMES[professionId] ?? `profession_${professionId}`;
  if (leveled_up)
    console.log(`[PROF] char ${characterId} — ${profName} leveled to ${skill_level}`);

  return res.json({
    success: true,
    data: { skill_level, skill_xp, leveled_up, profession_id: professionId }
  });
});

// ── Block 2: GET /api/professions/recipes/:characterId ───────────────────────
// Returns all recipes the character is eligible to use, joined with their
// current skill levels. Unity uses this to populate the Smelt + Craft tabs.
//
// Response: { success, data: { woodcutting: N, fishing: N, mining: N,
//             smelt: [{recipe_id, result_item_id, result_name, skill_level_required,
//                      craft_time_seconds, ingredients:[{item_id,name,quantity}]}, ...],
//             craft: [...same shape...] } }

app.get('/api/professions/recipes/:characterId', requireJWT, async (req, res) => {
  const { characterId } = req.params;

  const [chars] = await db.promise().query(
    'SELECT id FROM characters WHERE id = ? AND account_id = ?',
    [characterId, req.user.accountId]
  );
  if (!chars.length)
    return res.json({ success: false, error: 'Character not found or not yours' });

  // Load all profession levels for this character
  const [profs] = await db.promise().query(
    'SELECT profession_id, skill_level FROM professions WHERE character_id = ?',
    [characterId]
  );
  const levels = { 0: 1, 1: 1, 2: 1 }; // defaults
  for (const p of profs) levels[p.profession_id] = p.skill_level;

  // Load all recipes with ingredients + result item name
  const [rows] = await db.promise().query(`
    SELECT
      r.id            AS recipe_id,
      r.profession_id,
      r.skill_level_required,
      r.result_item_id,
      r.recipe_type,
      r.craft_time_seconds,
      i.name          AS result_name,
      i.rarity        AS result_rarity,
      ri.item_id      AS ing_item_id,
      ri.quantity     AS ing_quantity,
      ii.name         AS ing_name
    FROM recipes r
    JOIN items i  ON i.id = r.result_item_id
    LEFT JOIN recipe_ingredients ri ON ri.recipe_id = r.id
    LEFT JOIN items ii ON ii.id = ri.item_id
    ORDER BY r.recipe_type, r.profession_id, r.skill_level_required
  `);

  // Group into recipe objects
  const recipeMap = {};
  for (const row of rows) {
    if (!recipeMap[row.recipe_id]) {
      recipeMap[row.recipe_id] = {
        recipe_id:            row.recipe_id,
        profession_id:        row.profession_id,
        skill_level_required: row.skill_level_required,
        result_item_id:       row.result_item_id,
        result_name:          row.result_name,
        result_rarity:        row.result_rarity,
        recipe_type:          row.recipe_type,
        craft_time_seconds:   row.craft_time_seconds,
        unlocked:             levels[row.profession_id] >= row.skill_level_required,
        ingredients:          []
      };
    }
    if (row.ing_item_id) {
      recipeMap[row.recipe_id].ingredients.push({
        item_id:  row.ing_item_id,
        name:     row.ing_name,
        quantity: row.ing_quantity
      });
    }
  }

  const all    = Object.values(recipeMap);
  const smelt  = all.filter(r => r.recipe_type === 'smelt');
  const craft  = all.filter(r => r.recipe_type !== 'smelt');

  return res.json({
    success: true,
    data: {
      skill_levels: levels,
      smelt,
      craft
    }
  });
});

// ── Block 3: POST /api/craft ─────────────────────────────────────────────────
// REPLACE the existing /api/craft handler with this version.
// Additions vs original:
//   - Handles recipe_type = 'smelt' identically to 'craft' (same ingredient check, same award)
//   - Returns craft_time_seconds so the client can show a progress bar
//   - Awards profession XP on successful craft (xpAmount = skill_level_required × 5)
//   - Uses a transaction for ingredient deduction + item award

app.post('/api/craft', requireJWT, async (req, res) => {
  const { characterId, recipeId } = req.body;
  if (!characterId || !recipeId)
    return res.json({ success: false, error: 'Missing characterId or recipeId' });

  const char = await ownedCharacter(req, res, characterId);
  if (!char) return;

  // Load recipe + ingredients
  const [[recipe]] = await db.promise().query(
    `SELECT r.*, i.name AS result_name, i.rarity AS result_rarity
     FROM recipes r JOIN items i ON i.id = r.result_item_id
     WHERE r.id = ?`,
    [recipeId]
  );
  if (!recipe)
    return res.json({ success: false, error: 'Recipe not found' });

  // Skill level check
  const [[profRow]] = await db.promise().query(
    'SELECT skill_level FROM professions WHERE character_id = ? AND profession_id = ?',
    [characterId, recipe.profession_id]
  );
  const skillLevel = profRow?.skill_level ?? 1;
  if (skillLevel < recipe.skill_level_required)
    return res.json({
      success: false,
      error: `Requires ${PROFESSION_NAMES[recipe.profession_id] ?? 'profession'} level ${recipe.skill_level_required}`
    });

  const [ingredients] = await db.promise().query(
    'SELECT ri.item_id, ri.quantity, i.name FROM recipe_ingredients ri JOIN items i ON i.id = ri.item_id WHERE ri.recipe_id = ?',
    [recipeId]
  );

  // Load current inventory
  const [invRows] = await db.promise().query(
    'SELECT slot_index, item_id, quantity FROM inventory WHERE character_id = ?',
    [characterId]
  );

  // Check we have all ingredients (aggregate across slots)
  const invMap = {};
  for (const row of invRows) {
    invMap[row.item_id] = invMap[row.item_id] || [];
    invMap[row.item_id].push({ slot: row.slot_index, qty: row.quantity });
  }

  for (const ing of ingredients) {
    const slots  = invMap[ing.item_id] ?? [];
    const total  = slots.reduce((s, r) => s + r.qty, 0);
    if (total < ing.quantity)
      return res.json({ success: false, error: `Not enough ${ing.name} (need ${ing.quantity})` });
  }

  const conn = await db.promise().getConnection();
  try {
    await conn.beginTransaction();

    // Deduct ingredients
    for (const ing of ingredients) {
      let needed = ing.quantity;
      for (const slot of invMap[ing.item_id] ?? []) {
        if (needed <= 0) break;
        const take = Math.min(slot.qty, needed);
        if (slot.qty - take <= 0) {
          await conn.query('DELETE FROM inventory WHERE character_id = ? AND slot_index = ?', [characterId, slot.slot]);
        } else {
          await conn.query('UPDATE inventory SET quantity = quantity - ? WHERE character_id = ? AND slot_index = ?',
            [take, characterId, slot.slot]);
        }
        slot.qty -= take;
        needed   -= take;
      }
    }

    // Award result item — stack on existing slot or find empty slot
    const [[existing]] = await conn.query(
      'SELECT slot_index, quantity FROM inventory WHERE character_id = ? AND item_id = ? LIMIT 1',
      [characterId, recipe.result_item_id]
    );

    if (existing) {
      await conn.query(
        'UPDATE inventory SET quantity = quantity + 1 WHERE character_id = ? AND slot_index = ?',
        [characterId, existing.slot_index]
      );
    } else {
      const usedSlots = new Set(invRows.map(r => r.slot_index));
      let emptySlot   = 0;
      while (usedSlots.has(emptySlot)) emptySlot++;
      await conn.query(
        'INSERT INTO inventory (character_id, slot_index, item_id, quantity, equipped) VALUES (?, ?, ?, 1, 0)',
        [characterId, emptySlot, recipe.result_item_id]
      );
    }

    // Award profession XP
    const craftXp = Math.max(1, recipe.skill_level_required) * 5;
    await conn.query(
      `INSERT INTO professions (character_id, profession_id, skill_level, skill_xp)
       VALUES (?, ?, 1, ?)
       ON DUPLICATE KEY UPDATE skill_xp = skill_xp + ?`,
      [characterId, recipe.profession_id, craftXp, craftXp]
    );

    // Level-up loop for profession
    const [[profAfter]] = await conn.query(
      'SELECT skill_level, skill_xp FROM professions WHERE character_id = ? AND profession_id = ?',
      [characterId, recipe.profession_id]
    );
    let { skill_level, skill_xp } = profAfter;
    let leveled_up = false;
    while (skill_xp >= xpToNextLevel(skill_level)) {
      skill_xp  -= xpToNextLevel(skill_level);
      skill_level++;
      leveled_up = true;
    }
    await conn.query(
      'UPDATE professions SET skill_level = ?, skill_xp = ? WHERE character_id = ? AND profession_id = ?',
      [skill_level, skill_xp, characterId, recipe.profession_id]
    );

    await conn.commit();

    const profName = PROFESSION_NAMES[recipe.profession_id] ?? `profession_${recipe.profession_id}`;
    console.log(`[CRAFT] char ${characterId} ${recipe.recipe_type} → ${recipe.result_item_id} (+${craftXp} ${profName} xp)`);
    if (leveled_up)
      console.log(`[PROF]  char ${characterId} — ${profName} leveled to ${skill_level} via crafting`);

    return res.json({
      success: true,
      data: {
        result_item_id:     recipe.result_item_id,
        result_name:        recipe.result_name,
        result_rarity:      recipe.result_rarity,
        recipe_type:        recipe.recipe_type,
        craft_time_seconds: recipe.craft_time_seconds,
        xp_gained:          craftXp,
        leveled_up,
        skill_level
      }
    });
  } catch (err) {
    await conn.rollback();
    console.error(`POST /api/craft char#${characterId}: ${err.message}`);
    return res.json({ success: false, error: 'Craft failed — please try again' });
  } finally {
    conn.release();
  }
});
