"use strict";

var gameConfig = require("./gameConfig");

class RankBasicConfig
{
    constructor() {
        this.likeCntLimit = 0;
        this.likeRankLimit = 0; //前多少名才可以被点赞(从1开始)
        this.doLikeReward = {}; //原本是Number[][]，后面转换成{物品ID:数量}
    }

    static fieldsDesc() {
        return {
            likeCntLimit: {type: Number},
            likeRankLimit: {type: Number},
            doLikeReward: {type: Array, elemType:Number, arrayLayer:2},
        };
    }

    /**
     * 读完一行后，执行
     * @param {RankBasicConfig} row - 行数据对象
     */
    static afterDefReadRow(row) {
        var doLikeReward = {};
        for (var i = 0, len = row.doLikeReward.length; i < len; ++i)
        {
            var arr = row.doLikeReward[i];
            var itemId = arr[0];
            var num = arr[1];
            doLikeReward[itemId] = (doLikeReward[itemId] || 0) + num;
        }
        row.doLikeReward = doLikeReward;
    }
}

class RankLikeReward
{
    constructor() {
        this.rank = 0;
        this.reward = []; //Number[][]，后面转换成Item[]
    }

    static fieldsDesc() {
        return {
            rank: {type: Number},
            reward: {type: Array, elemType:Number, arrayLayer:2},
        };
    }

    /**
     * 读完一行后，执行
     * @param {RankLikeReward} row - 行数据对象
     */
    static afterDefReadRow(row) {
        var items = [];
        for (var i = 0, len = row.reward.length; i < len; ++i)
        {
            var arr = row.reward[i];
            items.push({itemId:arr[0], num:arr[1]});
        }
        row.reward = items;
    }

    /**
     * 读完全部行后，执行
     * @param {RankLikeReward[]|object.<(string|number), RankLikeReward>} rows
     */
    static afterReadAll(rows) {
        var rowCnt = 0;
        for (var key in rows) {
            ++rowCnt;
        }
        RankLikeReward.RowsCount = rowCnt;
    }
}

/**
 *
 * @returns {RankBasicConfig}
 */
function getRankBasicConfig()
{
    return gameConfig.getCsvConfig("rankBasic")[0];
}

/**
 *
 * @param {number?} key - 排名值，1~N，这个值不填的话，就是取全部排行
 * @returns {RankLikeReward|object.<(string|number), RankLikeReward>}
 */
function getRankLikeReward(key)
{
    return gameConfig.getCsvConfig("rankLikeReward", key);
}

exports.RankBasicConfig = RankBasicConfig;
exports.RankLikeReward = RankLikeReward;
exports.getRankBasicConfig = getRankBasicConfig;
exports.getRankLikeReward = getRankLikeReward;