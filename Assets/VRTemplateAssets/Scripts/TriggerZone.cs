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
    public GameObject avatarInFront;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

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
        var rb = glass.GetComponent<Rigidbody>();
        var col = glass.GetComponent<Collider>();
        var grab = glass.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;
        if (grab != null) grab.enabled = false;

        // Move glass in front of avatar
        if (avatarInFront != null)
        {
            Vector3 inFrontPos = avatarInFront.transform.position + avatarInFront.transform.forward * 0.4f + Vector3.up * 0.1f;
            glass.transform.position = inFrontPos;

            Quaternion lookRot = Quaternion.LookRotation(-avatarInFront.transform.forward);
            glass.transform.rotation = lookRot;
        }

        lastPlacedGlass = glass;

        if (lastTextBubble != null) Destroy(lastTextBubble);

        scoreManager?.AddPoints(pointsPerGlass);
        GetComponent<ConfettiOnPlacement>()?.TriggerConfetti();

        if (avatarInFront != null)
        {
            string avatarTag = avatarInFront.tag;
            string prompt = promptTrigger != null ? promptTrigger.GetCurrentPrompt() : "Unknown";

            Debug.Log($"[TriggerZone] Avatar '{avatarTag}' was served during prompt: '{prompt}'");

            string line = FindObjectOfType<AvatarReactionManager>().GetRandomReaction(avatarTag);
            lastTextBubble = ShowTextBubble(avatarInFront, line);

            Vector3 targetPos = Camera.main.transform.position;
            targetPos.y = avatarInFront.transform.position.y;
            avatarInFront.transform.LookAt(targetPos);
        }

        StartCoroutine(SpawnNewGlassAfterDelay(0.5f));
        promptTrigger?.ResetPrompt();
    }

    private IEnumerator SpawnNewGlassAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (glassPrefab != null && glassSpawnPoint != null)
        {
            if (lastSpawnedGlass != null) Destroy(lastSpawnedGlass);

            GameObject newGlass = Instantiate(glassPrefab, glassSpawnPoint.position, glassSpawnPoint.rotation);
            newGlass.tag = "ShotGlass";

            // Ensure the new glass is grabbable
            var rb = newGlass.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            var col = newGlass.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            var grab = newGlass.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null) grab.enabled = true;

            lastSpawnedGlass = newGlass;

            Debug.Log("[TriggerZone]  New glass spawned and ready.");
        }
    }

    public void OnGlassPickedUp(GameObject grabbedGlass)
    {
        if (lastPlacedGlass != null)
        {
            Destroy(lastPlacedGlass);
            lastPlacedGlass = null;
            Debug.Log("[TriggerZone] Old placed glass removed after new one was picked up.");
        }
        if (lastTextBubble != null)
        {
            Destroy(lastTextBubble);
            lastTextBubble = null;
            Debug.Log("[TriggerZone] Text bubble removed after new glass was picked up.");
        }
    }

    private GameObject ShowTextBubble(GameObject avatar, string text)
    {
        Transform head = avatar.transform.Find("Head") ?? avatar.transform;

        GameObject bubble = Instantiate(textBubblePrefab, head);
        bubble.transform.localPosition = new Vector3(0, 0.3f, 0);
        bubble.transform.localRotation = Quaternion.identity;
        bubble.transform.localScale = Vector3.one * 0.03f;

        bubble.transform.LookAt(Camera.main.transform);
        bubble.transform.Rotate(0, 180, 0);

        var textField = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (textField != null)
        {
            textField.text = text;
        }

        return bubble;
    }
}
