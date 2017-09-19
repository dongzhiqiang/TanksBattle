using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIMgr : MonoBehaviour {
   

    public static UIMgr instance;
    
    public static System.Type[] PANEL_SHOW_ALWAYS = new System.Type[] { 
        typeof(UIMessage) ,
        typeof(UIFxPanel),
        typeof(UITip),
#if !ART_DEBUG
        
#endif
   };
    public int m_debugOrderTop= 0;
    public string m_debugOrderTopPanel = "";
    public string m_debugTopPanel = "";


    RectTransform m_canvasRoot;
    RectTransform m_canvasRootHight;
    Camera m_camera;
    Camera m_cameraHight;
    Canvas m_canvas;
    Canvas m_canvasHight;
    UIPanelBase m_topPanel;
    UIPanelBase m_closingTopPanel; //正在关闭的顶层窗口。一个顶层窗口关闭的时候，在它下面的窗口变为顶层窗口，它变为正在关闭的顶层窗口
    Dictionary<string, UIPanel> m_panels = new Dictionary<string, UIPanel>();
    Dictionary<string, Sprite> m_sprites = new Dictionary<string, Sprite>();
    

    public Dictionary<string, UIPanel> Panels { get { return m_panels; } }
    
    public UIPanel TopPanel { get { return m_topPanel == null?null:m_topPanel.Panel; } }
    public UIPanel ClosingTopPanel { get { return m_closingTopPanel == null?null:m_closingTopPanel.Panel; } }
    public RectTransform Root { get{return m_canvasRoot;}}
    public RectTransform RootHight { get { return m_canvasRootHight; } }
    public Canvas UICanvas { get { return m_canvas; } }
    public Canvas UICanvasHight { get { return m_canvasHight; } }
    public Camera UICamera { get { return m_camera; } }
    public Camera UICameraHight { get { return m_cameraHight; } }

    //public SortedList<int, UIPanel> AutoOrderPanels
    //{
    //    get{
    //        SortedList<int, UIPanel> l = new SortedList<int,UIPanel>();
    //        foreach(var panel in m_panels.Values){
    //            if(panel.IsOpen && panel.mAutoOrder)
    //                l.Add(panel.m_base.GetOrder(), panel);
    //        }
    //        return l;
    //    }
    //}


	// Use this for initialization
    void Awake()
    {
        instance = this;
        Object.DontDestroyOnLoad(this.gameObject);//过场景保留
        m_topPanel = null;
        m_canvasRoot =transform.Find("CanvasRoot") as RectTransform;
        m_canvasRootHight = transform.Find("CanvasRootHight") as RectTransform;
        m_canvas =m_canvasRoot.GetComponent<Canvas>();
        m_canvasHight = m_canvasRootHight.GetComponent<Canvas>();
        m_camera = transform.Find("UICamera").GetComponent<Camera>();
        m_cameraHight = transform.Find("UICameraHight").GetComponent<Camera>();

        //手机上删除掉多余的输入组件
#if !(UNITY_EDITOR || UNITY_STANDALONE)
        StandaloneInputModule pcInput= transform.Find("EventSystem").GetComponent<StandaloneInputModule>();
        Object.Destroy(pcInput);
#endif     
    }

    void Start () {
        

    }
	
	// Update is called once per frame
	void Update () {
	
	}
    
    //初始化
    public AsyncOpInitUI Init()
    {
        return new AsyncOpInitUI(this);
    }
    
    public void AddSprite(Sprite s)
    {
        m_sprites[s.name] = s;
    }

    public UIPanel AddPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debuger.LogError("有已经被删除的界面：" + prefab);
            return null;
        }

        UIPanel panel = prefab.GetComponent<UIPanel>();
        if (panel == null)
        {
            Debuger.LogError("找不到UIPanel：" + prefab.name);
            return null;
        }

        if (m_panels.ContainsKey(panel.GetType().ToString()))//有就不用再加进去了
        {
            Debuger.LogError("重复加载了界面:{0}", prefab.name);
            return null;
        }
        Debug.Log("prefab " + prefab.name);
        GameObject go = Object.Instantiate(prefab) as GameObject;
        panel = go.GetComponent<UIPanel>();
        go.name = prefab.name;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.SetParent(go.layer == LayerMask.NameToLayer("UIHight") ? m_canvasRootHight : m_canvasRoot,false);
        //Debuger.Log(string.Format("界面:{0} layer：{1} hight：{2}", panel.name, go.layer, LayerMask.NameToLayer("UI_hight")));
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.localScale = Vector3.one;
        rt.anchoredPosition3D = Vector3.zero;
        rt.sizeDelta = Vector2.zero;
        
        m_panels[panel.GetType().ToString()] = panel;
        panel.Init();

        AddSoundFX(panel);//添加UI音效
        return panel;
    }

    //添加UI音效
    void AddSoundFX(UIPanel panel)
    {
#if !ART_DEBUG
        ImageEx[] imgExs = panel.GetComponentsInChildren<ImageEx>(true);
        int len = imgExs.Length;
        SoundUICfg cfg = null;
        StateHandle stateHandle;
        for (int i=0; i<len; i++)
        {
            ImageEx ex = imgExs[i];
            if (ex.sprite == null)
                continue;

            cfg = SoundUICfg.Get(ex.sprite.name);
            if (cfg == null)
                continue;

            stateHandle = ex.GetComponent<StateHandle>();
            if (stateHandle == null)    //StateHandle和ImageEx同级 或者 没有StateHandle
            {
                stateHandle = ex.transform.parent.GetComponent<StateHandle>();
                if (stateHandle == null) //没有StateHandle
                    continue;
            }

            stateHandle.AddComponentIfNoExist<UIPlaySFX>().soundId = cfg.soundId;
        }
#endif
    }

    

    public UIPanel Get(string id)
    {
        UIPanel p;
        if (m_panels.TryGetValue(id, out p) == false)
            return null;
        return p;
    }

    public T Get<T>() where T : UIPanel
    {
        return Get(typeof(T).ToString()) as T;
    }

    public UIPanel Open(string panelType, object param = null, bool immediate = false)
    {
        UIPanel p = Get(panelType);
        if (p == null)
        {
            Debuger.LogError("找不到对应的面板:" + panelType);
            return null;
        }

        p.Open(param, immediate);
        return p;
    }

    public  static Sprite GetSprite(string name)
    {
        if( instance ==null )
            return UIResMgr.instance.GetSprite(name);


        Sprite sprite = null;
        if (instance.m_sprites.TryGetValue(name, out sprite))
            return sprite;

        Debuger.LogError(string.Format("找不到精灵:{0}", name));
        return null;
    }



    public T Open<T>(object param = null, bool immediate = false) where T : UIPanel
    {
        return Open(typeof(T).ToString(),param,immediate) as T;
    }

    public T Fresh<T>(object param = null) where T : UIPanel
    {
        UIPanel p = Get(typeof(T).ToString());
        if (p == null)
        {
            Debuger.LogError("找不到对应的面板:" + typeof(T).ToString());
            return null;
        }

        p.Fresh();
        return p as T;
    }

    public T Close<T>(bool immediate = false) where T : UIPanel
    {
        UIPanel p = Get(typeof(T).ToString());
        if (p == null)
        {
            Debuger.LogError("关闭的时候找不到对应的面板:" + typeof(T).ToString());
            return null;
        }

        p.Close(immediate);

        return p as T;
    }

    public void CloseAll()
    {
        if (m_topPanel != null)
        {
            m_topPanel.IsTop = false;
            m_topPanel = null;
        }
        
        foreach(var panel in m_panels.Values){
            if (System.Array.IndexOf(PANEL_SHOW_ALWAYS, panel.GetType())!=-1)
                continue;
            if(panel.IsOpen )
                panel.Close(true);
        }
        
    }

    //检查是不是置顶，和动态层级
    public void CheckOpenTopAndAutoOrder(UIPanelBase panelBase)
    {
        //TimeCheck check = new TimeCheck();
        
        if (!panelBase.IsOpen)
        {
            Debuger.LogError("打开状态下才能检测动态层级." + panelBase.name);
            return;
        }

        if (panelBase.m_autoOrder && panelBase.gameObject.layer == LayerMask.NameToLayer("UI_hight"))
        {
            Debuger.LogError("UI_hight层的面板不能支持动态层级." + panelBase.name);
            panelBase.m_autoOrder = false;
        }
        
        //动态层级
        if (panelBase.m_autoOrder)
        {
            int order = UIPanelBase.AUTO_ORDER_MIN;
            UIPanelBase pBase;
            foreach (var p in m_panels.Values)
            {
                pBase = p.PanelBase;
                if (!pBase.IsOpen || pBase == panelBase || !pBase.m_autoOrder)
                    continue;

                int orderHight = pBase.GetOrder() + 10;//p.GetOrderCount()
                if(orderHight > order)
                    order= orderHight;
            }
            if (order >= UIPanelBase.AUTO_ORDER_MAX)
                Debuger.LogError("面板的动态层级超过了最大值." + panelBase.name);

            panelBase.SetOrder(order);
        }

        //置顶
        if (panelBase.m_canTop && (m_topPanel == null || m_topPanel.GetOrder() <= panelBase.GetOrder()))
        {
            if(m_topPanel!=null)
            {
                m_topPanel.IsTop = false;
            }
            m_topPanel = panelBase;
            //如果有正在关闭的顶层窗口，现在打开了一个置顶的窗口，那么正在关闭的顶层窗口多半被挡住了，那么就把它设置成null
            m_closingTopPanel = null;
            panelBase.IsTop = true;

            
        }
        DebugerOrderPanel();
        //Debuger.Log(string.Format("重设动态层级的时间：{0} 面板:{1}", check.delayMS,panel.name));
    }

    
    //检查是不是取消置顶
    public void CheckCloseTopAndAutoOrder(UIPanelBase panelBase,bool needAniClose)
    {
        //TimeCheck check = new TimeCheck();
        

        //设置成正在关闭的顶层窗口
        if(panelBase.m_canTop)
        {
            if (panelBase.IsTop && needAniClose)
                m_closingTopPanel = panelBase;
            else
                m_closingTopPanel = null;
        }
        
        //取消置顶
        if (panelBase.IsTop)
        {
            if (m_topPanel != panelBase)
            {
                Debuger.LogError(string.Format("逻辑错误.面板标记为顶层，于管理器不符.面板:{0}  管理器的顶层面板", panelBase.name, m_topPanel == null ? "" : m_topPanel.name));
                m_topPanel.IsTop = false;
            }
            panelBase.IsTop = false;

            //找到一个顶层面板
            UIPanelBase top = null;
            UIPanelBase pBase;
            foreach (var p in m_panels.Values)
            {
                pBase = p.PanelBase;
                if (!pBase.IsOpen || pBase.IsAniPlaying || !pBase.m_canTop)
                    continue;

                if (top == null || top.GetOrder() < pBase.GetOrder())
                    top = pBase;
            }
            m_topPanel = top;
            if (m_topPanel!= null)
            {
                m_topPanel.IsTop = true;   
            }    
        }


        DebugerOrderPanel();
        //Debuger.Log(string.Format("取消置顶操作的时间：{0} 面板:{1}", check.delayMS, panel.name));
    }

    //检查顶层窗口的m_checkTop是不是需要显示，在当前窗口播放完
    public void CheckTopByOtherPanelClose(UIPanelBase panelBase)
    {
        
        if (m_closingTopPanel == panelBase)
            m_closingTopPanel = null;

        if (m_topPanel != null)
            m_topPanel.CheckTop();

        DebugerOrderPanel();
    }

    public void CheckDisableSceneCamera(){
        if(CameraMgr.instance == null || CameraMgr.instance.CurCamera == null)
            return;
        bool enable =true;
        foreach (var p in m_panels.Values)
        {
            if (p.PanelBase.IsOpen && !p.PanelBase.IsAniPlaying && p.PanelBase.m_disableCameraIfOpen)
            {
                enable = false;
                break;
            }
        }
        
        if (enable != CameraMgr.instance.CurCamera.enabled)
        {
            CameraMgr.instance.CurCamera.enabled = enable;
        }
    }

    public void DebugerOrderPanel()
    {
        m_debugOrderTop = -1;
        m_debugOrderTopPanel = "";
        m_debugTopPanel = m_topPanel ==null?"":m_topPanel.name;
        UIPanelBase pBase;
        foreach (var p in m_panels.Values)
        {
            pBase = p.PanelBase;
            if(!p.IsOpen  || !p.PanelBase.m_autoOrder)
                continue;

            if(m_debugOrderTop == -1 || m_debugOrderTop < pBase.GetOrder())
            {
                m_debugOrderTop = pBase.GetOrder();
                m_debugOrderTopPanel = pBase.name;
            }
        }
        
    }
}
