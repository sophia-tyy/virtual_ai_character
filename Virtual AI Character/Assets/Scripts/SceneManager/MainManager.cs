using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{

    void Start()
    {
        AffectionDataManager.Instance.CheckStreakandResetTask();
        AffectionDataManager.Instance.FinishTask("Open the App");
    }

    public void OpenChatHistory()
    {
        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.ChatHistory, SceneDatabase.Scenes.ChatHistoryScene)
            .WithOverlay()
            .Perform();
    }

    public void OpenAffectionSystem()
    {
        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.AffectionSystem, SceneDatabase.Scenes.AffectionSystemScene)
            .WithOverlay()
            .Perform();
    }
}
