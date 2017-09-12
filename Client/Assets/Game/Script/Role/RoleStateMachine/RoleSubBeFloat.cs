using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：RoleSubBeFloat
 
 * 日期：2015.12.22
 * 描述：
 * *********************************************************
 */



public class BeFloatCxt : IBehitCxt
{
    public override enBehit Type { get { return enBehit.befloat; } }
    public HitFloatSkillEventCfg cfg;
}

public  class RoleSubBeFloat:RoleBehitSubState
{
    #region Fields
    
    TranPartCxt m_tranCxt;
    bool m_isUp = true;
    #endregion

    #region Properties
    public override enBehit Type { get { return enBehit.befloat; } }
    public BeFloatCxt MyCxt { get { return (BeFloatCxt)Cxt; } }
    #endregion

    #region Frame
    public RoleSubBeFloat(RoleStateBehit parent, enBehit stateTo) : base(parent, stateTo) { }

    //能不能切换到目标上下文
    public override IBehitCxt CanStateTo(IBehitCxt cxt)
    {
        BehitCxt behitCxt = cxt as BehitCxt;
        if (behitCxt != null)
        {
            if (behitCxt.cfg.floatSpeed > m_tranCxt.speed)
            {
                AniPart aniPart = Parent.AniPart;
                //下降状态要加回上升状态
                if (m_tranCxt.speed <= 0)
                {
                    aniPart.Play(AniFxMgr.Ani_FuKong01, WrapMode.ClampForever, Ani_Fade, 1, true);
                    m_isUp = true;
                }
                    
              
                //重设初速度
                m_tranCxt.speed = behitCxt.cfg.floatSpeed;
                m_tranCxt.accelerate = behitCxt.cfg.floatAccelerated;

            }
        }
        return base.CanStateTo(cxt);
    }

    public override bool CanParentLeave(){
        return false;
    }

    public override void Leave()
    {
        Parent.TranPart.RemoveCxt(m_tranCxt);
        m_tranCxt = null;
    }

    public override void Enter()
    {
        m_tranCxt = Parent.TranPart.AddCxt();
        
        Do();
    }

    public override void Do()
    {
        AniPart aniPart = Parent.AniPart;
        aniPart.Play(AniFxMgr.Ani_FuKong01, WrapMode.ClampForever, Ani_Fade,1,true);
        
        m_tranCxt.moveType = TranPartCxt.enMove.dir;        
        m_tranCxt.SetMoveDir(Vector3.up, enValidAxis.vertical) ;
        m_tranCxt.speed =MyCxt.cfg.speed;
        m_tranCxt.accelerate = MyCxt.cfg.acceleratedUp;
        m_isUp = true;

    }
    public override void Update()
    {
        //落地
        if (m_tranCxt.count >0&& m_tranCxt.speed <=0&& Parent.TranPart.IsGrounded)//m_parent.Model.localPosition.y <= 0
        {
            GroundCxt cxt = IdTypePool<GroundCxt>.Get();
            cxt.duration = MyCxt.cfg.groundDuration;
            m_stateBehit.GotoState(cxt,false,true);
        }
        //进入下落状态
        else if (m_tranCxt.speed <= MyCxt.cfg.reverseSpeed && m_isUp)
        {
            m_isUp = false;
            m_tranCxt.speed = (MyCxt.cfg.speedDown > 0) ? 0 : MyCxt.cfg.speedDown;
            AniPart aniPart = Parent.AniPart;
            aniPart.Play(AniFxMgr.Ani_FuKong02, WrapMode.ClampForever, Ani_Fade,1,true);
            m_tranCxt.accelerate = MyCxt.cfg.acceleratedDown;
        }
    }

    #endregion

    #region Private Methods
    
    #endregion

    
}
