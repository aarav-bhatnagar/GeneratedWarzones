using UnityEngine;

public class ArrowMover : MonoBehaviour
{
    [HideInInspector]
    public float moveSpeed;

    void Update()
    {
        // Move the object forward at the set speed
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}
