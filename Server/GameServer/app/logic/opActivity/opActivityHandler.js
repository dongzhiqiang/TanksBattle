"use strict";

var appUtil = require("../../libs/appUtil");
var logUtil = require("../../libs/logUtil");
var dateUtil = require("../../libs/dateUtil");
var handlerMgr = require("../session/handlerMgr");
var enOpActProp = require("../enumType/opActivityPropDefine").enOpActProp;
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var CmdIdsOpActivity = require("../netMessage/opActivityMessage").CmdIdsOpActivity;
var ResultCodeOpActivity = require("../netMessage/opActivityMessage").ResultCodeOpActivity;
var opActivityMessage = require("../netMessage/opActivityMessage");
var lotteryConfig = require("../gameConfig/lotteryConfig");
var roleConfig = require("../gameConfig/roleConfig");

/**
 * 签到领奖
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @return {CheckInRes|Number}
 */
function onCheckIn(session, role, msgObj) {
    var opActivityPart = role.getOpActivityPart();
    var curTime = dateUtil.getTimestamp();
    var lastCheckInTime=opActivityPart.getNumber(enOpActProp.lastCheckIn);

    if(dateUtil.isToday(lastCheckInTime))
    {
        return ResultCodeOpActivity.CHECK_IN_FAILED;
    }
    else
    {
        if(!dateUtil.isThisMonth(lastCheckInTime))
        {
            opActivityPart.startBatch();
            opActivityPart.setNumber(enOpActProp.checkInNums,1);
            opActivityPart.setNumber(enOpActProp.lastCheckIn,curTime);
            opActivityPart.endBatch();
        }
        else
        {
            opActivityPart.startBatch();
            opActivityPart.addNumber(enOpActProp.checkInNums,1);
            opActivityPart.setNumber(enOpActProp.lastCheckIn,curTime);
            opActivityPart.endBatch();
        }
        return new opActivityMessage.CheckInRes(opActivityPart.addCheckInReward());

    }

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_OPACTIVITY, CmdIdsOpActivity.CMD_CHECK_IN, onCheckIn, null, true);


/**
 * 领取等级礼包
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 *  @param {LvRewardReq} reqObj
 * @return {LvRewardRes|Number}
 */
function getLevelReward(session, role, msgObj,reqObj) {

    var levelId = reqObj.levelId;
    var opActivityPart=role.getOpActivityPart();
    if(opActivityPart.getLevelReward(levelId))
        return new opActivityMessage.LvRewardRes(levelId);
    else
        return ResultCodeOpActivity.LEVEL_REWARD_FAILED;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_OPACTIVITY, CmdIdsOpActivity.CMD_LEVEL_REWARD, getLevelReward, opActivityMessage.LvRewardReq, true);

/**
 * 领取vip礼包
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 *  @param {VipGiftReq} reqObj
 * @return {VipGiftRes|Number}
 */
function getVipGift(session, role, msgObj,reqObj) {

    var vipLv = reqObj.vipLv;
    var opActivityPart = role.getOpActivityPart();
    if(opActivityPart.getVipGift(vipLv))
        return new opActivityMessage.VipGiftRes(vipLv);
    else
        return ResultCodeOpActivity.VIP_GIFT_FAILED;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_OPACTIVITY, CmdIdsOpActivity.CMD_VIP_GIFT, getVipGift, opActivityMessage.VipGiftReq, true);

/**
 * 请求购买宝藏
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {DrawLotteryReq} reqObj
 * @return {DrawLotteryRes|Number}
 */
function onDrawLottery(session, role, msgObj, reqObj) {
    if ((reqObj.type !== 1 && reqObj.type !== 2) || (reqObj.subType !== 1 && reqObj.subType !== 2))
        return ResultCode.BAD_PARAMETER;

    var basicCfg = lotteryConfig.getLotteryBasicCfg(reqObj.type);
    var itemPart = role.getItemsPart();
    var opActPart = role.getOpActivityPart();
    var curTime = dateUtil.getTimestamp();

    //要扣的物品和数据
    var costItemId = 0;
    var costItemNum = 0;
    //购买类型，1是免费一次，2是花费常规道具1次，3是花费常规道具10次，4是花费券道具10次
    var buyType = 0;

    //检测条件
    switch (reqObj.subType)
    {
        case 1:
            {
                let lastBuyFree = opActPart.getNumber(reqObj.type === 1 ? enOpActProp.advLtyLastBuyFree : enOpActProp.topLtyLastBuyFree);
                let buyFreeCnt = opActPart.getNumber(reqObj.type === 1 ? enOpActProp.advLtyBuyFreeCnt : enOpActProp.topLtyBuyFreeCnt);
                let freeBuyCnt = basicCfg.freeBuyCnt;
                let freeBuyCD = basicCfg.freeBuyCD;
                //如果过了CD时间，那就是买了0次
                if (curTime - lastBuyFree > freeBuyCD)
                    buyFreeCnt = 0;
                //超过了免费次数？那就判断道具是否够
                if (buyFreeCnt >= freeBuyCnt)
                {
                    let buyOneItemId = basicCfg.buyOneWithItemCost[0];
                    let buyOneCostNum = basicCfg.buyOneWithItemCost[1];
                    if (itemPart.getItemNum(buyOneItemId) < buyOneCostNum)
                        return ResultCodeOpActivity.LACK_NEED_ITEM;
                    else
                    {
                        costItemId = buyOneItemId;
                        costItemNum = buyOneCostNum;
                        buyType = 2;
                    }
                }
                else
                {
                    //免费的，都0
                    costItemId = 0;
                    costItemNum = 0;
                    buyType = 1;
                }
            }
            break;
        case 2:
            {
                var ticketItemId = basicCfg.buyTenWithTicketCost[0];
                var ticketCostNum = basicCfg.buyTenWithTicketCost[1];
                var ticketItemNum = itemPart.getItemNum(ticketItemId);
                //优先道具不够？看常规道具
                if (ticketItemNum < ticketCostNum)
                {
                    var buyTenItemId = basicCfg.buyTenWithItemCost[0];
                    var buyTenCostNum = basicCfg.buyTenWithItemCost[1];
                    if (itemPart.getItemNum(buyTenItemId) < buyTenCostNum)
                        return ResultCodeOpActivity.LACK_NEED_ITEM;
                    else
                    {
                        costItemId = buyTenItemId;
                        costItemNum = buyTenCostNum;
                        buyType = 3;
                    }
                }
                else
                {
                    costItemId = ticketItemId;
                    costItemNum = ticketCostNum;
                    buyType = 4;
                }
            }
            break;
    }

    //扣物品吧
    if (costItemId != 0 && costItemNum != 0)
    {
        if (!itemPart.costItem(costItemId, costItemNum))
        {
            logUtil.error("检测物品够，扣除物品又失败了");
        }
    }

    //修改属性值
    opActPart.startBatch();
    //买1次的计数，包括免费的
    if (buyType == 1 || buyType == 2)
    {
        //免费的计数
        if (buyType == 1)
        {
            var buyCountPropName = reqObj.type === 1 ? enOpActProp.advLtyBuyFreeCnt : enOpActProp.topLtyBuyFreeCnt;
            var buyTimePropName = reqObj.type === 1 ? enOpActProp.advLtyLastBuyFree : enOpActProp.topLtyLastBuyFree;
            //如果上次免费购买时间超过CD，就置次数为1，否则加1
            if (curTime - opActPart.getNumber(buyTimePropName) > basicCfg.freeBuyCD)
                opActPart.setNumber(buyCountPropName, 1);
            else
                opActPart.addNumber(buyCountPropName, 1);
            opActPart.setNumber(buyTimePropName, curTime);
        }

        if (reqObj.type == 1)
        {
            opActPart.addNumber(enOpActProp.advLtyBuy1Cnt, 1);
            opActPart.setNumber(enOpActProp.advLtyLastBuy1, curTime);
        }
        else
        {
            opActPart.addNumber(enOpActProp.topLtyBuy1Cnt, 1);
            opActPart.setNumber(enOpActProp.topLtyLastBuy1, curTime);
        }
    }
    //买10次的计数，包括使用优先道具的
    else
    {
        if (reqObj.type == 1)
        {
            opActPart.addNumber(enOpActProp.advLtyBuy10Cnt, 1);
            opActPart.setNumber(enOpActProp.advLtyLastBuy10, curTime);
        }
        else
        {
            opActPart.addNumber(enOpActProp.topLtyBuy10Cnt, 1);
            opActPart.setNumber(enOpActProp.topLtyLastBuy10, curTime);
        }
    }
    opActPart.endBatch();

    //给必得道具
    var mustGetItem = buyType === 1 || buyType === 2 ? basicCfg.buyOneGet : basicCfg.buyTenGet;
    for (let i = 0; i < mustGetItem.length; ++i)
    {
        var elem = mustGetItem[i];
        var itemId = elem[0];
        var itemCnt = elem[1];
        itemPart.addItem(itemId, itemCnt);
    }

    //计数相关
    //总购买次数
    var totalDrawCount = opActPart.getNumber(enOpActProp.advLtyBuy1Cnt) + opActPart.getNumber(enOpActProp.topLtyBuy1Cnt) + opActPart.getNumber(enOpActProp.advLtyBuy10Cnt) + opActPart.getNumber(enOpActProp.topLtyBuy10Cnt);
    //各个池最近购买在第几次
    var counterObj = {};
    var counterJson = opActPart.getString(enOpActProp.ltyPoolCounter);
    if (counterJson)
        counterObj = appUtil.parseJsonObj(counterJson) || {};

    //计算随机库
    var randIds = [];
    if (buyType == 1 || buyType == 2)
    {
        let firstN = basicCfg.buyOneWithItemFirstNGift.length;
        let buyCnt = opActPart.getNumber(reqObj.type === 1 ? enOpActProp.advLtyBuy1Cnt : enOpActProp.topLtyBuy1Cnt);
        let giftPool;
        if (buyCnt > firstN)
            giftPool = buyType == 1 ? basicCfg.freeBuyOneGift : basicCfg.buyOneWithItemGift;
        else
            giftPool = basicCfg.buyOneWithItemFirstNGift[buyCnt - 1];
        let randCfgList = lotteryConfig.getLotteryRandByPoolId(giftPool[0]);
        let wantRandCnt = giftPool[1];
        //构造权重列表
        let rndIdWeightList = [];
        for (let i = 0; i < randCfgList.length; ++i)
        {
            let temp = randCfgList[i];
            let randId = temp.randId;
            //基本权重 + 附加权重 * (总购买次数 - 1 - 上次本池被选中所属次数)
            //这里减一是为了排除本次
            let weight = temp.basicWeight + temp.addedWeight * (totalDrawCount - 1 - (counterObj[temp.randPoolId] || 0));
            rndIdWeightList.push({randId:randId, weight:weight});
        }
        //随机抽取N个，追加到结果列表
        randIds = randIds.concat(appUtil.getRepeatableRandItems(rndIdWeightList, wantRandCnt, 'weight', 'randId'));
    }
    else
    {
        let firstN = basicCfg.buyTenWithItemFirstNGift.length;
        let buyCnt = opActPart.getNumber(reqObj.type === 1 ? enOpActProp.advLtyBuy10Cnt : enOpActProp.topLtyBuy10Cnt);
        let giftPools;
        if (buyCnt > firstN)
            giftPools = buyType == 3 ? basicCfg.buyTenWithItemGift : basicCfg.buyTenWithTicketGift;
        else
            giftPools = basicCfg.buyTenWithItemFirstNGift[buyCnt - 1];

        for (let i = 0; i < giftPools.length; ++i)
        {
            let giftPool = giftPools[i];
            let randCfgList = lotteryConfig.getLotteryRandByPoolId(giftPool[0]);
            let wantRandCnt = giftPool[1];
            //构造权重列表
            let rndIdWeightList = [];
            for (let i = 0; i < randCfgList.length; ++i)
            {
                let temp = randCfgList[i];
                let randId = temp.randId;
                //基本权重 + 附加权重 * (总购买次数 - 1 - 上次本池被选中所属次数)
                //这里减一是为了排除本次
                let weight = temp.basicWeight + temp.addedWeight * (totalDrawCount - 1 - (counterObj[temp.randPoolId] || 0));
                rndIdWeightList.push({randId:randId, weight:weight});
            }
            //随机抽取N个，追加到结果列表
            randIds = randIds.concat(appUtil.getRepeatableRandItems(rndIdWeightList, wantRandCnt, 'weight', 'randId'));
        }
    }

    //统计道具和宠物ID
    var pieceRandIds = [];
    var giftItems = {};
    var poolIdList = []; //找出哪些Pool被抽中了附加权重>0的项
    for (let i = 0; i < randIds.length; ++i)
    {
        var randId = randIds[i];
        var randCfg = lotteryConfig.getLotteryRandPoolCfg(randId);
        //附加权重大于0？把池ID加入列表，要计数
        if (randCfg.addedWeight > 0)
            poolIdList.pushIfNotExist(randCfg.randPoolId);
        if (randCfg.objectType === 1)
        {
            giftItems[randCfg.objectId] = (giftItems[randCfg.objectId] || 0) + randCfg.count;
        }
        else if (randCfg.objectType === 2)
        {

            pieceRandIds.pushIfNotExist(randCfg.randId);
            let roleCfg = roleConfig.getRoleConfig(randCfg.objectId);
            giftItems[roleCfg.pieceItemId] = (giftItems[roleCfg.pieceItemId] || 0) + roleCfg.pieceNumReturn;
        }
    }

    //保存抽中所属次数数据
    if (poolIdList.length > 0)
    {
        for (let i = 0; i < poolIdList.length; ++i)
        {
            let poolId = poolIdList[i];
            counterObj[poolId] = totalDrawCount;
        }
        //把json当字符串保存
        counterJson = JSON.stringify(counterObj);
        opActPart.setString(enOpActProp.ltyPoolCounter, counterJson);
    }

    //给与道具和宠物
    itemPart.addItems(giftItems);

    /**
     * @type {DrawLotteryRes}
     */
    var ret = new opActivityMessage.DrawLotteryRes();
    ret.type = reqObj.type;
    ret.subType = reqObj.subType;
    ret.randIds = randIds;
    ret.pieceRandIds = pieceRandIds;
    return ret;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_OPACTIVITY, CmdIdsOpActivity.CMD_DRAW_LOTTERY, onDrawLottery, opActivityMessage.DrawLotteryReq, true);