"use strict";

var logUtil = require("../../libs/logUtil");
var systemConfig = require("../gameConfig/systemConfig");
var roleConfig = require("../gameConfig/roleConfig");

class System
{
    constructor(data)
    {

        this.systemId = data.systemId;
        this.active = data.active;

        /**
         * 角色
         * @type {Role|null}
         */
        Object.defineProperty(this, "_owner", {enumerable: false, writable: true, value: null});

    }

    /**
     *
     * @param {Role} v
     */
    setOwner(v)
    {
        this._owner = v;
    }

    /**
     *
     * @returns {Role|null}
     */
    getOwner()
    {
        return this._owner;
    }

    /**
     * 当已在数据库时，存盘
     */
    syncAndSave(notSync)
    {
        if (!this._owner)
            return;

        var part = this._owner.getSystemsPart();
        if (!part)
            return;

        part.syncAndSaveSystem(this.systemId, notSync);
    }

    release()
    {
        if (this._owner)
            logUtil.debug("System销毁，角色guid：" + this._owner.getGUID() + "，systemId：" + this.systemId);
        else
            logUtil.debug("System销毁，systemId：" + this.systemId);
    }
}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isSystemData(data)
{
    return !!(data && data.systemId != "");
}

/**
 *
 * @param data
 * @returns {System|null}
 */
function createSystem(data)
{
    if (!isSystemData(data))
    {
        logUtil.error("System基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var systemCfg = systemConfig.getSystemConfig(data.systemId);
    if (!systemCfg)
    {
        logUtil.error("SystemId无效，systemId：" + data.systemId);
        return null;
    }

    return new System(data);
}




exports.isSystemData = isSystemData;
exports.createSystem = createSystem;
