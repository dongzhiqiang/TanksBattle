#region Header
/**
 * 名称：SimpleRoleFreeState
 
 * 日期：2015.12.7
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class SimpleRoleFreeState : SimpleRoleState
{
    public SimpleRoleFreeState(SimpleRole r) : base(r) { }
    //进入这个状态的时候
    public override void OnEnter(SimpleRole.enState lastState,object param){
        m_parent.Ani.Play(AniFxMgr.Ani_DaiJi, WrapMode.Loop, 0.2f);
    }

    //用于输入检测、位移和结束判断
    public override void OnUpdate()
    {
        //输入检测
        if (m_parent.CheckBeiji())//被击
            return;
        if(m_parent.CheckAttack())//攻击
            return;
        if(m_parent.IsMove())//移动
        {   
            m_parent.GotoState(SimpleRole.enState.move,null);
            return;
        }


        //位移
        //controller.collisionFlags & CollisionFlags.Below  
        //未实现
    }
   
    

}
