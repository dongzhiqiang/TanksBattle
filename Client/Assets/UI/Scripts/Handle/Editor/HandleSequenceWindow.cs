/**********************************************************
 * 名称：处理序列编辑器
 
 * 日期：2015.7.23
 * 描述：
 * *********************************************************/
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class HandleSequenceWindow : EditorWindow
{
    public static void ShowWnd(object param)
    {
        object[] pp = (object[])param;
        ShowWnd((Component)pp[0],(Handle)pp[1]);
    }

    public static void ShowWnd(Component c,Handle h)
    {
        if (c == null || h == null)
        {
            Debuger.LogError("打开处理序列编辑器的参数为空");
            return;
        }
            
        HandleSequenceWindow wnd = (HandleSequenceWindow)EditorWindow.GetWindow(typeof(HandleSequenceWindow));
        wnd.m_comp = c;
        wnd.m_handle = h;
        wnd.titleContent = new GUIContent("处理序列编辑器");
        wnd.minSize = new Vector2(750.0f, 400f);
        wnd.autoRepaintOnSceneChange = true;

    }
    #region 各类事件监听
    public void Awake()
    {
        

    }

    //更新
    void Update()
    {
        m_handle.Update();
        if (m_handle.IsPlaying)
            _curTime = m_handle.CurFactor * m_handle.Duration;
    }

    void OnEnable()
    {
      
    }

    void OnDisable()
    {
        //Debuger.Log("当窗口disable时调用一次");
    }
    void OnFocus()
    {
        //Debuger.Log("当窗口获得焦点时调用一次");
    }

    void OnLostFocus()
    {
        //Debuger.Log("当窗口丢失焦点时调用一次");
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
        //当窗口处于开启状态，并且在Hierarchy视图中选择某游戏对象时调用
        //foreach (Transform t in Selection.transforms)
        //{
        //    //有可能是多选，这里开启一个循环打印选中游戏对象的名称
        //    Debuger.Log("OnSelectionChange" + t.name);
        //}
    }

    void OnDestroy()
    {
        //Debuger.Log("当窗口关闭时调用");
    }

    #endregion
    public Component m_comp;
    public Handle m_handle;
    public SequenceHandle m_subHandles;
    public HandleSequence m_seq;
    public 
    
    
    //绘制窗口时调用
    void OnGUI()
    {
        if (m_comp == null || m_handle == null || m_handle.m_type != Handle.Type.sequence)
        {
            this.ShowNotification(new GUIContent("空指针或处理的类型不是序列了。请设置正确后再打开编辑器"));
            return;
        }
        this.RemoveNotification();

        if (m_handle.m_go == null && m_comp.GetComponent<SequenceHandle>() != null)
        {
            m_handle.m_go = m_comp.gameObject;
            EditorUtil.SetDirty(m_comp.gameObject);
        }

        if (m_handle.m_go == null)
        {
            if (GUILayout.Button("原对象上创建一个SequenceHandle", GUILayout.Height(50)))
            {
                m_comp.AddComponentIfNoExist<SequenceHandle>();
                EditorUtil.SetDirty(m_comp.gameObject);
            }
            this.ShowNotification(new GUIContent("找不到SequenceHandle"));
            return;
        }
        //每次都重新获取下吧
        m_seq = m_handle.CurHandle as HandleSequence;
        m_subHandles = m_handle.m_go.GetComponent<SequenceHandle>();

        //工具栏
        DrawTopToolbar();

        using (new AutoBeginHorizontal())
        {
            //左边序列控件属性区
            DrawLeftInfo();

            //右边时间轴
            GUIStyle style  = GUI.skin.box;//先收窄box的边距
            GUIStyle boxStyle = new GUIStyle(style);
            boxStyle.margin = new RectOffset(0, 0, 2, 2);
            GUI.skin.box = boxStyle;
            DrawRightTimeLine();
            GUI.skin.box = style;
        }
        
        //下边子处理详细页面
        DrawBottomSubHandles();
        

        //ProcessHotkeys();*/
    }

    
    

    //工具栏
    void DrawTopToolbar()
    {
        GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(hToolBar));

        if (Event.current.type == EventType.Repaint)
            TopToolbar = GUILayoutUtility.GetLastRect();
        using (new AutoBeginArea(TopToolbar))
        {
            using (new AutoBeginHorizontal()){
                if (GUILayout.Button("展开所有", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    foreach (var h in m_subHandles.m_handles) h.m_seqIsExpand = true;
                }
                if (GUILayout.Button("收起所有", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    foreach (var h in m_subHandles.m_handles) h.m_seqIsExpand = false;
                }
                if (GUILayout.Button("显示所有", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    foreach (var h in m_subHandles.m_handles) h.m_seqIsShow = true;
                }
                if (GUILayout.Button("隐藏所有", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    foreach (var h in m_subHandles.m_handles) h.m_seqIsShow = false;
                }
                GUILayout.Space(wToolbarSpace);

                if (GUILayout.Button(EditorGUIUtility.IconContent(m_handle.IsPlaying ? "PauseButton" : "Animation.Play"), EditorStyles.toolbarButton, GUILayout.Width(wToolbarSpace)))
                {
                    if(m_handle.IsPlaying)
                        m_handle.Clear();
                    else
                        m_handle.Start();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.PrevKey", "往前0.1秒"), EditorStyles.toolbarButton, GUILayout.Width(wToolbarSpace)))
                {
                    CurTime = CurTime - 0.1f;
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("Animation.NextKey", "往后0.1秒"), EditorStyles.toolbarButton, GUILayout.Width(wToolbarSpace)))
                {
                    CurTime = CurTime + 0.1f;
                }
                GUILayout.Space(wToolbarSpace);
            }
        }
    }

    //左边序列控件属性区
    void DrawLeftInfo()
    {
        using (new AutoBeginVertical(EditorStyles.objectFieldThumb, GUILayout.Width(wLeft), GUILayout.Height(hMiddle)))
        {
            using (new AutoLabelWidth(100))
            {
                if (m_seq.DrawGoField<SequenceHandle>(m_comp, m_handle, "子处理的外部对象"))
                    m_subHandles = m_handle.m_go.GetComponent<SequenceHandle>();

                EditorGUI.BeginChangeCheck();
                float delay = EditorGUILayout.FloatField("延迟", m_handle.m_delay);
                float duration = EditorGUILayout.FloatField("持续时间", m_handle.m_duration);
                float rate = EditorGUILayout.FloatField("倍速", m_handle.m_rate);
                bool isRealtime = EditorGUILayout.Toggle("真实时间", m_handle.m_isRealtime);
                int playType = EditorGUILayout.Popup("类型", (int)m_handle.m_playType, Handle.PlayTypeName);
                int endCount = m_handle.m_endCount;
                if (m_handle.IsEndCountValid) endCount = UnityEditor.EditorGUILayout.IntField("循环次数", m_handle.m_endCount);
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_handle.m_isRealtime != isRealtime || m_handle.m_rate != rate)
                    {
                        m_handle.m_rate = rate;
                        m_handle.m_isRealtime = isRealtime;
                        this.m_subHandles.SyncHandle(this.m_comp,this.m_handle);
                        EditorUtil.SetDirty(m_comp);
                        return;
                    }
                    EditorUtil.RegisterUndo("Handle Change", m_comp);
                    m_handle.m_delay = delay;
                    m_handle.m_duration = duration;
                    m_handle.m_playType = (Handle.PlayType)playType;
                    m_handle.m_endCount = endCount;
                    EditorUtil.SetDirty(m_comp);
                }
            }
        }
    }

    //右边时间轴
    void DrawRightTimeLine()
    {
        using (new AutoBeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(hMiddle)))
        {
            //先绘制布局
            TimeLineLayout();

            //计算鼠标点击事件
            TimeLineInput();
           
            //计算相关值
            float wOneSce = RectTimeLine.width / (m_handle.m_duration*1.04f);//1秒的长度
            float minMarkUnit = minUnit * wOneSce;//最小单位对应的长度
            int oneSceMarkCount=1;//1秒画多少个刻度，五种规格20,10,5,2,1
            if (wOneSce / 20f >= 7f)
                oneSceMarkCount = 20;    
            else if (wOneSce / 10f >= 7f)
                oneSceMarkCount = 10;    
            else if (wOneSce / 5f >= 7f)
                oneSceMarkCount = 5;
            else if (wOneSce / 2f >= 7f)
                oneSceMarkCount = 2;
            float wMark = wOneSce/oneSceMarkCount;

            //画刻度线
            int markCount = Mathf.FloorToInt(RectTimeLine.width/wMark)+1;
            float hBigMark = hTimeLine*3/4f;
            float hMidMark = hTimeLine*2/5f;
            float hSmallMark = 2f;
            float topBigMark = hToolBar+hTimeLine - hBigMark-2;
            float topMidMark = hToolBar + hTimeLine - hMidMark - 2;
            float topSmallMark = hToolBar+hTimeLine- hSmallMark-2;
            for (int i = 0; i < markCount; ++i)
            {
                if (i % oneSceMarkCount == 0)
                {
                    GUI.DrawTexture(new Rect(RectTimeLine.x + i * wMark, topBigMark, 1, hBigMark), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);
                    GUI.Label(new Rect(RectTimeLine.x + i * wMark, hToolBar - 3, 30, 15),
                        string.Format("{0:0.##}", i / oneSceMarkCount));
                }
                else if (oneSceMarkCount >= 5 && i % 5 == 0)
                {
                    GUI.DrawTexture(new Rect(RectTimeLine.x + i * wMark, topMidMark - 3, 1, hMidMark), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);
                    GUI.Label(new Rect(RectTimeLine.x + i * wMark, hToolBar - 3, 30, 15),
                        string.Format("{0:0.##}", i / (float)oneSceMarkCount));
                }
                else
                {
                    GUI.DrawTexture(new Rect(RectTimeLine.x + i * wMark, topSmallMark, 1, hSmallMark), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);
                }

                Color PreviousColor = GUI.color;
                GUI.color = i % 2 == 0 ? Color.white * 0.4f : Color.white * 0.5f;
                GUI.DrawTexture(new Rect(RectTimeLine.x + i * wMark, hToolBar + hTimeLine + hTimeLineHandle + 4, 1, hMiddle - hTimeLineHandle - hTimeLine), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);
                GUI.color = PreviousColor;
            }
            
            //画点
            int idx = 0;
            float time = -1;
            int idxTime = -1;
            foreach (var h in this.m_subHandles.m_handles)
            {
                //画处理组，大菱形
                if (!Mathf.Approximately(h.m_delay, time))
                {
                    idx = 0;
                    ++idxTime;
                    time = h.m_delay;
                    Color PreviousColor = GUI.color;

                    GUI.color = ((CurSelectHandles != null && CurSelectHandles.IndexOf(h) != -1) || (PreRemoveSubHandles != null && PreRemoveSubHandles == h)) ?
                        Color.red : (h.m_seqIsExpand ? new Color(0.3f, 0.55f, 0.95f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f));//区域选中的要显示红色，展开的要显示蓝色
                    GUI.DrawTexture(
                        new Rect(RectTimeLine.x + h.m_delay * wOneSce - 11,
                        hToolBar + hTimeLine-4,
                        22, 22),
                        EditorGUIUtility.IconContent("blendKey").image, ScaleMode.StretchToFill);
                    GUI.color = PreviousColor;
                    GUIStyle style = new GUIStyle();
                    style.richText = true;
                    GUI.Label(new Rect(RectTimeLine.x + h.m_delay * wOneSce - 4, hToolBar + hTimeLine, 20, 20), string.Format("<b><color=maroon>{0}</color></b>", idxTime), style);
                }
                else
                {
                    ++idx;
                }
                    
                
                //画处理,小菱形
                if (idx < ((ScrollHandles.y - hHandleRow / 2) / (hHandleRow + 2)) ||
                     idx > (handleCount + ScrollHandles.y / (hHandleRow + 2)))//在滚动区域外不显示
                    continue;

                float left = RectHandles.x + h.m_delay * wOneSce - 6;
                float top = -ScrollHandles.y + RectHandles.yMax  + (hHandleRow + 2f) * idx;
                Color PreviousColor2 = GUI.color;
                GUI.color = this.PreRemoveSubHandle !=null && this.PreRemoveSubHandle == h?
                    Color.red : (h.m_seqIsShow ? new Color(0.3f, 0.55f, 0.95f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f));//显示参数详细页面要显示蓝色
                //子处理，小菱形
                GUI.DrawTexture(new Rect(left, top, 13, 13), EditorGUIUtility.IconContent("blendKey").image, ScaleMode.StretchToFill);
                GUI.color = PreviousColor2;

                //如果是展开的话要显示名字
                if (h.m_seqIsExpand)
                    GUI.Label(new Rect(left + 10, top, 40, 15), h.CurTypeName);
            }

            //当前时间线
            using (new AutoChangeColor(Color.yellow))
                GUI.DrawTexture(new Rect(RectTimeLine.x + CurTime * wOneSce, RectTimeLine.y, 1, hMiddle+5), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);

            //拖动中的处理组
            if (CurDragHandles != null)
            {
                using (new AutoChangeColor(Color.blue * 0.6f))
                    GUI.DrawTexture(new Rect(RectHandles.x + DragHandlesTime * wOneSce, RectHandles.y, 1, hMiddle - hTimeLine), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);
            }

            //选择中的区域
            if (CanSelect || (!CanDragSelect && CurSelectHandles != null))
            {
                float left = SelectLeft<= SelectRight ? SelectLeft : SelectRight;
                float w = Mathf.Abs(SelectRight - SelectLeft) * wOneSce;
                using (new AutoChangeColor(Color.red * 0.4f))
                    GUI.DrawTexture(new Rect(
                        RectHandles.x + left * wOneSce,
                        RectHandles.y,
                        w,RectHandles.height), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);
            }

            //拖动中的区域
            if (CanDragSelect)
            {
                float left = SelectLeft <= SelectRight ? SelectLeft : SelectRight;
                left = left + Util.ClosestOf(SelectOffsetTime, minUnit) ;
                float w = Mathf.Max(Mathf.Abs(SelectRight - SelectLeft) * wOneSce, 15f);
                using (new AutoChangeColor(Color.red * 0.4f))
                    GUI.DrawTexture(new Rect(
                        RectHandles.x + left * wOneSce,
                        RectHandles.y,
                        w, RectHandles.height), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);
            }

            //拖动中的处理
            if (CurDragSubHandle != null)
            {
                using (new AutoChangeColor(Color.blue * 0.6f))
                    GUI.DrawTexture(new Rect(RectHandles.x + DragSubHandleTime * wOneSce, RectHandles.y, 1, hMiddle - hTimeLine), EditorUtil.LoadTexture2D("blank"), ScaleMode.ScaleAndCrop);
            }
        }
    }

    //布局timeline，取得所有位置
    void TimeLineLayout()
    {
        //时间轴
        using(new AutoBeginHorizontal()){
            GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(hTimeLine));
            if (Event.current.type == EventType.Repaint)
                RectTimeLine = GUILayoutUtility.GetLastRect();
            GUILayout.Space(wRightScrollBar);
        }
        

        //某一时间对应的处理组的父节点区域
        using (new AutoBeginHorizontal())
        {
            GUILayout.Box("", "AnimationEventBackground",GUILayout.ExpandWidth(true), GUILayout.Height(hTimeLine));
            if (Event.current.type == EventType.Repaint)
                RectHandles = GUILayoutUtility.GetLastRect();
            GUILayout.Space(wRightScrollBar);
        }
        

        //子节点区域
        using (AutoBeginScrollView a = new AutoBeginScrollView(ScrollHandles, false, true, GUILayout.ExpandWidth(true), GUILayout.Height(hMiddle - hTimeLine - hTimeLineHandle)))
        {
            ScrollHandles = a.Scroll;

            int maxCount =this.m_subHandles.MaxCount;
            for (int i = 0; i < maxCount; ++i)
            {
                Color PreviousColor = GUI.color;
                GUI.color = i % 2 == 0 ? Color.white * 0.3f : Color.white * 0.6f;
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(hHandleRow));
                GUI.color = PreviousColor;

                //if (Event.current.type == EventType.Repaint)
                //    uiSeq.RectHandles[i] = GUILayoutUtility.GetLastRect();
            }
        }
    }

    //计算时间轴区域的点击事件
    void TimeLineInput()
    {
        float wOneSce = RectTimeLine.width / (m_handle.m_duration * 1.04f);//1秒的长度
        float minMarkUnit = minUnit * wOneSce;//最小单位对应的长度

        //鼠标点击事件
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            //跳至对应时间
            if (RectTimeLine.Contains(Event.current.mousePosition))
            {
                CanDragTimeLine = true;
                CurTime = (Event.current.mousePosition.x - RectTimeLine.x) / wOneSce;
                Event.current.Use();
            }
            //处理组的展开和拖动
            else if (RectHandles.Contains(Event.current.mousePosition) )
            {
                float t = (Event.current.mousePosition.x - RectHandles.x) / wOneSce;
                Handle h = m_subHandles.GetClosest(t, 0, 8 / wOneSce);//5像素内点击成功

                if (CurSelectHandles != null)
                {
                    if (t >= SelectLeft && t <= SelectRight)
                        CanDragSelect = true;
                    else
                        CurSelectHandles = null;
                }
                else if (h != null)
                    CurDowmHandles = h;
                else
                {
                    CanSelect = true;
                    SelectLeft = t;
                    SelectRight = t;
                }
                Event.current.Use();
            }
            //处理的显示和拖动
            else if (RectSubHandle.Contains(Event.current.mousePosition) )
            {
                CurDowmSubHandle = m_subHandles.GetClosest(
                    (Event.current.mousePosition.x - RectHandles.x) / wOneSce,
                    (int)((Event.current.mousePosition.y - ScrollHandles.y - RectHandles.yMax - 5) / (hHandleRow + 2f)),
                    7 / wOneSce);//5像素内点击成功
                Event.current.Use();
            }
        }
        else if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            float t = Mathf.Clamp((Event.current.mousePosition.x - RectTimeLine.x) / wOneSce, 0, m_handle.m_duration);
            float tClosest = Mathf.Clamp(Util.ClosestOf((Event.current.mousePosition.x - RectTimeLine.x) / wOneSce, minUnit), 0, m_handle.m_duration);
            bool use =true;
            //跳至对应时间
            if (CanDragTimeLine == true)
                CurTime = t;
            //处理组的拖动
            else if (CurDowmHandles != null)
            {
                CurDragHandles = CurDowmHandles;
                CurDowmHandles = null;
                DragHandlesTime = tClosest;
            }
            else if (CurDragHandles != null)
                DragHandlesTime = tClosest;
            else if (CurDowmSubHandle != null)
            {
                CurDragSubHandle = CurDowmSubHandle;
                CurDowmSubHandle = null;
                DragSubHandleTime = tClosest;
            }
            else if (CurDragSubHandle != null)
                DragSubHandleTime = tClosest;
            else if (CanSelect)
                SelectRight = t;
            else if (CanDragSelect)
            {
                SelectOffsetTime = Mathf.Clamp(
                    SelectOffsetTime+ Event.current.delta.x / wOneSce,
                    -SelectLeft,
                    m_handle.m_duration  -SelectRight);
            }
            else
                use = false;
            if(use)Event.current.Use();
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            bool use = true;
            //处理组的拖动
            if (CurDowmHandles != null)
                m_subHandles.ToggleExpand(CurDowmHandles.m_delay);
            else if (CurDragHandles != null)
                m_subHandles.ChangeTime(m_comp,m_handle,CurDragHandles.m_delay, DragHandlesTime);
            else if (CurDowmSubHandle != null)
                CurDowmSubHandle.m_seqIsShow = !CurDowmSubHandle.m_seqIsShow;
            else if (CurDragSubHandle != null)
                m_subHandles.ChangeTime(m_comp, m_handle, CurDragSubHandle, DragSubHandleTime);
            else if (CanDragSelect )
            {
                SelectOffsetTime = Util.ClosestOf(SelectOffsetTime, minUnit);
                m_subHandles.OffsetTime(m_comp, m_handle, CurSelectHandles, SelectOffsetTime);
                CurSelectHandles = null;
            }
            else if (CanSelect)
            {
                CurSelectHandles = m_subHandles.GetRange(SelectLeft, SelectRight);
                SelectOffsetTime = 0;
                if (CurSelectHandles.Count == 0)
                    CurSelectHandles = null;
                else
                {
                    SelectLeft = CurSelectHandles[0].m_delay;
                    SelectRight = CurSelectHandles[CurSelectHandles.Count-1].m_delay;
                }
             
            }
            else
                use = false;

            CanDragTimeLine = false;
            CurDowmHandles = null;
            CurDragHandles = null;
            CurDowmSubHandle = null;
            CurDragSubHandle = null;
            CanSelect = false;
            CanDragSelect = false;
            this.PreRemoveSubHandles = null;
            this.PreRemoveSubHandle = null;
            if (use) Event.current.Use();
        }
        else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            float tClosest = Mathf.Clamp(Util.ClosestOf((Event.current.mousePosition.x - RectTimeLine.x) / wOneSce, minUnit), 0, m_handle.m_duration);
            bool use = true;
            //跳至对应时间
            if (RectHandles.Contains(Event.current.mousePosition))
            {
                this.PreRemoveSubHandle = null;
                this.PreRemoveSubHandles = m_subHandles.GetClosest(tClosest, 0, 8 / wOneSce);//5像素内点击成功
            }
            //处理的增加删除
            else if (RectSubHandle.Contains(Event.current.mousePosition))
            {
                this.PreRemoveSubHandle = m_subHandles.GetClosest(tClosest,
                    (int)((Event.current.mousePosition.y - ScrollHandles.y - RectHandles.yMax - 5) / (hHandleRow + 2f)),
                    7 / wOneSce);//5像素内点击成功
                this.PreRemoveSubHandles = null;
            }
            else
                use = false;
            if(use)Event.current.Use();
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
        {
            if (RectHandles.Contains(Event.current.mousePosition)|| RectSubHandle.Contains(Event.current.mousePosition))
            {
                float tClosest = Mathf.Clamp(Util.ClosestOf((Event.current.mousePosition.x - RectTimeLine.x) / wOneSce, minUnit), 0, m_handle.m_duration);
                GenericMenu contextMenu = new GenericMenu();

                if (this.PreRemoveSubHandles != null)
                {
                    contextMenu.AddItem(new GUIContent("删除"), false, () =>//不导出中文
                    {
                        this.m_subHandles.RemoveSubHandle(this.m_comp, this.m_handle, this.PreRemoveSubHandles.m_delay);
                        this.PreRemoveSubHandles = null;
                        this.PreRemoveSubHandle = null;
                    });
                }
                else if (this.PreRemoveSubHandle != null)
                {
                    contextMenu.AddItem(new GUIContent("删除"), false, () =>//不导出中文
                    {
                        this.m_subHandles.RemoveSubHandle(this.m_comp, this.m_handle, this.PreRemoveSubHandle);
                        this.PreRemoveSubHandles = null;
                        this.PreRemoveSubHandle = null;
                    });
                }
                for (int i = 0; i < (int)Handle.Type.max; ++i)
                {
                    int tem = i;
                    contextMenu.AddItem(new GUIContent("添加/" + Handle.TypeName[tem]), false, () =>//不导出中文
                    {
                        this.m_subHandles.AddSubHandle(this.m_comp, this.m_handle, (Handle.Type)tem, tClosest);
                    });
                }
                contextMenu.ShowAsContext();
                Event.current.Use();
                
            }
        }
    }
    
    Color clrTitle = new Color(0.9f,0.9f,1f);
    //下边子处理详细页面
    void DrawBottomSubHandles()
    {
        Handle  remove = null;
        using (AutoBeginScrollView a = new AutoBeginScrollView(ScrollBottom, false, false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            ScrollBottom = a.Scroll;
            using (new AutoBeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                int idx = 0;
                float time = -1;
                int idxTime = -1;//第几个时间点，包含不可见的
                int row = 0;//第几行,可见的
                int col = 0;//第几列,可见的
                bool isDurationInvalid = false;//是不是即时的
                Color PreviousColor = GUI.backgroundColor;
                foreach (var h in this.m_subHandles.m_handles)
                {
                    //画处理组，大菱形
                    if (!Mathf.Approximately(h.m_delay, time))
                    {
                        idx = 0;
                        ++idxTime;
                        time = h.m_delay;
                        row = 0;    
                    }
                    else
                        ++idx;

                    if (h.m_seqIsShow ==false)
                        continue;

                    //新一列
                    if (row == 0 && h.m_seqIsShow)
                    {
                        if (col != 0)
                        {
                            GUILayout.EndVertical();
                            GUILayout.EndScrollView();
                        }

                        GUI.backgroundColor = col % 2 == 0 ? Color.white :new Color(0.90f,0.90f,1f);
                        ScrolBottomCol[col] = GUILayout.BeginScrollView(ScrolBottomCol[col],  GUILayout.Width(250), GUILayout.ExpandHeight(true));
                        GUILayout.BeginVertical(EditorStyles.miniButtonMid,GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                        //这一列是不是即时的
                        GUILayout.BeginHorizontal( GUILayout.ExpandWidth(true));
                        float old2 = UnityEditor.EditorGUIUtility.labelWidth;
                        UnityEditor.EditorGUIUtility.labelWidth = 50;
                        isDurationInvalid = EditorGUILayout.Toggle("即时", h.m_isDurationInvalid, GUILayout.ExpandWidth(true));
                        if (isDurationInvalid != h.m_isDurationInvalid)
                            this.m_subHandles.SetDurationInvalid(h.m_delay,isDurationInvalid);
                        UnityEditor.EditorGUIUtility.labelWidth = old2;
                        GUIStyle style = new GUIStyle();
                        style.richText = true;
                        GUILayout.Label(string.Format("<color=white>索引:<size=14><b><color=maroon>{0}</color></b></size> {1:0.##}</color>", idxTime, h.m_delay), style, GUILayout.Width(60));
                        GUILayout.EndHorizontal();
                        ++col;
                    }


                    GUI.backgroundColor = PreviousColor;
                    GUILayout.BeginVertical(EditorStyles.objectFieldThumb, GUILayout.ExpandWidth(true));
                    GUILayout.Space(5);
                    if (!isDurationInvalid)
                    {
                        
                        //画标题
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("隐藏", EditorStyles.miniButtonLeft, GUILayout.Width(50), GUILayout.Height(20)))
                            h.m_seqIsShow = false;
                        GUIStyle style = new GUIStyle(EditorStyles.miniButtonMid);
                        style.richText = true;
                        GUILayout.Label(string.Format("<size=14><color=white>{0}{1}</color></size>", h.CurTypeName,h.m_id), style, GUILayout.ExpandWidth(true), GUILayout.Height(20));
                        if (GUILayout.Button("删除", EditorStyles.miniButtonRight, GUILayout.Width(50), GUILayout.Height(20)))
                            remove = h;
                        GUILayout.EndHorizontal();

                        //画内容
                        float old = UnityEditor.EditorGUIUtility.labelWidth;
                        UnityEditor.EditorGUIUtility.labelWidth = 50;
                        if (h.CurHandle.OnDrawGo(m_comp, h, "对象"))
                            h.CurHandle.OnReset(h, false, true);
                        h.CurHandle.OnDrawMid(m_comp, h, SimpleHandleEditor.OnOpenWnd, true);
                        UnityEditor.EditorGUIUtility.labelWidth = old;
                    }
                    else
                    {
                        //即时处理的话显示得简单点
                        float old = UnityEditor.EditorGUIUtility.labelWidth;
                        UnityEditor.EditorGUIUtility.labelWidth = 50;
                        GUILayout.BeginHorizontal();
                        if (h.CurHandle.OnDrawGo(m_comp, h, h.CurTypeName))
                            h.CurHandle.OnReset(h, false, true);
                        if (GUILayout.Button("删除", GUILayout.Width(50), GUILayout.Height(20)))
                            remove = h;
                        GUILayout.EndHorizontal();
                        h.CurHandle.OnDrawMin(m_comp, h, SimpleHandleEditor.OnOpenWnd, true);
                        UnityEditor.EditorGUIUtility.labelWidth = old;
                    }
                    GUILayout.Space(5);
                    GUILayout.EndVertical();
                    

                    ++row;
                }

                if (col != 0) {
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                }
                GUI.backgroundColor = PreviousColor; 
            }

        }

        //删除
        if (remove!= null)
        {
            m_subHandles.RemoveSubHandle(this.m_comp, this.m_handle,remove);
        }
    }

    /*private void ProcessHotkeys()
    {
        if (Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.P&& Event.current.alt)
        {
            PlayOrPause();
            Event.current.Use();
        }

        if (Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
        {
            Stop();
            Event.current.Use();
        }
    }*/

    
}
