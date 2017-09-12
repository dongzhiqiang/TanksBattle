"use strict";

var logUtil = require("../../libs/logUtil");
var propertyTable = require("../gameConfig/propertyTable");
var propValueConfig = require("../gameConfig/propValueConfig");
var roleConfig = require("../gameConfig/roleConfig");
var petBattleAssistRateConfig = require("../gameConfig/petBattleAssistRateConfig");
var roleTypePropConfig = require("../gameConfig/roleTypePropConfig");
var enProp = require("../enumType/propDefine").enProp;
var enPropFight = require("../enumType/propDefine").enPropFight;
var eventNames = require("../enumType/eventDefine").eventNames;
var enPetFormation = require("../enumType/globalDefine").enPetFormation;
var enPetPos = require("../enumType/globalDefine").enPetPos;

class PetPosPart
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
        }, eventNames.PET_MAIN_CHANGE);
        ownerRole.addListener(function(eventName, context, notifier) {
            thisObj.onPropUpdated();
        }, eventNames.PROP_UPDATED);
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

        var idx;
        var owner = this._role.getOwner();
        var petFormation = owner.getPetFormationsPart().getPetFormation(enPetFormation.normal);
        if(petFormation.formation[enPetPos.pet1Main]==this._role.getString(enProp.guid))
        {
            idx = 1;
        }
        else if(petFormation.formation[enPetPos.pet2Main]==this._role.getString(enProp.guid))
        {
            idx = 2;
        }
        else
        {
            return;
        }

        var petPos1;
        var petPos2;
        var assistId1;
        var assistId2;
        if (idx==1)
        {
            petPos1 = enPetPos.pet1Sub1;
            petPos2 = enPetPos.pet1Sub2;
            assistId1 = 1;
            assistId2 = 2;
        }
        else
        {
            petPos1 = enPetPos.pet2Sub1;
            petPos2 = enPetPos.pet2Sub2;
            assistId1 = 3;
            assistId2 = 4;
        }
        var petGUID;
        var tempProps={};
        var tempProps1={};
        var pet;
        petGUID = petFormation.formation[petPos1];
        if (petGUID)
        {
            pet = owner.getPetsPart().getPetByGUID(petGUID);
            if (pet)
            {
                propertyTable.copy(pet.getPropsPart()._fightProps, tempProps);
                propertyTable.mul(tempProps, petBattleAssistRateConfig.getPetBattleAssistRateConfig(assistId1).props, tempProps);
                propertyTable.add(this._partValues, tempProps, this._partValues);
                propertyTable.mul(tempProps, roleTypePropConfig.getPowerProp(), tempProps1);
                propertyTable.mul(tempProps, roleTypePropConfig.getPowerRateProp(), tempProps);
                this._partValues[enProp.power] = this._partValues[enProp.power] + propertyTable.sum(tempProps1);
                this._partRates[enProp.power] = this._partRates[enProp.power] + propertyTable.sum(tempProps);
            }
        }
        petGUID = petFormation.formation[petPos2];
        if (petGUID)
        {
            pet = owner.getPetsPart().getPetByGUID(petGUID);
            if (pet)
            {
                propertyTable.copy(pet.getPropsPart()._fightProps, tempProps);
                propertyTable.mul(tempProps, petBattleAssistRateConfig.getPetBattleAssistRateConfig(assistId2).props, tempProps);
                propertyTable.add(this._partValues, tempProps, this._partValues);
                propertyTable.mul(tempProps, roleTypePropConfig.getPowerProp(), tempProps1);
                propertyTable.mul(tempProps, roleTypePropConfig.getPowerRateProp(), tempProps);
                this._partValues[enProp.power] = this._partValues[enProp.power] + propertyTable.sum(tempProps1);
                this._partRates[enProp.power] = this._partRates[enProp.power] + propertyTable.sum(tempProps);
            }
        }

        //logUtil.info("宠物出战 角色增加生命值:" + this._partValues[enPropFight.hpMax]);
        //logUtil.info("宠物出战 角色增加战斗力:" + this._partValues[enProp.power]);
        //logUtil.info("宠物出战 角色增加战斗力系数:" + this._partRates[enProp.power]);
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
        //logUtil.info("Fresh part:petPosPart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }

    onPropUpdated()
    {
        var owner = this._role.getOwner();
        var petFormation = owner.getPetFormationsPart().getPetFormation(enPetFormation.normal);
        var guid = this._role.getString(enProp.guid);
        var mainGUID = null;
        if(petFormation.formation[enPetPos.pet1Sub1]==guid || petFormation.formation[enPetPos.pet1Sub2]==guid)
        {
            mainGUID = petFormation.formation[enPetPos.pet1Main];
        }
        if(petFormation.formation[enPetPos.pet2Sub1]==guid || petFormation.formation[enPetPos.pet2Sub2]==guid)
        {
            mainGUID = petFormation.formation[enPetPos.pet2Main];
        }

        if(mainGUID)
        {
            var mainPet = owner.getPetsPart().getPetByGUID(mainGUID);
            if(mainPet)
            {
                mainPet.fireEvent(eventNames.PET_MAIN_CHANGE);
            }
        }
    }
}

exports.PetPosPart = PetPosPart;