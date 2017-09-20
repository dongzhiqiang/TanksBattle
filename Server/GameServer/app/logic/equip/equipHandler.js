"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var equipMessage = require("../netMessage/equipMessage");
var CmdIdsEquip = require("../netMessage/equipMessage").CmdIdsEquip;
var ResultCodeEquip = require("../netMessage/equipMessage").ResultCodeEquip;
var enProp = require("../enumType/propDefine").enProp;
var equipConfig = require("../gameConfig/equipConfig");
var equipUpgradeCostConfig = require("../gameConfig/equipUpgradeCostConfig");
var equipAdvanceCostConfig = require("../gameConfig/equipAdvanceCostConfig");
var equipAdvanceRateConfig = require("../gameConfig/equipAdvanceRateConfig");
var equipRouseCostConfig = require("../gameConfig/equipRouseCostConfig");
var eventNames = require("../enumType/eventDefine").eventNames;
var enEquipPos = require("../equip/equip").enEquipPos;
var dateUtil = require("../../libs/dateUtil");

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpgradeEquipRequestVo} upgradeReq
 */
function upgrade(session, role, msgObj, upgradeReq) {
    try {

        var ownerGUID = upgradeReq.ownerGUID;
        var equipPosIndex = upgradeReq.equipPosIndex;
        var owner = role;
        var equip = owner.getEquipsPart().getEquipByIndex(equipPosIndex);
        if(equip.level >= owner.getPropsPart().getNumber(enProp.level))
        {
            msgObj.setResponseData(ResultCodeEquip.EQUIP_PLAYER_LEVEL_LIMIT);
            role.send(msgObj);
            return;
        }
        var equipCfg = equipConfig.getEquipConfig(equip.equipId);
        var equipAdvCfg = equipAdvanceRateConfig.getEquipAdvanceRateConfig(equip.advLv);
        if(equip.level >= equipAdvCfg.maxLv)
        {
            msgObj.setResponseData(ResultCodeEquip.EQUIP_LEVEL_LIMIT);
            role.send(msgObj);
            return;
        }
        var costCfg = equipUpgradeCostConfig.getEquipUpgradeCostConfig(equip.getPosIndex()+"_"+equip.level);
        var costItems = costCfg.cost;
        if(!role.getItemsPart().canCostItems(costItems))
        {
            msgObj.setResponseData(ResultCodeEquip.EQUIP_NO_ENOUGE_ITEM);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costItems(costItems);

        var result = new equipMessage.GrowEquipResultVo();
        result.guidOwner = ownerGUID;
        result.oldEquip = equip.getCopyedData();

        equip.level = equip.level + 1;
        equip.syncAndSave();

        owner.fireEvent(eventNames.EQUIP_CHANGE);

        result.newEquip = equip;

        //记录升级装备时间，并更新每日装备升级次数
        role.getTaskPart().addEquipUpgradeNum(1);

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("equipHandler~upgrade", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_EQUIP, CmdIdsEquip.CMD_UPGRADE, upgrade, equipMessage.UpgradeEquipRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {AdvanceEquipRequestVo} advanceReq
 */
function advance(session, role, msgObj, advanceReq) {
    try {

        var ownerGUID = advanceReq.ownerGUID;
        var equipPosIndex = advanceReq.equipPosIndex;
        var owner = role;
        var equip = owner.getEquipsPart().getEquipByIndex(equipPosIndex);
        var equipCfg = equipConfig.getEquipConfig(equip.equipId);
        var equipAdvCfg = equipAdvanceRateConfig.getEquipAdvanceRateConfig(equip.advLv);
        if(equip.level < equipAdvCfg.needLv)
        {
            msgObj.setResponseData(ResultCodeEquip.EQUIP_NO_LEVEL);
            role.send(msgObj);
            return;
        }
        var costCfg = equipAdvanceCostConfig.getEquipAdvanceCostConfig(equip.getPosIndex() + "_" + equip.advLv);
        if(!costCfg)
        {
            msgObj.setResponseData(ResultCodeEquip.EQUIP_MAX_ADV_LV);
            role.send(msgObj);
            return;
        }
        var costItems = costCfg.getCost();
        if(!role.getItemsPart().canCostItems(costItems))
        {
            msgObj.setResponseData(ResultCodeEquip.EQUIP_NO_ENOUGE_ITEM);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costItems(costItems);

        var result = new equipMessage.GrowEquipResultVo();
        result.guidOwner = ownerGUID;
        result.oldEquip = equip.getCopyedData();

        equip.advLv = equip.advLv+1;
        equip.syncAndSave();

        owner.fireEvent(eventNames.EQUIP_CHANGE);

        result.newEquip = equip;

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("equipHandler~advance", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_EQUIP, CmdIdsEquip.CMD_ADVANCE, advance, equipMessage.AdvanceEquipRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {RouseEquipRequestVo} rouseReq
 */
function rouse(session, role, msgObj, rouseReq) {
    try {

        var ownerGUID = rouseReq.ownerGUID;
        var equipPosIndex = rouseReq.equipPosIndex;
        var owner = role;
        var equip = owner.getEquipsPart().getEquipByIndex(equipPosIndex);
        var equipCfg = equipConfig.getEquipConfig(equip.equipId);
        var costCfg = equipRouseCostConfig.getEquipRouseCostConfig(equipCfg.rouseCostId);
        if(!costCfg)
        {
            msgObj.setResponseData(ResultCodeEquip.EQUIP_MAX_ROUSE);
            role.send(msgObj);
            return;
        }
        var costItems = costCfg.cost;
        if(!role.getItemsPart().canCostItems(costItems))
        {
            msgObj.setResponseData(ResultCodeEquip.EQUIP_NO_ENOUGE_ITEM);
            role.send(msgObj);
            return;
        }
        role.getItemsPart().costItems(costItems);

        var result = new equipMessage.GrowEquipResultVo();
        result.guidOwner = ownerGUID;
        result.oldEquip = equip.getCopyedData();

        equip.equipId = equipCfg.rouseEquipId;
        equip.syncAndSave();

        owner.fireEvent(eventNames.EQUIP_CHANGE);

        result.newEquip = equip;

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("equipHandler~rouse", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_EQUIP, CmdIdsEquip.CMD_ROUSE, rouse, equipMessage.RouseEquipRequestVo);

/**
 *
 * @param {Role} role
 * @param {Equip} equip
 * @param {object} removeItemIdMap
 * @param {object} updateItemIdMap
 * @returns {boolean}
 */
function tryAdvanceOneEquip(role, equip, removeItemIdMap, updateItemIdMap)
{
    var costCfg = equipAdvanceCostConfig.getEquipAdvanceCostConfig(equip.getPosIndex() + "_" + equip.advLv);
    if(!costCfg)
    {
        return false;
    }
    var costItems = costCfg.getCost();
    if(!role.getItemsPart().canCostItems(costItems))
    {
        return false;
    }
    role.getItemsPart().batchCostItems(costItems, removeItemIdMap, updateItemIdMap);

    equip.advLv = equip.advLv+1;

    return true;
}

/**
 *
 * @param {Role} role
 * @param {Equip} equip
 * @param {object} removeItemIdMap
 * @param {object} updateItemIdMap
 * @returns {boolean}
 */
function tryUpgradeOneEquip(role, equip, removeItemIdMap, updateItemIdMap)
{
    var costCfg = equipUpgradeCostConfig.getEquipUpgradeCostConfig(equip.getPosIndex() + "_" + equip.level);
    var costItems = costCfg.cost;
    if (!role.getItemsPart().canCostItems(costItems)) {
        return false;
    }
    role.getItemsPart().batchCostItems(costItems, removeItemIdMap, updateItemIdMap);

    equip.level = equip.level + 1;
    return true;
}

/**
 *
 * @param {Role} role
 * @param {Role} owner
 * @param {Equip} equip
 * @param {object} resultObj
 * @param {object} removeItemIdMap
 * @param {object} updateItemIdMap
 * @returns {boolean}
 */
function upgradeOnceOneEquip(role, owner, equip, resultObj, removeItemIdMap, updateItemIdMap)
{
    resultObj.oldEquip = equip.getCopyedData();

    var changed = false;

    while(true) {

        var equipAdvCfg = equipAdvanceRateConfig.getEquipAdvanceRateConfig(equip.advLv);
        if(equip.level >= equipAdvCfg.maxLv){
            if(tryAdvanceOneEquip(role, equip, removeItemIdMap, updateItemIdMap))
            {
                changed = true;
            }
            else
            {
                break;
            }
        }
        else
        {
            if (equip.level >= owner.getPropsPart().getNumber(enProp.level)) {
                break;
            }
            if(tryUpgradeOneEquip(role, equip, removeItemIdMap, updateItemIdMap))
            {
                changed = true;
            }
            else
            {
                break;
            }
        }

    }

    if(changed) {
        equip.syncAndSave();
    }

    resultObj.newEquip = equip;

    return changed;
}

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpgradeOnceEquipRequestVo} upgradeOnceReq
 */
function upgradeOnce(session, role, msgObj, upgradeOnceReq) {
    try {

        var ownerGUID = upgradeOnceReq.ownerGUID;
        var equipPosIndex = upgradeOnceReq.equipPosIndex;
        var owner = role;
        var equip = owner.getEquipsPart().getEquipByIndex(equipPosIndex);
        var equipCfg = equipConfig.getEquipConfig(equip.equipId);
        var equipAdvCfg = equipAdvanceRateConfig.getEquipAdvanceRateConfig(equip.advLv);

        var result = new equipMessage.GrowEquipResultVo();
        result.guidOwner = ownerGUID;
        result.oldEquip = equip.getCopyedData();

        var resultObj = {};

        var removeItemIdMap = {};
        var updateItemIdMap = {};

        role.getItemsPart().startBatchCostItems();
        var changed = upgradeOnceOneEquip(role, owner, equip, resultObj, removeItemIdMap, updateItemIdMap);
        role.getItemsPart().endBatchCostItems(removeItemIdMap, updateItemIdMap);

        if(changed) {
            owner.fireEvent(eventNames.EQUIP_CHANGE);
        }

        result.newEquip = equip;



        //记录升级装备时间，并更新每日装备升级次数
        var upGradeLevel = result.newEquip.level - result.oldEquip.level;
        role.getTaskPart().addEquipUpgradeNum(upGradeLevel);

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("equipHandler~upgradeOnce", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_EQUIP, CmdIdsEquip.CMD_UPGRADE_ONCE, upgradeOnce, equipMessage.UpgradeOnceEquipRequestVo);


/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpgradeAllEquipRequestVo} upgradeAllReq
 */
function upgradeAll(session, role, msgObj, upgradeAllReq) {
    try {

        var ownerGUID = upgradeAllReq.ownerGUID;
        var owner = role;

        var result = new equipMessage.UpgradeAllEquipResultVo(ownerGUID, []);

        var removeItemIdMap = {};
        var updateItemIdMap = {};
        role.getItemsPart().startBatchCostItems();

        var equips = [];
        var weapon;
        var equip;

        for(var equipPosIndex=0;equipPosIndex<enEquipPos.equipCount;equipPosIndex++) {
            if (equipPosIndex >= enEquipPos.minWeapon && equipPosIndex <= enEquipPos.maxWeapon) {
                if (owner.getWeaponPart()) {
                    if (equipPosIndex != owner.getWeaponPart().curWeapon + enEquipPos.minWeapon) {
                        //不是当前装备武器不升级
                        continue;
                    }
                }
                equip = owner.getEquipsPart().getEquipByIndex(equipPosIndex);
                if(equip)weapon = equip;
                continue;
            }
            equip = owner.getEquipsPart().getEquipByIndex(equipPosIndex);
            if (!equip) {
                continue;
            }
            equips.push(equip);
        }
        equips.splice(0, 0, weapon);
        var upGradeLevel = 0; //装备升级总次数
        for(var i=0; i<equips.length; i++)
        {
            equip = equips[i];
            var equipCfg = equipConfig.getEquipConfig(equip.equipId);
            var equipAdvCfg = equipAdvanceRateConfig.getEquipAdvanceRateConfig(equip.advLv);

            var resultObj = {};

            if(!upgradeOnceOneEquip(role, owner, equip, resultObj, removeItemIdMap, updateItemIdMap))
            {
                continue;
            }
            upGradeLevel += (resultObj.newEquip.level - resultObj.oldEquip.level);
            result.equipList.push(resultObj);
        }

        //记录升级装备时间，并更新每日装备升级次数
        role.getTaskPart().addEquipUpgradeNum(upGradeLevel);


        role.getItemsPart().endBatchCostItems(removeItemIdMap, updateItemIdMap);

        if(result.equipList.length>0) {
            owner.fireEvent(eventNames.EQUIP_CHANGE);
        }

        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("equipHandler~upgradeAll", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    };
}
handlerMgr.registerHandler(ModuleIds.MODULE_EQUIP, CmdIdsEquip.CMD_UPGRADE_ALL, upgradeAll, equipMessage.UpgradeAllEquipRequestVo);