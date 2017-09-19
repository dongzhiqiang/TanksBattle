using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


public class UILotteryPreview : UIPanel
{

    #region Fields
    public StateGroup m_topTabs;
    public RectTransform m_itemsNode;
    public StateGroup m_itemsGrid;

    private int[] m_pools = null;
    #endregion

    #region Properties

    #endregion

    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        m_topTabs.AddSel(OnSelTopTab);
    }


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        var basicCfg = (LotteryBasicCfg)param;
        m_pools = basicCfg.previewPools;

        m_topTabs.Get(0).GetComponentInChildren<TextEx>(true).text = basicCfg.previewTabs[0];
        m_topTabs.Get(1).GetComponentInChildren<TextEx>(true).text = basicCfg.previewTabs[1];

        m_topTabs.SetSel(0);
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {
        m_itemsGrid.SetCount(0);
    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }
    #endregion

    #region Private Methods
    private void OnSelTopTab(StateHandle state, int idx)
    {
        m_itemsNode.gameObject.SetActive(false);

        var poolId = m_pools[idx];
        var list = LotteryPreview.GetByPoolId(poolId);
        if (list == null || list.Count <= 0)
            return;

        //取第一个的类型作为展示类型
        var type = list[0].objectType;
        switch (type)
        {
            case 1:
                {
                    m_itemsNode.gameObject.SetActive(true);
                    m_itemsGrid.SetCount(list.Count);
                    for (var i = 0; i < list.Count; ++i)
                    {
                        var dataItem = list[i];
                        if (dataItem.objectType != type)
                        {
                            Debuger.LogError("宝藏预览库，同一个库里的项的对象类型不一致，项ID：" + dataItem.showId);
                            continue;
                        }

                        var uiItem = m_itemsGrid.Get<UIItemIcon>(i);
                        var itemId = StringUtil.ToInt(dataItem.objectId);
                        uiItem.Init(itemId, dataItem.objectCnt, false);
                    }
                }
                break;
        }
    }
    #endregion


}
