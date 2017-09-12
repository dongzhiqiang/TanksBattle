"use strict";

var Promise = require("bluebird");

var appCfg = require("../../../config");
var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var httpUtil = require("../../libs/httpUtil");

////////////模块内变量////////////
var server_id = 3;//appCfg.serverId;

function doRequest(logType, body)
{
    if (!appCfg.adminServerUrl)
        return;

    var postTxt = "[" + dateUtil.getDateString2() + "][" + logType + "]," + JSON.stringify(body);
    logUtil.debug(postTxt);
    httpUtil.doPost(appCfg.adminServerUrl, postTxt, function(err, res){
        if (err)
            logUtil.error("请求管理后台接口失败", err);
        else
            logUtil.debug("请求管理后台返回：" + res);
    }, "text");
}

/**
 *
 * @param {number} roleNum
 */
function logOnlineRoleNum(roleNum) {
    var data = {
        server: server_id.toString(),
        online: roleNum,
        online_time: dateUtil.getTimestamp() * 1000
    };
    doRequest("OnlineRoleNum", data);
}

/**
 *
 * @param {string} ip
 * @param {string} ipv6
 * @param {string} device_model
 * @param {string} os_name
 * @param {string} os_ver
 * @param {string} mac_addr
 * @param {string} udid
 * @param {string} app_channel
 * @param {string} app_ver
 * @param {string} account_id
 * @param {string} role_id
 * @param {string} role_name
 * @param {number} create_time
 */
function logCreateRole(ip, ipv6, device_model, os_name, os_ver, mac_addr, udid, app_channel, app_ver, account_id, role_id, role_name, create_time) {
    var data = {
        server: server_id.toString(),
        ip: ip,
        ipv6: ipv6,
        device_model: device_model,
        os_name: os_name,
        os_ver: os_ver,
        mac_addr: mac_addr,
        udid: udid,
        app_channel: app_channel,
        app_ver: app_ver,
        account_id: account_id,
        role_id: role_id,
        role_name: role_name,
        create_time: create_time,
    };
    doRequest("CreateRole", data);
}

/**
 *
 * @param {string} ip
 * @param {string} ipv6
 * @param {string} device_model
 * @param {string} os_name
 * @param {string} os_ver
 * @param {string} mac_addr
 * @param {string} udid
 * @param {string} app_channel
 * @param {string} app_ver
 * @param {string} account_id
 * @param {string} role_id
 * @param {string} role_name
 * @param {number} create_time
 * @param {string} network
 * @param {string} isp
 * @param {number} device_height
 * @param {number} device_width
 * @param {number} role_level
 * @param {number} login_time
 * @param {number} last_logout_time
 * @param {number} offline_money
 * @param {number} offline_exp
 * @param {number} root
 * @param {number} vip_level
 */
function logLoginRole(ip, ipv6, device_model, os_name, os_ver, mac_addr, udid, app_channel, app_ver, account_id, role_id, role_name, create_time,
                      network, isp, device_height, device_width, role_level, login_time, last_logout_time, offline_money, offline_exp, root, vip_level) {
    var data = {
        server: server_id.toString(),
        ip: ip,
        ipv6: ipv6,
        device_model: device_model,
        os_name: os_name,
        os_ver: os_ver,
        mac_addr: mac_addr,
        udid: udid,
        app_channel: app_channel,
        app_ver: app_ver,
        account_id: account_id,
        role_id: role_id,
        role_name: role_name,
        create_time: create_time,
        network: network,
        isp: isp,
        device_height: device_height,
        device_width: device_width,
        role_level: role_level,
        login_time: login_time,
        last_logout_time: last_logout_time,
        offline_money: offline_money,
        offline_exp: offline_exp,
        root: root,
        vip_level: vip_level,
    };
    doRequest("LoginRole", data);
}

/**
 *
 * @param {string} ip
 * @param {string} ipv6
 * @param {string} device_model
 * @param {string} os_name
 * @param {string} os_ver
 * @param {string} mac_addr
 * @param {string} udid
 * @param {string} app_channel
 * @param {string} app_ver
 * @param {string} account_id
 * @param {string} role_id
 * @param {string} role_name
 * @param {number} create_time
 * @param {string} network
 * @param {string} isp
 * @param {number} device_height
 * @param {number} device_width
 * @param {number} role_level
 * @param {number} exp
 * @param {number} logout_time
 * @param {number} online_time
 * @param {string} scene
 * @param {string} axis
 * @param {number} money_sum
 * @param {number} exp_sum
 * @param {number} vip_level
 */
function logLogoutRole(ip, ipv6, device_model, os_name, os_ver, mac_addr, udid, app_channel, app_ver, account_id, role_id, role_name, create_time,
                      network, isp, device_height, device_width, role_level, exp, logout_time, online_time, scene, axis, money_sum, exp_sum, vip_level) {
    var data = {
        server: server_id.toString(),
        ip: ip,
        ipv6: ipv6,
        device_model: device_model,
        os_name: os_name,
        os_ver: os_ver,
        mac_addr: mac_addr,
        udid: udid,
        app_channel: app_channel,
        app_ver: app_ver,
        account_id: account_id,
        role_id: role_id,
        role_name: role_name,
        create_time: create_time,
        network: network,
        isp: isp,
        device_height: device_height,
        device_width: device_width,
        role_level: role_level,
        exp: exp,
        logout_time: logout_time,
        online_time: online_time,
        scene: scene,
        axis: axis,
        money_sum: money_sum,
        exp_sum: exp_sum,
        vip_level: vip_level,
    };
    doRequest("LogoutRole", data);
}

/**
 *
 * @param {string} account_id
 * @param {number} role_vip_lv
 * @param {string} old_accountid
 * @param {string} app_channel
 * @param {string} role_id
 * @param {string} role_name
 * @param {number} role_level
 * @param {string} mac_addr
 * @param {string} udid
 * @param {string} pay_channel
 * @param {number} yuanbao
 * @param {number} free_yuanbao
 * @param {number} cash
 * @param {string} currency
 * @param {number} left_yuanbao
 * @param {number} left_free_yuanbao
 * @param {string} pay_time
 * @param {string} pay_method
 * @param {object} get_item
 * @param {string} sn
 * @param {object} prepaid_detail
 * @param {number} purchase_type
 */
function logPrepaid(account_id,role_vip_lv,old_accountid,app_channel,role_id,role_name,role_level,mac_addr,udid,pay_channel,
                    yuanbao,free_yuanbao,cash,currency,left_yuanbao,left_free_yuanbao,pay_time,pay_method,get_item,sn,prepaid_detail,purchase_type) {
    var data = {
        server: server_id.toString(),
        account_id: account_id,
        role_vip_lv: role_vip_lv,
        old_accountid: old_accountid,
        app_channel: app_channel,
        role_id: role_id,
        role_name: role_name,
        role_level: role_level,
        mac_addr: mac_addr,
        udid: udid,
        pay_channel: pay_channel,
        yuanbao: yuanbao,
        free_yuanbao: free_yuanbao,
        cash: cash,
        currency: currency,
        left_yuanbao: left_yuanbao,
        left_free_yuanbao: left_free_yuanbao,
        pay_time: pay_time,
        pay_method: pay_method,
        get_item: get_item,
        sn: sn,
        prepaid_detail: prepaid_detail,
        purchase_type
    };
    doRequest("Prepaid", data);
}

/**
 *
 * @param {string} account_id
 * @param {string} role_id
 * @param {string} role_name
 * @param {number} role_level
 * @param {string} mac_addr
 * @param {string} udid
 * @param {string} reason
 * @param {number} free_yuanbao
 * @param {number} left_yuanbao
 * @param {number} left_free_yuanbao
 * @param {number} gain_time
 * @param {object} details
 */
function logYuanbaoGain(account_id,role_id,role_name,role_level,mac_addr,udid,reason,free_yuanbao,left_yuanbao,left_free_yuanbao,gain_time,details) {
    var data = {
        server: server_id.toString(),
        account_id: account_id,
        role_id: role_id,
        role_name: role_name,
        role_level: role_level,
        mac_addr: mac_addr,
        udid: udid,
        reason: reason,
        free_yuanbao: free_yuanbao,
        left_yuanbao: left_yuanbao,
        left_free_yuanbao: left_free_yuanbao,
        gain_time: gain_time,
        details: details
    };
    doRequest("YuanbaoGain", data);
}

/**
 *
 * @param {string} account_id
 * @param {string} role_id
 * @param {string} role_name
 * @param {number} role_level
 * @param {string} mac_addr
 * @param {string} udid
 * @param {string} reason
 * @param {number} yuanbao
 * @param {number} left_yuanbao
 * @param {number} free_yuanbao
 * @param {number} left_free_yuanbao
 * @param {number} use_time
 * @param {object} details
 */
function logYuanbaoUse(account_id,role_id,role_name,role_level,mac_addr,udid,reason,yuanbao,left_yuanbao,free_yuanbao,left_free_yuanbao,use_time,details) {
    var data = {
        server: server_id.toString(),
        account_id: account_id,
        role_id: role_id,
        role_name: role_name,
        role_level: role_level,
        mac_addr: mac_addr,
        udid: udid,
        reason: reason,
        yuanbao: yuanbao,
        left_yuanbao: left_yuanbao,
        free_yuanbao: free_yuanbao,
        left_free_yuanbao: left_free_yuanbao,
        use_time: use_time,
        details: details
    };
    doRequest("YuanbaoUse", data);
}

var doInitCoroutine = Promise.coroutine(function * () {
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;

exports.logOnlineRoleNum = logOnlineRoleNum;
exports.logCreateRole = logCreateRole;
exports.logLoginRole = logLoginRole;
exports.logLogoutRole = logLogoutRole;
exports.logPrepaid = logPrepaid;
exports.logYuanbaoGain = logYuanbaoGain;
exports.logYuanbaoUse = logYuanbaoUse;