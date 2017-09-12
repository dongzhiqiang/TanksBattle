using UnityEngine;
using System;
using System.Collections;


public class AutoBeginHorizontal : IDisposable
{
    public AutoBeginHorizontal()
    {
        GUILayout.BeginHorizontal();
    }

    public AutoBeginHorizontal(params GUILayoutOption[] layoutOptions)
    {
        GUILayout.BeginHorizontal(layoutOptions);
    }

    public AutoBeginHorizontal(GUIStyle guiStyle, params GUILayoutOption[] layoutOptions)
    {
        GUILayout.BeginHorizontal(guiStyle, layoutOptions);
    }

    public void Dispose()
    {
        GUILayout.EndHorizontal();
    }
}
