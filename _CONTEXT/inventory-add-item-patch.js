// ═══════════════════════════════════════════════════════════════════════════════
//  VPS PATCH — POST /api/inventory/add-item
//
//  INSERT near the existing POST /api/inventory/save route in server.js.
//
//  Used by AfkGatheringStation to award a single item per gather tick.
//  Stacks on an existing slot for the same item_id; otherwise finds the first
//  empty slot (0-indexed, scans up to 24 slots).
//
//  After applying:
//    sudo systemctl restart crossworlds-auth
//    curl -s -X POST http://localhost:3000/api/inventory/add-item \
//      -H "Content-Type: application/json" \
//      -H "Authorization: Bearer <jwt>" \
//      -d '{"characterId":1,"itemId":"ore_copper","quantity":1}'
// ═══════════════════════════════════════════════════════════════════════════════

app.post('/api/inventory/add-item', requireJWT, async (req, res) => {
  const { characterId, itemId, quantity } = req.body;

  if (!characterId || !itemId || !quantity)
    return res.json({ success: false, error: 'Missing characterId, itemId, or quantity' });

  const qty = Math.min(Math.max(parseInt(quantity, 10), 1), 99);

  // Ownership check
  const char = await ownedCharacter(req, res, characterId);
  if (!char) return;

  // Validate item exists
  const [[item]] = await db.promise().query(
    'SELECT id, stackable, max_stack_size FROM items WHERE id = ?', [itemId]
  );
  if (!item)
    return res.json({ success: false, error: `Unknown item: ${itemId}` });

  const stackable = item.stackable !== 0;
  const maxStack = stackable ? Math.max(parseInt(item.max_stack_size, 10) || 99, 1) : 1;
  let remaining = qty;

  if (stackable) {
    const [existingStacks] = await db.promise().query(
      'SELECT slot_index, quantity FROM inventory WHERE character_id = ? AND item_id = ? AND equipped = 0 AND quantity < ? ORDER BY slot_index',
      [characterId, itemId, maxStack]
    );
    for (const existing of existingStacks) {
      if (remaining <= 0) break;
      const added = Math.min(remaining, maxStack - existing.quantity);
      await db.promise().query(
        'UPDATE inventory SET quantity = ? WHERE character_id = ? AND slot_index = ?',
        [existing.quantity + added, characterId, existing.slot_index]
      );
      remaining -= added;
    }
  }

  // Find first empty slot (scan 0–23)
  const [slots] = await db.promise().query(
    'SELECT slot_index FROM inventory WHERE character_id = ? ORDER BY slot_index',
    [characterId]
  );
  const usedSlots = new Set(slots.map(r => r.slot_index));
  while (remaining > 0) {
    let emptySlot = 0;
    while (usedSlots.has(emptySlot) && emptySlot < 24) emptySlot++;
    if (emptySlot >= 24) break;

    const added = Math.min(remaining, maxStack);
    await db.promise().query(
      'INSERT INTO inventory (character_id, slot_index, item_id, quantity, equipped) VALUES (?, ?, ?, ?, 0)',
      [characterId, emptySlot, itemId, added]
    );
    usedSlots.add(emptySlot);
    remaining -= added;
  }

  const stored = qty - remaining;
  if (stored <= 0)
    return res.json({ success: false, error: 'Inventory is full' });

  return res.json({
    success: remaining === 0,
    data: { stored, rejected: remaining }
  });
});
