using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class GeminiChat : MonoBehaviour
{
    // Your API key (hardcoded for prototype; don't ship this!)
    public static string apiKey = "AIzaSyBHy9uR1e21KPrzE7zLrMLWTSlVbu3fVbA"; // Replace with your key from Step 1
    public static string model = "gemini-2.5-flash-lite";

    // API endpoint (use gemini-1.5-flash for free tier; swap to gemini-1.5-pro if needed)
    public string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

    // Chat history: List of message objects (role: "user" or "model", text content)
    public List<ChatMessage> chatHistory = new List<ChatMessage>();

    // Simple classes for JSON serialization (matches API format)
    [System.Serializable]
    public class ChatMessage
    {
        public string role;
        public List<Part> parts = new List<Part>();
    }

    [System.Serializable]
    public class Part
    {
        public string text;
    }

    [System.Serializable]
    public class RequestBody
    {
        public List<ChatMessage> contents = new List<ChatMessage>();
    }

    [System.Serializable]
    public class ResponseBody
    {
        public List<Candidate> candidates;
    }

    [System.Serializable]
    public class Candidate
    {
        public ChatMessage content;
    }

    void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("Error. Check internet connection!");
        }
        else
        {
            Debug.Log("Internet connected!");
        }
            // Optional: Start with a system prompt or initial message
            AddMessage("model", "You are a helpful AI assistant.");
    }

    void Update()
    {
        // Test: Press Space to send a sample message and log response
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SendMessage("Tell me a joke about Unity game development."));
        }
    }

    // Add a message to history (user or model)
    private void AddMessage(string role, string text)
    {
        var message = new ChatMessage{ role = role };
        message.parts.Add(new Part { text = text });
        chatHistory.Add(message);

    //    Debug.Log($"message role:{message.role} message part: {message.parts[0].text}");
    //    for (int i = 0; i < message.parts.Count; i ++)
    //    {
    //        Debug.Log(i);
    //        Debug.Log($"message part: {message.parts[0]}");
    //    }

    }

    // Send user message, get AI response, and append to history
    new public IEnumerator SendMessage(string userText)
    {
        // Add user's new message to history
        AddMessage("user", userText);
        Debug.Log("User: " + userText);

        // Build request body with full history

        //for (int i = 0; i < chatHistory.Count; i++)
        //{
        //    Debug.Log($"i: {i}\n role: {chatHistory[i].role} mes: {chatHistory[i].parts[0].text}");
        //}

        var requestBody = new RequestBody { contents = chatHistory };
        //for (int i = 0; i < requestBody.contents.Count; i++)
        //{
        //    Debug.Log($"i: {i}\n role: {requestBody.contents[i].role} mes: {requestBody.contents[i].parts[0].text}");
        //}


        var jsonBody = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

        Debug.Log(jsonBody);

        // Set up POST request
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("x-goog-api-key", apiKey); // Auth via header
            // Send and wait
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
            else
            {
                // Parse response
                string responseJson = request.downloadHandler.text;
                ResponseBody response = JsonUtility.FromJson<ResponseBody>(responseJson);

                if (response.candidates != null && response.candidates.Count > 0)
                {
                    string aiText = response.candidates[0].content.parts[0].text;
                    Debug.Log("AI: " + aiText);

                    // Append AI response to history for next turn
                    AddMessage("model", aiText);
                }
                else
                {
                    Debug.LogError("No response from AI.");
                }
            }
        }
    }
}