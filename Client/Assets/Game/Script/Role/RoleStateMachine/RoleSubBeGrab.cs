using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
/*
 * *********************************************************
 * 名称：RoleSubBeBrab
 
 * 日期：2016.3.10
 * 描述：被抓取状态
 * *********************************************************
 */



public class BeGrabCxt : IBehitCxt
{
    public override enBehit Type { get { return enBehit.beGrab; } }
    public GrabCxt grabCxt;
    public Skill parentSkill;
    int grabRoleId;
    Role grabRole;

    public Role GrabRole {
        get { return grabRole == null ||grabRole.IsDestroy(grabRoleId) || grabRole.State != Role.enState.alive ? null : grabRole; }
        set
        {
            if (value == null || value.IsInPool || value.State != Role.enState.alive)
            { 
                Debuger.LogError("逻辑错误，抓取上下文的抓取者为空或者已经死亡");
                grabRole =null;
                grabRoleId = -1;
                return;
            }
            grabRole = value;
            grabRoleId = grabRole.Id;

        }    
    }
}

public  class RoleSubBeGrab:RoleBehitSubState
{
    #region Fields
    bool m_isPlaying=false;
    float m_beginTime ;
    int m_frameLastTime;
    int m_curPosCxtIdx = 0;
    int m_posEnterFrame = 0;
    Vector3 m_speedDir = Vector3.zero;
    Vector3 m_velocity = Vector3.zero;
    Transform m_sourceBone;
    TranPartCxt m_tranCxt;
    int m_tranCxtId;
    SkillEventGroup m_eventGroup = new SkillEventGroup();
    #endregion

    #region Properties
    public override enBehit Type { get { return enBehit.beGrab; } }
    public BeGrabCxt MyCxt { get { return (BeGrabCxt)Cxt; } }
    public GrabCxt GrabCxt { get { return Cxt == null ? null:MyCxt.grabCxt; } }
    public SkillEventGroup EventGroup { get{return m_eventGroup;}}
    #endregion

    #region Frame
    public RoleSubBeGrab(RoleStateBehit parent, enBehit stateTo) : base(parent, stateTo) { }

   
    public override bool CanParentLeave(){
        return false;
    }
    public override IBehitCxt CanStateTo(IBehitCxt cxt)
    {
        if (!m_isPlaying)
            return cxt;
        else
            return null;
    }
    public override void Leave()
    {
        ClearTranCxt( enSkillStop.normal);
    }

    public override void Enter()
    {
        Do();
    }

    public override void Do()
    {
        //清空之前
        ClearTranCxt( enSkillStop.force);

        //计算一些值
        m_beginTime = TimeMgr.instance.logicTime;
        m_isPlaying = true;
        m_frameLastTime = 0;        

        //动作序列的播放
        AniPart aniPart = Parent.AniPart;
        if (GrabCxt.anis.anis.Count != 0)
            aniPart.Play(MyCxt.grabCxt.anis);

        //位置和方向的控制
        TranPart tranPart =Parent.TranPart;
        m_tranCxt = tranPart.AddCxt();
        m_tranCxt.moveType = TranPartCxt.enMove.avoid;
        m_tranCxtId = m_tranCxt.Id;
        m_curPosCxtIdx = 0;
        m_posEnterFrame = 0;

        if (MyCxt.GrabRole != null &&m_curPosCxtIdx < GrabCxt.poss.Count)
        {
            GrabPosAndDirCxt c = GrabCxt.poss[m_curPosCxtIdx];
            m_sourceBone = string.IsNullOrEmpty(c.bone) ? MyCxt.GrabRole.transform : MyCxt.GrabRole.transform.Find(c.bone);
            
            if (m_sourceBone != null )
            {
                c.OnEnter(MyCxt.GrabRole, Parent, m_sourceBone, ref m_speedDir, ref m_velocity);
            }
            else if (m_sourceBone == null)
                Debuger.LogError("抓取位置上下文没有找到抓取者骨骼:{0} 抓取者:{1} 被抓取者:{2} ", c.bone, MyCxt.GrabRole.Cfg.id, Parent.Cfg.id);    
        }

        //事件组
        if (MyCxt.GrabRole != null)
        {
            MyCxt.GrabRole.SetGrabTarget(Parent);
            m_eventGroup.Init(GrabCxt.eventGroup, MyCxt.GrabRole, null, Parent.RoleModel.Model,MyCxt.parentSkill);
            m_eventGroup.Play(Parent, tranPart.transform.position);
        }
        
        
    }
    public override void Update()
    {
        //抓取者死亡的判断
        if (MyCxt.GrabRole == null|| (GrabCxt.poss.Count != 0 && m_sourceBone == null))
            m_isPlaying =false;
        if(CheckLeave())
            return;

        //事件组
        m_eventGroup.UpdateAuto();

        //当前帧
        int curFrame = (int)((TimeMgr.instance.logicTime-m_beginTime)/Util.One_Frame);

        //角色特效的播放
        foreach(GrabFxCxt c in GrabCxt.fxs){
            if (c.frame >= m_frameLastTime && c.frame < curFrame)
            {
                RoleFxCfg.Play(c.roleFxName, Parent, MyCxt.GrabRole);
            }      
        }
        m_frameLastTime = curFrame;

        //位置的控制
        TranPart tranPart = Parent.TranPart;
        if (m_curPosCxtIdx < GrabCxt.poss.Count)
        {
            if (!GrabCxt.poss[m_curPosCxtIdx].OnUpdate(MyCxt.GrabRole, Parent, m_sourceBone, m_frameLastTime - m_posEnterFrame, m_speedDir, ref m_velocity))
            {
                m_posEnterFrame = m_frameLastTime;
                ++m_curPosCxtIdx;
                if (m_curPosCxtIdx < GrabCxt.poss.Count)
                {
                    GrabPosAndDirCxt c = GrabCxt.poss[m_curPosCxtIdx];
                    m_sourceBone = string.IsNullOrEmpty(c.bone) ? MyCxt.GrabRole.transform : MyCxt.GrabRole.transform.Find(c.bone);
                    

                    if (m_sourceBone != null )
                    {
                        c.OnEnter(MyCxt.GrabRole, Parent, m_sourceBone, ref m_speedDir, ref m_velocity);
                    }
                    else if (m_sourceBone == null)
                        Debuger.LogError("抓取位置上下文没有找到抓取者骨骼:{0} 抓取者:{1} 被抓取者:{2} ", c.bone, MyCxt.GrabRole.Cfg.id, Parent.Cfg.id);
                }
            }
        }

        //如果碰到地板，自动进入落地状态
        if (m_curPosCxtIdx < GrabCxt.poss.Count && GrabCxt.poss[m_curPosCxtIdx].autoIfObstacle && Parent.TranPart.IsGrounded)
            m_isPlaying = false;
        if (CheckLeave())
            return;

        //判断是不是结束了
        AniPart aniPart = Parent.AniPart;
        if(GrabCxt.frame == -1)
            m_isPlaying =m_curPosCxtIdx < GrabCxt.poss.Count &&!aniPart.IsAnisOver(GrabCxt.anis);
        else
            m_isPlaying = (TimeMgr.instance.logicTime-m_beginTime) < (GrabCxt.frame*Util.One_Frame);
        if(CheckLeave())
            return;
    }

    #endregion

    #region Private Methods
    void ClearTranCxt(enSkillStop stopType)
    {
        if (m_tranCxt != null)
        {
            //事件组
            if (m_eventGroup.IsPlaying)
            {
                m_eventGroup.Stop(stopType);
            }


            if (!m_tranCxt.IsDestroy(m_tranCxtId))
            {
                Parent.TranPart.RemoveCxt(m_tranCxt);
            }
                
            m_tranCxtId = -1;
            m_tranCxt = null;
        }
        
    }
    bool CheckLeave()
    {
        if (m_isPlaying)
            return false;

        Role grabRole = MyCxt.GrabRole;

        //结束事件组，属于抓取者
        if (grabRole != null && !string.IsNullOrEmpty(MyCxt.grabCxt.endEventGroupId))
            CombatMgr.instance.PlayEventGroup(grabRole, MyCxt.grabCxt.endEventGroupId, Parent.transform.position, Parent, MyCxt.parentSkill);

        //结束状态，加载被抓取者身上的
        if (Parent != null && Parent.State== Role.enState.alive)
        {
            if (GrabCxt.endBuffId > 0)
                Parent.BuffPart.AddBuff(GrabCxt.endBuffId, MyCxt.GrabRole);
        }
        
        

        //结束销毁
        if (GrabCxt.destroyWhenEnd)
        {
            Parent.DeadPart.Handle(true);
            return true;
        }
        //如果碰到地板，自动进入落地状态
        else if (Parent.TranPart.IsGrounded)
        {
            GroundCxt cxt = IdTypePool<GroundCxt>.Get();
            cxt.duration = 0.5f;
            m_stateBehit.GotoState(cxt,false,true);
            return true;
        }
        else //否则进入浮空中的下落状态
        {
            BeFloatCxt cxt = IdTypePool<BeFloatCxt>.Get();
            cxt.cfg = RoleStateBehit.s_fall;
            m_stateBehit.GotoState(cxt,false,true);
            return true;
        } 

    }
    #endregion

    
}
