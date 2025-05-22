using UnityEngine;

public class PromptTrigger : MonoBehaviour
{
    public PromptManager promptManager;
    public PromptGenerator promptGenerator;
    private bool hasShownPrompt = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasShownPrompt)
        {
            Debug.Log("[PromptTrigger] Player entered the prompt zone.");
            string prompt = promptManager.GetRandomPrompt();
            promptGenerator.ShowPrompt(prompt);
            hasShownPrompt = true;
        }
    }

public void ResetPrompt()
{
    hasShownPrompt = false;

    if (promptGenerator != null)
    {
        promptGenerator.Hide();
        Debug.Log("[PromptTrigger] Prompt hidden and reset.");
    }
}

public string GetCurrentPrompt()
{
    if (promptGenerator != null)
    {
        return promptGenerator.currentPrompt;
    }
    return "No prompt";
}

}
