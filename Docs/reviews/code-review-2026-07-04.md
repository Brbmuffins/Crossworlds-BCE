# Crossworlds BCE — Code Review (2026-07-04)

**Mode:** review-only. Nothing in `Assets/**` or gameplay code was changed. Findings are
notated here to plan around. Static review only — no Unity compile/run was performed, so
nothing is build-verified.

**Context:** this follows several fix passes on 2026-07-03 (boss controllers, professions,
combat polish, missing `BossArenaTrigger`, orphaned `.meta` files, `ProfessionManager`
bootstrap, and the prepared VPS craft-fix). Items fixed then are **not** re-listed as bugs;
what remains below is what those passes did *not* cover.

---

## Bug & Quality Findings (most severe first)

### 1. The new professions crafting UI is unreachable — two crafting UIs, only the legacy one is wired
- **Files:** `Assets/Game/Networking/ForgeNPC.cs:82-83` opens `CraftingUI.Instance.Open(professionId)`; `Assets/Game/UI/ForgeCraftingPanel.cs` (Smelt/Craft tabs, `/api/professions/recipes/:id`, craft-time progress bar) has **no caller** — nothing invokes `ForgeCraftingPanel.Open()`.
- **Failure scenario:** a player walks to the Forge Master and presses E → the *legacy* `CraftingUI` opens (old `GET /api/recipes?profession=` flow). The entire smelt→craft two-stage loop built for the professions feature — the reason `POST /api/craft` was extended and `craft_time_seconds` added — is never seen in-game. The feature is effectively dead on the client.
- **Suggested fix (to plan):** decide which panel is canonical. If `ForgeCraftingPanel` is the intended one, wire `ForgeNPC.OpenCrafting()` to it and retire `CraftingUI`; otherwise delete `ForgeCraftingPanel` to remove the dead duplicate. Do not keep both.

### 2. Inventory slot cap disagrees across three layers → items can land in invisible slots
- **Files:** `Assets/Game/Systems/InventoryManager.cs:23` `MaxSlots = 32` (and `FindNextFreeSlot` scans 0–31); `Assets/Game/UI/InventoryBagUI.cs:38-40` renders a 4×6 = **24**-slot grid and clamps `slot_index < TOTAL_SLOTS` (`:113`); the server `add-item` and `craft` handlers cap at **24**.
- **Failure scenario:** client-side pickup logic (`OnItemPickedUp`) fills a 25th–32nd slot; the bag UI silently drops any `slot_index >= 24`, so the item exists in memory/DB but is invisible and unequippable. Client and server also disagree on when the bag is "full."
- **Suggested fix (to plan):** pick one capacity (24 matches the bag UI and the server) and use it everywhere — change `InventoryManager.MaxSlots` to a single shared constant.

### 3. Crafted consumables can never be used — `ConsumableEffect.Apply` has no caller
- **Files:** `Assets/Game/Systems/ConsumableEffect.cs` (effect table + runner) — grep shows **zero** runtime call sites.
- **Failure scenario:** a player crafts `flask_void_resist` or `kit_iron_warden` (explicitly designed as craft-exclusive boss counterplay), opens the bag, clicks "use" … and nothing happens, because no inventory/hotbar path calls `ConsumableEffect.Apply(itemId, player)`. The exclusive crafted items — a core payoff of the professions economy — do nothing.
- **Note:** also interacts with the tracked damage-reduction single-channel limitation (resist flasks would clobber Siege Mode / Threat Protocol). Wire the caller and the DR refactor together.

### 4. Two parallel progression systems with unclear canon (character level vs gear/mastery)
- **Files:** `Assets/Game/Items/Scripts/CharacterStats.cs:4` states *"DESIGN PILLAR: no leveling — every bonus comes from equipped gear"*, yet `Assets/Game/UI/PlayerProgressManager.cs:95` `AwardXp` accrues character level/experience (from kills via `EnemyController`), and the `characters` table carries `level`/`experience`.
- **Failure scenario:** not a crash — a design ambiguity that leaks into UX. Character level visibly climbs but grants no power (gear/mastery do), so players chase a number that does nothing, or a future contributor wires power to it and breaks the gear pillar.
- **Suggested fix (to plan):** decide whether character level is retired, cosmetic (title/flavor only), or repurposed — and document it in one place so the two systems don't drift.

### 5. `AfkStationBuilder` still spawns a redundant `ProfessionManager` (low)
- **File:** `Assets/Game/Editor/AfkStationBuilder.cs:111` `go.AddComponent<ProfessionManager>()`.
- Since the 2026-07-03 fix, `ProfessionManager` self-bootstraps (`RuntimeInitializeOnLoadMethod`). The editor-placed instance is now redundant; the singleton guard destroys the duplicate, so it's harmless but misleading (and its log line "calls Load() on enable — wire that to your hub flow" is now inaccurate).
- **Suggested fix (to plan):** drop the `AddComponent` and update the log, so setup docs match reality.

---

## Improvement Opportunities

Framing per the standing guidance: **keep systems clean and easy to navigate.** Several
items below are less "new features" than "collapse duplicate/dead paths so the codebase
stays legible." Every idea cites real files. Nothing here was implemented.

### Quick wins
- **Collapse the duplicate crafting UI** `[impact: high / effort: S]` — resolves Finding #1 and removes a whole dead class. One forge, one panel, one recipe endpoint. Biggest legibility win available.
- **One inventory-capacity constant** `[med / S]` — resolves #2; a single `const` referenced by `InventoryManager`, `InventoryBagUI`, and mirrored in the server cap. Removes a class of "lost item" bugs.
- **Delete the redundant `ProfessionManager` placement + stale log** `[low / S]` — resolves #5; keeps the BCE setup menu honest.
- **A single `#if !UNITY_SERVER` audit checklist in CLAUDE.md** `[low / S]` — the recurring bug class this session was client-only types referenced from server-compiled code. A short "before adding a client singleton" checklist would prevent repeats.

### Bigger bets
- **Wire consumables end-to-end + ref-count the DR channel** `[high / M]` — resolves #3 and the tracked damage-reduction limitation together: a hotbar/inventory "use" action → `ConsumableEffect.Apply`, and a `Health` DR model that sums/ref-counts its drivers (PassiveThreatProtocol, SiegeModeHandler, resist flasks) instead of last-write-wins. Unlocks the craft-exclusive boss counterplay the economy is built around.
- **Deepen the state-layer payoff loop** `[high / L]` — the DoT/HoT/transition design (`StatusEffectManager`, `README` Combat System) is rich on paper but transition states depend on wiring that may not all exist yet. Audit which transitions actually fire in code vs. design, and close the gaps so cross-class synergies are real, not aspirational.
- **Server-authoritative player HP** `[high / L]` — `_CONTEXT` notes player HP is client-trusted today; combat kill rewards are server-gated but damage is not. A server HP model would close the largest exploit surface. Fits the server-authority pillar; sized as its own project.
- **Clarify progression canon** `[med / M]` — the design decision behind #4, then make code + UI reflect it.

---

## Clean areas (checked, found solid)
- Boss controllers (`IronWardenController`, `WorldBossController`, `SiegeTurretBehaviour`) after the 2026-07-03 pass: Health-event-driven phases, networked turret spawns, `[ClientRpc]` pull, immunity handling — consistent with Mirror discipline.
- `StatusEffectManager` core (effect add/refresh/expiry, DoT tick batching) and `Health` (shield/redirect/absorption ordering) read correctly.
- `.meta` coverage: no tracked `.cs` is missing its `.meta`, no orphans (verified 2026-07-03).
- `AfkGatheringStation` / `GatheringHUD` stop-flow and `ProfessionManager` bootstrap/load path are sound after the recent fixes.

---

## Known-tracked (not re-reported)
- Damage-reduction single shared scalar (needs ref-counted refactor — see Bigger Bets).
- Existing enemy/boss `.glb` committed as raw blobs before `*.glb` LFS pin (needs history migration).
- `_CONTEXT/professions-craft-fix.js` + `tools/deploy-craft-fix.sh` prepared but pending VPS deploy.
