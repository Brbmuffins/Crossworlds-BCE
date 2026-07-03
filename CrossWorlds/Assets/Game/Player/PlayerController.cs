// Copy to: Assets/Game/Player/PlayerController.cs
#if !UNITY_SERVER
using UnityEngine;
using Mirror;

/// <summary>
/// PlayerController — WASD movement, gravity, mouse-look rotation, Animator drive.
///
/// Requires CharacterController on the same GameObject.
/// Only processes input on the local player (isLocalPlayer guard).
///
/// Animator contract (optional — null-safe if no Animator present):
///   float "Speed"  — 0 = idle, 1 = full walk/run
///
/// Setup:
///   1. Add CharacterController to the player prefab (remove old Rigidbody if present)
///   2. Add this script to the player prefab
///   3. If you have an Animator, add it to the prefab and set your blend tree to use "Speed"
///
/// Run BCE/Setup/4b to patch existing class prefabs automatically.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed   = 5f;
    public float runSpeed    = 9f;
    [Tooltip("Hold Left Shift to run")]
    public bool  allowSprint = true;

    [Header("Rotation")]
    [Tooltip("Degrees per second the player rotates toward movement direction")]
    public float turnSpeed   = 720f;

    [Header("Physics")]
    public float gravity         = -18f;   // stronger than default for snappy feel
    public float groundedPulldown = -2f;   // constant small downward force when grounded

    // ─── Private ──────────────────────────────────────────────────────────────
    CharacterController _cc;
    Animator            _anim;
    Vector3             _velocity;          // vertical velocity accumulator

    static readonly int SPEED_HASH = Animator.StringToHash("Speed");

    // ─── Init ─────────────────────────────────────────────────────────────────
    void Awake()
    {
        _cc   = GetComponent<CharacterController>();
        _anim = GetComponentInChildren<Animator>();   // null-safe if no Animator
    }

    public override void OnStartLocalPlayer()
    {
        // Lock cursor for play mode (optional — remove if you have a UI overlay)
        // Cursor.lockState = CursorLockMode.Locked;
    }

    // ─── Update ───────────────────────────────────────────────────────────────
    void Update()
    {
        // Only the local player handles input — all others are driven by Mirror sync
        if (!isLocalPlayer) return;

        HandleGravity();
        HandleMovement();
    }

    // ─── Gravity ──────────────────────────────────────────────────────────────
    void HandleGravity()
    {
        if (_cc.isGrounded)
        {
            // Snap to ground with a small constant pull — prevents float/jitter on slopes
            _velocity.y = groundedPulldown;
        }
        else
        {
            _velocity.y += gravity * Time.deltaTime;
        }
    }

    // ─── Movement ─────────────────────────────────────────────────────────────
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");   // A/D or Left/Right
        float v = Input.GetAxis("Vertical");     // W/S or Up/Down

        Vector3 input = new Vector3(h, 0f, v);

        // Cap diagonal movement to 1
        if (input.sqrMagnitude > 1f) input.Normalize();

        float speed = (allowSprint && Input.GetKey(KeyCode.LeftShift))
            ? runSpeed
            : moveSpeed;

        // ── Rotate toward movement direction (tank-turn style) ────────────────
        if (input.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(input, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, turnSpeed * Time.deltaTime);
        }

        // ── Apply movement + gravity via CharacterController ─────────────────
        Vector3 move = input * speed + _velocity;
        _cc.Move(move * Time.deltaTime);

        // ── Drive Animator ────────────────────────────────────────────────────
        if (_anim != null)
        {
            // Normalize to [0,1]: 0=idle, 0.5=walk, 1=run
            float animSpeed = input.magnitude * (speed / runSpeed);
            _anim.SetFloat(SPEED_HASH, animSpeed, 0.1f, Time.deltaTime);
        }
    }
}
#endif
