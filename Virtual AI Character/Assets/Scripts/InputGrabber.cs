using TMPro;
using UnityEngine;

public class InputGrabber : MonoBehaviour
{
    [Header("Private Variable")]
    [SerializeField] private GameObject outputTextbox;
    [SerializeField] private TMP_Text outputText;
    [SerializeField] private string inputText;
    
    public void GetInputText(string input)
    {
        inputText = input;
        gameObject.SetActive(false);
        DisplayInput();
    }

    private void DisplayInput()
    {
        outputText.text = inputText;
        outputTextbox.SetActive(true);
    }
}