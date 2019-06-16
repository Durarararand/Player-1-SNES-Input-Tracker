using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class InputReaderController : MonoBehaviour
{
    const int TRIM_QUEUE_INTERVAL_IN_MILLISECONDS = 60000;
    const int ITEM_COUNT_TO_TRIGGER_TRIM_QUEUE = 10000;

    List<List<string>> inputQueue;
    ButtonController[] theButtonControllers;
    Timer trimTimer;

    bool isSomeoneLookingForInput = false;

    private void Start()
    {
        GetButtonControllers();
        InstantiateQueue();
        TrimQueueTimer();
    }

    private void GetButtonControllers()
    {
        theButtonControllers = FindObjectsOfType<ButtonController>();
    }

    private void InstantiateQueue()
    {
        inputQueue = new List<List<string>>();
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

    private void TrimQueueTimer()
    {
        trimTimer = new Timer();
        trimTimer.Elapsed += new ElapsedEventHandler(TrimQueue);
        trimTimer.Interval = TRIM_QUEUE_INTERVAL_IN_MILLISECONDS;
        trimTimer.AutoReset = false;
        trimTimer.Enabled = true;
    }

    public void AddToInputQueue(List<string> addedElement)
    {
        inputQueue.Add(addedElement);
    }

    public int CheckQueueLength()
    {
        if (inputQueue is null) { return 0; }
        return inputQueue.Count;
    }

    public void DelFromInputQueue(int removalIndex)
    {
        inputQueue.RemoveAt(removalIndex);
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

    public bool CheckLookingForInput()
    {
        return isSomeoneLookingForInput;
    }

    public void SetLookingForInput(bool isLooking)
    {
        isSomeoneLookingForInput = isLooking;
    }
}
