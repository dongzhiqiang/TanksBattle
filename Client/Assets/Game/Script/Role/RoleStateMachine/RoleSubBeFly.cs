using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：RoleSubBeFly
 
 * 日期：2015.12.22
 * 描述：
 * *********************************************************
 */



public class BeFlyCxt : IBehitCxt
{
    public override enBehit Type { get { return enBehit.beFly; } }
    public HitFlySkillEventCfg cfg;
}

public  class RoleSubBeFly:RoleBehitSubState
{
    #region Fields
    TranPartCxt m_tranCxt;
    bool m_isUp=true;
    #endregion

    #region Properties
    public override enBehit Type { get { return enBehit.beFly; } }
    public BeFlyCxt MyCxt { get { return (BeFlyCxt)Cxt; } }
    #endregion

    #region Frame
    public RoleSubBeFly(RoleStateBehit parent, enBehit stateTo) : base(parent, stateTo) { }


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
        if(aniPart.CurSt==null || aniPart.CurSt.name != AniFxMgr.Ani_JiFei)
            aniPart.Play(AniFxMgr.Ani_JiFei, WrapMode.Loop, Ani_Fade,1,true);

        m_tranCxt.moveType = TranPartCxt.enMove.dir;
        m_tranCxt.SetMoveDir(Vector3.up, enValidAxis.vertical);
        m_tranCxt.speed = MyCxt.cfg.speed;
        m_tranCxt.accelerate = MyCxt.cfg.acceleratedUp;
        m_isUp = true;
    }
    public override void Update()
    {
        //落地
        if (m_tranCxt.count > 0 && Parent.TranPart.IsGrounded )
        {
            GroundCxt cxt = IdTypePool<GroundCxt>.Get();
            cxt.duration = MyCxt.cfg.groundDuration;
            m_stateBehit.GotoState(cxt,false,true);
        }
        //进入下落状态
        else if (m_tranCxt.speed <= MyCxt.cfg.reverseSpeed&& m_isUp )
        {
            m_isUp = false;
            m_tranCxt.speed = (MyCxt.cfg.speedDown > 0)?0:MyCxt.cfg.speedDown;
            m_tranCxt.accelerate = MyCxt.cfg.acceleratedDown;
        }
    }

    #endregion

    #region Private Methods
    
    #endregion

    
}
