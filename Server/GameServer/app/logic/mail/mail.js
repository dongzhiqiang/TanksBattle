"use strict";

var logUtil = require("../../libs/logUtil");
//邮件状态
var enMailStatus = {
    Unread  : 0,    //未读
    Readed  : 1,    //已读
};

class Mail
{
    constructor(data)
    {
        this.mailId = data.mailId;
        this.title = data.title;
        this.sender = data.sender;
        this.status = data.status;
        this.sendTime = data.sendTime;
        this.content = data.content;
        this.attach = data.attach;  //附件

    }

    release()
    {
        logUtil.debug("邮件销毁，mailId：" + this.mailId);
    }
}

/**
 *
 * @param data
 * @returns {Mail|null}
 */

function createMail(data)
{
    return new Mail(data);
}


exports.Mail = Mail;
exports.createMail = createMail;
exports.enMailStatus = enMailStatus;