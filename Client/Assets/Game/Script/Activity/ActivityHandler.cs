using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[NetModule(MODULE.MODULE_ACTIVITY)]
public class ActivityHandler
{
    [NetHandler(MODULE_ACTIVITY.PUSH_SYNC_PROP)]
    public void RoleSyncPropVo(SyncActivityPropVo info)
    {
        Role role = RoleMgr.instance.Hero;
        if (role == null)
            return;

        ActivityPart part = role.ActivityPart;
        part.OnSyncProps(info);
    }

    public void SendEnterGoldLevel(int mode)
    {
        EnterGoldLevelVo req = new EnterGoldLevelVo();
        req.mode = mode;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ENTER_GOLD_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_ENTER_GOLD_LEVEL)]
    public void OnEnterGoldLevel(EnterGoldLevelResultVo vo)
    {
        GoldLevelModeCfg modeCfg = GoldLevelModeCfg.Get(vo.mode);
        if (modeCfg == null)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_NONE, RESULT_CODE.CONFIG_ERROR));
            return;
        }

        LevelMgr.instance.ChangeLevel(modeCfg.roomId, modeCfg);
    }

    public void SendSweepGoldLevel(int mode, int hpMax)
    {
        SweepGoldLevelVo req = new SweepGoldLevelVo();
        req.mode = mode;
        req.hpMax = hpMax;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_SWEEP_GOLD_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_SWEEP_GOLD_LEVEL)]
    public void OnSweepGoldLevel(EndGoldLevelResultVo vo)
    {
        UILevelEnd2Context cxt = new UILevelEnd2Context();
        cxt.moveCamera = false;
        cxt.rate = vo.rate;
        cxt.desc.Add(new KeyValuePair<string, string>("通关评分：", vo.score.ToString()));
        cxt.desc.Add(new KeyValuePair<string, string>("雕像受损：", vo.damage + "%"));
        cxt.items.Add(new KeyValuePair<int, int>(ITEM_ID.GOLD, vo.gold));
        if (vo.rewards != null)
        {
            foreach (var e in vo.rewards)
            {
                cxt.items.Add(new KeyValuePair<int, int>(StringUtil.ToInt(e.Key), e.Value));
            }
        }
        UIMgr.instance.Get<UILevelEnd2>().OnLevelEnd(cxt);
    }

    public void SendEndGoldLevel(int hp, int hpMax)
    {
        EndGoldLevelVo req = new EndGoldLevelVo();
        req.hp = hp;
        req.hpMax = hpMax;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_END_GOLD_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_END_GOLD_LEVEL, true)]
    public void OnEndGoldLevel(int errorCode, string errorMsg, EndGoldLevelResultVo vo)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { LevelMgr.instance.GotoMaincity(); });
            Debuger.LogError(errorMsg);
            return;
        }

        UIMgr.instance.Close<UILevel>();

        UILevelEnd2Context cxt = new UILevelEnd2Context();
        cxt.moveCamera = true;
        cxt.rate = vo.rate;
        cxt.desc.Add(new KeyValuePair<string, string>("通关评分：", vo.score.ToString()));
        cxt.desc.Add(new KeyValuePair<string, string>("雕像受损：", vo.damage + "%"));
        cxt.items.Add(new KeyValuePair<int, int>(ITEM_ID.GOLD, vo.gold));
        if (vo.rewards != null)
        {
            foreach (var e in vo.rewards)
            {
                cxt.items.Add(new KeyValuePair<int, int>(StringUtil.ToInt(e.Key), e.Value));
            }
        }
        UIMgr.instance.Get<UILevelEnd2>().OnLevelEnd(cxt);
    }



    public void SendEnterHadesLevel(int mode)
    {
        EnterHadesLevelVo req = new EnterHadesLevelVo();
        req.mode = mode;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ENTER_HADES_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_ENTER_HADES_LEVEL)]
    public void OnEnterHadesLevel(EnterHadesLevelResultVo vo)
    {

        HadesLevelModeCfg modeCfg = HadesLevelModeCfg.m_cfgs[vo.mode];
        if (modeCfg == null)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_NONE, RESULT_CODE.CONFIG_ERROR));
            return;
        }

        LevelMgr.instance.ChangeLevel(modeCfg.roomId, modeCfg);
    }


    public void SendSweepHadesLevel(int mode)
    {
        SweepHadesLevelVo req = new SweepHadesLevelVo();
        req.mode = mode;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_SWEEP_HADES_LEVEL, req);
    }

    void ShowHadesLevelEnd(EndHadesLevelResultVo vo)
    {
        UILevelEnd2Context cxt = new UILevelEnd2Context();
        cxt.moveCamera = false;
        cxt.rate = LevelEvaluateCfg.m_cfgs[vo.evaluation].name;
        //cxt.items.Add(new KeyValuePair<int, int>(ITEM_ID.EXP, vo.exp));
        foreach (ItemVo itemVo in vo.itemList)
        {
            cxt.items.Add(new KeyValuePair<int, int>(itemVo.itemId, itemVo.num));
        }
        cxt.desc.Add(new KeyValuePair<string, string>("刷新波次：", "      "+vo.wave.ToString()));
        cxt.desc.Add(new KeyValuePair<string, string>("剩余BOSS：", "      " + vo.bossCount.ToString()));
        UIMgr.instance.Get<UILevelEnd2>().OnLevelEnd(cxt);
    }
    
    [NetHandler(MODULE_ACTIVITY.CMD_SWEEP_HADES_LEVEL)]
    public void OnSweepHadesLevel(EndHadesLevelResultVo vo)
    {
        ShowHadesLevelEnd(vo);
    }

    public void SendEndHadesLevel(int wave, int bossCount)
    {
        EndHadesLevelVo req = new EndHadesLevelVo();
        req.wave = wave;
        req.bossCount = bossCount;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_END_HADES_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_END_HADES_LEVEL, true)]
    public void OnEndHadesLevel(int errorCode, string errorMsg, EndHadesLevelResultVo vo)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { LevelMgr.instance.GotoMaincity(); });
            Debuger.LogError(errorMsg);
            return;
        }

        UIMgr.instance.Close<UILevel>();

        ShowHadesLevelEnd(vo);
    }

    public void SendReqArenaChallengers()
    {
        var msg = new ReqArenaChallengersVo();
        msg.listTime = ActivityMgr.instance.GetArenaChallengersListTime();
        msg.dataTime = ActivityMgr.instance.GetArenaChallengersDataTime();
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_REQ_CHALLENGERS, msg);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_REQ_CHALLENGERS, true)]
    public void OnArenaChallengers(int errorCode, string errorMsg, ReqArenaChallengersResultVo vo)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);

            UIMessage.Show(errorMsg);
            Debuger.LogError(errorMsg);
            //不用return，继续执行，好让UI做清理
        }

        ActivityMgr.instance.OnArenaChallengers(vo);
    }

    public void SendReqStartChallenge(int heroId)
    {
        var msg = new ReqStartChallengeVo();
        msg.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_START_CHALLENGE, msg);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_START_CHALLENGE)]
    public void OnStartChallenge(FullRoleInfoVo vo)
    {
        ArenaBasicCfg cfg = ArenaBasicCfg.Get();
        LevelMgr.instance.ChangeLevel(cfg.roomId, vo);
    }

    public void SendReqGetArenaPos(int heroId)
    {
        var msg = new ReqStartChallengeVo();
        msg.heroId = heroId;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ARENA_GET_POS, msg);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_ARENA_GET_POS)]
    public void OnGetArenaPos(FullRoleInfoVo vo)
    {        
        UIMgr.instance.Open<UIArenaPos>(vo);
    }

    public void SendReqSetArenaPos(string  arenaPos)
    {
        var msg = new ArenaPosVo();
        msg.arenaPos = arenaPos;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ARENA_SET_POS, msg);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_ARENA_SET_POS)]
    public void OnSetArenaPos(ArenaPosVo vo)
    {
        UIArenaPos uiArenaPos = UIMgr.instance.Get<UIArenaPos>();
        if (uiArenaPos.IsOpen)
        {
            uiArenaPos.RefreshHeroArenaPos();
        }
       
    }

    public void SendReqEndArenaCombat(bool weWin)
    {
        var req = new ReqEndArenaCombatVo();
        req.weWin = weWin;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_END_ARENA_COMBAT, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_END_ARENA_COMBAT, true)]
    public void OnReqEndArenaCombatResult(int errorCode, string errorMsg, ReqEndArenaCombatResultVo vo)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { LevelMgr.instance.GotoMaincity(); });
            Debuger.LogError(errorMsg);
            return;
        }

        UIMgr.instance.Close<UILevel>();

        var arenaScene = LevelMgr.instance.CurLevel as ArenaScene;
        if (arenaScene == null)
        {
            Debuger.LogError("OnReqEndArenaCombatResult，arenaScene == null");
            LevelMgr.instance.GotoMaincity();
            return;
        }

        FullRoleInfoVo roleVo = (FullRoleInfoVo)arenaScene.mParam;
        if (roleVo == null)
        {
            Debuger.LogError("OnReqEndArenaCombatResult，roleVo == null");
            LevelMgr.instance.GotoMaincity();
            return;
        }

        Role hero = RoleMgr.instance.Hero;

        UICombatEnd.CombatEndParam param = new UICombatEnd.CombatEndParam();
        UIArenaUpgrade uiArenaUpgrade = UIMgr.instance.Get<UIArenaUpgrade>();
        param.weWin = vo.weWin;
        param.myRankVal = vo.myRankVal;
        param.myOldRankVal = vo.myOldRankVal;
        param.myScoreVal = vo.myScoreVal;
        uiArenaUpgrade.score = vo.myScoreVal;        
        param.myOldScoreVal = vo.myOldScoreVal;
        if (vo.rewards != null)
        {          
            param.rewardId = vo.rewardId;
        }
        
        if (vo.upgradeRewards != null)
        {
            uiArenaUpgrade.upgradeRewardId = vo.upgradeRewardId;
            foreach (var e in vo.upgradeRewards)
            {
                uiArenaUpgrade.items.Add(new KeyValuePair<int, int>(StringUtil.ToInt(e.Key), e.Value));
            }
        }

        param.roleIdLeft = hero.GetString(enProp.roleId);
        param.roleIdRight = roleVo.props.ContainsKey("roleId") ? roleVo.props["roleId"].String : "";

        param.leftDmgParams = arenaScene.m_leftDmgParams;
        param.rightDmgParams = arenaScene.m_rightDmgParams;

        UIMgr.instance.Open<UICombatEnd>(param);
    }

    public void SendBuyArenaChance()
    {
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ARENA_BUY_CHANCE, null);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_ARENA_BUY_CHANCE)]
    public void OnSendBuyArenaChance()
    {
    }

    public void SendReqArenaLog()
    {
        var msg = new ReqArenaLogVo();
        msg.lastTime = ActivityMgr.instance.GetLastArenaTime();
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ARENA_COMBAT_LOG, msg);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_ARENA_COMBAT_LOG)]
    public void OnArenaCombatLog(ReqArenaLogResultVo vo)
    {
        ActivityMgr.instance.OnArenaLogs(vo);
    }

    public void SendEnterVenusLevel()
    {
        EnterVenusLevelVo req = new EnterVenusLevelVo();
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ENTER_VENUS_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_ENTER_VENUS_LEVEL)]
    public void OnEnterVenusLevel()
    {
        LevelMgr.instance.ChangeLevel(VenusLevelBasicCfg.Get().roomId, null);
    }

    public void SendEndVenusLevel(int evaluation, float percentage)
    {
        EndVenusLevelVo req = new EndVenusLevelVo();
        req.evaluation = evaluation;
        req.percentage = percentage;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_END_VENUS_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_END_VENUS_LEVEL, true)]
    public void OnEndVenusLevel(int errorCode, string errorMsg, EndVenusLevelResultVo vo)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { LevelMgr.instance.GotoMaincity(); });
            Debuger.LogError(errorMsg);
            return;
        }

        UIMgr.instance.Close<UIGainStamina>();

        UILevelEnd2Context cxt = new UILevelEnd2Context();
        cxt.moveCamera = false;
        cxt.rate = LevelEvaluateCfg.m_cfgs[vo.evaluation].name;
        cxt.desc.Add(new KeyValuePair<string, string>("进度：", vo.percentage.ToString()+"%"));
        cxt.items.Add(new KeyValuePair<int, int>(ITEM_ID.STAMINA, vo.stamina));
        cxt.items.Add(new KeyValuePair<int, int>(ITEM_ID.REDSOUL, vo.soul));
        UIMgr.instance.Get<UILevelEnd2>().OnLevelEnd(cxt);
    }

    public void SendEnterGuardLevel(int mode)
    {
        EnterGuardLevelVo req = new EnterGuardLevelVo();
        req.mode = mode;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ENTER_GUARD_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_ENTER_GUARD_LEVEL)]
    public void OnEnterGuardLevel(EnterGuardLevelResultVo vo)
    {

        GuardLevelModeCfg modeCfg = GuardLevelModeCfg.m_cfgs[vo.mode];
        if (modeCfg == null)
        {
            UIMessage.Show(ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_NONE, RESULT_CODE.CONFIG_ERROR));
            return;
        }

        LevelMgr.instance.ChangeLevel(modeCfg.roomId, modeCfg);
    }

    public void SendSweepGuardLevel(int mode)
    {
        SweepGuardLevelVo req = new SweepGuardLevelVo();
        req.mode = mode;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_SWEEP_GUARD_LEVEL, req);
    }

    void ShowGuardLevelEnd(EndGuardLevelResultVo vo)
    {
        UILevelEnd2Context cxt = new UILevelEnd2Context();
        cxt.moveCamera = false;
        cxt.rate = LevelEvaluateCfg.m_cfgs[vo.evaluation].name;
        cxt.items.Add(new KeyValuePair<int, int>(ITEM_ID.EXP, vo.exp));
        //cxt.desc.Add(new KeyValuePair<string, string>("刷新波次：", vo.wave.ToString()));
        //cxt.desc.Add(new KeyValuePair<string, string>("主角剩余血量：", String.Format("{0:F}%", vo.roleHp)));
        //cxt.desc.Add(new KeyValuePair<string, string>("家人剩余血量：", String.Format("{0:F}%", vo.familyHp)));
        cxt.desc.Add(new KeyValuePair<string, string>("积分：", vo.point.ToString()));
        foreach (ItemVo itemVo in vo.itemList)
        {
            cxt.items.Add(new KeyValuePair<int, int>(itemVo.itemId, itemVo.num));
        }        
        UIMgr.instance.Get<UILevelEnd2>().OnLevelEnd(cxt);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_SWEEP_GUARD_LEVEL)]
    public void OnSweepGuardLevel(EndGuardLevelResultVo vo)
    {
        ShowGuardLevelEnd(vo);
    }

    public void SendEndGuardLevel(int wave, int point)
    {
        EndGuardLevelVo req = new EndGuardLevelVo();
        req.wave = wave;
        req.point = point;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_END_GUARD_LEVEL, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_END_GUARD_LEVEL, true)]
    public void OnEndGuardLevel(int errorCode, string errorMsg, EndGuardLevelResultVo vo)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { LevelMgr.instance.GotoMaincity(); });
            Debuger.LogError(errorMsg);
            return;
        }

        UIMgr.instance.Close<UILevel>();

        ShowGuardLevelEnd(vo);
    }
    //请求勇士试炼信息
    public void GetWarriorTriedData()
    {
        WarriorTriedDataReq req = new WarriorTriedDataReq();
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_REQ_WARRIOR_TRIED, req);
    }
    //请求勇士试炼信息返回
    [NetHandler(MODULE_ACTIVITY.CMD_REQ_WARRIOR_TRIED)]
    public void OnWarriorTriedDataRes(WarriorTriedDataRes resData)
    {
        if (resData == null)
            return;
        ActivityPart part = RoleMgr.instance.Hero.ActivityPart;
        if (resData.triedData != null)
            part.warriorTried = resData.triedData;
        
        UIWarriorsTried ui = UIMgr.instance.Get<UIWarriorsTried>();
        if (ui.IsOpen)
            ui.UpdatePanelData();
    }
    //请求刷新勇士试炼关卡
    public void RefreshWarrior()
    {
        RefreshWarriorReq req = new RefreshWarriorReq();
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_REFRESH_WARRIOR, req);
    }
    //请求刷新勇士试炼关卡返回
    [NetHandler(MODULE_ACTIVITY.CMD_REFRESH_WARRIOR)]
    public void OnRefreshWarrior(RefreshWarriorRes resData)
    {
        if (resData == null)
            return;
        UIMessage.Show("刷新成功");
        ActivityPart part = RoleMgr.instance.Hero.ActivityPart;
        part.warriorTried.trieds = resData.trieds;
        part.warriorTried.refresh = resData.refresh;
        UIWarriorsTried ui = UIMgr.instance.Get<UIWarriorsTried>();
        if (ui.IsOpen)
            ui.UpdatePanelData();
    }
    //请求进入勇士试炼
    public void SendEnterWarrior(int index)
    {
        EnterWarriorReq req = new EnterWarriorReq();
        req.index = index;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ENTER_WARRIOR_LEVEL, req);
    }
    //请求进入勇士试炼返回
    [NetHandler(MODULE_ACTIVITY.CMD_ENTER_WARRIOR_LEVEL)]
    public void OnEnterWarriorLevel(EnterWarriorRes resData)
    {
        if (resData == null)
            return;
        RoleMgr.instance.Hero.ActivityPart.warrIndex = resData.index+1;
        LevelMgr.instance.ChangeLevel(resData.roomId, null);

    }
    //请求结束勇士试炼关卡
    public void SendEndWarrior(int index, bool isWin)
    {
        EndWarriorReq req = new EndWarriorReq();
        req.index = index;
        req.isWin = isWin;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_END_WARRIOR_LEVEL, req);
    }
    //请求结束勇士试炼关卡返回
    [NetHandler(MODULE_ACTIVITY.CMD_END_WARRIOR_LEVEL, true)]
    public void OnEndWarriorLevel(int errorCode, string errorMsg, EndWarriorRes resData)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);
            UIMessageBox.Open(errorMsg, () => { LevelMgr.instance.GotoMaincity(); });
            Debuger.LogError(errorMsg);
            return;
        }

        WarriorLevelScene level = LevelMgr.instance.CurLevel as WarriorLevelScene;
        if (level != null)
        {
            if (resData.isWin)
                level.OnWin(resData);
            else
                level.OnLose();
        }

        if (resData.isWin)
        {
            ActivityPart part = RoleMgr.instance.Hero.ActivityPart;
            part.warriorTried.remainTried = resData.remainTried;
            part.warriorTried.trieds[resData.index].status = 1;
        }
        //else
        //    RoleMgr.instance.Hero.ActivityPart.warrIndex = 0;   //失败则把这个标记为0，回到主城不会弹出领取界面
    }
    //勇士试炼领取奖励
    public void SendWarriorGetReward(int index)
    {
        GetWarriorRewardReq req = new GetWarriorRewardReq();
        req.index = index;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_WARRIOR_REWARD, req);
    }
    //勇士试炼领取奖励返回
    [NetHandler(MODULE_ACTIVITY.CMD_WARRIOR_REWARD)]
    public void OnWarriorGetReward(GetWarriorRewardRes resData)
    {
        if (resData == null)
            return;
        ActivityPart part = RoleMgr.instance.Hero.ActivityPart;
        part.warriorTried.trieds[resData.index].status = 2;
        if(resData.trieds != null)  //关卡都通关奖励都领完了 自动刷新
        {
            part.warriorTried.trieds = resData.trieds;
            UIMessage.Show("所有试炼完成，自动刷新");
        }
        UIWarriorsTried ui = UIMgr.instance.Get<UIWarriorsTried>();
        if (ui.IsOpen)
            ui.UpdatePanelData();
        UIMessage.Show("领取成功");
    }


    public void SendReqTreasureRob(bool useGold)
    {
        var msg = new ReqTreasureRobRequestVo();
        msg.listTime = ActivityMgr.instance.GetTreasureChallengersListTime();
        msg.dataTime = ActivityMgr.instance.GetTreasureChallengersDataTime();
        msg.useGold = useGold;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_REQ_TREASURE_ROB, msg);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_REQ_TREASURE_ROB)]
    public void OnReqTreasureRob(ReqTreasureRobResultVo vo)
    {
        ActivityMgr.instance.OnTreasureChallengers(vo);
    }

    [NetHandler(MODULE_ACTIVITY.PUSH_TREASURE_ROB_BATTLE_LOG)]
    public void OnTreasureRobBattleLog(TreasureRobBattleLogVo vo)
    {
        ActivityMgr.instance.OnTreasureRobBattleLog(vo);
        if(!UIMgr.instance.Get<UIMainCity>().IsOpen)
        {
            ActivityMgr.instance.AddTreasureRobMsg(string.Format("{0}试图抢夺你的神器碎片，快去查看战报！", vo.name));
        }
        else
        {
            UIMessage.Show(string.Format("{0}试图抢夺你的神器碎片，快去查看战报！", vo.name));
        }

    }

    public void SendStartTreasureRob(int heroId, int battleLogIndex)
    {
        var msg = new StartTreasureRobRequestVo();
        msg.heroId = heroId;
        msg.battleLogIndex = battleLogIndex;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_START_TREASURE_ROB, msg);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_START_TREASURE_ROB)]
    public void OnStartTreasureRob(FullRoleInfoVo vo)
    {
        TreasureRobBasicCfg cfg = TreasureRobBasicCfg.Get();
        LevelMgr.instance.ChangeLevel(cfg.roomId, vo);
    }

    public void SendEndTreasureRob(bool weWin)
    {
        var req = new EndTreasureRobRequestVo();
        req.weWin = weWin;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_END_TREASURE_ROB, req);
    }

    [NetHandler(MODULE_ACTIVITY.CMD_END_TREASURE_ROB, true)]
    public void OnEndTreasureRob(int errorCode, string errorMsg, EndTreasureRobResultVo vo)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { LevelMgr.instance.GotoMaincity(); });
            Debuger.LogError(errorMsg);
            return;
        }

        UIMgr.instance.Close<UILevel>();

        if(!vo.weWin)
        {
            UIMgr.instance.Open<UILevelFail>();
            return;
        }

        UILevelEnd2Context cxt = new UILevelEnd2Context();
        cxt.moveCamera = false;
        cxt.rate = "";
        cxt.items.Add(new KeyValuePair<int, int>(vo.itemId, vo.itemNum));
        UIMgr.instance.Get<UILevelEnd2>().OnLevelEnd(cxt);
    }



    //预言者之塔进入
    public void SendTowerEnter(ProphetTowerType type)
    {
        EnterTowerReq req = new EnterTowerReq();
        req.towerType = (int)type;
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_ENTER_TOWER, req);
    }

    //预言者之塔进入返回 
    [NetHandler(MODULE_ACTIVITY.CMD_ENTER_TOWER)]
    public void OnTowerEnter(EnterTowerRes resData)
    {
        UIProphetTowerLevel ui = UIMgr.instance.Get<UIProphetTowerLevel>();
        if (ui.IsOpen)
            ui.OnEnterRes(resData);
    }

    //预言者之塔挑战结束
    public void SendTowerEnd(EndTowerReq req)
    {
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_END_TOWER, req);
    }

    //预言者之塔挑战返回
    [NetHandler(MODULE_ACTIVITY.CMD_END_TOWER)]
    public void OnTowerEnd(EndTowerRes resData)
    {
        ProphetTowerLevel level = LevelMgr.instance.CurLevel as ProphetTowerLevel;
        if (level == null)
        {
            LevelMgr.instance.GotoMaincity();
            return;
        }

        if (resData.isWin)
            level.OnWin(resData);
        else
            level.OnLose();
    }

    //预言者之塔挑战结束
    public void SendGetTowerReward(GetTowerRewardReq req)
    {
        NetMgr.instance.Send(MODULE.MODULE_ACTIVITY, MODULE_ACTIVITY.CMD_GET_TOWER_REWARD, req);
    }

    //预言者之塔挑战返回
    [NetHandler(MODULE_ACTIVITY.CMD_GET_TOWER_REWARD)]
    public void OnGetTowerReward(GetTowerRewardRes resData)
    {
        UIProphetTowerLevel ui = UIMgr.instance.Get<UIProphetTowerLevel>();
        if (ui.IsOpen)
            ui.OnGetReward(resData.idx);
    }
}