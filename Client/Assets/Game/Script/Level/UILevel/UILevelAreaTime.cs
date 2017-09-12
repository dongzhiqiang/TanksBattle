#region Header
/**
 * 名称：UILevelAreaHead
 
 * 日期：2016.1.13
 * 描述：
 **/
#endregion
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class UILevelAreaTime : UILevelArea
{
    
    #region Fields
    public TextEx m_time;
   
    int m_EndTime;        //设置的限制时间
    bool m_pause;         //是否暂停标记
    float m_startTime;     //开始时间
    int m_overTime;       //恢复时间后走的时间
    int m_PauseTime;     //暂停时的时间
    bool m_isEnd;         //标记是否结束
    int m_obPause;
    int m_obExit;
    #endregion

    #region Properties
    public override enLevelArea Type { get { return enLevelArea.time; } }
    public override bool IsOpenOnStart { get{return false;} }

    bool Pause { 
        get { 
            return m_pause; 
        }
        set {
            m_pause = value;
        }
    }
    #endregion

    #region Frame
    //首次初始化的时候调用
    protected override void OnInitPage()
    {
        Pause = true;
        m_EndTime = 0;
        m_PauseTime = 0;
    }

    //显示
    protected override void OnOpenArea(bool reopen)
    {
        m_obPause = EventMgr.AddAll(MSG.MSG_FRAME, MSG_FRAME.FRAME_PAUSE_CHANGE, OnGamePauseChange);
        m_obExit = EventMgr.AddAll(MSG.MSG_SCENE, MSG_SCENE.EXIT, OnSceneExit);

        if (reopen)
        {
            //没结束才会恢复计时
            if (!m_isEnd)
                OnPause(false);
        }
        else
        {
            Clear();
            OnStart();
        }
    }

    protected override void OnUpdateArea()
    {
        int passTime = 0;
        if (!Pause)
        {
            passTime = (int)(TimeMgr.instance.logicTime - m_startTime);
            m_overTime = passTime + m_PauseTime;
        }

        int showTime = m_overTime;

        if (m_EndTime > 0)
        {
            if (m_EndTime - m_overTime <= 0)
            {
                showTime = 0;
                Pause = true;
                Clear();

                m_isEnd = true;
                LevelMgr.instance.CurLevel.OnTimeout(m_EndTime);
            }
            else
                showTime = m_EndTime - m_overTime;
        }
        
        m_time.text = StringUtil.SceIntToMinSceStr(showTime);
    }

    //关闭
    protected override void OnCloseArea()
    {
        OnPause(true);
        if (m_obExit != EventMgr.Invalid_Id) { EventMgr.Remove(m_obExit); m_obExit = EventMgr.Invalid_Id; }
        if (m_obPause != EventMgr.Invalid_Id) { EventMgr.Remove(m_obPause); m_obPause = EventMgr.Invalid_Id; }
        
    }

    protected override void OnRoleBorn()
    {
       
    }

    public void OnStart()
    {
        m_startTime = TimeMgr.instance.logicTime;
        Pause = false;
        m_PauseTime = 0;
        m_overTime = 0;
    }

    public void SetTime(int time)
    {
        Clear();
        m_EndTime = time;
        OnStart();
    }

    public void OnPause(bool bPause)
    {
        Pause = bPause;
        if (!Pause)
            m_startTime = TimeMgr.instance.logicTime;
        else
            m_PauseTime = m_overTime;
    }

    public void Clear()
    {
        m_overTime = 0;
        Pause = true;
        m_EndTime = 0;
        m_isEnd = false;
    }

    #endregion

    #region Private Methods

    void OnGamePauseChange(object param)
    {
        if (!m_isEnd)
            OnPause((bool)param);
        
    }

    void OnSceneExit()
    {
        Clear();
    }

    #endregion

    
}
