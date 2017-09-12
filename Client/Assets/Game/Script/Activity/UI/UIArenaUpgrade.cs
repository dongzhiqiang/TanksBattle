using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIArenaUpgrade: UIPanel
{

    #region SerializeFields
    public StateGroup m_itemGroup;
    public UIArtFont m_grade;
    [HideInInspector]
    public int score;
    [HideInInspector]
    public List<KeyValuePair<int, int>> items = new List<KeyValuePair<int, int>>();
    [HideInInspector]
    public int upgradeRewardId;

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {

    }

    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        List<RewardItem> itemsList = RewardCfg.GetRewardsDefinite(upgradeRewardId);
        m_itemGroup.SetCount(itemsList.Count);
        for (var i = 0; i < itemsList.Count; ++i)
        {
            var uiItem = m_itemGroup.Get<UIItemIcon>(i);
            var dataItem = itemsList[i];
            uiItem.Init(dataItem.itemId, dataItem.itemNum);
        }
        int grade = ArenaGradeCfg.GetGrade(score);
        m_grade.SetNum((grade + 1).ToString());

    }
    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        items.Clear();
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods

    

    #endregion

}



