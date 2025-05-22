using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GlassPickup : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("[GlassPickupListener] Glass picked up!");
        ZoneVisualManager.Instance?.ShowAllZones();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log("[GlassPickupListener] Glass dropped!");
        // Optional: Hide here too, if needed
        ZoneVisualManager.Instance?.HideAllZones();
    }
}
