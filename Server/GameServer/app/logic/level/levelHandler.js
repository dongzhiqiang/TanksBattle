"use strict";

var handlerMgr = require("../session/handlerMgr");
var enProp = require("../enumType/propDefine").enProp;
var enItemId = require("../enumType/globalDefine").enItemId;
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var levelMsg = require("../netMessage/levelMessage");
var CmdIdsLevel = require("../netMessage/levelMessage").CmdIdsLevel;
var ResultCodeLevel = require("../netMessage/levelMessage").ResultCodeLevel;
var levelCfg = require("../gameConfig/levelConfig");
var vipCfg = require("../gameConfig/vipConfig");
var dateUtil = require("../../libs/dateUtil");
var enActProp = require("../enumType/activityPropDefine").enActProp;
var eventNames = require("../enumType/eventDefine").eventNames;
var starRewardCfg = require("../gameConfig/starRewardConfig");
var rewardCfg = require("../gameConfig/rewardConfig");
var enPetFormation = require("../enumType/globalDefine").enPetFormation;
var enPetPos = require("../enumType/globalDefine").enPetPos;

//进入关卡消息
function enterLevel(session, role, msgObj, enterReq)
{
    //先判断体力
    var cfg = levelCfg.getLevelConfig(enterReq.roomId);
    var curStamina = role.getStamina();
    if (curStamina < cfg.staminaCost) {
        msgObj.setResponseData(ResultCodeLevel.LEVEL_NO_STAMINA);
        role.send(msgObj);
        return;
    }

    var levelPart = role.getLevelsPart();
    var levelInfo = levelPart.getLevelById(enterReq.roomId);
    if (levelInfo)  //已经记录这个关卡 判断进入次数
    {
        //过天重置次数
        if (!dateUtil.isSameDay(levelInfo.lastEnter, dateUtil.getTimestamp()))
            levelInfo.enterNum = 0;

        if (levelInfo.enterNum >= cfg.maxChallengeNum)
        {
            msgObj.setResponseData(ResultCodeLevel.LEVEL_NO_NUM);
            role.send(msgObj);
            return;
        }

        levelInfo.lastEnter = dateUtil.getTimestamp();

        //更新关卡信息
        levelPart.updateLevel(levelInfo);
    }
    else    //没有记录 插入新数据
    {
        levelInfo = levelPart.addLevel(enterReq.roomId);
    }

    var levelsData = levelPart.getLevelInfo();

    levelPart.onEnterLevel(levelInfo.roomId);

    let retObj = new levelMsg.LevelUpdateVo(levelsData.curNode, levelsData.curLevel, levelInfo, levelPart.getDropReward());
    msgObj.setResponseData(ResultCode.SUCCESS, retObj);
    role.send(msgObj);
    return;
}
handlerMgr.registerHandler(ModuleIds.MODULE_LEVEL, CmdIdsLevel.CMD_ENTER, enterLevel, levelMsg.LevelEnterVo);

//结算消息
function endLevel(session, role, msgObj, endReq) {

    var levelPart = role.getLevelsPart();
    var itemsPart = role.getItemsPart();
    var actPart = role.getActivityPart();
    var petsPart = role.getPetsPart();
    var curTime = dateUtil.getTimestamp();

    //获取并判断关卡配置
    var roomCfg = levelCfg.getLevelConfig(endReq.roomId);
    if (!roomCfg)
    {
        msgObj.setResponseData(ResultCodeLevel.LEVEL_NOT_EXSTIS);
        role.send(msgObj);
        return;
    }

    //判断体力够不够
    var curStamina = role.getStamina();
    if (curStamina < roomCfg.staminaCost)
    {
        msgObj.setResponseData(ResultCodeLevel.LEVEL_NO_STAMINA);
        role.send(msgObj);
        return;
    }

    //判断有没有相关的关卡数据
    var levelInfo = levelPart.getLevelById(endReq.roomId);
    if (!levelInfo)
    {
        msgObj.setResponseData(ResultCodeLevel.LEVEL_NO_RECORD);
        role.send(msgObj);
        return;
    }

    //过天重置次数
    if (!dateUtil.isSameDay(levelInfo.lastEnter, curTime))
        levelInfo.enterNum = 0;

    //判断今天是不是打满次数了
    if (levelInfo.enterNum >= roomCfg.maxChallengeNum)
    {
        msgObj.setResponseData(ResultCodeLevel.LEVEL_NO_NUM);
        role.send(msgObj);
        return;
    }

    let reward = new levelMsg.LevelRewardVo();

    //输了不做什么操作
    if (!endReq.isWin)
    {
        let levelsData = levelPart.getLevelInfo();
        reward.isWin = endReq.isWin;
        reward.roomId = endReq.roomId;
        reward.updateInfo = new levelMsg.LevelUpdateVo(levelsData.curNode, levelsData.curLevel, levelInfo);
        msgObj.setResponseData(ResultCode.SUCCESS, reward);
        role.send(msgObj);
        return;
    }

    //验证掉落奖励 给掉落奖励
    var isTrue = levelPart.checkRewards(endReq);
    //检查出作假 直接返回
    if (!isTrue)
    {
        msgObj.setResponseData(ResultCode.BAD_REQUEST);
        role.send(msgObj);
        return;
    }

    //清奖励数据
    levelPart.clearDropReward();

    //修改关卡数据
    levelInfo.enterNum++;
    levelInfo.lastEnter = curTime;
    levelInfo.isWin = endReq.isWin;     //是否通关标记

    for(var id in endReq.starsInfo)
    {
        let num = endReq.starsInfo[id];
        let oldNum = levelInfo.starsInfo[id] || 0;
        levelInfo.starsInfo[id] = Math.max(oldNum, num);
    }

    //保存关卡数据
    levelPart.updateLevel(levelInfo);

    //触发事件
    role.fireEvent(eventNames.PASS_LEVEL, endReq.roomId);

    //扣除体力
    role.startBatch();
    curStamina = Math.max(0, curStamina - roomCfg.staminaCost);
    role.setNumber(enProp.stamina, curStamina);
    role.setNumber(enProp.staminaTime, curTime);
    role.endBatch();

    //增加经验
    role.addExp(roomCfg.expReward);

    //给掉落奖励
    var rewardItems = {};
    for(var i = 0; i < endReq.monsterItems.length; i++)
        rewardItems[endReq.monsterItems[i].itemId] = (rewardItems[endReq.monsterItems[i].itemId] || 0) + endReq.monsterItems[i].num;

    for(var i = 0; i < endReq.specialItems.length; i++)
        rewardItems[endReq.specialItems[i].itemId] = (rewardItems[endReq.specialItems[i].itemId] || 0) + endReq.specialItems[i].num;

    for(var i = 0; i < endReq.bossItems.length; i++)
        rewardItems[endReq.bossItems[i].itemId] = (rewardItems[endReq.bossItems[i].itemId] || 0) + endReq.bossItems[i].num;

    for(var i = 0; i < endReq.boxItems.length; i++)
        rewardItems[endReq.boxItems[i].itemId] = (rewardItems[endReq.boxItems[i].itemId] || 0) + endReq.boxItems[i].num;

    itemsPart.addItems(rewardItems);

    var petFormation = role.getPetFormationsPart().getPetFormation(enPetFormation.normal);
    //给宠物经验
    var myPets = petsPart.getMainPets();
    for (let i = 0, len = Math.min(roomCfg.petNum, myPets.length); i < len; ++i)
    {
        let pet = myPets[i];
        pet.addExp(roomCfg.petExp);

        if (pet.getGUID() == petFormation.formation[enPetPos.pet1Main])
            reward.pet1Exp = roomCfg.petExp;
        else if (pet.getGUID() == petFormation.formation[enPetPos.pet2Main])
            reward.pet2Exp = roomCfg.petExp;
    }

    //活动属性
    var mainLvlTime = actPart.getNumber(enActProp.mainLvlTime);
    var mainNormalLvlCnt = actPart.getNumber(enActProp.mainNormalLvlCnt);
    var todayNormalCnt = dateUtil.isToday(mainLvlTime) ? mainNormalLvlCnt + 1 : 1;
    actPart.startBatch();
    actPart.setNumber(enActProp.mainLvlTime, curTime);
    actPart.setNumber(enActProp.mainNormalLvlCnt, todayNormalCnt);
    actPart.endBatch();

    //修改返回的数据
    var levelsData = levelPart.getLevelInfo();
    reward.heroExp = roomCfg.expReward;
    reward.isWin = endReq.isWin;
    reward.roomId = endReq.roomId;
    reward.updateInfo = new levelMsg.LevelUpdateVo(levelsData.curNode, levelsData.curLevel, levelInfo);

    msgObj.setResponseData(ResultCode.SUCCESS, reward);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_LEVEL, CmdIdsLevel.CMD_END, endLevel, levelMsg.LevelEndVo);

/**
 * 扫荡关卡
 * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
 * @param {Role} role - 在这里role是空的，还没有role
 * @param {Message} msgObj
 * @param {SweepLevelReq} reqObj
 */
function sweepLevel(session, role, msgObj, reqObj)
{
    var levelPart = role.getLevelsPart();
    var itemsPart = role.getItemsPart();
    var petsPart = role.getPetsPart();
    var actPart = role.getActivityPart();
    var curTime = dateUtil.getTimestamp();

    //获取并判断关卡配置
    var roomCfg = levelCfg.getLevelConfig(reqObj.roomId);
    if (!roomCfg)
        return ResultCodeLevel.LEVEL_NOT_EXSTIS;

    //判断有没有打通过
    var levelInfo = levelPart.getLevelById(reqObj.roomId);
    if (!levelInfo || !levelInfo.isWin)
        return ResultCodeLevel.MUST_PASS_FIRST;

    var vipLv = role.getNumber(enProp.vipLv);
    var starInfo = levelInfo.starsInfo;
    var stars = 0;
    for(let k in starInfo)
        stars += starInfo[k];
    var sweepCfg = levelCfg.getSweepLevelCfg(reqObj.multiple ? 1 : 0);

    if (!((sweepCfg.condOp == 0 && (stars >= sweepCfg.stars || vipLv >= sweepCfg.vipLv))
        ||
        (sweepCfg.condOp != 0 && (stars >= sweepCfg.stars && vipLv >= sweepCfg.vipLv))))
        return ResultCodeLevel.SWEEP_COND_NOT_MATCH;

    //判断体力够不够
    var curStamina = role.getStamina();
    if (curStamina < roomCfg.staminaCost)
        return ResultCodeLevel.LEVEL_NO_STAMINA;

    //过天重置次数
    if (!dateUtil.isSameDay(levelInfo.lastEnter, curTime))
        levelInfo.enterNum = 0;

    //判断今天是不是打满次数了
    if (levelInfo.enterNum >= roomCfg.maxChallengeNum)
        return ResultCodeLevel.LEVEL_NO_NUM;

    //计算最大可扫荡次数
    var sweepTimes = 1;
    if (reqObj.multiple)
    {
        var myVipCfg = vipCfg.getVipConfig(vipLv);
        sweepTimes = Math.min(myVipCfg.sweepLvlTimes, roomCfg.maxChallengeNum);
    }

    var retBody = new levelMsg.SweepLevelRes();
    var expReward = 0;
    var petExpReward = 0;
    var petGuids = [];
    var itemRewards = {};
    var realSweepCnt = 0;

    var myPets = petsPart.getMainPets();
    for (let i = 0, len = Math.min(roomCfg.petNum, myPets.length); i < len; ++i)
    {
        let pet = myPets[i];
        petGuids.push(pet.getGUID());
    }

    for (let i = 0; i < sweepTimes; ++i)
    {
        //判断体力够不够
        if (curStamina < roomCfg.staminaCost)
            break;

        //判断次数
        if (levelInfo.enterNum >= roomCfg.maxChallengeNum)
            break;

        //加次数
        ++realSweepCnt;
        ++levelInfo.enterNum;

        //扣体力
        curStamina -= roomCfg.staminaCost;

        //数值类奖励
        expReward += roomCfg.expReward;
        petExpReward += roomCfg.petExp;

        //生成物品奖励
        levelPart.onEnterLevel(levelInfo.roomId);
        var dropRewards = levelPart.getDropReward();

        var rewardInfo = new levelMsg.SweepLevelRewardInfo();
        rewardInfo.heroExp = roomCfg.expReward;
        for (let j = 0; j < petGuids.length; ++j)
            rewardInfo.petExps[petGuids[j]] = roomCfg.petExp;
        for (var k in dropRewards)
        {
            var list = dropRewards[k];
            for (let j = 0; j < list.length; ++j)
            {
                let e = list[j];
                rewardInfo.items[e.itemId] = (rewardInfo.items[e.itemId] || 0) + e.num;
                itemRewards[e.itemId] = (itemRewards[e.itemId] || 0) + e.num;
            }
        }

        retBody.rewards.push(rewardInfo);
    }

    //增加经验
    role.addExp(expReward);
    //给物品
    itemsPart.addItems(itemRewards);
    //给宠物经验
    for (let i = 0; i < petGuids.length; ++i)
    {
        let pet = petsPart.getPetByGUID(petGuids[i]);
        pet.addExp(petExpReward);
    }
    //更新用户属性
    role.startBatch();
    role.setNumber(enProp.stamina, curStamina);
    role.setNumber(enProp.staminaTime, curTime);
    role.endBatch();
    //活动属性
    var mainLvlTime = actPart.getNumber(enActProp.mainLvlTime);
    var mainNormalLvlCnt = actPart.getNumber(enActProp.mainNormalLvlCnt);
    var todayNormalCnt = dateUtil.isToday(mainLvlTime) ? mainNormalLvlCnt + realSweepCnt : realSweepCnt;
    actPart.startBatch();
    actPart.setNumber(enActProp.mainLvlTime, curTime);
    actPart.setNumber(enActProp.mainNormalLvlCnt, todayNormalCnt);
    actPart.endBatch();
    //更新关卡数据
    levelInfo.lastEnter = curTime;
    levelPart.updateLevel(levelInfo);

    //设置一些要发给客户端的关卡数据
    var levelsData = levelPart.getLevelInfo();
    retBody.roomId = reqObj.roomId;
    retBody.curNode = levelsData.curNode;
    retBody.curLevel = levelsData.curLevel;
    retBody.levelInfo = levelInfo;
    return retBody;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_LEVEL, CmdIdsLevel.CMD_SWEEP, sweepLevel, levelMsg.SweepLevelReq, true);


/**
 * 星级奖励
 * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
 * @param {Role} role - 在这里role是空的，还没有role
 * @param {Message} msgObj
 * @param {LevelStarRewardReq} reqObj
 */

function getStarReward(session, role, msgObj, reqObj)
{
    var levelPart = role.getLevelsPart();
    var itemsPart = role.getItemsPart();
    var starReward = levelPart.getLevelInfo().starsReward || {};
    var reward = starReward[reqObj.nodeId] || {};
    var state = reward[reqObj.starNum] || 0;
    //已领取过
    if (state != 0)
        return ResultCodeLevel.LEVEL_STAR_CANT_GET;

    //判断获得的星星能否领取
    var starsNum = levelPart.getStarsByNodeId(reqObj.nodeId);
    if(starsNum < reqObj.starNum) {
        return ResultCodeLevel.LEVEL_STAR_CANT_GET;
    }
    reward[reqObj.starNum] = 1;
    if (levelPart.getLevelInfo().starsReward == undefined) {
        levelPart.getLevelInfo().starsReward = {};
    }
    levelPart.getLevelInfo().starsReward[reqObj.nodeId] = reward;

    levelPart.updateLevelStarReward();
    var rewardId = starRewardCfg.getRewardId(reqObj.nodeId, reqObj.starNum, 1);
    //找不到奖励
    if (rewardId == 0) {
        return ResultCodeLevel.LEVEL_STAR_CANT_GET;
    }
    var rewardList = rewardCfg.getRandomReward(rewardId);
    //找不到奖励
    if (rewardList == null) {
        return ResultCodeLevel.LEVEL_STAR_CANT_GET;
    }
    //给物品
    itemsPart.addItems(rewardList);

    var retBody = new levelMsg.LevelStarRewardVo();
    retBody.nodeId = reqObj.nodeId;
    retBody.starNum = reqObj.starNum;
    return retBody;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_LEVEL, CmdIdsLevel.CMD_STAR, getStarReward, levelMsg.LevelStarRewardReq, true);
