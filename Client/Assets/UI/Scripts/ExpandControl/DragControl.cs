using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class DragControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static Action<PointerEventData> ms_globalDragBeginHook;

    private GameObject m_draggingObj;
    private RectTransform m_draggingPlane;
    public object m_data;
    public Action<GameObject> m_initCopy;
    public bool m_canDrag = true;
    public Action m_onDrag;
    public Action m_onEndDrag;

    public static void AddGlobalHook(Action<PointerEventData> dragBegin = null)
    {
        if (dragBegin != null)
            ms_globalDragBeginHook += dragBegin;
    }

    public static void RemoveGlobalHook(Action<PointerEventData> dragBegin = null)
    {
        if (dragBegin != null)
            ms_globalDragBeginHook -= dragBegin;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!m_canDrag)
        {
            return;
        }

        var uiPanel = FindInParents<UIPanel>(gameObject);
        if (uiPanel == null)
            return;

        m_draggingObj = GameObject.Instantiate(gameObject) as GameObject;
        if (m_initCopy != null)
        {
            m_initCopy.Invoke(m_draggingObj);
        }
        m_draggingObj.transform.SetParent(uiPanel.transform, false);
        m_draggingObj.transform.SetAsLastSibling();

        CanvasGroup group = m_draggingObj.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;

        m_draggingPlane = transform as RectTransform;

        SetDraggedPosition(eventData);

        if (ms_globalDragBeginHook != null)
        {
            ms_globalDragBeginHook(eventData);
        }
    }



    private void SetDraggedPosition(PointerEventData data)
    {
        if (data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
            m_draggingPlane = data.pointerEnter.transform as RectTransform;

        var cam = data.pressEventCamera;
        if (cam == null)
        {
            int UI_LAYER = LayerMask.NameToLayer("UI");
            int UI_HIGHT_LAYER = LayerMask.NameToLayer("UIHight");
            cam = data.pointerPress && data.pointerPress.layer == UI_HIGHT_LAYER ? UIMgr.instance.UICameraHight : UIMgr.instance.UICamera;
        }            
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_draggingPlane, data.position, cam, out globalMousePos))
        {
            var rt = m_draggingObj.GetComponent<RectTransform>();
            rt.position = globalMousePos;
            rt.rotation = m_draggingPlane.rotation;
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (!m_canDrag)
        {
            return;
        }
        if (m_draggingObj != null)
            SetDraggedPosition(data);
        if (m_onDrag != null)
        {
            m_onDrag();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_draggingObj != null)
            Destroy(m_draggingObj);
        if (!m_canDrag)
        {
            return;
        }
        if (m_onEndDrag != null)
        {
            m_onEndDrag();
        }
    }

    static public T FindInParents<T>(GameObject go) where T : Component
    {
        if (go == null) return null;
        var comp = go.GetComponent<T>();

        if (comp != null)
            return comp;

        Transform t = go.transform.parent;
        while (t != null && comp == null)
        {
            comp = t.gameObject.GetComponent<T>();
            t = t.parent;
        }
        return comp;
    }
}