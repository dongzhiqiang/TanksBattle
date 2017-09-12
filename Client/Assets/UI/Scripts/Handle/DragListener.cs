using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;




public class DragListener : MonoBehaviour, 
        IInitializePotentialDragHandler,
        IBeginDragHandler, 
        IDragHandler, 
        IEndDragHandler
       
{
    public Action<PointerEventData> onDragBegin;
    public Action<PointerEventData> onDrag;
    public Action<PointerEventData> onDragEnd;



    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (onDragBegin != null)
            onDragBegin(eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (onDrag != null)
            onDrag(eventData);

        
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {

        if (onDragEnd != null)
            onDragEnd(eventData);
        

        
    }



}
