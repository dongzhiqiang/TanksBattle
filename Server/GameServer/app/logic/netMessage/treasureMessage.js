"use strict";

const CmdIdsTreasure = {

    CMD_UPGRADE_TREASURE: 1,           //升级神器
    CMD_CHANGE_BATTLE_TREASURE: 2,    //设置出战神器
    PUSH_ADD_OR_UPDATE_TREASURE: -1,  //添加更新神器
    PUSH_UPDATE_BATTLE_TREASURE: -2,  //添加更新装备神器
};

const ResultCodeTreasure = {
    TREASURE_MAX_LEVEL: 1,             //神器已经满级
    TREASURE_NOT_ENOUGE_ITEM: 2,      //所需神器碎片不足
};

/////////////////////////////////请求类////////////////////////////

//升级神器
class UpgradeTreasureRequestVo
{
    constructor() {
        this.treasureId = 0;
    }
    static fieldsDesc() {
        return{
            treasureId:{type: Number, notNull: true},
        }
    }
}

//出战神器
class ChangeBattleTreasureRequestVo
{
    constructor() {
        this.battleTreasure = [];
    }
    static fieldsDesc() {
        return{
            battleTreasure:{type: Array, itemType: Number, notNull: true},
        }
    }
}

/////////////////////////////////回复类////////////////////////////


/////////////////////////////////推送类////////////////////////////

class AddOrUpdateTreasureVo {
    /**
     * @param {boolean} isAdd - 否则是update
     * @param {Treasure} treasure
     */
    constructor(isAdd, treasure) {
        this.isAdd = isAdd;
        this.treasure = treasure;
    }
}

class UpdateBattleTreasureVo {
    /**
     * @param {array.<number>} battleTreasure
     */
    constructor(battleTreasure) {
        this.battleTreasure = battleTreasure;
    }
}

exports.CmdIdsTreasure = CmdIdsTreasure;
exports.ResultCodeTreasure = ResultCodeTreasure;
exports.AddOrUpdateTreasureVo = AddOrUpdateTreasureVo;
exports.UpdateBattleTreasureVo = UpdateBattleTreasureVo;
exports.UpgradeTreasureRequestVo = UpgradeTreasureRequestVo;
exports.ChangeBattleTreasureRequestVo = ChangeBattleTreasureRequestVo;