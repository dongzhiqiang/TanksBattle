/**
 * Created by pc20 on 2016/6/14.
 */
"use strict";

var dateUtil = require("../libs/dateUtil");

/**
 * 是否在恢复中
 * @param {Number} recordTime - 上一次恢复时记录的时间
 * @param {Number} curNum   - 当前记录的数量
 * @param {Number} recoverTime - 每隔多久恢复1个
 * @param {Number} recoverMaxLimit - 最多恢复多少
 * @returns {boolean}
 */
function IsRecovering(recordTime, curNum, recoverTime, recoverMaxLimit)
{
    if (recoverMaxLimit < 0)
        return true;

    if (curNum >= recoverMaxLimit)
        return false;

    var leftTime = recoverTime - recordTime % recoverTime;
    return (recordTime + (recoverMaxLimit - curNum - 1) * recoverTime) + leftTime >= dateUtil.getTimestamp();
}

/**
 * 计算本次还剩多少时间才恢复
 * @param recordTime - 上一次恢复时记录的时间
 * @param recoverTime - 每隔多久恢复1个
 * @returns {number}
 */
function getLeftTime(recordTime, recoverTime)
{
    return recoverTime - recordTime % recoverTime;
}

/**
 * 根据记录时间计算上一个恢复点时间
 * @param recordTime - 上一次恢复时记录的时间
 * @param recoverTime - 每隔多久恢复1个
 * @returns {number}
 */
function getLastTimeRecover(recordTime, recoverTime)
{
    return recordTime - recordTime % recoverTime;
}

/**
 * 给出记录的时间和个数获取到当前应该恢复的个数
 * @param recordTime - 上一次恢复时记录的时间
 * @param curNum - 当前记录的数量
 * @param recoverTime - 每隔多久恢复1个
 * @param recoverMaxLimit - 最多恢复多少
 * @returns {number}
 */
function getNum(recordTime, curNum, recoverTime, recoverMaxLimit)
{
    if(curNum >= recoverMaxLimit)
        return curNum;

    var lastTimeRecover = getLastTimeRecover(recordTime, recoverTime);
    var totalSec = Math.max(dateUtil.getTimestamp() - lastTimeRecover);

    return Math.min(recoverMaxLimit, Math.floor(totalSec/recoverTime)+ curNum);
}


////////////导出元素////////////
exports.getNum = getNum;
exports.getLastTimeRecover = getLastTimeRecover;
exports.getLeftTime = getLeftTime;
exports.IsRecovering = IsRecovering;
