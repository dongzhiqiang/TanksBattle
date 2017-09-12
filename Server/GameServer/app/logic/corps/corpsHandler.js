"use strict";

var Promise = require("bluebird");
var dbUtil = require("../../libs/dbUtil");
var logUtil = require("../../libs/logUtil");
var handlerMgr = require("../session/handlerMgr");
var globalServerAgent = require("../http/globalServerAgent");
var appCfg = require("../../../config");
var dateUtil = require("../../libs/dateUtil");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var ResultCode = require("../netMessage/netMsgConst").ResultCode;
var corpsMessage = require("../netMessage/corpsMessage");
var CmdIdsCorps = require("../netMessage/corpsMessage").CmdIdsCorps;
var corpsMgr = require("../corps/corpsMgr");
var ResultCodeCorps = require("../netMessage/corpsMessage").ResultCodeCorps;
var CorpsConfig =  require("../gameConfig/corpsConfig");
var CorpsPosFunc = require("../netMessage/corpsMessage").CorpsPosFunc;
var enProp = require("../enumType/propDefine").enProp;
var CorpsPosEnum = require("../netMessage/corpsMessage").CorpsPosEnum;
var onlineRoleMgr = require("../role/onlineRoleMgr");
var enItemId = require("../enumType/globalDefine").enItemId;

const CORPS_NAME_MAX_LEN = 12;   //公会名限制长度

//创建公会
var createCorpsCoroutine = Promise.coroutine(
    /**
     * @param {ClientSession} session - 由于role是空的，所以使用session回复消息
     * @param {Role} role - 在这里role是空的，还没有role
     * @param {Message} msgObj
     * @param {CreateCorpsReq} createReq
     */
    function * (session, role, msgObj, createReq)
    {
        var name = createReq.name;
        var level = 1;
        var serverId = appCfg.serverId;

        //检查达到公会开放等级了没
        if(role.getNumber(enProp.level) < CorpsConfig.getCorpsBaseCfg().openLevel)
        {
            msgObj.setResponseData(ResultCodeCorps.NOT_REACH_LIMIT);
            role.send(msgObj);
            return;
        }

        //检查下是否已经有公会了
        if(role.getNumber(enProp.corpsId))
        {
            msgObj.setResponseData(ResultCodeCorps.HAS_CORPS_ALREADY);
            role.send(msgObj);
            return;
        }
        //检查一下自己过了退会冷却时间了没有
        var quitTime = role.getCorpsPart().getQuitCorpsTime();
        if(quitTime > 0 && dateUtil.getTimestamp() - quitTime <= CorpsConfig.getCorpsBaseCfg().quitCorpsCd)
        {
            msgObj.setResponseData(ResultCodeCorps.QUIT_CORPS_CD); //退会冷却cd中
            role.send(msgObj);
            return;
        }
        role.getCorpsPart().setQuitCorpsTime(0);   //冷却时间已过的就可以重置为0了

        //检查下钻石是否足够
        if(role.getNumber(enProp.diamond) < CorpsConfig.getCorpsBaseCfg().createCost)
        {
            msgObj.setResponseData(ResultCode.DIAMOND_INSUFFICIENT);
            role.send(msgObj);
            return;
        }

        //检查下创建的公会名字是否空
        if(!name || name == "")
        {
            msgObj.setResponseData(ResultCodeCorps.NAME_IS_EMPTY);
            role.send(msgObj);
            return;
        }
        //公会名字超出长度
        if (countStrLength(name.length) > CORPS_NAME_MAX_LEN)
        {
            msgObj.setResponseData(ResultCodeCorps.NAME_TOO_LONG);
            role.send(msgObj);
            return;
        }
        var reqRes = yield globalServerAgent.requestNewCorps(name, serverId, level);
        //请求失败？
        if (!reqRes.ok)
        {
            msgObj.setResponseWithMsg(ResultCode.SERVER_ERROR, reqRes.msg);
            role.send(msgObj);
            return;
        }
        //返回的数据不对
        if (!reqRes.cxt || !Object.isNumber(reqRes.cxt.corpsId))
        {
            logUtil.error("cropsHandler~createCrops，得到id为空");
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
            role.send(msgObj);
            return;
        }
        //随机宣言
        var random = Math.ceil(Math.random()*4);
        var declare = CorpsConfig.getCorpsDecalreCfg(random).declare;

        //插入新公会数据
        var db = dbUtil.getDB(0);
        var col = db.collection("corps");
        //构造初始数据
        var curTime = dateUtil.getTimestamp();
        var corpsId = reqRes.cxt.corpsId;
        var corpsData = {
            //公会属性
            props: {
                corpsId: corpsId,
                serverId: serverId,
                createTime: curTime,
                name: name,
                president: role.getString(enProp.name),
                level:level,
                growValue: 0,
                declare: declare,
                rank: 1,   //排名 需要修改
                joinSet: 0,   //入会设置 0则为无限制，1需要申请
                joinSetLevel: CorpsConfig.getCorpsBaseCfg().openLevel,   //入会等级设置
                memsNum: 1,   //人数
                buildUptime: 0,   //记录任何人最近一次参与公会建设的时间
            },
            //成员 设置自己为公会会长
            members: [
                {
                    "heroId":role.getNumber(enProp.heroId),
                    "pos":1,
                    "contribution":0,
                }
            ],
            //申请
            reqs: [],
            //日志
            logs: [
                {
                    "id":6,
                    "opt":role.getString(enProp.name),
                    "time":dateUtil.getTimestamp()
                },
                {
                    "id":9,
                    "opt":role.getString(enProp.name),
                    "time":dateUtil.getTimestamp()
                }
            ],
            //建设记录
            buildLogs: [],
            //三种建设已建设的人
            hasBuild:[[],[],[]],
            //今日建设过的人
            buildIds: []

        };

        //执行插入操作
        var dbRes = yield col.insertOne(corpsData);
        //再检测一下
        if (dbRes <= 0)
        {
            msgObj.setResponseData(ResultCode.DB_ERROR);
            role.send(msgObj);
            return;
        }
        //扣钻石
        var itemsPart = role.getItemsPart();
        itemsPart.costItem(enItemId.DIAMOND,CorpsConfig.getCorpsBaseCfg().createCost);

        logUtil.debug("创建公会成功，corpsId：" + corpsId + "，serverId：" + serverId +  "，name：" + name);
        role.setNumber(enProp.corpsId, corpsId);
        role.setString(enProp.corpsName, corpsData.props.name);

        //管理器内存添加
        corpsMgr.createCorps(corpsId, corpsData, role);

        //下发消息
        let retObj = new corpsMessage.CorpsDataRes(corpsData.props,corpsData.logs,true);
        msgObj.setResponseData(ResultCode.SUCCESS, retObj);
        role.send(msgObj);
    }
);


//解散销毁公会
var destroyCorpsCoroutine = Promise.coroutine(
    /**
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {ExitCorpsReq} req
     */
    function * (session, role, msgObj, req) {

        var reqRes = yield globalServerAgent.requestDelCorps(req.corpsId);
        //请求失败？
        if (!reqRes.ok)
        {
            msgObj.setResponseWithMsg(ResultCode.SERVER_ERROR, reqRes.msg);
            role.send(msgObj);
            return;
        }

        //返回的数据不对
        if (!reqRes.cxt)
        {
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
            role.send(msgObj);
            return;
        }

        var db = dbUtil.getDB(0);
        var col = db.collection("corps");
        //执行删除操作
        yield col.deleteOne({"props.corpsId": req.corpsId});

        logUtil.debug("删除公会成功，corpsId：" + req.corpsId);
        role.setNumber(enProp.corpsId, 0);
        role.setString(enProp.corpsName, "");
        //下发通知
        var resObj = new corpsMessage.ExitCorpsRes(1);
        msgObj.setResponseData(ResultCode.SUCCESS, resObj);
        role.send(msgObj);

    }
);
//请求数据库，读出指定id的角色基础数据
var loadRolesFromDB = Promise.coroutine(
    /**
     * @param {Number} corpsId
     * @param {Number} heroId
     */
    function * (corpsId, heroId) {
        try{
            var db = dbUtil.getDB(0);
            var col = db.collection("role");
            //一次性从数据库读数据
            var infos = yield col.findArray({"props.heroId":{$in:[heroId]}},{
                "props.heroId":1,
                "props.name":1,
                "props.level":1,
                "props.roleId":1,
                "props.powerTotal":1,
                "props.lastLogout":1  //离线时间
            });

            var curTime = dateUtil.getTimestamp();
            var props = infos[0].props;
            informNewMemsReqs(corpsId, props, true);
        }
        catch(err){
            throw err;
        }
    }
);

/**
 * 计算字符串长度
 * @param str
 * @returns {number}
 */
function countStrLength(str)
{
    var len = 0;
    for (var i=0; i<str.length; i++)
    {
        if (str.charCodeAt(i) > 127 || str.charCodeAt(i) == 94)
            len += 2;
        else
            len ++;
    }
    return len;
}


/**
 * 创建公会请求
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {CreateCorpsReq} reqObj
 */
function reqCreateCorps(session, role, msgObj, reqObj)
{
    createCorpsCoroutine(session, role, msgObj, reqObj).catch (function (err){
        logUtil.error("创建公会失败~corpsHandler~createCorps", err);
        if (err instanceof dbUtil.MongoError)
            msgObj.setResponseData(ResultCode.DB_ERROR);
        else
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
        role.send(msgObj);
    });

}
handlerMgr.registerHandler(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_CREATE_CORPS, reqCreateCorps, corpsMessage.CreateCorpsReq);

/**
 * 请求公会数据
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {CorpsDataReq} reqObj
 */
function reqCorpsData(session, role, msgObj, reqObj)
{
    /**
     * @type {CorpsInfo}
     */
    var corpsData = corpsMgr.getCorpsData(reqObj.corpsId);  //取得对应id的公会所有数据
    if(!corpsData)//公会不存在
    {
        msgObj.setResponseData(ResultCodeCorps.CORPS_NOT_EXIST);
        role.send(msgObj);
    }

    if(reqObj.isFirst)
        var resObj = new corpsMessage.CorpsDataRes(corpsData.props, corpsData.logs, reqObj.isFirst);
    else
        var resObj = new corpsMessage.CorpsDataRes(corpsData.props, [], reqObj.isFirst);
    msgObj.setResponseData(ResultCode.SUCCESS, resObj);
    role.send(msgObj);
}
handlerMgr.registerHandler(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_REQ_CORPS, reqCorpsData, corpsMessage.CorpsDataReq);

/**
 * 请求修改公会宣言
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ModifyCorpsDeclareReq} reqObj
 */
function reqModifyDeclare(session, role, msgObj, reqObj)
{
    //长度检查一下
    if(countStrLength(reqObj.declare.length) > CorpsConfig.getCorpsBaseCfg().declareLimit)
        return ResultCodeCorps.DECLARE_TOO_LONG;

    //同时检测：1.是否有这个公会、2.修改者是不是这个公会的
    var m = corpsMgr.findCorpsMember(reqObj.corpsId, reqObj.heroId);
    if(m == null)  //找不到该成员，说明不是这个公会的
        return ResultCodeCorps.IS_NOT_MENMBER;  //不是本公会会员

    //判断修改者职位有没有这个权限
    if(!CorpsConfig.checkHasFunc(m.pos, Number(CorpsPosFunc.Modify)))
        return ResultCodeCorps.CORPS_PERMISSION_DENIED;   //公会职能权限不足

    //修改宣言
    corpsMgr.modifyDeclare(reqObj.corpsId, reqObj.heroId, reqObj.declare);
    return new corpsMessage.ModifyCorpsDeclareRes(reqObj.declare);

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_MODIFYDECLARE, reqModifyDeclare, corpsMessage.ModifyCorpsDeclareReq);

/**
 * 申请加入公会
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ApplyJoinCorpsReq} reqObj
 */
function reqJoinCorps(session, role, msgObj, reqObj)
{
    if(role.getNumber(enProp.corpsId) > 0)
        return ResultCodeCorps.HAS_CORPS_ALREADY;   //自己已有公会了
    var corpsData = corpsMgr.getCorpsData(reqObj.corpsId);
    if(corpsData == null)
        return ResultCodeCorps.CORPS_NOT_EXIST;  //公会不存在
    //检查一下自己过了退会冷却时间了没有
    var quitTime = role.getCorpsPart().getQuitCorpsTime();
    if(quitTime > 0 && dateUtil.getTimestamp() - quitTime <= CorpsConfig.getCorpsBaseCfg().quitCorpsCd)
        return ResultCodeCorps.QUIT_CORPS_CD;  //退会冷却cd中
    role.getCorpsPart().setQuitCorpsTime(0);   //冷却时间已过的就可以重置为0了
    if(corpsData.members.length >= CorpsConfig.getCorpsLevelCfg(corpsData.props.level).maxMember)
        return ResultCodeCorps.MEMBERS_FULL;  //会员已满
    if(corpsData.reqs.length >= CorpsConfig.getCorpsBaseCfg().maxReq)
        return ResultCodeCorps.REQ_FULL;  //请求已达到上限

    if(role.getNumber(enProp.level) < corpsData.props.joinSet)  //达不到入会等级限制
        return ResultCodeCorps.NOT_REACH_LIMIT;

    if(corpsData.props.joinSet == 0)//入会无需申请
    {
        corpsMgr.joinCorpsById(reqObj.corpsId, role.getHeroId(), role.getString(enProp.name), true);
        //修改属性
        role.setNumber(enProp.corpsId, reqObj.corpsId);
        role.setString(enProp.corpsName, corpsData.props.name);

        //通知公会里的在线的人 自己的信息
        informNewMemsReqs(reqObj.corpsId, role, true);

        //通知客户端
        //成员数据会在加入公会后打开面板时请求，这里就不发了
        return new corpsMessage.CorpsJoinExitRes(corpsData.props, 1, 0);
    }
    else  //需要申请
    {
        //写入数据库，加入申请中
        var db = dbUtil.getDB(role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":reqObj.heroId}, {"$addToSet":{"corps.reqCorps":reqObj.corpsId}});  //入会申请过的公会id都存在social.reqCorps

        //通知公会里的在线的人 自己的申请
        informNewMemsReqs(reqObj.corpsId, role, false);

        //处理申请 并下发客户端
        if(corpsMgr.addReqById(reqObj.corpsId, role))
            return new corpsMessage.CorpsJoinExitRes(null, 0, reqObj.corpsId);
        else
            return new corpsMessage.CorpsJoinExitRes(null, 1, reqObj.corpsId);
    }

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_APPLY_JOIN, reqJoinCorps, corpsMessage.ApplyJoinCorpsReq);

/**
 * 通知公会在线角色 新的会员信息或申请信息
 * @param {Number} corpsId
 * @param {Role} role
 * @param {boolean} isMember
 */
function informNewMemsReqs(corpsId, role, isMember)
{
    var corpsData = corpsMgr.getCorpsData(corpsId);
    var mData = corpsMgr.makeMemberDataFromRole(role, dateUtil.getTimestamp(), 0, isMember?3:0);
    //检查一下该公会在线的人，下发通知
    for(var i = 0, len = corpsData.members.length; i < len; ++i)
    {
        var olRole = onlineRoleMgr.findRoleByHeroId(corpsData.members[i].heroId);
        if(olRole)
        {
            //在线就下发信息通知他
            var netMsg = new corpsMessage.PushNewMemsReqDataRes(mData);
            olRole.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.PUSH_CORPS_MEMBER_REQ, netMsg);
        }
    }
}
//同意对方加入公会的逻辑检测、执行
var checkCorpsReqs = Promise.coroutine(
    /**
     * @param {ClientSession} session
     * @param {Role} role
     * @param {Message} msgObj
     * @param {HandleMemberReq} reqObj
     * @returns {*}
     */
    function * (session, role, msgObj, reqObj) {
        var heroId = reqObj.beHandler;
        var db = dbUtil.getDB();
        var col = db.collection("role");
        //从数据库中找到申请过公会id的数据  顺便把公会id找出来
        var info = yield col.findOne({"props.heroId": heroId},
            {
                "props.corpsId": 1,
                "corps.reqCorps": 1
            });
        if(info == null)
        {
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
            role.send(msgObj);
            return;
        }

        if (info.props.corpsId != null && info.props.corpsId > 0)  //说明他已经有公会了
        {
            //顺便更新内存，存盘操作
            corpsMgr.removeCorpsReq(reqObj.corpsId, reqObj.beHandler, true);

            var resObj = new corpsMessage.HandleMemberRes(reqObj.type, reqObj.beHandler, reqObj.beHandlerName, -1);
            msgObj.setResponseData(ResultCode.SUCCESS, resObj);
            role.send(msgObj);
            return;
        }

        if(corpsMgr.joinCorpsById(reqObj.corpsId, reqObj.beHandler, reqObj.beHandlerName, false) == false)
        {
            msgObj.setResponseData(ResultCodeCorps.HANDLE_CORPS_ERROR);  //操作错误
            role.send(msgObj);
            return;
        }
        //将这个人申请其他公会的请求都移除
        if (info.corps.reqCorps != null && info.corps.reqCorps.length > 0)//有记录
        {
            //把找到的公会id从对应的公会中去掉这个人的申请，批量修改数据库数据
            var db = dbUtil.getDB(0);
            var col = db.collection("corps");
            col.updateManyNoThrow({"props.corpsId":{$in:info.corps.reqCorps}}, {$pull:{"reqs":heroId}});
            //修改自己的数据
            var col2 = db.collection("role");
            col2.updateOneNoThrow({"props.heroId":heroId}, {"$set": {"corps.reqCorps": []}});
            //修改内存
            corpsMgr.removeRoleReq(heroId, info.corps.reqCorps);
        }

        var r = onlineRoleMgr.findRoleByHeroId(reqObj.beHandler);
        //如果对方在线，给对方下发数据通知
        if(r)
        {
            informNewMemsReqs(reqObj.corpsId, r, true);//通知所有人

            //通知对方客户端  成员数据会在加入公会后打开面板时请求，这里就不发了
            var corpsData = corpsMgr.getCorpsData(reqObj.corpsId);
            var netMsg = new corpsMessage.CorpsJoinExitRes(corpsData.props, 1, 0);
            r.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_APPLY_JOIN, netMsg);

            r.setNumber(enProp.corpsId, reqObj.corpsId);//修改属性
            r.setString(enProp.corpsName, corpsData.props.name);
        }
        else
        {
            //请求数据库
            loadRolesFromDB(reqObj.corpsId, reqObj.beHandler);
            //对方的属性修改存盘
            var db = dbUtil.getDB(role.getUserId());
            var col = db.collection("role");
            col.updateOneNoThrow({"props.heroId":reqObj.beHandler}, {"$set":{"props.corpsId":reqObj.corpsId}});
        }

        //返回通知自己的客户端 只通知处理情况，完成beHandler从申请表移除
        var resObj = new corpsMessage.HandleMemberRes(reqObj.type, reqObj.beHandler, reqObj.beHandlerName, 0);
        msgObj.setResponseData(ResultCode.SUCCESS, resObj);
        role.send(msgObj);
    }
);

/**
 * 处理会员操作
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {HandleMemberReq} reqObj
 */
function reqHandleMember(session, role, msgObj, reqObj)
{
    //先检查操作人有没有这个权限
    //同时检测：1.是否有这个公会、2.操作人是不是这个公会的
    var m = corpsMgr.findCorpsMember(reqObj.corpsId, reqObj.handler);
    if(m == null)  //找不到该成员，说明不是这个公会的
    {
        msgObj.setResponseData(ResultCodeCorps.IS_NOT_MENMBER);  //不是本公会会员
        role.send(msgObj);
        return;
    }
    if(!CorpsConfig.checkHasFunc(m.pos, Number(CorpsPosFunc.HandleReq)))   //“同意拒绝”和“踢出会员”应该是一样的
    {
        msgObj.setResponseData(ResultCodeCorps.CORPS_PERMISSION_DENIED); //公会职能权限不足
        role.send(msgObj);
        return;
    }

    switch (reqObj.type)
    {
        case 1:  //同意入会
            //检查公会会员是否已满
            var corpsData = corpsMgr.getCorpsData(reqObj.corpsId);
            if(corpsData.members.length >= CorpsConfig.getCorpsLevelCfg(corpsData.props.level).maxMember)
            {
                msgObj.setResponseData(ResultCodeCorps.MEMBERS_FULL);  //会员已满
                role.send(msgObj);
                return;
            }

            //这里要检测一下，看对方是否已经加入了其他公会
            checkCorpsReqs(session, role, msgObj, reqObj);
            break;

        case 2:  //拒绝入会
            if(corpsMgr.removeReqById(reqObj.corpsId, reqObj.beHandler) == false)
            {
                msgObj.setResponseData(ResultCodeCorps.HANDLE_CORPS_ERROR);  //操作错误
                role.send(msgObj);
                return;
            }
            //返回通知自己的客户端
            var resObj = new corpsMessage.HandleMemberRes(reqObj.type, reqObj.beHandler, reqObj.beHandlerName, 0);
            msgObj.setResponseData(ResultCode.SUCCESS, resObj);
            role.send(msgObj);
            break;

        case 3:  //踢出公会
            var kicker = corpsMgr.findCorpsMember(reqObj.corpsId, reqObj.beHandler);
            if(kicker == null)  //对方已经不在公会
            {
                msgObj.setResponseData(ResultCodeCorps.HANDLE_CORPS_ERROR);  //操作错误
                role.send(msgObj);
                return;
            }
            if(m.pos != CorpsPosEnum.President || (kicker.pos == CorpsPosEnum.President || kicker.pos == CorpsPosEnum.Elder))  //自己不是会长，对方是会长或长老，没有权限踢人
            {
                msgObj.setResponseData(ResultCodeCorps.CORPS_PERMISSION_DENIED);  //权限不足
                role.send(msgObj);
                return;
            }

            corpsMgr.removeMemberById(reqObj.corpsId, reqObj.beHandler, reqObj.beHandlerName, 0)
            var ro = onlineRoleMgr.findRoleByHeroId(reqObj.beHandler);
            //如果对方在线，给对方下发数据通知
            if(ro)
            {
                //修改属性
                ro.setNumber(enProp.corpsId, 0);
                ro.setString(enProp.corpsName, "");
                //通知对方客户端
                var netMsg = new corpsMessage.CorpsJoinExitRes(null, 2, 0);
                ro.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_APPLY_JOIN, netMsg);
            }
            else  //不在线就不能通过role.setNumber存盘了，只能操作数据库
            {
                var db = dbUtil.getDB(role.getUserId());
                var col = db.collection("role");
                col.updateOneNoThrow({"props.heroId":reqObj.beHandler}, {"$set":{"props.corpsId":0}});
            }
            //返回通知自己的客户端
            var resObj = new corpsMessage.HandleMemberRes(reqObj.type, reqObj.beHandler, reqObj.beHandlerName, 0);
            msgObj.setResponseData(ResultCode.SUCCESS, resObj);
            role.send(msgObj);
            break;
        case 4:  //任命职位
            if(m.pos > CorpsPosEnum.Elder)
            {
                msgObj.setResponseData(ResultCodeCorps.CORPS_PERMISSION_DENIED);  //权限不足
                role.send(msgObj);
                return;
            }
            //检查一下长老职位人数是否已满
            if(reqObj.option == CorpsPosEnum.Elder && corpsMgr.countCorpsPosNum(reqObj.corpsId, reqObj.option) >= 3)
            {
                msgObj.setResponseData(ResultCodeCorps.ELDER_IS_ENOUGH);  //长老人数已满
                role.send(msgObj);
                return;
            }
            //任命职位
            corpsMgr.appointedById(reqObj.corpsId, reqObj.handler, role.getString(enProp.name), reqObj.beHandler, reqObj.beHandlerName, reqObj.option);

            var ro = onlineRoleMgr.findRoleByHeroId(reqObj.beHandler);
            //如果对方在线，给对方下发数据通知
            if(ro)
            {
                //通知对方客户端
                var netMsg = new corpsMessage.CorpsPosChangeRes(reqObj.option);
                ro.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.PUSH_CORPS_POS_CHANGE, netMsg);
            }
            //返回通知自己的客户端
            var resObj = new corpsMessage.HandleMemberRes(reqObj.type, reqObj.beHandler, reqObj.beHandlerName, reqObj.option);
            msgObj.setResponseData(ResultCode.SUCCESS, resObj);
            role.send(msgObj);
            break;
        default:  //异常操作
            msgObj.setResponseData(ResultCode.SERVER_ERROR);
            role.send(msgObj);
            break;
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_HANDLE_MEMBER, reqHandleMember, corpsMessage.HandleMemberReq);

/**
 * 公会设置
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {CorpsSetReq} reqObj
 */
function reqCorpsSet(session, role, msgObj, reqObj)
{
    //先检查操作人有没有这个权限
    //同时检测：1.是否有这个公会、2.操作人是不是这个公会的
    var m = corpsMgr.findCorpsMember(reqObj.corpsId, reqObj.handler);
    if(m == null)  //找不到该成员，说明不是这个公会的
        return ResultCodeCorps.IS_NOT_MENMBER;  //不是本公会会员
    if(!CorpsConfig.checkHasFunc(m.pos, Number(CorpsPosFunc.CorpsSet)))   //公会的设置应该是一样的
        return ResultCodeCorps.CORPS_PERMISSION_DENIED;   //公会职能权限不足
    switch (reqObj.type)
    {
        case 1://入会设置
            corpsMgr.setJoinLimit(reqObj.corpsId, reqObj.opt1, reqObj.opt2);
            return new corpsMessage.CorpsSetRes(reqObj.type, reqObj.opt1, reqObj.opt2);
            break;
        default:  //异常操作
            return ResultCode.SERVER_ERROR;
            break;
    }
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_CORPS_SET, reqCorpsSet, corpsMessage.CorpsSetReq);

/**
 * 退出公会/解散公会
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ExitCorpsReq} reqObj
 */
function reqExitCorps(session, role, msgObj, reqObj)
{
    //先判断是不是会长，是的话只有当公会人数只有一人时才能解散公会，否则返回错误
    var m = corpsMgr.findCorpsMember(reqObj.corpsId, reqObj.heroId);
    if(m == null)
        return ResultCodeCorps.IS_NOT_MENMBER;  //不是本公会会员
    if(m.pos == CorpsPosEnum.President)  //会长
    {
        var res = corpsMgr.dissolveCorps(reqObj.corpsId, reqObj.heroId);
        if(res)
        {
            role.setNumber(enProp.corpsId, 0);
            role.setString(enProp.corpsName, "");
            //退出公会有一个cd冷却时间
            role.getCorpsPart().setQuitCorpsTime(dateUtil.getTimestamp());

            //销毁公会
            destroyCorpsCoroutine(session, role, msgObj, reqObj).catch (function (err){
                logUtil.error("销毁公会失败~corpsHandler~reqExitCorps", err);
                if (err instanceof dbUtil.MongoError)
                    msgObj.setResponseData(ResultCode.DB_ERROR);
                else
                    msgObj.setResponseData(ResultCode.SERVER_ERROR);
                role.send(msgObj);
            });
        }
        else
        {
            msgObj.setResponseData(ResultCodeCorps.PRESIDENT_CANNOT_EXIT);  //公会还有其他人
            role.send(msgObj);
        }
    }
    else
    {
        corpsMgr.removeMemberById(reqObj.corpsId, reqObj.heroId, role.getString(enProp.name), 1);

        role.setNumber(enProp.corpsId, 0);
        role.setString(enProp.corpsName, "");
        //退出公会有一个cd冷却时间
        role.getCorpsPart().setQuitCorpsTime(dateUtil.getTimestamp());

        //下发通知
        var resObj = new corpsMessage.ExitCorpsRes(0);
        msgObj.setResponseData(ResultCode.SUCCESS, resObj);
        role.send(msgObj);
    }
}
handlerMgr.registerHandler(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_EXIT_CORPS, reqExitCorps, corpsMessage.ExitCorpsReq);

/**
 * 请求公会会员和申请列表的详细数据  有更新的才返回
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {CorpsMembersReq} reqObj
 */
function reqCorpsMembers(session, role, msgObj, reqObj)
{
    if(!reqObj.isInit)
    {
        var coll = corpsMgr.collectUpdateMembersAndReqs(reqObj.corpsId, role.getCorpsPart().getMemUptime());
        var mens = coll.mems;
        var reqs = coll.reqs;
    }
    else   //初始数据 全部发送
    {
        var corpsData = corpsMgr.getCorpsData(reqObj.corpsId);
        var mens = corpsData.members;
        var reqs = corpsData.reqs;
    }
    role.getCorpsPart().setMemUptime(dateUtil.getTimestamp());//更新这个同步时间
    return new corpsMessage.CorpsMembersRes(mens, reqs, reqObj.isInit);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_REQ_MEMBERS, reqCorpsMembers, corpsMessage.CorpsMembersReq);

/**
 * 请求获取所有的公会
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {GetAllCorpsReq} reqObj
 */
function reqGetAllCorps(session, role, msgObj, reqObj)
{
    //还没有公会排行榜，先处理成全部公会的基础数据都发
    /** @type {CorpsProps[]}*/
    var list = corpsMgr.getAllCorps();
    var hasReqs = reqObj.isFirst ? role.getCorpsPart().getHasReqCorps() : [];
    return new corpsMessage.GetAllCorpsRes(list, hasReqs);

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_REQ_ALL_CORPS, reqGetAllCorps, corpsMessage.GetAllCorpsReq);

/**
 * 请求弹劾情况
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {ImpeachStatusReq} reqObj
 */
function reqImpeachStatus(session, role, msgObj, reqObj)
{
    var corps = corpsMgr.getCorpsData(reqObj.corpsId);
    return new corpsMessage.ImpeachStatusRes(corps.impeach);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_REQ_IMPEACH_STATUS, reqImpeachStatus, corpsMessage.ImpeachStatusReq);
/**
 * 请求发起弹劾
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {InitiateImpeachReq} reqObj
 */
function reqInitiateImpeach(session, role, msgObj, reqObj)
{
    var president = corpsMgr.findPresident(reqObj.corpsId);
    if(president == null)
        return ResultCodeCorps.HANDLE_CORPS_ERROR;
    //判断会长未登录时间是否达到条件
    if(dateUtil.getTimestamp() - president.lastLogout < CorpsConfig.getCorpsBaseCfg().CDROfftime)
        return ResultCodeCorps.CANNOT_IMPEACH;
    //判断贡献值是否满足要求
    var personalData = corpsMgr.findCorpsMember(reqObj.corpsId, reqObj.heroId);
    if(personalData.contribution < CorpsConfig.getCorpsBaseCfg().impContribute)
        return ResultCodeCorps.CANNOT_IMPEACH;

    if(corpsMgr.initiateImpeach(reqObj.corpsId, reqObj.heroId, role.getString(enProp.name)))
    {
        var corps = corpsMgr.getCorpsData(reqObj.corpsId);
        return new corpsMessage.ImpeachStatusRes(corps.impeach);
    }
    else
        return ResultCodeCorps.OTHERS_IMPEACH;

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_INITIATE_IMPEACH, reqInitiateImpeach, corpsMessage.InitiateImpeachReq);
/**
 * 赞成弹劾
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {AgreeImpeachReq} reqObj
 */
function reqAgreeImpeach(session, role, msgObj, reqObj)
{
    var corps = corpsMgr.getCorpsData(reqObj.corpsId);
    if(corps == null)   //还是先检查下
        return ResultCodeCorps.HANDLE_CORPS_ERROR;
    //判断贡献值是否满足要求
    var personalData = corpsMgr.findCorpsMember(reqObj.corpsId, reqObj.heroId);
    if(personalData.contribution < CorpsConfig.getCorpsBaseCfg().supportContribute)
        return ResultCodeCorps.CANNOT_AGREE_IMPEACH;

    if(dateUtil.getTimestamp() - corps.impeach.time > CorpsConfig.getCorpsBaseCfg().impTime)   //弹劾已超时
        return ResultCodeCorps.IMPEACH_OUTTIME;
    var res = corpsMgr.agreeImpeach(reqObj.corpsId, reqObj.heroId);
    if(res > 0) //赞成+1
    {
        if(res == 1)   //赞成+1 下发更新消息
        {
            var corps = corpsMgr.getCorpsData(reqObj.corpsId);
            var netMsg = new corpsMessage.ImpeachStatusRes(corps.impeach);
            role.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_REQ_IMPEACH_STATUS, netMsg);

        }
        else  //人数达到要求，会长换人 下发公会基础消息
        {
            var corps = corpsMgr.getCorpsData(reqObj.corpsId);
            var netMsg = new corpsMessage.CorpsDataRes(corps.props, [], false);
            role.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_REQ_CORPS, netMsg);
        }
        return new corpsMessage.AgreeImpeachRes(res);
    }
    else
        return ResultCodeCorps.HANDLE_CORPS_ERROR;
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_AGREE_IMPEACH, reqAgreeImpeach, corpsMessage.AgreeImpeachReq);

/**
 * 请求公会建设数据
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {CorpsBuildDataReq} reqObj
 */
function reqCorpsBuildData(session, role, msgObj, reqObj)
{
    //这里检测一下次日刷新
    var isReset = corpsMgr.checkCorpsBuildReset(reqObj.corpsId);
    if(isReset)
        role.getCorpsPart().resetOwnBuildState();

    var corps = corpsMgr.getCorpsData(reqObj.corpsId);
    var pers = role.getCorpsPart().getOwnBuildState();
    //返回消息
    return new corpsMessage.CorpsBuildDataRes(corps.buildIds.length, corps.buildLogs, pers);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_CORPS_BUILD_DATA, reqCorpsBuildData, corpsMessage.CorpsBuildDataReq);

/**
 * 请求建设公会
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {BuildCorpsReq} reqObj
 */
function reqBuildCorps(session, role, msgObj, reqObj)
{
    //这里检测一下次日刷新
    var isReset = corpsMgr.checkCorpsBuildReset(reqObj.corpsId);
    if(isReset)
    {
        role.getCorpsPart().resetOwnBuildState();
        var pers = role.getCorpsPart().getOwnBuildState();
        var netMsg = new corpsMessage.CorpsBuildDataRes(isReset.buildIds.length, isReset.buildLogs, pers);
        role.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_CORPS_BUILD_DATA, netMsg);
        return ResultCodeCorps.CORPS_BUILD_DATA_RESET;   //通知重置
    }

    var buildCfg = CorpsConfig.getCorpsBuildCfg(reqObj.buildId);
    //检查一下vip等级
    if(reqObj.buildId == 3 && role.getNumber(enProp.vipLv) < buildCfg.openVipLv)
        return ResultCodeCorps.VIP_LEVEL_NOT_REACH;   //vip等级不足
    var res = corpsMgr.buildCorps(reqObj.corpsId, role, reqObj.buildId);
    //{"res":true, "contri":corpsMemsMap[corpsId][heroId].contribution, "constr":corps.props.growValue, "level":corps.props.level, "buildNum":corps.buildIds.length}
    if(res.res)   //建设成功
    {
        //修改自己的建设状态
        role.getCorpsPart().setOwnBuildState(reqObj.buildId, 1);
        var corps = corpsMgr.getCorpsData(reqObj.corpsId);
        return new corpsMessage.BuildCorpsRes(reqObj.buildId, res.contri, res.constr, res.level, res.buildNum);
    }
    else
        return res.errorCode;

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_BUILD_CORPS, reqBuildCorps, corpsMessage.BuildCorpsReq);

/**
 * 请求他人的公会信息
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 * @param {OtherCorpsReq} reqObj
 */
function reqOtherCorps(session, role, msgObj, reqObj)
{
    var corps = corpsMgr.getCorpsData(reqObj.corpsId);
    if(corps == null)
        return ResultCodeCorps.CORPS_NOT_EXIST;
    //会员只返回会长和长老
    var m = [];
    for(var i = 0,len = corps.members.length; i<len; ++i)
    {
        var pos = corps.members[i].pos;
        if(pos == CorpsPosEnum.Elder || pos == CorpsPosEnum.President)
            m.push(corps.members[i]);
    }
    var power = corpsMgr.getCorpsPower(reqObj.corpsId);
    return new corpsMessage.OtherCorpsRes(corps.props, m, power);
}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.CMD_REQ_OTHER_CORPS, reqOtherCorps, corpsMessage.OtherCorpsReq);
