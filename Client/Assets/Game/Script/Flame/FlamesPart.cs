#region Header
/**
 * 名称：物品部件
 
 * 日期：2015.9.21
 * 描述：背包和装备
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlamesPart : RolePart
{
    #region Fields
    /// <summary>
    /// 圣火ID与圣火的映射
    /// </summary>
    private Dictionary<int, Flame> m_flames = new Dictionary<int, Flame>();
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.flames; } }
    public Dictionary<int, Flame> Flames { get { return m_flames; } }
    #endregion


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
  
        if( vo.flames != null )
        {
            m_flames = new Dictionary<int, Flame>();
            foreach (FlameVo flameVo in vo.flames)
            {
                Flame flame = Flame.Create(flameVo);
                AddOrUpdateFlame(flame);
            }
        }
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnClear()
    {
        if (m_flames != null)
            m_flames.Clear();
    }

    public override void OnFreshBaseProp(PropertyTable values, PropertyTable rates)
    {
        //float oldHpMax = values.GetFloat(enProp.hpMax);
        //float oldPower = values.GetFloat(enProp.power);
        //float oldPowerRate = rates.GetFloat(enProp.power);
        foreach (Flame flame in m_flames.Values)
        {
            FlameCfg flameCfg = FlameCfg.m_cfgs[flame.FlameId];
            FlameLevelCfg levelCfg = FlameLevelCfg.Get(flame.FlameId, flame.Level);
            if(levelCfg==null)
            {
                continue;
            }
            PropValueCfg valueCfg = PropValueCfg.Get(levelCfg.attributeId);
            if (valueCfg == null)
            {
                continue;
            }
            PropertyTable.Add(values, valueCfg.props, values);
            /*
            values.SetFloat(enProp.power, values.GetFloat(enProp.power) + levelCfg.power);
            rates.SetFloat(enProp.power, rates.GetFloat(enProp.power) + levelCfg.powerRate);
             */
        }
        //Debuger.Log("圣火 角色增加生命值:{0}", values.GetFloat(enProp.hpMax)-oldHpMax);
        //Debuger.Log("圣火 角色增加战斗力:{0}", values.GetFloat(enProp.power) - oldPower);
        //Debuger.Log("圣火 角色增加战斗力系数:{0}", rates.GetFloat(enProp.power) - oldPowerRate);
    }


    void AddOrUpdateFlame(Flame flame)
    {

        if (m_flames == null)
            return;

        m_flames[flame.FlameId] = flame;

    }

    public void AddOrUpdateFlame(FlameVo flameVo)
    {
        if (m_flames == null)
            return;

        Flame flame;
        if (m_flames.TryGetValue(flameVo.flameId, out flame))
        {
            flame.LoadFromVo(flameVo);
        }
        else
        {
            flame = Flame.Create(flameVo);
            AddOrUpdateFlame(flame);
        }
    }

    public Flame GetFlame(int flameId)
    {
        Flame result;
        if (m_flames.TryGetValue(flameId, out result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }

    #endregion


    #region Private Methods
    
    #endregion
}