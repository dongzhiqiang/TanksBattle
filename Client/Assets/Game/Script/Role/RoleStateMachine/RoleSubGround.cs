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



public class GroundCxt : IBehitCxt
{
    public override enBehit Type { get { return enBehit.ground; } }
    public float duration;
}

public  class RoleSubGround:RoleBehitSubState
{
    #region Fields
    float m_groundEndTime = 0;
    float m_groundMoveTime = 0;
    TranPartCxt m_tranCxt;
    #endregion

    #region Properties
    public override enBehit Type { get { return enBehit.ground; } }
    public GroundCxt MyCxt { get { return (GroundCxt)Cxt; } }
    #endregion

    #region Frame
    public RoleSubGround(RoleStateBehit parent, enBehit stateTo) : base(parent, stateTo) { }


    public override bool CanParentLeave(){
        AnimationState st = Parent.AniPart.CurSt;
        return (st == null || TimeMgr.instance.logicTime >= m_groundEndTime || st.normalizedTime >= 1)  && Parent.DeadPart.Check();
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

    public override IBehitCxt CanStateTo(IBehitCxt cxt)
    {
        if(CanMove())
        {
            BeFloatCxt beFloatCxt = cxt as BeFloatCxt;
            if (beFloatCxt != null)
                return beFloatCxt;

            BeFlyCxt beFlyCxt = cxt as BeFlyCxt;
            if (beFlyCxt != null)
                return beFlyCxt;

            BehitCxt behitCxt = cxt as BehitCxt;
            if (behitCxt != null)
            {
                BeFloatCxt newCxt = IdTypePool<BeFloatCxt>.Get();
                newCxt.cfg = behitCxt.cfg.groundFloat;
                return newCxt;
            }
        }
       

        return base.CanStateTo(cxt);
    }

    public override void Do()
    {
        AniPart aniPart = Parent.AniPart;
        aniPart.Play(AniFxMgr.Ani_DaoDi, WrapMode.ClampForever, Ani_Fade, 1f, true);
        Parent.TranPart.ResetHight();

        var st = aniPart.CurSt;
        m_groundEndTime = TimeMgr.instance.logicTime + MyCxt.duration;
        float aniEndTime = TimeMgr.instance.logicTime + (st == null?0.1f: st.length);

        if (m_groundEndTime > aniEndTime)
            m_groundMoveTime = aniEndTime;
        else
            m_groundMoveTime = m_groundEndTime - 0.05f;
    }

    public override void Update()
    {
      
        if (!Parent.DeadPart.Check() &&TimeMgr.instance.logicTime >= m_groundEndTime)
        {
           
            GetUpCxt cxt = IdTypePool<GetUpCxt>.Get();
            m_stateBehit.GotoState(cxt,false,true);
        }
    }

    #endregion

    #region Public Methods
    public bool CanMove()
    {
        return TimeMgr.instance.logicTime < m_groundMoveTime;
    }
    #endregion

    
}
