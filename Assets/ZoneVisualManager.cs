using UnityEngine;

public class ZoneVisualManager : MonoBehaviour
{
    public static ZoneVisualManager Instance;
    public TriggerZone[] allZones;

   private void Awake()
{
    Instance = this;

    // Hide all visuals when the game starts
    HideAllZones();
}


    public void ShowAllZones()
    {
        foreach (var zone in allZones)
        {
            if (zone.zoneVisual != null)
            {
                zone.zoneVisual.SetActive(true);
            }
        }
    }

    public void HideAllZones()
    {
        foreach (var zone in allZones)
        {
            if (zone.zoneVisual != null)
            {
                zone.zoneVisual.SetActive(false);
            }
        }
    }
}
