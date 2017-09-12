#region Header
/**
 * 名称：战斗管理器
 
 * 日期：2015.12.17
 * 描述：
 *      技能、事件组等战斗对象的池管理
 *      事件组的update支持
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CombatRecord
{
    public int      id = 0;//运行时的id
    public string   roleId = string.Empty;//角色id
    public string   roleName = string.Empty;  //角色名
    public int      hitDamage = 0;//总伤害
    public int      beHitDamage = 0;//总被击伤害
    public int      addHp = 0;//总的治疗别人的量
}


public class CombatMgr : SingletonMonoBehaviour<CombatMgr>
{
    ValueObject<float> s_addDamage = new ValueObject<float>(0);
    ValueObject<float> s_addDefDamage = new ValueObject<float>(0);

    #region Fields
    public int m_debugRoleId = -1;
    public string m_debugSkillId = string.Empty;
    public LinkedList<Flyer> m_playingFlyers = new LinkedList<Flyer>();//正在播放的飞出物
    public LinkedList<SkillEventGroup> m_eventGroups = new LinkedList<SkillEventGroup>();
    public bool m_isEventGroupClearing=false;
    public Dictionary<int,CombatRecord> m_combatRecords = new Dictionary<int,CombatRecord>();
    #endregion


    #region Properties
    
    #endregion

    #region Mono Frame
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(TimeMgr.instance.IsPause)
            return;

        SkillEventGroup g;
        LinkedListNode<SkillEventGroup> cur = null;
        LinkedListNode < SkillEventGroup > next= m_eventGroups.First;
        while (next != null)
        {
            cur = next;
            g = cur.Value;
            next = cur.Next;
            if(g.IsInPool)//可能被删除了，简单判断下
            {
                Debuger.LogError("事件组可能在遍历的过程中被删除掉了");
                continue;
            }
                
                
            g.UpdateAuto();
            if (g.IsInPool)//可能ClearEventGroup()了
                break;

            if (!g.IsPlaying)
            {
                g.Put();
                m_eventGroups.Remove(cur);
            }
            
            if(TimeMgr.instance.IsPause)//如果暂停了，那么不要再执行了，暂停期间不应该执行任何战斗逻辑
                break;
            
        }

     
    }
    #endregion
   


    #region Private Methods
    
    #endregion

    


    //isCut表明要不要做生命偷取计算
    public bool Damage(Role source, Role target, Vector3 pos, float rate, string hitFx, bool isCut, enElement elem)
    {
        if (source.State != Role.enState.alive || target.State != Role.enState.alive)
        {
            Debuger.LogError("角色已经死亡 不能伤害计算:{0} {1}", source.Cfg.id,target.Cfg.id);
            return false;
        }


        //增减攻击伤害、增减被击伤害
        s_addDamage.Value = 0;
        source.Fire(MSG_ROLE.ADD_DAMAGE, s_addDamage, target);
        s_addDefDamage.Value = 0;
        target.Fire(MSG_ROLE.ADD_DEF_DAMAGE, s_addDefDamage);
        float addDamage = 1f + s_addDamage.Value + s_addDefDamage.Value;

        //伤害计算
        PropBasicCfg baseCfg = PropBasicCfg.instance;
        PropertyTable sProp = source.PropPart.Props;
        PropertyTable tProp = target.PropPart.Props;
        enRolePropType propType =target.Cfg.RolePropType;
        RoleLvPropCfg roleLvPropCfg =RoleLvPropCfg.Get(tProp.GetInt(enProp.level));

        //战力修正系数
        float powerFactor = 1;
        //减伤率的f(lv)
        float defRateLv =0;
        if (propType == enRolePropType.monster)
        {
            defRateLv = roleLvPropCfg.defRateMonster;
            powerFactor = LevelMgr.instance.PowerHitFactor;
        }
        else if (propType == enRolePropType.role)
        {
            defRateLv = roleLvPropCfg.defRateRole;
            powerFactor = LevelMgr.instance.PowerBeHitFactor;
        }
        else
        {
            defRateLv = roleLvPropCfg.defRatePet;
            powerFactor = LevelMgr.instance.PowerBeHitFactor;
        }
        
        //减伤率=C*护甲/(护甲+B+A*f(lv))
        float def = tProp.GetFloat(enProp.def);
        float defRate = Mathf.Max(baseCfg.defRateC * def / (def + baseCfg.defRateB + baseCfg.defRateA * defRateLv), 0f);

        //打击属性
        float hitPropRate = 0;
        float levelHitPropRate = LevelMgr.instance.CurLevel.roomCfg.hitProp;
        var fightWeapon = source.CombatPart.FightWeapon;
        var hitPropCfg = fightWeapon != null ? fightWeapon.HitPropCfg : null;
        enHitDef hitDef = enHitDef.none;
        if (levelHitPropRate!=0 &&hitPropCfg != null)
        {
            hitDef = target.Cfg.GetHitDef(hitPropCfg);
            hitPropRate = hitPropCfg.GetRate(hitDef)* levelHitPropRate;
        }
            
        //暴击概率=攻.暴击几率-受.抗暴几率，取值范围0-1
        float criticalRate = Mathf.Clamp01(sProp.GetFloat(enProp.critical) - tProp.GetFloat(enProp.criticalDef));
        bool isCritical = criticalRate == 0?false:UnityEngine.Random.value < criticalRate;
        //暴击伤害倍数=2+攻.暴击伤害
        float critical = isCritical ? (2 + sProp.GetFloat(enProp.criticalDamage)) : 1;
        //非暴击时伤害=C*(攻击*(1-减伤率)+Max(最终伤害-减免伤害,0))*技能伤害修正系数*技能伤害事件伤害系数*增伤率*(1+打击属性倍率)*（1+元素伤害倍率)*rand(95%,105%)   取值大于等于1
        //暴击时伤害=非暴击时伤害*暴击伤害
        float damage = (sProp.GetFloat(enProp.atk) * (1 - defRate) + Mathf.Max(sProp.GetFloat(enProp.damage) - tProp.GetFloat(enProp.damageDef), 0)) * rate * addDamage *(1+ hitPropRate) * UnityEngine.Random.Range(0.95f,1.05f);
        if (isCritical)//这里暴击的C和非暴击的C不一样
            damage = Mathf.Max(baseCfg.damageCritical * damage * critical, 1);
        else
            damage = Mathf.Max(baseCfg.damage* damage , 1);

        //元素伤害倍率=C*(元素.攻-元素.抗)/(元素.攻+A)  取值大于等于0
        float elemRate = 0;
        float sElem = 0;
        float tElem = 0;
        switch (elem)
        {
            case enElement.none: break;
            case enElement.fire: sElem = sProp.GetFloat(enProp.fire); tElem = tProp.GetFloat(enProp.fireDef); break;
            case enElement.ice: sElem = sProp.GetFloat(enProp.ice); tElem = tProp.GetFloat(enProp.iceDef); break;
            case enElement.thunder: sElem = sProp.GetFloat(enProp.thunder); tElem = tProp.GetFloat(enProp.thunderDef); break;
            case enElement.dark: sElem = sProp.GetFloat(enProp.dark); tElem = tProp.GetFloat(enProp.darkDef); break;
            default: Debuger.LogError("未知的元素类型:{0}", elem); elem = enElement.none; break;
        }
        elemRate = elem == enElement.none ? 0 : Mathf.Max(0, baseCfg.elementC * (sElem - tElem) / (sElem + baseCfg.elementA));
        float elemDamage = elemRate == 0 ? 0 : damage * elemRate;
        damage += elemDamage;
        

        //战力修正计算
        damage *= powerFactor;
        elemDamage *= powerFactor;

        //日志
       /* string log = string.Format("攻击者:{0} 被攻击者:{1}", source.Cfg.id, target.Cfg.id);
        log += string.Format("增伤率=1+增减攻击伤害+增减受击伤害 = 1+{0}+{1}={2}\n", s_addDamage.Value, s_addDefDamage.Value, addDamage);
        log += string.Format("减伤率=C*护甲/(护甲+B+A*减伤等级系数)={0}*{1}/({1}+{2}+{3}*{4})={5}\n", baseCfg.defRateC, def, baseCfg.defRateB, baseCfg.defRateA, defRateLv, defRate);
        log += string.Format("{0} 元素伤害倍率 = C * (元素.攻 - 元素.抗) / (元素.攻 + A)={1}*({2}-{3})/({2}-{4})={5}\n", ElementCfg.Element_Names[(int)elem], baseCfg.elementC, sElem, tElem, baseCfg.elementA, elemRate);
        if (isCritical)
            log += string.Format("暴击:{0} 暴击伤害:{1} 战斗力修正={2} 伤害=C*(攻击*(1-减伤率)+Max(最终伤害-减免伤害,0))*技能伤害修正系数*技能伤害事件伤害系数*增伤率*（1+元素伤害倍率)*rand(95%,105%)最后再乘以暴击伤害和战斗力修正={3}", isCritical, critical, powerFactor, damage);
        else
            log += string.Format("暴击:{0} 战斗力修正={1}  伤害=C*(攻击*(1-减伤率)+Max(最终伤害-减免伤害,0))*技能伤害修正系数*技能伤害事件伤害系数*增伤率*（1+元素伤害倍率)*rand(95%,105%)最后再乘以战斗力修正={2}", isCritical, powerFactor, damage);
        Debuger.LogError(log);*/
        
        return Damage(source, target, pos, (int)damage, hitFx, isCritical, isCut, elem, (int)elemDamage, hitDef);
    }

    //pos表明了打击的位置,cutHp表明扣的血量,hitFx表明打击特效
    public bool Damage(Role source, Role target, Vector3 pos, int cutHp, string hitFx, bool isCritical, bool isCut, enElement elem, int elemCutHp, enHitDef hitDef)
    {
        if (source.State != Role.enState.alive || target.State != Role.enState.alive)
        {
            Debuger.LogError("角色已经死亡 不能伤害计算:{0} {1}", source.Cfg.id, target.Cfg.id);
            return false;
        }
       
        //先扣血
        int trueCut=AddHp(target,-cutHp, isCritical,true, elem, -elemCutHp, hitDef);

        //战斗数据记录
        CombatRecord recordSrc =GetCombatRecord(source);
        CombatRecord recordTarget = GetCombatRecord(target);
        if (trueCut < 0)
        {
            recordTarget.beHitDamage += -trueCut;
            recordSrc.hitDamage +=-trueCut;
        }

        //打击特效
        if (!string.IsNullOrEmpty(hitFx))
            RoleFxCfg.Play(hitFx, target, source, pos, elem);

        //被击特效(创建在角色上)
        List<string> behitFxs = target.Cfg.behitFxs;
        for (int i = 0; i < behitFxs.Count; ++i)
        {
            RoleFxCfg.Play(behitFxs[i], target, source, pos);
        }

        //仇恨收集
        target.HatePart.BeHit(source, cutHp);
        source.HatePart.Hit(target);

        //生命偷取(攻击方)
        float srcHpCut = source.GetFloat(enProp.hpCut);
        if (isCut && srcHpCut > 0 )//第一次触发的时候才能生命偷取,见传进来的isCut
        {
            int c = (int)Mathf.Abs(trueCut*srcHpCut);
            if (c > 0)
            {
                int trueAddHp =AddHp(source, c, false, true);
                if (trueAddHp>0)
                    recordSrc.addHp += trueAddHp;//战斗数据记录
            }
        }

        //伤害反弹
        float damageReflect = target.GetFloat(enProp.damageReflect);
        if (damageReflect > 0)
        {
            if (source.Fire(MSG_ROLE.DAMAGE_REFLECT_AVOID, target))//先检查有没有否决伤害反弹，没有才做伤害反弹
            {
                int c = (int)Mathf.Abs(trueCut * damageReflect);
                if (c > 0)
                {
                    int trueDamageReflect = AddHp(source, -c, false, true);
                    if (trueDamageReflect < 0)
                        recordSrc.beHitDamage += -trueDamageReflect;//战斗数据记录
                }
            }
        }


        //广播被击和攻击消息
        int srcId = source.Id;
        int targetId = target.Id;
        source.Fire(MSG_ROLE.HIT, target);
        if (target.IsUnAlive(targetId) || source.IsUnAlive(srcId))//FIX:有可能角色已经删除，这里先判断下
            return true;
        target.Fire(MSG_ROLE.BEHIT, source);
        if (target.IsUnAlive(targetId) || source.IsUnAlive(srcId))//FIX:有可能角色已经删除，这里先判断下
            return true;

        //死亡处理
        target.DeadPart.CheckAndHandle();
        return true;
    }

    

    //增减血，返回实际加或者减了多少
    public int AddHp(Role target,int add, bool isCritical,bool showUI, enElement elem = enElement.none, int elemAdd=0, enHitDef hitDef = enHitDef.none)
    {
        if (add == 0)
            return 0;

        //增减血
        int hp = target.GetInt(enProp.hp);
        int hpNew = (int)Mathf.Clamp(hp+add,0,target.GetFloat(enProp.hpMax));
        target.SetInt(enProp.hp, hpNew);

        //飘血数字，血量没有变化的话不飘
        if (target.Cfg.roleType == enRoleType.box || target.Cfg.roleType == enRoleType.trap)
            showUI = false;
        if (showUI)
        {
            UILevelAreaNum.enType numType;
            if (add > 0)
                numType = UILevelAreaNum.enType.greenAdd;
            else if (isCritical)
                numType = UILevelAreaNum.enType.redSubCritical;
            else if(target.IsHero)//主角被击的话，颜色要不一样
                numType = UILevelAreaNum.enType.redSubHero;
            else if(hitDef == enHitDef.weak)//抗性弱，伤害显示强
                numType = UILevelAreaNum.enType.redSubStrong; 
            else if (hitDef == enHitDef.strong)
                numType = UILevelAreaNum.enType.redSubWeak;
            else
                numType = UILevelAreaNum.enType.redSub;

            UIMgr.instance.Get<UILevel>().Get<UILevelAreaNum>().ShowNum(numType, target.RoleModel.Title.position, Mathf.Abs(add - elemAdd), elem, Mathf.Abs(elemAdd));
        }

        return hpNew-hp;
    }

    //增减怒气
    public void AddMp(Role target, int add,bool showUI)
    {
        int mp = target.GetInt(enProp.mp);
        int mpNew = (int)Mathf.Clamp(mp + add, 0, target.GetFloat(enProp.mpMax));

        if (target.Cfg.roleType == enRoleType.box || target.Cfg.roleType == enRoleType.trap)
            showUI = false;
        if (showUI)
        {
            UILevelAreaNum.enType numType;
            if (add > 0)
                numType = UILevelAreaNum.enType.blueAdd;
            
            else
                numType = UILevelAreaNum.enType.blueSub;
            UIMgr.instance.Get<UILevel>().Get<UILevelAreaNum>().ShowNum(numType, target.RoleModel.Title.position, Mathf.Abs(add), enElement.none, 0);
        
        }
        target.SetInt(enProp.mp, mpNew);
    }

    //增减气力值
    public void AddShield(Role target, int add)
    {
        int v = target.GetInt(enProp.shield);
        int vNew = (int)Mathf.Clamp(v + add, 0, target.GetFloat(enProp.shieldMax));
        target.SetInt(enProp.shield, vNew);

        //判断进入气绝状态,实现上就是删除气力状态,然后它就会执行结束状态，也就是气力状态
        if (add < 0 && vNew == 0)
        {
            RemoveShield(target);
        }
    }


    void RemoveShield(Role target)
    {
        int shieldBuffId=target.RuntimeShieldBuff;
        if(shieldBuffId <= 0 ||!target.RSM.IsShield)//气力状态下才有气力状态可以删除，这里先判断下提升下效率
            return;
        BuffCfg shieldBuffCfg = BuffCfg.Get(shieldBuffId);
        if(shieldBuffCfg==null){
            Debuger.LogError("{0}找不到气力状态配置：{1}", target.Cfg.id,shieldBuffId);
            return;
        }
        
        BuffPart buffPart = target.BuffPart;
        int count = buffPart.RemoveBuffByBuffId(shieldBuffId);
        if (count != 1){
            Debuger.LogError("{0}气绝状态维护出错，删除了{1}个气力状态", target.Cfg.id, count);
            return;
        }

        //计算下恢复时间，气绝状态持续时间=通用气绝时间/(1+气力时间系数*(1+Min(10%*n,50%))
        int unshieldCounter = target.GetFlag("unshieldCounter");
        float counterRate =Mathf.Min(unshieldCounter*ConfigValue.shieldRateAdd,ConfigValue.shieldRateLimit);
        float duration = ConfigValue.unshieldDuation/(1+target.Cfg.shieldTimeRate*(1+counterRate));
        
        //记录下进入气绝状态的次数
        target.AddFlag("unshieldCounter");

        //找到气绝状态
        int unshieldBuffId = shieldBuffCfg.endBuffId == null || shieldBuffCfg.endBuffId.Length ==0?0:shieldBuffCfg.endBuffId[0];
        if (unshieldBuffId <= 0)
        {
            Debuger.LogError("{0}气绝状态(气力状态的结束状态)id无效：{1}", target.Cfg.id, unshieldBuffId);
            return;
        }
        Buff unshieldBuff = buffPart.GetBuffByBuffId(unshieldBuffId);
        if(unshieldBuff == null)
        {
            Debuger.LogError("{0}找不到气绝状态(气力状态的结束状态)：{1}", target.Cfg.id, unshieldBuffId);
            return;
        }
        unshieldBuff.Time = duration;
        
        //增加一个恢复满气力的状态，在气绝状态结束的时候
        Buff shieldRegainBuff = buffPart.AddBuff(100);
        shieldRegainBuff.Time = duration;
        
    }

    public void AddFlyer(Flyer f)
    {
        m_playingFlyers.AddLast(f);
        if(m_playingFlyers.Count > 50)   
        {
            Debuger.LogError("运行中的飞出物数量超过50个，是不是有内存泄露:{0}",f.Cfg.file);
        }
    }

    public void RemoveFlyer(Flyer f)
    {
        LinkedListNode<Flyer> node = m_playingFlyers.Find(f);
        if (f == null)
        {
            Debuger.LogError("要清除掉飞出物的时候发现没有注册过，是不是重复清除了");
            return;
        }
        m_playingFlyers.Remove(node);
    }

    public void GetFlyers(string flyerId,ref List<Flyer> l)
    {
        l.Clear();
        LinkedListNode<Flyer> node = m_playingFlyers.First;
        while (node!=null)
        {
            if(node.Value.Cfg.file == flyerId)
                l.Add(node.Value);
            node = node.Next;
        }
    }

    public void ClearFlyers()
    {
        LinkedListNode<Flyer> node = m_playingFlyers.First;
        Flyer f;
        while (node != null)
        {
            f = node.Value;
            node = node.Next;
            f.Stop(true);
        }
        m_playingFlyers.Clear();
    }

    public void PlayEventGroup(Role role,string eventGroupId, Vector3 happonPos, Role target=null,Skill parentSkill=null){
        if (m_isEventGroupClearing)
        {
            Debuger.LogError("正在清空中，事件组不能添加:{0} {1}", role.Cfg == null ? "" : role.Cfg.id, eventGroupId);
            return;
        }
        if(role.State!= Role.enState.alive)
        {
            Debuger.LogError("角色不在alive状态，不能创建事件组:{0} {1}", role.Cfg == null ? "" : role.Cfg.id, eventGroupId);
            return;
        }

        SkillEventGroupCfg cfg = SkillEventGroupCfg.Get(eventGroupId, false);
        if (cfg == null)
        {
            Debuger.LogError(string.Format("找不到事件组:{0} {1}", role.Cfg.id,eventGroupId));
            return;
        }

        SkillEventGroup eventGroup = IdTypePool<SkillEventGroup>.Get();
        eventGroup.Init(cfg, role, null, role.transform, parentSkill);
        if (eventGroup.HaveForeverFrame)
        {
            Debuger.LogError("事件组不能填帧的结束帧为-1，否则永远都结束不了:{0} {1}", role.Cfg.id, eventGroupId);
        }
        eventGroup.Play(target, happonPos);

        //可能马上就结束了，这里要判断下
        if (!eventGroup.HaveForeverFrame && eventGroup.MaxFrame==0)
        {
            eventGroup.Stop(enSkillStop.normal);
            eventGroup.Put();    
            return;
        }
            
        m_eventGroups.AddLast(eventGroup);

    }

    //清空所有运行中的事件组，这里指的是由combatMgr托管的事件组，技能和飞出物的事件组不会清空
    public void ClearEventGroup()
    {
        m_isEventGroupClearing =true;

        LinkedListNode<SkillEventGroup> node = m_eventGroups.First;
        SkillEventGroup g;
        while (node != null)
        {
            g = node.Value;
            node = node.Next;
            g.Stop(enSkillStop.force);
            g.Put();

        }
        m_eventGroups.Clear();

        m_isEventGroupClearing = false;
    }

    public CombatRecord GetCombatRecord(Role r)
    {
        if (r == null || r.IsInPool)
        {
            Debuger.LogError("CombatMgr.GetCombatRecord 传进来的角色已经被销毁或者为空");
            return null;
        }
        CombatRecord record = GetCombatRecord(r.Id);
        if (string.IsNullOrEmpty(record.roleId))
        {
            record.roleId = r.Cfg.id;
            record.roleName = r.GetString(enProp.name);
        }            
        return record;
    }

    public CombatRecord GetCombatRecord(int id)
    {
        CombatRecord record =m_combatRecords.Get(id);
        if (record == null)
        {
            record = new CombatRecord();
            record.id = id;
            m_combatRecords[id] = record;
        }
        return record;
    }

    public void ClearCombatRecord()
    {
        m_combatRecords.Clear();
    }

    public void StopAllLogic()
    {

        //飞出物强制销毁
        CombatMgr.instance.ClearFlyers();
        //所有事件组停止
        CombatMgr.instance.ClearEventGroup();

        //人物强制切换到待机
        foreach (Role r in RoleMgr.instance.Roles)
        {
            if (r.State == Role.enState.alive && r.RSM.CurStateType != enRoleState.born && r.RSM.CurStateType != enRoleState.dead)
            {
                r.RSM.CheckFree(true);
                r.TranPart.ResetHight();
            }
        }

    }
}
