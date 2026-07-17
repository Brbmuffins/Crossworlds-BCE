using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HubReturnArrival
{
    const float PlayerSpacing = 1.5f;

    static bool _pending;
    static string _sceneName;
    static string _spawnId;
    static bool _applySpawnRotation;
    static GameObject _localPlayerToPlace;

    public static void Request(string sceneName, string spawnId, bool applySpawnRotation)
    {
        _pending = true;
        _sceneName = sceneName;
        _spawnId = string.IsNullOrWhiteSpace(spawnId) ? HubReturnSpawnPoint.DefaultSpawnId : spawnId.Trim();
        _applySpawnRotation = applySpawnRotation;
    }

    public static bool TryGetRequestForScene(string loadedSceneName, out string spawnId, out bool applySpawnRotation)
    {
        spawnId = _spawnId;
        applySpawnRotation = _applySpawnRotation;
        return _pending && SceneMatches(loadedSceneName, _sceneName);
    }

    public static void Clear()
    {
        _pending = false;
        _sceneName = null;
        _spawnId = null;
        _applySpawnRotation = true;
        _localPlayerToPlace = null;
    }

    public static void CarryLocalPlayer(GameObject player)
    {
        if (player == null)
            return;

        _localPlayerToPlace = player.transform.root.gameObject;
        UnityEngine.Object.DontDestroyOnLoad(_localPlayerToPlace);
    }

    public static Vector3 OffsetForPlayer(Transform spawnPoint, int index, int total)
    {
        if (spawnPoint == null || total <= 1)
            return Vector3.zero;

        float centeredIndex = index - ((total - 1) * 0.5f);
        return spawnPoint.right * centeredIndex * PlayerSpacing;
    }

    public static void PlacePlayer(GameObject player, Transform spawnPoint, Vector3 offset, bool applySpawnRotation)
    {
        if (player == null || spawnPoint == null)
            return;

        Vector3 position = spawnPoint.position + offset;
        Quaternion rotation = applySpawnRotation ? spawnPoint.rotation : player.transform.rotation;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            rb.rotation = rotation;
        }

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        player.transform.SetPositionAndRotation(position, rotation);

        if (controller != null)
            controller.enabled = true;

        Physics.SyncTransforms();
    }

    public static bool SceneMatches(string loadedSceneName, string requestedSceneName)
    {
        if (string.IsNullOrWhiteSpace(requestedSceneName))
            return false;

        if (string.Equals(loadedSceneName, requestedSceneName, StringComparison.OrdinalIgnoreCase))
            return true;

        Scene activeScene = SceneManager.GetActiveScene();
        if (string.Equals(activeScene.name, requestedSceneName, StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(activeScene.path, requestedSceneName, StringComparison.OrdinalIgnoreCase))
            return true;

        return string.Equals(loadedSceneName, SceneNameOnly(requestedSceneName), StringComparison.OrdinalIgnoreCase);
    }

    static string SceneNameOnly(string sceneNameOrPath)
    {
        string value = sceneNameOrPath.Replace('\\', '/');
        int slash = value.LastIndexOf('/');
        if (slash >= 0)
            value = value.Substring(slash + 1);

        const string unityExtension = ".unity";
        if (value.EndsWith(unityExtension, StringComparison.OrdinalIgnoreCase))
            value = value.Substring(0, value.Length - unityExtension.Length);

        return value;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void HookSceneLoaded()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_pending || NetworkServer.active || NetworkClient.active || _localPlayerToPlace == null)
            return;

        if (!SceneMatches(scene.name, _sceneName))
            return;

        Transform spawnPoint = HubReturnSpawnPoint.Find(_spawnId);
        if (spawnPoint != null)
            PlacePlayer(_localPlayerToPlace, spawnPoint, Vector3.zero, _applySpawnRotation);

        SceneManager.MoveGameObjectToScene(_localPlayerToPlace, scene);
        Clear();
    }
}
