using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


//事件组编辑器
public class EventGroupEditorWindow2 : MultiAdapterWindow
{
     [MenuItem("Tool/事件组编辑器2 %F4", false, 105)]
    public static void ShowWindow()
    {
        EventGroupEditorWindow2 instance = (EventGroupEditorWindow2)EditorWindow.GetWindow(typeof(EventGroupEditorWindow2));//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(600.0f, 300.0f);
        instance.titleContent = new GUIContent("事件组编辑器2");
        instance.MultiWindow = new EventGroupEditor();
        instance.autoRepaintOnSceneChange = true;
    }
}


