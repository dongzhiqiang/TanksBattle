using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


//技能编辑器
public class SkillEditorWindow1 : MultiAdapterWindow
{
    [MenuItem("Tool/技能编辑器 %F1",false,100)]
    public static void ShowWindow()
    {
        SkillEditorWindow1 instance = (SkillEditorWindow1)EditorWindow.GetWindow(typeof(SkillEditorWindow1));//很遗憾，窗口关闭的时候instance就会为null
        instance.minSize = new Vector2(600.0f, 300.0f);
        instance.titleContent = new GUIContent("技能编辑器1");
        SkillEditor skillEditor =new SkillEditor();
        skillEditor.m_id=1;
        instance.MultiWindow = skillEditor;
        instance.autoRepaintOnSceneChange = true;
        


        if (RoleMgr.instance!= null&& RoleMgr.instance.Hero != null && RoleMgr.instance.Hero.State == Role.enState.alive)
        {
            skillEditor.SetRole(RoleMgr.instance.Hero);
        }

    }
}
