using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject inputPanel;
    public TMP_InputField inputField;

    public GameObject audioInputPanel;
    public Button recordButton;
    public TMP_Text audioStatusText;

    public GameObject outputPanel;
    public TMP_Text outputText;
    public Button nextInputButton;

    public Button toggleTextInputButton;
    public Button toggleAudioInputButton;

    // input mode //
    private enum InputMode { None, Text, Audio }
    private InputMode currentInputMode = InputMode.None;

    // audio input //
    private AudioClip audioInput;
    private bool isRecording = false;   // must need?
    private string microphoneDevice;
    private int sampleRate = 16000;     // what is this?
    private AudioHandler audioHandler;

    void Start()
    {
        inputPanel.SetActive(false);
        audioInputPanel.SetActive(false);
        outputPanel.SetActive(false);

        toggleTextInputButton.onClick.AddListener(ToggleTextInput);
        toggleAudioInputButton.onClick.AddListener(ToggleAudioInput);
        nextInputButton.onClick.AddListener(PrepareForNextInput);

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
    }

    void ShowTextInput()
    {
        inputPanel.SetActive(true);
        audioInputPanel.SetActive(false);
        outputPanel.SetActive(false);
        currentInputMode = InputMode.Text;
        inputField.text = "";
    }

    void ShowAudioInput()
    {
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
        outputPanel.SetActive(true);
        outputText.text = $"You just entered: {text}";
        currentInputMode = InputMode.None;
    }

    public void OnAudioInputReceived(string response)
    {
        if (currentInputMode != InputMode.Audio)
            return;

        audioInputPanel.SetActive(false);
        outputPanel.SetActive(true);
        outputText.text = $"You just said: {response}";
        currentInputMode = InputMode.None;
    }

    void PrepareForNextInput()
    {
        outputPanel.SetActive(false);
        ShowTextInput();
    }

    void StartRecording()
    {
        if (isRecording || microphoneDevice == null) return;
        audioStatusText.text = "Now recording";
        isRecording = true;
        audioInput = Microphone.Start(microphoneDevice, false, 10, sampleRate);
    }

    void StopRecording()
    {
        if (!isRecording) return;
        Microphone.End(microphoneDevice);
        isRecording = false;

        audioStatusText.text = "Processing...";
        if (audioHandler == null)
        {
            audioStatusText.text = "Cannot process audio";
        }
        else
        {
            audioHandler.ProcessAudio(audioInput, OnAudioInputReceived);
        }
    }
}