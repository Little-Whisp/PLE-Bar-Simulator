using UnityEngine;
using System.Collections;

public class TriggerZone : MonoBehaviour
{
    [Header("Glass Placement")]
    public bool isGlassZone = false;
    public Score scoreManager;
    public GameObject zoneVisual;
    public int pointsPerGlass = 10;
    public GameObject glassPrefab;
    public Transform glassSpawnPoint;
    public PromptTrigger promptTrigger;

    [Header("Avatar Tracking")]
    public GameObject avatarInFront; // ← Assign in Inspector or dynamically

    private Vector3 initialPosition;
    private Quaternion initialRotation;

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
        if (isGlassZone && other.CompareTag("ShotGlass"))
        {
            Debug.Log("[TriggerZone] Detected ShotGlass, attempting to add points...");
            HandleGlassPlacement(other.gameObject);
        }
    }

    private void HandleGlassPlacement(GameObject glass)
    {
        // Add points
        scoreManager?.AddPoints(pointsPerGlass);

        // Play confetti
        GetComponent<ConfettiOnPlacement>()?.TriggerConfetti();

        // 🎯 Log avatar type
        if (avatarInFront != null)
        {
            string avatarType = avatarInFront.tag;
            string prompt = promptTrigger != null ? promptTrigger.GetCurrentPrompt() : "Unknown";

            Debug.Log($"[TriggerZone] Served a '{avatarType}' avatar during prompt: '{prompt}'");

            // Optionally track this in a GameManager
            GameManager.Instance?.OnAvatarServed(avatarType, prompt);
        }

        // Reset glass and prompt
        StartCoroutine(ResetGlassAfterDelay(glass, 0.5f));
        promptTrigger?.ResetPrompt();
    }

    private IEnumerator ResetGlassAfterDelay(GameObject glass, float delay)
    {
        yield return new WaitForSeconds(delay);
        glass.transform.position = initialPosition;
        glass.transform.rotation = initialRotation;

        if (glassPrefab != null && glassSpawnPoint != null)
        {
            Instantiate(glassPrefab, glassSpawnPoint.position, glassSpawnPoint.rotation);
            Debug.Log("[TriggerZone] Spawned new glass.");
        }
    }
}
