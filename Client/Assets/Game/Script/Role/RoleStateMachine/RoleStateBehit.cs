using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：角色被击状态
 
 * 日期：2015.12.22
 * 描述：
 * *********************************************************
 */

public enum enBehit
{
    none = 0x00,
    behit = 0x01,//被击
    befloat = 0x02,//浮空
    beFly = 0x04,//击飞
    ground = 0x08,//倒地
    getUp = 0x10,//起身
    beGrab = 0x20,//被抓取
    beQte = 0x40,//大qte
}

public class RoleStateBehit: RoleState
{
    public static HitFloatSkillEventCfg s_fall = new HitFloatSkillEventCfg();//空中被击时的下落上下文

    #region Fields
    Dictionary<enBehit, RoleBehitSubState> m_states = new Dictionary<enBehit, RoleBehitSubState>();
    RoleBehitSubState m_curState;
    #endregion

    #region Properties
    public override enRoleState Type { get { return enRoleState.beHit; } }
    public RoleBehitSubState CurState { get{return m_curState== null?null:m_curState;}}
    public enBehit CurStateType { get { return m_curState == null ? enBehit.none : m_curState.Type; } }
    #endregion

    #region Frame
    static RoleStateBehit()
    {
        s_fall.speed = 6;//浮空初速度,上升阶段的初始速度
        s_fall.acceleratedUp = 0;//上升加速度,上升阶段的加速度
        s_fall.acceleratedDown = 0;//下落加速度,下落阶段的加速度
        s_fall.groundDuration = 0.1f;//倒地时间
    }
     
    public RoleStateBehit(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
        m_states[enBehit.behit] = new RoleSubBehit(this, enBehit.behit | enBehit.befloat | enBehit.beFly | enBehit.beGrab);
        m_states[enBehit.befloat] = new RoleSubBeFloat(this, enBehit.befloat | enBehit.beFly | enBehit.ground | enBehit.beGrab);
        m_states[enBehit.beFly] = new RoleSubBeFly(this, enBehit.beFly | enBehit.ground | enBehit.beGrab);
        m_states[enBehit.ground] = new RoleSubGround(this, enBehit.getUp | enBehit.beGrab | enBehit.beFly | enBehit.befloat);
        m_states[enBehit.getUp] = new RoleSubGetUp(this, enBehit.beGrab);
        m_states[enBehit.beGrab] = new RoleSubBeGrab(this, enBehit.none);
        m_states[enBehit.beQte] = new RoleSubBeQte(this, enBehit.none);
    }

    //如果没有碰撞不能移动
    public override bool CanEnter() { return !RSM.IsNoCollider; }

    public override void Enter(object param)
    {
        IBehitCxt cxt = (IBehitCxt)param;
        if(m_curState!=null)
        {
            Debuger.LogError("没有清空状态");
            m_curState.Cxt.Put();
        }

        //如果在空中被击，那么一律进入下落状态
        bool isAir =m_rsm.IsAir;
        if (isAir) m_rsm.IsAir = false;//退出空中状态
        if (isAir && (cxt == null || cxt.Type == enBehit.behit))
        {
            

            BeFloatCxt floatCxt = IdTypePool<BeFloatCxt>.Get();
            floatCxt.cfg = ((BehitCxt)cxt).cfg.groundFloat;
            m_curState = m_states[enBehit.befloat];
            m_curState.Cxt = floatCxt;
            m_curState.Enter();
            if (cxt != null)
            {
                cxt.Put();
                cxt = null;
            }

            return;
        }
        

        m_curState = m_states[cxt.Type];
        m_curState.Cxt = cxt;
        m_curState.Enter();
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        IBehitCxt cxt = (IBehitCxt)param;
        IBehitCxt cxtStateTo = m_curState.CanStateTo(cxt);
        if (cxtStateTo == null)
        {
            cxt.Put();//放回对象池
            return;
        }

        if(cxtStateTo != cxt)
            cxt.Put();//放回对象池


        //子状态间切换
        if (m_curState.Type != cxtStateTo.Type)
        {
            m_curState.Leave();
            m_curState.Cxt.Put();//放回对象池
            m_curState.Cxt = null;
            m_curState = m_states[cxtStateTo.Type];
            m_curState.Cxt = cxtStateTo;
            m_curState.Enter();
        }
        //子状态刷新上下文
        else
        {
            m_curState.Cxt.Put();//放回对象池
            m_curState.Cxt = cxtStateTo;
            m_curState.Do();
        }

    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return m_curState.CanParentLeave();
    }

    public override void Leave()
    {
        m_curState.ParentLeave();
        m_curState.Cxt.Put();//放回对象池
        m_curState.Cxt = null;
        m_curState = null;
    }


    public override void Update()
    {
        m_curState.Update();
        if (m_curState == null)//内部也可能会退出状态
            return;
        if (m_curState.CanParentLeave())
        {
            //死亡
            if (m_rsm.CheckDead(false))
                return;
            //移动中切移动
            else if (m_rsm.CheckMove())
                return;
            //待机
            else if (m_rsm.CheckFree())
                return;
        }
            
    }
    #endregion

    #region Private Methods
    
    #endregion

    
}
