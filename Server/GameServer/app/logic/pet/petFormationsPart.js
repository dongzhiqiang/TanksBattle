"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var petFormationModule = require("../pet/petFormation");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var petMessage = require("../netMessage/petMessage");
var CmdIdsPet = require("../netMessage/petMessage").CmdIdsPet;
var petFormationConfig = require("../gameConfig/petFormationConfig");
var propertyTable = require("../gameConfig/propertyTable");
var enProp = require("../enumType/propDefine").enProp;
var enPropFight = require("../enumType/propDefine").enPropFight;
var propValueConfig = require("../gameConfig/propValueConfig");
var eventNames = require("../enumType/eventDefine").eventNames;

class PetFormationsPart
{
    constructor(ownerRole, data)
    {
        /**
         * @type {PetFormation[]}
         * @private
         */
        this._petFormations = [];

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});
        Object.defineProperty(this, "_formationIdPetFormationMap", {enumerable: false, writable:true, value: {}});

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
            var petFormations = data.petFormations || [];
            for (var i = 0; i < petFormations.length; ++i) {
                var petFormationData = petFormations[i];
                var petFormation = petFormationModule.createPetFormation(petFormationData);
                if (!petFormation) {
                    throw new Error("创建PetFormation失败");
                }

                //设置主人
                petFormation.setOwner(ownerRole);
                //加入列表
                this._petFormations.push(petFormation);
                this._formationIdPetFormationMap[petFormation.formationId] = petFormation;
            }

            //添加事件处理
            var thisObj = this;
            ownerRole.addListener(function(eventName, context, notifier) {
                thisObj.onPartFresh();
            }, eventNames.PET_MAIN_CHANGE);
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        var petFormations = this._petFormations;
        for (var i = 0; i < petFormations.length; ++i)
        {
            petFormations[i].release();
        }
        this._petFormations = [];
        this._formationIdPetFormationMap = {};
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.petFormations = this._petFormations;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.petFormations = this._petFormations;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        rootObj.petFormations = this._petFormations;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        rootObj.petFormations = this._petFormations;
    }

    /**
     *
     * @param {PetFormation} petFormation
     * @private
     */
    _addPetFormation(petFormation)
    {
        var formationId = petFormation.formationId;
        this._petFormations.push(petFormation);
        this._formationIdPetFormationMap[formationId] = petFormation;
    }

    /**
     *
     * @param {PetFormation|number} val
     * @private
     */
    _removePetFormation(val)
    {
        var formationId = Object.isNumber(val) ? val : val.formationId;
        this._petFormations.removeValue(this._formationIdPetFormationMap[formationId]);
        delete this._formationIdPetFormationMap[formationId];
    }

    /**
     *
     * @param {number} formationId
     * @returns {PetFormation|undefined}
     */
    _getPetFormationByFormationId(formationId)
    {
        return this._formationIdPetFormationMap[formationId];
    }

    /**
     *
     * @param {number} formationId
     * @returns {PetFormation|undefined}
     */
    getPetFormation(formationId)
    {
        var petFormation = this._getPetFormationByFormationId(formationId);
        if(!petFormation)
        {
            this.addPetFormationWithData({formationId:formationId});
            petFormation = this._getPetFormationByFormationId(formationId);
        }
        return petFormation;
    }

    /**
     *
     * @param {object} data
     * @return {boolean} 如果添加成功就返回true
     */
    addPetFormationWithData(data)
    {
        //必须有最基本数据
        //因为下面要获取formationId，所以要先判断
        if (!petFormationModule.isPetFormationData(data))
            return false;

        var formationId = data.formationId;
        //已存在？不能添加
        if (this._getPetFormationByFormationId(formationId))
            return false;

        var petFormation = petFormationModule.createPetFormation(data);
        if (!petFormation)
            return false;

        //设置主人
        var roleCur = this._role;
        petFormation.setOwner(roleCur);

        //添加到列表
        this._addPetFormation(petFormation);

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
        updateObj = {$push:{"petFormations":petFormation}};

        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new petMessage.AddOrUpdatePetFormationVo(true, petFormation);
        roleTop.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_OR_UPDATE_PET_FORMATION, netMsg);
        return true;
    }


    //用于保存已在数据库的数据
    syncAndSavePetFormation(formationId)
    {
        var petFormation = this._formationIdPetFormationMap[formationId];
        //不存在？不能继续
        if (!petFormation)
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


        queryObj = {"props.heroId":heroId, "petFormations":{$elemMatch:{"formationId":formationId}}};
        updateObj =  {$set: {"petFormations.$": petFormation}};
        col.updateOneNoThrow(queryObj, updateObj);


        //通知客户端
        var netMsg = new petMessage.AddOrUpdatePetFormationVo(false, petFormation);
        roleTop.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_OR_UPDATE_PET_FORMATION, netMsg);
        return true;
    }

    /**
     * 计算部件的属性
     */
    freshPartProp()
    {

    }

    /**
     * 累加部件的属性
     */
    onFreshBaseProp(values, rates)
    {

    }

    onPartFresh()
    {
        //logUtil.info("Fresh part:petFormationsPart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }
}

exports.PetFormationsPart = PetFormationsPart;