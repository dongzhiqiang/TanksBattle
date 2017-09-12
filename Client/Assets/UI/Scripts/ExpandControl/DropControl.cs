using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class DropControl : MonoBehaviour, IDropHandler//, IPointerEnterHandler, IPointerExitHandler
{
    public Action<object> m_onDrop;

    public void OnDrop(PointerEventData data)
    {
        var originalObj = data.pointerDrag;
        if(originalObj == null)
        {
            return;
        }

        var dragControl = originalObj.GetComponent<DragControl>();
        if(dragControl == null)
        {
            return;
        }
        
        if(m_onDrop != null)
        {
            m_onDrop.Invoke(dragControl.m_data);
        }
    }
}