"use strict";

var logUtil = require("../../libs/logUtil");
var petFormationConfig = require("../gameConfig/petFormationConfig");
var roleConfig = require("../gameConfig/roleConfig");
var enPetPos = require("../enumType/globalDefine").enPetPos;
var enProp = require("../enumType/propDefine").enProp;

class PetFormation
{
    constructor(data)
    {

        this.formationId = data.formationId;
        this.formation = data.formation || ["","","","","",""];
        /**
         * 角色
         * @type {Role|null}
         */
        Object.defineProperty(this, "_owner", {enumerable: false, writable: true, value: null});

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
     * 当已在数据库时，存盘
     */
    syncAndSave()
    {
        if (!this._owner)
            return;

        var part = this._owner.getPetFormationsPart();
        if (!part)
            return;

        part.syncAndSavePetFormation(this.formationId);
    }

    release()
    {
        if (this._owner)
            logUtil.debug("PetFormation销毁，角色guid：" + this._owner.getGUID() + "，formationId：" + this.formationId);
        else
            logUtil.debug("PetFormation销毁，formationId：" + this.formationId);
    }


    /**
     *
     * @returns {Role[]}
     */
    getMainPets()
    {
        var roleList = [];
        var guid1 = this.formation[enPetPos.pet1Main];
        var guid2 = this.formation[enPetPos.pet2Main];
        if(guid1)
        {
            var pet1 = this._owner.getPetsPart().getPetByGUID(guid1);
            if(pet1)roleList.push(pet1);
        }
        if(guid2)
        {
            var pet2 = this._owner.getPetsPart().getPetByGUID(guid2);
            if(pet2)roleList.push(pet2);
        }

        return roleList;
    }

    isMainPet(guid)
    {
        return this.formation[enPetPos.pet1Main] == guid || this.formation[enPetPos.pet2Main] == guid;
    }
}

/**
 *
 * @param data
 * @returns {boolean}
 */
function isPetFormationData(data)
{
    return !!(data && data.formationId != null);
}

/**
 *
 * @param data
 * @returns {PetFormation|null}
 */
function createPetFormation(data)
{
    if (!isPetFormationData(data))
    {
        logUtil.error("PetFormation基本数据不完整或有错，数据：" + JSON.stringify(data));
        return null;
    }

    var petFormationCfg = petFormationConfig.getPetFormationConfig(data.formationId);
    if (!petFormationCfg)
    {
        logUtil.error("FormationId无效，formationId：" + data.formationId);
        return null;
    }

    return new PetFormation(data);
}

function getPetMainPosByPos(pos)
{
    if(pos === enPetPos.pet1Sub1 || pos === enPetPos.pet1Sub2 || pos === enPetPos.pet1Main)
    {
        return enPetPos.pet1Main;
    }

    if(pos === enPetPos.pet2Sub1 || pos === enPetPos.pet2Sub2 || pos === enPetPos.pet2Main)
    {
        return enPetPos.pet2Main;
    }

    return enPetPos.pet1Main;
}

//TODO　用于过渡的临时函数
function oldPosToNewPos(pos)
{
    switch(pos)
    {
        case enProp.pet1Main:
            return enPetPos.pet1Main;
        case enProp.pet1Sub1:
            return enPetPos.pet1Sub1;
        case enProp.pet1Sub2:
            return enPetPos.pet1Sub2;
        case enProp.pet2Main:
            return enPetPos.pet2Main;
        case enProp.pet2Sub1:
            return enPetPos.pet2Sub1;
        case enProp.pet2Sub2:
            return enPetPos.pet2Sub2;
    }
    return enPetPos.pet1Main;
}

function newPosToOldPos(pos)
{
    switch (pos)
    {
        case enPetPos.pet1Main:
            return enProp.pet1Main;
        case enPetPos.pet1Sub1:
            return enProp.pet1Sub1;
        case enPetPos.pet1Sub2:
            return enProp.pet1Sub2;
        case enPetPos.pet2Main:
            return enProp.pet2Main;
        case enPetPos.pet2Sub1:
            return enProp.pet2Sub1;
        case enPetPos.pet2Sub2:
            return enProp.pet2Sub2;
    }
    return enProp.pet1Main;
}


exports.isPetFormationData = isPetFormationData;
exports.createPetFormation = createPetFormation;
exports.oldPosToNewPos = oldPosToNewPos;
exports.newPosToOldPos = newPosToOldPos;
exports.getPetMainPosByPos = getPetMainPosByPos;