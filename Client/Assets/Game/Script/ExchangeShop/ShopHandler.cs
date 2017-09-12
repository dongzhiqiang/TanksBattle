using UnityEngine;
using System.Collections;

[NetModule(MODULE.MODULE_SHOP)]
public class ShopHandler
{

    public ShopHandler()
    {
        UIMainCity.AddClick(enSystem.shop, () =>
        {
            UIMgr.instance.Open<UIShop>();
        });        
    }

  
    //发送,刷新商店（分砖石刷新和定时刷新）
    public void SendRefreshShop(int shopId,bool isDiamond)
    {
        RefreshShopReq request = new RefreshShopReq();
        request.shopId = shopId;
        request.isDiamond = isDiamond;
        NetMgr.instance.Send(MODULE.MODULE_SHOP, MODULE_SHOP.CMD_REFRESH_SHOP, request);
    }

    //接收,刷新商店
    [NetHandler(MODULE_SHOP.CMD_REFRESH_SHOP)]
    public void OnRefreshShop(RefreshShopRes res)
    {
        Role hero = RoleMgr.instance.Hero;
        ShopsPart shopsPart = hero.ShopsPart;
        shopsPart.UpdateShop(res.shop);
        UIShop uiShop = UIMgr.instance.Get<UIShop>();
        if (uiShop.IsOpen)
            uiShop.RefreshCurrentShop();       
    }

    //发送，购买商品
    public void SendBuyWare(int shopId,int wareIndex)
    {
        BuyWareReq request = new BuyWareReq();
        request.shopId = shopId;
        request.wareIndex = wareIndex;
        NetMgr.instance.Send(MODULE.MODULE_SHOP, MODULE_SHOP.CMD_BUY_WARE, request);
    }

    //接收,购买商品
    [NetHandler(MODULE_SHOP.CMD_BUY_WARE)]
    public void OnBuyWare(BuyWareRes res)
    {
        Role hero = RoleMgr.instance.Hero;
        ShopsPart shopsPart = hero.ShopsPart;
        shopsPart.UpdateWare(res.shopId, res.ware);
        UIMgr.instance.Get<UIShop>().RefreshCurrentShop();
        UIMessage.Show("兑换成功");
    }
    
}
