"use strict";

var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var mailMessage = require("../netMessage/mailMessage");
var CmdIdsMail = require("../netMessage/mailMessage").CmdIdsMail;
var ResultCodeMail = require("../netMessage/mailMessage").ResultCodeMail;
var MailReadRewardReq = require("../netMessage/mailMessage").ReadOrRewardMailReq;
var MailOneKeyItemsReq = require("../netMessage/mailMessage").OneKeyItemsReq;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;

//邮件查看、关闭、领奖
function reqReadOrReward(session, role, msgObj, reqObj)
{
    let mailPart = role.getMailPart();
    if(!mailPart.setMailStatus(role, reqObj.mailId, reqObj.type))
        return ResultCodeMail.MAIL_STATUS_CHANGE_ERROR;  //状态有误

    return new mailMessage.ReadOrRewardMailRes(reqObj.mailId, reqObj.type);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_MAIL, CmdIdsMail.CMD_READ_OR_REWARD_MAIL, reqReadOrReward, MailReadRewardReq);

//一键领取物品
function reqOneKeyItems(session, role, msgObj, reqObj)
{
    let mailPart = role.getMailPart();
    if(!mailPart.oneKeyGetAttachment(role, reqObj.mailIds))
        return ResultCodeMail.MAIL_ONEKEY_ITEM_ERROR;

    return ResultCode.SUCCESS;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_MAIL, CmdIdsMail.CMD_ONEKEY_REWARDS, reqOneKeyItems, MailOneKeyItemsReq);