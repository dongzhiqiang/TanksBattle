"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var equipModule = require("../equip/equip");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var equipMessage = require("../netMessage/equipMessage");
var CmdIdsEquip = require("../netMessage/equipMessage").CmdIdsEquip;
var propertyTable = require("../gameConfig/propertyTable");
var enProp = require("../enumType/propDefine").enProp;
var enPropFight = require("../enumType/propDefine").enPropFight;
var enEquipPos = equipModule.enEquipPos;
var eventNames = require("../enumType/eventDefine").eventNames;
var equipConfig = require("../gameConfig/equipConfig");

class EquipsPart
{
    constructor(ownerRole, data)
    {
        /**
         * 预先就创建足够数量的数组元素
         * @type {Equip[]}
         * @private
         */
        this._equips = new Array(equipModule.enEquipPos.equipCount);

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});

        /**
         * 属性计算部位中间值
         * @type {object}
         */
        Object.defineProperty(this, "_partValues", {enumerable: false, writable:true, value: {}});
        /**
         * 属性计算部位中间值
         * @type {object}
         */
        Object.defineProperty(this, "_partRates", {enumerable: false, writable:true, value: {}});

        try {
            var equips = data.equips || [];
            //这里保证全部槽位都更新过
            for (var i = 0; i < this._equips.length; ++i) {
                var equipData = equips[i];
                if (equipData === null || equipData === undefined) {
                    //清槽位
                    this._removeEquip(i);
                }
                else {
                    var equip = equipModule.createEquip(equipData);
                    if (!equip)
                        throw new Error("创建Equip失败");
                    if (equip.getPosIndex() !== i)
                       throw new Error("Equip位置不对，角色guid：" + this._role.getGUID() + "，equipId：" + equip.equipId + "，配置位置：" + equip.getPosIndex() + "，数据位置：" + i);
                    //设置主人
                    equip.setOwner(ownerRole);
                    //加入列表
                    this._addEquip(equip);
                }
            }

            //添加事件处理
            var thisObj = this;
            ownerRole.addListener(function(eventName, context, notifier) {
                thisObj.onPartFresh();
            }, eventNames.EQUIP_CHANGE);
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release()
    {
        var equips = this._equips;
        for (var i = 0; i < equips.length; ++i)
        {
            var equip = equips[i];
            if (equip)
                equip.release();
        }
        this._equips = [];
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.equips = this._equips;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.equips = this._equips;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        rootObj.equips = this._equips;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        rootObj.equips = this._equips;
    }

    /**
     *
     * @param {Equip} equip
     * @private
     */
    _addEquip(equip)
    {
        var index = equip.getPosIndex();
        this._equips[index] = equip;
    }

    /**
     *
     * @param {Equip|number} val
     * @private
     */
    _removeEquip(val)
    {
        var index = Object.isNumber(val) ? val : val.getPosIndex();
        this._equips[index] = null;
    }

    /**
     *
     * @param {number} index
     * @returns {Equip|null|undefined} 有可能是null
     */
    getEquipByIndex(index)
    {
        return this._equips[index];
    }

    /**
     *
     * @param {number} equipId
     * @returns {Equip|null} 找不到就返回null
     */
    getEquipByEquipId(equipId)
    {
        var equips = this._equips;
        for (var i = 0; i < equips.length; ++i)
        {
            var equip = equips[i];
            if (equip && equip.equipId === equipId)
                return equip;
        }
        return null;
    }

    /**
     *
     * @param {number} index
     * @param {boolean} noRelease - 有可能是要被转移到别的地方，这时就不要release
     * @return {boolean} 如果装备存在且删除成功就返回true
     */
    removeEquipByIndex(index, noRelease)
    {
        var equip = this.getEquipByIndex(index);
        //不存在？不能删除
        if (!equip)
            return false;

        //先删除
        this._removeEquip(index);

        //设置主人为空
        equip.setOwner(null);

        //如果要释放，那就释放
        if (!noRelease)
        {
            try {
                equip.release();
            }
            catch (err) {
                logUtil.error("装备销毁出错", err);
            }
        }

        //检测条件
        var roleCur = this._role;
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;
        if (isCurHero) {
            //如果是主角自己的装备
            queryObj = {"props.heroId":heroId};
            updateObj = {$set:{["equips." + index]:null}};
        }
        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new equipMessage.RemoveEquipVo(guidCur, index);
        roleTop.sendEx(ModuleIds.MODULE_EQUIP, CmdIdsEquip.PUSH_REMOVE_EQUIP, netMsg);
        return true;
    }

    /**
     *
     * @param {number} equipId
     * @param {boolean?} noRelease - 有可能是要被转移到别的地方，这时就不要release
     * @return {boolean} 如果装备存在且删除成功就返回true
     */
    removeEquipByEquipId(equipId, noRelease)
    {
        var equip = this.getEquipByEquipId(equipId);
        if (!equip)
            return false;

        return this.removeEquipByIndex(equip.getPosIndex(), noRelease);
    }

    /**
     *
     * @param {object} data
     * @return {boolean} 如果添加成功就返回true
     */
    addEquipWithData(data)
    {
        //必须有最基本数据
        //因为下面要获取equipId，所以要先判断
        if (!equipModule.isEquipData(data))
            return false;

        var equipId = data.equipId;
        //已存在？不能添加
        if (this.getEquipByEquipId(equipId))
            return false;

        var equip = equipModule.createEquip(data);
        if (!equip)
            return false;

        //设置主人
        var roleCur = this._role;
        equip.setOwner(roleCur);

        //添加到列表
        this._addEquip(equip);

        //检测条件
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        //存盘
        var index = equip.getPosIndex();
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;
        if (isCurHero) {
            //如果是主角自己的装备
            queryObj = {"props.heroId":heroId};
            updateObj = {$set:{["equips." + index]:equip}};
        }
        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new equipMessage.AddOrUpdateEquipVo(guidCur, true, equip);
        roleTop.sendEx(ModuleIds.MODULE_EQUIP, CmdIdsEquip.PUSH_ADD_OR_UPDATE_EQUIP, netMsg);
        return true;
    }

    /**
     *
     * @param {Equip} equip
     * @return {boolean} 如果添加成功就返回true
     */
    addEquipWithEquip(equip)
    {
        //不能有主人
        if (equip.getOwner())
        {
            logUtil.warn("addEquipWithEquip，有主人的装备必须脱离主人才能给别人");
            return false;
        }

        //已存在？不能添加
        if (this.getEquipByEquipId(equip.equipId))
            return false;

        //设置主人
        var roleCur = this._role;
        equip.setOwner(roleCur);

        //添加到列表
        this._addEquip(equip);

        //检测条件
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        //存盘
        var index = equip.getPosIndex();
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;
        if (isCurHero) {
            //如果是主角自己的装备
            queryObj = {"props.heroId":heroId};
            updateObj = {$set:{["equips." + index]:equip}};
        }
        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new equipMessage.AddOrUpdateEquipVo(guidCur, true, equip);
        roleTop.sendEx(ModuleIds.MODULE_EQUIP, CmdIdsEquip.PUSH_ADD_OR_UPDATE_EQUIP, netMsg);
        return true;
    }

    //用于保存已在数据库的装备
    syncAndSaveEquip(index)
    {
        var equip = this._equips[index];
        //不存在？不能继续
        if (!equip)
            return false;

        //检测条件
        var roleCur = this._role;
        if (!roleCur.canSyncAndSave())
            return true;

        var roleTop = roleCur.getOwner();
        var isCurHero = roleCur.isHero();
        var guidCur = roleCur.getGUID();

        //存盘
        var userId = roleTop.getUserId();
        var heroId = roleTop.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        var queryObj;
        var updateObj;
        if (isCurHero) {
            //如果是主角自己的装备
            queryObj = {"props.heroId":heroId};
            updateObj = {$set:{["equips." + index]:equip}};
        }
        col.updateOneNoThrow(queryObj, updateObj);

        //通知客户端
        var netMsg = new equipMessage.AddOrUpdateEquipVo(guidCur, false, equip);
        roleTop.sendEx(ModuleIds.MODULE_EQUIP, CmdIdsEquip.PUSH_ADD_OR_UPDATE_EQUIP, netMsg);
        return true;
    }

    /**
     * 计算部件的属性
     */
    freshPartProp()
    {
        propertyTable.set(0,  this._partValues);
        propertyTable.set(0,  this._partRates);
        this._partValues[enProp.power] = 0;
        this._partRates[enProp.power] = 0;

        var weaponPos = 0;
        if (this._role.getWeaponPart() != null)//有武器部件，那么是主角
            weaponPos = this._role.getWeaponPart().curWeapon+enEquipPos.minWeapon;
        else//宠物，那么取第一个武器
            weaponPos = enEquipPos.weapon1;

        var tempProp = {};

        for (var pos =0; pos<this._equips.length; pos++)
        {
            var equip = this._equips[pos];
            if(!equip )
            {
                continue;
            }
            //不是当前武器不加属性
            if (pos >= enEquipPos.minWeapon && pos <= enEquipPos.maxWeapon)
            {
                if (pos!= weaponPos)
                    continue;
            }
            equip.getBaseProp(tempProp);
            propertyTable.add(this._partValues, tempProp, this._partValues);
            this._partValues[enProp.power] = this._partValues[enProp.power] + tempProp[enProp.power];
            //logUtil.info("装备"+pos+" "+equip.equipId+" 角色增加战斗力:" + tempProp[enProp.power]);
            var equipCfg = equipConfig.getEquipConfig(equip.equipId);
            this._partRates[enProp.power] = this._partRates[enProp.power] + equipCfg.powerMul;
        }
        //logUtil.info("装备 角色增加生命值:" + this._partValues[enPropFight.hpMax]);
        //logUtil.info("装备 角色增加战斗力:" + this._partValues[enProp.power]);
        //logUtil.info("装备 角色增加战斗系数:" + this._partRates[enProp.power]);
    }

    /**
 * 累加部件的属性
 */
onFreshBaseProp(values, rates)
{
    propertyTable.add(values, this._partValues, values);
    propertyTable.add(rates, this._partRates, rates);
    values[enProp.power] = values[enProp.power] + (this._partValues[enProp.power] || 0);
    rates[enProp.power] = rates[enProp.power] + (this._partRates[enProp.power] || 0);
}

    onPartFresh()
    {
        //logUtil.info("Fresh part:equipsPart");
        this.freshPartProp();
        this._role.getPropsPart().onFreshBasePropUpdate();
    }
}

exports.EquipsPart = EquipsPart;