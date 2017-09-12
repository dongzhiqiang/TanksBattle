"use strict";

var dateUtil = require("../dateUtil");
var BaseProxy = require("./baseProxy").BaseProxy;

class DateProxy extends BaseProxy
{
    /**
     * 读取数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {number} flag - 类型标记
     * @return {Date} 读取的数据
     * @override
     */
    getValue(ioBuf, flag)
    {
        //var typeFlag = BaseProxy.getFlagType(flag);
        //if (typeFlag !== BaseProxy.TYPE_FLAG_DATETIME)
        //{
        //    throw new Error("DateProxy~getValue 必须是date类型");
        //}

        //时间戳，单位秒
        var timeStamp = BaseProxy.readVarUInt(ioBuf);
        return dateUtil.getDateFromTimestamp(timeStamp);
    }

    /**
     * 写入数据
     * @param {IOBuffer} ioBuf - 数据缓冲
     * @param {Date} value - 数据
     * @override
     */
    setValue(ioBuf, value)
    {
        //if (!Object.isDate(value))
        //{
        //    throw new Error("DateProxy~setValue 必须是date类型");
        //}

        ioBuf.writeUInt8(BaseProxy.TYPE_FLAG_DATETIME);
        var timeStamp = Math.floor(value.getTime() / 1000);
        BaseProxy.writeVarUInt(ioBuf, timeStamp);
    }
}

exports.DateProxy = DateProxy;