using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// General-purpose return-to-hub waypoint.
///
/// Put this on the portal/waypoint root. The object should have a NetworkIdentity
/// and at least one Collider on itself or a child. While the local player is near,
/// the waypoint can be clicked and/or activated with a key, then asks for
/// confirmation before sending everyone back to the configured hub scene.
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class HubReturnTrigger : NetworkBehaviour
{
    [Header("Hub Scene")]
    [Tooltip("Scene name exactly as it appears in Build Settings.")]
    public string hubSceneName = SceneNames.Hub;

    [Header("Interaction")]
    [Min(0.5f)] public float interactionRange = 5f;
    public bool allowMouseClick = true;
    public bool allowKeyboardInteract = true;
    public Key interactKey = Key.E;
    [Tooltip("If enabled, walking into the trigger opens the confirmation immediately.")]
    public bool returnOnTriggerEnter = false;
    public LayerMask clickableLayers = ~0;
    [Min(1f)] public float clickRaycastDistance = 200f;

    [Header("Prompt")]
    public bool showWorldPrompt = true;
    public string promptText = "Click to Return to HUB";
    public Vector3 promptOffset = new Vector3(0f, 3.2f, 0f);

    [Header("Confirmation")]
    public bool askForConfirmation = true;
    public string confirmationTitle = "Return to HUB?";
    [TextArea(2, 4)]
    public string confirmationMessage = "Leave this area and return to HUB?";
    public string confirmButtonText = "Return";
    public string cancelButtonText = "Stay";

    [Header("Return Behavior")]
    public bool saveProgressBeforeReturn = true;
    public bool runArenaCleanup = true;

    [Header("Hub Arrival")]
    public bool placePlayerAtHubArrival = true;
    public string hubArrivalSpawnId = HubReturnSpawnPoint.DefaultSpawnId;
    public bool useHubArrivalRotation = true;

    bool _returning;
    bool _playerNear;
    Transform _localPlayer;
    GameObject _promptObject;
    TextMeshPro _promptLabel;

    void Awake()
    {
        if (IsAlreadyInHubScene())
        {
            enabled = false;
            return;
        }

        AttachChildTriggerForwarders();
    }

    void Start()
    {
        if (showWorldPrompt)
            BuildWorldPrompt();
    }

    void Update()
    {
        if (_returning) return;

        if (_localPlayer == null)
            _localPlayer = FindLocalPlayer();
        _playerNear = _localPlayer != null &&
                      Vector3.Distance(transform.position, _localPlayer.position) <= interactionRange;

        SetPromptVisible(_playerNear);
        if (!_playerNear) return;

        if (allowKeyboardInteract && WasInteractPressed() && !IsTypingInInputField())
        {
            OpenConfirmationOrReturn();
            return;
        }

        if (allowMouseClick && WasClicked())
            OpenConfirmationOrReturn();
    }

    void OnTriggerEnter(Collider other)
    {
        HandleTriggerEnter(other);
    }

    internal void HandleTriggerEnter(Collider other)
    {
        if (!returnOnTriggerEnter || _returning) return;
        if (!IsLocalPlayerCollider(other)) return;

        _localPlayer = GetPlayerTransform(other);
        OpenConfirmationOrReturn();
    }

    void OpenConfirmationOrReturn()
    {
        if (_returning) return;

        if (askForConfirmation)
        {
            HubReturnConfirmationUI.Show(
                confirmationTitle,
                confirmationMessage,
                confirmButtonText,
                cancelButtonText,
                RequestReturnToHub);
            return;
        }

        RequestReturnToHub();
    }

    void RequestReturnToHub()
    {
        if (_returning) return;
        _returning = true;
        SetPromptVisible(false);

        if (saveProgressBeforeReturn)
            PlayerProgressManager.Local?.SaveProgress();

        PrepareHubArrival(hubSceneName);

        if (NetworkServer.active)
        {
            EndArenaSessionIfNeeded();
            ChangeToHubScene(hubSceneName);
            return;
        }

        if (NetworkClient.active)
        {
            CmdRequestReturnToHub(hubSceneName);
            return;
        }

        Transform localPlayer = _localPlayer != null ? _localPlayer : FindLocalPlayer();
        if (localPlayer != null)
            HubReturnArrival.CarryLocalPlayer(localPlayer.gameObject);

        SceneManager.LoadScene(hubSceneName);
    }

    [Command(requiresAuthority = false)]
    void CmdRequestReturnToHub(string requestedHubSceneName, NetworkConnectionToClient sender = null)
    {
        if (string.IsNullOrWhiteSpace(requestedHubSceneName))
        {
            Debug.LogWarning("[HUB] Return request ignored: hub scene name is empty.");
            return;
        }

        EndArenaSessionIfNeeded();
        PrepareHubArrival(requestedHubSceneName);
        ChangeToHubScene(requestedHubSceneName);
    }

    void PrepareHubArrival(string sceneName)
    {
        if (!placePlayerAtHubArrival) return;

        HubReturnArrival.Request(sceneName, hubArrivalSpawnId, useHubArrivalRotation);
    }

    void EndArenaSessionIfNeeded()
    {
        if (!runArenaCleanup) return;

        ArenaSessionController.Instance?.EndSession();

        var spawner = FindAnyObjectByType<WaveSpawner>();
        spawner?.StopWaves();
    }

    static void ChangeToHubScene(string sceneName)
    {
        if (NetworkManager.singleton != null && NetworkServer.active)
        {
            NetworkManager.singleton.ServerChangeScene(sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

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

    bool IsAlreadyInHubScene()
    {
        if (string.IsNullOrWhiteSpace(hubSceneName)) return false;

        var activeScene = SceneManager.GetActiveScene();
        return string.Equals(activeScene.name, hubSceneName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(activeScene.path, hubSceneName, StringComparison.OrdinalIgnoreCase);
    }

    bool IsLocalPlayerCollider(Collider other)
    {
        var identity = other.GetComponentInParent<NetworkIdentity>();
        if (identity != null)
            return identity.isLocalPlayer || (!NetworkClient.active && identity.CompareTag("Player"));

        return HasPlayerTagInParents(other.transform);
    }

    Transform GetPlayerTransform(Collider other)
    {
        var identity = other.GetComponentInParent<NetworkIdentity>();
        if (identity != null) return identity.transform;

        for (Transform current = other.transform; current != null; current = current.parent)
            if (current.CompareTag("Player"))
                return current;

        return null;
    }

    static bool HasPlayerTagInParents(Transform start)
    {
        for (Transform current = start; current != null; current = current.parent)
            if (current.CompareTag("Player"))
                return true;

        return false;
    }

    static Transform FindLocalPlayer()
    {
        var identities = FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude);
        foreach (var identity in identities)
            if (identity.isLocalPlayer)
                return identity.transform;

        var taggedPlayer = GameObject.FindWithTag("Player");
        return taggedPlayer != null ? taggedPlayer.transform : null;
    }

    void BuildWorldPrompt()
    {
        if (_promptObject != null) return;

        _promptObject = new GameObject("HubReturnPrompt");
        _promptObject.transform.SetParent(transform, false);
        _promptObject.transform.localPosition = promptOffset;
        _promptObject.AddComponent<RodBillboard>();

        _promptLabel = _promptObject.AddComponent<TextMeshPro>();
        _promptLabel.text = promptText;
        _promptLabel.alignment = TextAlignmentOptions.Center;
        _promptLabel.fontSize = 3f;
        _promptLabel.richText = true;
        _promptLabel.raycastTarget = false;
        _promptLabel.textWrappingMode = TextWrappingModes.NoWrap;
        _promptLabel.color = Color.white;

        SetPromptVisible(false);
    }

    void SetPromptVisible(bool visible)
    {
        if (_promptObject != null && _promptObject.activeSelf != visible)
            _promptObject.SetActive(visible);
    }

    void AttachChildTriggerForwarders()
    {
        var colliders = GetComponentsInChildren<Collider>(true);
        foreach (var col in colliders)
        {
            if (col.transform == transform) continue;
            if (!col.isTrigger) continue;

            var forwarder = col.GetComponent<HubReturnTriggerForwarder>();
            if (forwarder == null)
                forwarder = col.gameObject.AddComponent<HubReturnTriggerForwarder>();

            forwarder.Owner = this;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
#endif
}

public sealed class HubReturnTriggerForwarder : MonoBehaviour
{
    public HubReturnTrigger Owner { get; set; }

    void OnTriggerEnter(Collider other)
    {
        Owner?.HandleTriggerEnter(other);
    }
}

sealed class HubReturnConfirmationUI : MonoBehaviour
{
    static HubReturnConfirmationUI _instance;

    CanvasGroup _group;
    TextMeshProUGUI _title;
    TextMeshProUGUI _message;
    TextMeshProUGUI _confirmLabel;
    TextMeshProUGUI _cancelLabel;
    Button _confirmButton;
    Button _cancelButton;
    Action _onConfirm;
    bool _open;

    public static void Show(string title, string message, string confirmText, string cancelText, Action onConfirm)
    {
        EnsureInstance().Open(title, message, confirmText, cancelText, onConfirm);
    }

    static HubReturnConfirmationUI EnsureInstance()
    {
        if (_instance != null) return _instance;

        var go = new GameObject("HubReturnConfirmationUI");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<HubReturnConfirmationUI>();
        return _instance;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        Build();
        HideImmediate();
    }

    void Update()
    {
        if (!_open) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
            Hide();
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            Confirm();
    }

    void Open(string title, string message, string confirmText, string cancelText, Action onConfirm)
    {
        EnsureEventSystem();

        _title.text = string.IsNullOrWhiteSpace(title) ? "Return to HUB?" : title;
        _message.text = string.IsNullOrWhiteSpace(message) ? "Leave this area and return to HUB?" : message;
        _confirmLabel.text = string.IsNullOrWhiteSpace(confirmText) ? "Return" : confirmText;
        _cancelLabel.text = string.IsNullOrWhiteSpace(cancelText) ? "Stay" : cancelText;
        _onConfirm = onConfirm;

        gameObject.SetActive(true);
        _group.alpha = 1f;
        _group.interactable = true;
        _group.blocksRaycasts = true;
        _open = true;
        _confirmButton.Select();
    }

    void Confirm()
    {
        if (!_open) return;

        Action callback = _onConfirm;
        Hide();
        callback?.Invoke();
    }

    void Hide()
    {
        _onConfirm = null;
        _open = false;
        HideImmediate();
    }

    void HideImmediate()
    {
        if (_group != null)
        {
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        _group = gameObject.AddComponent<CanvasGroup>();

        var overlay = MakeRect("Overlay", transform);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        var overlayImage = overlay.gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.62f);

        var panel = MakeRect("Panel", overlay);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(560f, 300f);
        panel.anchoredPosition = Vector2.zero;
        var panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.09f, 0.11f, 0.96f);

        _title = MakeText("Title", panel, 32f, FontStyles.Bold);
        SetRect(_title.rectTransform, new Vector2(0f, 0.72f), new Vector2(1f, 0.92f), 32f, 0f, 32f, 0f);
        _title.alignment = TextAlignmentOptions.Center;

        _message = MakeText("Message", panel, 22f, FontStyles.Normal);
        SetRect(_message.rectTransform, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.7f), 0f, 0f, 0f, 0f);
        _message.alignment = TextAlignmentOptions.Center;
        _message.enableWordWrapping = true;

        _confirmButton = MakeButton("ReturnButton", panel, new Color(0.24f, 0.63f, 0.42f, 1f), out _confirmLabel);
        SetRect(_confirmButton.GetComponent<RectTransform>(), new Vector2(0.12f, 0.12f), new Vector2(0.46f, 0.29f), 0f, 0f, 0f, 0f);
        _confirmButton.onClick.AddListener(Confirm);

        _cancelButton = MakeButton("StayButton", panel, new Color(0.23f, 0.25f, 0.3f, 1f), out _cancelLabel);
        SetRect(_cancelButton.GetComponent<RectTransform>(), new Vector2(0.54f, 0.12f), new Vector2(0.88f, 0.29f), 0f, 0f, 0f, 0f);
        _cancelButton.onClick.AddListener(Hide);
    }

    static RectTransform MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static TextMeshProUGUI MakeText(string name, Transform parent, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    static Button MakeButton(string name, Transform parent, Color color, out TextMeshProUGUI label)
    {
        var rect = MakeRect(name, parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;

        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        label = MakeText("Label", rect, 22f, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        SetRect(label.rectTransform, Vector2.zero, Vector2.one, 0f, 0f, 0f, 0f);

        return button;
    }

    static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, float left, float bottom, float right, float top)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule), typeof(SingleEventSystem));
        DontDestroyOnLoad(go);
    }
}
