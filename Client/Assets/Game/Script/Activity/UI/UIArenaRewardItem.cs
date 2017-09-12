using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIArenaRewardItem : MonoBehaviour {
    public UIArtFont normalIcon;
    public StateGroup rewardsGroup;
    public string prefixBigNum;
    public string prefixSmallNum;
    public bool isRank;
	public void Init(int index,int rewardId)
    {
        if (!isRank)
        {
            normalIcon.gameObject.SetActive(true);
            normalIcon.SetNum(index.ToString());
        }
        else
        {
            normalIcon.m_prefix = index <= 3 ? prefixBigNum : prefixSmallNum;
            normalIcon.SetNum(index.ToString());
        }
        List<RewardItem> rewardList = RewardCfg.GetRewardsDefinite(rewardId);
        rewardsGroup.SetCount(rewardList.Count);
        for(int i=0;i<rewardsGroup.Count;++i)
        {
            UITaskRewardItem item = rewardsGroup.Get<UITaskRewardItem>(i);
            ItemCfg itemCfg = ItemCfg.m_cfgs[rewardList[i].itemId];
            item.init(itemCfg.iconSmall, rewardList[i].itemNum);
        }

    }
}
