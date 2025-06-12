using UnityEngine;

public class PromptTrigger : MonoBehaviour
{
    public PromptManager promptManager;
    public PromptGenerator promptGenerator;
    private bool hasShownPrompt = false;

    public Animator bartenderAnimator; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasShownPrompt)
        {
            Debug.Log("[PromptTrigger] Player entered the prompt zone.");

            string prompt = promptManager.GetNextPrompt();  // Get next prompt
            promptGenerator.ShowNextPrompt();               // Show it on screen

            if (bartenderAnimator != null)
            {
                bartenderAnimator.SetTrigger("Talking");    // Trigger animation every time player enters
            }

            hasShownPrompt = true;
        }
    }

    public void ResetPrompt()
    {
        hasShownPrompt = false;
        promptGenerator.Hide();
    }

    public string GetCurrentPrompt()
    {
        return promptGenerator.currentPrompt;
    }
}
