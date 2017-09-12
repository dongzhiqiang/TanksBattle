using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.UI;

[CustomEditor(typeof(UISmoothProgress), false)]
public class UISmoothProgressEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        UISmoothProgress t = target as UISmoothProgress;
        base.OnInspectorGUI();
        t.Progress=EditorGUILayout.Slider("Progress",t.Progress,0f,1f);
    }

    

}

