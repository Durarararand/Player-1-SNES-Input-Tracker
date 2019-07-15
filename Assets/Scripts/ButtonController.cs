using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    const int CONTROLLER_IDENTIFIER_INDEX = 0;
    const string AXIS_IDENTIFIER = "Axis";
    const int AXIS_IDENTIFIER_INDEX = 1;
    const int AXIS_NAME_INDEX = 2;
    const int X_AXIS_INDEX = 3;
    const int Y_AXIS_INDEX = 4;
    const int KEY_IDENTIFIER_INDEX = 0;
    const int KEY_INDEX = 1;
    const string BUTTON_IDENTIFIER = "Button";
    const int BUTTON_IDENTIFIER_INDEX = 1;
    const int BUTTON_INDEX = 2;
    const float BUTTON_ALPHA_ON = 0.6f;
    const float BUTTON_ALPHA_OFF = 0f;

    enum KeyIdentifier
    {
        PRESSED,
        RELEASED
    }

    DebugDisplay theDebugDisplay;
    InputReaderController theInputReaderController;
    ColorBlock buttonColors;
    float buttonAlpha;

    List<string> inputIdentifier;
    List<string> latestInputList;
    bool isAxis = false;
    bool isButton = false;
    bool isKey = false;
    bool isInputPressed = false;
    bool previousInputPressState = false;
    int inputQueueIndex = 0;

    private void Start()
    {
        theDebugDisplay = FindObjectOfType<DebugDisplay>();
        theInputReaderController = 
            FindObjectOfType<InputReaderController>();
        buttonColors = GetComponentInParent<Button>().colors;
    }

    private void Update()
    {
        
        UpdateLatestInputList();

        isAxis = CheckAxisInput(inputIdentifier) 
            && 
            CheckAxisInput(latestInputList);
        isInputPressed = AxisMatch(X_AXIS_INDEX) && AxisMatch(Y_AXIS_INDEX);

        isButton =
            CheckButtonInput(inputIdentifier)
            &&
            CheckButtonInput(latestInputList);
        isInputPressed = ButtonMatch();

        isKey = CheckKeyInput(inputIdentifier)
            &&
            CheckKeyInput(latestInputList);
        isInputPressed = KeyMatch();

        UpdateButtonUI();
        
    }

    private void UpdateLatestInputList()
    {
        if (inputQueueIndex >= theInputReaderController.CheckQueueLength())
        {
            return;
        }

        latestInputList = 
            theInputReaderController.CheckInput(inputQueueIndex);
        ++inputQueueIndex;
    }

    private bool CheckAxisInput(List<string> inputList)
    {
        /*
         * Check if the input received belongs to an axis and is not the 
         * at-rest point of the axis.  Return true if this is an axis and not 
         * the at-rest point.  Return false otherwise.
         */
        // Return the value that was already set if the inputList has 
        // not been set or this is not an axis input.
        if (inputList == null) { return false; }
        if (inputList[AXIS_IDENTIFIER_INDEX] != AXIS_IDENTIFIER)
        { return false; }
        
        return true;
    }

    private bool AxisMatch(int index)
    {
        /*
         * If an axis is pressed and the input for this button is an axis, 
         * check if they match.
         */
        // Check if this is an axis before proceeding.
        if (!isAxis) { return isInputPressed; }

        // If the axis is not the same, return the same button status.
        if (inputIdentifier[AXIS_NAME_INDEX]
            !=
            latestInputList[AXIS_NAME_INDEX]
            ||
            inputIdentifier[CONTROLLER_IDENTIFIER_INDEX]
            !=
            latestInputList[CONTROLLER_IDENTIFIER_INDEX])
        { return isInputPressed; }

        // If the axis is at rest, return false.
        if (int.Parse(latestInputList[X_AXIS_INDEX]) == 0
            &&
            int.Parse(latestInputList[Y_AXIS_INDEX]) == 0)
        { return false; }

        // Run through checks to determine if the input entered means that 
        // the correct axis inputs are pressed.
        if (int.Parse(inputIdentifier[index]) == 0) { return true; }
        else if (int.Parse(inputIdentifier[index])
            ==
            int.Parse(latestInputList[index]))
        { return true; }

        return false;
    }

    private bool CheckButtonInput(List<string> inputList)
    {
        /* Return whether or not the input identifies a button and not an 
         * axis.
         */
        
        if (inputList == null) { return false; }
        if (inputList[BUTTON_IDENTIFIER_INDEX] != BUTTON_IDENTIFIER)
        { return false; }

        return true;
    }

    private bool CheckKeyInput(List<string> inputList)
    {
        /* 
         * Return whether or not the input identifies a key and not an axis.
         */

        if (inputList == null) { return false; }
        foreach (
            KeyIdentifier keyIdentifier in 
            (KeyIdentifier[])Enum.GetValues(typeof(KeyIdentifier))
            )
        {
            if (inputList[KEY_IDENTIFIER_INDEX] == keyIdentifier.ToString())
                { return true; }
        }

        // If the input identifier was not found, return false.
        return false;
    }

    private bool ButtonMatch()
    {
        /*
         * If a button is pressed and the input for this button is not an 
         * axis, check if they match.
         */
        if (
            !isButton || latestInputList[CONTROLLER_IDENTIFIER_INDEX] 
            != 
            inputIdentifier[CONTROLLER_IDENTIFIER_INDEX]
            )
        { return isInputPressed; }

        int inputSign;
        int input;
        int buttonSign;
        int button;
        // -0 is a possible identifer and needs a special case.
        
        input =  int.Parse(latestInputList[BUTTON_INDEX]);
        button = int.Parse(inputIdentifier[BUTTON_INDEX]);

        // Set the signs.
        if (latestInputList[BUTTON_INDEX].TrimStart().Substring(0, 1) == "-")
        {
            inputSign = -1;
        }
        else { inputSign = 1; }
        if (inputIdentifier[BUTTON_INDEX].TrimStart().Substring(0, 1) == "-")
        {
            buttonSign = -1;
        }
        else { buttonSign = 1; }

        // Make the input and button positive, since we store the signs
        //separately.
        input = Mathf.Abs(input);
        button = Mathf.Abs(button);

        // Check for matches and toggle the button state.
        if (inputSign == buttonSign && input == button) { return true; }
        if (inputSign != buttonSign && input == button) { return false; }

        // In all other cases, the button is still in its previous state.
        return isInputPressed;
    }

    private bool KeyMatch()
    {
        /*
         * If a key is pressed and the input for this button is not an 
         * axis, check if they match.
         */
        if (!isKey) { return isInputPressed; }

        string inputState;
        string input;
        string keyState;
        string key;

        inputState = latestInputList[KEY_IDENTIFIER_INDEX];
        input = latestInputList[KEY_INDEX];
        keyState = inputIdentifier[KEY_IDENTIFIER_INDEX];
        key = inputIdentifier[KEY_INDEX];

        // Check for matches and toggle the button state.
        if (inputState == keyState && input == key) { return true; }
        if (inputState != keyState && input == key) { return false; }

        // In all other cases, the button is still in its previous state.
        return isInputPressed;
    }

    private void UpdateButtonUI()
    {
        /*
         * Update whether the button is highlighted or not when input is 
         * received.
         */

        // Do no processing if the button state is still the same.
        if (previousInputPressState == isInputPressed) { return; }

        if (isInputPressed)
        {
            buttonAlpha = BUTTON_ALPHA_ON;
            buttonColors.normalColor =
                new Color(
                    buttonColors.normalColor.r,
                    buttonColors.normalColor.g,
                    buttonColors.normalColor.b,
                    buttonAlpha);
            GetComponentInParent<Button>().colors =
                buttonColors;
        }
        else
        {
            buttonAlpha = BUTTON_ALPHA_OFF;
            buttonColors.normalColor =
                new Color(
                    buttonColors.normalColor.r,
                    buttonColors.normalColor.g,
                    buttonColors.normalColor.b,
                    buttonAlpha);
            GetComponentInParent<Button>().colors =
                buttonColors;
        }

        previousInputPressState = isInputPressed;
    }

    public int CheckButtonQueueIndex()
    {
        return inputQueueIndex;
    }

    public void SetButtonQueueIndex(int index)
    {
        inputQueueIndex = index;
    }

    public void SetButton()
    {
        /*
         * When the button is pressed, set the button to read whatever input 
         * comes in next.
         */
        inputIdentifier = theInputReaderController.GetInput(inputQueueIndex);

        // Unselect the button.
        EventSystem.current.SetSelectedGameObject(null);

        
    }
}
