"use strict";

const CmdIdsOpActivity=
{
    CMD_CHECK_IN: 1,    // 签到
    CMD_LEVEL_REWARD:2,//等级礼包
    CMD_VIP_GIFT: 3,   //vip礼包
    CMD_DRAW_LOTTERY: 4,//购买宝藏
    PUSH_SYNC_PROP: -1 //同步属性

};

const ResultCodeOpActivity=
{
    CHECK_IN_FAILED : 1,        //签到失败
    LEVEL_REWARD_FAILED : 2,    //获取等级礼包失败
    VIP_GIFT_FAILED : 3,        //vip等级不足或已领取
    LACK_NEED_ITEM : 4,         //缺少所需物品
};


/////////////////////////////////请求类////////////////////////////

class LvRewardReq {
    /**
     * @param {Number} levelId
     */
    constructor(levelId) {
        this.levelId = levelId;
    }

    static fieldsDesc() {
        return {
            levelId: {type: Number, notNull: true},
        };
    }
}

class VipGiftReq {
    /**
     * @param {Number} vipLv
     */
    constructor(vipLv) {
        this.vipLv = vipLv;
    }

    static fieldsDesc() {
        return {
            vipLv: {type: Number, notNull: true},
        };
    }

}

class DrawLotteryReq
{
    constructor() {
        this.type = 0;
        this.subType = 0;
    }

    static fieldsDesc() {
        return {
            type: {type: Number, notNull: true},
            subType: {type: Number, notNull: true}
        };
    }
}
/////////////////////////////////回复类////////////////////////////

class CheckInRes
{
    /**
     * @param {Number} checkInId
     */
    constructor(checkInId) {
        this.checkInId = checkInId;
    }
}

class LvRewardRes
{
    /**
     * @param {Number} levelId
     */
    constructor(levelId)
    {
        this.levelId = levelId;
    }
}

class VipGiftRes
{
    /**
     * @param {Number} vipLv
     */
    constructor(vipLv)
    {
        this.vipLv = vipLv;
    }
}

class DrawLotteryRes
{
    constructor() {
        this.type = 0;
        this.subType = 0;
        this.randIds = [];
        this.pieceRandIds = [];
    }
}

//////////////////////////////
class SyncOpActivityPropVo {
    /**
     *
     * @param {object.<string, *>} props
     */
    constructor(props) {
        this.props = props;
    }
}

exports.CmdIdsOpActivity=CmdIdsOpActivity;
exports.ResultCodeOpActivity=ResultCodeOpActivity;
exports.SyncOpActivityPropVo=SyncOpActivityPropVo;

exports.LvRewardReq=LvRewardReq;
exports.VipGiftReq=VipGiftReq;
exports.DrawLotteryReq = DrawLotteryReq;

exports.CheckInRes=CheckInRes;
exports.LvRewardRes=LvRewardRes;
exports.DrawLotteryRes = DrawLotteryRes;

exports.VipGiftRes=VipGiftRes;