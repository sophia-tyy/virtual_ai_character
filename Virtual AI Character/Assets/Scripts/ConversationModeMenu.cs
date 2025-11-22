using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ConversationModeMenu : MonoBehaviour
{
    public Button toggleMenuButton;
    public GameObject buttonPrefab;
    public Transform menuPanel;
    public String[] conversationModes = { "Default", "Happy", "Caring", "Fun" };
    public GameObject conModeStatusText;
    [SerializeField] private AIChatbot aiChatbot;
    private List<GameObject> selectedIndicators = new List<GameObject>();

    void Start()
    {
        toggleMenuButton.onClick.AddListener(ToggleMenu);

        foreach (String conversationMode in conversationModes)
        {
            string mode = conversationMode;
            GameObject btnObj = Instantiate(buttonPrefab, menuPanel);
            Button btn = btnObj.GetComponent<Button>();
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            btnText.text = mode;

            GameObject selectedIndicator = btnObj.transform.Find("SelectedIndicator").gameObject;
            if (mode == "Default")
            {
                selectedIndicator.SetActive(true);
                conModeStatusText.GetComponent<TMP_Text>().text = mode;
            }
            else
            {
                selectedIndicator.SetActive(false);
            }
            selectedIndicators.Add(selectedIndicator);

            btn.onClick.AddListener(() =>
            {
                string promptResourceName = "system_prompt_" + mode.ToLower();
                if (aiChatbot != null)
                {
                    aiChatbot.SetPromptName(promptResourceName);
                    Debug.Log("Conversation mode changed to " + mode + ", applied prompt: " + promptResourceName);
                    conModeStatusText.GetComponent<TMP_Text>().text = mode;
                }
                else
                {
                    Debug.LogWarning("AIChatbot reference not set on ConversationModeMenu. Assign it in the Inspector.");
                }

                foreach (GameObject indicator in selectedIndicators)
                {
                    indicator.SetActive(false);
                }
                selectedIndicator.SetActive(true);
            });
        }
    }

    void ToggleMenu()
    {
        menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);
    }
}