using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    enum KeyIdentifier
    {
        PRESSED,
        RELEASED
    }
    const float BUTTON_ALPHA_ON = 0.6f;
    const float BUTTON_ALPHA_OFF = 0f;
    
    ErrorController theErrorController;
    InputIndexController theInputIndexController;
    InputReaderController theInputReaderController;
    GamepadReader theGamepadReader;
    KeyboardReader theKeyboardReader;
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
        theInputIndexController = FindObjectOfType<InputIndexController>();
        theErrorController = FindObjectOfType<ErrorController>();
        theInputReaderController = 
            FindObjectOfType<InputReaderController>();
        theGamepadReader = FindObjectOfType<GamepadReader>();
        theKeyboardReader = FindObjectOfType<KeyboardReader>();
        buttonColors = GetComponentInParent<Button>().colors;
    }

    private void Update()
    {
        
        UpdateLatestInputList();

        isAxis = CheckAxisInput(inputIdentifier) 
            && 
            CheckAxisInput(latestInputList);
        isInputPressed = AxisMatch(theInputIndexController.X_AXIS_INDEX) 
            && 
            AxisMatch(theInputIndexController.Y_AXIS_INDEX);

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
        if (inputList[theInputIndexController.AXIS_IDENTIFIER_INDEX] 
            !=
            theInputIndexController.AXIS_IDENTIFIER)
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
        if (
            inputIdentifier[theInputIndexController.CONTROLLER_IDENTIFIER_INDEX]
            !=
            latestInputList[theInputIndexController.CONTROLLER_IDENTIFIER_INDEX]
            ||
            inputIdentifier[theInputIndexController.AXIS_NAME_INDEX]
            !=
            latestInputList[theInputIndexController.AXIS_NAME_INDEX]
            )
        { return isInputPressed; }

        // Run through checks to determine if the input entered means that 
        // the correct axis inputs are pressed.
        int inputIdentifierInt;
        int latestInputListInt;
        float inputIdentifierSign;
        float latestInputListSign;

        inputIdentifierInt = Mathf.Abs(int.Parse(inputIdentifier[index]));
        latestInputListInt = Mathf.Abs(int.Parse(latestInputList[index]));
        inputIdentifierSign = Mathf.Sign(int.Parse(inputIdentifier[index]));
        latestInputListSign = Mathf.Sign(int.Parse(latestInputList[index]));
        // Set the signs to be equal if either axis is 0.
        if (inputIdentifierInt == 0 || latestInputListInt == 0)
            { latestInputListSign = inputIdentifierSign; }

        if (
            inputIdentifierSign == latestInputListSign
            &&
            inputIdentifierInt <= latestInputListInt
            )
        { return true; }
        
        return false;
    }

    private bool CheckButtonInput(List<string> inputList)
    {
        /* Return whether or not the input identifies a button and not an 
         * axis.
         */
        
        if (inputList == null) { return false; }
        if (inputList[theInputIndexController.BUTTON_IDENTIFIER_INDEX] 
            != 
            theInputIndexController.BUTTON_IDENTIFIER)
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
            if (inputList[theInputIndexController.KEY_IDENTIFIER_INDEX] 
                == keyIdentifier.ToString())
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
            !isButton 
            || latestInputList[theInputIndexController.CONTROLLER_IDENTIFIER_INDEX] 
            != 
            inputIdentifier[theInputIndexController.CONTROLLER_IDENTIFIER_INDEX]
            )
        { return isInputPressed; }

        int inputSign;
        int input;
        int buttonSign;
        int button;
        // -0 is a possible identifer and needs a special case.
        
        input = 
            int.Parse(latestInputList[theInputIndexController.BUTTON_INDEX]);
        button = 
            int.Parse(inputIdentifier[theInputIndexController.BUTTON_INDEX]);

        // Set the signs.
        if (latestInputList[theInputIndexController.BUTTON_INDEX].TrimStart().Substring(0, 1) 
            == "-")
        {
            inputSign = -1;
        }
        else { inputSign = 1; }
        if (inputIdentifier[theInputIndexController.BUTTON_INDEX].TrimStart().Substring(0, 1) 
            == "-")
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

        inputState = 
            latestInputList[theInputIndexController.KEY_IDENTIFIER_INDEX];
        input = 
            latestInputList[theInputIndexController.KEY_INDEX];
        keyState = 
            inputIdentifier[theInputIndexController.KEY_IDENTIFIER_INDEX];
        key = 
            inputIdentifier[theInputIndexController.KEY_INDEX];

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

    private bool IsAnythingPluggedIn()
    {
        if (theGamepadReader.AreControllersPluggedIn()) { return true; }
        if (theKeyboardReader.IsKeyboardEnabled()) { return true; }
        return false;
    }

    private void ParseAxisInput()
    {
        if (inputIdentifier is null
            ||
            inputIdentifier[theInputIndexController.AXIS_IDENTIFIER_INDEX]
            !=
            theInputIndexController.AXIS_IDENTIFIER
            )
        { return; }

        int xAxisValue;
        int yAxisValue;
        float xAxisSign;
        float yAxisSign;
        string zeroValue;
        xAxisValue =
            Mathf.Abs(
                int.Parse(
                    inputIdentifier[theInputIndexController.X_AXIS_INDEX]
                    )
                );
        yAxisValue =
            Mathf.Abs(
                int.Parse(
                    inputIdentifier[theInputIndexController.Y_AXIS_INDEX]
                    )
                );
        xAxisSign =
            Mathf.Sign(
                int.Parse(
                    inputIdentifier[theInputIndexController.X_AXIS_INDEX]
                    )
                );
        yAxisSign =
            Mathf.Sign(
                int.Parse(
                    inputIdentifier[theInputIndexController.Y_AXIS_INDEX]
                    )
                );
        zeroValue = "0";

        // If both axes are 0, do not change anything.
        if (xAxisValue == yAxisValue && xAxisValue == 0) { return; }

        // Set the larger axis value to the deadzone threshold and the other 
        // to 0.
        if (xAxisValue >= yAxisValue)
        {
            inputIdentifier[theInputIndexController.X_AXIS_INDEX] =
                (
                    theInputIndexController.AXIS_DEADZONE
                    *
                    xAxisSign
                ).ToString();
            inputIdentifier[theInputIndexController.Y_AXIS_INDEX] =
                zeroValue;
        }
        else
        {
            inputIdentifier[theInputIndexController.Y_AXIS_INDEX] =
                (
                    theInputIndexController.AXIS_DEADZONE
                    *
                    yAxisSign
                ).ToString();
            inputIdentifier[theInputIndexController.X_AXIS_INDEX] =
                zeroValue;
        }
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
        if (!IsAnythingPluggedIn())
        {
            // Unselect the button.
            EventSystem.current.SetSelectedGameObject(null);
            // Show the error window.
            theErrorController.NoInputError();
        }
        else
        {
            inputIdentifier = 
                theInputReaderController.GetInput(inputQueueIndex);
            // Parse the axis data to account for nuances when setting the 
            // direction of analog joysticks.
            ParseAxisInput();

            // Unselect the button.
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void ClearInput()
    {
        inputIdentifier = null;
    }
}
