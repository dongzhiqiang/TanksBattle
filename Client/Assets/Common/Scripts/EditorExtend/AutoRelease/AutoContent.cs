using UnityEngine;
using System;
using System.Collections;


public class AutoContent : IDisposable
{
    float old;
    public AutoContent()
    {

        GUILayout.BeginHorizontal();
	    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        GUILayout.BeginVertical();
		GUILayout.Space(2f);
#if UNITY_EDITOR
        old = UnityEditor.EditorGUIUtility.labelWidth;
        UnityEditor.EditorGUIUtility.labelWidth = old-10;
#endif

    }

    public void Dispose()
    {

        GUILayout.Space(3f);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();

		GUILayout.Space(3f);
#if UNITY_EDITOR
        UnityEditor.EditorGUIUtility.labelWidth = old;
#endif
    }
}
