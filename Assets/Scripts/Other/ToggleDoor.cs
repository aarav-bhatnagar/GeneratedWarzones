using UnityEngine;
using Unity.Netcode;

public class ToggleDoor : NetworkBehaviour
{

    private NetworkVariable<bool> Open = new NetworkVariable<bool>();
    private float doorSpeed = 2f;  // Base speed
    private Quaternion targetRot;
    private float currentLerpTime; // Tracks the time for interpolation
    [SerializeField] private bool inverted;

    private void Start()
    {
        // Initialize targetRot to the current rotation
        targetRot = transform.localRotation;
        currentLerpTime = 0f;  // Start at 0 time
    }

    private void Update()
    {
        if (Open.Value == true)
        {
            if (!inverted)
            {
                targetRot = Quaternion.Euler(0, 100, 0);

            }
            else
            {
                targetRot = Quaternion.Euler(0, -100, 0);
            }

        }
        // Rotate the door towards the target rotation using a smooth deceleration
        if (transform.localRotation != targetRot)
        {
            currentLerpTime += Time.deltaTime * doorSpeed;
            float t = Mathf.Clamp01(currentLerpTime);
            t = Mathf.SmoothStep(0f, 1f, t);  // Ease in and out behavior

            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, t);

            // Reset the time if the door has reached the target
            if (Quaternion.Angle(transform.localRotation, targetRot) < 0.01f)
            {
                currentLerpTime = 0f;  // Reset to allow smooth open/close next time
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            Open.Value = true;
        }
    }
}
