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
        SceneManager.sceneUnloaded += OnSceneUnloaded;
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

    private void OnSceneUnloaded(Scene scene)
    {
        Scene mainScene = SceneManager.GetSceneByName("Main");
        if (!mainScene.isLoaded) return;

        Camera MainSceneCamera = null;

        GameObject setupObj = null;
        foreach (GameObject rootObj in mainScene.GetRootGameObjects())
        {
            if (rootObj.name == "--- Setup ---")
            {
                setupObj = rootObj;
                break;
            }
            MainSceneCamera = setupObj.GetComponentInChildren<Camera>();
            if (MainSceneCamera != null)
            {
                backgroundCanvas.worldCamera = MainSceneCamera;
                return;
            }
        }

        if (setupObj != null)
        {
            MainSceneCamera = setupObj.GetComponentInChildren<Camera>();
            if (MainSceneCamera != null)
            {
                backgroundCanvas.worldCamera = MainSceneCamera;
                return;
            }
        }

        Debug.LogWarning("No setup object or camera found in scene Main.");
    }
}