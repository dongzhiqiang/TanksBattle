"use strict";

const CmdIdsShop=
{
    CMD_REFRESH_SHOP : 1,
    CMD_BUY_WARE : 2,
};

const ResultCodeShop=
{
    REFRESH_SHOP_FAILED : 1,
    BUY_WARE_FAILED : 2,
}

/////////////////////////////////请求类////////////////////////////

class RefreshShopReq {
    /**
     * @param {Number} shopId
     * @param {Boolean} isDiamond
     */
    constructor(shopId, isDiamond) {
        this.shopId = shopId;
        this.isDiamond = isDiamond;
    }

    static fieldsDesc() {
        return {
            shopId: {type: Number, notNull: true},
            isDiamond: {type: Boolean, notNull: true}
        };
    }
}

class BuyWareReq {
    /**
     * @param {Number} shopId
     * @param {Number} wareIndex
     */
    constructor(shopId, wareIndex) {
        this.shopId = shopId;
        this.wareIndex = wareIndex;
    }

    static fieldsDesc() {
        return {
            shopId: {type: Number, notNull: true},
            wareIndex: {type: Number, notNull: true}
        };
    }
}

/////////////////////////////////回复类////////////////////////////

class RefreshShopRes
{
    /**
     * @param {Shop} shop
     */
    constructor(shop)
    {
        this.shop = shop;
    }
}

class BuyWareRes
{
    /**
     * @param {Number} shopId
     * @param {Ware} ware
     */
    constructor(shopId,ware)
    {
        this.shopId = shopId;
        this.ware = ware;
    }
}


exports.CmdIdsShop=CmdIdsShop;
exports.ResultCodeShop=ResultCodeShop;
exports.RefreshShopReq=RefreshShopReq;
exports.RefreshShopRes=RefreshShopRes;
exports.BuyWareReq=BuyWareReq;
exports.BuyWareRes=BuyWareRes;
