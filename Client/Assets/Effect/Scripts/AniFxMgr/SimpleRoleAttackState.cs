#region Header
/**
 * 名称：SimpleRoleAttackState
 
 * 日期：2015.12.7
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class SimpleRoleAttackState : SimpleRoleState
{
    SimpleRole.AttackCxt m_curCxt;
    public SimpleRoleAttackState(SimpleRole r) : base(r) { }
    //进入这个状态的时候
    public override void OnEnter(SimpleRole.enState lastState, object param)
    {
        m_curCxt = (SimpleRole.AttackCxt)param;
        m_parent.Ani.Play(m_curCxt.aniName, m_curCxt.wrapMode, 0);

    }

    //用于输入检测、位移和结束判断
    public override void OnUpdate()
    {
        //输入检测
        if (m_parent.CheckBeiji())//被击
            return;

        //位移  
        if (m_curCxt.canMove && m_parent.IsMove())
            m_parent.CC.Move(m_parent.GetDir() * m_curCxt.moveSpeed * Time.deltaTime + SimpleRole.Default_Gravity_Speed * Time.deltaTime);
        if (m_curCxt.canRotate)
        {
            Vector3 euler = m_parent.transform.localEulerAngles;
            euler.y += m_curCxt.rotateSpeed * Time.deltaTime;
            m_parent.transform.localEulerAngles = euler;
        }

        //时间到
        if (m_parent.Ani.CurSt == null || m_parent.Ani.CurSt.enabled == false || (m_curCxt.duration > 0 && m_parent.Ani.CurSt.time >= m_curCxt.duration))
        {
            m_parent.GotoAuto();
        }
    }
   
    

}
