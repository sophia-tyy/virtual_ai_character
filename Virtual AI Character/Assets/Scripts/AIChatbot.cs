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
    public static string apiKey = "AIzaSyBHy9uR1e21KPrzE7zLrMLWTSlVbu3fVbA";
    public static string model = "gemini-2.5-flash-lite";
    public string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
    public bool geminiFail = false;
    string prompt_name = "system_prompt_default";

    public void SetPromptName(string newPromptName, bool applyImmediately = true)
    {
        if (string.IsNullOrEmpty(newPromptName))
            return;
        prompt_name = newPromptName;
        if (applyImmediately)
            ApplySystemPrompt();
    }

    // Load the system prompt from Resources using the current prompt_name and add it to history
    public void ApplySystemPrompt()
    {
        TextAsset promptAsset = Resources.Load<TextAsset>(prompt_name);
        string systemPrompt;
        if (promptAsset != null)
        {
            systemPrompt = promptAsset.text;
            Debug.Log($"Applied system prompt '{prompt_name}'");
        }
        else
        {
            systemPrompt = "You are a helpful AI assistant. For every reply return a JSON object with two fields: 'text' (the message to send to the user) and 'emotion' (an object mapping emotion names to numeric values between 0 and 1). Example: {\"text\":\"Hello!\",\"emotion\":{\"happiness\":0.5}}. If you cannot produce valid JSON, still return text in the 'text' field.";
            Debug.LogWarning($"Prompt '{prompt_name}' not found in Resources when applying system prompt. Using default prompt.");
        }

        ReplaceOrAddSystemPrompt(systemPrompt);
    }

    private void ReplaceOrAddSystemPrompt(string systemPrompt)
    {
        if (string.IsNullOrEmpty(systemPrompt))
            return;

        bool replaced = false;
        for (int i = 0; i < chatHistory.Count; i++)
        {
            var msg = chatHistory[i];
            if (msg == null) continue;
            var role = (msg.role ?? "").ToLowerInvariant();
            if (role == "model" || role == "system")
            {
                if (msg.parts == null || msg.parts.Count == 0)
                    msg.parts = new List<Part> { new Part { text = systemPrompt } };
                else
                    msg.parts[0].text = systemPrompt;

                chatHistory[i] = msg;
                replaced = true;

                if (isLlamaInitialized && aiCharacter != null)
                {
                    aiCharacter.AddMessage(systemPrompt, msg.role);
                }

                break;
            }
        }

        if (!replaced)
        {
            AddMessage("model", systemPrompt);
        }
    }

    // LLMUnity Llama settings
    [Header("Llama Fallback")]
    [SerializeField] private LLMCharacter aiCharacter;
    private bool isLlamaInitialized = false;

    // Chat history: Shared for Gemini and Llama
    public List<ChatMessage> chatHistory = new List<ChatMessage>();

    // Latest parsed emotions from the most recent model response (shared for Gemini and Llama)
    public Dictionary<string, float> currentEmotions = new Dictionary<string, float>();

    // // Helper to read an emotion value safely
    // public float GetEmotionValue(string name, float defaultValue = 0f)
    // {
    //     if (string.IsNullOrEmpty(name)) return defaultValue;
    //     if (currentEmotions != null && currentEmotions.TryGetValue(name, out float v))
    //         return v;
    //     return defaultValue;
    // }

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

    public IEnumerator SendMessageGemini(string userText)
    {
        AddMessage("user", userText);
        Debug.Log("User: " + userText);

        string aiResponse = "";

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            var requestBody = new RequestBody { contents = chatHistory };
            var jsonBody = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-goog-api-key", apiKey);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text;
                    ResponseBody response = JsonUtility.FromJson<ResponseBody>(responseJson);

                    if (response.candidates != null && response.candidates.Count > 0)
                    {
                        aiResponse = response.candidates[0].content.parts[0].text;
                        Debug.Log("Gemini AI (raw): " + aiResponse);

                        ParseAIResponse(aiResponse, out string parsedText, out Dictionary<string, float> emotions);
                        currentEmotions = emotions ?? new Dictionary<string, float>();
                        Debug.Log($"Gemini parsed text: {parsedText}");
                        if (emotions.Count > 0)
                        {
                            foreach (var kv in emotions)
                                Debug.Log($"{kv.Key}: {kv.Value}");
                        }

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

    private IEnumerator GenerateLlamaResponse(string userText, System.Action<string> onComplete)
    {
        string response = "";
        bool completed = false;
        AddMessage("user", userText);
        Debug.Log("User: " + userText);

        response = "";
        aiCharacter.Chat(userText, (reply) =>
        {
            response += reply;
        }, () =>
        {
            completed = true;
        });

        yield return new WaitUntil(() => completed);

        ParseAIResponse(response, out string llamaText, out Dictionary<string, float> llamaEmotions);
        currentEmotions = llamaEmotions ?? new Dictionary<string, float>();
        Debug.Log($"Llama parsed text: {llamaText}");
        if (llamaEmotions.Count > 0)
        {
            foreach (var kv in llamaEmotions)
                Debug.Log($"{kv.Key}: {kv.Value}");
        }

        onComplete?.Invoke(llamaText);
    }

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
                    InitializeLlama();
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