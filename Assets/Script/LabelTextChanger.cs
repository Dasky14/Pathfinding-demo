using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LabelTextChanger : MonoBehaviour
{
    public TextMeshProUGUI text;
    public string suffix = "";

    public void SetTextFloat(float v)
    {
        if (TextExists())
        {
            text.text = v.ToString() + suffix;
        }
    }

    private bool TextExists()
    {
        if (text != null)
        {
            return true;
        }

        Debug.LogWarning("Text reference not found!", this);
        return false;
    }
}
