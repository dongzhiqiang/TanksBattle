"use strict";

var gameConfig = require("./gameConfig");

class LotteryBasicCfg
{
    constructor() {
        this.typeId = 0;
        this.typeName = "";
        this.tipText = "";
        this.previewTabs = [];
        this.previewPools = [];
        this.chipItemId = 0;
        this.freeBuyCnt = 0;
        this.freeBuyCD = 0;
        this.buyOneWithItemCost = [];
        this.buyTenWithItemCost = [];
        this.buyTenWithTicketCost = [];
        this.buyOneGet = [];
        this.buyTenGet = [];
        this.freeBuyOneGift = [];
        this.buyOneWithItemGift = [];
        this.buyTenWithItemGift = [];
        this.buyTenWithTicketGift = [];
        this.buyOneWithItemFirstNGift = [];
        this.buyTenWithItemFirstNGift = [];
    }

    static fieldsDesc() {
        return {
            typeId: {type: Number},
            typeName: {type: String},
            tipText: {type: String},
            previewTabs: {type: Array, elemType:String},
            previewPools: {type: Array, elemType:Number},
            chipItemId: {type: Number},
            freeBuyCnt: {type: Number},
            freeBuyCD: {type: Number},
            buyOneWithItemCost: {type: Array, elemType:Number},
            buyTenWithItemCost: {type: Array, elemType:Number},
            buyTenWithTicketCost: {type: Array, elemType:Number},
            buyOneGet: {type: Array, elemType:Number, arrayLayer:2},
            buyTenGet: {type: Array, elemType:Number, arrayLayer:2},
            freeBuyOneGift: {type: Array, elemType:Number},
            buyOneWithItemGift: {type: Array, elemType:Number},
            buyTenWithItemGift: {type: Array, elemType:Number, arrayLayer:2},
            buyTenWithTicketGift: {type: Array, elemType:Number, arrayLayer:2},
            buyOneWithItemFirstNGift: {type: Array, elemType:Number, arrayLayer:2},
            buyTenWithItemFirstNGift: {type: Array, elemType:Number, arrayLayer:3},
        };
    }
}

class LotteryRandPool
{
    constructor() {
        this.randId = 0;
        this.randPoolId = 0;
        this.objectType = 0;
        this.objectId = "";
        this.count = 0;
        this.basicWeight = 0;
        this.addedWeight = 0;
        this.turnType = 0;
        this.broadcast = 0;
    }

    static fieldsDesc() {
        return {
            randId: {type: Number},
            randPoolId: {type: Number},
            objectType: {type: Number},
            objectId: {type: String},
            count: {type: Number},
            basicWeight: {type: Number},
            addedWeight: {type: Number},
            turnType: {type: Number},
            broadcast: {type: Number},
        };
    }

    /**
     * 读完全部数据行后，做后续处理
     * @param {object[]|object.<(string|number), object>} rows
     */
    static afterReadAll(rows)
    {
        var poolIdItemsMap = {};
        for(var k in rows)
        {
            var item = rows[k];
            var pool = poolIdItemsMap[item.randPoolId];
            if (!pool)
                poolIdItemsMap[item.randPoolId] = pool = [];
            pool.push(item);
        }

        LotteryRandPool.poolIdItemsMap = poolIdItemsMap;
    }
}

/**
 * @param {number} type - 类型
 * @returns {LotteryBasicCfg}
 */
function getLotteryBasicCfg(type)
{
    return gameConfig.getCsvConfig("lotteryBasic", type);
}

/**
 * @param {number} randId - 随机项ID
 * @returns {LotteryRandPool}
 */
function getLotteryRandPoolCfg(randId)
{
    return gameConfig.getCsvConfig("lotteryRandPool", randId);
}

/**
 * @param {number} randPoolId - 随机库ID
 * @returns {LotteryRandPool[]}
 */
function getLotteryRandByPoolId(randPoolId)
{
    return LotteryRandPool.poolIdItemsMap[randPoolId];
}

exports.LotteryBasicCfg = LotteryBasicCfg;
exports.getLotteryBasicCfg = getLotteryBasicCfg;

exports.LotteryRandPool = LotteryRandPool;
exports.getLotteryRandPoolCfg = getLotteryRandPoolCfg;
exports.getLotteryRandByPoolId = getLotteryRandByPoolId;