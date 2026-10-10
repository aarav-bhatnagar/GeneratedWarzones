using UnityEngine;
using Unity.Netcode;

public class ArrowRotation : NetworkBehaviour
{
    [SerializeField] private Transform playerTransform;

    private void Update()
    {
        if (!IsOwner) return;
        // Get the player's Y-axis rotation
        float playerYRotation = playerTransform.eulerAngles.y;

        // Create a new Quaternion for the arrow with the same Y-axis rotation
        Quaternion newRotation = Quaternion.Euler(0, 0, -playerYRotation);

        // Apply the new rotation to the arrow
        transform.rotation = newRotation;
    }
}
