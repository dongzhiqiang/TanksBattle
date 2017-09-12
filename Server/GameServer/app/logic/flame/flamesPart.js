"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var flameModule = require("../flame/flame");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var flameMessage = require("../netMessage/flameMessage");
var CmdIdsFlame = require("../netMessage/flameMessage").CmdIdsFlame;
var flameConfig = require("../gameConfig/flameConfig");
var flameLevelConfig = require("../gameConfig/flameLevelConfig");
var propertyTable = require("../gameConfig/propertyTable");
var enProp = require("../enumType/propDefine").enProp;
var enPropFight = require("../enumType/propDefine").enPropFight;
var propValueConfig = require("../gameConfig/propValueConfig");
var eventNames = require("../enumType/eventDefine").eventNames;

class FlamesPart
{
    constructor(ownerRole, data)
    {
        /**
         * @type {Flame[]}
         * @private
         */
        this._flames = [];

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});
        Object.defineProperty(this, "_flameIdFlameMap", {enumerable: false, writable:true, value: {}});

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

        try {
            var flames = data.flames || [];
            for (var i = 0; i < flames.length; ++i) {
                var flameData = flames[i];
                var flame = flameModule.createFlame(flameData);
                if (!flame) {
                    throw new Error("创建Flame失败");
                }

                //设置主人
                flame.setOwner(ownerRole);
                //加入列表
                this._flames.push(flame);
                this._flameIdFlameMap[flame.flameId] = flame;
            }

            //添加事件处理
            var thisObj = this;
            ownerRole.addListener(function(eventName, context, notifier) {
                thisObj.onPartFresh();
            }, eventNames.FLAME_CHANGE);
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        var flames = this._flames;
        for (var i = 0; i < flames.length; ++i)
        {
            flames[i].release();
        }
        this._flames = [];
        this._flameIdFlameMap = {};
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.flames = this._flames;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.flames = this._flames;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        rootObj.flames = this._flames;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        rootObj.flames = this._flames;
    }

    /**
     *
     * @param {Flame} flame
     * @private
     */
    _addFlame(flame)
    {
        var flameId = flame.flameId;
        this._flames.push(flame);
        this._flameIdFlameMap[flameId] = flame;
    }

    /**
     *
     * @param {Flame|number} val
     * @private
     */
    _removeFlame(val)
    {
        var flameId = Object.isNumber(val) ? val : val.flameId;
        this._flames.removeValue(this._flameIdFlameMap[flameId]);
        delete this._flameIdFlameMap[flameId];
    }

    /**
     *
     * @param {number} flameId
     * @returns {Flame|undefined}
     */
    getFlameByFlameId(flameId)
    {
        return this._flameIdFlameMap[flameId];
    }


    /**
     *
     * @param {object} data
     * @return {boolean} 如果添加成功就返回true
     */
    addFlameWithData(data)
    {
        //必须有最基本数据
        //因为下面要获取flameId，所以要先判断
        if (!flameModule.isFlameData(data))
            return false;

        var flameId = data.flameId;
        //已存在？不能添加
        if (this.getFlameByFlameId(flameId))
            return false;

        var flame = flameModule.createFlame(data);
        if (!flame)
            return false;

        //设置主人
        var roleCur = this._role;
        flame.setOwner(roleCur);

        //添加到列表
        this._addFlame(flame);

        //检测条件
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;
        queryObj = {"props.heroId":heroId};
        updateObj = {$push:{"flames":flame}};

        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new flameMessage.AddOrUpdateFlameVo(true, flame);
        roleTop.sendEx(ModuleIds.MODULE_FLAME, CmdIdsFlame.PUSH_ADD_OR_UPDATE_FLAME, netMsg);
        return true;
    }


    //用于保存已在数据库的数据
    syncAndSaveFlame(flameId)
    {
        var flame = this._flameIdFlameMap[flameId];
        //不存在？不能继续
        if (!flame)
            return false;

        //检测条件
        var roleCur = this._role;
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;


        queryObj = {"props.heroId":heroId, "flames":{$elemMatch:{"flameId":flameId}}};
        updateObj =  {$set: {"flames.$": flame}};
        col.updateOneNoThrow(queryObj, updateObj);


        //通知客户端
        var netMsg = new flameMessage.AddOrUpdateFlameVo(false, flame);
        roleTop.sendEx(ModuleIds.MODULE_FLAME, CmdIdsFlame.PUSH_ADD_OR_UPDATE_FLAME, netMsg);
        return true;
    }

    /**
     * 计算部件的属性
     */
    freshPartProp()
    {
        propertyTable.set(0,  this._partValues);
        propertyTable.set(0,  this._partRates);
        this._partValues[enProp.power] = 0;
        this._partRates[enProp.power] = 0;

        var tempProp = {};

        for (var i =0; i<this._flames.length; i++)
        {
            var flame = this._flames[i];
            //var flameCfg = flameConfig.getFlameConfig(flame.flameId);
            var levelCfg = flameLevelConfig.getFlameLevelConfig(flame.flameId, flame.level);
            if(!levelCfg)
            {
                continue;
            }
            var valueCfg = propValueConfig.getPropValueConfig(levelCfg.attributeId);
            if(!valueCfg)
            {
                continue;
            }
            propertyTable.add(this._partValues, valueCfg.props, this._partValues);
            this._partValues[enProp.power] = this._partValues[enProp.power] + levelCfg.power;
            this._partRates[enProp.power] = this._partRates[enProp.power] + levelCfg.powerRate;
        }
        //logUtil.info("圣火 角色增加生命值:" + this._partValues[enPropFight.hpMax]);
        //logUtil.info("圣火 角色增加战斗力:" + this._partValues[enProp.power]);
        //logUtil.info("圣火 角色增加战斗力系数:" + this._partRates[enProp.power]);
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

    onPartFresh()
    {
        //logUtil.info("Fresh part:flamesPart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }
}

exports.FlamesPart = FlamesPart;