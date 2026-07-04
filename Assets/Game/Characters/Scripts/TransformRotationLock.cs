using UnityEngine;

[DisallowMultipleComponent]
public class TransformRotationLock : MonoBehaviour
{
    public bool lockRotation = true;
    public bool useLocalRotation = true;
    public bool captureStartRotation = true;

    public bool lockX = true;
    public bool lockY = true;
    public bool lockZ = true;

    public Vector3 lockedEulerAngles;

    void Awake()
    {
        if (captureStartRotation)
            lockedEulerAngles = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
    }

    void LateUpdate()
    {
        if (!lockRotation)
            return;

        Vector3 current = useLocalRotation ? transform.localEulerAngles : transform.eulerAngles;
        Vector3 next = current;

        if (lockX) next.x = lockedEulerAngles.x;
        if (lockY) next.y = lockedEulerAngles.y;
        if (lockZ) next.z = lockedEulerAngles.z;

        if (useLocalRotation)
            transform.localEulerAngles = next;
        else
            transform.eulerAngles = next;
    }
}
