
"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var dateUtil = require("../../libs/dateUtil");
var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var enProp = require("../enumType/propDefine").enProp;
var CorpsConfig =  require("../gameConfig/corpsConfig");
var CorpsLogType = require("../netMessage/corpsMessage").CorpsLogType;
var roleMgr = require("../role/roleMgr");
var onlineRoleMgr = require("../role/onlineRoleMgr");
var CorpsPosEnum = require("../netMessage/corpsMessage").CorpsPosEnum;
var corpsMessage = require("../netMessage/corpsMessage");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var CmdIdsCorps = require("../netMessage/corpsMessage").CmdIdsCorps;
var eventMgr = require("../../libs/eventMgr");
var eventNames = require("../enumType/eventDefine").eventNames;
var CorpsPart = require("../corps/corpsPart");
var ResultCodeCorps = require("../netMessage/corpsMessage").ResultCodeCorps;
var ResultCodeWeapon = require("../netMessage/weaponMessage").ResultCodeWeapon;
var enItemId = require("../enumType/globalDefine").enItemId;
var rankMgr = require("../rank/rankMgr");

/**
 * 公会数据表
 * @type {object.<Number, CorpsInfo>}
 */
var corpsMap = {};
/**
 * 公会成员表，方便查找检查成员信息
 * @type {object.<Number, object>}
 */
var corpsMemsMap = {};
/**
 * 公会申请表，方便查找检查信息
 * @type {object.<Number, object>}
 */
var corpsReqsMap = {};

//公会初始化时临时存储
var tempMap = {};

/**
 * 定时器
 * @type {*}
 */
var timer = null;

/**
 * 定时任务的间隔 单位毫秒 这里设置10分钟
 * @type {number}
 */
const TIMER_TASK_INV = 1000 * 600;
//公会最高等级
const TOP_CORPS_LEVEL = 10;
//个人贡献是物品类型，物品id
const CONTRI_ITEM_ID = 30009;

////////////函数集////////////
/**
 * 查找数据库
 * @returns {MyCollection}
 */
function getDBCollection()
{
    var db = dbUtil.getDB(0);
    return db.collection("corps");
}

/**
 * 获取公会信息
 * @param {Number} corpsId
 * @returns {CorpsInfo}
 */
function getCorpsData(corpsId)
{
    return corpsMap[corpsId];
}

/**
 * 查找指定公会id和heroId的成员基本信息,返回null则不是该公会成员
 * @param corpsId 公会id
 * @param heroId 玩家heroId
 * @returns {CorpsMember|null}
 */
function findCorpsMember(corpsId, heroId)
{
    var data = corpsMemsMap[corpsId];
    if(data == null)
        return null;
    return data[heroId];
}
/**
 * 查找指定公会id和heroId的申请者信息,返回null则不在申请中
 * @param corpsId 公会id
 * @param heroId 玩家heroId
 * @returns {object|null}
 */
function findCorpsReqs(corpsId, heroId)
{
    return corpsReqsMap[corpsId][heroId];
}
/**
 * 查找会长数据
 * @param {Number} corpsId
 * @returns {CorpsMember|null}
 */
function findPresident(corpsId)
{
    /**@type {CorpsInfo}*/
    var corpsData = corpsMap[corpsId];
    if(corpsData == null)
        return null;
    for(var i = 0,len = corpsData.members.length; i < len; ++i)
    {
        if(corpsData.members[i].pos == CorpsPosEnum.President)
            return corpsData.members[i];
    }
    return null;  //以防报错
}
/**
 * 创建新公会，添加数据
 * @param {Number} corpsId
 * @param {Object} corpsData
 * @param {Role} role
 */
function createCorps(corpsId, corpsData, role)
{
    //corpsMap增加新公会数据
    corpsMap[corpsId] = corpsData;
    var curTime = dateUtil.getTimestamp();
    var m = corpsMap[corpsId].members[0];  //此时只有一个会长
    var mInfo = makeMemberDataFromRole(role, curTime, m.contribution, m.pos);
    corpsMap[corpsId].members = [];
    corpsMap[corpsId].members.push(mInfo);

    //corpsMemsMap
    corpsMemsMap[corpsId] = {};
    corpsMemsMap[corpsId][role.getHeroId()] = mInfo;
    corpsReqsMap[corpsId] = {};

    //加入排行
    rankMgr.addToAllRankByCorpsData(corpsData);
}

/**
 * 修改公会宣言
 * @param corpsId 公会id
 * @param heroId 修改者id
 * @param newDeclare 新的宣言
 */
function modifyDeclare(corpsId, heroId, newDeclare)
{
    corpsMap[corpsId].props.declare = newDeclare;
    //写入数据库
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId}, {$set:{"props.declare":newDeclare}});

}
/**
 * 修改入会设置
 * @param corpsId
 * @param setLimit
 * @param setLevel
 */
function setJoinLimit(corpsId, setLimit, setLevel)
{
    corpsMap[corpsId].props.joinSet = setLimit;
    corpsMap[corpsId].props.joinSetLevel = setLevel;
    //写入数据库
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId}, {$set:{"props.joinSet":setLimit, "props.joinSetLevel":setLevel}});
}

//将自己的id从曾经申请过的所有公会中的申请列表里去掉
var removeCorpsReqs = Promise.coroutine(
    function * (heroId) {
        var db = dbUtil.getDB();
        var col = db.collection("role");
        //从数据库中找到申请过公会id的数据
        var info = yield col.findOne({"props.heroId": heroId},{"corps.reqCorps": 1});
        if(info == null || info.corps == null || info.corps.reqCorps == null ||info.corps.reqCorps.length == 0)//没有记录
            return;

        //把找到的公会id从对应的公会中去掉这个人的申请，批量修改数据库数据
        var col = getDBCollection();
        col.updateManyNoThrow({"props.corpsId":{$in:info.corps.reqCorps}}, {$pull:{"reqs":heroId}});

        //修改自己的数据
        var db = dbUtil.getDB(0);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId}, {"$set": {"corps.reqCorps": []}});

        //同时修改corpsMap
        for(var i = 0,len = info.corps.reqCorps.length; i < len; ++i)
        {
            let corpsId = info.corps.reqCorps[i];
            removeCorpsReq(corpsId, heroId, false);
        }
    }
);
/**
 * 将某个人从申请中去除，并修改数据库
 * @param {Number} corpsId
 * @param {Number} heroId
 * @param {String} clearDB
 */
function removeCorpsReq(corpsId, heroId, clearDB)
{
    for(var i = 0,len = corpsMap[corpsId].reqs.length; i < len; ++i)
    {
        if(corpsMap[corpsId].reqs[i].heroId == heroId)//找到就移除
        {
            corpsMap[corpsId].reqs.splice(i, 1);
            delete corpsReqsMap[corpsId][heroId];
            break;
        }
    }
    if(clearDB)  //需要修改数据库
    {
        var col = getDBCollection();
        col.updateOneNoThrow({"props.corpsId":corpsId}, {$pull:{"reqs":heroId}});
    }
}
/**
 * 将某个人从曾经申请过的公会里的申请请求都移除
 * @param {Number} heroId
 * @param {Number[]} corpsIds
 */
function removeRoleReq(heroId, corpsIds)
{
    //修改corpsMap
    for(var i = 0,len = corpsIds.length; i < len; ++i)
    {
        let corpsId = corpsIds[i];
        for(var j = 0,l = corpsMap[corpsId].reqs.length; j < l; ++j)
        {
            if(corpsMap[corpsId].reqs[j].heroId == heroId)//找到就移除
            {
                corpsMap[corpsId].reqs.splice(j, 1);
                delete corpsReqsMap[corpsId][heroId];
                break;
            }
        }
    }
}
/**
 * 将指定role玩家加入指定id公会
 * @param {Number} corpsId 公会id
 * @param {Number} heroId 加入者的id
 * @param {string} heroName 加入者的名字
 * @param {boolean} auto 是否自动加入
 * @returns {boolean}
 */
function joinCorpsById(corpsId, heroId, heroName, auto)
{
    //先检查下这个玩家还在申请表吗
    if(!auto && corpsReqsMap[corpsId][heroId] == null)
        return false;

    //将这名玩家的其他公会入会申请都移除
    if(auto)   //无限制加入的，需要把其他申请去掉
        removeCorpsReqs(heroId);

    var m = {"heroId":heroId,"pos":Number(CorpsPosEnum.Common),"contribution":0};
    //更新corpsMemsMap
    var addRole = roleMgr.findRoleByHeroId(heroId);
    var t = dateUtil.getTimestamp();
    if(addRole)
    {
        var data = makeMemberDataFromRole(addRole, t, 0,3);
        corpsMemsMap[corpsId][heroId] = data;
        corpsMap[corpsId].members.push(data);
        //退会时间清0
        addRole.getCorpsPart().setQuitCorpsTime(t);
    }
    else  //不在线要从数据库取数据
    {
        //退会时间清0
        CorpsPart.CorpsPart.setQuitCorpsTimeDB(0, heroId, t);
        corpsMemsMap[corpsId][heroId] = {};   //先留个位置
        //用临时字典存
        tempMap[heroId] = {"info":m, "corpsId":corpsId};
        loadRolesFromDB([heroId]);
    }
    corpsMap[corpsId].props.memsNum ++;

    //写入日志
    var log = addCorpsLog(corpsId, CorpsLogType.JoinCorps, heroName);
    //写入数据库 这里一次处理就够了
    //更新人数、加入会员数据、写入日志
    var newMemNum = corpsMap[corpsId].members.length;
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId}, {$set:{"props.memsNum":newMemNum},
        $push:{"members":m, "logs":{$each:[log], $position:0, $slice:CorpsConfig.getCorpsBaseCfg().logMax}}});
    //$position:0 从前面插入，  $slice保留数组前面N个，多出的删掉
    return true;
}
/**
 * 将指定id玩家移除出指定id的公会
 * @param {Number} corpsId
 * @param {Number} heroId
 * @param {string} heroName 名字
 * @param type 0:被踢 1：自己退
 */
function removeMemberById(corpsId, heroId, heroName, type)
{
    //修改corpsMap
    for(var i = 0,len = corpsMap[corpsId].members.length; i < len; ++i)
    {
        if(corpsMap[corpsId].members[i].heroId == heroId)//找到就移除
        {
            corpsMap[corpsId].members.splice(i, 1);
            break;
        }
    }
    corpsMap[corpsId].props.memsNum -= 1;
    //修改corpsMemsMap
    corpsMemsMap[corpsId][heroId] = null;
    delete corpsMemsMap[corpsId][heroId];

    //写入日志
    var log = null;
    if(type == 0)
        log = addCorpsLog(corpsId, CorpsLogType.KickedOut, heroName);
    else
        log = addCorpsLog(corpsId, CorpsLogType.ExitCorps, heroName);
    //写入数据库 这里一次处理就够了
    //更新会员数量、写入日志、移除会员
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId}, {$push:{"logs":{$each:[log], $position:0, $slice:CorpsConfig.getCorpsBaseCfg().logMax}},
        $set:{"props.memsNum":corpsMap[corpsId].members.length}, $pull:{"members":{"heroId":heroId}}});
}
/**
 * 将指定role玩家添加到指定id的公会申请列表
 * @param {Number} corpsId
 * @param {Role} role
 * @returns {boolean}
 */
function addReqById(corpsId, role)
{
    var heroId = role.getHeroId();
    for(var i = 0,len = corpsMap[corpsId].reqs.length; i < len; ++i)
    {
        if(corpsMap[corpsId].reqs[i].heroId == heroId)//说明已经在申请中了
            return false;
    }
    var mData = makeMemberDataFromRole(role, 0, 3);
    corpsMap[corpsId].reqs.push(mData);
    corpsReqsMap[corpsId][heroId] = mData;
    //写入数据库
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId}, {$addToSet:{"reqs":heroId}});
    return true;
}
/**
 * 将指定id玩家移除出指定id的公会申请列表
 * @param {Number} corpsId
 * @param {Number} heroId
 * @returns {boolean}
 */
function removeReqById(corpsId, heroId)
{
    //先检查下这个玩家还在申请表吗
    if(corpsReqsMap[corpsId][heroId] == null)
        return false;

    for(var i = 0,len = corpsMap[corpsId].reqs.length; i < len; ++i)
    {
        if(corpsMap[corpsId].reqs[i].heroId == heroId)//找到就移除
        {
            corpsMap[corpsId].reqs.splice(i, 1);
            delete corpsReqsMap[corpsId][heroId];
            break;
        }
    }
    //写入数据库
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId}, {$pull:{"reqs":heroId}});
    return true;
}

/**
 * 对指定id玩家任命职位
 * @param {Number} corpsId
 * @param {Number} hanId
 * @param {String} hanName
 * @param {Number} heroId 被处理者id
 * @param {String} beHandlerName 被处理者名字
 * @param {Number} pos 职位 1：会长 2：长老 3：成员
 */
function appointedById(corpsId, hanId, hanName, heroId, beHandlerName, pos)
{
    //直接修改corpsMemsMap就可以了
    corpsMemsMap[corpsId][heroId].pos = pos;
    //写入数据库 修改职位
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId, "members.heroId":heroId}, {$set:{"members.$.pos":pos}});

    //标记更新
    updateProp(corpsId, heroId, {"pos":corpsMemsMap[corpsId][heroId].pos});

    //日志处理
    if(pos == CorpsPosEnum.President)  //转让会长
    {
        //将自己的职位置为普通会员
        col.updateOneNoThrow({"props.corpsId":corpsId, "members.heroId":hanId}, {$set:{"members.$.pos":Number(CorpsPosEnum.Common)}});
        //修改内存
        corpsMemsMap[corpsId][hanId].pos = Number(CorpsPosEnum.Common);
        corpsMap[corpsId].props.president = beHandlerName;
        var log = addCorpsLog(corpsId, CorpsLogType.NewPresident, beHandlerName);
        //数据库操作：修改会长、写入日志
        col.updateOneNoThrow({"props.corpsId":corpsId}, {$set:{"props.president":beHandlerName},
            $push:{"logs":{$each:[log], $position:0, $slice:CorpsConfig.getCorpsBaseCfg().logMax}}});
    }
    else if(pos == CorpsPosEnum.Elder)  //任命长老
    {
        var log = addCorpsLog(corpsId, CorpsLogType.NewElder, beHandlerName);
        //数据库操作：写入日志
        col.updateOneNoThrow({"props.corpsId":corpsId}, {$push:{"logs":{$each:[log], $position:0, $slice:CorpsConfig.getCorpsBaseCfg().logMax}}});
    }
}
/**
 * 解散公会
 * @param corpsId
 */
function dissolveCorps(corpsId, heroId)
{
    if(corpsMap[corpsId].members.length > 1)   //公会里还有其他成员
        return false;
    removeMemberById(corpsId, heroId, "", 1);
    delete corpsMap[corpsId];
    delete corpsMemsMap[corpsId];
    delete corpsReqsMap[corpsId];

    //从排行删除
    rankMgr.removeFromAllRankByCorpsId(corpsId);

    return true;
}

/**
 * 指定id的公会添加新日志,写入内存、消息通知在线成员。数据库处理不写在这里，可以减少修改数据库的次数。
 * @param {Number} corpsId
 * @param {Number} type
 * @param {String} opt 附加参数
 * @returns {{id: *, opt: *, time: (*|number)}}
 */
function addCorpsLog(corpsId, type, opt)
{
    let maxLog = CorpsConfig.getCorpsBaseCfg().logMax;
    //修改corpsMap
    if(corpsMap[corpsId].logs.length >= maxLog)//超出最大上限则删除最后（最旧的）一条
        corpsMap[corpsId].logs.pop();
    var newLog = {"id":type, "opt":opt, "time":dateUtil.getTimestamp()};
    corpsMap[corpsId].logs.unshift(newLog);   //修改内存corpsMap  从前面插入
    //对在线的role推送消息通知
    for(var i = 0,len = corpsMap[corpsId].members.length; i < len; ++i)
    {
        var role = onlineRoleMgr.findRoleByHeroId(corpsMap[corpsId].members[i].heroId);
        if(role)
        {
            var netMsg = new corpsMessage.PushNewLogRes(newLog);
            role.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.PUSH_NEW_LOG, netMsg);
        }
    }
    return newLog;
}
/**
 * 发起弹劾
 * @param {Number} corpsId
 * @param {Number} heroId
 * @param {String} heroName
 * @returns {Boolean}
 */
function initiateImpeach(corpsId, heroId, heroName)
{
    var corps = corpsMap[corpsId];
    var impeach = corps.impeach || {};
    if(impeach.initiateId != null && impeach.initiateId != "")   //已经有别人发起了弹劾
        return false;
    var time = dateUtil.getTimestamp();
    impeach = {"initiateId":heroId, "initiateName":heroName, "time":time, "agree":[]};
    corpsMap[corpsId].impeach = impeach;

    //写入数据库
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId}, {$set:{"impeach":impeach}});

    return true;
}
/**
 * 同意弹劾
 * @param {Number} corpsId
 * @param {Number} heroId
 * @returns {Number}  -1：失败  1：赞成成功，会长还没改变  2：会长发生改变
 */
function agreeImpeach(corpsId, heroId)
{
    var corps = corpsMap[corpsId];
    var impeach = corps.impeach || {};
    if(impeach.initiateId == null || impeach.initiateId == "")   //没有人发起弹劾
        return -1;
    corps.impeach.agree.push(heroId);
    if(corps.impeach.agree.length >= CorpsConfig.getCorpsBaseCfg().supportNum)  //人数满足，会长转换
    {
        //对在线的role推送消息通知
        var oriPreId = findPresident(corpsId).heroId;  //先记着原来的会长heroId
        for(var i = 0,len = corps.members.length; i < len; ++i)
        {
            var role = onlineRoleMgr.findRoleByHeroId(corps.members[i].heroId);
            if(role)
            {
                var netMsg = new corpsMessage.PushImpeachSuccessRes(oriPreId, corps.impeach.initiateId, corps.impeach.initiateName);
                role.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.PUSH_IMPEACH_SUCCESS, netMsg);
            }
        }

        //修改内存
        var newPresident = corps.impeach.initiateName;
        corps.props.president = newPresident;   //属性里的会长修改
        var iniId = corps.impeach.initiateId;
        corpsMemsMap[corpsId][iniId].pos = CorpsPosEnum.President;   //职位修改
        corpsMemsMap[corpsId][oriPreId].pos = CorpsPosEnum.Common;
        var log = addCorpsLog(corpsId, CorpsLogType.NewPresident, newPresident);   //日志添加
        corps.impeach = {};   //弹劾信息重置

        //写入数据库:会长修改、发起人职位变动修改、旧会长职位变动修改、日志修改、弹劾信息重置
        var col = getDBCollection();
        col.updateOneNoThrow({"props.corpsId":corpsId, "members.heroId":iniId}, {$set:{"members.$.pos":Number(CorpsPosEnum.President)}});
        col.updateOneNoThrow({"props.corpsId":corpsId, "members.heroId":oriPreId}, {$set:{"members.$.pos":Number(CorpsPosEnum.Common)}});
        col.updateOneNoThrow({"props.corpsId":corpsId}, {$set:{"props.president":newPresident, "impeach":{}},
            $push:{"logs":{$each:[log], $position:0, $slice:CorpsConfig.getCorpsBaseCfg().logMax}}});
        return 2;
    }
    else
    {
        //写入数据库
        var col = getDBCollection();
        col.updateOneNoThrow({"props.corpsId":corpsId}, {$push:{"impeach.agree":heroId}});
        return 1;
    }
    return 0;
}
/**
 * 公会建设
 * @param {Number} corpsId 公会id
 * @param {Role} role 角色
 * @param {Number} buildId 建设id
 * @returns {boolean}
 */
function buildCorps(corpsId, role, buildId)
{
    var corps = getCorpsData(corpsId);
    var heroId = role.getHeroId();
    //拿到配置
    var buildCfg = CorpsConfig.getCorpsBuildCfg(buildId);

    //先检查今天是否已经建设过
    var arr = corps.hasBuild[buildId-1];
    if(arr.length > 0)
    {
        for(var i = 0,len = arr.length; i < len; ++i)
        {
            if(arr[i] == heroId)   //自己已经建设过
                return {"res":false, "errorCode":ResultCodeCorps.TODAY_HAS_BUILD};
        }
    }
    //判断消耗
    var costs = buildCfg.cost.split('|');
    var itemsPart = role.getItemsPart();
    if(costs[0] == 1)  //消耗金币
    {
        if(role.getNumber(enProp.gold) >= costs[1])
            itemsPart.costItem(enItemId.GOLD,costs[1]);
        else
            return {"res":false, "errorCode":ResultCodeCorps.NO_ENOUGH_GOLD};
    }
    else if(costs[0] == 2)  //消耗钻石
    {

        if(role.getNumber(enProp.diamond) >= costs[1])
            itemsPart.costItem(enItemId.DIAMOND,costs[1]);
        else
            return {"res":false, "errorCode":ResultCodeCorps.DIAMOND_INSUFFICIENT};
    }

    //今日建设人数修改
    corps.buildIds.pushIfNotExist(heroId);   //不在的才加进来
    arr.pushIfNotExist(heroId);
    //自己增加贡献
    corpsMemsMap[corpsId][heroId].contribution += buildCfg.contri;
    //贡献物品添加
    role.getItemsPart().addItem(CONTRI_ITEM_ID, buildCfg.contri);
    //公会增加贡献
    var levelUp = false;
    //判断建设人数是否已达到上限
    var isFull = corps.buildIds.length > CorpsConfig.getCorpsLevelCfg(corps.props.level).maxMember;
    if(!isFull)  //建设人数不达到上限才可以加公会建设值和个人贡献，否则只加个人贡献
    {
        levelUp = addCorpsGrowValue(corpsId, buildCfg.corpsConstr);
        //建设记录
        var news = {"name":role.getString(enProp.name), "buildId":buildId};
        corps.buildLogs.unshift(news);
        if(corps.buildLogs.length > CorpsConfig.getCorpsBaseCfg().buildLogMax)
            corps.buildLogs.pop();

        //对在线的角色推送新记录
        for(var i = 0,len = corps.members.length; i < len; ++i)
        {
            var onlineRole = onlineRoleMgr.findRoleByHeroId(corps.members[i].heroId);
            if(onlineRole)
            {
                var netMsg = new corpsMessage.PushNewBuildLogRes(news);
                onlineRole.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.PUSH_NEW_BUILD_LOG, netMsg);
            }
        }
    }

    //标记更新
    updateProp(corpsId, heroId, {"contribution":corpsMemsMap[corpsId][heroId].contribution});

    //更新建设时间
    var uptime = dateUtil.getTimestamp();
    corps.props.buildUptime = uptime;

    //写入数据库——更新公会建设值、等级、建设列表信息、建设记录
    var col = getDBCollection();
    col.updateOneNoThrow({"props.corpsId":corpsId}, {"$addToSet":{"buildIds":heroId}, "$set":{"hasBuild":corps.hasBuild,
        "props.growValue":corps.props.growValue, "props.level":corps.props.level, "props.buildUptime":uptime},
     /*   $push:{"buildLogs":{$each:[news], $position:0, $slice:CorpsConfig.getCorpsBaseCfg().buildLogMax}}*/});
    if(!isFull)  //达不到上限才能写入记录
    {
        col.updateOneNoThrow({"props.corpsId":corpsId}, {$push:{"buildLogs":{$each:[news], $position:0, $slice:CorpsConfig.getCorpsBaseCfg().buildLogMax}}});
        //$position:0 从前面插入，  $slice保留数组前面N个，多出的删掉
    }

    //更新个人贡献写入数据库
    col.updateOneNoThrow({"props.corpsId":corpsId, "members.heroId":heroId}, {$set:{"members.$.contribution":corpsMemsMap[corpsId][heroId].contribution}});
    if(levelUp)   //升级要添加日志
    {
        var log = addCorpsLog(corpsId, CorpsLogType.CorpsLevelUp, corps.props.level.toString());
        col.updateOneNoThrow({"props.corpsId":corpsId}, {$push:{"logs":{$each:[log], $position:0, $slice:CorpsConfig.getCorpsBaseCfg().logMax}}});
    }

    return {"res":true, "contri":corpsMemsMap[corpsId][heroId].contribution, "constr":corps.props.growValue, "level":corps.props.level, "buildNum":corps.buildIds.length};

}
/**
 * 公会增加建设值
 * @param {Number} corpsId 公会id
 * @param {Number} constru 增加的建设值
 * @constructor
 */
function addCorpsGrowValue(corpsId, constru)
{
    var corps = getCorpsData(corpsId);
    corps.props.growValue += constru;
    if(corps.props.level == TOP_CORPS_LEVEL)   //已经满级
        return false;
    //判断是否达到升级条件 还未实现
    var upValue = CorpsConfig.getCorpsLevelCfg(corps.props.level+1).upValue;
    if(corps.props.growValue >= upValue)  //公会升级
    {
        corps.props.level ++;
        corps.props.growValue -= upValue;

        //通知在线的人
        for(var i = 0,len = corps.members.length; i<len; ++i)
        {
            var mRole = onlineRoleMgr.findRoleByHeroId(corps.members[i].heroId);
            if(mRole)
            {
                var netMsg = new corpsMessage.PushCorpsLevelUpRes(corps.props.growValue, corps.props.level);
                mRole.sendEx(ModuleIds.MODULE_CORPS, CmdIdsCorps.PUSH_CORPS_LEVEL_UP, netMsg);
            }
        }
        return true;
    }
    return false;
}
/**
 * 检查时间是否重置公会建设数据
 * @param corpsId
 */
function checkCorpsBuildReset(corpsId)
{
    var corps = getCorpsData(corpsId);
    if(!dateUtil.isToday(corps.props.buildUptime))   //不是今天 重置
    {
        corps.buildLogs = [];
        corps.hasBuild = [[],[],[]];
        corps.buildIds = [];
        corps.props.buildUptime = dateUtil.getTimestamp();

        //这里不用对每个在线的人通知，减少遍历和消息通知，让每个人打开再判断是否重置就可以。

        //存盘操作
        var col = getDBCollection();
        col.updateOneNoThrow({"props.corpsId":corpsId}, {"$set":{"buildLogs":[], "hasBuild":[[],[],[]], "buildIds":[],
            "props.buildUptime":corps.props.buildUptime}});

        return corps;
    }
    return null;

}

/**
 * 查找某个公会某职位人数
 * @param corpsId
 * @param pos
 * @returns {number}
 */
function countCorpsPosNum(corpsId, pos)
{
    var corps = getCorpsData(corpsId);
    var count = 0;
    for(var i = 0,len = corps.members.length; i < len; ++i)
    {
        if(corps.members[i].pos == pos)
            count++;
    }
    return count;
}

/**
 * 计算公会总战力
 * @param corpsId
 * @returns {number}
 */
function getCorpsPower(corpsId)
{
    var corps = getCorpsData(corpsId);
    if(corps == null)return 0;
    var totalPower = 0;
    for(var i = 0,len = corps.members.length; i<len; ++i)
        totalPower += corps.members[i].powerTotal;
    return totalPower;
}

/**
 * 收集有发生更新的会员信息和申请人信息
 * @param {Number} corpsId
 * @param {Number} uptime
 */
function collectUpdateMembersAndReqs(corpsId, uptime)
{
    /**@type {object.<Number, CorpsMember>}*/
    var memData = corpsMemsMap[corpsId];
    var data = [];
    var corps = getCorpsData(corpsId);
    for(var i = 0,len = corps.members.length; i<len; ++i)
    {
        //这里要判断upTime是否一致,如果不同就要发送
        if(uptime < corps.members[i].upTime)
            data.push(memData[corps.members[i].heroId]);
    }

    /**@type {object.<Number, CorpsMember>}*/
    var c = corpsReqsMap[corpsId];
    var data2 = [];
    for(var i = 0,len = corps.reqs.length; i<len; ++i)
    {
        //这里要判断指定id的map里面的信息upTime是否一致
        if(corps.reqs[i].upTime != uptime)
            data2.push(c[corps.reqs[i]]);
    }
    //更新memUptime
    corps.memUptime = dateUtil.getTimestamp();

    return {"mems":data, "reqs":data2};
}
//返回所有公会
function getAllCorps()
{
    /** @type {CorpsProps[]}*/
    var list = [];
    for(var key in corpsMap)
    {
        list.push(corpsMap[key].props);
    }
    return list;
}

function getCorpsMap()
{
    return corpsMap;
}

/**
 * 从role对象获取所需要的公会成员数据
 * @param {Role} role
 * @param {Number} curTime
 * @param {Number} contribution
 * @param {Number} pos
 * @returns {CorpsMember}
 */
function makeMemberDataFromRole(role, curTime, contribution, pos)
{
    /** @type {CorpsMember}*/
    var data = {};
    data.heroId = role.getNumber(enProp.heroId);
    data.name = role.getString(enProp.name);
    data.level = role.getNumber(enProp.level);
    data.roleId = role.getString(enProp.roleId);
    data.powerTotal = role.getNumber(enProp.powerTotal);
    data.lastLogout = role.getLastOfflineTime();
    data.upTime = curTime;
    data.contribution = contribution;
    data.pos = pos;
    return data;
}

/**
 * 把公会成员和申请人信息存起来，方便查找
 */
function addMemsReqsMap()
{
    for(var key in corpsMap)
    {
        var data = corpsMap[key];
        var map = {};
        for(var i = 0,len = data.members.length; i < len; ++i)
        {
            map[data.members[i].heroId] = data.members[i];
        }
        corpsMemsMap[key] = map;

        var reqMap = {};
        for(var i = 0,len = data.reqs.length; i < len; ++i)
        {
            reqMap[data.reqs[i].heroId] = data.reqs[i];
        }
        corpsReqsMap[key] = reqMap;
    }
    tempMap = {}; //将临时字典清空
}
//定时器检查
function onTimerTask()
{
    var nowTime = dateUtil.getTimestamp();
    //检查弹劾进度
    for(var corpsId in corpsMap)
    {
        if(corpsMap[corpsId].impeach != null && corpsMap[corpsId].impeach.initiateId > 0)  //有人发起弹劾
        {
            if(nowTime - corpsMap[corpsId].impeach.time > CorpsConfig.getCorpsBaseCfg().impTime)  //弹劾超过时间了 失败
            {
                corpsMap[corpsId].impeach = {};   //弹劾信息重置
                //写入数据库
                var col = getDBCollection();
                col.updateOneNoThrow({"props.corpsId":corpsId}, {$set:{"impeach":{}}});
            }
        }
    }
}
/**
 * 更新指定corosId和heroId的成员属性
 * @param {Number} corpsId
 * @param {Number}  heroId
 * @param {Object} props
 */
function updateProp(corpsId, heroId, props)
{
    var memsData = findCorpsMember(corpsId, heroId);
    if(memsData == null)
        return;
    for(var key in props)
    {
        memsData[key] = props[key];
    }
    memsData.upTime = dateUtil.getTimestamp();  //修改时间，以便找到
}

/**
 * 从数据库取角色数据
 */
var loadRolesFromDB = Promise.coroutine(
    function * (findIds){
        try{
            var db = dbUtil.getDB(0);
            var col = db.collection("role");
            //一次性从数据库读数据
            var infos = yield col.findArray({"props.heroId":{$in:findIds}},{
                "props.heroId":1,
                "props.name":1,
                "props.level":1,
                "props.roleId":1,
                "props.powerTotal":1,
                "props.lastLogout":1  //离线时间
            });

            var curTime = dateUtil.getTimestamp();
            var infoCnt = infos.length;

            for(let i = 0;i < infoCnt; ++i)
            {
                let info = infos[i];
                /** @type {CorpsMember} */
                let props = info.props;
                let heroId = props.heroId;
                var m = tempMap[heroId];
                props.upTime = curTime;
                props.contribution = m.info.contribution;
                props.pos = m.info.pos;

                //在原有上面添加
                var corpsId = m.corpsId;
                //按各自位置加入
                if(corpsMemsMap[corpsId][heroId])
                {
                    corpsMemsMap[corpsId][heroId] = props;
                    corpsMap[corpsId].members.push(props);
                }
                else if(corpsReqsMap[corpsId][heroId])
                {
                    corpsReqsMap[corpsId][heroId] = props;
                    corpsMap[corpsId].reqs.push(props);
                }
            }
        }
        catch(err){
            throw err;
        }
    }
);

/**
 * 公会模块数据初始化
 */
var doInitCoroutine = Promise.coroutine(function * () {
    logUtil.info("公会模块数据开始初始化...");

    var curTime = dateUtil.getTimestamp();
    //把公会数据库一次读出来
    var col = getDBCollection();
    var arr = yield col.findArray({});
    for (let i = 0; i < arr.length; ++i)
    {
        /** @type {CorpsInfo}*/
        var data = arr[i];
        corpsMap[data.props.corpsId] = data;
    }

    //获取会员的详细数据
    var idsList = [];
    var curTime = dateUtil.getTimestamp();

    for(var corpsId in corpsMap)
    {
        //记得先初始化
        corpsMemsMap[corpsId] = {};
        corpsReqsMap[corpsId] = {};
        //存储详细信息
        var details = [];
        //处理会员信息
        for(var i = 0,len = corpsMap[corpsId].members.length; i < len; ++i)
        {
            var m = corpsMap[corpsId].members[i];
            var role = roleMgr.findRoleByHeroId(m.heroId);
            if (role)  //内存中存在
            {
                var mInfo = makeMemberDataFromRole(role, curTime, m.contribution, m.pos);
                details.push(mInfo);
            }
            else
            {
                idsList.push(m.heroId);
                //用临时字典存
                tempMap[m.heroId] = {"info":m, "corpsId":corpsId};
                corpsMemsMap[corpsId][m.heroId] = {};   //先留个位置
            }
        }
        //置空原来的，用新的详细role数据去设置members数组
        corpsMap[corpsId].members = details;
        var details2 = [];
        //申请人信息
        for(var i = 0,len = corpsMap[corpsId].reqs.length; i < len; ++i)
        {
            /**@type {Number}*/
            var m = corpsMap[corpsId].reqs[i];
            var role = roleMgr.findRoleByHeroId(m);
            if (role)  //内存中存在
            {
                var mInfo = makeMemberDataFromRole(role, curTime, 0, 0);
                details2.push(mInfo);
            }
            else
            {
                idsList.push(m);
                var inf = { "heroId":m, "pos":0, "contribution":0};
                //用临时字典存
                tempMap[m] = {"info":inf, "corpsId":corpsId};
                corpsReqsMap[corpsId][m] = {};   //先留个位置

            }
        }
        corpsMap[corpsId].reqs = details2;
    }

    if(idsList.length > 0)  //数组长度不为0才请求数据库
        yield loadRolesFromDB(idsList);
    addMemsReqsMap();

    //创建定时器
    timer = setInterval(onTimerTask, TIMER_TASK_INV);
    //马上调用一次
    onTimerTask.call(timer);

    //订阅角色登录事件
    eventMgr.addGlobalListener(onRoleLogin, eventNames.ROLE_LOGIN);

    logUtil.info("公会模块数据完成初始化");
});

function doInit()
{
    return doInitCoroutine();
}
/**
 * 订阅角色登录消息
 * @param {string} eventName
 * @param {*} context
 * @param {Role} notifier
 */
function onRoleLogin(eventName, context, notifier)
{
    var corpsId = notifier.getNumber(enProp.corpsId);
    if(corpsId > 0) //有公会
    {
        var heroId = notifier.getHeroId();
        if(corpsMap[corpsId].impeach != null && corpsMap[corpsId].impeach.initiateId)  //有人发起了弹劾
        {
            //原会长上线了 弹劾终止
            if(corpsMemsMap[corpsId][heroId].pos == CorpsPosEnum.President)
            {
                corpsMap[corpsId].impeach = {};   //弹劾信息重置
                //写入数据库
                var col = getDBCollection();
                col.updateOneNoThrow({"props.corpsId":corpsId}, {$set:{"impeach":{}}});
            }
        }
     /*   var data = corpsMemsMap[corpsId][heroId];
        //更新在线信息
        data.lastLogout = 0;
        data.upTime = dateUtil.getTimestamp();*/

    }
}

/**
 * 公会模块数据销毁
 */
var doDestroyCoroutine = Promise.coroutine(function * () {
    logUtil.info("公会模块数据开始销毁...");

    //取消订阅角色登录事件
    eventMgr.removeGlobalListener(onRoleLogin, eventNames.ROLE_LOGIN);

    //清数据
    corpsMap = {};
    corpsMemsMap = {};
    corpsReqsMap = {};

    //删除定时器
    if (timer)
    {
        clearInterval(timer);
        timer = null;
    }

    logUtil.info("公会模块数据完成销毁");
});

function doDestroy()
{
    return doDestroyCoroutine();
}


exports.doInit = doInit;
exports.doDestroy = doDestroy;
exports.getCorpsData = getCorpsData;
exports.findCorpsMember = findCorpsMember;
exports.modifyDeclare = modifyDeclare;
exports.setJoinLimit = setJoinLimit;
exports.joinCorpsById = joinCorpsById;
exports.removeReqById = removeReqById;
exports.removeMemberById = removeMemberById;
exports.appointedById = appointedById;
exports.dissolveCorps = dissolveCorps;
exports.addCorpsLog = addCorpsLog;
exports.collectUpdateMembersAndReqs = collectUpdateMembersAndReqs;
exports.createCorps = createCorps;
exports.getAllCorps = getAllCorps;
exports.getCorpsMap = getCorpsMap;
exports.addReqById = addReqById;
exports.makeMemberDataFromRole = makeMemberDataFromRole;
exports.findPresident = findPresident;
exports.initiateImpeach = initiateImpeach;
exports.agreeImpeach = agreeImpeach;
exports.updateProp = updateProp;
exports.removeRoleReq = removeRoleReq;
exports.removeCorpsReq = removeCorpsReq;
exports.countCorpsPosNum = countCorpsPosNum;
exports.buildCorps = buildCorps;
exports.checkCorpsBuildReset = checkCorpsBuildReset;
exports.getCorpsPower = getCorpsPower;
