using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugToScreen : MonoBehaviour
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
