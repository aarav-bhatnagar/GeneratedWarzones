using UnityEngine;
using System.Collections.Generic;

public class PlanePropRotater : MonoBehaviour
{
    [SerializeField] private Transform[] propellers;

    // Rotation speed in degrees per second
    [SerializeField] private float rotationSpeed = 10f;

    // Update is called once per frame
    void Update()
    {
        // Calculate the rotation step for this frame
        float rotationStep = rotationSpeed * Time.deltaTime;

        // Rotate each transform in the array
        for (int i = 0; i < propellers.Length; i++)
        {
            Transform t = propellers[i];

            // Check if the transform is one of the last two
            if (i >= propellers.Length - 2)
            {
                // Maintain the -180 degree y rotation while rotating around the z-axis
                t.localRotation = Quaternion.Euler(t.rotation.eulerAngles.x, -180f, t.rotation.eulerAngles.z - rotationStep);
            }
            else
            {
                // Rotate normally
                t.Rotate(0, 0, -rotationStep);
            }

        }
    }
}
