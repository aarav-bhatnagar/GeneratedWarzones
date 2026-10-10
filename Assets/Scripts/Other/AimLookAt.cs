using UnityEngine;
using Unity.Netcode;

public class AimLookAt : NetworkBehaviour
{
    public Transform cameraTransform;

    private void Start()
    {
        

        cameraTransform = Camera.main.transform;

    }

    private void Update()
    {
        if (!IsOwner) return;
        if (cameraTransform == null) return;
        transform.position = cameraTransform.position + cameraTransform.forward * 20;
        transform.rotation = cameraTransform.rotation;
    }

}
