using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：角色空状态
 
 * 日期：2016.6.27
 * 描述：用于没有动作的角色
 * *********************************************************
 */

public class RoleStateEmpty : RoleState
{
    #region Fields

    #endregion

    #region Properties
    public override enRoleState Type { get{return enRoleState.free;}}
    #endregion

    #region Frame
    public RoleStateEmpty(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }



    public override void Enter(object param)
    {
        
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return false;
    }

    public override void Leave()
    {

    }

    public override void Update()
    {

    }
    #endregion
}
