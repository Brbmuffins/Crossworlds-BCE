# Necromancer server handoff

The Unity client and dedicated-server project reserve **class index 5** for
**Necromancer**. Existing indices 0-4 are unchanged and must never be reordered.

| Index | Canonical player-facing name | Existing prefab name where different |
|---:|---|---|
| 0 | Marauder | Marauder |
| 1 | Templar | Ironclad |
| 2 | Night Hunter | Shadowblade |
| 3 | Cleric | Cleric |
| 4 | Arcanist | Arcanist |
| 5 | Necromancer | Necromancer |

## Required auth/API work on the VPS

Work in `/opt/crossworlds-auth/server.js`; do not change the old gear endpoints or
tables. Append Necromancer to the existing `CLASS_NAMES` array and widen every
class-index validation from `0..4` to `0..5`. Search for both explicit numeric
checks and array-length assumptions.

The relevant flows to verify are:

- character creation/selection accepts `{"class_index":5}`;
- `GET /character` returns `class_index: 5` and `class_name: "Necromancer"`;
- the game-server authentication lookup returns class index 5 unchanged;
- any default base-stat map has an entry for index 5 (Necromancer's primary stat
  is Intelligence in Unity);
- hero-mastery creation and validation creates/accepts hero ID 5 instead of only
  IDs 0-4;
- any database CHECK constraint or enum that limits class or hero IDs is widened
  without renumbering existing rows.

Suggested base identity for a new character, if the API requires explicit starting
stats: Strength 5, Agility 5, Intelligence 10, Vitality 10. Keep the API as the
authority if the live balance table already defines different values.

## Rollout order

1. Apply and test the auth/API change on the VPS.
2. Restart `crossworlds-auth` and confirm its logs are clean.
3. Push the Unity commit to `main` so CI builds and deploys the dedicated server.
4. Run the Necromancer editor builder locally, save its generated prefab/data/scene
   changes, and include those generated assets in the Unity deployment commit.

## Acceptance test

Create/select a Necromancer account character and confirm all of the following:

- the API persists class index 5 after relogging;
- the dedicated server spawns `Necromancer.prefab`, not Marauder fallback;
- the character-select card and portrait appear;
- the four starter abilities appear in the action bar and Spell Forge;
- movement, cast, jump, and death animations play for local and remote clients;
- mastery and ordinary character XP still refresh normally.
