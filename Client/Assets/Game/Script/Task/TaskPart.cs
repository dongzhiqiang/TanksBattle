using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TaskPart : RolePart
{
    #region Fields
    private Dictionary<enTaskProp, Property> m_props = new Dictionary<enTaskProp, Property>();
    int m_vitality;
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.task; } }
    public Dictionary<enTaskProp, Property> Props { get { return m_props; } }
    public int CurVitality { get { return m_vitality; } set { m_vitality = value; } }
    #endregion

    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        return true;
    }
    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        if (vo.taskProps == null)
            return;

        foreach (var e in vo.taskProps)
        {
            try
            {
                enTaskProp prop = (enTaskProp)Enum.Parse(typeof(enTaskProp), e.Key);
                m_props[prop] = e.Value;
            }
            catch (Exception err)
            {
                Debuger.LogError("转换成enTaskProp失败", err.Message);               
            }
        }
        m_vitality = GetInt(enTaskProp.vitality);

        
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {


    }



    public override void OnClear()
    {

    }

    public void OnSyncProps(SyncTaskPropVo vo)
    {
        if (vo.props.Count <= 0)
            return;

        List<int> temp = new List<int>();

        //先填写完整
        foreach (var e in vo.props)
        {
            try
            {
                enTaskProp prop = (enTaskProp)Enum.Parse(typeof(enTaskProp), e.Key);
                m_props[prop] = e.Value;
                temp.Add((int)prop);
            }
            catch (Exception err)
            {
                Debuger.LogError("转换成enOpActProp失败", err.Message);
            }
        }
        m_vitality = GetInt(enTaskProp.vitality);
        CheckTip();
        


        /*     //再触发事件
             //特定属性的事件
             foreach (var propId in temp)
             {
                 m_parent.Fire(MSG_ROLE.ACT_PROP_CHANGE + propId, propId);
             }
             //不特定属性的事件
             m_parent.Fire(MSG_ROLE.NET_ACT_PROP_SYNC, 0);*/
    }
    #endregion


    #region Private Methods 
    
    void SetTaskTip(bool tip)
    {
        SystemMgr.instance.SetTip(enSystem.dailyTask, tip);
    }

    #endregion   
    public enRewardState CanGetDailyTaskReward(TaskRewardCfg taskRewardCfg)
    {
        Role hero = RoleMgr.instance.Hero;        
        int currentLevel = hero.GetInt(enProp.level);       
        if (currentLevel < taskRewardCfg.level)
        {
            return enRewardState.cantGetReward;
        }

        enTaskType taskType = (enTaskType)Enum.Parse(typeof(enTaskType), taskRewardCfg.taskType);
        switch (taskType)
        {
            case enTaskType.activity:
                {
                    ActivityTask task = new ActivityTask();
                    return (task.CanGetReward(taskRewardCfg.id));
                }
            case enTaskType.lottery:
                {
                    LotteryTask task = new LotteryTask();
                    return (task.CanGetReward(taskRewardCfg.id));
                }
            case enTaskType.cost:
                {
                    CostTask task = new CostTask();
                    return (task.CanGetReward(taskRewardCfg.id));
                }
            case enTaskType.corps:
                {
                    CorpsTask task = new CorpsTask();
                    return (task.CanGetReward(taskRewardCfg.id));
                }
            case enTaskType.vip:
                {
                    VipTask task = new VipTask();
                    return (task.CanGetReward(taskRewardCfg.id));
                }
            case enTaskType.upGrade:
            case enTaskType.prophetTower:
                {
                    UpgradeTask task = new UpgradeTask();
                    return (task.CanGetReward(taskRewardCfg.id));
                }
            case enTaskType.warriorTried:
                {
                    WarriorTriedTask task = new WarriorTriedTask();
                    return (task.CanGetReward(taskRewardCfg.id));
                }
            case enTaskType.eliteLv:
                {
                    EliteLvTask task = new EliteLvTask();
                    return (task.CanGetReward(taskRewardCfg.id));
                }
        }
        return enRewardState.cantGetReward;
    }

    public GrowthTask CanGetGrowthTaskReward(GrowthTaskCfg growthTaskCfg)
    {
        enTaskType taskType = (enTaskType)Enum.Parse(typeof(enTaskType), growthTaskCfg.type);
        enTaskProp taskProp = (enTaskProp)Enum.Parse(typeof(enTaskProp), growthTaskCfg.prop);
        long taskGetTime = GetLong(taskProp);
        Role hero = RoleMgr.instance.Hero;
        GrowthTask growthTask = new GrowthTask();
        int currentLevel = hero.GetInt(enProp.level);
        GrowthTaskStageCfg growthTaskStageCfg = GrowthTaskStageCfg.m_cfgs[growthTaskCfg.stage];

        if (currentLevel < growthTaskStageCfg.minLevel)
        {
            growthTask.taskState = enRewardState.cantGetReward;
        }
        else if (taskGetTime != 0)
        {
            growthTask.taskState = enRewardState.hasGetReward;
        }
        else
        {
            float current = 0;
            float target = 0;
            switch (taskType)
            {
                case enTaskType.normalLv:
                    {
                        List<int> paramList = growthTaskCfg.GetParamList();
                        int tarGetLevel = 0;
                        if (paramList.Count == 2)
                        {
                            target = paramList[0];
                            tarGetLevel = paramList[1];
                        }
                        else
                        {
                            Debuger.LogError("成长任务配置错误：参数长度错误");
                        }
                        LevelsPart levelsPart = hero.LevelsPart;
                        current = levelsPart.GetStarsByNodeId(tarGetLevel.ToString());
                        break;
                    }
                case enTaskType.specialLv:
                    {
                        List<int> paramList = growthTaskCfg.GetParamList();
                        int tarGetLevel = 0;
                        if (paramList.Count == 2)
                        {
                            target = paramList[0];
                            tarGetLevel = paramList[1];
                        }
                        else
                        {
                            Debuger.LogError("成长任务配置错误：参数长度错误");
                        }
                        LevelsPart levelsPart = hero.LevelsPart;
                        current = levelsPart.GetStarsByNodeId(tarGetLevel.ToString());
                        break;
                    }
                case enTaskType.weaponSkill:
                    {
                        List<int> paramList = growthTaskCfg.GetParamList();
                        int tarGetLevel = 0;
                        if (paramList.Count == 2)
                        {
                            target = paramList[0];
                            tarGetLevel = paramList[1];
                        }
                        else
                        {
                            Debuger.LogError("成长任务配置错误：参数长度错误");
                        }
                        WeaponPart weaponPart = hero.WeaponPart;
                        for (int i = 0; i < (int)enEquipPos.maxWeapon - (int)enEquipPos.minWeapon + 1; ++i)
                        {
                            Weapon weapon = weaponPart.GetWeapon(i);
                            for (int j = 0; j < (int)enSkillPos.max; ++j)
                            {
                                WeaponSkill skill = weapon.GetSkill(j);
                                if (skill.lv >= tarGetLevel)
                                {
                                    current++;
                                }
                            }
                        }
                        break;
                    }
                case enTaskType.equipAdvLv:
                    {
                        List<int> paramList = growthTaskCfg.GetParamList();
                        int tarGetLevel = 0;
                        if (paramList.Count == 2)
                        {
                            target = paramList[0];
                            tarGetLevel = paramList[1];
                        }
                        else
                        {
                            Debuger.LogError("成长任务配置错误：参数长度错误");
                        }
                        EquipsPart equipsPart = hero.EquipsPart;
                        for (int i = 0; i < (int)enEquipPos.maxWeapon - (int)enEquipPos.minNormal + 1; ++i)
                        {
                            Equip equip = equipsPart.GetEquip((enEquipPos)i);
                            if (equip.AdvLv >= tarGetLevel)
                            {
                                current++;
                            }
                        }
                        break;
                    }
                case enTaskType.equipStar:
                    {
                        List<int> paramList = growthTaskCfg.GetParamList();
                        int tarGetLevel = 0;
                        if (paramList.Count == 2)
                        {
                            target = paramList[0];
                            tarGetLevel = paramList[1];
                        }
                        else
                        {
                            Debuger.LogError("成长任务配置错误：参数长度错误");
                        }
                        EquipsPart equipsPart = hero.EquipsPart;
                        for (int i = 0; i < (int)enEquipPos.maxWeapon - (int)enEquipPos.minNormal + 1; ++i)
                        {
                            Equip equip = equipsPart.GetEquip((enEquipPos)i);
                            int star = EquipCfg.m_cfgs[equip.EquipId].star;
                            if (star >= tarGetLevel)
                            {
                                current++;
                            }
                        }
                        break;
                    }
                case enTaskType.arena:
                    {
                        ActivityPart activityPart = hero.ActivityPart;
                        current = activityPart.GetInt(enActProp.arenaTotalWin);
                        target = int.Parse(growthTaskCfg.param);
                        break;
                    }
                case enTaskType.daily:
                    {
                        current = GetInt(enTaskProp.dailyTaskTotal);
                        target = int.Parse(growthTaskCfg.param);
                        break;
                    }
                case enTaskType.corps:
                    {
                        
                        int corpsId = hero.GetInt(enProp.corpsId);
                        current = corpsId > 0 ? 1 : 0;
                        target = int.Parse(growthTaskCfg.param);
                        break;
                    }
                case enTaskType.friend:
                    {
                        SocialPart socialPart = hero.SocialPart;
                        current = socialPart.friends.Count;
                        target = int.Parse(growthTaskCfg.param);
                        break;
                    }
                case enTaskType.power:
                    {
                        current = hero.GetInt(enProp.powerTotal);
                        target = int.Parse(growthTaskCfg.param);
                        break;
                    }
            }

            if (current >= target)
            {
                growthTask.taskState = enRewardState.canGetReward;
            }
            else
            {
                growthTask.progress = current.ToString() + "/" + target.ToString();
                growthTask.taskState = enRewardState.cantGetReward;
            }
        }
        return growthTask;
    }
    public void InitCheckTaskTip()
    {      
        CheckTip();
        m_parent.Add(MSG_ROLE.EQUIP_CHANGE, CheckTip);        
        m_parent.Add(MSG_ROLE.NET_ACT_PROP_SYNC, CheckTip);
        m_parent.Add(MSG_ROLE.NET_OPACT_PROP_SYNC, CheckTip);
        m_parent.Add(MSG_ROLE.WEAPON_SKILL_CHANGE, CheckTip);
        m_parent.Add(MSG_ROLE.ADD_FRIEND, CheckTip);
        m_parent.Add(MSG_ROLE.JOIN_CORPS, CheckTip);
        m_parent.Add(MSG_ROLE.CORPS_BUILD, CheckTip);
        m_parent.AddPropChange(enProp.powerTotal, CheckTip);
        m_parent.AddPropChange(enProp.vipLv, CheckTip);
        m_parent.AddPropChange(enProp.upEquipTime, CheckTip);
        m_parent.AddPropChange(enProp.towerEnterTime, CheckTip);

    }
    public void CheckTip()
    {
        if (CheckDailyTaskTip() || CheckGrowthTaskTip())
        {
            SetTaskTip(true);
        }
        else
        {
            SetTaskTip(false);
        }


    }

    public bool CheckDailyTaskTip()
    {
        bool isTip = false;
        Role hero = RoleMgr.instance.Hero;
        int currentLevel = hero.GetInt(enProp.level);
        int currentVipLv = hero.GetInt(enProp.vipLv);
        List<TaskRewardCfg> taskRewardCfg = TaskRewardCfg.GetCfgsByLevelAndVip(currentLevel,currentVipLv);
        for (int i = 0; i < taskRewardCfg.Count; ++i)
        {
            if (CanGetDailyTaskReward(taskRewardCfg[i]) == enRewardState.canGetReward)
            {
                isTip = true;
            }
        }
       
        List<VitalityCfg> vitalityCfg = VitalityCfg.GetListByLevel(currentLevel);


        for (int i = 0; i < vitalityCfg.Count; ++i)
        {
            long getBoxTime = 0;
            switch (vitalityCfg[i].boxNum)
            {
                case (1):
                    {
                        getBoxTime = GetLong(enTaskProp.vitalityBox1);
                        break;
                    }
                case (2):
                    {
                        getBoxTime = GetLong(enTaskProp.vitalityBox2);
                        break;
                    }
                case (3):
                    {
                        getBoxTime = GetLong(enTaskProp.vitalityBox3);
                        break;
                    }
                case (4):
                    {
                        getBoxTime = GetLong(enTaskProp.vitalityBox4);
                        break;
                    }
            }
            if (!TimeMgr.instance.IsToday(GetLong(enTaskProp.dailyTaskGet)))
            {
                m_vitality = 0;
            }
            if (m_vitality >= vitalityCfg[i].vitality && !TimeMgr.instance.IsToday(getBoxTime))
            {
                isTip = true;
            }
        }      
        return isTip;
    }
    public bool CheckGrowthTaskTip()
    {
        bool isTip = false;

        Dictionary<int, GrowthTaskCfg> growthTaskCfg = GrowthTaskCfg.m_cfgs;
        for (int i = 1; i < growthTaskCfg.Count + 1; ++i)
        {
            if (CanGetGrowthTaskReward(growthTaskCfg[i]).taskState == enRewardState.canGetReward)
            {
                isTip = true;
            }
        }      
        return isTip;
    }
    
    public int GetInt(enTaskProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Int;
        return 0;
    }

    public long GetLong(enTaskProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Long;
        return 0;
    }

    public float GetFloat(enTaskProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Float;
        return 0.0f;
    }

    public string GetString(enTaskProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.String;
        return "";
    }


}
