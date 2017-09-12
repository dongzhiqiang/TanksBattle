"use strict";
////////////外部模块////////////
var Promise = require("bluebird");
var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var dateUtil = require("../../libs/dateUtil");
var enProp = require("../enumType/propDefine").enProp;
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var socialMessage = require("../netMessage/socialMessage");
var CmdIdsSocial = require("../netMessage/socialMessage").CmdIdsSocial;
var onlineRoleMgr = require("../role/onlineRoleMgr");
var roleMgr = require("../role/roleMgr");
var valueConfig = require("../gameConfig/valueConfig");
var FriendMaxConfig = require("../gameConfig/friendMaxConfig");
var ResultCodeSocial = require("../netMessage/socialMessage").ResultCodeSocial;
var eventNames = require("../enumType/eventDefine").eventNames;
var corpsMgr = require("../corps/corpsMgr");
var appUtil = require("../../libs/appUtil");

/**
 * 好友信息
 * @typedef {Object} Friend
 * @property {Number} heroId
 * @property {String} name
 * @property {Number} level
 * @property {String} roleId
 * @property {Number} powerTotal
 * @property {Number} vipLv
 * @property {Number} lastLogout 离线时间
 * @property {Number} upTime 更新时间
*/

/**
 * 根据heroId从数据库读取数据
 */
var loadFriendRoles = Promise.coroutine(
    function * (mainUesrId, mainHeroId, findIds){
        try{
            var db = dbUtil.getDB(mainUesrId);
            var col = db.collection("role");
            var infos = yield col.findArray({"props.heroId":{$in:findIds}},{
                "props.heroId":1,
                "props.name":1,
                "props.level":1,
                "props.roleId":1,
                "props.vipLv":1,
                "props.powerTotal":1,
                "props.lastLogout":1  //离线时间
            });
            var mainRole = onlineRoleMgr.findRoleByHeroId(mainHeroId);
            if(!mainRole)
                return;
            var part = mainRole.getSocialPart();
            part.updateRoleInfosFromDB(infos, findIds);   //

        }
        catch(err){
            throw err;
        }
    }

);

class SocialPart{
    /**
     * @param {Role} role
     * @param {object} data
     */
    constructor(role, data) {
        /**
         * @type {Friend[]}
         * */
        this._friends = [];  //好友
        /**
         * @type {Friend[]}
         * */
        this._addReqs = [];   //他人加好友的申请
        /**
         * @type {Number[]}
         * */
        this._sendStam = [];   //赠送给别人体力 记录他们的heroId
        /**
         * @type {Number[]}
         * */
        this._collStam = [];   //已领的体力 记录他们的heroId
        /**
         * @type {Object[]}
         * */
        this._unCollStam = [];   //别人送的还没领的体力，记录他们的heroId和赠送时间
        /**
         * @type {Number}
         */
        this._updateStamp = 0;  //记录上一次赠送或领取体力的时间，用于判断是否当天从而做重置的操作。
        /**
         * @type {Number}
         */
        this._recUpdateStamp = 0;   //记录上一次刷新推荐好友的时间
        /**
         * @type {Friend[]}
         */
        this._recFriends = [];   //推荐的好友

        /**
         * @type {Number}
         * */
        Object.defineProperty(this, "_friUptime", {enumerable: false, writable:true, value: 0});    //记录自己最近一次请求好友详细数据的时间

        /**
         * 定义role
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});
        /**
         * 存储好友的数据集合
         * @type {Friend}
         */
        Object.defineProperty(this, "_friendsMap", {enumerable: false, writable:true, value: {}});
        /**
         * 存储他人请求的数据集合
         * @type {Friend}
         */
        Object.defineProperty(this, "_addReqMap", {enumerable: false, writable:true, value: {}});


        //登录初始化数据
        try {
            //如果是机器人就返回
            if(role.isRobot())
                return;

            //添加事件处理
            var thisObj = this;
            role.addListener(function(eventName, context, notifier) {
                thisObj.onPropChange({"level":role.getNumber(enProp.level)});
            }, eventNames.LEVEL_UP);   //等级变化
            role.addListener(function(eventName, context, notifier) {
                thisObj.onPropChange({"powerTotal":role.getNumber(enProp.powerTotal)});
            }, eventNames.POWER_CHANGE);   //战斗力变化
            role.addListener(function(eventName, context, notifier) {
                thisObj.onPropChange({"vipLv":role.getNumber(enProp.vipLv)});
            }, eventNames.VIP_LV_CHANGE);   //vip变化

            var socialData = data.social || {};
            var friends = socialData.friends || [];   //好友id数组
            var reqs = socialData.addReqs || [];   //请求id数组
            this._sendStam = socialData.sendStam || [];
            this._collStam = socialData.collStam || [];
            this._unCollStam = socialData.unCollStam || [];
            this._updateStamp = socialData.updateStamp || 0;
            this._recUpdateStamp = socialData.recUpdateStamp || 0;

            if(!dateUtil.isToday(this._updateStamp))  //不是今天,说明需要重置了
            {
                this.resetStaminaNums();
            }

            if(friends.length == 0 && reqs.length == 0)  //好友和申请人都没那就不用再执行了
                return;

            var idsList = [];  //存储没法从roleMgr获取到role的id数组
            var curTime = dateUtil.getTimestamp();
            //初始化map
            for(let i=0;i<friends.length;i++)
            {
                let heroId = friends[i];
                let role = roleMgr.findRoleByHeroId(heroId);
                if(role)
                    this.pushFriend(SocialPart.makeFriendDataFromRole(role, curTime));
                else
                {
                    this._friendsMap[heroId] = {};
                    idsList.push(heroId);
                }
            }
            for(let i=0;i<reqs.length;i++)
            {
                let heroId = reqs[i];

                let role = roleMgr.findRoleByHeroId(heroId);
                if(role)
                    this.pushRequest(SocialPart.makeFriendDataFromRole(role, curTime));
                else
                {
                    this._addReqMap[heroId] = {};
                    idsList.push(heroId);
                }
            }

            if (idsList.length > 0)
                //根据heroId从数据库中取role数据
                loadFriendRoles(this._role.getUserId(), this._role.getHeroId(), idsList);

        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        //断开连接时不会马上调用，当从offlineRoleMgr移除时才调用
        /**
         * @type {Friend[]}
         */
        let friends = this._friends;
        let len = friends.length;
        for (var i = 0; i < len; ++i)
        {
            friends[i] = {};
        }
        this._friends = [];
        this._friendsMap = {};
        this._addReqs = [];
        this._addReqMap = {};
    }
    /**
     * 存盘数据 只存id数组
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.social = {};
        let arr = [];
        for(let i = 0;i < this._friends.length;i++)
            arr.push(this._friends[i].heroId);
        rootObj.social.friends = arr;
        arr.clear();
        for(var i = 0;i<this._addReqs.length;i++)
            arr.push(this._addReqs[i].heroId)
        rootObj.social.addReqs = arr;
        rootObj.social.sendStam = this._sendStam;
        rootObj.social.collStam = this._collStam;
        rootObj.social.unCollStam = this._unCollStam;
    }

    /**
     * 下发客户端的数据 存object数组
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.social = {};
        rootObj.social.friends = this._friends;
        rootObj.social.addReqs = this._addReqs;

        rootObj.social.sendStam = this._sendStam;
        rootObj.social.collStam = this._collStam;
        rootObj.social.unCollStam = this._unCollStam;
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

    //获取最近一次请求好友数据的时间
    getFriUptime()
    {
        return this._friUptime;
    }
    //设置最近一次请求好友数据的时间
    setFriUptime(newTime)
    {
        this._friUptime = newTime;
    }

    /**
     * 从服务端取得的数据更新好友的角色信息
     * @param {object} infos
     * @param {Number[]} findIds
     */
    updateRoleInfosFromDB(infos, findIds)
    {
        var curTime = dateUtil.getTimestamp();
        var infoCnt = infos.length;
        var findCnt = findIds.length;
        for(let i = 0;i < infoCnt; ++i)
        {
            let info = infos[i];
            /** @type {Friend} */
            let props = info.props;
            let heroId = props.heroId;
            props.upTime = curTime;
            if(this._friendsMap[heroId])
                this.pushFriend(props);
            else if(this._addReqMap[heroId])
                this.pushRequest(props);
            else
                logUtil.warn("好友Map未初始化的id："+heroId);
        }
        if(infoCnt != findCnt) //数据有异常，可能某个好友已经从数据库中删除了
        {
            //收集失效的好友
            var badFrdHeroIds = [];
            var badReqHeroIds = [];
            for (let i = 0; i < findCnt;++i)
            {
                let heroId = findIds[i];
                let info = this._friendsMap[heroId];
                if (info && !info.heroId)
                {
                    badFrdHeroIds.push(heroId);
                    delete this._friendsMap[heroId];
                    continue;
                }

                info = this._addReqMap[heroId];
                if (info && !info.heroId)
                {
                    badReqHeroIds.push(heroId);
                    delete this._addReqMap[heroId];
                }
            }
            if(badFrdHeroIds.length > 0 || badReqHeroIds.length > 0)
            {
                //从数据库删除失效的好友id
                var db = dbUtil.getDB(this._role.getUserId());
                var col = db.collection("role");
                col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {$pull:{"social.friends":{$in:badFrdHeroIds}, "social.addReqs":{$in:badReqHeroIds}}});
            }
        }

        //好友通知客户端
        var netMsg = new socialMessage.ReqFriendsRes(this._friends, this._collStam, this._unCollStam, this._sendStam, false);
        this._role.sendEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_REQ_FRIENDS, netMsg);

        //申请通知客户端
        var netMsg = new socialMessage.PushAddFriend(this._addReqs);
        this._role.sendEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.PUSH_ADD_FRIEND, netMsg);
    }

    /**
     * 添加申请列表
     * @param {Friend} friend
     */
    pushRequest(friend)
    {
        this._addReqs.push(friend);
        this._addReqMap[friend.heroId] = friend;
    }

    /**
     * 从申请列表中删除，并返回被删除的这个好友信息
     * @param {Number} heroId
     * @returns {Friend}
     */
    removeRequest(heroId)
    {
        var friend;
        for(let i=0;i<this._addReqs.length;i++)
        {
            if(this._addReqs[i].heroId == heroId)
            {
                friend = this._addReqs.splice(i,1);
                break;
            }
        }
        delete this._addReqMap[heroId];
        return friend[0];
    }

    /**
     * 添加好友列表
     * @param {Friend} friend
     * @private
     */
    pushFriend(friend)
    {
        this._friends.push(friend);
        this._friendsMap[friend.heroId] = friend;
    }

    /**
     * 从好友列表删除好友，并返回这个好友信息
     * @param {Number} heroId
     */
    removeFriend(heroId)
    {
        var friend;
        for(let i=0;i<this._friends.length;i++)
        {
            if(this._friends[i].heroId == heroId)
            {
                friend = this._friends.splice(i,1);
                break;
            }
        }
        delete this._friendsMap[heroId];
    }

    /**
     * 有人送了体力，修改内存
     * @param {Number} heroId 赠送人id
     * @param {Number} timeStamp 赠送的时间
     */
    pushUnCollectStamina(heroId, timeStamp)
    {
        this._unCollStam.push({"heroId":heroId, "timeStamp":timeStamp});
    }

    /**
     * 根据id获取好友信息
     * @param {Number} heroId
     * @returns {Friend|null}
     */
    getFriendbyHeroId(heroId)
    {
        return this._friendsMap[heroId];
    }

    /**
     * 收集uptime有变化的好友信息
     */
    collectUpdateFriend(/*friendUptime*/)
    {
        var friends = [];
        //遍历好友列表，如果uptime不一致就下发
        for(let i=0; i<this._friends.length; i++)
        {
            let friend = this.getFriendbyHeroId(this._friends[i].heroId);
            if(this._friUptime < friend.upTime)//时间不是最新的 收集
            {
                friends.push(friend);
            }
        }
        this._friUptime = dateUtil.getTimestamp();  //更新这个时间
        return friends;
    }

    /**
     * 添加好友
     * @param {Number} reqId
     * @returns {Friend|null}
     */
    addFriend(reqId)
    {
        //先判断自己的好友是否满了
       if(this.isFriendFull())
           return null;

        //存盘操作
        SocialPart.addDBFriend(this._role.getUserId(), this._role.getHeroId(), reqId);

        //更新内存，通知客户端
        //将这个人从申请列表中删除，同时加到好友列表
        var friend = this.removeRequest(reqId);
        this.pushFriend(friend);

        return friend;
    }

    /**
     * 删除好友
     * @param {Number} deleteId
     * @returns {Boolean}
     */
    deleteFriend(deleteId)
    {
        if(this.getFriendbyHeroId(deleteId) == null)
            return false;
        //存盘操作
        var mainUserId = this._role.getUserId();
        var mainHeroId = this._role.getHeroId();
        SocialPart.delDBFriend(mainUserId, mainHeroId, deleteId);

        //修改自己的内存
        this.removeFriend(deleteId);

        //这里如果在线通知对方客户端
        var friRole = onlineRoleMgr.findRoleByHeroId(deleteId);
        if(friRole)
        {
            //修改对方内存
            friRole.getSocialPart().removeFriend(mainHeroId);
            //消息通知对方
            var netMsg = new socialMessage.DeleteFriendRes(mainHeroId);
            friRole.sendEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_DELETE_FRIEND, netMsg);
        }
        return true;
    }

    /**
     * 将别人的加好友请求添加到申请列表中
     * @param {Role} role
     * @param {Friend} adder
     */
    addFriendReq(role, adder)
    {
        //更新缓存
        this.pushRequest(adder);

        //通知客户端
        var netMsg = new socialMessage.PushAddFriend([adder]);
        role.sendEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.PUSH_ADD_FRIEND, netMsg);

    }

    /**
     * 拒绝/删除申请
     * @param {Number} reqId
     * @returns {Friend}
     */
    delFriendReq(reqId)
    {
        //存盘
        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        SocialPart.delDBFriendReq(userId, heroId, [reqId]);  //自己的申请移除存盘
        //更新内存，通知客户端
        //将这个人从申请列表中删除
        var friend = this.removeRequest(reqId);
        return friend;
    }

    /**
     * 删除所有的申请
     */
    delAllFriendReq()
    {
        var ids=[];
        for(let i=0;i<this._addReqs.length;++i)
            ids.push(this._addReqs[i].heroId);
        //存盘
        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        SocialPart.delDBFriendReq(userId, heroId, ids);

        //更新内存
        this._addReqs = [];
        this._addReqMap = {};
    }

    /**
     * 赠送体力
     * @param {Number} heroId 对方heroId
     * @returns {Number}
     */
    sendStaminaReq(heroId)
    {
        //检测下是否超出今天送出的次数上限
        if(this._sendStam.length >= parseInt(valueConfig.getConfigValueConfig("maxSendFriendStam")["value"]))
            return -1;

        //检查下是不是自己好友
        let f = this.getFriendbyHeroId(heroId);
        if(f == null)
            return -3;

        //先检测一下自己是否已经赠送过给对方
        if(this._sendStam.length > 0)
        {
            for(let i=0; i<this._sendStam.length; i++)
            {
                if(this._sendStam[i] == heroId)//今日已经赠送过
                    return -2;
            }
        }
        //更新自己内存
        this._sendStam.push(heroId);
        this._updateStamp = dateUtil.getTimestamp();

        //加入数据库
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {$addToSet:{"social.sendStam":heroId},
            $set: {"social.updateStamp": this._updateStamp}});  //自己的赠送和赠送时间存盘
        col.updateOneNoThrow({"props.heroId":heroId}, {$addToSet:{"social.unCollStam":{"heroId":this._role.getHeroId(), "timeStamp":this._updateStamp}}});
            //对方的接收存盘 存盘数据为赠送人id和赠送时间

        //如果对方在线更新内存、通知对方客户端
        var friRole = onlineRoleMgr.findRoleByHeroId(heroId);
        if(friRole)
        {
            //修改对方内存
            friRole.getSocialPart().pushUnCollectStamina(this._role.getHeroId(), this._updateStamp);
            //消息通知对方
            var netMsg = new socialMessage.PushNewStamina(this._role.getString(enProp.name), this._role.getHeroId(), this._updateStamp);
            friRole.sendEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.PUSH_NEW_STAMINA, netMsg);
        }
        //返回消息通知自己客户端
        return 0;
    }

    /**
     * 领取体力
     * @param {Number} heroId 对方heroId
     * @returns {Number}
     */
    getStaminaReq(heroId)
    {
        //先检测下是否超过今天的领取上限
        if(this._collStam.length >= parseInt(valueConfig.getConfigValueConfig("maxGetFriendStam")["value"]))
            return -1;
        //检查下是不是自己好友
        let f = this.getFriendbyHeroId(heroId);
        if(f == null)
            return -3;
        //检查下对方是否真的有送过体力
        var mark = -1;  //记录索引，不用再遍历一次
        if(this._unCollStam.length > 0)
        {
            for(let i=0,count = this._unCollStam.length; i<count;++i)
            {
                if(this._unCollStam[i]["heroId"] == heroId)
                {
                    mark = i;
                    break;
                }
            }
            if(mark == -1)  //检查完还是没有这个人，返回错误
                return -2;
        }
        else  //未领取表里都没有了，返回错误
            return -2;

        //检查下自己是否已经领过他送的体力
        if(this._collStam.length>0)
        {
            let len = this._collStam.length;
            if(len > 0)
            {
                for(let i=0; i<len; ++i)
                {
                    if(this._collStam[i] == heroId)//已经领过他的体力了
                        return -2;
                }
            }
        }
        //领取体力
        let staValue = parseInt(valueConfig.getConfigValueConfig("friendStamina")["value"])
        this._role.addStamina(staValue);

        //修改内存
        this._updateStamp = dateUtil.getTimestamp();
        this._unCollStam.splice(mark, 1);
        this._collStam.push(heroId);

        //存盘的操作
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {"$addToSet":{"social.collStam":heroId},
            "$pull":{"social.unCollStam":{"heroId":heroId}}});//自己的领取存盘,并从未领取的表中剔除
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {$set: {"social.updateStamp": this._updateStamp}});  //更新时间

        return 0;
    }

    /**
     * 一键领取体力
     * @param {Number[]} ids
     * @returns {Number[]}
     */
    onekeyGetStaminaReq(ids)
    {
        //先检测下是否超过今天的领取上限
        let maxTime = parseInt(valueConfig.getConfigValueConfig("maxGetFriendStam")["value"]);
        if(this._collStam.length >= maxTime)
            return [];
        //服务端再次检查一遍客户端传的次数是否有误
        let remain = maxTime-this._collStam.length;
        if(ids.length > remain)//次数有误，做删减
            ids = ids.splice(0, remain);

        var arr = [];
        for(let i=0; i<ids.length; ++i)
        {
            let heroId = ids[i];
            //再次检查下是不是自己好友
            if(this.getFriendbyHeroId(heroId) == null)
                continue;
            //检查下对方是否真的有送过体力
            var mark = false;
            if(this._unCollStam.length > 0)
            {
                for(let i=0; i<this._unCollStam.length;++i)
                {
                    if(this._unCollStam[i]["heroId"] == heroId)
                    {
                        mark = true;
                        break;
                    }
                }
                if(mark == false)  //检查完还是没有这个人
                    continue;
            }
            else  //未领取表里都没有了
                continue;

            //检查下自己是否已经领过他送的体力
            if(this._collStam.length>0)
            {
                let len = this._collStam.length;
                if(len > 0)
                {
                    var getted = false;
                    for(let i=0; i<len; ++i)
                    {
                        if(this._collStam[i] == heroId)//已经领过他的体力了
                        {
                            getted = true;
                            break;
                        }
                    }
                    if(getted)
                        continue;
                }
            }
            arr.push(heroId);   //对方送过没领的才计数
        }
        if(arr.length == 0)
            return [];

        //领取体力
        let staValue = parseInt(valueConfig.getConfigValueConfig("friendStamina")["value"])
        this._role.addStamina(staValue * arr.length);

        //修改内存
        this._unCollStam = [];
        this._updateStamp = dateUtil.getTimestamp();
        this._collStam = this._collStam.concat(ids);

        //存盘的操作
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {"$addToSet":{"social.collStam":{"$each":arr}},
            "$pull":{"social.unCollStam":{"heroId":{"$in":arr}}}, "$set": {"social.updateStamp": this._updateStamp}});

        return arr;
    }

    /**
     * 自己属性修改时调用,检测自己的在线好友，修改自己的信息
     * @param {Object} props
     */
    onPropChange(props)
    {
        //遍历自己的在线好友
        for(let i=0;i<this._friends.length;i++)
        {
            /** @type {Friend}*/
            var f = this._friends[i];
            var role = onlineRoleMgr.findRoleByHeroId(f.heroId);
            if(role)//在线
            {
                //更新别人身上自己的属性
               role.getSocialPart().updateProp(this._role.getHeroId(), props);
            }
        }

        //更新公会里面的数据
        corpsMgr.updateProp(this._role.getNumber(enProp.corpsId), this._role.getHeroId(), props);
    }

    /**
     * 更新指定id的好友信息
     * @param {Number} friendId
     * @param {Objcet} props
     */
    updateProp(friendId, props)
    {
        /** @type {Friend}*/
        var fri = this._friendsMap[friendId];
        for(var key in props)
        {
            fri[key] = props[key];
        }
        fri.upTime = dateUtil.getTimestamp();  //修改时间，以便找到
    }

    /**
     * 重置赠送、领取次数
     */
    resetStaminaNums()
    {
        //重置的时候记得不要把未领取的都重置，可能包含今天别人刚送的，清除今天之前的，保留今天的。
        this._updateStamp = dateUtil.getTimestamp();
        /** @type {Number[]}*/
        var del = [];
        /** @type {Object}*/
        var remain = [];
        for(let i=0; i<this._unCollStam.length; i++)
        {
            if(!dateUtil.isToday(this._unCollStam[i]["timeStamp"]))  //不是今天的，清除。
                del.push(this._unCollStam[i].heroId);  //只存id
            else
                remain.push(this._unCollStam[i]);
        }
        //数据库清盘
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {$pull:{"social.sendStam":{"$in":this._sendStam}, "social.collStam":{"$in":this._collStam},
            "social.unCollStam":{"heroId":{"$in":del}}}, $set:{"social.updateStamp": this._updateStamp}});
        //$pull支持一次从多个表取出

        //更新内存
        this._sendStam = [];
        this._collStam = [];
        this._unCollStam = remain;  //更新这个表，保留今天别人送的体力
    }

    /**
     * 获取随机在线的推荐好友
     * @returns {Friend[]}
     */
    getRecommendFriend(reset)
    {
        if(!reset)
        {
            return this._recFriends;
        }
        var curTime = dateUtil.getTimestamp();
        this._recUpdateStamp = curTime;   //更新时间
        //存盘操作
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {"$set": {"social.recUpdateStamp": this._recUpdateStamp}});


        var roles = onlineRoleMgr.getOnlineRoles();
        //先筛选一遍，不是自己并且不是好友的才加进来
        this._recFriends = [];
        var mainId = this._role.getHeroId();  //自己的id
        for(var i = 0,len = roles.length; i < len; ++i)
        {
            var heroId = roles[i].getHeroId();
            if(heroId != mainId && this.getFriendbyHeroId(heroId) == null)
                this._recFriends.push(SocialPart.makeFriendDataFromRole(roles[i], curTime));
        }

        //对数组乱序
        for(var i = 0,len = this._recFriends.length; i < len; ++i)
        {
            var rand = appUtil.getRandom(0, len-1);
            var temp = this._recFriends[rand];
            this._recFriends[rand] = this._recFriends[i];
            this._recFriends[i] = temp;
        }
        if(this._recFriends.length <= 10)  //不足10个则全推荐了
            return this._recFriends;
        return this._recFriends.slice(0, 10);
    }

    //获取上次推荐好友的刷新时间
    getRecUptime()
    {
        return this._recUpdateStamp;
    }

    /**
     * 检查是否重置
     * @returns {boolean}
     */
    checkTimeReset()
    {
        if(!dateUtil.isToday(this._updateStamp))//不是今天 需要重置
        {
            this.resetStaminaNums();

            //推送新的数据告诉客户端更新
            var netMsg = new socialMessage.ReqFriendsRes(this._friends, this._collStam, this._unCollStam, this._sendStam, true);
            this._role.sendEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_REQ_FRIENDS, netMsg);

            return true;
        }
        return false;
    }

    /**
     * 判断自己的好友是否满了
     * @returns {boolean}
     */
    isFriendFull()
    {
        var level = this._role.getNumber(enProp.level);
        if(this._friends.length >= FriendMaxConfig.getFriendMaxCfg(level).maxFriend)
            return true;
        else
            return false;
    }

    /************************************静态方法***************************************************/

    /**
     * 从role数据创建好友结构
     * @param {Role} role
     * @param {Number} curTime
     * @returns {Friend}
     */
    static makeFriendDataFromRole(role, curTime)
    {
        /**
         * @type {Friend}
         */
        var data = {};
        data.heroId = role.getNumber(enProp.heroId);
        data.name = role.getString(enProp.name);
        data.level = role.getNumber(enProp.level);
        data.roleId = role.getString(enProp.roleId);
        data.powerTotal = role.getNumber(enProp.powerTotal);
        data.vipLv = role.getNumber(enProp.vipLv);
        data.lastLogout = role.getLastOfflineTime();
        data.upTime = curTime;
        return data;
    }


    /**
     * 添加好友存盘
     * @param {String} userId 自己的userId
     * @param {Number} heroId 自己的heroId
     * @param {Number} reqId 请求者的id
     */
    static addDBFriend(userId, heroId, reqId)
    {
        //存盘操作 从申请表去掉，加到好友表
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId}, {"$addToSet":{"social.friends":reqId}, "$pull":{"social.addReqs":reqId}});
        col.updateOneNoThrow({"props.heroId":reqId}, {"$addToSet":{"social.friends":heroId}, "$pull":{"social.addReqs":heroId}});
    }

    /**
     * 删除好友存盘
     * @param {String} userId 自己的userId
     * @param {Number} heroId 自己的heroId
     * @param {Number} deleteId 删除的id
     */
    static delDBFriend(userId, heroId, deleteId)
    {
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        //将对方从好友列表和未领体力列表删除
        col.updateOneNoThrow({"props.heroId":heroId}, {"$pull":{"social.friends":deleteId, "unCollStam":{"heroId":deleteId}}});
        //将自己也从对方的好友列表和未领列表中删除
        col.updateOneNoThrow({"props.heroId":deleteId}, {"$pull":{"social.friends":heroId, "unCollStam":{"heroId":heroId}}});

    }

    /**
     * 在B的申请表添加A的申请 存盘  （A申请加B为好友）
     * @param {String} userId B的userId
     * @param {Number} heroId B的heroId
     * @param {Number} reqId A的id
     */
    static addDBFriendReq(userId, heroId, reqId)
    {
        //加入数据库
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        var friendReqMax = parseInt(valueConfig.getConfigValueConfig("friendReqMax")["value"]);  //最大上限
        col.updateOneNoThrow({"props.heroId":heroId}, {$addToSet:{"social.addReqs":{$each:[reqId], $position:0, $slice:friendReqMax}}});//
    }
    /**
     * 批量在B、C、D、……的申请表添加A的申请 存盘  （A申请加B、C、D、……为好友）
     * @param {Number[]} heroIds B、C、D、……的heroId
     * @param {Number} reqId A的heroId
     */
    static batchAddDBFriendReq(heroIds, reqId)
    {
        //加入数据库
        var db = dbUtil.getDB(0);
        var col = db.collection("role");
        var friendReqMax = parseInt(valueConfig.getConfigValueConfig("friendReqMax")["value"]);  //最大上限
        col.updateManyNoThrow({"props.heroId":{$in:heroIds}}, {$addToSet:{"social.addReqs":{$each:[reqId], $position:0, $slice:friendReqMax}}});//
    }

    /**
     * 移除别人的申请存盘
     * @param {String} userId 自己的userId
     * @param {Number} heroId 自己的heroId
     * @param {Number[]} reqIds 请求者的id
     */
    static delDBFriendReq(userId, heroId, reqIds)
    {
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId}, {$pull:{"social.addReqs":{"$in":reqIds}}});
    }
}


exports.SocialPart = SocialPart;