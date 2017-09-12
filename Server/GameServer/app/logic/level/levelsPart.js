/**
 * Created by pc20 on 2016/2/18.
 */
"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var levelMessage = require("../netMessage/levelMessage");
var levelCfg = require("../gameConfig/levelConfig");
var dateUtil = require("../../libs/dateUtil");
var enProp = require("../enumType/propDefine").enProp;
var rewardConfig = require("../gameConfig/rewardConfig");
var gameConfig = require("../gameConfig/gameConfig");
var rankTypes = require("../enumType/rankDefine").rankTypes;
var rankMgr = require("../rank/rankMgr");

class LevelsPart {

    constructor(role, data) {
        //掉落奖励保存
        this._dropRewards = {};
        this._levelInfo = data.levelInfo || {};
        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});
    }

    release() {
        this._dropRewards = {};
        this._levelInfo = {};
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj) {
        rootObj.levelInfo = this._levelInfo;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj) {
        rootObj.levelInfo = this._levelInfo;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        rootObj.levelInfo = null;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        rootObj.levelInfo = null;
    }

    /**
     *
     * @param {string} levelId
     * @returns {}
     */
    getLevelById(levelId) {
        return this._levelInfo.levels[levelId];
    }

    getLevelInfo()
    {
        return this._levelInfo;
    }


    /**
     * 获取星星数
     * @param {string}nodeId
     * @returns {number}
     */
    getStarsByNodeId(nodeId)
    {
        var starsNum = 0;
        var levels = this._levelInfo.levels;
        for(var id in levels) {
            var item1 = levels[id];
            if (item1.nodeId == nodeId) {
                var item2 = item1.starsInfo;
                for(var k in item2) {
                    starsNum += item2[k];
                }
            }
        }
        return starsNum;
    }

    /**
     * 获取全部星星数
     * @returns {number}
     */
    getAllStars()
    {
        var starsNum = 0;
        var levels = this._levelInfo.levels;
        for(var id in levels) {
            var item1 = levels[id];
            var item2 = item1.starsInfo;
            for(var k in item2) {
                starsNum += item2[k];
            }
        }
        return starsNum;
    }

    /**
     *
     * @param level
     */
    updateLevel(level) {
        this._levelInfo[level.roomId] = level;

        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        if (this._levelInfo.curNode == undefined || parseInt(this._levelInfo.curNode) < parseInt(level.nodeId))
            this._levelInfo.curNode = level.nodeId;
        col.updateOneNoThrow({"props.heroId": heroId}, {$set: {"levelInfo.curNode":this._levelInfo.curNode, ["levelInfo.levels." + level.roomId]: level}});

        //加入或更新排行
        rankMgr.addToRankByRole(rankTypes.levelStar, this._role);
    }

    updateLevelStarReward()
    {
        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        col.updateOneNoThrow({"props.heroId": heroId}, {$set: {"levelInfo.curNode":this._levelInfo.curNode, "levelInfo.starsReward": this._levelInfo.starsReward}});
    }

    /**
     *
     * @param {string} roomId
     * @returns {*|LevelVo}
     */
    addLevel(roomId) {
        var cfg = levelCfg.getLevelConfig(roomId);

        let level = new levelMessage.LevelVo();
        level.isWin = false;
        level.roomId = cfg.id;
        level.nodeId = cfg.roomNodeId;
        level.starsInfo = {};
        level.enterNum = 0;
        level.lastEnter = dateUtil.getTimestamp();


        //更新level信息
        this._levelInfo.levels[level.roomId] = level;

        this._levelInfo.curLevel = roomId;

        //更新数据库
        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        col.updateOneNoThrow({"props.heroId": heroId}, {$set: {"levelInfo.curLevel": level.roomId, "levelInfo.curNode": level.roomNodeId, ["levelInfo.levels." + level.roomId]: level}});

        return level;
    }

    /**
     *
     * @param {number} num
     */
    openLevel(num)
    {
        var room = gameConfig.getCsv("room");

        if (num == -1) {
            num = 100;
        }
        var curLevelId = "";
        var curNodeId = "";
        for(var i = 1; i <= num; i ++) {
            var cfg = room[i+""];
            if (cfg != null) {
                let level = this._levelInfo.levels[cfg.id];
                if(level == null) {
                    level = new levelMessage.LevelVo();
                    level.isWin = true;
                    level.roomId = cfg.id;
                    level.nodeId = cfg.roomNodeId;
                    level.starsInfo = {};
                    var taskIDs = cfg.taskId.split('|');
                    for (var k = 0; k < taskIDs.length; k++)
                    {
                        level.starsInfo[parseInt(taskIDs[k])] = 1;
                    }
                    level.enterNum = 1;
                    level.lastEnter = dateUtil.getTimestamp();
                    this._levelInfo.curLevel = cfg.id;
                    this._levelInfo.levels[level.roomId] = level;
                }
                else
                {
                    level.isWin = true;
                }
                curLevelId = cfg.id;
                curNodeId = cfg.roomNodeId;
            }
            else
            {
                break;
            }
        }

        //更新数据库
        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        col.updateOneNoThrow({"props.heroId": heroId}, {$set: {"levelInfo.curLevel": curLevelId, "levelInfo.curNode": curNodeId, ["levelInfo.levels"]: this._levelInfo.levels}});

        //加入或更新排行
        rankMgr.addToRankByRole(rankTypes.levelStar, this._role);
    }

    /**
     *
     * @param {string} roomId
     */
    onEnterLevel(roomId)
    {
        var lvCfg = levelCfg.getLevelConfig(roomId);
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

    clearLevelInfo() {
        this._levelInfo = {
            curLevel: "0",
            curNode: "0",
            starsReward: {},
            levels: {}
        };
        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        col.updateOneNoThrow({"props.heroId": heroId}, {$set: {"levelInfo": this._levelInfo}});

        //加入或更新排行
        rankMgr.addToRankByRole(rankTypes.levelStar, this._role);
    }

}

exports.LevelsPart = LevelsPart;
