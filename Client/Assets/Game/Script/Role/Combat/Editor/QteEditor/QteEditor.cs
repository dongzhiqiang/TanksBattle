using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TimeTagData
{
    public Rect TagRect;
    public Rect TagLabelRect;
    public string TagLabel;
}

public class QteEditor : EditorWindow
{
    #region Fields
    public static float Bar_H = 17.0f;
    public static float TimeLine_H = 25.0f;
    public static float TopBtn_W = 70.0f;
    public static float BottomLab_W = 70.0f;
    public static float TopBtn_W2 = 40.0f;
    public static float LeftArea_W = 200.0f;
    public static float LeftArea_H1 = 90.0f;
    public static float WinStage_H = 220.0f;
    public static float LoseStage_W = 500.0f;

    public static Rect TopBar_Rect;
    public static Rect Bottom_Rect;
    public static Rect LeftArea_Rect;
    public static Rect LeftAreaDebug_Rect;
    public static Rect RightTopArea_Rect;
    public static Rect RightWinArea_Rect;
    public static Rect RightLoseArea_Rect;

    Vector2 m_scrollLoseStage;

    public List<QteStageEditor> m_stageEditors = new List<QteStageEditor>();
    public List<TimeTagData> m_timeTagData = new List<TimeTagData>();   //保存刻度信息
    public List<QteStageInfo> m_removeStages = new List<QteStageInfo>();
    public BigQteCfg CurQteCfg
    {
        get
        {
            if (CurQte != null)
                return CurQte.Cfg;
            else
                return null;
        }
    }

    public BigQte CurQte { get { return BigQte.CurQte; } set { BigQte.CurQte = value; } }

    float PixelTimeline_W;  //时间线宽度

    bool m_bHandleDrag;     //标记是否拖拽
    float m_eachTagDist;    //每个刻度距离
    float m_eachTagValue;   //每个刻度的值
    bool m_bHandleDragStage;//标记是否拖动阶段柄
    float m_prevTime;       //记录的是非运行的时间
    bool m_bOpenObj;        //记录是否打开调试对象的面板
    bool m_bOpenPos;        //记录是否打开位置调试面板
    public static QteEditor m_instance;
    #endregion

    #region Properties
    public static QteEditor instance
    {
        get
        {
            if (m_instance == null)
                ShowWindow();

            return m_instance;
        }
    }

    public float MaxPosX
    {
        get
        {
            if (CurQteCfg == null)
                return 0;
            else
                return TimeToContentX(CurQteCfg.Duration);
        }
    }
    public float CurRunningTime
    {
        get { return CurQte.CurRunning; }
        set { CurQte.CurRunning = value; }
    }

    #endregion

    [MenuItem("Tool/大QTE编辑器 %F8", false, 110)]
    public static void ShowWindow()
    {
        m_instance = (QteEditor)EditorWindow.GetWindow(typeof(QteEditor));
        m_instance.minSize = new Vector2(920, 600);
        m_instance.titleContent = new GUIContent("大QTE编辑器");
        m_instance.autoRepaintOnSceneChange = true;

    }

    void OnGUI()
    {
        DrawTopBar();
        DrawContent();
        DrawBottomBar();
        DrawLeftArea();

        ProcessHotkeys();


        UpdateQte();

        if (m_removeStages.Count > 0)
        {
            foreach (QteStageInfo info in m_removeStages)
            {
                CurQteCfg.stages.Remove(info);
            }

            m_removeStages.Clear();
            UpdateStage();
            Repaint();
        }

    }

    #region 绘制界面
    //绘制顶部
    void DrawTopBar()
    {
        GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(Bar_H));

        if (Event.current.type == EventType.Repaint)
            TopBar_Rect = GUILayoutUtility.GetLastRect();

        using (new AutoBeginArea(TopBar_Rect))
        {
            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button("新建", EditorStyles.toolbarButton, GUILayout.Width(TopBtn_W)))
                {
                    CurQte = BigQte.Get("大qte");
                    UpdateStage();
                }

                string qte = CurQteCfg == null ? "选择" : CurQteCfg.Name;
                if (GUILayout.Button(qte, EditorStyles.toolbarButton, GUILayout.Width(TopBtn_W)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("新建Qte"), false, () =>
                    {
                        BigQte.Get("新qte");
                        UpdateStage();
                    });

                    BigQteTableCfg.Init();

                    for (int i = 0; i < BigQteTableCfg.mCfgList.Count; i++)
                    {
                        string name = BigQteTableCfg.mCfgList[i].Name;
                        int idx = i;
                        menu.AddItem(new GUIContent(name), false, () =>
                        {
                            BigQte.Get(BigQteTableCfg.mCfgList[idx]);
                            UpdateStage();
                        });
                    }
                    menu.ShowAsContext();
                    Repaint();
                }

                if (CurQteCfg != null)
                {
                    CurQteCfg.Name = GUILayout.TextField(CurQteCfg.Name, GUILayout.Width(80));
                    GUILayout.Box("", EditorStyles.toolbarButton, GUILayout.Width(10));

                    if (GUILayout.Button(CurQte.IsPlaying ? EditorGUIUtility.IconContent("PauseButton") : EditorGUIUtility.IconContent("PlayButton"), EditorStyles.toolbarButton, GUILayout.Width(TopBtn_W2)))
                    {
                        m_prevTime = Time.realtimeSinceStartup;
                        CurQte.PlayOrPause();

                    }

                    using (new AutoChangeColor(CurRunningTime != 0 ? Color.red : GUI.color))
                    {
                        if (GUILayout.Button(EditorGUIUtility.IconContent("PreMatQuad"), EditorStyles.toolbarButton, GUILayout.Width(TopBtn_W2)))
                            CurQte.Stop();
                    }

                    GUILayout.Box("", EditorStyles.toolbarButton, GUILayout.Width(6));

                    if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.PrevKey"), EditorStyles.toolbarButton, GUILayout.Width(TopBtn_W2)))
                        CurQte.PrevKeyframe();

                    if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.NextKey"), EditorStyles.toolbarButton, GUILayout.Width(TopBtn_W2)))
                        CurQte.NextKeyframe();

                    if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(TopBtn_W2)))
                    {

                        AddStage();
                    }

                    GUILayout.Space(50);

                    if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(TopBtn_W)))
                    {
                        if (string.IsNullOrEmpty(CurQteCfg.Name))
                        {
                            EditorUtility.DisplayDialog("保存配置", "文件名不能为空", "确定");
                            return;
                        }

                        CurQteCfg.SaveCfg(CurQteCfg.Name);
                    }

                }
            }
        }
    }

    //绘制内容
    void DrawContent()
    {
        if (CurQteCfg != null)
            DrawCurQte();
        else
            GUILayout.Box("当前无Qte可编辑", EditorStyleEx.BoxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }

    //绘制左边
    void DrawLeftArea()
    {
        if (CurQte == null)
            return;

        using (new AutoBeginArea(LeftArea_Rect))
        {
            using (new AutoBeginVertical())
            {
                DrawModels();
                if (Application.isPlaying && CurQte != null)
                {
                    CombatPart combatPart = CurQte.Hero.CombatPart;
                    if (GUILayout.Button("使用Qte"))
                    {
                        combatPart.Play("qte", null, false, true);
                    }
                }

                GUILayout.Space(10);
            }
        }

    }

    void DrawModels()
    {
        GUILayout.Space(15);

        using (new AutoLabelWidth(50))
        {
            EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(true, "非运行调试", "m_debugArea", EditorStyleEx.PixelBoxStyle);
            m_bOpenObj = area.open;
            GUILayout.Space(5);
            if (area.Show())
            {
                CurQte.ModelHero = (Transform)EditorGUILayout.ObjectField("主角模型", CurQte.ModelHero, typeof(Transform), true);
                CurQte.ModelTarget = (Transform)EditorGUILayout.ObjectField("敌人模型", CurQte.ModelTarget, typeof(Transform), true);
                CurQte.ModelCamera = (Transform)EditorGUILayout.ObjectField("相机模型", CurQte.ModelCamera, typeof(Transform), true);
            }

            EditorGUILayoutEx.instance.EndFadeArea();
            Repaint();
        }
    }

    //绘制当前编辑内容
    void DrawCurQte()
    {
        LayoutArea();

        //绘制标签刻度
        using (new AutoBeginArea(RightTopArea_Rect))
        {
            foreach (var tag in m_timeTagData)
            {
                using (new AutoChangeBkColor(new Color(1.0f, 0.7f, 0.1f, 0.65f)))
                    GUI.DrawTexture(tag.TagRect, EditorStyleEx.TimelineMarker);
                if (tag.TagLabel != string.Empty)
                {
                    using (new AutoChangeContentColor(new Color(1.0f, 1.4f, 0.8f, 0.9f)))
                        GUI.Label(tag.TagLabelRect, tag.TagLabel);
                }
            }
            //绘制当前位置红色标记
            float curPos = TimeToContentX(CurRunningTime);
            float halfHandleTag_W = 3.0f;
            Rect handleTagRect = new Rect(curPos - halfHandleTag_W, 0.0f, halfHandleTag_W * 2.0f, RightTopArea_Rect.height);
            using (new AutoChangeContentColor(new Color(1.0f, 0.1f, 0.1f, 0.65f)))
                GUI.DrawTexture(handleTagRect, EditorStyleEx.TimelineScrubHead);

            //当前播到的位置
            handleTagRect.x += handleTagRect.width;
            handleTagRect.width = 100.0f;
            GUI.Label(handleTagRect, CurRunningTime.ToString("#.##"));

            //点击拖动
            if (Event.current.type == EventType.MouseDown)
                m_bHandleDrag = true;
            if (Event.current.rawType == EventType.MouseUp)
                m_bHandleDrag = false;

            if (m_bHandleDrag && Event.current.isMouse)
            {
                float mousePosOnTimeline = ContentXToTime(Event.current.mousePosition.x);
                CurQte.SetRunning(mousePosOnTimeline);
                Event.current.Use();
            }
        }

        //绘制事件内容
        DrawWinStage();

        DrawLoseStage();

        //绘制红线
        Rect lineRect = new Rect(RightTopArea_Rect.x + TimeToContentX(CurRunningTime), RightWinArea_Rect.y, 1.0f, 1000);
        if (lineRect.x < RightTopArea_Rect.x)
            return;
        using (new AutoChangeColor(new Color(1.0f, 0.1f, 0.1f, 0.65f)))
            GUI.DrawTexture(lineRect, EditorStyleEx.TimelineScrubTail);
    }

    void DrawWinStage()
    {
        using (new AutoBeginArea(RightWinArea_Rect))
        {
            using (new AutoBeginHorizontal())
            {
                foreach (QteStageEditor e in m_stageEditors)
                {
                    bool bChange = false;
                    if (e.m_info.idx == CurQte.CurStageIdx && CurQte.IsWin)
                        bChange = true;
                    using (new AutoChangeColor(bChange ? Color.green : Color.gray))
                        e.OnGUIWin();
                }

                //绘制阶段线
                foreach (QteStageEditor e in m_stageEditors)
                {
                    bool bChange = false;
                    if (e.m_info.idx == CurQte.CurStageIdx && CurQte.IsWin)
                        bChange = true;
                    float endPosX = TimeToContentX(e.WinEndTime);
                    //分割线
                    Rect lineRect = new Rect(endPosX, 0, 1.0f, WinStage_H);
                    using (new AutoChangeColor(bChange ? Color.red : new Color(0.2f, 0.8f, 0.7f, 0.65f)))
                        GUI.DrawTexture(lineRect, EditorStyleEx.TimelineScrubTail);
                }
            }
        }
    }

    void DrawLoseStage()
    {
        using (new AutoBeginArea(RightLoseArea_Rect))
        {
            using (AutoBeginScrollView sv = new AutoBeginScrollView(m_scrollLoseStage, "PreferencesSectionBox", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                m_scrollLoseStage = sv.Scroll;
                using (new AutoBeginVertical())
                {
                    foreach (QteStageEditor e in m_stageEditors)
                    {
                        bool bChange = false;
                        if (e.m_info.idx == CurQte.CurStageIdx && !CurQte.IsWin)
                            bChange = true;
                        using (new AutoChangeColor(bChange ? Color.green : Color.gray))
                            e.OnGUILose();
                    }

                    //绘制阶段线
                    foreach (QteStageEditor e in m_stageEditors)
                    {
                        if (e.m_info.loseInfo != null)
                        {
                            bool bChange = false;
                            if (e.m_info.idx == CurQte.CurStageIdx && !CurQte.IsWin)
                                bChange = true;
                            float endPosX = TimeToContentX(e.LoseEndTime);
                            //分割线
                            Rect lineRect = new Rect(endPosX, 0, 1.0f, 1000);
                            using (new AutoChangeColor(bChange ? Color.red : new Color(0.2f, 0.8f, 0.7f, 0.65f)))
                                GUI.DrawTexture(lineRect, EditorStyleEx.TimelineScrubTail);
                        }

                    }
                }
            }
        }
    }

    //布局
    void LayoutArea()
    {
        using (new AutoBeginHorizontal())
        {
            GUILayout.Box("", EditorStyleEx.BoxStyle, GUILayout.Width(LeftArea_W), GUILayout.ExpandHeight(true));
            if (Event.current.type == EventType.Repaint)
            {
                if (LeftArea_Rect != GUILayoutUtility.GetLastRect())
                {
                    LeftArea_Rect = GUILayoutUtility.GetLastRect();
                }
            }

            using (new AutoBeginVertical())
            {
                GUILayout.Box("", EditorStyleEx.BoxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(TimeLine_H));
                if (Event.current.type == EventType.Repaint)
                {
                    if (RightTopArea_Rect != GUILayoutUtility.GetLastRect())
                    {
                        RightTopArea_Rect = GUILayoutUtility.GetLastRect();
                        RightTopArea_Rect.width -= 10;
                        UpdateTagData();
                    }
                }

                using (new AutoBeginVertical())
                {
                    GUILayout.Box("", EditorStyleEx.BoxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(WinStage_H));
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (RightWinArea_Rect != GUILayoutUtility.GetLastRect())
                        {
                            RightWinArea_Rect = GUILayoutUtility.GetLastRect();
                            RightWinArea_Rect.width -= 10;
                        }
                    }

                    GUILayout.Box("", EditorStyleEx.BoxStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (RightLoseArea_Rect != GUILayoutUtility.GetLastRect())
                        {
                            RightLoseArea_Rect = GUILayoutUtility.GetLastRect();
                            RightLoseArea_Rect.width -= 10;
                        }
                    }
                }

            }
        }
    }
    //绘制底部
    void DrawBottomBar()
    {
        GUILayout.Box("", EditorStyleEx.BoxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(Bar_H));
        if (Event.current.type == EventType.Repaint)
        {
            if (Bottom_Rect != GUILayoutUtility.GetLastRect())
            {
                Bottom_Rect = GUILayoutUtility.GetLastRect();
            }
        }

        using (new AutoBeginArea(Bottom_Rect))
        {
            using (new AutoLabelWidth(32))
            {
                if (CurQteCfg != null)
                {
                    using (new AutoBeginHorizontal())
                    {
                        //总的时间
                        float time = EditorGUILayout.FloatField("时长:", CurQteCfg.Duration, GUILayout.Width(BottomLab_W));
                        time = Mathf.Max(time, 1);
                        if (time != CurQteCfg.Duration)
                        {
                            CurQteCfg.Duration = time;
                        }
                        GUILayout.Space(30);
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        BigQte.OnQteChangedCallback += UpdateStage;
    }

    #endregion

    //更新tag数据
    void UpdateTagData()
    {
        m_timeTagData.Clear();

        m_eachTagDist = RightTopArea_Rect.width / CurQteCfg.Duration;
        float minXTag_H = RightTopArea_Rect.height * 0.5f;
        float midXTag_H = RightTopArea_Rect.height * 0.7f;
        float maxXTag_H = RightTopArea_Rect.height * 0.85f;

        float eachNum = RightTopArea_Rect.width / m_eachTagDist;
        m_eachTagValue = Mathf.Ceil(CurQteCfg.Duration / eachNum);

        PixelTimeline_W = m_eachTagDist * (CurQteCfg.Duration / m_eachTagValue);

        float tagValue = 0.0f;
        Rect tagRect = new Rect(0, 0.0f, 1.0f, maxXTag_H);
        while (tagRect.x < RightTopArea_Rect.width)
        {
            if (m_eachTagValue > CurQteCfg.Duration)
                break;

            //大刻度
            TimeTagData data = new TimeTagData();
            m_timeTagData.Add(data);
            data.TagRect = tagRect;
            data.TagLabelRect = new Rect(tagRect.x + 2.0f, tagRect.y + 7, 40.0f, RightTopArea_Rect.height);
            data.TagLabel = tagValue.ToString();

            //小刻度
            for (int n = 0; n < 10; n++)
            {
                float smallPos = tagRect.x + m_eachTagDist / 10.0f * n;
                Rect smallTagRect = tagRect;
                smallTagRect.x = smallPos;
                smallTagRect.height = minXTag_H;

                if (n == 5)
                    smallTagRect.height = midXTag_H;

                TimeTagData smallData = new TimeTagData();
                m_timeTagData.Add(smallData);
                smallData.TagRect = smallTagRect;
            }
            tagRect.x += m_eachTagDist;
            tagValue += m_eachTagValue;
        }
    }

    void ProcessHotkeys()
    {
        if (Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.P)
        {
            CurQte.PlayOrPause();
            Event.current.Use();
        }

        if (Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
        {
            CurQte.Stop();
            Event.current.Use();
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Period)
        {
            CurQte.NextKeyframe();
            Event.current.Use();
        }
        else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Comma)
        {
            CurQte.PrevKeyframe();
            Event.current.Use();
        }

    }

    //根据时间获取位置
    public float TimeToContentX(float time)
    {
        return (time / m_eachTagValue * m_eachTagDist);
    }

    //根据位置获取时间
    public float ContentXToTime(float pos)
    {
        CurQte.m_recordTime = 0.0f;
        CurQte.IsPlaying = false;
        return (pos / RightTopArea_Rect.width) * CurQteCfg.Duration;
    }
    void UpdateQte()
    {
        if (!Application.isPlaying)
        {
            if (CurQte != null && CurQte.IsPlaying)
            {
                float time = Time.realtimeSinceStartup;
                CurQte.UpdateEvent(time - m_prevTime);
                Repaint();
            }
        }

    }

    void Stop()
    {
        CurRunningTime = 0.0f;
        CurQte.IsPlaying = false;
    }

    public QteStageInfo AddStage()
    {
        QteStageInfo info = CurQte.AddStage();
        QteStageEditor stageEditor = new QteStageEditor();
        stageEditor.Init(info);
        m_stageEditors.Add(stageEditor);
        UpdateStage();
        Repaint();
        return info;
    }

    public void UpdateStage()
    {
        m_stageEditors.Clear();
        for (int i = 0; i < CurQteCfg.stages.Count; i++)
        {
            QteStageEditor editor = new QteStageEditor();
            CurQteCfg.stages[i].idx = i;
            editor.Init(CurQteCfg.stages[i]);
            m_stageEditors.Add(editor);
        }
    }
}
