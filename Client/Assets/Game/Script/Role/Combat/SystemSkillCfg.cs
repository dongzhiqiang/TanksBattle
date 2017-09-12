#region Header
/**
 * 名称：副本固定属性
 
 * 日期：2015.11.24
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class SystemSkillCfg 
{
    public string roleId;
    public string id;
    public string name;
    public string icon;
    public LvValue damageRate;
    public string parentSkillId;
    public int[] levelBuffs;
    
    public static Dictionary<string, Dictionary<string, SystemSkillCfg>> s_cfgs = new Dictionary<string, Dictionary<string, SystemSkillCfg>>();
    public static Dictionary<int, SystemSkillCfg> s_buffs = new Dictionary<int, SystemSkillCfg>();//找到状态所属的技能
    public static void Init()
    {
        s_cfgs.Clear();
        s_buffs.Clear();
        List<SystemSkillCfg> l = Csv.CsvUtil.Load<SystemSkillCfg>("systemSkill/systemSkill");
        foreach (SystemSkillCfg cfg in l)
        {
            //添加到索引            
            s_cfgs.GetNewIfNo(cfg.roleId)[cfg.id] = cfg;

            //添加到状态所属技能的索引
            if (cfg.levelBuffs != null)
            {
                foreach (int buffId in cfg.levelBuffs)
                {
                    if (buffId == 0)
                        continue;
                    if (s_buffs.ContainsKey(buffId))
                    {
                        Debuger.LogError("技能表，技能升级影响的状态不能填在多个技能里，重复的状态{0}，冲突的技能:{1}、{2}",buffId, s_buffs[buffId], cfg.id);
                        continue;
                    }
                    s_buffs[buffId] = cfg;
                }
            }
        }
    }

    public static SystemSkillCfg Get(string roleId, string skillId)
    {
        Dictionary<string, SystemSkillCfg> d = s_cfgs.Get(roleId);
        if (d == null)//允许找不到，找不到就是没有图标、技能伤害系数等东西而已
        {
            //Debuger.LogError("技能表找不到技能:{0} {1}", roleId, skillId);
            return null;
        }
        SystemSkillCfg cfg = d.Get(skillId);
        if (cfg == null)//允许找不到，找不到就是没有图标、技能伤害系数等东西而已
        {
            //Debuger.LogError("技能表找不到技能:{0} {1}", roleId, skillId);
            return null;
        }
        return cfg;
    }

    public static SystemSkillCfg GetByBuff(int buffId)
    {
        return s_buffs.Get(buffId);
    }
}
