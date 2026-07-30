#if UNITY_EDITOR || !UNITY_SERVER
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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
    public float pollInterval = 0.05f;

    Transform _localPlayer;
    Scene _appliedZone;
    string _requestedZoneName;
    int _appliedSceneCount = -1;
    float _nextPoll;

    // Original component state is remembered so an object intentionally disabled
    // by its scene is not accidentally enabled when that scene becomes local.
    readonly Dictionary<Behaviour, bool> _originalEnabled = new Dictionary<Behaviour, bool>();
    readonly Dictionary<AudioSource, bool> _originalAudioMute = new Dictionary<AudioSource, bool>();
    readonly Dictionary<Scene, EnvironmentState> _zoneEnvironments = new Dictionary<Scene, EnvironmentState>();
    readonly HashSet<Scene> _pendingEnvironmentCaptures = new HashSet<Scene>();

    struct EnvironmentState
    {
        public Material skybox;
        public bool fog;
        public Color fogColor;
        public FogMode fogMode;
        public float fogDensity;
        public float fogStartDistance;
        public float fogEndDistance;
        public AmbientMode ambientMode;
        public Color ambientSkyColor;
        public Color ambientEquatorColor;
        public Color ambientGroundColor;
        public Color ambientLight;
        public float ambientIntensity;
        public Color subtractiveShadowColor;
        public DefaultReflectionMode defaultReflectionMode;
        public int defaultReflectionResolution;
        public Texture customReflection;
        public float reflectionIntensity;
        public int reflectionBounces;
        public float haloStrength;
        public float flareStrength;
        public float flareFadeSpeed;
    }

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

    /// <summary>Applies camera and presentation immediately after a local zone move.</summary>
    public static void RefreshNow()
    {
        if (Instance != null)
            Instance.Apply(force: true);
    }

    /// <summary>
    /// Records the server-authoritative destination before additive loading begins.
    /// This prevents sceneLoaded from briefly reapplying the previous zone's
    /// presentation while both source and destination scenes are present.
    /// </summary>
    public static void BeginTravel(string zoneName)
    {
        if (Instance == null) return;
        Instance._requestedZoneName = SceneNames.NormalizeZone(zoneName);
    }

    /// <summary>
    /// Applies presentation for an explicitly confirmed server destination. Remote
    /// clients briefly contain both source and destination additive scenes, so scene
    /// count alone cannot identify the correct one during that overlap.
    /// </summary>
    public static void RefreshNow(string zoneName)
    {
        if (Instance == null) return;
        Instance._requestedZoneName = SceneNames.NormalizeZone(zoneName);
        Instance.Apply(force: true);
    }

    /// <summary>
    /// True when a scene owns the presentation currently selected for the local
    /// player. With a remote client the player object remains in the container,
    /// so trigger code cannot determine this from player.gameObject.scene.
    /// </summary>
    public static bool IsCurrentLocalZone(Scene scene)
    {
        return Instance == null
               || !Instance._appliedZone.IsValid()
               || Instance._appliedZone == scene;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(force: true);
    }

    void OnSceneUnloaded(Scene scene)
    {
        _zoneEnvironments.Remove(scene);
        _pendingEnvironmentCaptures.Remove(scene);
        Apply(force: true);
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

        Scene previousZone = _appliedZone;
        _appliedZone = zone;
        _appliedSceneCount = SceneManager.sceneCount;

        int enabled = 0;
        Camera chosen = null;

        // Keep the original global discovery path, with a direct scene fallback.
        foreach (Camera cam in Resources.FindObjectsOfTypeAll<Camera>())
        {
            if (cam == null || !cam.gameObject.scene.IsValid()) continue;   // prefab assets
            if (!cam.gameObject.activeInHierarchy) continue;

            bool mine = cam.gameObject.scene == zone;
            cam.enabled = mine;

            if (cam.TryGetComponent(out AudioListener listener))
                listener.enabled = mine;

            if (mine) { enabled++; if (chosen == null) chosen = cam; }
        }

        // Unity can briefly omit a newly additively-loaded local-physics scene from
        // Resources.FindObjectsOfTypeAll during the same frame. Read that scene's
        // hierarchy directly so camera/movement binding never waits for a later poll.
        if (chosen == null)
        {
            foreach (Camera cam in FindSceneComponents<Camera>(zone))
            {
                if (cam == null || !cam.gameObject.activeInHierarchy) continue;
                cam.enabled = true;

                if (cam.TryGetComponent(out AudioListener listener))
                    listener.enabled = true;

                chosen = cam;
                enabled = 1;
                break;
            }
        }

        if (chosen == null)
        {
            Debug.LogWarning($"[ZoneCam] No camera found in '{zone.name}' — the view will be black. " +
                             $"That zone scene needs a Camera.");
            return;
        }

        // Camera-relative movement must be restored before any optional visual work.
        // A malformed scene environment must never be able to interrupt WASD binding.
        Rebind(chosen);

        // Everything below is presentation-only and intentionally runs after the
        // original camera/movement path has completed.
        if (previousZone.IsValid())
        {
            try
            {
                CaptureZoneState(previousZone);
                _zoneEnvironments[previousZone] = CaptureEnvironment();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ZoneCam] Could not preserve presentation for '{previousZone.name}'.\n{ex}");
            }
        }

        try
        {
            ApplyZoneEnvironment(zone);
            ApplyZonePresentation(zone);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ZoneCam] Presentation setup failed for '{zone.name}', but camera and movement " +
                           $"remain active.\n{ex}");
        }

        Debug.Log($"[ZoneCam] Active zone '{zone.name}' — using '{chosen.name}', {enabled} enabled, others off.");
    }

    /// <summary>
    /// Keeps additively-loaded server zones operational while selecting the local
    /// player's lights, probes, volumes and audio. Renderers and Terrain are
    /// deliberately untouched: disabling arbitrary static scene presentation can
    /// hide a destination before its hierarchy has fully completed additive setup.
    /// Physics, colliders, NavMesh, scripts and networking are also untouched.
    /// </summary>
    void ApplyZonePresentation(Scene zone)
    {
        Light chosenSun = null;

        foreach (Light light in Resources.FindObjectsOfTypeAll<Light>())
        {
            if (!IsLoadedZoneComponent(light)) continue;
            Remember(_originalEnabled, light, light.enabled);

            bool active = light.gameObject.scene == zone && _originalEnabled[light];
            light.enabled = active;

            if (active && chosenSun == null && light.type == LightType.Directional)
                chosenSun = light;
        }

        foreach (ReflectionProbe probe in Resources.FindObjectsOfTypeAll<ReflectionProbe>())
        {
            if (!IsLoadedZoneComponent(probe)) continue;
            Remember(_originalEnabled, probe, probe.enabled);
            probe.enabled = probe.gameObject.scene == zone
                && _originalEnabled[probe];
        }

        foreach (Volume volume in Resources.FindObjectsOfTypeAll<Volume>())
        {
            if (!IsLoadedZoneComponent(volume)) continue;
            Remember(_originalEnabled, volume, volume.enabled);
            volume.enabled = volume.gameObject.scene == zone
                && _originalEnabled[volume];
        }

        foreach (AudioSource source in Resources.FindObjectsOfTypeAll<AudioSource>())
        {
            if (!IsLoadedZoneComponent(source)) continue;
            Remember(_originalAudioMute, source, source.mute);
            source.mute = source.gameObject.scene == zone
                ? _originalAudioMute[source]
                : true;
        }

        ApplyZoneMusic(zone);

        // RenderSettings are global across all additively-loaded scenes.
        RenderSettings.sun = chosenSun;
    }

    void ApplyZoneMusic(Scene zone)
    {
        foreach (MusicZoneTrigger trigger in FindSceneComponents<MusicZoneTrigger>(zone))
        {
            if (trigger == null || !trigger.isActiveAndEnabled)
                continue;

            trigger.ActivateForLocalZone();
            return;
        }

        // The music controller is intentionally persistent. Without a destination
        // trigger its previous clip would otherwise follow the player forever and
        // overlap any ordinary AudioSources authored in this zone.
        MusicController.Instance?.Stop();
    }

    void ApplyZoneEnvironment(Scene zone)
    {
        if (!_zoneEnvironments.TryGetValue(zone, out EnvironmentState environment))
        {
            if (_pendingEnvironmentCaptures.Add(zone))
                StartCoroutine(CaptureZoneEnvironmentAfterActivation(zone));
            return;
        }

        ApplyEnvironmentValues(environment);
    }

    IEnumerator CaptureZoneEnvironmentAfterActivation(Scene zone)
    {
        // Let the additive load complete all scene activation callbacks first.
        yield return null;

        if (!zone.IsValid() || !zone.isLoaded)
        {
            _pendingEnvironmentCaptures.Remove(zone);
            yield break;
        }

        Scene previousActive = SceneManager.GetActiveScene();
        bool switched = previousActive != zone && SceneManager.SetActiveScene(zone);

        // RenderSettings and URP volumes do not reliably expose a newly active
        // additive scene's serialized environment until the following frame.
        yield return null;

        if (!zone.IsValid() || !zone.isLoaded)
        {
            _pendingEnvironmentCaptures.Remove(zone);
            yield break;
        }

        EnvironmentState environment = CaptureEnvironment();
        _zoneEnvironments[zone] = environment;

        if (switched && previousActive.IsValid() && previousActive.isLoaded)
            SceneManager.SetActiveScene(previousActive);

        // Restoring the server container restores its global environment too, so
        // immediately reapply the destination values for this local presentation.
        ApplyEnvironmentValues(environment);

        foreach (Light light in Resources.FindObjectsOfTypeAll<Light>())
        {
            if (light != null && light.gameObject.scene == zone
                && light.enabled && light.type == LightType.Directional)
            {
                RenderSettings.sun = light;
                break;
            }
        }

        _pendingEnvironmentCaptures.Remove(zone);
        LoadingScreen.NotifyEnvironmentReady();
    }

    static void ApplyEnvironmentValues(EnvironmentState environment)
    {
        RenderSettings.skybox = environment.skybox;
        RenderSettings.fog = environment.fog;
        RenderSettings.fogColor = environment.fogColor;
        RenderSettings.fogMode = environment.fogMode;
        RenderSettings.fogDensity = environment.fogDensity;
        RenderSettings.fogStartDistance = environment.fogStartDistance;
        RenderSettings.fogEndDistance = environment.fogEndDistance;
        RenderSettings.ambientMode = environment.ambientMode;
        RenderSettings.ambientSkyColor = environment.ambientSkyColor;
        RenderSettings.ambientEquatorColor = environment.ambientEquatorColor;
        RenderSettings.ambientGroundColor = environment.ambientGroundColor;
        RenderSettings.ambientLight = environment.ambientLight;
        RenderSettings.ambientIntensity = environment.ambientIntensity;
        RenderSettings.subtractiveShadowColor = environment.subtractiveShadowColor;
        RenderSettings.defaultReflectionMode = environment.defaultReflectionMode;
        RenderSettings.defaultReflectionResolution = environment.defaultReflectionResolution;
        if (environment.defaultReflectionMode == DefaultReflectionMode.Custom
            && environment.customReflection != null)
            RenderSettings.customReflectionTexture = environment.customReflection;
        RenderSettings.reflectionIntensity = environment.reflectionIntensity;
        RenderSettings.reflectionBounces = environment.reflectionBounces;
        RenderSettings.haloStrength = environment.haloStrength;
        RenderSettings.flareStrength = environment.flareStrength;
        RenderSettings.flareFadeSpeed = environment.flareFadeSpeed;
    }

    static EnvironmentState CaptureEnvironment()
    {
        EnvironmentState environment = new EnvironmentState
        {
            skybox = RenderSettings.skybox,
            fog = RenderSettings.fog,
            fogColor = RenderSettings.fogColor,
            fogMode = RenderSettings.fogMode,
            fogDensity = RenderSettings.fogDensity,
            fogStartDistance = RenderSettings.fogStartDistance,
            fogEndDistance = RenderSettings.fogEndDistance,
            ambientMode = RenderSettings.ambientMode,
            ambientSkyColor = RenderSettings.ambientSkyColor,
            ambientEquatorColor = RenderSettings.ambientEquatorColor,
            ambientGroundColor = RenderSettings.ambientGroundColor,
            ambientLight = RenderSettings.ambientLight,
            ambientIntensity = RenderSettings.ambientIntensity,
            subtractiveShadowColor = RenderSettings.subtractiveShadowColor,
            defaultReflectionMode = RenderSettings.defaultReflectionMode,
            defaultReflectionResolution = RenderSettings.defaultReflectionResolution,
            reflectionIntensity = RenderSettings.reflectionIntensity,
            reflectionBounces = RenderSettings.reflectionBounces,
            haloStrength = RenderSettings.haloStrength,
            flareStrength = RenderSettings.flareStrength,
            flareFadeSpeed = RenderSettings.flareFadeSpeed
        };

        if (environment.defaultReflectionMode == DefaultReflectionMode.Custom)
            environment.customReflection = RenderSettings.customReflectionTexture;

        return environment;
    }

    void CaptureZoneState(Scene zone)
    {
        foreach (Light light in Resources.FindObjectsOfTypeAll<Light>())
            if (light != null && light.gameObject.scene == zone)
                _originalEnabled[light] = light.enabled;

        foreach (ReflectionProbe probe in Resources.FindObjectsOfTypeAll<ReflectionProbe>())
            if (probe != null && probe.gameObject.scene == zone)
                _originalEnabled[probe] = probe.enabled;

        foreach (Volume volume in Resources.FindObjectsOfTypeAll<Volume>())
            if (volume != null && volume.gameObject.scene == zone)
                _originalEnabled[volume] = volume.enabled;

        foreach (AudioSource source in Resources.FindObjectsOfTypeAll<AudioSource>())
            if (source != null && source.gameObject.scene == zone)
                _originalAudioMute[source] = source.mute;
    }

    static void Remember<T>(Dictionary<T, bool> states, T component, bool value)
    {
        if (!states.ContainsKey(component))
            states.Add(component, value);
    }

    static bool IsLoadedZoneComponent(Component component)
    {
        return component != null && IsZoneScene(component.gameObject.scene);
    }

    static List<T> FindSceneComponents<T>(Scene scene) where T : Component
    {
        var found = new List<T>();
        if (!scene.IsValid() || !scene.isLoaded) return found;

        foreach (GameObject root in scene.GetRootGameObjects())
            root.GetComponentsInChildren(true, found);

        return found;
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
        // Reassigning the same target calls SnapToTarget and resets the player's
        // orbit behind the character. Only snap when actually changing cameras.
        if (follow.target != _localPlayer)
            follow.target = _localPlayer;

        var movement = _localPlayer.GetComponent<PlayerMovement>();
        if (movement != null) movement.cam = cam;

        // Ability targeting also caches a camera for ScreenPointToRay. Keep it on
        // the exact same authoritative zone camera as movement so additive loading
        // can never briefly mirror or reverse mouse targeting.
        var abilityCaster = _localPlayer.GetComponent<AbilityCaster>();
        if (abilityCaster != null) abilityCaster.cam = cam;
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

        if (!string.IsNullOrWhiteSpace(_requestedZoneName))
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene requested = SceneManager.GetSceneAt(i);
                if (requested.isLoaded && IsZoneScene(requested) &&
                    string.Equals(requested.name, _requestedZoneName,
                        System.StringComparison.OrdinalIgnoreCase))
                    return requested;
            }
        }

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
