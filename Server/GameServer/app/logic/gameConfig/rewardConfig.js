"use strict";

var gameConfig = require("./gameConfig");
var logUtil = require("../../libs/logUtil");

class RewardConfig
{
    constructor() {
        this.id = 0;
        this.rewards1 = [];
        this.rewards2 = [];
        this.rewards3 = [];
        this.rewards4 = [];
        this.rewards5 = [];
        this.rewards6 = [];
        this.rewardsDefinite = [];
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            rewards1: {type: String},
            rewards2: {type: String},
            rewards3: {type: String},
            rewards4: {type: String},
            rewards5: {type: String},
            rewards6: {type: String},
            rewardsDefinite: {type: String},
        };
    }

    /**
     * 使用默认读取方式读完一行数据后可以执行对行对象的再次处理
     * 如果使用自定义读取方式，直接在那个自定义读取方式里处理就行了，不用这个函数了
     * 这个函数可选，没有就不执行
     * @param {object} row - 行数据对象
     */
    static afterDefReadRow(row)
    {
        var rewards = [];
        for(var k=1;k<=6;k++)
        {
            var rewardStr = row["rewards"+k];

            if(rewardStr && rewardStr.length>0)
            {
                let rewardAry = rewardStr.split(",");
                let rewardGroup = [];
                for(let i=0; i<rewardAry.length; i++)
                {
                    if(rewardAry[i] && rewardAry[i].length>0) {
                        let rewardStrs = rewardAry[i].split("|");
                        let rewardItem = {
                            itemId: parseInt(rewardStrs[0]),
                            itemNum: parseInt(rewardStrs[1]),
                            rate: parseInt(rewardStrs[2])
                        };
                        rewardGroup.push(rewardItem);
                    }
                }
                rewards.push(rewardGroup)
            }
        }
        var rewardShow = row.rewardsDefinite;

        if(rewardShow && rewardShow.length>0)
        {
            let rewardAry = rewardShow.split(",");

            for(let i=0; i<rewardAry.length; i++)
            {
                let rewardGroup = [];
                if(rewardAry[i] && rewardAry[i].length>0) {

                    let rewardStrs = rewardAry[i].split("|");
                    let rewardItem = {
                        itemId: parseInt(rewardStrs[0]),
                        itemNum: parseInt(rewardStrs[1]),
                        rate: 10000
                    };
                    rewardGroup.push(rewardItem);
                }
                rewards.push(rewardGroup)
            }

        }


        row.rewards = rewards;
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {RewardConfig}
 */
function getRewardConfig(key)
{
    return gameConfig.getCsvConfig("reward", key);
}
//返回对象
function getRandomReward(dropId)
{
    var rewardItems = {};

    var rewards = getRewardConfig(dropId).rewards;
    if (rewards == null) {
        logUtil.error("没有找到掉落组 "+dropId);
        return null;
    }

    for(var i = 0, leni = rewards.length; i < leni; ++i)
    {
        var rewardGroup = rewards[i];
        var randNum = Math.random() * 10000;
        for(var j = 0, lenj = rewardGroup.length; j < lenj; ++j)
        {
            var rewardItem = rewardGroup[j];
            if(randNum < rewardItem.rate)
            {
                rewardItems[rewardItem.itemId] = (rewardItems[rewardItem.itemId] || 0) + rewardItem.itemNum;
                break;
            }
            randNum -= rewardItem.rate;
        }
    }

    return rewardItems;
}
//返回数组 数组里存物品 id num
function getRandomReward2(dropId)
{
    var rewardItemList =[];

    var rewards = getRewardConfig(dropId).rewards;
    if (rewards == null) {
        logUtil.error("没有找到掉落组 "+dropId);
        return null;
    }

    for(var i = 0, leni = rewards.length; i < leni; ++i)
    {
        var rewardGroup = rewards[i];
        var randNum = Math.random() * 10000;

        for(var j = 0, lenj = rewardGroup.length; j < lenj; ++j)
        {
            var rewardItem = rewardGroup[j];
            if(randNum < rewardItem.rate)
            {
                var item = {};
                item.itemId = rewardItem.itemId;
                item.num = rewardItem.itemNum;
                rewardItemList.push(item);
                break;
            }
            randNum -= rewardItem.rate;
        }
    }

    return rewardItemList;
}

exports.RewardConfig = RewardConfig;
exports.getRewardConfig = getRewardConfig;
exports.getRandomReward = getRandomReward;
exports.getRandomReward2 = getRandomReward2;