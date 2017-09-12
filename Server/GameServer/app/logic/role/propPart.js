"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var roleMessage = require("../netMessage/roleMessage");
var CmdIdsRole = require("../netMessage/roleMessage").CmdIdsRole;
var roleConfig = require("../gameConfig/roleConfig");
var globalDefine = require("../enumType/globalDefine").globalDefine;
var lvExpConfig = require("../gameConfig/lvExpConfig");
var petUpgradeCostConfig = require("../gameConfig/petUpgradeCostConfig");
var propDefine = require("../enumType/propDefine");
var dateUtil = require("../../libs/dateUtil");
var valueConfig = require("../gameConfig/valueConfig");
var eventNames = require("../enumType/eventDefine").eventNames;
var propertyTable = require("../gameConfig/propertyTable");
var recoverUtil = require("../../libs/recoverUtil");

////////////////////////////////////////
const PF_PROTECT = propDefine.PF_PROTECT;
const PF_PUBLIC = propDefine.PF_PUBLIC;
const PF_SAVEDB = propDefine.PF_SAVEDB;
const enProp = propDefine.enProp;
const enPropFight = propDefine.enPropFight;
const protectPropNames = propDefine.protectPropNames;
const publicPropNames = propDefine.publicPropNames;
const saveDBPropNames = propDefine.saveDBPropNames;
const allPropNeedSave = propDefine.allPropNeedSave;
const propConfigMap = propDefine.propConfigMap;

/**
 * 如果是单独修改角色属性，调用这个函数更新排行榜
 * 参数分别是（角色对象，属性名，属性值）
 * @type {function(Role, string, *)}
 */
var updateRankFuncByOneProp = null;

/**
 * 如果是批量修改角色属性，调用这个函数更新排行榜
 * 参数分别是（角色对象，属性名列表）
 * @type {function(Role, string[])}
 */

var updateRankFuncByProps = null;
////////////////////////////////////////

class PropPart
{
    /**
     *
     * @param {Role} role
     * @param {object.<string, *>} data
     */
    constructor(role, data)
    {
        this._props = data.props || {};

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});
        /**
         * 当前是否处于批量修改属性的状态，如果是，那就等结束批量修改后再同步到客户端、存盘
         * @type {boolean}
         */
        Object.defineProperty(this, "_inBatch", {enumerable: false, writable:true, value: false});
        /**
         * 属性计算部位中间值
         * @type {object}
         */
        Object.defineProperty(this, "_partValues", {enumerable: false, writable:true, value: {}});
        /**
         * 属性计算部位中间值
         * @type {object}
         */
        Object.defineProperty(this, "_partRates", {enumerable: false, writable:true, value: {}});
        /**
         * 属性计算中间值
         * @type {object}
         */
        Object.defineProperty(this, "_values", {enumerable: false, writable:true, value: {}});
        /**
         * 属性计算中间值
         * @type {object}
         */
        Object.defineProperty(this, "_rates", {enumerable: false, writable:true, value: {}});
        /**
         * 战斗属性值
         * @type {object}
         */
        Object.defineProperty(this, "_fightProps", {enumerable: false, writable:true, value: {}});

        //添加事件处理
        var thisObj = this;
        role.addListener(function(eventName, context, notifier) {
            thisObj.onPartFresh();
        }, eventNames.LEVEL_UP);
        role.addListener(function(eventName, context, notifier) {
            thisObj.onPartFresh();
        }, eventNames.ADV_LV_CHANGE);
        role.addListener(function(eventName, context, notifier) {
            thisObj.onPartFresh();
        }, eventNames.STAR_CHANGE);
    }

    release()
    {
        //以防万一，检测一下批量保存的
        this.endBatch();
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        var data;
        if (allPropNeedSave)
        {
            data = this._props;
        }
        else
        {
            data = {};
            var props = this._props;
            //经测试，发现for in 效率比Object.keys再for i更高
            for (var key in props)
            {
                if (saveDBPropNames[key])
                    data[key] = props[key];
            }
        }
        rootObj.props = data;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.props = this._props;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        var data = {};
        var props = this._props;
        //经测试，发现for in 效率比Object.keys再for i更高
        for (var key in props)
        {
            if (publicPropNames[key])
                data[key] = props[key];
        }
        rootObj.props = data;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        var data = {};
        var props = this._props;
        //经测试，发现for in 效率比Object.keys再for i更高
        for (var key in props)
        {
            if (protectPropNames[key])
                data[key] = props[key];
        }
        rootObj.props = data;
    }

    /**
     *
     * @param {number} enumVal
     * @returns {number}
     */
    getNumber(enumVal)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propConfig = propConfigMap[enumVal || 0];
        if (!propConfig)
            return 0;

        var propName = propConfig.name;
        var propVal = this._props[propName];
        if (propVal === null || propVal === undefined)
            //为防止性能问题，填上默认值
            this._props[propName] = propVal = 0;

        return propVal;
    }

    /**
     *
     * @param {number} enumVal
     * @returns {string}
     */
    getString(enumVal)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propConfig = propConfigMap[enumVal || 0];
        if (!propConfig)
            return "";

        var propName = propConfig.name;
        var propVal = this._props[propName];
        if (propVal === null || propVal === undefined)
            //为防止性能问题，填上默认值
            this._props[propName] = propVal = "";

        return propVal;
    }

    /**
     *
     * @param {number} enumVal
     * @param {number} newVal
     * @param {boolean?} noSync
     * @returns {number}
     */
    setNumber(enumVal, newVal, noSync)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propConfig = propConfigMap[enumVal || 0];
        if (!propConfig)
            return 0;

        if (!Object.isNumber(newVal))
            newVal = parseFloat(newVal) || 0;

        var propName = propConfig.name;
        var oldVal = this._props[propName];
        this._props[propName] = newVal;

        if (oldVal !== newVal)
            this._onChange(propName, newVal, noSync, (propConfig.flag & PF_SAVEDB) === 0);

        return newVal;
    }

    /**
     *
     * @param {number} enumVal
     * @param {string} newVal
     * @param {boolean?} noSync
     * @returns {string}
     */
    setString(enumVal, newVal, noSync)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propConfig = propConfigMap[enumVal || 0];
        if (!propConfig)
            return "";

        //如果不是string，转为string
        if (newVal === null || newVal === undefined)
            newVal = "";
        else if (!Object.isString(newVal))
            newVal += "";

        var propName = propConfig.name;
        var oldVal = this._props[propName];
        this._props[propName] = newVal;

        if (oldVal !== newVal)
            this._onChange(propName, newVal, noSync, (propConfig.flag & PF_SAVEDB) === 0);

        return newVal;
    }

    /**
     *
     * @param {number} enumVal
     * @param {number} delta
     * @param {boolean?} noSync
     * @returns {number}
     */
    addNumber(enumVal, delta, noSync)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propConfig = propConfigMap[enumVal || 0];
        if (!propConfig)
            return 0;

        //如果本来就是number，执行parseFloat，会性能不行，如果不是number，parseFloat性能本来就不行
        //所以一定要判断是否number
        if (!Object.isNumber(delta))
            delta = parseFloat(delta) || 0;

        var propName = propConfig.name;
        var oldVal = this._props[propName] || 0;
        var newVal = oldVal + delta;
        this._props[propName] = newVal;

        if (oldVal !== newVal)
            this._onChange(propName, newVal, noSync, (propConfig.flag & PF_SAVEDB) === 0);

        return newVal;
    }


    /**
     *
     * @param {number} expAdded
     */
    addExp(expAdded)
    {
        if(level >= globalDefine.MAX_ROLE_LEVEL)
        {
            return;
        }

        var level = this.getNumber(enProp.level);
        var oldLevel = level;
        var exp = this.getNumber(enProp.exp);
        var oldExp = exp;
        var roleCfg = roleConfig.getRoleConfig(this.getString(enProp.roleId));
        exp = exp + expAdded;
        var maxLevel = globalDefine.MAX_ROLE_LEVEL;
        if(this._role.isPet())
        {
            maxLevel = this._role.getOwner().getNumber(enProp.level);
        }

        while(level < maxLevel)
        {
            var needExp = 0;
            //宠物和主角使用不同的经验表
            if(this._role.isPet())
            {
                needExp = petUpgradeCostConfig.getPetUpgradeCostConfig(roleCfg.upgradeCostId+"_"+level).exp;
            }
            else
            {
                needExp = lvExpConfig.getLvExpConfig(level).needExp;
            }
            if(exp<needExp)
            {
                break;
            }
            exp -= needExp;
            level += 1;
        }
        if(level >= globalDefine.MAX_ROLE_LEVEL)
        {
            exp = 0;
        }
        if(oldExp == exp && oldLevel == level)
        {
            return;
        }

        this.startBatch();
        this.setNumber(enProp.exp, exp);
        if(level != oldLevel)
        {
            this.setNumber(enProp.level, level);

            if(!this._role.isPet())
            {
                //升级增加体力
                var staminaAdded = 0;
                for(var i=oldLevel; i<level; i++)
                {
                    staminaAdded += lvExpConfig.getLvExpConfig(i).upgradeStamina;
                }
                this.addStamina(staminaAdded);
            }

            //通知全局服更新登录时间
            this._role.updateGlobalServerInfo();

            //触发事件
            this._role.fireEvent(eventNames.LEVEL_UP, level);
        }
        this.endBatch();
    }

    /**
     *
     * @param {string[]} syncKeys
     * @param {string[]} saveKeys
     * @private
     */
    _doBatchSyncSave(syncKeys, saveKeys)
    {
        var syncLen = syncKeys.length;
        var saveLen = saveKeys.length;

        //同步、保存列表都空的？
        if (syncLen <= 0 && saveLen <= 0)
            return;

        //检测条件
        var roleCur = this._role;
        if (!roleCur.canSyncAndSave())
            return;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = this.getString(enProp.guid);
        var roleProps = this._props;

        if (syncLen > 0)
        {
            var syncObj = {};
            for (let i = 0; i < syncLen; ++i)
            {
                let key =  syncKeys[i];
                syncObj[key] = roleProps[key];
            }
            var syncMsg = new roleMessage.RoleSyncPropVo(guidCur, syncObj);
            roleTop.sendEx(ModuleIds.MODULE_ROLE, CmdIdsRole.PUSH_SYNC_PROP, syncMsg);
        }

        if (saveLen > 0)
        {
            var userId = roleTop.getString(enProp.userId);
            var heroId = roleTop.getNumber(enProp.heroId);
            var queryObj = isCurHero ? {"props.heroId":heroId} : {"props.heroId":heroId, "pets.props.guid":guidCur};
            var updatePrefx = isCurHero ? "props." : "pets.$.props.";
            var updateObj = {};

            for (let i = 0; i < saveLen; ++i)
            {
                let key =  saveKeys[i];
                updateObj[updatePrefx + key] = roleProps[key];
            }

            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow(queryObj, {"$set":updateObj});

            //一般要存盘的才是用于排行的
            if (updateRankFuncByProps && roleTop.isBuildOK() && roleCur.isBuildOK())
                updateRankFuncByProps(this._role, saveKeys);
        }
    }

    _doSingleSyncSave(propName, propVal, noSync, noSave)
    {
        //不同步、不保存？那就返回
        if (noSync && noSave)
            return;

        //检测条件
        var roleCur = this._role;
        if (!roleCur.canSyncAndSave())
            return;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = this.getString(enProp.guid);

        //同步
        if (!noSync)
        {
            var syncMsg = new roleMessage.RoleSyncPropVo(guidCur, {[propName]: propVal});
            roleTop.sendEx(ModuleIds.MODULE_ROLE, CmdIdsRole.PUSH_SYNC_PROP, syncMsg);
        }

        //存盘
        if (!noSave)
        {
            var userId = roleTop.getString(enProp.userId);
            var heroId = roleTop.getNumber(enProp.heroId);
            var queryObj = isCurHero ? {"props.heroId":heroId} : {"props.heroId":heroId, "pets.props.guid":guidCur};
            var updateKey = isCurHero ? "props." + propName : "pets.$.props." + propName;
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow(queryObj, {"$set":{[updateKey] : propVal}});

            if (updateRankFuncByOneProp && roleTop.isBuildOK() && roleCur.isBuildOK())
                updateRankFuncByOneProp(this._role, propName, propVal);
        }
    }

    _onChange(propName, propVal, noSync, noSave)
    {
        //如果是批量修改，加入批量修改缓存
        if (this._inBatch)
        {
            if (!noSync)
                this._batchSyncKeys.pushIfNotExist(propName);
            if (!noSave)
                this._batchSaveKeys.pushIfNotExist(propName);
        }
        //否则直接存盘、同步
        else
        {
            this._doSingleSyncSave(propName, propVal, noSync, noSave);
        }
    }

    startBatch()
    {
        this._inBatch = true;
        //这两个属是性延迟加入，以免用不着时浪费
        if (!this._batchSyncKeys)
            Object.defineProperty(this, "_batchSyncKeys", {enumerable: false, writable:true, value: []});
        if (!this._batchSaveKeys)
            Object.defineProperty(this, "_batchSaveKeys", {enumerable: false, writable:true, value: []});
    }

    endBatch()
    {
        if (this._inBatch)
        {
            var syncKeys = this._batchSyncKeys;
            var saveKeys = this._batchSaveKeys;

            //提前置空，以防下面的函数导致意外问题，导致本函数重复执行
            this._inBatch = false;
            this._batchSyncKeys = [];
            this._batchSaveKeys = [];

            this._doBatchSyncSave(syncKeys, saveKeys);
        }
    }

    getStamina()
    {
        var staminaNum = this.getNumber(enProp.stamina);
        var staminaTime = this.getNumber(enProp.staminaTime);
        var recoverTIme = valueConfig.getNumber("StaminaRecoverTime");
        var recoverMax = lvExpConfig.getLvExpConfig(this.getNumber(enProp.level)).maxStamina;
        staminaTime = Math.max(staminaTime, 0);
        var num = recoverUtil.getNum(staminaTime, staminaNum, recoverTIme, recoverMax);
        num = Math.max(0, num);
        staminaNum = Math.max(0, staminaNum);
        if (num != staminaNum)
        {
            staminaNum = num;
            this.setNumber(enProp.stamina, staminaNum, true);
            this.setNumber(enProp.staminaTime, dateUtil.getTimestamp(), true);
            return staminaNum;
        }
        else
        {
            return staminaNum;
        }
    }

    addStamina(delta)
    {
        this.setNumber(enProp.stamina, this.getStamina() + delta);
        this.setNumber(enProp.staminaTime, dateUtil.getTimestamp());
    }

    /**
     * 第一次计算属性值，对所有部件做一次重新计算
     */
    freshBaseProp()
    {
        var parts = this._role.getParts();
        for(var i=0; i<parts.length; i++)
        {
            var part = parts[i];
            if(part.freshPartProp) {
                part.freshPartProp();
            }
        }

        this.freshBasePropUpdate();
    }

    /**
     * 计算部件的属性
     */
    freshPartProp()
    {
        var roleCfg = roleConfig.getRoleConfig(this.getString(enProp.roleId));
        //值属性
        roleCfg.getBaseProp(this._partValues, this.getNumber(enProp.level), this.getNumber(enProp.advLv), this.getNumber(enProp.star));//自身成长属性

        //百分比属性
        propertyTable.set(0,  this._partRates);//暂时没有需要用的，这里置0
    }

    /**
     * 在计算完所有部件的属性(或者更新单个部件的属性)之后，进行最后的累加和结算
     */
    freshBasePropUpdate()
    {
        propertyTable.set(0, this._values);
        this._values[enProp.power] = 0;
        propertyTable.set(0, this._rates);
        this._rates[enProp.power] = 0;
        var parts = this._role.getParts();
        for(var i=0; i<parts.length; i++)
        {
            var part = parts[i];
            if(part.onFreshBaseProp) {
                part.onFreshBaseProp(this._values, this._rates);
            }
        }

        for(i=enPropFight.minFightProp+1; i<enPropFight.maxFightProp; i++)
        {
            this.freshProp(i);
        }
        this.freshProp(enProp.power);
        //logUtil.info("power v:"+this._values[enProp.power]);
        //logUtil.info("power r:"+this._rates[enProp.power]);
        this.setNumber(enProp.power, Math.floor(this._fightProps[enProp.power]));

        //要先更新自己的，不然如果自己的宠物，下面的更新主人的总战斗力就不准了
        this.updateFullPower();

        if( this._role.isPet() )
            this._role.getOwner().getPropsPart().updateFullPower();

        //logUtil.info("power:"+this._fightProps[enProp.power]);
    }

    updateFullPower()
    {
        if( this._role.isPet() )
        {
            this.setNumber(enProp.powerTotal, this.getNumber(enProp.power));
        }
        else
        {
            var allPetPower = 0;
            var mainPetPower = 0;
            var maxPower = Number.MIN_VALUE;
            var maxPowerPet = null;
            var petsPart = this._role.getPetsPart();
            for (var i = 0, len = petsPart.getPetCount(); i < len; ++i)
            {
                var rolePet = petsPart.getPetByIndex(i);
                var power = rolePet.getNumber(enProp.power);

                allPetPower += power;

                if (petsPart.isMainPet(rolePet.getGUID()))
                    mainPetPower += power;

                if (power > maxPower)
                {
                    maxPower = power;
                    maxPowerPet = rolePet;
                }
            }

            this.startBatch();
            this.setString(enProp.maxPowerPet, maxPowerPet ? maxPowerPet.getGUID() : "");
            this.setNumber(enProp.powerPets, allPetPower);
            this.setNumber(enProp.powerTotal, this.getNumber(enProp.power) + mainPetPower);
            this.endBatch();
        }

        this._role.fireEvent(eventNames.POWER_CHANGE);
    }

    /**
     * 累加部件的属性
     */
    onFreshBaseProp(values, rates)
    {
        propertyTable.add(values, this._partValues, values);
        propertyTable.add(rates, this._partRates, rates);
        values[enProp.power] = values[enProp.power] + (this._partValues[enProp.power] || 0);
        rates[enProp.power] = rates[enProp.power] + (this._partRates[enProp.power] || 0);
    }

    freshProp(prop)
    {
        this._fightProps[prop] = this._values[prop]*(1+this._rates[prop]);
        //logUtil.info(prop+":"+this._fightProps[prop]);
    }

    onPartFresh()
    {
        //logUtil.info("Fresh part:propPart");
        this.freshPartProp();
        this.onFreshBasePropUpdate();
    }

    onFreshBasePropUpdate()
    {
        this.freshBasePropUpdate();
        this._role.fireEvent(eventNames.PROP_UPDATED);
    }
}

function setUpdateRankFunc(onePropFunc, propsFunc)
{
    updateRankFuncByOneProp = onePropFunc;
    updateRankFuncByProps = propsFunc;
}

exports.PropPart = PropPart;
exports.setUpdateRankFunc = setUpdateRankFunc;