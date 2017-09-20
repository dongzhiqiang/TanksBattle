"use strict";

var globalDefine = {
    MIN_ROLE_LEVEL : 1,
    MAX_ROLE_LEVEL : 100,
    MAX_EVALUATION : 6,
};

var enItemType = {
    ABSTRACT_ITEM: 9,    // 抽象物品
};

var enItemId = {
    GOLD: 30000,       //金币
    EXP: 30001,        //经验值
    DIAMOND: 30002,    //钻石
    STAMINA: 30003,    //体力
    ARENA_COIN: 30004, //竞技场兑换币
    REDSOUL: 40200,    //红魂
};

exports.globalDefine = globalDefine;
exports.enItemType = enItemType;
exports.enItemId = enItemId;