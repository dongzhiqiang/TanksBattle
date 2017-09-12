"use strict";

var skillLvValueConfig = require("../gameConfig/skillLvValueConfig");
var skillLvRateConfig = require("../gameConfig/skillLvRateConfig");
var propTypeConfig = require("../gameConfig/propTypeConfig");
var logUtil = require("../../libs/logUtil");

class LvValue
{
    /**
     *
     * @param {string} s
     */
    constructor(s) {
        var resultObj = {};
        if(!tryParse(s, resultObj))
        {
            this.isPercent = false;
            this.cfg = null;
            this.value = 0;
            this.error = true;
        }
        else
        {
            this.isPercent = resultObj.isPercent;
            this.cfg = resultObj.lvValueCfg;
            this.value = resultObj.val;
            this.isMinus = resultObj.isMinus;
            this.error = false;
        }
    }

    /**
     * @returns {number}
     */
    get()
    {
        if(this.cfg)
        {
            logUtil.error("逻辑错误，有配置的情况下获取了值:"+this.cfg.id);
        }

        return this.value;
    }

    /**
     *
     * @param {number} lv
     * @returns {number}
     */
    getByLv(lv)
    {
        if(!this.cfg)
        {
            return this.value;
        }

        var r=skillLvRateConfig.getSkillLvRateConfig(this.cfg.prefix, lv);
        if(!r)
        {
            return 0;
        }
        var v = this.cfg.value + r.rate * this.cfg.rate;
        if(this.isMinus)
            v = -v;
        return this.isPercent ? v / 100 : v;
    }
}

class AddPropCxt
{
    /**
     *
     * @param {string} s
     */
    constructor(s) {
        var pp = s.split('|');
        if (pp.length < 2)
        {
            this.error = true;
            return;
        }

        var cfg = propTypeConfig.getByName(pp[0]);
        if (cfg == null)
        {
            this.error = true;
            return;
        }
        this.prop = cfg.id;

        this.value = new LvValue(pp[1]);
        if (this.value.error)
        {
            this.error = true;
        }
    }
}

/**
 *
 * @param {string} s
 * @param {object} resultObj
 * @returns {boolean}
 */
function tryParse(s, resultObj)
{
    resultObj.lvValueCfg = null;
    resultObj.isPercent = false;
    resultObj.val = 0;
    resultObj.isMinus = false;
    if (!s)
        return false;

    var startIdx = 0;
    var endIdx = s.length - 1;

    //百分比
    resultObj.isPercent = s[endIdx] == '%';
    if (resultObj.isPercent)
        endIdx = endIdx - 1;

    //值
    if(s[endIdx] != '}')
    {
        var val = parseFloat(s.substr(startIdx, endIdx - startIdx + 1));
        if (!isNaN(val))
        {
            if (resultObj.isPercent)
                val = val / 100;

            resultObj.isMinus = val < 0;
            resultObj.val = val;
            return true;
        }
    else
        return false;
    }


    //检查正负号
    if(s[startIdx] == '-')
    {
        resultObj.isMinus = true;
        startIdx += 1;
    }

    if (s[startIdx] != '{' )
        return false;

    //等级值
    startIdx += 1;
    endIdx -= 1;
    s = s.substr(startIdx, endIdx- startIdx+1);
    resultObj.lvValueCfg = skillLvValueConfig.getSkillLvValueConfig(s);

    return resultObj.lvValueCfg != null;
}

exports.LvValue = LvValue;
exports.AddPropCxt = AddPropCxt;