using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GlassPickup : MonoBehaviour
{
    public TriggerZone triggerZone;
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

        // Notify the TriggerZone that this glass was picked up
        if (triggerZone != null)
        {
            triggerZone.OnGlassPickedUp(gameObject);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log("[GlassPickupListener] Glass dropped!");

        ZoneVisualManager.Instance?.HideAllZones();
    }
}