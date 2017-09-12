"use strict";

var logUtil = require("../../libs/logUtil");
var itemConfig = require("../gameConfig/itemConfig");

class Item
{
    constructor(data)
    {
        /**
         * 禁止修改这个值
         * @type {number}
         */
        this.itemId = data.itemId;
        this.num = data.num;

        /**
         * 角色
         * @type {Role|null}
         */
        Object.defineProperty(this, "_owner", {enumerable: false, writable: true, value: null});

        //logUtil.debug("Item创建，itemId：" + this.itemId);
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

        var part = this._owner.getItemsPart();
        if (!part)
            return;

        part.syncAndSaveItem(this.itemId);
    }

    release()
    {
        if (this._owner)
            logUtil.debug("Item销毁，角色guid：" + this._owner.getGUID() + "，itemId：" + this.itemId);
        else
            logUtil.debug("Item销毁，itemId：" + this.itemId);
    }
}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isItemData(data)
{
    return !!(data && data.itemId > 0 && data.num >= 0);
}

/**
 *
 * @param data
 * @returns {Item|null}
 */
function createItem(data)
{
    if (!isItemData(data))
    {
        logUtil.error("Item基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var itemCfg = itemConfig.getItemConfig(data.itemId);
    if (!itemCfg)
    {
        logUtil.error("Item无效，itemId：" + data.itemId);
        return null;
    }

    return new Item(data);
}

exports.Item = Item;
exports.isItemData = isItemData;
exports.createItem = createItem;