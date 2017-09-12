using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;


public class FxCreateWindow : EditorWindow
{
    public FxCreateCfg m_cfg;
    public Transform m_source;

    [InitializeOnLoadMethod]
    static void OnLoad()
    {        
        EventMgr.AddAll(MSG.MSG_FRAME, MSG_FRAME.FX_EDITOR,OnOpen);
    }


    static void OnOpen(object param1,object param2, object param3)
    {
        string title = (string)param1;
        FxCreateCfg cfg = (FxCreateCfg)param2;
        Transform source = (Transform)param3;
        ShowWindow(title, cfg, source);
    }
    

    public static void ShowWindow(string title,FxCreateCfg cfg,Transform source)
    {
        FxCreateWindow instance = (FxCreateWindow)EditorWindow.GetWindow(typeof(FxCreateWindow), true);//很遗憾，窗口关闭的时候instance就会为null
        
        instance.minSize = new Vector2(200f, 100.0f);
        instance.titleContent = new GUIContent(title);
        instance.m_cfg = cfg;
        instance.m_source = source;
    }

    
    #region 监听
    public void Awake()
    {
        

    }

    //更新
    void Update()
    {
        
    }

    //显示和focus时初始化下
    void OnFocus()
    {
        
    }

    void OnHierarchyChange()
    {
        //Debuger.Log("当Hierarchy视图中的任何对象发生改变时调用一次");

    }

    void OnProjectChange()
    {
        //Debuger.Log("当Project视图中的资源发生改变时调用一次");
    }

    void OnInspectorUpdate()
    {
        //Debuger.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
    }

    void OnSelectionChange()
    {
        //当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
        foreach (Transform t in Selection.transforms)
        {
            //有可能是多选，这里开启一个循环打印选中游戏对象的名称
           // Debuger.Log("OnSelectionChange" + t.name);
        }
    }

    void OnDestroy()
    {
        //Debuger.Log("当窗口关闭时调用");
    }

    #endregion
    void OnLostFocus()
    {
        //Close();
        //Debuger.Log("当窗口丢失焦点时调用一次");
    }
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.RepaintAll();
    }

    //绘制窗口时调用
    void OnGUI()
    {
        if (m_cfg == null)return;
        using (new AutoBeginHorizontal())
        {
            m_cfg.name = EditorGUILayout.TextField("预制体名",m_cfg.name);

            if (GUILayout.Button("特效浏览器", GUILayout.Width(80)))
            {
                FxBrowserWindow.ShowWindow((string fxName)=>m_cfg.name = fxName);
            }
        }
        using (new AutoBeginHorizontal())
        {
            m_cfg.fireFx = EditorGUILayout.TextField("预制体名(火)", m_cfg.fireFx);

            if (GUILayout.Button("特效浏览器", GUILayout.Width(80)))
            {
                FxBrowserWindow.ShowWindow((string fxName) => m_cfg.fireFx = fxName);
            }
        }
        using (new AutoBeginHorizontal())
        {
            m_cfg.iceFx = EditorGUILayout.TextField("预制体名(冰)", m_cfg.iceFx);

            if (GUILayout.Button("特效浏览器", GUILayout.Width(80)))
            {
                FxBrowserWindow.ShowWindow((string fxName) => m_cfg.iceFx = fxName);
            }
        }
        using (new AutoBeginHorizontal())
        {
            m_cfg.thunderFx = EditorGUILayout.TextField("预制体名(雷)", m_cfg.thunderFx);

            if (GUILayout.Button("特效浏览器", GUILayout.Width(80)))
            {
                FxBrowserWindow.ShowWindow((string fxName) => m_cfg.thunderFx = fxName);
            }
        }
        using (new AutoBeginHorizontal())
        {
            m_cfg.darkFx = EditorGUILayout.TextField("预制体名(冥)", m_cfg.darkFx);

            if (GUILayout.Button("特效浏览器", GUILayout.Width(80)))
            {
                FxBrowserWindow.ShowWindow((string fxName) => m_cfg.darkFx = fxName);
            }
        }
        using (new AutoEditorTipButton("多少帧后销毁，填-1的话就不销毁"))
            m_cfg.durationFrame = EditorGUILayout.IntField("持续帧数", m_cfg.durationFrame);
        m_cfg.dirType = (enFxCreateDir)EditorGUILayout.Popup("出生方向",(int)m_cfg.dirType,FxCreateCfg.DirTypeNames);
        m_cfg.posType = (enFxCreatePos)EditorGUILayout.Popup("出生点", (int)m_cfg.posType, FxCreateCfg.PosTypeName);
        if (m_cfg.posType == enFxCreatePos.bone)
        {
            using (new AutoEditorTipButton("如果为空那么就是根节点(主角脚底)。支持嵌套，比如：\"model/body_mesh\"。"))
                m_cfg.bone = EditorGUILayout.TextField("骨骼名",m_cfg.bone);
            m_cfg.follow = EditorGUILayout.Toggle("跟随",m_cfg.follow);
        }


        if (m_cfg.posType != enFxCreatePos.matrial)
        {
            using (new AutoEditorTipButton("高度和主角对齐，一般用来使特效创建在贴地表的地方"))
                m_cfg.alignSourceY = EditorGUILayout.Toggle("高度对齐主角", m_cfg.alignSourceY);
            using (new AutoEditorTipButton("这里只支持水平角度偏移，偏移的计算流程是先计算角度偏移，设置出身方向，再计算出生点，再相对于方向对出生点进行位置"))
                m_cfg.dirOffset = EditorGUILayout.FloatField("角度偏移", m_cfg.dirOffset);
            using (new AutoEditorTipButton("偏移的计算流程是先计算角度偏移，设置出身方向，再计算出生点，再相对于方向对出生点进行位置"))
                m_cfg.posOffset = EditorGUILayout.Vector3Field("位置偏移", m_cfg.posOffset);
            m_cfg.randomBegin = EditorGUILayout.FloatField("随机范围(左区间)", m_cfg.randomBegin);
            m_cfg.randomEnd = EditorGUILayout.FloatField("随机范围(右区间)", m_cfg.randomEnd);
            m_cfg.num = EditorGUILayout.IntField("数量", m_cfg.num);
            if (m_cfg.num > 1)
            {
                using (new AutoEditorTipButton("相对于上一个特效的偏移角度"))
                    m_cfg.multiDirOffset = EditorGUILayout.FloatField("多个时的方向偏移", m_cfg.multiDirOffset);

                using (new AutoEditorTipButton("相对于上一个特效的偏移位置"))
                    m_cfg.multiPosOffset = EditorGUILayout.Vector3Field("多个时的位置偏移", m_cfg.multiPosOffset);
            }
        }
        

    }

    

    void OnSceneGUI(SceneView sceneView)
    {
        if (m_source == null || 
        (m_cfg.posType != enFxCreatePos.source && m_cfg.posType != enFxCreatePos.target&& m_cfg.posType != enFxCreatePos.bone)
        || m_cfg.dirType == enFxCreateDir.look || m_cfg.dirType == enFxCreateDir.back)
            return;

        //计算出出生点和出生方向并显示
        Vector3 pos;
        Vector3 euler;
        if(FxCreateCfg.BindBoneAndGetPosAnDir(null, m_source, m_source, m_source.position, 
            m_cfg.dirType, m_cfg.posType, m_cfg.dirOffset, m_cfg.posOffset,
             m_cfg.bone, m_cfg.follow, m_cfg.alignSourceY, out pos, out euler,null))
        {
            Handles.Label(pos, "特效出生点");
            Handles.PositionHandle(pos, Quaternion.Euler(euler));
        }
    }
}
