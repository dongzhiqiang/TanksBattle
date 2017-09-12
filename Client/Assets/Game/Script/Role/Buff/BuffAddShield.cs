#region Header
/**
 * 名称：增减气力
 
 * 日期：2016.3.3
 * 描述：
类型,值或百分比
类型，上限/当前
值或百分比,正数加血，负数扣血，血小于等于0则死亡，举例:30、-30、30%、-30%
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class BuffAddShieldCfg : BuffExCfg
{
    public bool baseOnMax;//基于上限，否则基于当前
    public LvValue value;


    public override bool Init(string[] pp)
    {
        if (pp.Length < 2)
            return false;

        if (pp[0] == "上限")
            baseOnMax = true;
        else if (pp[0] == "当前")
            baseOnMax = false;
        else
            return false;

        value = new LvValue(pp[1]);
        if (value.error)
            return false;

        
        return true;
    }
}

public class BuffAddShield : Buff
{
    public BuffAddShieldCfg ExCfg { get { return (BuffAddShieldCfg)m_cfg.exCfg; } }
    
    

    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
       
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {   
        
        PropPart propPart =m_parent.PropPart;
        float value = this.GetLvValue(ExCfg.value);
        if (ExCfg.value.isPercent)
        {
            float v = ExCfg.baseOnMax ? propPart.GetFloat(enProp.shieldMax) : propPart.GetInt(enProp.shield);
            value = v * value;
        }
        

        CombatMgr.instance.AddShield(m_parent, (int)value);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        
        
    }

    
    
  
}

