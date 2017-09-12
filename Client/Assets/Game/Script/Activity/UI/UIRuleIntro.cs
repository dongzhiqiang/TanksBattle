using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using Candlelight.UI;
using UnityEngine;

public class UIRuleIntro : UIPanel
{
    public Text richTextCotent;
    public GameObject rulePanel;
    public GameObject rewardPanel;
    public StateGroup topBtnGroup;  
    public StateGroup rewardTypeGroup;  
    public StateGroup rankRewardGroup;
    public StateGroup gradeRewardGroup;
    public GameObject jiantou;
    bool isInitRank = false;


    #region Frame
    //初始化时调用
    public override void OnInitPanel()
    {
        topBtnGroup.AddSel(OnTopBtnClick);
        rewardTypeGroup.AddSel(OnRewardTypeBtnClick);
        rankRewardGroup.transform.parent.GetComponent<ScrollRect>().onValueChanged.AddListener(OnRankScrollChanged);
        gradeRewardGroup.transform.parent.GetComponent<ScrollRect>().onValueChanged.AddListener(OnGradeScrollChanged);
    }
  


    //显示,保证在初始化之后
    public override void OnOpenPanel(object param)
    {
        topBtnGroup.SetSel(0);
    }

    //关闭，保证在初始化之后
    public override void OnClosePanel()
    {

    }

    //更新，保证在初始化之后
    public override void OnUpdatePanel()
    {

    }

    public void SetContent(string str)
    {
        richTextCotent.text = str;
    }

    public void OnLinkClick(HyperText ht, HyperText.LinkInfo info)
    {
        Debuger.Log(info.Name);
    }
    #endregion



    #region Private Methods     
    void OnRankScrollChanged(Vector2 v)
    {
        jiantou.gameObject.SetActive(rankRewardGroup.Count > 4 && rankRewardGroup.transform.parent.GetComponent<ScrollRect>().verticalNormalizedPosition > 0.01f);
    }

    void OnGradeScrollChanged(Vector2 v)
    {
        jiantou.gameObject.SetActive(gradeRewardGroup.Count > 4 && gradeRewardGroup.transform.parent.GetComponent<ScrollRect>().verticalNormalizedPosition > 0.01f);
    }

    void OnTopBtnClick(StateHandle s, int idx)
    {
        switch (idx)
        {
            case 0:
                rulePanel.SetActive(true);
                rewardPanel.SetActive(false);
                rankRewardGroup.SetCount(10);
                isInitRank = false;
                break;
            case 1:
                rulePanel.SetActive(false);
                rewardPanel.SetActive(true);
                rewardTypeGroup.SetSel(0);
                break;
        }

    }

    void OnRewardTypeBtnClick(StateHandle s, int idx)
    {
        switch (idx)
        {
            case 0:
                InitRankReward();                
                break;
            case 1:
                InitUpgradeReward();
                break;
            case 2:
                InitGradeReward();
                break;
        }
    }
    void InitRankReward()
    {
        gradeRewardGroup.transform.parent.parent.gameObject.SetActive(false);
        rankRewardGroup.transform.parent.parent.gameObject.SetActive(true);

        if (!isInitRank)
        {
            List<ArenaRankCfg> arenaRankCfg = ArenaRankCfg.m_cfg;

            rankRewardGroup.SetCount(arenaRankCfg.Count);
            for (int i = 0; i < arenaRankCfg.Count; ++i)
            {
                UIArenaRewardItem item = rankRewardGroup.Get<UIArenaRewardItem>(i);
                item.Init(arenaRankCfg[i].rank, arenaRankCfg[i].rewardId);
            }
            isInitRank = true;
        }
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(rankRewardGroup.transform.parent.GetComponent<ScrollRect>(), 0); });

    }
    void InitUpgradeReward()
    {
        gradeRewardGroup.transform.parent.parent.gameObject.SetActive(true);
        rankRewardGroup.transform.parent.parent.gameObject.SetActive(false);
        Dictionary<int, ArenaGradeCfg> arenaGradeCfg = ArenaGradeCfg.m_cfgs;
        gradeRewardGroup.SetCount(arenaGradeCfg.Count - 1);
        for (int i = 0 ; i < arenaGradeCfg.Count-1; ++i)
        {
            UIArenaRewardItem item = gradeRewardGroup.Get<UIArenaRewardItem>(i);
            item.Init(arenaGradeCfg[arenaGradeCfg.Count - i - 1].grade+1, arenaGradeCfg[arenaGradeCfg.Count - i - 1].upgradeRewardId);
        }
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(gradeRewardGroup.transform.parent.GetComponent<ScrollRect>(), 0); });

    }

    void InitGradeReward()
    {
        gradeRewardGroup.transform.parent.parent.gameObject.SetActive(true);
        rankRewardGroup.transform.parent.parent.gameObject.SetActive(false);
        Dictionary<int, ArenaGradeCfg> arenaGradeCfg = ArenaGradeCfg.m_cfgs;
        gradeRewardGroup.SetCount(arenaGradeCfg.Count);
        for (int i = 0; i < arenaGradeCfg.Count; ++i)
        {
            UIArenaRewardItem item = gradeRewardGroup.Get<UIArenaRewardItem>(i);
            item.Init(arenaGradeCfg[arenaGradeCfg.Count - i - 1].grade + 1, arenaGradeCfg[arenaGradeCfg.Count - i - 1].dayRewardId);
        }
        TimeMgr.instance.AddTimer(0.1f, () => { UIScrollTips.ScrollPos(gradeRewardGroup.transform.parent.GetComponent<ScrollRect>(), 0); });

    }


    #endregion
}
