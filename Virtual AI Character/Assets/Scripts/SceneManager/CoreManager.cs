using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoreManager : MonoBehaviour
{
    public static CoreManager instance;
    public AudioSource backgroundMusic;
    public Image backgroundImage;
    public Canvas backgroundCanvas;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Start, SceneDatabase.Scenes.StartScene)
            .Perform();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneDatabase.Scenes.ChatHistoryScene) return;

        Camera loadedSceneCamera = null;

        GameObject setupObj = null;
        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            if (rootObj.name == "--- Setup ---")
            {
                setupObj = rootObj;
                break;
            }
        }

        if (setupObj != null)
        {
            loadedSceneCamera = setupObj.GetComponentInChildren<Camera>();
            if (loadedSceneCamera != null)
            {
                backgroundCanvas.worldCamera = loadedSceneCamera;
                return;
            }
        }

        Debug.LogWarning("No setup object or camera found in the loaded scene.");
    }
}