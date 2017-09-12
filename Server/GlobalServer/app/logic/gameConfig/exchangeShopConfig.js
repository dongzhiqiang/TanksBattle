"use strict";

var gameConfig = require("./gameConfig");


class ExchangeShopConfig
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.refreshTime = 0;
        this.diamondCost = "";
        this.moneyId = 0;
        this.groupId = "";
        this.itemNum = "";
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            refreshTime: {type: Number},
            diamondCost: {type: String},
            moneyId: {type: Number},
            groupId: {type: String},
            itemNum: {type: String},
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
        var groupIds = [];

        if(row.groupId!="")
        {
            var groupIdStr = row.groupId.split("|");
            for(var k=0;k<groupIdStr.length;k++)
            {
                groupIds.push(parseInt(groupIdStr[k]));
            }
        }
        row.groupId = groupIds;

        var itemNums = [];
        if(row.itemNum!="")
        {
            var itemNumStr = row.itemNum.split("|");
            for(var k=0;k<itemNumStr.length;k++)
            {
                itemNums.push(parseInt(itemNumStr[k])) ;
            }
        }
        row.itemNum = itemNums;

        var diamondCosts = [];
        if(row.diamondCost!="")
        {
            var diamondCostStr = row.diamondCost.split("|");
            for(var k=0;k<diamondCostStr.length;k++)
            {
                diamondCosts.push(parseInt(diamondCostStr[k])) ;
            }
        }
        row.diamondCost = diamondCosts;
    }
}

function getExchangeShopConfig(key)
{
    return gameConfig.getCsvConfig("exchangeShop", key);
}

function getExchangeShopConfigLength()
{
    let a = 0;
    for(var key in gameConfig.getCsvConfig("exchangeShop"))
    {
        a++;
    }
    return a;
}


exports.ExchangeShopConfig = ExchangeShopConfig;
exports.getExchangeShopConfig = getExchangeShopConfig;
exports.getExchangeShopConfigLength = getExchangeShopConfigLength;
