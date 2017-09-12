using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ActivityPart : RolePart
{
    #region Fields
    Dictionary<enActProp, Property> m_props = new Dictionary<enActProp, Property>();
    //勇士试炼信息
    WarriorTriedInfo m_warriorTried;
    //预言者之塔信息
    ProphetTowerInfo m_prophetTower;
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.activity; } }
    public Dictionary<enActProp, Property> Props { get { return m_props; } }
    public WarriorTriedInfo warriorTried { get { return m_warriorTried; }set { m_warriorTried = value; } }

    public ProphetTowerInfo prophetTower { get { return m_prophetTower; } set { m_prophetTower = value; } }
    #endregion

    private TimeMgr.Timer m_checkTimer = null;
    //当前试炼的关卡索引 1开始
    public int warrIndex;

    #region Frame
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        if (vo.actProps == null)
            return;

        foreach (var e in vo.actProps)
        {
            try
            {
                enActProp prop = (enActProp)Enum.Parse(typeof(enActProp), e.Key);
                m_props[prop] = e.Value;
            }
            catch (Exception err)
            {
                Debuger.LogError("转换成enActProp失败", err.Message);
            }
        }

        if (vo.warriorTried != null)
            m_warriorTried = vo.warriorTried;

        if (vo.prophetTower != null)
            m_prophetTower = vo.prophetTower;
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
    }

    public override void OnClear()
    {
        if (m_checkTimer != null)
        {
            m_checkTimer.Release();
            m_checkTimer = null;
        }

        if (m_props != null)
            m_props.Clear();
    }

    public void OnSyncProps(SyncActivityPropVo vo)
    {
        if (vo.props.Count <= 0)
            return;

        List<int> temp = new List<int>();

        //先填写完整
        foreach (var e in vo.props)
        {
            try
            {
                enActProp prop = (enActProp)Enum.Parse(typeof(enActProp), e.Key);
                m_props[prop] = e.Value;
                temp.Add((int)prop);
            }
            catch (Exception err)
            {
                Debuger.LogError("转换成enActProp失败", err.Message);
            }
        }

        //再触发事件
        //特定属性的事件
        foreach (var propId in temp)
        {
            m_parent.Fire(MSG_ROLE.ACT_PROP_CHANGE + propId, propId);
        }
        //不特定属性的事件
        m_parent.Fire(MSG_ROLE.NET_ACT_PROP_SYNC, 0);        
    }
    #endregion


    #region Private Methods   
    #endregion

    public int GetInt(enActProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Int;
        return 0;
    }

    public long GetLong(enActProp prop) 
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Long;
        return 0;
    }

    public float GetFloat(enActProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Float;
        return 0.0f;
    }

    public string GetString(enActProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.String;
        return "";
    }

    public bool IsComplete(enSystem actSysId)
    {
        switch (actSysId)
        {
            case enSystem.goldLevel:
                {
                    GoldLevelBasicCfg basicCfg = GoldLevelBasicCfg.Get();

                    long goldLvlTime = GetLong(enActProp.goldLvlTime);
                    int goldLvlCnt = GetInt(enActProp.goldLvlCnt);

                    long curTime = TimeMgr.instance.GetTimestamp();

                    if (!TimeMgr.instance.IsToday(goldLvlTime))
                        goldLvlCnt = 0;

                    if (goldLvlCnt >= basicCfg.dayMaxCnt)
                    {
                        return true;
                    }
                }
                return false;
            case enSystem.hadesLevel:
                {
                    HadesLevelBasicCfg basicCfg = HadesLevelBasicCfg.Get();

                    long hadesLvlTime = GetLong(enActProp.hadesLvlTime);
                    int hadesLvlCnt = GetInt(enActProp.hadesLvlCnt);

                    if (!TimeMgr.instance.IsToday(hadesLvlTime))
                        hadesLvlCnt = 0;

                    if (hadesLvlCnt >= basicCfg.dayMaxCnt)
                    {
                        return true;
                    }
                }
                return false;
            case enSystem.venusLevel:
                {
                    VenusLevelBasicCfg basicCfg = VenusLevelBasicCfg.Get();

                    long venusLvlTime = GetLong(enActProp.venusLvlTime);
                    int venusLvlEnt1 = GetInt(enActProp.venusLvlEntered1);
                    int venusLvlEnt2 = GetInt(enActProp.venusLvlEntered2);

                    if (!TimeMgr.instance.IsToday(venusLvlTime))
                    {
                        venusLvlEnt1 = 0;
                        venusLvlEnt2 = 0;
                    }

                    if (venusLvlEnt1>0 && venusLvlEnt2>0)
                    {
                        return true;
                    }
                }
                return false;
            case enSystem.guardLevel:
                return false;
        }
        return false;
    }

    bool IsEnterCondition(enSystem actSysId)
    {
        switch(actSysId)
        {
            case enSystem.goldLevel:
                {
                    GoldLevelBasicCfg basicCfg = GoldLevelBasicCfg.Get();

                    long goldLvlTime = GetLong(enActProp.goldLvlTime);
                    int goldLvlCnt = GetInt(enActProp.goldLvlCnt);

                    if (!TimeMgr.instance.IsToday(goldLvlTime))
                        goldLvlCnt = 0;

                    if (basicCfg.dayMaxCnt <= goldLvlCnt)
                    {
                        //没可用次数了
                        return false;
                    }
                    else
                    {
                        long curTime = TimeMgr.instance.GetTimestamp();
                        long timePass = curTime >= goldLvlTime ? curTime - goldLvlTime : goldLvlTime - curTime;
                        //看看是否还在冷却中
                        return timePass >= basicCfg.coolDown;
                    }
                }
            case enSystem.hadesLevel:
                {
                    HadesLevelBasicCfg basicCfg = HadesLevelBasicCfg.Get();

                    long hadesLvlTime = GetLong(enActProp.hadesLvlTime);
                    int hadesLvlCnt = GetInt(enActProp.hadesLvlCnt);

                    if (!TimeMgr.instance.IsToday(hadesLvlTime))
                        hadesLvlCnt = 0;

                    if (basicCfg.dayMaxCnt <= hadesLvlCnt)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            case enSystem.venusLevel:
                {
                    VenusLevelBasicCfg basicCfg = VenusLevelBasicCfg.Get();

                    long venusLvlTime = GetLong(enActProp.venusLvlTime);
                    int venusLvlEnt1 = GetInt(enActProp.venusLvlEntered1);
                    int venusLvlEnt2 = GetInt(enActProp.venusLvlEntered2);

                    if (!TimeMgr.instance.IsToday(venusLvlTime))
                    {
                        venusLvlEnt1 = 0;
                        venusLvlEnt2 = 0;
                    }

                    var index = 0;
                    if (TimeMgr.instance.IsNowBetweenTime(basicCfg.openTime1, 0, basicCfg.closeTime1, 0))
                    {
                        index = 1;
                    }
                    else if (TimeMgr.instance.IsNowBetweenTime(basicCfg.openTime2, 0, basicCfg.closeTime2, 0))
                    {
                        index = 2;
                    }

                    if (index == 0)
                    {
                        return false;
                    }

                    //次数检查
                    if ((index == 1 && venusLvlEnt1 > 0) || (index == 2 && venusLvlEnt2 > 0))
                    {
                        return false;
                    }
                }
                return true;
            case enSystem.guardLevel:
                return false;
            case enSystem.prophetTower:
                Role hero = RoleMgr.instance.Hero;
                int topTower = hero.GetInt(enProp.towerLevel) + 1;
                int level = topTower == 0 ? 1 : topTower;
                ProphetTowerCfg towerCfg = ProphetTowerCfg.Get(level);
                if (towerCfg == null)
                    return false;
                RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(towerCfg.roomId);
                if (roomCfg == null)
                    return false;

                //战力符合挑战
                if((float)hero.GetInt(enProp.powerTotal) > roomCfg.powerNum)
                    return true;

                //随机模式有剩余次数
                int enterNum = 0;
                if (!TimeMgr.instance.IsToday((long)hero.GetInt(enProp.towerEnterTime)))
                    enterNum = 0;
                else
                    enterNum = hero.GetInt(enProp.towerEnterNums);
                if (enterNum < 5)
                    return true;

                //可领取奖励
                for (int i = 0; i < 5; i++)
                {
                    if (enterNum > i && m_prophetTower.getRewardState[i] == 0)
                        return true;
                }

                return false;
        }
        return false;
    }

    public bool CanEnter(enSystem actSysId)
    {
        string errMsg = "";
        if(!SystemMgr.instance.IsEnabled(actSysId, out errMsg))
        {
            return false;
        }
        return IsEnterCondition(actSysId);
    }

    public void CheckActivityTip()
    {
        enSystem[] checkActivity = { enSystem.goldLevel, enSystem.hadesLevel, enSystem.venusLevel, enSystem.guardLevel, enSystem.prophetTower };
        bool hasActivityCanEnter = false;
        foreach(enSystem actSysId in checkActivity)
        {
            if(CanEnter(actSysId))
            {
                hasActivityCanEnter = true;
                SystemMgr.instance.SetTip(actSysId, true);
            }
            else
            {
                SystemMgr.instance.SetTip(actSysId, false);
            }
        }
        SystemMgr.instance.SetTip(enSystem.activity, hasActivityCanEnter);
    }

    public void InitCheckActivityTip()
    {

        CheckActivityTip();
        m_parent.Add(MSG_ROLE.NET_ACT_PROP_SYNC, CheckActivityTip);       
        //因为存在和时间相关的条件，一分钟检查一次
        if (m_checkTimer == null)
        {
            m_checkTimer = TimeMgr.instance.AddTimer(60, CheckActivityTip, -1, -1);
        }
    }
}