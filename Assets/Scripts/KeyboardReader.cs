using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardReader : MonoBehaviour
{
    const string EXE_PATH = "stdbuf";
    const string GKSUDO_PROCESS_EXE_ARGS = "-oL ./checkForGksudo.sh";
    const string KEYBOARD_PROCESS_EXE_ARGS = "-oL ./keyboardReader.sh";
    const float BUTTON_UPDATE_DELAY = 0f;
    const float BUTTON_UPDATE_REPEAT = .5f;
    const int CHECK_FOR_INPUT_DELAY = 1000;

    bool isGksudoInstalled = false;
    bool isEnabled = false;
    bool isRunning = false;

    DebugDisplay theDebugDisplay;
    Process gksudoProc;
    Process keyboardProc;
    ProcessStartInfo gksudoProcStartInfo;
    ProcessStartInfo keyboardProcStartInfo;
    InputReaderController theInputReaderController;
    ErrorController theErrorController;
    TextMeshProUGUI enableKeyboardButtonText;

    private void Start()
    {
        theDebugDisplay = FindObjectOfType<DebugDisplay>();
        theErrorController = FindObjectOfType<ErrorController>();
        enableKeyboardButtonText = 
            GameObject.Find("Enable Keyboard").
            GetComponentInChildren<TextMeshProUGUI>();
        InvokeRepeating(
            "UpdateButtonText", BUTTON_UPDATE_DELAY, BUTTON_UPDATE_REPEAT
            );
        SetCurrentDirectory();
        SetupProcessStartInfo();
        CheckGksudoInstallation();
        GetInputReader();
        Thread readInputThread = new Thread(RunKeyboardReader);
        readInputThread.Start();
    }

    private void UpdateButtonText()
    {
        if (!isEnabled)
        {
            enableKeyboardButtonText.text = "Enable Keyboard";
            // Close the keyboard error if the keyboard process has ended.
            theErrorController.CloseKeyboardError();
        }
        else
        {
            enableKeyboardButtonText.text = "Disable Keyboard";
        }
    }

    private void SetCurrentDirectory()
    {
        Directory.SetCurrentDirectory(
            Application.dataPath +
            "/Resources/Read Keyboard Input/"
            );
    }

    private void SetupProcessStartInfo()
    {
        gksudoProcStartInfo = new ProcessStartInfo
        {
            FileName = EXE_PATH,
            Arguments = GKSUDO_PROCESS_EXE_ARGS,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        keyboardProcStartInfo = new ProcessStartInfo
        {
            FileName = EXE_PATH,
            Arguments = KEYBOARD_PROCESS_EXE_ARGS,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
    }

    private void CheckGksudoInstallation()
    {
        string output = "";
        gksudoProc = Process.Start(gksudoProcStartInfo);
        while (!gksudoProc.StandardOutput.EndOfStream)
        {
            output += gksudoProc.StandardOutput.ReadLine();
        }

        isGksudoInstalled = output == "1" ? true : false;
    }

    private void GetInputReader()
    {
        theInputReaderController = 
            GetComponentInParent<InputReaderController>();
    }

    private void KillKeyboardReader()
    {
        if (!isRunning) { return; }
        
        keyboardProc.Kill();
        isRunning = false;
    }

    private void RunKeyboardReader()
    {
        string input;
        while (true)
        {
            // Set a delay for checking the enabled status (save CPU%).
            Thread.Sleep(CHECK_FOR_INPUT_DELAY);
            while (!isEnabled) { Thread.Sleep(CHECK_FOR_INPUT_DELAY); }
            isRunning = true;

            keyboardProc = Process.Start(keyboardProcStartInfo);

            StreamReader reader = keyboardProc.StandardOutput;
            while (!keyboardProc.HasExited && !reader.EndOfStream)
            {
                input = reader.ReadLine();
                if (isEnabled)
                {
                    theInputReaderController.
                        AddToInputQueue(input.Split(',').ToList());
                    theInputReaderController.SetLookingForInput(false);
                }
            }
            isRunning = false;
            isEnabled = false;
        }
    }

    public void ToggleKeyboardReader()
    {
        // Deselect the button so pressing enter does not press it 
        // again.
        
        EventSystem.current.SetSelectedGameObject(null);
        theErrorController.GksuMissing(isGksudoInstalled);
        
        if (isGksudoInstalled)
        {
            KillKeyboardReader();
            if (!isEnabled)
            {
                isEnabled = true;
            }
            else
            {
                // Make a pop-up appear prompting the user for keyboard input.
                theErrorController.KeyboardError();
            }
        }
    }
}
