"use strict";

var gameConfig = require("./gameConfig");

class EquipUpgradeCostConfig
{
    constructor() {
        this.id = "";
        this.cost = "";
    }

    static fieldsDesc() {
        return {
            id: {type: String},
            cost: {type: String},
        };
    }

    /**
     * 使用默认读取方式读完一行数据后可以执行对行对象的再次处理
     * 如果使用自定义读取方式，直接在那个自定义读取方式里处理就行了，不用这个函数了
     * 这个函数可选，没有就不执行
     * @param {object} row - 行数据对象
     */
    static afterDefReadRow(row)
    {
        var result = {};
        var itemStrs = row.cost.split(",");
        for(var k=0;k<itemStrs.length;k++)
        {
            if(itemStrs[k]!="")
            {
                var itemOneStr = itemStrs[k].split("|");
                var itemId = parseInt(itemOneStr[0]);
                result[itemId] = parseInt(itemOneStr[1]);
            }
        }
        row.cost = result;
    }
}

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {EquipUpgradeCostConfig}
 */
function getEquipUpgradeCostConfig(key)
{
    return gameConfig.getCsvConfig("equipUpgradeCost", key);
}

exports.EquipUpgradeCostConfig = EquipUpgradeCostConfig;
exports.getEquipUpgradeCostConfig = getEquipUpgradeCostConfig;