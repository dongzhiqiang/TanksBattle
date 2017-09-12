/**
 * Created by pc20 on 2016/8/27.
 */
"use strict";

var gameConfig = require("./gameConfig");

class ProphetTowerConfig
{
    constructor() {
        this.id = 0;
        this.roomId = 0;
        this.rewardId;
        this.stage;
        this.range;
        this.isBoss;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            roomId: {type: String},
            rewardId: {type: Number},
            stage: {type: Number},
            range: {type: Array, elemType:Number},
            isBoss: {type: Number},
        };
    }
}


/**
 *
 * @returns {ProphetTowerConfig}
 */
function getProphetTowerConfig(key)
{
    return gameConfig.getCsvConfig("prophetTower", key);
}


exports.ProphetTowerConfig = ProphetTowerConfig;
exports.getProphetTowerConfig = getProphetTowerConfig;
