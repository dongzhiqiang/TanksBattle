"use strict";

class Shop
{
    constructor(data)
    {
        this.shopId = data.shopId;
        this.freshNum = data.freshNum;
        this.lastRefreshTime = data.lastRefreshTime;
        this.wares = data.wares;
    }
}


/**
 *
 * @param data
 * @returns {Ware|null}
 */
class Ware
{
    constructor(data)
    {
        this.wareId = data.wareId;
        this.wareIndex = data.wareIndex;
        this.isSold = data.isSold;
    }
}


/**
 *
 * @param data
 * @returns {Shop|null}
 */

function createShop(data)
{
    return new Shop(data);
}

/**
 *
 * @param {number}wareId
 * @returns {Ware|null}
 */
function createWare(wareId,wareIndex)
{
    let obj = {wareId : wareId, wareIndex: wareIndex, isSold : false};
    return new Ware(obj);
}

function createSoldWare(data)
{
    return new Ware(data);
}


exports.Shop = Shop;
exports.Ware = Ware;
exports.createShop = createShop;
exports.createWare = createWare;
exports.createSoldWare = createSoldWare;