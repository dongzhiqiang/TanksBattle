"use strict";

var gameConfig = require("./gameConfig");


class ArenaBuyConfig
{
    constructor() {
        this.arenaBuyNum = 0;
        this.price = 0;

    }

    static fieldsDesc() {
        return {
            arenaBuyNum: {type: Number},
            price: {type: Number},

        };
    }

}

function getArenaBuyConfig(key)
{
    return gameConfig.getCsvConfig("arenaBuy", key);
}


exports.ArenaBuyConfig = ArenaBuyConfig;
exports.getArenaBuyConfig = getArenaBuyConfig;
