using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIVenusLevel : UIPanel
{
    public TextEx m_eventOpenTime;
    public StateHandle m_button;
    public StateHandle m_help;

    public override void OnInitPanel()
    {
        m_button.AddClick(OnButton);
        m_help.AddClick(() =>
        {
            UIMgr.instance.Open<UIRuleDesc>(ActivityCfg.Get(enSystem.venusLevel).ruleIntro);
        });
    }

    public override void OnOpenPanel(object param)
    {
    }

    public override void OnClosePanel()
    {
    }

    public override void OnUpdatePanel()
    {
   
    }


    void OnButton()
    {
        Close();
        NetMgr.instance.ActivityHandler.SendEnterVenusLevel();
    }
}
