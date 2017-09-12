"use strict";

var enProp = require("../enumType/propDefine").enProp;
var enPropFight = require("../enumType/propDefine").enPropFight;
var propTypeConfig = require("../gameConfig/propTypeConfig");

var propTypeTable =
{
    1:"hp",
    2:"atk",
    3:"def",
    4:"damageDef",
    5:"damage",
    6:"critical",
    7:"criticalDef",
    8:"criticalDamage",
    9:"fire",
    10:"ice",
    11:"thunder",
    12:"dark",
    13:"fireDef",
    14:"iceDef",
    15:"thunderDef",
    16:"darkDef",
    17:"hpCut",
    18:"damageReflect",
    19:"mp",
    20:"speed",
    21:"cdCut",
    22:"shield"

}

/**
 *
 * @param {object} paramList
 * @returns {object}
 */
function addFightPropParamList(paramList)
{
    paramList["hp"] = {type: Number};
    paramList["atk"] = {type: Number};
    paramList["def"] = {type: Number};
    paramList["damageDef"] = {type: Number};
    paramList["damage"] = {type: Number};
    paramList["critical"] = {type: Number};
    paramList["criticalDef"] = {type: Number};
    paramList["criticalDamage"] = {type: Number};
    paramList["fire"] = {type: Number};
    paramList["ice"] = {type: Number};
    paramList["thunder"] = {type: Number};
    paramList["dark"] = {type: Number};
    paramList["fireDef"] = {type: Number};
    paramList["iceDef"] = {type: Number};
    paramList["thunderDef"] = {type: Number};
    paramList["darkDef"] = {type: Number};
    paramList["hpCut"] = {type: Number};
    paramList["damageReflect"] = {type: Number};
    paramList["mp"] = {type: Number};
    paramList["speed"] = {type: Number};
    paramList["cdCut"] = {type: Number};
    paramList["shieldMax"] = {type: Number};
    return paramList;
}

/**
 *
 * @param {object} row
 * @returns {object}
 */
function getPropertyTableFromRow(row)
{
    var result = {};
    for(var i=enPropFight.minFightProp+1; i<enPropFight.maxFightProp; i++)
    {
        result[i] = row[propTypeTable[i]];
    }
    return result;
}

/**
 *
 * @param {object} source
 * @param {object} source2
 * @param {object} target
 */
function add(source, source2, target)
{
    for(var i=enPropFight.minFightProp+1; i<enPropFight.maxFightProp; i++)
    {
        var p1 = source[i] || 0;
        var p11 = source2[i] || 0;
        target[i] = p1+p11;
    }
}

/**
 *
 * @param {object} source
 * @param {object} source2
 * @param {object} target
 */
function mul(source, source2, target)
{
    for(var i=enPropFight.minFightProp+1; i<enPropFight.maxFightProp; i++)
    {
        var p1 = source[i] || 0;
        var p11 = source2[i] || 0;
        target[i] = p1*p11;
    }
}

/**
 *
 * @param {number} f
 * @param {object} source
 * @param {object} target
 */
function mulValue(f, source, target)
{
    for(var i=enPropFight.minFightProp+1; i<enPropFight.maxFightProp; i++)
    {
        var p1 = source[i] || 0;
        target[i] = p1*f;
    }
}

/**
 *
 * @param {number} value
 * @param {object} target
 */
function set(value, target)
{
    for(var i=enPropFight.minFightProp+1; i<enPropFight.maxFightProp; i++)
    {
        target[i] = value;
    }
}

/**
 *
 * @param {object} source
 * @param {object} target
 */
function copy(source, target)
{
    for(var i=enPropFight.minFightProp+1; i<enPropFight.maxFightProp; i++)
    {
        var p1 = source[i] || 0;
        target[i] = p1;
    }
}

/**
 *
 * @param {object} source
 * @returns {number}
 */
function sum(source)
{
    var result = 0;
    for(var i=enPropFight.minFightProp+1; i<enPropFight.maxFightProp; i++)
    {
        var p1 = source[i] || 0;
        result += p1;
    }
    return result;
}

exports.addFightPropParamList = addFightPropParamList;
exports.getPropertyTableFromRow = getPropertyTableFromRow;
exports.add = add;
exports.mul = mul;
exports.mulValue = mulValue;
exports.set = set;
exports.copy = copy;
exports.sum = sum;