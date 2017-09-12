"use strict";

////////////模块内变量//////////
/**
 * 周日是不是一周的第一天
 * @type {boolean}
 */
var SUNDAY_FIRST = false;

/**
 * 一天的分界点，是从0点开始过的秒数
 * 不一定非要点到0点
 * 时 * 3600 + 分 * 60 + 秒
 * @type {number}
 */
var DAY_BREAK_POINT = 5 * 3600;

/**
 * 虚拟时间和真实时间的差值，单位秒
 * @type {number}
 */
var timeDelta = 0;

////////////导出函数////////////
/**
 * 转换日期对象为时间戳，单位秒
 * @param {Date} obj
 * @returns {number}
 */
function getTimestampFromDate(obj)
{
    return Math.floor(obj.getTime() / 1000);
}

/**
 * 转换日期对象为时间戳，单位毫秒
 * @param {Date} obj
 * @returns {number}
 */
function getTimestampMSFromDate(obj)
{
    return Math.floor(obj.getTime());
}

/**
 * 转换日期字符串为时间戳，单位秒
 * @param {string} str
 * @returns {number}
 */
function getTimestampFromString(str)
{
    return Math.floor(Date.parse(str) / 1000);
}

/**
 * 转换日期字符串为时间戳，单位毫秒
 * @param {string} str
 * @returns {number}
 */
function getTimestampMSFromString(str)
{
    return Math.floor(Date.parse(str));
}

/**
 *
 * @param {number} timestamp - 时间戳，单位秒
 * @returns {Date}
 */
function getDateFromTimestamp(timestamp)
{
    return new Date(timestamp * 1000);
}

/**
 *
 * @param {number} timestamp - 时间戳，单位毫秒
 * @returns {Date}
 */
function getDateFromTimestampMS(timestamp)
{
    return new Date(timestamp);
}

/**
 *
 * @param {string} str - 时间字符串
 * @returns {Date}
 */
function getDateFromString(str)
{
    return new Date(str);
}

/**
 * 虚拟时间，单位秒
 * @returns {number}
 */
function getTimestamp()
{
    return Math.floor(Date.now() / 1000) + timeDelta;
}

/**
 * 获取虚拟时间的当天0点时间戳，单位秒
 * @returns {number}
 */
function getDayBreakPointTimestamp()
{
    let date = this.getDate();
    return this.getTimestamp() - date.getHours()*60*60 - date.getMinutes()*60 - date.getSeconds();
}



/**
 * 虚拟时间对象
 * @returns {Date}
 */
function getDate()
{
    var d = new Date();
    d.setSeconds(d.getSeconds() + timeDelta);
    return d;
}

/**
 *
 * @returns {string}
 */
function getDateString()
{
    var d = getDate();
    return d.getFullYear() + "-" +  + "-" + d.getDate() + " " + d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
}

/**
 *
 * @returns {string}
 */
function getDateString2()
{
    var d = getDate();
    var month = (d.getMonth() + 1);
    month = month < 10 ? "0" + month : month;
    var date = d.getDate();
    date = date < 10 ? "0" + date : date;
    var hour = d.getHours();
    hour = hour < 10 ? "0" + hour : hour;
    var minute = d.getMinutes();
    minute = minute < 10 ? "0" + minute : minute;
    var second = d.getSeconds();
    second = second < 10 ? "0" + second : second;
    return d.getFullYear() + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;
}

/**
 * 获取虚拟时间的星期几，1-7，分别表示周一到周日
 * @returns {number}
 */
function getDayOfWeek()
{
    return getDate().getDay() || 7;
}

/**
 * 设置虚拟时间
 * @param {number} timestamp - 时间戳，单位秒
 */
function setTimeFromTimestamp(timestamp)
{
    timeDelta = timestamp - getTrueTimestamp();
    if (isNaN(timeDelta))
        timeDelta = 0;
}

/**
 * 虚拟时间，单位毫秒
 * @returns {number}
 */
function getTimestampMS()
{
    return Math.floor(Date.now()) + timeDelta;
}

/**
 * 真实时间，单位秒
 * @returns {number}
 */
function getTrueTimestamp()
{
    return Math.floor(Date.now() / 1000);
}

/**
 * 真实时间，单位毫秒
 * @returns {number}
 */
function getTrueTimestampMS()
{
    return Date.now();
}

/**
 *
 * @returns {Date}
 */
function getTrueDate()
{
    return new Date();
}

/**
 *
 * @returns {string}
 */
function getTrueDateString()
{
    var d = getTrueDate();
    return d.getFullYear() + "-" + (d.getMonth() + 1) + "-" + d.getDate() + " " + d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
}

/**
 * 会补前导0的格式
 * @returns {string}
 */
function getTrueDateString2()
{
    var d = getTrueDate();
    var month = (d.getMonth() + 1);
    month = month < 10 ? "0" + month : month;
    var date = d.getDate();
    date = date < 10 ? "0" + date : date;
    var hour = d.getHours();
    hour = hour < 10 ? "0" + hour : hour;
    var minute = d.getMinutes();
    minute = minute < 10 ? "0" + minute : minute;
    var second = d.getSeconds();
    second = second < 10 ? "0" + second : second;
    return d.getFullYear() + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;
}

/**
 * 获取真实时间的星期几，1-7，分别表示周一到周日
 */
function getTrueDayOfWeek()
{
    return getTrueDate().getDay() || 7;
}

/**
 * 判断是否同一天
 * @param {number} ts1 - 时间戳1
 * @param {number} ts2 - 时间戳2
 * @returns {boolean}
 */
function isSameDay(ts1, ts2)
{
    if (ts1 === ts2)
        return true;

    //把时间戳移到0点
    ts1 -= DAY_BREAK_POINT;
    ts2 -= DAY_BREAK_POINT;

    var d1 = getDateFromTimestamp(ts1);
    var d2 = getDateFromTimestamp(ts2);

    return d1.getFullYear() == d2.getFullYear() && d1.getMonth() == d2.getMonth() && d1.getDate() == d2.getDate();
}

/**
 * 用的是虚拟时间
 * @param {number} ts
 */
function isToday(ts)
{
    return isSameDay(ts, getTimestamp());
}

/**
 * 用的是真实时间
 * @param {number} ts
 */
function isTrueToday(ts)
{
    return isSameDay(ts, getTrueTimestamp());
}

/**
 * 判断是否同一周，注意周日可能是一周的第一天，也可能是不是
 * @param {number} ts1 - 时间戳1
 * @param {number} ts2 - 时间戳2
 * @returns {boolean}
 */
function isSameWeek(ts1, ts2)
{
    if (ts1 === ts2)
        return true;

    var d1 = getDateFromTimestamp(ts1);
    var d2 = getDateFromTimestamp(ts2);

    var wd1 = d1.getDay();  //周日是0，周六是6
    var wd2 = d2.getDay();  //周日是0，周六是6

    //把日期移到开始
    //如果开始日期是周日，那就移到周日
    if (SUNDAY_FIRST)
    {
        ts1 -= wd1 * 86400;
        ts2 -= wd2 * 86400;
    }
    //否则就认为开始日期是周一，移到周一
    else
    {
        ts1 -= ((wd1 || 7) - 1) * 86400;
        ts2 -= ((wd2 || 7) - 1) * 86400;
    }

    d1 = getDateFromTimestamp(ts1);
    d2 = getDateFromTimestamp(ts2);

    return d1.getFullYear() == d2.getFullYear() && d1.getMonth() == d2.getMonth() && d1.getDate() == d2.getDate();
}

/**
 * 用的是虚拟时间
 * @param {number} ts
 */
function isThisWeek(ts)
{
    return isSameWeek(ts, getTimestamp());
}

/**
 * 用的是真实时间
 * @param {number} ts
 */
function isTrueThisWeek(ts)
{
    return isSameWeek(ts, getTrueTimestamp());
}

/**
 * 判断是否同一月
 * @param {number} ts1 - 时间戳1
 * @param {number} ts2 - 时间戳2
 * @returns {boolean}
 */
function isSameMonth(ts1, ts2)
{
    if (ts1 === ts2)
        return true;

    //把时间戳移到0点
    ts1 -= DAY_BREAK_POINT;
    ts2 -= DAY_BREAK_POINT;

    var d1 = getDateFromTimestamp(ts1);
    var d2 = getDateFromTimestamp(ts2);

    return d1.getFullYear() == d2.getFullYear() && d1.getMonth() == d2.getMonth();
}

/**
 * 用的是虚拟时间
 * @param {number} ts
 */
function isThisMonth(ts)
{
    return isSameMonth(ts, getTimestamp());
}

/**
 * 用的是真实时间
 * @param {number} ts
 */
function isTrueThisMonth(ts)
{
    return isSameMonth(ts, getTrueTimestamp());
}

/**
 * 判断是否同一年
 * @param {number} ts1 - 时间戳1
 * @param {number} ts2 - 时间戳2
 * @returns {boolean}
 */
function isSameYear(ts1, ts2)
{
    if (ts1 === ts2)
        return true;

    //把时间戳移到0点
    ts1 -= DAY_BREAK_POINT;
    ts2 -= DAY_BREAK_POINT;

    var d1 = getDateFromTimestamp(ts1);
    var d2 = getDateFromTimestamp(ts2);

    return d1.getFullYear() == d2.getFullYear();
}

/**
 * 用的是虚拟时间
 * @param {number} ts
 */
function isThisYear(ts)
{
    return isSameYear(ts, getTimestamp());
}

/**
 * 用的是真实时间
 * @param {number} ts
 */
function isTrueThisYear(ts)
{
    return isSameYear(ts, getTrueTimestamp());
}

/**
 *
 * @param {number} sundayFirst
 * @param {number} dayBreakPoint
 */
function setParameter(sundayFirst, dayBreakPoint)
{
    SUNDAY_FIRST = !!sundayFirst;
    DAY_BREAK_POINT = parseInt(dayBreakPoint, 10) || 0;
}

/**
 *
 * @param {number} timestamp
 * @returns {string}
 */
function getStringFromTimestamp(timestamp)
{
    var d = getDateFromTimestamp(timestamp);
    return d.getFullYear() + "-" + (d.getMonth() + 1) + "-" + d.getDate() + " " + d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
}

/**
 *
 * @param {Date} d
 * @returns {string}
 */
function getStringFromDate(d)
{
    return d.getFullYear() + "-" + (d.getMonth() + 1) + "-" + d.getDate() + " " + d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
}

/**
 * 获取本地时间跟UTC时间相差的分钟数
 * 这里取反，是因为这个getTimezoneOffset是UTC时间跟本地时间的差异
 * @returns {number}
 */
function getTimezoneOffset()
{
    return -new Date().getTimezoneOffset();
}

/**
 * 判断当前时间是否在某个时间段内，可以跨0点
 * @param {number} startMinutes - 时间段开始的分钟数（从0点开始的分钟）
 * @param {number} endMinutes - 时间段结束的分钟数（从0点开始的分钟），时间段不包含这分钟
 * @returns {boolean}
 */
function isNowBetweenTime(startMinutes, endMinutes)
{
    var curDate = getDate();
    var curMinutes = curDate.getHours() * 60 + curDate.getMinutes();
    return (startMinutes <= endMinutes && (curMinutes >= startMinutes && curMinutes < endMinutes))
        ||
        (startMinutes > endMinutes && (curMinutes >= startMinutes || curMinutes < endMinutes));
}

/**
 * 判断当前时间是否在某个时间段内，可以跨0点
 * @param {number} startHour - 时间段开始的时钟（从0点开始的分钟）
 * @param {number} startMinute - 时间段开始的分钟（从0点开始的分钟）
 * @param {number} endHour - 时间段结束的时钟（从0点开始的分钟）
 * @param {number} endMinute - 时间段结束的分钟（从0点开始的分钟），时间段不包含这分钟
 * @returns {boolean}
 */
function isNowBetweenTimeEx(startHour, startMinute, endHour, endMinute)
{
    return isNowBetweenTime(startHour * 60 + startMinute, endHour * 60 + endMinute);
}

////////////导出元素////////////
exports.getTimestampFromDate = getTimestampFromDate;
exports.getTimestampMSFromDate = getTimestampMSFromDate;
exports.getTimestampFromString = getTimestampFromString;
exports.getTimestampMSFromString = getTimestampMSFromString;
exports.getDateFromTimestamp = getDateFromTimestamp;
exports.getDateFromTimestampMS = getDateFromTimestampMS;
exports.getDateFromString = getDateFromString;
exports.getTimestamp = getTimestamp;
exports.getDate = getDate;
exports.getDateString = getDateString;
exports.getDateString2 = getDateString2;
exports.getDayOfWeek = getDayOfWeek;
exports.setTimeFromTimestamp = setTimeFromTimestamp;
exports.getTimestampMS = getTimestampMS;
exports.getTrueTimestamp = getTrueTimestamp;
exports.getTrueTimestampMS = getTrueTimestampMS;
exports.getTrueDate = getTrueDate;
exports.getTrueDateString = getTrueDateString;
exports.getTrueDateString2 = getTrueDateString2;
exports.getTrueDayOfWeek = getTrueDayOfWeek;
exports.isSameDay = isSameDay;
exports.isToday = isToday;
exports.isTrueToday = isTrueToday;
exports.isSameWeek = isSameWeek;
exports.isThisWeek = isThisWeek;
exports.isTrueThisWeek = isTrueThisWeek;
exports.isSameMonth = isSameMonth;
exports.isThisMonth = isThisMonth;
exports.isTrueThisMonth = isTrueThisMonth;
exports.isSameYear = isSameYear;
exports.isThisYear = isThisYear;
exports.isTrueThisYear = isTrueThisYear;
exports.setParameter = setParameter;
exports.getStringFromTimestamp = getStringFromTimestamp;
exports.getStringFromDate = getStringFromDate;
exports.getTimezoneOffset = getTimezoneOffset;
exports.isNowBetweenTime = isNowBetweenTime;
exports.isNowBetweenTimeEx = isNowBetweenTimeEx;
exports.getDayBreakPointTimestamp = getDayBreakPointTimestamp;
