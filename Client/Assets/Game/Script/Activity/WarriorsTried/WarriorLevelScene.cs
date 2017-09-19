using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WarriorLevelScene: LevelBase
{
    //public class RateInfo
    //{
    //    public int rate;
    //    public int num;
    //    public RateInfo(int rate, int num)
    //    {
    //        this.rate = rate;
    //        this.num = num;
    //    }
    //}

    public static int m_oldLevel = 0;
    public static int m_oldExp = 0;

    static int lastSecond = 20;

    public float startTime;
    private UILevelAreaReward m_uiReward;
    
    protected bool bShowLimitTime = false;
    
    #region Frame
    //场景切换完成
    public override void OnLoadFinish()
    {
        //打开摇杆和魂值、金币、物品栏
        UILevel uiLevel = UIMgr.instance.Open<UILevel>();

        m_uiReward = uiLevel.Open<UILevelAreaReward>();
        m_uiReward.ResetUI();
        
        uiLevel.Open<UILevelAreaGizmos>();

        if (Room.instance.roomCfg.time > 0)     //大于0 倒计时
        {
            var area = uiLevel.Open<UILevelAreaTime>();
            area.SetTime(Room.instance.roomCfg.time);
        }
        else if (Room.instance.roomCfg.time == 0)   //等于0 正计时
        {
            uiLevel.Open<UILevelAreaTime>();
        }
        
        startTime = TimeMgr.instance.logicTime;
        bShowLimitTime = false;
    }

    //public override void OnEnterAgain()
    //{
    //    //打开摇杆和魂值、金币、物品栏
    //    UILevel uiLevel = UIMgr.instance.Open<UILevel>();

    //    m_uiReward = uiLevel.Open<UILevelAreaReward>();

    //    //打开通关条件
    //    uiLevel.Open<UILevelAreaGizmos>();

    //    if (Room.instance.roomCfg.time > 0)     //大于0 倒计时
    //    {
    //        var area = uiLevel.Open<UILevelAreaTime>();
    //        area.SetTime(Room.instance.roomCfg.time);
    //    }
    //    else if (Room.instance.roomCfg.time == 0)   //等于0 正计时
    //    {
    //        uiLevel.Open<UILevelAreaTime>();
    //    }
    //}

    public override void SendResult(bool isWin)
    {
       // Debug.Log(string.Format("通关时间 : {0}", TimeMgr.instance.logicTime - startTime));
        //单机直接回城
        if (Main.instance.isSingle)
        {
            LevelMgr.instance.GotoMaincity();
            return;
        }
        
        NetMgr.instance.ActivityHandler.SendEndWarrior(RoleMgr.instance.Hero.ActivityPart.warrIndex-1, isWin);
        bShowLimitTime = false;
    }

    //角色死亡 isNow:是否立即销毁 包括怪、宠物
    public override void OnRoleDead(Role role, bool isNow)
    {
        //勇士试炼不用
        //if (role.IsHero)
        //    return;

     //   GiveReward(role);
        
    }

    public override void OnLeave()
    {
        Dictionary<int, Role> newDict = new Dictionary<int, Role>();
        Role hero = RoleMgr.instance.Hero;
        if (hero != null && hero.State == Role.enState.alive)
        {
            newDict.Add(hero.Id, hero);
        }
        mRoleDic.Clear();
        mRoleDic = newDict;
    }

    public override void OnUpdate()
    {
        if (State == LevelState.End)
            return;

        base.OnUpdate();
        RoomCfg cfg = Room.instance.roomCfg;
        if (cfg == null)
            return;

        if (cfg.limitTime <= 0)
            return;

        if (!bShowLimitTime && cfg.limitTime - lastSecond <= TimeMgr.instance.logicTime - startTime)
        {
            UILevel uiLevel = UIMgr.instance.Get<UILevel>();
            if (uiLevel.IsOpen)
            {
                var area = uiLevel.Open<UILevelAreaTime>();
                area.SetTime(lastSecond);
            }

            bShowLimitTime = true;
        }

        if (cfg.limitTime <= TimeMgr.instance.logicTime - startTime)
        {
            LevelMgr.instance.SetLose();
        }
    }

    public override void OnRoleEnter(Role role)
    {
      
    }

    public override void OnHeroEnter(Role hero)
    {
        if (hero != null)
        {
            m_oldLevel = hero.GetInt(enProp.level);
            m_oldExp = hero.GetInt(enProp.exp);

            hero.AIPart.RePlay();
        }
    }

    #endregion
    
    
    public void OnWin(EndWarriorRes vo)
    {
        Room.instance.StartCoroutine(CoWin(vo));
    }

    IEnumerator CoWin(EndWarriorRes vo)
    {
        yield return new WaitForSeconds(1);

        //隐藏宠物
        Role hero = RoleMgr.instance.Hero;
        if (hero != null)
        {
            hero.RoleModel.Foot.gameObject.SetActive(false);
        }

        //打开界面
        //   yield return Room.instance.StartCoroutine(UIMgr.instance.Get<UILevelEnd>().onLevelEnd(vo));

        UILevelEnd2Context cxt = new UILevelEnd2Context();
        cxt.moveCamera = false;
        //cxt.rate = LevelEvaluateCfg.m_cfgs[vo.evaluation].name;
        //cxt.desc.Add(new KeyValuePair<string, string>("进度：", vo.percentage.ToString() + "%"));
        //cxt.items.Add(new KeyValuePair<int, int>(ITEM_ID.REDSOUL, vo.soul));
        UIMgr.instance.Get<UILevelEnd2>().OnLevelEnd(cxt);
    }

    public void OnLose()
    {
        Room.instance.StartCoroutine(CoLose());
    }

  

    IEnumerator CoLose()
    {
        UIMgr.instance.Open<UILevelFail>();
        yield return 0;
    }


}
