#region Header
/**
 * 名称：增减血
 
 * 日期：2016.2.25
 * 描述：
类型,值或百分比,ui显示,对象
类型，上限/当前/属性(生命、护甲、攻击等)
值或百分比,正数加血，负数扣血，血小于等于0则死亡，举例:30、-30、30%、-30%
ui显示,1显示，0不显示
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class BuffAddHpCfg : BuffExCfg
{
    public bool baseOnMax;//基于上限，否则基于当前
    public enProp prop;
    public LvValue value;
    public bool showUINum;
    public enBuffTargetType targetType = enBuffTargetType.self;


    public override bool Init(string[] pp)
    {
        if (pp.Length < 3)
            return false;

        prop = enProp.minFightProp;
        if (pp[0] == "上限")
            baseOnMax = true;
        else if (pp[0] == "当前")
            baseOnMax = false;
        else
        {
            PropTypeCfg c = PropTypeCfg.GetByName(pp[0]);
            if (c == null)
                return false;
            prop = (enProp)c.id;
        }

        value = new LvValue(pp[1]);
        if (value.error)
            return false;

        showUINum = pp[2] == "1";

        //作用对象
        int i = 0;
        if (pp.Length > 3 && int.TryParse(pp[3], out i))
            targetType = (enBuffTargetType)i;
        return true;
    }
}


public class BuffAddHp : Buff
{
    public BuffAddHpCfg ExCfg { get { return (BuffAddHpCfg)m_cfg.exCfg; } }

    float m_targetProp = -1;
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        m_targetProp = -1;
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {  
        PropPart propPart =m_parent.PropPart;
        float value = this.GetLvValue(ExCfg.value);
        if (ExCfg.value.isPercent)
        {
            if(ExCfg.prop == enProp.minFightProp)
            {
                float v = ExCfg.baseOnMax ? propPart.GetFloat(enProp.hpMax) : propPart.GetInt(enProp.hp);
                value = v * value;
            }
            else
            {
                if(m_targetProp == -1)
                {
                    Role r = this.GetRole(ExCfg.targetType, null);
                    m_targetProp = r == null ? 0 : r.GetFloat(ExCfg.prop);
                }
                if (m_targetProp == 0)
                    return;
                
                value = m_targetProp * value;
            }
            
        }
        
        int trueAdd =CombatMgr.instance.AddHp(m_parent, (int)value, false, ExCfg.showUINum);
        CombatRecord recordParent = CombatMgr.instance.GetCombatRecord(m_parent);
        CombatRecord recordTarget = Source == null?null:CombatMgr.instance.GetCombatRecord(Source);
        //战斗数据记录
        if (trueAdd < 0)
        {
            recordParent.beHitDamage += -trueAdd;
            if (recordTarget!= null)
                recordTarget.hitDamage += -trueAdd;
        }
        else if (trueAdd > 0)
        {
            if (recordTarget != null)
                recordTarget.addHp+= trueAdd;
        }

        m_parent.DeadPart.CheckAndHandle();//检查是不是死亡
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        m_targetProp = -1;


    }
}

