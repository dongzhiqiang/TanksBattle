"use strict";

var logUtil = require("../../libs/logUtil");
var propertyTable = require("../gameConfig/propertyTable");
var propValueConfig = require("../gameConfig/propValueConfig");
var roleConfig = require("../gameConfig/roleConfig");
var petBondConfig = require("../gameConfig/petBondConfig");
var enProp = require("../enumType/propDefine").enProp;
var enPropFight = require("../enumType/propDefine").enPropFight;
var eventNames = require("../enumType/eventDefine").eventNames;

class PetBondPart
{
    constructor(ownerRole, data)
    {
        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});

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

        //添加事件处理
        var thisObj = this;
        ownerRole.addListener(function(eventName, context, notifier) {
            thisObj.onPartFresh();
        }, eventNames.PET_NEW);
    }

    release()
    {
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

        var roleCfg = roleConfig.getRoleConfig(this._role.getString(enProp.roleId));

        for (var i = 0; i < roleCfg.petBonds.length; i++)
        {
            var petBondCfg = petBondConfig.getPetBondConfig(roleCfg.petBonds[i]);
            var bondPets = petBondCfg.pets;
            if (bondPets.length == 0)
            {
                continue;
            }
            var minStar = 5;
            var hasBond = true;
            for (var j = 0; j < bondPets.length; j++)
            {
                var starObj={};
                if (this._role.getOwner().getPetsPart().hasPet(bondPets[j],starObj))
                {
                    if(minStar > starObj.star)
                    {
                        minStar = starObj.star;
                    }
                }
            else
                {
                    hasBond = false;
                    break;
                }

            }
            if (!hasBond)
            {
                continue;
            }
            var rateCxts = petBondCfg.getRateCxts();
            for(j=0; j<rateCxts.length; j++)
            {
                var cxt = rateCxts[j];
                var v = cxt.value.getByLv(minStar);

                if (!cxt.value.isPercent)
                {
                    this._partValues[cxt.prop] = this._partValues[cxt.prop] + v;
                }
                else
                {
                    this._partRates[cxt.prop] = this._partRates[cxt.prop] + v;
                }
            }

            this._partRates[enProp.power] = this._partRates[enProp.power] + petBondCfg.getPowerRateLvValue().getByLv(minStar);
        }

        //logUtil.info("宠物羁绊 角色增加生命值:" + this._partValues[enPropFight.hpMax]);
        //logUtil.info("宠物羁绊 角色增加生命值系数:" + this._partRates[enPropFight.hpMax]);
        //logUtil.info("宠物羁绊 角色增加战斗力系数:" + this._partRates[enProp.power]);
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
        //logUtil.info("Fresh part:petBondPart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }
}

exports.PetBondPart = PetBondPart;