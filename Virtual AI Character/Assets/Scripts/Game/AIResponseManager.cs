using UnityEngine;
using TMPro;
using System.Collections;

public class AIResponseManager : MonoBehaviour {
    public GameObject aiResponsePanel;
    public TMP_Text aiResponseText;
    public AudioSource aiAudioSource;
    private AIChatbot aiChatbot;
    private AudioHandler audioHandler;

    void Awake() {
        aiChatbot = FindObjectOfType<AIChatbot>();
        audioHandler = FindObjectOfType<AudioHandler>();
        if (aiChatbot == null)
            Debug.LogError("AIChatbot not found");
        if (audioHandler == null)
            Debug.LogError("AudioHandler not found");
    }

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

    private IEnumerator HandleAIResponse(string prompt, System.Action<string> onResponseReady)
    {
        if (aiChatbot == null || audioHandler == null)
        {
            onResponseReady?.Invoke("AI not ready in AIResponseManager");
            yield break;
        }

        string aiText = "";

        yield return StartCoroutine(aiChatbot.GetAIResponse(prompt, (response) =>
        {
            aiText = response;
        }));

        if (string.IsNullOrEmpty(aiText))
        {
            Debug.LogWarning("AIChatbot returned empty in AIResponseManager");
            aiText = "Nice try!";
        }

        audioHandler.SpeakText(aiText, (audioClip) =>
        {
            if (audioClip != null && aiAudioSource != null)
            {
                aiAudioSource.clip = audioClip;
                aiAudioSource.Play();
            }
        });

        onResponseReady?.Invoke(aiText);
    }

    void OnHighestScoreUpdated() {
        string prompt = "I just got a new highest score in my Snack minigame! Give me a short, fun and excited sentence to celebrate this achievement.";
        
        StartCoroutine(HandleAIResponse(prompt, (aiText) =>
        {
            aiResponseText.text = aiText;
            aiResponsePanel.SetActive(true);
            Invoke("HideResponse", 8f);
        }));
    }

    void OnGameEnd() {
        string prompt = "I just finished the Snake game and died. Give me a short, encouraging sentence to cheer me up and encourage me to try again.";
        
        StartCoroutine(HandleAIResponse(prompt, (aiText) =>
        {
            aiResponseText.text = aiText;
            aiResponsePanel.SetActive(true);
            Invoke("HideResponse", 8f);
        }));
    }

    void HideResponse() {
        aiResponsePanel.SetActive(false);
    }
}
