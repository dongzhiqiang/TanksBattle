"use strict";

var dbUtil = require("../../libs/dbUtil");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var activityMessage = require("../netMessage/activityMessage");
var CmdIdsActivity = require("../netMessage/activityMessage").CmdIdsActivity;
var actPropDefine = require("../enumType/activityPropDefine");
var dateUtil = require("../../libs/dateUtil");
var warriorTriedConfig =  require("../gameConfig/warriorTriedConfig");
var enProp = require("../enumType/propDefine").enProp;
var enItemId = require("../enumType/globalDefine").enItemId;
var rewardConfig = require("../../logic/gameConfig/rewardConfig");
var WarTriedBaseConfig = require("../../logic/gameConfig/warriorTriedConfig");
var eventNames = require("../enumType/eventDefine").eventNames;
var treasureRobUtil = require("./treasureRob/treasureRobUtil");
var rankMgr = require("../rank/rankMgr");
var treasureRobConfig = require("../gameConfig/treasureRobConfig");
var rankTypes = require("../enumType/rankDefine").rankTypes;
var appUtil = require("../../libs/appUtil");
var systemMgr = require("../system/systemMgr");
var enSystemId = require("../enumType/systemDefine").enSystemId;
var mailMgr = require("../mail/mailMgr");
var towerConfig = require("../gameConfig/prophetTowerConfig");

////////////////////////////////////////
const enActProp = actPropDefine.enActProp;
const MaxWarriorLevel = 4;

/**
 *
 * @type {object.<number, string>}
 */
var propNameMap = {};
for (var propName in enActProp)
{
    propNameMap[enActProp[propName]] = propName;
}
////////////////////////////////////////
/**
 * 定义服务器数据的字段，方便提示
 * @typedef {Object} ActivityServerData
 * @property {number} goldLevelCurMode - 当前正在打的金币副本难度
 * @property {number} hadesLevelCurMode - 当前正在打的哈迪斯副本难度
 * @property {number[]} arenaCHeroIds - 选中的竞技场对手的主角ID列表，除非重新登录、比赛胜了，一般不刷新它
 * @property {number} arenaCTime - 竞技场对手的主角ID列表生成时间
 * @property {number} arenaCurCHeroId - 竞技场当前对手的主角ID
 */

class ActivityPart
{
    /**
     *
     * @param {Role} role
     * @param {object.<string, *>} data
     */
    constructor(role, data)
    {
        /**
         *
         * @type {object}
         * @private
         */
        this._actProps = data.actProps || {};
        /**
         *
         * @type {ArenaLogItemVo[]}
         * @private
         */
        this._arenaLog = data.arenaLog || [];
        /**
         * 不用存盘的纯服务器数据
         * @type {ActivityServerData}
         * @private
         */
        this._serverData = {};

        /**
         * 勇士试炼的相关信息
         * @type {WarriorTried}
         */
        this._warriorTried = data.warriorTried || {};

        /**
         *预言者之塔相关信息
         * @type {ProphetTower}
         */
        this._prophetTower = data.prophetTower || {};

        /**
         *神器抢夺相关信息
         * @type {object}
         */
        this._treasureRob = data.treasureRob || {};

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});
        /**
         * 当前是否处于批量修改属性的状态，如果是，那就等结束批量修改后再同步到客户端、存盘
         * @type {boolean}
         */
        Object.defineProperty(this, "_inBatch", {enumerable: false, writable:true, value: false});

        //添加事件处理
        var thisObj = this;
        role.addListener(function(eventName, context, notifier) {
            thisObj.onLogin();
        }, eventNames.ROLE_LOGIN);


        try {
            //如果是机器人就返回
            if(role.isRobot())
                return;

            this.checkProphetTower();

            var errObj = {};
            if(!systemMgr.isEnabled(this._role,enSystemId.warriorTried,errObj))   //勇士试炼未开启
                return;
            this.checkWarriorTriedReset();

        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        //以防万一，检测一下批量保存的
        this.endBatch();
    }

    /**
     * 存盘数据
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.actProps = this._actProps;
        rootObj.warriorTried = this._warriorTried;
        rootObj.prophetTower = this._prophetTower;
        rootObj.treasureRob = this._treasureRob;
    }

    /**
     * 下发客户端的数据
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.actProps = this._actProps;
        rootObj.warriorTried = this._warriorTried;
        rootObj.prophetTower = this._prophetTower;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        rootObj.actProps = null;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        var thisProps = this._actProps;
        var destProps = {};

        destProps.arenaPos = thisProps.arenaPos;
        destProps.arenaRank = rankMgr.getRankValueByKey(rankTypes.arena, this._role.getHeroId());
        destProps.arenaScore = thisProps.arenaScore;

        rootObj.actProps = destProps;
    }

    /**
     *
     * @returns {ActivityServerData}
     */
    getServerData()
    {
        return this._serverData;
    }

    /**
     *
     * @param {number} enumVal
     * @returns {number}
     */
    getNumber(enumVal)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propName = propNameMap[enumVal || 0];
        if (!propName)
            return 0;

        var propVal = this._actProps[propName];
        if (propVal === null || propVal === undefined)
        //为防止性能问题，填上默认值
            this._actProps[propName] = propVal = 0;

        return propVal;
    }

    /**
     *
     * @param {number} enumVal
     * @returns {string}
     */
    getString(enumVal)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propName = propNameMap[enumVal || 0];
        if (!propName)
            return "";

        var propVal = this._actProps[propName];
        if (propVal === null || propVal === undefined)
        //为防止性能问题，填上默认值
            this._actProps[propName] = propVal = "";

        return propVal;
    }

    /**
     *
     * @param {number} enumVal
     * @param {number} newVal
     * @returns {number}
     */
    setNumber(enumVal, newVal)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propName = propNameMap[enumVal || 0];
        if (!propName)
            return 0;

        if (!Object.isNumber(newVal))
            newVal = parseFloat(newVal) || 0;

        var oldVal = this._actProps[propName];
        this._actProps[propName] = newVal;

        if (oldVal !== newVal)
            this._onChange(propName, newVal);

        return newVal;
    }

    /**
     *
     * @param {number} enumVal
     * @param {string} newVal
     * @returns {string}
     */
    setString(enumVal, newVal)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propName = propNameMap[enumVal || 0];
        if (!propName)
            return "";

        //如果不是string，转为string
        if (newVal === null || newVal === undefined)
            newVal = "";
        else if (!Object.isString(newVal))
            newVal += "";

        var oldVal = this._actProps[propName];
        this._actProps[propName] = newVal;

        if (oldVal !== newVal)
            this._onChange(propName, newVal);

        return newVal;
    }

    /**
     *
     * @param {number} enumVal
     * @param {number} delta
     * @returns {number}
     */
    addNumber(enumVal, delta)
    {
        //为防止enumVal为null或undefined拖慢性能
        var propName = propNameMap[enumVal || 0];
        if (!propName)
            return 0;

        //如果本来就是number，执行parseFloat，会性能不行，如果不是number，parseFloat性能本来就不行
        //所以一定要判断是否number
        if (!Object.isNumber(delta))
            delta = parseFloat(delta) || 0;

        var oldVal = this._actProps[propName] || 0;
        var newVal = oldVal + delta;
        this._actProps[propName] = newVal;

        if (oldVal !== newVal)
            this._onChange(propName, newVal);

        return newVal;
    }

    /**
     *
     * @param {string[]} batchKeys
     * @private
     */
    _doBatchSyncSave(batchKeys)
    {
        var keysLen = batchKeys.length;

        if (keysLen <= 0)
            return;

        var roleCur = this._role;
        var actProps = this._actProps;

        var syncObj = {};
        for (let i = 0; i < keysLen; ++i)
        {
            let key =  batchKeys[i];
            syncObj[key] = actProps[key];
        }
        var syncMsg = new activityMessage.SyncActivityPropVo(syncObj);
        roleCur.sendEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.PUSH_SYNC_PROP, syncMsg);

        var userId = roleCur.getUserId();
        var heroId = roleCur.getHeroId();
        var queryObj = {"props.heroId":heroId};
        var updatePrefx = "actProps.";
        var updateObj = {};

        for (let i = 0; i < keysLen; ++i)
        {
            let key =  batchKeys[i];
            updateObj[updatePrefx + key] = actProps[key];
        }

        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        col.updateOneNoThrow(queryObj, {"$set":updateObj});
    }

    _doSingleSyncSave(propName, propVal)
    {
        var roleCur = this._role;

        //同步
        var syncMsg = new activityMessage.SyncActivityPropVo({[propName]: propVal});
        roleCur.sendEx(ModuleIds.MODULE_ACTIVITY, CmdIdsActivity.PUSH_SYNC_PROP, syncMsg);

        //存盘
        var userId = roleCur.getUserId();
        var heroId = roleCur.getHeroId();
        var queryObj = {"props.heroId":heroId};
        var updateKey = "actProps." + propName;
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        col.updateOneNoThrow(queryObj, {"$set":{[updateKey] : propVal}});
    }

    _onChange(propName, propVal)
    {
        //如果是批量修改，加入批量修改缓存
        if (this._inBatch)
        {
            this._batchKeys.pushIfNotExist(propName);
        }
        //否则直接存盘、同步
        else
        {
            this._doSingleSyncSave(propName, propVal);
        }
    }

    startBatch()
    {
        this._inBatch = true;
        //这个属性是延迟加入，以免用不着时浪费
        if (!this._batchKeys)
            Object.defineProperty(this, "_batchKeys", {enumerable: false, writable:true, value: []});
    }

    endBatch()
    {
        if (this._inBatch)
        {
            var batchKeys = this._batchKeys;

            //提前置空，以防下面的函数导致意外问题，导致本函数重复执行
            this._inBatch = false;
            this._batchKeys = [];

            this._doBatchSyncSave(batchKeys);
        }
    }

    addArenaLog(myIsWin, myOldRank, myRank, opHeroId, opRoleId, opName, opOldScore, time, keepLogNum)
    {
        //机器人不存战斗记录
        if (this._role.isRobot())
            return;

        //构造日志对象
        var logObj = {win:myIsWin, oldRank:myOldRank, rank:myRank, opHeroId:opHeroId, opRoleId:opRoleId, opName:opName, opOldScore:opOldScore, time:time};

        //插入内存
        this._arenaLog.unshift(logObj);
        var exceedingCnt = this._arenaLog.length - keepLogNum;
        if (exceedingCnt === 1)
            this._arenaLog.pop();
        else if (exceedingCnt > 1)
            this._arenaLog.splice(keepLogNum, exceedingCnt);

        var userId = this._role.getUserId();
        var heroId = this._role.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj = {"props.heroId":heroId};
        var updateObj = {$push:{"arenaLog":{$each:[logObj], $position:0, $slice:keepLogNum}}};
        col.updateOneNoThrow(queryObj, updateObj);
    }

    /**
     *
     * @returns {ArenaLogItemVo[]}
     */
    getArenaLog()
    {
        return this._arenaLog;
    }

    /**
     * 检测勇士试炼是否需要重置
     */
    checkWarriorTriedReset()
    {
        if(this._warriorTried.uptime == null || !dateUtil.isToday(this._warriorTried.uptime))  //不是今天,说明需要重置勇士试炼
        {
            var trieds = this.resetTriedLevel();
            var baseCfg = warriorTriedConfig.getTriedBaseConfig();
            //修改内存
            this._warriorTried = {"remainTried":baseCfg.dailyNum, "refresh":0, "uptime":dateUtil.getTimestamp(), "trieds":trieds};
            //存盘修改
            this.updateWarriorDB();
            return true;  //true表示重置了
        }
        return false;
    }

    /**
     * 重置勇士试炼关卡
     * @returns {Array}
     */
    resetTriedLevel()
    {
        //获取随机的关卡、星级、状态数组
        var trieds = [];
        var levels = warriorTriedConfig.getRandomTriedLevel(this._role.getNumber(enProp.level), 4);
        //根据配置的权重拿试炼任务
        var baseCfg = warriorTriedConfig.getTriedBaseConfig();
        var taskWeight = baseCfg.taskWeight;
        var arr = taskWeight.split('|');
        var s = warriorTriedConfig.getRandomStar(arr[0], arr[1]);
        var add = warriorTriedConfig.getRandomStar(arr[2], arr[3]);
        /** @type {TriedStarConfig[]}*/
        var starts = s.concat(add);


        for(var i = 0; i < MaxWarriorLevel;++i)
        {
            //随机奖励
            var cfg = WarTriedBaseConfig.getStarConfig(starts[i].star);
            var rewards = rewardConfig.getRandomReward2(cfg.rewardId);

            trieds.push({"star":starts[i].star, "room":levels[i].roomId, "status":0, "rewards":rewards});  //奖励物品
        }

        trieds.shuffle();
        return trieds;
    }

    /**
     * @returns {WarriorTried}
     */
    getWarriorTriedData()
    {
        return this._warriorTried;
    }

    /**
     * 刷新勇士试炼
     * @returns {boolean}
     */
    refreshWarrior(auto)
    {
        if(!auto)
        {
            if(this._warriorTried.refresh < warriorTriedConfig.getTriedBaseConfig().freeRefresh)  //免费刷新
            {
                this._warriorTried.refresh++;
            }
            else  //付费
            {
                //先检查一下钻石是否满足
                var cost = warriorTriedConfig.getRefreshCostConfig(this._warriorTried.refresh+1)
                if(this._role.getNumber(enProp.diamond) < cost)
                    return false;

                //扣钻石
                this._role.getItemsPart().costItem(enItemId.DIAMOND, cost);
                this._warriorTried.refresh++;
            }
        }

        this._warriorTried.trieds = this.resetTriedLevel();

        //存盘修改
        this.updateWarriorDB();
        return true;
    }

    /**
     * 结束试炼关卡
     */
    doEndWarriorWin(index)
    {
        var part = this._role.getActivityPart();
        var wt = part.getWarriorTriedData();
        wt.trieds[index].status = 1;
        wt.remainTried--;
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {"$set":{"warriorTried.trieds":wt.trieds,
            "warriorTried.remainTried":wt.remainTried}});
    }

    //数据库存盘
    updateWarriorDB()
    {
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {"$set":{"warriorTried":this._warriorTried}});
    }

    getProphetTowerData()
    {
        return this._prophetTower;
    }
    updateProphetTowerDB()
    {
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {"$set":{"prophetTower":this._prophetTower}});
    }

    checkProphetTower()
    {
        if (!dateUtil.isToday(this._role.getPropsPart().getNumber(enProp.towerWinTime))) {
            var nums = this._role.getPropsPart().getNumber(enProp.towerEnterNums);
            if (this._prophetTower.getRewardState != undefined && nums != 0) {
                var getNum = 0;
                for(var i =0; i < 5; i++) {
                    getNum += this._prophetTower.getRewardState[i];
                }
                if (nums > getNum) {
                    var boxNum = nums - getNum;
                    var levelNum = this._role.getNumber(enProp.towerLevel);
                    var cfg = towerConfig.getProphetTowerConfig(levelNum);
                    for(var i = 0; i < boxNum; i++) {
                        let items = rewardConfig.getRandomReward2(cfg.rewardId);
                        let mail = mailMgr.createMail("预言者之塔奖励", "系统", "由于您在预言者之塔中未领取奖励，现已将奖励发送到您的邮箱中。请您及时领取！", items);

                        this._role.getMailPart().sendMail(this._role, mail, true);
                    }
                }
            }
            this._role.getPropsPart().setNumber(enProp.towerWinTime, dateUtil.getTimestamp());
        }
    }



    getTreasureRobChallengers()
    {
        return this._treasureRob.challengers;
    }

    setTreasureRobChallengers(challengers)
    {
        this._treasureRob.challengers = challengers;
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {"$set":{"treasureRob.challengers":this._treasureRob.challengers}});
    }

    getTreasureRobBattleLogs()
    {
        if(this._treasureRob.battleLogs) //临时，清掉一些错误的战报数据
        {
            for(var i=0; i<this._treasureRob.battleLogs.length; i++)
            {
                if(!this._treasureRob.battleLogs[i].heroId)
                {
                    this._treasureRob.battleLogs.splice(i,1);
                    i--;
                }
            }
        }
        return this._treasureRob.battleLogs || [];
    }

    addTreasureRobBattleLog(heroId, name, itemId, itemNum, iStart, iWin, time)
    {
        var battleLog = {
            heroId : heroId,
            name : name,
            itemId : itemId,
            itemNum : itemNum,
            iStart : iStart,
            iWin : iWin,
            time : time,
            revenged : false
        };
        var battleLogs = this.getTreasureRobBattleLogs();
        battleLogs.push(battleLog);
        if(battleLogs.length>20)
        {
            battleLogs.splice(0,1);
        }
        this.setTreasureRobBattleLogs(battleLogs);
    }

    setTreasureRobBattleLogs(battleLogs)
    {
        this._treasureRob.battleLogs = battleLogs;
        var db = dbUtil.getDB(this._role.getUserId());
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":this._role.getHeroId()}, {"$set":{"treasureRob.battleLogs":this._treasureRob.battleLogs}});
    }

    onLogin()
    {
        this.checkTreasureRobed();
    }

    checkTreasureRobed()
    {
        if (!dateUtil.isToday(this.getNumber(enActProp.treasureRobedTime))) {
            var curTimestamp = dateUtil.getTimestamp();
            this.startBatch();
            this.setNumber(enActProp.treasureRobedTime, curTimestamp);
            this.setNumber(enActProp.treasureRobedMax, appUtil.getRandom(0, 3));
            this.setNumber(enActProp.treasureRobedCnt, 0);
            this.endBatch();
        }
        if (this.getNumber(enActProp.treasureRobedCnt) >= this.getNumber(enActProp.treasureRobedMax)) {
            return;
        }
        var rankInfo = treasureRobUtil.getRankInfo(this._role, 1, treasureRobConfig.getTreasureRobBasicCfg().maxPowerRate);
        var heroId = treasureRobUtil.selectRankItem(rankInfo, [this._role.getHeroId()]);
        if(heroId)
        {
            var rankVal = rankMgr.getRankValueByKey(rankTypes.realPower, heroId);
            if(rankVal)
            {
                treasureRobUtil.robed(this._role, heroId, rankInfo.rankArr[rankVal].name);
            }
        }
    }
}

exports.ActivityPart = ActivityPart;