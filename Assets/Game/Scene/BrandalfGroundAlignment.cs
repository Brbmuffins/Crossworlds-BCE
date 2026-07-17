using UnityEngine;

[DisallowMultipleComponent]
public sealed class BrandalfGroundAlignment : MonoBehaviour
{
    private const float VisualDrop = 0.15f;

    private void Awake()
    {
        Transform visualRoot = null;
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            var candidate = renderer.transform;
            while (candidate.parent != null && candidate.parent != transform)
                candidate = candidate.parent;

            if (candidate != transform)
            {
                visualRoot = candidate;
                break;
            }
        }

        if (visualRoot == null)
        {
            Debug.LogWarning("Brandalf visual root could not be found; model alignment was not changed.", this);
            return;
        }

        visualRoot.localPosition += Vector3.down * VisualDrop;
        Debug.Log($"[BrandalfGround] Lowered visual model root '{visualRoot.name}' by {VisualDrop:F2}m; physics collider unchanged.");
    }
}