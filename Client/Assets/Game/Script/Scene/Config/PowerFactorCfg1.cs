using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerFactorCfg1
{
    #region Fields
    public int id = 0;  //ID
    public float minSubPower = -100000000; //战斗力差值下限
    public float maxSubPower = -200000; //战斗力差值上限
    public float hitFactor = -0.25f; //英雄对怪物伤害系数2（包括宠物与英雄、宠物召唤物）
    public float beHitFactor = 0.26f; //英雄受到怪物伤害系数2（包括宠物与英雄、宠物召唤物）

    #endregion
    public static List<PowerFactorCfg1> m_cfg = new List<PowerFactorCfg1>();
    public static void Init()
    {
        m_cfg.Clear();
        m_cfg = Csv.CsvUtil.Load<PowerFactorCfg1>("room/powerFactor1");
    }

    public static PowerFactorCfg1 Get(float powerNum)
    {
        for (int i = 0; i < m_cfg.Count; i++)
        {
            PowerFactorCfg1 cfg = m_cfg[i];
            if (powerNum > cfg.minSubPower && powerNum <= cfg.maxSubPower)
            {
                return cfg;
            }
        }
        Debuger.LogError("没有找到战力差值{0}对应英雄打怪系数2", powerNum);
        return m_cfg[0];
    }

    public static float GetHitFactor(float powerNum)
    {
        for(int i = 0; i < m_cfg.Count; i++)
        {
            PowerFactorCfg1 cfg = m_cfg[i];
            if ( powerNum > cfg.minSubPower && powerNum <= cfg.maxSubPower)
            {
                return cfg.hitFactor;
            }
        }
        Debuger.LogError("没有找到战力差值{0}对应英雄打怪系数2", powerNum);
        return 0;
    }

    public static float GetBeHitFactor(float powerNum)
    {
        for (int i = 0; i < m_cfg.Count; i++)
        {
            PowerFactorCfg1 cfg = m_cfg[i];
            if (powerNum > cfg.minSubPower && powerNum <= cfg.maxSubPower)
            {
                return cfg.beHitFactor;
            }
        }
        Debuger.LogError("没有找到战力差值{0}对应英雄被打系数2", powerNum);
        return 0;
    }

}
