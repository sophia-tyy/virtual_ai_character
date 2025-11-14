using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BackgroundMusicMenu : MonoBehaviour
{
    public Button toggleMenuButton;
    public GameObject buttonPrefab;
    public Transform menuPanel;
    public AudioSource audioSource;
    private List<GameObject> selectedIndicators = new List<GameObject>();

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

            GameObject selectedIndicator = btnObj.transform.Find("SelectedIndicator").gameObject;
            if (audioSource.clip == musicClip)
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
                audioSource.Stop();
                audioSource.clip = musicClip;
                audioSource.Play();

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