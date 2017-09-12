using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class UITask : UIPanel
{
    #region SerializeFields
    public StateGroup btnsGroup;  
    public UIDailyTask uiDailyTask;
    public UIGrowthTask uiGrowthTask;

        
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        btnsGroup.AddSel(OnTaskBtn);
    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        btnsGroup.SetCount(2);
        for(int i=0;i<btnsGroup.Count;++i)
        {
            UITaskBtnItem item = btnsGroup.Get<UITaskBtnItem>(i);
            item.Init(i);
        }

        btnsGroup.SetSel(0);       
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        Role hero = RoleMgr.instance.Hero;
        TaskPart taskPart = hero.TaskPart;
        taskPart.CheckTip();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {    
        
    }
    #endregion

    #region Private Methods     
    
    void OnTaskBtn(StateHandle s, int idx)
    {            
            UITaskBtnItem item = s.GetComponent<UITaskBtnItem>();

            if (item.index == 0)
            {
                uiDailyTask.gameObject.SetActive(true);
                uiGrowthTask.gameObject.SetActive(false);
                uiDailyTask.Init();
            }
            else if (item.index == 1)
            {
                uiDailyTask.gameObject.SetActive(false);
                uiGrowthTask.gameObject.SetActive(true);
                uiGrowthTask.Init();
            }        
    }

  
    
    #endregion
    public void GetDailyTaskReward(int taskId)
    {
        TaskRewardCfg taskRewardCfg = TaskRewardCfg.m_cfgs[taskId];
        List<int> itemIds = taskRewardCfg.GetItemIdList();
        List<int> itemNums = taskRewardCfg.GetItemNumList();

        List<RewardItem> rewardItems = new List<RewardItem>();

        for(int i=0;i<itemIds.Count;++i)
        {
            RewardItem rewardItem = new RewardItem();
            rewardItem.itemId = itemIds[i];
            rewardItem.itemNum = itemNums[i];
            rewardItems.Add(rewardItem);
        }

        UIMgr.instance.Open<UIReward>(rewardItems);
    }

    public void GetVitalityReward(int vitalityId)
    {
        VitalityCfg vitalityCfg = VitalityCfg.m_cfgs[vitalityId];
       
        List<int> itemIds = vitalityCfg.GetItemIdList();
        List<int> itemNums = vitalityCfg.GetItemNumList();

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

    public void GetGrowthTaskReward(int taskId)
    {
        GrowthTaskCfg taskRewardCfg = GrowthTaskCfg.m_cfgs[taskId];
        List<int> itemIds = taskRewardCfg.GetItemIdList();
        List<int> itemNums = taskRewardCfg.GetItemNumList();

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

    public void CheckTip()
    {
        btnsGroup.Get<UITaskBtnItem>(0).tip.gameObject.SetActive(uiDailyTask.CheckTip());
        btnsGroup.Get<UITaskBtnItem>(1).tip.gameObject.SetActive(uiGrowthTask.CheckTip());
      
    }


}



