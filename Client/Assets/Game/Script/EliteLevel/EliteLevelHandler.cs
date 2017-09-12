using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[NetModule(MODULE.MODULE_ELITE_LEVEL)]
public class EliteLevelHandler
{
    //发送，进入
    public void SendEnterEliteLevel(int levelId)
    {
        EnterEliteLevelRequestVo request = new EnterEliteLevelRequestVo();
        request.levelId = levelId;
        NetMgr.instance.Send(MODULE.MODULE_ELITE_LEVEL, MODULE_ELITE_LEVEL.CMD_ENTER_ELITE_LEVEL, request);
    }

    //接收，进入
    [NetHandler(MODULE_ELITE_LEVEL.CMD_ENTER_ELITE_LEVEL)]
    public void OnEnterEliteLevel(EnterEliteLevelResultVo info)
    {
        LevelsPart levelPart = RoleMgr.instance.Hero.LevelsPart;
        if (levelPart == null)
            return;

        levelPart.DropMonsterItems = info.dropItems.Get("monsterDrop");
        levelPart.DropSpecialItems = info.dropItems.Get("specialDrop");
        levelPart.DropBossItems = info.dropItems.Get("bossDrop");
        levelPart.DropBoxItems = info.dropItems.Get("boxDrop");

#if DEBUG_DROPITEM
            Debuger.Log("服务器下发掉落奖励");
            Debuger.Log("小怪");
            foreach (ItemVo item in levelPart.DropMonsterItems)
                Debuger.Log("{0} - {1}", item.itemId, item.num);
            Debuger.Log("精英");
            foreach (ItemVo item in levelPart.DropMonsterItems)
                Debuger.Log("{0} - {1}", item.itemId, item.num);
            Debuger.Log("boss");
            foreach (ItemVo item in levelPart.DropMonsterItems)
                Debuger.Log("{0} - {1}", item.itemId, item.num);
#endif

        EliteLevelCfg eliteLevelCfg = EliteLevelCfg.m_cfgs[info.levelId];
        LevelMgr.instance.ChangeLevel(eliteLevelCfg.roomId, eliteLevelCfg);
    }

    //发送，结算
    public void SendEndEliteLevel(EndEliteLevelRequestVo request)
    {
        NetMgr.instance.Send(MODULE.MODULE_ELITE_LEVEL, MODULE_ELITE_LEVEL.CMD_END_ELITE_LEVEL, request);
    }

    //接收，结算
    [NetHandler(MODULE_ELITE_LEVEL.CMD_END_ELITE_LEVEL, true)]
    public void OnEndEliteLevel(int errorCode, string errorMsg, EndEliteLevelResultVo info)
    {
        if (errorCode != 0)
        {
            errorMsg = ErrorCodeCfg.GetErrorDesc(MODULE.MODULE_ACTIVITY, errorCode, errorMsg);

            UIMessageBox.Open(errorMsg, () => { LevelMgr.instance.GotoMaincity(); });
            Debuger.LogError(errorMsg);
            return;
        }


        LevelEndResVo res = new LevelEndResVo();
        res.isWin = info.isWin;
        res.heroExp = info.heroExp;
        res.pet1Exp = info.pet1Exp;
        res.pet2Exp = info.pet2Exp;
        res.roomId = EliteLevelCfg.m_cfgs[info.levelId].roomId;

        EliteLevelScene level = LevelMgr.instance.CurLevel as EliteLevelScene;
        if (level != null)
        {
            if (info.isWin)
                level.OnWin(res);
            else
                level.OnLose();
        }
        RoleMgr.instance.Hero.Fire(MSG_ROLE.ELITELV_CHANGE);

        return;
    }

    //发送，扫荡
    public void SendSweepEliteLevel(int levelId, bool multiple)
    {
        var req = new SweepEliteLevelRequestVo();
        req.levelId = levelId;
        req.multiple = multiple;
        NetMgr.instance.Send(MODULE.MODULE_ELITE_LEVEL, MODULE_ELITE_LEVEL.CMD_SWEEP_ELITE_LEVEL, req);
    }

    //接收，扫荡
    [NetHandler(MODULE_ELITE_LEVEL.CMD_SWEEP_ELITE_LEVEL)]
    public void OnSweepEliteLevel(SweepEliteLevelResultVo res)
    {


        UIMgr.instance.Get<UIEliteLevel>().Reflesh();
        UIMgr.instance.Get<UISweepEliteLevel>().OnSweepResult(res);

        UIHeroUpgrade.CheckOpen();
        RoleMgr.instance.Hero.Fire(MSG_ROLE.GET_REWARD);
        RoleMgr.instance.Hero.Fire(MSG_ROLE.ELITELV_CHANGE);
    }

    //发送，领取首充奖励
    public void SendGetFirstReward(int levelId)
    {
        var req = new GetFirstRewardRequestVo();
        req.levelId = levelId;
        NetMgr.instance.Send(MODULE.MODULE_ELITE_LEVEL, MODULE_ELITE_LEVEL.CMD_GET_FIRST_REWARD, req);
    }

    //接收，扫荡
    [NetHandler(MODULE_ELITE_LEVEL.CMD_GET_FIRST_REWARD)]
    public void OnGetFirstReward(GetFirstRewardResultVo res)
    {
        UIMessage.Show("成功获取奖励");
    }

    //发送，重置
    public void SendResetEliteLevel(int levelId)
    {
        var req = new ResetEliteLevelRequestVo();
        req.levelId = levelId;
        NetMgr.instance.Send(MODULE.MODULE_ELITE_LEVEL, MODULE_ELITE_LEVEL.CMD_RESET_ELITE_LEVEL, req);
    }

    //接收，重置
    [NetHandler(MODULE_ELITE_LEVEL.CMD_RESET_ELITE_LEVEL)]
    public void OnGetFirstReward(ResetEliteLevelResultVo res)
    {
        UIMgr.instance.Get<UIEliteLevel>().Reflesh();
    }


    [NetHandler(MODULE_ELITE_LEVEL.PUSH_ADD_OR_UPDATE_ELITE_LEVEL)]
    public void OnAddOrUpdateEliteLevel(AddOrUpdateEliteLevelVo info)
    {
        Role role = RoleMgr.instance.Hero;
        role.EliteLevelsPart.AddOrUpdateEliteLevel(info.eliteLevel);
    }

}