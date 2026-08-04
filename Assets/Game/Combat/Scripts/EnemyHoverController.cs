#if UNITY_EDITOR || !UNITY_SERVER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Client-side enemy hover scanner. Shows a top-screen enemy tooltip and applies
/// a temporary hover highlight to the enemy under the cursor.
/// </summary>
public class EnemyHoverController : MonoBehaviour
{
    const int HitBufferSize = 64;

    public static EnemyHoverController Instance { get; private set; }

    static readonly IComparer<RaycastHit> HitDistanceComparer =
        Comparer<RaycastHit>.Create((left, right) => left.distance.CompareTo(right.distance));

    [SerializeField, Min(1f)] float maxHoverDistance = 150f;
    [SerializeField] LayerMask hoverMask = ~0;

    Health _hoveredEnemy;
    EnemyHoverHighlight _hoverHighlight;
    Camera _camera;
    readonly RaycastHit[] _hitBuffer = new RaycastHit[HitBufferSize];
    readonly List<Renderer> _rendererScratch = new List<Renderer>(16);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null)
            return;

        var go = new GameObject("[EnemyHoverController]");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<EnemyHoverController>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        Health target = FindHoveredEnemy();

        if (target != _hoveredEnemy)
            SetHoveredEnemy(target);

    }

    void OnDisable()
    {
        SetHoveredEnemy(null);
    }

    void OnDestroy()
    {
        SetHoveredEnemy(null);
    }

    Health FindHoveredEnemy()
    {
        var mouse = Mouse.current;
        if (mouse == null) return null;
        if (Cursor.lockState == CursorLockMode.Locked) return null;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return null;

        if (_camera == null || !_camera.isActiveAndEnabled)
            _camera = Camera.main;
        if (_camera == null) return null;

        Vector2 screenPos = mouse.position.ReadValue();
        if (screenPos.x < 0f || screenPos.y < 0f || screenPos.x > Screen.width || screenPos.y > Screen.height)
            return null;

        Ray ray = _camera.ScreenPointToRay(screenPos);
        int hitCount = ZonePhysics.RaycastNonAlloc(
            gameObject, ray, _hitBuffer, maxHoverDistance, hoverMask, QueryTriggerInteraction.Collide);
        if (hitCount == 0)
            return null;

        if (hitCount == _hitBuffer.Length)
        {
            RaycastHit[] allHits = ZonePhysics.RaycastAll(
                gameObject, ray, maxHoverDistance, hoverMask, QueryTriggerInteraction.Collide);
            Array.Sort(allHits, HitDistanceComparer);
            return FindFirstHoverable(allHits, allHits.Length);
        }

        Array.Sort(_hitBuffer, 0, hitCount, HitDistanceComparer);
        return FindFirstHoverable(_hitBuffer, hitCount);
    }

    Health FindFirstHoverable(RaycastHit[] hits, int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            Collider collider = hits[i].collider;
            if (collider == null) continue;

            Health fallback = null;
            Health[] candidates = collider.GetComponentsInParent<Health>(true);
            for (int candidateIndex = 0; candidateIndex < candidates.Length; candidateIndex++)
            {
                Health health = candidates[candidateIndex];
                if (!IsHoverableEnemy(health))
                    continue;

                // A few authored enemies contain nested Health components. Prefer
                // the one whose player-facing identity was actually configured
                // instead of falling back to a nearer prefab/object name.
                if (!string.IsNullOrWhiteSpace(health.ConfiguredEnemyDisplayName))
                    return health;

                if (fallback == null)
                    fallback = health;
            }

            if (fallback != null)
                return fallback;
        }

        return null;
    }

    bool IsHoverableEnemy(Health health)
    {
        if (health == null) return false;
        if (!health.enabled || !health.gameObject.activeInHierarchy) return false;
        if (!health.IsAlive) return false;
        if (!health.ShouldShowEnemyHoverInfo()) return false;

        _rendererScratch.Clear();
        health.GetComponentsInChildren(true, _rendererScratch);
        foreach (Renderer renderer in _rendererScratch)
        {
            if (renderer != null && renderer.enabled)
                return true;
        }

        return false;
    }

    void SetHoveredEnemy(Health target)
    {
        if (_hoverHighlight != null)
        {
            _hoverHighlight.SetHighlighted(false);
            _hoverHighlight = null;
        }

        _hoveredEnemy = target;
        if (_hoveredEnemy == null)
        {
            if (EnemyHoverTooltipUI.Instance != null)
                EnemyHoverTooltipUI.Instance.Hide();
            return;
        }

        _hoverHighlight = _hoveredEnemy.GetComponent<EnemyHoverHighlight>();
        if (_hoverHighlight == null)
            _hoverHighlight = _hoveredEnemy.gameObject.AddComponent<EnemyHoverHighlight>();

        _hoverHighlight.SetHighlighted(true);
        EnemyHoverTooltipUI.EnsureInstance().Show(_hoveredEnemy);
    }
}
#endif
