using Mirror;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MapZoneTrigger : MonoBehaviour
{
    [Header("Zone")]
    public string zoneName = "New Zone";
    public string subtitle = "";

    [Header("Rules")]
    public bool showOnEnter = true;
    public bool localPlayerOnly = true;
    public float repeatDelay = 1.5f;

    float _lastShownTime = -999f;

    void Reset()
    {
        EnsureTrigger();
    }

    void OnValidate()
    {
        EnsureTrigger();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!showOnEnter || string.IsNullOrWhiteSpace(zoneName)) return;
        if (Time.unscaledTime - _lastShownTime < repeatDelay) return;
        if (localPlayerOnly && !IsLocalPlayer(other)) return;

        _lastShownTime = Time.unscaledTime;
        ZoneTitleUI.GetOrCreate()?.Show(zoneName, subtitle);
    }

    bool IsLocalPlayer(Collider other)
    {
        var identity = other.GetComponentInParent<NetworkIdentity>();
        if (identity != null) return identity.isLocalPlayer;

        return other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null;
    }

    void EnsureTrigger()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.18f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.85f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.85f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
}
