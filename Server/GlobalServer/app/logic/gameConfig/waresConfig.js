"use strict";

var gameConfig = require("./gameConfig");


class WaresConfig
{
    constructor() {
        this.id = 0;
        this.groupId = 0;
        this.itemId = 0;
        this.itemNum = 0;
        this.price = 0;
        this.sureAppear = 0;
        this.weight = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            groupId: {type: Number},
            itemId: {type: Number},
            itemNum: {type: Number},
            price: {type: Number},
            sureAppear: {type: Number},
            weight: {type: Number},
        };
    }
}

function getWaresConfig(key)
{
    return gameConfig.getCsvConfig("wares", key);
}

function getAllWaresConfig()
{
    return gameConfig.getCsvConfig("wares");
}


exports.WaresConfig = WaresConfig;
exports.getWaresConfig = getWaresConfig;
exports.getAllWaresConfig = getAllWaresConfig;
