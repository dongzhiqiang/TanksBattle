using UnityEngine;
using System;
using System.Collections;


public class AutoBeginScrollView : IDisposable
{
	public Vector2 Scroll
	{
		get;
		set;
	}
	
	public AutoBeginScrollView(Vector2 scrollPosition)
	{
		Scroll = GUILayout.BeginScrollView(scrollPosition);
	}
		
	public AutoBeginScrollView(Vector2 scrollPosition, params GUILayoutOption[] options)
	{
		Scroll = GUILayout.BeginScrollView(scrollPosition, options);
	}
		
	public AutoBeginScrollView(Vector2 scrollPosition, GUIStyle style)
	{
		Scroll = GUILayout.BeginScrollView(scrollPosition, style);
	}
		
	public AutoBeginScrollView(Vector2 scrollPosition, GUIStyle style, params GUILayoutOption[] options)
	{
		Scroll = GUILayout.BeginScrollView(scrollPosition, style, options);
	}
		
	public AutoBeginScrollView(Vector2 scrollPosition, GUIStyle horizontalScrollBar, GUIStyle verticalScrollBar)
	{
		Scroll = GUILayout.BeginScrollView(scrollPosition, horizontalScrollBar, verticalScrollBar);
	}

    public AutoBeginScrollView(Vector2 scrollPosition, GUIStyle horizontalScrollBar, GUIStyle verticalScrollBar, params GUILayoutOption[] options)
	{
		Scroll = GUILayout.BeginScrollView(scrollPosition, horizontalScrollBar, verticalScrollBar, options);
	}

    public AutoBeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, params GUILayoutOption[] options)
	{
		Scroll = GUILayout.BeginScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical, options);
	}

		
	public void Dispose() 
	{
		GUILayout.EndScrollView();
	}
}