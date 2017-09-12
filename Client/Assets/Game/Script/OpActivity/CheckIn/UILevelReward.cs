using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class UILevelReward : MonoBehaviour {
    public StateGroup itemsGroup;
    public ScrollRect itemsScroll;


    public void Init()
    {
        Dictionary<int, LevelRewardCfg> levelRewardCfg = LevelRewardCfg.m_cfgs;
        itemsGroup.SetCount(levelRewardCfg.Count);

        for (int i = 0; i < itemsGroup.Count; ++i)
        {
            UILevelRewardItem item = itemsGroup.Get<UILevelRewardItem>(i);
            item.Init(levelRewardCfg[i + 1]);
        }

        Role hero = RoleMgr.instance.Hero;
        OpActivityPart opActivityPart = hero.OpActivityPart;
        int scrollIndex = opActivityPart.CheckLevelReward();

        TimeMgr.instance.AddTimer(0.1f, () =>
        {            
            UIScrollTips.ScrollPos(itemsScroll, scrollIndex);
            if (scrollIndex > 1&&scrollIndex< levelRewardCfg.Count-2)
            {
                Vector3 pos = itemsGroup.GetComponent<RectTransform>().localPosition;
                itemsGroup.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, pos.y - 86, pos.z);
            }
        });

    }

    public void getLevelReward(int levelId)
    {
        LevelRewardCfg levelRewardCfg = LevelRewardCfg.m_cfgs[levelId];
     

        List<int> itemIds = levelRewardCfg.GetItemIdList();
        List<int> itemNums = levelRewardCfg.GetItemNumList();

        List<RewardItem> rewardItems = new List<RewardItem>();

        for (int i = 0; i < itemIds.Count; ++i)
        {
            RewardItem rewardItem = new RewardItem();
            rewardItem.itemId = itemIds[i];
            rewardItem.itemNum = itemNums[i];
            rewardItems.Add(rewardItem);
        }

        UIMgr.instance.Open<UIReward>(rewardItems);
    }
}
