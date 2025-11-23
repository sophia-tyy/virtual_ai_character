using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

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
            contentText.text = File.ReadAllText(filePath);
        }
        else
        {
            contentText.text = "File not found.";
        }
    }
}
