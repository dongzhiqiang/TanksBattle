"use strict";

const CmdIdsFlame = {
    CMD_UPGRADE_FLAME: 1, // 升级圣火
    PUSH_ADD_OR_UPDATE_FLAME: -1,  //添加更新圣火
};

const ResultCodeFlame = {
    FLAME_MAX_LEVEL: 1, //圣火已到达最大等级
    FLAME_LEVEL_LIMIT: 2, // 角色等级尚未达到圣火开启等级
    FLAME_NO_ENOUGE_ITEM: 3, // 所需材料不足
    FLAME_WRONG_ITEM: 4, // 使用的材料不存在
    FLAME_NOT_EXIST: 5, //圣火不存在
    FLAME_NO_ENOUGE_GOLD: 6, //金币不足
};

/////////////////////////////////请求类////////////////////////////

//升级圣火
class UpgradeFlameRequestVo
{
    constructor() {
        this.flameId = 0;
        /**
         * 包含itemId和num
         * @type {Object[]}
         */
        this.items = [];
    }
    static fieldsDesc() {
        return{
            flameId:{type: Number, notNull: true},
            items:{type: Array, itemType:Object, notNull: true}
        }
    }
}

/////////////////////////////////回复类////////////////////////////

//升级圣火
class UpgradeFlameResultVo
{
    constructor() {
        this.levelAdd = 0;
    }
}

/////////////////////////////////推送类////////////////////////////

class AddOrUpdateFlameVo {
    /**
     * @param {boolean} isAdd - 否则是update
     * @param {Flame} flame
     */
    constructor(isAdd, flame) {
        this.isAdd = isAdd;
        this.flame = flame;
    }
}

exports.CmdIdsFlame = CmdIdsFlame;
exports.ResultCodeFlame = ResultCodeFlame;
exports.UpgradeFlameRequestVo = UpgradeFlameRequestVo;
exports.UpgradeFlameResultVo = UpgradeFlameResultVo;
exports.AddOrUpdateFlameVo = AddOrUpdateFlameVo;