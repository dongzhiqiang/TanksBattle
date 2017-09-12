using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;



//状态控件
public partial class StateHandle : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler
        //IBeginDragHandler, 
        //IDragHandler, 
        //IEndDragHandler
        //IPointerEnterHandler,
        //IPointerExitHandler,
        //IPointerClickHandler,
        //IInitializePotentialDragHandler,
        //IDropHandler,
        //IScrollHandler,
        //IUpdateSelectedHandler,
        //ISelectHandler,
        //IDeselectHandler,
        //IMoveHandler,
        //ISubmitHandler,
        //ICancelHandler
{
    //控制器类型
    public enum CtrlType
    {
        none,
        button,//
        toggle,
        //funBool,//每个update检查一个函数，返回值true 1,false 0
        //funInt,//每个update检查一个函数，返回值int设置状态
    }
    public float m_pressHoldTime = 0.4f;
    public CtrlType m_ctrlType = CtrlType.none;


    bool m_enableCtrlState = true;//false就不会自己改变状态，但是消息仍然正常发送
    bool m_isPressing = false;//是不是在按紧状态
    bool m_isPressHoldThisTime = false;
    float m_lastPressTime;//最后一次按的时间
    //public UnityEvent<bool> m_onFunBool;
    //public UnityEvent<int> m_onFunInt;

    #region 消息
    static Action<StateHandle> ms_globalClickHook;
    static Action<StateHandle> ms_globalHoldHook;
    static Action<PointerEventData> ms_globalDownHook;
    static Action<PointerEventData> ms_globalUpHook;
    static Action<PointerEventData> ms_globalDragBeginHook;
    static Action<PointerEventData> ms_globalDragEndHook;
    static Action<StateHandle, int> ms_globalStateHook;

    Action m_onClick;
    Action<StateHandle> m_onClickEx;
    Action<StateHandle> m_onPressHold;//按紧，如果有这个回调，那么按紧超过这个时间后就会调用，并且不触发onClick
    Action<StateHandle,int> m_onChangeState;
    Action<PointerEventData> m_onPointUp;
    Action<PointerEventData> m_onPointDown;
    Action<PointerEventData> m_onDragBegin;
    Action<PointerEventData> m_onDrag;
    Action<PointerEventData> m_onDragEnd;
    #endregion

    
    public bool EnableCtrlState { get{return m_enableCtrlState;}set{m_enableCtrlState = value;} }

    #region 消息
    public static void AddGlobalHook(Action<StateHandle> click = null, Action<StateHandle> hold = null, Action<PointerEventData> onPointDown = null, Action<PointerEventData> onPointUp = null, Action<PointerEventData> dragBegin = null, Action<PointerEventData> dragEnd = null, Action<StateHandle, int> onChangeState = null)
    {
        if (click != null)
            ms_globalClickHook += click;
        if (hold != null)
            ms_globalHoldHook += hold;
        if (onPointDown != null)
            ms_globalDownHook += onPointDown;
        if (onPointUp != null)
            ms_globalUpHook += onPointUp;
        if (dragBegin != null)
            ms_globalDragBeginHook += dragBegin;
        if (dragEnd != null)
            ms_globalDragEndHook += dragEnd;
        if (onChangeState != null)
            ms_globalStateHook += onChangeState;
    }
    public static void RemoveGlobalHook(Action<StateHandle> click = null, Action<StateHandle> hold = null, Action<PointerEventData> onPointDown = null, Action<PointerEventData> onPointUp = null, Action<PointerEventData> dragBegin = null, Action<PointerEventData> dragEnd = null, Action<StateHandle, int> onChangeState = null)
    {
        if (click != null)
            ms_globalClickHook -= click;
        if (hold != null)
            ms_globalHoldHook -= hold;
        if (onPointDown != null)
            ms_globalDownHook -= onPointDown;
        if (onPointUp != null)
            ms_globalUpHook -= onPointUp;
        if (dragBegin != null)
            ms_globalDragBeginHook -= dragBegin;
        if (dragEnd != null)
            ms_globalDragEndHook -= dragEnd;
        if (onChangeState != null)
            ms_globalStateHook -= onChangeState;
    }

    public void AddClick(Action cb, bool reset = false)
    {
        if (m_onClick == null || reset)
        {
            m_onClick = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onClick.GetInvocationList();
        foreach (Delegate d in inlist)
        {
            if (d == cb)
                return;
        }

        m_onClick += cb;
    }

    public void AddClickEx(Action<StateHandle> cb, bool reset = false)
    {
        if (m_onClickEx == null || reset)
        {
            m_onClickEx = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onClickEx.GetInvocationList();
        foreach (Delegate d in inlist)
        {
            if (d == cb)
                return;
        }

        m_onClickEx += cb;
    }

    public void AddPressHold(Action<StateHandle> cb, bool reset = false)
    {
        if (m_onPressHold == null || reset)
        {
            m_onPressHold = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onPressHold.GetInvocationList();
        foreach (Delegate d in inlist)
        {
            if (d == cb)
                return;
        }

        m_onPressHold += cb;
    }

    public void AddChangeState(Action<StateHandle,int> cb, bool reset = false)
    {
        if (m_onChangeState == null || reset)
        {
            m_onChangeState = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onChangeState.GetInvocationList();
        foreach (Delegate d in inlist)
        {
            if (d == cb)
                return;
        }

        m_onChangeState += cb;
    }

    public void AddPointUp(Action<PointerEventData> cb, bool reset = false)
    {
        if (m_onPointUp == null || reset)
        {
            m_onPointUp = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onPointUp.GetInvocationList();
        foreach (Delegate d in inlist)
            if (d == cb) return;

        m_onPointUp += cb;
    }

    public void AddPointDown(Action<PointerEventData> cb, bool reset = false)
    {
        if (m_onPointDown == null || reset)
        {
            m_onPointDown = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onPointDown.GetInvocationList();
        foreach (Delegate d in inlist)
            if (d == cb) return;

        m_onPointDown += cb;
    }

    public void AddDragBegin(Action<PointerEventData> cb, bool reset = false)
    {
        CheckDragListener();
        if (m_onDragBegin == null || reset)
        {
            m_onDragBegin = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onDragBegin.GetInvocationList();
        foreach (Delegate d in inlist)
            if (d == cb) return;

        m_onDragBegin += cb;
    }

    public void AddDrag(Action<PointerEventData> cb, bool reset = false)
    {
        CheckDragListener();
        if (m_onDrag == null || reset)
        {
            m_onDrag = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onDrag.GetInvocationList();
        foreach (Delegate d in inlist)
            if (d == cb) return;

        m_onDrag += cb;
    }

    public void AddDragEnd(Action<PointerEventData> cb, bool reset = false)
    {
        CheckDragListener();
        if (m_onDragEnd == null || reset)
        {
            m_onDragEnd = cb;
            return;
        }

        //如果重复添加，那么就不添加了
        Delegate[] inlist = m_onDragEnd.GetInvocationList();
        foreach (Delegate d in inlist)
            if (d == cb) return;

        m_onDragEnd += cb;
    }

    #endregion

    #region 状态变化监听
    void UpdateSelectable()
    {
        if(!m_isPressHoldThisTime &&m_isPressing&& Time.unscaledTime - m_lastPressTime > m_pressHoldTime)
        {
            m_isPressHoldThisTime = true;
            if (m_onPressHold != null)
            {
                m_onPressHold(this);

                if (ms_globalHoldHook != null)
                    ms_globalHoldHook(this);
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //如果已经触发了按紧，那么不触发点击
        if (m_onPressHold != null &&Time.unscaledTime - m_lastPressTime > m_pressHoldTime)
            return;

        //Vector2 p = eventData == null ? Vector2.zero : eventData.position;
        //if (eventData.eligibleForClick)//如果鼠标提起的时候已经偏移了5个像素，那么没有点击事件 (pressPosition - p).sqrMagnitude < 25
        //{
            if (m_onClickEx != null)
                m_onClickEx(this);
            if (m_onClick != null)
                m_onClick();
        //}

        if ((m_onClickEx != null || m_onClick != null) && ms_globalClickHook != null)
            ms_globalClickHook(this);
    }

    
    //Vector2 pressPosition = Vector2.zero;
    public void OnPointerDown(PointerEventData eventData)
    {
        //记录下最后按下的时间
        m_isPressing = true;
        m_isPressHoldThisTime = false;
        m_lastPressTime = Time.unscaledTime;


        if (m_onPointDown != null)
        {
            //lastPress很可能不是当前对象，这里重新赋值
            if(eventData != null)
            {
                eventData.pointerPress = gameObject;
            }                
            else
            {
                eventData = new PointerEventData(EventSystem.current);
                eventData.pointerPress = gameObject;
            }

            m_onPointDown(eventData);

            if (ms_globalDownHook != null)
                ms_globalDownHook(eventData);
        }
        //if (!IsActive() || !IsInteractable())
        //    return;
        //pressPosition = eventData == null?Vector2.zero:eventData.position;
        //控件状态变化
        if (!m_enableCtrlState)
            return;
        else if (m_ctrlType == CtrlType.button)
        {
            if(this.m_curState == 0 || this.m_curState == 1)
                this.SetState(1);
        }
            
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        m_isPressing = false;

        //如果已经触发了按紧，那么不触发点击
        
        //控件状态变化
        if (!m_enableCtrlState)
        {

        }            
        else if(m_ctrlType == CtrlType.button)
        {
            if(this.m_curState == 0 || this.m_curState == 1)
                this.SetState(0);
        }
        else if (m_ctrlType == CtrlType.toggle)
        {
            if (m_onPressHold != null && Time.unscaledTime - m_lastPressTime > m_pressHoldTime)
                return;
            if (this.m_curState == 0 || this.m_curState == 1)
                this.SetState(this.m_curState==0?1:0);
        }

        //if (!IsActive() || !IsInteractable())
        //    return;

       

        if (m_onPointUp != null)
        {
            //这个函数被手动调用时，eventData可能为null
            if (eventData == null)
            {
                eventData = new PointerEventData(EventSystem.current);
                eventData.pointerPress = gameObject;
            }

            m_onPointUp(eventData);

            if (ms_globalUpHook != null)
                ms_globalUpHook(eventData);
        }
    }
    void CheckDragListener()
    {
        DragListener l =this.GetComponent<DragListener>();
        if (l != null)
            return;
        l = this.gameObject.AddComponentIfNoExist<DragListener>();
        l.onDragBegin = OnBeginDrag;
        l.onDrag = OnDrag;
        l.onDragEnd = OnEndDrag;
    }

    void OnBeginDrag(PointerEventData eventData)
    {
        if (m_onDragBegin != null)
            m_onDragBegin(eventData);
        //这里不判断m_onDragBegin不为null再执行ms_globalDragBeginHook，是为了引导回调，因为有些UI没订阅BeginDrag
        if (ms_globalDragBeginHook != null)
            ms_globalDragBeginHook(eventData);
    }

    void OnDrag(PointerEventData eventData)
    {
        if (m_onDrag != null)
            m_onDrag(eventData);
    }

    void OnEndDrag(PointerEventData eventData)
    {
        if (m_onDragEnd != null)
            m_onDragEnd(eventData);
        //这里不判断m_onDragEnd不为null再执行ms_globalDragEndHook，是为了引导回调，因为有些UI没订阅EndDrag
        if (ms_globalDragEndHook != null)
            ms_globalDragEndHook(eventData);
    }

    #region 模拟操作
    public void ExecuteClick()
    {
        if (m_onClickEx != null)
            m_onClickEx(this);
        if (m_onClick != null)
            m_onClick();
        if ((m_onClickEx != null || m_onClick != null) && ms_globalClickHook != null)
            ms_globalClickHook(this);
    }

    public void ExecuteDown()
    {
        ExecuteDown(Vector2.zero);
    }

    public void ExecuteDown(Vector2 position)
    {
        if (m_onPointDown != null)
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.pointerPress = gameObject;
            eventData.position = position;

            m_onPointDown(eventData);

            if (ms_globalDownHook != null)
                ms_globalDownHook(eventData);
        }
    }

    public void ExecuteUp()
    {
        ExecuteUp(Vector2.zero);
    }

    public void ExecuteUp(Vector2 position)
    {        
        if (m_onPointUp != null)
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.pointerPress = gameObject;
            eventData.position = position;

            m_onPointUp(eventData);

            if (ms_globalUpHook != null)
                ms_globalUpHook(eventData);
        }        
    }

    public void ExecuteHold()
    {
        if (m_onPressHold != null)
        {
            m_onPressHold(this);

            if (ms_globalHoldHook != null)
                ms_globalHoldHook(this);
        }
    }

    public void ExecuteBeginDrag()
    {
        ExecuteBeginDrag(Vector2.zero);
    }

    public void ExecuteBeginDrag(Vector2 position)
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.pointerPress = gameObject;
        eventData.pointerDrag = gameObject;
        eventData.position = position;

        OnBeginDrag(eventData);
    }

    public void ExecuteDrag()
    {
        ExecuteDrag(Vector2.zero);
    }

    public void ExecuteDrag(Vector2 position)
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.pointerPress = gameObject;
        eventData.pointerDrag = gameObject;
        eventData.position = position;

        OnDrag(eventData);
    }

    public void ExecuteEndDrag()
    {
        ExecuteEndDrag(Vector2.zero);
    }

    public void ExecuteEndDrag(Vector2 position)
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.pointerPress = gameObject;
        eventData.pointerDrag = gameObject;
        eventData.position = position;

        OnEndDrag(eventData);
    }
    #endregion

    //public virtual void OnPointerEnter(PointerEventData eventData)
    //{

    //}

    //public virtual void OnPointerExit(PointerEventData eventData)
    //{

    //}



    //public virtual void OnDrop(PointerEventData eventData)
    //{

    //}


    //public virtual void OnPointerClick(PointerEventData eventData)
    //{

    //}

    //public virtual void OnSelect(BaseEventData eventData)
    //{

    //}

    //public virtual void OnDeselect(BaseEventData eventData)
    //{

    //}

    //public virtual void OnScroll(PointerEventData eventData)
    //{

    //}

    //public virtual void OnMove(AxisEventData eventData)
    //{

    //}

    //public virtual void OnUpdateSelected(BaseEventData eventData)
    //{

    //}

    //public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    //{

    //}



    //public virtual void OnSubmit(BaseEventData eventData)
    //{

    //}

    //public virtual void OnCancel(BaseEventData eventData)
    //{

    //}
    #endregion



}
