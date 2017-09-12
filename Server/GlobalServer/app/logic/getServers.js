"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil = require("../libs/appUtil");
var dateUtil = require("../libs/dateUtil");
var logUtil = require("../libs/logUtil");
var httpUtil = require("../libs/httpUtil");
var dbUtil = require("../libs/dbUtil");

////////////模块内变量////////////
const CACHE_LIFE    = 10;   //缓存时间，单位秒
const DOWN_REF_VAL  = 60;   //服务器的存活时间距离现在多久算挂了

/**
 * loadState有down、idle、busy、crowd
 * showState有normal、newSvr、recommend、hotSvr
 */
var serverListCache = [];
var serverListUpTime= 0;

////////////导出函数////////////
var doLogicCoroutine = Promise.coroutine(function * (req, res, body, pathName){
    var retMsg = {};
    try {
        //转换提交的内容
        //{channelId:"default",userId:"1"}
        var jsonObj = httpUtil.getJsonObjFromTEA(body);
        logUtil.debug(JSON.stringify(jsonObj));

        //检查字段和格式
        if (!jsonObj)
        {
            retMsg.code = 2;
            retMsg.msg = "数据格式不正确";
            return;
        }
        if (!Object.isString(jsonObj.channelId))
        {
            retMsg.code = 3;
            retMsg.msg = "渠道ID必须是字符串";
            return;
        }
        if (!Object.isString(jsonObj.userId))
        {
            retMsg.code = 4;
            retMsg.msg = "用户ID必须是字符串";
            return;
        }
        var channelId = jsonObj.channelId.trim();
        var userId = jsonObj.userId.trim();
        if (channelId.length < 0)
        {
            retMsg.code = 5;
            retMsg.msg = "必须提供渠道ID";
            return;
        }
        if (userId.length < 0)
        {
            retMsg.code = 6;
            retMsg.msg = "必须提供用户ID";
            return;
        }

        //获取数据库对象
        var db = dbUtil.getDB();

        //查询用户所有的角色
        var col = db.collection("roleList");
        var query = {channelId:channelId, userId:userId};
        var fields = {"name" : 1, "level" : 1, "roleId" : 1, "serverId" : 1, "heroId" : 1, "lastLogin" : 1};
        var roleList = yield col.findArray(query, fields);

        //查询服务器列表
        var curTime = dateUtil.getTrueTimestamp();
        //看看缓存有没有过期
        var serverList = [];
        if (Math.abs(curTime - serverListUpTime) > CACHE_LIFE)
        {
            logUtil.debug("从数据库读取服务器列表");

            col = db.collection("serverList");
            query = {};
            fields = {"area" : 1, "name" : 1, "index" : 1, "host" : 1, "port" : 1, "serverId" : 1, "loadState" : 1, "showState" : 1, "liveTime":1};
            serverList = yield col.findArray(query, fields);

            //整理数据
            curTime = dateUtil.getTrueTimestamp(); //由于这是异步过程，这里重新获取时间值
            for (var i = 0; i < serverList.length; ++i)
            {
                var item = serverList[i];
                if (Math.abs(curTime - item.liveTime) > DOWN_REF_VAL)
                {
                    item.loadState = "down";
                    logUtil.warn("发现服务器长时间不活跃，区名：" + item.area + "，服务器名：" + item.name + "，服务器ID：" + item.serverId);
                }
                //这字段用完就删
                delete item.liveTime;
            }

            serverListCache = serverList;
            serverListUpTime = curTime;
        }
        else
        {
            serverList = serverListCache;
        }

        //下发数据
        retMsg.code = 0;
        retMsg.msg = "OK";
        retMsg.cxt = {roleList:roleList, serverList:serverList};
    }
    catch (err) {
        retMsg.code = 1;
        retMsg.msg = "服务器异常";
    }
    finally {
        httpUtil.responseTEA(res, retMsg);
    }
});

function doLogic(req, res, body, pathName)
{
    doLogicCoroutine(req, res, body, pathName);
}

////////////全局执行////////////
appUtil.addProcessListener("refreshSvrList", function(){
    serverListCache = [];
    serverListUpTime= 0;
    logUtil.info("刷新服务器列表");
});

////////////导出元素////////////
exports.doLogic = doLogic;
exports.binaryBody = true;