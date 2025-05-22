using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.Threading.Tasks;        

// package manager > install by name:
// com.unity.nuget.newtonsoft-json

// this code uses HTTPCLIENT instead of UNITYWEBREQUEST - use TASK + async await instead of coroutines + IENumerator

public class OpenAIFetch
{
    private readonly string apiKey;
    private readonly string model = "gpt-3.5-turbo";
    private readonly float temperature = 0.9f;
    private readonly string apiUrl = "https://api.openai.com/v1/chat/completions";
    private readonly HttpClient httpClient;

    public OpenAIFetch(string key, HttpClient client = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("API key cannot be null or empty.", nameof(key));

        apiKey = key;
        httpClient = client ?? new HttpClient();
    }

    public async Task<string> SendRequestAsync(string prompt, string personality)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Debug.LogError("Prompt cannot be empty.");
            return null;
        }

        var requestPayload = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = personality },
                new { role = "user", content = prompt + ". The result should be maximum 90 characters long. Do not use emoji." }
            },
            temperature = temperature
        };

        string jsonPayload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, apiUrl))
            {
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = content;

                HttpResponseMessage response = await httpClient.SendAsync(request);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    JObject responseObject = JObject.Parse(jsonResponse);
                    string aiResponse = responseObject["choices"]?[0]?["message"]?["content"]?.ToString();

                    if (!string.IsNullOrEmpty(aiResponse)){
                        //Debug.Log(aiResponse);
                        return aiResponse;
                    } else {
                        Debug.LogError("No response received from OpenAI.");
                        return null;
                    }
                } else{
                    Debug.LogError($"Error: {response.StatusCode} - {jsonResponse}");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception in SendRequestAsync: {ex.Message}");
            return null;
        }
    }
}




/*
// old code works with IENumerator
public class OpenAIFetch {

    private string apiKey;
    private string model = "gpt-3.5-turbo";
    private float temperature = 0.9f;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIFetch(string key) {
        apiKey = key;
    }

    public IEnumerator SendRequest(string prompt, string personality, System.Action<string> onComplete)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Debug.LogError("Prompt cannot be empty.");
            yield break;
        }

        var requestPayload = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = personality },
                new { role = "user", content = prompt + ". the result should be maximum 90 characters long." }
            },
            temperature = temperature
        };

        string jsonPayload = JsonConvert.SerializeObject(requestPayload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                JObject responseObject = JObject.Parse(jsonResponse);
                string aiResponse = responseObject["choices"]?[0]?["message"]?["content"]?.ToString();

                if (!string.IsNullOrEmpty(aiResponse)) {
                    //responseText.text = aiMessage;
                    //aiSpeech.SpeakSomeText(aiMessage);
                    Debug.Log("Received OpenAI response");
                    Debug.Log(aiResponse);
                    onComplete.Invoke(aiResponse);
                } else{
                    Debug.LogError("No response received from OpenAI.");
                }
            }
            else
            {
                Debug.LogError($"Error: {request.responseCode} - {request.error}\nResponse: {request.downloadHandler.text}");
            }
        }
    }
}
*/
