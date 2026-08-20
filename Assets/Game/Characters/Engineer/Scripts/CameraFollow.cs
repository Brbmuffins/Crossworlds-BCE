using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// ═══════════════════════════════════════════════════════════════════════════
//  CameraFollow — Smite / MOBA-style 3rd-person camera
//
//  Right mouse drag   → orbit (yaw + pitch), cursor locked during the drag
//                       (only when not aiming — AbilityCaster owns RMB while aiming)
//  Scroll wheel       → zoom in / out
//  Free cursor otherwise; AbilityCaster owns aim indicator positioning.
//
//  GM free camera:
//    WASD + Space/Ctrl → move, right mouse → look
//    Shift → fast, Alt → slow precision movement
//
//  Target wiring:
//    • Fast path: PlayerMovement.Start() calls follow.target = transform
//    • Fallback:  FindLocalPlayer() coroutine polls every 0.2 s until found
// ═══════════════════════════════════════════════════════════════════════════
public class CameraFollow : MonoBehaviour
{
    public const string ZoomDistancePrefKey = "CameraZoomDistance";
    public const float DefaultZoomDistance = 7f;
    public const float DefaultFreeCameraSpeed = RodPlayerAuth.DefaultFreeCameraSpeed;
    public const float MinFreeCameraSpeed = RodPlayerAuth.MinFreeCameraSpeed;
    public const float MaxFreeCameraSpeed = RodPlayerAuth.MaxFreeCameraSpeed;

    [Header("Distance")]
    public float distance    = DefaultZoomDistance;
    public float minDistance = 1.5f;
    public float maxDistance = 20f;
    public float zoomSpeed   = 4f;

    [Header("Orbit (middle mouse)")]
    [Tooltip("Degrees per pixel of mouse movement.")]
    public float mouseSensitivity = 0.25f;
    public float minPitch = -20f;
    public float maxPitch =  70f;

    [Header("Follow")]
    public float heightOffset = 1.6f;

    [Header("Collision")]
    public bool      cameraCollision     = true;
    public LayerMask collisionMask       = ~0;
    public float     collisionRadius     = 0.28f;
    public float     collisionBuffer     = 0.15f;
    public float     collisionSmoothSpeed = 18f;

    [Header("GM Free Camera")]
    [SerializeField] float freeCameraMoveSpeed = DefaultFreeCameraSpeed;
    [SerializeField, Min(1f)] float freeCameraBoostMultiplier = 3f;
    [SerializeField, Range(0.05f, 1f)] float freeCameraPrecisionMultiplier = 0.25f;

    // Set by PlayerMovement.Start() or by FindLocalPlayer coroutine.
    Transform _target;
    public Transform target
    {
        get => _target;
        set
        {
            if (_target == value)
                return;

            if (_freeCameraActive && _target != null)
                _target.GetComponent<PlayerMovement>()
                    ?.SetGmFreeCameraInputLocked(false);

            _target = value;
            if (_target == null)
                return;

            if (_freeCameraActive)
                _target.GetComponent<PlayerMovement>()
                    ?.SetGmFreeCameraInputLocked(true);
            else
                SnapToTarget();
        }
    }

    public float Yaw => _yaw;
    public static bool IsAnyFreeCameraActive { get; private set; }
    public bool IsFreeCameraActive => _freeCameraActive;
    public float FreeCameraMoveSpeed => freeCameraMoveSpeed;

    float _yaw;
    float _pitch = 18f;
    bool  _prevOrbitActive;
    float _currentCollisionDistance;
    bool _freeCameraActive;
    float _returnYaw;
    float _returnPitch;
    readonly Dictionary<Canvas, bool> _freeCameraCanvasStates = new();
    float _nextFreeCameraUiSweep;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        LoadZoomPreference();

        if (_target != null) SnapToTarget();

        StartCoroutine(FindLocalPlayer());
    }

    // Polls until a local player is found. Stops once target is confirmed.
    IEnumerator FindLocalPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            if (_target != null) yield break;

            // Networked: find the Mirror local player
            foreach (var ni in FindObjectsByType<NetworkIdentity>(
                         FindObjectsInactive.Exclude))
            {
                if (!ni.isLocalPlayer) continue;
                if (ni.GetComponent<PlayerMovement>() == null) continue;
                target = ni.transform;   // setter calls SnapToTarget
                yield break;
            }

            // Solo / editor (no Mirror session): find any PlayerMovement
            if (!NetworkClient.active && !NetworkServer.active)
            {
                var pm = FindAnyObjectByType<PlayerMovement>();
                if (pm != null) { target = pm.transform; yield break; }
            }
        }
    }

    void LateUpdate()
    {
        if (_target == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        var selGO       = EventSystem.current?.currentSelectedGameObject;
        bool typingInUI = (selGO != null && selGO.GetComponent<TMPro.TMP_InputField>() != null)
                        || (RodChatManager.Instance != null && RodChatManager.Instance.IsOpen);

        if (_freeCameraActive)
        {
            bool freeCameraTyping = RodChatManager.Instance != null &&
                                    RodChatManager.Instance.IsOpen;
            UpdateFreeCamera(mouse, freeCameraTyping);
            return;
        }

        // Smite-style: hold RIGHT mouse and drag to rotate the camera. Only when NOT
        // aiming an ability — AbilityCaster owns RMB (cancel) while an indicator is up —
        // and not typing in chat.
        bool orbitActive = mouse.rightButton.isPressed
                        && !typingInUI
                        && !AbilityCaster.IsAimingLocally;

        // Lock & hide the cursor while dragging so rotation is continuous and the pointer
        // can't slip off-screen; restore the free aiming cursor on release.
        if (orbitActive && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        else if (!orbitActive && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        bool justEntered = orbitActive && !_prevOrbitActive;
        _prevOrbitActive = orbitActive;

        if (orbitActive && !justEntered)
        {
            Vector2 delta = mouse.delta.ReadValue();
            delta.x = Mathf.Clamp(delta.x, -50f, 50f);
            delta.y = Mathf.Clamp(delta.y, -50f, 50f);
            _yaw   += delta.x * mouseSensitivity;
            _pitch -= delta.y * mouseSensitivity;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        float scroll = mouse.scroll.ReadValue().y;
        bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (Mathf.Abs(scroll) > 0.01f && !AbilityCaster.IsAimingLocally && !pointerOverUi)
            SetZoomDistance(distance - scroll * zoomSpeed * 0.01f);

        Vector3    pos     = _target.position;
        Quaternion rot     = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    offset  = rot * new Vector3(0f, 0f, -distance);
        Vector3    lookAt  = pos + Vector3.up * heightOffset;
        Vector3    desired = lookAt + offset;

        transform.position = ResolveCameraPosition(lookAt, desired);
        transform.LookAt(lookAt);
    }

    public void SetFreeCameraEnabled(bool enabled)
    {
        if (_freeCameraActive == enabled)
            return;

        _freeCameraActive = enabled;
        IsAnyFreeCameraActive = enabled;
        PlayerMovement movement = _target != null
            ? _target.GetComponent<PlayerMovement>()
            : null;
        movement?.SetGmFreeCameraInputLocked(enabled);

        if (enabled)
        {
            HideFreeCameraUi();
            _returnYaw = _yaw;
            _returnPitch = _pitch;

            Vector3 euler = transform.rotation.eulerAngles;
            _yaw = euler.y;
            _pitch = Mathf.Clamp(NormalizeAngle(euler.x), minPitch, maxPitch);
            _prevOrbitActive = false;
            return;
        }

        RestoreCursor();
        RestoreFreeCameraUi();
        _yaw = _returnYaw;
        _pitch = _returnPitch;
        if (_target != null)
            SnapToTarget(preserveOrbit: true);
    }

    public void SetFreeCameraSpeed(float speed)
    {
        freeCameraMoveSpeed = Mathf.Clamp(
            speed, MinFreeCameraSpeed, MaxFreeCameraSpeed);
    }

    void UpdateFreeCamera(Mouse mouse, bool typingInUI)
    {
        KeepFreeCameraUiHidden();

        bool lookActive = mouse.rightButton.isPressed && !typingInUI;
        UpdateOrbitInput(mouse, lookActive);
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        var keyboard = Keyboard.current;
        if (typingInUI || keyboard == null)
            return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (RodChatManager.Instance != null)
                RodChatManager.Instance.RequestGmFreeCameraExit();
            else
                SetFreeCameraEnabled(false);
            return;
        }

        Vector3 input = Vector3.zero;
        if (keyboard.wKey.isPressed) input += transform.forward;
        if (keyboard.sKey.isPressed) input -= transform.forward;
        if (keyboard.dKey.isPressed) input += transform.right;
        if (keyboard.aKey.isPressed) input -= transform.right;
        if (keyboard.spaceKey.isPressed || keyboard.eKey.isPressed) input += Vector3.up;
        if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed ||
            keyboard.qKey.isPressed || keyboard.cKey.isPressed) input -= Vector3.up;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        float speed = freeCameraMoveSpeed;
        if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
            speed *= freeCameraBoostMultiplier;
        if (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed)
            speed *= freeCameraPrecisionMultiplier;

        transform.position += input * speed * Time.unscaledDeltaTime;
    }

    void HideFreeCameraUi()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>(
                     FindObjectsInactive.Include))
        {
            if (canvas == null)
                continue;

            if (!_freeCameraCanvasStates.ContainsKey(canvas))
                _freeCameraCanvasStates.Add(canvas, canvas.enabled);
            canvas.enabled = false;
        }

        _nextFreeCameraUiSweep = Time.unscaledTime + 0.25f;
    }

    void KeepFreeCameraUiHidden()
    {
        foreach (Canvas canvas in _freeCameraCanvasStates.Keys)
        {
            if (canvas != null)
                canvas.enabled = false;
        }

        if (Time.unscaledTime < _nextFreeCameraUiSweep)
            return;

        HideFreeCameraUi();
    }

    void RestoreFreeCameraUi()
    {
        foreach (KeyValuePair<Canvas, bool> state in _freeCameraCanvasStates)
        {
            if (state.Key != null)
                state.Key.enabled = state.Value;
        }

        _freeCameraCanvasStates.Clear();
    }

    void UpdateOrbitInput(Mouse mouse, bool orbitActive)
    {
        if (orbitActive && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!orbitActive)
        {
            RestoreCursor();
        }

        bool justEntered = orbitActive && !_prevOrbitActive;
        _prevOrbitActive = orbitActive;
        if (!orbitActive || justEntered)
            return;

        Vector2 delta = mouse.delta.ReadValue();
        delta.x = Mathf.Clamp(delta.x, -50f, 50f);
        delta.y = Mathf.Clamp(delta.y, -50f, 50f);
        _yaw += delta.x * mouseSensitivity;
        _pitch = Mathf.Clamp(
            _pitch - delta.y * mouseSensitivity, minPitch, maxPitch);
    }

    static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    static void RestoreCursor()
    {
        if (Cursor.lockState != CursorLockMode.None)
            Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDestroy()
    {
        if (!_freeCameraActive)
            return;

        PlayerMovement movement = _target != null
            ? _target.GetComponent<PlayerMovement>()
            : null;
        movement?.SetGmFreeCameraInputLocked(false);
        IsAnyFreeCameraActive = false;
        RestoreFreeCameraUi();
        RestoreCursor();
    }

    public void SetZoomDistance(float value, bool savePreference = true)
    {
        distance = Mathf.Clamp(value, minDistance, maxDistance);

        if (!savePreference) return;

        PlayerPrefs.SetFloat(ZoomDistancePrefKey, distance);
        PlayerPrefs.Save();
    }

    public void SetZoomNormalized(float normalized, bool savePreference = true)
    {
        SetZoomDistance(Mathf.Lerp(minDistance, maxDistance, Mathf.Clamp01(normalized)), savePreference);
    }

    public float GetZoomNormalized()
    {
        return Mathf.InverseLerp(minDistance, maxDistance, distance);
    }

    void LoadZoomPreference()
    {
        SetZoomDistance(PlayerPrefs.GetFloat(ZoomDistancePrefKey, distance), savePreference: false);
    }

    public void SnapToTarget(bool preserveOrbit = false)
    {
        if (_target == null) return;
        if (!preserveOrbit)
            _yaw = _target.eulerAngles.y;

        Vector3    pos    = _target.position;
        Quaternion rot    = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    offset = rot * new Vector3(0f, 0f, -distance);
        Vector3    lookAt = pos + Vector3.up * heightOffset;

        _currentCollisionDistance = distance;
        transform.position = ResolveCameraPosition(lookAt, lookAt + offset, snap: true);
        transform.LookAt(lookAt);
    }

    Vector3 ResolveCameraPosition(Vector3 lookAt, Vector3 desiredPosition, bool snap = false)
    {
        if (!cameraCollision) return desiredPosition;

        Vector3 toCamera    = desiredPosition - lookAt;
        float   desiredDist = toCamera.magnitude;
        if (desiredDist <= 0.001f) return desiredPosition;

        Vector3 dir       = toCamera / desiredDist;
        float   clearDist = desiredDist;

        RaycastHit[] hits = ZonePhysics.SphereCastAll(
            gameObject, lookAt, collisionRadius, dir, desiredDist,
            collisionMask, QueryTriggerInteraction.Ignore);

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (_target != null && hit.collider.transform.IsChildOf(_target)) continue;
                clearDist = Mathf.Max(0.05f, hit.distance - collisionBuffer);
                break;
            }
        }

        float targetDist = Mathf.Clamp(clearDist, 0.05f, desiredDist);
        _currentCollisionDistance = snap
            ? targetDist
            : Mathf.Lerp(
                _currentCollisionDistance <= 0f ? targetDist : _currentCollisionDistance,
                targetDist,
                1f - Mathf.Exp(-collisionSmoothSpeed * Time.deltaTime));

        return lookAt + dir * _currentCollisionDistance;
    }
}
