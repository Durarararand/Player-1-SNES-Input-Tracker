using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using UnityEngine;

public class ReadGamepad : MonoBehaviour
{
    const string EXE_PATH = "joystick_SNESInputTracker.sh";
    const string EXE_ARGS = "";
    const int TRIM_QUEUE_INTERVAL_IN_MILLISECONDS = 60000;
    const int ITEM_COUNT_TO_TRIGGER_TRIM_QUEUE = 10000;

    //DebugToScreen theDebugToScreen;
    ButtonController[] theButtonControllers;
    System.Timers.Timer trimTimer;

    List<List<string>> inputQueue;
    Process gamepadProc;

    bool isSomeoneLookingForInput = false;

    private void Start()
    {
        //theDebugToScreen = FindObjectOfType<DebugToScreen>();
        theButtonControllers = FindObjectsOfType<ButtonController>();

        SetCurrentDirectory();
        inputQueue = new List<List<string>>();
        Thread readInputThread = new Thread(RunGamepadReader);
        readInputThread.Start();

        TrimQueueTimer();
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
            inputQueue.Add(reader.ReadLine().Split(',').ToList());
            isSomeoneLookingForInput = false;
        }
    }

    private void TrimQueueTimer()
    {
        trimTimer = new System.Timers.Timer();
        trimTimer.Elapsed += new ElapsedEventHandler(TrimQueue);
        trimTimer.Interval = TRIM_QUEUE_INTERVAL_IN_MILLISECONDS;
        trimTimer.AutoReset = false;
        trimTimer.Enabled = true;
    }

    private void TrimQueue(
        object source, 
        ElapsedEventArgs theElapsedEventArgs
        )
    {
        /*
         * Remove elements from the queue which have already been read.  This 
         * is run on a periodic bases, but more aggressively if the queue 
         * length is high.
         */

        do
        {
            int lowestButtonIndex = int.MaxValue;
            int aButtonControllerIndex;

            // Get the lowest index of all of the button controllers.
            foreach (
                ButtonController aButtonController in theButtonControllers
                )
            {
                aButtonControllerIndex =
                    aButtonController.CheckButtonQueueIndex();
                if (aButtonControllerIndex <= lowestButtonIndex)
                {
                    lowestButtonIndex = aButtonControllerIndex;
                }
            }

            // Set the indices of the button controllers to the earliest 
            // unread input index.
            foreach (
                ButtonController aButtonController in theButtonControllers
                )
            {
                aButtonControllerIndex =
                    aButtonController.CheckButtonQueueIndex();
                aButtonController.SetButtonQueueIndex(
                    aButtonControllerIndex - lowestButtonIndex
                    );
            }

            // Trim the queue.
            inputQueue.RemoveRange(0, lowestButtonIndex);
        } while (CheckQueueLength() >= ITEM_COUNT_TO_TRIGGER_TRIM_QUEUE);

        // Restart the timer.
        trimTimer.Start();
    }

    private void KillInputReaderOnExit()
    {
        gamepadProc.Kill();

        Process killProcess;
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "killall",
            Arguments = "joystick_SNESInputTracker",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        killProcess = Process.Start(startInfo);
    }

    public int CheckQueueLength()
    {
        if (inputQueue is null) { return 0; }
        return inputQueue.Count;
    }

    public List<string> GetInput(int index)
    {
        /*
         * Used to set the input to a button.  This method will wait until 
         * the user presses a gamepad input before proceeding.
         */
        isSomeoneLookingForInput = true;
        // Wait for the input to be available.
        while (isSomeoneLookingForInput)
        {
            continue;
        }
        return inputQueue[index];
    }

    public List<string> CheckInput(int index)
    {
        /*
         * Return whatever the latest input from the gamepad is.
         */
        return inputQueue[index];
    }
}
