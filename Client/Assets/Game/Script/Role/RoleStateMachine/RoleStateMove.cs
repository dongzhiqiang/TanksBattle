using UnityEngine;
using System.Collections;

/*
 * *********************************************************
 * 名称：角色移动状态
 
 * 日期：2015.8.25
 * 描述：播放出生动作
 * *********************************************************
 */

public enum enRoleMoveType{
    dir,//朝某个方向移动
    pos,//朝某个点移动
}
public class RoleStateMove : RoleState
{
    #region Fields
    TranPartCxt m_cxt;
    Vector3 m_curDir = Vector3.zero;//最后的移动方向，用于其他状态结束后恢复移动状态
    Vector3 m_curPos= Vector3.zero;
    Vector3 m_lastDir = Vector3.zero;
    enRoleMoveType m_moveType;
    float m_enterTime;
    int m_curId;
    #endregion

    #region Properties
    public override enRoleState Type { get{return enRoleState.move;}}
    public bool NeedMove {get{return m_curDir != Vector3.zero;}}

    public Vector3 LastDir { get{return m_lastDir;}}

    public Vector3 CurDir { get { return m_curDir; } set { m_curPos = Vector3.zero;  m_curDir = value; if(m_curDir!=Vector3.zero)m_lastDir = m_curDir;} }

    public Vector3 CurPos { get { return m_curPos; } set { m_curPos = value; m_curDir = Vector3.zero; } }
    public float MoveTime { get{return m_enterTime==-1 ?-1:(TimeMgr.instance.logicTime-m_enterTime);}}
    #endregion

    #region Frame
    public RoleStateMove(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }

    //如果没有碰撞不能移动
    public override bool CanEnter() { return !RSM.IsNoCollider; }

    public override void Enter(object param)
    {
        if (m_cxt!=null)//检错下
            Debuger.LogError("逻辑错误");

        m_enterTime = TimeMgr.instance.logicTime;
        m_cxt = TranPart.AddCxt();
        m_curId = m_cxt.Id;
        m_cxt.moveValidAxis = enValidAxis.horizontal;
        m_cxt.speed = Parent.GetFloat(enProp.speed);
        m_cxt.dirType = TranPartCxt.enDir.forward;
        m_cxt.dirValidAxis = enValidAxis.horizontal;
        m_cxt.dirModelSmooth = true;

        if(m_cxt.speed == 0)
        {
            Debuger.LogError("角色速度为0，移动会卡住：{0}",this.Parent.Cfg.id);
            m_cxt.speed = 0.5f;
        }

        m_rsm.AniPart.Play(AniFxMgr.Ani_PaoBu, WrapMode.Loop, 0.2f, m_cxt.speed / AniPart.MoveAniSpeed,true);
        Do(param);
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        m_moveType = param == null ? enRoleMoveType.dir:(enRoleMoveType)param;
        if (m_moveType == enRoleMoveType.dir)
        {
            m_cxt.moveType = TranPartCxt.enMove.dir;
            m_cxt.SetMoveDir(m_curDir);
        }
        else{
            m_cxt.moveType = TranPartCxt.enMove.path;
            TranPart.SetPathPos(m_curPos);
        }
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return true;
    }

    public override void Leave()
    {
        if (TranPart!= null)
            TranPart.RemoveCxt(m_cxt);
        m_cxt = null;
        m_enterTime =-1;
    }

    public override void Update()
    {
        //如果寻路结束了，TranPart会销毁这个上下文
        if (m_cxt.IsDestroy(m_curId))
        {
            RSM.CheckFree();
        }
    }
    #endregion
}
