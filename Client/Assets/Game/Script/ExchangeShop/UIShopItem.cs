using UnityEngine;
using System.Collections;

public class UIShopItem : MonoBehaviour {
    public TextEx itemName;
    public UIItemIcon itemIcon;
    public GameObject fx;
    public ImageEx moneyIcon;
    public TextEx price;
    public StateHandle exchangeBtn;
    public GameObject hasExchangedBtn;
    Ware ware = new Ware();
    int shopId;
    public void Init(int shopId, Ware ware)
    {
        this.ware = ware;
        this.shopId = shopId;
        ExchangeShopCfg shopCfg = ExchangeShopCfg.m_cfgs[shopId];
        WaresCfg wareCfg = WaresCfg.m_cfgs[ware.wareId];
        ItemCfg moneyCfg = ItemCfg.m_cfgs[shopCfg.moneyId];
        ItemCfg itemCfg = ItemCfg.m_cfgs[wareCfg.itemId];
        itemName.text = itemCfg.name;       
        itemName.color= QualityCfg.GetColor(itemCfg.quality);
        moneyIcon.Set(moneyCfg.icon);
        itemIcon.Init(wareCfg.itemId, wareCfg.itemNum);
        fx.SetActive(wareCfg.hasEffect == 1);
        exchangeBtn.gameObject.SetActive(!ware.isSold);
        hasExchangedBtn.SetActive(ware.isSold);
        price.text = wareCfg.price.ToString();
        exchangeBtn.AddClick(OnExchangeBtn);
    }

    void OnExchangeBtn()
    {
        ExchangeShopCfg shopCfg = ExchangeShopCfg.m_cfgs[shopId];
        WaresCfg wareCfg = WaresCfg.m_cfgs[ware.wareId];       
        Role hero = RoleMgr.instance.Hero;
        ItemsPart itemsPart = hero.ItemsPart;
        int price = wareCfg.price;
        int total = itemsPart.GetItemNum(shopCfg.moneyId);

        if (total >= price)
            NetMgr.instance.ShopHandler.SendBuyWare(shopId, ware.wareIndex);
        else
            UIMessage.Show("兑换币不足");

    }
}
