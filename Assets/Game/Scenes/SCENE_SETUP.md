# Null Architect Boss Arena — Scene Setup Guide

**Scene:** `Assets/Game/Scenes/VoidDungeon.unity`  
**Status:** Placeholder → Playable after completing all steps below.

---

## 0. Before You Start

Confirm the following are already in the project (check Project window):

| Asset | Path | Status |
|-------|------|--------|
| WorldBossController.cs | Assets/Game/Combat/Scripts/ | ✅ live |
| NullArchitectArenaStarter.cs | Assets/Game/Combat/Scripts/ | ✅ live (new) |
| NullArchitectRoomBuilder.cs | Assets/Game/Editor/ | ✅ live (new) |
| WorldBossBuilder.cs | Assets/Game/Editor/ | ✅ live |
| WorldBossHealthBar.cs | Assets/Game/UI/ | ✅ live |

---

## 1. Generate Missing Assets (run once)

Run both scripts from the repo root.  Keys are read from env — never hardcoded.

```powershell
# Set keys (PowerShell)
$env:TRIPO_API_KEY  = "tsk_..."    # from TRIPO_API_KEY env var — not stored here
$env:GEMINI_API_KEY = "AQ..."     # from GEMINI_API_KEY env var — not stored here

# Generate 3D set-dressing (cathedral pillars, altar, crystals)
python Tools/generate_null_architect_assets_tripo.py

# Generate 2D textures (fog sheets, telegraph decals, rune glyphs)
python Tools/generate_null_architect_textures_gemini.py
```

**Outputs:**
- `Assets/Game/3d Assets/NullArchitect/` — GLB meshes + `MANIFEST.txt`
- `Assets/Game/Textures/NullArchitect/` — PNG textures + `MANIFEST.txt`

**Import in Unity:** drag generated files into their output folders in the Project window.  
Do NOT add `NetworkIdentity` to any generated mesh or material — all cosmetic.

---

## 2. Build the Room

In Unity, with VoidDungeon.unity open (or closed — builder opens it):

```
BCE/Setup/10a ▶ Build Null Architect Room (VoidDungeon)
```

This builder deterministically places:
- Render settings (thick violet fog, near-zero ambient, no skybox)
- 1 faint directional void-moon light
- 8 god-ray spot lights + 8 floor rune point lights + 1 boss halo
- Arena floor (80×80 rune-stone, NavMeshSurface component)
- 8 broken cathedral pillars (r = 38u, varying heights 8–18u)
- 4 partial arch ribs
- 10 floor debris chunks
- 6 void seam tear strips + 3 ceiling rifts (emissive + particle emitters)
- Drifting void particulate dome (ParticleSystem)
- 4 rune-floor glow emitters
- 4 `FX_LightPillar` god-ray prefab instances
- 2 `Glowing orbs` ambient prefab instances
- Invisible boundary walls
- BossSpawnMarker + 3 ShardAnchor reference empties
- 4 PlayerSpawn (NetworkStartPosition)
- `NullArchitectArenaStarter` (NetworkIdentity — server-spawns boss)
- `ArenaSessionController` (NetworkIdentity)
- `RodChatManager` (NetworkIdentity)
- Return portal (HubReturnTrigger to Hub)
- Main camera
- Saves scene, registers in Build Settings

---

## 3. Create the Boss Prefab

```
BCE/Setup/6 ▶ Create World Boss (Null Architect)
```

This creates a `NullArchitect_Boss` GameObject in the active scene with:
- Capsule placeholder mesh (3× scale)
- `NetworkIdentity`, `NavMeshAgent`, `Health` (2000 HP)
- `WorldBossController` with all numeric fields pre-set
- `BossTrigger` child (SphereCollider, r = 15u)
- `VoidDrainVFX` child (inactive placeholder)
- Point light child

**Save as prefab:**

1. Drag `NullArchitect_Boss` from Hierarchy → `Assets/Game/Prefabs/NullArchitect_Boss.prefab`
2. Delete the instance from the scene hierarchy (it must NOT live directly in VoidDungeon.unity)
3. Confirm: the Hierarchy shows no `NullArchitect_Boss` — only `NullArchitectArenaStarter`

---

## 4. Inspector Assignments (required before first play)

### 4a. NullArchitectArenaStarter

| Field | Value |
|-------|-------|
| Boss Prefab | `Assets/Game/Prefabs/NullArchitect_Boss.prefab` |
| Spawn Delay | `3` (seconds — players see room before boss materialises) |

### 4b. WorldBossController (on the boss prefab)

Open `Assets/Game/Prefabs/NullArchitect_Boss.prefab` in Prefab Edit mode.

| Field | Asset to assign | Notes |
|-------|----------------|-------|
| `reflectTelegraphVFX` | `brbmuffins Fantasy Pack/Prefabs/Effects normal/Death magic circle.prefab` | Phase 1 reflect warning — tint cyan-violet in its material |
| `voidDrainVFX` | `brbmuffins Magic Pack/Prefabs/Environment/Crystal effect blue.prefab` | Phase 3 drain zone — tint purple in material; already set as child |
| `transitionVFXPrefab` | `brbmuffins Magic Pack/Prefabs/AoE effects/Smoke AOE explosion.prefab` | Phase transition burst |
| `deathVFXPrefab` | `brbmuffins Magic Pack/Prefabs/AoE effects/Red energy explosion.prefab` | Boss death collapse |
| `tetherLinePrefab` | (leave null for inline LineRenderer) | Optional — null = code creates it |
| `nullShardPrefab` | `Assets/Game/Prefabs/NullShard.prefab` | Auto-built by WorldBossBuilder |
| `worldItemPrefab` | `Assets/Game/Prefabs/WorldItem.prefab` | Loot drop prefab |

### 4c. Generated texture assignments (optional — enhances visuals)

| Texture | Apply to |
|---------|---------|
| `rune_glyph_floor.png` | `M_RuneSeamEmissive` material → Emission Map |
| `telegraph_decal_reflect.png` | Death magic circle material → Albedo |
| `telegraph_decal_drain.png` | Crystal effect blue material → Albedo |
| `fog_sheet_01.png` | VoidParticulateDome ParticleSystem → Renderer material |
| `void_seam_crack.png` | `M_VoidSeam_*` materials → Emission Map |

### V1. URP Volume (GlobalVolume_VoidCathedral — optional but high-impact)

1. Create a URP `VolumeProfile` asset: `Assets/Game/Settings/NullArchitectVolumeProfile.asset`
2. Assign to the `GlobalVolume_VoidCathedral` scene object → Profile field
3. Add overrides:
   - **Fog** → Enable, Color `(0.13, 0.03, 0.20)`, Density Multiplier `2.5`, Max Fog Distance `80`
   - **Bloom** → Enable, Intensity `0.4`, Scatter `0.7`, Tint purple `(0.8, 0.5, 1.0)`
   - **Color Adjustments** → Saturation `-15`, Post Exposure `-0.5`
   - **Vignette** → Enable, Intensity `0.35`, Smoothness `0.4`, Color `(0.1, 0.0, 0.15)`

---

## 5. NetworkManager Registration

Open the Login scene → Select `RodNetworkManager`:

Add to **Registered Spawnable Prefabs**:
- `Assets/Game/Prefabs/NullArchitect_Boss.prefab`
- `Assets/Game/Prefabs/NullShard.prefab`
- `Assets/Game/Prefabs/WorldItem.prefab` (if not already present)

---

## 6. Bake NavMesh

```
Window → AI → Navigation → Bake
```

Make sure `ArenaFloor` (the 80×80 plane) is selected and **Static → Navigation Static** is checked.

NavMesh bake settings recommended:
| Setting | Value |
|---------|-------|
| Agent Radius | 0.5 |
| Agent Height | 2.0 |
| Max Slope | 45° |
| Step Height | 0.4 |

---

## 7. Boss Scale + Model Replacement

The boss starts as a 2.5×3×2.5 capsule — enormous and wrong-shaped.

**Replace with real model** (if available at `Docs/models/null-architect/null-architect.glb`):
1. Import GLB → ensure it has no `NetworkIdentity`
2. In Prefab Edit mode on `NullArchitect_Boss`: delete capsule MeshFilter/MeshRenderer
3. Drop model as child of `NullArchitect_Boss`, reset local position to (0, 0, 0)
4. Add `VFX_Trail_Void.prefab` as child of boss root for ambient void trail

**Scale guidance:** The boss should loom at roughly 5–6m effective height at the boss origin point (y = 0). If using the GLB from Docs, set model scale to approximately 2.5× if it imports at 1m scale.

---

## 8. Phase VFX Colour Matching (art direction lock)

The phase colour arc must read THROUGH the purple base fog:

| Phase | Boss light colour | reflectTelegraphVFX tint | HP bar colour |
|-------|-----------------|--------------------------|---------------|
| Phase 1 | `(0.4, 0.7, 1.0)` cyan-violet | Cyan-violet | Cyan |
| Phase 2 | `(1.0, 0.5, 0.1)` orange-into-purple | Orange | Orange |
| Phase 3 | `(0.8, 0.1, 0.1)` deep red | Deep red | Red |
| Transition/immune | `(1.0, 1.0, 1.0)` white | — | White |

These are already set in `WorldBossHealthBar.cs` (phase-synced via `OnPhaseSync`).  
The boss point light (`BossHalo_Point`) can be animated per-phase by extending `RpcShowTransitionVFX` in `WorldBossController.cs`.

---

## 9. Build Settings

VoidDungeon is auto-added by the builder. Verify in:
```
File → Build Settings → Scenes In Build
```
Expected order:
- 0: Assets/Game/Scenes/LoginScene.unity
- 1: Assets/Game/Scenes/CharacterSelect.unity
- 2: Assets/Game/Scenes/Hub.unity
- 3: Assets/Game/Scenes/Arena_Copper.unity  (if built)
- 4: Assets/Game/Scenes/VoidDungeon.unity

---

## 10. Checklist — Remaining Un-Scriptable Unity Clicks

The following cannot be done by a CLI agent and must be completed in the Unity editor.  
Work through them in order.

| # | Step | Where |
|---|------|--------|
| 1 | Run `BCE/Setup/10a` builder | BCE menu |
| 2 | Run `BCE/Setup/6` to create boss GO | BCE menu |
| 3 | Drag boss GO to `Assets/Game/Prefabs/NullArchitect_Boss.prefab` | Hierarchy → Project |
| 4 | Delete boss from Hierarchy (must NOT be in scene) | Hierarchy |
| 5 | Open boss prefab; assign `reflectTelegraphVFX` | Inspector |
| 6 | Open boss prefab; assign `voidDrainVFX` | Inspector |
| 7 | Open boss prefab; assign `transitionVFXPrefab` | Inspector |
| 8 | Open boss prefab; assign `deathVFXPrefab` | Inspector |
| 9 | Open boss prefab; assign `nullShardPrefab` | Inspector |
| 10 | Open boss prefab; assign `worldItemPrefab` | Inspector |
| 11 | Select `NullArchitectArenaStarter`; assign `Boss Prefab` | Inspector |
| 12 | RodNetworkManager → spawnPrefabs: add Boss + Shard + WorldItem | Inspector |
| 13 | Mark `ArenaFloor` Navigation Static | Inspector → Static dropdown |
| 14 | Bake NavMesh | Window → AI → Navigation → Bake |
| 15 | Create URP VolumeProfile, assign to GlobalVolume_VoidCathedral, add overrides (§ V1) | Inspector |
| 16 | Replace capsule with real boss model (if GLB available) | Prefab Edit mode |
| 17 | Attach `VFX_Trail_Void` as child of boss prefab root | Prefab Edit mode |
| 18 | Ctrl+S — save VoidDungeon.unity | File |
| 19 | Confirm VoidDungeon is in Build Settings at correct index | File → Build Settings |
| 20 | Server rebuild + deploy after all prefab changes | `tools/build-server.ps1` |

---

## Asset Manifest Summary

| Source | Asset | Role |
|--------|-------|------|
| **Reused** | `Death magic circle.prefab` | reflectTelegraphVFX |
| **Reused** | `Smoke AOE explosion.prefab` | transitionVFXPrefab |
| **Reused** | `Red energy explosion.prefab` | deathVFXPrefab |
| **Reused** | `Crystal effect blue.prefab` | voidDrainVFX zone |
| **Reused** | `FX_LightPillar.prefab` | God-ray pillars (×4 in scene) |
| **Reused** | `VFX_Trail_Void.prefab` | Boss ambient trail |
| **Reused** | `Glowing orbs.prefab` | Ceiling ambient (×2 in scene) |
| **Reused** | `Magic circle.prefab` | Reflect floor decal |
| **Reused** | `Charge slash purple.prefab` | Reflect pulse ring |
| **Generated (Tripo)** | `broken_cathedral_pillar.glb` | Pillar set-dressing |
| **Generated (Tripo)** | `rune_stone_altar.glb` | Altar prop |
| **Generated (Tripo)** | `void_crystal_shard.glb` | Shard scatter prop |
| **Generated (Tripo)** | `altar_fragment_a/b.glb` | Floor debris |
| **Generated (Gemini)** | `rune_glyph_floor.png` | Floor emissive map |
| **Generated (Gemini)** | `telegraph_decal_reflect.png` | Reflect decal |
| **Generated (Gemini)** | `telegraph_decal_drain.png` | Drain decal |
| **Generated (Gemini)** | `fog_sheet_01/02.png` | Particle fog sprites |
| **Generated (Gemini)** | `void_seam_crack.png` | Seam emissive map |
| **Generated (Gemini)** | `void_texture_tile.png` | Void background tile |

Full per-asset manifests: `MANIFEST.txt` in each output folder.

---

*Last updated by NullArchitectRoomBuilder — 2026-07-05*
