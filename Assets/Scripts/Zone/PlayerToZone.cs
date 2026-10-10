using UnityEngine;
using Unity.Netcode;

public class PlayerToZone : NetworkBehaviour
{
    [SerializeField] private Transform playerTransform; // The player's transform
    [SerializeField] private float lineHeight = 1.0f; // Height of the line
    private LineRenderer lineRenderer; // The LineRenderer component
    private ZoneWall zoneWall; // Reference to the ZoneWall script

    private Transform jumperPlanePlayerHolder;


    private void OnEnable()
    {
        // Get the LineRenderer component attached to the same GameObject
        lineRenderer = GetComponent<LineRenderer>();
        // Find the ZoneWall in the scene (assuming there's only one)
        zoneWall = FindFirstObjectByType<ZoneWall>();
        jumperPlanePlayerHolder = FindFirstObjectByType<JumperPlanePlayerHolder>().transform;

    }

    private void Update()
    {
        if (IsOwner || jumperPlanePlayerHolder.transform.childCount > 0)
        {
            if (lineRenderer != null && playerTransform != null && zoneWall != null)
            {
                UpdateLine();
            }
        }
    }

    private void UpdateLine()
    {
        ZonePositions nextZone;
        // Get the next zone position and radius

        if (zoneWall.CurrentZone.Value + 1 >= zoneWall.ZonePositions.Count)
        {
            nextZone = zoneWall.ZonePositions[zoneWall.CurrentZone.Value];
        }
        else
        {
            nextZone = zoneWall.ZonePositions[zoneWall.CurrentZone.Value + 1];
        }


        if (PlayerInCircle(playerTransform.position, nextZone.CentrePoint, nextZone.Radius) == true)
        {
            return;
        }


        Vector3 pointOnCircle = ClosestPointOnCircle(playerTransform.position, nextZone.CentrePoint, nextZone.Radius);

        // Set the height of the points
        pointOnCircle.y = lineHeight;
        Vector3 adjustedPlayerPosition = playerTransform.position;
        adjustedPlayerPosition.y = lineHeight;

        // Draw the line between the player's position and the closest point on the next zone
        DrawLineBetweenPoints(adjustedPlayerPosition, pointOnCircle);
    }

    private bool PlayerInCircle(Vector3 playerPosition, Vector3 circleCentre, float circleRadius)
    {
        // Flatten positions to the XZ plane
        playerPosition.y = 0;
        circleCentre.y = 0;

        // Calculate the squared distance between the player and the circle's center
        float distanceSquared = (playerPosition - circleCentre).sqrMagnitude;

        // Compare the squared distance to the squared radius
        return distanceSquared <= circleRadius * circleRadius;

    }

    private Vector3 ClosestPointOnCircle(Vector3 playerPosition, Vector3 circleCentre, float circleRadius)
    {
        // Flatten positions to the XZ plane
        playerPosition.y = 0;
        circleCentre.y = 0;

        // Calculate the direction from the circle's center to the player's position
        Vector3 direction = playerPosition - circleCentre;

        // Normalize the direction to get a unit vector
        direction.Normalize();

        // Scale the unit vector by the circle's radius to find the closest point on the circle
        Vector3 closestPoint = circleCentre + direction * circleRadius;

        return closestPoint;
    }

    private void DrawLineBetweenPoints(Vector3 pointA, Vector3 pointB)
    {
        // Set the number of points to 2
        lineRenderer.positionCount = 2;
        // Set the first point
        lineRenderer.SetPosition(0, pointA);
        // Set the second point
        lineRenderer.SetPosition(1, pointB);
    }
}
