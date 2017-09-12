"use strict";

var logUtil = require("../../libs/logUtil");
var equipConfig = require("../gameConfig/equipConfig");
var propValueConfig = require("../gameConfig/propValueConfig");
var equipAdvanceRateConfig = require("../gameConfig/equipAdvanceRateConfig");
var equipRouseRateConfig = require("../gameConfig/equipRouseRateConfig");
var propertyTable = require("../gameConfig/propertyTable");
var enProp = require("../enumType/propDefine").enProp;
var propDistributeConfig = require("../gameConfig/propDistributeConfig");
var roleTypePropConfig = require("../gameConfig/roleTypePropConfig");
var roleLvPropConfig = require("../gameConfig/roleLvPropConfig");
var propBasicConfig = require("../gameConfig/propBasicConfig");

var enEquipPos = {
    minNormal   : 0,    //为了编程方便，定一个最小值枚举
    shoulder    : 0,    //护肩
    hand        : 1,    //护手
    waist       : 2,    //护腰
    boots       : 3,    //护靴
    amulet      : 4,    //护身符
    ring        : 5,    //神戒
    maxNormal   : 5,    //为了编程方便，定一个最大值枚举
    minWeapon   : 6,    //为了编程方便，定一个最小值枚举
    weapon1     : 6,    //混沌之刃
    weapon2     : 7,    //宙斯之剑
    weapon3     : 8,    //斯巴达武装
    weapon4     : 9,    //野蛮之锤
    maxWeapon   : 9,    //为了编程方便，定一个最大值枚举
    equipCount  : 10,   //装备数量
};

class Equip
{
    constructor(data, index)
    {
        /**
         * 这个值可以修改，但必须新的装备ID对应的位置索引一样
         */
        this.equipId = data.equipId;
        this.level = data.level;
        this.advLv = data.advLv;

        /**
         * 位置索引，存盘就是按位置索引来的，所以这个不要被枚举
         * @type {number}
         */
        Object.defineProperty(this, "_index", {enumerable: false, writable: true, value: index});
        /**
         * 角色
         * @type {Role|null}
         */
        Object.defineProperty(this, "_owner", {enumerable: false, writable: true, value: null});

        //logUtil.debug("Equip创建，equipId：" + this.equipId + "，posIndex：" + this._index);
    }

    getCopyedData()
    {
        var result = {};
        result.equipId = this.equipId;
        result.level = this.level;
        result.advLv = this.advLv;
        return result;
    }

    /**
     *
     * @returns {*|EquipConfig}
     */
    getEquipCfg(){
        return equipConfig.getEquipConfig(this.equipId);
    }

    /**
     *
     * @param {Role} v
     */
    setOwner(v)
    {
        this._owner = v;
    }

    /**
     *
     * @returns {Role|null}
     */
    getOwner()
    {
        return this._owner;
    }

    /**
     *
     * @returns {number}
     */
    getPosIndex()
    {
        return this._index;
    }

    /**
     * 当已在数据库时，存盘
     */
    syncAndSave()
    {
        if (!this._owner)
            return;

        var part = this._owner.getEquipsPart();
        if (!part)
            return;

        part.syncAndSaveEquip(this._index);
    }

    release()
    {
        if (this._owner)
            logUtil.debug("Equip销毁，角色guid：" + this._owner.getGUID() + "，equipId：" + this.equipId);
        else
            logUtil.debug("Equip销毁，equipId：" + this.equipId);
    }

    getEquipBaseProp(props, equipCfg, level, advLv, star)
    {
        var propCfg = propValueConfig.getPropValueConfig(equipCfg.attributeId);
        var advanceRateCfg = equipAdvanceRateConfig.getEquipAdvanceRateConfig(advLv);
        var rouseRateCfg = equipRouseRateConfig.getEquipRouseRateConfig(star);
        propertyTable.mulValue(1 + advanceRateCfg.baseRate + rouseRateCfg.baseRate, propCfg.props, props);

        var lvRate = roleLvPropConfig.getRoleLvPropConfig(level).rate * propBasicConfig.getPropBasicConfig().equipPoint * (1 + advanceRateCfg.lvRate + rouseRateCfg.lvRate);
        var temp ={};

        propertyTable.mul(roleTypePropConfig.getRoleTypeProp(), propDistributeConfig.getPropDistributeConfig(equipCfg.attributeAllocateId).props, temp);
        propertyTable.mulValue(lvRate, temp, temp);
        propertyTable.add(props, temp, props);

        var equipPower = equipCfg.power*(1 + advanceRateCfg.baseRate + rouseRateCfg.baseRate);
        //logUtil.info(equipCfg.power+" * ( 1 +"+advanceRateCfg.baseRate+" + "+rouseRateCfg.baseRate+" ) = "+equipPower);
        equipPower += lvRate*equipCfg.powerRate;
        //logUtil.info(propBasicConfig.getPropBasicConfig().equipPoint + " * "+equipCfg.powerRate+" * "+roleLvPropConfig.getRoleLvPropConfig(level).rate
        //    + " * ( 1 + "+advanceRateCfg.lvRate+" + "+rouseRateCfg.lvRate+" ) = "+lvRate*equipCfg.powerRate);
        //logUtil.info("+ = " + equipPower);
        equipPower *= propBasicConfig.getPropBasicConfig().powerRate;
        //logUtil.info("* "+propBasicConfig.getPropBasicConfig().powerRate+" = "+equipPower);
        props[enProp.power] = equipPower;
    }

    getBaseProp(props)
    {
        var equipCfg = equipConfig.getEquipConfig(this.equipId);
        this.getEquipBaseProp(props, equipCfg, this.level, this.advLv, equipCfg.star);
    }
}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isEquipData(data)
{
    return !!(data && data.equipId > 0 && data.level >= 0 && data.advLv > 0);
}

/**
 *
 * @param data
 * @returns {Equip|null}
 */
function createEquip(data)
{
    if (!isEquipData(data))
    {
        logUtil.error("Equip基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var equipCfg = equipConfig.getEquipConfig(data.equipId);
    if (!equipCfg)
    {
        logUtil.error("EquipId无效，equipId：" + data.equipId);
        return null;
    }

    //由于posIndex不会轻易修改，所以可以直接从配置里取index然后缓存到Equip对象
    return new Equip(data, equipCfg.posIndex);
}

exports.enEquipPos = enEquipPos;
exports.isEquipData = isEquipData;
exports.createEquip = createEquip;