using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;

public class ZoneDamage : NetworkBehaviour
{
    [SerializeField] private GameHandler gameHandler;
    [SerializeField] private float WaterDepth;
    public Transform CameraPos;
    private bool inZone = true;
    private ZoneWall zoneWall;
    private float timeSinceLastDamage = 0.0f;
    private int DamageRate;
    [SerializeField] private GameObject inZonePanel;


    private void Start()
    {
        zoneWall = FindFirstObjectByType<ZoneWall>();
        CameraPos = Camera.main.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.name == "ZoneCollider")
        {
            inZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;
        
        if (other.name == "ZoneCollider")
        {
            inZone = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (inZone)
        {
            inZonePanel.SetActive(false);
            return;
        }
        else
        {
            inZonePanel.SetActive(true);
        }

        DamageRate = zoneWall.ZonePositions[zoneWall.CurrentZone.Value].zoneDamageperSecond;


        timeSinceLastDamage += Time.deltaTime;
        if (timeSinceLastDamage >= 1f)
        {
            // Call TakeDamage
            gameHandler.TakeDamage(DamageRate);

            // Reset the timer
            timeSinceLastDamage = 0.0f;
        }
    }
}
