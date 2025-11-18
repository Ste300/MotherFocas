using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkSealSync : NetworkBehaviour
{
    [Networked] private Vector3 NetPosition { get; set; }
    [Networked] private Quaternion NetRotation { get; set; }

    [SerializeField] float positionLerp = 15f;
    [SerializeField] float rotationLerp = 15f;

    private bool _first = true;

    // (HOST) Writes the current position/rotation to the networked properties.
    public override void FixedUpdateNetwork()
    {
        if (Object == null) return;

        if (Object.HasStateAuthority)
        {
            NetPosition = transform.position;
            NetRotation = transform.rotation;
        }
    }

    // (CLIENT) Interpolates to the networked position/rotation.
    void Update()
    {
        if (Object == null) return;

        if (!Object.HasStateAuthority)
        {
            if (_first)
            {
                transform.position = NetPosition;
                transform.rotation = NetRotation;
                _first = false;
                return;
            }

            transform.position = Vector3.Lerp(transform.position, NetPosition, Mathf.Clamp01(positionLerp * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, NetRotation, Mathf.Clamp01(rotationLerp * Time.deltaTime));
        }
    }
}