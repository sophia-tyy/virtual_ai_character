using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class StreakTracker : MonoBehaviour
{
    public GameObject dayIndicatorPrefab;
    public Transform menuPanel;
    private Sprite missionIncompleteIcon;
    private Sprite missionCompleteIcon;
    private Sprite missionToDoIcon;
    private List<string> weekLabel = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    void OnEnable()
    {
        missionCompleteIcon = Resources.Load<Sprite>("MissionIcon/mission_complete");
        missionIncompleteIcon = Resources.Load<Sprite>("MissionIcon/mission_incomplete");
        // missionToDoIcon = Resources.Load<Sprite>("MissionIcon/mission_todo");
        List<int> weeklyStreakHistory = AffectionDataManager.Instance.GetWeeklyStreakHistory();

        for (int i = 0; i < 7; i++)
        {
            GameObject dayObj = Instantiate(dayIndicatorPrefab, menuPanel);

            TMP_Text dayText = dayObj.GetComponentInChildren<TMP_Text>();
            dayText.text = weekLabel[i];

            GameObject dayIndicatorObj = dayObj.transform.Find("icon").gameObject;
            if (weeklyStreakHistory[i] == 1) dayIndicatorObj.GetComponent<Image>().sprite = missionCompleteIcon;
            else if (weeklyStreakHistory[i] == 0) dayIndicatorObj.GetComponent<Image>().sprite = missionIncompleteIcon;
            // else dayIndicatorObj.GetComponent<Image>().sprite = missionToDoIcon;
        }
    }
}