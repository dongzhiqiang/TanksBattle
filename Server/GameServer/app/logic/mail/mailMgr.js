"use strict";

var Mail = require("../mail/mail");
var MailPart = require("../mail/mailPart");
var roleMgr = require("../role/roleMgr");
var guidGenerator = require("../../libs/guidGenerator");
var itemModule = require("../item/item");
var dateUtil = require("../../libs/dateUtil");
var enMailStatus = require("../mail/mail").enMailStatus;

/**
 * 批量发送邮件
 * @param {Number[]} heroIds
 * @param {Mail} mail
 */
function sendMailToMultiRole(heroIds, mail)
{
    let count = heroIds.length;
    for (var i=0; i<count; ++i)
    {
        var id = heroIds[i];
        if (id < 0)//机器人除外
            continue;
        /**
         * 在线的修改内存，同步到客户端
         * @type{Role}
         */
        var role = roleMgr.findRoleByHeroId(id);
        if(role != null)
            role.getMailPart().sendMail(role, mail, true);

    }
    //存盘
    MailPart.MailPart.saveMailToMultiRole(heroIds, mail);
}
/**
* 根据内容生成邮件
* @param {String} title 标题
* @param {String} sender 发送者
* @param {String} content 内容
* @param {Item[]?} attach 物品附件
* @returns {Mail}
*/
function createMail(title, sender, content, attach)
{
    attach = attach || [];
    //附件物品检查
    if(attach.length > 0)
    {
        let count = attach.length;
        for (let i=0;i<count;i++)
        {
            if (!itemModule.isItemData(attach[i]))
                return null;
        }
    }
    var obj = {title:title,sender:sender,content:content,sendTime:dateUtil.getTimestamp(),status:enMailStatus.Unread,
        mailId:guidGenerator.generateGUID(),attach:attach,attachStatus:1};
    return Mail.createMail(obj);

}

exports.sendMailToMultiRole = sendMailToMultiRole;
exports.createMail = createMail;