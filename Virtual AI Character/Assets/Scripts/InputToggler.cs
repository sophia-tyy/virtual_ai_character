using UnityEngine;
using UnityEngine.UI;

public class InputToggler : MonoBehaviour
{
    public Button toggleButton;
    public GameObject inputFieldObject;

    void Start()
    {
        inputFieldObject.SetActive(false);
        toggleButton.onClick.AddListener(ToggleInput);
    }

    void ToggleInput()
    {
        bool isActive = inputFieldObject.activeSelf;
        inputFieldObject.SetActive(!isActive);
    }
}