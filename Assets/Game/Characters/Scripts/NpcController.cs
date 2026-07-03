using System.Collections;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════
//  NpcController
//  Attach to any NPC root. Handles:
//    • Idle breathing (gentle vertical bob)
//    • Slow patrol between auto-generated waypoints
//    • VFX managed as children — assigned in Inspector or by RodNpcBuilder
//
//  No dependencies on Mirror, CharacterStats, or Health.
//  Safe to use on both networked and local-only NPCs.
// ═══════════════════════════════════════════════════════════════════════════

[AddComponentMenu("BCE/World/NPC Controller")]
public class NpcController : MonoBehaviour
{
    public enum NpcType { Enemy, Friendly, Environmental }

    [Header("Identity")]
    public string  npcName    = "NPC";
    public NpcType npcType    = NpcType.Enemy;

    [Header("Patrol")]
    [Tooltip("How far from spawn the NPC wanders. 0 = stationary.")]
    public float patrolRadius  = 6f;
    public float moveSpeed     = 1.2f;
    public float waypointPause = 2.5f;  // seconds to stand at each waypoint

    [Header("Idle Bob")]
    public float bobHeight     = 0.08f;  // vertical oscillation amplitude
    public float bobSpeed      = 1.1f;

    [Header("Idle Rotation")]
    [Tooltip("Degrees per second to slowly rotate while standing still.")]
    public float idleRotateSpeed = 18f;

    [Header("VFX — drag prefabs here or let RodNpcBuilder assign them")]
    public GameObject primaryVFX;    // main aura / circle under feet
    public GameObject secondaryVFX;  // accent effect

    // ── Runtime ──────────────────────────────────────────────────────────────
    private Vector3  _spawnPos;
    private Vector3  _waypoint;
    private bool     _moving;
    private float    _pauseTimer;
    private float    _bobT;
    private Animator _anim;
    private bool     _hasSpeedParam;

    void Start()
    {
        _spawnPos = transform.position;
        _waypoint = _spawnPos;

        _anim = GetComponentInChildren<Animator>();
        if (_anim != null && _anim.runtimeAnimatorController != null)
            foreach (var p in _anim.parameters)
                if (p.name == "Speed") { _hasSpeedParam = true; break; }

        SpawnVFX();

        if (patrolRadius > 0f)
            StartCoroutine(PatrolRoutine());
    }

    void Update()
    {
        // Drive animator Speed (0=idle, 1=walking) if controller has the parameter.
        if (_hasSpeedParam)
            _anim.SetFloat("Speed", _moving ? 1f : 0f);
        else
        {
            // Fallback procedural animation: bob + idle rotate when no Animator assigned.
            _bobT += Time.deltaTime * bobSpeed;
            Vector3 pos = transform.position;
            pos.y = _spawnPos.y + Mathf.Sin(_bobT) * bobHeight;
            transform.position = pos;
            if (!_moving)
                transform.Rotate(Vector3.up, idleRotateSpeed * Time.deltaTime);
        }
    }

    // ── Patrol ────────────────────────────────────────────────────────────────

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            // Stand at waypoint
            _moving = false;
            yield return new WaitForSeconds(waypointPause + Random.Range(-0.5f, 0.5f));

            // Pick a new random waypoint within radius
            Vector2 rnd    = Random.insideUnitCircle * patrolRadius;
            _waypoint      = _spawnPos + new Vector3(rnd.x, 0f, rnd.y);
            _moving        = true;

            // Walk toward it
            while (Vector3.Distance(transform.position, _waypoint) > 0.25f)
            {
                Vector3 dir = (_waypoint - transform.position);
                dir.y = 0f;
                dir.Normalize();

                // Face direction of travel
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(dir),
                        8f * Time.deltaTime);

                // Move (preserve bob Y)
                Vector3 p = transform.position;
                p.x += dir.x * moveSpeed * Time.deltaTime;
                p.z += dir.z * moveSpeed * Time.deltaTime;
                transform.position = p;

                yield return null;
            }
        }
    }

    // ── VFX ───────────────────────────────────────────────────────────────────

    void SpawnVFX()
    {
        if (primaryVFX != null)
        {
            var go = Instantiate(primaryVFX, transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
        }

        if (secondaryVFX != null)
        {
            // Slightly offset so effects don't perfectly overlap
            var go = Instantiate(secondaryVFX, transform);
            go.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale    = Vector3.one * 0.7f;
        }
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = npcType == NpcType.Enemy
            ? new Color(1f, 0.2f, 0.2f, 0.3f)
            : new Color(0.2f, 1f, 0.4f, 0.3f);

        Gizmos.DrawWireSphere(Application.isPlaying ? _spawnPos : transform.position, patrolRadius);
    }
}
