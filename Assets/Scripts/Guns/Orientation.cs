using UnityEngine;
using Cinemachine;
public class Orientation : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float turnSpeed = 15f;

    private void LateUpdate()
    {
        // Get the yaw (Y-axis rotation) of the camera
        float yawCamera = playerCamera.transform.rotation.eulerAngles.y;

        // Compute the target rotation
        Quaternion targetRotation = Quaternion.Euler(0, yawCamera, 0);

        // Smoothly interpolate the current rotation to the target rotation
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }


}
