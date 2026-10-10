using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ZoneMinimap : NetworkBehaviour
{
    public ZoneWall zoneWallcs;
    [SerializeField] private LineRenderer nextCircleLineRenderer;
    private float nextCircleHeight = 103;

    [SerializeField] private LineRenderer deathZoneLineRenderer;
    [SerializeField] private float borderWidth;

    private Transform jumperPlanePlayerHolder;

    private void Start()
    {
        jumperPlanePlayerHolder = FindFirstObjectByType<JumperPlanePlayerHolder>().transform;
    }

    private void Update()
    {

        if (!zoneWallcs.centresCalculated.Value || jumperPlanePlayerHolder.transform.childCount > 0) return;
        UpdateDeathZone();

        if (!zoneWallcs.shrinking.Value)
        {
            UpdateNextCircle();
        }

        if (zoneWallcs.ZonePositions.Count - 1 == zoneWallcs.CurrentZone.Value)
        {
            if (zoneWallcs.waiting == true)
            {
                ClearCircle();
            }
            return;
        }
    }

    private void UpdateDeathZone()
    {
        float currentRadius = zoneWallcs.transform.localScale.x * zoneWallcs.OriginalRadius;
        Vector3 currentCentre = zoneWallcs.transform.localPosition;
        DrawCircle(currentRadius + borderWidth, currentCentre, deathZoneLineRenderer);
        
    }


    public void UpdateNextCircle()
    {
        if (zoneWallcs.CurrentZone.Value + 1 >= zoneWallcs.ZonePositions.Count)
        {
            ClearCircle();
            return; // Exit if the next index is out of range
        }

        float nextRadius = zoneWallcs.ZonePositions[zoneWallcs.CurrentZone.Value + 1].Radius;
        Vector3 nextCentre = zoneWallcs.ZonePositions[zoneWallcs.CurrentZone.Value + 1].CentrePoint;

        // Add logic to handle updating the next circle here
        DrawCircle(nextRadius, nextCentre, nextCircleLineRenderer);
    }

    private void DrawCircle(float radius, Vector3 centre, LineRenderer lineRenderer)
    {
        int segments = 360;
        lineRenderer.positionCount = segments + 1; // +1 to close the circle

        float angleStep = 360f / segments; // Calculate angle between points
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            // Convert angle to radians
            float radian = Mathf.Deg2Rad * angle;

            // Calculate x and z positions on the XZ plane
            float x = Mathf.Cos(radian) * radius;
            float z = Mathf.Sin(radian) * radius;

            // Set the position in the Line Renderer
            lineRenderer.SetPosition(i, centre + new Vector3(x, nextCircleHeight, z));

            angle += angleStep; // Increment angle
        }

    }

    private void ClearCircle()
    {
        nextCircleLineRenderer.positionCount = 0;
    }
}
