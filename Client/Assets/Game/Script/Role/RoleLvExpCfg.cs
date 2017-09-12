#region Header
/**
 * 名称：角色经验配置
 
 * 日期：2015.9.21
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class RoleLvExpCfg 
{
    public int level;
    public int needExp;//升到下一级需要的经验
    public int maxStamina;
    public int upgradeStamina;

    public static Dictionary<int, RoleLvExpCfg> m_cfgs;
    public static List<int> m_exps = new List<int>();
    public static List<int> m_totalExps = new List<int>();
    public static int TopLevel;

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, RoleLvExpCfg>("role/lvExp", "level");

        //补上0级
        RoleLvExpCfg cfg = new RoleLvExpCfg();
        cfg.level = 0;
        cfg.needExp = 0;
        m_cfgs[cfg.level] = cfg;

        //计算出阶段表
        m_exps.Clear();
        for(int i = 0;i<m_cfgs.Count;++i)
            m_exps.Add(m_cfgs[i].needExp);
        
        //计算阶段累计表
        m_totalExps = StageUtil.ListToTotalList(m_exps);

        TopLevel = StageUtil.GetTopStageByTotal(true, m_totalExps);
    }

    public static RoleLvExpCfg Get(int lv)
    {
        RoleLvExpCfg cfg = m_cfgs.Get(lv);
        if (cfg == null)
            Debuger.LogError("对应等级不存在，请检查经验表:{0}", lv);
        return cfg;
    }

    //当前级升到下一级需要的经验
    public static int GetNeedExp(int lv)
    {
        return m_exps[lv];
    }

    //升到当前级需要的经验
    public static int GetTotalExp(int lv)
    {
        return m_totalExps[lv];
    }
}
