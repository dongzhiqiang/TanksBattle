"use strict";

var Promise = require("bluebird");

var appUtil = require("../../../libs/appUtil");
var dateUtil = require("../../../libs/dateUtil");
var logUtil = require("../../../libs/logUtil");
var dbUtil = require("../../../libs/dbUtil");
var handlerMgr = require("../../session/handlerMgr");
var ModuleIds = require(".././../netMessage/netMsgConst").ModuleIds;
var ResultCode = require(".././../netMessage/netMsgConst").ResultCode;
var activityMessage = require("../../netMessage/activityMessage");
var CmdIdsActivity = require(".././../netMessage/activityMessage").CmdIdsActivity;
var ResultCodeActivity = require(".././../netMessage/activityMessage").ResultCodeActivity;
var enProp = require("../../enumType/propDefine").enProp;
var enActProp = require("../../enumType/activityPropDefine").enActProp;
var arenaConfig = require("../../gameConfig/arenaConfig");
var rankTypes = require("../../enumType/rankDefine").rankTypes;
var enItemId = require("../../enumType/globalDefine").enItemId;
var rankMgr = require("../../rank/rankMgr");
var roleMgr = require("../../role/roleMgr");
var vipConfig = require("../../gameConfig/vipConfig");
var treasureRobConfig = require("../../gameConfig/treasureRobConfig");
var treasureConfig = require("../../gameConfig/treasureConfig");
var getRankInfo = require("./treasureRobUtil").getRankInfo;
var selectRankItem = require("./treasureRobUtil").selectRankItem;
var getPieceItem = require("./treasureRobUtil").getPieceItem;
var robed = require("./treasureRobUtil").robed;
var enSystemId = require("../../enumType/systemDefine").enSystemId;
var systemMgr = require("../../system/systemMgr");

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ReqTreasureRobRequestVo} reqObj
 */
function reqTreasureRob(session, role, msgObj, reqObj)
{
    var listTime = reqObj.listTime;
    var dataTime = reqObj.dataTime;
    var useGold = reqObj.useGold;
    var actPart = role.getActivityPart();
    var serverData = actPart.getServerData();

    var retObj = new activityMessage.ReqTreasureRobResultVo();

    var rankData = rankMgr.getRankData(rankTypes.realPower);
    //如果是同一天不重新获取
    //如果有对手且客户端的数据是新的
    var challengers = actPart.getTreasureRobChallengers();
    var idlistNew = challengers && challengers.length > 0 && actPart.getNumber(enActProp.treasureLstTime) === listTime;
    if (idlistNew && rankData.upTime === dataTime && !useGold)
    {
        retObj.clientNew = true;
        retObj.listTime = actPart.getNumber(enActProp.treasureLstTime);
        retObj.dataTime = rankData.upTime;

        msgObj.setResponseData(ResultCode.SUCCESS, retObj);
        role.send(msgObj);
        return;
    }

    var basicCfg = treasureRobConfig.getTreasureRobBasicCfg();
    var rankInfo = getRankInfo(role, basicCfg.minPowerRate, basicCfg.maxPowerRate);

    if(useGold)
    {
        if(!role.getItemsPart().canCostGold(basicCfg.fleshGold))
        {
            msgObj.setResponseData(ResultCode.NO_ENOUGH_GOLD);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costGold(basicCfg.fleshGold);
    }

    if(useGold || !(challengers && challengers.length > 0  && dateUtil.isSameDay(actPart.getNumber(enActProp.treasureLstTime), dateUtil.getTimestamp())))
    {
        challengers = [];
        var exclude = [];
        exclude.push(role.getHeroId());
        for(var i=0; i<3; i++)
        {
            var heroId = selectRankItem(rankInfo, exclude);
            if(heroId)
            {
                exclude.push(heroId);
                var itemInfo = getPieceItem(role);
                var challengeInfo = {"heroId":heroId,"itemInfo":itemInfo};
                challengers.push(challengeInfo);
            }
        }
        actPart.setTreasureRobChallengers(challengers);
        actPart.setNumber(enActProp.treasureLstTime, dateUtil.getTimestamp());
    }

    var chaInfos = [];

    for (i = 0; i < challengers.length; ++i)
    {
        var challenger = challengers[i];
        var rankVal = rankMgr.getRankValueByKey(rankTypes.realPower, challenger.heroId);

        if (rankVal >= 0)
        {
            var item = {rank:rankVal, info:rankInfo.rankArr[rankVal], itemId:challenger.itemInfo.itemId, itemNum:challenger.itemInfo.itemNum};
            chaInfos.push(item);
        }
    }

    var battleLogs = actPart.getTreasureRobBattleLogs();
    if(battleLogs.length>0 && !battleLogs[0].itemId)
    {
        //暂时 修正错误
        battleLogs = [];
        actPart.setTreasureRobBattleLogs(battleLogs);
    }

    retObj.clientNew = false;
    retObj.listTime = actPart.getNumber(enActProp.treasureLstTime);
    retObj.dataTime = rankData.upTime;
    retObj.challengers = chaInfos;
    retObj.battleLogs = battleLogs

    msgObj.setResponseData(ResultCode.SUCCESS, retObj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_REQ_TREASURE_ROB, reqTreasureRob, activityMessage.ReqTreasureRobRequestVo);



var startTreasureRobCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {StartTreasureRobRequestVo} reqObj
     */
    function * (session, role, msgObj, reqObj) {
        var heroId = reqObj.heroId;
        var battleLogIndex = reqObj.battleLogIndex;
        var actPart = role.getActivityPart();
        var serverData = actPart.getServerData();
        var challengers = actPart.getTreasureRobChallengers();

        var basicCfg = treasureRobConfig.getTreasureRobBasicCfg();

        var curTime = dateUtil.getTimestamp();
        var treasureTime = actPart.getNumber(enActProp.treasureTime);
        var treasureCnt = actPart.getNumber(enActProp.treasureCnt);

        var errObj={};
        if(!systemMgr.isEnabled(role,enSystemId.treasureRob,errObj))
        {
            msgObj.setResponseData(ResultCodeActivity.SYSTEM_NOT_OPEN);
            role.send(msgObj);
            return;
        }

        var leftCnt = dateUtil.isToday(treasureTime) ? basicCfg.dayMaxCnt-treasureCnt : basicCfg.dayMaxCnt;
        if (leftCnt <= 0 && battleLogIndex==-1)
        {
            msgObj.setResponseData(ResultCodeActivity.DAY_MAX_CNT);
            role.send(msgObj);
            return;
        }

        var found = false;
        var challengeIndex = -1;
        if(battleLogIndex>=0)
        {
            var battleLogs = actPart.getTreasureRobBattleLogs();
            if(battleLogIndex<battleLogs.length)
            {
                var battleLog = battleLogs[battleLogIndex];
                if(battleLog.heroId==heroId&&!battleLog.iStart&&!battleLog.iWin&&!battleLog.revenged)
                {
                    found = true;
                }
            }
        }
        else
        {
            for(var i=0; i<challengers.length; i++)
            {
                var challenger = challengers[i];
                if(challenger.heroId == heroId)
                {
                    found = true;
                    challengeIndex = i;
                    break;
                }
            }
        }
        if (!found)
        {
            msgObj.setResponseData(ResultCodeActivity.TREASURE_NOT_CHALLENGER);
            role.send(msgObj);
            return;
        }

        var roleOp = yield roleMgr.findRoleOrLoadOfflineByHeroId(heroId);
        if (roleOp) {
            //保存对手的主角ID
            serverData.treasureHeroId = heroId;
            serverData.treasureBLogIndex = battleLogIndex;

            msgObj.setResponseData(ResultCode.SUCCESS, roleOp.getProtectNetData());
            role.send(msgObj);
        }
        else {
            msgObj.setResponseData(ResultCodeActivity.ROLE_NOT_EXISTS);
            role.send(msgObj);
        }
    }
);

function startTreasureRob(session, role, msgObj, reqObj) {
    startTreasureRobCoroutine(session, role, msgObj, reqObj).catch (function (err){
        logUtil.error("arenaHandler~startTreasureRob", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_START_TREASURE_ROB, startTreasureRob, activityMessage.StartTreasureRobRequestVo);


var endTreasureRobCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {EndTreasureRobRequestVo} reqObj
     */
    function * (session, role, msgObj, reqObj) {
        var weWin = reqObj.weWin;
        var actPart = role.getActivityPart();
        var serverData = actPart.getServerData();
        var opHeroId = serverData.treasureHeroId;
        var battleLogIndex = serverData.treasureBLogIndex;
        var challengers = actPart.getTreasureRobChallengers();

        if (!opHeroId)
        {
            msgObj.setResponseData(ResultCodeActivity.LVL_NO_START);
            role.send(msgObj);
            return;
        }

        var basicCfg = treasureRobConfig.getTreasureRobBasicCfg();

        var curTimestamp = dateUtil.getTimestamp();
        var treasureTime = actPart.getNumber(enActProp.treasureTime);
        var treasureCnt = actPart.getNumber(enActProp.treasureCnt);

        var challenger;
        var challengeIndex = -1;
        var battleLog;
        if(battleLogIndex>=0)
        {
            var battleLogs = actPart.getTreasureRobBattleLogs();
            if(battleLogIndex<battleLogs.length)
            {
                battleLog = battleLogs[battleLogIndex];
            }
        }
        else
        {
            for(var i=0; i<challengers.length; i++)
            {
                var ca = challengers[i];
                if(ca.heroId == opHeroId)
                {
                    challenger = ca;
                    challengeIndex = i;
                    break;
                }
            }
        }
        if (!battleLog && !challenger)
        {
            msgObj.setResponseData(ResultCodeActivity.TREASURE_NOT_CHALLENGER);
            role.send(msgObj);
            return;
        }

        //获取对手的数据
        var opRoleId = "";
        var opName = "";

        /**
         * @type {Role}
         */
        var roleOp = null;
        /**
         * @type {ActivityPart}
         */
        var actPartOp = null;

        roleOp = yield roleMgr.findRoleOrLoadOfflineByHeroId(opHeroId);
        if (roleOp) {
            actPartOp = roleOp.getActivityPart();
            opRoleId = roleOp.getString(enProp.roleId);
            opName = roleOp.getString(enProp.name);
        }
        else {
            msgObj.setResponseData(ResultCodeActivity.ROLE_NOT_EXISTS);
            role.send(msgObj);
            return;
        }

        // TOD，触发对方的抢夺战报
        if(challenger) {
            if (!dateUtil.isToday(actPartOp.getNumber(enActProp.treasureRobedTime))) {
                actPartOp.startBatch();
                actPartOp.setNumber(enActProp.treasureRobedTime, curTimestamp);
                actPartOp.setNumber(enActProp.treasureRobedMax, appUtil.getRandom(0, 3));
                actPartOp.setNumber(enActProp.treasureRobedCnt, 0);
                actPartOp.endBatch();
            }
            if (actPartOp.getNumber(enActProp.treasureRobedCnt) < actPartOp.getNumber(enActProp.treasureRobedMax)) {
                robed(roleOp, role.getHeroId(), role.getString(enProp.name));
            }
        }


        //用户数据
        actPart.startBatch();

        if(dateUtil.isToday(treasureTime))
        {
            if(!battleLog )
                actPart.setNumber(enActProp.treasureCnt, treasureCnt+1);
        }
        else {
            if (!battleLog) {
                actPart.setNumber(enActProp.treasureCnt, 1);
            }
            else
            {
                actPart.setNumber(enActProp.treasureCnt, 0);
            }
        }
        actPart.setNumber(enActProp.treasureTime, curTimestamp);
        actPart.setNumber(enActProp.treasureLstTime, curTimestamp);

        actPart.endBatch();

        var itemId, itemNum;
        if(challenger)
        {
            itemId = challenger.itemInfo.itemId;
            itemNum = challenger.itemInfo.itemNum;
        }
        else
        {
            itemId = battleLog.itemId;
            itemNum = 1;
        }


        //保存自己的挑战记录
        if(battleLog)
        {
            battleLog.revenged = true;
        }
        actPart.addTreasureRobBattleLog(opHeroId, opName, itemId, itemNum, true, weWin, curTimestamp);

        //清除当前对手的主角ID
        delete serverData.treasureHeroId;
        delete serverData.treasureBLogIndex;

        //清除当前挑战对象
        var exclude = [];
        exclude.push(role.getHeroId());
        for(var i=0; i<challengers.length; i++)
        {
            exclude.push(challengers[i].heroId);
        }
        var rankInfo = getRankInfo(role, basicCfg.minPowerRate, basicCfg.maxPowerRate);
        var heroId = selectRankItem(rankInfo, exclude);
        if(heroId)
        {
            exclude.push(heroId);
            var itemInfo = getPieceItem(role);
            var challengeInfo = {"heroId":heroId,"itemInfo":itemInfo};
            challengers[challengeIndex] = challengeInfo;
        }
        else
        {
            challengers.splice(challengeIndex,1);
        }
        actPart.setTreasureRobChallengers(challengers);

        //如果赢了，获得抢夺道具
        if (weWin)
        {
            role.getItemsPart().addItem(itemId, itemNum);
        }

        var bodyObj = new activityMessage.EndTreasureRobResultVo();
        bodyObj.weWin = weWin;
        bodyObj.itemId = itemId;
        bodyObj.itemNum = itemNum;

        msgObj.setResponseData(ResultCode.SUCCESS, bodyObj);
        role.send(msgObj);
    }
);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EndTreasureRobRequestVo} reqObj
 */
function endTreasureRob(session, role, msgObj, reqObj) {
    endTreasureRobCoroutine(session, role, msgObj, reqObj).catch (function (err){
        logUtil.error("arenaHandler~endTreasureRob", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_END_TREASURE_ROB, endTreasureRob, activityMessage.EndTreasureRobRequestVo);

