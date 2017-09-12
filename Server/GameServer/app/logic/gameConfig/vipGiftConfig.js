
"use strict";

var gameConfig = require("./gameConfig");

class VipGiftConfig
{
    constructor() {
        this.level = 0;
        this.vipGiftValue = 0;
        this.vipGiftDiamondCost = 0;
        this.vipGiftItemId = "";
        this.vipGiftItemNum = "";

    }

    static fieldsDesc() {
        return {
            level: {type: Number},
            vipGiftValue: {type: Number},
            vipGiftDiamondCost: {type: Number},
            vipGiftItemId: {type: String},
            vipGiftItemNum: {type: String},
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

        var itemIds = [];
        if(row.vipGiftItemId!="")
        {
            var itemIdStr = row.vipGiftItemId.split("|");
            for(var k=0;k<itemIdStr.length;k++)
            {
                itemIds.push(parseInt(itemIdStr[k])) ;
            }
        }
        row.vipGiftItemId = itemIds;

        var itemNums = [];
        if(row.vipGiftItemNum!="")
        {
            var itemNumStr = row.vipGiftItemNum.split("|");
            for(var k=0;k<itemNumStr.length;k++)
            {
                itemNums.push(parseInt(itemNumStr[k])) ;
            }
        }
        row.vipGiftItemNum = itemNums;
    }
}

function getVipGiftConfig(key)
{
    return gameConfig.getCsvConfig("vipGift", key);
}


exports.VipGiftConfig = VipGiftConfig;
exports.getVipGiftConfig = getVipGiftConfig;
