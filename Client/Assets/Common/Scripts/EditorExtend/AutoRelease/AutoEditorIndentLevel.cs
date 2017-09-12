using UnityEngine;
using System;
using System.Collections;


public class AutoEditorIndentLevel : IDisposable
{
    int add=1;
    public AutoEditorIndentLevel(int add=1)
    {
    #if UNITY_EDITOR
        this.add =add;
        UnityEditor.EditorGUI.indentLevel += add;
#endif

    }

    public void Dispose()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorGUI.indentLevel -= add;
#endif
    }
}
