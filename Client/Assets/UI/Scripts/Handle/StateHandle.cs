using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

//状态控件
public partial class StateHandle : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler
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
    //单个状态
    [System.Serializable]
    public class State
    {
        public List<Handle> publicHandles = new List<Handle>();//共有处理，每个状态都有的，对同一个对象的处理
        public List<Handle> privateHandles = new List<Handle>();//独有处理，只有这个状态需要做的处理
        public string name="";
        public bool isDuration=false;
        public bool isEnterSound = false;
        public int enterSoundId = 0;
        public bool isExitSound = false;
        public int exitSoundId = 0;
        public State(string name)
        {
            this.name = name;
        }
        
        public void Start()
        {
            for (int i=0;i< publicHandles.Count;++i)
                publicHandles[i].Start();

            for (int i=0;i< privateHandles.Count;++i)
                privateHandles[i].Start();


        }

        public void End()
        {

            for (int i = 0; i < publicHandles.Count; ++i)
                publicHandles[i].End();

            for (int i = 0; i < privateHandles.Count; ++i)
                privateHandles[i].End();

        }

        public void PlayEnterSound()
        {
            if (isEnterSound)
            {
#if !ART_DEBUG
                SoundMgr.instance.Play2DSoundAutoChannel(enterSoundId);
#endif   
            }
        }

        public void PlayExitSound()
        {
            if (isExitSound)
            {
#if !ART_DEBUG
                SoundMgr.instance.Play2DSoundAutoChannel(exitSoundId);
#endif
            }
        }

        public void Update()
        {
            for (int i = 0; i < publicHandles.Count; ++i)
                publicHandles[i].Update();

            for (int i = 0; i < privateHandles.Count; ++i)
                privateHandles[i].Update();
        }
        public void DoAllHandle(bool includePrivate,System.Action<Handle> a)
        {
            for (int i = 0; i < publicHandles.Count; ++i)
                a(publicHandles[i]);
            if (includePrivate)
                for (int i = 0; i < privateHandles.Count; ++i)
                    a(privateHandles[i]);

            
        }
    }

    //状态
    public List<State> m_states = new List<State>() { new State("提起"), new State("按下") };

    //默认状态
    public int m_curState = 0;

    //如果有持续性的状态那么，那么持续时间是多少
    [SerializeField] float m_duration=0.1f;

    //是不是不受时间缩放影响
    [SerializeField]bool m_isRealtime = false;         

    bool m_cached = false;

    public float Duration
    {
        get { return m_duration;}
        set {
            m_duration = value;
            DoAllHandle(true,handle=>handle.m_duration = m_duration);
            EditorUtil.SetDirty(this);
        }
    }

    public bool IsRealTime
    {
        get { return m_isRealtime; }
        set
        {
            m_isRealtime = value;
            DoAllHandle(true, handle => handle.m_isRealtime = m_isRealtime);
            EditorUtil.SetDirty(this);
        }
    }

    public State CurState { get{return m_curState < m_states.Count?m_states[m_curState]:null;}}

    public int CurStateIdx { get{return m_curState;}}
    
    public string[] StateName { get{
        string [] temStateName = new string[m_states.Count];
        for (int i = 0; i < m_states.Count; ++i)
            temStateName[i] =m_states[i].name;
        return temStateName;
    }}


    #region 数据的增删处理
    
    public void DoAllHandle(bool includePrivate, System.Action<Handle> a)
    {
        foreach (State s in m_states)
            s.DoAllHandle(includePrivate, a);
    }

    public bool CanMoveState(int state, bool up)
    {
        if(state >= m_states.Count)
            return false;

        if (up && state <= 0)
            return false;
        
        if(!up && state >= m_states.Count-1)
            return false;
        return true;            
    }
    //修改状态位置,up==true上移，否则下移
    public void MoveState(int state ,bool up)
    {
        if(!CanMoveState(state,up))
            return;
        State s1 =m_states[state];
        m_states[state] = m_states[up?state-1:state+1] ;
        m_states[up?state-1:state+1] = s1;    
        EditorUtil.SetDirty(this);

    }
    
    //增加状态
    public void AddState()
    {
        State state = new State("");
        //复制共有处理的共有的属性
        if (m_states.Count != 0)
        {
            m_states[0].DoAllHandle(false,hOld=>{
                Handle h = new Handle();
                h.m_go = hOld.m_go;
                h.m_duration = hOld.m_duration;
                h.m_isRealtime = hOld.m_isRealtime;
                h.m_isUseNowStart = hOld.m_isUseNowStart;
                h.SetType(hOld.m_type);
                state.publicHandles.Add(h);
            });
        }
        state.name = "状态";
        m_states.Add(state);
        EditorUtil.SetDirty(this);
    }

    //删除状态
    public void RemoveState(int state)
    {
        if (state >= m_states.Count)
            return;
        m_states.RemoveAt(state);
        EditorUtil.SetDirty(this);
    }

    //增加共有处理
    public void AddPublicHandle(Handle.Type type)
    {
        foreach(State state in m_states){
            Handle h = new Handle();
            h.m_duration = m_duration;
            h.m_isRealtime = IsRealTime;
            h.m_isUseNowStart = true;
            h.SetType(type);
            state.publicHandles.Add(h);
        }

        EditorUtil.SetDirty(this);
    }

    //删除共有处理
    public void RemovePublicHandle(int handle)
    {
        foreach (State state in m_states)
            state.publicHandles.RemoveAt(handle);
        EditorUtil.SetDirty(this);
    }

    //增加独有处理
    public void AddPrivateHandle(int state, Handle.Type type)
    {        
        State s = m_states[state];
        Handle h = new Handle();
        h.m_duration = m_duration;
        h.m_isRealtime = m_isRealtime;
        h.m_isUseNowStart = true;
        h.SetType(type);
        s.privateHandles.Add(h);
        EditorUtil.SetDirty(this);
    }

    //删除独有处理
    public void RemovePrivateHandle(int state, int handle)
    {
        if (state >= m_states.Count)
            return;
        State s = m_states[state];
        if (handle >= s.privateHandles.Count)
            return;
        s.privateHandles.RemoveAt(handle);
        EditorUtil.SetDirty(this);
    }

    #endregion

    void Cache()
    {
        if (m_cached)
            return;
        m_cached = true;
        if (m_curState < m_states.Count)
            SetState(m_curState,false);
    }

    void Start()
    {   
        Cache();
    }

    public void LateUpdate()
    {
        if(CurState!=null)
            CurState.Update();

        UpdateSelectable();
    }

    public T Get<T>() where T : Component
    {
        Cache();
        return this.GetComponent<T>();
    }

    //设置当前状态
    public void SetState(int state,bool isDuration = true,bool checkSameState=false)
    {
        if (state >= m_states.Count)
        {
            Debuger.LogError("设置状态错误");
            return;
        }
        if (checkSameState && m_curState == state && m_cached ==true)
            return;

        m_states[m_curState].PlayExitSound();
        m_curState = state;
        m_cached = true;  
        if (isDuration && m_states[m_curState].isDuration)
            m_states[m_curState].Start();
        else
            m_states[m_curState].End();
        m_states[m_curState].PlayEnterSound();

        if(m_onChangeState!=null)
        {
            m_onChangeState(this, m_curState);
        }

        if (ms_globalStateHook != null && (m_ctrlType == CtrlType.toggle || m_ctrlType == CtrlType.none))
            ms_globalStateHook(this, m_curState);
    }

    //获取状态
    public State GetState(int state)
    {
        if (state >= m_states.Count)
        {
            Debuger.LogError("获取状态错误");
            return null;
        }
        return m_states[state];
    }

  
}
