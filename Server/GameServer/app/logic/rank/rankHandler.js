"use strict";

var appUtil = require("../../libs/appUtil");
var dateUtil = require("../../libs/dateUtil");
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var rankMessage = require("../netMessage/rankMessage");
var CmdIdsRank = require("../netMessage/rankMessage").CmdIdsRank;
var ResultCodeRank = require("../netMessage/rankMessage").ResultCodeRank;
var rankMgr = require("./rankMgr");
var rankConfig = require("../gameConfig/rankConfig");
var rankTypes = require("../enumType/rankDefine").rankTypes;
var enProp = require("../enumType/propDefine").enProp;

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {RequestRankDataVo} reqObj
 */
function requestRankData(session, role, msgObj, reqObj)
{
    var type = reqObj.type;
    var time = reqObj.time;
    var start = reqObj.start;
    var len   = reqObj.len;
    var getMyRank = reqObj.myRank;

    //提交检查是否要清一些东西
    rankMgr.checkRankTime(type);

    var data = rankMgr.getRankData(type);
    if (!data)
    {
        msgObj.setResponseData(ResultCodeRank.RANK_TYPE_WRONG);
        role.send(msgObj);
        return;
    }

    var netMsg = new rankMessage.RankDataVo();
    netMsg.type = type;
    netMsg.upTime = data.upTime;
    netMsg.start = start;
    netMsg.reqLen = len;
    netMsg.total = data.data.length;

    //有可能自己没排上名，这个排行的upTime不会变，但是我还是要判断自己的排行信息
    if (start === 0 && getMyRank)
    {
        var myRankData = rankMgr.getRankDataByRole(type, role);
        netMsg.myRank = myRankData.rank;
        netMsg.myData = myRankData.data == null ? "" : JSON.stringify(myRankData.data);
    }
    else
    {
        netMsg.myRank = -1;
        netMsg.myData = "";
    }

    if (data.upTime === time)
    {
        netMsg.clientNew = true;
        netMsg.data = "";
        netMsg.extra = "";
    }
    else
    {
        netMsg.clientNew = false;
        netMsg.data = JSON.stringify(data.data.slice(start, start + len));
        netMsg.extra = data.extra == null ? "" : JSON.stringify(data.extra);
    }
    msgObj.setResponseData(ResultCode.SUCCESS, netMsg);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_RANK, CmdIdsRank.CMD_REQUEST_RANK, requestRankData, rankMessage.RequestRankDataVo, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ReqMyRankValueVo} reqObj
 */
function requestMyRankValue(session, role, msgObj, reqObj)
{
    var type = reqObj.type;

    //提交检查是否要清一些东西
    rankMgr.checkRankTime(type);

    var netMsg = new rankMessage.MyRankValueVo();
    netMsg.type = type;
    netMsg.rankVal = rankMgr.getRankValueByRole(type, role);
    msgObj.setResponseData(ResultCode.SUCCESS, netMsg);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_RANK, CmdIdsRank.CMD_REQ_MY_RANK_VAL, requestMyRankValue, rankMessage.ReqMyRankValueVo, true);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {DoLikeRankItemReq} reqObj
 */
function requestDoLikeRankItem(session, role, msgObj, reqObj)
{
    var type = reqObj.type;
    var key = reqObj.key;

    //类型对不对
    if (!rankTypes[type])
        return ResultCode.BAD_PARAMETER;

    //取点赞记录
    var lastLikeTime = role.getNumber(enProp.lastRankLike);
    var rankLikeLog;
    if (dateUtil.isToday(lastLikeTime))
        rankLikeLog = appUtil.parseJsonObj(role.getString(enProp.rankLikeLog)) || {};   //{排行类型1:[点赞对象ID1,点赞对象ID2]}
    else
        rankLikeLog = {};

    //今天赞过这个人了，就不赞了
    var likeKeys = rankLikeLog[type] || [];
    if (likeKeys.existsValue(key))
        return ResultCodeRank.RANK_TYPE_DO_LIKE_FAIL;

    //本榜点赞超过次数了？
    var maxLikeCnt = rankConfig.getRankBasicConfig().likeCntLimit;
    if (likeKeys.length >= maxLikeCnt)
        return ResultCodeRank.RANK_TYPE_DO_LIKE_FAIL;

    //提交检查是否要清一些东西
    rankMgr.checkRankTime(type);

    //点赞
    var newLike = rankMgr.doLikeRankItem(type, key);
    if (newLike < 0)
        return ResultCodeRank.RANK_TYPE_DO_LIKE_FAIL;

    //加入记录
    likeKeys.push(key);
    rankLikeLog[type] = likeKeys;    //为了防止rankLikeLog里没有这个type的key

    //保存点赞记录
    role.startBatch();
    role.setNumber(enProp.lastRankLike, dateUtil.getTimestamp());
    role.setString(enProp.rankLikeLog, JSON.stringify(rankLikeLog));
    role.endBatch();

    //奖励物品
    var rewards = rankConfig.getRankBasicConfig().doLikeReward;
    role.getItemsPart().addItems(rewards);

    return new rankMessage.DoLikeRankItemRes(type, key, newLike);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_RANK, CmdIdsRank.CMD_DO_LIKE, requestDoLikeRankItem, rankMessage.DoLikeRankItemReq, true);