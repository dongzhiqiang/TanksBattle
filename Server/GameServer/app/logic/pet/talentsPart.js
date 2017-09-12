"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var talentModule = require("../pet/talent");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var petMessage = require("../netMessage/petMessage");
var CmdIdsPet = require("../netMessage/petMessage").CmdIdsPet;
var propertyTable = require("../gameConfig/propertyTable");
var roleConfig = require("../gameConfig/roleConfig");
var talentConfig = require("../gameConfig/talentConfig");
var talentPosConfig = require("../gameConfig/talentPosConfig");
var enProp = require("../enumType/propDefine").enProp;
var eventNames = require("../enumType/eventDefine").eventNames;

class TalentsPart
{
    constructor(ownerRole, data)
    {
        /**
         * @type {Talent[]}
         * @private
         */
        this._talents = [];

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});
        Object.defineProperty(this, "_talentIdTalentMap", {enumerable: false, writable:true, value: {}});

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
            var talents = data.talents || [];
            for (var i = 0; i < talents.length; ++i) {
                var talentData = talents[i];
                var talent = talentModule.createTalent(talentData);
                if (!talent) {
                    throw new Error("创建Talent失败");
                }

                //设置主人
                talent.setOwner(ownerRole);
                //加入列表
                this._talents.push(talent);
                this._talentIdTalentMap[talent.talentId] = talent;
            }

            //添加事件处理
            var thisObj = this;
            ownerRole.addListener(function(eventName, context, notifier) {
                thisObj.onPartFresh();
            }, eventNames.PET_TALENT_CHANGE);
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        var talents = this._talents;
        for (var i = 0; i < talents.length; ++i)
        {
            talents[i].release();
        }
        this._talents = [];
        this._talentIdTalentMap = {};
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.talents = this._talents;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.talents = this._talents;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        rootObj.talents = this._talents;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        rootObj.talents = this._talents;
    }

    /**
     *
     * @param {Talent} talent
     * @private
     */
    _addTalent(talent)
    {
        var talentId = talent.talentId;
        this._talents.push(talent);
        this._talentIdTalentMap[talentId] = talent;
    }

    /**
     *
     * @param {Talent|string} val
     * @private
     */
    _removeTalent(val)
    {
        var talentId = Object.isString(val) ? val : val.talentId;
        this._talents.removeValue(this._talentIdTalentMap[talentId]);
        delete this._talentIdTalentMap[talentId];
    }

    /**
     *
     * @param {string} talentId
     * @returns {Talent|undefined}
     */
    getTalentByTalentId(talentId)
    {
        return this._talentIdTalentMap[talentId];
    }


    saveInitTalents() // 仅用于没有初始化天赋时
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
        updateObj = {$set:{["pets.$.talents"]:[]}};

        col.updateOneNoThrow(queryObj, updateObj);
        return true;
    }

    /**
     *
     * @param {object} data
     * @return {boolean} 如果添加成功就返回true
     */
    addTalentWithData(data)
    {
        //必须有最基本数据
        //因为下面要获取talentId，所以要先判断
        if (!talentModule.isTalentData(data))
            return false;

        var talentId = data.talentId;
        //已存在？不能添加
        if (this.getTalentByTalentId(talentId))
            return false;

        var talent = talentModule.createTalent(data);
        if (!talent)
            return false;

        //设置主人
        var roleCur = this._role;
        talent.setOwner(roleCur);

        //添加到列表
        this._addTalent(talent);

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
        updateObj = {$push:{"pets.$.talents":talent}};
        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new petMessage.AddOrUpdateTalentVo(guidCur, true, talent);
        roleTop.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_OR_UPDATE_TALENT, netMsg);
        return true;
    }


    //用于保存已在数据库的天赋
    syncAndSaveTalent(talentId)
    {
        var talent = this._talentIdTalentMap[talentId];
        //不存在？不能继续
        if (!talent)
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
        updateObj = {$pull:{"pets.$.talents":{talentId:talentId}}};
        col.updateOneNoThrow(queryObj, updateObj);
        queryObj = {"props.heroId":heroId, "pets.props.guid":guidCur};
        updateObj = {$push:{"pets.$.talents":talent}};
        col.updateOneNoThrow(queryObj, updateObj);


        //通知客户端
        var netMsg = new petMessage.AddOrUpdateTalentVo(guidCur, false, talent);
        roleTop.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_OR_UPDATE_TALENT, netMsg);
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

        var roleCfg = roleConfig.getRoleConfig(this._role.getString(enProp.roleId));

        for (var i=0; i<roleCfg.talents.length; i++)
        {
            var talentId = roleCfg.talents[i];
            var talentPosCfg = talentPosConfig.getTalentPosConfig(i);
            if( this._role.getNumber(enProp.advLv) < talentPosCfg.needAdvLv)
            {
                continue;
            }
            var level = 1;
            var talent = this.getTalentByTalentId(talentId);
            if(talent)
            {
                level = talent.level;
            }
            var talentCfg = talentConfig.getTalentConfig(talentId);
            this._partValues[enProp.power] = this._partValues[enProp.power] + talentCfg.getPowerLvValue().getByLv(level);
            this._partRates[enProp.power] = this._partRates[enProp.power] + talentCfg.getPowerRateLvValue().getByLv(level);
        }

        //logUtil.info("宠物天赋 角色增加战斗力:" + this._partValues[enProp.power]);
        //logUtil.info("宠物天赋 角色增加战斗力系数:" + this._partRates[enProp.power]);
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
        //logUtil.info("Fresh part:talentsPart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }
}

exports.TalentsPart = TalentsPart;