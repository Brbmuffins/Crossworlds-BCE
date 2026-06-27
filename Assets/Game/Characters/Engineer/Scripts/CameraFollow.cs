using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// ═══════════════════════════════════════════════════════════════════════════
//  CameraFollow — WoW-style 3rd-person camera
//
//  Right mouse held   → lock cursor, orbit camera
//  Left mouse held    → lock cursor, orbit camera (character faces cam on move)
//  Either released    → unlock cursor (can click UI)
//  Scroll wheel       → zoom in / out
//  Both mouse held    → auto-walk forward (handled in PlayerMovement)
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
    public float heightOffset    = 1.6f;
    public float positionDamping = 12f;

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
    public bool  RightMouseHeld => _rightHeld &&
        !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());

    float   _yaw;
    float   _pitch = 18f;
    bool    _rightHeld;
    bool    _leftHeld;
    Vector3 _smoothPos;

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
        _leftHeld  = mouse.leftButton.isPressed;

        var selGO = EventSystem.current?.currentSelectedGameObject;
        bool typingInUI = selGO != null && selGO.GetComponent<TMPro.TMP_InputField>() != null;
        bool overUI     = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool lookActive = (_rightHeld || _leftHeld) && !overUI && !typingInUI;

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
        // mouse.delta is per-frame pixels — do NOT multiply by Time.deltaTime
        if (lookActive)
        {
            Vector2 delta = mouse.delta.ReadValue();
            _yaw   += delta.x * mouseSensitivity;
            _pitch -= delta.y * mouseSensitivity;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        // ── Zoom ──────────────────────────────────────────────────────────
        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
            distance = Mathf.Clamp(distance - scroll * zoomSpeed * 0.01f,
                                   minDistance, maxDistance);

        // ── Position ──────────────────────────────────────────────────────
        _smoothPos = Vector3.Lerp(_smoothPos, _target.position, positionDamping * Time.deltaTime);

        Quaternion rot    = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    offset = rot * new Vector3(0f, 0f, -distance);
        Vector3    lookAt = _smoothPos + Vector3.up * heightOffset;

        transform.position = lookAt + offset;
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

        transform.position = lookAt + offset;
        transform.LookAt(lookAt);
    }
}
