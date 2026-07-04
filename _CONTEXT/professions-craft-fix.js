app.post('/api/craft', requireJWT, async (req, res) => {
  const { characterId, recipeId } = req.body;
  if (!characterId || !recipeId)
    return res.json({ success: false, error: 'Missing characterId or recipeId' });

  const char = await ownedCharacter(req, res, characterId);
  if (!char) return;

  // Bag UI is a 4×6 grid — 24 usable slots (0-23). Keep server + client in sync.
  const MAX_INV_SLOTS = 24;

  // Load recipe + result item metadata
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

  // Load current inventory (unequipped only — equipped gear is never a craft ingredient)
  const [invRows] = await db.promise().query(
    'SELECT slot_index, item_id, quantity FROM inventory WHERE character_id = ? AND equipped = 0',
    [characterId]
  );

  // Aggregate ingredient stock across slots, then verify we have enough
  const invMap = {};
  for (const row of invRows) {
    invMap[row.item_id] = invMap[row.item_id] || [];
    invMap[row.item_id].push({ slot: row.slot_index, qty: row.quantity });
  }
  for (const ing of ingredients) {
    const slots = invMap[ing.item_id] ?? [];
    const total = slots.reduce((s, r) => s + r.qty, 0);
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

    // Award result — stack onto an existing UNEQUIPPED stack of the same item …
    const [[existing]] = await conn.query(
      'SELECT slot_index FROM inventory WHERE character_id = ? AND item_id = ? AND equipped = 0 LIMIT 1',
      [characterId, recipe.result_item_id]
    );

    if (existing) {
      await conn.query(
        'UPDATE inventory SET quantity = quantity + 1 WHERE character_id = ? AND slot_index = ?',
        [characterId, existing.slot_index]
      );
    } else {
      // … otherwise place it in the first free slot. Re-query occupancy INSIDE the
      // transaction so slots freed by the deductions above are counted as free.
      const [occ] = await conn.query(
        'SELECT slot_index FROM inventory WHERE character_id = ?',
        [characterId]
      );
      const usedSlots = new Set(occ.map(r => r.slot_index));
      let emptySlot = 0;
      while (usedSlots.has(emptySlot) && emptySlot < MAX_INV_SLOTS) emptySlot++;
      if (emptySlot >= MAX_INV_SLOTS)
        throw Object.assign(new Error('Inventory is full'), { userError: true });

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

    // Level-up loop for the profession
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
    console.log(`[CRAFT] char ${characterId} ${recipe.recipe_type} -> ${recipe.result_item_id} (+${craftXp} ${profName} xp)`);
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
    if (err.userError)
      return res.json({ success: false, error: err.message });
    console.error(`POST /api/craft char#${characterId}: ${err.message}`);
    return res.json({ success: false, error: 'Craft failed — please try again' });
  } finally {
    conn.release();
  }
});
