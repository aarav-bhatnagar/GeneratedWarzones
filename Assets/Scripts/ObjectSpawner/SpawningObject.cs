using UnityEngine;

[System.Serializable]

public class SpawningObject
{
    public GameObject gameObject;
    public int numberOfObjects;
    public float minHeight;
    public float maxHeight;
    public float maxDistanceFromCentre;
    public float maxSlopeAngle; // Maximum allowed slope angle in degrees
    public float bufferDistance; // Minimum distance to other objects


}