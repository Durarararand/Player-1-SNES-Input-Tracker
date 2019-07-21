using UnityEngine;
using UnityEngine.EventSystems;

public class ClearInput : MonoBehaviour
{
    ButtonController[] theButtonControllers;

    private void Start()
    {
        GetButtonControllers();
    }

    private void GetButtonControllers()
    {
        theButtonControllers = FindObjectsOfType<ButtonController>();
    }

    public void ClearAllInputs()
    {
        foreach (ButtonController buttonController in theButtonControllers)
        {
            buttonController.ClearInput();
        }

        // Unselect the button.
        EventSystem.current.SetSelectedGameObject(null);
    }
}
