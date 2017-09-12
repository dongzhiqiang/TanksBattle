#region Header
/**
 * 名称：HeroTalentCfg
 
 * 日期：2016.4.5
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class HeroTalentCfg 
{
    public int id;
    public string name;
    public string desc;
    public string icon;
    public int[] levelBuffs;
    public int levelCostId;



    public static Dictionary<int, HeroTalentCfg> s_cfgs = new Dictionary<int, HeroTalentCfg>();
    public static Dictionary<int, HeroTalentCfg> s_buffs = new Dictionary<int, HeroTalentCfg>();//找到状态所属的铭文
    public static void Init()
    {
        s_cfgs = Csv.CsvUtil.Load<int, HeroTalentCfg>("systemSkill/heroTalent", "id");
        s_buffs.Clear();
        foreach (HeroTalentCfg cfg in s_cfgs.Values)
        {
            //添加到状态所属铭文的索引
            if (cfg.levelBuffs != null)
            {
                foreach (int buffId in cfg.levelBuffs)
                {
                    if (buffId == 0)
                        continue;
                    if (s_buffs.ContainsKey(buffId))
                    {
                        Debuger.LogError("铭文表，铭文升级影响的状态不能填在多个技能里，重复的状态{0}，冲突的铭文:{1}、{2}", buffId, s_buffs[buffId], cfg.id);
                        continue;
                    }
                    s_buffs[buffId] = cfg;
                }
            }
        }
    }

    public static HeroTalentCfg Get(int id)
    {
        HeroTalentCfg cfg = s_cfgs.Get(id);
        if(cfg == null)
        {
            Debuger.LogError("铭文表找不到id:{0}",id);
            return null;
        }
        return cfg;
    }
    public static HeroTalentCfg GetByBuff(int buffId)
    {
        return s_buffs.Get(buffId);
    }
}
