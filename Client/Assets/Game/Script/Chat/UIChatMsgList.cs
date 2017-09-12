using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIChatMsgList : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    public RectTransform m_viewport;
    public RectTransform m_content;
    public float m_scrollSensitivity = -50;
    public float m_decelerationRate = 0.0135f;
    public float m_bounceValue = 10.0f;

    private bool m_cached = false;
    private bool m_needRefresh = false;
    private bool m_dragging = false;
    private List<UIChatMsgItem> m_uiItemList = new List<UIChatMsgItem>();
    private int m_uiItemCount = 0;
    private List<ChatMsgItem> m_itemList = null;
    private bool m_showHead = true;
    private bool m_showName = true;
    private int m_startIndex = int.MaxValue;
    private int m_centerIndex = -1;
    private float m_velocity = 0.0f;
    private float m_lastMouseY = 0.0f;

    public List<ChatMsgItem> ItemList
    {
        get { return m_itemList; }
    }

    private void Cache()
    {
        if (m_cached)
            return;

        var itemRect = m_content.GetChild(0) as RectTransform;
        if (MathUtil.IsEqual(itemRect.sizeDelta.y, 0.0f))
            return;

        m_cached = true;

        m_uiItemList.Clear();
        m_uiItemCount = 0;
        
        var inViewCnt = Math.Ceiling(m_viewport.sizeDelta.y / itemRect.sizeDelta.y);
        var totalCnt = inViewCnt * 3;
        var exitsCnt = m_content.childCount;

        if (exitsCnt > totalCnt)
        {
            for (var i = exitsCnt - 1; i >= totalCnt; --i)
                DestroyImmediate(m_content.GetChild(i));
        }
        else if (exitsCnt < totalCnt)
        {
            for (var i = exitsCnt; i < totalCnt; ++i)
            {
                var go = Instantiate(itemRect.gameObject);
                var rf = go.transform;
                rf.SetParent(m_content, false);
                rf.localPosition = Vector3.zero;
                rf.localRotation = itemRect.localRotation;
                rf.localScale = itemRect.localScale;
            }
        }

        for (int i = 0; i < m_content.childCount; ++i)
        {
            var item = m_content.GetChild(i).GetComponent<UIChatMsgItem>();
            item.name = "item" + i;
            item.gameObject.SetActive(false);
            m_uiItemList.Add(item);
        }
    }

    private void SetUIItemCount(int count)
    {
        var itemCount = Mathf.Min(count, m_uiItemList.Count);
        if (m_uiItemCount == itemCount)
            return;

        m_uiItemCount = itemCount;
        for (var i = 0; i < m_uiItemList.Count; ++i)
        {
            var go = m_uiItemList[i].gameObject;
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

    public void SetItemList(List<ChatMsgItem> itemList, bool showHead, bool showName)
    {
        m_itemList = itemList;
        m_showHead = showHead;
        m_showName = showName;
        m_startIndex = int.MaxValue;
        m_needRefresh = true;
        m_velocity = 0.0f;
        ScrollToBottom();
    }

    public void Clear()
    {
        SetItemList(null, false, false);
    }

    public void ScrollTo(int index)
    {
        if (m_itemList == null)
            return;

        index = Mathf.Clamp(index, 0, m_itemList.Count - 1);
        m_startIndex = Mathf.Max(0, index - m_uiItemCount / 2);
        m_centerIndex = index;
        m_needRefresh = true;
        m_velocity = 0.0f;
    }

    public void ScrollToTop()
    {
        ScrollTo(0);
    }

    public void ScrollToBottom()
    {
        ScrollTo(int.MaxValue);
    }

    private void RefreshUI()
    {
        Cache();

        SetUIItemCount(m_itemList == null ? 0 : m_itemList.Count);

        if (m_uiItemCount <= 0)
            return;

        var startIndex = m_startIndex = Mathf.Clamp(m_startIndex, 0, m_itemList.Count - m_uiItemCount);
        var endIndex = m_startIndex + m_uiItemCount;

        var hero = RoleMgr.instance.Hero;
        var myHeroId = hero == null ? 0 : hero.GetInt(enProp.heroId);

        for (var i  = startIndex; i < endIndex; ++i)
        {
            var dataItem = m_itemList[i];
            var uiItem = m_uiItemList[i - startIndex];
            var dockType = myHeroId == dataItem.heroId ? UIChatDockType.right : UIChatDockType.left;
            uiItem.Init(dockType, m_showHead, m_showName, dataItem.msg, dataItem.name, dataItem.time, RoleCfg.GetHeadIcon(dataItem.roleId), dataItem.rolelv, dataItem.viplv);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(m_content);

        if (m_centerIndex >= 0)
        {
            var centerIndex = Mathf.Clamp(m_centerIndex, startIndex, endIndex);
            m_centerIndex = -1;

            //中间对齐
            var uiItem = m_uiItemList[centerIndex - startIndex];
            var itemCenter = MathUtil.GetCenterInWorldSpace(uiItem.transform as RectTransform);
            var viewCenter = MathUtil.GetCenterInWorldSpace(m_viewport);
            m_content.Translate(new Vector3(0, viewCenter.y - itemCenter.y, 0));

            //如果上或下有空白了，那就移回去
            var viewportRect = MathUtil.GetRectInWorldSpace(m_viewport as RectTransform);
            var contentRect = MathUtil.GetRectInWorldSpace(m_content as RectTransform);
            var contentYOffTop = viewportRect.yMax - contentRect.yMax;
            var contentYOffBottom = viewportRect.yMin - contentRect.yMin;
            if (contentRect.height > viewportRect.height)
            {
                if (contentYOffTop > Mathf.Epsilon)
                    m_content.Translate(new Vector3(0, contentYOffTop, 0));
                else if (contentYOffBottom < -Mathf.Epsilon)
                    m_content.Translate(new Vector3(0, contentYOffBottom, 0));
            }
            else
            {
                if (contentYOffTop > Mathf.Epsilon || contentYOffTop < -Mathf.Epsilon)
                    m_content.Translate(new Vector3(0, contentYOffTop, 0));
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_dragging = true;
            m_lastMouseY = UIMgr.instance.UICamera.ScreenToWorldPoint(Input.mousePosition).y;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            var deltaWorld = m_content.TransformVector(eventData.delta);

            var viewportRect = MathUtil.GetRectInWorldSpace(m_viewport as RectTransform);
            var contentRect = MathUtil.GetRectInWorldSpace(m_content as RectTransform);
            var offset = 0.0f;
            var max = contentRect.yMax + deltaWorld.y;
            var min = contentRect.yMin + deltaWorld.y;
            if (max < viewportRect.yMax)
                offset = viewportRect.yMax - max;
            else if (min > viewportRect.yMin)
                offset = min - viewportRect.yMin;

            var deltaY = deltaWorld.y * Mathf.Clamp(1.0f - offset / viewportRect.height, 0.0f, 1.0f);
            m_content.Translate(new Vector3(0, deltaY, 0));
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            m_dragging = false;
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        var delta = m_content.TransformVector(eventData.scrollDelta);
        m_content.Translate(new Vector3(0, delta.y * m_scrollSensitivity, 0));
    }

    private void LateUpdate()
    {
        if (m_needRefresh || !m_cached)
        {
            m_needRefresh = false;
            RefreshUI();
        }

        if (m_uiItemCount <= 0)
            return;

        float unscaledDeltaTime = Time.unscaledDeltaTime;
        if (m_dragging)
        {
            var curMouseY = UIMgr.instance.UICamera.ScreenToWorldPoint(Input.mousePosition).y;
            var velocity = (curMouseY - m_lastMouseY) / unscaledDeltaTime;
            m_velocity = Mathf.Lerp(m_velocity, velocity, unscaledDeltaTime * 10f);
            m_lastMouseY = curMouseY;
        }
        else
        {
            if (!MathUtil.IsEqual(m_velocity, 0.0f))
            {
                m_velocity *= Mathf.Pow(m_decelerationRate, unscaledDeltaTime);
                m_content.Translate(new Vector3(0, m_velocity * unscaledDeltaTime, 0));
            }
        }

        var viewportRect = MathUtil.GetRectInWorldSpace(m_viewport as RectTransform);
        Vector3 refItemPos;

        //最顶项作为判断项，如果中心点显示出来了，就上面加载更多，并保存这个位置
        if (viewportRect.Contains(refItemPos = MathUtil.GetCenterInWorldSpace(m_uiItemList[0].transform as RectTransform)))
        {
            if (m_startIndex > 0)
            {
                var delta = Mathf.Clamp(m_uiItemCount / 3, 1, m_startIndex);
                m_startIndex -= delta;
                RefreshUI();

                //获取原最顶项
                Vector3 refItemPos2 = MathUtil.GetCenterInWorldSpace(m_uiItemList[delta].transform as RectTransform);

                //移动viewport，使得原最顶项还是显示在原位置
                m_content.Translate(new Vector3(0, refItemPos.y - refItemPos2.y, 0));
            }
        }
        //最底项作为判断项，如果中心点显示出来了，就下面加载更多，并保存这个位置
        else if (viewportRect.Contains(refItemPos = MathUtil.GetCenterInWorldSpace(m_uiItemList[m_uiItemCount - 1].transform as RectTransform)))
        {
            var maxIndex = m_itemList.Count - m_uiItemCount;
            if (m_startIndex < maxIndex)
            {
                var delta = Mathf.Clamp(m_uiItemCount / 3, 1, maxIndex - m_startIndex);
                m_startIndex += delta;
                RefreshUI();

                //获取原最底项
                Vector3 refItemPos2 = MathUtil.GetCenterInWorldSpace(m_uiItemList[m_uiItemCount - 1 - delta].transform as RectTransform);

                //移动viewport，使得原最底项还是显示在原位置
                m_content.Translate(new Vector3(0, refItemPos.y - refItemPos2.y, 0));
            }
        }

        if (!m_dragging)
        {
            var contentRect = MathUtil.GetRectInWorldSpace(m_content as RectTransform);
            var contentYOffTop = viewportRect.yMax - contentRect.yMax;
            var contentYOffBottom = viewportRect.yMin - contentRect.yMin;
            if (contentRect.height > viewportRect.height)
            {
                if (contentYOffTop > Mathf.Epsilon)
                    m_content.Translate(new Vector3(0, contentYOffTop * Mathf.Clamp01(unscaledDeltaTime) * m_bounceValue, 0));
                else if (contentYOffBottom < -Mathf.Epsilon)
                    m_content.Translate(new Vector3(0, contentYOffBottom * Mathf.Clamp01(unscaledDeltaTime) * m_bounceValue, 0));
            }
            else
            {
                if (contentYOffTop > Mathf.Epsilon || contentYOffTop < -Mathf.Epsilon)
                    m_content.Translate(new Vector3(0, contentYOffTop * Mathf.Clamp01(unscaledDeltaTime) * m_bounceValue, 0));
            }
        }        
    }
}