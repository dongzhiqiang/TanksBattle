"use strict";

var gameConfig = require("./gameConfig");


class TaskRewardConfig
{
    constructor() {
        this.id = 0;
        this.taskName = "";
        this.taskType="";
        this.taskField="";
        this.taskProp=0;
        this.taskRewardTime="";
        this.itemId="";
        this.itemNum="";
        this.vitality=0;




    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            taskName: {type: String},
            taskType: {type: String},
            taskField: {type: String},
            taskProp: {type: Number},
            taskRewardTime: {type: String},
            itemId: {type: String},
            itemNum: {type: String},
            vitality: {type: Number},

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
        if(row.taskField!="")
        {
            var taskFieldStr = row.taskField.split("|");
            row.taskField = taskFieldStr;
        }


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

function getTaskRewardConfig(key)
{
    return gameConfig.getCsvConfig("taskReward", key);
}


exports.TaskRewardConfig = TaskRewardConfig;
exports.getTaskRewardConfig=getTaskRewardConfig;
