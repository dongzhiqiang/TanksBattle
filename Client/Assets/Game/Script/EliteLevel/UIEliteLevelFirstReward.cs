using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIEliteLevelFirstReward : MonoBehaviour
{
    public StateHandle m_close;
    public StateGroup m_rewards;
    public StateHandle m_getReward;
    private int m_levelId;

    public void Init()
    {
        m_close.AddClick(OnClose);
        m_getReward.AddClick(OnGetReward);
    }

    public void Open(int levelId)
    {
        m_levelId = levelId;
        gameObject.SetActive(true);

        EliteLevelCfg eliteLevelCfg = EliteLevelCfg.m_cfgs[levelId];
        RoomCfg roomCfg = RoomCfg.GetRoomCfgByID(eliteLevelCfg.roomId);
        Role hero = RoleMgr.instance.Hero;
        EliteLevel eliteLevel = hero.EliteLevelsPart.GetEliteLevel(levelId);

        if(eliteLevel != null && eliteLevel.passed && !eliteLevel.firstRewarded)
        {
            m_getReward.gameObject.SetActive(true);
        }
        else
        {
            m_getReward.gameObject.SetActive(false);
        }

        RewardCfg rewardCfg;
        if(RewardCfg.m_cfgs.TryGetValue(eliteLevelCfg.firstReward, out rewardCfg))
        {
            //m_rewards.SetCount(rewardCfg.rewardsDefinite)
            List<RewardItem> rewardItems = RewardCfg.GetRewardsDefinite(eliteLevelCfg.firstReward);
            m_rewards.SetCount(rewardItems.Count);
            for(int i=0; i<rewardItems.Count; i++)
            {
                m_rewards.Get<UIItemIcon>(i).Init(rewardItems[i].itemId, rewardItems[i].itemNum);
            }
        }
        else
        {
            Debuger.LogError("首杀奖励填写错误{0}", eliteLevelCfg.id);
            m_rewards.SetCount(0);
        }
    }


    void OnClose()
    {
        gameObject.SetActive(false);
    }

    void OnGetReward()
    {
        NetMgr.instance.EliteLevelHandler.SendGetFirstReward(m_levelId);
        gameObject.SetActive(false);
    }
}
