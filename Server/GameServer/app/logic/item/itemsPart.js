"use strict";

var appUtil = require("../../libs/appUtil");
var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var dateUtil = require("../../libs/dateUtil");
var itemModule = require("../item/item");
var enProp = require("../enumType/propDefine").enProp;
var enItemId = require("../enumType/globalDefine").enItemId;
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var itemMessage = require("../netMessage/itemMessage");
var CmdIdsItem = require("../netMessage/itemMessage").CmdIdsItem;
var rewardConfig = require("../gameConfig/rewardConfig");
var enOpActProp = require("../enumType/opActivityPropDefine").enOpActProp;
var itemUtil = require("./itemUtil");
var adminServerAgent = require("../http/adminServerAgent");

class ItemsPart
{
    constructor(ownerRole, data)
    {
        /**
         *
         * @type {Item[]}
         * @private
         */
        this._items = [];

        /**
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: ownerRole});
        /**
         * @type {object.<number, Item>}
         */
        Object.defineProperty(this, "_itemMap", {enumerable: false, writable:true, value: {}});

        try {
            var items = data.items || [];
            for (var i = 0; i < items.length; ++i) {
                var item = itemModule.createItem(items[i]);
                if (!item)
                    throw new Error("创建Item失败");
                //设置主人
                item.setOwner(ownerRole);
                //加入列表
                this._addItem(item);
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
        var items = this._items;
        for (var i = 0; i < items.length; ++i)
        {
            items[i].release();
        }
        this._items = [];
        this._itemMap = {};
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj)
    {
        rootObj.items = this._items;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj)
    {
        rootObj.items = this._items;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
        //不会发给别人
        rootObj.items = null;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
        //不会发给别人
        rootObj.items = null;
    }

    /**
     *
     * @param {Item} item
     * @private
     */
    _addItem(item)
    {
        this._items.push(item);
        this._itemMap[item.itemId] = item;
    }

    /**
     *
     * @param {Item} item
     * @private
     */
    _removeItem(item)
    {
        this._items.removeValue(item);
        delete this._itemMap[item.itemId];
    }

    logDiamondAdd(addVal)
    {
        var role = this._role;
        var session = role.getSession();
        /** @type {AccountInfo} */
        var accountInfo = session ? session.getAccountInfo() : null;
        var account_id = role.getString(enProp.userId);
        var role_id = role.getNumber(enProp.heroId).toString();
        var role_name = role.getString(enProp.name);
        var role_level = role.getNumber(enProp.level);
        var mac_addr = accountInfo ? accountInfo.macAddr : "";
        var udid = "";
        var reason = "添加";
        var free_yuanbao = addVal;
        var left_yuanbao = role.getNumber(enProp.diamond);
        var left_free_yuanbao = role.getNumber(enProp.diamond);
        var gain_time = dateUtil.getTimestamp() * 1000;
        var details = {};
        adminServerAgent.logYuanbaoGain(account_id, role_id, role_name, role_level, mac_addr, udid, reason, free_yuanbao, left_yuanbao, left_free_yuanbao, gain_time, details);
    }

    logDiamondCost(costVal)
    {
        var role = this._role;
        var session = role.getSession();
        /** @type {AccountInfo} */
        var accountInfo = session ? session.getAccountInfo() : null;
        var account_id = role.getString(enProp.userId);
        var role_id = role.getNumber(enProp.heroId).toString();
        var role_name = role.getString(enProp.name);
        var role_level = role.getNumber(enProp.level);
        var mac_addr = accountInfo ? accountInfo.macAddr : "";
        var udid = "";
        var reason = "消耗";
        var yuanbao = costVal;
        var left_yuanbao = role.getNumber(enProp.diamond);
        var free_yuanbao = costVal;
        var left_free_yuanbao = role.getNumber(enProp.diamond);
        var use_time = dateUtil.getTimestamp() * 1000;
        var details = {};
        adminServerAgent.logYuanbaoUse(account_id, role_id, role_name, role_level, mac_addr, udid, reason, yuanbao, left_yuanbao, free_yuanbao, left_free_yuanbao, use_time, details);
    }

    /**
     * 有些字段不是简单的add，还要处理一些逻辑，所以要调用单独
     * @param {PropPart} propsPart
     * @param {number} propId
     * @param {number} delta
     * @private
     */
    _addPropVal(propsPart, propId, delta)
    {
        switch (propId)
        {
            case enProp.exp:
                propsPart.addExp(delta);
                break;
            case enProp.stamina:
                propsPart.addStamina(delta);
                break;
            case enProp.gold:
                propsPart.setNumber(enProp.gold, Math.clamp(propsPart.getNumber(enProp.gold) + delta, 0, appUtil.INT32_MAX_POSITIVE));
                break;
            case enProp.diamond:
                propsPart.addNumber(propId, delta);
                if (delta > 0)
                {
                    this.logDiamondAdd(delta);
                }
                else if(delta < 0)
                {
                    let opActivityPart = this._role.getOpActivityPart();
                    let cost = -delta;

                    opActivityPart.startBatch();
                    opActivityPart.addNumber(enOpActProp.diamondTotalCost,cost);
                    if(!dateUtil.isToday(opActivityPart.getNumber(enOpActProp.lastCostDiamond)))
                        opActivityPart.setNumber(enOpActProp.diamondDayCost,cost);
                    else
                        opActivityPart.addNumber(enOpActProp.diamondDayCost,cost);
                    opActivityPart.setNumber(enOpActProp.lastCostDiamond,dateUtil.getTimestamp());
                    opActivityPart.endBatch();

                    this.logDiamondCost(cost);
                }
                break;
            default:
                propsPart.addNumber(propId, delta);
                break;
        }
    }

    /**
     * @param {PropPart} propsPart
     * @param {number} propId
     * @private
     */
    _getPropVal(propsPart, propId)
    {
        switch (propId)
        {
            case enProp.stamina:
                return propsPart.getStamina();
            default:
                return propsPart.getNumber(propId);
        }
    }

    /**
     * 获取全部物品数量
     * 不包括虚拟物品
     * @returns {Number}
     */
    getAllItemsCount()
    {
        return this._items.length;
    }

    /**
     * 不包括虚拟物品
     * @param {number} index
     * @returns {Item}
     */
    getItemByIndex(index)
    {
        return this._items[index];
    }

    /**
     * 根据itemId获取物品数量
     * 包括虚拟物品
     * @param {number} itemId
     * @return {number}
     */
    getItemNum(itemId)
    {
        var propId = itemUtil.getItemPropId(itemId);
        if (propId)
        {
            return this._getPropVal(this._role.getPropsPart(), propId);
        }
        else
        {
            var item = this._itemMap[itemId];
            return item ? item.num : 0;
        }
    }

    /**
     * 获取金币数
     * @return {number}
     */
    getGoldNum()
    {
        return this._role.getNumber(enProp.gold);
    }

    /**
     * 如果是虚拟物品，获取的是null
     * @param {number} itemId
     * @returns {Item}
     */
    getItemByItemId(itemId)
    {
        var propId = itemUtil.getItemPropId(itemId);
        if (propId)
            return null;
        else
            return this._itemMap[itemId];
    }

    /**
     * 根据物品ID添加物品，如果物品不存在，则新建，如果物品已存在，则数量添加
     * 包括虚拟物品
     * @param {object.<number, number>} items - 要添加的物品的{物品ID:添加数}键值对
     * @returns {boolean}
     */
    addItems(items)
    {
        var existsItemIds = [];  //原来有的，更新
        var newItemDataArr = [];  //原来没有的，添加
        var addPropIds = [];    //要添加的属性

        for (var itemId in items)
        {
            //转为整数，不然有些地方可能逻辑有问题
            itemId = parseInt(itemId);
            var num = items[itemId];

            var propId = itemUtil.getItemPropId(itemId);
            if (propId) {
                addPropIds.push({propId:propId, num:num});
            }
            else {
                var item = this.getItemByItemId(itemId);
                if (item) {
                    item.num += num;
                    existsItemIds.push(itemId);
                }
                else {
                    newItemDataArr.push({itemId: itemId, num: num});
                }
            }
        }

        if (existsItemIds.length > 0 && !this.syncAndSaveItems(existsItemIds, true))
            return false;

        if (newItemDataArr.length > 0 && !this.addItemsWithDataArr(newItemDataArr, true))
            return false;

        var propIdCnt = addPropIds.length;
        var propsPart = this._role.getPropsPart();
        if (propIdCnt === 1)
        {
            let e = addPropIds[0];
            this._addPropVal(propsPart, e.propId, e.num);
        }
        else if (propIdCnt > 1)
        {
            propsPart.startBatch();
            for (var i = 0; i < addPropIds.length; ++i)
            {
                let e = addPropIds[i];
                this._addPropVal(propsPart, e.propId, e.num);
            }
            propsPart.endBatch();
        }

        return true;
    }

    /**
     * 根据物品ID添加物品，如果物品不存在，则新建，如果物品已存在，则数量添加
     * 包括虚拟物品
     * @param {number} itemId
     * @param {number?} [num=1]
     * @returns {boolean} 是否添加成功
     */
    addItem(itemId, num)
    {
        num = num || 1;
        if (num <= 0)
            return true;

        var propId = itemUtil.getItemPropId(itemId);
        if (propId) {
            this._addPropVal(this._role.getPropsPart(), propId, num);
            return true;
        }
        else {
            var item = this.getItemByItemId(itemId);
            if (item)
            {
                item.num += num;
                return this.syncAndSaveItem(itemId);
            }
            else
            {
                return this.addItemWithData({itemId:itemId, num:num});
            }
        }
    }

    /**
     * 添加金币数
     */
    addGold(num)
    {
        this.addItem(enItemId.GOLD, num);
    }

    /**
     * 添加钻石数
     */
    addDiamond(num)
    {
        this.addItem(enItemId.DIAMOND, num);
    }

    /**
     * 物品ID和需求量的键值对
     * 包括虚拟物品
     * @param {object.<number, number>} costs
     * @returns {boolean}
     */
    canCostItems(costs)
    {
        for (var itemId in costs)
        {
            //转为整数，不然有些地方可能逻辑有问题
            itemId = parseInt(itemId);
            var num = costs[itemId];
            if (this.getItemNum(itemId) < num)
                return false;
        }
        return true;
    }

    /**
     * 物品ID和需求量的键值对
     * 一般执行这个函数时会执行canCostItems，所以这里数量不够也不会返回false
     * 包括虚拟物品
     * @param {object.<number, number>} costs
     * @returns {boolean}
     */
    costItems(costs)
    {
        var removeItemIds = [];    //扣到0了，要删除的物品
        var updateItemIds = [];    //没扣到0，要更新的物品
        var decPropIds = [];    //要扣减的属性

        var propsPart = this._role.getPropsPart();

        for (var itemId in costs)
        {
            //转为整数，不然有些地方可能逻辑有问题
            itemId = parseInt(itemId);
            var num = costs[itemId];
            if (num <= 0)
                continue;

            var propId = itemUtil.getItemPropId(itemId);
            if (propId) {
                var propVal = this._getPropVal(propsPart, propId);
                if (propVal >= num)
                    decPropIds.push({propId:propId, num:num});
                else
                    logUtil.warn("costItems，属性不够扣，角色guid：" + this._role.getGUID() + "，propId：" + itemId + "，属性值：" + propVal + "，扣减值：" + num);
            }
            else {
                var item = this.getItemByItemId(itemId);
                if (item && item.num >= num) {
                    item.num -= num;
                    if (item.num <= 0)
                        removeItemIds.push(itemId);
                    else
                        updateItemIds.push(itemId);
                }
                else {
                    logUtil.warn("costItems，物品不够扣，角色guid：" + this._role.getGUID() + "，itemId：" + itemId + "，物品num：" + (item ? item.num : 0) + "，要扣num：" + num);
                }
            }
        }

        if (removeItemIds.length > 0 && !this.removeItemsByItemIds(removeItemIds, false, true))
            return false;

        if (updateItemIds.length > 0 && !this.syncAndSaveItems(updateItemIds, true))
            return false;

        var propIdCnt = decPropIds.length;
        if (propIdCnt === 1)
        {
            let e = decPropIds[0];
            this._addPropVal(propsPart, e.propId, -e.num);
        }
        else if (propIdCnt > 1)
        {
            propsPart.startBatch();
            for (var i = 0; i < decPropIds.length; ++i)
            {
                let e = decPropIds[i];
                this._addPropVal(propsPart, e.propId, -e.num);
            }
            propsPart.endBatch();
        }

        return true;
    }

    /**
     * 物品ID和需求量的键值对 (暂时不同步和保存，直到endBatch)
     * 一般执行这个函数时会执行canCostItems，所以这里数量不够也不会返回false
     * 包括虚拟物品
     * @param {object.<number, number>} costs
     * @param {object.<number, number>} removeItemIdMap
     * @param {object.<number, number>} updateItemIdMap
     * @returns {boolean}
     */
    batchCostItems(costs, removeItemIdMap, updateItemIdMap)
    {
        var decPropIds = [];    //要扣减的属性

        var propsPart = this._role.getPropsPart();

        for (var itemId in costs)
        {
            //转为整数，不然有些地方可能逻辑有问题
            itemId = parseInt(itemId);
            var num = costs[itemId];
            if (num <= 0)
                continue;

            var propId = itemUtil.getItemPropId(itemId);
            if (propId) {
                var propVal = this._getPropVal(propsPart, propId);
                if (propVal >= num)
                    decPropIds.push({propId:propId, num:num});
                else
                    logUtil.warn("costItems，属性不够扣，角色guid：" + this._role.getGUID() + "，propId：" + itemId + "，属性值：" + propVal + "，扣减值：" + num);
            }
            else {
                var item = this.getItemByItemId(itemId);
                if (item && item.num >= num) {
                    item.num -= num;
                    if (item.num <= 0) {
                        removeItemIdMap[itemId] = 1;
                        updateItemIdMap[itemId]=undefined;
                    }
                    else
                    {
                        updateItemIdMap[itemId]=1;
                    }
                }
                else {
                    logUtil.warn("costItems，物品不够扣，角色guid：" + this._role.getGUID() + "，itemId：" + itemId + "，物品num：" + (item ? item.num : 0) + "，要扣num：" + num);
                }
            }
        }

        var propIdCnt = decPropIds.length;
        if (propIdCnt === 1)
        {
            let e = decPropIds[0];
            this._addPropVal(propsPart, e.propId, -e.num);
        }
        else if (propIdCnt > 1)
        {
            for (var i = 0; i < decPropIds.length; ++i)
            {
                let e = decPropIds[i];
                this._addPropVal(propsPart, e.propId, -e.num);
            }
        }

        return true;
    }
    startBatchCostItems()
    {
        var propsPart = this._role.getPropsPart();
        propsPart.startBatch();
    }
    endBatchCostItems(removeItemIdMap, updateItemIdMap)
    {
        var removeItemIds = [];    //扣到0了，要删除的物品
        var updateItemIds = [];    //没扣到0，要更新的物品

        for(var itemId in removeItemIdMap)
        {
            itemId = parseInt(itemId);
            if(removeItemIdMap[itemId])
            {
                removeItemIds.push(itemId);
            }
        }
        for(itemId in updateItemIdMap)
        {
            itemId = parseInt(itemId);
            if(updateItemIdMap[itemId])
            {
                updateItemIds.push(itemId);
            }
        }

        if (removeItemIds.length > 0)
            this.removeItemsByItemIds(removeItemIds, false, true);

        if (updateItemIds.length > 0)
            this.syncAndSaveItems(updateItemIds, true);

        var propsPart = this._role.getPropsPart();
        propsPart.endBatch();
    }

    /**
     * 包括虚拟物品
     * @param itemId
     * @param num
     * @returns {boolean}
     */
    canCostItem(itemId, num)
    {
        return this.getItemNum(itemId) >= num;
    }

    /**
     * 判断是否可以扣除金币数
     * @return {boolean}
     */
    canCostGold(num)
    {
        return this._role.getNumber(enProp.gold) >= num;
    }

    /**
     * 判断是否可以扣除砖石数
     * @return {boolean}
     */
    canCostDiamond(num)
    {
        return this._role.getNumber(enProp.diamond) >= num;
    }

    /**
     * 包括虚拟物品
     * @param {number} itemId
     * @param {number?} [num=1]
     * @returns {boolean}
     */
    costItem(itemId, num)
    {
        num = num || 1;
        if (num <= 0)
            return true;

        var propsPart = this._role.getPropsPart();
        var propId = itemUtil.getItemPropId(itemId);
        if (propId) {
            var propVal = this._getPropVal(propsPart, propId);
            if (propVal >= num) {
                this._addPropVal(propsPart, propId, -num);
                return true;
            }
            else {
                logUtil.warn("costItem，属性不够扣，角色guid：" + this._role.getGUID() + "，propId：" + itemId + "，属性值：" + propVal + "，扣减值：" + num);
                return false;
            }
        }
        else {
            var item = this.getItemByItemId(itemId);
            if (item && item.num >= num) {
                item.num -= num;
                if (item.num <= 0)
                    this.removeItemByItemId(itemId);
                else
                    this.syncAndSaveItem(itemId);
                return true;
            }
            else {
                logUtil.warn("costItem，物品不够扣，角色guid：" + this._role.getGUID() + "，itemId：" + itemId + "，物品num：" + (item ? item.num : 0) + "，要扣num：" + num);
                return false;
            }
        }
    }

    /**
     * 扣除金币
     * @return {boolean}
     */
    costGold(num)
    {
        if (this.getGoldNum() >= num) {
            this.costItem(enItemId.GOLD, num);
            return true;
        }
        else {
            return false;
        }
    }

    /**
     * 根据奖励ID添加物品奖励(经验值特殊处理)，返回一个{物品ID:添加数}键值对
     * 包括虚拟物品
     * @param {number} rewardId - 要添加的物品的{物品ID:添加数}键值对
     * @returns {object.<number, number>}
     */
    addRewards(rewardId)
    {
        var rewardItems = rewardConfig.getRandomReward(rewardId);
        this.addItems(rewardItems);
        return rewardItems;
    }


    /**
     * 虚拟物品不能用这个
     * @param {number} itemId
     * @param {boolean?} noRelease - 有可能是要被转移到别的地方，这时就不要release
     * @return {boolean} 如果装备存在且删除成功就返回true
     */
    removeItemByItemId(itemId, noRelease)
    {
        var item = this.getItemByItemId(itemId);
        //不存在？不能删除
        if (!item)
            return false;

        //先删除
        this._removeItem(item);

        //设置主人为空
        item.setOwner(null);

        //如果要释放，那就释放
        if (!noRelease)
        {
            try {
                item.release();
            }
            catch (err) {
                logUtil.error("装备销毁出错", err);
            }
        }

        var ownerRole = this._role;
        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow({"props.heroId":heroId}, {$pull:{"items":{"itemId":itemId}}});

            //通知客户端
            var netMsg = new itemMessage.RemoveItemVo(itemId);
            ownerRole.sendEx(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_REMOVE_ITEM, netMsg);
        }

        return true;
    }

    /**
     * 批量删除物品
     * 如果某个物品不存在，那就跳过，删除存在的
     * 虚拟物品不能用这个
     * @param {number[]} itemIds
     * @param {boolean?} noRelease - 有可能是要被转移到别的地方，这时就不要release
     * @param {boolean?} noCheckDup - 为了效率，如果可以保证不重复，就不检测重复了
     * @return {boolean}
     */
    removeItemsByItemIds(itemIds, noRelease, noCheckDup)
    {
        if (!noCheckDup)
            var itemIdsTemp = [];
        for (var i = 0; i < itemIds.length; ++i)
        {
            var itemId = itemIds[i];
            var item = this.getItemByItemId(itemId);
            //不存在或重复？跳过
            if (!item || (!noCheckDup && itemIdsTemp.existsValue(itemId)))
                continue;

            //先删除
            this._removeItem(item);

            //设置主人为空
            item.setOwner(null);

            //如果要释放，那就释放
            if (!noRelease)
            {
                try {
                    item.release();
                }
                catch (err) {
                    logUtil.error("装备销毁出错", err);
                }
            }

            if (!noCheckDup)
                itemIdsTemp.push(itemId);
        }

        if (!noCheckDup)
            itemIds = itemIdsTemp;

        if (itemIds.length <= 0)
            return true;

        //批量删除存盘、同步客户端
        var ownerRole = this._role;
        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow({"props.heroId":heroId}, {$pull:{"items":{"itemId":{"$in":itemIds}}}});

            //通知客户端
            var netMsg = new itemMessage.RemoveItemsVo(itemIds);
            ownerRole.sendEx(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_REMOVE_ITEMS, netMsg);
        }

        return true;
    }

    /**
     * 添加物品，如果同类物品已存在，则不能添加
     * 虚拟物品不能用这个
     * @param {object} data
     * @return {boolean} 如果添加成功就返回true
     */
    addItemWithData(data)
    {
        //必须有最基本数据
        //因为下面要获取属性，所以要先判断
        if (!itemModule.isItemData(data))
            return false;

        var itemId = data.itemId;

        //已存在？不能添加
        if (this.getItemByItemId(itemId))
            return false;

        //虚拟物品？不能添加
        var propId = itemUtil.getItemPropId(itemId);
        if (propId)
            return false;

        var item = itemModule.createItem(data);
        if (!item)
            return false;

        //设置主人
        var ownerRole = this._role;
        item.setOwner(ownerRole);

        //添加到列表
        this._addItem(item);

        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow({"props.heroId":heroId}, {$push:{"items":item}});

            //通知客户端
            var netMsg = new itemMessage.AddOrUpdateItemVo(true, item);
            ownerRole.sendEx(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_ADD_OR_UPDATE_ITEM, netMsg);
        }

        return true;
    }

    /**
     * 批量添加物品
     * 如果有一个物品已存在，则全部不能添加
     * 虚拟物品不能用这个
     * @param {object[]} dataArr
     * @param {boolean?} noCheckDup - 为了效率，如果可以保证不重复，就不检测重复了
     * @return {boolean}
     */
    addItemsWithDataArr(dataArr, noCheckDup)
    {
        if (!noCheckDup)
            var itemIds = [];
        var items = [];
        for (let i = 0; i < dataArr.length; ++i)
        {
            let data = dataArr[i];
            //为空？跳过
            if (!data)
                continue;

            //必须有最基本数据
            //因为下面要获取属性，所以要先判断
            if (!itemModule.isItemData(data))
                return false;

            var itemId = data.itemId;

            //已存在？不能添加
            if (this.getItemByItemId(itemId))
                return false;

            //虚拟物品？不能添加
            var propId = itemUtil.getItemPropId(itemId);
            if (propId)
                return false;

            //重复？跳过
            if (!noCheckDup && itemIds.existsValue(itemId))
                continue;

            let item = itemModule.createItem(data);
            //创建失败？不能添加
            if (!item)
                return false;

            if (!noCheckDup)
                itemIds.push(itemId);
            items.push(item);
        }

        if (items.length <= 0)
            return true;

        var ownerRole = this._role;
        for (let i = 0; i < items.length; ++i) {
            let item = items[i];
            //设置主人
            item.setOwner(ownerRole);
            //添加到列表
            this._addItem(item);
        }

        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow({"props.heroId":heroId}, {$push:{"items":{"$each":items}}});

            //通知客户端
            var netMsg = new itemMessage.AddOrUpdateItemsVo(true, items);
            ownerRole.sendEx(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_ADD_OR_UPDATE_ITEMS, netMsg);
        }

        return true;
    }

    /**
     * 添加物品，如果同类物品已存在，则不能添加
     * 虚拟物品不能用这个
     * @param {Item} item
     * @return {boolean} 如果添加成功就返回true
     */
    addItemWithItem(item)
    {
        //不能有主人
        if (item.getOwner())
        {
            logUtil.warn("addItemWithItem，有主人的装备必须脱离主人才能给别人");
            return false;
        }

        var itemId = item.itemId;

        //已存在？不能添加
        if (this.getItemByItemId(itemId))
            return false;

        //虚拟物品？不能添加
        var propId = itemUtil.getItemPropId(itemId);
        if (propId)
            return false;

        //设置主人
        var ownerRole = this._role;
        item.setOwner(ownerRole);

        //添加到列表
        this._addItem(item);

        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow({"props.heroId":heroId}, {$push:{"items":item}});

            //通知客户端
            var netMsg = new itemMessage.AddOrUpdateItemVo(true, item);
            ownerRole.sendEx(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_ADD_OR_UPDATE_ITEM, netMsg);
        }

        return true;
    }

    /**
     * 批量添加物品
     * 如果有一个物品已存在，则全部不能添加
     * 虚拟物品不能用这个
     * @param {Item[]} items
     * @param {boolean?} noCheckDup - 为了效率，如果可以保证不重复，就不检测重复了
     * @return {boolean}
     */
    addItemsWithItems(items, noCheckDup)
    {
        if (!noCheckDup) {
            var itemIds = [];
            var itemsTemp = [];
        }
        for (let i = 0; i < items.length; ++i)
        {
            let item = items[i];
            //为空？跳过
            if (!item)
                continue;

            //有主人？停止添加
            if (item.getOwner()) {
                logUtil.warn("addItemWithItems，有主人的装备必须脱离主人才能给别人");
                return false;
            }

            var itemId = item.itemId;

            //已存在？停止添加
            if (this.getItemByItemId(itemId)) {
                logUtil.warn("addItemWithItems，不能添加已有的物品");
                return false;
            }

            //虚拟物品？不能添加
            var propId = itemUtil.getItemPropId(itemId);
            if (propId) {
                logUtil.warn("addItemWithItems，不能添加虚拟物品");
                return false;
            }

            //重复？跳过
            if (!noCheckDup && itemIds.existsValue(itemId))
                continue;

            if (!noCheckDup) {
                itemIds.push(itemId);
                itemsTemp.push(item);
            }
        }

        if (!noCheckDup)
            items = itemsTemp;

        if (items.length <= 0)
            return true;

        var ownerRole = this._role;
        for (let i = 0; i < items.length; ++i) {
            let item = items[i];
            //设置主人
            item.setOwner(ownerRole);
            //添加到列表
            this._addItem(item);
        }

        //主角才会自动存盘、同步客户端
        if (ownerRole.isHero()) {
            //存盘
            var userId = ownerRole.getUserId();
            var heroId = ownerRole.getHeroId();
            var db = dbUtil.getDB(userId);
            var col = heroId < 0 ? db.collection("robot") : db.collection("role");
            col.updateOneNoThrow({"props.heroId":heroId}, {$push:{"items":{"$each":items}}});

            //通知客户端
            var netMsg = new itemMessage.AddOrUpdateItemsVo(true, items);
            ownerRole.sendEx(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_ADD_OR_UPDATE_ITEMS, netMsg);
        }

        return true;
    }

    /**
     * 保存、同步已在数据库的物品
     * 虚拟物品不能用这个
     * @param {number} itemId
     * @returns {boolean}
     */
    syncAndSaveItem(itemId)
    {
        var item = this.getItemByItemId(itemId);
        //不存在？不能继续
        if (!item)
            return false;

        var ownerRole = this._role;
        //主角才会自动存盘、同步客户端
        if (!ownerRole.isHero())
            return true;

        //存盘
        var userId = ownerRole.getUserId();
        var heroId = ownerRole.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        //这里由于不是多层数组，所以可以直接用set
        col.updateOneNoThrow({"props.heroId":heroId, "items.itemId":itemId}, {$set:{"items.$":item}});

        //通知客户端
        var netMsg = new itemMessage.AddOrUpdateItemVo(false, item);
        ownerRole.sendEx(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_ADD_OR_UPDATE_ITEM, netMsg);

        return true;
    }

    /**
     * 批量保存、同步已在数据库的物品
     * 如果某个物品不存在，那就跳过
     * 虚拟物品不能用这个
     * @param {number[]} itemIds
     * @param {boolean?} noCheckDup - 为了效率，如果可以保证不重复，就不检测重复了
     * @returns {boolean}
     */
    syncAndSaveItems(itemIds, noCheckDup)
    {
        if (!noCheckDup)
            var itemIdsTemp = [];
        var items = [];
        for (var i = 0; i < itemIds.length; ++i)
        {
            var itemId = itemIds[i];
            var item = this.getItemByItemId(itemId);
            //不存在或重复？跳过
            if (!item || (!noCheckDup && itemIdsTemp.existsValue(itemId)))
                continue;

            if (!noCheckDup)
                itemIdsTemp.push(itemId);
            items.push(item);
        }

        if (!noCheckDup)
            itemIds = itemIdsTemp;

        if (itemIds.length <= 0)
            return true;

        var ownerRole = this._role;
        //主角才会自动存盘、同步客户端
        if (!ownerRole.isHero())
            return true;

        //存盘
        var userId = ownerRole.getUserId();
        var heroId = ownerRole.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = heroId < 0 ? db.collection("robot") : db.collection("role");
        //这里是更新多个数组元素，直接更新的办法是不太好实现，但可以全部符合条件的删除再添加
        col.updateOneNoThrow({"props.heroId":heroId}, {$pull:{"items":{"itemId":{"$in":itemIds}}}});
        col.updateOneNoThrow({"props.heroId":heroId}, {$push:{"items":{"$each":items}}});

        //通知客户端
        var netMsg = new itemMessage.AddOrUpdateItemsVo(false, items);
        ownerRole.sendEx(ModuleIds.MODULE_ITEM, CmdIdsItem.PUSH_ADD_OR_UPDATE_ITEMS, netMsg);

        return true;
    }
}

exports.ItemsPart = ItemsPart;