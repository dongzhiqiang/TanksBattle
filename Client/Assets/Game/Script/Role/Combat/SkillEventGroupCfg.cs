#region Header
/**
 * 名称：帧数表
 
 * 日期：2015.9.28
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Reflection;

public class SkillEventGroupFile
{
    //public int id;
    public string file;

}
public class SkillEventGroupCfg
{
    //public int id;
    public string file = "skill_event_group";
    public bool isFile=false;//是自己成一个单独的文件，还是放在技能和飞出物表中
    public List<SkillEventFrameCfg> frames = new List<SkillEventFrameCfg>();

    //技能编辑器ui相关，也保存到配置表里吧
    public int selId = 0;
    public Vector2 allScroll= Vector2.zero;
    public Vector2 contentScroll = Vector2.zero;
    public float maxWidth = 0;
    AniFxGroup m_aniFxGroup = null;//编辑器用

 
    

    static string[] groupIds;
    static Dictionary<string, SkillEventGroupCfg> s_cfgs = new Dictionary<string, SkillEventGroupCfg>();
    
    public static string[] GroupIds
    {
        get
        {
            if (groupIds == null)
            {
                groupIds = new string[SkillCfgMgr.instance.eventGroups.Count+2];
                int i = 0;
                foreach (SkillEventGroupFile c in SkillCfgMgr.instance.eventGroups)
                    groupIds[i++] = c.file;

                System.Array.Sort(groupIds,0,SkillCfgMgr.instance.eventGroups.Count);

                groupIds[i++] = string.Empty;
                groupIds[i++] = "新增事件组";
            }
            return groupIds;
        }
    }

    public AniFxGroup AniFxGroup { get{return m_aniFxGroup;}set{m_aniFxGroup = value;}}//编辑器用

    

    public static void Reset()
    {
        groupIds = null;
    }

    public static SkillEventGroupCfg Get(string file,bool newIfNoExist =true)
    {
        SkillEventGroupCfg cfg = s_cfgs.Get(file);
        if (cfg != null)
            return cfg;

        cfg = Util.LoadJsonFile<SkillEventGroupCfg>("skill/" + file); 
        if (cfg == null)
        {
            if (!newIfNoExist)
                return null;
            cfg = new SkillEventGroupCfg();
            cfg.file = file;
        }

        s_cfgs[cfg.file] = cfg;
        return cfg;
    }

    public static SkillEventGroupCfg Add(string file)
    {
        if (string.IsNullOrEmpty(file)){
            
            Debuger.LogError("创建事件组不能传进来空文件名");
            return null;
        }

        if (System.Array.IndexOf(SkillEventGroupCfg.GroupIds, file) != -1)
        {
            Debuger.LogError("事件组id已经存在，不能重复创建");
            return null;
        }

        SkillEventGroupCfg cfg = new SkillEventGroupCfg();
        cfg.file =file;
        cfg.isFile =true;

        SkillEventGroupFile f = new SkillEventGroupFile();
        f.file = cfg.file;
        SkillCfgMgr.instance.eventGroups.Add(f);
        SkillCfgMgr.instance.Save();
        cfg.Save();
        s_cfgs[cfg.file] = cfg;
        Reset();
        return cfg;
    }

    public static void RemoveCache(SkillEventGroupCfg cfg)
    {
        if (cfg == null) return;
        SkillEventGroupCfg cacheCfg = s_cfgs.Get(cfg.file);
        if (cacheCfg == cfg)
            s_cfgs.Remove(cfg.file);

    }

    public static void Remove(SkillEventGroupCfg group)
    {
        if(group == null)return;

        //从已经加载的配置中删除
        if(s_cfgs.ContainsKey(group.file)){
            s_cfgs.Remove(group.file);
        }

        //从SkillCfgMgr的索引中删除
        if (group.isFile)
        {
            for (int i = 0; i < SkillCfgMgr.instance.eventGroups.Count; ++i)
            {
                if (group.file == SkillCfgMgr.instance.eventGroups[i].file)
                {
                    SkillCfgMgr.instance.eventGroups.RemoveAt(i);
                    SkillCfgMgr.instance.Save();
                    break;
                }
            }
        }
        
        //删除本地文件
        string path =Application.dataPath + "/Config/Resources/skill/" + group.file+".json";
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

    }

    public static void PreLoad(string eventGroupId)
    {
        if(string.IsNullOrEmpty(eventGroupId))
            return;
        SkillEventGroupCfg cfg =  Get(eventGroupId,false);
        if (cfg == null) {
            Debuger.LogError("预加载的时候找不到事件组:{0}", eventGroupId);
            return;
        }

        cfg.PreLoad();
    }

    //预加载
    public void PreLoad()
    {
        for(int i=0;i<frames.Count;++i)
            frames[i].PreLoad();
    }

    //复制
    public void CopyFrom(SkillEventGroupCfg cfg)
    {
        if (cfg == null) return;

        //复制其他
        frames.Clear();
        foreach (SkillEventFrameCfg c in cfg.frames)
        {
            AddFrame(enSkillEventType.empty);
            frames[frames.Count - 1].CopyFrom(c);
        }

        //复制值类型的属性
        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance, "file");

    }

    public void Save()
    {
        Util.SaveJsonFile("skill/" + file, this);
        SkillCfgMgr.instance.Save();
    }

    public SkillEventFrameCfg GetById(int id)
    {
        foreach(SkillEventFrameCfg c in frames){
            if(c.id == id)
                return c;
        }
        return null;
    }

    public void AddFrame(enSkillEventType type)
    {
        int maxFrame = frames.Count == 0?0:frames[frames.Count-1].frameBegin;

        SkillEventFrameCfg frameCfg = new SkillEventFrameCfg();
        frameCfg.frameBegin =maxFrame;

        //添加默认的作用对象
        frameCfg.targetRanges.Add(new TargetRangeCfg());

        //添加一个事件
        SkillEventCfg eventCfg = AddEvent(frameCfg, type);
        if (eventCfg == null)
            return;

        int id = frames.Count + 1;
        while (GetById(id) != null)
            ++id;

        frameCfg.id = id;//++SkillCfgMgr.instance.eventFrameCounter;
        selId = frameCfg.id;
        frameCfg.selId = eventCfg.id;
        frames.Add(frameCfg);
    }

    public void RemoveFrame(SkillEventFrameCfg frameCfg){
        frames.Remove(frameCfg);
    }

    public SkillEventCfg AddEvent(SkillEventFrameCfg frameCfg, enSkillEventType type)
    {
        SkillEventCfg eventCfg = SkillEventFactory.Create(type);
        if (eventCfg == null)return null;

        int id = frameCfg.events.Count+1;
        while (frameCfg.GetById(id)!=null)
            ++id;

        eventCfg.id = id;
        frameCfg.events.Add(eventCfg);
        return eventCfg;
    }

    public void RemoveEvent(SkillEventFrameCfg frameCfg,SkillEventCfg eventCfg)
    {
        if (frameCfg.events.Count == 1)
        {
            Debuger.LogError("逻辑错误，剩下一个事件的时候不能删除");
            return;
        }

        frameCfg.events.Remove(eventCfg);
    }
    
}
