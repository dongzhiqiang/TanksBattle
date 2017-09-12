using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：角色战斗状态
 
 * 日期：2015.8.31
 * 描述：
 * *********************************************************
 */

public class RoleStateCombat : RoleState
{
    #region Fields
    public Skill m_curSkill;
    public Skill m_lastSkill;
    public Skill m_pressSkill;
    
    TranPartCxt m_tranCxt;
    int m_tranCxtId;
    bool m_isTranBegin=false;
    #endregion

    #region Properties
    public override enRoleState Type { get{return enRoleState.combat;}}
    //当前技能，如果是内部的，那么这里会返回它的所属技能
    public Skill CurSkill { get { return (m_curSkill == null || m_curSkill.InternalParentSkill == null) ? m_curSkill : m_curSkill.InternalParentSkill;  } }
    //当前技能
    public Skill CurSkillSelf { get { return m_curSkill; } }
    //最后一次使用的技能,如果是内部的，那么这里会返回它的所属技能
    public Skill LastSkill { get{return (m_lastSkill == null || m_lastSkill.InternalParentSkill == null) ? m_lastSkill : m_lastSkill.InternalParentSkill; }}
    //最后一次使用的技能
    public Skill LastSkillSelf { get { return m_lastSkill; } }
    //用于判断一个技能的技能键是不是按紧的，有的技能是按紧的情况下不结束
    public Skill PressSkill { get{return m_pressSkill;} set{m_pressSkill = value;}}
    #endregion

    #region Frame
    public RoleStateCombat(RoleStateMachine rsm, enRoleState enterType)
        : base(rsm, enterType)
    {
            
    }

    //被对象池回收的时候
    public override void OnDestroy() {
        m_curSkill = null;
        m_lastSkill = null;
        m_pressSkill = null;
    }
    public override void Enter(object param)
    {
        if (m_curSkill != null)
            Debuger.LogError("逻辑错误，当前技能不为空");

        m_curSkill = (Skill)param;
        m_lastSkill = m_curSkill;
        SkillCfg skillCfg = m_curSkill.Cfg;

        //有敌人的话自动朝向
        Role r =m_curSkill.Target;
        if(!m_curSkill.Cfg.autoFace)
        {

        }
        else if (r != null)
        {
            Parent.TranPart.SetDir(r.transform.position-Parent.transform.position);
        }
        else
        {
            //如果有摇杆方向，朝摇杆方向
            Vector3 lastDir = StateMove.CurDir;
            if (lastDir != Vector3.zero)
            {
                lastDir.y = 0;
                Parent.TranPart.SetDir(lastDir);
            }
        }

        //技能中移动和朝向支持
        m_isTranBegin = false;
        if (m_tranCxt != null)
        {
            if (TranPart!= null)
                TranPart.RemoveCxt(m_tranCxt);
            m_tranCxt = null;
            m_tranCxtId =-1;
        }
        if (skillCfg.beginTranFrame == 0 && !m_isTranBegin)
        {
            BeginMoveAndRot();
            UpdateMoveAndRot();
        }
            
        m_curSkill.Play();
    }

    //重新传递参数给当前状态,比如走动中换方向，使用技能时强制使用第二个技能
    public override void Do(object param)
    {
        if (m_curSkill != null)
        {
            m_curSkill.Stop(enSkillStop.cancel);
            m_curSkill = null;
        }
        Enter(param);
            
    }

    //判断能不能离开
    public override bool CanLeave(RoleState nextState)
    {
        return m_curSkill == null || !m_curSkill.IsPlayingSelf;
    }

    public override void Leave()
    {
        CheckEndMoveAndRot();
        if (m_curSkill != null && !m_curSkill.IsInPool && m_curSkill.IsPlayingSelf)
            m_curSkill.Stop(enSkillStop.behit);//这里直接认为是被击的就可以了，因为如果是自行结束的技能的话，IsPlayingSelf为false不会执行到这里
        m_curSkill = null;
    }

    public override void Update()
    {
        Role r = Parent;
        int poolId = r.Id;

        //技能的更新
        m_curSkill.Update();

        //FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
        if (r.IsDestroy(poolId))
            return;

        //如果切换了状态要及时返回
        if(!this.IsCur)
            return;

        //连击和取消
        if (m_curSkill.PlayCancelBuff())   
            return;
        if (m_curSkill.PlayComboBuff())
            return;


        //结束判断
        if (!m_curSkill.IsPlayingSelf)
        {
            //下落
            if (m_rsm.CheckFall())
                return;
            //移动中切移动
            else if (m_rsm.CheckMove())
                return;
            //待机
            else if(m_rsm.CheckFree())
                return;
        }


        //移动和方向
        int frame = m_curSkill.CurFrame;
        if (m_curSkill.Cfg.endTranFrame != -1 && frame >=m_curSkill.Cfg.endTranFrame)
        {
            CheckEndMoveAndRot();
        }
        else if (frame >= m_curSkill.Cfg.beginTranFrame )
        {
            if(!m_isTranBegin)
                BeginMoveAndRot();
            UpdateMoveAndRot();
        }            
    }
    #endregion

    #region Private Methods
    void BeginMoveAndRot()
    {
        
        SkillCfg skillCfg = m_curSkill.Cfg;
        if(m_isTranBegin){
            Debuger.LogError("{0} {1}.逻辑错误，技能期间重复计算移动和方向", Parent.Cfg.id, skillCfg.skillId);
            return;
        }
        m_isTranBegin =true;
        
        if (skillCfg.moveType != SkillCfg.enMoveType.none || skillCfg.dirType != SkillCfg.enDirType.none)
        {
            m_tranCxt = TranPart.AddCxt();
            if (m_tranCxt == null)
                return;
            m_tranCxtId = m_tranCxt.Id;

            switch (skillCfg.moveType)
            {
                case SkillCfg.enMoveType.none: m_tranCxt.moveType = TranPartCxt.enMove.none; break;
                case SkillCfg.enMoveType.joystick: m_tranCxt.moveType = TranPartCxt.enMove.dir; break;
                case SkillCfg.enMoveType.keepMove:
                    {
                        Vector3 d = RSM.StateMove.CurDir;
                        if (d != Vector3.zero)
                            m_tranCxt.SetMoveDir(d);
                        else
                            m_tranCxt.SetMoveDir(Parent.transform.forward);

                        m_tranCxt.speed = skillCfg.moveSpeed;
                        m_tranCxt.moveType = TranPartCxt.enMove.dir;
                    } break;
                case SkillCfg.enMoveType.findTarget:
                    {
                        Vector3 d = RSM.StateMove.CurDir;
                        if (d != Vector3.zero)
                            m_tranCxt.SetMoveDir(d);
                        else
                        {
                            Role grabRole = Parent.GetGrabTarget();
                            Role target = RoleMgr.instance.GetClosestTarget(Parent, enSkillEventTargetType.enemy, false, false, grabRole);
                            if (target == null)
                                m_tranCxt.SetMoveDir(Parent.transform.forward);
                            else
                            {
                                Vector3 dir = target.transform.position - Parent.transform.position;
                                m_tranCxt.SetMoveDir(dir);
                            }
                        }

                        m_tranCxt.speed = skillCfg.moveSpeed;
                        m_tranCxt.moveType = TranPartCxt.enMove.dir;
                    }break;
                default: Debuger.LogError("{0} {1}.未知的移动类型：{2} ", Parent.Cfg.id, skillCfg.skillId, skillCfg.moveType); m_tranCxt.moveType = TranPartCxt.enMove.none; break;
            }

            switch (skillCfg.dirType)
            {
                case SkillCfg.enDirType.none: m_tranCxt.dirType = TranPartCxt.enDir.none; break;
                case SkillCfg.enDirType.keepRotate: m_tranCxt.dirType = TranPartCxt.enDir.dir; break;
                case SkillCfg.enDirType.forward: m_tranCxt.dirType = TranPartCxt.enDir.forward; break;
                default: Debuger.LogError("{0} {1}.未知的移动类型：{2} ", Parent.Cfg.id, skillCfg.skillId, skillCfg.moveType); m_tranCxt.moveType = TranPartCxt.enMove.none; break;
            }
        }
    }

    void UpdateMoveAndRot()
    {
        if(m_tranCxt == null)
            return;

        SkillCfg skillCfg = m_curSkill.Cfg;
        if (m_tranCxt != null && m_tranCxt.IsDestroy(m_tranCxtId))
            m_tranCxt = null;
        if (m_tranCxt == null)
            return;

        m_tranCxt.speed = skillCfg.moveSpeed;
        switch (skillCfg.moveType)
        {
            case SkillCfg.enMoveType.none: break;
            case SkillCfg.enMoveType.joystick:
                {
                    Vector3 dir = RSM.StateMove.CurDir;
                    if (dir != Vector3.zero)
                    {
                        m_tranCxt.SetMoveDir(dir);
                        m_tranCxt.speed = skillCfg.moveSpeed;
                    }
                    else
                        m_tranCxt.speed = 0;
                }; break;
            case SkillCfg.enMoveType.keepMove:
                {
                    Vector3 dir = RSM.StateMove.CurDir;
                    if (dir != Vector3.zero)
                        m_tranCxt.SetMoveDir(dir);
                }; break;
            case SkillCfg.enMoveType.findTarget:
                {
                    Vector3 dir = RSM.StateMove.CurDir;
                    if (dir != Vector3.zero)
                        m_tranCxt.SetMoveDir(dir);
                    else
                    {
                        Role grabRole = Parent.GetGrabTarget();
                        Role target = RoleMgr.instance.GetClosestTarget(Parent, enSkillEventTargetType.enemy, false, false, grabRole);

                        if (target == null)
                            m_tranCxt.SetMoveDir(Parent.transform.forward);
                        else
                        {
                            dir = target.transform.position - Parent.transform.position;
                            m_tranCxt.SetMoveDir(dir);
                        }
                    }
                }; break;
            default: Debuger.LogError("{0} {1}.未知的移动类型：{2} ", Parent.Cfg.id, skillCfg.skillId, skillCfg.moveType); m_tranCxt.moveType = TranPartCxt.enMove.none; break;
        }

        switch (skillCfg.dirType)
        {
            case SkillCfg.enDirType.none:  break;
            case SkillCfg.enDirType.keepRotate: {
                m_tranCxt.SetDirDir(Quaternion.Euler(0, skillCfg.rotateSpeed * TimeMgr.instance.delta, 0) * RoleModel.Root.forward);
            } break;
            case SkillCfg.enDirType.forward: break;
            default: Debuger.LogError("{0} {1}.未知的移动类型：{2} ", Parent.Cfg.id, skillCfg.skillId, skillCfg.moveType); m_tranCxt.moveType = TranPartCxt.enMove.none; break;
        }

    }

    void CheckEndMoveAndRot()
    {
        if (m_tranCxt != null)
        {
            if (TranPart != null)
                TranPart.RemoveCxt(m_tranCxt);
            m_tranCxt = null;
            m_tranCxtId = -1;
        }
    }
    #endregion

    
}
