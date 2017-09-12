"use strict";

var logUtil = require("../../libs/logUtil");
var dateUtil = require("../../libs/dateUtil");
var enSysVisType = require("../enumType/systemDefine").enSysVisType;
var enSysActiveType = require("../enumType/systemDefine").enSysActiveType;
var enSysOpenType = require("../enumType/systemDefine").enSysOpenType;
var systemConfig = require("../gameConfig/systemConfig");
var enProp = require("../enumType/propDefine").enProp;

////////////模块内变量////////////
const TEACH_KEY_MIN_LEN = 1;
const TEACH_KEY_MAX_LEN = 12;
const TEACH_KEY_MAC_CNT = 1000;

////////////私有函数////////////
/**
 *
 * @param {Role} role
 * * @param {object} condObj
 * @param {object} errObj
 * @returns {boolean}
 */
function isCondPassLevel(role, condObj, errObj)
{
    var levelsPart = role.getLevelsPart();
    var level = levelsPart.getLevelById(condObj.param);
    if(level && level.isWin)
    {
        return true;
    }
    else
    {
        errObj.errMsg = "未通关关卡"+condObj.param;
        return false;
    }
}

/**
 *
 * @param {Role} role
 * * @param {object} condObj
 * @param {object} errObj
 * @returns {boolean}
 */
function isCondLevel(role, condObj, errObj)
{
    if(role.getNumber(enProp.level)>=condObj.param)
    {
        return true;
    }
    else
    {
        errObj.errMsg = "未到达等级"+condObj.param;
        return false;
    }
}

/**
 *
 * @param {Role} role
 * * @param {object} condObj
 * @param {object} errObj
 * @returns {boolean}
 */
function isCondTime(role, condObj, errObj)
{
    var date = dateUtil.getDate();
    for(var i=0; i<condObj.param.length; i++) {
        if (condObj.param[i].from.lessThanOrEqual(date) && condObj.param[i].to.greaterThan(date)) {
            return true;
        }

    }

    errObj.errMsg = "不在时间范围内";
    return false;

}

/**
 *
 * @param {Role} role
 * @param {number} systemId
 * @param {object} errObj
 * @returns {boolean}
 */
function isActiveCond(role, systemId, errObj)
{
    var systemCfg = systemConfig.getSystemConfig(systemId);
    for(var i=0; i<systemCfg.activeCond.length; i++)
    {
        var condObj = systemCfg.activeCond[i];
        var result;
        switch (condObj.type)
        {
            case enSysActiveType.ACTIVE:
                result = true;
                break;
            case enSysActiveType.PASS_LEVEL:
                result = isCondPassLevel(role, condObj, errObj);
                break;
            case enSysActiveType.LEVEL:
                result = isCondLevel(role, condObj, errObj);
                break;
            default:
                logUtil.error("未实现的系统激活类型："+condObj.type);
                return false;

        }
        if(!result)
        {
            return false;
        }
    }
    return true;
}

////////////导出函数////////////

/**
 * 是否已激活
 * @param {Role} role
 * @param {number} systemId
 * @param {object} errObj
 * @returns {boolean}
 */
function isActive(role, systemId, errObj)
{
    var systemsPart = role.getSystemsPart();

    var system = systemsPart.getSystemBySystemId(systemId);
    if(system && system.active)
    {
        return true;
    }

    var active = isActiveCond(role, systemId, errObj);
    if(!system)
    {
        if(!systemsPart.addSystemWithSystemId(systemId,active,false))
        {
            logUtil.error("Add system failed");
            return false;
        }
    }
    else
    {
        if (system.active !== active)
        {
            system.active = active;
            system.syncAndSave();
        }
    }
    return active;
}

/**
 * 检测激活，如果有改变返回system以发送客户端
 * @param {Role} role
 * @param {number} systemId
 * @param {object} errObj
 * @returns {System|null}
 */
function checkActive(role, systemId, errObj)
{
    var systemsPart = role.getSystemsPart();

    var system = systemsPart.getSystemBySystemId(systemId);
    if(system && system.active)
    {
        return null;
    }

    var active = isActiveCond(role, systemId, errObj);
    if(!system)
    {
        if(!systemsPart.addSystemWithSystemId(systemId,active,true))
        {
            logUtil.error("Add system failed");
            return null;
        }

        system = systemsPart.getSystemBySystemId(systemId);
    }
    else
    {
        if (system.active === active)
            return null;

        system.active = active;
        system.syncAndSave(true);
    }
    return system;
}

/**
 * 是否开启
 * @param {Role} role
 * @param {number} systemId
 * @param {object} errObj
 * @returns {boolean}
 */
function isOpen(role, systemId, errObj)
{
    var systemCfg = systemConfig.getSystemConfig(systemId);
    for(var i=0; i<systemCfg.openCond.length; i++)
    {
        var condObj = systemCfg.openCond[i];
        var result;
        switch (condObj.type)
        {
            case enSysOpenType.TIME:
                result = isCondTime(role, condObj, errObj);
                break;
            case enSysOpenType.LEVEL:
                result = isCondLevel(role, condObj, errObj);
                break;
            default:
                logUtil.error("未实现的系统开启类型："+condObj.type);
                return false;

        }
        if(!result)
        {
            return false;
        }
    }
    return true;
}

/**
 * 是否显示
 * @param {Role} role
 * @param {number} systemId
 * @param {object} errObj
 * @returns {boolean}
 */
function isVisible(role, systemId, errObj)
{
    var systemCfg = systemConfig.getSystemConfig(systemId);
    var result;
    switch (systemCfg.visibility.type)
    {
        case enSysVisType.VISIBLE:
            result = true;
            break;
        case enSysOpenType.LEVEL:
            result = false;
            errObj.errMsg = "系统设置为不显示";
            break;
        default:
            logUtil.error("未实现的系统显示类型："+condObj.type);
            return false;

    }
    return result;
}

/**
 * 是否变灰
 * @param {Role} role
 * @param {number} systemId
 * @param {object} errObj
 * @returns {boolean}
 */
function isGrey(role, systemId, errObj)
{
    return !isNotGrey(role, systemId, errObj);
}

/**
 *
 * @param {Role} role
 * @param {number} systemId
 * @param {object} errObj
 * @returns {boolean}
 */
function isNotGrey(role, systemId, errObj)
{
    return isActive(role, systemId, errObj) && isOpen(role, systemId, errObj);
}

/**
 * 是否可用
 * @param {Role} role
 * @param {number} systemId
 * @param {object} errObj
 * @returns {boolean}
 */
function isEnabled(role, systemId, errObj)
{
    return isVisible(role, systemId, errObj) && isNotGrey(role, systemId, errObj);
}

/**
 *
 * @param {string} key
 * @returns {boolean}
 */
function isTeachKeyOK(key)
{
    if (!Object.isString(key))
        return false;
    if (key.length < TEACH_KEY_MIN_LEN || key.length > TEACH_KEY_MAX_LEN)
        return false;
    for (var i = 0, len = key.length; i < len; ++i)
    {
        var ch = key[i];
        if (!(ch === '_' || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9')))
            return false;
    }
    return true;
}


exports.isActive = isActive;
exports.checkActive = checkActive;
exports.isOpen = isOpen;
exports.isGrey = isGrey;
exports.isNotGrey = isNotGrey;
exports.isEnabled = isEnabled;
exports.isVisible = isVisible;
exports.isTeachKeyOK = isTeachKeyOK;
exports.TEACH_KEY_MAC_CNT = TEACH_KEY_MAC_CNT;