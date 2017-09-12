"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil     = require("../../libs/appUtil");
var logUtil     = require("../../libs/logUtil");
var dateUtil    = require("../../libs/dateUtil");
var playerClass = require("./player");

////////////模块变量////////////
/**
 *
 * @type {Player[]}
 */
var playerList = [];

////////////私有变量////////////
//没啥用，充当模块标记
function onProcessExiting() {}

////////////导出函数////////////
var createPlayerCoroutine = Promise.coroutine(function * (start, num) {
    //添加监听，保证要退出时先清除玩家再退出
    appUtil.addProcessExitingListener(onProcessExiting);
    logUtil.info("开始创建玩家...");

    for (var i = start, end = start + num; i < end; ++i)
    {
        try {
            var username = "test" + i;
            var password = "123456";
            var player = new playerClass.Player(username, password);

            yield player.loginAccount();
            yield player.loginGameServer();

            playerList.push(player);
        }
        catch (err) {
            logUtil.error(err.message);
        }
    }

    logUtil.info("完成创建玩家");
});

/**
 * 创建玩家对象
 * @param {number} start - 开始用户名前缀
 * @param {number} num - 创建数量
 */
function createPlayer(start, num)
{
    return createPlayerCoroutine(start, num);
}

var clearPlayerCoroutine = Promise.coroutine(function * () {
    logUtil.info("开始清理玩家...");
    for (var i = 0; i < playerList.length; ++i)
    {
        try {
            var player = playerList[i];
            yield player.logoutGameServer();
            yield player.logoutAccount();
        }
        catch (err) {
            logUtil.error(err.message);
        }
    }
    playerList = [];
    logUtil.info("完成清理玩家");
    //删除监听，本模块清理完毕
    appUtil.removeProcessExitingListener(onProcessExiting);
});

function clearPlayer()
{
    return clearPlayerCoroutine();
}

/**
 *
 * @param {Message} msg
 * @param {number?} [playerCount=-1] - 发送给多少用户，-1表示全部发送
 * @param {boolean} [allowNotLoginFull=false] - 没完全登录的player也可以发送，默认不是
 * @param {object.<string, object.<string, number>>?} [timeProfiler=undefined] - 可选定，会写入每个用户的当前消息的开始发送时间
 */
function sendToAllPlayer(msg, playerCount, allowNotLoginFull, timeProfiler)
{
    if (!msg)
        return;

    for (var i = 0; i < playerList.length; ++i) {
        if (playerCount >= 0 && i >= playerCount)
            break;

        try {
            var player = playerList[i];
            if (allowNotLoginFull || player.getRoleInfo())
            {
                if (timeProfiler) {
                    var username = player.getUsername();
                    var subData = timeProfiler[username];
                    if (!subData)
                        timeProfiler[username] = subData = {};
                    var module = msg.getModule();
                    var command = msg.getCommand();
                    subData[module + "_" + command] = dateUtil.getTimestampMS();
                }
                player.send(msg);
            }
        }
        catch (err) {
            logUtil.error(err.message);
        }
    }
}

/**
 * 用定时器发送所有数据给用户（避免发送时阻塞接收）
 * @param {Message} msg
 * @param {number?} [playerCount=-1] - 发送给多少用户，-1表示全部发送
 * @param {boolean} [allowNotLoginFull=false] - 没完全登录的player也可以发送，默认不是
 * @param {object.<string, object.<string, number>>?} [timeProfiler=undefined] - 可选定，会写入每个用户的当前消息的开始发送时间
 */
function sendToAllPlayerOnTimer(msg, playerCount, allowNotLoginFull, timeProfiler)
{
    if (!msg)
        return;

    playerCount = playerCount >= 0 ? playerCount : playerList.length;

    var sendFunc = function(index) {
        for (var i = 0; i < 1; ++i) {
            if (playerCount >= 0 && index >= playerCount)
                return;

            try {
                var player = playerList[index];
                if (allowNotLoginFull || player.getRoleInfo()) {
                    if (timeProfiler) {
                        var username = player.getUsername();
                        var subData = timeProfiler[username];
                        if (!subData)
                            timeProfiler[username] = subData = {};
                        var module = msg.getModule();
                        var command = msg.getCommand();
                        subData[module + "_" + command] = dateUtil.getTimestampMS();
                    }
                    player.send(msg);
                }
            }
            catch (err) {
                logUtil.error(err.message);
            }

            ++index;
        }

        setImmediate(sendFunc, index);
    };
    setImmediate(sendFunc, 0);
}

var runGameLogicCoroutine = Promise.coroutine(function * () {
    while (!appUtil.isProcessExiting()){
        yield Promise.delay(2000);

        //不能放到顶上require，不然执行顺序不如意
        var doTestModule= require("./doTest");
        doTestModule.doTestNow();
    }
});

function runGameLogic()
{
    return runGameLogicCoroutine();
}

////////////导出元素////////////
exports.createPlayer = createPlayer;
exports.runGameLogic = runGameLogic;
exports.clearPlayer = clearPlayer;
exports.sendToAllPlayer = sendToAllPlayer;
exports.sendToAllPlayerOnTimer = sendToAllPlayerOnTimer;