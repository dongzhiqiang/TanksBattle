using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

[DisallowMultipleComponent]
//[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class UIScrollWrap : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    public enum enType
    {
        horizontal,
        vertical,
    }

    public enum enDir
    {
        forward,
        reverse,
    }

    public enum enChildAlign
    {
        leftOrTop,
        center,
        rightOrBottom,
    }

    #region Const
    private const float DELAY_AUTO_CENTER_WAIT = 0.5f;
    private static List<object> EMPTY_OBJECT_LIST = new List<object>();
    #endregion

    #region Fields
    /////////////////////
    public RectTransform m_viewport;
    public RectTransform m_leftOrTopTip;
    public RectTransform m_rightOrBottomTip;

    public enType m_type = enType.vertical;
    public enDir m_dir = enDir.forward;
    public enChildAlign m_childAlign = enChildAlign.center;
    public bool m_dataLoop = false;
    public bool m_childAutoCenter = false;
    public bool m_dockSide = true; //如果内容比显示区域小，是否靠边，还是居中

    public float m_scrollSensitivity = -50;
    public float m_decelerationRate = 0.0135f;
    public float m_bounceValue = 10.0f;
    public float m_rollToIndexSpeed = 40.0f;
    public float m_autoCenterSpeed = 20.0f;
    public float m_fireEventOffset = 50.0f;
    /////////////////////

    private List<object> m_dataItems = new List<object>();
    private int m_startIndex = -1;
    private int m_wantCenterIdx = -1;
    private int m_dataIdxInViewCenter = -1;
    private int m_delayCenterIdx = -1;
    private float m_delayCenterStart = 0;
    private Action<UIScrollWrap, int> m_onCenterDataChanged = null;

    private List<UIScrollWrapItem> m_uiItems = new List<UIScrollWrapItem>();
    private int m_uiItemCount = 0;
    private int m_itemCntInView = 0;
    private bool m_cached = false;
    private bool m_inCaching = false;
    private bool m_dragging = false;
    private float m_velocity = 0.0f;
    private float m_lastMousePos = 0.0f;
    private float m_lastFixedPos = 0.0f;
    private bool m_lastShowStartBorder = true;
    private bool m_lastStartBorderEvtOffset = true;
    private Action<UIScrollWrap> m_onUIItemsReachStart = null;
    private bool m_lastShowEndBorder = true;
    private bool m_lastEndBorderEvtOffset = true;
    private Action<UIScrollWrap> m_onUIItemsReachEnd = null;    

    private enType m_oldType = enType.horizontal;
    private enDir m_oldDir = enDir.reverse;
    private enChildAlign m_oldChildAlign = enChildAlign.leftOrTop;
    private bool m_oldDataLoop = true;
    #endregion

    #region Properties
    public Action<UIScrollWrap, int> OnCenterDataChanged
    {
        get { return m_onCenterDataChanged; }
        set { m_onCenterDataChanged = value; }
    }

    public Action<UIScrollWrap> OnUIItemsReachStart
    {
        get { return m_onUIItemsReachStart; }
        set { m_onUIItemsReachStart = value; }
    }

    public Action<UIScrollWrap> OnUIItemsReachEnd
    {
        get { return m_onUIItemsReachEnd; }
        set { m_onUIItemsReachEnd = value; }
    }
    #endregion

    #region Private Methods
    private void Cache()
    {
        if (m_cached)
            return;
        m_cached = true;

        m_inCaching = true;
        InitUIItems();
        m_inCaching = false;
    }

    private List<UIScrollWrapItem> GetUIScrollWrapItemInChildren()
    {
        var uiItems = new List<UIScrollWrapItem>();
        for (int i = 0; i < m_viewport.childCount; ++i)
        {
            var uiItem = m_viewport.GetChild(i).GetComponent<UIScrollWrapItem>();
            if (uiItem == null)
                continue;
            uiItems.Add(uiItem);
        }
        return uiItems;
    }

    private void InitUIItems()
    {
        if (m_viewport == null)
            m_viewport = this.transform as RectTransform;

        m_uiItems.Clear();
        m_uiItemCount = 0;

        var existsItems = GetUIScrollWrapItemInChildren();
        if (existsItems.Count < 1)
        {
            Debuger.LogError("必须至少有一个UIScrollWrapItem充当模板");
            return;
        }

        var templateRect = existsItems[0].RectTransform;
        m_itemCntInView = Mathf.CeilToInt(m_viewport.sizeDelta[(int)m_type] / templateRect.sizeDelta[(int)m_type]);
        var totalCnt = Math.Max(3, m_itemCntInView + (m_itemCntInView % 2 == 0 ? 1 : 2));
        var exitsCnt = existsItems.Count;

        if (exitsCnt > totalCnt)
        {
            for (var i = exitsCnt - 1; i >= totalCnt; --i)
                DestroyImmediate(existsItems[i].gameObject);
        }
        else if (exitsCnt < totalCnt)
        {
            for (var i = exitsCnt; i < totalCnt; ++i)
            {
                var go = Instantiate(templateRect.gameObject);
                var rf = go.transform;
                rf.SetParent(m_viewport, false);
                rf.localPosition = Vector3.zero;
                rf.localRotation = templateRect.localRotation;
                rf.localScale = templateRect.localScale;
            }
        }

        existsItems = GetUIScrollWrapItemInChildren();
        for (int i = 0; i < existsItems.Count; ++i)
        {
            var item = existsItems[i];
            item.name = "item" + i;
            item.gameObject.SetActive(false);
            item.RectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            item.RectTransform.anchorMax = new Vector2(0.0f, 1.0f);
            m_uiItems.Add(item);
        }
    }

    private void SetUIItemCount(int count)
    {
        Cache();

        var itemCount = Mathf.Min(count, m_uiItems.Count);
        if (m_uiItemCount == itemCount)
            return;

        m_uiItemCount = itemCount;
        for (var i = 0; i < m_uiItems.Count; ++i)
        {
            var go = m_uiItems[i].gameObject;
            if (i < m_uiItemCount)
            {
                if (!go.activeSelf)
                    go.SetActive(true);
            }
            else
            {
                if (go.activeSelf)
                    go.SetActive(false);
            }
        }
    }

    public void RefreshUIItems(bool reset = false, int refUIIdx = -1)
    {
        ArrangeUIItem(0, 0, true, reset, refUIIdx);
    }

    private bool IsDataIndexIn(int startIdx, int uiLen, int dataLen, int idx)
    {
        var endIdx = startIdx + uiLen;
        if (endIdx <= dataLen)
            return idx >= startIdx && idx < endIdx;
        else
            return idx >= startIdx || idx < NormalizeNumber(endIdx, dataLen);
    }

    private bool IsZero(float f)
    {
        return f >= -1e-6 && f <= 1e-6;
    }

    private void ArrangeUIItem(int dataDelta, float posDelta, bool force = false, bool reset = false, int refUIIdx2 = -1)
    {
        Cache();

        //如果是重置，那也会是force
        if (reset)
            force = true;

        if (!force && dataDelta == 0 && IsZero(posDelta))
            return;

        if (m_uiItemCount < 1)
            return;

        var refUIIdx = 0;

        if (refUIIdx2 >= 0)
        {
            refUIIdx = refUIIdx2;
        }
        else
        {
            if (m_uiItemCount > 1)
            {
                //要显示前面的数据
                if (dataDelta < 0)
                {
                    var tempDelta = Mathf.Min(-dataDelta, m_itemCntInView);
                    refUIIdx = tempDelta;
                    //把后面的delta个UI条目往前移
                    for (var i = 0; i < tempDelta; ++i)
                    {
                        var lastItem = m_uiItems[m_uiItemCount - 1];
                        //先删除“最后”元素
                        m_uiItems.RemoveAt(m_uiItemCount - 1);
                        //插入到前面
                        m_uiItems.Insert(0, lastItem);
                    }
                }
                //要显示后面的数据
                else if (dataDelta > 0)
                {
                    var tempDelta = Mathf.Min(dataDelta, m_itemCntInView);
                    refUIIdx = m_uiItemCount - 1 - tempDelta;
                    //把前面的delta个UI条目往后移
                    for (var i = 0; i < tempDelta; ++i)
                    {
                        var firstItem = m_uiItems[0];
                        //先插入“最后”位置
                        m_uiItems.Insert(m_uiItemCount, firstItem);
                        //再删除首位元素
                        m_uiItems.RemoveAt(0);                        
                    }
                }
            }
        }        

        var type2 = m_type == enType.horizontal ? enType.vertical : enType.horizontal;
        var refUIPos = 0.0f;

        var oldStartIdx = m_startIndex;

        var refUIItem = m_uiItems[refUIIdx];
        var refUIItemRt = refUIItem.RectTransform;
        if (!reset && m_dir == m_oldDir && m_type == m_oldType && m_childAlign == m_oldChildAlign && m_dataLoop == m_oldDataLoop)
        {
            if (m_startIndex < 0)
                m_startIndex = 0;
            m_startIndex = m_dataItems.Count < 1 ? 0 : NormalizeNumber(m_startIndex + dataDelta, m_dataItems.Count);
            refUIPos = refUIItemRt.offsetMin[(int)m_type] + posDelta;
        }
        else
        {
            m_oldDir = m_dir;
            m_oldType = m_type;
            m_oldChildAlign = m_childAlign;
            m_oldDataLoop = m_dataLoop;

            m_startIndex = 0;
            refUIIdx = 0;

            refUIItem = m_uiItems[refUIIdx];
            refUIItemRt = refUIItem.RectTransform;
            var firstUIItemRt = m_uiItems[0].RectTransform;

            refUIPos = m_type == enType.horizontal ? (m_dir == enDir.forward ? 0 : m_viewport.sizeDelta.x - firstUIItemRt.sizeDelta.x) : (m_dir == enDir.forward ? -firstUIItemRt.sizeDelta.y : -m_viewport.sizeDelta.y);
            switch (m_childAlign)
            {
                case enChildAlign.leftOrTop:
                    m_lastFixedPos = m_type == enType.horizontal ? -(m_viewport.sizeDelta.y - refUIItemRt.sizeDelta.y) / 2 : (m_viewport.sizeDelta.x - refUIItemRt.sizeDelta.x) / 2;
                    break;
                case enChildAlign.center:
                    m_lastFixedPos = m_type == enType.horizontal ? -m_viewport.sizeDelta.y / 2 : m_viewport.sizeDelta.x / 2;
                    break;
                case enChildAlign.rightOrBottom:
                    m_lastFixedPos = m_type == enType.horizontal ? -(m_viewport.sizeDelta.y + refUIItemRt.sizeDelta.y) / 2 : (m_viewport.sizeDelta.x + refUIItemRt.sizeDelta.x) / 2;
                    break;
            }
        }

        var viewportSize = m_viewport.sizeDelta[(int)m_type];
        var viewportCenterPos = m_type == enType.horizontal ? m_viewport.sizeDelta.x / 2 : -m_viewport.sizeDelta.y / 2;
        var minDistFromViewCenter = float.MaxValue;
        var oldDataIdxInViewCenter = m_dataIdxInViewCenter;
        m_dataIdxInViewCenter = -1;

        var tempRefUIPos = refUIPos;
        for (var i = refUIIdx; i >= 0; --i)
        {
            var uiItem = m_uiItems[i];
            object dataItem = null;
            var dataItemIdx = -1;
            if (m_dataItems.Count > 0)
            {
                dataItemIdx = (m_startIndex + i) % m_dataItems.Count;
                dataItem = m_dataItems[dataItemIdx];
            }
                
            var rectTrans = uiItem.RectTransform;

            if (i != refUIIdx)
            {
                if ((m_type == enType.vertical && m_dir == enDir.reverse) || (m_type == enType.horizontal && m_dir == enDir.forward))
                    tempRefUIPos -= rectTrans.sizeDelta[(int)m_type];
            }

            var factor = (m_type == enType.horizontal ? 1 : -1) * (tempRefUIPos + rectTrans.sizeDelta[(int)m_type] / 2) / viewportSize;
            if (force || !IsDataIndexIn(oldStartIdx, m_uiItemCount, m_dataItems.Count, dataItemIdx))
                uiItem.InitData(dataItem);
            uiItem.SetFactor(factor);

            var newOffMin = new Vector2();
            newOffMin[(int)m_type] = tempRefUIPos;
            switch (m_childAlign)
            {
                case enChildAlign.leftOrTop:
                    newOffMin[(int)type2] = m_type == enType.horizontal ? m_lastFixedPos - rectTrans.sizeDelta[(int)type2] : m_lastFixedPos;
                    break;
                case enChildAlign.center:
                    newOffMin[(int)type2] = m_lastFixedPos - rectTrans.sizeDelta[(int)type2] / 2;
                    break;
                case enChildAlign.rightOrBottom:
                    newOffMin[(int)type2] = m_type == enType.horizontal ? m_lastFixedPos : m_lastFixedPos - rectTrans.sizeDelta[(int)type2];
                    break;
            }
            rectTrans.anchoredPosition = newOffMin + Vector2.Scale(rectTrans.sizeDelta, rectTrans.pivot);

            var uiItemCenterPos = tempRefUIPos + rectTrans.sizeDelta[(int)m_type] / 2;
            var distFromViewCenter = Mathf.Abs(viewportCenterPos - uiItemCenterPos);
            if (distFromViewCenter < minDistFromViewCenter)
            {
                minDistFromViewCenter = distFromViewCenter;
                m_dataIdxInViewCenter = dataItemIdx;
            }  

            if ((m_type == enType.vertical && m_dir == enDir.forward) || (m_type == enType.horizontal && m_dir == enDir.reverse))
                tempRefUIPos += rectTrans.sizeDelta[(int)m_type];
        }

        tempRefUIPos = refUIPos;
        for (var i = refUIIdx; i < m_uiItemCount; ++i)
        {
            var uiItem = m_uiItems[i];
            object dataItem = null;
            var dataItemIdx = -1;
            if (m_dataItems.Count > 0)
            {
                dataItemIdx = (m_startIndex + i) % m_dataItems.Count;
                dataItem = m_dataItems[dataItemIdx];
            }

            var rectTrans = uiItem.RectTransform;

            if (i != refUIIdx)
            {
                if ((m_type == enType.vertical && m_dir == enDir.forward) || (m_type == enType.horizontal && m_dir == enDir.reverse))
                    tempRefUIPos -= rectTrans.sizeDelta[(int)m_type];
            }

            var factor = (m_type == enType.horizontal ? 1 : -1) * (tempRefUIPos + rectTrans.sizeDelta[(int)m_type] / 2) / viewportSize;
            if (force || !IsDataIndexIn(oldStartIdx, m_uiItemCount, m_dataItems.Count, dataItemIdx))
                uiItem.InitData(dataItem);
            uiItem.SetFactor(factor);

            var newOffMin = new Vector2();
            newOffMin[(int)m_type] = tempRefUIPos;
            switch (m_childAlign)
            {
                case enChildAlign.leftOrTop:
                    newOffMin[(int)type2] = m_type == enType.horizontal ? m_lastFixedPos - rectTrans.sizeDelta[(int)type2] : m_lastFixedPos;
                    break;
                case enChildAlign.center:
                    newOffMin[(int)type2] = m_lastFixedPos - rectTrans.sizeDelta[(int)type2] / 2;
                    break;
                case enChildAlign.rightOrBottom:
                    newOffMin[(int)type2] = m_type == enType.horizontal ? m_lastFixedPos : m_lastFixedPos - rectTrans.sizeDelta[(int)type2];
                    break;
            }
            rectTrans.anchoredPosition = newOffMin + Vector2.Scale(rectTrans.sizeDelta, rectTrans.pivot);

            var uiItemCenterPos = tempRefUIPos + rectTrans.sizeDelta[(int)m_type] / 2;
            var distFromViewCenter = Mathf.Abs(viewportCenterPos - uiItemCenterPos);
            if (distFromViewCenter < minDistFromViewCenter)
            {
                minDistFromViewCenter = distFromViewCenter;
                m_dataIdxInViewCenter = dataItemIdx;
            }

            if ((m_type == enType.vertical && m_dir == enDir.reverse) || (m_type == enType.horizontal && m_dir == enDir.forward))
                tempRefUIPos += rectTrans.sizeDelta[(int)m_type];
        }

        if (oldDataIdxInViewCenter != m_dataIdxInViewCenter && m_onCenterDataChanged != null)
            m_onCenterDataChanged(this, m_dataIdxInViewCenter);
    }

    private int NormalizeNumber(int num, int total)
    {
        var temp = num % total;
        return temp >= 0 ? temp : (temp + total) % total;
    }
    #endregion

    #region Mono Frame
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_dragging = true;
            m_lastMousePos = Input.mousePosition[(int)m_type];
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ArrangeUIItem(0, eventData.delta[(int)m_type]);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_dragging = false;
            if (m_childAutoCenter)
                m_wantCenterIdx = m_dataIdxInViewCenter;
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        ArrangeUIItem(0, eventData.scrollDelta.y * m_scrollSensitivity);
        if (m_childAutoCenter)
        {
            m_delayCenterIdx = m_dataIdxInViewCenter;
            m_delayCenterStart = Time.unscaledTime;
        }            
    }

    private void CheckDelayRollTo()
    {
        /////////////////////////执行延时居中////////////////////////
        {
            if (m_delayCenterIdx >= 0)
            {
                if (Time.unscaledTime - m_delayCenterStart > DELAY_AUTO_CENTER_WAIT)
                {
                    m_wantCenterIdx = m_delayCenterIdx;
                    m_delayCenterStart = 0.0f;
                    m_delayCenterIdx = -1;
                }
            }
        }
        /////////////////////////执行延时居中////////////////////////
    }

    private void DoNormalMove(float unscaledDeltaTime)
    {
        var posDelta = 0.0f;
        var dataDelta = 0;

        /////////////////////////拖动时的惯性////////////////////////
        {
            if (m_dragging)
            {
                var curMousePos = Input.mousePosition[(int)m_type];
                var velocity = (curMousePos - m_lastMousePos) / unscaledDeltaTime;
                m_velocity = Mathf.Lerp(m_velocity, velocity, unscaledDeltaTime * 10f);
                m_lastMousePos = curMousePos;
            }
            else if (m_wantCenterIdx < 0)
            {
                if (!MathUtil.IsEqual(m_velocity, 0.0f))
                {
                    m_velocity *= Mathf.Pow(m_decelerationRate, unscaledDeltaTime);
                    posDelta = m_velocity * unscaledDeltaTime;
                }
            }
        }
        /////////////////////////拖动时的惯性////////////////////////

        /////////////////////////根据偏移调整显示的数据////////////////////////
        {
            var viewportSize = m_viewport.sizeDelta[(int)m_type];
            var firstUIItem = m_uiItems[0].RectTransform;
            var lastUIItem = m_uiItems[m_uiItemCount - 1].RectTransform;
            if (m_type == enType.vertical)
            {
                if (m_dir == enDir.forward)
                {
                    var contentSize = firstUIItem.offsetMax.y - lastUIItem.offsetMin.y;
                    var offsetToChange = contentSize / m_uiItemCount;
                    var topPosWhenCenter = (contentSize - viewportSize) / 2;
                    var topPosCurrent = firstUIItem.offsetMax.y;
                    if (topPosWhenCenter - topPosCurrent > offsetToChange / 2)
                    {
                        if (m_dataLoop)
                        {
                            dataDelta = -Mathf.Max(Mathf.FloorToInt((topPosWhenCenter - topPosCurrent) / offsetToChange), 1);
                        }
                        else
                        {
                            if (m_startIndex > 0)
                                dataDelta = -Mathf.Clamp(Mathf.FloorToInt((topPosWhenCenter - topPosCurrent) / offsetToChange), 1, m_startIndex);
                        }
                    }
                    else if (topPosCurrent - topPosWhenCenter > offsetToChange / 2)
                    {
                        if (m_dataLoop)
                        {
                            dataDelta = Mathf.Max(Mathf.CeilToInt((topPosCurrent - topPosWhenCenter) / offsetToChange), 1);
                        }
                        else
                        {
                            var maxIndex = m_dataItems.Count - m_uiItemCount;
                            if (m_startIndex < maxIndex)
                                dataDelta = Mathf.Clamp(Mathf.CeilToInt((topPosCurrent - topPosWhenCenter) / offsetToChange), 1, maxIndex - m_startIndex);
                        }
                    }
                }
                else
                {
                    var contentSize = lastUIItem.offsetMax.y - firstUIItem.offsetMin.y;
                    var offsetToChange = contentSize / m_uiItemCount;
                    var topPosWhenCenter = (contentSize - viewportSize) / 2;
                    var topPosCurrent = lastUIItem.offsetMax.y;
                    if (topPosCurrent - topPosWhenCenter > offsetToChange / 2)
                    {
                        if (m_dataLoop)
                        {
                            dataDelta = -Mathf.Max(Mathf.FloorToInt((topPosCurrent - topPosWhenCenter) / offsetToChange), 1);
                        }
                        else
                        {
                            if (m_startIndex > 0)
                                dataDelta = -Mathf.Clamp(Mathf.FloorToInt((topPosCurrent - topPosWhenCenter) / offsetToChange), 1, m_startIndex);
                        }
                    }
                    else if (topPosWhenCenter - topPosCurrent > offsetToChange / 2)
                    {
                        if (m_dataLoop)
                        {
                            dataDelta = Mathf.Max(Mathf.CeilToInt((topPosWhenCenter - topPosCurrent) / offsetToChange), 1);
                        }
                        else
                        {
                            var maxIndex = m_dataItems.Count - m_uiItemCount;
                            if (m_startIndex < maxIndex)
                                dataDelta = Mathf.Clamp(Mathf.CeilToInt((topPosWhenCenter - topPosCurrent) / offsetToChange), 1, maxIndex - m_startIndex);
                        }
                    }
                }
            }
            else
            {
                if (m_dir == enDir.forward)
                {
                    var contentSize = lastUIItem.offsetMax.x - firstUIItem.offsetMin.x;
                    var offsetToChange = contentSize / m_uiItemCount;
                    var leftPosWhenCenter = (viewportSize - contentSize) / 2;
                    var leftPosCurrent = firstUIItem.offsetMin.x;
                    if (leftPosCurrent - leftPosWhenCenter > offsetToChange / 2)
                    {
                        if (m_dataLoop)
                        {
                            dataDelta = -Mathf.Max(Mathf.FloorToInt((leftPosCurrent - leftPosWhenCenter) / offsetToChange), 1);
                        }
                        else
                        {
                            if (m_startIndex > 0)
                                dataDelta = -Mathf.Clamp(Mathf.FloorToInt((leftPosCurrent - leftPosWhenCenter) / offsetToChange), 1, m_startIndex);
                        }
                    }
                    else if (leftPosWhenCenter - leftPosCurrent > offsetToChange / 2)
                    {
                        if (m_dataLoop)
                        {
                            dataDelta = Mathf.Max(Mathf.CeilToInt((leftPosWhenCenter - leftPosCurrent) / offsetToChange), 1);
                        }
                        else
                        {
                            var maxIndex = m_dataItems.Count - m_uiItemCount;
                            if (m_startIndex < maxIndex)
                                dataDelta = Mathf.Clamp(Mathf.CeilToInt((leftPosWhenCenter - leftPosCurrent) / offsetToChange), 1, maxIndex - m_startIndex);
                        }
                    }
                }
                else
                {
                    var contentSize = firstUIItem.offsetMax.x - lastUIItem.offsetMin.x;
                    var offsetToChange = contentSize / m_uiItemCount;
                    var leftPosWhenCenter = (viewportSize - contentSize) / 2;
                    var leftPosCurrent = lastUIItem.offsetMin.x;
                    if (leftPosWhenCenter - leftPosCurrent > offsetToChange / 2)
                    {
                        if (m_dataLoop)
                        {
                            dataDelta = -Mathf.Max(Mathf.FloorToInt((leftPosWhenCenter - leftPosCurrent) / offsetToChange), 1);
                        }
                        else
                        {
                            if (m_startIndex > 0)
                                dataDelta = -Mathf.Clamp(Mathf.FloorToInt((leftPosWhenCenter - leftPosCurrent) / offsetToChange), 1, m_startIndex);
                        }
                    }
                    else if (leftPosCurrent - leftPosWhenCenter > offsetToChange / 2)
                    {
                        if (m_dataLoop)
                        {
                            dataDelta = Mathf.Max(Mathf.CeilToInt((leftPosCurrent - leftPosWhenCenter) / offsetToChange), 1);
                        }
                        else
                        {
                            var maxIndex = m_dataItems.Count - m_uiItemCount;
                            if (m_startIndex < maxIndex)
                                dataDelta = Mathf.Clamp(Mathf.CeilToInt((leftPosCurrent - leftPosWhenCenter) / offsetToChange), 1, maxIndex - m_startIndex);
                        }
                    }
                }
            }
        }
        /////////////////////////根据偏移调整显示的数据////////////////////////

        ArrangeUIItem(dataDelta, posDelta);
    }

    private void DoRollToIdx(float unscaledDeltaTime)
    {
        var posDelta = 0.0f;

        /////////////////////////执行滚动跳转的任务////////////////////////
        {
            if (!m_dragging && m_wantCenterIdx >= 0)
            {
                var firstUIItem = m_uiItems[0].RectTransform;
                var lastUIItem = m_uiItems[m_uiItemCount - 1].RectTransform;
                var curCenterIdx = GetCenterDataIndex();
                var indexDiff = 0.0f;
                var contentSize = 0.0f;
                var halfItemCntInView = m_itemCntInView / 2.0f;
                var checkIdxCond1 = m_wantCenterIdx < halfItemCntInView;
                var checkIdxCond2 = m_dataItems.Count - 1 - m_wantCenterIdx < halfItemCntInView;
                if (m_type == enType.vertical)
                {
                    if (m_dir == enDir.forward)
                    {
                        contentSize = firstUIItem.offsetMax.y - lastUIItem.offsetMin.y;
                        if (m_dataLoop || (!checkIdxCond1 && !checkIdxCond2))
                            indexDiff = m_wantCenterIdx - curCenterIdx;
                        else if (checkIdxCond1)
                            indexDiff = 0 - m_startIndex;
                        else if (checkIdxCond2)
                            indexDiff = (m_dataItems.Count - 1) - (m_startIndex + m_uiItemCount - 1);
                    }
                    else
                    {
                        contentSize = lastUIItem.offsetMax.y - firstUIItem.offsetMin.y;

                        if (m_dataLoop || (!checkIdxCond1 && !checkIdxCond2))
                            indexDiff = curCenterIdx - m_wantCenterIdx;
                        else if (checkIdxCond1)
                            indexDiff = m_startIndex;
                        else if (checkIdxCond2)
                            indexDiff = (m_startIndex + m_uiItemCount - 1) - (m_dataItems.Count - 1);
                    }
                }
                else
                {
                    if (m_dir == enDir.forward)
                    {
                        contentSize = lastUIItem.offsetMax.x - firstUIItem.offsetMin.x;

                        if (m_dataLoop || (!checkIdxCond1 && !checkIdxCond2))
                            indexDiff = curCenterIdx - m_wantCenterIdx;
                        else if (checkIdxCond1)
                            indexDiff = m_startIndex;
                        else if (checkIdxCond2)
                            indexDiff = (m_startIndex + m_uiItemCount - 1) - (m_dataItems.Count - 1);
                    }
                    else
                    {
                        contentSize = firstUIItem.offsetMax.x - lastUIItem.offsetMin.x;

                        if (m_dataLoop || (!checkIdxCond1 && !checkIdxCond2))
                            indexDiff = m_wantCenterIdx - curCenterIdx;
                        else if (checkIdxCond1)
                            indexDiff = 0 - m_startIndex;
                        else if (checkIdxCond2)
                            indexDiff = (m_dataItems.Count - 1) - (m_startIndex + m_uiItemCount - 1);
                    }
                }

                if (indexDiff != 0)
                {
                    var offsetToChange = contentSize / m_uiItemCount;
                    //循环的话，看看反向走，走走捷径
                    if (m_dataLoop && Math.Abs(indexDiff) > m_dataItems.Count / 2)
                        indexDiff = indexDiff > 0 ? indexDiff - m_dataItems.Count : m_dataItems.Count + indexDiff;
                    posDelta = Math.Sign(indexDiff) * Math.Min(Math.Abs(indexDiff) - 0.5f, m_itemCntInView) * offsetToChange * Mathf.Min(unscaledDeltaTime * m_rollToIndexSpeed, 1.0f);
                    //Debuger.Log(indexDiff + "," + dataDelta + "," + posDelta + "," + unscaledDeltaTime);
                }
                else
                {
                    var viewportSize = m_viewport.sizeDelta[(int)m_type];
                    if (viewportSize < contentSize)
                    {
                        var diff = 0.0f;
                        var refUIItem = (RectTransform)null;

                        if (m_dataLoop || (!checkIdxCond1 && !checkIdxCond2))
                        {
                            var refUIIdx = NormalizeNumber(m_wantCenterIdx - m_startIndex, m_dataItems.Count);
                            refUIItem = m_uiItems[refUIIdx].RectTransform;

                            if (m_type == enType.vertical)
                            {
                                var refUIPos = refUIItem.offsetMin.y + refUIItem.sizeDelta.y / 2;
                                var targetPos = -m_viewport.sizeDelta.y / 2;
                                diff = targetPos - refUIPos;
                            }
                            else
                            {
                                var refUIPos = refUIItem.offsetMin.x + refUIItem.sizeDelta.x / 2;
                                var targetPos = m_viewport.sizeDelta.x / 2;
                                diff = targetPos - refUIPos;
                            }
                        }
                        else if (checkIdxCond1)
                        {
                            var refUIIdx = 0;
                            refUIItem = m_uiItems[refUIIdx].RectTransform;

                            if (m_type == enType.vertical)
                            {
                                var refUIPos = refUIItem.offsetMin.y;

                                if (m_dir == enDir.forward)
                                {
                                    var targetPos = -refUIItem.sizeDelta.y;
                                    diff = targetPos - refUIPos;
                                }
                                else
                                {
                                    var targetPos = -m_viewport.sizeDelta.y;
                                    diff = targetPos - refUIPos;
                                }
                            }
                            else
                            {
                                var refUIPos = refUIItem.offsetMin.x;

                                if (m_dir == enDir.forward)
                                {
                                    var targetPos = 0;
                                    diff = targetPos - refUIPos;
                                }
                                else
                                {
                                    var targetPos = m_viewport.sizeDelta.x - refUIItem.sizeDelta.x;
                                    diff = targetPos - refUIPos;
                                }
                            }
                        }
                        else if (checkIdxCond2)
                        {
                            var refUIIdx = m_uiItemCount - 1;
                            refUIItem = m_uiItems[refUIIdx].RectTransform;

                            if (m_type == enType.vertical)
                            {
                                var refUIPos = refUIItem.offsetMin.y;

                                if (m_dir == enDir.forward)
                                {
                                    var targetPos = -m_viewport.sizeDelta.y;
                                    diff = targetPos - refUIPos;
                                }
                                else
                                {
                                    var targetPos = -refUIItem.sizeDelta.y;
                                    diff = targetPos - refUIPos;
                                }
                            }
                            else
                            {
                                var refUIPos = refUIItem.offsetMin.x;

                                if (m_dir == enDir.forward)
                                {
                                    var targetPos = m_viewport.sizeDelta.x - refUIItem.sizeDelta.x;
                                    diff = targetPos - refUIPos;
                                }
                                else
                                {
                                    var targetPos = 0;
                                    diff = targetPos - refUIPos;
                                }
                            }
                        }

                        var epsilon = contentSize / m_uiItemCount / 1000;
                        if (Math.Abs(diff) < epsilon)
                        {
                            m_velocity = 0.0f;
                            m_wantCenterIdx = -1;
                        }
                        else
                            posDelta = diff * Mathf.Min(unscaledDeltaTime * m_autoCenterSpeed, 1.0f);
                    }
                    else
                    {
                        m_velocity = 0.0f;
                        m_wantCenterIdx = -1;
                    }
                }
            }
        }
        /////////////////////////执行滚动跳转的任务////////////////////////

        ArrangeUIItem(0, posDelta);
    }

    private void CheckReachStartOrEndCallback(float startOffset, float endOffset)
    {
        if (m_startIndex == 0)
        {
            var showBorder = false;
            var evtOffset = false;

            if (startOffset >= m_fireEventOffset)
            {
                evtOffset = true;
                showBorder = true;
            }
            else if (startOffset >= 0)
                showBorder = true;

            if ((showBorder && !m_lastShowStartBorder || evtOffset && !m_lastStartBorderEvtOffset) && m_onUIItemsReachStart != null)
                m_onUIItemsReachStart(this);
            m_lastShowStartBorder = showBorder;
            m_lastStartBorderEvtOffset = evtOffset;
        }
        else
        {
            m_lastShowStartBorder = false;
            m_lastStartBorderEvtOffset = false;
        }

        if (m_startIndex + m_uiItemCount == m_dataItems.Count)
        {
            var showBorder = false;
            var evtOffset = false;

            if (endOffset >= m_fireEventOffset)
            {
                evtOffset = true;
                showBorder = true;
            }
            else if (endOffset >= 0)
                showBorder = true;

            if ((showBorder && !m_lastShowEndBorder || evtOffset && !m_lastEndBorderEvtOffset) && m_onUIItemsReachEnd != null)
                m_onUIItemsReachEnd(this);
            m_lastShowEndBorder = showBorder;
            m_lastEndBorderEvtOffset = evtOffset;
        }
        else
        {
            m_lastShowEndBorder = false;
            m_lastEndBorderEvtOffset = false;
        }
    }

    private void DoBounceMove(float unscaledDeltaTime)
    {
        var posDelta = 0.0f;

        /////////////////////////执行回弹////////////////////////
        var needBounce = !m_dragging && m_wantCenterIdx < 0 && !IsZero(m_bounceValue);

        var viewportSize = m_viewport.sizeDelta;
        var firstUIItem = m_uiItems[0].RectTransform;
        var lastUIItem = m_uiItems[m_uiItemCount - 1].RectTransform;

        var contentSize = 0.0f;
        var contentOffTopOrLeft = 0.0f;
        var contentOffRightOrBottom = 0.0f;

        if (m_type == enType.vertical)
        {
            if (m_dir == enDir.forward)
            {
                contentSize = firstUIItem.offsetMax.y - lastUIItem.offsetMin.y;
                contentOffTopOrLeft = -firstUIItem.offsetMax.y;
                contentOffRightOrBottom = -viewportSize.y - lastUIItem.offsetMin.y;
            }
            else
            {
                contentSize = lastUIItem.offsetMax.y - firstUIItem.offsetMin.y;
                contentOffTopOrLeft = -lastUIItem.offsetMax.y;
                contentOffRightOrBottom = -viewportSize.y - firstUIItem.offsetMin.y;
            }

            var epsilon = contentSize / m_uiItemCount / 1000;

            if (contentSize > viewportSize.y)
            {
                if (needBounce)
                {
                    if (contentOffTopOrLeft > epsilon)
                        posDelta = contentOffTopOrLeft * unscaledDeltaTime * m_bounceValue;
                    else if (contentOffRightOrBottom < -epsilon)
                        posDelta = contentOffRightOrBottom * unscaledDeltaTime * m_bounceValue;
                }

                if (!m_dataLoop)
                {
                    if (m_dir == enDir.forward)
                        CheckReachStartOrEndCallback(contentOffTopOrLeft, -contentOffRightOrBottom);
                    else
                        CheckReachStartOrEndCallback(-contentOffRightOrBottom, contentOffTopOrLeft);
                }                
            }
            else
            {
                if (m_dockSide)
                {
                    if (m_dir == enDir.forward)
                    {
                        if (needBounce)
                        {
                            if (contentOffTopOrLeft > epsilon || contentOffTopOrLeft < -epsilon)
                                posDelta = contentOffTopOrLeft * unscaledDeltaTime * m_bounceValue;
                        }

                        if (!m_dataLoop)
                        {
                            CheckReachStartOrEndCallback(contentOffTopOrLeft, -contentOffRightOrBottom - (viewportSize.y - contentSize));
                        }
                    }
                    else
                    {
                        if (needBounce)
                        {
                            if (contentOffRightOrBottom > epsilon || contentOffRightOrBottom < -epsilon)
                                posDelta = contentOffRightOrBottom * unscaledDeltaTime * m_bounceValue;
                        }

                        if (!m_dataLoop)
                        {
                            CheckReachStartOrEndCallback(-contentOffRightOrBottom, contentOffTopOrLeft - (viewportSize.y - contentSize));
                        }
                    }
                }
                else
                {
                    var halfSizeDiff = (viewportSize.y - contentSize) / 2;

                    if (needBounce)
                    {
                        var topOrLeftDiff = contentOffTopOrLeft - halfSizeDiff;
                        if (topOrLeftDiff > epsilon || topOrLeftDiff < -epsilon)
                            posDelta = topOrLeftDiff * unscaledDeltaTime * m_bounceValue;
                    }

                    if (!m_dataLoop)
                    {
                        if (m_dir == enDir.forward)
                            CheckReachStartOrEndCallback(contentOffTopOrLeft - halfSizeDiff, -contentOffRightOrBottom - halfSizeDiff);
                        else
                            CheckReachStartOrEndCallback(-contentOffRightOrBottom - halfSizeDiff, contentOffTopOrLeft - halfSizeDiff);
                    }
                }
            }

            if (contentOffTopOrLeft < -epsilon)
            {
                if (m_leftOrTopTip != null && !m_leftOrTopTip.gameObject.activeSelf)
                    m_leftOrTopTip.gameObject.SetActive(true);
            }
            else
            {
                if (m_leftOrTopTip != null && m_leftOrTopTip.gameObject.activeSelf)
                    m_leftOrTopTip.gameObject.SetActive(false);
            }

            if (contentOffRightOrBottom > epsilon)
            {
                if (m_rightOrBottomTip != null && !m_rightOrBottomTip.gameObject.activeSelf)
                    m_rightOrBottomTip.gameObject.SetActive(true);
            }
            else
            {
                if (m_rightOrBottomTip != null && m_rightOrBottomTip.gameObject.activeSelf)
                    m_rightOrBottomTip.gameObject.SetActive(false);
            }
        }
        else
        {
            if (m_dir == enDir.forward)
            {
                contentSize = lastUIItem.offsetMax.x - firstUIItem.offsetMin.x;
                contentOffTopOrLeft = -firstUIItem.offsetMin.x;
                contentOffRightOrBottom = viewportSize.x - lastUIItem.offsetMax.x;
            }
            else
            {
                contentSize = firstUIItem.offsetMax.x - lastUIItem.offsetMin.x;
                contentOffTopOrLeft = -lastUIItem.offsetMin.x;
                contentOffRightOrBottom = viewportSize.x - firstUIItem.offsetMax.x;
            }

            var epsilon = contentSize / m_uiItemCount / 1000;

            if (contentSize > viewportSize.x)
            {
                if (needBounce)
                {
                    if (contentOffTopOrLeft < -epsilon)
                        posDelta = contentOffTopOrLeft * unscaledDeltaTime * m_bounceValue;
                    else if (contentOffRightOrBottom > epsilon)
                        posDelta = contentOffRightOrBottom * unscaledDeltaTime * m_bounceValue;
                }

                if (!m_dataLoop)
                {
                    if (m_dir == enDir.forward)
                        CheckReachStartOrEndCallback(-contentOffTopOrLeft, contentOffRightOrBottom);
                    else
                        CheckReachStartOrEndCallback(contentOffRightOrBottom, -contentOffTopOrLeft);
                }
            }
            else
            {
                if (m_dockSide)
                {
                    if (m_dir == enDir.forward)
                    {
                        if (needBounce)
                        {
                            if (contentOffTopOrLeft > epsilon || contentOffTopOrLeft < -epsilon)
                                posDelta = contentOffTopOrLeft * unscaledDeltaTime * m_bounceValue;
                        }

                        if (!m_dataLoop)
                        {
                            CheckReachStartOrEndCallback(-contentOffTopOrLeft, contentOffRightOrBottom - (viewportSize.x - contentSize));
                        }
                    }
                    else
                    {
                        if (needBounce)
                        {
                            if (contentOffRightOrBottom > epsilon || contentOffRightOrBottom < -epsilon)
                                posDelta = contentOffRightOrBottom * unscaledDeltaTime * m_bounceValue;
                        }

                        if (!m_dataLoop)
                        {
                            CheckReachStartOrEndCallback(contentOffRightOrBottom, -contentOffTopOrLeft - (viewportSize.x - contentSize));
                        }
                    }
                }
                else
                {
                    var halfSizeDiff = (viewportSize.x - contentSize) / 2;

                    if (needBounce)
                    {
                        var topOrLeftDiff = halfSizeDiff + contentOffTopOrLeft;
                        if (topOrLeftDiff > epsilon || topOrLeftDiff < -epsilon)
                            posDelta = topOrLeftDiff * unscaledDeltaTime * m_bounceValue;
                    }

                    if (!m_dataLoop)
                    {
                        if (m_dir == enDir.forward)
                            CheckReachStartOrEndCallback(-contentOffTopOrLeft - halfSizeDiff, contentOffRightOrBottom - halfSizeDiff);
                        else
                            CheckReachStartOrEndCallback(contentOffRightOrBottom - halfSizeDiff, -contentOffTopOrLeft - halfSizeDiff);
                    }
                }
            }

            if (contentOffTopOrLeft > epsilon)
            {
                if (m_leftOrTopTip != null && !m_leftOrTopTip.gameObject.activeSelf)
                    m_leftOrTopTip.gameObject.SetActive(true);
            }
            else
            {
                if (m_leftOrTopTip != null && m_leftOrTopTip.gameObject.activeSelf)
                    m_leftOrTopTip.gameObject.SetActive(false);
            }

            if (contentOffRightOrBottom < -epsilon)
            {
                if (m_rightOrBottomTip != null && !m_rightOrBottomTip.gameObject.activeSelf)
                    m_rightOrBottomTip.gameObject.SetActive(true);
            }
            else
            {
                if (m_rightOrBottomTip != null && m_rightOrBottomTip.gameObject.activeSelf)
                    m_rightOrBottomTip.gameObject.SetActive(false);
            }
        }

        /////////////////////////执行回弹////////////////////////

        ArrangeUIItem(0, posDelta);
    }

    private void LateUpdate()
    {
        Cache();

        if (m_uiItemCount < 1)
            return;

        var unscaledDeltaTime = Mathf.Clamp01(Time.unscaledDeltaTime);

        CheckDelayRollTo();

        DoRollToIdx(unscaledDeltaTime);

        DoNormalMove(unscaledDeltaTime);

        DoBounceMove(unscaledDeltaTime);
    }
    #endregion

    public void SetDataList(List<object> objs, bool reset = true)
    {
        m_dataItems = objs ?? EMPTY_OBJECT_LIST;
        m_wantCenterIdx = -1;
        m_dataIdxInViewCenter = -1;
        m_delayCenterIdx = -1;
        m_delayCenterStart = 0;
        m_velocity = 0.0f;

        m_lastShowStartBorder = true;
        m_lastStartBorderEvtOffset = true;
        m_lastShowEndBorder = true;
        m_lastEndBorderEvtOffset = true;

        SetUIItemCount(m_dataItems.Count);

        if (reset)
            m_startIndex = -1;
        else
            m_startIndex = Math.Min(m_startIndex, m_dataItems.Count - m_uiItemCount);

        RefreshUIItems(reset);
    }

    public List<object> GetDataList()
    {
        return m_dataItems;
    }

    public int GetDataCount()
    {
        return m_dataItems.Count;
    }

    public object GetData(int idx)
    {
        if (idx < 0 || idx >= m_dataItems.Count)
            return null;
        return m_dataItems[idx];
    }

    public void SetData(int idx, object data)
    {
        if (idx < 0 || idx >= m_dataItems.Count)
            return;
        m_dataItems[idx] = data;
        RefreshUIItems();
    }

    public int GetCenterDataIndex()
    {
        Cache();

        if (m_dataItems.Count < 1)
            return -1;

        return m_dataIdxInViewCenter;
    }

    public void SetCenterDataIndex(int idx, bool immediately = false)
    {
        Cache();

        if (m_dataItems.Count < 1)
            return;

        if (m_dataLoop)
            idx = NormalizeNumber(idx, m_dataItems.Count);
        else
            idx = Mathf.Clamp(idx, 0, m_dataItems.Count - 1);

        m_velocity = 0.0f;
        m_wantCenterIdx = -1;
        m_delayCenterIdx = -1;
        m_delayCenterStart = 0;

        if (immediately)
        {
            var halfUIItemCnt = m_uiItemCount / 2;

            if (m_dataLoop)
                m_startIndex = NormalizeNumber(idx - halfUIItemCnt, m_dataItems.Count);
            else
                m_startIndex = Mathf.Clamp(idx - Math.Min((m_dataItems.Count - idx) / 2, halfUIItemCnt), 0, m_dataItems.Count - m_uiItemCount);

            var refUIIdx = 0;
            var refUIItem = (RectTransform)null;
            var targetPos = 0.0f;

            //////////////////////
            var viewportSize = m_viewport.sizeDelta[(int)m_type];
            var firstUIItem = m_uiItems[0].RectTransform;
            var lastUIItem = m_uiItems[m_uiItemCount - 1].RectTransform;
            var contentSize = 0.0f;

            if (m_type == enType.vertical)
            {
                if (m_dir == enDir.forward)
                {
                    contentSize = firstUIItem.offsetMax.y - lastUIItem.offsetMin.y;
                }
                else
                {
                    contentSize = lastUIItem.offsetMax.y - firstUIItem.offsetMin.y;
                }
            }
            else
            {
                if (m_dir == enDir.forward)
                {
                    contentSize = lastUIItem.offsetMax.x - firstUIItem.offsetMin.x;
                }
                else
                {
                    contentSize = firstUIItem.offsetMax.x - lastUIItem.offsetMin.x;
                }
            }
            //////////////////////

            if (viewportSize < contentSize)
            {
                var halfItemCntInView = m_itemCntInView / 2.0f;
                var checkIdxCond1 = idx < halfItemCntInView;
                var checkIdxCond2 = m_dataItems.Count - 1 - idx < halfItemCntInView;

                for (var i = 0; i < 3; ++i)
                {
                    if (m_dataLoop || (!checkIdxCond1 && !checkIdxCond2))
                    {
                        refUIIdx = halfUIItemCnt;
                        refUIItem = m_uiItems[refUIIdx].RectTransform;

                        if (m_type == enType.vertical)
                        {
                            targetPos = -(m_viewport.sizeDelta.y + refUIItem.sizeDelta.y) / 2;
                        }
                        else
                        {
                            targetPos = (m_viewport.sizeDelta.x - refUIItem.sizeDelta.x) / 2;
                        }
                    }
                    else if (checkIdxCond1)
                    {
                        refUIIdx = 0;
                        refUIItem = m_uiItems[refUIIdx].RectTransform;

                        if (m_type == enType.vertical)
                        {
                            if (m_dir == enDir.forward)
                            {
                                targetPos = -refUIItem.sizeDelta.y;
                            }
                            else
                            {
                                targetPos = -m_viewport.sizeDelta.y;
                            }
                        }
                        else
                        {
                            if (m_dir == enDir.forward)
                            {
                                targetPos = 0;
                            }
                            else
                            {
                                targetPos = m_viewport.sizeDelta.x - refUIItem.sizeDelta.x;
                            }
                        }
                    }
                    else if (checkIdxCond2)
                    {
                        refUIIdx = m_uiItemCount - 1;
                        refUIItem = m_uiItems[refUIIdx].RectTransform;

                        if (m_type == enType.vertical)
                        {
                            if (m_dir == enDir.forward)
                            {
                                targetPos = -m_viewport.sizeDelta.y;
                            }
                            else
                            {
                                targetPos = -refUIItem.sizeDelta.y;
                            }
                        }
                        else
                        {
                            if (m_dir == enDir.forward)
                            {
                                targetPos = m_viewport.sizeDelta.x - refUIItem.sizeDelta.x;
                            }
                            else
                            {
                                targetPos = 0;
                            }
                        }
                    }

                    var newOffMin = refUIItem.offsetMin;
                    newOffMin[(int)m_type] = targetPos;
                    refUIItem.anchoredPosition = newOffMin + Vector2.Scale(refUIItem.sizeDelta, refUIItem.pivot);

                    RefreshUIItems(false, refUIIdx);
                }
            }
            else
            {
                RefreshUIItems(false, refUIIdx);
            }
        }
        else
        {
            m_wantCenterIdx = idx;
        }
    }

    #region Test
    public int m_testVal = 0;

    [ContextMenu("测试—装载数据")]
    public void Test0()
    {       
        var dataItems = new List<object>();
        for (var i = 0; i < m_testVal; ++i)
            dataItems.Add(i.ToString());

        SetDataList(dataItems);

        m_testVal = 0;
    }

    [ContextMenu("测试—立即跳转")]
    public void Test1()
    {
        var idx = m_testVal;
        SetCenterDataIndex(idx, true);
        Debuger.Log(idx.ToString());
    }

    [ContextMenu("测试—滚动跳转")]
    public void Test2()
    {
        var idx = m_testVal;
        SetCenterDataIndex(idx, false);
        Debuger.Log(idx.ToString());
    }
    #endregion
}
