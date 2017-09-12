"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var eliteLevelModule = require("../eliteLevel/eliteLevel");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var eliteLevelMessage = require("../netMessage/eliteLevelMessage");
var CmdIdsEliteLevel = require("../netMessage/eliteLevelMessage").CmdIdsEliteLevel;
var eliteLevelConfig = require("../gameConfig/eliteLevelConfig");
var levelConfig = require("../gameConfig/levelConfig");
var enProp = require("../enumType/propDefine").enProp;
var rewardConfig = require("../gameConfig/rewardConfig");

class EliteLevelsPart
{
    constructor(ownerRole, data)
    {
        /**
         * @type {EliteLevel[]}
         * @private
         */
        this._eliteLevels = [];

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});
        Object.defineProperty(this, "_levelIdEliteLevelMap", {enumerable: false, writable:true, value: {}});
        Object.defineProperty(this, "_dropRewards", {enumerable: false, writable:true, value: {}});

        try {
            var eliteLevels = data.eliteLevels || [];
            for (var i = 0; i < eliteLevels.length; ++i) {
                var eliteLevelData = eliteLevels[i];
                var eliteLevel = eliteLevelModule.createEliteLevel(eliteLevelData);
                if (!eliteLevel) {
                    throw new Error("创建EliteLevel失败");
                }

                //设置主人
                eliteLevel.setOwner(ownerRole);
                //加入列表
                this._eliteLevels.push(eliteLevel);
                this._levelIdEliteLevelMap[eliteLevel.levelId] = eliteLevel;
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
        var eliteLevels = this._eliteLevels;
        for (var i = 0; i < eliteLevels.length; ++i)
        {
            eliteLevels[i].release();
        }
        this._eliteLevels = [];
        this._levelIdEliteLevelMap = {};
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.eliteLevels = this._eliteLevels;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.eliteLevels = this._eliteLevels;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        rootObj.eliteLevels = this._eliteLevels;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        rootObj.eliteLevels = this._eliteLevels;
    }

    /**
     *
     * @param {EliteLevel} eliteLevel
     * @private
     */
    _addEliteLevel(eliteLevel)
    {
        var levelId = eliteLevel.levelId;
        this._eliteLevels.push(eliteLevel);
        this._levelIdEliteLevelMap[levelId] = eliteLevel;
    }

    /**
     *
     * @param {EliteLevel|number} val
     * @private
     */
    _removeEliteLevel(val)
    {
        var levelId = Object.isNumber(val) ? val : val.levelId;
        this._eliteLevels.removeValue(this._levelIdEliteLevelMap[levelId]);
        delete this._levelIdEliteLevelMap[levelId];
    }

    /**
     *
     * @param {number} levelId
     * @returns {EliteLevel|undefined}
     */
    getEliteLevelByLevelId(levelId)
    {
        return this._levelIdEliteLevelMap[levelId];
    }

    /**
     *
     * @returns {EliteLevel[]}
     */
    getEliteLevels()
    {
        return this._eliteLevels;
    }

    /**
     *
     * @param {string} roomId
     */
    initRewards(roomId)
    {
        var lvCfg = levelConfig.getLevelConfig(roomId);
        this._dropRewards = {};
        var strKey = ["monsterDrop", "specialDrop", "bossDrop", "boxDrop"];
        for(var i = 0; i < strKey.length; ++i) {
            var key = strKey[i];
            this._dropRewards[key] = [];
            var rewardCfg = lvCfg[key];
            for(var j = 0; j < rewardCfg.length; ++j)
            {
                var rewardSubCfg = rewardCfg[j];
                var randTimes = rewardSubCfg[1];
                var dropId = rewardSubCfg[0];
                for(var k = 0; k < randTimes; ++k)
                {
                    var rewardItems = rewardConfig.getRandomReward2(dropId);
                    if (rewardItems != null)
                        this._dropRewards[key] = this._dropRewards[key].concat(rewardItems);
                }
            }
        }

    }

    /**
     *
     * @returns {{}|*}
     */
    getDropReward()
    {
        return this._dropRewards;
    }

    clearDropReward()
    {
        this._dropRewards = {};
    }

    /**
     *
     * @param rewards
     * @returns {boolean}
     */
    checkRewards(rewards)
    {
        var mDrop = this._dropRewards.monsterDrop;
        var sDrop = this._dropRewards.specialDrop;
        var bDrop = this._dropRewards.bossDrop;
        var boxDrop = this._dropRewards.boxDrop;

        //被清过了？
        if (!mDrop || !sDrop || !bDrop || !boxDrop)
            return false;

        var bInclude = false;

        if (rewards.monsterItems.length > mDrop.length || rewards.specialItems.length > sDrop.length || rewards.bossItems.length > bDrop.length || rewards.boxItems.length > boxDrop.length) {
            logUtil.error("掉落作弊-------玩家 "+this._role.getString(enProp.name));
            return false;
        }
        for(var i = 0; i < rewards.monsterItems.length; i++)
        {
            bInclude = false;
            for(var k = 0; k < mDrop.length; k++) {
                if (mDrop[k].itemId === rewards.monsterItems[i].itemId && mDrop[k].num === rewards.monsterItems[i].num)
                {
                    bInclude = true;
                    mDrop.removeValue(mDrop[k]);
                    break;
                }
            }

            if (!bInclude)
            {
                logUtil.error("关卡掉落作弊 玩家:"+this._role.getString(enProp.name)+" itemId : "+rewards.monsterItems[i].itemId+" num :"+rewards.monsterItems[i].num )
                return false;
            }
        }

        for(var i = 0; i < rewards.specialItems.length; i++)
        {
            bInclude = false;
            for(var k = 0; k < sDrop.length; k++) {
                if (sDrop[k].itemId === rewards.specialItems[i].itemId && sDrop[k].num === rewards.specialItems[i].num)
                {
                    bInclude = true;
                    sDrop.removeValue(sDrop[k]);
                    break;
                }
            }

            if (!bInclude)
            {
                logUtil.error("关卡掉落作弊 玩家:"+this._role.getString(enProp.name)+" itemId : "+rewards.specialItems[i].itemId+" num :"+rewards.specialItems[i].num )
                return false;
            }
        }

        for(var i = 0; i < rewards.bossItems.length; i++)
        {
            bInclude = false;
            for(var k = 0; k < bDrop.length; k++) {
                if (bDrop[k].itemId === rewards.bossItems[i].itemId && bDrop[k].num === rewards.bossItems[i].num)
                {
                    bInclude = true;
                    bDrop.removeValue(bDrop[k]);
                    break;
                }
            }

            if (!bInclude)
            {
                logUtil.error("关卡掉落作弊 玩家:"+this._role.getString(enProp.name)+" itemId : "+rewards.bossItems[i].itemId+" num :"+rewards.bossItems[i].num )
                return false;
            }
        }

        for(var i = 0; i < rewards.boxItems.length; i++)
        {
            bInclude = false;
            for(var k = 0; k < boxDrop.length; k++) {
                if (boxDrop[k].itemId === rewards.boxItems[i].itemId && boxDrop[k].num === rewards.boxItems[i].num)
                {
                    bInclude = true;
                    boxDrop.removeValue(boxDrop[k]);
                    break;
                }
            }

            if (!bInclude)
            {
                logUtil.error("关卡掉落作弊 玩家:"+this._role.getString(enProp.name)+" itemId : "+rewards.boxItems[i].itemId+" num :"+rewards.boxItems[i].num )
                return false;
            }
        }

        return true;
    }

    /**
     *
     * @param {number} levelId
     * @return {EliteLevel} 返回EliteLevel对象
     */
    addEliteLevelWithLevelId(levelId)
    {
        return this.addEliteLevelWithData({
            "levelId" : levelId,
            "passed" : false,
            "starsInfo" : {},
            "enterTime" : 0,
            "count" : 0,
            "resetCount" : 0,
            "firstRewarded" : false
        });
    }

    /**
     *
     * @param {object} data
     * @return {EliteLevel} 返回EliteLevel对象
     */
    addEliteLevelWithData(data)
    {
        //必须有最基本数据
        //因为下面要获取eliteLevelId，所以要先判断
        if (!eliteLevelModule.isEliteLevelData(data))
            return null;

        var levelId = data.levelId;
        //已存在？不能添加
        if (this.getEliteLevelByLevelId(levelId))
            return null;

        var eliteLevel = eliteLevelModule.createEliteLevel(data);
        if (!eliteLevel)
            return null;

        //设置主人
        var roleCur = this._role;
        eliteLevel.setOwner(roleCur);

        //添加到列表
        this._addEliteLevel(eliteLevel);

        //检测条件
        if (!roleCur.canSyncAndSave())
            return eliteLevel;

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
        updateObj = {$push:{"eliteLevels":eliteLevel}};

        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new eliteLevelMessage.AddOrUpdateEliteLevelVo(true, eliteLevel);
        roleTop.sendEx(ModuleIds.MODULE_ELITE_LEVEL, CmdIdsEliteLevel.PUSH_ADD_OR_UPDATE_ELITE_LEVEL, netMsg);
        return eliteLevel;
    }


    //用于保存已在数据库的数据
    syncAndSaveEliteLevel(levelId)
    {
        var eliteLevel = this._levelIdEliteLevelMap[levelId];
        //不存在？不能继续
        if (!eliteLevel)
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


        queryObj = {"props.heroId":heroId, "eliteLevels":{$elemMatch:{"levelId":levelId}}};
        updateObj =  {$set: {"eliteLevels.$": eliteLevel}};
        col.updateOneNoThrow(queryObj, updateObj);


        //通知客户端
        var netMsg = new eliteLevelMessage.AddOrUpdateEliteLevelVo(false, eliteLevel);
        roleTop.sendEx(ModuleIds.MODULE_ELITE_LEVEL, CmdIdsEliteLevel.PUSH_ADD_OR_UPDATE_ELITE_LEVEL, netMsg);
        return true;
    }

}

exports.EliteLevelsPart = EliteLevelsPart;