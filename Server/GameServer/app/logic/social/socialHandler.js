"use strict";

var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var socialMessage = require("../netMessage/socialMessage");
var CmdIdsSocial = require("../netMessage/socialMessage").CmdIdsSocial;
var ResultCodeSocial = require("../netMessage/socialMessage").ResultCodeSocial;
var ReqFriendsReq = require("../netMessage/socialMessage").ReqFriendsReq;
var AddFriendReq = require("../netMessage/socialMessage").AddFriendReq;
var HandleFriendReq = require("../netMessage/socialMessage").HandleFriendReq;
var SendStaminaReq = require("../netMessage/socialMessage").SendStaminaReq;
var GetStaminaReq = require("../netMessage/socialMessage").GetStaminaReq;
var OnekeyGetStaminaReq = require("../netMessage/socialMessage").OnekeyGetStaminaReq;
var DeleteFriendReq = require("../netMessage/socialMessage").DeleteFriendReq;
var FriendRecommendReq = require("../netMessage/socialMessage").FriendRecommendReq;
var RefreshRecommendReq = require("../netMessage/socialMessage").RefreshRecommendReq;
var OneKeyAddFriendReq = require("../netMessage/socialMessage").OneKeyAddFriendReq;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var enProp = require("../enumType/propDefine").enProp;
var dbUtil = require("../../libs/dbUtil");
var onlineRoleMgr = require("../role/onlineRoleMgr");
var roleMgr = require("../role/roleMgr");
var robotMgr = require("../role/robotRoleMgr");
var Promise = require("bluebird");
var SocialPart = require("../social/socialPart");
var HandlerTypeEnum = require("../netMessage/socialMessage").HandlerTypeEnum;
var valueConfig = require("../gameConfig/valueConfig");
var dateUtil = require("../../libs/dateUtil");
var FriendMaxConfig = require("../gameConfig/friendMaxConfig");

/**
 * 打开好友请求数据，这里只下发uptime有变化的数据
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ReqFriendsReq} reqObj
 */
function reqFriend(session, role, msgObj, reqObj)
{
    let socialPart = role.getSocialPart();
    //先检查时间。看是否需要重置
    if(socialPart.checkTimeReset())
        return ResultCodeSocial.RESET_STAMINA_TIMES;

    var friends = socialPart.collectUpdateFriend();
    return new socialMessage.ReqFriendsRes(friends, [], [], [], false);

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_REQ_FRIENDS, reqFriend, ReqFriendsReq);


//根据name从数据库读取数据
var loadFriendRoles = Promise.coroutine(
    /**
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {AddFriendReq} reqObj
     */
    function * (session, role, msgObj, reqObj)
    {
        var mainUesrId = role.getUserId();
        var mainHeroId = role.getHeroId();
        var db = dbUtil.getDB(mainUesrId);
        var col = db.collection("role");
        //通过要添加的reqObj.addName从数据库中找数据
        var info = yield col.findOne({"props.name":reqObj.addName},
            {
                "props.heroId":1,
                "props.userId":1
            });
        if(info == null)//数据库找不到
        {
            msgObj.setResponseData(ResultCodeSocial.FRIEND_NOT_FOUND_ERROR);//找不到指定名字的角色
            role.send(msgObj);
            return;
        }

        //判断是否已经是好友了
        if(role.getSocialPart().getFriendbyHeroId(info.props.heroId))
        {
            msgObj.setResponseData(ResultCodeSocial.IS_FRIEND);
            role.send(msgObj);
        }
        //存盘
        SocialPart.SocialPart.addDBFriendReq(role.getUserId(), info.props.heroId, reqObj.adder.heroId);

        var netMsg = new socialMessage.AddFriendRes(reqObj.addName);
        msgObj.setResponseData(ResultCode.SUCCESS, netMsg);
        role.send(msgObj);
    }

);

//根据name从数据库读取数据
var loadFriendRolesBatch = Promise.coroutine(
    /**
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {Friend} adder
     * @param {string[]} names
     */
    function * (session, role, msgObj, adder, names)
    {
        var mainUesrId = role.getUserId();
        var mainHeroId = role.getHeroId();
        var db = dbUtil.getDB(mainUesrId);
        var col = db.collection("role");
        //通过要添加的reqObj.addName从数据库中找数据
        var infos = yield col.findArray({"props.name":{$in:names}},
            {
                "props.heroId":1
            });
        if(infos == null || infos.length == 0)//数据库找不到
            return;

        var arr = [];
        var part = role.getSocialPart();
        for(var i = 0,len = infos.length; i < len; ++i)
        {
            //判断是否已经是好友了
            var f = part.getFriendbyHeroId(infos[i].props.heroId);
            if(f != null)
                continue;
            arr.push(infos[i].props.heroId);
        }
        if(arr.length > 0)  //批量存盘
            SocialPart.SocialPart.batchAddDBFriendReq(arr, adder.heroId);
    }

);

/**
 * 请求添加好友（名字）
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {AddFriendReq} reqObj
 */
function reqAddFriend(session, role, msgObj, reqObj)
{
    //先判断是否为空
    if(!reqObj.addName || reqObj.addName == "")
    {
        msgObj.setResponseData(ResultCodeSocial.FRIEND_NOT_FOUND_ERROR);
        role.send(msgObj);
        return;
    }
    //不能加自己好友
    if(reqObj.addName == role.getString(enProp.name))
    {
        msgObj.setResponseData(ResultCodeSocial.ADD_SELF_ERROR);
        role.send(msgObj);
        return;
    }
    //先判断自己的好友是否满了
    if(role.getSocialPart().isFriendFull())
    {
        msgObj.setResponseData(ResultCodeSocial.FRIEND_IS_FULL);
        role.send(msgObj);
        return;
    }
    //判断是不是机器人，是的话直接告诉玩家说申请成功了
    var aRole = robotMgr.findRoleByName(reqObj.addName);
    if(aRole)
    {
        var netMsg = new socialMessage.AddFriendRes(reqObj.addName);
        msgObj.setResponseData(ResultCode.SUCCESS, netMsg);
        role.send(msgObj);
        return;
    }

    //这里要先判断一下有没有B这个玩家(先在内存找，再找数据库)，因为取的申请列表数据比较多，所以用$addToSet往对方申请列表里面直接加(重复的不会添加)
    //同时对B玩家推送一个添加的主动请求
    var fRole = roleMgr.findRoleByHeroName(reqObj.addName);
    if(fRole)
    {
        let _socialPart = fRole.getSocialPart();
        //判断是否已经是好友了
        if(role.getSocialPart().getFriendbyHeroId(fRole.getHeroId()))
        {
            msgObj.setResponseData(ResultCodeSocial.IS_FRIEND);
            role.send(msgObj);
        }

        if(_socialPart)
            _socialPart.addFriendReq(fRole, reqObj.adder);
        //存盘
        SocialPart.SocialPart.addDBFriendReq(fRole.getUserId(), fRole.getHeroId(), reqObj.adder.heroId);

        var netMsg = new socialMessage.AddFriendRes(reqObj.addName);
        msgObj.setResponseData(ResultCode.SUCCESS, netMsg);
        role.send(msgObj);
    }
    else
    {
        loadFriendRoles(session, role, msgObj, reqObj);
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_ADD_FRIEND, reqAddFriend, AddFriendReq);

//处理添加好友
var HandleAddFriendCorout = Promise.coroutine(
    /**
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {HandleFriendReq} reqObj
     */
    function * (session, role, msgObj, reqObj)
    {
        //要先判断一下对方的好友满了没，满了则无法添加
        var frole = roleMgr.findRoleByHeroId(reqObj.heroId);
        if(frole)
        {
            if(frole.getSocialPart().isFriendFull())//对方好友已满
            {
                msgObj.setResponseData(ResultCodeSocial.OTHER_FRIEND_IS_FULL);
                role.send(msgObj);
                return;
            }
        }
        else
        {
            var mainUesrId = role.getUserId();
            var db = dbUtil.getDB(mainUesrId);
            var col = db.collection("role");
            var infos = yield col.findOne({"props.heroId":reqObj.heroId},
                {
                    "props.level":1,
                    "social.friends":1   //把他的好友也读出来
                });
            if(infos == null)//数据库找不到
            {
                msgObj.setResponseData(ResultCode.SERVER_ERROR);
                role.send(msgObj);
                return;
            }
            if(infos.social.friends != null && infos.social.friends.length >= FriendMaxConfig.getFriendMaxCfg(infos.props.level).maxFriend)  //对方好友已满
            {
                msgObj.setResponseData(ResultCodeSocial.OTHER_FRIEND_IS_FULL);
                role.send(msgObj);
                return;
            }
        }

        //A同意了B的好友请求，(先判断A的好友数目是否未满)更新数据库：把B加到A的好友列表，同时将申请列表里的A移除掉;把A加到B的好友列表，同时要检查A的申请列表里是否有B，有就移除掉
        //更新A的内存，通知A的客户端； 如果B在线，通知B
        var socialPart = role.getSocialPart();
        var friend = socialPart.addFriend(reqObj.heroId);
        if(friend)
        {
            //如果对方在线，要通知对方
            if(frole)
            {
                //更新内存
                var p = frole.getSocialPart();
                //封装自己的好友数据 推给对方
                /** @type {Friend} */
                var fData = SocialPart.SocialPart.makeFriendDataFromRole(role);
                p.pushFriend(fData);
                //通知对方客户端
                var netMsg = new socialMessage.HandleFriendRes(HandlerTypeEnum.BeAgreed, fData);
                frole.sendEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_HANDLE_FRIEND, netMsg);

            }
            var res = new socialMessage.HandleFriendRes(HandlerTypeEnum.Agree, friend);   //返回通知添加成功
            msgObj.setResponseData(ResultCode.SUCCESS, res);
            role.send(msgObj);
            return;
        }
        else  //自己好友已满
        {
            msgObj.setResponseData(ResultCodeSocial.FRIEND_IS_FULL);
            role.send(msgObj);
            return;
        }
    }
);
/**
 * 处理好友请求
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {HandleFriendReq} reqObj
 */
function reqHandlerFriend(session, role, msgObj, reqObj)
{
    var socialPart = role.getSocialPart();
    switch (reqObj.type)
    {
        case HandlerTypeEnum.Agree:    //同意申请
            if(reqObj.heroId == role.getHeroId())//不能添加自己为好友
            {
                msgObj.setResponseData(ResultCodeSocial.ADD_SELF_ERROR);
                role.send(msgObj);
                return;
            }
            HandleAddFriendCorout(session, role, msgObj, reqObj);
            break;

        case HandlerTypeEnum.Refuse:   //拒绝申请
            let f = socialPart.delFriendReq(reqObj.heroId);

            //如果对方在线，要通知对方
            let friRole = onlineRoleMgr.findRoleByHeroId(reqObj.heroId);
            if(friRole)
            {
                var fData = SocialPart.SocialPart.makeFriendDataFromRole(role);
                //通知对方客户端
                var netMsg = new socialMessage.HandleFriendRes(HandlerTypeEnum.BeRefused, fData);
                friRole.sendEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_HANDLE_FRIEND, netMsg);
            }
            var res = new socialMessage.HandleFriendRes(HandlerTypeEnum.Refuse, f);   //返回通知拒绝成功
            msgObj.setResponseData(ResultCode.SUCCESS, res);
            role.send(msgObj);
            break;

        case HandlerTypeEnum.ResuseAll:      //一键清空拒绝
            socialPart.delAllFriendReq();
            var res = new socialMessage.HandleFriendRes(HandlerTypeEnum.ResuseAll, null);   //返回通知拒绝成功
            msgObj.setResponseData(ResultCode.SUCCESS, res);
            role.send(msgObj);
            break;
        default:
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
            role.send(msgObj);
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_HANDLE_FRIEND, reqHandlerFriend, HandleFriendReq);

/**
 * 赠送好友体力
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {SendStaminaReq} reqObj
 */
function sendStamina(session, role, msgObj, reqObj)
{
    let socialPart = role.getSocialPart();
    //先检查时间。看是否需要重置
    if(socialPart.checkTimeReset())
        return ResultCodeSocial.RESET_STAMINA_TIMES;

    let result = socialPart.sendStaminaReq(reqObj.heroId);
    if(result == 0)
        return new socialMessage.SendStaminaRes(reqObj.heroId);//赠送成功
    else if(result == -1)
        return ResultCodeSocial.SEND_STAMINA_TIMEOUT;   //超出今天最大赠送上限
    else if(result == -2)
        return ResultCodeSocial.SEND_STAMINA_ERROR;  //今天已经赠送过该好友体力
    else if(result == -3)
        return ResultCodeSocial.IS_NOT_FRIEND  //对方不是自己好友
    else
        return ResultCode.SERVER_ERROR;


}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_SEND_STAMINA, sendStamina, SendStaminaReq);

/**
 * 领取好友体力
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {GetStaminaReq} reqObj
 */
function getStamina(session, role, msgObj, reqObj)
{
    let socialPart = role.getSocialPart();
    //先检查时间。看是否需要重置
    if(socialPart.checkTimeReset())
        return ResultCodeSocial.RESET_STAMINA_TIMES;

    let result = socialPart.getStaminaReq(reqObj.heroId);
    if(result == 0)
        return new socialMessage.GetStaminaRes(reqObj.heroId);//领取成功
    else if(result == -1)
        return ResultCodeSocial.GET_STAMINA_TIMEOUT;   //次数用完
    else if(result == -2)
        return ResultCodeSocial.GET_STAMINA_ERROR;   //今天已领过
    else if(result == -3)
        return ResultCodeSocial.IS_NOT_FRIEND  //对方不是自己好友
    else
        return ResultCode.SERVER_ERROR;

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_GET_STAMINA, getStamina, GetStaminaReq);

/**
 * 一键领取好友体力
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {OnekeyGetStaminaReq} reqObj
 */
function onekeyGetStamina(session, role, msgObj, reqObj)
{
    let part = role.getSocialPart();
    //先检查时间。看是否需要重置
    if(part.checkTimeReset())
        return ResultCodeSocial.RESET_STAMINA_TIMES;

    var arr = part.onekeyGetStaminaReq(reqObj.heroIds);
    if(arr.length > 0)
        return new socialMessage.OnekeyGetStaminaRes(arr);  //领取成功
    else
        return ResultCodeSocial.GET_STAMINA_TIMEOUT;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_ONEKEY_STAMINA, onekeyGetStamina, OnekeyGetStaminaReq);

/**
 * 删除好友
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {DeleteFriendReq} reqObj
 */
function deleteFriendHandler(session, role, msgObj, reqObj)
{
    let part = role.getSocialPart();
    var res = part.deleteFriend(reqObj.heroId);
    if(res)
        return new socialMessage.DeleteFriendRes(reqObj.heroId);
    else
        return ResultCodeSocial.IS_NOT_FRIEND;  //不是好友，返回错误
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_DELETE_FRIEND, deleteFriendHandler, DeleteFriendReq);

/**
 * 推荐好友
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {FriendRecommendReq} reqObj
 */
function friendRecHandler(session, role, msgObj, reqObj)
{
    var part = role.getSocialPart();
    var time = part.getRecUptime();
    if(dateUtil.getTimestamp() - time < valueConfig.getConfigValueConfig("friendRecRefresh")["value"])  //刷新时间未到
    {
        if(reqObj.isFirst)  //第一次请求
        {
            var arr = part.getRecommendFriend(false);
            time = part.getRecUptime();
            return new socialMessage.FriendRecommendRes(arr, true, time);
        }
        else
            return new socialMessage.FriendRecommendRes([], false, time);
    }
    else
    {
        if(reqObj.isFirst)  //第一次请求
            var arr = part.getRecommendFriend(true);
        else
            var arr = part.getRecommendFriend(false);
        time = part.getRecUptime();
        return new socialMessage.FriendRecommendRes(arr, true, time);
    }
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_FRIEND_RECOMMEND, friendRecHandler, FriendRecommendReq);

/**
 * 刷新推荐好友
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {RefreshRecommendReq} reqObj
 */
function refreshRecommend(session, role, msgObj, reqObj)
{
    var part = role.getSocialPart();
    if(dateUtil.getTimestamp() - part.getRecUptime() < valueConfig.getConfigValueConfig("friendRecRefresh")["value"])
        return ResultCodeSocial.RECOM_REFR_TIME_CD;
    var arr = part.getRecommendFriend(true);
    var time = part.getRecUptime();
    return new socialMessage.RefreshRecommendRes(arr, time);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_REFRESH_RECOMMEND, refreshRecommend, RefreshRecommendReq);

/**
 * 一键添加好友返回（添加名字）
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {OneKeyAddFriendReq} reqObj
 */
function oneKeyAddFriend(session, role, msgObj, reqObj)
{
    var part = role.getSocialPart();

    if(reqObj.addNames.length == 0)
        return new socialMessage.OneKeyAddFriendRes();
    //先判断自己的好友是否满了
    if(role.getSocialPart().isFriendFull())
    {
        msgObj.setResponseData(ResultCodeSocial.FRIEND_IS_FULL);
        role.send(msgObj);
    }

    var onIdArr = [];    //用来存在线的id
    var offNameArr = [];    //用来存不在线的name
    for(var i = 0,len = reqObj.addNames.length; i < len; ++i)
    {
        //先判断是否为空
        if(!reqObj.addNames[i] || reqObj.addNames[i] == "")
            continue;
        //不能加自己好友
        if(reqObj.addNames[i] == role.getString(enProp.name))
            continue;
        var fRole = roleMgr.findRoleByHeroName(reqObj.addNames[i]);
        if(fRole)   //在线
        {
            let _socialPart = fRole.getSocialPart();
            //判断是否已经是好友了
            if(part.getFriendbyHeroId(fRole.getHeroId()))
                continue;
            _socialPart.addFriendReq(fRole, reqObj.adder);
            //先存进数组，最后再批量存盘
            onIdArr.push(fRole.getHeroId());
        }
        else
            offNameArr.push(reqObj.addNames[i]);

    }
    //这里批量存盘 对在线的添加
    SocialPart.SocialPart.batchAddDBFriendReq(onIdArr, reqObj.adder.heroId);
    //开启协程读数据库
    loadFriendRolesBatch(session, role, msgObj, reqObj.adder, offNameArr);

    //返回通知一下，虽然上面有协程需要去服务器数据的可能会慢一点，但现在返回通知不影响顺序。
    return new socialMessage.OneKeyAddFriendRes(reqObj.addNames);

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SOCIAL, CmdIdsSocial.CMD_ONEKEY_ADD_FRIEND, oneKeyAddFriend, OneKeyAddFriendReq);