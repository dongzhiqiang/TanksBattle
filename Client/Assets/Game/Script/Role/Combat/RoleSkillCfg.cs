#region Header
/**
 * 名称：角色技能配置
 
 * 日期：2015.12.8
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Reflection;

public class RoleSkillCfg
{
    public string file;
    public List<SkillCfg> skills = new List<SkillCfg>();

    static Dictionary<string, RoleSkillCfg> s_cfgs = new Dictionary<string, RoleSkillCfg>();
    static HashSet<string> s_preLoads = new HashSet<string>();

    string[] skillIds;
    Dictionary<string,SkillCfg> skillsBySkillId = new Dictionary<string,SkillCfg>();
    bool needCalcCombo=true;//是不是需要重新计算下连击技能
    

    public bool NeedCalcCombo
    {
        get { return needCalcCombo; }
        set { needCalcCombo = value;}
    }

    public string[] SkillIds
    {
        get
        {
            if (skillIds == null)
            {
                List<string> l = new List<string>();
                
                foreach (SkillCfg c in skills)
                    l.Add(c.skillId);
                l.Sort();

                l.Add(string.Empty);
                l.Add("新增技能");
                
                skillIds = l.ToArray();
            }
            
            return skillIds;
        }
    }

    public static RoleSkillCfg Get(string file)
    {
        if (string.IsNullOrEmpty(file))
            return null;

        RoleSkillCfg cfg;
        cfg = s_cfgs.Get(file);
        if (cfg != null)
            return cfg;

        cfg = Util.LoadJsonFile<RoleSkillCfg>("skill/" + file);
        if(cfg == null)
        {
            cfg = new RoleSkillCfg();
            cfg.file = file;
        }
        cfg.Reset();

        s_cfgs[cfg.file] = cfg;
        return cfg;
    }

    //预加载
    public static void PreLoad(string file)
    {
        if (s_preLoads.Contains(file))
            return;
        s_preLoads.Add(file);

        RoleSkillCfg cfg = Get(file);
        if (cfg == null)
            return;

        for(int i =0;i<cfg.skills.Count;++i)
            cfg.skills[i].PreLoad();
    }

    public static void RemoveCache(RoleSkillCfg cfg){
        if(cfg == null)return;
        RoleSkillCfg cacheCfg =s_cfgs.Get(cfg.file);
        if(cacheCfg==cfg)
            s_cfgs.Remove(cfg.file);
        
    }
    
    public void Reset(){
        skillIds= null;
        skillsBySkillId.Clear();
        for(int i=0;i<skills.Count;++i)
        {
            var s = skills[i];
            skillsBySkillId[s.skillId] = s;
            s.Reset();
            
        }
            
        needCalcCombo=true;
    }

    public SkillCfg GetBySkillId(string id,bool logError = true){
        SkillCfg s =skillsBySkillId.Get(id);
        if (s == null&& logError)
            Debuger.LogError("{0}找不到技能id:{1}",file,id);
            
        return s;
    }

    public SkillCfg Add()
    {
        //计算出一个新的id
        int idx = skills.Count;
        string skillId;
        do{
            skillId= string.Format("id_{0:00}",++idx);
        }while(System.Array.IndexOf(SkillIds,skillId)!=-1);
         
        SkillCfg s = new SkillCfg();
        //s.id = ++SkillCfgMgr.instance.skillCounter;
        s.skillId = skillId;
        skills.Add(s);

        //要重新计算一些索引信息
        Reset();
        return s;
    }

    public void Save()
    {
        Util.SaveJsonFile("skill/" + file, this);
        
        SkillCfgMgr.instance.Save();
    }

    public void Remove(SkillCfg cfg)
    {
        skills.Remove(cfg);
        //要重新计算一些索引信息
        Reset();
    }

    public void CopyFrom(string roleId)
    {
        RoleCfg roleCfg = RoleCfg.Get(roleId);
        if(roleCfg == null) return;

        CopyFrom(RoleSkillCfg.Get(roleCfg.skillFile));
    }
    public void CopyFrom(RoleSkillCfg cfg)
    {
        if (cfg == null) return;

        //复制值类型的属性
        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance, "file");

        //复制其他
        skills.Clear();
        foreach (SkillCfg s in cfg.skills)
        {
            Add().CopyFrom(s);
        }
        Reset();
    }

}
