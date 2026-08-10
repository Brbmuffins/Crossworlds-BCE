using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Server-safe rules for the open-combat PVP zone. Existing offensive abilities
/// are authored against the Enemy tag; inside PVPZONE, another player also counts
/// as an offensive target. Player damage remains blocked everywhere else.
/// </summary>
public static class PvpCombatRules
{
    const string OffensiveTargetTag = "Enemy";

    public static bool IsPvpZone(GameObject context)
    {
        if (context == null) return false;

        Scene scene = context.scene;
        return scene.IsValid() &&
               scene.isLoaded &&
               string.Equals(scene.name, SceneNames.PvpZone, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsOpponent(GameObject attacker, Health target)
    {
        Health attackerHealth = FindPlayerHealth(attacker);
        if (attackerHealth == null || target == null || !target.isPlayer || attackerHealth == target)
            return false;

        Scene attackerScene = attackerHealth.gameObject.scene;
        Scene targetScene = target.gameObject.scene;
        return attackerScene.IsValid() &&
               targetScene.IsValid() &&
               attackerScene.handle == targetScene.handle &&
               IsPvpZone(attackerHealth.gameObject);
    }

    /// <summary>
    /// Defence-in-depth for damage paths that bypass AbilityCaster targeting.
    /// Enemy/environmental damage is unaffected; only player-on-player damage is gated.
    /// </summary>
    public static bool ShouldBlockPlayerDamage(GameObject source, Health target)
    {
        if (target == null || !target.isPlayer)
            return false;

        Health sourcePlayer = FindPlayerHealth(source);
        if (sourcePlayer == null || sourcePlayer == target)
            return false;

        return !IsOpponent(sourcePlayer.gameObject, target);
    }

    public static bool MatchesTarget(
        GameObject attacker,
        Collider hit,
        string targetTag,
        out Health health)
    {
        health = hit != null ? hit.GetComponentInParent<Health>() : null;
        return health != null &&
               health.IsAlive &&
               MatchesTarget(attacker, hit, health, targetTag);
    }

    public static bool MatchesTarget(
        GameObject attacker,
        Collider hit,
        Health health,
        string targetTag)
    {
        if (health == null)
            return false;

        if (string.IsNullOrEmpty(targetTag))
            return true;

        if ((hit != null && hit.CompareTag(targetTag)) || health.CompareTag(targetTag))
            return true;

        Transform root = health.transform.root;
        if (root != null && root.CompareTag(targetTag))
            return true;

        return string.Equals(targetTag, OffensiveTargetTag, StringComparison.OrdinalIgnoreCase) &&
               IsOpponent(attacker, health);
    }

    static Health FindPlayerHealth(GameObject source)
    {
        if (source == null) return null;

        Health health = source.GetComponentInParent<Health>();
        if (health == null)
            health = source.GetComponentInChildren<Health>();

        return health != null && health.isPlayer ? health : null;
    }
}
