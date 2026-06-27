using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMountBike : MonoBehaviour
{
    public GameObject motorcycle;
    public CameraFollow cameraFollow;

    void Update()
    {
        // G to mount — backtick is reserved for GM console
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            MountBike();
        }
    }

    void MountBike()
    {
        motorcycle.transform.position = transform.position;
        motorcycle.transform.rotation = transform.rotation;

        MotorcycleController controller =
            motorcycle.GetComponent<MotorcycleController>();

        if (controller != null)
        {
            controller.player = gameObject;
            controller.cameraFollow = cameraFollow;
        }

        motorcycle.SetActive(true);

        if (cameraFollow != null)
        {
            cameraFollow.target = motorcycle.transform;
        }

        gameObject.SetActive(false);
    }
}