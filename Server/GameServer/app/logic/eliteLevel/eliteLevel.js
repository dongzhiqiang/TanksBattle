"use strict";

var logUtil = require("../../libs/logUtil");
var eliteLevelConfig = require("../gameConfig/eliteLevelConfig");

class EliteLevel
{
    constructor(data)
    {

        this.levelId = data.levelId;
        this.passed = data.passed;
        this.starsInfo = data.starsInfo;
        this.enterTime = data.enterTime;
        this.count = data.count;
        this.resetCount = data.resetCount;
        this.firstRewarded = data.firstRewarded;

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

        var part = this._owner.getEliteLevelsPart();
        if (!part)
            return;

        part.syncAndSaveEliteLevel(this.levelId);
    }

    release()
    {
        if (this._owner)
            logUtil.debug("EliteLevel销毁，角色guid：" + this._owner.getGUID() + "，levelId：" + this.levelId);
        else
            logUtil.debug("EliteLevel销毁，levelId：" + this.levelId);
    }
}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isEliteLevelData(data)
{
    return !!(data && data.levelId != "");
}

/**
 *
 * @param data
 * @returns {EliteLevel|null}
 */
function createEliteLevel(data)
{
    if (!isEliteLevelData(data))
    {
        logUtil.error("EliteLevel基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var eliteLevelCfg = eliteLevelConfig.getEliteLevelBasicCfg(data.levelId);
    if (!eliteLevelCfg)
    {
        logUtil.error("LevelId无效，levelId：" + data.levelId);
        return null;
    }

    return new EliteLevel(data);
}




exports.isEliteLevelData = isEliteLevelData;
exports.createEliteLevel = createEliteLevel;
