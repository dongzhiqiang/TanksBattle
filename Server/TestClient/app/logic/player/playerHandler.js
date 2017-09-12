"use strict";

var appCfg = require("../../../config");
var appUtil = require("../../libs/appUtil");
var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var handlerMgr = require("../network/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var CmdIdsAccount = require("../netMessage/accountMessage").CmdIdsAccount;
var CmdIdsRole = require("../netMessage/roleMessage").CmdIdsRole;
var CmdIdsPet = require("../netMessage/petMessage").CmdIdsPet;
var CmdIdsItem = require("../netMessage/itemMessage").CmdIdsItem;
var CmdIdsEquip = require("../netMessage/equipMessage").CmdIdsEquip;
var CmdIdsGm = require("../netMessage/gmMessage").CmdIdsGm;
var CmdIdsActivity = require("../netMessage/activityMessage").CmdIdsActivity;
var ResultCodeAccount = require("../netMessage/accountMessage").ResultCodeAccount;
var accountMsg = require("../netMessage/accountMessage");
var roleMessage = require("../netMessage/roleMessage");
var petMessage = require("../netMessage/petMessage");
var itemMessage = require("../netMessage/itemMessage");
var equipMessage = require("../netMessage/equipMessage");
var activityMessage = require("../netMessage/activityMessage");

var testEchoTimeCostMap = {};

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {LoginTipMsgVo} tipMsg
 */
function onPushTipMsg(player, code, errMsg, tipMsg)
{
    player.logInfo(tipMsg.msg);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_TIP_MSG, onPushTipMsg, accountMsg.LoginTipMsgVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {ForceLogoutVo} context
 */
function onPushForceLogout(player, code, errMsg, context)
{
    player.logWarn("被踢下线，原因：" + context.msg);
    player.setNeedReconn(false);
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_FORCE_LOGOUT, onPushForceLogout, accountMsg.ForceLogoutVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {RoleListVo} roleList
 */
function onLogin(player, code, errMsg, roleList)
{
    if (code != ResultCode.SUCCESS) {
        switch (code) {
            case ResultCodeAccount.CHECK_TOKEN_FAIL:
                player.logWarn(errMsg);
                break;
            case ResultCode.DB_ERROR:
                player.logWarn("数据库错误");
                break;
            case ResultCode.SERVER_ERROR:
                player.logWarn("服务器错误");
                break;
            case ResultCode.BAD_PARAMETER:
                player.logWarn("参数错误");
                break;
            case ResultCode.BAD_REQUEST:
                player.logWarn("请求暂时无效");
                break;
            default:
                player.logWarn("未知错误，错误码：" + code);
                break;
        }
    }
    else {
        var reqObj;
        //没有角色？创建吧
        if (!roleList.roleList || roleList.roleList.length <= 0)
        {
            /**
             *
             * @type {CreateRoleRequestVo}
             */
            reqObj = {};
            reqObj.roleId = "kratos";
            reqObj.name = player.getUsername() + (appUtil.getRandom(0, 10000));
            player.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_CREATE_ROLE, reqObj);
        }
        else
        {
            var roleInfo = roleList.roleList[0];
            /**
             *
             * @type {ActivateRoleRequestVo}
             */
            reqObj = {};
            reqObj.heroId = roleInfo.heroId;
            player.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_ACTIVATE_ROLE, reqObj);
        }
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_LOGIN, onLogin, accountMsg.RoleListVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {RoleListVo} roleList
 */
function onCreateRole(player, code, errMsg, roleList)
{
    if (code != ResultCode.SUCCESS) {
        switch (code) {
            case ResultCode.DB_ERROR:
                player.logWarn("数据库错误");
                break;
            case ResultCode.SERVER_ERROR:
                player.logWarn("服务器错误");
                break;
            case ResultCode.BAD_PARAMETER:
                player.logWarn("参数错误");
                break;
            case ResultCode.BAD_REQUEST:
                player.logWarn("请求暂时无效");
                break;
            default:
                player.logWarn("未知错误，错误码：" + code);
                break;
        }
    }
    else {
        var roleInfo = roleList.roleList[0];

        /**
         *
         * @type {ActivateRoleRequestVo}
         */
        var reqObj = {};
        reqObj.heroId = roleInfo.heroId;
        player.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_ACTIVATE_ROLE, reqObj);
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_CREATE_ROLE, onCreateRole, accountMsg.RoleListVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {FullRoleInfoVo} roleInfo
 */
function onActivateRole(player, code, errMsg, roleInfo)
{
    if (code != ResultCode.SUCCESS) {
        switch (code) {
            case ResultCodeAccount.ROLE_DATA_LOST:
                player.logWarn("角色数据丢失");
                break;
            case ResultCode.DB_ERROR:
                player.logWarn("数据库错误");
                break;
            case ResultCode.SERVER_ERROR:
                player.logWarn("服务器错误");
                break;
            case ResultCode.BAD_PARAMETER:
                player.logWarn("参数错误");
                break;
            case ResultCode.BAD_REQUEST:
                player.logWarn("请求暂时无效");
                break;
            default:
                player.logWarn("未知错误，错误码：" + code);
                break;
        }
    }
    else {
        var first = player.getRoleInfo() == null;

        player.setRoleInfo(roleInfo);

        if (first)
        {
            //player.logWarn("制造意外");
            //player.getNetConn().close(true);
            //player.setNetConn(null);

            //player.logInfo("开始发送测试消息");
            //testEchoTimeCostMap = {};
            //var testCount = 100000;
            //var sendTestEcho = function(curNumber){
            //    let reqObj = {};
            //    if (curNumber === testCount)
            //    {
            //        reqObj.num = -1;
            //        player.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_TEST_ECHO, reqObj);
            //        player.logInfo("结束发送测试消息");
            //    }
            //    else
            //    {
            //        reqObj.num = curNumber;
            //        reqObj.propNum = 123456789;
            //        reqObj.propStr = "123456789";
            //        reqObj.propBool = true;
            //        reqObj.propArr = [1,2,3,4,5,6];
            //        reqObj.propNull = null;
            //        reqObj.propObj = {propNum : 123456789, propStr : "123456789", propBool : true, propArr : [1,2,3,4,5,6], propNull : null};
            //        player.sendEx(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_TEST_ECHO, reqObj);
            //        testEchoTimeCostMap[curNumber] = dateUtil.getTimestamp();
            //        setImmediate(sendTestEcho, curNumber + 1);
            //    }
            //};
            //setImmediate(sendTestEcho, 0);

            //player.sendEx(ModuleIds.MODULE_GM, CmdIdsGm.CMD_PROCESS_GM_CMD, {msg:"time 2015 1 1 1 2 3"});
        }
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_ACTIVATE_ROLE, onActivateRole, roleMessage.FullRoleInfoVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {null} body - 无body
 */
function onRelogin(player, code, errMsg, body)
{
    if (code != ResultCode.SUCCESS) {
        switch (code) {
            case ResultCodeAccount.CHECK_TOKEN_FAIL:
                player.logWarn(errMsg);
                break;
            case ResultCodeAccount.RELOGIN_FAIL:
                player.logWarn("重新登录失败");
                break;
            case ResultCode.DB_ERROR:
                player.logWarn("数据库错误");
                break;
            case ResultCode.SERVER_ERROR:
                player.logWarn("服务器错误");
                break;
            case ResultCode.BAD_PARAMETER:
                player.logWarn("参数错误");
                break;
            case ResultCode.BAD_REQUEST:
                player.logWarn("请求暂时无效");
                break;
            default:
                player.logWarn("未知错误，错误码：" + code);
                break;
        }
    }
    else {
        player.logDebug("自动重登成功");
        player.sendPendingMsgList();
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_RELOGIN, onRelogin);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {*} body - 任意类型
 */
function onTestEcho(player, code, errMsg, body)
{
    if (code != ResultCode.SUCCESS) {
        switch (code) {
            case ResultCode.DB_ERROR:
                player.logWarn("数据库错误");
                break;
            case ResultCode.SERVER_ERROR:
                player.logWarn("服务器错误");
                break;
            case ResultCode.BAD_PARAMETER:
                player.logWarn("参数错误");
                break;
            case ResultCode.BAD_REQUEST:
                player.logWarn("请求暂时无效");
                break;
            default:
                player.logWarn("未知错误，错误码：" + code);
                break;
        }
    }
    else {
        if (body.num === -1) {
            var max = appUtil.INT32_MAX_NEGTIVE;
            var min = appUtil.INT32_MAX_POSITIVE;
            var sum = 0;
            var cnt = 0;
            var avg = 0;
            for (var i in testEchoTimeCostMap) {
                var n = testEchoTimeCostMap[i];
                if (n > max)
                    max = n;
                if (n < min)
                    min = n;
                sum += n;
                cnt += 1;
            }
            if (cnt > 0)
            {
                avg = sum / cnt;
                player.logInfo("max:" + max);
                player.logInfo("min:" + min);
                player.logInfo("cnt:" + cnt);
                player.logInfo("avg:" + avg);
            }
            else
            {
                player.logWarn("无测试数据");
            }
        }
        else {
            var curTime = dateUtil.getTimestamp();
            testEchoTimeCostMap[body.num] = curTime - testEchoTimeCostMap[body.num];

            if (appCfg.debug)
                player.logDebug("testEcho:" + JSON.stringify(body));
        }
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_TEST_ECHO, onTestEcho);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {RoleSyncPropVo} syncProp - 键值对
 */
function onRoleSyncProp(player, code, errMsg, syncProp)
{
    player.logDebug("更新前数据：" + JSON.stringify(player.getRoleInfo()));
    if (player.syncRoleProp(syncProp.guid, syncProp.props))
    {
        player.logDebug("更新属性成功");
        player.logDebug("更新后数据：" + JSON.stringify(player.getRoleInfo()));
    }
    else
    {
        player.logDebug("更新属性失败");
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ROLE, CmdIdsRole.PUSH_SYNC_PROP, onRoleSyncProp, roleMessage.RoleSyncPropVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {RemovePetRoleVo} body
 */
function onPushRemovePet(player, code, errMsg, body)
{
    if (player.onRemovePet(body.guid))
    {
        player.logDebug("删除宠物成功，guid：" + body.guid);
        player.logDebug("删除后数据：" + JSON.stringify(player.getRoleInfo()));
    }
    else
    {
        player.logDebug("删除宠物失败，guid：" + body.guid);
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_REMOVE_PET, onPushRemovePet, petMessage.RemovePetRoleVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {FullRoleInfoVo} roleInfo
 */
function onPushAddPet(player, code, errMsg, roleInfo)
{
    if (player.onAddPet(roleInfo))
    {
        player.logDebug("添加宠物成功，data：" + JSON.stringify(roleInfo));
        player.logDebug("添加后数据：" + JSON.stringify(player.getRoleInfo()));
    }
    else
    {
        player.logDebug("添加宠物失败，data：" + JSON.stringify(roleInfo));
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_PET, onPushAddPet, roleMessage.FullRoleInfoVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {AddOrUpdateItemVo} body
 */
function onPushAddOrUpdateItem(player, code, errMsg, body)
{
    var opName = body.isAdd ? "添加" : "更新";
    if (player.onAddOrUpdateItem(body.item))
    {
        player.logDebug(opName + "物品成功，data：" + JSON.stringify(body.item));
        player.logDebug(opName + "后数据：" + JSON.stringify(player.getRoleInfo()));
    }
    else
    {
        player.logDebug(opName + "物品失败，data：" + JSON.stringify(body.item));
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_ADD_OR_UPDATE_ITEM, onPushAddOrUpdateItem, itemMessage.AddOrUpdateItemVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {AddOrUpdateItemsVo} body
 */
function onPushAddOrUpdateItems(player, code, errMsg, body)
{
    var opName = body.isAdd ? "添加" : "更新";
    var items = body.items;
    var doneIds = [];
    var failIds = [];
    for (var i = 0; i < items.length; ++i)
    {
        var item = items[i];
        if (player.onAddOrUpdateItem(item))
            doneIds.push(item.itemId);
        else
            failIds.push(item.itemId);
    }

    if (failIds.length > 0)
        player.logDebug(opName + "失败的物品，itemIds：" + failIds.join(","));

    if (doneIds.length > 0)
    {
        player.logDebug(opName + "成功的物品，itemIds：" + doneIds.join(","));
        player.logDebug(opName + "后数据：" + JSON.stringify(player.getRoleInfo()));
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_ADD_OR_UPDATE_ITEMS, onPushAddOrUpdateItems, itemMessage.AddOrUpdateItemsVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {RemoveItemVo} body
 */
function onPushRemoveItem(player, code, errMsg, body)
{
    if (player.onRemoveItem(body.itemId))
    {
        player.logDebug("删除物品成功，itemId：" + body.itemId);
        player.logDebug("删除后数据：" + JSON.stringify(player.getRoleInfo()));
    }
    else
    {
        player.logDebug("删除物品失败，itemId：" + body.itemId);
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_REMOVE_ITEM, onPushRemoveItem, itemMessage.RemoveItemVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {RemoveItemsVo} body
 */
function onPushRemoveItems(player, code, errMsg, body)
{
    var itemIds = body.itemIds;
    var doneIds = [];
    var failIds = [];
    for (var i = 0; i < itemIds.length; ++i)
    {
        var itemId = itemIds[i];
        if (player.onRemoveItem(itemId))
            doneIds.push(itemId);
        else
            failIds.push(itemId);
    }

    if (failIds.length > 0)
        player.logDebug("删除失败的物品，itemIds：" + failIds.join(","));

    if (doneIds.length > 0)
    {
        player.logDebug("删除成功的物品，itemIds：" + doneIds.join(","));
        player.logDebug("删除后数据：" + JSON.stringify(player.getRoleInfo()));
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_REMOVE_ITEMS, onPushRemoveItems, itemMessage.RemoveItemsVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {AddOrUpdateEquipVo} body
 */
function onPushAddOrUpdateEquip(player, code, errMsg, body)
{
    var opName = body.isAdd ? "添加" : "更新";
    if (player.onAddOrUpdateEquip(body.guidOwner, body.equip))
    {
        player.logDebug(opName + "装备成功，guidOwner：" + body.guidOwner + "，data：" + JSON.stringify(body.equip));
        player.logDebug(opName + "后数据：" + JSON.stringify(player.getRoleInfo()));
    }
    else
    {
        player.logDebug(opName + "装备失败，guidOwner：" + body.guidOwner + "，data：" + JSON.stringify(body.equip));
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_EQUIP, CmdIdsEquip.PUSH_ADD_OR_UPDATE_EQUIP, onPushAddOrUpdateEquip, equipMessage.AddOrUpdateEquipVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {RemoveEquipVo} body
 */
function onPushRemoveEquip(player, code, errMsg, body)
{
    if (player.onRemoveEquip(body.guidOwner, body.index))
    {
        player.logDebug("删除装备成功，guidOwner：" + body.guidOwner + "，index：" + body.index);
        player.logDebug("删除后数据：" + JSON.stringify(player.getRoleInfo()));
    }
    else
    {
        player.logDebug("删除装备失败，guidOwner：" + body.guidOwner + "，index：" + body.index);
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_EQUIP, CmdIdsEquip.PUSH_REMOVE_EQUIP, onPushRemoveEquip, equipMessage.RemoveEquipVo);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {SyncServerTime} body
 */
function onPushServerTime(player, code, errMsg, body)
{
    player.logDebug("当前真实时间：" + dateUtil.getTrueDateString());
    player.logDebug("原虚拟时间：" + dateUtil.getDateString());
    dateUtil.setTimeFromTimestamp(body.time);
    player.logDebug("新虚拟时间：" + dateUtil.getDateString());
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.PUSH_SERVER_TIME, onPushServerTime, accountMsg.SyncServerTime);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {SyncServerTime} body
 */
function onRetServerTime(player, code, errMsg, body)
{
    player.logDebug("当前真实时间：" + dateUtil.getTrueDateString());
    player.logDebug("原虚拟时间：" + dateUtil.getDateString());
    dateUtil.setTimeFromTimestamp(body.time);
    player.logDebug("新虚拟时间：" + dateUtil.getDateString());
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACCOUNT, CmdIdsAccount.CMD_SERVER_TIME, onRetServerTime, accountMsg.SyncServerTime);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {null} body
 */
function onRetGmResult(player, code, errMsg, body)
{
    if (code != ResultCode.SUCCESS) {
        switch (code) {
            case ResultCode.DB_ERROR:
                player.logWarn("数据库错误");
                break;
            case ResultCode.SERVER_ERROR:
                player.logWarn("服务器错误");
                break;
            case ResultCode.BAD_PARAMETER:
                player.logWarn("参数错误");
                break;
            case ResultCode.BAD_REQUEST:
                player.logWarn("请求暂时无效");
                break;
            default:
                player.logWarn("未知错误，错误码：" + code);
                break;
        }
    }
    else {
        player.logInfo("执行GM命令成功");
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_GM, CmdIdsGm.CMD_PROCESS_GM_CMD, onRetGmResult);

/**
 *
 * @param {Player} player
 * @param {number} code
 * @param {string|null} errMsg
 * @param {SyncActivityPropVo} syncProp - 键值对
 */
function onSyncActivityProp(player, code, errMsg, syncProp)
{
    player.logDebug("更新前数据：" + JSON.stringify(player.getRoleInfo()));
    if (player.syncActivityProp(syncProp.props))
    {
        player.logDebug("更新属性成功");
        player.logDebug("更新后数据：" + JSON.stringify(player.getRoleInfo()));
    }
    else
    {
        player.logDebug("更新属性失败");
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.PUSH_SYNC_PROP, onSyncActivityProp, activityMessage.SyncActivityPropVo);