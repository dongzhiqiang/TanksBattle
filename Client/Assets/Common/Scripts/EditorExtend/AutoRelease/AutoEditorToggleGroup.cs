using UnityEngine;
using System;
using System.Collections;


public class AutoEditorToggleGroup : IDisposable
{
    public bool Toggle
    {
        get;
        set;
    }
    public AutoEditorToggleGroup(bool toggle,string name = "")
    {
#if UNITY_EDITOR
        Toggle =UnityEditor.EditorGUILayout.BeginToggleGroup(name,toggle);
#endif
    }

    public void Dispose()
    {
#if UNITY_EDITOR
        UnityEditor.EditorGUILayout.EndToggleGroup();
#endif
    }
}
