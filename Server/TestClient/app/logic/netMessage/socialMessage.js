"use strict";

const CmdIdsSocial = {
    CMD_REQ_FRIENDS: 1,   //请求好友列表
    CMD_ADD_FRIEND:  2,   //添加好友
    CMD_HANDLE_FRIEND: 3,  //处理好友请求
    CMD_SEND_STAMINA:  4,  //赠送体力
    CMD_GET_STAMINA:  5,  //领取体力
    CMD_ONEKEY_STAMINA:  6,   //一键领取体力
    CMD_DELETE_FRIEND:  7,  //删除好友
    CMD_FRIEND_RECOMMEND:  8,  //好友推荐
    CMD_REFRESH_RECOMMEND:  9,  //刷新推荐好友
    CMD_ONEKEY_ADD_FRIEND:  10,  //一键添加好友

    PUSH_ADD_FRIEND: -1,    //他人的加好友推送
    PUSH_NEW_STAMINA:  -2,   //新的体力赠送


};
const ResultCodeSocial = {
    FRIEND_NOT_FOUND_ERROR: 1,  //此玩家不存在，请检查名字是否有误
    IS_IN_FRIEND_REQ: 2,   //已经在对方的申请列表中
    FRIEND_IS_FULL: 3,   //自己的好友已满
    ADD_SELF_ERROR: 4,   //不能添加自己为好友
    SEND_STAMINA_ERROR:  5,  //今天已经赠送过该好友体力
    SEND_STAMINA_TIMEOUT:  6,  //超出今天最大赠送上限
    GET_STAMINA_ERROR:  7,  //今天已经领取过该好友体力
    GET_STAMINA_TIMEOUT:  8,  //超出今天最大领取上限
    RESET_STAMINA_TIMES:  9,  //体力领取和赠送次数发生重置了
    IS_NOT_FRIEND:  10,  //对方不是好友
    IS_FRIEND: 11,  //对方已经是好友
    RECOM_REFR_TIME_CD:  12,  //推荐好友刷新冷却中
    OTHER_FRIEND_IS_FULL:  13,  //对方好友已满
};

const HandlerTypeEnum = {
    Agree  : 0,    //同意
    Refuse  : 1,    //拒绝
    ResuseAll  :  2,   //全部拒绝
    BeAgreed:  3,   //被同意
    BeRefused:  4,  //被拒绝
};

/////////////////////////////////请求////////////////////////////

//请求好友信息
class ReqFriendsReq
{
    constructor() {
    }
}
//添加好友
class AddFriendReq
{
    constructor() {
        this.addName = "";   //对方名字
        /**
         * @type {Friend}
         */
        this.adder = {};   //发起添加的角色信息
    }
    static fieldsDesc() {
        return {
            addName: {type: String, notNull: true},
            adder: {type:Object, notNull: true}
        };
    }
}

//处理好友请求
class HandleFriendReq
{
    constructor() {
        this.name = "";   //处理请求方的名字
        this.heroId = 0;   //被处理方的heroId
        this.type = 0;   //处理类型
    }
    static fieldsDesc() {
        return {
            name: {type: String, notNull: true},
            heroId: {type: Number, notNull: true},
            type: {type: Number, notNull: true},
        };
    }
}
//赠送体力
class SendStaminaReq
{
    constructor(){
        this.heroId = 0;
    }
    static fieldsDesc(){
        return {
            heroId: {type: Number, notNull: true},
        }
    }
}
//领取体力
class GetStaminaReq
{
    constructor(){
        this.heroId = 0;
    }
    static fieldsDesc(){
        return {
            heroId: {type: Number, notNull: true},
        }
    }
}
//一键领取体力
class OnekeyGetStaminaReq
{
    constructor(){
        this.heroIds = [];
    }
    static fieldsDesc(){
        return{
            heroIds:{type: Array, itemType:Number, notNull: true},
        }
    }
}
//删除好友
class DeleteFriendReq
{
    constructor(){
        this.heroId = 0;
    }
    static fieldsDesc(){
        return{
            heroId:{type: Number, notNull: true},
        }
    }
}

//好友推荐
class FriendRecommendReq
{
    constructor() {
        this.isFirst = false;
    }
    static fieldsDesc() {
        return{
            isFirst: {type: Boolean, notNull: true},
        }
    }
}
//刷新推荐好友
class RefreshRecommendReq
{
    constructor() {
    }
}
//一键添加好友
class OneKeyAddFriendReq
{
    constructor() {
        this.addNames = "";   //添加的名字们
        /**
         * @type {Friend}
         */
        this.adder = {};   //发起添加的角色信息
    }
    static fieldsDesc() {
        return {
            addNames: {type: Array, itemType:String, notNull: true},
            adder: {type:Object, notNull: true}
        };
    }
}

/////////////////////////////////返回////////////////////////////

//添加好友返回
class AddFriendRes
{
    constructor(friName) {
        this.friName = friName;
    }
}


//请求好友数据返回/有更新的好友信息/变化的体力情况
class ReqFriendsRes
{
    /**
     * @param {Friend[]} friends //更新的好友数据
     * @param {Number[]} collStam  //已领过赠送体力的id
     * @param {Object[]} unCollStam   //还可以领赠送体力的id
     * @param {Number[]} sendStam   //赠送他人的表
     * @param {Boolean} reset //是否重置
     */
    constructor(friends, collStam, unCollStam, sendStam, reset) {
        this.friends = friends;
        this.collStam = collStam;
        this.unCollStam = unCollStam;
        this.sendStam = sendStam;
        this.reset = reset;
    }
}
//推送新的添加好友请求
class PushAddFriend
{
    /**
     * @param {Friend[]} adders 添加者
     */
    constructor(adders){
        this.adders = adders;
    }
}

//处理好友请求返回
class HandleFriendRes
{
    /**
     * @param {Number} type  处理类型
     * @param {Friend} friData  从申请中添加的好友信息
     */
    constructor(type, friData) {
        this.type = type;
        this.friData = friData;
    }
}
//体力赠送返回
class SendStaminaRes
{
    /**
     * @param {Number} heroId 赠送id
     */
    constructor(heroId) {
        this.heroId = heroId;
    }
}
//体力领取返回
class GetStaminaRes
{
    /**
     * @param {Number} heroId
     */
    constructor(heroId) {
        this.heroId = heroId;
    }
}
//推送新的体力赠送
class PushNewStamina
{
    /**
     * @param {String} roleName 赠送者名字
     * @param {Number} heroId 接受者id
     * @param {Number} timeStamp 赠送时间
     */
    constructor(roleName, heroId, timeStamp) {
        this.roleName = roleName;
        this.heroId = heroId;
        this.timeStamp = timeStamp;
    }
}
//一键领取体力返回
class OnekeyGetStaminaRes
{
    /**
     * @param {Number[]} heroIds
     */
    constructor(heroIds){
        this.heroIds = heroIds;
    }
}
//删除好友返回
class DeleteFriendRes
{
    /**
     * @param {Number} heroId
     */
    constructor(heroId){
        this.heroId = heroId;
    }
}
//好友推荐返回
class FriendRecommendRes
{
    /**
     * @param {Friend[]} heroId
     * @param {Boolean} isFirst
     * @param {Number} upTime
     */
    constructor(recFriends, isFirst, upTime){
        this.recFriends = recFriends;
        this.isFirst = isFirst;
        this.upTime = upTime;
    }
}
//推荐好友返回
class RefreshRecommendRes
{
    /**
     * @param {Friend[]} heroId
     * @param {Number} upTime
     */
    constructor(recFriends, upTime){
        this.recFriends = recFriends;
        this.upTime = upTime;
    }
}
//一键添加好友返回
class OneKeyAddFriendRes
{
    constructor(addNames) {
        this.addNames = addNames;
    }
}

/////////////////////////////////导出////////////////////////////
exports.CmdIdsSocial = CmdIdsSocial;
exports.ResultCodeSocial = ResultCodeSocial;
exports.HandlerTypeEnum = HandlerTypeEnum;
exports.AddFriendReq = AddFriendReq;
exports.AddFriendRes = AddFriendRes;
exports.PushAddFriend = PushAddFriend;
exports.HandleFriendReq = HandleFriendReq;
exports.HandleFriendRes = HandleFriendRes;
exports.ReqFriendsReq = ReqFriendsReq;
exports.ReqFriendsRes = ReqFriendsRes;
exports.SendStaminaReq = SendStaminaReq;
exports.SendStaminaRes = SendStaminaRes;
exports.GetStaminaReq = GetStaminaReq;
exports.GetStaminaRes = GetStaminaRes;
exports.PushNewStamina = PushNewStamina;
exports.OnekeyGetStaminaReq = OnekeyGetStaminaReq;
exports.OnekeyGetStaminaRes = OnekeyGetStaminaRes;
exports.DeleteFriendReq = DeleteFriendReq;
exports.DeleteFriendRes = DeleteFriendRes;
exports.FriendRecommendReq = FriendRecommendReq;
exports.FriendRecommendRes = FriendRecommendRes;
exports.RefreshRecommendReq = RefreshRecommendReq;
exports.RefreshRecommendRes = RefreshRecommendRes;
exports.OneKeyAddFriendReq = OneKeyAddFriendReq;
exports.OneKeyAddFriendRes = OneKeyAddFriendRes;