"use strict";

var appCfg = require("../../config");
var dateUtil = require("./dateUtil");

//一个递增的数字
var numberSeed = 0;

/**
 * 产生通用唯一ID
 * 格式：服务器ID_毫秒级时间戳_顺序值
 * @returns {string}
 */
function generateGUID()
{
    numberSeed = numberSeed > 16777216 ? 0 : numberSeed + 1;
    var svrIdPart = appCfg.serverId.toString(16);
    var timePart = dateUtil.getTrueTimestampMS().toString(16);
    var numPart = numberSeed.toString(16);
    return svrIdPart + "_" + timePart + "_" + numPart;
}

exports.generateGUID = generateGUID;