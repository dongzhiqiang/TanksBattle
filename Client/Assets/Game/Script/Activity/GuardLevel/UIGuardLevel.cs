using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIGuardLevel : UIPanel
{
    public GameObject m_grid;
    public TextEx m_lblCntTitle;
    public TextEx m_lblCntValue;
    public DateTime m_lastRefresh = DateTime.Now;
    public StateHandle m_help;

    private UIGuardLevelItem[] m_items;

    public override void OnInitPanel()
    {
        m_help.AddClick(() =>
        {
            UIMgr.instance.Open<UIRuleDesc>(ActivityCfg.Get(enSystem.guardLevel).ruleIntro);
        });

        m_items = m_grid.GetComponentsInChildren<UIGuardLevelItem>();
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
        GuardLevelBasicCfg basicCfg = GuardLevelBasicCfg.Get();

        Role hero = RoleMgr.instance.Hero;
        ActivityPart part = hero.ActivityPart;

        long guardLvlTime = part.GetLong(enActProp.guardLvlTime);
        int guardLvlCnt = part.GetInt(enActProp.guardLvlCnt);
        int guardLvlMax = part.GetInt(enActProp.guardLvlMax);

        long curTime = TimeMgr.instance.GetTimestamp();
        long timePass = curTime >= guardLvlTime ? curTime - guardLvlTime : guardLvlTime - curTime;
        if (timePass < basicCfg.coolDown)
        {
            long coolDownLeft = basicCfg.coolDown - timePass;
            m_lblCntTitle.text = "冷却时间：";
            m_lblCntValue.text = StringUtil.FormatTimeSpan(coolDownLeft);
        }
        else
        {
            if (!TimeMgr.instance.IsToday(guardLvlTime))
                guardLvlCnt = 0;
            m_lblCntTitle.text = "今日剩余次数：";
            m_lblCntValue.text = (basicCfg.dayMaxCnt - guardLvlCnt) + "/" + basicCfg.dayMaxCnt;
        }

        foreach (var item in m_items)
        {
            item.SetState(guardLvlMax);
        }
    }
}
