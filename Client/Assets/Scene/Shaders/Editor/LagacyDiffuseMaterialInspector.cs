
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


[CanEditMultipleObjects]
public class LagacyDiffuseMaterialInspector : MaterialEditor
{
    public override void OnInspectorGUI()
	{
        EditorGUILayout.HelpBox("请使用Mobile下的shader", MessageType.Error);
		base.OnInspectorGUI();
    }
    

}
