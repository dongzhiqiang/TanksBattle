using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(ScrollRect))]
public class SlidePage : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    private ScrollRect m_scroll;

    void Awake()
    {
        m_scroll = GetComponent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var cnt = GetItemCount();
        if (cnt <= 0)
            return;
        var curIndex = GetCurIndex();
        SwitchToItem(curIndex);
        Debug.Log(curIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging)
            return;

        var cnt = GetItemCount();
        if (cnt <= 0)
            return;
        var curIndex = GetCurIndex();
        SwitchToItem((curIndex + 1) % cnt);
    }

    public int GetItemCount()
    {
        if (m_scroll == null || m_scroll.content == null)
            return 0;

        return m_scroll.content.childCount;
    }

    public int GetCurIndex()
    {
        if (m_scroll == null || m_scroll.content == null)
            return -1;

        int childCnt = m_scroll.content.childCount;
        if (childCnt <= 0)
            return -1;

        //主要思路就是，看哪个的Item的中心离视口的中心近就算谁的

        //算视口中点的位置
        Transform viewPort = m_scroll.viewport == null ? m_scroll.transform : m_scroll.viewport.transform;
        Vector2 viewPortCenter = MathUtil.GetCenterInWorldSpace(viewPort as RectTransform);

        //计算每个Item中心离视口中心的距离
        float minDist = float.MaxValue;
        int minDistIndex = -1;
        for (int i = 0; i < childCnt; ++i)
        {
            Transform curItem = m_scroll.content.GetChild(i);
            Vector2 curItemCenter = MathUtil.GetCenterInWorldSpace(curItem as RectTransform);
            float dist = Vector2.Distance(curItemCenter, viewPortCenter);
            if (dist < minDist)
            {
                minDist = dist;
                minDistIndex = i;
            }
        }
        return minDistIndex;
    }

    public Transform GetItem(int index)
    {
        if (m_scroll == null || m_scroll.content == null)
            return null;

        return m_scroll.content.GetChild(index);
    }

    public void SwitchToItem(int index)
    {
        UIScrollTips.ScrollPos(m_scroll, index);
    }
}