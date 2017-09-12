"use strict";

var globalDefine = {
    MIN_ROLE_LEVEL : 1,
    MAX_ROLE_LEVEL : 100,
    MAX_EVALUATION : 6,
};

var enItemType = {
    ABSTRACT_ITEM: 9,    // 抽象物品
    PET_EXP_ITEM: 15,     // 神侍经验丹
};

var enPetPos = {
    pet1Main: 0,
    pet1Sub1: 1,
    pet1Sub2: 2,
    pet2Main: 3,
    pet2Sub1: 4,
    pet2Sub2: 5,
};

var enPetFormation = {
    normal: 1,
    arena: 2,
    eliteLevel: 3,
    treasureRob: 4,
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
exports.enPetPos = enPetPos;
exports.enPetFormation = enPetFormation;