#if UNITY_EDITOR || !UNITY_SERVER
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Client-side enemy hover scanner. Shows a top-screen enemy tooltip and applies
/// a temporary hover highlight to the enemy under the cursor.
/// </summary>
public class EnemyHoverController : MonoBehaviour
{
    public static EnemyHoverController Instance { get; private set; }

    [SerializeField, Min(1f)] float maxHoverDistance = 150f;
    [SerializeField] LayerMask hoverMask = ~0;

    Health _hoveredEnemy;
    EnemyHoverHighlight _hoverHighlight;

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

        if (_hoveredEnemy != null)
            EnemyHoverTooltipUI.EnsureInstance().Show(_hoveredEnemy);
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

        Camera cam = Camera.main;
        if (cam == null) return null;

        Vector2 screenPos = mouse.position.ReadValue();
        if (screenPos.x < 0f || screenPos.y < 0f || screenPos.x > Screen.width || screenPos.y > Screen.height)
            return null;

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxHoverDistance, hoverMask, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0)
            return null;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            Health health = hit.collider.GetComponentInParent<Health>();
            if (IsHoverableEnemy(health))
                return health;
        }

        return null;
    }

    static bool IsHoverableEnemy(Health health)
    {
        if (health == null) return false;
        if (!health.enabled || !health.gameObject.activeInHierarchy) return false;
        if (!health.IsAlive) return false;
        if (!health.ShouldShowEnemyHoverInfo()) return false;

        foreach (var renderer in health.GetComponentsInChildren<Renderer>(true))
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
            EnemyHoverTooltipUI.EnsureInstance().Hide();
            return;
        }

        _hoverHighlight = _hoveredEnemy.GetComponent<EnemyHoverHighlight>();
        if (_hoverHighlight == null)
            _hoverHighlight = _hoveredEnemy.gameObject.AddComponent<EnemyHoverHighlight>();

        _hoverHighlight.SetHighlighted(true);
    }
}
#endif
