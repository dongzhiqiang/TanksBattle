using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PowerDifficulty
{
    //最小
    min,
    //容易
    easy,
    //略难
    normal,
    //困难
    hard,
    //凶险
    danger,
    //最大
    max,
}

public class PowerFactorCfg2
{

    #region Fields
    public int id = 0;  //ID
    public float minPercentPower = 0; //战斗力比值下限
    public float maxPercentPower = 0.5f; //战斗力比值上限
    public float hitFactor = 0.3f; //英雄对怪物伤害系数1（包括宠物与英雄、宠物召唤物）
    public float beHitFactor = 1.7f; //英雄受到怪物伤害系数1（包括宠物与英雄、宠物召唤物）
    public PowerDifficulty difficulty = PowerDifficulty.min;    //难度评级
    public string desc = "";    //难度展示

    #endregion
    public static List<PowerFactorCfg2> m_cfg = new List<PowerFactorCfg2>();
    public static void Init()
    {
        m_cfg.Clear();
        m_cfg = Csv.CsvUtil.Load<PowerFactorCfg2>("room/powerFactor2");
    }

    public static PowerFactorCfg2 Get(float powerNum)
    {
        for (int i = 0; i < m_cfg.Count; i++)
        {
            PowerFactorCfg2 cfg = m_cfg[i];
            if (powerNum > cfg.minPercentPower && powerNum <= cfg.maxPercentPower)
            {
                return cfg;
            }
        }
        Debuger.LogError("没有找到战力比值{0}对应英雄打怪系数1", powerNum);
        return m_cfg[0];
    }

    public static float GetHitFactor(float powerNum)
    {
        for (int i = 0; i < m_cfg.Count; i++)
        {
            PowerFactorCfg2 cfg = m_cfg[i];
            if (powerNum > cfg.minPercentPower && powerNum <= cfg.maxPercentPower)
            {
                return cfg.hitFactor;
            }
        }
        Debuger.LogError("没有找到战力比值{0}对应英雄打怪系数1", powerNum);
        return 0;
    }

    public static float GetBeHitFactor(float powerNum)
    {
        for (int i = 0; i < m_cfg.Count; i++)
        {
            PowerFactorCfg2 cfg = m_cfg[i];
            if (powerNum > cfg.minPercentPower && powerNum <= cfg.maxPercentPower)
            {
                return cfg.beHitFactor;
            }
        }
        Debuger.LogError("没有找到战力比值{0}对应英雄被打系数1", powerNum);
        return 0;
    }

}