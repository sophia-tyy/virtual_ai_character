using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class MissionMenu : MonoBehaviour
{
    public GameObject checklistPrefab;
    public Transform menuPanel;
    private Sprite missionToDoIcon;
    private Sprite missionCompleteIcon;

    void OnEnable()
    {
        List<AffectionData.TaskProgress> dailyTasks = AffectionDataManager.Instance.GetDailyTasks();
        missionCompleteIcon = Resources.Load<Sprite>("MissionIcon/mission_complete");
        missionToDoIcon = Resources.Load<Sprite>("MissionIcon/mission_todo");

        foreach (var task in dailyTasks)
        {
            GameObject textObj = Instantiate(checklistPrefab, menuPanel);

            TMP_Text missionText = textObj.GetComponentInChildren<TMP_Text>();
            missionText.text = task.taskName;

            GameObject missionIndicatorObj = textObj.transform.Find("finished_indicator").gameObject;

            bool isFinished = task.completed;
            if (isFinished) missionIndicatorObj.GetComponent<Image>().sprite = missionCompleteIcon;
            else missionIndicatorObj.GetComponent<Image>().sprite = missionToDoIcon;
            missionIndicatorObj.SetActive(true);
        }
    }
}