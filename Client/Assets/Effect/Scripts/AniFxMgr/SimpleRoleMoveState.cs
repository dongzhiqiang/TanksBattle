#region Header
/**
 * 名称：SimpleRoleMoveState
 
 * 日期：2015.12.7
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class SimpleRoleMoveState : SimpleRoleState
{
    
    Vector3 m_lastDir;
    float m_avoidBeginTime = 0;
    int m_stuckCount = 0;
    public SimpleRoleMoveState(SimpleRole r) : base(r) { }
    //进入这个状态的时候
    public override void OnEnter(SimpleRole.enState lastState, object param)
    {
        m_parent.Ani.Play(AniFxMgr.Ani_PaoBu, WrapMode.Loop, 0.2f, m_parent.m_moveSpeed / m_parent.m_moveAniSpeed);
        //按照方向移动
        if (m_parent.GetDir() != Vector3.zero)
        {
            m_lastDir = m_parent.GetDir();
            m_parent.transform.forward = m_lastDir;
        }
        //寻路
        else 
        {
            m_lastDir =m_parent.transform.forward;
            m_stuckCount =0;
            m_avoidBeginTime = 0;
        }
        
    }

    //用于输入检测、位移和结束判断
    public override void OnUpdate()
    {
        //输入检测
        if (m_parent.CheckBeiji())//被击
            return;
        if (m_parent.CheckAttack())//攻击
            return;

        if (!m_parent.IsMove())//待机
        {
            if(!m_parent.RolePath.Reached)
                m_parent.RolePath.Stop();
            m_parent.GotoState(SimpleRole.enState.free, null);
            return;
        }

        //按照方向移动
        Vector3 dir = m_parent.GetDir();
        if (dir != Vector3.zero)
        {
            //如果是寻路中切换过来的话，要停止下寻路
            if (!m_parent.RolePath.Reached)
                m_parent.RolePath.Stop();

            //方向改变的话修改方向
            if (dir != m_lastDir)
            {
                m_lastDir = dir;
                m_parent.Root.forward = m_lastDir;
            }

            //移动
            m_parent.CC.Move(dir * m_parent.m_moveSpeed * Time.deltaTime + SimpleRole.Default_Gravity_Speed * Time.deltaTime); 
        }
        //寻路
        else
        {
            //计算出寻路方向
            dir = m_parent.RolePath.CalculateOffset(m_parent.Root.position, m_parent.m_moveSpeed,Time.deltaTime);
            //m_parent.pathDir = dir;
            //寻路结束或者不用寻路了
            if (m_parent.RolePath.Reached)
            {
                dir = m_parent.GetMovePos() - m_parent.Root.position; //方向调整到正确方向
                dir.y = 0;
                if (dir != Vector3.zero)
                    m_parent.Root.forward = dir;
                m_parent.GotoState(SimpleRole.enState.free, null);
                return;
            }

            if (dir != Vector3.zero)
            {
                //方向
                Quaternion rot = m_parent.Root.rotation;
                Quaternion toTarget = Quaternion.LookRotation(dir);
                rot = Quaternion.Slerp(rot, toTarget, 7 * Time.deltaTime);
                m_parent.Root.rotation = rot;

                //位置
                Vector3 pos = m_parent.Root.position;
                m_parent.CC.Move(dir + SimpleRole.Default_Gravity_Speed * Time.deltaTime); 
                Vector3 newPos = m_parent.Root.position;
                Vector3 trueDir = newPos - pos;

                ////检测是不是卡住
                //m_parent.RolePath.CheckStuck(newPos, dir, trueDir);
                //if (m_parent.RolePath.Reached)
                //{
                //    m_parent.GotoState(SimpleRole.enState.free, null);
                //    return;
                //}
            }
        }
    }
}
