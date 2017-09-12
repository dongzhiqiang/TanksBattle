using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//关卡场景
public class LevelScene : LevelBase
{
    public class RateInfo
    {
        public int rate;
        public int num;
        public RateInfo(int rate, int num)
        {
            this.rate = rate;
            this.num = num;
        }
    }

    public static int m_oldLevel = 0;
    public static int m_oldExp = 0;

    static int lastSecond = 20;

    public float startTime;
    private UILevelAreaReward m_uiReward;

    RateInfo m_monsterRate;
    RateInfo m_specialRate;
    RateInfo m_bossRate;
    RateInfo m_boxRate;

    protected List<ItemVo> m_monsterItems = new List<ItemVo>();
    protected List<ItemVo> m_specialItems = new List<ItemVo>();
    protected List<ItemVo> m_bossItems = new List<ItemVo>();
    protected List<ItemVo> m_boxItems = new List<ItemVo>();

    LevelsPart levelPart;

    List<string> showWaveGroupIdList;

    string curWaveGroupFlag = "";
    //int curWaveNum;
    int maxWaveNum;
    int prevGroupWaveNum = 0;
    protected bool bShowLimitTime = false;

    //UILevelAreaWave uiAreaWave;

    #region Frame
    //场景切换完成
    public override void OnLoadFinish()
    {
        //打开摇杆和魂值、金币、物品栏
        UILevel uiLevel = UIMgr.instance.Open<UILevel>();

        m_uiReward = uiLevel.Open<UILevelAreaReward>();
        m_uiReward.ResetUI();

        //打开通关条件
        //uiLevel.Open<UILevelAreaCondition>();
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

        //读取掉落配置
        RoomCfg cfg = Room.instance.roomCfg;
        if (!string.IsNullOrEmpty(cfg.monsterRandom))
        {
            string[] info = cfg.monsterRandom.Split('|');
            m_monsterRate = new RateInfo(int.Parse(info[0]), int.Parse(info[1]));
        }
        if (!string.IsNullOrEmpty(cfg.specialRandom))
        {
            string[] info = cfg.specialRandom.Split('|');
            m_specialRate = new RateInfo(int.Parse(info[0]), int.Parse(info[1]));
        }
        if (!string.IsNullOrEmpty(cfg.bossRandom))
        {
            string[] info = cfg.bossRandom.Split('|');
            m_bossRate = new RateInfo(int.Parse(info[0]), int.Parse(info[1]));
        }

        if (!string.IsNullOrEmpty(cfg.boxRandom))
        {
            string[] info = cfg.boxRandom.Split('|');
            m_boxRate = new RateInfo(int.Parse(info[0]), int.Parse(info[1]));
        }
        //SceneCfg.SceneData sceneData = SceneMgr.instance.SceneData;
        //curWaveGroupFlag = sceneData.mShowWaveGroupId;
        //if (!string.IsNullOrEmpty(curWaveGroupFlag))
        //{
        //    showWaveGroupIdList = sceneData.mGroupIdList;
        //}

        //maxWaveNum = 0;
        //if (showWaveGroupIdList != null)
        //{
        //    for (int i = 0; i < sceneData.mRefGroupList.Count; i++)
        //    {
        //        if (showWaveGroupIdList.Contains(sceneData.mRefGroupList[i].groupFlag))
        //        {
        //            maxWaveNum += sceneData.mRefGroupList[i].refreshNum;
        //        }
        //    }
        //}

        //uiAreaWave = UIMgr.instance.Get<UILevel>().Get<UILevelAreaWave>();

        m_monsterItems.Clear();
        m_specialItems.Clear();
        m_bossItems.Clear();
        m_boxItems.Clear();

        startTime = TimeMgr.instance.logicTime;
        bShowLimitTime = false;
    }

    public override void OnEnterAgain()
    {

        //打开摇杆和魂值、金币、物品栏
        UILevel uiLevel = UIMgr.instance.Open<UILevel>();

        m_uiReward = uiLevel.Open<UILevelAreaReward>();

        //打开通关条件
        //uiLevel.Open<UILevelAreaCondition>();
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
    }

    public override void SendResult(bool isWin)
    {
        Debug.Log(string.Format("通关时间 : {0}", TimeMgr.instance.logicTime - startTime));
        //单机直接回城
        if (Main.instance.isSingle)
        {
            LevelMgr.instance.GotoMaincity();
            return;
        }

        LevelEndReqVo request = new LevelEndReqVo();
        request.isWin = isWin;
        request.roomId = Room.instance.roomCfg.id;
        request.time = (int)(TimeMgr.instance.logicTime - startTime);      //通关时间
        request.starsInfo = new Dictionary<string, int>();
        List<SceneTrigger> triList = SceneEventMgr.instance.conditionTriggerList;
        for (int i = 0; i < triList.Count; i++)
        {
            RoomConditionCfg cfg = triList[i].GetConditionCfg();
            request.starsInfo.Add(cfg.id + "", triList[i].bReach() ? 1 : 0);
        }

        request.monsterItems = m_monsterItems;
        request.specialItems = m_specialItems;
        request.bossItems = m_bossItems;
        request.boxItems = m_boxItems;

        NetMgr.instance.LevelHandler.SendEnd(request);
        bShowLimitTime = false;
    }

    //角色死亡 isNow:是否立即销毁
    public override void OnRoleDead(Role role, bool isNow)
    {
        if (role.IsHero)
            return;

        GiveReward(role);

        ////立即销毁的 直接飞魂值
        //if (isNow)
        //{
        //}
    }
    ////角色死亡状态结束  //有些怪是直接爆开 没有死亡状态
    //public override void OnRoleDeadEnd(Role role)
    //{
    //    if (!role.IsHero)
    //    {
    //        GiveReward(role);
    //    }
    //}

    public override void OnLeave()
    {
        Dictionary<int, Role> newDict = new Dictionary<int, Role>();
        Role hero = RoleMgr.instance.Hero;
        if (hero != null && hero.State == Role.enState.alive)
        {
            newDict.Add(hero.Id, hero);
            List<Role> pets = hero.PetsPart.GetMainPets();
            foreach (Role p in pets)
            {
                if (p != null && p.State != Role.enState.alive)
                    newDict.Add(p.Id, p);
            }
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
        //if (role == null || showWaveGroupIdList == null)
        //    return;

        //for(int i = 0; i < showWaveGroupIdList.Count; i++)
        //{
        //    if (role.GetFlag(showWaveGroupIdList[i]) > 0)
        //    {
        //        //开始刷新出需要记录波数的刷新组时 打开显示波数的界面
        //        if (showWaveGroupIdList[i] == SceneMgr.instance.SceneData.mShowWaveGroupId)
        //        {
        //            if (!uiAreaWave .IsOpen)
        //                uiAreaWave.OpenArea(null);
        //        }
        //        //下一个刷新组刷怪了
        //        if (curWaveGroupFlag != showWaveGroupIdList[i])
        //        {
        //            curWaveGroupFlag = showWaveGroupIdList[i];
        //            prevGroupWaveNum = GetCurWaveNum();         //记录下之前所有刷新组刷的波数
        //        }
        //        RefreshBase refGroup = SceneMgr.instance.GetRefreshNpcByFlag(showWaveGroupIdList[i]);
        //        curWaveNum = refGroup.waveNum;

        //        SetWaveNum();

        //        return;
        //    }
        //}
    }

    public override void OnHeroEnter(Role hero)
    {
        if (hero != null)
        {
            levelPart = hero.GetPart(enPart.levels) as LevelsPart;
            m_oldLevel = hero.GetInt(enProp.level);
            m_oldExp = hero.GetInt(enProp.exp);

            hero.AIPart.RePlay();
        }
    }

    #endregion

    void OnFlyEnd()
    {

    }

    //public int GetCurWaveNum()
    //{
    //    return prevGroupWaveNum + curWaveNum;
    //}

    //public void SetWaveNum()
    //{
    //    if (uiAreaWave.IsOpen)
    //    {
    //        uiAreaWave.SetWave(GetCurWaveNum(), maxWaveNum);
    //    }
    //}

    public void OnWin(LevelEndResVo vo)
    {
        Room.instance.StartCoroutine(CoWin(vo));
    }

    IEnumerator CoWin(LevelEndResVo vo)
    {
        yield return new WaitForSeconds(1);

        //隐藏宠物
        Role hero = RoleMgr.instance.Hero;
        if (hero != null)
        {
            List<Role> pets = hero.PetsPart.GetMainPets();
            foreach (Role pet in pets)
            {
                if (pet != null && pet.State == Role.enState.alive)
                    pet.RoleModel.Show(false);
            }

            hero.RoleModel.Foot.gameObject.SetActive(false);
        }

        //打开界面
        yield return Room.instance.StartCoroutine(UIMgr.instance.Get<UILevelEnd>().onLevelEnd(vo));
    }

    public void OnLose()
    {
        Room.instance.StartCoroutine(CoLose());
    }

    public Dictionary<int, ItemVo> GetAllDropRewards()
    {
        Dictionary<int, ItemVo> items = new Dictionary<int, ItemVo>();
        foreach (ItemVo item in m_specialItems)
        {
            ItemVo it;
            if (!items.TryGetValue(item.itemId, out it))
            {
                it = new ItemVo();
                it.itemId = item.itemId;
                it.num = item.num;
                items.Add(it.itemId, it);
            }
            else
            {
                it.num += item.num;
            }
        }

        foreach (ItemVo item in m_monsterItems)
        {
            ItemVo it;
            if (!items.TryGetValue(item.itemId, out it))
            {
                it = new ItemVo();
                it.itemId = item.itemId;
                it.num = item.num;
                items.Add(it.itemId, it);
            }
            else
            {
                it.num += item.num;
            }
        }

        foreach (ItemVo item in m_bossItems)
        {
            ItemVo it;
            if (!items.TryGetValue(item.itemId, out it))
            {
                it = new ItemVo();
                it.itemId = item.itemId;
                it.num = item.num;
                items.Add(it.itemId, it);
            }
            else
            {
                it.num += item.num;
            }
        }

        foreach (ItemVo item in m_boxItems)
        {
            ItemVo it;
            if (!items.TryGetValue(item.itemId, out it))
            {
                it = new ItemVo();
                it.itemId = item.itemId;
                it.num = item.num;
                items.Add(it.itemId, it);
            }
            else
            {
                it.num += item.num;
            }
        }
        return items;

    }

    IEnumerator CoLose()
    {
        //yield return new WaitForSeconds(2f);
        UIMgr.instance.Open<UILevelFail>();
        yield return 0;
    }

    public void GiveReward(Role role)
    {
        if (State == LevelState.End)
            return;

        if (Main.instance.isSingle || Room.instance.roomCfg.id == "1000")
            return;

        List<ItemVo> items = new List<ItemVo>();

        if (role.Cfg.roleType == enRoleType.monster)
        {

            for (int i = 0; i < m_monsterRate.num; i++)
            {
                //掉没了就不掉了
                if (levelPart.DropMonsterItems.Count <= 0)
                    break;

                int rate = Random.Range(0, 10000);


                if (rate < m_monsterRate.rate)
                {
                    int idx = Random.Range(0, levelPart.DropMonsterItems.Count - 1);
                    ItemVo item = levelPart.DropMonsterItems[idx];
                    m_monsterItems.Add(item);
                    items.Add(item);
                    levelPart.DropMonsterItems.RemoveAt(idx);
#if DEBUG_DROPITEM
                    Debuger.Log("小怪掉落 ： {0} - {1}", item.itemId, item.num);
#endif
                }
            }
        }
        else if (role.Cfg.roleType == enRoleType.special)
        {
            for (int i = 0; i < m_specialRate.num; i++)
            {
                //掉没了就不掉了
                if (levelPart.DropSpecialItems.Count <= 0)
                    break;

                int rate = Random.Range(0, 10000);

                if (rate < m_specialRate.rate)
                {
                    int idx = Random.Range(0, levelPart.DropSpecialItems.Count - 1);
                    ItemVo item = levelPart.DropSpecialItems[idx];
                    m_specialItems.Add(item);
                    items.Add(item);
                    levelPart.DropSpecialItems.RemoveAt(idx);
#if DEBUG_DROPITEM
                    Debuger.Log("精英掉落 ： {0} - {1}", item.itemId, item.num);
#endif
                }
            }
        }
        else if (role.Cfg.roleType == enRoleType.boss)
        {
            for (int i = 0; i < m_bossRate.num; i++)
            {
                //掉没了就不掉了
                if (levelPart.DropBossItems.Count <= 0)
                    break;

                int rate = Random.Range(0, 10000);

                if (rate < m_bossRate.rate)
                {
                    int idx = Random.Range(0, levelPart.DropBossItems.Count - 1);
                    ItemVo item = levelPart.DropBossItems[idx];
                    m_bossItems.Add(item);
                    items.Add(item);
                    levelPart.DropBossItems.RemoveAt(idx);
#if DEBUG_DROPITEM
                    Debuger.Log("boss掉落 ： {0} - {1}", item.itemId, item.num);
#endif
                }
            }
        }
        else if (role.Cfg.roleType == enRoleType.box)
        {
            //回血回蓝的箱子不掉奖励
            if (role.Cfg.addBuffType > 0)
                return;

            for (int i = 0; i < m_boxRate.num; i++)
            {
                //掉没了就不掉了
                if (levelPart.DropBoxItems.Count <= 0)
                    break;

                int rate = Random.Range(0, 10000);

                if (rate < m_boxRate.rate)
                {
                    int idx = Random.Range(0, levelPart.DropBoxItems.Count - 1);
                    ItemVo item = levelPart.DropBoxItems[idx];
                    m_boxItems.Add(item);
                    items.Add(item);
                    levelPart.DropBoxItems.RemoveAt(idx);
#if DEBUG_DROPITEM
                    Debuger.Log("boss掉落 ： {0} - {1}", item.itemId, item.num);
#endif
                }
            }
        }
        foreach (ItemVo item in items)
        {
            switch (item.itemId)
            {
                case ITEM_ID.GOLD:
                    m_uiReward.PlayGoldFly(role, item.num);
                    break;
                case ITEM_ID.REDSOUL:
                    m_uiReward.PlayRedSoulFly(role, item.num);
                    break;
                case ITEM_ID.EXP:
                    break;
                default:
                    m_uiReward.PlayItemFly(role, item.num);
                    break;
            }
        }
    }
}
