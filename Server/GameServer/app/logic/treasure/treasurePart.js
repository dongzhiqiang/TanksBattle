
"use strict";

var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var treasureMessage = require("../netMessage/treasureMessage");
var CmdIdsTreasure = require("../netMessage/treasureMessage").CmdIdsTreasure;
var treasureModule =require("./treasure");
var dbUtil = require("../../libs/dbUtil");
var propertyTable = require("../gameConfig/propertyTable");
var eventNames = require("../enumType/eventDefine").eventNames;
var logUtil = require("../../libs/logUtil");
var enProp = require("../enumType/propDefine").enProp;
var treasureConfig = require("../gameConfig/treasureConfig");
var propValueConfig = require("../gameConfig/propValueConfig");
var enPropFight = require("../enumType/propDefine").enPropFight;

class TreasurePart {
    constructor(role, data) {
        var data = data.treasures || {};

        //成员
        this.battleTreasure =data.battleTreasure || [];
        /**@type {Role}*/
        Object.defineProperty(this, "_role", {enumerable: false, value: role});

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

        /** @type {object.<number, Treasure>}*/
        this.treasures = {};

        try {
            let treasures = data.treasures|| {};
            for (var i in treasures) {
                var treasureData = treasures[i];
                this.treasures[treasureData.treasureId] = treasureModule.createTreasure(treasureData);
                //设置主人
                this.treasures[treasureData.treasureId].setOwner(role);
            }

            //添加事件处理
            var thisObj = this;
            role.addListener(function(eventName, context, notifier) {
                thisObj.onPartFresh();
            }, eventNames.TREASURE_CHANGE);
        }
        catch (err) {
            //清除已创建的
            this.release();
            err.message = "神器部件,{0}".format(err.message);
            throw err;
        }
    }


    release() {
        this.battleTreasure =[];
        this.treasures = {};
    }


    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj) {
        this.getPrivateNetData(rootObj);
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj) {
        let tem ={};
        rootObj.treasures=tem;

        tem.battleTreasure =this.battleTreasure;
        tem.treasures = this.treasures;
        console.log("神器:"+JSON.stringify(tem));
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        this.getPrivateNetData(rootObj);
    }

    save(key,value){
        //存盘


        //检测条件
        var roleCur = this._role;
        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;

        queryObj = {"props.heroId":heroId};
        updateObj = {$set:{[key] :value}};

        col.updateOneNoThrow(queryObj, updateObj);
    }


    //存盘和同步单个神器
    syncAndSaveTreasure(treasureId)
    {
        var treasure = this.treasures[treasureId];
        this.save("treasures.treasures." +treasureId, treasure);
        //通知客户端
        var netMsg = new treasureMessage.AddOrUpdateTreasureVo(false, treasure);
        this._role.sendEx(ModuleIds.MODULE_TREASURE, CmdIdsTreasure.PUSH_ADD_OR_UPDATE_TREASURE, netMsg);
        return true;
    }

    //存盘和同步神器出战
    syncAndSaveBattleTreasure()
    {
        this.save("treasures.battleTreasure", this.battleTreasure);
        //通知客户端
        var netMsg = new treasureMessage.UpdateBattleTreasureVo(this.battleTreasure);
        this._role.sendEx(ModuleIds.MODULE_TREASURE, CmdIdsTreasure.PUSH_UPDATE_BATTLE_TREASURE, netMsg);
        return true;
    }

    getTreasure(treasureId)
    {
        return this.treasures[treasureId];
    }

    addTreasure(treasureId)
    {
        if(this.treasures[treasureId])
        {
            return;
        }
        this.treasures[treasureId] = treasureModule.createTreasure(
            {
                "treasureId" : treasureId,
                "level" : 1,
            }
        );

        //设置主人
        var roleCur = this._role;
        this.treasures[treasureId].setOwner(roleCur);

        this.syncAndSaveTreasure(treasureId);
    }

    setBattleTreasure(battleTreasure)
    {
        this.battleTreasure = battleTreasure;

        this.syncAndSaveBattleTreasure();
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

        for (var i in this.treasures)
        {
            var treasure = this.treasures[i];
            //var flameCfg = flameConfig.getFlameConfig(flame.flameId);
            var levelCfg = treasureConfig.getTreasureLevelConfig(treasure.treasureId, treasure.level);
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
        }

        for (var j=0; j<this.battleTreasure.length; j++)
        {
            var treasure = this.treasures[this.battleTreasure[j]];
            //var flameCfg = flameConfig.getFlameConfig(flame.flameId);
            var levelCfg = treasureConfig.getTreasureLevelConfig(treasure.treasureId, treasure.level);
            if(!levelCfg)
            {
                continue;
            }

            this._partRates[enProp.power] = this._partRates[enProp.power] + levelCfg.powerRate;
        }
        //logUtil.info("神器 角色增加生命值:" + this._partValues[enPropFight.hpMax]);
        //logUtil.info("神器 角色增加战斗力:" + this._partValues[enProp.power]);
        //logUtil.info("神器 角色增加战斗力系数:" + this._partRates[enProp.power]);
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
        //logUtil.info("Fresh part:treasurePart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }
}

exports.TreasurePart = TreasurePart;