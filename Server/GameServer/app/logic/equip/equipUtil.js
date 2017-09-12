"use strict";

var roleConfig = require("../gameConfig/roleConfig");
var equipInitListConfig = require("../gameConfig/equipInitListConfig");
var equipConfig = require("../gameConfig/equipConfig");
var logUtil = require("../../libs/logUtil");

/**
 *
 * @param {string} roleCfgId
 * @returns {Array}
 */
function getInitEquips(roleCfgId)
{
    var roleCfg = roleConfig.getRoleConfig(roleCfgId);
    var equipInit = equipInitListConfig.getEquipInitListConfig(roleCfg.initEquips);
    if(!equipInit)
    {
        logUtil.error("找不到初始装备表，角色id:"+roleCfgId);
    }
    var result = [];
    var equipsAry = equipInit.equips;
    var key;
    if(equipsAry.length == 10)
    {
        for(key=0;key<equipsAry.length;key++ )
        {
            result.push({equipId:equipsAry[key],level:1,advLv:1});
        }
    }
    else if(equipsAry.length == 7)
    {
        for(key=0;key<equipsAry.length;key++ )
        {
            result.push({equipId:equipsAry[key],level:1,advLv:1});
            if(result.length == 7)
            {
                result.push(null);
                result.push(null);
                result.push(null);
            }
        }
    }

    return result;
}

function getEquipIdByEquipIdAndStar(equipId, star)
{
    return equipConfig.getEquipIdByEquipIdAndStar(equipId, star);
}

////////////导出元素////////////
exports.getInitEquips = getInitEquips;
exports.getEquipIdByEquipIdAndStar = getEquipIdByEquipIdAndStar;