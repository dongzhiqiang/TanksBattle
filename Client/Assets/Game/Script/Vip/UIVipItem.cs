using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIVipItem : MonoBehaviour {
    public TextEx descriptionTitle;
    public StateGroup descriptionGroup;
    public TextEx vipGiftDescription;
    public StateGroup vipGiftGroup;
    public TextEx oldPrice;
    public TextEx currentPrice;
    public StateHandle buyBtn;
    public GameObject hasBuyBtn;
    public GameObject arrow;
    public ScrollRect scroll;
    VipCfg vipCfg;
    VipGiftCfg vipGiftCfg;
    enRewardState rewardState;

    public void Init(VipCfg vipCfg)
    {
        buyBtn.AddClick(OnBuyBtn);
        scroll.onValueChanged.AddListener(OnScrollChanged);
        this.vipCfg = vipCfg;
        vipGiftCfg = VipGiftCfg.Get(vipCfg.level);

        List<string> descriptions = vipCfg.GetDescriptionList();
        descriptionTitle.text = descriptions[0];
        descriptionGroup.SetCount(descriptions.Count - 1);
        for(int i=1;i< descriptions.Count;++i)
        {
            UIVipDesItem desItem = descriptionGroup.Get<UIVipDesItem>(i - 1);
            desItem.Init(descriptions[i]);
        }

        vipGiftDescription.text = vipGiftCfg.description;
        oldPrice.text = vipGiftCfg.vipGiftValue.ToString();
        currentPrice.text = vipGiftCfg.vipGiftDiamondCost.ToString();
        List<int> giftIds = vipGiftCfg.GetItemIdList();
        List<int> giftNums = vipGiftCfg.GetItemNumList();
        vipGiftGroup.SetCount(giftIds.Count);

        for(int i=0;i< vipGiftGroup.Count;++i)
        {
            UIItemIcon icon = vipGiftGroup.Get<UIItemIcon>(i);
            icon.Init(giftIds[i], giftNums[i]);
        }

        Role hero = RoleMgr.instance.Hero;

        OpActivityPart opActivityPart = hero.OpActivityPart;

        switch (opActivityPart.CanGetVipGift(vipGiftCfg.level))
        {
            case enRewardState.canGetReward:
                rewardState = enRewardState.canGetReward;
                buyBtn.gameObject.SetActive(true);
                buyBtn.GetComponent<ImageEx>().SetGrey(false);
                hasBuyBtn.SetActive(false);
                break;
            case enRewardState.cantGetReward:
                rewardState = enRewardState.hasGetReward;
                buyBtn.gameObject.SetActive(true);
                buyBtn.GetComponent<ImageEx>().SetGrey(true);
                hasBuyBtn.SetActive(false);
                break;
            case enRewardState.hasGetReward:
                buyBtn.gameObject.SetActive(false);              
                hasBuyBtn.SetActive(true);
                break;
        }



     
        

    }
    void OnScrollChanged(Vector2 v)
    {
        arrow.gameObject.SetActive(descriptionGroup.Count > 5 && scroll.verticalNormalizedPosition > 0.01f);
    }
    void OnBuyBtn()
    {
        if(rewardState==enRewardState.canGetReward)
        {
            Role hero = RoleMgr.instance.Hero;
            int currentDiamond = hero.GetInt(enProp.diamond);
            if(currentDiamond>=vipGiftCfg.vipGiftDiamondCost)
                NetMgr.instance.OpActivityHandler.SendGetVipGift(vipGiftCfg.level);
            else
                UIMessageBox.Open(LanguageCfg.Get("diamond_buy_desc"), () =>
                {   //打开充值界面
                }, () => { UIMgr.instance.Close<UIMessageBox>(); }, LanguageCfg.Get("confirm"), LanguageCfg.Get("cancle"), LanguageCfg.Get("diamond_low"));
        }
        else
        {
            UIMessage.Show(LanguageCfg.Get("vip_level_low"));
        }
    }
}


