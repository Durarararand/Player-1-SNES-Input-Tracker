using UnityEngine;
using UnityEngine.UI;

public class DebugDisplay : MonoBehaviour
{
    Text debugText;

    private void Start()
    {
        debugText = GetComponentInChildren<Text>();
        SetDebugText("");
    }

    public void SetDebugText(string newText)
    {
        debugText.text = newText;
    }
}
