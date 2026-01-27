using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AffectionData {
    public int xp = 0;
    public int level = 0;
    public int streak = 0;
    public int longestStreak = 0;
    public string lastInteractionDate = "";
    public List<int> weeklyStreakHistory = new List<int>{-1, -1, -1, -1, -1, -1, -1};
    [SerializeField] public List<TaskProgress> dailyTasks = new List<TaskProgress>();
    
    [System.Serializable]
    public class TaskProgress {
        public string taskName;
        public int xp;
        public bool completed;
        
        public TaskProgress(string name, int xp, bool completed = false) {
            taskName = name;
            this.xp = xp;
            this.completed = completed;
        }
    }
}