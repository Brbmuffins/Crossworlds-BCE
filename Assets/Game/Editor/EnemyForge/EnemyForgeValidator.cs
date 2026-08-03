#if UNITY_EDITOR
using System.Collections.Generic;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Crossworlds.EditorTools.EnemyForge
{
    internal enum EnemyForgeSeverity { Info, Warning, Error }

    internal readonly struct EnemyForgeIssue
    {
        public readonly EnemyForgeSeverity severity;
        public readonly string message;
        public EnemyForgeIssue(EnemyForgeSeverity severity, string message)
        { this.severity = severity; this.message = message; }
    }

    internal static class EnemyForgeValidator
    {
        public static List<EnemyForgeIssue> ValidateDefinition(EnemyForgeDefinition d)
        {
            var issues = new List<EnemyForgeIssue>();
            if (d == null) { issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Create or select a definition.")); return issues; }
            if (d.source == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "A source model or prefab is required."));
            if (d.rootTag == EnemyForgeRootTag.Player)
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning,
                    "Player tag selected: the prefab still uses EnemyController combat behavior, but other enemies may recognize it as a player target."));
            if (string.IsNullOrWhiteSpace(d.templateId)) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Template ID is required."));
            if (d.enemyLevel < 0) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Enemy Level cannot be negative."));
            if (string.IsNullOrWhiteSpace(d.outputFolder) || !d.outputFolder.StartsWith("Assets/")) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Output folder must be inside Assets/."));
            if (d.attackRange < d.stoppingDistance) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "Attack range is smaller than the NavMesh stopping distance."));
            if (d.stoppingDistance < d.agentRadius)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Warning,
                    "Combat spacing is smaller than the agent radius. Increase Stopping Distance to reduce overlap or stacking on players."));
            float sourceFootprintRadius = EstimateHorizontalFootprintRadius(d.source);
            if (sourceFootprintRadius > 0f && d.agentRadius < sourceFootprintRadius * 0.4f)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Warning,
                    $"NavMesh Agent Radius ({d.agentRadius:0.00}) is small for the model's approximate " +
                    $"{sourceFootprintRadius:0.00} footprint radius. Increase it to keep roaming mobs from visually overlapping."));
            if (d.leashRadius < d.aggroRadius) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "Leash radius is smaller than aggro radius."));
            if (d.enableRoaming && d.roamingRadius <= 0f) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Roaming radius must be greater than zero when roaming is enabled."));
            if (d.enableRoaming && d.roamingRadius > d.leashRadius) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Roaming radius cannot exceed the combat leash radius."));
            if (d.roamingMaxWait < d.roamingMinWait) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Roaming maximum wait must be at least the minimum wait."));
            if (d.IsRanged && d.projectilePrefab == null)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Info,
                    "No custom projectile is selected. Enemy Forge will assign its network-ready ranged test projectile."));
            if (d.dropTable != null && d.worldItemPrefab == null)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Error,
                    "A Drop Table requires a Loot Visual. Assign any prefab visual (bag, sword, chest, or other model); Enemy Forge will create and register its network-ready pickup wrapper."));
            if (d.IsRanged && d.attackRange < d.preferredRange + Mathf.Max(0.5f, d.agentRadius))
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Warning,
                    "Basic Attack Range does not reach the preferred ranged position. Enemy Forge will expand the generated prefab's attack envelope so it cannot stall after acquiring a target."));
            if (d.heavyMaxCooldown < d.heavyMinCooldown) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Heavy maximum cooldown must be at least its minimum."));
            if (d.IsRanged && d.rangedCastDistance > d.aggroRadius)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Warning,
                    "Cast Distance to Target exceeds Aggro Radius, so the enemy cannot acquire targets at its full casting distance."));
            if (d.IsRanged && d.rangedCastDistance < d.preferredRange)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Warning,
                    "Cast Distance to Target is smaller than Preferred Range, so cast attacks may not begin from the enemy's preferred position."));
            float longestAttack = LongestAttackLength(d);
            bool requiresFullAttackCycle = d.IsRanged || d.attackAnimation2 != null ||
                d.attackAnimation3 != null || d.attackAnimation4 != null;
            if (requiresFullAttackCycle && longestAttack > 0f && d.attackInterval < longestAttack)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Info,
                    $"The configured attack interval is shorter than the longest selected attack animation ({longestAttack:0.00} seconds). Enemy Forge will use the full clip duration as this prefab's minimum attack cycle."));
            if (d.IsRanged && d.castImmediatelyOnAggro && !d.addHeavyAttack)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Error,
                    "Cast Immediately on Aggro requires Enable Ranged Attack."));
            if (d.IsRanged && d.castImmediatelyOnAggro && d.rangedCastDistance < d.aggroRadius)
                issues.Add(new EnemyForgeIssue(
                    EnemyForgeSeverity.Info,
                    "Opening Cast range is smaller than Aggro Radius. Enemy Forge will expand the generated cast range to the Aggro Radius so the opening cast can begin immediately."));
            if (d.source != null && d.source.GetComponentInChildren<Renderer>(true) == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "The source has no renderer in its hierarchy."));
            if (d.generateAnimatorController && d.idleAnimation == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "Animator generation is enabled but no Idle animation is assigned."));
            if (d.generateAnimatorController && d.attackAnimation == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "No combat Attack animation is assigned."));
            if (d.attackAnimation == null &&
                (d.attackAnimation2 != null || d.attackAnimation3 != null || d.attackAnimation4 != null))
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                    "Combat Attack 1 is required before additional randomized attacks can be used."));
            if (d.generateAnimatorController && d.deathAnimation == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "No Death animation is assigned."));
            if (d.generateAnimatorController)
            {
                ValidateClip(d.idleAnimation, "Idle", true, issues);
                ValidateClip(d.chaseAnimation, "Chase / Movement", true, issues);
                ValidateClip(d.attackAnimation, "Combat Attack", false, issues);
                ValidateClip(d.attackAnimation2, "Combat Attack 2", false, issues);
                ValidateClip(d.attackAnimation3, "Combat Attack 3", false, issues);
                ValidateClip(d.attackAnimation4, "Combat Attack 4", false, issues);
                ValidateClip(d.getHitAnimation, "Get Hit", false, issues);
                ValidateClip(d.deathAnimation, "Death", false, issues);
            }
            if (d.animationDriverMode == EnemyForgeAnimationDriverMode.HybridOverride) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "Hybrid Override retains model animation drivers. They may write Speed, Attack, Die, or IsDead alongside EnemyController."));
            if (d.animationDriverMode == EnemyForgeAnimationDriverMode.ExistingModelDriver && d.source != null && !HasAnimationDriver(d.source)) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "Existing Model Driver mode is selected, but no recognized model animation driver was found."));
            if (d.source != null && CountActiveAiControllers(d.source) > 1) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "The source contains multiple AI controllers. Enemy Forge will disable legacy AI controllers on the generated copy."));
            return issues;
        }

        static float LongestAttackLength(EnemyForgeDefinition d)
        {
            float longest = d.attackAnimation != null ? d.attackAnimation.length : 0f;
            if (d.attackAnimation2 != null) longest = Mathf.Max(longest, d.attackAnimation2.length);
            if (d.attackAnimation3 != null) longest = Mathf.Max(longest, d.attackAnimation3.length);
            if (d.attackAnimation4 != null) longest = Mathf.Max(longest, d.attackAnimation4.length);
            return longest;
        }

        static void ValidateClip(AnimationClip clip, string stateName, bool shouldLoop,
            List<EnemyForgeIssue> issues)
        {
            if (clip == null) return;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime != shouldLoop)
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Info,
                    $"{stateName} clip '{clip.name}' has a different loop setting. Enemy Forge will normalize looping on a prefab-local clip without changing the source FBX."));
            if (clip.length <= 0.05f)
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                    $"{stateName} clip '{clip.name}' has no usable animation duration."));
            if (clip.legacy)
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                    $"{stateName} clip '{clip.name}' is Legacy and cannot be used reliably by the generated Mecanim controller."));
        }

        public static List<EnemyForgeIssue> ValidatePrefab(GameObject prefab)
        {
            var issues = new List<EnemyForgeIssue>();
            if (prefab == null) return issues;
            if (prefab.GetComponent<NetworkIdentity>() == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "NetworkIdentity is missing."));
            if (prefab.GetComponent<Health>() == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "Health is missing."));
            if (!prefab.CompareTag("Enemy") && !prefab.CompareTag("Player"))
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                    "The prefab root must use either the Enemy or Player tag. Deploy will apply the selected Enemy Forge root tag."));
            var enemyController = prefab.GetComponent<EnemyController>();
            if (enemyController == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "EnemyController is missing."));
            else if (enemyController.enemyForgeRuntimeProfileVersion < EnemyController.EnemyForgeRuntimeProfileVersion)
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning,
                    $"This prefab uses Enemy Forge runtime profile {enemyController.enemyForgeRuntimeProfileVersion}. Deploy once to upgrade it to profile {EnemyController.EnemyForgeRuntimeProfileVersion}."));
            if (enemyController != null && enemyController.isRanged)
            {
                GameObject projectile = enemyController.projectilePrefab;
                if (projectile == null)
                    issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "The ranged prefab has no projectile assigned. Deploy will add the Enemy Forge test projectile."));
                else
                {
                    if (projectile.GetComponent<EnemyProjectile>() == null)
                        issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "The assigned ranged projectile is missing EnemyProjectile."));
                    if (projectile.GetComponent<NetworkIdentity>() == null)
                        issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "The assigned ranged projectile is missing NetworkIdentity."));
                    var projectileCollider = projectile.GetComponent<Collider>();
                    if (projectileCollider == null || !projectileCollider.isTrigger)
                        issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "The assigned ranged projectile requires a trigger collider."));
                }
            }
            if (enemyController != null && enemyController.dropTable != null)
            {
                GameObject pickup = enemyController.worldItemPrefab;
                if (pickup == null)
                    issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                        "The prefab has a Drop Table but no generated loot pickup. Assign a Loot Visual and deploy again."));
                else
                {
                    if (pickup.GetComponent<WorldItem>() == null)
                        issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                            "The generated loot pickup is missing WorldItem."));
                    if (pickup.GetComponent<NetworkIdentity>() == null)
                        issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                            "The generated loot pickup is missing NetworkIdentity."));
                    var pickupCollider = pickup.GetComponent<Collider>();
                    if (pickupCollider == null || !pickupCollider.isTrigger)
                        issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                            "The generated loot pickup requires a trigger collider."));
                    if (!EnemyForgeDeployment.IsNetworkRegistered(pickup))
                        issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                            "The generated loot pickup is not registered with Mirror. Deploy the forged enemy again before building."));
                }
            }
            if (enemyController != null && enemyController.keepCorpseGrounded)
            {
                if (enemyController.corpseGroundingRenderer == null)
                    issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning,
                        "Corpse grounding has no explicit body renderer. Deploy will assign the main skinned body and exclude weapons/accessories."));
                else if (!enemyController.corpseGroundingRenderer.transform.IsChildOf(prefab.transform))
                    issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                        "The corpse grounding renderer does not belong to this prefab."));
            }
            if (prefab.GetComponent<NavMeshAgent>() == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error, "NavMeshAgent is missing."));
            if (!HasUsableRootCombatCollider(prefab))
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning,
                    "The Enemy/Player combat root needs an enabled, non-trigger collider beside Health. Deploy will add or repair the root combat collider; child model, weapon, and accessory colliders do not satisfy combat targeting."));
            var animator = prefab.GetComponentInChildren<Animator>(true);
            if (animator == null) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning, "No Animator was found; combat works, but animations will not play."));
            else if (animator.applyRootMotion && animator.runtimeAnimatorController is AnimatorOverrideController)
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Error,
                    "Animator Apply Root Motion must be disabled so animation clips cannot move the NavMesh-controlled prefab through the ground."));
            else if (animator.runtimeAnimatorController is AnimatorOverrideController overrideController)
            {
                var mappings = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(mappings);
                if (mappings.Count == 0 || mappings.TrueForAll(pair => pair.Value == null))
                    issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning,
                        "The assigned Animator Override Controller contains no animation replacements. Deploy will rebuild it from the current mappings."));
            }
            var path = AssetDatabase.GetAssetPath(prefab);
            var agent = prefab.GetComponent<NavMeshAgent>();
            if (agent != null && Mathf.Abs(agent.baseOffset) > agent.height)
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning,
                    "NavMeshAgent Base Offset exceeds the agent height and is likely to make the model float or sink."));
            float footprintRadius = EstimateHorizontalFootprintRadius(prefab);
            if (agent != null && footprintRadius > 0f && agent.radius < footprintRadius * 0.4f)
                issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Warning,
                    $"NavMeshAgent Radius ({agent.radius:0.00}) is small for the model's approximate " +
                    $"{footprintRadius:0.00} footprint radius. Roaming mobs may visually overlap."));
            if (string.IsNullOrEmpty(path)) issues.Add(new EnemyForgeIssue(EnemyForgeSeverity.Info, "This is a scene object, not a saved prefab."));
            return issues;
        }

        static float EstimateHorizontalFootprintRadius(GameObject root)
        {
            if (root == null) return 0f;

            bool found = false;
            Bounds combined = default;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || renderer is ParticleSystemRenderer ||
                    renderer is TrailRenderer || renderer is LineRenderer)
                    continue;

                if (!found) { combined = renderer.bounds; found = true; }
                else combined.Encapsulate(renderer.bounds);
            }

            return found ? Mathf.Max(combined.extents.x, combined.extents.z) : 0f;
        }

        static bool HasUsableRootCombatCollider(GameObject prefab)
        {
            foreach (var collider in prefab.GetComponents<Collider>())
                if (collider != null && collider.enabled && !collider.isTrigger)
                    return true;

            return false;
        }

        static bool HasAnimationDriver(GameObject root)
        {
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (component != null && IsAnimationDriver(component.GetType().Name)) return true;
            return false;
        }

        static int CountActiveAiControllers(GameObject root)
        {
            int count = root.GetComponent<EnemyController>() != null ? 1 : 0;
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null || !component.enabled) continue;
                string name = component.GetType().Name;
                if (name == "EnemyWanderAI" || name == "EnemyAI") count++;
            }
            return count;
        }

        internal static bool IsAnimationDriver(string typeName) =>
            typeName == "EnemyWanderAnimationDriver" || typeName == "OgreAnimationDriver" ||
            typeName == "FieldGoulAnimationDriver" || typeName == "CyclopsAnimationDriver";
    }
}
#endif
