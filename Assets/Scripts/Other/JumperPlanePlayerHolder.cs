using UnityEngine;
using Unity.Netcode;

public class JumperPlanePlayerHolder : NetworkBehaviour
{
    private Transform jumperPlane;

    public override void OnNetworkSpawn()
    {
        jumperPlane = FindFirstObjectByType<PlaneController>().transform;
    }

    private void Update()
    {
        if (IsServer)
        {
            transform.position = jumperPlane.transform.position;
        }
    }
}
