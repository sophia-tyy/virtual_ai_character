
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class AffectionDataManager : MonoBehaviour {
    public static AffectionDataManager Instance;
    private AffectionData data;
    private string savePath;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "affection.json");
            System.Console.WriteLine("AffectionDataManager save path: " + savePath);
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
            data = JsonUtility.FromJson<AffectionData>(json);
            UpdateLevel();
        } else {
            data = new AffectionData();
        }
    }

    public void SaveData() {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("SAVING JSON: " + json);

    }

    private void UpdateLevel() {
        int[] thresholds = AffectionSystemConstants.AffectionLevelThresholds;
        for (int i = thresholds.Length - 1; i >= 0; i--) {
            if (data.xp >= thresholds[i]) {
                data.level = i;
                return;
            }
        }
        data.level = 0;
    }

    public void CheckStreakandResetTask() {
        DateTime today = DateTime.Now.Date;
        string todayStr = today.ToString("yyyy-MM-dd");
        DateTime lastInteractionDate = DateTime.Parse(data.lastInteractionDate).Date;
        
        if (today != lastInteractionDate) {
            foreach (var task in data.dailyTasks) task.completed = false;

            if (today.DayOfWeek == DayOfWeek.Monday) data.weeklyStreakHistory = new List<int>{-1, -1, -1, -1, -1, -1, -1};
            int dayIndex = ((int)today.DayOfWeek - 1 + 7) % 7;
            for (int i = 0; i < dayIndex; i++){
                if (data.weeklyStreakHistory[i] == -1) data.weeklyStreakHistory[i] = 0;
            }
            data.weeklyStreakHistory[dayIndex] = 1;
            
            if (today == lastInteractionDate.AddDays(1).Date) data.streak++;
            else data.streak = 1;
            if (data.streak > data.longestStreak) data.longestStreak = data.streak;
        }
        data.lastInteractionDate = todayStr;
    }

    public void FinishTask(string task) {
        var index = data.dailyTasks.FindIndex(t => t.taskName == task);
        if (index == -1 || data.dailyTasks[index].completed) return;
        
        data.dailyTasks[index] = new AffectionData.TaskProgress(data.dailyTasks[index].taskName, data.dailyTasks[index].xp, true);
        data.xp += data.dailyTasks[index].xp;

        UpdateLevel();
        SaveData();

        Debug.Log($"Updated {task} to completed. Tasks count: {data.dailyTasks.Count}");
    }


    public int GetXP() => data.xp;
    public int GetLevel() => data.level;
    public int GetStreak() => data.streak;
    public int GetLongestStreak() => data.longestStreak;
    public List<AffectionData.TaskProgress> GetDailyTasks() => data.dailyTasks;
    public List<int> GetWeeklyStreakHistory() => data.weeklyStreakHistory;
}
