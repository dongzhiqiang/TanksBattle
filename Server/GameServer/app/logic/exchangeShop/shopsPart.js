"use strict";

var logUtil = require("../../libs/logUtil");
var dbUtil = require("../../libs/dbUtil");
var dateUtil = require("../../libs/dateUtil");
var Shop = require("./shop");
var Ware = require("./shop").Ware;
var exchangeShopConfig = require("../gameConfig/exchangeShopConfig");
var waresConfig = require("../gameConfig/waresConfig");
var appUtil = require("../../libs/appUtil");
var enItemId = require("../enumType/globalDefine").enItemId;

class ShopsPart{
    /**
     * @param {Role} role
     * @param {object} data
     */
    constructor(role, data) {
        this._shops = [];
        /**
         * 定义role
         * @type {Role}
         */
        Object.defineProperty(this, "_role", {enumerable: false, value: role});
        Object.defineProperty(this, "_shopsMap", {enumerable: false, writable:true, value: {}});

        //登录初始化数据
        try {

            if(this._role.isRobot())
                return;


            var shops = data.shops || [];
            let len = shops.length;
            let isRefresh = false;
            let cfgLengh = exchangeShopConfig.getExchangeShopConfigLength();
            if(len == 0 || len != cfgLengh)
            {
                shops = this.getRefreshShops();
                isRefresh = true;
            }
            for (let i = 0; i < shops.length; ++i)
            {
                let currentTime = dateUtil.getTimestamp();
                let shop = Shop.createShop(shops[i]);
                if (!shop)
                    throw new Error("创建商店失败");
                let shopCfg = exchangeShopConfig.getExchangeShopConfig(shop.shopId);

                if(currentTime - shop.lastRefreshTime < shopCfg.refreshTime)
                    this.addShop(shop);
                else
                    this.addShop(this.getRefreshShop(shop.shopId,false));
            }
            if(isRefresh)
            {
                this.syncAndSaveShops();
            }
        }
        catch (err) {
            //清除已创建的
            this.release();
            throw err;
        }
    }

    release() {
        this.shops = [];
        this._shopsMap = {};
    }
    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getDBData(rootObj) {
        rootObj.shops = this._shops;
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPrivateNetData(rootObj) {
        rootObj.shops = this._shops;
    }
    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getPublicNetData(rootObj)
    {
    }

    /**
     *
     * @param {FullRoleInfoVo} rootObj
     */
    getProtectNetData(rootObj)
    {
    }

    /**
     * 保存、同步已在数据库的商店
     * @returns {Boolean}
     */
    syncAndSaveShops()
    {
        if(!this._role.isHero() || this._role.isRobot())
            return false;
        if (!this._shops)
            return false;

        var ownerRole = this._role;
        //存盘
        var userId = ownerRole.getUserId();
        var heroId = ownerRole.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId}, {$set:{"shops":this._shops}});//注意数据这么写
        return true;
    }

    /**
     * 保存、同步已在数据库的商店
     * @param {Shop} shop
     * @returns {Boolean}
     */
    syncAndSaveShop(shop)
    {
        if(!this._role.isHero() || this._role.isRobot())
            return false;
        if (!this._shops)
            return false;

        var ownerRole = this._role;
        //存盘
        var userId = ownerRole.getUserId();
        var heroId = ownerRole.getHeroId();
        var db = dbUtil.getDB(userId);
        var col = db.collection("role");
        col.updateOneNoThrow({"props.heroId":heroId, "shops.shopId":shop.shopId}, {$set:{"shops.$":shop}});//注意数据这么写
        return true;
    }


    /**
     * 获取刷新后的所有商店
     * @returns {Shop[]}
     */
    getRefreshShops()
    {
        let shopCfgLength = exchangeShopConfig.getExchangeShopConfigLength();
        let shops = [];
        for(let i = 1;i < shopCfgLength+1;++i)
        {
            shops.push(this.getRefreshShop(i,false));
        }
        return shops;
    }

    /**
     * 是否够时间刷新商店
     * @param {number}shopId
     * @returns {Boolean}
     */
    canRefreshShop(shopId)
    {
        let shopCfg = exchangeShopConfig.getExchangeShopConfig(shopId);
        let refreshTime = shopCfg.refreshTime;
        let currentTime = dateUtil.getTimestamp();
        let shop = this._shopsMap[shopId];
        if(currentTime - shop.lastRefreshTime >= refreshTime)
        {
            return true;
        }
        else
        {
            return false;
        }


    }

    /**
     * 刷新商店
     * @param {number}shopId
     * @param {Boolean}isDiamond
     * @returns {Boolean | Shop}
     */
    refreshShop(shopId,isDiamond)
    {
        let shop;
        if(isDiamond)
        {
            shop = Shop.createShop(this.getRefreshShop(shopId,isDiamond));
            if(!shop)
                return false;
            this.setAndSaveShop(shop);
            return shop;
        }
        else
        {
            if(this.canRefreshShop(shopId))
            {
                shop = Shop.createShop(this.getRefreshShop(shopId,isDiamond));
                if(!shop)
                    return false;
                this.setAndSaveShop(shop);
                return shop;
            }
            else
            {
                return false;
            }

        }
    }

    /**
     * 保存商店
     * @param {Shop}shop
     */
    setAndSaveShop(shop)
    {
        this._shopsMap[shop.shopId] = shop;
        for(let i = 0;i < this._shops.length;++i)
        {
            if(this._shops[i].shopId == shop.shopId) {
                this._shops[i] = shop;
                break;
            }
        }
        this.syncAndSaveShop(shop);
    }

    /**
     * 获取刷新后的商店
     * @param {number}shopId
     * @param {Boolean}isDiamond
     * @returns {Shop | Boolean}
     */
    getRefreshShop(shopId,isDiamond)
    {
        let shopCfg = exchangeShopConfig.getExchangeShopConfig(shopId);
        let wares = [];
        let groupIds = shopCfg.groupId;
        let itemNums = shopCfg.itemNum;

        for(let i = 0;i < groupIds.length;++i)
        {
            let currentWares = this.getWares(groupIds[i],itemNums[i],wares.length);
            for(let j = 0;j < currentWares.length;++j)
            {
                wares.push(currentWares[j]);
            }
        }
        let freshNum;
        let newRefreshTime ;
        if(isDiamond)
        {
            newRefreshTime = this._shopsMap[shopId].lastRefreshTime;
            let diamondCosts = shopCfg.diamondCost;
            let diamondCost;
            if(this._shopsMap[shopId].freshNum < diamondCosts.length)
            {
                diamondCost = diamondCosts[this._shopsMap[shopId].freshNum];
            }
            else
            {
                diamondCost = diamondCosts[diamondCosts.length - 1];
            }
            let itemsPart = this._role.getItemsPart();
            if(itemsPart.canCostDiamond(diamondCost))
            {
                itemsPart.costItem(enItemId.DIAMOND,diamondCost);
                freshNum = this._shopsMap[shopId].freshNum + 1;
            }
            else
            {
                return false;
            }
        }
        else
        {
            freshNum = 0;
            let freshTime = shopCfg.refreshTime;
            let dayBreakPoingTime = dateUtil.getDayBreakPointTimestamp();
            let currentTime = dateUtil.getTimestamp();
            newRefreshTime = Math.floor(((currentTime - dayBreakPoingTime) / freshTime)) * freshTime + dayBreakPoingTime;
        }
        let  obj = {shopId:shopId,freshNum:freshNum,lastRefreshTime:newRefreshTime,wares:wares};

        return Shop.createShop(obj);

    }

    /**
     * 抽取商品
     * @param {number}groupId
     * @param {number}itemNum
     * @returns {Ware[]}
     */
    getWares(groupId,itemNum,wareIndex)
    {
        let allWareCfg = waresConfig.getAllWaresConfig();

        let groupWaresCfg = [];
        let wares = [];
        for (var key in allWareCfg)
        {
            if(allWareCfg[key].groupId == groupId)
            {
                groupWaresCfg.push(allWareCfg[key]);
            }
        }
        if(itemNum == -1)
        {
            for(let i = 0;i < groupWaresCfg.length;++i)
            {
                wares.push(Shop.createWare(groupWaresCfg[i].id,wareIndex));
                wareIndex++;
            }
        }
        else
        {
            let ramdomWaresCfg = [];
            for(let i = 0;i < groupWaresCfg.length;++i)
            {
                if(groupWaresCfg[i].sureAppear ==1)
                {
                    wares.push(Shop.createWare(groupWaresCfg[i].id,wareIndex));
                    wareIndex++;
                    itemNum--;
                }
                else
                {
                    ramdomWaresCfg.push(groupWaresCfg[i]);
                }
            }

            if(itemNum > 0) {
                let randomArray = [];
                for (let i = 0; i < ramdomWaresCfg.length; ++i)
                {
                    let num1, num2;
                    if (i == 0)
                    {
                        num1 = 0, num2 = ramdomWaresCfg[i].weight;
                        let array = [];
                        array.push(num1);
                        array.push(num2);
                        randomArray.push(array);
                    }
                    else
                    {
                        num1 = randomArray[i - 1][1];
                        num2 = randomArray[i - 1][1] + ramdomWaresCfg[i].weight;
                        let array = [];
                        array.push(num1);
                        array.push(num2);
                        randomArray.push(array);
                    }
                }

                while (itemNum > 0)
                {
                    let rate = appUtil.getRandom(1, 10000);
                    for (let j = 0; j < randomArray.length; ++j)
                    {
                        if (rate > randomArray[j][0] && rate <= randomArray[j][1])
                        {
                            let ware = Shop.createWare(ramdomWaresCfg[j].id,wareIndex);
                            if(ware.wareId)
                            {
                                wares.push(ware);
                                itemNum--;
                                wareIndex++;
                                break;
                            }
                        }
                    }
                }
            }
        }

        return wares;
    }

    /**
     *购买商品
     * @param {number} wareIndex
     * @returns {Boolean | Ware}
     */
    buyWare(shopId,wareIndex)
    {
        let shop = this._shopsMap[shopId];
        let ware;
        for(let i = 0;i < shop.wares.length;++i)
        {
            if(shop.wares[i].wareIndex == wareIndex)
            {
                ware = shop.wares[i];
            }
        }
        if(ware == undefined)
        {
            return false;
        }

        let wareCfg = waresConfig.getWaresConfig(ware.wareId);
        let shopCfg = exchangeShopConfig.getExchangeShopConfig(shopId);
        let moneyId = shopCfg.moneyId;
        let price = wareCfg.price;
        let itemId = wareCfg.itemId;
        let itemNum = wareCfg.itemNum;

        let itemPart = this._role.getItemsPart();
        if(!ware.isSold && itemPart.canCostItem(moneyId,price))
        {
            if(itemPart.costItem(moneyId,price))
            {
                let itemsPart = this._role.getItemsPart();
                itemsPart.addItem(itemId,itemNum);
                return this.setAndSaveWareSold(shopId,wareIndex);
            }
            else
            {
                return false;
            }
        }
        else
            return false;
    }

    /**
     *设置商品购买信息
     * @param {number} wareIndex
     * @returns {Ware | Boolean}
     */
    setAndSaveWareSold(shopId , wareIndex)
    {
        for(let i = 0;i<this._shops.length;++i)
        {
            if(this._shops[i].shopId == shopId)
            {
                for(let j = 0;j < this._shops[i].wares.length;++j)
                {
                    if(this._shops[i].wares[j].wareIndex == wareIndex)
                    {
                        this._shops[i].wares[j].isSold = true;
                        this._shopsMap[shopId].wares[j].isSold = true;
                        this.syncAndSaveShop(this._shops[i]);
                        return this._shops[i].wares[j];
                    }
                }
            }
        }
        return false;
    }


    /**
     *添加商店
     * @param {Shop} shop
     * @private
     */
    addShop(shop)
    {
        this._shops.push(shop);
        this._shopsMap[shop.shopId] = shop;
    }


}

exports.ShopsPart = ShopsPart;