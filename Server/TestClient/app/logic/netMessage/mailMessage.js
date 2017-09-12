"use strict";

const CmdIdsMail = {
    CMD_READ_OR_REWARD_MAIL: 1,  //打开邮件、关闭邮件、领取附件
    CMD_ONEKEY_REWARDS: 2, //一键领奖励


    PUSH_ADD_OR_UPDATE_MAIL: -1,   //更新邮件
    PUSH_ADD_OR_UPDATE_MAILS: -2,   //批量更新邮件
    PUSH_DELETE_MAIL: -3,  //删除邮件
    PUSH_DELETE_MAILS: -4,  //批量删除邮件

};
/////////////////////////////////错误////////////////////////////
const ResultCodeMail = {
    MAIL_STATUS_CHANGE_ERROR: 1,
    MAIL_ONEKEY_ITEM_ERROR:2
};

/////////////////////////////////请求类////////////////////////////

//请求改变邮件状态
class ReadOrRewardMailReq
{
    constructor() {
        this.mailId = "";
        this.type = 0;
    }
    static fieldsDesc() {
        return {
            mailId: {type: String, notNull: true},
            type: {type: Number, notNull: true}
        };
    }
}
//一键领取
class OneKeyItemsReq
{
    constructor(){
        this.mailIds = [];
    }
    static fieldsDesc() {
        return {
            mailIds : {type: Array, itemType:String, notNull: true}
        };
    }


}

/////////////////////////////////推送类////////////////////////////

//添加或更新邮件返回
class AddOrUpdateMailRes
{
    /**
     * @param {Mail} mail
     * @param {Boolean} isAdd - 否则是全部update
     * @param {Mail[]?} delMails 删除的邮件
     */
    constructor(mail, isAdd, delMails) {
        this.mail = mail;
        this.isAdd = isAdd;
        this.delMails = delMails;
    }
}
//添加或更新多封邮件返回
class AddOrUpdateMailsRes
{
    /**
     * @param {Mail[]?} mails
     * @param {Boolean} isAdd - 否则是全部update
     */
    constructor(mails, isAdd) {
        this.mails = mails;
        this.isAdd = isAdd;
    }
}
//请求改变邮件状态返回
class ReadOrRewardMailRes
{
    /**
     * @param {String} mailId
     * @param {Number} type
     */
    constructor(mailId, type) {
        this.mailId = mailId;
        this.type = type;
    }
}
//删除邮件返回
class DeleteMailRes
{
    /**
     * @param {String} mailId
     */
    constructor(mailId) {
        this.mailId = mailId;
    }
}
//批量删除邮件返回
class DeleteMailsRes
{
    /**
     * @param {String[]} mailIds
     */
    constructor(mailIds) {
        this.mailIds = mailIds;
    }
}
//一键领取返回
class OneKeyItemsRes
{
    constructor() {
    }
}
/////////////////////////////////导出////////////////////////////
exports.CmdIdsMail = CmdIdsMail;
exports.ResultCodeMail = ResultCodeMail;

exports.AddOrUpdateMailsRes = AddOrUpdateMailsRes;
exports.AddOrUpdateMailRes = AddOrUpdateMailRes;
exports.ReadOrRewardMailReq = ReadOrRewardMailReq;
exports.ReadOrRewardMailRes = ReadOrRewardMailRes;
exports.DeleteMailRes = DeleteMailRes;
exports.DeleteMailsRes = DeleteMailsRes;
exports.OneKeyItemsReq = OneKeyItemsReq;
exports.OneKeyItemsRes = OneKeyItemsRes;