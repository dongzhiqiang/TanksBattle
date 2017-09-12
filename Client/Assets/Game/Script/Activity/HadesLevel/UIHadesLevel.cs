using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIHadesLevel : UIPanel
{
    public GameObject m_grid;
    public TextEx m_lblCntTitle;
    public TextEx m_lblCntValue;
    public DateTime m_lastRefresh = DateTime.Now;
    public StateHandle m_help;

    private UIHadesLevelItem[] m_items;

    public override void OnInitPanel()
    {
        m_help.AddClick(() =>
        {
            UIMgr.instance.Open<UIRuleDesc>(ActivityCfg.Get(enSystem.hadesLevel).ruleIntro);
        });

        m_items = m_grid.GetComponentsInChildren<UIHadesLevelItem>();
        foreach (var item in m_items)
        {
            item.Init();
        }
    }

    public override void OnOpenPanel(object param)
    {
        RefreshUI();
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
        HadesLevelBasicCfg basicCfg = HadesLevelBasicCfg.Get();

        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;

        long hadesLvlTime = part.GetLong(enActProp.hadesLvlTime);
        int hadesLvlCnt = part.GetInt(enActProp.hadesLvlCnt);
        int hadesLvlMax = part.GetInt(enActProp.hadesLvlMax);

        long curTime = TimeMgr.instance.GetTimestamp();
        if (!TimeMgr.instance.IsToday(hadesLvlTime))
            hadesLvlCnt = 0;
        int cnt = (basicCfg.dayMaxCnt - hadesLvlCnt);
        if (cnt > 0)
        {
            m_lblCntTitle.text = "今日剩余次数：";
            m_lblCntValue.text = cnt + "/" + basicCfg.dayMaxCnt;
        }
        else
        {
            m_lblCntTitle.text = "今日挑战次数已用完";
            m_lblCntValue.text = "";
        }

        foreach (var item in m_items)
        {
            item.SetState(hadesLvlMax);
        }
    }
}
