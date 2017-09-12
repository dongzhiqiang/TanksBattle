using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.UI;

[CustomEditor(typeof(UIProgressEx), false)]
public class UIProgressExEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        UIProgressEx t = target as UIProgressEx;
        base.OnInspectorGUI();
        float v = EditorGUILayout.Slider("Progress", t.Value, 0f, 1f);
        if(t.Value !=v)
        {
            t.Value = v;
        }
         
    }

    

}

