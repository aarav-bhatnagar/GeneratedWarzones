using UnityEngine;
using Unity.Netcode;

public class PlaneController : NetworkBehaviour
{
    [SerializeField] private Transform meshCentre;
    [SerializeField] private float planeSpeed;
    [SerializeField] private float distanceFromCentre;
    [SerializeField] private float planeHeight;
    [SerializeField] private float grassHeight;

    public bool isPlaneFlying;
    public NetworkVariable<bool> isAllowedToJump = new NetworkVariable<bool>();
    private Vector3 startingPos;

    [SerializeField] GameObject arrow;
    public float spawnInterval;
    public float moveSpeed;

    private float spawnTimer = 0.0f;

    [SerializeField] private Transform vfx_Arrows_Holder;
    [HideInInspector] public float angle;

    private void Start()
    {
        //Calculate postion and direction of plane
        meshCentre.rotation = Quaternion.Euler(0, angle, 0);

        transform.position = meshCentre.forward * distanceFromCentre;
        transform.LookAt(meshCentre);

        startingPos = new Vector3(transform.position.x, planeHeight, transform.position.z);
        transform.position = startingPos;


        isPlaneFlying = true;
    }

    private void Update()
    {
        // Increment the timer by the time passed since the last frame
        spawnTimer += Time.deltaTime;

        // Check if it's time to spawn a new object
        if (spawnTimer >= spawnInterval)
        {
            // Spawn the object
            GameObject newObject = Instantiate(arrow, transform.position, transform.rotation, vfx_Arrows_Holder);

            // Add the Mover script to the new object and set the speed
            ArrowMover mover = newObject.AddComponent<ArrowMover>();
            mover.moveSpeed = moveSpeed;

            Destroy(newObject, 10.0f);

            // Reset the timer
            spawnTimer = 0.0f;
        }


        if (IsServer)
        {
            if (isPlaneFlying == true)
            {
                // Calculate the forward movement for this frame
                Vector3 forwardMovement = transform.forward * planeSpeed * Time.deltaTime;
                // Apply the movement to the object's position
                transform.Translate(forwardMovement, Space.World);

                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
                {
                    if (hit.point.y > grassHeight)
                    {
                        isAllowedToJump.Value = true;
                    }
                }

            }
        }

    }

}
