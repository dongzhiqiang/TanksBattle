using UnityEngine;
using System;
using System.Collections;


public class AutoLabelWidth : IDisposable
{
    float old;
    
    public AutoLabelWidth(float w)
    {
#if UNITY_EDITOR
        old = UnityEditor.EditorGUIUtility.labelWidth;
        UnityEditor.EditorGUIUtility.labelWidth = w;
#endif
    }

    public void Dispose()
    {
#if UNITY_EDITOR
        UnityEditor.EditorGUIUtility.labelWidth = old;
#endif
    }
}
