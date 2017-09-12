using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


//飞出物编辑器
public class FlyerEditor : MultiWindow
{
    static FlyerEditorWindow1 s_wnd1;
    static FlyerEditorWindow2 s_wnd2;
    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        EventMgr.AddAll(MSG.MSG_FRAME, MSG_FRAME.FLYER_EDITOR, OnOpen);
    }


    static void OnOpen(object param1, object param2, object param3)
    {
        Show((FlyerCfg)param1, (Action<string>)param2);
    }


    public static void Show(FlyerCfg cfg, Action<string> onSel)
    {
        if(s_wnd1 == null || s_wnd1.MultiWindow==null)
        {
            s_wnd1 = (FlyerEditorWindow1)EditorWindow.GetWindow(typeof(FlyerEditorWindow1));//很遗憾，窗口关闭的时候instance就会为null
            s_wnd1.minSize = new Vector2(600.0f, 200.0f);
            s_wnd1.titleContent = new GUIContent("飞出物编辑器1");
            FlyerEditor flyerEditor = new FlyerEditor();
            s_wnd1.MultiWindow = flyerEditor;
            flyerEditor.SetCfg(cfg);
            flyerEditor.m_onSel = onSel;
            flyerEditor.m_id=1;
            s_wnd1.autoRepaintOnSceneChange = true;
            return;
        }
        else if (((FlyerEditor)s_wnd1.MultiWindow).m_cfg == cfg)
        {
            s_wnd1.Focus();
            return;
        }
        else if (s_wnd2 == null || s_wnd2.MultiWindow == null)
        {
            s_wnd2 = (FlyerEditorWindow2)EditorWindow.GetWindow(typeof(FlyerEditorWindow2));//很遗憾，窗口关闭的时候instance就会为null
            s_wnd2.minSize = new Vector2(600.0f, 200.0f);
            s_wnd2.titleContent = new GUIContent("飞出物编辑器2");
            FlyerEditor flyerEditor = new FlyerEditor();
            s_wnd2.MultiWindow = flyerEditor;
            flyerEditor.SetCfg(cfg);
            flyerEditor.m_onSel = onSel;
            flyerEditor.m_id = 2;
            s_wnd2.autoRepaintOnSceneChange = true;
            return;
        }
        else if (((FlyerEditor)s_wnd2.MultiWindow).m_cfg == cfg)
        {
            s_wnd2.Focus();
            return;
        }
            

        EditorUtility.DisplayDialog("","不能同时打开超过两个飞出物编辑器","确定");
    }

    public int m_id;
    public FlyerCfg m_cfg;
    public Action<string> m_onSel;
    bool m_infoArea = true;
    bool m_pathArea = true;
    EventGroupEditor m_eventGroupEditor;
    EventObserver m_observer;

    public void SetCfg(FlyerCfg cfg)
    {
        m_cfg = cfg;
        if (m_cfg != null)
        {
            m_eventGroupEditor.SetFlyerId(cfg.file);
            m_eventGroupEditor.SetGroup(m_cfg.eventGroup);
        }
        else
        {
            m_eventGroupEditor.SetFlyerId(null);
            m_eventGroupEditor.SetGroup(null);
        }
    }

    public override void OnEnable()
    {

        m_eventGroupEditor = new EventGroupEditor();
        m_eventGroupEditor.OnEnable();
        m_eventGroupEditor.OnFocus();
        
    }
    

    public override void OnDisable()
    {
        if (m_eventGroupEditor != null)
            m_eventGroupEditor.OnDisable();    
    }
    
    //绘制窗口时调用
    public override void OnGUI()
    {
        //工具栏
        using (new AutoBeginHorizontal(EditorStyles.toolbarButton))
        {
            DrawToolBar();
        }

        if (m_cfg == null)
            return;

        using (new AutoBeginHorizontal())
        {
            //左边，技能所有的信息
            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(250)))
            {
                using (new AutoLabelWidth(80))
                {
                    EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_infoArea, "基本信息", "flyerInfoArea" + m_id, EditorStyleEx.BoxStyle);
                    m_infoArea = area.open;
                    if (area.Show())
                        DrawInfo();
                    EditorGUILayoutEx.instance.EndFadeArea();

                    area = EditorGUILayoutEx.instance.BeginFadeArea(m_pathArea, "弹道", "flyerPathArea" + m_id, EditorStyleEx.BoxStyle);
                    m_pathArea = area.open;
                    if (area.Show())
                        DrawPath();
                    EditorGUILayoutEx.instance.EndFadeArea();
                }

            }

            //右边，技能的事件表
            using (new AutoBeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(adapterWindow.position.height - 20)))
            {
                if (m_eventGroupEditor != null)
                    m_eventGroupEditor.Draw();
            }

        }
    }

    void DrawToolBar()
    {
        //选择事件组
        int idx = m_cfg == null ? -1 : Array.IndexOf(FlyerCfg.FlyerIds, m_cfg.file);
        int newIdx = EditorGUILayout.Popup(idx, FlyerCfg.FlyerIds, EditorStyles.toolbarPopup, GUILayout.Width(150));
        int len = FlyerCfg.FlyerIds.Length;
        if (newIdx != -1 && idx != newIdx && newIdx != len - 2)
        {
            if (m_cfg != null && EditorUtility.DisplayDialog("", "是否保存" + m_cfg.file + "的修改?", "保存", "否"))
                m_cfg.Save();
            else
            {
                FlyerCfg.RemoveCache(m_cfg);
                SetCfg(null);
            }

            if (newIdx == len - 1)
            {
                MessageBoxWindow.ShowAsInputBox("flyer_xxxx", (string name, object context) =>
                {
                    if (string.IsNullOrEmpty(name) || name == "flyer_xxxx")
                        return;

                    SetCfg(FlyerCfg.Add(name));

                }, (string name, object context) => { });
            }
            else
                SetCfg(FlyerCfg.Get(FlyerCfg.FlyerIds[newIdx]));
        }


        if (m_cfg == null)
            return;

        //保存
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Duplicate"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            m_cfg.Save();
            adapterWindow.ShowNotification(new GUIContent("保存成功"));

        }

        //重载
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Refresh"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            if (EditorUtility.DisplayDialog("", "是否确定要重载，所有未保存的修改都将还原?", "是", "否"))
            {
                string s = m_cfg.file;
                FlyerCfg.RemoveCache(m_cfg);
                SetCfg(FlyerCfg.Get(s));
            }
        }
        //删除
        if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(30)))
        {
            if (EditorUtility.DisplayDialog("", "是否确定要删除?", "是", "否"))
            {
                FlyerCfg.Remove(m_cfg);
                SetCfg(null);
            }
        }
    }

    void DrawInfo()
    {
        using (new AutoBeginHorizontal())
        {
            EditorGUILayout.TextField("飞出物名", m_cfg.file);
            if (m_onSel != null && GUILayout.Button("选中", GUILayout.Width(40)))
            {
                m_onSel(m_cfg.file);
                adapterWindow.Close();
            }
                
        }

        using (new AutoEditorTipButton("多少帧后销毁，填-1的话就不销毁"))
            m_cfg.durationFrame = EditorGUILayout.IntField("销毁帧数", m_cfg.durationFrame);

        using (new AutoEditorTipButton("一旦接触敌人多少帧后销毁，注意只对身上有碰撞的飞出物有用"))
            m_cfg.touchDestroyFrame = EditorGUILayout.IntField("接触敌人销毁帧数", m_cfg.touchDestroyFrame);

        using (new AutoEditorTipButton("一旦接触技能目标多少帧后销毁，注意只对身上有碰撞的飞出物有用"))
            m_cfg.touchTargetDestroyFrame = EditorGUILayout.IntField("接触目标销毁帧数", m_cfg.touchTargetDestroyFrame);

        using (new AutoEditorTipButton("结束事件组"))
            m_cfg.endEventGroupId = EditorGUILayout.TextField("结束事件组", m_cfg.endEventGroupId);
        using (new AutoBeginHorizontal())
        {
            m_cfg.endFlyerCreateCfg.name =EditorGUILayout.TextField("结束特效", m_cfg.endFlyerCreateCfg.name);
            if (GUILayout.Button( "编辑",GUILayout.Width(40)))
            {
                FxCreateWindow.ShowWindow(string.Format("{0}的结束特效",m_cfg.file),m_cfg.endFlyerCreateCfg,null);
            }
        }   
        using (new AutoBeginHorizontal())
        {
            m_cfg.endFlyer =EditorGUILayout.TextField("结束飞出物id", m_cfg.endFlyer);
            if (GUILayout.Button( "编辑",GUILayout.Width(40)))
            {
                FlyerCfg flyerCfg = string.IsNullOrEmpty(m_cfg.endFlyer) ? null : FlyerCfg.Get(m_cfg.endFlyer);
                Show(flyerCfg, (string flyerId) => m_cfg.endFlyer = flyerId);
            }
        }
        using (new AutoEditorTipButton("在创建飞出物的时候外部会传进来目标，这里可以修改这个目标。如果选择目标类型，则不修改。如果选择目标(没有的话自动找最近的敌人)类型则如果目标为空或者不为敌人会找最近的敌人"))
            m_cfg.targetType = (enFlyerTargetType)EditorGUILayout.Popup("目标", (int)m_cfg.targetType, FlyerPathCfg.FlyerTargetTypeName);
    }

    void DrawPath()
    {
        int newIdx = EditorGUILayout.Popup((int)m_cfg.pathCfg.Type, FlyerPathFactory.TypeName);
        if (newIdx != (int)m_cfg.pathCfg.Type)
        {
            m_cfg.pathCfg = FlyerPathFactory.CreateCfg((enFlyerPathType)newIdx);
        }
        m_cfg.pathCfg.Draw();
    }

    
}
