"use strict";

var logUtil = require("../libs/logUtil");

function signOf(num)
{
    if(num<0)
    {
        return -1;
    }
    else if(num>0)
    {
        return 1;
    }
    else
    {
        return 0;
    }
}

class CronTime
{
    constructor(cronStr)
    {
        var tempStr = cronStr.split(' ');
        if (tempStr.length < 6)
        {
            logUtil.error("cron表达式不正确:" + cronStr);
            return;
        }
        var parseResult;
        parseResult = this.parseOneNum(tempStr[0]);
        this.sec = parseResult.num;
        this.hasSec = parseResult.hasNum;
        parseResult = this.parseOneNum(tempStr[1]);
        this.min = parseResult.num;
        this.hasMin = parseResult.hasNum;
        parseResult = this.parseOneNum(tempStr[2]);
        this.hour = parseResult.num;
        this.hasHour = parseResult.hasNum;
        parseResult = this.parseOneNum(tempStr[3]);
        this.day = parseResult.num;
        this.hasDay = parseResult.hasNum;
        parseResult = this.parseOneNum(tempStr[4]);
        this.month = parseResult.num;
        this.hasMonth = parseResult.hasNum;
        parseResult = this.parseOneNum(tempStr[5]);
        this.dayOfWeek = parseResult.num;
        this.hasDayOfWeek = parseResult.hasNum;
        if(tempStr.length>6)
        {
            parseResult = this.parseOneNum(tempStr[6]);
            this.year = parseResult.num;
            this.hasYear = parseResult.hasNum;
        }
        else
        {
            this.year = 0;
            this.hasYear = false;
        }
    }

    /**
     *
     * @param {string} cronNum
     * @returns {object}
     */
    parseOneNum(cronNum)
    {
        var result = {};
        result.num = 0;
        if (cronNum == "*")
        {
            result.hasNum = false;
        }
        else
        {
            result.num =parseInt(cronNum);
            if (result.num || result.num == 0)
            {
                result.hasNum = true;
            }
            else
            {
                result.hasNum = false;
                result.num = 0;
                logUtil.error("cron表达式不正确，未支持的子表达式:" + cronNum);
            }
        }
        return result;
    }



    /**
     *
     * @param {Date} time
     * @param {number} compareSign
     * @param {boolean} orEqual
     * @returns {boolean}
     */
    compareWith(time, compareSign, orEqual)
    {
        var compared = false;
        var sign = 0;
        if(this.hasYear)
        {
            compared = true;
            sign = signOf(this.year - time.getFullYear());
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if(sign != 0)
            {
                return false;
            }
        }
        if (this.hasMonth)
        {
            compared = true;
            sign = signOf(this.month - (time.getMonth()+1));
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (this.hasDay)
        {
            compared = true;
            sign = signOf(this.day - time.getDate());
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (this.hasDayOfWeek) //..
        {
            compared = true;
            sign = signOf(this.dayOfWeek - (time.getDay()||7));
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (this.hasHour)
        {
            compared = true;
            sign = signOf(this.hour - time.getHours());
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (this.hasMin)
        {
            compared = true;
            sign = signOf(this.min - time.getMinutes());
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (this.hasSec)
        {
            compared = true;
            sign = signOf(this.sec - time.getSeconds());
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if(!compared || orEqual)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /**
     *
     * @param {Date} time
     * @returns {boolean}
     */
    greaterThan(time)
    {
        return this.compareWith(time, 1, false);
    }

    /**
     *
     * @param {Date} time
     * @returns {boolean}
     */
    greaterThanOrEqual(time)
    {
        return this.compareWith(time, 1, true);
    }

    /**
     *
     * @param {Date} time
     * @returns {boolean}
     */
    lessThan(time)
    {
        return this.compareWith(time, -1, false);
    }

    /**
     *
     * @param {Date} time
     * @returns {boolean}
     */
    lessThanOrEqual(time)
    {
        return this.compareWith(time, -1, true);
    }

    /**
     *
     * @param {Date} time
     * @returns {boolean}
     */
    equalTo(time)
    {
        return this.compareWith(time, 0, true);
    }

}

/*
function testWith(result, msg)
{
    if(result)
    {
        logUtil.info(msg + " pass");
    }
    else
    {
        logUtil.error(msg + " failed");
    }
}


function test()
{
    testWith(getCronTime("* * * * * 1").equalTo(new Date("2016-04-18 15:43:00")), "case 1");
    testWith(!getCronTime("1 43 15 * * *").equalTo(new Date("2016-04-18 15:43:00")), "case 2");
    testWith(getCronTime("0 43 15 * 4 *").equalTo(new Date("2016-04-18 15:43:00")), "case 3");
    testWith(getCronTime("* * * * * 1").greaterThanOrEqual(new Date("2016-04-18 15:43:00")), "case 4");
    testWith(getCronTime("0 43 15 * 4 *").greaterThanOrEqual(new Date("2016-04-18 15:43:00")), "case 5");
    testWith(getCronTime("* * * * * 1").lessThanOrEqual(new Date("2016-04-18 15:43:00")), "case 6");
    testWith(getCronTime("0 43 15 * 4 *").lessThanOrEqual(new Date("2016-04-18 15:43:00")), "case 7");
    testWith(!getCronTime("1 43 15 * * *").lessThan(new Date("2016-04-18 15:43:00")), "case 8");
    testWith(getCronTime("1 43 15 * * *").greaterThan(new Date("2016-04-18 15:43:00")), "case 9");
    testWith(!getCronTime("1 43 15 * * *").lessThanOrEqual(new Date("2016-04-18 15:43:00")), "case 10");
    testWith(getCronTime("1 43 15 * * *").greaterThanOrEqual(new Date("2016-04-18 15:43:00")), "case 11");
    testWith(getCronTime("1 43 15 * * * 2015").lessThan(new Date("2016-04-18 15:43:00")), "case 12");
    testWith(getCronTime("1 43 15 * 5 * 2016").greaterThan(new Date("2016-04-18 15:43:00")), "case 13");
    testWith(getCronTime("1 43 15 * * * 2015").lessThanOrEqual(new Date("2016-04-18 15:43:00")), "case 14");
    testWith(getCronTime("1 43 15 * 5 * 2016").greaterThanOrEqual(new Date("2016-04-18 15:43:00")), "case 15");
    testWith(getCronTime("1 43 15 17 4 * 2016").lessThan(new Date("2016-04-18 15:43:00")), "case 16");
}
*/

/**
 *
 * @param {string} cronStr
 * @returns {CronTime}
 */
function getCronTime(cronStr)
{
    return new CronTime(cronStr);
}


exports.getCronTime = getCronTime;