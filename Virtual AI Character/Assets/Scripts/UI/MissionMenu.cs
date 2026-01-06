using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class MissionMenu : MonoBehaviour
{
    public GameObject checklistPrefab;
    public Transform menuPanel;
    public String[] missions = { "Chat", "Deep Talk" };

    void Start()
    {
        foreach (String mission in missions)
        {
            GameObject textObj = Instantiate(checklistPrefab, menuPanel);

            TMP_Text missionText = textObj.GetComponentInChildren<TMP_Text>();
            missionText.text = mission;

            GameObject missionIndicatorObj = textObj.transform.Find("finished_indicator").gameObject;

            bool isFinished = true; // Placeholder for actual mission completion check
            if (isFinished) missionIndicatorObj.SetActive(true);
            else missionIndicatorObj.SetActive(false);
        }
    }
}