using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_SHOP
{
    public const int CMD_REFRESH_SHOP = 1; //刷新商店
    public const int CMD_BUY_WARE = 2; //购买商品
   
}
public class RESULT_CODE_SHOP : RESULT_CODE
{
    public const int REFRESH_SHOP_FAILED = 1; //刷新商店失败
    public const int BUY_WARE_FAILED = 2;//购买商品失败 
}

public class RefreshShopReq
{
    public int shopId;
    public bool isDiamond;
}

public class RefreshShopRes
{
    public Shop shop;
}

public class BuyWareReq
{
    public int shopId;
    public int wareIndex;
}

public class BuyWareRes
{
    public int  shopId;
    public Ware ware;
}



