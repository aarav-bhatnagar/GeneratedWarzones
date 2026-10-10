using UnityEngine;
using TMPro;

public class ZoneUI : MonoBehaviour
{
    private TMP_Text text;
    private ZoneWall zoneWall;
    public Transform jumperPlanePlayerHolder;

    private void Start()
    {
        text = GetComponentInChildren<TMP_Text>();
        zoneWall = FindFirstObjectByType<ZoneWall>();

    }

    private void Update()
    {
        if (jumperPlanePlayerHolder == null) return;
        if (zoneWall.CurrentZone.Value + 1 >= zoneWall.ZonePositions.Count || jumperPlanePlayerHolder.childCount > 0)
        {
            text.text = "";
            return; // Exit if the next index is out of range
        }

        if (zoneWall.seconds.Value == 0)
        {
            text.text = "";
        }
        else
        {
            text.text = "Storm eye shrinking in " + zoneWall.seconds.Value.ToString() + " seconds!";
        }
    }
}
