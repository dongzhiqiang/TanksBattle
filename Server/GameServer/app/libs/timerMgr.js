"use strict";

var logUtil = require("../libs/logUtil");

/**
 *
 * @type {Map.<(object|function), Map.<number, object>>}
 */
var timerMap = new Map();

function createTimerCaller(cxt)
{
    return function(){
        try {
            if (Object.isFunction(cxt.handler))
                cxt.handler(cxt.timerID);
            else
                cxt.handler.onTimer(cxt.timerID);
        }
        catch (err) {
            logUtil.error("定时器回调发生错误", err);
        }
        finally {
            if (cxt.once)
                removeTimer(cxt.handler, cxt.timerID);
        }
    }
}

/**
 *
 * @param {OnTimerClass|OnTimerCallback} handler - 带onTimer成员函数的对象或直接一个函数
 * @param {number} timerID - 定时器ID，对于不同的handler，可以重复
 * @param {number} interval - 定时间隔，单位毫秒
 * @param {boolean?} [once=false] - 是否只执行一次就删除本定时器
 */
function addTimer(handler, timerID, interval, once)
{
    if (!(Object.isFunction(handler) || (Object.isObject(handler) && Object.isFunction(handler.onTimer))))
    {
        logUtil.error("TimerMgr~addTimer handler必须是函数或带有onTimer函数成员的对象");
        return;
    }

    var timerCxt;
    var subMap = timerMap.get(handler);
    if (!subMap)
    {
        subMap = new Map();
        timerMap.set(handler, subMap);
    }
    else
    {
        //检查是否已存在这个ID的定时器
        timerCxt = subMap.get(timerID);
        //那如果interval或once不一样，那就要重新建立timer
        if (timerCxt && (!!timerCxt.once !== !!once || timerCxt.interval !== interval))
            clearInterval(timerCxt.timerRef);
    }

    timerCxt = {handler:handler, timerID:timerID, once:once, interval:interval, timerRef:null};
    var caller = createTimerCaller(timerCxt);
    timerCxt.timerRef = setInterval(caller, interval);
    subMap.set(timerID, timerCxt);
}

/**
 *
 * @param {OnTimerClass|OnTimerCallback} handler
 * @param {number?} timerID - 不提供这个参数则删除全部属于handler的定时器
 */
function removeTimer(handler, timerID)
{
    var subMap = timerMap.get(handler);
    if (!subMap)
        return;

    var timerCxt;
    if (timerID === null || timerID === undefined)
    {
        for (timerCxt of subMap.values())
            clearInterval(timerCxt.timerRef);
        timerMap.delete(handler);
    }
    else
    {
        timerCxt = subMap.get(timerID);
        if (!timerCxt)
            return;
        clearInterval(timerCxt.timerRef);
        subMap.delete(timerID);
        if (subMap.size <= 0)
            timerMap.delete(handler);
    }
}

/**
 *
 * @param {OnTimerClass|OnTimerCallback} handler
 * @param {number?} timerID - 不提供这个参数则不检测这个参数
 * @return {boolean}
 */
function hasTimer(handler, timerID)
{
    var subMap = timerMap.get(handler);
    if (!subMap)
        return false;
    if (timerID === null || timerID === undefined)
        return true;
    return subMap.has(timerID);
}

exports.addTimer = addTimer;
exports.removeTimer = removeTimer;
exports.hasTimer = hasTimer;