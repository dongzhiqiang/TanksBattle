using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITeach : UIPanel
{
    private class DragDemoPlayer
    {
        private const float MOVE_COST = 1.500f;

        private bool playing = false;

        private RectTransform indicator = null;
        private RectTransform rectFrom = null;
        private RectTransform rectTo = null;

        private Vector3 posCur;

        public void Play(RectTransform indicator, RectTransform rectFrom, RectTransform rectTo)
        {
            if (playing)
                Stop();

            //两个对象一样的？
            if (rectFrom == rectTo)
                return;

            this.rectFrom = rectFrom;
            this.rectTo = rectTo;
            this.indicator = indicator;

            Vector3 posFrom = MathUtil.GetCenterInWorldSpace(rectFrom);

            indicator.gameObject.SetActive(true);
            indicator.position = new Vector3(posFrom.x, posFrom.y, indicator.position.z);

            posCur = posFrom;
            playing = true;
        }

        public void Stop()
        {
            if (!playing)
                return;

            playing = false;
            indicator = null;
            rectFrom = null;
            rectTo = null;
        }

        public bool IsPlaying()
        {
            return playing;
        }

        public void Update()
        {
            if (playing)
            {
                Vector3 posFrom = MathUtil.GetCenterInWorldSpace(rectFrom);
                Vector3 posTo = MathUtil.GetCenterInWorldSpace(rectTo);
                Vector3 vecFtoT = posTo - posFrom;
                Vector3 vecCtoT = posTo - posCur;
                float lenFtoT = vecFtoT.magnitude;
                float lenCtoT = vecCtoT.magnitude;
                if (lenCtoT < 0.1f)
                    posCur = posFrom;
                else
                    posCur += vecCtoT.normalized * (Math.Max(lenFtoT, lenCtoT) / MOVE_COST) * Time.unscaledDeltaTime;
                indicator.position = new Vector3(posCur.x, posCur.y, indicator.position.z);
            }
        }
    }

    #region Constant
    public const int DEF_WAIT_UI_TIME = 5000;
    private static Rect EMPTY_RECT = new Rect();
    private const float AUTO_SCROLL_TIME = 0.4f;
    #endregion

    #region Fields
    private List<RectTransform> m_hightlightList = new List<RectTransform>();
    private RectTransform m_curIndicateUI;
    private UIPanel m_curUIPanel;
    private Rect m_lastGlobalRectOfIndicateUI;
    private Rect m_lastlocalRectOfBubbleTxt;
    private float m_blastFxPlayTime;
    private DragDemoPlayer m_dragDemoPlayer = new DragDemoPlayer();
    private bool m_inAutoScroll = false;
    private bool m_scrollFlagReopen = false;

    public RectTransform m_content;
    public int m_highLightLayer;
    public StateHandle m_fullScreenBlockHandle;
    public ImageEx m_fullScreenBlockImage;
    public ImageEx m_fullScreenImage;
    public RectTransform m_windowImageTrans;
    public TextEx m_windowImageTitle;
    public ImageEx m_windowImageImg;
    public RectTransform m_circleImg;
    public RectTransform m_arrowImg;
    public Vector2 m_arrowOffset;
    public RectTransform m_handStaticImg;
    public RectTransform m_handClickImg;
    public RectTransform m_handHoldImg;
    public RectTransform m_handDragImg;
    public RectTransform m_bubble;
    public TextEx m_bubbleTxt;
    public Vector2 m_bubbleOffsetHasArrow;
    public Vector2 m_bubbleOffsetNoArrow;
    public Vector2 m_bubbleOffsetScale;    
    public RectTransform m_blastFx;
    public RectTransform m_circleFlash;
    public RectTransform m_squareFlash;
    public RectTransform m_centerTextBg;
    public TextEx m_centerTextTxt;
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_blastFxPlayTime = m_blastFx.GetComponent<FxDestroy>().m_delay;
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        SimpleClearUI();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {        
        if (m_curIndicateUI != null)
        {
            if (!(m_curUIPanel.IsTop && m_curIndicateUI.gameObject.activeInHierarchy))
            {
                if (m_content.gameObject.activeSelf)
                    m_content.gameObject.SetActive(false);
                return;
            }
            else if (m_curUIPanel.IsTop && m_curIndicateUI.gameObject.activeInHierarchy)
            {
                if (!m_content.gameObject.activeSelf)
                {
                    if (!m_scrollFlagReopen)
                    {
                        m_scrollFlagReopen = true;
                        ShowUIInScrollRect(m_curIndicateUI);
                    }

                    if (m_curUIPanel.IsAniPlaying || m_inAutoScroll)
                        return;

                    m_scrollFlagReopen = false;
                    m_content.gameObject.SetActive(true);                    
                }
            }

            var highlightRect = MathUtil.GetRectInWorldSpace(m_curIndicateUI);
            var bubbleTxtRect = ((RectTransform)m_bubbleTxt.transform).rect;
            if (highlightRect != m_lastGlobalRectOfIndicateUI || bubbleTxtRect != m_lastlocalRectOfBubbleTxt)
            {
                m_lastGlobalRectOfIndicateUI = highlightRect;
                m_lastlocalRectOfBubbleTxt = bubbleTxtRect;

                if (!m_dragDemoPlayer.IsPlaying())
                {
                    if (m_handStaticImg.gameObject.activeSelf)
                        MathUtil.CenterCtrl(m_curIndicateUI, m_handStaticImg, Vector2.zero);

                    if (m_handClickImg.gameObject.activeSelf)
                        MathUtil.CenterCtrl(m_curIndicateUI, m_handClickImg, Vector2.zero);

                    if (m_handHoldImg.gameObject.activeSelf)
                        MathUtil.CenterCtrl(m_curIndicateUI, m_handHoldImg, Vector2.zero);

                    if (m_handDragImg.gameObject.activeSelf)
                        MathUtil.CenterCtrl(m_curIndicateUI, m_handDragImg, Vector2.zero);

                    if (m_arrowImg.gameObject.activeSelf)
                        MathUtil.AlignCtrlToCornerOfSquare((RectTransform)transform, m_curIndicateUI, m_arrowImg, m_arrowOffset, true);
                }

                if (m_circleImg.gameObject.activeSelf)
                    MathUtil.CenterCtrl(m_curIndicateUI, m_circleImg, Vector2.zero);

                if (m_blastFx.gameObject.activeSelf)
                    MathUtil.CenterCtrl(m_curIndicateUI, m_blastFx, Vector2.zero);

                if (m_circleFlash.gameObject.activeSelf)
                {
                    if (m_curIndicateUI.sizeDelta.x == m_curIndicateUI.sizeDelta.y)
                    {
                        m_circleFlash.sizeDelta = m_curIndicateUI.sizeDelta;
                    }                        
                    else
                    {
                        float w = Mathf.Min(m_curIndicateUI.sizeDelta.x, m_curIndicateUI.sizeDelta.y);
                        m_circleFlash.sizeDelta = new Vector2(w, w);
                    }
                    MathUtil.CenterCtrl(m_curIndicateUI, m_circleFlash, Vector2.zero);                    
                }

                if (m_squareFlash.gameObject.activeSelf)
                {
                    m_squareFlash.sizeDelta = m_curIndicateUI.sizeDelta;
                    MathUtil.CenterCtrl(m_curIndicateUI, m_squareFlash, Vector2.zero);
                }

                if (m_bubble.gameObject.activeSelf)
                {
                    var corner = MathUtil.AlignCtrlToCornerOfSquare((RectTransform)transform, m_curIndicateUI, m_bubble, m_arrowImg.gameObject.activeSelf ? m_bubbleOffsetHasArrow : m_bubbleOffsetNoArrow, true);
                    m_bubbleTxt.transform.rotation = Quaternion.identity;

                    switch (corner)
                    {
                        case MathUtil.CornerType.leftTop:
                            m_bubble.sizeDelta = new Vector2(bubbleTxtRect.width, bubbleTxtRect.height);
                            m_bubble.localPosition += new Vector3(-bubbleTxtRect.width * m_bubbleOffsetScale.x, +bubbleTxtRect.height * m_bubbleOffsetScale.y, 0);
                            break;
                        case MathUtil.CornerType.rightBottom:
                            m_bubble.sizeDelta = new Vector2(bubbleTxtRect.width, bubbleTxtRect.height);
                            m_bubble.localPosition += new Vector3(+bubbleTxtRect.width * m_bubbleOffsetScale.x, -bubbleTxtRect.height * m_bubbleOffsetScale.y, 0);
                            break;
                        case MathUtil.CornerType.leftBottom:
                            m_bubble.sizeDelta = new Vector2(bubbleTxtRect.height, bubbleTxtRect.width);
                            m_bubble.localPosition += new Vector3(-bubbleTxtRect.width * m_bubbleOffsetScale.x, -bubbleTxtRect.height * m_bubbleOffsetScale.y, 0);
                            break;
                        case MathUtil.CornerType.rightTop:
                            m_bubble.sizeDelta = new Vector2(bubbleTxtRect.height, bubbleTxtRect.width);
                            m_bubble.localPosition += new Vector3(+bubbleTxtRect.width * m_bubbleOffsetScale.x, +bubbleTxtRect.height * m_bubbleOffsetScale.y, 0);
                            break;
                    }
                }
            }

            m_dragDemoPlayer.Update();
        }
    }
    #endregion

    #region 内用
    private void ShowUIInScrollRect(Transform ui)
    {
        var scroll = Util.GetRoot<ScrollRect>(ui);
        if (scroll != null)
        {
            do
            {
                if (ui.parent == scroll.content)
                    break;
                else if (ui.parent != null)
                {
                    if (ui.parent.GetComponent<StateGroup>() != null)
                        break;

                    ui = ui.parent;
                }                    
                else
                    return;
            } while (true);

            var ui2 = ui as RectTransform;
            if (ui2 != null)
            {
                //如果没有宽高，那等下一帧吧，可能刚显示出来
                //if (ui2.rect.width <= 0 && ui2.rect.height <= 0)
                //    TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(scroll, ui2); });
                //else
                //    UIScrollTips.ScrollPos(scroll, ui2);
                //有些窗口也会延时滚动ScrollRect，这里定长一点，以覆盖窗口的滚动
                m_inAutoScroll = true;
                TimeMgr.instance.AddTimer(AUTO_SCROLL_TIME, () => { m_inAutoScroll = false; UIScrollTips.ScrollPos(scroll, ui2); });
            }
        }
    }

    private void SetHighlightUI(RectTransform ui, bool noRaycaster = false, bool cancelBefore = true)
    {
        if (cancelBefore)
            CancelHighlightUI();

        if (m_hightlightList.Contains(ui))
            return;

        if (ui.GetComponent<Canvas>() != null || ui.GetComponent<GraphicRaycaster>() != null)
        {
            Debuger.LogError("不允许高光有Canvas、GraphicRaycaster的UI");
            return;
        }

        if (m_fullScreenBlockImage.gameObject.activeSelf)
        {
            Canvas canvas = ui.AddComponentIfNoExist<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = m_highLightLayer;

            if (!noRaycaster)
                ui.AddComponentIfNoExist<GraphicRaycaster>();
        }

        m_hightlightList.Add(ui);

        //如果是ScrollRect的content下的子项，滚动到可见区域
        ShowUIInScrollRect(ui);
    }

    private void CancelHighlightUI()
    {
        if (m_hightlightList.Count <= 0)
            return;

        for (var i = 0; i < m_hightlightList.Count; ++i)
        {
            var ui = m_hightlightList[i];

            GraphicRaycaster raycaster = ui.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
                GameObject.DestroyImmediate(raycaster, true);

            Canvas canvas = ui.GetComponent<Canvas>();
            if (canvas != null)
                GameObject.DestroyImmediate(canvas, true);
        }

        m_hightlightList.Clear();
    }

    private void SetIndicateUI(RectTransform ui)
    {
        if (m_curIndicateUI == ui)
            return;

        m_curIndicateUI = ui;
        m_curUIPanel = Util.GetRoot<UIPanel>(m_curIndicateUI);
        m_lastGlobalRectOfIndicateUI = EMPTY_RECT;        
    }

    private void CancelIndicateUI()
    {
        m_curIndicateUI = null;
        m_curUIPanel = null;
    }

    private bool IsCurStepConfig(TeachStepConfig stepCfg)
    {
        TeachMgr teachMgr = TeachMgr.instance;
        var curTeachCfg = teachMgr.CurTeachConfig;
        var curPlayIndex = teachMgr.CurPlayIndex;
        if (!teachMgr.PlayNow || curTeachCfg == null || curPlayIndex < 0 || curPlayIndex >= curTeachCfg.stepList.Count || stepCfg != curTeachCfg.stepList[curPlayIndex])
            return false;
        return true;
    }

    private bool NoRaycasterStep(TeachStepConfig stepCfg)
    {
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.fullScreenImg:
            case TeachUIOpType.windowImg:
            case TeachUIOpType.fullScreenClick:
                return true;
        }
        return false;
    }

    private IEnumerator CoFindUI(TeachStepConfig stepCfg)
    {
        TeachMgr teachMgr = TeachMgr.instance;
        TimeMgr timeMgr = TimeMgr.instance;

        var path = stepCfg.uiOpParam;
        var path2 = stepCfg.uiOpParam2;
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.stateHandle:
            case TeachUIOpType.stateGroup:
                path2 = stepCfg.uiOpParam;  //StateHandle只能是自己，stateGroup的话，path2要等path的对象找到了才会去找Item
                break;
            default:
                path2 = string.IsNullOrEmpty(stepCfg.uiOpParam2) ? stepCfg.uiOpParam : stepCfg.uiOpParam2;
                break;
        }

        var waitTime = stepCfg.stepObj == TeachStepObj.uiOp ? StringUtil.ToInt(stepCfg.stepObjParam, DEF_WAIT_UI_TIME) : DEF_WAIT_UI_TIME;

        GameObject go = null;
        var waitToTime = timeMgr.GetTrueTimestampMS() + waitTime;        
        while ((go = GameObject.Find(path)) == null && IsCurStepConfig(stepCfg))
        {
            var curTime = timeMgr.GetTrueTimestampMS();
            if (curTime >= waitToTime)
            {
                var errMsg = "找不到对象：" + path;
                Debuger.LogError(errMsg);
                teachMgr.ShowNotice(errMsg);
                teachMgr.OnFindUIError(stepCfg);
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (!IsCurStepConfig(stepCfg))
            yield break;

        GameObject go2 = null;
        if (path == path2)
        {
            go2 = go;
        }
        else
        {
            waitToTime = timeMgr.GetTrueTimestampMS() + waitTime;
            while ((go2 = GameObject.Find(path2)) == null && IsCurStepConfig(stepCfg))
            {
                var curTime = timeMgr.GetTrueTimestampMS();
                if (curTime >= waitToTime)
                {
                    var errMsg = "找不到对象：" + path2;
                    Debuger.LogError(errMsg);
                    teachMgr.ShowNotice(errMsg);
                    teachMgr.OnFindUIError(stepCfg);
                    yield break;
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            if (!IsCurStepConfig(stepCfg))
                yield break;
        }

        var rect = go.transform as RectTransform;
        if (rect == null)
        {
            var errMsg = "对象的Tranform不是RectTransform：" + path;
            Debuger.LogError(errMsg);
            teachMgr.ShowNotice(errMsg);
            teachMgr.OnFindUIError(stepCfg);
            yield break;
        }

        var rect2 = go == go2 ? rect : go2.transform as RectTransform;
        if (rect2 == null)
        {
            var errMsg = "对象的Tranform不是RectTransform：" + path2;
            Debuger.LogError(errMsg);
            teachMgr.ShowNotice(errMsg);
            teachMgr.OnFindUIError(stepCfg);
            yield break;
        }

        var panel = Util.GetRoot<UIPanel>(rect);
        if (panel == null)
        {
            var errMsg = "控件不在UIPanel下：" + path;
            Debuger.LogError(errMsg);
            teachMgr.ShowNotice(errMsg);
            teachMgr.OnFindUIError(stepCfg);
            yield break;
        }

        var panel2 = rect == rect2 ? panel : Util.GetRoot<UIPanel>(rect2);
        if (panel2 == null)
        {
            var errMsg = "控件不在UIPanel下：" + path2;
            Debuger.LogError(errMsg);
            teachMgr.ShowNotice(errMsg);
            teachMgr.OnFindUIError(stepCfg);
            yield break;
        }

        if (panel != panel2)
        {
            var errMsg = "两个控件不能所属不同Panel：" + path + "，" + path2;
            Debuger.LogError(errMsg);
            teachMgr.ShowNotice(errMsg);
            teachMgr.OnFindUIError(stepCfg);
            yield break;
        }

        waitToTime = timeMgr.GetTrueTimestampMS() + waitTime;
        while (UIMgr.instance.TopPanel != panel && IsCurStepConfig(stepCfg))
        {
            var curTime = timeMgr.GetTrueTimestampMS();
            if (curTime >= waitToTime)
            {
                var errMsg = "UIPanel不在最上层：" + path;
                Debuger.LogError(errMsg);
                teachMgr.ShowNotice(errMsg);
                teachMgr.OnFindUIError(stepCfg);
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (!IsCurStepConfig(stepCfg))
            yield break;

        waitToTime = timeMgr.GetTrueTimestampMS() + waitTime;
        while (!go.activeInHierarchy && IsCurStepConfig(stepCfg))
        {
            var curTime = timeMgr.GetTrueTimestampMS();
            if (curTime >= waitToTime)
            {
                var errMsg = "对象还未显示出来：" + path;
                Debuger.LogError(errMsg);
                teachMgr.ShowNotice(errMsg);
                teachMgr.OnFindUIError(stepCfg);
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
        if (!IsCurStepConfig(stepCfg))
            yield break;

        if (go != go2)
        {
            waitToTime = timeMgr.GetTrueTimestampMS() + waitTime;
            while (!go2.activeInHierarchy && IsCurStepConfig(stepCfg))
            {
                var curTime = timeMgr.GetTrueTimestampMS();
                if (curTime >= waitToTime)
                {
                    var errMsg = "对象还未显示出来：" + path2;
                    Debuger.LogError(errMsg);
                    teachMgr.ShowNotice(errMsg);
                    teachMgr.OnFindUIError(stepCfg);
                    yield break;
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            if (!IsCurStepConfig(stepCfg))
                yield break;
        }

        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.uiDragDemo:
                {
                    SetHighlightUI(rect, NoRaycasterStep(stepCfg));
                    SetHighlightUI(rect2, NoRaycasterStep(stepCfg), false);
                    SetIndicateUI(rect);
                }
                break;
            case TeachUIOpType.stateGroup:
                {
                    var stateGroup = rect.GetComponent<StateGroup>();
                    if (stateGroup == null)
                    {
                        var errMsg = "对象不包括StateGroup组件：" + path;
                        Debuger.LogError(errMsg);
                        teachMgr.ShowNotice(errMsg);
                        teachMgr.OnFindUIError(stepCfg);
                        yield break;
                    }

                    var selIdx = StringUtil.ToInt(stepCfg.uiOpParam2);
                    selIdx = selIdx >= 0 ? selIdx : stateGroup.Count + selIdx;
                    var selItem = stateGroup.Get(selIdx);
                    if (selItem == null)
                    {
                        var errMsg = "StateGroup不包含下标为" + selIdx +  "的Item：" + path;
                        Debuger.LogError(errMsg);
                        teachMgr.ShowNotice(errMsg);
                        teachMgr.OnFindUIError(stepCfg);
                        yield break;
                    }
                    rect2 = selItem.transform as RectTransform;

                    SetHighlightUI(rect, NoRaycasterStep(stepCfg));
                    SetIndicateUI(rect2);
                }
                break;
            default:
                {
                    SetHighlightUI(rect, NoRaycasterStep(stepCfg));
                    SetIndicateUI(rect2);
                }
                break;
        }
    }

    private void TryShowBackBlock(TeachStepConfig stepCfg)
    {
        var needBlockOp = false;
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.fullScreenImg:
            case TeachUIOpType.windowImg:
            case TeachUIOpType.fullScreenClick:
                needBlockOp = true;
                break;
        }

        if (stepCfg.force || needBlockOp)
        {
            var clr = m_fullScreenBlockImage.color;
            m_fullScreenBlockImage.color = new Color(clr.r, clr.g, clr.b, stepCfg.maskAlpha / 255.0f);
            m_fullScreenBlockImage.gameObject.SetActive(true);
        }
    }

    private IEnumerator CoPlayUIStep(TeachStepConfig stepCfg, RectTransform rectDirect = null, RectTransform rectDirect2 = null)
    {
        TryShowBackBlock(stepCfg);

        if (stepCfg.uiOpType == TeachUIOpType.none)
            yield break;

        if (rectDirect != null)
        {
            switch (stepCfg.uiOpType)
            {
                case TeachUIOpType.uiDragDemo:
                    {
                        SetHighlightUI(rectDirect, NoRaycasterStep(stepCfg));
                        SetHighlightUI(rectDirect2 == null ? rectDirect : rectDirect2, NoRaycasterStep(stepCfg), false);
                        SetIndicateUI(rectDirect);
                    }
                    break;
                default:
                    {
                        SetHighlightUI(rectDirect, NoRaycasterStep(stepCfg));
                        SetIndicateUI(rectDirect2 == null ? rectDirect : rectDirect2);
                    }
                    break;
            }
        }
        else
        {
            yield return UIMgr.instance.StartCoroutine(CoFindUI(stepCfg));
        }

        if (m_hightlightList == null || m_curIndicateUI == null)
            yield break;

        while (m_curUIPanel.IsAniPlaying || m_inAutoScroll)
            yield return new WaitForSeconds(0.1f);

        RectTransform rectCircle = null;
        switch (stepCfg.circleType)
        {
            case TeachCircleType.roundWave:
                rectCircle = m_circleImg;
                break;
            case TeachCircleType.blastFx:
                rectCircle = m_blastFx;
                break;
            case TeachCircleType.circleFlash:
                rectCircle = m_circleFlash;
                break;
            case TeachCircleType.squareFlash:
                rectCircle = m_squareFlash;
                break;
            case TeachCircleType.blastFxClone:
                {
                    rectCircle = GameObject.Instantiate(m_blastFx);
                    rectCircle.SetParent(m_curIndicateUI);
                    rectCircle.localPosition = Vector3.zero;
                    rectCircle.gameObject.SetActive(true);
                    GameObject.Destroy(rectCircle.gameObject, m_blastFxPlayTime);
                }                
                break;
        }
        if (rectCircle != null)
            rectCircle.gameObject.SetActive(true);

        RectTransform rectArrow = null;
        switch (stepCfg.arrowType)
        {
            case TeachArrowType.arrow:
                rectArrow = m_arrowImg;
                break;
            case TeachArrowType.hand:
                {
                    switch (stepCfg.uiOpType)
                    {
                        case TeachUIOpType.uiClick:
                        case TeachUIOpType.uiPtrDown:
                        case TeachUIOpType.uiPtrUp:
                        case TeachUIOpType.stateHandle:
                        case TeachUIOpType.stateGroup:
                        case TeachUIOpType.uiPanelOpenTop:
                        case TeachUIOpType.uiPanelClose:
                            rectArrow = m_handClickImg;
                            break;
                        case TeachUIOpType.uiHold:
                            rectArrow = m_handHoldImg;
                            break;
                        case TeachUIOpType.uiDragBegin:
                            rectArrow = m_handDragImg;
                            break;
                        default:
                            rectArrow = m_handStaticImg;
                            break;
                    }
                }
                break;
        }
        if (rectArrow != null)
            rectArrow.gameObject.SetActive(true);

        //为了重新计算指示UI的位置
        m_lastGlobalRectOfIndicateUI = EMPTY_RECT;

        if (!string.IsNullOrEmpty(stepCfg.tipMsg))
        {
            m_bubble.gameObject.SetActive(true);
            m_bubbleTxt.text = stepCfg.tipMsg;
            m_lastlocalRectOfBubbleTxt = EMPTY_RECT;
        }

        if (rectArrow != null && m_hightlightList.Count >= 2)
        {
            m_dragDemoPlayer.Play(rectArrow, m_hightlightList[0], m_hightlightList[1]);
        }

        //本帧校正一下位置
        OnUpdatePanel();
    }
    #endregion

    #region 接口
    public void AddFullScreenClick(Action cb)
    {
        m_fullScreenBlockHandle.AddClick(cb);
    }

    public void PlayUIStep(TeachStepConfig stepCfg, RectTransform rectDirect = null, RectTransform rectDirect2 = null)
    {
        UIMgr.instance.StartCoroutine(CoPlayUIStep(stepCfg, rectDirect, rectDirect2));
    }

    public void PlayShowImg(TeachStepConfig stepCfg)
    {
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.fullScreenImg:
                {
                    m_fullScreenImage.Set(stepCfg.uiOpParam);
                    AspectRatioFitterEx fitter = m_fullScreenImage.GetComponent<AspectRatioFitterEx>();
                    if (fitter != null)
                        fitter.aspectMode = stepCfg.uiOpParam2 == "inMode" ? AspectRatioFitterEx.AspectMode.FitInParent : AspectRatioFitterEx.AspectMode.EnvelopeParent;
                    m_fullScreenImage.gameObject.SetActive(true);
                    TryShowBackBlock(stepCfg);
                }
                break;
            case TeachUIOpType.windowImg:
                {
                    m_windowImageImg.Set(stepCfg.uiOpParam);
                    m_windowImageTitle.text = stepCfg.uiOpParam2;
                    m_windowImageTrans.gameObject.SetActive(true);
                    TryShowBackBlock(stepCfg);
                }
                break;
        }
    }
    
    public void ShowUINode(GameObject go)
    {
        if (go != null)
        {
            var uiPanel = go.GetComponent<UIPanel>();
            if (uiPanel == null)
            {
                if (!go.activeSelf)
                    go.SetActive(true);
            }
            else
            {
                if (uiPanel == UIMgr.instance.Get<UIMainCity>())
                {
                    ClearAndShowMainCityUI();
                }
                else if (!uiPanel.IsOpenEx)
                {
                    uiPanel.Open(null);
                }
                else if (!uiPanel.IsTop)
                {
                    uiPanel.Close(true);
                    uiPanel.Open(null, true);
                }
            }
        }
    }

    public void HideUINode(GameObject go)
    {
        if (go != null)
        {
            var uiPanel = go.GetComponent<UIPanel>();
            if (uiPanel == null)
            {
                if (go.activeSelf)
                    go.SetActive(false);
            }
            else
            {
                if (uiPanel.IsOpenEx)
                    uiPanel.Close(false);
            }
        }
    }

    public void ShowHideUINode(TeachStepConfig stepCfg, RectTransform rectDirect = null, RectTransform rectDirect2 = null)
    {
        var go1 = rectDirect == null ? GameObject.Find(stepCfg.uiOpParam) : rectDirect.gameObject;
        var go2 = rectDirect2 == null ? GameObject.Find(stepCfg.uiOpParam2) : rectDirect2.gameObject;
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.showUINode:
                {
                    ShowUINode(go1);
                    ShowUINode(go2);
                }
                break;
            case TeachUIOpType.hideUINode:
                {
                    HideUINode(go1);
                    HideUINode(go2);
                }
                break;
        }
    }

    public void ShowCenterText(string str)
    {
        m_centerTextTxt.text = str;
        m_centerTextBg.gameObject.SetActive(true);
    }

    public void ExecuteUIStep(TeachStepConfig stepCfg, RectTransform rectDirect = null)
    {
        switch (stepCfg.uiOpType)
        {
            case TeachUIOpType.uiClick:
            case TeachUIOpType.uiPtrDown:
            case TeachUIOpType.uiPtrUp:
            case TeachUIOpType.uiHold:
            case TeachUIOpType.stateHandle:
                { 
                    var go = rectDirect == null ? GameObject.Find(stepCfg.uiOpParam) : rectDirect.gameObject;
                    if (go == null)
                    {
                        var errMsg = "找不到对象：" + stepCfg.uiOpParam;
                        Debuger.LogError(errMsg);
                        TeachMgr.instance.ShowNotice(errMsg);
                        return;
                    }
                    var stateHandle = go.GetComponent<StateHandle>();
                    if (stateHandle == null)
                    {
                        var errMsg = "无StateHandle组件：" + Util.GetGameObjectPath(go);
                        Debuger.LogError(errMsg);
                        TeachMgr.instance.ShowNotice(errMsg);
                        return;
                    }
                    switch (stepCfg.uiOpType)
                    {
                        case TeachUIOpType.uiClick:
                            stateHandle.ExecuteClick();
                            break;
                        case TeachUIOpType.uiPtrDown:
                            stateHandle.ExecuteDown();
                            break;
                        case TeachUIOpType.uiPtrUp:
                            stateHandle.ExecuteUp();
                            break;
                        case TeachUIOpType.uiHold:
                            stateHandle.ExecuteHold();
                            break;
                        case TeachUIOpType.stateHandle:
                            stateHandle.SetState(StringUtil.ToInt(stepCfg.uiOpParam2));
                            break;
                    }                    
                }
                break;
            case TeachUIOpType.stateGroup:
                {
                    var go = rectDirect == null ? GameObject.Find(stepCfg.uiOpParam) : rectDirect.gameObject;
                    if (go == null)
                    {
                        var errMsg = "找不到对象：" + stepCfg.uiOpParam;
                        Debuger.LogError(errMsg);
                        TeachMgr.instance.ShowNotice(errMsg);
                        return;
                    }
                    var stateGroup = go.GetComponent<StateGroup>();
                    if (stateGroup == null)
                    {
                        var errMsg = "无StateGroup组件：" + Util.GetGameObjectPath(go);
                        Debuger.LogError(errMsg);
                        TeachMgr.instance.ShowNotice(errMsg);
                        return;
                    }
                    var refIdx = StringUtil.ToInt(stepCfg.uiOpParam2);
                    refIdx = refIdx >= 0 ? refIdx : stateGroup.Count + refIdx;
                    stateGroup.SetSel(refIdx);
                }
                break;
            case TeachUIOpType.fullScreenImg:
            case TeachUIOpType.windowImg:
            case TeachUIOpType.fullScreenClick:
                {
                    m_fullScreenBlockHandle.ExecuteClick();
                }
                break;
            case TeachUIOpType.uiPanelOpenTop:
            case TeachUIOpType.uiPanelClose:
                {
                    var go = rectDirect == null ? GameObject.Find(stepCfg.uiOpParam) : rectDirect.gameObject;
                    if (go == null)
                    {
                        var errMsg = "找不到对象：" + stepCfg.uiOpParam;
                        Debuger.LogError(errMsg);
                        TeachMgr.instance.ShowNotice(errMsg);
                        return;
                    }
                    var uiPanel = go.GetComponent<UIPanel>();
                    if (uiPanel == null)
                    {
                        var errMsg = "无UIPanel组件：" + Util.GetGameObjectPath(go);
                        Debuger.LogError(errMsg);
                        TeachMgr.instance.ShowNotice(errMsg);
                        return;
                    }

                    switch (stepCfg.uiOpType)
                    {
                        case TeachUIOpType.uiPanelOpenTop:
                            if (uiPanel == UIMgr.instance.Get<UIMainCity>())
                            {
                                ClearAndShowMainCityUI();
                            }
                            else if (!uiPanel.IsOpenEx)
                            {
                                uiPanel.Open(null);
                            }
                            else if (!uiPanel.IsTop)
                            {
                                uiPanel.Close(true);
                                uiPanel.Open(null, true);
                            }
                            break;
                        case TeachUIOpType.uiPanelClose:
                            if (uiPanel.IsOpenEx)
                                uiPanel.Close(false);
                            break;
                    }
                }
                break;
            case TeachUIOpType.showMainCityUI:
                {
                    ClearAndShowMainCityUI();
                }
                break;
        }
    }

    public static void ClearAndShowMainCityUI()
    {
        if (PlayerStateMachine.Instance.GetCurStateType() == enPlayerState.playGame && LevelMgr.instance.IsMainCity())
        {
            UIMgr.instance.CloseAll();
            UIMgr.instance.Open<UIMainCity>();
        }
    }

    public static StateHandle FindStateHandle(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        var go = GameObject.Find(path);
        if (go == null)
        {
            var errMsg = "找不到对象：" + path;
            Debuger.LogError(errMsg);
            return null;
        }
        var stateHandle = go.GetComponent<StateHandle>();
        if (stateHandle == null)
        {
            var errMsg = "无StateHandle组件：" + path;
            Debuger.LogError(errMsg);
            return null;
        }
        return stateHandle;
    }

    public static StateGroup FindStateGroup(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        var go = GameObject.Find(path);
        if (go == null)
        {
            var errMsg = "找不到对象：" + path;
            Debuger.LogError(errMsg);
            return null;
        }
        var stateGroup = go.GetComponent<StateGroup>();
        if (stateGroup == null)
        {
            var errMsg = "无StateGroup组件：" + path;
            Debuger.LogError(errMsg);
            return null;
        }
        return stateGroup;
    }

    public static UIPanel FindUIPanel(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        var go = GameObject.Find(path);
        if (go == null)
        {
            var errMsg = "找不到对象：" + path;
            Debuger.LogError(errMsg);
            return null;
        }
        var uiPanel = go.GetComponent<UIPanel>();
        if (uiPanel == null)
        {
            var errMsg = "无UIPanel组件：" + path;
            Debuger.LogError(errMsg);
            return null;
        }
        return uiPanel;
    }

    public static RectTransform FindRectTransform(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        var go = GameObject.Find(path);
        if (go == null)
        {
            //var errMsg = "找不到对象：" + path;
            //Debuger.LogError(errMsg);
            return null;
        }
        var rect = go.GetComponent<RectTransform>();
        if (rect == null)
        {
            var errMsg = "无RectTransform组件：" + path;
            Debuger.LogError(errMsg);
            return null;
        }
        return rect;
    }

    public void SimpleClearUI()
    {
        CancelHighlightUI();
        CancelIndicateUI();
        m_fullScreenImage.Set(null);
        m_windowImageImg.Set(null);
        m_windowImageTitle.text = "";
        m_centerTextTxt.text = "";
        m_bubbleTxt.text = "";
        m_dragDemoPlayer.Stop();
        m_inAutoScroll = false;
        m_scrollFlagReopen = false;
    }

    public void ClearTeachUI()
    {
        SimpleClearUI();

        if (!m_content.gameObject.activeSelf)
            m_content.gameObject.SetActive(true);
        if (m_fullScreenBlockImage.gameObject.activeSelf)
            m_fullScreenBlockImage.gameObject.SetActive(false);
        if (m_fullScreenImage.gameObject.activeSelf)
            m_fullScreenImage.gameObject.SetActive(false);
        if (m_windowImageTrans.gameObject.activeSelf)
            m_windowImageTrans.gameObject.SetActive(false);
        if (m_circleImg.gameObject.activeSelf)
            m_circleImg.gameObject.SetActive(false);
        if (m_arrowImg.gameObject.activeSelf)
            m_arrowImg.gameObject.SetActive(false);
        if (m_handStaticImg.gameObject.activeSelf)
            m_handStaticImg.gameObject.SetActive(false);
        if (m_handClickImg.gameObject.activeSelf)
            m_handClickImg.gameObject.SetActive(false);
        if (m_handHoldImg.gameObject.activeSelf)
            m_handHoldImg.gameObject.SetActive(false);
        if (m_handDragImg.gameObject.activeSelf)
            m_handDragImg.gameObject.SetActive(false);
        if (m_bubble.gameObject.activeSelf)
            m_bubble.gameObject.SetActive(false);
        if (m_blastFx.gameObject.activeSelf)
            m_blastFx.gameObject.SetActive(false);
        if (m_circleFlash.gameObject.activeSelf)
            m_circleFlash.gameObject.SetActive(false);
        if (m_squareFlash.gameObject.activeSelf)
            m_squareFlash.gameObject.SetActive(false);
        if (m_centerTextBg.gameObject.activeSelf)
            m_centerTextBg.gameObject.SetActive(false);
    }
    #endregion
}