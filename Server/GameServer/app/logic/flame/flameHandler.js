"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var flameMessage = require("../netMessage/flameMessage");
var CmdIdsFlame = require("../netMessage/flameMessage").CmdIdsFlame;
var ResultCodeFlame = require("../netMessage/flameMessage").ResultCodeFlame;
var enProp = require("../enumType/propDefine").enProp;
var flameConfig = require("../gameConfig/flameConfig");
var flameLevelConfig = require("../gameConfig/flameLevelConfig");
var flameMaterialConfig = require("../gameConfig/flameMaterialConfig");
var eventNames = require("../enumType/eventDefine").eventNames;
var enItemId = require("../enumType/globalDefine").enItemId;

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {UpgradeFlameRequestVo} upgradeReq
 */
function upgradeFlame(session, role, msgObj, upgradeReq) {
    try {

        var flameId = upgradeReq.flameId;
        var items = upgradeReq.items;
        var flame = role.getFlamesPart().getFlameByFlameId(flameId);
        var flameCfg = flameConfig.getFlameConfig(flameId);
        if(!flameCfg)
        {
            msgObj.setResponseData(ResultCodeFlame.FLAME_NOT_EXIST);
            role.send(msgObj);
            return;
        }
        if(flameCfg.needLevel > role.getNumber(enProp.level))
        {
            msgObj.setResponseData(ResultCodeFlame.FLAME_LEVEL_LIMIT);
            role.send(msgObj);
            return;
        }
        var level = 0;
        var exp = 0;
        if(flame)
        {
            level = flame.level;
            exp = flame.exp;
        }
        var oldLevel = level;

        var levelCfg = flameLevelConfig.getFlameLevelConfig(flameId, level+1);
        if(!levelCfg)
        {
            msgObj.setResponseData(ResultCodeFlame.FLAME_MAX_LEVEL);
            role.send(msgObj);
            return;
        }
        var totalExp = 0;
        var costItems = {};
        for(var i=0; i<items.length; i++)
        {
            if(items[i])
            {
                var itemId = items[i].itemId;
                var num = items[i].num;
                costItems[itemId] = (costItems[itemId] || 0) + num;
                var materialCfg = flameMaterialConfig.getFlameMaterialConfig(itemId);
                if(!materialCfg)
                {
                    msgObj.setResponseData(ResultCodeFlame.FLAME_WRONG_ITEM);
                    role.send(msgObj);
                    return;
                }
                totalExp += materialCfg.exp*num;
            }
        }

        if(!role.getItemsPart().canCostItems(costItems))
        {
            msgObj.setResponseData(ResultCodeFlame.FLAME_NO_ENOUGE_ITEM);
            role.send(msgObj);
            return;
        }

        var costGold = totalExp * flameCfg.costGold;
        if(role.getNumber(enProp.gold) < costGold)
        {
            msgObj.setResponseData(ResultCodeFlame.FLAME_NO_ENOUGE_GOLD);
            role.send(msgObj);
            return;
        }

        exp += totalExp;
        while(levelCfg)
        {
            if(exp >= levelCfg.exp)
            {
                level++;
                exp -= levelCfg.exp;
                levelCfg = flameLevelConfig.getFlameLevelConfig(flameId, level+1);
            }
            else
            {
                break;
            }
        }

        role.getItemsPart().costItems(costItems);
        role.getItemsPart().costItem(enItemId.GOLD,costGold);

        if(flame)
        {
            flame.level = level;
            flame.exp = exp;
            flame.syncAndSave();
        }
        else
        {
            role.getFlamesPart().addFlameWithData({flameId: flameId, level: level, exp: exp});
        }

        if(level-oldLevel>0)
        {
            role.fireEvent(eventNames.FLAME_CHANGE);
        }

        var result = new flameMessage.UpgradeFlameResultVo();
        result.levelAdd = level-oldLevel;
        msgObj.setResponseData(ResultCode.SUCCESS, result);
        role.send(msgObj);

    }catch (err){
        logUtil.error("flameHandler~upgradeFlame", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_FLAME, CmdIdsFlame.CMD_UPGRADE_FLAME, upgradeFlame, flameMessage.UpgradeFlameRequestVo);


