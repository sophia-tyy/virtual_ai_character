using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
    public void OpenChatHistory()
    {
        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.ChatHistory, SceneDatabase.Scenes.ChatHistoryScene)
            .WithOverlay()
            .Perform();
    }
}
