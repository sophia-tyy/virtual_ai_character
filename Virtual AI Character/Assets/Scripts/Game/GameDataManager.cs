
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Events;

public class GameDataManager : MonoBehaviour {
    public static GameDataManager Instance;
    private GameData data;
    private string savePath;

    public UnityEvent onHighestScoreUpdated;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "game.json");
            System.Console.WriteLine("GameDataManager save path: " + savePath);
            LoadData();
        } else {
            Destroy(gameObject);
        }
    }

    void OnApplicationPause(bool pause) { if (pause) SaveData(); }
    void OnApplicationFocus(bool focus) { if (!focus) SaveData(); }
    void OnApplicationQuit() { SaveData(); }

    public void LoadData() {
        if (File.Exists(savePath)) {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<GameData>(json);
        } else {
            data = new GameData();
        }
    }

    public void SaveData() {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("SAVING JSON: " + json);

    }

    public void AddScore(int score) {
        data.currentScore += score;
        UpdateHighestScore();
    }

    public void ResetCurrentScore() {
        data.currentScore = 0;
    }

    private void UpdateHighestScore() {
        data.highestScore = Math.Max(data.highestScore, data.currentScore);
        if (data.currentScore == data.highestScore) {
            onHighestScoreUpdated.Invoke();
        }
    }

    public int GetHighestScore() => data.highestScore;
    public int GetCurrentScore() => data.currentScore;
}
