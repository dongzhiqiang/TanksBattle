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
var enSystemId = require("../../enumType/systemDefine").enSystemId;
var systemMgr = require("../../system/systemMgr");

function getRankInfo(role, minRate, maxRate)
{
    var result = {};
    var rankData = rankMgr.getRankData(rankTypes.realPower);
    var rankArr = rankData.data;
    result.rankArr = rankArr;
    var myRankVal = rankMgr.getRankValueByKey(rankTypes.realPower, role.getHeroId());
    result.myRankVal = myRankVal;
    var myRankData = rankArr[myRankVal];
    var power = myRankData.power;
    var minPower = power * minRate;
    var maxPower = power * maxRate;
    // 计算最低可用index
    var minRankVal = myRankVal;
    while(minRankVal-1>=0 && rankArr[minRankVal-1].power>=minPower)minRankVal--;
    // 计算最高可用index
    var maxRankVal = myRankVal;
    while(maxRankVal+1<rankArr.length && rankArr[maxRankVal+1].power<=maxPower)maxRankVal++;
    result.minRankVal = minRankVal;
    result.maxRankVal = maxRankVal;
    return result;
}

function selectRankItem(rankInfo, exclude)
{
    //判端元素是否足够
    var min = rankInfo.minRankVal;
    var max = rankInfo.maxRankVal;
    var elemCount = max-min+1;
    if(elemCount <= exclude.length)
    {
        min = 0;
        max = rankInfo.rankArr.length-1;
        elemCount = max-min+1;
        if(elemCount <= exclude.length)
        {
            return null;
        }
    }

    //随机index
    var index = appUtil.getRandom(min, max);

    //累加index直到不重复为止
    while(exclude.indexOf(rankInfo.rankArr[index].key)!= -1)
    {
        index ++;
        if(index > max)index = min;
    }
    return rankInfo.rankArr[index].key;
}

function getPieceItem(role)
{
    //查找未满级的神器
    var pieceIds = [];
    var treasureCfgs = treasureConfig.getTreasureConfig();
    var f;
    for(var i in treasureCfgs)
    {
        if(!f)f=i;
        var treasureCfg = treasureCfgs[i];
        var treasure = role.getTreasurePart().getTreasure(treasureCfg.id);
        if(!treasure || treasureConfig.getTreasureLevelConfig(treasureCfg.id,treasure.level+1))
        {
            pieceIds.push(treasureCfg.pieceId);
        }
    }
    var pieceId = treasureCfgs[f].pieceId;
    if(pieceIds.length>0)
    {
        pieceId = pieceIds[appUtil.getRandom(0,pieceIds.length-1)];
    }
    //随机数量
    var maxRandNum = 0;
    for(var i=0;i<treasureRobConfig.getTreasureRobPieceCfg().length;i++)
    {
        maxRandNum += treasureRobConfig.getTreasureRobPieceCfg()[i].rate;
    }
    var rand = appUtil.getRandom(0,maxRandNum-1);
    var itemNum;
    for(var i=0;i<treasureRobConfig.getTreasureRobPieceCfg().length;i++)
    {
        if(rand< treasureRobConfig.getTreasureRobPieceCfg()[i].rate)
        {
            itemNum = treasureRobConfig.getTreasureRobPieceCfg()[i].count;
            break;
        }
        rand-=treasureRobConfig.getTreasureRobPieceCfg()[i].rate;
    }
    return {"itemId":pieceId, "itemNum":itemNum};
}

//被抢
function robed(role, opHeroId, opName)
{
    //这里不需要判断系统开启，因为无碎片不会被抢
    var pieceIds = [];
    var treasureCfgs = treasureConfig.getTreasureConfig();
    for(var i in treasureCfgs)
    {
        var treasureCfg = treasureCfgs[i];
        if(role.getItemsPart().getItemNum(treasureCfg.pieceId)>0)
        {
            pieceIds.push(treasureCfg.pieceId);
        }
    }
    if(pieceIds.length==0)
    {
        return;
    }
    var pieceId = pieceIds[appUtil.getRandom(0,pieceIds.length-1)];
    var isWin = appUtil.getRandom(0,1)==0;
    if(!isWin)
    {
        role.getItemsPart().costItem(pieceId,1);
    }

    var actPart = role.getActivityPart();
    var curTime = dateUtil.getTimestamp();
    actPart.setNumber(enActProp.treasureRobedCnt, actPart.getNumber(enActProp.treasureRobedCnt)+1);
    actPart.addTreasureRobBattleLog(opHeroId, opName, pieceId, 1, false, isWin, curTime);

    var battleLogs = actPart.getTreasureRobBattleLogs();
    var netMsg = battleLogs[battleLogs.length-1];
    role.sendEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.PUSH_TREASURE_ROB_BATTLE_LOG, netMsg);
}

exports.getRankInfo = getRankInfo;
exports.selectRankItem = selectRankItem;
exports.getPieceItem = getPieceItem;
exports.robed = robed;
