#region Header
/**
 * 名称：编辑器窗口类模板
 
 * 日期：2016.xx.xx
 * 描述：新建编辑器窗口类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


//xxx编辑器
public class TemplateWindow : EditorWindow
{
    //[MenuItem("Art/场景/镜头编辑器", false, 9)]
    //public static void ShowWindow()
    //{
    //    CameraEditorWindow instance = (CameraEditorWindow)EditorWindow.GetWindow(typeof(CameraEditorWindow));//很遗憾，窗口关闭的时候instance就会为null
    //    instance.titleContent = new GUIContent("镜头编辑器");//不导出中文
    //}

    const string m_windowName = "TemplateWindow";

    //int m_observer;
    int m_id = 0;
    bool m_area1 = true;
    bool m_area2 = true;
    


    #region 监听
    public void OnEnable()
    {
        //m_observer = DrawGL.Add(OnDrawGL);

        //SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    public void OnDisable()
    {
        //if (m_observer != EventMgr.Invalid_Id) { EventMgr.Remove(m_observer); m_observer = EventMgr.Invalid_Id; }

        //SceneView.onSceneGUIDelegate -= OnSceneGUI;
        //SceneView.RepaintAll();
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

    //void OnDrawGL(object obj)
    //{
    //    DrawGL draw = (DrawGL)obj;
    //}

    //void OnSceneGUI(SceneView sceneView)
    //{
    //    Handles.PositionHandle(pos, rot);
    //}
    
    string GetPrefs(string key)
    {
        return string.Format("{0}_{1}_{2}", m_windowName, key, m_id);
    }
    void OnGUI()
    {
        //有时候可能要做些数据收集
        if(!CheckSomeThing())
            return;

        //布局1,如果界面不是用GUILayout布局出来的，而是自己算的，那么写个布局函数和绘制函数,如果需要的话还可以写个消息处理函数
        //Layout();
        //Draw();
        //Handle();

        //布局2,如果界面是用GUILayout布局出来的,那么直接写在这里就可以了
        using (new AutoBeginHorizontal())
        {
            //左边
            using (new AutoBeginVertical("PreferencesSectionBox", GUILayout.Width(250)))
            {
                
                using (new AutoLabelWidth(80))
                {
                    //1 区域一
                    EditorGUILayoutEx.FadeArea area = EditorGUILayoutEx.instance.BeginFadeArea(m_area1, "区域一", GetPrefs("m_area1"), EditorStyleEx.BoxStyle);
                    m_area1 = area.open;
                    if (area.Show())
                    {
                        DrawArea1();
                    }
                    EditorGUILayoutEx.instance.EndFadeArea();

                    //2 区域二
                    area = EditorGUILayoutEx.instance.BeginFadeArea(m_area2, "区域二", GetPrefs("m_area2"), EditorStyleEx.BoxStyle);
                    m_area2 = area.open;
                    if (area.Show())
                    {
                        DrawArea2();
                    }
                    EditorGUILayoutEx.instance.EndFadeArea();
                }
            }

            //右边
            using (new AutoBeginVertical(GUILayout.ExpandWidth(true)))
            {
                DrawRight();
            }
        }

    }

    bool CheckSomeThing()
    {
        return true;
    }

    void DrawArea1()
    {
        //对于简单地方，绘制和消息处理写在一起就可以了
    }

    void DrawArea2()
    {
        //对于简单地方，绘制和消息处理写在一起就可以了
    }

    void DrawRight()
    {
        //对于复杂的界面，布局、绘制和消息处理可能要分开,它们之间用Rect来进行沟通
        LayoutRight();
        InternalDrawRight();
        HandleRight();
    }

    void LayoutRight()
    {

    }

    void InternalDrawRight()
    {

    }

    void HandleRight()
    {

    }
}
