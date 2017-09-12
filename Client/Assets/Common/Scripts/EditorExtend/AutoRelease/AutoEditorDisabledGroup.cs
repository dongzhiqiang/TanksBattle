using UnityEngine;
using System;
using System.Collections;


public class AutoEditorDisabledGroup : IDisposable
{
    public AutoEditorDisabledGroup(bool disable)
    {
#if UNITY_EDITOR
        UnityEditor.EditorGUI.BeginDisabledGroup(disable);
#endif
    }

    public void Dispose()
    {
#if UNITY_EDITOR
        UnityEditor.EditorGUI.EndDisabledGroup();
#endif
    }
}
