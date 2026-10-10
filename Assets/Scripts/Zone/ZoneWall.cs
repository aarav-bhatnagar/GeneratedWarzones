using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ZoneWall : NetworkBehaviour
{
    public List<ZonePositions> ZonePositions = new List<ZonePositions>();
    public float OriginalRadius;
    public NetworkVariable<int> CurrentZone = new NetworkVariable<int>();
    public NetworkVariable<bool> shrinking = new NetworkVariable<bool>();
    public bool waiting = false;
    public NetworkVariable<int> seconds = new NetworkVariable<int>(0);

    [SerializeField] private Vector2 baseTiling;
    [SerializeField] private Material zoneMaterial;
    public NetworkList<Vector3> publicZoneCentres = new NetworkList<Vector3>();
    public NetworkVariable<bool> centresCalculated = new NetworkVariable<bool>();
    public Transform jumperPlayerPlaneHolder;

    // OnNetworkSpawn ensures the list is correctly initialized

    public void CalculateCentres()
    {
        if (IsServer)
        {
            if (ZonePositions == null || ZonePositions.Count == 0)
            {
                Debug.LogError("ZonePositions is not initialized or empty.");
                return;
            }

            OriginalRadius = ZonePositions[0].Radius;

            for (int i = 1; i < ZonePositions.Count; i++)
            {
                if (ZonePositions[i] == null || ZonePositions[i - 1] == null)
                {
                    Debug.LogError($"ZonePositions[{i}] or ZonePositions[{i - 1}] is not initialized.");
                    continue;
                }

                float previousRadius = ZonePositions[i - 1].Radius;
                float newRadius = ZonePositions[i].Radius;
                Vector3 newCentre;
                do
                {
                    float maxDistance = previousRadius - newRadius;  // Maximum allowed distance from the previous center
                    newCentre = RandomPointInCircle(ZonePositions[i - 1].CentrePoint, maxDistance);
                } while (!CheckCentre(newCentre, newRadius, i));

                ZonePositions[i].CentrePoint = newCentre;
                publicZoneCentres.Add(newCentre);
            }

            centresCalculated.Value = true;

        }

    }

    public void UpdateCentres()
    {
        print("RUn " + publicZoneCentres.Count);
        for (int i = 0; i < publicZoneCentres.Count; i++)
        {
            print("HELLO" + publicZoneCentres[i].ToString());
            ZonePositions[i+1].CentrePoint = publicZoneCentres[i];
        }
    }

    private bool CheckCentre(Vector3 newCentre, float newRadius, int i)
    {
        float distance = Vector3.Distance(ZonePositions[i - 1].CentrePoint, newCentre);
        float previousRadius = ZonePositions[i - 1].Radius;
        return (distance <= (previousRadius - newRadius));  // Ensure new center is within bounds of the previous zone
    }

    private Vector3 RandomPointInCircle(Vector3 center, float maxDistance)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(0f, maxDistance);
        return new Vector3(
            center.x + distance * Mathf.Cos(angle),
            center.y,
            center.z + distance * Mathf.Sin(angle)
        );
    }
    private bool Set = false;

    private void Update()
    {
        if (CurrentZone.Value + 1 == ZonePositions.Count && !shrinking.Value && waiting && IsServer && Set == false)
        {
            // Activate end game on all players
            var players = FindObjectsByType<winManager>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                player.EndGameStarted.Value = true;
                player.EndGameTimer.Value = 30f;
            }
            Set = true;

        }

        if (jumperPlayerPlaneHolder.childCount == 0)
        {
            if (!shrinking.Value && !waiting && CurrentZone.Value < ZonePositions.Count && IsServer)
            {
                StartCoroutine(ShrinkAndMoveZone());
            }
        }


    }

    private IEnumerator ShrinkAndMoveZone()
    {
        shrinking.Value = true;

        Vector3 currentPos = transform.localPosition;
        Vector3 targetPos = ZonePositions[CurrentZone.Value].CentrePoint;
        float startScale = transform.localScale.x;
        float targetScale = (float)ZonePositions[CurrentZone.Value].Radius / OriginalRadius;

        float duration = ZonePositions[CurrentZone.Value].timeInShrinking;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            float newScale = Mathf.Lerp(startScale, targetScale, progress);
            Vector3 newPos = Vector3.Lerp(currentPos, targetPos, progress);

            ScaleAndMoveZone(newScale, newPos);

            yield return null;
        }

        // Ensure the final values are set
        ScaleAndMoveZone(targetScale, targetPos);

        shrinking.Value = false;
        waiting = true;

        StartCoroutine(WaitInZone());
    }

    private IEnumerator WaitInZone()
    {
        float elapsedTime = 0.0f;
        float secondCounter = 0.0f;

        seconds.Value = ZonePositions[CurrentZone.Value].timeInZone;

        while (seconds.Value > 0)
        {
            elapsedTime += Time.deltaTime;
            secondCounter += Time.deltaTime;

            if (secondCounter >= 1.0f)
            {
                secondCounter = 0.0f;

                if (IsServer)
                {
                    seconds.Value--;
                }
            }

            yield return null;
        }

        seconds.Value = 0;
        waiting = false;

        if ((CurrentZone.Value + 1) < ZonePositions.Count)
        {
            CurrentZone.Value++;
        }
    }

    private void ScaleAndMoveZone(float scale, Vector3 position)
    {
        transform.localScale = new Vector3(scale, scale, scale);
        Vector2 newTiling = new Vector2(baseTiling.x * scale, baseTiling.y * scale);

        zoneMaterial.mainTextureScale = newTiling;
        transform.localPosition = position;
    }
}