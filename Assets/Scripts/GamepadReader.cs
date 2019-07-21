using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public class GamepadReader : MonoBehaviour
{
    const string EXE_PATH = "stdbuf";
    const string EXE_ARGS = "-oL ./joystick_SNESInputTracker";
    const string PROC_TERMINATING_STRING = "EOF";
    const int GAMEPAD_NUMBER_START_INDEX = 13;
    const float READER_UPDATE_DELAY = 0f;
    const float READER_UPDATE_REPEAT = .10f;

    List<Process> gamepadProcs;
    InputIndexController theInputIndexController;
    InputReaderController theInputReaderController;
    GamepadSeeker theGamepadSeeker;
    List<int> enabledGamepadList;
    string gamepadDirectory;
    string readString;

    private void Awake()
    {
        theInputIndexController = FindObjectOfType<InputIndexController>();
        enabledGamepadList = new List<int>();
        gamepadProcs = new List<Process>();
        GetInputReader();
        GetGamepadSeeker();
        SetCurrentDirectory();
        InvokeRepeating(
            "RunGamepadReaders", READER_UPDATE_DELAY, READER_UPDATE_REPEAT
            );
        RunGamepadReaders();
    }

    private void OnApplicationQuit()
    {
        KillInputReaderOnExit();
    }

   
    private void SetCurrentDirectory()
    {
        gamepadDirectory = 
            Application.dataPath + 
            "/Resources/Read Joypad Input/";
    }

    private void GetInputReader()
    {
        theInputReaderController = 
            GetComponentInParent<InputReaderController>();
    }

    private void GetGamepadSeeker()
    {
        theGamepadSeeker = FindObjectOfType<GamepadSeeker>();
    }

    private void RunGamepadReaders()
    {
        List<string> gamepadList = theGamepadSeeker.GetGamepadList();

        // Do not process if the gamepad list is empty or null.
        if (gamepadList is null || !gamepadList.Any()) { return; }

        foreach (string gamepad in gamepadList)
        {
            int gamepadIndex = int.Parse(
                gamepad.Substring(
                    GAMEPAD_NUMBER_START_INDEX, gamepad.Length -
                    GAMEPAD_NUMBER_START_INDEX)
                    );

            if (enabledGamepadList is null ||
                !enabledGamepadList.Contains(gamepadIndex)
                )
            {
                Thread readInputThread =
                    new Thread(() => RunGamepadReader(gamepad));
                readInputThread.Start();

                // Add the index of the gamepad to enabledGamepadList.
                enabledGamepadList.Add(gamepadIndex);
            }
        }
    }

    private void RunGamepadReader(string gamepad)
    {
        /*
         * Start the process to read gamepad input.
         */
        int gamepadIndex = int.Parse(
            gamepad.Substring(
                GAMEPAD_NUMBER_START_INDEX, gamepad.Length -
                GAMEPAD_NUMBER_START_INDEX)
                );

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = EXE_PATH,
            Arguments = EXE_ARGS + " " + gamepad,
            WorkingDirectory = gamepadDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        Process gamepadProc;
        gamepadProc = Process.Start(startInfo);
        bool gamepadProcRunning = true;
        gamepadProcs.Add(gamepadProc);

        StreamReader reader = gamepadProc.StandardOutput;
        while (gamepadProcRunning && !gamepadProc.HasExited)
        {
            readString = reader.ReadLine();
            if (readString == PROC_TERMINATING_STRING)
            {
                gamepadProcRunning = false;
            }
            else
            {
                List<string> splitStringList;
                splitStringList = readString.Split(',').ToList();
                // Verify whether the input meets or exceeds the axis 
                // deadzone threshold.  Only add the input to the queue if it 
                // meets or exceeds the threshold or no the buttons on the UI 
                // are not being set.
                if (!theInputReaderController.CheckLookingForInput() 
                    || 
                    MeetsAxisThreshold(splitStringList)
                    )
                {
                    theInputReaderController.AddToInputQueue(splitStringList);
                    theInputReaderController.SetLookingForInput(false);
                }
            }
        }
        enabledGamepadList.Remove(gamepadIndex);
        gamepadProcs.Remove(gamepadProc);
    }

    private bool MeetsAxisThreshold(List<string> stringList)
    {
        // Return true this is not an axis.
        if (stringList[theInputIndexController.AXIS_IDENTIFIER_INDEX] 
            != 
            theInputIndexController.AXIS_IDENTIFIER) { return true; }
        // Return true if the input meets the threshold.
        if (Mathf.Abs(int.Parse(stringList[theInputIndexController.X_AXIS_INDEX])) 
            >= theInputIndexController.AXIS_DEADZONE
            ||
            Mathf.Abs(int.Parse(stringList[theInputIndexController.Y_AXIS_INDEX])) 
            >= theInputIndexController.AXIS_DEADZONE)
        { return true; }

        // Otherwise return false.
        return false;
    }

    private void KillInputReaderOnExit()
    {
        foreach(Process gamepadProc in gamepadProcs)
        {
            gamepadProc.Kill();
        }
    }

    public bool AreControllersPluggedIn()
    {
        if (enabledGamepadList != null && enabledGamepadList.Any())
        { return true; }
        else
        { return false; }
    }
}
