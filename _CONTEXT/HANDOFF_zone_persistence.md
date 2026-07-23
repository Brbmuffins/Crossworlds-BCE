# HANDOFF — Zone persistence (VPS session)

**For:** a Claude Code session running on the VPS (`/opt/crossworlds-auth/server.js`).
**Companion to:** ROADMAP.md task 6.2 (Phase 6 — Multi-Zone Persistent World).
**Written:** 2026-07-23, from the Unity repo side. Read `_CONTEXT/CLAUDE.md` first.

---

## Why this exists

Crossworlds is becoming a multi-zone persistent world (ROADMAP Phase 6). Several zones —
Hub, Darkwood, Ashen Wastelands, Toujam Basin, VoidDungeon — will be resident on one Unity
server process at once, with each player independently in one of them.

That requires the DB to answer **"which zone was this character in when they logged out?"**
Right now it cannot. The Unity client hardcodes the string `"GameWorld"` into every position
save (`Assets/Game/Networking/RodPositionSaver.cs:67`), so every character in the database
claims to be in the same nonexistent map. On login the server gets coordinates with no zone,
and will drop the player at Darkwood's coordinates inside Hub.

The Unity half is being fixed under task 6.2. This handoff covers the API half.

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
   or does it accept and silently discard it? Unity has been sending
   `{"x":…,"y":…,"z":…,"map":"GameWorld","orientation":…}` all along.
3. Does the `GET /character` response include the map/zone in its payload? The Unity side
   currently parses only `class_index`, `id`, `pos_x`, `pos_y`, `pos_z`, `gm_enabled`,
   `gm_level` — so even if you return it today, nothing reads it.
4. Is there any other writer to `characters.map` (dashboard, admin tooling, migration)?

---

## Step 2 — Recommended shape (confirm before building)

Because the position endpoints are sacred, **do not add zone semantics to them.** Add a new
endpoint under the `/api/*` convention instead, leaving the old path byte-identical:

```
GET   /api/character/:id/zone   → { success: true, data: { zone: "Darkwood" } }
PATCH /api/character/:id/zone   → body { zone: "Darkwood" }, returns { success: true }
```

New responses follow `{success, data}` / `{success, error}` with player-readable error strings
(root `CLAUDE.md`). Auth: same JWT bearer check as the other `/api/*` routes; the character
must belong to the requesting account — a player must not be able to relocate someone else's
character.

**Validate the zone against a server-side allowlist.** An unrecognised zone name must 400,
not be stored. A client that can write arbitrary zone strings can strand a character in a map
that does not exist, or teleport into content it has not unlocked. Current scene names:
`Hub`, `Darkwood`, `Ashen Wastelands`, `Toujam Basin`, `GM Island`, `VoidDungeon`.
`GM Island` must additionally require `gm_enabled` on the character.

If step 1 finds `characters.map` already populated and writable, reuse that column rather than
adding a second one — one source of truth for where a character is.

---

## Step 3 — Backfill

Every existing character has `map = "GameWorld"` (or NULL), which is not a real scene. Migrate
them to `Hub` so nobody logs into a nonexistent map. Hub is the safe default: it has spawn
points, no combat, and a waypoint out.

Take a DB backup before the UPDATE.

---

## Notes for the Unity side (not your job, listed so the contract is clear)

The repo session handles: sending the real zone name instead of `"GameWorld"`, adding a `map`
field to `RodPlayerAuth` and parsing it in `RodNetworkAuthenticator`, a periodic save tick
(30–60s) so a crash loses at most one interval instead of every online player's position, and
save-on-zone-change.

**Coordinate caveat — ROADMAP 6.7.** Zones will likely be offset in world space (Hub x=0,
Darkwood x=10000, …) to stop NavMesh and physics merging across additively-loaded scenes. If
that lands after players have accumulated saved positions, those absolute coordinates need
migrating. Flag it if you see saved positions piling up before 6.7 is done.

---

## Acceptance

- `GET`/`PATCH` zone round-trips for a character the caller owns; 401/403 for one they do not.
- An unrecognised zone name returns 400 and does not write.
- No behavioural change to `GET /character`, `POST /character`, or `PATCH /character/position`
  — diff them and confirm byte-identical, or explain exactly what changed and why.
- Every existing character row holds a real scene name.
- Report back what you changed so ROADMAP 6.2 can be marked done on the Unity side.
