using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BackgroundImageMenu : MonoBehaviour
{
    public Button toggleMenuButton;
    public GameObject buttonPrefab;
    public Transform menuPanel;
    public Image backgroundImage;

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

            btn.onClick.AddListener(() =>
            {
                backgroundImage.sprite = bgSprite;
            });
        }
    }

    void ToggleMenu()
    {
        menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);
    }
}
