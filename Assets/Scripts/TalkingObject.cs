using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class TalkingObject : MonoBehaviour
{
    private OpenAIFetch aiFetch;
    private OpenAISpeech aiSpeech;
    private TMP_Text responseText;

    [System.Serializable]
    public enum VoiceOptions
    {
        Alloy,
        Echo,
        Fable,
        Onyx,
        Nova,
        Shimmer
    }

    [Header("Your Personal OpenAI Key")]
    public string OpenAiApiKey;

    [Header("Prompt instructions")]
    public string prompt;
    public string personality;
    public VoiceOptions selectedVoice;

    [Header("Speech bubble with textfield")]
    public GameObject speechBubble;

    void Start()
    {
        if (string.IsNullOrEmpty(OpenAiApiKey))
        {
            Debug.LogError("API key is missing.");
            return;
        }

        string voice = selectedVoice.ToString().ToLower();

        aiFetch = new OpenAIFetch(OpenAiApiKey);
        aiSpeech = new OpenAISpeech(OpenAiApiKey, voice);

        // Get the text field in the speech bubble
        if (speechBubble != null)
        {
            responseText = speechBubble.GetComponentInChildren<TMP_Text>();
            if (responseText == null)
            {
                Debug.LogError("TMP_Text component not found in speech bubble.");
            }
        }
        else
        {
            Debug.LogError("Speech bubble GameObject not assigned.");
        }

        Generate(); // Start AI talking
    }

    public async void Generate()
    {
        Debug.Log("Generating response...");
        string response = await aiFetch.SendRequestAsync(prompt, personality);

        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("Received empty response from AI.");
            return;
        }

        Debug.Log("AI Response: " + response);

        if (responseText != null)
        {
            responseText.text = response;
        }

        await SpeakResponseAsync(response);
    }

    async Task SpeakResponseAsync(string response)
    {
        bool success = await aiSpeech.SpeakSomeTextAsync(response);
        if (!success)
        {
            Debug.LogError("Failed to play audio response.");
        }
    }

    void Update()
    {
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}
