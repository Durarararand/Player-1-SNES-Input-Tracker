using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GamepadSeeker : MonoBehaviour
{
    const string GAMEPAD_DRIVER_PATH = "/dev/input/js";
    List<string> gamepadList;
    
    private void Awake()
    {
        gamepadList = new List<string>();
        UpdateGamepadList();
    }
    
    private void Update()
    {
        UpdateGamepadList();
    }

    private void UpdateGamepadList()
    {
        List<string> tempGamepadList = new List<string>();
        int gamepadIndex = 0;

        string gamepadIndexString = gamepadIndex.ToString();
        string fullGamepadDriverPath =
            GAMEPAD_DRIVER_PATH + gamepadIndexString;

        while (File.Exists(fullGamepadDriverPath)) {

            tempGamepadList.Add(fullGamepadDriverPath);

            ++gamepadIndex;
            gamepadIndexString = gamepadIndex.ToString();
            fullGamepadDriverPath = 
                GAMEPAD_DRIVER_PATH + gamepadIndexString;
        }

        gamepadList = tempGamepadList;
    }

    public List<string> GetGamepadList()
    {
        return gamepadList;
    }
}
