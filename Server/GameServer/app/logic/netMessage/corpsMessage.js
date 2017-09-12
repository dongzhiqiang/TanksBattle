"use strict";

/**
 * 公会信息
 * @typedef {Object} CorpsInfo
 * @property {CorpsProps} props 基础属性数据
 * @property {CorpsMember[]} members 成员——     存盘只存{heroId、pos、contribution}
 * @property {CorpsMember[]} reqs 申请——     存盘只存{id}
 * @property {Object[]} logs 日志 {id、opt、time}-数组
 * @property {Object} impeach 弹劾信息
 * @property {Object[]} buildLogs 建设纪录 {name、id}-数组
 * @property {Array} hasBuild 已参与建设的id [[],[],[]] 0今日未建设  1今日已建设    (这个不会发送给客户端)
 * @property {Number[]} buildIds 今日参与建设的id们     (这个不会发送客户端,只发送长度)
 */

/**
 * 公会基础信息
 * @typedef {Object} CorpsProps
 * @property {Number} corpsId 公会id
 * @property {Number} serverId 服务器id
 * @property {Number} createTime 创建时间
 * @property {String} name 公会名
 * @property {String} president 会长
 * @property {Number} level 公会等级
 * @property {Number} growValue 建设值 用于升级
 * @property {String} declare 宣言
 * @property {Number} rank 排名
 * @property {Number} joinSet 入会申请设置 0为无限制，1需要申请
 * @property {Number} joinSetLevel 入会等级设置
 * @property {Number} memsNum 公会人数
 * @property {Number} buildUptime 记录最近建设时间
 */

/**
 * 公会成员信息
 * @typedef {Object} CorpsMember
 * @property {Number} heroId
 * @property {String} name
 * @property {Number} level
 * @property {String} roleId
 * @property {Number} powerTotal
 * @property {Number} lastLogout 离线时间
 * @property {Number} upTime 更新时间
 * @property {Number} contribution 贡献
 * @property {Number} pos 职位
 */

/**
 * 简单的公会信息，用于查看他人的公会时发送
 * @typedef {Object} OtherCorpsInfo
 * @property {CorpsProps} props 基础属性数据
 * @property {CorpsMember[]} members 成员   只有会长、长老
 */

const CmdIdsCorps = {
    CMD_CREATE_CORPS:  1,     //创建公会
    CMD_REQ_CORPS:  2,        //请求公会信息
    CMD_MODIFYDECLARE:  3,     //修改公会宣言
    CMD_APPLY_JOIN:  4,         //申请加入公会
    CMD_HANDLE_MEMBER:  5,      //处理会员操作
    CMD_CORPS_SET:  6,           //公会设置
    CMD_EXIT_CORPS:  7,           //退出公会
    CMD_REQ_MEMBERS:  8,           //请求成员和申请信息
    CMD_REQ_ALL_CORPS:  9,         //请求所有公会
    CMD_REQ_IMPEACH_STATUS:  10,     //请求弹劾情况
    CMD_INITIATE_IMPEACH:  11,      //发起弹劾
    CMD_AGREE_IMPEACH:  12,          //同意弹劾
    CMD_CORPS_BUILD_DATA:  13,         //公会建设数据
    CMD_BUILD_CORPS:  14,            //建设公会
    CMD_REQ_OTHER_CORPS:  15,          //请求别人公会信息

    PUSH_CORPS_POS_CHANGE:  -1,     //通知自己的职位变动
    PUSH_CORPS_MEMBER_REQ:  -2,      //通知新成员信息和新申请信息
    PUSH_NEW_LOG:  -3,               //推送新的日志
    PUSH_IMPEACH_SUCCESS:  -4,        //弹劾成功会长换人,推送信息
    PUSH_CORPS_LEVEL_UP:  -5,          //公会升级
    PUSH_NEW_BUILD_LOG:  -6,          //推送新的建设记录
};

const ResultCodeCorps = {
    NAME_TOO_LONG:  1, //公会名字超出长度
    CORPS_NOT_EXIST:  2, //公会不存在
    IS_NOT_MENMBER:  3,  //不是本公会会员
    CORPS_PERMISSION_DENIED:  4,  //公会职能权限不足
    DECLARE_TOO_LONG:  5,  //公会宣言超出长度
    MEMBERS_FULL:  6,  //会员已满
    REQ_FULL:  7,  //请求已达到上限
    NOT_REACH_LIMIT:  8,  //等级不足
    DIAMOND_INSUFFICIENT:  9,  //钻石不足
    PRESIDENT_CANNOT_EXIT:  10,  //还有其他成员，会长不能退出公会
    HAS_CORPS_ALREADY:  11,  //已经有公会
    HANDLE_CORPS_ERROR:  12,    //公会数据有误，请刷新
    NAME_IS_EMPTY:  13,   //公会名字为空
    CANNOT_IMPEACH:  14,    //未达到弹劾条件
    OTHERS_IMPEACH:  15,     //已有其它成员发起弹劾
    CANNOT_AGREE_IMPEACH:  16,    //未达到赞成弹劾条件
    IMPEACH_OUTTIME:  17,        //弹劾已超时
    QUIT_CORPS_CD:  18,          //退出公会冷却中
    ELDER_IS_ENOUGH:  19,        //长老人数已满
    TODAY_HAS_BUILD:  20,      //今日已建设该建筑
    NO_ENOUGH_GOLD:  21,        //金币不足
    VIP_LEVEL_NOT_REACH:  22,   //VIP等级不够
    CORPS_BUILD_DATA_RESET:   23,   //公户建设数据已重置

};
//职能
const CorpsPosFunc = {
    Appoint: 1,    //任命职位
    Modify: 2,    //修改公告
    KickedOut:  3,   //踢除会员
    HandleReq:  4,   //处理入会请求
    CorpsSet:  5,  //入会设置
    CorpsBossSet:  6,  //公会boss设置
    Quit:  7,  //退出公会
    Impeach:  8,  //弹劾
};
//日志类型
const CorpsLogType = {
    NewElder:  1,  //xx成为了长老
    JoinCorps:  2,  //xx加入了公会
    ExitCorps:  3,  //xx离开了公会
    KillBoss:  4,  //xx击杀了boss
    Impeach:  5,  //xx发起了弹劾
    NewPresident:  6,  //xx成为了新会长
    CorpsLevelUp:  7,  //公会升级
    KickedOut:  8,  //xx被踢出了公会
    CorpsCreate:  9,  //xx创建了公会
}
//职位
const CorpsPosEnum = {
    Req : 0,   //申请中
    President : 1,    //会长
    Elder : 2,      //长老
    Common : 3    //普通会员
}

/////////////////////////////////请求////////////////////////////

//创建公会
class CreateCorpsReq
{
    constructor() {
        this.name = "";
    }
    static fieldsDesc() {
        return{
            name:{type: String, notNull: true}
        }
    }
}

//请求公会基础信息
class CorpsDataReq
{
    constructor() {
        this.corpsId = 0;
        this.isFirst = false;
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            isFirst:{type: Boolean, notNull: true},
        }
    }
}

//修改公会宣言
class ModifyCorpsDeclareReq
{
    constructor() {
        this.corpsId = 0;  //公会id
        this.declare = "";  //想要修改的宣言
        this.heroId = 0;   //修改人heroId
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            declare:{type: String, notNull: true},
            heroId:{type: Number, notNull: true},
        }
    }
}
//入会申请
class ApplyJoinCorpsReq
{
    constructor() {
        this.corpsId = 0;  //公会id
        this.heroId = 0;   //申请加入者heroId
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            heroId:{type: Number, notNull: true},
        }
    }
}
//处理会员操作
//1：同意入会 2：拒绝入会 3：踢出公会  4:任命职位
class HandleMemberReq
{
    constructor() {
        this.corpsId = 0;  //公会id
        this.handler = 0;   //处理者heroId
        this.beHandler = 0;   //被处理者heroId
        this.beHandlerName = "";   //被处理者名字
        this.type = 0;        //处理方式
        this.option = 0;      //附加参数 如果type=4则option代表职位
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            handler:{type: Number, notNull: true},
            beHandler:{type: Number, notNull: true},
            beHandlerName:{type: String, notNull: true},
            type:{type: Number, notNull: true},
            option:{type: Number, notNull: true},
        }
    }
}
//公会设置
class CorpsSetReq
{
    constructor() {
        this.corpsId = 0;  //公会id
        this.handler = 0;   //处理者heroId
        this.type = 0;        //设置类别————1：入会设置
        this.opt1 = 0;      //设置参数1
        this.opt2 = 0;      //设置参数2
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            handler:{type: Number, notNull: true},
            type:{type: Number, notNull: true},
            opt1:{type: Number, notNull: true},
            opt2:{type: Number, notNull: true},
        }
    }
}
//退出公会/会长解散公会
class ExitCorpsReq
{
    constructor() {
        this.corpsId = 0;  //公会id
        this.heroId = 0;   //退出者heroId
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            heroId:{type: Number, notNull: true},
        }
    }
}

//请求会员和申请信息
class CorpsMembersReq
{
    constructor() {
        this.isInit = false;
        this.corpsId = 0;
    }
    static fieldsDesc() {
        return{
            isInit:{type: Boolean, notNull: true},
            corpsId:{type: Number, notNull: true},
        }
    }
}
//获取所有公会
class GetAllCorpsReq
{
    constructor() {
        this.isFirst = false;    //第一次为true服务端会把请求过的id发送
    }
    static fieldsDesc() {
        return{
            isFirst:{type: Boolean, notNull: true},
        }
    }
}
//请求弹劾情况
class ImpeachStatusReq
{
    constructor() {
        this.corpsId = 0;  //公会id
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
        }
    }
}
//发起弹劾
class InitiateImpeachReq
{
    constructor() {
        this.corpsId = 0;  //公会id
        this.heroId = 0;   //发起者heroId
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            heroId:{type: Number, notNull: true},
        }
    }
}
//赞成弹劾
class AgreeImpeachReq
{
    constructor() {
        this.corpsId = 0;  //公会id
        this.heroId = 0;   //发起者heroId
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            heroId:{type: Number, notNull: true},
        }
    }
}
//请求公会建设数据
class CorpsBuildDataReq
{
    constructor() {
        this.corpsId = 0;  //公会id
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
        }
    }
}
//建设公会
class BuildCorpsReq
{
    constructor() {
        this.corpsId = 0;  //公会id
        this.heroId = 0;
        this.buildId = 0;
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
            heroId:{type: Number, notNull: true},
            buildId:{type: Number, notNull: true},
        }
    }
}
//请求他人的公会信息
class OtherCorpsReq
{
    constructor() {
        this.corpsId = 0;  //公会id
    }
    static fieldsDesc() {
        return{
            corpsId:{type: Number, notNull: true},
        }
    }
}

/////////////////////////////////返回////////////////////////////

//请求公会基础信息返回
class CorpsDataRes
{
    constructor(corpsProps, logs, isFirst) {
        this.corpsProps = corpsProps;
        this.logs = logs;
        this.isFirst = isFirst;
    }
}
//修改公会宣言返回
class ModifyCorpsDeclareRes
{
    constructor(newDeclare) {
        this.newDeclare = newDeclare;
    }
}
//公会进入/被踢 通知
class CorpsJoinExitRes
{
    constructor(corpsProps, status, option) {
        this.corpsProps = corpsProps;  //公会数据
        this.status = status;  //状态  1：加入成功 2:被踢出公会
        this.option = option;
    }
}
//处理会员操作返回
class HandleMemberRes
{
    constructor(type, beHandelId, beHandelName,opt) {
        this.type = type;   //操作类型  1：同意入会 2：拒绝入会 3：踢出公会  4:任命职位
        this.beHandelId = beHandelId;
        this.beHandelName = beHandelName;
        this.opt = opt;  //附加参数 如果type=4则option代表新职位 type=1则option=-1代表该玩家已加入其他公会，通知客户端刷新界面
    }
}
//公会设置返回
class CorpsSetRes{
    constructor(type, opt1, opt2) {
        this.type = type;
        this.opt1 = opt1;
        this.opt2 = opt2;
    }
}
//公会职位变动通知
class CorpsPosChangeRes
{
    constructor(pos) {
        this.pos = pos;  //新职位
    }
}
//退出公会/解散公会返回
class ExitCorpsRes
{
    constructor(status) {
        this.status = status;  //如果原来是会长则statue为1，否则为0
    }
}

//请求会员数据和申请数据返回 只返回有更新的数据
class CorpsMembersRes
{
    constructor(members, reqs, isInit) {
        /** @type {CorpsMember[]}*/
        this.members = members;
        /** @type {CorpsMember[]}*/
        this.reqs = reqs;
        this.isInit = isInit;
    }
}
//获取所有公会返回
class GetAllCorpsRes
{
    constructor(corpsList, hasReqs) {
        /** @type {CorpsProps[]}*/
        this.corpsList = corpsList;    //所有公户列表
        this.hasReqs = hasReqs;    //曾申请过的公会id  只有第一次请求才会下发，否则返回空数组
    }
}
//推送新的会员信息和新的申请人信息
class PushNewMemsReqDataRes
{
    constructor(newMems) {
        /** @type {CorpsMember}*/
        this.newMems = newMems;   //pos为0代表是新的申请信息
    }
}
//推送新的日志
class PushNewLogRes
{
    constructor(newLog) {
        this.newLog = newLog;   //新的日志
    }
}
//请求弹劾情况返回
class ImpeachStatusRes
{
    constructor(impeach) {
        this.impeach = impeach;
    }
}
//发起弹劾返回
class InitiateImpeachRes
{
    constructor() {

    }
}
//同意弹劾返回
class AgreeImpeachRes
{
    constructor(result) {
        this.result = result;
    }
}
//弹劾成功，推送消息
class PushImpeachSuccessRes
{
    constructor(oriPreId, newPreId, newPreName) {
        this.oriPreId = oriPreId;  //原会长id
        this.newPreId = newPreId;  //新会长id
        this.newPreName = newPreName;  //新会长名字
    }
}
//请求公会建设数据返回
class CorpsBuildDataRes
{
    constructor(buildNum, buildLog, personalState) {
        this.buildNum = buildNum;  //今日建设人数
        this.buildLog = buildLog;  //建设记录
        this.personalState = personalState;   //自己参与建设的状态 -数组 -0代表今天还可建设，1代表已不可建设
    }
}
//建设公会返回
class BuildCorpsRes
{
    constructor(buildId, contri, constru, level, buildNum) {
        this.buildId = buildId;  //建设id
        this.contri = contri;  //更新自己的贡献度
        this.constru = constru;   //更新公会的建设值
        this.level = level;      //公会等级
        this.buildNum = buildNum;  //今日建设人数
    }
}
//推送公会升级
class PushCorpsLevelUpRes
{
    constructor(constru, level) {
        this.constru = constru;   //更新公会的建设值
        this.level = level;      //公会等级
    }
}
//推送新的建设记录
class PushNewBuildLogRes
{
    constructor(newLog) {
        this.newLog = newLog;   //新的记录
    }
}
//请求他人公会信息返回
class OtherCorpsRes
{
    constructor(props, mems, corpsPower) {
        this.props = props;
        this.mems = mems;
        this.corpsPower = corpsPower;
    }
}



exports.CmdIdsCorps = CmdIdsCorps;
exports.ResultCodeCorps = ResultCodeCorps;
exports.CorpsPosFunc = CorpsPosFunc;
exports.CorpsLogType = CorpsLogType;
exports.CorpsPosEnum = CorpsPosEnum;
exports.CreateCorpsReq = CreateCorpsReq;
exports.CorpsDataReq = CorpsDataReq;
exports.CorpsDataRes = CorpsDataRes;
exports.ModifyCorpsDeclareReq = ModifyCorpsDeclareReq;
exports.ModifyCorpsDeclareRes = ModifyCorpsDeclareRes;
exports.ApplyJoinCorpsReq = ApplyJoinCorpsReq;
exports.CorpsJoinExitRes = CorpsJoinExitRes;
exports.HandleMemberReq = HandleMemberReq;
exports.HandleMemberRes = HandleMemberRes;
exports.CorpsSetReq = CorpsSetReq;
exports.CorpsSetRes = CorpsSetRes;
exports.CorpsPosChangeRes = CorpsPosChangeRes;
exports.ExitCorpsReq = ExitCorpsReq;
exports.ExitCorpsRes = ExitCorpsRes;
exports.CorpsMembersReq = CorpsMembersReq;
exports.CorpsMembersRes = CorpsMembersRes;
exports.GetAllCorpsReq = GetAllCorpsReq;
exports.GetAllCorpsRes = GetAllCorpsRes;
exports.PushNewMemsReqDataRes = PushNewMemsReqDataRes;
exports.PushNewLogRes = PushNewLogRes;
exports.InitiateImpeachReq = InitiateImpeachReq;
exports.InitiateImpeachRes = InitiateImpeachRes;
exports.AgreeImpeachReq = AgreeImpeachReq;
exports.AgreeImpeachRes = AgreeImpeachRes;
exports.ImpeachStatusReq = ImpeachStatusReq;
exports.ImpeachStatusRes = ImpeachStatusRes;
exports.PushImpeachSuccessRes = PushImpeachSuccessRes;
exports.CorpsBuildDataReq = CorpsBuildDataReq;
exports.CorpsBuildDataRes = CorpsBuildDataRes;
exports.BuildCorpsReq = BuildCorpsReq;
exports.BuildCorpsRes = BuildCorpsRes;
exports.PushCorpsLevelUpRes = PushCorpsLevelUpRes;
exports.PushNewBuildLogRes = PushNewBuildLogRes;
exports.OtherCorpsReq = OtherCorpsReq;
exports.OtherCorpsRes = OtherCorpsRes;
