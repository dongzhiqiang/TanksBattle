using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：RoleSubBehit
 
 * 日期：2015.12.22
 * 描述：
 * *********************************************************
 */



public class BehitCxt : IBehitCxt
{
    public override enBehit Type { get{return enBehit.behit;} }
    public HitSkillEventCfg cfg;
    public float duration;//僵直时间
}

public  class RoleSubBehit:RoleBehitSubState
{
    #region Fields
    bool m_behitToggle = false;
    float m_behitEndTime = 0;
    float m_lastBehitTime= 0;
    #endregion

    #region Properties
    public override enBehit Type { get{return enBehit.behit;}  }
    public BehitCxt MyCxt { get { return (BehitCxt)Cxt; } }
    #endregion

    #region Frame
    public RoleSubBehit(RoleStateBehit parent,enBehit stateTo):base(parent,stateTo){}


    public override bool CanParentLeave(){
        return TimeMgr.instance.logicTime >= m_behitEndTime;
    }

    public override void Leave()
    {

    }

    public override void Enter()
    {
        m_lastBehitTime= 0;
        
        Do();
    }

    public override void Do()
    {
        AniPart aniPart = Parent.AniPart;
        float t = TimeMgr.instance.logicTime;
        if(t - m_lastBehitTime <0.1f )//0.1s内动作不切换
            return;
        if ( t+ MyCxt.duration <=m_behitEndTime  )//被击结束时间比当前短，不切换
            return;

        //首次被击不渐变，不然反应太慢
        float fade = 0;
        string curAni = aniPart.CurSt.name;
        if (curAni == AniFxMgr.Ani_BeiJi01 || curAni == AniFxMgr.Ani_BeiJi02)
            fade = Ani_Fade;


        aniPart.Play(m_behitToggle ? AniFxMgr.Ani_BeiJi01 : AniFxMgr.Ani_BeiJi02, WrapMode.ClampForever, fade, 1,true);
        m_lastBehitTime = t;
        m_behitToggle = !m_behitToggle;
        m_behitEndTime = t + MyCxt.duration;
    }
    public override void Update()
    {

    }

    #endregion

    #region Private Methods
    
    #endregion

    
}
