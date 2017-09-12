using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIGoldLevel : UIPanel
{
    //public StateHandle m_btnRank;
    public GameObject m_grid;
    public TextEx m_lblCntTitle;
    public TextEx m_lblCntValue;
    public StateHandle m_stateCnt;
    public ScrollRect m_scrollView;
    public StateHandle m_help;
    //public TextEx m_lblMyRankVal;

    private DateTime m_lastRefresh = DateTime.Now;
    private UIGoldLevelItem[] m_items;

    public override void OnInitPanel()
    {
        //m_btnRank.AddClick(() => {
        //    UIMgr.instance.Open<UIGoldLevelRank>(); 
        //});
        m_help.AddClick(() =>
        {
            UIMgr.instance.Open<UIRuleDesc>(ActivityCfg.Get(enSystem.goldLevel).ruleIntro);
        });

        m_items = m_grid.GetComponentsInChildren<UIGoldLevelItem>();
        foreach (var item in m_items)
        {
            item.Init();
        }
    }

    public override void OnOpenPanel(object param)
    {
        //NetMgr.instance.RankHandler.SendReqRankValData(RankMgr.RANK_TYPE_GOLD_LEVEL);

        RefreshUI();

        if (m_items.Length > 0)
        {
            Role hero = RoleMgr.instance.Hero;
            ActivityPart part = hero.ActivityPart;
            int goldLvlMax = part.GetInt(enActProp.goldLvlMax);
            int selIndex = 0;
            for (var i = 0; i < m_items.Length; ++i)
            {
                var item = m_items[i];
                if (item.m_mode == goldLvlMax + 1)
                {
                    selIndex = i;
                    break;
                }
                else if (item.m_mode == goldLvlMax)
                {
                    selIndex = i;
                    //这里不break，有可能goldLvlMax + 1存在
                }
            }

            TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(m_scrollView, selIndex); });            
        }
    }

    public override void OnClosePanel()
    {
    }

    public override void OnUpdatePanel()
    {
        DateTime curTime = DateTime.Now;
        if ((curTime - m_lastRefresh).TotalSeconds >= 1)
        {
            m_lastRefresh = curTime;
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        GoldLevelBasicCfg basicCfg = GoldLevelBasicCfg.Get();

        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;

        long goldLvlTime = part.GetLong(enActProp.goldLvlTime);
        int goldLvlCnt = part.GetInt(enActProp.goldLvlCnt);
        int goldLvlMax = part.GetInt(enActProp.goldLvlMax);

        if (!TimeMgr.instance.IsToday(goldLvlTime))
            goldLvlCnt = 0;

        if (basicCfg.dayMaxCnt <= goldLvlCnt)
        {
            m_stateCnt.SetState(1);
        }
        else
        {
            m_stateCnt.SetState(0);
            long curTime = TimeMgr.instance.GetTimestamp();
            long timePass = curTime >= goldLvlTime ? curTime - goldLvlTime : goldLvlTime - curTime;
            if (timePass < basicCfg.coolDown)
            {
                m_lblCntTitle.text = "冷却时间：";
                m_lblCntValue.text = StringUtil.FormatTimeSpan(basicCfg.coolDown - timePass);
                
            }
            else
            {
                m_lblCntTitle.text = "今日剩余次数：";
                m_lblCntValue.text = (basicCfg.dayMaxCnt - goldLvlCnt) + "/" + basicCfg.dayMaxCnt;
            }
        }
        

        foreach (var item in m_items)
        {
            item.SetState(goldLvlMax);
        }

        //int rankVal = RankMgr.instance.GetRankDataRankVal(RankMgr.RANK_TYPE_GOLD_LEVEL);
        //m_lblMyRankVal.text = rankVal >= 0 ? (rankVal + 1).ToString() : "--";
    }
}
