using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Newtonsoft.Json.Linq;
using System;

public class FileReader : MonoBehaviour
{
    public TMP_Text contentText;
    private string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "chat_history.txt");
    }

    void Start()
    {
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            try
            {
                JArray jsonArray = JArray.Parse(jsonContent);
                List<string> lines = new List<string>();

                bool firstAIMessageSkipped = false;

                foreach (JObject entry in jsonArray)
                {
                    string role = entry["role"]?.ToString() ?? "unknown";
                    JArray parts = entry["parts"] as JArray;
                    if (parts != null && parts.Count > 0)
                    {
                        string text = parts[0]["text"]?.ToString() ?? "";

                        if (text.Contains("{") || text.Contains("}")) continue;
                        if (role == "model" && !firstAIMessageSkipped)
                        {
                            firstAIMessageSkipped = true;
                            continue;
                        }

                        string label = (role == "user") ? "User" : "AI";
                        lines.Add($"{label}: {text}");
                    }
                }

                contentText.text = string.Join("\n", lines);
            }
            catch (Exception e)
            {
                contentText.text = "Error parsing file content.";
                Debug.LogError("JSON Parsing Error: " + e.Message);
            }
        }
        else
        {
            contentText.text = "File not found.";
        }
    }
}
