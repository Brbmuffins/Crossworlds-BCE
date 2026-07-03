# ROADMAP.md — Phase 1 + Phase 2 Status (AI-readable)

> This is a structured snapshot for AI context. The visual HTML version is at the repo root.
> For actual game facts / DB schema, see CROSSWORLDS.md. For Unity tasks, read per-week below.

**Last verified:** 2026-06-29  
**Phase 1 scope:** 8 weeks → playtest 10–20 players, stress test, vote on Phase 2

---

## Overall Status

| Layer | Status |
|---|---|
| Server / VPS | ✅ Complete |
| Unity client | ⚠️ ~70% — scripts deployed, wiring + prefabs remain |
| Database schema | ✅ Complete (incl. Phase 2 stubs) |
| Web / download page | ✅ Live at playcrossworlds.com |
| combat-atlas.html | ✅ Live at playcrossworlds.com/combat-atlas.html |

---

## Unity Scripts — What's Now in Assets/Game/ (as of 2026-06-29)

All scripts are in `CrossWorlds/Assets/Game/`. **No Inspector wiring needed** — all use self-bootstrap patterns.

### Combat/Scripts/
| Script | Status | Notes |
|---|---|---|
| Health.cs | ✅ Deployed | Server-authoritative HP, UnityEvents |
| EnemyController.cs | ✅ Deployed | FSM: Idle/Chase/Attack/Dead, melee + ranged |
| EnemyProjectile.cs | ✅ Deployed | Server-spawned ranged projectile |
| DropTable.cs | ✅ Deployed | ScriptableObject weighted loot rolls |
| WorldItem.cs | ✅ Deployed | Floor pickup, rarity glow, network-synced |
| WaveSpawner.cs | ✅ Deployed | Server-authoritative wave escalation |
| WorldBossController.cs | ✅ Deployed | Phase-based world boss |
| CombatSessionTracker.cs | ✅ NEW | Tracks dmg/kills/waves → POST /api/combat/session/end |
| SoulBondTether.cs | ✅ NEW | Cleric LineRenderer tether to bonded ally |

### Systems/
| Script | Status | Notes |
|---|---|---|
| InventoryManager.cs | ✅ Deployed | Singleton, GET/POST /api/inventory/* |
| ItemCatalogManager.cs | ✅ NEW | Singleton, loads GET /api/items, no auth needed |
| HeroMasteryManager.cs | ✅ NEW | Singleton, GET/POST /api/mastery/*, fires OnMasteryLevelUp |
| NPCInteractionManager.cs | ✅ NEW | Singleton, E-key routing, auto-disables in Arena |

### Characters/Scripts/
| Script | Status | Notes |
|---|---|---|
| TalentModifierApplier.cs | ✅ Deployed | Applies talent modifiers to stats |
| HeroCosmeticApplier.cs | ✅ NEW | Per-player mastery tier tints (Bronze/Silver/Gold/Diamond) |

### UI/
| Script | Status | Notes |
|---|---|---|
| GuildPanelUI.cs | ✅ Deployed | G-key guild panel |
| QuestLogUI.cs | ✅ Deployed | Q-key quest log |
| QuestTracker.cs | ✅ Deployed | HUD quest tracker widget |
| OnlinePlayersHUD.cs | ✅ Deployed | Online player count display |
| TalentTreeUI.cs | ✅ Deployed | Talent tree panel |
| WorldBossHealthBar.cs | ✅ Deployed | Full-width boss HP bar |
| ShieldValueHUD.cs | ✅ NEW | World-space shield bar above shielded ally |
| ClericRadarUI.cs | ✅ NEW | Low-HP ally radar (Cleric-only, classIndex==3) |
| StatusEffectHUD.cs | ✅ NEW | 6-icon status effect row with timers |
| HangmanDialogueUI.cs | ✅ NEW | The Hangman NPC dialogue panel, fade in/out, ESC close |

### Scene/
| Script | Status | Notes |
|---|---|---|
| HangmanNPC.cs | ✅ NEW | Hub NPC → arena entry trigger, Mirror Command/ClientRpc |

### Editor/
| Script | Status | Notes |
|---|---|---|
| EnemyBuilder.cs | ✅ Deployed | BCE editor tool — enemy prefab setup |
| WorldBossBuilder.cs | ✅ Deployed | BCE editor tool — boss setup |
| Phase2Builder.cs | ✅ Deployed | BCE editor tool — Phase 2 scene setup |

---

## Still Missing from Assets/Game/ (copy from VPS when ready)

Scripts at `/opt/crossworlds-auth/unity-scripts/` on VPS — not yet local:
- `ApiClient.cs` — typed HTTP wrapper for all endpoints
- `EnemyTemplate.cs` + `EnemyTemplateRegistry.cs` — enemy data from GET /api/enemies
- `EnemyAI.cs` — NavMesh state machine using EnemyTemplate stats
- `PlayerHealth.cs` — player HP stub, die → scene reload
- `HUDManager.cs` — arena HUD (TextMeshPro level/XP/gold)
- `CraftingManager.cs` — professions + recipes + POST /api/craft

Scripts confirmed in Session B output but not yet located:
- `SoulBondTether.cs` — ✅ rewritten 2026-06-29
- `ShieldValueHUD.cs` — ✅ rewritten 2026-06-29
- `ClericRadarUI.cs` — ✅ rewritten 2026-06-29
- `StatusEffectHUD.cs` — ✅ rewritten 2026-06-29
- `CombatSessionTracker.cs` — ✅ rewritten 2026-06-29
- `ClassPoolBuilder.cs` — NOT yet written
- `FloatingDamageText.cs` (extended) — NOT yet written

Scripts confirmed in main project but in separate Unity folder (Assets/Game/ in original project):
- `PlayerProgressManager.cs`, `XpBar.cs`, `CharacterSheetUI.cs`, `LevelUpScreen.cs`
- `CraftingUI.cs`, `ForgeNPC.cs`, `ResourceNode.cs`
- `AbilityCaster.cs`, all Passive*.cs scripts
- `PortalTransition.cs`, `PlayerIdentity.cs`, `RodNetworkManager.cs`
- `HubSceneBuilder.cs`, `ArenaSceneBuilder.cs`
- `FloatingDamageText.cs`, `EnemyHealthBar.cs`, `PlayerHealthBar.cs`
- `AbilityHUD.cs`, `WaveHUD.cs`, `ArenaClearUI.cs`, `EnemyDeathVFX.cs`
- `LoginManager.cs`, `GmConsole.cs`
- `StatusEffect.cs`, `StatusEffectManager.cs`

---

## Wire-Up Still Needed in Unity Editor

| Task | Where | Notes |
|---|---|---|
| Add Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem to NetworkManager.spawnPrefabs | Inspector | CRITICAL — spawns silently fail without this |
| Bake NavMesh in Arena scene | Unity AI | Required for EnemyController pathfinding |
| Set enemyTemplateId on each enemy prefab | Inspector | Links to GET /api/enemies data |
| Add Player tag to player prefabs | Inspector | EnemyController aggro scan uses CompareTag("Player") |
| Place HangmanNPC in Hub scene | Scene | Interactable NPC — adjust interactRadius in Inspector |
| Add #if !UNITY_SERVER to GmConsole.cs | Code | Open bug — crashes server build every frame |

---

## Open Bugs

| Bug | Location | Priority |
|---|---|---|
| `GmConsole.cs` — no `#if !UNITY_SERVER` guard, crashes server build | Unity | High |
| `orientation:F3` — PATCH /character/position sends formatted float | Unity | Medium |
| `WaveSpawner` not calling `CombatSessionTracker.Local?.NotifyEnemySpawned(go)` | WaveSpawner.cs line 143 | Medium |
| `RodNetworkManager` not calling `CombatSessionTracker.Local?.NotifyAllySpawned(go)` | RodNetworkManager.cs | Medium |
| `CombatSessionTracker.BeginSession()` not called on Arena scene load | ArenaSceneBuilder / PortalTransition | Medium |

---

## Phase 2 — Server Complete, Unity Pending

Server endpoints live as of 2026-06-28:
- Talent trees: GET /api/talents/tree/:heroClass, GET/POST /api/talents/:characterId, POST /api/talents/invest, POST /api/talents/respec
- Guilds: POST /api/guilds/create, invite, leave, GET /api/guilds/:id, PATCH /api/guilds/motd
- Quests: 10 starter quests seeded, GET /api/quests/available, POST /api/quests/accept/progress/complete
- Combat sessions: POST /api/combat/session/end, POST /api/combat/death, GET /api/combat/stats/:characterId
- Leaderboards: GET /api/leaderboard/damage|healing|waves|mastery
- Hero mastery: GET /api/mastery/:characterId, POST /api/mastery/award

Unity client Phase 2 scripts deployed (in Assets/Game/):
- TalentTreeUI.cs, TalentModifierApplier.cs, GuildPanelUI.cs, QuestLogUI.cs, QuestTracker.cs
- CombatSessionTracker.cs (new), HeroMasteryManager.cs (new), HeroCosmeticApplier.cs (new)
- HangmanNPC.cs (new), HangmanDialogueUI.cs (new), NPCInteractionManager.cs (new)
