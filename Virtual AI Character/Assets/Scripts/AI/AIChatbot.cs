using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using Newtonsoft.Json;
using LLMUnity;
using TMPro;
using Unity.VisualScripting;

public class AIChatbot : MonoBehaviour
{
    public GameObject processStatusText;
    private static string apiKey = "AIzaSyBHy9uR1e21KPrzE7zLrMLWTSlVbu3fVbA";
    private static string model = "gemini-2.5-flash-lite";
    private string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
    string prompt_name = "system_prompt_default";

    [Header("Llama")]
    [SerializeField] private LLMCharacter aiCharacter;
    private bool isLlamaInitialized = false;

    public List<ChatMessage> chatHistory = new List<ChatMessage>();
    // persistence filename (stored in Application.persistentDataPath)
    private readonly string chatHistoryFileName = "chat_history.txt";

    public Dictionary<string, float> currentEmotions = new Dictionary<string, float>();

 
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

    // prompt -----------------------------------------------------------------
    public void SetPromptName(string newPromptName)
    {
        if (string.IsNullOrEmpty(newPromptName))
            return;
        prompt_name = newPromptName;
        ApplySystemPrompt();
    }

    public void ApplySystemPrompt(bool addonly=false)
    {
        TextAsset promptAsset = Resources.Load<TextAsset>($"prompt/{prompt_name}");
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

        ReplaceOrAddSystemPrompt(systemPrompt, addonly);
    }

    private void ReplaceOrAddSystemPrompt(string systemPrompt, bool addonly)
    {
        if (string.IsNullOrEmpty(systemPrompt))
            return;

        if (addonly)
        {
            AddMessage("model", systemPrompt);
            return;
        }

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

    // add message to history ------------------------------------------------
    private void AddMessage(string role, string text)
    {
        var message = new ChatMessage { role = role };
        message.parts.Add(new Part { text = text });
        chatHistory.Add(message);

        if (isLlamaInitialized && aiCharacter != null)
        {
            aiCharacter.AddMessage(text, role);
        }
        
        SaveChatHistoryToFile();
    }

    private string GetChatHistoryPath()
    {
        return Path.Combine(Application.persistentDataPath, chatHistoryFileName);
    }

    private bool SaveChatHistoryToFile()
    {
        try
        {
            var path = GetChatHistoryPath();
            var json = JsonConvert.SerializeObject(chatHistory, Formatting.Indented);
            File.WriteAllText(path, json);
            Debug.Log($"Saved chat history to {path}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed saving chat history: {ex}");
            return false;
        }
    }

    private bool LoadChatHistoryFromFile()
    {
        try
        {
            var path = GetChatHistoryPath();
            if (!File.Exists(path)) return false;
            var json = File.ReadAllText(path);
            var loaded = JsonConvert.DeserializeObject<List<ChatMessage>>(json);
            if (loaded == null) return false;
            chatHistory = loaded;
            Debug.Log($"Loaded chat history from {path} ({chatHistory.Count} messages)");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed loading chat history: {ex}");
            return false;
        }
    }

    // start -----------------------------------------------------------------
    void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("No internet! Will use Llama fallback.");
        }
        else
        {
            Debug.Log("Internet connected!");
        }
        bool loaded = LoadChatHistoryFromFile();
        if (!loaded)
        {
            TextAsset promptAsset = Resources.Load<TextAsset>($"prompt/{prompt_name}");
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
    }

    // parse json and return text + emotion dictionary-----------------------------------------------------------
    private void ParseAIResponse(string raw, out string textOut, out Dictionary<string, float> emotions)
    {
        textOut = raw ?? "";
        emotions = new Dictionary<string, float>();

        if (string.IsNullOrWhiteSpace(raw))
            return;

        int first = raw.IndexOf('{');
        int last = raw.LastIndexOf('}');
        string candidate = raw;
        if (first >= 0 && last > first)
        {
            candidate = raw.Substring(first, last - first + 1);
            Debug.Log("Raw Json: " + candidate);
        }
        else
        {
            Debug.LogWarning("No JSON object found in response.");
            Debug.Log("first: " + first + ", last: " + last);
            ApplySystemPrompt(true);
            Debug.Log("prompt reapplied.............");
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
        }
    }

    // models init and call ------------------------------------------------------------------------------
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

        foreach (var msg in chatHistory)
        {
            aiCharacter.AddMessage(msg.parts[0].text, msg.role);
        }

        isLlamaInitialized = true;
        Debug.Log("Llama initialized and chat history synced.");
    }

        public IEnumerator GetAIResponse(string userInput, System.Action<string> onComplete)
    {
        string aiResponse = "";
        bool usedGemini = false;

        AddMessage("user", userInput);
        Debug.Log("User: " + userInput);

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
                        processStatusText.GetComponent<TMP_Text>().text = "Received response from\nGemini (online)";
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
                        onComplete?.Invoke(parsedText);
                        usedGemini = true;
                        yield break;
                    }
                    else Debug.LogWarning("No response from Gemini AI.");
                }
                else Debug.LogWarning($"Gemini failed: {request.error}. {request.downloadHandler.text}. Falling back to Llama.");
            }
        }
        else Debug.LogWarning("No internet. Falling back to Llama.");

        if (!usedGemini)
        {
            if (!isLlamaInitialized) InitializeLlama();
            yield return GenerateLlamaResponse(userInput, (llamaResult) =>
            {
                processStatusText.GetComponent<TMP_Text>().text = "Received response from\nLlama (offline)";
                AddMessage("model", llamaResult);
                onComplete?.Invoke(llamaResult);
            });
        }
    } 

    private IEnumerator GenerateLlamaResponse(string userText, System.Action<string> onComplete)
    {
        Debug.Log("Llama start generating response");
        string aiResponse = "";
        bool completed = false;

        aiCharacter.Chat(userText,
                        (reply) => { aiResponse += reply; },
                        () => { completed = true; });

        yield return new WaitUntil(() => completed);

        ParseAIResponse(aiResponse, out string llamaText, out Dictionary<string, float> llamaEmotions);
        currentEmotions = llamaEmotions ?? new Dictionary<string, float>();
        Debug.Log($"Llama parsed text: {llamaText}");
        if (llamaEmotions.Count > 0)
        {
            foreach (var kv in llamaEmotions)
                Debug.Log($"{kv.Key}: {kv.Value}");
        }

        onComplete?.Invoke(llamaText);
    }
}