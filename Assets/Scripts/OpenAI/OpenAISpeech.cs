using System;
using System.IO;
using System.Net.Http;          // for using fetch to openai
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;              
using UnityEngine.Networking;   // for loading the audio file

// package manager > install by name:
// com.unity.nuget.newtonsoft-json

// NOTE
// this code uses HTTPCLIENT instead of UNITYWEBREQUEST - use async await + Task instead of coroutines

// TODO
// this saves audio files, make sure to delete them after use?

// TODO 
// REALTIME AUDIO STREAMING INSTEAD OF DOWNLOADING AUDIO FILE
// https://platform.openai.com/docs/guides/text-to-speech/voice-options

public class OpenAISpeech
{
    private const string ApiUrl = "https://api.openai.com/v1/audio/speech";
    private readonly string apiKey;
    private readonly string model = "tts-1";
    private readonly string voice;
    private readonly HttpClient httpClient;
    private readonly string saveFilePath;

    // voice options: alloy, echo, fable, onyx, nova, shimmer

    public OpenAISpeech(string apiKey, string voice = "alloy", HttpClient client = null)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));

        this.apiKey = apiKey;
        this.voice = voice;
        httpClient = client ?? new HttpClient();

        // Correctly set Authorization header at the client level
        if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "speech.mp3");
    }

    public async Task<bool> SpeakSomeTextAsync(string textToConvert)
    {
        if (string.IsNullOrEmpty(textToConvert) || textToConvert.Length > 4096)
        {
            Debug.LogError("Text input is either empty or exceeds 4096 characters.");
            return false;
        }

        Debug.Log("Now trying to speak the text...");

        var payload = new
        {
            model = model,
            voice = voice,
            input = textToConvert
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            // Create a request and correctly set headers
            using (var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl))
            {
                request.Content = content;

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    byte[] audioData = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(saveFilePath, audioData);
                    Debug.Log($"Audio saved to: {saveFilePath}");

                    await PlayAudioAsync(saveFilePath);
                    return true;
                }
                else
                {
                    Debug.LogError($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception in SpeakSomeTextAsync: {ex.Message}");
            return false;
        }
    }

    // todo if there is an audiosource already, use that to play the clip

    private async Task PlayAudioAsync(string filePath)
    {
        using (var request = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(request);
                var audioSource = new GameObject("AISpeech_AudioSource").AddComponent<AudioSource>();
                //audioSource.transform.SetParent(transform);
                audioSource.clip = clip;
                audioSource.Play();
                //Debug.Log("Playing audio...");
            }
            else
            {
                Debug.LogError($"Error playing audio: {request.error}");
            }
        }
    }
}



/*
// old code error
public class OpenAISpeech
{
    private const string ApiUrl = "https://api.openai.com/v1/audio/speech";
    private readonly string apiKey;
    private readonly string model = "tts-1";
    private readonly string voice;
    private readonly HttpClient httpClient;
    private readonly string saveFilePath;

    // voice options: alloy, echo, fable, onyx, nova, shimmer

    public OpenAISpeech(string apiKey, string voice = "alloy", HttpClient client = null)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));

        this.apiKey = apiKey;
        this.voice = voice;
        httpClient = client ?? new HttpClient();

        saveFilePath = Path.Combine(Application.persistentDataPath, "speech.mp3");
    }

    public async Task<bool> SpeakSomeTextAsync(string textToConvert)
    {
        if (string.IsNullOrEmpty(textToConvert) || textToConvert.Length > 4096)
        {
            Debug.LogError("Text input is either empty or exceeds 4096 characters.");
            return false;
        }

        Debug.Log("Now trying to speak the text...");

        var payload = new
        {
            model = model,
            voice = voice,
            input = textToConvert
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        content.Headers.Add("Authorization", $"Bearer {apiKey}");

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(ApiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                byte[] audioData = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(saveFilePath, audioData);
                Debug.Log($"Audio saved to: {saveFilePath}");

                await PlayAudioAsync(saveFilePath);
                return true;
            }
            else
            {
                Debug.LogError($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception in SpeakSomeTextAsync: {ex.Message}");
            return false;
        }
    }

    private async Task PlayAudioAsync(string filePath)
    {

        using (var request = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(request);
                var audioSource = new GameObject("AISpeech_AudioSource").AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("Playing audio...");
            }
            else
            {
                Debug.LogError($"Error playing audio: {request.error}");
            }
        }
    }
}
*/