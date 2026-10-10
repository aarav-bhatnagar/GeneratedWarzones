using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
public class ObjectSpawner : NetworkBehaviour
{
    [SerializeField] private List<SpawningObject> ObjectsToBeSpawned;
    [SerializeField] private Vector2 mapSize;
    [SerializeField] private float checkerHeight = 100;
    [SerializeField] private float loweringHeight = 0.5f;
    private GameLoader gameLoader;

    public List<Bounds> spawnedObjectBounds = new List<Bounds>();

    private int ToExit = 2000;



    private Quaternion GetSlopeAlignedRotation(Vector3 slopeNormal, float originalYRotation)
    {
        // Calculate the rotation that aligns the object's up axis with the slope normal
        Quaternion slopeAlignment = Quaternion.FromToRotation(Vector3.up, slopeNormal);

        // Apply the original Y rotation after aligning the slope
        Quaternion finalRotation = slopeAlignment * Quaternion.Euler(0, originalYRotation, 0);

        return finalRotation;
    }

    /*
    public void PlaceObjects()
    {
        for (int i = 0; i < spawnedObjectPositions.Count; i++)
        {
            Instantiate(CalculateGameObject(i), spawnedObjectPositions[i].Value, spawnedObjectRotations[i].Value, transform);

            print(CalculateGameObject(i));
        }
    }
    */
    private GameObject CalculateGameObject(int index)
    {
        // Go through each SpawningObject in the list to determine which one corresponds to the given index
        int accumulatedIndex = 0;

        for (int i = 0; i < ObjectsToBeSpawned.Count; i++)
        {
            int objectCount = ObjectsToBeSpawned[i].numberOfObjects;

            // Check if the index falls within the range for the current SpawningObject
            if (index >= accumulatedIndex && index < accumulatedIndex + objectCount)
            {
                return ObjectsToBeSpawned[i].gameObject;
            }

            // Accumulate the index range for the next SpawningObject
            accumulatedIndex += objectCount;
        }

        // If something goes wrong and no object is found, return null (although this shouldn't happen)
        return null;
    }


    public void SpawnObjects()
    {
        gameLoader = FindFirstObjectByType<GameLoader>();
        int Count = 0;
        for (int i = 0; i < ObjectsToBeSpawned.Count; i++)
        {
            int objectsSpawned = 0;
            while (objectsSpawned < ObjectsToBeSpawned[i].numberOfObjects && Count < ToExit)
            {
                Count++;
                Vector3 position = new Vector3(Random.Range(-mapSize.x, mapSize.x), checkerHeight, Random.Range(-mapSize.y, mapSize.y));

                if (Physics.Raycast(position, Vector3.down, out RaycastHit hit))
                {
                    if (hit.point.y > ObjectsToBeSpawned[i].minHeight && hit.point.y < ObjectsToBeSpawned[i].maxHeight)
                    {
                        if (Vector3.Distance(Vector3.zero, hit.point) < ObjectsToBeSpawned[i].maxDistanceFromCentre)
                        {
                            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                            if (slopeAngle <= ObjectsToBeSpawned[i].maxSlopeAngle)
                            {
                                float originalYRotation = GetRandomRotation();

                                // Get the final rotation that aligns with the slope and applies the original Y rotation

                                Quaternion rotation = GetSlopeAlignedRotation(hit.normal, originalYRotation);

                                Bounds objectBounds = GetObjectBounds(ObjectsToBeSpawned[i].gameObject, hit.point, rotation);

                                if (!IsOverlapping(objectBounds, ObjectsToBeSpawned[i].bufferDistance))
                                {
                                    // Inside the SpawnObjects method
                                    Vector3 loweredPosition = hit.point - new Vector3(0, loweringHeight, 0);

                                    // Store the position and rotation separately
                                    Instantiate(ObjectsToBeSpawned[i].gameObject, loweredPosition, rotation).GetComponent<NetworkObject>().Spawn();

                                    // Store bounds and count as before
                                    spawnedObjectBounds.Add(GetObjectBounds(ObjectsToBeSpawned[i].gameObject, loweredPosition, rotation));
                                    objectsSpawned++;

                                }
                            }
                        }
                    }
                }

                if (objectsSpawned == ObjectsToBeSpawned[i].numberOfObjects)    //ie. ToExit code
                {
                    //Debug.LogWarning($"Could only place {objectsSpawned} out of {ObjectsToBeSpawned[i].numberOfObjects} objects for {ObjectsToBeSpawned[i].gameObject.name}.");
                }
            }
        }


    }

    private float GetRandomRotation()
    {
        int[] rotations = { 0, 90, 180, 270 };
        return rotations[Random.Range(0, rotations.Length)];
    }

    private Bounds GetObjectBounds(GameObject obj, Vector3 position, Quaternion rotation)
    {
        GameObject tempObject = Instantiate(obj, position, rotation);
        Bounds bounds = tempObject.GetComponent<Renderer>().bounds;
        Destroy(tempObject);
        return bounds;
    }

    private bool IsOverlapping(Bounds newObjectBounds, float bufferDistance)
    {
        foreach (Bounds bounds in spawnedObjectBounds)
        {
            Bounds expandedBounds = bounds;
            expandedBounds.Expand(bufferDistance);
            if (newObjectBounds.Intersects(expandedBounds))
            {
                return true;
            }
        }
        return false;
    }

}