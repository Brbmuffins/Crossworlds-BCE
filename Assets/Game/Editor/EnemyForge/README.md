# Enemy Forge

Selecting a source automatically infers its archetype, establishes complete archetype defaults, and then imports existing values from `Health`, `NavMeshAgent`, `EnemyController`, and `EnemyHeavyAttack`. Existing prefab customization wins; defaults are used only where source combat data is absent. The import changes only the authoring definition, never the source prefab.

**Restore Defaults** is an explicit, confirmed reset that replaces imported/customized health, movement, attack, and heavy-attack values with the currently selected archetype preset.

In Validation & Build, **Validate** appears before **Restore Defaults**, and **Save As New** performs the generated-prefab save workflow.

The Validation & Build section is permanently expanded and pinned beneath the scrolling authoring sections, so validation, save, deploy, and revert controls remain visible while editing long configurations.

Reopening or dragging a previously forged prefab automatically restores that prefab as the Deployment Target. Validation checks both the current definition and that exact prefab; changing the target invalidates the prior validation so Deploy can never act on an unvalidated asset.

Enemy Forge is network-only. Every generated prefab disables offline simulation, uses server-authoritative Mirror components, and is registered for runtime spawning.

Every Save As New and Deploy stamps the current Enemy Forge runtime profile onto the prefab. The profile reapplies authority mode, NavMesh grounding values, root-motion protection, combat/lifecycle settings, and animation overrides. Validation warns when an older forged prefab needs a one-time Deploy migration; shared runtime protections also suppress missing Get Hit states and reject dead targets for every forged enemy.

The former Update Matching World Instances checkbox is no longer shown or applied during save. **Deploy** is an explicit, validation-gated, centered final action in Validation & Build. After an OK/Cancel warning, it updates the current forged prefab in place and applies the same configuration to matching root instances in loaded scenes. Scene-instance changes support Undo; the original source model is untouched.

**Save As New** and **Deploy** are validation-gated. They remain disabled until Validate completes without blocking errors. Any subsequent definition or animation-mapping change invalidates that result and requires another validation pass.

**Revert Changes** offers two notified recovery paths. Revert Unsaved Settings restores the authoring definition baseline and requires validation again. Roll Back Last Deploy restores the prefab, animation override, and saved loaded scenes from the pre-deploy backup stored under `Library/EnemyForgeBackups/LastDeploy`; this backup is local and Git-ignored. Deploy requires saving modified scenes before creating this backup.

Every major authoring area is an independent foldout. Prefab Source begins expanded for immediate drag-and-drop; all other sections begin collapsed.

The Prefab Source section displays Unity's generated preview image for the selected prefab or imported model, with an asset-thumbnail fallback when a full preview is unavailable.

When animation states are mapped, the source preview renders an isolated, script-disabled copy of the character and cycles through each assigned state. The preview includes play/pause, manual state selection, and a state progress bar; it does not alter the source prefab or animation clips.

## Network deployment

Newly built prefab roots are normalized to zero position and rotation so dragging them into a scene does not retain an imported world position. The deployment panel can validate a prefab, place it near the selected object or Scene view pivot (snapping to a nearby baked NavMesh), and replace a selected scene object with full Undo support.

Save As New and Deploy configure `NetworkIdentity`, server-authoritative `NetworkTransformUnreliable`, and server-authoritative `NetworkAnimator`, then add the forged prefab to both `RodNetworkManager.worldPrefabs` and Mirror `spawnPrefabs` in `LoginScene`. This is a Git-visible project change. The Enemy Forge editor code remains local-only.

Before entering Play mode, Enemy Forge warns when loaded scenes contain network enemies but no `RodNetworkManager`. Placement reminds the user to save the changed scene and test through LoginScene using the normal server flow.

Local Unity Editor tooling for turning an imported model or prefab into a Crossworlds enemy prefab.

Open **BCE > Enemy Forge**, then drag a prefab or imported model onto the drop area. You can also create a saved definition first. Validate and build when ready. The source asset is never modified. Generated prefabs use the existing `Health`, `EnemyController`, `EnemyHeavyAttack`, Mirror `NetworkIdentity`, and `NavMeshAgent` components.

Generated enemies default to the source asset's folder. If the target prefab already exists, Enemy Forge asks whether to replace it, save with a new unique name, or cancel. Enable **Update Matching World Instances** to apply the same settings to matching root prefab instances in currently loaded scenes.

Enemy Forge locally fingerprints its required combat scripts using Unity GUIDs and dependency hashes, and snapshots their serialized contracts. On a change it reports added, removed, and type-changed requirements. Additive fields pass automatically and retain their component defaults. Removed fields, changed types, and missing sources keep the tool locked until its adapter is updated. Acknowledgement is available only after the compatibility audit passes. Accepted contracts are kept in local Editor preferences and are not committed.

The Animation State Mapping panel searches corresponding `Assets/Game/3D Models/**/Animation` folders and lists only clips embedded in `.fbx` files. It walks upward from the selected prefab and uses the nearest model-family directory containing an `Animation` child; it never falls back to an unrelated model folder. Changing the source prefab clears the old model's mappings before resolving the new folder. Manual folder and file browsing enforce the same location and `.fbx` requirement. Each Idle, Chase, Attack, Get Hit, and Death state has a clip dropdown plus a file browser. In the recommended Enemy Forge Standard mode, building uses the protected shared `Assets/Game/Data/EnemyForge/EnemyForge_Base.controller` and creates a prefab-specific `_Animations.overrideController` beside the generated prefab. The shared controller owns Speed, Attack, GetHit, and Death transitions; the override changes only clips.

Dropdown labels use `FBX filename — embedded clip name`, so Mixamo files remain distinguishable even when every embedded take is named `mixamo.com`.

Driver modes:
- Enemy Forge Standard: disables recognized legacy AI and animation writers only on the generated copy and assigns the override controller.
- Existing Model Driver: disables legacy AI, preserves the model's animation driver/controller, and does not apply clip mappings.
- Hybrid Override: disables legacy AI, keeps the model animation driver, and assigns the override controller; validation warns about competing Animator writes.

Animation validation runs for every Enemy Forge definition. It verifies looping expectations, usable duration, and Mecanim compatibility for each assigned clip. Enemy Forge repairs its shared controller whenever a prefab override is generated: Write Defaults is disabled, transitions use normalized blending, Death has priority, and Attack/Get Hit return directly to the correct Idle or Chase state. Source FBX import settings are reported but never silently changed because an FBX may be shared by unrelated prefabs.

When a selected prefab already uses an Enemy Forge `AnimatorOverrideController`, its existing Idle, Chase, Attack, Get Hit, and Death replacements are loaded into the matching dropdowns. Animation-folder discovery remains the fallback for sources without an associated override.

Override mappings are written explicitly against Enemy Forge's five placeholder clips and validated after saving. An existing empty override is reported as a repairable warning, allowing Deploy to rebuild it from the current mappings. NavMeshAgent Base Offset is an explicit Movement & Perception setting (default `0`) rather than being guessed from animated renderer bounds, preventing forged enemies from floating or sinking as poses change.

Enemy Forge always prepares generated prefabs for Mirror server play and maintains their LoginScene spawn registration.
