using UnityEngine;
using System.Collections;

/// <summary>
/// 勇士试炼
/// </summary>
public class UIWarriorsTried : UIPanel
{
    public TextEx m_special;
    public TextEx m_tip;
    public TextEx m_remainTxt;
    public TextEx m_refresh;
    public StateHandle m_refreshBtn;
    public StateHandle m_exchangeBtn;
    public UITriedCell[] m_cells;
    public StateHandle m_help;

    const int MedalItemId = 30013;
    
    public override void OnInitPanel()
    {
        //刷新
        m_refreshBtn.AddClick(OnRefresh);
        //兑换
        m_exchangeBtn.AddClick(OnExchange);
        //帮助规则
        m_help.AddClick(() =>
        {
            UIMgr.instance.Open<UIRuleDesc>(ActivityCfg.Get(enSystem.warriorTried).ruleIntro);
        });
    }

    public override void OnOpenPanel(object param)
    {
        //检查一下是否到了重置时间，是才请求
        ActivityPart part = RoleMgr.instance.Hero.ActivityPart;
        if (part.warriorTried.uptime == 0 || !TimeMgr.instance.IsToday(part.warriorTried.uptime))
            NetMgr.instance.ActivityHandler.GetWarriorTriedData();
        else
            UpdatePanelData();
    }
    public override void OnClosePanel()
    {
    }

    public void UpdatePanelData()
    {
        m_special.text = RoleMgr.instance.Hero.ItemsPart.GetItemNum(MedalItemId).ToString();
        WarriorTriedInfo warriorTriedInfo = RoleMgr.instance.Hero.ActivityPart.warriorTried;
        WarTriedBaseConfig baseCfg = WarTriedBaseCfg.Get();
        m_tip.text = string.Format("VIP{0}开启扫荡功能", VipCfg.GetWarrSweepVipLv());
        m_remainTxt.text = string.Format("今天剩余试炼次数：{0}/{1}", warriorTriedInfo.remainTried, baseCfg.dailyNum);
        if (warriorTriedInfo.refresh < baseCfg.freeRefresh)
            m_refresh.text = string.Format("免费{0}次", baseCfg.freeRefresh - warriorTriedInfo.refresh);
        else
            m_refresh.text = "";

        //设置副本信息
        for(var i = 0;i<4;++i)
        {
            UITriedCell cell = m_cells[i];
            cell.OnSetData(warriorTriedInfo.trieds[i], i);
        }
    }

    #region PrivateMethod
    //刷新
    void DoRefresh()
    {
        WarTriedBaseConfig baseCfg = WarTriedBaseCfg.Get();
        int refresh = RoleMgr.instance.Hero.ActivityPart.warriorTried.refresh;
        if (refresh >= baseCfg.freeRefresh)
        {
            string msg = string.Format("是否花费{0}钻石刷新？", TriedRefreshCostCfg.Get(refresh + 1).cost);
            UIMessageBox.Open(msg, () =>
            {
                NetMgr.instance.ActivityHandler.RefreshWarrior();
            }, () => { UIMgr.instance.Close<UIMessageBox>(); }, "确定", "取消");
        }
        else
            NetMgr.instance.ActivityHandler.RefreshWarrior();
    }
    //提示是否刷新
    int CheckRefresh()
    {
        WarriorTriedInfo wt = RoleMgr.instance.Hero.ActivityPart.warriorTried;
        for (int i = 0, len = wt.trieds.Count; i < len; ++i)
        {
            if (wt.trieds[i].status == 1)
                return 1;
        }
        for (int i = 0, len = wt.trieds.Count; i < len; ++i)
        {
            if (wt.trieds[i].star > 3)
                return 2;
        }
        return 0;
    } 

    void OnRefresh()
    {
        int check = CheckRefresh();
        if (check == 1)   //有未领取的奖励
            UIMessageBox.Open(LanguageCfg.Get("warriors_refresh_tip1"), () => { DoRefresh(); }, () => { UIMgr.instance.Close<UIMessageBox>(); });
        else if (check == 2)
            UIMessageBox.Open(LanguageCfg.Get("warriors_refresh_tip2"), () => { DoRefresh(); }, () => { UIMgr.instance.Close<UIMessageBox>(); });
        else
            DoRefresh();
    }
    //void OnRule()
    //{
    //    UIMgr.instance.Open<UIRuleDesc>(WarTriedBaseCfg.Get().rule);
    //}
    void OnExchange()
    {
        UIMgr.instance.Open<UIShop>(enShopType.warriorMedalShop);
    }

    #endregion
}
