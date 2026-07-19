using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkIdentity))]
[AddComponentMenu("BCE/Scene/Waypoint Map Trigger")]
public sealed class WaypointMapTrigger : NetworkBehaviour
{
    [Header("Interaction")]
    [Min(0.5f)] public float interactionRange = 5f;
    public bool allowKeyboardInteract = true;
    public Key interactKey = Key.E;
    public bool allowMouseClick = true;
    public LayerMask clickableLayers = ~0;
    [Min(1f)] public float clickRaycastDistance = 200f;

    [Header("Prompt")]
    public bool showWorldPrompt = true;
    public string promptText = "Open World Map";
    public Vector3 promptOffset = new Vector3(0f, 3.2f, 0f);

    [Header("Map")]
    public string mapTitle = "WORLD MAP";
    public Sprite mapBackground;
    public bool closeMapAfterTravelRequest = true;
    public WaypointMapNode[] nodes = DefaultNodes();
    public WaypointMapConnection[] connections = DefaultConnections();

    bool _playerNear;
    bool _loading;
    Transform _localPlayer;
    GameObject _promptObject;
    TextMeshPro _promptLabel;

    void Reset()
    {
        nodes = DefaultNodes();
        connections = DefaultConnections();
    }

    void Awake()
    {
        if (nodes == null || nodes.Length == 0)
            nodes = DefaultNodes();
        if (connections == null || connections.Length == 0)
            connections = DefaultConnections();

    }

    void Start()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (showWorldPrompt)
            BuildWorldPrompt();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR || !UNITY_SERVER
        if (_loading) return;

        if (_localPlayer == null)
            _localPlayer = FindLocalPlayer();

        _playerNear = _localPlayer != null &&
                      Vector3.Distance(transform.position, _localPlayer.position) <= interactionRange;

        SetPromptVisible(_playerNear);
        if (!_playerNear) return;

        if (allowKeyboardInteract && WasInteractPressed() && !IsTypingInInputField())
        {
            OpenMap();
            return;
        }

        if (allowMouseClick && WasClicked())
            OpenMap();
#endif
    }

#if UNITY_EDITOR || !UNITY_SERVER
    void OpenMap()
    {
        WaypointMapUI.Show(mapTitle, mapBackground, nodes, connections, HandleNodeSelected);
    }
#endif

    void HandleNodeSelected(WaypointMapNode node)
    {
        if (node == null)
            return;

        if (!node.CanTravel)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            WaypointMapUI.SetStatus($"{node.displayName} is not available yet.");
#endif
            return;
        }

        string sceneName = node.sceneName.Trim();
        if (!CanLoadScene(sceneName))
        {
#if UNITY_EDITOR || !UNITY_SERVER
            WaypointMapUI.SetStatus($"{node.displayName} scene is not in Build Settings yet.");
#endif
            return;
        }

        _loading = true;
#if UNITY_EDITOR || !UNITY_SERVER
        WaypointMapUI.SetStatus($"Traveling to {node.displayName}...");
        if (closeMapAfterTravelRequest)
            WaypointMapUI.Hide();
#endif

        if (NetworkServer.active)
        {
            ChangeScene(sceneName);
            return;
        }

        if (NetworkClient.active)
        {
            CmdRequestTravel(sceneName);
            return;
        }

        ChangeScene(sceneName);
    }

    [Command(requiresAuthority = false)]
    void CmdRequestTravel(string sceneName, NetworkConnectionToClient sender = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        if (!CanLoadScene(sceneName))
        {
            if (sender != null)
                TargetTravelRejected(sender, $"{sceneName} is not in Build Settings yet.");
            return;
        }

        ChangeScene(sceneName);
    }

    [TargetRpc]
    void TargetTravelRejected(NetworkConnectionToClient target, string message)
    {
        _loading = false;
#if UNITY_EDITOR || !UNITY_SERVER
        WaypointMapUI.SetStatus(message);
#endif
    }

    static void ChangeScene(string sceneName)
    {
        if (NetworkManager.singleton != null && NetworkServer.active)
        {
            NetworkManager.singleton.ServerChangeScene(sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    static bool CanLoadScene(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName) &&
               (Application.CanStreamedLevelBeLoaded(sceneName) ||
                string.Equals(SceneManager.GetActiveScene().name, sceneName, StringComparison.OrdinalIgnoreCase));
    }

#if UNITY_EDITOR || !UNITY_SERVER
    bool WasInteractPressed()
    {
        var keyboard = Keyboard.current;
        return keyboard != null &&
               interactKey != Key.None &&
               keyboard[interactKey].wasPressedThisFrame;
    }

    bool WasClicked()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return false;

        var eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.IsPointerOverGameObject()) return false;

        var camera = Camera.main;
        if (camera == null) return false;

        Ray ray = camera.ScreenPointToRay(mouse.position.ReadValue());
        return Physics.Raycast(ray, out RaycastHit hit, clickRaycastDistance, clickableLayers, QueryTriggerInteraction.Collide)
               && HitBelongsToThisWaypoint(hit.transform);
    }

    bool HitBelongsToThisWaypoint(Transform hit)
    {
        return hit == transform || hit.IsChildOf(transform) || transform.IsChildOf(hit);
    }

    bool IsTypingInInputField()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        return selected != null &&
               (selected.GetComponent<TMP_InputField>() != null ||
                selected.GetComponent<UnityEngine.UI.InputField>() != null);
    }

    static Transform FindLocalPlayer()
    {
        var identities = FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var identity in identities)
            if (identity.isLocalPlayer)
                return identity.transform;

        GameObject taggedPlayer = GameObject.FindWithTag("Player");
        return taggedPlayer != null ? taggedPlayer.transform : null;
    }

    void BuildWorldPrompt()
    {
        if (_promptObject != null) return;

        _promptObject = new GameObject("WaypointMapPrompt");
        _promptObject.transform.SetParent(transform, false);
        _promptObject.transform.localPosition = promptOffset;
        _promptObject.AddComponent<RodBillboard>();

        _promptLabel = _promptObject.AddComponent<TextMeshPro>();
        _promptLabel.text = promptText;
        _promptLabel.alignment = TextAlignmentOptions.Center;
        _promptLabel.fontSize = 3f;
        _promptLabel.richText = true;
        _promptLabel.raycastTarget = false;
        _promptLabel.enableWordWrapping = false;
        _promptLabel.color = Color.white;

        SetPromptVisible(false);
    }

    void SetPromptVisible(bool visible)
    {
        if (_promptObject != null && _promptObject.activeSelf != visible)
            _promptObject.SetActive(visible);
    }
#endif // UNITY_EDITOR || !UNITY_SERVER

    static WaypointMapNode[] DefaultNodes()
    {
        return new[]
        {
            new WaypointMapNode
            {
                id = "neimos",
                displayName = "NEIMOS",
                subtitle = "island - not to scale",
                sceneName = "",
                unlocked = false,
                normalizedPosition = new Vector2(0.07f, 0.15f),
                labelOffset = new Vector2(0f, -42f),
                color = new Color(0.0f, 0.75f, 0.78f, 1f),
                description = "Placeholder island node."
            },
            new WaypointMapNode
            {
                id = "sw",
                displayName = "SW",
                subtitle = "unwritten",
                sceneName = "",
                unlocked = false,
                normalizedPosition = new Vector2(0.05f, 0.52f),
                labelOffset = new Vector2(0f, -40f),
                color = new Color(0.72f, 0.66f, 0.66f, 1f),
                description = "Placeholder southwest region."
            },
            new WaypointMapNode
            {
                id = "gathering",
                displayName = "GATHERING ZONE",
                subtitle = "harvest · craft · relax",
                sceneName = SceneNames.GatheringZone,
                unlocked = true,
                normalizedPosition = new Vector2(0.24f, 0.72f),
                labelOffset = new Vector2(0f, -34f),
                color = new Color(0.82f, 0.36f, 0.38f, 1f),
                description = "A peaceful zone for AFK woodcutting, fishing, and mining. A forge is available for crafting."
            },
            new WaypointMapNode
            {
                id = "brightwood",
                displayName = "BRIGHTWOOD",
                subtitle = "",
                sceneName = "",
                unlocked = false,
                normalizedPosition = new Vector2(0.46f, 0.86f),
                labelOffset = new Vector2(0f, -34f),
                color = new Color(0.34f, 0.68f, 0.31f, 1f),
                description = "Placeholder forest zone."
            },
            new WaypointMapNode
            {
                id = "darkwood",
                displayName = "DARKWOOD",
                subtitle = "",
                sceneName = SceneNames.Darkwood,
                unlocked = true,
                normalizedPosition = new Vector2(0.44f, 0.45f),
                labelOffset = new Vector2(0f, -36f),
                color = new Color(0.78f, 0.35f, 0.28f, 1f),
                description = "Darkwood region."
            },
            new WaypointMapNode
            {
                id = "toujam",
                displayName = "TOUJAM BASIN",
                subtitle = "",
                sceneName = SceneNames.ToujamBasin,
                unlocked = true,
                normalizedPosition = new Vector2(0.72f, 0.55f),
                labelOffset = new Vector2(0f, -34f),
                color = new Color(0.78f, 0.62f, 0.02f, 1f),
                description = "Placeholder basin zone."
            },
            new WaypointMapNode
            {
                id = "ashen",
                displayName = "ASHEN WASTELAND",
                subtitle = "burned and blighted",
                sceneName = SceneNames.AshenWastelands,
                unlocked = true,
                normalizedPosition = new Vector2(0.94f, 0.74f),
                labelOffset = new Vector2(0f, -40f),
                color = new Color(0.85f, 0.42f, 0.18f, 1f),
                description = "A scorched wasteland consumed by ancient fire."
            },
        };
    }

    static WaypointMapConnection[] DefaultConnections()
    {
        return new[]
        {
            new WaypointMapConnection { fromNodeId = "sw", toNodeId = "gathering", dashed = true },
            new WaypointMapConnection { fromNodeId = "gathering", toNodeId = "brightwood" },
            new WaypointMapConnection { fromNodeId = "gathering", toNodeId = "darkwood" },
            new WaypointMapConnection { fromNodeId = "brightwood", toNodeId = "darkwood" },
            new WaypointMapConnection { fromNodeId = "darkwood", toNodeId = "toujam" },
            new WaypointMapConnection { fromNodeId = "toujam", toNodeId = "ashen" },
            new WaypointMapConnection { fromNodeId = "neimos", toNodeId = "darkwood", dashed = true },
        };
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
#endif
}
