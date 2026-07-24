#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.SceneManagement;

// ═══════════════════════════════════════════════════════════════════════════
//  ZoneCameraDirector — exactly one live camera, always (ROADMAP 6.9)
//
//  THE PROBLEM
//  Every zone scene carries its own Camera and AudioListener. That was correct
//  when only one zone was ever loaded. With zones loaded additively there can be
//  several at once — guaranteed in host mode, where the server loads every zone
//  a player visits, and transiently on a real client during travel, which briefly
//  holds both the old and new zone.
//
//  Consequences, all observed:
//    • Camera.main returns an ARBITRARY MainCamera. PlayerMovement is
//      camera-relative, so binding to another zone's camera rotates WASD by
//      however that camera happens to face — the "movement is a mix of WASD" bug.
//    • Unity warns about multiple AudioListeners and the audio goes wrong.
//    • The wrong view can render entirely.
//
//  WHY NOT JUST DELETE THE ZONE CAMERAS
//  Because each carries its own settings — culling mask, clear flags, projection,
//  whatever post-processing it is set up for. One shared camera in the container
//  would be configured correctly for no zone at all. So instead of removing them,
//  this enables the one belonging to the player's current zone and disables the
//  rest. Per-zone look is preserved; ambiguity is not.
//
//  Camera.allCameras only returns ENABLED cameras, so once this has run,
//  PlayerMovement.ResolveCamera sees exactly one and cannot pick wrong.
//
//  Client-side presentation only — the dedicated server renders nothing.
// ═══════════════════════════════════════════════════════════════════════════

[AddComponentMenu("BCE/Network/Zone Camera Director")]
public class ZoneCameraDirector : MonoBehaviour
{
    public static ZoneCameraDirector Instance { get; private set; }

    [Tooltip("Seconds between checks for the local player having changed zone.")]
    public float pollInterval = 0.25f;

    Transform _localPlayer;
    Scene _appliedZone;
    int _appliedSceneCount = -1;
    float _nextPoll;

    /// <summary>Creates the director if it does not exist yet, and points it at the local player.</summary>
    public static void EnsureExists(Transform localPlayer)
    {
        if (Instance == null)
        {
            var go = new GameObject("ZoneCameraDirector");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<ZoneCameraDirector>();
        }

        Instance._localPlayer = localPlayer;
        Instance.Apply(force: true);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (Time.unscaledTime < _nextPoll) return;
        _nextPoll = Time.unscaledTime + pollInterval;

        Apply(force: false);
    }

    void Apply(bool force)
    {
        if (_localPlayer == null) return;

        Scene zone = ResolveActiveZone();
        if (!zone.IsValid()) return;

        // Scene count is part of the key: travel unloads a zone and destroys its
        // camera, so the same zone handle can need re-applying afterwards.
        if (!force && zone == _appliedZone && SceneManager.sceneCount == _appliedSceneCount)
            return;

        _appliedZone = zone;
        _appliedSceneCount = SceneManager.sceneCount;

        int enabled = 0;
        Camera chosen = null;

        foreach (Camera cam in Resources.FindObjectsOfTypeAll<Camera>())
        {
            if (cam == null || cam.gameObject.scene.handle == 0) continue;   // prefab assets
            if (!cam.gameObject.activeInHierarchy) continue;

            bool mine = cam.gameObject.scene == zone;
            cam.enabled = mine;

            if (cam.TryGetComponent(out AudioListener listener))
                listener.enabled = mine;

            if (mine) { enabled++; if (chosen == null) chosen = cam; }
        }

        if (chosen == null)
        {
            Debug.LogWarning($"[ZoneCam] No camera found in '{zone.name}' — the view will be black. " +
                             $"That zone scene needs a Camera.");
            return;
        }

        Rebind(chosen);
        Debug.Log($"[ZoneCam] Active zone '{zone.name}' — using '{chosen.name}', {enabled} enabled, others off.");
    }

    /// <summary>
    /// Points the follow rig and the player's movement at the newly chosen camera.
    ///
    /// Necessary because PlayerMovement caches its camera in Start and never looks
    /// again — without this, travelling to a new zone leaves movement relative to
    /// the OLD zone's camera, which is disabled and about to be destroyed when that
    /// zone unloads. That is the same class of bug as the original Camera.main one,
    /// just deferred to the second zone the player visits.
    /// </summary>
    void Rebind(Camera cam)
    {
        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();

        follow.cameraCollision = true;
        follow.collisionMask = ~0;
        follow.target = _localPlayer;   // setter snaps to target

        var movement = _localPlayer.GetComponent<PlayerMovement>();
        if (movement != null) movement.cam = cam;
    }

    /// <summary>
    /// Which loaded scene counts as "where the player is".
    ///
    /// Host: the player object was moved into its zone scene server-side, so its own
    /// scene is the answer directly.
    ///
    /// Remote client: Mirror instantiates spawned objects into the ACTIVE scene, so
    /// the player sits in the empty container while the zone is loaded additively.
    /// There the answer is the single loaded scene that is not the container.
    /// </summary>
    Scene ResolveActiveZone()
    {
        Scene playerScene = _localPlayer.gameObject.scene;
        if (playerScene.IsValid() && IsZoneScene(playerScene))
            return playerScene;

        Scene found = default;
        int count = 0;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (!s.isLoaded || !IsZoneScene(s)) continue;

            found = s;
            count++;
        }

        // Exactly one zone loaded — unambiguous. More than one and the player is not
        // in any of them: nothing sensible to pick, so leave the view alone rather
        // than flickering between zones.
        return count == 1 ? found : default;
    }

    static bool IsZoneScene(Scene scene)
    {
        return scene.IsValid()
               && scene.isLoaded
               && scene.name != SceneNames.Container
               && scene.name != "DontDestroyOnLoad";
    }
}
#endif
