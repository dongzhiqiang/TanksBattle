
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class RolePrefabToolWindow : EditorWindow
{
    public static void ShowWnd(GameObject prefab)
    {
        RolePrefabToolWindow wnd = EditorWindow.GetWindow<RolePrefabToolWindow>();
        wnd.minSize = new Vector2(480f, 200.0f);
        wnd.maxSize = new Vector2(481f, 300.0f);
        wnd.titleContent = new GUIContent("迁移角色模型");
        wnd.autoRepaintOnSceneChange = true;
        wnd.m_newPrefab = prefab;

    }

    public GameObject m_oldPrefab;
    public GameObject m_newPrefab;
    public bool m_canTransfer = true;
    public string m_log = "";


    #region 各类事件监听
    public void Awake()
    {
        

    }

    //更新
    void Update()
    {
        
    }

    void OnEnable()
    {
        //Debuger.Log("当窗口enable时调用一次");
        //初始化
        //GameObject go = Selection.activeGameObject;
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
//        Debuger.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
    }

    void OnProjectChange()
    {
  //      Debuger.Log("当Project视图中的资源发生改变时调用一次");
    }

    void OnInspectorUpdate()
    {
        //Debuger.Log("窗口面板的更新");
        //这里开启窗口的重绘，不然窗口信息不会刷新
        this.Repaint();
    }

    void OnDestroy()
    {
        //Debuger.Log("当窗口关闭时调用");
    }
    void OnSelectionChange()
    {
        //当窗口处于开启状态，并且在Hierarchy视图中选择某游戏对象时调用
        //foreach (Transform t in Selection.transforms)
        //{
        //   //有可能是多选，这里开启一个循环打印选中游戏对象的名称
        //    Debuger.Log("OnSelectionChange" + t.name);
        //}

    }
    #endregion

    //绘制窗口时调用
    void OnGUI()
    {
        //标题 
        using (new AutoFontSize(21, EditorStyles.objectFieldThumb))
            GUILayout.Label("本工具用于将老的角色预制体的设置迁移到新的",EditorStyles.objectFieldThumb);

        //模型拖动
        using(new AutoBeginHorizontal()){
            m_oldPrefab = (GameObject)UnityEditor.EditorGUILayout.ObjectField("", m_oldPrefab, typeof(GameObject), false);
            GUILayout.Label("  =>  ", EditorStyles.centeredGreyMiniLabel);
            m_newPrefab = (GameObject)UnityEditor.EditorGUILayout.ObjectField("", m_newPrefab, typeof(GameObject), false);
        }

        m_canTransfer =true;
        m_log = "";
        //判断能不能复制，不能报个错
        CalcCanTransfer();

        //复制
        using(new AutoEditorDisabledGroup(!m_canTransfer))
        {
            if (GUILayout.Button("转换", GUILayout.Height(40)))
            {
                Transfer();
                ShowNotification(new GUIContent("转换结束"));
            }
        }
        

        //警告
        EditorGUILayout.HelpBox(m_log, MessageType.Warning);
            
    }

    void CheckPrefab(GameObject prefab,string name)
    {
        //有SimpleRole、AniFxMgr、fx_shadow、fx_shadow_dynamic、Title
        if (prefab == null)
        {
            m_canTransfer = false;
            m_log += "请先把" + name + "拖进来\n";
            return;
        }

        SimpleRole simpleRole = prefab.GetComponent<SimpleRole>();
        if(simpleRole==null)
        {
            m_canTransfer = false;
            m_log += name + "找不到SimpleRole组件\n";
            return;
        }

        Transform t = prefab.transform;
        Transform model = t.Find("model");
        if (model == null)
        {
            m_canTransfer = false;
            m_log += name + "找不到model\n";
            return;
        }

        AniFxMgr aniFxMgr = model.GetComponent<AniFxMgr>();
        if (aniFxMgr == null)
        {
            m_canTransfer = false;
            m_log += name + "找不到AniFxMgr\n";
            return;
        }

        Transform title = model.Find("Title");
        if (title == null)
        {
            m_canTransfer = false;
            m_log += name + "找不到Title\n";
            return;
        }

        Transform fx_shadow = t.Find("fx_shadow");
        if (fx_shadow == null)
        {
            m_canTransfer = false;
            m_log += name + "找不到fx_shadow\n";
            return;
        }

        Transform fx_shadow_dynamic = t.Find("fx_shadow_dynamic");
        if (fx_shadow_dynamic == null)
        {
            m_canTransfer = false;
            m_log += name + "找不到fx_shadow_dynamic\n";
            return;
        }

        

    }

    void CalcCanTransfer()
    {
        CheckPrefab(m_oldPrefab,"老预制体");
        if(!m_canTransfer)return;
        CheckPrefab(m_newPrefab, "新预制体"); 
        if(!m_canTransfer)return;

        
        //检查动作有没有丢失
        AniFxMgr aniFxMgr = m_oldPrefab.transform.Find("model").GetComponent<AniFxMgr>();
        Animation animation = m_newPrefab.transform.Find("model").GetComponent<Animation>();
        Transform t =m_newPrefab.transform;
        foreach (AniFxGroup g in aniFxMgr.m_groups)
        {
            if(string.IsNullOrEmpty(g.name))
                continue;

            //动作丢失
            if (animation[g.name] == null)
            {
                m_log += "新预制体找不到动作" + g.name+"\n";
                continue;
            }

            foreach(AniFx fx in g.fxs){
                if(fx.bone == null)
                    continue;

                string path = "";
                //骨骼丢失
                if (!AniFx.FindBone(t, fx.bone, ref path))
                {
                    m_log += "新预制体动作" + g.name + "的骨骼找不到:"+path+"\n";
                    continue;
                }
            }
        }

    }


    void Transfer()
    {
        Transform t = m_oldPrefab.transform;
        Transform tNew = m_newPrefab.transform;

        SimpleRole simpleRole = m_oldPrefab.GetComponent<SimpleRole>();
        SimpleRole simpleRoleNew = m_newPrefab.GetComponent<SimpleRole>();
        Util.Copy(simpleRole, simpleRoleNew, BindingFlags.Public | BindingFlags.Instance);


        Transform model = t.Find("model");
        Transform modelNew = tNew.Find("model");
        //动作特效
        AniFxMgr aniFxMgr = model.GetComponent<AniFxMgr>();
        AniFxMgr aniFxMgrNew = modelNew.GetComponent<AniFxMgr>();
        Animation animation = modelNew.GetComponent<Animation>();
        foreach (AniFxGroup g in aniFxMgr.m_groups)
        {
            if (string.IsNullOrEmpty(g.name))
                continue;

            if (animation[g.name] == null)
                continue;

            AniFxGroup gNew = new AniFxGroup();
            gNew.CopyFrom(tNew, g);
            aniFxMgrNew.m_groups.Add(gNew);
        }

        Transform title = model.Find("Title");
        Transform titleNew = modelNew.Find("Title");
        titleNew.localPosition = title.localPosition;


        Transform fx_shadow = t.Find("fx_shadow/Plane");
        Transform fx_shadowNew = tNew.Find("fx_shadow/Plane");
        titleNew.localPosition = title.localPosition;
        titleNew.localRotation = title.localRotation;
        titleNew.localScale = title.localScale;


        Transform fx_shadow_dynamic = t.Find("fx_shadow_dynamic/Shadow Projector");
        Transform fx_shadow_dynamicNew = tNew.Find("fx_shadow_dynamic/Shadow Projector");
        fx_shadow_dynamicNew.localPosition = fx_shadow_dynamic.localPosition;
        fx_shadow_dynamicNew.localRotation = fx_shadow_dynamic.localRotation;
        fx_shadow_dynamicNew.localScale = fx_shadow_dynamic.localScale;

        Projector p= fx_shadow_dynamic.GetComponent<Projector>();
        Projector pNew = fx_shadow_dynamicNew.GetComponent<Projector>();
        pNew.fieldOfView = p.fieldOfView;
        pNew.nearClipPlane = p.nearClipPlane;
        pNew.farClipPlane = p.farClipPlane;
        pNew.aspectRatio = p.aspectRatio;
        pNew.orthographic = p.orthographic;
        pNew.orthographicSize = p.orthographicSize;

        UnityEditor.EditorUtility.SetDirty(m_newPrefab);
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
    }
}