#region Header
/**
 * 名称：光环
 
 * 日期：2016.7.22
 * 描述：
触发次数,触发间隔帧数,作用对象类型,状态列表,是否结束删除子状态,作用对象,释放者
每次碰到或者碰撞范围内一定间隔时间内有角色，则触发
碰撞范围计算方式：先取状态特效上的碰撞，没有的话尝试取角色上的碰撞
是否结束删除子状态，1默认删除，0则不删除
触发次数，如果大于1，定时判断，次数大于这个数就不触发
        如果是-1,定时判断，次数没有上限
        如果是-2,碰到判断,每次碰到触发
        如果是-3,碰到判断,首次碰到触发
触发间隔帧数，定时判断的间隔
碰撞对象匹配类型，1自己，2敌人，3友方，4中立，5友方和自己，7释放者，8除了自己
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BuffHaloCfg : BuffExCfg
{
    public int triggerLimit ;
    public int intervalFrame =1;
    public enSkillEventTargetType matchType;
    public List<int> buffIds = new List<int>();
    public bool isEndRemove = false;
    public enBuffTargetType targetType = enBuffTargetType.another;
    public enBuffTargetType sourceType = enBuffTargetType.self;
    public override bool Init(string[] pp)
    {
        if (pp.Length < 4)
            return false;
        
        if (!int.TryParse(pp[0], out triggerLimit))
            return false;

        if (pp[1] != "" && !int.TryParse(pp[1], out intervalFrame))
            return false;

        int i = 0;
        if ( !int.TryParse(pp[2], out i))
            return false;
        matchType = (enSkillEventTargetType)i;

        //状态列表
        if (int.TryParse(pp[3], out i))
            buffIds.Add(i);
        else
        {
            if (!StringUtil.TryParse(pp[3].Split('|'), ref buffIds))
                return false;
        }

        //是否结束删除
        if (pp.Length > 4 )
            isEndRemove = pp[4]=="1";

        //作用对象
        if (pp.Length > 5 && int.TryParse(pp[5], out i))
            targetType = (enBuffTargetType)i;

        //释放者
        if (pp.Length > 6 && int.TryParse(pp[6], out i))
            sourceType = (enBuffTargetType)i;

        return true;
    }
    public override void PreLoad()
    {
        for (int i = 0; i < buffIds.Count; ++i)
        {
            BuffCfg.ProLoad(buffIds[i]);
        }
    }
}

public class BuffHaloTarget:IdType
{    
    public Role target;
    public int targetId;
    public List<int> buffs = new List<int>();
    public override void OnClear() {
        buffs.Clear();
        targetId = 0;
        target = null;
    }
}

public class BuffHalo: Buff
{
    public BuffHaloCfg ExCfg { get { return (BuffHaloCfg)m_cfg.exCfg; } }

    Dictionary<int,BuffHaloTarget> m_buffTargets = new Dictionary<int, BuffHaloTarget>();
    Dictionary<int, float> m_lastTriggerTime = new Dictionary<int, float>();
    Dictionary<int, int> m_triggerCounter = new Dictionary<int, int>();
    RoleColliderListener m_listener = null;
    float m_lastUpdateTime;
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (m_count > 1)
        {
            Debuger.LogError("光环状态不需要执行多次，状态id:{0}", m_cfg.id);
            return;
        }

        if (this.m_roleFx != null)
            m_listener = RoleColliderListener.Set(this.m_roleFx, OnTrigger);
        if (m_listener == null)
            m_listener = RoleColliderListener.Set(this.Parent.transform.gameObject, OnTrigger);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        if (!isClear)//如果是清空，那么子状态也会清空，不用再删除
        {
            foreach(var t in m_buffTargets.Values)
            {
                if (!t.target.IsUnAlive(t.targetId)&& t.buffs.Count>0)
                {
                    t.target.BuffPart.BuffPart.RemoveBuffByIds(t.buffs, false);
                }
                t.Put();
            }
        }
        
        m_buffTargets.Clear();
        m_lastTriggerTime.Clear();
        m_triggerCounter.Clear();
        if (m_listener != null)
        {
            m_listener.Stop();
            m_listener = null;
        }
        m_lastUpdateTime = 0;

    }

    //每帧更新
    public override void OnBuffUpdate() {
        if (ExCfg.triggerLimit < -1 || m_listener == null)
            return;

        if (TimeMgr.instance.logicTime - m_lastUpdateTime < ExCfg.intervalFrame * Util.One_Frame)
            return;

        m_lastUpdateTime = TimeMgr.instance.logicTime;

        int poolId = this.Id;
        
        foreach(var model in m_listener.ColliderRoles)
        {
            var r = model.Parent;
            if (r == null || r.State != Role.enState.alive)
                continue;

            if (!RoleMgr.instance.MatchTargetType(ExCfg.matchType, this.Parent, r))
                continue;

            //如果触发次数超过上限
            int triggerCount;
            if (!m_triggerCounter.TryGetValue(r.Id, out triggerCount))
                triggerCount = 0;
            if (ExCfg.triggerLimit >= 0 && ExCfg.triggerLimit <= triggerCount)
                continue;

            //作用对象
            Role target = this.GetRole(ExCfg.targetType, r);
            if (target == null)
                return;
            int targetId = target.Id;
            BuffPart buffPart = target.BuffPart;

            //释放者
            Role source = this.GetRole(ExCfg.sourceType, r);
            if (source == null)
                return;
            int sourceId = source.Id;


            m_lastTriggerTime[r.Id] = m_lastUpdateTime;
            m_triggerCounter[r.Id] = triggerCount + 1;

            BuffHaloTarget haloTarget;
            if (!m_buffTargets.TryGetValue(targetId, out haloTarget))
            {
                haloTarget = IdTypePool<BuffHaloTarget>.Get();
                haloTarget.target = target;
                haloTarget.targetId = targetId;
            }

            for (int i = 0; i < ExCfg.buffIds.Count; ++i)
            {
                var buff = buffPart.AddBuff(ExCfg.buffIds[i], source);
                if (IsUnneedHandle(poolId, target, targetId, source, sourceId))
                    return;
                if (buff != null && ExCfg.isEndRemove)
                    haloTarget.buffs.Add(buff.Id);
            }

        }
        
    }

    void OnTrigger(Role r)
    {
        if (!RoleMgr.instance.MatchTargetType(ExCfg.matchType, this.Parent, r))
            return;

        //是间隔判断的，这里不用做处理
        if (ExCfg.triggerLimit >= -1)
            return;

        //首次碰到才触发
        int triggerCount;
        if (!m_triggerCounter.TryGetValue(r.Id, out triggerCount))
            triggerCount = 0;
        if (ExCfg.triggerLimit ==-3 &&  triggerCount>0)
            return;


        int poolId = this.Id;
        
        //作用对象
        Role target = this.GetRole(ExCfg.targetType, r);
        if (target == null)
            return;
        int targetId = target.Id;
        BuffPart buffPart = target.BuffPart;

        //释放者
        Role source = this.GetRole(ExCfg.sourceType, r);
        if (source == null)
            return;
        int sourceId = source.Id;


        m_lastTriggerTime[r.Id] = TimeMgr.instance.logicTime;
        m_triggerCounter[r.Id] = triggerCount + 1;

        BuffHaloTarget haloTarget; 
        if(!m_buffTargets.TryGetValue(targetId,out haloTarget))
        {
            haloTarget = IdTypePool<BuffHaloTarget>.Get();
            haloTarget.target = target;
            haloTarget.targetId = targetId;
        }
        
        for (int i = 0; i < ExCfg.buffIds.Count; ++i)
        {
            var buff = buffPart.AddBuff(ExCfg.buffIds[i], source);
            if (IsUnneedHandle(poolId, target, targetId, source, sourceId))
                return;
            if (buff != null && ExCfg.isEndRemove)
                haloTarget.buffs.Add(buff.Id);
        }
    }
}

