#region Header
/**
 * 名称：增减耐力
 
 * 日期：2016.2.25
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

public class BuffAddMpCfg : BuffExCfg
{
    public bool baseOnMax;//基于上限，否则基于当前
    public LvValue value;
    public bool showUINum=true;


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

        if(pp.Length > 2)
            showUINum = pp[2] == "1";
        return true;
    }
}


public class BuffAddMp : Buff
{
    public BuffAddMpCfg ExCfg { get { return (BuffAddMpCfg)m_cfg.exCfg; } }
   
    

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
            float v = ExCfg.baseOnMax ? propPart.GetFloat(enProp.mpMax) : propPart.GetInt(enProp.mp);
            value = v * value;
        }
        

        CombatMgr.instance.AddMp(m_parent, (int)value, ExCfg.showUINum);
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        
        
    }

    
    
  
}

