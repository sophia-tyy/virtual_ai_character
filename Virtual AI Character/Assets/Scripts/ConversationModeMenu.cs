using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConversationModeMenu : MonoBehaviour
{
    public Button toggleMenuButton;
    public GameObject buttonPrefab;
    public Transform menuPanel;
    public String[] conversationModes = { "Happy", "Caring" };

    void Start()
    {
        toggleMenuButton.onClick.AddListener(ToggleMenu);

        foreach (String conversationMode in conversationModes)
        {
            GameObject btnObj = Instantiate(buttonPrefab, menuPanel);
            Button btn = btnObj.GetComponent<Button>();
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            btnText.text = conversationMode;

            btn.onClick.AddListener(() =>
            {
                // TODO: change prompt
                Debug.Log("Conversation mode changed to " + conversationMode);
            });
        }
    }

    void ToggleMenu()
    {
        menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);
    }
}