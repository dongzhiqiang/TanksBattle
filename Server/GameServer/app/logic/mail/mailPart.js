"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var mailMessage = require("../netMessage/mailMessage");
var CmdIdsMail = require("../netMessage/mailMessage").CmdIdsMail;
var dateUtil = require("../../libs/dateUtil");
var Mail = require("../mail/mail");
var enMailStatus = require("../mail/mail").enMailStatus;
var valueConfig = require("../gameConfig/valueConfig");


class MailPart{
    /**
     * @param {Role} role
     * @param {object} data
     */
    constructor(role, data) {
        this._mails = [];
        /**
         * 定义role
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});
        /**
         * 存储mail的数据集合
         * @type {Role}
         */
        Object.defineProperty(this, "_mailMap", {enumerable: false, writable:true, value: {}});


        //登录初始化数据
        try {
            var mails = data.mails || [];
            let len = mails.length;
            if(len == 0)
                return;
            //检查超时邮件、超额邮件
            var waiteDelete = [];

            for (var i = 0; i < len; ++i) {
                var m = Mail.createMail(mails[i]);
                if (!m)
                    throw new Error("创建邮件失败");

                if(m.attach.length == 0 && (dateUtil.getTimestamp() - m.sendTime) > valueConfig.getMailKeepTime())//收集超时的普通附件
                    waiteDelete.push(m.mailId);
                else
                    this.addMail(m);
            }
            if(waiteDelete.length > 0){
                //从数据库中删掉
                var userId = this._role.getUserId();
                var heroId = this._role.getHeroId();
                var db = dbUtil.getDB(userId);
                var col = db.collection("role");
                col.updateOneNoThrow({"props.heroId":heroId}, {$pull:{"mails":{"mailId":{"$in":waiteDelete}}}});
            }
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        let mails = this._mails;
        let len = mails.length;
        for (var i = 0; i < len; ++i)
        {
            mails[i].release();
        }
        this._mails = [];
        this._mailMap = {};
    }
    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj) {
        rootObj.mails = this._mails;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj) {
        rootObj.mails = this._mails;
    }
    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
    }

    /**
     * 保存、同步已在数据库的邮件
     * @param {Mail?} mail
     * @returns {Boolean}
     */
    syncAndSaveMail(mail)
    {
        if(!this._role.isHero() || this._role.isRobot())
            return false;
        if (!mail)
            return false;

        var ownerRole = this._role;
        //存盘
        var userId = ownerRole.getUserId();
        var heroId = ownerRole.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId, "mails.mailId":mail.mailId}, {$set:{"mails.$":mail}});//注意数据这么写

        //通知客户端
        var netMsg = new mailMessage.AddOrUpdateMailRes(mail,false);
        ownerRole.sendEx(ModuleIds.MODULE_MAIL, CmdIdsMail.PUSH_ADD_OR_UPDATE_MAIL, netMsg);

        return true;
    }

    /**
     *
     * @param {Mail} mail
     * @private
     */
    addMail(mail)
    {
        this._mails.push(mail);
        this._mailMap[mail.mailId] = mail;
    }

    /**
     * 删除邮件
     * @param {Mail} mail
     */
    removeMail(mail)
    {
        if(!this._role.isHero() || this._role.isRobot())
            return false;
        if(!mail)//检查下
            return;
        //删除
        this._mails.removeValue(mail);
        delete this._mailMap[mail.mailId];

        try {
            mail.release();
        }
        catch (err) {
            logUtil.error("邮件删除出错", err);
        }

        //存盘
        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId}, {$pull:{"mails":{"mailId":mail.mailId}}});

        //通知客户端
        var netMsg = new mailMessage.DeleteMailRes(mail.mailId);
        this._role.sendEx(ModuleIds.MODULE_MAIL, CmdIdsMail.PUSH_DELETE_MAIL, netMsg);


        return true;
    }
    /**
     * 批量删除邮件
     * @param {String[]} ids
     * @returns {Boolean}
     */
    removeMailsByIds(ids)
    {
        if(!this._role.isHero() || this._role.isRobot())
            return false;
        if(!ids || ids.length == 0)//检查下
            return true;
        let count = ids.length;
        var mail;
        for(let i=0;i<count;++i)
        {
            mail = this.getMailByMailId(ids[i]);
            if(mail == null)
                continue;
            //删除
            this._mails.removeValue(mail);
            delete this._mailMap[mail.mailId];

            try {
                mail.release();
            }
            catch (err) {
                logUtil.error("邮件删除出错", err);
            }
        }
        //存盘
        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId}, {$pull:{"mails":{"mailId":{"$in":ids}}}});

        //通知客户端
        var netMsg = new mailMessage.DeleteMailsRes(ids);
        this._role.sendEx(ModuleIds.MODULE_MAIL, CmdIdsMail.PUSH_DELETE_MAILS, netMsg);


        return true;

    }
    /**
     * 获取邮件数量
     * @returns {Number}
     */
    getAllMailsCount()
    {
        return this._mails.length;
    }
    /**
     * 根据邮件id获取邮件信息
     * @param {String} mailId
     * @returns {Mail|undefined}
     */
    getMailByMailId(mailId)
    {
        return this._mailMap[mailId];
    }

    /**
     * 阅读邮件、关闭邮件、领取附件
     * @returns {Boolean}
     */
    setMailStatus(role, mailId, type)
    {
        if(!role.isHero() || role.isRobot())
            return false;
        let mail = this.getMailByMailId(mailId);
        if(mail == null)
            return false;
        if(type == 1)//打开邮件
        {
            if(mail.attach.length == 0){
                mail.status = enMailStatus.Readed;
                this._mailMap[mailId] = mail;
                this.syncAndSaveMail(mail);
            }
            return true;
        }
        else if(type == 2 && mail.attach.length == 0)//阅读完无附件邮件并点击关闭，自动删除
        {
            this.removeMail(mail);
            return true;
        }
        else if(type == 3)//领取附件
        {
            if(mail.attach.length == 0)
                return false;

            var itemsPart = role.getItemsPart();
            let count = mail.attach.length;
            var items = {};
            for(var i=0; i<count;++i)
            {
                items[mail.attach[i].itemId] = (items[mail.attach[i].itemId] || 0) + mail.attach[i].num;
            }
            itemsPart.addItems(items);
            this.removeMail(mail);
            return true;
        }
        return false;
    }
    /**
     * 一键领取附件
     * @returns {Boolean}
     */
    oneKeyGetAttachment(role, ids)
    {
        if(!role.isHero() || role.isRobot())
            return false;
        let mailNum = ids.length;
        if(mailNum == 0)
            return false;
        var itemsPart = role.getItemsPart();
        var mail;
        var items = {};
        for(let i=0;i<mailNum;++i) {
            mail = this.getMailByMailId(ids[i]);
            if(mail == null)
                continue;
            if (mail.attach.length == 0)
                continue;
            var count = mail.attach.length;

            for (let m = 0; m < count; ++m) {
                items[mail.attach[m].itemId] = (items[mail.attach[m].itemId] || 0) + mail.attach[m].num;
            }

        }
        itemsPart.addItems(items);
        this.removeMailsByIds(ids);//批量删除邮件
        return true;
    }
    /**
     * 发送邮件
     * @param {Role} role
     * @param {Mail} mail
     * @param {Boolean} noSaveDb 是否存盘
     */
    sendMail(role, mail, noSaveDb)
    {
        if(!role.isHero() || role.isRobot())
            return;
        var dels = [];
        //在线的角色要添加到map
        if(this._mails.length >= valueConfig.getMaxMaillNum())   //在线的要更新缓存数据
        {
            dels = this._mails.splice(0,this._mails.length - valueConfig.getMaxMaillNum()+1);
            for(var i=0,len = dels.length;i<len;i++)
            {
                delete this._mailMap[dels[i].mailId];
            }

        }
        this.addMail(mail);

        //通知客户端
        var netMsg = new mailMessage.AddOrUpdateMailRes(mail, true, dels);
        role.sendEx(ModuleIds.MODULE_MAIL, CmdIdsMail.PUSH_ADD_OR_UPDATE_MAIL, netMsg);

        if (!noSaveDb)
            MailPart.saveMailToOneRole(this._role.getUserId(), this._role.getHeroId(), mail);
    }
    /**
     * 存盘
     * @param {String} userId
     * @param {Number} heroId
     * @param {Mail} mail
     */
    static saveMailToOneRole(userId, heroId, mail)
    {
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId}, {$push:{"mails":{$each:[mail], $position:0, $slice:valueConfig.getMaxMaillNum()}}});//注意数据这么写
    }

    /**
     * 批量存盘
     * @param {Number[]} heroIds
     * @param {Mail} mail
     */
    static saveMailToMultiRole(heroIds, mail)
    {
        var db = dbUtil.getDB();
        var col = db.collection("role");
        col.updateManyNoThrow({"props.heroId":{$in:heroIds}}, {$push:{"mails":{$each:[mail], $position:0, $slice:valueConfig.getMaxMaillNum()}}});//注意数据这么写
    }

}

exports.MailPart = MailPart;