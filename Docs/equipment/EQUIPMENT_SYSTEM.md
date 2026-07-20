# Equipment & Slot System — Crossworlds BCE

Server-authoritative, networked equipment: what a player has equipped is shared state
that every client (and every late joiner) sees, driving a cosmetic world model on each
hero's rig. Stats come from the gear itself via `CharacterStats`; this system owns the
**shared net + visual state and the slot model**.

---

## Components

| Script | Type | Role |
|---|---|---|
| `PlayerEquipment` | `NetworkBehaviour` (on player) | Owns a `SyncDictionary<int,string>` of slot → `serverItemId`. Server-authoritative mutations; owner sends `[Command]`s; each client attaches/removes the item's world model on the matching rig mount. |
| `EquipmentRig` | `MonoBehaviour` (on every hero prefab) | Exposes the attach `Transform` for each slot (`headMount`, `chestMount`, `feetMount`, `handsMount`, `mainHandMount`, `offHandMount`). Assign these to rig bones in the editor. A null mount = no visual for that slot (item still equips + applies stats). |
| `EquipmentCatalog` | `ScriptableObject` at `Resources/EquipmentCatalog` | Registry of every equippable `ItemData`, keyed by `serverItemId`. Loaded by **both** the dedicated server (slot validation via `GetSlot`) and clients (model + stats). Server never touches `worldModelPrefab`. |
| `EquipmentSetupBuilder` | Editor tool (`BCE/Setup/Equipment`) | Generates the sneaker upgrade items + a base loadout for all 5 classes (5 × 6 slots), collects them into `Resources/EquipmentCatalog.asset`. Safe to re-run. |

## Slots (`EquipmentSlotType`, in `ItemData.cs`)

`Head, Chest, Feet, Hands, MainHand, OffHand` — plus legacy aliases `Weapon` (→ MainHand)
and `Legs` kept for the old local prototype. `MainHand` = sword/primary (right hand),
`OffHand` = shield/off-hand (left hand/forearm).

---

## Data flow (equip)

```
owner client                     server                         all clients
────────────                     ──────                         ───────────
RequestEquip(serverItemId)
  └─ CmdEquip ───────────────▶  ServerEquip:
                                  catalog.GetSlot(id) validates
                                  _equipped[(int)slot] = id  ──▶  SyncDictionary replicates
                                                                  OnAdd/OnSet/OnRemove →
                                                                  RefreshVisual: Instantiate
                                                                  worldModelPrefab under
                                                                  EquipmentRig.GetMount(slot)
```

- **Late joiners** get the full `_equipped` dictionary in the spawn payload; `OnStartClient`
  renders whatever is already equipped.
- **Persistence** is separate: `InventoryManager` writes equip state to the auth DB
  (`POST /api/inventory/equip`) on the owning client. On spawn, `PlayerEquipment`
  re-applies whatever the DB says is equipped once `InventoryManager` has loaded.

## Server ↔ DB contract

`serverItemId` on each `ItemData` **must** match `items.id` in the auth DB. Keep the catalog
in lockstep with:
- [`_CONTEXT/equipment-items.sql`](../../_CONTEXT/equipment-items.sql) — the item seed rows.
- [`_CONTEXT/EQUIPMENT_VPS_HANDOFF.md`](../../_CONTEXT/EQUIPMENT_VPS_HANDOFF.md) — the server/DB handoff.

## Mirror discipline

- All `_equipped` mutations are inside `[Server]` methods; the owner requests changes via
  `[Command]`s only.
- Visual instantiation is guarded `#if UNITY_EDITOR || !UNITY_SERVER` — the dedicated server
  never loads models. (Not `!UNITY_SERVER` alone — that would strip it from the editor's DS
  build target too.)
- World models are spawned **locally** on each client (plain `Instantiate`, not
  `NetworkServer.Spawn`): no extra `NetworkIdentity`, and everyone stays in sync because the
  driving `SyncDictionary` is replicated. World-model prefabs must **not** carry a
  `NetworkIdentity`.

---

## Editor setup (required — CLI can't do these)

1. Ensure `Resources/EquipmentCatalog.asset` exists (run `BCE/Setup/Equipment` to generate it,
   or the system logs a warning and disables visuals + server slot validation).
2. On each hero prefab, add `EquipmentRig` and assign the six mount transforms to the rig bones.
3. Seed the matching `items` rows on the VPS from `_CONTEXT/equipment-items.sql`.

## Where it lives in git

The scripts, `Resources/EquipmentCatalog.asset`, the item defs under
`Assets/Game/Items/Definitions/`, the models/icons under `3D Models/Equipment/` and
`UI/Icons/Equipment/`, and `_CONTEXT/equipment-items.sql` were committed in
`Equipment Slots / manager`. This document is the reference for that system. If the
`EquipmentCatalog` asset is ever missing at runtime the scripts compile and no-op
gracefully (they log a warning and skip visuals + server slot validation).
