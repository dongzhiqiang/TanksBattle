"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var logUtil = require("../../../libs/logUtil");
var treasureRobotConfig = require("../../gameConfig/treasureRobotConfig");
var rankMgr = require("../../rank/rankMgr");
var robotRoleMgr = require("../../role/robotRoleMgr");

////////////导出函数////////////
var checkRobotCoroutine = Promise.coroutine(function * ()
{
    logUtil.info("开始检查神器抢夺机器人数据...");

    /**
     * @type {TreasureRobotConfig[]}
     */
    var robotCfgs = treasureRobotConfig.getTreasureRobotConfig();

    for (let i = 0; i < robotCfgs.length; ++i)
    {
        var robotCfg = robotCfgs[i];

        //获取本robotId对应的机器人的数量
        var thisRobotNum = robotRoleMgr.getHeroNumByRobotId(robotCfg.robotId);

        //如果本robotId对应的机器人的数量更少，就补足
        for (let j = thisRobotNum; j < robotCfg.robotNum; ++j)
        {
            robotRoleMgr.getRobotRoleOrAddNew(robotCfg.robotId, j);
        }
    }

    yield robotRoleMgr.checkTempNewRobotData();

    logUtil.info("结束检查神器抢夺机器人数据");
});

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("神器抢夺模块开始初始化...");

    //检查神器抢夺机器人
    yield checkRobotCoroutine();

    logUtil.info("神器抢夺模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("神器抢夺模块开始销毁...");

    logUtil.info("神器抢夺模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

exports.doInit = doInit;
exports.doDestroy = doDestroy;