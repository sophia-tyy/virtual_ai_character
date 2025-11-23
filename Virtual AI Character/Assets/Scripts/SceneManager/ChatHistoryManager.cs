using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatHistoryManager : MonoBehaviour
{
    public void BackToMain()
    {
        SceneController.instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.ChatHistory)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}
