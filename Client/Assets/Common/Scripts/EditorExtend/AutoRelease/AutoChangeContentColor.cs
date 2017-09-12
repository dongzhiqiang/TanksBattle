using UnityEngine;
using System;
using System.Collections;


public class AutoChangeContentColor : IDisposable
{
    [SerializeField]
    private Color PreviousColor
    {
        get;
        set;
    }

    public AutoChangeContentColor(Color newColor)
    {
        PreviousColor = GUI.contentColor;
        GUI.contentColor = newColor;
    }

    public void Dispose()
    {
        GUI.contentColor = PreviousColor;
    }
}
