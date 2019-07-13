﻿using System.Collections.Generic;
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
    InputReaderController theInputReaderController;
    GamepadSeeker theGamepadSeeker;
    List<int> enabledGamepadList;
    DebugDisplay theDebugDisplay;
    string readString;

    private void Start()
    {
        theDebugDisplay = FindObjectOfType<DebugDisplay>();
        enabledGamepadList = new List<int>();
        gamepadProcs = new List<Process>();
        GetInputReader();
        GetGamepadSeeker();
        InvokeRepeating(
            "RunGamepadReaders", READER_UPDATE_DELAY, READER_UPDATE_REPEAT
            );
        RunGamepadReaders();

    }

    private void Update()
    {
        string gamepadListString = "";
        foreach (int item in enabledGamepadList)
        {
            gamepadListString += item.ToString() + ", ";
        }
        theDebugDisplay.SetDebugText(gamepadListString);
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
            int gamepadIndex = int.Parse(
                gamepad.Substring(
                    GAMEPAD_NUMBER_START_INDEX, gamepad.Length -
                    GAMEPAD_NUMBER_START_INDEX)
                    );

            if (enabledGamepadList is null ||
                !enabledGamepadList.Contains(gamepadIndex)
                )
            {
                // Default the directory to where the gamepad reader file is.
                SetCurrentDirectory();
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
                theInputReaderController.
                    AddToInputQueue(readString.Split(',').ToList());
                theInputReaderController.SetLookingForInput(false);
            }
        }
        enabledGamepadList.Remove(gamepadIndex);
        gamepadProcs.Remove(gamepadProc);
    }

    private void KillInputReaderOnExit()
    {
        foreach(Process gamepadProc in gamepadProcs)
        {
            gamepadProc.Kill();
        }
    }
}
