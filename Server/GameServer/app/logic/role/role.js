"use strict";

var appCfg = require("../../../config");
var logUtil = require("../../libs/logUtil");
var dateUtil = require("../../libs/dateUtil");
var eventMgr = require("../../libs/eventMgr");
var timerMgr = require("../../libs/timerMgr");
var Message = require("../../libs/message").Message;
var roleMgr = require("./roleMgr");
var enProp = require("../enumType/propDefine").enProp;
var PropPart = require("./propPart").PropPart;
var ItemsPart = require("../item/itemsPart").ItemsPart;
var PetsPart = require("../pet/petsPart").PetsPart;
var PetSkillsPart = require("../pet/petSkillsPart").PetSkillsPart;
var TalentsPart = require("../pet/talentsPart").TalentsPart;
var PetBondPart = require("../pet/petBondPart").PetBondPart;
var PetPosPart = require("../pet/petPosPart").PetPosPart;
var LevelsPart = require("../level/levelsPart").LevelsPart;
var EquipsPart = require("../equip/equipsPart").EquipsPart;
var ActivityPart = require("../activity/activityPart").ActivityPart;
var WeaponPart = require("../weapon/weaponPart");
var OpActivityPart=require("../opActivity/opActivityPart");
var MailPart = require("../mail/mailPart").MailPart;
var SystemsPart = require("../system/systemsPart").SystemsPart;
var FlamesPart = require("../flame/flamesPart").FlamesPart;
var TaskPart=require("../task/taskPart");
var SocialPart = require("../social/socialPart").SocialPart;
var CorpsPart = require("../corps/corpsPart").CorpsPart;
var ShopsPart = require("../exchangeShop/shopsPart").ShopsPart;
var EliteLevelsPart = require("../eliteLevel/eliteLevelsPart").EliteLevelsPart;
var PetFormationsPart = require("../pet/petFormationsPart").PetFormationsPart;
var TreasurePart = require("../treasure/treasurePart").TreasurePart;
var isServerNoResend = require("../netMessage/netMsgConst").isServerNoResend;
var globalServerAgent = require("../http/globalServerAgent");
var roleConfig = require("../gameConfig/roleConfig");
var valueConfig = require("../gameConfig/valueConfig");
var eventNames = require("../enumType/eventDefine").eventNames;
var adminServerAgent = require("../http/adminServerAgent");

//客户端意外断线，Role保持在服务器内存，但要一定时间后可能要自动删除
const TIMER_ID_OFFLINE_DESTROY  = 1;
const TIMER_INV_OFFLINE_DESTROY = 1000 * 60 * 30; //30分钟后如果用户没有上线就删除

const ROLE_FLAG_UNKNOWN     = 0x0;                      //未知
const ROLE_FLAG_HERO        = 0x1;                      //是主角
const ROLE_FLAG_PET         = 0x2;                      //是宠物

/**
 * 注意Role（角色）可以是主角（Hero，可自动存盘有宠物）、宠物（Pet，如果主人是主角，可自动存盘，不能有宠物）、未知（Unknown）
 */
class Role
{
    constructor(data)
    {
        /**
         * 主角ID，非主角这个值为0
         * @type {number}
         */
        Object.defineProperty(this, "_heroId", {enumerable: false, value: data.props.heroId || 0});
        /**
         * 角色类型
         * 这个语句要放在上面语句的下面
         * @type {number}
         */
        Object.defineProperty(this, "_roleFlag", {enumerable: false, writable: true, value: (Object.isNumber(this._heroId) && this._heroId !== 0 ? ROLE_FLAG_HERO : ROLE_FLAG_UNKNOWN)});
        /**
         * 主人，不过有主人不代表本Role就是主人的宠物
         * 主角的主人就是自己
         * 这个语句要放在上面语句的下面
         * @type {Role|null}
         */
        Object.defineProperty(this, "_owner", {enumerable: false, writable: true, value: (this.isHero() ? this : null)});
        /**
         * 放置部件的数组，用于遍历
         * @type {Array}
         */
        Object.defineProperty(this, "_parts", {enumerable: false, writable: true, value: []});
        /**
         * 判断过程中，不能执行某些代码，这里加个标记
         * @type {boolean}
         */
        Object.defineProperty(this, "_buildOK", {enumerable: false, writable: true, value: false});

        try {
            this._props = new PropPart(this, data); this._parts.push(this._props);
            this._equips = new EquipsPart(this, data); this._parts.push(this._equips);

            //主角有物品、宠物、会话等模块
            if (this.isHero()) {
                this._items = new ItemsPart(this, data); this._parts.push(this._items);
                this._systems = new SystemsPart(this,data); this._parts.push(this._systems);   //系统开启模块放在前面，有些系统需要检测是否已开启
                this._pets = new PetsPart(this, data); this._parts.push(this._pets);
                this._levelInfo = new LevelsPart(this, data); this._parts.push(this._levelInfo);
                this._mails = new MailPart(this, data); this._parts.push(this._mails);          //在活动部件前初始化 活动部件会有隔天补奖励需求
                this._actProps = new ActivityPart(this, data); this._parts.push(this._actProps);
                this._weapons = new WeaponPart(this,data); this._parts.push(this._weapons);
                this._opActivity=new OpActivityPart(this,data); this._parts.push(this._opActivity);
                this._flames = new FlamesPart(this,data); this._parts.push(this._flames);
                this._task=new TaskPart(this,data); this._parts.push(this._task);
				this._social = new SocialPart(this, data); this._parts.push(this._social);
                this._corps = new CorpsPart(this, data); this._parts.push(this._corps);
                this._shop = new ShopsPart(this, data); this._parts.push(this._shop);
                this._eliteLevels = new EliteLevelsPart(this, data); this._parts.push(this._eliteLevels);
                this._petFormations = new PetFormationsPart(this, data); this._parts.push(this._petFormations);
                this._treasure = new TreasurePart(this, data); this._parts.push(this._treasure);
				
                /**
                 * @type {ClientSession|null}
                 */
                Object.defineProperty(this, "_session", {enumerable: false, writable: true, value: null});
                /**
                 * @type {Message[]}
                 */
                Object.defineProperty(this, "_pendingMsgList", {enumerable: false, writable: true, value: []});
            }
            else
            {
                this._petSkills = new PetSkillsPart(this, data); this._parts.push(this._petSkills);
                this._talents = new TalentsPart(this, data); this._parts.push(this._talents);
                this._petBond = new PetBondPart(this, data); this._parts.push(this._petBond); //只计算，无数据
                this._petPos = new PetPosPart(this, data); this._parts.push(this._petPos); //只计算，无数据
            }
            if (this.isHero()) { //宠物等主人的所有宠物都载入完毕后再计算基础属性(因为涉及到羁绊，出战)
                this._pets.freshPetProp();
                this._props.freshBaseProp();
            }
        }
        catch (err) {
            //先清理已创建的部件
            if (this._props)
            {
                try {
                    this._props.release();
                }
                catch (err) {
                }
                this._props = null;
            }
            if (this._equips)
            {
                try {
                    this._equips.release();
                }
                catch (err) {
                }
                this._equips = null;
            }
            if (this._pets)
            {
                try {
                    this._pets.release();
                }
                catch (err) {
                }
                this._pets = null;
            }
            if (this._petSkills)
            {
                try {
                    this._petSkills.release();
                }
                catch (err) {
                }
                this._petSkills = null;
            }
            if (this._talents)
            {
                try {
                    this._talents.release();
                }
                catch (err) {
                }
                this._talents = null;
            }
            if (this._petBond)
            {
                try {
                    this._petBond.release();
                }
                catch (err) {
                }
                this._petBond = null;
            }
            if (this._petPos)
            {
                try {
                    this._petPos.release();
                }
                catch (err) {
                }
                this._petPos = null;
            }
            if (this._items)
            {
                try {
                    this._items.release();
                }
                catch (err) {
                }
                this._items = null;
            }
            if (this._levelInfo) {
                try {
                    this._levelInfo.release();
                }
                catch (err) {
                }
                this._levelInfo = null;
            }
            if (this._actProps) {
                try {
                    this._actProps.release();
                }
                catch (err) {
                }
                this._actProps = null;
            }
            if (this._weapons) {
                try {
                    this._weapons.release();
                }
                catch (err) {
                }
                this._weapons = null;
            }
            if (this._systems) {
                try {
                    this._systems.release();
                }
                catch (err) {
                }
                this._systems = null;
            }
            if (this._mails) {
                try {
                    this._mails.release();
                }
                catch (err) {
                }
                this._mails = null;
            }
            if (this._opActivity) {
                try {
                    this._opActivity.release();
                }
                catch (err) {
                }
                this._opActivity = null;
            }
            if (this._flames) {
                try {
                    this._flames.release();
                }
                catch (err) {
                }
                this._flames = null;
            }
            if (this._task) {
                try {
                    this._task.release();
                }
                catch (err) {
                }
                this._task = null;
            }
            if(this._social){
                try {
                    this._social.release();
                }
                catch (err) {
                }
                this._social = null;
            }
            if(this._corps){
                try {
                    this._corps.release();
                }
                catch (err) {
                }
                this._corps = null;
            }
            if(this._shop){
                try {
                    this._shop.release();
                }
                catch (err) {
                }
                this._shop = null;
            }
            if(this._eliteLevels){
                try {
                    this._eliteLevels.release();
                }
                catch (err) {
                }
                this._eliteLevels = null;
            }
            if(this._petFormations){
                try {
                    this._petFormations.release();
                }
                catch (err) {
                }
                this._petFormations = null;
            }
            if(this._treasure){
                try {
                    this._treasure.release();
                }
                catch (err) {
                }
                this._treasure = null;
            }
            this._parts = [];
            throw err;
        }
        finally
        {
            this._buildOK = true;
        }
    }

    isHero()
    {
        return (this._roleFlag & ROLE_FLAG_HERO) === ROLE_FLAG_HERO;
    }

    isGM()
    {
        return !!this._props.getNumber(enProp.gmFlag);
    }

    /**
     * 如果是神侍，而主人是机器人，也会返回true
     * @returns {boolean}
     */
    isRobot()
    {
        return this._heroId < 0 || (this.isPet() && this.getOwner().isRobot());
    }

    isBuildOK()
    {
        return this._buildOK;
    }

    /**
     * 是否真的离线（就是不是掉线，而是加载为离线角色）
     * @returns {boolean}
     */
    isOfflineReally()
    {
        return this._props.getNumber(enProp.offline) > 0;
    }

    /**
     * 从表面上看，是否离线，包括掉线和加载为离线角色
     * @returns {boolean}
     */
    isOfflineSeemingly()
    {
        return !!this._props.getNumber(enProp.offline);
    }

    /**
     * 获取上次“离线”时间
     * 如果当前在线，返回0
     * 如果是掉线，就返回最近掉线时间
     * 如果是离线，就返回上次下线时间
     * @returns {number}
     */
    getLastOfflineTime()
    {
        //如果有offline值，且不为0
        var offline = this._props.getNumber(enProp.offline);
        if (!!offline)
        {
            //小于0就是掉线，取反
            if (offline < 0)
                return -offline;
            //大于0就是真离线，取上次下线时间
            else if (offline > 0)
                return this._props.getNumber(enProp.lastLogout);
        }
        //其它情况返回0，表示当前在线
        return 0;
    }

    /**
     * 是否真实玩家的角色且在线（不包括掉线）
     * @returns {boolean}
     */
    isTrueHeroAndOnline()
    {
        return this.isHero() && !this.isRobot() && !this.isOfflineReally();
    }

    /**
     * 是否可以用延迟消息队列
     * @returns {boolean}
     */
    canUsePendingMsg()
    {
        //如果是真实玩家的非离线角色，那就可以把消息放到延迟队列里
        return this.isTrueHeroAndOnline();
    }

    isPet()
    {
        return (this._roleFlag & ROLE_FLAG_PET) === ROLE_FLAG_PET;
    }

    getGUID()
    {
        return this._props.getString(enProp.guid);
    }

    /**
     * 快速获取主角ID，主角才有
     * @returns {number}
     */
    getHeroId()
    {
        return this._props.getNumber(enProp.heroId);
    }

    /**
     * 快速获取账号ID，主角才有，一般用于存盘
     * @returns {string}
     */
    getUserId()
    {
        return this._props.getString(enProp.userId);
    }

    /**
     * 
     * @returns {*|RoleConfig}
     */
    getRoleCfg(){
        return roleConfig.getRoleConfig(this._props.getString(enProp.roleId));
    }
    /**
     * 标记为宠物
     */
    markAsPetType()
    {
        if ((this._roleFlag & ROLE_FLAG_HERO) === 0)
            this._roleFlag = ROLE_FLAG_PET;
        //主角不能变成宠物
        else
            logUtil.warn("当前Role类型不能切换为别的类型：" + this._roleFlag);
    }

    /**
     * 标记为未知类型
     */
    markAsUnknownType()
    {
        if ((this._roleFlag & ROLE_FLAG_HERO) === 0)
            this._roleFlag = ROLE_FLAG_UNKNOWN;
        //主角不能变成未知
        else
            logUtil.warn("当前Role类型不能切换为别的类型：" + this._roleFlag);
    }

    /**
     *
     * @param {Role} v
     */
    setOwner(v)
    {
        //主角不能设置主人
        if (this.isHero())
            logUtil.error("主角角色不能设置主人");
        else
            this._owner = v;
    }

    /**
     * 获取主人
     * 主角的主人就是自己
     * @returns {Role|null}
     */
    getOwner()
    {
        return this._owner;
    }

    /**
     *
     * @returns {ClientSession}
     */
    getSession()
    {
        return this._session;
    }

    /**
     * 仅用于ClientSession的setRole
     * @param {ClientSession} session
     * @param {boolean?} [timerDestroy=false] - 是否要开启定时删除
     */
    setSession(session, timerDestroy)
    {
        if (!this.isHero())
        {
            logUtil.error("非主角角色不能有会话");
            return;
        }

        this._session = session;

        if (session)
        {
            //尝试删除长时间掉线销毁定时器
            this.cancelTimerDestroy();
            //有连接了，离线时间设置为0
            this.clearOfflineTime();
        }
        else
        {
            //建立长时间掉线销毁定时器（一次性的）
            if (timerDestroy)
                this.startTimerDestroy();
            //没连接了，应该是掉线了，离线时间设置为负整数
            this.setOfflineTime(false);
            //发起一个事件
            this.fireEvent(eventNames.ROLE_LOSTCONN);
        }
    }

    /**
     * 设置离线时间，如果是真实离线，就是正数，掉线就是负数
     * @param {boolean?} realOffline
     */
    setOfflineTime(realOffline)
    {
        var val = dateUtil.getTimestamp() * (realOffline ? 1 : -1);
        this.setNumber(enProp.offline, val);
    }

    clearOfflineTime()
    {
        this.setNumber(enProp.offline, 0);
    }

    startTimerDestroy()
    {
        timerMgr.addTimer(this, TIMER_ID_OFFLINE_DESTROY, TIMER_INV_OFFLINE_DESTROY, true);
    }

    cancelTimerDestroy()
    {
        timerMgr.removeTimer(this, TIMER_ID_OFFLINE_DESTROY);
    }

    onTimer(timerID)
    {
        //如果这时会话还是空的话，那就删除自己
        if (timerID === TIMER_ID_OFFLINE_DESTROY && !this._session)
        {
            this.release();
        }
    }

    /**
     *
     * @param {Message} msgObj
     */
    send(msgObj)
    {
        //有连接就发给连接
        if (this._session)
        {
            this._session.send(msgObj);
        }
        //没连接？如果可以用延迟队列才让用
        else if (this.canUsePendingMsg())
        {
            //不需要重发的，离线就不加入延迟队列
            if (!isServerNoResend(msgObj.getModule(), msgObj.getCommand()))
                this._pendingMsgList.push(msgObj);
        }
    }

    /**
     *
     * @param {number} module
     * @param {number} cmd
     * @param {*} body
     */
    sendEx(module, cmd, body)
    {
        if (this._session)
        {
            this._session.sendEx(module, cmd, body);
        }
        //没连接？如果可以用延迟队列才让用
        else if (this.canUsePendingMsg() && this._pendingMsgList)
        {
            //不需要重发的，离线就不加入延迟队列
            if (!isServerNoResend(module, cmd)) {
                var msgObj = Message.newRequest(module, cmd, body);
                this._pendingMsgList.push(msgObj);
            }
        }
    }

    /**
     * 如果向多个连接发送同样的数据，就不要重复各种写入、加密、压缩、生成校验码
     * @param {IOBuffer} ioBuf - 经过各种加长度、校验和、加密、压缩的buffer
     * @param {Message} msgObj - 为了能断线重连时重发，还是要带上这个，不过只是简单加入队列
     */
    sendBufDirectly(ioBuf, msgObj)
    {
        //有连接就发给连接
        if (this._session)
        {
            this._session.sendBufDirectly(ioBuf, msgObj);
        }
        //没连接？如果可以用延迟队列才让用
        else if (this.canUsePendingMsg())
        {
            //不需要重发的，离线就不加入延迟队列
            if (!isServerNoResend(msgObj.getModule(), msgObj.getCommand()))
                this._pendingMsgList.push(msgObj);
        }
    }

    /**
     *
     * @param {Message[]} msgList
     */
    addPendingMsgList(msgList)
    {
        //使用连接的方式
        if (msgList && msgList.length > 0)
            this._pendingMsgList = this._pendingMsgList.concat(msgList);
    }

    /**
     * 自动重连之后发送延时网络消息
     */
    sendPendingMsgList()
    {
        //有新连接了？有延迟队列？那就发送吧
        var pending = this._pendingMsgList;
        var pendLen = pending.length;
        if (pendLen > 0)
        {
            //有可能重发过程中会连接失效，这时用this.send可以把旧的包再次加入重发队列
            //另外，先清空pendingMsgList，因为重发过程中如果失败会concat
            this._pendingMsgList = [];
            for (var i = 0; i < pendLen; ++i)
            {
                var msgObj = pending[i];
                if (!isServerNoResend(msgObj.getModule(), msgObj.getCommand()))
                    this.send(msgObj);
            }
        }
    }

    /**
     *
     * @returns {PropPart}
     */
    getPropsPart()
    {
        return this._props;
    }

    /**
     *
     * @returns {ItemsPart}
     */
    getItemsPart()
    {
        return this._items;
    }

    /**
     *
     * @returns {PetsPart}
     */
    getPetsPart()
    {
        return this._pets;
    }

    /**
     *
     * @returns {PetSkillsPart}
     */
    getPetSkillsPart()
    {
        return this._petSkills;
    }

    /**
     *
     * @returns {TalentsPart}
     */
    getTalentsPart()
    {
        return this._talents;
    }

    /**
     *
     * @returns {PetBondPart}
     */
    getPetBondPart()
    {
        return this._petBond;
    }

    /**
     *
     * @returns {PetPosPart}
     */
    getPetPosPart()
    {
        return this._petPos;
    }

    /**
     *
     * @returns {EquipsPart}
     */
    getEquipsPart()
    {
        return this._equips;
    }

    /**
     *
     * @returns {LevelsPart}
     */
    getLevelsPart()
    {
        return this._levelInfo;
    }

    /**
     *
     * @returns {ActivityPart}
     */
    getActivityPart()
    {
        return this._actProps;
    }

    /**
     *
     * @returns {WeaponPart}
     */
    getWeaponPart()
    {
        return this._weapons;
    }

    /**
     *
     * @returns {SystemsPart}
     */
    getSystemsPart()
    {
        return this._systems;
    }
    /**
     *
     * @returns {MailPart}
     */
    getMailPart()
    {
        return this._mails;
    }

    /**
     *
     * @returns {OpActivityPart}
     */
    getOpActivityPart()
    {
        return this._opActivity;
    }

    /**
     *
     * @returns {FlamesPart}
     */
    getFlamesPart()
    {
        return this._flames;
    }

    /**
     *
     * @returns {TaskPart}
     */
    getTaskPart()
    {
        return this._task;
    }
	 /**
     *
     * @returns {SocialPart}
     */
    getSocialPart()
    {
        return this._social;
    }
    /**
     *
     * @returns {CorpsPart}
     */
    getCorpsPart()
    {
        return this._corps;
    }

    /**
     *
     * @returns {ShopsPart}
     */
    getShopsPart()
    {
        return this._shop;
    }

    /**
     *
     * @returns {EliteLevelsPart}
     */
    getEliteLevelsPart()
    {
        return this._eliteLevels;
    }

    /**
     *
     * @returns {PetFormationsPart}
     */
    getPetFormationsPart()
    {
        return this._petFormations;
    }

    /**
     *
     * @returns {TreasurePart}
     */
    getTreasurePart()
    {
        return this._treasure;
    }

    /**
     * 获取所有部件
     * @returns {Array}
     */
    getParts()
    {
        return this._parts;
    }

    getDBData()
    {
        /**
         *
         * @type {FullRoleInfoVo}
         */
        var retObj = {};
        this._props.getDBData(retObj);
        this._equips.getDBData(retObj);
        if (this._pets)
            this._pets.getDBData(retObj);
        if (this._items)
            this._items.getDBData(retObj);
        if (this._levelInfo)
            this._levelInfo.getDBData(retObj);
        if (this._petSkills)
            this._petSkills.getDBData(retObj);
        if (this._talents)
            this._talents.getDBData(retObj);
        if (this._actProps)
            this._actProps.getDBData(retObj);
        if (this._weapons)
            this._weapons.getDBData(retObj);
        if (this._systems)
            this._systems.getDBData(retObj);
        if (this._mails)
            this._mails.getDBData(retObj);
        if (this._opActivity)
            this._opActivity.getDBData(retObj);
        if (this._flames)
            this._flames.getDBData(retObj);
        if (this._task)
            this._task.getDBData(retObj);
        if (this._social)
            this._social.getDBData(retObj);
        if (this._corps)
            this._corps.getDBData(retObj);
        if (this._shop)
            this._shop.getDBData(retObj);
        if (this._eliteLevels)
            this._eliteLevels.getDBData(retObj);
        if (this._petFormations)
            this._petFormations.getDBData(retObj);
        if (this._treasure)
            this._treasure.getDBData(retObj);
        return retObj;
    }

    /**
     * 获取私有网络数据，发给自己
     * @returns {FullRoleInfoVo}
     */
    getPrivateNetData()
    {
        /**
         *
         * @type {FullRoleInfoVo}
         */
        var retObj = {};
        this._props.getPrivateNetData(retObj);
        this._equips.getPrivateNetData(retObj);
        if (this._pets)
            this._pets.getPrivateNetData(retObj);
        if (this._items)
            this._items.getPrivateNetData(retObj);
        if (this._levelInfo)
            this._levelInfo.getPrivateNetData(retObj);
        if (this._petSkills)
            this._petSkills.getPrivateNetData(retObj);
        if (this._talents)
            this._talents.getPrivateNetData(retObj);
	    if (this._actProps)
            this._actProps.getPrivateNetData(retObj);
        if (this._weapons)
            this._weapons.getPrivateNetData(retObj);
        if (this._systems)
            this._systems.getPrivateNetData(retObj);
        if (this._mails)
            this._mails.getPrivateNetData(retObj);
        if (this._opActivity)
            this._opActivity.getPrivateNetData(retObj);
        if (this._flames)
            this._flames.getPrivateNetData(retObj);
        if (this._task)
            this._task.getPrivateNetData(retObj);
        if (this._social)
            this._social.getPrivateNetData(retObj);
        if (this._corps)
            this._corps.getPrivateNetData(retObj);
        if (this._shop)
            this._shop.getPrivateNetData(retObj);
        if (this._eliteLevels)
            this._eliteLevels.getPrivateNetData(retObj);
        if (this._petFormations)
            this._petFormations.getPrivateNetData(retObj);
        if (this._treasure)
            this._treasure.getPrivateNetData(retObj);
        return retObj;
    }

    getPublicNetData()
    {
        /**
         *
         * @type {FullRoleInfoVo}
         */
        var retObj = {};
        this._props.getPublicNetData(retObj);
        this._equips.getPublicNetData(retObj);
        return retObj;
    }

    getProtectNetData()
    {
        /**
         *
         * @type {FullRoleInfoVo}
         */
        var retObj = {};
        this._props.getProtectNetData(retObj);
        this._equips.getProtectNetData(retObj);
        if (this._pets)
            this._pets.getProtectNetData(retObj);
        if (this._petSkills)
            this._petSkills.getProtectNetData(retObj);
        if (this._talents)
            this._talents.getProtectNetData(retObj);
        if (this._weapons)
            this._weapons.getProtectNetData(retObj);
        if (this._petFormations)
            this._petFormations.getProtectNetData(retObj);
        if (this._flames)
            this._flames.getProtectNetData(retObj);
        if (this._treasure)
            this._treasure.getProtectNetData(retObj);
        if (this._actProps)
            this._actProps.getProtectNetData(retObj);
        return retObj;
    }

    getNumber(enumVal)
    {
        return this._props.getNumber(enumVal);
    }

    getString(enumVal)
    {
        return this._props.getString(enumVal);
    }

    setNumber(enumVal, newVal, nosync)
    {
        this._props.setNumber(enumVal, newVal, nosync);
    }

    setString(enumVal, newVal, nosync)
    {
        this._props.setString(enumVal, newVal, nosync);
    }

    addNumber(enumVal, delta, nosync)
    {
        this._props.addNumber(enumVal, delta, nosync);
    }

    addExp(exp)
    {
        this._props.addExp(exp);
    }

    updateGlobalServerInfo()
    {
        //更新全局服的最后登录时间
        var channelId = this.getString(enProp.channelId);
        var userId = this.getString(enProp.userId);
        var guid = this.getString(enProp.guid);
        var name = this.getString(enProp.name);
        var level = this.getNumber(enProp.level);
        var roleId = this.getString(enProp.roleId);
        var heroId = this.getNumber(enProp.heroId);
        var serverId = appCfg.serverId;
        var callback = function(ok, msg, cxt){};
        globalServerAgent.updateRoleInfo(channelId, userId, guid, name, level, roleId, heroId, serverId, callback);
    }

    getStamina()
    {
        return this._props.getStamina();
    }

    addStamina(delta)
    {
        this._props.addStamina(delta);
    }

    canSyncAndSave()
    {
        var owner = this.getOwner();
        return owner && owner.isHero() && (this === owner || this.isPet());
    }

    startBatch()
    {
        this._props.startBatch();
    }

    endBatch()
    {
        this._props.endBatch();
    }

    addListener(observer, eventName)
    {
        eventMgr.addListener(observer, eventName, this);
    }

    removeListener(observer, eventName)
    {
        eventMgr.removeListener(observer, eventName, this);
    }

    fireEvent(eventName, context)
    {
        eventMgr.fire(eventName, context, this);
    }

    release()
    {
        roleMgr.removeRoleByGUID(this._props.getString(enProp.guid));
    }

    logLogout()
    {
        //记录统计日志
        var session = this.getSession();
        /** @type {AccountInfo} */
        var accountInfo = session ? session.getAccountInfo() : null;
        var curTime = dateUtil.getTimestamp();
        var log_ip = session && session.getRemoteFamily() === "IPv4" ? session.getRemoteAddress() : "";
        var log_ipv6 = session && session.getRemoteFamily() === "IPv6" ? session.getRemoteAddress() : "";
        var log_device_model = accountInfo ? accountInfo.deviceModel : "";
        var log_os_name = accountInfo ? accountInfo.osName : "";
        var log_os_ver = "";
        var log_mac_addr = accountInfo ? accountInfo.macAddr : "";
        var log_udid = "";
        var log_app_channel = accountInfo ? accountInfo.channelId : "";
        var log_app_ver = accountInfo ? accountInfo.clientVer : "";
        var log_account_id = this.getString(enProp.userId);
        var log_role_id = this.getNumber(enProp.heroId).toString();
        var log_role_name = this.getString(enProp.name);
        var log_create_time = this.getNumber(enProp.createTime) * 1000;
        var log_network = accountInfo ? accountInfo.network : "";
        var log_isp = "";
        var log_device_height = accountInfo ? accountInfo.screenHeight : 0;
        var log_device_width = accountInfo ? accountInfo.screenWidth : 0;
        var log_role_level = this.getNumber(enProp.level);
        var log_exp = this.getNumber(enProp.exp);
        var log_logout_time = curTime * 1000;
        var log_online_time = (curTime - this.getNumber(enProp.lastLogin)) * 1000;
        var log_scene = "";
        var log_axis = "";
        var log_money_sum = 0; //TODO
        var log_exp_sum = 0; //TODO
        var log_vip_level = this.getNumber(enProp.vipLv);
        adminServerAgent.logLogoutRole(log_ip, log_ipv6, log_device_model, log_os_name, log_os_ver, log_mac_addr, log_udid, log_app_channel, log_app_ver, log_account_id, log_role_id, log_role_name, log_create_time,
            log_network, log_isp, log_device_height, log_device_width, log_role_level, log_exp, log_logout_time, log_online_time, log_scene, log_axis, log_money_sum, log_exp_sum, log_vip_level);
    }

    /**
     * 注意：只给roleMgr调用
     */
    destroy()
    {
        // 如果是真实玩家的非离线角色
        // 那就记录下线时间，掉线不算下线
        if (this.isTrueHeroAndOnline())
        {
            this.setNumber(enProp.lastLogout, dateUtil.getTimestamp());
            this.fireEvent(eventNames.ROLE_LOGOUT);

            //记录日志
            this.logLogout();
        }

        if (this._props)
            this._props.release();
	    if (this._equips)
            this._equips.release();
        if (this._items)
            this._items.release();
        if (this._pets)
            this._pets.release();
        if (this._petSkills)
            this._petSkills.release();
        if (this._talents)
            this._talents.release();
        if (this._petBond)
            this._petBond.release();
        if (this._petPos)
            this._petPos.release();
        if (this._actProps)
            this._actProps.release();
        if (this._levelInfo)
            this._levelInfo.release();
        if (this._weapons)
            this._weapons.release();
        if (this._systems)
            this._systems.release();
        if (this._mails)
            this._mails.release();
        if (this._opActivity)
            this._opActivity.release();
        if (this._flames)
            this._flames.release();
        if (this._task)
            this._task.release();
		if (this._social)
            this._social.release();
        if (this._corps)
            this._corps.release();
        if (this._shop)
            this._shop.release();
        if (this._eliteLevels)
            this._eliteLevels.release();
        if (this._petFormations)
            this._petFormations.release();
        if (this._treasure)
            this._treasure.release();
        this._parts = [];
        eventMgr.removeNotifier(this);
        timerMgr.removeTimer(this);
        this._session = null;
        this._pendingMsgList = [];
        this._owner = null;
    }
}

exports.Role = Role;