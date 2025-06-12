using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PromptManager promptManager; 

    private void Awake()
    {
        // Singleton pattern: ensures only 1 instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Reset prompts at start of playthrough
        if (promptManager != null)
        {
            promptManager.ResetCategoryCycle();
            Debug.Log("[GameManager] Prompt cycle reset.");
        }
        else
        {
            Debug.LogWarning("[GameManager] PromptManager is not assigned.");
        }
    }

    public void OnAvatarServed(string avatarTag, string prompt)
    {
        Debug.Log($"[GameManager] Player served a '{avatarTag}' based on prompt: '{prompt}'");

    }
}
