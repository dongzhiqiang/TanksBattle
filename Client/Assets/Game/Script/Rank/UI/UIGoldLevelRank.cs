using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIGoldLevelRank : UIPanel
{
    private const int PAGE_SIZE = 10;
    private int m_firstOneHeroId = 0;

    public UIScrollWrap m_table;
    public ImageEx m_firstOneHead;
    public TextEx m_firstOneName;
    public TextEx m_firstOneLevel;
    public TextEx m_firstOneCorps;
    public StateHandle m_firstOneView;

    public override void OnInitPanel()
    {
        m_table.OnUIItemsReachStart += OnScrollReachStart;
        m_table.OnUIItemsReachEnd += OnScrollReachEnd;
        m_firstOneView.AddClick(()=> {
            if (m_firstOneHeroId != 0)
                NetMgr.instance.RoleHandler.RequestHeroInfo(m_firstOneHeroId);
        });
    }

    public override void OnOpenPanel(object param)
    {
        NetMgr.instance.RankHandler.SendRequestRankData(RankMgr.RANK_TYPE_GOLD_LEVEL, 0, PAGE_SIZE, true);
    }

    public override void OnClosePanel()
    {        
    }

    public override void OnUpdatePanel()
    {
    }

    private void OnScrollReachStart(UIScrollWrap ui)
    {
        NetMgr.instance.RankHandler.SendRequestRankData(RankMgr.RANK_TYPE_GOLD_LEVEL, 0, PAGE_SIZE, true);
    }

    private void OnScrollReachEnd(UIScrollWrap ui)
    {
        NetMgr.instance.RankHandler.SendRequestRankData(RankMgr.RANK_TYPE_GOLD_LEVEL, m_table.GetDataCount(), PAGE_SIZE, true);
    }

    public void OnUpdateData()
    {
        var items = RankMgr.instance.GetRankList(RankMgr.RANK_TYPE_GOLD_LEVEL);
        m_table.SetDataList(items, false);

        if (items.Count > 0)
        {
            GoldLevelRankItem rankItem = (GoldLevelRankItem)((RankItemWrapper)items[0]).data;
            m_firstOneHeroId = rankItem.key;
            m_firstOneName.text = rankItem.name;
            m_firstOneLevel.text = rankItem.level.ToString();
            m_firstOneCorps.text = rankItem.corpsName;
        }
        else
        {
            m_firstOneHeroId = 0;
            m_firstOneName.text = "暂缺";
            m_firstOneLevel.text = "暂缺";
            m_firstOneCorps.text = "暂缺";
        }
    }
}
