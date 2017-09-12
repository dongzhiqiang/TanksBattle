using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


//抓取编辑器
public class GrabEditor : MultiWindow
{
    static GrabEditorWindow1 s_wnd1;
    static GrabEditorWindow2 s_wnd2;
    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        EventMgr.AddAll(MSG.MSG_FRAME, MSG_FRAME.GRAB_EDITOR, OnOpen);
    }


    static void OnOpen(object param1, object param2, object param3)
    {
        Show((Role)param1, (GrabCxt)param2);
    }

   
    public static void Show(Role source, GrabCxt grabCxt)
    {
        if(s_wnd1 == null || s_wnd1.MultiWindow==null)
        {
            s_wnd1 = (GrabEditorWindow1)EditorWindow.GetWindow(typeof(GrabEditorWindow1));//很遗憾，窗口关闭的时候instance就会为null
            s_wnd1.minSize = new Vector2(600.0f, 200.0f);
            s_wnd1.titleContent = new GUIContent("抓取编辑器1");
            GrabEditor grabEditor = new GrabEditor();
            grabEditor.m_id = 1;
            s_wnd1.MultiWindow = grabEditor;
            grabEditor.m_source = source;
            grabEditor.SetCfg(grabCxt);
            s_wnd1.autoRepaintOnSceneChange = true;
            return;
        }
        else if (((GrabEditor)s_wnd1.MultiWindow).m_cxt == grabCxt){
            s_wnd1.Focus();
            return;
        }
        else if (s_wnd2 == null || s_wnd2.MultiWindow == null)
        {
            s_wnd2 = (GrabEditorWindow2)EditorWindow.GetWindow(typeof(GrabEditorWindow2));//很遗憾，窗口关闭的时候instance就会为null
            s_wnd2.minSize = new Vector2(600.0f, 200.0f);
            s_wnd2.titleContent = new GUIContent("抓取编辑器2");
            GrabEditor grabEditor = new GrabEditor();
            grabEditor.m_id = 2;
            s_wnd2.MultiWindow = grabEditor;
            grabEditor.m_source = source;
            grabEditor.SetCfg(grabCxt);
            s_wnd2.autoRepaintOnSceneChange = true;
            return;
        }
        else if (((GrabEditor)s_wnd2.MultiWindow).m_cxt == grabCxt)
        {
            s_wnd2.Focus();
            return;
        }
            
       
        EditorUtility.DisplayDialog("", "不能同时打开超过两个抓取编辑器", "确定");
    }

    public int m_id;
    public GrabCxt m_cxt;
    public Role m_source;
    bool m_anisArea = true;
    bool m_posAndDirArea = true;
    bool m_infoArea =true;
    bool m_fxArea = true;
    EventGroupEditor m_eventGroupEditor;

    public void SetCfg(GrabCxt cxt)
    {
        m_cxt = cxt;
        if (m_cxt != null)
        {
            m_eventGroupEditor.SetGrabRole(m_source);
            m_eventGroupEditor.SetGroup(m_cxt.eventGroup);
        }
        else
        {
            m_eventGroupEditor.SetGrabRole(null);
            m_eventGroupEditor.SetGroup(null);
        }
    }

    public override void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        m_eventGroupEditor = new EventGroupEditor();
        m_eventGroupEditor.OnEnable();
        m_eventGroupEditor.OnFocus();
    }
    

    public override void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.RepaintAll();
        if (m_eventGroupEditor != null)
            m_eventGroupEditor.OnDisable(); 
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (m_source == null ||m_cxt==null||m_source.IsInPool)
            return;

        Transform t = m_source.transform;
        if(t == null)
            return;

        int i=0;
        foreach (GrabPosAndDirCxt c in m_cxt.poss)
        {
            ++i;
            if(!c.expand)
                continue;
            if (c.posType != enGrabPos.bone &&c.posType != enGrabPos.speed &&c.dirType == enGrabDir.bone)
                continue;

            Vector3 pos =Vector3.zero;
            Quaternion rot = Quaternion.identity;
            bool ret = c.GetPosAndRot(t, ref pos, ref rot);
            if (!ret)
                continue;

            Handles.Label(pos, string.Format("抓取第{0}阶段", i));
            if (c.posType == enGrabPos.speed)
            {
                Vector3 speedDir = Vector3.forward;
                ret = c.GetSpeedRot(t, ref speedDir);
                if (ret)
                {
                    Handles.ArrowCap(m_id, pos, Quaternion.LookRotation(speedDir), 2);
                }
            }
            else
            {
                
                Handles.PositionHandle(pos, rot);
            }
        }
        
    }

    //绘制窗口时调用
    public override void OnGUI()
    {
        if(m_cxt == null)
            return;

        using (new AutoBeginHorizontal())
        {
            //左边，技能所有的信息
            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(250)))
            {
                using (new AutoLabelWidth(80))
                {
                    //1 基本信息
                    EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_infoArea, "基本信息", "GrabEditor.m_infoArea" + m_id, EditorStyleEx.BoxStyle);
                    m_infoArea = area.open;
                    if (area.Show())
                    {
                        using (new AutoEditorTipButton("多少帧后被抓取者退出被抓状态，-1则动作什么时候播放完什么时候退出"))
                            m_cxt.frame = EditorGUILayout.IntField("持续帧数", m_cxt.frame);

                        using (new AutoEditorTipButton("结束时销毁被抓取角色"))
                            m_cxt.destroyWhenEnd = EditorGUILayout.Toggle("结束销毁", m_cxt.destroyWhenEnd);
                        using (new AutoEditorTipButton("结束时被抓取角色添加这个状态"))
                            m_cxt.endBuffId = EditorGUILayout.IntField("结束状态", m_cxt.endBuffId);
                        using (new AutoEditorTipButton("结束时播放一个事件组，属于抓取者"))
                            m_cxt.endEventGroupId = EditorGUILayout.TextField("结束事件组", m_cxt.endEventGroupId);
                    }
                    EditorGUILayoutEx.instance.EndFadeArea();

                    //2动作序列
                    DrawAnis();


                    //3 位置和方向控制
                    DrawPosAndDirs();

                    //4 特效
                    DrawFxs();
                }
            }

            //右边，技能的事件表
            using (new AutoBeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(adapterWindow.position.height)))
            {
                if (m_eventGroupEditor != null)
                    m_eventGroupEditor.Draw();
            }

        }
        
        
    }

    bool DrawAni(int id, SimpleAniCxt c)
    {
        Color tmp1 = GUI.color;
        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(c.expand, "", "GrabEditor" + c.aniName + (id * 10 + m_id), EditorStyleEx.BoxStyle);
        Color tmp2 = GUI.color;//BeginFadeArea 需要
        GUI.color = tmp1;//BeginFadeArea 需要
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(c.aniName , EditorGUILayoutEx.defaultLabelStyle))
                c.expand = !c.expand;
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                return false;
            }
        }

        if (area.Show())
        {
            GUI.color = tmp2;//BeginFadeArea 需要
            c.aniName = EditorGUILayout.TextField("动作", c.aniName);

            string s = SimpleAniCxt.s_aniTypes.Get(c.wrapMode);
            int idx = string.IsNullOrEmpty(s) ? -1 : System.Array.IndexOf(SimpleAniCxt.AniTypeName, s);
            int idxNew = EditorGUILayout.Popup("循环类型", idx, SimpleAniCxt.AniTypeName);
            if (idx != idxNew && idxNew != -1)
            {
                c.wrapMode = SimpleAniCxt.s_aniTypeNames[SimpleAniCxt.AniTypeName[idxNew]];
            }

            c.duration = EditorGUILayout.FloatField("持续时间", c.duration);
            c.fade = EditorGUILayout.FloatField("动作渐变", c.fade);

        }
        EditorGUILayoutEx.instance.EndFadeArea();
        return true;
    }

    void DrawAnis()
    {
        Color tmp1 = GUI.color;
        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_anisArea, "", "GrabEditor.m_anisArea" + m_id, EditorStyleEx.BoxStyle);
        Color tmp2 = GUI.color;//BeginFadeArea 需要
        GUI.color = tmp1;//BeginFadeArea 需要
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("动作序列", EditorGUILayoutEx.defaultLabelStyle))
                m_anisArea = !m_anisArea;
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                m_cxt.anis.anis.Add(new SimpleAniCxt());
            }
        }

        if (area.Show())
        {
            GUI.color = tmp2;//BeginFadeArea 需要
            SimpleAnimationsCxt anisCxt = m_cxt.anis;
            SimpleAniCxt removeAni = null;

            int i = 0;
            foreach (SimpleAniCxt c in anisCxt.anis)
            {
                ++i;
                if (!DrawAni(i,c))
                {
                    removeAni = c;
                }
            }
            if (removeAni != null)
                anisCxt.anis.Remove(removeAni);
        }
        EditorGUILayoutEx.instance.EndFadeArea();
    }

    bool DrawPosAniDir(int id,GrabPosAndDirCxt c)
    {   
        Color tmp1 = GUI.color;
        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(c.expand, "", "GrabEditor.posAndDir" + (id*10 + m_id), EditorStyleEx.BoxStyle);
        Color tmp2 = GUI.color;//BeginFadeArea 需要
        GUI.color = tmp1;//BeginFadeArea 需要
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(string.Format("第{0}阶段", id), EditorGUILayoutEx.defaultLabelStyle))
                c.expand = !c.expand;

            if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                return false;
            }
        }

        if (area.Show())
        {
            GUI.color = tmp2;//BeginFadeArea 需要
            using (new AutoEditorTipButton("多少帧后进入下一个位置计算阶段，-1则表明直到抓取结束"))
                c.frame = EditorGUILayout.IntField("持续帧数", c.frame);

            using (new AutoEditorTipButton("如果碰到墙或者地板，自动进入浮空状态或者倒地"))
                c.autoIfObstacle = EditorGUILayout.Toggle("碰撞结束", c.autoIfObstacle);

            using (new AutoEditorTipButton("渐变帧数,0则马上跳到计算的点和方向，否则在规定时间内渐变到计算的点和方向"))
                c.smoothFrame = EditorGUILayout.IntField("渐变帧数", c.smoothFrame);

            
            

            using (new AutoEditorTipButton(
@"位置类型:
不改变位置，不改变被抓取者的位置
骨骼，位置的相对于抓取者的某个骨骼，如果为空则为抓取者的根节点，被抓取过程一直跟随
速度，初始速度相对于抓取者的某个骨骼，之后的位置随着速度变化
"
))
            c.posType = (enGrabPos)EditorGUILayout.Popup("位置类型", (int)c.posType, GrabPosAndDirCxt.PosTypeName);
            c.dirType = (enGrabDir)EditorGUILayout.Popup("方向类型", (int)c.dirType, GrabPosAndDirCxt.DirTypeName);

            if (c.posType == enGrabPos.bone || c.posType == enGrabPos.speed || c.dirType == enGrabDir.bone)
            {
                using (new AutoEditorTipButton("要绑定的骨骼，方向或者位置计算过程中可能用到如果为空则为抓取者的根节点"))
                    c.bone = EditorGUILayout.TextField("绑定骨骼", c.bone);

                EditorGUI.BeginChangeCheck();
                c.posOffset = EditorGUILayout.Vector3Field("位置偏移", c.posOffset);
                c.dirOffset = EditorGUILayout.Vector3Field("方向偏移", c.dirOffset);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }

            if (c.posType == enGrabPos.speed)
            {
                c.speedDir = EditorGUILayout.Vector3Field("速度的方向", c.speedDir);
                EditorGUI.BeginChangeCheck();
                c.speed = EditorGUILayout.FloatField("速度的大小", c.speed);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }

            
                


        }
        EditorGUILayoutEx.instance.EndFadeArea();
        return true;
    }
    void DrawPosAndDirs()
    {
        Color tmp1 = GUI.color;
        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_posAndDirArea, "", "GrabEditor.m_posAndDirArea" + m_id, EditorStyleEx.BoxStyle);
        Color tmp2 = GUI.color;//BeginFadeArea 需要
        GUI.color = tmp1;//BeginFadeArea 需要
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("位置和方向控制", EditorGUILayoutEx.defaultLabelStyle))
                m_posAndDirArea = !m_posAndDirArea;
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                m_cxt.poss.Add(new GrabPosAndDirCxt());
            }
        }

        if (area.Show())
        {
            GUI.color = tmp2;//BeginFadeArea 需要
            List<GrabPosAndDirCxt> cxts = m_cxt.poss;
            GrabPosAndDirCxt remove = null;

            int i=0;
            foreach (GrabPosAndDirCxt c in cxts)
            {
                ++i;
                if (!DrawPosAniDir(i,c))
                {
                    
                    remove =c;
                }
            }
            if (remove != null)
                cxts.Remove(remove);
        }
        EditorGUILayoutEx.instance.EndFadeArea();
        
    }

    bool DrawFx(int id, GrabFxCxt c)
    {
        Color tmp1 = GUI.color;
        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(c.expand, "", "GrabEditor.fx" + (id * 10 + m_id), EditorStyleEx.BoxStyle);
        Color tmp2 = GUI.color;//BeginFadeArea 需要
        GUI.color = tmp1;//BeginFadeArea 需要
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button(string.Format("特效{0}", id), EditorGUILayoutEx.defaultLabelStyle))
                c.expand = !c.expand;

            if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                return false;
            }
        }

        if (area.Show())
        {
            GUI.color = tmp2;//BeginFadeArea 需要
            using (new AutoEditorTipButton("第几帧创建"))
                c.frame = EditorGUILayout.IntField("开始帧", c.frame);

            int idx = System.Array.IndexOf(RoleFxCfg.FxNames, c.roleFxName);
            int newIdx = EditorGUILayout.Popup("角色特效名", (int)idx, RoleFxCfg.FxNames);
            if (newIdx != idx)
            {
                c.roleFxName = RoleFxCfg.FxNames[newIdx];
            }

           
        }
        EditorGUILayoutEx.instance.EndFadeArea();
        return true;
    }

    void DrawFxs()
    {
        Color tmp1 = GUI.color;
        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_fxArea, "", "GrabEditor.m_fxArea" + m_id, EditorStyleEx.BoxStyle);
        Color tmp2 = GUI.color;//BeginFadeArea 需要
        GUI.color = tmp1;//BeginFadeArea 需要
        using (new AutoBeginHorizontal())
        {
            if (GUILayout.Button("特效列表", EditorGUILayoutEx.defaultLabelStyle))
                m_fxArea = !m_fxArea;
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                m_cxt.fxs.Add(new GrabFxCxt());
            }
        }

        if (area.Show())
        {
            GUI.color = tmp2;//BeginFadeArea 需要

            GrabFxCxt remove = null;
            int i = 0;
            foreach (GrabFxCxt c in m_cxt.fxs)
            {
                ++i;
                if (!DrawFx(i, c))
                {
                    remove = c;
                }
            }
            if (remove != null)
                m_cxt.fxs.Remove(remove);
        }
        EditorGUILayoutEx.instance.EndFadeArea();
    }
}
