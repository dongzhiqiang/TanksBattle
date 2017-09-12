"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var petMessage = require("../netMessage/petMessage");
var CmdIdsPet = require("../netMessage/petMessage").CmdIdsPet;
var ResultCodePet = require("../netMessage/petMessage").ResultCodePet;
var enProp = require("../enumType/propDefine").enProp;
var enItemType = require("../enumType/globalDefine").enItemType;
var itemConfig = require("../gameConfig/itemConfig");
var petAdvLvPropRateConfig = require("../gameConfig/petAdvLvPropRateConfig");
var petAdvanceCostConfig = require("../gameConfig/petAdvanceCostConfig");
var petUpstarCostConfig = require("../gameConfig/petUpstarCostConfig");
var petPosConfig = require("../gameConfig/petPosConfig");
var roleConfig = require("../gameConfig/roleConfig");
var petSkillModule = require("../pet/petSkill");
var talentModule = require("../pet/talent");
var talentConfig = require("../gameConfig/talentConfig");
var talentPosConfig = require("../gameConfig/talentPosConfig");
var petTalentLvConfig = require("../gameConfig/petTalentLvConfig");
var globalDefine = require("../enumType/globalDefine").globalDefine;
var RoleSkillConfig = require("../gameConfig/roleSkillConfig");
var SkillLvCostConfig = require("../gameConfig/skillLvCostConfig");
var valueConfig = require("../gameConfig/valueConfig");
var petUpgradeCostConfig = require("../gameConfig/petUpgradeCostConfig");
var eventNames = require("../enumType/eventDefine").eventNames;
var dateUtil = require("../../libs/dateUtil");
var enPetPos = require("../enumType/globalDefine").enPetPos;
var enPetFormation = require("../enumType/globalDefine").enPetFormation;
var petFormationModule = require("../pet/petFormation");
/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpgradePetRequestVo} upgradeReq
 */
function upgradePet(session, role, msgObj, upgradeReq) {
    try {

        var guid = upgradeReq.guid;
        var itemId = upgradeReq.itemId;
        var num = upgradeReq.num;
        var pet = role.getPetsPart().getPetByGUID(guid);
        if (!pet) {
            msgObj.setResponseData(ResultCodePet.PET_NO_PET);
            role.send(msgObj);
            return;
        }
        if (pet.getNumber(enProp.level) >= globalDefine.MAX_ROLE_LEVEL) {
            msgObj.setResponseData(ResultCodePet.PET_MAX_LEVEL);
            role.send(msgObj);
            return;
        }
        if (pet.getNumber(enProp.level) >= role.getNumber(enProp.level)) {
            msgObj.setResponseData(ResultCodePet.PET_LEVEL_OVER_OWNER);
            role.send(msgObj);
            return;
        }
        var itemCfg = itemConfig.getItemConfig(itemId);
        if (itemCfg.type != enItemType.PET_EXP_ITEM) {
            msgObj.setResponseData(ResultCodePet.PET_WRONG_ITEM);
            role.send(msgObj);
            return;
        }
        var addExp = parseInt(itemCfg.useValue1);


        var level = pet.getNumber(enProp.level);
        var exp = pet.getNumber(enProp.exp);
        var roleCfg = roleConfig.getRoleConfig(pet.getString(enProp.roleId));
        var maxLevel = role.getNumber(enProp.level);
        var addNum = 0;

        while(level < maxLevel)
        {

            var needExp = petUpgradeCostConfig.getPetUpgradeCostConfig(roleCfg.upgradeCostId+"_"+level).exp;

            while(exp<needExp && addNum < num)
            {
                addNum++;
                exp += addExp;
            }
            exp -= needExp;
            level += 1;
        }

        var costItems = {};
        costItems[itemId] = addNum;
        if (!role.getItemsPart().canCostItems(costItems)) {
            msgObj.setResponseData(ResultCodePet.PET_NO_ENOUGH_ITEM);
            role.send(msgObj);
            return;
        }

        role.getItemsPart().costItems(costItems);

        var oldLevel = pet.getNumber(enProp.level);

        pet.getPropsPart().addExp(addExp*addNum);

        var result = new petMessage.UpdatePetResultVo();
        result.addExp = addExp*addNum;
        result.addLv = pet.getNumber(enProp.level) - oldLevel;

        //记录升级神侍时间，并更新每日神侍升级次数
        role.getTaskPart().addPetUpgradeNum(result.addLv);

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("petHandler~upgradePet", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.CMD_UPGRADE_PET, upgradePet, petMessage.UpgradePetRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {AdvancePetRequestVo} advanceReq
 */
function advancePet(session, role, msgObj, advanceReq) {
    try {

        var guid = advanceReq.guid;
        var pet = role.getPetsPart().getPetByGUID(guid);
        if(!pet)
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_PET);
            role.send(msgObj);
            return;
        }
        var roleCfg = roleConfig.getRoleConfig(pet.getString(enProp.roleId));
        if(pet.getNumber(enProp.advLv) >= roleCfg.maxAdvanceLevel)
        {
            msgObj.setResponseData(ResultCodePet.PET_MAX_ADV_LEVEL);
            role.send(msgObj);
            return;
        }
        var petAdvLvCfg = petAdvLvPropRateConfig.getPetAdvLvPropRateConfig(pet.getNumber(enProp.advLv));
        if(pet.getNumber(enProp.level) < petAdvLvCfg.needLv)
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_LEVEL);
            role.send(msgObj);
            return;
        }
        var costItems = petAdvanceCostConfig.getPetAdvanceCostConfig(roleCfg.advanceCostId+"_"+pet.getNumber(enProp.advLv)).cost;
        if(!role.getItemsPart().canCostItems(costItems))
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_ENOUGH_ITEM);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costItems(costItems);

        pet.addNumber(enProp.advLv, 1);
        pet.fireEvent(eventNames.ADV_LV_CHANGE);

        msgObj.setResponseData(ResultCode.SUCCESS);
        role.send(msgObj);

    }catch (err){
        logUtil.error("petHandler~advancePet", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.CMD_ADVANCE_PET, advancePet, petMessage.AdvancePetRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpstarPetRequestVo} upstarReq
 */
function upstarPet(session, role, msgObj, upstarReq) {
    try {

        var guid = upstarReq.guid;
        var pet = role.getPetsPart().getPetByGUID(guid);
        if(!pet)
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_PET);
            role.send(msgObj);
            return;
        }
        var roleCfg = roleConfig.getRoleConfig(pet.getString(enProp.roleId));
        if(pet.getNumber(enProp.star) >= roleCfg.maxStar)
        {
            msgObj.setResponseData(ResultCodePet.PET_MAX_STAR);
            role.send(msgObj);
            return;
        }
        var costItems = petUpstarCostConfig.getPetUpstarCostConfig(roleCfg.upstarCostId+"_"+pet.getNumber(enProp.star)).cost;
        if(!role.getItemsPart().canCostItems(costItems))
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_ENOUGH_ITEM);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costItems(costItems);

        pet.addNumber(enProp.star, 1);
        pet.fireEvent(eventNames.STAR_CHANGE);

        var result = new petMessage.UpstarPetResultVo();
        result.newStar = pet.getNumber(enProp.star);

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("petHandler~upstarPet", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.CMD_UPSTAR_PET, upstarPet, petMessage.UpstarPetRequestVo);



/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ChoosePetRequestVo} chooseReq
 */
function choosePet(session, role, msgObj, chooseReq) {
    try {
        var guid = chooseReq.guid;
        // !!这里的petPos 已经改成 enPetPos
        var petPos = chooseReq.petPos;
        var petFormationId = chooseReq.petFormation;
        var pet = role.getPetsPart().getPetByGUID(guid);
        if(!pet)
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_PET);
            role.send(msgObj);
            return;
        }
        if(petPos<enPetPos.pet1Main||petPos>enPetPos.pet2Sub2)
        {
            msgObj.setResponseData(ResultCodePet.PET_WRONG_POS);
            role.send(msgObj);
            return;
        }
        var posCfg = petPosConfig.getPetPosConfig(petPos);
        if(role.getNumber(enProp.level)<posCfg.level)
        {
            msgObj.setResponseData(ResultCodePet.PET_POS_NEED_LEVEL);
            role.send(msgObj);
            return;
        }
        var petFormation = role.getPetFormationsPart().getPetFormation(petFormationId);
        role.startBatch();
        var mainPos;
        var changedMap = {};
        for(var pos=enPetPos.pet1Main; pos <=enPetPos.pet2Sub2; pos++)
        {
            if(petFormation.formation[pos] === guid)
            {
                if(pos === petPos)
                {
                    // nothing changed
                    msgObj.setResponseData(ResultCode.SUCCESS);
                    role.send(msgObj);
                    return;
                }
                if(pos === enPetPos.pet1Main || pos === enPetPos.pet2Main)
                {
                    changedMap[petFormation.formation[pos]] = 1;

                    if(petFormationId == enPetFormation.normal) {
                        if (pos === enPetPos.pet1Main)
                            role.setString(enProp.pet1MRId, "");
                        else if (pos === enPetPos.pet2Main)
                            role.setString(enProp.pet2MRId, "");
                    }
                }
                else
                {
                    mainPos = petFormationModule.getPetMainPosByPos(pos);
                    if(petFormation.formation[mainPos] != "")
                    {
                        changedMap[petFormation.formation[mainPos]] = 1;
                    }
                }
                petFormation.formation[pos] = "";
                if(petFormationId == enPetFormation.normal) {
                    role.setString(petFormationModule.newPosToOldPos(pos), "");
                }
            }
        }
        if(petFormation.formation[petPos] != "")
        {
            if(petPos === enPetPos.pet1Main || petPos === enPetPos.pet2Main)
            {
                changedMap[petFormation.formation[petPos]] = 1;
            }
            else
            {
                mainPos = petFormationModule.getPetMainPosByPos(petPos);
                if(petFormation.formation[mainPos] != "")
                {
                    changedMap[petFormation.formation[mainPos]] = 1;
                }
            }
        }
        petFormation.formation[petPos] = guid;
        if(petFormationId == enPetFormation.normal) {
            role.setString(petFormationModule.newPosToOldPos(petPos), guid);
        }
        if(petPos === enPetPos.pet1Main || petPos === enPetPos.pet2Main)
        {
            if(petFormationId == enPetFormation.normal) {
                if (petPos === enPetPos.pet1Main)
                    role.setString(enProp.pet1MRId, pet.getString(enProp.roleId));
                else if (petPos === enPetPos.pet2Main)
                    role.setString(enProp.pet2MRId, pet.getString(enProp.roleId));
            }

            changedMap[guid] = 1;
        }
        role.endBatch();
        petFormation.syncAndSave();
        var changeList = [];
        if(petFormationId == enPetFormation.normal) { //不是默认阵型不刷新属性
            for (var k in changedMap) {
                changeList.push(k);
                var mainPet = role.getPetsPart().getPetByGUID(k);
                if (mainPet)mainPet.fireEvent(eventNames.PET_MAIN_CHANGE);
            }
        }
        var result = new petMessage.ChoosePetResultVo();
        result.needUpdatePets = changeList;
        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("petHandler~choosePet", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.CMD_CHOOSE_PET, choosePet, petMessage.ChoosePetRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpgradePetSkillRequestVo} upgradeReq
 */
function upgradePetSkill(session, role, msgObj, upgradeReq) {
    try {

        var guid = upgradeReq.guid;
        var skillId = upgradeReq.skillId;
        var pet = role.getPetsPart().getPetByGUID(guid);
        if(!pet)
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_PET);
            role.send(msgObj);
            return;
        }

        var roleId =pet.getString(enProp.roleId);
        if(!petSkillModule.hasPetSkills(roleId, skillId))
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_SKILL);
            role.send(msgObj);
            return;
        }
        var skillCfg = RoleSkillConfig.getRoleSkillConfig(roleId,skillId);
        if(skillCfg.needPetStar>pet.getNumber(enProp.star))
        {
            msgObj.setResponseData(ResultCodePet.PET_SKILL_NEED_STAR);
            role.send(msgObj);
            return;
        }
        var isAdd = false;
        var petSkill = pet.getPetSkillsPart().getPetSkillBySkillId(skillId);
        var level;
        if(petSkill==null)
        {
            isAdd = true;
            level = 1;
        }
        else
        {
            level = petSkill.level;
        }

        var maxSkillLevel = parseInt(valueConfig.getConfigValueConfig("maxSkillLevel")["value"]);
        if(level>=maxSkillLevel)
        {
            msgObj.setResponseData(ResultCodePet.PET_MAX_SKILL);
            role.send(msgObj);
            return;
        }
        if(level>=pet.getNumber(enProp.level))
        {
            msgObj.setResponseData(ResultCodePet.PET_SKILL_OVER_PET_LV);
            role.send(msgObj);
            return;
        }
        var costItems = SkillLvCostConfig.getSkillLvCostConfig(skillCfg.levelCostId+level).upgradeCost;
        if(!role.getItemsPart().canCostItems(costItems))
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_ENOUGH_ITEM);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costItems(costItems);

        level = level+1;
        if(isAdd)
        {
            pet.getPetSkillsPart().addPetSkillWithData({roleId:roleId,skillId:skillId,level:level});
        }
        else
        {
            petSkill.level = level;
            petSkill.syncAndSave();
        }

        pet.fireEvent(eventNames.PET_SKILL_CHANGE);

        var result = new petMessage.UpgradePetSkillResultVo();
        result.skillId = skillId;

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("petHandler~upgradePetSkill", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.CMD_UPGRADE_PET_SKILL, upgradePetSkill, petMessage.UpgradePetSkillRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpgradePetTalentRequestVo} upgradeReq
 */
function upgradePetTalent(session, role, msgObj, upgradeReq) {
    try {

        var guid = upgradeReq.guid;
        var talentId = upgradeReq.talentId;
        var pet = role.getPetsPart().getPetByGUID(guid);
        if(!pet)
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_PET);
            role.send(msgObj);
            return;
        }
        var outPos = {};
        if(!talentModule.hasTalent(pet.getString(enProp.roleId), talentId, outPos))
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_TALENT);
            role.send(msgObj);
            return;
        }
        var talentCfg = talentConfig.getTalentConfig(talentId);
        var talentPosCfg = talentPosConfig.getTalentPosConfig(outPos.pos);
        if(talentPosCfg.needAdvLv>pet.getNumber(enProp.advLv))
        {
            msgObj.setResponseData(ResultCodePet.PET_TALENT_NEED_ADV_LV);
            role.send(msgObj);
            return;
        }
        var isAdd = false;
        var talent = pet.getTalentsPart().getTalentByTalentId(talentId);
        var level;
        if(talent==null)
        {
            isAdd = true;
            level = 1;
        }
        else
        {
            level = talent.level;
        }
        if(level>=talentCfg.maxLevel)
        {
            msgObj.setResponseData(ResultCodePet.PET_MAX_TALENT);
            role.send(msgObj);
            return;
        }
        var advLvCfg = petAdvLvPropRateConfig.getPetAdvLvPropRateConfig(pet.getNumber(enProp.advLv));
        if(level>=advLvCfg.maxTalentLv)
        {
            msgObj.setResponseData(ResultCodePet.PET_TALENT_OVER_PET_ADV_LV);
            role.send(msgObj);
            return;
        }
        var costItems = petTalentLvConfig.getPetTalentLvConfig(talentCfg.upgradeId+level-1).upgradeCost;
        if(!role.getItemsPart().canCostItems(costItems))
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_ENOUGH_ITEM);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costItems(costItems);

        level = level+1;
        if(isAdd)
        {
            pet.getTalentsPart().addTalentWithData({talentId:talentId,level:level});
        }
        else
        {
            talent.level = level;
            talent.syncAndSave();
        }

        pet.fireEvent(eventNames.PET_TALENT_CHANGE);
        role.getPropsPart().updateFullPower();

        var result = new petMessage.UpgradePetTalentResultVo();
        result.talentId = talentId;

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("petHandler~upgradePetTalent", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.CMD_UPGRADE_PET_TALENT, upgradePetTalent, petMessage.UpgradePetTalentRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UnchoosePetRequestVo} unchooseReq
 */
function unchoosePet(session, role, msgObj, unchooseReq) {
    try {
        // !!这里的petPos 已经改成 enPetPos
        var petPos = unchooseReq.petPos;
        var petFormationId = unchooseReq.petFormation;

        if(petPos<enPetPos.pet1Main||petPos>enPetPos.pet2Sub2)
        {
            msgObj.setResponseData(ResultCodePet.PET_WRONG_POS);
            role.send(msgObj);
            return;
        }

        var petFormation = role.getPetFormationsPart().getPetFormation(petFormationId);
        var mainPos;
        var changedMap = {};
        if(petFormation.formation[petPos] != "")
        {
            if(petPos == enPetPos.pet1Main || petPos == enPetPos.pet2Main)
            {
                changedMap[petFormation.formation[petPos]] = 1;
            }
            else
            {
                mainPos = petFormationModule.getPetMainPosByPos(petPos);
                if(petFormation.formation[mainPos] != "")
                {
                    changedMap[petFormation.formation[mainPos]] = 1;
                }
            }
        }
        if(petFormationId == enPetFormation.normal)
        {
            role.setString(petFormationModule.newPosToOldPos(petPos), "");
            if (petPos === enPetPos.pet1Main)
                role.setString(enProp.pet1MRId, "");
            else if (petPos === enPetPos.pet2Main)
                role.setString(enProp.pet2MRId, "");
        }
        petFormation.formation[petPos] = "";
        petFormation.syncAndSave();

        var changeList = [];
        if(petFormationId == enPetFormation.normal)
        {
            for(var k in changedMap)
            {
                changeList.push(k);
                var mainPet = role.getPetsPart().getPetByGUID(k);
                if(mainPet)mainPet.fireEvent(eventNames.PET_MAIN_CHANGE);
            }
            role.getPropsPart().updateFullPower();
        }


        var result = new petMessage.ChoosePetResultVo();
        result.needUpdatePets = changeList;
        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("petHandler~unchoosePet", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.CMD_UNCHOOSE_PET, unchoosePet, petMessage.UnchoosePetRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {RecruitPetRequestVo} recruitReq
 */
function recruitPet(session, role, msgObj, recruitReq) {
    try {

        var roleId = recruitReq.roleId;
        var roleCfg = roleConfig.getRoleConfig(roleId);
        if(!roleCfg || roleCfg.roleType!=2)
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_PET);
            role.send(msgObj);
            return;
        }

        // 检查是否存在此宠物
        if(role.getPetsPart().getPetByRoleId(roleId))
        {
            msgObj.setResponseData(ResultCodePet.PET_EXISTS);
            role.send(msgObj);
            return;
        }

        if(!role.getItemsPart().canCostItem(roleCfg.pieceItemId, roleCfg.pieceNum))
        {
            msgObj.setResponseData(ResultCodePet.PET_NO_ENOUGH_ITEM);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costItem(roleCfg.pieceItemId, roleCfg.pieceNum);

        role.getPetsPart().addPet(roleId);

        var pet = role.getPetsPart().getPetByRoleId(roleId);
        var result = new petMessage.RecruitPetResultVo();
        result.guid = pet.getNumber(enProp.guid);

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("petHandler~recruitPet", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_PET, CmdIdsPet.CMD_RECRUIT_PET, recruitPet, petMessage.RecruitPetRequestVo);