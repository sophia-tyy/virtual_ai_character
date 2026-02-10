using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void BackToMain()
    {
        SceneController.instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.Game)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}
