"use strict";

var gameConfig = require("./gameConfig");
var cronTime = require("../../libs/cronTime");
var enSysActiveType = require("../enumType/systemDefine").enSysActiveType;
var enSysOpenType = require("../enumType/systemDefine").enSysOpenType;

class SystemConfig
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.visibility = {};
        this.activeCond = [];
        this.openCond = [];
        this.resetTime = "";
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            visibility: {type: String},
            activeCond: {type: String},
            openCond: {type: String},
            resetTime: {type: String}
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
        var visibility = {};
        var visStrs = row.visibility.split("|");
        visibility.type = parseInt(visStrs[0]);
        if(visStrs.length>1)
        {
            visibility.param = parseInt(visStrs[1]);
        }
        row.visibility = visibility;
        var activeStrs = row.activeCond.length>0 ? row.activeCond.split(",") : [];
        var activeCond = [];
        var cond;
        for(var k=0;k<activeStrs.length;k++)
        {
            if(activeStrs[k]!="")
            {
                var activeOneStrs = activeStrs[k].split("|");
                cond = {};
                cond.type = parseInt(activeOneStrs[0]);
                switch(cond.type)
                {
                    case enSysActiveType.ACTIVE:
                        break;
                    case enSysActiveType.LEVEL:
                        cond.param = parseInt(activeOneStrs[1]);
                        break;
                    default:
                        if(activeOneStrs.length>1)
                        {
                            cond.param = activeOneStrs[1];
                        }
                }
                activeCond.push(cond);
            }

        }
        row.activeCond = activeCond;
        var openStrs = row.openCond.length>0 ? row.openCond.split(",") : [];
        var openCond = [];
        for(var k=0;k<openStrs.length;k++)
        {
            if(openStrs[k]!="")
            {
                var openOneStrs = openStrs[k].split("|");
                cond = {};
                cond.type = parseInt(openOneStrs[0]);
                switch(cond.type)
                {
                    case enSysOpenType.TIME:
                        cond.param = [];
                        for(var i=1;i<openOneStrs.length;i++) {
                            if(!openOneStrs[i])
                            {
                                continue;
                            }
                            var timeObj = {}
                            var timeStrs = openOneStrs[i].split("-");
                            timeObj.from = cronTime.getCronTime(timeStrs[0]);
                            timeObj.to = cronTime.getCronTime(timeStrs[1]);
                            cond.param.push(timeObj);
                        }
                        break;
                    case enSysOpenType.LEVEL:
                    case enSysOpenType.SERVER_TIME:
                    case enSysOpenType.ACCOUNT_TIME:
                        cond.param = parseInt(openOneStrs[1]);
                        break;
                    default:
                        if(openOneStrs.length>1)
                        {
                            cond.param = openOneStrs[1];
                        }
                }
                openCond.push(cond);
            }

        }
        row.openCond = openCond;
        if(row.resetTime)
            row.resetTime = cronTime.getCronTime(row.resetTime);
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {SystemConfig}
 */
function getSystemConfig(key)
{
    return gameConfig.getCsvConfig("system", key);
}

exports.SystemConfig = SystemConfig;
exports.getSystemConfig = getSystemConfig;