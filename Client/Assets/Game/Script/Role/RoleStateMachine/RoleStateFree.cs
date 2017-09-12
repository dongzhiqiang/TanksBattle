using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：角色待机状态
 
 * 日期：2015.8.25
 * 描述：播放出生动作
 * *********************************************************
 */

public class RoleStateFree : RoleState
{
    #region Fields

    #endregion

    #region Properties
    public override enRoleState Type { get{return enRoleState.free;}}
    #endregion

    #region Frame
    public RoleStateFree(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }



    public override void Enter(object param)
    {
        m_rsm.AniPart.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop,0.2f,1,true);
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        if(param == null)return;
        bool force = (bool)param;//切换武器后要强制进入这个动作
        if(force)
            Enter(param);
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return true;
    }

    public override void Leave()
    {

    }

    public override void Update()
    {

    }
    #endregion
}
