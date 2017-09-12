using UnityEngine;
using System;
using System.Collections;


public class AutoBeginHandles : IDisposable
{
    public AutoBeginHandles()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();
    #endif
    }

    public void Dispose()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.EndGUI();
#endif
    }
}
