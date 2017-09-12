"use strict";


var dbUtil = require("../../libs/dbUtil");
var dateUtil = require("../../libs/dateUtil");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var taskRewardConfig = require("../gameConfig/taskRewardConfig");
var vitalityRewardConfig = require("../gameConfig/vitalityRewardConfig");
var growthTaskConfig=require("../gameConfig/growthTaskConfig");
var equipConfig = require("../gameConfig/equipConfig");
var taskMessage = require("../netMessage/taskMessage");
var CmdIdsTask = require("../netMessage/taskMessage").CmdIdsTask;
var taskPropDefine = require("../enumType/taskPropDefine");
var ActivityTask = require("./activityTask").ActivityTask;
var LotteryTask = require("./lotteryTask").LotteryTask;
var CostTask = require("./costTask").CostTask;
var CorpsTask = require("./corpsTask").CorpsTask;
var VipTask = require("./vipTask").VipTask;
var UpgradeTask = require("./upgradeTask").UpgradeTask;
var WarriorTriedTask = require("./warriorTriedTask").WarriorTriedTask;
var EliteLvTask = require("./eliteLvTask").EliteLvTask;
var enProp = require("../enumType/propDefine").enProp;
var enEquipPos = require("../equip/equip").enEquipPos;
var enSkillPos = require("../weapon/weapon").enSkillPos;
var enOpActProp = require("../enumType/opActivityPropDefine").enOpActProp;
var enActProp = require("../enumType/activityPropDefine").enActProp;

////////////////////////////////////////
const enTaskProp = taskPropDefine.taskProp;
const enTaskType = taskPropDefine.taskType;

/**
 *
 * @type {object.<number, string>}
 */
var propNameMap = {};
for (var propName in enTaskProp)
{
    propNameMap[enTaskProp[propName]] = propName;
}


/**
 *
 * @typedef {Object} taskProps - 每日任务属性表
 * @property {number} checkInNums - 当月签到次数
 * @property {number} lastCheckIn - 上次签到时间
 */
class TaskPart {
    constructor(role, data) {
        /**
         *
         * @type {object}
         * @private
         */
        this.taskProps = data.taskProps || {};




        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});
        /**
         * 当前是否处于批量修改属性的状态，如果是，那就等结束批量修改后再同步到客户端、存盘
         * @type {boolean}
         */
        Object.defineProperty(this, "_inBatch", {enumerable: false, writable: true, value: false});
        //成员
    }

    release() {
        //以防万一，检测一下批量保存的
        this.endBatch();
    }


    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj) {
        rootObj.taskProps = this.taskProps;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj) {
        rootObj.taskProps = this.taskProps;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj) {

    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj) {

    }




    /**
     * 能否获取成长任务奖励
     * @param {number}taskId
     * @returns {boolean}
     */
    canGetGrowthTaskReward(taskId)
    {
        var growthTaskCfg=growthTaskConfig.getGrowthTaskConfig(taskId);
        var taskType = enTaskType[growthTaskCfg.type];
        var taskProp = enTaskProp[growthTaskCfg.prop];
        var taskGetTime = this.getNumber(taskProp);

        if(taskGetTime != 0)
        {
            return false;
        }
        else {
            var current = 0;
            var target = growthTaskCfg.param[0];
            var targetLevel = 0;
            if (growthTaskCfg.param.length > 1) {
                targetLevel = growthTaskCfg.param[1];
            }

            switch (taskType) {
                case enTaskType.normalLv:
                {
                    let levelsPart = this._role.getLevelsPart();
                    current = levelsPart.getStarsByNodeId(targetLevel.toString());
                    break;
                }
                case enTaskType.specialLv:
                {
                    let levelsPart = this._role.getLevelsPart();
                    current = levelsPart.getStarsByNodeId(targetLevel.toString());
                    break;
                }
                case enTaskType.weaponSkill:
                {
                    let weaponPart = this._role.getWeaponPart();
                    for (let i = 0; i < enEquipPos.maxWeapon - enEquipPos.minWeapon + 1; ++i) {
                        for (let j = 0; j < 4; ++j) {
                            if (weaponPart.getWeaponSkill(i, j).lv >= targetLevel) {
                                current++;
                            }
                        }
                    }
                    break;
                }
                case enTaskType.petsEquipAdvlv:
                {
                    let petsPart = this._role.getPetsPart();
                    let pets = petsPart._pets;
                    for (let i = 0; i < pets.length; ++i)
                    {
                        let equipsPart = pets[i].getEquipsPart();
                        for (let j = 0; j < enEquipPos.minWeapon - enEquipPos.minNormal + 1; ++j)
                        {
                            if (equipsPart.getEquipByIndex(j).advLv >= targetLevel)
                            {
                                current++;
                            }
                        }
                    }
                    break;
                }
                case enTaskType.petsEquipStar:
                {
                    let petsPart = this._role.getPetsPart();
                    let pets = petsPart._pets;
                    for (let i = 0; i < pets.length; ++i)
                    {
                        let equipsPart = pets[i].getEquipsPart();
                        for (let j = 0; j < enEquipPos.minWeapon - enEquipPos.minNormal + 1; ++j)
                        {
                            let equip = equipsPart.getEquipByIndex(i);
                            let star = equipConfig.getEquipConfig(equip.equipId).star;
                            if (star >= targetLevel)
                            {
                                current++;
                            }
                        }
                    }

                    break;
                }
                case enTaskType.equipAdvLv:
                {
                    let equipsPart = this._role.getEquipsPart();
                    for (let i = 0; i < enEquipPos.maxWeapon - enEquipPos.minNormal + 1; ++i) {
                        if (equipsPart.getEquipByIndex(i).advLv >= targetLevel) {
                            current++;
                        }
                    }
                    break;
                }
                case enTaskType.equipStar:
                {
                    let equipsPart = this._role.getEquipsPart();
                    for (let i = 0; i < enEquipPos.maxWeapon - enEquipPos.minNormal + 1; ++i) {
                        let equip = equipsPart.getEquipByIndex(i);
                        let star = equipConfig.getEquipConfig(equip.equipId).star;
                        if (star >= targetLevel) {
                            current++;
                        }
                    }
                    break;
                }
                case enTaskType.petsNum:
                {
                    let petsPart = this._role.getPetsPart();
                    current = petsPart.getPetCount();
                    break;
                }
                case enTaskType.petAdvLv:
                {
                    let petsPart = this._role.getPetsPart();
                    let pets = petsPart._pets;
                    for (let i = 0; i < pets.length; ++i) {
                        if (pets[i].getNumber(enProp.advLv) >= targetLevel) {
                            current++;
                        }
                    }
                    break;
                }
                case enTaskType.petStar:
                {
                    let petsPart = this._role.getPetsPart();
                    let pets = petsPart._pets;
                    for (let i = 0; i < pets.length; ++i) {
                        if (pets[i].getNumber(enProp.star) >= targetLevel) {
                            current++;
                        }
                    }
                    break;
                }
                case enTaskType.arena:
                {
                    let activityPart = this._role.getActivityPart();
                    current = activityPart.getNumber(enActProp.arenaTotalWin);
                    break;
                }
                case enTaskType.daily:
                {
                    current = this.getNumber(enTaskProp.dailyTaskTotal);
                    break;
                }
                case enTaskType.friend:
                {
                    let socialPart = this._role.getSocialPart();
                    current = socialPart._friends.length;
                    break;
                }
                case enTaskType.power:
                {
                    current = this._role.getNumber(enProp.powerTotal);
                    break;
                }
                case enTaskType.corps:
                {
                    let corpsId = this._role.getNumber(enProp.corpsId);
                    current = corpsId > 0? 1 : 0;
                    break;
                }

            }

            if(current >= target)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /**
     * 领取成长任务奖励
     * @param {number}taskId
     * @returns {boolean}
     */
    getGrowthTaskReward(taskId)
    {
        if(this.canGetGrowthTaskReward(taskId))
        {
            var growthTaskCfg = growthTaskConfig.getGrowthTaskConfig(taskId);
            var itemIds=growthTaskCfg.itemId;
            var itemNums=growthTaskCfg.itemNum;
            var itemsPart=this._role.getItemsPart();
            var rewardItems = {};
            for(let i = 0; i < itemIds.length; i++)
                rewardItems[itemIds[i]] = (rewardItems[itemIds[i]] || 0) + itemNums[i];
            itemsPart.addItems(rewardItems);
            var taskGetTime = enTaskProp[growthTaskCfg.prop];
            this.setNumber(taskGetTime,dateUtil.getTimestamp());
            return true;
        }
        else
        {
            return false;
        }

    }



    /**
     * 检测是否能够获取每日任务奖励
     * @param {number}id
     * @returns {boolean}
     */
    canGetDailyTaskReward(id) {
        var taskRewardCfg = taskRewardConfig.getTaskRewardConfig(id);
        switch (enTaskType[taskRewardCfg.taskType])
        {
            case enTaskType.activity:
            {
                let task = new ActivityTask(this._role);
                return task.canGetReward(id);
            }
            case enTaskType.lottery:
            {
                let task = new LotteryTask(this._role);
                return task.canGetReward(id);
            }
            case enTaskType.cost:
            {
                let task = new CostTask(this._role);
                return task.canGetReward(id);
            }
            case enTaskType.corps:
            {
                let task = new CorpsTask(this._role);
                return task.canGetReward(id);
            }
            case enTaskType.vip:
            {
                let task = new VipTask(this._role);
                return task.canGetReward(id);
            }
            case enTaskType.upGrade:
            case enTaskType.prophetTower:
            {
                let task = new UpgradeTask(this._role);
                return task.canGetReward(id);
            }
            case enTaskType.eliteLv:
            {
                let task = new EliteLvTask(this._role);
                return task.canGetReward(id);
            }
            case enTaskType.warriorTried:
            {
                let task = new WarriorTriedTask(this._role);
                return task.canGetReward(id);
            }
        }


    }

    /**
     * 领取每日任务奖励
     * @param {number}taskId
     * @returns {boolean}
     */
    getDailyTaskReward(taskId)
    {
        if(this.canGetDailyTaskReward(taskId))
        {
            var taskRewardCfg = taskRewardConfig.getTaskRewardConfig(taskId);
            var itemIds=taskRewardCfg.itemId;
            var itemNums=taskRewardCfg.itemNum;

            var itemsPart=this._role.getItemsPart();
            var rewardItems = {};


            for(let i = 0; i < itemIds.length; i++)
                rewardItems[itemIds[i]] = (rewardItems[itemIds[i]] || 0) + itemNums[i];
            itemsPart.addItems(rewardItems);
            var taskRewardTime = enTaskProp[taskRewardCfg.taskRewardTime];

            var oldVitality;
            if(dateUtil.isToday(this.getNumber(enTaskProp.dailyTaskGet)))
            {
                oldVitality = this.getNumber(enTaskProp.vitality);
            }
            else
            {
                oldVitality = 0;
            }
            this.startBatch();
            this.setNumber(taskRewardTime,dateUtil.getTimestamp());
            this.setNumber(enTaskProp.dailyTaskGet,dateUtil.getTimestamp());
            this.setNumber(enTaskProp.vitality,oldVitality + taskRewardCfg.vitality);
            this.addNumber(enTaskProp.dailyTaskTotal,1);
            this.endBatch();
            return true;
        }
        else
        {
            return false;
        }

    }

    /**
     * 检测是否能获取活跃度奖励
     * @param {number}vitalityId
     * @returns {boolean}
     */
    canGetVitalityReward(vitalityId)
    {
        var currentVitallity;
        if(!dateUtil.isToday(this.getNumber(enTaskProp.dailyTaskGet)))
        {
            currentVitallity=0;
            this.setNumber(enTaskProp.vitality,0);
        }
        else
        {
            currentVitallity=this.getNumber(enTaskProp.vitality);
        }
        var vitalityRewardCfg=vitalityRewardConfig.getVitalityRewardConfig(vitalityId);
        var getVitilityTime;
        switch (vitalityRewardCfg.boxNum)
        {
            case(1):
            {
                getVitilityTime = this.getNumber(enTaskProp.vitalityBox1);
                break;
            }
            case(2):
            {
                getVitilityTime = this.getNumber(enTaskProp.vitalityBox2);
                break;
            }
            case(3):
            {
                getVitilityTime = this.getNumber(enTaskProp.vitalityBox3);
                break;
            }
            case(4):
            {
                getVitilityTime = this.getNumber(enTaskProp.vitalityBox4);
                break;
            }
        }

        var roleLevel = this._role.getNumber(enProp.level);
        if(currentVitallity >= vitalityRewardCfg.vitality&&!dateUtil.isToday(getVitilityTime)&&roleLevel>=vitalityRewardCfg.level)
        {
            return true;
        }
        else
        {
            return false;
        }

    }


    /**
     * 获取活跃度奖励
     * @param {number}vitalityId
     * @returns {boolean}
     */
    getVitalityReward(vitalityId)
    {
        if(this.canGetVitalityReward(vitalityId))
        {

            var vitalityRewardCfg = vitalityRewardConfig.getVitalityRewardConfig(vitalityId)
            var itemIds=vitalityRewardCfg.itemId;
            var itemNums=vitalityRewardCfg.itemNum;

            var itemsPart=this._role.getItemsPart();
            var rewardItems = {};

            for(let i = 0; i < itemIds.length; i++)
                rewardItems[itemIds[i]] = (rewardItems[itemIds[i]] || 0) + itemNums[i];
            itemsPart.addItems(rewardItems);

            switch (vitalityRewardCfg.boxNum)
            {
                case(1):
                {
                    this.setNumber(enTaskProp.vitalityBox1,dateUtil.getTimestamp());
                    break;
                }
                case(2):
                {
                    this.setNumber(enTaskProp.vitalityBox2,dateUtil.getTimestamp());
                    break;
                }
                case(3):
                {
                    this.setNumber(enTaskProp.vitalityBox3,dateUtil.getTimestamp());
                    break;
                }
                case(4):
                {
                    this.setNumber(enTaskProp.vitalityBox4,dateUtil.getTimestamp());
                    break;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }


    /**
     * 更新每日升级神侍次数
     * @param {number}num
     */
    addPetUpgradeNum(num)
    {
        let lastTime = this._role.getNumber(enProp.upPetTime);
        this._role.startBatch();
        if(dateUtil.isToday(lastTime))
        {
            this._role.addNumber(enProp.upPetNum,num);
        }
        else
        {
            this._role.setNumber(enProp.upPetNum,num);
        }
        this._role.setNumber(enProp.upPetTime,dateUtil.getTimestamp());
        this._role.endBatch();
    }

    /**
     * 更新每日升级装备次数
     * @param {number}num
     */
    addEquipUpgradeNum(num)
    {
        let lastTime = this._role.getNumber(enProp.upEquipTime);
        this._role.startBatch();
        if(dateUtil.isToday(lastTime))
        {
            this._role.addNumber(enProp.upEquipNum,num);
        }
        else
        {
            this._role.setNumber(enProp.upEquipNum,num);
        }
        this._role.setNumber(enProp.upEquipTime,dateUtil.getTimestamp());
        this._role.endBatch();
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

        var propVal = this.taskProps[propName];
        if (propVal === null || propVal === undefined)
        //为防止性能问题，填上默认值
            this.taskProps[propName] = propVal = 0;

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

        var propVal = this.taskProps[propName];
        if (propVal === null || propVal === undefined)
        //为防止性能问题，填上默认值
            this.taskProps[propName] = propVal = "";

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

        var oldVal = this.taskProps[propName];
        this.taskProps[propName] = newVal;

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

        var oldVal = this.taskProps[propName];
        this.taskProps[propName] = newVal;

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

        var oldVal = this.taskProps[propName] || 0;
        var newVal = oldVal + delta;
        this.taskProps[propName] = newVal;

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
        var taskProps = this.taskProps;

        var syncObj = {};
        for (let i = 0; i < keysLen; ++i)
        {
            let key =  batchKeys[i];
            syncObj[key] = taskProps[key];
        }
        var syncMsg = new taskMessage.SyncTaskPropVo(syncObj);

        roleCur.sendEx(ModuleIds.MODULE_TASK, CmdIdsTask.PUSH_SYNC_PROP, syncMsg);

        var userId = roleCur.getUserId();
        var heroId = roleCur.getHeroId();
        var queryObj = {"props.heroId":heroId};
        var updatePrefx = "taskProps.";
        var updateObj = {};

        for (let i = 0; i < keysLen; ++i)
        {
            let key =  batchKeys[i];
            updateObj[updatePrefx + key] = taskProps[key];
        }

        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        col.updateOneNoThrow(queryObj, {"$set":updateObj});
    }

    _doSingleSyncSave(propName, propVal)
    {
        var roleCur = this._role;

        //同步
        var syncMsg = new taskMessage.SyncTaskPropVo({[propName]: propVal});
        roleCur.sendEx(ModuleIds.MODULE_TASK, CmdIdsTask.PUSH_SYNC_PROP, syncMsg);

        //存盘
        var userId = roleCur.getUserId();
        var heroId = roleCur.getHeroId();
        var queryObj = {"props.heroId":heroId};
        var updateKey = "taskProps." + propName;
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
}

module.exports = TaskPart;