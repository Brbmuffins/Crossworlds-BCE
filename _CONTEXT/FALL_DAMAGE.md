# Player fall damage (2026-09-18)

The Unity game server attaches `PlayerFallDamage` to each spawned player from
`Health.OnStartServer`. It samples server-observed positions and verifies walkable
ground with a raycast in the player's active physics scene. Landing after at least
0.15 seconds airborne deals `maxHealth * clamp01((fallDistance - 12) * 0.012)`.
Thus a 22 m fall deals 12% maximum health before the existing Health shield,
reduction, immunity, and downed rules. The coefficients are initial tuning, not
copied from AzerothCore. Enemy health and the auth API are unchanged.

Tracking resets on a large one-tick upward/horizontal jump (teleport safeguard), explicit
zone/respawn teleports, zone-transition protection, downed state, water triggers,
and server-authorized GM flight. A client cannot request fall damage or submit a
fall distance. However, client-authoritative movement still means the observed
position stream is not fully cheat-proof; complete anti-cheat would require
server-authoritative movement validation.

Before release, compile in Unity and test ordinary jumps, 12/22/40 m drops,
low-health landing, shield and damage-reduction behavior, water, slopes, moving
platforms, knockbacks, ability movement, respawn, zone portals, GM flight, and
latency on both host and dedicated-server/client sessions. Confirm the server
logs no missing collider/physics-scene errors and each landing damages once.
