using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BackgroundMusicMenu : MonoBehaviour
{
    public Button toggleMenuButton;
    public Slider volumeSlider;
    public Image volumeIcon;
    public GameObject buttonPrefab;
    public Transform menuPanel;
    public AudioSource audioSource;
    private List<GameObject> selectedIndicators = new List<GameObject>();
    
    private Sprite volumeHighIcon;
    private Sprite volumeMediumIcon;
    private Sprite volumeLowIcon;
    private Sprite volumeMuteIcon;

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

        volumeHighIcon = Resources.Load<Sprite>("VolumeIcon/volume_high");
        volumeMediumIcon = Resources.Load<Sprite>("VolumeIcon/volume_medium");
        volumeLowIcon = Resources.Load<Sprite>("VolumeIcon/volume_low");
        volumeMuteIcon = Resources.Load<Sprite>("VolumeIcon/volume_mute");
        volumeSlider.value = audioSource.volume;
        volumeSlider.onValueChanged.AddListener((value) =>
        {
            audioSource.volume = value;
            if (value == 0)
                volumeIcon.sprite = volumeMuteIcon;
            else if (value <= 0.33f)
                volumeIcon.sprite = volumeLowIcon;
            else if (value <= 0.66f)
                volumeIcon.sprite = volumeMediumIcon;
            else
                volumeIcon.sprite = volumeHighIcon;
        });
        
    }

    void ToggleMenu()
    {
        menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);
    }
}