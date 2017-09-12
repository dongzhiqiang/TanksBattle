using UnityEngine;
using System;
using System.Collections;


public class AutoChangeColor : IDisposable
{
    [SerializeField]
    private Color PreviousColor
    {
        get;
        set;
    }

    public AutoChangeColor(Color newColor)
    {
        PreviousColor = GUI.color;
        GUI.color = newColor;
    }

    public void Dispose()
    {
        GUI.color = PreviousColor;
    }
}
