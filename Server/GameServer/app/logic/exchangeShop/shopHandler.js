"use strict";
var handlerMgr = require("../session/handlerMgr");
var ModuleIds = require("../netMessage/netMsgConst").ModuleIds;
var CmdIdsShop = require("../netMessage/shopMessage").CmdIdsShop;
var ResultCodeShop = require("../netMessage/shopMessage").ResultCodeShop;
var RefreshShopReq = require("../netMessage/shopMessage").RefreshShopReq;
var RefreshShopRes = require("../netMessage/shopMessage").RefreshShopRes;
var BuyWareReq = require("../netMessage/shopMessage").BuyWareReq;
var BuyWareRes = require("../netMessage/shopMessage").BuyWareRes;
var dateUtil = require("../../libs/dateUtil");
var Shop = require("./shop");

/**
 * 刷新商店
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 *  @param {RefreshShopReq} reqObj
 * @return {RefreshShopRes|Number}
 */
function refreshShop(session, role, msgObj,reqObj) {

    let shopsPart = role.getShopsPart();


    let shop = Shop.createShop(shopsPart.refreshShop(reqObj.shopId, reqObj.isDiamond))
    if (!shop.shopId) {
        return ResultCodeShop.REFRESH_SHOP_FAILED;
    }
    else {
        return new RefreshShopRes(shop);
    }

}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SHOP, CmdIdsShop.CMD_REFRESH_SHOP, refreshShop, RefreshShopReq);

/**
 * 购买商品
 * @param {ClientSession} session
 * @param {Role} role
 * @param {Message} msgObj
 *  @param {BuyWareReq} reqObj
 * @return {BuyWareRes|Number}
 */
function buyWare(session, role, msgObj,reqObj) {

    let shopsPart = role.getShopsPart();

    let ware = Shop.createSoldWare(shopsPart.buyWare(reqObj.shopId, reqObj.wareIndex))
    if (!ware.wareId) {
        return ResultCodeShop.BUY_WARE_FAILED;
    }
    else {
        return new BuyWareRes(reqObj.shopId, ware);
    }


}
handlerMgr.registerHandlerEx(ModuleIds.MODULE_SHOP, CmdIdsShop.CMD_BUY_WARE, buyWare, BuyWareReq);


