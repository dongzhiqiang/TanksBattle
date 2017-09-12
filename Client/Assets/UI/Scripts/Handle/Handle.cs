//-----------------------------------------------------------------
//                          操作类            
//-----------------------------------------------------------------
//描述: 用于实现一系列在一定时间内渐变执行的操作的基类。
//      提供了延迟一段时间再播放、播放类型(单次、循环、乒乓)、可填
//      播放次数、播放倍数、动画曲线等功能。
//      组件用到的话，通过Reset()、Start()和Update()就可以实现强大的功能
//-----------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public sealed class Handle
{
    public enum WndType
    {
        sequenceEditor, //处理序列编辑器
    }

    public enum Type{
        none,
        visiable,
        position,
        rotate,
        scale,
        color,
        alpha,
        size,
        image,
        num,
        sequence,
        state,
        grey,
        progress,
        max,
    }
    public static string[] TypeName = new string[] { "无","可见","位置","旋转","缩放","颜色","透明","大小","图片","数字","序列","状态","变灰","进度"};//尽量填两个字以下

    #region 框架
    public enum PlayType
    {
        Once,
        Loop,
        PingPong,
    }
    public static string[] PlayTypeName = new string[] { "单次", "循环", "来回"};
    
    const float init_time = -1f;

    public float m_delay=0f;                //延迟时间
    public bool ingore = false;             //禁止执行，一般用于界面调试
    public float m_duration=1f;             //持续时间，对loop和pingpong来说是单次的持续时间
    public int m_endCount=-1;               //播放次数，这里只对loop或者PingPong有影响，对Once始终认为是一次，如果是-1那么就是无数次
    public bool m_isRealtime=false;         //是不是不受时间缩放影响
    public float m_rate=1f;                 //播放倍数
    public PlayType m_playType= PlayType.Once;  //播放类型
    public AnimationCurve m_animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));//变化曲线
    public bool m_isUseNowStart = false;    //是否将使用作用对象当前的值作为开始值(比如是改变位置的处理，那么就用当前位置为开始值)
    public bool m_isDurationInvalid = false;//是不是即时执行到结束
    
    public System.Action m_onEnd;
    
    
    float m_beginTime = init_time;
    bool m_isCatchUseNowStart = false;

    //是不是持续性的
    public bool IsDurationValid { get { return CurHandle == null ? false : !m_isDurationInvalid&&CurHandle.IsDurationValid; } }

    //是不是播放次数有效的
    public bool IsEndCountValid { get { return m_playType == PlayType.Loop || m_playType == PlayType.PingPong; } }

    //真正的持续时间
    public float Duration { get { if (!IsDurationValid)return 0; else return m_duration; } }

    //是不是在播放中
    public bool IsPlaying { get { return m_beginTime != init_time; } }

    //是不是已经结束了，包含结束那一刻
    public bool IsEnd { 
        get{
            if(!IsPlaying){
                Debuger.LogError("不能在没有运行的时候调用");
                return false;
            }

            if(IsEndCountValid)
                return m_endCount!=-1&&PlayCount >= m_endCount;
            else
                return CurTime >= m_delay + Duration;
        }
    }

    //是不是已经结束了，超过结束那一刻
    public bool IsOverEnd{
        get{
            if (!IsEndCountValid)//播放单次的
                return CurTime > m_delay + Duration;
            else if(m_endCount == -1)//播放无数次的
                return false;
            else
                return CurTime > Duration * m_endCount;
        }
    }

    public float EndTime
    {
        get
        {
            if (!IsDurationValid)
                return m_delay;
            if (!IsEndCountValid)//播放单次的
                return m_delay + Duration;
            else if (m_endCount == -1)//播放无数次的
                return m_delay + Duration;
            else
                return Duration * m_endCount;
        }
    }


    //开始运行后多久
    public float CurTime{
        get{
            if (!IsPlaying)
            {
                //Debuger.LogError("没有在运行过程中调用CurTime");
                return m_delay;
            }

            return (Now - m_beginTime) * m_rate;
        }
        set{
            m_beginTime = Now - value / m_rate;
        }
    }

    //运行了多少比例
    public float CurFactor
    {
        get
        {
            float curTime =CurTime - m_delay;
            if (curTime < 0)
                return 0;
            else if(Duration ==0)
                return 1;

            else if (m_playType == PlayType.Once)
            {
                return Mathf.Clamp01(curTime / Duration);//这里超出是有可能的，所以要clamp下
            }
            else if (m_playType == PlayType.Loop)
            {
                float factor = curTime / Duration;
                return  factor - Mathf.Floor(factor);
            }
            else if (m_playType == PlayType.PingPong)
            {
                float factor = curTime / Duration;
                bool isOdd = Mathf.FloorToInt(factor)%2 == 1;//奇数次
                factor =factor - Mathf.Floor(factor); 
                return isOdd?1f-factor:factor;
            }
            else
            {
                Debuger.LogError("未实现的类型:" + m_type);
                return 0;
            }
        }
        set
        {
            if (value > 1f || value < 0)
            {
                Debuger.LogError("无效的值，必须在0~1之间。value=" + value);
                return;
            }

            CurTime = value * Duration + m_delay;
        }
    }
    
    //正在播放第几次
    public int PlayCount
    {
        get
        {
            if (Duration == 0 || !IsEndCountValid)
                return 0;
            float curTime =CurTime - m_delay;
            if (curTime < 0)
                return 0;
            else 
            {
                float factor = curTime / Duration;
                return  Mathf.FloorToInt(factor);                
            }
        }
    }

    float Now
    {
        get
        {
            if (Application.isPlaying)
            {
                return m_isRealtime ? Time.unscaledTime : Time.time;
            }
            else
            {
                var span = System.DateTime.Now.TimeOfDay;//这里怕totalsceonds转成float的时候超出float的最大值，所以先减下
                return (float)span.TotalSeconds;
            }
        }
    }

    
    //设置到某一帧
    public void SetTime(float time,bool ingoreDelay=false,bool stopIfPlay = false,bool endIfOverEnd = false)
    {
        
        if(ingoreDelay == false && time < m_delay)//还在delay中的话不用做
            return;
        bool isPlaying = IsPlaying;
        CurTime = ingoreDelay ? time + m_delay : time;
        bool isOverEnd =IsOverEnd;

        if (endIfOverEnd && isOverEnd)//已经结束的话，看是不是需要设置为结束
        {
            if (!ingore) 
                CurHandle.Update(this, m_animationCurve.Evaluate(1f), stopIfPlay);
            EditorUtil.SetDirty(m_go);//编辑器下没有运行的时候可能不会刷新,这里手动调用下
        }
        else if (!isOverEnd && CurTime >= m_delay&&CurHandle != null)//没有结束而且超过延迟
        {
            if (!ingore) 
                CurHandle.Update(this, m_animationCurve.Evaluate(CurFactor), stopIfPlay);
            EditorUtil.SetDirty(m_go);//编辑器下没有运行的时候可能不会刷新,这里手动调用下
        }
        
            
        if (!isPlaying || stopIfPlay)
        {
            Clear();
        }
       
    }

    //设置到某一比例，注意如果这里是循环或者乒乓的话可能会使次数减少，最好用SetTime替代
    public void SetFactor(float factor, bool stopIfPlay = false)
    {
        
        bool isPlaying = IsPlaying;
        CurFactor = factor;
        if (CurHandle != null)
        {
            if(!ingore)
                CurHandle.Update(this, m_animationCurve.Evaluate(factor), stopIfPlay);
        }
            
        if (!isPlaying || stopIfPlay)
        {
            Clear();
        }   
    }

    //重置到一开始，但是不运行
    public void Reset(bool stopIfPlay=false)
    {
        SetTime(0, true, stopIfPlay);
    }

    //清空,一般作为停止函数调用
    public void Clear()
    {
        m_isCatchUseNowStart = false;
        m_beginTime = init_time;
    }

    //开始,如果结束那么返回true
    public void Start() { 
        Clear();
        m_beginTime = Now;

        HandleSequence seqHandle = null;
        if (CurHandle != null)
        {
            if (_type == Type.sequence)
            {
                seqHandle = CurHandle as HandleSequence;
                seqHandle.Start(this);
            }
            else
                CurHandle.OnStart(this);
        }

        if (CurHandle != null && m_isUseNowStart)
        {
            if (!ingore)
            {
                CurHandle.OnUseNowStart(this);
            }
            m_isCatchUseNowStart = true;
        }
            
        if (IsEnd)
        {
            End();
        }
    }

    //执行
    public void End()
    {
        if (CurHandle != null )
        {
            CurHandle.OnStart(this);

            //直接设置到结束的，并且需要用当前值作开始值的，这里得计算下当前值，不然下面的Update()计算出来的值可能不正确
            if (m_isUseNowStart && !m_isCatchUseNowStart)
            {
                if (!ingore)
                    CurHandle.OnUseNowStart(this);
                m_isCatchUseNowStart = true;
            }
            if (!ingore)
                CurHandle.Update(this, m_animationCurve.Evaluate(1f), false); 
            EditorUtil.SetDirty(m_go);
        }

        Clear();
        if(m_onEnd!=null)
            m_onEnd();
    }

    //更新
    public void Update() {
        if (!IsPlaying)
            return;

        //Debuger.LogError(string.Format("now:{0} curTime:{1}",Now,CurTime));
        if (IsEnd)
            End();
        else
        {
            if (CurTime >= m_delay && CurHandle != null)
            {
                if (!ingore)
                    CurHandle.Update(this, m_animationCurve.Evaluate(CurFactor), false); 
                EditorUtil.SetDirty(m_go);
            }
                
        }
    }

    
#if UNITY_EDITOR
    //绘制ui
    public void OnGUI(Component comp, System.Action<WndType, object> onOpenWnd, bool setGoWhenChangeType = true)
    {
        //类型
        GUI.changed = false;
        int type= UnityEditor.EditorGUILayout.Popup("类型", (int)m_type,TypeName);
        if (GUI.changed)
        {
            EditorUtil.RegisterUndo("Handle Change", comp);
            if (m_go == null && setGoWhenChangeType)
            {
                m_go = comp.gameObject;
            }
            SetType((Type)type);
            EditorUtil.SetDirty(comp);
        }

        //具体内容
        if (CurHandle != null)
        {
            CurHandle.OnDrawGo(comp, this);
            CurHandle.OnDraw(comp, this, onOpenWnd);
        }
    }
#endif
    #endregion



    #region 功能
    //数据
    public int m_id;//对于父节点来说的唯一标识
    public GameObject m_go;
    public bool m_b1 = true;
    public string m_s1 = "";
    public Sprite m_sprite1;

    public float m_fBegin = 0f;
    public float m_fEnd = 1f;
    public int m_iBegin = 0;
    public int m_iEnd = 1;
    public Color m_cBegin= Color.white;
    public Color m_cEnd = Color.white;
    public Vector3 m_vBegin = Vector3.zero;
    public Vector3 m_vEnd = Vector3.zero;

    public Type m_type;

    

    //临时数据
    [System.NonSerialized]public float m_fBeginNow = 0f;
    [System.NonSerialized]public int m_iBeginNow = 0;
    [System.NonSerialized]public Color m_cBeginNow = Color.white;
    [System.NonSerialized]public Vector3 m_vBeginNow=Vector3.zero;
    [System.NonSerialized]public bool m_seqIsShow = false;//用于序列编辑器的界面计算，子处理详细页面上是否显示
    [System.NonSerialized]public bool m_seqIsExpand = false;//用于序列编辑器的界面计算，右边时间轴上是否展开
    public Type CurType { get{return m_type;}}
     
    public string CurTypeName { get{return TypeName[(int)m_type];}}
    public void SetType(Type type){
        m_type = type;
#if UNITY_EDITOR
        if (CurHandle!=null && Application.isEditor && !UnityEditor.EditorApplication.isPlaying)
        {
            CurHandle.OnReset(this);
        }    
#endif
    }

    Type _type = Type.none;
    IHandle _handle = null;
    public IHandle CurHandle{
        get{
            if ((_handle == null && _type != Type.none) ||
                _type != m_type)
            {
                _type = m_type;
                switch (m_type)
                {
                    case Type.none: _handle = null;break;
                    case Type.visiable: _handle = new HandleVisiable(); break;
                    case Type.position: _handle = new HandlePosition(); break;
                    case Type.rotate: _handle = new HandleRotate(); break;
                    case Type.scale: _handle = new HandleScale(); break;
                    case Type.color: _handle = new HandleColor(); break;
                    case Type.alpha: _handle = new HandleAlpha(); break;
                    case Type.size: _handle = new HandleSize(); break;
                    case Type.image: _handle = new HandleImage(); break;
                    case Type.num: _handle = new HandleNum(); break;
                    case Type.sequence: _handle = new HandleSequence(); break;
                    case Type.state: _handle = new HandleState(); break;
                    case Type.grey: _handle = new HandleGrey(); break;
                    case Type.progress: _handle = new HandleProgressEx(); break;
                    default: Debuger.LogError("未知的类型"); _handle = null;break;
                }
            }
            return _handle;
        }
    
    }
    
    #endregion
    

    
}
