"use strict";


var dbUtil = require("../../libs/dbUtil");
var dateUtil = require("../../libs/dateUtil");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var CheckInRewardConfig=require("../gameConfig/checkInRewardConfig");
var levelRewardConfig = require("../gameConfig/levelRewardConfig");
var vipConfig = require("../gameConfig/vipConfig");
var vipGiftConfig = require("../gameConfig/vipGiftConfig");
var opActivityMessage = require("../netMessage/opActivityMessage");
var CmdIdsOpActivity = require("../netMessage/opActivityMessage").CmdIdsOpActivity;
var opActPropDefine = require("../enumType/opActivityPropDefine");
var enProp = require("../enumType/propDefine").enProp;
var enItemId = require("../enumType/globalDefine").enItemId;
var eventNames = require("../enumType/eventDefine").eventNames;

////////////////////////////////////////
const enOpActProp = opActPropDefine.enOpActProp;

/**
 *
 * @type {object.<number, string>}
 */
var propNameMap = {};
for (var propName in enOpActProp)
{
    propNameMap[enOpActProp[propName]] = propName;
}


/**
 *
 * @typedef {Object} opActProps - 运营活动属性表
 * @property {number} checkInNums - 当月签到次数
 * @property {number} lastCheckIn - 上次签到时间
 */
class OpActivityPart {
    constructor(role, data) {
        /**
         *
         * @type {object}
         * @private
         */
        this.opActProps = data.opActProps|| {};

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});
        /**
         * 当前是否处于批量修改属性的状态，如果是，那就等结束批量修改后再同步到客户端、存盘
         * @type {boolean}
         */
        Object.defineProperty(this, "_inBatch", {enumerable: false, writable:true, value: false});
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
        rootObj.opActProps = this.opActProps;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj) {
        rootObj.opActProps = this.opActProps;
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
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {

    }


    /**
     *领取等级礼包奖励
     * @param id {number}
     * @return  {boolean}
     */
    getLevelReward(id)
    {
        var levelRewardCfg=levelRewardConfig.getLevelRewardConfig(id);
        var roleLevel = this._role.getNumber(enProp.level);
        let prop = "lv" + levelRewardCfg.level.toString();
        var opActProp = enOpActProp[prop];
        var rewardTime = this.getNumber(opActProp);
        if(rewardTime==0&&roleLevel>=levelRewardCfg.level)
        {

            var itemIds=levelRewardCfg.itemId;
            var itemNums=levelRewardCfg.itemNum;

            var itemsPart=this._role.getItemsPart();
            var rewardItems = {};


            for(var i = 0; i < itemIds.length; i++)
                rewardItems[itemIds[i]] = (rewardItems[itemIds[i]] || 0) + itemNums[i];
            itemsPart.addItems(rewardItems);

            this.setNumber(opActProp,dateUtil.getTimestamp());

            return true;
        }
        else
        {
            return false;
        }
    }


    /**
     * 领取签到奖励
     * @returns {number}
     */
    addCheckInReward()
    {
        var checkInRewardConfig = CheckInRewardConfig.getCheckInRewardConfig(dateUtil.getDateFromTimestamp(this.getNumber(enOpActProp.lastCheckIn)).getMonth()+1 ,this.getNumber(enOpActProp.checkInNums));
        if(checkInRewardConfig != undefined) {
            let vipLv=this._role.getNumber(enProp.vipLv);

            var itemId = checkInRewardConfig.itemId;
            var itemNums = vipLv >= checkInRewardConfig.vipLevel && checkInRewardConfig.vipLevel != 0? checkInRewardConfig.itemNums * 2: checkInRewardConfig.itemNums;
            this._role.getItemsPart().addItem(itemId, itemNums);
        }
        return checkInRewardConfig.id;
    }

    /**
     * 更新vip等级
     */
    updateVipLv()
    {
        let currentVipLv = this._role.getNumber(enProp.vipLv);
        let vipCfg = vipConfig.getVipConfig(currentVipLv + 1);
        let newVipLv = currentVipLv;
        while(vipCfg)
        {
            if (this.getNumber(enOpActProp.totalRecharge) >= vipCfg.totalRecharge)
            {
                newVipLv++;
                vipCfg = vipConfig.getVipConfig(newVipLv + 1);
            }
            else
            {
                break;
            }
        }
        this._role.setNumber(enProp.vipLv,newVipLv);
        this._role.fireEvent(eventNames.VIP_LV_CHANGE);
    }

    /**
     *领取vip礼包
     * @param vipLv {number}
     * @return  {boolean}
     */
    getVipGift(vipLv)
    {
        var vipGiftCfg = vipGiftConfig.getVipGiftConfig(vipLv);
        var currentVipLv = this._role.getNumber(enProp.vipLv);
        let prop = "vip" + vipLv.toString() + "Gift";
        var opActProp = enOpActProp[prop];
        var rewardTime = this.getNumber(opActProp);
        let itemsPart = this._role.getItemsPart();
        if(rewardTime == 0 && currentVipLv >= vipLv && itemsPart.canCostDiamond(vipGiftCfg.vipGiftDiamondCost))
        {
            var itemIds=vipGiftCfg.vipGiftItemId;
            var itemNums=vipGiftCfg.vipGiftItemNum;
            var rewardItems = {};

            for(var i = 0; i < itemIds.length; i++)
                rewardItems[itemIds[i]] = (rewardItems[itemIds[i]] || 0) + itemNums[i];
            itemsPart.addItems(rewardItems);
            itemsPart.costItem(enItemId.DIAMOND,vipGiftCfg.vipGiftDiamondCost);
            this.setNumber(opActProp,dateUtil.getTimestamp());
            return true;
        }
        else
        {
            return false;
        }
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

        var propVal = this.opActProps[propName];
        if (propVal === null || propVal === undefined)
        //为防止性能问题，填上默认值
            this.opActProps[propName] = propVal = 0;

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

        var propVal = this.opActProps[propName];
        if (propVal === null || propVal === undefined)
        //为防止性能问题，填上默认值
            this.opActProps[propName] = propVal = "";

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

        var oldVal = this.opActProps[propName];
        this.opActProps[propName] = newVal;

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

        var oldVal = this.opActProps[propName];
        this.opActProps[propName] = newVal;

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

        var oldVal = this.opActProps[propName] || 0;
        var newVal = oldVal + delta;
        this.opActProps[propName] = newVal;

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
        var opActProps = this.opActProps;

        var syncObj = {};
        for (let i = 0; i < keysLen; ++i)
        {
            let key =  batchKeys[i];
            syncObj[key] = opActProps[key];
        }
        var syncMsg = new opActivityMessage.SyncOpActivityPropVo(syncObj);

        roleCur.sendEx(ModuleIds.MODULE_OPACTIVITY, CmdIdsOpActivity.PUSH_SYNC_PROP, syncMsg);

        var userId = roleCur.getUserId();
        var heroId = roleCur.getHeroId();
        var queryObj = {"props.heroId":heroId};
        var updatePrefx = "opActProps.";
        var updateObj = {};

        for (let i = 0; i < keysLen; ++i)
        {
            let key =  batchKeys[i];
            updateObj[updatePrefx + key] = opActProps[key];
        }

        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        col.updateOneNoThrow(queryObj, {"$set":updateObj});
    }

    _doSingleSyncSave(propName, propVal)
    {
        var roleCur = this._role;

        //同步
        var syncMsg = new opActivityMessage.SyncOpActivityPropVo({[propName]: propVal});
        roleCur.sendEx(ModuleIds.MODULE_OPACTIVITY, CmdIdsOpActivity.PUSH_SYNC_PROP, syncMsg);

        //存盘
        var userId = roleCur.getUserId();
        var heroId = roleCur.getHeroId();
        var queryObj = {"props.heroId":heroId};
        var updateKey = "opActProps." + propName;
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

module.exports = OpActivityPart;