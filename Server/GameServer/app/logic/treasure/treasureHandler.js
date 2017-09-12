"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var treasureMessage = require("../netMessage/treasureMessage");
var CmdIdsTreasure = require("../netMessage/treasureMessage").CmdIdsTreasure;
var ResultCodeTreasure = require("../netMessage/treasureMessage").ResultCodeTreasure;
var enProp = require("../enumType/propDefine").enProp;
var treasureConfig = require("../gameConfig/treasureConfig");
var eventNames = require("../enumType/eventDefine").eventNames;
var enItemId = require("../enumType/globalDefine").enItemId;

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpgradeTreasureRequestVo} upgradeReq
 * @returns {number|object}
 */
function upgradeTreasure(session, role, msgObj, upgradeReq) {
    try {

        var treasureId = upgradeReq.treasureId;
        var treasure = role.getTreasurePart().getTreasure(treasureId);
        var treasureCfg = treasureConfig.getTreasureConfig(treasureId);
        if(!treasureCfg)
        {
            return ResultCode.NOT_EXIST_ERROR;
        }
        var treasureLevel = 0;
        if(treasure)
        {
            treasureLevel = treasure.level;
        }

        var levelCfg = treasureConfig.getTreasureLevelConfig(treasureId, treasureLevel+1);
        if(!levelCfg)
        {
            return ResultCodeTreasure.TREASURE_MAX_LEVEL;
        }

        if(!role.getItemsPart().canCostGold(levelCfg.costGold))
        {
            return ResultCode.NO_ENOUGH_GOLD;
        }

        if(!role.getItemsPart().canCostItem(treasureCfg.pieceId, levelCfg.pieceNum))
        {
            return ResultCodeTreasure.TREASURE_NOT_ENOUGE_ITEM;
        }


        role.getItemsPart().costGold(levelCfg.costGold);
        role.getItemsPart().costItem(treasureCfg.pieceId, levelCfg.pieceNum);

        if(treasure)
        {
            treasure.level = treasure.level + 1;
            treasure.syncAndSave();
        }
        else
        {
            role.getTreasurePart().addTreasure(treasureId);
        }


        role.fireEvent(eventNames.TREASURE_CHANGE);

        return ResultCode.SUCCESS;

    }catch (err){
        logUtil.error("treasureHandler~upgradeTreasure", err);
        if (err instanceof dbUtil.MongoError)
            return ResultCode.DB_ERROR;
        else
            return ResultCode.SERVER_ERROR;
    }
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_TREASURE, CmdIdsTreasure.CMD_UPGRADE_TREASURE, upgradeTreasure, treasureMessage.UpgradeTreasureRequestVo);

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ChangeBattleTreasureRequestVo} changeReq
 * @returns {number|object}
 */
function changeBattleTreasure(session, role, msgObj, changeReq) {
    try {

        var battleTreasure = changeReq.battleTreasure;
        if(battleTreasure.length>3)
        {
            return ResultCode.BAD_REQUEST;
        }
        for(var i=0; i<battleTreasure.length; i++)
        {
            if(!role.getTreasurePart().getTreasure(battleTreasure[i]))
            {
                return ResultCode.NOT_EXIST_ERROR;
            }
        }

        role.getTreasurePart().setBattleTreasure(battleTreasure)

        role.fireEvent(eventNames.TREASURE_CHANGE);

        return ResultCode.SUCCESS;

    }catch (err){
        logUtil.error("treasureHandler~changeBattleTreasure", err);
        if (err instanceof dbUtil.MongoError)
            return ResultCode.DB_ERROR;
        else
            return ResultCode.SERVER_ERROR;
    }
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_TREASURE, CmdIdsTreasure.CMD_CHANGE_BATTLE_TREASURE, changeBattleTreasure, treasureMessage.ChangeBattleTreasureRequestVo);


