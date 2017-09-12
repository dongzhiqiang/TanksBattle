"use strict";

var Promise = require("bluebird");

var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var Message = require("../../libs/message").Message;
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var gmMessage = require("../netMessage/gmMessage");
var CmdIdsGm = require("../netMessage/gmMessage").CmdIdsGm;
var ResultCodeGm = require("../netMessage/gmMessage").ResultCodeGm;
var sessionMgr = require("../session/sessionMgr");
var enProp = require("../enumType/propDefine").enProp;
var enActProp = require("../enumType/activityPropDefine").enActProp;
var itemConfig = require("../gameConfig/itemConfig");
var globalDefine = require("../enumType/globalDefine").globalDefine;
var enItemId = require("../enumType/globalDefine").enItemId;
var goldLevelConfig = require("../gameConfig/goldLevelConfig");
var guardLevelConfig = require("../gameConfig/guardLevelConfig");
var hadesLevelConfig = require("../gameConfig/hadesLevelConfig");
var roleConfig = require("../gameConfig/roleConfig");
var itemModule = require("../item/item");
var MailMgr = require("../mail/mailMgr");
var enOpActProp = require("../enumType/opActivityPropDefine").enOpActProp;
var eventNames = require("../enumType/eventDefine").eventNames;
var systemConfig = require("../gameConfig/systemConfig");
var activityMessage = require("../netMessage/activityMessage");
var CmdIdsActivity = require("../netMessage/activityMessage").CmdIdsActivity;
var roleMgr = require("../role/roleMgr");
var chatMgr = require("../chat/chatMgr");
var valueConfig = require("../gameConfig/valueConfig");
var treasureConfig = require("../gameConfig/treasureConfig");
var eliteLevelConfig = require("../gameConfig/eliteLevelConfig");

//一律用小写字母
const GMCommand = {
    CMD_ADD_ITEM: "additem",    //添加道具
    CMD_ADD_ITEM_BY_TYPE: "additembytype",    //所有指定类型道具增加指定数量
    CMD_SET_LEVEL: "setlevel",  //设置等级
    CMD_RESET_LEVEL: "resetlevel",  //重置关卡信息
    CMD_OPEN_LEVEL: "openlevel", //开启关卡
    CMD_OPEN_ELITE_LEVEL: "openelitelevel", //开启精英关卡
    CMD_SET_TIME: "time",   //设置服务器时间
    CMD_RESET_GOLD_LV: "resetgoldlv",   //重置金币副本次数
    CMD_RESET_HADES_LV: "resethadeslv",   //重置哈迪斯副本次数
    CMD_RESET_GUARD_LV: "resetguardlv",   //重置守护副本次数
    CMD_RESET_ARENA: "resetarena",   //重置竞技场次数
    CMD_RESET_TREASURE_ROB: "resettreasurerob",   //重置神器抢夺次数
    CMD_RESET_DATA: "reset",   //重置整个角色数据
    CMD_QUICK_TEST: "quicktest",   //获取用于测试的初始资源 等级30, 金币100000 等
    CMD_ADD_PET: "addpet", //添加宠物
    CMD_ADD_MAIL: "addmail",     //添加测试邮件
    CMD_RESET_CHECK_IN: "resetcheckin",  //重设签到
    CMD_SET_SVR_NUM_PROP: "setsvrnumprop",  //设置服务端数字属性
    CMD_SET_SVR_STR_PROP: "setsvrstrprop",  //设置服务端字符串属性
    CMD_ADD_SVR_NUM_PROP: "addsvrnumprop",  //增加服务端数字属性
    CMD_GET_SVR_PROP: "getsvrprop",  //获取服务端属性，如果数字属性，也当字符串来获取
    CMD_SET_TEACH_DATA: "setteachdata",     //修改引导数据
    CMD_CLEAR_TEACH_DATA: "clearteachdata",     //修改引导数据
    CMD_TEST_MODE: "opentestmode",     //打开全部功能图标
    CMD_START_ARENA: "startarena",     //和任意用户（除自己外）在竞技场PK
    CMD_SEND_SYS_CHAT: "sendsyschat",     //发系统消息给某用户
    CMD_RECHARGE: "recharge", //充值钻石
    CMD_CLEAR_JOINT_CORPS_CD: "clearjoincorpscd",  //清除入会cd
    CMD_FINISH_WARRIOR:"finishwarrior",             //通关勇士试炼
    CMD_ADD_TREASURE: "addtreasure",        //添加神器
    CMD_OPEN_ICON: "openicon",     //打开功用图标
};

// 因为gm命令复杂性，可能用到异步处理，所以还是使用coroutine
var processGmCmdCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {ProcessGmCmdVo} processGmCmdReq
     */
    function *(session, role, msgObj, processGmCmdReq) {
        var msg = processGmCmdReq.msg.trim();
        var msgs = msg.split(/\s+/);
        var cmd = msgs.length > 0 ? msgs[0].toLowerCase() : "";

        switch (cmd) {
            case GMCommand.CMD_ADD_ITEM:
                var itemId = parseInt(msgs[1]);
                if (isNaN(itemId)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 物品ID 物品数量。 物品ID必须是数字。");
                    role.send(msgObj);
                    return;
                }
                var itemNum = parseInt(msgs[2]);
                if (isNaN(itemNum)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 物品ID 物品数量。 物品数量必须是数字。");
                    role.send(msgObj);
                    return;
                }
                // 检查是否存在此道具id
                if (!itemConfig.getItemConfig(itemId)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "无ID为" + itemId + "的道具。");
                    role.send(msgObj);
                    return;
                }
                role.getItemsPart().addItem(itemId, itemNum);
                break;
            case GMCommand.CMD_ADD_ITEM_BY_TYPE:
                var itemType = parseInt(msgs[1]);
                if (isNaN(itemType)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 物品类型 物品数量。 物品类型必须是数字。");
                    role.send(msgObj);
                    return;
                }
                var itemNum = parseInt(msgs[2]);
                if (isNaN(itemNum)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 物品类型 物品数量。 物品数量必须是数字。");
                    role.send(msgObj);
                    return;
                }
                // 检查是否存在此道具id
                var allConfig = itemConfig.getItemConfig();
                for (var k in allConfig) {
                    var cfg = allConfig[k];
                    if (cfg.type == itemType) {
                        role.getItemsPart().addItem(cfg.id, itemNum);
                    }
                }
                break;
            case GMCommand.CMD_SET_LEVEL:
                var level = parseInt(msgs[1]);
                if (isNaN(level)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 等级。 等级必须是数字。");
                    role.send(msgObj);
                    return;
                }
                if (level < globalDefine.MIN_ROLE_LEVEL || level > globalDefine.MAX_ROLE_LEVEL) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "等级必须是" + globalDefine.MIN_ROLE_LEVEL + "和" + globalDefine.MAX_ROLE_LEVEL + "之间。");
                    role.send(msgObj);
                    return;
                }
                role.setNumber(enProp.level, level);
                //通知全局服更新登录时间
                role.updateGlobalServerInfo();
                role.fireEvent(eventNames.LEVEL_UP, level);
                break;
            case GMCommand.CMD_RESET_LEVEL:
                role.getLevelsPart().clearLevelInfo();
                break;
            case GMCommand.CMD_OPEN_LEVEL:
                var num = parseInt(msgs[1]) || 0;
                role.getLevelsPart().openLevel(num);
                break;
            case GMCommand.CMD_OPEN_ELITE_LEVEL:
                var num = parseInt(msgs[1]) || 0;
                if(!eliteLevelConfig.getEliteLevelCfg(num))
                {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "提供的关卡Id无效");
                    role.send(msgObj);
                    return;
                }
                var eliteLevel = role.getEliteLevelsPart().getEliteLevelByLevelId(num);
                if(eliteLevel==null)
                {
                    eliteLevel = role.getEliteLevelsPart().addEliteLevelWithLevelId(num);
                }
                if(!eliteLevel.passed)
                {
                    eliteLevel.passed = true;
                    eliteLevel.syncAndSave();
                }
                break;
            case GMCommand.CMD_SET_TIME:
                //必须是time 年 月 日 [时 [分 [秒]]] 或 time t 时 [分 [秒]]
                if ((msgs[1] === "t" && msgs.length < 3) || (msgs[1] !== "t" && msgs.length < 4)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式1：" + cmd + " 年 月 日 [时 [分 [秒]]] \r\n格式2： " + cmd + " t 时 [分 [秒]] \r\n服务器时间：" + dateUtil.getDateString());
                    role.send(msgObj);
                    return;
                }

                var year;
                var month;
                var day;
                var hour;
                var minute;
                var second;
                var curDate = dateUtil.getDate();
                if (msgs[1] === "t") {
                    year = curDate.getFullYear();
                    month = curDate.getMonth() + 1;
                    day = curDate.getDate();
                    hour = msgs[2] != undefined ? parseInt(msgs[2]) : curDate.getHours();
                    minute = msgs[3] != undefined ? parseInt(msgs[3]) : curDate.getMinutes();
                    second = msgs[4] != undefined ? parseInt(msgs[4]) : curDate.getSeconds();
                }
                else {
                    year = msgs[1] != undefined ? parseInt(msgs[1]) : curDate.getFullYear();
                    month = msgs[2] != undefined ? parseInt(msgs[2]) : curDate.getMonth() + 1;
                    day = msgs[3] != undefined ? parseInt(msgs[3]) : curDate.getDate();
                    hour = msgs[4] != undefined ? parseInt(msgs[4]) : curDate.getHours();
                    minute = msgs[5] != undefined ? parseInt(msgs[5]) : curDate.getMinutes();
                    second = msgs[6] != undefined ? parseInt(msgs[6]) : curDate.getSeconds();
                }

                var dateObj = new Date(year, month - 1, day, hour, minute, second);
                var newTime = dateUtil.getTimestampFromDate(dateObj);
                if (newTime <= 0)
                {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "时间似乎有问题，导致时间戳小于等于0了");
                    role.send(msgObj);
                    return;
                }

                dateUtil.setTimeFromTimestamp(newTime);
                sessionMgr.syncVirtualTimeToAll();
                break;
            case GMCommand.CMD_RESET_GOLD_LV:
                {
                    let actPart = role.getActivityPart();
                    actPart.startBatch();
                    actPart.setNumber(enActProp.goldLvlTime, dateUtil.getTimestamp() - goldLevelConfig.getGoldLevelBasicCfg().coolDown);
                    actPart.setNumber(enActProp.goldLvlCnt, 0);
                    if (msgs[1] != undefined) {
                        let goldLvlMax = parseInt(msgs[1]);
                        if (!isNaN(goldLvlMax))
                            actPart.setNumber(enActProp.goldLvlMax, goldLvlMax);
                    }
                    actPart.endBatch();
                }
                break;
            case GMCommand.CMD_RESET_HADES_LV:
                {
                    let actPart = role.getActivityPart();
                    actPart.startBatch();
                    actPart.setNumber(enActProp.hadesLvlCnt, 0);
                    if (msgs[1] != undefined) {
                        let hadesLvlMax = parseInt(msgs[1]);
                        if (!isNaN(hadesLvlMax))
                            actPart.setNumber(enActProp.hadesLvlMax, hadesLvlMax);
                    }
                    actPart.endBatch();
                }
                break;
            case GMCommand.CMD_RESET_GUARD_LV:
            {
                let actPart = role.getActivityPart();
                actPart.startBatch();
                actPart.setNumber(enActProp.guardLvlTime, dateUtil.getTimestamp() - guardLevelConfig.getGuardLevelBasicCfg().coolDown);
                actPart.setNumber(enActProp.guardLvlCnt, 0);
                if (msgs[1] != undefined) {
                    let guardLvlMax = parseInt(msgs[1]);
                    if (!isNaN(guardLvlMax))
                        actPart.setNumber(enActProp.guardLvlMax, guardLvlMax);
                }
                actPart.endBatch();
            }
                break;
            case GMCommand.CMD_RESET_DATA:
                var db = dbUtil.getDB(role.getUserId());
                var col = db.collection("role");
                col.deleteOneNoThrow({"props.heroId": role.getHeroId()});
                break;
            case GMCommand.CMD_QUICK_TEST:
                role.setNumber(enProp.level, 30);
                role.getItemsPart().addItem(enItemId.GOLD, 100000);
                role.getItemsPart().addItem(enItemId.DIAMOND, 1000);
                break;
            case GMCommand.CMD_RESET_ARENA:
                {
                    let actPart = role.getActivityPart();
                    actPart.startBatch();
                    actPart.setNumber(enActProp.arenaTime, 0);
                    actPart.setNumber(enActProp.arenaCnt, 0);
                    actPart.endBatch();
                }
                break;
            case GMCommand.CMD_RESET_TREASURE_ROB:
            {
                let actPart = role.getActivityPart();
                actPart.startBatch();
                actPart.setNumber(enActProp.treasureRobedCnt, 0);
                actPart.setNumber(enActProp.treasureCnt, 0);
                actPart.endBatch();
            }
                break;
            case GMCommand.CMD_ADD_PET:
                var petId = msgs[1];
                var roleCfg = roleConfig.getRoleConfig(petId);
                if (!roleCfg || roleCfg.roleType != 2) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "无ID为" + petId + "的角色或角色不是宠物。");
                    role.send(msgObj);
                    return;
                }

                // 检查是否存在此宠物
                if (role.getPetsPart().getPetByRoleId(petId)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "ID为" + petId + "的宠物已存在，不能重复添加。");
                    role.send(msgObj);
                    return;
                }

                role.getPetsPart().addPet(petId, 1);
                break;
            case GMCommand.CMD_ADD_MAIL://作弊加邮件测试
                var heroIds = [];
                if (msgs[2] > 0)//给多账号发邮件
                {
                    let heronum = msgs.length - 2;
                    for (let i = 0; i < heronum; ++i) {
                        heroIds.push(parseInt(msgs[2 + i]));
                    }
                }
                else//只给自己发
                    heroIds.push(role.getHeroId());
                var mail;
                if (msgs[1] == 0)//无附件邮件
                {
                    mail = MailMgr.createMail(dateUtil.getTimestamp() + "普通邮件", "GM", "这是一封没附件的测试邮件，测试的内容我不告诉你嘿嘿这是一封没附件的测试邮件，测试的内容我不告诉你嘿嘿", []);
                    MailMgr.sendMailToMultiRole(heroIds, mail);
                }
                else if (msgs[1] == 1) {
                    var items = [];
                    items.push(itemModule.createItem({itemId: 100003, num: 5}));
                    items.push(itemModule.createItem({itemId: 100005, num: 6}));
                    items.push(itemModule.createItem({itemId: 100204, num: 10}));
                    items.push(itemModule.createItem({itemId: 100308, num: 12}));
                    items.push(itemModule.createItem({itemId: 110006, num: 15}));
                    items.push(itemModule.createItem({itemId: 110006, num: 15}));
                    items.push(itemModule.createItem({itemId: 110006, num: 15}));
                    items.push(itemModule.createItem({itemId: 100204, num: 10}));
                    items.push(itemModule.createItem({itemId: 100308, num: 12}));
                    items.push(itemModule.createItem({itemId: 100005, num: 6}));
                    mail = MailMgr.createMail(dateUtil.getTimestamp() + "附件邮件", "GM", "这是一封有附件的邮件，你快点收下吧！这是一封有附件的邮件，你快点收下吧！", items);
                    MailMgr.sendMailToMultiRole(heroIds, mail);
                }
                break;
            case GMCommand.CMD_RESET_CHECK_IN:
                {
                    var opActsPart = role.getOpActivityPart();
                    opActsPart.setNumber(enOpActProp.lastCheckIn, opActsPart.getNumber(enOpActProp.lastCheckIn) - 86400);
                }
                break;
            case GMCommand.CMD_SET_SVR_NUM_PROP:
                {
                    if (msgs.length < 3)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 属性名 数字");
                        role.send(msgObj);
                        return;
                    }

                    let propName = msgs[1];
                    let propId = enProp[propName];
                    if (propId == undefined)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "属性名" + propName + "不存在");
                        role.send(msgObj);
                        return;
                    }

                    let numVal = parseInt(msgs[2], 10);
                    if (isNaN(numVal))
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "属性值" + propVal + "必须是数字");
                        role.send(msgObj);
                        return;
                    }

                    role.setNumber(propId, numVal);

                    let propVal = role.getString(propId);
                    msgObj.setResponseWithMsg(ResultCode.SUCCESS, "属性名：" + propName + "，属性值：" + propVal);
                    role.send(msgObj);
                    return;
                }
                break;
            case GMCommand.CMD_SET_SVR_STR_PROP:
                {
                    if (msgs.length < 3)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 属性名 字符串");
                        role.send(msgObj);
                        return;
                    }

                    let propName = msgs[1];
                    let propId = enProp[propName];
                    if (propId == undefined)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "属性名" + propName + "不存在");
                        role.send(msgObj);
                        return;
                    }

                    role.setString(propId, msgs[2]);

                    let propVal = role.getString(propId);
                    msgObj.setResponseWithMsg(ResultCode.SUCCESS, "属性名：" + propName + "，属性值：" + propVal);
                    role.send(msgObj);
                    return;
                }
                break;
            case GMCommand.CMD_ADD_SVR_NUM_PROP:
                {
                    if (msgs.length < 3)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 属性名 数字");
                        role.send(msgObj);
                        return;
                    }

                    let propName = msgs[1];
                    let propId = enProp[propName];
                    if (propId == undefined)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "属性名" + propName + "不存在");
                        role.send(msgObj);
                        return;
                    }

                    let numVal = parseInt(msgs[2], 10);
                    if (isNaN(numVal))
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "属性值" + propVal + "必须是数字");
                        role.send(msgObj);
                        return;
                    }

                    if (propId == enProp.exp)
                        role.addExp(numVal);
                    else
                        role.addNumber(propId, numVal);

                    let propVal = role.getString(propId);
                    msgObj.setResponseWithMsg(ResultCode.SUCCESS, "属性名：" + propName + "，属性值：" + propVal);
                    role.send(msgObj);
                    return;
                }
                break;
            case GMCommand.CMD_GET_SVR_PROP:
                {
                    if (msgs.length < 2)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 属性名");
                        role.send(msgObj);
                        return;
                    }

                    let propName = msgs[1];
                    let propId = enProp[propName];
                    if (propId == undefined)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "属性名" + propName + "不存在");
                        role.send(msgObj);
                        return;
                    }

                    let propVal = role.getString(propId);
                    msgObj.setResponseWithMsg(ResultCode.SUCCESS, "属性名：" + propName + "，属性值：" + propVal);
                    role.send(msgObj);
                    return;
                }
                break;
            case GMCommand.CMD_SET_TEACH_DATA:
                {
                    if (msgs.length < 3)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 键名 数字");
                        role.send(msgObj);
                        return;
                    }

                    let keyName = msgs[1];

                    let numVal = parseInt(msgs[2], 10);
                    if (isNaN(numVal))
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "键值" + numVal + "必须是数字");
                        role.send(msgObj);
                        return;
                    }

                    let part = role.getSystemsPart();
                    let ret = part.setTeachData(keyName, numVal, true);
                    switch (ret)
                    {
                        case 1:
                            msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "键名" + keyName + "格式不对");
                            role.send(msgObj);
                            return;
                        case 2:
                            msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "键值" + numVal + "必须是数字");
                            role.send(msgObj);
                            return;
                        case 3:
                            msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "引导键数已达上限了");
                            role.send(msgObj);
                            return;
                    }
                }
                break;
            case GMCommand.CMD_CLEAR_TEACH_DATA:
                {
                    let part = role.getSystemsPart();
                    part.clearTeachData(true);
                }
                break;
            case GMCommand.CMD_TEST_MODE:
                {
                    let sysPart = role.getSystemsPart();
                    sysPart.setTeachData("atk_op", 1, true);
                    sysPart.setTeachData("skill_op", 1, true);
                    sysPart.setTeachData("qte_op", 1, true);
                    sysPart.setTeachData("block_op", 1, true);

                    let systemCfgs = systemConfig.getSystemConfig();
                    for(var k in systemCfgs)
                    {
                        var systemId = parseInt(k);
                        var system = sysPart.getSystemBySystemId(systemId);
                        if(!system)
                        {
                            sysPart.addSystemWithSystemId(systemId,true,false)
                        }
                        else
                        {
                            system.active = true;
                            system.syncAndSave();
                        }
                    }

                    let itemPart = role.getItemsPart();
                    let items = valueConfig.getTestItemConfig();
                    for (let k in items)
                    {
                        let item = items[k];
                        itemPart.addItem(item.itemId, item.itemNum);
                    }
                }
                break;
            case GMCommand.CMD_START_ARENA:
                {
                    let opHeroId = parseInt(msgs[1]);
                    if (isNaN(opHeroId))
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 主角ID");
                        role.send(msgObj);
                        return;
                    }

                    if (opHeroId == role.getHeroId())
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "不允许填自己的主角ID");
                        role.send(msgObj);
                        return;
                    }

                    var actPart = role.getActivityPart();
                    var serverData = actPart.getServerData();
                    var roleOp = yield roleMgr.findRoleOrLoadOfflineByHeroId(opHeroId);
                    if (roleOp) {
                        //保存对手的主角ID
                        serverData.arenaCurCHeroId = opHeroId;

                        var respMsg = Message.newResponse(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_START_CHALLENGE, ResultCode.SUCCESS, null, roleOp.getProtectNetData());
                        role.send(respMsg);
                    }
                    else {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "找不到角色");
                        role.send(msgObj);
                    }
                }
                break;
            case GMCommand.CMD_SEND_SYS_CHAT:
                {
                    let opHeroId = parseInt(msgs[1]);
                    if (isNaN(opHeroId) || msgs.length < 3)
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 主角ID 消息内容");
                        role.send(msgObj);
                        return;
                    }

                    chatMgr.sendSystemChatMsg(msgs[2], opHeroId);
                }
                break;
            case GMCommand.CMD_RECHARGE:
                var diamondNum = parseInt(msgs[1]);
                if (isNaN(diamondNum)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "格式：" + cmd + " 充值砖石数量。  砖石数量必须是数字。");
                    role.send(msgObj);
                    return;
                }
                role.getItemsPart().addDiamond(diamondNum);
                role.getOpActivityPart().startBatch();
                role.getOpActivityPart().addNumber(enOpActProp.totalRecharge,diamondNum);
                role.getOpActivityPart().updateVipLv();
                role.getOpActivityPart().endBatch();
                break;
            case GMCommand.CMD_CLEAR_JOINT_CORPS_CD:
                role.getCorpsPart().setQuitCorpsTime(0);
                break;
            case GMCommand.CMD_FINISH_WARRIOR:
                if(msgs[1] >= 0)
                {
                    role.getActivityPart().doEndWarriorWin(msgs[1]);
                    var netMsg = new activityMessage.WarriorTriedDataRes(role.getActivityPart().getWarriorTriedData(), false);
                    role.sendEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.CMD_REQ_WARRIOR_TRIED, netMsg);
                }
                break;
            case GMCommand.CMD_ADD_TREASURE:
                var treasureId = parseInt(msgs[1]);
                var treasureCfg = treasureConfig.getTreasureConfig(treasureId);
                if (!treasureId) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "无ID为" + treasureId + "的神器。");
                    role.send(msgObj);
                    return;
                }

                // 检查是否存在此宠物
                if (role.getTreasurePart().getTreasure(treasureId)) {
                    msgObj.setResponseWithMsg(ResultCodeGm.GM_EXECUTE_ERROR, "ID为" + treasureId + "的神器已存在，不能重复添加。");
                    role.send(msgObj);
                    return;
                }

                role.getTreasurePart().addTreasure(treasureId);
                break;

            case GMCommand.CMD_OPEN_ICON:
                {
                    var systemId = parseInt(msgs[1]);
                    if (isNaN(systemId))
                    {
                        msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "请输入系统枚举的整数值");
                        role.send(msgObj);
                        return;
                    }

                    let sysPart = role.getSystemsPart();
                    var system = sysPart.getSystemBySystemId(systemId);
                    if(!system)
                    {
                        sysPart.addSystemWithSystemId(systemId,true,false)
                    }
                    else
                    {
                        system.active = true;
                        system.syncAndSave();
                    }
                }
                break;

            default:
                msgObj.setResponseWithMsg(ResultCodeGm.GM_WRONG_FORMAT, "未知GM命令。");
                role.send(msgObj);
                return;
        }

        msgObj.setResponseData(ResultCode.SUCCESS);
        role.send(msgObj);
    }
);

function processGmCmd(session, role, msgObj, processGmCmdReq) {
    processGmCmdCoroutine(session, role, msgObj, processGmCmdReq).catch(function (err) {
        logUtil.error("gmHandler~processGmCmd", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_GM, CmdIdsGm.CMD_PROCESS_GM_CMD, processGmCmd, gmMessage.ProcessGmCmdVo);