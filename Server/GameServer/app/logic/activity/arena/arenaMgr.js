"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var appUtil = require("../../../libs/appUtil");
var dateUtil = require("../../../libs/dateUtil");
var logUtil = require("../../../libs/logUtil");
var rankTypes = require("../../enumType/rankDefine").rankTypes;
var arenaConfig = require("../../gameConfig/arenaConfig");
var rankMgr = require("../../rank/rankMgr");
var robotRoleMgr = require("../../role/robotRoleMgr");
var enActProp = require("../../enumType/activityPropDefine").enActProp;
var mailMgr = require("../../mail/mailMgr");
var itemModule = require("../../item/item");
var enItemId = require("../../enumType/globalDefine").enItemId;
var rewardConfig = require("../../gameConfig/rewardConfig");

////////////模块内变量////////////
/**
 * 定时检查间隔，单位毫秒
 * @type {number}
 */
var TIMER_CHECK_INV = 60 * 1000;

/**
 * 定时检查的定时器对象
 * @type {object}
 */
var timerCheck = null;

/**
 * 现在是否在运行某个定时任务
 * @type {boolean}
 */
var inRunTimerTask = false;

////////////导出函数////////////
var checkRobotCoroutine = Promise.coroutine(function * ()
{
    logUtil.info("开始检查竞技场机器人数据...");

    /**
     * @type {ArenaRobotCfg[]}
     */
    var arenaRobotCfgs = arenaConfig.getArenaRobotCfg();

    for (let i = 0; i < arenaRobotCfgs.length; ++i)
    {
        var arenaRobotCfg = arenaRobotCfgs[i];

        //获取本robotId对应的机器人的数量
        var thisRobotNum = robotRoleMgr.getHeroNumByRobotId(arenaRobotCfg.robotId);

        //如果本robotId对应的机器人的数量更少，就补足
        for (let j = thisRobotNum; j < arenaRobotCfg.robotNum; ++j)
        {
            var robotRole = robotRoleMgr.getRobotRoleOrAddNew(arenaRobotCfg.robotId, j, true);
            var score = appUtil.getRandom(arenaRobotCfg.minScore, arenaRobotCfg.maxScore);
            robotRole.getActivityPart().setNumber(enActProp.arenaScore, score);
            robotRoleMgr.addRobotToAllRank(robotRole);
        }
    }

    yield robotRoleMgr.checkTempNewRobotData();

    logUtil.info("结束检查竞技场机器人数据");
});

function sendDayRewardMail(heroIds, grade)
{
    //获取配置
    let gradeCfg = arenaConfig.getArenaGradeCfg(grade)
    let items = rewardConfig.getRandomReward2(gradeCfg.dayRewardId);

    //奖品数量


    //生成附件
    /*if (goldNum > 0)
        items.push(itemModule.createItem({itemId:enItemId.GOLD, num:goldNum}));
    if (arenaCoinNum > 0)
        items.push(itemModule.createItem({itemId:enItemId.ARENA_COIN, num:arenaCoinNum}));
    if (diamondNum > 0)
        items.push(itemModule.createItem({itemId:enItemId.DIAMOND, num:diamondNum}));*/
    //有东西可给才发邮件
    if (items.length > 0)
    {
        //创建邮件
        let mail = mailMgr.createMail("竞技场每日奖励", "系统", "由于您在竞技场处于“" + gradeCfg.gradeName + "”段位，特此给与您奖励！", items);
        //发送
        mailMgr.sendMailToMultiRole(heroIds, mail);
    }
}

function sendDayRankRewardMail(heroId, rank)
{
    let arenaRankCfg = arenaConfig.getArenaRankCfg(rank);
    let items = rewardConfig.getRandomReward2(arenaRankCfg.rewardId);
    //有东西可给才发邮件
    if (items.length > 0)
    {
        //创建邮件
        let mail = mailMgr.createMail("竞技场每日奖励", "系统", "由于您在竞技场位于第" + rank + "名，特此给与您奖励！", items);
        //发送
        mailMgr.sendMailToMultiRole([heroId], mail);
    }


}

function sendWeekRewardMail(heroIds, grade)
{
    //获取配置
    /*let gradeCfg = arenaConfig.getArenaGradeCfg(grade);

    //奖品数量
    let goldNum = gradeCfg.weekRewardGold;
    let arenaCoinNum = gradeCfg.weekRewardArenaCoin;
    let diamondNum = gradeCfg.weekRewardDiamond;

    //生成附件
    let items = [];
    if (goldNum > 0)
        items.push(itemModule.createItem({itemId:enItemId.GOLD, num:goldNum}));
    if (arenaCoinNum > 0)
        items.push(itemModule.createItem({itemId:enItemId.ARENA_COIN, num:arenaCoinNum}));
    if (diamondNum > 0)
        items.push(itemModule.createItem({itemId:enItemId.DIAMOND, num:diamondNum}));
    //有东西可给才发邮件
    if (items.length > 0)
    {
        //创建邮件
        let mail = mailMgr.createMail("竞技场每周奖励", "系统", "由于您在竞技场处于“" + gradeCfg.gradeName + "”段位，特此给与您奖励！", items);
        //发送
        mailMgr.sendMailToMultiRole(heroIds, mail);
    }*/
}

var onTimerCheckCoroutine = Promise.coroutine(function * () {
    //已有一个定时任务在运行中？先不继续了
    if (inRunTimerTask)
        return;

    try {
        inRunTimerTask = true;

        var basicCfg = arenaConfig.getArenaBasicCfg();
        var rankData = rankMgr.getRankData(rankTypes.arena);
        var rankArr = rankData.data;
        var rankExtra = rankData.svrExtra;
        var lastDayRewardTime = rankExtra.lastDayRewardTime || 0;
        rankExtra.lastDayRewardTime = lastDayRewardTime;    //回写一下，以防原来没这个变量
        var lastWeekRewardTime = rankExtra.lastWeekRewardTime || 0;
        rankExtra.lastWeekRewardTime = lastWeekRewardTime;  //回写一下，以防原来没这个变量

        var lastDayRankRewardTime = rankExtra.lastDayRankRewardTime || 0;
        rankExtra.lastDayRankRewardTime = lastDayRankRewardTime;    //回写一下，以防原来没这个变量

        var dayRewardTags = rankExtra.dayRewardTags || {};
        rankExtra.dayRewardTags = dayRewardTags;            //回写一下，以防原来没这个变量，导致临时对象没关联
        var weekRewardTags = rankExtra.weekRewardTags || {};
        rankExtra.weekRewardTags = weekRewardTags;          //回写一下，以防原来没这个变量，导致临时对象没关联

        var curTime = dateUtil.getTimestamp();
        var dayOfWeek = dateUtil.getDayOfWeek();
        var curDate = dateUtil.getDate();
        var yearMonthDate = curDate.getFullYear() * 10000 + (curDate.getMonth() + 1) * 100 + curDate.getDate();
        var minutePass = curDate.getHours() * 60 + curDate.getMinutes();

        if (minutePass >= basicCfg.dayRewardTime && !dateUtil.isSameDay(curTime, lastDayRewardTime))
        {
            logUtil.info("竞技场每日奖励开始发放");
            let profileTime1 = dateUtil.getTimestamp();

            let tmpHeroIdMap = {};
            let tmpHeroIdArr = [];
            let doBatchSend = false;
            let lastGrade = -1;
            let batchSendGrade = -1;
            //注意，如果这里有yield，可能会导致rankArr其实会变化
            for (let i = 0; i < rankArr.length; ++i)
            {
                let rankItem = rankArr[i];
                let heroId = rankItem.key;

                //机器人、无效主角ID就不给奖励了
                if (heroId <= 0)
                    continue;

                let rewardTag = dayRewardTags[heroId];
                //今天的奖励没给？
                if (rewardTag !== yearMonthDate)
                {
                    let score = rankItem.score;
                    let grade = arenaConfig.getGradeByScore(score);
                    if (lastGrade === -1)
                        lastGrade = grade;
                    //段位有变化了？必须做一次批量发送，不然会发错奖
                    if (grade !== lastGrade)
                    {
                        --i; //下次循环还要使用本元素，所以减一
                        lastGrade = grade;
                        doBatchSend = true;
                    }
                    else
                    {
                        dayRewardTags[heroId] = yearMonthDate;

                        //加入临时容器，用于更新数据库
                        tmpHeroIdMap["svrExtra.dayRewardTags." + heroId] = yearMonthDate;
                        tmpHeroIdArr.push(heroId);
                        batchSendGrade = grade;

                        //处理一百个后
                        if (tmpHeroIdArr.length >= 100)
                            doBatchSend = true;
                    }
                }

                if (doBatchSend)
                {
                    doBatchSend = false;

                    if (tmpHeroIdArr.length > 0)
                    {
                        //发送邮件
                        sendDayRewardMail(tmpHeroIdArr, batchSendGrade);

                        //保存标记
                        yield rankMgr.doCustomDBUpdate(rankTypes.arena, {$set: tmpHeroIdMap});

                        //清变量
                        tmpHeroIdMap = {};
                        tmpHeroIdArr = [];

                        //并休息一下
                        yield Promise.delay(5);
                    }
                }
            }
            if (tmpHeroIdArr.length > 0)
            {
                //发送邮件
                sendDayRewardMail(tmpHeroIdArr, batchSendGrade);

                //保存标记
                yield rankMgr.doCustomDBUpdate(rankTypes.arena, {$set:tmpHeroIdMap});

                //清变量
                tmpHeroIdMap = {};
                tmpHeroIdArr = [];
            }

            //保存lastDayRewardTime和dayRewardTags
            rankExtra.lastDayRewardTime = curTime;
            rankExtra.dayRewardTags = {};   //处理完了，可以清除标记了
            let updateObj = {};
            updateObj["svrExtra.lastDayRewardTime"] = curTime;
            updateObj["svrExtra.dayRewardTags"] = {};
            yield rankMgr.doCustomDBUpdate(rankTypes.arena, {$set:updateObj});

            let profileTime2 = dateUtil.getTimestamp();
            logUtil.info("竞技场每日奖励结束发放，耗时：" + (profileTime2 - profileTime1) + "秒");
        }


        if (minutePass >= basicCfg.dayRewardTime && !dateUtil.isSameDay(curTime, lastDayRankRewardTime))
        {
            for (let i = 0; i < rankArr.length; ++i)
            {
                if(i<100)
                {
                    let rankItem = rankArr[i];
                    let heroId = rankItem.key;
                    sendDayRankRewardMail(heroId,i + 1);
                }
                else
                {
                    break;
                }

            }
            //保存lastDayRewardTime和dayRewardTags
            rankExtra.lastDayRankRewardTime = curTime;

        }


        /*if (dayOfWeek == basicCfg.weekRewardDay && minutePass >= basicCfg.weekRewardTime &&  !dateUtil.isSameDay(curTime, lastWeekRewardTime))
        {
            logUtil.info("竞技场每周奖励开始发放");
            let profileTime1 = dateUtil.getTimestamp();

            let tmpHeroIdMap = {};
            let tmpHeroIdArr = [];
            let doBatchSend = false;
            let lastGrade = -1;
            let batchSendGrade = -1;
            //注意，如果这里有yield，可能会导致rankArr其实会变化
            for (let i = 0; i < rankArr.length; ++i)
            {
                let rankItem = rankArr[i];
                let heroId = rankItem.key;

                //机器人、无效主角ID就不给奖励了
                if (heroId <= 0)
                    continue;

                let rewardTag = weekRewardTags[heroId];
                //今天的奖励没给？
                if (rewardTag !== yearMonthDate)
                {
                    let score = rankItem.score;
                    let grade = arenaConfig.getGradeByScore(score);
                    if (lastGrade === -1)
                        lastGrade = grade;
                    //段位有变化了？必须做一次批量发送，不然会发错奖
                    if (grade !== lastGrade)
                    {
                        --i; //下次循环还要使用本元素，所以减一
                        lastGrade = grade;
                        doBatchSend = true;
                    }
                    else
                    {
                        weekRewardTags[heroId] = yearMonthDate;

                        //加入临时容器，用于更新数据库
                        tmpHeroIdMap["svrExtra.weekRewardTags." + heroId] = yearMonthDate;
                        tmpHeroIdArr.push(heroId);
                        batchSendGrade = grade;

                        //处理一百个后
                        if (tmpHeroIdArr.length >= 100)
                            doBatchSend = true;
                    }
                }

                if (doBatchSend)
                {
                    doBatchSend = false;

                    if (tmpHeroIdArr.length > 0)
                    {
                        //发送邮件
                        sendWeekRewardMail(tmpHeroIdArr, batchSendGrade);

                        //保存标记
                        yield rankMgr.doCustomDBUpdate(rankTypes.arena, {$set: tmpHeroIdMap});

                        //清变量
                        tmpHeroIdMap = {};
                        tmpHeroIdArr = [];

                        //并休息一下
                        yield Promise.delay(5);
                    }
                }
            }
            if (tmpHeroIdArr.length > 0)
            {
                //发送邮件
                sendWeekRewardMail(tmpHeroIdArr, batchSendGrade);

                //保存标记
                yield rankMgr.doCustomDBUpdate(rankTypes.arena, {$set:tmpHeroIdMap});

                //清变量
                tmpHeroIdMap = {};
                tmpHeroIdArr = [];
            }

            //保存lastWeekRewardTime和weekRewardTags
            rankExtra.lastWeekRewardTime = curTime;
            rankExtra.weekRewardTags = {};   //处理完了，可以清除标记了
            let updateObj = {};
            updateObj["svrExtra.lastWeekRewardTime"] = curTime;
            updateObj["svrExtra.weekRewardTags"] = {};
            yield rankMgr.doCustomDBUpdate(rankTypes.arena, {$set:updateObj});

            let profileTime2 = dateUtil.getTimestamp();
            logUtil.info("竞技场每周奖励结束发放，耗时：" + (profileTime2 - profileTime1) + "秒");
        }*/
    }
    catch (err) {
        logUtil.error("", err);
    }
    finally {
        inRunTimerTask = false;
    }
});

function onTimerCheck()
{
    //竞技场相关的定时操作
    onTimerCheckCoroutine();
}

var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("竞技场模块开始初始化...");

    //检查竞技场机器人
    yield checkRobotCoroutine();

    //开启定时器
    timerCheck = setInterval(onTimerCheck, TIMER_CHECK_INV);

    logUtil.info("竞技场模块完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("竞技场模块开始销毁...");

    //清除定时器
    clearInterval(timerCheck);

    logUtil.info("竞技场模块完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}

exports.doInit = doInit;
exports.doDestroy = doDestroy;