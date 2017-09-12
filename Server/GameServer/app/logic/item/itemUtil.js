"use strict";

var itemConfig = require("../gameConfig/itemConfig");
var enProp = require("../enumType/propDefine").enProp;
var propConfigMap = require("../enumType/propDefine").propConfigMap;
var enItemId = require("../enumType/globalDefine").enItemId;

/**
 * 有些物品是对应属性的，不是实际物品，要特别处理
 * 我们把这类物品叫“虚拟物品”
 * @type {object.<number, number>}
 */
const ITEMID_ROLEPROP_MAP = {
    [enItemId.GOLD] : enProp.gold,
    [enItemId.EXP] : enProp.exp,
    [enItemId.DIAMOND] : enProp.diamond,
    [enItemId.STAMINA] : enProp.stamina,
    [enItemId.ARENA_COIN] : enProp.arenaCoin,
};

/**
 *
 * @param {number} itemId
 * @returns {number}
 */
function getItemPropId(itemId)
{
    return ITEMID_ROLEPROP_MAP[itemId];
}

/**
 *
 * @param {string} roleId
 * @param {object?} props - 用于接收虚拟物品的{enProp : Number}
 * @returns {Item[]}
 */
function getInitItems(roleId, props)
{
    var items = [];

    var cfg = itemConfig.getItemInitListConfig(roleId);
    if (!cfg)
        return items;

    var cfgItems = cfg.items;

    for (var i = 0, len = cfgItems.length; i < len; ++i)
    {
        var item = cfgItems[i];
        var propId = getItemPropId(item.itemId);
        if (propId)
        {
            if (props)
            {
                var propCfg = propConfigMap[propId];
                if (propCfg != null)
                    props[propCfg.name] = item.num;
            }
        }
        else
        {
            items.push(item);
        }
    }

    return items;
}

////////////导出元素////////////
exports.getItemPropId = getItemPropId;
exports.getInitItems = getInitItems;