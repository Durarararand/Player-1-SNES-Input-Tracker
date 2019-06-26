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
    
    Process gamepadProc;
    InputReaderController theInputReaderController;
    GamepadSeeker theGamepadSeeker;

    private void Start()
    {
        SetCurrentDirectory();
        GetInputReader();
        GetGamepadSeeker();
        RunGamepadReaders();

    }

    private void OnApplicationQuit()
    {
        KillInputReaderOnExit();
    }

   
    private void SetCurrentDirectory()
    {
        Directory.SetCurrentDirectory(
            Application.dataPath + 
            "/Resources/Read Joypad Input/"
            );
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
            Thread readInputThread = 
                new Thread(() => RunGamepadReader(gamepad));
            readInputThread.Start();
        }
    }

    private void RunGamepadReader(string gamepad)
    {
        /*
         * Start the process to read gamepad input.
         */
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = EXE_PATH,
            Arguments = EXE_ARGS + " " + gamepad,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        gamepadProc = Process.Start(startInfo);

        StreamReader reader = gamepadProc.StandardOutput;
        while (!gamepadProc.HasExited)
        {
            theInputReaderController.
                AddToInputQueue(reader.ReadLine().Split(',').ToList());
            theInputReaderController.SetLookingForInput(false);
        }
    }

    private void KillInputReaderOnExit()
    {
        gamepadProc.Kill();
    }
}
