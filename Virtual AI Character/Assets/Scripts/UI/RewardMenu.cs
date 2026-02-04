using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech.Diagnostics.Logging;

public class RewardMenu : MonoBehaviour
{
    public GameObject infoTextPrefab;
    public Transform menuPanel;

    void OnEnable()
    {
        string[] rewards = AffectionSystemConstants.AffectionLevelRewards;
        int currentLevel = AffectionDataManager.Instance.GetLevel();

        foreach (var reward in rewards)
        {
            if (reward == "N/A") continue;
            if (currentLevel >= Array.IndexOf(rewards, reward)) continue;

            GameObject textObj = Instantiate(infoTextPrefab, menuPanel);
            TMP_Text rewardText = textObj.transform.Find("milestone_name").gameObject.GetComponent<TMP_Text>();
            rewardText.text = reward;
            TMP_Text levelText = textObj.transform.Find("unlock_time").gameObject.GetComponent<TMP_Text>();
            levelText.text = "Level " + Array.IndexOf(rewards, reward);
        }
    }
}