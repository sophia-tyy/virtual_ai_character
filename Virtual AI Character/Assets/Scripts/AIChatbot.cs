using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using LLMUnity;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AIChatbot : MonoBehaviour
{
    // Gemini API settings
    public static string apiKey = "AIzaSyBHy9uR1e21KPrzE7zLrMLWTSlVbu3fVbA"; // Replace with your Gemini key
    public static string model = "gemini-2.5-flash-lite";
    public string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
    public bool geminiFail = false; // Track if Gemini succeeded
    string prompt_name = "system_prompt"; // Name of the system prompt file in Resources

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

        ListAllResources();

        // Load system prompt from Resources/SystemPrompt.txt
        TextAsset promptAsset = Resources.Load<TextAsset>(prompt_name);
        string systemPrompt;
        if (promptAsset != null)
        {
            systemPrompt = promptAsset.text;
            Debug.Log("System prompt loaded successfully");
        }
        else
        {
            systemPrompt = "You are a helpful AI assistant. For every reply return a JSON object with two fields: 'text' (the message to send to the user) and 'emotion' (an object mapping emotion names to numeric values between 0 and 1). Example: {\"text\":\"Hello!\",\"emotion\":{\"happiness\":0.5}}. If you cannot produce valid JSON, still return text in the 'text' field.";
            Debug.LogWarning("Prompt not found in Resources! Using default prompt.");
        }

        AddMessage("model", systemPrompt);
    }

    // Parse model output that may be either pure JSON or text containing a JSON object.
    // Returns parsed text and a dictionary of emotion values (may be empty).
    private void ParseAIResponse(string raw, out string textOut, out Dictionary<string, float> emotions)
    {
        textOut = raw ?? "";
        emotions = new Dictionary<string, float>();

        if (string.IsNullOrWhiteSpace(raw))
            return;

        // Attempt to locate a JSON object within the raw string (handles surrounding commentary)
        int first = raw.IndexOf('{');
        int last = raw.LastIndexOf('}');
        string candidate = raw;
        if (first >= 0 && last > first)
        {
            candidate = raw.Substring(first, last - first + 1);
        }

        try
        {
            var j = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(candidate);
            if (j == null)
                return;

            var txt = j["text"]?.ToString();
            if (j["text"] == null) Debug.LogWarning("ParseAIResponse: 'text' field missing in JSON.");
            if (!string.IsNullOrEmpty(txt))
                textOut = txt.Trim();

            var emo = j["emotion"] as Newtonsoft.Json.Linq.JObject;
            if (j["emotion"] == null) Debug.LogWarning("ParseAIResponse: 'emotion' field missing in JSON.");
            if (emo != null)
            {
                foreach (var prop in emo.Properties())
                {
                    if (float.TryParse(prop.Value.ToString(), out float v))
                    {
                        emotions[prop.Name] = Mathf.Clamp01(v);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ParseAIResponse: failed to parse JSON, using raw text. Error: {ex.Message}");
            // leave textOut as raw
        }
    }

    // Public helper: list all assets under Assets/Resources and return their names.
    // Also logs the list to the Unity Console for quick debugging.
    public void ListAllResources()
    {
#if UNITY_EDITOR
        // In Editor: use AssetDatabase to enumerate actual files under Assets/Resources
        var guids = AssetDatabase.FindAssets("", new[] { "Assets/Resources" });
        List<string> names = new List<string>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj == null) continue;
            if (!string.IsNullOrEmpty(obj.name) && !names.Contains(obj.name))
                names.Add(obj.name);
        }
        Debug.Log($"Assets/Resources contains {names.Count} assets: {string.Join(", ", names)}");
        return;
#else
        // At runtime: enumerate what Resources includes
        var all = Resources.LoadAll<UnityEngine.Object>("");
        List<string> names = new List<string>();
        foreach (var a in all)
        {
            if (a == null) continue;
            if (!string.IsNullOrEmpty(a.name) && !names.Contains(a.name))
                names.Add(a.name);
        }

        Debug.Log($"Resources contains {names.Count} assets: {string.Join(", ", names)}");
#endif
    }

    // Initialize Llama and sync initial chat history
    private void InitializeLlama()
    {
        if (aiCharacter == null)
        {
            Debug.LogError("LLMCharacter not assigned! Drag AICharacter GameObject to Inspector.");
            return;
        }

        TextAsset promptAsset = Resources.Load<TextAsset>(prompt_name);
        string systemPrompt;
        if (promptAsset != null)
        {
            systemPrompt = promptAsset.text;
            Debug.Log("System prompt loaded for Llama");
        }
        else
        {
            systemPrompt = "You are a helpful AI assistant. For every reply return a JSON object with two fields: 'text' (the message to send to the user) and 'emotion' (an object mapping emotion names to numeric values between 0 and 1). Example: {\"text\":\"Hello!\",\"emotion\":{\"happiness\":0.5}}. If you cannot produce valid JSON, still return text in the 'text' field.";
            Debug.LogWarning("SystemPrompt not found in Resources! Using default prompt for Llama.");
        }

        aiCharacter.AddMessage(systemPrompt, "model");

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
                        Debug.Log("Gemini AI (raw): " + aiResponse);

                        // Parse model response for text + emotion
                        ParseAIResponse(aiResponse, out string parsedText, out Dictionary<string, float> emotions);
                        Debug.Log($"Gemini parsed text: {parsedText}");
                        if (emotions.Count > 0)
                        {
                            foreach (var kv in emotions)
                                Debug.Log($"{kv.Key}: {kv.Value}");
                        }

                        // Store only the user-facing text in history
                        AddMessage("model", parsedText);
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
        // The Chat callback may stream chunks; collect them and parse only after completion.
        response = "";
        aiCharacter.Chat(userText, (reply) =>
        {
            // Append chunks (some models stream), but we'll parse the full text at the end
            response += reply;
        }, () =>
        {
            completed = true;
        });

        // Wait for Llama to finish
        yield return new WaitUntil(() => completed);

        // Parse the final Llama response for JSON text+emotion and return the cleaned text
        ParseAIResponse(response, out string llamaText, out Dictionary<string, float> llamaEmotions);
        Debug.Log($"Llama parsed text: {llamaText}");
        if (llamaEmotions.Count > 0)
        {
            foreach (var kv in llamaEmotions)
                Debug.Log($"{kv.Key}: {kv.Value}");
        }

        onComplete?.Invoke(llamaText);
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
                    Debug.Log("Llama AI (raw): " + response);
                    AddMessage("model", response);
                }));
            }
            
        }

    }
}