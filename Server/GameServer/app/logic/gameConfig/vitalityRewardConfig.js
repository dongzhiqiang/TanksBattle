"use strict";

var gameConfig = require("./gameConfig");


class VitalityRewardConfig
{
    constructor() {
        this.id=0;
        this.boxNum=0;
        this.vitality=0;
        this.itemId="";
        this.itemNum="";
        this.level=0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            boxNum: {type: Number},
            vitality: {type: Number},
            itemId: {type: String},
            itemNum: {type: String},
            level: {type: Number},
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
        if(row.itemId!="")
        {
            var itemIdStr = row.itemId.split("|");
            for(var k=0;k<itemIdStr.length;k++)
            {
                itemIds.push(parseInt(itemIdStr[k])) ;
            }
        }
        row.itemId = itemIds;

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
    }
}

function getVitalityRewardConfig(key)
{
    return gameConfig.getCsvConfig("vitalityReward", key);
}


exports.VitalityRewardConfig = VitalityRewardConfig;
exports.getVitalityRewardConfig=getVitalityRewardConfig;
