using UnityEngine;
using Unity.Netcode;

public class CrossHairTarget : NetworkBehaviour
{
    public Camera playerCamera;
    Ray ray;
    RaycastHit hitInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = Camera.main;
        if (!IsOwner)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCamera == null) return;
        ray.origin = playerCamera.transform.position;
        ray.direction = playerCamera.transform.forward;
        if (Physics.Raycast(ray, out hitInfo))
        {
            transform.position = hitInfo.point;
        }
    }
}
