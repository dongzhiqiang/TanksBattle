"use strict";

var gameConfig = require("./gameConfig");

class FriendMaxConfig
{
    constructor() {
        this.level = 0;
        this.maxFriend = 0;

    }

    static fieldsDesc() {
        return {
            level: {type: Number},
            maxFriend: {type: Number},
        };
    }
}

/**
 *
 * @returns {FriendMaxConfig}
 */
function getFriendMaxCfg(key)
{
    return gameConfig.getCsvConfig("friendMaxConfig", key);
}


exports.FriendMaxConfig = FriendMaxConfig;
exports.getFriendMaxCfg = getFriendMaxCfg;