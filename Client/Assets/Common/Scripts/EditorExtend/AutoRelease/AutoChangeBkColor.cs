using UnityEngine;
using System;
using System.Collections;


public class AutoChangeBkColor : IDisposable
{
    [SerializeField]
    private Color PreviousColor
    {
        get;
        set;
    }

    public AutoChangeBkColor(Color newColor)
    {
        PreviousColor = GUI.backgroundColor;
        GUI.backgroundColor = newColor;
    }

    public void Dispose()
    {
        GUI.backgroundColor = PreviousColor;
    }
}
