using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIReward : UIPanel
{

    #region SerializeFields
    public StateGroup m_itemGroup;
    public GameObject fx;
    public StateHandle closeBtn;
    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        fx.SetActive(true);
        closeBtn.enabled = false;

        List<RewardItem> rewardItems = (List<RewardItem>)param;

        m_itemGroup.SetCount(rewardItems.Count);

        for (int i = 0; i < m_itemGroup.Count; ++i)
        {
            UIRewardItem item = m_itemGroup.Get<UIRewardItem>(i);
            item.init(rewardItems[i].itemId, rewardItems[i].itemNum);
        }

        TimeMgr.instance.AddTimer(0.5f, () => { closeBtn.enabled=true; });
    }
    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        fx.SetActive(false);
        closeBtn.enabled = false;
        UIHeroUpgrade.CheckOpen();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods

    void ClosePanel()
    {
        this.Close();
    }

    #endregion

}



