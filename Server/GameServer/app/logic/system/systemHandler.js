"use strict";

var logUtil = require("../../libs/logUtil");
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("./../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("./../netMessage/netMsgConst").ResultCode;
var systemMessage = require("../netMessage/systemMessage");
var CmdIdsSystem = require("./../netMessage/systemMessage").CmdIdsSystem;
var ResultCodeSystem = require("./../netMessage/systemMessage").ResultCodeSystem;

/**
 *
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {SetTeachDataVo} reqObj
 */
function setTeachData(session, role, msgObj, reqObj)
{
    var part = role.getSystemsPart();
    var ret = part.setTeachData(reqObj.key, reqObj.val);
    switch (ret)
    {
        case 1:
        case 2:
            return ResultCodeSystem.BAD_TEACH_DATA;
        case 3:
            return ResultCodeSystem.TEACH_KEYS_MAX;
        default:
            return ResultCode.SUCCESS;
    }
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SYSTEM, CmdIdsSystem.CMD_SET_TEACH_DATA, setTeachData, systemMessage.SetTeachDataVo, true);