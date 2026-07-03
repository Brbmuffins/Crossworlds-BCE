// ═══════════════════════════════════════════════════════════════════════════════
//  VPS PATCH — POST /api/professions/award-xp
//
//  INSERT THIS BLOCK into /opt/crossworlds-auth/server.js
//  Place it near the existing GET /api/professions/:characterId route.
//
//  XP formula (matches ProfessionManager.cs + ProfessionsPanel.ts):
//    xpToNextLevel = currentLevel × 50
//    e.g. level 1 → 2 requires 50 xp; level 10 → 11 requires 500 xp
//
//  After applying:
//    sudo systemctl restart crossworlds-auth
//    curl -s -X POST http://localhost:3000/api/professions/award-xp \
//      -H "Content-Type: application/json" \
//      -H "Authorization: Bearer <jwt>" \
//      -d '{"characterId":1,"professionId":2,"xpAmount":10}'
// ═══════════════════════════════════════════════════════════════════════════════

// ── Profession ID labels (used in log messages only) ────────────────────────────
const PROFESSION_NAMES = ['Woodcutting', 'Fishing', 'Mining'];

// ── XP thresholds ────────────────────────────────────────────────────────────────
function xpToNextLevel(currentLevel) {
  return Math.max(1, currentLevel) * 50;
}

// ── POST /api/professions/award-xp ───────────────────────────────────────────────
app.post('/api/professions/award-xp', requireJWT, async (req, res) => {
  const { characterId, professionId, xpAmount } = req.body;

  // ── Validate inputs ──────────────────────────────────────────────────────────
  if (!characterId || professionId === undefined || !xpAmount)
    return res.json({ success: false, error: 'Missing characterId, professionId, or xpAmount' });

  if (typeof professionId !== 'number' || professionId < 0 || professionId > 10)
    return res.json({ success: false, error: 'Invalid professionId' });

  const xp = Math.min(Math.max(parseInt(xpAmount, 10), 1), 500); // cap 1–500 per call

  // ── Ownership check ──────────────────────────────────────────────────────────
  const [chars] = await db.promise().query(
    'SELECT id FROM characters WHERE id = ? AND account_id = ?',
    [characterId, req.user.accountId]
  );
  if (!chars.length)
    return res.json({ success: false, error: 'Character not found or not yours' });

  // ── Upsert profession record ─────────────────────────────────────────────────
  await db.promise().query(
    `INSERT INTO professions (character_id, profession_id, skill_level, skill_xp)
     VALUES (?, ?, 1, 0)
     ON DUPLICATE KEY UPDATE skill_xp = skill_xp`,
    [characterId, professionId]
  );

  // ── Load current state ───────────────────────────────────────────────────────
  const [[prof]] = await db.promise().query(
    'SELECT skill_level, skill_xp FROM professions WHERE character_id = ? AND profession_id = ?',
    [characterId, professionId]
  );

  let { skill_level, skill_xp } = prof;
  skill_xp += xp;

  let leveled_up = false;

  // Level-up loop (can gain multiple levels from one tick at low level)
  while (skill_xp >= xpToNextLevel(skill_level)) {
    skill_xp  -= xpToNextLevel(skill_level);
    skill_level++;
    leveled_up = true;
  }

  // ── Persist ──────────────────────────────────────────────────────────────────
  await db.promise().query(
    `UPDATE professions SET skill_level = ?, skill_xp = ?
     WHERE character_id = ? AND profession_id = ?`,
    [skill_level, skill_xp, characterId, professionId]
  );

  const profName = PROFESSION_NAMES[professionId] ?? `profession_${professionId}`;
  if (leveled_up)
    console.log(`[PROF] char ${characterId} — ${profName} leveled up to ${skill_level}`);

  return res.json({
    success: true,
    data: { skill_level, skill_xp, leveled_up, profession_id: professionId }
  });
});
