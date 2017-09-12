using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class OpActivityPart : RolePart
{
    #region Fields
    private Dictionary<enOpActProp, Property> m_props = new Dictionary<enOpActProp, Property>();
    #endregion


    #region Properties
    public override enPart Type { get { return enPart.opActivity; } }
    public Dictionary<enOpActProp, Property> Props { get { return m_props; } }
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
        if (vo.opActProps == null)
            return;

        foreach (var e in vo.opActProps)
        {
            try
            {
                enOpActProp prop = (enOpActProp)Enum.Parse(typeof(enOpActProp), e.Key);
                m_props[prop] = e.Value;
            }
            catch (Exception err)
            {
                Debuger.LogError("转换成enOpActProp失败", err.Message);
            }
        }

        //UIMainCity.AddOpen(CheckTip);
    }

    //后置初始化，模型已经创建，每个模块都初始化过一次，每次角色从对象池取出来都会调用(可以理解为Start())
    public override void OnPostInit()
    {


    }



    public override void OnClear()
    {

    }

    public void OnSyncProps(SyncOpActivityPropVo vo)
    {
        if (vo.props.Count <= 0)
            return;

        List<int> temp = new List<int>();

        //先填写完整
        foreach (var e in vo.props)
        {
            try
            {
                enOpActProp prop = (enOpActProp)Enum.Parse(typeof(enOpActProp), e.Key);
                m_props[prop] = e.Value;
                temp.Add((int)prop);
            }
            catch (Exception err)
            {
                Debuger.LogError("转换成enOpActProp失败", err.Message);
            }
        }

        Role hero = RoleMgr.instance.Hero;
        //Debuger.Log("当前累计充值:" + GetInt( enOpActProp.totalRecharge) + "      当前vip等级:" + hero.GetInt(enProp.vipLv));  
        
        //不特定属性的事件
        m_parent.Fire(MSG_ROLE.NET_OPACT_PROP_SYNC, 0);
    }
    #endregion


    #region Private Methods

    void SetOpActivityTip(bool tip)
    {
        SystemMgr.instance.SetTip(enSystem.opActivity, tip);
    }

    bool IsCheckIn()
    {
        if (!TimeMgr.instance.IsToday(GetLong(enOpActProp.lastCheckIn)))
        {            
            return false;
        }
        else
        {            
            return true;
        }
    }

    public enRewardState CanGetLevelReward(int id)
    {
        LevelRewardCfg levelRewardCfg = LevelRewardCfg.m_cfgs[id];
        int level = levelRewardCfg.level;
        string levelProp = "lv" + level.ToString();
        enOpActProp prop = (enOpActProp)Enum.Parse(typeof(enOpActProp), levelProp); 
        Role hero = RoleMgr.instance.Hero;
        int currentLevel = hero.GetInt(enProp.level);

        if(currentLevel>=level)
        {
            if(GetLong(prop)==0)
            {
                return enRewardState.canGetReward;
            }
            else
            {
                return enRewardState.hasGetReward;
            }
        }
        else
        {
            return enRewardState.cantGetReward;
        }
    }

    public bool CanGetLevelReward()
    {
        Dictionary<int, LevelRewardCfg> levelRewardCfg = LevelRewardCfg.m_cfgs;
        for(int i=1;i<levelRewardCfg.Count+1;++i)
        {
            if(CanGetLevelReward(i)==enRewardState.canGetReward)
            {
                return true;
            }
        }
        return false;
    }

    public enRewardState CanGetVipGift(int vipLv)
    {
        string vipGiftProp = string.Format("vip{0}Gift", vipLv);
        enOpActProp prop = (enOpActProp)Enum.Parse(typeof(enOpActProp), vipGiftProp);
        long vipGiftGetTime = GetLong(prop);
        Role hero = RoleMgr.instance.Hero;
        int currentVipLv = hero.GetInt(enProp.vipLv);
        if(currentVipLv>=vipLv)
        {
            if(vipGiftGetTime!=0)
            {
                return enRewardState.hasGetReward;
            }
            else
            {
                return enRewardState.canGetReward;
            }
        }
        else
        {
            return enRewardState.cantGetReward;
        }

    }

    public int CheckLevelReward()
    {
        Dictionary<int, LevelRewardCfg> levelRewardCfg = LevelRewardCfg.m_cfgs;
        for (int i = 1; i < levelRewardCfg.Count + 1; ++i)
        {
            if (CanGetLevelReward(i) == enRewardState.canGetReward)
            {
                return i;
            }           
        }
        for (int i = 1; i < levelRewardCfg.Count + 1; ++i)
        {
            if (CanGetLevelReward(i) == enRewardState.cantGetReward)
            {
                return i;
            }
        }
        return 0;
    }



    #endregion
    public void CheckTip()
    {
        if (!IsCheckIn() || CanGetLevelReward())
        {
            SetOpActivityTip(true);
        }
        else
        {
            SetOpActivityTip(false);
        }
    }

    public void InitCheckOpActivityTip()
    {
        CheckTip();
        m_parent.AddPropChange(enProp.level, CheckTip);
    }
    public int GetInt(enOpActProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Int;
        return 0;
    }

    public long GetLong(enOpActProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Long;
        return 0;
    }

    public float GetFloat(enOpActProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.Float;
        return 0.0f;
    }

    public string GetString(enOpActProp prop)
    {
        Property p;
        if (m_props.TryGetValue(prop, out p))
            return p.String;
        return "";
    }

}
