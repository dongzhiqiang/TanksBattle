using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.UI;


[CustomEditor(typeof(UIArtFont), false)]
public class UIArtFontEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        UIArtFont cur = this.target as UIArtFont;

        EditorGUI.BeginChangeCheck();
        cur.m_prefix = EditorGUILayout.TextField("前缀", cur.m_prefix);
        cur.m_num = EditorGUILayout.TextField("数字", cur.m_num);
        cur.m_align = (UIArtFont.enAlign)EditorGUILayout.EnumPopup("对齐",cur.m_align);
        cur.m_space = EditorGUILayout.FloatField("间隔", cur.m_space);
        if (EditorGUI.EndChangeCheck())
            cur.SetNum(cur.m_num,true);
            

    }

    
}

