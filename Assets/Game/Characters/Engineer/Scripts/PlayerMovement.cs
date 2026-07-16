using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpForce = 6f;
    public float rotationSpeed = 12f;
    public Camera cam;

    [Header("Rotation Lock")]
    public bool lockCharacterRotation = false;
    public bool captureStartRotationAsLock = true;
    public Vector3 lockedRotationEuler = Vector3.zero;

    [Header("Physics Stability")]
    public bool keepUpright = true;

    [Header("Movement Locks")]
    [Tooltip("Debug readout: movement is temporarily frozen by another system, such as spell commit.")]
    [SerializeField] private bool movementLocked = false;

    // ── Dodge Roll ────────────────────────────────────────────────
    [Header("Dodge Roll")]
    public float dodgeForce       = 14f;   // burst speed during dodge
    public float dodgeDuration    = 0.35f; // seconds of i-frames + movement burst
    public int   dodgeMaxCharges  = 2;     // maximum stored charges
    public float dodgeRecharge    = 5f;    // seconds to regain one charge
    // Assign a small ground-burst prefab (e.g. brbmuffins Technologies/.../SparksEffect.prefab)
    public GameObject dodgeVFX;

    private int   _dodgeCharges;
    private float _dodgeRechargeTimer;
    private bool  _isDodging;

    // ── Internals ─────────────────────────────────────────────────
    private Animator anim;
    private Rigidbody rb;
    private CharacterStats stats;
    private Health health;

    // Cached animator parameter names — avoids SetBool warnings on missing params
    private System.Collections.Generic.HashSet<string> _animParams =
        new System.Collections.Generic.HashSet<string>();

    private bool isGrounded = true;
    private bool inWater = false;

    private Vector3 moveDirection = Vector3.zero;
    private Quaternion targetRotation;
    private bool wantsMove = false;
    private bool jumpRequested = false;
    private float currentSpeed;
    private float movementLockUntil = -1f;

    void Start()
    {
        // ── Mirror: disable input on remote player objects ────────────────────
        // When no network session is running (editor solo play), any PlayerMovement
        // is treated as the local player.  Once a network session is active, only the
        // Mirror-designated local player may process input; scene-placed characters
        // with no NetworkIdentity are decorations and must disable themselves.
        var netId = GetComponent<NetworkIdentity>();
        bool networkActive = NetworkClient.active || NetworkServer.active;
        bool isLocal = networkActive
            ? (netId != null && netId.isLocalPlayer)   // net session: must be the local player
            : (netId == null || netId.isLocalPlayer);  // solo editor: anything goes
        if (!isLocal)
        {
            enabled = false;
            return;
        }

        anim   = GetComponentInChildren<Animator>();
        rb     = GetComponent<Rigidbody>();
        stats  = GetComponent<CharacterStats>();
        health = GetComponent<Health>();

        ConfigureRigidbody();

        // Movement is Rigidbody-driven (MovePosition). If the imported rig has Apply
        // Root Motion enabled, the run/sprint clip physically drags the mesh forward
        // while the Rigidbody drags the root — the mesh detaches and flies around the
        // camera's follow point. Force it off so the animation stays in-place.
        if (anim != null) anim.applyRootMotion = false;

        // Cache which parameters the controller actually has
        if (anim != null && anim.runtimeAnimatorController != null)
            foreach (var p in anim.parameters)
                _animParams.Add(p.name);

        // Auto-find camera and wire CameraFollow to this transform.
        if (cam == null) cam = Camera.main;
        if (cam == null && Camera.allCamerasCount > 0) cam = Camera.allCameras[0];
        if (cam != null)
        {
            // Prefer scene-placed CameraFollow anywhere in the scene to avoid creating a
            // second component with default settings that fights the scene-placed one.
            var follow = FindFirstObjectByType<CameraFollow>();
            if (follow == null)
                follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.cameraCollision = true;
            follow.collisionMask = ~0;
            follow.target = transform; // setter calls SnapToTarget() automatically
        }

        if (lockCharacterRotation && captureStartRotationAsLock)
            lockedRotationEuler = transform.rotation.eulerAngles;

        targetRotation = lockCharacterRotation
            ? UprightRotation(Quaternion.Euler(lockedRotationEuler))
            : UprightRotation(transform.rotation);
        currentSpeed   = moveSpeed;
        _dodgeCharges  = dodgeMaxCharges;

        SetAnimBool("isGrounded", true);
        SetAnimBool("inWater", false);
        SetAnimBool("isBackwards", false);
    }

    // Only call SetBool if the controller actually has that parameter
    void SetAnimBool(string param, bool value)
    {
        if (anim != null && _animParams.Contains(param))
            anim.SetBool(param, value);
    }

    void SetAnimTrigger(string param)
    {
        if (anim != null && _animParams.Contains(param))
            anim.SetTrigger(param);
    }

    // Returns true when the player is typing in any UI input field.
    // Also checks RodChatManager.IsOpen directly — the new Input System does NOT
    // consume key events when a TMP_InputField has focus, so WASD would still
    // move the character while typing without this second check.
    static bool IsTypingInUI()
    {
        var sel = EventSystem.current?.currentSelectedGameObject;
        return (sel != null && sel.GetComponent<TMP_InputField>() != null)
            || (RodChatManager.Instance != null && RodChatManager.Instance.IsOpen);
    }

    public void RequestMovementLock(float duration)
    {
        if (duration <= 0f)
            return;

        movementLockUntil = Mathf.Max(movementLockUntil, Time.time + duration);
        movementLocked = true;
        ClearMovementIntent();
        StopHorizontalMotion();
    }

    void Update()
    {
        if (health != null && health.IsDowned)
        {
            ClearMovementIntent();
            return;
        }

        // Yield all keyboard input to UI while player is typing
        if (IsTypingInUI())
        {
            ClearMovementIntent();
            return;
        }

        if (IsMovementLocked())
        {
            ClearMovementIntent();
            return;
        }

        Vector2 input = Vector2.zero;

        bool pressingW = Keyboard.current.wKey.isPressed;
        bool pressingS = Keyboard.current.sKey.isPressed;
        bool pressingA = Keyboard.current.aKey.isPressed;
        bool pressingD = Keyboard.current.dKey.isPressed;

        if (pressingW) input.y += 1;
        if (pressingS) input.y -= 1;
        if (pressingA) input.x -= 1;
        if (pressingD) input.x += 1;

        bool isSprinting = Keyboard.current.leftShiftKey.isPressed && input.sqrMagnitude > 0 && !pressingS;
        bool isMoving = input.sqrMagnitude > 0;
        bool isBackwards = pressingS;

        SetAnimBool("isMoving", isMoving);
        SetAnimBool("isSprinting", isSprinting);
        SetAnimBool("isBackwards", isBackwards);

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            jumpRequested = true;
        }

        wantsMove = isMoving;
        currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        if (stats != null) currentSpeed *= stats.MoveSpeedMultiplier;   // gear/attunement bonus

        // ── Dodge roll input (Left Alt or V) ─────────────────────
        bool dodgePressed = Keyboard.current.leftAltKey.wasPressedThisFrame
                         || Keyboard.current.vKey.wasPressedThisFrame;
        if (dodgePressed && _dodgeCharges > 0 && !_isDodging)
        {
            StartCoroutine(DodgeRoutine());
        }

        // Recharge one charge at a time
        if (_dodgeCharges < dodgeMaxCharges)
        {
            _dodgeRechargeTimer += Time.deltaTime;
            if (_dodgeRechargeTimer >= dodgeRecharge)
            {
                _dodgeRechargeTimer = 0f;
                _dodgeCharges++;
            }
        }

        if (cam != null)
        {
            Vector3 camForward = cam.transform.forward;
            Vector3 camRight   = cam.transform.right;
            camForward.y = 0; camForward.Normalize();
            camRight.y   = 0; camRight.Normalize();

            if (isMoving)
                moveDirection = (camForward * input.y + camRight * input.x).normalized;
        }

        // Character faces movement direction only. Mouse cursor is for ability aim, not rotation.
        if (!lockCharacterRotation && moveDirection.sqrMagnitude > 0.001f)
            targetRotation = Quaternion.LookRotation(moveDirection);
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (health != null && health.IsDowned)
        {
            ClearMovementIntent();
            StopHorizontalMotion();
            return;
        }

        if (keepUpright)
            KeepBodyUpright();

        if (IsMovementLocked())
        {
            ClearMovementIntent();
            StopHorizontalMotion();
            return;
        }

        if (jumpRequested)
        {
            jumpRequested = false;
            isGrounded = false;
            SetAnimBool("isGrounded", false);
            SetAnimTrigger("Jump");

            Vector3 velocity = rb.linearVelocity;
            if (velocity.y < 0f)
            {
                velocity.y = 0f;
                rb.linearVelocity = velocity;
            }

            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        if (!_isDodging)
        {
            Vector3 velocity = rb.linearVelocity;
            if (wantsMove)
            {
                Vector3 horizontalVelocity = moveDirection * currentSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
            }
            else if (isGrounded)
            {
                rb.linearVelocity = new Vector3(0f, velocity.y, 0f);
            }
        }

        if (lockCharacterRotation)
        {
            targetRotation = UprightRotation(Quaternion.Euler(lockedRotationEuler));
            rb.MoveRotation(targetRotation);
        }
        else
        {
            targetRotation = UprightRotation(targetRotation);
            Quaternion currentRotation = keepUpright ? UprightRotation(rb.rotation) : rb.rotation;
            rb.MoveRotation(Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    void ConfigureRigidbody()
    {
        if (rb == null)
            return;

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (!keepUpright)
            return;

        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.angularVelocity = Vector3.zero;
        rb.rotation = UprightRotation(rb.rotation);
    }

    void KeepBodyUpright()
    {
        Vector3 angularVelocity = rb.angularVelocity;
        angularVelocity.x = 0f;
        angularVelocity.z = 0f;
        rb.angularVelocity = angularVelocity;

        float tilt = Vector3.Angle(rb.transform.up, Vector3.up);
        if (tilt > 0.5f)
            rb.rotation = UprightRotation(rb.rotation);
    }

    static Quaternion UprightRotation(Quaternion rotation)
    {
        return Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
    }

    bool IsMovementLocked()
    {
        movementLocked = Time.time < movementLockUntil;
        return movementLocked;
    }

    void ClearMovementIntent()
    {
        wantsMove = false;
        jumpRequested = false;
        SetAnimBool("isMoving", false);
        SetAnimBool("isSprinting", false);
        SetAnimBool("isBackwards", false);
    }

    void StopHorizontalMotion()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (rb == null)
            return;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = 0f;
        velocity.z = 0f;
        rb.linearVelocity = velocity;
    }

    // ── Dodge Roll ────────────────────────────────────────────────
    // Returns the number of charges remaining — used by UI to draw stamina pips.
    public int DodgeCharges  => _dodgeCharges;
    public int DodgeMaxCharges => dodgeMaxCharges;

    private IEnumerator DodgeRoutine()
    {
        _isDodging = true;
        _dodgeCharges--;

        // Direction: current move direction, or backward if standing still
        Vector3 dir = moveDirection.sqrMagnitude > 0.01f ? moveDirection : -transform.forward;
        dir.y = 0;
        dir.Normalize();

        // Grant i-frames — must be set on the server (Health.TakeDamage runs server-side).
        // In a network session, send a Command; in editor solo-play write locally.
        if (health != null) health.isInvulnerable = true;   // local write (editor/solo)
        if (NetworkClient.active) CmdSetInvulnerable(true);

        SetAnimTrigger("dodge");

        // Spawn entry VFX at feet
        if (dodgeVFX != null)
        {
            GameObject fx = Instantiate(dodgeVFX, transform.position, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        // Burst movement over the dodge window
        float elapsed = 0f;
        while (elapsed < dodgeDuration && (health == null || !health.IsDowned))
        {
            rb.MovePosition(rb.position + dir * dodgeForce * Time.fixedDeltaTime);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Remove i-frames
        if (health != null) health.isInvulnerable = false;  // local write (editor/solo)
        if (NetworkClient.active) CmdSetInvulnerable(false);

        _isDodging = false;
    }

    // ── Server i-frame sync ───────────────────────────────────────────────────────
    // Sets isInvulnerable on the server-side Health so dodge i-frames are respected
    // by TakeDamage (which runs server-authoritative). The local write above covers
    // editor solo-play; the Command covers dedicated-server and hosted sessions.
    //
    // Anti-abuse: track last invuln-grant time and max window on the server.
    // A legit dodge grants invuln for ~dodgeDuration (0.35s); reject grants that arrive
    // faster than INVULN_MIN_INTERVAL or try to stay active longer than INVULN_MAX_WINDOW.
    const float INVULN_MIN_INTERVAL = 3.0f;  // must match dodgeRecharge (5s) minus tolerance
    const float INVULN_MAX_WINDOW   = 0.60f;  // max time invuln stays true server-side (2× dodgeDuration + buffer)
    float _lastInvulnGrantTime = -999f;

    [Command]
    public void CmdSetInvulnerable(bool value)
    {
        if (value)
        {
            // Reject if granted too recently (exploit: spam Command to keep invuln permanently)
            if (Time.time - _lastInvulnGrantTime < INVULN_MIN_INTERVAL)
            {
                Debug.LogWarning($"[COMBAT] {name}: CmdSetInvulnerable(true) rejected — too soon after last grant");
                return;
            }
            _lastInvulnGrantTime = Time.time;

            // Auto-clear after the max window even if CmdSetInvulnerable(false) never arrives
            StartCoroutine(AutoClearInvuln());
        }

        if (health != null) health.isInvulnerable = value;
    }

    [Server]
    System.Collections.IEnumerator AutoClearInvuln()
    {
        yield return new WaitForSeconds(INVULN_MAX_WINDOW);
        if (health != null && health.isInvulnerable)
        {
            health.isInvulnerable = false;
            Debug.Log($"[COMBAT] {name}: server auto-cleared stale invuln after {INVULN_MAX_WINDOW}s");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || HasWalkableContact(collision))
        {
            isGrounded = true;
            SetAnimBool("isGrounded", true);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || HasWalkableContact(collision))
        {
            isGrounded = true;
            SetAnimBool("isGrounded", true);
        }
    }

    static bool HasWalkableContact(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y >= 0.45f)
                return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            inWater = true;
            SetAnimBool("inWater", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            inWater = false;
            SetAnimBool("inWater", false);
        }
    }
}
