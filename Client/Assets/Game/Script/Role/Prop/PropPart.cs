#region Header
/**
 * 名称：属性部件
 
 * 日期：2015.9.21
 * 描述：记录着角色血量、等级、经验等等属性
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class PropPart:RolePart
{
    private static PropertyTable tempProps = new PropertyTable();

    #region Fields
    PropertyTable m_values = new PropertyTable();//内部值属性，进入战斗中就不会变化
    PropertyTable m_rates = new PropertyTable();//内部百分比属性，进入战斗中就不会变化
    
    PropertyTable m_aliveValues = new PropertyTable();//战斗值属性，战斗中不断变化
    PropertyTable m_aliveRates = new PropertyTable();//战斗百分比属性，战斗中不断变化

    PropertyTable m_props = new PropertyTable();//实时属性 = (m_values+m_aliveValues )(1+m_rates+m_aliveRates)
    Dictionary<string, int> m_flags = new Dictionary<string,int>();
    Dictionary<string, int> m_fightFlags = new Dictionary<string, int>();

    //在网络修改某个属性需要重新计算战斗力的属性
    HashSet<enProp> s_syncPropsNeedFresh = Util.ArrayToSet<enProp>(new enProp[] { enProp.level,  enProp.advLv, enProp.star });
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.prop; } }
    public Dictionary<string, int> FightFlags { get { return m_fightFlags; } }
    public PropertyTable Values { get { return m_values; } }//值属性，进入战斗中就不会变化
    public PropertyTable Rates { get { return m_rates; } }//百分比属性，进入战斗中就不会变化
    public PropertyTable AliveValues { get { return m_aliveValues; } }
    public PropertyTable AliveRates { get { return m_aliveRates; } }
    public PropertyTable Props { get { return m_props; } }
    public bool UnFreshBaseProp { get; set; }
    #endregion

    


    #region Frame    
    //初始化，不保证模型已经创建，每次角色从对象池取出来都会调用(可以理解为Awake)
    public override bool OnInit()
    {
        m_values.Clear();
        m_rates.Clear();
        m_aliveValues.Clear();
        m_aliveRates.Clear();
        m_props.Clear();
        m_flags.Clear();
        m_fightFlags.Clear();
        UnFreshBaseProp = false;
        return true;
    }

    //网络数据初始化
    public override void OnNetInit(FullRoleInfoVo vo)
    {
        foreach (var e in vo.props)
        {
            try
            {
                enProp prop = (enProp)Enum.Parse(typeof(enProp), e.Key);
                Property value = e.Value;
                m_props.SetValue(prop, value);
            }
            catch (Exception err)
            {
                Debuger.LogError("转换成enProp失败:{0}", err.Message);
            }
        }
        
    }
    
    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {
        m_aliveValues.Clear();
        m_aliveRates.Clear();
        m_fightFlags.Clear();

        //刷新属性
        FreshBaseProp(false);

        //设置标记
        List<string> flags = m_parent.Cfg.flags;
        for(int i=0;i<flags.Count;++i){
            SetFlag(flags[i]);
        }
        
        //重设血量、耐力等有最大值的变化属性
        for (int i = (int)enProp.minFightProp + 1; i < (int)enProp.maxFightProp; ++i)
        {
            enProp propChange = PropTypeCfg.GetPropByMax((enProp)i);
            if(propChange == enProp.minFightProp)
                continue;
            m_props.SetInt(propChange, (int)GetFloat((enProp)i));//这里不调用自己的SetInt，因为不需要发送变化的消息      
        }
    }

    public override void OnClear()
    {
        
    }
    #endregion


    #region Private Methods
    
    #endregion
    
    public void SyncProps(RoleSyncPropVo vo)
    {
        if (vo.props.Count <= 0)
            return;
        bool needFresh = false;
        List<int> temp = new List<int>();
        HeroUpgradeInfo upgradeInfo = null;
        if(Parent.IsHero && vo.props.ContainsKey("level"))
        {
            upgradeInfo = new HeroUpgradeInfo();
            // 如果体力没有改变
            upgradeInfo.oldStamina = GetStamina();
            upgradeInfo.newStamina = upgradeInfo.oldStamina;
            UIPowerUp.SaveOldProp(Parent);
        }
        //先填写完整
        foreach (var e in vo.props)
        {
            try
            {
                enProp prop = (enProp)Enum.Parse(typeof(enProp), e.Key);
                if (upgradeInfo!=null && prop == enProp.level)
                {
                    upgradeInfo.oldLevel = GetInt(prop);
                }
                m_props.SetValue(prop, e.Value);
                temp.Add((int)prop);
                if(!needFresh &&s_syncPropsNeedFresh.Contains(prop))
                    needFresh = true;
            }
            catch (Exception err)
            {
                Debuger.LogError("转换成enProp失败", err.Message);
            }
        }

        if (upgradeInfo != null)
        {
            upgradeInfo.newLevel = GetInt(enProp.level);
            upgradeInfo.newStamina = GetStamina();
            UIHeroUpgrade.SetUpgradeInfo(upgradeInfo);
        }

        //刷新计算属性
        if (needFresh)
            FreshBaseProp();

        //让上层的部件可以进行一些判断
        RolePart part;
        for (int i = 0; i < (int)enPart.max; ++i)
        {
            part = m_parent.GetPart((enPart)i);
            if (part != null)
                part.OnSyncProps(temp);
        }

        //再触发事件
        //特定属性的事件
        foreach (var propId in temp)
        {
            m_parent.Fire(MSG_ROLE.PROP_CHANGE + propId);
            if (propId == (int)enProp.power && !Parent.IsHero && m_parent.PetsPart.Owner.PetsPart.IsMainPet(GetString(enProp.guid)))
            {
                m_parent.PetsPart.Owner.Fire(MSG_ROLE.PROP_CHANGE + propId);
            }
        }
        //不特定属性的事件
        m_parent.Fire(MSG_ROLE.NET_PROP_SYNC, 0);
    }

    public int GetInt(enProp prop) { return m_props.GetInt(prop); }

    public void SetInt(enProp prop, int v) { m_props.SetInt(prop, v); m_parent.Fire(MSG_ROLE.PROP_CHANGE + (int)prop); }

    public void AddInt(enProp prop, int v) { m_props.AddInt(prop, v, true); m_parent.Fire(MSG_ROLE.PROP_CHANGE + (int)prop); }

    public long GetLong(enProp prop) { return m_props.GetLong(prop); }

    public void SetLong(enProp prop, long v) { m_props.SetLong(prop, v); m_parent.Fire(MSG_ROLE.PROP_CHANGE + (int)prop); }

    public void AddLong(enProp prop, long v) { m_props.AddLong(prop, v, true); m_parent.Fire(MSG_ROLE.PROP_CHANGE + (int)prop); }

    public float GetFloat(enProp prop) { return m_props.GetFloat(prop); }

    public void SetFloat(enProp prop, float v) { m_props.SetFloat(prop, v); m_parent.Fire(MSG_ROLE.PROP_CHANGE + (int)prop); }

    public void AddFloat(enProp prop, float v) { m_props.AddFloat(prop, v, true); m_parent.Fire(MSG_ROLE.PROP_CHANGE + (int)prop); }

    public string GetString(enProp prop) { return m_props.GetString(prop); }

    public void SetString(enProp prop, string v) { m_props.SetString(prop, v); m_parent.Fire(MSG_ROLE.PROP_CHANGE + (int)prop, prop); }

    //fightTemp表明是不是关卡中的临时属性，对于怪物是没有区别的，但是对于主角和宠物如果希望任何时候有持有这个属性的话填false
    public void SetFlag(string flag,int n=1,bool levelTemp=true)
    {
        if (levelTemp)
            m_fightFlags[flag] = n;
        else
            m_flags[flag] = n;

        m_parent.Fire(MSG_ROLE.FLAG_CHANGE, flag);
    }

    //fightTemp表明是不是关卡中的临时属性，对于怪物是没有区别的，但是对于主角和宠物如果希望任何时候有持有这个属性的话填false
    public void AddFlag(string flag, int n = 1, bool levelTemp = true)
    {
        int i;
        if (levelTemp)
        {
            if (!m_fightFlags.TryGetValue(flag, out i))
                m_fightFlags[flag] = n;
            else
                m_fightFlags[flag] = i + n;
        }
        else
        {
            if (!m_flags.TryGetValue(flag, out i))
                m_flags[flag] = n;
            else
                m_flags[flag] = i + n;
        }
        m_parent.Fire(MSG_ROLE.FLAG_CHANGE, flag);
    }

    public int GetFlag(string flag)
    {
        int i;
        if (m_fightFlags.TryGetValue(flag, out i))
            return i;
        if(m_flags.TryGetValue(flag,out i))
            return i;            
        else
            return 0;
    }

    public bool HasFlag(string flag)
    {
        return m_fightFlags.ContainsKey(flag) || m_flags.ContainsKey(flag);
    }
    public string LogFlags()
    {
        string log = string.Format("{0}个标记\n", m_fightFlags.Count+ m_flags.Count);
        foreach (var pair in m_fightFlags)
            log += string.Format("{0} {1}\n", pair.Key.PadRight(12), pair.Value);
        foreach (var pair in m_flags)
            log += string.Format("{0} {1}\n", pair.Key.PadRight(12), pair.Value);
        return log;
    }

    public int GetStamina()
    {
        var staminaNum = GetInt(enProp.stamina);
        var staminaTime = GetInt(enProp.staminaTime);
        staminaTime = Mathf.Max(staminaTime, 0);
        var recoverTIme = ConfigValue.GetInt("StaminaRecoverTime");
        var recoverMax = RoleLvExpCfg.Get(GetInt(enProp.level)).maxStamina;
        var num = RecoverUtil.GetNum(staminaTime, staminaNum, recoverTIme, recoverMax);
        if (num != staminaNum)
            return Mathf.Max(0,num);
        else
            return Mathf.Max(0, staminaNum);
    }
    
    //刷新战斗属性，第一次调用在Init(InitNet)和PostInit之间
    //注意这里修改的是基础属性，战斗中不可修改
    public void FreshBaseProp(bool checkError=true)
    {
        if (UnFreshBaseProp)
            return;

        //检错下
        if (checkError&&(Parent.State == Role.enState.alive || Parent.IsLoading) && LevelMgr.instance.CurLevel.State != LevelState.End)
        {
            Debuger.LogError("战斗时候不能刷新基础属性");
        }
    #if PROP_DEBUG
        Debuger.Log("开始属性计算:{0}",m_parent.Cfg.id);
        string log = "结束属性计算\n";
        #endif
        //值属性
        m_parent.Cfg.GetBaseProp(m_values, GetInt(enProp.level), GetInt(enProp.advLv), GetInt(enProp.star));//自身成长属性

        //百分比属性
        PropertyTable.Set(0,  m_rates);//暂时没有需要用的，这里置0
        
        //如果是怪物要加关卡修正
        if (m_parent.Cfg.RolePropType == enRolePropType.monster)
        {
            RoomCfg roomCfg = Room.instance.roomCfg;
            if (!string.IsNullOrEmpty(roomCfg.levelPropRate))
            {
                PropertyTable.Add(1f, PropRateCfg.Get(roomCfg.levelPropRate).props, tempProps);
                PropertyTable.Mul(tempProps, m_values, m_values);
            }
            
            if (!string.IsNullOrEmpty(roomCfg.levelProp))
                PropertyTable.Add(m_values, PropValueCfg.Get(roomCfg.levelProp).props, m_values);
            
            #if PROP_DEBUG
            log+=string.Format("加关卡修正相关={0}\n", m_values.GetFloat(enProp.hpMax));
            #endif
        }
        

        //让上层的部件可以叠加
        RolePart part;
        for (int i = 0; i < (int)enPart.max; ++i)
        {
             part = m_parent.GetPart((enPart)i);
             if (part!= null)
                 part.OnFreshBaseProp(m_values, m_rates);
        }

#if PROP_DEBUG
        log+=string.Format("加上层部件={0}\n", m_values.GetFloat(enProp.hpMax));
#endif
        //让外部可以叠加
        m_parent.Fire(MSG_ROLE.FRESH_BASE_PROP, m_values, m_rates);

#if PROP_DEBUG
        log+=string.Format("加外部={0}\n", m_values.GetFloat(enProp.hpMax));
#endif
        

        //如果是有等级值的关卡的话，主角和宠物
        if (m_parent.Cfg.RolePropType != enRolePropType.monster)
		{
			var level = LevelMgr.instance.CurLevel;
			if(level!=null&&level.roomCfg.hpRate!=null)
			{
				float rate = level.roomCfg.hpRate.GetByLv(level.GetHpRateLv());
				m_values.SetFloat(enProp.hpMax, m_values.GetFloat(enProp.hpMax) * (1 + rate));
			}
		}
        
        //角色的最终属性=(基础属性值+状态固定值...)*(1+状态百分比...)
        for (int i = (int)enProp.minFightProp+1; i < (int)enProp.maxFightProp; ++i)
        {
            FreshProp((enProp)i,true);
        }

        /*
        FreshProp(enProp.power, true);

        //战斗力结果取整
        SetFloat(enProp.power, Mathf.Floor(GetFloat(enProp.power)));
         */
        #if PROP_DEBUG
        log += string.Format("最终值={0}\n", m_props.GetFloat(enProp.hpMax));
        Debuger.Log(log);
#endif
    }

    public void FreshProp(enProp prop,bool resetMax = false)
    {
        //角色的最终属性=(基础属性值+状态固定值...)*(1+状态百分比...)
        float v = (m_values.GetFloat(prop) + m_aliveValues.GetFloat(prop)) * (1f +m_rates.GetFloat(prop)+ m_aliveRates.GetFloat(prop));
        m_props.SetFloat(prop, v);

        //会变化的值(血量、耐力、气力值等)不能超过最大值，要重新赋值
        enProp propChange = PropTypeCfg.GetPropByMax(prop);
        if (propChange != enProp.minFightProp)
        {
            if (resetMax || m_props.GetInt(propChange) <= (int)v)
            {
                m_props.SetInt(propChange, (int)v);
            }
        }
    }

    //状态固定值，角色的最终属性=(基础属性值+状态固定值...)*(1+状态百分比...)
    public void AddBuffValue(enProp prop,float add)
    {
        if(PropTypeCfg.Get(prop).format == enPropFormat.FloatRate)
            add/=10000;
        m_aliveValues.AddFloat(prop,add,true);
    }

    //状态百分比，角色的最终属性=(基础属性值+状态固定值...)*(1+状态百分比...)
    public void AddBuffRate(enProp prop, float add)
    {
        m_aliveRates.AddFloat(prop, add, true);
    }

    //修改属性
    public void SetBaseProps(PropertyTable value,PropertyTable rate=null)
    {
        PropertyTable.Copy(value, m_values);

        if(rate == null)
            PropertyTable.Mul(0, m_rates, m_rates);//暂时没有需要用的，这里置0
        else
            PropertyTable.Copy(rate, m_rates);

        //角色的最终属性=(基础属性值+状态固定值...)*(1+状态百分比...)
        for (int i = (int)enProp.minFightProp + 1; i < (int)enProp.maxFightProp; ++i)
        {
            FreshProp((enProp)i, true);
        }
    }
}
