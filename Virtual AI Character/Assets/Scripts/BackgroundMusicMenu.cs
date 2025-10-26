using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BackgroundMusicMenu : MonoBehaviour
{
    public Button toggleMenuButton;
    public GameObject buttonPrefab;
    public Transform menuPanel;
    public AudioSource audioSource;

    void Start()
    {
        toggleMenuButton.onClick.AddListener(ToggleMenu);

        Object[] musicClips = Resources.LoadAll("Music", typeof(AudioClip));
        foreach (Object musicObj in musicClips)
        {
            AudioClip musicClip = musicObj as AudioClip;
            GameObject btnObj = Instantiate(buttonPrefab, menuPanel);
            Button btn = btnObj.GetComponent<Button>();
            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            btnText.text = musicClip.name;

            btn.onClick.AddListener(() =>
            {
                audioSource.Stop();
                audioSource.clip = musicClip;
                audioSource.Play();
            });
        }
    }

    void ToggleMenu()
    {
        menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);
    }
}