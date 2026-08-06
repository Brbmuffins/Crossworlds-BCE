# Level/category character XP contract

This is the required replacement for the live auth server's per-template combat XP lookup.
The auth implementation lives at `/opt/crossworlds-auth/server.js` on the VPS and is not
mirrored into this Unity repository.

## Requests

`POST /api/combat/hit`:

```json
{
  "characterId": 42,
  "enemyLevel": 5,
  "enemyCategory": "brute",
  "enemyInstanceId": 1234,
  "damageDealt": 18.5
}
```

`POST /api/combat/kill`:

```json
{
  "characterId": 42,
  "enemyLevel": 5,
  "enemyCategory": "brute",
  "enemyInstanceId": 1234
}
```

Both remain JWT-protected and must verify that `characterId` belongs to the account.

## Server calculation

```js
const XP_MULTIPLIER = Object.freeze({
  grunt: 1,
  brute: 1.5,
  elite: 2,
  boss: 5,
});

function calculateKillXp(enemyLevel, enemyCategory) {
  if (!Number.isInteger(enemyLevel) || enemyLevel < 1 || enemyLevel > 100)
    throw new Error('invalid enemy level');

  const multiplier = XP_MULTIPLIER[enemyCategory];
  if (multiplier === undefined)
    throw new Error('invalid enemy category');

  return Math.round((10 * enemyLevel + 5) * multiplier);
}
```

The client must never submit `xpGained`. Continue using the existing transactional XP
award and level-up/stat-allocation loop, substituting `calculateKillXp(...)` for the
`enemy_templates.xp_reward` lookup.

## Hit gate

Key `recentHits` with account ID, character ID, and `enemyInstanceId`. Store the validated
level/category alongside the timestamp on hit. On kill, require the same instance, level,
and category within 30 seconds, then consume the entry. Retain the existing per-character
two-second kill cooldown.

## Expected values

| Enemy | Level | Category | XP |
|---|---:|---|---:|
| Basic grunt | 1 | grunt | 15 |
| Basic grunt | 2 | grunt | 25 |
| Gunda | 4 | grunt | 45 |
| Ogre Brute | 5 | brute | 83 |
| Level 5 elite | 5 | elite | 110 |
| Level 5 boss | 5 | boss | 275 |

## Acceptance check

1. Hit then kill Gunda: response contains `xpGained: 45`.
2. Hit then kill the level-5 Ogre Brute: response contains `xpGained: 83`.
3. Repeating kill without a new hit is rejected.
4. Unknown category, level 0, and level 101 are rejected.
5. No row in `enemy_templates` is required for character XP.
