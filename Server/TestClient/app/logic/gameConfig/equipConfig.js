"use strict";

var gameConfig = require("./gameConfig");

class EquipConfig
{
    constructor() {
        this.id = 0;
        this.name = "";
        this.posIndex = 0;
        this.stateId = "";
        this.attributeId = "";
        this.attributeAllocateId = "";
        this.isSell = false;
        this.priceId = "";
        this.star = 0;
        this.rouseEquipId = 0;
        this.rouseCostId = "";
        this.weaponId = 0;
        this.icon = "";
        this.rouseDescription = "";
        this.power = 0;
        this.powerRate = 0;
        this.powerMul = 0;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            name: {type: String},
            posIndex: {type: Number},
            stateId: {type: String},
            attributeId: {type: String},
            attributeAllocateId: {type: String},
            isSell: {type: Boolean},
            priceId: {type: String},
            star: {type: Number},
            rouseEquipId: {type: Number},
            rouseCostId: {type: String},
            weaponId: {type: Number},
            icon: {type: String},
            rouseDescription: {type: String},
            power: {type: Number},
            powerRate: {type: Number},
            powerMul: {type: Number},
        };
    }

    /**
     * 用于计算装备在不同星级下对应的装备ID
     * @param {object.<number, EquipConfig>} rows
     */
    static afterReadAll(rows)
    {
        EquipConfig.EquipStarIdMap = {};
        for (var equipId in rows)   //注意equipId是字符串，如果要当整数用，请用parseInt(x,10)转成整数
        {
            var equipCfg = rows[equipId];
            if (equipCfg.star === 0)
            {
                var startIdMap = {};
                do
                {
                    startIdMap[equipCfg.star] = equipCfg.id;
                    equipCfg = equipCfg.rouseEquipId ? rows[equipCfg.rouseEquipId] : null;
                }
                while (equipCfg);

                for (var star in startIdMap)    //注意star是字符串，如果要当整数用，请用parseInt(x,10)转成整数
                {
                    var equipId2 = startIdMap[star];
                    EquipConfig.EquipStarIdMap[equipId2] = startIdMap;
                }
            }
        }
    }
}

/**
 * 格式：{任意装备ID：{星级，星级对应的装备ID}}
 * @type {object.<number, object.<number, number>>}
 */
EquipConfig.EquipStarIdMap = {};

/**
 *
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {EquipConfig}
 */
function getEquipConfig(key)
{
    return gameConfig.getCsvConfig("equip", key);
}

function getEquipIdByEquipIdAndStar(equipId, star)
{
    var startIdMap = EquipConfig.EquipStarIdMap[equipId];
    if (!startIdMap)
        return 0;
    return startIdMap[star] || 0;
}

exports.EquipConfig = EquipConfig;
exports.getEquipConfig = getEquipConfig;
exports.getEquipIdByEquipIdAndStar = getEquipIdByEquipIdAndStar;