using UnityEngine;
using System;
using System.Collections;


public class AutoBeginArea : IDisposable
{
    public AutoBeginArea(Rect area)
    {
        GUILayout.BeginArea(area);
    }

    public AutoBeginArea(Rect area, string content)
    {
        GUILayout.BeginArea(area, content);
    }

    public AutoBeginArea(Rect area, string content, string style)
    {
        GUILayout.BeginArea(area, content, style);
    }

    public void Dispose()
    {
        GUILayout.EndArea();
    }
}
