"use strict";

var appCfg = require("../../../config");
var appUtil = require("../../libs/appUtil");
var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var handlerMgr = require("../network/handlerMgr");
var playerMgr = require("./playerMgr");
var Message = require("../../libs/message").Message;
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var accountMsg = require("../netMessage/accountMessage");
var CmdIdsAccount = require("../netMessage/accountMessage").CmdIdsAccount;
var ResultCodeAccount = require("../netMessage/accountMessage").ResultCodeAccount;

//////////////////////////////////本模块内的变量/////////////////////////////////////
var timeProfiler = {};
var timeCostSum = 0;
var timeCostMax = appUtil.INT32_MAX_NEGTIVE;
var timeCostMin = appUtil.INT32_MAX_POSITIVE;
var timeCostCnt = 0;

//////////////////////////////////辅助函数/////////////////////////////////////
function printTimeProfile(player, module, command)
{
    var username = player.getUsername();
    var subData = timeProfiler[username];
    if (subData)
    {
        var subKey = module + "_" + command;
        var startTime = subData[subKey];
        if (startTime)
        {
            delete subData[subKey];
            var curTime = dateUtil.getTimestampMS();
            var costTime = curTime - startTime;
            logUtil.info("Time Profile, username: " + username + ", module: " + module + ", command: " + command + ", reply Cost Time: " + costTime + "ms");

            timeCostSum += costTime;
            timeCostCnt += 1;
            if (costTime > timeCostMax)
                timeCostMax = costTime;
            if (costTime < timeCostMin)
                timeCostMin = costTime;
            if (timeCostCnt >= appCfg.createPlayerNum)
            {
                logUtil.info("Time Profile Summary, ref count: " + timeCostCnt + ", max time:" + timeCostMax + ", min time:" + timeCostMin + ", average time: " + (timeCostSum / timeCostCnt) + "ms");
                timeCostSum = 0;
                timeCostCnt = 0;
            }
        }
    }
}

//////////////////////////////////接收服务端消息的处理函数/////////////////////////////////////
/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {SyncServerTime} body
 */
function onRetServerTime(player, code, errMsg, body)
{
    ////////////////
    var module = ModuleIds.MODULE_ACCOUNT;
    var command = CmdIdsAccount.CMD_SERVER_TIME;
    printTimeProfile(player, module, command);
    ////////////////

    if (code)
    {
        player.logError("onRetServerTime, code:" + code + ", errMsg:" + errMsg);
    }
    else
    {
        player.logDebug("当前真实时间：" + dateUtil.getTrueDateString());
        player.logDebug("原虚拟时间：" + dateUtil.getDateString());
        dateUtil.setTimeFromTimestamp(body.time);
        player.logDebug("新虚拟时间：" + dateUtil.getDateString());
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_SERVER_TIME, onRetServerTime, accountMsg.SyncServerTime);

//////////////////////////////////发送消息测试函数/////////////////////////////////////
/**
 * 获取虚拟时间
 */
function requestServerTime()
{
    var msg = Message.newRequest(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_SERVER_TIME, null);
    playerMgr.sendToAllPlayerOnTimer(msg, -1, false, timeProfiler);
}

//////////////////////////////////测试入口函数/////////////////////////////////////
function doTestNow()
{
    //requestServerTime();
}

//////////////////////////////////导出接口/////////////////////////////////////
exports.doTestNow = doTestNow;