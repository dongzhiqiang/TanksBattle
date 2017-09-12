using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：角色被击状态的子状态基类
 
 * 日期：2015.12.22
 * 描述：
 * *********************************************************
 */


public abstract class IBehitCxt:IdType
{
    public abstract enBehit Type { get; }
}


public abstract class RoleBehitSubState
{
    #region Fields
    public const float Ani_Fade = 0.2f;
    protected RoleStateBehit m_stateBehit;
    protected enBehit m_stateTo;//可以跳转到的状态
    protected IBehitCxt m_cxt;
    #endregion

    #region Properties
    public abstract enBehit Type { get ; }
    
    public IBehitCxt Cxt { get{return m_cxt;} set{m_cxt=value;}}
    public Role Parent { get{return m_stateBehit.Parent;}}
    #endregion

    #region Frame
    public RoleBehitSubState(RoleStateBehit stateBehit,enBehit stateTo)
    {
        m_stateBehit = stateBehit;
        m_stateTo = stateTo;
    }

    //能不能切换到目标上下文
    public virtual IBehitCxt CanStateTo(IBehitCxt cxt)
    {
        return (m_stateTo & cxt.Type) != 0 ? cxt : null;
    }

    //父状态能不能退出
    public abstract bool CanParentLeave();

    //父状态退出回调
    public virtual void ParentLeave() {Leave(); }

    //退出回调
    public abstract void Leave();

    //进入回调
    public abstract void Enter();

    //同一个子状态上下文改变回调
    public abstract void Do();

    //每帧更新
    public abstract void Update();
    #endregion

    #region Private Methods
    
    #endregion

    
}
