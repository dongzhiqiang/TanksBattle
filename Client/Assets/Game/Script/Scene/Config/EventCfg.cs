using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using LitJson;

[JsonCanInherit]
public class EventCfg : CheckCfgBase
{
    public SceneCfg.EventType mType { get; set; }

    public EventCfg()
    {

    }
}

public class EventCfg_NpcDead : EventCfg
{
    public string npcID;
    public int count;
    public bool bDeadStart = false;

    public EventCfg_NpcDead()
    {
        mType = SceneCfg.EventType.NpcIDDead;

#if UNITY_EDITOR
        TypeDesc = "NPC死亡";
        ParamDesc = new string[] { "NpcID", "个数", "死亡开始触发" };
#endif
    }
}

public class EventCfg_Win : EventCfg
{
    public EventCfg_Win()
    {
        mType = SceneCfg.EventType.Win;

#if UNITY_EDITOR
        TypeDesc = "胜利";
#endif
    }
}

public class EventCfg_Lose : EventCfg
{
    public EventCfg_Lose()
    {
        mType = SceneCfg.EventType.Lose;

#if UNITY_EDITOR
        TypeDesc = "失败";
#endif
    }
}

public class EventCfg_EnterTime : EventCfg
{
    public int secNum;
    public EventCfg_EnterTime()
    {
        mType = SceneCfg.EventType.EnterTime;

#if UNITY_EDITOR
        TypeDesc = "进入时长";
        ParamDesc = new string[] { "秒数" };
#endif

    }
}

public class EventCfg_StartLevel : EventCfg
{
    public EventCfg_StartLevel()
    {
        mType = SceneCfg.EventType.StartLevel;

#if UNITY_EDITOR
        TypeDesc = "开始关卡";
        ParamDesc = new string[] { };
#endif

    }
}

public class EventCfg_Area : EventCfg
{
    public string areaFlag = "";
    public bool bEnter = false;
    public bool bHaveHero = true;
    public string NpcIdList = "";
    public int timeNum = 0;
    public EventCfg_Area()
    {
        mType = SceneCfg.EventType.Area;

#if UNITY_EDITOR
        TypeDesc = "区域事件";
        ParamDesc = new string[] { AreaFlagDesc, "是否进入(不勾选就走出)", "包括主角", "NpcId列表,分割", "时长(N秒后触发)" };
#endif

    }
}

public class EventCfg_RoleEnter : EventCfg
{
    public string flagId = "";
    public EventCfg_RoleEnter()
    {
        mType = SceneCfg.EventType.RoleEnter;

#if UNITY_EDITOR
        TypeDesc = "角色进入";
        ParamDesc = new string[] { RoleFlagDesc };
#endif

    }
}

public class EventCfg_RoleBlood : EventCfg
{
    public string flagId = "";
    public int percent = 0;
    public bool bUnder = false;
    public EventCfg_RoleBlood()
    {
        mType = SceneCfg.EventType.RoleBlood;

#if UNITY_EDITOR
        TypeDesc = "角色血量";
        ParamDesc = new string[] { RoleFlagDesc, "百分比", "是否低于(不勾选就高于)" };
#endif

    }
}

public class EventCfg_RoleDead : EventCfg
{
    public string flagId = "";
    public bool bDeadStart = false;
    public EventCfg_RoleDead()
    {
        mType = SceneCfg.EventType.RoleDead;

#if UNITY_EDITOR
        TypeDesc = "角色死亡";
        ParamDesc = new string[] { RoleFlagDesc, "死亡开始触发" };
#endif

    }
}

public class EventCfg_RefreshDead : EventCfg
{
    public string flagId = "";
    public bool bDeadStart = false;
    public EventCfg_RefreshDead()
    {
        mType = SceneCfg.EventType.RefreshDead;

#if UNITY_EDITOR
        TypeDesc = "刷新点怪全死";
        ParamDesc = new string[] { RefreshFlagDesc, "死亡开始触发" };
#endif

    }
}

public class EventCfg_RoleNum : EventCfg
{
    public int roleNum = 0;
    public bool bUnder = false;
    public string roleId = "";
    public EventCfg_RoleNum()
    {
        mType = SceneCfg.EventType.RoleNum;

#if UNITY_EDITOR
        TypeDesc = "角色数量";
        ParamDesc = new string[] { "个数", "是否低于(不勾选就高于)", "角色ID(空表所有)"};
#endif

    }
}

public class EventCfg_GroupDeadNum : EventCfg
{
    public string groupId = "";
    public int deadNum = 0;
    public bool bDeadStart = false;
    public EventCfg_GroupDeadNum()
    {
        mType = SceneCfg.EventType.GroupDeadNum;

#if UNITY_EDITOR
        TypeDesc = "刷新组死亡N个";
        ParamDesc = new string[] { RefreshFlagDesc, "死亡个数", "死亡开始触发" };
#endif

    }
}

public class EventCfg_None : EventCfg
{
    public EventCfg_None()
    {
        mType = SceneCfg.EventType.None;

#if UNITY_EDITOR
        TypeDesc = "无触发";
#endif

    }
}

public class EventCfg_FinishEvent : EventCfg
{
    public string eventId;
    public EventCfg_FinishEvent()
    {
        mType = SceneCfg.EventType.FinishEvent;

#if UNITY_EDITOR
        TypeDesc = "完成事件";
        ParamDesc = new string[] { EventFlagDesc };
#endif

    }
}


public class EventCfgFactory : Singleton<EventCfgFactory>
{
    public static Dictionary<string, EventCfg> cfgMap = new Dictionary<string, EventCfg>();

    public EventCfg GetEventCfg(SceneCfg.EventType cfgType, string content = "")
    {
        EventCfg eventCfg = null;
        switch (cfgType)
        {
            case SceneCfg.EventType.None:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_None();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_None>(content);
                break;
            case SceneCfg.EventType.EnterTime:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_EnterTime();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_EnterTime>(content);
                break;
            case SceneCfg.EventType.NpcIDDead:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_NpcDead();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_NpcDead>(content);
                break;
            case SceneCfg.EventType.Win:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_Win();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_Win>(content);
                break;
            case SceneCfg.EventType.Area:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_Area();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_Area>(content);
                break;
            case SceneCfg.EventType.Lose:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_Lose();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_Lose>(content);
                break;
            case SceneCfg.EventType.RoleDead:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_RoleDead();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_RoleDead>(content);
                break;
            case SceneCfg.EventType.RoleEnter:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_RoleEnter();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_RoleEnter>(content);
                break;
            case SceneCfg.EventType.StartLevel:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_StartLevel();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_StartLevel>(content);
                break;
            case SceneCfg.EventType.RefreshDead:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_RefreshDead();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_RefreshDead>(content);
                break;
            case SceneCfg.EventType.RoleBlood:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_RoleBlood();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_RoleBlood>(content);
                break;
            case SceneCfg.EventType.RoleNum:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_RoleNum();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_RoleNum>(content);
                break;
            case SceneCfg.EventType.GroupDeadNum:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_GroupDeadNum();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_GroupDeadNum>(content);
                break;
            case SceneCfg.EventType.FinishEvent:
                if (string.IsNullOrEmpty(content))
                    eventCfg = new EventCfg_FinishEvent();
                else
                    eventCfg = JsonMapper.ToObject<EventCfg_FinishEvent>(content);
                break;
        }
        return eventCfg;
    }

    public Dictionary<string, EventCfg> GetAllEventCfg()
    {
        if (cfgMap.Count > 0)
            return cfgMap;

        Type eventType = typeof(SceneCfg.EventType);
        foreach (int cfgType in Enum.GetValues(eventType))
        {
            EventCfg cfg = GetEventCfg((SceneCfg.EventType)cfgType);
            if (cfg != null)
            {
                cfgMap[cfg.GetTypeDesc()] = cfg;
            }
            else
            {
                //Debug.LogError("有定义的事件类型 {0}" + cfgType + "，没有创建成功");
            }
        }

        return cfgMap;
    }

}