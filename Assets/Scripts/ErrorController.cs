using TMPro;
using UnityEngine;

public class ErrorController : MonoBehaviour
{
    const string GKSU_ERROR_TEXT = "Gksu must be installed in order to enable keyboard input.";
    const string DISABLE_KEYBOARD_TEXT = "Press any key to disable keyboard input.";
    const string ATTACH_INPUT_TEXT = "Please plug in a controller or enable keyboard input.";
    const string NULL_TEXT = "";

    GameObject errorWindow;
    TextMeshProUGUI errorText;
    GameObject okButton;
    GameObject uiButtons;
    KeyboardReader theKeyboardReader;
    GamepadReader theGamepadReader;
    bool isKeyboardErrorOn = false;
    

    private void Start()
    {
        errorWindow = GameObject.Find("Popup Windows").
            GetComponentInChildren<ErrorWindow>(true).gameObject;
        errorText = errorWindow.GetComponentInChildren<TextMeshProUGUI>();
        uiButtons = GameObject.Find("Main UI Buttons");
        theKeyboardReader = FindObjectOfType<KeyboardReader>();
        theGamepadReader = FindObjectOfType<GamepadReader>();
    }

    public void CloseErrorWindow()
    {
        uiButtons.SetActive(true);
        errorText.text = NULL_TEXT;
        errorWindow.SetActive(false);
    }

    public void GksuMissing(bool isInstalled)
    {
        if (isInstalled) { return; }
        
        uiButtons.SetActive(false);
        errorText.text = GKSU_ERROR_TEXT;
        errorWindow.SetActive(true);
    }

    public void KeyboardError()
    {
        isKeyboardErrorOn = true;

        uiButtons.SetActive(false);
        errorWindow.SetActive(true);
        okButton = GameObject.Find("Ok Button");
        // Do not show the okButton for disabling the keyboard.
        okButton.SetActive(false);
        errorText.text = DISABLE_KEYBOARD_TEXT;
    }

    public void DisableGksuError()
    {
        errorWindow.SetActive(false);
        uiButtons.SetActive(true);
    }

    public void CloseKeyboardError()
    {
        if (!isKeyboardErrorOn) { return; }
        if (errorWindow.activeSelf == false) { return; }

        isKeyboardErrorOn = false;
        okButton.SetActive(true);
        errorWindow.SetActive(false);
        uiButtons.SetActive(true);
    }

    public void NoInputError()
    {
        if (isKeyboardErrorOn) { return; }

        bool inputEnabled;
        inputEnabled = theKeyboardReader.IsKeyboardEnabled()
            ||
            theGamepadReader.AreControllersPluggedIn();

        if (inputEnabled) { return; }
        

        uiButtons.SetActive(false);
        errorText.text = ATTACH_INPUT_TEXT;
        errorWindow.SetActive(true);
    }
}
