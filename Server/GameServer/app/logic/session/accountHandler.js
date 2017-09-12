"use strict";

var Promise = require("bluebird");

var appCfg = require("../../../config");
var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var eventMgr = require("../../libs/eventMgr");
var eventNames = require("../enumType/eventDefine").eventNames;
var handlerMgr = require("./handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var accountMsg = require("../netMessage/accountMessage");
var CmdIdsAccount = require("../netMessage/accountMessage").CmdIdsAccount;
var ResultCodeAccount = require("../netMessage/accountMessage").ResultCodeAccount;
var sessionMgr = require("./sessionMgr");
var roleMgr = require("../role/roleMgr");
var onlineRoleMgr = require("../role/onlineRoleMgr");
var offlineRoleMgr = require("../role/offlineRoleMgr");
var enProp = require("../enumType/propDefine").enProp;
var globalServerAgent = require("../http/globalServerAgent");
var adminServerAgent = require("../http/adminServerAgent");
var guidGenerator = require("../../libs/guidGenerator");
var equipUtil = require("../equip/equipUtil");
var roleConfig = require("../gameConfig/roleConfig");
var valueConfig = require("../gameConfig/valueConfig");
var petSkillModule = require("../pet/petSkill");
var talentModule = require("../pet/talent");
var itemUtil = require("../item/itemUtil");

const HERO_NAME_MAX_LEN = 20;   //主角名限制长度，单位：字符

var loginCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
     * @param {Role} role - 在这里role是空的，还没有role
     * @param {Message} msgObj
     * @param {LoginRequestVo} loginReq
     */
    function * (session, role, msgObj, loginReq) {
        var channelId = loginReq.channelId;
        var userId = loginReq.userId;
        var token = loginReq.token;

        //验证登录
        var checkRes = yield globalServerAgent.checkLogin(channelId, userId, token);
        logUtil.debug("accountHandler~login, ok:" + checkRes.ok + ", msg:" + checkRes.msg);

        //token验证失败就踢下线
        if (!checkRes.ok)
        {
            msgObj.setResponseWithMsg(ResultCodeAccount.CHECK_TOKEN_FAIL, checkRes.msg);
            session.sendThenKick(msgObj);
            return;
        }

        //差不多可以把另一个已登录的同一个账号踢下去了
        var oldSession = sessionMgr.getSessionByAccount(channelId, userId);
        if (oldSession)
        {
            logUtil.debug("accountHandler~login，发现已存在的账号，渠道：" + channelId + "，用户ID：" + userId);
            oldSession.kickSession("账号在别处登录，您被执行下线操作", true);
        }

        //根据用户ID的哈希获取数据库连接
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");

        //获取本服角色列表
        var dbRes = yield col.findArray({"props.channelId":channelId, "props.userId":userId}, {_id:0, "props.guid":1, "props.name":1, "props.level":1, "props.roleId":1, "props.heroId":1});

        //标记为登录成功
        session.setAuth(checkRes.ok);

        //把本会话的账号信息添加到会话管理
        session.setAccountInfo(channelId, userId, loginReq.network, loginReq.osName, loginReq.clientVer, loginReq.lang,
            loginReq.deviceModel, loginReq.root, loginReq.macAddr, loginReq.screenWidth, loginReq.screenHeight);

        var roleList = [];
        for (var i = 0; i < dbRes.length; ++i)
        {
            var props = dbRes[i].props;
            roleList.push(new accountMsg.RoleBriefVo(props.guid, props.name, props.level, props.roleId, props.heroId));
        }

        //再下发角色列表
        let retObj = new accountMsg.RoleListVo(roleList);
        msgObj.setResponseData(ResultCode.SUCCESS, retObj);
        session.send(msgObj);

        logUtil.debug("登录成功，channelId：" + channelId + "，userId：" + userId);
    }
);

function login(session, role, msgObj, loginReq)
{
    loginCoroutine(session, role, msgObj, loginReq).catch (function (err) {
        logUtil.error("accountHandler~login", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        session.sendThenKick(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_LOGIN, login, accountMsg.LoginRequestVo, true);

var reloginCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
     * @param {Role} role - 在这里role是空的，还没有role
     * @param {Message} msgObj
     * @param {ReloginRequestVo} reloginReq
     */
    function * (session, role, msgObj, reloginReq) {
        //虽然是自动重新登录，但还是要验证账号
        var checkRes = yield globalServerAgent.checkLogin(reloginReq.channelId, reloginReq.userId, reloginReq.token);
        logUtil.debug("accountHandler~relogin, ok:" + checkRes.ok + ", msg:" + checkRes.msg);

        if (!checkRes.ok)
        {
            msgObj.setResponseWithMsg(ResultCodeAccount.CHECK_TOKEN_FAIL, checkRes.msg);
            session.sendThenKick(msgObj);
            return;
        }

        //////////这个判断不要了，因为现在的连接可能是旧连接，服务器还不知道网络已断//////////
        ////本账号在另外一个地方登录了？本会话优先级更低。
        //var oldSession = sessionMgr.getSessionByAccount(reloginReq.channelId, reloginReq.userId);
        //if (oldSession)
        //{
        //    msgObj.setResponseWithMsg(ResultCodeAccount.CHECK_TOKEN_FAIL, "账号已在别处登录");
        //    session.sendThenKick(msgObj);
        //    return;
        //}

        //看看Role是否在在线列表
        role = onlineRoleMgr.findRoleByHeroId(reloginReq.heroId);
        if (!role)
        {
            //role不在了？不在？踢出去
            msgObj.setResponseData(ResultCodeAccount.RELOGIN_FAIL);
            session.sendThenKick(msgObj);
            return;
        }

        //这个role居然不是这个账号的？踢出去
        //另外有可能这个role上次登录的时间跟客户端的时间不一样，那可能是这上客户端离线时，又有另一个客户端上线又离线了，导致有一个不属于前者的离线role
        if (role.getString(enProp.channelId) !== reloginReq.channelId || role.getString(enProp.userId) !== reloginReq.userId || role.getString(enProp.lastLogin) !== reloginReq.lastLogin)
        {
            msgObj.setResponseData(ResultCodeAccount.RELOGIN_FAIL);
            session.sendThenKick(msgObj);
            return;
        }

        //怎么角色还有网络连接？是不是服务端不知道旧连接已断？
        var oldSession2 = role.getSession();
        if (oldSession2)
        {
            //把role剥离旧连接
            oldSession2.setRole(null);
            //这个连接踢掉
            oldSession2.kickSession(null, false);
        }

        //标记为登录成功
        session.setAuth(checkRes.ok);

        //把本会话的账号信息添加到会话管理
        session.setAccountInfo(reloginReq.channelId, reloginReq.userId, reloginReq.network, reloginReq.osName, reloginReq.clientVer, reloginReq.lang,
            reloginReq.deviceModel, reloginReq.root, reloginReq.macAddr, reloginReq.screenWidth, reloginReq.screenHeight);

        //把role与本会话关联起来
        session.setRole(role);

        //先发送执行成功消息
        msgObj.setResponseData(ResultCode.SUCCESS);
        session.send(msgObj);

        //重发没发成功的网络消息
        role.sendPendingMsgList();

        //发出一个角色重新登录事件
        role.fireEvent(eventNames.ROLE_RELOGIN);

        logUtil.debug("自动重登成功，channelId：" + reloginReq.channelId + "，userId：" + reloginReq.userId + "，heroId：" + reloginReq.heroId);
    }
);

function relogin(session, role, msgObj, reloginReq)
{
    reloginCoroutine(session, role, msgObj, reloginReq).catch (function (err){
        logUtil.error("accountHandler~relogin", err);
        msgObj.setResponseData(ResultCode.SERVER_ERROR);
        session.sendThenKick(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_RELOGIN, relogin, accountMsg.ReloginRequestVo, true);

var createRoleCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
     * @param {Role} role - 在这里role是空的，还没有role
     * @param {Message} msgObj
     * @param {CreateRoleRequestVo} createReq
     */
    function * (session, role, msgObj, createReq) {
        //请求全局服生成角色实例ID、插入角色数据到全局服的角色列表
        var accountInfo = session.getAccountInfo();
        var channelId = accountInfo.channelId;
        var userId = accountInfo.userId;
        var guid = guidGenerator.generateGUID();
        var name = createReq.name;
        var level = 1;
        var roleId = createReq.roleId;
        var serverId = appCfg.serverId;

        //请求失败？
        if (name.length > HERO_NAME_MAX_LEN)
        {
            msgObj.setResponseWithMsg(ResultCode.BAD_PARAMETER, "主角名不能超过" + HERO_NAME_MAX_LEN + "个字");
            session.send(msgObj);
            return;
        }

        var reqRes = yield globalServerAgent.requestNewRole(channelId, userId, guid, name, roleId, serverId, level);

        //请求失败？
        if (!reqRes.ok)
        {
            msgObj.setResponseWithMsg(ResultCode.SERVER_ERROR, reqRes.msg);
            session.send(msgObj);
            return;
        }

        //返回的数据不对？
        if (!reqRes.cxt || !Object.isNumber(reqRes.cxt.heroId))
        {
            logUtil.error("accountHandler~createRole，得到heroId为空");
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
            session.send(msgObj);
            return;
        }

        //插入新角色数据
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");

        //构造初始数据
        var curTime = dateUtil.getTimestamp();
        var heroId = reqRes.cxt.heroId;
        var itemProps = {};
        var initItems = itemUtil.getInitItems(roleId, itemProps);

        var initData = {
            props: {
                channelId:channelId,
                userId:userId,
                heroId:heroId,
                guid:guid,
                createTime:curTime,
                roleId:roleId,
                name:name,
                level:level,
                exp:0,
                stamina: parseInt(valueConfig.getConfigValueConfig("Stamina")["value"]),
                staminaTime: dateUtil.getTimestamp(),
                gold:0
            },
            pets: [/*{
                    props: {
                        guid:guidGenerator.generateGUID(),
                        createTime:curTime,
                        roleId:"cw_6",
                        name:roleConfig.getRoleConfig("cw_6").name,
                        level:level,
                        exp:0,
                        star:1,
                        advLv:1
                    },
                    equips: equipUtil.getInitEquips("cw_6"),
                    petSkills: petSkillModule.getInitPetSkills("cw_6"),
                    talents: talentModule.getInitTalents("cw_6")
                },
                {
                    props: {
                        guid:guidGenerator.generateGUID(),
                        createTime:curTime,
                        roleId:"cw_8",
                        name:roleConfig.getRoleConfig("cw_8").name,
                        level:level,
                        exp:0,
                        star:1,
                        advLv:1
                    },
                    equips: equipUtil.getInitEquips("cw_8"),
                    petSkills: petSkillModule.getInitPetSkills("cw_8"),
                    talents: talentModule.getInitTalents("cw_8")
                }*/
            ],
            equips: equipUtil.getInitEquips(roleId),
            items: initItems,
            levelInfo: {
                curLevel: "0",
                curNode: "0",
                starsReward: {},
                levels: {}
            },
            weapons:{curWeapon:3},
            //systems:[],
            //新号登录邮件
            mails:[
                {
                    "mailId" : "a4_154dcef5e83_new",
                    "title" : "系统邮件",
                    "sender" : "智慧女神雅典娜",
                    "status" : 0,
                    "sendTime" : dateUtil.getTimestamp(),
                    "content" : "尊敬的玩家：欢迎您来到《战神—远征》的世界，在这里我们将竭诚为您提供最优质的游戏体验。现在属于您的战神传奇开始了，请释放您的热血和杀欲吧！",
                    "attach" : []
                }
            ],
            treasures: {
                "battleTreasures":[],
                "treasures":{},
            },
            //social:{},
            teaches:{atk_op:1},
        };

        //如果初始物品里有虚拟物品，那就设置为属性
        for (var key in itemProps)
        {
            initData.props[key] = itemProps[key];
        }

        //执行插入操作
        var dbRes = yield col.insertOne(initData);
        if (dbRes <= 0)
        {
            msgObj.setResponseData(ResultCode.DB_ERROR);
            session.send(msgObj);
            return;
        }

        //重新读取角色列表
        dbRes = yield col.findArray({"props.channelId":channelId, "props.userId":userId}, {_id:0, "props.guid":1, "props.name":1, "props.level":1, "props.roleId":1, "props.heroId":1});

        var roleList = [];
        for (var i = 0; i < dbRes.length; ++i)
        {
            var props = dbRes[i].props;
            roleList.push(new accountMsg.RoleBriefVo(props.guid, props.name, props.level, props.roleId, props.heroId));
        }

        //再下发角色列表
        let retObj = new accountMsg.RoleListVo(roleList);
        msgObj.setResponseData(ResultCode.SUCCESS, retObj);
        session.send(msgObj);

        //记录统计日志
        var log_ip = session.getRemoteFamily() === "IPv4" ? session.getRemoteAddress() : "";
        var log_ipv6 = session.getRemoteFamily() === "IPv6" ? session.getRemoteAddress() : "";
        var log_device_model = accountInfo.deviceModel;
        var log_os_name = accountInfo.osName;
        var log_os_ver = "";
        var log_mac_addr = accountInfo.macAddr;
        var log_udid = "";
        var log_app_channel = accountInfo.channelId;
        var log_app_ver = accountInfo.clientVer;
        var log_account_id = userId;
        var log_role_id = heroId.toString();
        var log_role_name = name;
        var log_create_time = dateUtil.getTimestamp() * 1000;
        adminServerAgent.logCreateRole(log_ip, log_ipv6, log_device_model, log_os_name, log_os_ver, log_mac_addr, log_udid, log_app_channel, log_app_ver, log_account_id, log_role_id, log_role_name, log_create_time);

        logUtil.debug("创建角色成功，channelId：" + channelId + "，userId：" + userId + "，heroId：" + heroId + "，name：" + name);
    }
);

function createRole(session, role, msgObj, createReq)
{
    createRoleCoroutine(session, role, msgObj, createReq).catch (function (err){
        logUtil.error("accountHandler~createRole", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        session.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_CREATE_ROLE, createRole, accountMsg.CreateRoleRequestVo);

var activateRoleCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
     * @param {Role} role - 在这里role是空的，还没有role
     * @param {Message} msgObj
     * @param {ActivateRoleRequestVo} activateReq
     */
    function * (session, role, msgObj, activateReq) {
        var accountInfo = session.getAccountInfo();
        var channelId = accountInfo.channelId;
        var userId = accountInfo.userId;
        var heroId = activateReq.heroId;

        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        //为了防止请求别人的角色了，这里带上自己的渠道号和用户ID
        var roleData = yield col.findOne({"props.heroId":heroId, "props.channelId":channelId, "props.userId":userId});
        if (!roleData)
        {
            msgObj.setResponseData(ResultCode.ROLE_DATA_LOST);
            session.send(msgObj);
            return;
        }

        //看看Role是否在在线列表，如果已存在，考虑删除它
        role = onlineRoleMgr.findRoleByHeroId(heroId);
        if (role)
        {
            //如果有连接就踢连接
            var oldSession = role.getSession();
            if (oldSession)
                //oldSession.kickSession会调用sessionMgr.delSession
                //sessionMgr.delSession会调用role.release
                //role.release会调用roleMgr.removeRoleByGUID
                //于是role被删除了
                oldSession.kickSession("账号在别处登录，您被执行下线操作", true);
            else
                //role.release会调用roleMgr.removeRoleByGUID
                //于是role被删除了
                role.release();
        }
        else {
            //看看Role是否在离线列表，如果已存在，考虑删除它
            role = offlineRoleMgr.findRoleByHeroId(heroId);
            if (role)
                role.release();
        }

        role = roleMgr.createRole(roleData);
        //居然创建失败？
        if (!role)
        {
            logUtil.error("创建角色失败，角色数据：" + JSON.stringify(roleData));
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
            session.send(msgObj);
            return;
        }
        session.setRole(role);

        //开启批量修改
        role.startBatch();
        var curTime = dateUtil.getTimestamp();
        //修改最后登录时间
        role.setNumber(enProp.lastLogin, curTime, true);
        //登录次数
        role.addNumber(enProp.loginCount, 1, true);
        //修改完了
        role.endBatch();

        var retObj = role.getPrivateNetData();
        msgObj.setResponseData(ResultCode.SUCCESS, retObj);
        session.send(msgObj);

        //发出一个角色登录事件
        role.fireEvent(eventNames.ROLE_LOGIN);

        logUtil.debug("激活角色成功，channelId：" + channelId + "，userId：" + userId + "，heroId：" + heroId);

        //下发服务器时间
        session.syncVirtualTime();

        //通知全局服更新登录时间
        role.updateGlobalServerInfo();

        //记录统计日志
        var log_ip = session.getRemoteFamily() === "IPv4" ? session.getRemoteAddress() : "";
        var log_ipv6 = session.getRemoteFamily() === "IPv6" ? session.getRemoteAddress() : "";
        var log_device_model = accountInfo.deviceModel;
        var log_os_name = accountInfo.osName;
        var log_os_ver = "";
        var log_mac_addr = accountInfo.macAddr;
        var log_udid = "";
        var log_app_channel = accountInfo.channelId;
        var log_app_ver = accountInfo.clientVer;
        var log_account_id = userId;
        var log_role_id = heroId.toString();
        var log_role_name = role.getString(enProp.name);
        var log_create_time = role.getNumber(enProp.createTime) * 1000;
        var log_network = accountInfo.network;
        var log_isp = "";
        var log_device_height = accountInfo.screenHeight;
        var log_device_width = accountInfo.screenWidth;
        var log_role_level = role.getNumber(enProp.level);
        var log_login_time = dateUtil.getTimestamp() * 1000;
        var log_last_logout_time = role.getNumber(enProp.lastLogout) * 1000;
        var log_offline_money = role.getNumber(enProp.gold);
        var log_offline_exp = role.getNumber(enProp.exp);
        var log_root = accountInfo.root;
        var log_vip_level = role.getNumber(enProp.vipLv);
        adminServerAgent.logLoginRole(log_ip, log_ipv6, log_device_model, log_os_name, log_os_ver, log_mac_addr, log_udid, log_app_channel, log_app_ver, log_account_id, log_role_id, log_role_name, log_create_time,
            log_network, log_isp, log_device_height, log_device_width, log_role_level, log_login_time, log_last_logout_time, log_offline_money, log_offline_exp, log_root, log_vip_level);

        /////////////////////测试代码/////////////////
        //role.addNumber(enProp.loginCount, 1);
        ////开启批量修改
        //role.startBatch();
        ////修改最后登录时间
        //role.setNumber(enProp.lastLogin, curTime + 1);
        ////登录次数
        //role.addNumber(enProp.loginCount, 1);
        ////修改完了
        //role.endBatch();
        //var equipPart = role.getEquipsPart();
        //equipPart.addEquipWithData({equipId:10801, level:11});
        //equipPart.removeEquipByEquipId(10700);
        //
        //var itemPart = role.getItemsPart();
        //itemPart.addItemWithData({itemId:20003, num:10});
        //itemPart.removeItemByItemId(20002);
        //itemPart.addItems({100010:100,100011:100,100012:100,100013:100});
        //itemPart.costItems({100010:100,100011:100,100012:99,100013:99});
        //
        //var petPart = role.getPetsPart();
        //petPart.addPet("kuangzhanshi");
        //var pet = petPart.getPetByIndex(0);
        //petPart.removePetByGUID(pet.getGUID());
        //var pet = petPart.getPetByIndex(0);
        //pet.setNumber(enProp.exp, 1000);
        //var petEquipPart = pet.getEquipsPart();
        //petEquipPart.addEquipWithData({equipId:10801, level:11});
        //petEquipPart.removeEquipByEquipId(10700);
        //var enActProp = require("../enumType/activityPropDefine").enActProp;
        //var actPart = role.getActivityPart();
        //actPart.setNumber(enActProp.goldLvlTime, curTime);
        //actPart.addNumber(enActProp.goldLvlCnt, 1);
        //actPart.startBatch();
        //actPart.setNumber(enActProp.goldLvlTime, curTime + 1);
        //actPart.addNumber(enActProp.goldLvlCnt, 1);
        //actPart.endBatch();

        //if (session.getSessionId() === 1)
        //{
        //    logUtil.debug("开始发送测试消息");
        //    for (var i = 0; i < 10; ++i)
        //    {
        //        retObj = new accountMsg.LoginTipMsgVo(i.toString());
        //        role.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_TIP_MSG, retObj);
        //    }
        //    logUtil.debug("结束发送测试消息");
        //}
        /////////////////////测试代码/////////////////
    }
);

function activateRole(session, role, msgObj, activateReq)
{
    activateRoleCoroutine(session, role, msgObj, activateReq).catch (function (err){
        logUtil.error("accountHandler~activateRole", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        session.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_ACTIVATE_ROLE, activateRole, accountMsg.ActivateRoleRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {null} bodyObj - 没有body
 */
function logout(session, role, msgObj, bodyObj)
{
    //客户端要主动退出？那就不保留Role了
    //不设置消息，免得客户端提示
    session.kickSession(null, false);

    var accountInfo = session.getAccountInfo();
    if (accountInfo)
        logUtil.debug("用户下线，channelId：" + accountInfo.channelId + "，userId：" + accountInfo.userId);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_LOGOUT, logout);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {*} bodyObj - 内容任意
 */
function testEcho(session, role, msgObj, bodyObj)
{
    msgObj.setResponseData(ResultCode.SUCCESS, bodyObj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_TEST_ECHO, testEcho);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {null} bodyObj
 */
function reqServerTime(session, role, msgObj, bodyObj)
{
    var curTime = dateUtil.getTimestamp();
    var tzOffset = dateUtil.getTimezoneOffset();
    return new accountMsg.SyncServerTime(curTime, tzOffset);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_SERVER_TIME, reqServerTime);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {null} bodyObj
 */
function onPingReq(session, role, msgObj, bodyObj)
{
    return ResultCode.SUCCESS;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_PING, onPingReq);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {GetDemoHeroDataReq} reqObj
 */
function onGetDemoHeroData(session, role, msgObj, reqObj)
{
    var roleId = reqObj.roleId;
    var roleCfg = roleConfig.getRoleConfig(roleId);
    if (!roleCfg)
        return ResultCode.BAD_PARAMETER;

    var accountInfo = session.getAccountInfo();
    var channelId = accountInfo.channelId;
    var userId = accountInfo.userId;
    var heroId = -1;
    var guid = guidGenerator.generateGUID();
    var curTime = dateUtil.getTimestamp();
    var name = roleCfg.name;
    var level = 1;

    var roleData = {
        props: {
            channelId:channelId,
            userId:userId,
            heroId:heroId,
            guid:guid,
            createTime:curTime,
            roleId:roleId,
            name:name,
            level:level,
            stamina: parseInt(valueConfig.getConfigValueConfig("Stamina")["value"]),
            staminaTime: dateUtil.getTimestamp(),
        },
        equips: equipUtil.getInitEquips(roleId),
        weapons:{curWeapon:3},
    };

    return roleData;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_DEMO_HERO_DATA, onGetDemoHeroData);