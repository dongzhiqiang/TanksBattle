using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UIShop : UIPanel
{
    #region SerializeFields
    public ScrollRect shopScroll;
    public StateGroup shopGroup;
    public StateGroup btnGroup;
    public TextEx shopDescription;
    public StateHandle refreshBtn;
    public TextEx countDown;
    public ImageEx btnBg;
    public ImageEx jiantou;
    public UIItemIcon moneyIcon;
    public TextEx moneyTotal;
    public TextEx diamondTotal;
    List<Shop> shops = new List<Shop>();
    TimeMgr.Timer countDownTimer;
    int shopIndex = 0;
    int shopId;
    int countDownNum;
    bool isRefresh = false;
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        btnGroup.AddSel(OnShopBtn);
        shopScroll.onValueChanged.AddListener(OnScrollChanged);
        refreshBtn.AddClick(OnRefreshShopBtn);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        Role hero = RoleMgr.instance.Hero;
        ShopsPart shopsPart = hero.ShopsPart;
        shops = shopsPart.shops;
        btnGroup.SetCount(shops.Count);
        Vector2 btnBgSize = btnBg.GetComponent<RectTransform>().sizeDelta;
        if (btnGroup.Count==1)
        {
            btnBgSize.y = 165;
        }
        else if(btnGroup.Count>1)
        {
            btnBgSize.y = 256 + 82 * (btnGroup.Count - 2);
        }
        btnBg.GetComponent<RectTransform>().sizeDelta = btnBgSize;

        


        for (int i=0;i<btnGroup.Count;++i)
        {
            UIShopBtnItem item = btnGroup.Get<UIShopBtnItem>(i);
            item.Init(shops[i].shopId);
        }
        if(param==null)
        {
            btnGroup.SetSel(0);
        }
        else
        {
            enShopType shopType = (enShopType)param;
            SetSel(shopType);
        }

        GetComponent<ShowUpController>().Prepare();
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        if (countDownTimer != null)
        {
            TimeMgr.instance.RemoveTimer(countDownTimer);
        }
        TimeMgr.instance.AddTimer(0.15f, () =>
        {
            for (int i = 0; i < shopGroup.Count; ++i)
            {
                UIShopItem item = shopGroup.Get<UIShopItem>(i);
                item.fx.SetActive(false);
            }
        });
        

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods

    void CountDown()
    {
        /*int second = countDownNum % 60;
        int minute = countDownNum / 60 % 60;
        int hour = countDownNum / 60 / 60;

        string secondStr = second < 10 ? "0" + second.ToString() : second.ToString();
        string minuteStr = minute < 10 ? "0" + minute.ToString() : minute.ToString();
        string hourStr = hour < 10 ? "0" + hour.ToString() : hour.ToString();

        countDown.text = string.Format("{0}:{1}:{2}", hourStr, minuteStr, secondStr);*/
        countDown.text = StringUtil.SceIntToHourMinSceStr(countDownNum);
        countDownNum--;
        if (countDownNum == -1)
            NetMgr.instance.ShopHandler.SendRefreshShop(shopId, false);
        
    }
    void OnShopBtn(StateHandle s, int idx)
    {
        UIShopBtnItem item = s.GetComponent<UIShopBtnItem>();
        if (item.shopId == shops[idx].shopId)
        {
            ExchangeShopCfg shopCfg = ExchangeShopCfg.m_cfgs[item.shopId];
            long currentTime = TimeMgr.instance.GetTimestamp();
            long lastRefreshTime = shops[idx].lastRefreshTime;
            countDownNum = (int)(lastRefreshTime + shopCfg.refreshTime - currentTime);
                       
            if ((int)(currentTime - lastRefreshTime) >= shopCfg.refreshTime)
            {
                NetMgr.instance.ShopHandler.SendRefreshShop(shops[idx].shopId, false);
            }



            if (countDownTimer != null)
            {
                TimeMgr.instance.RemoveTimer(countDownTimer);
            }
            countDownTimer = TimeMgr.instance.AddTimer(1, CountDown, 0, countDownNum + 1);

            shopIndex = idx;
            shopId = item.shopId;
            shopGroup.SetCount(shops[idx].wares.Count);
            for (int i = 0; i < shopGroup.Count; ++i)
            {
                UIShopItem shopItem = shopGroup.Get<UIShopItem>(i);
                shopItem.Init(item.shopId, shops[idx].wares[i]);
                //Debug.Log("index:"+shops[idx].wares[i].wareIndex + "      id:" + shops[idx].wares[i].wareId);
            }

            ItemCfg moneyCfg = ItemCfg.m_cfgs[shopCfg.moneyId];
            Role hero = RoleMgr.instance.Hero;
            ItemsPart itemsPart = hero.ItemsPart;
            moneyIcon.Init(shopCfg.moneyId, 0);
            moneyTotal.text = itemsPart.GetItemNum(moneyCfg.id).ToString();
            diamondTotal.text = hero.GetInt(enProp.diamond).ToString();
            shopDescription.text = shopCfg.description;
            TimeMgr.instance.AddTimer(0.1f, () => { if(shopScroll.IsActive())shopScroll.GetComponent<UIParticleMask>().UpdateLimitRect(); });
            if (!isRefresh)
                TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(shopScroll, 0); });
            else
                isRefresh = false;
        }
        else
        {
            Debug.LogError("商店参数有误");
            return;
        }
    }

    void OnRefreshShopBtn()
    {
        int refreshNum = shops[shopIndex].freshNum;
        ExchangeShopCfg shopCfg = ExchangeShopCfg.m_cfgs[shopId];
        int diamondCost;
        List<int> diamondCosts = shopCfg.GetDiamondCostList();
        if (refreshNum < diamondCosts.Count)
        {
            diamondCost= diamondCosts[refreshNum];
        }
        else
        {
            diamondCost = diamondCosts[diamondCosts.Count-1];
        }
        

        
        Role hero = RoleMgr.instance.Hero;
        int diamondTotal = hero.GetInt(enProp.diamond);
        UIMessageBox.Open(string.Format(LanguageCfg.Get("refresh_shop_desc"), diamondCost), ()=> 
        {
            if(diamondTotal>=diamondCost)
            {
                NetMgr.instance.ShopHandler.SendRefreshShop(shopId,true);
            }
            else
            {
                UIMessageBox.Open(LanguageCfg.Get("diamond_buy_desc"), () =>
                {   //打开充值界面
                }, () => { UIMgr.instance.Close<UIMessageBox>(); }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"), LanguageCfg.Get("diamond_low"));
            }
        }, () => { UIMgr.instance.Close<UIMessageBox>(); }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"), LanguageCfg.Get("refresh_shop"));
    }

    void OnScrollChanged(Vector2 v)
    {
        jiantou.gameObject.SetActive(shopGroup.Count > 8 && shopScroll.verticalNormalizedPosition > 0.01f);      
    }
    void SetSel(enShopType shopType)
    {
        for (int i = 0; i < btnGroup.Count; ++i)
        {
            UIShopBtnItem item = btnGroup.Get<UIShopBtnItem>(i);
            ExchangeShopCfg shopCfg = ExchangeShopCfg.m_cfgs[item.shopId];
            enShopType thisShopType = (enShopType)Enum.Parse(typeof(enShopType), shopCfg.type);
            if (shopType == thisShopType)
            {
                btnGroup.SetSel(i);
                return;
            }
        }
    }
    #endregion



    public void RefreshCurrentShop()
    {
        isRefresh = true;
        btnGroup.SetSel(shopIndex);
    }

    override public void OnOpenPanelEnd()
    {
        GetComponent<ShowUpController>().Start();
    }
}



