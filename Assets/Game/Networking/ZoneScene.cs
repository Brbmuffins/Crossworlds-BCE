using UnityEngine;
using UnityEngine.SceneManagement;

// ═══════════════════════════════════════════════════════════════════════════
//  ZoneScene — scene placement for server-spawned objects.
//
//  WHY THIS EXISTS (ROADMAP 6.1, prerequisite for 6.3):
//  Object.Instantiate places the new object in the ACTIVE scene, not in the
//  scene of whatever spawned it. Today the active scene IS the zone, so that
//  happens to be correct. Once zones load additively on top of an empty
//  container scene (ROADMAP 6.3), the active scene becomes that empty
//  container — so every enemy, drop, projectile and deployable would be
//  created OUTSIDE the zone it belongs to.
//
//  That breaks more than tidiness: SceneInterestManagement keys observers off
//  gameObject.scene, so the entire world would be filed under "container" and
//  no player would ever observe any of it.
//
//  Call PlaceWith(obj, owner) after Instantiate and BEFORE NetworkServer.Spawn
//  — Mirror reads the scene at spawn time to build the observer set.
//
//  This is deliberately a NO-OP under the current single-scene setup: the
//  early-out on line "already in the right scene" catches every case today.
// ═══════════════════════════════════════════════════════════════════════════

public static class ZoneScene
{
    const string DontDestroyOnLoadSceneName = "DontDestroyOnLoad";

    /// <summary>
    /// Moves a freshly Instantiated object into the same scene as the thing that
    /// spawned it, so it belongs to that zone. Safe to call in offline/solo play
    /// and safe to call when the object is already in the right scene.
    /// </summary>
    public static void PlaceWith(GameObject obj, GameObject owner)
    {
        if (obj == null || owner == null) return;

        // MoveGameObjectToScene throws on non-root objects. Every current caller
        // Instantiates without a parent; if that ever changes, the child already
        // travels with its parent's scene, so skipping is correct.
        if (obj.transform.parent != null) return;

        Scene target = owner.scene;
        if (!target.IsValid() || !target.isLoaded) return;

        // An owner marked DontDestroyOnLoad lives in Unity's DDOL pseudo-scene,
        // which is not a zone. Leave the spawned object where it is rather than
        // dragging it somewhere no player can observe it (see ROADMAP 6.5).
        if (target.name == DontDestroyOnLoadSceneName) return;

        if (obj.scene == target) return;

        SceneManager.MoveGameObjectToScene(obj, target);
    }

    /// <summary>
    /// Overload for spawners that know the destination scene directly rather
    /// than via an owner GameObject (zone spawn tables, ZoneManager).
    /// </summary>
    public static void PlaceIn(GameObject obj, Scene target)
    {
        if (obj == null) return;
        if (obj.transform.parent != null) return;
        if (!target.IsValid() || !target.isLoaded) return;
        if (target.name == DontDestroyOnLoadSceneName) return;
        if (obj.scene == target) return;

        SceneManager.MoveGameObjectToScene(obj, target);
    }
}
