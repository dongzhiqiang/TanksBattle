using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UITreasureReport : UIPanel
{


    public StateGroup m_battleLogs;

    public override void OnInitPanel()
    {


    }

    public override void OnOpenPanel(object param)
    {
        Reflesh();
    }

    public void Reflesh()
    {
        List<TreasureRobBattleLogVo> battleLogInfos = ActivityMgr.instance.GetTreasureBattleLogs();
        m_battleLogs.SetCount(battleLogInfos.Count);
        for (int i = 0; i < battleLogInfos.Count; i++)
        {
            m_battleLogs.Get<UITreasureRobBattleLog>(i).Init(battleLogInfos[battleLogInfos.Count - i - 1], battleLogInfos.Count - i - 1);
        }

    }

    public override void OnClosePanel()
    {
    }

    public override void OnUpdatePanel()
    {
   
    }



}
