"use strict";

const CmdIdsItem = {
    PUSH_ADD_OR_UPDATE_ITEM: -1,   //添加或更新物品
    PUSH_ADD_OR_UPDATE_ITEMS: -2,   //批量添加或更新物品
    PUSH_REMOVE_ITEM: -3,   //删除物品
    PUSH_REMOVE_ITEMS: -4,   //批量删除物品
};

const ResultCodeItem = {
};

class AddOrUpdateItemVo {
    /**
     * @param {boolean} isAdd - 否则是update
     * @param {Item} item
     */
    constructor(isAdd, item) {
        this.isAdd = isAdd;
        this.item = item;
    }
}

class AddOrUpdateItemsVo {
    /**
     * @param {boolean} isAdd - 否则是update
     * @param {Item[]} items
     */
    constructor(isAdd, items) {
        this.isAdd = isAdd;
        this.items = items;
    }
}

class RemoveItemVo {
    /**
     *
     * @param {number} itemId
     */
    constructor(itemId) {
        this.itemId = itemId;
    }
}

class RemoveItemsVo {
    /**
     *
     * @param {number[]} itemIds
     */
    constructor(itemIds) {
        this.itemIds = itemIds;
    }
}

exports.CmdIdsItem = CmdIdsItem;
exports.ResultCodeItem = ResultCodeItem;

exports.AddOrUpdateItemVo = AddOrUpdateItemVo;
exports.AddOrUpdateItemsVo = AddOrUpdateItemsVo;
exports.RemoveItemVo = RemoveItemVo;
exports.RemoveItemsVo = RemoveItemsVo;