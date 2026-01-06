using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class MilestoneMenu : MonoBehaviour
{
    public GameObject infoTextPrefab;
    public Transform menuPanel;

    // to be updated with actual milestones
    public String[] milestones = { "Milestone 1", "Milestone 2" };
    public String[] unlock_time = { "Level 2", "Level 3" };

    void Start()
    {
        foreach (String milestone in milestones)
        {
            GameObject textObj = Instantiate(infoTextPrefab, menuPanel);

            GameObject milestoneObj = textObj.transform.Find("milestone_name").gameObject;
            TMP_Text milestoneText = milestoneObj.GetComponentInChildren<TMP_Text>();
            milestoneText.text = milestone;

            GameObject unlockTimeObj = textObj.transform.Find("unlock_time").gameObject;
            TMP_Text unlockTimeText = unlockTimeObj.GetComponentInChildren<TMP_Text>();
            int index = Array.IndexOf(milestones, milestone);
            unlockTimeText.text = unlock_time[index];
        }
    }
}