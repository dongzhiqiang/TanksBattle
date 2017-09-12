"use strict";

var enSystemId = {
    venusLevel: 50,    // 爱神的恩赐
    hadesLevel: 51,     // 哈迪斯之血
    goldLevel: 52,    // 金币副本
    guardLevel: 55,    // 守护家园
    weapon1: 62,
    weapon2: 63,
    weapon3: 64,
};

var enSysVisType = {
    VISIBLE: 0,       //默认可见
    INVISIBLE: 1,        //不可见
    SERVER_TIME: 2,    //开服多少天开启
    ACCOUNT_TIME: 3,    //创建账号多少天开启
};

var enSysActiveType = {
    ACTIVE: 0,       //默认解锁
    PASS_LEVEL: 1,        //通关副本
    LEVEL: 2,    //等级到达
    QUEST: 3,    //任务触发
};

var enSysOpenType = {
    TIME: 1,       //时间段
    LEVEL: 2,        //等级
    SERVER_TIME: 3,    //开服天数
    ACCOUNT_TIME: 4,    //创建账号天数
};

exports.enSystemId = enSystemId;
exports.enSysVisType = enSysVisType;
exports.enSysActiveType = enSysActiveType;
exports.enSysOpenType = enSysOpenType;