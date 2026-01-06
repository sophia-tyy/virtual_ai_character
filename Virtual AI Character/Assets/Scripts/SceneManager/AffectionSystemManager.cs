using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffectionSystemManager : MonoBehaviour
{
    public void BackToMain()
    {
        SceneController.instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.AffectionSystem)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}
