#region Header
/**
 * 名称：增减属性
 
 * 日期：2016.2.25
 * 描述：
属性名|值或百分比,属性名|值或百分比....
属性名,生命,攻击,护甲,减免伤害,最终伤害,暴击几率,抗暴几率,暴击伤害,火,冰,雷,冥,火抗,冰抗,雷抗,冥抗,生命偷取,伤害反弹,怒气,速度,冷却缩减,
值或百分比,举例:30、-30、30%、-30%
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


public class BuffAddPropCfg: BuffExCfg
{
    public List<AddPropCxt> cxts = new List<AddPropCxt>();
    public override bool SupportUnalive { get { return true; } }
    public override bool Init(string[] pp)
    {
        foreach (string s in pp)
        {
            AddPropCxt cxt = new AddPropCxt(s);
            if (cxt.error )
                return false;
            cxts.Add(cxt);
        }
        return true;
    }
   
}



public class BuffAddProp : Buff
{
    public BuffAddPropCfg ExCfg { get {return (BuffAddPropCfg)m_cfg.exCfg; } }

    Dictionary<enProp, float> m_addValues = new Dictionary<enProp, float>();
    Dictionary<enProp, float> m_addRates = new Dictionary<enProp, float>();
    int m_observer;
    //初始化，状态创建的时候调用，一般用来解析下参数
    public override void OnBuffInit() {
        if (m_addValues.Count != 0 || m_addRates.Count!=0)
        {
            Debuger.LogError("逻辑错误，增减属性初始化的时候发现增加的属性没有清空");
            m_addValues.Clear();
            m_addRates.Clear();
        }
        if (!this.Cfg.IsAliveBuff)
        {
            m_observer = m_parent.Add(MSG_ROLE.FRESH_BASE_PROP, OnEvent,false);
            this.Parent.PropPart.FreshBaseProp();
        }
    }

    //处理，可能会调用多次
    public override void OnBuffHandle()
    {
        if (this.Cfg.IsAliveBuff)
        {
            PropPart propPart = m_parent.PropPart;
            //叠加属性的状态值部分
            AddPropCxt cxt;
            for (int i = 0; i < ExCfg.cxts.Count; ++i)
            {
                cxt = ExCfg.cxts[i];
                float v = GetLvValue(cxt.value);

                if (!cxt.value.isPercent)
                {
                    propPart.AddBuffValue(cxt.prop, v);
                    m_addValues[cxt.prop] = m_addValues.Get(cxt.prop) + v;
                }
                else
                {
                    propPart.AddBuffRate(cxt.prop, v);
                    m_addRates[cxt.prop] = m_addRates.Get(cxt.prop) + v;
                }
            }

            //刷新到最终属性
            foreach (enProp p in m_addValues.Keys)
            {
                propPart.FreshProp(p);
            }
            foreach (enProp p in m_addRates.Keys)
            {
                if (!m_addValues.ContainsKey(p))
                    propPart.FreshProp(p);
            }
        }
        
      
    }

    //结束
    public override void OnBuffStop(bool isClear) {
        //如果是战斗状态
        if (!isClear)
        {
            if (this.Cfg.IsAliveBuff)
            {
                PropPart propPart = m_parent.PropPart;
                //叠加属性的状态值部分
                foreach (KeyValuePair<enProp, float> a in m_addValues)
                {
                    propPart.AddBuffValue(a.Key, -a.Value);
                }
                foreach (KeyValuePair<enProp, float> a in m_addRates)
                {
                    propPart.AddBuffRate(a.Key, -a.Value);
                }

                //刷新到最终属性
                foreach (enProp p in m_addValues.Keys)
                {
                    propPart.FreshProp(p);
                }
                foreach (enProp p in m_addRates.Keys)
                {
                    if (!m_addValues.ContainsKey(p))
                        propPart.FreshProp(p);
                }
            }
            else
            {
                if (m_observer != EventMgr.Invalid_Id)
                    EventMgr.Remove(m_observer);

                Parent.PropPart.FreshBaseProp();
            }
        }
        

        
        m_addValues.Clear();
        m_addRates.Clear();
        
        m_observer = EventMgr.Invalid_Id;
    }

    void OnEvent(object p1, object p2)
    {
        if (this.Cfg.IsAliveBuff)
        {
            this.LogError("逻辑错误，战斗状态不需要监听属性变化");
            return;
        }
        PropertyTable values = (PropertyTable)p1;
        PropertyTable rates = (PropertyTable)p2;


        //叠加属性的状态值部分
        AddPropCxt cxt;
        for (int i = 0; i < ExCfg.cxts.Count; ++i)
        {
            cxt = ExCfg.cxts[i];
            float v = GetLvValue(cxt.value);

            if (!cxt.value.isPercent)
                values.AddFloat(cxt.prop, v, true);
            else
                rates.AddFloat(cxt.prop, v, true);
        }

        

    }



}

