"use strict";

var gameConfig = require("./gameConfig");


class GrowthTaskConfig
{
    constructor() {
        this.id = 0;
        this.param = "";
        this.itemId = "";
        this.itemNum = "";
        this.type = "";
        this.prop="";




    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            param: {type: String},
            itemId: {type: String},
            itemNum: {type: String},
            type: {type: String},
            prop: {type: String},

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
        var params = [];
        if(row.param!="")
        {
            var paramStr = row.param.split("|");
            for(var k=0;k<paramStr.length;k++)
            {
                params.push(parseInt(paramStr[k])) ;
            }
        }
        row.param = params;


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

function getGrowthTaskConfig(key)
{
    return gameConfig.getCsvConfig("growthTask", key);
}


exports.GrowthTaskConfig = GrowthTaskConfig;
exports.getGrowthTaskConfig=getGrowthTaskConfig;
