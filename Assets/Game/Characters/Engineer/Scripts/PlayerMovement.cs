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

    [Header("Animation")]
    [Tooltip("Disable for characters that turn around instead of playing a backward-walk clip.")]
    public bool useBackwardMovementAnimation = true;

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
    private Coroutine abilityMovementRoutine;

    void Start()
    {
        // ── Mirror: disable input on remote player objects ────────────────────
        // When no network session is running (editor solo play), any PlayerMovement
        // is treated as the local player.  Once a network session is active, only the
        // Mirror-designated local player may process input; scene-placed characters
        // with no NetworkIdentity are decorations and must disable themselves.
        var netId = GetComponent<NetworkIdentity>();
        bool networkActive = NetworkClient.active || NetworkServer.active;
        bool isLocal = !networkActive
            || (netId != null && netId.isLocalPlayer); // net session: must be the local player
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

        // ROADMAP 6.9: with zones loaded additively there can be several cameras alive
        // at once. Let the director enable only the one for this player's zone BEFORE
        // resolving — Camera.allCameras returns enabled cameras only, so after this
        // there is exactly one to find.
#if UNITY_EDITOR || !UNITY_SERVER
        ZoneCameraDirector.EnsureExists(transform);
#endif

        // Auto-find camera and wire CameraFollow to this transform.
        if (cam == null) cam = ResolveCamera();
        if (cam != null)
        {
            // Prefer a CameraFollow already on the camera we chose. FindFirstObjectByType
            // is global, so with several zones loaded it can return another zone's rig and
            // then drive a camera nobody is looking through (ROADMAP 6.5 / 6.9).
            var follow = cam.GetComponent<CameraFollow>();
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

    /// <summary>
    /// Moves this locally controlled player to a spell's selected ground point.
    /// NetworkTransform propagates the Rigidbody position to the server and peers.
    /// </summary>
    public void MoveToAbilityTarget(
        Vector3 destination,
        float speed,
        bool instant,
        float arcHeight = 0f,
        float durationOverride = -1f)
    {
        if (health == null)
            health = GetComponent<Health>();
        if (health != null && health.IsDowned)
            return;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (abilityMovementRoutine != null)
        {
            StopCoroutine(abilityMovementRoutine);
            abilityMovementRoutine = null;
        }

        ClearMovementIntent();

        Vector3 start = rb != null ? rb.position : transform.position;
        Vector3 facing = destination - start;
        facing.y = 0f;
        if (!lockCharacterRotation && facing.sqrMagnitude > 0.0001f)
        {
            targetRotation = UprightRotation(
                Quaternion.LookRotation(facing.normalized));
            if (rb != null)
                rb.rotation = targetRotation;
            else
                transform.rotation = targetRotation;
        }

        if (instant || Vector3.Distance(start, destination) < 0.01f)
        {
            RequestMovementLock(0.08f);
            SetAbilityMovementPosition(destination, true);
            return;
        }

        float safeSpeed = Mathf.Max(0.1f, speed);
        float duration = durationOverride >= 0f
            ? durationOverride
            : Vector3.Distance(start, destination) / safeSpeed;

        if (duration < 0.01f)
        {
            RequestMovementLock(0.08f);
            SetAbilityMovementPosition(destination, true);
            return;
        }

        RequestMovementLock(duration + 0.1f);
        abilityMovementRoutine = StartCoroutine(
            AbilityMovementRoutine(
                start,
                destination,
                duration,
                Mathf.Max(0f, arcHeight)));
    }

    IEnumerator AbilityMovementRoutine(
        Vector3 start,
        Vector3 destination,
        float duration,
        float arcHeight)
    {
        float elapsed = 0f;
        while (elapsed < duration &&
               (health == null || !health.IsDowned))
        {
            elapsed = Mathf.Min(
                duration,
                elapsed + Time.fixedDeltaTime);
            float t = duration > 0f
                ? elapsed / duration
                : 1f;

            Vector3 position =
                Vector3.Lerp(start, destination, t);
            position.y +=
                4f * t * (1f - t) * arcHeight;

            SetAbilityMovementPosition(position, false);
            yield return new WaitForFixedUpdate();
        }

        if (health == null || !health.IsDowned)
            SetAbilityMovementPosition(destination, true);

        movementLockUntil = Mathf.Min(
            movementLockUntil,
            Time.time + 0.05f);
        abilityMovementRoutine = null;
    }

    void SetAbilityMovementPosition(
        Vector3 position,
        bool teleport)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (teleport)
                rb.position = position;
            else
                rb.MovePosition(position);
        }
        else
        {
            transform.position = position;
        }
    }

    void OnDisable()
    {
        if (abilityMovementRoutine != null)
        {
            StopCoroutine(abilityMovementRoutine);
            abilityMovementRoutine = null;
        }
    }

    void Update()
    {
        if (health != null && health.IsDowned)
        {
            ClearMovementIntent();
            return;
        }

        // Yield all keyboard input to UI while player is typing
        if (IsTypingInUI() ||
            PlayerHUD.IsSpellLoadoutOpen)
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
        bool isBackwards = useBackwardMovementAnimation && pressingS && !pressingW;

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

    /// <summary>
    /// Picks the camera this player should move relative to.
    ///
    /// Camera.main is not safe here any more. Every zone scene carries its own
    /// MainCamera, and with zones loaded additively there can be several at once —
    /// Camera.main then returns an arbitrary one. Because movement is camera-relative
    /// (see the camForward/camRight block below), binding to another zone's camera
    /// rotates WASD by however that camera happens to be facing. That is the
    /// "movement is a mix of WASD" symptom.
    ///
    /// Preference order:
    ///   1. A camera in this player's own scene. Correct in host mode, where the
    ///      player object lives in its zone scene alongside that zone's camera.
    ///   2. The only camera, if there is exactly one. Correct on a real client: the
    ///      player sits in the container scene while the single camera is in the one
    ///      additively-loaded zone.
    ///   3. Camera.main, then anything at all.
    ///
    /// ROADMAP 6.9 removes the per-zone cameras outright, which makes this moot —
    /// until then this keeps host-mode testing usable.
    /// </summary>
    Camera ResolveCamera()
    {
        Camera[] all = Camera.allCameras;

        foreach (Camera c in all)
            if (c != null && c.gameObject.scene == gameObject.scene)
                return c;

        if (all.Length == 1) return all[0];

        return Camera.main != null ? Camera.main : (all.Length > 0 ? all[0] : null);
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
