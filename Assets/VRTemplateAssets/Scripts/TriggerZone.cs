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

    [Header("Audio Reactions")]
    public AudioClip defaultVoiceClip;
    public AudioClip[] characterVoiceClips;

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
            Destroy(previousTextBubble);
            previousTextBubble = null;
        }

        if (previousAvatar != null)
        {
            previousAvatar.transform.rotation = previousAvatarRotation;
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

            Vector3 targetPos = Camera.main.transform.position;
            targetPos.y = avatarInFront.transform.position.y;
            avatarInFront.transform.LookAt(targetPos);

            string avatarTag = avatarInFront.tag;
            string prompt = promptTrigger != null ? promptTrigger.GetCurrentPrompt() : "Unknown";

            string line = FindObjectOfType<AvatarReactionManager>().GetRandomReaction(avatarTag);
            previousTextBubble = ShowTextBubble(avatarInFront, line);

            // 🎤 Play voice
            PlayAvatarVoice(avatarInFront);
        }
        else
        {
            previousAvatar = null;
            previousAvatarRotation = Quaternion.identity;
            previousTextBubble = null;
        }

        // Update glass tracking
        previousPlacedGlass = lastPlacedGlass;
        lastPlacedGlass = glass;

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

    private void PlayAvatarVoice(GameObject avatar)
    {
        if (avatar == null) return;

        AudioSource audioSource = avatar.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = avatar.AddComponent<AudioSource>();
        }

        AudioClip clipToPlay = GetVoiceClipForAvatar(avatar.tag);
        if (clipToPlay != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }

    private AudioClip GetVoiceClipForAvatar(string tag)
    {
        switch (tag)
        {
            case "Avatar1":
                return characterVoiceClips.Length > 0 ? characterVoiceClips[0] : defaultVoiceClip;
            case "Avatar2":
                return characterVoiceClips.Length > 1 ? characterVoiceClips[1] : defaultVoiceClip;
            case "Avatar3":
                return characterVoiceClips.Length > 2 ? characterVoiceClips[2] : defaultVoiceClip;
            default:
                return defaultVoiceClip;
        }
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

            var rb = newGlass.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            var col = newGlass.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            var grab = newGlass.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null) grab.enabled = true;

            var pickup = newGlass.GetComponent<GlassPickup>();
            if (pickup != null)
            {
                pickup.triggerZone = this;
            }

            lastSpawnedGlass = newGlass;
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
            }

            if (previousTextBubble != null)
            {
                Destroy(previousTextBubble);
                previousTextBubble = null;
            }

            if (previousAvatar != null)
            {
                previousAvatar.transform.rotation = previousAvatarRotation;
            }
        }
    }

    private GameObject ShowTextBubble(GameObject avatar, string text)
    {
        Transform head = avatar.transform.Find("Head") ?? avatar.transform;

        GameObject bubble = Instantiate(textBubblePrefab, head);
        bubble.transform.localPosition = new Vector3(0, 0.3f, 0);
        bubble.transform.localRotation = Quaternion.identity;
        bubble.transform.localScale = Vector3.one * 0.05f;

        bubble.transform.LookAt(Camera.main.transform);
        bubble.transform.Rotate(0, 180, 0);

        var textField = bubble.GetComponentInChildren<TextMeshProUGUI>();
        if (textField != null)
        {
            textField.fontSize = 3.5f;
            textField.text = text;
        }

        return bubble;
    }
}
