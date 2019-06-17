using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorController : MonoBehaviour
{
    const string GKSU_ERROR_TEXT = "Gksu must be installed in order to enable keyboard input.";
    const string DISABLE_KEYBOARD_TEXT = "Press any key to disable keyboard input.";

    GameObject errorWindow;
    TextMeshProUGUI errorText;
    GameObject okButton;
    GameObject uiButtons;
    

    private void Start()
    {
        errorWindow = GameObject.Find("Popup Windows").
            GetComponentInChildren<ErrorWindow>(true).gameObject;
        errorText = errorWindow.GetComponentInChildren<TextMeshProUGUI>();
        uiButtons = GameObject.Find("Main UI Buttons");
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
        if (errorWindow.activeSelf == false) { return; }

        okButton.SetActive(true);
        errorWindow.SetActive(false);
        uiButtons.SetActive(true);
    }
}
