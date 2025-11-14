using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BackgroundImageMenu : MonoBehaviour
{
    public Button toggleMenuButton;
    public GameObject buttonPrefab;
    public Transform menuPanel;
    public Image backgroundImage;
    private List<GameObject> selectedIndicators = new List<GameObject>();

    void Start()
    {
        toggleMenuButton.onClick.AddListener(ToggleMenu);

        Object[] backgroundSprites = Resources.LoadAll("BackgroundImage", typeof(Sprite));
        foreach (Object bgObj in backgroundSprites)
        {
            Sprite bgSprite = bgObj as Sprite;
            GameObject btnObj = Instantiate(buttonPrefab, menuPanel);
            Button btn = btnObj.GetComponent<Button>();
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            btnText.text = bgSprite.name;

            GameObject selectedIndicator = btnObj.transform.Find("SelectedIndicator").gameObject;
            if (backgroundImage.sprite == bgSprite)
            {
                selectedIndicator.SetActive(true);
            }
            else
            {
                selectedIndicator.SetActive(false);
            }
            selectedIndicators.Add(selectedIndicator);

            btn.onClick.AddListener(() =>
            {
                backgroundImage.sprite = bgSprite;

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
