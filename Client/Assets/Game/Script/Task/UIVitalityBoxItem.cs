using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIVitalityBoxItem : MonoBehaviour
{

    public StateHandle boxOpen;
    public StateHandle boxClose;
    public StateHandle boxCanGet;
    public GameObject fx;

    public TextEx vitality;
    //private int vitalityId;
    private VitalityCfg vitalityCfg;
    private long getBoxTime = 0;

    public void init(int vitalityId, float total)
    {
        boxOpen.AddClick(OnBoxOpen);
        boxClose.AddClick(OnBoxClose);
        boxCanGet.AddClick(OnBoxCanGet);
        //this.vitalityId = vitalityId;
        vitalityCfg = VitalityCfg.m_cfgs[vitalityId];
        vitality.text = vitalityCfg.vitality.ToString();
        Vector3 pos = GetComponent<RectTransform>().localPosition;
        float boxVitality = vitalityCfg.vitality;
        GetComponent<RectTransform>().localPosition = new Vector3(boxVitality/ total * 540, pos.y, pos.z);
        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;
        switch (vitalityCfg.boxNum)
        {
            case (1):
                {
                    getBoxTime = taskPart.GetLong(enTaskProp.vitalityBox1);
                    break;
                }
            case (2):
                {
                    getBoxTime = taskPart.GetLong(enTaskProp.vitalityBox2);
                    break;
                }
            case (3):
                {
                    getBoxTime = taskPart.GetLong(enTaskProp.vitalityBox3);
                    break;
                }
            case (4):
                {
                    getBoxTime = taskPart.GetLong(enTaskProp.vitalityBox4);
                    break;
                }
        }
        if (TimeMgr.instance.IsToday(getBoxTime))
        {
            SetBoxOpen(enBoxState.hasGetReward);                       
        }
        else
        {          
            int currentVitality = taskPart.CurVitality;
            if (currentVitality >= boxVitality)
            {
                SetBoxOpen(enBoxState.canGetReward);
            }
            else
            {
                SetBoxOpen(enBoxState.cantGetReward);
            }



            
        }


    }

    void SetBoxOpen(enBoxState boxState)
    {
        switch(boxState)
        {
            case (enBoxState.canGetReward):
                {
                    boxClose.gameObject.SetActive(false);
                    boxOpen.gameObject.SetActive(false);
                    fx.SetActive(true);
                    break;
                }
            case (enBoxState.cantGetReward):
                {
                    boxClose.gameObject.SetActive(true);
                    boxOpen.gameObject.SetActive(false);
                    fx.SetActive(false);
                    break;
                }
            case (enBoxState.hasGetReward):
                {
                    boxOpen.gameObject.SetActive(true);
                    boxClose.gameObject.SetActive(false);
                    fx.SetActive(false);                    
                    break;
                }
        }
    }

    void OnBoxOpen()
    {
        ShowItemsTip();
    }

    void ShowItemsTip()
    {
        List<int> itemIds = vitalityCfg.GetItemIdList();
        List<int> itemNums = vitalityCfg.GetItemNumList();
        List<RewardItem> itemList = new List<RewardItem>();
        for (int i = 0; i < itemIds.Count; i++)
        {
            if (i >= itemNums.Count)
                break;
            RewardItem item = new RewardItem();
            item.itemId = itemIds[i];
            item.itemNum = itemNums[i];
            itemList.Add(item);
        }
        UIRewardTip tips = UIMgr.instance.Open<UIRewardTip>(itemList);
        tips.SetTitle("奖励");
    }

    void OnBoxClose()
    {
        ShowItemsTip();
    }

    void OnBoxCanGet()
    {
        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;
        int currentVitality = taskPart.CurVitality;
        float boxVitality = vitalityCfg.vitality;
        if (currentVitality >= boxVitality)
        {
            NetMgr.instance.TaskHandler.SendGetVitalityReward(vitalityCfg.id);
        }

    }
}


public enum enBoxState
{
    canGetReward = 1,
    hasGetReward = 2,
    cantGetReward = 3,
}
