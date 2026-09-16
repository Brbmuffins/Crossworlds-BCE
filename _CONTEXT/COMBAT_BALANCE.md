# Combat scaling and tuning

The Unity dedicated server resolves damage. Authenticated character stats and
server-fetched equipment bonuses feed `CharacterStats`; the client cannot choose
those values. The retired local equipment path and old gear API are unchanged.

## Versioned ability coefficients

`Assets/Game/Resources/Combat/ability-scaling-v1.json` is the bundled version 1
balance definition. Each entry names a class index, exact `AbilityDef.abilityName`,
scaling stat, and extra damage per stat point above baseline. Classes use the
existing 0–5 indices; valid stats are Strength, Agility, Intelligence, Vitality.
Strength/Agility/Intelligence use baseline 5; Vitality uses baseline 10. A new
damaging ability already receives the existing global character damage multiplier.
Add a rule only when it needs additional ability-specific scaling. A damaging
variant uses its own rule if present, otherwise its parent's rule. Support and
healing effects do not receive ability-specific damage coefficients.

The server reads its own bundled definition on first use. On a dedicated server,
`CROSSWORLDS_COMBAT_BALANCE_PATH` may point to an administrator-owned JSON override
so tuning can change after a server restart without a Unity rebuild. The override
must use the same schema and a **new version number**. Invalid or missing JSON
disables ability-specific bonuses and logs an error; it does not change global
stat scaling. Do not edit the live override without reviewing and backing it up.
The server synchronizes its balance version to each player's `CharacterStats`.
The client character sheet and ability/spellbook tooltips only display ability
coefficients when the bundled version matches the server version. Updating an
override's coefficients therefore also requires a matching client release for
accurate tooltip explanations.

## Damage order

For an ordinary uncharged hit, the cast multiplier produces the equivalent of
`(ability base damage + extra stat damage) × existing global damage multiplier`.
Charge, phase/mastery bonuses, critical hits, and target reductions continue
through the existing combat path. Charged and multi-hit effects scale
proportionally from the ability's base hit; their exact totals require play-mode
verification. Spawned player-owned damage snapshots the cast multiplier once;
damage-over-time ticks do not apply it a second time. Enemy damage is untouched.

## Baseline and play-mode checks before wider tuning

Record damage and time-to-kill at the same character level, target, ability,
charge state, and gear setup. Compare no gear with a known +10 relevant-stat
piece. The pilot ability-specific term should be +2.5 damage before the global
multiplier at +10 above baseline. Check Ice Spikes, Blade Flurry and variants;
then a mine, turret, nanite chip tick, null-field curse tick, Shieldwall Charge,
and Counter Blow. Confirm one curse tick is not multiplied twice and that a
support/heal ability and an enemy attack stay unchanged. Check character-sheet
breakdown and version-mismatch messaging. Unity 6.4 editor compilation
completed with zero errors (19 unrelated warnings), and `BCE → Validate →
Combat Balance` passed for version 1 and both ability rules. Host and
dedicated-server/client play tests, including time-to-kill measurement,
remain required before deployment.
