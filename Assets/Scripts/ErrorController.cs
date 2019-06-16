using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorController : MonoBehaviour
{

    GameObject errorWindow;
    GameObject uiButtons;

    private void Start()
    {
        errorWindow = GameObject.Find("Popup Windows").
            GetComponentInChildren<ErrorWindow>(true).gameObject;
        uiButtons = GameObject.Find("Main UI Buttons");
    }
    public void GksuMissing(bool isInstalled)
    {
        if (isInstalled) { return; }

        uiButtons.SetActive(false);
        errorWindow.SetActive(true);
    }

    public void CloseErrorWindow()
    {
        errorWindow.SetActive(false);
        uiButtons.SetActive(true);
    }
}
