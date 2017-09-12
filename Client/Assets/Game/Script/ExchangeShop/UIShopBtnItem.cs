using UnityEngine;
using System.Collections;

public class UIShopBtnItem : MonoBehaviour {


    public TextEx shopName;
    public int shopId;


    public void Init(int shopId)
    {
        ExchangeShopCfg shopCfg = ExchangeShopCfg.m_cfgs[shopId];
        shopName.text = shopCfg.name;
        this.shopId = shopId;
    }
}
