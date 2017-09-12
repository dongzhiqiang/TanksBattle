"use strict";

var logUtil = require("../../libs/logUtil");
var flameConfig = require("../gameConfig/flameConfig");
var roleConfig = require("../gameConfig/roleConfig");

class Flame
{
    constructor(data)
    {

        this.flameId = data.flameId;
        this.level = data.level;
        this.exp = data.exp;

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
    syncAndSave()
    {
        if (!this._owner)
            return;

        var part = this._owner.getFlamesPart();
        if (!part)
            return;

        part.syncAndSaveFlame(this.flameId);
    }

    release()
    {
        if (this._owner)
            logUtil.debug("Flame销毁，角色guid：" + this._owner.getGUID() + "，flameId：" + this.flameId);
        else
            logUtil.debug("Flame销毁，flameId：" + this.flameId);
    }
}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isFlameData(data)
{
    return !!(data && data.flameId != "");
}

/**
 *
 * @param data
 * @returns {Flame|null}
 */
function createFlame(data)
{
    if (!isFlameData(data))
    {
        logUtil.error("Flame基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var flameCfg = flameConfig.getFlameConfig(data.flameId);
    if (!flameCfg)
    {
        logUtil.error("FlameId无效，flameId：" + data.flameId);
        return null;
    }

    return new Flame(data);
}




exports.isFlameData = isFlameData;
exports.createFlame = createFlame;
