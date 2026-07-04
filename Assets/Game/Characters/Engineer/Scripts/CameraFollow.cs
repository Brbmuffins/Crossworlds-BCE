using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// ═══════════════════════════════════════════════════════════════════════════
//  CameraFollow — WoW-style 3rd-person camera
//
//  Right mouse held   → lock cursor, orbit camera
//  Left mouse held    -> no camera/movement action
//  Either released    → unlock cursor (can click UI)
//  Scroll wheel       → zoom in / out
//
//  Setting Target snaps the camera behind the character immediately —
//  no lerp from world origin on zone-in.
// ═══════════════════════════════════════════════════════════════════════════

public class CameraFollow : MonoBehaviour
{
    [Header("Distance")]
    public float distance    = 7f;
    public float minDistance = 1.5f;
    public float maxDistance = 20f;
    public float zoomSpeed   = 4f;

    [Header("Orbit")]
    [Tooltip("Degrees per pixel of mouse movement. 0.15–0.35 is typical MMO range.")]
    public float mouseSensitivity = 0.25f;
    public float minPitch = -20f;
    public float maxPitch =  70f;

    [Header("Follow")]
    public float heightOffset = 1.6f;

    [Header("Collision")]
    public bool cameraCollision = true;
    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.28f;
    public float collisionBuffer = 0.15f;
    public float collisionSmoothSpeed = 18f;

    // ── Target property — snaps camera immediately on assign ──────────────
    Transform _target;
    public Transform target
    {
        get => _target;
        set
        {
            _target = value;
            if (_target != null) SnapToTarget();
        }
    }

    // Read by PlayerMovement
    public float Yaw            => _yaw;
    public bool  RightMouseHeld => _rightHeld && !_typingInUI;

    float   _yaw;
    float   _pitch = 18f;
    bool    _rightHeld;
    bool    _typingInUI;
    bool    _prevLookActive;   // detect first frame of look to discard stale delta
    Vector3 _smoothPos;
    float   _currentCollisionDistance;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Start()
    {
        // Snap if target was set before Start (e.g. via Inspector)
        if (_target != null) SnapToTarget();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void LateUpdate()
    {
        if (_target == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        _rightHeld = mouse.rightButton.isPressed;

        // Block orbit when typing OR when chat is open — the second check covers the
        // one-frame gap between ActivateInputField() and currentSelectedGameObject being set.
        var selGO = EventSystem.current?.currentSelectedGameObject;
        _typingInUI = (selGO != null && selGO.GetComponent<TMPro.TMP_InputField>() != null)
                   || (RodChatManager.Instance != null && RodChatManager.Instance.IsOpen);
        bool lookActive = _rightHeld && !_typingInUI;

        // ── Cursor lock / unlock ──────────────────────────────────────────
        if (lookActive && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        else if (!lookActive && Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // ── Camera rotation ───────────────────────────────────────────────
        // mouse.delta accumulates while cursor is free. On the FIRST frame we enter
        // look mode that stale delta would cause a violent swing — discard it.
        bool justEnteredLook = lookActive && !_prevLookActive;
        _prevLookActive = lookActive;

        if (lookActive && !justEnteredLook)
        {
            Vector2 delta = mouse.delta.ReadValue();
            // Clamp per-frame delta to prevent single-frame spikes
            delta.x = Mathf.Clamp(delta.x, -50f, 50f);
            delta.y = Mathf.Clamp(delta.y, -50f, 50f);
            _yaw   += delta.x * mouseSensitivity;
            _pitch -= delta.y * mouseSensitivity;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        // ── Zoom ──────────────────────────────────────────────────────────
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
            distance = Mathf.Clamp(distance - scroll * zoomSpeed * 0.01f,
                                   minDistance, maxDistance);

        // ── Position — instant follow, no lag ─────────────────────────────
        // Positional smoothing causes the character to drift out of frame.
        // WoW-style cameras follow position immediately; smoothness comes from
        // character animation, not camera lag.
        _smoothPos = _target.position;

        Quaternion rot    = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    offset = rot * new Vector3(0f, 0f, -distance);
        Vector3    lookAt = _smoothPos + Vector3.up * heightOffset;

        Vector3 desiredPosition = lookAt + offset;
        transform.position = ResolveCameraPosition(lookAt, desiredPosition);
        transform.LookAt(lookAt);
    }

    // ── Public helpers ────────────────────────────────────────────────────

    /// <summary>Instantly places the camera behind the target. Call after setting target.</summary>
    public void SnapToTarget()
    {
        if (_target == null) return;

        _yaw       = _target.eulerAngles.y;
        _smoothPos = _target.position;

        Quaternion rot    = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    offset = rot * new Vector3(0f, 0f, -distance);
        Vector3    lookAt = _smoothPos + Vector3.up * heightOffset;

        Vector3 desiredPosition = lookAt + offset;
        _currentCollisionDistance = distance;
        transform.position = ResolveCameraPosition(lookAt, desiredPosition, true);
        transform.LookAt(lookAt);
    }

    Vector3 ResolveCameraPosition(Vector3 lookAt, Vector3 desiredPosition, bool snap = false)
    {
        if (!cameraCollision)
            return desiredPosition;

        Vector3 toCamera = desiredPosition - lookAt;
        float desiredDistance = toCamera.magnitude;
        if (desiredDistance <= 0.001f)
            return desiredPosition;

        Vector3 direction = toCamera / desiredDistance;
        float clearDistance = desiredDistance;

        RaycastHit[] hits = Physics.SphereCastAll(
            lookAt,
            collisionRadius,
            direction,
            desiredDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore);

        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (_target != null && hit.collider.transform.IsChildOf(_target))
                    continue;

                clearDistance = Mathf.Max(0.05f, hit.distance - collisionBuffer);
                break;
            }
        }

        float targetDistance = Mathf.Clamp(clearDistance, 0.05f, desiredDistance);
        _currentCollisionDistance = snap
            ? targetDistance
            : Mathf.Lerp(
                _currentCollisionDistance <= 0f ? targetDistance : _currentCollisionDistance,
                targetDistance,
                1f - Mathf.Exp(-collisionSmoothSpeed * Time.deltaTime));

        return lookAt + direction * _currentCollisionDistance;
    }
}
