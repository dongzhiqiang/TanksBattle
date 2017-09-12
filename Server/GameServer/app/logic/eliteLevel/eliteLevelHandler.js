"use strict";

var handlerMgr = require("../session/handlerMgr");
var enProp = require("../enumType/propDefine").enProp;
var enItemId = require("../enumType/globalDefine").enItemId;
var enPetPos = require("../enumType/globalDefine").enPetPos;
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var eliteLevelMsg = require("../netMessage/eliteLevelMessage");
var levelMsg = require("../netMessage/levelMessage");
var CmdIdsEliteLevel = require("../netMessage/eliteLevelMessage").CmdIdsEliteLevel;
var ResultCodeEliteLevel = require("../netMessage/eliteLevelMessage").ResultCodeEliteLevel;
var eliteLevelCfg = require("../gameConfig/eliteLevelConfig");
var vipConfig = require("../gameConfig/vipConfig");
var levelCfg = require("../gameConfig/levelConfig");
var vipCfg = require("../gameConfig/vipConfig");
var dateUtil = require("../../libs/dateUtil");
var enActProp = require("../enumType/activityPropDefine").enActProp;
var eventNames = require("../enumType/eventDefine").eventNames;
var rewardConfig = require("../gameConfig/rewardConfig");
var enPetFormation = require("../enumType/globalDefine").enPetFormation;

/**
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EnterEliteLevelRequestVo} enterReq
 * @returns {number|object}
 */
function enterEliteLevel(session, role, msgObj, enterReq)
{
    //先判断体力
    var cfg = eliteLevelCfg.getEliteLevelCfg(enterReq.levelId);
    if(!cfg)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NOT_EXISTS;
    }
    var baseCfg = eliteLevelCfg.getEliteLevelBasicCfg();
    //var roomCfg = levelCfg.getLevelConfig(cfg.roomId);
    var curStamina = role.getStamina();
    if (curStamina < baseCfg.costStamina) {
        return ResultCodeEliteLevel.ELITE_LEVEL_NO_STAMINA;
    }

    if(cfg.openPassLvl)
    {
        var level = role.getLevelsPart().getLevelById(cfg.openPassLvl);
        if(!level || !level.isWin)
        {
            return ResultCodeEliteLevel.ELITE_LEVEL_NEED_PASS_LEVEL;
        }
    }
    if(cfg.openPassEltLvl)
    {
        var el = role.getEliteLevelsPart().getEliteLevelByLevelId(cfg.openPassEltLvl);
        if(!el || !el.passed)
        {
            return ResultCodeEliteLevel.ELITE_LEVEL_NEED_PASS_LEVEL;
        }
    }

    var eliteLevelsPart = role.getEliteLevelsPart();
    var eliteLevel = eliteLevelsPart.getEliteLevelByLevelId(enterReq.levelId);
    if (eliteLevel)  //已经记录这个关卡 判断进入次数
    {
        //过天重置次数
        if (!dateUtil.isSameDay(eliteLevel.enterTime, dateUtil.getTimestamp()))
        {
            eliteLevel.count = 0;
            eliteLevel.resetCount = 0;
        }

        if (eliteLevel.count >= baseCfg.dayMaxCnt)
        {
            return ResultCodeEliteLevel.ELITE_LEVEL_NO_NUM;
        }

        eliteLevel.enterTime = dateUtil.getTimestamp();

        //更新关卡信息
        eliteLevel.syncAndSave();
    }
    else    //没有记录 插入新数据
    {
        eliteLevel = eliteLevelsPart.addEliteLevelWithLevelId(enterReq.levelId);
    }

    eliteLevelsPart.initRewards(cfg.roomId);

    let retObj = new eliteLevelMsg.EnterEliteLevelResultVo(enterReq.levelId, eliteLevelsPart.getDropReward());
    return retObj;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ELITE_LEVEL, CmdIdsEliteLevel.CMD_ENTER_ELITE_LEVEL, enterEliteLevel, eliteLevelMsg.EnterEliteLevelRequestVo);

/**
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {EndEliteLevelRequestVo} endReq
 * @returns {number|object}
 */
function endEliteLevel(session, role, msgObj, endReq) {

    var eliteLevelsPart = role.getEliteLevelsPart();
    var itemsPart = role.getItemsPart();
    var petsPart = role.getPetsPart();
    var curTime = dateUtil.getTimestamp();

    //获取并判断关卡配置
    var cfg = eliteLevelCfg.getEliteLevelCfg(endReq.levelId);
    if(!cfg)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NOT_EXISTS;
    }

    //判断体力够不够
    var baseCfg = eliteLevelCfg.getEliteLevelBasicCfg();
    var curStamina = role.getStamina();
    if (curStamina < baseCfg.costStamina)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NO_STAMINA;
    }

    //判断有没有相关的关卡数据
    var eliteLevel = eliteLevelsPart.getEliteLevelByLevelId(endReq.levelId);
    if (!eliteLevel)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NO_RECORD;
    }

    //过天重置次数
    if (!dateUtil.isSameDay(eliteLevel.enterTime, dateUtil.getTimestamp()))
    {
        eliteLevel.count = 0;
        eliteLevel.resetCount = 0;
    }

    //判断今天是不是打满次数了
    if (eliteLevel.count >= baseCfg.dayMaxCnt)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NO_NUM;
    }

    var endResult = new eliteLevelMsg.EndEliteLevelResultVo();

    //输了不做什么操作
    if (!endReq.isWin)
    {
        endResult.isWin = endReq.isWin;
        endResult.levelId = endReq.levelId;
        return endResult;
    }

    //验证掉落奖励 给掉落奖励
    var isTrue = eliteLevelsPart.checkRewards(endReq);
    //检查出作假 直接返回
    if (!isTrue)
    {
        return ResultCode.BAD_REQUEST;
    }

    //清奖励数据
    eliteLevelsPart.clearDropReward();

    //修改关卡数据
    eliteLevel.count++;
    eliteLevel.enterTime = curTime;
    if(!eliteLevel.passed)
    {
        //首次通关奖励
        eliteLevel.passed = true;     //是否通关标记
    }

    for(var id in endReq.starsInfo)
    {
        let num = endReq.starsInfo[id];
        if(num > 1)
        {
            continue;
        }
        let oldNum = eliteLevel.starsInfo[id] || 0;
        eliteLevel.starsInfo[id] = Math.max(oldNum, num);
    }

    //保存关卡数据
    eliteLevel.syncAndSave();

    //扣除体力
    role.startBatch();
    curStamina = Math.max(0, curStamina - baseCfg.costStamina);
    role.setNumber(enProp.stamina, curStamina);
    role.setNumber(enProp.staminaTime, curTime);
    role.endBatch();

    //增加经验
    var roomCfg = levelCfg.getLevelConfig(cfg.roomId);
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

    //给宠物经验
    var myPets = petsPart.getMainPets();
    var petFormation = role.getPetFormationsPart().getPetFormation(enPetFormation.eliteLevel);
    for (let i = 0, len = Math.min(roomCfg.petNum, myPets.length); i < len; ++i)
    {
        let pet = myPets[i];
        pet.addExp(roomCfg.petExp);

        if (pet.getGUID() == petFormation.formation[enPetPos.pet1Main])
            endResult.pet1Exp = roomCfg.petExp;
        else if (pet.getGUID() == petFormation.formation[enPetPos.pet2Main])
            endResult.pet2Exp = roomCfg.petExp;
    }

    //修改返回的数据
    endResult.heroExp = roomCfg.expReward;
    endResult.isWin = endReq.isWin;
    endResult.levelId = endReq.levelId;

    return endResult;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ELITE_LEVEL, CmdIdsEliteLevel.CMD_END_ELITE_LEVEL, endEliteLevel, eliteLevelMsg.EndEliteLevelRequestVo);

/**
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {SweepEliteLevelRequestVo} sweepReq
 * @returns {number|object}
 */
function sweepEliteLevel(session, role, msgObj, sweepReq)
{
    var eliteLevelsPart = role.getEliteLevelsPart();
    var itemsPart = role.getItemsPart();
    var petsPart = role.getPetsPart();
    var curTime = dateUtil.getTimestamp();

    //获取并判断关卡配置
    var cfg = eliteLevelCfg.getEliteLevelCfg(sweepReq.levelId);
    if(!cfg)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NOT_EXISTS;
    }

    //判断有没有打通过
    var eliteLevel = eliteLevelsPart.getEliteLevelByLevelId(sweepReq.levelId);
    if (!eliteLevel || !eliteLevel.passed) {
        return ResultCodeEliteLevel.ELITE_LEVEL_MUST_PASS;
    }

    var vipLv = role.getNumber(enProp.vipLv);
    var starInfo = eliteLevel.starsInfo;
    var stars = 0;
    for(let k in starInfo)
        stars += starInfo[k];
    var sweepCfg = levelCfg.getSweepLevelCfg(sweepReq.multiple ? 1 : 0);

    if (!((sweepCfg.condOp == 0 && (stars >= sweepCfg.stars || vipLv >= sweepCfg.vipLv))
        ||
        (sweepCfg.condOp != 0 && (stars >= sweepCfg.stars && vipLv >= sweepCfg.vipLv))))
        return ResultCodeEliteLevel.ELITE_LEVEL_SWEEP_COND;

    //判断体力够不够
    var baseCfg = eliteLevelCfg.getEliteLevelBasicCfg();
    var curStamina = role.getStamina();
    if (curStamina < baseCfg.costStamina)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NO_STAMINA;
    }

    //过天重置次数
    if (!dateUtil.isSameDay(eliteLevel.enterTime, dateUtil.getTimestamp()))
    {
        eliteLevel.count = 0;
        eliteLevel.resetCount = 0;
    }

    //判断今天是不是打满次数了
    if (eliteLevel.count >= baseCfg.dayMaxCnt)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NO_NUM;
    }

    //计算最大可扫荡次数
    var sweepTimes = 1;
    if (sweepReq.multiple)
    {
        var myVipCfg = vipCfg.getVipConfig(vipLv);
        sweepTimes = Math.min(myVipCfg.sweepLvlTimes, baseCfg.dayMaxCnt-eliteLevel.count);
    }

    var sweepResult = new eliteLevelMsg.SweepEliteLevelResultVo();
    var expReward = 0;
    var petExpReward = 0;
    var petGuids = [];
    var itemRewards = {};
    var realSweepCnt = 0;

    var roomCfg = levelCfg.getLevelConfig(cfg.roomId);
    var myPets = role.getPetFormationsPart().getPetFormation(enPetFormation.eliteLevel).getMainPets();
    for (let i = 0, len = Math.min(roomCfg.petNum, myPets.length); i < len; ++i)
    {
        let pet = myPets[i];
        petGuids.push(pet.getGUID());
    }

    for (let i = 0; i < sweepTimes; ++i)
    {
        //判断体力够不够
        if (curStamina < baseCfg.costStamina)
            break;

        //判断次数
        if (eliteLevel.count >= baseCfg.dayMaxCnt)
            break;

        //加次数
        ++realSweepCnt;
        ++eliteLevel.count;

        //扣体力
        curStamina -= baseCfg.costStamina;

        //数值类奖励
        expReward += roomCfg.expReward;
        petExpReward += roomCfg.petExp;

        //生成物品奖励
        eliteLevelsPart.initRewards(cfg.roomId);
        var dropRewards = eliteLevelsPart.getDropReward();

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

        sweepResult.rewards.push(rewardInfo);
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
    //更新关卡数据
    eliteLevel.enterTime = curTime;
    eliteLevel.syncAndSave();

    //设置一些要发给客户端的关卡数据
    sweepResult.levelId = sweepReq.levelId;
    return sweepResult;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ELITE_LEVEL, CmdIdsEliteLevel.CMD_SWEEP_ELITE_LEVEL, sweepEliteLevel, eliteLevelMsg.SweepEliteLevelRequestVo);



/**
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {GetFirstRewardRequestVo} getReq
 * @returns {number|object}
 */
function getFirstReward(session, role, msgObj, getReq) {

    var eliteLevelsPart = role.getEliteLevelsPart();
    var itemsPart = role.getItemsPart();

    //获取并判断关卡配置
    var cfg = eliteLevelCfg.getEliteLevelCfg(getReq.levelId);
    if(!cfg)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NOT_EXISTS;
    }

    //判断有没有打通过
    var eliteLevel = eliteLevelsPart.getEliteLevelByLevelId(getReq.levelId);
    if (!eliteLevel || !eliteLevel.passed) {
        return ResultCodeEliteLevel.ELITE_LEVEL_MUST_PASS;
    }

    //判断是否已领取过
    if (eliteLevel.firstRewarded)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_REWARDED;
    }


    //修改关卡数据
    eliteLevel.firstRewarded = true;
    //保存关卡数据
    eliteLevel.syncAndSave();

    //给掉落奖励
    var rewardList = rewardConfig.getRandomReward2(cfg.firstReward);
    var items = {};
    for(var i = 0,len = rewardList.length; i<len;++i)
    {
        items[rewardList[i].itemId] = (items[rewardList[i].itemId] || 0) + rewardList[i].num;
    }
    //给物品
    itemsPart.addItems(items);


    //修改返回的数据
    var result = new eliteLevelMsg.GetFirstRewardResultVo();
    result.levelId = getReq.levelId;

    return result;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ELITE_LEVEL, CmdIdsEliteLevel.CMD_GET_FIRST_REWARD, getFirstReward, eliteLevelMsg.GetFirstRewardRequestVo);



/**
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ResetEliteLevelRequestVo} resetReq
 * @returns {number|object}
 */
function resetEliteLevel(session, role, msgObj, resetReq) {

    var eliteLevelsPart = role.getEliteLevelsPart();
    var itemsPart = role.getItemsPart();

    //获取并判断关卡配置
    var cfg = eliteLevelCfg.getEliteLevelCfg(resetReq.levelId);
    if(!cfg)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NOT_EXISTS;
    }

    //判断有没有相关的关卡数据
    var eliteLevel = eliteLevelsPart.getEliteLevelByLevelId(resetReq.levelId);
    if (!eliteLevel)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NO_RECORD;
    }

    //过天重置次数
    if (!dateUtil.isSameDay(eliteLevel.enterTime, dateUtil.getTimestamp()))
    {
        eliteLevel.enterTime = dateUtil.getTimestamp();
        eliteLevel.count = 0;
        eliteLevel.resetCount = 0;
    }

    var vipCfg = vipConfig.getVipConfig(role.getNumber(enProp.vipLv));

    var resetCfg = eliteLevelCfg.getEliteLevelResetCfg(eliteLevel.resetCount+1);
    if(!resetCfg || eliteLevel.resetCount>=vipCfg.specialLvlResetNum)
    {
        return ResultCodeEliteLevel.ELITE_LEVEL_NO_RESET_NUM;
    }

    if (role.getNumber(enProp.diamond) < resetCfg.costDiamond)
    {
        return ResultCode.DIAMOND_INSUFFICIENT;
    }

    //扣钻石
    itemsPart.costItem(enItemId.DIAMOND,resetCfg.costDiamond);

    //修改关卡数据
    eliteLevel.count = 0;
    eliteLevel.resetCount = eliteLevel.resetCount+1;
    //保存关卡数据
    eliteLevel.syncAndSave();


    //修改返回的数据
    var result = new eliteLevelMsg.ResetEliteLevelResultVo();
    result.levelId = resetReq.levelId;

    return result;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ELITE_LEVEL, CmdIdsEliteLevel.CMD_RESET_ELITE_LEVEL, resetEliteLevel, eliteLevelMsg.ResetEliteLevelRequestVo);
