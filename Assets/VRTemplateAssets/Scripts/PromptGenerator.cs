using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PromptGenerator : MonoBehaviour
{
    public TextMeshProUGUI promptText;

    // ✅ This holds the current prompt
    public string currentPrompt;

    public void ShowPrompt(string prompt)
    {
        currentPrompt = prompt; // Save the prompt for later access
        promptText.text = prompt;
        gameObject.SetActive(true); // Show prompt visually (or animate in)
    }

    public void Hide()
    {
        currentPrompt = ""; // Clear when hiding (optional, but tidy)
        gameObject.SetActive(false); // Hide prompt
    }
}
