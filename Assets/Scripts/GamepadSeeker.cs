using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GamepadSeeker : MonoBehaviour
{
    const string GAMEPAD_DRIVER_PATH = "/dev/input/js";
    const float UPDATE_LIST_DELAY = 0f;
    const float UPDATE_LIST_REPEAT = 1f;
    List<string> gamepadList;
    
    private void Awake()
    {
        gamepadList = new List<string>();
        UpdateGamepadList();
        InvokeRepeating(
            "UpdateGamepadList", UPDATE_LIST_DELAY, UPDATE_LIST_REPEAT
            );
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
