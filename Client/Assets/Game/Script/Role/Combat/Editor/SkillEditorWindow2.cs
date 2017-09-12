using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


//技能编辑器
public class SkillEditorWindow2 : MultiAdapterWindow
{
    [MenuItem("Tool/技能编辑器2 %F2", false, 101)]
    public static void ShowWindow()
    {
        SkillEditorWindow2 instance = (SkillEditorWindow2)EditorWindow.GetWindow(typeof(SkillEditorWindow2));//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(600.0f, 300.0f);
        instance.titleContent = new GUIContent("技能编辑器2");
        SkillEditor skillEditor = new SkillEditor();
        skillEditor.m_id = 2;
        instance.MultiWindow = skillEditor;
        instance.autoRepaintOnSceneChange = true;
    }
}
