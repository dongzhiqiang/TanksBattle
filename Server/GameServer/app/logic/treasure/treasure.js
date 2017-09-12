"use strict";
var treasureConfig= require("../gameConfig/treasureConfig");
var logUtil = require("../../libs/logUtil");


class Treasure {
    /**
     *
     * @param data
     */
    constructor(data) {
        //为了防止data为空
        data = data || {};


        /**@type {Role} 拥有者*/
        Object.defineProperty(this, "_owner", {enumerable: false, writable: true, value: null});

        /**@type {number}*/
        this.treasureId = data.treasureId;
        /**@type {number}*/
        this.level = data.level;



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

        var part = this._owner.getTreasurePart();
        if (!part)
            return;

        part.syncAndSaveTreasure(this.treasureId);
    }

}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isTreasureData(data)
{
    return !!(data && data.treasureId != "");
}

/**
 *
 * @param data
 * @returns {Treasure|null}
 */
function createTreasure(data)
{
    if (!isTreasureData(data))
    {
        logUtil.error("Treasure基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var treasureCfg = treasureConfig.getTreasureConfig(data.treasureId);
    if (!treasureCfg)
    {
        logUtil.error("TreasureId无效，treasureId：" + data.treasureId);
        return null;
    }

    return new Treasure(data);
}




exports.isTreasureData = isTreasureData;
exports.createTreasure = createTreasure;