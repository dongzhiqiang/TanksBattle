"use strict";

var gameConfig = require("./gameConfig");

class ItemConfig
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.type = 0;
        this.quality = 0;
        this.qualityLevel = 0;
        this.stateId = "";
        this.isSell = false;
        this.priceId = "";
        this.useValue1 = "";
        this.icon = "";
        this.description = "";
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            type: {type: Number},
            quality: {type: Number},
            qualityLevel: {type: Number},
            stateId: {type: String},
            isSell: {type: Boolean},
            priceId: {type: String},
            useValue1: {type: String},
            icon: {type: String},
            description: {type: String}
        };
    }
}

class ItemInitListConfig
{
    constructor() {
        this.roleId = "";
        /**
         * @type {Item[]}
         */
        this.items = [];    //原先是Number[][]，后面改成Item[]
    }

    static fieldsDesc() {
        return {
            roleId: {type: String},
            items: {type: Array, elemType:Number, arrayLayer:2},
        };
    }

    /**
     * 整数一下数据，搞成可以直接用的[{itemId:整数, num:整数},{itemId:整数, num:整数}]
     * @param {ItemInitListConfig} row - 行数据对象
     */
    static afterDefReadRow(row)
    {
        var items = [];
        for (var i = 0, len = row.items.length; i < len; ++i)
        {
            var subArr = row.items[i];
            items.push({itemId:subArr[0], num:subArr[1]});
        }
        row.items = items;
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {ItemConfig}
 */
function getItemConfig(key)
{
    return gameConfig.getCsvConfig("item", key);
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {ItemInitListConfig|object.<string,ItemInitListConfig>}
 */
function getItemInitListConfig(key)
{
    return gameConfig.getCsvConfig("itemInitList", key);
}

exports.ItemConfig = ItemConfig;
exports.ItemInitListConfig = ItemInitListConfig;
exports.getItemConfig = getItemConfig;
exports.getItemInitListConfig = getItemInitListConfig;