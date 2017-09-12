"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var systemModule = require("../system/system");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var systemMessage = require("../netMessage/systemMessage");
var CmdIdsSystem = require("../netMessage/systemMessage").CmdIdsSystem;
var systemConfig = require("../gameConfig/systemConfig");
var systemMgr = require("../system/systemMgr");
var eventNames = require("../enumType/eventDefine").eventNames;

class SystemsPart
{
    constructor(ownerRole, data)
    {
        /**
         * @type {System[]}
         * @private
         */
        this._systems = [];

        /**
         *
         * @type {object.<string, number>}
         * @private
         */
        this._teaches = data.teaches || {};

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});
        Object.defineProperty(this, "_systemIdSystemMap", {enumerable: false, writable:true, value: {}});
        /**
         * 为了防止客户端恶意写入太多key，有数量上限
         * 这里先取key数量
         * @type {number}
         */
        Object.defineProperty(this, "_teachKeyCnt", {enumerable: false, writable:true, value: Object.keys(this._teaches).length});

        try {
            var systems = data.systems || [];
            for (var i = 0; i < systems.length; ++i) {
                var systemData = systems[i];
                var system = systemModule.createSystem(systemData);
                if (!system) {
                    throw new Error("创建System失败");
                }

                //设置主人
                system.setOwner(ownerRole);
                //加入列表
                this._systems.push(system);
                this._systemIdSystemMap[system.systemId] = system;
            }

            //添加事件处理
            var thisObj = this;
            ownerRole.addListener(function(eventName, context, notifier) {
                thisObj.onActiveEventFire();
            }, eventNames.LEVEL_UP);
            ownerRole.addListener(function(eventName, context, notifier) {
                thisObj.onActiveEventFire();
            }, eventNames.PASS_LEVEL);
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        var systems = this._systems;
        for (var i = 0; i < systems.length; ++i)
        {
            systems[i].release();
        }
        this._systems = [];
        this._systemIdSystemMap = {};
        this._teaches = {};
        this._teachKeyCnt = 0;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.systems = this._systems;
        rootObj.teaches = this._teaches;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.systems = this._systems;
        rootObj.teaches = this._teaches;
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
     *
     * @param {System} system
     * @private
     */
    _addSystem(system)
    {
        var systemId = system.systemId;
        this._systems.push(system);
        this._systemIdSystemMap[systemId] = system;
    }

    /**
     *
     * @param {System|number} val
     * @private
     */
    _removeSystem(val)
    {
        var systemId = Object.isNumber(val) ? val : val.systemId;
        this._systems.removeValue(this._systemIdSystemMap[systemId]);
        delete this._systemIdSystemMap[systemId];
    }

    /**
     *
     * @param {number} systemId
     * @returns {System|undefined}
     */
    getSystemBySystemId(systemId)
    {
        return this._systemIdSystemMap[systemId];
    }

    /**
     *
     * @param {number} systemId
     * @param {boolean} active
     * @param {boolean} notSync
     * @return {boolean} 如果添加成功就返回true
     */
    addSystemWithSystemId(systemId,active,notSync)
    {
        var data = {};
        data.systemId = systemId;
        data.active = active;
        return this.addSystemWithData(data,notSync);
    }

    /**
     *
     * @param {object} data
     * @param {boolean} notSync
     * @return {boolean} 如果添加成功就返回true
     */
    addSystemWithData(data, notSync)
    {
        //必须有最基本数据
        //因为下面要获取systemId，所以要先判断
        if (!systemModule.isSystemData(data))
            return false;

        var systemId = data.systemId;
        //已存在？不能添加
        if (this.getSystemBySystemId(systemId))
            return false;

        var system = systemModule.createSystem(data);
        if (!system)
            return false;

        //设置主人
        var roleCur = this._role;
        system.setOwner(roleCur);

        //添加到列表
        this._addSystem(system);

        //检测条件
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;
        queryObj = {"props.heroId":heroId};
        updateObj = {$push:{"systems":system}};

        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        if(!notSync)
        {
            var netMsg = new systemMessage.AddOrUpdateSystemVo(true, system);
            roleTop.sendEx(ModuleIds.MODULE_SYSTEM, CmdIdsSystem.PUSH_ADD_OR_UPDATE_SYSTEM, netMsg);
        }

        return true;
    }


    //用于保存已在数据库的系统数据
    syncAndSaveSystem(systemId, notSync)
    {
        var system = this._systemIdSystemMap[systemId];
        //不存在？不能继续
        if (!system)
            return false;

        //检测条件
        var roleCur = this._role;
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;

        queryObj = {"props.heroId":heroId, "systems":{$elemMatch:{"systemId":systemId}}};
        updateObj =  {$set: {"systems.$": system}};
        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        if(!notSync)
        {
            var netMsg = new systemMessage.AddOrUpdateSystemVo(false, system);
            roleTop.sendEx(ModuleIds.MODULE_SYSTEM, CmdIdsSystem.PUSH_ADD_OR_UPDATE_SYSTEM, netMsg);
        }

        return true;
    }

    onActiveEventFire()
    {
        //logUtil.info("active event fired");
        var systemCfgs = systemConfig.getSystemConfig();
        var systems = [];
        for(var k in systemCfgs)
        {
            var errObj = {};
            var system = systemMgr.checkActive(this._role, systemCfgs[k].id, errObj);
            if(system)
            {
                systems.push(system);
            }
        }
        if(systems.length>0)
        {
            var roleCur = this._role;
            var roleTop = roleCur.getOwner();
            var netMsg = new systemMessage.AddOrUpdateSystemsVo(systems);
            roleTop.sendEx(ModuleIds.MODULE_SYSTEM, CmdIdsSystem.PUSH_ADD_OR_UPDATE_SYSTEMS, netMsg);
        }
    }

    /**
     *
     * @param {string} key
     * @param {number} val
     * @param {boolean} [sync=false] - 一般用于GM命令，如果是普通修改，客户端会自己先行修改
     * @returns {number} 0表示无错误，1表示键名格式错误，2表示值类型错误，3表示记录的数量已达上限
     */
    setTeachData(key, val, sync)
    {
        if (!systemMgr.isTeachKeyOK(key))
            return 1;
        if (!Object.isNumber(val))
            return 2;

        var oldVal = this._teaches[key];
        if (oldVal == undefined)
        {
            if (this._teachKeyCnt >= systemMgr.TEACH_KEY_MAC_CNT)
                return 3;
            ++this._teachKeyCnt;
        }
        else if (oldVal === val)
        {
            return 0;
        }

        //修改内存数据
        this._teaches[key] = val;

        //修改数据库
        var roleCur = this._role;
        var userId = roleCur.getUserId();
        var heroId = roleCur.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj = {"props.heroId":heroId};
        var updateObj = {$set:{["teaches." + key]:val}};
        col.updateOneNoThrow(queryObj, updateObj);

        if (sync)
        {
            var syncMsg = new systemMessage.PushSetTeachDataVo(key, val);
            roleCur.sendEx(ModuleIds.MODULE_SYSTEM, CmdIdsSystem.PUSH_SET_TEACH_DATA, syncMsg);
        }
        return 0;
    }

    /**
     *
     * @param {boolean} [sync=false] - 一般用于GM命令，如果是普通修改，客户端会自己先行修改
     */
    clearTeachData(sync)
    {
        this._teachKeyCnt = 0;
        this._teaches = {};

        //修改数据库
        var roleCur = this._role;
        var userId = roleCur.getUserId();
        var heroId = roleCur.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj = {"props.heroId":heroId};
        var updateObj = {$set:{"teaches":{}}};
        col.updateOneNoThrow(queryObj, updateObj);

        if (sync)
        {
            roleCur.sendEx(ModuleIds.MODULE_SYSTEM, CmdIdsSystem.PUSH_CLEAR_TEACH_DATA, null);
        }
    }
}

exports.SystemsPart = SystemsPart;