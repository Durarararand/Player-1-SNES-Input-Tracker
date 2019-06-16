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

    private void Start()
    {
        SetCurrentDirectory();
        GetInputReader();
        Thread readInputThread = new Thread(RunGamepadReader);
        readInputThread.Start();

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

    private void RunGamepadReader()
    {
        /*
         * Start the process to read gamepad input.
         */
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = EXE_PATH,
            Arguments = EXE_ARGS,
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
