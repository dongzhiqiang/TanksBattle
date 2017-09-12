#region Header
/**
 * 名称：事件帧
 
 * 日期：2015.12.17
 * 描述：事件帧
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public sealed class SkillEventFrame : IdType//注意这个类的设计决定了它不适合继承
{
    public enum enState
    {
        unTrigger,//没有触发过
        triggering,//触发中
        finish,//不需要再触发了
    }
    #region Fields
    SkillEventGroup m_eventGroup;
    SkillEventFrameCfg m_cfg;
    
    enState m_state;
    int m_count ;//总的作用对象次数
    int m_frameCount;//有效帧次数，如果一帧作用过至少一个对象，那么就算有效帧
    List<Role> m_targets = new List<Role>();
    Dictionary<int,int> m_targetsCount =new Dictionary<int,int>();//对象触发次数计数
    bool m_havaHandle;
    float m_factor;
    int m_frameTriggerCount =0;
    TargetRangeCfg m_curRangeTarget;
    #endregion


    #region Properties
    public Skill Skill { get{return m_eventGroup.Skill; }}
    public SkillEventGroup EventGroup { get{return m_eventGroup;}}
    public Role Parent { get{return m_eventGroup.Parent; }}
    public SkillEventFrameCfg Cfg { get { return m_cfg; } }
    //总的作用对象次数
    public int TargetCount { get{return m_count;}}
    //有效帧次数，如果一帧作用过至少一个对象，那么就算有效帧
    public int FrameCount { get{return m_frameCount;}}

    //帧触发次数,当前帧正在触发第几个对象
    public int FrameTriggerCount { get{return m_frameTriggerCount;}}
    public float Factor { get { return m_factor; } }
    public TargetRangeCfg CurRangeTarget{get{return m_curRangeTarget;}}
    #endregion


    #region Private Methods
    
    #endregion
    public void Init(SkillEventFrameCfg cfg, SkillEventGroup g, Skill s)
    {
        m_cfg =cfg;
        m_eventGroup =g;
        m_state = enState.unTrigger;
        m_count = 0;
        m_frameCount = 0;
        m_targets.Clear();
        
        m_targetsCount.Clear();
        m_havaHandle =false;
        m_factor =0;
        m_curRangeTarget = null;
    }

    public override void OnClear()
    {
        
        m_cfg= null;
        m_eventGroup = null;
        m_targets.Clear();
        m_targetsCount.Clear();
    }

    //保证了每帧有且只执行一次
    public void Update(int frame,bool end)
    {
        int poolId = this.Id;

        if(m_state == enState.finish)
            return;

        //先根据帧判断能不能触发
        bool trigger =false;
        bool finish = false;
        if(m_cfg.frameType == enSkillEventFrameType.once){
            trigger = frame == m_cfg.frameBegin;
            finish = true;
        }
        else if(m_cfg.frameType == enSkillEventFrameType.multi){
            if ((m_cfg.frameEnd != -1 && frame >= m_cfg.frameEnd))
            {
                trigger =true;
                finish = true;
            }
            else
                trigger = frame >= m_cfg.frameBegin &&(frame - m_cfg.frameBegin) % m_cfg.frameInterval == 0;    
        }
        else
            Debuger.LogError("未知的帧类型:{0} 帧id:{1} 所属技能或者事件组:{2}", m_cfg.frameType, m_cfg.id, m_eventGroup.Name);       

        if(!trigger)
            return;

        if(finish)
            m_state = enState.finish;
        else 
            m_state = enState.triggering;

        //总的作用对象次数限制和有效帧次数限制
        if ((m_cfg.countLimit > 0 && m_count >= m_cfg.countLimit) || (m_cfg.frameLimit > 0 && m_frameCount >= m_cfg.frameLimit))
            return;

        
        //前置事件限制
        for (int i = 0; i < m_cfg.conditions.Count; ++i)
        {
            if (!m_cfg.conditions[i].IsMatch(this))
                return;
        }
            

        //阵营、碰撞检测限制、次数限制
        Role target;
        m_factor = GetFactor(frame);
        Vector3 srcPos = m_eventGroup.Root.position;
        Vector3 srcDir = m_eventGroup.Root.forward;
        m_frameTriggerCount = 0;
        
        for(int i=0;i<m_cfg.targetRanges.Count;++i)
        {
            //RoleMgr.instance.GetTargets(m_parent, m_parent.transform.position, m_cfg.targetRanges[i], ref m_targets);
            m_targets.Clear();
            m_curRangeTarget = m_cfg.targetRanges[i];
            if (m_curRangeTarget.targetType == enSkillEventTargetType.selfAlway)
            {
                //相关次数限制，如果不排序的话可以在这里先判断了，减少碰撞检测
                if (m_cfg.targetOrderType == enTargetOrderType.none && !CheckLimitAndCondition(m_frameTriggerCount, Parent))
                    continue;

                m_targets.Add(Parent);
                if (m_cfg.targetOrderType == enTargetOrderType.none) AddTargetCount(Parent.Id);//记录下，同一对象作用次数加1
                ++m_frameTriggerCount;
            }
            else if (m_curRangeTarget.targetType == enSkillEventTargetType.target)
            {
                target = this.m_eventGroup.Target;
                if (target== null)
                    continue;
                
                //相关次数限制，如果不排序的话可以在这里先判断了，减少碰撞检测
                if (m_cfg.targetOrderType == enTargetOrderType.none && !CheckLimitAndCondition(m_frameTriggerCount, target))
                    continue;

                //碰撞检测
                if (!CollideUtil.Hit(srcPos, srcDir, target.TranPart.Pos, target.RoleModel.Radius, m_curRangeTarget.range, m_factor))
                    continue;

                m_targets.Add(target);
                if (m_cfg.targetOrderType == enTargetOrderType.none) AddTargetCount(target.Id);//记录下，同一对象作用次数加1
                ++m_frameTriggerCount;
            }
            else if (m_curRangeTarget.range.type == enRangeType.collider)//用unity的Collider进行碰撞检测
            {
                Flyer f =m_eventGroup.Flyer;
                if (f == null) { Debuger.LogError("不是飞出物不能使用碰撞类型的范围检测"); continue; }
                if (!f.HasCollider) { Debuger.LogError("飞出物上没有碰撞，不能做碰撞检测:{0} {1}",f.Cfg.file,f.name); continue; }
                Role r;
                foreach (RoleModel roleModel in f.ColliderRoles)
                {
                    if(roleModel==null )continue;
                    r =roleModel.Parent;
                    if(r== null||r.State!= Role.enState.alive)continue;
                    //阵营限制
                    if (!RoleMgr.instance.MatchTargetType(m_curRangeTarget.targetType, Parent, r))
                        continue;
                    //相关次数限制，如果不排序的话可以在这里先判断了
                    if (m_cfg.targetOrderType == enTargetOrderType.none && !CheckLimitAndCondition(m_frameTriggerCount, r))
                        continue;

                    //高度也判断下
                    if (!CollideUtil.HitHeight(srcPos.y, r.TranPart.Pos.y ,m_curRangeTarget.range.heightLimit))
                        continue;

                    m_targets.Add(r);
                    if (m_cfg.targetOrderType == enTargetOrderType.none) AddTargetCount(r.Id);//记录下，同一对象作用次数加1
                    ++m_frameTriggerCount;
                }
            }
            else//用自己写的2d碰撞检测
            {
                
                if(m_eventGroup.Flyer!=null && m_cfg.frameType == enSkillEventFrameType.multi)
                {
                    Debuger.LogError("飞出物的多帧事件必须用飞出物自己的碰撞进行检测:{0}", m_eventGroup.Flyer.Cfg.file);
                    break;
                }

                //遍历进行碰撞检测
                foreach (Role r in RoleMgr.instance.Roles)
                {
                    //全局敌人不能被碰撞检测到
                    if (r == RoleMgr.instance.GlobalEnemy)
                        continue;

                    //阵营限制
                    if (!RoleMgr.instance.MatchTargetType(m_curRangeTarget.targetType, Parent, r))
                        continue;

                    //相关次数限制，如果不排序的话可以在这里先判断了，减少碰撞检测
                    if (m_cfg.targetOrderType == enTargetOrderType.none && !CheckLimitAndCondition(m_frameTriggerCount, r))
                        continue;

                    //碰撞检测
                    if (!CollideUtil.Hit(srcPos, srcDir, r.TranPart.Pos, r.RoleModel.Radius, m_curRangeTarget.range, m_factor))
                        continue;

                    m_targets.Add(r);
                    if (m_cfg.targetOrderType == enTargetOrderType.none) AddTargetCount(r.Id);//记录下，同一对象作用次数加1
                    ++m_frameTriggerCount;
                }
            }

            if(m_targets.Count!=0)
                break;
        }

        if(m_targets.Count==0)
            return;

        m_frameTriggerCount = 0;//次数一定要清空防止出错
        //排序
        if(m_cfg.targetOrderType== enTargetOrderType.distance)
        {
            m_targets.Sort(RoleDistanceComp);
        }

        
        SkillEventCfg e;
        bool ret;
        Role parent =Parent;
        int parentId = parent.Id;
        int targetId;
        bool isHero = parent.transform == CameraMgr.instance.GetFollow();
        for (int i = 0;i<m_targets.Count;++i){
            target = m_targets[i];
            targetId = target.Id;
            //相关次数限制，如果排序的话可以在这里判断了，不排序在上面判断
            if (m_cfg.targetOrderType != enTargetOrderType.none && !CheckLimitAndCondition(m_frameTriggerCount, target))
                continue;

            int handleCount = 0;
            //执行事件
            for (int j = 0; j < m_cfg.events.Count; ++j)
            {
                if (parent.IsDestroy(parentId) || parent.State != Role.enState.alive)
                    return;
                if (target.IsDestroy(targetId) || target.State != Role.enState.alive)
                    break;
                
                e = m_cfg.events[j];

                //调试的时候无视了
                if (e.ingore || (!isHero&&e.ingoreIfNotHero))
                    continue;

                //事件次数限制
                if (e.eventCountLimit != -1 && (m_count + m_frameTriggerCount) >= e.eventCountLimit)
                    continue;

                //帧事件次数，一帧能同时作用的对象的次数
                if (e.eventCountFrameLimit != -1 && m_frameTriggerCount >= e.eventCountFrameLimit)
                    continue;

                //事件执行消息（执行前）,可能中断当前技能，或者使当前角色死亡要判断下
                target.Fire(MSG_ROLE.TARGET_SKILL_EVENT_PRE, parent, e);
                if (this.IsDestroy(poolId))//FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
                    return;
                if (target.IsDestroy(targetId) || target.State != Role.enState.alive)//目标可能死亡了，再判断下
                    break;
                

                //技能事件否决
                if (!target.Fire(MSG_ROLE.SKILL_EVENT_AVOID, parent, e))
                    continue;

                ret = e.Handle(parent, target, this);
                if (this.IsDestroy(poolId))//FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
                    return;
                
                if (!target.IsDestroy(targetId) && target.State == Role.enState.alive)//目标可能死亡了，再判断下
                {
                    parent.Fire(MSG_ROLE.SOURCE_SKILL_EVENT, target, e, this);
                }
                if (this.IsDestroy(poolId))//FIX: 上一行的处理可能导致这个对象被回收了，这个时候要马上返回，不能做任何修改
                    return;
                if (ret)
                    ++handleCount;
                if (ret && !m_havaHandle)
                {
                    m_havaHandle = true;
                }

            }

            if (m_cfg.targetOrderType != enTargetOrderType.none) AddTargetCount(target.Id);//记录下，同一对象作用次数加1
            if(handleCount>0)
                ++m_frameTriggerCount;
            if (parent.State != Role.enState.alive )
                break;
        }
        
        //如果是有效帧，计数加一
        if (m_frameTriggerCount > 0)
        {
            m_count += m_frameTriggerCount;
            ++m_frameCount;
        }
            
    }

    bool CheckLimitAndCondition(int triggerCount,Role r)
    {
        if (m_cfg.countLimit > 0 && (m_count + triggerCount) >= m_cfg.countLimit)//总的作用对象次数限制
            return false;
        if (m_cfg.countTargetLimit > 0 && GetTargetCount(r.Id) >= m_cfg.countTargetLimit)//对象触发次数限制
            return false;
        if (m_cfg.countFrameLimit > 0 && triggerCount >= m_cfg.countFrameLimit)//当前帧触发的次数限制
            return false;
        //前置对象事件
        for (int i = 0; i < m_cfg.targetConditions.Count; ++i)
        {
            if (!m_cfg.targetConditions[i].IsMatch(this, r))
                return false;
        }

        return true;
    }

    public int GetTargetCount(int id)
    {
        int i;
        if(!m_targetsCount.TryGetValue(id,out i))
        {
            m_targetsCount.Add(id,0);
            return 0;
        }
        else
            return i;
    }

    void AddTargetCount(int id)
    {
        int i;
        if (!m_targetsCount.TryGetValue(id, out i))
            m_targetsCount.Add(id, 1);
        else
            m_targetsCount[id] = i+1;
    }

    public float GetFactor(int frame)
    {
        if (frame <= m_cfg.frameBegin)
            return 0;


        //计算下结束帧，这里如果是-1的话要转换下
        int frameEndOfFactor;
        if (m_cfg.frameEnd == -1)
        {
            if (Skill != null)
                frameEndOfFactor = Skill.MaxFrame;
            else
                frameEndOfFactor = m_eventGroup.MaxFrame;
        }
        else
            frameEndOfFactor = m_cfg.frameEnd;


        //计算下进度
        if (frameEndOfFactor <= m_cfg.frameBegin)
            return 0;
        else if (frame >= frameEndOfFactor)
            return 1;
        else
            return (float)(frame - m_cfg.frameBegin) / (float)(frameEndOfFactor - m_cfg.frameBegin);
    }

    int RoleDistanceComp(Role a,Role b)
    {
        Vector3 root = m_eventGroup.Root.position;
        float d1 = Util.XZSqrMagnitude(root, a.TranPart.Pos);
        float d2 = Util.XZSqrMagnitude(root, b.TranPart.Pos);
        return d1.CompareTo(d2);
    }
}
