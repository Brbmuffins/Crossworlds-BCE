# HANDOFF — Zone persistence (VPS session)

**For:** a Claude Code session running on the VPS (`/opt/crossworlds-auth/server.js`).
**Companion to:** ROADMAP.md task 6.2 (Phase 6 — Multi-Zone Persistent World).
**Written:** 2026-07-23. **Revised:** 2026-07-23 after Phase 6.3/6.4 landed — the Unity side is
further along than when this was first drafted, and three things below are new. Read
`_CONTEXT/CLAUDE.md` first.

---

## Why this exists

Crossworlds is now a multi-zone persistent world. Several zones — Hub, Darkwood, Ashen
Wastelands, Toujam Basin, GM Island, VoidDungeon — are resident on one Unity server process at
once, and each player is independently in one of them. That shipped on the Unity side in
ROADMAP 6.3/6.4.

That requires the DB to answer **"which zone was this character in when they logged out?"**
Right now it cannot. The Unity client used to hardcode the string `"GameWorld"` into every
position save, so every character in the database claims to be in the same nonexistent map.

The Unity half is **done** (ROADMAP 6.2, client half): the game now sends the player's real zone
in the existing `map` field of `PATCH /character/position`, saves every 45s instead of only on
disconnect, and reads `map` back off `GET /character` into the spawn path. This handoff covers
the API and data half.

---

## ⚠ Read this before touching anything

`GET /character`, `POST /character`, and `PATCH /character/position` are on the **sacred list**
(`_CONTEXT/CLAUDE.md` §"Old gear system — LEAVE ALONE"). Unity calls them on every single spawn.
Breaking one takes the whole game down.

**Therefore: step 1 is inspection, not modification.** Do not write code until you have
reported findings back.

---

## Step 1 — Inspect and report (no changes)

Answer these against the live VPS and report back before doing anything else:

1. Does the `characters` table have a `map` column? What type, what default, and what
   distinct values does it currently hold?
   (`SELECT map, COUNT(*) FROM characters GROUP BY map;`)
2. Does `PATCH /character/position` actually persist the `map` field from the request body,
   or does it accept and silently discard it? Unity has always sent it, and now sends a real
   zone name rather than `"GameWorld"` — so if the endpoint already persists it, much of this
   job may already be done and you should say so rather than building something new.
3. Does the `GET /character` response include the map/zone? Unity now parses a `map` field off
   that response (`CharacterResponse.map` → `RodPlayerAuth.zone`). If the API does not return
   it, the client silently falls back to Hub for everyone — which is safe, but means zone
   persistence is a no-op until you add it.
4. Is there any other writer to `characters.map` (dashboard, admin tooling, migration)?

---

## Step 2 — Recommended shape (confirm before building)

Because the position endpoints are sacred, **do not add zone semantics to them.** Prefer adding
to the `GET /character` response only if step 1 shows `map` is already stored and returning it is
purely additive; otherwise add a new endpoint under the `/api/*` convention:

```
GET   /api/character/:id/zone   → { success: true, data: { zone: "Darkwood" } }
PATCH /api/character/:id/zone   → body { zone: "Darkwood" }, returns { success: true }
```

New responses follow `{success, data}` / `{success, error}` with player-readable error strings
(root `CLAUDE.md`). Auth: same JWT bearer check as the other `/api/*` routes; the character must
belong to the requesting account — a player must not be able to relocate someone else's
character.

### The allowlist (this is the security-relevant part)

**Validate the zone against a server-side allowlist.** An unrecognised zone must 400, not be
stored. A client that can write arbitrary zone strings can strand a character in a map that does
not exist, or place itself into content it has not unlocked.

Valid zones, mirroring `SceneNames.Zones` in the Unity client:

```
HUB
Darkwood
Ashen Wastelands
Toujam Basin
GM Island
VoidDungeon
```

Three rules that are easy to get wrong:

- **`GM Island` must additionally require `gm_enabled` on the character.** It is the one zone in
  the list that is a privilege, not a place.
- **`_Container` must NEVER be accepted.** It is the empty scene the NetworkManager lives in —
  every zone loads additively on top of it. A character "in" the container is a character
  standing in the void.
- **`Arena_Copper` and `Gathering Zone` are deliberately absent.** They appear in the Unity
  `SceneNames.Zones` array but **have no scene file yet** — see the warning in ROADMAP Phase 6.
  Do not add them to the allowlist until someone confirms the scenes exist.

### Instanced zones must not be persisted (new — from 6.3)

`VoidDungeon` and the arenas are **instanced**: each party gets its own copy, created on entry
and destroyed when the last member leaves. A character who logs out inside one cannot be put back
there — that instance no longer exists.

So: when a save arrives with an instanced zone, **store `HUB` instead**, or store the instanced
name and have the read path translate it to `HUB`. Either is fine; pick one and comment which.
The Unity client does not currently do this translation, so if the API does not, a player who
logs out in a dungeon will be sent back to the dungeon on login, ZoneManager will spin up a fresh
empty instance, and they will wake up alone in a dead dungeon with no party.

---

## Step 3 — Backfill

Every existing character has `map = "GameWorld"` (or NULL), which is not a real scene. Migrate
them to `HUB` so nobody logs into a nonexistent map. Hub is the safe default: it has spawn
points, no combat, and a waypoint out.

Note the exact casing — the Unity scene is `HUB`, not `Hub`. The client compares
case-insensitively (`SceneNames.IsZone`), but matching the real name keeps the DB readable.

Take a DB backup before the UPDATE.

---

## What is already done on the Unity side (so you do not rebuild it)

- Real zone name sent in `map` on `PATCH /character/position` (`RodPositionSaver`).
- Periodic save every 45s, staggered per player so simultaneous joins do not burst the auth
  server; plus a save immediately before any zone change.
- `map` parsed off `GET /character` into `RodPlayerAuth.zone` and consumed at spawn.
- Client-side allowlist (`SceneNames.NormalizeZone`) collapsing unknown/legacy values to Hub.
  **This is belt-and-braces, not a substitute for your server-side check** — it runs on the
  server-side Unity process, but the DB is the thing multiple clients share.

## Not needed yet: party persistence

ROADMAP Phase 7 adds a party system. The recommended design is **ephemeral parties** — server
memory only, dying with the last member's disconnect — which needs **no DB tables and no VPS
work at all**. Do not build party persistence as part of this handoff. It only becomes a VPS
concern if guilds or persistent groups get designed later, or if Phase 7.5 (party XP/loot
attribution) forces `/api/combat/kill` to understand groups. Both are flagged in ROADMAP.

---

## Acceptance

- `GET`/`PATCH` zone round-trips for a character the caller owns; 401/403 for one they do not.
- An unrecognised zone name returns 400 and does not write. `_Container` is rejected.
- `GM Island` is rejected for a character without `gm_enabled`.
- An instanced zone (`VoidDungeon`) never persists as itself — a player logging out there comes
  back in Hub.
- No behavioural change to `GET /character`, `POST /character`, or `PATCH /character/position`
  beyond additively returning `map` — diff them and confirm, or explain exactly what changed
  and why.
- Every existing character row holds a real scene name.
- Report back what you changed so ROADMAP 6.2 can be marked done on the Unity side.
