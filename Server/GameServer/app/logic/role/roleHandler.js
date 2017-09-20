"use strict";

var Promise = require("bluebird");

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var roleMessage = require("../netMessage/roleMessage");
var CmdIdsRole = require("../netMessage/roleMessage").CmdIdsRole;
var ResultCodeRole = require("../netMessage/roleMessage").ResultCodeRole;
var roleMgr = require("./roleMgr");

var reqHeroInfoCoroutine = Promise.coroutine(
    /**
     *
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {RequestHeroInfoVo} reqObj
     */
    function * (session, role, msgObj, reqObj) {
        var heroId = reqObj.heroId;

        var roleOp = yield roleMgr.findRoleOrLoadOfflineByHeroId(heroId);
        if (roleOp) {
            msgObj.setResponseData(ResultCode.SUCCESS, roleOp.getProtectNetData());
            role.send(msgObj);
        }
        else {
            msgObj.setResponseData(ResultCodeRole.ROLE_NOT_EXISTS);
            role.send(msgObj);
        }
    }
);

function reqHeroInfo(session, role, msgObj, reqObj) {
    reqHeroInfoCoroutine(session, role, msgObj, reqObj).catch (function (err){
        logUtil.error("roleHandler~reqHeroInfo", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    });
}
handlerMgr.registerHandler(ModuleIds.MODULE_ROLE, CmdIdsRole.CMD_REQ_HERO_INFO, reqHeroInfo, roleMessage.RequestHeroInfoVo);

