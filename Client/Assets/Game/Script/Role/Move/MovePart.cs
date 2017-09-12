#region Header
/**
 * 名称：移动部件
 
 * 日期：2015.9.21
 * 描述：控制角色移动，带寻路接口
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class MovePart:RolePart
{
    #region Fields
    RoleStateMove m_moveState;
    #endregion


    #region Properties
    public Vector3 LastDir { get { return m_moveState.LastDir; } }
    public Vector3 CurDir { get { return m_moveState.CurDir; } }
    public override enPart Type { get { return enPart.move; } }
    public bool IsMoveing { get { return RSM.CurStateType == enRoleState.move; } }
    public float MoveTime { get{return m_moveState.MoveTime;}}
    #endregion


    #region Frame    
    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        m_moveState = RSM.StateMove;
    }
    #endregion


    #region Private Methods
    
    #endregion
    
    //2d移动，根据相机方向
    public bool MoveByCameraDir(Vector2 dir)
    {

        if (TimeMgr.instance.IsPause) return false;
        m_moveState.CurDir = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y)) * CameraMgr.instance.HorizontalDir;
        return m_moveState.GotoState(enRoleMoveType.dir);
    }

    public void Stop()
    {
        m_moveState.CurDir = Vector3.zero;//表示停止移动了
        if (RSM.CurStateType == enRoleState.move)
        {
            RSM.CheckFree();
        }
            
    }

    public bool MovePos(Vector3 pos)
    {
        if (TimeMgr.instance.IsPause) return false;
        //下一个寻路点和当前一样，那么就不要再寻了
        if (m_moveState.IsCur && m_moveState.CurPos != Vector3.zero && Util.XZSqrMagnitude(m_moveState.CurPos, pos) < 0.01)
            return true;
        m_moveState.CurPos = pos;
        return m_moveState.GotoState(enRoleMoveType.pos);
    }

    public bool IsMovingToPos(Vector3 pos)
    {
        return m_moveState.IsCur && m_moveState.CurPos == pos;
    }
}
