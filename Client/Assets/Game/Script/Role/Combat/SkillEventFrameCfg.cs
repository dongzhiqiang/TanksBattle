#region Header
/**
 * 名称：技能帧事件
 
 * 日期：2015.9.28
 * 描述：
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif


public enum enSkillEventTargetType
{
    selfAlway,      //释放者，总是执行到，除非角色不可战斗了
    self,           //释放者，受碰撞检测
    enemy,          //敌人阵营
    same,           //友方，同一阵营但是不是自己
    neutral,        //中立阵营
    selfSame,       //友方和自己
    target,         //当前技能的目标，使用技能的时候可能会指定目标，否则就会自动选择目标
    exceptSelf,       //除了自己
    //master,         //主人，总是执行到，除非角色不可战斗了    
    //pet,            //宠物
    //summon,         //召唤物
    //master,         //主人，或者召唤者
    max
}



public enum enSkillEventFrameType
{
    once,           //单帧
    multi,          //多帧
    //buff,           //缓冲后帧，也就是每一帧都判断，如果达到触发条件了，那么结束的时候执行
    max
}

//对象排序类型
public enum enTargetOrderType
{
    none,//默认，一般是按照创建顺序
    distance,//距离
}


public class TargetRangeCfg
{
    public RangeCfg range= new RangeCfg();//碰撞检测的范围
    public enSkillEventTargetType targetType = enSkillEventTargetType.selfAlway;//对象类型

    public void CopyFrom(TargetRangeCfg cfg)
    {
        if (cfg == null) return;

        //复制其他
        range.CopyFrom(cfg.range);

        //复制值类型的属性
        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance);

    }
}

public class SkillEventFrameCfg
{
   
    #region Fields
    public static string[] FrameTypeName = new string[] { "单帧", "多帧" };
    public static string[] TargetTypeName = new string[] { "释放者", "释放者(碰)", "敌方", "友方", "中立", "友方和自己","技能目标","除了自己" };
    public static string[] TargetOrderTypeName = new string[] { "无","距离(近)"};

    public int id;
    

    //帧数相关
    public enSkillEventFrameType frameType = enSkillEventFrameType.once;
    public int frameBegin=0;
    public int frameEnd=-1;//表明到技能结束
    public int frameInterval=1;//间隔多少帧
    

    //触发次数的控制
    public int countLimit = -1;//触发次数限制，总的作用对象次数不能超过这个值
    public int countTargetLimit = -1;//对象触发次数限制，对同一个作用对象而言触发次数不能超过这个值
    public int countFrameLimit = -1;//当前帧触发的次数限制，当前帧能同时作用的对象的数量的上限
    public int frameLimit = -1;//有效帧次数，如果一帧作用过至少一个对象，那么就算有效帧，有效帧超过这个值那么就不会再执行

    //是不是洗牌,每次都碰撞检测后都洗牌
    public enTargetOrderType targetOrderType = enTargetOrderType.none;

    //前置事件
    public List<SkillEventConditionCfg> conditions= new List<SkillEventConditionCfg>();//前置条件,如果前置事件没有执行，那么不执行
    public List<SkillEventConditionCfg> targetConditions = new List<SkillEventConditionCfg>();//前置对象条件，如果前置事件没有对这个作用对象执行过，那么不执行
    
    //作用对象
    public List<TargetRangeCfg> targetRanges = new List<TargetRangeCfg>();

    //事件相关
    public List<SkillEventCfg> events = new List<SkillEventCfg>();

    //技能编辑器ui相关，也保存到配置表里吧
    public int selId = 0;
    
    #endregion


    #region Properties
    
    #endregion


    #region Constructors
   
    #endregion

    #region Private Methods
    
    #endregion
    //预加载
    public void PreLoad()
    {
        for(int i = 0;i<events.Count;++i)
            events[i].PreLoad();
    }

    //复制
    public void CopyFrom(SkillEventFrameCfg cfg)
    {
        if (cfg == null) return;

        //复制值类型的属性
        Util.Copy(cfg, this, BindingFlags.Public | BindingFlags.Instance, "file");

        //复制其他
        conditions.Clear();
        foreach (SkillEventConditionCfg c2 in cfg.conditions)
        {
            SkillEventConditionCfg c = SkillEventFactory.CreateCondition(c2.Type);
            c.CopyFrom(c2);
            conditions.Add(c);
        }

        targetConditions.Clear();
        foreach (SkillEventConditionCfg c2 in cfg.targetConditions)
        {
            SkillEventConditionCfg c = SkillEventFactory.CreateCondition(c2.Type);
            c.CopyFrom(c2);
            targetConditions.Add(c);
        }

        targetRanges.Clear();
        foreach (TargetRangeCfg c2 in cfg.targetRanges)
        {
            TargetRangeCfg c = new TargetRangeCfg();
            c.CopyFrom(c2);
            targetRanges.Add(c);
        }

        events.Clear();
        foreach (SkillEventCfg c2 in cfg.events)
        {
            SkillEventCfg c = SkillEventFactory.Create(c2.Type);;
            c.CopyFrom(c2);
            events.Add(c);
        }
    }

    public SkillEventCfg GetById(int id)
    {
        foreach (SkillEventCfg c in events)
        {
            if (c.id == id)
                return c;
        }
        return null;
    }
}
