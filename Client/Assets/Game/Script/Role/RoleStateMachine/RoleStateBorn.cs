using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：角色出生状态
 
 * 日期：2015.8.25
 * 描述：播放出生动作
 * *********************************************************
 */

public class RoleStateBornCxt{
    public string bornTypeId;
    public bool rePlayAI;
    public RoleStateBornCxt(string bornTypeId,bool rePlayAI =false)
    {
        this.bornTypeId = bornTypeId;
        this.rePlayAI = rePlayAI;
    }
}
public class RoleStateBorn : RoleState
{
    #region Fields
    RoleStateBornCxt m_cxt;
    bool m_canLeave = false;
    BornLogic m_bornLogic;
    BornCfg m_bornCfg;
    
    #endregion

    #region Properties
    public override enRoleState Type { get{return enRoleState.born;}} 
    #endregion

    #region Frame
    public RoleStateBorn(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }

    
    public override void Enter(object param)
    {
        m_cxt = (RoleStateBornCxt)param;
        m_bornCfg = BornCfg.GetCfg(m_cxt.bornTypeId);
        m_bornLogic = new BornLogic();
        m_bornLogic.Init(Parent, m_bornCfg);
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        this.Enter(param);
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return m_canLeave;
    }

    public override void Leave()
    {
        if (m_bornLogic != null)
            m_bornLogic.OnExit();

        //设置下ai
        if (Parent.State == Role.enState.alive)
        {
            if (!Parent.IsHero)
            {
                if (m_cxt.rePlayAI)
                    AIPart.RePlay();
                else
                    AIPart.Play(Parent.RoleBornCxt.aiBehavior);
            }
        }

        m_bornLogic = null;
        m_cxt = null;
    }

    public override void Update()
    {
        if (m_bornLogic != null)
        {
            m_bornLogic.Update();

            CheckLeave();
        }
    }
    #endregion

    void CheckLeave()
    {
        //bossUI结束 并且 动作播完 出生结束
        if (m_bornCfg == null || (m_bornLogic != null && m_bornLogic.isEnd()))
        {
            m_canLeave = true;
            m_rsm.CheckFree();
            this.Parent.Fire(MSG_ROLE.BORN_END, null);

        }

    }
}
