# Backend GM Permissions Handoff

We added a first-pass GM command framework in Unity. Commands are typed through chat, but enforcement happens server-side in the game server, not on the client.

## Current Working Commands

`/gm on`

`/gm off`

`/gmhelp`

`/arrive hub`

`/arrive darkwood`

`/arrive ashen`

`/fly`

`/fly on/off`

`/speed <multiplier>`

## Current Unity Expectation

During Mirror authentication, the game server already calls the auth server's `/character` endpoint using the player JWT. We need that response to include GM permission fields:

```json
{
  "id": 123,
  "class_index": 0,
  "class_name": "Warden",
  "pos_x": 0,
  "pos_y": 2,
  "pos_z": 0,
  "gm_enabled": true,
  "gm_level": 10,
  "gm_permissions": "arrive,fly,speed"
}
```

## Suggested Backend Model

GM permissions should live on the account/user record, not the character record. The `/character` response can include the account's GM fields because the game server already consumes that endpoint during login.

Suggested DB fields:

```sql
gm_enabled BOOLEAN DEFAULT false
gm_level INTEGER DEFAULT 0
gm_permissions TEXT DEFAULT ''
```

## Security Model

The client never grants itself GM access. The Unity client only sends slash command text. The game server checks the authenticated connection's server-side auth data before running any GM command.

How it works after integration:

1. Player logs in and receives JWT.
2. Game client connects to Mirror server.
3. Mirror server sends JWT to auth server via `/character`.
4. Auth server verifies JWT and returns character data plus GM fields.
5. Game server stores those GM fields on the player connection.
6. `/gm on` only enables the session toggle if that account is authorized.
7. `/arrive`, `/fly`, and `/speed` only run if GM is authorized and currently toggled on.

## Current Temporary Unity Fallback

For testing, dev mode and a small hardcoded username allowlist can use GM commands. Once backend GM fields are live, we should remove or disable the temporary username allowlist.

## Future Recommended Addition

Add GM audit logging, either in the game server logs or via a backend endpoint:

`POST /api/gm/audit`

Payload example:

```json
{
  "accountId": 123,
  "username": "brbmuffins",
  "command": "arrive",
  "args": "ashen",
  "scene": "HUB",
  "timestamp": "2026-07-20T18:30:00Z"
}
```

## Core Rule

The auth server decides who is GM, the game server enforces every command, and the client UI is never trusted.
