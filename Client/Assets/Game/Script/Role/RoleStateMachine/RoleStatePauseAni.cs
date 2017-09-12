using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：定身状态
 
 * 日期：2016.3.8
 * 描述：定身开始时动作将停留在当前的姿势不动，定身期间不会被其他行为(被击、浮空、移动等，除了死亡)中断
 * *********************************************************
 */


public class RoleStatePauseAni : RoleState
{
    #region Fields
    bool m_canLeave = false;
    object m_cxt = null; 
    #endregion

    #region Properties
    public override enRoleState Type { get { return enRoleState.pauseAni; } }
    
    #endregion

    #region Frame
    public RoleStatePauseAni(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }



    public override void Enter(object param)
    {
        Do(param);
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        m_canLeave = false;
        m_cxt = param;
        AniPart.AddPause(10000);
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return m_canLeave;
    }

    public override void Leave()
    {
        m_cxt = null;
        AniPart.ResetPause();
    }

    public override void Update()
    {
        
    }
    //被对象池回收的时候
    public override void OnDestroy() {
        
    }
    #endregion    


    public void CheckLeave(object cxt)
    {
        if (!IsCur|| m_cxt == null || m_cxt != cxt) return;
        m_canLeave = true;
        RSM.CheckFree();
    }
}
