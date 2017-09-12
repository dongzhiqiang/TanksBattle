using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


//镜头编辑器
public class CameraEditorWindow : EditorWindow
{
    [MenuItem("Art/场景/镜头编辑器", false, 9)]
    public static void ShowWindow()
    {
        CameraEditorWindow instance = (CameraEditorWindow)EditorWindow.GetWindow(typeof(CameraEditorWindow));//很遗憾，窗口关闭的时候instance就会为null
        instance.titleContent = new GUIContent("镜头编辑器");//不导出中文
    }

    string m_tip = "提示\n";//不导出中文
    string m_notifity = "";
    Vector2 m_scroll = Vector2.zero;
    int m_observer;

    CameraPath cameraPath = null;

    #region 监听
    public void OnEnable()
    {    
        m_observer = DrawGL.Add(OnDrawGL);
    }

    public void OnDisable()
    {
        if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }
    }

    public void Awake()
    {
        

    }

    //更新
    void Update()
    {
        this.Repaint();
    }

    //显示和focus时初始化下
    void OnFocus()
    {
        
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

    void OnDrawGL(object obj)
    {
        if (CameraMgr.instance==null) return;
        CameraHandle handle = CameraMgr.instance.CurHandle;
        if (handle == null) return;

        DrawGL draw = (DrawGL)obj;
        handle.OnDrawGL(draw);
    }
    
    //绘制窗口时调用
    void OnGUI()
    {
   
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("运行的时候才可以编辑", MessageType.Error);
            return;
        }
        if (CameraMgr.instance == null)
        {
            EditorGUILayout.HelpBox("没有CameraMgr", MessageType.Error);
            return;
        }
        if (CameraTriggerMgr.instance == null)
        {
            GameObject mgr = new GameObject("CameraTriggerManage", typeof(CameraTriggerMgr));
            mgr.GetComponent<CameraTriggerMgr>().AddGroup();
        }
        if (CameraTriggerMgr.instance.CurGroup == null)
        {
            EditorGUILayout.HelpBox("当前的镜头组为空", MessageType.Error);
            return;
        }
        if (!CameraMgr.instance.IsCache)
        {
            return;
        }

        m_notifity = "";
        DrawToolBar();//工具栏
        //DrawTestStill();//测试相机管理器的Still接口，这个接口提供了战斗用的推镜的功能
        DrawGroup();//当前镜头组的信息
        DrawTriggers();//所有镜头

        this.RemoveNotification();
        if(!string.IsNullOrEmpty(m_notifity))
            this.ShowNotification(new GUIContent(m_notifity));
    }

    Vector3 testStillOffset = Vector3.zero;
    float testStillMoveTime =1;
    float testStillStopTime = 2;
    float testStillFov = -1;
    float testStillHorizontalAngle = -1;
    float testStillVerticalAngle = -1;
    float testStillDistance = -1;
    float testStillOverDuration = 0.5f;

    void DrawTestStill()
    {
        testStillOffset = EditorGUILayout.Vector3Field("位移", testStillOffset);
        testStillMoveTime = EditorGUILayout.FloatField("移动时间", testStillMoveTime);
        testStillStopTime = EditorGUILayout.FloatField("停留时间", testStillStopTime);
        testStillFov = EditorGUILayout.FloatField("视野", testStillFov);
        testStillHorizontalAngle = EditorGUILayout.FloatField("水平角", testStillHorizontalAngle);
        testStillVerticalAngle = EditorGUILayout.FloatField("高度角", testStillVerticalAngle);
        testStillDistance = EditorGUILayout.FloatField("距离", testStillDistance);
        testStillOverDuration = EditorGUILayout.FloatField("结束时间", testStillOverDuration);
        if (GUILayout.Button("测试"))
        {
            Transform follow = CameraMgr.instance.GetFollow();
            CameraMgr.instance.Still(follow.position, follow.forward, testStillOffset,testStillMoveTime, testStillStopTime,
                testStillFov, testStillHorizontalAngle, testStillVerticalAngle, testStillDistance,testStillOverDuration);
        }
    }

    void DrawToolBar()
    {
        using (new AutoBeginHorizontal(EditorStyles.toolbar,GUILayout.ExpandWidth(true)))
        {
            CameraTriggerMgr mgr = CameraTriggerMgr.instance;
            string[] groupNames = mgr.GroupNames;
            int idx = System.Array.IndexOf(groupNames, mgr.CurGroup.name);
            int newIdx = EditorGUILayout.Popup(idx, groupNames, EditorStyles.toolbarPopup, GUILayout.Width(130));
            if (newIdx != idx)
                mgr.CurGroup = mgr.GetGroupByName(groupNames[newIdx]);

            if (GUILayout.Button("增加镜头组", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                mgr.CurGroup = mgr.AddGroup();
            }

            if (GUILayout.Button("当前组增加镜头", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                mgr.CurGroup.AddTrigger();
            }
        }
    }

    void DrawGroup()
    {
        CameraTriggerMgr mgr = CameraTriggerMgr.instance;
       

        mgr.CurGroup.gameObject.name = EditorGUILayout.TextField("镜头组名", mgr.CurGroup.gameObject.name);
        using (new AutoBeginHorizontal())
        {
            EditorGUILayout.PrefixLabel("出生点");
            mgr.CurGroup.m_bornPos = EditorGUILayout.Vector3Field(GUIContent.none, mgr.CurGroup.m_bornPos, GUILayout.Height(18));
            if (GUILayout.Button("同步到跟随者", GUILayout.Width(100)))
            {
                mgr.CurGroup.m_bornPos = CameraMgr.instance.GetFollowPos();
            }
        }

        
    }

    void DrawTriggers()
    {
        CameraTriggerMgr mgr = CameraTriggerMgr.instance;
        CameraTriggerGroup group = mgr.CurGroup;
        using (AutoBeginScrollView a = new AutoBeginScrollView(m_scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false)))//, GUILayout.ExpandHeight(true)
        {
            m_scroll = a.Scroll;
            DrawCameraInfo(mgr.CurGroup.m_defaultInfo, "默认镜头", true, true);

            foreach (CameraTrigger t in group.Triggers)
            {
                if (DrawTrigger(t))
                {
                    group.RemoveTrigger(t);
                    return;
                }
            }
        }
    }

    //返回值表示是不是删除了这个触发器
    bool DrawTrigger(CameraTrigger t)
    {
        Color tmp1 = GUI.color;
        EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(t.isExpand, "", t.name + "_cameraTrigger", EditorStyleEx.BoxStyle);//
        Color tmp2 = GUI.color;//BeginFadeArea 需要
        GUI.color = tmp1;//BeginFadeArea 需要
        using (new AutoBeginHorizontal())
        {
            t.gameObject.name = EditorGUILayout.TextField(t.gameObject.name, EditorGUILayoutEx.defaultLabelStyle, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            if (GUILayout.Button("", EditorGUILayoutEx.defaultLabelStyle))
                t.isExpand = !t.isExpand;

            if (GUILayout.Button(EditorGUIUtility.IconContent(CameraMgr.instance.m_curCameraInfo == t.m_info ? "preAudioLoopOn" : "preAudioLoopOff"), EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                CameraMgr.instance.Add(t.m_info, false, CameraInfo.Camera_Editor_Priority);//优先级要提高点
                return false;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                return true;
            }
        }
        
        
        if (area.Show()) 
        {
            GUI.color = tmp2;//BeginFadeArea 需要
            
            Transform tran = t.transform;
            EditorGUILayout.ObjectField("触发体", t.GetComponent<BoxCollider>(), typeof(BoxCollider), false);

            using (new AutoBeginHorizontal())
            {
                EditorGUILayout.PrefixLabel("触发体位置");
                tran.position = EditorGUILayout.Vector3Field(GUIContent.none, tran.position, GUILayout.Height(18));
                if (GUILayout.Button("同步到跟随者", GUILayout.Width(100)))
                {
                    tran.position = CameraMgr.instance.GetFollowPos();
                }
            }

            using (new AutoBeginHorizontal())
            {
                EditorGUILayout.PrefixLabel("触发体大小");
                tran.localScale = EditorGUILayout.Vector3Field(GUIContent.none, tran.localScale, GUILayout.Height(18));
            }

            float y = EditorGUILayout.FloatField("触发体旋转", tran.eulerAngles.y);
            tran.eulerAngles = new Vector3(tran.eulerAngles.x, y, tran.eulerAngles.z);

            //镜头信息
            DrawCameraInfo(t.m_info, t.gameObject.name, false, true);
        }
        EditorGUILayoutEx.instance.EndFadeArea();
        return false;
    }

    public static void DrawCameraInfo(CameraInfo info,string name="",bool isDefault=false,bool sync = false)
    {
        EditorGUILayoutEx.FadeArea area;
        Color tmp1 = GUI.color;
        Color tmp2;
        if (isDefault)
        {
            area = EditorGUILayoutEx.instance.BeginFadeArea(info.isExpand, "", name + "_camera", EditorStyleEx.BoxStyle);//
            tmp2 = GUI.color;//BeginFadeArea 需要
            GUI.color = tmp1;//BeginFadeArea 需要
            using (new AutoBeginHorizontal())
            {
                if (GUILayout.Button(name, EditorGUILayoutEx.defaultLabelStyle))
                    info.isExpand = !info.isExpand;

                if (isDefault && GUILayout.Button(EditorGUIUtility.IconContent(CameraMgr.instance.m_curCameraInfo == info ? "preAudioLoopOn" : "preAudioLoopOff"), EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    CameraMgr.instance.Add(info, false, CameraInfo.Camera_Editor_Priority);//优先级要提高点
                    return;
                }
            }
        }
        else
        {
            area = EditorGUILayoutEx.instance.BeginFadeArea(info.isExpand, "镜头参数", name + "_camera", EditorStyleEx.BoxStyle);//
            info.isExpand = area.open;
            tmp2 = GUI.color;//BeginFadeArea 需要
            GUI.color = tmp1;//BeginFadeArea 需要
        }
        

        if (area.Show()) //using (AutoEditorToggleGroup tg = new AutoEditorToggleGroup(info.isExpand, "默认镜头"))
        {
            GUI.color = tmp2;//BeginFadeArea 需要
           
            bool needSample = false;
            //1 镜头变化相关参数

            EditorGUI.BeginChangeCheck();
            info.lookType = (CameraInfo.enLookType)EditorGUILayout.Popup("类型", (int)info.lookType, CameraInfo.LookTypeName);
            if(info.NeedShowRefPos){
                using (new AutoBeginHorizontal())
                {
                    EditorGUILayout.LabelField("看的点(白)", info.refPos.ToString());

                    //在某些状态下参考点不能自动计算，这个增加一个同步按钮
                    if (GUILayout.Button("同步到跟随者", GUILayout.Width(100)))
                    {
                        info.refPos = CameraMgr.instance.GetFollowPos();
                    }
                }
            }
            
            using (new AutoBeginHorizontal())
            {
                EditorGUILayout.PrefixLabel(info.NeedShowRefPos ? "偏移(黄)" : "偏移");
                info.offset = EditorGUILayout.Vector3Field(GUIContent.none, info.offset, GUILayout.Height(18));
            }
            info.verticalAngle = EditorGUILayout.Slider("高度角", info.verticalAngle, -90f, 90f);
            info.horizontalAngle = EditorGUILayout.Slider("水平角", info.horizontalAngle, 0f, 360f);
            info.fov = EditorGUILayout.Slider("视野", info.fov, 30f, 90f);
            info.distance = EditorGUILayout.Slider("距离", info.distance, 3f, 50f);

            info.blur = EditorGUILayout.Slider("模糊程度", info.blur, 0f, 5f);
            info.blurDuration = EditorGUILayout.Slider("模糊总时间", info.blurDuration, 0f, 50f);
            info.blurBeginSmooth = EditorGUILayout.Slider("开始模糊时间", info.blurBeginSmooth, 0f, 1f);
            info.blurEndSmooth = EditorGUILayout.Slider("结束模糊时间", info.blurEndSmooth, 0f, 1f);
            using (new AutoBeginHorizontal())
            {
                EditorGUILayout.PrefixLabel("模糊偏移");
                info.blurOffset = EditorGUILayout.Vector3Field(GUIContent.none, info.blurOffset, GUILayout.Height(18));

            }


            //跟随对象和敌人之间的特有参数
            if (info.lookType == CameraInfo.enLookType.betweenTwo)
            {
                using (new AutoBeginHorizontal())
                {
                    info.useBetweenTwoLimit = EditorGUILayout.Toggle("两者距离限制", info.useBetweenTwoLimit, GUILayout.ExpandWidth(false));
                    if (info.useBetweenTwoLimit)
                    {
                        info.betweenTwoLimit = EditorGUILayout.Slider(GUIContent.none, info.betweenTwoLimit, 0f, 30f);
                    }
                }

            }
            //盯着的特有参数
            if (info.lookType == CameraInfo.enLookType.stillLook)
            {
                using (new AutoBeginHorizontal())
                {
                    info.useStilllookLimit = EditorGUILayout.Toggle("盯着最远距离", info.useStilllookLimit, GUILayout.ExpandWidth(false));
                    if (info.useStilllookLimit)
                    {
                        info.stillLookLimit = EditorGUILayout.Slider(GUIContent.none, info.stillLookLimit, 0f, 30f);
                    }
                }
            }
            //镜头轨道
            if (info.lookType == CameraInfo.enLookType.path)
            {
                info.cameraPath = (CameraPath)EditorGUILayout.ObjectField("轨道", info.cameraPath, typeof(CameraPath), true);
                info.pathLag = EditorGUILayout.Slider("轨道镜头偏移", info.pathLag, -10, 10);
            }

            //跟随对象盯住目标
            if (info.lookType == CameraInfo.enLookType.followBehind)
            {
                info.targetId = EditorGUILayout.TextField("目标的角色id", info.targetId);
                info.bone = EditorGUILayout.TextField("骨骼路径", info.bone);
                
                using (new AutoBeginHorizontal())
                {
                    EditorGUILayout.PrefixLabel("偏移");
                    info.bornOffset = EditorGUILayout.Vector3Field(GUIContent.none, info.bornOffset, GUILayout.Height(18));
                }
            }

            needSample = EditorGUI.EndChangeCheck();//改变的话要sample下

            //2 渐变和跟随控制相关
            if (!isDefault)
                info.durationType = (CameraInfo.enDurationType)EditorGUILayout.Popup("置顶时渐变策略", (int)info.durationType, CameraInfo.DuratioTypeName);
            info.isDurationInvalid = EditorGUILayout.Toggle("不渐变", info.isDurationInvalid);
            if (!info.isDurationInvalid)
            {
                info.durationSmooth = EditorGUILayout.Slider("渐变时间", info.durationSmooth, 0f, 10f);
                info.animationCurve = EditorGUILayout.CurveField("渐变曲线", info.animationCurve, GUILayout.Width(300f), GUILayout.Height(30f));
            }

            if (!isDefault)
            {
                info.isOverAfterDuration = EditorGUILayout.Toggle("渐变完结束", info.isOverAfterDuration);
                info.duration = EditorGUILayout.FloatField("结束时间", info.duration);
                using (new AutoBeginHorizontal())
                {
                    info.useOverDuration = EditorGUILayout.Toggle("结束渐变时间", info.useOverDuration, GUILayout.ExpandWidth(false));
                    if (info.useOverDuration)
                    {
                        info.overDuationSmooth = EditorGUILayout.Slider(GUIContent.none, info.overDuationSmooth, 0f, 10f);
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            using (new AutoBeginHorizontal())
            {
                info.uselock = EditorGUILayout.Toggle(info.uselock ? "锁定方向(黄线)" : "锁定方向", info.uselock, GUILayout.ExpandWidth(false));
                if (info.uselock)
                {
                    //info.m_lockEuler = EditorGUILayout.Vector3Field(GUIContent.none, info.m_lockEuler, GUILayout.Height(18),GUILayout.ExpandWidth(true));
                    float yEuler = EditorGUILayout.Slider(GUIContent.none, info.lockEuler.y, 0f, 360f);
                    if (yEuler != info.lockEuler.y)
                        info.lockEuler = new Vector3(info.lockEuler.x, yEuler, info.lockEuler.z);

                }
            }
            using(new AutoEditorTipButton("看着的点距离衰减计算出的渐变速度(目的是为了让跟随对象离相机越远相机跟随越快，而离相机越进则跟随越慢)，注意和渐变过程的计算出的渐变速度是取两者的最大值(也就是取慢的那个)"))
                info.useDisSmooth = EditorGUILayout.Toggle( "距离渐变", info.useDisSmooth);
            if (info.useDisSmooth)
            {
                info.disSmooth = EditorGUILayout.FloatField("距离渐变值", info.disSmooth);
                info.disSmoothLimit = EditorGUILayout.FloatField("距离渐变距离", info.disSmoothLimit);
            }
            needSample = EditorGUI.EndChangeCheck() || needSample;//锁定的相关参数变化也要同步下


            //3 sample下
            if (needSample && sync)
            {
                CameraHandle handle = CameraMgr.instance.Set(info, CameraInfo.Camera_Editor_Priority);//优先级要提高点
            }
        }
        EditorGUILayoutEx.instance.EndFadeArea();
    }

   

    

}
