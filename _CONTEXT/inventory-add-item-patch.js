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
    'SELECT id FROM items WHERE id = ?', [itemId]
  );
  if (!item)
    return res.json({ success: false, error: `Unknown item: ${itemId}` });

  // Try to stack on existing slot
  const [[existing]] = await db.promise().query(
    'SELECT slot_index, quantity FROM inventory WHERE character_id = ? AND item_id = ? LIMIT 1',
    [characterId, itemId]
  );

  if (existing) {
    const newQty = existing.quantity + qty;
    await db.promise().query(
      'UPDATE inventory SET quantity = ? WHERE character_id = ? AND slot_index = ?',
      [newQty, characterId, existing.slot_index]
    );
    return res.json({ success: true, data: { slot_index: existing.slot_index, quantity: newQty, stacked: true } });
  }

  // Find first empty slot (scan 0–23)
  const [slots] = await db.promise().query(
    'SELECT slot_index FROM inventory WHERE character_id = ? ORDER BY slot_index',
    [characterId]
  );
  const usedSlots = new Set(slots.map(r => r.slot_index));
  let emptySlot = 0;
  while (usedSlots.has(emptySlot) && emptySlot < 24) emptySlot++;

  if (emptySlot >= 24)
    return res.json({ success: false, error: 'Inventory is full' });

  await db.promise().query(
    'INSERT INTO inventory (character_id, slot_index, item_id, quantity, equipped) VALUES (?, ?, ?, ?, 0)',
    [characterId, emptySlot, itemId, qty]
  );

  console.log(`[GATHER] char ${characterId} received ${qty}x ${itemId} → slot ${emptySlot}`);
  return res.json({ success: true, data: { slot_index: emptySlot, quantity: qty, stacked: false } });
});
