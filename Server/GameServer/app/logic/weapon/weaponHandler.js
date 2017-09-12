/**
 * 名称：weaponHandle
 * 日期：2016.4.7
 * 描述：
 */
"use strict";

var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var CmdIdsWeapon = require("../netMessage/weaponMessage").CmdIdsWeapon;
var ResultCodeWeapon = require("../netMessage/weaponMessage").ResultCodeWeapon;
var WeaponSkillLevelUpReq = require("../netMessage/weaponMessage").WeaponSkillLevelUpReq;
var WeaponSkillLevelUpRes = require("../netMessage/weaponMessage").WeaponSkillLevelUpRes;
var WeaponChangeReq = require("../netMessage/weaponMessage").WeaponChangeReq;
var WeaponChangeRes = require("../netMessage/weaponMessage").WeaponChangeRes;
var WeaponSkillTalentUpReq = require("../netMessage/weaponMessage").WeaponSkillTalentUpReq;
var WeaponSkillTalentUpRes = require("../netMessage/weaponMessage").WeaponSkillTalentUpRes;
var WeaponElementChangeReq = require("../netMessage/weaponMessage").WeaponElementChangeReq;
var WeaponElementChangeRes = require("../netMessage/weaponMessage").WeaponElementChangeRes;
var enProp = require("../enumType/propDefine").enProp;
var SkillLvCostConfig = require("../gameConfig/skillLvCostConfig");
var logUtil = require("../../libs/logUtil");
var enEquipPos = require("../equip/equip").enEquipPos;
var valueConfig = require("../gameConfig/valueConfig");
var eventNames = require("../enumType/eventDefine").eventNames;

/**
 * 换武器
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {WeaponChangeReq} changeWeaponReq
 * @return {WeaponChangeRes|ResultCode|ResultCodeWeapon|Number}
 */
function onChangeWeapon(session, role, msgObj, changeWeaponReq) {
    let weaponPart =role.getWeaponPart();
    let len = enEquipPos.maxWeapon -enEquipPos.minWeapon+1;
    let weaponIdx= changeWeaponReq.weapon;

    //位置可能出错了
    if(weaponIdx>= len || weaponPart.curWeapon ==weaponIdx)
        return ResultCodeWeapon.WEAPON_POS_ERROR;

    //换武器
    weaponPart.curWeapon =weaponIdx;
    weaponPart.saveWeaponIdx();

    role.fireEvent(eventNames.EQUIP_CHANGE);
    role.fireEvent(eventNames.WEAPON_CHANGE);

    return new WeaponChangeRes(weaponIdx);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_WEAPON, CmdIdsWeapon.CMD_CHANGE_WEAPON, onChangeWeapon, WeaponChangeReq);

/**
 * 技能升级
 * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
 * @param {Role} role - 在这里role是空的，还没有role
 * @param {Message} msgObj
 * @param {WeaponSkillLevelUpReq} req
 * @return {WeaponSkillLevelUpRes|ResultCode|ResultCodeWeapon|Number}
 */
function onWeaponSkillLevelUp(session, role, msgObj, req)
{
    let weaponPart =role.getWeaponPart();
    let skill = weaponPart.getWeaponSkill(req.weapon,req.skill);
    //是不是有这个技能
    if(!skill)
        return ResultCode.NOT_EXIST_ERROR;

    var maxSkillLevel = parseInt(valueConfig.getConfigValueConfig("maxSkillLevel")["value"]);
    //技能是不是已经满级了
    if(skill.lv>=maxSkillLevel)
        return ResultCodeWeapon.LEVEL_MAX;

    //是不是要升到下一级
    if(req.lv!=skill.lv+1)
        return ResultCodeWeapon.LEVEL_ERROR;

    //技能等级不能超过角色等级
    if(req.lv>role.getNumber(enProp.level))
        return ResultCodeWeapon.ROLE_LEVEL_LIMIT;

    //消耗足够
    let skillCfg =skill.getSkillCfg();
    if(!skillCfg){
        logUtil.error("找不到武器的技能配置");
        return ResultCode.CONFIG_ERROR;
    }
    var costItems = SkillLvCostConfig.getSkillLvCostConfig(skillCfg.levelCostId+skill.lv).upgradeCost;
    if(!role.getItemsPart().canCostItems(costItems))
        return ResultCodeWeapon.NO_ENOUGH_GOLD;

    role.getItemsPart().costItems(costItems);

    //升级
    skill.lv =req.lv;
    weaponPart.saveWeaponSkillLv(req.weapon,req.skill);

    if(req.weapon == weaponPart.curWeapon)
    {
        role.fireEvent(eventNames.WEAPON_CHANGE);
    }


    return new WeaponSkillLevelUpRes(req.weapon,req.skill,req.lv);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_WEAPON, CmdIdsWeapon.CMD_SKILL_LEVEL_UP, onWeaponSkillLevelUp, WeaponSkillLevelUpReq);

/**
 * 铭文升级
 * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
 * @param {Role} role - 在这里role是空的，还没有role
 * @param {Message} msgObj
 * @param {WeaponSkillTalentUpReq} req
 * @return {WeaponSkillTalentUpRes|ResultCode|ResultCodeWeapon|Number}
 */
function onTalentUp(session, role, msgObj, req)
{
    let weaponPart =role.getWeaponPart();
    let talent = weaponPart.getTalent(req.weapon,req.skill,req.talent);
    //是不是有这个铭文
    if(!talent)
        return ResultCode.NOT_EXIST_ERROR;

    var maxLevel = parseInt(valueConfig.getConfigValueConfig("maxTalentLevel")["value"]);
    //技能是不是已经满级了
    if(talent.lv>=maxLevel)
        return ResultCodeWeapon.LEVEL_MAX;

    //是不是要升到下一级
    if(req.lv!=talent.lv+1)
        return ResultCodeWeapon.LEVEL_ERROR;

    //消耗足够
    let cfg =talent.getTalentCfg();
    if(!cfg){
        logUtil.error("找不到铭文的配置");
        return ResultCode.CONFIG_ERROR;
    }
    var costItems = SkillLvCostConfig.getSkillLvCostConfig(cfg.levelCostId+talent.lv).upgradeCost;
    if(!role.getItemsPart().canCostItems(costItems))
        return ResultCodeWeapon.NO_ENOUGH_ITEM;

    role.getItemsPart().costItems(costItems);

    //升级
    talent.lv =req.lv;
    weaponPart.saveTalentLv(req.weapon,req.skill,req.talent);

    if(req.weapon == weaponPart.curWeapon)
    {
        role.fireEvent(eventNames.WEAPON_CHANGE);
    }

    return new WeaponSkillTalentUpRes(req.weapon,req.skill,req.talent,req.lv);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_WEAPON, CmdIdsWeapon.CMD_TALENT_LEVEL_UP, onTalentUp, WeaponSkillTalentUpReq);

/**
 * 切换武器属性
 * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
 * @param {Role} role - 在这里role是空的，还没有role
 * @param {Message} msgObj
 * @param {WeaponElementChangeReq} req
 * @return {WeaponSkillTalentUpRes|ResultCode|ResultCodeWeapon|Number}
 */
function onChangeElement(session, role, msgObj, req)
{
    let weaponPart =role.getWeaponPart();
    let elements = weaponPart.getWeaponElement(req.weapon);
    if(!elements)
        return ResultCode.NOT_EXIST_ERROR;

    //位置错误
    if(req.idx <=0 || req.idx>=elements.length)
        return ResultCodeWeapon.ELEMENT_POS_ERROR;

    //交换下
    let i = elements[0];
    elements[0] =elements[req.idx];
    elements[req.idx] =i;
    weaponPart.saveElement(req.weapon);

    return new WeaponElementChangeRes(req.weapon,elements);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_WEAPON, CmdIdsWeapon.CMD_CHANGE_ELEMENT, onChangeElement, WeaponElementChangeReq);