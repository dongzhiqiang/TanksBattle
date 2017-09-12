using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIPanelBase : MonoBehaviour 
{
    public class SortLayerRenderer
    {
        public Renderer renderer;
        int order;//原始的层级
        UIPanelCheckSortLayer check;
        public int Order
        {
            get { return order + check.m_refOrder; }
        }
        public SortLayerRenderer(Renderer renderer, UIPanelCheckSortLayer check) { this.renderer = renderer; this.order = renderer.sortingOrder; this.check = check; }
    }
    public class SortLayerCanvas
    {
        public Canvas canvas;
        int order;//原始的层级
        UIPanelCheckSortLayer check;

        public int Order
        {
            get { return order + check.m_refOrder; }
        }
        public SortLayerCanvas(Canvas canvas, UIPanelCheckSortLayer check) { this.canvas = canvas; this.order = canvas.sortingOrder; this.check =check; }
    }

    #region Fields
    public const int AUTO_ORDER_MIN = 100;
    public const int AUTO_ORDER_MAX = 300;

    public bool m_canTop = true;//加入置顶判断中，像摇杆和聊天提示这种界面不需要加入判断
    public bool m_disableRaycasterIfNotTop = true;//如果不是最顶层的界面，禁用GraphicRaycaster,注意如果界面打开多了，那么这个组件会极大影响性能，勾上这个选项后就不会了
    public bool m_autoOrder = true;//动态层级，如果为false那么层级就是Canvas上的层级了。动态层级从50~200，ui_hight层不支持动态层级
    public bool m_disableCameraIfOpen = true;//如果这个面板是顶层面板，那么把场景相机关了
    public string m_openAniName;  //打开动画
    public string m_closeAniName;  //关闭动画
    public string m_panelName; // UI 界面中文名

    public StateHandle m_btnClose;
    public List<GameObject> m_checkTop = new List<GameObject>();//如果这个界面是顶层界面，那么显示，否则隐藏

    Animator m_ani;

    bool m_isInit = false;
    object m_param;

    bool m_isTop = false;
    bool m_isAniOpening = false;//是否正在播放开启效果
    bool m_isAniClosing = false;//是否正在播放关闭效果

    float m_beginOpenTime = 0;
    float m_beginCloseTime = 0;

    UIPanel uiPanel;
    GraphicRaycaster m_raycaster;//控制能不能接收输入
    Dictionary<int, SortLayerRenderer> m_childRenderers = new Dictionary<int, SortLayerRenderer>();//所有子节点的renderer，用于动态层级支持
    Dictionary<int, SortLayerCanvas> m_childCanvass = new Dictionary<int, SortLayerCanvas>();//所有子节点的Canvas，用于动态层级支持
    #endregion

    #region Properties
    public UIPanel Panel { get { return uiPanel; } }
    public bool IsOpen { get { return m_isInit && gameObject.activeSelf; } }
    /// <summary>
    /// 正在关闭的窗口不算打开的窗口
    /// </summary>
    public bool IsOpenEx { get { return m_isInit && gameObject.activeSelf && !m_isAniClosing; } }
    public bool IsTop
    {
        get { return m_isTop; }
        set
        {
            if (!m_canTop)
            {
                Debuger.LogError("非置顶判断的窗口竟然被修改了置顶值");
                return;
            }
            m_isTop = value;
            if (m_raycaster != null && m_disableRaycasterIfNotTop)
                m_raycaster.enabled = m_isTop;
            CheckTop();

        }
    }
    public bool IsAniPlaying { get { return m_isAniOpening || m_isAniClosing; } }
    public float BeginOpenTime { get { return this.m_beginOpenTime; } }
    public float BeginCloseTime { get { return this.m_beginCloseTime; } }
    #endregion

    #region Mono Frame
    void Update()
    {
        if (m_isInit)
        {
            if (m_isAniOpening)
            {
                AnimatorStateInfo stateInfo = m_ani.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 1f)
                {
                    ClearAni();
                    //检查场景相机的禁用
                    UIMgr.instance.CheckDisableSceneCamera();
                    uiPanel.OnOpenPanelEnd();
                }

            }

            if (m_isAniClosing)
            {
                AnimatorStateInfo stateInfo = m_ani.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 1f)
                {
                    this.gameObject.SetActive(false);

                    ClearAni();
                    uiPanel.OnClosePanelEnd();
                    UIMgr.instance.CheckTopByOtherPanelClose(this);
                    return;
                }

            }

            uiPanel.OnUpdatePanel();
        }
    }
    #endregion
    
    #region Private Methods
    void ClearAni()
    {
        m_isAniOpening = false;
        m_isAniClosing = false;
        if (m_ani != null)
            m_ani.enabled = false;
    }
    #endregion

    #region 动态层级支持
    void CheckSubOrder(UIPanelCheckSortLayer check) { check.Check(); }

    public void AddSubOrder(UIPanelCheckSortLayer check)
    {
        Transform thisTran = this.transform;
        Util.DoAllChild<Transform>(check.transform, (Transform t) =>
        {
            if (t == thisTran)
                return;
            Renderer r = t.GetComponent<Renderer>();
            if (r != null)
            {
                if (!m_childRenderers.ContainsKey(r.GetInstanceID()))
                {
                    m_childRenderers[r.GetInstanceID()] = new SortLayerRenderer(r, check);
                }
                else
                    Debuger.LogError("UIPanelCheckSortLayer嵌套，不支持，请删除嵌套的UIPanelCheckSortLayer:{0}的{1}", this.gameObject.name, r.gameObject.name);
            }
            Canvas c = t.GetComponent<Canvas>();
            if (c != null)
            {
                if (!m_childCanvass.ContainsKey(c.GetInstanceID()))
                {
                    m_childCanvass[c.GetInstanceID()] = new SortLayerCanvas(c, check);
                }
                else
                    Debuger.LogError("UIPanelCheckSortLayer嵌套，不支持，请删除嵌套的UIPanelCheckSortLayer:{0}的{1}",this.gameObject.name,c.gameObject.name);
            }
    
        });
    }



    //获取这个面板需要分配多少个order
    public int GetOrderCount()
    {
        int orderCount = 1;
        foreach (SortLayerRenderer r in m_childRenderers.Values)
        {
            if (r. Order + 1 > orderCount)
                orderCount = r. Order + 1;
        }

        foreach (SortLayerCanvas c in m_childCanvass.Values)
        {
            if (c.Order + 1 > orderCount)
                orderCount = c.Order + 1;
        }
        return orderCount;
    }

    public int GetOrder()
    {
        return this.GetComponent<Canvas>().sortingOrder;
    }

    public void SetOrder(int order)
    {
        if(UIMgr.instance != null && !m_isInit)//如果在游戏中，那么初始化完才能设置层级
        {
            Debuger.LogError("未初始化完就设置层级了:{0}",this.gameObject.name);
            return;
        }
            
        Canvas canvas = this.GetComponent<Canvas>();
        if (canvas.sortingOrder != order)
            canvas.sortingOrder = order;

        int tem;
        foreach (SortLayerRenderer r in m_childRenderers.Values)
        {
            tem = (order + r.Order);
            if (r != null && r.renderer !=null&& r.renderer.sortingOrder != tem)
                r.renderer.sortingOrder = tem;
        }
        foreach (SortLayerCanvas c in m_childCanvass.Values)
        {
            tem = (order + c.Order);
            if (c != null && c.canvas.sortingOrder != tem)
                c.canvas.sortingOrder = tem;
        }
    }

    public void ResetOrder()
    {
        if (UIMgr.instance != null && !m_isInit)//如果在游戏中，那么初始化完才能设置层级
            return;
        SetOrder(this.GetComponent<Canvas>().sortingOrder);
    }
    #endregion

    public void Init()
    {
        uiPanel = this.GetComponent<UIPanel>();

        if (m_isInit)
            Debuger.LogError("重复初始化");

        m_ani = this.GetComponent<Animator>();
        m_raycaster = this.GetComponent<GraphicRaycaster>();
        this.gameObject.SetActive(true);

        //动态层级支持
        Util.DoAllChild<UIPanelCheckSortLayer>(this.transform, CheckSubOrder);

        //通用关闭
        if (m_btnClose != null)
            m_btnClose.AddClick(uiPanel.Close);

        uiPanel.OnInitPanel();
        this.gameObject.SetActive(false);//这里一定要隐藏
        m_isInit = true;
    }
    
    public bool PlayAni(string name, bool checkHierarchy = true)
    {
        if (m_ani == null || string.IsNullOrEmpty(name))
            return false;

        //检查界面结构，如果结构不正确，那么不做处理
        Transform t = this.transform;
        CanvasGroup thisGroup = GetComponent<CanvasGroup>();
        //Transform tranBk = t.Find("bk");
        Transform tranFrame = t.Find("frame");
        if (checkHierarchy && (thisGroup == null || tranFrame == null))
        {
            Debuger.LogError("不符合规定的结构不能播放");
            return false;
        }

        m_ani.enabled = false;
        //m_ani.Rebind(); //
        m_ani.Play(name, -1, 0);
        m_ani.enabled = true;
        m_ani.Update(0);

        return true;
    }

    public void Open(object param, bool immediate = false)
    {
        if (m_isAniOpening)
        {
            Debuger.LogError("播放打开动画中");
            return;
        }
        if (!m_isInit)
        {
            Debuger.LogError("打开的时候没有初始化");
            return;
        }

        gameObject.SetActive(true);
        m_isAniOpening = true;
        m_isAniClosing = false;
        m_beginOpenTime = Time.time;

        //动态层级和置顶
        UIMgr.instance.CheckOpenTopAndAutoOrder(this);

        //打开效果
        if (string.IsNullOrEmpty(m_openAniName) || immediate)
        {
            //这里要重置到初始值
            CanvasGroup thisGroup = GetComponent<CanvasGroup>();
            Transform t = this.transform;
            Transform tranFrame = t.Find("frame");
            if (thisGroup != null)
                thisGroup.alpha = 1f;
            if (tranFrame)
                tranFrame.localScale = Vector3.one;

            ClearAni();
            uiPanel.OnOpenPanel(param);

            //检查场景相机的禁用
            UIMgr.instance.CheckDisableSceneCamera();
            uiPanel.OnOpenPanelEnd();
        }
        else
        {
            PlayAni(m_openAniName);
            uiPanel.OnOpenPanel(param);
        }

#if !ART_DEBUG
        //发个事件吧
        EventMgr.FireAll(MSG.MSG_SYSTEM, MSG_SYSTEM.PANEL_OPEN, uiPanel);
#endif
    }

    public void Fresh()
    {
        if (!uiPanel.IsOpen || IsAniPlaying)
            return;
        uiPanel.OnOpenPanel(m_param);
        uiPanel.OnOpenPanelEnd();
    }
    
    public void Close(bool immediate)
    {
        if (IsAniPlaying && !immediate)
        {
            Debuger.Log("播放关闭动画中");
            return;
        }

        bool isAniCloseBefore = m_isAniClosing;
        if (gameObject.activeSelf == false)
        {
//            Debuger.Error("重复关闭");
            return;
        }

        m_isAniClosing = true;
        m_isAniOpening = false;
        m_beginCloseTime = Time.time;

        bool needAniClose = !immediate && !string.IsNullOrEmpty(m_closeAniName);
        //取消置顶
        UIMgr.instance.CheckCloseTopAndAutoOrder(this, needAniClose);

        //检查场景相机的禁用
        UIMgr.instance.CheckDisableSceneCamera();

        if (!needAniClose)
        {
            this.gameObject.SetActive(false);
            ClearAni();
            if (!isAniCloseBefore)
                uiPanel.OnClosePanel();
            uiPanel.OnClosePanelEnd();
        }
        else//关闭效果
        {
            PlayAni(m_closeAniName);
            PoolMgr.instance.GCCollect();//垃圾回收下
            uiPanel.OnClosePanel();
        }

#if !ART_DEBUG
        //发个事件吧
        EventMgr.FireAll(MSG.MSG_SYSTEM, MSG_SYSTEM.PANEL_CLOSE, uiPanel);
#endif
    }


    public void CheckTop()
    {
        bool b = m_isTop && UIMgr.instance.ClosingTopPanel == null;
        foreach (GameObject go in m_checkTop)
            go.SetActive(b);
    }
}
