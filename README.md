# Rate of Decay ONLINE

**Genre:** 4-10 player co-op combat MMO  
**Engine:** Unity 6 LTS · Universal Render Pipeline  
**Networking:** Mirror (KCP transport, port 7777) · Server-authoritative  
**Backend:** Node.js / Express 5 · MySQL · VPS auth server

---

## Classes

| Class | Role | Identity |
|-------|------|----------|
| **Engineer** | Damage / Control | Turrets, shock mines, arc cannon — builds the killzone before enemies arrive |
| **Guardian** | Tank / CC | Iron Tether, Breach Slam, Siege Mode — the anvil the team fights around |
| **Wraith** | DoT / Debuff | Corruption stacks, Null Field, Collapse detonation — no aim required, pure pressure |
| **Medic** | Support | Emergency revivals, heal shields, Transfer Protocol — keeps the team alive |
| **Phaser** | Assassin | Phase shifts, singularity pulls, spatial repositioning *(not yet playable)* |

Each class has 4 equipped abilities + 1 ultimate. See [`COMBAT.md`](COMBAT.md) for full ability tables, combos, and design intent.

---

## Systems Built

### Networking
- Mirror host/client live end-to-end (KCP transport, port 7777)
- `RodNetworkAuthenticator` — JWT verify → server-side GET /character → stores class + spawn position in `conn.authenticationData`
- `RodNetworkManager` — server-authoritative class selection; reads DB class in production, client message in dev mode
- `RodNetworkAuthenticator` dev mode — bypasses JWT for local HOST testing with one click

### Authentication & Characters (VPS)
- Node.js / Express 5 auth server at `/opt/rod-auth/` · port 3000 · systemd service `rod-auth`
- Admin dashboard at `/opt/rod-dashboard/` · port 4000 · HTTP Basic auth
- `POST /register` · `POST /login` · `GET /health`
- `POST /character` — idempotent create/confirm, returns full character + 6-slot gear
- `GET /character` — spawn-path critical: class index + last saved position + equipped gear
- `PATCH /character/position` — saves X/Y/Z/map/orientation on disconnect
- `POST /character/gear/equip` · `GET /items`

### Database (rod_online · MySQL)
- `characters` — account FK, class_index, level, XP, position, online flag, last_logout
- `item_template` — static gear definitions, 12 seeded rows, 6 slots, per-stat ranges
- `item_instance` — per-player items with rolled stats
- `character_gear` — equipped loadout (one row per slot per character)
- `loot_tables` — exists, wired for future drop system

### Scene Flow
```
LoginScene → CharacterSelect → GameWorld
```
- `LoginManager` — full MMO login UI built in code; username/password auth, register panel, server IP field (saves to PlayerPrefs), animated title, dev HOST button
- `CharacterSelectManager` — 3D character preview via RenderTexture on layer 31; class list, ability readout, ENTER WORLD; reads server IP from PlayerPrefs before StartClient()
- `RodPositionSaver` — attached at runtime to server-side player objects; PATCH /character/position on disconnect or app quit via temporary coroutine host

### Combat
- `Health` — shields, damage reduction, absorb window, downed state (players), gear stat channels, `onDeath` UnityEvent
- `StatusEffectManager` — Slow, Stagger, Suppress, DamageOverTime, Exposed, Tethered; per-tick DoT, `ConsumeDebuffStacks()` for Wraith Collapse
- `EnemyAI` — aggro, move, attack, stealth suppression window, status-gated actions
- `TurretController` — range scan, retarget interval, muzzle flash, LineRenderer tracer, Drone Command override, System Overload burst
- `AbilityCaster` — Cone / Circle / Rectangle targeting indicators; full 20+ ability spellbook
- `WraithAbilities` — complete DoT kit: Corruption (passive), Dark Blast (cone), Null Field (zone), Event Horizon (expose), Collapse (detonate all debuffs), Phase Shift (2s invuln + exit poison)
- `WaveChest` — hold-E activation, prep window, per-wave enemy spawning, player-count scaling, loot on clear
- `WaveManager` — scene-level arena orchestrator; wave definitions with mob/elite/boss mix, difficulty multiplier per cycle (×1.2), shared boss health pool, loot score = wave × difficulty × √playerCount, arena boundary leash, fail detection

### Editor Automation (`RoD →` menu)
| Menu Item | What it does |
|-----------|-------------|
| `Setup/0 ▶ Create Character Select Scene` | Builds CharacterSelect.unity with 3D preview camera, layer 31 isolation, EventSystem |
| `Setup/1 ▶ Create Login Scene` | Builds LoginScene with NetworkManager, authenticator, KCP transport, UI |
| `Setup/2 ▶ Clean GameWorld` | Removes stray NetworkManager components from GameWorld |
| `Setup/3 ▶ Fix Build Settings` | Sets scene order: Login(0) → CharacterSelect(1) → GameWorld(2) |
| `Setup/4 ▶ Create Class Prefabs` | Creates Engineer / Guardian / Wraith / Medic prefabs from FBX, assigns to NetworkManager |
| `Setup/5 ▶ Fix Animator Controllers` | Assigns AnimatorController to all class prefabs |
| `World/Populate GameWorld with NPCs` | Places Zompy, Bob, Kodiac, Turret NPCs with VFX and ground plane |
| `World/Build Combat Base` | Populates GameWorld: arena floor, cover blocks, Sentinel Turret (ElectricalSparks VFX), 3 zone indicators (Circle/Cone/Rect with Dark Arts VFX), 3 enemy clusters, WaveChest, ambient VFX |

### VFX
- brbmuffins Technologies particle pack (ElectricalSparks, EnergyExplosion, SmallExplosion, FireFlies, HeatDistortion)
- brbmuffins Dark Arts fantasy pack (Magic circle, Death magic circle, Lightning strike, Mana wall, Ground spikes, Fireball)
- `RodBillboard` — zone label text always faces camera
- `EnemyDeathVFX` — spawns death particles via `Health.onDeath`
- `LoginScreenVFX` — ambient atmosphere on login screen

---

## Design Pillars

- **Harder = more reward** — wave difficulty multiplier feeds loot score; higher cycles = rarer drops
- **Shared common goal** — one enemy pool, one boss HP bar; every player's damage contributes
- **Multiple paths** — Guardian tanks, Engineer builds, Medic sustains, Wraith pressures — all valid, all needed
- **Zone in and battle** — no lobby meta, no mandatory prep; pick class, enter arena, fight
- **Community crafting** — server hub (max-player zone) has trainers, shared loot pool, community-built upgrades *(planned)*
- **DoT class complete** — Wraith is fully playable with the corruption → stack → detonate loop

---

## Project Structure

```
Assets/
├── Game/
│   ├── Combat/Scripts/   — Health, EnemyAI, TurretController, WaveChest, WaveManager,
│   │                       WraithAbilities, StatusEffect(Manager), EnemyDeathVFX, RodBillboard
│   ├── Editor/           — RodEditorSetup, RodCombatWorldBuilder, RodNpcBuilder,
│   │                       RodPrefabBuilder, RodProjectSettings (HTTP fix, InitializeOnLoad)
│   ├── Networking/       — RodNetworkManager, RodNetworkAuthenticator, RodPositionSaver
│   ├── Player/           — PlayerMovement (dodge roll), CameraFollow, ability handlers
│   ├── Prefabs/          — Engineer, Guardian, Wraith, Medic
│   ├── Scenes/           — LoginScene.unity, CharacterSelect.unity
│   └── UI/               — LoginManager, CharacterSelectManager, LoginScreenVFX, AbilityCaster, AbilityBar
├── brbmuffins */         — VFX asset packs
├── Mirror/               — Mirror Networking
└── Docs/                 — vps-character-system-prompt.txt, BackendSetup
brbmuffins Skybox/
└── Scenes/GameWorld.unity
COMBAT.md                 — Combat bible: dodge system, class breakdowns, combos
DESIGN_DOCUMENT.md        — Full system design and architecture decisions
```

---

## Setup

**Prerequisites:** Unity 6 LTS, Mirror (included), Node.js on VPS

**First-time (run in order):**
1. Open project in Unity
2. `Edit → Project Settings → Player → Other Settings` → **Allow downloads over HTTP → Always allowed**
3. `RoD → Setup → 0 ▶ Create Character Select Scene`
4. `RoD → Setup → 1 ▶ Create Login Scene`
5. `RoD → Setup → 4 ▶ Create Class Prefabs`
6. `RoD → Setup → 5 ▶ Fix Animator Controllers`
7. `RoD → World → Build Combat Base`
8. Press Play in LoginScene → **HOST (DEV)** → pick class → **ENTER WORLD**

**For multiplayer (others joining your session):**
- Players enter your IP in the **GAME SERVER** field on the login screen before logging in
- Default is the VPS IP; change to host's LAN/WAN IP for player-hosted sessions

For VPS auth server setup: `Docs/vps-character-system-prompt.txt`

---

## About

Mostly smarter people than Brbmuffins. Some Unity store assets. Lots of AI. Not much of my own original work really. Puprets

---
