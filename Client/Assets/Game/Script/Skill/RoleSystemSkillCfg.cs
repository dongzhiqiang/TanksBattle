#region Header
/**
 * 名称：RoleSystemSkillCfg
 
 * 日期：2016.4.5
 * 描述：
 *      
 **/
#endregion
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class RoleSystemSkillCfg 
{
    public string roleId;
    public string id;
    public string name;
    public string description;
    public string briefDesc;
    //public string type;
    public int[] comboLimit =null;
    public int levelCostId=0;
    public int needPetStar;
    public int[] talent;//主角技能的铭文id
    public LvValue powerRate;
    
    

    public static Dictionary<string,Dictionary<string, RoleSystemSkillCfg>> s_cfgs = new Dictionary<string,Dictionary<string,RoleSystemSkillCfg>>();
    public static void Init()
    {
        s_cfgs.Clear();
        List<RoleSystemSkillCfg> l= Csv.CsvUtil.Load<RoleSystemSkillCfg>("systemSkill/roleSkill");
        foreach(RoleSystemSkillCfg cfg in l){
            s_cfgs.GetNewIfNo(cfg.roleId)[cfg.id] = cfg;
        }
    }

    public static RoleSystemSkillCfg Get(string roleId,string skillId)
    {
        Dictionary<string,RoleSystemSkillCfg> d = s_cfgs.Get(roleId);
        if (d == null)
        {
            Debuger.LogError("角色技能表找不到技能:{0} {1}",roleId,skillId);
            return null;
        }
        RoleSystemSkillCfg cfg = d.Get(skillId);
        if(cfg == null)
        {
            Debuger.LogError("角色技能表找不到技能:{0} {1}", roleId, skillId);
            return null;
        }
        return cfg;
    }
}
