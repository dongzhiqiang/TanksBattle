using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：角色死亡状态
 
 * 日期：2015.12.23
 * 描述：播放出生动作
 * *********************************************************
 */


public class RoleStateDeadCxt
{
    public string deadTypeId;
    public bool bHeroKill;
    public RoleStateDeadCxt(string deadTypeId)
    {
        this.deadTypeId = deadTypeId;
    }
}

public class RoleStateDead : RoleState
{
    #region Fields
    RoleStateDeadCxt m_cxt;
    BornLogic m_deadLogic;
    BornCfg m_deadCfg;
    #endregion

    #region Properties
    public override enRoleState Type { get{return enRoleState.dead;}}
    #endregion

    #region Frame
    public RoleStateDead(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }

    
    public override void Enter(object param)
    {
        RenderPart.SetLayer( enGameLayer.deadCollider);
        TranPart.ResetHight();

        m_cxt = (RoleStateDeadCxt)param;
        m_deadCfg = BornCfg.GetCfg(m_cxt.deadTypeId);

        m_deadLogic = new BornLogic();
        m_deadLogic.Init(Parent, m_deadCfg);

        //m_rsm.AniPart.Play(AniFxMgr.Ani_SiWang, WrapMode.ClampForever,0.2f,1,true);
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        this.Enter(param);
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return false;
    }

    public override void Leave()
    {
        if (m_deadLogic != null)
            m_deadLogic.OnExit();

        m_deadCfg = null;
        m_deadLogic = null;
        m_cxt = null;
    }

    public override void Update()
    {
        m_deadLogic.Update();

        if (m_deadCfg == null || m_deadLogic.isEnd())
        {
            if (m_cxt != null && m_cxt.bHeroKill)
                this.Parent.Fire(MSG_ROLE.DEAD_END, null);
            RoleMgr.instance.DestroyRole(this.Parent);
            return;
        }
    }
    #endregion

}
