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
var ArenaPosVo= require(".././../netMessage/activityMessage").ArenaPosVo;
var ResultCodeActivity = require(".././../netMessage/activityMessage").ResultCodeActivity;
var enProp = require("../../enumType/propDefine").enProp;
var enActProp = require("../../enumType/activityPropDefine").enActProp;
var arenaConfig = require("../../gameConfig/arenaConfig");
var rankTypes = require("../../enumType/rankDefine").rankTypes;
var enItemId = require("../../enumType/globalDefine").enItemId;
var rankMgr = require("../../rank/rankMgr");
var roleMgr = require("../../role/roleMgr");
var vipConfig = require("../../gameConfig/vipConfig");
var arenaBuyConfig = require("../../gameConfig/arenaBuyConfig");
var rewardConfig = require("../../gameConfig/rewardConfig");


function getRankGap(minFactor, maxFactor, myRankVal, candidateNum, numToChoose)
{
    var rankGap = appUtil.getRandom(minFactor / 1000 * myRankVal, maxFactor / 1000 * myRankVal);
    rankGap = rankGap >= 2 ? rankGap : appUtil.getRandom(1, 2);
    rankGap = Math.max(1, Math.min(rankGap, candidateNum / numToChoose));
    return Math.floor(rankGap);
}

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ReqArenaChallengersVo} reqObj
 */
function reqChallengers(session, role, msgObj, reqObj)
{
    var listTime = reqObj.listTime;
    var dataTime = reqObj.dataTime;
    var actPart = role.getActivityPart();
    var serverData = actPart.getServerData();

    var retObj = new activityMessage.ReqArenaChallengersResultVo();

    var rankData = rankMgr.getRankData(rankTypes.arena);
    //如果有对手且客户端的数据是新的
    var idlistNew = serverData.arenaCHeroIds && serverData.arenaCHeroIds.length > 0 && serverData.arenaCTime === listTime;
    if (idlistNew && rankData.upTime === dataTime)
    {
        retObj.clientNew = true;
        retObj.listTime = serverData.arenaCTime;
        retObj.dataTime = rankData.upTime;

        msgObj.setResponseData(ResultCode.SUCCESS, retObj);
        role.send(msgObj);
        return;
    }

    var rankArr = rankData.data;
    var rankArrLen = rankArr.length;
    var myRankVal = rankMgr.getRankValueByKey(rankTypes.arena, role.getHeroId());
    if (myRankVal < 0)
        myRankVal = rankArrLen;

    var challengers = [];
    var i, idx, lastIdx, len, item, heroIds;

    var basicCfg = arenaConfig.getArenaBasicCfg();
    var chooseUpwards = basicCfg.chooseUpwards;
    var chooseDownwards = basicCfg.chooseDownwards;
    var totalChoose = chooseUpwards + chooseDownwards;

    //如果对手主角ID列表没变，那就是角色的信息变了，更新角色信息
    if (idlistNew)
    {
        heroIds = serverData.arenaCHeroIds;
        for (i = 0; i < heroIds.length; ++i)
        {
            var heroId = heroIds[i];
            var rankVal = rankMgr.getRankValueByKey(rankTypes.arena, heroId);
            if (rankVal >= 0)
            {
                item = {rank:rankVal, info:rankArr[rankVal]};
                challengers.push(item);
            }
        }

        //如果有对手不在排行榜里了，进一步处理
        if (challengers.length !== heroIds.length)
        {
            //如果剩余的太少了，而实际排行榜项目比较多
            if (challengers.length / totalChoose < 1 / 2 && rankArrLen / totalChoose > 2)
            {
                challengers = [];
            }
            //否则，更新一下服务器的对方ID列表
            else
            {
                heroIds = [];
                for (i = 0; i < challengers.length; ++i)
                {
                    item = challengers[i];
                    heroIds.push(item.info.key);
                }
                serverData.arenaCHeroIds = heroIds;
                serverData.arenaCTime = dateUtil.getTimestamp();
            }
        }
    }

    //如果前面获取对方信息太少，那就这里重新选一次
    if (challengers.length <= 0)
    {
        var minFactor = basicCfg.chooseGapMinFactor;
        var maxFactor = basicCfg.chooseGapMaxFactor;
        heroIds = [];

        //先向上取
        for (i = 0, lastIdx = myRankVal, len = basicCfg.chooseUpwards; i < len; ++i)
        {
            idx = lastIdx - getRankGap(minFactor, maxFactor, myRankVal, lastIdx, chooseUpwards  - challengers.length);
            if (idx < 0)
                break;
            item = {rank:idx, info:rankArr[idx]};
            challengers.push(item);
            heroIds.push(item.info.key);
            lastIdx = idx;
        }
        //反转一下，因为是从后往前的
        challengers.reverse();

        //再向下取
        for (i = 0, lastIdx = myRankVal, len = Math.min(rankArrLen - lastIdx - 1, totalChoose - challengers.length); i < len; ++i)
        {
            idx = lastIdx + getRankGap(minFactor, maxFactor, myRankVal, rankArrLen - lastIdx - 1, totalChoose - challengers.length);
            if (idx >= rankArrLen)
                break;
            item = {rank:idx, info:rankArr[idx]};
            challengers.push(item);
            heroIds.push(item.info.key);
            lastIdx = idx;
        }

        serverData.arenaCHeroIds = heroIds;
        serverData.arenaCTime = dateUtil.getTimestamp();
    }

    retObj.clientNew = false;
    retObj.listTime = serverData.arenaCTime;
    retObj.dataTime = rankData.upTime;
    retObj.myRankVal = myRankVal;
    retObj.challengers = challengers;

    msgObj.setResponseData(ResultCode.SUCCESS, retObj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_REQ_CHALLENGERS, reqChallengers, activityMessage.ReqArenaChallengersVo);

var startChallengeCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {ReqStartChallengeVo} reqObj
     */
    function * (session, role, msgObj, reqObj) {
        var heroId = reqObj.heroId;
        var actPart = role.getActivityPart();
        var serverData = actPart.getServerData();

        var basicCfg = arenaConfig.getArenaBasicCfg();
        var roleLv = role.getNumber(enProp.level);
        if (roleLv < basicCfg.openLevel)
        {
            msgObj.setResponseData(ResultCodeActivity.HERO_LVL_WRONG);
            role.send(msgObj);
            return;
        }

        var curTime = dateUtil.getTimestamp();
        var arenaTime = actPart.getNumber(enActProp.arenaTime);
        var arenaCnt = actPart.getNumber(enActProp.arenaCnt);
        var arenaBuyCntTime = actPart.getNumber(enActProp.arenaBuyCntTime);
        var arenaBuyCnt = actPart.getNumber(enActProp.arenaBuyCnt);

        var timePass = curTime >= arenaTime ? curTime - arenaTime : arenaTime - curTime;
        var vipCfg = vipConfig.getVipConfig(role.getNumber(enProp.vipLv));
        if (timePass < vipCfg.arenaFreezeTime)
        {
            msgObj.setResponseData(ResultCodeActivity.IN_COOL_DOWN);
            role.send(msgObj);
            return;
        }

        var leftCnt = Math.max(0, basicCfg.freeChance + (dateUtil.isToday(arenaBuyCntTime) ? arenaBuyCnt : 0) - (dateUtil.isToday(arenaTime) ? arenaCnt : 0));
        if (leftCnt <= 0)
        {
            msgObj.setResponseData(ResultCodeActivity.DAY_MAX_CNT);
            role.send(msgObj);
            return;
        }

        if (!serverData.arenaCHeroIds || !serverData.arenaCHeroIds.existsValue(heroId))
        {
            msgObj.setResponseData(ResultCodeActivity.ARENA_NOT_CHALLENGER);
            role.send(msgObj);
            return;
        }

        var roleOp = yield roleMgr.findRoleOrLoadOfflineByHeroId(heroId);
        if (roleOp) {
            //保存对手的主角ID
            serverData.arenaCurCHeroId = heroId;

            msgObj.setResponseData(ResultCode.SUCCESS, roleOp.getProtectNetData());
            role.send(msgObj);
        }
        else {
            msgObj.setResponseData(ResultCodeActivity.ROLE_NOT_EXISTS);
            role.send(msgObj);
        }
    }
);

var getRoleOpVoCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {ReqStartChallengeVo} reqObj
     */
    function * (session, role, msgObj, reqObj) {

        var heroId = reqObj.heroId;
        var roleOp = yield roleMgr.findRoleOrLoadOfflineByHeroId(heroId);
        if (roleOp) {

            msgObj.setResponseData(ResultCode.SUCCESS, roleOp.getProtectNetData());
            role.send(msgObj);
        }
        else {
            msgObj.setResponseData(ResultCodeActivity.ROLE_NOT_EXISTS);
            role.send(msgObj);
        }
    }
);

function startChallenge(session, role, msgObj, reqObj) {
    startChallengeCoroutine(session, role, msgObj, reqObj).catch (function (err){
        logUtil.error("arenaHandler~startChallenge", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_START_CHALLENGE, startChallenge, activityMessage.ReqStartChallengeVo);

function getArenaPos(session, role, msgObj, reqObj) {
    getRoleOpVoCoroutine(session, role, msgObj, reqObj).catch (function (err){
        logUtil.error("arenaHandler~getRoleOpVo", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ARENA_GET_POS, getArenaPos, activityMessage.ReqStartChallengeVo);


/**
 *设置竞技场阵型
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ArenaPosVo} reqObj
 * @return {ArenaPosVo}
 */
function setArenaPos(session, role, msgObj, reqObj) {

    var actPart = role.getActivityPart();
    actPart.setString(enActProp.arenaPos, reqObj.arenaPos);
    return new ArenaPosVo(reqObj.arenaPos);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ARENA_SET_POS, setArenaPos , ArenaPosVo);


var endArenaCombatCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {ReqEndArenaCombatVo} reqObj
     */
    function * (session, role, msgObj, reqObj) {
        var weWin = reqObj.weWin;
        var actPart = role.getActivityPart();
        var serverData = actPart.getServerData();
        var opHeroId = serverData.arenaCurCHeroId;

        if (!opHeroId)
        {
            msgObj.setResponseData(ResultCodeActivity.LVL_NO_START);
            role.send(msgObj);
            return;
        }

        var basicCfg = arenaConfig.getArenaBasicCfg();
        //获取竞技场排行数据
        var arenaRankLength = rankMgr.getRankData(rankTypes.arena).data.length;
        var curTimestamp = dateUtil.getTimestamp();

        //获取对手的数据
        var opOldArenaScore = 0;
        var opRoleId = "";
        var opName = "";
        var opOldRankVal = rankMgr.getRankValueByKey(rankTypes.arena, opHeroId);
        opOldRankVal = opOldRankVal < 0 ? arenaRankLength : opOldRankVal;

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
            opOldArenaScore = actPartOp.getNumber(enActProp.arenaScore);
            opRoleId = roleOp.getString(enProp.roleId);
            opName = roleOp.getString(enProp.name);
        }
        else {
            msgObj.setResponseData(ResultCodeActivity.ROLE_NOT_EXISTS);
            role.send(msgObj);
            return;
        }

        //获取自己的数据
        var myHeroId = role.getHeroId();
        var myOldArenaScore = actPart.getNumber(enActProp.arenaScore);
        var myOldArenaTime = actPart.getNumber(enActProp.arenaTime);
        var myOldArenaCnt = actPart.getNumber(enActProp.arenaCnt);
        var myOldArenaMaxScore = actPart.getNumber(enActProp.arenaMaxScore);
        var myOldArenaConWin = actPart.getNumber(enActProp.arenaConWin);
        var myOldRankVal = rankMgr.getRankValueByKey(rankTypes.arena, myHeroId);
        myOldRankVal = myOldRankVal < 0 ? arenaRankLength : myOldRankVal;

        //计算积分（连胜有额外积分加成）
        var myArenaConWin = weWin ? myOldArenaConWin + 1 : 0;   //如果本次胜，那就连胜加1，如果败，那就连胜重置为0
        var isContinuousWin = myArenaConWin >= 2;   //是否连胜
        var myOldGrade = arenaConfig.getGradeByScore(myOldArenaScore);
        var myOldGradeCfg = arenaConfig.getArenaGradeCfg(myOldGrade);
        var myArenaCnt = dateUtil.isToday(myOldArenaTime) ? myOldArenaCnt + 1 : 1;
        var myArenaScore = myOldArenaScore + (weWin ? myOldGradeCfg.atkWinScore + (isContinuousWin ? myOldGradeCfg.atkCtnWinScore : 0) : 0);
        var myArenaMaxScore = Math.max(myOldArenaMaxScore, myArenaScore);

        var opArenaScore = Math.max(0, opOldArenaScore - (weWin ? myOldGradeCfg.dfdLoseScore : 0));

        //保存对手的数据(用户数据和排行榜)
        //用户数据
        actPartOp.setNumber(enActProp.arenaScore, opArenaScore);
        //排行榜
        rankMgr.addToRankByRole(rankTypes.arena, roleOp);

        //保存对手的挑战记录
        var opNewRankVal = rankMgr.getRankValueByKey(rankTypes.arena, opHeroId);
        opNewRankVal = opNewRankVal < 0 ? arenaRankLength : opNewRankVal;
        actPartOp.addArenaLog(!weWin, opOldRankVal, opNewRankVal, opHeroId, opRoleId, opName, opOldArenaScore, curTimestamp, basicCfg.logNum);

        //保存自己的数据(用户数据和排行榜)
        //用户数据
        actPart.startBatch();
        actPart.setNumber(enActProp.arenaScore, myArenaScore);
        actPart.setNumber(enActProp.arenaTime, curTimestamp);
        actPart.setNumber(enActProp.arenaCnt, myArenaCnt);
        actPart.setNumber(enActProp.arenaMaxScore, myArenaMaxScore);
        actPart.setNumber(enActProp.arenaConWin, myArenaConWin);
        if(weWin)
            actPart.addNumber(enActProp.arenaTotalWin,1);
        actPart.endBatch();
        //排行榜
        rankMgr.addToRankByRole(rankTypes.arena, role);

        //保存自己的挑战记录
        var myNewRankVal = rankMgr.getRankValueByKey(rankTypes.arena, myHeroId);
        myNewRankVal = myNewRankVal < 0 ? arenaRankLength : myNewRankVal;
        actPart.addArenaLog(weWin, myOldRankVal, myNewRankVal, opHeroId, opRoleId, opName, opOldArenaScore, curTimestamp, basicCfg.logNum);

        //如果本人首次晋级新段位，给奖励
        var myNewGrade = arenaConfig.getGradeByScore(myArenaScore);
        var myNewGradeCfg = arenaConfig.getArenaGradeCfg(myNewGrade);
        var myOldMaxGrade = arenaConfig.getGradeByScore(myOldArenaMaxScore);


        var upgradeRewardItems = {};
        var upgradeRewardId = 0;
        //新积分的段位比以往最高段位还高？
        if (myNewGrade > myOldMaxGrade) {
            var itemsPart = role.getItemsPart();
            itemsPart.addRewards(myNewGradeCfg.upgradeRewardId);
            upgradeRewardItems = rewardConfig.getRandomReward(myNewGradeCfg.upgradeRewardId);
            upgradeRewardId = myNewGradeCfg.upgradeRewardId;
        }

        //清除当前对手的主角ID
        delete serverData.arenaCurCHeroId;

        var rewardItems = {};
        var rewardId = 0;
        //如果赢了，就清除对手列表，下次刷新一批，下发单场胜利或失败奖励
        if (weWin)
        {
            delete serverData.arenaCHeroIds;
            delete serverData.arenaCTime;
            rewardItems = rewardConfig.getRandomReward(basicCfg.winRewardId);
            rewardId = basicCfg.winRewardId;
            role.getItemsPart().addRewards(basicCfg.winRewardId);
        }
        else
        {
            rewardItems = rewardConfig.getRandomReward(basicCfg.loseRewardId);
            rewardId = basicCfg.loseRewardId;
            role.getItemsPart().addRewards(basicCfg.loseRewardId);
        }

        //这里如果没上榜，名次不能用-1
        var bodyObj = new activityMessage.ReqEndArenaCombatResultVo();
        bodyObj.weWin = weWin;
        bodyObj.myRankVal = myNewRankVal;
        bodyObj.myOldRankVal = myOldRankVal;
        bodyObj.rewards = rewardItems;
        bodyObj.upgradeRewards = upgradeRewardItems;
        bodyObj.rewardId = rewardId;
        bodyObj.upgradeRewardId = upgradeRewardId;
        bodyObj.myScoreVal = myArenaScore;
        bodyObj.myOldScoreVal = myOldArenaScore;

        msgObj.setResponseData(ResultCode.SUCCESS, bodyObj);
        role.send(msgObj);
    }
);

function endArenaCombat(session, role, msgObj, reqObj) {
    endArenaCombatCoroutine(session, role, msgObj, reqObj).catch (function (err){
        logUtil.error("arenaHandler~endArenaCombat", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_END_ARENA_COMBAT, endArenaCombat, activityMessage.ReqEndArenaCombatVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {null} reqObj
 */
function buyArenaChance(session, role, msgObj, reqObj) {
    var basicCfg = arenaConfig.getArenaBasicCfg();
    var actPart = role.getActivityPart();
    var arenaBuyCntTime = actPart.getNumber(enActProp.arenaBuyCntTime);
    var arenaBuyCnt = actPart.getNumber(enActProp.arenaBuyCnt);
    var todayBuyCnt = dateUtil.isToday(arenaBuyCntTime) ? arenaBuyCnt : 0;
    var vipCfg = vipConfig.getVipConfig(role.getNumber(enProp.vipLv));
    var arenaBuyCfg = arenaBuyConfig.getArenaBuyConfig(todayBuyCnt);
    var itemsPart = role.getItemsPart();

    if (role.getNumber(enProp.diamond) < arenaBuyCfg.price)
    {
        msgObj.setResponseData(ResultCodeActivity.DIAMOND_INSUFFICIENT);
        role.send(msgObj);
        return;
    }


    if (todayBuyCnt >= vipCfg.arenaBuyNum)
    {
        msgObj.setResponseData(ResultCodeActivity.BUY_CHANCE_MAX_COUNT);
        role.send(msgObj);
        return;
    }

    //扣钻石
    itemsPart.costItem(enItemId.DIAMOND,arenaBuyCfg.price);



    //添加次数和设置时间
    actPart.setNumber(enActProp.arenaBuyCntTime, dateUtil.getTimestamp());
    actPart.setNumber(enActProp.arenaBuyCnt, todayBuyCnt + 1);

    msgObj.setResponseData(ResultCode.SUCCESS);
    role.send(msgObj);

}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ARENA_BUY_CHANCE, buyArenaChance);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ReqArenaLogVo} reqObj
 */
function reqArenaLog(session, role, msgObj, reqObj) {
    var actPart = role.getActivityPart();
    var logs = actPart.getArenaLog();
    var bodyObj = new activityMessage.ReqArenaLogResultVo();

    if (logs.length > 0 && logs[0].time === reqObj.lastTime)
    {
        bodyObj.clientNew = true;
        bodyObj.logs = [];
    }
    else
    {
        bodyObj.clientNew = false;
        bodyObj.logs = logs;
    }

    msgObj.setResponseData(ResultCode.SUCCESS, bodyObj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_ARENA_COMBAT_LOG, reqArenaLog, activityMessage.ReqArenaLogVo);