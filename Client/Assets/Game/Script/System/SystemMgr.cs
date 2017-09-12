using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class SystemCond
{
    public abstract bool IsFullfilled(Role role, out string errMsg);
}

class SystemCondEnable : SystemCond
{
    public override bool IsFullfilled(Role role, out string errMsg) { errMsg = ""; return true; }
}

class SystemCondDisable : SystemCond
{
    public override bool IsFullfilled(Role role, out string errMsg) { errMsg = "暂不开启"; return false; }
}

class SystemCondPassLevel : SystemCond
{
    string levelId;
    public SystemCondPassLevel(string levelId)
    {
        this.levelId = levelId;
    }

    public override bool IsFullfilled(Role role, out string errMsg)
    {
        var level = role.LevelsPart.GetLevelInfoById(levelId);
        if(level != null && level.isWin)
        {
            errMsg = "";
            return true;
        }
        else
        {
            RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(levelId);
            if (roomCfg==null)
            {
                errMsg = "条件关卡不存在:" + levelId;
            }
            else errMsg = "需要通过关卡" + roomCfg.roomName;
            return false;
        }
    }
}

class SystemCondLevel : SystemCond
{
    int level;
    public SystemCondLevel(int level)
    {
        this.level = level;
    }

    public override bool IsFullfilled(Role role, out string errMsg)
    {
        if (role.GetInt(enProp.level)>=level)
        {
            errMsg = "";
            return true;
        }
        else
        {

            errMsg = "角色需要达到" + level + "级";
            return false;
        }
    }
}

struct TimePair
{
    public TimePair(CronTime from, CronTime to)
    {
        this.from = from;
        this.to = to;
    }
    public CronTime from;
    public CronTime to;
}

class SystemCondTime : SystemCond
{
    List<TimePair> times;
    public SystemCondTime(List<TimePair> times)
    {
        this.times = times;
    }

    public override bool IsFullfilled(Role role, out string errMsg)
    {
        var date = TimeMgr.instance.GetServerDateTime();
        foreach (TimePair timePair in times)
        {
            if (timePair.from.LessThanOrEqual(date) && timePair.to.GreaterThan(date))
            {
                errMsg = "";
                return true;
            }
        }

        
        errMsg = "";
        bool first = true;
        foreach (TimePair timePair in times)
        {
            if(!first)
            {
                errMsg += ",";
            }
            errMsg += timePair.from.GetString(first) + "到" + timePair.to.GetString();
            first = false;
        }
        errMsg += "开启";
        //errMsg = "尚未到达开启时间内";
            
        return false;
        
    }
}

public class SystemMgr : Singleton<SystemMgr>
{
    #region Fields
    EventNotifier m_notifier = EventMgr.Get();
    Dictionary<enSystem, bool> m_systemTips = new Dictionary<enSystem,bool>();
    #endregion

    #region Public Method
    //激活通知
    public void FireActive(enSystem systemId)
    {
        m_notifier.Fire(MSG.MSG_SYSTEM, MSG_SYSTEM.ACTIVE, systemId);
    }
    //添加激活监听
    public void AddActiveListener(EventObserver.OnFire1 onFire)
    {
        m_notifier.Add(MSG.MSG_SYSTEM, MSG_SYSTEM.ACTIVE, onFire);
    }

    //设置红点
    public void SetTip(enSystem systemId, bool value=true)
    {
        if (m_systemTips.ContainsKey(systemId) && m_systemTips[systemId] == value)
        {
            return;
        }
        m_systemTips[systemId] = value;
        FireTip(systemId);
    }

    public bool IsTip(enSystem systemId)
    {
        bool result;
        if(!m_systemTips.TryGetValue(systemId, out result))
        {
            return false;
        }
        return result;
    }

    //添加红点监听
    public void AddTipListener(EventObserver.OnFire1 onFire)
    {
        m_notifier.Add(MSG.MSG_SYSTEM, MSG_SYSTEM.TIP, onFire);
    }

    public SystemCond CreateActiveCond(string condStr)
    {
        string[] condStrs = condStr.Split('|');
        SystemCond result = null;
        switch(int.Parse(condStrs[0]))
        {
            case SYSTEM_ACTIVE_TYPE.ACTIVE:
                result = new SystemCondEnable();
                break;
            case SYSTEM_ACTIVE_TYPE.LEVEL:
                result = new SystemCondLevel(int.Parse(condStrs[1]));
                break;
            case SYSTEM_ACTIVE_TYPE.PASS_LEVEL:
                result = new SystemCondPassLevel(condStrs[1]);
                break;
            default:
                Debuger.LogError("未实现的系统激活类型：" + condStrs[0]);
                break;
        }

        return result;
    }

    public SystemCond CreateOpenCond(string condStr)
    {
        string[] condStrs = condStr.Split('|');
        SystemCond result = null;
        switch (int.Parse(condStrs[0]))
        {
            case SYSTEM_OPEN_TYPE.TIME:
                List<TimePair> timePairs = new List<TimePair>();
                for (int i = 1; i < condStrs.Length; i++ )
                {
                    if(!string.IsNullOrEmpty(condStrs[i]))
                    {
                        string[] times = condStrs[i].Split('-');
                        timePairs.Add(new TimePair(new CronTime(times[0]), new CronTime(times[1])));
                    }
                }
                result = new SystemCondTime(timePairs);
                break;
            case SYSTEM_OPEN_TYPE.LEVEL:
                result = new SystemCondLevel(int.Parse(condStrs[1]));
                break;
            default:
                Debuger.LogError("未实现的系统开启类型：" + condStrs[0]);
                break;
        }

        return result;
    }

    public SystemCond CreateVisibleCond(string condStr)
    {
        string[] condStrs = condStr.Split('|');
        SystemCond result = null;
        switch (int.Parse(condStrs[0]))
        {
            case SYSTEM_VIS_TYPE.VISIBLE:
                result = new SystemCondEnable();
                break;
            case SYSTEM_VIS_TYPE.INVISIBLE:
                result = new SystemCondDisable();
                break;
            default:
                Debuger.LogError("未实现的系统显示类型：" + condStrs[0]);
                break;
        }

        return result;
    }

    // 是否激活
    public bool IsActive(enSystem systemId, out string errMsg)
    {
        Role role = RoleMgr.instance.Hero;
        SystemData system = role.SystemsPart.GetSystem((int)systemId);
        errMsg = "";
        if(system != null && system.Active)
        {
            return true;
        }
        SystemCfg cfg = SystemCfg.m_cfgs.Get((int)systemId);
        if (cfg == null)
            return true;
        foreach(SystemCond cond in cfg.activeConds)
        {
            if(!cond.IsFullfilled(role, out errMsg))
            {
                return false;
            }
        }
        return true;
    }

    // 是否开启
    public bool IsOpen(enSystem systemId, out string errMsg)
    {
        errMsg = "";
        Role role = RoleMgr.instance.Hero;
        SystemCfg cfg = SystemCfg.m_cfgs.Get((int)systemId);
        if (cfg == null)
            return true;
        foreach (SystemCond cond in cfg.openConds)
        {
            if (!cond.IsFullfilled(role, out errMsg))
            {
                return false;
            }
        }
        
        return true;
    }

    // 是否可见
    public bool IsVisible(enSystem systemId, out string errMsg)
    {
        errMsg = "";
        Role role = RoleMgr.instance.Hero;
        SystemCfg cfg = SystemCfg.m_cfgs.Get((int)systemId);
        if (cfg == null)
            return true;
        return cfg.visibilityCond.IsFullfilled(role, out errMsg);
    }




    public bool IsNotGrey(enSystem systemId, out string errMsg)
    {
        return IsActive(systemId, out errMsg) && IsOpen(systemId, out errMsg);
    }

    // 是否灰色
    public bool IsGrey(enSystem systemId, out string errMsg)
    {
        return !IsNotGrey(systemId, out errMsg);
    }

    // 是否可用
    public bool IsEnabled(enSystem systemId, out string errMsg)
    {
        return IsVisible(systemId, out errMsg) && IsNotGrey(systemId, out errMsg);
    }
    #endregion

    #region Private Method
    
    void FireTip(enSystem systemId)
    {
        m_notifier.Fire(MSG.MSG_SYSTEM, MSG_SYSTEM.TIP, systemId);
    }
    #endregion
}