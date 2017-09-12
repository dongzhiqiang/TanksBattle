"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var petSkillModule = require("../pet/petSkill");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var petMessage = require("../netMessage/petMessage");
var CmdIdsPet = require("../netMessage/petMessage").CmdIdsPet;
var enProp = require("../enumType/propDefine").enProp;
var propertyTable = require("../gameConfig/propertyTable");
var propValueConfig = require("../gameConfig/propValueConfig");
var roleConfig = require("../gameConfig/roleConfig");
var roleSkillConfig = require("../gameConfig/roleSkillConfig");
var eventNames = require("../enumType/eventDefine").eventNames;

class PetSkillsPart
{
    constructor(ownerRole, data)
    {
        /**
         * @type {PetSkill[]}
         * @private
         */
        this._petSkills = [];

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});
        Object.defineProperty(this, "_skillIdPetSkillMap", {enumerable: false, writable:true, value: {}});

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
            var petSkills = data.petSkills || [];
            var roleId =ownerRole.getString(enProp.roleId);
            //如果技能表为空，初始化技能表
            for (var i = 0; i < petSkills.length; ++i) {
                var petSkillData = petSkills[i];
                petSkillData.roleId =roleId;
                var petSkill = petSkillModule.createPetSkill(petSkillData);
                if (!petSkill) {
                    throw new Error("创建PetSkill失败");
                }

                //设置主人
                petSkill.setOwner(ownerRole);
                //加入列表
                this._petSkills.push(petSkill);
                this._skillIdPetSkillMap[petSkill.skillId] = petSkill;
            }

            //添加事件处理
            var thisObj = this;
            ownerRole.addListener(function(eventName, context, notifier) {
                thisObj.onPartFresh();
            }, eventNames.PET_SKILL_CHANGE);
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        var petSkills = this._petSkills;
        for (var i = 0; i < petSkills.length; ++i)
        {
            petSkills[i].release();
        }
        this._petSkills = [];
        this._skillIdPetSkillMap = {};
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.petSkills = this._petSkills;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.petSkills = this._petSkills;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        rootObj.petSkills = this._petSkills;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        rootObj.petSkills = this._petSkills;
    }

    /**
     *
     * @param {PetSkill} petSkill
     * @private
     */
    _addPetSkill(petSkill)
    {
        var skillId = petSkill.skillId;
        this._petSkills.push(petSkill);
        this._skillIdPetSkillMap[skillId] = petSkill;
    }

    /**
     *
     * @param {PetSkill|string} val
     * @private
     */
    _removePetSkill(val)
    {
        var skillId = Object.isString(val) ? val : val.skillId;
        this._petSkills.removeValue(this._skillIdPetSkillMap[skillId]);
        delete this._skillIdPetSkillMap[skillId];
    }

    /**
     *
     * @param {string} skillId
     * @returns {PetSkill|undefined}
     */
    getPetSkillBySkillId(skillId)
    {
        return this._skillIdPetSkillMap[skillId];
    }


    saveInitPetSkills() // 仅用于创建的宠物没有初始化技能时
    {
        //检测条件
        var roleCur = this._role;
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;

        queryObj = {"props.heroId":heroId, "pets.props.guid":guidCur};
        updateObj = {$set:{["pets.$.petSkills"]:[]}};

        col.updateOneNoThrow(queryObj, updateObj);
        return true;
    }

    /**
     *
     * @param {object} data
     * @return {boolean} 如果添加成功就返回true
     */
    addPetSkillWithData(data)
    {
        //必须有最基本数据
        //因为下面要获取skillId，所以要先判断
        if (!petSkillModule.isPetSkillData(data))
            return false;

        var skillId = data.skillId;
        //已存在？不能添加
        if (this.getPetSkillBySkillId(skillId))
            return false;

        var petSkill = petSkillModule.createPetSkill(data);
        if (!petSkill)
            return false;

        //设置主人
        var roleCur = this._role;
        petSkill.setOwner(roleCur);

        //添加到列表
        this._addPetSkill(petSkill);

        //检测条件
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;
        queryObj = {"props.heroId":heroId, "pets.props.guid":guidCur};
        updateObj = {$push:{"pets.$.petSkills":petSkill}};
        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new petMessage.AddOrUpdatePetSkillVo(guidCur, true, petSkill);
        roleTop.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_OR_UPDATE_PET_SKILL, netMsg);
        return true;
    }


    //用于保存已在数据库的宠物技能
    syncAndSavePetSkill(skillId)
    {
        var petSkill = this._skillIdPetSkillMap[skillId];
        //不存在？不能继续
        if (!petSkill)
            return false;

        //检测条件
        var roleCur = this._role;
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;

        queryObj = {"props.heroId":heroId, "pets.props.guid":guidCur};
        updateObj = {$pull:{"pets.$.petSkills":{skillId:skillId}}};
        col.updateOneNoThrow(queryObj, updateObj);
        queryObj = {"props.heroId":heroId, "pets.props.guid":guidCur};
        updateObj = {$push:{"pets.$.petSkills":petSkill}};
        col.updateOneNoThrow(queryObj, updateObj);


        //通知客户端
        var netMsg = new petMessage.AddOrUpdatePetSkillVo(guidCur, false, petSkill);
        roleTop.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_OR_UPDATE_PET_SKILL, netMsg);
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

        var petSkills = [];

        var cfg = roleConfig.getRoleConfig(this._role.getString(enProp.roleId));
        petSkills.push(cfg.atkUpSkill);
        for (var i=0; i<cfg.skills.length; i++)
        {
            petSkills.push(cfg.skills[i]);
        }

        for (i=0; i<petSkills.length; i++)
        {
            var skillId = petSkills[i];
            var roleSkillCfg = roleSkillConfig.getRoleSkillConfig(this._role.getString(enProp.roleId), skillId);
            if( this._role.getNumber(enProp.star) < roleSkillCfg.needPetStar)
            {
                continue;
            }
            var level = 1;
            var skill = this.getPetSkillBySkillId(skillId);
            if(skill)
            {
                level = skill.level;
            }
            this._partRates[enProp.power] = this._partRates[enProp.power] + roleSkillCfg.getPowerRateLvValue().getByLv(level);
            //logUtil.info("lvl"+level);
            //logUtil.info("LvValue"+roleSkillCfg.getPowerRateLvValue().getByLv(level));
        }

        //logUtil.info("宠物技能 角色增加战斗力系数:" + this._partRates[enProp.power]);
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
        //logUtil.info("Fresh part:petSkillsPart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }
}

exports.PetSkillsPart = PetSkillsPart;