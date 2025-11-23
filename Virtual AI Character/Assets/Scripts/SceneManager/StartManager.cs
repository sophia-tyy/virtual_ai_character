using UnityEngine;

public class StartManager : MonoBehaviour
{
    public void StartMain()
    {
        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Main, SceneDatabase.Scenes.MainScene)
            .Unload(SceneDatabase.Slots.Start)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}
