#region Header
/**
 * 名称：飞出物配置
 
 * 日期：2016.1.19
 * 描述：相比于特效，飞出物主要支持战斗相关的配置，下面支持的功能
 *  拥有自己的事件组
 *  结束可以释放结束飞出物
 *  碰到敌人会销毁
 *  弹道控制
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LitJson;


public class FlyerFile
{
    //public int id;
    public string file;

}

public class FlyerCfg
{
    public string file = "";
    public SkillEventGroupCfg eventGroup = new SkillEventGroupCfg() ;
    public int durationFrame = -1;//持续帧数
    public FxCreateCfg endFlyerCreateCfg = new FxCreateCfg();
    public string endFlyer;
    public int touchDestroyFrame = -1;
    public int touchTargetDestroyFrame = -1;
    public FlyerPathCfg pathCfg = new FlyerPathCfg();
    public string endEventGroupId = string.Empty;
    public enFlyerTargetType targetType = enFlyerTargetType.autoTarget;

    static string[] s_flyerIds;
    static Dictionary<string, FlyerCfg> s_cfgs = new Dictionary<string, FlyerCfg>();

    public static string[] FlyerIds
    {
        get
        {
            if (s_flyerIds == null)
            {
                s_flyerIds = new string[SkillCfgMgr.instance.flyers.Count + 2];
                int i = 0;
                foreach (FlyerFile c in SkillCfgMgr.instance.flyers)
                    s_flyerIds[i++] = c.file;

                System.Array.Sort(s_flyerIds, 0, SkillCfgMgr.instance.flyers.Count);

                s_flyerIds[i++] = string.Empty;
                s_flyerIds[i++] = "新增飞出物";
            }
            return s_flyerIds;
        }
    }
    public static void Reset()
    {
        s_flyerIds = null;
    }

    public static void PreLoad(string file)
    {
        if(string.IsNullOrEmpty(file))
            return;
        if (s_cfgs.ContainsKey(file))
            return;
        FlyerCfg cfg =Get(file);
        if (cfg == null)
            return;

        cfg.eventGroup.PreLoad();
        SkillEventGroupCfg.PreLoad(cfg.endEventGroupId);
        cfg.endFlyerCreateCfg.PreLoad();
        if (!string.IsNullOrEmpty(cfg.endFlyer))
            FlyerCfg.PreLoad(cfg.endFlyer);
    }

    public static FlyerCfg Get(string file)
    {
        FlyerCfg cfg = s_cfgs.Get(file);
        if (cfg != null)
            return cfg;

        cfg = Util.LoadJsonFile<FlyerCfg>("skill/" + file);
        if (cfg == null)
        {
            Debuger.LogError("找不到飞出物:{0}",file);
            return null;
        }

        s_cfgs[cfg.file] = cfg;
        return cfg;
    }

    public static FlyerCfg Add(string file)
    {
        if (string.IsNullOrEmpty(file)||System.Array.IndexOf(FlyerIds, file) != -1)
        {
            Debuger.LogError("不能创建，飞出物名为空或者已经存在:{0}",file);
            return null;
        }


        FlyerCfg cfg = new FlyerCfg();
        cfg.file = file;
        s_cfgs[cfg.file] = cfg;

        //添加下索引
        FlyerFile f = new FlyerFile();
        f.file = cfg.file;
        SkillCfgMgr.instance.flyers.Add(f);
        SkillCfgMgr.instance.Save();

        cfg.Save();

        //要重新计算一些索引信息
        Reset();

        return cfg;
    }
    public static void RemoveCache(FlyerCfg cfg)
    {
        if (cfg == null) return;
        FlyerCfg cacheCfg = s_cfgs.Get(cfg.file);
        if (cacheCfg == cfg)
            s_cfgs.Remove(cfg.file);

    }
    public static void Remove(FlyerCfg cfg)
    {
        if (cfg == null) return;

        //从已经加载的配置中删除
        if (s_cfgs.ContainsKey(cfg.file))
        {
            s_cfgs.Remove(cfg.file);
        }

        //从SkillCfgMgr的索引中删除
        for (int i = 0; i < SkillCfgMgr.instance.eventGroups.Count; ++i)
        {
            if (cfg.file == SkillCfgMgr.instance.eventGroups[i].file)
            {
                SkillCfgMgr.instance.eventGroups.RemoveAt(i);
                SkillCfgMgr.instance.Save();
                break;
            }
        }

        //删除本地文件
        string path = Application.dataPath + "/Config/Resources/skill/" + cfg.file + ".json";
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

    }

    public void Save()
    {
        Util.SaveJsonFile("skill/" + file, this);
        SkillCfgMgr.instance.Save();
    }
}
