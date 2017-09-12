using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UIVip : MonoBehaviour {
    public ImageEx vipTitle;
    public ImageEx rechargeTitle;
    public UIArtFont vipLevel;
    public GameObject vipData;
    public TextEx leftRecharge;
    public TextEx nextVipLv;
    public Image progress;
    public TextEx progressTxt;
    public StateHandle rechargeBtn;   
    public StateHandle leftArrow;
    public StateHandle rightArrow;
    public ScrollRect vipScroll;
    public StateGroup vipGroup;
    int vipIndex = 0;
    bool  isOnArrow=false;
    
    

    void Start ()
    {
        rechargeBtn.AddClick(OnRechargeBtn);       
        leftArrow.AddClick(OnLeftArrow);
        rightArrow.AddClick(OnRightArrow);

    }
	
	
	public void Init()
    {
        Role hero = RoleMgr.instance.Hero;
        int currentVipLv = hero.GetInt(enProp.vipLv);
        RefreshVipLv();
        RefreshVipDes();
        SwithToVipLevel(currentVipLv);
    }

 

    void OnRechargeBtn()
    {

    }
       

    void OnLeftArrow()
    {      
        StartCoroutine(lastPage());
    }

    void OnRightArrow()
    {
        StartCoroutine(nextPage());
    }


    IEnumerator nextPage()
    {
        if (vipIndex < vipGroup.Count - 1&&!isOnArrow)
        {
            if(vipIndex == vipGroup.Count - 2)
            {
                rightArrow.gameObject.SetActive(false);
            }
            if (vipIndex == 0)
            {
                leftArrow.gameObject.SetActive(true);
            }
            isOnArrow = true;
            UIVipItem item = vipGroup.Get<UIVipItem>(vipIndex + 1);
            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(item.scroll, 0); });
            Vector3 pos = vipGroup.GetComponent<RectTransform>().localPosition;
            float goal = pos.x - 725;

           
            while (pos.x > goal+1f)
            {                
                float lerp = Mathf.Lerp(pos.x, goal, 0.15f);              
                pos.x = lerp;
                vipGroup.GetComponent<RectTransform>().localPosition = pos;            
                yield return 0;
            }
            SwithToVipLevel(++vipIndex);           
            isOnArrow = false;
            yield return 1;
        }
    }
    IEnumerator lastPage()
    {
        if (vipIndex > 0&&!isOnArrow)
        {
            if (vipIndex == 1)
            {
                leftArrow.gameObject.SetActive(false);
            }
            if (vipIndex == vipGroup.Count - 1)
            {
                rightArrow.gameObject.SetActive(true);
            }
            UIVipItem item = vipGroup.Get<UIVipItem>(vipIndex - 1);
            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(item.scroll, 0); });
            isOnArrow = true;
            Vector3 pos = vipGroup.GetComponent<RectTransform>().localPosition;
            float goal = pos.x + 725;
            while (pos.x < goal-1f)
            {
                float lerp = Mathf.Lerp(pos.x, goal, 0.15f);
                pos.x = lerp;
                vipGroup.GetComponent<RectTransform>().localPosition = pos;
                yield return 0;
            }
            SwithToVipLevel(--vipIndex);            
            isOnArrow = false;
            yield return 0;
        }
    }

    void SwithToVipLevel(int vipLv)
    {
        vipIndex = vipLv;
        vipScroll.enabled = true;
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(vipScroll, vipLv); vipScroll.enabled = false; });
        UpdateArrow();
    }

    void UpdateArrow()
    {
        if (vipIndex == 0)
        {
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(true);
        }
        else if (vipIndex == vipGroup.Count - 1)
        {
            leftArrow.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(false);
        }
        else
        {
            leftArrow.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(true);
        }
    }

    public void RefreshVipDes()
    {
        Role hero = RoleMgr.instance.Hero;
        int currentVipLv = hero.GetInt(enProp.vipLv);
        vipGroup.SetCount(VipCfg.m_cfg.Count);
        for (int i = 0; i < vipGroup.Count; ++i)
        {
            VipCfg vipCfg = VipCfg.Get(i);
            UIVipItem item = vipGroup.Get<UIVipItem>(i);
            item.Init(vipCfg);
        }
     
    }

    public void RefreshVipLv()
    {
        Role hero = RoleMgr.instance.Hero;
        int currentVipLv = hero.GetInt(enProp.vipLv);
        if (currentVipLv < VipCfg.m_cfg.Count - 1)
        {
            vipData.SetActive(true);
            VipCfg nextVipLvCfg = VipCfg.Get(currentVipLv + 1);
            OpActivityPart opActivityPart = hero.OpActivityPart;
            int totalRecharge = opActivityPart.GetInt(enOpActProp.totalRecharge);
            progress.fillAmount = (float)totalRecharge / (float)nextVipLvCfg.totalRecharge;
            progressTxt.text = totalRecharge + "/" + nextVipLvCfg.totalRecharge;
            leftRecharge.text = (nextVipLvCfg.totalRecharge - totalRecharge).ToString();
            nextVipLv.text = "V" + (currentVipLv + 1).ToString();
        }
        else
        {
            vipData.SetActive(false);
        }
        vipLevel.SetNum(currentVipLv.ToString());
    }
    public void getVipGift(int vipLv)
    {        

        VipGiftCfg vipGiftCfg = VipGiftCfg.Get(vipLv);

        List<int> itemIds = vipGiftCfg.GetItemIdList();
        List<int> itemNums = vipGiftCfg.GetItemNumList();

        List<RewardItem> rewardItems = new List<RewardItem>();

        for (int i = 0; i < itemIds.Count; ++i)
        {
            RewardItem rewardItem = new RewardItem();
            rewardItem.itemId = itemIds[i];
            rewardItem.itemNum = itemNums[i];
            rewardItems.Add(rewardItem);
        }

        UIMgr.instance.Open<UIReward>(rewardItems);
    }
}
