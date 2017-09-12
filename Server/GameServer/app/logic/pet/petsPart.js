"use strict";

var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var enProp = require("../enumType/propDefine").enProp;
var roleMgr = require("../role/roleMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var petMessage = require("../netMessage/petMessage");
var CmdIdsPet = require("../netMessage/petMessage").CmdIdsPet;
var roleConfig = require("../gameConfig/roleConfig");
var guidGenerator = require("../../libs/guidGenerator");
var equipUtil = require("../equip/equipUtil");
var petSkillModule = require("../pet/petSkill");
var talentModule = require("../pet/talent");
var eventNames = require("../enumType/eventDefine").eventNames;
var rankMgr = require("../rank/rankMgr");
var enPetFormation = require("../enumType/globalDefine").enPetFormation;
var enPetPos = require("../enumType/globalDefine").enPetPos;

class PetsPart
{
    constructor(ownerRole, data)
    {
        /**
         *
         * @type {Role[]}
         * @private
         */
        this._pets = [];

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});
        Object.defineProperty(this, "_guidPetMap", {enumerable: false, writable:true, value: {}});

        try {
            var pets = data.pets || [];
            for (var i = 0; i < pets.length; ++i) {
                var rolePet = roleMgr.createRole(pets[i]);
                if (!rolePet) {
                    //抛出错误
                    throw new Error("创建宠物角色失败，角色数据：" + JSON.stringify(pets[i]));
                }

                //设置为宠物类型
                rolePet.markAsPetType();
                //设置主人
                rolePet.setOwner(ownerRole);
                this._pets.push(rolePet);
                var guid = rolePet.getNumber(enProp.guid);
                this._guidPetMap[guid] = rolePet;

                //临时，等数据正常后删除
                /*
                if(!pets[i].petSkills)
                {
                    rolePet.getPetSkillsPart().saveInitPetSkills();
                }
                if(!pets[i].talents)
                {
                    rolePet.getTalentsPart().saveInitTalents();
                }*/

            }



            //添加事件处理
            var thisObj = this;
            ownerRole.addListener(function(eventName, context, notifier) {
                thisObj.onOwnerUpgrade();
            }, eventNames.LEVEL_UP);
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    freshPetProp()
    {
        //对所有宠物计算基础属性
        //this._role._pets = this;
        var mainPets = [];
        var petFormation = this._role.getPetFormationsPart().getPetFormation(enPetFormation.normal);
        for(var i=0; i<this._pets.length; i++)
        {
            var pet = this._pets[i];
            if(pet.getString(enProp.guid)==petFormation.formation[enPetPos.pet1Main] || pet.getString(enProp.guid)==petFormation.formation[enPetPos.pet2Main])
            {
                mainPets.push(pet);
                continue;
            }
            pet.getPropsPart().freshBaseProp();
        }
        //对主战位宠物计算出战属性
        for(i=0; i<mainPets.length; i++)
        {
            mainPets[i].getPropsPart().freshBaseProp();
        }
    }

    release()
    {
        var pets = this._pets;
        for (var i = 0; i < pets.length; ++i)
        {
            pets[i].release();
        }
        this._pets = [];
        this._guidPetMap = {};
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        var data = [];
        var pets = this._pets;
        for (var i = 0; i < pets.length; ++i)
        {
            var rolePet = pets[i];
            data.push(rolePet.getDBData());
        }
        rootObj.pets = data;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        var data = [];
        var pets = this._pets;
        for (var i = 0; i < pets.length; ++i)
        {
            var rolePet = pets[i];
            data.push(rolePet.getPrivateNetData());
        }
        rootObj.pets = data;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        var data = [];
        var pets = this._pets;
        for (var i = 0; i < pets.length; ++i)
        {
            var rolePet = pets[i];
            data.push(rolePet.getPublicNetData());
        }
        rootObj.pets = data;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        var data = [];
        var pets = this._pets;
        for (var i = 0; i < pets.length; ++i)
        {
            var rolePet = pets[i];
            data.push(rolePet.getProtectNetData());
        }
        rootObj.pets = data;
    }

    /**
     *
     * @returns {Number}
     */
    getPetCount()
    {
        return this._pets.length;
    }

    /**
     *
     * @param {number} index
     * @returns {Role}
     */
    getPetByIndex(index)
    {
        return this._pets[index];
    }

    /**
     *
     * @param {string} guid
     * @returns {Role}
     */
    getPetByGUID(guid)
    {
        return this._guidPetMap[guid];
    }

    /**
     *
     * @param {string} roleId
     * @returns {Role}
     */
    getPetByRoleId(roleId)
    {
        for (var i = 0; i < this._pets.length; ++i)
        {
            var rolePet = this._pets[i];
            if(rolePet.getString(enProp.roleId) == roleId)
            {
                return rolePet;
            }
        }
        return null;
    }

    /**
     * 根据角色类型添加宠物
     * @param {string} roleId - 角色类型ID
     * @param {number?} star
     */
    addPet(roleId, star)
    {
        var roleCfg = roleConfig.getRoleConfig(roleId);
        if (!roleCfg)
        {
            logUtil.error("找不到角色类型：" + roleId);
            return false;
        }

        var petData = {
            props: {
                    guid:guidGenerator.generateGUID(),
                    createTime:dateUtil.getTimestamp(),
                    roleId:roleId,
                    name:roleCfg.name,
                    level:1,
                    exp:0,
                    star:star == undefined ? roleCfg.initStar : star,
                    advLv:1
            },
            equips: equipUtil.getInitEquips(roleId),
            petSkills: petSkillModule.getInitPetSkills(roleId),
            talents: talentModule.getInitTalents(roleId)
        };

        return this.addPetWithData(petData);
    }

    /**
     *
     * @param guid
     * @param {boolean} noRelease - 有可能是要被转移到别的地方，这时就不要release
     * @return {boolean} 如果宠物存在且删除成功就返回true
     */
    removePetByGUID(guid, noRelease)
    {
        var rolePet = this._guidPetMap[guid];
        //不存在？不能删除
        if (!rolePet)
            return false;

        //先删除
        delete this._guidPetMap[guid];
        this._pets.removeValue(rolePet);

        //设置为未知类型
        rolePet.markAsUnknownType();
        //设置主人为空
        rolePet.setOwner(null);

        //如果要释放，那就释放
        if (!noRelease)
        {
            try {
                rolePet.release();
            }
            catch (err) {
                logUtil.error("宠物角色销毁出错", err);
            }
        }

        var ownerRole = this._role;
        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow({"props.heroId": heroId}, {$pull: {"pets": {"props.guid": guid}}});

            //通知客户端
            var netMsg = new petMessage.RemovePetRoleVo(guid);
            ownerRole.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_REMOVE_PET, netMsg);
        }

        //从排行删除
        rankMgr.removeFromAllRankByRole(rolePet);

        return true;
    }

    /**
     *
     * @param {object} data
     * @return {boolean} 如果添加成功就返回true
     */
    addPetWithData(data)
    {
        //必须有最基本数据
        if (!roleMgr.isRoleData(data))
            return false;

        //不能有heroId，也就是主角不充当宠物
        if (Object.isNumber(data.props.heroId) && data.props.heroId !== 0)
        {
            logUtil.warn("addPetWithData，主角不能当宠物");
            return false;
        }

        var guid = data.props.guid;
        //已存在？不能添加
        if (this._guidPetMap[guid])
            return false;

        var rolePet = roleMgr.createRole(data);
        if (!rolePet)
        {
            logUtil.error("创建角色失败，角色数据：" + JSON.stringify(data));
            return false;
        }

        //设置为宠物类型
        rolePet.markAsPetType();
        //设置主人
        var ownerRole = this._role;
        rolePet.setOwner(ownerRole);

        //添加到列表
        this._pets.push(rolePet);
        this._guidPetMap[guid] = rolePet;

        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow({"props.heroId":heroId}, {$push:{"pets":data}});

            //通知客户端
            ownerRole.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_PET, data);
        }

        //计算属性
        rolePet.getPropsPart().freshBaseProp();

        //通知其它神侍
        for(var i=0; i<this._pets.length; i++)
        {
            var pet = this._pets[i];
            if(pet==rolePet)continue;
            pet.fireEvent(eventNames.PET_NEW);
        }

        //加入排行
        rankMgr.addToAllRankByRole(rolePet);

        return true;
    }

    /**
     *
     * @param {Role} rolePet
     * @return {boolean} 如果添加成功就返回true
     */
    addPetWithRole(rolePet)
    {
        //主角不能当宠物
        if (rolePet.isHero())
        {
            logUtil.warn("addPetWithRole，主角不能当宠物");
            return false;
        }

        //不能有主人
        if (rolePet.getOwner())
        {
            logUtil.warn("addPetWithRole，有主人的角色必须脱离主人才能当别人的宠物");
            return false;
        }

        var guid = rolePet.getNumber(enProp.guid);
        //已存在？不能添加
        if (this._guidPetMap[guid])
            return false;

        //设置为宠物类型
        rolePet.markAsPetType();
        //设置主人
        var ownerRole = this._role;
        rolePet.setOwner(ownerRole);

        //添加到列表
        this._pets.push(rolePet);
        this._guidPetMap[guid] = rolePet;

        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            var dbData = rolePet.getDBData();
            col.updateOneNoThrow({"props.heroId":heroId}, {$push:{"pets":dbData}});

            //通知客户端
            var netMsg = rolePet.getPrivateNetData();
            ownerRole.sendEx(ModuleIds.MODULE_PET, CmdIdsPet.PUSH_ADD_PET, netMsg);
        }

        //计算属性
        rolePet.getPropsPart().freshBaseProp();

        //通知其它神侍
        for(var i=0; i<this._pets.length; i++)
        {
            var pet = this._pets[i];
            if(pet==rolePet)continue;
            pet.fireEvent(eventNames.PET_NEW);
        }

        //加入排行
        rankMgr.addToAllRankByRole(rolePet);

        return true;
    }

    onOwnerUpgrade()
    {
        for (var i = 0; i < this._pets.length; ++i)
        {
            var rolePet = this._pets[i];
            rolePet.addExp(0);
        }
    }

    hasPet(roleId, starObj)
    {
        starObj.star = 0;

        for(var i=0; i < this._pets.length; ++i)
        {
            var pet = this._pets[i];
            if(pet.getString(enProp.roleId) == roleId)
            {
                starObj.star = pet.getNumber(enProp.star);
                return true;
            }
        }
        return false;
    }

    /**
     *
     * @returns {Role[]}
     */
    getMainPets(petFormationId)
    {
        if(petFormationId == null)petFormationId = enPetFormation.normal;
        var petFormation = this._role.getPetFormationsPart().getPetFormation(petFormationId);
        return petFormation.getMainPets();
    }

    /** 注意，这个函数只判断主出战阵营!*/
    isMainPet(guid)
    {
        var petFormation = this._role.getPetFormationsPart().getPetFormation(enPetFormation.normal);
        return petFormation.isMainPet(guid);
    }
}

exports.PetsPart = PetsPart;