#region Header
/**
 * 名称：属性定义表
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public enum enBuff
{
    min,
    empty,      //空状态
    addProp,    //增减属性
    addHp,      //增减血
    addMp,      //增减怒气
    avoidBuff,  //免疫状态
    avoidEvent, //免疫事件
    group,      //组合
    addShield,  //增减气力
    ani,        //动作序列
    beTriggerEvent,//被动事件触发
    playEventGroup,//触发事件组
    playSkill,//触发技能
    avoidDamageReflect,//免疫伤害反弹
    addDamage,//增减攻击伤害
    addDefDamage,//增减受击伤害
    triggerEvent,//主动事件触发
    addFlag,//加印记
    triggerFlag,//主动印记触发
    beTriggerFlag,//被动印记触发
    triggerKill,//主动杀死触发
    beTriggerKill,//被动杀死触发
    percent,//概率
    triggerHp,//主动血量触发,血量小于一定值
    beTriggerHp,//被动血量触发
    limitMove,//不可位移
    halo,       //光环
    colliderLayer,   //碰撞层
    changeHit,  //转换被击
    addHate,    //增减仇恨
    resetHate,  //清空仇恨
    triggerMp,  //耐力触发
    silent,     //沉默
    pauseAni,     //定身
    max,
}



public static class BuffFactory
{
    public static BuffExCfg CreateExCfg(BuffCfg cfg)
    {
        BuffExCfg exCfg;
        switch (cfg.BuffType)
        {
            case enBuff.empty: exCfg = new BuffExCfg(); break;
            case enBuff.addProp: exCfg = new BuffAddPropCfg(); break;
            case enBuff.addHp: exCfg = new BuffAddHpCfg(); break;
            case enBuff.addMp: exCfg = new BuffAddMpCfg(); break;
            case enBuff.avoidBuff: exCfg = new BuffAvoidBuffCfg(); break;
            case enBuff.avoidEvent: exCfg = new BuffAvoidEventCfg(); break;
            case enBuff.group: exCfg = new BuffGroupCfg(); break;
            case enBuff.addShield: exCfg = new BuffAddShieldCfg(); break;
            case enBuff.ani: exCfg = new BuffAniCfg(); break;
            case enBuff.beTriggerEvent: exCfg = new BuffBeTriggerEventCfg(); break;
            case enBuff.playEventGroup: exCfg = new BuffPlayEventGroupCfg(); break;
            case enBuff.playSkill: exCfg = new BuffPlaySkillCfg(); break;
            case enBuff.avoidDamageReflect: exCfg = new BuffAvoidDamageReflectCfg(); break;
            case enBuff.addDamage: exCfg = new BuffAddDamageCfg(); break;
            case enBuff.addDefDamage: exCfg = new BuffAddDefDamageCfg(); break;
            case enBuff.triggerEvent: exCfg = new BuffTriggerEventCfg(); break;
            case enBuff.addFlag: exCfg = new BuffAddFlagCfg(); break;
            case enBuff.triggerFlag: exCfg = new BuffTriggerFlagCfg(); break;
            case enBuff.beTriggerFlag: exCfg = new BuffBeTriggerFlagCfg(); break;
            case enBuff.triggerKill: exCfg = new BuffTriggerKillCfg(); break;
            case enBuff.beTriggerKill: exCfg = new BuffBeTriggerKillCfg(); break;
            case enBuff.percent: exCfg = new BuffPercentCfg(); break;
            case enBuff.triggerHp: exCfg = new BuffTriggerHpCfg(); break;
            case enBuff.beTriggerHp: exCfg = new BuffBeTriggerHpCfg(); break;
            case enBuff.limitMove: exCfg = new BuffLimitMoveCfg(); break;
            case enBuff.halo: exCfg = new BuffHaloCfg(); break;
            case enBuff.colliderLayer: exCfg = new BuffColliderLayerCfg(); break;
            case enBuff.changeHit: exCfg = new BuffChangeHitCfg(); break;
            case enBuff.addHate: exCfg = new BuffAddHateCfg(); break;
            case enBuff.resetHate: exCfg = new BuffResetHateCfg(); break;
            case enBuff.triggerMp: exCfg = new BuffTriggerMpCfg(); break;
            case enBuff.silent: exCfg = new BuffSilentCfg(); break;
            case enBuff.pauseAni: exCfg = new BuffPauseAniCfg(); break;
            default:
                {
                    Debuger.LogError("未知的类型，不能创建扩展配置:{0}", cfg.BuffType);
                    return null;
                }
        }

        if (exCfg != null && !exCfg.Init(cfg.param))
        {
            Debuger.LogError("{0}状态参数解析出错.状态id:{1}", cfg.type, cfg.id);
            return null;
        }

        return exCfg;
    }

    public static Buff CreateBuff(enBuff type)
    {
        Buff buff;
        switch (type)
        {
            case enBuff.empty: buff = IdTypePool<Buff>.Get(); break;
            case enBuff.addProp: buff = IdTypePool<BuffAddProp>.Get(); break;
            case enBuff.addHp: buff = IdTypePool<BuffAddHp>.Get(); break;
            case enBuff.addMp: buff = IdTypePool<BuffAddMp>.Get(); break;
            case enBuff.avoidBuff: buff = IdTypePool<BuffAvoidBuff>.Get(); break;
            case enBuff.avoidEvent: buff = IdTypePool<BuffAvoidEvent>.Get(); break;
            case enBuff.group: buff = IdTypePool<BuffGroup>.Get(); break;
            case enBuff.addShield: buff = IdTypePool<BuffAddShield>.Get(); break;
            case enBuff.ani: buff = IdTypePool<BuffAni>.Get(); break;
            case enBuff.beTriggerEvent: buff = IdTypePool<BuffBeTriggerEvent>.Get(); break;
            case enBuff.playEventGroup: buff = IdTypePool<BuffPlayEventGroup>.Get(); break;
            case enBuff.playSkill: buff = IdTypePool<BuffPlaySkill>.Get(); break;
            case enBuff.avoidDamageReflect: buff = IdTypePool<BuffAvoidDamageReflect>.Get(); break;
            case enBuff.addDamage: buff = IdTypePool<BuffAddDamage>.Get(); break;
            case enBuff.addDefDamage: buff = IdTypePool<BuffAddDefDamage>.Get(); break;
            case enBuff.triggerEvent: buff = IdTypePool<BuffTriggerEvent>.Get(); break;
            case enBuff.addFlag: buff = IdTypePool<BuffAddFlag>.Get(); break;
            case enBuff.triggerFlag: buff = IdTypePool<BuffTriggerFlag>.Get(); break;
            case enBuff.beTriggerFlag: buff = IdTypePool<BuffBeTriggerFlag>.Get(); break;
            case enBuff.triggerKill: buff = IdTypePool<BuffTriggerKill>.Get(); break;
            case enBuff.beTriggerKill: buff = IdTypePool<BuffBeTriggerKill>.Get(); break;
            case enBuff.percent: buff = IdTypePool<BuffPercent>.Get(); break;
            case enBuff.triggerHp: buff = IdTypePool<BuffTriggerHp>.Get(); break;
            case enBuff.beTriggerHp: buff = IdTypePool<BuffBeTriggerHp>.Get(); break;
            case enBuff.limitMove: buff = IdTypePool<BuffLimitMove>.Get(); break;
            case enBuff.halo: buff = IdTypePool<BuffHalo>.Get(); break;
            case enBuff.colliderLayer: buff = IdTypePool<BuffColliderLayer>.Get(); break;
            case enBuff.changeHit: buff = IdTypePool<BuffChangeHit>.Get(); break;
            case enBuff.addHate: buff = IdTypePool<BuffAddHate>.Get(); break;
            case enBuff.resetHate: buff = IdTypePool<BuffResetHate>.Get(); break;
            case enBuff.triggerMp: buff = IdTypePool<BuffTriggerMp>.Get(); break;
            case enBuff.silent: buff = IdTypePool<BuffSilent>.Get(); break;
            case enBuff.pauseAni: buff = IdTypePool<BuffPauseAni>.Get(); break;
            default:
                {
                    Debuger.LogError("未知的类型，不能添加:{0}", type);
                    return null;
                }
        }
        return buff;
    }

    
}
