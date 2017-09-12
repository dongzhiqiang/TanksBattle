"use strict";

var gameConfig = require("./gameConfig");


class StaminaBuyConfig
{
    constructor() {
        this.staminaBuyNum = 0;
        this.price = 0;

    }

    static fieldsDesc() {
        return {
            staminaBuyNum: {type: Number},
            price: {type: Number},

        };
    }

}

function getStaminaBuyConfig(key)
{
    return gameConfig.getCsvConfig("staminaBuy", key);
}


exports.StaminaBuyConfig = StaminaBuyConfig;
exports.getStaminaBuyConfig = getStaminaBuyConfig;
