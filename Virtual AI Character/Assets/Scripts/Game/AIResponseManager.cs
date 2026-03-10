using UnityEngine;
using TMPro;

public class AIResponseManager : MonoBehaviour {
    public GameObject aiResponsePanel;
    public TMP_Text aiResponseText;

    void OnEnable() {
        GameDataManager gameDataManager = FindObjectOfType<GameDataManager>();
        if (gameDataManager != null) {
            gameDataManager.onHighestScoreUpdated.AddListener(OnHighestScoreUpdated);
        }

        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null) {
            gameController.onGameEnd.AddListener(OnGameEnd);
        }
    }

    void OnDisable() {
        GameDataManager gameDataManager = FindObjectOfType<GameDataManager>();
        if (gameDataManager != null) {
            gameDataManager.onHighestScoreUpdated.RemoveListener(OnHighestScoreUpdated);
        }

        GameController gameController = FindObjectOfType<GameController>();
        if (gameController != null) {
            gameController.onGameEnd.RemoveListener(OnGameEnd);
        }
    }

    void OnHighestScoreUpdated() {
        // aiResponseText.text = "Placeholder :D";
        // aiResponsePanel.SetActive(true);
        // Invoke("HideResponse", 3f);
    }

    void OnGameEnd() {
        // aiResponseText.text = "Game Over! :(";
        // aiResponsePanel.SetActive(true);
        // Invoke("HideResponse", 3f);
    }

    void HideResponse() {
        aiResponsePanel.SetActive(false);
    }
}
