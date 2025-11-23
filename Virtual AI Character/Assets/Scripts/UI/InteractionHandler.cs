using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InteractionHandler : MonoBehaviour
{
    // input mode //
    private enum InputMode { None, Text, Audio }
    private InputMode currentInputMode = InputMode.None;
    // AI //
    public AIChatbot aiChatbot;

    [Header("References")]
    // processing indicator //
    public GameObject processingIndicator;
    public GameObject processingText;
    // content tag //
    public GameObject contentTag;
    public TMP_Text contentTagText;
    // output //
    public GameObject outputPanel;
    public TMP_Text outputText;
    // text input //
    public Button toggleTextInputButton;
    public GameObject inputPanel;
    public TMP_InputField inputField;
    // audio input //
    public Button toggleAudioInputButton;
    public GameObject audioInputPanel;
    public Button recordButton;
    public TMP_Text audioStatusText;
    private AudioClip audioInput;
    private bool isRecording = false;
    private string microphoneDevice;
    private AudioHandler audioHandler;
    // AI audio output //
    public AudioSource speechAudioSource;

    void Start()
    {
        inputPanel.SetActive(false);
        audioInputPanel.SetActive(false);
        outputPanel.SetActive(false);
        contentTag.SetActive(false);
        processingIndicator.SetActive(false);

        toggleTextInputButton.onClick.AddListener(ToggleTextInput);
        toggleAudioInputButton.onClick.AddListener(ToggleAudioInput);

        inputField.onSubmit.AddListener(OnTextInputSubmitted);

        // audio input setup //
        recordButton.onClick.AddListener(( ) => { });
        EventTrigger recordButtonTrigger = recordButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { StartRecording(); });
        recordButtonTrigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { StopRecording(); });
        recordButtonTrigger.triggers.Add(pointerUp);

        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
        }
        else
        {
            microphoneDevice = null;
            Debug.LogWarning("No microphone detected");
        }

        audioHandler = FindObjectOfType<AudioHandler>();
        if (audioHandler == null)
        {
            Debug.LogWarning("AudioHandler script not found in scene");
        }
    }

    void ToggleTextInput()
    {
        if (currentInputMode == InputMode.Text)
        {
            HideAllInputs();
        }
        else
        {
            ShowTextInput();
        }
    }

    void ToggleAudioInput()
    {
        if (currentInputMode == InputMode.Audio)
        {
            HideAllInputs();
        }
        else
        {
            ShowAudioInput();
        }
    }

    void HideAllInputs()
    {
        inputPanel.SetActive(false);
        audioInputPanel.SetActive(false);
        currentInputMode = InputMode.None;
        outputPanel.SetActive(false);
        contentTag.SetActive(false);
    }

    void ShowTextInput()
    {
        contentTagText.text = "User";
        contentTag.SetActive(true);
        inputPanel.SetActive(true);
        inputField.interactable = true;
        audioInputPanel.SetActive(false);
        outputPanel.SetActive(false);
        currentInputMode = InputMode.Text;
        inputField.text = "";
    }

    void ShowAudioInput()
    {
        contentTagText.text = "User";
        contentTag.SetActive(true);
        audioInputPanel.SetActive(true);
        inputPanel.SetActive(false);
        outputPanel.SetActive(false);
        currentInputMode = InputMode.Audio;
        audioStatusText.text = "Press and hold to record";
    }

    public void OnTextInputSubmitted(string text)
    {
        if (currentInputMode != InputMode.Text)
            return;

        inputPanel.SetActive(false);
        contentTag.SetActive(false);
        processingText.GetComponent<TMP_Text>().text = "Processing...";
        processingIndicator.SetActive(true);

        StartCoroutine(aiChatbot.GetAIResponse(text, (aiResponse) =>
        {
            audioHandler.SpeakText(aiResponse, (audioResponse) =>
            {
                speechAudioSource.clip = audioResponse;
                speechAudioSource.Play();
                processingIndicator.SetActive(false);
                contentTagText.text = "AI";
                contentTag.SetActive(true);
                outputPanel.SetActive(true);
                outputText.text = $"{aiResponse}";
                currentInputMode = InputMode.None;
            });
        }));
    }

    public void OnAudioInputReceived(string response)
    {
        if (currentInputMode != InputMode.Audio)
            return;

        contentTag.SetActive(false);

        StartCoroutine(aiChatbot.GetAIResponse(response, (aiResponse) =>
        {
            audioHandler.SpeakText(aiResponse, (audioResponse) =>
            {
                speechAudioSource.clip = audioResponse;
                speechAudioSource.Play();
                processingIndicator.SetActive(false);
                contentTagText.text = "AI";
                contentTag.SetActive(true);
                outputPanel.SetActive(true);
                outputText.text = $"{aiResponse}";
                currentInputMode = InputMode.None;
            });
        }));
    }

    void StartRecording()
    {
        if (isRecording || microphoneDevice == null) return;
        audioStatusText.text = "Now recording";
        isRecording = true;
        audioInput = Microphone.Start(microphoneDevice, false, 10, 16000);
    }

    void StopRecording()
    {
        if (!isRecording) return;
        Microphone.End(microphoneDevice);
        isRecording = false;

        audioInputPanel.SetActive(false);
        contentTag.SetActive(false);
        processingText.GetComponent<TMP_Text>().text = "Processing...";
        processingIndicator.SetActive(true);

        if (audioHandler == null)
        {
            processingIndicator.GetComponentInChildren<Text>().text = "Cannot process audio";
        }
        else
        {
            audioHandler.ProcessAudio(audioInput, OnAudioInputReceived);
        }
    }
}