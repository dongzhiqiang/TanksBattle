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

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {ItemConfig}
 */
function getItemConfig(key)
{
    return gameConfig.getCsvConfig("item", key);
}

exports.ItemConfig = ItemConfig;
exports.getItemConfig = getItemConfig;