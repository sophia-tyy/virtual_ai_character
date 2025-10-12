using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using LLMUnity;

public class GeminiChat2 : MonoBehaviour
{
    // Gemini API settings
    public static string apiKey = "AIzaSyBHy9uR1e21KPrzE7zLrMLWTSlVbu3fVbA"; // Replace with your Gemini key
    public static string model = "gemini-2.5-flash-lite";
    public string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
    public bool geminiFail = false; // Track if Gemini succeeded

    // LLMUnity Llama settings
    [Header("Llama Fallback")]
    [SerializeField] private LLMCharacter aiCharacter; // Drag GameObject with LLMCharacter here
    private bool isLlamaInitialized = false;

    // Chat history: Shared for Gemini and Llama
    public List<ChatMessage> chatHistory = new List<ChatMessage>();

    // Classes for JSON serialization (unchanged)
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
        // Check internet for Gemini
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("No internet! Will use Llama fallback.");
            geminiFail = true;
        }
        else
        {
            Debug.Log("Internet connected!");
        }

        // Start with a system prompt
        AddMessage("model", "You are a helpful AI assistant.");
    }

    // Initialize Llama and sync initial chat history
    private void InitializeLlama()
    {
        if (aiCharacter == null)
        {
            Debug.LogError("LLMCharacter not assigned! Drag AICharacter GameObject to Inspector.");
            return;
        }

        // Sync existing chatHistory to Llama
        foreach (var msg in chatHistory)
        {
            aiCharacter.AddMessage(msg.parts[0].text, msg.role);
        }

        isLlamaInitialized = true;
        Debug.Log("Llama initialized and chat history synced.");
    }

    // Add a message to shared history (Gemini and Llama)
    private void AddMessage(string role, string text)
    {
        var message = new ChatMessage { role = role };
        message.parts.Add(new Part { text = text });
        chatHistory.Add(message);

        // Sync with Llama history
        if (isLlamaInitialized && aiCharacter != null)
        {
            aiCharacter.AddMessage(text, role);
        }
    }

    // Send message: Try Gemini, fallback to Llama
    public IEnumerator SendMessageGemini(string userText)
    {
        // Add user message to history
        AddMessage("user", userText);
        Debug.Log("User: " + userText);

        // Try Gemini first
        
        string aiResponse = "";

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            // Build Gemini request
            var requestBody = new RequestBody { contents = chatHistory };
            var jsonBody = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-goog-api-key", apiKey);

                // Send and wait
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text;
                    ResponseBody response = JsonUtility.FromJson<ResponseBody>(responseJson);

                    if (response.candidates != null && response.candidates.Count > 0)
                    {
                        aiResponse = response.candidates[0].content.parts[0].text;
                        Debug.Log("Gemini AI: " + aiResponse);
                        AddMessage("model", aiResponse);
                    }
                    else
                    {
                        Debug.LogWarning("No response from Gemini AI.");
                        geminiFail = true;
                    }
                }
                else
                {
                    Debug.LogWarning($"Gemini failed: {request.error}. {request.downloadHandler.text}. Falling back to Llama.");
                    geminiFail = true;
                }
            }
        }
        else
        {
            Debug.LogWarning("No internet. Falling back to Llama.");
            geminiFail = true;
        }

    }

    // Generate response using Llama
    private IEnumerator GenerateLlamaResponse(string userText, System.Action<string> onComplete)
    {
        string response = "";
        bool completed = false;
        AddMessage("user", userText);
        Debug.Log("User: " + userText);

        // Use LLMCharacter's Chat method (history already synced via AddMessage)
        aiCharacter.Chat(userText, (reply) => response += reply, () =>
        {
            // This block runs when complete, but you won't know success/failure here.
            completed = true;
        });

        // Wait for Llama to finish
        yield return new WaitUntil(() => completed);

        // Return response via callback
        onComplete?.Invoke(response);
    }

    // Test: Press Space to send a sample message
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!geminiFail)
            {
                StartCoroutine(SendMessageGemini("Tell me a joke about Unity game development."));
            }
            else
            {
                if (!isLlamaInitialized)
                {
                    InitializeLlama(); // Ensure Llama is initialized if Gemini failed
                }
                StartCoroutine(GenerateLlamaResponse("Tell me a joke about Unity game development.", (response) =>
                {
                    Debug.Log("Llama AI: " + response);
                    AddMessage("model", response);
                }));
            }
            
        }

    }
}