using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NetModule(MODULE.MODULE_LEVEL)]
public class LevelHandler
{
    #region Fields

    #endregion


    #region Properties

    #endregion

    #region Net
    
    //发送，进入
    public void SendEnter(string levelId)
    {
        RoomCfg cfg = RoomCfg.GetRoomCfgByID(levelId);
        LevelEnterReqVo request = new LevelEnterReqVo();
        request.roomId = cfg.id;
        request.nodeId = cfg.roomNodeId;
        NetMgr.instance.Send(MODULE.MODULE_LEVEL, MODULE_LEVEL.CMD_ENTER, request);
    }

    //发送，结算
    public void SendEnd(LevelEndReqVo request)
    {
        NetMgr.instance.Send(MODULE.MODULE_LEVEL, MODULE_LEVEL.CMD_END, request);
    }

    //发送，星级奖励
    public void SendStar(LevelStarReqVo request)
    {
        NetMgr.instance.Send(MODULE.MODULE_LEVEL, MODULE_LEVEL.CMD_STAR, request);
    }

    //发送，扫荡
    public void SendSweep(string roomId, bool multiple)
    {
        var req = new SweepLevelReq();
        req.roomId = roomId;
        req.multiple = multiple;
        NetMgr.instance.Send(MODULE.MODULE_LEVEL, MODULE_LEVEL.CMD_SWEEP, req);
    }

    //接收，进入
    [NetHandler(MODULE_LEVEL.CMD_ENTER, true)]
    public void OnEnter(int errorCode, string errorMsg, LevelUpdateResVo levelInfo)
    {
        if (errorCode == 0)
        {
            LevelsPart levelPart = RoleMgr.instance.Hero.LevelsPart;
            if (levelPart == null)
                return;

            levelPart.CurNodeId = levelInfo.curNode;
            levelPart.CurLevelId = levelInfo.curLevel;

            levelPart.DropMonsterItems = levelInfo.dropItems.Get("monsterDrop");
            levelPart.DropSpecialItems = levelInfo.dropItems.Get("specialDrop");
            levelPart.DropBossItems = levelInfo.dropItems.Get("bossDrop");
            levelPart.DropBoxItems = levelInfo.dropItems.Get("boxDrop");

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

            if (levelInfo.level != null)
            {
                levelPart.UpdateLevelInfo(levelInfo.level);
                LevelMgr.instance.ChangeLevel(levelInfo.level.roomId);
            }
            return;
        }
    }

    //接收，结算
    [NetHandler(MODULE_LEVEL.CMD_END, true)]
    public void OnEnd(int errorCode, string errorMsg, LevelEndResVo info)
    {
        if (errorCode == 0)
        {
            LevelsPart levelPart = RoleMgr.instance.Hero.LevelsPart;
            if (levelPart == null)
                return;

            if (info.updateInfo == null)
                Debug.LogError("结算时同步关卡数据出错");
            else
            {
                levelPart.CurNodeId = info.updateInfo.curNode;
                levelPart.CurLevelId = info.updateInfo.curLevel;

                if (info.updateInfo.level != null)
                {
                    levelPart.UpdateLevelInfo(info.updateInfo.level);
                }
            }

            LevelScene level = LevelMgr.instance.CurLevel as LevelScene;
            if (level != null)  
            {
                if (info.isWin)
                    level.OnWin(info);
                else
                    level.OnLose();
            }
            

            return;
        }
    }

    //接收，扫荡
    [NetHandler(MODULE_LEVEL.CMD_SWEEP)]
    public void OnSweep(SweepLevelRes res)
    {
        LevelsPart levelPart = RoleMgr.instance.Hero.LevelsPart;
        if (levelPart == null)
            return;

        levelPart.UpdateLevelInfo(res.levelInfo);
        levelPart.CurNodeId = res.curNode;
        levelPart.CurLevelId = res.curLevel;

        UIMgr.instance.Get<UILevelDetail>().UpdateDetail();
        UIMgr.instance.Get<UILevelSelect>().RefreshLevelItem(res.roomId);
        UIMgr.instance.Get<UISweepLevel>().OnSweepResult(res);

        UIHeroUpgrade.CheckOpen();
        RoleMgr.instance.Hero.Fire(MSG_ROLE.GET_REWARD);
    }

    //接收，星级奖励
    [NetHandler(MODULE_LEVEL.CMD_STAR)]
    public void OnGetStarReward(LevelStarRewardRes res)
    {
        UIMgr.instance.Get<UILevelSelect>().ShowStarReward(res.nodeId, res.starNum);
    }
    #endregion
}
