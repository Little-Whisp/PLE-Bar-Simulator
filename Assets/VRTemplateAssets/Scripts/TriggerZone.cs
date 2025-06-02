using UnityEngine;
using System.Collections;
using TMPro;

public class TriggerZone : MonoBehaviour
{
    [Header("Glass Placement")]
    public bool isGlassZone = false;
    public GameObject textBubblePrefab;
    public Score scoreManager;
    public GameObject zoneVisual;
    public int pointsPerGlass = 10;
    public GameObject glassPrefab;
    public Transform glassSpawnPoint;
    public PromptTrigger promptTrigger;

    [Header("Avatar Tracking")]
    private GameObject previousAvatar;
    private GameObject previousTextBubble;
    private GameObject previousPlacedGlass;
    private Quaternion previousAvatarRotation;

    public GameObject avatarInFront;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Quaternion originalAvatarRotation;

    [HideInInspector] public GameObject lastPlacedGlass;
    [HideInInspector] public GameObject lastSpawnedGlass;
    [HideInInspector] public GameObject lastTextBubble;

    private void Start()
    {
        if (isGlassZone)
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            Debug.Log("[TriggerZone] Saved initial position: " + initialPosition);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[TriggerZone] OnTriggerEnter called with object: " + other.gameObject.name);

        if (isGlassZone && other.CompareTag("ShotGlass"))
        {
            Debug.Log("[TriggerZone] Detected ShotGlass, attempting to add points...");
            HandleGlassPlacement(other.gameObject);
        }
    }

    private void HandleGlassPlacement(GameObject glass)
    {
        // 🧹 Clean up previous avatar's bubble and rotation
        if (previousTextBubble != null)
        {
            Debug.Log("[TriggerZone] Destroying previousTextBubble: " + previousTextBubble.name);
            Destroy(previousTextBubble);
            previousTextBubble = null;
        }
        else
        {
            Debug.Log("[TriggerZone] No previousTextBubble to remove.");
        }

        if (previousAvatar != null)
        {
            previousAvatar.transform.rotation = previousAvatarRotation;
            Debug.Log("[TriggerZone] Previous avatar rotation reset: " + previousAvatar.name);
        }
        else
        {
            Debug.Log("[TriggerZone] No previousAvatar to reset.");
        }

        // Update previous avatar + rotation + bubble tracking
        if (avatarInFront != null)
        {
            previousAvatar = avatarInFront;
            previousAvatarRotation = avatarInFront.transform.rotation;

            Vector3 forwardOffset = avatarInFront.transform.forward * 0.4f;
            Vector3 upOffset = Vector3.up * 0.15f;
            Vector3 tableCenter = transform.position + forwardOffset + upOffset;

            glass.transform.position = tableCenter;
            glass.transform.rotation = Quaternion.LookRotation(-avatarInFront.transform.forward);

            Quaternion lookRot = Quaternion.LookRotation(-avatarInFront.transform.forward);
            glass.transform.rotation = lookRot;

            // Make the avatar face the player
            Vector3 targetPos = Camera.main.transform.position;
            targetPos.y = avatarInFront.transform.position.y;
            avatarInFront.transform.LookAt(targetPos);

            // Show the bubble
            string avatarTag = avatarInFront.tag;
            string prompt = promptTrigger != null ? promptTrigger.GetCurrentPrompt() : "Unknown";
            Debug.Log($"[TriggerZone] Avatar '{avatarTag}' was served during prompt: '{prompt}'");

            string line = FindObjectOfType<AvatarReactionManager>().GetRandomReaction(avatarTag);
            previousTextBubble = ShowTextBubble(avatarInFront, line);
            Debug.Log("[TriggerZone] New text bubble shown: " + previousTextBubble.name);
        }
        else
        {
            previousAvatar = null;
            previousAvatarRotation = Quaternion.identity;
            previousTextBubble = null;
            Debug.Log("[TriggerZone] No avatar in front to assign as previous.");
        }

        // Update glass tracking
        previousPlacedGlass = lastPlacedGlass;
        lastPlacedGlass = glass;
        Debug.Log("[TriggerZone] previousPlacedGlass set to: " + previousPlacedGlass);
        Debug.Log("[TriggerZone] lastPlacedGlass set to: " + lastPlacedGlass);

        // Disable glass interaction
        var rb = glass.GetComponent<Rigidbody>();
        var col = glass.GetComponent<Collider>();
        var grab = glass.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;
        if (grab != null) grab.enabled = false;

        // Award points + effects
        scoreManager?.AddPoints(pointsPerGlass);
        GetComponent<ConfettiOnPlacement>()?.TriggerConfetti();

        StartCoroutine(SpawnNewGlassAfterDelay(0.5f));
        promptTrigger?.ResetPrompt();
    }

    private IEnumerator SpawnNewGlassAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (glassPrefab != null && glassSpawnPoint != null)
        {
            if (lastSpawnedGlass != null)
                Destroy(lastSpawnedGlass);

            GameObject newGlass = Instantiate(glassPrefab, glassSpawnPoint.position, glassSpawnPoint.rotation);
            newGlass.tag = "ShotGlass";

            // Ensure the new glass is grabbable
            var rb = newGlass.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            var col = newGlass.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            var grab = newGlass.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null) grab.enabled = true;

            // Assign this TriggerZone to the GlassPickup script
            var pickup = newGlass.GetComponent<GlassPickup>();
            if (pickup != null)
            {
                pickup.triggerZone = this;
            }

            lastSpawnedGlass = newGlass;

            Debug.Log("[TriggerZone]  New glass spawned and ready.");
        }
    }

    public void OnGlassPickedUp(GameObject grabbedGlass)
    {
        if (grabbedGlass == lastSpawnedGlass)
        {
            if (previousPlacedGlass != null)
            {
                Destroy(previousPlacedGlass);
                previousPlacedGlass = null;
                Debug.Log("[TriggerZone] Previous placed glass removed.");
            }

            if (previousTextBubble != null)
            {
                Destroy(previousTextBubble);
                previousTextBubble = null;
                Debug.Log("[TriggerZone] Previous text bubble removed.");
            }

            if (previousAvatar != null)
            {
                previousAvatar.transform.rotation = previousAvatarRotation;
                Debug.Log("[TriggerZone] Previous avatar rotation reset.");
            }
        }
    }

    private GameObject ShowTextBubble(GameObject avatar, string text)
    {
        Transform head = avatar.transform.Find("Head") ?? avatar.transform;

        GameObject bubble = Instantiate(textBubblePrefab, head);

        // Set consistent local position and size
        bubble.transform.localPosition = new Vector3(0, 0.3f, 0);
        bubble.transform.localRotation = Quaternion.identity;
        bubble.transform.localScale = Vector3.one * 0.05f; // 

        // Make the bubble face the camera
        bubble.transform.LookAt(Camera.main.transform);
        bubble.transform.Rotate(0, 180, 0);

        // Set readable font
        var textField = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (textField != null)
        {   
            textField.fontSize = 3.5f;              // Set consistent size
            textField.text = text;
        }

        return bubble;
    }

}
