using Mirror;
using UnityEngine;

public class PlayerFallReset : MonoBehaviour
{
    [SerializeField] private float fallYThreshold = -20f;
    [SerializeField] private Vector3 resetPosition = new Vector3(4.03f, 0.98f, -4.63f);

    private Rigidbody _rb;
    private NetworkIdentity _netId;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _netId = GetComponent<NetworkIdentity>();
    }

    void FixedUpdate()
    {
        if (_netId != null && !_netId.isLocalPlayer)
            return;

        float y = _rb != null ? _rb.position.y : transform.position.y;
        if (y > fallYThreshold)
            return;

        ResetPlayerPosition();
    }

    void ResetPlayerPosition()
    {
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.position = resetPosition;
        }
        else
        {
            transform.position = resetPosition;
        }
    }
}
