using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.UI;

[CanEditMultipleObjects]
[CustomEditor(typeof(TextEx), false)]
public class TextExEditor : UnityEditor.UI.TextEditor
{
    

    public override void OnInspectorGUI()
    {
        TextEx textEx= target as TextEx;
        EditorGUI.BeginChangeCheck();
        textEx.m_minPreferredWidth = EditorGUILayout.FloatField("minPreferredWidth", textEx.m_minPreferredWidth);
        textEx.m_maxPreferredWidth = EditorGUILayout.FloatField("maxPreferredWidth", textEx.m_maxPreferredWidth);
        
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtil.SetDirty(textEx);
            LayoutRebuilder.MarkLayoutForRebuild(textEx.transform as RectTransform);
        }
        
        base.OnInspectorGUI();
        
    }


}

