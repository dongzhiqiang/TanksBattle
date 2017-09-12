using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


//事件组编辑器
public class EventGroupEditorWindow1 : MultiAdapterWindow
{
    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        EventMgr.AddAll(MSG.MSG_FRAME, MSG_FRAME.EVENT_GROUP_EDITOR, OnOpen);
    }


    static void OnOpen(object param1, object param2, object param3)
    {
        ShowWindow();
    }

    [MenuItem("Tool/事件组编辑器 %F3", false, 104)]
    public static void ShowWindow()
    {
        EventGroupEditorWindow1 instance = (EventGroupEditorWindow1)EditorWindow.GetWindow(typeof(EventGroupEditorWindow1));//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(600.0f, 300.0f);
        instance.titleContent = new GUIContent("事件组编辑器1");
        instance.MultiWindow = new EventGroupEditor();
        instance.autoRepaintOnSceneChange = true;
    }
}


