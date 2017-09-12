using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UITreasureRob : UIPanel
{

    public StateHandle m_help;
    public StateGroup m_challegers;
    public StateHandle m_report;
    public StateHandle m_flesh;
    public TextEx m_gold;
    public TextEx m_times;
    private Color m_goldColor;
    private Color m_red = QualityCfg.ToColor("CE3535");

    public override void OnInitPanel()
    {

        m_help.AddClick(() =>
            {
                UIMgr.instance.Open<UIRuleDesc>(ActivityCfg.Get(enSystem.treasureRob).ruleIntro);
            });
        m_report.AddClick(() =>
        {
            UIMgr.instance.Open<UITreasureReport>("");
        });
        m_flesh.AddClick(OnFlesh);
        m_goldColor = m_gold.color;
    }

    public override void OnOpenPanel(object param)
    {
        Reflesh();

        NetMgr.instance.ActivityHandler.SendReqTreasureRob(false);
    }

    public void Reflesh()
    {
        List<TreasureRobChallengerVo> challengersInfos = ActivityMgr.instance.GetTreasureChallengers();
        m_challegers.SetCount(challengersInfos.Count);
        for (int i = 0; i < challengersInfos.Count; i++)
        {
            m_challegers.Get<UITreasureRobIcon>(i).Init(challengersInfos[i]);
        }

        Role hero = RoleMgr.instance.Hero;
        m_gold.text = TreasureRobBasicCfg.Get().fleshGold.ToString();
        if(RoleMgr.instance.Hero.ItemsPart.GetGold()<TreasureRobBasicCfg.Get().fleshGold)
        {
            m_gold.color = m_red;
        }
        else
        {
            m_gold.color = m_goldColor;
        }
        int maxCount = TreasureRobBasicCfg.Get().dayMaxCnt;
        int count = hero.ActivityPart.GetInt(enActProp.treasureCnt);
        if(!TimeMgr.instance.IsToday(hero.ActivityPart.GetInt(enActProp.treasureTime)))
        {
            count = 0;
        }
        m_times.text = string.Format("{0}/{1}", maxCount - count, maxCount);
    }

    public override void OnClosePanel()
    {
    }

    public override void OnUpdatePanel()
    {
   
    }

    void OnFlesh()
    {
        if (RoleMgr.instance.Hero.ItemsPart.GetGold()<TreasureRobBasicCfg.Get().fleshGold)
        {
            UIMessage.Show("金币不足");
            return;
        }
        NetMgr.instance.ActivityHandler.SendReqTreasureRob(true);
    }

    public static bool CheckOpen()
    {
        if (LevelMgr.instance.PrevRoomId == TreasureRobBasicCfg.Get().roomId)
        {
            UIMgr.instance.Open<UITreasureRob>();



            return true;
        }
        return false;
    }

}
