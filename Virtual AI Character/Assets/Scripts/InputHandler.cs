using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject inputPanel;
    public TMP_InputField inputField;

    public GameObject audioInputPanel;
    // Add audio input UI elements here as needed, e.g., Button to start/stop recording

    public GameObject outputPanel;
    public TMP_Text outputText;
    public Button nextInputButton;

    public Button toggleTextInputButton;
    public Button toggleAudioInputButton;

    private enum InputMode { None, Text, Audio }
    private InputMode currentInputMode = InputMode.None;

    void Start()
    {
        inputPanel.SetActive(false);
        audioInputPanel.SetActive(false);
        outputPanel.SetActive(false);

        toggleTextInputButton.onClick.AddListener(ToggleTextInput);
        toggleAudioInputButton.onClick.AddListener(ToggleAudioInput);
        nextInputButton.onClick.AddListener(PrepareForNextInput);

        inputField.onSubmit.AddListener(OnTextInputSubmitted);
        // Setup audio input submission callbacks as needed
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
        // Initialize audio recording UI or state here
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

    // Call this method when audio input finishes and delivers a response string
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
}