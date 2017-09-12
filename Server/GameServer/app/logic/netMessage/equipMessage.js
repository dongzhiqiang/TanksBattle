"use strict";

const CmdIdsEquip = {
    CMD_UPGRADE: 1,     // 升级装备
    CMD_ADVANCE: 2,     // 进阶装备
    CMD_ROUSE: 3,      // 觉醒装备
//    CMD_CHANGE_WEAPON: 4,     // 更换武器
    CMD_UPGRADE_ONCE: 5,     // 一键升级
    CMD_UPGRADE_ALL: 6,     // 全部升级
    PUSH_ADD_OR_UPDATE_EQUIP: -1,   //添加或更新装备
    PUSH_REMOVE_EQUIP: -2,   //删除装备
};

const ResultCodeEquip = {
    EQUIP_PLAYER_LEVEL_LIMIT: 1, //装备等级超过角色等级
    EQUIP_LEVEL_LIMIT: 2, // 装备等级超过配置等级
    EQUIP_NO_ENOUGE_ITEM: 3, // 所需材料不足
    EQUIP_NO_LEVEL: 4, // 装备等级未达到升品条件
    EQUIP_IS_NOT_POS_TYPE: 5, // 装备位错误
    EQUIP_MAX_ADV_LV: 6, // 装备已经进阶满级
    EQUIP_MAX_ROUSE: 7, // 装备已经觉醒满级
};

/////////////////////////////////请求类////////////////////////////

class UpgradeEquipRequestVo {
    constructor() {
        this.ownerGUID = "";           //所属角色(或宠物)guid
        this.equipPosIndex = 0;           //装备位置
    }

    static fieldsDesc() {
        return {
            ownerGUID: {type: String, notNull: true},
            equipPosIndex: {type: Number, notNull: true},
        };
    }
}

class UpgradeOnceEquipRequestVo {
    constructor() {
        this.ownerGUID = "";           //所属角色(或宠物)guid
        this.equipPosIndex = 0;           //装备位置
    }

    static fieldsDesc() {
        return {
            ownerGUID: {type: String, notNull: true},
            equipPosIndex: {type: Number, notNull: true},
        };
    }
}

class AdvanceEquipRequestVo {
    constructor() {
        this.ownerGUID = "";           //所属角色(或宠物)guid
        this.equipPosIndex = 0;           //装备位置
    }

    static fieldsDesc() {
        return {
            ownerGUID: {type: String, notNull: true},
            equipPosIndex: {type: Number, notNull: true},
        };
    }
}

class RouseEquipRequestVo {
    constructor() {
        this.ownerGUID = "";           //所属角色(或宠物)guid
        this.equipPosIndex = 0;           //装备位置
    }

    static fieldsDesc() {
        return {
            ownerGUID: {type: String, notNull: true},
            equipPosIndex: {type: Number, notNull: true},
        };
    }
}

class UpgradeAllEquipRequestVo {
    constructor() {
        this.ownerGUID = "";           //所属角色(或宠物)guid
    }

    static fieldsDesc() {
        return {
            ownerGUID: {type: String, notNull: true},
        };
    }
}

/////////////////////////////////推送类////////////////////////////

class AddOrUpdateEquipVo {
    /**
     * @param {string} guidOwner - 所有者的唯一ID，可能是主角、可能是宠物
     * @param {boolean} isAdd - 否则是update
     * @param {Equip} equip
     */
    constructor(guidOwner, isAdd, equip) {
        this.guidOwner = guidOwner;
        this.isAdd = isAdd;
        this.equip = equip;
    }
}

class RemoveEquipVo {
    /**
     * @param {string} guidOwner - 所有者的唯一ID，可能是主角、可能是宠物
     * @param {number} index
     */
    constructor(guidOwner, index) {
        this.guidOwner = guidOwner;
        this.index = index;
    }
}

/////////////////////////////////回复类////////////////////////////
class GrowEquipResultVo {
    /**
     * @param {string} guidOwner - 所有者的唯一ID，可能是主角、可能是宠物
     * @param {Equip} oldEquip
     * @param {Equip} equip
     */
    constructor(guidOwner, oldEquip, newEquip) {
        this.guidOwner = guidOwner;
        this.oldEquip = oldEquip;
        this.newEquip = newEquip;
    }
}

class UpgradeAllEquipResultVo {
    /**
     * @param {string} guidOwner - 所有者的唯一ID，可能是主角、可能是宠物
     * @param {Array} equipList - 修改过的装备列表
     */
    constructor(guidOwner, equipList) {
        this.guidOwner = guidOwner;
        this.equipList = equipList;
    }
}

/////////////////////////////////导出元素////////////////////////////
exports.CmdIdsEquip = CmdIdsEquip;
exports.ResultCodeEquip = ResultCodeEquip;

exports.AddOrUpdateEquipVo = AddOrUpdateEquipVo;
exports.RemoveEquipVo = RemoveEquipVo;
exports.UpgradeEquipRequestVo = UpgradeEquipRequestVo;
exports.AdvanceEquipRequestVo = AdvanceEquipRequestVo;
exports.RouseEquipRequestVo = RouseEquipRequestVo;
exports.UpgradeOnceEquipRequestVo = UpgradeOnceEquipRequestVo;
exports.UpgradeAllEquipRequestVo = UpgradeAllEquipRequestVo;
exports.GrowEquipResultVo = GrowEquipResultVo;
exports.UpgradeAllEquipResultVo = UpgradeAllEquipResultVo;