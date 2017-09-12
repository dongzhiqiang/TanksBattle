using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

//[CustomEditor(typeof(CameraChange), false)]
public class CameraChangeEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        CameraChange change = target as CameraChange;
       
        EditorGUI.BeginChangeCheck();
        change.m_duration = EditorGUILayout.FloatField("结束时间", change.m_duration);
        CameraEditorWindow.DrawCameraInfo(change.m_info);
        if (EditorGUI.EndChangeCheck())
        {
            //Debuger.Log("修改");
            EditorUtil.SetDirty(change);
        }
    }
    
}

