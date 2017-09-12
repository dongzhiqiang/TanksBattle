using UnityEngine;
using System;
using System.Collections;


public class AutoEditorTipButton : IDisposable
{
    string tip;
    bool end;

    public AutoEditorTipButton(string tip,bool end=true)
    {
        this.tip=tip;
        this.end = end;
        GUILayout.BeginHorizontal();
#if UNITY_EDITOR
        if (!end && GUILayout.Button("", EditorUtil.TipButton))
        {
            MessageBoxWindow.ShowWindow(true,tip);
        }
#endif

    }

    public void Dispose()
    {

#if UNITY_EDITOR
        if (end&&GUILayout.Button("", EditorUtil.TipButton))
        {
            MessageBoxWindow.ShowWindow(true,tip);
        }
#endif

        GUILayout.EndHorizontal();
    }
}
